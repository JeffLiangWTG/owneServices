using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core.Modules;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.GUI.Testing
{
	[TestedType(typeof(USInBondForm))]
	sealed class USInBondFormTest : ZFormBasherTest
	{
		public void TestHiddentTabShouldNotCheckNotificationsFromChildren()
		{
			var header = Factory.New<CusInBondHeader>();
			using (var form = new USInBondForm(header))
			{
				form.Show();
				var tabControl = form.Controls.Find("MainTabControl", true)[0] as ZTabControl;
				tabControl.SelectTab(form.BillsTabPage);
				var billGrid = form.BillsTabPage.Controls.Find("BillsGrid", true)[0];
				header.BH_ImportTransportMode = "40";
				tabControl.SelectTab(form.AirBillsAndMovementsDetailsTabPage);
				NotificationBroadcaster.Instance.BroadcastVisibilityChange(billGrid);
				Assert(!form.BillsTabPage.TabVisible);
				NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.EntityFramework.NotificationType.MessageError, billGrid);
				Assert(!form.BillsTabPage.TabVisible);
			}
		}

		public void TestBondedWarehouseSecurityCheckOnShowPreSaveDialogs()
		{
			string expectedMessage = "You do not have security rights to save a Bonded Warehousing job. ";
			expectedMessage += Env.Security.USInBondEditBondedWarehouse.DisplayTextPathToSecurityRight;
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "W#@");
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = true;
			AssertEquals(false, header.HasAtLeastOneMovementWithWHSTransaction);
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			Env.Security.USInBondEditBondedWarehouse.IsAllowed = false;
			using (var form = new USInBondForm(header))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Env.Security.USInBondEditBondedWarehouse.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				AssertNotEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNoDuplicateMoveHeader_WI00052095()
		{
			var header = Factory.New<CusInBondHeader>();
			using (var form = new USInBondForm(header))
			{
				form.Show();
				var inbondHeaderControl = form.Controls.Find("InBondHeaderDetailsUserControl", true)[0] as USInBondHeaderDetailUserControl;
				var inbondHeaderGrid = inbondHeaderControl.Controls.Find("MovementHeadersGrid", true)[0] as ZGrid;
				var moveHeader = (US.Business.CusInBondMoveHeader)((IBindingList)header.MovementHeaders).AddNew();
				moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._3ImmediateExport;
				((ICancelAddNew)header.MovementHeaders).EndNew(0);
				AssertEquals(1, header.MovementHeaders.Count);
			}
		}

		public void TestFormCaptions()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_JobReference = "INB23423BD";
			Factory.Save();
			using (USInBondForm form = new USInBondForm(header))
			{
				form.Show();
				AssertContains(header.HumanReadableName, form.FormCaption);
			}
		}

		public void TestPlugIns()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			using (USInBondForm form = new USInBondForm(header))
			{
				form.Show();
				AssertNotNull(ControllerIDs.eDocsPlugIn.Name, form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
			}
		}

		public void TestBillsTabControlsWhenAMSHBREffective()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;

			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "MMAATT2";
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				using (USInBondForm form = new USInBondForm(header))
				{
					var billsTab = form.Controls.Find("BillsTabPage", true);
					form.Show();
					var billsTabPage = billsTab[0] as ZTabPage;
					billsTabPage.Show();

					var inbondBillsUserControl = billsTabPage.Controls[0] as USInBondBillsUserControl;
					inbondBillsUserControl.Show();

					var issuerControl = inbondBillsUserControl.Controls.Find("B0_HouseBillIssuerCodeCodeFindBox", true)[0] as ZCodeFindBox;
					AssertEquals("Hide HouseBillIsserCodeFindBox", false, issuerControl.Visible);
					AssertEquals("Hide B0_HouseBillNumberTextBox", false, inbondBillsUserControl.B0_HouseBillNumberTextBox.Visible);
				}
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				using (USInBondForm form = new USInBondForm(header))
				{
					var billsTab = form.Controls.Find("BillsTabPage", true);
					form.Show();
					var billsTabPage = billsTab[0] as ZTabPage;
					billsTabPage.Show();

					var inbondBillsUserControl = billsTabPage.Controls[0] as USInBondBillsUserControl;
					inbondBillsUserControl.Show();

					var issuerControl = inbondBillsUserControl.Controls.Find("B0_HouseBillIssuerCodeCodeFindBox", true)[0] as ZCodeFindBox;
					AssertEquals("Show HouseBillIsserCodeFindBox", true, issuerControl.Visible);
					AssertEquals("Show B0_HouseBillNumberTextBox", true, inbondBillsUserControl.B0_HouseBillNumberTextBox.Visible);
				}
			}
		}

			public void TestControlsVisibility()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			using (USInBondForm form = new USInBondForm(header))
			{
				form.Show();
				// AIR
				header.BH_ImportTransportMode = "40";
				AssertEquals("BillsTabPage is hidden", 0, form.Controls.Find("BillsTabPage", true).Length);
				AssertEquals("MovementDetailsTabPage is hidden", 0, form.Controls.Find("MovementDetailsTabPage", true).Length);
				AssertEquals("AirBillsAndMovementsDetailsTabPage is shown", 1, form.Controls.Find("AirBillsAndMovementsDetailsTabPage", true).Length);
				// NONAIR
				header.BH_ImportTransportMode = "10";
				AssertEquals("BillsTabPage is shown", 1, form.Controls.Find("BillsTabPage", true).Length);
				AssertEquals("MovementDetailsTabPage is shown", 1, form.Controls.Find("MovementDetailsTabPage", true).Length);
				AssertEquals("AirBillsAndMovementsDetailsTabPage is hidden", 0, form.Controls.Find("AirBillsAndMovementsDetailsTabPage", true).Length);
			}
		}

		public void TestSaveToRecentItems()
		{
			var header = Factory.New<CusInBondHeader>();
			Factory.Save();

			using (var form = new USInBondForm(header))
			{
				form.ControllerID = ControllerIDs.Customs.US.InBond;
				form.Show();
				Application.DoEvents();

				var favoriteProvider = ObjectFactory.Get<IFavoriteProvider>();
				var linkWrapper = ZFormUtilities.GetLinkWrapperForFavoriteOrRecent(form);
				Assert(RecentItemManager.Instance.IsInRecentItems(linkWrapper.ModuleName, linkWrapper));
				RecentItemManager.Instance.RemoveAllRecentItems(linkWrapper.ModuleName);
			}

			using (var form = new USInBondForm(header))
			{
				form.ControllerID = ControllerIDs.Customs.US.InBond;
				form.SkipRecentItems = true;
				form.Show();
				Application.DoEvents();

				var favoriteProvider = ObjectFactory.Get<IFavoriteProvider>();
				var linkWrapper = ZFormUtilities.GetLinkWrapperForFavoriteOrRecent(form);
				Assert(!RecentItemManager.Instance.IsInRecentItems(linkWrapper.ModuleName, linkWrapper));
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_FTZMove = true;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var cargoDesc = container.Commodities.AddNew();
			header.SelectedMovementHeader = moveHeader.PK;
			Factory.Save();
			return new USInBondForm(header)
			{
				ControllerID = ControllerIDs.Customs.US.InBond
			};
		}
	}
}
