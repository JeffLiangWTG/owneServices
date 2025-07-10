namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class AdditionalReferenceCodeTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override IEReferenceData.Services.ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.NCTS;

		protected override string ExpectedCode => "AR44N";

		protected override string ExpectedNameInFile => "CL380 – CL Additional Reference";

		protected override string ExpectedTableTitleInFile => "Code Name / description ";

		protected override string ExpectedCodeFormattingRegularExpression => "([A-Z0-9]{4})";

		protected override bool ExpectedAllowCombination => false;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new AdditionalReference();
	}
}
