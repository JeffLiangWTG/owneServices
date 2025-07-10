using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	sealed class AdditionalProcedureTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "AIS.Expected_AdditionalProcedures";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new AdditionalProcedureDetails();
	}
}
