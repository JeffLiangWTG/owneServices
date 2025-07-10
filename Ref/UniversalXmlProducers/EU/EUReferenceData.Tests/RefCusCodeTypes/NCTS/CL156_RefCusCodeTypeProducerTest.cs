using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class CL156_RefCusCodeTypeProducerTest : RefCusCodeTypeProducerAbstractTest<CL156_RefCusCodeTypeProducer>
	{
		protected override string CodeType => "CL156";

		protected override string Description => "NCTS Role of Requester";

		protected override bool ReadOnly => true;

		protected override byte MaxLength => 1;

		protected override RefCusCodeTypeProducer Producer => new CL156_RefCusCodeTypeProducer();

		protected override string expectedOutputFileName => "RefCusCodeTypeZZ_EUN_CL156.xml";
	}
}
