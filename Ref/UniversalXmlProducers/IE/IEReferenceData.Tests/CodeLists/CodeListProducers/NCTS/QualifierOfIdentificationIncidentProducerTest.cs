using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class QualifierOfIdentificationIncidentProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "NCTS.Expected_QualifierOfIdentificationIncident";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new QualifierOfIdentificationIncident();
	}
}
