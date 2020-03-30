using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DynamicRouting.Kentico.MVC
{
    public static class DynamicRoutingAnalyzer
    {
        private static readonly Dictionary<string, DynamicRouteConfiguration> classNameLookup =
            new Dictionary<string, DynamicRouteConfiguration>();

        static DynamicRoutingAnalyzer()
        {
            var attributes = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(a => !a.FullName.StartsWith("CMS.") && !a.FullName.StartsWith("Kentico."))
                .SelectMany(a => a.GetCustomAttributes<DynamicRoutingAttribute>());

            foreach (var attribute in attributes)
            {
                if (attribute == null)
                {
                    continue;
                }
                foreach (string pageClassName in attribute.PageClassNames)
                {
                    string pageClassNameLookup = pageClassName.ToLowerInvariant();
                    if (classNameLookup.TryGetValue(pageClassNameLookup, out var pair))
                    {
                        throw new Exception(
                            "Duplicate Annotation: " +
                            $"{pair.ControllerName}Controller.{pair.ActionName} already registered for NodeClassName {pageClassNameLookup}. " +
                            $"Cannot be registered for {attribute.ControllerName}.{attribute.ActionMethodName}"
                        );
                    }

                    classNameLookup.Add(pageClassNameLookup, new DynamicRouteConfiguration(
                        controllerName: attribute.ControllerName,
                        actionName: attribute.ActionMethodName,
                        viewName: attribute.ViewName,
                        modelType: attribute.ModelType,
                        routeType: attribute.RouteType,
                        includeDocumentInOutputCache: attribute.IncludeDocumentInOutputCache,
                        useOutputCaching: attribute.UseOutputCaching
                        ));
                }
            }
        }

        public static bool TryFindMatch(string nodeClassName, out DynamicRouteConfiguration match)
        {
            return classNameLookup.TryGetValue(nodeClassName.ToLowerInvariant(), out match);
        }
    }

    public struct DynamicRouteConfiguration
    {
        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public string ViewName { get; set; }

        public Type ModelType { get; set; }

        public DynamicRouteType RouteType { get; set; }

        public Dictionary<string, object> RouteValues { get; set; }

        public bool IncludeDocumentInOutputCache { get; set; }

        public bool UseOutputCaching { get; set; }

        public DynamicRouteConfiguration(string controllerName, string actionName, string viewName, Type modelType, DynamicRouteType routeType, bool includeDocumentInOutputCache, bool useOutputCaching)
        {
            
            ViewName = viewName;
            ModelType = modelType;
            RouteType = routeType;
            IncludeDocumentInOutputCache = includeDocumentInOutputCache;

            // Adjust based on Route Type
            switch (RouteType)
            {
                case DynamicRouteType.View:
                    ControllerName = "DynamicRoute"+(useOutputCaching ? "Cached" : "");
                    ActionName = "RenderView";
                    UseOutputCaching = useOutputCaching;
                    break;
                case DynamicRouteType.ViewWithModel:
                    ControllerName = "DynamicRoute" + (useOutputCaching ? "Cached" : "");
                    ActionName = "RenderViewWithModel";
                    UseOutputCaching = useOutputCaching;
                    break;
                case DynamicRouteType.Controller:
                default:
                    ControllerName = controllerName;
                    ActionName = actionName;
                    UseOutputCaching = false;
                    break;
            }
            RouteValues = new Dictionary<string, object>();
        }
    }

    public enum DynamicRouteType
    {
        Controller, View, ViewWithModel
    }
}
