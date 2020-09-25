using System;
using CMS;
using CMS.Base;

#pragma warning disable 0618

[assembly: RegisterModule(typeof(CMSModuleLoader))]

#pragma warning restore 0618


/// <summary>
/// Module loader class, ensures initialization of other modules through this partial class
/// </summary>
[Obsolete("Use RegisterModule attribute for custom code module.")]
public partial class CMSModuleLoader : CMSModuleLoaderBase
{
    /// <summary>
    /// Constructor
    /// </summary>
    public CMSModuleLoader()
        : base("CMSModuleLoader")
    {
    }
}