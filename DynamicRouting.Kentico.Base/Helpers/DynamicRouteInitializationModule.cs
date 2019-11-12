using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Scheduler;
using CMS.SiteProvider;
using CMS.WorkflowEngine;
using DynamicRouting.Kentico;
using DynamicRouting.Kentico.Classes;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Transactions;

namespace DynamicRouting.Kentico
{
    /// <summary>
    /// This is the base OnInit, since the this is run on both the Mother and MVC, but we cannot initialize both modules (or it throw a duplicate error), this is called from a MVC and Mother specific initialization module
    /// </summary>
    public class DynamicRouteInitializationModule_Base
    {
        public DynamicRouteInitializationModule_Base()
        {

        }

        public void Init()
        {
            // Ensure that the Foreign Keys and Views exist
            try
            {
                ConnectionHelper.ExecuteNonQuery("DynamicRouting.UrlSlug.InitializeSQLEntities");
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("DynamicRouting", "ErrorRunningSQLEntities", ex, additionalMessage: "Could not run DynamicRouting.UrlSlug.InitializeSQLEntities Query, this sets up Views and Foreign Keys vital to operation.  Please ensure these queries exist.");
            }

            // Create Scheduled Tasks if it doesn't exist
            if(TaskInfoProvider.GetTasks().WhereEquals("TaskName", "CheckUrlSlugQueue").Count == 0)
            {
                TaskInfo CheckUrlSlugQueueTask = new TaskInfo()
                {
                    TaskName = "CheckUrlSlugQueue",
                    TaskDisplayName = "Dynamic Routing - Check Url Slug Generation Queue",
                    TaskAssemblyName = "DynamicRouting.Kentico",
                    TaskClass = "DynamicRouting.Kentico.DynamicRouteScheduledTasks",
                    TaskInterval = "hour;11/3/2019 4:54:30 PM;1;00:00:00;23:59:00;Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday",
                    TaskDeleteAfterLastRun = false,
                    TaskRunInSeparateThread = true,
                    TaskAllowExternalService = true,
                    TaskUseExternalService = false,
                    TaskRunIndividuallyForEachSite = false,
                    TaskEnabled = true,
                    TaskData = ""
                };
                TaskInfoProvider.SetTaskInfo(CheckUrlSlugQueueTask);
            }

            // Detect Site Culture changes
            CultureSiteInfo.TYPEINFO.Events.Insert.After += CultureSite_InsertDelete_After;
            CultureSiteInfo.TYPEINFO.Events.Delete.After += CultureSite_InsertDelete_After;

            // Catch Site Default Culture and Builder setting updates
            SettingsKeyInfo.TYPEINFO.Events.Insert.After += SettingsKey_InsertUpdate_After;
            SettingsKeyInfo.TYPEINFO.Events.Update.After += SettingsKey_InsertUpdate_After;

            // Catch ClassURLPattern changes
            DataClassInfo.TYPEINFO.Events.Update.Before += DataClass_Update_Before;
            DataClassInfo.TYPEINFO.Events.Update.After += DataClass_Update_After;

            // Document Changes
            DocumentEvents.ChangeOrder.After += Document_ChangeOrder_After;
            DocumentEvents.Copy.After += Document_Copy_After;
            DocumentEvents.Delete.After += Document_Delete_After;
            DocumentEvents.Insert.After += Document_Insert_After;
            DocumentEvents.InsertLink.After += Document_InsertLink_After;
            DocumentEvents.InsertNewCulture.After += Document_InsertNewCulture_After;
            DocumentEvents.Move.Before += Document_Move_Before;
            DocumentEvents.Move.After += Document_Move_After;
            DocumentEvents.Sort.After += Document_Sort_After;
            DocumentEvents.Update.After += Document_Update_After;
            WorkflowEvents.Publish.After += Document_Publish_After;

            // Handle 301 Redirect creation on Url Slug updates
            UrlSlugInfo.TYPEINFO.Events.Update.Before += UrlSlug_Update_Before_301Redirect;

            // Handle if IsCustom was true and is now false to re-build the slug
            UrlSlugInfo.TYPEINFO.Events.Update.Before += UrlSlug_Update_Before_IsCustomRebuild;
            UrlSlugInfo.TYPEINFO.Events.Update.After += UrlSlug_Update_After_IsCustomRebuild;
        }

        private void UrlSlug_Update_After_IsCustomRebuild(object sender, ObjectEventArgs e)
        {
            UrlSlugInfo UrlSlug = (UrlSlugInfo)e.Object;
            RecursionControl Trigger = new RecursionControl("UrlSlugNoLongerCustom_" + UrlSlug.UrlSlugGuid);
            if (!Trigger.Continue)
            {
                try
                {
                    // If Continue is false, then the Before update shows this needs to be rebuilt.
                    DynamicRouteInternalHelper.RebuildRoutesByNode(UrlSlug.UrlSlugNodeID);
                }
                catch (UrlSlugCollisionException ex)
                {
                    LogErrorsInSeparateThread(ex, "DynamicRouting", "UrlSlugConflict", $"Occurred on Url Slug {UrlSlug.UrlSlugID}");
                    e.Cancel();
                }
                catch (Exception ex)
                {
                    LogErrorsInSeparateThread(ex, "DynamicRouting", "Error", $"Occurred on Url Slug {UrlSlug.UrlSlugID}");
                }
            }
        }

        private void UrlSlug_Update_Before_IsCustomRebuild(object sender, ObjectEventArgs e)
        {
            UrlSlugInfo UrlSlug = (UrlSlugInfo)e.Object;

            // If the Url Slug is custom or was custom, then need to rebuild after.
            if (UrlSlug.UrlSlugIsCustom || ValidationHelper.GetBoolean(UrlSlug.GetOriginalValue("UrlSlugIsCustom"), UrlSlug.UrlSlugIsCustom))
            {
                // Add hook so the Url Slug will be re-rendered after it's updated
                RecursionControl Trigger = new RecursionControl("UrlSlugNoLongerCustom_" + UrlSlug.UrlSlugGuid);
                var Triggered = Trigger.Continue;
            }
        }


        private void Document_Publish_After(object sender, WorkflowEventArgs e)
        {
            // Update the document itself
            try
            {
                DynamicRouteEventHelper.DocumentInsertUpdated(e.Document.NodeID);
            }
            catch (UrlSlugCollisionException ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "UrlSlugConflict", $"Occurred on Document Publish After of Node {e.Document.NodeID} [{e.Document.NodeAliasPath}]");
                e.Cancel();
            }
            catch (Exception ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "Error", $"Occurred on Document Publish After of Node {e.Document.NodeID} [{e.Document.NodeAliasPath}]");
            }
        }


        private void UrlSlug_Update_Before_301Redirect(object sender, ObjectEventArgs e)
        {
            UrlSlugInfo UrlSlug = (UrlSlugInfo)e.Object;

            try
            {
                // Alternative Urls don't have the slash at the beginning
                string OriginalUrlSlug = ValidationHelper.GetString(UrlSlug.GetOriginalValue("UrlSlug"), UrlSlug.UrlSlug).Trim('/');

                // save previous Url to 301 redirects
                // Get DocumentID
                var Document = DocumentHelper.GetDocuments()
                    .WhereEquals("NodeID", UrlSlug.UrlSlugNodeID)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .Culture(UrlSlug.UrlSlugCultureCode)
                    .FirstOrDefault();
                var AlternativeUrl = AlternativeUrlInfoProvider.GetAlternativeUrls()
                    .WhereEquals("AlternativeUrlUrl", OriginalUrlSlug)
                    .FirstOrDefault();

                SiteInfo Site = SiteInfoProvider.GetSiteInfo(Document.NodeSiteID);
                string DefaultCulture = SettingsKeyInfoProvider.GetValue("CMSDefaultCultureCode", new SiteInfoIdentifier(Site.SiteName));

                if (AlternativeUrl != null)
                {
                    if (AlternativeUrl.AlternativeUrlDocumentID != Document.DocumentID)
                    {
                        // If Same NodeID, then make sure the DocumentID is of the one that is the DefaultCulture, if no DefaultCulture
                        // Exists, then just ignore
                        var AlternativeUrlDocument = DocumentHelper.GetDocument(AlternativeUrl.AlternativeUrlDocumentID, new TreeProvider());

                        // Log a warning
                        if (AlternativeUrlDocument.NodeID != UrlSlug.UrlSlugNodeID)
                        {
                            EventLogProvider.LogEvent("W", "DynamicRouting", "AlternativeUrlConflict", eventDescription: string.Format("Conflict between Alternative Url '{0}' exists for Document {1} [{2}] which already exists as an Alternative Url for Document {3} [{4}].",
                                AlternativeUrl.AlternativeUrlUrl,
                                Document.NodeAliasPath,
                                Document.DocumentCulture,
                                AlternativeUrlDocument.NodeAliasPath,
                                AlternativeUrlDocument.DocumentCulture
                                ));
                        }
                        TreeNode DefaultLanguage = DocumentHelper.GetDocuments()
                            .WhereEquals("NodeID", UrlSlug.UrlSlugNodeID)
                            .Culture(DefaultCulture)
                            .CombineWithDefaultCulture()
                            .FirstOrDefault();

                        // Save only if there is no default language, or it is the default language, or if there is a default language adn it isn't it, that the Url doesn't match
                        // Any of the default languages urls, as this often happens when you clone from an existing language and then save a new url.
                        bool DefaultLanguageExists = DefaultLanguage != null;
                        bool IsNotDefaultLanguage = DefaultLanguageExists && AlternativeUrl.AlternativeUrlDocumentID != DefaultLanguage.DocumentID;
                        bool MatchesDefaultLang = false;
                        if (DefaultLanguageExists && IsNotDefaultLanguage)
                        {
                            // See if the OriginalUrlSlug matches the default document, or one of it's alternates
                            var DefaultLangUrlSlug = UrlSlugInfoProvider.GetUrlSlugs()
                                .WhereEquals("UrlSlugNodeID", UrlSlug.UrlSlugNodeID)
                                .WhereEquals("UrlSlugCultureCode", DefaultLanguage.DocumentCulture)
                                .WhereEquals("UrlSlug", "/" + OriginalUrlSlug)
                                .FirstOrDefault();
                            var DefaultLangAltUrl = AlternativeUrlInfoProvider.GetAlternativeUrls()
                                .WhereEquals("AlternativeUrlDocumentID", DefaultLanguage.DocumentID)
                                .WhereEquals("AlternativeUrlUrl", OriginalUrlSlug)
                                .FirstOrDefault();
                            MatchesDefaultLang = DefaultLangUrlSlug != null || DefaultLangAltUrl != null;
                        }

                        if (!DefaultLanguageExists || !IsNotDefaultLanguage || (DefaultLanguageExists && IsNotDefaultLanguage && !MatchesDefaultLang))
                        {
                            AlternativeUrl.AlternativeUrlDocumentID = DefaultLanguage.DocumentID;
                            AlternativeUrlInfoProvider.SetAlternativeUrlInfo(AlternativeUrl);
                        }
                    }
                }
                else
                {
                    // Create new one
                    AlternativeUrl = new AlternativeUrlInfo()
                    {
                        AlternativeUrlDocumentID = Document.DocumentID,
                        AlternativeUrlSiteID = Document.NodeSiteID,
                    };
                    AlternativeUrl.SetValue("AlternativeUrlUrl", OriginalUrlSlug);

                    // Save only if there is no default language, or it is the default language, or if there is a default language adn it isn't it, that the Url doesn't match
                    // Any of the default languages urls, as this often happens when you clone from an existing language and then save a new url.
                    TreeNode DefaultLanguage = DocumentHelper.GetDocuments()
                            .WhereEquals("NodeID", UrlSlug.UrlSlugNodeID)
                            .Culture(DefaultCulture)
                            .FirstOrDefault();
                    bool DefaultLanguageExists = DefaultLanguage != null;
                    bool IsNotDefaultLanguage = DefaultLanguageExists && AlternativeUrl.AlternativeUrlDocumentID != DefaultLanguage.DocumentID;
                    bool MatchesDefaultLang = false;
                    if (DefaultLanguageExists && IsNotDefaultLanguage)
                    {
                        // See if the OriginalUrlSlug matches the default document, or one of it's alternates
                        var DefaultLangUrlSlug = UrlSlugInfoProvider.GetUrlSlugs()
                            .WhereEquals("UrlSlugNodeID", UrlSlug.UrlSlugNodeID)
                            .WhereEquals("UrlSlugCultureCode", DefaultLanguage.DocumentCulture)
                            .WhereEquals("UrlSlug", "/" + OriginalUrlSlug)
                            .FirstOrDefault();
                        var DefaultLangAltUrl = AlternativeUrlInfoProvider.GetAlternativeUrls()
                            .WhereEquals("AlternativeUrlDocumentID", DefaultLanguage.DocumentID)
                            .WhereEquals("AlternativeUrlUrl", OriginalUrlSlug)
                            .FirstOrDefault();
                        MatchesDefaultLang = DefaultLangUrlSlug != null || DefaultLangAltUrl != null;
                    }
                    if (!DefaultLanguageExists || !IsNotDefaultLanguage || (DefaultLanguageExists && IsNotDefaultLanguage && !MatchesDefaultLang))
                    {
                        try
                        {
                            AlternativeUrlInfoProvider.SetAlternativeUrlInfo(AlternativeUrl);
                        }
                        catch (InvalidAlternativeUrlException ex)
                        {
                            // Figure out what to do, it doesn't match the pattern constraints.
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "AlternateUrlError", $"Occurred on Url Slug Update for Url Slug {UrlSlug.UrlSlug} {UrlSlug.UrlSlugCultureCode}");
            }
        }

        private void Document_Update_After(object sender, DocumentEventArgs e)
        {
            // Update the document itself, only if there is no workflow
            try
            {
                if (e.Node.WorkflowStep == null)
                {
                    DynamicRouteEventHelper.DocumentInsertUpdated(e.Node.NodeID);
                }
                else
                {
                    if (e.Node.WorkflowStep.StepIsPublished && DynamicRouteInternalHelper.ErrorOnConflict())
                    {
                        DynamicRouteEventHelper.DocumentInsertUpdated_CheckOnly(e.Node.NodeID);
                    }
                }
            }
            catch (UrlSlugCollisionException ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "UrlSlugConflict", $"Occurred on Document Update After for ${e.Node.NodeAlias}.");
                e.Cancel();
            }
            catch (Exception ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "Error", $"Occurred on Document Update After for ${e.Node.NodeAlias}.");
                e.Cancel();
            }
        }

        private static void LogErrorsInSeparateThread(Exception ex, string Source, string EventCode, string Description)
        {
            CMSThread LogErrorsThread = new CMSThread(new ThreadStart(() => LogErrors(ex, Source, EventCode, Description)), new ThreadSettings()
            {
                Mode = ThreadModeEnum.Async,
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal,
                UseEmptyContext = false,
                CreateLog = true
            });
            LogErrorsThread.Start();
        }

        /// <summary>
        /// Async helper method, grabs the Queue that is set to be "running" for this application and processes.
        /// </summary>
        private static void LogErrors(Exception ex, string Source, string EventCode, string Description)
        {
            EventLogProvider.LogException(Source, EventCode, ex, additionalMessage: Description);
        }


        private void Document_Sort_After(object sender, DocumentSortEventArgs e)
        {
            // Check parent which will see if Children need update
            try
            {
                DynamicRouteInternalHelper.RebuildRoutesByNode(e.ParentNodeId);
            }
            catch (UrlSlugCollisionException ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "UrlSlugConflict", $"Occurred on Document Sort Update After for Parent Node ${e.ParentNodeId}.");
                e.Cancel();
            }
            catch (Exception ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "Error", $"Occurred on Document Sort Update After for Parent Node ${e.ParentNodeId}.");
            }
        }

        private void Document_Move_Before(object sender, DocumentEventArgs e)
        {
            // Add track of the Document's original Parent ID so we can rebuild on that after moved.
            try
            {
                var Slot = Thread.GetNamedDataSlot("PreviousParentIDForNode_" + e.Node.NodeID);
                if (Slot == null)
                {
                    Slot = Thread.AllocateNamedDataSlot("PreviousParentIDForNode_" + e.Node.NodeID);
                }
                Thread.SetData(Slot, e.Node.NodeParentID);
            }
            catch (Exception ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "Error",$"Occurred on Document Move Before for Node {e.Node.NodeAliasPath}");
            }
        }

        private void Document_Move_After(object sender, DocumentEventArgs e)
        {
            // Update on the Node itself, this will rebuild itself and it's children
            DynamicRouteInternalHelper.CommitTransaction(true);
            try
            {
                DynamicRouteEventHelper.DocumentInsertUpdated(e.Node.NodeID);

                var PreviousParentNodeID = Thread.GetData(Thread.GetNamedDataSlot("PreviousParentIDForNode_" + e.Node.NodeID));
                if (PreviousParentNodeID != null && (int)PreviousParentNodeID != e.TargetParentNodeID)
                {
                    // If differnet node IDs, it moved to another parent, so also run Document Moved check on both new and old parent
                    DynamicRouteEventHelper.DocumentMoved((int)PreviousParentNodeID, e.TargetParentNodeID);
                }
            }
            catch (UrlSlugCollisionException ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "UrlSlugConflict", $"Occurred on Document After Before for Node {e.Node.NodeAliasPath}");
                e.Cancel();
            }
            catch (Exception ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "Error", $"Occurred on Document Move After for Node {e.Node.NodeAliasPath}");
            }
        }

        private void Document_InsertNewCulture_After(object sender, DocumentEventArgs e)
        {
            try
            {
                DynamicRouteEventHelper.DocumentInsertUpdated(e.Node.NodeID);
            }
            catch (UrlSlugCollisionException ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "UrlSlugConflict", $"Occurred on Document New Culture After for Node {e.Node.NodeAliasPath}");
                e.Cancel();
            }
            catch (Exception ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "Error", $"Occurred on Document New Culture After for Node {e.Node.NodeAliasPath}");
            }
        }

        private void Document_InsertLink_After(object sender, DocumentEventArgs e)
        {
            try
            {
                DynamicRouteEventHelper.DocumentInsertUpdated(e.Node.NodeID);
            }
            catch (UrlSlugCollisionException ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "UrlSlugConflict", $"Occurred on Document Insert Link After for Node {e.Node.NodeAliasPath}");
                e.Cancel();
            }
            catch (Exception ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "Error", $"Occurred on Document Insert Link After for Node {e.Node.NodeAliasPath}");
            }
        }

        private void Document_Insert_After(object sender, DocumentEventArgs e)
        {
            // Prevents the CHangeOrderAfter which may trigger before this from creating a double queue item.
            RecursionControl PreventInsertAfter = new RecursionControl("PreventInsertAfter" + e.Node.NodeID);
            if (PreventInsertAfter.Continue)
            {
                try
                {
                    DynamicRouteEventHelper.DocumentInsertUpdated(e.Node.NodeID);
                }
                catch (UrlSlugCollisionException ex)
                {
                    LogErrorsInSeparateThread(ex, "DynamicRouting", "UrlSlugConflict", $"Occurred on Document Insert After for Node {e.Node.NodeAliasPath}");
                    e.Cancel();
                }
                catch (Exception ex)
                {
                    LogErrorsInSeparateThread(ex, "DynamicRouting", "Error", $"Occurred on Document Insert After for Node {e.Node.NodeAliasPath}");
                }
            }
        }

        private void Document_Delete_After(object sender, DocumentEventArgs e)
        {
            try
            {
                DynamicRouteEventHelper.DocumentDeleted(e.Node.NodeParentID);
            }
            catch (UrlSlugCollisionException ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "UrlSlugConflict", $"Occurred on Document Delete for Node {e.Node.NodeAliasPath}");
                e.Cancel();
            }
            catch (Exception ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "Error", $"Occurred on Document Delete for Node {e.Node.NodeAliasPath}");
            }
        }

        private void Document_Copy_After(object sender, DocumentEventArgs e)
        {
            try
            {
                DynamicRouteEventHelper.DocumentInsertUpdated(e.Node.NodeID);
            }
            catch (UrlSlugCollisionException ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "UrlSlugConflict", $"Occurred on Document Copy for Node {e.Node.NodeAliasPath}");
                e.Cancel();
            }
            catch (Exception ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "Error", $"Occurred on Document Copy for Node {e.Node.NodeAliasPath}");
            }
        }

        private void Document_ChangeOrder_After(object sender, DocumentChangeOrderEventArgs e)
        {
            // Sometimes ChangeOrder is triggered before the insert (if it inserts before other records),
            // So will use recursion helper to prevent this from running on the insert as well.
            RecursionControl PreventInsertAfter = new RecursionControl("PreventInsertAfter" + e.Node.NodeID);
            var Trigger = PreventInsertAfter.Continue;
            try
            {
                DynamicRouteEventHelper.DocumentInsertUpdated(e.Node.NodeID);
            }
            catch (UrlSlugCollisionException ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "UrlSlugConflict", $"Occurred on Document Change Order for Node {e.Node.NodeAliasPath}");
                e.Cancel();
            }
            catch (Exception ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "Error", $"Occurred on Document Change Order for Node {e.Node.NodeAliasPath}");
            }
        }

        private void DataClass_Update_Before(object sender, ObjectEventArgs e)
        {
            // Check if the Url Pattern is changing
            DataClassInfo Class = (DataClassInfo)e.Object;
            if (!Class.ClassURLPattern.Equals(ValidationHelper.GetString(e.Object.GetOriginalValue("ClassURLPattern"), "")))
            {
                // Add key that the "After" will check, if the Continue is "False" then this was hit, so we actually want to continue.
                RecursionControl TriggerClassUpdateAfter = new RecursionControl("TriggerClassUpdateAfter_" + Class.ClassName);
                var Trigger = TriggerClassUpdateAfter.Continue;
            }
        }

        private void DataClass_Update_After(object sender, ObjectEventArgs e)
        {
            DataClassInfo Class = (DataClassInfo)e.Object;
            RecursionControl PreventDoubleClassUpdateTrigger = new RecursionControl("PreventDoubleClassUpdateTrigger_" + Class.ClassName);

            // If the "Continue" is false, it means that a DataClass_Update_Before found that the UrlPattern was changed
            // Otherwise the "Continue" will be true that this is the first time triggering it.
            if (!new RecursionControl("TriggerClassUpdateAfter_" + Class.ClassName).Continue && PreventDoubleClassUpdateTrigger.Continue)
            {
                try
                {
                    DynamicRouteEventHelper.ClassUrlPatternChanged(Class.ClassName);
                }
                catch (UrlSlugCollisionException ex)
                {
                    LogErrorsInSeparateThread(ex, "DynamicRouting", "UrlSlugConflict", $"Occurred on Class Update After for class {Class.ClassName}");
                    e.Cancel();
                }
                catch (Exception ex)
                {
                    LogErrorsInSeparateThread(ex, "DynamicRouting", "Error", $"Occurred on Class Update After for class {Class.ClassName}");
                }
            }
        }

        private void SettingsKey_InsertUpdate_After(object sender, ObjectEventArgs e)
        {
            SettingsKeyInfo Key = (SettingsKeyInfo)e.Object;
            switch (Key.KeyName.ToLower())
            {
                case "cmsdefaultculturecode":
                    try
                    {
                        if (Key.SiteID > 0)
                        {
                            string SiteName = DynamicRouteInternalHelper.GetSite(Key.SiteID).SiteName;
                            DynamicRouteEventHelper.SiteDefaultLanguageChanged(SiteName);
                        }
                        else
                        {
                            foreach (string SiteName in SiteInfoProvider.GetSites().Select(x => x.SiteName))
                            {
                                DynamicRouteEventHelper.SiteDefaultLanguageChanged(SiteName);
                            }
                        }
                    }
                    catch (UrlSlugCollisionException ex)
                    {
                        LogErrorsInSeparateThread(ex, "DynamicRouting", "UrlSlugConflict", $"Occurred on Settings Key Update After for Key {Key.KeyName}");
                        e.Cancel();
                    }
                    catch (Exception ex)
                    {
                        LogErrorsInSeparateThread(ex, "DynamicRouting", "Error", $"Occurred on Settings Key Update After for Key {Key.KeyName}");
                    }
                    break;
                case "generateculturevariationurlslugs":
                    try
                    {
                        if (Key.SiteID > 0)
                        {
                            string SiteName = DynamicRouteInternalHelper.GetSite(Key.SiteID).SiteName;
                            DynamicRouteEventHelper.CultureVariationSettingsChanged(SiteName);
                        }
                        else
                        {
                            foreach (string SiteName in SiteInfoProvider.GetSites().Select(x => x.SiteName))
                            {
                                DynamicRouteEventHelper.CultureVariationSettingsChanged(SiteName);
                            }
                        }
                    }
                    catch (UrlSlugCollisionException ex)
                    {
                        LogErrorsInSeparateThread(ex, "DynamicRouting", "UrlSlugConflict", $"Occurred on Settings Key Update After for Key {Key.KeyName}");
                        e.Cancel();
                    }
                    catch (Exception ex)
                    {
                        LogErrorsInSeparateThread(ex, "DynamicRouting", "Error", $"Occurred on Settings Key Update After for Key {Key.KeyName}");
                    }
                    break;
            }
        }

        private void CultureSite_InsertDelete_After(object sender, ObjectEventArgs e)
        {
            CultureSiteInfo CultureSite = (CultureSiteInfo)e.Object;
            string SiteName = DynamicRouteInternalHelper.GetSite(CultureSite.SiteID).SiteName;
            try
            {
                DynamicRouteEventHelper.SiteLanguageChanged(SiteName);
            }
            catch (UrlSlugCollisionException ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "UrlSlugConflict", $"Occurred on Culture Site Insert/Delete for Site {SiteName}");
                e.Cancel();
            }
            catch (Exception ex)
            {
                LogErrorsInSeparateThread(ex, "DynamicRouting", "Error", $"Occurred on Culture Site Insert/Delete for Site {SiteName}");
            }
        }
    }
}
