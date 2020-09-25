using System;
using System.Collections;
using System.Data;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Base.Web.UI.ActionsConfig;
using CMS.ContactManagement.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.UIControls;


public partial class CMSModules_ContactManagement_FormControls_ContactSelectorDialog : CMSModalPage
{
    #region "Variables"

    private Hashtable mParameters;

    #endregion


    #region "Properties"

    /// <summary>
    /// Stop processing flag.
    /// </summary>
    public bool StopProcessing
    {
        get
        {
            return gridElem.StopProcessing;
        }
        set
        {
            gridElem.StopProcessing = value;
        }
    }


    /// <summary>
    /// Hashtable containing dialog parameters.
    /// </summary>
    private Hashtable Parameters
    {
        get
        {
            if (mParameters == null)
            {
                string identifier = QueryHelper.GetString("params", null);
                mParameters = (Hashtable)WindowHelper.GetItem(identifier);
            }
            return mParameters;
        }
    }

    #endregion


    #region "Methods"

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!QueryHelper.ValidateHash("hash") || Parameters == null)
        {
            StopProcessing = true;
            return;
        }

        CurrentMaster.PanelContent.RemoveCssClass("dialog-content");

        var excludedContactIDsCondition = ValidationHelper.GetString(Parameters["where"], null);

        // Check read permission
        if (AuthorizationHelper.AuthorizedReadContact(false))
        {
            PageTitle.TitleText = GetString("om.contact.select");
            Page.Title = PageTitle.TitleText;

            // Load header actions
            InitHeaderActions();

            if (!String.IsNullOrEmpty(excludedContactIDsCondition))
            {
                var excludedContactsCondition = "ContactID NOT IN (SELECT ContactID FROM OM_Contact WHERE " + excludedContactIDsCondition + ")";
                gridElem.WhereCondition = SqlHelper.AddWhereCondition(gridElem.WhereCondition, excludedContactsCondition);
            }

            gridElem.OnExternalDataBound += gridElem_OnExternalDataBound;
            gridElem.ShowActionsMenu = false;
            if (!RequestHelper.IsPostBack())
            {
                gridElem.Pager.DefaultPageSize = 10;
            }
        }
    }


    /// <summary>
    /// Contact status selected event handler.
    /// </summary>
    protected void btn_Click(object sender, EventArgs e)
    {
        int contactID = ValidationHelper.GetInteger(((LinkButton)sender).CommandArgument, 0);
        string script = ScriptHelper.GetScript(@"
wopener.SelectValue_" + ValidationHelper.GetString(Parameters["clientid"], string.Empty) + @"(" + contactID + @");
CloseDialog();
");

        ScriptHelper.RegisterStartupScript(Page, typeof(string), "CloseWindow", script);
    }


    /// <summary>
    /// Unigrid external databound handler.
    /// </summary>
    protected object gridElem_OnExternalDataBound(object sender, string sourceName, object parameter)
    {
        switch (sourceName)
        {
            case "ContactFullNameJoined":
                LinkButton btn = new LinkButton();
                DataRowView drv = parameter as DataRowView;
                btn.Text = HTMLHelper.HTMLEncode(ValidationHelper.GetString(drv["ContactFullNameJoined"], null));
                btn.Click += btn_Click;
                btn.CommandArgument = ValidationHelper.GetString(drv["ContactID"], null);
                return btn;
        }
        return parameter;
    }


    /// <summary>
    /// Initialize header actions.
    /// </summary>
    private void InitHeaderActions()
    {
        AddHeaderAction(new HeaderAction
        {
            Text = GetString("om.contact.new"),
            OnClientClick = @"wopener.SelectValue_" + ValidationHelper.GetString(Parameters["clientid"], string.Empty) + @"(0); CloseDialog();"
        });
    }

    #endregion
}
