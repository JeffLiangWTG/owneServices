using System.Linq;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class EU17CodeListGenerator : CustomsCountryCodeListGeneratorBase
	{
		protected override string CodeType => "EU17";

		protected override bool IsValidForRequirement(string valueForRequirement)
		{
			return valueForRequirement.Equals("2", System.StringComparison.Ordinal);
		}

		protected override bool IsMatchingRequirements(string valueForRequirement, string code) => IsValidForRequirement(valueForRequirement) || EftaCountries.Contains(code) || OtherEuLikeCountries.Contains(code);
	}
}
