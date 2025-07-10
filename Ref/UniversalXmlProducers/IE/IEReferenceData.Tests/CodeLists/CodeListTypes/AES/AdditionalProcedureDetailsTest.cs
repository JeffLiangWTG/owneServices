using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.AES.Tests
{
	class AdditionalProcedureDetailsTest : RevenueCodeListDetailsTest
	{
		protected override ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AES;

		protected override string ExpectedCode => "CPDC";

		protected override string ExpectedNameInFile => "CL102 – CL Additional Procedure";

		protected override string ExpectedTableTitleInFile => "Code Name / description";

		protected override string ExpectedCodeFormattingRegularExpression => "[A-Z0-9]{3}";

		protected override bool ExpectedAllowCombination => true;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new AdditionalProcedureDetails();
	}
}
