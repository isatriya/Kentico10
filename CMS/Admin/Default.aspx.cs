using System;
using System.Web.UI;

using CMS.Base;
using CMS.Helpers;


public partial class Admin_Default : Page
{
    protected override void OnPreInit(EventArgs e)
    {
        string customAdminPath = AdministrationUrlHelper.GetCustomAdministrationPath();
        if (String.IsNullOrEmpty(customAdminPath) || customAdminPath.EqualsCSafe(AdministrationUrlHelper.DEFAULT_ADMINISTRATION_PATH, true))
        {
            URLHelper.Redirect("~/Admin/CMSAdministration.aspx" + RequestContext.URL.Query);
        }
        else
        {
            RequestHelper.Respond404();
        }
        base.OnPreInit(e);
    }
}