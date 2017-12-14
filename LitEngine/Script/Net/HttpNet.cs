using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

using System.Net;
using System.Threading;

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace LitEngine
{
    using UpdateSpace;
    namespace NetTool
    {

        public class HttpData :System.IDisposable
        {
            public string AppName { get; private set; }
            public string Error { get; private set; }
            public bool IsDone { get; private set; }
            public byte[] RecBuffer { get; private set; }
            public UpdateNeedDisObject UpdateObj { get; private set; }
            string mKey;
            string mUrl;
            
            bool mSending = false;
            bool mThreadRuning = false;

            public HttpWebRequest Request { get; private set; }
            private Thread mSendThread = null;

            System.Action<string,string, byte[]> mDelgate;
            public HttpData(string _appname,string _key, string _url, System.Action<string,string, byte[]> _delgate)
            {
                AppName = _appname;
                mKey = _key;
                mUrl = _url;
                mDelgate = _delgate;

                UpdateObj = new UpdateNeedDisObject(_appname, Update, Dispose);

                Request = (HttpWebRequest)HttpWebRequest.Create(mUrl);
                Request.Timeout = 10000;

                IsDone = false;
                RecBuffer = null;
                Error = null;
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
                UpdateObj.UnRegToOwner();
                UpdateObj = null;

                mThreadRuning = false;
                if (Request != null)
                    Request.Abort();
                Request = null;

                if (mSendThread != null)
                    mSendThread.Join();
                mSendThread = null;

                mDelgate = null;
                RecBuffer = null;
                
            }


            public void SendAsync()
            {
                if (mSending) return;
                mSending = true;
                mThreadRuning = true;
                UpdateObj.RegToOwner();

                mSendThread = new Thread(SendRequest);
                mSendThread.IsBackground = true;
                mSendThread.Start();
            }


            private void SendRequest()
            {
                
                System.IO.Stream tHttpStream = null;
                System.IO.MemoryStream tmem = null;
                
                try
                {

                    if (mUrl.Contains("https://"))
                        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

                    WebResponse tresponse = Request.GetResponse();
                    long tcontexlen = tresponse.ContentLength;
                    tHttpStream = tresponse.GetResponseStream();

                    tmem = new System.IO.MemoryStream();
                    int tlen = 256;
                    byte[] tbuffer = new byte[tlen];
                    int tReadSize = 0;
                    tReadSize = tHttpStream.Read(tbuffer, 0, tlen);
                    while (tReadSize > 0 && mThreadRuning)
                    {
                        tmem.Write(tbuffer, 0, tReadSize);
                        tReadSize = tHttpStream.Read(tbuffer, 0, tlen);
                    }

                    RecBuffer = tmem.GetBuffer();
                }
                catch(System.Exception _error)
                {
                    Error = _error.ToString();
                }

                if (tHttpStream != null)
                    tHttpStream.Close();

                if (Request != null)
                    Request.Abort();
                Request = null;

                if (tmem != null)
                    tmem.Close();
                
                IsDone = true;

            }
            private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
            {
                return true;
            }

            private void Update()
            {
                if (!IsDone) return;
                if (mDelgate != null && mDelgate.Target != null)
                    mDelgate(mKey, Error, RecBuffer);
                mSending = false;
                Dispose();
            }

        }

        public class HttpNet : MonoBehaviour
        {
            static private HttpNet sInstance = null;
            private UpdateObjectVector UpdateList = new UpdateObjectVector(UpdateType.Update);
            static private void CreatInstance()
            {
                if (sInstance == null)
                {
                    UnityEngine.GameObject tobj = new UnityEngine.GameObject("HttpNet");
                    UnityEngine.GameObject.DontDestroyOnLoad(tobj);
                    sInstance = tobj.AddComponent<HttpNet>();
                    tobj.SetActive(false);
                }
            }
            static public void ClearByKey(string _appkey)
            {
                if (sInstance != null)
                    sInstance.UpdateList.ClearByKey(_appkey);
            }

            static public void Send(string _appname, string _url, string _key, System.Action<string,string, byte[]> _delegate)
            {
                if (sInstance == null) CreatInstance();
                SendData(new HttpData(_appname, _key, _url, _delegate));
            }
            static public void SendData(HttpData _data)
            {
                if (sInstance == null) CreatInstance();
                _data.UpdateObj.Owner = sInstance.UpdateList;
                _data.SendAsync();
                sInstance.SetActive(true);
            }

            protected void OnDestroy()
            {
                sInstance = null;
                UpdateList.Clear();
            }

            protected void SetActive(bool _active)
            {
                if (gameObject.activeSelf != _active)
                    gameObject.SetActive(_active);
            }

           

            void Update()
            {
                UpdateList.Update();
                if (UpdateList.Count == 0)
                    gameObject.SetActive(false);
            }
        }
    }
}


