using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace DynamicRouting
{
    /// <summary>
    /// Class providing <see cref="VersionHistoryUrlSlugInfo"/> management.
    /// </summary>
    public partial class VersionHistoryUrlSlugInfoProvider : AbstractInfoProvider<VersionHistoryUrlSlugInfo, VersionHistoryUrlSlugInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="VersionHistoryUrlSlugInfoProvider"/>.
        /// </summary>
        public VersionHistoryUrlSlugInfoProvider()
            : base(VersionHistoryUrlSlugInfo.TYPEINFO)
        {
        }


        /// <summary>
        /// Returns a query for all the <see cref="VersionHistoryUrlSlugInfo"/> objects.
        /// </summary>
        public static ObjectQuery<VersionHistoryUrlSlugInfo> GetVersionHistoryUrlSlugs()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="VersionHistoryUrlSlugInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="VersionHistoryUrlSlugInfo"/> ID.</param>
        public static VersionHistoryUrlSlugInfo GetVersionHistoryUrlSlugInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="VersionHistoryUrlSlugInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="VersionHistoryUrlSlugInfo"/> to be set.</param>
        public static void SetVersionHistoryUrlSlugInfo(VersionHistoryUrlSlugInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="VersionHistoryUrlSlugInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="VersionHistoryUrlSlugInfo"/> to be deleted.</param>
        public static void DeleteVersionHistoryUrlSlugInfo(VersionHistoryUrlSlugInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="VersionHistoryUrlSlugInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="VersionHistoryUrlSlugInfo"/> ID.</param>
        public static void DeleteVersionHistoryUrlSlugInfo(int id)
        {
            VersionHistoryUrlSlugInfo infoObj = GetVersionHistoryUrlSlugInfo(id);
            DeleteVersionHistoryUrlSlugInfo(infoObj);
        }
    }
}