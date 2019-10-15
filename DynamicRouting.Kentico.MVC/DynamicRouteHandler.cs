using System.Web;
using System.Web.Routing;

namespace DynamicRouting.Kentico.MVC
{
    public class DynamicRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new DynamicHttpHandler(requestContext);
        }
    }
}
