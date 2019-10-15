using System;
using System.Linq;

namespace DynamicRouting.Kentico.MVC
{
    /// <summary>
    /// Marks the given <see cref="System.Web.Mvc.Controller"/> as the handler for HTTP requests 
    /// for the specified <see cref="PageClassNames" /> matching <see cref="CMS.DocumentEngine.TreeNode.ClassName"/> 
    /// for custom Page Types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class DynamicRoutingAttribute : Attribute
    {
        private string actionMethodName = "Index";

        public DynamicRoutingAttribute(Type controllerType, string[] pageClassNames)
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
            ActionMethodName = actionMethodName;
            PageClassNames = pageClassNames
                .Select(n => n.ToLowerInvariant())
                .ToArray();
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
        public string ActionMethodName
        {
            get
            {
                return actionMethodName;
            }

            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(ActionMethodName));
                }

                actionMethodName = value;
            }
        }
    }
}
