namespace CargoWise.eHub.DataAccess.Integration
{
	public interface IRegistryAccessor
	{
		bool SelectRegistryIsProd(string clientId, string applicationCode, string name);
		string SelectRegistryValue(string clientId, string applicationCode, string name);
		void InsertMessageReference(string clientId, string applicationCode, string messageReference);
		string ResolveMessageReference(string reference, string applicationCode);
	}
}
