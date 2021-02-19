using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicRouting
{

    public static class DynamicRoutingEvents
    {
        /// <summary>
        /// Overwrites the handling of finding a page based on the request.
        /// </summary>
        public static GetPageEventHandler GetPage;

        /// <summary>
        /// Allows overwrite of how to get the current culture
        /// </summary>
        public static GetCultureEventHandler GetCulture;

        /// <summary>
        /// Allows you to adjust the MVC Routing by modifying the Request Context
        /// </summary>
        public static RequestRoutingEventHandler RequestRouting;

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

            RequestRouting = new RequestRoutingEventHandler()
            {
                Name = "DynamicRoutingEvents.RequestRouting"
            };
        }
    }
}
