﻿using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BeePassKeeper.Security
{
    public class Crypter : IDisposable
    {
        private string password;
        private const string salt = "AESENOPH8_B";

        public Crypter(string password)
        {
            this.password = password;
        }

        public string Encrypt(string dataToEncrypt)
        {
            if (String.IsNullOrWhiteSpace(dataToEncrypt))
            {
                return dataToEncrypt;
            }
            AesManaged aes = null;
            MemoryStream memoryStream = null;
            CryptoStream cryptoStream = null;

            try
            {
                //Generate a Key based on a Password and HMACSHA1 pseudo-random number generator
                //Salt must be at least 8 bytes long
                //Use an iteration count of at least 1000
                Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 50);

                //Create AES algorithm
                aes = new AesManaged();
                //Key derived from byte array with 32 pseudo-random key bytes
                aes.Key = rfc2898.GetBytes(32);
                //IV derived from byte array with 16 pseudo-random key bytes
                aes.IV = rfc2898.GetBytes(16);

                //Create Memory and Crypto Streams
                memoryStream = new MemoryStream();
                cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

                //Encrypt Data
                byte[] data = Encoding.UTF8.GetBytes(dataToEncrypt);
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();

                //Return Base 64 String
                return Convert.ToBase64String(memoryStream.ToArray());
            }
            finally
            {
                if (cryptoStream != null)
                    cryptoStream.Close();

                if (memoryStream != null)
                    memoryStream.Close();

                if (aes != null)
                    aes.Clear();
            }
        }

        public string Decrypt(string dataToDecrypt)
        {
            if (String.IsNullOrWhiteSpace(dataToDecrypt))
            {
                return dataToDecrypt;
            }

            AesManaged aes = null;
            MemoryStream memoryStream = null;

            try
            {
                //Generate a Key based on a Password and HMACSHA1 pseudo-random number generator
                //Salt must be at least 8 bytes long
                //Use an iteration count of at least 1000
                Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 50);

                //Create AES algorithm
                aes = new AesManaged();
                //aes.Padding = PaddingMode.None;
                //Key derived from byte array with 32 pseudo-random key bytes
                aes.Key = rfc2898.GetBytes(32);
                //IV derived from byte array with 16 pseudo-random key bytes
                aes.IV = rfc2898.GetBytes(16);

                //Create Memory and Crypto Streams
                memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write);

                //Decrypt Data
                byte[] data = Convert.FromBase64String(dataToDecrypt);
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();

                //Return Decrypted String
                byte[] decryptBytes = memoryStream.ToArray();

                //Dispose
                if (cryptoStream != null)
                    cryptoStream.Dispose();

                //Retval
                return Encoding.UTF8.GetString(decryptBytes, 0, decryptBytes.Length);
            }
            finally
            {
                if (memoryStream != null)
                    memoryStream.Dispose();

                if (aes != null)
                    aes.Clear();
            }
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }

}
