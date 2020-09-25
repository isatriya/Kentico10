using System;

using CMS.Base.Web.UI;
using CMS.Core;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.UIControls;


[UIElement(ModuleName.CMS, "Settings")]
public partial class CMSModules_Settings_Pages_Default : GlobalAdminPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (CultureHelper.IsUICultureRTL())
        {
            ControlsHelper.ReverseFrames(colsFrameset);
        }

        keysFrame.Attributes.Add("src", UIContextHelper.GetElementUrl(ModuleName.CMS, "Settings.Keys", false));
    }
}