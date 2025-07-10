using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers.DocumentSending.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CommercialInvoiceEDIMenuTest : TestCaseWithFactory
	{
		public void TestMenuItemsVisibility()
		{
			using (var menu = new CommercialInvoiceEDIMenu())
			{
				var header = Factory.New<BaseJobComInvoiceHeader>();
				menu.Declaration = new FakeDeclarationCreatorForInvoice(header).HeaderData;
				menu.ShowPopupMenu();
				var copyPrevisouInvoiceLineMenuItem = menu.MenuItems.FindByText("&Copy Previous Invoice Line");
				var autoApportionWeightMenuItem = menu.MenuItems.FindByText("Auto Apportion &Weight");
				var lockCustomsFileMenuItem = menu.MenuItems.FindByText("&Lock Commercial Invoice");
				var visibleMenuItemsCount = menu.MenuItems.ToList<ZMenuItem>().Count(x => x.Visible);
				AssertEquals(4, visibleMenuItemsCount);
				Assert(copyPrevisouInvoiceLineMenuItem.Visible);
				Assert(autoApportionWeightMenuItem.Visible);
				Assert("lockCustomsFileMenuItem.Visible", lockCustomsFileMenuItem.Visible);
				MenuAssertion.AssertHasMenu(menu, "Allocate Remaining Weight");
				var allocateRemainingWeighMenu = menu.MenuItems.FindByText("Allocate Remaining Weight");
				Assert(allocateRemainingWeighMenu.Visible);
				MenuAssertion.AssertHasMenu(allocateRemainingWeighMenu, "Allocate Remaining Weight by Price");
				MenuAssertion.AssertHasMenu(allocateRemainingWeighMenu, "Allocate Remaining Weight by Quantity");
			}
		}

		public void TestAddLockOrUnlockCustomsFileMenusOnceOnly()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			using (var form = new ZForm())
			{
				var menu = new CommercialInvoiceEDIMenu();
				menu.Declaration = declaration1;
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.ShowPopupMenu();
				AssertEquals("menu.MenuItems.Count", 5, menu.MenuItems.Count);
				var lockCustomsFileMenuItem = menu.MenuItems.FindByText("&Lock Commercial Invoice");
				var unlockCustomsFileMenuItem = menu.MenuItems.FindByText("&Unlock Commercial Invoice");
				AssertNotNull("lockCustomsFileMenuItem", lockCustomsFileMenuItem);
				AssertNotNull("unlockCustomsFileMenuItem", unlockCustomsFileMenuItem);
				menu.Declaration = declaration1;
				menu.ShowPopupMenu();
				AssertEquals("menu.MenuItems.Count", 5, menu.MenuItems.Count);
				AssertSame("lockCustomsFileMenuItem", lockCustomsFileMenuItem, menu.MenuItems.FindByText("&Lock Commercial Invoice"));
				AssertSame("unlockCustomsFileMenuItem", unlockCustomsFileMenuItem, menu.MenuItems.FindByText("&Unlock Commercial Invoice"));
			}
		}

		public void TestFireSaveButton()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			using (var form = new ZForm(declaration))
			{
				var menu = new CommercialInvoiceEDIMenu();
				menu.Declaration = declaration;
				form.Menu.MenuItems.Add(menu);
				form.Show();
				declaration.JE_GB = ZGuid.Invalid;
				AssertEquals("FireSaveButton", false, menu.FireSaveButton());
				AssertEquals("declaration.IsInDatabase", false, declaration.IsInDatabase);
				declaration.JE_GB = GlbBranch.CurrentBranch.PK;
				AssertEquals("FireSaveButton", true, menu.FireSaveButton());
				AssertEquals("declaration.IsInDatabase", true, declaration.IsInDatabase);
			}
		}

		public void TestAutoApportionWeightMenuItem()
		{
			using (var form = new ZForm())
			{
				var menu = new CommercialInvoiceEDIMenu();
				menu.Declaration = Factory.New<BaseJobDeclaration>();
				form.Menu.MenuItems.Add(menu);
				form.Show();
				var autoApportionWeightMenuItem = menu.MenuItems.FindByText("Auto Apportion &Weight");
				AssertNotNull(autoApportionWeightMenuItem);
				AssertEquals(false, autoApportionWeightMenuItem.Checked);
				AssertEquals(false, menu.Declaration.JE_AutoWeightApportion);
				var allocateRemainingWeighMenu = menu.MenuItems.FindByText("Allocate Remaining Weight");
				AssertEquals(true, allocateRemainingWeighMenu.Visible);
				autoApportionWeightMenuItem.PerformClick();
				AssertEquals(true, autoApportionWeightMenuItem.Checked);
				AssertEquals(true, menu.Declaration.JE_AutoWeightApportion);
				AssertEquals(true, allocateRemainingWeighMenu.Visible);
				autoApportionWeightMenuItem.PerformClick();
				AssertEquals(false, autoApportionWeightMenuItem.Checked);
				AssertEquals(false, menu.Declaration.JE_AutoWeightApportion);
				AssertEquals(true, allocateRemainingWeighMenu.Visible);
			}
		}

		public void TestCopyPreviousInvoiceLineMenuItem()
		{
			using (CustomsDataRegistry.Instance.AlwaysCopyFromPreviousLine.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				using (var form = new ZForm())
				{
					var declaration = BaseJobDeclaration.New(Factory);
					declaration.Invoices.AddNew();
					var menu = new CommercialInvoiceEDIMenu();
					menu.Declaration = declaration;
					form.Menu.MenuItems.Add(menu);
					form.Show();
					var copyPreviousInvoiceLineMenuItem = menu.MenuItems.FindByText("&Copy Previous Invoice Line");
					AssertNotNull(copyPreviousInvoiceLineMenuItem);
					AssertEquals(true, copyPreviousInvoiceLineMenuItem.Checked);
					AssertEquals(true, menu.Declaration.JE_CopyLastInvoiceLineDetailsToNewLines);
					AssertEquals(true, menu.Declaration.CopyLastLineDetailsToNewLines);
					copyPreviousInvoiceLineMenuItem.PerformClick();
					AssertEquals(false, copyPreviousInvoiceLineMenuItem.Checked);
					AssertEquals(false, menu.Declaration.JE_CopyLastInvoiceLineDetailsToNewLines);
					AssertEquals(false, menu.Declaration.CopyLastLineDetailsToNewLines);
					copyPreviousInvoiceLineMenuItem.PerformClick();
					AssertEquals(true, copyPreviousInvoiceLineMenuItem.Checked);
					AssertEquals(true, menu.Declaration.JE_CopyLastInvoiceLineDetailsToNewLines);
					AssertEquals(true, menu.Declaration.CopyLastLineDetailsToNewLines);
				}
			}

			using (CustomsDataRegistry.Instance.AlwaysCopyFromPreviousLine.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				using (var form = new ZForm())
				{
					var declaration = BaseJobDeclaration.New(Factory);
					declaration.Invoices.AddNew();
					var menu = new CommercialInvoiceEDIMenu();
					menu.Declaration = declaration;
					form.Menu.MenuItems.Add(menu);
					form.Show();
					var copyPreviousInvoiceLineMenuItem = menu.MenuItems.FindByText("&Copy Previous Invoice Line");
					AssertNotNull(copyPreviousInvoiceLineMenuItem);
					AssertEquals(false, copyPreviousInvoiceLineMenuItem.Checked);
					AssertEquals(false, menu.Declaration.JE_CopyLastInvoiceLineDetailsToNewLines);
					AssertEquals(false, menu.Declaration.CopyLastLineDetailsToNewLines);
					copyPreviousInvoiceLineMenuItem.PerformClick();
					AssertEquals(true, copyPreviousInvoiceLineMenuItem.Checked);
					AssertEquals(true, menu.Declaration.JE_CopyLastInvoiceLineDetailsToNewLines);
					AssertEquals(true, menu.Declaration.CopyLastLineDetailsToNewLines);
					copyPreviousInvoiceLineMenuItem.PerformClick();
					AssertEquals(false, copyPreviousInvoiceLineMenuItem.Checked);
					AssertEquals(false, menu.Declaration.JE_CopyLastInvoiceLineDetailsToNewLines);
					AssertEquals(false, menu.Declaration.CopyLastLineDetailsToNewLines);
				}
			}
		}

		public void TestLockOrUnlockCustomsFileMenuItems_Visible()
		{
			AssertMenuItemVisible(AutoEvents.LockForEdit, "lockCustomsFileMenuItem", false);
			AssertMenuItemVisible(AutoEvents.UnlockForEdit, "unlockCustomsFileMenuItem", false);

			AssertMenuItemVisible(AutoEvents.UnlockForEdit, "lockCustomsFileMenuItem", true);
			AssertMenuItemVisible(AutoEvents.LockForEdit, "unlockCustomsFileMenuItem", true);
		}

		void AssertMenuItemVisible(Event eventType, string menuName, bool isVisible)
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var log = invoice.Logs.AddNew(eventType, ZDateTimeOffset.Now);
			log.IsCancelled = false;

			invoice.Factory.Save();

			var originalValue = Env.Security.LockOrUnlockFileForEdit.IsAllowed;
			var securityAction = new DisposableAction(() => { Env.Security.LockOrUnlockFileForEdit.IsAllowed = true; }, () => { Env.Security.LockOrUnlockFileForEdit.IsAllowed = originalValue; });

			using (securityAction)
			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				var menu = form.Menu.MenuItems.FindByText("&Brokerage");
				var menuItem = menu.MenuItems.FindByName(menuName);
				AssertEquals(isVisible, menuItem != null && menuItem.Visible);
			}
		}

		public void TestLockOrUnlockCustomsFileMenuItems_Click()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			Factory.Save();

			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				var menu = form.Menu.MenuItems.FindByText("&Brokerage");
				var lockCustomsFileMenuItem = menu.MenuItems.FindByName("lockCustomsFileMenuItem");
				var unlockCustomsFileMenuItem = menu.MenuItems.FindByName("unlockCustomsFileMenuItem");

				lockCustomsFileMenuItem.PerformClick();

				var log = invoice.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.LockForEditCode).First();
				Assert("Should contains the active LCK event.", !log.IsCancelled);

				Factory.Save();

				unlockCustomsFileMenuItem.PerformClick();

				log = invoice.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.UnlockForEditCode).First();
				Assert("Should contains the actived UCK event.", !log.IsCancelled);
			}
		}

		public void TestBrokerageMenuItemsEnabled_WhenLockAndUnlock()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			Factory.Save();

			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				var menu = (CommercialInvoiceEDIMenu)form.Menu.MenuItems.FindByText("&Brokerage");
				var autoApportionWeightMenuItem = menu.MenuItems.FindByText("Auto Apportion &Weight");
				var copyPrevisouInvoiceLineMenuItem = menu.MenuItems.FindByText("&Copy Previous Invoice Line");
				var allocateRemainingWeighMenu = menu.MenuItems.FindByText("Allocate Remaining Weight");
				var lockCustomsFileMenuItem = menu.MenuItems.FindByName("lockCustomsFileMenuItem");
				var unlockCustomsFileMenuItem = menu.MenuItems.FindByName("unlockCustomsFileMenuItem");
				menu.ShowPopupMenu();
				AssertMenuItem(autoApportionWeightMenuItem, true, true);
				AssertMenuItem(copyPrevisouInvoiceLineMenuItem, true, true);
				AssertMenuItem(allocateRemainingWeighMenu, true, true);
				AssertMenuItem(lockCustomsFileMenuItem, true, true);
				AssertMenuItem(unlockCustomsFileMenuItem, false, false);

				lockCustomsFileMenuItem.PerformClick();
				menu.ShowPopupMenu();
				Factory.Save();
				AssertMenuItem(autoApportionWeightMenuItem, true, false);
				AssertMenuItem(copyPrevisouInvoiceLineMenuItem, true, false);
				AssertMenuItem(allocateRemainingWeighMenu, true, false);
				AssertMenuItem(lockCustomsFileMenuItem, false, false);
				AssertMenuItem(unlockCustomsFileMenuItem, true, true);

				unlockCustomsFileMenuItem.PerformClick();
				menu.ShowPopupMenu();
				AssertMenuItem(autoApportionWeightMenuItem, true, true);
				AssertMenuItem(copyPrevisouInvoiceLineMenuItem, true, true);
				AssertMenuItem(allocateRemainingWeighMenu, true, true);
				AssertMenuItem(lockCustomsFileMenuItem, true, true);
				AssertMenuItem(unlockCustomsFileMenuItem, false, false);
			}
		}

		void AssertMenuItem(MenuItem menuItem, bool visible, bool enabled)
		{
			var text = menuItem.Text;
			AssertEquals(text + " Visible", visible, menuItem.Visible);
			AssertEquals(text + " Enabled", enabled, menuItem.Enabled);
		}

		public void TestAllocateRemainingWeightByPriceMenuItem()
		{
			AssertAllocateRemainingWeight("Allocate Remaining Weight by Price", (line, value) =>
			{
				line.JI_LinePrice = value;
			});
		}

		public void TestAllocateRemainingWeightByQuantityMenuItem()
		{
			AssertAllocateRemainingWeight("Allocate Remaining Weight by Quantity", (line, value) =>
			{
				line.JI_InvoiceQuantity = value;
			});
		}

		void AssertAllocateRemainingWeight(string menuText, Action<BaseJobComInvoiceLine, ZDecimal> setInvoiceLineValueAction)
		{
			using (var form = new ZForm())
			{
				var menu = new CommercialInvoiceEDIMenu();
				form.Menu.MenuItems.Add(menu);
				form.Show();
				var notificationCollector = new MessageNotificationCollector_ForTest();
				var declaration = Factory.New<BaseJobDeclaration>();
				menu.Declaration = declaration;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_NetWeight = 1000M;
				invoiceHeader.JZ_Weight = 10000M;
				invoiceHeader.JZ_InvoiceNumber = "INV1";
				declaration.JE_AutoWeightApportion = false;
				var line1 = invoiceHeader.JobComInvoiceLines.AddNew();
				line1.JI_InvoiceUQ = "PCS";
				line1.JI_NetWeight = 600M;
				line1.JI_Weight = 6000M;
				var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
				line2.JI_InvoiceUQ = "PCS";
				line2.JI_NetWeight = 0M;
				line2.JI_Weight = 0M;
				var line3 = invoiceHeader.JobComInvoiceLines.AddNew();
				line3.JI_InvoiceUQ = "PCS";
				line3.JI_NetWeight = 0M;
				line3.JI_Weight = 0M;
				setInvoiceLineValueAction.Invoke(line1, 1M);
				setInvoiceLineValueAction.Invoke(line2, 1M);
				setInvoiceLineValueAction.Invoke(line3, 2M);
				var allocateRemainingWeighMenu = menu.MenuItems.FindByText("Allocate Remaining Weight");
				var menuItem = allocateRemainingWeighMenu.MenuItems.FindByText(menuText);
				menuItem.PerformClick();
				CombineAssertions(() =>
				{
					AssertNull(notificationCollector.LastMessage);
					AssertEquals("line1.JI_Weight", 6000M, line1.JI_Weight);
					AssertEquals("line2.JI_Weight", 1333.333M, line2.JI_Weight);
					AssertEquals("line3.JI_Weight", 2666.667M, line3.JI_Weight);
					AssertEquals("line1.JI_NetWeight", 600M, line1.JI_NetWeight);
					AssertEquals("line2.JI_NetWeight", 133.333M, line2.JI_NetWeight);
					AssertEquals("line3.JI_NetWeight", 266.667M, line3.JI_NetWeight);
				});
				declaration.JE_AutoWeightApportion = true;
				menu.Declaration = declaration;
				menu.ShowPopupMenu();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menuItem.PerformClick();
				AssertEquals("'Auto Apportion Weight' will be turned off before allocating weight. Do you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menuItem.PerformClick();
				var autoApportionWeightMenuItem = menu.MenuItems.FindByText("Auto Apportion &Weight");
				Assert("Should be changed to false.", !declaration.JE_AutoWeightApportion);
				menu.RefreshMenu();
				Assert("Should be unticked.", !autoApportionWeightMenuItem.Checked);
				declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_AutoWeightApportion = true;
				menu.Declaration = declaration;
				menu.ShowPopupMenu();
				AssertEquals(true, allocateRemainingWeighMenu.Visible);
			}
		}
	}
}
