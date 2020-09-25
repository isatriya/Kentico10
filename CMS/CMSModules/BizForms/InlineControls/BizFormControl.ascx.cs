using System;

using CMS.Base.Web.UI;
using CMS.Helpers;


public partial class CMSModules_BizForms_InlineControls_BizFormControl : InlineUserControl
{
    /// <summary>
    /// Form name.
    /// </summary>
    public string FormName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("FormName"), null);
        }
        set
        {
            SetValue("FormName", value);
            Bizform1.FormName = value;
        }
    }


    /// <summary>
    /// Control parameter.
    /// </summary>
    public override string Parameter
    {
        get
        {
            return FormName;
        }
        set
        {
            FormName = value;
        }
    }


    protected void Page_Load(object sender, EventArgs e)
    {
        SetupControl();
    }


    /// <summary>
    /// Initializes the control properties.
    /// </summary>
    protected void SetupControl()
    {
        Bizform1.FormName = FormName;
        Bizform1.IsLiveSite = IsLiveSite;
    }
}