using System;

namespace DynamicRouting
{
    /// <summary>
    /// Represents a Node Url Slug, including references back to existing slug guid and settings
    /// </summary>
    [Serializable]
    public class NodeUrlSlug
    {
        public string CultureCode { get; set; }
        public int SiteID { get; set; }
        public string UrlSlug { get; set; }
        public bool IsDefault { get; set; }
        public bool IsCustom { get; set; }
        public bool IsNewOrUpdated { get; set; } = false;
        public bool Delete { get; set; } = false;
        public string PreviousUrlSlug { get; set; }
        public Guid ExistingNodeSlugGuid { get; set; }
        public NodeUrlSlug()
        {

        }
    }
}
