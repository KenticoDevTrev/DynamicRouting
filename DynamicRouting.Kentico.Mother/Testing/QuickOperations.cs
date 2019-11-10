using CMS.Base;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.SiteProvider;
using CMS.UIControls;
using DynamicRouting;
using DynamicRouting.Kentico;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMSApp.CMSModules.DynamicRouting
{
    public partial class QuickOperations : CMSPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnRebuildSite_Click(object sender, EventArgs e)
        {
            DynamicRouteInternalHelper.RebuildRoutesBySite(SiteContext.CurrentSiteName);
        }

        protected void btnRebuildSubTree_Click(object sender, EventArgs e)
        {
            TreeNode Page = DocumentHelper.GetDocuments().Path("/"+tbxPath.Value.ToString().Trim('%').Trim('/')).FirstOrDefault();

            if (Page != null)
            {
                DynamicRouteInternalHelper.RebuildSubtreeRoutesByNode(Page.NodeID);
            }
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