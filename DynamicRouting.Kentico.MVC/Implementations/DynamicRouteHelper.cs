﻿using CMS.Base;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.SiteProvider;
using DynamicRouting.Interfaces;
using DynamicRouting.Kentico.MVC;
using DynamicRouting.Kentico.MVCOnly.Helpers;
using Kentico.Content.Web.Mvc;
using Kentico.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DynamicRouting.Implementations
{
    public class BaseDynamicRouteHelper : IDynamicRouteHelper
    {
        public BaseDynamicRouteHelper()
        {
        }

        /// <summary>
        /// Helper in case SiteContext is null for OWIN requests
        /// </summary>
        /// <returns></returns>
        public SiteInfo SiteContextSafe()
        {
            return SiteContext.CurrentSite != null ? SiteContext.CurrentSite : SiteInfoProvider.GetSites().TopN(1).FirstOrDefault();
        }

        /// <summary>
        /// Gets the CMS Page, returning the culture variation that either matches the given culture or the Url's culture, or the default site culture if not found.  You can use the DynamicRoutingEvents to hook in and apply your logic.        /// </summary>
        /// <param name="Url">The Url (part after the domain), if empty will use the Current Request</param>
        /// <param name="Culture">The Culture, not needed if the Url contains the culture that the UrlSlug has as part of it's generation.</param>
        /// <param name="SiteName">The Site Name, defaults to current site.</param>
        /// <param name="Columns">List of columns you wish to include in the data returned.</param>
        /// <param name="AddPageToCacheDependency">If true, the found page will have it's DocumentID added to the request's Output Cache Dependency</param>
        /// <returns>The Page that matches the Url, for the given or matching culture (or default culture if one isn't found).</returns>
        public ITreeNode GetPage(string Url = "", string Culture = "", string SiteName = "", IEnumerable<string> Columns = null, bool AddPageToCacheDependency = true)
        {
            // Load defaults
            SiteName = (!string.IsNullOrWhiteSpace(SiteName) ? SiteName : SiteContextSafe().SiteName);
            string DefaultCulture = SiteContextSafe().DefaultVisitorCulture;
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
            catch (InvalidOperationException) { }

            GetCultureEventArgs CultureArgs = new GetCultureEventArgs()
            {
                DefaultCulture = DefaultCulture,
                SiteName = SiteName,
                Request = HttpContext.Current.Request,
                PreviewEnabled = PreviewEnabled,
                Culture = Culture
            };

            using (var DynamicRoutingGetCultureTaskHandler = DynamicRoutingEvents.GetCulture.StartEvent(CultureArgs))
            {
                // If Preview is enabled, use the Kentico Preview CultureName
                if (PreviewEnabled && string.IsNullOrWhiteSpace(CultureArgs.Culture))
                {
                    try
                    {
                        CultureArgs.Culture = HttpContext.Current.Kentico().Preview().CultureName;
                    }
                    catch (Exception) { }
                }

                // If culture still not set, use the LocalizationContext.CurrentCulture
                if (string.IsNullOrWhiteSpace(CultureArgs.Culture))
                {
                    try
                    {
                        CultureArgs.Culture = LocalizationContext.CurrentCulture.CultureName;
                    }
                    catch (Exception) { }
                }

                // If that fails then use the System.Globalization.CultureInfo
                if (string.IsNullOrWhiteSpace(CultureArgs.Culture))
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

            // Convert Columns to string, must include DocumentID though at all times
            if (Columns != null && !Columns.Contains("*") && !Columns.Contains("documentid", StringComparer.InvariantCultureIgnoreCase))
            {
                var Appended = Columns.ToList();
                Appended.Add("documentid");
                Columns = Appended;
            }
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
                // Use the Event Hooks to apply the logic for your site to determine the Page

                // Finish event, this will trigger the After
                DynamicRoutingGetPageTaskHandler.FinishEvent();

                // Return whatever Found Page
                FoundPage = DynamicRoutingGetPageTaskHandler.EventArguments.FoundPage;
            }

            // Add documentID to the output cache dependencies, we ensured that DocumentID would be returned in the result always.
            if (FoundPage != null && AddPageToCacheDependency && HttpContext.Current != null && HttpContext.Current.Response != null)
            {
                string Key = $"documentid|{FoundPage.DocumentID}";
                CacheHelper.EnsureDummyKey(Key);
                HttpContext.Current.Response.AddCacheItemDependency(Key);
            }

            return FoundPage;
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
        public T GetPage<T>(string Url = "", string Culture = "", string SiteName = "", IEnumerable<string> Columns = null, bool AddPageToCacheDependency = true) where T : ITreeNode
        {
            return (T)GetPage(Url, Culture, SiteName, Columns, AddPageToCacheDependency);
        }

        /// <summary>
        /// Gets the Page's Url based on the given DocumentID and it's Culture.
        /// </summary>
        /// <param name="DocumentID">The Document ID</param>
        /// <returns>The Url (with ~ prepended) or Null if page not found.</returns>
        public string GetPageUrl(int DocumentID)
        {
            return "~" + DocumentHelper.GetDocument(DocumentID, new TreeProvider()).RelativeURL.Trim('~');
        }

        /// <summary>
        /// Gets the Page's Url based on the given DocumentGuid and it's Culture.
        /// </summary>
        /// <param name="DocumentGuid">The Document Guid</param>
        /// <returns>The Url (with ~ prepended) or Null if page not found.</returns>
        public string GetPageUrl(Guid DocumentGuid)
        {
            return "~" + DocumentHelper.GetDocuments().WhereEquals("DocumentGuid", DocumentGuid).FirstOrDefault().RelativeURL.Trim('~');
        }

        /// <summary>
        /// Gets the Page's Url based on the given NodeAliasPath, Culture and SiteName.  If Culture not found, then will prioritize the Site's Default Culture, then Cultures by alphabetical order.
        /// </summary>
        /// <param name="NodeAliasPath">The Node alias path you wish to select</param>
        /// <param name="DocumentCulture">The Document Culture, if not provided will use default Site's Culture.</param>
        /// <param name="SiteName">The Site Name, if not provided then the Current Site's name is used.</param>
        /// <returns>The Url (with ~ prepended) or Null if page not found.</returns>
        public string GetPageUrl(string NodeAliasPath, string DocumentCulture = null, string SiteName = null)
        {
            var SelectionParams = new NodeSelectionParameters()
            {
                AliasPath = NodeAliasPath,
                CombineWithDefaultCulture = true
            };
            if (!string.IsNullOrWhiteSpace(DocumentCulture))
            {
                SelectionParams.CultureCode = DocumentCulture;
            }
            if (!string.IsNullOrWhiteSpace(SiteName))
            {
                SelectionParams.SiteName = SiteName;
            }
            return "~" + DocumentHelper.GetDocument(SelectionParams, new TreeProvider()).RelativeURL.Trim('~');
        }

        /// <summary>
        /// Gets the Page's Url based on the given NodeGuid and Culture.  If Culture not found, then will prioritize the Site's Default Culture, then Cultures by alphabetical order.
        /// </summary>
        /// <param name="NodeGuid">The Node to find the Url Slug</param>
        /// <param name="DocumentCulture">The Document Culture, if not provided will use default Site's Culture.</param>
        /// <returns>The Url (with ~ prepended) or Null if page not found.</returns>
        public string GetPageUrl(Guid NodeGuid, string DocumentCulture = null)
        {
            var DocQuery = DocumentHelper.GetDocuments()
                .WhereEquals("NodeGuid", NodeGuid)
                .CombineWithAnyCulture()
                .CombineWithDefaultCulture();
            if (!string.IsNullOrWhiteSpace(DocumentCulture))
            {
                DocQuery.Culture(DocumentCulture);
            }
            return "~" + DocQuery.FirstOrDefault().RelativeURL.Trim('~');
        }

        /// <summary>
        /// Gets the Page's Url based on the given NodeID and Culture.  If Culture not found, then will prioritize the Site's Default Culture, then Cultures by alphabetical order.
        /// </summary>
        /// <param name="NodeID">The NodeID</param>
        /// <param name="DocumentCulture">The Document Culture, if not provided will use default Site's Culture.</param>
        /// <param name="SiteName">The Site Name, if not provided then will query the NodeID to find it's site.</param>
        /// <returns>The Url (with ~ prepended) or Null if page not found.</returns>
        public string GetPageUrl(int NodeID, string DocumentCulture = null, string SiteName = null)
        {
            return "~" + DocumentHelper.GetDocument(NodeID, DocumentCulture, new TreeProvider()).RelativeURL.Trim('~');
        }

        /// <summary>
        /// Gets the Route Configuration based on The node's Class Name.  
        /// </summary>
        /// <param name="node">The ITreeNode object</param>
        /// <returns>The Route Configuration, empty DynamicRouteconfiguration if not found</returns>
        public DynamicRouteConfiguration GetRouteConfiguration(ITreeNode node)
        {
            if (!DynamicRoutingAnalyzer.TryFindMatch(node.ClassName, out var match))
            {
                return new DynamicRouteConfiguration();
            }

            return match;
        }
    }
}
