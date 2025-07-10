using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.FormExtensions;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal sealed class BillOfLadingFormTest : BaseAgencyTest
	{
		public void TestDtbTransportBooking()
		{
			var bill = Factory.New<BillOfLading>();

			using (BillOfLadingForm form = new BillOfLadingForm(bill))
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DtbBooking));
			}
		}

		public void TestDocumentVisualizer()
		{
			var bill = Factory.NewWithValidTestData<BillOfLading>();
			Factory.Save();

			using (var form = new BillOfLadingForm(bill))
			{
				form.Show();
				Application.DoEvents();

				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocumentVisualizer));

				var documentsMenuItem = form.Menu.MenuItems.FindByText("&Documents", true);
				documentsMenuItem.PerformClick();

				var customiseDocumentsMenuItem = documentsMenuItem.MenuItems.FindByText("Customize (Documents)", true);
				var customiseFormsMenuItem = documentsMenuItem.MenuItems.FindByText("Customize (Forms)", true);

				AssertNotNull(customiseDocumentsMenuItem);
				AssertNotNull(customiseFormsMenuItem);
			}
		}

		public void TestBillOfLadingDocumentAndForm()
		{
			var bill = Factory.NewWithValidTestData<BillOfLading>();
			Factory.Save();

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderLinerAndAgencyDocumentsOnly.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AgencyRegistry.Instance.UseNewFormBuilderBillOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new BillOfLadingForm(bill))
			{
				form.Show();
				Application.DoEvents();

				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocumentVisualizer));

				var documentsMenuItem = form.Menu.MenuItems.FindByText("&Documents", false);
				documentsMenuItem.PerformClick();

				var exportMenuItem = documentsMenuItem.MenuItems.FindByText("Export", false);
				exportMenuItem.PerformClick();

				var billOfLadingMenuItem = exportMenuItem.MenuItems.FindByText("Bill of Lading", false);

				AssertNotNull(billOfLadingMenuItem);

				var legacyDocumentsMenuItem = documentsMenuItem.MenuItems.FindByText("Legacy Documents", false);
				legacyDocumentsMenuItem.PerformClick();
				var legacyExportMenuItem = legacyDocumentsMenuItem.MenuItems.FindByText("Export", false);
				legacyExportMenuItem.PerformClick();
				var legacyBillOfLadingMenuItem = legacyExportMenuItem.MenuItems.FindByText("Bill of Lading", false);

				AssertNotNull(legacyBillOfLadingMenuItem);
			}

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderLinerAndAgencyDocumentsOnly.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AgencyRegistry.Instance.UseNewFormBuilderBillOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new BillOfLadingForm(bill))
			{
				form.Show();
				Application.DoEvents();

				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocumentVisualizer));

				var documentsMenuItem = form.Menu.MenuItems.FindByText("&Documents", false);
				documentsMenuItem.PerformClick();

				var exportMenuItem = documentsMenuItem.MenuItems.FindByText("Export", false);
				exportMenuItem.PerformClick();

				var billOfLadingMenuItem = exportMenuItem.MenuItems.FindByText("Bill of Lading", false);

				AssertNotNull(billOfLadingMenuItem);

				var legacyDocumentsMenuItem = documentsMenuItem.MenuItems.FindByText("Legacy Documents", false);
				legacyDocumentsMenuItem?.PerformClick();
				var legacyExportMenuItem = legacyDocumentsMenuItem?.MenuItems.FindByText("Export", false);
				legacyExportMenuItem?.PerformClick();
				var legacyBillOfLadingMenuItem = legacyExportMenuItem?.MenuItems.FindByText("Bill of Lading", false);

				AssertNull(legacyBillOfLadingMenuItem);
			}

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderLinerAndAgencyDocumentsOnly.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AgencyRegistry.Instance.UseNewFormBuilderBillOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new BillOfLadingForm(bill))
			{
				form.Show();
				Application.DoEvents();

				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocumentVisualizer));

				var documentsMenuItem = form.Menu.MenuItems.FindByText("&Documents", false);
				documentsMenuItem.PerformClick();

				var exportMenuItem = documentsMenuItem.MenuItems.FindByText("Export", false);
				exportMenuItem.PerformClick();

				var billOfLadingMenuItem = exportMenuItem.MenuItems.FindByText("Bill of Lading", false);

				AssertNotNull(billOfLadingMenuItem);

				var legacyDocumentsMenuItem = documentsMenuItem.MenuItems.FindByText("Legacy Documents", false);
				legacyDocumentsMenuItem?.PerformClick();
				var legacyExportMenuItem = legacyDocumentsMenuItem?.MenuItems.FindByText("Export", false);
				legacyExportMenuItem?.PerformClick();
				var legacyBillOfLadingMenuItem = legacyExportMenuItem?.MenuItems.FindByText("Bill of Lading", false);

				AssertNull(legacyBillOfLadingMenuItem);
			}

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderLinerAndAgencyDocumentsOnly.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AgencyRegistry.Instance.UseNewFormBuilderBillOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new BillOfLadingForm(bill))
			{
				form.Show();
				Application.DoEvents();

				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocumentVisualizer));

				var documentsMenuItem = form.Menu.MenuItems.FindByText("&Documents", false);
				documentsMenuItem.PerformClick();

				var exportMenuItem = documentsMenuItem.MenuItems.FindByText("Export", false);
				exportMenuItem.PerformClick();

				var billOfLadingMenuItem = exportMenuItem.MenuItems.FindByText("Bill of Lading", false);

				AssertNotNull(billOfLadingMenuItem);

				var legacyDocumentsMenuItem = documentsMenuItem.MenuItems.FindByText("Legacy Documents", false);
				legacyDocumentsMenuItem?.PerformClick();
				var legacyExportMenuItem = legacyDocumentsMenuItem?.MenuItems.FindByText("Export", false);
				legacyExportMenuItem?.PerformClick();
				var legacyBillOfLadingMenuItem = legacyExportMenuItem?.MenuItems.FindByText("Bill of Lading", false);

				AssertNull(legacyBillOfLadingMenuItem);
			}
		}

		public void TestBillOfLadingDocumentAndFormFilterMacro()
		{
			var billOfLadingFormQuery = new ZQuery()
				.AddToFilter(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "Bill of Lading")
				.AddToFilter(StmMenuItemSchema.SU_BusinessContext, nameof(CargoWise.Definitions.BusinessContext.AgencyDocumentation))
				.AddToFilter(StmMenuItemSchema.SU_MenuPath, "Export")
				.AddToFilter(StmMenuItemSchema.SU_MenuType, "FRM");
			var billOfLadingFormMenuItem = Factory.Load<StmMenuItem>(billOfLadingFormQuery);

			AssertEquals(1, billOfLadingFormMenuItem.Length);

			var menuItem = billOfLadingFormMenuItem.First();
			AssertContains($"{menuItem.SU_MenuName}|{menuItem.PK} - filter contains correct condition",
				"UseNewFormBuilderBillOfLading",
				menuItem.SU_FilterList);

			var billOfLadingDocumentQuery = new ZQuery()
				.AddToFilter(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "Bill of Lading")
				.AddToFilter(StmMenuItemSchema.SU_BusinessContext, nameof(CargoWise.Definitions.BusinessContext.AgencyDocumentation))
				.AddToFilter(StmMenuItemSchema.SU_MenuType, "DOC");
			var billOfLadingDocumentMenuItems = Factory.Load<StmMenuItem>(billOfLadingDocumentQuery);

			AssertEquals(2, billOfLadingDocumentMenuItems.Length);

			var billOfLadingDocumentMenuItem = billOfLadingDocumentMenuItems.First(x => x.SU_MenuPath == "Export");
			AssertContains($"{billOfLadingDocumentMenuItem.SU_MenuName}|{billOfLadingDocumentMenuItem.PK} - filter contains correct condition",
				"\"<UseNewFormBuilderBillOfLading>\" != \"Y\"",
				billOfLadingDocumentMenuItem.SU_FilterList);

			var legacyBillOfLadingDocumentMenuItem = billOfLadingDocumentMenuItems.First(x => x.SU_MenuPath == "Legacy Documents/Export");
			AssertContains($"{legacyBillOfLadingDocumentMenuItem.SU_MenuName}|{legacyBillOfLadingDocumentMenuItem.PK} - filter contains correct condition",
				"\"<UseNewFormBuilderBillOfLading>\" == \"Y\"",
				legacyBillOfLadingDocumentMenuItem.SU_FilterList);
		}

		public void TestPortAuthorityValidationIsRegistered()
		{
			var bill = Factory.New<BillOfLading>();

			using (var form = new BillOfLadingForm(bill))
			{
				AssertEquals("PortAuthority", true, PortAuthorityBusinessObjectValidation.IsRegisteredInFactory(Factory));
			}
		}

		[EIDOMessagingConfiguration(Enabled = true)]
		public void TestEIDOValidationIsRegistered()
		{
			MessagingValidationStrategyFactory.ResetAllStrategiesAndSetFilterCriteria(string.Empty);

			var bill = Factory.New<BillOfLading>();

			using (var form = new BillOfLadingForm(bill))
			{
				AssertEquals("EIDO", true, EIDOBusinessObjectValidation.IsRegisteredInFactory(Factory));
			}

			var xml = @"<Strategies>
  <Strategy>
    <MessageValidationStrategyType>Enterprise.Freight.Agency.Business.EIDOBusinessObjectValidation</MessageValidationStrategyType>
    <IsEnabled>true</IsEnabled>
  </Strategy>
</Strategies>";

			MessagingValidationStrategyFactory.ResetAllStrategiesAndSetFilterCriteria(xml);

			using (var form = new BillOfLadingForm(bill))
			{
				AssertEquals("EIDO", true, EIDOBusinessObjectValidation.IsRegisteredInFactory(Factory));
			}
		}

		[EIDOMessagingConfiguration(Enabled = false)]
		public void TestEIDOValidationIsNotRegistered()
		{
			MessagingValidationStrategyFactory.ResetAllStrategiesAndSetFilterCriteria(string.Empty);

			var bill = Factory.New<BillOfLading>();

			using (var form = new BillOfLadingForm(bill))
			{
				AssertEquals("EIDO", false, EIDOBusinessObjectValidation.IsRegisteredInFactory(Factory));
			}

			var xml = @"<Strategies>
  <Strategy>
    <MessageValidationStrategyType>Enterprise.Freight.Agency.Business.EIDOBusinessObjectValidation</MessageValidationStrategyType>
    <IsEnabled>false</IsEnabled>
  </Strategy>
</Strategies>";

			MessagingValidationStrategyFactory.ResetAllStrategiesAndSetFilterCriteria(xml);

			using (var form = new BillOfLadingForm(bill))
			{
				AssertEquals("EIDO", false, EIDOBusinessObjectValidation.IsRegisteredInFactory(Factory));
			}
		}

		public void TestSelectAndShowContainer_FCL()
		{
			var bill = Factory.New<BillOfLading>();
			bill.JS_PackingMode = Constants.ContainerModes.FCL;

			var container1 = bill.RealContainers.AddNew();
			var container2 = bill.RealContainers.AddNew();

			using (var form = new BillOfLadingForm(bill))
			{
				form.Show();
				Application.DoEvents();

				var containers = GetControl<BillOfLadingContainersPage>(form, "containersControl");
				var grid = GetControl<BillOfLadingContainersPage, ZGrid>(containers, "containersGrid");

				AssertEquals("hidden to begin with", false, grid.Visible);

				form.SelectAndShowContainer(container1.PK);
				AssertEquals("should be shown now", true, grid.Visible);
				AssertEquals("should have selected container1", container1, grid.ListManager.GetCurrent());

				form.SelectAndShowContainer(container2.PK);
				AssertEquals("should have selected container2", container2, grid.ListManager.GetCurrent());

				form.SelectAndShowContainer(ZGuid.Empty);
				form.SelectAndShowContainer(ZGuid.NewZGuid());
			}
		}

		public void TestSelectAndShowContainer_BreakBulk()
		{
			var bill = Factory.New<BillOfLading>();
			bill.JS_PackingMode = Constants.ContainerModes.BreakBulk;

			using (var form = new BillOfLadingForm(bill))
			{
				form.Show();
				Application.DoEvents();

				var containers = GetControl<BillOfLadingContainersPage>(form, "containersControl");
				var grid = GetControl<BillOfLadingContainersPage, ZGrid>(containers, "containersGrid");

				AssertEquals("hidden to begin with", false, grid.Visible);

				form.SelectAndShowContainer(ZGuid.Empty);
				AssertEquals("still not shown", false, grid.Visible);

				form.SelectAndShowContainer(ZGuid.NewZGuid());
				AssertEquals("still not shown", false, grid.Visible);
			}
		}

		public void TestShowPreSaveDialogs()
		{
			Env.Registry.PromptToSaveBuyerSupplier = false;

			var now = ZDateTime.Now;

			var principal = Factory.New<OrgHeader>();
			principal.OH_Code = "PPPNNNLLL";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			Factory.Save();

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CCCRRRCCC";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			var shipment = Factory.New<BillOfLading>();
			shipment.JS_NKLoadPort = "AUSYD";
			shipment.JS_NKDischargePort = "HKHKC";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.JS_OH_DeliveryAgent = principal.PK;
			shipment.JS_GoodsDescription = "description";
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;

			using (var form = new BillOfLadingForm(shipment))
			{
				form.Show();

				form.FireSaveButton();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				var voyage = Factory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
				voyage.JV_VoyageFlight = "x42";
				voyage.JV_OH_Line = NewCarrier().PK;

				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "AUSYD";
				origin.JA_E_DEP = now.AddDays(1);

				var destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = "HKHKC";
				destination.JB_E_ARV = now.AddDays(2);

				var sailing = voyage.Sailings[0];
				shipment.JS_JX = sailing.PK;

				form.FireSaveButton();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sailing.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;
				form.FireSaveButton();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUpdateShipmentTotals()
		{
			Env.Registry.PromptToSaveBuyerSupplier = false;

			var messageString = "Total vehicles, weight and volume do not match the shipment total. Would you like to update the booking to match the vehicles totals?";

			var principal = Factory.New<OrgHeader>();
			principal.OH_Code = "PPPPPPPPP";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			Factory.Save();

			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_PackingMode = Constants.ContainerModes.FCL;

			var container = billOfLading.RealContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerCount = 2;

			var packLine = billOfLading.OuterPackLines.AddNew();
			packLine.JL_Width = 1.2;
			packLine.JL_Height = 1.2;
			packLine.JL_Length = 1.2;
			packLine.JL_ActualVolume = 1.728;
			packLine.JL_PackageCount = 3;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CCCRRRCCC";
			consignor.OH_IsConsignor = true;
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			billOfLading.ConsigneePK = consignee.PK;
			billOfLading.ConsignorPK = consignor.PK;
			billOfLading.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			billOfLading.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			billOfLading.JS_OH_DeliveryAgent = principal.PK;
			billOfLading.JS_RL_NKOrigin = "AUSYD";
			billOfLading.JS_RL_NKDestination = "AUBNE";
			billOfLading.JS_GoodsDescription = "description";

			using (var form = new BillOfLadingForm(billOfLading))
			{
				form.Show();
				form.FireSaveButton();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				billOfLading.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;

				var vehicle = billOfLading.ShippingContainers.AddNew();
				vehicle.JC_GrossWeight = 10m;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();

				AssertEquals(messageString, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShipmentPackingModeChanging()
		{
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_PackingMode = Constants.ContainerModes.FCL;
			billOfLading.RealContainers.AddNew();

			using (var form = new BillOfLadingForm(billOfLading))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				billOfLading.JS_PackingMode = Constants.ContainerModes.BreakBulk;

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("Are you sure you want to continue?"));
				AssertEquals("User cancelled mode change", Constants.ContainerModes.FCL, billOfLading.JS_PackingMode);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				billOfLading.JS_PackingMode = Constants.ContainerModes.BreakBulk;

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("Are you sure you want to continue?"));
				AssertEquals("User confirmed mode change", Constants.ContainerModes.BreakBulk, billOfLading.JS_PackingMode);
			}
		}

		public void TestActionsMenuNotAvailableInViewMode()
		{
			ActionsMenuItemsHelperTest.AssertActionsMenuItemsNotAvailableInViewMode(new BillOfLadingForm(Factory.New<BillOfLading>()));
		}

		public void TestInitialiseRelatedJobsMenu()
		{
			var billOfLading = Factory.New<BillOfLading>();

			using (var form = new BillOfLadingForm(billOfLading))
			{
				form.Show();
				var iModuleToModuleForm = (IModuleToModuleForm<BillOfLading>)form;
				MenuAssertion.AssertHasMenu(form.Menu, iModuleToModuleForm.TopLevelMenuItemCaption, iModuleToModuleForm.MainMenuItemCaption);
			}
		}

		public void TestExportXML()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			using (var form = new BillOfLadingForm(Factory.New<BillOfLading>()))
			{
				var item = MenuAssertion.AssertHasMenu(form.Menu, "Actio&ns", "Export to XML (Verbose)");
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		public void TestSpecialTabVisibility_FCL()
		{
			GenericTabVisibilityTest(Constants.ContainerModes.FCL);
		}

		public void TestSpecialTabVisibility_BLK()
		{
			GenericTabVisibilityTest(Constants.ContainerModes.Bulk);
		}

		public void TestSpecialTabVisibility_ROR()
		{
			GenericTabVisibilityTest(Constants.ContainerModes.RollOnRollOff);
		}

		public void TestSpecialTabVisibility_ShouldBeSetUsingBeginInvokeOverwiseItWillBeRaceFromDelayedInvocationsFromZGrid()
		{
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;

			using (var form = new BillOfLadingForm(billOfLading))
			{
				form.Show();
				Application.DoEvents();

				var helper = new BillOfLadingForm.TestHelper(form);
				AssertEquals(true, helper.VehiclesVisible);
				AssertEquals(false, helper.ContainersVisible);

				var container = billOfLading.Vehicles.AddNew();
				container.JC_GrossWeightUQ = "XX";
				AssertEquals("Precondition", true, container.HasErrors);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				billOfLading.JS_PackingMode = Constants.ContainerModes.FCL;

				AssertEquals(true, helper.VehiclesVisible);
				AssertEquals(false, helper.ContainersVisible);

				Application.DoEvents();
				AssertEquals(false, helper.VehiclesVisible);
				AssertEquals(true, helper.ContainersVisible);
			}
		}

		public void TestSaving_BSRelationshipExists()
		{
			SetSailings();

			Sailing1.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;

			BillOfLading shipment = GetShipmentWithoutErrors(Sailing1);

			OrgSupplierBuyerLink link = shipment.Consignor.BuyerLinks.AddNew();
			link.OL_OH_Buyer = shipment.Consignee.PK;
			link.OL_RN_NKImporterCountry = shipment.JS_RL_NKDestination.Left(2);

			using (BillOfLadingForm form = new BillOfLadingForm(shipment))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.FireSaveButton();

				AssertEquals("None ", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Save should have succedded", false, shipment.HasChanges);
			}
		}

		public void TestSaving_AddBSRelationship()
		{
			SetSailings();

			Sailing1.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;

			BillOfLading shipment = GetShipmentWithoutErrors(Sailing1);
			AssertNull("precondition: link should not exist yet", GetBSRelationshipLink(shipment.Consignor, shipment.Consignee));

			using (BillOfLadingForm form = new BillOfLadingForm(shipment))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.FireSaveButton();

				AssertEquals("Question Do you wish to save this Supplier-Consignor/Buyer-Consignee relationship?", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Save should have succedded", false, shipment.HasChanges);

				OrgSupplierBuyerLink link = GetBSRelationshipLink(shipment.Consignor, shipment.Consignee);
				AssertNotNull("Should have created a link.", link);
				AssertEquals("Link should be saved.", true, link.IsInDatabase);
			}
		}

		public void TestSaving_DontAddBSRelationship()
		{
			SetSailings();

			Sailing1.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;

			BillOfLading shipment = GetShipmentWithoutErrors(Sailing1);
			AssertNull("precondition: link should not exist yet", GetBSRelationshipLink(shipment.Consignor, shipment.Consignee));

			using (BillOfLadingForm form = new BillOfLadingForm(shipment))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.FireSaveButton();

				AssertEquals("Question Do you wish to save this Supplier-Consignor/Buyer-Consignee relationship?", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Save should have succedded", false, shipment.HasChanges);

				OrgSupplierBuyerLink link = GetBSRelationshipLink(shipment.Consignor, shipment.Consignee);
				AssertNull("Should have created a link.", link);
			}
		}

		public void TestSaving_IncreaseAllocation()
		{
			SetSailings();

			Sailing1.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Sailing;
			var allocation = Sailing1.SlotAllocations.GetAllocation(ZGuid.Empty);
			allocation.SetAspect(AllocationAspectTypes.Tonnes, 20);
			allocation.E0_UseDefaultOverAllocation = false;
			allocation.E0_OverAllocationPercent = 0m;

			var shipment = GetShipmentWithoutErrors(Sailing1);
			AssertEquals("precondition: shipment should be an export", true, shipment.IsExport());

			shipment.JS_ActualWeight = 21;

			using (var form = new BillOfLadingForm(shipment))
			{
				form.Show();

				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				form.FireSaveButton();

				AssertNotNull("Should have shown the dialog.", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Should have shown the correct dialog.", typeof(AllocationAdjustmentDialog), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("Save should have succedded", false, shipment.HasChanges);
				AssertEquals("Should have increased the overallocation percent to 5%", 5m, allocation.E0_OverAllocationPercent);
			}
		}

		public void TestSaving_DontIncreaseAllocation()
		{
			SetSailings();

			Sailing1.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Sailing;
			var allocation = Sailing1.SlotAllocations.GetAllocation(ZGuid.Empty);
			allocation.SetAspect(AllocationAspectTypes.Tonnes, 20);
			allocation.E0_UseDefaultOverAllocation = false;
			allocation.E0_OverAllocationPercent = 0m;

			var shipment = GetShipmentWithoutErrors(Sailing1);
			AssertEquals("precondition: shipment should be an export", true, shipment.IsExport());

			shipment.JS_ActualWeight = 21;

			using (var form = new BillOfLadingForm(shipment))
			{
				form.Show();

				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

				form.FireSaveButton();

				AssertNotNull("Should have shown the dialog.", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Should have shown the correct dialog.", typeof(AllocationAdjustmentDialog), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("Save should not have succedded", true, shipment.HasChanges);
				AssertEquals("Should not have increased the overallocation percent", 0m, allocation.E0_OverAllocationPercent);
			}
		}

		public void TestSaving_NoVoyage()
		{
			Env.Registry.PromptToSaveBuyerSupplier = false;

			var shipment = GetShipmentWithoutErrors(null);

			using (var form = new BillOfLadingForm(shipment))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();

				AssertEquals("Save should have succedded", false, shipment.HasChanges);
				AssertEquals("Should not have displayed a dialog", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestSaving_VoyageNotLocked()
		{
			Env.Registry.PromptToSaveBuyerSupplier = false;

			SetSailings();

			Sailing1.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Sailing;

			var allocation = Sailing1.SlotAllocations.GetAllocation(ZGuid.Empty);
			allocation.SetAspect(AllocationAspectTypes.Tonnes, 20);

			var shipment = GetShipmentWithoutErrors(Sailing1);
			AssertEquals("Precondition: Shipment should be an export", true, shipment.IsExport());

			using (var form = new BillOfLadingForm(shipment))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();

				AssertEquals("Save should have succedded", false, shipment.HasChanges);
				AssertEquals("Should not have displayed a dialog", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestSaving_VoyageLocked()
		{
			SetSailings();

			Sailing1.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Sailing;
			Sailing1.SlotAllocations.GetAllocation(ZGuid.Empty).SetAspect(AllocationAspectTypes.Tonnes, 20);

			var shipment = GetShipmentWithoutErrors(Sailing1);
			AssertEquals("Precondition: Shipment should be an export", true, shipment.IsExport());

			var mutex = new AgencyAllocationMutex(Voyage);
			mutex.Lock();
			Assert("Precondition: the test needs to be holding the mutex", mutex.HasLock);

			try
			{
				using (var form = new BillOfLadingForm(shipment))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					form.FireSaveButton();
					var info = mutex.GetLockInfo();
					AssertEquals("Save should not have succedded", false, shipment.IsInDatabase);
					AssertEquals("Should have displayed a dialog", $"User {info.UserWithLock.GS_LoginName} has this voyage locked since {info.LockStartTime}. Do you want to release the existing lock?", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.FireSaveButton();
					AssertEquals("Save should not have succeeded", true, shipment.IsInDatabase);
				}
			}
			finally
			{
				mutex.Unlock();
			}
		}

		public void TestSaving_EmptyAllocation()
		{
			SetSailings();

			Sailing1.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;
			var shipment = GetShipmentWithoutErrors(Sailing1);
			AssertEquals("precondition: shipment should be an export", true, shipment.IsExport());

			Sailing1.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Sailing;
			var allocation = Sailing1.SlotAllocations.GetAllocation(ZGuid.Empty);
			allocation.SetAspect(AllocationAspectTypes.Tonnes, 0);

			using (var form = new BillOfLadingForm(shipment))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();

				AssertEquals("Save should not have succedded", false, shipment.IsInDatabase);
				AssertEquals("Should have displayed the errors dialog", "Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestSaving_InsufficientAllocation()
		{
			Env.Registry.PromptToSaveBuyerSupplier = false;

			SetSailings();
			Sailing1.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Sailing;

			var allocation = Sailing1.SlotAllocations.GetAllocation(ZGuid.Empty);
			allocation.SetAspect(AllocationAspectTypes.Tonnes, 8);

			var shipment = GetShipmentWithoutErrors(Sailing1);
			AssertEquals("precondition: shipment should be an export", true, shipment.IsExport());

			using (var form = new BillOfLadingForm(shipment))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();

				AssertEquals("Save should not have succedded", false, shipment.IsInDatabase);
				AssertEquals("Should have displayed the errors dialog", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertNotNull("Should have displayed a form", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Should have shown the correct form", typeof(AllocationAdjustmentDialog), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestDefaultWeightAndVolumeUnits()
		{
			Env.Registry.PromptToSaveBuyerSupplier = false;
			AgencyRegistry.Instance.DefaultBillWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Tonnes);
			AgencyRegistry.Instance.DefaultBillVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.CubicMetres);

			SetSailings();
			Sailing1.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;

			BillOfLading shipment = GetShipmentWithoutErrors(Sailing1);
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_UnitOfWeight = "";
			shipment.JS_UnitOfVolume = "";
			shipment.OuterPackLines.RemoveAndDeleteAll();

			using (BillOfLadingForm form = new BillOfLadingForm(shipment))
			{
				form.Show();

				form.FireSaveButton();

				AssertEquals("Should not have shown a dialog", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals(Constants.Volume.CubicMetres, shipment.JS_UnitOfVolume);
				AssertEquals(Constants.Weight.Tonnes, shipment.JS_UnitOfWeight);
			}
		}

		public void TestSaving_UpdatePackLines_Yes()
		{
			AssertShipmentUpdateFromTopLevelPacksOnSaving(DialogResult.Yes, true);
		}

		public void TestSaving_UpdatePackLines_No()
		{
			AssertShipmentUpdateFromTopLevelPacksOnSaving(DialogResult.No, false);
		}

		void AssertShipmentUpdateFromTopLevelPacksOnSaving(DialogResult dialogResult, bool shouldBeUpdated)
		{
			Env.Registry.PromptToSaveBuyerSupplier = false;

			SetSailings();
			Sailing1.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Sailing;
			Sailing1.SlotAllocations.GetAllocation(ZGuid.Empty).SetAspect(AllocationAspectTypes.Tonnes, 50);

			var shipment = GetShipmentWithoutErrors(Sailing1);
			AssertEquals("Precondation: Shipment should be an export", true, shipment.IsExport());

			shipment.JS_ActualWeight = 50;
			shipment.TopLevelPacks[0].JC_GrossWeight = 20;

			using (var form = new BillOfLadingForm(shipment))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(dialogResult);
				form.FireSaveButton();

				AssertEquals("Should have displayed a dialog", "Question Total packs, weight and volume do not match the shipment total. Would you like to update the shipment to match the packline totals?", UnitTestUserNotification.Instance.LastMessage.ToString());

				string message = string.Format("Shipment should {0} have been updated", shouldBeUpdated ? "" : "not");
				AssertEquals(message, shouldBeUpdated ? 20m : 50m, shipment.JS_ActualWeight);
			}
		}

		public void TestSaving_EIDOChangeWarning()
		{
			SetSailings();
			Sailing1.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;

			var containerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var shipment = GetShipmentWithoutErrors(Sailing1);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.RealContainers.RemoveAndDeleteAll();

			var containers = new BillOfLadingContainer[6];
			for (var i = 0; i < containers.Length; i++)
			{
				var message = Factory.New<EIDOMessage>();
				message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EIDO;
				message.EM_MessageSubType = EIDOMessageTypes.Codes.Original;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;

				containers[i] = shipment.RealContainers.AddNew();
				containers[i].JC_ContainerNum = "Container" + i;
				containers[i].JC_RC = containerType.PK;
				containers[i].JC_IsEmptyContainer = true;
				containers[i].Messages.Add(message);
			}

			Factory.Save();

			AssertEquals("Precondition: Shipment should be an export", true, shipment.IsExport());

			using (var form = new BillOfLadingForm(shipment))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				for (var i = 0; i < 5; i++)
				{
					containers[i].JC_SealNum = "Bob";
				}

				shipment.RunPreSaveValidation();
				AssertNoErrors("precondition:", shipment);
				form.FireSaveButton();

				const string expected1 =
					"Warning " +
					"The following containers affected by your changes have previously sent E-IDO messages. You should re-send the E-IDO messages.\r\n" +
					"\r\n" +
					"\u2022 CONTAINER0\r\n" +
					"\u2022 CONTAINER1\r\n" +
					"\u2022 CONTAINER2\r\n" +
					"\u2022 CONTAINER3\r\n" +
					"\u2022 CONTAINER4\r\n" +
					"";

				AssertMultilineASCIIEquals("", expected1, UnitTestUserNotification.Instance.LastMessage.ToString());

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				for (var i = 0; i < 6; i++)
				{
					containers[i].JC_SealNum = "Fread";
				}

				shipment.RunPreSaveValidation();
				AssertNoErrors("precondition:", shipment);
				form.FireSaveButton();

				const string expected2 =
					"Warning " +
					"Several containers affected by your changes have previously sent E-IDO messages. You should re-send the E-IDO messages.\r\n" +
					"";

				AssertMultilineASCIIEquals("", expected2, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestCustomFieldsTabVisible()
		{
			var bill = Factory.New<BillOfLading>();
			bill.JS_PackingMode = Constants.ContainerModes.FCL;

			using (var form = new BillOfLadingForm(bill))
			{
				form.Show();
				Application.DoEvents();
				var customfieldsTabPage = form.GetControl<ZTabPage>("customfieldsTabPage");
				AssertNotNull(customfieldsTabPage);
				AssertEquals("should be shown", true, customfieldsTabPage.TabVisible);

				var customFieldsControl = form.GetControl<ProcessTemplateCustomFieldsControl>("customFieldsControl");
				AssertNotNull(customFieldsControl);
			}
		}

		#region UpdateShipmentStatus

		public void TestUpdateShipmentStatus()
		{
			var bill = Factory.NewWithValidTestData<BillOfLading>();
			Factory.Save();
			bill.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicShippingInstruction;
			Factory.Save();

			AssertEquals(1, bill.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == Core.Constants.EventReferenceMessageTypes.ShipmentStatus));

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new BillOfLadingForm(bill))
			{
				UnitTestUserNotification.Instance.AddUserResponse("No Reason");

				bill.JS_ShipmentStatus = ShipmentStatusList.Codes.SIRejected;

				var msg = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals(2, bill.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
					AssertEquals("Message Prompt was fired", ZDialogResult.OK, msg.Answer);
					AssertEquals("Check message caption", "Rejection Reason", msg.Caption);
					AssertEquals("Check message text", "Please enter the reason of rejection.", msg.Text);
					AssertEquals("Status should have changed", ShipmentStatusList.Codes.SIRejected, bill.JS_ShipmentStatus);
					AssertEquals("A status changed event should have been logged", "|NEW=SIJ|OLD=ESI|RES=Shipping Instruction Rejected, No Reason|TYP=Shipment Status", bill.Logs.MostRecentLogByEventTime(Events.StatusUpdated).SL_Reference);
				});
			}
		}

		public void TestUpdateShipmentStatus_WhenUserSelectCancel()
		{
			var bill = Factory.NewWithValidTestData<BillOfLading>();
			Factory.Save();
			bill.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicShippingInstruction;
			Factory.Save();

			AssertEquals(1, bill.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
			var expectedLog = bill.Logs.MostRecentLogByEventTime(Events.StatusUpdated);

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new BillOfLadingForm(bill))
			{
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);

				bill.JS_ShipmentStatus = ShipmentStatusList.Codes.SIRejected;

				var msg = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals(1, bill.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
					AssertEquals("Message Prompt was fired", ZDialogResult.Cancel, msg.Answer);
					AssertEquals("Check message caption", "Rejection Reason", msg.Caption);
					AssertEquals("Check message text", "Please enter the reason of rejection.", msg.Text);
					AssertEquals("Status should not have changed", ShipmentStatusList.Codes.ElectronicShippingInstruction, bill.JS_ShipmentStatus);
					AssertEquals("No new Status changed event should have been logged", expectedLog, bill.Logs.MostRecentLogByEventTime(Events.StatusUpdated));
				});
			}
		}

		#endregion

		#region Implementation

		OrgSupplierBuyerLink GetBSRelationshipLink(OrgHeader consignor, OrgHeader consignee)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Supplier, consignor.PK);
			filter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Buyer, consignee.PK);

			return consignor.Factory.LoadTop1<OrgSupplierBuyerLink>(filter);
		}

		protected override void SetUp()
		{
			base.SetUp();

			// If this registry is true, it will cause tests to fail when trying to create an accounting invoicing job.
			// This is tested in Accounting.Business & Accounting.Gui and is not required here

			OrganisationsDataRegistry.Instance.UseARSettlementGroupCreditLimit.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>())).Returns(false);
			mock.Setup(m => m.APAccountGroup).Returns(Factory.NewWithValidTestData<OrgCreditorGroup>().PK.ToGuid());
			mock.Setup(m => m.ARAccountGroup).Returns(Factory.NewWithValidTestData<OrgDebtorGroup>().PK.ToGuid());
			thingo = ObjectFactory.Substitute(mock.Object);
		}

		IDisposable thingo;

		protected override void TearDown()
		{
			base.TearDown();
			thingo.Dispose();
		}

		void GenericTabVisibilityTest(ZString containerMode)
		{
			var shipment = Factory.New<BillOfLading>();
			shipment.JS_PackingMode = containerMode;

			var showContainerInfo = (containerMode == Constants.ContainerModes.FCL);
			var showVehiclesInfo = (containerMode == Constants.ContainerModes.RollOnRollOff);
			var showPacksInfo = !(showContainerInfo || showVehiclesInfo);

			var containerIndex = -1;
			var vehicleIndex = -1;
			var packsIndex = -1;

			using (var form = new BillOfLadingForm(shipment))
			{
				form.Show();
				Application.DoEvents();

				var helper = new BillOfLadingForm.TestHelper(form);

				AssertEquals("ContainerVisibility should be true if and only if the current container mode is FCL", showContainerInfo, helper.ContainersVisible);
				AssertEquals("VehicleVisibility should be true if and only if the current container mode is ROR", showVehiclesInfo, helper.VehiclesVisible);
				AssertEquals("PacksVisibility should be true if and only if neither ContainerVisibility nor PacksVisibility is true", showPacksInfo, helper.PacksVisible);

				containerIndex = helper.ContainerTabIndex;
				vehicleIndex = helper.VehicleTabIndex;
				packsIndex = helper.PacksTabIndex;

				helper.VehiclesVisible = false;
				helper.PacksVisible = false;
				helper.ContainersVisible = true;
				AssertEquals("The Container Tab should be in the tab controller", true, helper.ContainersTabIsInTheTabController);

				if (showContainerInfo)
				{
					AssertEquals("ContainerIndex should not change.", containerIndex, helper.ContainerTabIndex);
				}
				else
				{
					containerIndex = helper.ContainerTabIndex;
				}

				AssertEquals("The Vehicles Tab should not be in the tab controller", false, helper.VehiclesTabInTheTabController);
				AssertEquals("The Packs tab should not be in the tab controller", false, helper.PacksTabInTheTabController);

				helper.ContainersVisible = false;
				helper.VehiclesVisible = true;

				if (showVehiclesInfo)
				{
					AssertEquals("Vehicle Index should not change.", vehicleIndex, helper.VehicleTabIndex);
				}
				else
				{
					vehicleIndex = helper.VehicleTabIndex;
				}

				AssertEquals("The Container Tab should not be in the tab controller", false, helper.ContainersVisible);

				helper.VehiclesVisible = false;
				helper.PacksVisible = true;

				if (showPacksInfo)
				{
					AssertEquals("PacksIndex should not change.", packsIndex, helper.PacksTabIndex);
				}
				else
				{
					packsIndex = helper.PacksTabIndex;
				}

				AssertEquals("The Vehicles Tab should not be in the tab controller", false, helper.VehiclesTabInTheTabController);

				helper.PacksVisible = false;
				helper.ContainersVisible = true;
				AssertEquals("The ContainerTab should be put back where it was.", containerIndex, helper.ContainerTabIndex);

				helper.ContainersVisible = false;
				helper.VehiclesVisible = true;
				AssertEquals("The VehiclesTab should be put back where it was.", vehicleIndex, helper.VehicleTabIndex);

				helper.VehiclesVisible = false;
				helper.PacksVisible = true;
				AssertEquals("The PacksTab should be put back where it was.", vehicleIndex, helper.PacksTabIndex);
			}
		}

		BillOfLading GetShipmentWithoutErrors(JobSailing sailing)
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			var principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			Factory.Save();

			var shipment = Factory.New<BillOfLading>();
			shipment.JS_PackingMode = Constants.ContainerModes.Bulk;
			shipment.JS_UnitOfWeight = Constants.Weight.Tonnes;
			shipment.JS_OuterPacks = 1;
			shipment.JS_ActualWeight = 10;
			shipment.JS_JX = sailing == null ? ZGuid.Empty : sailing.PK;
			shipment.JS_GoodsDescription = "Blah";
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_OH_DeliveryAgent = principal.PK;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			shipment.RunPreSaveValidation();
			AssertNoErrors("Precondition: Should not have any errors. If there are errors then just modify the shipment to remove them.", shipment);

			return shipment;
		}

		JobVoyage Voyage;
		VoyageOrigin Origin1;
		VoyageOrigin Origin2;
		VoyageOrigin Origin3;
		JobSailing Sailing1;

		void SetSailings()
		{
			Voyage = Factory.New<JobVoyage>();
			Voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
			Voyage.JV_VoyageFlight = "x42";
			Voyage.JV_OH_Line = NewCarrier().PK;

			Origin1 = Voyage.Origins.AddNew();
			Origin1.JA_RL_NKPortOfLoading = "AUSYD";
			Origin1.JA_E_DEP = ZDateTime.Now.AddDays(10);

			Origin2 = Voyage.Origins.AddNew();
			Origin2.JA_RL_NKPortOfLoading = "AUBNE";
			Origin2.JA_E_DEP = ZDateTime.Now.AddDays(16);

			Origin3 = Voyage.Origins.AddNew();
			Origin3.JA_RL_NKPortOfLoading = "NZAKL";
			Origin3.JA_E_DEP = ZDateTime.Now.AddDays(22);

			var destination1 = Voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUBNE";
			destination1.JB_E_ARV = ZDateTime.Now.AddDays(14);

			var destination2 = Voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "NZAKL";
			destination2.JB_E_ARV = ZDateTime.Now.AddDays(20);

			var destination3 = Voyage.Destinations.AddNew();
			destination3.JB_RL_NKPortOfDischarge = "SGSIN";
			destination3.JB_E_ARV = ZDateTime.Now.AddDays(26);

			Sailing1 = Voyage.Sailings.GetSailingFromLoadAndDischarge(Origin1.JA_RL_NKPortOfLoading, destination2.JB_RL_NKPortOfDischarge);
		}

		T GetControl<T>(BillOfLadingForm form, string name)
			where T : Control
		{
			return GetControl<BillOfLadingForm, T>(form, name);
		}

		T GetControl<P, T>(P parent, string name)
			where P : Control
			where T : Control
		{
			return (T)typeof(P).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(parent);
		}

		#region ComplianceRiskPlugin

		public void TestComplianceRiskPlugin()
		{
			AssertComplianceRiskPluginVisibility(true);
			AssertComplianceRiskPluginVisibility(false);

			void AssertComplianceRiskPluginVisibility(bool registryValue)
			{
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Consol);
				var bill = Factory.New<BillOfLading>();

				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(registryValue)))
				using (LinerAgencyDataRegistry.Instance.LinerAgencyEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(registryValue)))
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
				using (var form = new BillOfLadingForm(bill))
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

		public void TestIncidentDefaultModuleOnComplianceRiskTab()
		{
			var bill = Factory.New<BillOfLading>();

			using (LinerAgencyDataRegistry.Instance.LinerAgencyEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new BillOfLadingForm(bill))
			{
				form.Show();
				AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency, (form as ICustomerServiceMenuSectionCodeOverridable).SectionCode);

				((ZTemplateTabControl)(typeof(BillOfLadingForm).GetField("MainTabControl", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form))).SelectTab("ComplianceRiskTabPage");
				AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.ComplianceWise, (form as ICustomerServiceMenuSectionCodeOverridable).SectionCode);
			}
		}

		#endregion

		#endregion
	}
}
