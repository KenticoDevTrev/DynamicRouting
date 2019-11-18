using CMS;
using CMS.DataEngine;
using CMS.Modules;
using DynamicRouting.Kentico.Mother;

[assembly: RegisterModule(typeof(DynamicRouteInitializationModule_Mother))]

namespace DynamicRouting.Kentico.Mother
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

            // Nuget Manifest Build
            ModulePackagingEvents.Instance.BuildNuSpecManifest.After += BuildNuSpecManifest_After;
        }

        private void BuildNuSpecManifest_After(object sender, BuildNuSpecManifestEventArgs e)
        {
            if (e.ResourceName.Equals("DynamicRouting.Kentico", System.StringComparison.InvariantCultureIgnoreCase))
            {
                e.Manifest.Metadata.Owners = "Kentico Community";
                e.Manifest.Metadata.ProjectUrl = "https://github.com/KenticoDevTrev/DynamicRouting";
                e.Manifest.Metadata.IconUrl = "http://www.kentico.com/favicon.ico";
                e.Manifest.Metadata.Copyright = "Copyright 2019 Kentico Community";
                e.Manifest.Metadata.Title = "Dynamic Routing for Kentico v12 SP";
                e.Manifest.Metadata.ReleaseNotes = "Fixed Column Reference error on internal helper";
            }
        }
    }
}
