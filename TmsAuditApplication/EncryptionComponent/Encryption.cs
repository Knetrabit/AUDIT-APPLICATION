using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;


namespace CommonComponent;

    public class Encryption
    {
        public class Symmetric
        {
            public enum Provider
            {
                DES,
                RC2,
                Rijndael,
                TripleDES
            }

            private const string _DefaultIntializationVector = "%1Az=-@qT";

            private const int _BufferSize = 2048;

            private Data _data;

            private Data _key;

            private Data _iv;

            private SymmetricAlgorithm _crypto;

            private byte[] _EncryptedBytes;

            private bool _UseDefaultInitializationVector;

            public int KeySizeBytes
            {
                get
                {
                    return _crypto.KeySize / 8;
                }
                set
                {
                    _crypto.KeySize = checked(value * 8);
                    _key.MaxBytes = value;
                }
            }

            public int KeySizeBits
            {
                get
                {
                    return _crypto.KeySize;
                }
                set
                {
                    _crypto.KeySize = value;
                    _key.MaxBits = value;
                }
            }

            public Data Key
            {
                get
                {
                    return _key;
                }
                set
                {
                    _key = value;
                    _key.MaxBytes = _crypto.LegalKeySizes[0].MaxSize / 8;
                    _key.MinBytes = _crypto.LegalKeySizes[0].MinSize / 8;
                    _key.StepBytes = _crypto.LegalKeySizes[0].SkipSize / 8;
                }
            }

            public Data IntializationVector
            {
                get
                {
                    return _iv;
                }
                set
                {
                    _iv = value;
                    _iv.MaxBytes = _crypto.BlockSize / 8;
                    _iv.MinBytes = _crypto.BlockSize / 8;
                }
            }

            private Symmetric()
            {
            }

            public Symmetric(Provider provider, bool useDefaultInitializationVector = true)
            {
                switch (provider)
                {
                    case Provider.DES:
                        _crypto = new DESCryptoServiceProvider();
                        break;
                    case Provider.RC2:
                        _crypto = new RC2CryptoServiceProvider();
                        break;
                    case Provider.Rijndael:
                        _crypto = new RijndaelManaged();
                        break;
                    case Provider.TripleDES:
                        _crypto = new TripleDESCryptoServiceProvider();
                        break;
                }

                Key = RandomKey();
                if (useDefaultInitializationVector)
                {
                    IntializationVector = new Data("%1Az=-@qT");
                }
                else
                {
                    IntializationVector = RandomInitializationVector();
                }
            }

            public Data RandomInitializationVector()
            {
                _crypto.GenerateIV();
                return new Data(_crypto.IV);
            }

            public Data RandomKey()
            {
                _crypto.GenerateKey();
                return new Data(_crypto.Key);
            }

            private void ValidateKeyAndIv(bool isEncrypting)
            {
                if (_key.IsEmpty)
                {
                    if (!isEncrypting)
                    {
                        throw new CryptographicException("No key was provided for the decryption operation!");
                    }

                    _key = RandomKey();
                }

                if (_iv.IsEmpty)
                {
                    if (!isEncrypting)
                    {
                        throw new CryptographicException("No initialization vector was provided for the decryption operation!");
                    }

                    _iv = RandomInitializationVector();
                }

                _crypto.Key = _key.Bytes;
                _crypto.IV = _iv.Bytes;
            }

            public Data Encrypt(Data d, Data key)
            {
                Key = key;
                return Encrypt(d);
            }

            public Data Encrypt(Data d)
            {
                MemoryStream memoryStream = new MemoryStream();
                ValidateKeyAndIv(isEncrypting: true);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, _crypto.CreateEncryptor(), CryptoStreamMode.Write);
                cryptoStream.Write(d.Bytes, 0, d.Bytes.Length);
                cryptoStream.Close();
                memoryStream.Close();
                return new Data(memoryStream.ToArray());
            }

            public Data Encrypt(Stream s, Data key, Data iv)
            {
                IntializationVector = iv;
                Key = key;
                return Encrypt(s);
            }

            public Data Encrypt(Stream s, Data key)
            {
                Key = key;
                return Encrypt(s);
            }

            public Data Encrypt(Stream s)
            {
                MemoryStream memoryStream = new MemoryStream();
                byte[] buffer = new byte[2049];
                ValidateKeyAndIv(isEncrypting: true);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, _crypto.CreateEncryptor(), CryptoStreamMode.Write);
                for (int num = s.Read(buffer, 0, 2048); num > 0; num = s.Read(buffer, 0, 2048))
                {
                    cryptoStream.Write(buffer, 0, num);
                }

                cryptoStream.Close();
                memoryStream.Close();
                return new Data(memoryStream.ToArray());
            }

            public Data Decrypt(Data encryptedData, Data key)
            {
                Key = key;
                return Decrypt(encryptedData);
            }

            public Data Decrypt(Stream encryptedStream, Data key)
            {
                Key = key;
                return Decrypt(encryptedStream);
            }

            public Data Decrypt(Stream encryptedStream)
            {
                MemoryStream memoryStream = new MemoryStream();
                byte[] buffer = new byte[2049];
                ValidateKeyAndIv(isEncrypting: false);
                CryptoStream cryptoStream = new CryptoStream(encryptedStream, _crypto.CreateDecryptor(), CryptoStreamMode.Read);
                for (int num = cryptoStream.Read(buffer, 0, 2048); num > 0; num = cryptoStream.Read(buffer, 0, 2048))
                {
                    memoryStream.Write(buffer, 0, num);
                }

                cryptoStream.Close();
                memoryStream.Close();
                return new Data(memoryStream.ToArray());
            }

            public Data Decrypt(Data encryptedData)
            {
                MemoryStream stream = new MemoryStream(encryptedData.Bytes, 0, encryptedData.Bytes.Length);
                checked
                {
                    byte[] array = new byte[encryptedData.Bytes.Length - 1 + 1];
                    ValidateKeyAndIv(isEncrypting: false);
                    CryptoStream cryptoStream = new CryptoStream(stream, _crypto.CreateDecryptor(), CryptoStreamMode.Read);
                    try
                    {
                        cryptoStream.Read(array, 0, encryptedData.Bytes.Length - 1);
                    }
                    catch (CryptographicException ex)
                    {
                        ProjectData.SetProjectError(ex);
                        CryptographicException inner = ex;
                        throw new CryptographicException("Unable to decrypt data. The provided key may be invalid.", inner);
                    }
                    finally
                    {
                        cryptoStream.Close();
                    }

                    return new Data(array);
                }
            }
        }

        public class Data
        {
            private byte[] _b;

            private int _MaxBytes;

            private int _MinBytes;

            private int _StepBytes;

            public static Encoding DefaultEncoding = Encoding.GetEncoding("Windows-1252");

            public Encoding Encoding;

            public bool IsEmpty
            {
                get
                {
                    if (_b == null)
                    {
                        return true;
                    }

                    if (_b.Length == 0)
                    {
                        return true;
                    }

                    return false;
                }
            }

            public int StepBytes
            {
                get
                {
                    return _StepBytes;
                }
                set
                {
                    _StepBytes = value;
                }
            }

            public int StepBits
            {
                get
                {
                    return checked(_StepBytes * 8);
                }
                set
                {
                    _StepBytes = value / 8;
                }
            }

            public int MinBytes
            {
                get
                {
                    return _MinBytes;
                }
                set
                {
                    _MinBytes = value;
                }
            }

            public int MinBits
            {
                get
                {
                    return checked(_MinBytes * 8);
                }
                set
                {
                    _MinBytes = value / 8;
                }
            }

            public int MaxBytes
            {
                get
                {
                    return _MaxBytes;
                }
                set
                {
                    _MaxBytes = value;
                }
            }

            public int MaxBits
            {
                get
                {
                    return checked(_MaxBytes * 8);
                }
                set
                {
                    _MaxBytes = value / 8;
                }
            }

            public byte[] Bytes
            {
                get
                {
                    checked
                    {
                        if (_MaxBytes > 0 && _b.Length > _MaxBytes)
                        {
                            byte[] array = new byte[_MaxBytes - 1 + 1];
                            Array.Copy(_b, array, array.Length);
                            _b = array;
                        }

                        if (_MinBytes > 0 && _b.Length < _MinBytes)
                        {
                            byte[] array2 = new byte[_MinBytes - 1 + 1];
                            Array.Copy(_b, array2, _b.Length);
                            _b = array2;
                        }

                        return _b;
                    }
                }
                set
                {
                    _b = value;
                }
            }

            public string Text
            {
                get
                {
                    if (_b == null)
                    {
                        return "";
                    }

                    int num = Array.IndexOf((Array)_b, (object)(byte)0);
                    if (num >= 0)
                    {
                        return Encoding.GetString(_b, 0, num);
                    }

                    return Encoding.GetString(_b);
                }
                set
                {
                    _b = Encoding.GetBytes(value);
                }
            }

            public string Hex
            {
                get
                {
                    return Utils.ToHex(_b);
                }
                set
                {
                    _b = Utils.FromHex(value);
                }
            }

            public string Base64
            {
                get
                {
                    return Utils.ToBase64(_b);
                }
                set
                {
                    _b = Utils.FromBase64(value);
                }
            }

            public Data()
            {
                _MaxBytes = 0;
                _MinBytes = 0;
                _StepBytes = 0;
                Encoding = DefaultEncoding;
            }

            public Data(byte[] b)
            {
                _MaxBytes = 0;
                _MinBytes = 0;
                _StepBytes = 0;
                Encoding = DefaultEncoding;
                _b = b;
            }

            public Data(string s)
            {
                _MaxBytes = 0;
                _MinBytes = 0;
                _StepBytes = 0;
                Encoding = DefaultEncoding;
                Text = s;
            }

            public Data(string s, Encoding encoding)
            {
                _MaxBytes = 0;
                _MinBytes = 0;
                _StepBytes = 0;
                Encoding = DefaultEncoding;
                Encoding = encoding;
                Text = s;
            }

            public new string ToString()
            {
                return Text;
            }

            public string ToBase64()
            {
                return Base64;
            }

            public string ToHex()
            {
                return Hex;
            }
        }

        internal class Utils
        {
            internal static string ToHex(byte[] ba)
            {
                if (ba == null || ba.Length == 0)
                {
                    return "";
                }

                StringBuilder stringBuilder = new StringBuilder();
                foreach (byte b in ba)
                {
                    stringBuilder.Append($"{b:X2}");
                }

                return stringBuilder.ToString();
            }

            internal static byte[] FromHex(string hexEncoded)
            {
                if (hexEncoded == null || hexEncoded.Length == 0)
                {
                    return null;
                }

                checked
                {
                    try
                    {
                        int num = Convert.ToInt32((double)hexEncoded.Length / 2.0);
                        byte[] array = new byte[num - 1 + 1];
                        int num2 = num - 1;
                        for (int i = 0; i <= num2; i++)
                        {
                            array[i] = Convert.ToByte(hexEncoded.Substring(i * 2, 2), 16);
                        }

                        return array;
                    }
                    catch (Exception ex)
                    {
                        ProjectData.SetProjectError(ex);
                        Exception innerException = ex;
                        throw new FormatException("The provided string does not appear to be Hex encoded:" + Environment.NewLine + hexEncoded + Environment.NewLine, innerException);
                    }
                }
            }

            internal static byte[] FromBase64(string base64Encoded)
            {
                if (base64Encoded == null || base64Encoded.Length == 0)
                {
                    return null;
                }

                try
                {
                    return Convert.FromBase64String(base64Encoded);
                }
                catch (FormatException ex)
                {
                    ProjectData.SetProjectError(ex);
                    FormatException innerException = ex;
                    throw new FormatException("The provided string does not appear to be Base64 encoded:" + Environment.NewLine + base64Encoded + Environment.NewLine, innerException);
                }
            }

            internal static string ToBase64(byte[] b)
            {
                if (b == null || b.Length == 0)
                {
                    return "";
                }

                return Convert.ToBase64String(b);
            }
        }
    }
    #if false // Decompilation log
    '26' items in cache
    ------------------
    Resolve: 'mscorlib, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
    Found single assembly: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
    WARN: Version mismatch. Expected: '1.0.5000.0', Got: '4.0.0.0'
    Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\mscorlib.dll'
    ------------------
    Resolve: 'Microsoft.VisualBasic, Version=7.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
    Found single assembly: 'Microsoft.VisualBasic, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
    WARN: Version mismatch. Expected: '7.0.5000.0', Got: '10.0.0.0'
    Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\Microsoft.VisualBasic.dll'
    ------------------
    Resolve: 'System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
    Found single assembly: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
    WARN: Version mismatch. Expected: '1.0.5000.0', Got: '4.0.0.0'
    Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.dll'
    ------------------
    Resolve: 'System.Data, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
    Found single assembly: 'System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
    WARN: Version mismatch. Expected: '1.0.5000.0', Got: '4.0.0.0'
    Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Data.dll'
    ------------------
    Resolve: 'System.Xml, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
    Found single assembly: 'System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
    WARN: Version mismatch. Expected: '1.0.5000.0', Got: '4.0.0.0'
    Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Xml.dll'
    ------------------
    Resolve: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
    Found single assembly: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
    Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\mscorlib.dll'
    #endif

