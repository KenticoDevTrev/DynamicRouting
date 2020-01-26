# DynamicRouting - MVC Only
Dynamic Routing in Kentico using Assembly Attribute.  This module is the MVC Stand Alone package, and should only be used if you do not wish to use the normal DynamicRouting.Kentico packages (which include the Url Slug Generation logic).

This package requires you provide the logic to Get the Page based on the request using the DynamicRoutingEvents.GetPage [global event](https://docs.kentico.com/k12sp/custom-development/handling-global-events) hooks.

## Installation

1. Install the NugetPackage DynamicRouting.Kentico.MVCOnly into your MVC Site
1. Hook onto the DynamicRoutingEvents.GetPage (Before or after) [global event](https://docs.kentico.com/k12sp/custom-development/handling-global-events)
1. Add your logic to find the page based on the given `GetPageEventArgs`, see "Sample GetPage Logic" below
1. Adjust your route.config to include your dynamic routing, see "Route Configuration" below
1. Add [assembly: DynamicRouting] Attribute tags to define your Dynamic Routing

### Route Configuring
In order for MVC to implement your Dynamic Routing, you must adjust your Route Configuration.  Below is an example of what you would have.  

The `StaticRoutePriorityConstraint` allows you to define Controllers as taking priority over any dynamic route match, otherwise the system will look for Dynamic Route matches before normal controller lookup.

```csharp
public static void RegisterRoutes(RouteCollection routes)
    {
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

        // Maps routes to Kentico HTTP handlers and features enabled in ApplicationConfig.cs
        // Always map the Kentico routes before adding other routes. Issues may occur if Kentico URLs are matched by a general route, for example images might not be displayed on pages
        routes.Kentico().MapRoutes();

        // Redirect to administration site if the path is "admin"
        routes.MapRoute(
            name: "Admin",
            url: "admin",
            defaults: new { controller = "AdminRedirect", action = "Index" }
        );

        // If a normal MVC Route is found and it has priority, it will take it, otherwise it will bypass.
        var route = routes.MapRoute(
             name: "DefaultIfPriority",
             url: "{controller}/{action}/{id}",
             defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
             constraints: new { ControlIsPriority = new StaticRoutePriorityConstraint() }
         );

        // If the Page is found, will handle the routing dynamically
        route = routes.MapRoute(
            name: "DynamicRouting",
            url: "{*url}",
            defaults: new { defaultcontroller = "HttpErrors", defaultaction = "Index" },
            constraints: new { PageFound = new DynamicRouteConstraint() }
        );
        route.RouteHandler = new DynamicRouteHandler();

        // This will again look for matching routes or node alias paths, this time it won't care if the route is priority or not.
        routes.MapRoute(
             name: "Default",
             url: "{controller}/{action}/{id}",
             defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
        );

        // Finally, 404
        routes.MapRoute(
            name: "PageNotFound",
            url: "{*url}",
            defaults: new { controller = "HttpErrors", action = "Index" }
            );
    }
```

### Sample GetPage Logic

Since this is a stand alone Dynamic Routing, you have to tell Kentico how you wish to translate a Url to a Page.  

```csharp
using Boilerplate;
using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using DynamicRouting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

[assembly: RegisterModule(typeof(ToolsMVCInitializationModule))]

namespace Boilerplate
{
    public class ToolsMVCInitializationModule : Module
    {
        // Module class constructor, the system registers the module under the name "CustomInit"
        public ToolsMVCInitializationModule()
            : base("ToolsMVCInitializationModule")
        {
        }

        // Contains initialization code that is executed when the application starts
        protected override void OnInit()
        {
            base.OnInit();
            DynamicRoutingEvents.GetPage.Before += GetPage_Before;
        }

        private void GetPage_Before(object sender, GetPageEventArgs e)
        {
            // Find page based on a NodeAliasPath match
            if (e.FoundPage == null)
            {                
                string NodeAliasPath = e.RelativeUrl;
                string ClassName = GetPageClass(e, NodeAliasPath);
                e.FoundPage = GetPage(e, NodeAliasPath, ClassName);
            }
        }

        private string GetPageClass(GetPageEventArgs e, string NodeAliasPath)
        {
            return CacheHelper.Cache(cs =>
            {
                var Page = DocumentHelper.GetDocuments()
               .Path(NodeAliasPath, PathTypeEnum.Single)
               .CombineWithAnyCulture()
               .CombineWithDefaultCulture()
               .Columns("ClassName, NodeID")
               .OnSite(e.SiteName)
               .FirstOrDefault();
                if (cs.Cached && Page != null)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(new string[]
                    {
                        $"nodeid|{Page.NodeID}"
                    });
                }
                return Page?.ClassName;
            }, new CacheSettings(1440, "DynamicRoutingGetPageClass", NodeAliasPath, e.SiteName));
        }

        private ITreeNode GetPage(GetPageEventArgs e, string NodeAliasPath, string ClassName)
        {
            return CacheHelper.Cache(cs =>
            {
                var LookupQuery = DocumentHelper.GetDocuments(ClassName)
               .Path(NodeAliasPath, PathTypeEnum.Single)
               .CombineWithAnyCulture()
               .CombineWithDefaultCulture()
               .OnSite(e.SiteName);
                if (!string.IsNullOrWhiteSpace(e.ColumnsVal) && e.ColumnsVal.IndexOf('*') == -1)
                {
                    // Ensure DocumentID is on there for cache
                    string Columns = string.Join(", ",
                        (e.ColumnsVal + ",DocumentID")
                        .Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim().ToLower())
                        .Distinct()
                        .ToList());
                    LookupQuery.Columns(Columns);
                }
                if (!string.IsNullOrWhiteSpace(e.Culture))
                {
                    LookupQuery.Culture(e.Culture);
                }
                if (e.PreviewEnabled)
                {
                    LookupQuery.LatestVersion(true)
                        .Published(false);
                }
                else
                {
                    LookupQuery.PublishedVersion();
                }

                var Page = LookupQuery.FirstOrDefault();

                if (cs.Cached && Page != null)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(new string[]
                    {
                        $"documentid|{Page.DocumentID}"
                    });
                }
                return Page;
            }, new CacheSettings(e.PreviewEnabled ? 0 : 1440, "DynamicRoutingGetPage", NodeAliasPath, ClassName, e.Culture, e.DefaultCulture, e.ColumnsVal));
            
        }
    }
}
```

If you use a `/{% DocumentCulture %}/{% NodeALiasPath %}` format instead, you can use the below:

```csharp
using Boilerplate;
using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using DynamicRouting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

[assembly: RegisterModule(typeof(ToolsMVCInitializationModule))]

namespace Boilerplate
{
    public class ToolsMVCInitializationModule : Module
    {
        // Module class constructor, the system registers the module under the name "CustomInit"
        public ToolsMVCInitializationModule()
            : base("ToolsMVCInitializationModule")
        {
        }

        // Contains initialization code that is executed when the application starts
        protected override void OnInit()
        {
            base.OnInit();
            DynamicRoutingEvents.GetCulture.After += GetCulture_After;
            DynamicRoutingEvents.GetPage.Before += GetPage_Before;
        }

        private void GetCulture_After(object sender, GetCultureEventArgs e)
        {
            // If beginning of the Url matches a Culture code, then set that as culture
            string PossibleCulture = e.Request.Url.AbsolutePath.Trim('/').Split('/')[0];
            Regex CultureMatch = new Regex("^[a-z]{2,3}(?:-[A-Z]{2,3}(?:-[a-zA-Z]{4})?)?$");
            if (CultureMatch.IsMatch(PossibleCulture))
            {
                e.Culture = PossibleCulture;
            }
        }

        private void GetPage_Before(object sender, GetPageEventArgs e)
        {
            // Find page based on a NodeAliasPath match
            if (e.FoundPage == null)
            {
                // Remove DocumentCulture From RelativeUrl so we get the path
                string NodeAliasPath = e.RelativeUrl;
                string CultureLookup = $"/{e.Culture}/";
                if (NodeAliasPath.ToLower().IndexOf($"/{e.Culture.ToLower()}/") == 0)
                {
                    // Remove /Culture-Code from Path
                    NodeAliasPath = NodeAliasPath.Substring(CultureLookup.Length - 1);
                }

                string ClassName = GetPageClass(e, NodeAliasPath);
                e.FoundPage = GetPage(e, NodeAliasPath, ClassName);
            }
        }

        private string GetPageClass(GetPageEventArgs e, string NodeAliasPath)
        {
            return CacheHelper.Cache(cs =>
            {
                var Page = DocumentHelper.GetDocuments()
               .Path(NodeAliasPath, PathTypeEnum.Single)
               .CombineWithAnyCulture()
               .CombineWithDefaultCulture()
               .Columns("ClassName, NodeID")
               .OnSite(e.SiteName)
               .FirstOrDefault();
                if (cs.Cached && Page != null)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(new string[]
                    {
                        $"nodeid|{Page.NodeID}"
                    });
                }
                return Page?.ClassName;
            }, new CacheSettings(1440, "DynamicRoutingGetPageClass", NodeAliasPath, e.SiteName));
        }

        private ITreeNode GetPage(GetPageEventArgs e, string NodeAliasPath, string ClassName)
        {
            return CacheHelper.Cache(cs =>
            {
                var LookupQuery = DocumentHelper.GetDocuments(ClassName)
               .Path(NodeAliasPath, PathTypeEnum.Single)
               .CombineWithAnyCulture()
               .CombineWithDefaultCulture()
               .OnSite(e.SiteName);
                if (!string.IsNullOrWhiteSpace(e.ColumnsVal) && e.ColumnsVal.IndexOf('*') == -1)
                {
                    // Ensure DocumentID is on there for cache
                    string Columns = string.Join(", ",
                        (e.ColumnsVal + ",DocumentID")
                        .Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim().ToLower())
                        .Distinct()
                        .ToList());
                    LookupQuery.Columns(Columns);
                }
                if (!string.IsNullOrWhiteSpace(e.Culture))
                {
                    LookupQuery.Culture(e.Culture);
                }
                if (e.PreviewEnabled)
                {
                    LookupQuery.LatestVersion(true)
                        .Published(false);
                }
                else
                {
                    LookupQuery.PublishedVersion();
                }

                var Page = LookupQuery.FirstOrDefault();

                if (cs.Cached && Page != null)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(new string[]
                    {
                        $"documentid|{Page.DocumentID}"
                    });
                }
                return Page;
            }, new CacheSettings(e.PreviewEnabled ? 0 : 1440, "DynamicRoutingGetPage", NodeAliasPath, ClassName, e.Culture, e.DefaultCulture, e.ColumnsVal));
            
        }
    }
}
```

## Dynamic Routing Attributes
Add DynamicRouting attribute tags to link ClassNames with either Controller, Controller + Action, View, View + ITreeNode model, or View + Model (that is of type ITreeNode)

Here are some samples:
```csharp
[assembly: DynamicRouting(typeof(ListController), new string[] { Listing.CLASS_NAME }, nameof(ListController.Listing) )]
[assembly: DynamicRouting(typeof(ListController), new string[] { ListItem.CLASS_NAME }, nameof(ListController.ListItem))]
[assembly: DynamicRouting("DynamicRoutingTesting/DynamicNoModel", new string[] { "My.Class" }, false)]
[assembly: DynamicRouting("DynamicRoutingTesting/DynamicITreeNodeModel", new string[] { "My.OtherClass" }, true)]
[assembly: DynamicRouting("DynamicRoutingTesting/DynamicModel", typeof(MyPageTypeModel), MyPageTypeModel.CLASS_NAME)]
```

## DynamicRoutingEvents
I have also included 3 Global Event hooks for you to leverage. DynamicRoutingEvents.GetPage.Before/After, DynamicRoutingEvents.GetCulture.Before/After, and DynamicRoutingEvents.RequestRouting.Before/After, which allow you to customize the logic of getting the page or the culture (in case you wish to implement some custom functionality), or the Routing itself.

# Note on automatic Model Casting
In order for `DynamicRouteHelper.GetPage()` to return the properly typed page (with a Type that matches your page type's generated code), that generated page type's class must be in a discoverable assembly, either the existing project, or in a separate class library that has the `[assembly: AssemblyDiscoverable]` attribute in it's AssemblyInfo.cs.  Otherwise it will return a TreeNode only and won't be able to convert to your Page Type Specific model dynamically, adn will throw an `InvalidCastException`.

# Contributions, but fixes and License
Feel free to Fork and submit pull requests to contribute.

You can submit bugs through the issue list and i will get to them as soon as i can, unless you want to fix it yourself and submit a pull request!

Check the License.txt for License information

# Compatability
Can be used on any Kentico 12 SP site (hotfix 29 or above).
