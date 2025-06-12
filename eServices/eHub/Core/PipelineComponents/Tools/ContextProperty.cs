using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
    /// <summary>
    /// Simplifies strongly typed access to context property names and namespaces for use when reading and writing
    /// property values.  The properties are cached to provide fast access to the name and namespace.
    /// </summary>
    /// <typeparam name="T">Context property to be queried</typeparam>
	public static class ContextProperty<T> where T : PropertyBase
	{
        private static Dictionary<Type, PropertyBase> _properties;

        public static string Name
        {
            get
            {
                return Instance.Name.Name;
            }
        }

        public static string Namespace
        {
            get
            {
                return Instance.Name.Namespace;
            }
        }

        private static PropertyBase Instance
        {
            get
            {
                if (!Properties.ContainsKey(typeof(T)))
                {
                    CacheProperty();
                }
                return Properties[typeof(T)];
            }
        }

        private static Dictionary<Type, PropertyBase> Properties
        {
            get
            {
                if (_properties == null)
                {
                    CreatePropertiesCache();
                }
                return _properties;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void CreatePropertiesCache()
        {
            if (_properties == null)
            {
                Dictionary<Type, PropertyBase> properties = new Dictionary<Type, PropertyBase>();
                _properties = properties;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void CacheProperty()
        {
            if (!Properties.ContainsKey(typeof(T)))
            {
                PropertyBase property = Activator.CreateInstance<T>() as PropertyBase;
                Properties.Add(typeof(T), property);
            }
        }
	}
}
