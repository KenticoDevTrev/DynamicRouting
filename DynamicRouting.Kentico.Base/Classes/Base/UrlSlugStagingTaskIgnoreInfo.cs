using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using DynamicRouting;

[assembly: RegisterObjectType(typeof(UrlSlugStagingTaskIgnoreInfo), UrlSlugStagingTaskIgnoreInfo.OBJECT_TYPE)]

namespace DynamicRouting
{
    /// <summary>
    /// Data container class for <see cref="UrlSlugStagingTaskIgnoreInfo"/>.
    /// </summary>
    [Serializable]
    public partial class UrlSlugStagingTaskIgnoreInfo : AbstractInfo<UrlSlugStagingTaskIgnoreInfo>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "dynamicrouting.urlslugstagingtaskignore";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(UrlSlugStagingTaskIgnoreInfoProvider), OBJECT_TYPE, "DynamicRouting.UrlSlugStagingTaskIgnore", "UrlSlugStagingTaskIgnoreID", "UrlSlugStagingTaskIgnoreLastModified", null, null, null, null, null, null, null)
        {
            ModuleName = "DynamicRouting.Kentico",
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("UrlSlugStagingTaskIgnoreUrlSlugID", "dynamicrouting.urlslug", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Url slug staging task ignore ID.
        /// </summary>
        [DatabaseField]
        public virtual int UrlSlugStagingTaskIgnoreID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("UrlSlugStagingTaskIgnoreID"), 0);
            }
            set
            {
                SetValue("UrlSlugStagingTaskIgnoreID", value);
            }
        }


        /// <summary>
        /// Url slug staging task ignore url slug ID.
        /// </summary>
        [DatabaseField]
        public virtual int UrlSlugStagingTaskIgnoreUrlSlugID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("UrlSlugStagingTaskIgnoreUrlSlugID"), 0);
            }
            set
            {
                SetValue("UrlSlugStagingTaskIgnoreUrlSlugID", value);
            }
        }


        /// <summary>
        /// Url slug staging task ignore last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime UrlSlugStagingTaskIgnoreLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("UrlSlugStagingTaskIgnoreLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("UrlSlugStagingTaskIgnoreLastModified", value);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            UrlSlugStagingTaskIgnoreInfoProvider.DeleteUrlSlugStagingTaskIgnoreInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            UrlSlugStagingTaskIgnoreInfoProvider.SetUrlSlugStagingTaskIgnoreInfo(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected UrlSlugStagingTaskIgnoreInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="UrlSlugStagingTaskIgnoreInfo"/> class.
        /// </summary>
        public UrlSlugStagingTaskIgnoreInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="UrlSlugStagingTaskIgnoreInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public UrlSlugStagingTaskIgnoreInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}