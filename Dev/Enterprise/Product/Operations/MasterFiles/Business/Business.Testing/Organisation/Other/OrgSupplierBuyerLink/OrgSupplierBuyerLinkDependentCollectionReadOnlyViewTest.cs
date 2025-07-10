using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSupplierBuyerLinkCollectionReadOnlyView))]
	sealed class OrgSupplierBuyerLinkDependentCollectionReadOnlyViewTest : BusinessObjectCollectionViewTestCase<OrgSupplierBuyerLinkCollectionReadOnlyView>
	{
		protected override OrgSupplierBuyerLinkCollectionReadOnlyView GetCollectionToTest()
		{
			Organisation = Factory.New<OrgHeader>();
			return new OrgSupplierBuyerLinkCollectionReadOnlyView(Organisation.SupplierLinks);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Organisation.BuyerLinks.AddNew();
		}

		public void TestOrgSupplierBuyerLinkCollectionReadOnlyView()
		{
			OrgHeader supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_Code = "X1";
			OrgHeader supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			supplier2.OH_Code = "X2";
			OrgHeader supplier3 = Factory.NewWithValidTestData<OrgHeader>();
			supplier3.OH_Code = "X3";

			OrgSupplierBuyerLink supplierLink1 = Organisation.SupplierLinks.AddNew();
			OrgSupplierBuyerLink supplierLink2 = Organisation.SupplierLinks.AddNew();
			OrgSupplierBuyerLink supplierLink3 = Organisation.SupplierLinks.AddNew();

			supplierLink1.OL_OH_Supplier = supplier1.PK;
			supplierLink1.OL_InitialShipmentExpected = ZDateTime.Empty;
			supplierLink1.UpdateShipmentDate = true;
			supplierLink2.OL_OH_Supplier = supplier2.PK;
			supplierLink2.OL_InitialShipmentExpected = ZDateTime.Today;
			supplierLink3.OL_OH_Supplier = supplier3.PK;
			supplierLink3.OL_InitialShipmentExpected = ZDateTime.Empty;

			supplierLink1.SelectedForPrinting = true;
			supplierLink2.SelectedForPrinting = true;
			supplierLink3.SelectedForPrinting = true;

			OrgSupplierBuyerLinkCollectionReadOnlyView supplierBuyerView = new OrgSupplierBuyerLinkCollectionReadOnlyView(Organisation.SupplierLinks);
			supplierLink3.ExpectedShipmentMonthsAddition = "6";
			Organisation.SupplierLinks.SetNewDateOnFirstExpectedShipment();

			AssertEquals("Supplier 1 shipment date", ZDateTime.Today.AddMonths(3), supplierLink1.OL_InitialShipmentExpected);
			AssertEquals("Supplier 2 shipment date", ZDateTime.Today, supplierLink2.OL_InitialShipmentExpected);
			AssertEquals("Supplier 3 shipment date", ZDateTime.Today.AddMonths(6), supplierLink3.OL_InitialShipmentExpected);
		}

		#region Implementation

		OrgHeader Organisation;
		protected override void SetUp()
		{
			base.SetUp();
			Organisation = Factory.NewWithValidTestData<OrgHeader>();
		}

		#endregion
	}
}
