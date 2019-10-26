using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;
using DynamicRouting.Kentico;
using System;
using System.Linq;
using System.Threading;

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
            try { 
                ConnectionHelper.ExecuteNonQuery("DynamicRouting.UrlSlug.InitializeSQLEntities");
            } catch(Exception ex)
            {
                EventLogProvider.LogException("DynamicRouting", "ErrorRunningSQLEntities", ex, additionalMessage: "Could not run DynamicRouting.UrlSlug.InitializeSQLEntities Query, this sets up Views and Foreign Keys vital to operation.  Please ensure these queries exist.");
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
            DocumentEvents.ChangeOrder.After += Document_ChangeOrder_After; // Done
            DocumentEvents.Copy.After += Document_Copy_After;
            DocumentEvents.Delete.After += Document_Delete_After; // Done
            DocumentEvents.Insert.After += Document_Insert_After;  // Done
            DocumentEvents.InsertLink.After += Document_InsertLink_After;
            DocumentEvents.InsertNewCulture.After += Document_InsertNewCulture_After; // Done
            DocumentEvents.Move.Before += Document_Move_Before;
            DocumentEvents.Move.After += Document_Move_After;
            DocumentEvents.Sort.After += Document_Sort_After; // Done
            DocumentEvents.Update.After += Document_Update_After;

            // Handle 301 Redirect creation on Url Slug updates
            UrlSlugInfo.TYPEINFO.Events.Update.Before += UrlSlug_Update_Before;
        }

        private void UrlSlug_Update_Before(object sender, ObjectEventArgs e)
        {
            UrlSlugInfo UrlSlug = (UrlSlugInfo)e.Object;
            string OriginalUrlSlug = ValidationHelper.GetString(UrlSlug.GetOriginalValue("UrlSlug"), UrlSlug.UrlSlug);
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
            if (AlternativeUrl != null)
            {
                if (AlternativeUrl.AlternativeUrlDocumentID != Document.DocumentID)
                {
                    // If Same NodeID, then make sure the DocumentID is of the one that is the DefaultCulture, if no DefaultCulture
                    // Exists, then just ignore
                    var AlternativeUrlDocument = DocumentHelper.GetDocument(AlternativeUrl.AlternativeUrlDocumentID, new TreeProvider());
                    if (AlternativeUrlDocument.NodeID == UrlSlug.UrlSlugNodeID)
                    {
                        TreeNode DefaultLanguage = DocumentHelper.GetDocuments()
                            .WhereEquals("NodeID", UrlSlug.UrlSlugNodeID)
                            .CombineWithDefaultCulture()
                            .FirstOrDefault();
                        if (DefaultLanguage != null && AlternativeUrl.AlternativeUrlDocumentID != DefaultLanguage.DocumentID)
                        {
                            AlternativeUrl.AlternativeUrlDocumentID = DefaultLanguage.DocumentID;
                            AlternativeUrlInfoProvider.SetAlternativeUrlInfo(AlternativeUrl);
                        }
                    }
                    else
                    {
                        // Log 
                        EventLogProvider.LogEvent("W", "DynamicRouting", "AlternativeUrlConflict", eventDescription: string.Format("Could not create Alternative Url '{0}' for Document {1} [{2}] because it already exists as an Alternative Url for Document {3} [{4}]",
                            AlternativeUrl.AlternativeUrlUrl,
                            Document.NodeAliasPath,
                            Document.DocumentCulture,
                            AlternativeUrlDocument.NodeAliasPath,
                            AlternativeUrlDocument.DocumentCulture
                            ));
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
                AlternativeUrlInfoProvider.SetAlternativeUrlInfo(AlternativeUrl);
            }
        }

        private void Document_Update_After(object sender, DocumentEventArgs e)
        {
            // Update the document itself
            DynamicRouteEventHelper.DocumentInsertUpdated(e.Node.NodeID);
        }

        private void Document_Sort_After(object sender, DocumentSortEventArgs e)
        {
            // Check parent which will see if Children need update
            DynamicRouteInternalHelper.RebuildRoutesByNode(e.ParentNodeId);
        }

        private void Document_Move_Before(object sender, DocumentEventArgs e)
        {
            // Add track of the Document's original Parent ID so we can rebuild on that after moved.
            var Slot = Thread.AllocateNamedDataSlot("PreviousParentIDForNode_" + e.Node.NodeID);
            Thread.SetData(Slot, e.Node.NodeParentID);
        }

        private void Document_Move_After(object sender, DocumentEventArgs e)
        {
            var Slot = Thread.AllocateNamedDataSlot("PreviousParentIDForNode_" + e.Node.NodeID);
            var PreviousParentNodeID = Thread.GetData(Slot);
            if (PreviousParentNodeID != null)
            {
                // Check if the move was just ordering
                if((int)PreviousParentNodeID != e.TargetParentNodeID) { 
                    DynamicRouteEventHelper.DocumentMoved((int)PreviousParentNodeID, e.TargetParentNodeID);
                } else
                {
                    // Just update the document which will check for siblings with NodeOrder
                    DynamicRouteEventHelper.DocumentInsertUpdated(e.Node.NodeID);
                }
            }
        }

        private void Document_InsertNewCulture_After(object sender, DocumentEventArgs e)
        {
            DynamicRouteEventHelper.DocumentInsertUpdated(e.Node.NodeID);
        }

        private void Document_InsertLink_After(object sender, DocumentEventArgs e)
        {
            DynamicRouteEventHelper.DocumentInsertUpdated(e.Node.NodeID);
        }

        private void Document_Insert_After(object sender, DocumentEventArgs e)
        {
            // Prevents the CHangeOrderAfter which may trigger before this from creating a double queue item.
            RecursionControl PreventInsertAfter = new RecursionControl("PreventInsertAfter" + e.Node.NodeID);
            if(PreventInsertAfter.Continue) { 
                DynamicRouteEventHelper.DocumentInsertUpdated(e.Node.NodeID);
            }
        }

        private void Document_Delete_After(object sender, DocumentEventArgs e)
        {
            DynamicRouteEventHelper.DocumentDeleted(e.Node.NodeParentID);
        }

        private void Document_Copy_After(object sender, DocumentEventArgs e)
        {
            DynamicRouteEventHelper.DocumentInsertUpdated(e.Node.NodeID);
        }

        private void Document_ChangeOrder_After(object sender, DocumentChangeOrderEventArgs e)
        {
            // Sometimes ChangeOrder is triggered before the insert (if it inserts before other records),
            // So will use recursion helper to prevent this from running on the insert as well.
            RecursionControl PreventInsertAfter = new RecursionControl("PreventInsertAfter" + e.Node.NodeID);
            var Trigger = PreventInsertAfter.Continue;
            DynamicRouteEventHelper.DocumentInsertUpdated(e.Node.NodeID);
        }

        private void DataClass_Update_Before(object sender, ObjectEventArgs e)
        {
            // Check if the Url Pattern is changing
            DataClassInfo Class = (DataClassInfo)e.Object;
            if(!Class.ClassURLPattern.Equals(ValidationHelper.GetString(e.Object.GetOriginalValue("ClassURLPattern"), "")))
            {
                // Add key that the "After" will check, if the Continue is "False" then this was hit, so we actually want to continue.
                RecursionControl TriggerClassUpdateAfter = new RecursionControl("TriggerClassUpdateAfter_" + Class.ClassName);
                var Trigger = TriggerClassUpdateAfter.Continue;
            }
        }

        private void DataClass_Update_After(object sender, ObjectEventArgs e)
        {
            DataClassInfo Class = (DataClassInfo)e.Object;
            // If the "Continue" is false, it means that a DataClass_Update_Before found that the UrlPattern was changed
            // Otherwise the "Continue" will be true that this is the first time triggering it.
            if (!new RecursionControl("TriggerClassUpdateAfter_" + Class.ClassName).Continue) {
                DynamicRouteEventHelper.ClassUrlPatternChanged(Class.ClassName);
            }
        }

        private void SettingsKey_InsertUpdate_After(object sender, ObjectEventArgs e)
        {
            SettingsKeyInfo Key = (SettingsKeyInfo)e.Object;
            switch (Key.KeyName.ToLower())
            {
                case "cmsdefaultculturecode":
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
                    break;
                case "generateculturevariationurlslugs":
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
                    break;
            }
        }

        private void CultureSite_InsertDelete_After(object sender, ObjectEventArgs e)
        {
            CultureSiteInfo CultureSite = (CultureSiteInfo)e.Object;
            string SiteName = DynamicRouteInternalHelper.GetSite(CultureSite.SiteID).SiteName;
            DynamicRouteEventHelper.SiteLanguageChanged(SiteName);
        }
    }
}
