using CMS;
using CMS.DataEngine;
using DynamicRouting.Kentico.MVC;

[assembly: RegisterModule(typeof(DynamicRouteInitializationModule_Mother))]

namespace DynamicRouting.Kentico.MVC
{
    public class DynamicRouteInitializationModule_Mother : Module
    {
        public DynamicRouteInitializationModule_Mother() : base("DynamicRouteInitializationModule_Mother")
        {

        }

        protected override void OnInit()
        {
            base.OnInit();

            // Call OnInit of the Base
            var BaseInitializationModule = new DynamicRouteInitializationModule_Base();
            BaseInitializationModule.Init();
        }
    }
}
