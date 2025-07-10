using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	sealed class JobDeclarationModuleBaseOnlyTest : JobDeclarationModuleAbstractTest
	{
		[ExpectNoExceptions]
		public void TestImportDeclarationToAnotherCountry()
		{
			ZGuid decPK;
			var factory = new BusinessObjectFactory();
			using (DisposableEnvironment.ForBranchCodeSlowerThanPK("SYD"))
			{
				decPK = factory.NewWithValidTestData<BaseJobDeclaration>().PK;
				factory.Save();
			}

			factory = new BusinessObjectFactory();
			using (DisposableEnvironment.ForBranchCodeSlowerThanPK("SIN"))
			{
				using (var module = new JobDeclarationModuleForTest())
				{
					var importJobDeclaration = new ImportJobDeclaration(factory);
					importJobDeclaration.CountryCode = Core.Constants.CountryCodes.Australia;
					importJobDeclaration.DeclarationPK = decPK;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					module.CreateAndImportDeclarationFromAnotherDeclaration(importJobDeclaration);
					module.lastEditForm?.Dispose();
				}
			}
		}

		public void TestPrintJobProfitDocument()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_DeclarationReference = "B0001";
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001";
			BaseJobDeclaration shipmentDeclaration = BaseJobDeclaration.New(Factory);
			shipmentDeclaration.JE_DeclarationReference = "S0001";
			shipmentDeclaration.JE_JS = shipment.PK;
			Factory.Save();
			var printingHelper = new BulkJobProfitPrintingModuleHelperForTest();
			using (ObjectFactory.Substitute<IBulkJobProfitPrintingModuleHelper>(printingHelper))
			using (JobDeclarationModuleForTest module = new JobDeclarationModuleForTest())
			{
				module.selectedElements = new BaseJobDeclaration[2];
				module.selectedElements[0] = declaration;
				module.selectedElements[1] = shipmentDeclaration;
				module.PrintJobProfitDocument();
				AssertNotNull(printingHelper.PrintedBizObjects);
				AssertEquals("Two business objects created for printing", 2, printingHelper.PrintedBizObjects.Count);
				AssertCollectionContains(declaration, printingHelper.PrintedBizObjects);
				AssertCollectionContains(shipment, printingHelper.PrintedBizObjects);
			}
		}

		public void TestModuleIDAndSupportsWorkflow()
		{
			using (JobDeclarationModule module = new JobDeclarationModule())
			{
				AssertEquals(ModuleIDs.Customs.JobDeclaration, module.ID);
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		public void TestImportFromXmlMenu()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil()
			{ EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			using (JobDeclarationModule module = new JobDeclarationModule())
			{
				module.CountryCode = Enterprise.Core.Constants.CountryCodes.SriLanka;
				AssertNotNull(module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("Data Transfer").MenuItems.FindByText("Import From XML"));
			}
		}

		public void TestCopyDeclarationOnly_ExistsInContextMenu()
		{
			using (JobDeclarationModuleForTest module = new JobDeclarationModuleForTest())
			{
				AssertNull(module.FormActionMenu.FindByText(JobDeclarationModuleForTest.CopyDeclarationOnlyText));
			}
		}

		public void TestAllowEdit()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			using (JobDeclarationModule module = new JobDeclarationModule())
			{
				module.CountryCode = Core.Constants.CountryCodes.Australia;
				AssertEquals(true, module.AllowEdit);
				module.CountryCode = Core.Constants.CountryCodes.Bangladesh;
				AssertEquals(false, module.AllowEdit);
			}
		}

		public void TestAllowNew()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			using (JobDeclarationModule module = new JobDeclarationModule())
			{
				module.CountryCode = Core.Constants.CountryCodes.Australia;
				AssertEquals(true, module.AllowNew);
				module.CountryCode = Core.Constants.CountryCodes.Bangladesh;
				AssertEquals(false, module.AllowNew);
			}
		}

		public void TestAllowView()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			using (JobDeclarationModule module = new JobDeclarationModule())
			{
				module.CountryCode = Core.Constants.CountryCodes.Australia;
				AssertEquals(true, module.AllowView);
				module.CountryCode = Core.Constants.CountryCodes.Bangladesh;
				AssertEquals(false, module.AllowView);
			}
		}

		public void TestAllowDelete()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			using (JobDeclarationModule module = new JobDeclarationModule())
			{
				module.CountryCode = Core.Constants.CountryCodes.Australia;
				AssertEquals(true, module.AllowDelete);
				module.CountryCode = Core.Constants.CountryCodes.Bangladesh;
				AssertEquals(false, module.AllowDelete);
			}
		}

		public void TestActionMenuIsNotShownForOtherCountry()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			using (JobDeclarationModule module = new JobDeclarationModule())
			{
				module.CountryCode = Core.Constants.CountryCodes.Australia;
				AssertNotNull(module.FormActionMenu.FindByText("&Actions"));
			}

			using (JobDeclarationModule module = new JobDeclarationModule())
			{
				module.CountryCode = Core.Constants.CountryCodes.Bangladesh;
				AssertNull(module.FormActionMenu.FindByText("&Actions"));
			}
		}

		public void TestDPSMenuItemsAreShownForAllCountries()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			using (JobDeclarationModule module = new JobDeclarationModule())
			{
				module.CountryCode = Core.Constants.CountryCodes.UnitedStates;
				AssertNotNull(module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("&View Compliance Status"));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (JobDeclarationModule module = new JobDeclarationModule())
			{
				module.CountryCode = Core.Constants.CountryCodes.UnitedKingdom;
				AssertNotNull(module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("&View Compliance Status"));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (JobDeclarationModule module = new JobDeclarationModule())
			{
				module.CountryCode = Core.Constants.CountryCodes.Australia;
				AssertNotNull(module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("&View Compliance Status"));
			}
		}

		public void TestCopyDeclarationOnly()
		{
			using (JobDeclarationModuleForTest module = new JobDeclarationModuleForTest())
			{
				module.CountryCode = GlbCompany.CurrentCompany.Country.Code;
				BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
				ForwardingShipment shipment = Factory.New<ForwardingShipment>();
				shipment.JS_UniqueConsignRef = "TST001";
				declaration.JE_JS = shipment.PK;
				Factory.Save();
				module.SimulateContextMenuClick();
				AssertEquals("Information Please select one Declaration to copy.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("pre-condition", 0, OpenedFormCache.GetInstance().Count);
				module.selectedElements = new BaseJobDeclaration[1];
				module.selectedElements[0] = declaration;
				module.SimulateContextMenuClick();
				AssertEquals(1, OpenedFormCache.GetInstance().Count);
				OpenedFormCache.GetInstance().CloseAllCachedForms();
			}
		}

		public void TestSetGuiProviders()
		{
			using (JobDeclarationModuleForTest module = new JobDeclarationModuleForTest())
			{
				module.GetNewGridCollection();
				var servicesSelectionProvider = module.Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull(servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);
				module.PerformSearch();
				servicesSelectionProvider = module.Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull(servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = new JobDeclarationModule())
			{
				AssertEquals(Env.Security.CustomsDeclarationEnquiry, module.SecurityCheckpoint);
			}
		}

		public void TestSecurityForShipment()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			Factory.Save();
			Env.Security.CustomsDeclarationEnquiry.IsAllowed = false;
			using (var module = new JobDeclarationModule())
			using (var form = module.ShowForms_Exposed(new BusinessObject[] { declaration }, view: true)[0])
			{
				AssertNull("Shipment Form", form);
			}

			Env.Security.CustomsDeclarationEnquiry.IsAllowed = true;
			using (var module = new JobDeclarationModule())
			using (var form = module.ShowForms_Exposed(new BusinessObject[] { declaration }, view: true)[0])
			{
				var shipmentForm = form as Freight.Forwarding.GUI.ShipmentForm;
				AssertNotNull(shipmentForm);
				AssertEquals(shipmentForm.PlugInIDToSelectOnLoaded, ControllerIDs.Customs.JobDeclaration);
			}
		}

		public void TestTransportBookingActionAdded()
		{
			using (var module = new JobDeclarationModuleForTest())
			{
				AssertNotNull(module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems.FindByText("Transport Booking"));
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.SriLanka;

		protected override Type GetExpectedJobDeclarationType() => typeof(BaseJobDeclaration);

		protected override Type GetExpectedInvoiceHeaderType() => typeof(BaseJobComInvoiceHeader);

		protected override Type GetExpectedInvoiceLineType() => typeof(BaseJobComInvoiceLine);

		#region JobDeclarationModuleForTest
		class BulkJobProfitPrintingModuleHelperForTest : IBulkJobProfitPrintingModuleHelper
		{
			public void PrintJobProfitDocument(BusinessObjectFactory factory, BusinessObject[] selectedElements)
			{
				PrintedBizObjects = new List<BusinessObject>(selectedElements);
			}

			public IMenuItem GetMenuItem(JobProfitPrintingDelegate jobProfitPrintingDelegate)
			{
				return null;
			}

			public void SetInvoiceCheckPoint(SecurityCheckpoint invoicingCheckpoint)
			{
			}

			public List<BusinessObject> PrintedBizObjects;
		}

		public class JobDeclarationModuleForTest : JobDeclarationModule
		{
			public new ZController GetNewController(BusinessObject selectedBusinessObject)
			{
				return GetNewController(selectedBusinessObject);
			}

			public new MenuItem[] GetNewStandardMenuItems()
			{
				return GetNewStandardMenuItems();
			}

			public new NameEventHandlerCollection ImportMenuItems
			{
				get
				{
					return ImportMenuItems;
				}
			}

			public override bool AllowNew
			{
				get
				{
					return true;
				}
			}

			protected override MenuItem[] GetNewActionMenuItems()
			{
				++CallsToGetNewActionMenuItems;
				return base.GetNewActionMenuItems();
			}

			public int CallsToGetNewActionMenuItems;
			public new MenuItem[] ContextMenu
			{
				get
				{
					return base.ContextMenu;
				}
			}

			public void SimulateContextMenuClick()
			{
				MenuItem actionsMenus = Grid.ContextMenu.MenuItems.FindByText("Actions");
				if (actionsMenus != null)
				{
					MenuItem menuItem = actionsMenus.MenuItems.FindByText(CopyDeclarationOnlyText);
					menuItem.PerformClick();
				}
			}

			public void SimulatePrintJobProfitContextMenuClick()
			{
				MenuItem actionsMenus = Grid.ContextMenu.MenuItems.FindByText("Actions");
				if (actionsMenus != null)
				{
					MenuItem menuItem = actionsMenus.MenuItems.FindByText("Print Job Profit Document");
					menuItem.PerformClick();
				}
			}

			protected override BusinessObject[] GetSelectedElements()
			{
				return selectedElements;
			}

			public BusinessObject[] selectedElements;
			public new BusinessObjectFactory Factory
			{
				get
				{
					return base.Factory;
				}
			}

			public new IBusinessObjectCollection GetNewGridCollection()
			{
				return base.GetNewGridCollection();
			}

			public void PerformSearch()
			{
				base.PerformSearch();
			}

			protected override IZForm ShowEditForm(BusinessObject selectedBusinessObject)
			{
				lastEditForm = base.ShowEditForm(selectedBusinessObject);
				return lastEditForm;
			}

			public IZForm lastEditForm;
		}
		#endregion
	}
}
