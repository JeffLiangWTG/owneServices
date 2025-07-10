using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Customs.NZ.GUI.Test.Declaration.NZEDIMenuConsolidationTest;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	sealed class NZConsolidatedEntryMenuProviderTest : TestCaseWithFactory
	{
		public void TestMergeDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_VoyageFlightNo = "XXX999";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = messageInitiator;
			Factory.Save();

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = new ConsolidationTestMenu())
			{
				var nzProvider = menu.ConsolidatedEntryMenuProvider;
				var consolidatedEntryMenuItem = nzProvider.CreateMenuEntries().FirstOrDefault(m => m.Name == "consolidatedEntryMenuItem");
				var queueForConsolidationMenuItem = consolidatedEntryMenuItem.MenuItems.FindByName("queueForConsolidationMenuItem");
				nzProvider.RefreshMenu(declaration);

				Assert("Not Merged", !declaration.CustomsEntryHeaders.Any());
				Assert("Not require Merging", !declaration.MergeManager.RequiresMerge);

				queueForConsolidationMenuItem.PerformClick();

				Assert("Is Merged", declaration.CustomsEntryHeaders.Any());
				Assert("Not require Merging", !declaration.MergeManager.RequiresMerge);
			}
		}

		public void TestMenuItemsVisible()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			using (var menu = new ConsolidationTestMenu())
			{
				var nzProvider = menu.ConsolidatedEntryMenuProvider;
				var menuItems = nzProvider.CreateMenuEntries().ToArray();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				nzProvider.RefreshMenu(declaration);
				Assert("Not enabled, does not show menu.", !menuItems.Any(m => m.Visible));

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				nzProvider.RefreshMenu(declaration);
				Assert("Not enabled, does not show menu.", !menuItems.Any(m => m.Visible));
			}

			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var menu = new ConsolidationTestMenu())
			{
				var nzProvider = menu.ConsolidatedEntryMenuProvider;
				var menuItems = nzProvider.CreateMenuEntries().ToArray();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				nzProvider.RefreshMenu(declaration);
				Assert("Not enabled for export, does not show menu.", !menuItems.Any(m => m.Visible));

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = Business.JobMessageSubTypeList.Codes.Periodic;
				nzProvider.RefreshMenu(declaration);
				Assert("Enabled for Import and PER.  Shows menu.", menuItems.Any(m => m.Visible));

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = Business.JobMessageSubTypeList.Codes.Normal;
				nzProvider.RefreshMenu(declaration);
				Assert("Enabled for Import and NOR.  Shows menu.", menuItems.Any(m => m.Visible));

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = Business.JobMessageSubTypeList.Codes.WriteOff;
				nzProvider.RefreshMenu(declaration);
				Assert("Not enabled for ECI, does not show menu.", !menuItems.Any(m => m.Visible));

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				nzProvider.RefreshMenu(declaration);
				Assert("Never enabled when Interface is active, does not show menu.", !menuItems.Any(m => m.Visible));

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = Business.JobMessageSubTypeList.Codes.Periodic;
				nzProvider.RefreshMenu(declaration);
				Assert("Never enabled when Interface is active, does not show menu.", !menuItems.Any(m => m.Visible));

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = Business.JobMessageSubTypeList.Codes.Normal;
				nzProvider.RefreshMenu(declaration);
				Assert("Never enabled when Interface is active, does not show menu.", !menuItems.Any(m => m.Visible));

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = Business.JobMessageSubTypeList.Codes.WriteOff;
				nzProvider.RefreshMenu(declaration);
				Assert("Never enabled when Interface is active, does not show menu.", !menuItems.Any(m => m.Visible));
			}
		}
	}
}
