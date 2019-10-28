using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using DynamicRouting;

[assembly: RegisterObjectType(typeof(VersionHistoryUrlSlugInfo), VersionHistoryUrlSlugInfo.OBJECT_TYPE)]

namespace DynamicRouting
{
    /// <summary>
    /// Data container class for <see cref="VersionHistoryUrlSlugInfo"/>.
    /// </summary>
    [Serializable]
    public partial class VersionHistoryUrlSlugInfo : AbstractInfo<VersionHistoryUrlSlugInfo>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "dynamicrouting.versionhistoryurlslug";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(VersionHistoryUrlSlugInfoProvider), OBJECT_TYPE, "DynamicRouting.VersionHistoryUrlSlug", "VersionHistoryUrlSlugID", "VersionHistoryUrlSlugLastModified", "VersionHistoryUrlSlugGuid", null, null, null, null, null, null)
        {
            ModuleName = "DynamicRouting.Kentico",
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("VersionHistoryUrlSlugVersionHistoryID", "cms.versionhistory", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Version history url slug ID.
        /// </summary>
        [DatabaseField]
        public virtual int VersionHistoryUrlSlugID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("VersionHistoryUrlSlugID"), 0);
            }
            set
            {
                SetValue("VersionHistoryUrlSlugID", value);
            }
        }


        /// <summary>
        /// Version history url slug version history ID.
        /// </summary>
        [DatabaseField]
        public virtual int VersionHistoryUrlSlugVersionHistoryID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("VersionHistoryUrlSlugVersionHistoryID"), 0);
            }
            set
            {
                SetValue("VersionHistoryUrlSlugVersionHistoryID", value);
            }
        }


        /// <summary>
        /// Version history url slug.
        /// </summary>
        [DatabaseField]
        public virtual string VersionHistoryUrlSlug
        {
            get
            {
                return ValidationHelper.GetString(GetValue("VersionHistoryUrlSlug"), String.Empty);
            }
            set
            {
                SetValue("VersionHistoryUrlSlug", value);
            }
        }


        /// <summary>
        /// Version history url slug guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid VersionHistoryUrlSlugGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("VersionHistoryUrlSlugGuid"), Guid.Empty);
            }
            set
            {
                SetValue("VersionHistoryUrlSlugGuid", value);
            }
        }


        /// <summary>
        /// Version history url slug last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime VersionHistoryUrlSlugLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("VersionHistoryUrlSlugLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("VersionHistoryUrlSlugLastModified", value);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            VersionHistoryUrlSlugInfoProvider.DeleteVersionHistoryUrlSlugInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            VersionHistoryUrlSlugInfoProvider.SetVersionHistoryUrlSlugInfo(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected VersionHistoryUrlSlugInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="VersionHistoryUrlSlugInfo"/> class.
        /// </summary>
        public VersionHistoryUrlSlugInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="VersionHistoryUrlSlugInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public VersionHistoryUrlSlugInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}