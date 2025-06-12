using System;
using System.ServiceModel;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebService
{
	[ServiceContract(Namespace = "http://cargowise.com/ehub/products/gbcustoms")]
	public interface ICredentialWebService
	{
		[OperationContract(Action = "PersistAuthorisationToken")]
		Response PersistAuthorisationToken(string naturalKey, string authorisationToken, string uri, DateTime issuedUtc);

		[OperationContract(Action = "RequestAccessToken")]
		Response RequestAccessToken(string authorisationToken, string redirect_uri, string naturalKey);

		[OperationContract(Action = "RefreshAccessToken")]
		Response RefreshAccessToken(string accessToken, string refreshToken, string naturalKey);

		[OperationContract(Action = "RefreshAccessToken")]
		Response RequestAppWideAccessToken(string accessToken, string naturalKey, string scope);
	}
}
