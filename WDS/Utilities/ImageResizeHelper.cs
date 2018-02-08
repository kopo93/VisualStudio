using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace WDS.Utilities
{
    public class ImageResizeHelper
    {
        /// <summary>
        /// 儲存不超過設定的寬和高的圖檔
        /// </summary>
        /// <param name="img">圖檔</param>
        /// <param name="width">最大寬度</param>
        /// <param name="height">最大高度</param>
        /// <param name="filename">儲存的檔名</param>
        /// <returns>回傳是否儲存成功</returns>
        public static bool Save(System.Drawing.Image img, int? width, int? height, string filename)
        {
            return Save(img, width, height, filename, 100L);
            //using (System.Drawing.Image thumb = GetThumbnail(width, height, img))
            //{
            //    if (thumb != null)
            //    {
            //        thumb.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
            //        return true;
            //    }
            //    else
            //    {
            //        img.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
            //    }
            //}
            //return false;
        }
        /// <summary>
        /// 儲存不超過設定的寬和高的圖檔
        /// </summary>
        /// <param name="img">圖檔</param>
        /// <param name="width">最大寬度</param>
        /// <param name="height">最大高度</param>
        /// <param name="filename">儲存的檔名</param>
        /// <param name="quality">儲存的品質</param>
        /// <returns>回傳是否儲存成功</returns>
        public static bool Save(System.Drawing.Image img, int? width, int? height, string filename, long quality)
        {
            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            Encoder encoderQuality = Encoder.Quality;
            EncoderParameter encoderParameter = new EncoderParameter(encoderQuality, quality);
            EncoderParameters encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = encoderParameter;

            using (System.Drawing.Image thumb = GetThumbnail(width, height, img))
            {
                if (thumb != null)
                {
                    thumb.Save(filename, jpgEncoder, encoderParameters);
                    return true;
                }
                else
                {
                    img.Save(filename, jpgEncoder, encoderParameters);
                }
            }
            return false;
        }
        /// <summary>
        /// 取得 Size Object 後, 再依物件取得 Resize 圖檔
        /// </summary>
        /// <param name="recommendWidth">最大寬度</param>
        /// <param name="recommendHeight">最大高度</param>
        /// <param name="img">圖檔</param>
        /// <returns>回傳 Resize 的圖檔</returns>
        private static System.Drawing.Image GetThumbnail(int? recommendWidth, int? recommendHeight, System.Drawing.Image img)
        {

            System.Drawing.Size thumbSize = GetThumbSize(img.Width, img.Height, recommendWidth, recommendHeight);
            if (thumbSize != Size.Empty)
            {
                try { return ResizeImage(img, thumbSize.Width, thumbSize.Height); }
                catch (Exception) { }

            }
            return null;
        }
        /// <summary>
        /// 依據原始長寬和建議長寬, 計算 New Size
        /// </summary>
        /// <param name="width">原始寬度</param>
        /// <param name="height">原始高度</param>
        /// <param name="recommandWidth">建議寬度</param>
        /// <param name="recommandHeight">建議高度</param>
        /// <returns>回傳新的 Size 物件</returns>
        public static System.Drawing.Size GetThumbSize(int width, int height, int? recommandWidth, int? recommandHeight)
        {
            System.Drawing.Size size = Size.Empty;
            if (((!recommandWidth.HasValue) && (!recommandHeight.HasValue))
                || (width <= recommandWidth && height <= recommandHeight))
            {
                size.Width = width;
                size.Height = height;
            }
            else
            {
                if (!recommandWidth.HasValue) recommandWidth = System.Int32.MaxValue;
                if (!recommandHeight.HasValue) recommandHeight = System.Int32.MaxValue;
                //取得長的縮小比率
                double ratioHeight = (double)recommandHeight / (double)height;
                //取得寬的縮小比率
                double ratioWidth = (double)recommandWidth / (double)width;
                //取長或寬比率比較小的, 目的是讓縮小的圖檔不會超過期望的長或寬

                //double ratio = ratioHeight > ratioWidth ? ratioWidth : ratioHeight;
                //size.Width = (int)((double)width * ratio);
                //size.Height = (int)((double)height * ratio);
                if (ratioHeight > ratioWidth)
                {
                    size.Width = recommandWidth.Value;
                    size.Height = (int)((double)height * ratioWidth);
                }
                else
                {
                    size.Width = (int)((double)width * ratioHeight);
                    size.Height = recommandHeight.Value;
                }
            }
            return size;
        }

        
        /// <summary>
        /// 將圖檔存成不超過設定的寬和高的同比例圖 (4)-Do Resize
        /// </summary>
        /// <param name="imgPhoto"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static System.Drawing.Image ResizeImage(System.Drawing.Image imgPhoto, int width, int height)
        {
            System.Drawing.Size sizeNew = Size.Empty;
            sizeNew.Width = width;
            sizeNew.Height = height;
            return ResizeImage(imgPhoto, sizeNew);
        }

        /// <summary>
        /// 將圖檔存成不超過設定的寬和高的同比例圖 (4)-Do Resize
        /// </summary>
        /// <param name="imgPhoto"></param>
        /// <param name="sizeNew"></param>
        /// <returns></returns>
        public static System.Drawing.Image ResizeImage(System.Drawing.Image imgPhoto, System.Drawing.Size sizeNew)
        {
            return ResizeImage(imgPhoto, 0, 0, sizeNew);
        }
        /// <summary>
        /// 將圖檔存成不超過設定的寬和高的同比例圖 (4)-Do Resize
        /// </summary>
        /// <param name="imgPhoto"></param>
        /// <param name="positionX"></param>
        /// <param name="positionY"></param>
        /// <param name="sizeNew"></param>
        /// <returns></returns>
        public static System.Drawing.Image ResizeImage(System.Drawing.Image imgPhoto, int positionX, int positionY, System.Drawing.Size sizeNew)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;

            int destX = positionX;
            int destY = positionY;
            int destWidth = sizeNew == Size.Empty ? imgPhoto.Width : sizeNew.Width;
            int destHeight = sizeNew == Size.Empty ? imgPhoto.Height : sizeNew.Height;

            Bitmap bmPhoto = new Bitmap(destWidth, destHeight, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                                    imgPhoto.VerticalResolution);

            using (Graphics grPhoto = Graphics.FromImage(bmPhoto))
            {
                grPhoto.Clear(Color.Black);
                grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
                grPhoto.SmoothingMode = SmoothingMode.HighQuality;
                grPhoto.DrawImage(imgPhoto,
                    new Rectangle(destX, destY, destWidth, destHeight),
                    new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                    GraphicsUnit.Pixel);
            }
            return bmPhoto;
        }

        //public static void SaveAsFixedSize(Image source, string stringSaveTo, int width, int height)
        //{
        //    long quality = 75L;
        //    int positionX = 0;
        //    int positionY = 0;
        //    Size sizeNew = Size.Empty;
        //    sizeNew.Height = height;
        //    sizeNew.Width = width;
        //    int sourceHeight = source.Height;
        //    int sourceWidth = source.Width;

        //    Image imageResult;
        //    if (sizeNew.Width > sourceWidth && sizeNew.Height > sourceHeight)
        //    {
        //        positionX = (sizeNew.Width - sourceWidth) / 2;
        //        positionY = (sizeNew.Height - sourceHeight) / 2;
        //        imageResult = ResizeImage(source, positionX, positionY, sizeNew);
        //    }
        //    else if (sizeNew.Width - sourceWidth > sizeNew.Height - sourceHeight)
        //    {
        //        //補左右黑邊
        //        positionX = (sizeNew.Width - sourceWidth) / 2;
        //        positionY = 0;
        //        imageResult = ResizeImage(source, positionX, positionY, sizeNew);
        //    }
        //    else if (sizeNew.Width - sourceWidth < sizeNew.Height)
        //    {
        //        //補上下黑邊
        //        positionX = 0;
        //        positionY = (sizeNew.Height - sourceHeight) / 2;
        //        imageResult = ResizeImage(source, positionX, positionY, sizeNew);
        //    }
        //    else
        //    {
        //        //不用補邊
        //        imageResult = source;
        //    }

        //    ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
        //    EncoderParameters myEncoderParameters = new EncoderParameters(1);
        //    System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
        //    EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, quality);
        //    myEncoderParameters.Param[0] = myEncoderParameter;
        //    imageResult.Save(stringSaveTo, jgpEncoder, myEncoderParameters);
        //}

        /// <summary>
        /// 取得 JPEG 的 Codec
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }

}