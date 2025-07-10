using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OrgSupplierBuyerLinksForm))]
	sealed class OrgSupplierBuyerLinksFormTest : ZFormBasherTest
	{
		OrgHeader Organisation;
		OrgSupplierBuyerLinkCollectionReadOnlyView SupplierBuyerView;

		protected override Form GetFormToBashCore()
		{
			SupplierBuyerView.SupplierBuyerCollection.SetNewDateOnFirstExpectedShipment();
			var form = new OrgSupplierBuyerLinksForm(SupplierBuyerView, OrgHeaderDocumentSupporter.SuppliersBuyersType.Suppliers);
			MissingResourceStringChecker.ExcludeFromTest(form.SetShipmentDatesCheckBox);
			return form;
		}

		public void TestExpectedShipmentMonthsShowsError()
		{
			OrgHeader testBuyer = OrgHeader.New(Factory);
			testBuyer.OH_IsConsignee = true;
			testBuyer.OH_FullName = "FOCUS YOURSELF!";
			testBuyer.OH_RL_NKClosestPort = "USTOO";
			testBuyer.MainAddress.OA_Address1 = "Address1";
			OrgSupplierBuyerLink link = testBuyer.SupplierLinks.AddNew();

			OrgHeader testSupplier = OrgHeader.New(Factory);
			testSupplier.OH_IsConsignor = true;
			testSupplier.OH_FullName = "GETTALIANI FUCCARACII RUGS";
			testSupplier.OH_RL_NKClosestPort = "USKED";
			testSupplier.MainAddress.OA_Address1 = "Address1";

			link.OL_OH_Supplier = testSupplier.PK;
			link.ExpectedShipmentMonthsAddition = "";

			OrgSupplierBuyerLinkCollectionReadOnlyView view = new OrgSupplierBuyerLinkCollectionReadOnlyView(testBuyer.SupplierLinks);
			using (OrgSupplierBuyersLinksFormForTest formForTest = new OrgSupplierBuyersLinksFormForTest(view, OrgHeaderDocumentSupporter.SuppliersBuyersType.Suppliers))
			{
				formForTest.Show();
				link.ExpectedShipmentMonthsAddition = "20";
				formForTest.OKButton_Exposed.PerformClick();
				AssertEquals("Last shown dialog is Error Notification", "Cannot Print Document...", ZFormModaliser.LastFormShownDialogForTest.Text);
				AssertEquals("Dialog should not yet be closed", DialogResult.None, formForTest.DialogResult);

				link.ExpectedShipmentMonthsAddition = "-2";
				formForTest.OKButton_Exposed.PerformClick();
				AssertEquals("Last shown dialog is Error Notification", "Cannot Print Document...", ZFormModaliser.LastFormShownDialogForTest.Text);
				AssertEquals("Dialog should not yet be closed", DialogResult.None, formForTest.DialogResult);

				link.ExpectedShipmentMonthsAddition = "10";
				formForTest.OKButton_Exposed.PerformClick();
				AssertEquals("Dialog should be successfully closed", DialogResult.OK, formForTest.DialogResult);
			}

			AssertEquals("After reset, the value is as expected", "3", link.ExpectedShipmentMonthsAddition);
		}

		public void TestExpectedShipmentMonthsCancelPrinting()
		{
			OrgHeader testBuyer = OrgHeader.New(Factory);
			testBuyer.OH_FullName = "EATON MYSTERY FLIGHTS";
			testBuyer.OH_RL_NKClosestPort = "NOHIT";
			OrgSupplierBuyerLink link = testBuyer.SupplierLinks.AddNew();

			OrgHeader testSupplier = OrgHeader.New(Factory);
			link.OL_OH_Supplier = testSupplier.PK;
			link.ExpectedShipmentMonthsAddition = "";

			OrgSupplierBuyerLinkCollectionReadOnlyView view = new OrgSupplierBuyerLinkCollectionReadOnlyView(testBuyer.SupplierLinks);
			using (OrgSupplierBuyersLinksFormForTest formForTest = new OrgSupplierBuyersLinksFormForTest(view, OrgHeaderDocumentSupporter.SuppliersBuyersType.Suppliers))
			{
				formForTest.Show();
				formForTest.CancelButton_Exposed.PerformClick();
				AssertEquals("Dialog should be closed with a Cancel Result", DialogResult.Cancel, formForTest.DialogResult);
			}

			AssertEquals("After reset, the value is as expected", "3", link.ExpectedShipmentMonthsAddition);
		}

		protected override void SetUp()
		{
			base.SetUp();

			Organisation = Factory.NewWithValidTestData<OrgHeader>();

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
			supplierLink1.UpdateShipmentDate = false;
			supplierLink1.OL_InitialShipmentExpected = ZDateTime.Today;
			supplierLink2.OL_OH_Supplier = supplier2.PK;
			supplierLink2.OL_InitialShipmentExpected = ZDateTime.Today;
			supplierLink2.UpdateShipmentDate = false;
			supplierLink3.OL_OH_Supplier = supplier3.PK;
			supplierLink3.OL_InitialShipmentExpected = ZDateTime.Today;
			supplierLink3.UpdateShipmentDate = false;

			SupplierBuyerView = new OrgSupplierBuyerLinkCollectionReadOnlyView(Organisation.SupplierLinks);
		}
	}
}
