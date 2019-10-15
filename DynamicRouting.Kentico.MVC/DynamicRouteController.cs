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
        public ActionResult RenderView()
        {
            var node = DynamicRouteHelper.GetPage();
            var routeConfig = DynamicRouteHelper.GetRouteConfiguration(node);
            HttpContext.Kentico().PageBuilder().Initialize(node.DocumentID);

            return View(routeConfig.ViewName);
        }

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
    }
}
