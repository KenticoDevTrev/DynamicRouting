using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.SiteProvider;
using DynamicRouting.Kentico.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml.Serialization;

namespace DynamicRouting
{
    /// <summary>
    /// Helper methods used internally
    /// </summary>
    public static class DynamicRouteInternalHelper
    {
        public static SiteInfo SiteContextSafe()
        {
            return SiteContext.CurrentSite != null ? SiteContext.CurrentSite : SiteInfoProvider.GetSites().TopN(1).FirstOrDefault();
        }

        /// <summary>
        /// Cleans up the Url given the site settings
        /// </summary>
        /// <param name="Url">The Relative Url</param>
        /// <param name="SiteName">The Site name to get settings from</param>
        /// <returns>The cleaned Url.</returns>
        public static string GetCleanUrl(string Url, string SiteName = "")
        {
            // Remove trailing or double //'s and any url parameters / anchors
            Url = "/" + Url.Trim("/ ".ToCharArray()).Split('?')[0].Split('#')[0];
            Url = HttpUtility.UrlDecode(Url);

            // Replace forbidden characters
            // Remove / from the forbidden characters because that is part of the Url, of course.
            if (string.IsNullOrWhiteSpace(SiteName) && !string.IsNullOrWhiteSpace(SiteContextSafe().SiteName))
            {
                SiteName = SiteContextSafe().SiteName;
            }
            if (!string.IsNullOrWhiteSpace(SiteName))
            {
                string ForbiddenCharacters = URLHelper.ForbiddenURLCharacters(SiteName).Replace("/", "");
                string Replacement = URLHelper.ForbiddenCharactersReplacement(SiteName).ToString();
                Url = ReplaceAnyCharInString(Url, ForbiddenCharacters.ToCharArray(), Replacement);
            }

            // Escape special url characters
            Url = URLHelper.EscapeSpecialCharacters(Url);

            return Url;
        }

        /// <summary>
        /// Replaces any char in the char array with the replace value for the string
        /// </summary>
        /// <param name="value">The string to replace values in</param>
        /// <param name="CharsToReplace">The character array of characters to replace</param>
        /// <param name="ReplaceValue">The value to replace them with</param>
        /// <returns>The cleaned string</returns>
        private static string ReplaceAnyCharInString(string value, char[] CharsToReplace, string ReplaceValue)
        {
            string[] temp = value.Split(CharsToReplace, StringSplitOptions.RemoveEmptyEntries);
            return String.Join(ReplaceValue, temp);
        }

        #region "Relational Checks required"

        /// <summary>
        /// Check if any pages of children of this node's parent have a class with Url Pattern contains NodeOrder, if so then the siblings of a changed node need to have its siblings built (start with parent)
        /// </summary>
        /// <returns>If Siblings need to be checked for URL slug changes</returns>
        public static bool CheckSiblings(string NodeAliasPath = "", string SiteName = "")
        {
            // Get any classes that have {% NodeOrder %}
            List<int> ClassIDs = CacheHelper.Cache<List<int>>(cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency("cms.class|all");
                }
                return DataClassInfoProvider.GetClasses()
                .WhereLike("ClassURLPattern", "%NodeOrder%")
                .Select(x => x.ClassID).ToList();
            }, new CacheSettings(1440, "ClassesForSiblingCheck"));

            // If no NodeAliasPath given, then return if there are any Classes that have NodeOrder
            if (string.IsNullOrWhiteSpace(NodeAliasPath))
            {
                return ClassIDs.Count > 0;
            }
            else
            {
                SiteName = GetSiteName(SiteName);

                var Document = DocumentHelper.GetDocuments()
                    .WhereEquals("NodeAliasPath", NodeAliasPath)
                    .OnSite(SiteName)
                    .CombineWithAnyCulture().FirstOrDefault();

                // return if any siblings have a class NodeOrder exist
                return DocumentHelper.GetDocuments()
                    .WhereEquals("NodeParentID", Document.NodeParentID)
                    .WhereNotEquals("NodeID", Document.NodeID)
                    .WhereIn("NodeClassID", ClassIDs)
                    .CombineWithAnyCulture()
                    .Columns("NodeID")
                    .Count > 0;
            }
        }

        /// <summary>
        /// Check if any child Url Pattern contains NodeLevel, NodeParentID, NodeAliasPath, DocumentNamePath if so then must check all children as there could be one of these page types down the tree that is modified.
        /// </summary>
        /// <returns>If Descendents need to be checked for URL slug changes</returns>
        public static bool CheckDescendents(string NodeAliasPath = "", string SiteName = "")
        {
            // Get any classes that have {% NodeOrder %}
            List<int> ClassIDs = CacheHelper.Cache<List<int>>(cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency("cms.class|all");
                }
                return DataClassInfoProvider.GetClasses()
                .WhereLike("ClassURLPattern", "%NodeLevel%")
                .Or()
                .WhereLike("ClassURLPattern", "%NodeParentID%")
                .Or()
                .WhereLike("ClassURLPattern", "%NodeAliasPath%")
                .Or()
                .WhereLike("ClassURLPattern", "%DocumentNamePath%")
                .Select(x => x.ClassID).ToList();
            }, new CacheSettings(1440, "ClassesForDescendentCheck"));

            // If no NodeAliasPath given, then return if there are any Classes that have NodeOrder
            if (string.IsNullOrWhiteSpace(NodeAliasPath))
            {
                return ClassIDs.Count > 0;
            }
            else
            {
                SiteName = GetSiteName(SiteName);

                // return if any siblings have a class NodeOrder exist
                return DocumentHelper.GetDocuments()
                    .Path(NodeAliasPath, PathTypeEnum.Children)
                    .OnSite(new SiteInfoIdentifier(SiteName))
                    .CombineWithAnyCulture()
                    .WhereIn("NodeClassID", ClassIDs)
                    .Columns("NodeID")
                    .Count > 0;
            }
        }

        /// <summary>
        /// Check if any Url pattern contains ParentUrl
        /// </summary>
        /// <returns>If Children need to be checked for URL slug changes</returns>
        public static bool CheckChildren(string NodeAliasPath = "", string SiteName = "")
        {
            // Get any classes that have {% NodeOrder %}
            List<int> ClassIDs = CacheHelper.Cache<List<int>>(cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency("cms.class|all");
                }
                return DataClassInfoProvider.GetClasses()
                .WhereLike("ClassURLPattern", "%ParentUrl%")
                .Select(x => x.ClassID).ToList();
            }, new CacheSettings(1440, "ClassesForChildrenCheck"));

            // If no NodeAliasPath given, then return if there are any Classes that have NodeOrder
            if (string.IsNullOrWhiteSpace(NodeAliasPath))
            {
                return ClassIDs.Count > 0;
            }
            else
            {
                SiteName = GetSiteName(SiteName);

                var Document = DocumentHelper.GetDocuments()
                    .Path(NodeAliasPath, PathTypeEnum.Single)
                    .OnSite(SiteName)
                    .CombineWithAnyCulture()
                    .FirstOrDefault();

                // return if any Children have a class NodeOrder exist
                return DocumentHelper.GetDocuments()
                    .WhereEquals("NodeParentID", Document.NodeID)
                    .WhereIn("NodeClassID", ClassIDs)
                    .CombineWithAnyCulture()
                    .Columns("NodeID")
                    .Count > 0;
            }
        }

        #endregion

        #region "Node Item Builder"

        /// <summary>
        /// Get the Node Item Builder Settings based on the given NodeAliasPath, should be used for individual page updates as will limit what is checked based on the page.
        /// </summary>
        /// <param name="NodeAliasPath">The Node Alias Path</param>
        /// <param name="SiteName"></param>
        /// <param name="CheckingForUpdates"></param>
        /// <param name="CheckEntireTree"></param>
        /// <returns>The node item builder setting</returns>
        private static NodeItemBuilderSettings GetNodeItemBuilderSettings(string NodeAliasPath, string SiteName, bool CheckingForUpdates, bool CheckEntireTree)
        {
            return CacheHelper.Cache(cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(new string[]
                    {
                        "cms.site|byname|"+SiteName,
                        "cms.siteculture|all",
                        "cms.settingskey|byname|CMSDefaultCultureCode",
                        "cms.settingskey|byname|GenerateCultureVariationUrlSlugs",
                        "cms.class|all"
                    });
                }
                // Loop through Cultures and start rebuilding pages
                SiteInfo Site = SiteInfoProvider.GetSiteInfo(SiteName);
                string DefaultCulture = SettingsKeyInfoProvider.GetValue("CMSDefaultCultureCode", new SiteInfoIdentifier(SiteName));

                bool GenerateCultureVariationUrlSlugs = SettingsKeyInfoProvider.GetBoolValue("GenerateCultureVariationUrlSlugs", new SiteInfoIdentifier(SiteName));

                var BaseMacroResolver = MacroResolver.GetInstance(true);
                BaseMacroResolver.AddAnonymousSourceData(new object[] { Site });

                // Now build URL slugs for the default language always.
                List<string> Cultures = CultureSiteInfoProvider.GetSiteCultureCodes(SiteName);

                // Configure relational checks based on node
                bool BuildSiblings = CheckSiblings(NodeAliasPath, SiteName);
                bool BuildDescendents = CheckDescendents(NodeAliasPath, SiteName);
                bool BuildChildren = CheckChildren(NodeAliasPath, SiteName);

                return new NodeItemBuilderSettings(Cultures, DefaultCulture, GenerateCultureVariationUrlSlugs, BaseMacroResolver, CheckingForUpdates, CheckEntireTree, BuildSiblings, BuildChildren, BuildDescendents, SiteName);
            }, new CacheSettings(1440, "GetNodeItemBuilderSettings", NodeAliasPath, SiteName, CheckingForUpdates, CheckEntireTree));
        }

        /// <summary>
        /// Helper that fills in the NodeItemBuilderSetting based on the SiteName and configured options.
        /// </summary>
        /// <param name="SiteName">The Site Name</param>
        /// <param name="CheckingForUpdates">If Updates should be checked or not, triggering recursive checking</param>
        /// <param name="CheckEntireTree">If the entire tree should be checked</param>
        /// <param name="BuildSiblings">If siblings should be checked</param>
        /// <param name="BuildChildren">If Children Should be checked</param>
        /// <param name="BuildDescendents">If Descdendents should be checked</param>
        /// <returns>The NodeItemBuilderSetting with Site, Cultures and Default culture already set.</returns>
        private static NodeItemBuilderSettings GetNodeItemBuilderSettings(string SiteName, bool CheckingForUpdates, bool CheckEntireTree, bool BuildSiblings, bool BuildChildren, bool BuildDescendents)
        {
            return CacheHelper.Cache(cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(new string[]
                    {
                        "cms.site|byname|"+SiteName,
                        "cms.siteculture|all",
                        "cms.settingskey|byname|CMSDefaultCultureCode",
                        "cms.settingskey|byname|GenerateCultureVariationUrlSlugs",
                    });
                }
                // Loop through Cultures and start rebuilding pages
                SiteInfo Site = SiteInfoProvider.GetSiteInfo(SiteName);
                string DefaultCulture = SettingsKeyInfoProvider.GetValue("CMSDefaultCultureCode", new SiteInfoIdentifier(SiteName));

                bool GenerateCultureVariationUrlSlugs = SettingsKeyInfoProvider.GetBoolValue("GenerateCultureVariationUrlSlugs", new SiteInfoIdentifier(SiteName));

                var BaseMacroResolver = MacroResolver.GetInstance(true);
                BaseMacroResolver.AddAnonymousSourceData(new object[] { Site });

                // Now build URL slugs for the default language always.
                List<string> Cultures = CultureSiteInfoProvider.GetSiteCultureCodes(SiteName);

                return new NodeItemBuilderSettings(Cultures, DefaultCulture, GenerateCultureVariationUrlSlugs, BaseMacroResolver, CheckingForUpdates, CheckEntireTree, BuildSiblings, BuildChildren, BuildDescendents, SiteName);
            }, new CacheSettings(1440, "GetNodeItemBuilderSettings", SiteName, CheckingForUpdates, CheckEntireTree, BuildSiblings, BuildChildren, BuildDescendents));
        }

        #endregion

        #region "Rebuilding Route Methods"

        /// <summary>
        /// Rebuilds all URL Routes on all sites.
        /// </summary>
        public static void RebuildRoutes()
        {
            foreach (SiteInfo Site in SiteInfoProvider.GetSites())
            {
                RebuildRoutesBySite(Site.SiteName);
            }
        }

        /// <summary>
        /// Rebuilds all URL Routes on the given Site
        /// </summary>
        /// <param name="SiteName">The Site name</param>
        public static void RebuildRoutesBySite(string SiteName)
        {
            //EventLogProvider.LogInformation("DynamicRouteTesting", "SyncBuildStart", eventDescription: DateTime.Now.ToString() + " " + DateTime.Now.Millisecond.ToString());
            // Get NodeItemBuilderSettings
            NodeItemBuilderSettings BuilderSettings = GetNodeItemBuilderSettings(SiteName, true, true, true, true, true);

            // Get root NodeID
            TreeNode RootNode = DocumentHelper.GetDocuments()
                .Path("/", PathTypeEnum.Single)
                .OnSite(SiteName)
                .CombineWithAnyCulture()
                .FirstOrDefault();

            // Rebuild NodeItem tree structure, this will only affect the initial node syncly.
            NodeItem RootNodeItem = new NodeItem(RootNode.NodeID, BuilderSettings);
            if (ErrorOnConflict())
            {
                // If error on conflict, everything MUST be done syncly.
                RootNodeItem.BuildChildren();
                if (RootNodeItem.ConflictsExist())
                {
                    EventLogProvider.LogEvent("E", "DynamicRouting", "Conflict Exists", eventDescription: $"Could not rebuild the site {SiteName}'s routes due to a conflict in the generated routes.");
                    throw new UrlSlugCollisionException("Conflict Exists, aborting save");
                }
                // Save changes
                RootNodeItem.SaveChanges();
            }
            else
            {
                // Save itself and then queue up rest
                RootNodeItem.SaveChanges(false);
                // Do rest asyncly.
                QueueUpUrlSlugGeneration(RootNodeItem, BuilderSettings.CheckQueueImmediately);
            }
            //EventLogProvider.LogInformation("DynamicRouteTesting", "SyncBuildEnd", eventDescription: DateTime.Now.ToString() + " " + DateTime.Now.Millisecond.ToString());
        }

        /// <summary>
        /// Rebuilds all URL Routes for nodes which use this class across all sites.
        /// </summary>
        /// <param name="ClassName">The Class Name</param>
        public static void RebuildRoutesByClass(string ClassName)
        {
            DataClassInfo Class = GetClass(ClassName);
            foreach (string SiteName in SiteInfoProvider.GetSites().Select(x => x.SiteName))
            {
                // Get NodeItemBuilderSettings, will be searching all children in case of changes.
                NodeItemBuilderSettings BuilderSettings = GetNodeItemBuilderSettings(SiteName, true, true, true, true, true);

                BuilderSettings.CheckQueueImmediately = false;

                // Build all, gather nodes of any Node that is of this type of class, check for updates.
                List<string> NodeAliasPathsToRemove = new List<string>();
                List<string> NodeAliasPaths = DocumentHelper.GetDocuments()
                    .WhereEquals("NodeClassID", Class.ClassID)
                    .OnSite(new SiteInfoIdentifier(SiteName))
                    .CombineWithDefaultCulture()
                    .Distinct()
                    .PublishedVersion()
                    .Columns("NodeAliasPath, NodeLevel, NodeOrder")
                    .OrderBy("NodeLevel, NodeOrder")
                    .Result.Tables[0].Rows.Cast<DataRow>().Select(x => (string)x["NodeAliasPath"]).ToList();

                // Remove any NodeAliasPaths that are a descendent of a parent item, as they will be ran when the parent is checked.
                NodeAliasPaths.ForEach(x =>
                {
                    NodeAliasPathsToRemove.AddRange(NodeAliasPaths.Where(y => y.Contains(x) && x != y));
                });
                NodeAliasPaths = NodeAliasPaths.Except(NodeAliasPathsToRemove).ToList();

                // Now convert NodeAliasPaths into NodeIDs
                List<int> NodeIDs = DocumentHelper.GetDocuments()
                        .WhereEquals("NodeClassID", Class.ClassID)
                        .WhereIn("NodeAliasPath", NodeAliasPaths)
                        .OnSite(new SiteInfoIdentifier(SiteName))
                        .CombineWithDefaultCulture()
                        .Distinct()
                        .PublishedVersion()
                        .Columns("NodeID, NodeLevel, NodeOrder")
                        .OrderBy("NodeLevel, NodeOrder")
                        .Result.Tables[0].Rows.Cast<DataRow>().Select(x => (int)x["NodeID"]).ToList();

                // Check all parent nodes for changes
                foreach (int NodeID in NodeIDs)
                {
                    RebuildRoutesByNode(NodeID, BuilderSettings);
                }

                // Now check queue to run tasks
                CheckUrlSlugGenerationQueue();
            }
        }

        /// <summary>
        /// Rebuilds the routes for the given Node
        /// </summary>
        /// <param name="NodeID">The NodeID</param>
        public static void RebuildRoutesByNode(int NodeID)
        {
            RebuildRoutesByNode(NodeID, null);
        }

        /// <summary>
        /// Rebuilds the routes for the given Node
        /// </summary>
        /// <param name="NodeID">The NodeID</param>
        public static void RebuildRoutesByNode_CheckOnly(int NodeID)
        {
            RebuildRoutesByNode(NodeID, null, true);
        }

        /// <summary>
        /// Rebuilds the node and all child node
        /// </summary>
        /// <param name="NodeID">The Node ID</param>
        public static void RebuildSubtreeRoutesByNode(int NodeID)
        {
            // Set up settings
            // Get Site from Node
            TreeNode Page = DocumentHelper.GetDocuments()
                .WhereEquals("NodeID", NodeID)
                .CombineWithAnyCulture()
                .Columns("NodeSiteID")
                .FirstOrDefault();

            // Get Settings based on the Page itself
            NodeItemBuilderSettings Settings = GetNodeItemBuilderSettings(Page.NodeAliasPath, GetSite(Page.NodeSiteID).SiteName, true, false);

            // Check all descendents
            Settings.BuildDescendents = true;
            Settings.CheckingForUpdates = false;

            RebuildRoutesByNode(NodeID, Settings);
        }

        /// <summary>
        /// Rebuilds the Routes for the given Node, optionally allows for settings to be passed
        /// </summary>
        /// <param name="NodeID">The NodeID</param>
        /// <param name="Settings">The Node Item Build Settings, if null will create settings based on the Node itself.</param>
        private static void RebuildRoutesByNode(int NodeID, NodeItemBuilderSettings Settings = null, bool CheckConflictOnly = false)
        {
            // If settings are not set, then get settings based on the given Node
            string NodeAliasPath = "";
            if (Settings == null)
            {
                // Get Site from Node
                TreeNode Page = DocumentHelper.GetDocuments()
                    .WhereEquals("NodeID", NodeID)
                    .Columns("NodeSiteID, NodeAliasPath")
                    .CombineWithAnyCulture()
                    .FirstOrDefault();
                NodeAliasPath = Page.NodeAliasPath;
                // Get Settings based on the Page itself
                Settings = GetNodeItemBuilderSettings(Page.NodeAliasPath, GetSite(Page.NodeSiteID).SiteName, true, false);
            }

            // Build and save
            NodeItem GivenNodeItem = new NodeItem(NodeID, Settings, CheckConflictOnly);
            if (ErrorOnConflict() || CheckConflictOnly)
            {
                // If error on conflict, everything MUST be done syncly.
                GivenNodeItem.BuildChildren();
                if (GivenNodeItem.ConflictsExist())
                {
                    string Error = $"Could not save document at {NodeAliasPath} due to a conflict in the generated route:\n\r {string.Join("\n\r", GivenNodeItem.GetConflictItems())}";
                    EventLogProvider.LogEvent("E", "DynamicRouting", "Conflict Exists", eventDescription: Error);
                    throw new UrlSlugCollisionException($"{Error} aborting save");
                }
                // Save changes if not only checking conflict
                if (CheckConflictOnly)
                {
                    return;
                }
                GivenNodeItem.SaveChanges();
            }
            else
            {
                // Save main one
                GivenNodeItem.SaveChanges(false);

                // Do rest asyncly.
                QueueUpUrlSlugGeneration(GivenNodeItem, Settings.CheckQueueImmediately);
            }
        }

        #endregion

        #region "Generation Queue Helpers"

        /// <summary>
        /// Adds the NodeItem to the Url Slug Generation Queue so it can be handled asyncly in the order it's added.
        /// </summary>
        /// <param name="NodeItem">The Node Item</param>
        private static void QueueUpUrlSlugGeneration(NodeItem NodeItem, bool CheckQueueImmediately = true)
        {
            // Add item to the Slug Generation Queue
            SlugGenerationQueueInfo NewQueue = new SlugGenerationQueueInfo()
            {
                SlugGenerationQueueNodeItem = SerializeObject<NodeItem>(NodeItem)
            };
            SlugGenerationQueueInfoProvider.SetSlugGenerationQueueInfo(NewQueue);

            // Run Queue checker
            if (CheckQueueImmediately)
            {
                CheckUrlSlugGenerationQueue();
            }
        }

        /// <summary>
        /// Clears the QueueRUnning flag on any tasks that the thread doesn't actually exist (something happened and the thread failed or died without setting the Running to false
        /// </summary>
        public static void ClearStuckUrlGenerationTasks()
        {
            Process currentProcess = Process.GetCurrentProcess();

            // Set any threads by this application that shows it's running but the Thread doesn't actually exist.
            SlugGenerationQueueInfoProvider.GetSlugGenerationQueues()
                .WhereEquals("SlugGenerationQueueRunning", true)
                .WhereEquals("SlugGenerationQueueApplicationID", SystemHelper.ApplicationIdentifier)
                .WhereNotIn("SlugGenerationQueueThreadID", currentProcess.Threads.Cast<ProcessThread>().Select(x => x.Id).ToArray())
                .ForEachObject(x =>
                {
                    x.SlugGenerationQueueRunning = false;
                    SlugGenerationQueueInfoProvider.SetSlugGenerationQueueInfo(x);
                });
        }

        /// <summary>
        /// Checks for Url Slug Generation Queue Items and processes any asyncly.
        /// </summary>
        public static void CheckUrlSlugGenerationQueue()
        {
            // Clear any stuck tasks
            ClearStuckUrlGenerationTasks();

            DataSet NextGenerationResult = ConnectionHelper.ExecuteQuery("DynamicRouting.SlugGenerationQueue.GetNextRunnableQueueItem", new QueryDataParameters()
            {
                {"@ApplicationID", SystemHelper.ApplicationIdentifier }
                ,{"@SkipErroredGenerations", SkipErroredGenerations()}
            });

            if (NextGenerationResult.Tables.Count > 0 && NextGenerationResult.Tables[0].Rows.Count > 0)
            {
                // Queue up task asyncly
                CMSThread UrlGenerationThread = new CMSThread(new ThreadStart(RunSlugGenerationQueueItem), new ThreadSettings()
                {
                    Mode = ThreadModeEnum.Async,
                    IsBackground = true,
                    Priority = ThreadPriority.AboveNormal,
                    UseEmptyContext = false,
                    CreateLog = true
                });
                UrlGenerationThread.Start();
            }
        }

        /// <summary>
        /// Async helper method, grabs the Queue that is set to be "running" for this application and processes.
        /// </summary>
        private static void RunSlugGenerationQueueItem()
        {
            //EventLogProvider.LogInformation("DynamicRouteTesting", "AsyncBuildStart", eventDescription: DateTime.Now.ToString() + " " + DateTime.Now.Millisecond.ToString());
            // Get the current thread ID and the item to run
            SlugGenerationQueueInfo ItemToRun = SlugGenerationQueueInfoProvider.GetSlugGenerationQueues()
                .WhereEquals("SlugGenerationQueueRunning", 1)
                .WhereEquals("SlugGenerationQueueApplicationID", SystemHelper.ApplicationIdentifier)
                .FirstOrDefault();
            if (ItemToRun == null)
            {
                return;
            }

            // Update item with thread and times
            ItemToRun.SlugGenerationQueueThreadID = Thread.CurrentThread.ManagedThreadId;
            ItemToRun.SlugGenerationQueueStarted = DateTime.Now;
            ItemToRun.SetValue("SlugGenerationQueueErrors", null);
            ItemToRun.SetValue("SlugGenerationQueueEnded", null);
            SlugGenerationQueueInfoProvider.SetSlugGenerationQueueInfo(ItemToRun);

            // Get the NodeItem from the SlugGenerationQueueItem
            var serializer = new XmlSerializer(typeof(NodeItem));
            NodeItem QueueItem;
            using (TextReader reader = new StringReader(ItemToRun.SlugGenerationQueueNodeItem))
            {
                QueueItem = (NodeItem)serializer.Deserialize(reader);
            }
            // Build and Save Items
            try
            {
                QueueItem.BuildChildren();
                QueueItem.SaveChanges();

                // If completed successfully, delete the item
                ItemToRun.Delete();

                // Now that we are 'finished' call the Check again to processes next item.
                CheckUrlSlugGenerationQueue();
            }
            catch (Exception ex)
            {
                ItemToRun.SlugGenerationQueueErrors = EventLogProvider.GetExceptionLogMessage(ex);
                ItemToRun.SlugGenerationQueueRunning = false;
                ItemToRun.SlugGenerationQueueEnded = DateTime.Now;
                SlugGenerationQueueInfoProvider.SetSlugGenerationQueueInfo(ItemToRun);

                // Commit transaction so next check will see this change
                CommitTransaction(true);

                // Now that we are 'finished' call the Check again to processes next item.
                CheckUrlSlugGenerationQueue();
            }
            //EventLogProvider.LogInformation("DynamicRouteTesting", "AsyncBuildComplete", eventDescription: DateTime.Now.ToString()+" "+DateTime.Now.Millisecond.ToString());
        }

        /// <summary>
        /// Runs the Generation on the given Slug Generation Queue, runs regardless of whether or not any other queues are running.
        /// </summary>
        /// <param name="SlugGenerationQueueID"></param>
        public static void RunSlugGenerationQueueItem(int SlugGenerationQueueID)
        {
            SlugGenerationQueueInfo ItemToRun = SlugGenerationQueueInfoProvider.GetSlugGenerationQueues()
                .WhereEquals("SlugGenerationQueueID", SlugGenerationQueueID)
                .FirstOrDefault();
            if (ItemToRun == null)
            {
                return;
            }

            // Update item with thread and times
            ItemToRun.SlugGenerationQueueThreadID = Thread.CurrentThread.ManagedThreadId;
            ItemToRun.SlugGenerationQueueStarted = DateTime.Now;
            ItemToRun.SlugGenerationQueueRunning = true;
            ItemToRun.SlugGenerationQueueApplicationID = SystemHelper.ApplicationIdentifier;
            ItemToRun.SetValue("SlugGenerationQueueErrors", null);
            ItemToRun.SetValue("SlugGenerationQueueEnded", null);
            SlugGenerationQueueInfoProvider.SetSlugGenerationQueueInfo(ItemToRun);

            // Get the NodeItem from the SlugGenerationQueueItem
            var serializer = new XmlSerializer(typeof(NodeItem));
            NodeItem QueueItem;
            using (TextReader reader = new StringReader(ItemToRun.SlugGenerationQueueNodeItem))
            {
                QueueItem = (NodeItem)serializer.Deserialize(reader);
            }

            // Build and Save Items
            try
            {
                QueueItem.BuildChildren();
                if (ErrorOnConflict() && QueueItem.ConflictsExist())
                {
                    ItemToRun.SlugGenerationQueueErrors = $"The Following Conflicts were found:\n\r{string.Join("\n\r", QueueItem.GetConflictItems())}\n\rPlease Correct and re-run queue item.";
                    ItemToRun.SlugGenerationQueueEnded = DateTime.Now;
                    ItemToRun.SetValue("SlugGenerationQueueThreadID", null);
                    ItemToRun.SetValue("SlugGenerationQueueApplicationID", null);
                    ItemToRun.SlugGenerationQueueRunning = false;
                    SlugGenerationQueueInfoProvider.SetSlugGenerationQueueInfo(ItemToRun);
                    return;
                }
                else
                {
                    QueueItem.SaveChanges();
                    // If completed successfully, delete the item
                    ItemToRun.Delete();
                }
            }
            catch (Exception ex)
            {
                ItemToRun.SlugGenerationQueueErrors = EventLogProvider.GetExceptionLogMessage(ex);
                ItemToRun.SlugGenerationQueueRunning = false;
                ItemToRun.SlugGenerationQueueEnded = DateTime.Now;
                SlugGenerationQueueInfoProvider.SetSlugGenerationQueueInfo(ItemToRun);
            }

            // Now that we are 'finished' call the Check again to processes next item.
            CheckUrlSlugGenerationQueue();
        }

        /// <summary>
        /// Commits the current transaction so the previous item's content is in the database to be called upon, avoids null lookups on related items.
        /// <paramref name="StartNewTransaction">Start a new transaction after this one is committed.</paramref>
        /// </summary>
        public static void CommitTransaction(bool StartNewTransaction = true)
        {
            // Commits any previous action to database since this may call on items from those methods.
            if (ConnectionContext.CurrentScopeConnection.IsTransaction())
            {
                ConnectionContext.CurrentScopeConnection.CommitTransaction();
                if (StartNewTransaction)
                {
                    ConnectionContext.CurrentScopeConnection.BeginTransaction();
                }
            }

        }


        /// <summary>
        /// If true, then other queued Url Slug Generation tasks will be executed even if one has an error.  Risk is that you could end up with a change waiting to processes that was altered by a later task, thus reverting the true value possibly.
        /// </summary>
        /// <returns></returns>
        private static bool SkipErroredGenerations()
        {
            return ValidationHelper.GetString(SettingsKeyInfoProvider.GetValue("QueueErrorBehavior", new SiteInfoIdentifier(SiteContextSafe().SiteID)), "skip").ToLower() == "skip";
        }

        /// <summary>
        /// Helper that Serializes an object into a string
        /// </summary>
        /// <typeparam name="T">The Type</typeparam>
        /// <param name="toSerialize">The object to Serialize</param>
        /// <returns>the string representing the XML serialization of the object</returns>
        private static string SerializeObject<T>(this T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }

        #endregion

        #region "Cached Helpers"

        /// <summary>
        /// If the Site name is empty or null, returns the current site context's site name
        /// </summary>
        /// <param name="SiteName">The Site Name</param>
        /// <returns>The Site Name if given, or the current site name</returns>
        private static string GetSiteName(string SiteName)
        {
            return !string.IsNullOrWhiteSpace(SiteName) ? SiteName : SiteContextSafe().SiteName;
        }

        /// <summary>
        /// If an exception should be thrown when a conflict is detected.  Default is false.
        /// </summary>
        /// <returns></returns>
        public static bool ErrorOnConflict()
        {
            return ValidationHelper.GetString(SettingsKeyInfoProvider.GetValue("UrlSlugConflictBehavior", new SiteInfoIdentifier(SiteContextSafe().SiteID)), "append").Equals("cancel", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Gets the Class Names that shouldn't use the Url Slugs for their relative/absolute url.  These classes will also be ignored in finding a page through the DynamicRouteHelper.GetPage()
        /// </summary>
        /// <returns>List of the class names, lower cased</returns>
        public static List<string> UrlSlugExcludedClassNames()
        {
            return CacheHelper.Cache(cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency("cms.settingskey|byname|urlslugexcludedclasses");
                }
                return ValidationHelper.GetString(SettingsKeyInfoProvider.GetValue("UrlSlugExcludedClasses", new SiteInfoIdentifier(SiteContextSafe().SiteID)), "").ToLower().Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();

            }, new CacheSettings(1440, "UrlSlugExcludedClassNames"));
        }

        /// <summary>
        /// If Url Conflicts should have an -(#) appended to resolve conflicts
        /// </summary>
        /// <returns>True if the conflicting url should have an appendage added to it.</returns>
        public static bool AppendPostFixOnConflict()
        {
            return ValidationHelper.GetString(SettingsKeyInfoProvider.GetValue("UrlSlugConflictBehavior", new SiteInfoIdentifier(SiteContextSafe().SiteID)), "append").Equals("append", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Cached helper to gets the SiteInfo based on the SiteID
        /// </summary>
        /// <param name="SiteID">The SiteID</param>
        /// <returns>The SiteINfo object</returns>
        public static SiteInfo GetSite(int SiteID)
        {
            return CacheHelper.Cache(cs =>
            {
                var Site = SiteInfoProvider.GetSiteInfo(SiteID);
                if (cs.Cached)
                {
                }
                cs.CacheDependency = CacheHelper.GetCacheDependency("cms.site|byid|" + SiteID);
                return Site;
            }, new CacheSettings(1440, "GetSiteByID", SiteID));
        }

        /// <summary>
        /// Cached helper to gets the DataClassInfo object
        /// </summary>
        /// <param name="ClassName">Class Name</param>
        /// <returns>The DataClassINfo of that class</returns>
        public static DataClassInfo GetClass(string ClassName)
        {
            return CacheHelper.Cache(cs =>
            {
                var Class = DataClassInfoProvider.GetDataClassInfo(ClassName);
                cs.CacheDependency = CacheHelper.GetCacheDependency("cms.class|byname|" + ClassName);
                return Class;
            }, new CacheSettings(1440, "GetClassByName", ClassName));
        }

        /// <summary>
        /// Cached helper to gets the DataClassInfo object
        /// </summary>
        /// <param name="ClassName">Class Name</param>
        /// <returns>The DataClassINfo of that class</returns>
        public static DataClassInfo GetClass(int ClassID)
        {
            return CacheHelper.Cache(cs =>
            {
                var Class = DataClassInfoProvider.GetDataClassInfo(ClassID);
                cs.CacheDependency = CacheHelper.GetCacheDependency("cms.class|byid|" + ClassID);
                return Class;
            }, new CacheSettings(1440, "GetClassByID", ClassID));
        }



        /// <summary>
        /// Cached helper to get the CultureInfo object for the given Culture code
        /// </summary>
        /// <param name="CultureCode">the Culture code (ex en-US)</param>
        /// <returns>The Culture Info</returns>
        public static CultureInfo GetCulture(string CultureCode)
        {
            return CacheHelper.Cache<CultureInfo>(cs =>
            {
                var Culture = CultureInfoProvider.GetCultureInfo(CultureCode);
                cs.CacheDependency = CacheHelper.GetCacheDependency("cms.culture|byname|" + CultureCode);
                return Culture;
            }, new CacheSettings(1440, "GetCultureByName", CultureCode));
        }

        #endregion

        #region "Get Page Url Helpers"

        /// <summary>
        /// Gets the Page's Url Slug based on the given DocumentID and it's Culture.
        /// </summary>
        /// <param name="DocumentID">The Document ID</param>
        /// <returns></returns>
        public static string GetPageUrl(int DocumentID)
        {
            // Convert DocumentID to NodeID + Cultulre
            TreeNode Page = DocumentHelper.GetDocuments().WhereEquals("DocumentID", DocumentID).Columns("NodeID, DocumentCulture, NodeSiteID").FirstOrDefault();
            if (Page != null)
            {
                return GetPageUrl(Page.NodeID, Page.DocumentCulture, SiteInfoProvider.GetSiteName(Page.NodeSiteID));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the Page's Url Slug based on the given DocumentGuid and it's Culture.
        /// </summary>
        /// <param name="DocumentGuid">The Document Guid</param>
        /// <returns>The UrlSlug (with ~ prepended) or Null if page not found.</returns>
        public static string GetPageUrl(Guid DocumentGuid)
        {
            // Convert DocumentGuid to NodeID + Cultulre
            TreeNode Page = DocumentHelper.GetDocuments().WhereEquals("DocumentGuid", DocumentGuid).Columns("NodeID, DocumentCulture, NodeSiteID").FirstOrDefault();
            if (Page != null)
            {
                return GetPageUrl(Page.NodeID, Page.DocumentCulture, SiteInfoProvider.GetSiteName(Page.NodeSiteID));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the Page's Url Slug based on the given NodeAliasPath, Culture and SiteName.  If Culture not found, then will prioritize the Site's Default Culture, then Cultures by alphabetical order.
        /// </summary>
        /// <param name="NodeAliasPath">The Node alias path you wish to select</param>
        /// <param name="DocumentCulture">The Document Culture, if not provided will use default Site's Culture.</param>
        /// <param name="SiteName">The Site Name, if not provided then the Current Site's name is used.</param>
        /// <returns>The UrlSlug (with ~ prepended) or Null if page not found.</returns>
        public static string GetPageUrl(string NodeAliasPath, string DocumentCulture = null, string SiteName = null)
        {
            if (string.IsNullOrWhiteSpace(SiteName))
            {
                SiteName = SiteContextSafe().SiteName;
            }
            TreeNode Page = null;
            if (string.IsNullOrWhiteSpace(DocumentCulture))
            {
                Page = DocumentHelper.GetDocuments()
                    .OnSite(SiteName)
                    .Path(NodeAliasPath)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .Columns("NodeID, DocumentCulture, NodeSiteID")
                    .FirstOrDefault();
            }
            else
            {
                Page = DocumentHelper.GetDocuments()
                    .OnSite(SiteName)
                    .Path(NodeAliasPath)
                    .Culture(DocumentCulture)
                    .CombineWithAnyCulture()
                    .Columns("NodeID, DocumentCulture, NodeSiteID")
                    .FirstOrDefault();
            }
            if (Page != null)
            {
                return GetPageUrl(Page.NodeID, Page.DocumentCulture, SiteInfoProvider.GetSiteName(Page.NodeSiteID));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the Page's Url Slug based on the given NodeGuid and Culture.  If Culture not found, then will prioritize the Site's Default Culture, then Cultures by alphabetical order.
        /// </summary>
        /// <param name="NodeGuid">The Node to find the Url Slug</param>
        /// <param name="DocumentCulture">The Document Culture, if not provided will use default Site's Culture.</param>
        /// <returns>The UrlSlug (with ~ prepended) or Null if page not found.</returns>
        public static string GetPageUrl(Guid NodeGuid, string DocumentCulture = null)
        {
            TreeNode Page = null;
            if (string.IsNullOrWhiteSpace(DocumentCulture))
            {
                Page = DocumentHelper.GetDocuments()
                    .WhereEquals("NodeGuid", NodeGuid)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .Columns("NodeID, DocumentCulture, NodeSiteID")
                    .FirstOrDefault();
            }
            else
            {
                Page = DocumentHelper.GetDocuments()
                    .WhereEquals("NodeGuid", NodeGuid)
                    .Culture(DocumentCulture)
                    .CombineWithAnyCulture()
                    .Columns("NodeID, DocumentCulture, NodeSiteID")
                    .FirstOrDefault();
            }
            if (Page != null)
            {
                return GetPageUrl(Page.NodeID, Page.DocumentCulture, SiteInfoProvider.GetSiteName(Page.NodeSiteID));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the Page's Url Slug based on the given NodeID and Culture.  If Culture not found, then will prioritize the Site's Default Culture, then Cultures by alphabetical order.
        /// </summary>
        /// <param name="NodeID">The NodeID</param>
        /// <param name="DocumentCulture">The Document Culture, if not provided will use default Site's Culture.</param>
        /// <param name="SiteName">The Site Name, if not provided then will query the NodeID to find it's site.</param>
        /// <returns>The UrlSlug (with ~ prepended) or Null if page not found.</returns>
        public static string GetPageUrl(int NodeID, string DocumentCulture = null, string SiteName = null)
        {
            if (SiteName == null)
            {
                TreeNode Page = DocumentHelper.GetDocuments().WhereEquals("NodeID", NodeID).Columns("NodeSiteID").FirstOrDefault();
                if (Page != null)
                {
                    SiteName = SiteInfoProvider.GetSiteName(Page.NodeSiteID);
                }
                else
                {
                    // No page found, so won't find a Url Slug either
                    return null;
                }
            }

            if (string.IsNullOrWhiteSpace(DocumentCulture))
            {
                DocumentCulture = CultureHelper.GetDefaultCultureCode(SiteName);
            }

            UrlSlugInfo UrlSlug = UrlSlugInfoProvider.GetUrlSlugs()
                .WhereEquals("UrlSlugNodeID", NodeID)
                .OrderBy($"case when UrlSlugCultureCode = '{SqlHelper.EscapeQuotes(DocumentCulture)}' then 0 else 1 end, case when UrlSlugCultureCode = '{CultureHelper.GetDefaultCultureCode(SiteName)}' then 0 else 1 end, UrlSlugCultureCode")
                .FirstOrDefault();

            return UrlSlug != null ? "~" + UrlSlug.UrlSlug : null;
        }

        #endregion
    }
}
