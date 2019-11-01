using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace DynamicRouting
{
    /// <summary>
    /// Class providing <see cref="VersionHistoryGenerationQueueInfo"/> management.
    /// </summary>
    public partial class VersionHistoryGenerationQueueInfoProvider : AbstractInfoProvider<VersionHistoryGenerationQueueInfo, VersionHistoryGenerationQueueInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="VersionHistoryGenerationQueueInfoProvider"/>.
        /// </summary>
        public VersionHistoryGenerationQueueInfoProvider()
            : base(VersionHistoryGenerationQueueInfo.TYPEINFO)
        {
        }


        /// <summary>
        /// Returns a query for all the <see cref="VersionHistoryGenerationQueueInfo"/> objects.
        /// </summary>
        public static ObjectQuery<VersionHistoryGenerationQueueInfo> GetVersionHistoryGenerationQueues()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="VersionHistoryGenerationQueueInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="VersionHistoryGenerationQueueInfo"/> ID.</param>
        public static VersionHistoryGenerationQueueInfo GetVersionHistoryGenerationQueueInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="VersionHistoryGenerationQueueInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="VersionHistoryGenerationQueueInfo"/> to be set.</param>
        public static void SetVersionHistoryGenerationQueueInfo(VersionHistoryGenerationQueueInfo infoObj)
        {
            // Set required field if not set.
            if (DataHelper.GetNull(infoObj.GetValue("VersionHistoryGenerationQueueRunning")) == null)
            {
                infoObj.VersionHistoryGenerationQueueRunning = false;
            }
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="VersionHistoryGenerationQueueInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="VersionHistoryGenerationQueueInfo"/> to be deleted.</param>
        public static void DeleteVersionHistoryGenerationQueueInfo(VersionHistoryGenerationQueueInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="VersionHistoryGenerationQueueInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="VersionHistoryGenerationQueueInfo"/> ID.</param>
        public static void DeleteVersionHistoryGenerationQueueInfo(int id)
        {
            VersionHistoryGenerationQueueInfo infoObj = GetVersionHistoryGenerationQueueInfo(id);
            DeleteVersionHistoryGenerationQueueInfo(infoObj);
        }
    }
}