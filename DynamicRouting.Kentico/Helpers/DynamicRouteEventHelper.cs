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
}
