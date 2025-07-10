using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS
{
	public class AuthorisationTypeDeparture : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.NCTS;

		public string Code => Constants.NctsCodeTypes.AuthorisationType;

		public string NameInFile => "CL235 – CL Authorisation Type Departure";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => @"([A-Z0-9]{4})";

		public override bool AllowCombination => true;
	}
}
