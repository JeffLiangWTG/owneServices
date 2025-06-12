using System;
using CargoWise.eHub.DataAccess.Integration;

namespace CargoWise.eHub.DataAccess.Sql
{
	[Serializable]
	class PartyAccessor : IPartyAccessor
	{
		public PartyAccessor() : this(new eServices.eHubDataAccess.Sql.PartyAccessor()) { }

		public PartyAccessor(eServices.eHubDataAccess.Integration.IPartyAccessor partyAccessor)
		{
			this.partyAccessor = partyAccessor;
		}

		private readonly eServices.eHubDataAccess.Integration.IPartyAccessor partyAccessor;

		public bool ClientExists(string id)
			=> partyAccessor.ClientExists(id);

		public void InsertClientEntry(string clientID, string friendlyName, string email, Guid ediProdOrgHeaderPK)
			=> partyAccessor.InsertClientEntry(clientID, friendlyName, email, ediProdOrgHeaderPK);

		public void InsertClientAndClientSystem(string clientID, string friendlyName, string email, Guid ediProdOrgHeaderPK, string systemCategory = "Enterprise")
			=> partyAccessor.InsertClientAndClientSystem(clientID, friendlyName, email, ediProdOrgHeaderPK, systemCategory);

		public ClientDetails GetClientDetails(string clientID)
			=> partyAccessor.GetClientDetails(clientID);

		public void UpdateEmailAddress(string clientID, ClientDetails clientDetails)
			=> partyAccessor.UpdateEmailAddress(clientID, clientDetails);

		public ClientDetails GetEdiProdClientDetailsForSystem(string clientID)
			=> partyAccessor.GetEdiProdClientDetailsForSystem(clientID);

		public string GetClientIDFromAirPIMA(string PIMAAddress, string serviceProviderID)
			=> partyAccessor.GetClientIDFromAirPIMA(PIMAAddress, serviceProviderID);

		public string GetClientIDFromAirlineCode(string airlineCode, string serviceProviderID)
			=> partyAccessor.GetClientIDFromAirlineCode(airlineCode, serviceProviderID);

		public string GetClientIDFromAS2Code(string code)
			=> partyAccessor.GetClientIDFromAS2Code(code);

		public string GetClientIDFromEmail(string emailName, string emailAddress)
			=> partyAccessor.GetClientIDFromEmail(emailName, emailAddress);

		public string GetClientIDFromClientPIMA(string clientPIMA, string serviceProviderID)
			=> partyAccessor.GetClientIDFromClientPIMA(clientPIMA, serviceProviderID);

		public bool IsCW1System(string clientID)
			=> partyAccessor.IsCW1System(clientID);
			
		public bool IsUnrestrictedClient(string clientID)
			=> partyAccessor.IsUnrestrictedClient(clientID);

		public bool IsPermitInboxRecipient(string clientID, bool useCache = true)
			=> partyAccessor.IsPermitInboxRecipient(clientID, useCache);

		public bool IsLegacyXmlAllowedClient(string clientID)
			=> partyAccessor.IsLegacyXmlAllowedClient(clientID);

		public bool IsXHubSystem(string clientID)
			=> partyAccessor.IsXHubSystem(clientID);

		public bool IsXHSystem(string clientID)
			=> partyAccessor.IsXHSystem(clientID);

		public string GetClientIDFromClientAWB(string clientAWB, string serviceProviderID)
			=> partyAccessor.GetClientIDFromClientAWB(clientAWB, serviceProviderID);

		public string GetAirlineCodeFromPrefix(string prefix)
			=> partyAccessor.GetAirlineCodeFromPrefix(prefix);

		public int GetClientFromAirPIMACount(string clientPIMA, string serviceProviderID)
			=> partyAccessor.GetClientFromAirPIMACount(clientPIMA, serviceProviderID);

		public string GetClientSystemIDFromClientSystemRegistration(string registrationTypeID, string registrationCode)
			=> partyAccessor.GetClientSystemIDFromClientSystemRegistration(registrationTypeID, registrationCode);
	}
}
