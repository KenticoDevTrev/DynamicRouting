﻿using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.SiteProvider;
using DynamicRouting.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DynamicRouting.Kentico
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
            SiteName = (!string.IsNullOrWhiteSpace(SiteName) ? SiteName : SiteContext.CurrentSiteName);
            string DefaultCulture = SiteContext.CurrentSite.DefaultVisitorCulture;
            if (string.IsNullOrWhiteSpace(Url))
            {
                Url = EnvironmentHelper.GetUrl(HttpContext.Current.Request.Url.AbsolutePath, HttpContext.Current.Request.ApplicationPath, SiteName);
            }

            // Handle Preview, during Route Config the Preview isn't available and isn't really needed, so ignore the thrown exception
            bool PreviewEnabled = false;
            try
            {
                PreviewEnabled = PortalContext.ViewMode != ViewModeEnum.LiveSite;
            }
            catch (InvalidOperationException ex) { }

            // Convert Columns to 
            string ColumnsVal = Columns != null ? string.Join(",", Columns.Distinct()) : "*";

            // Get Page based on Url
            return CacheHelper.Cache<TreeNode>(cs =>
            {
                // Using custom query as Kentico's API was not properly handling a Join and where.
                DataTable NodeTable = ConnectionHelper.ExecuteQuery("DynamicRouting.UrlSlug.GetDocumentsByUrlSlug", new QueryDataParameters()
                {
                    {"@Url", Url },
                    {"@Culture", Culture },
                    {"@DefaultCulture", DefaultCulture },
                    {"@SiteName", SiteName },
                    {"@PreviewEnabled", PreviewEnabled }
                }, topN: 1, columns: "DocumentID, ClassName").Tables[0];
                if (NodeTable.Rows.Count > 0)
                {
                    int DocumentID = ValidationHelper.GetInteger(NodeTable.Rows[0]["DocumentID"], 0);
                    string ClassName = ValidationHelper.GetString(NodeTable.Rows[0]["ClassName"], "");

                    DocumentQuery Query = DocumentHelper.GetDocuments(ClassName)
                            .WhereEquals("DocumentID", DocumentID);

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
                            "dynamicrouting.versionhistoryurlslug|all",
                            "dynamicrouting.versionhistoryurlslug|bydocumentid|"+Page.DocumentID,
                            "documentid|" + Page.DocumentID,  });
                        }
                        else
                        {
                            cs.CacheDependency = CacheHelper.GetCacheDependency(new string[] { "dynamicrouting.urlslug|all", "dynamicrouting.versionhistoryurlslug|all" });
                        }

                    }

                    // Return Page Data
                    return Query.FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }, new CacheSettings(1440, "DynamicRoutine.GetPage", Url, Culture, DefaultCulture, SiteName, PreviewEnabled, ColumnsVal));
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
    }
}