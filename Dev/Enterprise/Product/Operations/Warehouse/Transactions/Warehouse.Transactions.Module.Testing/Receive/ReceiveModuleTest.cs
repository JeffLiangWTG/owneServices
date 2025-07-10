using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(ReceiveModule))]
	internal class ReceiveModuleTest : ZModuleBasherTest
	{
		#region TestSupportsWorkflow

		public void TestSupportsWorkflow()
		{
			using (var module = new ReceiveModule())
			{
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.WhsReceive, module.ID);
			}
		}

		#endregion

		#region TestLicenseCheckPoint

		public void TestLicenseCheckPoint()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerOperationsAnd3PL, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region TestSecurityCheckPoint

		public void TestSecurityCheckPoint()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsReceive, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region TestControllersDefinedForAllCountriesModuleDefinedOn

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert("No controller available for this module", true);
		}

		#endregion

		#region TestIImportCollectionInfoProvider

		public void TestIImportCollectionInfoProvider()
		{
			using (var module = new ReceiveModule())
			{
				var provider = (IImportCollectionInfoProvider)module;
				AssertEquals("WhsReceiveModuleImportWizard", provider.ContextKey);
				var info = provider.ImportCollectionInfo;
				AssertEquals(typeof(ImportCollectionInfoImplForWhsDocketFlattened), info.GetType());

				var info2 = provider.ImportCollectionInfo;
				AssertNotEquals("Must be a different instance so the import starts fresh", info, info2);
				AssertNotEquals("Must be a different instance so the import starts fresh", info.Collection, info2.Collection);
			}
		}

		#endregion

		#region TestCSVImport

		[TestDate(2015, 10, 21)]
		public void TestCSVImport_InterfaceConnectorOn()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Today.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			using (var module = (ReceiveModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var item = MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions", "D&ata Transfer", "Import From &Excel (.csv)");
				item.PerformClick();

				var form = ZFormModaliser.LastFormShownDialogForTest;
				AssertNotNull("form was shown", form);
				AssertEquals("form is correct type", typeof(ImportReceiveFromCSVForm), form.GetType());
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		[TestDate(2015, 10, 21)]
		public void TestCSVImport_InterfaceConnectorOff()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			using (var module = (ReceiveModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions", "D&ata Transfer", "Import From &Excel (.csv)");
				// BG: For now does not matter what InterfaceConnector registry is set to.
			}
		}

		#endregion

		#region TestAllowDelete

		public void TestAllowDelete()
		{
			using (var receiveModule = new ReceiveModule())
			{
				Assert("Delete option should not be available", !receiveModule.AllowDelete);
			}
		}

		#endregion

		#region DeniedPartyScreeningMenuItems

		public void TestDeniedPartyScreeningMenuAdded()
		{
			using (var module = new ReceiveModule())
			{
				AssertNotNull("View Compliance Status", module.FormActionMenu.FindByText("View Compliance Status", true));
			}
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsReceive;
		}

		#endregion
	}
}
