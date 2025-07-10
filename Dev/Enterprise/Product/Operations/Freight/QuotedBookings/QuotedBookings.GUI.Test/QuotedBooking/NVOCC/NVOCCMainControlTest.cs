using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	public class NVOCCMainControlTest : TestCaseWithFactory
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
				using (var control = new NVOCCMainControl())
				{
					form.Controls.Add(control);
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

		public void TestCreditorFieldIsVisible()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingForm(quotedBooking))
			using (var control = new NVOCCMainControl())
			{
				form.Controls.Add(control);
				form.Show();
				var carrierGuidFindBox = form.Controls.Find("ScreeningControlsGroupBox", true).First();
				var creditorGuidFindBox = form.Controls.Find("screeningStatusDropEdit", true).First();
				
				AssertEquals(true, carrierGuidFindBox.Visible);
				AssertEquals(true, creditorGuidFindBox.Visible);
			}
		}
	}
}
