using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Accounting.Business;
using Enterprise.ComplianceRisk.GUI;
using Enterprise.Core.Modules;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.DeniedPartyScreening.GUI.Test;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.GUI;
using Enterprise.Freight.GUI.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.ApiClient;
using Enterprise.Integration;
using Enterprise.Integration.Packing;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Internal;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using CO2eBusinessTestHelper = Enterprise.Freight.CarbonEmissions.Business.Testing.CO2eTestHelper;
using CO2eTestHelper = Enterprise.Freight.DataTransfer.Universal.Testing.CO2eTestHelper;
using Constants = Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;
using FreightRegistry = Enterprise.Registry.Business.FreightDataRegistry;
using GlowRegistry = Enterprise.Registry.Business.GlowRegistry;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
#if !WINZOR
using Enterprise.Freight.Forwarding.Logging;
#endif

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ShipmentFormTest : ZFormBindingContextTester
	{
		[RequiresSTA]
		public void TestConvertingFromBooking()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_GoodsDescription = "Test Desc 1";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var job = new JobHeader.Loader(shipment).TryLoadOrCreate();
			job.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			var mockFavoriteProvider = new Mock<IFavoriteProvider>();

			var addToRecentItemsCount = 0;
			mockFavoriteProvider.Setup(x => x.AddToRecentItems(Moq.It.IsAny<LinkWrapper>())).Callback(() => ++addToRecentItemsCount);

			ObjectFactory.Substitute(mockFavoriteProvider.Object);

			using (var form = new ShipmentForm(shipment))
			{
				form.ControllerID = ControllerIDs.JobShipment;
				shipment.JS_IsForwardRegistered = true;
				form.Show();
				Application.DoEvents();
				AssertEquals(0, addToRecentItemsCount);

				form.DialogResult = DialogResult.Yes;
				form.TestValidateAndSave();
				AssertEquals(1, addToRecentItemsCount);

				form.Close();
			}
		}

		public void TestNoConcurrencyErrorWhenAccessingEDocs()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.New<ForwardingShipment>();
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_GoodsDescription = "Test Desc 1";
			var consignee = factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			var consignor = factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = factory.NewWithValidTestData<OrgHeader>().PK;

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";

			ChildEditableService.SetState(factory, ChildEditableServiceStates.Shipment);

			factory.Save();

			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				var plugin = (DocumentScanning.PlugIn.eDocsPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);

				shipment.JS_GoodsDescription = "Test Desc 2";
				shipment.DocManagerInfo.MasterFactory.FactoryForEverythingExceptEDocs.Load<ForwardingShipment>(shipment.PK).MarkLightValidationAsValidForTesting();
				plugin.SelectTabPage();

				AssertNoExceptionThrown(form.TestValidateAndSave);
				AssertEquals("Shipment description should have successfully updated.", "Test Desc 2", shipment.JS_GoodsDescription);
			}
		}

		public void TestMarkAsJobClearMenuItemExist()
		{
			AssertMarkAsJobClearMenuItemExist(true);
			AssertMarkAsJobClearMenuItemExist(false);

			void AssertMarkAsJobClearMenuItemExist(bool registryValue)
			{
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var screenStatus = (shipment as IScreeningStatusProvider)?.ScreeningStatus ?? string.Empty;

				using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
				using (var form = new ShipmentFormForTest(shipment))
				{
					form.Show();
					new DpsMarkJobScreeningStatusClearTest().AssertMenuItemAccessibilityCheckpoint(form, registryValue);
				}
			}
		}

		public void TestMarkJobClearWhenScreeningStatusIsJCLorCLR_ShouldShowWarningMessage()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ShipmentFormForTest(shipment))
			{
				shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
				AssertEquals("Precondition shipment screening status", "JCL", shipment.JS_ScreeningStatus);
				Factory.Save();

				var markJobClearTestHelper = new DpsMarkJobScreeningStatusClearTest();
				markJobClearTestHelper.AssertStatusAlreadyClearOrJobClear(form);

				shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				AssertEquals("Precondition shipment screening status", "CLR", shipment.JS_ScreeningStatus);
				Factory.Save();

				markJobClearTestHelper.AssertStatusAlreadyClearOrJobClear(form);
			}
		}

		public void TestMarkJobClearWhenJobIsNotSaved_ShouldShowWarningMessage()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ShipmentFormForTest(shipment))
			{
				shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				AssertEquals("Precondition shipment screening status", "UNK", shipment.JS_ScreeningStatus);
				new DpsMarkJobScreeningStatusClearTest().AssertSaveBeforeMarkingClear(form);
			}
		}

		[RequiresSTA]
		public void TestJobShipmentWhenMarkingJobClear_ShouldUpdateJobScreeningStatusToJCL()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ShipmentFormForTest(shipment))
			{
				shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				AssertEquals("Precondition shipment screening status", "MAT", shipment.JS_ScreeningStatus);
				Factory.Save();

				new DpsMarkJobScreeningStatusClearTest().AssertScreeenigStatusToJCL(form, shipment);
			}
		}

		[RequiresSTA]
		public void TestMarkAsJobClearWhenSecurityRightsIsDenied_ShouldErrorMessage()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var tmpSecurityCore = GetTemporarySecurityCore();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ShipmentFormForTest(shipment))
			{
				Factory.Save();
				form.Show();

				shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
				Factory.Save();

				tmpSecurityCore.OrgDeniedPartyScreeningAllowJobLevelClear.IsAllowed = false;
				new DpsMarkJobScreeningStatusClearTest().AssertSecurityRightsAccessibilityCheckpoint(form, tmpSecurityCore);
			}
		}

		public void TestDisposeAviationSecuritySupport()
		{
			AssertDisposeAviationSecuritySupportWithTransportMode(Constants.TransportModes.Sea);
			AssertDisposeAviationSecuritySupportWithTransportMode(Constants.TransportModes.Air);
			AssertDisposeAviationSecuritySupportWithTransportMode(Constants.TransportModes.Road);
			AssertDisposeAviationSecuritySupportWithTransportMode(Constants.TransportModes.Rail);
			AssertDisposeAviationSecuritySupportWithTransportMode(Constants.TransportModes.Courier);
		}

		void AssertDisposeAviationSecuritySupportWithTransportMode(string transportMode)
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = transportMode;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			using (var frm = new ShipmentForm(shipment))
			{
				frm.Show();
			}

			Assert("Should have no leaked one or more disposable objects that have not been collected by the GC.", true);
		}

		[RequiresSTA]
		public void TestNCTsPlugInIsAfterBrokerage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKDestination = "FRC3B";

				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();
					var mainTabControl = (ZTemplateTabControl)form.Controls["MainTabControl"];
					var docDataTabPage = mainTabControl.GetTabPage("DocDataTabPage");
					var docDataTabPageIndex = mainTabControl.TabPages.IndexOf(docDataTabPage);
					var brokerageTabPage = mainTabControl.GetTabPage("BrokerageTabPage");
					var nCTsTabPage = mainTabControl.GetTabPage("NCTSTabPage");
					var brokerageTabPageIndex = mainTabControl.TabPages.IndexOf(brokerageTabPage);
					var nCTsTabPageIndex = mainTabControl.TabPages.IndexOf(nCTsTabPage);
					AssertEquals(string.Format("NCTs Tab Page ({0}) should be after Brokerage TabPage ({1})", nCTsTabPageIndex, brokerageTabPageIndex), true, brokerageTabPageIndex < nCTsTabPageIndex);
				}
			}
		}

		public void TestCcsukPlugInIsBeforeBrokerage()
		{
			var originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_MasterBillNum = "11122222222";
				consol.JK_RL_NKDischargePort = "GBLHR";
				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKDestination = "GBLHR";
				shipment.JS_HouseBill = "12345678";

				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();
					var mainTabControl = (ZTemplateTabControl)form.Controls["MainTabControl"];
					var brokerageTabPage = mainTabControl.GetTabPage("BrokerageTabPage");
					var ccsukTabPage = mainTabControl.GetTabPage("CCS-UKTabPage");
					var brokerageTabPageIndex = mainTabControl.TabPages.IndexOf(brokerageTabPage);
					var ccsukTabPageIndex = mainTabControl.TabPages.IndexOf(ccsukTabPage);
					AssertEquals(string.Format("CCSUK Tab Page ({0}) should be before Brokerage TabPage ({1})", ccsukTabPageIndex, brokerageTabPageIndex), true, brokerageTabPageIndex > ccsukTabPageIndex);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountryCode);
			}
		}

		public void TestGuiFactoryServices()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			IForwardingShipmentDocumentSupporterQueryProvider shipmentDocSupporterQueryProvider = Factory.GetValue<IForwardingShipmentDocumentSupporterQueryProvider>();
			AssertNull("shipment doc supporter", shipmentDocSupporterQueryProvider);

			IServicesSelectionProvider servicesSelectionProvider = Factory.GetValue<IServicesSelectionProvider>();
			AssertNull("services selection provider", servicesSelectionProvider);

			using (new ShipmentForm(shipment))
			{
				shipmentDocSupporterQueryProvider = Factory.GetValue<IForwardingShipmentDocumentSupporterQueryProvider>();
				AssertNotNull("shipment doc supporter", shipmentDocSupporterQueryProvider);
				Assert(shipmentDocSupporterQueryProvider is ForwardingShipmentDocumentSupporterGuiQueryProvider);

				servicesSelectionProvider = Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull("services selection provider", servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);
			}
		}

		public void TestReportsBigNumberOfFactories()
		{
			var worryingNumberOfFactories = 500;
			var roughNumberOfFactoriesWhenOpeningAShipmentForm = 10;
			var margin = 15; // must be >= roughNumberOfFactoriesWhenOpeningAShipmentForm + 1

			SystemDataRegistry.Instance.NumberOfFactoriesNeededForWarningReport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, worryingNumberOfFactories);
			int nbFactoriesCounted, nbFactoriesToAdd = 0;

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			var factoriesBeginning = PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories();
			var list = new List<BusinessObjectFactory>();

			if (factoriesBeginning.Length < worryingNumberOfFactories) // There could be some factories already from previous tests
			{
				nbFactoriesToAdd = worryingNumberOfFactories - factoriesBeginning.Length - roughNumberOfFactoriesWhenOpeningAShipmentForm;
				for (int i = 0; i < nbFactoriesToAdd; i++)
				{
					list.Add(new BusinessObjectFactory() { NameForDebugging = "Factory " + i });
				}

				var sw = Stopwatch.StartNew();
				nbFactoriesCounted = PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Length;
				sw.Stop();

				CombineAssertions(() =>
				{
					AssertEquals("Should have added all factories", list.Count + factoriesBeginning.Length, nbFactoriesCounted);
					Assert("Should not have enough factories yet to trigger reporting", nbFactoriesCounted < worryingNumberOfFactories);
					Assert("Should have been quick", 1 > sw.Elapsed.TotalSeconds);
				});
			}

			ErrorReporter.Instance.Clear();

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("Message should be empty", string.Empty, ErrorReporter.LastMessageReported);
					AssertEquals("Should not have reported too many factories", string.Empty, ErrorReporter.LastKeyReported);
				});
			}

			for (int i = nbFactoriesToAdd; i < nbFactoriesToAdd + margin; i++)
			{
				list.Add(new BusinessObjectFactory() { NameForDebugging = "Factory " + i });
			}

			nbFactoriesCounted = PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Length;

			Assert("Should now have enough factories to trigger reporting", nbFactoriesCounted > worryingNumberOfFactories);

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();

				AssertEquals("Should have now reported too many factories", "ShipmentForm_WarningNumberFactories", ErrorReporter.LastKeyReported);
			}

			ErrorReporter.Instance.Clear();
			list.Clear();
		}

		#region ActionMenu

		public void TestActionMenuItems()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				FindActionMenuItem(form).OnPopup(EventArgs.Empty);

				AssertNotNull(FindActionMenuItem(form).MenuItems);
				var resetValuesFromSubShipmentsMenuItem = FindActionMenuItem(form).MenuItems.FindByText("Reset Values From Sub Shipments");
				Assert(resetValuesFromSubShipmentsMenuItem is MenuItem);
				Assert(resetValuesFromSubShipmentsMenuItem.Visible);
				Assert(FindActionMenuItem(form).MenuItems.FindByText("Calculate Inspection Status") is MenuItem);
				Assert(FindActionMenuItem(form).MenuItems.FindByText("Copy harmonized details from Booking to Declaration Commercial Invoice") is MenuItem);
				Assert(FindActionMenuItem(form).MenuItems.FindByText("Transit Warehouse") is MenuItem);
				Assert(FindActionMenuItem(form).MenuItems.FindByText("Recalculate Related Parties for logged in Company") is MenuItem);
				Assert(FindActionMenuItem(form).MenuItems.FindByText("Selection of Rate Commodity (with FMC Tariff ID)") is MenuItem);

				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				FindActionMenuItem(form).OnPopup(EventArgs.Empty);
				Assert(!FindActionMenuItem(form).MenuItems.FindByText("Reset Values From Sub Shipments").Visible);

				shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
				FindActionMenuItem(form).OnPopup(EventArgs.Empty);
				Assert(FindActionMenuItem(form).MenuItems.FindByText("Reset Values From Sub Shipments").Visible);
			}
		}

		public void TestActionMenuItemsForProductWarehouse()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			Factory.Save();

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				FindActionMenuItem(form).OnPopup(EventArgs.Empty);

				AssertNotNull(FindActionMenuItem(form).MenuItems);
				Assert(FindActionMenuItem(form).MenuItems.FindByText("Product Warehouse") is MenuItem);

				var productWarehouseMenu = FindActionMenuItem(form).MenuItems.FindByText("Product Warehouse");
				Assert(productWarehouseMenu.MenuItems.FindByText("Create Warehouse Receive") is MenuItem);
			}
		}

		public void TestActionMenuItemsForProductWarehouse_HaveAttachedReceive()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();

			var receive = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsReceive>());
			var whsReceiveShipmentPivot = Factory.New(ObjectFactory.GetType<IWhsDocketJobPivot>());

			whsReceiveShipmentPivot[WhsDocketJobPivotSchema.WV_ParentId] = shipment.PK;
			whsReceiveShipmentPivot[WhsDocketJobPivotSchema.WV_ParentTableCode] = shipment.TablePrefix;
			whsReceiveShipmentPivot[WhsDocketJobPivotSchema.WV_WD_Docket] = receive.PK;
			whsReceiveShipmentPivot[WhsDocketJobPivotSchema.WV_DocketType] = ((IWhsDocket)receive).WD_DocketType;
			Factory.Save();

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				FindActionMenuItem(form).OnPopup(EventArgs.Empty);

				AssertNotNull(FindActionMenuItem(form).MenuItems);
				Assert(FindActionMenuItem(form).MenuItems.FindByText("Product Warehouse") is MenuItem);

				var productWarehouseMenu = FindActionMenuItem(form).MenuItems.FindByText("Product Warehouse");
				Assert(productWarehouseMenu.MenuItems.FindByText("View/Edit Warehouse Receive") is MenuItem);
				Assert(productWarehouseMenu.MenuItems.FindByText("Override Warehouse Receive") is MenuItem);
			}
		}

		public void TestCalculateDeliveryDueDateMenuItem()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			void AssertCalculateDDDMenuItem(string message, bool shouldBeAvailable)
			{
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();
					var calculateDeliveryDueDateMenuItem = FindActionMenuItem(form).MenuItems.FindByText("Calculate Delivery Due Date");
					AssertEquals(message, shouldBeAvailable, calculateDeliveryDueDateMenuItem != null);
				}
			}

			using (FreightRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				Env.Security.MaintainShipmentDeliveryDueDateOverride.IsAllowed = true;
				AssertCalculateDDDMenuItem("Calculate Delivery Due Date is not available when registry is disabled", false);
			}

			using (FreightRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				Env.Security.MaintainShipmentDeliveryDueDateOverride.IsAllowed = false;
				AssertCalculateDDDMenuItem("Calculate Delivery Due Date is not available when registry is enabled and user does not have security right", true);

				Env.Security.MaintainShipmentDeliveryDueDateOverride.IsAllowed = true;
				AssertCalculateDDDMenuItem("Calculate Delivery Due Date is available when registry is enabled and user has security right", true);
			}
		}

		public void TestCalculateInspectionStatusMenuItem_Template()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.HongKong))
			{
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "USLAX";

				var templateRecord = Factory.New<StmTemplateRecord>();
				var templateRecordProvider = (ITemplateRecordProvider)shipment;
				templateRecordProvider.TemplateRecord = templateRecord;
				templateRecordProvider.IsTemplateRecord = true;
				using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
				{
					templateRecordProvider.SaveToTemplateRecord();
				}

				Factory.Save();

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					var calculateInspectionStatusMenuItem = FindActionMenuItem(form).MenuItems.FindByText("Calculate Inspection Status");
					AssertNotNull(calculateInspectionStatusMenuItem);

					calculateInspectionStatusMenuItem.PerformClick();

					AssertEquals("Expected message", "The Inspection status of Template record is not used when creating Shipment so this function is not applicable.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestCO2ePlugin()
		{
			var shipment = Factory.New<ForwardingShipment>();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			void AssertMenuItem(string pluginMessage, string menuItemMessage, bool shouldBeAvailable)
			{
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();
					var co2ePlugin = form.PlugIns.GetPlugIn(ControllerIDs.CO2ePlugin);
					var menuItem = FindActionMenuItem(form).MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)");
					AssertEquals(pluginMessage, shouldBeAvailable, co2ePlugin != null);
					AssertEquals(menuItemMessage, shouldBeAvailable, menuItem != null);
				}
			}

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(false))
			{
				AssertMenuItem("CO2e Plugin is not added when registry is disabled", "Calculate Greenhouse Gas Emissions (CO2e) action menu item should not be visible when registry is disabled", false);
			}

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(true))
			{
				AssertMenuItem("CO2e Plugin is added when registry is enabled", "Calculate Greenhouse Gas Emissions (CO2e) action menu item should be visible when registry is enabled", true);
			}
		}

		public void TestNoCO2ePluginInShipmentTemplate()
		{
			var shipment = Factory.New<ForwardingShipment>();
			((ITemplateRecordProvider)shipment).IsTemplateRecord = true;
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(true))
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				var co2ePlugin = form.PlugIns.GetPlugIn(ControllerIDs.CO2ePlugin);
				AssertNull(co2ePlugin);
			}
		}

		public void TestDangerousGoodPlugin_ShouldBeAvailable_WhenTheRegistryIsEnabled()
		{
			var shipment = Factory.New<ForwardingShipment>();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			void AssertMenuItem(string pluginMessage, string menuItemMessage, bool shouldBeAvailable)
			{
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();
					var dangerousGoodsPlugin = form.PlugIns.GetPlugIn(ControllerIDs.DangerousGoodsPlugin);
					var menuItem = FindActionMenuItem(form).MenuItems.FindByText("Dangerous Goods Portal");
					AssertEquals(pluginMessage, shouldBeAvailable, dangerousGoodsPlugin != null);
					AssertEquals(menuItemMessage, shouldBeAvailable, menuItem != null);
				}
			}

			using (FreightConfigurationRegistry.Instance.EnableDangerousGoodsPortal.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertMenuItem("Dangerous Goods Plugin is not added when registry is disabled", "Dangerous Goods action menu item should not be visible when registry is disabled", false);
			}

			using (FreightConfigurationRegistry.Instance.EnableDangerousGoodsPortal.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertMenuItem("Dangerous Goods Plugin is added when registry is enabled", "Dangerous Goods action menu item should be visible when registry is enabled", true);
			}
		}

		public void TestNoDangerousGoodsPluginInShipmentTemplate()
		{
			var shipment = Factory.New<ForwardingShipment>();
			((ITemplateRecordProvider)shipment).IsTemplateRecord = true;
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			using (FreightConfigurationRegistry.Instance.EnableDangerousGoodsPortal.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				var dangerousGoodsPlugin = form.PlugIns.GetPlugIn(ControllerIDs.DangerousGoodsPlugin);
				AssertNull(dangerousGoodsPlugin);
			}
		}

		[RequiresSTA]
		public void TestRegenerateHouseBillNumberActionMenu_Show()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "TEST";

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();

				FindActionMenuItem(shipmentForm).OnPopup(EventArgs.Empty);

				var menuItem = FindActionMenuItem(shipmentForm).MenuItems.FindByText("Regenerate House Bill Number");

				AssertNotNull("Regenerate House Bill Number is in Actions Menu", menuItem);

				menuItem.PerformClick();

				Assert("Shipment change", shipment.HasChanges);

				var houseBillTextBox = shipmentForm.ShipmentUserControl.Controls.Find("HouseBill", true).OfType<ZTextBox>().First();

				AssertEquals("HouseBill TextBox Text", "PENDING ALLOCATION..", houseBillTextBox.Text);
			}
		}

		public void TestRegenerateHouseBillNumberActionMenu_ShowErrorMessage_WhenHouseBillNumberIsReadOnlyDueToPhaseAndTransportModeIsNotAir()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "TEST";
			shipment.ReadOnly = true;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();

				FindActionMenuItem(shipmentForm).OnPopup(EventArgs.Empty);

				var menuItem = FindActionMenuItem(shipmentForm).MenuItems.FindByText("Regenerate House Bill Number");
				AssertNotNull("Regenerate House Bill Number is in Actions Menu", menuItem);
				menuItem.PerformClick();
				AssertEquals("The House Bill Number cannot be regenerated while the House Bill field has been set to read-only status",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRegenerateHouseBillNumberActionMenu_ShowErrorMessage_WhenHouseBillNumberIsReadOnlyDueToPhaseAndTransportModeIsAirAndRegistryIsFalse()
		{
			using (FreightRegistry.Instance.EnforceUniqueHAWBNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consol = Factory.New<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				shipment.JS_HouseBill = "TEST";
				shipment.ReadOnly = true;
				shipment.JS_TransportMode = Constants.TransportModes.Air;

				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
				using (var shipmentForm = new ShipmentForm(shipment))
				{
					shipmentForm.Show();

					FindActionMenuItem(shipmentForm).OnPopup(EventArgs.Empty);

					var menuItem = FindActionMenuItem(shipmentForm).MenuItems.FindByText("Regenerate House Bill Number");
					AssertNotNull("Regenerate House Bill Number is in Actions Menu", menuItem);
					menuItem.PerformClick();
					AssertEquals("When House Bill is read only, it can only be generated for Air shipment if “Registry > Freight > AWB > HAWB > Enforce Unique HAWB Numbers” is set to Yes",
						UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		[RequiresSTA]
		public void TestRegenerateHouseBillNumberActionMenu_WillNotShowErrorMessBoolEffectiveDateage_WhenHouseBillNumberIsReadOnlyDueToPhaseAndTransportModeIsAirAndRegistryIsTrue()
		{
			using (FreightRegistry.Instance.EnforceUniqueHAWBNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.New<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				shipment.JS_HouseBill = "TEST";
				shipment.ReadOnly = true;
				shipment.JS_TransportMode = Constants.TransportModes.Air;

				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
				using (var shipmentForm = new ShipmentForm(shipment))
				{
					shipmentForm.Show();

					FindActionMenuItem(shipmentForm).OnPopup(EventArgs.Empty);

					var menuItem = FindActionMenuItem(shipmentForm).MenuItems.FindByText("Regenerate House Bill Number");

					AssertNotNull("Regenerate House Bill Number is in Actions Menu", menuItem);

					var userNotificationTextPreviousValue = UnitTestUserNotification.Instance.LastMessage.Text;

					menuItem.PerformClick();
					AssertEquals(userNotificationTextPreviousValue, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#endregion

		[RequiresSTA]
		public void TestCustomizeMenuItemsShow_DocDataIsCheckedOrUnchecked()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			Factory.Save();

			using var shipmentForm = new ShipmentForm(shipment);
			shipmentForm.Show();

			var viewMenuItem = FindMenuItem(shipmentForm, "View");
			viewMenuItem.OnPopup(EventArgs.Empty);
			var docDataMenuItem = viewMenuItem.MenuItems.FindByText("Doc Data");

			AssertNotNull("Doc Data", docDataMenuItem);
			AssertEquals(true, docDataMenuItem.Checked);

			var documentsMenuItem = FindMenuItem(shipmentForm, "&Documents");
			documentsMenuItem.OnPopup(EventArgs.Empty);

			AssertNotNull("Customize (Documents)", documentsMenuItem.MenuItems.FindByText("Customize (Documents)"));
			AssertNotNull("Customize (Forms)", documentsMenuItem.MenuItems.FindByText("Customize (Forms)"));

			docDataMenuItem.PerformClick();
			Factory.Save();
			shipmentForm.Close();

			using var shipmentForm2 = new ShipmentForm(shipment);
			shipmentForm2.Show();

			viewMenuItem = FindMenuItem(shipmentForm2, "View");
			viewMenuItem.OnPopup(EventArgs.Empty);
			docDataMenuItem = viewMenuItem.MenuItems.FindByText("Doc Data");

			AssertNotNull("Doc Data", docDataMenuItem);
			AssertEquals(false, docDataMenuItem.Checked);

			documentsMenuItem = FindMenuItem(shipmentForm2, "&Documents");
			documentsMenuItem.OnPopup(EventArgs.Empty);

			AssertNotNull("Customize (Documents)", documentsMenuItem.MenuItems.FindByText("Customize (Documents)"));
			AssertNotNull("Customize (Forms)", documentsMenuItem.MenuItems.FindByText("Customize (Forms)"));
		}

		public void TestElectronicMessagingMenuItems_Easipass()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_RL_NKOrigin = "CNSHA";

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			var chineseBranch = Factory.New<GlbBranch>();
			chineseBranch.GB_RL_NKHomePort = "CNSHA";
			chineseBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			using (chineseBranch.SetAsTemporaryContext())
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.Cast<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Electronic Messaging");

				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				AssertContains("Electronic Menu Items for sea shipment (CNSHA -> AUSYD)",
@"Electronic Messaging
   Booking Party
      Draft Bill of Lading
   Carrier
      eManifest",
					electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
			}
		}

		public void TestElectronicMenuItems_ACASShipmentReport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USJFK";

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.OfType<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Electronic Messaging");

				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);
				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				AssertContains(@"Electronic Messaging
   Advanced Air Cargo Report
      ACAS Shipment Report", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
			}
		}

		public void TestGivenElectronicMessagingMenuItems_WhenHVLShipmentIsUSAir_ThenShowACASReportMenuItem()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USJFK";
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.OfType<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Electronic Messaging");

				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);
				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				AssertContains("Expected it to display menu item 'ACAS Shipment Report' in 'Advanced Air Cargo Report' because it is not a HVL shipment with US destination",
@"Electronic Messaging
   Advanced Air Cargo Report
      ACAS Shipment Report", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
			}

			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.OfType<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Electronic Messaging");

				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				AssertNotContains("Expected it not to display menu item 'ACAS Shipment Report' in 'Advanced Air Cargo Report' because it is a HVL shipment", "ACAS Shipment Report", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
			}
		}

		public void TestBolMenuItemWithNoPermission()
		{
			AssertBolMenuItem(null, false, WhatAreYouDoingYouPermissionlessFool);
		}

		public void TestBolMenuItemWithPermission_AnswersNo()
		{
			AssertBolMenuItem(DialogResult.No, true, YouThinkYouHavePermissionForAnythingMrBigShot);
		}

		public void TestBolMenuItemWithPermission_AnswersYes()
		{
			AssertBolMenuItem(DialogResult.Yes, true, YouThinkYouHavePermissionForAnythingMrBigShot);
		}

		void AssertBolMenuItem(DialogResult? answer, bool allowPrinting, string expectedMessage)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "USCHI";
				shipment.JS_RL_NKDestination = "AUSYD";
				var packline = shipment.OuterPackLines.AddNew();
				packline.JL_LinePrice = 10000;
				packline.JL_HarmonisedCode = "12345678";

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.AddRelatedParty(GlbCompany.CurrentCompany.GC_OH_OrgProxy, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.All, Constants.ContainerModes.FCL, GlbCompany.CurrentCompany);
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

				Factory.Save();

				AssertGreaterThan<decimal>("Precondition: PackLine line price should be greater than ShipmentHTSMaximumValue", packline.JL_LinePrice, (decimal)ObjectFactory.Get<US.IUSCustomsDataRegistry>().ShipmentHTSMaximumValue.Value);

				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
				Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed = allowPrinting;
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					var bolMenuItem = FindMenuItem(form, "&Documents", "Departure", "Bill Of Lading", "Bill Of Lading");
					AssertNotNull("Precondition: Bill Of Lading menu item exists", bolMenuItem);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					bolMenuItem.PerformClick();
					if (answer != null)
					{
						UnitTestUserNotification.Instance.AddAnswer((DialogResult)answer);
					}
					AssertEquals("should show correct message", string.Format(expectedMessage, shipment.JS_UniqueConsignRef), UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestElectronicMenuItems_CCT()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRSAO";

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.OfType<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Electronic Messaging");

				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);
				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				AssertContains(@"Electronic Messaging
   Advanced Air Cargo Report
      CCT Shipment Report", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());

				var cctMenuItem = FindMenuItem(form, "Electronic Messaging", "Advanced Air Cargo Report", "CCT Shipment Report");

				shipment.JS_ShipmentType = "CLB";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Factory.Save();
				cctMenuItem.PerformClick();
				AssertEquals("Advanced Air Cargo Reporting is only available from Shipments with types STD, BCN, CLD, ASM or HVL.", UnitTestUserNotification.Instance.LastMessage.Text);

				var masterShipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = "STD";
				shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
				masterShipment.JS_ShipmentType = "CLD";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Factory.Save();
				cctMenuItem.PerformClick();
				AssertEquals("In co-load scenario, Advance Air Cargo Reporting should be done from the Co-Load Master (CLD) shipment.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestElectronicMenuItems_FrenchPorts_RelatedPort_Export()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "FRCAL";
				shipment.JS_RL_NKDestination = "BRSAO";
				shipment.JS_RL_NKLoadPort = "FRCAL";
				shipment.JS_PackingMode = "LCL";

				var exportReceivingDepot = Factory.New<OrgHeader>();
				exportReceivingDepot.OH_FullName = "FromFR";
				exportReceivingDepot.OH_RL_NKClosestPort = "FRCAL";
				exportReceivingDepot.MainAddress.Address1 = "Unit 13";
				exportReceivingDepot.MainAddress.Address2 = "4 Lost Lane";
				exportReceivingDepot.MainAddress.City = "French";
				exportReceivingDepot.MainAddress.Postcode = "2000";
				exportReceivingDepot.MainAddress.OA_RN_NKCountryCode = "FR";

				shipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.MainAddress.PK;

				var consol = shipment.Consols.AddNew();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "FRCAL";
				consol.JK_RL_NKDischargePort = "BRSAO";

				var transport = consol.Transports[0];
				transport.JW_RL_NKLoadPort = "FRCAL";
				transport.JW_RL_NKDiscPort = "BRSAO";

				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.OfType<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Electronic Messaging");

					AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);
					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertContains(@"Port Messaging
      Export
         File Creation Request (DOS) (FR)
         Customs Check CAED (FR)
         Goods Received CRESA (FR)", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestElectronicMenuItems_FrenchPorts_LoadPort_Export()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "FRCAL";
				shipment.JS_RL_NKDestination = "BRSAO";
				shipment.JS_RL_NKLoadPort = "FRCAL";
				shipment.JS_PackingMode = "LCL";

				var consol = shipment.Consols.AddNew();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "FRCAL";
				consol.JK_RL_NKDischargePort = "BRSAO";

				var transport = consol.Transports[0];
				transport.JW_RL_NKLoadPort = "FRCAL";
				transport.JW_RL_NKDiscPort = "BRSAO";

				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.OfType<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Electronic Messaging");

					AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);
					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertContains(@"Port Messaging
      Export
         Customs Check CAED (FR)", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestElectronicMenuItems_FrenchPorts_RelatedPort_Import()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "BRSAO";
				shipment.JS_RL_NKDestination = "FRCAL";
				shipment.JS_RL_NKDischargePort = "FRCAL";
				shipment.JS_PackingMode = "LCL";

				var importReceivingDepot = Factory.New<OrgHeader>();
				importReceivingDepot.OH_FullName = "FromFR";
				importReceivingDepot.OH_RL_NKClosestPort = "FRCAL";
				importReceivingDepot.MainAddress.Address1 = "Unit 13";
				importReceivingDepot.MainAddress.Address2 = "4 Lost Lane";
				importReceivingDepot.MainAddress.City = "French";
				importReceivingDepot.MainAddress.Postcode = "2000";
				importReceivingDepot.MainAddress.OA_RN_NKCountryCode = "FR";

				shipment.JS_OA_ImportReleaseDepot = importReceivingDepot.MainAddress.PK;

				var consol = shipment.Consols.AddNew();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "BRSAO";
				consol.JK_RL_NKDischargePort = "FRCAL";

				var transport = consol.Transports[0];
				transport.JW_RL_NKLoadPort = "BRSAO";
				transport.JW_RL_NKDiscPort = "FRCAL";

				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.OfType<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Electronic Messaging");

					AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);
					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertContains(@"Port Messaging
      Import
         File Creation Request (DOS) (FR)
         Customs Check CAED (FR)", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestElectronicMenuItems_FrenchPorts_DischargePort_Import()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "BRSAO";
				shipment.JS_RL_NKDestination = "FRCAL";
				shipment.JS_RL_NKDischargePort = "FRCAL";
				shipment.JS_PackingMode = "LCL";

				var consol = shipment.Consols.AddNew();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "BRSAO";
				consol.JK_RL_NKDischargePort = "FRCAL";

				var transport = consol.Transports[0];
				transport.JW_RL_NKLoadPort = "BRSAO";
				transport.JW_RL_NKDiscPort = "FRCAL";

				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.OfType<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Electronic Messaging");

					AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);
					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertContains(@"Port Messaging
      Import
         Customs Check CAED (FR)", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		[RequiresSTA]
		public void TestElectronicMenuItems_MexicanPorts_Export()
		{
			using (FreightRegistry.Instance.EnableMexicanPortIntegrationFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "MXCUU";
				shipment.JS_RL_NKDestination = "FRCAL";

				var consol = shipment.Consols.AddNew();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "MXCUU";
				consol.JK_RL_NKDischargePort = "FRCAL";

				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.OfType<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Electronic Messaging");

					AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);
					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertContains(@"Electronic Messaging
   Port Messaging
      Export
         House AWB (MX)",
		 electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestElectronicMenuItems_MexicanPorts_Disabled()
		{
			using (FreightRegistry.Instance.EnableMexicanPortIntegrationFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "MXCUU";
				shipment.JS_RL_NKDestination = "BRSAO";

				var consol = shipment.Consols.AddNew();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "MXCUU";
				consol.JK_RL_NKDischargePort = "FRCAL";

				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.OfType<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Electronic Messaging");

					AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);
					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertNotContains("Port Messaging", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestSendTransitWarehouseInstructionWithComfirmation()
		{
			using (FreightRegistry.Instance.EnableMexicanPortIntegrationFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "MCUU";
				shipment.JS_RL_NKDestination = "BRSAO";
				shipment.JS_UniqueConsignRef = "SH00001";
				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus;
				packLine1.JL_PackageCount = 1;
				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies;
				packLine2.JL_PackageCount = 2;
				var packLine3 = shipment.OuterPackLines.AddNew();
				packLine3.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed;
				packLine3.JL_PackageCount = 3;
				var packLine4 = shipment.OuterPackLines.AddNew();
				packLine4.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown;
				packLine4.JL_PackageCount = 4;
				new TestObjectCreator(shipment.Factory).CreateJob(shipment, false);
				Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(mi => mi.Text == "Actio&ns").OnPopup(EventArgs.Empty);

					var sendPickupTWReceiptInstructionMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Actio&ns")
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Transit Warehouse")
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Pickup TW")
						.MenuItems
						.Cast<MenuItem>().
						FirstOrDefault(mi => mi.Text == "Send Receipt Instruction");

					AssertNotNull("Pickup TW's Send Receipt Instruction menu item exists", sendPickupTWReceiptInstructionMenuItem);

					sendPickupTWReceiptInstructionMenuItem.PerformClick();
					AssertEquals(4, shipment.OuterPackLines.Count);
					Assert(UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.Text == "TW Matching Status of some shipment pack lines are not confirmed. Are you sure you want to proceed?"));
				}
			}

			UnitTestUserNotification.Instance.ClearMessages();
			UnitTestUserNotification.Instance.ClearUserResponses();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (FreightRegistry.Instance.EnableMexicanPortIntegrationFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "MCUU";
				shipment.JS_RL_NKDestination = "BRSAO";
				shipment.JS_UniqueConsignRef = "SH00002";
				Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(mi => mi.Text == "Actio&ns").OnPopup(EventArgs.Empty);

					var sendPickupTWReceiptInstructionMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Actio&ns")
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Transit Warehouse")
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Pickup TW")
						.MenuItems
						.Cast<MenuItem>().
						FirstOrDefault(mi => mi.Text == "Send Receipt Instruction");

					AssertNotNull("Pickup TW's Send Receipt Instruction menu item exists", sendPickupTWReceiptInstructionMenuItem);

					sendPickupTWReceiptInstructionMenuItem.PerformClick();
					Assert(!UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.Text?.Contains("TW Matching Status is not confirmed.  Are you sure you want to proceed?") ?? false));
				}
			}
		}

		public void TestSendTransitWarehousePrepareDispatchInstruction_OnSave_WhenShipmentHasDCNAuthorizedToDispatchAttached()
		{
			SendTransitWarehouseDispatchOrPrepareDispatchInstruction_OnSaveCore(false, false, false);
		}

		public void TestSendTransitWarehouseDispatchInstruction_OnSave_WhenShipmentHasNoDCNAuthorizedToDispatchAttached()
		{
			SendTransitWarehouseDispatchOrPrepareDispatchInstruction_OnSaveCore(true, false, false);
		}

		public void TestShowErrorMessage_OnSave__WhenShipmentHasDCNAttachedWhichIsSplit()
		{
			SendTransitWarehouseDispatchOrPrepareDispatchInstruction_OnSaveCore(true, true, true);
		}

		public void SendTransitWarehouseDispatchOrPrepareDispatchInstruction_OnSaveCore(bool isDcnAuthorizedToDispatchAssigned, bool isDcnSplit, bool showsErrorMessage)
		{
			var depot = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var shipment = GetShipmentWithNoErrors();
			shipment.JS_OA_ExportReceivingDepot = depot.PK;
			var communicationMode = depot.Header.EDICommunicationsModes.AddNew();
			communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			communicationMode.EK_Destination = "Blah";
			communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationMode.EK_Module = "SHP";

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseType = "TRW";
			warehouse.WW_IsActive = true;
			warehouse.WW_IsActive = true;
			warehouse.WW_OA_WarehouseAddress = depot.PK;

			if (isDcnAuthorizedToDispatchAssigned)
			{
				var dispatchConsignment = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
				dispatchConsignment.WDC_ConsignmentID = "DCN1";
				dispatchConsignment.WDC_ParentID = shipment.PK;
				dispatchConsignment.WDC_ParentTableCode = shipment.TablePrefix;
				dispatchConsignment.WDC_WW_Warehouse = warehouse.PK;
			}

			if (isDcnSplit)
			{
				var dispatchConsignment = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
				dispatchConsignment.WDC_ConsignmentID = "DCN2";
				dispatchConsignment.WDC_ParentID = shipment.PK;
				dispatchConsignment.WDC_ParentTableCode = shipment.TablePrefix;
				dispatchConsignment.WDC_WW_Warehouse = warehouse.PK;
			}

			Factory.Save();

			var transitUniversalServiceMock = new Mock<ITransitUniversalService>();
			transitUniversalServiceMock.Setup(m => m.GetNotificationForInstruction(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new WarningNotification("Instruction has been queued to send. Check DEX logs for details."));

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (ObjectFactory.Substitute(nameof(ITransitUniversalService), _ => transitUniversalServiceMock.Object))
			using (var form = new ShipmentForm(shipment))
			{
				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus;
				packLine1.JL_OA_LastKnownTransitWarehouseAddress = depot.PK;
				packLine1.JL_PackageCount = 1;
				packLine1.BlindPackageAttached = true;
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				form.FireSaveButton();
				Assert("Skip confirmation check", !UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.Text == "TW Matching Status of some shipment pack lines are not confirmed. Are you sure you want to proceed?"));

				if (showsErrorMessage)
				{
					Assert("Error message is shown", UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.Text?.Contains("Failed to send Dispatch Instruction as blind packages cannot be attached or detached when the Shipment is already linked to a split DCN.") ?? false));
				}
				else
				{
					if (isDcnAuthorizedToDispatchAssigned)
					{
						Assert("Info message is shown", UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.Text?.Contains("You have modified Blind packages attached to this shipment. A Dispatch Instruction will be sent to Transit Warehouse.") ?? false));
					}
					else
					{
						Assert("Info message is shown", UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.Text?.Contains("You have modified Blind packages attached to this shipment. A Prepare Dispatch Instruction will be sent to Transit Warehouse.") ?? false));
					}

					AssertEquals(@"The Transit Warehouse Instruction has been sent.
Processing Shipment BlahBlahBlah
Universal Shipment queued for sending to Organization [MIDINT].

Warning: Instruction has been queued to send. Check DEX logs for details.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(false, shipment.RequireSendingPrepareDispatchTWInstructionForBlindPackages);

					var newFactory = Factory.CreateNewFactory();
					shipment = newFactory.Load<ForwardingShipment>(shipment.PK);
					var logs = shipment.Logs.GetAllLogs();
					var svrEvent = logs.Cast<StmALog>().Single(l => l.SL_SE_NKEvent == "SVR");
					AssertNotNull("SVR event created", svrEvent);

					var expectedReference = isDcnAuthorizedToDispatchAssigned ? FormattableString.Invariant($"|FAC=CFS|LOC=AUBNE|TYP=Dispatch") : FormattableString.Invariant($"|FAC=CFS|LOC=AUBNE|TYP=Prepare Dispatch|WHS=MIDINT");
					AssertEquals("SVR event reference", expectedReference, svrEvent.SL_Reference);

					Assert("DEX event created", logs.Cast<StmALog>().Any(l => l.SL_SE_NKEvent == "DEX"));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					transitUniversalServiceMock.VerifyAll();
				}
			}
		}

		public void TestSendTransitWarehouseDispatchInstructionWithDifferentShipment()
		{
			var depot = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var shipment = GetShipmentWithNoErrors();
			shipment.JS_OA_ExportReceivingDepot = depot.PK;
			var communicationMode = depot.Header.EDICommunicationsModes.AddNew();
			communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			communicationMode.EK_Destination = "Blah";
			communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationMode.EK_Module = "SHP";

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseType = "TRW";
			warehouse.WW_IsActive = true;
			warehouse.WW_OA_WarehouseAddress = depot.PK;

			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var receiveConsignment = helper.CreateReceiveConsignment("RC0001", warehouse.PK);
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_JobID = "RC0001";
			packageJob.KJ_ParentID = receiveConsignment.PK;
			packageJob.KJ_ParentTableCode = receiveConsignment.TablePrefix;
			var package = ReadOnlyPackageForLink.CreatePkgPackageSample(packageJob, "package1", "PLT", 1, goodsDescription: "GD", requiresTemperatureControl: false, requiredTemperatureMaximum: 6, requiredTemperatureMinimum: 6, requiredTemperatureUnit: "C");

			var anotherShipment = GetShipmentWithNoErrors();
			anotherShipment.JS_UniqueConsignRef = "BlahBlah";
			anotherShipment.JS_OA_ExportReceivingDepot = depot.PK;
			anotherShipment.OuterPackLines.AddNew().AdditionalReferenceNumbers.AddNewIfNotExist(ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC, "RC0001");
			((ITransitWarehouseParent)anotherShipment).AttachPackages(new ITransitPackage[] { package });

			Factory.Save();

			var transitUniversalServiceMock = new Mock<ITransitUniversalService>();
			transitUniversalServiceMock.Setup(m => m.GetNotificationForInstruction(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new WarningNotification("Instruction has been queued to send. Check DEX logs for details."));

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (ObjectFactory.Substitute(nameof(ITransitUniversalService), _ => transitUniversalServiceMock.Object))
			using (var form = new ShipmentForm(shipment))
			{
				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus;
				packLine1.JL_OA_LastKnownTransitWarehouseAddress = depot.PK;
				packLine1.JL_PackageCount = 1;
				packLine1.BlindPackageAttached = true;
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				((ITransitWarehouseParent)shipment).GetPreAttachingValidationMessage(new ITransitPackage[] { package });

				form.FireSaveButton();
				Assert("Skip confirmation check", !UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.Text == "TW Matching Status of some shipment pack lines are not confirmed. Are you sure you want to proceed?"));

				Assert("Info message is shown", UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.Text?.Contains("You have modified Blind packages attached to this shipment. A Prepare Dispatch Instruction will be sent to Transit Warehouse.") ?? false));

				AssertEquals(@"The Transit Warehouse Instruction has been sent.
Processing Shipment BlahBlahBlah
Universal Shipment queued for sending to Organization [MIDINT].

Warning: Instruction has been queued to send. Check DEX logs for details.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, shipment.RequireSendingPrepareDispatchTWInstructionForBlindPackages);

				var newFactory = Factory.CreateNewFactory();
				shipment = newFactory.Load<ForwardingShipment>(shipment.PK);
				var logs = shipment.Logs.GetAllLogs();
				var svrEvent = logs.Cast<StmALog>().Single(l => l.SL_SE_NKEvent == "SVR");
				AssertNotNull("SVR event created", svrEvent);

				var expectedReference = FormattableString.Invariant($"|FAC=CFS|LOC=AUBNE|TYP=Prepare Dispatch|WHS=MIDINT");
				AssertEquals("SVR event reference", expectedReference, svrEvent.SL_Reference);

				Assert("DEX event created", logs.Cast<StmALog>().Any(l => l.SL_SE_NKEvent == "DEX"));

				anotherShipment = newFactory.Load<ForwardingShipment>(anotherShipment.PK);
				var anotherShipmentLogs = anotherShipment.Logs.GetAllLogs();
				var anotherShipmentSvrEvent = anotherShipmentLogs.Cast<StmALog>().Single(l => l.SL_SE_NKEvent == "SVR");
				AssertNotNull("SVR event created", anotherShipmentSvrEvent);
				var expectedReference2 = FormattableString.Invariant($"|FAC=CFS|LOC=AUBNE|TYP=Prepare Dispatch|WHS=MIDINT");
				AssertEquals("SVR event reference", expectedReference2, anotherShipmentSvrEvent.SL_Reference);
				Assert("DEX event created", anotherShipmentLogs.Cast<StmALog>().Any(l => l.SL_SE_NKEvent == "DEX"));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				transitUniversalServiceMock.VerifyAll();
			}
		}

		public void TestSendTransitWarehouseDispatchInstruction_WhenDCNIsSplit()
		{
			var depot = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var shipment = GetShipmentWithNoErrors();
			shipment.JS_OA_ExportReceivingDepot = depot.PK;

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseType = "TRW";
			warehouse.WW_IsActive = true;
			warehouse.WW_IsActive = true;
			warehouse.WW_OA_WarehouseAddress = depot.PK;

			var dispatchConsignment1 = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			dispatchConsignment1.WDC_ConsignmentID = "DCN1";
			dispatchConsignment1.WDC_ParentID = shipment.PK;
			dispatchConsignment1.WDC_ParentTableCode = shipment.TablePrefix;
			dispatchConsignment1.WDC_WW_Warehouse = warehouse.PK;

			var dispatchConsignment2 = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			dispatchConsignment2.WDC_ConsignmentID = "DCN2";
			dispatchConsignment2.WDC_ParentID = shipment.PK;
			dispatchConsignment2.WDC_ParentTableCode = shipment.TablePrefix;
			dispatchConsignment2.WDC_WW_Warehouse = warehouse.PK;

			Factory.Save();

			var transitUniversalServiceMock = new Mock<ITransitUniversalService>();
			transitUniversalServiceMock.Setup(m => m.GetNotificationForInstruction(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new WarningNotification("Instruction has been queued to send. Check DEX logs for details."));

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (ObjectFactory.Substitute(nameof(ITransitUniversalService), _ => transitUniversalServiceMock.Object))
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddYesAnswer();

				form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(mi => mi.Text == "Actio&ns").OnPopup(EventArgs.Empty);
				var sendPickupTWDispatchInstructionMenuItem = (ZMenuItem)form.Menu
				.MenuItems
				.Cast<MenuItem>()
				.FirstOrDefault(mi => mi.Text == "Actio&ns")
				.MenuItems
				.Cast<MenuItem>()
				.FirstOrDefault(mi => mi.Text == "Transit Warehouse")
				.MenuItems
				.Cast<MenuItem>()
				.FirstOrDefault(mi => mi.Text == "Pickup TW")
				.MenuItems
				.Cast<MenuItem>().
				FirstOrDefault(mi => mi.Text == "Send Dispatch Instruction");

				AssertNotNull("Pickup TW's Send Dispatch Instruction menu item exists", sendPickupTWDispatchInstructionMenuItem);

				sendPickupTWDispatchInstructionMenuItem.PerformClick();

				Assert("Error message is shown", UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.Text?.Contains("Failed to send Dispatch Instruction - Transit Warehouse Dispatch Consignments have already been split.") ?? false));
			}
		}

		public void TestElectronicMenuItems_ConsolidationAdvice_Enabled()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var coloadShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.CoLoadShipments.Add(coloadShipment);

			var coloadShipmentNumber = coloadShipment.Numbers.AddNew();
			coloadShipmentNumber.CE_EntryNum = "SHP1223";
			coloadShipmentNumber.CE_EntryType = CustomsReferenceNumberType.eHubInterchangeReference.HIR;

			AssertEquals(true, shipment.CoLoadShipments.OfType<ForwardingShipment>().Any(x => x.HasHIRAndStartWithSHPNumber));

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.Cast<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Electronic Messaging");
				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);
				var messagesToBookingPartyMenuItem = GetChildMenuItemByTextSingleOrDefault(electronicMessagingMenuItem, "Booking Party");

				AssertNotNull("Booking Party menu item exists", messagesToBookingPartyMenuItem);
				AssertContains("Consolidation Advice", messagesToBookingPartyMenuItem.GetVisibleMenuItemsCaptions());
			}
		}

		public void TestElectronicMenuItems_ConsolidationAdvice_Enabled_STDShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var shipmentNumber = shipment.Numbers.AddNew();
			shipmentNumber.CE_EntryNum = "SHP1223";
			shipmentNumber.CE_EntryType = CustomsReferenceNumberType.eHubInterchangeReference.HIR;

			AssertEquals(true, shipment.HasHIRAndStartWithSHPNumber);

			var log = shipment.Logs.AddNew(Events.StatusUpdated, new KeyValuePair<string, string>(Params.Type, Constants.EventReferenceMessageTypes.ShipmentStatus));
			log.UpdateReference($"|{Params.Type}={Constants.EventReferenceMessageTypes.ShipmentStatus}|{Params.New}={ShipmentStatusList.Codes.ElectronicBooking}");
			AssertEquals(true, shipment.IsElectronicBookingReceived);

			AssertEquals(null, shipment.CoLoadMasterShipment);

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.Cast<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Electronic Messaging");
				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				var messagesToBookingPartyMenuItem = GetChildMenuItemByTextSingleOrDefault(electronicMessagingMenuItem, "Booking Party");

				AssertNotNull("Booking Party menu item exists", messagesToBookingPartyMenuItem);
				AssertContains("Consolidation Advice", messagesToBookingPartyMenuItem.GetVisibleMenuItemsCaptions());
			}
		}

		public void TestElectronicMenuItems_BookingRequest_Enabled()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c1bb";
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_CarrierName = "testship";
			shippingLine.RSL_IsNVO = true;
			shippingLine.RSL_IsShippingLine = false;
			shippingLine.RSL_IsCW1User = true;
			shippingLine.RSL_BookingRequestAvailable = true;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "BBG";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			shipment.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;

			AssertNotEquals("Precondition:", shipment.BookedShippingLine, null);
			AssertEquals("Precondition", shipment.CarrierOfBookedShippingLineIsNVOCC, true);
			AssertEquals("Precondition", shipment.CarrierOfBookedShippingLineIsCW1User, true);
			AssertEquals("Precondition", shipment.CarrierOfBookedShippingLineHasBookingRequestIntegration, true);

			AssertElectronicMenuItems_BookingRequest(true, shipment);
		}

		public void TestElectronicMenuItems_BookingRequest_CarrierOfBookedShippingLineIsNotNVOCC_Disabled()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c1bb";
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_CarrierName = "testship";
			shippingLine.RSL_IsNVO = false;
			shippingLine.RSL_IsCW1User = true;
			shippingLine.RSL_BookingRequestAvailable = true;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "BBG";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			shipment.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;

			AssertNotEquals("Precondition:", shipment.BookedShippingLine, null);
			AssertEquals("Precondition", shipment.CarrierOfBookedShippingLineIsNVOCC, false);
			AssertEquals("Precondition", shipment.CarrierOfBookedShippingLineIsCW1User, true);
			AssertEquals("Precondition", shipment.CarrierOfBookedShippingLineHasBookingRequestIntegration, true);

			AssertElectronicMenuItems_BookingRequest(false, shipment);
		}

		public void TestElectronicMenuItems_BookingRequest_CarrierOfBookedShippingLineIsNotCW1User_Disabled()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c1bb";
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_CarrierName = "testship";
			shippingLine.RSL_IsNVO = true;
			shippingLine.RSL_IsCW1User = false;
			shippingLine.RSL_BookingRequestAvailable = true;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "BBG";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			shipment.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;

			AssertNotEquals("Precondition:", shipment.BookedShippingLine, null);
			AssertEquals("Precondition", shipment.CarrierOfBookedShippingLineIsNVOCC, true);
			AssertEquals("Precondition", shipment.CarrierOfBookedShippingLineIsCW1User, false);
			AssertEquals("Precondition", shipment.CarrierOfBookedShippingLineHasBookingRequestIntegration, true);

			AssertElectronicMenuItems_BookingRequest(false, shipment);
		}

		public void TestElectronicMenuItems_BookingRequest_CarrierOfBookedShippingLineDoesNotHaveAvailableIntegration_Disabled()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c1bb";
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_CarrierName = "testship";
			shippingLine.RSL_IsNVO = true;
			shippingLine.RSL_IsCW1User = true;
			shippingLine.RSL_BookingRequestAvailable = false;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "BBG";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			shipment.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;

			AssertNotEquals("Precondition:", shipment.BookedShippingLine, null);
			AssertEquals("Precondition", shipment.CarrierOfBookedShippingLineIsNVOCC, true);
			AssertEquals("Precondition", shipment.CarrierOfBookedShippingLineIsCW1User, true);
			AssertEquals("Precondition", shipment.CarrierOfBookedShippingLineHasBookingRequestIntegration, false);

			AssertElectronicMenuItems_BookingRequest(false, shipment);
		}

		public void TestElectronicMenuItems_BookingRequest_HasConsol_Enabled()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AgentType = "CLD";

			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c1bb";
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_CarrierName = "testship2";
			shippingLine.RSL_IsNVO = true;
			shippingLine.RSL_IsCW1User = true;
			shippingLine.RSL_BookingRequestAvailable = true;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_Code = "ABC";
			creditor.OH_RSL_ShippingLine = shippingLine.PK;

			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			AssertNotEquals("Precondition:", consol.Creditor, null);
			AssertEquals("Precondition", consol.CreditorIsNVOCC, true);
			AssertEquals("Precondition", consol.CreditorIsCW1User, true);
			AssertEquals("Precondition", consol.CreditorHasBookingRequestIntegration, true);

			AssertElectronicMenuItems_BookingRequest(true, shipment);
		}

		public void TestElectronicMenuItems_BookingRequest_HasConsol_Disabled()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AgentType = "CLD";

			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c1bb";
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_CarrierName = "testship";
			shippingLine.RSL_IsNVO = true;
			shippingLine.RSL_IsCW1User = true;
			shippingLine.RSL_BookingRequestAvailable = true;

			AssertElectronicMenuItems_BookingRequest(false, shipment);
		}

		public void TestElectronicMenuItems_BookingRequest_HasConsol_ConsolCreditorIsNotNVOCC_Disabled()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AgentType = "CLD";

			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c1bb";
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_CarrierName = "testship";
			shippingLine.RSL_IsNVO = false;
			shippingLine.RSL_IsCW1User = true;
			shippingLine.RSL_BookingRequestAvailable = true;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_Code = "ABC";
			creditor.OH_RSL_ShippingLine = shippingLine.PK;

			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			AssertNotEquals("Precondition:", consol.Creditor, null);
			AssertEquals("Precondition", consol.CreditorIsNVOCC, false);
			AssertEquals("Precondition", consol.CreditorIsCW1User, true);
			AssertEquals("Precondition", consol.CreditorHasBookingRequestIntegration, true);

			AssertElectronicMenuItems_BookingRequest(false, shipment);
		}

		public void TestElectronicMenuItems_BookingRequest_HasConsol_ConsolCreditorIsNotCW1User_Disabled()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AgentType = "CLD";

			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c1bb";
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_CarrierName = "testship";
			shippingLine.RSL_IsNVO = true;
			shippingLine.RSL_IsCW1User = false;
			shippingLine.RSL_BookingRequestAvailable = true;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_Code = "ABC";
			creditor.OH_RSL_ShippingLine = shippingLine.PK;

			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			AssertNotEquals("Precondition:", consol.Creditor, null);
			AssertEquals("Precondition", consol.CreditorIsNVOCC, true);
			AssertEquals("Precondition", consol.CreditorIsCW1User, false);
			AssertEquals("Precondition", consol.CreditorHasBookingRequestIntegration, true);

			AssertElectronicMenuItems_BookingRequest(false, shipment);
		}

		[RequiresSTA]
		public void TestElectronicMenuItems_BookingRequest_HasConsol_ConsolCreditorDoesNotHaveAvailableIntegration_Disabled()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AgentType = "CLD";

			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "c1bb";
			shippingLine.RSL_StandardCarrierAlphaCode = "1234";
			shippingLine.RSL_CarrierName = "testship";
			shippingLine.RSL_IsNVO = true;
			shippingLine.RSL_IsCW1User = true;
			shippingLine.RSL_BookingRequestAvailable = false;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_Code = "ABC";
			creditor.OH_RSL_ShippingLine = shippingLine.PK;

			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			AssertNotEquals("Precondition:", consol.Creditor, null);
			AssertEquals("Precondition", consol.CreditorIsNVOCC, true);
			AssertEquals("Precondition", consol.CreditorIsCW1User, true);
			AssertEquals("Precondition", consol.CreditorHasBookingRequestIntegration, false);

			AssertElectronicMenuItems_BookingRequest(false, shipment);
		}

		void AssertElectronicMenuItems_BookingRequest(bool containBookingRequest, ForwardingShipment shipment)
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.Cast<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Electronic Messaging");
				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				if (containBookingRequest)
				{
					AssertContains(@"Booking Request", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
				else
				{
					AssertNotContains(@"Booking Request", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestElectronicMenuItems_DraftBill_Enabled()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.Cast<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Electronic Messaging");
				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				var messagesToBookingPartyMenuItem = GetChildMenuItemByTextSingleOrDefault(electronicMessagingMenuItem, "Booking Party");

				AssertNotNull("Booking Party menu item exists", messagesToBookingPartyMenuItem);
				AssertContains(@"Draft Bill of Lading", messagesToBookingPartyMenuItem.GetVisibleMenuItemsCaptions());
			}
		}

		public void TestElectronicMenuItems_CargoReceiptAdvice_Enabled()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.Logs.AddNew(Events.StatusUpdated, $"|{Params.Type}={Constants.EventReferenceMessageTypes.ShipmentStatus}", new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.ElectronicBooking));

			var messageReference = Factory.New<CusEntryNumber>();
			messageReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			messageReference.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			messageReference.CE_EntryType = CustomsReferenceNumberType.eHubInterchangeReference.HIR;
			messageReference.CE_EntryNum = "SHP001";
			shipment.Numbers.Add(messageReference);

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			Assert(shipment.IsElectronicBookingReceived);

			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.Cast<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Electronic Messaging");
				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				var messagesToBookingPartyMenuItem = GetChildMenuItemByTextSingleOrDefault(electronicMessagingMenuItem, "Booking Party");

				AssertNotNull("Booking Party menu item exists", messagesToBookingPartyMenuItem);
				AssertContains("Cargo Receipt Advice", messagesToBookingPartyMenuItem.GetVisibleMenuItemsCaptions());
			}
		}

		public void TestElectronicMenuItems_CargoReceiptAdvice_Disabled()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.Cast<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Electronic Messaging");
				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				AssertNotContains("Cargo Receipt Advice", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
			}
		}

		[RequiresSTA]
		public void TestElectronicMenuItems_CINExportNotification()
		{
			var number1 = Factory.New<CusEntryNumber>();
			number1.CE_RN_NKCountryCode = Constants.CountryCodes.France;
			number1.CE_EntryType = "MRN";
			number1.CE_EntryNum = "MRN01";

			var number2 = Factory.New<CusEntryNumber>();
			number2.CE_RN_NKCountryCode = Constants.CountryCodes.France;
			number2.CE_EntryType = "COC";
			number2.CE_EntryNum = "N01";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "FRPAR";
			shipment.JS_RL_NKDestination = "FRSXB";
			shipment.Numbers.Add(number1);
			shipment.Numbers.Add(number2);

			var cfs = Factory.New<OrgHeader>();
			cfs.OH_FullName = "CONSPA";
			cfs.OH_RL_NKClosestPort = "FRPAR";
			cfs.MainAddress.Address1 = "Unit 15";
			cfs.MainAddress.Address2 = "5 Lost Lane";
			cfs.MainAddress.City = "Marseille";
			cfs.MainAddress.Postcode = "2000";
			cfs.MainAddress.OA_RN_NKCountryCode = "FR";
			cfs.MainAddress.OA_RL_NKRelatedPortCode = "FRPAR";

			var orgCusCode = cfs.CustomsCodes.AddNew();
			orgCusCode.OK_OH = cfs.PK;
			orgCusCode.OK_CodeType = "CTR";
			orgCusCode.SecuredCustomsRegNo = "N01";
			orgCusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.France;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol.JK_MasterBillNum = "MAWB01";
			consol.JK_RL_NKLoadPort = "FRPAR";
			consol.JK_RL_NKDischargePort = "FRSXB";
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (PortMessagingRegistry.Instance.AllowToSendExportNotification.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.OfType<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Electronic Messaging");

				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);
				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				AssertContains(@"Port Messaging
      Export
         CIN Export Notification 755 (FR)", electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
			}
		}

		public void TestMenuItem_JAEPACertificateOfOrigin()
		{
			using (DocumentsDataRegistry.Instance.EnableJAEPASubmissionToCAB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = Factory.New<ForwardingShipment>();

				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					shipment.JS_RL_NKOrigin = "AUSYD";
					shipment.JS_RL_NKDestination = "JPSBU";
					Factory.Save();

					var menuItem = FindMenuItem(form, "&Documents", "Trade Documents", "Certificate Of Origin", "JAEPA");
					AssertNotNull("Precondition: JAEPA menu item exists", menuItem);

					shipment.JS_RL_NKOrigin = "JPSBU";
					shipment.JS_RL_NKDestination = "AUSYD";
					Factory.Save();

					menuItem = FindMenuItem(form, "&Documents", "Trade Documents", "Certificate Of Origin", "JAEPA");
					AssertNotNull("Precondition: JAEPA menu item exists", menuItem);

					shipment.JS_RL_NKOrigin = "JPSBU";
					shipment.JS_RL_NKDestination = "JPSBU";
					Factory.Save();

					menuItem = FindMenuItem(form, "&Documents", "Trade Documents", "Certificate Of Origin", "JAEPA");
					AssertNull(menuItem);
				}
			}
		}

		public void TestMenuItem_ChAFTACertificateOfOrigin()
		{
			using (DocumentsDataRegistry.Instance.EnableChAFTASubmissionToCAB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = Factory.New<ForwardingShipment>();

				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					shipment.JS_RL_NKOrigin = "AUSYD";
					shipment.JS_RL_NKDestination = "CNNKG";
					Factory.Save();

					var menuItem = FindMenuItem(form, "&Documents", "Trade Documents", "Certificate Of Origin", "ChAFTA");
					AssertNotNull("Precondition: ChAFTA menu item exists", menuItem);
				}
			}
		}

		[RequiresSTA]
		public void TestMenuItem_IACEPACertificateOfOrigin()
		{
			using (DocumentsDataRegistry.Instance.EnableIACEPASubmissionToCAB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = Factory.New<ForwardingShipment>();

				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					shipment.JS_RL_NKOrigin = "AUSYD";
					shipment.JS_RL_NKDestination = "IDDPS";
					Factory.Save();

					var menuItem = FindMenuItem(form, "&Documents", "Trade Documents", "Certificate Of Origin", "IA-CEPA");
					AssertNotNull("Precondition: IA-CEPA menu item exists", menuItem);
				}
			}
		}

		public void TestOpenOrgForUnmatchedControllingCustomer_ShipmentDetailsTabPage()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var unmatchedOrgAddr = InitUnmatchedAddress();

			SetUnmatchedAddressAndStmNote(shipment, unmatchedOrgAddr, DocAddressTypes.Codes.ControllingCustomer, OrganisationTypes.ControllingCustomer);
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				var controllingCustomerAddress = form.Controls.Find("ControllingCustomerAddress", true)[0] as ZDocAddressControl;
				var controllingCustomerControl = controllingCustomerAddress.Controls.Find("CutDownSingleLineNoGroupBoxOrgFindBox", true)[0];

				var controllingCustomerOrgFindBox = (controllingCustomerControl as ZOrganisationFindBox);
				controllingCustomerOrgFindBox.List = null;
				var controllingCustomerFindBox = controllingCustomerControl as IFindBox;
				AssertNotNull(controllingCustomerFindBox);

				var newPulledList = controllingCustomerOrgFindBox.List;
				AssertNotNull(newPulledList);
				controllingCustomerFindBox.ListProvider.GetBusinessObjectsFromCodeWithoutFilter(OrgHeader.UnmatchedOrganisationCode);
				AssertFindResult(newPulledList as IOrganisationDefaultProvider);
			}
		}

		[RequiresSTA]
		public void TestOpenOrgForUnmatchedControllingCustomer_AddressesTabPage()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var unmatchedOrgAddr = InitUnmatchedAddress();

			SetUnmatchedAddressAndStmNote(shipment, unmatchedOrgAddr, DocAddressTypes.Codes.ControllingCustomer, OrganisationTypes.ControllingCustomer);
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				var mainTabControl = form.Controls.Find("MainTabControl", true)[0] as ZTemplateTabControl;
				var addressesTabPage = form.Controls.Find("AddressesTabPage", true)[0] as ZTabPage;
				mainTabControl.SelectedTab = addressesTabPage;

				var controllingCustomerAddress = addressesTabPage.Controls.Find("AddressControl", true)[0] as ZDocAddressControl;
				var controllingCustomerControl = controllingCustomerAddress.Controls.Find("CutDownSingleLineOrgFindBox", true)[0];

				var controllingCustomerOrgFindBox = (controllingCustomerControl as ZOrganisationFindBox);
				var newPulledList = controllingCustomerOrgFindBox.List = null;
				var controllingCustomerFindBox = controllingCustomerControl as IFindBox;
				AssertNotNull(controllingCustomerFindBox);

				newPulledList = controllingCustomerOrgFindBox.List;
				AssertNotNull(newPulledList);
				controllingCustomerFindBox.ListProvider.GetBusinessObjectsFromCodeWithoutFilter(OrgHeader.UnmatchedOrganisationCode);
				AssertFindResult(newPulledList as IOrganisationDefaultProvider);
			}
		}

		[RequiresSTA]
		public void TestOpenOrgForUnmatchedControllingAgent_ShipmentDetailsTabPage()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var unmatchedOrgAddr = InitUnmatchedAddress();

			SetUnmatchedAddressAndStmNote(shipment, unmatchedOrgAddr, DocAddressTypes.Codes.ControllingAgent, OrganisationTypes.ControllingAgent);
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				var controllingAgentAddress = form.Controls.Find("ControllingAgentAddress", true)[0] as ZDocAddressControl;
				var controllingAgentControl = controllingAgentAddress.Controls.Find("CutDownSingleLineNoGroupBoxOrgFindBox", true)[0];

				var controllingAgentOrgFindBox = (controllingAgentControl as ZOrganisationFindBox);
				controllingAgentOrgFindBox.List = null;
				var controllingAgentFindBox = controllingAgentControl as IFindBox;
				AssertNotNull(controllingAgentFindBox);

				var newPulledList = controllingAgentOrgFindBox.List;
				AssertNotNull(newPulledList);
				controllingAgentFindBox.ListProvider.GetBusinessObjectsFromCodeWithoutFilter(OrgHeader.UnmatchedOrganisationCode);
				AssertFindResult(newPulledList as IOrganisationDefaultProvider);
			}
		}

		public void TestOpenOrgForUnmatchedControllingAgent_AddressesTabPage()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var unmatchedOrgAddr = InitUnmatchedAddress();

			SetUnmatchedAddressAndStmNote(shipment, unmatchedOrgAddr, DocAddressTypes.Codes.ControllingAgent, OrganisationTypes.ControllingAgent);
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				var mainTabControl = form.Controls.Find("MainTabControl", true)[0] as ZTemplateTabControl;
				var addressesTabPage = form.Controls.Find("AddressesTabPage", true)[0] as ZTabPage;
				mainTabControl.SelectedTab = addressesTabPage;
				var controllingAgentAddress = addressesTabPage.Controls.Find("AddressControl", true)[0] as ZDocAddressControl;
				var controllingAgentControl = controllingAgentAddress.Controls.Find("CutDownSingleLineOrgFindBox", true)[0];

				var controllingAgentOrgFindBox = (controllingAgentControl as ZOrganisationFindBox);
				var newPulledList = controllingAgentOrgFindBox.List = null;
				var controllingAgentFindBox = controllingAgentControl as IFindBox;
				AssertNotNull(controllingAgentFindBox);

				newPulledList = controllingAgentOrgFindBox.List;
				AssertNotNull(newPulledList);
				controllingAgentFindBox.ListProvider.GetBusinessObjectsFromCodeWithoutFilter(OrgHeader.UnmatchedOrganisationCode);
				AssertFindResult(newPulledList as IOrganisationDefaultProvider);
			}
		}

		BusinessObject InitUnmatchedAddress()
		{
			var unmatchedOrgPK = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation;
			var unmatchedOrgAddr = Factory.LoadTop1(typeof(OrgAddress), new ZQuery(OrgAddressSchema.OA_OH, unmatchedOrgPK));
			AssertNotNull(unmatchedOrgAddr);
			return unmatchedOrgAddr;
		}

		void SetUnmatchedAddressAndStmNote(ForwardingShipment shipment, BusinessObject unmatchedOrgAddr, string docAddressTypes, OrganisationTypes organisationType)
		{
			var unmatchedAddress = Factory.NewWithValidTestData<JobDocAddress>();
			unmatchedAddress.E2_OA_Address = unmatchedOrgAddr.PK;
			unmatchedAddress.E2_AddressType = docAddressTypes;
			unmatchedAddress.E2_ParentID = shipment.PK;
			unmatchedAddress.E2_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var stmNote = Factory.NewWithValidTestData<StmNote>();
			stmNote.ST_ParentID = shipment.PK;
			stmNote.ST_Description = PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Code;
			stmNote.ST_Table = JobShipmentSchema.Constants.TableName;
			stmNote.ST_NoteText = $@"<UnmatchOrgRecords><UnmatchOrgRecord><OrganisationType>{organisationType}</OrganisationType><OrganisationSubType>{organisationType}</OrganisationSubType><OwnerCode /><EDICode>CONSALVADALV</EDICode>
<OrganisationName>CONSIGNOR</OrganisationName><AddressLine1>TIAN</AddressLine1><AddressLine2 /><City>NANJING</City><PostCode>21000</PostCode><StateOrProvince>32</StateOrProvince><Country>CN</Country><DocAddressType /></UnmatchOrgRecord></UnmatchOrgRecords>";
			Factory.Save();
			shipment.DocAddresses.Load();
		}

		void AssertFindResult(IOrganisationDefaultProvider newPulledList)
		{
			AssertEquals("ShouldSetValuesFromConditionalDefaults is set to true", true, newPulledList.ShouldSetValuesFromConditionalDefaults);
			var orgFromUnmatched = Factory.New<OrgHeader>();
			((OrganisationsFindBoxCollection)newPulledList).SetupNewElementButDoNotAddIt(orgFromUnmatched, true);
			var mainAddress = orgFromUnmatched.MainAddress;
			AssertEquals("TIAN", mainAddress.Address1);
			AssertEquals("NANJING", mainAddress.City);
			AssertEquals("CN", mainAddress.Country.Code);
			AssertEquals("21000", mainAddress.Postcode);
			AssertEquals("32", mainAddress.OA_State);
		}

		#region Transit Warehouse

		[TestDate(2020, 11, 3, 1, 2, 3)]
		public void TestSendTransitWarehousePickupReceiptInstruction()
		{
			TestTransitWarehouseInstruction(true, TransitWarehouseInstructionHelper.ServiceRequest.Receipt);
		}

		[TestDate(2020, 11, 3, 1, 2, 3)]
		[RequiresSTA]
		public void TestSendTransitWarehousePickupDispatchInstruction()
		{
			TestTransitWarehouseInstruction(true, TransitWarehouseInstructionHelper.ServiceRequest.Dispatch);
		}

		[TestDate(2020, 11, 3, 1, 2, 3)]
		public void TestSendTransitWarehousePickupReceiptAndDispatchInstruction()
		{
			TestTransitWarehouseInstruction(true, TransitWarehouseInstructionHelper.ServiceRequest.ReceiveAndDispatch);
		}

		[TestDate(2020, 11, 3, 1, 2, 3)]
		public void TestSendTransitWarehouseDeliveryReceiptInstruction()
		{
			TestTransitWarehouseInstruction(false, TransitWarehouseInstructionHelper.ServiceRequest.Receipt);
		}

		[TestDate(2020, 11, 3, 1, 2, 3)]
		public void TestSendTransitWarehouseDeliveryDispatchInstruction()
		{
			TestTransitWarehouseInstruction(false, TransitWarehouseInstructionHelper.ServiceRequest.Dispatch);
		}

		[TestDate(2020, 11, 3, 1, 2, 3)]
		[RequiresSTA]
		public void TestSendTransitWarehouseDeliveryReceiptAndDispatchInstruction()
		{
			TestTransitWarehouseInstruction(false, TransitWarehouseInstructionHelper.ServiceRequest.ReceiveAndDispatch);
		}

		ZPropertyInfo[] GetTransitWarehouseInstructionRequestedDateInfos(ForwardingShipment shipment, bool testPickup, TransitWarehouseInstructionHelper.ServiceRequest receiptDispatch)
		{
			if (testPickup)
			{
				if (receiptDispatch == TransitWarehouseInstructionHelper.ServiceRequest.Receipt)
				{
					return new ZPropertyInfo[] { shipment.JS_ExportReceivingDepotReceiptRequestedInfo };
				}
				else if (receiptDispatch == TransitWarehouseInstructionHelper.ServiceRequest.Dispatch)
				{
					return new ZPropertyInfo[] { shipment.JS_ExportReceivingDepotDispatchRequestedInfo };
				}
				else
				{
					return new ZPropertyInfo[] { shipment.JS_ExportReceivingDepotReceiptRequestedInfo, shipment.JS_ExportReceivingDepotDispatchRequestedInfo };
				}
			}
			else
			{
				if (receiptDispatch == TransitWarehouseInstructionHelper.ServiceRequest.Receipt)
				{
					return new ZPropertyInfo[] { shipment.JS_ImportReleaseDepotReceiptRequestedInfo };
				}
				else if (receiptDispatch == TransitWarehouseInstructionHelper.ServiceRequest.Dispatch)
				{
					return new ZPropertyInfo[] { shipment.JS_ImportReleaseDepotDispatchRequestedInfo };
				}
				else
				{
					return new ZPropertyInfo[] { shipment.JS_ImportReleaseDepotReceiptRequestedInfo, shipment.JS_ImportReleaseDepotDispatchRequestedInfo };
				}
			}
		}

		void TestTransitWarehouseInstruction(bool testPickup, TransitWarehouseInstructionHelper.ServiceRequest receiptDispatch)
		{
			var pickupOrDelivery = testPickup ? "Pickup" : "Delivery";
			var receiptOrDispatch = "Receipt and Dispatch";
			if (receiptDispatch != TransitWarehouseInstructionHelper.ServiceRequest.ReceiveAndDispatch)
			{
				receiptOrDispatch = receiptDispatch == TransitWarehouseInstructionHelper.ServiceRequest.Receipt ? "Receipt" : "Dispatch";
			}

			var depot = Factory.LoadTop1<OrgAddress>(new ZQuery());

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001000";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_TransportMode = "AIR";

			var transitUniversalServiceMock = new Mock<ITransitUniversalService>();
			transitUniversalServiceMock.Setup(m => m.GetNotificationForInstruction(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new WarningNotification("Instruction has been queued to send. Check DEX logs for details."));

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			using (ObjectFactory.Substitute(nameof(ITransitUniversalService), _ => transitUniversalServiceMock.Object))
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				var transitWarehouseMenu = FindActionMenuItem(shipmentForm).MenuItems.FindByText("Transit Warehouse");
				var pickupOrDeliveryMenu = transitWarehouseMenu.MenuItems.FindByText(FormattableString.Invariant($"{pickupOrDelivery} TW"));
				var menu = pickupOrDeliveryMenu.MenuItems.FindByText(FormattableString.Invariant($"Send {receiptOrDispatch} Instruction"));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				menu.PerformClick();
				AssertEquals("Please save your changes before sending the Transit Warehouse Instruction.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				menu.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(FormattableString.Invariant($"The {pickupOrDelivery} CFS / Transit Warehouse must be entered before the {pickupOrDelivery} TW {receiptOrDispatch} Instruction can be sent."), UnitTestUserNotification.Instance.LastMessage.Text);

				if (testPickup)
				{
					shipment.JS_OA_ExportReceivingDepot = depot.PK;
				}
				else
				{
					shipment.JS_OA_ImportReleaseDepot = depot.PK;
				}

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				menu.PerformClick();
				AssertStartsWith("Should have EDI comms failure message", "No EDI Communications settings were found", UnitTestUserNotification.Instance.LastMessage.Text);

				var logs = shipment.Logs.GetAllLogs();
				Assert("No DEX event", !logs.Cast<StmALog>().Any(l => l.SL_SE_NKEvent == "DEX"));
				Assert("No SVR event", !logs.Cast<StmALog>().Any(l => l.SL_SE_NKEvent == "SVR"));

				var dateInfos = GetTransitWarehouseInstructionRequestedDateInfos(shipment, testPickup, receiptDispatch);
				foreach (var dateInfo in dateInfos)
				{
					AssertEquals("Date has not been updated", ZDateTime.Empty, (ZDateTime)dateInfo.Value);
				}

				var communicationMode = depot.Header.EDICommunicationsModes.AddNew();
				communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationMode.EK_Destination = "Blah";
				communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationMode.EK_Module = "SHP";

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				menu.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);

				if (receiptDispatch != TransitWarehouseInstructionHelper.ServiceRequest.ReceiveAndDispatch)
				{
					AssertEquals(@"The Transit Warehouse Instruction has been sent.
Processing Shipment S00001000
Universal Shipment queued for sending to Organization [MIDINT].

Warning: Instruction has been queued to send. Check DEX logs for details.", UnitTestUserNotification.Instance.LastMessage.Text);

					var newFactory = Factory.CreateNewFactory();
					shipment = newFactory.Load<ForwardingShipment>(shipment.PK);
					logs = shipment.Logs.GetAllLogs();
					var svrEvent = logs.Cast<StmALog>().Single(l => l.SL_SE_NKEvent == "SVR");
					AssertNotNull("SVR event created", svrEvent);

					var expectedReference = FormattableString.Invariant($"|FAC=CFS|LOC=AUBNE|TYP={receiptOrDispatch}");
					AssertEquals("SVR event reference", expectedReference, svrEvent.SL_Reference);

					Assert("DEX event created", logs.Cast<StmALog>().Any(l => l.SL_SE_NKEvent == "DEX"));
				}
				else
				{
					AssertEquals(@"The Transit Warehouse Instruction has been sent.
Processing Shipment S00001000
Universal Shipment queued for sending to Organization [MIDINT].

Warning: Instruction has been queued to send. Check DEX logs for details.
Processing Shipment S00001000
Universal Shipment queued for sending to Organization [MIDINT].

Warning: Instruction has been queued to send. Check DEX logs for details.", UnitTestUserNotification.Instance.LastMessage.Text);

					var newFactory = Factory.CreateNewFactory();
					shipment = newFactory.Load<ForwardingShipment>(shipment.PK);
					logs = shipment.Logs.GetAllLogs();
					var svrEvent = logs.Cast<StmALog>().First(l => l.SL_SE_NKEvent == "SVR");
					AssertNotNull("SVR event created", svrEvent);

					var expectedReference = FormattableString.Invariant($"|FAC=CFS|LOC=AUBNE|TYP=Receipt");
					AssertEquals("SVR event reference", expectedReference, svrEvent.SL_Reference);

					svrEvent = logs.Cast<StmALog>().Last(l => l.SL_SE_NKEvent == "SVR");
					AssertNotNull("SVR event created", svrEvent);

					expectedReference = FormattableString.Invariant($"|FAC=CFS|LOC=AUBNE|TYP=Dispatch");
					AssertEquals("SVR event reference", expectedReference, svrEvent.SL_Reference);

					Assert("DEX event created", logs.Cast<StmALog>().Any(l => l.SL_SE_NKEvent == "DEX"));
				}

				dateInfos = GetTransitWarehouseInstructionRequestedDateInfos(shipment, testPickup, receiptDispatch);
				foreach (var dateInfo in dateInfos)
				{
					AssertNotEquals("Date has been updated", ZDateTime.Empty, (ZDateTime)dateInfo.Value);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
			transitUniversalServiceMock.VerifyAll();
		}

		public void TestTransitWarehouseActionMenuLooksCleaner()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001000";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_TransportMode = "AIR";

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				var transitWarehouseMenu = FindActionMenuItem(shipmentForm).MenuItems.FindByText("Transit Warehouse");
				var pickupTWMenu = transitWarehouseMenu.MenuItems.FindByText("Pickup TW");
				var deliveryTWMenu = transitWarehouseMenu.MenuItems.FindByText("Delivery TW");

				AssertEquals("There should be 5 menu items within the Transit Warehouse menu", 5, transitWarehouseMenu.MenuItems.Count);
				AssertEquals("Attach Warehouse Packages", transitWarehouseMenu.MenuItems[0].Text);
				AssertEquals("Detach Warehouse Packages", transitWarehouseMenu.MenuItems[1].Text);
				AssertEquals("View Shipment Packages", transitWarehouseMenu.MenuItems[2].Text);
				AssertEquals("Pickup TW", transitWarehouseMenu.MenuItems[3].Text);
				AssertEquals("Delivery TW", transitWarehouseMenu.MenuItems[4].Text);

				AssertEquals("There should be 7 items within the Pickup TW menu", 7, pickupTWMenu.MenuItems.Count);
				AssertEquals("Send Receipt Instruction", pickupTWMenu.MenuItems[0].Text);
				AssertEquals("Send Dispatch Instruction", pickupTWMenu.MenuItems[1].Text);
				AssertEquals("Send Receipt and Dispatch Instruction", pickupTWMenu.MenuItems[2].Text);
				AssertEquals("Send Prepare to Dispatch Instruction", pickupTWMenu.MenuItems[3].Text);
				AssertEquals("-", pickupTWMenu.MenuItems[4].Text);
				AssertEquals("Receipt Instruction Planning Portal", pickupTWMenu.MenuItems[5].Text);
				AssertEquals("Dispatch Instruction Planning Portal", pickupTWMenu.MenuItems[6].Text);

				AssertEquals("There should be 7 items within the Delivery TW menu", 7, deliveryTWMenu.MenuItems.Count);
				AssertEquals("Send Receipt Instruction", deliveryTWMenu.MenuItems[0].Text);
				AssertEquals("Send Dispatch Instruction", deliveryTWMenu.MenuItems[1].Text);
				AssertEquals("Send Receipt and Dispatch Instruction", deliveryTWMenu.MenuItems[2].Text);
				AssertEquals("Send Prepare to Dispatch Instruction", deliveryTWMenu.MenuItems[3].Text);
				AssertEquals("-", deliveryTWMenu.MenuItems[4].Text);
				AssertEquals("Receipt Instruction Planning Portal", deliveryTWMenu.MenuItems[5].Text);
				AssertEquals("Dispatch Instruction Planning Portal", deliveryTWMenu.MenuItems[6].Text);
			}
		}

		#endregion

		#region Controlling Agent authorize request during saving

		public void TestShipmentFormSavedAndDoesNotShowAuthorizeRequestWhenControllingAgentIsNotEmpty()
		{
			var controllingAgent = Factory.NewWithValidTestData<OrgHeader>();
			controllingAgent.OH_Code = "CAG";
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = controllingAgent.PK;
			orgAddress.OA_Code = "CAG";

			var shipment = GetShipmentWithConsignorAndConsignee(Constants.TransportModes.Courier);
			shipment.ControllingAgentDocumentaryAddress.E2_OA_Address = orgAddress.PK;

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = false;

			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				var result = shipmentForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, shipment.IsInDatabase);
		}

		public void TestShipmentNotSavedWhenControllingAgentIsEmptyAndAuthorizeRequestCancelled()
		{
			var shipment = GetShipmentWithConsignorAndConsignee(Constants.TransportModes.Courier);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = false;

			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();

				int loginFormShownCount = 0;
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					}
				});

				var result = shipmentForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
			}

			AssertEquals(false, shipment.IsInDatabase);
		}

		public void TestShipmentNotSavedWhenControllingAgentIsEmptyAndUserEnteredToAthorizationFormDoesNotExists()
		{
			var shipment = GetShipmentWithConsignorAndConsignee(Constants.TransportModes.Courier);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = false;

			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();

				int loginFormShownCount = 0;
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						loginForm.DoLoginForTest("aaa", "pass");
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});

				var result = shipmentForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
				AssertEquals("User does not exist, password is invalid or password is expired.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(false, shipment.IsInDatabase);
		}

		public void TestShipmentNotSavedWhenControllingAgentIsEmptyAndSupervisorDoesNotHaveSecurityRightsToOverrideSecurityPolicy()
		{
			var shipment = GetShipmentWithConsignorAndConsignee(Constants.TransportModes.Courier);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = false;

			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();

				int loginFormShownCount = 0;
				var newUserLogin = "User1";
				var newUserPassword = "pass";

				SecurityTestObject.CreateTestUser(false, Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.Code, "US1", newUserLogin, newUserPassword);
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						loginForm.DoLoginForTest(newUserLogin, newUserPassword);
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});

				var result = shipmentForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
				AssertEquals("User does not have security rights to override this security policy.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(false, shipment.IsInDatabase);
		}

		[RequiresSTA]
		public void TestShipmentFormSavedAndDoesNotShowAuthorizeRequestWhenControllingAgentIsEmptyAndUserHasPermissions()
		{
			AssertShipmentFormSavedAndDoesNotShowAuthorizeRequestWhenControllingAgentIsEmptyAndUserHasPermissions(Constants.TransportModes.Air, Env.Security.MaintainShipmentAllowSaveWithoutControllingAgentAir);
			AssertShipmentFormSavedAndDoesNotShowAuthorizeRequestWhenControllingAgentIsEmptyAndUserHasPermissions(Constants.TransportModes.Sea, Env.Security.MaintainShipmentAllowSaveWithoutControllingAgentSea);
			AssertShipmentFormSavedAndDoesNotShowAuthorizeRequestWhenControllingAgentIsEmptyAndUserHasPermissions(Constants.TransportModes.Road, Env.Security.MaintainShipmentAllowSaveWithoutControllingAgentRoad);
			AssertShipmentFormSavedAndDoesNotShowAuthorizeRequestWhenControllingAgentIsEmptyAndUserHasPermissions(Constants.TransportModes.Rail, Env.Security.MaintainShipmentAllowSaveWithoutControllingAgentRail);
			AssertShipmentFormSavedAndDoesNotShowAuthorizeRequestWhenControllingAgentIsEmptyAndUserHasPermissions(Constants.TransportModes.Courier, Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent);
		}

		public void TestShipmentSavedWhenControllingAgentIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm()
		{
			AssertShipmentSavedWhenControllingAgentIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(Constants.TransportModes.Air, Env.Security.MaintainShipmentAllowSaveWithoutControllingAgentAir);
			AssertShipmentSavedWhenControllingAgentIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(Constants.TransportModes.Sea, Env.Security.MaintainShipmentAllowSaveWithoutControllingAgentSea);
			AssertShipmentSavedWhenControllingAgentIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(Constants.TransportModes.Road, Env.Security.MaintainShipmentAllowSaveWithoutControllingAgentRoad);
			AssertShipmentSavedWhenControllingAgentIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(Constants.TransportModes.Rail, Env.Security.MaintainShipmentAllowSaveWithoutControllingAgentRail);
			AssertShipmentSavedWhenControllingAgentIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(Constants.TransportModes.Courier, Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent);
		}

		void AssertShipmentFormSavedAndDoesNotShowAuthorizeRequestWhenControllingAgentIsEmptyAndUserHasPermissions(string transportMode, SecurityCheckpoint securityCheckpoint)
		{
			var shipment = GetShipmentWithConsignorAndConsignee(transportMode);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = false;
			securityCheckpoint.IsAllowed = true;

			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				var result = shipmentForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, shipment.IsInDatabase);
		}

		void AssertShipmentSavedWhenControllingAgentIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(string transportMode, SecurityCheckpoint securityCheckpoint)
		{
			var shipment = GetShipmentWithConsignorAndConsignee(transportMode);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = false;
			securityCheckpoint.IsAllowed = false;

			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				int loginFormShownCount = 0;

				var newUserLogin = "User1" + transportMode;
				var newUserPassword = "pass";

				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;
				SecurityTestObject.CreateTestUser(true, securityCheckpoint.Code, transportMode, newUserLogin, newUserPassword);

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						loginForm.DoLoginForTest(newUserLogin, newUserPassword);
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});

				var result = shipmentForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
				AssertEquals(1, loginFormShownCount);
			}

			AssertEquals(true, shipment.IsInDatabase);
		}

		public void TestShipmentFormSavedAndNoAuthorizeRequestPriorToControllingAgentEffectiveDate()
		{
			AssertShipmentFormSavedAndNoAuthorizeRequestPriorToControllingAgentEffectiveDate(Constants.TransportModes.Sea);
			AssertShipmentFormSavedAndNoAuthorizeRequestPriorToControllingAgentEffectiveDate(Constants.TransportModes.Air);
			AssertShipmentFormSavedAndNoAuthorizeRequestPriorToControllingAgentEffectiveDate(Constants.TransportModes.Road);
			AssertShipmentFormSavedAndNoAuthorizeRequestPriorToControllingAgentEffectiveDate(Constants.TransportModes.Rail);
			AssertShipmentFormSavedAndNoAuthorizeRequestPriorToControllingAgentEffectiveDate(Constants.TransportModes.Courier);
		}

		[RequiresSTA]
		public void TestShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingAgentEffectiveDate()
		{
			AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingAgentEffectiveDate(Constants.TransportModes.Sea, ZDateTime.UtcToday.AddDays(-2).ToDateTime());
			AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingAgentEffectiveDate(Constants.TransportModes.Air, ZDateTime.UtcToday.AddDays(-2).ToDateTime());
			AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingAgentEffectiveDate(Constants.TransportModes.Road, ZDateTime.UtcToday.AddDays(-2).ToDateTime());
			AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingAgentEffectiveDate(Constants.TransportModes.Rail, ZDateTime.UtcToday.AddDays(-2).ToDateTime());
			AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingAgentEffectiveDate(Constants.TransportModes.Courier, ZDateTime.UtcToday.AddDays(-2).ToDateTime());

			AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingAgentEffectiveDate(Constants.TransportModes.Sea, ZDateTime.UtcToday.ToDateTime());
			AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingAgentEffectiveDate(Constants.TransportModes.Air, ZDateTime.UtcToday.ToDateTime());
			AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingAgentEffectiveDate(Constants.TransportModes.Road, ZDateTime.UtcToday.ToDateTime());
			AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingAgentEffectiveDate(Constants.TransportModes.Rail, ZDateTime.UtcToday.ToDateTime());
			AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingAgentEffectiveDate(Constants.TransportModes.Courier, ZDateTime.UtcToday.ToDateTime());
		}

		public void TestControllingAgentRegistryFallBack()
		{
			AssertControllingAgentRegistryFallBack(Constants.TransportModes.Sea);
			AssertControllingAgentRegistryFallBack(Constants.TransportModes.Air);
			AssertControllingAgentRegistryFallBack(Constants.TransportModes.Road);
			AssertControllingAgentRegistryFallBack(Constants.TransportModes.Rail);
		}

		[RequiresSTA]
		public void TestSavedShipmentWillShowLoginFormIfCreatedAfterControllingAgentEffectieveDate()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_IsConsignee = true;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			Factory.Save();
			AssertEquals(true, shipment.IsInDatabase);

			shipment.GetControllingAgentSecurityCheckPoint().IsAllowed = false;
			shipment.GetControllingCustomerSecurityCheckPoint().IsAllowed = true;

			var registry = FreightRegistry.Instance.MandatoryControllingAgentEffectiveDateSea;

			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.AddDays(-3).ToDateTime()))
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();

				int loginFormShownCount = 0;
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					}
				});

				AssertEquals(ContinueWithSave.No, shipmentForm.FireSaveButton());
				AssertEquals(1, loginFormShownCount);
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
			}
		}

		[RequiresSTA]
		public void TestSavedShipmentWillNotShowLoginFormIfCreatedBeforeControllingAgentEffectieveDate()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_IsConsignee = true;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			Factory.Save();

			shipment.JS_SystemCreateTimeUtc = ZDateTime.UtcToday.AddDays(-2).ToDateTime();

			AssertEquals(true, shipment.IsInDatabase);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = false;
			shipment.GetControllingCustomerSecurityCheckPoint().IsAllowed = true;

			var registry = FreightRegistry.Instance.MandatoryControllingAgentEffectiveDateSea;

			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.ToDateTime()))
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				shipment.JS_ActualWeight = 22;

				var result = shipmentForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}
		}

		public void AssertControllingAgentRegistryFallBack(string transportMode)
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR" + transportMode;
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE" + transportMode;
			consignee.OH_IsConsignee = true;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			shipment.JS_TransportMode = transportMode;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = false;
			shipment.GetControllingCustomerSecurityCheckPoint().IsAllowed = true;

			using (FreightRegistry.Instance.MandatoryControllingAgentEffectiveDateAll.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.AddDays(3).ToDateTime()))
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				var result = shipmentForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, shipment.IsInDatabase);
		}

		void AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingAgentEffectiveDate(string transportMode, DateTime effectiveDate)
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR" + transportMode;
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE" + transportMode;
			consignee.OH_IsConsignee = true;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			shipment.JS_TransportMode = transportMode;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			shipment.GetControllingAgentSecurityCheckPoint().IsAllowed = false;
			shipment.GetControllingCustomerSecurityCheckPoint().IsAllowed = true;

			var registry = shipment.GetMandatoryControllingAgentEffectiveDateRegistry();

			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, effectiveDate))
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();

				int loginFormShownCount = 0;
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					}
				});

				var result = shipmentForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
			}

			AssertEquals(false, shipment.IsInDatabase);
		}

		void AssertShipmentFormSavedAndNoAuthorizeRequestPriorToControllingAgentEffectiveDate(string transportMode)
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR" + transportMode;
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE" + transportMode;
			consignee.OH_IsConsignee = true;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			shipment.JS_TransportMode = transportMode;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = false;
			shipment.GetControllingCustomerSecurityCheckPoint().IsAllowed = true;

			var registry = shipment.GetMandatoryControllingAgentEffectiveDateRegistry();

			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.AddDays(3).ToDateTime()))
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				var result = shipmentForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, shipment.IsInDatabase);
		}

		#endregion

		#region Controlling Customer authorize request during saving

		public void TestShipmentFormSavedAndDoesNotShowAuthorizeRequestWhenTransportModeCourierAndControllingCustomerIsNotEmpty()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CAG";
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = controllingCustomer.PK;
			orgAddress.OA_Code = "CAG";

			var shipment = GetShipmentWithConsignorAndConsignee(Constants.TransportModes.Courier);
			shipment.ControllingCustomerAddress.E2_OA_Address = orgAddress.PK;

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = true;
			Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomer.IsAllowed = false;

			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				var result = shipmentForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, shipment.IsInDatabase);
		}

		public void TestShipmentNotSavedWhenTransportModeCourierAndControllingCustomerIsEmptyAndAuthorizeRequestCancelled()
		{
			var shipment = GetShipmentWithConsignorAndConsignee(Constants.TransportModes.Courier);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = true;
			Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomer.IsAllowed = false;

			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();

				int loginFormShownCount = 0;
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					}
				});

				var result = shipmentForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
			}

			AssertEquals(false, shipment.IsInDatabase);
		}

		public void TestShipmentNotSavedWhenTransportModeCourierAndControllingCustomerIsEmptyAndUserEnteredToAthorizationFormDoesNotExists()
		{
			var shipment = GetShipmentWithConsignorAndConsignee(Constants.TransportModes.Courier);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = true;
			Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomer.IsAllowed = false;

			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();

				int loginFormShownCount = 0;
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						loginForm.DoLoginForTest("aaa", "pass");
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});

				var result = shipmentForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
				AssertEquals("User does not exist, password is invalid or password is expired.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(false, shipment.IsInDatabase);
		}

		public void TestShipmentNotSavedWhenTransportModeCourierAndControllingCustomerIsEmptyAndSupervisorDoesNotHaveSecurityRightsToOverrideSecurityPolicy()
		{
			var shipment = GetShipmentWithConsignorAndConsignee(Constants.TransportModes.Courier);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = true;
			Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomer.IsAllowed = false;

			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();

				int loginFormShownCount = 0;
				var newUserLogin = "User1";
				var newUserPassword = "pass";

				SecurityTestObject.CreateTestUser(false, Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.Code, "US1", newUserLogin, newUserPassword);
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						loginForm.DoLoginForTest(newUserLogin, newUserPassword);
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});

				var result = shipmentForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
				AssertEquals("User does not have security rights to override this security policy.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(false, shipment.IsInDatabase);
		}

		public void TestShipmentFormSavedAndDoesNotShowAuthorizeRequestWhenControllingCustomerIsEmptyAndUserHasPermissions()
		{
			AssertShipmentFormSavedAndDoesNotShowAuthorizeRequestWhenControllingCustomerIsEmptyAndUserHasPermissions(Constants.TransportModes.Air, Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerAir);
			AssertShipmentFormSavedAndDoesNotShowAuthorizeRequestWhenControllingCustomerIsEmptyAndUserHasPermissions(Constants.TransportModes.Sea, Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerSea);
			AssertShipmentFormSavedAndDoesNotShowAuthorizeRequestWhenControllingCustomerIsEmptyAndUserHasPermissions(Constants.TransportModes.Road, Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerRoad);
			AssertShipmentFormSavedAndDoesNotShowAuthorizeRequestWhenControllingCustomerIsEmptyAndUserHasPermissions(Constants.TransportModes.Rail, Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerRail);
			AssertShipmentFormSavedAndDoesNotShowAuthorizeRequestWhenControllingCustomerIsEmptyAndUserHasPermissions(Constants.TransportModes.Courier, Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomer);
		}

		public void TestShipmentSavedWhenControllingCustomerIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm()
		{
			AssertShipmentSavedWhenControllingCustomerIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(Constants.TransportModes.Air, Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerAir);
			AssertShipmentSavedWhenControllingCustomerIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(Constants.TransportModes.Sea, Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerSea);
			AssertShipmentSavedWhenControllingCustomerIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(Constants.TransportModes.Road, Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerRoad);
			AssertShipmentSavedWhenControllingCustomerIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(Constants.TransportModes.Rail, Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerRail);
			AssertShipmentSavedWhenControllingCustomerIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(Constants.TransportModes.Courier, Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomer);
		}

		void AssertShipmentFormSavedAndDoesNotShowAuthorizeRequestWhenControllingCustomerIsEmptyAndUserHasPermissions(string transportMode, SecurityCheckpoint securityCheckpoint)
		{
			var shipment = GetShipmentWithConsignorAndConsignee(transportMode);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = true;
			Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomer.IsAllowed = false;
			securityCheckpoint.IsAllowed = true;

			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				var result = shipmentForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, shipment.IsInDatabase);
		}

		void AssertShipmentSavedWhenControllingCustomerIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm(string transportMode, SecurityCheckpoint securityCheckpoint)
		{
			var shipment = GetShipmentWithConsignorAndConsignee(transportMode);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = true;
			Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomer.IsAllowed = false;
			securityCheckpoint.IsAllowed = false;

			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				int loginFormShownCount = 0;

				var newUserLogin = "User1" + transportMode;
				var newUserPassword = "pass";

				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;
				SecurityTestObject.CreateTestUser(true, securityCheckpoint.Code, transportMode, newUserLogin, newUserPassword);

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						loginForm.DoLoginForTest(newUserLogin, newUserPassword);
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});

				var result = shipmentForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
				AssertEquals(1, loginFormShownCount);
			}

			AssertEquals(true, shipment.IsInDatabase);
		}

		public void TestShipmentFormSavedAndNoAuthorizeRequestPirorToControllingCustomerEffectiveDate()
		{
			AssertShipmentFormSavedAndNoAuthorizeRequestPirorToControllingCustomerEffectiveDate(Constants.TransportModes.Sea);
			AssertShipmentFormSavedAndNoAuthorizeRequestPirorToControllingCustomerEffectiveDate(Constants.TransportModes.Air);
			AssertShipmentFormSavedAndNoAuthorizeRequestPirorToControllingCustomerEffectiveDate(Constants.TransportModes.Road);
			AssertShipmentFormSavedAndNoAuthorizeRequestPirorToControllingCustomerEffectiveDate(Constants.TransportModes.Rail);
			AssertShipmentFormSavedAndNoAuthorizeRequestPirorToControllingCustomerEffectiveDate(Constants.TransportModes.Courier);
		}

		public void TestShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingCustomerEffectiveDate()
		{
			AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingCustomerEffectiveDate(Constants.TransportModes.Sea, ZDateTime.UtcToday.AddDays(-2).ToDateTime());
			AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingCustomerEffectiveDate(Constants.TransportModes.Air, ZDateTime.UtcToday.AddDays(-2).ToDateTime());
			AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingCustomerEffectiveDate(Constants.TransportModes.Road, ZDateTime.UtcToday.AddDays(-2).ToDateTime());
			AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingCustomerEffectiveDate(Constants.TransportModes.Rail, ZDateTime.UtcToday.AddDays(-2).ToDateTime());
			AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingCustomerEffectiveDate(Constants.TransportModes.Courier, ZDateTime.UtcToday.AddDays(-2).ToDateTime());

			AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingCustomerEffectiveDate(Constants.TransportModes.Sea, ZDateTime.UtcToday.ToDateTime());
			AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingCustomerEffectiveDate(Constants.TransportModes.Air, ZDateTime.UtcToday.ToDateTime());
			AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingCustomerEffectiveDate(Constants.TransportModes.Road, ZDateTime.UtcToday.ToDateTime());
			AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingCustomerEffectiveDate(Constants.TransportModes.Rail, ZDateTime.UtcToday.ToDateTime());
			AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingCustomerEffectiveDate(Constants.TransportModes.Courier, ZDateTime.UtcToday.ToDateTime());
		}

		public void TestControllingCustomerRegistryFallBack()
		{
			AssertControllingCustomerRegistryFallBack(Constants.TransportModes.Sea);
			AssertControllingCustomerRegistryFallBack(Constants.TransportModes.Air);
			AssertControllingCustomerRegistryFallBack(Constants.TransportModes.Road);
			AssertControllingCustomerRegistryFallBack(Constants.TransportModes.Rail);
		}

		void AssertShipmentFormNotSavedAndShowAuthorizeRequestAfterControllingCustomerEffectiveDate(string transportMode, DateTime effectiveDate)
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR" + transportMode;
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE" + transportMode;
			consignee.OH_IsConsignee = true;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			shipment.JS_TransportMode = transportMode;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = true;
			shipment.GetControllingCustomerSecurityCheckPoint().IsAllowed = false;

			var registry = shipment.GetMandatoryControllingCustomerEffectiveDateRegistry();

			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, effectiveDate))
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();

				int loginFormShownCount = 0;
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					}
				});

				var result = shipmentForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
			}

			AssertEquals(false, shipment.IsInDatabase);
		}

		void AssertShipmentFormSavedAndNoAuthorizeRequestPirorToControllingCustomerEffectiveDate(string transportMode)
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR" + transportMode;
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE" + transportMode;
			consignee.OH_IsConsignee = true;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			shipment.JS_TransportMode = transportMode;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = true;
			shipment.GetControllingCustomerSecurityCheckPoint().IsAllowed = false;

			var registry = shipment.GetMandatoryControllingCustomerEffectiveDateRegistry();

			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.AddDays(3).ToDateTime()))
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				var result = shipmentForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, shipment.IsInDatabase);
		}

		public void AssertControllingCustomerRegistryFallBack(string transportMode)
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR" + transportMode;
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE" + transportMode;
			consignee.OH_IsConsignee = true;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			shipment.JS_TransportMode = transportMode;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = true;
			shipment.GetControllingCustomerSecurityCheckPoint().IsAllowed = false;

			using (FreightRegistry.Instance.MandatoryControllingCustomerEffectiveDateAll.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.AddDays(3).ToDateTime()))
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				var result = shipmentForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, shipment.IsInDatabase);
		}

		#endregion

		#region Test Department Charges are defaulted correctly

		public void TestDefaultDepartmentChargesForDomesticShipments()
		{
			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			consigneeOrg.OH_RL_NKClosestPort = "AUSYD";
			var consignorOrg = Factory.NewWithValidTestData<OrgHeader>();
			consignorOrg.OH_RL_NKClosestPort = "AUMEL";
			consignorOrg.OH_IsDebtor = true;
			var code1 = Factory.New<AccChargeCode>();
			code1.AC_DepartmentFilterList = "ALL";
			code1.AC_Code = "CODE1";
			code1.AC_ChargeGroup = "FRT";
			var code2 = Factory.New<AccChargeCode>();
			code2.AC_DepartmentFilterList = "ALL";
			code2.AC_Code = "CODE2";
			code2.AC_ChargeGroup = "FRT";
			var domesticRoadDepartment = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FDR");
			var charge1 = domesticRoadDepartment.DeptCharges.AddNew();
			charge1.GD_AC = code1.PK;
			var charge2 = domesticRoadDepartment.DeptCharges.AddNew();
			charge2.GD_AC = code2.PK;

			Factory.Save();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUMEL";

			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();
				shipment.JS_TransportMode = "ROA";
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignorOrg.PK;
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = consigneeOrg.PK;
				AssertEquals(consignorOrg.OH_RL_NKClosestPort, shipment.JS_RL_NKOrigin);
				AssertEquals(consigneeOrg.OH_RL_NKClosestPort, shipment.JS_RL_NKDestination);
				AssertEquals(domesticRoadDepartment.PK, shipment.Job.JH_GE);
				AssertEquals(consignorOrg.PK, shipment.Job.LocalChargesPK);
				var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, shipment.Job.PK));
				AssertEquals(2, charges.Length);
				AssertEquals(consignorOrg.PK, charges[0].JR_OH_SellAccount);
				AssertEquals(consignorOrg.PK, charges[1].JR_OH_SellAccount);
			}
		}

		public void TestLocalClientDefaulting()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			var localOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "AUBNE"));
			var chinaOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "CNSHA"));

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUBNE";

			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();

				shipment.ConsignorPK = localOrg.PK;
				shipment.ConsigneePK = chinaOrg.PK;
				shipment.JS_INCO = "FOB";
				AssertEquals("Local client is set to local org when Consignor is set before Consignee for export", localOrg.PK, shipment.Job.LocalCharges.PK);
			}

			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUBNE";

			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();

				shipment.ConsigneePK = chinaOrg.PK;
				shipment.ConsignorPK = localOrg.PK;
				shipment.JS_INCO = "FOB";
				AssertEquals("Local client is set to local org when Consignee is set before Consignor for export", localOrg.PK, shipment.Job.LocalCharges.PK);
			}
		}

		#endregion

		#region Tab Visible Persistence

		public void TestTabVisibilityPersistence()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Factory.Save();

			using (ShipmentFormForTest shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();

				ZTabPage tab1 = (ZTabPage)shipmentForm.MainTabControl.AllTabPages[0];
				ZTabPage tab2 = (ZTabPage)shipmentForm.MainTabControl.AllTabPages[1];
				AssertEquals(true, ((ITabVisibilityDeciderPersistence)shipmentForm).RetrieveTabPageVisible(tab1));
				AssertEquals(true, ((ITabVisibilityDeciderPersistence)shipmentForm).RetrieveTabPageVisible(tab2));

				tab1.TabVisible = false;
				tab2.TabVisible = false;

				((ITabVisibilityDeciderPersistence)shipmentForm).StoreTabVisible(tab1);
				((ITabVisibilityDeciderPersistence)shipmentForm).StoreTabVisible(tab2);
			}

			using (ShipmentFormForTest shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();

				ZTabPage tab1 = (ZTabPage)shipmentForm.MainTabControl.AllTabPages[0];
				ZTabPage tab2 = (ZTabPage)shipmentForm.MainTabControl.AllTabPages[1];

				AssertEquals(false, ((ITabVisibilityDeciderPersistence)shipmentForm).RetrieveTabPageVisible(tab1));
				AssertEquals(false, ((ITabVisibilityDeciderPersistence)shipmentForm).RetrieveTabPageVisible(tab2));

				MenuItem viewMenu = shipmentForm.MainMenu.MenuItems.FindByText("View");
				typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, viewMenu, new object[] { EventArgs.Empty });

				viewMenu.MenuItems[0].PerformClick();
				AssertEquals(true, ((ITabVisibilityDeciderPersistence)shipmentForm).RetrieveTabPageVisible(tab1));
				AssertEquals(false, ((ITabVisibilityDeciderPersistence)shipmentForm).RetrieveTabPageVisible(tab2));

				viewMenu.MenuItems[1].PerformClick();
				AssertEquals(true, ((ITabVisibilityDeciderPersistence)shipmentForm).RetrieveTabPageVisible(tab1));
				AssertEquals(true, ((ITabVisibilityDeciderPersistence)shipmentForm).RetrieveTabPageVisible(tab2));

				viewMenu.MenuItems[0].PerformClick();
				AssertEquals(false, ((ITabVisibilityDeciderPersistence)shipmentForm).RetrieveTabPageVisible(tab1));
				AssertEquals(true, ((ITabVisibilityDeciderPersistence)shipmentForm).RetrieveTabPageVisible(tab2));
			}

			using (ShipmentFormForTest shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();

				ZTabPage tab1 = (ZTabPage)shipmentForm.MainTabControl.AllTabPages[0];
				ZTabPage tab2 = (ZTabPage)shipmentForm.MainTabControl.AllTabPages[1];

				AssertEquals(false, ((ITabVisibilityDeciderPersistence)shipmentForm).RetrieveTabPageVisible(tab1));
				AssertEquals(true, ((ITabVisibilityDeciderPersistence)shipmentForm).RetrieveTabPageVisible(tab2));
			}
		}

		#endregion

		#region Resizable

		[RequiresSTA]
		public void TestIsResizableByTabPageAllowed()
		{
			using (var form = GetNewZShipmentForm())
			{
				AssertEquals("IsResizableByTabPageAllowed", true, form.IsResizableByTabPageAllowed);
			}
		}

		#endregion

		#region Plugins

		#region TestPlugIns

		[RequiresSTA]
		public void TestPlugIns()
		{
			var originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				using (ShipmentForm shipmentForm = GetNewZShipmentForm())
				{
					shipmentForm.Show();
					AssertContainsPlugIn(shipmentForm, ControllerIDs.Customs.AU.HouseAirCargo, Constants.CountryCodes.Australia);
					AssertContainsPlugIn(shipmentForm, ControllerIDs.Customs.AU.SeaCargo, Constants.CountryCodes.Australia);
					AssertContainsPlugIn(shipmentForm, ControllerIDs.Customs.AU.SeaCargoDepot, Constants.CountryCodes.Australia);
					AssertContainsPlugIn(shipmentForm, ControllerIDs.Customs.NZ.ShipmentToExpressECIConverter, Constants.CountryCodes.NewZealand);
					AssertContainsPlugIn(shipmentForm, ControllerIDs.Customs.NZ.ShipmentToSeaCargoWriteOffConverter, Constants.CountryCodes.NewZealand);
					AssertContainsPlugIn(shipmentForm, ControllerIDs.Customs.JobDeclaration);
					AssertContainsPlugIn(shipmentForm, ControllerIDs.JobInvoicing, Env.Security.MaintainShipmentJobInvoicing);
					AssertContainsPlugIn(shipmentForm, ControllerIDs.DocAddresses);
					AssertContainsPlugIn(shipmentForm, ControllerIDs.eDocsPlugIn);
					AssertContainsPlugIn(shipmentForm, ControllerIDs.DocDataPlugIn);
					AssertContainsPlugIn(shipmentForm, ControllerIDs.Customs.SG.CMDShipment, Constants.CountryCodes.Singapore);
					AssertContainsPlugIn(shipmentForm, ControllerIDs.ETailShipment);
					AssertContainsPlugIn(shipmentForm, ControllerIDs.DocumentVisualizer);
					using (var form = new ShipmentFormForTest(Shipment))
					{
						form.Show();
						var plugIn = form.ElectronicMessagingTabControl.PlugIns.GetPlugIn(ControllerIDs.CargoIMPPhase2);
						AssertNotNull("Cargo IMP Phase 2 should be plugged in", plugIn);
						plugIn = form.ElectronicMessagingTabControl.PlugIns.GetPlugIn(ControllerIDs.ExportConsignmentReleaseAdvice);
						AssertNotNull("Export Consignment Release (EXREL) should be plugged in", plugIn);
						AssertElectronicMessagingTabControlContainsPlugIn(form, ControllerIDs.Customs.IL.CustomsMessaging, null, CountryCodes.Israel);
						AssertElectronicMessagingTabControlContainsPlugIn(form, ControllerIDs.ElectronicBOL, null, ZString.Empty);
					}

					CheckCcsuk();
				}

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.UnitedStates);

				using (var shipmentForm = GetNewZShipmentForm())
				{
					AssertContainsPlugIn(shipmentForm, ControllerIDs.Customs.US.InBond, Constants.CountryCodes.UnitedStates);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountryCode);
			}
		}

		void CheckCcsuk()
		{
			using (var form = GetNewZShipmentForm())
			{
				form.Show();
				AssertContainsPlugIn(form, ControllerIDs.Customs.GB.CcsukAirInventoryHouse, Core.Constants.CountryCodes.UnitedKingdom);
			}
		}

		#endregion

		#region TestPlugIns_TransportBooking

		public void TestPlugIns_TransportBooking()
		{
			using (var form = GetNewZShipmentForm())
			{
				form.Show();
				AssertContainsPlugIn(form, ControllerIDs.DtbBooking);
			}
		}

		#endregion

		#region AssertContainsPlugIn

		void AssertContainsPlugIn(ShipmentForm form, ControllerID controllerID)
		{
			AssertContainsPlugIn(form, controllerID, null, "");
		}

		void AssertContainsPlugIn(ShipmentForm form, ControllerID controllerID, ZString countryCode)
		{
			AssertContainsPlugIn(form, controllerID, null, countryCode);
		}

		void AssertContainsPlugIn(ShipmentForm form, ControllerID controllerID, SecurityCheckpoint expectedSecurityCheckpoint)
		{
			AssertContainsPlugIn(form, controllerID, expectedSecurityCheckpoint, "");
		}

		void AssertContainsPlugIn(ShipmentForm form, ControllerID controllerID, SecurityCheckpoint expectedSecurityCheckpoint, ZString countryCode)
		{
			if (countryCode.IsEmpty)
			{
				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
			}
			else
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
			}
			var plugIn = form.PlugIns.GetPlugIn(controllerID);
			AssertNotNull("Should be plugged in", plugIn);
			if (expectedSecurityCheckpoint != null)
			{
				AssertEquals("Incorrect security checkpoint", expectedSecurityCheckpoint, plugIn.SecurityCheckpoint);
			}
		}

		void AssertElectronicMessagingTabControlContainsPlugIn(ShipmentFormForTest form, ControllerID controllerID, SecurityCheckpoint expectedSecurityCheckpoint, ZString countryCode)
		{
			if (countryCode.IsEmpty)
			{
				GlbCompany.CurrentCompany.SetCountry(CountryCodes.Australia);
			}
			else
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
			}

			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Israel);
			var plugIn = form.ElectronicMessagingTabControl.PlugIns.GetPlugIn(controllerID);
			AssertNotNull("Should be plugged in", plugIn);
			if (expectedSecurityCheckpoint != null)
			{
				AssertEquals("Incorrect security checkpoint", expectedSecurityCheckpoint, plugIn.SecurityCheckpoint);
			}
		}

		void AssertDoesNotContainPlugin(ShipmentForm form, ControllerID controllerID)
		{
			var plugIn = form.PlugIns.GetPlugIn(controllerID);
			Assert("Should not be plugged in", plugIn == null || !plugIn.Enabled);
		}

		#endregion

		public void TestSGBrokeragePlugIn()
		{
			var sGPluginLoaded = false;
			GlbCompany.CurrentCompany.SetCountry("SG");
			using (ShipmentForm form = GetNewZShipmentForm())
			{
				form.Show();

				foreach (var plugIn in form.PlugIns.Instances)
				{
					if (plugIn.GetType().FullName == "Enterprise.SGCustoms.ZInterface.PluginToFreight"
						|| plugIn.GetType().FullName == "Enterprise.Customs.SG.V4.GUI.BrokeragePlugIn")
					{
						sGPluginLoaded = true;
						break;
					}
				}

				AssertDoesNotContainPlugin(form, ControllerIDs.Customs.US.InBond);
			}
			Assert(sGPluginLoaded);
		}

		public void TestCAShipmentCargoReportPlugin()
		{
			CountrySpecificForwardingShipmentSupportTest.DeleteAnyCarrierCode(Factory);
			Shipment = GetShipmentWithNoErrors();
			var consol = Shipment.Consols.AddNew();
			consol.FillWithValidTestData();
			var transport = Shipment.MostInterestingTransport;
			transport.JW_RL_NKDiscPort = "CATOR";
			ChildEditableService.SetState(Shipment.Factory, ChildEditableServiceStates.Shipment);
			using (ShipmentForm shipmentForm2 = new ShipmentForm(Shipment))
			{
				shipmentForm2.Show();
				AssertDoesNotContainPlugin(shipmentForm2, ControllerIDs.Customs.CA.CAShipmentCargoReport);
				AssertDoesNotContainPlugin(shipmentForm2, ControllerIDs.Customs.US.InBond);
			}
			try
			{
				var currentCompany = GlbCompany.GetCurrentCompany(Factory);
				currentCompany.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "CC", Constants.CountryCodes.Canada);
				Factory.Save();
				using (ShipmentForm shipmentForm = new ShipmentForm(Shipment))
				{
					shipmentForm.Show();
					AssertContainsPlugIn(shipmentForm, ControllerIDs.Customs.CA.CAShipmentCargoReport);
				}
				transport.JW_RL_NKDiscPort = "GBLON";
				using (ShipmentForm shipmentForm2 = new ShipmentForm(Shipment))
				{
					shipmentForm2.Show();
					AssertContainsPlugIn(shipmentForm2, ControllerIDs.Customs.CA.CAShipmentCargoReport);
				}
			}
			finally
			{
				CountrySpecificForwardingShipmentSupportTest.DeleteAnyCarrierCode(Factory);
			}
		}

		#endregion

		#region ISupportSwitchTabPage

		public void TestSwitchTabPage()
		{
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (var shipmentForm = GetNewZShipmentForm())
			{
				const string tabPageName = "ComplianceRiskTabPage";
				shipmentForm.Show();
				AssertNotEquals(tabPageName, shipmentForm.MainTabControl.SelectedTab.Name);

				((ISupportSwitchTabPage)shipmentForm).SwitchTabPage(tabPageName);
				AssertEquals(tabPageName, shipmentForm.MainTabControl.SelectedTab.Name);
			}
		}

		#endregion

		#region IsCargoOnlyControlInvisibleOnNew

		[RequiresSTA]
		public void TestIsCargoOnlyControlInvisibleOnNew()
		{
			using (var shipmentForm = GetNewZShipmentForm())
			{
				const string tabPageName = "RoutingTabPage";
				shipmentForm.Show();
				((ISupportSwitchTabPage)shipmentForm).SwitchTabPage(tabPageName);
				AssertEquals(tabPageName, shipmentForm.MainTabControl.SelectedTab.Name);
				var control = shipmentForm.GetControl<ZDropEdit>("JW_AdditionalTransportModeBoundDropEdit");
				AssertEquals(false, control.Visible);
			}
		}

		#endregion

		#region Saving

		[RequiresSTA]
		public void TestSaving()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipmentForTestSaving>();
			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				var anotherShipment = Factory.New<ForwardingShipment>();
				anotherShipment.JS_HouseBill = "HB101";
				shipment.JS_HouseBill = "HB101";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var result = shipmentForm.ShowPreSaveDialogs();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals("Should show the Duplicate HouseBill dialog", "This House Bill number is already in use on: \r\nNew Shipment\r\n\r\nDo you wish to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZDialogResult.No, UnitTestUserNotification.Instance.LastMessage.Answer);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				shipment.JS_HouseBill = "HB102";
				shipment.WasAttachedToConsolOnRetrieve = true;
				result = shipmentForm.ShowPreSaveDialogs();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals("Should show the Unallocating Shipment dialog", "This Shipment is no longer on a Consol and will be unallocated after you save. Do you wish to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZDialogResult.No, UnitTestUserNotification.Instance.LastMessage.Answer);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				shipment.WasAttachedToConsolOnRetrieve = false;
				shipment.ConsignorPK = Factory.New<OrgHeader>().PK;
				shipment.ConsigneePK = Factory.New<OrgHeader>().PK;
				result = shipmentForm.ShowPreSaveDialogs();
				AssertEquals(ContinueWithSave.Yes, result);
				AssertEquals("Should show to Save Buyer-Supplier link dialog", "Do you wish to save this Supplier-Consignor/Buyer-Consignee relationship?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZDialogResult.No, UnitTestUserNotification.Instance.LastMessage.Answer);
				AssertEquals("Should not be linked", 0, shipment.Consignee.SupplierLinks.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				result = shipmentForm.ShowPreSaveDialogs();
				AssertEquals(ContinueWithSave.Yes, result);
				AssertEquals("Should show to Save Buyer-Supplier link dialog", "Do you wish to save this Supplier-Consignor/Buyer-Consignee relationship?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZDialogResult.Yes, UnitTestUserNotification.Instance.LastMessage.Answer);
				AssertEquals("Should be linked now", 1, shipment.Consignee.SupplierLinks.Count);
			}
		}

		public void TestSubShipmentJobHeadersAreCreatedOnSave()
		{
			var savedSubShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			AssertNull("Precondition: savedShipment doesn't have job", savedSubShipment.JobHeader);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			using (var testForm = new ShipmentFormForTest(shipment))
			{
				var unsavedSubShipment1 = shipment.CoLoadShipments.AddNew();
				var unsavedSubShipment2 = shipment.CoLoadShipments.AddNew();
				shipment.CoLoadShipments.Add(savedSubShipment);

				testForm.Show();

				unsavedSubShipment1.JS_RL_NKOrigin = "AUBNE";
				unsavedSubShipment1.JS_RL_NKDestination = "MYKUL";

				unsavedSubShipment2.JS_RL_NKOrigin = "AUBNE";
				unsavedSubShipment2.JS_RL_NKDestination = "MYKUL";

				CombineAssertions("Pre-Condition", delegate
				{
					AssertNull("Unsaved Shipment 1: no job yet", unsavedSubShipment1.JobHeader);
					AssertNull("Unsaved Shipment 2: no job yet", unsavedSubShipment2.JobHeader);
					AssertNull("Saved Shipment: no job yet", savedSubShipment.JobHeader);
				});

				testForm.ValidateAndSave();

				CombineAssertions(delegate
				{
					AssertNotNull("Unsaved Shipment 1: job created", unsavedSubShipment1.JobHeader);
					AssertNotNull("Unsaved Shipment 2: job created", unsavedSubShipment2.JobHeader);
					AssertNull("Saved Shipment: no job is created for previously saved shipment", savedSubShipment.JobHeader);
				});

				unsavedSubShipment1.JobHeader.Dispose();
				unsavedSubShipment2.JobHeader.Dispose();
			}
		}

		[RequiresSTA]
		public void TestHouseBillReGeneration()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var customisations = new BillOfLadingNumberCustomisationsByServiceLevel();
			var allCustomisation = customisations.BillOfLadingNumberCustomisations["ALL"];
			allCustomisation.RemoveFountainPrefix = true;
			foreach (BillOfLadingNumberCustomisationElement element in allCustomisation.Elements)
			{
				switch (element.Key)
				{
					case BillOfLadingNumberCustomisationElement.Keys.SequenceNumber:
						element.Include = true;
						element.Detail = "6";
						element.Order = 50;
						break;

					case BillOfLadingNumberCustomisationElement.Keys.ServiceLevel:
						element.Include = true;
						element.Fountain = true;
						element.Order = 1;
						break;

					default:
						element.Include = false;
						break;
				}
			}

			FreightRegistry.Instance.HouseBillNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisations);
			FreightRegistry.Instance.HouseBillNumberRegeneration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = GetShipmentWithNoErrors();
			shipment.JS_UniqueConsignRef = "";
			shipment.JS_RS_NKServiceLevel = "STD";
			shipment.JS_RL_NKOrigin = "AUSYD";
			Factory.Save();
			AssertEquals("STD000001", shipment.JS_HouseBill);
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			using (var form = new ShipmentForm(shipment))
			{
				shipment.JS_RS_NKServiceLevel = "D2D";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.FireSaveButton();
				AssertEquals("Service Level has changed from 'STD' to 'D2D'. Do you wish to regenerate the House Bill Number?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZDialogResult.No, UnitTestUserNotification.Instance.LastMessage.Answer);
				AssertEquals("STD000001", shipment.JS_HouseBill);

				shipment.JS_RS_NKServiceLevel = "TSP";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.FireSaveButton();
				AssertEquals("Service Level has changed from 'D2D' to 'TSP'. Do you wish to regenerate the House Bill Number?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZDialogResult.Yes, UnitTestUserNotification.Instance.LastMessage.Answer);
				AssertEquals("TSP000001", shipment.JS_HouseBill);
			}
		}

		public void TestEditShipmentFormNotReorderingGateways()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			consol.Shipments.Add(shipment);

			Factory.Save();

			var gateway = shipment.Gateways.AddNew();
			var gatewayOrg = Factory.NewWithValidTestData<OrgHeader>();
			gateway.JSG_OA_ForwarderAddress = gatewayOrg.MainAddress.PK;

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			ChildEditableService.SetState(otherFactory, ChildEditableServiceStates.Shipment);

			using (var shipmentForm = new ShipmentForm(otherFactory.Load<ForwardingShipment>(shipment.PK)))
			{
				shipmentForm.Show();

				shipment.Gateways.SwapGateways(Convert.ToByte(3), Convert.ToByte(1));

				AssertEquals("Manually added gateway should be first gateway before saving", shipment.Gateways[0].JSG_OA_ForwarderAddress, gatewayOrg.MainAddress.PK);
				shipmentForm.FireSaveButton();
				AssertEquals("Manually added gateway should be first gateway after saving", shipment.Gateways[0].JSG_OA_ForwarderAddress, gatewayOrg.MainAddress.PK);
			}
		}

		class ForwardingShipmentForTestSaving : ForwardingShipment
		{
			public ForwardingShipmentForTestSaving(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool WasAttachedToConsolOnRetrieve
			{
				get { return fWasAttachedToConsolOnRetrieve; }
				set { fWasAttachedToConsolOnRetrieve = value; }
			}
		}

		public void TestSaving_ConsolsPreAllocationsNotExceeded()
		{
			var shipment = GetShipmentWithPreAllocationExceededConsols();
			shipment.JS_ActualWeight = 10m;
			AssertEquals("Precondition: consol's pre-allocation not exceeded", false, shipment.Consols.Cast<ForwardingConsol>().Any(consol => consol.IsPreAllocationExceededAndRestricted));
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (ShipmentFormForTest form = new ShipmentFormForTest(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.Show();
				ContinueWithSave continueWithSave = form.ShowPreSaveDialogs();

				AssertNull("Dialog wasn't shown", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Saving allowed", ContinueWithSave.Yes, continueWithSave);
			}
		}

		public void TestSaving_ConsolsPreAllocationsExceeded_Adjusted()
		{
			ForwardingShipment shipment = GetShipmentWithPreAllocationExceededConsols();

			IEnumerable<ForwardingConsol> expectedConsolsInDialog = shipment.Consols.Cast<ForwardingConsol>().Where(consol => consol.IsPreAllocationExceededAndRestricted);
			AssertEquals("Precondition: consols with pre-allocation exceeded", 2, expectedConsolsInDialog.Count());
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (ShipmentFormForTest form = new ShipmentFormForTest(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				form.Show();
				ContinueWithSave continueWithSave = form.ShowPreSaveDialogs();

				AllocationAdjustmentDialog dialog = ZFormModaliser.LastFormShownDialogForTest as AllocationAdjustmentDialog;
				AssertNotNull("Dialog was shown", dialog);
				AssertEquals("Saving allowed", ContinueWithSave.Yes, continueWithSave);

				AllocationAdjustmentsSecurity adjustmentsSecurity = (AllocationAdjustmentsSecurity)dialog.LastDataSourceForTest;
				AssertContainsExactElementsInAnyOrder(expectedConsolsInDialog, adjustmentsSecurity.Adjustments.Cast<AllocationAdjustment>().Select(x => x.Consol));
			}
		}

		public void TestSaving_ConsolsPreAllocationsExceeded_NotAdjusted()
		{
			var shipment = GetShipmentWithPreAllocationExceededConsols();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (ShipmentFormForTest form = new ShipmentFormForTest(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

				form.Show();
				ContinueWithSave continueWithSave = form.ShowPreSaveDialogs();

				AllocationAdjustmentDialog dialog = ZFormModaliser.LastFormShownDialogForTest as AllocationAdjustmentDialog;
				AssertNotNull("Dialog was shown", dialog);
				AssertEquals("Saving restricted", ContinueWithSave.No, continueWithSave);
			}
		}

		[RequiresSTA]
		public void TestSaving_ShipmentWithDirectConsol_ChangedToAssemblyMaster()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var shipment = GetShipmentWithNoErrors();
			var consol = shipment.Consols.AddNew();

			consol.JK_AgentType = "DRT";
			shipment.JS_ShipmentType = "ASM";
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (ShipmentFormForTest form = new ShipmentFormForTest(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				form.ShowPreSaveDialogs();

				var notificationMessage = UnitTestUserNotification.Instance.LastMessage;
				AssertNotNull("Dialog was shown", notificationMessage);
				AssertEquals("Question Assembly Master as Direct Master may breach customs law", notificationMessage.ToString());
			}
		}

		public void TestSaving_AssemblyMasterAsDirectMaster_ShouldShowAMessageboxWithCheckboxIfRegistrySettingIsSetToTrue()
		{
			var shipment = GetShipmentWithNoErrors();
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = "DRT";
			shipment.JS_ShipmentType = "ASM";
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (FreightRegistry.Instance.AssemblyMasterOnDirectConsolComplianceDisclaimer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ShipmentFormForTest form = new ShipmentFormForTest(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.ShowPreSaveDialogs();
				var shownDialog = ZFormModaliser.LastFormShownDialogForTest as AcknowledgementMessageBox;
				AssertNotNull("Acknowledgement message box is shown", shownDialog);
			}
		}

		[RequiresSTA]
		public void TestSaving_AssemblyMasterAsDirectMaster_ShouldNotShowAMessageboxWithCheckboxIfRegistrySettingIsSetToFalse()
		{
			var shipment = GetShipmentWithNoErrors();
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = "DRT";
			shipment.JS_ShipmentType = "ASM";
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (FreightRegistry.Instance.AssemblyMasterOnDirectConsolComplianceDisclaimer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ShipmentFormForTest form = new ShipmentFormForTest(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.ShowPreSaveDialogs();
				var shownDialog = ZFormModaliser.LastFormShownDialogForTest as AcknowledgementMessageBox;
				AssertNull("Acknowledgement message box is not shown", shownDialog);
			}
		}

		public void TestSaving_AssemblyMasterAsDirectMaster_ShouldLogEvent()
		{
			var shipment = GetShipmentWithNoErrors();
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = "DRT";
			shipment.JS_ShipmentType = "ASM";
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (FreightRegistry.Instance.AssemblyMasterOnDirectConsolComplianceDisclaimer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ShipmentFormForTest form = new ShipmentFormForTest(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var continueWithSaveResult = form.ShowPreSaveDialogs();
				AssertEquals(ContinueWithSave.Yes, continueWithSaveResult);
				var ackEvent = shipment.Logs.MostRecentLogByEventTime(Events.Acknowledged);
				AssertNotNull("Event was logged", ackEvent);
				var eventParameters = StmALog.GetParametersFromReference(ackEvent.SL_Reference);
				AssertEquals(eventParameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type], "that using Assembly Master as Direct Master might breach customs law by typing the disclaimer");
			}
		}

		[RequiresSTA]
		public void TestSaving_AssemblyMasterAsDirectMaster_ShouldLogEventIndicatingThatTheUserTickedTheCheckbox()
		{
			var shipment = GetShipmentWithNoErrors();
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = "DRT";
			shipment.JS_ShipmentType = "ASM";
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (FreightRegistry.Instance.AssemblyMasterOnDirectConsolComplianceDisclaimer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ShipmentFormForTest form = new ShipmentFormForTest(shipment))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var continueWithSaveResult = form.ShowPreSaveDialogs();
				AssertEquals(continueWithSaveResult, ContinueWithSave.Yes);
				var ackEvent = shipment.Logs.MostRecentLogByEventTime(Events.Acknowledged);
				AssertNotNull("Event was logged", ackEvent);
				var eventParameters = StmALog.GetParametersFromReference(ackEvent.SL_Reference);
				AssertEquals(eventParameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type], "that using Assembly Master as Direct Master might breach customs law by checking the \"I acknowledge\" tick box");
			}
		}

		[RequiresSTA]
		public void TestSaving_ControllingPartiesDefaulted()
		{
			FreightRegistry.Instance.DefaultShipmentControllingCustomer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightRegistry.Instance.DefaultShipmentControllingAgent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = GetShipmentWithNoErrors();

			var controllingCustomer1 = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomer2 = Factory.NewWithValidTestData<OrgHeader>();
			var controllingAgent1 = Factory.NewWithValidTestData<OrgHeader>();
			var controllingAgent2 = Factory.NewWithValidTestData<OrgHeader>();

			var relatedParty1 = controllingCustomer1.AllRelatedParties.AddNew();
			relatedParty1.PR_OH_RelatedParty = controllingAgent1.PK;
			relatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
			relatedParty1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Sales;

			var relatedParty2 = controllingCustomer2.AllRelatedParties.AddNew();
			relatedParty2.PR_OH_RelatedParty = controllingAgent2.PK;
			relatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
			relatedParty2.PR_FreightDirection = RelatedPartyDirectionList.Codes.Sales;

			var supplierLink = shipment.Consignee.SupplierLinks.AddNew();
			supplierLink.OL_OH_Supplier = shipment.Consignor.PK;

			var supplierLinkTransportMode = supplierLink.OrgSupBuyLinkTrnModes[0];
			supplierLinkTransportMode.PF_TransportMode = shipment.JS_TransportMode;
			supplierLinkTransportMode.PF_ContainerMode = shipment.JS_PackingMode;
			supplierLinkTransportMode.PF_OH_ControllingCustomer = controllingCustomer1.PK;

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.FireSaveButton();

				AssertEquals(controllingCustomer1.PK, shipment.ControllingCustomer.PK);
				AssertEquals(controllingAgent1.PK, shipment.ControllingAgentDocumentaryAddress.OrganisationPK);

				shipment.ControllingCustomerNameOrPK = controllingCustomer2.PK.ToString();

				form.FireSaveButton();

				AssertEquals(controllingCustomer2.PK, shipment.ControllingCustomer.PK);
				AssertEquals(controllingAgent2.PK, shipment.ControllingAgentDocumentaryAddress.OrganisationPK);
			}
		}

		ForwardingShipment GetShipmentWithPreAllocationExceededConsols()
		{
			var shipment = GetShipmentWithNoErrors();
			shipment.JS_ActualWeight = 100m;

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_TotalShipmentActWeightCheck = 100m;

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_TotalShipmentActWeightCheck = 120m;

			var consol3 = shipment.Consols.AddNew();
			consol3.JK_TotalShipmentActWeightCheck = 1000m;

			var allocationChecks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			allocationChecks.Weight.Action = PreAllocationCheck.Actions.Restriction;
			allocationChecks.Weight.Percentage = 50m;
			ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allocationChecks);

			AssertEquals("Precondition: consol's pre-allocation exceeded", true, consol1.IsPreAllocationExceededAndRestricted);
			AssertEquals("Precondition: consol's pre-allocation exceeded", true, consol2.IsPreAllocationExceededAndRestricted);
			AssertEquals("Precondition: consol's pre-allocation not exceeded", false, consol3.IsPreAllocationExceededAndRestricted);

			return shipment;
		}

		public void TestInactiveAddressShowsWarningAfterClickingSaveButton()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG1";
			org1.OH_FullName = "Org 1";
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG2";
			org2.OH_FullName = "Org 2";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = org1.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = org2.PK;
			shipment.ConsigneeDocumentaryAddress.Address.OA_IsActive = false;
			shipment.ConsignorDocumentaryAddress.Address.OA_IsActive = false;
			((ILightValidationInternals)shipment.ConsigneeDocumentaryAddress).IsValid = true;
			((ILightValidationInternals)shipment.ConsignorDocumentaryAddress).IsValid = true;
			Factory.Save();

			var shipmentInAnotherFactory = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);
			ChildEditableService.SetState(shipmentInAnotherFactory.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentFormForTest(shipmentInAnotherFactory))
			{
				form.Show();
				var jobDocAddress1 = ((IZAddress)shipmentInAnotherFactory.ConsigneeDocumentaryAddress).AddressList; // load the consignee address list and add the inactive address
				var jobDocAddress2 = ((IZAddress)shipmentInAnotherFactory.ConsignorDocumentaryAddress).AddressList; // load the consignor address list and add the inactive address
				form.FireSaveButton();
				AssertHasWarning("inactive consignee address", shipmentInAnotherFactory.ConsigneeDocumentaryAddress.E2_OA_AddressInfo, "This Consignee Documentary Address: Address is inactive.");
				AssertHasWarning("inactive consignor address", shipmentInAnotherFactory.ConsignorDocumentaryAddress.E2_OA_AddressInfo, "This Consignor Documentary Address: Address is inactive.");
			}
		}

#if !WINZOR
		[RequiresSTA]
		public void TestShipmentFormLoadPerformanceTracker()
		{
			var expectedLogFileDir = Enterprise.Freight.Forwarding.Logging.PerformanceLogger.LogFileDir;
			if (Directory.Exists(expectedLogFileDir))
			{
				Enterprise.Freight.Forwarding.Logging.PerformanceLogger.Shutdown();
				Directory.Delete(expectedLogFileDir, recursive: true);
				Enterprise.Freight.Forwarding.Logging.PerformanceLogger.ConfigNLog();
			}

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipmentForTestSaving>();

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "AUSYD";

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_LinePrice = 10000;
			packline.JL_HarmonisedCode = "12345678";
			Factory.Save();

			var numberOfRecords = shipment.OuterPackLines.Count;

			var objectType = shipment.GetType().Name;
			var pk = shipment.PK.ToString();

			var formOpenTimespan = TimeSpan.FromSeconds(1);
			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				// Act
				Thread.Sleep(formOpenTimespan);
				shipmentForm.InvokeFormOnShown_ForTest();
			}

			Enterprise.Freight.Forwarding.Logging.PerformanceLogger.Flush();
			Enterprise.Freight.Forwarding.Logging.PerformanceLogger.Shutdown();

			// Assert
			var logDir = Enterprise.Freight.Forwarding.Logging.PerformanceLogger.LogFileDir;
			AssertEquals(true, Directory.Exists(logDir));
			var logFiles = Directory.GetFiles(logDir, "*.log");
			AssertNotNull(logFiles);
			AssertEquals(1, logFiles.Length);

			var content = File.ReadAllText(logFiles[0]);
			var lines = content.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			AssertNotNull(lines);
			AssertEquals(2, lines.Length);

			// first line
			var performanceInfo = JsonSerializer.Deserialize<PerformanceInfo>(lines[0]);
			AssertNotNull(performanceInfo);
			AssertNotEquals(DateTime.MinValue, performanceInfo.Timestamp);
			AssertEquals(objectType, performanceInfo.Object);
			AssertEquals(pk, performanceInfo.Pk);
			AssertEquals(shipment.OuterPackLines.Count, performanceInfo.Records);
			AssertEquals(0, performanceInfo.Duration);
			AssertEquals("Load", performanceInfo.Op);
			AssertEquals("start", performanceInfo.Message);

			// second line
			performanceInfo = JsonSerializer.Deserialize<PerformanceInfo>(lines[1]);
			AssertNotNull(performanceInfo);
			AssertNotEquals(DateTime.MinValue, performanceInfo.Timestamp);
			AssertEquals(objectType, performanceInfo.Object);
			AssertEquals(pk, performanceInfo.Pk);
			AssertEquals(shipment.OuterPackLines.Count, performanceInfo.Records);
			AssertGreaterThan(performanceInfo.Duration, 0);
			AssertEquals("Load", performanceInfo.Op);
			AssertEquals("completed", performanceInfo.Message);
		}

		[RequiresSTA]
		public void TestShipmentFormLoadPerformanceTracker_WhenShipmentIsNull()
		{
			var expectedLogFileDir = Enterprise.Freight.Forwarding.Logging.PerformanceLogger.LogFileDir;
			if (Directory.Exists(expectedLogFileDir))
			{
				Enterprise.Freight.Forwarding.Logging.PerformanceLogger.Shutdown();
				Directory.Delete(expectedLogFileDir, recursive: true);
				Enterprise.Freight.Forwarding.Logging.PerformanceLogger.ConfigNLog();
			}

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipmentForTestSaving>();

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "AUSYD";

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_LinePrice = 10000;
			packline.JL_HarmonisedCode = "12345678";
			Factory.Save();

			var numberOfRecords = shipment.OuterPackLines.Count;

			var objectType = shipment.GetType().Name;
			var pk = shipment.PK.ToString();

			var formOpenTimespan = TimeSpan.FromSeconds(1);
			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				// Act
				Thread.Sleep(formOpenTimespan);
				shipmentForm.Dispose();
				AssertNoExceptionThrown(shipmentForm.InvokeFormOnShown_ForTest);
			}

			Enterprise.Freight.Forwarding.Logging.PerformanceLogger.Flush();
			Enterprise.Freight.Forwarding.Logging.PerformanceLogger.Shutdown();

			// Assert
			var logDir = Enterprise.Freight.Forwarding.Logging.PerformanceLogger.LogFileDir;
			AssertEquals(true, Directory.Exists(logDir));
			var logFiles = Directory.GetFiles(logDir, "*.log");
			AssertEquals(1, logFiles.Length);

			var content = File.ReadAllText(logFiles[0]);
			var lines = content.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			AssertNotNull(lines);
			AssertEquals(1, lines.Length);
		}

		public void TestShipmentFormSavePerformanceTracker()
		{
			var expectedLogFileDir = Enterprise.Freight.Forwarding.Logging.PerformanceLogger.LogFileDir;
			if (Directory.Exists(expectedLogFileDir))
			{
				Enterprise.Freight.Forwarding.Logging.PerformanceLogger.Shutdown();
				Directory.Delete(expectedLogFileDir, recursive: true);
				Enterprise.Freight.Forwarding.Logging.PerformanceLogger.ConfigNLog();
			}

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipmentForTestSaving>();

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "AUSYD";

			var objectType = shipment.GetType().Name;
			var pk = shipment.PK.ToString();

			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				// Act
				shipment.JS_HouseBill = "HB101";
				shipment.WasAttachedToConsolOnRetrieve = true;

				var consignorOrg = Factory.New<OrgHeader>();
				consignorOrg.OH_Code = "ORG1";
				var consigneeOrg = Factory.New<OrgHeader>();
				consigneeOrg.OH_Code = "ORG2";

				shipment.ConsignorPK = consignorOrg.PK;
				shipment.ConsigneePK = consigneeOrg.PK;

				var packline = shipment.OuterPackLines.AddNew();
				packline.JL_LinePrice = 10000;
				packline.JL_HarmonisedCode = "12345678";

				shipmentForm.InvokeSaveInternal_ForTest();
			}

			Enterprise.Freight.Forwarding.Logging.PerformanceLogger.Flush();
			Enterprise.Freight.Forwarding.Logging.PerformanceLogger.Shutdown();

			// Assert
			var logDir = Enterprise.Freight.Forwarding.Logging.PerformanceLogger.LogFileDir;
			AssertEquals(true, Directory.Exists(logDir));
			var logFiles = Directory.GetFiles(logDir, "*.log");
			AssertNotNull(logFiles);
			AssertEquals(1, logFiles.Length);

			var content = File.ReadAllText(logFiles[0]);
			var lines = content.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			AssertNotNull(lines);
			AssertEquals(3, lines.Length);

			// first line was Load - skip assertion

			// second line Save - Start
			var performanceInfo = JsonSerializer.Deserialize<PerformanceInfo>(lines[1]);
			AssertNotNull(performanceInfo);
			AssertNotEquals(DateTime.MinValue, performanceInfo.Timestamp);
			AssertEquals(objectType, performanceInfo.Object);
			AssertEquals(pk, performanceInfo.Pk);
			AssertEquals(shipment.OuterPackLines.Count, performanceInfo.Records);
			AssertEquals(0, performanceInfo.Duration);
			AssertEquals("Save", performanceInfo.Op);
			AssertEquals("start", performanceInfo.Message);

			// third line Save - Completed
			performanceInfo = JsonSerializer.Deserialize<PerformanceInfo>(lines[2]);
			AssertNotNull(performanceInfo);
			AssertNotNull(performanceInfo);
			AssertNotEquals(DateTime.MinValue, performanceInfo.Timestamp);
			AssertEquals(objectType, performanceInfo.Object);
			AssertEquals(pk, performanceInfo.Pk);
			AssertEquals(shipment.OuterPackLines.Count, performanceInfo.Records);
			AssertGreaterThan(performanceInfo.Duration, 0);
			AssertEquals("Save", performanceInfo.Op);
			AssertEquals("completed", performanceInfo.Message);
		}
#endif

		#endregion

		#region Export to XML

		[TestDate(2015, 10, 21)]
		public void TestExportNativeXMLAfterXMLLightWeight()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(1).ToDateTime();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				FindActionMenuItem(form);
				var actionsMenu = form.ActionsMenuItem.MenuItems;
				var lightWeightIndex = actionsMenu.FindByText(ExportXmlMenuItemHelper.LightWeightMenuItemText).Index;
				var nativeExportIndex = actionsMenu.FindByText(ExportXmlMenuItemHelper.NativeMenuItemText).Index;
				AssertEquals("Native XML export should be after lightweight XML export option", (lightWeightIndex + 1), nativeExportIndex);
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		[TestDate(2015, 10, 21)]
		public void TestExportNativeXML_EmptyXMLLightWeight_HasNoIterfaceConnector()
		{
			var empty = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, empty);

			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(1).ToDateTime();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				FindActionMenuItem(form);
				var actionsMenu = form.ActionsMenuItem.MenuItems;
				AssertNull(actionsMenu.FindByText(ExportXmlMenuItemHelper.LightWeightMenuItemText));
				var nativeExportIndex = actionsMenu.FindByText(ExportXmlMenuItemHelper.NativeMenuItemText).Index;
				Assert(nativeExportIndex > 0);
			}
		}

		[TestDate(2015, 10, 21)]
		[RequiresSTA]
		public void TestStoreAsALPOMenuItemVisibility()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				FindActionMenuItem(form).OnPopup(EventArgs.Empty);

				AssertALPOMenus(form, false);
			}

			using (var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				using (var form = new ShipmentFormForTest(shipment))
				{
					form.Show();
					FindActionMenuItem(form).OnPopup(EventArgs.Empty);

					AssertALPOMenus(form, true);
				}
			}

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "DEHAM";

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				FindActionMenuItem(form).OnPopup(EventArgs.Empty);

				AssertALPOMenus(form, true);
			}

			transport.JW_RL_NKLoadPort = "";

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				FindActionMenuItem(form).OnPopup(EventArgs.Empty);

				AssertALPOMenus(form, false);
			}

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEBRE";

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				FindActionMenuItem(form).OnPopup(EventArgs.Empty);

				AssertALPOMenus(form, true);
			}
		}

		[TestDate(2026, 01, 01)]
		[RequiresSTA]
		public void TestStoreAsALPOMenuItemVisibility_Invisible()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				FindActionMenuItem(form).OnPopup(EventArgs.Empty);

				AssertALPOMenus(form, false);
			}

			using (var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				using (var form = new ShipmentFormForTest(shipment))
				{
					form.Show();
					FindActionMenuItem(form).OnPopup(EventArgs.Empty);

					AssertALPOMenus(form, false);
				}
			}
		}

		[TestTimeZoneUNLOCO("USERI")]
		[TestDate(2025, 12, 31, 23, 00, 00, 0)]
		[RequiresSTA]
		public void TestStoreAsALPOMenuItemVisibility_InvisableForTimeZoneLaterThanDEHAM()
		{
			TestDateAttribute.UseUNLOCO = true;
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "DEHAM";

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				FindActionMenuItem(form).OnPopup(EventArgs.Empty);

				AssertEquals(true, ZDateTime.Now < new ZDate(2026, 01, 01));
				AssertALPOMenus(form, false);
			}
		}

		void AssertALPOMenus(ShipmentFormForTest form, bool expectingALPO)
		{
			AssertEquals(expectingALPO, form.ActionsMenuItem.MenuItems.FindByText("Store as ALPO") != null);
		}

		[TestDate(2025, 5, 16)]
		[RequiresSTA]
		public void TestStoreAsALPODecommissionWarningDialogVisibility()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "DEHAM";

			using (var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				using (var form = new ShipmentFormForTest(shipment))
				{
					form.Show();
					FindActionMenuItem(form).OnPopup(EventArgs.Empty);
					var menu = form.ActionsMenuItem.MenuItems.FindByText("Store as ALPO");

					AssertNotNull(menu);
					menu.PerformClick();

					AssertEquals(typeof(ZMessageBoxWithCheckbox), ZFormModaliser.LastFormShownDialogForTest.GetType());
					var messagebox = (ZMessageBoxWithCheckbox)ZFormModaliser.LastFormShownDialogForTest;
					AssertEquals("You are attempting to run the legacy ALPO Interface, which will be decommissioned by 31 December 2025. To connect to the new ALPO Interface, please raise a CR9 eRequest with WiseTech and refer to the Update Note 'ALPO Port Order Integration'.", messagebox.MessageMultilingual);
					AssertEquals("https://wisetechacademy.com/search?quickstart=daf4190a-ea3b-4611-82ef-e506a36344b5", messagebox.Link);
				}
			}
		}

		public void TestExportUsesForwardingAdapter()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			JobHeader.Loader jobLoader = new JobHeader.Loader(shipment);
			JobHeader job = jobLoader.TryCreate();
			shipment.Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				form.StoreAsShipmentMenuClick(form, EventArgs.Empty);
				AssertEquals(typeof(ForwardingShipmentValueObjectDataAdapter), form.LastExportTransferExporterForTest.Adapter.GetType());
			}
		}

		[RequiresSTA]
		public void TestOnStoreAsShipment_Click_Security()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			JobHeader.Loader jobLoader = new JobHeader.Loader(shipment);
			JobHeader job = jobLoader.TryCreate();
			shipment.Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.MaintainShipmentExportToXml.IsAllowed = true;
				form.StoreAsShipmentMenuClick(form, EventArgs.Empty);
				AssertNotEquals("Error " + Env.Security.MaintainShipmentExportToXml.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.ToString());

				Env.Security.MaintainShipmentExportToXml.IsAllowed = false;
				form.StoreAsShipmentMenuClick(form, EventArgs.Empty);
				AssertEquals("Error " + Env.Security.MaintainShipmentExportToXml.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		[RequiresSTA]
		public void TestOnStoreAsDeclaration_Click()
		{
			Shipment = GetShipmentWithNoErrors();

			SystemDataRegistry.Instance.CustomDeclarationExportDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, EnvProxy.Instance.TempPath);
			var expectedFileName = Shipment.JS_UniqueConsignRef + "_" + ZDateTime.Today.ToString("yyyyMMdd");
			try
			{
				ChildEditableService.SetState(Shipment.Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentFormForTest(Shipment))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.Show();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Env.Security.MaintainShipmentExportToXml.IsAllowed = true;
					form.StoreAsDeclarationMenuClick(form, EventArgs.Empty);
					Assert("File Should have been created", Path.GetFileName(EnvProxy.Instance.TempPath + expectedFileName).StartsWith(expectedFileName));

					Env.Security.MaintainShipmentExportToXml.IsAllowed = false;
					form.StoreAsDeclarationMenuClick(form, EventArgs.Empty);
					AssertEquals("Error " + Env.Security.MaintainShipmentExportToXml.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.ToString());
				}
			}
			finally
			{
				DeleteIfExists(Path.Combine(EnvProxy.Instance.TempPath, expectedFileName + ".xml"));
			}
		}

		[TestDate(2006, 9, 28, 12, 0, 0)]
		public void TestExportToXml_ShipmentReferenceInFileName()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var jobLoader = new JobHeader.Loader(shipment);
			var job = jobLoader.TryCreate();

			shipment.Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			Factory.Save();

			SystemDataRegistry.Instance.ShipmentExportDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, EnvProxy.Instance.TempPath);
			var expectedFileName = shipment.JS_UniqueConsignRef + "_" + ZDateTime.Today.ToString("yyyyMMddhhmmss");
			ZFormModaliser.FileNameToSelectInShowCommonDialog = Path.Combine(EnvProxy.Instance.TempPath, expectedFileName + ".xml");
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			try
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				using (var shipmentForm = new ShipmentFormWithXmlDirector(shipment))
				{
					shipmentForm.Show();
					Application.DoEvents();

					FindActionMenuItem(shipmentForm).OnPopup(EventArgs.Empty);

					var exportToXMLMenu = shipmentForm.ActionsMenuItem.MenuItems.FindByText(ExportXmlMenuItemHelper.VerboseMenuItemText);
					AssertNotNull(exportToXMLMenu);

					var storeAsFileMenu = exportToXMLMenu.MenuItems.FindByText("Store as Shipment XML file");
					AssertNotNull(storeAsFileMenu);
					storeAsFileMenu.PerformClick();
				}

				Assert("File Should have been created", File.Exists(Path.Combine(EnvProxy.Instance.TempPath, expectedFileName + ".xml")));
			}
			finally
			{
				DeleteIfExists(Path.Combine(EnvProxy.Instance.TempPath, expectedFileName + ".xml"));
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		class ShipmentFormWithXmlDirector : ShipmentForm
		{
			public ShipmentFormWithXmlDirector(ForwardingShipment bO)
				: base(bO)
			{
			}

			protected override XmlDataTransferExporter GetNewXmlDataTransferExporter(IValueObjectDataAdapter adapter, bool checkForLicence)
			{
				return new XmlDataTransferExporter(adapter, checkForLicence);
			}

			public new MenuItem ActionsMenuItem
			{
				get { return base.ActionsMenuItem; }
			}
		}

		#endregion

		#region Export Cartage to XML

		void AssertParentMenuContainsMenuOfType(MenuItem parentMenu, Type expectedType)
		{
			var result = false;
			foreach (MenuItem item in parentMenu.MenuItems)
			{
				if (item.GetType() == expectedType)
				{
					result = true;
					break;
				}
			}
			Assert("Should contain menu of type " + expectedType.ToString(), result);
		}

		#endregion

		#region Shipment Number Entry

		#region Shipment Number Entry - Not Allowed

		public void TestSavePath_DisallowManual()
		{
			var old = Env.Registry.AllowManualShipmentEntry;
			Env.Registry.AllowManualShipmentEntry = false;
			try
			{
				var shipment = GetShipmentWithNoErrors();
				shipment.JS_UniqueConsignRef = ZString.Empty;
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();
					form.FireSaveButton();

					AssertNull("Should not have shown the ShipmentNumberEntryForm", ZFormModaliser.LastFormShownDialogForTest);
					AssertSaved(shipment);
				}
			}
			finally
			{
				Env.Registry.AllowManualShipmentEntry = old;
			}
		}

		#endregion

		#region Shipment Number Entry - Blank Number

		public void TestShowShipmentWithChildEditableServiceStates()
		{
			var old = Env.Registry.AllowManualShipmentEntry;
			Env.Registry.AllowManualShipmentEntry = true;
			try
			{
				var shipment = GetShipmentWithNoErrors();
				shipment.JS_UniqueConsignRef = ZString.Empty;

				using (var form = new ShipmentForm(shipment))
				{
					AssertEquals("The state should be set to ChildEditableServiceStates.Shipment before loading or instantiating the Shipment if this Shipment is to be shown on a ShipmentForm", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			}
			finally
			{
				Env.Registry.AllowManualShipmentEntry = old;
			}
		}

		[RequiresSTA]
		public void TestSavePath_AllowManual_EmptyRef()
		{
			var old = Env.Registry.AllowManualShipmentEntry;
			Env.Registry.AllowManualShipmentEntry = true;
			try
			{
				var shipment = GetShipmentWithNoErrors();
				shipment.JS_UniqueConsignRef = ZString.Empty;
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();
					form.FireSaveButton();

					AssertEquals("Should have shown the ShipmentNumberEntryForm", typeof(ShipmentNumberEntryForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
					AssertNotSaved(shipment);
				}
			}
			finally
			{
				Env.Registry.AllowManualShipmentEntry = old;
			}
		}

		#endregion

		#region Shipment Number Entry - OK

		public void TestSavePath_AllowManual_NoProblems()
		{
			var old = Env.Registry.AllowManualShipmentEntry;
			Env.Registry.AllowManualShipmentEntry = true;
			try
			{
				var shipment = GetShipmentWithNoErrors();
				shipment.JS_UniqueConsignRef = "BlahBlahBlah";
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();
					form.FireSaveButton();

					AssertNull("Should not have shown the ShipmentNumberEntryForm", ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals("Should not have displayed an error", false, UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertSaved(shipment);
				}
			}
			finally
			{
				Env.Registry.AllowManualShipmentEntry = old;
			}
		}

		#endregion

		void AssertNotSaved(ForwardingShipment shipment)
		{
			AssertEquals("Should not have saved", true, shipment.HasChanges);
			AssertEquals("Should not be in the Db", false, shipment.IsInDatabase);
		}

		void AssertSaved(ForwardingShipment shipment)
		{
			AssertEquals("Should have saved", false, shipment.HasChanges);
			AssertEquals("Should be in the Db", true, shipment.IsInDatabase);
		}

		#endregion

		#region Pack Line Totals

		[RequiresSTA]
		public void TestSaving_UpdatePackLines_Yes()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var shipment = GetShipmentWithNoErrors();
			shipment.JS_ActualWeight = 50;
			shipment.OuterPackLines[0].JL_ActualWeight = 20;
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.FireSaveButton();

				AssertEquals("Should have displayed a dialog", "Question Total packs, weight and volume do not match the shipment total. Would you like to update the shipment to match the packline totals?", UnitTestUserNotification.Instance.PreviousMessages[1].ToString());
				AssertEquals("Shipment.JS_ActualWeight should have updated", 20m, shipment.JS_ActualWeight);
			}
		}

		public void TestSaving_UpdatePackLines_No()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var shipment = GetShipmentWithNoErrors();
			shipment.JS_ActualWeight = 50;
			shipment.OuterPackLines[0].JL_ActualWeight = 20;
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.FireSaveButton();

				AssertEquals("Should have displayed a dialog", "Question Total packs, weight and volume do not match the shipment total. Would you like to update the shipment to match the packline totals?", UnitTestUserNotification.Instance.PreviousMessages[1].ToString());
				AssertEquals("Shipment.JS_ActualWeight should not have updated", 50m, shipment.JS_ActualWeight);
			}
		}

		[RequiresSTA]
		public void TestSaving_UpdatePackLines_No_OuterPackLines()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var shipment = GetShipmentWithNoErrors();
			var packLine1 = shipment.OuterPackLines.AddNew();

			packLine1.JL_PackageCount = 5;
			packLine1.JL_ActualWeight = 50;
			packLine1.JL_ActualVolume = 50;

			Factory.Save();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_PackageCount = 2;
				packLine2.JL_ActualWeight = 20;
				packLine2.JL_ActualVolume = 20;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.FireSaveButton();

				packLine1.JL_PackageCount = 6;
				packLine1.JL_ActualWeight = 60;
				packLine1.JL_ActualVolume = 60;

				AssertEquals("Should have displayed a dialog", "Question Total packs, weight and volume do not match the shipment total. Would you like to update the shipment to match the packline totals?", UnitTestUserNotification.Instance.PreviousMessages[0].ToString());
				AssertEquals("Shipment.TotalOuterPacks has updated", 8, shipment.TotalOuterPacks);
				AssertEquals("Shipment.TotalOuterPacksWeight has updated", 80m, shipment.TotalOuterPacksWeight);
				AssertEquals("Shipment.TotalOuterPacksVolume has updated", 80m, shipment.TotalOuterPacksVolume);
			}
		}

		public void TestSaving_UpdatePackLines_No_InnerPackLines()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var shipment = GetShipmentWithNoErrors();
			var packLine1 = shipment.InnerPackLines.AddNew();

			packLine1.JL_PackageCount = 5;
			packLine1.JL_ActualWeight = 50;
			packLine1.JL_ActualVolume = 50;

			Factory.Save();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				var packLine2 = shipment.InnerPackLines.AddNew();
				packLine2.JL_PackageCount = 2;
				packLine2.JL_ActualWeight = 20;
				packLine2.JL_ActualVolume = 20;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.FireSaveButton();

				packLine1.JL_PackageCount = 6;
				packLine1.JL_ActualWeight = 60;
				packLine1.JL_ActualVolume = 60;

				AssertEquals("Shipment.TotalInnerPackLinePackages has updated", 8, shipment.TotalInnerPackLinePackages);
				AssertEquals("Shipment.TotalInnerPackLineWeight has updated", 80m, shipment.TotalInnerPackLineWeight);
				AssertEquals("Shipment.TotalInnerPackLineVolume has updated", 80m, shipment.TotalInnerPackLineVolume);
			}
		}

		[RequiresSTA]
		public void TestSaving_UpdateHVLVShipmentItems_Yes()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var shipment = GetShipmentWithNoErrors();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TotalPackageCount = 0;

			var header = Factory.LoadTop1<IHVLVConsignmentHeader>(new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipment.PK)) as BusinessObject;

			var consignment = Factory.New<IHVLVConsignment>() as BusinessObject;
			consignment[HVLVConsignmentSchema.HVC_HCH_Header.Name] = header.PK;

			var item = Factory.New<IHVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.FireSaveButton();

				AssertEquals("Should have displayed a dialog", "Total items, weight and volume do not match the shipment total. Would you like to update the shipment to match the item totals?", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
				AssertEquals("Shipment.JS_TotalPackageCount should have updated", 1, shipment.JS_TotalPackageCount);
			}
		}

		public void TestSaving_UpdateHVLVShipmentItems_No()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var shipment = GetShipmentWithNoErrors();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TotalPackageCount = 0;

			var header = Factory.LoadTop1<IHVLVConsignmentHeader>(new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipment.PK)) as BusinessObject;

			var consignment = Factory.New<IHVLVConsignment>() as BusinessObject;
			consignment[HVLVConsignmentSchema.HVC_HCH_Header.Name] = header.PK;

			var item = Factory.New<IHVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.FireSaveButton();

				AssertEquals("Should have displayed a dialog", "Total items, weight and volume do not match the shipment total. Would you like to update the shipment to match the item totals?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("Shipment.JS_TotalPackageCount should have updated", 0, shipment.JS_TotalPackageCount);
			}
		}

		[RequiresSTA]
		public void TestSaving_UpdateHVLVShipmentActualWeight()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_ActualWeight = 1;
			shipment.JS_UnitOfWeight = Weight.Kilograms;
			shipment.JS_TotalPackageCount = 1;

			var header = Factory.LoadTop1<IHVLVConsignmentHeader>(new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipment.PK)) as BusinessObject;

			var consignment = Factory.New<IHVLVConsignment>() as BusinessObject;
			consignment[HVLVConsignmentSchema.HVC_HCH_Header.Name] = header.PK;
			consignment[HVLVConsignmentSchema.HVC_WeightUQ.Name] = Weight.Grams;

			var item = Factory.New<IHVLVItem>() as BusinessObject;
			item[HVLVItemSchema.HVI_HVC_Consignment.Name] = consignment.PK;
			item[HVLVItemSchema.HVI_JS_LoadedOnShipment.Name] = shipment.PK;
			item[HVLVItemSchema.HVI_ActualWeight.Name] = 3000;

			Factory.Save();

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.FireSaveButton();

				AssertEquals("Should have displayed a dialog",
					"Total items, weight and volume do not match the shipment total. Would you like to update the shipment to match the item totals?",
					UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("Shipment.JS_ActualWeight should have updated", 3m, shipment.JS_ActualWeight);
			}
		}

		[RequiresSTA]
		public void TestSaving_UpdateHVLVShipmentActualWeight_WithDifferentMethod()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_ActualWeight = 1;
			shipment.JS_UnitOfWeight = Weight.Kilograms;
			shipment.JS_TotalPackageCount = 1;

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 4;
			packLine1.JL_ActualWeight = 50;
			packLine1.JL_ActualVolume = 60;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 5;
			packLine2.JL_ActualWeight = 60;
			packLine2.JL_ActualVolume = 70;

			var header = Factory.LoadTop1<IHVLVConsignmentHeader>(new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipment.PK)) as BusinessObject;

			var consignment = Factory.New<IHVLVConsignment>() as BusinessObject;
			consignment[HVLVConsignmentSchema.HVC_HCH_Header.Name] = header.PK;
			consignment[HVLVConsignmentSchema.HVC_WeightUQ.Name] = Weight.Grams;

			var item = PrepareHVLVItem(shipment, consignment);

			Factory.Save();

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			AssertShipmentFormPopupAndActualWeight("A dialog should exist when the registry is ShowWarningFromConsignmentsDetails.", Core.Constants.ShipmentWeightUpdateOptions.Code.ShowWarningFromConsignmentsDetails, 3m, "Total items, weight and volume do not match the shipment total. Would you like to update the shipment to match the item totals?");

			item[HVLVItemSchema.HVI_ActualWeight.Name] = 6000;
			AssertShipmentFormPopupAndActualWeight("A dialog should not exist when the registry is ShowWarningFromConsignmentsDetails.", Core.Constants.ShipmentWeightUpdateOptions.Code.AlwaysUpdateFromConsignmentsDetails, 6m);

			item[HVLVItemSchema.HVI_ActualWeight.Name] = 7000;
			AssertShipmentFormPopupAndActualWeight("A dialog should not exist when the registry is DoNotUpdate.", Core.Constants.ShipmentWeightUpdateOptions.Code.DoNotUpdate, 6m);

			AssertShipmentFormPopupAndActualWeight("A dialog should exist when the registry is ShowWarningFromPackingDetails and the InterPacksTotals has not changed.", Core.Constants.ShipmentWeightUpdateOptions.Code.ShowWarningFromPackingDetails, 111m, "Total items, weight and volume do not match the shipment total. Would you like to update the shipment to match the packing details?");

			var item2 = PrepareHVLVItem(shipment, consignment);
			AssertShipmentFormPopupAndActualWeight("A dialog should exist when the registry is ShowWarningFromPackingDetails and the OuterPacksTotals has not changed.", Core.Constants.ShipmentWeightUpdateOptions.Code.ShowWarningFromPackingDetails, 111m, "Total items do not match shipment inners, would you like to update shipment inners to match total items?");

			var item3 = PrepareHVLVItem(shipment, consignment);
			packLine2.JL_ActualWeight = 65;
			AssertShipmentFormPopupAndActualWeight("A dialog should exist when the registry is ShowWarningFromPackingDetails ", Core.Constants.ShipmentWeightUpdateOptions.Code.ShowWarningFromPackingDetails, 116m, "Total items, weight and volume do not match the shipment total. Would you like to update the shipment to match the packing details and total items?");

			packLine2.JL_ActualWeight = 70;
			AssertShipmentFormPopupAndActualWeight("A dialog should not exist when the registry is AlwaysUpdateFromPackingDetails.", Core.Constants.ShipmentWeightUpdateOptions.Code.AlwaysUpdateFromPackingDetails, 121m);

			shipment.JS_ShipmentType = ShipmentTypes.ShippersConsolLead;
			item[HVLVItemSchema.HVI_ActualWeight.Name] = 8000;
			packLine2.JL_ActualWeight = 80;
			AssertShipmentFormPopupAndActualWeight("A dialog should not exist when the shipment type is not HVL", Core.Constants.ShipmentWeightUpdateOptions.Code.ShowWarningFromConsignmentsDetails, 131m, "Total packs, weight and volume do not match the shipment total. Would you like to update the shipment to match the packline totals?");

			void AssertShipmentFormPopupAndActualWeight(string assertionCondition, string updateMethod, decimal expectedWeight, string expectedDialogMsg = null)
			{
				HVLVDataRegistry.Instance.HVLVShipmentWeightUpdateMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, updateMethod);
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.FireSaveButton();

					if (Core.Constants.ShipmentWeightUpdateOptions.IsShowWarning(updateMethod))
					{
						AssertEquals(assertionCondition, expectedDialogMsg, UnitTestUserNotification.Instance.PreviousMessages[1].Text);
					}
					else
					{
						AssertNull(assertionCondition, UnitTestUserNotification.Instance.PreviousMessages[1].Text);
					}
					AssertEquals(expectedWeight, shipment.JS_ActualWeight);
				}
			}

			BusinessObject PrepareHVLVItem(ForwardingShipment shipment, BusinessObject consignment)
			{
				var item = Factory.New<IHVLVItem>() as BusinessObject;
				item[HVLVItemSchema.HVI_HVC_Consignment.Name] = consignment.PK;
				item[HVLVItemSchema.HVI_JS_LoadedOnShipment.Name] = shipment.PK;
				item[HVLVItemSchema.HVI_ActualWeight.Name] = 3000;

				return item;
			}
		}

		public void TestSaving_UpdateHVLVShipmentActualVolume()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_ActualVolume = 1;
			shipment.JS_UnitOfVolume = Volume.CubicMetres;
			shipment.JS_TotalPackageCount = 1;

			var header = Factory.LoadTop1<IHVLVConsignmentHeader>(new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipment.PK)) as BusinessObject;

			var consignment = Factory.New<IHVLVConsignment>() as BusinessObject;
			consignment[HVLVConsignmentSchema.HVC_HCH_Header.Name] = header.PK;
			consignment[HVLVConsignmentSchema.HVC_VolumeUQ.Name] = Volume.CubicCentimeters;

			var item = Factory.New<IHVLVItem>() as BusinessObject;
			item[HVLVItemSchema.HVI_HVC_Consignment.Name] = consignment.PK;
			item[HVLVItemSchema.HVI_JS_LoadedOnShipment.Name] = shipment.PK;
			item[HVLVItemSchema.HVI_ActualVolume.Name] = 300000;

			Factory.Save();

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.FireSaveButton();

				AssertEquals("Should have displayed a dialog",
					"Total items, weight and volume do not match the shipment total. Would you like to update the shipment to match the item totals?",
					UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("Shipment.JS_ActualVolume should have updated", 0.3m, shipment.JS_ActualVolume);
			}
		}

		#endregion

		#region Sub HVL Shipment Inspection Type Update

		[RequiresSTA]
		public void TestUpdateSubHVLShipmentInspectionType_Yes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var shipment = GetShipmentWithNoErrors();
				shipment.JS_ShipmentType = "HVM";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "CNSHA";

				var subShipment = shipment.CoLoadShipments.AddNew();
				subShipment.JS_InspectionTypeCode = "UNK";
				subShipment.JS_ShipmentType = "HVL";

				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					shipment.JS_InspectionTypeCode = "CMD";

					AssertEquals("Should have displayed a dialog", "Question Do you want to apply this Inspection Type to all sub HVL shipments?", UnitTestUserNotification.Instance.PreviousMessages[0].ToString());
					AssertEquals("Sub HVL Shipment Inspection Type is updated", "CMD", subShipment.JS_InspectionTypeCode);
				}
			}
		}

		public void TestUpdateSubHVLShipmentInspectionType_No()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var shipment = GetShipmentWithNoErrors();
				shipment.JS_ShipmentType = "HVM";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "CNSHA";

				var subShipment = shipment.CoLoadShipments.AddNew();
				subShipment.JS_InspectionTypeCode = "UNK";
				subShipment.JS_ShipmentType = "HVL";

				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

					shipment.JS_InspectionTypeCode = "CMD";

					AssertEquals("Should have displayed a dialog", "Question Do you want to apply this Inspection Type to all sub HVL shipments?", UnitTestUserNotification.Instance.PreviousMessages[0].ToString());
					AssertEquals("Sub HVL Shipment Inspection Type is not updated", "UNK", subShipment.JS_InspectionTypeCode);
				}
			}
		}

		public void TestUpdateSubHVLShipmentInspectionType_NoSubHVLShipments()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var shipment = GetShipmentWithNoErrors();
				shipment.JS_ShipmentType = "HVM";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "CNSHA";

				var subShipment = shipment.CoLoadShipments.AddNew();
				subShipment.JS_InspectionTypeCode = "UNK";
				subShipment.JS_ShipmentType = "SDT";

				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					shipment.JS_InspectionTypeCode = "CMD";

					AssertNull("Should not display a dialog", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
					AssertEquals("Sub Shipment Inspection Type is not updated", "UNK", subShipment.JS_InspectionTypeCode);
				}
			}
		}

		public void TestUpdateSubHVLShipmentInspectionType_NonHVMShipment()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var shipment = GetShipmentWithNoErrors();
				shipment.JS_ShipmentType = "SDT";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "CNSHA";

				var subShipment = shipment.CoLoadShipments.AddNew();
				subShipment.JS_InspectionTypeCode = "UNK";
				subShipment.JS_ShipmentType = "HVL";

				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					shipment.JS_InspectionTypeCode = "CMD";

					AssertNull("Should not display a dialog", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
					AssertEquals("Sub HVL Shipment Inspection Type is not updated", "UNK", subShipment.JS_InspectionTypeCode);
				}
			}
		}

		#endregion

		#region Pack Line Additional Inspection Type

		[RequiresSTA]
		public void TestUpdatePackLineAdditionalInspectionType_Yes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_IsHighRisk = true;
				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_IsHighRisk = true;
				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_IsHighRisk = true;
				AssertEquals(FreightRegistry.AviationSecurity_Unknown_Code, packLine1.JL_AdditionalInspectionTypeCode);
				AssertEquals(FreightRegistry.AviationSecurity_Unknown_Code, packLine2.JL_AdditionalInspectionTypeCode);
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					shipment.GetReasonChangingSecurityAdditionalInspectionStatusEventHandler -= MessagePopupHelper.PromptReasonForChangingSecurityAdditionalInspectionStatusEventHandler;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					shipment.JS_AdditionalInspectionTypeCode = AdditionalScreeningMethods.Codes.VisualCheck;

					AssertEquals("Should have displayed a dialog", "Question Do you want to apply this Inspection Type to all packlines on this Shipment?", UnitTestUserNotification.Instance.PreviousMessages[0].ToString());
					AssertEquals("Pack1 is updated", AdditionalScreeningMethods.Codes.VisualCheck, packLine1.JL_AdditionalInspectionTypeCode);
					AssertEquals("Pack2 is updated", AdditionalScreeningMethods.Codes.VisualCheck, packLine2.JL_AdditionalInspectionTypeCode);
					shipment.Validation.ValidateJS_AdditionalInspectionTypeCode();
					AssertNoWarnings(shipment.JS_AdditionalInspectionTypeCodeInfo);
				}
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_IsHighRisk = true;
				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_IsHighRisk = true;
				Assert(packLine1.JL_AdditionalInspectionTypeCodeInfo.ReadOnly);
				AssertEquals(FreightRegistry.AviationSecurity_Unknown_Code, packLine1.JL_AdditionalInspectionTypeCode);
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					shipment.GetReasonChangingSecurityAdditionalInspectionStatusEventHandler -= MessagePopupHelper.PromptReasonForChangingSecurityAdditionalInspectionStatusEventHandler;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					shipment.JS_AdditionalInspectionTypeCode = AdditionalScreeningMethods.Codes.VisualCheck;

					AssertEquals("Because JL_AdditionalInspectionTypeCode is read-only, there will be no dialog appearing here", "None ", UnitTestUserNotification.Instance.PreviousMessages[0].ToString());
				}
			}
		}

		public void TestUpdatePackLineAdditionalInspectionType_No()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_IsHighRisk = true;
				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_IsHighRisk = true;
				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_IsHighRisk = true;
				AssertEquals(FreightRegistry.AviationSecurity_Unknown_Code, packLine1.JL_AdditionalInspectionTypeCode);
				AssertEquals(FreightRegistry.AviationSecurity_Unknown_Code, packLine2.JL_AdditionalInspectionTypeCode);
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					shipment.GetReasonChangingSecurityAdditionalInspectionStatusEventHandler -= MessagePopupHelper.PromptReasonForChangingSecurityAdditionalInspectionStatusEventHandler;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

					shipment.JS_AdditionalInspectionTypeCode = AdditionalScreeningMethods.Codes.VisualCheck;

					AssertEquals("Should have displayed a dialog", "Question Do you want to apply this Inspection Type to all packlines on this Shipment?", UnitTestUserNotification.Instance.PreviousMessages[0].ToString());
					AssertEquals("Pack1 is not updated", FreightRegistry.AviationSecurity_Unknown_Code, packLine1.JL_AdditionalInspectionTypeCode);
					AssertEquals("Pack2 is not updated", FreightRegistry.AviationSecurity_Unknown_Code, packLine2.JL_AdditionalInspectionTypeCode);
					shipment.Validation.ValidateJS_AdditionalInspectionTypeCode();
					AssertHasWarning(shipment.JS_AdditionalInspectionTypeCodeInfo, "Shipment > Basic Registration > Additional Inspection will be overridden once all high-risk packlines have Additional Inspection.");
					packLine1.JL_AdditionalInspectionTypeCode = AdditionalScreeningMethods.Codes.PhysicalInspectionAndHandSearch;
					packLine2.JL_AdditionalInspectionTypeCode = AdditionalScreeningMethods.Codes.XRayEquipment;
					AssertEquals("Shipment is updated", BaseJobShipmentLookups.InspectionType_Screened, shipment.JS_AdditionalInspectionTypeCode);
					packLine2.JL_AdditionalInspectionTypeCode = AdditionalScreeningMethods.Codes.PhysicalInspectionAndHandSearch;
					AssertEquals("Shipment is updated", AdditionalScreeningMethods.Codes.PhysicalInspectionAndHandSearch, shipment.JS_AdditionalInspectionTypeCode);
					shipment.Validation.ValidateJS_AdditionalInspectionTypeCode();
					AssertNoWarning(shipment.JS_AdditionalInspectionTypeCodeInfo, "Shipment > Basic Registration > Additional Inspection will be overridden once all high-risk packlines have Additional Inspection.");
				}
			}
		}
		#endregion

		#region Pack Line Is High Risk

		public void TestUpdatePackLineIsHighRisk_Yes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "USLAX";
				var packLine1 = shipment.OuterPackLines.AddNew();
				var packLine2 = shipment.OuterPackLines.AddNew();
				AssertEquals(false, packLine1.JL_IsHighRisk);
				AssertEquals(false, packLine2.JL_IsHighRisk);
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					shipment.JS_IsHighRisk = true;

					AssertEquals("Should have displayed a dialog", "Question Do you want to apply ‘Is High Risk’ to all packlines on this Shipment?", UnitTestUserNotification.Instance.PreviousMessages[0].ToString());
					AssertEquals("Pack1 is updated", true, packLine1.JL_IsHighRisk);
					AssertEquals("Pack2 is updated", true, packLine2.JL_IsHighRisk);
					shipment.Validation.ValidateJS_IsHighRisk();
					AssertNoWarnings(shipment.JS_IsHighRiskInfo);
				}
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";
				var packLine = shipment.OuterPackLines.AddNew();
				AssertEquals(false, packLine.JL_IsHighRisk);
				Assert(packLine.JL_IsHighRiskInfo.ReadOnly);
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					shipment.JS_IsHighRisk = true;
					AssertEquals("Because JL_IsHighRisk is read-only, there will be no dialog appearing here", "None ", UnitTestUserNotification.Instance.PreviousMessages[0].ToString());
				}
			}
		}

		public void TestUpdatePackLineIsHighRisk_No()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "USLAX";
				var packLine1 = shipment.OuterPackLines.AddNew();
				var packLine2 = shipment.OuterPackLines.AddNew();
				AssertEquals(false, packLine1.JL_IsHighRisk);
				AssertEquals(false, packLine2.JL_IsHighRisk);
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

					shipment.JS_IsHighRisk = true;

					AssertEquals("Should have displayed a dialog", "Question Do you want to apply ‘Is High Risk’ to all packlines on this Shipment?", UnitTestUserNotification.Instance.PreviousMessages[0].ToString());
					AssertEquals("Pack1 is not updated", false, packLine1.JL_IsHighRisk);
					AssertEquals("Pack2 is not updated", false, packLine2.JL_IsHighRisk);

					shipment.Validation.ValidateJS_IsHighRisk();
					AssertHasWarning(shipment.JS_IsHighRiskInfo, "Shipment has been flagged as ‘High Risk’ but packline is not.");
					shipment.JS_IsHighRisk = false;
					packLine1.JL_IsHighRisk = true;
					shipment.Validation.ValidateJS_IsHighRisk();
					AssertEquals(true, shipment.JS_IsHighRisk);
					AssertNoWarning(shipment.JS_IsHighRiskInfo, "Shipment has been flagged as ‘High Risk’ but packline is not.");
				}
			}
		}

		#endregion

		#region Pack Line Inspection Type Code

		public void TestUpdatePackLineInspectionTypeCode_Yes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var shipment = GetShipmentWithNoErrors();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.OuterPackLines[0].JL_InspectionTypeCode = "UNK";
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					shipment.JS_InspectionTypeCode = "MAI";

					AssertEquals("Should have displayed a dialog", "Question Do you want to apply this Inspection Type to all Pack Lines on this Shipment?", UnitTestUserNotification.Instance.PreviousMessages[0].ToString());
					AssertEquals("Pack is updated", "MAI", shipment.OuterPackLines[0].JL_InspectionTypeCode);
				}
			}
		}

		public void TestUpdatePackLineInspectionTypeCode_No()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var shipment = GetShipmentWithNoErrors();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.OuterPackLines[0].JL_InspectionTypeCode = "UNK";
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

					shipment.JS_InspectionTypeCode = "MAI";

					AssertEquals("Should have displayed a dialog", "Question Do you want to apply this Inspection Type to all Pack Lines on this Shipment?", UnitTestUserNotification.Instance.PreviousMessages[0].ToString());
					AssertEquals("Pack is not updated", "UNK", shipment.OuterPackLines[0].JL_InspectionTypeCode);
				}
			}
		}

		[RequiresSTA]
		public void TestUpdatePackLineInspectionTypeCode_AUExport()
		{
			var shipment = GetShipmentWithNoErrors();

			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "HKHKG";
			shipment.OuterPackLines[0].JL_InspectionTypeCode = "UNK";

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				shipment.JS_InspectionTypeCode = "MAI";

				AssertEquals("Should have displayed a dialog", "Question Do you want to apply this Inspection Type to all Pack Lines on this Shipment?", UnitTestUserNotification.Instance.PreviousMessages[0].ToString());
				AssertEquals("Answer: No - Inspection type should not default", "UNK", shipment.OuterPackLines[0].JL_InspectionTypeCode);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				shipment.JS_InspectionTypeCode = "PHS";

				AssertEquals("Should have displayed a dialog", "Question Do you want to apply this Inspection Type to all Pack Lines on this Shipment?", UnitTestUserNotification.Instance.PreviousMessages[0].ToString());
				AssertEquals("Answer: Yes - Inspection type should default", "PHS", shipment.OuterPackLines[0].JL_InspectionTypeCode);
			}
		}

		[RequiresSTA]
		public void TestRecordInspectionTypeCodeChangeEvent()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.HongKong))
			{
				var shipment = GetShipmentWithNoErrors();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.OuterPackLines[0].JL_InspectionTypeCode = FreightRegistry.AviationSecurity_Unknown_Code;
				shipment.OuterPackLines.RemoveAndDeleteAll();
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					AssertEquals("InspectionTypeCode should be set UNK at first", FreightRegistry.AviationSecurity_Unknown_Code, shipment.JS_InspectionTypeCode);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddUserResponse("test reason");
					shipment.JS_InspectionTypeCode = ExemptionCodes.Codes.NuclearMaterial;
					var msg = UnitTestUserNotification.Instance.LastMessage;
					Factory.Save();
					var secEvent = shipment.Logs.MostRecentLogByEventTime(Events.SecurityModified);

					CombineAssertions(() =>
					{
						Assert("Should not prompt message for change reason input when changing from UNK", msg.WasNone);
						AssertEquals("InspectionTypeCode should be set after changed", ExemptionCodes.Codes.NuclearMaterial, shipment.JS_InspectionTypeCode);
						AssertEquals("new SEC event created", 1, shipment.Logs.Find(x => x.SL_SE_NKEvent == Events.SecurityModified.Code).Count());
						AssertEquals("SEC event Reference", "|NEW=NUC|OLD=UNK", secEvent.SL_Reference);
					});

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddUserResponse("test reason");
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					shipment.JS_InspectionTypeCode = ExemptionCodes.Codes.LifeSavingMaterials;
					msg = UnitTestUserNotification.Instance.LastMessage;
					Factory.Save();

					secEvent = shipment.Logs.MostRecentLogByEventTime(Events.SecurityModified);

					CombineAssertions(() =>
					{
						AssertEquals("Check message caption", "Change Reason", msg.Caption);
						AssertEquals("Check message text", "Enter the reason for changing the Security Inspection Status.\r\nThe reason will be recorded on the SEC-Security Modified event for future reference.", msg.Text);
						AssertEquals("InspectionTypeCode should be set after changed", ExemptionCodes.Codes.LifeSavingMaterials, shipment.JS_InspectionTypeCode);
						AssertEquals("new SEC event created", 2, shipment.Logs.Find(x => x.SL_SE_NKEvent == Events.SecurityModified.Code).Count());
						AssertEquals("SEC event Reference", "|NEW=LFS|OLD=NUC|RES=test reason", secEvent.SL_Reference);
					});

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddUserResponse(null);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					shipment.JS_InspectionTypeCode = ExemptionCodes.Codes.BiomedicalSamples;
					msg = UnitTestUserNotification.Instance.LastMessage;
					Factory.Save();

					CombineAssertions(() =>
					{
						AssertNotNull("Should prompt message for change reason input", msg);
						AssertEquals("Check message caption", "Change Reason", msg.Caption);
						AssertEquals("Check message text", "Enter the reason for changing the Security Inspection Status.\r\nThe reason will be recorded on the SEC-Security Modified event for future reference.", msg.Text);
						AssertEquals("InspectionTypeCode should be not set after cancel", ExemptionCodes.Codes.LifeSavingMaterials, shipment.JS_InspectionTypeCode);
						AssertEquals("No new SEC event is created", 2, shipment.Logs.Find(x => x.SL_SE_NKEvent == Events.SecurityModified.Code).Count());
					});

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddUserResponse("test reason");
					shipment.SetApprovedShipperStatus("", true);
					msg = UnitTestUserNotification.Instance.LastMessage;
					Factory.Save();

					secEvent = shipment.Logs.MostRecentLogByEventTime(Events.SecurityModified);

					CombineAssertions(() =>
					{
						AssertNotNull("Should prompt message for change reason input", msg);
						AssertEquals("InspectionTypeCode should be set after recalculated manually", FreightRegistry.AviationSecurity_Unknown_Code, shipment.JS_InspectionTypeCode);
						AssertEquals("New SEC event is created", 3, shipment.Logs.Find(x => x.SL_SE_NKEvent == Events.SecurityModified.Code).Count());
						AssertEquals("SEC event Reference", "|NEW=UNK|OLD=LFS|RES=test reason", secEvent.SL_Reference);
					});

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddUserResponse("test reason");
					var oldValue = shipment.JS_InspectionTypeCode;
					shipment.JS_InspectionTypeCode = ExemptionCodes.Codes.NuclearMaterial;
					shipment.JS_InspectionTypeCode = ExemptionCodes.Codes.BiomedicalSamples;
					shipment.JS_InspectionTypeCode = ExemptionCodes.Codes.DiplomaticBagsOrDiplomaticMail;
					UnitTestUserNotification.Instance.ClearMessages();
					shipment.JS_InspectionTypeCode = oldValue;
					msg = UnitTestUserNotification.Instance.LastMessage;
					Factory.Save();

					CombineAssertions(() =>
					{
						Assert("Should not prompt message for change reason input", msg.WasNone);
						AssertEquals("InspectionTypeCode should be set the old value after setting the old value finally", FreightRegistry.AviationSecurity_Unknown_Code, shipment.JS_InspectionTypeCode);
						AssertEquals("No new SEC event is created", 3, shipment.Logs.Find(x => x.SL_SE_NKEvent == Events.SecurityModified.Code).Count());
					});
				}
			}
		}

		public void TestNotPromptReasonForChangingSecurityInspectionStatusForUncertifiedUser()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			{
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "AUBNE";
				shipment.JS_InspectionTypeCode = "XRY";

				Factory.Save();

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					shipment.JS_InspectionTypeCode = "PHS";
					var msg = UnitTestUserNotification.Instance.LastMessage;
					Factory.Save();

					CombineAssertions(() =>
					{
						Assert("Should not prompt message for change reason input when user doesn't have the certification", msg.WasNone);
						AssertEquals("InspectionTypeCode should be set after changed", "PHS", shipment.JS_InspectionTypeCode);
					});
				}
			}
		}

		public void TestNotPromptReasonForChangingSecurityAdditionalInspectionStatusForUncertifiedUser()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			{
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "AUBNE";
				shipment.JS_AdditionalInspectionTypeCode = "XRY";

				Factory.Save();

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					shipment.JS_AdditionalInspectionTypeCode = "PHS";
					var msg = UnitTestUserNotification.Instance.LastMessage;
					Factory.Save();

					CombineAssertions(() =>
					{
						Assert("Should not prompt message for change reason input when user doesn't have the certification", msg.WasNone);
						AssertEquals("InspectionTypeCode should be set after changed", "PHS", shipment.JS_AdditionalInspectionTypeCode);
					});
				}
			}
		}

		public void TestTemplateNotPromptReasonForChangingSecurityInspectionStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.HongKong))
			{
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var consignorApproval = consignor.MainAddress.KnownShipperDetails.AddNew();
				consignorApproval.OV_OH_OrgHeader = consignor.PK;
				consignorApproval.OV_EXApprovedOrMajorExporter = "KC";
				consignorApproval.OV_EXApprovalNumber = "1234";
				consignorApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(100);

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.ConsignorPK = consignor.PK;
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_InspectionTypeCode = "APP";

				var templateRecord = Factory.New<StmTemplateRecord>();
				var templateRecordProvider = (ITemplateRecordProvider)shipment;
				templateRecordProvider.TemplateRecord = templateRecord;
				templateRecordProvider.IsTemplateRecord = true;
				using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
				{
					templateRecordProvider.SaveToTemplateRecord();
				}

				Factory.Save();

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					shipment.JS_InspectionTypeCode = "UNK";
					var msg = UnitTestUserNotification.Instance.LastMessage;
					Factory.Save();

					CombineAssertions(() =>
					{
						Assert("Should not prompt message for change reason input when changing to UNK", msg.WasNone);
						AssertEquals("InspectionTypeCode should be set after changed", "UNK", shipment.JS_InspectionTypeCode);
					});
				}
			}
		}

		#endregion

		#region CO2 Emission

		public void TestCalculateCO2EmissionMenuItem_ActionMenuItemClick_EHub()
		{
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(true))
			using (FreightRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Ehub))
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				Factory.Save();
				FindActionMenuItem(form).MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)").PerformClick();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals("Check message caption", "Request sent", lastMessage.Caption);
					AssertEquals("Check message text", "The greenhouse gas emissions calculation has been requested.", lastMessage.Text);
				});

				AssertEquals("Shipment CO2eStatus should be set", CO2eStatusList.Codes.Pending, shipment.GetCO2eStatus());
				AssertEquals("Transport CO2eStatus should be set", CO2eStatusList.Codes.Pending, shipment.TransportsIncludingRelated[0].GetCO2eStatus());
				AssertEquals("Transport CO2eStatus should be set", CO2eStatusList.Codes.Pending, shipment.TransportsIncludingRelated[1].GetCO2eStatus());
				AssertEquals("TotalCO2eForBinding should be set", "Pending", shipment.TotalCO2eForBinding);
				AssertEquals("TotalCO2eForSorting should be cleared", 0m, shipment.TotalCO2eForSorting);
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ActionMenuItemClick_Api()
		{
			// Arrange
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			var client = new Mock<IApiClient>();
			client.Setup(x => x.PostAsync<EmissionResult>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() => CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response));

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(true))
			using (FreightRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Api))
			using (FreightRegistry.Instance.AddEmissionsCalculationLogForShipment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ShipmentForm(shipment))
			using (ObjectFactory.Substitute("HttpClient", client.Object))
			{
				form.Show();
				Factory.Save();

				// Act
				FindActionMenuItem(form).MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)").PerformClick();

				// Assert
				AssertEquals("Shipment CO2eStatus should be set", CO2eStatusList.Codes.Current, shipment.GetCO2eStatus());
				AssertEquals("Transport CO2eStatus should be set", CO2eStatusList.Codes.Current, shipment.TransportsIncludingRelated[0].GetCO2eStatus());
				AssertEquals("Transport CO2eStatus should be set", CO2eStatusList.Codes.Current, shipment.TransportsIncludingRelated[1].GetCO2eStatus());
				AssertEquals("TotalCO2eForBinding should be set", "10,000", shipment.TotalCO2eForBinding);
				AssertEquals("TotalCO2eForSorting should be set", 10000m, shipment.TotalCO2eForSorting);

				(form as ISupportSwitchTabPage).SwitchTabPage("RoutingTabPage");
				var grid = form.GetControl<ZGrid>("TransportsGrid");
				var transport1 = grid.List[0] as Transport;
				var transport2 = grid.List[1] as Transport;
				AssertEquals("CO2ePerTonneInKgForBinding should be set", 8000m, transport1.TotalCO2eForSorting);
				AssertEquals("CO2ePerTonneInKgForBinding should be set", 2000m, transport2.TotalCO2eForSorting);

				var note = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.EmissionsCalculationLog.Description).FirstOrDefault();
				AssertNotNull("Emissions Calculation Log note exists", note);
				AssertContains("Previous CO2e value: 0 kg", note.ST_NoteText);
				AssertContains("New CO2e value: 10000 kg", note.ST_NoteText);
				AssertContains("Calculated manually", note.ST_NoteText);
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ActionMenuItemClick_Api_WithTBs()
		{
			// Arrange
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			var dtbBookingPIC = (ICO2eCalculationSupporter)CO2eTestHelper.CreateTransportBooking(shipment, "PIC", Factory);
			var dtbBookingDLV = (ICO2eCalculationSupporter)CO2eTestHelper.CreateTransportBooking(shipment, "DLV", Factory);

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			var client = new Mock<IApiClient>();
			client.SetupSequence(x => x.PostAsync<EmissionResult>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response))
				.ReturnsAsync(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response_TB))
				.ReturnsAsync(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response_TB));

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(true))
			using (ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest()))
			using (FreightRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Api))
			using (var form = new ShipmentForm(shipment))
			using (ObjectFactory.Substitute("HttpClient", client.Object))
			{
				form.Show();
				Factory.Save();

				// Act
				FindActionMenuItem(form).MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)").PerformClick();

				// Assert
				AssertEquals("Shipment CO2eStatus should be set", CO2eStatusList.Codes.Current, shipment.GetCO2eStatus());
				AssertEquals("Shipment TotalCO2e should be set with TB emissions", 12000m, shipment.GetTotalCO2e());
				AssertEquals("Shipment TotalCO2eForBinding should be set with TB emissions", "12,000", shipment.TotalCO2eForBinding);
				AssertEquals("Shipment TotalCO2eForSorting should be set with TB emissions", 12000m, shipment.TotalCO2eForSorting);
				AssertEquals("Pickup TB CO2eStatus should be set", CO2eStatusList.Codes.Current, dtbBookingPIC.GetCO2eStatus());
				AssertEquals("Pickup TB TotalCO2e should be set", 1000m, dtbBookingPIC.GetTotalCO2e());
				AssertEquals("Delivery TB CO2eStatus should be set", CO2eStatusList.Codes.Current, dtbBookingDLV.GetCO2eStatus());
				AssertEquals("Delivery TB should be set", 1000m, dtbBookingDLV.GetTotalCO2e());
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ActionMenuItemClick_ClearValidation()
		{
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			shipment.SetCO2ePerTonneInKg(5m);
			shipment.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			shipment.SetTotalCO2e(5m);
			shipment.TransportsIncludingRelated[0].SetCO2ePerTonneInKg(2m);
			shipment.TransportsIncludingRelated[0].SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			shipment.TransportsIncludingRelated[1].SetCO2ePerTonneInKg(3m);
			shipment.TransportsIncludingRelated[1].SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(true))
			using (FreightRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Ehub))
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				Factory.Save();
				AssertHasWarning(shipment.TotalCO2eForBindingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);
				AssertHasWarning(shipment.TransportsIncludingRelated[0].TotalCO2eForSortingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);
				AssertHasWarning(shipment.TransportsIncludingRelated[1].TotalCO2eForSortingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);
				FindActionMenuItem(form).MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)").PerformClick();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals("Check message caption", "Request sent", lastMessage.Caption);
					AssertEquals("Check message text", "The greenhouse gas emissions calculation has been requested.", lastMessage.Text);
				});

				AssertEquals("Shipment CO2eStatus should be set", CO2eStatusList.Codes.Pending, shipment.GetCO2eStatus());
				AssertEquals("Transport CO2eStatus should be set", CO2eStatusList.Codes.Pending, shipment.TransportsIncludingRelated[0].GetCO2eStatus());
				AssertEquals("Transport CO2eStatus should be set", CO2eStatusList.Codes.Pending, shipment.TransportsIncludingRelated[1].GetCO2eStatus());
				AssertEquals("TotalCO2eForBinding should be set", "Pending", shipment.TotalCO2eForBinding);
				AssertEquals("TotalCO2eForSorting should be cleared", 0m, shipment.TotalCO2eForSorting);
				AssertNoWarning(shipment.TotalCO2eForBindingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);
				AssertNoWarning(shipment.TransportsIncludingRelated[0].TotalCO2eForSortingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);
				AssertNoWarning(shipment.TransportsIncludingRelated[1].TotalCO2eForSortingInfo, CO2eBusinessTestHelper.CO2eStaleWarning);
			}
		}

		public void TestCalculateCO2EmissionMenuItem_SentSuccessfullyWhenNoWeightAndRequireTEU() => SentSuccessfullyWhenNoWeightUsingTEU(TransportModes.Sea);

		public void TestCalculateCO2EmissionMenuItem_SentSuccessfullyWhenNoWeightAndIncludeTEU() => SentSuccessfullyWhenNoWeightUsingTEU(TransportModes.Road);

		public void SentSuccessfullyWhenNoWeightUsingTEU(string transportMode)
		{
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentRequiringTEU(Factory);
			shipment.JS_TransportMode = transportMode;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 0;

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(true))
			using (FreightRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Ehub))
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				Factory.Save();
				FindActionMenuItem(form).MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)").PerformClick();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals("Check message caption", "Request sent", lastMessage.Caption);
					AssertEquals("Check message text", "The greenhouse gas emissions calculation has been requested.", lastMessage.Text);
				});
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ShowErrorMessage_WhenMandatoryDataMissingOrHavingDuplicateTransportLeg()
		{
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(true))
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				var menuItem = FindActionMenuItem(form).MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)");
				void TestMissingField(Action change, string text)
				{
					change.Invoke();
					Factory.Save();
					menuItem.PerformClick();
					var lastMessage = UnitTestUserNotification.Instance.LastMessage;

					CombineAssertions(() =>
					{
						if (text != ZString.Empty)
						{
							AssertEquals("Check message caption", "Request failed", lastMessage.Caption);
							AssertContains("Check message text", "The greenhouse gas emissions calculation cannot be requested because following mandatory input is missing or invalid:", lastMessage.Text);
							AssertContains("Check mandatory field", text, lastMessage.Text);
						}
						else
						{
							AssertEquals("Check mandatory field", "The greenhouse gas emissions calculation has been requested.", lastMessage.Text);
						}
					});
				}
				TestMissingField(() => shipment.JS_TransportMode = ZString.Empty, "Shipment > Basic Registration > Transport Mode");
				TestMissingField(() => shipment.JS_ActualWeight = 0, "Shipment > Basic Registration > Weight");
				TestMissingField(() =>
				{
					var transport = shipment.Transports.AddNew();
					transport.JW_RL_NKLoadPort = "AUSYD";
					transport.JW_RL_NKDiscPort = "SGSIN";
					transport.JW_TransportMode = "SEA";
					transport.JW_VoyageFlight = "VY1";
				}, "Shipment > Routing > Duplicate Load Ports and Discharge");
				shipment.Transports.RemoveAndDeleteAll();
				TestMissingField(() => shipment.JS_RL_NKOrigin = ZString.Empty, "Shipment > Basic Registration > Origin (or Shipment > Additional Details > Planned Load)");
				TestMissingField(() => shipment.JS_RL_NKDestination = ZString.Empty, "Shipment > Basic Registration > Destination (or Shipment > Additional Details > Planned Discharge)");
				TestMissingField(() =>
				{
					shipment.JS_TransportMode = "SEA";
					shipment.JS_ActualWeight = 1m;
					shipment.JS_UnitOfWeight = "T";
					var transport = shipment.Transports.AddNew();
					transport.JW_RL_NKLoadPort = ZString.Empty;
					transport.JW_RL_NKDiscPort = ZString.Empty;
					transport.JW_TransportMode = "SEA";
					transport.JW_VoyageFlight = "VY1";
				}, "Shipment > Basic Registration > Origin (or Shipment > Routing > Load)");
				TestMissingField(() => { }, "Shipment > Basic Registration > Destination (or Shipment > Routing > Discharge)");
				TestMissingField(() => shipment.JS_RL_NKLoadPort = "AUSYD", "Shipment > Basic Registration > Destination (or Shipment > Routing > Discharge)");
				TestMissingField(() =>
				{
					shipment.JS_RL_NKLoadPort = ZString.Empty;
					shipment.JS_RL_NKDischargePort = "SGSIN";
				}, "Shipment > Basic Registration > Origin (or Shipment > Routing > Load)");
				TestMissingField(() =>
				{
					var transport = shipment.Transports[0];
					transport.JW_RL_NKLoadPort = ZString.Empty;
					transport.JW_RL_NKDiscPort = "AUSYD";
					shipment.JS_RL_NKOrigin = ZString.Empty;
					shipment.JS_RL_NKDestination = ZString.Empty;
					shipment.JS_RL_NKLoadPort = ZString.Empty;
					shipment.JS_RL_NKDischargePort = ZString.Empty;
				}, "Shipment > Basic Registration > Origin (or Shipment > Routing > Load)");
				TestMissingField(() =>
				{
					var transport = shipment.Transports[0];
					transport.JW_RL_NKLoadPort = "AUSYD";
					transport.JW_RL_NKDiscPort = ZString.Empty;
					shipment.JS_RL_NKOrigin = ZString.Empty;
					shipment.JS_RL_NKDestination = ZString.Empty;
					shipment.JS_RL_NKLoadPort = ZString.Empty;
					shipment.JS_RL_NKDischargePort = ZString.Empty;
				}, "Shipment > Basic Registration > Destination (or Shipment > Routing > Discharge)");
				TestMissingField(() =>
				{
					var transport = shipment.Transports[0];
					transport.JW_RL_NKLoadPort = "AUSYD";
					transport.JW_RL_NKDiscPort = "AUSYD";
				}, "Shipment > Origin (or Planned Load or Routing > Load) and Destination (or Planned Discharge or Routing > Discharge) cannot be the same");
				using (FreightRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Ehub))
				{
					TestMissingField(() =>
					{
						var transport1 = shipment.Transports[0];
						transport1.JW_RL_NKLoadPort = ZString.Empty;
						transport1.JW_RL_NKDiscPort = "AUSYD";
						var transport2 = shipment.Transports.AddNew();
						transport2.JW_RL_NKLoadPort = "HKHKG";
						transport2.JW_RL_NKDiscPort = ZString.Empty;
						shipment.JS_RL_NKOrigin = "AUSYD";
						shipment.JS_RL_NKDestination = "HKHKG";
					}, ZString.Empty);
					TestMissingField(() =>
					{
						var transport1 = shipment.Transports[0];
						transport1.JW_RL_NKLoadPort = ZString.Empty;
						transport1.JW_RL_NKDiscPort = "AUSYD";
						var transport2 = shipment.Transports.AddNew();
						transport2.JW_RL_NKLoadPort = "HKHKG";
						transport2.JW_RL_NKDiscPort = ZString.Empty;
						shipment.JS_RL_NKOrigin = ZString.Empty;
						shipment.JS_RL_NKDestination = "HKHKG";
					}, "Shipment > Basic Registration > Origin (or Shipment > Routing > Load)");
				}
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ShowErrorMessage_WhenMandatoryDataMissing_In_Pre_On_CarriageAddresses()
		{
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(true))
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				var menuItem = FindActionMenuItem(form).MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)");

				// ---------- Helper Methods ----------

				OrgHeader CreateOrgHeader(string closestPort, string city, string postCode)
				{
					var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
					orgHeader.OH_RL_NKClosestPort = closestPort;
					orgHeader.MainAddress.City = city;
					orgHeader.MainAddress.Postcode = postCode;
					return orgHeader;
				}

				OrgAddress CreateOrgAddress(ZGuid orgHeaderPK, string code, string city, string countryCode, string postCode)
				{
					var address = Factory.NewWithValidTestData<OrgAddress>();
					address.OA_OH = orgHeaderPK;
					address.OA_Code = code;
					address.City = city;
					address.Postcode = postCode;
					address.OA_RN_NKCountryCode = countryCode;
					return address;
				}

				OrgAddress CreateOrgAddressWithPort(string relatedPort, string city, string postCode)
				{
					var address = Factory.NewWithValidTestData<OrgAddress>();
					address.OA_RL_NKRelatedPortCode = relatedPort;
					address.City = city;
					address.Postcode = postCode;
					return address;
				}

				void TestMissingField(Action change, string text)
				{
					change.Invoke();
					Factory.Save();
					menuItem.PerformClick();
					var lastMessage = UnitTestUserNotification.Instance.LastMessage;

					CombineAssertions(() =>
					{
						if (text != ZString.Empty)
						{
							AssertEquals("Check message caption", "Request failed", lastMessage.Caption);
							AssertContains("Check message text", "The greenhouse gas emissions calculation cannot be requested because following mandatory input is missing or invalid:", lastMessage.Text);
							AssertContains("Check mandatory field", text, lastMessage.Text);
						}
						else
						{
							AssertEquals("Check mandatory field", "The greenhouse gas emissions calculation has been requested.", lastMessage.Text);
						}
					});
				}

				// ---------- Test Methods ----------

				TestMissingField(() =>
				{
					var orgHeader = CreateOrgHeader(string.Empty, "Melbourne", string.Empty);
					var expectedAddress = CreateOrgAddress(orgHeader.PK, "blerbity", "Melbourne", string.Empty, string.Empty);

					shipment.JS_OA_ExportReceivingDepot = expectedAddress.PK;
				}, "Shipment > Pickup > CFS (Ctry/Rgn. field is required with City field)");

				TestMissingField(() =>
				{
					var orgHeader = CreateOrgHeader(string.Empty, string.Empty, "2121");
					var expectedAddress = CreateOrgAddress(orgHeader.PK, "blerbity", string.Empty, string.Empty, "2121");

					shipment.JS_OA_ExportReceivingDepot = expectedAddress.PK;
				}, "Shipment > Pickup > CFS (Ctry/Rgn. field is required with City field)");

				TestMissingField(() =>
				{
					var consignorPickupAddress = CreateOrgAddressWithPort(string.Empty, "Melbourne", string.Empty);
					shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.PK;
				}, "Shipment > Pickup > Pickup From (Ctry/Rgn. field is required with City field)");

				TestMissingField(() =>
				{
					var consignorPickupAddress = CreateOrgAddressWithPort(string.Empty, string.Empty, "2121");
					shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.PK;
				}, "Shipment > Pickup > Pickup From (Ctry/Rgn. field is required with City field)");

				TestMissingField(() =>
				{
					var importOrgHeader = CreateOrgHeader(string.Empty, "Sydney", string.Empty);
					var expectedImportAddress = CreateOrgAddress(importOrgHeader.PK, "blerbity", "Sydney", string.Empty, string.Empty);

					shipment.JS_OA_ImportReleaseDepot = expectedImportAddress.PK;
				}, "Shipment > Delivery > CFS (Ctry/Rgn. field is required with City field)");

				TestMissingField(() =>
				{
					var importOrgHeader = CreateOrgHeader(string.Empty, string.Empty, "2121");
					var expectedImportAddress = CreateOrgAddress(importOrgHeader.PK, "blerbity", string.Empty, string.Empty, "2121");

					shipment.JS_OA_ImportReleaseDepot = expectedImportAddress.PK;
				}, "Shipment > Delivery > CFS (Ctry/Rgn. field is required with City field)");

				TestMissingField(() =>
				{
					var consigneeDeliveryAddress = CreateOrgHeader(string.Empty, "Sydney", string.Empty);
					shipment.ConsigneeDeliveryAddress.OrganisationPK = consigneeDeliveryAddress.PK;
				}, "Shipment > Delivery > Deliver To (Ctry/Rgn. field is required with City field)");

				TestMissingField(() =>
				{
					var consigneeDeliveryAddress = CreateOrgHeader(string.Empty, string.Empty, "2121");
					shipment.ConsigneeDeliveryAddress.OrganisationPK = consigneeDeliveryAddress.PK;
				}, "Shipment > Delivery > Deliver To (Ctry/Rgn. field is required with City field)");
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ShowErrorMessage_WhenShipmentNotSaved()
		{
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(true))
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				FindActionMenuItem(form).MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)").PerformClick();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals("Check message caption", "Request failed", lastMessage.Caption);
					AssertEquals("Check message text", "Please save before calculating greenhouse gas emissions.", lastMessage.Text);
				});
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ShowErrorIfExceptionIsThrown()
		{
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(true))
			using (FreightRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Ehub))
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				Factory.Save();
				BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
				{
					if (factory.NameForDebugging == "CO2e Calculation Request Sender")
					{
						throw new Exception("Hello world");
					}
				});

				AssertNoExceptionThrown(() => FindActionMenuItem(form).MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)").PerformClick());
				var lastMessage = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals("Check message text", "Sending greenhouse gas emissions calculation request failed due to an error. Please try again.", lastMessage.Text);
			}
		}

		#endregion

		#region Delivery Due Date

		[RequiresSTA]
		public void TestRecordDeliveryDateUpdatedEvent_ActionMenuItemClick()
		{
			var newDeliveryDueDate = new ZDateTime(2022, 10, 04, 09, 30, 0);
			DeliveryDueDateCalculationTestHelper.SetupDeliveryDueDateCalculatorManagerMock(newDeliveryDueDate);

			using (FreightRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				Env.Security.MaintainShipmentDeliveryDueDateOverride.IsAllowed = true;
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddUserResponse("calculate DDD from action menu item");

					var calculateDeliveryDueDateMenuItem = FindActionMenuItem(form).MenuItems.FindByText("Calculate Delivery Due Date");
					AssertNotNull("Pre-condition: Calculate Delivery Due Date menu item is available");
					calculateDeliveryDueDateMenuItem.PerformClick();

					var lastMessage = UnitTestUserNotification.Instance.LastMessage;
					Factory.Save();

					var ddeEvent = shipment.Logs.MostRecentLogByEventTime(AutoEvents.DeliveryDateUpdated);

					CombineAssertions(() =>
					{
						AssertEquals("Check message caption", "Change Reason", lastMessage.Caption);
						AssertEquals("Check message text", "Enter the reason for changing the Delivery Due Date.\r\nThe reason will be recorded on the DDE-Delivery Date Updated event for future reference.", lastMessage.Text);
						AssertEquals("JS_DeliveryDueDate should be set after changed", newDeliveryDueDate, shipment.JS_DeliveryDueDate);
						AssertEquals("new DDE event created", 1, shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.DeliveryDateUpdatedCode).Count());
						AssertEquals("DDE event Reference", "|ACT=Manual|NEW=04-Oct-22 09:30|RES=calculate DDD from action menu item|TYP=Original", ddeEvent.SL_Reference);
					});
				}
			}
		}

		[RequiresSTA]
		public void TestNotificationIsShownIfCalculatedDateNotChanged_ActionMenuItemClick()
		{
			// Arrange
			var newDeliveryDueDate = new ZDateTime(2022, 11, 21, 12, 0, 0);
			DeliveryDueDateCalculationTestHelper.SetupDeliveryDueDateCalculatorManagerMock(newDeliveryDueDate);

			using (FreightRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				Env.Security.MaintainShipmentDeliveryDueDateOverride.IsAllowed = true;
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddUserResponse("Calculate DDD");

					shipment.JS_DeliveryDueDate = newDeliveryDueDate;

					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

					// Act
					FindActionMenuItem(form).MenuItems.FindByText("Calculate Delivery Due Date").PerformClick();

					// Assert
					var lastMessage = UnitTestUserNotification.Instance.LastMessage;
					CombineAssertions(() =>
					{
						AssertEquals("Notification caption is not correct", "DDD Calculation Result", lastMessage.Caption);
						AssertEquals("Notification message is not correct", "The Delivery Due Date (DDD) has not been changed based on data entered.", lastMessage.Text);
					});
				}
			}
		}

		[RequiresSTA]
		public void TestRecordDeliveryDateUpdatedEvent_OverrideDeliveryDueDate()
		{
			var newDeliveryDueDate = new ZDateTime(2022, 10, 04, 09, 30, 0);
			DeliveryDueDateCalculationTestHelper.SetupDeliveryDueDateCalculatorManagerMock(newDeliveryDueDate);

			using (FreightRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				Env.Security.MaintainShipmentDeliveryDueDateOverride.IsAllowed = true;
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddUserResponse("calculate DDD from action menu item");

					shipment.JS_DeliveryDueDate = newDeliveryDueDate;

					var lastMessage = UnitTestUserNotification.Instance.LastMessage;
					Factory.Save();

					var ddeEvent = shipment.Logs.MostRecentLogByEventTime(AutoEvents.DeliveryDateUpdated);

					CombineAssertions(() =>
					{
						AssertEquals("Check message caption", "Change Reason", lastMessage.Caption);
						AssertEquals("Check message text", "Enter the reason for changing the Delivery Due Date.\r\nThe reason will be recorded on the DDE-Delivery Date Updated event for future reference.", lastMessage.Text);
						AssertEquals("JS_DeliveryDueDate should be set after changed", newDeliveryDueDate, shipment.JS_DeliveryDueDate);
						AssertEquals("new DDE event created", 1, shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.DeliveryDateUpdatedCode).Count());
						AssertEquals("DDE event Reference", "|ACT=Override|NEW=04-Oct-22 09:30|RES=calculate DDD from action menu item|TYP=Original", ddeEvent.SL_Reference);
					});

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.ClearUserResponses();
					shipment.JS_DeliveryDueDate = newDeliveryDueDate;
					Assert("Should not prompt message for change reason input when new value equals original value", UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
			}
		}

		public void TestErrorIsShownIfUserDoesNotHaveSecurityRights_ActionMenuItemClick()
		{
			// Arrange
			var newDeliveryDueDate = new ZDateTime(2022, 11, 21, 12, 0, 0);
			DeliveryDueDateCalculationTestHelper.SetupDeliveryDueDateCalculatorManagerMock(newDeliveryDueDate);

			using (FreightRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				Env.Security.MaintainShipmentDeliveryDueDateOverride.IsAllowed = false;
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddUserResponse("Calculate DDD");

					shipment.JS_DeliveryDueDate = newDeliveryDueDate;

					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

					// Act
					FindActionMenuItem(form).MenuItems.FindByText("Calculate Delivery Due Date").PerformClick();

					AssertEquals("Show error message when user does nor have security right",
						"You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:" +
						"\r\n\r\nOperate -> Forwarding -> Shipments -> Edit -> Allow override of Delivery Due Date",
						UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#endregion

		#region Master and SubShipment Inspection Type Code Validation

		[RequiresSTA]
		public void TestValidationJS_InspectionTypeCode_EU_WithPackLinesAndMasterShipment()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var otherFactory = new BusinessObjectFactory();

				var shipment1 = otherFactory.New<ForwardingShipment>();
				shipment1.JS_RL_NKOrigin = "DEFRA";
				shipment1.JS_RL_NKDestination = "USMIA";
				shipment1.JS_TransportMode = "AIR";
				shipment1.JS_ShipmentType = "ASM";
				shipment1.JS_InspectionTypeCode = "UNK";

				otherFactory.Save();

				var shipment2 = Factory.New<ForwardingShipment>();
				using (var form = new ShipmentFormForTest(shipment2))
				{
					form.Show();

					shipment2.JS_JS_ColoadMasterShipment = shipment1.PK;
					shipment2.JS_RL_NKOrigin = "DEFRA";
					shipment2.JS_RL_NKDestination = "USMIA";
					shipment2.JS_TransportMode = "AIR";

					var packline1 = shipment2.OuterPackLines.AddNew();
					var packline2 = shipment2.OuterPackLines.AddNew();
					packline1.JL_InspectionTypeCode = "XRY";
					packline1.JL_PackageCount = 1;
					packline2.JL_InspectionTypeCode = "EDD";
					packline2.JL_PackageCount = 1;

					form.ValidateAndSave();

					AssertNoErrors(shipment2.JS_InspectionTypeCodeInfo);
					AssertNoErrors(shipment2.CoLoadMasterShipment.JS_InspectionTypeCodeInfo);

					packline1.JL_InspectionTypeCode = "UNK";
					packline2.JL_InspectionTypeCode = "UNK";
					form.ValidateAndSave();

					AssertNoErrors(shipment2.JS_InspectionTypeCodeInfo);
					AssertNoErrors(shipment2.CoLoadMasterShipment.JS_InspectionTypeCodeInfo);
				}
			}
		}

		public void TestValidationJS_InspectionTypeCode_EU_WithKnownConsignorAndPackLinesAndMasterShipment()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var otherFactory = new BusinessObjectFactory();

				var shipment1 = otherFactory.New<ForwardingShipment>();
				shipment1.JS_RL_NKOrigin = "DEFRA";
				shipment1.JS_RL_NKDestination = "USMIA";
				shipment1.JS_TransportMode = "AIR";
				shipment1.JS_ShipmentType = "ASM";
				shipment1.JS_InspectionTypeCode = "SCR";

				otherFactory.Save();

				var shipment2 = Factory.New<ForwardingShipment>();
				using (var form = new ShipmentFormForTest(shipment2))
				{
					form.Show();

					var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "DEFRA"));
					var addressCountryData = consignor.MainAddress.KnownShipperDetails.AddNew();
					addressCountryData.OV_OH_OrgHeader = consignor.PK;
					addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
					addressCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

					shipment2.JS_JS_ColoadMasterShipment = shipment1.PK;
					shipment2.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
					shipment2.JS_RL_NKOrigin = "DEFRA";
					shipment2.JS_RL_NKDestination = "USMIA";
					shipment2.JS_TransportMode = "AIR";

					AssertEquals("Precondition", "APP", shipment2.JS_InspectionTypeCode);

					var packline1 = shipment2.OuterPackLines.AddNew();
					var packline2 = shipment2.OuterPackLines.AddNew();

					packline1.JL_PackageCount = 1;
					packline2.JL_PackageCount = 1;
					AssertEquals("Precondition", ZString.Empty, packline1.JL_InspectionTypeCode);
					AssertEquals("Precondition", ZString.Empty, packline2.JL_InspectionTypeCode);

					form.ValidateAndSave();
					AssertNoErrors(shipment2.JS_InspectionTypeCodeInfo);
					AssertNoErrors(shipment2.CoLoadMasterShipment.JS_InspectionTypeCodeInfo);
				}
			}
		}

		public void TestValidationJS_InspectionTypeCode_EU_WithSamePacklineInspectionTypesInMasterShipment()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var shipment1 = Factory.New<ForwardingShipment>();
				using (var form1 = new ShipmentFormForTest(shipment1))
				{
					form1.Show();

					shipment1.JS_RL_NKOrigin = "DEFRA";
					shipment1.JS_RL_NKDestination = "USMIA";
					shipment1.JS_TransportMode = "AIR";
					shipment1.JS_ShipmentType = "ASM";
					shipment1.JS_InspectionTypeCode = "XRY";

					form1.ValidateAndSave();
					AssertEquals("Precondition", false, shipment1.IsMasterInSubShipmentContext);

					var shipment2 = Factory.New<ForwardingShipment>();
					shipment2.JS_JS_ColoadMasterShipment = shipment1.PK;
					shipment2.JS_RL_NKOrigin = "DEFRA";
					shipment2.JS_RL_NKDestination = "USMIA";
					shipment2.JS_TransportMode = "AIR";

					var packline1 = shipment2.OuterPackLines.AddNew();
					var packline2 = shipment2.OuterPackLines.AddNew();

					packline1.JL_InspectionTypeCode = "XRY";
					packline1.JL_PackageCount = 1;
					packline2.JL_InspectionTypeCode = "XRY";
					packline2.JL_PackageCount = 1;

					AssertEquals("Precondition", false, shipment1.IsMasterInSubShipmentContext);
					AssertNoErrors(shipment2.JS_InspectionTypeCodeInfo);
					AssertNoErrors(shipment2.CoLoadMasterShipment.JS_InspectionTypeCodeInfo);

					shipment1.JS_InspectionTypeCode = "UNK";
					form1.ValidateAndSave();
					AssertHasErrors(shipment1.JS_InspectionTypeCodeInfo);
					AssertHasError(shipment1.JS_InspectionTypeCodeInfo, "Shipment inspection states UNK, however all packlines are screened by XRY. Change Shipment Inspection to match packline inspection type.");
				}
			}
		}

		#endregion

		#region Harmonized Details

		public void TestCopyHarmonizedDetails()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				FindActionMenuItem(form).OnPopup(EventArgs.Empty);
				FindActionMenuItem(form).MenuItems.FindByText("Copy harmonized details from Booking to Declaration Commercial Invoice").PerformClick();

				AssertEquals("Please create a declaration before copying harmonized details", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var dec = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			dec[JobDeclarationSchema.JE_JS] = shipment.PK;

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				FindActionMenuItem(form).OnPopup(EventArgs.Empty);
				FindActionMenuItem(form).MenuItems.FindByText("Copy harmonized details from Booking to Declaration Commercial Invoice").PerformClick();

				Assert("declaration exists", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		#endregion

		#region RecalculateRelatedParties

		public void TestRecalculateRelatedParties()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "CNSHA";

				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();
					var calculateDeliveryDueDateMenuItem = FindActionMenuItem(form).MenuItems.FindByText("Recalculate Related Parties for logged in Company");
					calculateDeliveryDueDateMenuItem.PerformClick();
					AssertEquals("Should show company does not match message",
						"You cannot Recalculate Related Parties for this company because the company you are logged in does not match Pickup or Delivery directions of this job.",
						UnitTestUserNotification.Instance.LastMessage.Text);

					var consignor = Factory.NewWithValidTestData<OrgHeader>();
					shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

					shipment.JS_RL_NKOrigin = "NASWP";

					var oldValue = Factory.NewWithValidTestData<OrgHeader>();
					shipment.JS_OH_ExportBroker = oldValue.PK;

					var exportBrokerRelatedParty = Factory.NewWithValidTestData<OrgHeader>();
					consignor.AddRelatedParty(exportBrokerRelatedParty.PK,
						RelatedPartyTypeList.Codes.CustomsAgentBroker,
						RelatedPartyDirectionList.Codes.Pickup,
						Constants.TransportModes.All,
						ZString.Empty, GlbCompany.CurrentCompany);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					calculateDeliveryDueDateMenuItem.PerformClick();
					AssertEquals("Should show already has value message",
						"Export Broker has already been entered. Do you wish to update the Export Broker based on your Company Related Party Configuration?",
						UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Export Broker should have its old value", oldValue.PK, shipment.JS_OH_ExportBroker);

					UnitTestUserNotification.Instance.ClearMessages();
					shipment.JS_OH_ExportBroker = ZGuid.Empty;

					calculateDeliveryDueDateMenuItem.PerformClick();
					AssertEquals("Export Broker should be defaulted by Related Party", exportBrokerRelatedParty.PK, shipment.JS_OH_ExportBroker);
					AssertEquals("Should not show already has value message", null, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#endregion

		#region HasChanges

		[RequiresSTA]
		public void TestShipmentNotDirtydByViewingValidData()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();

			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.Add(shipment);
			Factory.Save();

			AssertEquals("Shipment Must not be dirty before opening the form", false, shipment.HasChanges);

			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				Application.DoEvents();
				TabPageNotificationsExposer.ExposeTabPageNotifications(form, form.BusinessEntity);
				AssertEquals("Shipment must not be dirtied by viewing", false, shipment.HasChanges);
			}
		}

		public void TestShipmentNotDirtydByLoadingTemplateRecord()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_TransportMode = "AIR";
			shipment1.JS_HouseBill = "H0000TST1";
			var templateRecord = Factory.New<StmTemplateRecord>();
			var templateRecordProvider1 = (ITemplateRecordProvider)shipment1;
			templateRecordProvider1.TemplateRecord = templateRecord;

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				templateRecordProvider1.SaveToTemplateRecord();
			}

			AssertEquals("Template Record is not in the database yet", false, templateRecord.IsInDatabase);

			Factory.Save();

			AssertEquals("Template Record is already in the database", true, templateRecord.IsInDatabase);

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			var templateRecordProvider2 = (ITemplateRecordProvider)shipment2;
			templateRecordProvider2.LoadFromTemplateRecord(templateRecord);
			AssertEquals("HasChanges should be false after loading the template record", false, shipment2.HasChanges);
			Factory.Save();

			using (var form = new ShipmentForm(shipment2))
			{
				form.Show();
				Application.DoEvents();
				TabPageNotificationsExposer.ExposeTabPageNotifications(form, form.BusinessEntity);
				AssertEquals("Shipment must not be dirtied by viewing", false, shipment2.HasChanges);
			}
		}

		#endregion

		#region Invoicing Plug-In

		[RequiresSTA]
		public void TestInvoicePluginDontDirtyTheShipmentWhenViewed()
		{
			var origin = Factory.New<RefUNLOCO>();
			origin.RL_Code = "UAXX1";

			var destination = Factory.New<RefUNLOCO>();
			destination.RL_Code = "UAXX2";

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();
			AssertEquals("Shipment must not be dirty after creation of a new object", false, shipment.HasChanges);
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_RL_NKOrigin = origin.RL_Code;
			shipment.JS_RL_NKDestination = destination.RL_Code;
			Factory.Save();
			AssertEquals("Shipment Must not be dirty before opening the form", false, shipment.HasChanges);

			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				AssertEquals("Shipment Must not be dirty after opening the form", false, shipment.HasChanges);
				form.PlugIns.SelectPlugInTabPage(ControllerIDs.JobInvoicing);
				Application.DoEvents();
				TabPageNotificationsExposer.ExposeTabPageNotifications(form, form.BusinessEntity);
				Assert("Viewing the Invoicing Plugin should not change the shipment.", !shipment.HasChanges);
			}
		}

		#endregion

		#region View Shipment Tracking

		public void TestViewShipmentTrackingButtonExists()
		{
			TestCase(true);
			TestCase(false);

			void TestCase(bool isActive)
			{
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

				using (FreightRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = isActive }))
				using (var shipmentForm = new ShipmentFormForTest(shipment))
				{
					shipmentForm.Show();

					var panel = shipmentForm.Controls.Find("PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel", true).First();
					var visibilityButton = panel.Controls.Find("ViewShipmentTrackingButton", true).FirstOrDefault();

					if (isActive)
					{
						Assert("Shipment Visibility button is showed.", panel.Visible);
						Assert("Shipment Visibility button should be visible.", shipmentForm.ViewShipmentTrackingButton.Visible);

						var previousNextControl = GetZPreviousNextControl(panel.Parent);
						AssertNotNull("ZPreviousNextControl should exist.", previousNextControl);

						previousNextControl.Visible = true;
						AssertNotEquals("Display Shipment Visibility button to the right of ZPreviousNextControl in bottom panel.", 1, panel.Location.X);

						previousNextControl.Visible = false;
						AssertEquals("Display Shipment Visibility button at the left of bottom panel.", 1, panel.Location.X);
					}
					else
					{
						Assert("Shipment Visibility button is showed.", !panel.Visible);
						Assert("Shipment Visibility button should not be visible.", !shipmentForm.ViewShipmentTrackingButton.Visible);
					}
				}
			}
		}

		ZPreviousNextControl GetZPreviousNextControl(Control parentControl)
		{
			foreach (Control control in parentControl.Controls)
			{
				if (control is ZPreviousNextControl)
				{
					return (ZPreviousNextControl)control;
				}
			}

			return null;
		}

		public void TestViewShipmentTracking()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/portals");

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S01";

			using (FreightRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = true }))
			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();
				shipmentForm.ViewShipmentTrackingButton.PerformClick();
				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var expectedUrlPattern = @"https://address/portals/NST/Desktop\?noHeader=true#/tracker\?trackingNumber=S01&sso_otp=\w+";
				Assert(Regex.IsMatch(launchedUrl, expectedUrlPattern));
			}
		}

		public void TestShipmentTracking_GlowPortalsUrl_Empty() => TestViewShipmentTracking_WithNullOrWhiteSpaceGlowPortalsUrl_TriggersErrorModal(string.Empty);
		public void TestShipmentTracking_GlowPortalsUrl_Space() => TestViewShipmentTracking_WithNullOrWhiteSpaceGlowPortalsUrl_TriggersErrorModal(" ");
		public void TestShipmentTracking_GlowPortalsUrl_Tab() => TestViewShipmentTracking_WithNullOrWhiteSpaceGlowPortalsUrl_TriggersErrorModal("\t");
		public void TestShipmentTracking_GlowPortalsUrl_Newline() => TestViewShipmentTracking_WithNullOrWhiteSpaceGlowPortalsUrl_TriggersErrorModal("\r\n");

		void TestViewShipmentTracking_WithNullOrWhiteSpaceGlowPortalsUrl_TriggersErrorModal(string testUrl)
		{
			try
			{
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testUrl);
			}
			catch (RegistryValidationException)
			{
				// Swallow exception to enable us to bypass validation test whitespace URLs in the registry to emulate bad data in the database.
			}

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S01";

			using (FreightRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = true }))
			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();
				shipmentForm.ViewShipmentTrackingButton.PerformClick();
				var errorMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				Assert(errorMessage.Equals("This shipment cannot be opened in a browser as GLOW has not been configured for this client.\r\nRegistry: GLOW/Services/GLOW Portals Root URL"));
			}
		}

		#endregion

		#region ETailShipment Plugin

		public void TestShipmentTypeSetter_NewTypeIsHVL_MakeETailPluginTabVisible()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();

			using (SetupLicenceForETail())
			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();

				var pluginTabPage = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.ETailShipment).TabPage;

				AssertEquals("Precondition: Tab should not be visible by default", false, pluginTabPage.TabVisible);

				shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

				Assert("Tab is visible", pluginTabPage.TabVisible);
			}
		}

		[RequiresSTA]
		public void TestRelatedShipmentsTabVisibility_OpenSTDAndChangeToHVL()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			using (SetupLicenceForETail())
			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();
				Application.DoEvents();

				var relatedShipmentsTab = FindTab(shipmentForm, relatedShipmentsTabPageName);

				AssertNotNull("prerequisite: Related shipments tab has been found", relatedShipmentsTab);
				AssertEquals("Related shipments tab should be visible for STD shipment", true, relatedShipmentsTab.TabVisible);

				shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
				Application.DoEvents();

				AssertEquals("Related shipments tab should be visible for HVL shipment", true, relatedShipmentsTab.TabVisible);
			}
		}

		public void TestRelatedShipmentsTabVisibility_OpenHVLAndChangeToSTD()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

			using (SetupLicenceForETail())
			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();
				Application.DoEvents();

				var relatedShipmentsTab = FindTab(shipmentForm, relatedShipmentsTabPageName);

				AssertNotNull("prerequisite: Related shipments tab has been created for HVL shipment", relatedShipmentsTab);

				shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
				Application.DoEvents();

				relatedShipmentsTab = FindTab(shipmentForm, relatedShipmentsTabPageName);

				Assert("Related shipments tab should be visible for STD shipment", relatedShipmentsTab != null && relatedShipmentsTab.TabVisible);
			}
		}

		public void TestRelatedShipmentsTabVisibility_ChangeVisibilityFromViewMenu_HVL()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

			using (SetupLicenceForETail())
			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();
				Application.DoEvents();

				var relatedShipmentsTab = FindTab(shipmentForm, relatedShipmentsTabPageName);

				AssertNotNull("Related shipments tab has been created for HVL shipment", relatedShipmentsTab);
				AssertEquals("Related shipments tab is visible for HVL shipment", true, relatedShipmentsTab.TabVisible);

				var menu = FindMenuItem(shipmentForm, viewMenuItem, relatedShipmentsMenuItem);

				menu.PerformClick();

				AssertEquals("Related shipments tab is visible for HVL shipment as set in View menu", false, relatedShipmentsTab.TabVisible);
			}
		}

		public void TestScreenLayout_MoveConsolidationDetailsToBasicRegistrationAndHideAdditionalDetails_Works()
		{
			var query = new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, "SHP");

			var shipmentWorkflowTemplates = Factory.Load<ProcessTaskTemplate>(query);

			var workflowTemplate = shipmentWorkflowTemplates.First();

			foreach (var processTaskTemplate in shipmentWorkflowTemplates.Skip(1))
			{
				processTaskTemplate.P0_IsActive = false;
			}

			var additionalTabPageSettings = workflowTemplate.FormCustomisationSettings
				.DisplayTabs.Cast<FormCustomisableElement>()
				.First(t => t.ElementName == "AdditionalTabPage");
			var consolsElement = workflowTemplate.FormCustomisationSettings
				.DisplayFields.Cast<FormCustomisableElement>()
				.First(t => t.ElementName == "Consols");

			additionalTabPageSettings.Visible = false;
			consolsElement.Visible = true;
			consolsElement.DisplayTabCode = "ShipmentDetailsTabPage";
			consolsElement.Placement = "Middle Bottom";

			Factory.Save();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

			using (SetupLicenceForETail())
			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();
				Application.DoEvents();

				var loader = (IProcessTaskTemplateLoader)Activator.CreateInstance(ObjectFactory.GetType<IProcessTaskTemplateLoader>(), Factory);
				var matchedTemplate = loader.FindTemplateForScreenLayout(shipment);

				AssertEquals("prerequisite: matched correct workflow template", workflowTemplate, matchedTemplate);

				var additionalDetailsTabPage = FindTab(shipmentForm, "AdditionalTabPage");

				AssertNull("Additional Details tab is not visible", additionalDetailsTabPage);

				AssertNotNull("Consolidation Details is on Basic Registration tab", shipmentForm.FindSingleOrDefault<Control>("Consols"));
			}
		}

		public void TestRelatedShipmentsTabVisibility_DoNotShowRelatedShipmentsTabAsDirectedByWorkflow_HVL()
		{
			var query = new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, "SHP");

			var shipmentWorkflowTemplates = Factory.Load<ProcessTaskTemplate>(query);

			var workflowTemplate = shipmentWorkflowTemplates.First();

			foreach (var processTaskTemplate in shipmentWorkflowTemplates.Skip(1))
			{
				processTaskTemplate.P0_IsActive = false;
			}

			var relatedShipmentTabSettings = workflowTemplate.FormCustomisationSettings
				.DisplayTabs.Cast<FormCustomisableElement>()
				.First(t => t.ElementName == relatedShipmentsTabPageName);

			relatedShipmentTabSettings.Visible = true;

			Factory.Save();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

			using (SetupLicenceForETail())
			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();
				Application.DoEvents();

				var loader = (IProcessTaskTemplateLoader)Activator.CreateInstance(ObjectFactory.GetType<IProcessTaskTemplateLoader>(), Factory);
				var matchedTemplate = loader.FindTemplateForScreenLayout(shipment);

				AssertEquals("prerequisite: matched correct workflow template", workflowTemplate, matchedTemplate);

				var relatedShipmentsTab = FindTab(shipmentForm, relatedShipmentsTabPageName);

				Assert("Related shipments tab is visible", relatedShipmentsTab.TabVisible);
			}
		}

		#endregion

		#region Job Declaration Plugin

		public void TestJobDeclarationPlugIn()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();

			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();

				var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.JobDeclaration);
				AssertNotNull("Normal Job Declaration", plugin);
				AssertEquals("Correct Controller JobDeclaration on PlugIn", ControllerIDs.Customs.JobDeclaration, plugin.Controller.ID);
			}
		}

		#endregion

		#region EManifestShipmentImport Plugin

		[RequiresSTA]
		public void TestRelatedShipmentsTabVisibility_OpenSTDAndChangeToHLS()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			using (SetupLicenceForEManifest())
			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();
				Application.DoEvents();

				var relatedShipmentsTab = FindTab(shipmentForm, relatedShipmentsTabPageName);

				AssertNotNull("prerequisite: Related shipments tab has been found", relatedShipmentsTab);
				AssertEquals("Related shipments tab should be visible for STD shipment", true, relatedShipmentsTab.TabVisible);

				shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;
				Application.DoEvents();

				AssertEquals("Related shipments tab should be visible for HLS shipment", true, relatedShipmentsTab.TabVisible);
			}
		}

		public void TestRelatedShipmentsTabVisibility_OpenHLSAndChangeToSTD()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			using (SetupLicenceForEManifest())
			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();
				Application.DoEvents();

				var relatedShipmentsTab = FindTab(shipmentForm, relatedShipmentsTabPageName);

				AssertNotNull("prerequisite: Related shipments tab has been created for HLS shipment", relatedShipmentsTab);

				shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
				Application.DoEvents();

				relatedShipmentsTab = FindTab(shipmentForm, relatedShipmentsTabPageName);

				Assert("Related shipments tab should be visible for STD shipment", relatedShipmentsTab != null && relatedShipmentsTab.TabVisible);
			}
		}

		[RequiresSTA]
		public void TestRelatedShipmentsTabVisibility_ChangeVisibilityFromViewMenu_HLS()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			using (SetupLicenceForEManifest())
			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();
				Application.DoEvents();

				var relatedShipmentsTab = FindTab(shipmentForm, relatedShipmentsTabPageName);

				AssertNotNull("Related shipments tab has been created for HLS shipment", relatedShipmentsTab);
				AssertEquals("Related shipments tab is visible for HLS shipment", true, relatedShipmentsTab.TabVisible);

				var menu = FindMenuItem(shipmentForm, viewMenuItem, relatedShipmentsMenuItem);

				menu.PerformClick();

				AssertEquals("Related shipments tab is visible for HLS shipment as set in View menu", false, relatedShipmentsTab.TabVisible);
			}
		}

		public void TestRelatedShipmentsTabVisibility_ChangeVisibilityFromViewMenu_STD()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			using (SetupLicenceForEManifest())
			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();
				Application.DoEvents();

				var relatedShipmentsTab = FindTab(shipmentForm, relatedShipmentsTabPageName);

				AssertNotNull("prerequisite: Related shipments tab has been created", relatedShipmentsTab);
				AssertEquals("Related shipments tab is visible for STD shipment", true, relatedShipmentsTab.TabVisible);

				var menu = FindMenuItem(shipmentForm, viewMenuItem, relatedShipmentsMenuItem);

				menu.PerformClick();

				AssertEquals("Related shipments tab is not visible for STD shipment as set in View menu", false, relatedShipmentsTab.TabVisible);
			}
		}

		public void TestRelatedShipmentsTabVisibility_DoNotShowRelatedShipmentsTabAsDirectedByWorkflow_STD()
		{
			var query = new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, "SHP");

			var shipmentWorkflowTemplates = Factory.Load<ProcessTaskTemplate>(query);

			var workflowTemplate = shipmentWorkflowTemplates.First();

			foreach (var processTaskTemplate in shipmentWorkflowTemplates.Skip(1))
			{
				processTaskTemplate.P0_IsActive = false;
			}

			var relatedShipmentTabSettings = workflowTemplate.FormCustomisationSettings
				.DisplayTabs.Cast<FormCustomisableElement>()
				.First(t => t.ElementName == relatedShipmentsTabPageName);

			relatedShipmentTabSettings.Visible = false;

			Factory.Save();

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			using (SetupLicenceForEManifest())
			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();
				Application.DoEvents();

				var loader = (IProcessTaskTemplateLoader)Activator.CreateInstance(ObjectFactory.GetType<IProcessTaskTemplateLoader>(), Factory);
				var matchedTemplate = loader.FindTemplateForScreenLayout(shipment);

				AssertEquals("prerequisite: matched correct wokflow template", workflowTemplate, matchedTemplate);

				var relatedShipmentsTab = FindTab(shipmentForm, relatedShipmentsTabPageName);

				AssertNull("Related shipments tab is not visible as set by workflow template", relatedShipmentsTab);
			}
		}

		public void TestRelatedShipmentsTabVisibility_DoNotShowRelatedShipmentsTabAsDirectedByWorkflow_HLS()
		{
			var query = new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, "SHP");

			var shipmentWorkflowTemplates = Factory.Load<ProcessTaskTemplate>(query);

			var workflowTemplate = shipmentWorkflowTemplates.First();

			foreach (var processTaskTemplate in shipmentWorkflowTemplates.Skip(1))
			{
				processTaskTemplate.P0_IsActive = false;
			}

			var relatedShipmentTabSettings = workflowTemplate.FormCustomisationSettings
				.DisplayTabs.Cast<FormCustomisableElement>()
				.First(t => t.ElementName == relatedShipmentsTabPageName);

			relatedShipmentTabSettings.Visible = true;

			Factory.Save();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			using (SetupLicenceForEManifest())
			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();
				Application.DoEvents();

				var loader = (IProcessTaskTemplateLoader)Activator.CreateInstance(ObjectFactory.GetType<IProcessTaskTemplateLoader>(), Factory);
				var matchedTemplate = loader.FindTemplateForScreenLayout(shipment);

				AssertEquals("prerequisite: matched correct wokflow template", workflowTemplate, matchedTemplate);

				var relatedShipmentsTab = FindTab(shipmentForm, relatedShipmentsTabPageName);

				Assert("Related shipments tab is visible", relatedShipmentsTab.TabVisible);
			}
		}

		ZTabPage FindTab(ShipmentForm shipmentForm, string tabName)
		{
			return (ZTabPage)shipmentForm.Controls.Find(tabName, true)
				.FirstOrDefault();
		}

		const string relatedShipmentsTabPageName = "RelatedShipmentsTabPage";
		const string viewMenuItem = "View";
		const string relatedShipmentsMenuItem = "Related Shipments";

		#endregion

		#region Generate Packages with IDs

		public void TestGeneratePackagesWithIDsMenu()
		{
			foreach (var enableOverpacksAndSealsProjectFeature in new bool[] { false, true })
			{
				foreach (var enableTransitWarehouseIntegration in new bool[] { false, true })
				{
					using (FreightConfigurationRegistry.Instance.EnableOverpacksAndSealsProjectFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableOverpacksAndSealsProjectFeature))
					using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTransitWarehouseIntegration))
					{
						using (var shipmentForm = (ShipmentForm)GetEditFormToBash())
						{
							shipmentForm.Show();

							FindActionMenuItem(shipmentForm).OnPopup(EventArgs.Empty);

							var menuItem = FindActionMenuItem(shipmentForm).MenuItems.FindByText("Generate Packages with IDs");

							if (enableOverpacksAndSealsProjectFeature && enableTransitWarehouseIntegration)
							{
								AssertNotNull("Generate Packages with IDs is in Actions Menu while EnableOverpacksAndSealsProjectFeatures is true and EnableTransitWarehouseIntegration is true", menuItem);
							}
							else
							{
								AssertNull("Generate Packages with IDs is not in Actions Menu while EnableOverpacksAndSealsProjectFeatures is false or EnableTransitWarehouseIntegration is false", menuItem);
							}
						}
					}
				}
			}
		}

		#endregion

		#region Delivery Order Event

		public void TestDeliveryOrderHandedOverMenu()
		{
			using (var shipmentForm = (ShipmentForm)GetEditFormToBash())
			{
				shipmentForm.Show();

				FindActionMenuItem(shipmentForm).OnPopup(EventArgs.Empty);

				var menuItem = FindActionMenuItem(shipmentForm).MenuItems.FindByText("Delivery Order Handed Over");

				AssertNotNull("Delivery Order Handed Over is in Actions Menu", menuItem);
			}
		}

		public void TestNewRaiseEventLogForm()
		{
			var factory = new BusinessObjectFactory();
			ChildEditableService.SetState(factory, ChildEditableServiceStates.Shipment);
			var bO = factory.New<ForwardingShipment>();
			bO.Consols.AddNew();
			factory.Save();

			using (var shipmentForm = new ShipmentFormForTest(bO))
			{
				var @event = Events.DeliveryOrderHandedOver;
				using (var eventAddForm = shipmentForm.NewRaiseEventLogForm(@event))
				{
					var log = (BaseStmALog)eventAddForm.BusinessEntity;
					AssertEquals("Correct event type created", @event.Code, log.SL_SE_NKEvent);
				}
			}
		}

		#endregion

		#region Freight Labels Document

		public void TestDocumentShipmentFreightLabels()
		{
			var factory = new BusinessObjectFactory();
			ChildEditableService.SetState(factory, ChildEditableServiceStates.Shipment);
			var bO = factory.New<ForwardingShipment>();
			bO.Consols.AddNew();

			using (var shipmentForm = new ShipmentFormForTest(bO))
			{
				shipmentForm.Show();
				bO.DocumentSupporter.GetBODocDataProviders(new DataContextValueForTesting(Constants.DataContext.FreightLabels), null);
				Assert(ZFormModaliser.LastFormShownDialogForTest is DocumentShipmentForm);
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
			}
		}

		#endregion

		#region Shipper Letter of Indemnity Document

		public void TestDocumentShipmentLetterOfIndemnity()
		{
			var factory = new BusinessObjectFactory();
			ChildEditableService.SetState(factory, ChildEditableServiceStates.Shipment);
			var bO = factory.New<ForwardingShipment>();
			bO.Consols.AddNew();

			using (var shipmentForm = new ShipmentFormForTest(bO))
			{
				bO.DocumentSupporter.GetBODocDataProviders(new DataContextValueForTesting(Constants.DataContext.LetterOfIndemnity), null);
				Assert(ZFormModaliser.LastFormShownDialogForTest is DocumentNewDetailsForm);
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
			}
		}

		#endregion

		#region Charge Sheet Document

		public void TestDocumentShipmentChargeSheet()
		{
			OrgHeader debtor1 = Factory.New<OrgHeader>();
			debtor1.OH_Code = "AAA";
			debtor1.OH_FullName = "Alf";

			OrgHeader debtor2 = Factory.New<OrgHeader>();
			debtor2.OH_Code = "BBB";
			debtor2.OH_FullName = "Bronco Bill";

			Factory.Save();

			var menuItem = new BusinessObjectFactory().New<StmMenuItem>();
			menuItem.SU_DocumentDirection = "DEP";

			var factory = new BusinessObjectFactory();
			ChildEditableService.SetState(factory, ChildEditableServiceStates.Shipment);
			var shipment = factory.New<ForwardingShipment>();
			shipment.Consols.AddNew();

			JobHeader shipmentJob = new JobHeader.Loader(shipment).TryCreate();
			shipmentJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipmentJob.JH_GB = GlbBranch.CurrentBranch.PK;

			using (new ShipmentFormForTest(shipment))
			{
				IBODocDataProvider[] dataProviders = shipment.DocumentSupporter.GetBODocDataProviders(new DataContextValueForTesting(Constants.DataContext.ChargeSheet), menuItem);
				AssertNull(dataProviders);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			}

			JobCharge charge1 = (JobCharge)((BusinessObjectCollection)shipmentJob["Charges"]).AddNew();
			charge1.JR_OH_SellAccount = debtor1.PK;

			using (new ShipmentFormForTest(shipment))
			{
				IBODocDataProvider[] dataProviders = shipment.DocumentSupporter.GetBODocDataProviders(new DataContextValueForTesting(Constants.DataContext.ChargeSheet), menuItem);
				AssertEquals(1, dataProviders.Length);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			}

			JobCharge charge2 = (JobCharge)((BusinessObjectCollection)shipmentJob["Charges"]).AddNew();
			charge2.JR_OH_SellAccount = debtor2.PK;
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (new ShipmentFormForTest(shipment))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				IBODocDataProvider[] dataProviders = shipment.DocumentSupporter.GetBODocDataProviders(new DataContextValueForTesting(Constants.DataContext.ChargeSheet), menuItem);
				AssertEquals(2, dataProviders.Length);
				AssertEquals(typeof(DocumentChargeSheet), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		#endregion

		#region Routing Documents

		public void TestDocumentGenericFreightJobRouting()
		{
			var menuItem = new BusinessObjectFactory().New<StmMenuItem>();
			menuItem.SU_DocumentDirection = "DEP";
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var bizObj = Factory.New<ForwardingShipment>();
			bizObj.Transports.AddNew();
			bizObj.Transports.AddNew();

			using (var form = new ShipmentFormForTest(bizObj))
			{
				bizObj.DocumentSupporter.GetBODocDataProviders(new DataContextValueForTesting(Constants.DataContext.GenericFreightJobRouting), menuItem);
				Assert(ZFormModaliser.LastFormShownDialogForTest is DocumentSelectTransportForm);
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
			}
		}

		#endregion

		#region TestBolPrinting

		public void TestBolPrintingNoPermission()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed = false;
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "TestRef";
			using (new ShipmentForm(shipment))
			{
				IForwardingShipmentDocumentSupporterQueryProvider queryProvider = GetQueryProvider(shipment);
				AssertEquals(false, queryProvider.ConfirmBOLPrinting(shipment));
				AssertEquals("should show correct message", string.Format(WhatAreYouDoingYouPermissionlessFool, shipment.JS_UniqueConsignRef), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestBolPrintingWithPermission_AnswersNo()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed = true;
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "TestRef";
			using (new ShipmentForm(shipment))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				IForwardingShipmentDocumentSupporterQueryProvider queryProvider = GetQueryProvider(shipment);
				AssertEquals(false, queryProvider.ConfirmBOLPrinting(shipment));
				AssertEquals("should show correct message", string.Format(YouThinkYouHavePermissionForAnythingMrBigShot, shipment.JS_UniqueConsignRef), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestBolPrintingWithPermission_AnswersYes()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed = true;
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "TestRef";
			using (new ShipmentForm(shipment))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				IForwardingShipmentDocumentSupporterQueryProvider queryProvider = GetQueryProvider(shipment);
				AssertEquals(true, queryProvider.ConfirmBOLPrinting(shipment));
				AssertEquals("should show correct message", string.Format(YouThinkYouHavePermissionForAnythingMrBigShot, shipment.JS_UniqueConsignRef), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestBolPrintingWithPermission_Sea_AnswersNo()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed = true;
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "TestRef";
			using (new ShipmentForm(shipment))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				IForwardingShipmentDocumentSupporterQueryProvider queryProvider = GetQueryProvider(shipment);
				AssertEquals(true, queryProvider.ConfirmBOLPrinting(shipment));
				AssertEquals("should show correct message", string.Format(YouThinkYouHavePermissionForAnythingMrBigShot, shipment.JS_UniqueConsignRef), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		IForwardingShipmentDocumentSupporterQueryProvider GetQueryProvider(ForwardingShipment shipment)
		{
			return typeof(ForwardingShipmentDocumentSupporter).GetProperty("QueryProvider", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).GetValue(shipment.DocumentSupporter, null) as IForwardingShipmentDocumentSupporterQueryProvider;
		}

		readonly ZString WhatAreYouDoingYouPermissionlessFool = "The aggregate values of merchandise within shipment {0} that are covered by any single HTS number exceed $2500 and Customs Entry Number is not entered." + System.Environment.NewLine + System.Environment.NewLine +

				"You do not have the appropriate security rights to continue running this document." + System.Environment.NewLine + System.Environment.NewLine +

				"If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: " + System.Environment.NewLine + System.Environment.NewLine +

				"Operations -> Forwarding -> Shipments -> US Specifics -> Allow Printing of AWB/HBL if No Export Declaration Is Filed";
		readonly ZString YouThinkYouHavePermissionForAnythingMrBigShot = "The aggregate values of merchandise within shipment {0} that are covered by any single HTS number exceed $2500 and Customs Entry Number is not entered but you have the necessary security access to continue running this document." + System.Environment.NewLine + System.Environment.NewLine +
				"Are you sure you want to run this document?";

		#endregion

		#region Lead-Master changing

		[RequiresSTA]
		public void TestUnsetShipmentLeadOrMaster_Cancel()
		{
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.AssemblyMaster, Constants.ShipmentTypes.StandardHouse, false, DialogResult.Cancel, UnsetShipmentAsLeadOrMasterText);
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.BuyersConsolLead, Constants.ShipmentTypes.StandardHouse, false, DialogResult.Cancel, UnsetShipmentAsLeadOrMasterText);
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.CoLoadMaster, Constants.ShipmentTypes.StandardHouse, false, DialogResult.Cancel, UnsetShipmentAsLeadOrMasterText);
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.ShippersConsolLead, Constants.ShipmentTypes.StandardHouse, false, DialogResult.Cancel, UnsetShipmentAsLeadOrMasterText);
		}

		public void TestUnsetShipmentLeadOrMaster_OK()
		{
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.AssemblyMaster, Constants.ShipmentTypes.StandardHouse, true, DialogResult.OK, UnsetShipmentAsLeadOrMasterText);
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.BuyersConsolLead, Constants.ShipmentTypes.StandardHouse, true, DialogResult.OK, UnsetShipmentAsLeadOrMasterText);
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.CoLoadMaster, Constants.ShipmentTypes.StandardHouse, true, DialogResult.OK, UnsetShipmentAsLeadOrMasterText);
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.ShippersConsolLead, Constants.ShipmentTypes.StandardHouse, true, DialogResult.OK, UnsetShipmentAsLeadOrMasterText);
		}

		public void TestSetShipmentMaster_Cancel()
		{
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.AssemblyMaster, Constants.ShipmentTypes.CoLoadMaster, true, DialogResult.Cancel, null, false);
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.BuyersConsolLead, Constants.ShipmentTypes.CoLoadMaster, false, DialogResult.Cancel, SetShipmentAsCoLoadOrAssemblyMasterText);
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.ShippersConsolLead, Constants.ShipmentTypes.CoLoadMaster, false, DialogResult.Cancel, SetShipmentAsCoLoadOrAssemblyMasterText);
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.StandardHouse, Constants.ShipmentTypes.CoLoadMaster, false, DialogResult.Cancel, SetShipmentAsCoLoadOrAssemblyMasterText);

			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.CoLoadMaster, Constants.ShipmentTypes.AssemblyMaster, true, DialogResult.Cancel, null, false);
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.BuyersConsolLead, Constants.ShipmentTypes.AssemblyMaster, false, DialogResult.Cancel, SetShipmentAsCoLoadOrAssemblyMasterText);
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.ShippersConsolLead, Constants.ShipmentTypes.AssemblyMaster, false, DialogResult.Cancel, SetShipmentAsCoLoadOrAssemblyMasterText);
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.StandardHouse, Constants.ShipmentTypes.AssemblyMaster, false, DialogResult.Cancel, SetShipmentAsCoLoadOrAssemblyMasterText);
		}

		public void TestSetShipmentMaster_OK()
		{
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.AssemblyMaster, Constants.ShipmentTypes.CoLoadMaster, true, DialogResult.OK, null, false);
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.BuyersConsolLead, Constants.ShipmentTypes.CoLoadMaster, true, DialogResult.OK, SetShipmentAsCoLoadOrAssemblyMasterText);
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.ShippersConsolLead, Constants.ShipmentTypes.CoLoadMaster, true, DialogResult.OK, SetShipmentAsCoLoadOrAssemblyMasterText);
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.StandardHouse, Constants.ShipmentTypes.CoLoadMaster, true, DialogResult.OK, SetShipmentAsCoLoadOrAssemblyMasterText);

			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.CoLoadMaster, Constants.ShipmentTypes.AssemblyMaster, true, DialogResult.OK, null, false);
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.BuyersConsolLead, Constants.ShipmentTypes.AssemblyMaster, true, DialogResult.OK, SetShipmentAsCoLoadOrAssemblyMasterText);
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.ShippersConsolLead, Constants.ShipmentTypes.AssemblyMaster, true, DialogResult.OK, SetShipmentAsCoLoadOrAssemblyMasterText);
			AssertShipmentTypeChangingEventHandler(Constants.ShipmentTypes.StandardHouse, Constants.ShipmentTypes.AssemblyMaster, true, DialogResult.OK, SetShipmentAsCoLoadOrAssemblyMasterText);
		}

		void AssertShipmentTypeChangingEventHandler(ZString initialShipmentType, ZString selectedShipmentType, bool shouldChangeType, DialogResult dialogAnswer, string notificationMessage, bool makeNotFullfilRequirements = true)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(dialogAnswer);
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = initialShipmentType;

			if (makeNotFullfilRequirements)
			{
				switch (selectedShipmentType)
				{
					case Constants.ShipmentTypes.StandardHouse:
					case Constants.ShipmentTypes.HighVolumeLowValueLegacy:
						shipment.CoLoadShipments.AddNew();
						break;
					case Constants.ShipmentTypes.AssemblyMaster:
					case Constants.ShipmentTypes.CoLoadMaster:
						shipment.OuterPackLines.AddNew();
						break;
				}
			}

			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				shipment.JS_ShipmentType = selectedShipmentType;
				AssertEquals("Should show correct message", notificationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Shipment type", shouldChangeType ? selectedShipmentType : initialShipmentType, shipment.JS_ShipmentType);
			}
		}

		readonly string SetShipmentAsCoLoadOrAssemblyMasterText = @"Marking this shipment as a Co-Load Master or Assembly master will mean that inner pack-line and outer pack-line data will be removed from this shipment, as it will be calculated from the related sub-shipments.

Are you sure you want to continue?";
		readonly string UnsetShipmentAsLeadOrMasterText = @"Changing this shipment to be a Standard shipment will remove all related shipments from it.
Related shipments can only exist on a Buyers Consol Lead, Assembly Master or Co-Load Master shipment.

Are you sure you want to continue?";

		#endregion

		#region Workflow Form Customisation

		/// <summary>
		/// Don't delete this test if it breaks. It's pretty important.
		/// This is the *only* unit test in the entire of CW1 that is asserting form customisation works.
		/// </summary>
		public void TestWorkflowFormCustomisationWorks()
		{
			void SwapPositions(FormCustomisableElement element1, FormCustomisableElement element2)
			{
				var element1Placement = element1.Placement;
				var element1Tab = element1.DisplayTabCode;

				element1.Placement = element2.Placement;
				element1.DisplayTabCode = element2.DisplayTabCode;

				element2.Placement = element1Placement;
				element2.DisplayTabCode = element1Tab;
			}

			// Disable all of the system templates so I can just specify my own.
			var systemTemplates = Factory.Load<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, "SHP"));
			systemTemplates.ForEach(t => t.P0_IsActive = false);
			Factory.Save();

			// Set up a template that doesn't match the default shipment form setup.
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";
			var customFieldElement = template.FormCustomisationSettings.DisplayFields.Cast<FormCustomisableElement>().Single(n => n.ElementName == "CustomFields");
			var consolsElement = template.FormCustomisationSettings.DisplayFields.Cast<FormCustomisableElement>().Single(n => n.ElementName == "Consols");
			SwapPositions(consolsElement, customFieldElement);
			Factory.Save();

			var shipment = GetShipmentWithNoErrors();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				Application.DoEvents();

				// Consolidation Details should be on the first tab.
				AssertNotNull(form.FindAll<ZGroupBox>().SingleOrDefault(n => n.CaptionResourceString.Caption == "Consolidation Details"));

				// Custom field should be on the second tab (at least, after we open it)...  # Future proofing this test
				var additionalDetailTab = form.FindAll<ZTabPage>().SingleOrDefault(n => n.CaptionResourceString.Caption == "Additional Detail");
				var tabControl = (ZTabControl)additionalDetailTab.Parent;
				tabControl.SelectedTab = additionalDetailTab;
				Application.DoEvents();
				AssertNotNull(form.FindAll<ZGroupBox>().SingleOrDefault(n => n.CaptionResourceString.Caption == "Custom Fields"));
			}
		}

		#endregion

		#region Visible Tabs

		public void TestVisibleTabs()
		{
			var shipment = GetShipmentWithNoErrors();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				Application.DoEvents();

				form.DeliveryTabPage.TabVisible = false;
				((ITabVisibilityDeciderPersistence)form).StoreTabVisible(form.DeliveryTabPage);
				Factory.Save();
			}

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(false, ((ITabVisibilityDeciderPersistence)form).RetrieveTabPageVisible(form.DeliveryTabPage));
			}
		}

		public void TestNotifications()
		{
			var shipment = GetShipmentWithNoErrors();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.AddError("Display this to the user");
				AssertEquals("Display this to the user", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Confirmations

		public void TestDeliveryConfirmations()
		{
			var today = ZDateTime.Today;

			using (var form = GetNewZShipmentForm())
			{
				form.Show();
				Application.DoEvents();

				Shipment.JS_PackingMode = Constants.ContainerModes.Loose;
				Shipment.DocsAndCartage.JP_EstimatedDelivery = today;
				Shipment.OuterPackLines.AddNew();

				var confirms = Shipment.DeliveryConfirms;
				AssertEquals(0, confirms.Count);

				form.DeliveryTabPage.Show();
				var control = (ZTabControl)form.DeliveryTabPage.Controls.Find("DeliveryTabControl", true)[0];
				var tabDetails = (ZTabPage)form.DeliveryTabPage.Controls.Find("DeliveryDetailsTabPage", true)[0];
				var tabConfirms = (ZTabPage)form.DeliveryTabPage.Controls.Find("ConfirmationsPluginTabPage", true)[0];

				AssertConfirmations(control, tabDetails, tabConfirms, confirms, form);
				AssertEquals(today, confirms[0].EU_PlannedPickupDeliveryTime);
			}
		}

		public void TestPickupConfirmations()
		{
			var today = ZDateTime.Today;

			using (var form = GetNewZShipmentForm())
			{
				form.Show();
				Application.DoEvents();

				Shipment.JS_PackingMode = Constants.ContainerModes.Loose;
				Shipment.DocsAndCartage.JP_PickupRequiredBy = today;
				Shipment.OuterPackLines.AddNew();

				var confirms = Shipment.PickupConfirms;
				AssertEquals(0, confirms.Count);

				form.PickupTabPage.Show();
				var control = (ZTabControl)form.PickupTabPage.Controls.Find("PickupTabControl", true)[0];
				var tabDetails = (ZTabPage)form.PickupTabPage.Controls.Find("PickupDetailsTabPage", true)[0];
				var tabConfirms = (ZTabPage)form.PickupTabPage.Controls.Find("ConfirmationsPluginTabPage", true)[0];

				AssertConfirmations(control, tabDetails, tabConfirms, confirms, form);
				AssertEquals(today, confirms[0].EU_RequestedPickupDeliveryTime);
			}
		}

		void AssertConfirmations(ZTabControl control, ZTabPage tabDetails, ZTabPage tabConfirms, CommonPickupDeliveryConfirmCollection confirms, ShipmentFormForTest form)
		{
			FreightConfigurationRegistry.Instance.AutoCreateLooseConfirmations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, Shipment.AutoCreateLooseConfirmations);

			string emptyMessage = "None ";
			string expectedMessage = "Question The Confirmation was not previously created, would you like to create it now?";

			AssertEquals("Precondition", false, form.IsShipmentConfirmationsObsoleteWarningPrompted);

			Env.Security.PickupDeliveryConfirmationsNew.IsAllowed = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			control.SelectTab(tabDetails);
			AssertEquals(false, form.IsShipmentConfirmationsObsoleteWarningPrompted);
			AssertEquals(emptyMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(0, confirms.Count);

			control.SelectTab(tabConfirms);
			AssertEquals(false, form.IsShipmentConfirmationsObsoleteWarningPrompted);
			AssertEquals(emptyMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(0, confirms.Count);

			Env.Security.PickupDeliveryConfirmationsNew.IsAllowed = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			control.SelectTab(tabDetails);
			AssertEquals(false, form.IsShipmentConfirmationsObsoleteWarningPrompted);
			AssertEquals(emptyMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(0, confirms.Count);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			control.SelectTab(tabConfirms);
			AssertEquals(true, form.IsShipmentConfirmationsObsoleteWarningPrompted);
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(0, confirms.Count);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			control.SelectTab(tabDetails);
			AssertEquals(true, form.IsShipmentConfirmationsObsoleteWarningPrompted);
			AssertEquals(emptyMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(0, confirms.Count);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			control.SelectTab(tabConfirms);
			AssertEquals(true, form.IsShipmentConfirmationsObsoleteWarningPrompted);
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(1, confirms.Count);
		}

		#endregion

		#region SI Rejection Prompt

		[RequiresSTA]
		public void TestInactivePromptDoesDisplayWhenShipmentMadeInactive()
		{
			var shipment = GetShipmentWithEDIStatusUpdateLog();
			Factory.Save();

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				var actionMenuItemsProvider = (IFileMenuItemsProvider)form;
				var actionsMenuItem = actionMenuItemsProvider.ActionsMenuItem;
				var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");

				makeInactive.PerformClick();

				var msg = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertNotNull("Should prompt user if shipment made inactive", msg);
					AssertEquals("Shipment Rejection", msg.Caption);
					AssertEquals("This Shipment was created electronically, would you like to send a Shipping Instruction Rejection message to the booking party?", msg.Text);
				});
			}
		}

		public void TestInactivePromptDoesNotDisplayWhenShipmentMadeInactiveIfNotCreatedOrUpdatedByEDI()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();

				var actionMenuItemsProvider = (IFileMenuItemsProvider)form;
				var actionsMenuItem = actionMenuItemsProvider.ActionsMenuItem;
				var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");
				makeInactive.PerformClick();

				var msg = UnitTestUserNotification.Instance.LastMessage;

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertNull(msg.Text);
				AssertNotEquals("Shipment Rejection", msg.Caption);
				AssertNotEquals("This Shipment was created electronically, would you like to send a Shipping Instruction Rejection message to the booking party?", msg.Text);
			}
		}

		public void TestInactivePromptTriggersLogCreationIfAnsweredYes()
		{
			var shipment = GetShipmentWithEDIStatusUpdateLog();
			Factory.Save();

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var actionMenuItemsProvider = (IFileMenuItemsProvider)form;
				var actionsMenuItem = actionMenuItemsProvider.ActionsMenuItem;
				var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");
				makeInactive.PerformClick();

				var stuEvent = shipment.Logs.GetAllLogs().OfType<StmALog>().FirstOrDefault(log => log.SL_SE_NKEvent == Events.StatusUpdated.Code && log.SL_Reference.Contains(Params.New + "=" + ShipmentStatusList.Codes.SIRejected));

				AssertNotNull("STU event has been created on shipment", stuEvent);
				AssertEquals(ShipmentStatusList.Codes.SIRejected, stuEvent.Parameters[EventConstants.EventReferenceParameters.Codes.New]);
				AssertEquals("Shipment Cancelled", stuEvent.Parameters[EventConstants.EventReferenceParameters.Codes.Reason]);
				AssertEquals(ShipmentStatusList.Codes.SIRejected, shipment.JS_ShipmentStatus);
			}
		}

		public void TestInactivePromptDoesNotTriggerLogCreationIfAnsweredNo()
		{
			var shipment = GetShipmentWithEDIStatusUpdateLog();
			Factory.Save();

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var actionMenuItemsProvider = (IFileMenuItemsProvider)form;
				var actionsMenuItem = actionMenuItemsProvider.ActionsMenuItem;
				var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");

				makeInactive.PerformClick();

				var stuEvent = shipment.Logs.GetAllLogs().OfType<StmALog>().FirstOrDefault(log => log.SL_SE_NKEvent == Events.StatusUpdated.Code && log.SL_Reference.Contains(Params.New + "=" + ShipmentStatusList.Codes.SIRejected));

				AssertNull("STU event has not been created on shipment when not electronically created", stuEvent);
			}
		}

		#endregion

		#region Resynchronize Screening Status On Form Load

		public void TestResynchronizeScreeningStatusOnFormLoadDoNotShowSecurityNotAllowedMessage()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Env.Security.OrgDeniedPartyScreeningAllowResynchronize.IsAllowed = false;

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentFormForTest(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestResynchronizeScreeningStatusOnFormLoad()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			shipment.ConsignorPK = consignor.PK;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKDestination = "INBOM";
			shipment.JS_GoodsDescription = "GoodsDescription";
			shipment.JS_UniqueConsignRef = "BlahBlahBlah";
			shipment.JS_ReleaseType = shipment.Lookups.JS_ReleaseType_List[0].Code;
			Factory.Save();

			consignor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(false, shipment.Logs.Find(x => x.SL_SE_NKEvent == Events.DeniedPartyStatusUpdated.Code).Any());
				AssertEquals(ScreeningStatusesList.Codes.Clear, consignor.OH_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, consignee.OH_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Matched, shipment.JS_ScreeningStatus);
			});

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentFormForTest(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				CombineAssertions(() =>
				{
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(ScreeningStatusesList.Codes.Clear, consignor.OH_ScreeningStatus);
					AssertEquals(ScreeningStatusesList.Codes.Clear, consignee.OH_ScreeningStatus);
					AssertEquals(ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
					AssertEquals(1, shipment.Logs.Find(x => x.SL_SE_NKEvent == Events.DeniedPartyStatusUpdated.Code).Count());
					AssertContains("|NEW=CLR|OLD=MAT|TYP=SYNC", shipment.Logs.MostRecentLogByEventTime(Events.DeniedPartyStatusUpdated).SL_Reference);
				});
			}

			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				AssertEquals(ScreeningStatusesList.Codes.Matched, shipment.JS_ScreeningStatus);
				AssertEquals("The number of logs is still 1.", 1, shipment.Logs.Find(x => x.SL_SE_NKEvent == Events.DeniedPartyStatusUpdated.Code).Count());
			}
		}

		[RequiresSTA]
		public void TestResynchronizeScreeningStatusOnFormLoad_NoDeveloperNotificationExceptionThrown()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.ConsignorPK = consignor.PK;
			Factory.Save();

			consignor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			AssertNoExceptionThrown(() =>
			{
				using (TestingState.SuspendIsRunningTests())
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();
					CombineAssertions(() =>
					{
						AssertNotContains("Should NOT contain DeveloperNotificationException", "Created Changes Before Type (HasChanges: True, HasChangesNotIncludingChildren: False)", ErrorReporter.LastMessageReported);
						AssertEquals("Shipment screening status CLR.", ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
					});
				}
			});
		}

		public void TestResynchronizeScreeningStatusOnFormLoadIgnoreConcurrencyException()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_IsHighRisk = false;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			shipment.ConsignorPK = consignor.PK;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKDestination = "INBOM";
			shipment.JS_GoodsDescription = "GoodsDescription";
			shipment.JS_UniqueConsignRef = "BlahBlahBlah";
			shipment.JS_ReleaseType = shipment.Lookups.JS_ReleaseType_List[0].Code;
			Factory.Save();

			consignor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(false, shipment.Logs.Find(x => x.SL_SE_NKEvent == Events.DeniedPartyStatusUpdated.Code).Any());
				AssertEquals(ScreeningStatusesList.Codes.Clear, consignor.OH_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, consignee.OH_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Matched, shipment.JS_ScreeningStatus);
				AssertEquals(false, shipment.JS_IsHighRisk);
			});

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentFormForTest(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Db.Connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "UPDATE dbo.JobShipment SET JS_IsHighRisk = 1, JS_SystemLastEditTimeUtc = GETUTCDATE(), JS_SystemLastEditUser = '~BP' WHERE JS_PK = '{0}'", shipment.PK));
				AssertNoExceptionThrown(form.Show);

				CombineAssertions(() =>
				{
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(ScreeningStatusesList.Codes.Clear, consignor.OH_ScreeningStatus);
					AssertEquals(ScreeningStatusesList.Codes.Clear, consignee.OH_ScreeningStatus);
					AssertEquals(ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
					AssertEquals(1, shipment.Logs.Find(x => x.SL_SE_NKEvent == Events.DeniedPartyStatusUpdated.Code).Count());
					AssertContains("|NEW=CLR|OLD=MAT|TYP=SYNC", shipment.Logs.MostRecentLogByEventTime(Events.DeniedPartyStatusUpdated).SL_Reference);
					AssertEquals("not saved to DB", true, shipment.HasChanges);
				});
			}
		}

		[RequiresSTA]
		public void TestNoNeedToResynchronizeScreeningStatusOnFormLoad()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			shipment.ConsignorPK = consignor.PK;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKDestination = "INBOM";
			shipment.JS_GoodsDescription = "GoodsDescription";
			shipment.JS_UniqueConsignRef = "BlahBlahBlah";
			shipment.JS_ReleaseType = shipment.Lookups.JS_ReleaseType_List[0].Code;
			Factory.Save();

			consignor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(false, shipment.Logs.Find(x => x.SL_SE_NKEvent == Events.DeniedPartyStatusUpdated.Code).Any());
				AssertEquals(ScreeningStatusesList.Codes.Clear, consignor.OH_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, consignee.OH_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
			});

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentFormForTest(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				CombineAssertions(() =>
				{
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(false, shipment.Logs.Find(x => x.SL_SE_NKEvent == Events.DeniedPartyStatusUpdated.Code).Any());
					AssertEquals(ScreeningStatusesList.Codes.Clear, consignor.OH_ScreeningStatus);
					AssertEquals(ScreeningStatusesList.Codes.Clear, consignee.OH_ScreeningStatus);
					AssertEquals(ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
				});
			}
		}

		#endregion

		[RequiresSTA]
		public void TestRecordAdditionalInspectionTypeCodeChangeEvent()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_IsHighRisk = true;
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();

					AssertEquals("AdditionalInspectionTypeCode should be set UNK at first", FreightRegistry.AviationSecurity_Unknown_Code, shipment.JS_AdditionalInspectionTypeCode);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					UnitTestUserNotification.Instance.AddUserResponse("test reason");

					shipment.JS_AdditionalInspectionTypeCode = AdditionalScreeningMethods.Codes.PhysicalInspectionAndHandSearch;
					Factory.Save();

					shipment.JS_AdditionalInspectionTypeCode = AdditionalScreeningMethods.Codes.XRayEquipment;
					var msg = UnitTestUserNotification.Instance.LastMessage;
					Factory.Save();

					var secEvent = shipment.Logs.MostRecentLogByEventTime(Events.SecurityModified);
					CombineAssertions(() =>
					{
						AssertEquals("Check message caption", "Change Reason", msg.Caption);
						AssertEquals("Check message text", "Enter the reason for changing the Security Additional Inspection Status.\r\nThe reason will be recorded on the SEC-Security Modified event for future reference.", msg.Text);
						AssertEquals("AdditionalInspectionTypeCode should be set after changed", AdditionalScreeningMethods.Codes.XRayEquipment, shipment.JS_AdditionalInspectionTypeCode);
						AssertEquals("new SEC event created", 2, shipment.Logs.Find(x => x.SL_SE_NKEvent == Events.SecurityModified.Code).Count());
						AssertEquals("SEC event Reference", "|NEW=XRY|OLD=PHS|RES=test reason|TYP=High Risk", secEvent.SL_Reference);
					});
				}
			}
		}

		public void TestSaving_NonMatchingAgents()
		{
			var shipment = GetShipmentWithNoErrors();

			Factory.Save();

			AssertEquals("Precondition: Receiving Forwarder matches shipment consignee related agents", false, shipment.Validation.ConsigneeHasBeenChanged());
			AssertEquals("Precondition: Receiving Forwarder matches shipment consignor related agents", false, shipment.Validation.ConsignorHasBeenChanged());
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (ShipmentFormForTest form = new ShipmentFormForTest(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.Show();
				ContinueWithSave continueWithSave = form.ShowPreSaveDialogs();

				AssertNull("Dialog wasn't shown", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Saving allowed", ContinueWithSave.Yes, continueWithSave);
			}
		}

		public void TestSaving_NonMatchingAgents_Confirmed()
		{
			var shipment = GetShipmentWithShipmentConsigneeChanged();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentFormForTest(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				form.Show();
				ContinueWithSave continueWithSave = form.ShowPreSaveDialogs();

				var dialog = ZFormModaliser.LastFormShownDialogForTest as NonMatchingAgentsDialog;
				AssertNotNull("Dialog was shown", dialog);
				AssertEquals("Saving allowed", ContinueWithSave.Yes, continueWithSave);
			}
		}

		public void TestSaving_NonMatchingAgents_NotConfirmed()
		{
			var shipment = GetShipmentWithReceivingForwarderChanged();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentFormForTest(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;

				form.Show();
				ContinueWithSave continueWithSave = form.ShowPreSaveDialogs();

				var dialog = ZFormModaliser.LastFormShownDialogForTest as NonMatchingAgentsDialog;
				AssertNotNull("Dialog was shown", dialog);
				AssertEquals("Saving restricted", ContinueWithSave.No, continueWithSave);
			}
		}

		ForwardingShipment GetShipmentWithReceivingForwarderChanged()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignee1 = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = GetShipmentWithNoErrors();
			shipment.ConsigneePK = consignee.PK;
			consignee.SetRelatedParty(org, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			consignee1.SetRelatedParty(org1, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);

			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "C0000100";
			consol.JK_OA_ReceivingForwarderAddress = org.MainAddress.PK;

			shipment.Factory.Save();
			shipment.ConsigneePK = consignee1.PK;

			return shipment;
		}

		ForwardingShipment GetShipmentWithShipmentConsigneeChanged()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignee1 = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = GetShipmentWithNoErrors();
			shipment.ConsigneePK = consignee.PK;
			consignee.SetRelatedParty(org, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			consignee1.SetRelatedParty(org1, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);

			var consol = shipment.Consols.AddNew();
			consol.JK_OA_ReceivingForwarderAddress = org.MainAddress.PK;

			shipment.Factory.Save();
			shipment.ConsigneePK = consignee1.PK;

			return shipment;
		}

		ForwardingShipment GetShipmentWithConsignorAndConsignee(string transportMode)
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR" + transportMode;
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE" + transportMode;
			consignee.OH_IsConsignee = true;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			shipment.JS_TransportMode = transportMode;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;
			shipment.JS_ActualWeight = 20m;
			shipment.JS_ActualVolume = 1m;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			return shipment;
		}

		public ForwardingShipment GetShipmentWithEDIStatusUpdateLog()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.GenerateStatusEvent("ESI", "Electronic Shipping Instruction Received");
			return shipment;
		}

		public void TestActionsMenuItemsNotAvailableInViewMode()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			ActionsMenuItemsHelperTest.AssertActionsMenuItemsNotAvailableInViewMode(new ShipmentForm(Factory.New<ForwardingShipment>()));
		}

		public void TestShipmentLinkingMessagesSupporter()
		{
			Shipment = GetShipmentWithNoErrors();

			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message.EM_MessageType = "REL";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = "UNH+1+CUSRES:D:96A:UN'BGM+:::257+88888+11'LOC+22+0497:129::3072'DTM+58:201011250820:203'GIS+4'RFF+XC:10207000067891'UNT+7+1'";
			message.EM_MessageSubType = "CLR";
			message.EM_ApplicationReference = "10207000067891";
			message.EM_Status = EDIMessage.Status.Received;

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				var num = Shipment.Numbers.AddNew();
				ChildEditableService.SetState(Shipment.Factory, ChildEditableServiceStates.Shipment);
				using (ShipmentForm shipmentForm = new ShipmentForm(Shipment))
				{
					shipmentForm.Show();

					num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
					num.CE_EntryNum = "10207000067891";

					AssertEquals(1, Shipment.Messages.Count);
					AssertEquals(message.PK, Shipment.Messages[0].PK);

					num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;

					AssertEquals(0, Shipment.Messages.Count);
				}

				num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;

				AssertCollectionNotContains(message, Shipment.Messages);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				using (ShipmentForm shipmentForm = new ShipmentForm(Shipment))
				{
					shipmentForm.Show();

					var num = Factory.New<CusEntryNumber>();
					num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
					num.CE_EntryNum = "1020 7000067891";
					Shipment.Numbers.Add(num);

					AssertEquals(0, Shipment.Messages.Count);

					num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;

					AssertEquals(0, Shipment.Messages.Count);
				}
			}
		}

		public void TestBookingDetailsTabPage()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = GetShipmentWithNoErrors();
			shipment.JS_IsBooking = ZBool.True;
			Assert(shipment.IsStandAloneShipmentFromBooking);

			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();
				var bookingDetailsTabFound = shipmentForm.MainTabControl.AllTabPages.Any(page => page.Text == "Booking Details" && ((ZTabPage)page).TabVisible);
				Assert(bookingDetailsTabFound);

				shipment.JS_IsBooking = ZBool.False;
				var result = shipmentForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);

				bookingDetailsTabFound = shipmentForm.MainTabControl.AllTabPages.Any(page => page.Text == "Booking Details" && ((ZTabPage)page).TabVisible);
				Assert(!bookingDetailsTabFound);
			}
		}

		public void TestUpdateNotesTabWithRelatedChanges()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = GetShipmentWithNoErrors();

			shipment.JS_IsBooking = ZBool.True;
			Assert(shipment.IsStandAloneShipmentFromBooking);

			Factory.Save();

			string currentCode = Env.CurrentCompany.Code;
			var companies = Factory.Load<IGlbCompany>(new ZQuery());
			var nonCurrentCompany = companies.Where(x => x.GC_Code != currentCode).First();
			var currentCompany = companies.Where(x => x.GC_Code == currentCode).First();

			var org = Factory.Load<OrgHeader>(shipment.ConsignorPK);

			StmNote orderNote1 = org.Notes.AddNew(false, "Internal Work Notes 1", "Test Note Text 1");
			StmNote orderNote2 = org.Notes.AddNew(false, "Internal Work Notes 2", "Test Note Text 2");

			orderNote1.ST_GC_RelatedCompany = currentCompany.PK;
			orderNote1.ST_NoteContext = "AAS";

			orderNote2.ST_GC_RelatedCompany = currentCompany.PK;
			orderNote2.ST_NoteContext = "AAB";

			Factory.Save();

			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.Show();
				Application.DoEvents();
				Assert(shipment.Notes.VisibleNotes.Contains(orderNote1));
				Assert(!shipment.Notes.VisibleNotes.Contains(orderNote2));

				shipment.JS_TransportMode = Constants.TransportModes.AirSea;
				ZStmNoteTabPage notesTab = (ZStmNoteTabPage)shipmentForm.MainTabControl.AllTabPages.Single(page => page.Text == "Notes");
				notesTab.UpdateNoteImageOnRelatedChanges();
				Assert(!shipment.Notes.VisibleNotes.Contains(orderNote1));
				Assert(shipment.Notes.VisibleNotes.Contains(orderNote2));
			}
		}

		[RequiresSTA]
		public void TestShipmentFormIsNotRememberingSplitterLayout()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			using (var form = new ShipmentForm(shipment))
			{
				AssertEquals("Should not remember Splitter Layout", false, form.RememberSplitterLayout);
			}
		}

		[RequiresSTA]
		public void TestScreeningLogsTabPage()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			using (FreightRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (var form = new ShipmentFormForTest(shipment))
			{
				var tabControl = (ZTabControl)form.EventTabPageForTest.Controls[0].Controls[0];
				AssertEquals(2, tabControl.TabPages.Count);
				AssertEquals("Denied Party Screening Logs", tabControl.TabPages[1].Text);
				AssertEquals(typeof(RelatedDeniedPartyScreeningStatusControl), tabControl.TabPages[1].Controls[0].GetType());
			}
		}

		public void TestAddComplianceLogsTabPage_WhenComplianceWiseIsEnabled()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (var form = new ShipmentFormForTest(shipment))
			{
				var tabControl = (ZTabControl)form.EventTabPageForTest.Controls[0].Controls[0];
				AssertEquals(2, tabControl.TabPages.Count);
				AssertEquals("Compliance Logs", tabControl.TabPages[1].Text);
				AssertEquals(typeof(ComplianceLogUserControl), tabControl.TabPages[1].Controls[0].GetType());

				var tabControl2 = (ZTabControl)tabControl.TabPages[1].Controls[0].Controls[0];
				AssertEquals("Compliance Risk Status Changes", tabControl2.TabPages[0].Text);
				AssertEquals("Removed Commodities", tabControl2.TabPages[1].Text);
			}
		}

		public void TestAddComplianceLogsTabPage_WhenComplianceWiseIsEnabled_AndDpsLogTabIsAdded()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (var form = new ShipmentFormForTest(shipment))
			{
				form.AddScreeningLogsTabPage();
				var tabControl = (ZTabControl)form.EventTabPageForTest.Controls[0].Controls[0];
				AssertEquals(3, tabControl.TabPages.Count);
				AssertEquals("Compliance Logs", tabControl.TabPages[1].Text);
				AssertEquals(typeof(ComplianceLogUserControl), tabControl.TabPages[1].Controls[0].GetType());

				var tabControl2 = (ZTabControl)tabControl.TabPages[1].Controls[0].Controls[0];
				AssertEquals("Compliance Risk Status Changes", tabControl2.TabPages[0].Text);
				AssertEquals("Removed Commodities", tabControl2.TabPages[1].Text);
				AssertEquals("Denied Party Screening Logs", tabControl2.TabPages[2].Text);
			}
		}

		public void TestAddComplianceLogsTabPage_WhenComplianceWiseIsDisabled()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			using (FreightRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (var form = new ShipmentFormForTest(shipment))
			{
				var tabControl = (ZTabControl)form.EventTabPageForTest.Controls[0].Controls[0];
				AssertEquals(2, tabControl.TabPages.Count);
				AssertEquals("Change Logs", tabControl.TabPages[0].Text);
				AssertEquals("Denied Party Screening Logs", tabControl.TabPages[1].Text);
				AssertEquals(typeof(RelatedDeniedPartyScreeningStatusControl), tabControl.TabPages[1].Controls[0].GetType());
			}
		}

		public void TestPromptReasonWhileRejectingShipmentBookingStatus()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicShippingInstruction;
			Factory.Save();

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddUserResponse(ZString.Empty);
				shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.SIRejected;

				var msg = UnitTestUserNotification.Instance.LastMessage;
				CombineAssertions(() =>
				{
					AssertEquals("Check message caption", "Rejection Reason", msg.Caption);
					AssertEquals("Check message text", "Please enter the reason of rejection.", msg.Text);
					AssertEquals("Status should not be set", ShipmentStatusList.Codes.ElectronicShippingInstruction, shipment.JS_ShipmentStatus);
				});

				UnitTestUserNotification.Instance.AddUserResponse("Non empty user response.");
				shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.SIRejected;

				CombineAssertions(() =>
				{
					AssertEquals("Check message caption", "Rejection Reason", msg.Caption);
					AssertEquals("Check message text", "Please enter the reason of rejection.", msg.Text);
					AssertEquals("Status should be set", ShipmentStatusList.Codes.SIRejected, shipment.JS_ShipmentStatus);
				});
			}
		}

		public void TestOnPackLineAddedToContainer()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = GetShipmentWithNoErrors();
			var consol = (ForwardingConsol)shipment.Consols.FirstOrDefault() ?? shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.ULD;

			var container = consol.Containers.AddNew();
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "LD-3")).PK;
			container.JC_GrossWeightUQ = Weight.Kilograms;
			container.JC_TareWeight = 50;
			container.JC_ContainerNum = "CONT0001";
			container.JC_IsGrossWeightOverridden = true;
			container.JC_GrossWeight = 99;

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_JC = container.PK;

				CombineAssertions(() =>
				{
					var msg = UnitTestUserNotification.Instance.LastMessage;
					AssertEquals("caption", "", msg.Caption);
					AssertEquals("text", "Attaching Shipment BlahBlahBlah. Gross Weight of Container CONT0001 is overridden. Would you like to retain the Overridden Gross Weight?", msg.Text);
					AssertEquals("no change if user says retain", true, container.JC_IsGrossWeightOverridden);
					AssertEquals("no change if user says retain", 99m, container.JC_GrossWeight);
				});

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_JC = container.PK;
				AssertEquals("no prompt for second packline", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				packLine1.JL_JC = ZGuid.Empty;
				packLine2.JL_JC = ZGuid.Empty;
				packLine1.JL_JC = container.PK;
				CombineAssertions(() =>
				{
					var msg = UnitTestUserNotification.Instance.LastMessage;
					AssertEquals("caption", "", msg.Caption);
					AssertEquals("text", "Attaching Shipment BlahBlahBlah. Gross Weight of Container CONT0001 is overridden. Would you like to retain the Overridden Gross Weight?", msg.Text);
					AssertEquals("user says not retain", false, container.JC_IsGrossWeightOverridden);
					AssertEquals("JC_GrossWeight updated", 50m, container.JC_GrossWeight);
				});
			}
		}

		[RequiresSTA]
		public void TestPromptDraftBillOfLadingFormWhileConfirmingShipmentBookingStatus()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicShippingInstruction;
			shipment.Logs.AddNew(Events.StatusUpdated, $"|{Params.Type}={Constants.EventReferenceMessageTypes.ShipmentStatus}", new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.ElectronicShippingInstruction));

			Factory.Save();

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();

				shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;

				var msg = UnitTestUserNotification.Instance.LastMessage;
				CombineAssertions(() =>
				{
					AssertEquals("Draft Bill Of Lading form should be shown", "Enterprise.DocumentVisualizer.GUI.DocumentVisualizerForm", ZFormModaliser.LastFormShownDialogForTest.GetType().FullName);
					AssertEquals("Form's Text", "Draft Bill of Lading", ZFormModaliser.LastFormShownDialogForTest.Text);
				});
			}
		}

		public void TestUpdatingCarrierAddressShouldDefaultConsolBookingAgentAddress()
		{
			var expectedMessage = "The Carrier has been changed.  Do you wish to re-default the Carrier Booking Agent from the Carrier based on the new 1st Load Port?";

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			var (org1, consolA) = CreateOrgAndConsol("ABC");
			var (org2, consolB) = CreateOrgAndConsol("XYZ");

			// The defaulting only alerts when the consol is in the database
			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();

				consolA.Shipments.Add(shipment);
				AssertNull("Should not alert when setting values for first time", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull("Should have a doc address", consolA.CarrierBookingAgentDocumentaryAddress);

				consolA.SetShippingLine(org1.MainAddress.PK, "Testing");
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should alert when changing carrier of consol", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();

				shipment.Consols.Remove(consolA);
				shipment.Consols.Add(consolB);

				consolB.SetShippingLine(org2.MainAddress.PK, "Testing");
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should altert when changing carrier of consol that was not attached to the shipment when opened", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region TransitWarehousePlanningPortal

		[RequiresSTA]
		public void TestTransitActionMenuItemsVisible()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var nonSupportUser = Factory.New<GlbStaff>();
			AssertEquals("Precondition", false, nonSupportUser.IsSupportUser);

			using (Env.SetTemporaryUserContext(new UserContext(nonSupportUser, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				using (var form = new ShipmentFormForTest(shipment))
				{
					form.Show();
					var transitWarehouseMenu = FindActionMenuItem(form).MenuItems.FindByText("Transit Warehouse");
					var pickupTWMenu = transitWarehouseMenu.MenuItems.FindByText("Pickup TW");
					var deliveryTWMenu = transitWarehouseMenu.MenuItems.FindByText("Delivery TW");

					AssertNotNull(pickupTWMenu);
					AssertNotNull(deliveryTWMenu);
					AssertNotNull(pickupTWMenu.MenuItems.FindByText("Receipt Instruction Planning Portal"));
					AssertNotNull(pickupTWMenu.MenuItems.FindByText("Dispatch Instruction Planning Portal"));
					AssertNotNull(deliveryTWMenu.MenuItems.FindByText("Receipt Instruction Planning Portal"));
					AssertNotNull(deliveryTWMenu.MenuItems.FindByText("Dispatch Instruction Planning Portal"));
				}
			}
		}

		public void TestTransitUrlGlowRegistryEmpty()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseType = "TRW";
			warehouse.WW_IsActive = true;
			shipment.JS_OA_ExportReceivingDepot = warehouse.WW_OA_WarehouseAddress;
			var rcn = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			rcn.WRC_ParentID = shipment.PK;
			rcn.WRC_ParentTableCode = shipment.TablePrefix;
			rcn.WRC_WW_IntendedWarehouse = warehouse.PK;
			Factory.Save();

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				var transitWarehouseMenu = FindActionMenuItem(form).MenuItems.FindByText("Transit Warehouse");
				var pickupMenu = transitWarehouseMenu.MenuItems.FindByText("Pickup TW");
				pickupMenu.MenuItems.FindByText($"Receipt Instruction Planning Portal - {rcn.WRC_JobID}").PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.Contains($@"This {shipment.HumanReadableName} cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL"));
			}
		}

		public void TestTransitOpenPickupRCNsInBrowser()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseType = "TRW";
			shipment.JS_OA_ExportReceivingDepot = warehouse.WW_OA_WarehouseAddress;
			var rcn = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			var rcn2 = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			rcn.WRC_ParentID = rcn2.WRC_ParentID = shipment.PK;
			rcn.WRC_ParentTableCode = rcn2.WRC_ParentTableCode = shipment.TablePrefix;
			rcn.WRC_WW_IntendedWarehouse = rcn2.WRC_WW_IntendedWarehouse = warehouse.PK;
			Factory.Save();

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();

				var transitWarehouseMenu = FindActionMenuItem(form).MenuItems.FindByText("Transit Warehouse");
				var pickupMenu = transitWarehouseMenu.MenuItems.FindByText("Pickup TW");
				pickupMenu.MenuItems.FindByText($"Receipt Instruction Planning Portal - {rcn.WRC_JobID}").PerformClick();
				AssertEquals("No error should be thrown", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				var parametersToAssert = new NameValueCollection
				{
					{ "rcn", rcn.PK.ToString() }
				};
				AssertOpenedOnTheWeb(glowPortalsUri: "address", parametersToAssert);

				pickupMenu.MenuItems.FindByText($"Receipt Instruction Planning Portal - {rcn2.WRC_JobID}").PerformClick();
				AssertEquals("No error should be thrown", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				parametersToAssert = new NameValueCollection
				{
					{ "rcn", rcn2.PK.ToString() }
				};
				AssertOpenedOnTheWeb(glowPortalsUri: "address", parametersToAssert);
			}
		}

		[RequiresSTA]
		public void TestTransitOpenDeliveryRCNsInBrowser()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseType = "TRW";
			shipment.JS_OA_ImportReleaseDepot = warehouse.WW_OA_WarehouseAddress;
			var rcn = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			var rcn2 = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			rcn.WRC_ParentID = rcn2.WRC_ParentID = shipment.PK;
			rcn.WRC_ParentTableCode = rcn2.WRC_ParentTableCode = shipment.TablePrefix;
			rcn.WRC_WW_IntendedWarehouse = rcn2.WRC_WW_IntendedWarehouse = warehouse.PK;
			Factory.Save();

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();

				var transitWarehouseMenu = FindActionMenuItem(form).MenuItems.FindByText("Transit Warehouse");
				var deliveryMenu = transitWarehouseMenu.MenuItems.FindByText("Delivery TW");
				deliveryMenu.MenuItems.FindByText($"Receipt Instruction Planning Portal - {rcn.WRC_JobID}").PerformClick();
				AssertEquals("No error should be thrown", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				var parametersToAssert = new NameValueCollection
				{
					{ "rcn", rcn.PK.ToString() }
				};
				AssertOpenedOnTheWeb(glowPortalsUri: "address", parametersToAssert);

				deliveryMenu.MenuItems.FindByText($"Receipt Instruction Planning Portal - {rcn2.WRC_JobID}").PerformClick();
				AssertEquals("No error should be thrown", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				parametersToAssert = new NameValueCollection
				{
					{ "rcn", rcn2.PK.ToString() }
				};
				AssertOpenedOnTheWeb(glowPortalsUri: "address", parametersToAssert);
			}
		}

		[RequiresSTA]
		public void TestTransitNoPickUpAddress()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();

				var transitWarehouseMenu = FindActionMenuItem(form).MenuItems.FindByText("Transit Warehouse");
				var pickupMenu = transitWarehouseMenu.MenuItems.FindByText("Pickup TW");
				pickupMenu.MenuItems.FindByText("Receipt Instruction Planning Portal").PerformClick();
				AssertEquals("A valid Pickup CFS / Transit Warehouse address must be entered before the planning portal can be accessed.", UnitTestUserNotification.Instance.LastMessage.Text);
				pickupMenu.MenuItems.FindByText("Dispatch Instruction Planning Portal").PerformClick();
				AssertEquals("A valid Pickup CFS / Transit Warehouse address must be entered before the planning portal can be accessed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestTransitNoDeliveryAddress()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();

				var transitWarehouseMenu = FindActionMenuItem(form).MenuItems.FindByText("Transit Warehouse");
				var deliveryMenu = transitWarehouseMenu.MenuItems.FindByText("Delivery TW");
				deliveryMenu.MenuItems.FindByText("Receipt Instruction Planning Portal").PerformClick();
				AssertEquals("A valid Delivery CFS / Transit Warehouse address must be entered before the planning portal can be accessed.", UnitTestUserNotification.Instance.LastMessage.Text);
				deliveryMenu.MenuItems.FindByText("Dispatch Instruction Planning Portal").PerformClick();
				AssertEquals("A valid Delivery CFS / Transit Warehouse address must be entered before the planning portal can be accessed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestTransitOpenDeliveryDCNsInBrowser()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseType = "TRW";
			shipment.JS_OA_ImportReleaseDepot = warehouse.WW_OA_WarehouseAddress;
			var dcn = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			var dcn2 = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			dcn.WDC_ParentID = dcn2.WDC_ParentID = shipment.PK;
			dcn.WDC_ParentTableCode = dcn2.WDC_ParentTableCode = shipment.TablePrefix;
			dcn.WDC_WW_Warehouse = dcn2.WDC_WW_Warehouse = warehouse.PK;
			Factory.Save();

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();

				var transitWarehouseMenu = FindActionMenuItem(form).MenuItems.FindByText("Transit Warehouse");
				var deliveryMenu = transitWarehouseMenu.MenuItems.FindByText("Delivery TW");
				deliveryMenu.MenuItems.FindByText($"Dispatch Instruction Planning Portal - {dcn.WDC_JobID}").PerformClick();
				AssertEquals("No error should be thrown", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				var parametersToAssert = new NameValueCollection
				{
					{ "dcn", dcn.PK.ToString() }
				};
				AssertOpenedOnTheWeb(glowPortalsUri: "address", parametersToAssert);

				deliveryMenu.MenuItems.FindByText($"Dispatch Instruction Planning Portal - {dcn2.WDC_JobID}").PerformClick();
				AssertEquals("No error should be thrown", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				parametersToAssert = new NameValueCollection
				{
					{ "dcn", dcn2.PK.ToString() }
				};
				AssertOpenedOnTheWeb(glowPortalsUri: "address", parametersToAssert);
			}
		}

		public void TestTransitOpenPickupDCNsInBrowser()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseType = "TRW";
			shipment.JS_OA_ExportReceivingDepot = warehouse.WW_OA_WarehouseAddress;
			var dcn = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			var dcn2 = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			dcn.WDC_ParentID = dcn2.WDC_ParentID = shipment.PK;
			dcn.WDC_ParentTableCode = dcn2.WDC_ParentTableCode = shipment.TablePrefix;
			dcn.WDC_WW_Warehouse = dcn2.WDC_WW_Warehouse = warehouse.PK;
			Factory.Save();

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();

				var transitWarehouseMenu = FindActionMenuItem(form).MenuItems.FindByText("Transit Warehouse");
				var twPickupMenu = transitWarehouseMenu.MenuItems.FindByText("Pickup TW");
				twPickupMenu.MenuItems.FindByText($"Dispatch Instruction Planning Portal - {dcn.WDC_JobID}").PerformClick();
				AssertEquals("No error should be thrown", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				var parametersToAssert = new NameValueCollection
				{
					{ "dcn", dcn.PK.ToString() }
				};
				AssertOpenedOnTheWeb(glowPortalsUri: "address", parametersToAssert);

				twPickupMenu.MenuItems.FindByText($"Dispatch Instruction Planning Portal - {dcn2.WDC_JobID}").PerformClick();
				AssertEquals("No error should be thrown", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				parametersToAssert = new NameValueCollection
				{
					{ "dcn", dcn2.PK.ToString() }
				};
				AssertOpenedOnTheWeb(glowPortalsUri: "address", parametersToAssert);
			}
		}

		[RequiresSTA]
		public void TestTransitNoRCNOpenInBrowser()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseType = "TRW";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_OA_ImportReleaseDepot = shipment.JS_OA_ExportReceivingDepot = warehouse.WW_OA_WarehouseAddress;
			Factory.Save();

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();

				var transitWarehouseMenu = FindActionMenuItem(form).MenuItems.FindByText("Transit Warehouse");
				var pickupMenu = transitWarehouseMenu.MenuItems.FindByText("Pickup TW");
				var deliveryMenu = transitWarehouseMenu.MenuItems.FindByText("Delivery TW");
				pickupMenu.MenuItems.FindByText("Receipt Instruction Planning Portal").PerformClick();
				AssertEquals("Send instructions to the Transit Warehouse first, before trying to access the planning portal.", UnitTestUserNotification.Instance.LastMessage.Text);
				deliveryMenu.MenuItems.FindByText("Receipt Instruction Planning Portal").PerformClick();
				AssertEquals("Send instructions to the Transit Warehouse first, before trying to access the planning portal.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestTransitNoDCNOpenInBrowser()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseType = "TRW";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_OA_ImportReleaseDepot = shipment.JS_OA_ExportReceivingDepot = warehouse.WW_OA_WarehouseAddress;
			Factory.Save();

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();

				var transitWarehouseMenu = FindActionMenuItem(form).MenuItems.FindByText("Transit Warehouse");
				var pickupMenu = transitWarehouseMenu.MenuItems.FindByText("Pickup TW");
				var deliveryMenu = transitWarehouseMenu.MenuItems.FindByText("Delivery TW");
				pickupMenu.MenuItems.FindByText("Dispatch Instruction Planning Portal").PerformClick();
				AssertEquals("Send instructions to the Transit Warehouse first, before trying to access the planning portal.", UnitTestUserNotification.Instance.LastMessage.Text);
				deliveryMenu.MenuItems.FindByText("Dispatch Instruction Planning Portal").PerformClick();
				AssertEquals("Send instructions to the Transit Warehouse first, before trying to access the planning portal.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public static void AssertOpenedOnTheWeb(string glowPortalsUri, NameValueCollection expectedParameters)
		{
			var launchedUrl = WebUrlLauncher.LastUrlLaunched;
			var uri = new Uri(launchedUrl, UriKind.Absolute);
			var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
			var accessToken = queryKeyValuePairs["sso_otp"];

			AssertNotNull("A Glow Access Token should be attached", accessToken);
			AssertEquals("https", uri.Scheme);
			AssertEquals(glowPortalsUri, uri.Host);
			AssertEquals("/TWP/Desktop", uri.AbsolutePath);

			foreach (var paramName in expectedParameters.AllKeys)
			{
				AssertEquals(queryKeyValuePairs[paramName], expectedParameters[paramName]);
			}
		}

		#endregion

		#region PerformValidation

		[RequiresSTA]
		public void TestPerformValidation_ValidatesPacklines()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "MYPKG";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.OuterPackLines.AddNew().HarmonisedCodes.AddNew();
			shipment.InnerPackLines.AddNew().HarmonisedCodes.AddNew();

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.ValidateAndSave();

				AssertHasWarning(
					shipment.OuterPackLines.OfType<ForwardingPackLine>().First().JL_HarmonisedCodeInfo,
					"HS Code is required for exports and imports to/from Malaysia."
				);

				AssertHasWarning(
					shipment.InnerPackLines.OfType<ForwardingPackLine>().First().JL_PackageCountInfo,
					"Zero packages is valid only for an empty container or an unknown number of packages. If your container is empty, flag it accordingly from Consol > Containers tab."
				);
			}
		}

		#endregion

		#region ExportBroker/ImportBroker

		[RequiresSTA]
		public void TestShipmentForm_ConsigneePK_Set_ImportBroker()
		{
			var shipment = GetShipmentWithNoErrors();
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Factory.Save();

			shipment.ConsigneePK = ZGuid.NewZGuid();
			Assert(shipment.JS_OH_ImportBroker.IsEmpty);

			var testPKs = FreightTestHelper.CreateImportOrgMiscServInDB();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				shipment.ConsigneePK = testPKs.OrgHeader;
				AssertEquals("Shipment should have AIR customs broker", testPKs.AirImportCustomsBroker, shipment.JS_OH_ImportBroker);
				AssertEquals("Consignee has been changed. Do you wish to update the Import Broker?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShipmentForm_ConsignorPK_Set_ExportBroker()
		{
			var shipment = GetShipmentWithNoErrors();
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "";
			Factory.Save();

			shipment.JS_OH_ExportBroker = ZGuid.Empty;
			shipment.JS_RX_NKGoodsValueCurr = ZString.Empty;
			shipment.ConsignorPK = ZGuid.Empty;
			Assert(shipment.JS_OH_ExportBroker.IsEmpty);
			Assert(shipment.DocsAndCartage.PickupCartageCoPK.IsEmpty);
			Assert(shipment.JS_RX_NKGoodsValueCurr.IsEmpty);
			Assert(shipment.ConsignorPK.IsEmpty);

			shipment.ConsignorPK = Factory.New<OrgHeader>().PK;
			Assert(shipment.JS_OH_ExportBroker.IsEmpty);
			Assert(shipment.DocsAndCartage.PickupCartageCoPK.IsEmpty);

			var testPKs = FreightTestHelper.CreateExportOrgMiscServInDB();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();

				shipment.ConsignorPK = testPKs.OrgHeader;
				AssertEquals("Shipment should have an Exporter", testPKs.OrgHeader, shipment.ConsignorPK);
				AssertEquals("Shipment should have SEA customs broker", testPKs.SeaExportCustomsBroker, shipment.JS_OH_ExportBroker);
				AssertEquals("Consignor has been changed. Do you wish to update the Export Broker?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Template record

		public void TestAddressTabEnabledForNewTemplateForm()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";

			var templateRecord = Factory.New<StmTemplateRecord>();
			var templateRecordProvider = (ITemplateRecordProvider)shipment;
			templateRecordProvider.TemplateRecord = templateRecord;

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				AssertNotNull(shipmentForm.PlugIns.GetPlugIn(ControllerIDs.DocAddresses));
			}
		}

		public void TestAddressTabEnabledForEditTemplateForm()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";

			var templateRecord = Factory.New<StmTemplateRecord>();
			var templateRecordProvider = (ITemplateRecordProvider)shipment;
			templateRecordProvider.TemplateRecord = templateRecord;
			templateRecordProvider.SaveToTemplateRecord();

			Factory.Save();

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				AssertNotNull(shipmentForm.PlugIns.GetPlugIn(ControllerIDs.DocAddresses));
			}
		}

		[RequiresSTA]
		public void TestTemplateRecordValidationStandard()
		{
			DataRegistry.Instance.RawRegistry.TemplateRecordValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.TemplateRecordValidationCodes.StandardValidation);
			AssertTemplateRecordValidation(true, false);
		}

		public void TestTemplateRecordValidationNone()
		{
			DataRegistry.Instance.RawRegistry.TemplateRecordValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.TemplateRecordValidationCodes.NoValidation);
			AssertTemplateRecordValidation(false, true);
		}

		[RequiresSTA]
		public void TestTemplateRecordValidationIgnoreAndSave()
		{
			DataRegistry.Instance.RawRegistry.TemplateRecordValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.TemplateRecordValidationCodes.IgnoreAndSave);
			AssertTemplateRecordValidation(true, true, DialogResult.Ignore);
		}

		[RequiresSTA]
		public void TestTemplateRecordValidationIgnoreAndSaveAbort()
		{
			DataRegistry.Instance.RawRegistry.TemplateRecordValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.TemplateRecordValidationCodes.IgnoreAndSave);
			AssertTemplateRecordValidation(true, false, DialogResult.Abort);
		}

		void AssertTemplateRecordValidation(bool expectValidation, bool expectSave, DialogResult errorDialogResult = DialogResult.OK)
		{
			var factory = new TemplateRecordBusinessObjectFactory();
			var shipment = factory.New<ForwardingShipment>();
			factory.TemplateRecordProvider = shipment;

			var templateRecord = factory.TemplateRecordFactory.New<StmTemplateRecord>();
			var templateRecordProvider = (ITemplateRecordProvider)shipment;
			templateRecordProvider.TemplateRecord = templateRecord;
			templateRecordProvider.IsTemplateRecord = true;

			shipment.JS_TransportMode = "XYZ";
			AssertEquals(expectValidation, shipment.JS_TransportModeInfo.HasErrors());

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentFormForTest(shipment))
			{
				shipmentForm.ShowErrorsDialogCloseDialogResultForTest = errorDialogResult;

				AssertEquals(true, shipment.HasChanges);
				AssertEquals(false, templateRecord.IsInDatabase);
				AssertEquals(ODisplayMode.Edit, shipmentForm.DisplayMode);

				shipmentForm.FireSaveButton();

				AssertEquals(!expectSave, shipment.HasChanges);
				AssertEquals(expectSave, templateRecord.IsInDatabase);
				AssertEquals(expectSave ? ODisplayMode.Browse : ODisplayMode.Edit, shipmentForm.DisplayMode);
			}
		}

		[RequiresSTA]
		public void TestGetBusinessEntityForHasChanges()
		{
			var bizObj = Factory.New<ForwardingShipment>();

			ChildEditableService.SetState(bizObj.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentFormForTest(bizObj))
			{
				CombineAssertions("Should just return base entity for has changes", () =>
				{
					AssertEquals(bizObj, form.BusinessEntityForHasChanges);
					Assert(bizObj.IsTopLevel);
				});
			}
		}

		[RequiresSTA]
		public void TestGetBusinessEntityForHasChanges_TemplateRecord()
		{
			var factory = new TemplateRecordBusinessObjectFactory();
			var shipment = factory.New<ForwardingShipment>();
			factory.TemplateRecordProvider = shipment;

			var templateRecord = factory.TemplateRecordFactory.New<StmTemplateRecord>();
			var templateRecordProvider = (ITemplateRecordProvider)shipment;
			templateRecordProvider.TemplateRecord = templateRecord;
			templateRecordProvider.IsTemplateRecord = true;

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentFormForTest(shipment))
			{
				CombineAssertions("Template record exists on parent, should be the top level object for form", () =>
				{
					AssertEquals(templateRecord, form.BusinessEntityForHasChanges);
					Assert(!shipment.IsTopLevel);
					Assert(templateRecord.IsTopLevel);
				});
			}

			templateRecordProvider.TemplateRecord = null;

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentFormForTest(shipment))
			{
				CombineAssertions("Is a top level template provider but record does not exist, so provider should be top", () =>
				{
					AssertEquals(shipment, form.BusinessEntityForHasChanges);
					Assert(shipment.IsTopLevel);
				});
			}
		}

		[RequiresSTA]
		public void TestGetBusinessEntityForValidation()
		{
			var bizObj = Factory.New<ForwardingShipment>();

			ChildEditableService.SetState(bizObj.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentFormForTest(bizObj))
			{
				CombineAssertions("Should just return base entity for has changes", () =>
				{
					AssertEquals(bizObj, form.BusinessEntityForValidation_Exposed);
					Assert(bizObj.IsTopLevel);
				});
			}
		}

		[RequiresSTA]
		public void TestGetBusinessEntityForValidation_TemplateRecord()
		{
			var factory = new TemplateRecordBusinessObjectFactory();
			var shipment = factory.New<ForwardingShipment>();
			factory.TemplateRecordProvider = shipment;

			var templateRecord = factory.TemplateRecordFactory.New<StmTemplateRecord>();
			var templateRecordProvider = (ITemplateRecordProvider)shipment;
			templateRecordProvider.TemplateRecord = templateRecord;
			templateRecordProvider.IsTemplateRecord = true;

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentFormForTest(shipment))
			{
				CombineAssertions("Template record exists on parent, should be the top level object for form", () =>
				{
					AssertEquals(templateRecord, form.BusinessEntityForValidation_Exposed);
					Assert(!shipment.IsTopLevel);
					Assert(templateRecord.IsTopLevel);
				});
			}

			templateRecordProvider.TemplateRecord = null;

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentFormForTest(shipment))
			{
				CombineAssertions("Is a top level template provider but record does not exist, so provider should be top", () =>
				{
					AssertEquals(shipment, form.BusinessEntityForValidation_Exposed);
					Assert(shipment.IsTopLevel);
				});
			}
		}

		#endregion

		#region eConversations

		public void TestEConversationsPlugIn_Visible()
		{
			AssertEConversationPlugInVisibility(true);
		}

		public void TestEConversationsPlugIn_Hidden()
		{
			AssertEConversationPlugInVisibility(false);
		}

		void AssertEConversationPlugInVisibility(bool registryValue)
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			using (GlowRegistry.Instance.NeoEnableConversations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			using (var form = new ShipmentFormForTest(shipment))
			{
				var eConverationPlugIn = form.PlugIns.GetPlugIn(ControllerIDs.eConversationPlugIn);

				if (registryValue)
				{
					AssertNotNull(eConverationPlugIn);
				}
				else
				{
					AssertNull(eConverationPlugIn);
				}
			}
		}

		#endregion

		#region TestSetBookingPartyDocumentaryAddressReadonly

		[RequiresSTA]
		public void TestSetBookingPartyDocumentaryAddressReadonly_NewShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				Assert("BookingPartyDocumentaryAddress is not readonly for new shipment",
					!shipment.BookingPartyDocumentaryAddress.ReadOnly);
			}
		}

		public void TestSetBookingPartyDocumentaryAddressReadonly_ExistingSEAShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				Assert("BookingPartyDocumentaryAddress is not readonly for saved shipment which is not a part of NVOCC electroning message exchange",
					!shipment.BookingPartyDocumentaryAddress.ReadOnly);
			}
		}

		public void TestSetBookingPartyDocumentaryAddressReadonly_ExistingSEAShipmentWithSTULog_ElectronicShippingInstruction() =>
			AssertSetBookingPartyDocumentaryAddressReadonly_ExistingSEAShipmentWithSTULog(true, new KeyValuePair<string, string>("NEW", ShipmentStatusList.Codes.ElectronicShippingInstruction));

		public void TestSetBookingPartyDocumentaryAddressReadonly_ExistingSEAShipmentWithSTULog_WebBooking() =>
			AssertSetBookingPartyDocumentaryAddressReadonly_ExistingSEAShipmentWithSTULog(true, new KeyValuePair<string, string>("NEW", ShipmentStatusList.Codes.WebBooking));

		public void TestSetBookingPartyDocumentaryAddressReadonly_ExistingSEAShipmentWithSTULog_ElectronicBooking() =>
			AssertSetBookingPartyDocumentaryAddressReadonly_ExistingSEAShipmentWithSTULog(true, new KeyValuePair<string, string>("NEW", ShipmentStatusList.Codes.ElectronicBooking));

		public void TestSetBookingPartyDocumentaryAddressReadonly_ExistingSEAShipmentWithSTULog_NonNVOCCLog() =>
			AssertSetBookingPartyDocumentaryAddressReadonly_ExistingSEAShipmentWithSTULog(false, new KeyValuePair<string, string>("ZZZ", "hello"));

		void AssertSetBookingPartyDocumentaryAddressReadonly_ExistingSEAShipmentWithSTULog(bool expectedBookingPartyAddressReadOnly, params KeyValuePair<string, string>[] stuLogParameters)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var log = shipment.Logs.AddNew(Events.StatusUpdated, stuLogParameters);
			Factory.Save();

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();

				AssertEquals($"BookingPartyDocumentaryAddress readonly for STU {log.SL_Reference}",
					expectedBookingPartyAddressReadOnly,
					shipment.BookingPartyDocumentaryAddress.ReadOnly);
			}
		}

		#endregion

		#region ComplianceRiskPlugin

		public void TestIncidentDefaultModuleOnComplianceRiskTab()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding, (form as ICustomerServiceMenuSectionCodeOverridable).SectionCode);

				form.MainTabControl.SelectTab("ComplianceRiskTabPage");
				AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.ComplianceWise, (form as ICustomerServiceMenuSectionCodeOverridable).SectionCode);
			}
		}

		public void TestComplianceRiskPlugin_Visible()
		{
			AssertComplianceRiskPluginVisibility(false);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				AssertComplianceRiskPluginVisibility(true);
			}

			void AssertComplianceRiskPluginVisibility(bool registryValue)
			{
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				using (var form = new ShipmentFormForTest(shipment))
				{
					var complianceRiskPlugin = form.PlugIns.GetPlugIn(ControllerIDs.ComplianceRiskPlugin);
					if (registryValue)
					{
						AssertNotNull(complianceRiskPlugin);
					}
					else
					{
						AssertNull(complianceRiskPlugin);
					}
				}
			}
		}

		#endregion

		#region BusinessEntityForValidation

		public void TestSave_BusinessEntityForValidation()
		{
			var old = Env.Registry.AllowManualShipmentEntry;
			Env.Registry.AllowManualShipmentEntry = true;
			try
			{
				var shipment = GetShipmentWithNoErrors();
				shipment.JS_UniqueConsignRef = ZString.Empty;
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
				using (var form = new ShipmentFormForTest(shipment))
				{
					form.ShowShipmentNumberEntryFormHandle = () => { form.ForceClose(); return true; };
					form.Show();
					form.FireSaveButton();

					AssertEquals(0, UnitTestUserNotification.Instance.ShownErrorKeys.Length);
					AssertNotSaved(shipment);
				}
			}
			finally
			{
				Env.Registry.AllowManualShipmentEntry = old;
			}
		}

		#endregion

		#region Test JobDocAddress not sync

		OrgHeader CreateConsignorConsigneeOrgHeader(string code)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = "AUMEL";
			orgHeader.OH_Code = code;
			orgHeader.OH_IsConsignor = true;
			orgHeader.OH_IsConsignee = true;
			orgHeader.OH_FullName = $"{code} FullName";
			orgHeader.MainAddress.OA_Address1 = $"{code} Address";
			return orgHeader;
		}

		void InputCodeBox(ZForm form, ZArchitecture.GUI.Internal.ZFindBoxUserControl.ZCodeBox codeBox, string orgHeaderCode)
		{
			codeBox.Focus();
			codeBox.Text = orgHeaderCode;
			KeySender.PostKeyDown(codeBox, Keys.Tab);
			form.FireSaveButton();
			Application.DoEvents();
		}

		ZTabPage OpenShipmentTab(ShipmentFormForTest form, string tabPageName)
		{
			var curentTabPage = FindTab(form, tabPageName);
			((ISupportSwitchTabPage)form).SwitchTabPage(tabPageName);
			return curentTabPage;
		}

		public void TestJobDocAddressNotRefreshAfterSettingEmptyGuid()
		{
			var consigneeOrg = CreateConsignorConsigneeOrgHeader("CONSIGNEE");
			var consignorOrg1 = CreateConsignorConsigneeOrgHeader("TE1CONMEL");
			var consignorOrg2 = CreateConsignorConsigneeOrgHeader("TE2CONMEL");
			Factory.Save();

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			shipment.ConsigneePK = consigneeOrg.PK;
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_GoodsDescription = "GoodsDescription";
			shipment.JS_UniqueConsignRef = "SydneySydney";
			shipment.JS_ReleaseType = shipment.Lookups.JS_ReleaseType_List[0].Code;

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consigneeOrg.PK;
			shipment.ConsignorPickupAddress.E2_AddressOverride = false;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			consol.Transports[0].JW_VoyageFlight = "QF512";
			consol.Transports[0].JW_ETD = ZDateTime.Today;
			consol.JK_RL_NKDischargePort = "USCHI";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			shipment.Consols.Add(consol);

			shipment.RunPreSaveValidation();
			AssertNoErrors("precondition: should have no errors", shipment);
			Factory.Save();

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				Application.DoEvents();

				_ = OpenShipmentTab(form, "ShipmentDetailsTabPage");
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignorOrg1.PK;
				form.FireSaveButton();
				Application.DoEvents();

				var pickupTabPage = OpenShipmentTab(form, "PickupTabPage");
				Application.DoEvents();

				var docAddressControl = (ZDocAddressControl)pickupTabPage.Controls.Find("ConsignorPickupDocAddressControl", true)[0];
				var pickupOrgFindBox = docAddressControl.Controls.Find("fOrganisationFindBox", true)[0] as ZOrganisationFindBox.Bare;
				var codeBox = pickupOrgFindBox.CodeBox;
				AssertEquals(true, codeBox.Visible);
				AssertEquals(consignorOrg1.OH_Code, codeBox.Text);

				shipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				form.FireSaveButton();
				Application.DoEvents();

				AssertEquals("", codeBox.Text);

				InputCodeBox(form, codeBox, consignorOrg2.OH_Code);
				InputCodeBox(form, codeBox, consignorOrg2.OH_Code);

				var additionalTabPage = OpenShipmentTab(form, "AdditionalTabPage");
				Application.DoEvents();

				var consolsGrid = (ZModuleButtonGrid)additionalTabPage.Controls.Find("ConsolModuleButtonGrid", true)[0];
				consolsGrid.InnerGrid.Select(0);
				var toolStrip = (ZToolStrip)consolsGrid.Controls.Find("toolStrip", true)[0];
				var editButton = (ZToolStripButton)toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true)[0];
				editButton.PerformClick();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();
				using (var shownForm = (ZForm)consolsGrid.LastShownZForm)
				{
					AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
					AssertEquals(true, shownForm is ConsolForm);
				}
			}
		}

		#endregion

		#region TestNoDataBindingExceptionThrownWhenJobDocAddressDeleting

		[ExpectNoExceptions]
		public void TestNoDataBindingExceptionThrownWhenJobDocAddressDeleting()
		{
			var consigneeOrg = CreateConsignorConsigneeOrgHeader("CONSIGNEE");
			var consignorOrg = CreateConsignorConsigneeOrgHeader("TE1CONMEL");
			Factory.Save();

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			shipment.ConsigneePK = consigneeOrg.PK;
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_GoodsDescription = "GoodsDescription";
			shipment.JS_UniqueConsignRef = "SydneySydney";
			shipment.JS_ReleaseType = shipment.Lookups.JS_ReleaseType_List[0].Code;

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consigneeOrg.PK;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = ZGuid.Empty;

			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignorOrg.PK;
			shipment.ConsignorPickupAddress.OrganisationPK = ZGuid.Empty;

			shipment.RunPreSaveValidation();
			Factory.Save();

			using (var form = new ShipmentFormForTest(shipment))
			{
				form.Show();
				_ = OpenShipmentTab(form, "ShipmentDetailsTabPage");
				var additionalTabPage = OpenShipmentTab(form, "AddressesTabPage");
				var addressGrid = (DocAddressGrid)additionalTabPage.Controls.Find("AddressGrid", true)[0];
				addressGrid.CurrentCell = new DataGridCell(0, 0);
				(addressGrid.DataSource as JobDocAddressCollectionForPlugin).Sort("AddressDescription", ListSortDirection.Descending);
				(addressGrid.DataSource as JobDocAddressCollectionForPlugin).Sort("AddressDescription", ListSortDirection.Ascending);
				addressGrid.CurrentCell = new DataGridCell(1, 0);
				addressGrid.ContextMenu.DoPopup();
				addressGrid.DeleteMenuItem.PerformClick();
				Application.DoEvents();
			}
		}

		#endregion

		#region TestElectronicBOLPluginVisibility

		public void TestElectronicBOLPluginVisibility()
		{
			Env.Security.MaintainShipmentAllowPublisheHBL.IsAllowed = true;
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "123456789");
			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			AssertElectronicBOLPluginVisibility("S00001001", Core.Constants.TransportModes.Sea, Constants.ShipmentReleaseTypes.OriginalReqSurrender,
												"", Core.Constants.TransportModes.Sea, Constants.ShipmentReleaseTypes.OriginalReqSurrender);

			AssertElectronicBOLPluginVisibility("S00001001", Core.Constants.TransportModes.Sea, Constants.ShipmentReleaseTypes.OriginalReqSurrender,
												"S00001001", Core.Constants.TransportModes.Air, Constants.ShipmentReleaseTypes.OriginalReqSurrender);

			AssertElectronicBOLPluginVisibility("S00001001", Core.Constants.TransportModes.Sea, Constants.ShipmentReleaseTypes.OriginalReqSurrender,
												"S00001001", Core.Constants.TransportModes.Sea, Constants.ShipmentReleaseTypes.ExpressBofL);

			void AssertElectronicBOLPluginVisibility(string beforeHouseBill, string beforeTransportMode, string beforeReleaseType, string afterHouseBill, string afterTransportMode, string afterReleaseType)
			{
				using (FreightRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
				{
					var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
					shipment.JS_HouseBill = beforeHouseBill;
					shipment.JS_TransportMode = beforeTransportMode;
					shipment.JS_ReleaseType = beforeReleaseType;

					ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
					using (var form = new ShipmentFormForTest(shipment))
					{
						form.Show();

						var electronicBOL = form.ElectronicMessagingTabControl.PlugIns.GetPlugIn(ControllerIDs.ElectronicBOL);
						AssertEquals(true, electronicBOL.Enabled);

						shipment.JS_HouseBill = afterHouseBill;
						shipment.JS_TransportMode = afterTransportMode;
						shipment.JS_ReleaseType = afterReleaseType;
						AssertEquals(false, electronicBOL.Enabled);
					}
				}
			}
		}

		#endregion

		#region TestShowAlertWhenEHBLRequestsPending

		public void TestShowAlertWhenEHBLRequestsPending_AmendmentRequest()
		{
			var infoMessage = "The Electronic House Bill in this Shipment has an open Amendment Request.\r\nTo action, please go to Electronic Messages > Electronic Bill Of Lading.";

			Env.Security.MaintainShipmentAllowPublisheHBL.IsAllowed = true;
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "123456789");
			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_HouseBill = "S00000872";
				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				shipment.JS_ElectronicBillOfLadingStatus = "OBA";
				Factory.Save();

				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

				using (var shipmentForm = new ShipmentFormForTest(shipment))
				{
					shipmentForm.InvokeFormOnShown_ForTest();
					AssertContains(infoMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
				}

				shipment.JS_ElectronicBillOfLadingStatus = "OBP";
				Factory.Save();

				using (var shipmentForm = new ShipmentFormForTest(shipment))
				{
					shipmentForm.InvokeFormOnShown_ForTest();
					AssertNotContains(infoMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestShowAlertWhenEHBLRequestsPending_SwitchedToPaper()
		{
			var infoMessage = "The Electronic House Bill in this Shipment has an unactioned 'Switched To Paper' Instruction.\r\nPlease print the Original House Bill of Lading on paper.";

			Env.Security.MaintainShipmentAllowPublisheHBL.IsAllowed = true;
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "123456789");
			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_HouseBill = "S00000872";
				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				shipment.JS_ElectronicBillOfLadingStatus = "OBA";

				var switchedToPaperEventParameters = new[]
				{
					new KeyValuePair<string, string>(Params.Type, Core.Constants.BillStatusUpdatedTypes.SwitchedToPaper),
					new KeyValuePair<string, string>(Params.Department, ElectronicBOLConstants.EHBLEventDepartments.TitleRegistry)
				};
				shipment.Logs.CreateOrRecreateEventLog(Events.BillStatusUpdated, EstimateActual.Actual, new ZDateTimeOffset(2025, 02, 10, 0, 0, 1), string.Empty, switchedToPaperEventParameters);

				Factory.Save();

				AssertEquals(FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper, shipment.JS_ElectronicBillOfLadingStatus);

				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

				using (var shipmentForm = new ShipmentFormForTest(shipment))
				{
					shipmentForm.InvokeFormOnShown_ForTest();
					AssertContains(infoMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
				}

				shipment.Logs.CreateOrRecreateEventLog(Events.DocumentDelivered, EstimateActual.Actual, new ZDateTimeOffset(2025, 02, 11, 0, 0, 1), string.Empty,
					new[]
					{
						new KeyValuePair<string, string>(Params.Name, "Bill Of Lading"),
						new KeyValuePair<string, string>(Params.Type, "Original")
					});

				Factory.Save();

				AssertEquals(FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper, shipment.JS_ElectronicBillOfLadingStatus);

				using (var shipmentForm = new ShipmentFormForTest(shipment))
				{
					shipmentForm.InvokeFormOnShown_ForTest();
					AssertNotContains(infoMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}

				shipment.Logs.CreateOrRecreateEventLog(Events.BillStatusUpdated, EstimateActual.Actual, new ZDateTimeOffset(2025, 02, 12, 0, 0, 1), string.Empty, switchedToPaperEventParameters);

				Factory.Save();

				using (var shipmentForm = new ShipmentFormForTest(shipment))
				{
					shipmentForm.InvokeFormOnShown_ForTest();
					AssertContains(infoMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#endregion

		public void TestShipmentWithNoContainer_ChangesToConfirmsTab_NoErrorSeen_HasChangesShoulNotBeSetAfterTransactionIsCommitted()
		{
			RatingDataRegistry.Instance.ShouldShowAutoRatingNotRunWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEHAM";

			var objectCreator = new TestObjectCreator(Factory);
			var consignee = objectCreator.CreateOrgHeader("SIGNEE", false, false);
			consignee.OH_IsConsignee = true;
			var consignor = objectCreator.CreateOrgHeader("SIGNOR", false, false);
			consignor.OH_IsConsignor = true;

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_ActualWeight = 1m;
			shipment1.JS_UnitOfWeight = "T";
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "DEHAM";
			shipment1.JS_PackingMode = ContainerModes.LCL;
			shipment1.JS_ReleaseType = ShipmentReleaseTypes.OriginalReq;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			carrier.OH_Code = "ABC";
			Factory.Save();

			consol.Shipments.Add(shipment1);
			Factory.Save();

			var line1 = shipment1.OuterPackLines.AddNew();
			var line2 = shipment1.OuterPackLines.AddNew();
			Factory.Save();

			var confirm = shipment1.PickupConfirms.AddNew();
			confirm.FillWithValidTestData();
			confirm.EU_GoodsSignForBy = "Test";
			Factory.Save();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			vessel.RV_Name = "Vessel Test 1";
			Factory.Save();

			var leg = shipment1.Transports.AddNew();
			leg.JW_RL_NKLoadPort = "AUSYD";
			leg.JW_RL_NKDiscPort = "SGSIN";
			leg.JW_TransportMode = Core.Constants.TransportModes.Sea;
			leg.JW_VoyageFlight = "VY1";
			leg.JW_IsLinked = true;
			leg.JW_OA_CarrierAddress = carrier.MainAddress.PK;
			leg.CarrierPK = carrier.PK;
			leg.JW_Vessel = vessel.RV_FK;
			leg.JW_ETD = DateTime.Today;
			Factory.Save();

			shipment1.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var shipment2 = newFactory.Load<ForwardingShipment>(shipment1.PK);
			var consol2 = shipment2.Consols[0];
			ChildEditableService.SetState(shipment2.Factory, ChildEditableServiceStates.Shipment);
			Factory.Save();

			var errorReporter = new Mock<IErrorReporter>();
			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(true))
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporter.Object))
			using (new DisposableAction(() => Globals.IsTest_ForTest.ResetValue()))
			using (var form = new ShipmentFormForTest(shipment2))
			{
				form.Show();

				Env.Security.PickupDeliveryConfirmationsNew.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Application.DoEvents();

				var pickupTabPage = OpenShipmentTab(form, "PickupTabPage");
				var control = (ZTabControl)form.PickupTabPage.Controls.Find("PickupTabControl", true)[0];
				var tabDetails = (ZTabPage)form.PickupTabPage.Controls.Find("PickupDetailsTabPage", true)[0];
				var tabConfirms = (ZTabPage)form.PickupTabPage.Controls.Find("ConfirmationsPluginTabPage", true)[0];

				control.SelectTab(tabConfirms);
				var pickUpConfirmsGrid = tabConfirms.Controls.Find("ShipmentPickUpConfirmControl", true)[0];
				shipment2.PickupConfirms[0].EU_GoodsSignForBy = "TestModified";
				Application.DoEvents();

				var additionalTabPage = OpenShipmentTab(form, "AdditionalTabPage");
				var consolsGrid = (ZModuleButtonGrid)additionalTabPage.Controls.Find("ConsolModuleButtonGrid", true)[0];
				consolsGrid.InnerGrid.Select(0);
				var toolStrip = (ZToolStrip)consolsGrid.Controls.Find("toolStrip", true)[0];
				var editButton = (ZToolStripButton)toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true)[0];
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();

				AssertNoErrors("consol:", consol2);
				AssertNoErrors("shipment", shipment2);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				try
				{
					Globals.IsTest_ForTest.Value = false;
					AssertNoExceptionThrown(editButton.PerformClick);
					AssertEquals("The form must be saved before a Consol can be edited or a new Consol can be created. Do you wish to save the form?",
						UnitTestUserNotification.Instance.LastMessage.Text);
				}

				finally
				{
					var key = "HasChangesShouldNotBeSetWhileTransactionIsCommiting_2";
					var errorMessage = "HasChanges should not be set after factory transaction is committed. Inform IL team";
					errorReporter.Verify(x => x.Report(key, errorMessage, It.IsAny<Exception>()), Times.Never);

					using (var shownForm = (ZForm)consolsGrid.LastShownZForm)
					{
						AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
						AssertEquals(true, shownForm is ConsolForm);
					}
				}
			}
		}

		#region TestTransitPackage

		class ReadOnlyPackageForLink : ITransitPackage
		{
			readonly PkgPackage package;

			public ReadOnlyPackageForLink(PkgPackage package)
			{
				this.package = package;
			}

			public PkgPackage Package => package;

			public ZInt PackageQty => Package?.KP_PackageQty ?? ZInt.Zero;

			public ZString PackType => Package?.KP_F3_NKPackType ?? ZString.Empty;

			public ZString PackageID => Package?.KP_PackageID ?? ZString.Empty;

			public ZDecimal Length => Package?.KP_Length ?? ZDecimal.Zero;

			public ZDecimal Width => Package?.KP_Width ?? ZDecimal.Zero;

			public ZDecimal Height => Package?.KP_Height ?? ZDecimal.Zero;

			public ZDecimal Weight => Package?.KP_Weight ?? ZDecimal.Zero;

			public ZDecimal Volume => Package?.KP_Volume ?? ZDecimal.Zero;

			public ZString DimensionUQ => Package?.KP_DimensionUQ ?? ZString.Empty;

			public ZString WeightUQ => Package?.KP_WeightUQ ?? ZString.Empty;

			public ZString VolumeUQ => Package?.KP_VolumeUQ ?? ZString.Empty;

			public ZString MarksAndNumbers => Package?.KP_MarksAndNumbers ?? ZString.Empty;

			public ZString GoodsDescription => Package?.KP_GoodsDescription ?? ZString.Empty;

			public ZBool RequiresTemperatureControl => Package?.KP_RequiresTemperatureControl ?? ZBool.False;

			public ZDecimal RequiredTemperatureMinimum => Package?.KP_RequiredTemperatureMinimum ?? ZDecimal.Zero;

			public ZDecimal RequiredTemperatureMaximum => Package?.KP_RequiredTemperatureMaximum ?? ZDecimal.Zero;

			public ZString RequiredTemperatureUnit => Package?.KP_RequiredTemperatureUnit ?? ZString.Empty;

			public ZString CommodityCode => Package?.KP_RH_NKCommodityCode ?? ZString.Empty;

			public ZString HSCode => Package?.KP_HSCode ?? ZString.Empty;

			public ZDateTime UnloadTime { get; set; }

			public IEnumerable<IUNDGDataItem> UNDGDataItems => Package?.UNDGs ?? Enumerable.Empty<IUNDGDataItem>();

			public ZString RCN { get; set; }

			public IEnumerable<ICusEntryNumber> Numbers { get; set; }

			public IPkgPackage TransitPackage => Package;

			public ZBool IsDamaged { get; set; }

			public ZString DamagedReason { get; set; }

			public ZString UnitType { get; set; }

			public ZBool IsHighRisk { get; set; }

			public ZString ExternalReference => Package.KP_ExternalReference;

			public ZString OverriddenAviationSecurityInspectionType { get; set; }

			public static ReadOnlyPackageForLink CreatePkgPackageSample(PkgPackageJob packageJob,
				ZString packageID,
				string packType = "PLT",
				int packageQty = 1,
				decimal length = 11m,
				decimal width = 12m,
				decimal height = 10m,
				decimal volumn = 0m,
				decimal weight = 0m,
				string dimensionUQ = "M",
				string weightUQ = "KG",
				string volumeUQ = "M3",
				bool requiresTemperatureControl = false,
				decimal requiredTemperatureMaximum = 0m,
				decimal requiredTemperatureMinimum = 0m,
				string requiredTemperatureUnit = "C",
				string goodsDescription = "GD",
				string marksAndNumbers = "MN1",
				bool isDamaged = false,
				string damagedReason = "",
				IEnumerable<UNDGDataItem> undgs = null,
				IEnumerable<PkgPackageScreening> screenings = null,
				string unitType = "PKG",
				bool isHighRisk = false,
				string externalReference = "",
				string overriddenAviationSecurityInspectionType = "",
				ZGuid parentPackagePK = new ZGuid())
			{
				var pkg = packageJob.Packages.AddNew();
				pkg.KP_PackageID = packageID;
				pkg.KP_F3_NKPackType = packType;
				pkg.KP_PackageQty = packageQty;
				pkg.KP_GoodsDescription = goodsDescription;
				pkg.KP_Height = height;
				pkg.KP_Length = length;
				pkg.KP_Weight = weight;
				pkg.KP_Width = width;
				pkg.KP_DimensionUQ = dimensionUQ;
				pkg.KP_WeightUQ = weightUQ;
				pkg.KP_VolumeUQ = volumeUQ;
				pkg.KP_Volume = volumn;
				pkg.KP_MarksAndNumbers = marksAndNumbers;

				pkg.KP_RequiredTemperatureMaximum = requiredTemperatureMaximum;
				pkg.KP_RequiredTemperatureMinimum = requiredTemperatureMinimum;
				pkg.KP_RequiredTemperatureUnit = requiredTemperatureUnit;
				pkg.KP_RequiresTemperatureControl = requiresTemperatureControl;

				pkg.KP_HSCode = "HSCode";
				pkg.KP_RH_NKCommodityCode = "GEN";

				pkg.KP_IsDamaged = isDamaged;
				pkg.KP_DamagedReason = damagedReason;
				pkg.UNDGs.AddRange(undgs ?? Enumerable.Empty<UNDGDataItem>());
				pkg.Screenings.AddRange(screenings ?? Enumerable.Empty<PkgPackageScreening>());
				pkg.KP_ExternalReference = externalReference;
				pkg.KP_KP_ParentPackage = parentPackagePK;

				return new ReadOnlyPackageForLink(pkg)
				{
					RCN = packageJob.KJ_JobID,
					Numbers = new List<ICusEntryNumber>(),
					UnitType = unitType,
					IsHighRisk = isHighRisk,
					OverriddenAviationSecurityInspectionType = overriddenAviationSecurityInspectionType
				};
			}
		}

		#endregion

		#region Implementation

		bool rawEnableComplianceRisk;
		EnableComplianceWiseRegistryBusinessObject rawFreightComplianceWiseRegistry;

		protected override void SetUp()
		{
			base.SetUp();
			rawEnableComplianceRisk = RawDataRegistry.Instance.EnableComplianceRisk.Value;
			rawFreightComplianceWiseRegistry = FreightRegistry.Instance.FreightEnableComplianceWise.DefaultValue;

			FreightRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false));
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			base.TearDown();
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawEnableComplianceRisk);
			FreightRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawFreightComplianceWiseRegistry);
		}

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}

		(OrgHeader org, ForwardingConsol consol) CreateOrgAndConsol(string loadPort)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.Addresses.AddNewMainAddress();

			var agent = org.CarrierAppointedAgentPorts_Agency.AddNew();
			agent.O5_OA_AgentOfficeAddress = org.MainAddress.PK;
			agent.O5_PortOrCountry = loadPort;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = agent.O5_PortOrCountry;

			return (org, consol);
		}

		SecurityCore GetTemporarySecurityCore()
		{
			return new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
		}

		IDisposable SetupLicenceForEManifest()
		{
			Enterprise.Registry.Business.HVLVDataRegistry.HasHVLVClearance = true;
			return null;
		}

		IDisposable SetupLicenceForETail()
		{
			return null;
		}

		protected override ZForm GetBoundForm()
		{
			ZForm form = GetNewZShipmentForm();
			form.Show();

			return form;
		}

		ShipmentFormForTest GetNewZShipmentForm()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			Shipment = Factory.New<ForwardingShipment>();
			return new ShipmentFormForTest(Shipment);
		}

		ForwardingShipment Shipment;

		ForwardingShipment GetShipmentWithNoErrors()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			shipment.ConsignorPK = consignor.PK;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKDestination = "INBOM";
			shipment.JS_GoodsDescription = "GoodsDescription";
			shipment.JS_UniqueConsignRef = "BlahBlahBlah";
			shipment.JS_ReleaseType = shipment.Lookups.JS_ReleaseType_List[0].Code;

			shipment.RunPreSaveValidation();
			AssertNoErrors("precondition: should have no errors", shipment);

			return shipment;
		}

		MenuItem FindMenuItem(ShipmentForm form, params string[] menuItemNames)
		{
			IEnumerable<MenuItem> menuItems = form.Menu.MenuItems.Cast<MenuItem>();
			MenuItem menuItem = null;

			foreach (var menuItemName in menuItemNames)
			{
				menuItem = menuItems.FirstOrDefault(mi => mi.Text == menuItemName);

				if (menuItem == null)
				{
					break;
				}

				menuItem.OnPopup(EventArgs.Empty);
				menuItems = menuItem.MenuItems.Cast<MenuItem>();
			}

			return menuItem;
		}

		MenuItem FindActionMenuItem(ShipmentForm form)
		{
			return FindMenuItem(form, "Actio&ns");
		}

		Form GetEditFormToBash()
		{
			var factory = new BusinessObjectFactory();
			ChildEditableService.SetState(factory, ChildEditableServiceStates.Shipment);
			var bO = factory.New<ForwardingShipment>();
			bO.Consols.AddNew();

			factory.Save();

			return new ShipmentForm(bO);
		}

		#region Test classes

		class ShipmentFormForTest : ShipmentForm
		{
			public ShipmentFormForTest(ForwardingShipment bO)
				: base(bO)
			{
			}

			public new ZTemplateTabControl MainTabControl
			{
				get { return base.MainTabControl; }
			}

			public new ContinueWithSave ShowPreSaveDialogs()
			{
				return base.ShowPreSaveDialogs();
			}

			public new ContinueWithSave ValidateAndSave()
			{
				return base.ValidateAndSave();
			}

			protected override bool ShowShipmentNumberEntryForm()
			{
				if (ShowShipmentNumberEntryFormHandle != null)
				{
					return ShowShipmentNumberEntryFormHandle();
				}
				else
				{
					return base.ShowShipmentNumberEntryForm();
				}
			}

			public void InvokeFormOnShown_ForTest()
			{
				base.OnShown(EventArgs.Empty);
			}

			public void InvokeSaveInternal_ForTest()
			{
				base.SaveInternal();
			}

			public Func<bool> ShowShipmentNumberEntryFormHandle;

			public new MenuItem ActionsMenuItem
			{
				get { return base.ActionsMenuItem; }
			}

			public new ZStmALogAddForm NewRaiseEventLogForm(Event @event)
			{
				return base.NewRaiseEventLogForm(@event);
			}

			public new MainMenu MainMenu
			{
				get { return base.MainMenu; }
			}

			public new ZTabControl ElectronicMessagingTabControl
			{
				get { return base.ElectronicMessagingTabControl; }
			}

			public new ZTabPage ShipmentDetailsTabPage
			{
				get { return base.ShipmentDetailsTabPage; }
			}

			public new ZTabPage DeliveryTabPage
			{
				get { return base.DeliveryTabPage; }
			}

			public new ZTabPage PickupTabPage
			{
				get { return base.PickupTabPage; }
			}

			public new ZTabPage AdditionalTabPage
			{
				get { return base.AdditionalTabPage; }
			}

			public ZTabPage EventTabPageForTest => EventTabPage;

			public new ZButton ViewShipmentTrackingButton
			{
				get { return base.ViewShipmentTrackingButton; }
			}

			public IBusiness BusinessEntityForValidation_Exposed => base.BusinessEntityForValidation;

			#region Exposing Export functionality

			protected override XmlDataTransferExporter GetNewXmlDataTransferExporter(IValueObjectDataAdapter adapter, bool checkForLicence)
			{
				LastExportTransferExporterForTest = new XmlDataTransferExporterForTest(adapter);
				return LastExportTransferExporterForTest;
			}

			public new void StoreAsShipmentMenuClick(object sender, EventArgs e)
			{
				base.StoreAsShipmentMenuClick(sender, e);
			}

			public new void StoreAsDeclarationMenuClick(object sender, EventArgs e)
			{
				base.StoreAsDeclarationMenuClick(sender, e);
			}

			public XmlDataTransferExporterForTest LastExportTransferExporterForTest;

			#endregion
		}

		class XmlDataTransferExporterForTest : XmlDataTransferExporter
		{
			public XmlDataTransferExporterForTest(IValueObjectDataAdapter adapter)
				: base(adapter, false)
			{
			}

			public new IValueObjectDataAdapter Adapter
			{
				get { return base.Adapter; }
			}
		}

		#endregion

		MenuItem GetChildMenuItemByTextSingleOrDefault(MenuItem parentMenuItem, string text) => parentMenuItem?.MenuItems.Cast<MenuItem>().SingleOrDefault(mi => mi.Text == text);

		#endregion
	}
}
