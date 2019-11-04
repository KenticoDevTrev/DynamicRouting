using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using DynamicRouting;

[assembly: RegisterObjectType(typeof(VersionHistoryGenerationQueueInfo), VersionHistoryGenerationQueueInfo.OBJECT_TYPE)]

namespace DynamicRouting
{
    /// <summary>
    /// Data container class for <see cref="VersionHistoryGenerationQueueInfo"/>.
    /// </summary>
    [Serializable]
    public partial class VersionHistoryGenerationQueueInfo : AbstractInfo<VersionHistoryGenerationQueueInfo>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "dynamicrouting.versionhistorygenerationqueue";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(VersionHistoryGenerationQueueInfoProvider), OBJECT_TYPE, "DynamicRouting.VersionHistoryGenerationQueue", "VersionHistoryGenerationQueueID", "VersionHistoryGenerationQueueLastModified", "VersionHistoryGenerationQueueGuid", null, null, null, null, null, null)
        {
            ModuleName = "DynamicRouting.Kentico",
            TouchCacheDependencies = true,
            SupportsCloning = false,
            AllowDataExport = false,
            AllowRestore = false,
        };


        /// <summary>
        /// Version history generation queue ID.
        /// </summary>
        [DatabaseField]
        public virtual int VersionHistoryGenerationQueueID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("VersionHistoryGenerationQueueID"), 0);
            }
            set
            {
                SetValue("VersionHistoryGenerationQueueID", value);
            }
        }


        /// <summary>
        /// The Class ID of the Documents that need their version url slugs regenerated.
        /// </summary>
        [DatabaseField]
        public virtual int VersionHistoryGenerationQueueClassID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("VersionHistoryGenerationQueueClassID"), 0);
            }
            set
            {
                SetValue("VersionHistoryGenerationQueueClassID", value);
            }
        }


        /// <summary>
        /// The URL pattern of this Class that we are regenerating for..
        /// </summary>
        [DatabaseField]
        public virtual string VersionHistoryGenerationQueueUrlPattern
        {
            get
            {
                return ValidationHelper.GetString(GetValue("VersionHistoryGenerationQueueUrlPattern"), String.Empty);
            }
            set
            {
                SetValue("VersionHistoryGenerationQueueUrlPattern", value);
            }
        }


        /// <summary>
        /// If true, then it is currently being processed asyncly..
        /// </summary>
        [DatabaseField]
        public virtual bool VersionHistoryGenerationQueueRunning
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("VersionHistoryGenerationQueueRunning"), false);
            }
            set
            {
                SetValue("VersionHistoryGenerationQueueRunning", value);
            }
        }


        /// <summary>
        /// Thread ID of the thread running this.
        /// </summary>
        [DatabaseField]
        public virtual int VersionHistoryGenerationQueueThreadID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("VersionHistoryGenerationQueueThreadID"), 0);
            }
            set
            {
                SetValue("VersionHistoryGenerationQueueThreadID", value, 0);
            }
        }


        /// <summary>
        /// The ID of the Application running this task, this is to ensure that other Applications (MVC Site vs. Mother) do not try to run tasks when the other is already running..
        /// </summary>
        [DatabaseField]
        public virtual string VersionHistoryGenerationQueueApplicationID
        {
            get
            {
                return ValidationHelper.GetString(GetValue("VersionHistoryGenerationQueueApplicationID"), String.Empty);
            }
            set
            {
                SetValue("VersionHistoryGenerationQueueApplicationID", value, String.Empty);
            }
        }


        /// <summary>
        /// Errors that occurred while generating..
        /// </summary>
        [DatabaseField]
        public virtual string VersionHistoryGenerationQueueErrors
        {
            get
            {
                return ValidationHelper.GetString(GetValue("VersionHistoryGenerationQueueErrors"), String.Empty);
            }
            set
            {
                SetValue("VersionHistoryGenerationQueueErrors", value, String.Empty);
            }
        }


        /// <summary>
        /// Version history generation queue started.
        /// </summary>
        [DatabaseField]
        public virtual DateTime VersionHistoryGenerationQueueStarted
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("VersionHistoryGenerationQueueStarted"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("VersionHistoryGenerationQueueStarted", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// If failed, what time it ended.
        /// </summary>
        [DatabaseField]
        public virtual DateTime VersionHistoryGenerationQueueEnded
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("VersionHistoryGenerationQueueEnded"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("VersionHistoryGenerationQueueEnded", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Version history generation queue guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid VersionHistoryGenerationQueueGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("VersionHistoryGenerationQueueGuid"), Guid.Empty);
            }
            set
            {
                SetValue("VersionHistoryGenerationQueueGuid", value);
            }
        }


        /// <summary>
        /// Version history generation queue last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime VersionHistoryGenerationQueueLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("VersionHistoryGenerationQueueLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("VersionHistoryGenerationQueueLastModified", value);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            VersionHistoryGenerationQueueInfoProvider.DeleteVersionHistoryGenerationQueueInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            VersionHistoryGenerationQueueInfoProvider.SetVersionHistoryGenerationQueueInfo(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected VersionHistoryGenerationQueueInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="VersionHistoryGenerationQueueInfo"/> class.
        /// </summary>
        public VersionHistoryGenerationQueueInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="VersionHistoryGenerationQueueInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public VersionHistoryGenerationQueueInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}