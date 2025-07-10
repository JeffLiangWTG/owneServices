using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace CargoWise.RefDbRepo.ESReferenceData.Services
{
	public static class CertificateConfig
	{
		public static X509Certificate2 GetCertificate() => new X509Certificate2(ReadCertificateFromEmbeddedResource(), "password");

		public static byte[] ReadCertificateFromEmbeddedResource()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var certificateStream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.ESReferenceData.Services.Certificate.escertificate2024.pfx");
			using (var memoryStream = new MemoryStream())
			{
				certificateStream.CopyTo(memoryStream);
				return memoryStream.ToArray();
			}
		}
	}
}
