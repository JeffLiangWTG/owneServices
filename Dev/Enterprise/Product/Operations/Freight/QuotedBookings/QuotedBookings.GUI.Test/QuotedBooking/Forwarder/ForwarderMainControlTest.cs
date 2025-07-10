using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	public class ForwarderMainControlTest : QuotedBookingCustomizableControlTest<ForwarderMainControl>
	{
		public void TestDeniedPartyScreeningControlsVisibility()
		{
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);

			AssertDeniedPartyScreeningControlsVisibility(false, quickBooking, true);

			((ITemplateRecordProvider)quickBooking).IsTemplateRecord = false;
			AssertDeniedPartyScreeningControlsVisibility(true, quickBooking, false);

			((ITemplateRecordProvider)quickBooking).IsTemplateRecord = true;
			AssertDeniedPartyScreeningControlsVisibility(false, quickBooking, false);

			quickBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			((ITemplateRecordProvider)quickBooking).IsTemplateRecord = false;
			AssertDeniedPartyScreeningControlsVisibility(false, quickBooking, false);
		}

		void AssertDeniedPartyScreeningControlsVisibility(bool expectedResult, QuotedBooking quotedBooking, bool implementComplianceRisk)
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(implementComplianceRisk)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, implementComplianceRisk))
			{
				using (var form = new QuotedBookingForm(quotedBooking))
				{
					form.Show();
					var screeningControlsGroupBox = form.Controls.Find("ScreeningControlsGroupBox", true).First();
					var screeningStatusDropEdit = form.Controls.Find("screeningStatusDropEdit", true).First();
					var screenButton = form.Controls.Find("screenButton", true).First();
					CombineAssertions(string.Format(CultureInfo.InvariantCulture, "When Booking is Null: {0}, QuotedBooking is template: {1}", quotedBooking?.Booking == null, quotedBooking.IsTemplate), () =>
					{
						AssertEquals(expectedResult, screeningControlsGroupBox.Visible);
						AssertEquals(expectedResult, screeningStatusDropEdit.Visible);
						AssertEquals(expectedResult, screenButton.Visible);
					});
				}
			}
		}

		public void TestSetApprovedShipperInspectionVisibility()
		{
			QuotedBooking quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			using (var form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();
				var approvedShipperInspectionTypeDropEdit = (
					from ctrl in form.Controls.Find("ApprovedShipperInspectionTypeDropEdit", true)
					where ctrl.Parent.Name == "AviationSecurityPanel"
					select ctrl).First();
				AssertEquals(ZString.Empty, quotedBooking.Mode);
				AssertEquals(false, approvedShipperInspectionTypeDropEdit.Visible);
				quotedBooking.Mode = "LSE";
				AssertEquals(true, approvedShipperInspectionTypeDropEdit.Visible);
				quotedBooking.Mode = "LCL";
				AssertEquals(false, approvedShipperInspectionTypeDropEdit.Visible);
				quotedBooking.Mode = "ULD";
				AssertEquals(true, approvedShipperInspectionTypeDropEdit.Visible);
			}
		}

		public void TestSetCompTariffLevelVisibility()
		{
			QuotedBooking quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			using (var form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();
				var compTariffLevelDropEdit = (
					from ctrl in form.Controls.Find("CompTariffLevelDropEdit", true) select ctrl).First();
				AssertEquals(false, quotedBooking.ClientDocAddress.E2_AddressOverride);
				AssertEquals(true, compTariffLevelDropEdit.Visible);
				quotedBooking.ClientDocAddress.E2_AddressOverride = true;
				AssertEquals(true, compTariffLevelDropEdit.Visible);
			}
		}

		public void TestWhenCreateNewOneOffQuote_HBLDeliveryModeDropEditShouldBeVisible()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			using (var form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();
				var hblDeliveryModeDropEdit = form.Controls.Find("OneOffQuoteHBLDeliveryModeDropEdit", true).First();
				AssertEquals
				(
					"OneOffQuoteHBLDeliveryModeDropEdit control should be visible on One Off Quote Form.",
					true,
					hblDeliveryModeDropEdit.Visible
				);
			}
		}

		public void TestWhenCreateNewBookingWithQuote_HBLDeliveryModeDropEditShouldNotBeVisible()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			using (var form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();
				var hblDeliveryModeDropEdit = form.Controls.Find("OneOffQuoteHBLDeliveryModeDropEdit", true).First();
				AssertEquals
				(
					"OneOffQuoteHBLDeliveryModeDropEdit control should be invisible on Booking Form.",
					false,
					hblDeliveryModeDropEdit.Visible
				);
			}
		}

		public void TestSetOrgRoleVisibility()
		{
			using (RatingDataRegistry.Instance.EnableOverseasAgentInOneOffQuote.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
				using (var form = new QuotedBookingForm(quickBooking))
				{
					form.Show();
					var orgRoleDropEdit1 = (
						from ctrl in form.Controls.Find("OrgRoleDropEdit", true) select ctrl).First();
					AssertEquals(false, orgRoleDropEdit1.Visible);
				}

				var spotQuote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
				using (var form = new QuotedBookingForm(spotQuote))
				{
					form.Show();
					var orgRoleDropEdit2 = (
						from ctrl in form.Controls.Find("OrgRoleDropEdit", true) select ctrl).First();
					AssertEquals(true, orgRoleDropEdit2.Visible);
				}

				var bookingWithQuote = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
				using (var form = new QuotedBookingForm(bookingWithQuote))
				{
					form.Show();
					var orgRoleDropEdit3 = (
						from ctrl in form.Controls.Find("OrgRoleDropEdit", true) select ctrl).First();
					AssertEquals(true, orgRoleDropEdit3.Visible);
				}
			}
		}

		public void TestAdditionalContactsControl()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			QuotedBookingFormCustomisationSettingsProvider provider = new QuotedBookingFormCustomisationSettingsProvider(new QuotedBookingWorkflowDescriptor());
			using (var form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();
				Application.DoEvents();
				var additionalContactsControl = form.Controls.Find("AdditionalContacts", true).First();
				EnsureTabPageIsSelected(additionalContactsControl);
				var displayFields = provider.DisplayFields.Cast<FormCustomisableElement>().Where(x => x.ElementGroup == "AdditionalContacts").ToArray();
				AssertControlContainsCustomizableElement(additionalContactsControl, displayFields);
			}
		}

		public void TestSetDeliveryDueDateVisibility()
		{
			Env.Security.QuickBookingDeliveryDueDateOverride.IsAllowed = true;

			void AssertCalculateDDDMenuItem(QuotedBookingForm form, bool shouldBeAvailable)
			{
				form.Show();
				var calculateDeliveryDueDateMenuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Calculate Delivery Due Date");
				AssertEquals(shouldBeAvailable, calculateDeliveryDueDateMenuItem != null);
			}

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var oneOffQuote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
				oneOffQuote.TransportMode = Core.Constants.TransportModes.Air;
				using (var form = new QuotedBookingForm(oneOffQuote))
				{
					AssertCalculateDDDMenuItem(form, false);
				}

				var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
				quotedBooking.TransportMode = Core.Constants.TransportModes.Air;
				using (var form = new QuotedBookingForm(quotedBooking))
				{
					AssertCalculateDDDMenuItem(form, true);
				}
			}

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var oneOffQuote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
				oneOffQuote.TransportMode = Core.Constants.TransportModes.Air;
				using (var form = new QuotedBookingForm(oneOffQuote))
				{
					AssertCalculateDDDMenuItem(form, false);
				}

				var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
				quotedBooking.TransportMode = Core.Constants.TransportModes.Air;
				using (var form = new QuotedBookingForm(quotedBooking))
				{
					AssertCalculateDDDMenuItem(form, false);
				}
			}
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

		public void TestCarrierContractControlsHiddenForQuoteWithoutBooking()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingFormForTesting(quotedBooking))
			{
				form.Show();
				AssertEquals(false, form.CarrierContractNumberBoundTextEdit.Visible);
				AssertEquals(false, form.CarrierContractNumberFindBox.Visible);
				AssertEquals(false, form.AllocationRouteCodeFindBox.Visible);
			}
		}

		public void TestCarrierContractLookupsConditionalVisibility()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingFormForTesting(quotedBooking))
			{
				form.Show();
				AssertEquals(false, form.CarrierContractNumberBoundTextEdit.Visible);
				AssertEquals(true, form.CarrierContractNumberFindBox.Visible);
				AssertEquals(true, form.AllocationRouteCodeFindBox.Visible);
			}

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new QuotedBookingFormForTesting(quotedBooking))
			{
				form.Show();
				AssertEquals(true, form.CarrierContractNumberBoundTextEdit.Visible);
				AssertEquals(false, form.CarrierContractNumberFindBox.Visible);
				AssertEquals(false, form.AllocationRouteCodeFindBox.Visible);
			}

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new QuotedBookingFormForTesting(quotedBooking))
			{
				form.Show();
				AssertEquals(true, form.CarrierContractNumberBoundTextEdit.Visible);
				AssertEquals(false, form.CarrierContractNumberFindBox.Visible);
				AssertEquals(false, form.AllocationRouteCodeFindBox.Visible);
			}
		}

		public void TestCreditorAndCarrierFieldIsVisible()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingFormForTesting(quotedBooking))
			{
				form.Show();
				AssertEquals(true, form.CarrierGuidFindBox.Visible);
				AssertEquals(true, form.CreditorGuidFindBox.Visible);
			}
		}

		void EnsureTabPageIsSelected(Control control)
		{
			var parent = control.Parent;
			if (parent != null)
			{
				EnsureTabPageIsSelected(parent);
				if (control is ZTabPage tab && control.Parent is ZTabControl tabControl)
				{
					tabControl.SelectedTab = tab;
					Application.DoEvents();
				}
			}
		}

		class QuotedBookingFormForTesting : QuotedBookingForm
		{
			internal QuotedBookingFormForTesting(QuotedBooking quotedBooking)
				: base(quotedBooking)
			{
			}

			internal Control GetChildControl(string controlName)
			{
				return (Control)base.QuotedBookingDetailsControl.GetType().GetField(controlName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(QuotedBookingDetailsControl);
			}

			internal QuotedBookingContractAllocationGuidFindBox AllocationRouteCodeFindBox => (QuotedBookingContractAllocationGuidFindBox)GetChildControl(nameof(AllocationRouteCodeFindBox));
			internal QuotedBookingContractAllocationCodeFindBox CarrierContractNumberFindBox => (QuotedBookingContractAllocationCodeFindBox)GetChildControl(nameof(CarrierContractNumberFindBox));
			internal ZTextBox CarrierContractNumberBoundTextEdit => (ZTextBox)GetChildControl(nameof(CarrierContractNumberBoundTextEdit));
			internal ZGuidFindBox CarrierGuidFindBox => (ZGuidFindBox)GetChildControl(nameof(CarrierGuidFindBox));
			internal ZGuidFindBox CreditorGuidFindBox => (ZGuidFindBox)GetChildControl(nameof(CreditorGuidFindBox));
		}

		protected override string TabName
		{
			get
			{
				return "MainTabPage";
			}
		}

		protected override string[] ExpectedPanelNames
		{
			get
			{
				return new string[] { CustomizablePanelNames.LeftTopPanel, CustomizablePanelNames.LeftBottomPanel, CustomizablePanelNames.MiddleTopPanel, CustomizablePanelNames.MiddleBottomPanel, CustomizablePanelNames.RightTopPanel, CustomizablePanelNames.RightBottomPanel };
			}
		}
	}
}
