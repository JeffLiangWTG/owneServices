namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	using System;
	using System.Windows.Forms;
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
	using NUnit.Framework;

	public class ContainerLoadPlanUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestClickDetailLink_WhenGlowPortalsNotConfigured()
		{
			var entity = Factory.NewWithValidTestData<CFSContainerLoadList>();
			entity.CLH_LoadListId = "ABC";
			var glowSingleSignOnTokenProvider = BuildMockForGlowSingleSignOnTokenProvider();

			using (var form = new ContainerLoadPlanForm(entity))
			using (ObjectFactory.Substitute(glowSingleSignOnTokenProvider.Object))
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				form.Show();

				var label = form.FindSingle<ZLinkLabel>("LoadPlanContainerDetailLinkLabel");
				AssertNotNull("Link Label should exist.", label);
				Assert("Link Label should be visible", label.Visible);
				AssertEquals("Click here to show Container Load Plan record", label.CaptionResourceString.Caption);

				label.OnLinkClicked_Exposed(new LinkLabelLinkClickedEventArgs(null));
				AssertEquals("Notification message is correct.", @"This Container Load List ABC cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestClickDetailLink()
		{
			PrepareProperlyConfiguredGlowLink((baseURL, entity, label) =>
			{
				label.OnLinkClicked_Exposed(new LinkLabelLinkClickedEventArgs(null));
				AssertEquals($"{baseURL}/Goto/ContainerLoadPlan?sso_otp=SSOOTPTOKEN&entityPK={entity.PK}", WebUrlLauncher.LastUrlLaunched);
			});
		}

		public void TestClickDetailLink_ForNonSupportedLoadMode_ShouldThrowException()
		{
			PrepareProperlyConfiguredGlowLink((baseURL, entity, label) =>
			{
				entity.CLH_LoadMode = "***";
				AssertExceptionThrown<ArgumentException>(() => { label.OnLinkClicked_Exposed(new LinkLabelLinkClickedEventArgs(null)); });
			});
		}

		public void TestContainerLoadListUserControlIsINotifications()
		{
			using (var containerLoadListUserControl = new ContainerLoadListUserControl())
			{
				Assert("ContainerLoadListUserControl implements INotifications.", containerLoadListUserControl is INotifications);
			}
		}

		#region Implementation

		static Mock<IGlowSingleSignOnTokenProvider> BuildMockForGlowSingleSignOnTokenProvider()
		{
			var glowSingleSignOnTokenProvider = new Mock<IGlowSingleSignOnTokenProvider>();
			glowSingleSignOnTokenProvider.Setup(h => h.CreateLimitedToken(Env.CurrentUserPK, "GS", It.IsAny<GlowSingleSignOnTokenOptions>())).Returns("SSOOTPTOKEN");
			return glowSingleSignOnTokenProvider;
		}

		void PrepareProperlyConfiguredGlowLink(Action<string, CFSContainerLoadList, ZLinkLabel> assertAction)
		{
			var entity = Factory.NewWithValidTestData<CFSContainerLoadList>();
			var glowSingleSignOnTokenProvider = BuildMockForGlowSingleSignOnTokenProvider();

			var baseURL = "https://www.xxx.com";
			using (var form = new ContainerLoadPlanForm(entity))
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, baseURL))
			using (ObjectFactory.Substitute(glowSingleSignOnTokenProvider.Object))
			{
				form.Show();

				var label = form.FindSingle<ZLinkLabel>("LoadPlanContainerDetailLinkLabel");
				AssertNotNull("Link Label should exist.", label);
				Assert("Link Label should be visible", label.Visible);
				assertAction(baseURL, entity, label);
			}
		}

		#endregion
	}
}
