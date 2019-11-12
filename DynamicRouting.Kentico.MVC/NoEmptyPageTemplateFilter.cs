using Kentico.PageBuilder.Web.Mvc.PageTemplates;
using System.Collections.Generic;
using System.Linq;

namespace DynamicRouting.Kentico.MVC
{
    /// <summary>
    /// Removes the Empty.Template always from the options, thus disabling it.
    /// </summary>
    public class NoEmptyPageTemplateFilter : IPageTemplateFilter
    {
        public IEnumerable<PageTemplateDefinition> Filter(IEnumerable<PageTemplateDefinition> pageTemplates, PageTemplateFilterContext context)
        {
            // Remove Empty.Template always
            return pageTemplates.Where(t => !GetTemplates().Contains(t.Identifier));
        }

        // Gets all page templates that are allowed for landing pages
        public IEnumerable<string> GetTemplates() => new string[] { "Empty.Template" };
    }
}
