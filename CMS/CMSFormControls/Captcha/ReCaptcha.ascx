<%@ Control Language="C#" AutoEventWireup="true"  Codebehind="ReCaptcha.ascx.cs" Inherits="CMSFormControls_Captcha_ReCaptcha" %>

<asp:Panel ID="pnlCaptchaWrap" runat="server">
    <div id="cbCaptcha" style="display: none;"></div>
    <cms:RecaptchaControl ID="captcha" runat="server" />
</asp:Panel>
