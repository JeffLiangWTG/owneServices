namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES.Tests
{
	class RejectionReasonTypeTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override IEReferenceData.Services.ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AES;
		protected override string ExpectedCode => "CL560";
		protected override string ExpectedNameInFile => "CL560 - CL Rejection Reason Type";
		protected override string ExpectedTableTitleInFile => "Code Name / description ";
		protected override string ExpectedCodeFormattingRegularExpression => "([0-9]{3})";
		protected override bool ExpectedAllowCombination => false;
		protected override bool ExpectedIsPublished => true;
		protected override IRevenueCodeListDetails GetCodeListDetails() => new RejectionReasonType();
	}
}
