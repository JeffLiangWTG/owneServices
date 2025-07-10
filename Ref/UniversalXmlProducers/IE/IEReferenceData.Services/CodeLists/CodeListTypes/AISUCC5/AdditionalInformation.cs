using System;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5
{
	public class AdditionalInformation : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AISUCC5;

		public string Code => Constants.AISCodeTypes.AdditionalInformationType;

		public string NameInFile => "Additional information code";

		public string TableTitleInFile => "";

		public string CodeFormattingRegularExpression => "[0-9]{5}|[A-Z]{1}[0-9]{4}";
	}
}
