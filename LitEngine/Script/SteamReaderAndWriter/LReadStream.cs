using System.IO;
using System.Text;

namespace LitEngine
{
    namespace IO
    {
        #region 读取
        public class Reader : LStreamBase
        {
            public byte[] Buffer { get; protected set; }
            public Reader(string _FullName)
            {
                if (!File.Exists(_FullName))
                {
                    DLog.LogError("文件不存在 _FullName = " + _FullName);
                    return;
                }

                Buffer = File.ReadAllBytes(_FullName);
                Length = Buffer.Length - cSafeBtye;
                encryptAndUncrypt(Buffer, Length);

            }

            override protected void DisposeStream()
            {
                Buffer = null;
                Length = 0;
            }

            override public void Close()
            {
                Dispose();
            }

            #region 读取
            unsafe public static void GetValue(byte* pdata, byte[] _buffer, long _startindex, long _length)
            {
                if (_startindex + _length - 1 >= _buffer.Length)
                {
                    DLog.LogError("GetValue数组越界");
                    return;
                }
                long imax = _startindex + _length;
                for (long i = _startindex; i < imax; i++)
                    *pdata++ = _buffer[i];

            }
            public byte ReadByte()
            {
                return Buffer[mIndex++];
            }

            public byte[] ReadBytes(int _length)
            {
                if (mIndex + _length > Length)
                {
                    DLog.LogError("ReadBytes读取长度超出上限._length = " + _length);
                    return null;
                }
                byte[] ret = new byte[_length];
                System.Array.Copy(Buffer, mIndex, ret, 0, _length);

                mIndex += _length;
                return ret;
            }

            unsafe public short ReadShort()
            {
                short u = 0;
                GetValue((byte*)&u, Buffer, mIndex, sizeof(short));
                mIndex += sizeof(short);
                return u;
            }

            unsafe public int ReadInt()
            {
                int u = 0;
                GetValue((byte*)&u, Buffer, mIndex, sizeof(int));
                mIndex += sizeof(int);
                return u;
            }

            unsafe public long ReadLong()
            {
                long u = 0;
                GetValue((byte*)&u, Buffer, mIndex, sizeof(long));
                mIndex += sizeof(long);
                return u;
            }

            unsafe public float ReadFloat()
            {
                float u = 0;
                GetValue((byte*)&u, Buffer, mIndex, sizeof(float));
                mIndex += sizeof(float);
                return u;
            }

            unsafe public bool ReadBool()
            {
                bool u = false;
                byte* pdata = (byte*)&u;
                *pdata = Buffer[mIndex++];
                return u;
            }

            public string ReadString()
            {
                int len = ReadInt();
                byte[] tarry = ReadBytes(len);
                return Encoding.UTF8.GetString(tarry);
            }
            #endregion
        }
        #endregion
    }
}
