using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Windows.Forms;

//Based on code project article 
//http://www.codeproject.com/KB/security/encryption_decryption.aspx
//authored by fangfrank@hotmail.com



namespace imageMsg
{
    public class ImageMsgCrypto
    {

        private SymmetricAlgorithm CryptoService;


        public ImageMsgCrypto()
        {
            CryptoService = new RijndaelManaged();
            CryptoService.BlockSize = 256;
            CryptoService.Padding = PaddingMode.ISO10126;            
        }

        private byte[] PrepKeyIV(string Key)
        {          
            int blockSize = CryptoService.BlockSize;           
            
            // key sizes are in bits
            if (Key.Length * 8 > blockSize)
            {
                //truncate key to block size since password and IV are the same
                return ASCIIEncoding.ASCII.GetBytes(Key.Remove((blockSize / 8)));
            }
            else if (Key.Length * 8 < blockSize)
            {
                //pad key to block size since password and IV are the same
                return ASCIIEncoding.ASCII.GetBytes(Key.PadRight(blockSize / 8, ' '));
            }
            return null;

        }

        public string Encrypting(string Source, string Key)
        {
            byte[] bytIn = ASCIIEncoding.ASCII.GetBytes(Source);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            byte[] bytKey = PrepKeyIV(Key);

            //IV and BlockSize must be the same size
            CryptoService.Key = bytKey;
            CryptoService.IV = bytKey;
            
            ICryptoTransform encrypto = CryptoService.CreateEncryptor();          
            CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);

            // write out encrypted content into MemoryStream
            cs.Write(bytIn, 0, bytIn.Length);
            cs.FlushFinalBlock();

            byte[] bytOut = ms.ToArray();
            return System.Convert.ToBase64String(bytOut);
        }

        public string Decrypting(string Source, string Key)
        {
            byte[] bytIn = null;
            try
            {
                // convert from Base64 to binary            
                bytIn = System.Convert.FromBase64String(Source);                
                
            }
            catch (FormatException)
            {
                MessageBox.Show("Unable to decrypt data incorrect format","Error",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                return "";
            }
                byte[] bytKey = PrepKeyIV(Key);
                // set the private key
                CryptoService.Key = bytKey;
                CryptoService.IV = bytKey;

                // create a MemoryStream with the input
                System.IO.MemoryStream ms = new System.IO.MemoryStream(bytIn, 0, bytIn.Length);

                // create a Decryptor from the Provider Service instance
                ICryptoTransform encrypto = CryptoService.CreateDecryptor();

                // create Crypto Stream that transforms a stream using the decryption
                CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Read);
            try
            {
                // read out the result from the Crypto Stream
                System.IO.StreamReader sr = new System.IO.StreamReader(cs);
                return sr.ReadToEnd();
            }
            catch (Exception err)
            {
                MessageBox.Show("Either image contains no encrypted text, or key is wrong for this cypher " + Environment.NewLine + "Error message details: " + err.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return "";
            }

        }
    }
}