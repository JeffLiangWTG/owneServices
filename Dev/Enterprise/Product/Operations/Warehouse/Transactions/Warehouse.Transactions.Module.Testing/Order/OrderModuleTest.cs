using System;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(OrderModule))]
	public class OrderModuleTest : ZModuleBasherTest
	{
		#region TestImportFromCsv

		[TestDate(2015, 10, 21)]
		public void TestImportFromCsv_InterfaceConnectorOn()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Today.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			using (var module = (OrderModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var cSVImportItem = module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("Data Transfer").MenuItems.FindByText("Import From &CSV");
				AssertNotNull(cSVImportItem);
				cSVImportItem.PerformClick();

				AssertEquals("DataImporterForm", ZFormModaliser.LastFormShownDialogForTest.Name);
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		[TestDate(2015, 10, 21)]
		public void TestImportFromCsv_InterfaceConnectorOff()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			using (var module = (OrderModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var cSVImportItem = module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("Data Transfer").MenuItems.FindByText("Import From &CSV");
				AssertNotNull(cSVImportItem);
				// BG: For now does not matter what InterfaceConnector registry is set to.
			}
		}

		#endregion

		#region TestImportFromXml
		public void TestImportFromXml()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			using (var module = (OrderModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				module.DataTransferDirector = new XmlDataTransferDirectorForTest(new WhsOrderValueObjectDataAdapter(), false);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var xMLImportItem = module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("Data Transfer").MenuItems.FindByText("Import From &XML");
				AssertNotNull(xMLImportItem);
				xMLImportItem.PerformClick();

				AssertEquals("Import done", true, ((XmlDataTransferDirectorForTest)module.DataTransferDirector).ImportFromXmlDone);
			}
		}

		class XmlDataTransferDirectorForTest : XmlDataTransferDirector
		{
			public XmlDataTransferDirectorForTest(IValueObjectDataAdapter adapter, bool checkForLicence)
				: base(adapter, checkForLicence)
			{
			}

			protected override void PromptUserAndImportCore(BillingInterfaceName interfaceName)
			{
				ImportFromXmlDone = true;
			}

			public bool ImportFromXmlDone;
		}
		#endregion

		#region TestImportFromIFS

		public void TestImportFromIFS()
		{
			using (var module = (OrderModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				module.DataTransferDirectorIFS = new XmlDataTransferDirectorForTest(new WhsOrderCartageValueObjectDataAdapterIFS(), false);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var iFSImportItem = module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("Data Transfer").MenuItems.FindByText("Import From &IFS");
				AssertNotNull(iFSImportItem);

				iFSImportItem.PerformClick();
				AssertEquals("Import done", true, ((XmlDataTransferDirectorForTest)module.DataTransferDirectorIFS).ImportFromXmlDone);
			}
		}

		public void TestIFSDataTransferDirectorUsesCorrectSerializer()
		{
			using (var module = (OrderModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(typeof(XmlValueObjectSerializerIFS), module.DataTransferDirectorIFS.Serializer.GetType());
			}
		}

		#endregion

		#region TestExportToCartage

		//public void TestExportToCartage()
		//{
		//    using (OrderModule module = (OrderModule)ZModuleFactory.Instance.Create(GetModuleID()))
		//    {
		//        UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
		//        MenuItem exportToExportToCartageItem = module.FormActionMenu.FindByText("Data Transfer").MenuItems.FindByText("Export Selected Order to Cartage");
		//        AssertNotNull(exportToExportToCartageItem);
		//        exportToExportToCartageItem.PerformClick();

		//        AssertEquals("DataImporterForm", ZFormModaliser.LastFormShownDialogForTest.Name);
		//    }
		//}

		#endregion

		#region TestExportToCartageWithInvalidOrders

		//public void TestExportToCartageWithInvalidOrders()
		//{
		//    // must match on:
		//    // Consignee ADDRESS (not PK)
		//    // Carrier
		//    // Service Level

		//    //OrgHeader consignee = Factory.New<OrgHeader>();

		//    //WhsOrder order1 = Factory.New<WhsOrder>();
		//    //WhsOrder order2 = Factory.New<WhsOrder>();

		//    //order1.ConsigneePK = consignee.PK;
		//    //order2.ConsigneePK = consignee.PK;
		//}

		#endregion

		#region TestModuleIDAndSupportsWorkflow

		public void TestModuleIDAndSupportsWorkflow()
		{
			using (var module = new OrderModule())
			{
				AssertEquals(ModuleIDs.WhsOrder, module.ID);
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			using (var module = (OrderModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.WhsOrder, module.ID);
			}
		}

		#endregion

		#region TestBusinessContexts

		public void TestBusinessContexts()
		{
			using (var module = new OrderModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("WhsOrder business context should be returned", BusinessContext.WhsOrder, module.BusinessContexts[0]);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		#endregion

		#region TestLicenseCheckPoint

		public void TestLicenseCheckPoint()
		{
			using (var module = (OrderModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerOperationsAnd3PL, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region TestSecurityCheckPoint

		public void TestSecurityCheckPoint()
		{
			using (var module = (OrderModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsOrder, module.SecurityCheckpoint);
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
			using (var module = new OrderModule())
			{
				var provider = (IImportCollectionInfoProvider)module;
				AssertEquals("WhsOrderModuleImportWizard", provider.ContextKey);
				var info = provider.ImportCollectionInfo;
				AssertEquals(typeof(ImportCollectionInfoImplForWhsDocketFlattened), info.GetType());

				var info2 = provider.ImportCollectionInfo;
				AssertNotEquals("Must be a different instance so the import starts fresh", info, info2);
				AssertNotEquals("Must be a different instance so the import starts fresh", info.Collection, info2.Collection);
			}
		}

		#endregion

		#region TestAllowDelete

		public void TestAllowDelete()
		{
			using (var orderModule = new OrderModule())
			{
				Assert("Delete option should not be available", !orderModule.AllowDelete);
			}
		}

		#endregion

		#region DeniedPartyScreeningMenuItems

		public void TestDeniedPartyScreeningMenuAdded()
		{
			using (var module = new OrderModule())
			{
				AssertNotNull("View Compliance Status", module.FormActionMenu.FindByText("View Compliance Status", true));
			}
		}

		#endregion

		public void TestFindDoesNotThrowException()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var order1 = helper.CreateWhsOrder(data.Org1, data.Whs1, "Ord1");
			helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			var order2 = helper.CreateWhsOrder(data.Org1, data.Whs1, "Ord2");
			helper.CreateWhsOrderLine(order2, data.Part1, 6m);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.WD_RequiredDate = order1.WD_RequiredDate;
			Factory.Save();
			using (SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				var originalValue = EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult;
				try
				{
					EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult = true;
					using (var orderModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.WhsOrder))
					using (var form = new ZForm())
					{
						form.Controls.Add(orderModule.EmbeddedControl);
						form.Show();
						orderModule.FilterBusinessObject.Filter.Clear();
						Application.DoEvents();
						AssertNoExceptionThrown("Find 1 - This should not throw an exception.", () => ((ZFilterStripCommonControl)orderModule.EmbeddedControl).Find());
						AssertNoExceptionThrown("Find 2 - This should not throw an exception.", () => ((ZFilterStripCommonControl)orderModule.EmbeddedControl).Find());
					}
				}
				finally
				{
					EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult = originalValue;
				}
			}
		}

		public void TestNewOrderIsNotAddedToGridWhenShowExactRowIsOn()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var order1 = helper.CreateWhsOrder(data.Org1, data.Whs1, "Ord1");
			order1.WD_CustomerReference = "Thing1";
			var order2 = helper.CreateWhsOrder(data.Org1, data.Whs1, "Ord2");
			order2.WD_CustomerReference = "Thing2";
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.WD_RequiredDate = order1.WD_RequiredDate;
			Factory.Save();
			var originalValue = EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult;
			try
			{
				EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult = true;
				using (var orderModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.WhsOrder))
				using (var form = new ZForm())
				{
					form.Controls.Add(orderModule.EmbeddedControl);
					form.Show();
					orderModule.FilterBusinessObject.Filter.Clear();

					var result = (ModuleTextFilter)orderModule.FilterBusinessObject["Customer Reference"];
					result.Property = "OtherThing";
					result.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
					result.IsActive = true;
					Factory.Save();

					Application.DoEvents();

					((ZFilterStripCommonControl)orderModule.EmbeddedControl).Find();
					AssertEquals("Control test", 0, orderModule.GridCollection.Count);

					var order3 = helper.CreateWhsOrder(data.Org1, data.Whs1, "Ord3");
					order3.WD_CustomerReference = "NotThing3";
					Factory.Save();

					AssertEquals("Test failure", 0, orderModule.GridCollection.Count);
				}
			}
			finally
			{
				EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult = originalValue;
			}
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.WhsOrder;

		#endregion
	}
}
