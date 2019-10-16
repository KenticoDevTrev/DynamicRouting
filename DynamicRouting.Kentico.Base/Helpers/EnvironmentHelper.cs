using CMS.Base;
using CMS.SiteProvider;
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
        /// <param name="SiteName">SiteName, if provided will also remove the site's forbidden and replacement characters</param>
        /// <returns>The Url for lookup</returns>
        public static string GetUrl(HttpRequestBase Request, string SiteName = "")
        {
            return GetUrl(Request.Url.AbsolutePath, Request.ApplicationPath, SiteName);
        }

        /// <summary>
        /// Gets the Url requested, handling any Virtual Directories
        /// </summary>
        /// <param name="Request">The Request</param>
        /// <param name="SiteName">SiteName, if provided will also remove the site's forbidden and replacement characters</param>
        /// <returns>The Url for lookup</returns>
        public static string GetUrl(HttpRequest Request, string SiteName = "")
        {
            return GetUrl(Request.Url.AbsolutePath, Request.ApplicationPath, SiteName);
        }

        /// <summary>
        /// Gets the Url requested, handling any Virtual Directories
        /// </summary>
        /// <param name="Request">The Request</param>
        /// <param name="SiteName">SiteName, if provided will also remove the site's forbidden and replacement characters</param>
        /// <returns>The Url for lookup</returns>
        public static string GetUrl(IRequest Request, string SiteName = "")
        {
            return GetUrl(Request.Url.AbsolutePath, HttpContext.Current.Request.ApplicationPath, SiteName);
        }

        /// <summary>
        /// Removes Application Path from Url if present and ensures starts with /
        /// </summary>
        /// <param name="Url">The Url (Relative)</param>
        /// <param name="ApplicationPath"></param>
        /// <param name="SiteName">SiteName, if provided will also remove the site's forbidden and replacement characters</param>
        /// <returns></returns>
        public static string GetUrl(string RelativeUrl, string ApplicationPath, string SiteName = "")
        {
            // Remove Application Path from Relative Url if it exists at the beginning
            if (!string.IsNullOrWhiteSpace(ApplicationPath) && ApplicationPath != "/" && RelativeUrl.ToLower().IndexOf(ApplicationPath.ToLower()) == 0)
            {
                RelativeUrl = RelativeUrl.Substring(ApplicationPath.Length);
            }
            
            return DynamicRouteInternalHelper.GetCleanUrl(RelativeUrl, SiteName);
        }

    }
}
