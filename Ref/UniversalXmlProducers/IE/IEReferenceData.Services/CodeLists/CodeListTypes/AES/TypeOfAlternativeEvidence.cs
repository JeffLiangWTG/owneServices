using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES
{
	public class TypeOfAlternativeEvidence : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.AES;

		public string Code => Constants.AESCodeTypes.TypeOfAlternativeEvidenceType;

		public string NameInFile => "CL170 – CL Type of Alternative Evidence";

		public string CodeFormattingRegularExpression => "(1[0-9]{1})";

		public string TableTitleInFile => "Code Name / description ";
	}
}
