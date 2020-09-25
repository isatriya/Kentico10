using System;

using CMS.Base.Web.UI;
using CMS.Base.Web.UI.Internal;
using CMS.Core;
using CMS.UIControls;
using CMS.ContactManagement;


[EditedObject(ContactInfo.OBJECT_TYPE, "objectid")]
[UIElement(ModuleName.CONTACTMANAGEMENT, "ContactProfile", false, true)]
public partial class CMSModules_ContactManagement_Pages_Contact_Details : CMSPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        UpdateBreadcrumbs();

        string moduleId = "CMS.ContactManagement/ContactProfile/build";
        var angularLocalizationProvider = Service.Entry<IAngularLocalizationProvider>();

        ScriptHelper.RegisterAngularModule(moduleId, new
        {
            Resources = angularLocalizationProvider.GetAngularLocalization(moduleId),
            PersonaModuleAvailable = ModuleEntryManager.IsModuleLoaded(ModuleName.PERSONAS),
            FormModuleAvailable = ModuleEntryManager.IsModuleLoaded(ModuleName.BIZFORM),
            NewsletterModuleAvailable = ModuleEntryManager.IsModuleLoaded(ModuleName.NEWSLETTER)
        });
    }


    private void UpdateBreadcrumbs()
    {
        ContactInfo contact = (ContactInfo)EditedObject;

        // Refresh breadcrumbs
        ScriptHelper.RefreshTabHeader(Page, contact.ContactDescriptiveName);
    }
}