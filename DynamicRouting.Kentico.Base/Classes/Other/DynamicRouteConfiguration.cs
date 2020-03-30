using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicRouting.Kentico.MVC
{
    /// <summary>
    /// Dynamic Route Configuration Class, set in the Base so it is accessable by the Events
    /// </summary>
    public struct DynamicRouteConfiguration
    {
        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public string ViewName { get; set; }

        public Type ModelType { get; set; }

        public bool IncludeDocumentInOutputCache { get; set; }

        public bool UseOutputCaching { get; set; }

        public DynamicRouteType RouteType { get; set; }

        public Dictionary<string, object> RouteValues { get; set; }

        public DynamicRouteConfiguration(string controllerName, string actionName, string viewName, Type modelType, DynamicRouteType routeType,bool includeDocumentInOutputCache, bool useOutputCaching)
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
