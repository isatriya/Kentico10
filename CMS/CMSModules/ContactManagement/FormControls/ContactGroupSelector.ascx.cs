using System;

using CMS.Base;
using CMS.ContactManagement.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.UIControls;


public partial class CMSModules_ContactManagement_FormControls_ContactGroupSelector : FormEngineUserControl
{
    #region "Public properties"
    
    /// <summary>
    /// Returns Uniselector.
    /// </summary>
    public UniSelector UniSelector
    {
        get
        {
            EnsureChildControls();
            return uniSelector;
        }
    }


    /// <summary>
    /// Gets or sets field value.
    /// </summary>
    public override object Value
    {
        get
        {
            EnsureChildControls();
            return uniSelector.Value;
        }
        set
        {
            EnsureChildControls();
            uniSelector.Value = ValidationHelper.GetString(value, "");
        }
    }


    /// <summary>
    /// SQL WHERE condition of uniselector.
    /// </summary>
    public string WhereCondition
    {
        get;
        set;
    }

    #endregion


    #region "Methods"

    protected void Page_Load(object sender, EventArgs e)
    {
        if (StopProcessing)
        {
            uniSelector.StopProcessing = true;
        }
        else
        {
            ReloadData();
        }
    }


    /// <summary>
    /// Reloads the data in the selector.
    /// </summary>
    public void ReloadData()
    {
        if (!AuthorizationHelper.AuthorizedReadContact(false))
        {
            uniSelector.WhereCondition = "(1=0)";
        }

        // Initialize selector
        uniSelector.IsLiveSite = false;
        uniSelector.Reload(true);
    }


    /// <summary>
    /// Overrides base GetValue method and enables to return UniSelector control with 'uniselector' property name.
    /// </summary>
    /// <param name="propertyName">Property name</param>
    public override object GetValue(string propertyName)
    {
        if (propertyName.EqualsCSafe("uniselector", true))
        {
            // Return uniselector control
            return UniSelector;
        }

        // Return other values
        return base.GetValue(propertyName);
    }

    #endregion
}