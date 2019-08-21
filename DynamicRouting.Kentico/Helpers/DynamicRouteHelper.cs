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
        /// Check if any pages of children of this node's parent have a class with Url Pattern contains NodeOrder, if so then the siblings of a changed node need to have its siblings built (start with parent)
        /// </summary>
        /// <returns></returns>
        public static bool CheckSiblings(string NodeAliasPath = "")
        {
            throw new NotImplementedException();
            string ParentNodeAliasPath = GetParentNodeAliasPath(NodeAliasPath); // Build off NodeAliaPath
            return false;
        }

        /// <summary>
        /// Check if any child Url Pattern contains NodeLevel, NodeParentID, NodeAliasPath, DocumentNamePath if so then must check all children as there could be one of these page types down the tree that is modified.
        /// </summary>
        /// <returns></returns>
        public static bool CheckAllChildren(string NodeAliasPath = "")
        {
            throw new NotImplementedException();
            if (CheckSiblings(NodeAliasPath))
            {
                NodeAliasPath = GetParentNodeAliasPath(NodeAliasPath);
            }
            return false;
        }

        /// <summary>
        /// Check if any Url pattern contains ParentUrl
        /// </summary>
        public static bool CheckChildren(string NodeAliasPath = "")
        {
            throw new NotImplementedException();
            if (!CheckAllChildren(NodeAliasPath))
            {
                return false;
            } else
            {
                // Check if any children have ParentUrl in url pattern
                return false;
            }
        }

        /// <summary>
        /// Trims the last section of the NodeAliasPath to get it's parent's path.
        /// </summary>
        /// <param name="NodeAliasPath">The Node Alias Path</param>
        /// <returns>It's parent</returns>
        public static string GetParentNodeAliasPath(string NodeAliasPath)
        {
            throw new NotImplementedException();
            return "";
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
            
            
            var BaseMacroResolver = MacroResolver.GetInstance(true);
            BaseMacroResolver.AddAnonymousSourceData(new object[] { Site });

            // Now build URL slugs for the default language always.
            List<string> Cultures = CultureSiteInfoProvider.GetSiteCultureCodes(SiteName);

            // BUILD HERE
            throw new NotImplementedException();

        }

        public static void RebuildRoutesByClass(string ClassName)
        {
            throw new NotImplementedException();
        }

        public static void RebuildRoutesByNode(int NodeID)
        {
            throw new NotImplementedException();
        }

        public static void RebuilRouteByDocument(int DocumentID)
        {
            throw new NotImplementedException();
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
