using DynamicRouting.Implementations;
using DynamicRouting.Interfaces;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DynamicRouting.Kentico.MVC
{
    public class DynamicRouteCachedController : Controller
    {
        private IDynamicRouteHelper mDynamicRouteHelper;
        public DynamicRouteCachedController()
        {
            mDynamicRouteHelper = new BaseDynamicRouteHelper();
        }

        [OutputCache(CacheProfile = "DynamicRouteController")]
        public ActionResult RenderView(bool? IncludeDocumentInOutputCache = null)
        {
            if (!IncludeDocumentInOutputCache.HasValue)
            {
                IncludeDocumentInOutputCache = true;
            }
            var node = mDynamicRouteHelper.GetPage(AddPageToCacheDependency: IncludeDocumentInOutputCache.Value);
            var routeConfig = mDynamicRouteHelper.GetRouteConfiguration(node);
            HttpContext.Kentico().PageBuilder().Initialize(node.DocumentID);

            return View(routeConfig.ViewName);
        }

        [OutputCache(CacheProfile = "DynamicRouteController")]
        public ActionResult RenderViewWithModel(bool? IncludeDocumentInOutputCache = null)
        {
            if (!IncludeDocumentInOutputCache.HasValue)
            {
                IncludeDocumentInOutputCache = true;
            }
            var node = mDynamicRouteHelper.GetPage(AddPageToCacheDependency: IncludeDocumentInOutputCache.Value);
            var routeConfig = mDynamicRouteHelper.GetRouteConfiguration(node);
            HttpContext.Kentico().PageBuilder().Initialize(node.DocumentID);

            // Convert type
            if (routeConfig.ModelType != null)
            {
                try
                {
                    return View(routeConfig.ViewName, Convert.ChangeType(node, routeConfig.ModelType));
                }
                catch (InvalidCastException ex)
                {
                    throw new InvalidCastException(ex.Message + ", this may be caused by the generated PageType class not being found in the project, or if it's located in an assembly that does not have [assembly: AssemblyDiscoverable] in it's AssemblyInfo.cs.  The found page is of type " + (node == null ? "Null" : node.GetType().FullName), ex);
                }
            }
            else
            {
                return View(routeConfig.ViewName, node);
            }
        }

        public ActionResult RouteValuesNotFound()
        {
            var node = mDynamicRouteHelper.GetPage();
            return Content($"<h1>No Route Value Found</h1><p>No DynamicRouting assembly tag was found for the class <strong>{node.ClassName}</strong>, could not route page {node.NodeAliasPath}</p>");
        }
    }
}
