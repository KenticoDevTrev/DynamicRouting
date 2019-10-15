using System;

namespace DynamicRouting.Kentico.MVC
{
    /// <summary>
    /// This attribute indicates that Routes that map to this Controller should be enforced even if a page is found via Dynamic Routing.
    /// Example: If the Controller is ApiController and a request comes through as /api/GetItems, without this attribute if someone creates
    /// a page that has a UrlSlug of /api/GetItems then the request would be dynamically routed.  With this attribute, the request would
    /// properly go to your ApiController class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class StaticRoutePriorityAttribute : Attribute
    {
    }
}
