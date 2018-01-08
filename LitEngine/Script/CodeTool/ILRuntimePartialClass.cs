using ILRuntime.CLR.Method;
namespace ILRuntime.Runtime.Enviorment
{
    public partial class AppDomain
    {
        public string AppName { get; set; }
        public void Clear()
        {
            DelegateManager.Clear();

            freeIntepreters.Clear();
            intepreters.Clear();
            crossAdaptors.Clear();
            valueTypeBinders.Clear();
            mapType.Clear();
            clrTypeMapping.Clear();
            mapTypeToken.Clear();
            mapMethod.Clear();
            mapString.Clear();
            redirectMap.Clear();
            fieldGetterMap.Clear();
            fieldSetterMap.Clear();
            memberwiseCloneMap.Clear();
            createDefaultInstanceMap.Clear();
            createArrayInstanceMap.Clear();
            loadedAssemblies = null;
            references.Clear();

            LoadedTypes.Clear();
            RedirectMap.Clear();
            FieldGetterMap.Clear();
            FieldSetterMap.Clear();
            MemberwiseCloneMap.Clear();
            CreateDefaultInstanceMap.Clear();
            CreateArrayInstanceMap.Clear();
            CrossBindingAdaptors.Clear();
            ValueTypeBinders.Clear();
            Intepreters.Clear();
            FreeIntepreters.Clear();
        }
    }

    public partial class DelegateManager
    {
        public bool IsRegToMethodDelegate(ILMethod method)
        {
            if (method.ParameterCount == 0) return true;
            foreach (var i in methods)
            {
                if (i.ParameterTypes.Length == method.ParameterCount)
                {
                    bool match = true;
                    for (int j = 0; j < method.ParameterCount; j++)
                    {
                        if (i.ParameterTypes[j] != method.Parameters[j].TypeForCLR)
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                        return true;
                }
            }
            return false;
        }

        public void Clear()
        {
            methods.Clear();
            functions.Clear();
            clrDelegates.Clear();
        }
    }

    
}
namespace ILRuntime.Runtime.Intepreter
{
    abstract partial class DelegateAdapter : ILTypeInstance, IDelegateAdapter
    {
        public string AppName { get { return appdomain != null ? appdomain.AppName : ""; } }
    }
}
