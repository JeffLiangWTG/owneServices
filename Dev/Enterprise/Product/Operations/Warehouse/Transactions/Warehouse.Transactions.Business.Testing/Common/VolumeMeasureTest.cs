using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class VolumeMeasureTest : UnitOfMeasureTestCase<VolumeMeasure>
	{
		#region TestDecimalPlacesForRounding

		protected override int ExpectedDecimalPlacesForRounding => 3;

		#endregion

		#region TestGetQuantityFromProduct

		public void TestGetQuantityFromProduct()
		{
			IUnitOfMeasure measure = new VolumeMeasure("Volume", "M3");
			var part = Factory.New<OrgSupplierPart>();
			part.OP_Cubic = 43.5m;
			AssertEquals("Volume Quantity should come from OP_Cubic.", 43.5m, measure.GetQuantityFromProduct(part));
		}

		#endregion

		#region TestGetUQFromProduct

		public void TestGetUQFromProduct()
		{
			IUnitOfMeasure measure = new VolumeMeasure("Volume", "M3");
			var part = Factory.New<OrgSupplierPart>();
			part.OP_CubicUQ = "CC";
			AssertEquals("Volume UQ should come from OP_CubicUQ.", "CC", measure.GetUQFromProduct(part));
		}

		#endregion

		#region Implementation

		protected override VolumeMeasure GetNewMeasure(ZString name, ZString totalUQ) =>
			new VolumeMeasure(name, totalUQ);

		protected override VolumeMeasure GetNewMeasure(ZPropertyInfo totalQuantityInfo, ZString totalUQ) =>
			new VolumeMeasure(totalQuantityInfo, totalUQ);

		#endregion
	}
}
