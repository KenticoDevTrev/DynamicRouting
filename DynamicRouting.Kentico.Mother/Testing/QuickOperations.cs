using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Membership;
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
        protected override void OnPreInit(EventArgs e)
        {
            CurrentUserInfo currentUser = MembershipContext.AuthenticatedUser;

            // Ensure access
            if (!currentUser.IsAuthorizedPerResource("DynamicRouting.Kentico", "Read"))
            {
                RedirectToAccessDenied("DynamicRouting.Kentico", "Read");
            }

            base.OnPreInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnRebuildSite_Click(object sender, EventArgs e)
        {
            DynamicRouteInternalHelper.RebuildRoutesBySite(DynamicRouteInternalHelper.SiteContextSafe().SiteName);
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

        protected void btnCleanWipe_Click(object sender, EventArgs e)
        {
            // clear out
            ConnectionHelper.ExecuteNonQuery("truncate table DynamicRouting_SlugGenerationQueue", null, QueryTypeEnum.SQLQuery, true);
            ConnectionHelper.ExecuteNonQuery("truncate table DynamicRouting_UrlSlug", null, QueryTypeEnum.SQLQuery, true);
            ConnectionHelper.ExecuteNonQuery("truncate table DynamicRouting_UrlSlugStagingTaskIgnore", null, QueryTypeEnum.SQLQuery, true);
            foreach(SiteInfo Site in SiteInfoProvider.GetSites()) { 
                DynamicRouteInternalHelper.RebuildRoutesBySite(Site.SiteName);
            }
        }
    }
}