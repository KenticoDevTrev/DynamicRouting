using System;
using System.Web;
using System.Web.Mvc;
using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using RequestContext = System.Web.Routing.RequestContext;
using System.Web.SessionState;

namespace DynamicRouting.Kentico.MVC
{
    public class DynamicHttpHandler : IHttpHandler, IRequiresSessionState
    {
        public RequestContext RequestContext { get; set; }

        public DynamicHttpHandler(RequestContext requestContext) => RequestContext = requestContext;

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the page, and based on the Class of the Page, attempts to get the Dynamic routing information and processes.
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            var node = DynamicRouteHelper.GetPage();

            var routePair = ResolveRouteValues(node);

            RequestRoutingEventArgs RequestArgs = new RequestRoutingEventArgs()
            {
                Page = node,
                Configuration = routePair,
                CurrentRequestContext = RequestContext
            };

            // Use event to allow users to overwrite the Dynamic Routing Data
            using (var RequestRoutingHandler = DynamicRoutingEvents.RequestRouting.StartEvent(RequestArgs))
            {
                // Setup routing with new values
                RequestArgs.CurrentRequestContext.RouteData.Values["Controller"] = routePair.ControllerName;
                RequestArgs.CurrentRequestContext.RouteData.Values["Action"] = routePair.ActionName;

                // Allow users to adjust the RequestContext further
                RequestRoutingHandler.FinishEvent();

                // Pass back context
                RequestContext = RequestArgs.CurrentRequestContext;
            }

            IControllerFactory factory = ControllerBuilder.Current.GetControllerFactory();
            IController controller = factory.CreateController(RequestContext, routePair.ControllerName);
            controller.Execute(RequestContext);

            factory.ReleaseController(controller);
        }

        /// <summary>
        /// Determines where the Controller and Action should be based on the dynamic routing data
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private DynamicRouteConfiguration ResolveRouteValues(ITreeNode node)
        {
            string defaultController = RequestContext.RouteData.Values.ContainsKey("controller")
                ? RequestContext.RouteData.Values["controller"].ToString()
                : "";

            string defaultAction = RequestContext.RouteData.Values.ContainsKey("action")
                ? RequestContext.RouteData.Values["action"].ToString()
                : "";

            if(string.IsNullOrWhiteSpace(defaultController))
            {
                defaultController = "DynamicRoute";
            }
            if(string.IsNullOrWhiteSpace(defaultAction))
            {
                defaultAction = "RouteValuesNotFound";
            }

            if (node is null)
            {
                return new DynamicRouteConfiguration(defaultController, defaultAction, null, null, DynamicRouteType.Controller);
            }

            if (PageHasTemplate(node))
            {
                return new DynamicRouteConfiguration("DynamicRouteTemplate", "Index", null, null, DynamicRouteType.Controller);
            }

            if (!DynamicRoutingAnalyzer.TryFindMatch(node.ClassName, out var match))
            {
                return new DynamicRouteConfiguration(defaultController, defaultAction, null, null, DynamicRouteType.Controller);
            }

            return match;
        }

        /// <summary>
        /// Checks if the current page is using a template or not.
        /// </summary>
        /// <param name="Page">The Tree Node</param>
        /// <returns>If it has a template or not</returns>
        private static bool PageHasTemplate(ITreeNode Page)
        {
            string TemplateConfiguration = ValidationHelper.GetString(Page.GetValue("DocumentPageTemplateConfiguration"), "");

            // Check Temp Page builder widgets to detect a switch in template
            var InstanceGuid = ValidationHelper.GetGuid(URLHelper.GetQueryValue(HttpContext.Current.Request.Url.AbsoluteUri, "instance"), Guid.Empty);
            if (InstanceGuid != Guid.Empty)
            {
                var Table = ConnectionHelper.ExecuteQuery(String.Format("select PageBuilderTemplateConfiguration from Temp_PageBuilderWidgets where PageBuilderWidgetsGuid = '{0}'", InstanceGuid.ToString()), null, QueryTypeEnum.SQLQuery).Tables[0];
                if (Table.Rows.Count > 0)
                {
                    TemplateConfiguration = ValidationHelper.GetString(Table.Rows[0]["PageBuilderTemplateConfiguration"], "");
                }
            }

            return !String.IsNullOrWhiteSpace(TemplateConfiguration) && !TemplateConfiguration.ToLower().Contains("\"empty.template\"");
        }
    }
}