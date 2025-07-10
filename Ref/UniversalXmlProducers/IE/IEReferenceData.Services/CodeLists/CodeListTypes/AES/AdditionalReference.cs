using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES
{
	public class AdditionalReference : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AES;

		public string Code => Constants.AESCodeTypes.AdditionalReferenceType;

		public string NameInFile => "CL380 - CL Additional Reference Type";

		public string CodeFormattingRegularExpression => "([A-Z][0-9]{3})";

		public string TableTitleInFile => "Code Name / description ";

		public override bool AllowCombination => true;
	}
}
