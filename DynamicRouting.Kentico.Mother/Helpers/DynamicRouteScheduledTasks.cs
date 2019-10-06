using CMS.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicRouting.Kentico
{
    public class DynamicRouteScheduledTasks : ITask
    {
        public string Execute(TaskInfo task)
        {
            string Result = "";
            switch (task.TaskName.ToLower())
            {
                case "checkurlslugqueue":
                    DynamicRouteHelper.CheckUrlSlugGenerationQueue();
                    break;
            }
            return Result;
        }
    }
}
