using CMS;
using CMS.DataEngine;
using DynamicRouting.Kentico.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: RegisterModule(typeof(DynamicRouteInitializationModule_MVC))]

namespace DynamicRouting.Kentico.MVC
{
    public class DynamicRouteInitializationModule_MVC : Module
    {
        public DynamicRouteInitializationModule_MVC() : base("DynamicRouteInitializationModule_MVC")
        {

        }

        protected override void OnInit()
        {
            base.OnInit();
        }
    }
}
