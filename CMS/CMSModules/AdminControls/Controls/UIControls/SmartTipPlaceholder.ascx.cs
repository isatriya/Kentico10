using System;

using CMS.UIControls;


public partial class CMSModules_AdminControls_Controls_UIControls_SmartTipPlaceholder : CMSAbstractUIWebpart
{
    #region "Properties"

    /// <summary>
    /// Gets or sets the text content of the displayed smart tip either as simple text or html.
    /// </summary>
    public string SmartTipText
    {
        get
        {
            return GetStringContextValue("SmartTipText", String.Empty);
        }
        set
        {
            SetValue("SmartTipText", value);
        }
    }

    #endregion


    #region "Methods"

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        smrtp.Visible = false;

        String smartTip = SmartTipText;

        if (smartTip != String.Empty)
        {
            smrtp.Content = smartTip;
            smrtp.Visible = true;
        }
    }

    #endregion
}
