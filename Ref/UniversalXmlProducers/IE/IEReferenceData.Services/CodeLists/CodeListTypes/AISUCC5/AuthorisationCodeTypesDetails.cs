using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5
{
	public class AuthorisationCodeTypesDetails : RevenueCodeListDetails, IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
	{
		public ApplicationType ApplicationType => ApplicationType.AISUCC5;

		public string Code => Constants.AISCodeTypes.AuthorisationCodeType;

		public string NameInFile => "Authorisation type code";

		public string TableTitleInFile => "";

		public string CodeFormattingRegularExpression => "([A-Z]{3,4})|([A-Z]{2}[0-9])";

		public override UpdateType UpdateType => UpdateType.Deletion;
	}
}
