using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	class OrgSecondaryPartBOMPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestComponents()
		{
			var mainProduct = Factory.New<OrgSupplierPart>();
			var secondaryPart = mainProduct.SecondaryParts.AddNew();
			var pivot = secondaryPart.ComponentUsages.AddNew();
			AssertType<OrgPartBOMCollection>(pivot.Lookups.Components);
			AssertEquals("Lookups is cached.", pivot.Lookups.Components, pivot.Lookups.Components);
			AssertEquals(0, pivot.Lookups.Components.Count);

			var bom = mainProduct.BillOfMaterials.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { bom }, pivot.Lookups.Components);
		}

		public void TestComponents_NoSecondaryProduct()
		{
			var pivot = Factory.New<OrgSecondaryPartBOMPivot>();
			AssertType<OrgPartBOMCollection>(pivot.Lookups.Components);
			AssertEquals("Lookups is cached.", pivot.Lookups.Components, pivot.Lookups.Components);
			AssertEquals(0, pivot.Lookups.Components.Count);
		}
	}
}
