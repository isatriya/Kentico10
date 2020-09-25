using System;
using System.Data;

using CMS.Base;
using CMS.Core;
using CMS.Ecommerce;
using CMS.Ecommerce.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.UIControls;


[Title("DiscounCoupon_Edit.ItemListLink")]
[Action(0, "DiscounCoupon_List.NewItemCaption", "{%UIContextHelper.GetElementUrl(\"CMS.Ecommerce\", \"NewDiscountCoupon\", \"false\")|(encode)false%}")]

[UIElement(ModuleName.ECOMMERCE, "DiscountCoupons")]
public partial class CMSModules_Ecommerce_Pages_Tools_DiscountCoupons_DiscountCoupon_List : CMSDiscountCouponsPage
{
    #region "Page Events"

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        // Init Unigrid
        gridElem.OnAction += gridElem_OnAction;
        gridElem.OnExternalDataBound += gridElem_OnExternalDataBound;
        // Set empty grid message
        gridElem.ZeroRowsText = GetString("com.noproductcoupon");

        HandleGridsSiteIDColumn(gridElem);

        if (AllowGlobalObjects && ECommerceContext.IsExchangeRateFromGlobalMainCurrencyMissing)
        {
            ShowWarning(GetString("com.NeedExchangeRateFromGlobal"));
        }

        // Filter records by site
        gridElem.WhereCondition = InitSiteWhereCondition("DiscountCouponSiteID").ToString(true);
    }

    #endregion


    #region "Event Handlers"

    protected object gridElem_OnExternalDataBound(object sender, string sourceName, object parameter)
    {
        switch (sourceName.ToLowerCSafe())
        {
            case "couponvalue":
                DataRowView row = (DataRowView)parameter;
                double value = ValidationHelper.GetDouble(row["DiscountCouponValue"], 0);
                bool isFlat = ValidationHelper.GetBoolean(row["DiscountCouponIsFlatValue"], false);
                int siteId = ValidationHelper.GetInteger(row["DiscountCouponSiteID"], 0);

                if (isFlat)
                {
                    return CurrencyInfoProvider.GetFormattedPrice(value, siteId);
                }

                return value + "%";
        }

        return parameter;
    }


    /// <summary>
    /// Handles the UniGrid's OnAction event.
    /// </summary>
    /// <param name="actionName">Name of item (button) that throws event</param>
    /// <param name="actionArgument">ID (value of Primary key) of corresponding data row</param>
    protected void gridElem_OnAction(string actionName, object actionArgument)
    {
        if (actionName == "edit")
        {
            // Discount coupon detail url
            var redirectURL = UIContextHelper.GetElementUrl(ModuleName.ECOMMERCE, "EditDiscountCouponProperties", false, actionArgument.ToInteger(0));
            redirectURL = URLHelper.AddParameterToUrl(redirectURL, "siteid", SiteContext.CurrentSiteID.ToString());

            URLHelper.Redirect(redirectURL);
        }
    }

    #endregion
}