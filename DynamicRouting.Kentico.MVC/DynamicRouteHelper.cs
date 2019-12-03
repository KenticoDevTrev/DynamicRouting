using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.SiteProvider;
using DynamicRouting.Helpers;
using DynamicRouting.Kentico.MVC;
using Kentico.Content.Web.Mvc;
using Kentico.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;

namespace DynamicRouting
{
    public static class DynamicRouteHelper
    {
        /// <summary>
        /// Gets the CMS Page using Dynamic Routing, returning the culture variation that either matches the given culture or the Slug's culture, or the default site culture if not found.
        /// </summary>
        /// <param name="Url">The Url (part after the domain), if empty will use the Current Request</param>
        /// <param name="Culture">The Culture, not needed if the Url contains the culture that the UrlSlug has as part of it's generation.</param>
        /// <param name="SiteName">The Site Name, defaults to current site.</param>
        /// <param name="Columns">List of columns you wish to include in the data returned.</param>
        /// <returns>The Page that matches the Url Slug, for the given or matching culture (or default culture if one isn't found).</returns>
        public static ITreeNode GetPage(string Url = "", string Culture = "", string SiteName = "", IEnumerable<string> Columns = null)
        {
            // Load defaults
            SiteName = (!string.IsNullOrWhiteSpace(SiteName) ? SiteName : DynamicRouteInternalHelper.SiteContextSafe().SiteName);
            string DefaultCulture = DynamicRouteInternalHelper.SiteContextSafe().DefaultVisitorCulture;
            
            if (string.IsNullOrWhiteSpace(Url))
            {
                Url = EnvironmentHelper.GetUrl(HttpContext.Current.Request.Url.AbsolutePath, HttpContext.Current.Request.ApplicationPath, SiteName);
            }

            // Handle Preview, during Route Config the Preview isn't available and isn't really needed, so ignore the thrown exception
            bool PreviewEnabled = false;
            try
            {
                PreviewEnabled = HttpContext.Current.Kentico().Preview().Enabled;
            }
            catch (InvalidOperationException ex) { }

            GetCultureEventArgs CultureArgs = new GetCultureEventArgs()
            {
                DefaultCulture = DefaultCulture,
                SiteName = SiteName,
                Request = HttpContext.Current.Request,
                PreviewEnabled = PreviewEnabled
            };

            using (var DynamicRoutingGetCultureTaskHandler = DynamicRoutingEvents.GetCulture.StartEvent(CultureArgs))
            {

                // If Preview is enabled, use the Kentico Preview CultureName
                if (PreviewEnabled && string.IsNullOrWhiteSpace(Culture))
                {
                    try
                    {
                        CultureArgs.Culture = HttpContext.Current.Kentico().Preview().CultureName;
                    }
                    catch (Exception) { }
                }

                // If culture still not set, use the LocalizationContext.CurrentCulture
                if (string.IsNullOrWhiteSpace(Culture))
                {
                    try
                    {
                        CultureArgs.Culture = LocalizationContext.CurrentCulture.CultureName;
                    }
                    catch (Exception) { }
                }

                // If that fails then use the System.Globalization.CultureInfo
                if (string.IsNullOrWhiteSpace(Culture))
                {
                    try
                    {
                        CultureArgs.Culture = System.Globalization.CultureInfo.CurrentCulture.Name;
                    }
                    catch (Exception) { }
                }

                DynamicRoutingGetCultureTaskHandler.FinishEvent();
            }

            // set the culture
            Culture = CultureArgs.Culture;

            // Convert Columns to 
            string ColumnsVal = Columns != null ? string.Join(",", Columns.Distinct()) : "*";

            // Create GetPageEventArgs Event ARgs
            GetPageEventArgs Args = new GetPageEventArgs()
            {
                RelativeUrl = Url,
                Culture = Culture,
                DefaultCulture = DefaultCulture,
                SiteName = SiteName,
                PreviewEnabled = PreviewEnabled,
                ColumnsVal = ColumnsVal,
                Request = HttpContext.Current.Request
            };

            // Run any GetPage Event hooks which allow the users to set the Found Page
            ITreeNode FoundPage = null;
            using (var DynamicRoutingGetPageTaskHandler = DynamicRoutingEvents.GetPage.StartEvent(Args))
            {
                if (Args.FoundPage == null)
                {
                    try
                    {
                        Args.FoundPage = CacheHelper.Cache<TreeNode>(cs =>
                        {
                            // Using custom query as Kentico's API was not properly handling a Join and where.
                            DataTable NodeTable = ConnectionHelper.ExecuteQuery("DynamicRouting.UrlSlug.GetDocumentsByUrlSlug", new QueryDataParameters()
                            {
                    {"@Url", Url },
                    {"@Culture", Culture },
                    {"@DefaultCulture", DefaultCulture },
                    {"@SiteName", SiteName }
                            }, topN: 1, columns: "DocumentID, ClassName").Tables[0];
                            if (NodeTable.Rows.Count > 0)
                            {
                                int DocumentID = ValidationHelper.GetInteger(NodeTable.Rows[0]["DocumentID"], 0);
                                string ClassName = ValidationHelper.GetString(NodeTable.Rows[0]["ClassName"], "");

                                DocumentQuery Query = DocumentHelper.GetDocuments(ClassName)
                                        .WhereEquals("DocumentID", DocumentID)
                                        .CombineWithAnyCulture();

                                // Handle Columns
                                if (!string.IsNullOrWhiteSpace(ColumnsVal))
                                {
                                    Query.Columns(ColumnsVal);
                                }

                                // Handle Preview
                                if (PreviewEnabled)
                                {
                                    Query.LatestVersion(true)
                                      .Published(false);
                                }
                                else
                                {
                                    Query.PublishedVersion(true);
                                }

                                TreeNode Page = Query.FirstOrDefault();

                                // Cache dependencies on the Url Slugs and also the DocumentID if available.
                                if (cs.Cached)
                                {
                                    if (Page != null)
                                    {
                                        cs.CacheDependency = CacheHelper.GetCacheDependency(new string[] {
                            "dynamicrouting.urlslug|all",
                            "documentid|" + Page.DocumentID,  });
                                    }
                                    else
                                    {
                                        cs.CacheDependency = CacheHelper.GetCacheDependency(new string[] { "dynamicrouting.urlslug|all" });
                                    }

                                }

                                // Return Page Data
                                return Query.FirstOrDefault();
                            }
                            else
                            {
                                return null;
                            }
                        }, new CacheSettings((PreviewEnabled ? 0 : 1440), "DynamicRoutine.GetPage", Url, Culture, DefaultCulture, SiteName, PreviewEnabled, ColumnsVal));
                    }
                    catch (Exception ex)
                    {
                        // Add exception so they can handle
                        DynamicRoutingGetPageTaskHandler.EventArguments.ExceptionOnLookup = ex;
                    }
                }

                // Finish event, this will trigger the After
                DynamicRoutingGetPageTaskHandler.FinishEvent();

                // Return whatever Found Page
                FoundPage = DynamicRoutingGetPageTaskHandler.EventArguments.FoundPage;
            }
            return FoundPage;
        }

        /// <summary>
        /// Gets the CMS Page using Dynamic Routing, returning the culture variation that either matches the given culture or the Slug's culture, or the default site culture if not found.
        /// </summary>
        /// <param name="Url">The Url (part after the domain), if empty will use the Current Request</param>
        /// <param name="Culture">The Culture, not needed if the Url contains the culture that the UrlSlug has as part of it's generation.</param>
        /// <param name="SiteName">The Site Name, defaults to current site.</param>
        /// <param name="Columns">List of columns you wish to include in the data returned.</param>
        /// <returns>The Page that matches the Url Slug, for the given or matching culture (or default culture if one isn't found).</returns>
        public static T GetPage<T>(string Url = "", string Culture = "", string SiteName = "", IEnumerable<string> Columns = null) where T : ITreeNode
        {
            return (T)GetPage(Url, Culture, SiteName, Columns);
        }

        /// <summary>
        /// Gets the Page's Url Slug based on the given DocumentID and it's Culture.
        /// </summary>
        /// <param name="DocumentID">The Document ID</param>
        /// <returns></returns>
        public static string GetPageUrl(int DocumentID)
        {
            return DynamicRouteInternalHelper.GetPageUrl(DocumentID);
        }

        /// <summary>
        /// Gets the Page's Url Slug based on the given DocumentGuid and it's Culture.
        /// </summary>
        /// <param name="DocumentGuid">The Document Guid</param>
        /// <returns>The UrlSlug (with ~ prepended) or Null if page not found.</returns>
        public static string GetPageUrl(Guid DocumentGuid)
        {
            return DynamicRouteInternalHelper.GetPageUrl(DocumentGuid);
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
            return DynamicRouteInternalHelper.GetPageUrl(NodeAliasPath, DocumentCulture, SiteName);
        }

        /// <summary>
        /// Gets the Page's Url Slug based on the given NodeGuid and Culture.  If Culture not found, then will prioritize the Site's Default Culture, then Cultures by alphabetical order.
        /// </summary>
        /// <param name="NodeGuid">The Node to find the Url Slug</param>
        /// <param name="DocumentCulture">The Document Culture, if not provided will use default Site's Culture.</param>
        /// <returns>The UrlSlug (with ~ prepended) or Null if page not found.</returns>
        public static string GetPageUrl(Guid NodeGuid, string DocumentCulture = null)
        {
            return DynamicRouteInternalHelper.GetPageUrl(NodeGuid, DocumentCulture);
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
            return DynamicRouteInternalHelper.GetPageUrl(NodeID, DocumentCulture, SiteName);
        }

        /// <summary>
        /// Gets the Route Configuration based on The node's Class Name.  
        /// </summary>
        /// <param name="node">The ITreeNode object</param>
        /// <returns>The Route Configuration, empty DynamicRouteconfiguration if not found</returns>
        public static DynamicRouteConfiguration GetRouteConfiguration(ITreeNode node)
        {
            if (!DynamicRoutingAnalyzer.TryFindMatch(node.ClassName, out var match))
            {
                return new DynamicRouteConfiguration();
            }

            return match;
        }
    }
}
