using System;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class EditWarehouseOrderZPageTest : ZPageTestCase
	{
		#region TestRefreshingProductDescription

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

			WhsOrder order = TestPage.OrderForTest.WhsOrder;
			AssertNotNull("Order should be not null", order);
			order.WD_OH_Client = owner.PK;
			AssertNotNull("Client should be assigned", order.Client);
			AssertEquals("Client as expected", owner.PK, order.Client.PK);

			order.Lines.AddNew();
			order.Lines[0].WE_OP = product1.PK;
			AssertNotNull("Docket should be not null", order.Lines[0].Docket);
			order.Lines[0].Docket.WD_OH_Client = owner.PK;
			AssertNotNull("Docket Client should be assigned", order.Lines[0].Docket.Client);
			AssertEquals("Docket Client as expected", owner.PK, order.Lines[0].Docket.Client.PK);

			Factory.Save();

			TestPage.OnLoad(EventArgs.Empty);
			TestPage.WhsOrderLinesGridForTest.EditItemIndex = 0;
			TestPage.DataBind();

			Assert("ItemDataBound should be called", TestPage.ItemDataBoundWasCalled);
			ZGuidFindBox findBox = TestPage.WhsOrderLinesGridForTest.Items.Count > 0 &&
				TestPage.WhsOrderLinesGridForTest.Items[0].Cells.Count > 1 &&
				TestPage.WhsOrderLinesGridForTest.Items[0].Cells[1].Controls.Count > 0 ?
				TestPage.WhsOrderLinesGridForTest.Items[0].Cells[1].Controls[0] as ZGuidFindBox : null;
			AssertNotNull("Product GuidFindBox should be not null", findBox);

			ZTextLabel desc = TestPage.WhsOrderLinesGridForTest.Items.Count > 0 &&
				TestPage.WhsOrderLinesGridForTest.Items[0].Cells.Count > 2 &&
				TestPage.WhsOrderLinesGridForTest.Items[0].Cells[2].Controls.Count > 0 ?
				TestPage.WhsOrderLinesGridForTest.Items[0].Cells[2].Controls[0] as ZTextLabel : null;
			AssertNotNull("Description label should be not null", desc);

			AssertEquals("Product PartNum", product1.OP_PartNum, findBox.Text);
			AssertEquals("Product Description", product1.OP_Desc, desc.Text);

			findBox.Text = product2.OP_PartNum;
			findBox.HasChanges = true;
			typeof(ZGuidFindBox).InvokeMember("OnTextChanged", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, findBox, new object[] { EventArgs.Empty });
			AssertEquals("Product assigned", product2.PK, order.Lines[0].WE_OP);
			AssertEquals("Product Description", product2.OP_Desc, order.Lines[0].ProductDesc);

			int productEditCellNumber = TestPage.GetColumnIndexForTest("Product");

			findBox = TestPage.WhsOrderLinesGridForTest.Items.Count > 0 &&
				TestPage.WhsOrderLinesGridForTest.Items[0].Cells.Count > productEditCellNumber &&
				TestPage.WhsOrderLinesGridForTest.Items[0].Cells[productEditCellNumber].Controls.Count > 0 ?
				TestPage.WhsOrderLinesGridForTest.Items[0].Cells[productEditCellNumber].Controls[0] as ZGuidFindBox : null;
			AssertNotNull("Product GuidFindBox should be not null", findBox);

			desc = TestPage.WhsOrderLinesGridForTest.Items.Count > 0 &&
				TestPage.WhsOrderLinesGridForTest.Items[0].Cells.Count > productEditCellNumber + 1 &&
				TestPage.WhsOrderLinesGridForTest.Items[0].Cells[productEditCellNumber + 1].Controls.Count > 0 ?
				TestPage.WhsOrderLinesGridForTest.Items[0].Cells[productEditCellNumber + 1].Controls[0] as ZTextLabel : null;
			AssertNotNull("Description label should be not null", desc);
			AssertEquals("Product Description", product2.OP_Desc, desc.Text);

			findBox.Text = "SomethingElse";
			typeof(ZGuidFindBox).InvokeMember("OnTextChanged", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, findBox, new object[] { EventArgs.Empty });
			desc = TestPage.WhsOrderLinesGridForTest.Items.Count > 0 &&
				TestPage.WhsOrderLinesGridForTest.Items[0].Cells.Count > productEditCellNumber + 1 &&
				TestPage.WhsOrderLinesGridForTest.Items[0].Cells[productEditCellNumber + 1].Controls.Count > 0 ?
				TestPage.WhsOrderLinesGridForTest.Items[0].Cells[productEditCellNumber + 1].Controls[0] as ZTextLabel : null;
			AssertNotNull("Description label should be not null", desc);
			AssertEquals("Product Description", product2.OP_Desc, desc.Text);
			AssertEquals("Should not change Product Description because the TextChanged event is now unhooked", product2.OP_Desc, order.Lines[0].ProductDesc);
		}

		#endregion

		#region TestDecimalPlaces

		public void TestDecimalPlaces()
		{
			AssertEquals("WhsOrderLine+SupplierPart+OP_CountDecimalPlaces", ((ZCalcEditColumn)TestPage.WhsOrderLinesGridForTest.Columns[4]).BindToDecimals);
			AssertEquals("WhsOrderLine+SupplierPart+OP_CountDecimalPlaces", ((ZCalcEditColumn)TestPage.WhsOrderLinesGridForTest.Columns[6]).BindToDecimals);
		}

		#endregion

		#region TestOrderLinesGridWithCrossDocking

		public void TestClientDependentColumns()
		{
			var organisation = TestPage.SiteUser.LoggedInOrganisation;
			organisation.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.BatchNumber;
			organisation.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.JulianBatchNumber;
			organisation.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;

			organisation.MiscServ.OM_IMPartAttrib1Name = "Attribute1";
			organisation.MiscServ.OM_IMPartAttrib2Name = "Attribute2";
			organisation.MiscServ.OM_IMPartAttrib3Name = "Attribute3";

			TestPage.SetupOrderLinesGridForTesting();
			var grid = TestPage.WhsOrderLinesGridForTest;
			var attributeColumns = grid.Columns.OfType<ZTextEditColumn>().Where(c => c.HeaderText.StartsWith("Attribute"));
			AssertEquals(3, attributeColumns.Count());
			AssertEquals(true, attributeColumns.All(c => c.CanBeEnabledByClient));
		}

		public void TestClientDependentColumns_WithSerialNumberEnabledAndClientUsesSerialNumber()
		{
			TestClientDependentColumns_WithSerialNumberCore(clientUsesSerialNumber: true);
		}

		public void TestClientDependentColumns_ClientDoesNotUseSerialNumber()
		{
			TestClientDependentColumns_WithSerialNumberCore(clientUsesSerialNumber: false);
		}

		void TestClientDependentColumns_WithSerialNumberCore(bool clientUsesSerialNumber)
		{
			var organisation = TestPage.SiteUser.LoggedInOrganisation;
			organisation.MiscServ.OM_IMUseSerialNumber = clientUsesSerialNumber;

			TestPage.SetupOrderLinesGridForTesting();
			AssertEquals(clientUsesSerialNumber, TestPage.WhsOrderLinesGridForTest.Columns.OfType<ZTextEditColumn>().Any(c => c.HeaderText.StartsWith("Serial Number")));
		}

		public void TestOrderLinesGridAddOn()
		{
			var addOn = TestPage.LinesGridAddOnForTest;
			AssertNotNull(addOn);
			AssertEquals(TestPage.WhsOrderLinesGridForTest, addOn.Grid);

			AssertEquals(TrackingWhsOrderLine.WrapperSchema.WE_OP, addOn.ProductBindTo);
			AssertEquals(TrackingWhsOrderLine.WrapperSchema.ProductDescription, addOn.DescriptionBindTo);
			AssertEquals(TrackingWhsOrderLine.WrapperSchema.WE_PackQuantity, addOn.PacksBindTo);
			AssertEquals(TrackingWhsOrderLine.WrapperSchema.WE_F3_NKPackType, addOn.PacksUQBindTo);
			AssertEquals(TrackingWhsOrderLine.WrapperSchema.WE_TransactionQuantity, addOn.QuantityBindTo);
			AssertEquals(TrackingWhsOrderLine.WrapperSchema.ProductUQ, addOn.ProductUQBindTo);
			AssertEquals(TrackingWhsOrderLine.WrapperSchema.WE_ShortfallQuantityCached, addOn.ShortfallBindTo);
			AssertEquals(TrackingWhsOrderLine.WrapperSchema.WE_PartAttrib1, addOn.Attribute1BindTo);
			AssertEquals(TrackingWhsOrderLine.WrapperSchema.WE_PartAttrib2, addOn.Attribute2BindTo);
			AssertEquals(TrackingWhsOrderLine.WrapperSchema.WE_PartAttrib3, addOn.Attribute3BindTo);
			AssertEquals(TrackingWhsOrderLine.WrapperSchema.WE_SerialNumber, addOn.SerialNumberBindTo);
		}

		public void TestOrderLinesGrid_OnPreBindHasNoEffectOnNumberOfColumns()
		{
			var initialNumberOColumns = TestPage.WhsOrderLinesGridForTest.Columns.Count;

			TestPage.OnPreBindForTesting();
			AssertEquals(initialNumberOColumns, TestPage.WhsOrderLinesGridForTest.Columns.Count);
		}

		public void TestOrderLinesGrid_OnPreBindRunsColumnSetup()
		{
			TestPage.OnLoad(EventArgs.Empty);
			var orderPk = TestPage.OrderForTest.PK;
			var column = TestPage.CrossDocksLinkColumnForTest;
			AssertContains("Url contains order PK", $"Edit&Order={orderPk}", column.DataNavigateUrlFormatString);
		}

		public void TestOrderLinesGridWithCrossDocking()
		{
			AssertColumnIsInGrid("Product", 0, typeof(ZFindBoxColumn));
			AssertColumnIsInGrid("Description", 1, typeof(ZTextEditColumn));
			AssertColumnIsInGrid("Packs", 2, typeof(ZCalcEditColumn));
			AssertColumnIsInGrid("Packs UQ", 3, typeof(ZDropDownListColumn));
			AssertColumnIsInGrid("Quantity", 4, typeof(ZCalcEditColumn));
			AssertColumnIsInGrid("UQ", 5, typeof(ZDropDownListColumn));
			AssertColumnIsInGrid("Shortfall Qty.", 6, typeof(ZCalcEditColumn));
			AssertColumnIsInGrid("Reserved", 7, typeof(ZHyperLinkColumn));
		}

		void AssertColumnIsInGrid(ZString headerText, int columnIndex, Type columnType)
		{
			var whsOrderLinesGridForTesting = TestPage.WhsOrderLinesGridForTest;

			if (columnIndex < whsOrderLinesGridForTesting.Columns.Count)
			{
				bool columnIsInGrid = (whsOrderLinesGridForTesting.Columns[columnIndex].HeaderText.Equals(headerText));
				Assert(ZString.Format("Column '{0}' is not in the grid at index {1} as expected.", headerText, columnIndex), columnIsInGrid);
				AssertEquals(ZString.Format("Column '{0}' type", headerText), columnType, whsOrderLinesGridForTesting.Columns[columnIndex].GetType());
			}
			else
			{
				Fail(ZString.Format("The expected index {0} of column '{1}' is out of range.", columnIndex, headerText));
			}
		}

		#endregion

		#region TestSaveOrder_Click

		public void TestSaveOrder_Click()
		{
			var factory = TestPage.Factory;
			var data = new TestDataSimpleEnvironment(factory);

			// setup the tracking order
			var trackingOrder = TestPage.OrderForTest.WhsOrder;
			Helper.SetUpWhsOrder(trackingOrder, data.Org1, data.Whs1);
			AssertNotNull("Precondtion", trackingOrder);

			// setup orderline and data binding
			var orderLine = trackingOrder.Lines.AddNew();
			orderLine.WE_OP = data.Part1.PK;
			orderLine.Docket.WD_OH_Client = data.Org1.PK;
			trackingOrder.WorkflowItems.RemoveAndDeleteAll();
			var orderProcessTask = trackingOrder.WorkflowItems.AddNew();
			orderProcessTask.P9_Description = "Description";
			factory.Save();
			TestPage.OnLoad(EventArgs.Empty);
			TestPage.DataBind();

			// delete the tracking order in another factory
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var orderFromDifferentFactory = newFactory.Load<WhsOrder>(trackingOrder.PK);
			TrackingHelper.Get(orderFromDifferentFactory);
			var orderProcessTaskFRomAnotherFactory = orderFromDifferentFactory.WorkflowItems.Cast<WhsOrderProcessTasks>().Single(l => l.PK == orderProcessTask.PK);
			orderProcessTaskFRomAnotherFactory.Delete();
			newFactory.Save();
			AssertEquals("Precondition", false, orderProcessTaskFRomAnotherFactory.HasChanges);
			AssertEquals("Precondition", true, orderProcessTaskFRomAnotherFactory.IsDeleted);

			// modify the order process task which is deleted in another factory
			AssertEquals("Precondition", 0, ExceptionReporterTestListener.Instance.Count);
			orderProcessTask.P9_Description = "Different description";
			AssertEquals("Precondtion", true, orderProcessTask.HasChanges);
			AssertNoExceptionThrown("No exception should be thrown when the process task is deleted in another facotry and another factory tries to save the same tracking order in a different factory.",
				() => TestPage.FireSave_Click());
			AssertNull("JobHeader should be null on creation as Auto Rating now supports null JobHeaders", trackingOrder.JobHeader);
		}

		#endregion

		#region TestSaveButtonStayAvailableWhenThereIsValidationError

		public void TestSaveButtonStayAvailableWhenThereIsValidationError()
		{
			var factory = TestPage.Factory;
			var data = new TestDataSimpleEnvironment(factory);

			var trackingOrder = TestPage.OrderForTest.WhsOrder;
			Helper.SetUpWhsOrder(trackingOrder, data.Org1, data.Whs1);
			AssertNotNull("Precondtion", trackingOrder);

			// setup orderline and data binding
			var orderLine = trackingOrder.Lines.AddNew();
			orderLine.WE_OP = data.Part1.PK;
			orderLine.Docket.WD_OH_Client = data.Org1.PK;
			factory.Save();
			TestPage.OnLoad(EventArgs.Empty);
			TestPage.DataBind();

			AssertEquals("Pre-condition, Save Order button is available.", true, TestPage.SaveButtonForTest.Enabled);
			TestPage.RowErrorMessageWantToAdd = "Something is wrong, should not Save successfully.";
			TestPage.FireSave_Click();
			AssertEquals("Save Order button should be still available.", true, TestPage.SaveButtonForTest.Enabled);
		}

		#endregion

		#region TestBusinessObjectToValidate

		public void TestBusinessObjectToValidate()
		{
			TestPage.OnLoad(EventArgs.Empty);
			TestPage.DataBind();
			AssertEquals(typeof(TrackingWhsOrder), TestPage.GetBusinessObjectToValidate().GetType());
		}

		#endregion

		#region Implementation

		EditWarehouseOrderForTest TestPage
		{
			get { return Page as EditWarehouseOrderForTest; }
		}

		protected override ZPage GetNewZPage()
		{
			return new EditWarehouseOrderForTest();
		}

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(TestPage.Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion

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
