using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Base.Web.UI.ActionsConfig;
using CMS.Core;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.UIControls;


public partial class CMSModules_SystemDevelopment_DevMenu : CMSUserControl, ICallbackEventHandler
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (SystemContext.DevelopmentMode)
        {
            // Restart application
            menu.AddAction(new HeaderAction
            {
                Text = GetString("administration-system.btnrestart"),
                ButtonStyle = ButtonStyle.Default,
                Tooltip = GetString("administration-system.btnrestart"),
                OnClientClick = "function RestartPerformed() {return alert('" + GetString("administration-system.restartsuccess") + "');} if (confirm('" + GetString("system.restartconfirmation") + "')) {" + Page.ClientScript.GetCallbackEventReference(this, "'restart'", "RestartPerformed", String.Empty, true) + "}"
            });

            // Clear cache
            menu.AddAction(new HeaderAction
            {
                Text = GetString("administration-system.btnclearcache"),
                ButtonStyle = ButtonStyle.Default,
                Tooltip = GetString("administration-system.btnclearcache"),
                OnClientClick = "function ClearCachePerformed() {return alert('" + GetString("administration-system.clearcachesuccess") + "');} if (confirm('" + GetString("system.clearcacheconfirmation") + "')) {" + Page.ClientScript.GetCallbackEventReference(this, "'clearcache'", "ClearCachePerformed", String.Empty, true) + "}"
            });

            // Submit defect
            menu.AddAction(new HeaderAction
            {
                Text = "Submit defect",
                ButtonStyle = ButtonStyle.Default,
                Tooltip = "Submit defect",
                RedirectUrl = "https://kentico.atlassian.net/secure/CreateIssue!default.jspa",
                Target = "_blank"
            });

            // Virtual sites
            HeaderAction sites = new HeaderAction
            {
                Text = GetString("devmenu.sites"),
                ButtonStyle = ButtonStyle.Default,
                Tooltip = GetString("devmenu.sites"),
                Inactive = true
            };
           
            sites.AlternativeActions.AddRange(GetSitesAvailableForVirtualAccess());

            menu.AddAction(sites);
        }
        else
        {
            Visible = false;
        }
    }


    private List<HeaderAction> GetSitesAvailableForVirtualAccess()
    {
        List<HeaderAction> actions = new List<HeaderAction>();

        CacheHelper.Cache(() =>
        {
            var sitesData = SiteInfoProvider.GetSites()
                                .Columns("SiteName", "SiteDisplayName")
                                .OrderBy("SiteDisplayName")
                                .TypedResult;

            foreach (SiteInfo s in sitesData)
            {
                // Prepare the parameters
                NameValueCollection values = new NameValueCollection();
                values.Add(VirtualContext.PARAM_SITENAME, s.SiteName);

                HeaderAction site = new HeaderAction
                {
                    Text = HTMLHelper.HTMLEncode(ResHelper.LocalizeString(s.DisplayName)),
                    ButtonStyle = ButtonStyle.Default,
                    RedirectUrl = VirtualContext.GetVirtualContextPath(Request.Path, values),
                    Target = "_blank"
                };

                actions.Add(site);
            }

            return actions;
        },
        new CacheSettings(60, "DevMenu", "Sites", "VirtualContext")
        {
            GetCacheDependency = () => CacheHelper.GetCacheDependency(new[]
            {
                    SiteInfo.OBJECT_TYPE + "|all",
            })
        });

        return actions;
    }


    /// <summary>
    /// Returns callback result.
    /// </summary>
    public string GetCallbackResult()
    {
        return String.Empty;
    }


    /// <summary>
    /// Raises callback event.
    /// </summary>
    /// <param name="eventArgument">Event argument</param>
    public void RaiseCallbackEvent(string eventArgument)
    {
        switch (eventArgument)
        {
            case "restart":
                SystemHelper.RestartApplication(Request.PhysicalApplicationPath);
                break;

            case "clearcache":
                CacheHelper.ClearCache();
                break;
        }
    }
}