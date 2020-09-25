using System;

using CMS.DataEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.UIControls;


public partial class CMSModules_Ecommerce_FormControls_CustomerSelector : FormEngineUserControl
{
    private string mAdditionalItems = "";
    private bool mDisplayRegisteredCustomers = true;
    private bool mDisplayAnonymousCustomers = true;


    /// <summary>
    /// Gets or sets the field value.
    /// </summary>
    public override object Value
    {
        get
        {
            return CustomerID;
        }
        set
        {
            CustomerID = ValidationHelper.GetInteger(value, 0);
        }
    }


    /// <summary>
    /// Gets or sets the Customer ID.
    /// </summary>
    public int CustomerID
    {
        get
        {
            return ValidationHelper.GetInteger(uniSelector.Value, 0);
        }
        set
        {
            uniSelector.Value = value;
        }
    }


    /// <summary>
    /// Gets or sets the enabled state of the control.
    /// </summary>
    public override bool Enabled
    {
        get
        {
            return base.Enabled;
        }
        set
        {
            base.Enabled = value;
            if (uniSelector != null)
            {
                uniSelector.Enabled = value;
            }
        }
    }


    /// <summary>
    /// Returns ClientID of the dropdownlist.
    /// </summary>
    public override string ValueElementID
    {
        get
        {
            return uniSelector.TextBoxSelect.ClientID;
        }
    }


    /// <summary>
    /// Indicates if anonymous customers are to be displayed.
    /// </summary>
    public bool DisplayAnonymousCustomers
    {
        get
        {
            return mDisplayAnonymousCustomers;
        }
        set
        {
            mDisplayAnonymousCustomers = value;
        }
    }


    /// <summary>
    /// Indicates if registered customers are to be displayed.
    /// </summary>
    public bool DisplayRegisteredCustomers
    {
        get
        {
            return mDisplayRegisteredCustomers;
        }
        set
        {
            mDisplayRegisteredCustomers = value;
        }
    }


    /// <summary>
    /// Id of items which has to be displayed regardless other settings. Use ',' or ';' as separator if more ids required.
    /// </summary>
    public string AdditionalItems
    {
        get
        {
            return mAdditionalItems;
        }
        set
        {
            // Prevent from setting null value
            if (value != null)
            {
                mAdditionalItems = value.Replace(';', ',');
            }
            else
            {
                mAdditionalItems = "";
            }
        }
    }


    /// <summary>
    /// Gets the current UniSelector instance.
    /// </summary>
    public UniSelector UniSelector
    {
        get
        {
            EnsureChildControls();
            return uniSelector;
        }
    }


    protected void Page_Load(object sender, EventArgs e)
    {
        if (StopProcessing)
        {
            uniSelector.StopProcessing = true;
        }
        else
        {
            InitSelector();
        }
    }


    /// <summary>
    /// Initializes the selector.
    /// </summary>
    public void InitSelector()
    {
        uniSelector.IsLiveSite = IsLiveSite;
        uniSelector.AdditionalSearchColumns = "CustomerFirstName,CustomerCompany,CustomerEmail";

        // Set resource prefix if specified
        if (!string.IsNullOrEmpty(ResourcePrefix))
        {
            uniSelector.ResourcePrefix = ResourcePrefix;
        }

        var where = new WhereCondition();

        // Do not filter customer by UserID if both Anonymous and Registered are requested
        if (!DisplayRegisteredCustomers || !DisplayAnonymousCustomers)
        {
            // Add registered customers
            if (DisplayRegisteredCustomers)
            {
                where.WhereNotNull("CustomerUserID");
            }

            // Add anonymous customers
            if (DisplayAnonymousCustomers)
            {
                where.WhereNull("CustomerUserID");
            }
        }

        // Add items which have to be on the list
        if (!string.IsNullOrEmpty(AdditionalItems) && !where.WhereIsEmpty)
        {
            where.Or().WhereIn("CustomerID", AdditionalItems.Split(','));
        }

        // Selected value must be on the list
        if ((CustomerID > 0) && !where.WhereIsEmpty)
        {
            where.Or().WhereEquals("CustomerID", CustomerID);
        }

        uniSelector.WhereCondition = where.ToString(true);
    }
}