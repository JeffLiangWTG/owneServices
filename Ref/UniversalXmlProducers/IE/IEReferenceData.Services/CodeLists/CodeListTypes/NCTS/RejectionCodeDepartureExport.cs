using System;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS
{
	public class RejectionCodeDepartureExport : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.NCTS;

		public string Code => Constants.NctsCodeTypes.RejectionCodeDepartureExportType;

		public string NameInFile => "CL226 – CL Rejection Code Departure Export";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => "([0-9]{1,2})";
	}
}
