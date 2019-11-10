using CMS.Base;
using CMS.Helpers;
using CMS.SiteProvider;
using System;
using System.Web;

namespace DynamicRouting.Kentico.MVCOnly.Helpers
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
            
            return GetCleanUrl(RelativeUrl, SiteName);
        }

        public static string GetCleanUrl(string Url, string SiteName = "")
        {
            // Remove trailing or double //'s and any url parameters / anchors
            Url = "/" + Url.Trim("/ ".ToCharArray()).Split('?')[0].Split('#')[0];
            Url = HttpUtility.UrlDecode(Url);

            // Replace forbidden characters
            // Remove / from the forbidden characters because that is part of the Url, of course.
            if (string.IsNullOrWhiteSpace(SiteName) && !string.IsNullOrWhiteSpace(SiteContext.CurrentSiteName))
            {
                SiteName = SiteContext.CurrentSiteName;
            }
            if (!string.IsNullOrWhiteSpace(SiteName))
            {
                string ForbiddenCharacters = URLHelper.ForbiddenURLCharacters(SiteName).Replace("/", "");
                string Replacement = URLHelper.ForbiddenCharactersReplacement(SiteName).ToString();
                Url = ReplaceAnyCharInString(Url, ForbiddenCharacters.ToCharArray(), Replacement);
            }

            // Escape special url characters
            Url = URLHelper.EscapeSpecialCharacters(Url);

            return Url;
        }

        /// <summary>
        /// Replaces any char in the char array with the replace value for the string
        /// </summary>
        /// <param name="value">The string to replace values in</param>
        /// <param name="CharsToReplace">The character array of characters to replace</param>
        /// <param name="ReplaceValue">The value to replace them with</param>
        /// <returns></returns>
        private static string ReplaceAnyCharInString(string value, char[] CharsToReplace, string ReplaceValue)
        {
            string[] temp = value.Split(CharsToReplace, StringSplitOptions.RemoveEmptyEntries);
            return String.Join(ReplaceValue, temp);
        }

    }
}
