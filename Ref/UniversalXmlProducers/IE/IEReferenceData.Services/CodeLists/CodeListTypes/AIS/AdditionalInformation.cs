using System;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS
{
	public class AdditionalInformation : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AIS;

		public string Code => Constants.AISCodeTypes.AdditionalInformationType;

		public string NameInFile => "CL239 - Additional Information";

		public string TableTitleInFile => "Code Subject / Additional Information ";

		public string CodeFormattingRegularExpression => "[0-9]{5}|[A-Z]{1}[0-9]{4}";
	}
}
