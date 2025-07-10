using System;
using System.Linq;
using System.Reflection;
using System.Web.UI.WebControls;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class EditOrderZPageTest : ZPageTestCase
	{
		public void TestGetNewDataSource_CheckForValidButNonExistentGuid()
		{
			EditOrderForTest testForm = new EditOrderForTest();
			AssertNoExceptionThrown(() => testForm.OnLoad(null));
		}

		public void TestSaveOrder_Click()
		{
			TestPage.OrderForTest = Factory.New<TrackingOrder>();
			TestPage.OnLoad(EventArgs.Empty);
			TestPage.SaveButtonClick();
			TestPage.OrderForTest = Factory.New<TrackingOrder>();
			TestPage.OnLoad(EventArgs.Empty);
			TestPage.OrderForTest.JD_OrderNumber = "123456789";
			TestPage.SaveButtonClick();
			AssertEquals("123456789", TestPage.OrderForTest.JD_OrderNumber);

			var newPage = new EditOrderForTest();
			newPage.OrderForTest = Factory.New<TrackingOrder>();
			newPage.OrderForTest.JD_OrderNumber = "234567890";
			newPage.OnLoad(EventArgs.Empty);
			newPage.OrderForTest.JD_OH_SendingAgent = newPage.OrderForTest.JD_OH_ReceivingAgent = new ZGuid("F9163C5E-CEB2-4faa-B5BF-329BF39FA1E4");
			newPage.SaveButtonClick();
			AssertEquals(newPage.OrderForTest.JD_OH_SendingAgentInfo.HasErrors(), false);
			AssertEquals(newPage.OrderForTest.JD_OH_ReceivingAgentInfo.HasErrors(), false);
		}

		public void TestRefreshingProductDescription()
		{
			RunRefreshingProductDescription(OrgPartRelation.RelationshipTypes.Owner);
		}

		public void TestRefreshingProductDescriptionWithBoth()
		{
			RunRefreshingProductDescription(OrgPartRelation.RelationshipTypes.Both);
		}

		public void TestOrderLinesGridHasEnabledCustomFieldsForLoggedInOrg()
		{
			var helper = new TestHelper(Factory);
			var orgCustomLabel = helper.TestOrg.CustomLabels.AddNew();
			orgCustomLabel.OT_FieldName = "OrderLine.CustomAttrib1";
			orgCustomLabel.OT_Caption = "Custom Attribute 1 Test";

			Factory.Save();

			TestPage.SiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			TestPage.OnLoad(EventArgs.Empty);

			var columns = TestPage.OrderLinesGridForTest.Columns.Cast<DataGridColumn>();
			AssertCollectionContains(columns, (column) => column.HeaderText == "Custom Attribute 1 Test - CA");
		}

		public void TestOrderLinesGridDoesNotDuplicateCustomFieldsColumnsOnPostback()
		{
			var helper = new TestHelper(Factory);
			var orgCustomLabel = helper.TestOrg.CustomLabels.AddNew();
			orgCustomLabel.OT_FieldName = "OrderLine.CustomAttrib1";
			orgCustomLabel.OT_Caption = "Custom Attribute 1 Test";

			Factory.Save();

			TestPage.SiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			TestPage.OnLoad(EventArgs.Empty);

			var columns = TestPage.OrderLinesGridForTest.Columns.Cast<DataGridColumn>();
			AssertEquals(22, columns.Count());

			TestPage.SelectedIndexChangedPostBackForTest();
			AssertEquals(22, columns.Count());
		}

		public void TestOrderLinesGridHasEnabledCustomFieldsForBuyer()
		{
			var helper = new TestHelper(Factory);
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var buyerCustomLabel = buyer.CustomLabels.AddNew();
			buyerCustomLabel.OT_FieldName = "OrderLine.CustomAttrib2";
			buyerCustomLabel.OT_Caption = "Custom Attribute 2 Test";

			Factory.Save();

			TestPage.OrderForTest.BuyerPK = buyer.PK;
			TestPage.SiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			TestPage.OnLoad(EventArgs.Empty);

			var columns = TestPage.OrderLinesGridForTest.Columns.Cast<DataGridColumn>();
			AssertCollectionContains(columns, (column) => column.HeaderText == "Custom Attribute 2 Test - CA");
		}

		void RunRefreshingProductDescription(string ownerRelationship)
		{
			OrgSupplierPart product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "FIRST";
			product1.OP_Desc = "First Product";

			OrgSupplierPart product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "SECOND";
			product2.OP_Desc = "Second Product";
			OrgPartRelation relation = product2.RelatedOrganisations.AddNew();
			relation.OU_Relationship = ownerRelationship;
			OrgHeader owner = Factory.NewWithValidTestData<OrgHeader>();
			relation.OU_OH = owner.PK;

			Factory.Save();
			TrackingOrder order = TestPage.OrderForTest;
			AssertNotNull("Order should be not null", order);

			order.JD_OrderNumber = "123456789";
			order.BuyerPK = owner.PK;
			AssertNotNull("Buyer should be assigned", order.Buyer);
			AssertEquals("Buyer as expected", owner.PK, order.Buyer.PK);

			order.SupplierPK = owner.PK;
			AssertNotNull("Supplier should be assigned", order.Supplier);
			AssertEquals("Supplier as expected", owner.PK, order.Supplier.PK);

			order.OrderLines.AddNew();
			order.OrderLines[0].JO_Partno = product1.OP_PartNum;
			AssertNotNull("Order should be not null", order.OrderLines[0].Order);

			Factory.Save();

			TestPage.OnLoad(EventArgs.Empty);
			TestPage.OrderLinesGridForTest.EditItemIndex = 0;
			TestPage.DataBind();

			Assert("ItemDataBound should be called", TestPage.ItemDataBoundWasCalled);
			ZFindBox findBox = TestPage.OrderLinesGridForTest.Items.Count > 0 &&
				TestPage.OrderLinesGridForTest.Items[0].Cells.Count > 2 &&
				TestPage.OrderLinesGridForTest.Items[0].Cells[2].Controls.Count > 0 ?
				TestPage.OrderLinesGridForTest.Items[0].Cells[2].Controls[0] as ZFindBox : null;
			AssertNotNull("Product ZFindBox should be not null", findBox);

			ZTextBox desc = TestPage.OrderLinesGridForTest.Items.Count > 0 &&
				TestPage.OrderLinesGridForTest.Items[0].Cells.Count > 3 &&
				TestPage.OrderLinesGridForTest.Items[0].Cells[3].Controls.Count > 0 ?
				TestPage.OrderLinesGridForTest.Items[0].Cells[3].Controls[0] as ZTextBox : null;
			AssertNotNull("Description ZTextBox should be not null", desc);

			AssertEquals("Product PartNum", order.OrderLines[0].JO_Partno, findBox.Text);
			AssertEquals("Product Description", order.OrderLines[0].JO_Description, desc.Text);

			findBox.Text = product2.OP_PartNum;
			findBox.HasChanges = true;
			typeof(ZGuidFindBox).InvokeMember("OnPostDataChanged", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, findBox, new object[] { null, EventArgs.Empty });
			AssertEquals("Product assigned", product2.OP_PartNum, order.OrderLines[0].JO_Partno);
			AssertEquals("Product Description", product2.OP_Desc, order.OrderLines[0].JO_Description);

			int productEditCellNumber = TestPage.GetProductColumnIndexExposed();

			findBox = TestPage.OrderLinesGridForTest.Items.Count > 0 &&
				TestPage.OrderLinesGridForTest.Items[0].Cells.Count > productEditCellNumber &&
				TestPage.OrderLinesGridForTest.Items[0].Cells[productEditCellNumber].Controls.Count > 0 ?
				TestPage.OrderLinesGridForTest.Items[0].Cells[productEditCellNumber].Controls[0] as ZFindBox : null;
			AssertNotNull("Product ZFindBox should be not null", findBox);

			desc = TestPage.OrderLinesGridForTest.Items.Count > 0 &&
				TestPage.OrderLinesGridForTest.Items[0].Cells.Count > productEditCellNumber + 1 &&
				TestPage.OrderLinesGridForTest.Items[0].Cells[productEditCellNumber + 1].Controls.Count > 0 ?
				TestPage.OrderLinesGridForTest.Items[0].Cells[productEditCellNumber + 1].Controls[0] as ZTextBox : null;
			AssertNotNull("Description ZTextLabel should be not null", desc);
			AssertEquals("Product Description", product2.OP_Desc, desc.Text);

			findBox.Text = "SomethingElse";
			typeof(ZGuidFindBox).InvokeMember("OnTextChanged", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, findBox, new object[] { EventArgs.Empty });
			desc = TestPage.OrderLinesGridForTest.Items.Count > 0 &&
				TestPage.OrderLinesGridForTest.Items[0].Cells.Count > productEditCellNumber + 1 &&
				TestPage.OrderLinesGridForTest.Items[0].Cells[productEditCellNumber + 1].Controls.Count > 0 ?
				TestPage.OrderLinesGridForTest.Items[0].Cells[productEditCellNumber + 1].Controls[0] as ZTextBox : null;
			AssertNotNull("Description ZTextLabel should be not null", desc);
			AssertEquals("Product Description", product2.OP_Desc, desc.Text);
			AssertEquals("Should not change Product Description because the TextChanged event is now unhooked", product2.OP_Desc, order.OrderLines[0].JO_Description);
		}

		EditOrderForTest TestPage
		{
			get { return Page as EditOrderForTest; }
		}

		protected override ZPage GetNewZPage()
		{
			return new EditOrderForTest();
		}

		#region Test Setup

		bool oldIsWeb;

		protected override void SetUp()
		{
			base.SetUp();
			oldIsWeb = Globals.IsWeb;
			Globals.IsWeb = true;
		}

		protected override void TearDown()
		{
			Globals.IsWeb = oldIsWeb;
			base.TearDown();
		}

		#endregion
	}
}
