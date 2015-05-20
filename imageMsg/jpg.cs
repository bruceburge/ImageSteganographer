using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing;

//used with little modification from
//http://www.eggheadcafe.com/print.asp
//By Peter A. Bromberg, Ph.D.

namespace imageMsg
{
    public class Jpg
    {
        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            } return null;
        }

        public void WriteNewDescriptionInImage(string Filename, string NewDescription)
        {
            Image Pic = null;
            PropertyItem[] PropertyItems;
            byte[] bDescription = new Byte[NewDescription.Length];
            int i;
            string FilenameTemp = null;
            System.Drawing.Imaging.Encoder Enc = System.Drawing.Imaging.Encoder.Transformation;
            EncoderParameters EncParms = new EncoderParameters(1);
            EncoderParameter EncParm;
            ImageCodecInfo CodecInfo = GetEncoderInfo("image/jpeg");

            // copy description into byte array
            for (i = 0; i < NewDescription.Length; i++) bDescription[i] = (byte)NewDescription[i];

            try
            {
                // load the image to change
                Pic = Image.FromFile(Filename);

                // put the new description into the right property item
                PropertyItems = Pic.PropertyItems;
                PropertyItems[0].Id = 0xF00F; // 0xF00F not used as far as I know 
                PropertyItems[0].Type = 2;
                PropertyItems[0].Len = NewDescription.Length;
                PropertyItems[0].Value = bDescription;
                Pic.SetPropertyItem(PropertyItems[0]);
                // we cannot store in the same image, so use a temporary image instead
                FilenameTemp = Filename + ".temp";

                // for lossless rewriting must rotate the image by 90 degrees!
                EncParm = new EncoderParameter(Enc, (long)EncoderValue.TransformRotate90);
                EncParms.Param[0] = EncParm;

                // now write the rotated image with new description
                Pic.Save(FilenameTemp, CodecInfo, EncParms);


            }
            catch (ArgumentException err)
            {
                MessageBox.Show("An error has occured"+Environment.NewLine+"Application only supports JPG file format, detailed error message below"+Environment.NewLine+err.Message);
                return;
            }
            finally
            {
                // for computers with low memory and large pictures: release memory now
                Pic.Dispose();
                Pic = null;
                GC.Collect();
            }

            // now must rotate back the written picture
            Pic = Image.FromFile(FilenameTemp);
            EncParm = new EncoderParameter(Enc, (long)EncoderValue.TransformRotate270);
            EncParms.Param[0] = EncParm;

            //should probably rewrite this to a save/remove/rename method incase of error saving
            System.IO.File.Delete(Filename);
            Pic.Save(Filename, CodecInfo, EncParms);

            // release memory now
            Pic.Dispose();
            Pic = null;
            GC.Collect();

            // delete the temporary picture
            System.IO.File.Delete(FilenameTemp);
        }
    }
}