using CMS.Base;
using DynamicRouting;

namespace DynamicRouting
{
    public class RequestRoutingEventHandler : AdvancedHandler<RequestRoutingEventHandler, RequestRoutingEventArgs>
    {
        public RequestRoutingEventHandler()
        {

        }

        public RequestRoutingEventHandler StartEvent(RequestRoutingEventArgs RequestArgs)
        {
            return base.StartEvent(RequestArgs);
        }

        public void FinishEvent()
        {
            base.Finish();
        }
    }
}
