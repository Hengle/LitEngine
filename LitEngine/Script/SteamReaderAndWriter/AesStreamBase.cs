using System.Security.Cryptography;
using System.Text;
namespace LitEngine
{
    namespace IO
    {
        public abstract class AesStreamBase : System.IDisposable
        {
            protected const string AESKey = "ae125efkk4454eeff444ferfkny6oxi8";

            ~AesStreamBase()
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
                mDisposed = true;
                DisposeStream();
            }

            virtual protected RijndaelManaged GetRijndael()
            {
                RijndaelManaged ret = new RijndaelManaged();
                byte[] keyArray = Encoding.UTF8.GetBytes(AESKey);
                ret.Key = keyArray;
                ret.Mode = CipherMode.ECB;
                ret.Padding = PaddingMode.PKCS7;
                return ret;
            }

            abstract protected void DisposeStream();

        }
    }
   
}
