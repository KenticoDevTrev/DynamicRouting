using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.UIControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicRouting.Kentico
{
    public class VersionHistorySlugGenerationQueueUnigridExtension : ControlExtender<UniGrid>
    {
        public override void OnInit()
        {
            Control.OnAction += Control_OnAction;
            Control.OnExternalDataBound += Control_OnExternalDataBound;
        }

        private object Control_OnExternalDataBound(object sender, string sourceName, object parameter)
        {
            switch(sourceName.ToLower())
            {
                case "haserrors":
                    return string.IsNullOrWhiteSpace(ValidationHelper.GetString(parameter, "")) ? "No" : "Yes";
                default:
                    return parameter;
            }
        }

        private void Control_OnAction(string actionName, object actionArgument)
        {
            switch (actionName.ToLower())
            {
                case "run":
                    int VersionHistorySlugQueueID = ValidationHelper.GetInteger(actionArgument, 0);
                    if(VersionHistorySlugQueueID > 0)
                    {
                        DynamicRouteInternalHelper.RunVersionHistoryGenerationQueueItem(VersionHistorySlugQueueID);
                    }
                    break;
            }
        }
    }
}
