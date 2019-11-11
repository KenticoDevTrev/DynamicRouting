using System;
using System.Web;
using System.Web.Routing;
using CMS.Helpers;

namespace DynamicRouting.Kentico.MVC
{
    public class DynamicRouteConstraint : IRouteConstraint
    {
        /// <summary>
        /// Returns true if a Page is found using the Dynamic Routing
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="route"></param>
        /// <param name="parameterName"></param>
        /// <param name="values"></param>
        /// <param name="routeDirection"></param>
        /// <returns></returns>
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            string controllerName = values.ContainsKey("controller")
                    ? ValidationHelper.GetString(values["controller"], "")
                    : "";

            if (controllerName.Equals("KenticoFormWidget", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            var page = DynamicRouteHelper.GetPage();
            return page != null && !DynamicRouteInternalHelper.UrlSlugExcludedClassNames().Contains(page.ClassName.ToLower());
        }
    }
}
