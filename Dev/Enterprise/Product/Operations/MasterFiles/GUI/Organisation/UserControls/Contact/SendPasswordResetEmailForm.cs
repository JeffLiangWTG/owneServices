using System;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SendPasswordResetEmailForm : ZChildForm
	{
		public SendPasswordResetEmailForm() : base()
		{
			InitializeComponent();
			SetSenderLabelText(Env.Registry.SMTPDefaultDoNotReplyEmailAddress);
			#region Test Config
#if DEBUG
			if (Globals.IsTest)
			{
				useCurrentEmailCheckBox.Checked = UseCurrentUserEmailOverriddenForTest;
			}
#endif
			#endregion
		}

		void SetSenderLabelText(string senderEmail)
		{
			var currentEmailText = Res.GetString("2544B275-B68A-4906-9210-569A32C0750A", "Email will be sent from: {0}", senderEmail);
			fromEmailLabel.Text = currentEmailText;
		}

		void UseCurrentEmailCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			string emailAddress;
			if (useCurrentEmailCheckBox.Checked)
			{
				emailAddress = GlbStaff.CurrentUser.GS_EmailAddress;
			}
			else
			{
				emailAddress = Env.Registry.SMTPDefaultDoNotReplyEmailAddress;
			}
			if (string.IsNullOrEmpty(emailAddress))
			{
				emailAddress = Env.Registry.SMTPDefaultReturnEmailAddress;
			}
			SetSenderLabelText(emailAddress);
		}

		#region Test Config
#if DEBUG
		readonly static Overridable<bool> useCurrentUserEmailOverriddenForTest = new Overridable<bool>(false);
		internal static bool UseCurrentUserEmailOverriddenForTest { get => useCurrentUserEmailOverriddenForTest.Value; set => useCurrentUserEmailOverriddenForTest.Value = value; }
#endif
		#endregion
	}
}
