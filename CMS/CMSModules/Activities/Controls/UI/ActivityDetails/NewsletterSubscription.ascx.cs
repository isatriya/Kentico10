using System;

using CMS.Activities;
using CMS.Activities.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Newsletters;


public partial class CMSModules_Activities_Controls_UI_ActivityDetails_NewsletterSubscription : ActivityDetail
{
    #region "Methods"

    public override bool LoadData(ActivityInfo ai)
    {
        if (ai == null)
        {
            return false;
        }

        switch (ai.ActivityType)
        {
            case PredefinedActivityType.NEWSLETTER_SUBSCRIBING:
            case PredefinedActivityType.NEWSLETTER_UNSUBSCRIBING:
                break;
            default:
                return false;
        }

        // Get newsletter name
        NewsletterInfo newsletterInfo = NewsletterInfoProvider.GetNewsletterInfo(ai.ActivityItemID);
        if (newsletterInfo != null)
        {
            string subject = ValidationHelper.GetString(newsletterInfo.NewsletterDisplayName, null);
            ucDetails.AddRow("om.activitydetails.newsletter", subject);
        }

        // Get issue subject only for unsubscribing activity. Subscribing activity has reference to the subscriber in ItemDetailID.
        if (ai.ActivityType == PredefinedActivityType.NEWSLETTER_UNSUBSCRIBING)
        {
            IssueInfo issueInfo = IssueInfoProvider.GetIssueInfo(ai.ActivityItemDetailID);
            if (issueInfo != null)
            {
                string subject = ValidationHelper.GetString(issueInfo.IssueSubject, null);
                ucDetails.AddRow("om.activitydetails.newsletterissue", MacroSecurityProcessor.RemoveSecurityParameters(subject, true, null));
            }
        }

        return ucDetails.IsDataLoaded;
    }

    #endregion
}