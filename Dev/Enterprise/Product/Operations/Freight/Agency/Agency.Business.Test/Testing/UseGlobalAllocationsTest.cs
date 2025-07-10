using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class UseGlobalAllocationsTest : TransactionedTestCase
	{
		[UseGlobalAllocations(true)]
		public void TestEnabled()
		{
			AssertEquals(true, FreightConfigurationRegistry.Instance.UseGlobalAllocations.Value);
		}

		[UseGlobalAllocations(false)]
		public void TestDissabled()
		{
			AssertEquals(false, FreightConfigurationRegistry.Instance.UseGlobalAllocations.Value);
		}
	}
}
