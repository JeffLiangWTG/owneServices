using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS.Tests
{
	sealed class AdditionalReferenceTypeTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AIS;
		protected override string ExpectedCode => "AR44I";
		protected override string ExpectedNameInFile => "CL380 - Additional reference type";
		protected override string ExpectedTableTitleInFile => "Code Name / description";
		protected override string ExpectedCodeFormattingRegularExpression => "([A-Z][0-9]{3})";
		protected override bool ExpectedAllowCombination => true;
		protected override bool ExpectedIsPublished => true;
		protected override IRevenueCodeListDetails GetCodeListDetails() => new AdditionalReference();
	}
}
