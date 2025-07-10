using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class CL214IM_RefCusCodeTypeProducerTest : RefCusCodeTypeProducerAbstractTest<CL214IM_RefCusCodeTypeProducer>
	{
		protected override string CodeType => "214IM";

		protected override string Description => "CCI Previous Document Type (UCC6 Import)";

		protected override bool ReadOnly => true;

		protected override byte MaxLength => 4;

		protected override RefCusCodeTypeProducer Producer => new CL214IM_RefCusCodeTypeProducer();

		protected override string expectedOutputFileName => "RefCusCodeTypeZZ_EUN_214IM.xml";
	}
}
