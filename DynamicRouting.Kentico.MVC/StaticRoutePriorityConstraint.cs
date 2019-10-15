using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CacheHelper = CMS.Helpers.CacheHelper;
using CacheSettings = CMS.Helpers.CacheSettings;

namespace DynamicRouting.Kentico.MVC
{
    /// <summary>
    /// Checks if the Controller the route is mapping to has the StaticRoutePriorityAttribute
    /// </summary>
    public class StaticRoutePriorityConstraint : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            // Check if the controller is found and has the KMVCRouteOverPathPriority attribute.
            string ControllerName = (values.ContainsKey("controller") ? values["controller"].ToString() : "");
            return CacheHelper.Cache(cs =>
            {
                // Check if the Route that it found has the override
                IControllerFactory factory = ControllerBuilder.Current.GetControllerFactory();
                try
                {
                    var Controller = factory.CreateController(new RequestContext(httpContext, new RouteData(route, null)), ControllerName);
                    return Attribute.GetCustomAttribute(Controller.GetType(), typeof(StaticRoutePriorityAttribute)) != null;
                }
                catch (Exception)
                {
                    return false;
                }
            }, new CacheSettings(1440, "StaticRoutePriorityConstraint", ControllerName));
        }
    }

    
}