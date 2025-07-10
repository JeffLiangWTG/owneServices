using System;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class RejectionCodeDepartureExportProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "NCTS.Expected_RejectionCodeDepartureExport";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new RejectionCodeDepartureExport();
	}
}
