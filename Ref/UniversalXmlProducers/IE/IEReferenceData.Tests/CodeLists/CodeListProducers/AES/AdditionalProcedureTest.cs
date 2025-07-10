using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.AES.Tests
{
	internal class AdditionalProcedureTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "Expected_AdditionalProcedures";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new AdditionalProcedureDetails();
	}
}
