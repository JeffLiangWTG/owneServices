using CargoWise.RefDbRepo.IEReferenceData.Services;
using Microsoft.VisualBasic;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS.Tests
{
	sealed class AddtionalInformationTypeTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.AIS;

		protected override string ExpectedCode => "AI44I";

		protected override string ExpectedNameInFile => "CL239 - Additional Information";

		protected override string ExpectedTableTitleInFile => "Code Subject / Additional Information ";

		protected override string ExpectedCodeFormattingRegularExpression => "[0-9]{5}|[A-Z]{1}[0-9]{4}";

		protected override bool ExpectedAllowCombination => false;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new AdditionalInformation();
	}
}
