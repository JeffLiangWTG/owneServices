using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceModule))]
	public class CommercialInvoiceModuleTest : ZModuleBasherTest
	{
		protected sealed override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CommercialInvoice;
		}

		public void TestWorkflowType()
		{
			using (CommercialInvoiceModule module = new CommercialInvoiceModule())
			{
				AssertEquals("module.SupportsWorkflow", WorkflowDescriptors.CommericalInvoiceWorkflowDescriptorCode, module.WorkflowType);
			}
		}

		public void TestWorkflowSupported()
		{
			using (CommercialInvoiceModule module = new CommercialInvoiceModule())
			{
				Assert(module.SupportsWorkflow);
			}
		}

		public void TestDoesNotShowInvoicesAttachedToDeclarations()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				BaseJobComInvoiceHeader invoice1 = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
				BaseJobComInvoiceHeader invoice2 = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
				invoice1.JZ_InvoiceNumber = "TEST1";
				invoice2.JZ_InvoiceNumber = "TEST2";
				invoice1.JZ_JE = Guid.Empty;
				invoice2.JZ_JE = declaration.PK;
				Factory.Save();
				using (CommercialInvoiceModule module = (CommercialInvoiceModule)ZModuleFactory.Instance.Create(GetModuleID()))
				{
					using (ZForm form = new ZForm())
					{
						form.Controls.Add(module.EmbeddedControl);
						module.PerformSearchForTesting();
						AssertEquals("Should find only one", 1, module.ResultCountForTesting());
						AssertEquals("Should be TEST1", "TEST1", module.FirstInvoiceNumberForTesting);
					}
				}
			}
		}

		public void TestExportToExcelFromInvoiceStripControl_WithColumnsFromShipment()
		{
			FreightDataRegistry.Instance.ShipmentCustomText1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("ShipmentCustomText1", string.Empty));
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				BaseJobComInvoiceHeader invoice1 = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
				BaseJobComInvoiceHeader invoice2 = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
				invoice1.JZ_InvoiceNumber = "TEST1";
				invoice2.JZ_InvoiceNumber = "TEST2";
				invoice1.JZ_JE = Guid.Empty;
				invoice2.JZ_JE = declaration.PK;
				Factory.Save();
				using (CommercialInvoiceModule module = (CommercialInvoiceModule)ZModuleFactory.Instance.Create(GetModuleID()))
				{
					using (ZForm form = new ZForm())
					{
						var filterControl = module.EmbeddedControl as CommercialInvoiceFilterControl;
						form.Controls.Add(module.EmbeddedControl);
						module.PerformSearchForTesting();
						AssertNotNull(module.EmbeddedControl);
						try
						{
							AssertNoExceptionThrown(() => filterControl.Grid.ContextMenu.MenuItems.FindByText("Export All Columns To Excel").PerformClick());
						}
						finally
						{
							DeleteIfExists(ExcelExporter.LastExportedFileNameStaticForTest);
						}

						try
						{
							AssertNoExceptionThrown(() => filterControl.Grid.ContextMenu.MenuItems.FindByText("Export Visible Columns To Excel").PerformClick());
						}
						finally
						{
							DeleteIfExists(ExcelExporter.LastExportedFileNameStaticForTest);
						}
					}
				}
			}
		}

		public void TestImportFromXmlMenu()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil()
			{ EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			using (CommercialInvoiceModule module = new CommercialInvoiceModule())
			{
				AssertNotNull(module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("Data Transfer").MenuItems.FindByText("Import From XML"));
			}
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var bizo = (BaseJobComInvoiceHeader)factory.New(businessObjectType);
			bizo.FillWithValidTestData();
			bizo.JZ_InvoiceNumber = "MODULE BASHER";
			return bizo;
		}

		protected override void CustomiseFilterForFilterStripsHelperTests(FilterStripBusinessObject filterBusinessObject)
		{
			base.CustomiseFilterForFilterStripsHelperTests(filterBusinessObject);
			var filter = (ModuleTextFilter)filterBusinessObject["Invoice #"];
			filter.IsActive = true;
			filter.Property = "MODULE BASHER";
		}
	}
}
