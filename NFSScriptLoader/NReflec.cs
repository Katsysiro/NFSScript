using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace NFSScriptLoader
{
    static class NReflec
    {
        public static Type[] GetTypesFromDLL(string dllFilePath)
        {
            return Assembly.LoadFile(dllFilePath).GetTypes();
        }

        public static Type[] GetTypesFromAssembly(Assembly ass)
        {
            return ass.GetTypes();
        }

        public static MethodInfo[] GetMethodsFromDLL(string dllFilePath)
        {
            var m = new List<MethodInfo>();
            var types = Assembly.LoadFile(dllFilePath).GetTypes();
            for (var i = 0; i < types.Length; i++)
                m.AddRange(types[i].GetMethods());

            return m.ToArray();
        }

        public static MethodInfo[] GetMethodsFromAssembly(Assembly ass)
        {
            var m = new List<MethodInfo>();
            var types = ass.GetTypes();
            for (var i = 0; i < types.Length; i++)
                m.AddRange(types[i].GetMethods());

            return m.ToArray();
        }

        public static object CallMethodFromType(Type t, string methodName, params object[] o)
        {
            var mInfo = t.GetMethod(methodName);
            var parameters = mInfo.GetParameters();

            return mInfo.Invoke(Activator.CreateInstance(t, null), parameters.Length == 0 ? null : o);
        }

        public static object CallMethodFromFile(string dllFilePath, string typeName, string methodName, params object[] o)
        {
            var t = Assembly.LoadFile(dllFilePath).GetType(typeName);
            var mInfo = t.GetMethod(methodName);
            var parameters = mInfo.GetParameters();

            return mInfo.Invoke(Activator.CreateInstance(t, null), parameters.Length == 0 ? null : o);
        }
    }
}
