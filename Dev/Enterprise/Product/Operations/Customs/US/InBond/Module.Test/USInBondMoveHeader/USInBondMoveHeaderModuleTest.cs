using CargoWise.EntityFramework;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Customs.US.InBond.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Module.Testing
{
	[TestedType(typeof(USInBondMoveHeaderModule))]
	public class USInBondMoveHeaderModuleTest : ZModuleBasherTest
	{
		public void TestActionMenuOperationalActions()
		{
			using (var module = new CusInBondHeaderModule())
			{
				var grid = module.DisplayGrid as ZDisplayGrid;
				var actionMenu = grid.ContextMenu.MenuItems.FindByText("Actions");
				actionMenu.ShowPopupMenu();
				var operationalActionsMenu = actionMenu.MenuItems.FindByText("Operational Actions");
				AssertNotNull(operationalActionsMenu);
			}
		}

		public void TestLicenceAndSecurityCheckPoint()
		{
			using (var module = new USInBondMoveHeaderModule())
			{
				AssertEquals("Licence Checkpoint", Env.Licence.ImportBroker, module.LicenceCheckPoint);
				AssertEquals("SecurityCheckpoint", Env.Security.USInBond, module.SecurityCheckpoint);
			}
		}

		public void TestUSInBondMoveHeaderAllows()
		{
			using (var module = new USInBondMoveHeaderModule())
			{
				AssertEquals("module.AllowNew", false, module.AllowNew);
				AssertEquals("module.AllowEdit", true, module.AllowEdit);
				AssertEquals("module.AllowDelete", false, module.AllowDelete);
			}
		}

		public void TestSendArrivalMessagesMenuItem()
		{
			var inBondHeader1 = Factory.New<CusInBondHeader>();
			var movementHeader1 = inBondHeader1.MovementHeaders.AddNew();
			var movementHeader2 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader2.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureAmendment;
			movementHeader2.BM_MessageStatus = ImportMessageStatusList.Codes.AwaitingArrival;
			var movementHeader3 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader3.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureAmendment;
			movementHeader3.BM_MessageStatus = ImportMessageStatusList.Codes.ClearArrival;
			var inBondHeader2 = Factory.New<CusInBondHeader>();
			inBondHeader2.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			var movementHeader4 = inBondHeader2.MovementHeaders.AddNew();
			Factory.Save();

			using (var module = (USInBondMoveHeaderModule)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.PerformSearch_ForTest();
				module.DisplayGrid.SelectAllElements();
				AssertEquals(4, module.GridCollection.Count);
				AssertEquals(4, module.GetSelectedBusinessObjects().Length);

				var bulkSendMessagesMenuItem = module.ActionsMenuItem.MenuItems.FindByText("Bulk Send Messages") as ZMenuItem;
				var menuItem = bulkSendMessagesMenuItem.MenuItems.FindByText("Send Arrival Messages") as ZMenuItem;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.USInBondMessaging.IsAllowed = false;
				menuItem.PerformClick();
				AssertContains("The user was not allowed to send the message", "You do not have the appropriate security rights to run this function", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Env.Security.USInBondMessaging.IsAllowed = true;
				using (ZFormModaliser.SuspendDispose())
				{
					menuItem.PerformClick();
					var sendMessagesForm = (SendInBondMessageMenuItemForm)ZFormModaliser.LastFormShownDialogForTest;
					var entity = sendMessagesForm.BusinessEntity;
					AssertEquals("There is 1 movement header", 1, entity.InBondMenuItemMessageSendingObjects.Count);
				}
			}
		}

		public void TestSendExportationMessagesMenuItem()
		{
			var inBondHeader1 = Factory.New<CusInBondHeader>();
			var movementHeader1 = inBondHeader1.MovementHeaders.AddNew();
			var movementHeader2 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader2.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureAmendment;
			movementHeader2.BM_MessageStatus = ImportMessageStatusList.Codes.AwaitingArrival;
			var movementHeader3 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader3.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureAmendment;
			movementHeader3.BM_MessageStatus = ImportMessageStatusList.Codes.ClearArrival;
			var inBondHeader2 = Factory.New<CusInBondHeader>();
			inBondHeader2.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			var movementHeader4 = inBondHeader2.MovementHeaders.AddNew();
			Factory.Save();

			using (var module = (USInBondMoveHeaderModule)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.PerformSearch_ForTest();
				module.DisplayGrid.SelectAllElements();
				AssertEquals(4, module.GridCollection.Count);
				AssertEquals(4, module.GetSelectedBusinessObjects().Length);

				var bulkSendMessagesMenuItem = module.ActionsMenuItem.MenuItems.FindByText("Bulk Send Messages") as ZMenuItem;
				var menuItem = bulkSendMessagesMenuItem.MenuItems.FindByText("Send Export Messages") as ZMenuItem;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.USInBondMessaging.IsAllowed = false;
				menuItem.PerformClick();
				AssertContains("The user was not allowed to send the message", "You do not have the appropriate security rights to run this function", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Env.Security.USInBondMessaging.IsAllowed = true;
				using (ZFormModaliser.SuspendDispose())
				{
					menuItem.PerformClick();
					var sendMessagesForm = (SendInBondMessageMenuItemForm)ZFormModaliser.LastFormShownDialogForTest;
					var entity = sendMessagesForm.BusinessEntity;
					AssertEquals("There is 1 movement header", 1, entity.InBondMenuItemMessageSendingObjects.Count);
				}
			}
		}

		public void TestAllocatePedimentoNumberMenuItem()
		{
			var inBondHeader1 = Factory.New<CusInBondHeader>();
			var movementHeader1 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader1.InBondNumber = "111111111";
			var movementHeader2 = inBondHeader1.MovementHeaders.AddNew();
			var inBondHeader2 = Factory.New<CusInBondHeader>();
			var movementHeader3 = inBondHeader2.MovementHeaders.AddNew();
			movementHeader3.InBondNumber = "111111112";
			Factory.Save();

			using (var module = (USInBondMoveHeaderModule)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.PerformSearch_ForTest();
				module.DisplayGrid.SelectAllElements();
				AssertEquals(3, module.GridCollection.Count);
				AssertEquals(3, module.GetSelectedBusinessObjects().Length);

				Env.Security.USInBondEdit.IsAllowed = false;
				var menuItem = module.ActionsMenuItem.MenuItems.FindByText("Allocate Pedimento Number") as ZMenuItem;
				menuItem.PerformClick();
				AssertContains("The user was not allowed to edit the in-bond header", "You do not have the appropriate security rights to run this function", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Env.Security.USInBondEdit.IsAllowed = true;
				using (ZFormModaliser.SuspendDispose())
				{
					menuItem.PerformClick();
					var sendMessagesForm = (SendInBondMessageMenuItemForm)ZFormModaliser.LastFormShownDialogForTest;
					var entity = sendMessagesForm.BusinessEntity;
					AssertEquals("There is 3 movement header", 3, entity.InBondMenuItemMessageSendingObjects.Count);
				}
			}
		}

		public void TestBulkPrintDocumentMenuItem()
		{
			var inBondHeader1 = Factory.New<CusInBondHeader>();
			var movementHeader1 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader1.InBondNumber = "111111111";
			Factory.Save();

			using (var module = (USInBondMoveHeaderModule)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.PerformSearch_ForTest();
				module.DisplayGrid.SelectAllElements();
				AssertEquals(1, module.GridCollection.Count);
				AssertEquals(1, module.GetSelectedBusinessObjects().Length);
				var menuItem = module.ActionsMenuItem.MenuItems.FindByText("Bulk Print 7512 Departure Document") as ZMenuItem;
				using (ZFormModaliser.SuspendDispose())
				{
					menuItem.PerformClick();
					var sendMessagesForm = (SendInBondMessageMenuItemForm)ZFormModaliser.LastFormShownDialogForTest;
					var entity = sendMessagesForm.BusinessEntity;
					AssertEquals("There is 1 movement header", 1, entity.InBondMenuItemMessageSendingObjects.Count);
				}
			}
		}

		protected override BusinessObject GetBusinessObjectForHyperlinking(ZFilterGridModule module)
		{
			var inBondHeader1 = Factory.New<CusInBondHeader>();
			var movement = inBondHeader1.MovementHeaders.AddNew();
			Factory.Save();
			return Factory.Load<USInBondMoveHeader>(movement.PK);
		}

		protected override void BashModule(ZFilterModule module1)
		{
			GetBusinessObjectForHyperlinking(null);
			base.BashModule(module1);
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true);
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.InBondMoveHeader;

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override bool HasController() => true;
	}
}
