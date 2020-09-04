
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
3. Register either the `EmptyPageTemplateFilter()` or `NoEmptyPageTemplateFilter()` as the last PageBuilderFilter to enable or disable the Empty Template system (see **Page Templates and Empty Template** section on this readme)
3. Add DynamicRouting assembly tags as needed

### Route Configuring
In order for MVC to implement your Dynamic Routing, you must adjust your Route Configuration.  Below is an example of what you would have.  

The `StaticRoutePriorityConstraint` allows you to define Controllers as taking priority over any dynamic route match, otherwise the system will look for Dynamic Route matches before normal controller lookup.  This is useful if you do not want someone creating a page that may match your MVC route and overwriting it.

```csharp
public static void RegisterRoutes(RouteCollection routes)
    {
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

        routes.Kentico().MapRoutes();
    
        // This will honor Attribute Routing in MVC [Route("")] and [RoutePrefix("")] over Dynamic Routing
        //<see href="https://devblogs.microsoft.com/aspnet/attribute-routing-in-asp-net-mvc-5/">See Attribute Routing</see>
        routes.MapMvcAttributeRoutes();

        // Redirect to administration site if the path is "admin"
        // Can also replace this with the [Route("Admin")] on your AdminRedirectController's Index Method
        routes.MapRoute(
            name: "Admin",
            url: "admin",
            defaults: new { controller = "AdminRedirect", action = "Index" }
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
// View only, no model passed, Page Builder Widgets are Enabled, the document ID won't be passed in the Output cache dependencies and output caching will be disabled
[assembly: DynamicRouting("DynamicRoutingTesting/DynamicNoModel", new string[] { "My.Class" }, false, includeDocumentInOutputCache: false, useOutputCaching: false)]
// View + ITreeNode model, Page Builder Widgets are Enabled
[assembly: DynamicRouting("DynamicRoutingTesting/DynamicITreeNodeModel", new string[] { "My.OtherClass" }, true)]
// View + Typed Model, Model must be of type ITreeNode, Page Builder Widgets are Enabled, the response will be Output Cached, and since includeDocumentInOutputCache is by default true, the documentid|### is added to the output's cache dependencies
[assembly: DynamicRouting("DynamicRoutingTesting/DynamicModel", typeof(MyPageTypeModel), MyPageTypeModel.CLASS_NAME, useOutputCaching: true)]
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
I have also included 3 Global Event hooks for you to leverage.  DynamicRoutingEvents.GetPage.Before/After, DynamicRoutingEvents.GetCulture.Before/After, and DynamicRoutingEvents.RequestRouting.Before/After, which allow you to customize the logic of getting the page or the culture (in case you wish to implement some custom functionality), or the Routing itself.

## Page Templates and Empty Template
Kentico Page Templates are fully supported, and any page that is found that has a Page Template will automatically be routed to the Page Template instead of the predetermined Dynamic Routing.

Since Kentico's Default Page Template behavior is that if you only have 1 Page Template, it will automatically assign that, I have added an `Empty` Template that will appear as an option if Page Template's are available.  Selecting this will trigger the normal Dynamic Routing to occur instead of sending the Page to a Page Template.  This way you can allow the user to either select a Page Template you created, or just let the default Dynamic Routing occur.

You need to either Enable or Remove this template through the `PageBuilderFilters.PageTemplates` feature.  There are two Filters that are available in the `DynamicRouting.Kentico.MVC`, `EmptyPageTemplateFilter` will enable the logic that if there is a template available, the Empty one will be presented along with it, and `NoEmptyPageTemplateFilter` will remove the Empty template no matter what.

IF you use the `EmptyPageTemplateFilter()`, know it MUST be placed last on the list.

Please refer to [Page Template Filtering](https://docs.kentico.com/k12sp/developing-websites/page-builder-development/developing-page-templates-in-mvc/filtering-page-templates-in-mvc), your code should look like this in your `Global.asax.cs':

````csharp
protected void Application_Start()
    {
        ...

        RegisterPageTemplateFilters();
    }

    private void RegisterPageTemplateFilters()
    {
        PageBuilderFilters.PageTemplates.Add(new MyOtherFilters());
        
        // Enabled, This must be last!
        PageBuilderFilters.PageTemplates.Add(new EmptyPageTemplateFilter());
        // Disabled
        // PageBuilderFilters.PageTemplates.Add(new NoEmptyPageTemplateFilter());
    }

````
## Required Columns
It is often best to minimize the columns returned from your TreeNodes, so you do not send back extra data.  Dynamic Routing overwrites the `TreeNode.RelativeUrl` and does a lookup on the Url slug when retrieving this value.  It requires 2 fields, `NodeID` and `DocumentCulture` for this so you may need to include those.

If you do any overwriting of the GlobalEvents It also uses `DocumentID` and `ClassName` when it's doing it's own internal routing, which probably won't concern you unless you are overwriting the GlobalEvents for DynamicRouting.

## Caching
As of version 12.29.11, Output Caching support has been added.

### For Dynamic Routes to Controllers
If your Dynamic Route goes to a custom Controller, calling the `DynamicRouteHelper.GetPage()` will by default add the `documentid|<FoundDocID>` Cache Dependency key to the response.  This means if you add the [OutputCache] attribute on your action, it will clear when the page is updated.  While this is enabled by default, you can disable it by passing in a false for the property `AddPageToCacheDependency`

### For Automatic Routes
Two new properties have been added to the DynamicRoute attribute for View / View+Model routes, these are `includeDocumentInOutputCache` and `useOutputCaching`

`IncludeDocumentInOutputCache` is true by default, but you can disable it if you wish.
`useOutputCaching` is false by default, and if you enable it, it will use the `DynamicRouteCachedController` for it's rendering, which has this output cache on it's methods: `[OutputCache(CacheProfile = "DynamicRouteController")]`

**IMPORTANT**: If you use the Cached version, you *must* implement the `outputCacheProfile` of `DynamicRouteController`, this is how you can control how these are cached.  Add the below to your `<configuration><system.web>` section in your MVC Site's web.config:

```
<configuration>
  <system.web>
    <caching>
      <outputCacheSettings>
        <outputCacheProfiles>
          <add name="DynamicRouteController" duration="60" varyByParam="none"/>
        </outputCacheProfiles>
      </outputCacheSettings>
    </caching>
    ...
  </system.web>
  ...
</configuration
```

### For Templates
Since Templates are handled by Kentico, any output caching must be handled by the Page Template itself.  The `DocumentID` is added to the response Cache Dependency by default for you.   

If you wish to disable this behavior, you can use the Global Event`DynamicRoutingEvents.RequestRouting.Before` and set the `RequestRoutingEventArgs.Configuration.IncludeDocumentInOutputCache` to false if the `Configuration.ControllerName.Equals("DynamicRouteTemplate", StringComparison.InvariantCultureIgnoreCase); ` 

# Installing on Additional Environments
As with any Kentico module that is available in a NuGet package, if you install this on one environment (ex "Dev") and wish to push this to the other environments, you will need to either...

1. Install the Nuget Package on the other environments as well

or

1. Push the files (including 2 libraries in the bin) to the new environment, then go to Site -> Import Site or Object and select and import the `DynamicRouting.Kentico_12.29.3.zip`file found in the NuGet package (you can change .Nuget to .zip, extract it and find this file in `content\App_Data\CMSModules\DynamicRouting.Kentico\Install` and install, this will install the database objects

In both cases, you should go to the Dynamic Routing module within the Kentico Admin, and under Quick Operations rebuild the site's url slugs on each environment.

# IDynamicRouteHelper Interface
The `DynamicRouteHelper` static class has been obsoleted as of 12.29.12, and instead it is recommended that use the `IDynamicRouteHelper` interface.  

## Setup Using Dependency Injection
You can wire it up with Autofac or a similar dependency injection system using a command similar to this:

``` csharp
var builder = new ContainerBuilder();
...
builder.RegisterType(typeof(BaseDynamicRouteHelper)).As(typeof(IDynamicRouteHelper));
...
// Autowire Property Injection for controllers (can't have constructor injection)
var allControllers = Assembly.GetExecutingAssembly().GetTypes().Where(type => typeof(Controller).IsAssignableFrom(type));
foreach (var controller in allControllers)
{
	builder.RegisterType(controller).PropertiesAutowired();
}
```

Then you can leverage it in your classes like this:

``` csharp

public class ListController : Controller
{
	private IDynamicRouteHelper mDynamicRouteHelper;
	public ListController(IDynamicRouteHelper mDynamicRouteHelper)
	{
		this.mDynamicRouteHelper = mDynamicRouteHelper;
	}
	public ActionResult Listing()
	{
		var Page = mDynamicRouteHelper.GetPage();
		...
	}
}
```

## Manual Usage (not recommended)
If you do not have dependency injection or wish to simply call the logic normally, you can use the default Implentation `new BaseDynamicRouteHelper().GetPage()`

# Note on automatic Model Casting
In order for `DynamicRouteHelper.GetPage()` to return the properly typed page (with a Type that matches your page type's generated code), that generated page type's class must be in a discoverable assembly, either the existing project, or in a separate class library that has the `[assembly: AssemblyDiscoverable]` attribute in it's AssemblyInfo.cs.  Otherwise it will return a TreeNode only and won't be able to convert to your Page Type Specific model dynamically, adn will throw an `InvalidCastException`.

# Note on TreeNode.RelativeUrl
Dynamic Routing overwrites the [RelativeUrl](https://github.com/KenticoDevTrev/DynamicRouting/blob/master/DynamicRouting.Kentico.Base/Overrides/DocumentUrlProviderOverride.cs) property of TreeNode objects.  It does this through a query that uses the `NodeID` and `DocumentCulture` properties.  Be sure your node has these 2 fields populated in order to retrieve the proper path (in case you are selecting only certain columns)

# Acknowledgement, Contributions, bug fixes and License
I want to give a shout out to Sean G. Wright for his help with the MVC routing portion of things.

Also a big thanks to [Heartland Business Systems](https://www.hbs.net) for giving me the time to work on this, they have in essence funded this extension (and i've sunk over 70 hours into building it).

I really hope this module helps the community out.

 I've tested this module to the best of my ability, however if you do find a bug, please feel free to submit an Issue list item, and also feel free to fork and do a pull request, i can repackage it up if you add something that is beneficial to everyone.

Check the License.txt for License information, but in general this tool is free for all to use.

# Compatability
Can be used on any Kentico 12 SP site (hotfix 29 or above).  This was created with the Kentico team so upgrading shouldn't be a headache.
