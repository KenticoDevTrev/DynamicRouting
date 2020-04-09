using CMS.Base;
using CMS.SiteProvider;
using DynamicRouting.Implementations;
using DynamicRouting.Kentico.MVC;
using System;
using System.Collections.Generic;

namespace DynamicRouting
{
    [Obsolete("It is recommended you use the IDynamicRouteHelper interface on your constructor instead.  You may need to add to your AutoFac ContainerBuilder 'builder.RegisterType(typeof(BaseDynamicRouteHelper)).As(typeof(IDynamicRouteHelper));'")]
    public static class DynamicRouteHelper
    {
        /// <summary>
        /// Helper in case SiteContext is null for OWIN requests
        /// </summary>
        /// <returns></returns>
        public static SiteInfo SiteContextSafe()
        {
            return new BaseDynamicRouteHelper().SiteContextSafe();
        }

        /// <summary>
        /// Gets the CMS Page, returning the culture variation that either matches the given culture or the Url's culture, or the default site culture if not found.  You can use the DynamicRoutingEvents to hook in and apply your logic.        /// </summary>
        /// <param name="Url">The Url (part after the domain), if empty will use the Current Request</param>
        /// <param name="Culture">The Culture, not needed if the Url contains the culture that the UrlSlug has as part of it's generation.</param>
        /// <param name="SiteName">The Site Name, defaults to current site.</param>
        /// <param name="Columns">List of columns you wish to include in the data returned.</param>
        /// <param name="AddPageToCacheDependency">If true, the found page will have it's DocumentID added to the request's Output Cache Dependency</param>
        /// <returns>The Page that matches the Url, for the given or matching culture (or default culture if one isn't found).</returns>
        public static ITreeNode GetPage(string Url = "", string Culture = "", string SiteName = "", IEnumerable<string> Columns = null, bool AddPageToCacheDependency = true)
        {
            return new BaseDynamicRouteHelper().GetPage(Url, Culture, SiteName, Columns, AddPageToCacheDependency);
        }

        /// <summary>
        /// Gets the CMS Page, returning the culture variation that either matches the given culture or the Url's culture, or the default site culture if not found.  You can use the DynamicRoutingEvents to hook in and apply your logic.
        /// </summary>
        /// <param name="Url">The Url (part after the domain), if empty will use the Current Request</param>
        /// <param name="Culture">The Culture, not needed if the Url contains the culture that the UrlSlug has as part of it's generation.</param>
        /// <param name="SiteName">The Site Name, defaults to current site.</param>
        /// <param name="Columns">List of columns you wish to include in the data returned.</param>
        /// <param name="AddPageToCacheDependency">If true, the found page will have it's DocumentID added to the request's Output Cache Dependency</param>
        /// <returns>The Page that matches the Url, for the given or matching culture (or default culture if one isn't found).</returns>
        public static T GetPage<T>(string Url = "", string Culture = "", string SiteName = "", IEnumerable<string> Columns = null, bool AddPageToCacheDependency = true) where T : ITreeNode
        {
            return GetPage<T>(Url, Culture, SiteName, Columns, AddPageToCacheDependency);
        }

        /// <summary>
        /// Gets the Page's Url based on the given DocumentID and it's Culture.
        /// </summary>
        /// <param name="DocumentID">The Document ID</param>
        /// <returns>The Url (with ~ prepended) or Null if page not found.</returns>
        public static string GetPageUrl(int DocumentID)
        {
            return new BaseDynamicRouteHelper().GetPageUrl(DocumentID);
        }

        /// <summary>
        /// Gets the Page's Url based on the given DocumentGuid and it's Culture.
        /// </summary>
        /// <param name="DocumentGuid">The Document Guid</param>
        /// <returns>The Url (with ~ prepended) or Null if page not found.</returns>
        public static string GetPageUrl(Guid DocumentGuid)
        {
            return new BaseDynamicRouteHelper().GetPageUrl(DocumentGuid);
        }

        /// <summary>
        /// Gets the Page's Url based on the given NodeAliasPath, Culture and SiteName.  If Culture not found, then will prioritize the Site's Default Culture, then Cultures by alphabetical order.
        /// </summary>
        /// <param name="NodeAliasPath">The Node alias path you wish to select</param>
        /// <param name="DocumentCulture">The Document Culture, if not provided will use default Site's Culture.</param>
        /// <param name="SiteName">The Site Name, if not provided then the Current Site's name is used.</param>
        /// <returns>The Url (with ~ prepended) or Null if page not found.</returns>
        public static string GetPageUrl(string NodeAliasPath, string DocumentCulture = null, string SiteName = null)
        {
            return new BaseDynamicRouteHelper().GetPageUrl(NodeAliasPath, DocumentCulture, SiteName);
        }

        /// <summary>
        /// Gets the Page's Url based on the given NodeGuid and Culture.  If Culture not found, then will prioritize the Site's Default Culture, then Cultures by alphabetical order.
        /// </summary>
        /// <param name="NodeGuid">The Node to find the Url Slug</param>
        /// <param name="DocumentCulture">The Document Culture, if not provided will use default Site's Culture.</param>
        /// <returns>The Url (with ~ prepended) or Null if page not found.</returns>
        public static string GetPageUrl(Guid NodeGuid, string DocumentCulture = null)
        {
            return new BaseDynamicRouteHelper().GetPageUrl(NodeGuid, DocumentCulture);
        }

        /// <summary>
        /// Gets the Page's Url based on the given NodeID and Culture.  If Culture not found, then will prioritize the Site's Default Culture, then Cultures by alphabetical order.
        /// </summary>
        /// <param name="NodeID">The NodeID</param>
        /// <param name="DocumentCulture">The Document Culture, if not provided will use default Site's Culture.</param>
        /// <param name="SiteName">The Site Name, if not provided then will query the NodeID to find it's site.</param>
        /// <returns>The Url (with ~ prepended) or Null if page not found.</returns>
        public static string GetPageUrl(int NodeID, string DocumentCulture = null, string SiteName = null)
        {
            return new BaseDynamicRouteHelper().GetPageUrl(NodeID, DocumentCulture, SiteName);
        }

        /// <summary>
        /// Gets the Route Configuration based on The node's Class Name.  
        /// </summary>
        /// <param name="node">The ITreeNode object</param>
        /// <returns>The Route Configuration, empty DynamicRouteconfiguration if not found</returns>
        public static DynamicRouteConfiguration GetRouteConfiguration(ITreeNode node)
        {
            return new BaseDynamicRouteHelper().GetRouteConfiguration(node);
        }
    }
}
