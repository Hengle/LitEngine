using System.IO;
using System.Text;

namespace LitEngine
{
    namespace IO
    {
        #region 写入
        public class Writer : LStreamBase
        {
            override public long Length { get { return mIndex; } }
            public string FileName { get; private set; }
            private MemoryStream mMemS;
            public Writer(string _FullName)
            {
                FileName = _FullName;
            }

            ~Writer()
            {
            }


            override protected void DisposeStream()
            {
                mIndex = 0;
                mMemS.Close();
            }

            override public void Close()
            {
                byte[] tbytes = mMemS.ToArray();
                encryptAndUncrypt(tbytes, tbytes.Length);
                FileStream tfile = File.OpenWrite(FileName);
                tfile.Write(tbytes, 0, (int)mIndex);
                tfile.Write(new byte[100], 0, 100);
                tfile.Flush();
                tfile.Close();
                tfile.Dispose();
                Dispose();
            }

            #region Write
            unsafe public static byte[] GetBytes(byte* pdata, int _length)
            {
                byte[] retbuffer = new byte[_length];
                for (int i = 0; i < _length; i++)
                    retbuffer[i] = *pdata++;
                return retbuffer;
            }
            public void WriteByte(byte _src)
            {
                mMemS.WriteByte(_src);
                mIndex++;
            }

            public void WriteBytes(byte[] _src)
            {
                if (_src == null) return;
                short tlen = (short)_src.Length;
                mMemS.Write(_src, 0, tlen);
                mIndex += tlen;
            }

            public void WriteShort(short _src)
            {
                WriteBytes(ShortBytes(_src));
            }

            public void WriteInt(int _src)
            {
                WriteBytes(IntBytes(_src));
            }

            public void WriteLong(long _src)
            {
                WriteBytes(LongBytes(_src));
            }

            public void WriteFloat(float _src)
            {
                WriteBytes(FloatBytes(_src));
            }

            public void WriteBool(bool _src)
            {
                WriteByte(BoolBytes(_src));
            }

            public void WriteString(string _src)
            {
                byte[] strbyte = Encoding.UTF8.GetBytes(_src);
                WriteInt(strbyte.Length);
                WriteBytes(strbyte);
            }
            #endregion

            #region 取得bytes

            unsafe public static byte[] IntBytes(int _src)
            {
                return GetBytes((byte*)&_src, sizeof(int));
            }
            unsafe public static byte[] ShortBytes(short _src)
            {
                return GetBytes((byte*)&_src, sizeof(short));
            }
            unsafe public static byte[] LongBytes(long _src)
            {
                return GetBytes((byte*)&_src, sizeof(long));
            }

            unsafe public static byte[] FloatBytes(float _src)
            {
                return GetBytes((byte*)&_src, sizeof(float));
            }
            unsafe public static byte BoolBytes(bool _src)
            {
                byte* pdata = (byte*)&_src;
                byte tBuffer = *pdata;
                return tBuffer;
            }

            public static byte[] StringBytes(string _src)
            {
                byte[] strbyte = Encoding.UTF8.GetBytes(_src);
                byte[] lenbyte = IntBytes(strbyte.Length);
                byte[] ret = new byte[strbyte.Length + lenbyte.Length];

                lenbyte.CopyTo(ret, 0);
                if (strbyte.Length > 0)
                    strbyte.CopyTo(ret, lenbyte.Length);

                return ret;
            }
            #endregion

        }
        #endregion
    }
}
