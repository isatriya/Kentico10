using System;
using System.Collections.Generic;

using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.ResponsiveImages;

using Samples.ResponsiveImages;

[assembly: RegisterImageVariantDefinition(typeof(MediumSizeImageVariantDefinition))]

namespace Samples.ResponsiveImages
{
    /// <summary>
    /// Sample image variant definition for medium-size device.
    /// </summary>
    internal class MediumSizeImageVariantDefinition : ImageVariantDefinition
    {
        private const string IDENTIFIER = "medium";


        /// <summary>
        /// Definition identifier.
        /// </summary>
        public override string Identifier
        {
            get
            {
                return IDENTIFIER;
            }
        }


        /// <summary>
        /// Returns context scopes to restrict variant application.
        /// </summary>
        public override IEnumerable<IVariantContextScope> ContextScopes
        {
            get
            {
                return new[] {
                    new AttachmentVariantContextScope()
                        .OnSite("DancingGoat")
                        .Type("DancingGoat.Article")
                        .Path("/Articles")
                };
            }
        }


        /// <summary>
        /// Collection of filters used for variant generation.
        /// </summary>
        public override IEnumerable<IImageFilter> Filters
        {
            get
            {
                return new IImageFilter[]
                {
                    new ResizeImageFilter(ImageHelper.AUTOSIZE, 200),
                    new CropImageFilter(269, ImageHelper.AUTOSIZE, ImageHelper.ImageTrimAreaEnum.MiddleRight)
                };
            }
        }
    }
}