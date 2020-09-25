using System;

using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Newsletters;
using CMS.Newsletters.Web.UI;
using CMS.SiteProvider;
using CMS.UIControls;


[EditedObject(IssueInfo.OBJECT_TYPE, "objectid")]
[UIElement(ModuleName.NEWSLETTER, "Newsletter.Issue.Content")]
public partial class CMSModules_Newsletters_Tools_Newsletters_Newsletter_Issue_ShowPreview : CMSNewsletterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        IssueInfo issue = EditedObject as IssueInfo;
        if (issue == null)
        {
            RedirectToAccessDenied(GetString("general.invalidparameters"));
        }

        if(!issue.CheckPermissions(PermissionsEnum.Read, CurrentSiteName, CurrentUser))
        {
            RedirectToAccessDenied(issue.TypeInfo.ModuleName, "AuthorIssues");   
        }

        var subscriber = GetSubscriber(NewsletterInfoProvider.GetNewsletterInfo(issue.IssueNewsletterID));

        // Switch culture to the site culture, so the e-mail isn't rendered in the editor's culture
        string culture = CultureHelper.GetDefaultCultureCode(SiteContext.CurrentSiteName);
        using (new CMSActionContext { ThreadCulture = CultureHelper.GetCultureInfo(culture) })
        {
            string htmlPage = NewsletterHelper.GetPreviewHTML(issue, subscriber);
            Response.Clear();
            Response.Write(htmlPage);
        }

        RequestHelper.EndResponse();
    }


    /// <summary>
    /// Gets instance of <see cref="SubscriberInfo"/> according to the <see cref="NewsletterInfo.NewsletterType"/> of given <paramref name="newsletter"/>.
    /// If <paramref name="newsletter"/> has type <see cref="EmailCommunicationTypeEnum.Newsletter"/>, only returns existing <see cref="SubscriberInfo"/> identified by Guid passed as query parameter.
    /// Otherwise, returns new dummy <see cref="SubscriberInfo"/> filled with the values obtained from the <see cref="ContactInfo"/> identified by Guid passed as query paremeter.
    /// </summary>
    private static SubscriberInfo GetSubscriber(NewsletterInfo newsletter)
    {
        Guid recipientGuid = QueryHelper.GetGuid("recipientguid", Guid.Empty);

        if (newsletter.NewsletterType == EmailCommunicationTypeEnum.Newsletter)
        {
            return SubscriberInfoProvider.GetSubscriberInfo(recipientGuid, SiteContext.CurrentSiteID);
        }

        var recipient = ContactInfoProvider.GetContactInfo(recipientGuid);
        return recipient == null ? null : new SubscriberInfo
        {
            SubscriberFirstName = recipient.ContactFirstName,
            SubscriberLastName = recipient.ContactLastName,
            SubscriberEmail = recipient.ContactEmail
        };
    }
}