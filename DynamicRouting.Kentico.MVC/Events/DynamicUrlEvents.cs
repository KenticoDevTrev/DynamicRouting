using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicRouting
{

    public static class DynamicRoutingEvents
    {
        public static GetPageEventHandler GetPage;

        public static GetCultureEventHandler GetCulture;

        static DynamicRoutingEvents()
        {
            GetPage = new GetPageEventHandler()
            {
                Name = "DynamicRoutingEvents.GetPage"
            };

            GetCulture = new GetCultureEventHandler()
            {
                Name = "DynamicRoutingEvents.GetCulture"
            };
        }
    }
}
