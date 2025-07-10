using System;
using System.Linq;
using System.Web;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(GlbStaffChangeRequestForm))]
	public class GlbStaffChangeRequestFormTest : ZFormBasherTest
	{
		public void TestNoEdocs()
		{
			using var form = GetFormToBash();
			Assert("Disallow any viewing or editing of edocs through CW1, users must use HRMS and its appropriate entity security", !form.PlugIns.IsPlugInAvailable(ControllerIDs.eDocsPlugIn));
		}

		protected new GlbStaffChangeRequestForm GetFormToBash()
			=> (GlbStaffChangeRequestForm)base.GetFormToBash();

		protected override Form GetFormToBashCore() => new GlbStaffChangeRequestForm(ChangeRequest);

		protected override bool AllowHasChangesOnFormOpen => true;

		public override bool AllowUntranslatableFormTitle() => true;

		[RequiresSTA]
		public void TestControlsReadOnly()
		{
			using (var form = new GlbStaffChangeRequestFormForTest(ChangeRequest))
			{
				form.Show();
				Assert("Change Request Template is ReadOnly", form.TemplateGuidFindBoxExposed.ReadOnly);
				Assert("Change Request Status is ReadOnly", form.StatusTextBoxExposed.ReadOnly);
			}
		}

		public void TestControlsVisibility()
		{
			using (var form = new GlbStaffChangeRequestFormForTest(ChangeRequest))
			{
				form.Show();
				AssertEquals(true, form.Controls.Find("TemplateGuidFindBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("StatusTextBox", true).Single().Visible);
				AssertEquals(true, form.Controls.Find("ChangeRequestButton", true).Single().Visible);
			}
		}

		public void TestOpenInBrowser()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (var form = new GlbStaffChangeRequestFormForTest(ChangeRequest))
			{
				form.Show();
				var button = (ZButton)form.Controls.Find("ChangeRequestButton", true).Single();
				WebUrlLauncher.ClearLastUrlLaunched();
				button.PerformClick();

				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var uri = new Uri(launchedUrl, UriKind.Absolute);
				var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
				AssertEquals("/goto/changeRequest", uri.AbsolutePath);
				AssertNotNull("A Glow Access Token should be attached", queryKeyValuePairs["sso_otp"]);
				AssertEquals(ChangeRequest.PK.ToString(), queryKeyValuePairs["changeRequestPK"]);
			}
		}

		protected override void SetUp()
		{
			ChangeRequestTemplate = Factory.New<GlbStaffChangeRequestTemplate>();
			ChangeRequestTemplate.GSG_TemplateName = "DummyTemplateName";
			ChangeRequestTemplate.GSG_Code = "123";
			ChangeRequest = Factory.New<GlbStaffChangeRequest>();
			ChangeRequest.GCR_GSG_Template = ChangeRequestTemplate.PK;
		}

		protected GlbStaffChangeRequestTemplate ChangeRequestTemplate;
		protected GlbStaffChangeRequest ChangeRequest;

		public void TestOpenInBrowser_ShouldReturnErrorWhenGlowRegistryIsEmpty()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			using (var form = new GlbStaffChangeRequestFormForTest(ChangeRequest))
			{
				form.Show();
				var button = (ZButton)form.Controls.Find("ChangeRequestButton", true).Single();
				WebUrlLauncher.ClearLastUrlLaunched();
				button.PerformClick();

				var assertMessage = "Should display error when GLOW URL was not configured in registry.";
				AssertEquals(assertMessage,
					@"The Change Request record cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL",
					UnitTestUserNotification.Instance.LastMessage.Text);

				AssertNullOrEmpty("No URL was launched", WebUrlLauncher.LastUrlLaunched);
			}
		}

		class GlbStaffChangeRequestFormForTest : GlbStaffChangeRequestForm
		{
			public GlbStaffChangeRequestFormForTest(GlbStaffChangeRequest businessEntity) : base(businessEntity)
			{
			}

			internal TabControl MainTabControlExposed => base.MainTabControl;

			internal ZGuidFindBox TemplateGuidFindBoxExposed => TemplateGuidFindBox;

			internal ZTextBox StatusTextBoxExposed => StatusTextBox;
		}
	}
}
