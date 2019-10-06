using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace DynamicRouting
{
    /// <summary>
    /// Class providing <see cref="SlugGenerationQueueInfo"/> management.
    /// </summary>
    public partial class SlugGenerationQueueInfoProvider : AbstractInfoProvider<SlugGenerationQueueInfo, SlugGenerationQueueInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="SlugGenerationQueueInfoProvider"/>.
        /// </summary>
        public SlugGenerationQueueInfoProvider()
            : base(SlugGenerationQueueInfo.TYPEINFO)
        {
        }


        /// <summary>
        /// Returns a query for all the <see cref="SlugGenerationQueueInfo"/> objects.
        /// </summary>
        public static ObjectQuery<SlugGenerationQueueInfo> GetSlugGenerationQueues()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="SlugGenerationQueueInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="SlugGenerationQueueInfo"/> ID.</param>
        public static SlugGenerationQueueInfo GetSlugGenerationQueueInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="SlugGenerationQueueInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="SlugGenerationQueueInfo"/> to be set.</param>
        public static void SetSlugGenerationQueueInfo(SlugGenerationQueueInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="SlugGenerationQueueInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="SlugGenerationQueueInfo"/> to be deleted.</param>
        public static void DeleteSlugGenerationQueueInfo(SlugGenerationQueueInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="SlugGenerationQueueInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="SlugGenerationQueueInfo"/> ID.</param>
        public static void DeleteSlugGenerationQueueInfo(int id)
        {
            SlugGenerationQueueInfo infoObj = GetSlugGenerationQueueInfo(id);
            DeleteSlugGenerationQueueInfo(infoObj);
        }
    }
}