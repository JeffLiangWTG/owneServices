namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5.Tests
{
	class AdditionalFiscalRefRoleCodeDetailsTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override IEReferenceData.Services.ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AISUCC5;

		protected override string ExpectedCode => "AFRC";

		protected override string ExpectedNameInFile => "Additional fiscal ref role code";

		protected override string ExpectedTableTitleInFile => "";

		protected override string ExpectedCodeFormattingRegularExpression => @"FR\d";

		protected override bool ExpectedAllowCombination => false;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new AdditionalFiscalRefRoleCodeDetails();
	}
}
