using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	[SetGlobalsIsWeb]
	[HttpContextEnabledTest]
	sealed class TrackingInventoryTemplateTestWithFactory : TestCaseWithFactory
	{
		#region TestAdditionalInformationColuns

		class DummyTrackingInventoryTemplate : TrackingInventoryTemplate
		{
			public DummyTrackingInventoryTemplate() : base(new ZNewRowColumn(ZString.Empty)) { }

			public DataGridColumnCollection AddCustomColumns(OrgHeader loggedInOrg)
			{
				ZDataGrid grid = new ZDataGrid();
				AddCustomColumns(grid, loggedInOrg);
				return grid.Columns;
			}
		}

		public void TestAdditionalInformationColumns()
		{
			var testLoggedInOrg = Factory.NewWithValidTestData<OrgHeader>();

			GlbCompany.CurrentCompany.OrgProxy.CustomLabels.RemoveAndDeleteAll();
			testLoggedInOrg.CustomLabels.RemoveAndDeleteAll();

			var attribute1 = GlbCompany.CurrentCompany.OrgProxy.CustomLabels.AddNew();
			attribute1.OT_FieldName = Constants.CustomLabels.WhsDocketLine.CustomAttribute3;
			attribute1.OT_Caption = "OrgProxy'sCA3";

			var attribute2 = testLoggedInOrg.CustomLabels.AddNew();
			attribute2.OT_FieldName = Constants.CustomLabels.WhsDocketLine.CustomAttribute3;
			attribute2.OT_Caption = "LoggedInOrg'sCA3";

			var attribute3 = GlbCompany.CurrentCompany.OrgProxy.CustomLabels.AddNew();
			attribute3.OT_FieldName = Constants.CustomLabels.WhsDocketLine.CustomFlag4;
			attribute3.OT_Caption = "OrgProxy'sCF4";

			var columns = new DummyTrackingInventoryTemplate().AddCustomColumns(testLoggedInOrg);
			AssertEquals("Should be 1 column", 1, columns.Count);
			AssertEquals("LoggedInOrg'sCA3", columns[0].HeaderText);
		}

		public void TestGetInventoriesExcelExportColumns()
		{
			AssertEquals("All columns excluding part columns, docket attribute columns, View and Allocate should be persent", 25,
				TrackingInventoryTemplate.GetInventoriesExcelExportColumns(new TrackingWhsInventoryCollection(Factory) { Factory.NewWithValidTestData<TrackingWhsInventory>() }).Count);
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
			var testLoggedInOrg = Factory.New<OrgHeader>();
			testLoggedInOrg.OH_Code = "ABC";
			testLoggedInOrg.MiscServ.OM_IMUseSerialNumber = clientUsesSerialNumber;
			Factory.Save();

			var user = new TrackingSiteUser();
			user.LoginSupportForTest(testLoggedInOrg.OH_Code);
			((DummyHttpApplication)WebEnv.AppInstance).SetSiteUser(user);

			var inventoryExcelExportColumns = TrackingInventoryTemplate.GetInventoriesExcelExportColumns(new TrackingWhsInventoryCollection(Factory) { Factory.NewWithValidTestData<TrackingWhsInventory>() });
			AssertEquals(clientUsesSerialNumber, inventoryExcelExportColumns.Any(column => column.Description == "Serial Number"));
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
			var testLoggedInOrg = Factory.New<OrgHeader>();
			testLoggedInOrg.OH_Code = "ABC";
			testLoggedInOrg.MiscServ.OM_IMUseSerialNumber = clientUsesSerialNumber;
			Factory.Save();

			var user = new TrackingSiteUser();
			user.LoginSupportForTest(testLoggedInOrg.OH_Code);
			((DummyHttpApplication)WebEnv.AppInstance).SetSiteUser(user);

			var cell = new TableCell();
			var template = new TrackingInventoryTemplate(new ZNewRowColumn("Test"));
			template.InstantiateIn(cell);
			var control = cell.Controls[0] as ISelfBindingWebControl;
			var grid = control as ZDataGrid;
			AssertEquals(clientUsesSerialNumber, grid.Columns.OfType<ZTemplateColumn>().Any(column => column.HeaderText == "Serial Number"));
		}

		#endregion
	}
}
