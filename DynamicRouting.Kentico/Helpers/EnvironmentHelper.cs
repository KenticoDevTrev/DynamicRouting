using CMS.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DynamicRouting.Helpers
{
    public class EnvironmentHelper
    {

        /// <summary>
        /// Gets the Url requested, handling any Virtual Directories
        /// </summary>
        /// <param name="Request">The Request</param>
        /// <returns>The Url for lookup</returns>
        public static string GetUrl(HttpRequestBase Request)
        {
            return GetUrl(Request.Url.AbsolutePath, Request.ApplicationPath);
        }

        /// <summary>
        /// Gets the Url requested, handling any Virtual Directories
        /// </summary>
        /// <param name="Request">The Request</param>
        /// <returns>The Url for lookup</returns>
        public static string GetUrl(HttpRequest Request)
        {
            return GetUrl(Request.Url.AbsolutePath, Request.ApplicationPath);
        }

        /// <summary>
        /// Gets the Url requested, handling any Virtual Directories
        /// </summary>
        /// <param name="Request">The Request</param>
        /// <returns>The Url for lookup</returns>
        public static string GetUrl(IRequest Request)
        {
            return GetUrl(Request.Url.AbsolutePath, HttpContext.Current.Request.ApplicationPath);
        }

        /// <summary>
        /// Removes Application Path from Url if present and ensures starts with /
        /// </summary>
        /// <param name="Url">The Url (Relative)</param>
        /// <param name="ApplicationPath"></param>
        /// <returns></returns>
        public static string GetUrl(string RelativeUrl, string ApplicationPath)
        {
            // Remove Application Path from Relative Url if it exists at the beginning
            if (!string.IsNullOrWhiteSpace(ApplicationPath) && ApplicationPath != "/" && RelativeUrl.ToLower().IndexOf(ApplicationPath.ToLower()) == 0)
            {
                RelativeUrl = RelativeUrl.Substring(ApplicationPath.Length);
            }

            return "/" + RelativeUrl.Trim("/~".ToCharArray()).Split("?#:".ToCharArray())[0];
        }

    }
}
