using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using CommercialInvoiceConstants = Enterprise.GlobalCommercialInvoice.Integration.Constants;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	public class GlobalCommercialInvoiceQuotedBookingFormTest : TestCaseWithFactory
	{
		public void TestGlobalCommercialInvoice_PluginVisibility()
		{
			AssertComplianceCommercialInvoicePluginVisibility(false);
			AssertComplianceCommercialInvoicePluginVisibility(true);

			void AssertComplianceCommercialInvoicePluginVisibility(bool registryValue)
			{
				using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(registryValue)))
				using (var form = new QuotedBookingFormTest.QuotedBookingFormForTest(GetQuotedBooking()))
				{
					CombineAssertions(@$"Global Compliance Commercial Invoice new feature registry setting {registryValue}", () =>
					{
						var complianceRiskPlugin = form.PlugIns.GetPlugIn(ControllerIDs.GlobalCommercialInvoicePlugin);
						if (registryValue)
						{
							AssertNotNull(complianceRiskPlugin);
						}
						else
						{
							AssertNull(complianceRiskPlugin);
						}
					});
				}
			}
		}

		public void TestGlobalCommercialInvoice_PluginTabPageName()
		{
			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(true)))
			using (var form = new QuotedBookingFormTest.QuotedBookingFormForTest(GetQuotedBooking()))
			{
				form.Show();

				((ISupportSwitchTabPage)form).SwitchTabPage(CommercialInvoiceConstants.PluginTabPageName);
				Application.DoEvents();

				AssertEquals(CommercialInvoiceConstants.PluginTabPageName, form.MainTabControlExposed.SelectedTab.Name);
				form.Dispose();
			}
		}

		QuotedBooking GetQuotedBooking()
		{
			return QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
		}
	}
}
