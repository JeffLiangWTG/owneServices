using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor
{
	public class CertificateData
	{
		public string CertificateNumber { get; }
		public string ConditionValueType { get; }

		public CertificateData(string certificateNumber, string certIdCheckBoxValue)
		{
			Argument.NotNullOrEmpty(certificateNumber, nameof(certificateNumber));
			Argument.NotNullOrEmpty(certIdCheckBoxValue, nameof(certIdCheckBoxValue));

			CertificateNumber = certificateNumber;
			ConditionValueType = certIdCheckBoxValue == "true" ? "SUP" : "SNR";
		}
	}
}
