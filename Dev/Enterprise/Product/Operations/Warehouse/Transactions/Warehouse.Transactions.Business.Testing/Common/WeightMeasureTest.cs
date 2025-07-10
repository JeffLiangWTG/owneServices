using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WeightMeasureTest : UnitOfMeasureTestCase<WeightMeasure>
	{
		#region TestDecimalPlacesForRounding

		protected override int ExpectedDecimalPlacesForRounding => 2;

		#endregion

		#region TestGetQuantityFromProduct

		public void TestGetQuantityFromProduct()
		{
			IUnitOfMeasure measure = new WeightMeasure("Weight", "KG");
			var part = Factory.New<OrgSupplierPart>();
			part.OP_Weight = 22.3m;
			AssertEquals("Weight Quantity should come from OP_Weight.", 22.3m, measure.GetQuantityFromProduct(part));
		}

		#endregion

		#region TestGetUQFromProduct

		public void TestGetUQFromProduct()
		{
			IUnitOfMeasure measure = new WeightMeasure("Weight", "KG");
			var part = Factory.New<OrgSupplierPart>();
			part.OP_WeightUQ = "G";
			AssertEquals("Weight UQ should come from OP_WeightUQ.", "G", measure.GetUQFromProduct(part));
		}

		#endregion

		#region Implementation

		protected override WeightMeasure GetNewMeasure(ZString name, ZString totalUQ) =>
			new WeightMeasure(name, totalUQ);

		protected override WeightMeasure GetNewMeasure(ZPropertyInfo totalQuantityInfo, ZString totalUQ) =>
			new WeightMeasure(totalQuantityInfo, totalUQ);

		#endregion
	}
}
