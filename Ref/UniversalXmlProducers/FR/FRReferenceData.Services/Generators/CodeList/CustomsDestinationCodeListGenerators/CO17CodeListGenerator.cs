namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class CO17CodeListGenerator : CustomsCountryCodeListGeneratorBase
	{
		protected override string CodeType => "CO17";

		protected override bool IsValidForRequirement(string valueForRequirement)
		{
			return valueForRequirement.Equals("5", System.StringComparison.Ordinal);
		}

		protected override bool IsMatchingRequirements(string valueForRequirement, string code) => IsValidForRequirement(valueForRequirement);
	}
}
