using System.Collections.Generic;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	public static class CertificateOfOriginConstants
	{
		public static IEnumerable<string> DataContexts { get; } = new string[]
		{
			DataContext.CertificateOfOriginAANZFTA,
			DataContext.CertificateOfOriginNZCFTA,
			DataContext.CertificateOfOriginChAFTA,
			DataContext.CertificateOfOriginJAEPA,
			DataContext.CertificateOfOriginRCEP,
			DataContext.CertificateOfOriginNZ,
			DataContext.CertificateOfOriginCPTPP,
			DataContext.CertificateOfOriginTAFTA,
			DataContext.CertificateOfOriginPAFTA,
			DataContext.CertificateOfOriginAUKFTA,
			DataContext.CertificateOfOriginIAECTA,
			DataContext.CertificateOfOriginIACEPA,
			DataContext.CertificateOfOriginAUCONP,
			DataContext.CertificateOfOriginKAFTA,
			DataContext.CertificateOfOriginCOOUS
		};
	}
}
