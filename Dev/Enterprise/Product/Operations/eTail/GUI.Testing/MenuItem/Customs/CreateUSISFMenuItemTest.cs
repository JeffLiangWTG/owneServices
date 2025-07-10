using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.eTail.Integration.HVLVConstants;

namespace Enterprise.eTail.GUI.Testing
{
	sealed class CreateUSISFMenuItemTest : BaseNeedAddApplicationLockHVLVMenuItemTest
	{
		public void TestMenuItemCreatedByDefault()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var uSISFMenuGroup = customsMenu.MenuItems.OfType<USISFMenuGroup>().Single();

				var menuItem = uSISFMenuGroup.MenuItems.OfType<CreateUSISFMenuItem>().Single();
				Assert("Create US ISF menuitem should be created by default", menuItem.Visible);
			}
		}

		public void TestMenuItemVisibility_DependOnTransportMode()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
			shipment.JS_RL_NKDestination = "USCHI";
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var uSISFMenuGroup = customsMenu.MenuItems.OfType<USISFMenuGroup>().Single();

				shipment.JS_TransportMode = TransportModes.Sea;
				uSISFMenuGroup.OnPopup(EventArgs.Empty);
				var createUSISFMenuItem = uSISFMenuGroup.MenuItems.OfType<CreateUSISFMenuItem>().Single();
				Assert("Show Create US ISF menuitem when transport mode is SEA", createUSISFMenuItem.Visible);

				shipment.JS_TransportMode = TransportModes.Air;
				uSISFMenuGroup.OnPopup(EventArgs.Empty);
				createUSISFMenuItem = uSISFMenuGroup.MenuItems.OfType<CreateUSISFMenuItem>().Single();
				Assert("Hide Create US ISF menuitem when transport mode is AIR", !createUSISFMenuItem.Visible);

				shipment.JS_TransportMode = TransportModes.Road;
				uSISFMenuGroup.OnPopup(EventArgs.Empty);
				createUSISFMenuItem = uSISFMenuGroup.MenuItems.OfType<CreateUSISFMenuItem>().Single();
				Assert("Hide Create US ISF menuitem when transport mode is not AIR or SEA", !createUSISFMenuItem.Visible);
			}
		}

		public void TestMenuItemVisibility_DependOnDestination()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
			shipment.JS_TransportMode = TransportModes.Sea;
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var uSISFMenuGroup = customsMenu.MenuItems.OfType<USISFMenuGroup>().Single();

				shipment.JS_RL_NKDestination = "USCHI";
				uSISFMenuGroup.OnPopup(EventArgs.Empty);
				var createUSISFMenuItem = uSISFMenuGroup.MenuItems.OfType<CreateUSISFMenuItem>().Single();
				Assert("Show Create US ISF menuitem when destination is US", createUSISFMenuItem.Visible);

				shipment.JS_RL_NKDestination = "AUSYD";
				uSISFMenuGroup.OnPopup(EventArgs.Empty);
				createUSISFMenuItem = uSISFMenuGroup.MenuItems.OfType<CreateUSISFMenuItem>().Single();
				Assert("Hide Create US ISF menuitem when destination is AU", !createUSISFMenuItem.Visible);
			}
		}

		public void TestMenuItemVisibility_GenPivot()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_RL_NKDestination = "USCHI";
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();

				customsMenu.OnPopup(EventArgs.Empty);
				var uSISFMenuGroup = customsMenu.MenuItems.OfType<USISFMenuGroup>().Single();
				var createUSISFMenuItem = uSISFMenuGroup.MenuItems.OfType<CreateUSISFMenuItem>().Single();
				Assert("Show Create US ISF menuitem", createUSISFMenuItem.Visible);

				HVLVMenuItemTestHelper.CreateGenPivotRecord(shipment, typeof(Enterprise.Integration.Customs.US.ISF.ICusISFHeader));

				uSISFMenuGroup.OnPopup(EventArgs.Empty);
				createUSISFMenuItem = uSISFMenuGroup.MenuItems.OfType<CreateUSISFMenuItem>().Single();
				Assert("Hide Create US ISF menuitem after Gen Pivot record was created", !createUSISFMenuItem.Visible);
			}
		}

		public void TestMenuItemAction_ShipmentSuspendDeclarationForDocuments()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S001";
				shipment.JS_RL_NKDestination = "USCHI";

				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_JS = shipment.PK;

				Factory.Save();

				AssertNotNull("precondition : shipment.DeclarationForDocuments is not null", shipment.DeclarationForDocuments);

				using (var form = new ZForm(shipment))
				{
					ShowFormAndClickOnCreateUSISMenuItem(form);

					var isfHeader = (ZFormModaliser.LastFormShownForTest as Customs.US.ISF.GUI.ISFForm).BusinessEntity;
					AssertEquals("isfHeader.BF_OH_Importer should be shipment.ConsigneePK but not declaration.JE_OH_Importer when suspend shipment.DeclarationForDocuments", shipment.ConsigneePK, isfHeader.BF_OH_Importer);
				}
			}
		}

		public void TestMenuItemAction_WhenShipmentHasChanges_ShowError()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S001";
				shipment.JS_RL_NKDestination = "USCHI";
				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					shipment.HasChanges = true;

					ShowFormAndClickOnCreateUSISMenuItem(form);

					var saveFirstMessage = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertEquals("Please save the form before create US Importer Security Filing.", saveFirstMessage);
				}
			}
		}

		public void TestMenuItemAction_WhenRegistryDisabledFilingForNonUSBranchAndNonUSBranch_ShowError()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			using (HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S001";
				shipment.JS_RL_NKDestination = "USCHI";
				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					ShowFormAndClickOnCreateUSISMenuItem(form);

					var saveFirstMessage = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertEquals(@"US Security Filings are not enabled for this branch.

To enable go to Registry -> Freight -> HVLV -> Customs -> United States of America -> Enable Security Filings for non USA based Companies", saveFirstMessage);
				}
			}
		}

		public void TestMenuItemAction_WhenRegistryEnabledFilingForNonUSBranchAndNonUSBranch_ShowISFForm()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			using (HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertMenuItemAction_ShouldShowISFForm();
			}
		}

		public void TestMenuItemAction_WhenRegistryEnabledFilingForNonUSBranchAndUSBranch_ShowISFForm()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			using (HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertMenuItemAction_ShouldShowISFForm();
			}
		}

		public void TestMenuItemAction_WhenRegistryDisabledFilingForNonUSBranchAndUSBranch_ShowISFForm()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			using (HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertMenuItemAction_ShouldShowISFForm();
			}
		}

		void AssertMenuItemAction_ShouldShowISFForm()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_RL_NKDestination = "USCHI";
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var menuHVLV = plugin.TopLevelMenu;
				menuHVLV.PerformSelect();

				var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var uSISFMenuGroup = customsMenu.MenuItems.OfType<USISFMenuGroup>().Single();
				var menuItem = uSISFMenuGroup.MenuItems.OfType<CreateUSISFMenuItem>().Single();

				Assert(menuItem.Visible);
				menuItem.PerformClick();

				var isfForm = ZFormModaliser.LastFormShownForTest as Customs.US.ISF.GUI.ISFForm;
				AssertNotNull(isfForm);

				var isfHeader = isfForm.BusinessEntity;
				Assert("would not save AMS Header when create", !isfHeader.IsInDatabase);
			}
		}

		[TestDate(2021, 2, 2, 2, 5, 0)]
		public void TestMenuItemAction_WillPopulateHVI_SecurityFilingFirstUsageTimeUtc()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S001";
				shipment.JS_RL_NKDestination = "USLAX";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment = consignmentHeader.Consignments.AddNew();
				consignment.HVC_WaybillNumber = "LessThan12";
				var item1 = consignment.Items.AddNew();
				var item2 = Factory.NewWithValidTestData<HVLVItem>();
				item2.HVI_JS_LoadedOnShipment = shipment.PK;

				Factory.Save();

				var factory2 = new BusinessObjectFactory();
				var loadedShipment = factory2.Load<ForwardingShipment>(shipment.PK);
				var loadedItem1 = factory2.Load<HVLVItem>(item1.PK);
				var loadedItem2 = factory2.Load<HVLVItem>(item2.PK);

				using (var form = new ZForm(loadedShipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var uSISFMenuGroup = customsMenu.MenuItems.OfType<USISFMenuGroup>().Single();
					var menuItem = uSISFMenuGroup.MenuItems.OfType<CreateUSISFMenuItem>().Single();
					Assert(menuItem.Visible);

					Assert("pre-condition item HVI_SecurityFilingFirstUsageTimeUtc should be empty", loadedItem1.HVI_SecurityFilingFirstUsageTimeUtc.IsEmpty);
					Assert("pre-condition item HVI_SecurityFilingFirstUsageTimeUtc should be empty", loadedItem2.HVI_SecurityFilingFirstUsageTimeUtc.IsEmpty);

					menuItem.PerformClick();

					var isf = ZFormModaliser.LastIBusinessShownOnDialogForTest;
					AssertNotNull(isf);
					isf.Factory.Save();

					loadedItem1.Reload();
					loadedItem2.Reload();

					CombineAssertions(() =>
					{
						AssertEquals(new ZDateTime(2021, 2, 2, 2, 5, 0), loadedItem1.HVI_SecurityFilingFirstUsageTimeUtc);
						AssertEquals(new ZDateTime(2021, 2, 2, 2, 5, 0), loadedItem2.HVI_SecurityFilingFirstUsageTimeUtc);
					});
				}
			}
		}

		[TestDate(2021, 2, 2, 2, 2, 2)]
		public void TestMenuItemAction_WillNotRepopulateHVI_SecurityFilingFirstUsageTimeUtc()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S002";
				shipment.JS_RL_NKDestination = "USLAX";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;
				var item1 = consignment.Items.AddNew();
				var item2 = Factory.NewWithValidTestData<HVLVItem>();
				item2.HVI_JS_LoadedOnShipment = shipment.PK;

				item1.HVI_SecurityFilingFirstUsageTimeUtc = new ZDateTime(2019, 5, 3);
				item2.HVI_SecurityFilingFirstUsageTimeUtc = new ZDateTime(2019, 5, 3);

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					ShowFormAndClickOnCreateUSISMenuItem(form);

					var isf = ZFormModaliser.LastIBusinessShownOnDialogForTest;
					AssertNotNull(isf);
					isf.Factory.Save();

					item1.Reload();
					item2.Reload();

					CombineAssertions("HVI_SecurityFilingFirstUsageTimeUtc should not be repopulated if a value already exists", () =>
					{
						AssertNotEquals(new ZDateTime(2021, 2, 2, 2, 5, 0), item1.HVI_SecurityFilingFirstUsageTimeUtc);
						AssertNotEquals(new ZDateTime(2021, 2, 2, 2, 5, 0), item2.HVI_SecurityFilingFirstUsageTimeUtc);
					});
				}
			}
		}

		public void TestMenuItemAction_WillPopulateHVI_LastUsageCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S001";
				shipment.JS_RL_NKDestination = "USLAX";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment = consignmentHeader.Consignments.AddNew();
				consignment.HVC_WaybillNumber = "LessThan12";
				var item1 = consignment.Items.AddNew();
				var item2 = Factory.NewWithValidTestData<HVLVItem>();
				item2.HVI_JS_LoadedOnShipment = shipment.PK;

				Factory.Save();

				var factory2 = new BusinessObjectFactory();
				var loadedShipment = factory2.Load<ForwardingShipment>(shipment.PK);
				var loadedItem1 = factory2.Load<HVLVItem>(item1.PK);
				var loadedItem2 = factory2.Load<HVLVItem>(item2.PK);

				using (var form = new ZForm(loadedShipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var uSISFMenuGroup = customsMenu.MenuItems.OfType<USISFMenuGroup>().Single();
					var menuItem = uSISFMenuGroup.MenuItems.OfType<CreateUSISFMenuItem>().Single();
					Assert(menuItem.Visible);

					menuItem.PerformClick();

					var isf = ZFormModaliser.LastIBusinessShownOnDialogForTest;
					AssertNotNull(isf);
					isf.Factory.Save();

					loadedItem1.Reload();
					loadedItem2.Reload();

					CombineAssertions(() =>
					{
						AssertEquals(ExpectedISFUsageCode, loadedItem1.HVI_LastUsageCode);
						AssertEquals(ExpectedISFUsageCode, loadedItem2.HVI_LastUsageCode);
					});
				}
			}
		}

		public void TestUsageCodeAndCategory()
		{
			AssertEquals("UsageCode", "USF", UsageCodes.USImporterSecurityFiling);
			AssertEquals("UsageCategory", "SEC", UsageCategories.LookupByUsageCode[ExpectedISFUsageCode]);
		}

		const string ExpectedISFUsageCode = "USF";

		public void TestMenuItemAction_ShouldShowISFForm_With_OneConsignment()
		{
			AssertISFForm(1, true);
		}

		public void TestMenuItemAction_ShouldShowRelatedJobs_With_MoreThanOneConsignment()
		{
			AssertISFForm(2, false);
		}

		public void TestMenuItemAction_WhenNoConsignments_ShowsMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S001";
				shipment.JS_RL_NKDestination = "USLAX";

				HVLVConsignmentHeader.GetOrCreate(shipment);

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					ShowFormAndClickOnCreateUSISMenuItem(form);

					using (var isfForm = ZFormModaliser.LastFormShownDialogForTest as Customs.US.ISF.GUI.ISFForm)
					{
						AssertNull(isfForm);
						var noActiveConsignmentsMessage = UnitTestUserNotification.Instance.LastMessage.Text;
						AssertEquals("Please make sure there's at least one active consignment on this shipment.", noActiveConsignmentsMessage);
					}
				}
			}
		}

		public void TestMenuItem_WhenConsignmentWaybillHasNoSCACCodeAndIsOver12Characters_ShowsMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.Shipments.Add(shipment);

				var sendingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
				var cusCode = sendingAgentOrg.ConfigOrg.CustomsCodes.AddNew();
				cusCode.OK_CodeType = "CCC";
				cusCode.OK_CustomsRegNo = "ABCD";
				cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;

				consol.JK_OA_SendingForwarderAddress = sendingAgentOrg.MainAddress.PK;

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment_WithWaybillLength13 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment_WithWaybillLength13.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment_WithWaybillLength13.HVC_WaybillNumber = "1234567890123";
				consignment_WithWaybillLength13.HVC_HCH_Header = consignmentHeader.PK;

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					ShowFormAndClickOnCreateUSISMenuItem(form);

					using (var isfForm = ZFormModaliser.LastFormShownDialogForTest as Customs.US.ISF.GUI.ISFForm)
					{
						AssertNull(isfForm);
						var waybillValidationErrorMessage = UnitTestUserNotification.Instance.LastMessage.Text;
						AssertEquals("There are waybill(s) that exceed 12 in length, proceeding may cause message errors on US Importer Security Filing.\r\nWould you like to proceed?", waybillValidationErrorMessage);
					}
				}
			}
		}

		public void TestMenuItem_WhenConsignmentWaybillHasSCACCodeAndIsOver16Characters_ShowsMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.Shipments.Add(shipment);

				var sendingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
				var cusCode = sendingAgentOrg.ConfigOrg.CustomsCodes.AddNew();
				cusCode.OK_CodeType = "CCC";
				cusCode.OK_CustomsRegNo = "ABCD";
				cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;

				consol.JK_OA_SendingForwarderAddress = sendingAgentOrg.MainAddress.PK;

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment_WithWaybillLength17 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment_WithWaybillLength17.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment_WithWaybillLength17.HVC_WaybillNumber = "ABCD5678901234567";
				consignment_WithWaybillLength17.HVC_HCH_Header = consignmentHeader.PK;

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					ShowFormAndClickOnCreateUSISMenuItem(form);

					using (var isfForm = ZFormModaliser.LastFormShownDialogForTest as Customs.US.ISF.GUI.ISFForm)
					{
						AssertNull(isfForm);
						var waybillValidationErrorMessage = UnitTestUserNotification.Instance.LastMessage.Text;
						AssertEquals("There are waybill(s) that exceed 12 in length, proceeding may cause message errors on US Importer Security Filing.\r\nWould you like to proceed?", waybillValidationErrorMessage);
					}
				}
			}
		}

		public void TestMenuItemAction_WhenNoActiveConsignments_ShowsMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S001";
				shipment.JS_RL_NKDestination = "USLAX";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;

				Factory.Save();

				var factory2 = new BusinessObjectFactory();
				var reloadedConsignment = factory2.Load<HVLVConsignment>(consignment.PK);
				reloadedConsignment.HVC_IsActive = false;
				factory2.Save();

				using (var form = new ZForm(shipment))
				{
					ShowFormAndClickOnCreateUSISMenuItem(form);

					using (var isfForm = ZFormModaliser.LastFormShownDialogForTest as Customs.US.ISF.GUI.ISFForm)
					{
						AssertNull(isfForm);
						var noActiveConsignmentsMessage = UnitTestUserNotification.Instance.LastMessage.Text;
						AssertEquals("Please make sure there's at least one active consignment on this shipment.", noActiveConsignmentsMessage);
					}
				}
			}
		}

		public void TestMenuItemAction_WillShowCorrectNumberOfRelatedJobs_With_MoreThan_OneConsignment()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S001";
				shipment.JS_RL_NKDestination = "USLAX";

				AddConsignmentsToShipment(3, shipment);

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					var visibleRows = 0;
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
					{
						if (obj is ZChildForm metaHeaderForm && !(obj is ProgressForm))
						{
							var relatedJobsUserControl = metaHeaderForm.Controls.Find("RelatedJobsUserControl", true).Single() as RelatedJobsUserControl;
							var relatedJobsGrid = relatedJobsUserControl.Controls.Find("RelatedJobsGrid", true).Single() as ZGrid;
							relatedJobsGrid.Dock = DockStyle.Fill;
							visibleRows = relatedJobsGrid.VisibleRowCount;
						}
					});

					ShowFormAndClickOnCreateUSISMenuItem(form);

					AssertEquals("Related Jobs Grid should display three ISFs", 3, visibleRows);
				}
			}
		}

		public void TestMenuItemAction_CloseISFFormWithoutSaving_ThenSaveShipment_WouldNotSaveISFAndLog()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S001";
				shipment.JS_RL_NKDestination = "USCHI";
				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					ShowFormAndClickOnCreateUSISMenuItem(form);

					var isfForm = ZFormModaliser.LastFormShownForTest as Customs.US.ISF.GUI.ISFForm;
					var isfHeader = isfForm.BusinessEntity;

					isfForm.Close();

					AssertNotNull(isfHeader);
					Assert(!isfHeader.IsInDatabase);

					Factory.Save();
					Assert(!shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode).Any());
				}
			}
		}

		public void TestMenuItemAction_WhenISFIsSaved_ShouldUseBulkSave()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, setConsignorAddresses: true, createItem: true, createItemLine: true,numOfRecords: Factory.DefaultBulkCopyThreshold() + 1);
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S001";
				shipment.JS_RL_NKDestination = "USCHI";

				shipment.Transports.AddNew();
				Factory.Save();

				using var form = new ZForm(shipment);
				ShowFormAndClickOnCreateUSISMenuItem(form);

				var isfHeader = ZFormModaliser.LastIBusinessShownOnDialogForTest;
				CombineAssertions("All these datatables should have been BulkCopied", () =>
				{
					using (var bulkCopyEventTracker = new SqlBulkCopyEventTracker())
					{
						isfHeader.Factory.Save();
						AssertHasBulkCopyEvent(bulkCopyEventTracker, CusISFHeaderSchema.Constants.TableName);
						AssertHasBulkCopyEvent(bulkCopyEventTracker, CusISFBillSchema.Constants.TableName);
						AssertHasBulkCopyEvent(bulkCopyEventTracker, JobDocAddressSchema.Constants.TableName);
						AssertHasBulkCopyEvent(bulkCopyEventTracker, JobConsolTransportSchema.Constants.TableName);
						AssertHasBulkCopyEvent(bulkCopyEventTracker, CusISFLineSchema.Constants.TableName);
						AssertHasBulkCopyEvent(bulkCopyEventTracker, StmALogSchema.Constants.TableName);
					}
				});
			}
		}

		public void TestMenuItemAction_WhenISFIsSaved_ShouldUseBulkSave_WithWorkflowTemplate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				processTaskTemplate.P0_ProcessType = WorkflowDescriptors.CusISFHeaderWorkflowDescriptorCode;

				var task = processTaskTemplate.WorkflowItems.AddNew();
				task.P9_Type = "TRG";
				task.ProcessTaskNotifications.AddNew();

				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, setConsignorAddresses: true, createItem: true, numOfRecords: Factory.DefaultBulkCopyThreshold() + 1);
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S001";
				shipment.JS_RL_NKDestination = "USCHI";

				shipment.Transports.AddNew();
				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					ShowFormAndClickOnCreateUSISMenuItem(form);

					var isfHeader = ZFormModaliser.LastIBusinessShownOnDialogForTest;

					CombineAssertions("All these datatables should have been BulkCopied", () =>
					{
						using (var bulkCopyEventTracker = new SqlBulkCopyEventTracker())
						{
							isfHeader.Factory.Save();
							AssertHasBulkCopyEvent(bulkCopyEventTracker, CusISFHeaderSchema.Constants.TableName);
							AssertHasBulkCopyEvent(bulkCopyEventTracker, CusISFBillSchema.Constants.TableName);
							AssertHasBulkCopyEvent(bulkCopyEventTracker, JobDocAddressSchema.Constants.TableName);
							AssertHasBulkCopyEvent(bulkCopyEventTracker, JobConsolTransportSchema.Constants.TableName);
							AssertHasBulkCopyEvent(bulkCopyEventTracker, StmALogSchema.Constants.TableName);
						}
					});
				}
			}
		}

		public void TestMenuItemAction_WhenISFIsSaved_ShouldUseBulkSave_WithJobRequiredDocument()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var unitedStates = RefCountry.LoadFromCountryCode(Factory, CountryCodes.UnitedStates);

				var requiredDoc = unitedStates.RequiredDocuments.AddNew();
				requiredDoc.RD_DocType = RefDocTypes.CommercialInvoice;
				requiredDoc.RD_DocUsage = JobRequiredDocument.DocUsage.All;
				requiredDoc.RD_TransportMode = TransportModes.All;
				requiredDoc.RD_OnShipment = true;
				requiredDoc.RD_OnBrokerage = true;

				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, setConsignorAddresses: true, createItem: true, createItemLine: true, numOfRecords: Factory.DefaultBulkCopyThreshold() + 1);
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S001";
				shipment.JS_RL_NKDestination = "USCHI";

				shipment.Transports.AddNew();
				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					ShowFormAndClickOnCreateUSISMenuItem(form);

					var isfHeader = ZFormModaliser.LastIBusinessShownOnDialogForTest;

					CombineAssertions("All these datatables should have been BulkCopied", () =>
					{
						using (var bulkCopyEventTracker = new SqlBulkCopyEventTracker())
						{
							isfHeader.Factory.Save();
							AssertHasBulkCopyEvent(bulkCopyEventTracker, CusISFHeaderSchema.Constants.TableName);
							AssertHasBulkCopyEvent(bulkCopyEventTracker, CusISFBillSchema.Constants.TableName);
							AssertHasBulkCopyEvent(bulkCopyEventTracker, CusISFLineSchema.Constants.TableName);
							AssertHasBulkCopyEvent(bulkCopyEventTracker, JobDocAddressSchema.Constants.TableName);
							AssertHasBulkCopyEvent(bulkCopyEventTracker, JobConsolTransportSchema.Constants.TableName);
							AssertHasBulkCopyEvent(bulkCopyEventTracker, JobRequiredDocumentSchema.Constants.TableName);
							AssertHasBulkCopyEvent(bulkCopyEventTracker, StmALogSchema.Constants.TableName);
						}
					});
				}
			}
		}

		void ShowFormAndClickOnCreateUSISMenuItem(ZForm form)
		{
			form.PlugIns.Add(ControllerIDs.ETailShipment);
			form.Show();

			var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
			var menuHVLV = plugin.TopLevelMenu;
			menuHVLV.PerformSelect();

			var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
			var uSISFMenuGroup = customsMenu.MenuItems.OfType<USISFMenuGroup>().Single();
			var menuItem = uSISFMenuGroup.MenuItems.OfType<CreateUSISFMenuItem>().Single();

			Assert(menuItem.Visible);
			menuItem.PerformClick();
		}

		void AssertISFForm(int consignmentsCount, bool shouldShowISFForm)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S001";
				shipment.JS_RL_NKDestination = "USLAX";

				AddConsignmentsToShipment(consignmentsCount, shipment);

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					RelatedJobsUserControl relatedJobsUserControl = null;
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(obj =>
					{
						if (obj is ZChildForm relatedJobsForm && !(obj is ProgressForm) && !shouldShowISFForm)
						{
							relatedJobsUserControl = relatedJobsForm.Controls.Find("RelatedJobsUserControl", true).Single() as RelatedJobsUserControl;
						}
					});

					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var uSISFMenuGroup = customsMenu.MenuItems.OfType<USISFMenuGroup>().Single();
					var menuItem = uSISFMenuGroup.MenuItems.OfType<CreateUSISFMenuItem>().Single();

					Assert(menuItem.Visible);
					menuItem.PerformClick();

					if (!shouldShowISFForm)
					{
						AssertNotNull("A Related Jobs User Control was expected to be shown", relatedJobsUserControl);
					}

					using (var isfForm = ZFormModaliser.LastFormShownForTest as Customs.US.ISF.GUI.ISFForm)
					{
						if (shouldShowISFForm)
						{
							AssertNotNull($"ISF form should show when a shipment has {consignmentsCount} consignments", isfForm);
						}
						else
						{
							AssertNull($"ISF form should not show when a shipment has {consignmentsCount} consignments", isfForm);
						}
					}
				}
			}
		}

		void AddConsignmentsToShipment(int consignmentsCount, ForwardingShipment shipment)
		{
			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			for (var i = 0; i < consignmentsCount; i++)
			{
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;
			}
		}

		protected override ForwardingShipment PrepareShipment()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_RL_NKDestination = "USCHI";

			return shipment;
		}

		protected override ZMenuItem GetMenuItemForAppLockKeyTest(ZForm form)
		{
			var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
			var menuHVLV = plugin.TopLevelMenu;
			menuHVLV.PerformSelect();

			var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
			var uSISFMenuGroup = customsMenu.MenuItems.OfType<USISFMenuGroup>().Single();

			return uSISFMenuGroup.MenuItems.OfType<CreateUSISFMenuItem>().Single();
		}

		protected override string TestingCountry => CountryCodes.UnitedStates;
		protected override string ExpectedAppLockKey => "HVLV US ISF";

		void AssertHasBulkCopyEvent(SqlBulkCopyEventTracker tracker, string tableName)
		{
			List<string> bulkCopyOptions = ["CheckConstraints", "FireTriggers"];
			Assert(tableName, tracker.HasBulkCopyEvent(tableName, bulkCopyOptions));
		}
	}
}
