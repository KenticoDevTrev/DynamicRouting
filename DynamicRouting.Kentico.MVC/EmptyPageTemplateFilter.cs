using Kentico.PageBuilder.Web.Mvc.PageTemplates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicRouting.Kentico.MVC
{
    /// <summary>
    /// This is to prevent a template from automatically being assigned.  If there is at least 1 non-empty template that is available, this will add the "Empty" template as an option so the user can select.
    /// </summary>
    public class EmptyPageTemplateFilter : IPageTemplateFilter
    {
        public IEnumerable<PageTemplateDefinition> Filter(IEnumerable<PageTemplateDefinition> pageTemplates, PageTemplateFilterContext context)
        {
            // only add empty option if there is 1 non empty template remaining, so user has to choose.
            var NonEmptyTemplates = pageTemplates.Where(t => !GetTemplates().Contains(t.Identifier));
            if (NonEmptyTemplates.Count() > 0)
            {
                return pageTemplates;
            }
            else
            {
                // Remove the empty template as an option
                return pageTemplates.Where(t => !GetTemplates().Contains(t.Identifier));
            }
        }

        // Gets all page templates that are allowed for landing pages
        public IEnumerable<string> GetTemplates() => new string[] { "Empty.Template" };
    }
}
