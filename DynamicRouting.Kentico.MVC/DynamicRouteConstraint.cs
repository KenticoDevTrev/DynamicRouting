﻿using System;
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
            // Never match on URL generation, this is called with the Url.Action and HtmlLink and shouldn't ever be used.
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
