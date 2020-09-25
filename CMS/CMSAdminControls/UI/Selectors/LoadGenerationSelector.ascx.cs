using System;
using System.Web.UI.WebControls;

using CMS.Helpers;
using CMS.UIControls;


public partial class CMSAdminControls_UI_Selectors_LoadGenerationSelector : CMSUserControl
{
    private bool mNoChangeOption = false;


    /// <summary>
    /// If true, no change option (-1) is added to the list.
    /// </summary>
    public bool NoChangeOption
    {
        get
        {
            return mNoChangeOption;
        }
        set
        {
            mNoChangeOption = value;
        }
    }


    /// <summary>
    /// Selected value.
    /// </summary>
    public int Value
    {
        get
        {
            return ValidationHelper.GetInteger(drpGeneration.SelectedValue, 0);
        }
        set
        {
            try
            {
                drpGeneration.SelectedValue = value.ToString();
            }
            catch
            {
            }
        }
    }


    protected void Page_Init(object sender, EventArgs e)
    {
        if (NoChangeOption)
        {
            drpGeneration.Items.Add(new ListItem(GetString("LoadGeneration.NoChange"), "-1"));
        }
        drpGeneration.Items.Add(new ListItem(GetString("LoadGeneration.First"), "0"));
        drpGeneration.Items.Add(new ListItem(GetString("LoadGeneration.Second"), "1"));
        drpGeneration.Items.Add(new ListItem(GetString("LoadGeneration.Third"), "2"));
    }
}