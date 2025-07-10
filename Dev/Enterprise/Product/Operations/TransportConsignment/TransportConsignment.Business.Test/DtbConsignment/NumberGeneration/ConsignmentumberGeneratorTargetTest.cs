using System;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class ConsignmentumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestParameters()
		{
			var helper = new TransportConsignmentTestHelper(Factory);
			var target = new ConsignmentNumberGeneratorTarget(helper.CreateConsignment("LT001"));

			var registryItem = target.GetRegistryItem();
			Set(registryItem, "", "OSN");
			target.Context = new NumberGeneratorContext();

			AssertCustomisation("Should find the LandTransportNumberCustomisation", "OSN", target.NumberCustomisation);
			AssertLocation(registryItem, target.NumberCustomisationLocation);
			AssertEquals(20, target.MaxLength);
			AssertEquals("consignment number", target.Name);
			AssertEquals("Transport -> Land & Port Transport -> Land Transport -> Transport Consignment Number Format", target.NumberCustomisationLocation);
			AssertNotNull(target.NumberCustomisation);
			AssertEquals(15, target.NumberCustomisation.Elements.Count);
		}

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ConsignmentNumberGeneratorTarget(null));
		}
	}
}
