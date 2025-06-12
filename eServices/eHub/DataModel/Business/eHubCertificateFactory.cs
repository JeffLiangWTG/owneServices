using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using CargoWise.eHub.DataModel.eHubTransactions;

namespace CargoWise.eHub.DataModel.Business
{
	public class eHubCertificateFactory
	{
		public static eHubCertificate CreateeHubCertificate(string containerType, byte[] binaryContainer, string id,
			string password, string category, eHubClientSystem clientSystem)
		{
			var cert = new eHubCertificate
			{
				CE_BinaryContainer = binaryContainer,
				CE_ID = id,
				CE_ContainerType = containerType,
				CE_Password = password,
				CE_AddedUTC = DateTime.UtcNow,
				eHubClientSystem = clientSystem,
				CE_Category = category
			};
			switch (containerType)
			{
				case ContainerType.PKCS12:
					var importedCert = ImportPKCS12Certificate(binaryContainer, password);
					cert.CE_ValidFromUTC = importedCert.NotBefore.ToUniversalTime();
					cert.CE_ValidToUTC = importedCert.NotAfter.ToUniversalTime();
					cert.CE_Thumbprint = importedCert.Thumbprint;
					break;
				default:
					throw new ArgumentException("Unexpected Container Type: " + containerType);
			}
			return cert;
		}

		private static X509Certificate2 ImportPKCS12Certificate(byte[] certificateBinary, string password)
		{
			var certs = new X509Certificate2Collection();
			certs.Import(certificateBinary, password, X509KeyStorageFlags.UserKeySet);
			return certs.Cast<X509Certificate2>().LastOrDefault();
		}
	}

	public static class ContainerType
	{
		public const string PKCS12 = "pkcs12-binary";
	}
}
