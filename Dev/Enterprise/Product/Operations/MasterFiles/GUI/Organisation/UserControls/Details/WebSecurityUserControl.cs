using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.BrowserInterop;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public partial class WebSecurityUserControl : ZUserControl
	{
		public WebSecurityUserControl()
		{
			InitializeComponent();

			if (!CustomerSelfManagementEnabled.Value)
			{
				SecurityRightsGrid.ColumnStyles.Remove(SecurityRightsGrid.ColumnStyles.OfType<ZCheckBoxColumnStyleInfo>()
					.Single(x => x.ColumnName == OrgSecuritySchema.Constants.OX_IsCustomerManaged));
			}

			if (string.IsNullOrEmpty(URLHelpers.GlowPortalsUri))
			{
				SecurityInstructionsLabel.Links.Clear();
			}
			else
			{
				SecurityInstructionsLabel.Text = ResString.GetMultilingualString("WebSecurityUserControl|webSecurityLinkLabel", "The following security rights are the default rights used for Web access to this system. Each contact will have these rights unless otherwise overridden. Click on {0} to manage GLOW Web Security Rights.", CargoWiseWebPortalUserAdmin);
				var indexOfCargoWiseWebPortalUserAdmin = SecurityInstructionsLabel.Text.IndexOf(CargoWiseWebPortalUserAdmin);

				if (indexOfCargoWiseWebPortalUserAdmin > 0)
				{
					SecurityInstructionsLabel.LinkArea = new LinkArea(indexOfCargoWiseWebPortalUserAdmin, CargoWiseWebPortalUserAdmin.Length);
				}
				else
				{
					ErrorReporter.ReportOnce("CargoWiseWebPortalUserAdmin link is invalid.", FormattableString.Invariant($"Text is {SecurityInstructionsLabel.Text}"));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "User admin strings")]
		const string CargoWiseWebPortalUserAdmin = "CargoWise Web Portal User Administration";

		public static readonly Overridable<bool> CustomerSelfManagementEnabled = new Overridable<bool>();

		void zLinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			OpenDeveloperGuideURL();
		}

		void OpenDeveloperGuideURL()
		{
			WebUrlLauncher.Launch("http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20080225.pdf");
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var org = (OrgHeader)CurrentDataItem;
			SetWebSecurityButton.Enabled = SyncWebSecurityToNeoGroupButton.Enabled = !org?.SecurityRights.ReadOnly ?? false;
			SyncWebSecurityToNeoGroupButton.Visible = org?.IsWebSecuritySyncToNeoSupported ?? false;
		}

		void SetWebSecurityButton_Click(object sender, EventArgs e)
		{
			var org = (OrgHeader)CurrentDataItem;
			if (org != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new OrgSecurityProfileUpdateForm(new OrgSecurityProfileUpdater(org)));
			}
		}

		void SyncWebSecurityToNeoGroupButton_Click(object sender, EventArgs e)
		{
			var org = (OrgHeader)CurrentDataItem;
			if (org != null && org.IsInDatabase)
			{
				if (DialogResult.OK == Globals.Message.ShowConfirmation(
					Res.GetString("aaaa6dd7-2ece-4cc5-930e-6d7a60820ebc", @"This action will sync the current WebTracker security rights of the contacts of this organization to the Portal User Administration. It will override all existing Neo Groups set up in Portal User Administration against all contacts for this organization. Non-WebTracker/Neo related groups assigned to the contacts of this organization will not be affected.

This process is irreversible. Do you wish to continue?"),
					Res.GetString("83784a3b-44eb-4388-8715-b9d2d1a600a6", "Warning"),
					Res.GetString("86a978a0-5a04-4150-8ed3-d48f6a9b2d90", "I CONFIRM THAT I WANT TO SYNC THE SECURITY ITEMS."),
					MessageBoxIcon.Question))
				{
					org.SyncWebSecurityToNeoGroup();
					Globals.Message.Show(Res.GetString("5b4f021c-b5fd-4b32-95ad-c55ed3ae0c38", "Operation has been successful"));
				}
			}
		}

		void GlowPortalLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var org = (OrgHeader)CurrentDataItem;
			var url = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("goto/SSMOrganizationDetail", org.HumanReadableName, additionalQueryStrings: new[] { ("entityPK", org.PK.ToString()) });
			if (url != null)
			{
				WebUrlLauncher.Launch(url.ToString());
			}
			else
			{
				ErrorReporter.ReportOnce("Could not generate SSM url");
				Globals.Message.ShowError(Res.GetString("22ce125e-b8a0-46ca-88dd-0180e5955d96", "Could not generate URL."));
			}
		}
	}
}
