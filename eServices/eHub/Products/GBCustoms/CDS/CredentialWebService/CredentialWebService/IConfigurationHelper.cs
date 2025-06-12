using System;
using System.Data.SqlClient;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebService
{
	public interface IConfigurationHelper
	{
		void AddOrUpdateAuthorisationToken(string naturalKey, string authorisationToken, string uri, DateTime issuedUtc, DateTime expiryUtc);
		void NotifyCW1(string naturalKey);
		void AddOrUpdateAccessToken(string naturalKey, GBCustomsResponse response);
		void ProcessResponse(string naturalKey, RequestType requestType, GBCustomsResponse response, string clientID);
		void ProcessException(string naturalKey, string errorDescription, RequestType requestType, string clientID);
		bool DoesAuthorisationTokenAlreadyExist(string naturalKey);
		bool IsLicenceProduction(string naturalKey);
		string GetAccessToken(string naturalKey);
		void UpdateAuthorisationTokenStatus(string naturalKey, byte status);
		bool IsTokenRefreshed(string naturalKey, string currentAccessToken, int refreshedMinutes = 30);
		string FindRecipient(string key);
		SqlConnection GeteHubTransactionsConnection();
	}
}
