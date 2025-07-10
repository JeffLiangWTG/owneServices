using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5
{
	public class LocationIdentificationQualifiersDetails : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AISUCC5;

		public string Code => Constants.AISCodeTypes.LocationIdentificationQualifier;

		public string NameInFile => "Location identification qualifier";

		public string CodeFormattingRegularExpression => "([A-Z]{1})";

		public string TableTitleInFile => "";
	}
}

