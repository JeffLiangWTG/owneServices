using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class TransportNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestParameters()
		{
			var target = GetNumberGeneratorTargetCore();
			var registryItem = target.GetRegistryItemCore();
			Set(registryItem, "", "OSN");
			target.Context = new NumberGeneratorContext();

			AssertCustomisation("Should find the TransportBookingNumberCustomisation", "OSN", target.NumberCustomisation);
			AssertLocation(registryItem, target.NumberCustomisationLocation);
			AssertEquals(20, target.MaxLength);
		}

		protected abstract TransportNumberGeneratorTarget GetNumberGeneratorTargetCore();
	}
}
