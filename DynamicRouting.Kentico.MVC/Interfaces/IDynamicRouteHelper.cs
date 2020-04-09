using CMS.Base;
using DynamicRouting.Kentico.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicRouting.Interfaces
{
    public interface IDynamicRouteHelper
    {
        /// <summary>
        /// Gets the CMS Page using Dynamic Routing, returning the culture variation that either matches the given culture or the Slug's culture, or the default site culture if not found.
        /// </summary>
        /// <param name="Url">The Url (part after the domain), if empty will use the Current Request</param>
        /// <param name="Culture">The Culture, not needed if the Url contains the culture that the UrlSlug has as part of it's generation.</param>
        /// <param name="SiteName">The Site Name, defaults to current site.</param>
        /// <param name="Columns">List of columns you wish to include in the data returned.</param>
        /// <param name="AddPageToCacheDependency">If true, the found page will have it's DocumentID added to the request's Output Cache Dependency</param>
        /// <returns>The Page that matches the Url Slug, for the given or matching culture (or default culture if one isn't found).</returns>
        ITreeNode GetPage(string Url = "", string Culture = "", string SiteName = "", IEnumerable<string> Columns = null, bool AddPageToCacheDependency = true);

        /// <summary>
        /// Gets the CMS Page using Dynamic Routing, returning the culture variation that either matches the given culture or the Slug's culture, or the default site culture if not found.
        /// </summary>
        /// <param name="Url">The Url (part after the domain), if empty will use the Current Request</param>
        /// <param name="Culture">The Culture, not needed if the Url contains the culture that the UrlSlug has as part of it's generation.</param>
        /// <param name="SiteName">The Site Name, defaults to current site.</param>
        /// <param name="Columns">List of columns you wish to include in the data returned.</param>
        /// <param name="AddPageToCacheDependency">If true, the found page will have it's DocumentID added to the request's Output Cache Dependency</param>
        /// <returns>The Page that matches the Url Slug, for the given or matching culture (or default culture if one isn't found).</returns>
        T GetPage<T>(string Url = "", string Culture = "", string SiteName = "", IEnumerable<string> Columns = null, bool AddPageToCacheDependency = true) where T : ITreeNode;
       

        /// <summary>
        /// Gets the Page's Url Slug based on the given DocumentID and it's Culture.
        /// </summary>
        /// <param name="DocumentID">The Document ID</param>
        /// <returns></returns>
        string GetPageUrl(int DocumentID);


        /// <summary>
        /// Gets the Page's Url Slug based on the given DocumentGuid and it's Culture.
        /// </summary>
        /// <param name="DocumentGuid">The Document Guid</param>
        /// <returns>The UrlSlug (with ~ prepended) or Null if page not found.</returns>
        string GetPageUrl(Guid DocumentGuid);


        /// <summary>
        /// Gets the Page's Url Slug based on the given NodeAliasPath, Culture and SiteName.  If Culture not found, then will prioritize the Site's Default Culture, then Cultures by alphabetical order.
        /// </summary>
        /// <param name="NodeAliasPath">The Node alias path you wish to select</param>
        /// <param name="DocumentCulture">The Document Culture, if not provided will use default Site's Culture.</param>
        /// <param name="SiteName">The Site Name, if not provided then the Current Site's name is used.</param>
        /// <returns>The UrlSlug (with ~ prepended) or Null if page not found.</returns>
        string GetPageUrl(string NodeAliasPath, string DocumentCulture = null, string SiteName = null);


        /// <summary>
        /// Gets the Page's Url Slug based on the given NodeGuid and Culture.  If Culture not found, then will prioritize the Site's Default Culture, then Cultures by alphabetical order.
        /// </summary>
        /// <param name="NodeGuid">The Node to find the Url Slug</param>
        /// <param name="DocumentCulture">The Document Culture, if not provided will use default Site's Culture.</param>
        /// <returns>The UrlSlug (with ~ prepended) or Null if page not found.</returns>
        string GetPageUrl(Guid NodeGuid, string DocumentCulture = null);


        /// <summary>
        /// Gets the Page's Url Slug based on the given NodeID and Culture.  If Culture not found, then will prioritize the Site's Default Culture, then Cultures by alphabetical order.
        /// </summary>
        /// <param name="NodeID">The NodeID</param>
        /// <param name="DocumentCulture">The Document Culture, if not provided will use default Site's Culture.</param>
        /// <param name="SiteName">The Site Name, if not provided then will query the NodeID to find it's site.</param>
        /// <returns>The UrlSlug (with ~ prepended) or Null if page not found.</returns>
        string GetPageUrl(int NodeID, string DocumentCulture = null, string SiteName = null);


        /// <summary>
        /// Gets the Route Configuration based on The node's Class Name.  
        /// </summary>
        /// <param name="node">The ITreeNode object</param>
        /// <returns>The Route Configuration, empty DynamicRouteconfiguration if not found</returns>
        DynamicRouteConfiguration GetRouteConfiguration(ITreeNode node);
       
    }
}
