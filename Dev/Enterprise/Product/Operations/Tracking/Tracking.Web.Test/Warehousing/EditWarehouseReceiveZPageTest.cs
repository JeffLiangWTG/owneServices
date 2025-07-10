using System;
using System.Reflection;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[SetGlobalsIsWeb]
	sealed class EditWarehouseReceiveZPageTest : ZPageTestCase
	{
		public void TestRefreshingProductDescription()
		{
			RunRefreshingProductDescription(OrgPartRelation.RelationshipTypes.Owner);
		}
		public void TestRefreshingProductDescriptionWithBoth()
		{
			RunRefreshingProductDescription(OrgPartRelation.RelationshipTypes.Both);
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
			var trackingReceive = TestPage.ReceiveForTest;
			var receive = trackingReceive.WhsReceive;
			AssertNotNull("Receive should be not null", receive);
			receive.WD_OH_Client = owner.PK;
			AssertNotNull("Client should be assigned", receive.Client);
			AssertEquals("Client as expected", owner.PK, receive.Client.PK);

			trackingReceive.Lines.AddNew();
			var receiveLine = trackingReceive.Lines[0].WhsReceiveLine;
			receiveLine.WE_OP = product1.PK;
			AssertNotNull("Docket should be not null", receiveLine.Docket);
			receiveLine.Docket.WD_OH_Client = owner.PK;
			AssertNotNull("Docket Client should be assigned", receiveLine.Docket.Client);
			AssertEquals("Docket Client as expected", owner.PK, receiveLine.Docket.Client.PK);

			Factory.Save();

			TestPage.OnLoad(EventArgs.Empty);
			TestPage.WhsReceiveInventoryGridForTest.EditItemIndex = 0;
			TestPage.DataBind();

			Assert("ItemDataBound should be called", TestPage.ItemDataBoundWasCalled);
			ZGuidFindBox findBox = TestPage.WhsReceiveInventoryGridForTest.Items.Count > 0 &&
				TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells.Count > 1 &&
				TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells[1].Controls.Count > 0 ?
				TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells[1].Controls[0] as ZGuidFindBox : null;
			AssertNotNull("Product GuidFindBox should be not null", findBox);

			ZTextLabel desc = TestPage.WhsReceiveInventoryGridForTest.Items.Count > 0 &&
				TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells.Count > 2 &&
				TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells[2].Controls.Count > 0 ?
				TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells[2].Controls[0] as ZTextLabel : null;
			AssertNotNull("Description label should be not null", desc);

			AssertEquals("Product PartNum", product1.OP_PartNum, findBox.Text);
			AssertEquals("Product Description", product1.OP_Desc, desc.Text);

			findBox.Text = product2.OP_PartNum;
			findBox.HasChanges = true;
			typeof(ZGuidFindBox).InvokeMember("OnTextChanged", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, findBox, new object[] { EventArgs.Empty });
			AssertEquals("Product assigned", product2.PK, receiveLine.WE_OP);
			AssertEquals("Product Description", product2.OP_Desc, receiveLine.ProductDesc);

			int productEditCellNumber = TestPage.GetColumnIndexForTest("Product");

			findBox = TestPage.WhsReceiveInventoryGridForTest.Items.Count > 0 &&
				TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells.Count > productEditCellNumber &&
				TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells[productEditCellNumber].Controls.Count > 0 ?
				TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells[productEditCellNumber].Controls[0] as ZGuidFindBox : null;
			AssertNotNull("Product GuidFindBox should be not null", findBox);

			desc = TestPage.WhsReceiveInventoryGridForTest.Items.Count > 0 &&
				TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells.Count > productEditCellNumber + 1 &&
				TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells[productEditCellNumber + 1].Controls.Count > 0 ?
				TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells[productEditCellNumber + 1].Controls[0] as ZTextLabel : null;
			AssertNotNull("Description label should be not null", desc);
			AssertEquals("Product Description", product2.OP_Desc, desc.Text);

			findBox.Text = "SomethingElse";
			typeof(ZGuidFindBox).InvokeMember("OnTextChanged", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, findBox, new object[] { EventArgs.Empty });
			desc = TestPage.WhsReceiveInventoryGridForTest.Items.Count > 0 &&
				TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells.Count > productEditCellNumber + 1 &&
				TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells[productEditCellNumber + 1].Controls.Count > 0 ?
				TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells[productEditCellNumber + 1].Controls[0] as ZTextLabel : null;
			AssertNotNull("Description label should be not null", desc);
			AssertEquals("Product Description", product2.OP_Desc, desc.Text);
			AssertEquals("Should not change Product Description because the TextChanged event is now unhooked", product2.OP_Desc, receiveLine.ProductDesc);
		}

		public void TestOrderLinesGrid()
		{
			AssertColumnIsInGrid("Product", 0, typeof(ZFindBoxColumn));
			AssertColumnIsInGrid("Description", 1, typeof(ZTextEditColumn));
			AssertColumnIsInGrid("Packs", 2, typeof(ZCalcEditColumn));
			AssertColumnIsInGrid("Packs UQ", 3, typeof(ZDropDownListColumn));
			AssertColumnIsInGrid("Expected Quantity", 4, typeof(ZCalcEditColumn));
			AssertColumnIsInGrid("Quantity", 5, typeof(ZCalcEditColumn));
			AssertColumnIsInGrid("UQ", 6, typeof(ZDropDownListColumn));
		}

		public void TestColumnsAreSetupForQuantityCalculations()
		{
			OrgSupplierPart product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "FIRST";
			product1.OP_Desc = "First Product";
			var trackingReceive = TestPage.ReceiveForTest;
			trackingReceive.Lines.AddNew();
			var receiveLine = trackingReceive.Lines[0].WhsReceiveLine;
			receiveLine.WE_OP = product1.PK;
			Factory.Save();

			TestPage.OnLoad(EventArgs.Empty);
			TestPage.WhsReceiveInventoryGridForTest.EditItemIndex = 0;
			TestPage.DataBind();

			TestPage.ItemDataBoundWasCalled = false;
			int packsIndex = TestPage.GetColumnIndexForTest("Packs");
			int packsUQIndex = TestPage.GetColumnIndexForTest("Packs UQ");
			int quantityIndex = TestPage.GetColumnIndexForTest("Quantity");
			var expectedQuantityIndex = TestPage.GetColumnIndexForTest("Expected Quantity");

			AssertEquals("Should not do Packs AutoPostBack", false, ((ZNumericTextBox)(TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells[packsIndex].Controls[0])).AutoPostBack);
			AssertEquals("Should not do Packs UQ AutoPostBack", false, ((ZDropDownList)(TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells[packsUQIndex].Controls[0])).AutoPostBack);
			AssertEquals("Should not do Quantity AutoPostBack", false, ((ZNumericTextBox)(TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells[quantityIndex].Controls[0])).AutoPostBack);
			AssertEquals("Should not do Expected Quantity AutoPostBack", false, ((ZNumericTextBox)(TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells[expectedQuantityIndex].Controls[0])).AutoPostBack);

			// Test Packs AutoPostBack
			ZNumericTextBox packsBox = TestPage.WhsReceiveInventoryGridForTest.Items.Count > 0 ?
				TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells[packsIndex].Controls[0] as ZNumericTextBox : null;
			AssertNotNull("Packs control should not be null", packsBox);

			packsBox.HasChanges = true;
			typeof(ZNumericTextBox).InvokeMember("OnTextChanged", BindingFlags.Instance | BindingFlags.Instance |
				BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, packsBox, new object[] { EventArgs.Empty });
			Assert("AutoPostBack should have ocurred", TestPage.ItemDataBoundWasCalled);
			TestPage.ItemDataBoundWasCalled = false;

			// Test Packs UQ AutoPostBack
			ZDropDownList packsUQBox = TestPage.WhsReceiveInventoryGridForTest.Items.Count > 0 ?
				TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells[packsUQIndex].Controls[0] as ZDropDownList : null;
			AssertNotNull("Packs UQ control should not be null", packsUQBox);

			packsUQBox.HasChanges = true;
			typeof(ZDropDownList).InvokeMember("OnTextChanged", BindingFlags.Instance | BindingFlags.Instance |
				BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, packsUQBox, new object[] { EventArgs.Empty });
			Assert("AutoPostBack should have ocurred", TestPage.ItemDataBoundWasCalled);
			TestPage.ItemDataBoundWasCalled = false;

			// Test Quantity AutoPostBack
			ZNumericTextBox quantityBox = TestPage.WhsReceiveInventoryGridForTest.Items.Count > 0 ?
				TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells[quantityIndex].Controls[0] as ZNumericTextBox : null;
			AssertNotNull("Quantity control should not be null", quantityBox);

			quantityBox.HasChanges = true;
			typeof(ZNumericTextBox).InvokeMember("OnTextChanged", BindingFlags.Instance | BindingFlags.Instance |
				BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, quantityBox, new object[] { EventArgs.Empty });
			Assert("AutoPostBack should have ocurred", TestPage.ItemDataBoundWasCalled);
			TestPage.ItemDataBoundWasCalled = false;

			// Test Expected Quantity AutoPostBack
			var expectedQuantityBox = TestPage.WhsReceiveInventoryGridForTest.Items.Count > 0 ?
				TestPage.WhsReceiveInventoryGridForTest.Items[0].Cells[expectedQuantityIndex].Controls[0] as ZNumericTextBox : null;
			AssertNotNull("Quantity control should not be null", expectedQuantityBox);

			expectedQuantityBox.HasChanges = true;
			typeof(ZNumericTextBox).InvokeMember("OnTextChanged", BindingFlags.Instance | BindingFlags.Instance |
				BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, expectedQuantityBox, new object[] { EventArgs.Empty });
			Assert("AutoPostBack should have ocurred", TestPage.ItemDataBoundWasCalled);
			TestPage.ItemDataBoundWasCalled = false;
		}

		void AssertColumnIsInGrid(ZString headerText, int columnIndex, Type columnType)
		{
			ZDataGrid whsInventoryLinesGridForTesting = TestPage.WhsReceiveInventoryGridForTest;

			if (columnIndex < whsInventoryLinesGridForTesting.Columns.Count)
			{
				bool columnIsInGrid = (whsInventoryLinesGridForTesting.Columns[columnIndex].HeaderText.Equals(headerText));
				Assert(ZString.Format("Column '{0}' is not in the grid at index {1} as expected.", headerText, columnIndex), columnIsInGrid);
				AssertEquals(ZString.Format("Column '{0}' type", headerText), columnType, whsInventoryLinesGridForTesting.Columns[columnIndex].GetType());
			}
			else
			{
				Fail(ZString.Format("The expected index {0} of column '{1}' is out of range.", columnIndex, headerText));
			}
		}

		EditWarehouseReceiveForTest TestPage
		{
			get { return Page as EditWarehouseReceiveForTest; }
		}

		protected override ZPage GetNewZPage()
		{
			return new EditWarehouseReceiveForTest();
		}
	}
}
