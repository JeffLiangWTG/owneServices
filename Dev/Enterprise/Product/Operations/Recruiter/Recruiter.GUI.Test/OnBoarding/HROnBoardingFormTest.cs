using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Windows.Forms;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(HROnBoardingForm))]
	public class HROnBoardingFormTest : ZFormBasherTest
	{
		public void TestNoEdocs()
		{
			using var form = GetFormToBash();
			Assert("Disallow any viewing or editing of edocs through CW1, users must use HRMS and its appropriate entity security", !form.PlugIns.IsPlugInAvailable(ControllerIDs.eDocsPlugIn));
		}

		protected new HROnBoardingForm GetFormToBash()
			=> (HROnBoardingForm)base.GetFormToBash();

		protected override void SetUp()
		{
			base.SetUp();
			WebUrlLauncher.ClearLastUrlLaunched();
		}

		protected override Form GetFormToBashCore()
		{
			return new HROnBoardingForm(Factory.New<HROnBoarding>()) { ControllerID = ControllerIDs.HROnBoarding };
		}

		public void TestControlsReadOnly()
		{
			var hrOnBoarding = Factory.NewWithValidTestData<HROnBoarding>();
			using (var form = new TestHROnBoardingForm(hrOnBoarding))
			{
				form.Show();
				AssertCollectionNotContains(false, form.GuidFindBoxControlsExposed.Select(ctrl => ctrl.ReadOnly));
				AssertCollectionNotContains(false, form.ZTextBoxControlsExposed.Select(ctrl => ctrl.ReadOnly));
			}
		}

		public void TestControlsVisibility()
		{
			var hrOnBoarding = Factory.NewWithValidTestData<HROnBoarding>();
			using (var form = new TestHROnBoardingForm(hrOnBoarding))
			{
				form.Show();
				AssertEquals(true, form.Controls.Find("ApplicantGuidFindBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("BranchGuidFindBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("StartDateTextBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("WorkingBasisTextBox", true).Single().Visible);
			}
		}

		public void TestOnBoardingTabControl_TabPageOrder()
		{
			var hrOnBoarding = Factory.NewWithValidTestData<HROnBoarding>();
			using (var form = new TestHROnBoardingForm(hrOnBoarding)
			{ ControllerID = ControllerIDs.HROnBoarding })
			{
				form.Show();
				form.OnBoardingTabControlExposed.SelectedIndex = 0;
				AssertEquals("Workflow and Tracking tab page should be first", "Workflow && Tracking", form.OnBoardingTabControlExposed.SelectedTab.Text);
				form.OnBoardingTabControlExposed.SelectedIndex = 1;
				AssertEquals("Notes tab page should be second", "Notes", form.OnBoardingTabControlExposed.SelectedTab.Text);
				form.OnBoardingTabControlExposed.SelectedIndex = 2;
				AssertEquals("Logs tab page should be third", "Logs", form.OnBoardingTabControlExposed.SelectedTab.Text);
			}
		}

		public void TestOpenInBrowser()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var hrOnBoarding = Factory.NewWithValidTestData<HROnBoarding>();
			Factory.Save();

			using (var form = new TestHROnBoardingForm(hrOnBoarding))
			{
				form.Show();
				var button = (ZButton)form.Controls.Find("OnBoardingButton", true).Single();
				button.PerformClick();

				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var uri = new Uri(launchedUrl, UriKind.Absolute);
				var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
				AssertEquals("/goto/onBoarding", uri.AbsolutePath);
				AssertNotNull("A Glow Access Token should be attached", queryKeyValuePairs["sso_otp"]);
				AssertEquals(hrOnBoarding.PK.ToString(), queryKeyValuePairs["onboardingPK"]);
			}
		}

		public void TestOpenInBrowser_ShouldReturnErrorWhenGlowRegistryIsEmpty()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			var hrOnBoarding = Factory.NewWithValidTestData<HROnBoarding>();
			Factory.Save();

			using (var form = new TestHROnBoardingForm(hrOnBoarding))
			{
				form.Show();
				var button = (ZButton)form.Controls.Find("OnBoardingButton", true).Single();
				button.PerformClick();

				var assertMessage = "Should display error when GLOW URL was not configured in registry.";
				AssertEquals(assertMessage,
					@"The HR Onboarding record cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL",
					UnitTestUserNotification.Instance.LastMessage.Text);

				AssertNullOrEmpty("No URL was launched", WebUrlLauncher.LastUrlLaunched);
			}
		}

		class TestHROnBoardingForm : HROnBoardingForm
		{
			internal TabControl OnBoardingTabControlExposed => OnBoardingTabControl;
			public TestHROnBoardingForm(HROnBoarding businessEntity) : base(businessEntity)
			{
			}

			internal IReadOnlyList<ZGuidFindBox> GuidFindBoxControlsExposed => new List<ZGuidFindBox> { ApplicantGuidFindBox, BranchGuidFindBox };

			internal IReadOnlyList<ZTextBox> ZTextBoxControlsExposed => new List<ZTextBox> { StartDateTextBox, WorkingBasisTextBox };
		}
	}
}
