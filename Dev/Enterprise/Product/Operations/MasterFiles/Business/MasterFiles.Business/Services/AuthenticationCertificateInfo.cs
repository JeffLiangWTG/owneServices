using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CargoWise.SystemToSystemTrust;
using Enterprise.Registry.Business;
using WTG.IdentitySecurity;

namespace Enterprise.MasterFiles.Business
{
	public class AuthenticationCertificateInfo
	{
		const string DefaultDpsServerId = "1ae29a5d-8c05-4121-a540-90c9e75d5242";
		const string DefaultAvsServerId = "c34e06bf-4d88-483b-adf7-d650fdbdc62b";

		readonly string defaultServerId;
		readonly bool supportCertificateUtilised;

		public AuthenticationCertificateInfo(MDMProductCodes productCode)
		{
			if (productCode == MDMProductCodes.DPS)
			{
				AuthenticationInfo = OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS.Value;
				defaultServerId = DefaultDpsServerId;
			}
			else if (productCode == MDMProductCodes.AVS)
			{
				AuthenticationInfo = OrganisationRegistry.Instance.MDMSupportCertificateInfo;
				defaultServerId = DefaultAvsServerId;
			}
			else if (productCode == MDMProductCodes.BOL)
			{
				defaultServerId = OrganisationRegistry.Instance.EnableBoleroEHBLIntegration.Value.GetGalileoAudience();
			}

			if (string.IsNullOrEmpty(AuthenticationInfo?.ClientId))
			{
				AuthenticationInfo = SystemDataRegistry.Instance.SystemToSystemCertificate.Value;
			}
			else
			{
				supportCertificateUtilised = true;
			}

#pragma warning disable CS0618 // To be replaced with S2ST library once WI00771920 is implemented
			if (!(IsEmpty = string.IsNullOrEmpty(AuthenticationInfo.ClientId)))
			{
				Certificate = new X509Certificate2(AuthenticationInfo.CertificateBytes);
				PrivateKey = RSAKeyProvider.ImportPrivateKey(AuthenticationInfo.PrivateKey);
				ServerClientId = supportCertificateUtilised
					? AuthenticationInfo.OperationId
					: defaultServerId;
			}
#pragma warning restore CS0618 // To be replaced with S2ST library once WI00771920 is implemented
		}

		public ISystemToSystemTrustInfo AuthenticationInfo { get; }
		public string ServerClientId { get; }
		public string ClientId => AuthenticationInfo.ClientId;
		public string TenantId => AuthenticationInfo.TenantId;
		public X509Certificate2 Certificate { get; }
		public RSA PrivateKey { get; }
		public bool IsEmpty { get; }
	}
}
