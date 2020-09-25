using System;
using System.Collections.Generic;

using CMS.Base.Web.UI;
using CMS.Membership;
using CMS.UIControls;


public partial class CMSAdminControls_UI_SmartTip : CMSUserControl
{

    private readonly UserSmartTipDismissalManager mUserSmartTipManager = new UserSmartTipDismissalManager(MembershipContext.AuthenticatedUser);



    /// <summary>
    /// Gets or sets the identifier of the smart tip used for storing the collapsed state. If multiple smart tips with the same
    /// identifier are created, closing one will result in closing all of them.
    /// </summary>
    public string CollapsedStateIdentifier
    {
        get;
        set;
    }
    

    /// <summary>
    /// Sets the expanded header of the smart tip.
    /// Use plain text.
    /// </summary>
    public string ExpandedHeader
    {
        get;
        set;
    }


    /// <summary>
    /// Sets the collapsed header of the smart tip.
    /// Use plain text.
    /// </summary>
    public string CollapsedHeader
    {
        get;
        set;
    }


    /// <summary>
    /// Sets the content of the smart tip.
    /// Use HTML.
    /// </summary>
    public string Content
    {
        get;
        set;
    }

    protected void Page_PreRender(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(CollapsedStateIdentifier))
        {
            CollapsedStateIdentifier = ClientID;
        }

        var resources = new Dictionary<string, string>
        {
            {"smarttip.smarttip", GetString("smarttip.smarttip")},
            {"smarttip.expand", GetString("smarttip.expand")},
            {"general.collapse", GetString("general.collapse")},
        };

        if (string.IsNullOrEmpty(CollapsedHeader))
        {
            CollapsedHeader = ExpandedHeader;
        }

        if (string.IsNullOrEmpty(ExpandedHeader))
        {
            ExpandedHeader = CollapsedHeader;
        }

        ScriptHelper.RegisterModule(this, "CMS/SmartTips/SmartTip", new
        {
            selector = "#" + pnlTooltip.ClientID,
            expandedHeader = ExpandedHeader,
            collapsedHeader = CollapsedHeader,
            content = Content,
            isCollapsed = mUserSmartTipManager.IsSmartTipDismissed(CollapsedStateIdentifier),
            identifier = CollapsedStateIdentifier,
            resources = resources
        });
    }
}