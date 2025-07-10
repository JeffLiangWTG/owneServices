using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class CL054_RefCusCodeTypeProducerTest : RefCusCodeTypeProducerAbstractTest<CL054_RefCusCodeTypeProducer>
	{
		protected override string CodeType => "CL054";

		protected override string Description => "NCTS Query Identifier";

		protected override bool ReadOnly => true;

		protected override byte MaxLength => 1;

		protected override RefCusCodeTypeProducer Producer => new CL054_RefCusCodeTypeProducer();

		protected override string expectedOutputFileName => "RefCusCodeTypeZZ_EUN_CL054.xml";
	}
}
