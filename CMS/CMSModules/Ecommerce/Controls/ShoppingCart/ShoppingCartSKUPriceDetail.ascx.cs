using System;
using System.Linq;

using CMS.Ecommerce;
using CMS.Helpers;
using CMS.UIControls;


public partial class CMSModules_Ecommerce_Controls_ShoppingCart_ShoppingCartSKUPriceDetail_Control : CMSUserControl
{
    #region "Variables"

    private Guid mCartItemGuid = Guid.Empty;
    private ShoppingCartItemInfo mShoppingCartItem;

    #endregion


    #region "Properties"

    /// <summary>
    /// Shopping cart.
    /// </summary>
    public ShoppingCartInfo ShoppingCart
    {
        get;
        set;
    }


    /// <summary>
    /// If true product options are shown in detail.
    /// </summary>    
    public bool IncludeOptions
    {
        get;
        set;
    }


    /// <summary>
    /// Shopping cart item GUID.
    /// </summary>
    public Guid CartItemGuid
    {
        get
        {
            return mCartItemGuid;
        }
        set
        {
            mCartItemGuid = value;
        }
    }


    /// <summary>
    /// Shopping cart item.
    /// </summary>
    public ShoppingCartItemInfo ShoppingCartItem
    {
        get
        {
            if (mShoppingCartItem == null)
            {
                if ((CartItemGuid != Guid.Empty) && (ShoppingCart != null))
                {
                    // Get item from shopping cart
                    mShoppingCartItem = ShoppingCart.GetShoppingCartItem(CartItemGuid);
                }
            }

            return mShoppingCartItem;
        }
    }

    #endregion


    #region "Methods"

    protected void Page_Load(object sender, EventArgs e)
    {
        // Check if ShoppingCartItem exists
        if (ShoppingCartItem == null)
        {
            RedirectToInformation("general.ObjectNotFound");
            return;
        }

        // Discounts table header
        gridDiscounts.Columns[0].HeaderText = GetString("ProductPriceDetail.Discounts");

        // Discounts
        gridDiscounts.DataSource = ShoppingCartItem.DiscountsTable;
        gridDiscounts.DataBind();

        double optionsTotalPrice = 0;

        var accessories = ShoppingCartItem.ProductOptions.Where(o => o.IsAccessoryProduct).ToList();
        // Bind and show product options if they should be included in detail view
        if (IncludeOptions && accessories.Any())
        {
            OptionsUniView.Visible = true;
            OptionsUniView.DataSource = accessories;
            OptionsUniView.DataBind();

            // Compute product options prices which will be added to products total prices
            optionsTotalPrice += accessories.Sum(option => option.TotalPrice);
        }

        // Display unit price without tax
        lblPriceWithoutTaxValue.Text = ShoppingCart.GetFormattedPrice(ShoppingCartItem.UnitPrice);
        lblPriceWithoutTax.Text = GetString("ProductPriceDetail.PriceWithoutTax");

        // Display  unit price after discount
        lblPriceAfterDiscountValue.Text = ShoppingCart.GetFormattedPrice(ShoppingCartItem.UnitPriceAfterDiscount);
        lblTotalDiscountValue.Text = ShoppingCart.GetFormattedPrice(ShoppingCartItem.UnitTotalDiscount);

        // Display unit total tax and price with tax
        lblTotalTaxValue.Text = ShoppingCart.GetFormattedPrice(ShoppingCartItem.UnitTotalTax);
        lblPriceWithTaxValue.Text = ShoppingCart.GetFormattedPrice(ShoppingCartItem.UnitTotalPrice);

        lblTotalPriceValue.Text = ShoppingCart.GetFormattedPrice(ShoppingCartItem.TotalPrice + optionsTotalPrice, true);

        // Product name and units
        if ((ShoppingCartItem != null) && (ShoppingCartItem.SKU != null))
        {
            lblProductName.Text = HTMLHelper.HTMLEncode(ResHelper.LocalizeString(ShoppingCartItem.SKU.SKUName));
            lblProductUnitsValue.Text = ShoppingCartItem.CartItemUnits.ToString();
        }

        // Show subtotals
        plcTotalDiscount.Visible = (ShoppingCartItem.DiscountsTable.Rows.Count != 1);

        // Show tax total
        plcTotalTax.Visible = ShoppingCartItem.UnitTotalTax > 0;

        // Show discount total
        plcDiscounts.Visible = gridDiscounts.Rows.Count == 0;
    }


    /// <summary>
    /// Returns formatted tax/discount name.
    /// </summary>
    /// <param name="name">Tax/discount name</param>
    protected string GetFormattedName(object name)
    {
        return HTMLHelper.HTMLEncode(" - " + ResHelper.LocalizeString(Convert.ToString(name)));
    }


    /// <summary>
    /// Returns formatted value string.
    /// </summary>
    /// <param name="value">Value to be formatted</param>
    /// <param name="isFlat">True - it is a flat value, False - it is a relative value</param>
    protected string GetFormattedValue(object value, object isFlat)
    {
        bool mIsFlat = ValidationHelper.GetBoolean(isFlat, false);
        double mValue = ValidationHelper.GetDouble(value, 0);

        if (mIsFlat)
        {
            // Flat value
            return ShoppingCart.GetFormattedPrice(mValue);
        }

        // Relative value
        return mValue + "%";
    }

    #endregion
}