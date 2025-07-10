using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ConsolNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestParameters()
		{
			Set(FreightConfigurationRegistry.Instance.ConsolNumberCustomisation, "CON");

			ConsolNumberGeneratorTarget target = new ConsolNumberGeneratorTarget();
			target.Context = new NumberGeneratorContext();

			AssertCustomisation("Should find the ConsolNumberCustomisation", "CON", target.NumberCustomisation);
			AssertLocation(FreightConfigurationRegistry.Instance.ConsolNumberCustomisation, target.NumberCustomisationLocation);
			AssertEquals(JobConsolSchema.JK_UniqueConsignRef.MaxLength, target.MaxLength);
			AssertEquals("consol number", target.Name);
		}
	}
}
