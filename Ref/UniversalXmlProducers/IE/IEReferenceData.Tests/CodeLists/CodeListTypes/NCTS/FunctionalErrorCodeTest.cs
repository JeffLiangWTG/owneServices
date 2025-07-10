namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class FunctionalErrorCodeTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override IEReferenceData.Services.ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.NCTS;
		protected override string ExpectedCode => "CL180";
		protected override string ExpectedNameInFile => "CL180 – CL Functional Error Code";
		protected override string ExpectedTableTitleInFile => "Code Name / description ";
		protected override string ExpectedCodeFormattingRegularExpression => "([0-9]{2})";
		protected override bool ExpectedAllowCombination => true;
		protected override bool ExpectedIsPublished => true;
		protected override IRevenueCodeListDetails GetCodeListDetails() => new FunctionalErrorCode();
	}
}
