using System;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Base.Web.UI.ActionsConfig;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Newsletters;
using CMS.Newsletters.Web.UI;
using CMS.UIControls;


[Security(Resource = ModuleName.NEWSLETTER, Permission = "authorissues")]
[UIElement(ModuleName.NEWSLETTER, "Newsletter.Issue.Send")]
[EditedObject(IssueInfo.OBJECT_TYPE, "objectid")]
public partial class CMSModules_Newsletters_Tools_Newsletters_Newsletter_Issue_Send : CMSNewsletterPage
{
    public const string SCHEDULE_ACTION_IDENTIFIER = "schedule";

    private NewsletterInfo mNewsletter;


    /// <summary>
    /// Indicates if newsletter is template-based.
    /// </summary>
    public bool IsNewsletterTemplateBased
    {
        get
        {
            return mNewsletter.NewsletterTemplateID > 0;
        }
    }


    protected void Page_Load(object sender, EventArgs e)
    {
        // Normal messages must be bellow the information label
        MessagesPlaceHolder = plcMess;

        // Get newsletter issue and check its existence
        IssueInfo issue = EditedObject as IssueInfo;

        if (issue == null)
        {
            RedirectToAccessDenied(GetString("general.invalidparameters"));
        }

        if (!issue.CheckPermissions(PermissionsEnum.Read, CurrentSiteName, CurrentUser))
        {
            RedirectToAccessDenied(issue.TypeInfo.ModuleName, "AuthorIssues");
        }

        mNewsletter = NewsletterInfoProvider.GetNewsletterInfo(issue.IssueNewsletterID);

        string infoMessage;
        bool isABTest = issue.IssueIsABTest;
        bool sendingIssueAllowed = false;
        bool isSent = (issue.IssueMailoutTime != DateTimeHelper.ZERO_TIME) && (issue.IssueMailoutTime < DateTime.Now);

        sendElem.Visible = !isABTest;
        sendVariant.Visible = isABTest;

        if (isABTest)
        {
            sendVariant.StopProcessing = (issue.IssueID <= 0);
            sendVariant.IssueID = issue.IssueID;
            sendVariant.OnChanged -= sendVariant_OnChanged;
            sendVariant.OnChanged += sendVariant_OnChanged;
            sendVariant.ReloadData(!RequestHelper.IsPostBack());
            infoMessage = sendVariant.InfoMessage;
            sendingIssueAllowed = sendVariant.SendingAllowed;
        }
        else
        {
            sendElem.IssueID = issue.IssueID;
            sendElem.NewsletterID = issue.IssueNewsletterID;
            sendElem_TemplateBased.IssueID = issue.IssueID;
            infoMessage = GetInfoMessage(issue.IssueStatus);

            // If issue has 'Idle' or 'Ready for sending' status, it's allowed to send it
            if (mNewsletter != null)
            {
                sendingIssueAllowed = (issue.IssueStatus == IssueStatusEnum.Idle) || (issue.IssueStatus == IssueStatusEnum.ReadyForSending);
            }

            if (IsNewsletterTemplateBased)
            {
                sendElem.Visible = false;
                sendElem_TemplateBased.Visible = true;
            }
        }

        // Display additional information
        if (!String.IsNullOrEmpty(infoMessage))
        {
            ShowInformationInternal(infoMessage);
        }

        InitHeaderActions(isABTest && (issue.IssueStatus != IssueStatusEnum.Finished), sendingIssueAllowed, isSent);
        
        string scriptBlock = @"function RefreshPage() {{ document.location.replace(document.location); }}";
        ScriptHelper.RegisterClientScriptBlock(this, GetType(), "RefreshActions", scriptBlock, true);

        if (!RequestHelper.IsPostBack() && (QueryHelper.GetInteger("sent", 0) == 1))
        {
            ShowConfirmation(GetString("Newsletter_Send.SuccessfullySent"));
        }

        AddBrokenEmailUrlNotifier(mNewsletter, lblUrlWarning);
    }


    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        HandleBreadcrumbsScripts();
    }


    protected void sendVariant_OnChanged(object sender, EventArgs e)
    {
        ShowInformationInternal(sendVariant.InfoMessage);
    }


    /// <summary>
    /// Sends an issue.
    /// </summary>
    protected void Send()
    {
        CheckPermissions();

        string errMessage = null;

        if (sendElem.Visible)
        {
            errMessage = SendDynamicIssue();
        }
        else if (sendVariant.Visible)
        {
            errMessage = SendVariants();
        }
        else if (sendElem_TemplateBased.Visible)
        {
            errMessage = SendTemplateBasedIssue();
        }

        HandleActionResult(errMessage);
    }


    /// <summary>
    /// Saves current issue variant setting.
    /// </summary>
    protected void Save()
    {
        if (sendVariant.SaveIssue())
        {
            if (!String.IsNullOrEmpty(sendVariant.InfoMessage))
            {
                ShowInformationInternal(sendVariant.InfoMessage);
            }

            ShowChangesSaved();
        }
        else
        {
            ShowError(sendVariant.ErrorMessage);
        }
    }


    protected void hdrActions_ActionPerformed(object sender, CommandEventArgs e)
    {
        switch (e.CommandName)
        {
            case ComponentEvents.SAVE:
                Save();
                break;
            case ComponentEvents.SUBMIT:
                Send();
                break;
            case SCHEDULE_ACTION_IDENTIFIER:
                ScheduleTemplateBasedIssue();
                break;
        }
    }


    /// <summary>
    /// Schedules a template-based issue.
    /// </summary>
    /// <remarks>It also checks permissions and handles possible error messages.</remarks>
    /// <returns>Error message on failure</returns>
    private void ScheduleTemplateBasedIssue()
    {
        CheckPermissions();

        string errorMessage = String.Empty;

        if (!sendElem_TemplateBased.SendScheduled())
        {
            errorMessage = sendElem_TemplateBased.ErrorMessage;
        }

        HandleActionResult(errorMessage);
    }


    /// <summary>
    /// Sends a template-based issue.
    /// </summary>
    /// <returns>Error message on failure</returns>
    private string SendTemplateBasedIssue()
    {
        if (!sendElem_TemplateBased.SendNow())
        {
            return sendElem_TemplateBased.ErrorMessage;
        }

        return String.Empty;
    }


    /// <summary>
    /// Sends a dynamic issue.
    /// </summary>
    /// <returns>Error message on failure</returns>
    private string SendDynamicIssue()
    {
        if (!sendElem.SendIssue())
        {
            return sendElem.ErrorMessage;
        }
        else
        {
            if (mNewsletter != null)
            {
                // Redirect to the issue list page
                ScriptHelper.RegisterStartupScript(this, typeof(string), "Newsletter_Issue_Send", "parent.location='" + ResolveUrl("~/CMSModules/Newsletters/Tools/Newsletters/Newsletter_Issue_List.aspx?newsletterid=" + mNewsletter.NewsletterID) + "';", true);
            }
        }

        return String.Empty;
    }


    /// <summary>
    /// Sends issue with variants.
    /// </summary>
    /// <returns>Error message on failure</returns>
    private string SendVariants()
    {
        return !sendVariant.SendIssue() ? sendVariant.ErrorMessage : String.Empty;
    }


    /// <summary>
    /// Checks if user has permissions to access newsletters.
    /// </summary>
    private void CheckPermissions()
    {
        if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("cms.newsletter", "authorissues"))
        {
            RedirectToAccessDenied("cms.newsletter", "authorissues");
        }
    }


    /// <summary>
    /// Depending on <paramref name="errorMessage"/> it either shows error or if there is no error it redirects to confirmation url.
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    private void HandleActionResult(string errorMessage)
    {
        if (String.IsNullOrEmpty(errorMessage))
        {
            string url = URLHelper.AddParameterToUrl(RequestContext.CurrentURL, "sent", "1");
            URLHelper.Redirect(url);
        }
        else
        {
            ShowError(errorMessage);
        }
    }


    /// <summary>
    /// Gets info message depending on issue status.
    /// </summary>
    /// <param name="issueStatus">Issue status</param>
    /// <returns>Info message</returns>
    private string GetInfoMessage(IssueStatusEnum issueStatus)
    {
        switch (issueStatus)
        {
            case IssueStatusEnum.Finished:
                return GetString("Newsletter_Issue_Header.AlreadySent");

            case IssueStatusEnum.ReadyForSending:
                return GetString("Newsletter_Issue_Header.AlreadyScheduled");

            default:
                return GetString("Newsletter_Issue_Header.NotSentYet");
        }
    }


    /// <summary>
    /// Initializes header action control.
    /// </summary>
    /// <param name="isActiveABTest">Indicates if the issue is A/B test and its status is not 'Finished'</param>
    /// <param name="allowSendingIssue">Indicates if sending is allowed</param>
    /// <param name="isNewsletterSent">Indicates if newsletter was already sent</param>
    private void InitHeaderActions(bool isActiveABTest, bool allowSendingIssue, bool isNewsletterSent)
    {
        HeaderActions hdrActions = CurrentMaster.HeaderActions;
        hdrActions.ActionsList.Clear();

        // Init save button
        if (isActiveABTest)
        {
            hdrActions.ActionsList.Add(new SaveAction());
        }

        if (allowSendingIssue)
        {
            InitSendAndScheduleHeaderActions(hdrActions, isActiveABTest, isNewsletterSent);
        }

        hdrActions.ActionPerformed += hdrActions_ActionPerformed;
        hdrActions.ReloadData();

        CurrentMaster.DisplayActionsPanel = true;
    }


    /// <summary>
    /// Initializes send and schedule based header actions.
    /// </summary>
    /// <param name="hdrActions">Header actions</param>
    /// <param name="isActiveABTest">Indicates if the issue is A/B test and its status is not 'Finished'</param>
    /// <param name="isNewsletterSent">If true, newsletter has already been sent and if it's template-based newsletter, no buttons are shown.</param>
    private void InitSendAndScheduleHeaderActions(HeaderActions hdrActions, bool isActiveABTest, bool isNewsletterSent)
    {
        if (IsNewsletterTemplateBased && !isActiveABTest)
        {
            if (isNewsletterSent)
            {
                return;
            }

            AddTemplateBasedHeaderActions(hdrActions);
        }
        else
        {
            AddSendHeaderAction(hdrActions);
        }
    }


    /// <summary>
    /// Adds template-based header actions to <paramref name="hdrActions"/>.
    /// </summary>
    /// <param name="hdrActions">Header actions</param>
    private void AddTemplateBasedHeaderActions(HeaderActions hdrActions)
    {
        hdrActions.ActionsList.Add(new HeaderAction
        {
            CommandName = SCHEDULE_ACTION_IDENTIFIER,
            Text = GetString("newsletterissue_send.saveschedule"),
            Tooltip = GetString("newsletterissue_send.saveschedule"),
            Enabled = true
        });

        hdrActions.ActionsList.Add(new HeaderAction
        {
            CommandName = ComponentEvents.SUBMIT,
            Text = GetString("newsletterissue_send.sendnowbutton"),
            Tooltip = GetString("newsletterissue_send.sendnowbutton"),
            Enabled = true,
            OnClientClick = "return confirm('" + GetString("newsletterissue_send.confirmationdialog") + "');",
            ButtonStyle = ButtonStyle.Default
        });
    }


    /// <summary>
    /// Adds send header action to <paramref name="hdrActions"/>.
    /// </summary>
    /// <param name="hdrActions">Header actions</param>
    private void AddSendHeaderAction(HeaderActions hdrActions)
    {
        hdrActions.ActionsList.Add(new HeaderAction
        {
            CommandName = ComponentEvents.SUBMIT,
            Text = GetString("newsletterissue_send.send"),
            Tooltip = GetString("newsletterissue_send.send"),
            Enabled = true
        });
    }


    /// <summary>
    /// Shows user friendly message in ordinary label.
    /// </summary>
    /// <param name="message">Message to be shown</param>
    private void ShowInformationInternal(string message)
    {
        lblInfo.Text = message;
    }


    /// <summary>
    /// Handles manual rendering of breadcrumbs.
    /// On this page the breadcrumbs needs to be hard-coded in order to be able to access single email via link and ensure consistency of breadcrumbs.
    /// </summary>
    private void HandleBreadcrumbsScripts()
    {
        ScriptHelper.RegisterRequireJs(Page);

        ControlsHelper.RegisterClientScriptBlock(this, Page, typeof(string), "BreadcrumbsOverwriting", ScriptHelper.GetScript(@"
        cmsrequire(['CMS/EventHub'], function(hub) {
              hub.publish('OverwriteBreadcrumbs', " + IssueHelper.GetBreadcrumbsData((IssueInfo)EditedObject, (NewsletterInfo)EditedObjectParent) + @");
        });"));
    }
}
