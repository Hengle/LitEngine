﻿using UnityEngine;
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
            public long ContentLength { get; private set; }
            public long DownLoadedLength { get; private set; }
            public byte[] RecBuffer { get; private set; }
            public UpdateNeedDisObject UpdateObj { get; private set; }
            string mKey;
            string mUrl;
            
            bool mSending = false;
            bool mThreadRuning = false;

            public HttpWebRequest Request { get; private set; }
            private WebResponse mResponse;
            private System.IO.Stream mHttpStream = null;
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
                ContentLength = 0;
                DownLoadedLength = 0;
            }
            ~HttpData()
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

                UpdateObj.UnRegToOwner();
                UpdateObj = null;

                mThreadRuning = false;

                if (mSendThread != null)
                    mSendThread.Join();
                mSendThread = null;

                CloseHttpClient();

                mDelgate = null;
                RecBuffer = null;
                mSending = false;
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
                
                System.IO.MemoryStream tmem = null;
                
                try
                {

                    if (mUrl.Contains("https://"))
                        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

                    mResponse = Request.GetResponse();
                    
                    mHttpStream = mResponse.GetResponseStream();
                    ContentLength = mResponse.ContentLength;

                    tmem = new System.IO.MemoryStream();
                    int tlen = 256;
                    byte[] tbuffer = new byte[tlen];
                    int tReadSize = 0;
                    tReadSize = mHttpStream.Read(tbuffer, 0, tlen);
                    while (tReadSize > 0 && mThreadRuning)
                    {
                        DownLoadedLength += tReadSize;
                        tmem.Write(tbuffer, 0, tReadSize);
                        tReadSize = mHttpStream.Read(tbuffer, 0, tlen);
                    }

                    RecBuffer = tmem.GetBuffer();
                }
                catch(System.Exception _error)
                {
                    Error = _error.ToString();
                }

                if (Error == null && DownLoadedLength != ContentLength)
                {
                    Error = "Http请求未能返回完整数据.Stream 被中断.";
                }

                if (tmem != null)
                    tmem.Close();
                CloseHttpClient();
                IsDone = true;

            }

            private void CloseHttpClient()
            {
                if (mHttpStream != null)
                {
                    mHttpStream.Close();
                    mHttpStream.Dispose();
                    mHttpStream = null;
                }

                if (mResponse != null)
                {
                    mResponse.Close();
                    mResponse = null;
                }

                if (Request != null)
                {
                    Request.Abort();
                    Request = null;
                }
            }
            private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
            {
                return true;
            }

            private void Update()
            {
                if (!IsDone) return;

                System.Action<string, string, byte[]> tdelegate = mDelgate;
                string tkey = mKey;
                string terror = Error;
                byte[] tbuffer = RecBuffer;
                Dispose();
                try
                {
                    if (tdelegate != null)
                        tdelegate(tkey, terror, tbuffer);
                }
                catch (System.Exception _error)
                {
                    DLog.LogError(_error);
                }
                
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


