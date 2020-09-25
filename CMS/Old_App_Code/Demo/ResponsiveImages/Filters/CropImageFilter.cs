using System.Drawing;

using CMS.Core;
using CMS.ResponsiveImages;
using CMS.Helpers;

namespace Samples.ResponsiveImages
{
    /// <summary>
    /// Sample filter for cropping image by specific parameters.
    /// </summary>
    internal class CropImageFilter : IImageFilter
    {
        private int CropWidth
        {
            get;
            set;
        }


        private int CropHeight
        {
            get;
            set;
        }


        private ImageHelper.ImageTrimAreaEnum CropTo
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor, creates a new instance of <see cref="CropImageFilter" />.
        /// </summary>
        /// <param name="cropWidth">Width of the crop area in pixels. Use <see cref="ImageHelper.AUTOSIZE"/> to compute the width automatically with aspect ratio.</param>
        /// <param name="cropHeight">Height of the crop area in pixels. Use <see cref="ImageHelper.AUTOSIZE"/> to compute the height automatically with aspect ratio.</param>
        /// <param name="cropTo">Defines the position of area to crop.</param>
        public CropImageFilter(int cropWidth, int cropHeight, ImageHelper.ImageTrimAreaEnum cropTo = ImageHelper.ImageTrimAreaEnum.MiddleCenter)
        {
            CropWidth = cropWidth;
            CropHeight = cropHeight;
            CropTo = cropTo;
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

                // Crop image
                var croppedImage = GetCroppedImage(imageHelper);
                var croppedImageData = imageHelper.ImageToBytes(croppedImage);
                var croppedImageMetadata = new ImageMetadata(croppedImage.Width, croppedImage.Height, metadata.MimeType, metadata.Extension);

                return new ImageContainer(croppedImageData, croppedImageMetadata);
            }
        }


        private Image GetCroppedImage(ImageHelper imageHelper)
        {
            var cropWidth = LimitMaximumSize(CropWidth, imageHelper.ImageWidth);
            var cropHeight = LimitMaximumSize(CropHeight, imageHelper.ImageHeight);

            return imageHelper.GetTrimmedImage(cropWidth, cropHeight, CropTo);
        }


        private static int LimitMaximumSize(int size, int currentSize)
        {
            if (size == ImageHelper.AUTOSIZE)
            {
                return currentSize;
            }

            return size > currentSize ? currentSize : size;
        }
    }
}