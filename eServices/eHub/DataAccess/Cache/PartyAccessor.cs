using System;
using CargoWise.eHub.DataAccess.Integration;

namespace CargoWise.eHub.DataAccess.Cache
{
	[Serializable]
	class PartyAccessor : IPartyAccessor
	{
		private const string PARTY_ACCESSOR_CACHE_KEY = "CargoWise.eHub.DataAccess.Cache.PartyAccessor, Version=3.0.0.0";

		#region IPartyAccessor Members

		public bool ClientExists(string id)
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.ClientExists(id);
		}

		public void InsertClientEntry(string clientID, string friendlyName, string email, Guid ediProdOrgHeaderPK)
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			sqlAccessor.InsertClientEntry(clientID, friendlyName, email, ediProdOrgHeaderPK);
		}

		public void InsertClientAndClientSystem(string clientID, string friendlyName, string email, Guid ediProdOrgHeaderPK, string systemCategory = "Enterprise")
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			sqlAccessor.InsertClientAndClientSystem(clientID, friendlyName, email, ediProdOrgHeaderPK, systemCategory);
		}

		public ClientDetails GetEdiProdClientDetailsForSystem(string clientID)
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.GetEdiProdClientDetailsForSystem(clientID);
		}


		public ClientDetails GetClientDetails(string clientID)
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.GetClientDetails(clientID);
		}

		public void UpdateEmailAddress(string clientID, ClientDetails clientDetails)
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			sqlAccessor.UpdateEmailAddress(clientID, clientDetails);
		}

		#endregion

		public string GetClientIDFromAirPIMA(string PIMAAddress, string serviceProviderID)
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.GetClientIDFromAirPIMA(PIMAAddress, serviceProviderID);
		}

		public string GetClientIDFromAirlineCode(string airlineCode, string serviceProviderID)
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.GetClientIDFromAirlineCode(airlineCode, serviceProviderID);
		}

		public string GetClientIDFromAS2Code(string code)
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.GetClientIDFromAS2Code(code);
		}

		public string GetClientIDFromEmail(string emailName, string emailAddress)
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.GetClientIDFromEmail(emailName, emailAddress);
		}

		public string GetClientIDFromClientPIMA(string clientPIMA, string serviceProviderID)
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.GetClientIDFromClientPIMA(clientPIMA, serviceProviderID);
		}

		public bool IsCW1System(string clientID)
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.IsCW1System(clientID);
		}
		
		public bool IsUnrestrictedClient(string clientID)
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.IsUnrestrictedClient(clientID);
		}

		public bool IsPermitInboxRecipient(string clientID, bool useCache = true)
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.IsPermitInboxRecipient(clientID, useCache);
		}

		public bool IsLegacyXmlAllowedClient(string clientID)
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.IsLegacyXmlAllowedClient(clientID);
		}

		public bool IsXHubSystem(string clientID)
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.IsXHubSystem(clientID);
		}

		public bool IsXHSystem(string clientID)
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.IsXHSystem(clientID);
		}

		public string GetClientIDFromClientAWB(string clientAWB, string serviceProviderID)
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.GetClientIDFromClientAWB(clientAWB, serviceProviderID);
		}

		public string GetAirlineCodeFromPrefix(string prefix)
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.GetAirlineCodeFromPrefix(prefix);
		}

		public int GetClientFromAirPIMACount(string PIMAAddress, string serviceProviderID)
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.GetClientFromAirPIMACount(PIMAAddress, serviceProviderID);
		}

		public string GetClientSystemIDFromClientSystemRegistration(string registrationTypeID, string registrationCode)
		{
			var sqlAccessor = DataAccessFactories.NewPartyAccessorInstance(PARTY_ACCESSOR_CACHE_KEY);
			return sqlAccessor.GetClientSystemIDFromClientSystemRegistration(registrationTypeID, registrationCode);
		}
	}
}
