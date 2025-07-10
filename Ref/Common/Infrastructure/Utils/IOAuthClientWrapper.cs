using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.Common.Utils;

public interface IOAuthClientWrapper
{
	Task<string> GetClientAccessTokenAsync(string endpoint, RSA privateKey, X509Certificate2 cert, string clientIdentifier, string serviceIdentifier);
}
