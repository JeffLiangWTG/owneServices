using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class ControlTypeProducerTest : RevenueCodeListProducerAbstractTest
    {
        protected override string ExpectedTestFileName => "NCTS.Expected_ControlType";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new ControlType();
    }
}
