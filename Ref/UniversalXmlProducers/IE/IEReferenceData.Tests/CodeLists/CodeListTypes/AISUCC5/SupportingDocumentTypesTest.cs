using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5.Tests
{
	class SupportingDocumentTypesTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		[Test]
		public void IDoNotRegexEscapeTableTitleInPdf()
		{
			Assert.That(CodeListDetails, Is.AssignableTo(typeof(IDoNotRegexEscapeTableTitleInPdf)));
		}

		protected override IEReferenceData.Services.ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AISUCC5;

		protected override string ExpectedCode => "DC44I";

		protected override string ExpectedNameInFile => "Common documents type (TARIC)";

		protected override string ExpectedTableTitleInFile => "";

		protected override string ExpectedCodeFormattingRegularExpression => "([0-9][A-Z][0-9]{2})|([0-9][A-Z]{3})|([A-Z][0-9]{3})";

		protected override bool ExpectedAllowCombination => false;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new SupportingDocumentTypesDetails();
	}
}
