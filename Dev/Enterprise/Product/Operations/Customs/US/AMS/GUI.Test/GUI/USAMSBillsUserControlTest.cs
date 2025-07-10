using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.AMS.GUI.Testing
{
	sealed class USAMSBillsUserControlTest : TestCaseWithFactory
	{
		public void TestHasChanges()
		{
			var header = Factory.New<CusInBondHeader>();
			using (var form = new USAMSForm(header))
			{
				form.Show();
				var mainTabControl = form.Controls.Find("MainTabControl", true)[0] as ZTemplateTabControl;
				mainTabControl.SelectTab(1);
				Assert(!header.MovementHeader.HasChanges);
				Assert(header.HasChanges); //uncommitted object in a collection somewhere is causing changes to exist
			}
		}

		public void TestOverrideDefaultValuesMenuItem()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var container = bill.MovementDetail.Containers.AddNew();
			container.BC_ContainerNum = "NC";
			var commodity = container.Commodities.AddNew();
			using (var form = new ZForm(header))
			{
				var userControl = new USAMSConsolManifestUserControl(header);
				form.Controls.Add(userControl);
				form.Show();
				Application.DoEvents();
				userControl.SelectAndShowBill(bill.PK);
				var billsUserControl = userControl.FindSingle<USAMSBillsUserControl>("BillsDetailsUserControl");
				var billDetailsTabControl = userControl.FindSingle<ZTabControl>("BillDetailsTabControl");
				var cargoDetailsTabPage = billDetailsTabControl.FindSingle<ZTabPage>("CargoDetailsTabPage");
				billDetailsTabControl.SelectedTab = cargoDetailsTabPage;
				var billContainerCommoditiesGrid = cargoDetailsTabPage.FindSingle<ZGrid>("BillContainerCommoditiesGrid");
				AssertEquals(true, billContainerCommoditiesGrid.Visible);
				billContainerCommoditiesGrid.ContextMenu.OnPopup_ForTest();
				AssertNull("Override Default Menu should not be visible on stand alone", billContainerCommoditiesGrid.ContextMenu.MenuItems.FindByText("Override Default Values"));
			}

			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			using (var form = new ZForm(header))
			{
				var userControl = new USAMSConsolManifestUserControl(header);
				form.Controls.Add(userControl);
				form.Show();
				Application.DoEvents();
				userControl.SelectAndShowBill(bill.PK);
				var billsUserControl = userControl.FindSingle<USAMSBillsUserControl>("BillsDetailsUserControl");
				var billDetailsTabControl = userControl.FindSingle<ZTabControl>("BillDetailsTabControl");
				var cargoDetailsTabPage = billDetailsTabControl.FindSingle<ZTabPage>("CargoDetailsTabPage");
				billDetailsTabControl.SelectedTab = cargoDetailsTabPage;
				var billContainerCommoditiesGrid = cargoDetailsTabPage.FindSingle<ZGrid>("BillContainerCommoditiesGrid");
				AssertEquals(true, billContainerCommoditiesGrid.Visible);
				billContainerCommoditiesGrid.ContextMenu.OnPopup_ForTest();
				var overrideMenuItem = billContainerCommoditiesGrid.ContextMenu.MenuItems.FindByText("Override Default Values");
				AssertEquals(true, overrideMenuItem.Visible);
				AssertEquals(false, overrideMenuItem.Checked);
				overrideMenuItem.PerformClick();
				AssertEquals(true, overrideMenuItem.Checked);
				AssertEquals(true, header.BH_OverrideFreightDefaults);
				overrideMenuItem.PerformClick();
				AssertEquals(false, overrideMenuItem.Checked);
				AssertEquals(false, header.BH_OverrideFreightDefaults);
				header.BH_OverrideFreightDefaults = true;
				AssertEquals(true, overrideMenuItem.Checked);
			}
		}

		public void TestPortOfLadingChanged()
		{
			var header = Factory.New<CusInBondHeader>();
			header.PortArrivalDetails.AddNewIfNotExist("1101", ZDateTime.Today);
			var bill = header.Bills.AddNew();
			bill.MovementDetail.B9_CustomsStatus = "FIL";
			bill.B0_InBondPortOfDestDCode = "1101";
			using (var form = new ZForm(header))
			{
				var userControl = new USAMSConsolManifestUserControl(header);
				form.Controls.Add(userControl);
				form.Show();
				Application.DoEvents();
				var billsUserControl = GetControl<USAMSConsolManifestUserControl, USAMSBillsUserControl>(userControl, "BillsDetailsUserControl");
				var grid = GetControl<USAMSBillsUserControl, ZGrid>(billsUserControl, "BillsGrid");
				userControl.SelectAndShowBill(bill.PK);
				AssertEquals("should have selected bill1", bill, grid.ListManager.GetCurrent());
				bill.B0_InBondPortOfDestDCode = "1103";
				AssertEquals("Actual Arrival Date will be cleared. Continue?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMessageTabPageExist()
		{
			var header = Factory.New<CusInBondHeader>();
			using (var form = new ZForm(header))
			{
				var userControl = new USAMSConsolManifestUserControl(header);
				form.Controls.Add(userControl);
				form.Show();
				var billsUserControl = GetControl<USAMSConsolManifestUserControl, USAMSBillsUserControl>(userControl, "BillsDetailsUserControl");
				var tabPage = GetControl<USAMSBillsUserControl, ZTabPage>(billsUserControl, "MessagesTabPage");
				AssertNotNull("should have MessagesTabPage", tabPage);

				var splitContainer = tabPage.FindSingle<KSplitContainer>("MessagesTabPageSplitContainer");
				AssertNotNull("should have MessagesTabPageSplitContainer", splitContainer);

				var gird = splitContainer.FindSingle<ZGrid>("CusInBondBillMessagesGrid");
				var messageTabControl = splitContainer.FindSingle<ZTabControl>("CusInBondBillMessageTabControl");
				AssertNotNull("should have CusInBondBillMessagesGrid", gird);
				AssertNotNull("should have CusInBondBillMessageTabControl", messageTabControl);

				var messageDetailsTabPage = messageTabControl.FindSingle<ZTabPage>("CusInBondBillMessageDetailsTabPage");
				var messageTextTabPage = messageTabControl.FindSingle<ZTabPage>("CusInBondBillMessageTextTabPage");
				AssertNotNull("should have CusInBondBillMessageDetailsTabPage", messageDetailsTabPage);
				AssertNotNull("should have CusInBondBillMessageTextTabPage", messageTextTabPage);

				var messageDetailsTextBox = messageDetailsTabPage.FindSingle<ZTextBox>("CusInBondBillMessageDetailsTextBox");
				var messageTextTextBox = messageTextTabPage.FindSingle<ZTextBox>("CusInBondBillFormattedMessageTextTextBox");
				AssertNotNull("should have CusInBondBillMessageDetailsTextBox", messageDetailsTextBox);
				AssertNotNull("should have CusInBondBillFormattedMessageTextTextBox", messageTextTextBox);
			}
		}

		public void TestMessageTabPageBinding()
		{
			var header = Factory.New<CusInBondHeader>();
			header.PortArrivalDetails.AddNewIfNotExist("1101", ZDateTime.Today);
			var bill = header.Bills.AddNew();
			bill.MovementDetail.B9_CustomsStatus = "FIL";
			bill.B0_InBondPortOfDestDCode = "1101";
			var message = bill.Messages.AddNew();
			using (var form = new ZForm(header))
			{
				var userControl = new USAMSConsolManifestUserControl(header);
				form.Controls.Add(userControl);
				form.Show();
				var billsUserControl = GetControl<USAMSConsolManifestUserControl, USAMSBillsUserControl>(userControl, "BillsDetailsUserControl");
				userControl.SelectAndShowBill(bill.PK);
				var billDetailsTabControl = userControl.FindSingle<ZTabControl>("BillDetailsTabControl");
				var tabPage = billDetailsTabControl.FindSingle<ZTabPage>("MessagesTabPage");
				billDetailsTabControl.SelectedTab = tabPage;
				var messageGrid = tabPage.FindSingle<ZGrid>("CusInBondBillMessagesGrid");
				AssertEquals("should have message", message, messageGrid.ListManager.GetCurrent());
			}
		}

		public void TestMessageGridStructure()
		{
			var header = Factory.New<CusInBondHeader>();
			using (var form = new ZForm(header))
			{
				var userControl = new USAMSConsolManifestUserControl(header);
				form.Controls.Add(userControl);
				form.Show();
				var billsUserControl = GetControl<USAMSConsolManifestUserControl, USAMSBillsUserControl>(userControl, "BillsDetailsUserControl");
				var gird = billsUserControl.FindSingle<ZGrid>("CusInBondBillMessagesGrid");
				AssertNotNull("should have MessagesTabPage", gird);
				var columns = gird.ColumnStyles;
				AssertEquals(13, columns.Count);

				AssertEquals("EM_MessageNum", ((ZGridColumnInfo)columns[0]).ColumnName);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), columns[0].GetType());

				AssertEquals("EM_MessageType", ((ZGridColumnInfo)columns[1]).ColumnName);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), columns[1].GetType());

				AssertEquals("EM_MessageSubType", ((ZGridColumnInfo)columns[2]).ColumnName);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), columns[2].GetType());

				AssertEquals("EM_MessageDateTime", ((ZGridColumnInfo)columns[3]).ColumnName);
				AssertEquals(typeof(ZDateEditColumnStyleInfo), columns[3].GetType());

				AssertEquals("EM_SystemCreateTimeUtc", ((ZGridColumnInfo)columns[4]).ColumnName);
				AssertEquals(typeof(ZDateEditColumnStyleInfo), columns[4].GetType());

				AssertEquals("EM_InterchangeNumber", ((ZGridColumnInfo)columns[5]).ColumnName);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), columns[5].GetType());

				AssertEquals("EM_DateTimeInterchangeSent", ((ZGridColumnInfo)columns[6]).ColumnName);
				AssertEquals(typeof(ZDateEditColumnStyleInfo), columns[6].GetType());

				AssertEquals("EM_User", ((ZGridColumnInfo)columns[7]).ColumnName);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), columns[7].GetType());

				AssertEquals("EM_Status", ((ZGridColumnInfo)columns[8]).ColumnName);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), columns[8].GetType());

				AssertEquals("EM_InterchangeStatus", ((ZGridColumnInfo)columns[9]).ColumnName);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), columns[9].GetType());

				AssertEquals("EM_ReceiveTransmit", ((ZGridColumnInfo)columns[10]).ColumnName);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), columns[10].GetType());

				AssertEquals("EM_MessageSubTypeDescription", ((ZGridColumnInfo)columns[11]).ColumnName);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), columns[11].GetType());

				AssertEquals("EM_SendWithMessageErrorsFormatted", ((ZGridColumnInfo)columns[12]).ColumnName);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), columns[12].GetType());
			}
		}

		T GetControl<P, T>(P parent, string name)
			where P : Control
			where T : Control
		{
			return (T)typeof(P).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(parent);
		}
	}
}
