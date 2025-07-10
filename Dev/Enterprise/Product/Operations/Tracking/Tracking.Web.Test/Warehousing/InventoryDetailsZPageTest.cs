using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class InventoryDetailsZPageTest : ZPageTestCase
	{
		public void TestReferenceVisibility()
		{
			var docketRef = TestPage.DocketRefForTest;
			var receiptRef = TestPage.ReceiptRefLinkForTest;
			var docket = TestPage.InventoryForTest.Docket;

			TestPage.OnLoad(EventArgs.Empty);
			TestPage.DataBind();

			docket.WD_DocketType = DocketType.Codes.Receive;
			TestPage.OnPreBindForTest();
			Assert(receiptRef.Visible);
			Assert(!docketRef.Visible);

			docket.WD_DocketType = DocketType.Codes.Adjustment;
			TestPage.OnPreBindForTest();
			Assert(!receiptRef.Visible);
			Assert(docketRef.Visible);
		}

		public void TestCrossDockedOrderLinesGrid()
		{
			AssertColumnIsInGrid("Order No", 0, typeof(ZHyperLinkColumn));
			AssertColumnIsInGrid("Product", 1, typeof(ZFindBoxColumn));
			AssertColumnIsInGrid("Description", 2, typeof(ZTextEditColumn));
			AssertColumnIsInGrid("Quantity", 3, typeof(ZCalcEditColumn));
			AssertEquals("Inventory+SupplierPart+OP_CountDecimalPlaces", ((ZCalcEditColumn)TestPage.CrossDockedOrderLinesGridForTest.Columns[3]).BindToDecimals);
			AssertColumnIsInGrid("Expiry Date", 4, typeof(ZDateTimeColumn));
			AssertColumnIsInGrid("Packing Date", 5, typeof(ZDateTimeColumn));
		}

		public void TestCrossDockedOrderLinesGrid_WithSerialNumberEnabledAndClientUsesSerialNumber()
		{
			TestCrossDockedOrderLinesGrid_WithSerialNumberCore(clientUsesSerialNumber: true);
		}

		public void TestCrossDockedOrderLinesGrid_ClientDoesNotUseSerialNumber()
		{
			TestCrossDockedOrderLinesGrid_WithSerialNumberCore(clientUsesSerialNumber: false);
		}

		void TestCrossDockedOrderLinesGrid_WithSerialNumberCore(bool clientUsesSerialNumber)
		{
			var testLoggedInOrg = Factory.New<OrgHeader>();
			testLoggedInOrg.OH_Code = "ABC";
			testLoggedInOrg.MiscServ.OM_IMUseSerialNumber = clientUsesSerialNumber;
			Factory.Save();

			var inventoryDetailspage = new InventoryDetailsForTest(testLoggedInOrg.OH_Code);
			var crossDockedOrderLinesGrid = inventoryDetailspage.CrossDockedOrderLinesGridForTest;
			AssertEquals(clientUsesSerialNumber, crossDockedOrderLinesGrid.Columns.OfType<ZTemplateColumn>().Any(column => column.HeaderText.Equals("Serial Number")));
		}

		public void TestSetupCustomAttributes_WithSerialNumberEnabledAndClientUsesSerialNumber()
		{
			TestSetupCustomAttributes_WithSerialNumberCore(clientUsesSerialNumber: true);
		}

		public void TestSetupCustomAttributes_ClientDoesNotUseSerialNumber()
		{
			TestSetupCustomAttributes_WithSerialNumberCore(clientUsesSerialNumber: false);
		}

		void TestSetupCustomAttributes_WithSerialNumberCore(bool clientUsesSerialNumber)
		{
			var testLoggedInOrg = Factory.New<OrgHeader>();
			testLoggedInOrg.OH_Code = "ABC";
			testLoggedInOrg.MiscServ.OM_IMUseSerialNumber = clientUsesSerialNumber;
			Factory.Save();

			var inventoryDetailspage = new InventoryDetailsForTest(testLoggedInOrg.OH_Code);
			inventoryDetailspage.SetupCustomAttributes();
			AssertEquals(clientUsesSerialNumber, inventoryDetailspage.IsSerialNumberEnabledForTest);
			if (clientUsesSerialNumber)
			{
				AssertEquals("Serial Number", inventoryDetailspage.SerialNumberCaptionForTest);
			}
		}

		void AssertColumnIsInGrid(ZString headerText, int columnIndex, Type columnType)
		{
			var gridForTesting = TestPage.CrossDockedOrderLinesGridForTest;

			if (columnIndex < gridForTesting.Columns.Count)
			{
				bool columnIsInGrid = (gridForTesting.Columns[columnIndex].HeaderText.Equals(headerText));
				Assert(ZString.Format("Column '{0}' is not in the grid at index {1} as expected.", headerText, columnIndex), columnIsInGrid);
				AssertEquals(ZString.Format("Column '{0}' type", headerText), columnType, gridForTesting.Columns[columnIndex].GetType());
			}
			else
			{
				Fail(ZString.Format("The expected index {0} of column '{1}' is out of range.", columnIndex, headerText));
			}
		}

		InventoryDetailsForTest TestPage => Page as InventoryDetailsForTest;

		protected override ZPage GetNewZPage()
		{
			return new InventoryDetailsForTest("EDICUS");
		}
	}
}
