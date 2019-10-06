using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using DynamicRouting;

[assembly: RegisterObjectType(typeof(SlugGenerationQueueInfo), SlugGenerationQueueInfo.OBJECT_TYPE)]

namespace DynamicRouting
{
    /// <summary>
    /// Data container class for <see cref="SlugGenerationQueueInfo"/>.
    /// </summary>
    [Serializable]
    public partial class SlugGenerationQueueInfo : AbstractInfo<SlugGenerationQueueInfo>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "dynamicrouting.sluggenerationqueue";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SlugGenerationQueueInfoProvider), OBJECT_TYPE, "DynamicRouting.SlugGenerationQueue", "SlugGenerationQueueID", "SlugGenerationQueueLastModified", "SlugGenerationQueueGuid", null, null, null, null, null, null)
        {
            ModuleName = "DynamicRouting.Kentico",
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Slug generation queue ID.
        /// </summary>
        [DatabaseField]
        public virtual int SlugGenerationQueueID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("SlugGenerationQueueID"), 0);
            }
            set
            {
                SetValue("SlugGenerationQueueID", value);
            }
        }


        /// <summary>
        /// Slug generation queue node item.
        /// </summary>
        [DatabaseField]
        public virtual string SlugGenerationQueueNodeItem
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SlugGenerationQueueNodeItem"), String.Empty);
            }
            set
            {
                SetValue("SlugGenerationQueueNodeItem", value);
            }
        }


        /// <summary>
        /// If true, then it is currently being processed asyncly..
        /// </summary>
        [DatabaseField]
        public virtual bool SlugGenerationQueueRunning
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("SlugGenerationQueueRunning"), false);
            }
            set
            {
                SetValue("SlugGenerationQueueRunning", value);
            }
        }


        /// <summary>
        /// Thread ID of the thread running this.
        /// </summary>
        [DatabaseField]
        public virtual int SlugGenerationQueueThreadID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("SlugGenerationQueueThreadID"), 0);
            }
            set
            {
                SetValue("SlugGenerationQueueThreadID", value, 0);
            }
        }


        /// <summary>
        /// The ID of the Application running this task, this is to ensure that other Applications (MVC Site vs. Mother) do not try to run tasks when the other is already running..
        /// </summary>
        [DatabaseField]
        public virtual string SlugGenerationQueueApplicationID
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SlugGenerationQueueApplicationID"), String.Empty);
            }
            set
            {
                SetValue("SlugGenerationQueueApplicationID", value, String.Empty);
            }
        }


        /// <summary>
        /// Errors that occurred while generating..
        /// </summary>
        [DatabaseField]
        public virtual string SlugGenerationQueueErrors
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SlugGenerationQueueErrors"), String.Empty);
            }
            set
            {
                SetValue("SlugGenerationQueueErrors", value, String.Empty);
            }
        }


        /// <summary>
        /// Slug generation queue started.
        /// </summary>
        [DatabaseField]
        public virtual DateTime SlugGenerationQueueStarted
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("SlugGenerationQueueStarted"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SlugGenerationQueueStarted", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// If failed, what time it ended.
        /// </summary>
        [DatabaseField]
        public virtual DateTime SlugGenerationQueueEnded
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("SlugGenerationQueueEnded"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SlugGenerationQueueEnded", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Slug generation queue guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid SlugGenerationQueueGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("SlugGenerationQueueGuid"), Guid.Empty);
            }
            set
            {
                SetValue("SlugGenerationQueueGuid", value);
            }
        }


        /// <summary>
        /// Slug generation queue last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime SlugGenerationQueueLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("SlugGenerationQueueLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SlugGenerationQueueLastModified", value);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SlugGenerationQueueInfoProvider.DeleteSlugGenerationQueueInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SlugGenerationQueueInfoProvider.SetSlugGenerationQueueInfo(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected SlugGenerationQueueInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="SlugGenerationQueueInfo"/> class.
        /// </summary>
        public SlugGenerationQueueInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="SlugGenerationQueueInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public SlugGenerationQueueInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}