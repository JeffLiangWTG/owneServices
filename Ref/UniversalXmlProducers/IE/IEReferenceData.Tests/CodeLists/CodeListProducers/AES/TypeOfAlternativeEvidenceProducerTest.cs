using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	class TypeOfAlternativeEvidenceProducerTest : RevenueCodeListProducerAbstractTest
	{
		protected override string ExpectedTestFileName => "Expected_TypeOfAlternativeEvidence";
		protected override IRevenueCodeListDetails GetCodeListDetails() => new TypeOfAlternativeEvidence();
	}
}
