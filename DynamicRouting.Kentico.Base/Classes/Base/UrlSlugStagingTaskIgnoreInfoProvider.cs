using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace DynamicRouting
{
    /// <summary>
    /// Class providing <see cref="UrlSlugStagingTaskIgnoreInfo"/> management.
    /// </summary>
    public partial class UrlSlugStagingTaskIgnoreInfoProvider : AbstractInfoProvider<UrlSlugStagingTaskIgnoreInfo, UrlSlugStagingTaskIgnoreInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="UrlSlugStagingTaskIgnoreInfoProvider"/>.
        /// </summary>
        public UrlSlugStagingTaskIgnoreInfoProvider()
            : base(UrlSlugStagingTaskIgnoreInfo.TYPEINFO)
        {
        }


        /// <summary>
        /// Returns a query for all the <see cref="UrlSlugStagingTaskIgnoreInfo"/> objects.
        /// </summary>
        public static ObjectQuery<UrlSlugStagingTaskIgnoreInfo> GetUrlSlugStagingTaskIgnores()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="UrlSlugStagingTaskIgnoreInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="UrlSlugStagingTaskIgnoreInfo"/> ID.</param>
        public static UrlSlugStagingTaskIgnoreInfo GetUrlSlugStagingTaskIgnoreInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="UrlSlugStagingTaskIgnoreInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="UrlSlugStagingTaskIgnoreInfo"/> to be set.</param>
        public static void SetUrlSlugStagingTaskIgnoreInfo(UrlSlugStagingTaskIgnoreInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="UrlSlugStagingTaskIgnoreInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="UrlSlugStagingTaskIgnoreInfo"/> to be deleted.</param>
        public static void DeleteUrlSlugStagingTaskIgnoreInfo(UrlSlugStagingTaskIgnoreInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="UrlSlugStagingTaskIgnoreInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="UrlSlugStagingTaskIgnoreInfo"/> ID.</param>
        public static void DeleteUrlSlugStagingTaskIgnoreInfo(int id)
        {
            UrlSlugStagingTaskIgnoreInfo infoObj = GetUrlSlugStagingTaskIgnoreInfo(id);
            DeleteUrlSlugStagingTaskIgnoreInfo(infoObj);
        }
    }
}