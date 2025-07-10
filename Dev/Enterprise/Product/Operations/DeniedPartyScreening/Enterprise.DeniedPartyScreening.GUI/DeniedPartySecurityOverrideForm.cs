using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class DeniedPartySecurityOverrideForm : ZChildForm
	{
		public DeniedPartySecurityOverrideForm(DpsSecurityOverride deniedPartySecurityOverride)
			: base()
		{
			this.deniedPartySecurityOverride = deniedPartySecurityOverride;
			InitializeComponent();
		}

		readonly DpsSecurityOverride deniedPartySecurityOverride;

		public bool IsCancelled => isCancelled;

		internal string UserName => usernameText.Text;

		internal void okButton_Click(object sender, EventArgs e)
		{
			var errorMessage = deniedPartySecurityOverride.CheckUserPrivilege(usernameText.Text, passwordText.Text);
			if (string.IsNullOrEmpty(errorMessage))
			{
				isCancelled = false;
				Close();
			}
			else
			{
				UpdateStatusBar(errorMessage, CargoWise.ComponentModel.NotificationType.Error);
			}
		}

		void cancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		bool isCancelled = true;
	}
}
