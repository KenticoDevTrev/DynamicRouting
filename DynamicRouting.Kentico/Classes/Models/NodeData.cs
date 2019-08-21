using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicRouting
{
    sealed class NodeItem
    {
        public int NodeID;
        public List<NodeUrlSlug> UrlSlugs { get; set; }
        public bool IsContainer { get; set; }

        public string ClassName { get; set; }

        // If any of the Slugs were either new (no existing node slug guid) or they are updated, then updates occurred
        public bool HasUpdates
        {
            get
            {
                return UrlSlugs.Exists(x => x.ExistingNodeSlugGuid == null || x.IsUpdated);
            }
        }

        public NodeItemBuilderSettings Settings { get; set; }

        public NodeItem Parent { get; set; }

        public List<NodeItem> Children { get; set; }

        public bool ChildrenBuilt { get; set; } = false;



        /// <summary>
        /// This constructor should be called when a Node is updated, passing the NodeID and it's parent.  This is a starting point 
        /// </summary>
        /// <param name="ParentNodeID">The Parent Node id, this will be the primary root of the NodeItem Build</param>
        /// <param name="PageNodeID">The NodeID of the actual page modified, this item will also have it's children checked automatically</param>
        public NodeItem(int PageNodeID, NodeItemBuilderSettings Settings)
        {
            if (Settings.BuildSiblings)
            {
                // Start with Parent and build children, and children of the PageNodeID
                NodeID = DocumentHelper.GetDocuments().WhereEquals("NodeID", NodeID).FirstOrDefault().NodeParentID;
            }
            else
            {
                // Start with the node, and only build itself and it's children children.
                NodeID = PageNodeID;
            }

            TreeNode Node = DocumentHelper.GetDocuments().WhereEquals("NodeID", NodeID).FirstOrDefault();
            Parent = null;
            UrlSlugs = new List<NodeUrlSlug>();
            IsContainer = Node.IsCoupled;
            ClassName = Node.ClassName;
            Children = new List<NodeItem>();
            ChildrenBuilt = true;
            this.Settings = Settings;

            // Builds the slugs for itself
            BuildUrlSlugs();
            BuildChildren(Settings.BuildSiblings ? PageNodeID : -1);

        }

        public NodeItem(NodeItem parent, TreeNode Node, NodeItemBuilderSettings Settings, bool BuildChildren = false)
        {
            NodeID = Node.NodeID;
            Parent = parent;
            UrlSlugs = new List<NodeUrlSlug>();
            IsContainer = Node.IsCoupled;
            ClassName = Node.ClassName;
            this.Settings = Settings;

            // Build it's slugs
            BuildUrlSlugs();

            // If build children, then also add the children.
            if (BuildChildren || Settings.BuildAllChildren)
            {
                this.BuildChildren();
            }
        }

        /// <summary>
        /// Builds the child items
        /// </summary>
        /// <param name="AlsoBuildChildrenOfNodeID">The child that matches this NodeID will also have it's children built.</param>
        public void BuildChildren(int AlsoBuildChildrenOfNodeID = -1)
        {
            foreach (TreeNode Child in DocumentHelper.GetDocuments().WhereEquals("NodeParentNodeID", NodeID).Columns("NodeID, ClassIsCoupledClass, CLassName"))
            {
                Children.Add(new NodeItem(this, Child, Settings, AlsoBuildChildrenOfNodeID == Child.NodeID));
            }
            ChildrenBuilt = true;
        }

        /// <summary>
        /// Generates the Url slugs for itself, first pulling any Custom Url Slugs, then rendering all culture codes outlined in the settings
        /// </summary>
        public void BuildUrlSlugs()
        {
            var SlugQuery = UrlSlugInfoProvider.GetUrlSlugs()
                .WhereEquals("NodeID", NodeID);

            // If not checking for updates (rebuild), then the only ones we want to keep are the Custom Url Slugs.
            if (!Settings.CheckingForUpdates)
            {
                SlugQuery.WhereEquals("IsCustom", true);
            }

            // Import the existing Slugs (Custom if not checking for imports, or all of them)
            foreach (UrlSlugInfo ExistingSlug in SlugQuery)
            {
                UrlSlugs.Add(new NodeUrlSlug()
                {
                    IsCustom = ExistingSlug.IsCustom,
                    IsDefault = ExistingSlug.CultureCode.Equals(Settings.DefaultCultureCode, StringComparison.InvariantCultureIgnoreCase),
                    CultureCode = ExistingSlug.CultureCode,
                    UrlSlug = ExistingSlug.UrlSlug,
                    ExistingNodeSlugGuid = ExistingSlug.UrlSlugGuid
                });
            }

            // Go through any cultures that do not have custom slugs already, these are the cultures that need to be rebuilt
            foreach (string CultureCode in Settings.CultureCodes.Where(culture => UrlSlugs.Exists(slug => slug.IsCustom && slug.CultureCode.Equals(culture, StringComparison.InvariantCultureIgnoreCase))))
            {
                var CultureResolver = Settings.BaseResolver.CreateChild();
                CultureResolver.SetAnonymousSourceData(new object[] { DynamicRouteHelper.GetCulture(CultureCode) });

                bool IsDefaultCulture = CultureCode.Equals(Settings.DefaultCultureCode, StringComparison.InvariantCultureIgnoreCase);

                // Get actual Document, if it's the default culture, it MUST return some document, no matter what.
                TreeNode Document = DocumentHelper.GetDocuments(ClassName)
                    .WhereEquals("NodeID", NodeID)
                    .Culture(CultureCode)
                    .CombineWithDefaultCulture(IsDefaultCulture || Settings.GenerateIfCultureDoesntExist)
                    .FirstOrDefault();
                if (Document != null)
                {
                    // Add Document values and ParentUrl
                    var DocResolver = CultureResolver.CreateChild();
                    DocResolver.SetAnonymousSourceData(new object[] { Document });
                    if (Parent != null)
                    {
                        DocResolver.SetNamedSourceData("ParentUrl", Parent.GetUrlSlug(CultureCode));
                    }
                    var NodeSlug = new NodeUrlSlug()
                    {
                        CultureCode = CultureCode,
                        IsCustom = false,
                        IsDefault = IsDefaultCulture,
                        UrlSlug = DocResolver.ResolveMacros(DynamicRouteHelper.GetClassUrlPattern(ClassName)),
                    };

                    // If checking for updates, need to flag that an update was found
                    if (Settings.CheckingForUpdates)
                    {
                        var ExistingSlug = UrlSlugs.Where(x => x.CultureCode.Equals(CultureCode, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                        if (ExistingSlug != null)
                        {
                            // Update the existing UrlSlug only if it is different and not custom
                            if (!ExistingSlug.UrlSlug.Equals(NodeSlug.UrlSlug))
                            {
                                ExistingSlug.IsUpdated = true;
                                ExistingSlug.UrlSlug = NodeSlug.UrlSlug;
                            }
                        }
                        else
                        {
                            // No Slug exists for this culture, add.
                            UrlSlugs.Add(NodeSlug);
                        }
                    }
                    else
                    {
                        // Not checking for updates to just adding node slug.
                        UrlSlugs.Add(NodeSlug);
                    }
                }
            }
        }

        /// <summary>
        /// Checks for any changes for itself and it's children.  If changes are found and children have not been built, then Builds children and runs the CheckChanges on them.
        /// </summary>
        public void CheckChanges()
        {
            if (HasUpdates && !ChildrenBuilt)
            {
                BuildChildren();
                foreach (var Child in Children)
                {
                    Child.CheckChanges();
                }
            }
        }

        /// <summary>
        /// Gets the Url Slug for the given culture, falling back if the culture isn't found.
        /// </summary>
        /// <param name="CultureCode">The Culture Code</param>
        /// <returns>The Url Slug</returns>
        public string GetUrlSlug(string CultureCode)
        {
            var UrlSlug = UrlSlugs.Where(x => x.CultureCode.Equals(CultureCode, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (UrlSlug == null)
            {
                UrlSlug = UrlSlugs.Where(x => x.IsDefault).FirstOrDefault();
            }
            if (UrlSlug == null)
            {
                UrlSlug = UrlSlugs.FirstOrDefault();
            }
            return UrlSlug?.UrlSlug;
        }
    }


    sealed class NodeUrlSlug
    {
        public string CultureCode { get; set; }
        public string UrlSlug { get; set; }
        public bool IsDefault { get; set; }
        public bool IsCustom { get; set; }
        public bool IsUpdated { get; set; } = false;
        public Guid ExistingNodeSlugGuid { get; set; }
        public NodeUrlSlug()
        {

        }
    }

    sealed class NodeItemBuilderSettings
    {
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
        /// <summary>
        /// The Base Macro Resolver, this is used in building the Url Slugs.
        /// </summary>
        public MacroResolver BaseResolver { get; set; }
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
        /// If true, then all children need to be checked for updates. Usually set from DynamicRouteHelper.CheckAllChildren
        /// </summary>
        public bool BuildAllChildren { get; set; }

        public NodeItemBuilderSettings(List<string> CultureCodes, string DefaultCultureCode, bool GenerateIfCultureDoesntExist, MacroResolver BaseResolver, bool CheckingForUpdates, bool CheckEntireTree, bool BuildSiblings, bool BuildChildren, bool BuildAllChildren)
        {
            this.CultureCodes = CultureCodes;
            this.DefaultCultureCode = DefaultCultureCode;
            this.GenerateIfCultureDoesntExist = GenerateIfCultureDoesntExist;
            this.BaseResolver = BaseResolver;
            this.CheckingForUpdates = CheckingForUpdates;
            this.BuildSiblings = BuildSiblings;
            this.BuildChildren = BuildChildren;
            this.BuildAllChildren = BuildAllChildren;
        }
    }
}
