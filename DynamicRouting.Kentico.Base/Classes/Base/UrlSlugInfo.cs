using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using DynamicRouting;

[assembly: RegisterObjectType(typeof(UrlSlugInfo), UrlSlugInfo.OBJECT_TYPE)]

namespace DynamicRouting
{
    /// <summary>
    /// Data container class for <see cref="UrlSlugInfo"/>.
    /// </summary>
    [Serializable]
    public partial class UrlSlugInfo : AbstractInfo<UrlSlugInfo>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "dynamicrouting.urlslug";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(UrlSlugInfoProvider), OBJECT_TYPE, "DynamicRouting.UrlSlug", "UrlSlugID", "UrlSlugLastModified", "UrlSlugGuid", null, "UrlSlug", null, null, null, null)
        {
            ModuleName = "DynamicRouting.Kentico",
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("UrlSlugNodeID", "cms.node", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Url slug ID.
        /// </summary>
        [DatabaseField]
        public virtual int UrlSlugID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("UrlSlugID"), 0);
            }
            set
            {
                SetValue("UrlSlugID", value);
            }
        }


        /// <summary>
        /// The part of the url after the domain extension that identifies this page.  Ex "/My-Page".
        /// </summary>
        [DatabaseField]
        public virtual string UrlSlug
        {
            get
            {
                return ValidationHelper.GetString(GetValue("UrlSlug"), String.Empty);
            }
            set
            {
                SetValue("UrlSlug", value);
            }
        }


        /// <summary>
        /// What node this URL slug belongs to.
        /// </summary>
        [DatabaseField]
        public virtual int UrlSlugNodeID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("UrlSlugNodeID"), 0);
            }
            set
            {
                SetValue("UrlSlugNodeID", value);
            }
        }


        /// <summary>
        /// The Culture this URL slug applies to.  Can be null if this URL slug is the fall back URL for all cultural variations of this node..
        /// </summary>
        [DatabaseField]
        public virtual string UrlSlugCultureCode
        {
            get
            {
                return ValidationHelper.GetString(GetValue("UrlSlugCultureCode"), String.Empty);
            }
            set
            {
                SetValue("UrlSlugCultureCode", value, String.Empty);
            }
        }


        /// <summary>
        /// If checked, this indicates that the Url Slug was added manually and should not be overwritten during document URL slug updates..
        /// </summary>
        [DatabaseField]
        public virtual bool UrlSlugIsCustom
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("UrlSlugIsCustom"), false);
            }
            set
            {
                SetValue("UrlSlugIsCustom", value);
            }
        }


        /// <summary>
        /// Url slug guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid UrlSlugGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("UrlSlugGuid"), Guid.Empty);
            }
            set
            {
                SetValue("UrlSlugGuid", value);
            }
        }


        /// <summary>
        /// Url slug last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime UrlSlugLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("UrlSlugLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("UrlSlugLastModified", value);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            UrlSlugInfoProvider.DeleteUrlSlugInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            UrlSlugInfoProvider.SetUrlSlugInfo(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected UrlSlugInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="UrlSlugInfo"/> class.
        /// </summary>
        public UrlSlugInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="UrlSlugInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public UrlSlugInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}