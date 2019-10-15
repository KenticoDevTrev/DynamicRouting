using System;
using System.Web;
using System.Web.Routing;
using CMS.Helpers;

namespace DynamicRouting.Kentico.MVC
{
    public class DynamicRouteConstraint : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            string controllerName = values.ContainsKey("controller")
                    ? ValidationHelper.GetString(values["controller"], "")
                    : "";

            if (controllerName.Equals("KenticoFormWidget", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return DynamicRouteHelper.GetPage() is object;
        }
    }
}
