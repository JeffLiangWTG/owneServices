using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.Receive
{
	[TestedType(typeof(WhsTransitReceiveConsol))]
	class WhsTransitReceiveConsolTest : NonPersistentBusinessObjectTestCase
	{
		#region TestJobNumber

		public void TestJobNumber()
		{
			AssertEquals("Non persistent consol", new WhsTransitReceiveConsol().JobNumber);
		}

		#endregion
	}
}
