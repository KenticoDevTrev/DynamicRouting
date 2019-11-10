using CMS.Base;
using DynamicRouting;

namespace DynamicRouting
{
    public class GetCultureEventHandler : AdvancedHandler<GetCultureEventHandler, GetCultureEventArgs>
    {
        public GetCultureEventHandler()
        {

        }

        public GetCultureEventHandler StartEvent(GetCultureEventArgs CultureArgs)
        {
            return base.StartEvent(CultureArgs);
        }

        public void FinishEvent()
        {
            base.Finish();
        }
    }
}
