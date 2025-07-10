using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5.Tests
{
	class AdditionalProcedureDetailsTest : RevenueCodeListDetailsTest
	{
		protected override ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AISUCC5;

		protected override string ExpectedCode => "CPDC";

		protected override string ExpectedNameInFile => "Additional procedure";

		protected override string ExpectedTableTitleInFile => "";

		protected override string ExpectedCodeFormattingRegularExpression => "[A-Z0-9]{3}";

		protected override bool ExpectedAllowCombination => true;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new AdditionalProcedureDetails();
	}
}
