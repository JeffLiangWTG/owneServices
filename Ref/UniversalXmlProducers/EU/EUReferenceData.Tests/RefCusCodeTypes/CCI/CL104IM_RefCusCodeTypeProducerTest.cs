using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class CL104IM_RefCusCodeTypeProducerTest : RefCusCodeTypeProducerAbstractTest<CL104IM_RefCusCodeTypeProducer>
	{
		protected override string CodeType => "104IM";

		protected override string Description => "CCI Method Of Payment";

		protected override bool ReadOnly => true;

		protected override byte MaxLength => 1;

		protected override RefCusCodeTypeProducer Producer => new CL104IM_RefCusCodeTypeProducer();

		protected override string expectedOutputFileName => "RefCusCodeTypeZZ_EUN_104IM.xml";
	}
}
