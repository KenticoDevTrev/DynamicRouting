using System;
using System.Linq;

namespace DynamicRouting.Kentico.MVC
{
    /// <summary>
    /// Marks the given <see cref="System.Web.Mvc.Controller"/> as the handler for HTTP requests 
    /// for the specified <see cref="PageClassNames" /> matching <see cref="CMS.DocumentEngine.TreeNode.ClassName"/> 
    /// for custom Page Types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class DynamicRoutingAttribute : Attribute
    {
        /// <summary>
        /// Marks the given <see cref="System.Web.Mvc.Controller"/> as the handler for HTTP requests
        /// for the specified <see cref="PageClassNames" /> matching <see cref="CMS.DocumentEngine.TreeNode.ClassName"/> 
        /// for custom Page Types.
        /// </summary>
        /// <param name="controllerType">The Controller Type</param>
        /// <param name="pageClassNames">The Page Class Names</param>
        /// <param name="actionMethodName">The Optional Action Method in the Controller to handle this request.</param>
        public DynamicRoutingAttribute(Type controllerType, string[] pageClassNames, string actionMethodName = null)
        {
            if (controllerType is null)
            {
                throw new ArgumentNullException(nameof(controllerType));
            }

            if (pageClassNames is null)
            {
                throw new ArgumentNullException(nameof(pageClassNames));
            }

            ControllerName = controllerType.ControllerNamePrefix();

            if (!string.IsNullOrWhiteSpace(actionMethodName)) {
                ActionMethodName = actionMethodName;
            } else
            {
                ActionMethodName = "Index";
            }

            PageClassNames = pageClassNames
                .Select(n => n.ToLowerInvariant())
                .ToArray();
            RouteType = DynamicRouteType.Controller;
            UseOutputCaching = false;

        }

        /// <summary>
        /// Pages with the given Class Name will be routed to this View with the given Type (inheriting from ITreeNode)
        /// </summary>
        /// <param name="viewName">The View name that should be rendered.</param>
        /// <param name="modelType">The Model that inherits <see cref="CMS.Base.ITreeNode"/></param>
        /// <param name="pageClassName">The Class Name that this Dynamic Route applies to.</param>
        /// <param name="includeDocumentInOutputCache">If true, will add the Document ID to the repsonse's Cache Dependencies</param>
        /// <param name="useOutputCaching">If true, will use an Output Cached Controller to render this.</param>
        public DynamicRoutingAttribute(string viewName, Type modelType, string pageClassName, bool includeDocumentInOutputCache = true, bool useOutputCaching = false)
        {
            if (string.IsNullOrWhiteSpace(viewName))
            {
                throw new ArgumentNullException(nameof(viewName));
            }

            if (modelType is null)
            {
                throw new ArgumentNullException(nameof(modelType));
            }

            if (string.IsNullOrWhiteSpace(pageClassName))
            {
                throw new ArgumentNullException(nameof(pageClassName));
            }

            ViewName = viewName;
            ModelType = modelType;
            PageClassNames = new string[] { pageClassName };
            RouteType = DynamicRouteType.ViewWithModel;
            UseOutputCaching = useOutputCaching;
            IncludeDocumentInOutputCache = includeDocumentInOutputCache;
        }

        /// <summary>
        /// Pages with the given Class Names will be routed to this View.
        /// </summary>
        /// <param name="viewName">The View name that should be rendered.</param>
        /// <param name="pageClassNames">The Class Names that this Dynamic Route applies to.</param>
        /// <param name="includePageModel">Will pass the <see cref="CMS.Base.ITreeNode"/> page as the model for this view. If false, will not pass a model.</param>
        public DynamicRoutingAttribute(string viewName, string[] pageClassNames, bool IncludePageModel = true)
        {
            if (string.IsNullOrWhiteSpace(viewName))
            {
                throw new ArgumentNullException(nameof(viewName));
            }

            if (pageClassNames is null)
            {
                throw new ArgumentNullException(nameof(pageClassNames));
            }

            ViewName = viewName;
            PageClassNames = pageClassNames
                .Select(n => n.ToLowerInvariant())
                .ToArray();

            if(IncludePageModel)
            {
                RouteType = DynamicRouteType.ViewWithModel;
            } else
            {
                RouteType = DynamicRouteType.View;
            }
        }

        /// <summary>
        /// The name of the <see cref="System.Web.Mvc.Controller"/> that
        /// handles requests for routes for the given <see cref="PageClassNames"/>
        /// </summary>
        public string ControllerName { get; }

        /// <summary>
        /// <see cref="CMS.DocumentEngine.TreeNode.ClassName"/> values.
        /// </summary>
        public string[] PageClassNames { get; }

        /// <summary>
        /// The name of the action method that will handle the request
        /// </summary>
        public string ActionMethodName { get; }

        public Type ModelType { get; }

        public string ViewName { get; }

        public bool UseOutputCaching { get; }

        public bool IncludeDocumentInOutputCache { get; }

        public DynamicRouteType RouteType { get; }
    }
}
