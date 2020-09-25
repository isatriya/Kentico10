using System;

using CMS.Base.Web.UI;
using CMS.Base.Web.UI.Internal;
using CMS.Core;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.UIControls;
using CMS.WebAnalytics;
using CMS.WebAnalytics.Web.UI;


[EditedObject(CampaignInfo.OBJECT_TYPE, "campaignId")]
[UIElement("CMS.WebAnalytics", "Campaign.Reports")]
public partial class CMSModules_WebAnalytics_Pages_Tools_Campaign_Tab_Reports : CMSCampaignPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        var campaignId = QueryHelper.GetInteger("campaignId", 0);

        if (campaignId > 0)
        {
            var campaign = EditedObject as CampaignInfo;
            if ((campaign != null) && (campaign.CampaignSiteID == SiteContext.CurrentSiteID))
            {
                var moduleId = "CMS.WebAnalytics/CampaignReport/build";
                var angularLocalizationProvider = Service.Entry<IAngularLocalizationProvider>();
                var reportViewModelService = Service<ICampaignReportViewModelService>.Entry();

                ScriptHelper.RegisterAngularModule(moduleId, new {
                    Resources = angularLocalizationProvider.GetAngularLocalization(moduleId),
                    Report = reportViewModelService.GetViewModel(campaign)
                });
            }
            else
            {
                EditedObject = null;
            }
        }
        else
        {
            RedirectToInformation(GetString("campaign.nodata"));
        }
    }
}
