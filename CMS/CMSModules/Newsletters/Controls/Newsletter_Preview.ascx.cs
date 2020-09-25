using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Newsletters;
using CMS.SiteProvider;
using CMS.UIControls;

public partial class CMSModules_Newsletters_Controls_Newsletter_Preview : CMSUserControl
{
    #region "Constants"

    // Maximal number of subscribers for preview
    private const int MAX_PREVIEW_SUBSCRIBERS = 20;

    #endregion


    protected void Page_Load(object sender, EventArgs e)
    {
        var issue = (IssueInfo)UIContext.EditedObject;
        if ((issue == null) || (issue.IssueSiteID != SiteContext.CurrentSiteID))
        {
            return;
        }

        var newsletter = NewsletterInfoProvider.GetNewsletterInfo(issue.IssueNewsletterID);
        var issueHelper = new IssueHelper();

        var script = newsletter.NewsletterType == EmailCommunicationTypeEnum.Newsletter ? GetPreviewScriptForNewsletter(issue, newsletter, issueHelper) : GetPreviewScriptForCampaign(issue, newsletter, issueHelper);

        ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "PreviewData", ScriptHelper.GetScript(script));

        if (!RequestHelper.IsPostBack())
        {
            ScriptHelper.RegisterStartupScript(this, typeof(string), "LoadPreview" + ClientID, ScriptHelper.GetScript("pageLoad();"));
        }
    }


    private string GetPreviewScriptForCampaign(IssueInfo issue, NewsletterInfo newsletter, IssueHelper issueHelper)
    {
        string currentSiteName = SiteContext.CurrentSiteName;

        var output = new StringBuilder();
        var recipients = issue.GetRecipientsProvider()
                              .GetMarketableRecipients()
                              .TopN(MAX_PREVIEW_SUBSCRIBERS)
                              .ToList();

        if (!recipients.Any())
        {
            return InitializePreviewScriptForZeroSubscribers(issue, GetPreviewSubject(issue, newsletter, issueHelper, null, currentSiteName));
        }

        output.AppendLine(InitializePreviewScript(issue, recipients.Count));

        for (int index = 0; index < recipients.Count; index++)
        {
            var recipient = recipients[index];

            var dummySubscriber = new SubscriberInfo
            {
                SubscriberFirstName = recipient.ContactFirstName,
                SubscriberLastName = recipient.ContactLastName,
                SubscriberEmail = recipient.ContactEmail
            };

            output.AppendFormat("guid[{0}] = '{1}';", index, recipient.ContactGUID);
            output.AppendFormat("email[{0}] = '{1}';", index, recipient.ContactEmail);
            output.AppendFormat("subject[{0}] = {1};", index, ScriptHelper.GetString(HTMLHelper.HTMLEncode(GetPreviewSubject(issue, newsletter, issueHelper, dummySubscriber, currentSiteName))));
        }

        return output.ToString();
    }


    private string GetPreviewScriptForNewsletter(IssueInfo issue, NewsletterInfo newsletter, IssueHelper issueHelper)
    {
        string script;
        string siteName = SiteContext.CurrentSiteName;

        // Get specific number of subscribers subscribed to the newsletter
        DataSet subscribers = SubscriberInfoProvider
            .GetSubscribers()
            .TopN(MAX_PREVIEW_SUBSCRIBERS)
            .WhereIn("SubscriberID", new IDQuery(SubscriberNewsletterInfo.OBJECT_TYPE, "SubscriberID")
                .WhereEquals("NewsletterID", issue.IssueNewsletterID)
                .Where(w => w.WhereTrue("SubscriptionApproved")
                             .Or()
                             .WhereNull("SubscriptionApproved")));

        if (!DataHelper.DataSourceIsEmpty(subscribers))
        {
            // Limit max subscribers count to number of rows
            int maxCount = subscribers.Tables[0].Rows.Count;

            // Generate javascript based on subscribers
            script = InitializePreviewScript(issue, maxCount);

            for (int i = 0; i < maxCount; i++)
            {
                // Get subscriber
                SubscriberInfo subscriber = new SubscriberInfo(subscribers.Tables[0].Rows[i]);
                // Insert subscriber GUID
                script = string.Format("{0} guid[{1}] = '{2}'; \n ", script, i, subscriber.SubscriberGUID);

                // Get subscriber's member
                SortedDictionary<int, SubscriberInfo> subMembers = SubscriberInfoProvider.GetSubscribers(subscriber, 1);
                if ((subMembers != null) && (subMembers.Count > 0))
                {
                    foreach (KeyValuePair<int, SubscriberInfo> item in subMembers)
                    {
                        // Get 1st subscriber's member
                        SubscriberInfo sbMember = item.Value;
                        if (sbMember != null)
                        {
                            // Create information line
                            string infoLine = ScriptHelper.GetString(sbMember.SubscriberEmail, false);

                            // Add info about subscriber type
                            if (sbMember.SubscriberType.EqualsCSafe(PredefinedObjectType.CONTACTGROUP, true))
                            {
                                infoLine = string.Format("{0} ({1})", infoLine, GetString("objecttype.om_contactgroup").ToLowerCSafe());
                            }
                            else if (sbMember.SubscriberType.EqualsCSafe(PredefinedObjectType.CONTACT, true))
                            {
                                infoLine = string.Format("{0} ({1})", infoLine, GetString("objecttype.om_contact").ToLowerCSafe());
                            }

                            script = string.Format("{0}email[{1}] = '{2}'; \n ", script, i, HTMLHelper.HTMLEncode(infoLine));

                            // Create resolved subject
                            script = string.Format("{0}subject[{1}] = {2}; \n ", script, i, ScriptHelper.GetString(HTMLHelper.HTMLEncode(GetPreviewSubject(issue, newsletter, issueHelper, sbMember, siteName))));
                        }
                    }
                }
                else
                {
                    // Get generic name for external subscribers
                    var related = BaseAbstractInfoProvider.GetInfoById(subscriber.SubscriberType, subscriber.SubscriberRelatedID);
                    script = string.Format("{0}email[{1}] = '{2}'; \n ", script, i, HTMLHelper.HTMLEncode(TypeHelper.GetNiceObjectTypeName(subscriber.SubscriberType) + " \"" + HTMLHelper.HTMLEncode(related.Generalized.ObjectDisplayName) + "\""));

                    // Create resolved subject
                    return string.Format("{0}subject[{1}] = {2}; \n ", script, i, ScriptHelper.GetString(HTMLHelper.HTMLEncode(GetPreviewSubject(issue, newsletter, issueHelper, null, siteName))));
                }
            }
        }
        else
        {
            return InitializePreviewScriptForZeroSubscribers(issue, GetPreviewSubject(issue, newsletter, issueHelper, null, siteName));
        }
        return script;
    }


    private static string GetPreviewSubject(IssueInfo issue, NewsletterInfo newsletter, IssueHelper issueHelper, SubscriberInfo subscriber, string siteName)
    {
        // Resolve dynamic field macros ({%FirstName%}, {%LastName%}, {%Email%})
        return issueHelper.LoadDynamicFields(subscriber, newsletter, null, issue, true, siteName, null, null, null) ? 
            issueHelper.ResolveDynamicFieldMacros(issue.IssueSubject, newsletter, issue) : 
            null;
    }


    private static string InitializePreviewScriptForZeroSubscribers(IssueInfo issue, string subject)
    {
        return string.Format(
            @"var currentSubscriberIndex = 0;
var newsletterIssueId ={0};
var guid = new Array(1);
var email = new Array(1);
var subject = new Array(1);
subject[0] = '{1}';
var subscribers = new Array(guid, email);
guid[0] = 0;
email[0] = '(N/A)';", issue.IssueID, HTMLHelper.HTMLEncode(subject));
    }


    private static string InitializePreviewScript(IssueInfo issue, int maxCount)
    {
        return string.Format(
            @"var currentSubscriberIndex = 0;
var newsletterIssueId ={0};
var guid = new Array({1});
var email = new Array({1});
var subject = new Array({1});
var subscribers = new Array(guid, email);", issue.IssueID, maxCount);
    }


    protected void Page_PreRender(object sender, EventArgs e)
    {
        string elemString = "var lblEmail = '" + lblEmail.ClientID + "';\n" +
                            "var lnkPrev = '" + lnkPrevious.ClientID + "';\n" +
                            "var lnkNext = '" + lnkNext.ClientID + "';\n" +
                            "var lblSubj = '" + lblSubjectValue.ClientID + "';\n";

        // Register client IDs of the elements
        ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "PreviewElems", ScriptHelper.GetScript(elemString));
    }
}
