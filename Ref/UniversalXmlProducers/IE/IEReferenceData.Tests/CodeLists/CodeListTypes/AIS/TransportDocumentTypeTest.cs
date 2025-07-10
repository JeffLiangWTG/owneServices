using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS.Tests
{
	class TransportDocumentTypeTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AIS;

		protected override string ExpectedCode => "TD44I";

		protected override string ExpectedNameInFile => "CL754 - Transport document type";

		protected override string ExpectedTableTitleInFile => "Code Name / description";

		protected override string ExpectedCodeFormattingRegularExpression => "([A-Z][0-9]{3})|([0-9][A-Z][0-9]{2})";

		protected override bool ExpectedAllowCombination => false;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new TransportDocumentTypeDetails();
	}
}
