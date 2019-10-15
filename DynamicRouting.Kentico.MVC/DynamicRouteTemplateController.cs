using CMS.Base;
using CMS.DocumentEngine;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;
using Kentico.Web.Mvc;
using System.Web.Mvc;


namespace DynamicRouting.Kentico.MVC
{
    public class DynamicRouteTemplateController : PageTemplateController
    {
        /// <summary>
        /// Gets the node based on the current request url and then renders the template result.
        /// </summary>
        public ActionResult Index()
        {
            ITreeNode FoundNode = DynamicRouteHelper.GetPage();
            if (FoundNode != null)
            {
                HttpContext.Kentico().PageBuilder().Initialize(FoundNode.DocumentID);
                return new TemplateResult(FoundNode.DocumentID);
            }
            else
            {
                return new HttpNotFoundResult();
            }
        }
    }
}
