<%@ Page Language="C#" AutoEventWireup="true"  Codebehind="Tab_Activities.aspx.cs" MasterPageFile="~/CMSMasterPages/UI/SimplePage.master"
    Title="Contact properties - Activities" Inherits="CMSModules_ContactManagement_Pages_Tools_Contact_Tab_Activities"
    Theme="Default" %>

<%@ Register Src="~/CMSModules/Activities/Controls/UI/Activity/List.ascx"
    TagName="ActivityList" TagPrefix="cms" %>
<%@ Register Src="~/CMSAdminControls/Basic/DisabledModuleInfo.ascx" TagPrefix="cms"
    TagName="DisabledModule" %>
<asp:Content ID="cntBody" runat="server" ContentPlaceHolderID="plcContent">
    <asp:Panel runat="server" ID="pnlDis" Visible="false">
        <cms:DisabledModule runat="server" ID="ucDisabledModule" />
    </asp:Panel>
    <cms:ActivityList runat="server" ID="listElem" ZeroRowsText="om.contact.noactivities" FilteredZeroRowsText="om.contact.noactivities.filtered" ShowRemoveButton="true" ShowContactNameColumn="false" />
</asp:Content>
