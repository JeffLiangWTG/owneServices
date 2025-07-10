using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgBuyerSupplierLinkPackPivotCollection))]
	sealed class OrgBuyerSupplierLinkPackPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgBuyerSupplierLinkPackPivotCollection>
	{
		protected override OrgBuyerSupplierLinkPackPivotCollection GetCollectionToTest()
		{
			return new OrgBuyerSupplierLinkPackPivotCollection(Factory.New<OrgSupplierBuyerLink>());
		}

		[ExpectNoExceptions]
		public void TestGetDetailsForPackType()
		{
			var collection = new RefPackTypeCollection(Factory);
			var packType = Factory.New<RefPackType>();

			packType.F3_Code = "ZZZ";
			packType.F3_Description = "This is new.";

			var pivotCollection = GetCollectionToTest();
			var pivot = Factory.New<OrgBuyerSupplierLinkPackPivot>();

			pivot.Q0_F3 = packType.PK;
			pivotCollection.Add(pivot);

			var testPivot = pivotCollection.GetDetailsForPackType("ZZZ");

			AssertEquals(packType.PK, testPivot.Q0_F3);
			AssertEquals(pivotCollection.orgSupplierBuyerLink.PK, testPivot.Q0_OL);
			AssertNull(pivotCollection.GetDetailsForPackType("ABC"));
			AssertNull(pivotCollection.GetDetailsForPackType(null));
		}
	}
}
