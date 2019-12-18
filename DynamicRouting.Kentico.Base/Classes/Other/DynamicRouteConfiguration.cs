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
        public string ControllerName { get; }

        public string ActionName { get; }

        public string ViewName { get; }

        public Type ModelType { get; }

        public DynamicRouteType RouteType { get; }

        public DynamicRouteConfiguration(string controllerName, string actionName, string viewName, Type modelType, DynamicRouteType routeType)
        {

            ViewName = viewName;
            ModelType = modelType;
            RouteType = routeType;
            // Adjust based on Route Type
            switch (RouteType)
            {
                case DynamicRouteType.View:
                    ControllerName = "DynamicRoute";
                    ActionName = "RenderView";
                    break;
                case DynamicRouteType.ViewWithModel:
                    ControllerName = "DynamicRoute";
                    ActionName = "RenderViewWithModel";
                    break;
                case DynamicRouteType.Controller:
                default:
                    ControllerName = controllerName;
                    ActionName = actionName;
                    break;
            }
        }
    }

    public enum DynamicRouteType
    {
        Controller, View, ViewWithModel
    }
}
