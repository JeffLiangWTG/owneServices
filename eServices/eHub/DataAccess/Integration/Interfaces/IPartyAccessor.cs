using System;

namespace CargoWise.eHub.DataAccess.Integration
{
	public interface IPartyAccessor
	{
		bool ClientExists(string id);
		void InsertClientEntry(string clientID, string friendlyName, string email, Guid ediProdOrgHeaderPK);
		ClientDetails GetEdiProdClientDetailsForSystem(string clientID);
		ClientDetails GetClientDetails(string clientID);
		void UpdateEmailAddress(string clientID, ClientDetails clientDetails);
		string GetClientIDFromAirPIMA(string PIMAAddress, string serviceProviderID);
		int GetClientFromAirPIMACount(string clientPIMA, string serviceProviderID);
		string GetClientIDFromAirlineCode(string airlineCode, string serviceProviderID);
		string GetClientIDFromAS2Code(string code);
		string GetClientIDFromEmail(string emailName, string emailAddress);
		string GetClientIDFromClientPIMA(string clientPIMA, string serviceProviderID);
		string GetClientIDFromClientAWB(string clientAWB, string serviceProviderID);
		string GetAirlineCodeFromPrefix(string prefix);
		void InsertClientAndClientSystem(string clientID, string friendlyName, string email, Guid ediProdOrgHeaderPK, string systemCategory = "Enterprise");
		bool IsCW1System(string clientID);
		bool IsUnrestrictedClient(string clientID);
		bool IsPermitInboxRecipient(string clientID, bool useCache = true);
		bool IsLegacyXmlAllowedClient(string clientID);
		bool IsXHubSystem(string clientID);
		bool IsXHSystem(string clientID);
		string GetClientSystemIDFromClientSystemRegistration(string registrationTypeID, string registrationCode);
	}
}
