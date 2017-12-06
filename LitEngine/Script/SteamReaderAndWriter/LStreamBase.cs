using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LitEngine
{
    namespace IO
    {
        public class LStreamBase : System.IDisposable
        {
            static protected byte[] sSecret = Encoding.UTF8.GetBytes("aw#dl*(&^apbckytrzlkjvcxnpoiuq^*(lka5$#sdflkjsdffdnppz,<ds>");
            protected const int cSafeBtye = 100;
            virtual public long Length { get; protected set; }

            protected long mIndex = 0;

            virtual protected void encryptAndUncrypt(byte[] _value, long _size)
            {
                int j = 0;
                for (int i = 0; i < _size; i++)
                {
                    _value[i] = (byte)(_value[i] ^ sSecret[j]);
                    j++;
                    if (j >= (sSecret.Length - 1))
                        j = 0;
                }
            }

            virtual protected void encryptAES(byte[] _value, long _size)
            {

            }

            ~LStreamBase()
            {
                Dispose(false);
            }

            protected bool mDisposed = false;
            public void Dispose()
            {
                Dispose(true);
                System.GC.SuppressFinalize(this);
            }

            protected void Dispose(bool _disposing)
            {
                if (mDisposed)
                    return;
                DisposeStream();
                mDisposed = true;
            }

            virtual protected void DisposeStream()
            {

            }

            virtual public void Close()
            {

            }
        }
    }
}
