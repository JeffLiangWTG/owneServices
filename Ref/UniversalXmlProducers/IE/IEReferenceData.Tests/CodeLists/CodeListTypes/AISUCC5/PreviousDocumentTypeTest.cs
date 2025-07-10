using CargoWise.RefDbRepo.IEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5.Tests
{
	class PreviousDocumentTypeTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		[Test]
		public void IDoNotRegexEscapeTableTitleInPdf()
		{
			Assert.That(CodeListDetails, Is.AssignableTo(typeof(IDoNotRegexEscapeTableTitleInPdf)));
		}
		protected override ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AISUCC5;

		protected override string ExpectedCode => "DC40I";

		protected override string ExpectedNameInFile => "Previous document type";

		protected override string ExpectedTableTitleInFile => "";

		protected override string ExpectedCodeFormattingRegularExpression => "([0-9]{3})|([A-Z]{3})|([A-Z]{2}[0-9])|([A-Z][0-9][A-Z])";

		protected override bool ExpectedAllowCombination => false;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new PreviousDocumentTypesDetails();
	}
}
