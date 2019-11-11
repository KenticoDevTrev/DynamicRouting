# DynamicRouting
Dynamic Routing for Kentico is a two part system that allows you to automatically handle page requests and route them to certain actions based on the Page Type of the current page.  This is done through the automatic generation of Url Slugs that help Kentico identify which Page you are requesting (based on the Url), and Assembly Attribute tags to route that page appropriately.

While the main module consistant of a Kentico "Mother" Nuget package ([DynamicRouting.Kentico](https://www.nuget.org/packages/DynamicRouting.Kentico/12.29.0)) and an MVC Nuget Package ([DynamicRouting.Kentico.MVC](https://www.nuget.org/packages/DynamicRouting.Kentico.MVC/12.29.0)), there is a stand alone MVC package, housed on a [separate branch](https://github.com/KenticoDevTrev/DynamicRouting/tree/DynamicRouting-Only) that you can use if you want the Dynamic Routing Attributes without the Automatic Url Slug Generation.

## Installation

### Installing on the Admin ("Mother")
1. Install the NuGet Package [DynamicRouting.Kentico](https://www.nuget.org/packages/DynamicRouting.Kentico/12.29.0) on your Kentico Admin instance, and run the site.
1. Go to Modules within Kentico's Interface, edit Dynamic Routing, and go to Sites and add the module to the current site.
1. If you wish, create Roles and assign the Permissions "Read", "Modify" or "Manage Url Slug" appropriately (Manage Url Slug is needed for users to customize Url Slugs on pages), and assign the Url Slugs UI element under CMS - Adminstration - Content Management - Pages - Edit - Properties - Url Slugs
1. Configure Settings in Settings - URLs and SEO - Dynamic Routing if needed
1. Lastly, go to Dynamic Routing UI element -> Quick Operations, and click `Rebuild Site` to generate your Url Slugs for the first time.

### Installing on the MVC Site
1. Install the NuGet Package [DynamicRouting.Kentico.MVC](https://www.nuget.org/packages/DynamicRouting.Kentico.MVC/12.29.0) on your MVC Site, and run the site.
2. Configure your RouteConfig.cs as seen below (under RouteConfig)
3. Add DynamicRouting assembly tags as needed

### Route Configuring
In order for MVC to implement your Dynamic Routing, you must adjust your Route Configuration.  Below is an example of what you would have.  

The `StaticRoutePriorityConstraint` allows you to define Controllers as taking priority over any dynamic route match, otherwise the system will look for Dynamic Route matches before normal controller lookup.  This is useful if you do not want someone creating a page that may match your MVC route and overwriting it.

```csharp
public static void RegisterRoutes(RouteCollection routes)
    {
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

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

## Dynamic Routing Attributes
Add DynamicRouting attribute tags to link ClassNames with either Controller, Controller + Action, View, View + ITreeNode model, or View + Model (that is of type ITreeNode).  

If you do a Controller or Controller + Action, you can call `DynamicRouteHelper.GetPage();` to get the page that the Dynamic Routing found.

Here are some samples:
```csharp
// Controller + Action
[assembly: DynamicRouting(typeof(ListController), new string[] { Listing.CLASS_NAME }, nameof(ListController.Listing) )]
// Controller + Action
[assembly: DynamicRouting(typeof(ListController), new string[] { ListItem.CLASS_NAME }, nameof(ListController.ListItem))]
// View only, no model passed, Page Builder Widgets are Enabled
[assembly: DynamicRouting("DynamicRoutingTesting/DynamicNoModel", new string[] { "My.Class" }, false)]
// View + ITreeNode model, Page Builder Widgets are Enabled
[assembly: DynamicRouting("DynamicRoutingTesting/DynamicITreeNodeModel", new string[] { "My.OtherClass" }, true)]
// View + Typed Model, Model must be of type ITreeNode, Page Builder Widgets are Enabled
[assembly: DynamicRouting("DynamicRoutingTesting/DynamicModel", typeof(MyPageTypeModel), MyPageTypeModel.CLASS_NAME)]
```

## Site Settings Settings
The Dynamic Routing Settings are relatively simple, and if you hover over them they will give you more in depth information.  In summary though:

* **Generate All Culture Variation for Site**: If checked, it will generate Url Slugs for all culture variations, even if the page itself doesn't exist.
* **Excluded Classes**: These classes will not be handled by the Dynamic Routing.
* **Url Slug Conflict Behavior**: If a conflict occurs, you can either have it Append Postfix, which is the `-(#)` at the end, or Cancel the Action will prevent the action from occurring.
* **Queue Error Behavior**: If an error occurs while building the Url Slugs in the background, if you wish future queue items to execute or wait.  You can check Queue status and errors through the Dynamic Routing UI Element -> Url Slug Queue

## Url Slug Formatting and {% ParentUrl() %}
Url Slugs are determined through the Page Type's `Url Pattern`. You are allowed to use any CMS_Document, CMS_Tree fields, along with any field of that Page Type itself (such as *BlogTitle* or *PageName*).

You can also use a new macro `{% ParentUrl() %}` which will automatically pull in the Parent Page's Url Slug, allowing you to have paths such as `{% ParentUrl() %}/{% MyPageTitle %}`

If you wish to restore Kentico Portal Engine's Default behavior, you should just use either `{% NodeAliasPath %}` as your Pattern, or `/{% DocumentCulture %}{% NodeAliasPath %}`.

## DynamicRoutingEvents
I have also included 2 Global Event hooks for you to leverage.  DynamicRoutingEvents.GetPage.Before/After, and DynamicRoutingEvents.GetCulture.Before/After, which allow you to customize the logic of getting the page or culture in case you wish to implement some custom functionality.

## Page Templates and Empty Template
Kentico Page Templates are fully supported, and any page that is found that has a Page Template will automatically be routed to the Page Template instead of the predetermined Dynamic Routing.

Since Kentico's Default Page Template behavior is that if you only have 1 Page Template, it will automatically assign that, I have added an `Empty` Template that will appear as an option if Page Template's are available.  Selecting this will trigger the normal Dynamic Routing to occur instead of sending the Page to a Page Template.  This way you can allow the user to either select a Page Template you created, or just let the default Dynamic Routing occur.

# Acknowledgement, Contributions, but fixes and License
I want to give a shout out to Sean G. Wright for his help with the MVC routing portion of things.

Also a big thanks to [Heartland Business Systems](https://www.hbs.net) for giving me the time to work on this, they have in essence funded this extension (and i've sunk over 70 hours into building it).

I really hope this module helps the community out.

 I've tested this module to the best of my ability, however if you do find a bug, please feel free to submit an Issue list item, and also feel free to fork and do a pull request, i can repackage it up if you add something that is beneficial to everyone.

Check the License.txt for License information, but in general this tool is free for all to use.

# Compatability
Can be used on any Kentico 12 SP site (hotfix 29 or above).  This was created for the Kentico team and Kentico 2020 should feature some variation of this tool, so upgrading shouldn't be a headache.
