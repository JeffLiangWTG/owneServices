using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.AIS.Tests
{
	class AdditionalProcedureDetailsTest : RevenueCodeListDetailsTest
	{
		protected override ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AIS;

		protected override string ExpectedCode => "CPDC";

		protected override string ExpectedNameInFile => "CL457 - Additional Procedure";

		protected override string ExpectedTableTitleInFile => "Code Name / description";

		protected override string ExpectedCodeFormattingRegularExpression => "([A-Z][0-9]{2})|([0]{3})|([0-9][A-Z][0-9])";

		protected override bool ExpectedAllowCombination => true;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new AdditionalProcedureDetails();
	}
}
