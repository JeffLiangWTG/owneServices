using System;
using CargoWise.eHub.DataAccess.Integration;

namespace CargoWise.eHub.DataAccess.Cache
{
	[Serializable]
	class RegistryAccessor : IRegistryAccessor
	{
		private const string REGISTRY_ACCESSOR_CACHE_KEY = "CargoWise.eHub.DataAccess.Cache.RegistryAccessor, Version=3.0.0.0";

		#region IRegistryAccessor Members

		public string SelectRegistryValue(string clientId, string applicationCode, string name)
		{
			var sqlAccessor = DataAccessFactories.NewRegistryAccessorInstance(REGISTRY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.SelectRegistryValue(clientId, applicationCode, name);
		}

		#endregion

		public void InsertMessageReference(string clientId, string applicationCode, string messageReference)
		{
			var sqlAccessor = DataAccessFactories.NewRegistryAccessorInstance(REGISTRY_ACCESSOR_CACHE_KEY);
			sqlAccessor.InsertMessageReference(clientId, applicationCode, messageReference);
		}

		public string ResolveMessageReference(string messageReference, string applicationCode)
		{
			var sqlAccessor = DataAccessFactories.NewRegistryAccessorInstance(REGISTRY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.ResolveMessageReference(messageReference, applicationCode);
		}

		#region IRegistryAccessor Members


		public bool SelectRegistryIsProd(string clientId, string applicationCode, string name)
		{
			var sqlAccessor = DataAccessFactories.NewRegistryAccessorInstance(REGISTRY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.SelectRegistryIsProd(clientId, applicationCode, name);
		}

		#endregion
	}
}
