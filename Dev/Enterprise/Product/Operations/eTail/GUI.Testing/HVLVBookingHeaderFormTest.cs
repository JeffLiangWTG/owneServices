using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Customs.Common.GUI;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.eTail.Module;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.eTail.GUI.Testing
{
	[TestedType(typeof(HVLVBookingHeaderForm))]
	class HVLVBookingHeaderFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var form = new HVLVBookingHeaderForm(Factory.New<HVLVBookingHeader>());
			form.WindowState = FormWindowState.Maximized;
			form.ControllerID = ControllerIDs.HVLVBookingHeader;
			return form;
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			if (control.Name.Equals("showConsignmentDetailsCheckBox") || control.Name.Equals("showLineDetailsCheckBox"))
			{
				return true;
			}
			else
			{
				return base.ShouldIgnoreMissingBindingMember(control);
			}
		}

		public void TestTransportBookingPluginAdded()
		{
			var bookingHeader = CreateBookingHeaderConsignments();

			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				AssertNotNull(ControllerIDs.DtbBooking.Name, form.PlugIns.GetPlugIn(ControllerIDs.DtbBooking));
			}
		}

		public void TestPreScreenHVLVDetails_OnlyAvailableToRunAfterDataSaved()
		{
			var bookingHeader = CreateBookingHeaderConsignments();

			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				form.Show();
				bookingHeader.HasChanges = true;

				var menuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ScreeningHVLVDetails);
				AssertNotNull(menuItem);

				menuItem.PerformClick();
				AssertEquals("Should save before Pre-Screening", @"Please save the form before Pre-Screening", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestContainerNumberColumn()
		{
			var filterBizo = typeof(HVLVConsignmentUserControl).
				GetMethod(
				"GetConsignmentFilterBusinessObject",
				BindingFlags.Static |
				BindingFlags.Public);
			AssertNotNull("GetConsignmentFilterBusinessObject has to be public static, for the designer", filterBizo);
		}

		public void TestLastMileCarrierBookingAgentColumn()
		{
			using (var form = new HVLVBookingHeaderForm(Factory.New<HVLVBookingHeader>()))
			{
				form.Show();
				var grid = ((ZModuleButtonGrid)form.Controls.Find("consignmentsGrid", true)[0]).InnerGrid;

				Assert(grid.Columns.Contains(AutoHVLVConsignment.Schema.HVC_OH_LastMileCarrierBookingAgent));
				var columnStyle = grid.GetColumnStyle(AutoHVLVConsignment.Schema.HVC_OH_LastMileCarrierBookingAgent);
				AssertType<ZOrganisationFindBoxColumnStyleInfo>(columnStyle);
			}
		}

		public void TestGetConsignmentFilterBusinessObjectModifier()
		{
			var filterBizo = typeof(HVLVConsignmentUserControl).
				GetMethod(
				"GetConsignmentFilterBusinessObject",
				BindingFlags.Static |
				BindingFlags.Public);
			AssertNotNull("GetConsignmentFilterBusinessObject has to be public static, for the designer", filterBizo);
		}

		public void TestHVLVConsignmentFilterBusinessObject()
		{
			using (var control = new HVLVConsignmentUserControl())
			{
				var filterControl = control.Controls.Find("consignmentFilterStripControl", true)[0] as ZFilterStripBaseControl;
				AssertNotNull(filterControl);
				var filterBizO = filterControl.FilterBusinessObject;
				AssertEquals($"{nameof(HVLVConsignmentUserControl)} has a HVLVConsignmentFilterBusinessObject", "Enterprise.eTail.Module.HVLVConsignmentFilterBusinessObject", filterBizO.GetType().FullName);
			}
		}

		public void TestBookingStatusDropEditIsEditable()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				form.Show();
				var bookingStatusDropEdit = form.Controls.Find("bookingStatusDropEdit", true)[0] as ZDropEdit;
				AssertNotNull(bookingStatusDropEdit);
				Assert(!bookingStatusDropEdit.ReadOnly);
			}
		}

		public void TestDeniedPartyScreeningFormControls()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				form.Show();
				var deniedPartyScreeningStatusDropEdit = form.Controls.Find("deniedPartyScreeningStatusDropEdit", true)[0] as DeniedPartyScreeningStatusDropEdit;
				var deniedPartyScreeningButton = form.Controls.Find("deniedPartyScreeningButton", true)[0] as ZButton;

				CombineAssertions(() =>
				{
					AssertNotNull(deniedPartyScreeningStatusDropEdit);
					AssertNotNull(deniedPartyScreeningButton);
					Assert(deniedPartyScreeningStatusDropEdit.ReadOnly);
				});	
			}
		}

		public void TestDeniedPartyScreeningMenuItemControls()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			Factory.Save();

			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				var viewComplianceStatusMenuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().SingleOrDefault(x => x.Text == "View Compliance Status");
				var resynchronizeScreeningStatusMenuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().SingleOrDefault(x => x.Text == "Resynchronize Screening Status");

				CombineAssertions(() =>
				{
					AssertNotNull(viewComplianceStatusMenuItem);
					AssertNotNull(resynchronizeScreeningStatusMenuItem);
				});
			}
		}

		public void TestViewComplianceStatusMenuItem()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			Factory.Save();

			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				form.Show();
				var viewComplianceStatusMenuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().SingleOrDefault(x => x.Text == "View Compliance Status");

				using (HVLVDataRegistry.Instance.HVLVEnablePartyScreening.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new HVLVEnablePartyScreening() { EnableHVLVPartyScreening = false }))
				{
					viewComplianceStatusMenuItem.PerformClick();
					AssertEquals("HVLV Party Screening has not been enabled.\r\nTo enable go to Registry > Master Data > Organizations > Denied Party Screening > Enable HVLV Party Screening.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (HVLVDataRegistry.Instance.HVLVEnablePartyScreening.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new HVLVEnablePartyScreening() { EnableHVLVPartyScreening = true }))
				{
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					viewComplianceStatusMenuItem.PerformClick();
					AssertEquals("Should be showing the Party Compliance form", typeof(PartyComplianceForm), ZFormModaliser.ActiveForm.GetType());
				}
			}
		}

		public void TestResynchronizeScreeningStatusMenuItem()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			Factory.Save();

			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				form.Show();
				var resynchronizeScreeningStatusMenuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().SingleOrDefault(x => x.Text == "Resynchronize Screening Status");

				using (HVLVDataRegistry.Instance.HVLVEnablePartyScreening.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new HVLVEnablePartyScreening() { EnableHVLVPartyScreening = false }))
				{
					resynchronizeScreeningStatusMenuItem.PerformClick();
					AssertEquals("HVLV Party Screening has not been enabled.\r\nTo enable go to Registry > Master Data > Organizations > Denied Party Screening > Enable HVLV Party Screening.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestDeniedPartyScreeningButton()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			Factory.Save();

			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				form.Show();
				var deniedPartyScreeningButton = form.Controls.Find("deniedPartyScreeningButton", true)[0] as ZButton;

				using (HVLVDataRegistry.Instance.HVLVEnablePartyScreening.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new HVLVEnablePartyScreening() { EnableHVLVPartyScreening = false }))
				{
					deniedPartyScreeningButton.PerformClick();
					AssertEquals("HVLV Party Screening has not been enabled.\r\nTo enable go to Registry > Master Data > Organizations > Denied Party Screening > Enable HVLV Party Screening.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (HVLVDataRegistry.Instance.HVLVEnablePartyScreening.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new HVLVEnablePartyScreening() { EnableHVLVPartyScreening = true }))
				{
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					deniedPartyScreeningButton.PerformClick();
					AssertEquals("Should be showing the Party Compliance form", typeof(PartyComplianceForm), ZFormModaliser.ActiveForm.GetType());
				}
			}
		}

		public void TestFilterConsignments()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HVH_BookingHeader = consignment2.HVC_HVH_BookingHeader = bookingHeader.PK;
			consignment1.HVC_WaybillNumber = "HVC001";
			consignment2.HVC_WaybillNumber = "HVC002";
			consignment1.Items.AddNew();
			consignment2.Items.AddNew();
			Factory.Save();

			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				form.Show();
				var filterControl = form.Controls.Find("consignmentFilterStripControl", true)[0] as ZFilterStripBaseControl;
				var filterBO = filterControl.FilterBusinessObject;
				var consignmentIDFilter = filterBO.ModuleFilters["Consignment Waybill #"] as ModuleTextFilter;
				consignmentIDFilter.IsActive = true;
				consignmentIDFilter.Property = "001";
				consignmentIDFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;

				filterControl.FirePerformSearch();
				AssertEquals(1, filterControl.GridCollection.Count);
				AssertEquals(consignment1, filterControl.GridCollection[0]);

				consignmentIDFilter.IsActive = false;
				filterControl.FirePerformSearch();
				AssertEquals(2, filterControl.GridCollection.Count);
			}
		}

		public void TestConsignmentAttachAndDetachButtonIsNotVisible()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HVH_BookingHeader = consignment2.HVC_HVH_BookingHeader = bookingHeader.PK;
			Factory.Save();

			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				form.Show();
				var buttonGrid = (ZModuleButtonGrid)form.Controls.Find("consignmentsGrid", true).SingleOrDefault();
				AssertEquals(false, buttonGrid.AttachButtonForTest.Visible);
				AssertEquals(false, buttonGrid.DetachButtonForTest.Visible);
			}
		}

		public void TestReactivateHVLVBookingHeader_UserChoosesYesToActivateAllConsignments()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_IsActive = false;
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_IsActive = false;
			Factory.Save();

			using (var module = new HVLVBookingHeaderModule())
			using (var form = new ZForm())
			{
				var hvlvBookingHeaderFilterControl = (HVLVBookingHeaderFilterControl)module.EmbeddedControl;
				form.Controls.Add(hvlvBookingHeaderFilterControl);
				form.Show();

				var activeStatusFilter = hvlvBookingHeaderFilterControl.FilterBusinessObject.ModuleFilters["Active Status"] as ModuleTextFilter;
				activeStatusFilter.Property = "Inactive";
				((IFilterModuleInternalsForTesting)module).PerformSearch();
				AssertEquals("Precondition: GridCollection.Count", 1, module.GridCollection.Count);

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(0);
				module.DeleteMenuItem.PerformClick();

				var openForms = ZApplication.GetOpenForms();
				foreach (var openForm in openForms.Where(x => x.GetType() == typeof(HVLVBookingHeaderForm)))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					openForm.AcceptButton.PerformClick();

					CombineAssertions(() =>
					{
						AssertEquals(true, bookingHeader.HVH_IsActive);
						AssertEquals(true, consignment.HVC_IsActive);
					});
				}
			}
		}

		public void TestReactivateHVLVBookingHeader_UserChoosesNoToActivateAllConsignments()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_IsActive = false;
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_IsActive = false;
			Factory.Save();

			using (var module = new HVLVBookingHeaderModule())
			using (var form = new ZForm())
			{
				var hvlvBookingHeaderFilterControl = (HVLVBookingHeaderFilterControl)module.EmbeddedControl;
				form.Controls.Add(hvlvBookingHeaderFilterControl);
				form.Show();

				var activeStatusFilter = hvlvBookingHeaderFilterControl.FilterBusinessObject.ModuleFilters["Active Status"] as ModuleTextFilter;
				activeStatusFilter.Property = "Inactive";
				((IFilterModuleInternalsForTesting)module).PerformSearch();
				AssertEquals("Precondition: GridCollection.Count", 1, module.GridCollection.Count);

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(0);
				module.DeleteMenuItem.PerformClick();

				var openForms = ZApplication.GetOpenForms();
				foreach (var openForm in openForms.Where(x => x.GetType() == typeof(HVLVBookingHeaderForm)))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					openForm.AcceptButton.PerformClick();

					CombineAssertions(() =>
					{
						AssertEquals(true, bookingHeader.HVH_IsActive);
						AssertEquals(false, consignment.HVC_IsActive);
					});
				}
			}
		}

		public void TestCalculateLMCDepotDetailsActionMenuItemClick()
		{
			var bookingHeader = CreateBookingHeaderConsignments();
			Factory.Save();

			foreach (HVLVConsignment consignment in bookingHeader.Consignments)
			{
				AssertEquals(ZGuid.Empty, consignment.HVC_OA_DestinationDepot);
				AssertEquals(ZGuid.Empty, consignment.HVC_OH_LastMileCarrier);
				AssertEquals(ZString.Empty, consignment.HVC_PL_NKLastMileCarrierServiceLevel);
				AssertEquals(ZGuid.Empty, consignment.HVC_OH_LastMileCarrierBookingAgent);
			}

			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				form.Show();

				var menuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.CalculateLMCDepotDetails);
				AssertNotNull(menuItem);

				menuItem.PerformClick();

				foreach (HVLVConsignment consignment in bookingHeader.Consignments)
				{
					AssertEquals(depotAddress1.PK, consignment.HVC_OA_DestinationDepot);
					AssertEquals(carrier1.PK, consignment.HVC_OH_LastMileCarrier);
					AssertEquals("EXP", consignment.HVC_PL_NKLastMileCarrierServiceLevel);
					AssertEquals(agent1.PK, consignment.HVC_OH_LastMileCarrierBookingAgent);
				}

				PortHubSelectionTestDataCreator.ClearUpAllZoneItems(Factory);
				menuItem.PerformClick();

				foreach (HVLVConsignment consignment in bookingHeader.Consignments)
				{
					AssertEquals(ZGuid.Empty, consignment.HVC_OA_DestinationDepot);
					AssertEquals(ZGuid.Empty, consignment.HVC_OH_LastMileCarrier);
					AssertEquals(ZString.Empty, consignment.HVC_PL_NKLastMileCarrierServiceLevel);
					AssertEquals(ZGuid.Empty, consignment.HVC_OH_LastMileCarrierBookingAgent);
					AssertEquals(ZGuid.Empty, consignment.HVC_OA_DestinationDepot_ZAddress.OrgPK);
				}
			}
		}

		public void TestShowConsignmentDetailsCheckbox()
		{
			using (var control = new HVLVConsignmentUserControl())
			{
				var foundControls = control.Controls.Find("showConsignmentDetailsCheckBox", true);
				AssertEquals("showConsignmentDetailsCheckBox should be there.", 1, foundControls.Length);

				var showDetailsCheckBox = foundControls[0] as ZCheckBox;
				AssertNotNull(showDetailsCheckBox);
				AssertEquals("ShowConsignmentDetailsCheckBox should be set by default", true, showDetailsCheckBox.Checked);

				var tabConsignmentFoundControls = control.Controls.Find("tabConsignment", true);
				AssertEquals("tabConsignment should be there.", 1, tabConsignmentFoundControls.Length);

				var tabConsignment = tabConsignmentFoundControls[0] as ZTabControl;
				AssertNotNull(tabConsignment);
				Assert("tabConsignment should be visible", tabConsignment.Visible);

				showDetailsCheckBox.Checked = false;
				Assert("tabConsignment should be invisible when ShowDetails unchecked.", !tabConsignment.Visible);

				showDetailsCheckBox.Checked = true;
				Assert("tabConsignment should be visible when ShowDetails checked.", tabConsignment.Visible);
			}
		}

		HVLVBookingHeader CreateBookingHeaderConsignments()
		{
			var creator = new PortHubSelectionTestDataCreator(Factory);

			var billToParty = creator.GenerateAddress("AD1", "XY1", "XY 1");
			var dispatchDepotAddress1 = creator.GenerateAddress("AD2", "XY2", "XY 2");
			depotAddress1 = creator.GenerateAddress("AD3", "XY3", "XY 3");
			carrier1 = creator.GenerateOrganisation("XY4", "XY 4");
			agent1 = creator.GenerateOrganisation("ZZ5", "ZZ 5");

			var portHubSelectionPK = creator.CreatePortAndDepotSelectionWithUndgClass(depotAddress1.PK, dispatchDepotAddress1.PK, "EXP", "DLV", "ALL", "AAA", "ALL");
			var portHubSelection = Factory.Load<PortHubSelection>(portHubSelectionPK);
			portHubSelection.TY_OH_CarrierBookingAgent = agent1.PK;
			var zonePK = creator.AddZone(portHubSelectionPK, "Z2", carrier1.PK, "EXP");
			creator.AddZoneItem(zonePK, "Melbourne Metro", "VIC", "AU");

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_BookingReference = "M00000050";
			bookingHeader.HVH_OA_BillToParty = billToParty.PK;
			bookingHeader.HVH_RS_NKBookingServiceLevel = "EXP";
			bookingHeader.HVH_OA_DispatchAddress = dispatchDepotAddress1.PK;

			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN100";
			consignment1.HVC_UndgClass = "6";
			consignment1.HVC_ConsigneeAddress1 = "Test Address 11";
			consignment1.HVC_ConsigneeCity = "Melbourne Metro";
			consignment1.HVC_ConsigneeState = "VIC";
			consignment1.HVC_ConsigneePostcode = "3560";
			consignment1.HVC_RN_NKConsigneeCountryCode = "AU";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN101";
			consignment2.HVC_UndgClass = "6";
			consignment2.HVC_ConsigneeAddress1 = "Test Address 12";
			consignment2.HVC_ConsigneeCity = "Melbourne Metro";
			consignment2.HVC_ConsigneeState = "VIC";
			consignment2.HVC_ConsigneePostcode = "3561";
			consignment2.HVC_RN_NKConsigneeCountryCode = "AU";

			return bookingHeader;
		}

		public void TestShowLineDetailsCheckbox()
		{
			using (var control = new HVLVConsignmentUserControl())
			{
				var foundControls = control.Controls.Find("showLineDetailsCheckBox", true);
				AssertEquals(1, foundControls.Length);

				var showDetailsCheckBox = foundControls[0] as ZCheckBox;
				AssertNotNull(showDetailsCheckBox);
				AssertEquals(true, showDetailsCheckBox.Checked);

				var foundGroupBox = control.Controls.Find("groupBoxItemLines", true);
				AssertEquals(1, foundGroupBox.Length);

				var itemLinesGroupBox = foundGroupBox[0] as ZGroupBox;
				AssertNotNull(itemLinesGroupBox);
				Assert(itemLinesGroupBox.Visible);

				var foundContainer = control.Controls.Find("itemSplitContainer", true);
				AssertEquals(1, foundContainer.Length);
				var splitContainer = foundContainer[0] as KSplitContainer;

				showDetailsCheckBox.Checked = false;
				Assert(!itemLinesGroupBox.Visible);
				Assert(splitContainer.Panel2Collapsed);

				showDetailsCheckBox.Checked = true;
				Assert(itemLinesGroupBox.Visible);
				Assert(!splitContainer.Panel2Collapsed);
			}
		}

		public void TestClassificationLookupFindBoxColumnModuleDefaulting()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = header.Consignments.AddNew();
			consignment.HVC_RN_NKShipperCountryCode = "AU";
			consignment.HVC_RN_NKConsigneeCountryCode = "US";
			var item = consignment.Items.AddNew();
			item.Lines.AddNew();

			AssertModuleDefaulting(Constants.CountryCodes.UnitedStates, ModuleIDs.ImportClassification, "Enterprise.Customs.US.Module.ImportClassificationModule");
			AssertModuleDefaulting(Constants.CountryCodes.Australia, ModuleIDs.ExportClassification, "Enterprise.Customs.AU.Module.ExportClassificationModule");
			AssertModuleDefaulting(Constants.CountryCodes.China, ModuleIDs.SingleTariffClassification, "Enterprise.Customs.CN.Module.CusClassificationModule");

			void AssertModuleDefaulting(string countryCode, ModuleIdentifier expectedModuleId, string expectedModuleTypeFullName)
			{
				using (var form = new HVLVBookingHeaderForm(header))
				{
					form.Show();
					Application.DoEvents();
					var itemLinesGrid = form.Controls.Find("itemLinesGrid", true)[0] as ZGrid;
					itemLinesGrid.Select(0);
					var style = itemLinesGrid.Columns[AutoHVLVItemLine.Schema.HVS_CC_Lookup].ColumnStyle as ZGuidFindBoxColumnStyle;
					var findBox = style.EditControl as ZGridFindBox;
					findBox.Parent = form;
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
					{
						findBox.SelectFromPopupForm();
						var popup = ZFormModaliser.LastFormShownForTest as EmbeddedModulePopup;
						AssertEquals("Should use Global Classification Module", expectedModuleId, popup.Module_ForTest.ID);
						AssertEquals("Should be the US one", expectedModuleTypeFullName, popup.Module_ForTest.GetType().FullName);
						popup.Close();
					}
				}
			}
		}

		public void TestCanShowBorderWiseTariffModuleSearch()
		{
			var bookingheader = Factory.New<HVLVBookingHeader>();
			var consignment = bookingheader.Consignments.AddNew();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "USLAX";

			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var itemline = item.Lines.AddNew();
			itemline.HVS_RN_NKOriginCountryCode = "AU";

			DataRegistry.Instance.BorderWiseUmpApiBaseAddress = string.Empty;
			DataRegistry.Instance.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
			using (var form = new HVLVBookingHeaderForm(bookingheader))
			{
				form.Show();
				var grid = form.Controls.Find("itemLinesGrid", true)[0] as ZGrid;

				grid.CurrentCell = new DataGridCell(0, 4);
				var columnNumberOrigin = grid.Columns[grid.CurrentCell.ColumnNumber];
				AssertEquals(HVLVItemLine.Schema.HVS_FormattedOriginTariff, columnNumberOrigin.ColumnName);

				var findBoxOrigin = (ZPopupFindBox)(((HVLVTariffColumnStyle)columnNumberOrigin.ColumnStyle).EditControl);
				findBoxOrigin.PopupButton.PerformClick();
				AssertEquals(typeof(EmbeddedModulePopup), ((IFindBox)findBoxOrigin).PopupForm.GetType());

				grid.CurrentCell = new DataGridCell(0, 6);
				var columnNumberDestination = grid.Columns[grid.CurrentCell.ColumnNumber];
				AssertEquals(HVLVItemLine.Schema.HVS_FormattedDestinationTariff, columnNumberDestination.ColumnName);

				var findBoxDestination = (ZPopupFindBox)(((HVLVTariffColumnStyle)columnNumberDestination.ColumnStyle).EditControl);
				findBoxDestination.PopupButton.PerformClick();
				AssertEquals(typeof(EmbeddedModulePopup), ((IFindBox)findBoxDestination).PopupForm.GetType());
			}

			DataRegistry.Instance.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
			DataRegistry.Instance.BorderWiseEnableWebSocketClient = false;
			using (var form = new HVLVBookingHeaderForm(bookingheader))
			{
				form.Show();
				var grid = form.Controls.Find("itemLinesGrid", true)[0] as ZGrid;

				grid.CurrentCell = new DataGridCell(0, 4);
				var originTariffColumn = grid.Columns[grid.CurrentCell.ColumnNumber];
				AssertEquals(HVLVItemLine.Schema.HVS_FormattedOriginTariff, originTariffColumn.ColumnName);

				var originFindBox = (ZPopupFindBox)(((HVLVTariffColumnStyle)originTariffColumn.ColumnStyle).EditControl);
				originFindBox.PopupButton.PerformClick();
				AssertEquals(typeof(FindBoxWrapperForBorderWise), ((IFindBox)originFindBox).PopupForm.GetType());

				var popUpForm = ((IFindBox)originFindBox).PopupForm as FindBoxWrapperForBorderWise;
				AssertEquals("AU", popUpForm.AdditionalData.CountryCodeOverride);
				AssertEquals("E", popUpForm.AdditionalData.ParameterForBorderWise);

				grid.CurrentCell = new DataGridCell(0, 6);
				var destinationTariffColumn = grid.Columns[grid.CurrentCell.ColumnNumber];
				AssertEquals(HVLVItemLine.Schema.HVS_FormattedDestinationTariff, destinationTariffColumn.ColumnName);

				var destinationFindBox = (ZPopupFindBox)(((HVLVTariffColumnStyle)destinationTariffColumn.ColumnStyle).EditControl);
				destinationFindBox.PopupButton.PerformClick();
				AssertEquals(typeof(FindBoxWrapperForBorderWise), ((IFindBox)destinationFindBox).PopupForm.GetType());

				popUpForm = ((IFindBox)destinationFindBox).PopupForm as FindBoxWrapperForBorderWise;
				AssertEquals("US", popUpForm.AdditionalData.CountryCodeOverride);
				AssertEquals("I", popUpForm.AdditionalData.ParameterForBorderWise);

				itemline.HVS_RN_NKOriginCountryCode = "FR";
				grid.CurrentCell = new DataGridCell(0, 4);
				originFindBox.PopupButton.PerformClick();

				AssertEquals(typeof(EmbeddedModulePopup), ((IFindBox)originFindBox).PopupForm.GetType());
			}
		}

		public void TestEcommerceWebPortalsMenuItem_HasSubMenusEcommerceOriginDepotMenuAndEcommerceDestinationDepotMenu()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				form.Show();

				var menuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Ecommerce Web Portals");
				AssertEquals(2, menuItem.MenuItems.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "Ecommerce Origin Depot", "Ecommerce Destination Depot" }, menuItem.MenuItems.OfType<MenuItem>().Select(x => x.Text).ToList());
			}
		}

		public void TestEcommerceOriginDepotMenuItem_WhenClick_NavigationToPortalsBaseUrlWithOriginCode()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				var menuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.NavigateToEcommerceWebPortals);
				var originDepot = menuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.NavigateToEcommerceOriginDepot);

				HVLVMenuItemTestHelper.AssertEcommercePortalNavigated(EcommercePortals.Codes.EOS, originDepot);
			}
		}

		public void TestEcommerceDestinationDepotMenuItem_WhenClick_NavigationToPortalsBaseUrlWithDestinationCode()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				var menuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.NavigateToEcommerceWebPortals);
				var destinationDepot = menuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.NavigateToEcommerceDestinationDepot);

				HVLVMenuItemTestHelper.AssertEcommercePortalNavigated(EcommercePortals.Codes.ETL, destinationDepot);
			}
		}

		public void TestEcommerceDestinationDepotMenuItem_WhenWebPortalHasNotBeenConfigured_ShowsErrorMessage()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				var menuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.NavigateToEcommerceWebPortals);
				var destinationDepot = menuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.NavigateToEcommerceDestinationDepot);

				HVLVMenuItemTestHelper.AssertWebPortalMenuItemShowsErrorMessage(destinationDepot);
			}
		}

		public void TestEcommerceOriginDepotMenuItem_WhenWebPortalHasNotBeenConfigured_ShowsErrorMessage()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				var menuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.NavigateToEcommerceWebPortals);
				var originDepot = menuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.NavigateToEcommerceOriginDepot);

				HVLVMenuItemTestHelper.AssertWebPortalMenuItemShowsErrorMessage(originDepot);
			}
		}

		public void TestUpdatePreScreeningStatusWhenBillToPartyChanges()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ABCDEF";
			var address1 = org1.Addresses.AddNew();
			var address2 = org1.Addresses.AddNew();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "FEDCBA";
			var address3 = org2.Addresses.AddNew();
			var address4 = org2.Addresses.AddNew();

			bookingHeader.HVH_OA_BillToParty = address1.PK;

			var preScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration();
			preScreeningConfiguration.IsEnabled = true;

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration))
			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				form.Show();
				bookingHeader.HVH_OA_BillToParty = address3.PK;
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				var consignment1 = bookingHeader.Consignments.AddNew();
				consignment1.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;

				bookingHeader.HVH_OA_BillToParty = address2.PK;
				Application.DoEvents();
				var message = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals("Message shall be same as messageContent in method", "HVLV Pre-Screening is enabled, changing the eTailer will set the Pre-Screening Status on all HVLV Consignments to Unknown.", message.Text);
				AssertEquals("Caption shall be same as messageContent in method", "eTailer", message.Caption);
				AssertEquals("Pre-Screening status of consignments shall be Unknown.", HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment1.HVC_PreScreeningStatus);

				UnitTestUserNotification.Instance.ClearMessages();
				bookingHeader.HVH_OA_BillToParty = address4.PK;
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				consignment1.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;
				bookingHeader.HVH_OA_BillToParty = address3.PK;
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			preScreeningConfiguration.IsEnabled = false;
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;
			bookingHeader2.HVH_OA_BillToParty = address3.PK;

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration))
			using (var form2 = new HVLVBookingHeaderForm(bookingHeader2))
			{
				form2.Show();
				bookingHeader2.HVH_OA_BillToParty = address1.PK;
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestJobComInvoiceLinePartSynchronisationManager()
		{
			var factory = new BusinessObjectFactory();
			var importer = factory.NewWithValidTestData<OrgHeader>();
			var supplier = factory.NewWithValidTestData<OrgHeader>();
			var part = factory.New<OrgSupplierPart>();
			part.RelatedOrganisations.AddOwner(importer);
			part.RelatedOrganisations.AddSupplier(supplier);
			part.OP_PartNum = "P001";
			part.OP_Desc = "DESC";
			factory.Save();

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			var item = bookingHeader.Consignments.AddNew().Items.AddNew();
			var shipment = Factory.New<ForwardingShipment>();
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			shipment.ConsignorPK = supplier.PK;
			var itemLine = item.Lines.AddNew();
			itemLine.HVS_ProductCode = part.OP_PartNum;
			AssertEquals(part.OP_Desc, itemLine.HVS_GoodsDescription);

			part.OP_Desc = "DESC1";
			factory.Save();
			AssertNotEquals(part.OP_Desc, itemLine.HVS_GoodsDescription);

			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				form.Show();

				part.OP_Desc = "DESC2";
				factory.Save();
				AssertEquals(part.OP_Desc, itemLine.HVS_GoodsDescription);
			}

			part.OP_Desc = "DESC3";
			factory.Save();
			AssertNotEquals(part.OP_Desc, itemLine.HVS_GoodsDescription);
		}

		public void TestMainTabPageChildrenHaveCorrectTabIndex()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				form.Show();
				var headerDetailsGroupBox = form.Controls.Find("headerDetailsGroupBox", true)[0] as ZGroupBox;

				AssertNotNull(headerDetailsGroupBox);
				AssertEquals("headerDetailsGroupBox has tab index 0", 0, headerDetailsGroupBox.TabIndex);

				var bookingHeaderConsignmentUserControl = form.Controls.Find("bookingHeaderConsignmentUserControl", true)[0] as HVLVConsignmentUserControl;

				AssertNotNull(bookingHeaderConsignmentUserControl);
				AssertEquals("bookingHeaderConsignmentUserControl has tab index 1", 1, bookingHeaderConsignmentUserControl.TabIndex);
			}
		}

		public void TestBookingHeaderFormIsReadOnlyWhenHVH_IsProcessedAtOriginDepotTrue()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_IsProcessedAtOriginDepot = true;
			var consignment = bookingHeader.Consignments.AddNew();
			var item = consignment.Items.AddNew();
			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				form.Show();
				var consignmentGrid = form.Controls.Find("Grid", true)[0] as ZGrid;
				consignmentGrid.SelectSingleElement(consignment);
				Assert(consignmentGrid.GetFirstSelectedRow().ReadOnly);

				var itemGrid = form.Controls.Find("itemsGrid", true)[0] as ZGrid;
				itemGrid.SelectSingleElement(item);
				Assert(itemGrid.GetFirstSelectedRow().ReadOnly);
			}
		}

		public void TestCreateTestConsignmentsMenuItem_ClickWhenFormIsNotSaved_ShouldNotOpenCreateTestForm()
		{
			AssertCreateTestFormNotOpenedWhenFormNotSaved(HVLVMenuItemHelper.Captions.CreateTestConsignments, "Please save the form before creating test consignments");
		}

		public void TestCreateTestLoadListMenuItem_ClickWhenFormIsNotSaved_ShouldNotOpenCreateTestForm()
		{
			AssertCreateTestFormNotOpenedWhenFormNotSaved(HVLVMenuItemHelper.Captions.CreateTestLoadlist, "Please save the form before creating test load list");
		}

		void AssertCreateTestFormNotOpenedWhenFormNotSaved(string menuCaption, string notificationMessage)
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				var menuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().SingleOrDefault(x => x.Text == menuCaption);
				AssertNotNull(menuItem);

				menuItem.PerformClick();
				AssertEquals("Should pop up saving notification before creating test data", notificationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateTestConsignmentsMenuItem_ClickWhenFormIsSaved_ShouldOpenCreateTestForm()
		{
			AssertCreateTestFormOpenedWhenFormIsSaved(HVLVMenuItemHelper.Captions.CreateTestConsignments, typeof(HVLVBookingHeaderCreateTestConsignmentForm));
		}

		public void TestCreateTestLoadListMenuItem_ClickWhenFormIsSaved_ShouldOpenCreateTestForm()
		{
			AssertCreateTestFormOpenedWhenFormIsSaved(HVLVMenuItemHelper.Captions.CreateTestLoadlist, typeof(HVLVBookingHeaderCreateTestLoadListForm));
		}

		void AssertCreateTestFormOpenedWhenFormIsSaved(string menuCaption, Type formType)
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			Factory.Save();

			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				var menuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().SingleOrDefault(x => x.Text == menuCaption);
				AssertNotNull(menuItem);

				menuItem.PerformClick();

				AssertType(formType, ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		void AssertCreateTestMenuItemVisible(bool isVisible, string menuCaption)
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				var menuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().SingleOrDefault(x => x.Text == menuCaption);
				AssertNotNull(menuItem);
				AssertEquals(menuItem.Visible, isVisible);
			}
		}

		public void TestCreateTestConsignmentsMenuItem_IsVisibleOnlyInTestAndWithTestUser()
		{
			AssertCreateTestMenuItemIsVisibleOnlyInTestAndWithTestUser(HVLVMenuItemHelper.Captions.CreateTestConsignments);
		}

		public void TestCreateTestLoadListMenuItem_IsVisibleOnlyInTestAndWithTestUser()
		{
			AssertCreateTestMenuItemIsVisibleOnlyInTestAndWithTestUser(HVLVMenuItemHelper.Captions.CreateTestLoadlist);
		}

		void AssertCreateTestMenuItemIsVisibleOnlyInTestAndWithTestUser(string menuCaption)
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertCreateTestMenuItemVisible(false, menuCaption);
			}

			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			using (EnvProxy.Instance.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertCreateTestMenuItemVisible(false, menuCaption);
			}

			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertCreateTestMenuItemVisible(true, menuCaption);
			}
		}

		public void TestCreateLoadListMenuItem_ShouldOpenForm_WhenButtonOKClicked()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			Factory.Save();

			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var menuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().SingleOrDefault(x => x.Text == HVLVMenuItemHelper.Captions.CreateTestLoadlist);
				AssertNotNull(menuItem);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((dialog) =>
				{
					var loadListTestDataForm = dialog as HVLVBookingHeaderCreateTestLoadListForm;
					var createdLoadList = loadListTestDataForm.GetType().GetProperty("CreatedLoadList", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
					createdLoadList.SetValue(loadListTestDataForm, Factory.New<HVLVOriginLoadList>());
					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				});

				menuItem.PerformClick();

				AssertEquals("HVLV Origin Load-list form should be display for users to fill in values", typeof(HVLVOriginLoadListForm), ZFormModaliser.LastFormShownForTest.GetType());
			}
		}

		public void TestAuditTabpage()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			using (var form = new HVLVBookingHeaderForm(bookingHeader))
			{
				var propertyInfo = form.GetType().GetProperty("ShowAuditTab", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				var propertyValue = (bool)propertyInfo.GetValue(form);
				AssertEquals(true, propertyValue);
			}
		}

		OrgAddress depotAddress1;
		OrgHeader carrier1;
		OrgHeader agent1;
	}
}
