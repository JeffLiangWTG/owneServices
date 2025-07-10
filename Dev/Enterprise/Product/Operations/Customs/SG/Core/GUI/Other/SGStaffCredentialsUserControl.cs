using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class SGStaffCredentialsUserControl : MasterFiles.GUI.StaffCredentialsUserControl
	{
		public SGStaffCredentialsUserControl()
		{
			InitializeComponent();
			VisibleOnlyToDeveloperTextBox.Visible = GlbStaff.CurrentUser.GS_IsDeveloper;
			AccessVisibleOnlyToDeveloperTextBox.Visible = GlbStaff.CurrentUser.GS_IsDeveloper;
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			UnHookEvents();

			base.OnCurrentDataItemChanged(e);
			password = (CurrentDataItem as SGGlbStaffWrapper)?.AccessPassword;

			HookEvents();
		}

		void UnHookEvents()
		{
			if (password != null)
			{
				password.ShouldDefaultPasswordsEvent -= ShouldDefaultPasswords;
			}
		}

		void HookEvents()
		{
			if (password != null)
			{
				password.ShouldDefaultPasswordsEvent += ShouldDefaultPasswords;
			}
		}

		ZBool ShouldDefaultPasswords()
		{
			var message = Res.GetString("BAEDCFA1-82E5-431E-B8B5-64281C2D9DD4", "Would you like to default the password and status from the linked staff record which is using same ACCESS User ID in this company?");
			return Globals.Message.Show(message, AccessGroupBox.CaptionResourceString.Caption, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnHookEvents();
			}

			base.Dispose(disposing);
		}

		GlbExternalPassword_SGA password;
	}
}
