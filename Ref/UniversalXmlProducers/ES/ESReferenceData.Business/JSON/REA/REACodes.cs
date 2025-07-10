using System.Collections.Generic;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class REATariff
	{
		public REATariff(string tariffCode)
		{
			TariffCode = tariffCode;
			REACodes = new List<REACodes>();
		}

		public string TariffCode { get; set; }
		public List<REACodes> REACodes { get; }
	}

	public class REACodes
	{
		public REACodes(string code, string aidAmountForDirectConsumption, string aidAmountForTransformation, string description)
		{
			Code = code;
			AidAmountForDirectConsumption = aidAmountForDirectConsumption;
			AidAmountForTransformation = aidAmountForTransformation;
			Description = description;
		}

		public string Code { get; set; }
		public string AidAmountForDirectConsumption { get; set; }
		public string AidAmountForTransformation { get; set; }
		public string Description { get; set; }
	}

	public static class REAConstants
	{
		public const string DirectConsumption = "AYD";
		public const string Transformation = "AYT";
		public const string REA = "REA";
	}
}
