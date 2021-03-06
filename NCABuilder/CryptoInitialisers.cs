﻿using System.IO;
using System.Security.Cryptography;
using XTSSharp;

namespace NCABuilder
{
    internal class CryptoInitialisers
    {
        public static byte[] GenerateRandomKey(int Length)
        {
            byte[] RandomKey = new byte[Length];
            var RNG = new RNGCryptoServiceProvider();
            RNG.GetBytes(RandomKey);
            return RandomKey;
        }

        public static byte[] GenSHA256Hash(byte[] Data)
        {
            var SHA = SHA256.Create();
            return SHA.ComputeHash(Data);
        }

        public static byte[] GenSHA256StrmHash(Stream Data)
        {
            var SHA = SHA256.Create();
            return SHA.ComputeHash(Data);
        }

        // Thanks, Falo!
        public static byte[] AES_XTS(byte[] Key1, byte[] Key2, int SectorSize, byte[] Data, ulong Sector)
        {
            byte[] BlockData;
            Xts XTS128 = XtsAes128.Create(Key1, Key2, true);
            int Blocks;
            var MemStrm = new MemoryStream();
            var Writer = new BinaryWriter(MemStrm);
            var CryptoTransform = XTS128.CreateEncryptor();
            BlockData = new byte[SectorSize];
            Blocks = Data.Length / SectorSize;
            for (int i = 0; i < Blocks; i++)
            {
                CryptoTransform.TransformBlock(Data, i * SectorSize, SectorSize, BlockData, 0, Sector++);
                Writer.Write(BlockData);
            }
            return MemStrm.ToArray();
        }

        public static byte[] AES_EBC(byte[] Key, byte[] Data)
        {
            var AES = new RijndaelManaged
            {
                Key = Key,
                Mode = CipherMode.ECB
            };
            byte[] TransformedData = new byte[0x40];
            AES.CreateEncryptor().TransformBlock(Data, 0, 0x40, TransformedData, 0);
            return TransformedData;
        }

        public static byte[] AES_CTR(byte[] Key, byte[] CTR, byte[] Data)
        {
            byte[] Buffer = new byte[Data.Length];
            var AESCTR = new AES128CTR(CTR);
            ICryptoTransform Transform;
            Transform = AESCTR.CreateEncryptor(Key, null);
            Transform.TransformBlock(Data, 0, Data.Length, Buffer, 0);
            return Buffer;
        }

        public static byte[] Encrypt_RSA_2048_OAEP_MGF1_SHA256(byte[] Input, RSAParameters Params)
        {
            var RSA = new RSACng();
            RSA.ImportParameters(Params);
            var SHA256Padding = RSAEncryptionPadding.CreateOaep(HashAlgorithmName.SHA256);
            return RSA.Encrypt(Input, SHA256Padding);
        }
    }
}