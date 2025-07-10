using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class OrgSupplierBuyerLinkDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestResetExpectedShipmentMonthsAddition()
		{
			OrgSupplierBuyerLink link1 = Organisation.SupplierLinks.AddNew();
			link1.OL_InitialShipmentExpected = ZDateTime.Empty;
			link1.ExpectedShipmentMonthsAddition = "ZZ";

			OrgSupplierBuyerLink link2 = Organisation.SupplierLinks.AddNew();
			link2.OL_InitialShipmentExpected = ZDateTime.Now;
			link2.ExpectedShipmentMonthsAddition = "ZZ";

			Organisation.SupplierLinks.ResetExpectedShipmentMonths();
			AssertEquals("The Shipment Months should be 3", OrgSupplierBuyerLinkDependentCollection.DefaultExpectedShipmentMonths.ToString(), link1.ExpectedShipmentMonthsAddition);
			AssertEquals("The Shipment Months should be BLANK", "", link2.ExpectedShipmentMonthsAddition);
		}

		public void TestFindFirstRelationshipWithError()
		{
			OrgHeader newSupplier1 = OrgHeader.New(Factory);
			OrgHeader newSupplier2 = OrgHeader.New(Factory);
			newSupplier1.OH_IsConsignor = true;
			newSupplier2.OH_IsConsignor = true;
			Organisation.OH_IsConsignee = true;

			OrgSupplierBuyerLink link1 = Organisation.SupplierLinks.AddNew();
			AssertNoErrors("Initially the BO is error-less", link1);

			OrgSupplierBuyerLink result = Organisation.SupplierLinks.FindFirstRelationshipWithError();
			AssertEquals("First Object found should be Link 1", result, link1);

			link1.OL_OH_Supplier = newSupplier1.PK;
			OrgSupplierBuyerLink link2 = Organisation.SupplierLinks.AddNew();
			AssertNoErrors("Initially the BO is error-less", link1);
			AssertNoErrors("Initially the BO is error-less", link2);

			result = Organisation.SupplierLinks.FindFirstRelationshipWithError();
			AssertEquals("First Object found should be Link 2", result, link2);

			link2.OL_OH_Supplier = newSupplier2.PK;
			AssertNull("No Object should be found", Organisation.SupplierLinks.FindFirstRelationshipWithError());
		}

		public void TestSetDefaultsForNewChild()
		{
			OrgSupplierBuyerLink link1 = Organisation.SupplierLinks.AddNew();
			AssertEquals("Taken default Import INCOTERM", Organisation.MiscServ.OM_IMDefaultINCOTerm, link1.OrgSupBuyLinkTrnModes[0].PF_IncoTerm);

			Organisation.MiscServ.OM_IMDefaultINCOTerm = "TST";
			OrgSupplierBuyerLink link2 = Organisation.SupplierLinks.AddNew();
			AssertEquals("Taken default Import INCOTERM", Organisation.MiscServ.OM_IMDefaultINCOTerm, link2.OrgSupBuyLinkTrnModes[0].PF_IncoTerm);
			Assert("Link1.OL_IncoTerm != Link2.OL_IncoTerm", link1.OrgSupBuyLinkTrnModes[0].PF_IncoTerm != link2.OrgSupBuyLinkTrnModes[0].PF_IncoTerm);

			Organisation.MiscServ.OM_EXDefaultIncoTerm = "ZUB";
			OrgSupplierBuyerLink link3 = Organisation.BuyerLinks.AddNew();
			AssertEquals("Taken default Export INCOTERM", Organisation.MiscServ.OM_EXDefaultIncoTerm, link3.OrgSupBuyLinkTrnModes[0].PF_IncoTerm);
		}

		public void TestReadOnlyView()
		{
			OrgSupplierBuyerLink supplierLink1 = Organisation.SupplierLinks.AddNew();
			OrgSupplierBuyerLink supplierLink2 = Organisation.SupplierLinks.AddNew();
			OrgSupplierBuyerLink supplierLink3 = Factory.New<OrgSupplierBuyerLink>();

			OrgSupplierBuyerLinkCollectionReadOnlyView supplierBuyerView = new OrgSupplierBuyerLinkCollectionReadOnlyView(Organisation.SupplierLinks);

			AssertEquals("View should not allow new Items", false, supplierBuyerView.AllowNew);
			AssertEquals("View should contain all items in original collection", Organisation.SupplierLinks.Count + Organisation.BuyerLinks.Count, supplierBuyerView.Count);
			Assert("View contains specific item", supplierBuyerView.Contains(supplierLink1));
			Assert("View contains specific item", supplierBuyerView.Contains(supplierLink2));
			Assert("View does not contain item not in original collection", !supplierBuyerView.Contains(supplierLink3));
		}

		public void TestSetNewDateOnFirstExpectedShipment()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			OrgHeader newOrg = newFactory.NewWithValidTestData<OrgHeader>();
			OrgSupplierBuyerLink link1 = newOrg.SupplierLinks.AddNew();
			newOrg.OH_Code = "XXX";
			link1.FillWithValidTestData();
			link1.SelectedForPrinting = true;
			link1.OL_InitialShipmentExpected = ZDateTime.Empty;
			AssertEquals("Initial date is empty", ZDateTime.Empty, link1.OL_InitialShipmentExpected);
			AssertEquals("Routing Order Printed date is empty", ZDateTime.Empty, link1.OL_RoutingOrderPrinted);

			newOrg.SupplierLinks.SetNewDateOnFirstExpectedShipment();
			AssertEquals("Initial date is today", ZDateTime.Today, link1.OL_InitialShipmentExpected);
			AssertEquals("Routing Order Printed date is today", ZDateTime.Today, link1.OL_RoutingOrderPrinted);
		}

		#region Implementation

		protected OrgHeader Organisation;
		protected override void SetUp()
		{
			base.SetUp();
			Organisation = Factory.New<OrgHeader>();
		}

		#endregion
	}
}
