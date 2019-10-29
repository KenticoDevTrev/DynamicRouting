# DynamicRouting - STILL IN DEV
Dynamic Routing in Kentico using a Routing Table and Assembly Attribute.  This module is not yet complete, but progress is being made!

# First time Setup Preperation for development

## Mother Application
1. Install a blank Kentico Site locally
1. Install the [DynamicRouting.Kentico](https://www.nuget.org/packages/DynamicRouting.Kentico/12.0.0) NuGet Package on the Kentico Mother and run it.
1. Optionally, import the [DynamicRoutingStarterSite.zip](https://github.com/KenticoDevTrev/DynamicRouting/blob/master/DynamicRoutingStarterSite.zip) site in Kentico, this contains Dynamic Routing Test page types and pages.

## MVC Application
1. Pull down the [MVC Tools - Dynamic Routing Branch](https://github.com/KenticoDevTrev/KenticoTools/tree/DynamicRouting) locally.
1. Unzip the UnTrackedConfigFiles.zip Files, and adjust the `ConnectionStrings.config` to add your database connection info..  Optionally you can adjust the AppSettings.config `CustomAdminUrl` and `CMSCiRepositoryPath` as well.

## Setup Dynamic Routing Projects
1. Fork the Dynamic Routing master branch into your local repository (or just download if not going to modify yourself and just testing)

### Mother Application
1. Open the WebApp.sln that contains your `Mother` solution, include the projects [DynamicRouting.Kentico.Base](https://github.com/KenticoDevTrev/DynamicRouting/tree/master/DynamicRouting.Kentico.Base) and [DynamicRouting.Kentico](https://github.com/KenticoDevTrev/DynamicRouting/tree/master/DynamicRouting.Kentico.Mother) from the Git Repo
1. Configure so your WebApp `CMSApp` project references the `DynamicRouting.Kentico` project, and the `DynamicRouting.Kentico` project references DynamicRouting.Kentico.Base
1. Add a class to your Old_App_Code containing the [MacroMethodForDynamicRoutingMacroPageType.txt](https://github.com/KenticoDevTrev/DynamicRouting/blob/master/DynamicRouting.Kentico.Mother/MacroMethodForDynamicRoutingMacroPageType.txt) code, this is used on the Dynamic Routing (Macro) Page type.
1. Note, you may have to remove the libraries that are in the bin from the nuget package.

### MVC Web App
1. Open the ToolsMVC.sln that contains your `MVC Site` solution, include the projects [DynamicRouting.Kentico.Base](https://github.com/KenticoDevTrev/DynamicRouting/tree/master/DynamicRouting.Kentico.Base) and [DynamicRouting.Kentico.MVC](https://github.com/KenticoDevTrev/DynamicRouting/tree/master/DynamicRouting.Kentico.MVC) from the Git Repo
1. Configure so your WebApp `CMSApp` project references the `DynamicRouting.Kentico.MVC` project, and the `DynamicRouting.Kentico`) project references DynamicRouting.Kentico.Base

If you are installing this into your own site, the only thing required to hook this up is to modify your RouteConfig.cs and include the following Routes:

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

## Build
1. Rebuild your solutions and ensure all the nuget packages are restored.
1. In the Mother, Add the Dynamic Routing module to your site
1. Then, reload and you should see a `Dynamic Routing` UI Element, you can now run the Build on the site to generate the Url Slugs for the firs time (after that it automatically generates)

# Basic Usage
The Dynamic Routing runs off of 3 pieces.

1. Url Slugs: The Url that the page will be rendered at, generated from the Page Type's URL Pattern.  You can use `{% ParentUrl() %}` in the URL Pattern to substitute the page's parent URL Slug (ex: URL Pattern: `{% ParentUrl() %}/{% Name %}` )
2. a GetPage() Helper that takes the current (or given) URL and finds the page that has a matching slug (and thus also have the Class Name)
3. `DynamicRoutingAttribute` Assembly tags that then link Page Types to Controller, Controller+Action, View, or View+Model

Keep in mind that if a page has a Page Template, it will render the Page Template instead.

## Mother Application
Once you have gone to Dynamic Routing and clicked the `Rebuild Site` button, you should now be able to add and edit pages.  URL Slugs are automatically handled when pages are Inserted, Updated, Culture Insert, Moved, Sorted/Changed Order, Copied, or Published.  See [DynamicRouteInitializationModule.cs](https://github.com/KenticoDevTrev/DynamicRouting/blob/master/DynamicRouting.Kentico.Base/Helpers/DynamicRouteInitializationModule.cs).

## MVC Application
To leverage, simply add your DynamicRouting assembly tags, here are some samples:

```csharp
[assembly: DynamicRouting(typeof(ListController), new string[] { Listing.CLASS_NAME }, nameof(ListController.Listing) )]
[assembly: DynamicRouting(typeof(ListController), new string[] { ListItem.CLASS_NAME }, nameof(ListController.ListItem))]
[assembly: DynamicRouting("DynamicRoutingTesting/DynamicNoModel", new string[] { "DynamicRouting.Macro" }, false)]
[assembly: DynamicRouting("DynamicRoutingTesting/DynamicITreeNodeModel", new string[] { "DynamicRouting.Sibling" }, true)]
[assembly: DynamicRouting("DynamicRoutingTesting/DynamicModel", typeof(NodeAliasPath), NodeAliasPath.CLASS_NAME)]
```

# BETA TESTING NEEDS
Right now the biggest thing i need is testing.  I've already done some preliminary testing, except for Site Culture Changes, Class Url Pattern Changes, and some configuration settings, but the Document stuff should be operational.  Please list any issues in the Issue list you come across.
