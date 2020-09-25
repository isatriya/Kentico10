using System;
using System.Linq;

using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.Ecommerce.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;


/// <summary>
/// Customer registration web part for checkout process
/// </summary>
public partial class CMSWebParts_Ecommerce_Checkout_Forms_CustomerDetail : CMSCheckoutWebPart
{
    #region "Constants"

    const string COMPANY_TYPE = "Company";

    #endregion


    #region "Properties"

    /// <summary>
    /// Gets or sets a value indicating whether propagating changes on postback is allowed. 
    /// </summary>    
    /// <remarks>
    /// Use this property if dummy AccountType selector field is defined in customer`s alternative form set in <see cref="AlternativeFormName"/> property.
    /// </remarks>
    public bool PropagateChangesOnPostback
    {
        get
        {
            return ValidationHelper.GetBoolean(GetValue("PropagateChangesOnPostback"), false);
        }
        set
        {
            SetValue("PropagateChangesOnPostback", value);
        }
    }


    /// <summary>
    /// Alternative form name for this web part.
    /// </summary>
    public string AlternativeFormName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("AlternativeFormName"), "");
        }
        set
        {
            SetValue("AlternativeFormName", value);
        }
    }


    /// <summary>
    /// Gets the customer type selector [Personal/Company]. Returns null if alternative form does not include this field.
    /// </summary>    
    private FormEngineUserControl TypeSelector
    {
        get
        {
            return customerForm.FieldControls["AccountType"];
        }
    }

    #endregion


    #region "Life cycle"

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        customerForm.OnBeforeSave += customerForm_OnBeforeSave;

        string[] splitFormName = AlternativeFormName.Split('.');
        // UIForm cant process full path of alternative form if object type is already specified.
        customerForm.AlternativeFormName = splitFormName.LastOrDefault();

        InitCustomerForm();
    }


    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);
        customerForm.SubmitButton.Visible = false;
    }

    #endregion


    #region "Form events"

    protected void customerForm_OnBeforeSave(object sender, EventArgs e)
    {
        // Cancel saving, just set current filed values into EditableObject through UIForm.SaveData method
        customerForm.StopProcessing = true;
    }

    #endregion


    #region "Wizard methods"

    protected override void StepLoaded(object sender, StepEventArgs e)
    {
        base.StepLoaded(sender, e);

        // The default value is Personal, but if customer has some company fields filled, the Company account type should be selected
        if (!RequestHelper.IsPostBack())
        {
            SetCompanyAccountType();
        }

        // Propagate changes on postback if there is customer with company type and some tax registration id
        if (PropagateChangesOnPostback && (ShoppingCart.Customer != null))
        {
            ChangeTaxRegistrationIdBasedOnAccountType();

            ShoppingCart.InvalidateCalculations();
            ComponentEvents.RequestEvents.RaiseEvent(null, e, SHOPPING_CART_CHANGED);
        }
    }


    protected override void ValidateStepData(object sender, StepEventArgs e)
    {
        base.ValidateStepData(sender, e);

        if (!customerForm.ValidateData())
        {
            if (e != null)
            {
                e.CancelEvent = true;
            }
        }
    }


    protected override void SaveStepData(object sender, StepEventArgs e)
    {
        base.SaveStepData(sender, e);

        // Just set current filed values into EditableObject, saving was canceled in OnBeforeSave
        customerForm.SaveData(null, false);

        var customer = customerForm.EditedObject as CustomerInfo;
        var typeSelector = TypeSelector;

        // Clear company fields for non-company type
        if ((customer != null) && (typeSelector != null) && !typeSelector.Value.Equals(COMPANY_TYPE))
        {
            customer.CustomerCompany = "";
            customer.CustomerOrganizationID = "";
            customer.CustomerTaxRegistrationID = "";
        }

        var cart = ShoppingCart;

        // Assign save customer to the shopping cart -> will be needed for addresses
        cart.Customer = SaveCustomer(customer, cart.ShoppingCartUserID, cart.ShoppingCartSiteID);

        // Address cannot be inserted before customer is saved (otherwise it would be an orphan address)
        // In case of address update - AddressName and AddressPersonalName should be re-generated according current customer info (if not present on the address form)
        if (((cart.ShoppingCartBillingAddress != null) ||
            (cart.ShoppingCartShippingAddress != null) ||
            (cart.ShoppingCartCompanyAddress != null))
            && (customer != null) && (customer.CustomerID > 0))
        {
            SaveCustomerAddresses(cart);
        }

        ShoppingCartInfoProvider.SetShoppingCartInfo(cart);

        // Update contact with customer details
        ModuleCommands.OnlineMarketingUpdateContactFromExternalData(customer, ContactID);
    }

    #endregion


    private void InitCustomerForm()
    {
        if (ShoppingCart.Customer != null)
        {
            customerForm.EditedObject = ShoppingCart.Customer;
        }

        if (!RequestHelper.IsPostBack())
        {
            customerForm.ReloadData();
        }

        // Set first time user customer for postback tax recalculation
        if (PropagateChangesOnPostback && (ShoppingCart.Customer == null))
        {
            ShoppingCart.Customer = customerForm.EditedObject as CustomerInfo;
        }
    }


    private void ChangeTaxRegistrationIdBasedOnAccountType()
    {
        var typeSelector = TypeSelector;
        var isPersonalType = (typeSelector != null) && !typeSelector.Value.Equals(COMPANY_TYPE);

        ShoppingCart.Customer.CustomerTaxRegistrationID = isPersonalType ? string.Empty : ValidationHelper.GetString(customerForm.GetFieldValue("CustomerTaxRegistrationID"), string.Empty);
    }


    private void SetCompanyAccountType()
    {
        var typeSelector = TypeSelector;

        if (typeSelector != null && CustomerRepresentsCompany(ShoppingCart.Customer))
        {
            typeSelector.Value = COMPANY_TYPE;
        }
    }


    private static bool CustomerRepresentsCompany(CustomerInfo customer)
    {
        return (customer != null) && (!string.IsNullOrEmpty(customer.CustomerOrganizationID) ||
                                     !string.IsNullOrEmpty(customer.CustomerTaxRegistrationID) ||
                                     !string.IsNullOrEmpty(customer.CustomerCompany));
    }
}