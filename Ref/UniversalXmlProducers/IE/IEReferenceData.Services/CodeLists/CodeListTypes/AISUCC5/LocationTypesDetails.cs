using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5
{
	public class LocationTypesDetails : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AISUCC5;

		public string Code => Constants.AISCodeTypes.LocationType;

		public string NameInFile => "Location type";

		public string CodeFormattingRegularExpression => "([A-Z]{1})";

		public string TableTitleInFile => "";
	}
}
