using CMS.MacroEngine;
using CMS.SiteProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DynamicRouting
{
    [Serializable]
    public class NodeItemBuilderSettings
    {
        /// <summary>
        /// Mainly stored for serialization so the Macro Resolver can be restored after deserialization
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// List of all Culture Codes for the site.
        /// </summary>
        public List<string> CultureCodes { get; set; }
        /// <summary>
        /// The Deafult content Culture code for this site.
        /// </summary>
        public string DefaultCultureCode { get; set; }
        /// <summary>
        /// If true, a Url Slug will be generated for a culture of a Node even if that document doesn't explicitely exist.
        /// </summary>
        public bool GenerateIfCultureDoesntExist { get; set; }


        [NonSerialized]
        private MacroResolver _BaseResolver;

        /// <summary>
        /// The Base Macro Resolver, this is used in building the Url Slugs.
        /// </summary>
        [XmlIgnore]
        public MacroResolver BaseResolver
        {
            get
            {
                if (_BaseResolver == null)
                {
                    // Rebuild
                    SiteInfo Site = SiteInfoProvider.GetSiteInfo(SiteName);
                    _BaseResolver = MacroResolver.GetInstance();
                    _BaseResolver.AddAnonymousSourceData(new object[] { Site });
                }
                return _BaseResolver;
            }
            set
            {
                _BaseResolver = value;
            }
        }
        /// <summary>
        /// If True, then will only perform updates where needed, and will not check children beyond the main scope unless a change is found (unless CheckEntireTree is true)
        /// </summary>
        public bool CheckingForUpdates { get; set; } = false;


        /// <summary>
        /// If true, then the parent of the main node should be checked along with it's children (the main node's siblings).  Usually set from DynamicRouteHelper.CheckSiblings
        /// </summary>
        public bool BuildSiblings { get; set; }

        /// <summary>
        /// If true, then the Children should be checked on the applicable nodes.  Usually set from DynamicRouteHelper.CheckChildren
        /// </summary>
        public bool BuildChildren { get; set; }

        /// <summary>
        /// If true, then all descendents need to be checked for updates. Usually set from DynamicRouteHelper.CheckDescendents
        /// </summary>
        public bool BuildDescendents { get; set; }

        /// <summary>
        /// If true, then during save A check will be performed for conflicts and will log and not-save routes if conflicts do exist.
        /// </summary>
        public bool LogConflicts { get; set; } = false;

        /// <summary>
        /// Should only be used for deserialization
        /// </summary>
        public NodeItemBuilderSettings()
        {

        }

        /// <summary>
        /// Constructor of the NodeItemBuilderSettings that is used to guide the build processes.
        /// </summary>
        /// <param name="CultureCodes">List of the site's culture code</param>
        /// <param name="DefaultCultureCode">The site's default culture code</param>
        /// <param name="GenerateIfCultureDoesntExist">If the slugs should generate for each culture even if they don't exist.</param>
        /// <param name="BaseResolver">The Base Macro Resolver</param>
        /// <param name="CheckingForUpdates">If updates should be checked</param>
        /// <param name="CheckEntireTree">If the entire tree needs to be checked, usually triggered by site, culture, or page type changes</param>
        /// <param name="BuildSiblings">If siblings need to be built (if NodeOrder is found)</param>
        /// <param name="BuildChildren">If the children of the item need to be checked (has various Parent attributes)</param>
        /// <param name="BuildDescendents">If descendents need to be checked even if the parent of it doesn't change.</param>
        /// <param name="SiteName">The SiteName</param>
        public NodeItemBuilderSettings(List<string> CultureCodes, string DefaultCultureCode, bool GenerateIfCultureDoesntExist, MacroResolver BaseResolver, bool CheckingForUpdates, bool CheckEntireTree, bool BuildSiblings, bool BuildChildren, bool BuildDescendents, string SiteName)
        {
            this.SiteName = SiteName;
            this.CultureCodes = CultureCodes;
            this.DefaultCultureCode = DefaultCultureCode;
            this.GenerateIfCultureDoesntExist = GenerateIfCultureDoesntExist;
            this.BaseResolver = BaseResolver;
            this.CheckingForUpdates = CheckingForUpdates;
            this.BuildSiblings = BuildSiblings;
            this.BuildChildren = BuildChildren;
            this.BuildDescendents = BuildDescendents;
        }
    }
}
