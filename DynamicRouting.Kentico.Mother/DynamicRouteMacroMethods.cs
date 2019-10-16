using CMS;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.SiteProvider;
using DynamicRouting.Kentico;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: RegisterExtension(typeof(DynamicRouteMacroMethods), typeof(UtilNamespace))]

namespace DynamicRouting.Kentico
{
    public class DynamicRouteMacroMethods : MacroMethodContainer
    {
        [MacroMethod(typeof(string), "Retrieves the Parent's Url Slug", 0)]
        public static object ParentUrl(EvaluationContext context, params object[] parameters)
        {
            // Based on the Macro Resolver which has the TreeNode Data, return the ParentUrl
            int NodeID = ValidationHelper.GetInteger(context.Resolver.ResolveMacros("{% NodeID %}"), 0);
            int NodeParentID = ValidationHelper.GetInteger(context.Resolver.ResolveMacros("{% NodeParentID %}"), 0);
            string Culture = ValidationHelper.GetString(context.Resolver.ResolveMacros("{% DocumentCulture %}"), "en-US");
            string DefaultCulture = SiteContext.CurrentSite.DefaultVisitorCulture;
            return CacheHelper.Cache(cs =>
            {
                UrlSlugInfo Slug = UrlSlugInfoProvider.GetUrlSlugs()
                .WhereEquals("UrlSlugNodeID", NodeParentID)
                .OrderBy($"case when UrlSlugCultureCode = '{Culture}' then 0 else 1 end, case when UrlSlugCultureCode = '{DefaultCulture}' then 0 else 1 end")
                .Columns("UrlSlug")
                .FirstOrDefault();
                if(cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency("dynamicrouting.urlslug|all");
                }
                return Slug != null ? Slug.UrlSlug : "";
            }, new CacheSettings(1440, "GetUrlSlug", NodeParentID, Culture, DefaultCulture));
            
        }
    }
}
