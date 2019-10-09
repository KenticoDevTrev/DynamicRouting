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

        /// <summary>
        /// Rebuilds the Url Slugs affected by a class Url Pattern Change.
        /// </summary>
        /// <param name="ClassName"></param>
        public static void ClassUrlPatternChanged(string ClassName)
        {
            DynamicRouteHelper.RebuildRoutesByClass(ClassName);
        }

        /// <summary>
        /// Build ParentNodes, and Parent's immediate children, build the children and update recursively only if changes detected.
        /// </summary>
        /// <param name="ParentNodeID"></param>
        public static void DocumentDeleted(int ParentNodeID)
        {
            DynamicRouteHelper.RebuildRoutesByNode(ParentNodeID);
        }

        /// <summary>
        /// Build both ParentNodes, and Parent's immediate children, build the children and update recursively only if changes detected.
        /// </summary>
        /// <param name="OldParentNodeID"></param>
        /// <param name="NewParentNodeID"></param>
        public static void DocumentMoved(int OldParentNodeID, int NewParentNodeID)
        {
            DynamicRouteHelper.RebuildRoutesByNode(OldParentNodeID);
            DynamicRouteHelper.RebuildRoutesByNode(NewParentNodeID);
        }

        /// <summary>
        /// Build ParentNode, and Parent's immediate children, build the children and update recursively only if changes detected.
        /// </summary>
        /// <param name="NodeID"></param>
        public static void DocumentInsertUpdated(int NodeID)
        {
            DynamicRouteHelper.RebuildRoutesByNode(NodeID);
        }

        /// <summary>
        /// Build Node (and upward), and build the children and update recursively only if changes detected.
        /// </summary>
        /// <param name="UrlSlugID"></param>
        public static void UrlSlugModified(int UrlSlugID)
        {
            // Convert UrlSlugID to NodeID
            int NodeID = UrlSlugInfoProvider.GetUrlSlugInfo(UrlSlugID).UrlSlugNodeID;
            DynamicRouteHelper.RebuildRoutesByNode(NodeID);
        }

    }
}
