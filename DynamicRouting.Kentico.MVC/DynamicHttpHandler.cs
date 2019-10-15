using System;
using System.Web;
using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

using RequestContext = System.Web.Routing.RequestContext;

namespace DynamicRouting.Kentico.MVC
{
    public class DynamicHttpHandler : IHttpHandler
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

        public void ProcessRequest(HttpContext context)
        {
            var node = DynamicRouteHelper.GetPage();

            var routePair = ResolveRouteValues(node);

            // Setup routing with new values
            RequestContext.RouteData.Values["Controller"] = routePair.ControllerName;
            RequestContext.RouteData.Values["Action"] = routePair.ActionName;
        }

        private ControllerActionPair ResolveRouteValues(ITreeNode node)
        {
            string defaultController = RequestContext.RouteData.Values.ContainsKey("controller")
                ? RequestContext.RouteData.Values["controller"].ToString()
                : "";

            string defaultAction = RequestContext.RouteData.Values.ContainsKey("action")
                ? RequestContext.RouteData.Values["action"].ToString()
                : "";

            if (node is null)
            {
                return new ControllerActionPair(defaultController, defaultAction);
            }

            if (PageHasTemplate(node))
            {
                return new ControllerActionPair("DynamicPageTemplate", "Index");
            }

            if (!DynamicRoutingAnalyzer.TryFindMatch(node.ClassName, out var match))
            {
                return new ControllerActionPair(defaultController, defaultAction);
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