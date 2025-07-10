using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS
{
	public class AdditionalInformation : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.NCTS;

		public string Code => Constants.NctsCodeTypes.AdditionalInformationType;

		public string NameInFile => "CL239 – CL Additional Information";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => @"([0-9]{5})";
	}
}
