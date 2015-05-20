using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace imageMsg
{
    public partial class Form1 : Form
    {
        string sEncrypted;

        public Form1()
        {
            InitializeComponent();
            
        }

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            ImageMsgCrypto tmp = new ImageMsgCrypto();
            //encrypt the message using, key.text will be padded with ' ' to conform to 
            //256 bit length key. 
            //see crypto.cs
            string encryptedMsg = tmp.Encrypting(txtMessage.Text, txtKey.Text);
            rtbResult.Text = encryptedMsg;
 
            if (openFileDialog1.FileName != "")
            {
                Jpg tmpimage = new Jpg();
                //this function will add the encrypted message to the the image
                //uses a rotate function to effectively to do a lossless recompression.
                //see jpg.cs
                tmpimage.WriteNewDescriptionInImage(openFileDialog1.FileName, encryptedMsg);
            }
            if (pictureBox1.ImageLocation != "")
            {
                //force reload of image
                pictureBox1.ImageLocation = openFileDialog1.FileName;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            string encryptedString = rtbResult.Text;
            
            if (openFileDialog1.FileName != "" && sEncrypted != "")
            {
                //if we have loaded an image and the image has a encrypted string
                //use that string, other wise, use result of manual string encryption
                encryptedString = sEncrypted;
            }
            
            ImageMsgCrypto tmp = new ImageMsgCrypto();
            string decryptedMsg = tmp.Decrypting(encryptedString, txtKey.Text);
            rtbResult.Text = decryptedMsg;
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            
            openFileDialog1.InitialDirectory = Environment.SpecialFolder.Desktop.ToString();
            openFileDialog1.Filter = "jpg files (*.jpg)|*.jpg";            
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = false;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {                
                try
                {                     
                    pictureBox1.ImageLocation = openFileDialog1.FileName;                                          
                }
                catch (Exception err)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + err.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void pictureBox1_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            sEncrypted = "";
            //use file name as default key for crypto
            txtKey.Text = openFileDialog1.SafeFileName;
            lblStatus.Text = "";
            
            foreach (PropertyItem propItem in pictureBox1.Image.PropertyItems)
            {
                try
                {
                    //property ID that we store our encrypted data at
                    //doesn't seem to be in use by other applications.
                    if (propItem.Id == 0xF00F)
                    {
                        //convert byte[] to ascii string then chop off the null terminator
                        sEncrypted = ASCIIEncoding.ASCII.GetString(propItem.Value).TrimEnd('\0');
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                    return;
                }
            }
            if (sEncrypted.Length > 0)
            {
                lblStatus.Text = "Cypher from this program possibly detected";
                btnDecrypt.Enabled = true;
            }
            else
            {
                lblStatus.Text = "Cypher from this program not detected";
                btnDecrypt.Enabled = false;
            }
        }

        private void txtKey_TextChanged(object sender, EventArgs e)
        {
            if (txtKey.Text.Length > 0)
            {
                btnDecrypt.Enabled = true;
                btnEncrypt.Enabled = true;
            }
            else
            {
                btnDecrypt.Enabled = false;
                btnEncrypt.Enabled = false;
            }
        }

        private void rtbResult_TextChanged(object sender, EventArgs e)
        {
            if (rtbResult.Text.Length > 0)
            {
                btnDecrypt.Enabled = true;
            }
        }
    }
}