using CMS.Base;
using CMS.DocumentEngine;
using System;
using System.Web;

namespace DynamicRouting
{
    public class GetCultureEventArgs : CMSEventArgs
    {
        /// <summary>
        /// The Culture, this is what you should set when determining the culture
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// The Site's Default culture, based on the Current Site
        /// </summary>
        public string DefaultCulture { get; set; }

        /// <summary>
        /// The Site Code Name of the current site
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// The HttpRequest
        /// </summary>
        public HttpRequest Request { get; set; }

        /// <summary>
        /// True if Kentico's Preview is enable, if true the culture will be set by the PreviewEnabled after the "Before" event.
        /// </summary>
        public bool PreviewEnabled { get; set; }
    }
}
