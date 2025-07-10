using System.Linq;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class IM15CodeListGenerator : CustomsCountryCodeListGeneratorBase
	{
		protected override string CodeType => "IM15";

		protected override bool IsValidForRequirement(string valueForRequirement)
		{
			switch (valueForRequirement)
			{
				case "1":
				case "3":
				case "4":
				case "5":
					return true;
				default:
					return false;
			}
		}

		protected override bool IsMatchingRequirements(string valueForRequirement, string code) => IsValidForRequirement(valueForRequirement) && !EftaCountries.Contains(code) && !OtherEuLikeCountries.Contains(code);
	}
}
