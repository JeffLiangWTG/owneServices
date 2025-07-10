using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS.Tests
{
	class AuthorisationCodeTypeTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override IEReferenceData.Services.ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AIS;

		protected override string ExpectedCode => "AUTH";

		protected override string ExpectedNameInFile => "CL605 - Authorisation Type";

		protected override string ExpectedTableTitleInFile => "Code Name / description ";

		protected override string ExpectedCodeFormattingRegularExpression => "([A-Z][0-9]{3})";

		protected override bool ExpectedAllowCombination => false;

		protected override bool ExpectedIsPublished => true;

		protected override UpdateType ExpectedUpdateType => UpdateType.Deletion;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new AuthorisationCodeTypesDetails();
	}
}
