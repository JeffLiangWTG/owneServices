using CargoWise.eHub.DataAccess.Integration;

namespace CargoWise.eHub.DataAccess.Sql
{
	class RegistryAccessor : IRegistryAccessor
	{
		public RegistryAccessor() : this(new eServices.eHubDataAccess.Sql.RegistryAccessor()) { }

		public RegistryAccessor(eServices.eHubDataAccess.Integration.IRegistryAccessor registryAccessor)
		{
			this.registryAccessor = registryAccessor;
		}

		private readonly eServices.eHubDataAccess.Integration.IRegistryAccessor registryAccessor;

		public string SelectRegistryValue(string clientId, string applicationCode, string name)
			=> registryAccessor.SelectRegistryValue(clientId, applicationCode, name);

		public void InsertMessageReference(string clientId, string applicationCode, string messageReference)
			=> registryAccessor.InsertMessageReference(clientId, applicationCode, messageReference);

		public string ResolveMessageReference(string reference, string applicationCode)
			=> registryAccessor.ResolveMessageReference(reference, applicationCode);

		public bool SelectRegistryIsProd(string clientId, string applicationCode, string name)
			=> registryAccessor.SelectRegistryIsProd(clientId, applicationCode, name);
	}
}
