using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS
{
	public class LocationIdentificationQualifiersDetails : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AIS;

		public string Code => Constants.AISCodeTypes.LocationIdentificationQualifier;

		public string NameInFile => "CL326 - Qualifier of the Location Identification";

		public string CodeFormattingRegularExpression => "([A-Z]{1})";

		public string TableTitleInFile => "Code Name / description ";
	}
}

