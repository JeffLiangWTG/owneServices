using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	[TestedType(typeof(LTConsignmentConsolidation))]
	class LTConsignmentConsolidationTest : NonPersistentBusinessObjectTestCase
	{
		#region TestJobNumber
		public void TestJobNumber()
		{
			AssertEquals("Non persistent consol", new LTConsignmentConsolidation().JobNumber);
		}
		#endregion
	}
}
