using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GUI.Testing
{
	public abstract class EDIMenuConsolidationTest<T, T2> : TestCaseWithFactory where T : EDIMenu where T2 : BaseJobDeclaration
	{
		protected abstract T GetEdiMenu { get; }

		protected abstract T2 GetDeclaration { get; }

		protected abstract Func<T, IEnumerable<MenuItem>> QueuedForConsolidationDisablesMenuItems { get; }

		protected abstract Func<T, IEnumerable<MenuItem>> QueuedForConsolidationPromptsOnSubmitMenuItems { get; }

		protected abstract Func<T2, IList> GetMergedLinesFunc { get; }

		protected abstract Action<T2> MakeDeclarationMessageErrorAction { get; }

		protected abstract string MessageError { get; }

		protected abstract Action<T> SubmitDeclarationClick { get; }

		protected abstract Action<T, bool> SetRefuseLockForTesting { get; }

		protected abstract bool SupportsRemoveFromConsolidation { get; }

		public void TestConsolidationMenuDisabled()
		{
			var jobDeclaration = GetDeclaration;
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = GetEdiMenu)
			{
				menu.Declaration = jobDeclaration;
				menu.RefreshMenu();
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				var queueForConsolidationMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("queueForConsolidationMenuItem");
				AssertEquals("Menu is enabled", true, queueForConsolidationMenuItem.Enabled);
				jobDeclaration.DoMerge();
				var entryInstruction = jobDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First();
				entryInstruction.Messages.AddNew();
				menu.RefreshMenu();
				AssertEquals("Menu is now disabled", false, queueForConsolidationMenuItem.Enabled);
			}
		}

		public void TestQueueForConsolidationMenuItemVisibility()
		{
			var declaration = GetDeclaration;

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = GetEdiMenu)
			{
				menu.Declaration = declaration;
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				menu.RefreshMenu();
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				var queueForConsolidationMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("queueForConsolidationMenuItem");
				var dequeueFromConsolidationMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("dequeueFromConsolidationMenuItem");
				AssertEquals("QueueForConsolidation hidden when not Import", false, queueForConsolidationMenuItem.Visible);
				AssertEquals("DequeueForConsolidation hidden when not Import", false, dequeueFromConsolidationMenuItem.Visible);

				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				menu.RefreshMenu();
				AssertEquals("QueueForConsolidation visible when Import", true, queueForConsolidationMenuItem.Visible);
				AssertEquals("DequeueForConsolidation visible when Import", true, dequeueFromConsolidationMenuItem.Visible);
			}

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			using (var menu = GetEdiMenu)
			{
				menu.Declaration = declaration;
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				menu.RefreshMenu();
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				AssertNull("ConsolidatedEntry is not created when registry disabled", consolidatedEntryMenuItem);
			}
		}

		public void TestQueuedForConsolidationDisablesMenuItems()
		{
			var declaration = GetDeclaration;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			using (var menu = GetEdiMenu)
			{
				var menuItems = QueuedForConsolidationDisablesMenuItems(menu);
				menu.Declaration = declaration;
				declaration.JE_EntryStatus = string.Empty;
				menu.RefreshMenu();
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				AssertNull("ConsolidatedEntry is not created when registry disabled", consolidatedEntryMenuItem);
				AssertMenuItemsEnabled(menuItems, true);
			}

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = GetEdiMenu)
			{
				var menuItems = QueuedForConsolidationDisablesMenuItems(menu);
				menu.Declaration = declaration;
				declaration.JE_EntryStatus = string.Empty;
				declaration.JE_MessageStatus = string.Empty;
				menu.RefreshMenu();
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				var queueForConsolidationMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("queueForConsolidationMenuItem");
				var dequeueFromConsolidationMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("dequeueFromConsolidationMenuItem");
				AssertEquals("QueueForConsolidation Menu is enabled", true, queueForConsolidationMenuItem.Enabled);
				AssertEquals("DequeueForConsolidation Menu is disabled", false, dequeueFromConsolidationMenuItem.Enabled);
				AssertMenuItemsEnabled(menuItems, true);

				declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
				menu.RefreshMenu();
				AssertEquals("QueueForConsolidation Menu is disabled", false, queueForConsolidationMenuItem.Enabled);
				AssertEquals("DequeueForConsolidation Menu is enabled", true, dequeueFromConsolidationMenuItem.Enabled);
				AssertMenuItemsEnabled(menuItems, true);

				declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;
				menu.RefreshMenu();
				AssertEquals("QueueForConsolidation Menu is disabled", false, queueForConsolidationMenuItem.Enabled);
				AssertEquals($"DequeueForConsolidation Menu is {(declaration.ConsolidatedEntryProvider.CanRemove ? "enabled" : "disabled")}", declaration.ConsolidatedEntryProvider.CanRemove, dequeueFromConsolidationMenuItem.Enabled);
				AssertMenuItemsEnabled(menuItems, false);

				declaration.JE_EntryStatus = string.Empty;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.Messages.AddNew();
				menu.RefreshMenu();
				AssertEquals("QueueForConsolidation Menu is disabled", false, queueForConsolidationMenuItem.Enabled);
				AssertEquals("DequeueForConsolidation Menu is disabled", false, dequeueFromConsolidationMenuItem.Enabled);
				AssertMenuItemsEnabled(menuItems, true);
			}
		}

		public void TestQueueForConsolidationMenuItem()
		{
			var declaration = GetDeclaration;
			MakeDeclarationMessageErrorAction.Invoke(declaration);
			Factory.Save();
			declaration.MessageInitiator = new SendsMessagesToCustomsGUI();
			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = GetEdiMenu)
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				AssertEquals("Precondition: Declaration not merged", 0, GetMergedLinesFunc.Invoke(declaration).Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				var queueForConsolidationMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("queueForConsolidationMenuItem");
				queueForConsolidationMenuItem.PerformClick();
				AssertContains("Reports validation Message Error", MessageError, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Declaration not queued", ZString.Empty, declaration.JE_ConsolidationStatus);
				AssertEquals("Declaration is merged", 1, GetMergedLinesFunc.Invoke(declaration).Count);

				Factory.Save();
				menu.RefreshMenu();
				AssertEquals("Menu is still enabled", true, queueForConsolidationMenuItem.Enabled);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				queueForConsolidationMenuItem.PerformClick();
				AssertContains("Reports success", "Job queued for Consolidation", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Declaration is queued for consolidation", ConsolidatedEntryStatusList.Codes.ReadyForConsolidation, declaration.JE_ConsolidationStatus);

				menu.RefreshMenu();
				AssertEquals("Menu is now disabled", false, queueForConsolidationMenuItem.Enabled);
				AssertEquals(false, declaration.HasChanges);
			}
		}

		public void TestDequeueForConsolidationMenuItem_RemoveFromConsolidation()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			consolidatedDeclaration.LeadDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			Factory.Save();
			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = GetEdiMenu)
			{
				menu.Declaration = consolidatedDeclaration.LeadDeclaration;
				menu.RefreshMenu();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				var dequeueFromConsolidationMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("dequeueFromConsolidationMenuItem");
				if (!SupportsRemoveFromConsolidation)
				{
					Assert("Dequeue menu item should be disabled", !dequeueFromConsolidationMenuItem.Enabled);
					return;
				}

				Assert("Dequeue menu item should be enabled", dequeueFromConsolidationMenuItem.Enabled);
				dequeueFromConsolidationMenuItem.PerformClick();
				AssertContains("Cannot remove one declaration", "A Consolidation requires at least 1 declaration, you cannot remove the last one.", UnitTestUserNotification.Instance.LastMessage.Text);
				consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
				var declaration = consolidatedDeclaration.LeadDeclaration;
				var header = declaration.Invoices.AddNew();
				header.InvoiceLines.AddNew();
				var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.First();
				entryHeader.Charges.SetAmount("APC", 20m);
				entryHeader.Charges.SetAmount("DPC", 40m);
				menu.Declaration = declaration;
				menu.RefreshMenu();
				Factory.Save();
				dequeueFromConsolidationMenuItem.PerformClick();
				AssertContains("Declaration removed", "Job is no longer in Consolidation.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDequeueForConsolidationMenuItem()
		{
			var declaration = GetDeclaration;
			declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			Factory.Save();
			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = GetEdiMenu)
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				var dequeueFromConsolidationMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("dequeueFromConsolidationMenuItem");

				dequeueFromConsolidationMenuItem.PerformClick();
				AssertContains("Reports success", "Job is no longer queued for Consolidation.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Declaration is de queued", ZString.Empty, declaration.JE_ConsolidationStatus);
				AssertEquals(false, declaration.HasChanges);
			}
		}

		public void TestQueuedForConsolidationPromptsOnSubmit()
		{
			var declaration = GetDeclaration;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = GetEdiMenu)
			{
				menu.Declaration = declaration;

				declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.RefreshMenu();
				SubmitDeclarationClick.Invoke(menu);
				AssertContains("Cannot dequeue", "This entry has been linked to a Consolidated Entry and cannot be submitted individually.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No - Declaration remains in consolidation", ConsolidatedEntryStatusList.Codes.AppliedToConsolidation, declaration.JE_ConsolidationStatus);

				declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menu.RefreshMenu();
				SubmitDeclarationClick.Invoke(menu);

				var expectedPrompt = @"This entry is currently queued to be linked to a Consolidated Entry. Continuing to submit this entry will remove it from the queue.
Do you wish to continue to submit this entry?";

				AssertContains("Asks if should dequeue", expectedPrompt, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No - Declaration remains queued for consolidation", ConsolidatedEntryStatusList.Codes.ReadyForConsolidation, declaration.JE_ConsolidationStatus);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.RefreshMenu();
				SubmitDeclarationClick.Invoke(menu);

				var allNotifications = string.Join("\r\n", UnitTestUserNotification.Instance.PreviousMessages.Select(m => m.Text));
				AssertContains("Asks if should dequeue", expectedPrompt, allNotifications);
				AssertEquals("Yes - Declaration is de-queued from consolidation", ZString.Empty, declaration.JE_ConsolidationStatus);
			}
		}

		public void TestQueuedForConsolidationPromptsOnSubmit_MenuItems()
		{
			var declaration = GetDeclaration;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = GetEdiMenu)
			{
				menu.Declaration = declaration;
				declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;
				Factory.Save();

				foreach (var menuItem in QueuedForConsolidationPromptsOnSubmitMenuItems.Invoke(menu))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.RefreshMenu();
					menuItem.PerformClick();
					AssertContains($"Cannot dequeue {menuItem.Text}", "This entry has been linked to a Consolidated Entry and cannot be submitted individually.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestCannotSubmitWithoutAquiringConsolidationLock()
		{
			var jobDeclaration = GetDeclaration;
			jobDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			jobDeclaration.DoMerge();
			Factory.Save();

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = GetEdiMenu)
			{
				menu.Declaration = jobDeclaration;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.RefreshMenu();
				SetRefuseLockForTesting.Invoke(menu, true);
				SubmitDeclarationClick.Invoke(menu);
				AssertContains("Cannot aquire lock", "This entry is in the process of Consolidation and cannot be submitted at this time.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		static void AssertMenuItemsEnabled(IEnumerable<MenuItem> menuItems, bool shouldBeEnabled)
		{
			foreach (var menuItem in menuItems)
			{
				AssertEquals($"{menuItem.Text} is {(shouldBeEnabled ? "enabled" : "disabled")}", shouldBeEnabled, menuItem.Enabled);
			}
		}
	}
}
