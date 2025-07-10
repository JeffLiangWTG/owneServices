using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor
{
	public class RequirementData
	{
		public string RequirementType { get; }
		public string RequirementDescription { get; }

		public RequirementData(string type, string description)
		{
			Argument.NotNullOrEmpty(type, nameof(type));
			Argument.NotNullOrEmpty(description, nameof(description));

			RequirementType = type.Trim().Replace(":","");
			RequirementDescription = description.Trim();
		}
	}
}
