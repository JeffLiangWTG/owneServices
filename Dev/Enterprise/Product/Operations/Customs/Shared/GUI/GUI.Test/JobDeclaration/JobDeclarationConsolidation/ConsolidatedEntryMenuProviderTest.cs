using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class ConsolidatedEntryMenuProviderTest : TestCaseWithFactory
	{
		public void TestConsolidatedEntryMenuItemCaption()
		{
			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = new EDIMenuForConsolidatedEntriesTest())
			{
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				AssertEquals("&Consolidated Entry", consolidatedEntryMenuItem.Text);
			}
		}

		public void TestDequeueFromConsolidationMenuItemCaption()
		{
			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = new EDIMenuForConsolidatedEntriesTest())
			{
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				var dequeueFromConsolidationMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("dequeueFromConsolidationMenuItem");

				AssertEquals("&Dequeue/Remove from Consolidation", dequeueFromConsolidationMenuItem.Text);
			}
		}

		public void TestDequeueFromConsolidationMenuItem()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = new EDIMenuForConsolidatedEntriesTest())
			{
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				var dequeueFromConsolidationMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("dequeueFromConsolidationMenuItem");
				Assert(!dequeueFromConsolidationMenuItem.Visible);

				menu.Declaration = declaration;
				Assert(dequeueFromConsolidationMenuItem.Visible);
				Assert(!dequeueFromConsolidationMenuItem.Enabled);

				declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
				menu.RefreshMenu();
				Assert(dequeueFromConsolidationMenuItem.Visible);
				Assert(dequeueFromConsolidationMenuItem.Enabled);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				dequeueFromConsolidationMenuItem.PerformClick();
				AssertContains("Reports Error", "You must save the current Declaration details before submitting.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				dequeueFromConsolidationMenuItem.PerformClick();
				AssertContains("Reports success", "Job is no longer queued for Consolidation.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Declaration is de queued", ZString.Empty, declaration.JE_ConsolidationStatus);
				AssertEquals(false, declaration.HasChanges);

				menu.RefreshMenu();
				AssertEquals("Menu is now disabled", false, dequeueFromConsolidationMenuItem.Enabled);
			}
		}

		public void TestQueueForConsolidationMenuItem()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_VoyageFlightNo = "XXX999" + '\x1b';

			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = messageInitiator;

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = new EDIMenuForConsolidatedEntriesTest())
			{
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				var queueForConsolidationMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("queueForConsolidationMenuItem");
				Assert(!queueForConsolidationMenuItem.Visible);

				menu.Declaration = declaration;
				Assert(queueForConsolidationMenuItem.Visible);
				Assert(queueForConsolidationMenuItem.Enabled);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				queueForConsolidationMenuItem.PerformClick();
				AssertContains("Reports Error", "You must save the current Declaration details before submitting.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();
				Assert("Not Merged", !declaration.CustomsEntryHeaders.Any());

				messageInitiator.InvalidOperationText = string.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				queueForConsolidationMenuItem.PerformClick();
				AssertContains("Fails silently because of ShutterUpperer", "You can't merge this entry because there are no invoice headers.", messageInitiator.InvalidOperationText);
				AssertEquals("Declaration not queued", ZString.Empty, declaration.JE_ConsolidationStatus);

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				queueForConsolidationMenuItem.PerformClick();
				AssertContains("Validation Error was reported and captured", "Flight/Folio only accepts Western European languages characters.", messageInitiator.InvalidOperationText);
				AssertEquals("Fails silently because of ShutterUpperer", null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Declaration not queued", ZString.Empty, declaration.JE_ConsolidationStatus);

				declaration.JE_VoyageFlightNo = "XXX999";
				Factory.Save();

				messageInitiator.InvalidOperationText = string.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				queueForConsolidationMenuItem.PerformClick();
				AssertEquals("No hidden failures", ZString.Empty, messageInitiator.InvalidOperationText);
				AssertContains("Reports success", "Job queued for Consolidation", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Declaration is queued for consolidation", ConsolidatedEntryStatusList.Codes.ReadyForConsolidation, declaration.JE_ConsolidationStatus);
				AssertEquals(false, declaration.HasChanges);

				Assert("Is Merged", declaration.CustomsEntryHeaders.Any());
				Assert("Not require Merging", !declaration.MergeManager.RequiresMerge);

				menu.RefreshMenu();
				AssertEquals("Menu is now disabled", false, queueForConsolidationMenuItem.Enabled);

				menu.Declaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory).LeadDeclaration;
				menu.Declaration.JE_EntryStatus = "";
				menu.RefreshMenu();
				AssertEquals("Menu is disabled when consolidated even if JE_EntryStatus is empty", false, queueForConsolidationMenuItem.Enabled);
			}
		}

		public void TestWithoutDeclaration()
		{
			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = new EDIMenuForConsolidatedEntriesTest())
			{
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				var queueForConsolidationMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("queueForConsolidationMenuItem");
				var dequeueFromConsolidationMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("dequeueFromConsolidationMenuItem");

				Assert(!queueForConsolidationMenuItem.Visible);
				Assert(!dequeueFromConsolidationMenuItem.Visible);

				menu.RefreshMenu();
				Assert(!queueForConsolidationMenuItem.Visible);
				Assert(!dequeueFromConsolidationMenuItem.Visible);

				// this can't happen because its disabled, but if it did ...
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				queueForConsolidationMenuItem.PerformClick();
				AssertContains("Reports Error", "Could not find the Declaration for this messaging operation.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				dequeueFromConsolidationMenuItem.PerformClick();
				AssertContains("Reports Error", "Could not find the Declaration for this messaging operation.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestLinkToConsolidatedDeclarationMenuItemVisibility()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "NOR";
			declaration.JE_ApplicationCode = "TSW";
			declaration.JE_TransportMode = "AIR";
			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = new EDIMenuForConsolidatedEntriesTest())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				var linkToConsolidationDeclaration = consolidatedEntryMenuItem.MenuItems.FindByName("linkToConsolidationDeclaration");
				AssertEquals("Link to Consolidation Declaration should not be visible for a standard declaration", false, linkToConsolidationDeclaration.Visible);
			}

			declaration.ActiveEntryHeaders.AddNew();
			declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;

			var consolidatedDeclaration = Business.Testing.ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
			consolidatedDeclaration.JobDeclarations[0].JE_MessageSubType = consolidatedDeclaration.JobDeclarations[1].JE_MessageSubType = "NOR";

			consolidatedDeclaration.JobDeclarations.Add(declaration);
			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = new EDIMenuForConsolidatedEntriesTest())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				var linkToConsolidationDeclaration = consolidatedEntryMenuItem.MenuItems.FindByName("linkToConsolidationDeclaration");
				AssertEquals("Link to Consolidation Declaration should be visible for a declaration that has been consolidated", true, linkToConsolidationDeclaration.Visible);
			}
		}

		public void TestMenuVisibilityWhenInterfaced()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = new EDIMenuForConsolidatedEntriesTest())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();

				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				var queueForConsolidationMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("queueForConsolidationMenuItem");
				AssertEquals("Queue For Consolidation should be visible when NOT Interfaced", true, queueForConsolidationMenuItem.Visible);
				var dequeueFromConsolidationMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("dequeueFromConsolidationMenuItem");
				AssertEquals("De-Queue For Consolidation should be visible when NOT Interfaced", true, dequeueFromConsolidationMenuItem.Visible);
				var linkToConsolidationDeclaration = consolidatedEntryMenuItem.MenuItems.FindByName("linkToConsolidationDeclaration");
				AssertEquals("Link to Consolidation Declaration should be visible when NOT Interfaced", true, linkToConsolidationDeclaration.Visible);

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				menu.RefreshMenu();

				AssertEquals("Queue For Consolidation should be hidden when Interface is active", false, queueForConsolidationMenuItem.Visible);
				AssertEquals("De-Queue For Consolidation should be hidden when Interface is active", false, dequeueFromConsolidationMenuItem.Visible);
				AssertEquals("Link to Consolidation Declaration should be hidden when Interface is active", false, linkToConsolidationDeclaration.Visible);
			}
		}

		public void TestCreateConsolidatedEntryMenuItemCaption()
		{
			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = new EDIMenuForConsolidatedEntriesTest())
			{
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				var createConsolidatedEntryMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("createConsolidatedEntryMenuItem");

				AssertEquals("&Create Consolidated Entry", createConsolidatedEntryMenuItem.Text);
			}
		}

		public void TestCreateConsolidatedEntryMenuItem()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = new EDIMenuForConsolidatedEntriesTest())
			{
				var consolidatedEntryMenuItem = menu.MenuItems.FindByName("consolidatedEntryMenuItem");
				var createConsolidatedEntryMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("createConsolidatedEntryMenuItem");
				Assert(!createConsolidatedEntryMenuItem.Visible);

				menu.Declaration = declaration;
				Assert(createConsolidatedEntryMenuItem.Visible);
				Assert(!createConsolidatedEntryMenuItem.Enabled);

				declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
				menu.RefreshMenu();
				Assert(createConsolidatedEntryMenuItem.Visible);
				Assert(createConsolidatedEntryMenuItem.Enabled);

				declaration.JE_EntryStatus = "ATC";
				menu.RefreshMenu();
				AssertEquals("Menu is now disabled", false, createConsolidatedEntryMenuItem.Enabled);
			}
		}
	}

	sealed class EDIMenuForConsolidatedEntriesTest : EDIMenu
	{
		protected override ConsolidatedEntryMenuProvider GetConsolidatedEntryMenuProvider() => new ConsolidatedEntryMenuProvider(Form);
	}
}
