using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgSupplierPartBarcodeLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestProductUQList

		public void TestProductUQList()
		{
			AssertNotNull(Lookups.ProductUQList);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Parent = Factory.New<OrgSupplierPartBarcode>();
			Lookups = Parent.Lookups;
		}

		OrgSupplierPartBarcode Parent;
		OrgSupplierPartBarcodeLookups Lookups;

		#endregion
	}
}
