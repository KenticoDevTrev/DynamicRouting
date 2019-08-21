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
    sealed class DynamicRouteEventHelper
    {
        public DynamicRouteEventHelper()
        {

        }
        public void SiteLanguageChanged()
        {
            // Build all, update all
        }

        public void SiteDefaultLanguageChanged()
        {
            // Build all, update all
        }

        public void ClassTypeChanged(string ClassName)
        {
            // build all, update all
        }

        public void ClassUrlPatternChanged(string ClassName)
        {
            // Build all, gather parent nodes of any Node that is of this type of class, update children recursively if changes detected.
        }

        public void DocumentDeleted(int ParentNodeID)
        {
            // Build ParentNodes (and upwards), and Parent's immediate children, build the children and update recursively only if changes detected.
        }

        public void DocumentMoved(int OldParentNodeID, int NewParentNodeID)
        {
            // Build both ParentNodes (and upwards), and Parent's immediate children, build the children and update recursively only if changes detected.
        }

        public void DocumentInsertCopy(int NodeID)
        {
            // Build ParentNode (and upwards), and Parent's immediate children, build the children and update recursively only if changes detected.
        }

        public void UrlSlugModified(int UrlSlugID)
        {
            // Build Node (and upward), and build the children and update recursively only if changes detected.
        }

    }

    public static class DynamicRouteHelper
    {

        /// <summary>
        /// Checks The Node and it's siblings
        /// </summary>
        /// <param name="NodeID"></param>
        public static void CheckRoutes(int NodeID, bool CheckParent = false)
        {

        }

        public static void RebuildRoutes()
        {
            foreach(SiteInfo Site in SiteInfoProvider.GetSites())
            {
                RebuildRoutesBySite(Site.SiteName);
            }
        }

        public static void RebuildRoutesBySite(string SiteName)
        {
            // Loop through Cultures and start rebuilding pages
            SiteInfo Site = SiteInfoProvider.GetSiteInfo(SiteName);
            string DefaultCulture = SettingsKeyInfoProvider.GetValue("CMSDefaultCultureCode", new SiteInfoIdentifier(SiteName));

            // TODO Get from settings
            bool GenerateCultureVariationUrlSlugs = false;

            Dictionary<int, NodeItem> NodeIDToItem = new Dictionary<int, NodeItem>();
            List<NodeItem> RootNodes = new List<NodeItem>();
            
            // Gather all the Node data for that site, this is in NodeLevel NodeOrder listing so will add parents before their children.
            foreach(DataRow NodeData in ConnectionHelper.ExecuteQuery("DynamicRouting.UrlSlug.GetSiteNodes", null, where: "NodeSiteID = " + Site.SiteID.ToString()).Tables[0].Rows)
            {
                // See if parent exists, assign it
                int NodeParentID = ValidationHelper.GetInteger(NodeData["NodeParentNodeID"], 0);
                NodeItem nodeItem = new NodeItem(NodeIDToItem.ContainsKey(NodeParentID) ? NodeIDToItem[NodeParentID] : null, NodeData);
                NodeIDToItem.Add(nodeItem.NodeID, nodeItem);

                // If first level, add to the Root Nodes listing
                if(ValidationHelper.GetInteger(NodeData["NodeLevel"], 1) == 1)
                {
                    RootNodes.Add(nodeItem);
                }
            }

            var BaseMacroResolver = MacroResolver.GetInstance(true);
            BaseMacroResolver.AddAnonymousSourceData(new object[] { Site });

            // Now build URL slugs for the default language always.
            List<string> Cultures = CultureSiteInfoProvider.GetSiteCultureCodes(SiteName);
            foreach (NodeItem nodeItem in RootNodes)
            {
                nodeItem.BuildUrlSlugs(Cultures, DefaultCulture, GenerateCultureVariationUrlSlugs, BaseMacroResolver);
            }
        }

        public static void RebuildRoutesByClass(string ClassName)
        {

        }

        public static void RebuildRoutesByNode(int NodeID)
        {

        }

        public static void RebuilRouteByDocument(int DocumentID)
        {

        }

        public static string GetClassUrlPattern(string ClassName)
        {
            return CacheHelper.Cache<string>(cs =>
            {
                var Class = DataClassInfoProvider.GetDataClassInfo(ClassName);
                cs.CacheDependency = CacheHelper.GetCacheDependency("cms.class|byname|" + ClassName);
                return Class.ClassURLPattern;
            }, new CacheSettings(1440, "GetClassPatternByName", ClassName));
        }

        public static CultureInfo GetCulture(string CultureCode)
        {
            return CacheHelper.Cache<CultureInfo>(cs =>
            {
                var Culture = CultureInfoProvider.GetCultureInfo(CultureCode);
                cs.CacheDependency = CacheHelper.GetCacheDependency("cms.culture|byname|" + CultureCode);
                return Culture;
            }, new CacheSettings(1440, "GetCultureByName", CultureCode));
        }
    }
}
