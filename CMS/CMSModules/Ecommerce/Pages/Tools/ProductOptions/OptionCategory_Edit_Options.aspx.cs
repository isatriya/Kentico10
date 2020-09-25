using System;
using System.Data;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Base.Web.UI.ActionsConfig;
using CMS.Core;
using CMS.Ecommerce;
using CMS.Ecommerce.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.Modules;


public partial class CMSModules_Ecommerce_Pages_Tools_ProductOptions_OptionCategory_Edit_Options : CMSProductOptionCategoriesPage
{
    #region "Variables"

    protected int categoryId = 0;
    protected OptionCategoryInfo categoryObj = null;
    protected int editedSiteId = 0;
    protected bool allowActions = true;
    protected int parentProductId;

    #endregion


    #region "Page events"

    protected void Page_Load(object sender, EventArgs e)
    {
        // Get parent product from url
        parentProductId = QueryHelper.GetInteger("productId", 0);

        // Check UI permissions
        if (parentProductId <= 0)
        {
            // UIElement from option category list
            CheckUIElementAccessHierarchical(ModuleName.ECOMMERCE, "ProductOptions.Options");
        }
        else
        {
            // UIElement from product edit
            CheckUIElementAccessHierarchical(ModuleName.ECOMMERCE, "Products.ProductOptions.Options");
        }

        // Hide/rename columns before loading data 
        ugOptions.OnBeforeDataReload += grid_OnBeforeDataReload;

        // Get category ID and department ID from URL
        categoryId = QueryHelper.GetInteger("categoryid", 0);
        categoryObj = OptionCategoryInfoProvider.GetOptionCategoryInfo(categoryId);

        EditedObject = categoryObj;

        if (categoryObj != null)
        {
            editedSiteId = categoryObj.CategorySiteID;

            // Check edited objects site id
            CheckEditedObjectSiteID(editedSiteId);

            // Allow actions only for non-text categories
            allowActions = (categoryObj.CategorySelectionType != OptionCategorySelectionTypeEnum.TextBox) && (categoryObj.CategorySelectionType != OptionCategorySelectionTypeEnum.TextArea);

            ugOptions.ShowObjectMenu = allowActions;

            if (allowActions)
            {
                var hdrActions = CurrentMaster.HeaderActions;

                string newButtonText;
                if (categoryObj.CategoryType == OptionCategoryTypeEnum.Products)
                {
                    newButtonText = GetString("com.sku.newproduct");
                }
                else
                {
                    newButtonText = GetString("com.productoptions.newoption");
                }

                // New option action
                hdrActions.ActionsList.Add(new HeaderAction
                {
                    Text = newButtonText,
                    RedirectUrl = ResolveUrl("~/CMSModules/Ecommerce/Pages/Tools/Products/Product_New.aspx?categoryid=" + categoryId + "&siteId=" + SiteID + (parentProductId > 0 ? "&parentProductId=" + parentProductId : ""))
                });

                // Sort action
                hdrActions.ActionsList.Add(new HeaderAction
                {
                    Text = GetString("ProductOptions.SortAlphabetically"),
                    OnClientClick = "return confirm(" + ScriptHelper.GetLocalizedString("com.productoptions.sortalphaasc") + ");",
                    CommandName = "lnkSort_Click",
                    ButtonStyle = ButtonStyle.Default
                });

                hdrActions.ActionPerformed += HeaderActions_ActionPerformed;
            }

            // Unigrid
            ugOptions.OnAction += grid_OnAction;
            ugOptions.OnExternalDataBound += grid_OnExternalDataBound;
            ugOptions.WhereCondition = "SKUOptionCategoryID = " + categoryId;
            ugOptions.GridView.AllowSorting = false;
        }
    }

    #endregion


    #region "Event handlers"

    /// <summary>
    /// Hide/rename columns in uniGrid before loading data.
    /// </summary>
    void grid_OnBeforeDataReload()
    {
        switch (categoryObj.CategoryType)
        {
            case OptionCategoryTypeEnum.Attribute:
            case OptionCategoryTypeEnum.Text:
                ugOptions.NamedColumns["SKUName"].HeaderText = GetString("unigrid.productoptions.OptionName");
                ugOptions.NamedColumns["SKUNumber"].Visible = false;
                ugOptions.NamedColumns["SKUDepartment"].Visible = false;
                ugOptions.NamedColumns["SKUAvailableItems"].Visible = false;
                break;
        }

        // Hide price adjustment if category is used in product variants
        if (parentProductId > 0)
        {
            if (VariantHelper.AreCategoriesUsedInVariants(QueryHelper.GetInteger("productId", 0), new[] { categoryObj.CategoryID }))
            {
                ugOptions.NamedColumns["SKUPrice"].Visible = false;
            }
        }

    }


    private object grid_OnExternalDataBound(object sender, string sourceName, object parameter)
    {
        switch (sourceName.ToLowerCSafe())
        {
            case "skuprice":
                var optionRow = parameter as DataRowView;
                var option = new SKUInfo(optionRow.Row);

                if (sender == null)
                {
                    return option.SKUPrice;
                }

                var inlineSkuPrice = new InlineEditingTextBox();
                inlineSkuPrice.Text = option.SKUPrice.ToString();

                inlineSkuPrice.Formatting += (s, e) =>
                {
                    // Format price
                    inlineSkuPrice.FormattedText = CurrencyInfoProvider.GetRelativelyFormattedPrice(option.SKUPrice, option.SKUSiteID);
                };

                inlineSkuPrice.Update += (s, e) =>
                {
                    CheckModifyPermission();

                    var valid = false;
                    var price = option.SKUPrice;
                    // Update price if new value is valid
                    if (ValidationHelper.IsDouble(inlineSkuPrice.Text))
                    {
                        price = ValidationHelper.GetDouble(inlineSkuPrice.Text, option.SKUPrice);
                        // Accessory price can not be negative
                        if (!option.IsAccessoryProduct || !(price < 0.0))
                        {
                            valid = true;
                        }
                    }

                    if (valid)
                    {
                        option.SKUPrice = price;
                        option.MakeComplete(true);
                        option.Update();

                        ugOptions.ReloadData();
                    }
                    else
                    {
                        inlineSkuPrice.ErrorText = GetString("com.productedit.priceinvalid");
                    }
                };

                return inlineSkuPrice;

            case "skuavailableitems":
                var row = parameter as DataRowView;
                var optionStock = new SKUInfo(row.Row);

                int availableItems = optionStock.SKUAvailableItems;

                // Inventory tracking disabled
                if (optionStock.SKUTrackInventory == TrackInventoryTypeEnum.Disabled)
                {
                    return GetString("com.inventory.nottracked");
                }

                // Ensure correct values for unigrid export
                if (sender == null)
                {
                    return availableItems;
                }

                var inlineSkuAvailableItems = new InlineEditingTextBox();
                inlineSkuAvailableItems.Text = availableItems.ToString();
                inlineSkuAvailableItems.EnableEncode = false;

                inlineSkuAvailableItems.Formatting += (s, e) =>
                {
                    var reorderAt = optionStock.SKUReorderAt;

                    // Emphasize the number when product needs to be reordered
                    if (availableItems <= reorderAt)
                    {
                        // Format message informing about insufficient stock level
                        string reorderMsg = string.Format(GetString("com.sku.reorderatTooltip"), reorderAt);
                        string message = string.Format("<span class=\"alert-status-error\" onclick=\"UnTip()\" onmouseout=\"UnTip()\" onmouseover=\"Tip('{1}')\">{0}</span>", availableItems, reorderMsg);
                        inlineSkuAvailableItems.FormattedText = message;
                    }
                };

                inlineSkuAvailableItems.Update += (s, e) =>
                {
                    CheckModifyPermission();

                    var newNumberOfItems = ValidationHelper.GetInteger(inlineSkuAvailableItems.Text, availableItems);

                    // Update available items if new value is valid
                    if (ValidationHelper.IsInteger(inlineSkuAvailableItems.Text) && (-1000000000 <= newNumberOfItems) && (newNumberOfItems <= 1000000000))
                    {
                        optionStock.SKUAvailableItems = ValidationHelper.GetInteger(inlineSkuAvailableItems.Text, availableItems);
                        optionStock.MakeComplete(true);
                        optionStock.Update();

                        ugOptions.ReloadData();
                    }
                    else
                    {
                        inlineSkuAvailableItems.ErrorText = GetString("com.productedit.availableitemsinvalid");
                    }
                };

                return inlineSkuAvailableItems;

            case "delete":
            case "move":
                {
                    CMSGridActionButton button = sender as CMSGridActionButton;
                    if (button != null)
                    {
                        // Hide actions when not allowed
                        button.Visible = allowActions;
                    }
                }
                break;
        }

        return parameter;
    }


    private void HeaderActions_ActionPerformed(object sender, CommandEventArgs e)
    {
        switch (e.CommandName.ToLowerCSafe())
        {
            case "lnksort_click":
                if (allowActions)
                {
                    // Check permissions
                    CheckModifyPermission();

                    SKUInfoProvider.SortProductOptionsAlphabetically(categoryId);
                    ugOptions.ReloadData();
                }
                break;
        }
    }


    private void grid_OnAction(string actionName, object actionArgument)
    {
        int skuId = ValidationHelper.GetInteger(actionArgument, 0);

        switch (actionName.ToLowerCSafe())
        {
            case "edit":
                // Show product tabs for type Products, otherwise show only general tab
                {
                    var url = UIContextHelper.GetElementUrl(ModuleName.ECOMMERCE, "ProductOptions.Options.Edit");
                    url = URLHelper.AddParameterToUrl(url, "displaytitle", "false");
                    url = URLHelper.AddParameterToUrl(url, "productId", skuId.ToString());
                    url = URLHelper.AddParameterToUrl(url, "categoryid", categoryId.ToString());
                    url = URLHelper.AddParameterToUrl(url, "siteId", categoryObj.CategorySiteID.ToString());
                    url = URLHelper.AddParameterToUrl(url, "objectid", skuId.ToString());
                    // To be able to hide tax class tab for attribute and text option
                    url = URLHelper.AddParameterToUrl(url, "parentobjectid", categoryId.ToString());

                    // Add parent product id 
                    if (parentProductId > 0)
                    {
                        url += "&parentProductId=" + parentProductId;
                    }

                    if (QueryHelper.GetBoolean("isindialog", false))
                    {
                        url = URLHelper.AddParameterToUrl(url, "isindialog", "1");
                        url = ApplicationUrlHelper.AppendDialogHash(url);
                    }

                    URLHelper.Redirect(url);
                }
                break;

            case "delete":
                // Check permissions
                CheckModifyPermission();

                // Check dependencies
                if (SKUInfoProvider.CheckDependencies(skuId))
                {
                    // Show error message
                    ShowError(EcommerceUIHelper.GetDependencyMessage(SKUInfoProvider.GetSKUInfo(skuId)));

                    return;
                }

                // Check if same variant is defined by this option
                DataSet variants = VariantOptionInfoProvider.GetVariantOptions()
                                       .TopN(1)
                                       .Columns("VariantSKUID")
                                       .WhereEquals("OptionSKUID", skuId);

                if (!DataHelper.DataSourceIsEmpty(variants))
                {
                    // Option is used in some variant
                    ShowError(GetString("com.option.usedinvariant"));

                    return;
                }

                SKUInfoProvider.DeleteSKUInfo(skuId);
                ugOptions.ReloadData();

                break;
        }
    }

    #endregion


    #region "Security check methods"

    /// <summary>
    /// Checks if user has permission to modify/delete product.
    /// </summary>
    private void CheckModifyPermission()
    {
        // Check permissions
        bool global = (editedSiteId <= 0);
        if (!ECommerceContext.IsUserAuthorizedToModifyOptionCategory(global))
        {
            if (global)
            {
                RedirectToAccessDenied(ModuleName.ECOMMERCE, EcommercePermissions.ECOMMERCE_MODIFYGLOBAL);
            }
            else
            {
                RedirectToAccessDenied(ModuleName.ECOMMERCE, "EcommerceModify OR ModifyProducts");
            }
        }
    }

    #endregion
}