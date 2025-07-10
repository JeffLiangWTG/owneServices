using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS
{
	public class AuthorisationCodeTypesDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AIS;

		public string Code => Constants.AISCodeTypes.AuthorisationCodeType;

		public string NameInFile => "CL605 - Authorisation Type";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => "([A-Z][0-9]{3})";

		public override UpdateType UpdateType => UpdateType.Deletion;
	}
}
