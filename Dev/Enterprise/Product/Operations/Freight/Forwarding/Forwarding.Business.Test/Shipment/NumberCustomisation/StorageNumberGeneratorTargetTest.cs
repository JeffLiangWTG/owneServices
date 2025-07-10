using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class StorageNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestParameters()
		{
			Set(FreightDataRegistry.Instance.StorageNumberCustomisation, "SNC");
			NumberGeneratorContext context = new NumberGeneratorContext();
			NumberGeneratorTarget target = new StorageNumberGeneratorTarget();
			target.Context = context;

			AssertCustomisation("Should find the StorageNumberCustomisation", "SNC", target.NumberCustomisation);
			AssertLocation(FreightDataRegistry.Instance.StorageNumberCustomisation, target.NumberCustomisationLocation);
			AssertEquals(StorageMainSchema.SM_PhysicalLocation.MaxLength, target.MaxLength);
			AssertEquals("storage number", target.Name);
		}
	}
}
