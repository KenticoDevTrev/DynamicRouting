using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using System;
using System.Web.Mvc;

namespace DynamicRouting.Kentico.MVC
{
    
    public class DynamicRouteCachedController : Controller
    {
        /// <summary>
        /// Renders the Dynamic Route View (no model)
        /// </summary>
        /// <returns></returns>
        [OutputCache(CacheProfile = "DynamicRouteController")]
        public ActionResult RenderView(bool? IncludeDocumentInOutputCache = null)
        {
            if (!IncludeDocumentInOutputCache.HasValue)
            {
                IncludeDocumentInOutputCache = DynamicRouteInternalHelper.GetDefaultAddPageToCacheDependency();
            }
            // Get default Add Page to Output Dependency
            var node = DynamicRouteHelper.GetPage(AddPageToCacheDependency: IncludeDocumentInOutputCache.Value);
            var routeConfig = DynamicRouteHelper.GetRouteConfiguration(node);
            HttpContext.Kentico().PageBuilder().Initialize(node.DocumentID);

            return View(routeConfig.ViewName);
        }

        /// <summary>
        /// Renders the View with either an ITreeNode model or the given Model Type
        /// </summary>
        /// <returns></returns>
        [OutputCache(CacheProfile = "DynamicRouteController")]
        public ActionResult RenderViewWithModel(bool? IncludeDocumentInOutputCache = null)
        {
            if (!IncludeDocumentInOutputCache.HasValue)
            {
                IncludeDocumentInOutputCache = DynamicRouteInternalHelper.GetDefaultAddPageToCacheDependency();
            }
            var node = DynamicRouteHelper.GetPage(AddPageToCacheDependency: IncludeDocumentInOutputCache.Value);
            var routeConfig = DynamicRouteHelper.GetRouteConfiguration(node);
            HttpContext.Kentico().PageBuilder().Initialize(node.DocumentID);

            // Convert type
            if (routeConfig.ModelType != null)
            {
                try { 
                    return View(routeConfig.ViewName, Convert.ChangeType(node, routeConfig.ModelType));
                } catch(InvalidCastException ex)
                {
                    throw new InvalidCastException(ex.Message + ", this may be caused by the generated PageType class not being found in the project, or if it's located in an assembly that does not have [assembly: AssemblyDiscoverable] in it's AssemblyInfo.cs.  The found page is of type "+(node == null ? "Null" : node.GetType().FullName), ex);
                }
            }
            else
            {
                return View(routeConfig.ViewName, node);
            }
        }


        /// <summary>
        /// Returns an error message when the page is found but no DynamicRouting assembly tags were configured.
        /// </summary>
        /// <returns></returns>
        public ActionResult RouteValuesNotFound()
        {
            var node = DynamicRouteHelper.GetPage(AddPageToCacheDependency: DynamicRouteInternalHelper.GetDefaultAddPageToCacheDependency());
            return Content($"<h1>No Route Value Found</h1><p>No DynamicRouting assembly tag was found for the class <strong>{node.ClassName}</strong>, could not route page {node.NodeAliasPath}</p>");
        }
    }
}
