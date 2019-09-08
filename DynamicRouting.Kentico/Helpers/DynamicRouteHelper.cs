using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.SiteProvider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicRouting
{
    /// <summary>
    /// Event helper, these should be called from Global events to help trigger the proper updates
    /// </summary>
    public static class DynamicRouteEventHelper
    {
        public static void CultureVariationSettingsChanged(string SiteName)
        {
            // Build all, update all
            DynamicRouteHelper.RebuildRoutesBySite(SiteName);
        }

        public static void SiteLanguageChanged(string SiteName)
        {
            // Build all, update all
            DynamicRouteHelper.RebuildRoutesBySite(SiteName);
        }

        public static void SiteDefaultLanguageChanged(string SiteName)
        {
            // Build all, update all
            DynamicRouteHelper.RebuildRoutesBySite(SiteName);
        }

        public static void ClassUrlPatternChanged(string ClassName)
        {
            DynamicRouteHelper.RebuildRoutesByClass(ClassName);
        }

        public static void DocumentDeleted(int ParentNodeID)
        {
            // Build ParentNodes, and Parent's immediate children, build the children and update recursively only if changes detected.
            DynamicRouteHelper.RebuildRoutesByNode(ParentNodeID);
        }

        public static void DocumentMoved(int OldParentNodeID, int NewParentNodeID)
        {
            // Build both ParentNodes, and Parent's immediate children, build the children and update recursively only if changes detected.
            DynamicRouteHelper.RebuildRoutesByNode(OldParentNodeID);
            DynamicRouteHelper.RebuildRoutesByNode(NewParentNodeID);
        }

        public static void DocumentInsertUpdated(int NodeID)
        {
            // Build ParentNode, and Parent's immediate children, build the children and update recursively only if changes detected.
            DynamicRouteHelper.RebuildRoutesByNode(NodeID);
        }

        public static void UrlSlugModified(int UrlSlugID)
        {
            // Build Node (and upward), and build the children and update recursively only if changes detected.
            // Convert UrlSlugID to NodeID
            int NodeID = UrlSlugInfoProvider.GetUrlSlugInfo(UrlSlugID).NodeID;
            DynamicRouteHelper.RebuildRoutesByNode(NodeID);
        }

    }



    /// <summary>
    /// Helper methods that execute the checks
    /// </summary>
    public static class DynamicRouteHelper
    {
        /// <summary>
        /// If the Site name is empty or null, returns the current site context's site name
        /// </summary>
        /// <param name="SiteName">The Site Name</param>
        /// <returns>The Site Name if given, or the current site name</returns>
        private static string GetSiteName(string SiteName)
        {
            return !string.IsNullOrWhiteSpace(SiteName) ? SiteName : SiteContext.CurrentSiteName;
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

                var Document = DocumentHelper.GetDocument(new NodeSelectionParameters()
                {
                    SelectSingleNode = true,
                    AliasPath = NodeAliasPath,
                    SiteName = SiteName
                }, new TreeProvider());

                // return if any siblings have a class NodeOrder exist
                return DocumentHelper.GetDocuments()
                    .WhereEquals("NodeParentID", Document.NodeParentID)
                    .WhereNotEquals("NodeID", Document.NodeID)
                    .WhereIn("NodeClassID", ClassIDs)
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
                var Document = DocumentHelper.GetDocument(new NodeSelectionParameters()
                {
                    SelectSingleNode = true,
                    AliasPath = NodeAliasPath,
                    SiteName = SiteName
                }, new TreeProvider());

                // return if any siblings have a class NodeOrder exist
                return DocumentHelper.GetDocuments()
                    .Path(NodeAliasPath, PathTypeEnum.Children)
                    .OnSite(new SiteInfoIdentifier(SiteName))
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

                var Document = DocumentHelper.GetDocument(new NodeSelectionParameters()
                {
                    SelectSingleNode = true,
                    AliasPath = NodeAliasPath,
                    SiteName = SiteName
                }, new TreeProvider());

                // return if any Children have a class NodeOrder exist
                return DocumentHelper.GetDocuments()
                    .WhereEquals("NodeParentID", Document.NodeID)
                    .WhereIn("NodeClassID", ClassIDs)
                    .Columns("NodeID")
                    .Count > 0;
            }
        }

        #endregion

        /// <summary>
        /// Get the Node Item Builder Settings based on the given NodeAliasPath, should be used for individual page updates as will limit what is checked based on the page.
        /// </summary>
        /// <param name="NodeAliasPath">The Node Alias Path</param>
        /// <param name="SiteName"></param>
        /// <param name="CheckingForUpdates"></param>
        /// <param name="CheckEntireTree"></param>
        /// <returns></returns>
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

                return new NodeItemBuilderSettings(Cultures, DefaultCulture, GenerateCultureVariationUrlSlugs, BaseMacroResolver, CheckingForUpdates, CheckEntireTree, BuildSiblings, BuildChildren, BuildDescendents);
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

                return new NodeItemBuilderSettings(Cultures, DefaultCulture, GenerateCultureVariationUrlSlugs, BaseMacroResolver, CheckingForUpdates, CheckEntireTree, BuildSiblings, BuildChildren, BuildDescendents);
            }, new CacheSettings(1440, "GetNodeItemBuilderSettings", SiteName, CheckingForUpdates, CheckEntireTree, BuildSiblings, BuildChildren, BuildDescendents));
        }

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
            // Get NodeItemBuilderSettings
            NodeItemBuilderSettings BuilderSettings = GetNodeItemBuilderSettings(SiteName, true, true, true, true, true);

            // Get root NodeID
            TreeNode RootNode = DocumentHelper.GetDocument(new NodeSelectionParameters()
            {
                AliasPath = "/",
                SiteName = SiteName
            }, new TreeProvider());

            // Rebuild NodeItem tree structure
            NodeItem RootNodeItem = new NodeItem(RootNode.NodeID, BuilderSettings);
            if (ErrorOnConflict())
            {
                if (RootNodeItem.ConflictsExist())
                {
                    throw new Exception("Conflict Exists, aborting save");
                }
            }
            // Save changes
            RootNodeItem.SaveChanges();
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
                // Get NodeItemBuilderSettings
                NodeItemBuilderSettings BuilderSettings = GetNodeItemBuilderSettings(SiteName, true, true, true, true, true);

                // Build all, gather nodes of any Node that is of this type of class, check for updates.
                List<int> NodeIDs = DocumentHelper.GetDocuments()
                    .WhereEquals("NodeClassID", Class.ClassID)
                    .OnSite(new SiteInfoIdentifier(SiteName))
                    .CombineWithDefaultCulture()
                    .Distinct()
                    .Columns("NodeID")
                    .OrderBy("NodeLevel, NodeOrder")
                    .Select(x => x.NodeID)
                    .ToList();

                // Check all parent nodes for changes
                foreach (int NodeID in NodeIDs)
                {
                    RebuildRoutesByNode(NodeID, BuilderSettings);
                }
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
        /// Rebuilds the Routes for the given Node, optionally allows for settings to be passed
        /// </summary>
        /// <param name="NodeID">The NodeID</param>
        /// <param name="Settings">The Node Item Build Settings, if null will create settings based on the Node itself.</param>
        private static void RebuildRoutesByNode(int NodeID, NodeItemBuilderSettings Settings = null)
        {
            // If settings are not set, then get settings based on the given Node
            if (Settings == null)
            {
                // Get Site from Node
                TreeNode Page = DocumentHelper.GetDocuments()
                    .WhereEquals("NodeID", NodeID)
                    .Columns("NodeSiteID")
                    .FirstOrDefault();

                // Get Settings based on the Page itself
                Settings = GetNodeItemBuilderSettings(Page.NodeAliasPath, GetSite(Page.NodeSiteID).SiteName, true, false);
            }

            // Build and save
            NodeItem GivenNodeItem = new NodeItem(NodeID, Settings);
            if(ErrorOnConflict())
            {
                if(GivenNodeItem.ConflictsExist())
                {
                    throw new Exception("Conflict Exists, aborting save");
                }
            }
            GivenNodeItem.SaveChanges();
        }

        #region "Cached Helpers"

        // TO DO, get from setting
        public static bool ErrorOnConflict()
        {
            return true;
        }

        public static SiteInfo GetSite(int SiteID)
        {
            return CacheHelper.Cache(cs =>
            {
                var Site = SiteInfoProvider.GetSiteInfo(SiteID);
                if(cs.Cached) {
                }
                cs.CacheDependency = CacheHelper.GetCacheDependency("cms.site|byid|" + SiteID);
                return Site;
            }, new CacheSettings(1440, "GetSiteByID", SiteID));
        }

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

    }
}
