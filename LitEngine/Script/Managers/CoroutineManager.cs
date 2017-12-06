using UnityEngine;
using System.Collections;
namespace LitEngine
{
    #region 协同Object
    public class CoroutineManager : MonoManagerBase
    {
        override public void DestroyManager()
        {
            base.DestroyManager();
        }
        override protected void OnDestroy()
        {
            base.OnDestroy();
        }
    }
    #endregion
}


