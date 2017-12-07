using System.Security.Cryptography;
using System.IO;
namespace LitEngine
{
    namespace IO
    {
        public class AESReader : AesStreamBase
        {
            private BinaryReader mReaderStream = null;
            
            public AESReader(string _filename)
            {
                if (!File.Exists(_filename)) throw new System.NullReferenceException(_filename + "Can not found.");
                mStream = File.OpenRead(_filename);
                Init();
            }

            public AESReader(Stream _stream)
            {
                mStream = _stream;
                Init();
            }

            protected void Init()
            {
                mRijindael = GetRijndael();
                ICryptoTransform cTransform = mRijindael.CreateDecryptor();
                mCrypto = new CryptoStream(mStream, cTransform, CryptoStreamMode.Read);
                mReaderStream = new BinaryReader(mCrypto);
            }

            public override void Close()
            {
                if (mClosed) return;
                mClosed = true;
                mReaderStream.Close();
                base.Close();
            }

            public long Position
            {
                get
                {
                    return mStream.Position;
                }
            }
            public long Length
            {
                get
                {
                    return mStream.Length;
                }
            }

            #region 读取
            public virtual byte[] ReadAllBytes()
            {
                byte[] tbytes = ReadBytes((int)Length);
                byte[] ret = new byte[tbytes.Length - SafeByteLen];
                System.Array.Copy(tbytes, 0, ret, 0, tbytes.Length - SafeByteLen);
                return ret;
            }

            public virtual bool ReadBoolean()
            {
                return mReaderStream.ReadBoolean();
            }
            public virtual byte ReadByte()
            {
                return mReaderStream.ReadByte();
            }

            public virtual int Read(byte[] buffer, int index, int count)
            {
                return mReaderStream.Read(buffer, index, count);
            }

            public virtual byte[] ReadBytes(int count)
            {
                return mReaderStream.ReadBytes(count);
            }

            public virtual char ReadChar()
            {
                return mReaderStream.ReadChar();
            }

            public virtual char[] ReadChars(int count)
            {
                return mReaderStream.ReadChars(count);
            }

            public virtual decimal ReadDecimal()
            {
                return mReaderStream.ReadDecimal();
            }

            public virtual double ReadDouble()
            {
                return mReaderStream.ReadDouble();
            }

            public virtual short ReadInt16()
            {
                return mReaderStream.ReadInt16();
            }

            public virtual int ReadInt32()
            {
                return mReaderStream.ReadInt32();
            }

            public virtual long ReadInt64()
            {
                return mReaderStream.ReadInt64();
            }

            public virtual sbyte ReadSByte()
            {
                return mReaderStream.ReadSByte();
            }

            public virtual float ReadSingle()
            {
                return mReaderStream.ReadSingle();
            }

            public virtual string ReadString()
            {
                return mReaderStream.ReadString();
            }

            public virtual ushort ReadUInt16()
            {
                return mReaderStream.ReadUInt16();
            }

            public virtual uint ReadUInt32()
            {
                return mReaderStream.ReadUInt32();
            }

            public virtual ulong ReadUInt64()
            {
                return mReaderStream.ReadUInt64();
            }
            #endregion
        }
    }  
}
