using System.Security.Cryptography;
using System.IO;
using System.Text;
namespace LitEngine
{
    namespace IO
    {
        
        public class AESReader : AesStreamBase
        {
            private RijndaelManaged mRijindael = null;
            private FileStream mStream = null;
            private BinaryReader mReaderStream = null;
            public AESReader(string _filename)
            {
                if (!File.Exists(_filename)) throw new System.NullReferenceException(_filename + "Can not found.");
                mStream = File.OpenRead(_filename);
                Init(mStream);
            }

            protected void Init(Stream _stream)
            {
                mRijindael = GetRijndael();
                ICryptoTransform cTransform = mRijindael.CreateDecryptor();
                CryptoStream cst = new CryptoStream(_stream, cTransform, CryptoStreamMode.Read);
                mReaderStream = new BinaryReader(cst);
            }
            override protected void DisposeStream()
            {
                mReaderStream.Close();
                mStream.Close();
                mStream.Dispose();
                mRijindael.Clear();
            }
        }
    }  
}
