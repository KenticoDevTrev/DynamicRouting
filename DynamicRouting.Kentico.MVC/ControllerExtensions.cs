using System;
using System.Web.Mvc;

namespace DynamicRouting.Kentico.MVC
{
    public static class ControllerExtensions
    {
        /**
         * "Controller".Length
         */
        private const int CONTROLLER_SUFFIX_LENGTH = 10;

        public static string ControllerNamePrefix<T>(this T _) where T : Controller
        {
            var controllerType = typeof(T);

            return GetControllerNamePrefixFromType(controllerType);
        }

        public static string ControllerNamePrefix(this Type controllerType)
        {
            return GetControllerNamePrefixFromType(controllerType);
        }

        private static string GetControllerNamePrefixFromType(Type controllerType)
        {
            if (!typeof(Controller).IsAssignableFrom(controllerType))
            {
                throw new ArgumentException($"Type [{controllerType.Name}] is not assignable from [{nameof(Controller)}]");
            }

            return controllerType
                .Name
                .Substring(0, controllerType.Name.Length - CONTROLLER_SUFFIX_LENGTH);
        }
    }
}
