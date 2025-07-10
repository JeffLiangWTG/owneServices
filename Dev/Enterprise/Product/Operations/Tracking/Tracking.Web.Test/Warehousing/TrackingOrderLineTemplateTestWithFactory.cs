using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class TrackingOrderLineTemplateTestWithFactory : TestCaseWithFactory
	{
		public void TestGetInventoriesExcelExportColumns()
		{
			var orderLine = Factory.New<WhsOrderLine>();
			var releaseLine = orderLine.ReleaseLines.AddNew("A-123", "", "Green", "Serial", new ZDate(2008, 07, 11), new ZDate(2008, 01, 29));

			var collection = new TrackingWhsReleaseLineCollection(Factory);
			collection.Add(new TrackingWhsReleaseLine(releaseLine, null, 0));
			AssertEquals("All columns excluding part columns, docket attribute columns, View and Allocate should be persent", 7, TrackingOrderLineTemplate.GetWhsOrderLinesExcelExportColumns(collection).Count);
		}

		public void TestGetInventoriesExcelExportColumns_WithSerialNumberEnabledAndClientUsesSerialNumber()
		{
			TestGetInventoriesExcelExportColumns_WithSerialNumberCore(clientUsesSerialNumber: true);
		}

		public void TestGetInventoriesExcelExportColumns_ClientDoesNotUseSerialNumber()
		{
			TestGetInventoriesExcelExportColumns_WithSerialNumberCore(clientUsesSerialNumber: false);
		}

		void TestGetInventoriesExcelExportColumns_WithSerialNumberCore(bool clientUsesSerialNumber)
		{
			var testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);
			testHelper.TestSiteUser.LoggedInOrganisation.MiscServ.OM_IMUseSerialNumber = clientUsesSerialNumber;

			var orderLine = Factory.New<WhsOrderLine>();
			var releaseLine = orderLine.ReleaseLines.AddNew("A-123", "", "Green", "Serial", new ZDate(2008, 07, 11), new ZDate(2008, 01, 29));

			var collection = new TrackingWhsReleaseLineCollection(Factory);
			collection.Add(new TrackingWhsReleaseLine(releaseLine, null, 0));
			AssertEquals(clientUsesSerialNumber, TrackingOrderLineTemplate.GetWhsOrderLinesExcelExportColumns(collection).Any(column => column.Description == "Serial Number"));
		}

		public void TestGetControl_WithSerialNumberEnabledAndClientUsesSerialNumber()
		{
			TestGetControl_WithSerialNumberCore(clientUsesSerialNumber: true);
		}

		public void TestGetControl_ClientDoesNotUseSerialNumber()
		{
			TestGetControl_WithSerialNumberCore(clientUsesSerialNumber: false);
		}

		void TestGetControl_WithSerialNumberCore(bool clientUsesSerialNumber)
		{
			var factory = new BusinessObjectFactory();
			var testHelper = new TestHelper(factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);
			testHelper.TestSiteUser.LoggedInOrganisation.MiscServ.OM_IMUseSerialNumber = clientUsesSerialNumber;

			var cell = new TableCell();
			var template = new TrackingOrderLineTemplate(new ZNewRowColumn("Test"));
			template.InstantiateIn(cell);
			var control = cell.Controls[0] as ISelfBindingWebControl;
			var grid = control as ZDataGrid;
			AssertEquals(clientUsesSerialNumber, grid.Columns.OfType<ZTemplateColumn>().Any(column => column.HeaderText == "Serial Number"));
		}
	}
}
