using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace CargoWise.eHub.DataAccess.Integration
{
    public static class TypeCache
    {
        public static Type Retrieve(string typeKey)
        {
			if (!TypeCache.Types.ContainsKey(typeKey))
			{
				CacheType(typeKey);
			}
			return Types[typeKey];
        }

        private static Dictionary<string, Type> _types;
        private static Dictionary<string, Type> Types
        {
            get
            {
                if (_types == null)
                {
                    CreateTypesCache();
                }
                return _types;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void CreateTypesCache()
        {
            if (_types == null)
            {
                Dictionary<string, Type> types = new Dictionary<string, Type>();
                _types = types;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
		private static void CacheType(string typeKey)
        {
            if (!Types.ContainsKey(typeKey))
            {
				string accessorTypeName = typeKey;
				accessorTypeName = ConfigurationManager.AppSettings[typeKey];

				if (String.IsNullOrEmpty(accessorTypeName))
						throw new ConfigurationErrorsException(String.Format("DAL {0} could not be found in .config file.", typeKey));

                Type accessorType = Type.GetType(accessorTypeName, true);
                Types.Add(typeKey, accessorType);
            }
        }
    }
}
