using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS
{
	public class SpecificCircumstanceIndicatorCode : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.NCTS;

		public string Code => Constants.NctsCodeTypes.SpecificCircumstanceIndicatorCodeType;

		public string NameInFile => "CL296 – CL Specific Circumstance Indicator Code";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => @"([A-Z0-9]{3})";
	}
}

