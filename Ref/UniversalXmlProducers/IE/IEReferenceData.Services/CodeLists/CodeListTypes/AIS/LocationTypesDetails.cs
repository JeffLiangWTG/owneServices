using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS
{
	public class LocationTypesDetails : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AIS;

		public string Code => Constants.AISCodeTypes.LocationType;

		public string NameInFile => "CL347 - Type of Location";

		public string CodeFormattingRegularExpression => "([A-Z]{1})";

		public string TableTitleInFile => "Code Name / description ";
	}
}
