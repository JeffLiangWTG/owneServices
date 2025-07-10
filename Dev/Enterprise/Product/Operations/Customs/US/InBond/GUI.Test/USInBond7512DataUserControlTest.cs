using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.InBond.GUI.Testing
{
	sealed class USInBond7512DataUserControlTest : TestCaseWithFactory
	{
		public void TestPrint7512Departure()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			using (USInBondForm form = new USInBondForm(header))
			{
				form.Show();
				ZPanel mainPanel = (ZPanel)form.Controls["MainPanel"];
				ZTemplateTabControl mainTabControl = (ZTemplateTabControl)mainPanel.Controls["MainTabControl"];
				ZTabPage cBP7512TabPage = (ZTabPage)mainTabControl.Controls["CBP7512TabPage"];
				mainTabControl.SelectedTab = cBP7512TabPage;
				USInBond7512DataUserControl inBond7512DataUserControl = (USInBond7512DataUserControl)cBP7512TabPage.Controls["InBond7512DataUserControl"];
				AssertEquals(true, inBond7512DataUserControl.Visible);
				var cBP7512MoveHeaderAndDetailSplitContainer = (SplitContainer)inBond7512DataUserControl.Controls["CBP7512MoveHeaderAndDetailSplitContainer"];
				ZGroupBox cBP7512MoveHeaderGroupBox = (ZGroupBox)cBP7512MoveHeaderAndDetailSplitContainer.Panel1.Controls["CBP7512MoveHeaderGroupBox"];
				ZGrid cBP7512MoveHeaderGrid = (ZGrid)cBP7512MoveHeaderGroupBox.Controls["CBP7512MoveHeaderGrid"];
				MenuItem print7512DepartureItem = cBP7512MoveHeaderGrid.ContextMenu.MenuItems.FindByText(USInBond7512DataUserControl.Print7512DepartureItemName);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				print7512DepartureItem.PerformClick();
				AssertEquals("Data must saved before printing.", UnitTestUserNotification.Instance.LastMessage.Text);
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				print7512DepartureItem.PerformClick();
				AssertEquals("Please select at least one movement header to print.", UnitTestUserNotification.Instance.LastMessage.Text);
				cBP7512MoveHeaderGrid.SelectAllElements();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				print7512DepartureItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				ZGrid cBP7512MoveLineGrid = (ZGrid)cBP7512TabPage.Controls.Find("CBP7512MoveLineGrid", true)[0];
				AssertEquals("Description and Quantity of Merchandise", cBP7512MoveLineGrid.Columns["BI_Description"].ToString());
				SplitContainer cBP7512MoveDetailAndLineSplitContainer = (SplitContainer)cBP7512MoveHeaderAndDetailSplitContainer.Panel2.Controls["CBP7512MoveDetailAndLineSplitContainer"];
				var linesBottomPanel = (ZPanel)cBP7512MoveDetailAndLineSplitContainer.Panel2.Controls["LinesBottomPanel"];
				var defaultBondedWhsDataButton = (ZButton)linesBottomPanel.Controls["DefaultBondedWhsDataButton"];
				AssertEquals(defaultBondedWhsDataButton.Text, "Default Bonded Whs / FTZ Data");
			}
		}

		public void TestWarehouseDetail()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			header.BH_FTZMove = true;
			var helper = new WhsDataTestHelper(Factory);
			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_Code = "W#@33";
			var whsWarehouse = helper.GetNewWhsWarehouse(warehouse.MainAddress.PK, true, "W#@");
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.BM_OA_WarehouseAddress = warehouse.MainAddress.PK;
			var moveHeader2 = header.MovementHeaders.AddNew();
			using (var form = new USInBondForm(header))
			{
				form.Show();
				var mainPanel = (ZPanel)form.Controls["MainPanel"];
				var mainTabControl = (ZTemplateTabControl)mainPanel.Controls["MainTabControl"];
				var cBP7512TabPage = (ZTabPage)mainTabControl.Controls["CBP7512TabPage"];
				mainTabControl.SelectedTab = cBP7512TabPage;
				var inBond7512DataUserControl = (USInBond7512DataUserControl)cBP7512TabPage.Controls["InBond7512DataUserControl"];
				AssertEquals(true, inBond7512DataUserControl.Visible);
				var cBP7512MoveHeaderAndDetailSplitContainer = (SplitContainer)inBond7512DataUserControl.Controls["CBP7512MoveHeaderAndDetailSplitContainer"];
				var cBP7512MoveHeaderGroupBox = (ZGroupBox)cBP7512MoveHeaderAndDetailSplitContainer.Panel1.Controls["CBP7512MoveHeaderGroupBox"];
				var cBP7512MoveHeaderGrid = (ZGrid)cBP7512MoveHeaderGroupBox.Controls["CBP7512MoveHeaderGrid"];
				var cBP7512MoveDetailAndLineSplitContainer = (SplitContainer)cBP7512MoveHeaderAndDetailSplitContainer.Panel2.Controls["CBP7512MoveDetailAndLineSplitContainer"];
				var cBP7512MoveDetailAndWarehouseDetailSplitContainer = (SplitContainer)cBP7512MoveDetailAndLineSplitContainer.Panel1.Controls["CBP7512MoveDetailAndWarehouseDetailSplitContainer"];
				cBP7512MoveHeaderGrid.Select(1);
				AssertEquals("CBP7512MoveDetailAndWarehouseDetailSplitContainer.Panel2Collapsed", false, cBP7512MoveDetailAndWarehouseDetailSplitContainer.Panel2Collapsed);
				cBP7512MoveHeaderGrid.Select();
				AssertEquals("CBP7512MoveDetailAndWarehouseDetailSplitContainer.Panel2Collapsed", false, cBP7512MoveDetailAndWarehouseDetailSplitContainer.Panel2Collapsed);
				var mainTabPage = (ZTabPage)mainTabControl.Controls["MainTabPage"];
				mainTabControl.SelectedTab = mainTabPage;
				header.BH_FTZMove = false;
				mainTabControl.SelectedTab = cBP7512TabPage;
				cBP7512MoveHeaderGrid.Select(0);
				AssertEquals("CBP7512MoveDetailAndWarehouseDetailSplitContainer.Panel2Collapsed", true, cBP7512MoveDetailAndWarehouseDetailSplitContainer.Panel2Collapsed);
				mainTabControl.SelectedTab = mainTabPage;
				header.BH_FTZMove = true;
				mainTabControl.SelectedTab = cBP7512TabPage;
				cBP7512MoveHeaderGrid.Select(0);
				AssertEquals("CBP7512MoveDetailAndWarehouseDetailSplitContainer.Panel2Collapsed", false, cBP7512MoveDetailAndWarehouseDetailSplitContainer.Panel2Collapsed);
				mainTabControl.SelectedTab = mainTabPage;
				whsWarehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
				header.BH_FTZMove = false;
				header.BH_FTZMove = true; // visibility change
				mainTabControl.SelectedTab = cBP7512TabPage;
				cBP7512MoveHeaderGrid.Select(0);
				AssertEquals("CBP7512MoveDetailAndWarehouseDetailSplitContainer.Panel2Collapsed", true, cBP7512MoveDetailAndWarehouseDetailSplitContainer.Panel2Collapsed);
			}
		}
	}
}
