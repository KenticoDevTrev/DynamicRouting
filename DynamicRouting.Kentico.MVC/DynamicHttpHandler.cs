﻿using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;
using Newtonsoft.Json.Linq;
using RequestContext = System.Web.Routing.RequestContext;

namespace DynamicRouting.Kentico.MVC
{
    public class DynamicHttpHandler : IHttpHandler, IRequiresSessionState
    {
        private readonly IComponentDefinitionProvider<PageTemplateDefinition> _pageTemplateDefinitionProvider;

        public RequestContext RequestContext { get; set; }

        public DynamicHttpHandler(RequestContext requestContext)
        {
            RequestContext = requestContext;
            _pageTemplateDefinitionProvider = new ComponentDefinitionProvider<PageTemplateDefinition>();
        }



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
                RequestContext.RouteData.Values["Controller"] = routePair.ControllerName;
                RequestContext.RouteData.Values["Action"] = routePair.ActionName;

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
                var PageTemplateRouteConfig = new DynamicRouteConfiguration("DynamicRouteTemplate", "Index", null, null, DynamicRouteType.Controller);
                string PageTemplateControllerName = GetPageTemplateController(node);

                // When the Dynamic Route Template Controller renders the Page Template, the Route Controller needs to match or it won't look in the right spot for the view
                if (!string.IsNullOrWhiteSpace(PageTemplateControllerName))
                {
                    PageTemplateRouteConfig.RouteValues.Add("TemplateControllerName", PageTemplateControllerName);
                }
                return PageTemplateRouteConfig;
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
        private bool PageHasTemplate(ITreeNode Page)
        {
            string TemplateConfiguration = GetTemplateConfiguration(Page);
            return !string.IsNullOrWhiteSpace(TemplateConfiguration) && !TemplateConfiguration.ToLower().Contains("\"empty.template\"");
        }

        private string GetPageTemplateController(ITreeNode Page)
        {
            string TemplateConfiguration = GetTemplateConfiguration(Page);
            if (!string.IsNullOrWhiteSpace(TemplateConfiguration) && !TemplateConfiguration.ToLower().Contains("\"empty.template\""))
            {
                var json = JObject.Parse(TemplateConfiguration);
                var templateIdentifier = ValidationHelper.GetString(json["identifier"], "");

                // Return the controller name, if it has any
                return _pageTemplateDefinitionProvider.GetAll()
                                .FirstOrDefault(def => def.Identifier.Equals(templateIdentifier, StringComparison.InvariantCultureIgnoreCase))
                                ?.ControllerName;
            }
            else
            {
                // No template
                return null;
            }
        }

        private string GetTemplateConfiguration(ITreeNode Page)
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
            return TemplateConfiguration;
        }
    }
}