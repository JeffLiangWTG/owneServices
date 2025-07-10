namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	using System;
	using CargoWise.Application;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Environment;
	using Enterprise.Freight.Forwarding.Orders.Business;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.GlowInterop;
	using Enterprise.ZArchitecture.GUI;
	using Moq;

	public class SupplierBookingUserControlTest : TestCaseWithFactory
	{
		public void TestClickDetailLink_WhenGlowPortalsNotConfigured()
		{
			var entity = Factory.NewWithValidTestData<JobSupplierBooking>();
			entity.JSB_BookingId = "ABC";
			var glowSingleSignOnTokenProvider = BuildMockForGlowSingleSignOnTokenProvider();

			using (var form = new SupplierBookingForm(entity))
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			using (ObjectFactory.Substitute(glowSingleSignOnTokenProvider.Object))
			{
				form.Show();

				var label = form.FindSingle<ZLinkLabel>("BookingDetailLinkLabel");
				AssertNotNull("Link Label should exist.", label);
				Assert("Link Label should be visible", label.Visible);

				label.OnLinkClicked_Exposed(new System.Windows.Forms.LinkLabelLinkClickedEventArgs(null));
				AssertEquals("Notification message is correct.", @"This Supplier Booking ABC cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestClickDetailLink_WhenGlowPortalsProperlyConfigured()
		{
			var entity = Factory.NewWithValidTestData<JobSupplierBooking>();
			var glowSingleSignOnTokenProvider = BuildMockForGlowSingleSignOnTokenProvider();
			var baseURL = "https://www.xxx.com";

			using (var form = new SupplierBookingForm(entity))
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, baseURL))
			using (ObjectFactory.Substitute(glowSingleSignOnTokenProvider.Object))
			{
				form.Show();

				var label = form.FindSingle<ZLinkLabel>("BookingDetailLinkLabel");
				AssertNotNull("Link Label should exist.", label);
				Assert("Link Label should be visible", label.Visible);
				AssertEquals("Click here to show Supplier Booking record", label.CaptionResourceString.Caption);

				label.OnLinkClicked_Exposed(new System.Windows.Forms.LinkLabelLinkClickedEventArgs(null));
				AssertEquals($"{baseURL}/Goto/JobSupplierBooking?sso_otp=SSOOTPTOKEN&entityPK={entity.PK}", WebUrlLauncher.LastUrlLaunched);
			}
		}

		public void TestSupplierBookingUserControlIsINotifications()
		{
			using (var supplierBookingUserControl = new SupplierBookingUserControl())
			{
				Assert("SupplierBookingUserControl implements INotifications.", supplierBookingUserControl is INotifications);
			}
		}

		static Mock<IGlowSingleSignOnTokenProvider> BuildMockForGlowSingleSignOnTokenProvider()
		{
			var glowSingleSignOnTokenProvider = new Mock<IGlowSingleSignOnTokenProvider>();
			glowSingleSignOnTokenProvider.Setup(h => h.CreateLimitedToken(Env.CurrentUserPK, "GS", It.IsAny<GlowSingleSignOnTokenOptions>())).Returns("SSOOTPTOKEN");
			return glowSingleSignOnTokenProvider;
		}
	}
}
