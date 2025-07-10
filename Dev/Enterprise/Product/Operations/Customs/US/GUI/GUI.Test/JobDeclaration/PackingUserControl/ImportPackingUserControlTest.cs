using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class ImportPackingUserControlTest : TestCaseWithFactory
	{
		public void TestRemoveGridColumnsThatAreNotRelevantForImportUS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				userControl.MainTabControl.SelectedTab = userControl.PackingTabPage;
				var packingUserControl = (ImportPackingUserControl)userControl.Packing;
				AssertNull("CU_GUIPresentationRecord should not appear", packingUserControl.HouseBillsGrid.Columns[CusDecHouseBillSchema.CU_GUIPresentationRecord.Name]);
				AssertNull("CU_IssueDate should not appear", packingUserControl.HouseBillsGrid.Columns[CusDecHouseBillSchema.CU_IssueDate.Name]);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				userControl.MainTabControl.SelectedTab = userControl.PackingTabPage;
				var packingUserControl = (ImportPackingUserControl)userControl.Packing;
				AssertNull("Split indicator should not appears", packingUserControl.HouseBillsGrid.Columns[Bill.Schema.US_SESplitShip]);
			}

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				userControl.MainTabControl.SelectedTab = userControl.PackingTabPage;
				var packingUserControl = (ImportPackingUserControl)userControl.Packing;
				packingUserControl.BillOfLadingTabControl.SelectedTab = packingUserControl.ITNosSplitTabPage;
				AssertNotNull("Split indicator should be visible for AIR FTZ", packingUserControl.HouseBillsGrid.Columns[Bill.Schema.US_SESplitShip]);
				AssertNotNull("Split details columns should be visible", packingUserControl.itAndSplitDetailsUserControl.ITNumbersGrid.Columns[ITAndSplitDetails.Schema.US_ArrivalDate]);
				AssertNotNull("Split details columns should be visible", packingUserControl.itAndSplitDetailsUserControl.ITNumbersGrid.Columns[ITAndSplitDetails.Schema.US_CarrierCode]);
				AssertNotNull("Split details columns should be visible", packingUserControl.itAndSplitDetailsUserControl.ITNumbersGrid.Columns[ITAndSplitDetails.Schema.US_FlightNumber]);
			}
		}

		public void TestSplitDetailsColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				userControl.MainTabControl.SelectedTab = userControl.PackingTabPage;
				var packingUserControl = (ImportPackingUserControl)userControl.Packing;
				packingUserControl.BillOfLadingTabControl.SelectedTab = packingUserControl.ITNosSplitTabPage;
				var itGrid = packingUserControl.itAndSplitDetailsUserControl.ITNumbersGrid;
				AssertNotNull(itGrid.Columns[ITAndSplitDetails.Schema.US_ArrivalDate]);
				AssertNotNull(itGrid.Columns[ITAndSplitDetails.Schema.US_CarrierCode]);
				AssertNotNull(itGrid.Columns[ITAndSplitDetails.Schema.US_FlightNumber]);
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
				AssertNull(itGrid.Columns[ITAndSplitDetails.Schema.US_ArrivalDate]);
				AssertNull(itGrid.Columns[ITAndSplitDetails.Schema.US_CarrierCode]);
				AssertNull(itGrid.Columns[ITAndSplitDetails.Schema.US_FlightNumber]);
			}
		}

		public void TestITNumberColumn()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				userControl.MainTabControl.SelectedTab = userControl.PackingTabPage;
				var packingUserControl = (ImportPackingUserControl)userControl.Packing;
				var itGrid = packingUserControl.HouseBillsGrid;
				AssertNotNull(itGrid.Columns["ITNumber"]);
			}
		}

		public void TestRemoveContainerNumberWhenAirNotContainerised()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				CustomsBrokerageUserControl userControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				userControl.MainTabControl.SelectedTab = userControl.PackingTabPage;
				ImportPackingUserControl packingUserControl = (ImportPackingUserControl)userControl.Packing;
				AssertNotNull(packingUserControl.PackagesGrid.Columns[Package.Schema.CW_ContainerNoOrEquipmentNo]);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
				AssertNull(packingUserControl.PackagesGrid.Columns[Package.Schema.CW_ContainerNoOrEquipmentNo]);
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
				AssertNotNull(packingUserControl.PackagesGrid.Columns[Package.Schema.CW_ContainerNoOrEquipmentNo]);
				AssertEquals(Package.Schema.CW_HouseBill, packingUserControl.PackagesGrid.Columns[0].ColumnStyle.MappingName);
				AssertEquals(Package.Schema.CW_ContainerNoOrEquipmentNo, packingUserControl.PackagesGrid.Columns[1].ColumnStyle.MappingName);
			}
		}

		public void TestTrackingNumberColumn()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				userControl.MainTabControl.SelectedTab = userControl.PackingTabPage;
				var packingUserControl = (ImportPackingUserControl)userControl.Packing;
				var houseBillsGrid = packingUserControl.HouseBillsGrid;
				AssertNotNull(houseBillsGrid.Columns["US_ExpressTracking"]);
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertNull(houseBillsGrid.Columns["US_ExpressTracking"]);
				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertNotNull(houseBillsGrid.Columns["US_ExpressTracking"]);
			}
		}

		public void TestPTTUniqueIDControlVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				userControl.MainTabControl.SelectedTab = userControl.PackingTabPage;
				var packingUserControl = (ImportPackingUserControl)userControl.Packing;
				AssertEquals(false, packingUserControl.FTZTabPage.TabVisible);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				userControl.MainTabControl.SelectedTab = userControl.PackingTabPage;
				var packingUserControl = (ImportPackingUserControl)userControl.Packing;
				AssertEquals(true, packingUserControl.FTZTabPage.TabVisible);
				packingUserControl.BillOfLadingTabControl.SelectedTab = packingUserControl.FTZTabPage;
				var pttUniqueIDTextBox = packingUserControl.FTZTabPage.Controls.Find("PTTUniqueIDTextBox", true)[0];
				AssertEquals(true, pttUniqueIDTextBox.Visible);
			}
		}
	}
}
