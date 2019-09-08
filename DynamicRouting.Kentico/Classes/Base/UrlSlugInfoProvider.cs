using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace DynamicRouting
{
    /// <summary>
    /// Class providing <see cref="UrlSlugInfo"/> management.
    /// </summary>
    public partial class UrlSlugInfoProvider : AbstractInfoProvider<UrlSlugInfo, UrlSlugInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="UrlSlugInfoProvider"/>.
        /// </summary>
        public UrlSlugInfoProvider()
            : base(UrlSlugInfo.TYPEINFO)
        {
        }


        /// <summary>
        /// Returns a query for all the <see cref="UrlSlugInfo"/> objects.
        /// </summary>
        public static ObjectQuery<UrlSlugInfo> GetUrlSlugs()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="UrlSlugInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="UrlSlugInfo"/> ID.</param>
        public static UrlSlugInfo GetUrlSlugInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }

        /// <summary>
        /// Returns <see cref="UrlSlugInfo"/> with specified ID.
        /// </summary>
        /// <param name="Guid"><see cref="UrlSlugInfo"/> ID.</param>
        public static UrlSlugInfo GetUrlSlugInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="UrlSlugInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="UrlSlugInfo"/> to be set.</param>
        public static void SetUrlSlugInfo(UrlSlugInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="UrlSlugInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="UrlSlugInfo"/> to be deleted.</param>
        public static void DeleteUrlSlugInfo(UrlSlugInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="UrlSlugInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="UrlSlugInfo"/> ID.</param>
        public static void DeleteUrlSlugInfo(int id)
        {
            UrlSlugInfo infoObj = GetUrlSlugInfo(id);
            DeleteUrlSlugInfo(infoObj);
        }
    }
}