using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OrgSecurityProfileControl))]
	sealed class OrgSecurityProfileControlTest : RegistryZUserControlTestCase
	{
		protected override RegistryZUserControl GetNewControl() => new OrgSecurityProfileControl();

		protected override IBusiness GetNewBusinessEntity() => new OrgSecurityProfileCollection();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return (control.Controls.Find("ProfileGrid", true)[0] as ZGrid).ReadOnly || businessEntity.IsReadOnly;
		}

		public void TestManageGlowSecurityButtonNoPortalsUriShouldDisableButton()
		{
			AssertEquals("Precondition", string.Empty, GlowRegistry.Instance.GlowPortalsUri.Value);

			using (var control = (OrgSecurityProfileControl)GetNewControl())
			{
				var manageGlowSecurityButton = control.GetControl<ZButton>("manageGlowSecurityButton");
				AssertEquals("Should be disabled if portals uri not set", false, manageGlowSecurityButton.Enabled);
			}
		}

		public void TestManageGlowSecurityButton_Click()
		{
			WebUrlLauncher.ClearLastUrlLaunched();
			AssertEquals("Precondition", string.Empty, WebUrlLauncher.LastUrlLaunched);
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (var control = (OrgSecurityProfileControl)GetNewControl())
			{
				var manageGlowSecurityButton = control.GetControl<ZButton>("manageGlowSecurityButton");
				AssertEquals("Should be enabled if portals uri is set", true, manageGlowSecurityButton.Enabled);

				manageGlowSecurityButton.PerformClick();

				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var uri = new Uri(launchedUrl, UriKind.Absolute);

				AssertEquals("Launched uri is correct.", "https", uri.Scheme);
				AssertEquals("Launched uri is correct.", "address", uri.Host);
				AssertEquals("Launched uri is correct.", "/goto/StaffSecurityManagement", uri.AbsolutePath);
				AssertEquals("Launched uri is correct.", string.Empty, uri.Fragment);
				WebUrlLauncher.ClearLastUrlLaunched();
			}
		}
	}
}
