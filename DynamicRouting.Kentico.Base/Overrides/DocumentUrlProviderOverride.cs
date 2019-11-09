using CMS;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.SiteProvider;
using DynamicRouting.Kentico.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: RegisterCustomProvider(typeof(DynamicRoutingDocumentURLProvider))]

namespace DynamicRouting.Kentico.Base
{


    public class DynamicRoutingDocumentURLProvider : DocumentURLProvider
    {
        /// <summary>
        /// Returns relative URL for the specified tree node, using the Url Slug if the class is not in the excluded list.
        /// </summary>
        /// <param name="node">Tree node</param>
        /// <returns>The Relative Url</returns>
        protected override string GetUrlInternal(TreeNode node)
        {
            if (!DynamicRouteInternalHelper.UrlSlugExcludedClassNames().Contains(node.ClassName.ToLower()))
            {
                var FoundSlug = CacheHelper.Cache(cs =>
                {
                    if (cs.Cached)
                    {
                        cs.CacheDependency = CacheHelper.GetCacheDependency("DynamicRouting.UrlSlug|all");
                    }
                    return UrlSlugInfoProvider.GetUrlSlugs()
                    .WhereEquals("UrlSlugNodeID", node.NodeID)
                    .WhereEquals("UrlSlugCultureCode", node.DocumentCulture)
                    .FirstOrDefault();
                }, new CacheSettings(1440, "GetUrlSlugByNode", node.NodeID, node.DocumentCulture));

                if (FoundSlug != null)
                {
                    return FoundSlug.UrlSlug;
                }
            }
            return base.GetUrlInternal(node);
        }

        /// <summary>
        /// Returns presentation URL for the specified node, using UrlSlug if the class is not in the excluded list. This is the absolute URL where live presentation of given node can be found.
        /// </summary>
        /// <param name="node">Tree node to return presentation URL for.</param>
        /// <param name="preferredDomainName">A preferred domain name that should be used as the host part of the URL. Preferred domain must be assigned to the site as a domain alias otherwise site main domain is used.</param>
        /// <returns></returns>
        protected override string GetPresentationUrlInternal(TreeNode node, string preferredDomainName = null)
        {
            if (!DynamicRouteInternalHelper.UrlSlugExcludedClassNames().Contains(node.ClassName.ToLower()))
            {
                if (node == null)
                {
                    return null;
                }
                var FoundSlug = CacheHelper.Cache(cs =>
                {
                    if (cs.Cached)
                    {
                        cs.CacheDependency = CacheHelper.GetCacheDependency("DynamicRouting.UrlSlug|all");
                    }
                    return UrlSlugInfoProvider.GetUrlSlugs()
                    .WhereEquals("UrlSlugNodeID", node.NodeID)
                    .WhereEquals("UrlSlugCultureCode", node.DocumentCulture)
                    .FirstOrDefault();
                }, new CacheSettings(1440, "GetUrlSlugByNode", node.NodeID, node.DocumentCulture));

                if (FoundSlug != null)
                {
                    SiteInfo site = node.Site;
                    string url = FoundSlug.UrlSlug;
                    if (!string.IsNullOrEmpty(site.SitePresentationURL))
                    {
                        return URLHelper.CombinePath(url, '/', site.SitePresentationURL, null);
                    }
                    if (!string.IsNullOrEmpty(preferredDomainName))
                    {
                        return URLHelper.GetAbsoluteUrl(url, preferredDomainName);
                    }
                    return URLHelper.GetAbsoluteUrl(url, site.DomainName);
                }
            }

            return base.GetPresentationUrlInternal(node, preferredDomainName);
        }
    }
}
