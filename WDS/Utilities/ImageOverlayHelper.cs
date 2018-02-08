using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;


namespace WDS.Utilities
{
    public class ImageOverlayHelper
    {
        private int intWidth = 0;
        private int intHeight = 0;

        /// <summary>
        /// 另存尺寸圖片
        /// </summary>
        /// <param name="sizeRecommend">新尺寸</param>
        public ImageOverlayHelper(Size sizeRecommend)
        {
            intWidth = sizeRecommend.Width;
            intHeight = sizeRecommend.Height;
        }


        /// <summary>
        /// 將圖片補黑邊至期望的尺寸
        /// </summary>
        /// <param name="path">檔案路徑</param>
        /// <returns></returns>
        public bool CutImage(string toPath, Image imageSource)
        {
            Size sizePicture;
            sizePicture = ImageResizeHelper.GetThumbSize(imageSource.Width, imageSource.Height, intWidth, intHeight);
            try
            {
                //string[] temp = path.Split('.');
                //string thumbPath = path.Replace("." + temp[temp.Length - 1], "_s." + temp[temp.Length - 1]);
                //SaveAsJPG(ImageResizeHelper.ResizeImage(Image.FromFile(path), sizePicture.Width, sizePicture.Height), thumbPath);
                SaveAsJPG(ImageResizeHelper.ResizeImage(imageSource, sizePicture.Width, sizePicture.Height), toPath);
                return true;
            }
            catch
            {
                return false;
            }
        }


        ///// <summary>
        ///// 將圖片補黑邊至期望的尺寸
        ///// </summary>
        ///// <param name="path">檔案路徑</param>
        ///// <returns></returns>
        //public bool CutImage(string fromPath)
        //{
        //    if (!File.Exists(fromPath)) return false;

        //    Image image = Image.FromFile(fromPath);
        //    //固定鎖定高度, 所以建議寬度設大一點, 等於不設限

        //    Size sizePicture;
        //    sizePicture = ImageResizeHelper.GetThumbSize(image.Width, image.Height, intWidth, intHeight);
        //    try
        //    {
        //        //string[] temp = path.Split('.');
        //        //string thumbPath = path.Replace("." + temp[temp.Length - 1], "_s." + temp[temp.Length - 1]);
        //        //SaveAsJPG(ImageResizeHelper.ResizeImage(Image.FromFile(path), sizePicture.Width, sizePicture.Height), thumbPath);
        //        SaveAsJPG(ImageResizeHelper.ResizeImage(Image.FromFile(fromPath), sizePicture.Width, sizePicture.Height), path);
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}


        private void SaveAsJPG(Image imageSource, string stringSaveTo)
        {
            int intPositionX = 0;
            int intPositionY = 0;
            int intSourceHeight = imageSource.Height;
            int intSourceWidth = imageSource.Width;

            Image imageResult;
            if (intWidth - intSourceWidth > 0 && intHeight - intSourceHeight > 0)
            {
                //上下左右都補黑邊
                intPositionX = (intWidth - intSourceWidth) / 2;
                intPositionY = (intHeight - intSourceHeight) / 2;
                imageResult = ResizeImage(imageSource, intPositionX, intPositionY);
            }
            else if (intWidth - intSourceWidth > intHeight - intSourceHeight)
            {
                //補左右黑邊

                intPositionX = (intWidth - intSourceWidth) / 2;
                intPositionY = 0;
                //imageResult = PictureOverlay.Save(imageBackground, imageSource, intPositionX, intPositionY, 1);
                imageResult = ResizeImage(imageSource, intPositionX, intPositionY);
            }
            else if (intWidth - intSourceWidth < intHeight - intSourceHeight)
            {
                //補上下黑邊

                intPositionX = 0;
                intPositionY = (intHeight - intSourceHeight) / 2;
                //imageResult = PictureOverlay.Save(imageBackground, imageSource, intPositionX, intPositionY, 1);
                imageResult = ResizeImage(imageSource, intPositionX, intPositionY);
            }
            else
            {
                //不用補邊
                imageResult = imageSource;
            }

            //圖片縮圖不壓縮
            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
            imageResult.Save(stringSaveTo);

            //圖片縮圖的壓縮
            //long longQuality = 75L;
            //EncoderParameters myEncoderParameters = new EncoderParameters(1);
            //System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            //EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, longQuality);
            //myEncoderParameters.Param[0] = myEncoderParameter;
            //imageResult.Save(stringSaveTo, jgpEncoder, myEncoderParameters);
        }

        public System.Drawing.Image ResizeImage(System.Drawing.Image imgPhoto, int intPositionX, int intPositionY)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;


            Bitmap bmPhoto = new Bitmap(intWidth, intHeight, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                                    imgPhoto.VerticalResolution);

            using (Graphics grPhoto = Graphics.FromImage(bmPhoto))
            {
                //grPhoto.Clear(Color.Black);
                //grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //grPhoto.SmoothingMode = SmoothingMode.HighQuality;

                grPhoto.DrawImage(imgPhoto,
                    new Rectangle(intPositionX, intPositionY, sourceWidth, sourceHeight),
                    new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                    GraphicsUnit.Pixel);
            }
            return bmPhoto;
        }

        //private void SaveAsJPG(int intType, Image imageSource, string stringSaveTo)
        //{
        //    Image imageBackground;
        //    long longQuality = 75L;
        //    int intPositionX = 0;
        //    int intPositionY = 0;
        //    int intRecommendHeight;
        //    int intRecommendWidth;
        //    int intSourceHeight = imageSource.Height;
        //    int intSourceWidth = imageSource.Width;

        //    Image imageResult;
        //    if (intType == 0)
        //    {
        //        intRecommendHeight = 200;
        //        intRecommendWidth = 140;
        //        imageBackground = imageBackground0;
        //    }
        //    else
        //    {
        //        intRecommendHeight = 100;
        //        intRecommendWidth = 150;
        //        imageBackground = imageBackground1;
        //    }

        //    if (intRecommendWidth - intSourceWidth > intRecommendHeight - intSourceHeight)
        //    {
        //        //補左右黑邊

        //        intPositionX = (intWidth - intSourceWidth) / 2;
        //        intPositionY = 0;
        //        imageResult = PictureOverlay.Save(imageBackground, imageSource, intPositionX, intPositionY, 1);
        //    }
        //    else if (intWidth - intSourceWidth < intHeight - intSourceHeight)
        //    {
        //        //補上下黑邊

        //        intPositionX = 0;
        //        intPositionY = (intHeight - intSourceHeight) / 2;
        //        imageResult = PictureOverlay.Save(imageBackground, imageSource, intPositionX, intPositionY, 1);
        //    }
        //    else
        //    {
        //        //不用補邊
        //        imageResult = imageSource;
        //    }
            
        //    ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
        //    EncoderParameters myEncoderParameters = new EncoderParameters(1);
        //    System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
        //    EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, longQuality);
        //    myEncoderParameters.Param[0] = myEncoderParameter;
        //    imageResult.Save(stringSaveTo, jgpEncoder, myEncoderParameters);
        //}

        private ImageCodecInfo GetEncoder(ImageFormat format)
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
