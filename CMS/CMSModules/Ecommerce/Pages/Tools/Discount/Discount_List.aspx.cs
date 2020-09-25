using System;
using System.Collections.Generic;
using System.Data;

using CMS.Base;
using CMS.Base.Web.UI.ActionsConfig;
using CMS.Core;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.Ecommerce.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.UIControls;


public partial class CMSModules_Ecommerce_Pages_Tools_Discount_Discount_List : CMSEcommercePage
{
    #region "Variables & Properties"

    private ObjectTransformationDataProvider couponCountsDataProvider;

    /// <summary>
    /// Type of discounts which are being listed. By default Order discounts are listed.
    /// </summary>
    private DiscountApplicationEnum DiscountType
    {
        get;
        set;
    }

    #endregion


    #region "Page Events"

    protected override void OnPreInit(EventArgs e)
    {
        SetDiscountType();

        string elementName = "";
        switch (DiscountType)
        {
            case DiscountApplicationEnum.Products:
                elementName = "CatalogDiscounts";
                break;
            case DiscountApplicationEnum.Order:
                elementName = "OrderDiscounts";
                break;
            case DiscountApplicationEnum.Shipping:
                elementName = "ShippingDiscounts";
                break;
        }

        // Check UI personalization
        var uiElement = new UIElementAttribute(ModuleName.ECOMMERCE, elementName);
        uiElement.Check(this);

        base.OnPreInit(e);
    }


    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        couponCountsDataProvider = new ObjectTransformationDataProvider();
        couponCountsDataProvider.SetDefaultDataHandler(GetCountsDataHandler);

        // Init Unigrid
        ugDiscounts.OnAction += ugDiscounts_OnAction;
        ugDiscounts.WhereCondition = "DiscountSiteID = " + SiteID + " AND DiscountApplyTo = '" + DiscountType.ToStringRepresentation() + "'";
        ugDiscounts.OnExternalDataBound += ugDiscounts_OnExternalDataBound;
        ugDiscounts.GridView.AllowSorting = false;

        switch (DiscountType)
        {
            case DiscountApplicationEnum.Products:
                AddHeaderAction(new HeaderAction
                {
                    Text = GetString("com.discount.newcatalogdiscount"),
                    RedirectUrl = UIContextHelper.GetElementUrl(ModuleName.ECOMMERCE, "NewCatalogDiscount", false)
                });

                SetTitle(GetString("com.discount.catalogdiscounts"));

                // Set empty grid messages
                ugDiscounts.ZeroRowsText = GetString("com.nocatalogdiscounts");
                ugDiscounts.FilteredZeroRowsText = GetString("com.nocatalogdiscounts.filtered");
                break;

            case DiscountApplicationEnum.Order:
                AddHeaderAction(new HeaderAction
                {
                    Text = GetString("com.discount.neworderdiscount"),
                    RedirectUrl = UIContextHelper.GetElementUrl(ModuleName.ECOMMERCE, "NewOrderDiscount", false)
                });

                SetTitle(GetString("com.discount.orderdiscounts"));

                // Set empty grid messages
                ugDiscounts.ZeroRowsText = GetString("com.noorderdiscounts");
                ugDiscounts.FilteredZeroRowsText = GetString("com.noorderdiscounts.filtered");
                break;

            case DiscountApplicationEnum.Shipping:
                AddHeaderAction(new HeaderAction
                {
                    Text = GetString("com.discount.newshippingdiscount"),
                    RedirectUrl = UIContextHelper.GetElementUrl(ModuleName.ECOMMERCE, "NewShippingDiscount", false)
                });

                SetTitle(GetString("com.discount.shippingdiscounts"));

                // Set empty grid messages
                ugDiscounts.ZeroRowsText = GetString("com.noshippingdiscounts");
                ugDiscounts.FilteredZeroRowsText = GetString("com.noshippingdiscounts.filtered");
                break;
        }
    }


    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        switch (DiscountType)
        {
            case DiscountApplicationEnum.Products:
                ugDiscounts.NamedColumns["OrderAmount"].Visible = false;
                ugDiscounts.NamedColumns["Application"].Visible = false;
                break;

            case DiscountApplicationEnum.Order:
                break;

            case DiscountApplicationEnum.Shipping:
                ugDiscounts.NamedColumns["ApplyFurtherDiscounts"].Visible = false;
                ugDiscounts.NamedColumns["Value"].Visible = false;
                ugDiscounts.NamedColumns["Order"].Visible = false;
                break;
        }
    }


    #endregion


    #region "Event Handlers"

    protected void ugDiscounts_OnAction(string actionName, object actionArgument)
    {
        if (actionName.ToLowerCSafe() == "edit")
        {
            string elementName = null;
            switch (DiscountType)
            {
                case DiscountApplicationEnum.Products:
                    elementName = "EditCatalogDiscount";
                    break;

                case DiscountApplicationEnum.Order:
                    elementName = "EditOrderDiscount";
                    break;

                case DiscountApplicationEnum.Shipping:
                    elementName = "EditShippingDiscount";
                    break;
            }

            if (!string.IsNullOrEmpty(elementName))
            {
                var url = UIContextHelper.GetElementUrl(ModuleName.ECOMMERCE, elementName, false, actionArgument.ToInteger(0));
                URLHelper.Redirect(url);
            }
        }
    }


    protected object ugDiscounts_OnExternalDataBound(object sender, string sourceName, object parameter)
    {
        DataRowView discountRow = parameter as DataRowView;
        DiscountInfo discountInfo = null;

        if (discountRow != null)
        {
            discountInfo = new DiscountInfo(discountRow.Row);
        }
        if (discountInfo == null)
        {
            return String.Empty;
        }

        switch (sourceName.ToLowerCSafe())
        {
            // Append to a value field char '%' or site currency in case of flat discount
            case "value":
                if ((DiscountType == DiscountApplicationEnum.Shipping) && (!discountInfo.DiscountIsFlat) && (discountInfo.DiscountValue == 100))
                {
                    return GetString("general.free");
                }

                return ((discountInfo.DiscountIsFlat) ? CurrencyInfoProvider.GetFormattedPrice(discountInfo.DiscountValue, discountInfo.DiscountSiteID) : discountInfo.DiscountValue + "%");

            // Display discount status
            case "status":
                // Ensure correct values for unigrid export
                if (sender == null)
                {
                    return discountInfo.DiscountStatus.ToLocalizedString("com.discountstatus");
                }
                return new DiscountStatusTag(couponCountsDataProvider, discountInfo);

            case "discountorder":
                // Ensure correct values for unigrid export
                if ((sender == null) || !ECommerceContext.IsUserAuthorizedToModifyDiscount())
                {
                    return discountInfo.DiscountOrder;
                }

                return new PriorityInlineEdit
                {
                    PrioritizableObject = discountInfo,
                    Unigrid = ugDiscounts
                };

            case "application":

                // Display blank value if discount don't use coupons 
                if (!discountInfo.DiscountUsesCoupons)
                {
                    return "&mdash;";
                }

                var tr = new ObjectTransformation("CouponsCounts", discountInfo.DiscountID)
                {
                    DataProvider = couponCountsDataProvider,
                    Transformation = "{% FormatString(GetResourceString(\"com.couponcode.appliedxofy\"), Convert.ToString(Uses, \"0\"), (UnlimitedCodeCount != 0)? GetResourceString(\"com.couponcode.unlimited\") : Convert.ToString(Limit, \"0\")) %}",
                    NoDataTransformation = "{$com.discount.notcreated$}",
                    EncodeOutput = false
                };

                return tr;

            case "orderamount":
                double totalPriceInMainCurrency = ValidationHelper.GetDouble(discountInfo.DiscountItemMinOrderAmount, 0);

                // Display blank value in the discount listing if order amount is not configured
                if (totalPriceInMainCurrency == 0)
                {
                    return string.Empty;
                }
                // Format currency
                string priceInMainCurrencyFormatted = CurrencyInfoProvider.GetFormattedPrice(totalPriceInMainCurrency, SiteContext.CurrentSiteID);

                return HTMLHelper.HTMLEncode(priceInMainCurrencyFormatted);
        }

        return parameter;
    }

    #endregion


    #region "Private Methods"

    /// <summary>
    /// Returns dictionary of discount coupon use count and limit. Key of the dictionary is the ID of discount.
    /// </summary>
    /// <param name="type">Object type (ignored).</param>
    /// <param name="discountIDs">IDs of discount which the dictionary is to be filled with.</param>
    private SafeDictionary<int, IDataContainer> GetCountsDataHandler(string type, IEnumerable<int> discountIDs)
    {
        DataSet countsDs = CouponCodeInfoProvider.GetCouponCodeUseCount(discountIDs);

        return countsDs.ToDictionaryById("CouponCodeDiscountID");
    }


    /// <summary>
    /// Sets type of discounts which are being listed.
    /// </summary>
    private void SetDiscountType()
    {
        // Return Discount type
        var enumValueIndex = QueryHelper.GetInteger("type", 0);

        if (Enum.IsDefined(typeof(DiscountApplicationEnum), enumValueIndex))
        {
            DiscountType = (DiscountApplicationEnum)enumValueIndex;
        }
        else
        {
            DiscountType = EnumHelper.GetDefaultValue<DiscountApplicationEnum>();
        }
    }

    #endregion
}