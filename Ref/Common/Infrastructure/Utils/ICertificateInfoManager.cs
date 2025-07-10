using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.Common.Utils;

public interface ICertificateInfoManager : IDisposable
{
	RSA GetPrivateKey();
	X509Certificate2 GetCertificate();
	bool IsCertNearToExpire();
	Task RenewalCertificateAsync(string clientIdentifier, string accessToken);
}
