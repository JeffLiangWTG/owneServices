using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MockJobDocAddressParentForTest : JobDocAddressParentForTesting
	{
		public MockJobDocAddressParentForTest(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
