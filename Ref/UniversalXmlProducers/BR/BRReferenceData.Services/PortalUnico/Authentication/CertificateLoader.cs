using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public static class CertificateLoader
	{
		public static byte[] LoadCertificate()
		{
			var cert = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Services.PortalUnico.Authentication.Certificate.CertificateRefDataRepo.pfx");
			using (MemoryStream ms = new MemoryStream())
			{
				cert?.CopyTo(ms);
				return ms.ToArray();
			}
		}
	}
}
