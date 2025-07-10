using System.Linq;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class EU15CodeListGenerator : CustomsCountryCodeListGeneratorBase
	{
		protected override string CodeType => "EU15";

		protected override bool IsValidForRequirement(string valueForRequirement) => valueForRequirement.Equals("2", System.StringComparison.Ordinal);

		protected override bool IsMatchingRequirements(string valueForRequirement, string code) => IsValidForRequirement(valueForRequirement) || EftaCountries.Contains(code) || OtherEuLikeCountries.Contains(code);
	}
}
