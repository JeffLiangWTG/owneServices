using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CargoWise.eHub.DataModel.Business.Semantics
{
    public class eHubPortalSemanticsFactory
    {
        static Dictionary<string, Type> Cache = new Dictionary<string, Type>();

        static Type GetProvider(string product)
        {
            if (Cache.ContainsKey(product)) return Cache[product];
            var assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes().Where(t => t.IsClass))
            {
                var attributes = type.GetCustomAttributes(typeof(eHubPortalSemanticsProviderAttribute), false);

                foreach (eHubPortalSemanticsProviderAttribute attribute in attributes)
                {
                    if (attribute.Product == product)
                    {
                        Cache.Add(product, type);
                        return type;
                    }
                }
            }
            return null;
        }

        public static Dictionary<T, K> GetSemantics<T, K>(string product, string keyword)
        {
            var provider = GetProvider(product);
            if (provider == null) return GetSemantics<T, K>("Default", keyword);
            var fieldInfo = provider.GetField(keyword);
            return fieldInfo == null ? new Dictionary<T, K>() : (Dictionary<T, K>)fieldInfo.GetValue(provider);
        }

        public static T GetSemantics<T>(string product, string keyword) where T : new()
        {
	        var provider = GetProvider(product);
	        if (provider == null) return GetSemantics<T>("Default", keyword);
	        var fieldInfo = provider.GetField(keyword);
	        return fieldInfo == null ? new T() : (T)fieldInfo.GetValue(provider);
        }
	}
}
