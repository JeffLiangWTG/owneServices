using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using WTG.OpenIDConnect.Token;

namespace CargoWise.RefDbRepo.Common.Utils;

class OAuthClientWrapper : IOAuthClientWrapper
{
	public Task<string> GetClientAccessTokenAsync(string endpoint, RSA privateKey, X509Certificate2 cert, string clientIdentifier,
		string serviceIdentifier)
	{
		return OAuthClientAssertion.GetClientAccessTokenAsync(endpoint, privateKey, cert, clientIdentifier, serviceIdentifier);
	}
}
