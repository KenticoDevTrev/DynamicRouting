using Kentico.PageBuilder.Web.Mvc.PageTemplates;

// This is used as a trigger for the given page to ignore Page Templates, as Kentico by default requires a page template if one is selectable.
[assembly: RegisterPageTemplate("Empty.Template", "No Template", customViewName: "", Description = "No Template (Use standard Routing)", IconClass = "icon-modal-close")]