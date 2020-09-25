using System;
using System.Linq;

using CMS.Activities;
using CMS.Base.Web.UI.ActionsConfig;
using CMS.ContactManagement;
using CMS.ContactManagement.Web.UI;
using CMS.Core;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.UIControls;

// Edited object
[EditedObject(ContactInfo.OBJECT_TYPE, "contactId")]
[UIElement(ModuleName.ONLINEMARKETING, "ContactActivities")]
[Security(Resource = ModuleName.ACTIVITIES, Permission = "ReadActivities")]
public partial class CMSModules_ContactManagement_Pages_Tools_Contact_Tab_Activities : CMSContactManagementPage
{
    /// <summary>
    /// PreInit event handler
    /// </summary>
    protected override void OnPreInit(EventArgs e)
    {
        RequiresDialog = false;
        base.OnPreInit(e);
    }


    protected void Page_Load(object sender, EventArgs e)
    {
        if (EditedObject != null)
        {
            ContactInfo ci = (ContactInfo)EditedObject;

            ucDisabledModule.TestSettingKeys = "CMSEnableOnlineMarketing;CMSCMActivitiesEnabled";
            ucDisabledModule.ParentPanel = pnlDis;

            pnlDis.Visible = !ActivitySettingsHelper.ActivitiesEnabledAndModuleLoaded(SiteContext.CurrentSiteID);

            listElem.ShowSiteNameColumn = true;
            listElem.SiteID = UniSelector.US_ALL_RECORDS;
            listElem.ContactID = ci.ContactID;
            listElem.OrderBy = "ActivityCreated DESC";

            // Init header action for new custom activities only if contact is not global, a custom activity type exists and user is authorized to manage activities
            if (ActivitySettingsHelper.ActivitiesEnabledAndModuleLoaded(SiteContext.CurrentSiteName) && MembershipContext.AuthenticatedUser.IsAuthorizedPerResource(ModuleName.ACTIVITIES, "ManageActivities"))
            {
                // Disable manual creation of activity if no custom activity type is available
                var activityType = ActivityTypeInfoProvider.GetActivityTypes()
                                                   .WhereEquals("ActivityTypeIsCustom", 1)
                                                   .WhereEquals("ActivityTypeEnabled", 1)
                                                   .WhereEquals("ActivityTypeManualCreationAllowed", 1)
                                                   .TopN(1)
                                                   .Column("ActivityTypeID")
                                                   .FirstOrDefault();

                if (activityType != null)
                {
                    // Prepare target URL
                    string url = ResolveUrl(string.Format("~/CMSModules/Activities/Pages/Tools/Activities/Activity/New.aspx?contactId={0}", ci.ContactID));

                    // Init header action
                    HeaderAction action = new HeaderAction()
                    {
                        Text = GetString("om.activity.newcustom"),
                        RedirectUrl = url
                    };
                    CurrentMaster.HeaderActions.ActionsList.Add(action);
                }
            }

            if (!RequestHelper.IsPostBack() && QueryHelper.GetBoolean("saved", false))
            {
                // Display 'Save' message after new custom activity was created
                ShowChangesSaved();
            }
        }
    }
}