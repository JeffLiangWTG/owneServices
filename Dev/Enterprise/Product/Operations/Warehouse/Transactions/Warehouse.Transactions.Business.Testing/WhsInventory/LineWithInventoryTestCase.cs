using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestsSubclassesOf(typeof(ILineWithInventory))]
	abstract class LineWithInventoryTestCase : TestCaseWithFactory
	{
		#region TestCollectionsNotNull

		public void TestCollectionsNotNull()
		{
			var line = (ILineWithInventory)Factory.New(TestedTypeHelper.GetTestedType(GetType()));

			AssertNotNull(line.Inventory);
			TestCollectionsNotNullCore();
		}

		protected virtual void TestCollectionsNotNullCore()
		{
		}

		#endregion

		#region TestProductPK_MatchesProduct

		public void TestProductPK_MatchesProduct()
		{
			var part = Factory.New<OrgSupplierPart>();
			var line = (ILineWithInventory)Factory.New(TestedTypeHelper.GetTestedType(GetType()));

			SetProductPK(line, part);
			AssertEquals(part.PK, line.ProductPK);
			AssertEquals(part, line.Product.Parent);
		}

		protected abstract void SetProductPK(ILineWithInventory line, OrgSupplierPart part);

		#endregion
	}
}
