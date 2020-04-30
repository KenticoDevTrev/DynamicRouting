using System;
using System.Web;
using System.Web.Routing;
using CMS.Helpers;
using DynamicRouting.Implementations;
using DynamicRouting.Interfaces;

namespace DynamicRouting.Kentico.MVC
{
    public class DynamicRouteConstraint : IRouteConstraint
    {
        private IDynamicRouteHelper mDynamicRouteHelper;
        public DynamicRouteConstraint()
        {
            mDynamicRouteHelper = new BaseDynamicRouteHelper();
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            // Do not use Dynamic Routing for UrlGeneration
            if(routeDirection == RouteDirection.UrlGeneration)
            {
                return false;
            }

            string controllerName = values.ContainsKey("controller")
                    ? ValidationHelper.GetString(values["controller"], "")
                    : "";

            if (controllerName.Equals("KenticoFormWidget", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            var page = mDynamicRouteHelper.GetPage(AddPageToCacheDependency: false);
            return page != null;
        }
    }
}
