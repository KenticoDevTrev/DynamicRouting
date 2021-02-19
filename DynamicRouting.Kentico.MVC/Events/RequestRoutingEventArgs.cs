using CMS.Base;
using CMS.DocumentEngine;
using DynamicRouting.Kentico.MVC;
using System;
using System.Web;
using System.Web.Routing;

namespace DynamicRouting
{
    public class RequestRoutingEventArgs : CMSEventArgs
    {
        /// <summary>
        /// The Page found by the Dynamic Routing
        /// </summary>
        public ITreeNode Page { get; set; }

        /// <summary>
        /// The Route Configuration that was determined by the MVC DynamicRouting attributes.
        /// The RouteData's Controller and Action values are automatically set to the Configuration.ControllerName and Configuration.ActionName
        /// UseOutputCaching only applies to the DynamicRoute Controller, and IncludeDocumentInOutputCache applies to Template and DynamicRoute Controllers
        /// <see cref="https://github.com/KenticoDevTrev/DynamicRouting/blob/master/DynamicRouting.Kentico.MVC/DynamicHttpHandler.cs"/>
        /// </summary>
        public DynamicRouteConfiguration Configuration { get; set; }

        /// <summary>
        /// The Request Context, you can adjust the Route values through RequestContext.RouteData.Values.
        /// Configuration.ControllerName = "Blog";
        /// Configuration.ActionName = "Listing";
        /// CurrentRequestContext.RouteData.Values["SomeProperty"] = "Hello";
        /// This would route to the BlogController.Listing(string SomePropertyName) with SomePropertyName equalling Hello
        /// </summary>
        public RequestContext CurrentRequestContext { get; set; }

        public RequestRoutingEventArgs()
        {

        }
    }
}
