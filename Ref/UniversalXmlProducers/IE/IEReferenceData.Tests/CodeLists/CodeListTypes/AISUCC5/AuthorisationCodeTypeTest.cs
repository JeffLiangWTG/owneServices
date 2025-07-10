using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5.Tests
{
	class AuthorisationCodeTypeTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override IEReferenceData.Services.ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AISUCC5;

		protected override string ExpectedCode => "AUTH";

		protected override string ExpectedNameInFile => "Authorisation type code";

		protected override string ExpectedTableTitleInFile => "";

		protected override string ExpectedCodeFormattingRegularExpression => "([A-Z]{3,4})|([A-Z]{2}[0-9])";

		protected override bool ExpectedAllowCombination => false;

		protected override bool ExpectedIsPublished => true;

		protected override UpdateType ExpectedUpdateType => UpdateType.Deletion;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new AuthorisationCodeTypesDetails();
	}
}
