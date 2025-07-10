using CargoWise.RefDbRepo.IEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS.Tests
{
	class PreviousDocumentTypeTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		[Test]
		public void IDoNotRegexEscapeTableTitleInPdf()
		{
			Assert.That(CodeListDetails, Is.AssignableTo(typeof(IDoNotRegexEscapeTableTitleInPdf)));
		}
		protected override ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AIS;

		protected override string ExpectedCode => "DC40I";

		protected override string ExpectedNameInFile => "CL214 - Previous document type";

		protected override string ExpectedTableTitleInFile => "Code Name / description ";

		protected override string ExpectedCodeFormattingRegularExpression => "([A-Z][0-9]{3})|([A-Z]{4})";

		protected override bool ExpectedAllowCombination => false;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new PreviousDocumentTypesDetails();
	}
}
