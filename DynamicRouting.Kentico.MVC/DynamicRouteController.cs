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
    public class DynamicRouteController : Controller
    {
        /// <summary>
        /// Renders the Dynamic Route View (no model)
        /// </summary>
        /// <returns></returns>
        public ActionResult RenderView()
        {
            var node = DynamicRouteHelper.GetPage();
            var routeConfig = DynamicRouteHelper.GetRouteConfiguration(node);
            HttpContext.Kentico().PageBuilder().Initialize(node.DocumentID);

            return View(routeConfig.ViewName);
        }

        /// <summary>
        /// Renders the View with either an ITreeNode model or the given Model Type
        /// </summary>
        /// <returns></returns>
        public ActionResult RenderViewWithModel()
        {
            var node = DynamicRouteHelper.GetPage();
            var routeConfig = DynamicRouteHelper.GetRouteConfiguration(node);
            HttpContext.Kentico().PageBuilder().Initialize(node.DocumentID);

            // Convert type
            if (routeConfig.ModelType != null)
            {
                return View(routeConfig.ViewName, Convert.ChangeType(node, routeConfig.ModelType));
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
            var node = DynamicRouteHelper.GetPage();
            return Content($"<h1>No Route Value Found</h1><p>No DynamicRouting assembly tag was found for the class <strong>{node.ClassName}</strong>, could not route page {node.NodeAliasPath}</p>");
        }
    }
}
