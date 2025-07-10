using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	[TestedType(typeof(WhsTransitDispatchConsol))]
	class WhsTransitDispatchConsolTest : NonPersistentBusinessObjectTestCase
	{
		#region TestJobNumber

		public void TestJobNumber()
		{
			AssertEquals("Non persistent consol", new WhsTransitDispatchConsol().JobNumber);
		}

		#endregion
	}
}
