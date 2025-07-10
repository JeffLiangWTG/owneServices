using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemUNDGTotalsView))]
	class WhsItemUNDGTotalsViewTest : WhsEnvBusinessObjectTestCase
	{
		public void TestCanDelete()
		{
			AssertEquals("Not allowed to delete dbo.WhsItemUNDGTotalsView", false, GetNewBusinessObject().CanDelete);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var warehouse = Helper.CreateTRWWarehouse("AAA");
			var undgLimit = Helper.CreateWhsUNDGLimit(warehouse, "0004a");
			Factory.Save();
			var result = Factory.LoadTop1<WhsItemUNDGTotalsView>(new ZQuery());
			AssertNotNull("Precondition:", result);
			return result;
		}

		protected override bool IsDeleteSupported()
		{
			return false;
		}
	}
}
