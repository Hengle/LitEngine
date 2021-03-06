﻿using System;
using UnityEngine;
using System.Text;
namespace LitEngine
{
    using ProtoCSLS;
    namespace NetTool
    {
        public class SendData
        {
            short mCmd;
            short mLen;

            byte[] mData;
            short mIndex;
            bool mIsEnd;
            public bool Sended;
            #region 属性
            public byte[] Data
            {
                get
                {
                    return GetData();
                }
            }
            public short Len
            {
                get
                {
                    return mLen;
                }
            }

            public int SendLen
            {
                get
                {
                    return mIndex;
                }
            }

            public short Cmd
            {
                get
                {
                    return mCmd;
                }
            }
            #endregion
            public SendData(short _cmd)
            {
                mData = new byte[128];
                mCmd = _cmd;
                mLen = 0;
                mIndex = 0;
                mIsEnd = false;
                Sended = false;
                AddShort(mLen);
                AddShort(mCmd);
            }
            public void Rest()
            {
                mLen = 0;
                mIndex = 4;
                mIsEnd = false;
                Sended = false;
            }
            private byte[] GetData()
            {
                lock(this)
                {
                    if (mIsEnd) return mData;
                    mLen = (short)(mIndex - SocketDataBase.mPackageTopLen);
                    short tbackupindex = mIndex;
                    mIndex = 0;
                    AddShort(mLen);
                    mIndex = tbackupindex;
                    mIsEnd = true;
                    return mData;
                } 
            }

            #region　添加数据
            public void AddCSLEObject(CodeToolBase _codetool, object _object)
            {
                ProtoBufferWriterBuilderCSLS twbuilder = new ProtoBufferWriterBuilderCSLS(_codetool,_object);
                AddBytes(twbuilder.GetBuffer());
            }

            private void ChoseDataLen(short _len)
            {
                if ((_len + mIndex) < mData.Length) return;
                int tlen = mData.Length;
                byte[] tdata = new byte[_len + tlen * 2];
                Array.Copy(mData, tdata, tlen);
                mData = tdata;
            }
            public void AddByte(byte _src)
            {
                ChoseDataLen(1);
                mData[mIndex] = _src;
                mIndex++;
            }

            public void AddBytes(byte[] _src)
            {
                if (_src == null) return;
                short tlen = (short)_src.Length;
                ChoseDataLen(tlen);
                Array.Copy(_src, 0, mData, mIndex, tlen);
                mIndex += tlen;
            }

            public void AddShort(short _src)
            {
                AddBytes(BufferBase.GetBuffer(_src));
            }

            public void AddInt(int _src)
            {
                AddBytes(BufferBase.GetBuffer(_src));
            }

            public void AddLong(long _src)
            {
                AddBytes(BufferBase.GetBuffer(_src));
            }

            public void AddFloat(float _src)
            {
                AddBytes(BufferBase.GetBuffer(_src));
            }

            public void AddBool(bool _src)
            {
                AddByte(BufferBase.GetBuffer(_src));
            }

            public void AddString(string _src)
            {
                AddBytes(BufferBase.GetBuffer(_src));
            }


            #endregion
            #region 扩展的添加数据
            public void AddVector2(Vector2 _src)
            {
                AddFloat(_src.x);
                AddFloat(_src.y);
            }

            public void AddVector3(Vector3 _src)
            {
                AddFloat(_src.x);
                AddFloat(_src.y);
                AddFloat(_src.z);
            }

            public void AddVector4(Vector4 _src)
            {
                AddFloat(_src.x);
                AddFloat(_src.y);
                AddFloat(_src.z);
                AddFloat(_src.w);
            }

            public void AddQuaternion(Quaternion _src)
            {
                AddFloat(_src.x);
                AddFloat(_src.y);
                AddFloat(_src.z);
                AddFloat(_src.w);
            }
            #endregion


        }
    }
}

