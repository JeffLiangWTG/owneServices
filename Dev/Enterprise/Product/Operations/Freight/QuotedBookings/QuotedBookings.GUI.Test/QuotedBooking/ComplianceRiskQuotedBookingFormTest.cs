using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	public class ComplianceRiskQuotedBookingFormTest : BaseFreightTest
	{
		public void TestCompliancePotentialRiskMessageBannerWhenValidRegisrtySecurityShowVisibleON()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			var complianceRisk = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRisk.COR_ParentTableCode = booking.TablePrefix;
			complianceRisk.COR_ParentID = booking.PK;

			Factory.Save();

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableComplianceWarningMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();
				complianceRisk.COR_OverallRisk = "PSK";
				Application.DoEvents();

				var riskBanner = form.MainStatusBar.FindSingle<ZLabel>(x => x.Name == "CompliancePotentialRiskMessageBanner");
				AssertEquals("Job Compliance status is not Clear. View the Compliance Risk tab.", riskBanner.Text);
				AssertEquals(true, riskBanner.Visible);
			}
		}

		public void TestCompliancePotentialRiskMessageBannerWhenInvalidRegisrtySecurityShowVisibleOFf()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			var complianceRisk = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRisk.COR_ParentTableCode = booking.TablePrefix;
			complianceRisk.COR_ParentID = booking.PK;
			complianceRisk.COR_OverallRisk = ComplianceRisk.Integration.ComplianceRiskStatusCodeList.Codes.OverrideClear;

			Factory.Save();

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableComplianceWarningMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new QuotedBookingForm(quotedBooking))
			{
				form.Show();
				Application.DoEvents();

				var riskBanner = form.MainStatusBar.FindSingleOrDefault<ZLabel>(x => x.Name == "CompliancePotentialRiskMessageBanner");
				AssertNull(riskBanner);
			}
		}

		public void TestCompliancePotentialRiskMessageBannerVisibleOffWhenOneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quickBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			AssertNull(quickBooking.Booking);

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableComplianceWarningMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingForm(quickBooking))
			{
				form.Show();
				Application.DoEvents();

				var riskBanner = form.MainStatusBar.FindSingleOrDefault<ZLabel>(x => x.Name == "CompliancePotentialRiskMessageBanner");
				AssertNull(riskBanner);
			}
		}
	}
}
