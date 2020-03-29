using CMS.Base;
using CMS.DocumentEngine;
using System;
using System.Web;

namespace DynamicRouting
{
    public class GetPageEventArgs : CMSEventArgs
    {
        /// <summary>
        /// The Page that is found, this is what will be returned from the GetPage function, set this.
        /// </summary>
        public ITreeNode FoundPage { get; set; }

        /// <summary>
        /// The Request's Relative Url (no query strings), cleaned to be proper lookup format
        /// </summary>
        public string RelativeUrl { get; set; }

        /// <summary>
        /// The Request's Culture
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// The Site's default culture
        /// </summary>
        public string DefaultCulture { get; set; }

        /// <summary>
        /// The current SiteName
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// If Kentico's Preview mode is active (Preview/Edit)
        /// </summary>
        public bool PreviewEnabled { get; set; }

        /// <summary>
        /// The User's requested Columns to return with the page data
        /// </summary>
        public string ColumnsVal { get; set; }

        /// <summary>
        /// The full HttpRequest object
        /// </summary>
        public HttpRequest Request { get; set; }

        /// <summary>
        /// If an exception occurred between the Before and After (while looking up), this is the exception. Can be used for custom logging.
        /// </summary>
        public Exception ExceptionOnLookup { get; set; }
    }
}
