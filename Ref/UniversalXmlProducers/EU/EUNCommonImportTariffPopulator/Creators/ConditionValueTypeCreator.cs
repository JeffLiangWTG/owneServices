using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class ConditionValueTypeCreator : IConditionValueTypeCreator
	{
		public string Get(measureCondition condition)
		{
			switch (condition.conditionCodeId)
			{
				case "R":
				case "U":
					return "FRM";
				case "E":
				case "I":
					if (!condition.WithCertificate())
					{
						return "FRM";
					}
					else if (condition.CertificateOfSUP())
					{
						return "SUP";
					}
					else
					{
						return "SNR";
					}
				case "A":
					if (!condition.WithCertificate())
					{
						return "INF";
					}
					else if (condition.CertificateOfSUP())
					{
						return "SUP";
					}
					else
					{
						return "SNR";
					}
				default:
					if (condition.WithCertificate())
					{
						if (condition.CertificateOfSUP())
						{
							return "SUP";
						}
						else
						{
							return "SNR";
						}
					}
					else
					{
						return string.Empty;
					}
			}
		}
	}
}
