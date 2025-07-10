using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	class LegalBasisCodeTest : RevenueCodeListDetailsTest
	{
		protected override ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AIS;

		protected override string ExpectedCode => Constants.AISCodeTypes.LegalBasisCode;

		protected override string ExpectedNameInFile => "RL140 - Legal basis";

		protected override string ExpectedTableTitleInFile => "Code Description";

		protected override string ExpectedCodeFormattingRegularExpression => "([A-Z][0-9]{2})";

		protected override bool ExpectedAllowCombination => false;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new LegalBasisTypeDetails();
	}
}
