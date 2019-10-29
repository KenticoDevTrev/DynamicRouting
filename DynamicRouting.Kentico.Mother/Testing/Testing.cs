using CMS.Base;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.SiteProvider;
using DynamicRouting;
using DynamicRouting.Kentico;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMSApp.CMSModules.DynamicRouting
{
    public partial class Testing : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnRebuildSite_Click(object sender, EventArgs e)
        {
            //EventLogProvider.LogInformation("DynamicRouteTesting", "TestStart", eventDescription: DateTime.Now.ToString() + " " + DateTime.Now.Millisecond.ToString());
            DynamicRouteInternalHelper.RebuildRoutesBySite(SiteContext.CurrentSiteName);
            //EventLogProvider.LogInformation("DynamicRouteTesting", "TestEnd", eventDescription: DateTime.Now.ToString() + " " + DateTime.Now.Millisecond.ToString());
        }

        protected void btnRebuildSubTree_Click(object sender, EventArgs e)
        {
            TreeNode Page = DocumentHelper.GetDocuments().Path(tbxPath.Text).FirstOrDefault();

            if (Page != null)
            {
                DynamicRouteInternalHelper.RebuildSubtreeRoutesByNode(Page.NodeID);
            }
        }

        protected void btnRunQueue_Click(object sender, EventArgs e)
        {
            DynamicRouteInternalHelper.CheckUrlSlugGenerationQueue();
        }

        protected void btnCheckUrl_Click(object sender, EventArgs e)
        {
            ITreeNode Node = DynamicRouteHelper.GetPage(tbxRouteToTest.Text);
            if (Node != null)
            {
                ltrPageFound.Text = $"FOUND! {Node.NodeAliasPath} {Node.ClassName} {Node.DocumentCulture}";
            }
            else
            {
                ltrPageFound.Text = "No Node Found";
            }
        }
    }
}