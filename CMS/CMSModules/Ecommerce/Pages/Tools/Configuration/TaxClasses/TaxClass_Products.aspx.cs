﻿using System;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.Ecommerce.Web.UI;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.UIControls;


[UIElement(ModuleName.ECOMMERCE, "Configuration.TaxClasses.Products")]
public partial class CMSModules_Ecommerce_Pages_Tools_Configuration_TaxClasses_TaxClass_Products : CMSTaxClassesPage
{
    #region "Variables"

    protected int mTaxClassId = 0;
    protected int mCurrentSiteId = SiteContext.CurrentSiteID;
    protected string mCurrentValues = string.Empty;
    protected TaxClassInfo mTaxClassInfoObj = null;
    protected CurrentUserInfo cu = MembershipContext.AuthenticatedUser;

    #endregion


    #region "Page events"

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        uniSelector.OnSelectionChanged += uniSelector_OnSelectionChanged;
    }


    protected void Page_Load(object sender, EventArgs e)
    {
        mTaxClassId = QueryHelper.GetInteger("objectid", 0);
        if (mTaxClassId > 0)
        {
            mTaxClassInfoObj = TaxClassInfoProvider.GetTaxClassInfo(mTaxClassId);
            EditedObject = mTaxClassInfoObj;

            if (mTaxClassInfoObj != null)
            {
                int editedSiteId = mTaxClassInfoObj.TaxClassSiteID;
                // Check object's site id
                CheckEditedObjectSiteID(editedSiteId);

                // Offer global products when allowed
                bool offerGlobalProducts;
                if (editedSiteId != 0)
                {
                    offerGlobalProducts = ECommerceSettings.AllowGlobalProducts(editedSiteId);
                }
                // Configuring global products
                else
                {
                    offerGlobalProducts = ECommerceSettings.AllowGlobalProducts(CurrentSiteName);
                }

                PreloadUniSelector(offerGlobalProducts);
                uniSelector.WhereCondition = GetSelectorWhereCondition(offerGlobalProducts);
            }
        }
    }

    #endregion


    #region "Event handlers"

    protected void uniSelector_OnSelectionChanged(object sender, EventArgs e)
    {
        if (mTaxClassInfoObj == null)
        {
            return;
        }

        // Check permissions
        if ((cu == null) || (!cu.IsAuthorizedPerResource(ModuleName.ECOMMERCE, EcommercePermissions.ECOMMERCE_MODIFY) && !cu.IsAuthorizedPerResource(ModuleName.ECOMMERCE, EcommercePermissions.PRODUCTS_MODIFY)))
        {
            RedirectToAccessDenied(ModuleName.ECOMMERCE, "EcommerceModify OR ModifyProducts");
        }

        else
        {
            // Remove old items
            string newValues = ValidationHelper.GetString(uniSelector.Value, null);
            string items = DataHelper.GetNewItemsInList(newValues, mCurrentValues);
            if (!String.IsNullOrEmpty(items))
            {
                string[] newItems = items.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                // Remove all old items from tax class
                foreach (string item in newItems)
                {
                    int skuId = ValidationHelper.GetInteger(item, 0);
                    SKUTaxClassInfoProvider.RemoveTaxClassFromSKU(mTaxClassId, skuId);
                }
            }

            // Add new items
            items = DataHelper.GetNewItemsInList(mCurrentValues, newValues);
            if (!String.IsNullOrEmpty(items))
            {
                string[] newItems = items.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                // Add all new items to tax class
                foreach (string item in newItems)
                {
                    int skuId = ValidationHelper.GetInteger(item, 0);
                    SKUTaxClassInfoProvider.AddTaxClassToSKU(mTaxClassId, skuId);
                }
            }

            // Show message
            ShowChangesSaved();
        }
    }

    #endregion


    #region "Methods"

    /// <summary>
    /// Creates where condition for product selector.
    /// </summary>
    protected string GetSelectorWhereCondition(bool offerGlobalProducts)
    {
        // Select enabled products
        string where = "SKUOptionCategoryID IS NULL";

        if ((mTaxClassInfoObj != null))
        {
            // Add site specific product records
            if ((mTaxClassInfoObj.TaxClassSiteID > 0) || (!offerGlobalProducts) || !cu.IsAuthorizedPerResource(ModuleName.ECOMMERCE, EcommercePermissions.ECOMMERCE_MODIFYGLOBAL))
            {
                where = SqlHelper.AddWhereCondition(where, "SKUSiteID = " + mCurrentSiteId);
            }
            // Add site specific and global product records
            else
            {
                where = SqlHelper.AddWhereCondition(where, "SKUSiteID = " + mCurrentSiteId + " OR SKUSiteID IS NULL");
            }
        }

        return where;
    }


    /// <summary>
    /// Preloads uniSelector and selects actual data.
    /// </summary>
    protected void PreloadUniSelector(bool offerGlobalProducts)
    {
        // Get the active products
        var products = SKUInfoProvider.GetSKUs().Column("SKUID")
                                      .WhereIn("SKUID", SKUTaxClassInfoProvider.GetSKUTaxClasses().Column("SKUID").WhereEquals("TaxClassID", mTaxClassId))
                                      .WhereNull("SKUOptionCategoryID");

        // Restrict site
        var includeGlobal = offerGlobalProducts && (cu.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || cu.IsAuthorizedPerResource(ModuleName.ECOMMERCE, EcommercePermissions.ECOMMERCE_MODIFYGLOBAL));
        products.OnSite(mCurrentSiteId, includeGlobal);

        if (!DataHelper.DataSourceIsEmpty(products))
        {
            mCurrentValues = TextHelper.Join(";", DataHelper.GetStringValues(products.Tables[0], "SKUID"));
        }

        if (!RequestHelper.IsPostBack())
        {
            uniSelector.Value = mCurrentValues;
        }
    }

    #endregion
}