using System;
using System.Linq;
using System.Web;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class WebSecurityUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestWebSecurityButtons()
		{
			var org = OrgHeader.New(Factory);
			using (ZForm form = new ZForm(org))
			using (var control = new WebSecurityUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(org, "");
				Application.DoEvents();
				AssertEquals(false, control.Controls.Find("SetWebSecurityButton", true).Single().GetReadOnly());
				AssertEquals(false, control.Controls.Find("SyncWebSecurityToNeoGroupButton", true).Single().GetReadOnly());
			}

			org.SetReadOnlyIncludingChildren(true);
			using (ZForm form = new ZForm(org))
			using (var control = new WebSecurityUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(org, "");
				Application.DoEvents();
				AssertEquals(true, control.Controls.Find("SetWebSecurityButton", true).Single().GetReadOnly());
				AssertEquals(true, control.Controls.Find("SyncWebSecurityToNeoGroupButton", true).Single().GetReadOnly());
			}
		}

		public void TestWebSecurityLinkLabel_Click()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			WebUrlLauncher.ClearLastUrlLaunched();
			AssertEquals("Precondition", string.Empty, WebUrlLauncher.LastUrlLaunched);
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var contact = org.Contacts.AddNew();
			Factory.Save();

			using (var form = new ZForm(org))
			using (var control = new WebSecurityUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(org, "");
				Application.DoEvents();

				var webSecurityLinkLabel = control.GetControl<ZLinkLabel>("SecurityInstructionsLabel");
				webSecurityLinkLabel.OnLinkClicked_Exposed(null);
				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var uri = new Uri(launchedUrl, UriKind.Absolute);
				var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
				var accessToken = queryKeyValuePairs["sso_otp"];
				var entityPK = queryKeyValuePairs["entityPK"];

				AssertEquals("Launched uri is correct.", "https", uri.Scheme);
				AssertEquals("Launched uri is correct.", "address", uri.Host);
				AssertEquals("Launched uri is correct.", "/goto/SSMOrganizationDetail", uri.AbsolutePath);
				AssertNotNull("A Glow Access Token should be attached", accessToken);
				AssertEquals("Expected URL to include the PK of organization", org.PK.ToString(), entityPK);
				AssertEquals("Launched uri is correct.", string.Empty, uri.Fragment);
				WebUrlLauncher.ClearLastUrlLaunched();
			}
		}

		[RequiresSTA]
		public void TestWebSecurityLinkLabel_ShouldOnlyShowLinkIfValidUri()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			var contact = org.Contacts.AddNew();
			Factory.Save();

			using (var form = new ZForm(org))
			using (var control = new WebSecurityUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(org, "");
				Application.DoEvents();

				var webSecurityLinkLabel = control.GetControl<ZLinkLabel>("SecurityInstructionsLabel");
				AssertEquals("Should not show GLOW section", "The following security rights are the default rights used for Web access to this system. Each contact will have these rights unless otherwise overridden.", webSecurityLinkLabel.Text);
			}

			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (var form = new ZForm(org))
			using (var control = new WebSecurityUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(org, "");
				Application.DoEvents();

				var webSecurityLinkLabel = control.GetControl<ZLinkLabel>("SecurityInstructionsLabel");
				AssertEquals("Should show GLOW section", "The following security rights are the default rights used for Web access to this system. Each contact will have these rights unless otherwise overridden. Click on CargoWise Web Portal User Administration to manage GLOW Web Security Rights.", webSecurityLinkLabel.Text);
			}
		}
	}
}
