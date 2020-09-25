using System.Drawing;

using CMS.Core;
using CMS.ResponsiveImages;
using CMS.Helpers;

namespace Samples.ResponsiveImages
{
    /// <summary>
    /// Sample filter for resizing image to a specific size.
    /// </summary>
    internal class ResizeImageFilter : IImageFilter
    {
        private int Width
        {
            get;
            set;
        }


        private int Height
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor, creates a new instance of <see cref="ResizeImageFilter" />.
        /// </summary>
        /// <param name="width">Resize width in pixels. Use <see cref="ImageHelper.AUTOSIZE"/> to compute the width automatically with aspect ratio.</param>
        /// <param name="height">Resize height in pixels. Use <see cref="ImageHelper.AUTOSIZE"/> to compute the height automatically with aspect ratio.</param>
        public ResizeImageFilter(int width, int height)
        {
            Width = width;
            Height = height;
        }


        /// <summary>
        /// Applies the filter on the image data.
        /// </summary>
        /// <param name="container">Input image container.</param>
        /// <returns>
        /// New instance of <see cref="ImageContainer"/> with the applied filter or <c>null</c> when the filter was not applied.
        /// </returns>
        public ImageContainer ApplyFilter(ImageContainer container)
        {
            using (var stream = container.OpenReadStream())
            {
                var metadata = container.Metadata;
                var imageHelper = new ImageHelper(BinaryData.GetByteArrayFromStream(stream), metadata.Width, metadata.Height);

                // Resize image
                var resizedImage = GetResizedImage(imageHelper);
                var resizedImageData = imageHelper.ImageToBytes(resizedImage);
                var resizedMetadata = new ImageMetadata(resizedImage.Width, resizedImage.Height, metadata.MimeType, metadata.Extension);

                return new ImageContainer(resizedImageData, resizedMetadata);
            }
        }


        private Image GetResizedImage(ImageHelper imageHelper)
        {
            var width = LimitMaximumSize(Width, imageHelper.ImageWidth);
            var height = LimitMaximumSize(Height, imageHelper.ImageHeight);

            return imageHelper.GetResizedImage(width, height);
        }


        private int LimitMaximumSize(int size, int currentSize)
        {
            return size > currentSize ? currentSize : size;
        }
    }
}