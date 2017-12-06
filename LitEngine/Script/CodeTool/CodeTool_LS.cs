using System;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.CLR.Method;
using UnityEngine;
namespace LitEngine
{
    public class CodeTool_LS : CodeToolBase
    {
        #region 初始化
        public static System.Action<ILRuntime.Runtime.Enviorment.AppDomain> CLRBindingDelegate = null;
        public static System.Action<ILRuntime.Runtime.Enviorment.AppDomain> RegisterDelegate = null;
        public static System.Action<ILRuntime.Runtime.Enviorment.AppDomain> TypeBindingDelegate = null;

        protected ILRuntime.Runtime.Enviorment.AppDomain mApp;
        public CodeTool_LS(string _appname , ILRuntime.Runtime.Enviorment.AppDomain _app):base(_appname)
        {
            mApp = _app;
            RegDelegate();
            BindingCLR();
        }
        protected void BindingCLR()
        {
            if (CLRBindingDelegate != null)
                CLRBindingDelegate(mApp);
            if (TypeBindingDelegate != null)
                TypeBindingDelegate(mApp);
        }
        protected void RegDelegate()
        {
            mApp.DelegateManager.RegisterMethodDelegate<float>();
            mApp.DelegateManager.RegisterMethodDelegate<int>();
            mApp.DelegateManager.RegisterMethodDelegate<bool>();
            mApp.DelegateManager.RegisterMethodDelegate<short>();
            mApp.DelegateManager.RegisterMethodDelegate<string>();
            mApp.DelegateManager.RegisterMethodDelegate<object>();

            mApp.DelegateManager.RegisterMethodDelegate<NetTool.ReceiveData>();
            mApp.DelegateManager.RegisterMethodDelegate<NetTool.MSG_RECALL_DATA>();
            mApp.DelegateManager.RegisterMethodDelegate<UnityEngine.Object>();
            mApp.DelegateManager.RegisterMethodDelegate<WWW>();
            mApp.DelegateManager.RegisterMethodDelegate<Component>();
            mApp.DelegateManager.RegisterMethodDelegate<GameObject>();
            mApp.DelegateManager.RegisterMethodDelegate<Transform>();

            mApp.DelegateManager.RegisterMethodDelegate<string, byte[]>();
            mApp.DelegateManager.RegisterMethodDelegate<object, object>();
            mApp.DelegateManager.RegisterMethodDelegate<int, int>();
            mApp.DelegateManager.RegisterMethodDelegate<int, string>();
            mApp.DelegateManager.RegisterMethodDelegate<string, float>();
            mApp.DelegateManager.RegisterMethodDelegate<string, string>();
            mApp.DelegateManager.RegisterMethodDelegate<string, object>();

            mApp.DelegateManager.RegisterMethodDelegate<long, long, float>();
            mApp.DelegateManager.RegisterMethodDelegate<int,int,float>();
            mApp.DelegateManager.RegisterMethodDelegate<string, int, int>();
            mApp.DelegateManager.RegisterMethodDelegate<int, string, string>();
            mApp.DelegateManager.RegisterMethodDelegate<string, object, object>();
            mApp.DelegateManager.RegisterMethodDelegate<object, object, object>();

            if (RegisterDelegate != null)
                RegisterDelegate(mApp);
        }
        override protected void DisposeNoGcCode()
        {
            mApp.Clear();
            mApp = null;
        }
        #endregion

        #region 类型判断
        override public IType GetLType(string _name)
        {
            return mApp.GetType(_name);
        }

        override public IType GetObjectType(object _obj)
        {
            if (_obj == null) throw new NullReferenceException("LS GetObjectType _obj = null");
            if (_obj is ILTypeInstance)
                return ((ILTypeInstance)_obj).Type;
            else
                DLog.LogError("GetObjectType 只可用于ILTypeInstance");
            return null;
        }
        override public bool IsLSType(Type _type)
        {
            if (typeof(ILTypeInstance).IsAssignableFrom(_type))
                return true;
            return false;
        }
        override public IType GetListChildType(IType _type)
        {
            return _type.GenericArguments[0].Value;
        }
        #endregion
        #region 方法
        override public object GetLMethod(IType _type, string _funname, int _pamcount)
        {
            object ret = null;
            ret = _type.GetMethod(_funname, _pamcount);
            return ret;
        }

        override public object CallMethodNoTry(object method, object _this, params object[] _params)
        {
            return mApp.Invoke((IMethod)method, _this, _params);
        }

        override public object CallMethod(object method, object _this, params object[] _params)
        {
            try
            {
                return CallMethodNoTry(method, _this, _params);
            }
            catch (Exception _erro)
            {
                DLog.LogError(_erro);
            }
            return null;
        }

        override public object CallMethodByName(string _name, object _this, params object[] _params)
        {
            if (_name == null || _name.Equals("")) return null;
            if (_this == null || !IsLSType(_this.GetType())) return null;
            int tpramcount = _params != null ? _params.Length : 0;
            ILTypeInstance tilobj = _this as ILTypeInstance;
            IType ttype = tilobj.Type;
            object tmethod = GetLMethod(ttype, _name, tpramcount);
            return CallMethod(tmethod, _this, _params);
        }
        #endregion
        #region 属性
        #region 获取
        override public object GetTargetMemberByKey(string _key, object _target)
        {
            if (_target == null) return null;
            IType ttype = GetObjectType(_target);
            return GetMemberByKey(ttype, _key, _target);
        }

        override public object GetTargetMemberByIndex(int _index, object _target)
        {
            if (_target == null) return null;
            IType ttype = GetObjectType(_target);
            return GetMemberByIndex(ttype, _index, _target);
        }

        override public object GetMemberByKey(IType _type, string _key, object _target)
        {
            if (_type == null)
                throw new NullReferenceException("Base GetMemberByIndex _type = null");
            if (_target == null)
                throw new NullReferenceException("Base GetMemberByIndex _object = null");
            if (!typeof(ILTypeInstance).IsInstanceOfType(_target))
                throw new NullReferenceException("Base GetMemberByIndex _object is not ILTypeInstance");
            ILTypeInstance tilobj = _target as ILTypeInstance;
            int tindex = 0;
            IType ttype = tilobj.Type.GetField(_key, out tindex);
            return tilobj[tindex];

        }

        override public object GetMemberByIndex(IType _type, int _index, object _target)
        {
            if (_type == null)
                throw new NullReferenceException("Base GetMemberByIndex _type = null");
            if (_target == null)
                throw new NullReferenceException("Base GetMemberByIndex _object = null");
            if (!typeof(ILTypeInstance).IsInstanceOfType(_target))
                throw new NullReferenceException("Base GetMemberByIndex _object is not ILTypeInstance");
            ILTypeInstance tilobj = _target as ILTypeInstance;
            return tilobj[_index];
        }
        #endregion

        #region 设置
        override public void SetMember(IType _type, int _index, object _object, object _target)
        {
            if (_type == null)
                throw new NullReferenceException("Base GetMemberByIndex _type = null");
            if (_target == null)
                throw new NullReferenceException("Base GetMemberByIndex _object = null");
            if (!typeof(ILTypeInstance).IsInstanceOfType(_target))
                throw new NullReferenceException("Base GetMemberByIndex _object is not ILTypeInstance");
            ILTypeInstance tilobj = _target as ILTypeInstance;
            tilobj[_index] = _object;
        }
        override public void SetMember(IType _type, string _key, object _object, object _target)
        {
            if (_type == null)
                throw new NullReferenceException("Base GetMemberByIndex _type = null");
            if (_target == null)
                throw new NullReferenceException("Base GetMemberByIndex _object = null");
            if (!typeof(ILTypeInstance).IsInstanceOfType(_target))
                throw new NullReferenceException("Base GetMemberByIndex _object is not ILTypeInstance");
            ILTypeInstance tilobj = _target as ILTypeInstance;
            int tindex = 0;
            IType ttype = tilobj.Type.GetField(_key, out tindex);
            tilobj[tindex] = _object;
        }
        #endregion

        #endregion
        #region 对象获取
        override public object GetCSLEObjectParmasByType(IType _type, params object[] _parmas)
        {
            if (_type == null) throw new NullReferenceException("LS GetCSLEObjectParmasByType _type = null");
            ILType ilType = _type as ILType;
            if (ilType != null)
            {
                bool hasConstructor = _parmas != null && _parmas.Length != 0;
                ILTypeInstance res = ilType.Instantiate(!hasConstructor);
                if (hasConstructor)
                {
                    IMethod ilm = ilType.GetConstructor(_parmas.Length);
                    mApp.Invoke(ilm, res, _parmas);
                }
                return res;
            }
            return null;
        }

        #endregion
        #region 委托
        private IDelegateAdapter GetDelgateAdapter(string _Function,int _pramcount,IType _classtype, object _target ,out bool _isNeedReg)
        {
            _isNeedReg = false;
            if (_classtype == null || _target == null) return null;
            ILMethod methodctor = GetLMethod(_classtype, _Function, _pramcount) as ILMethod;
            if (methodctor == null) return null;
            if (!typeof(ILTypeInstance).IsInstanceOfType(_target)) return null;
            ILTypeInstance tclrobj = _target as ILTypeInstance;
            _isNeedReg = !mApp.DelegateManager.IsRegToMethodDelegate(methodctor);
            if (_isNeedReg) return null;
            IDelegateAdapter ret = tclrobj.GetDelegateAdapter(methodctor);
            if(ret == null)
                ret = mApp.DelegateManager.FindDelegateAdapter(tclrobj, methodctor);
            return ret;
        }

        override public K GetCSLEDelegate<K>(string _Function, IType _classtype, object _target)
        {
            if (_classtype == null || _target == null) return default(K);
            object ret = null;
            bool tneedreg = false;
            IDelegateAdapter tdelapt = GetDelgateAdapter(_Function,0, _classtype, _target,out tneedreg);
            if (tdelapt != null)
                ret = tdelapt.Delegate;
            return (K)ret;
        }

        override public K GetCSLEDelegate<K, T1>(string _Function, IType _classtype, object _target)
        {
            if (_classtype == null || _target == null) return default(K);
            object ret = null;
            bool tneedreg = false;
            IDelegateAdapter tdelapt = GetDelgateAdapter(_Function,1, _classtype, _target,out tneedreg);
            if(tneedreg)
            {
                mApp.DelegateManager.RegisterMethodDelegate<T1>();
                tdelapt = GetDelgateAdapter(_Function,1, _classtype, _target, out tneedreg);
            }
            if (tdelapt != null)
                ret = tdelapt.Delegate;
            return (K)ret;
        }

        override public K GetCSLEDelegate<K, T1, T2>(string _Function, IType _classtype, object _target)
        {
            if (_classtype == null || _target == null) return default(K);
            object ret = null;
            bool tneedreg = false;
            IDelegateAdapter tdelapt = GetDelgateAdapter(_Function,2, _classtype, _target, out tneedreg);
            if (tneedreg)
            {
                mApp.DelegateManager.RegisterMethodDelegate<T1,T2>();
                tdelapt = GetDelgateAdapter(_Function,2, _classtype, _target, out tneedreg);
            }
            if (tdelapt != null)
                ret = tdelapt.Delegate;
            return (K)ret;
        }

        override public K GetCSLEDelegate<K, T1, T2, T3>(string _Function, IType _classtype, object _target)
        {
            if (_classtype == null || _target == null) return default(K);
            object ret = null;
            bool tneedreg = false;
            IDelegateAdapter tdelapt = GetDelgateAdapter(_Function,3, _classtype, _target, out tneedreg);
            if (tneedreg)
            {
                mApp.DelegateManager.RegisterMethodDelegate<T1, T2,T3>();
                tdelapt = GetDelgateAdapter(_Function,3, _classtype, _target, out tneedreg);
            }
            if (tdelapt != null)
                ret = tdelapt.Delegate;
            return (K)ret;
        }
        override public K GetCSLEDelegate<K, T1, T2, T3, T4>(string _Function, IType _classtype, object _target)
        {
            if (_classtype == null || _target == null) return default(K);
            object ret = null;
            bool tneedreg = false;
            IDelegateAdapter tdelapt = GetDelgateAdapter(_Function,4, _classtype, _target, out tneedreg);
            if (tneedreg)
            {
                mApp.DelegateManager.RegisterMethodDelegate<T1, T2, T3,T4>();
                tdelapt = GetDelgateAdapter(_Function,4, _classtype, _target, out tneedreg);
            }
            if (tdelapt != null)
                ret = tdelapt.Delegate;
            return (K)ret;
        }
        #endregion
    }
}

