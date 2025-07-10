namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class PreviousDocumentExportTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override IEReferenceData.Services.ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.NCTS;

		protected override string ExpectedCode => "DC40N";

		protected override string ExpectedNameInFile => "CL228 – CL Previous Document Export Type";

		protected override string ExpectedTableTitleInFile => "Code Name / description ";

		protected override string ExpectedCodeFormattingRegularExpression => "([A-Z0-9]{4})";

		protected override bool ExpectedAllowCombination => true;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new PreviousDocumentExport();
	}
}
