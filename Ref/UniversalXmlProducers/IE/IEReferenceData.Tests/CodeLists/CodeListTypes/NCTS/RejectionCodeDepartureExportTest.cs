using System;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class RejectionCodeDepartureExportTest : CodeLists.Tests.RevenueCodeListDetailsTest
	{
		protected override ApplicationType ExpectedApplicationType => IEReferenceData.Services.ApplicationType.NCTS;

		protected override string ExpectedCode => "CL226";

		protected override string ExpectedNameInFile => "CL226 – CL Rejection Code Departure Export";

		protected override string ExpectedTableTitleInFile => "Code Name / description ";

		protected override string ExpectedCodeFormattingRegularExpression => "([0-9]{1,2})";

		protected override bool ExpectedAllowCombination => false;

		protected override bool ExpectedIsPublished => true;

		protected override IRevenueCodeListDetails GetCodeListDetails() => new RejectionCodeDepartureExport();
	}
}
