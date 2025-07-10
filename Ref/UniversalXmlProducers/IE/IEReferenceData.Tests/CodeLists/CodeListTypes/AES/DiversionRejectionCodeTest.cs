namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES.Tests
{
	class DiversionRejectionCodeTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override IEReferenceData.Services.ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AES;
		protected override string ExpectedCode => "CL046";
		protected override string ExpectedNameInFile => "CL046 - CL Diversion Rejection Code";
		protected override string ExpectedTableTitleInFile => "Code Name/description ";
		protected override string ExpectedCodeFormattingRegularExpression => "([0-9]{1,2})";
		protected override bool ExpectedAllowCombination => false;
		protected override bool ExpectedIsPublished => true;
		protected override IRevenueCodeListDetails GetCodeListDetails() => new DiversionRejectionCode();
	}
}
