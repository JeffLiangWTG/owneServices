using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES
{
	public class AdditionalReferenceNational : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AES;

		public string Code => Constants.AESCodeTypes.AdditionalReferenceType;

		public string NameInFile => "CL380N - CL Additional Reference Type (National)";

		public string CodeFormattingRegularExpression => "([0-9][A-Z0-9]{3})";

		public string TableTitleInFile => "Code Name / description ";

		public override bool AllowCombination => true;
	}
}
