using System.Linq;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class CO15CodeListGenerator : CustomsCountryCodeListGeneratorBase
	{
		protected override string CodeType => "CO15";

		protected override bool IsValidForRequirement(string valueForRequirement) => valueForRequirement.Equals("5", System.StringComparison.Ordinal);

		protected override bool IsMatchingRequirements(string valueForRequirement, string code) => IsValidForRequirement(valueForRequirement);
	}
}
