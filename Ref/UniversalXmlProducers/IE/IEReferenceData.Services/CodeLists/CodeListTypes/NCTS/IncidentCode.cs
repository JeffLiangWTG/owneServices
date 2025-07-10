using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS
{
	public class IncidentCode : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.NCTS;

		public string Code => Constants.NctsCodeTypes.IncidentCodeType;

		public string NameInFile => "CL019 – CL Incident Code";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => @"(\d)";
	}
}
