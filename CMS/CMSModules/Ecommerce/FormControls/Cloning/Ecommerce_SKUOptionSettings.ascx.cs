﻿using CMS.Ecommerce;
using CMS.UIControls;


public partial class CMSModules_Ecommerce_FormControls_Cloning_Ecommerce_SKUOptionSettings : CloneSettingsControl
{
    #region "Properties"

    /// <summary>
    /// Hide the control
    /// </summary>
    public override bool DisplayControl
    {
        get
        {
            return false;
        }
    }


    /// <summary>
    /// Excluded other binding types.
    /// </summary>
    public override string ExcludedOtherBindingTypes
    {
        get
        {
            return SKUInfo.OBJECT_TYPE_OPTIONSKU + ";" + VariantOptionInfo.OBJECT_TYPE + ";" + SKUAllowedOptionInfo.OBJECT_TYPE;
        }
    }

    #endregion
}