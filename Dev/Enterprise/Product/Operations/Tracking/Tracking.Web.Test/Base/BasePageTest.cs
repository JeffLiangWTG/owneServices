using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Web;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;
using LoginStatus = Enterprise.ZArchitecture.Web.GUI.WebControls.LoginStatus;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class BasePageTest : TestCaseWithFactory
	{
		#region Atributes

		public void TestAddAttributeColumns()
		{
			BasePageTester page = new BasePageTester();
			Globals.IsWeb = true;
			try
			{
				OrgHeader testLoggedInOrg = Factory.New<OrgHeader>();
				testLoggedInOrg.OH_Code = "XXXXX";

				OrgHeader testBuyer = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();

				page.SiteUser.LoginSupportForTest(testLoggedInOrg.OH_Code);
				testLoggedInOrg = page.SiteUser.LoggedInOrganisation;

				GlbCompany.CurrentCompany.OrgProxy.CustomLabels.RemoveAndDeleteAll();
				testLoggedInOrg.CustomLabels.RemoveAndDeleteAll();
				testBuyer.CustomLabels.RemoveAndDeleteAll();

				ZDataGrid testGrid = new ZDataGrid();

				//Asserting that part and header attributes are not included
				GlbCompany.CurrentCompany.OrgProxy.MiscServ.OM_IMPartAttrib1Name = "OrgProxy'sPA1";
				testBuyer.MiscServ.OM_IMPartAttrib2Name = "Buyer'sPA2";
				testLoggedInOrg.MiscServ.OM_IMPartAttrib3Name = "LoggedInOrg'sPA3";
				OrgCustomLabels b_OH_CD1 = testBuyer.CustomLabels.AddNew();
				b_OH_CD1.OT_FieldName = Core.Constants.CustomLabels.Order.CustomDate1;
				b_OH_CD1.OT_Caption = "TestBuyer'sOH_CD1";

				page.AddLineAttributeColumnsForTest(testGrid, AttributeManager.AttributeModules.Order, testBuyer);
				AssertEquals("Only line arrtibutes can be included", 0, testGrid.Columns.Count);

				OrgCustomLabels oP_CD3 = GlbCompany.CurrentCompany.OrgProxy.CustomLabels.AddNew();
				oP_CD3.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomDate3;
				oP_CD3.OT_Caption = "OrgProxy'sCD3";

				page.AddLineAttributeColumnsForTest(testGrid, AttributeManager.AttributeModules.Order, testBuyer);
				AssertEquals("Column Count", 1, testGrid.Columns.Count);
				AssertEquals("Should Bind to appropriate field", JobOrderLineSchema.JO_CustomDate3.Name, ((ZDateTimeColumn)testGrid.Columns[0]).BindTo);
				AssertEquals("Column Name", "OrgProxy'sCD3 - CA", testGrid.Columns[0].HeaderText);

				OrgCustomLabels b_CA1 = testBuyer.CustomLabels.AddNew();
				b_CA1.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomAttribute1;
				b_CA1.OT_Caption = "TestBuyer'sCA1";

				testGrid.Columns.Clear();
				page.AddLineAttributeColumnsForTest(testGrid, AttributeManager.AttributeModules.Order, testBuyer);
				AssertEquals("Column Count", 2, testGrid.Columns.Count);

				OrgCustomLabels lO_CA2 = testLoggedInOrg.CustomLabels.AddNew();
				lO_CA2.OT_FieldName = Core.Constants.CustomLabels.OrderLine.CustomAttribute2;
				lO_CA2.OT_Caption = "LoggedInOrg'sCA2";

				testGrid.Columns.Clear();
				page.AddLineAttributeColumnsForTest(testGrid, AttributeManager.AttributeModules.Order, testBuyer);
				AssertEquals("Column Count", 3, testGrid.Columns.Count);
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		public void TestSetupAdditionalInformationGridNoData()
		{
			ZCollapsablePanel panel = new ZCollapsablePanel();
			Table testTable = new Table();
			panel.Controls.Add(testTable);
			CustomLabelInfoList customLabels = new CustomLabelInfoList(typeof(TrackingOrder), Factory.New<OrgHeader>(), (NoResString)string.Empty, Factory);
			AssertEquals(0, customLabels.Count);
			new BasePageTester().SetupAdditionalInformationTableForTest(customLabels, testTable, panel);

			Assert("Should be hidden if no data foung", !panel.Visible);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		public void TestSetupAdditionalInformationGrid()
		{
			ZCollapsablePanel testPanel = new ZCollapsablePanel();
			Table testTable = new Table();
			testPanel.Controls.Add(testTable);

			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_FullName = "Test organization";
			testOrg.OH_Code = "TST";

			OrgCustomLabels customLabels1 = testOrg.CustomLabels.AddNew();
			customLabels1.OT_FieldName = "OrderHeader.CustomContact1";
			customLabels1.OT_Caption = "Test Custom Contact 1";

			OrgCustomLabels customLabels2 = testOrg.CustomLabels.AddNew();
			customLabels2.OT_FieldName = "OrderHeader.CustomFlag1";
			customLabels2.OT_Caption = "Test Custom Flag 1";

			OrgCustomLabels customLabels3 = testOrg.CustomLabels.AddNew();
			customLabels3.OT_FieldName = "OrderHeader.CustomDecimal1";
			customLabels3.OT_Caption = "Test Custom Decimal 1";

			OrgCustomLabels customLabels4 = testOrg.CustomLabels.AddNew();
			customLabels4.OT_FieldName = "OrderHeader.CustomDate1";
			customLabels4.OT_Caption = "Test Custom Date 1";

			OrgCustomLabels customLabels5 = testOrg.CustomLabels.AddNew();
			customLabels5.OT_FieldName = "OrderHeader.CustomAttrib1";
			customLabels5.OT_Caption = "Test Custom Attribute 1";

			TrackingOrder testTrackingOrder = Factory.New<TrackingOrder>();
			testTrackingOrder.BuyerPK = testOrg.PK;

			AssertEquals("There must be five custom fields.", 5, testOrg.CustomLabels.Count);

			CustomLabelInfoList customLabels = testTrackingOrder.GetAdditionalInformationFields();
			new BasePageTester().SetupAdditionalInformationTableForTest(customLabels, testTable, testPanel);

			AssertEquals("There must be five custom fields.", 5, testTable.Rows.Count);

			Dictionary<string, string> fields = new Dictionary<string, string>();

			Label fieldLabel1 = (Label)testTable.Rows[0].Cells[0].Controls[0];
			IBindTo valueControl1 = (IBindTo)testTable.Rows[0].Cells[1].Controls[0];
			fields.Add(fieldLabel1.Text, valueControl1.BindTo);

			Label fieldLabel2 = (Label)testTable.Rows[1].Cells[0].Controls[0];
			IBindTo valueControl2 = (ZTextLabel)testTable.Rows[1].Cells[1].Controls[0];
			fields.Add(fieldLabel2.Text, valueControl2.BindTo);

			Label fieldLabel3 = (Label)testTable.Rows[2].Cells[0].Controls[0];
			IBindTo valueControl3 = (IBindTo)testTable.Rows[2].Cells[1].Controls[0];
			fields.Add(fieldLabel3.Text, valueControl3.BindTo);

			Label fieldLabel4 = (Label)testTable.Rows[3].Cells[0].Controls[0];
			IBindTo valueControl4 = (IBindTo)testTable.Rows[3].Cells[1].Controls[0];
			fields.Add(fieldLabel4.Text, valueControl4.BindTo);

			Label fieldLabel5 = (Label)testTable.Rows[4].Cells[0].Controls[0];
			IBindTo valueControl5 = (IBindTo)testTable.Rows[4].Cells[1].Controls[0];
			fields.Add(fieldLabel5.Text, valueControl5.BindTo);

			Assert("The caption label must display the correct caption.", fields.ContainsKey(customLabels1.OT_Caption));
			AssertEquals("The value label must bind to the assoicated custom field.", Order.Schema.JD_FirstBuyerContact, fields[customLabels1.OT_Caption]);

			Assert("The caption label must display the correct caption.", fields.ContainsKey(customLabels2.OT_Caption));
			AssertEquals("The value label must bind to the assoicated custom field.", Order.Schema.JD_CustomFlag1, fields[customLabels2.OT_Caption]);

			Assert("The caption label must display the correct caption.", fields.ContainsKey(customLabels3.OT_Caption));
			AssertEquals("The value label must bind to the assoicated custom field.", Order.Schema.JD_CustomDecimal1, fields[customLabels3.OT_Caption]);

			Assert("The caption label must display the correct caption.", fields.ContainsKey(customLabels4.OT_Caption));
			AssertEquals("The value label must bind to the assoicated custom field.", Order.Schema.JD_CustomDate1, fields[customLabels4.OT_Caption]);

			Assert("The caption label must display the correct caption.", fields.ContainsKey(customLabels5.OT_Caption));
			AssertEquals("The value label must bind to the assoicated custom field.", Order.Schema.JD_CustomAttrib1, fields[customLabels5.OT_Caption]);
		}

		#endregion

		#region TestDocumentUrlFormatting

		public void TestDocumentUrlFormatting()
		{
			BasePageTester page = new BasePageTester();
			page.TestDataSource = GetTestDataSource();

			ZDataGrid documentsGrid = new ZDataGrid();

			page.OnLoad();

			page.SetupDocumentsGrid(documentsGrid);

			AssertDocumentGrid(documentsGrid);

			ZHyperLinkColumn documentsHyperLinkColumn = documentsGrid.Columns[3] as ZHyperLinkColumn;
			AssertNotNull("Last column of DocumentGrid should be a hyperlink column", documentsHyperLinkColumn);

			string expectedDocumentUrlFormatString = String.Format("{0}?Ref={{0}}&Doc={{1}}", eDocsRequestHandler.RequestHelper.BaseUrl);

			AssertEquals("DocumentsHyperLinkColumn DataNavigateUrlFormatString should contain the reference and Document",
				expectedDocumentUrlFormatString, documentsHyperLinkColumn.DataNavigateUrlFormatString);

			AssertEquals("DataNavigateUrlFields Length", 2, documentsHyperLinkColumn.DataNavigateUrlFields.Length);
			AssertEquals("First element in DataNavigateUrlFields", DocumentView.Schema.ParentPK, documentsHyperLinkColumn.DataNavigateUrlFields[0]);
			AssertEquals("Second element in DataNavigateUrlFields", DocumentView.Schema.StorageDocPK, documentsHyperLinkColumn.DataNavigateUrlFields[1]);
		}

		public void TestDocumentUrlFormattingForShipmentQuickView()
		{
			BasePageTester page = new BasePageTester();
			page.TestDataSource = GetTestDataSource();

			page.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			ZDataGrid documentsGrid = new ZDataGrid();

			page.OnLoad();

			page.SetupDocumentsGrid(documentsGrid);

			AssertDocumentGrid(documentsGrid);

			ZHyperLinkColumn documentsHyperLinkColumn = documentsGrid.Columns[3] as ZHyperLinkColumn;
			AssertNotNull("Last column of DocumentGrid should be a hyperlink column", documentsHyperLinkColumn);

			string expectedDocumentUrlFormatString = HttpContext.Current.Request.Url.ToString();

			AssertEquals("DocumentsHyperLinkColumn DataNavigateUrlFormatString should as expected", expectedDocumentUrlFormatString, documentsHyperLinkColumn.DataNavigateUrlFormatString);
			AssertEquals("First element in DocumentsHyperLinkColumn DataNavigateUrlFields length should be 0", 0, documentsHyperLinkColumn.DataNavigateUrlFields.Length);
		}

		#endregion

		#region TestSetupPackLinesGrid

		public void TestVolumeCalculatorForPackLines()
		{
			var page = new BasePageTester();
			var packsGrid = new ZDataGrid();
			page.SetupPackLinesGridForTesting(packsGrid, false, useVolumeCalculator: false);
			AssertNotEquals(packsGrid, page.VolumeCalcuatorGridAddOnForTesting.Grid);

			page.SetupPackLinesGridForTesting(packsGrid, false, useVolumeCalculator: true);
			AssertEquals(packsGrid, page.VolumeCalcuatorGridAddOnForTesting.Grid);
		}

		public void TestSetupPackLinesGridForShipment()
		{
			BasePageTester page = new BasePageTester();

			ZDataGrid packLinesGrid = new ZDataGrid();

			AssertEquals("Is not an anonymous login", false, page.SiteUser.IsShipmentQuickViewUser);

			page.SetupPackLinesGridForTesting(packLinesGrid, true);

			AssertEquals("PackLinesGrid should contain 16 columns", 16, packLinesGrid.Columns.Count);

			AssertEquals("Column #1 Title", "Pieces", packLinesGrid.Columns[0].HeaderText);
			AssertEquals("#Column #2 Title", "Pack Type", packLinesGrid.Columns[1].HeaderText);
			AssertEquals("Column #3 Title", "Length", packLinesGrid.Columns[2].HeaderText);
			AssertEquals("Column #4 Title", "Width", packLinesGrid.Columns[3].HeaderText);
			AssertEquals("Column #5 Title", "Height", packLinesGrid.Columns[4].HeaderText);
			AssertEquals("Column #6 Title", "UD", packLinesGrid.Columns[5].HeaderText);
			AssertEquals("Column #7 Title", "Weight", packLinesGrid.Columns[6].HeaderText);
			AssertEquals("Column #8 Title", "UQ", packLinesGrid.Columns[7].HeaderText);
			AssertEquals("Column #9 Title", "Volume", packLinesGrid.Columns[8].HeaderText);
			AssertEquals("Column #10 Title", "UQ", packLinesGrid.Columns[9].HeaderText);
			AssertEquals("Column #11 Title", "Description", packLinesGrid.Columns[10].HeaderText);
			AssertEquals("Column #12 Title", "Marks and Numbers", packLinesGrid.Columns[11].HeaderText);
			AssertEquals("Column #13 Title", "Line Price", packLinesGrid.Columns[12].HeaderText);
			AssertEquals("Column #14 Title", "Currency", packLinesGrid.Columns[13].HeaderText);
			AssertEquals("Column #15 Title", "Tariff Num.", packLinesGrid.Columns[14].HeaderText);
			AssertEquals("Column #16 Title", "Container", packLinesGrid.Columns[15].HeaderText);
		}

		public void TestSetupPackLinesGridForBookingEtc()
		{
			BasePageTester page = new BasePageTester();

			ZDataGrid packLinesGrid = new ZDataGrid();

			AssertEquals("Is not an anonymous login", false, page.SiteUser.IsShipmentQuickViewUser);

			page.SetupPackLinesGridForTesting(packLinesGrid, false);

			AssertEquals("PackLinesGrid should contain 14 columns", 14, packLinesGrid.Columns.Count);

			AssertEquals("Column #1 Title", "Pieces", packLinesGrid.Columns[0].HeaderText);
			AssertEquals("#Column #2 Title", "Pack Type", packLinesGrid.Columns[1].HeaderText);
			AssertEquals("Column #3 Title", "Length", packLinesGrid.Columns[2].HeaderText);
			AssertEquals("Column #4 Title", "Width", packLinesGrid.Columns[3].HeaderText);
			AssertEquals("Column #5 Title", "Height", packLinesGrid.Columns[4].HeaderText);
			AssertEquals("Column #6 Title", "UD", packLinesGrid.Columns[5].HeaderText);
			AssertEquals("Column #7 Title", "Weight", packLinesGrid.Columns[6].HeaderText);
			AssertEquals("Column #8 Title", "UQ", packLinesGrid.Columns[7].HeaderText);
			AssertEquals("Column #9 Title", "Volume", packLinesGrid.Columns[8].HeaderText);
			AssertEquals("Column #10 Title", "UQ", packLinesGrid.Columns[9].HeaderText);
			AssertEquals("Column #11 Title", "Description", packLinesGrid.Columns[10].HeaderText);
			AssertEquals("Column #12 Title", "Marks and Numbers", packLinesGrid.Columns[11].HeaderText);
			AssertEquals("Column #13 Title", "Line Price", packLinesGrid.Columns[12].HeaderText);
			AssertEquals("Column #14 Title", "Tariff Num.", packLinesGrid.Columns[13].HeaderText);
		}

		public void TestSetupPackLinesGridForDomesticBookingEtc()
		{
			BasePageTester page = new BasePageTester();

			ZDataGrid packLinesGrid = new ZDataGrid();

			AssertEquals("Is not an anonymous login", false, page.SiteUser.IsShipmentQuickViewUser);

			page.SetupPackLinesGridForTesting(packLinesGrid, false, true);

			AssertEquals("PackLinesGrid should contain 12 columns", 12, packLinesGrid.Columns.Count);

			AssertEquals("Column #1 Title", "Pieces", packLinesGrid.Columns[0].HeaderText);
			AssertEquals("#Column #2 Title", "Pack Type", packLinesGrid.Columns[1].HeaderText);
			AssertEquals("Column #3 Title", "Length", packLinesGrid.Columns[2].HeaderText);
			AssertEquals("Column #4 Title", "Width", packLinesGrid.Columns[3].HeaderText);
			AssertEquals("Column #5 Title", "Height", packLinesGrid.Columns[4].HeaderText);
			AssertEquals("Column #6 Title", "UD", packLinesGrid.Columns[5].HeaderText);
			AssertEquals("Column #7 Title", "Weight", packLinesGrid.Columns[6].HeaderText);
			AssertEquals("Column #8 Title", "UQ", packLinesGrid.Columns[7].HeaderText);
			AssertEquals("Column #9 Title", "Volume", packLinesGrid.Columns[8].HeaderText);
			AssertEquals("Column #10 Title", "UQ", packLinesGrid.Columns[9].HeaderText);
			AssertEquals("Column #11 Title", "Description", packLinesGrid.Columns[10].HeaderText);
			AssertEquals("Column #12 Title", "Marks and Numbers", packLinesGrid.Columns[11].HeaderText);
		}

		public void TestSetupPackLinesGridForShipmentWithAnonymousLogin()
		{
			BasePageTester page = new BasePageTester();
			OrgHeader currentOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, Env.CurrentCompany.OrganisationPK));
			AssertNotNull("Current Org", currentOrg);

			page.SiteUser.LoginSupportForTest(currentOrg.OH_Code);

			ZDataGrid packLinesGrid = new ZDataGrid();

			AssertEquals("Should be anonymous login", true, page.SiteUser.IsShipmentQuickViewUser);

			page.SetupPackLinesGridForTesting(packLinesGrid, true);

			AssertEquals("PackLinesGrid should contain 4 columns", 4, packLinesGrid.Columns.Count);

			AssertEquals("First Column Title", "Pieces", packLinesGrid.Columns[0].HeaderText);
			AssertEquals("Second Column Title", "Pack Type", packLinesGrid.Columns[1].HeaderText);
			AssertEquals("Third Column Title", "Description", packLinesGrid.Columns[2].HeaderText);
			AssertEquals("Fourth Column Title", "Container", packLinesGrid.Columns[3].HeaderText);
		}

		public void TestSetupPackLinesGridForBookingEtcWithAnonymousLogin()
		{
			BasePageTester page = new BasePageTester();
			OrgHeader currentOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, Env.CurrentCompany.OrganisationPK));
			AssertNotNull("Current Org", currentOrg);

			page.SiteUser.LoginSupportForTest(currentOrg.OH_Code);

			ZDataGrid packLinesGrid = new ZDataGrid();

			AssertEquals("Should be anonymous login", true, page.SiteUser.IsShipmentQuickViewUser);

			page.SetupPackLinesGridForTesting(packLinesGrid, false);

			AssertEquals("PackLinesGrid should contain 3 columns", 3, packLinesGrid.Columns.Count);

			AssertEquals("First Column Title", "Pieces", packLinesGrid.Columns[0].HeaderText);
			AssertEquals("Second Column Title", "Pack Type", packLinesGrid.Columns[1].HeaderText);
			AssertEquals("Third Column Title", "Description", packLinesGrid.Columns[2].HeaderText);
		}

		public void TestSetupPackLinesGridForDropDownDisplayStyle()
		{
			BasePageTester page = new BasePageTester();

			ZDataGrid packLinesGrid = new ZDataGrid();

			AssertEquals("Is not an anonymous login", false, page.SiteUser.IsShipmentQuickViewUser);

			page.SetupPackLinesGridForTesting(packLinesGrid, true, useVolumeCalculator: true);

			AssertEquals("Pack Type column style", OComboBoxDropDownStyle.DescriptionOnly, ((ZDropDownListColumn)packLinesGrid.Columns[1]).DisplayStyle);
			AssertEquals("Height UD column style", OComboBoxDropDownStyle.CodeOnly, ((ZDropDownListColumn)packLinesGrid.Columns[5]).DisplayStyle);
			AssertEquals("Weight UQ column style", OComboBoxDropDownStyle.CodeOnly, ((ZDropDownListColumn)packLinesGrid.Columns[7]).DisplayStyle);
			AssertEquals("Volume UQ column style", OComboBoxDropDownStyle.CodeOnly, ((ZDropDownListColumn)packLinesGrid.Columns[9]).DisplayStyle);
		}

		[ExpectNoExceptions]
		public void TestDocumentsGridHandlesNull()
		{
			var page = new BasePageTester();
			ZDataGrid documentsGrid = null;

			AssertNull("Precondition: DocumentsGrid is null", documentsGrid);
			page.SetupDocumentsGrid(documentsGrid);
		}

		public void TestDocumentsGridOnlyAddsOnePopupButton()
		{
			var page = new BasePageTester();
			var documentsGrid = new ZGrid();

			page.SetupDocumentsGrid(documentsGrid);
			page.SetupDocumentsGrid(documentsGrid);

			var buttons = documentsGrid.Container.Controls.OfType<eDocAttachPopup>();
			AssertEquals("Should still contain only 1 button", 1, buttons.Count());
		}

		#endregion

		#region Test Setup Warehouse Docket Containers Grid

		public void TestSetupWhsDocketContainersGrid()
		{
			BasePageTester page = new BasePageTester();

			ZDataGrid docketContainersGrid = new ZDataGrid();

			page.SetupWhsDocketContainersGrid(docketContainersGrid);

			AssertEquals("DocContainersGrid should contain 7 columns", 7, docketContainersGrid.Columns.Count);

			AssertEquals("First Column Title", "Container #", docketContainersGrid.Columns[0].HeaderText);
			AssertEquals("Second Column Title", "Seal #", docketContainersGrid.Columns[1].HeaderText);
			AssertEquals("Third Column Title", "Type", docketContainersGrid.Columns[2].HeaderText);
			AssertEquals("Fourth Column Title", "Palletized", docketContainersGrid.Columns[3].HeaderText);
			AssertEquals("Fifth Column Title", "Chargeable", docketContainersGrid.Columns[4].HeaderText);
			AssertEquals("Sixth Column Title", "Items", docketContainersGrid.Columns[5].HeaderText);
			AssertEquals("Seventh Column Title", "Pallets", docketContainersGrid.Columns[6].HeaderText);
		}

		#endregion

		#region Test Setup Receive Inventory Grid

		public void TestSetupReceiveInventoryGridCoreForEdit()
		{
			ZDataGrid testGrid = new ZDataGrid();
			testGrid.AllowEdit = true;

			AssertSetupReceiveInventoryGridCore(testGrid);
			DataGridColumn firstColumn = testGrid.Columns[0];
			AssertEquals("First Column should be a ZFindBoxColumn", typeof(ZFindBoxColumn), firstColumn.GetType());
		}

		public void TestSetupReceiveInventoryGridCoreForView()
		{
			ZDataGrid testGrid = new ZDataGrid();
			AssertEquals("Editing is not allowed", false, testGrid.AllowEdit);
			AssertEquals("Adding is not allowed", false, testGrid.AllowAdd);

			AssertSetupReceiveInventoryGridCore(testGrid);
			DataGridColumn firstColumn = testGrid.Columns[0];
			AssertEquals("First Column should be a ZHyperLinkColumn", typeof(ZHyperLinkColumn), firstColumn.GetType());
		}

		void AssertSetupReceiveInventoryGridCore(ZDataGrid testGrid)
		{
			var page = new BasePageTester();
			page.SetupReceiveInventoryGridCore(testGrid);

			AssertEquals("InventoryGrid should contain 7 columns", 7, testGrid.Columns.Count);

			AssertEquals("First Column Title", "Product", testGrid.Columns[0].HeaderText);
			AssertEquals("Second Column Title", "Description", testGrid.Columns[1].HeaderText);
			AssertEquals("Third Column Title", "Packs", testGrid.Columns[2].HeaderText);
			AssertEquals("Fourth Column Title", "Packs UQ", testGrid.Columns[3].HeaderText);
			AssertEquals("Fifth Column Title", "Expected Quantity", testGrid.Columns[4].HeaderText);
			AssertEquals("Fifth Column binds to Decimal Places", ((ZCalcEditColumn)testGrid.Columns[4]).BindToDecimals, "WhsReceiveLine.SupplierPart.OP_CountDecimalPlaces");
			AssertEquals("Sixth Column Title", "Quantity", testGrid.Columns[5].HeaderText);
			AssertEquals("Sixth Column binds to Decimal Places", ((ZCalcEditColumn)testGrid.Columns[5]).BindToDecimals, "WhsReceiveLine.SupplierPart.OP_CountDecimalPlaces");
			AssertEquals("Seventh Column Title", "UQ", testGrid.Columns[6].HeaderText);
		}

		public void TestSetupReceiveInventoryGrid_WithExpiryDateEnabledForOrg()
		{
			TestSetupReceiveInventory_WithExpiryDateCore(true);
		}

		public void TestSetupReceiveInventoryGrid_WithExpiryDateDisabledForOrg()
		{
			TestSetupReceiveInventory_WithExpiryDateCore(false);
		}

		void TestSetupReceiveInventory_WithExpiryDateCore(bool isExpiryDateEnabledForOrg)
		{
			var page = new BasePageTester();
			var testLoggedInOrg = Factory.New<OrgHeader>();
			testLoggedInOrg.OH_Code = "ABC";
			testLoggedInOrg.MiscServ.OM_IMUseExpiryDate = isExpiryDateEnabledForOrg;
			Factory.Save();

			page.SiteUser.LoginSupportForTest(testLoggedInOrg.OH_Code);

			var testGrid = new ZDataGrid();
			page.SetupReceiveInventoryGridCore(testGrid);

			AssertEquals("Grid contains the expiry date column.", isExpiryDateEnabledForOrg, testGrid.Columns.OfType<ZDateTimeColumn>().Any(column => column.HeaderText.Equals("Expiry Date")));
		}

		public void TestSetupReceiveInventoryGridCoreForEdit_WithSerialNumberEnabledAndClientUsesSerialNumber()
		{
			TestSetupReceiveInventoryGridCoreForEdit_WithSerialNumberCore(allowEdit: true, clientUsesSerialNumber: true);
		}

		public void TestSetupReceiveInventoryGridCoreForEdit_ClientDoesNotUseSerialNumber()
		{
			TestSetupReceiveInventoryGridCoreForEdit_WithSerialNumberCore(allowEdit: true, clientUsesSerialNumber: false);
		}

		public void TestSetupReceiveInventoryGridCoreForView_WithSerialNumberEnabledAndClientUsesSerialNumber()
		{
			TestSetupReceiveInventoryGridCoreForEdit_WithSerialNumberCore(allowEdit: false, clientUsesSerialNumber: true);
		}

		public void TestSetupReceiveInventoryGridCoreForView_ClientDoesNotUseSerialNumber()
		{
			TestSetupReceiveInventoryGridCoreForEdit_WithSerialNumberCore(allowEdit: false, clientUsesSerialNumber: false);
		}

		void TestSetupReceiveInventoryGridCoreForEdit_WithSerialNumberCore(bool allowEdit, bool clientUsesSerialNumber)
		{
			var page = new BasePageTester();

			var testLoggedInOrg = Factory.New<OrgHeader>();
			testLoggedInOrg.OH_Code = "ABC";
			testLoggedInOrg.MiscServ.OM_IMUseSerialNumber = clientUsesSerialNumber;
			Factory.Save();

			page.SiteUser.LoginSupportForTest(testLoggedInOrg.OH_Code);

			var testGrid = new ZDataGrid();
			testGrid.AllowEdit = allowEdit;

			page.SetupReceiveInventoryGridCore(testGrid);

			AssertEquals("Grid contains the serial number column.", clientUsesSerialNumber, testGrid.Columns.OfType<ZTemplateColumn>().Any(column => column.HeaderText.Equals("Serial Number")));
		}

		#endregion

		public void TestNewTrackingPortalLink()
		{
			var page = new BasePageTester();

			using (TrackingSiteUserTestHelper.EnableGlowTrackingPortalAccess(page.SiteUser))
			{
				page.OnInit();

				var loginStatus = page.LoginStatus;

				AssertEquals(0, loginStatus.AdditionalContent.Count); // Temporarily hidden - WI00238192
			}
		}

		public void TestNewTrackingPortalLink_NoAccess()
		{
			var page = new BasePageTester();

			page.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			page.OnInit();

			var loginStatus = page.LoginStatus;

			AssertEquals(0, loginStatus.AdditionalContent.Count);
		}

		#region Test Body Page Css Class

		public void TestBodyHasPageCssClass()
		{
			var page = new BasePageWithNameTester();
			var genericControl = new HtmlGenericControl() { ID = "DefaultBody" };
			page.Controls.Add(genericControl);

			page.OnInit();

			AssertEquals("BasePageTesterBody", genericControl.Attributes["class"]);
		}

		public void TestBodyIncludesExistingPageCssClass()
		{
			var page = new BasePageWithNameTester();
			var genericControl = new HtmlGenericControl() { ID = "DefaultBody" };
			genericControl.Attributes.Add("class", "SomeCssClass");
			page.Controls.Add(genericControl);

			page.OnInit();

			AssertEquals("BasePageTesterBody SomeCssClass", genericControl.Attributes["class"]);
		}

		public void TestBodyHasNoPageCssClassWhenPageNameNotSet()
		{
			var page = new BasePageTester();
			var genericControl = new HtmlGenericControl() { ID = "DefaultBody" };
			page.Controls.Add(genericControl);

			page.OnInit();

			AssertNull(genericControl.Attributes["class"]);
		}

		public void TestBodyHasNoPageCssClassWhenNoDefaultBody()
		{
			var page = new BasePageTester();
			var genericControl = new HtmlGenericControl() { ID = "NotDefaultBody" };
			page.Controls.Add(genericControl);

			page.OnInit();

			AssertNull(genericControl.Attributes["class"]);
		}

		#endregion

		public void TestPageRequiresLogin_NoUser()
		{
			var page = new BasePageTester(true);

			AssertEquals("LoginPage", false, page.PageRequiresLoginForTest(new Uri("http://localhost/" + page.AppInstance.LoginPage)));
			AssertEquals("ForgotPasswordPage", false, page.PageRequiresLoginForTest(new Uri("http://localhost/" + page.AppInstance.ForgotPasswordPage)));
			AssertEquals("ResetPasswordPage", false, page.PageRequiresLoginForTest(new Uri("http://localhost/" + page.AppInstance.ResetPasswordPage)));
			AssertEquals("SetPasswordPage", false, page.PageRequiresLoginForTest(new Uri("http://localhost/" + page.AppInstance.SetPasswordPage)));
			AssertEquals("ErrorPage", false, page.PageRequiresLoginForTest(new Uri("http://localhost/" + page.AppInstance.ErrorPage)));
			AssertEquals("TermsAndConditionsPage", false, page.PageRequiresLoginForTest(new Uri("http://localhost/" + page.AppInstance.TermsAndConditionsPage)));
			AssertEquals("LoginSupersededPage", false, page.PageRequiresLoginForTest(new Uri("http://localhost/" + page.AppInstance.LoginSupersededPage)));
			AssertEquals("LoginRedirectionPage", false, page.PageRequiresLoginForTest(new Uri("http://localhost/" + page.AppInstance.LoginRedirectionPage)));
			AssertEquals("SetMasterPasswordPage", false, page.PageRequiresLoginForTest(new Uri("http://localhost/" + page.AppInstance.SetMasterPasswordPage)));
			AssertEquals("LoginCompletePage", false, page.PageRequiresLoginForTest(new Uri("http://localhost/" + page.AppInstance.LoginCompletePage)));
			AssertEquals("CustomsExchangeRatesPage", true, page.PageRequiresLoginForTest(new Uri("http://localhost/" + page.AppInstance.CustomsExchangeRatesPage)));
			AssertEquals("ShipmentsPage", true, page.PageRequiresLoginForTest(new Uri("http://localhost/" + page.AppInstance.ShipmentsPage)));
			AssertEquals("FlightSchedulesPage", true, page.PageRequiresLoginForTest(new Uri("http://localhost/" + page.AppInstance.FlightSchedulesPage)));
			AssertEquals("SailingSchedulesPage", true, page.PageRequiresLoginForTest(new Uri("http://localhost/" + page.AppInstance.SailingSchedulesPage)));
			AssertEquals("RoadSchedulesPage", true, page.PageRequiresLoginForTest(new Uri("http://localhost/" + page.AppInstance.RoadSchedulesPage)));
			AssertEquals("RailSchedulesPage", true, page.PageRequiresLoginForTest(new Uri("http://localhost/" + page.AppInstance.RailSchedulesPage)));
		}

		public void TestLogModuleChange()
		{
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var page = new BasePageTester();
				var helper = new ZWebTestHelper(Factory);
				Factory.Save();
				page.SiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);

				page.OnLoad();

				Assert("Should log module change", page.SiteUser.LoggedInOrgContact.Logs.HasLogWith(x => x.SL_SE_NKEvent == AutoEvents.WebModuleAccessedModuleName.Code));
			}
		}

		public void TestLogModuleChange_QuickShipmentUser()
		{
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var page = new BasePageTester();

				page.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

				page.OnLoad();

				Assert("Should not log module change for ShipmentQuickViewUser", !page.SiteUser.LoggedInOrgContact.Logs.HasLogWith(x => x.SL_SE_NKEvent == AutoEvents.WebModuleAccessedModuleName.Code));
			}
		}

		public void TestCacheable_OnInit()
		{
			var initialCacheability = HttpContext.Current.Response.Cache.GetCacheability();
			var initialNoStore = HttpContext.Current.Response.Cache.GetNoStore();

			var page = new BasePageTester();
			page.OnInit();

			AssertEquals(initialCacheability, HttpContext.Current.Response.Cache.GetCacheability());
			AssertEquals(initialNoStore, HttpContext.Current.Response.Cache.GetNoStore());
		}

		public void TestNotCacheable_OnInit()
		{
			var page = new BasePageNotCacheable();
			page.OnInit();

			AssertEquals(HttpCacheability.NoCache, HttpContext.Current.Response.Cache.GetCacheability());
			Assert(HttpContext.Current.Response.Cache.GetNoStore());
		}

		#region Implementation

		BusinessObject GetTestDataSource()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();

			BaseJobDeclaration impDeclaration = Factory.New<BaseJobDeclaration>();
			impDeclaration.JE_MessageType = "IMP";
			impDeclaration.JE_JS = shipment.PK;
			impDeclaration.JE_OwnerRef = "IMP";

			StorageDocs storageDoc = (StorageDocs)shipment.DocumentHelper.PublishedEDocsAndFiles.AddNew();

			return shipment;
		}

		void AssertDocumentGrid(ZDataGrid documentsGrid)
		{
			AssertEquals("Documents should contain 4 columns", 4, documentsGrid.Columns.Count);
			AssertEquals("First column of DocumentGrid should be Date", "Date", documentsGrid.Columns[0].HeaderText);
			AssertEquals("Second column of DocumentGrid should be Description", "Description", documentsGrid.Columns[1].HeaderText);
			AssertEquals("Third column of DocumentGrid should be Type", "Type", documentsGrid.Columns[2].HeaderText);
			AssertEquals("Last column of DocumentGrid should be Empty", "", documentsGrid.Columns[3].HeaderText);
		}

		#endregion

		#region BasePageTester

		public class BasePageWithNameTester : BasePageTester
		{
			protected override string GetPageName()
			{
				return "BasePageTester";
			}
		}

		public class BasePageNotCacheable : BasePageTester
		{
			protected override bool Cacheable => false;
		}

		public class BasePageTester : BasePage
		{
			public BasePageTester(bool noSiteUser = false)
			{
				VolumeCalculatorGridAddOn = new VolumeCalculatorDataGridAddOn
				(JobPackLinesSchema.Constants.JL_PackageCount,
					JobPackLinesSchema.Constants.JL_Length,
					JobPackLinesSchema.Constants.JL_Width,
					JobPackLinesSchema.Constants.JL_Height,
					JobPackLinesSchema.Constants.JL_UnitOfDimension,
					JobPackLinesSchema.Constants.JL_ActualVolume,
					JobPackLinesSchema.Constants.JL_ActualVolumeUQ);

				this.noSiteUser = noSiteUser;
			}
			readonly bool noSiteUser;

			public VolumeCalculatorDataGridAddOn VolumeCalcuatorGridAddOnForTesting => VolumeCalculatorGridAddOn;

			protected override ZGlobal GetNewTestGlobal()
			{
				return noSiteUser ? new TestGlobalNoUser() : new TestGlobal();
			}

			protected override BusinessObject GetNewDataSource()
			{
				return TestDataSource;
			}
			public BusinessObject TestDataSource;

			public void OnLoad()
			{
				base.OnLoad(EventArgs.Empty);
			}

			public void OnInit()
			{
				base.OnInit(EventArgs.Empty);
			}

			public void OnPreRender()
			{
				base.OnPreRender(EventArgs.Empty);
			}

			public LoginStatus LoginStatus => loginStatus ?? (loginStatus = new LoginStatusForTest());
			LoginStatus loginStatus;

			protected override LoginStatus GetLoginStatus()
			{
				return LoginStatus;
			}

			protected override Uri RequestUrl
			{
				get { return new Uri("http://www.test.com/WebTracker/"); }
			}

			protected override BrowserType GetBrowserType()
			{
				return BrowserType.IE;
			}

			protected override bool ShouldTraverseControlTreeAndExtractOnInit => false;

			protected override ZString ModuleNameForEventLogging => "BasePageTester";

			public new TrackingSiteUser SiteUser
			{
				get { return base.SiteUser; }
			}

			public new void SetupDocumentsGrid(ZDataGrid documentsGrid)
			{
				base.SetupDocumentsGrid(documentsGrid);
			}

			public new ZGuid DocumentParentPK
			{
				get { return base.DocumentParentPK; }
			}

			public void SetupPackLinesGridForTesting(ZDataGrid packLinesGrid, bool shipmentPackLines, bool hideCustomsCodeAndPrice = false, bool useVolumeCalculator = true) => SetupPackLinesGrid(packLinesGrid, shipmentPackLines, hideCustomsCodeAndPrice, useVolumeCalculator);

			public new void SetupWhsDocketContainersGrid(ZDataGrid whsDocketContainerGrid)
			{
				base.SetupWhsDocketContainersGrid(whsDocketContainerGrid);
			}

			public new void SetupReceiveInventoryGridCore(ZDataGrid whsReceiveInventoryGrid, bool allowPostBack = true)
			{
				base.SetupReceiveInventoryGridCore(whsReceiveInventoryGrid, allowPostBack);
			}

			public void AddLineAttributeColumnsForTest(ZDataGrid grid, AttributeManager.AttributeModules module, OrgHeader relatedOrg)
			{
				AddLineAttributeColumns(grid, module, relatedOrg);
			}

			public void SetupAdditionalInformationTableForTest(CustomLabelInfoList customLabels, Table table, ZCollapsablePanel panel)
			{
				SetupAdditionalInformationTable(customLabels, table, panel);
			}
		}

		class LoginStatusForTest : LoginStatus
		{
			internal LoginStatusForTest()
			{
				AdditionalLoginContent = new HtmlGenericControl();
			}
		}
		class TestGlobalNoUser : TestGlobal
		{
			public override WebUser SiteUser => null;
		}

		#endregion
	}
}
