using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.BrowserInterop;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgSecurityProfileControl : RegistryZUserControl
	{
		public OrgSecurityProfileControl()
		{
			InitializeComponent();

			if (!CustomerSelfManagementEnabled.Value)
			{
				ProfileGrid.ColumnStyles.Remove(ProfileGrid.ColumnStyles.OfType<ZCheckBoxColumnStyleInfo>()
					.Single(x => x.ColumnName == OrgSecurityProfile.Schema.Published));

				SettingsGrid.ColumnStyles.Remove(SettingsGrid.ColumnStyles.OfType<ZCheckBoxColumnStyleInfo>()
					.Single(x => x.ColumnName == OrgSecurityProfileSetting.Schema.CustomerManaged));
			}

			if (string.IsNullOrEmpty(URLHelpers.GlowPortalsUri))
			{
				manageGlowSecurityButton.Enabled = false;
			}

			DisclaimerLabel.CaptionResourceString = Res.GetData("OrgSecurityProfileControl|Disclaimer",
				@"Apply Security Profile to ALL Organizations

* This can be a time-consuming process.
* This process cannot be reversed.

If this process is run during production hours on large systems it can potentially slow operational processing.
The safest approach on a larger heavily used system is to run this process out of business hours.");
		}

		public static readonly Overridable<bool> CustomerSelfManagementEnabled = new Overridable<bool>();

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ProfileGrid.ReadOnly = readOnly;
			SettingsGrid.ReadOnly = readOnly;
			NewButton.ReadOnly = readOnly;
			BulkUpdateButton.ReadOnly = readOnly;
		}

		void NewButton_Click(object sender, System.EventArgs e)
		{
			var newProfile = ((OrgSecurityProfileCollection)DataSource).AddNew();
			newProfile.OrgSecuritySettings.PopulateDefaultSettings();
			newProfile.Name = Res.GetString("OrgSecurityProfileControl|NewProfile", "New Profile");
			ProfileGrid.SelectSingleElement(newProfile);
		}

		void BulkUpdateButton_Click(object sender, System.EventArgs e)
		{
			if (FindForm() is RegistryForm registryForm && registryForm != null)
			{
				if (registryForm.HasChanges)
				{
					Globals.Message.ShowError(Res.GetString("b2044e96-550a-4279-aa6b-a255541b400b", "Please save all changes first before updating organizations."));
				}
				else
				{
					ZFormModaliser.ShowDialogAndDispose(new OrgSecurityProfileUpdateForm(new OrgSecurityProfileUpdater(), true));
				}
			}
		}

		void ManageGlowSecurityButton_Click(object sender, System.EventArgs e)
		{
			if (URLHelpers.TryGenerateURL("goto/StaffSecurityManagement", out var url))
			{
				WebUrlLauncher.Launch(url.ToString());
			}
			else
			{
				ErrorReporter.ReportOnce("Could not generate SSM url");
				Globals.Message.ShowError(Res.GetString("4fa587b8-f67d-4bc8-8d99-dc5236ccd259", "Could not generate URL."));
			}
		}
	}
}
