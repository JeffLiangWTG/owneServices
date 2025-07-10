using System;
using System.Windows.Forms;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ChangePasswordDialog : ZChildForm
	{
		public static DialogResult ChangePassword(GlbStaff staff) => ZFormModaliser.ShowDialogAndDispose(new ChangePasswordDialog(staff, true));

		public static DialogResult ResetPassword(GlbStaff staff) => ZFormModaliser.ShowDialogAndDispose(new ChangePasswordDialog(staff, false));

		internal ChangePasswordDialog(GlbStaff staff, bool requireOldPassword)
		{
			this.staff = staff;
			InitializeComponent();
			Embolden();
			this.BackgroundImage = BrandingFactory.Instance.LoginScreenImage;
			UserNameTextBox.Text = staff.GS_LoginName;
			OldPasswordTextBox.Enabled = requireOldPassword;
			this.MoveFormByMouseDrag(true);
		}

		public bool IsResetPassword => !OldPasswordTextBox.Enabled;

		void Embolden()
		{
			UserNameTextBox.GetExtension<LabelCaptionRenderer>().Font = OFont.GetFontBold();
			OldPasswordTextBox.GetExtension<LabelCaptionRenderer>().Font = OFont.GetFontBold();
			NewPasswordTextBox.GetExtension<LabelCaptionRenderer>().Font = OFont.GetFontBold();
			ConfirmPasswordTextBox.GetExtension<LabelCaptionRenderer>().Font = OFont.GetFontBold();
		}

		readonly GlbStaff staff;

		#region Implementation

		void ChangePasswordDialog_Load(object sender, EventArgs e)
		{
			ActiveControl = OldPasswordTextBox.Enabled ? OldPasswordTextBox : NewPasswordTextBox;
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			string error;
			var passwordControl = new PasswordControl();
			try
			{
				if (passwordControl.NewPasswordIsValid(staff, OldPasswordTextBox.Enabled ? OldPasswordTextBox.Text : null, NewPasswordTextBox.Text, ConfirmPasswordTextBox.Text, out error))
				{
					if (OldPasswordTextBox.Enabled)
					{
						staff.ChangePassword(OldPasswordTextBox.Text, NewPasswordTextBox.Text);
					}
					else
					{
						staff.ResetPassword(NewPasswordTextBox.Text);
					}

					DialogResult = DialogResult.OK;
				}
				else
				{
					SetError(error);
				}
			}
			catch (UserLockedOutException)
			{
				SetError(Res.GetString("f1bd00e0-9a79-4198-a49d-407a70cbb7bb", "Your login is currently locked out due to a previous failed login. Please try again later or see your system administrator if your account remains locked out after an extended period of time."));
			}
			catch (PasswordDoesNotMatchPolicyException)
			{
				SetError(Res.GetString("b85429eb-5c9a-42b8-b6a0-170ef6677cd2", "Unable to update the password. Either the old password is wrong or the value provided for the new password does not meet the length, complexity, or history requirements of the domain."));
			}
			catch (NoDomainPrivilegeException)
			{
				SetError(Res.GetString("0ea4044b-8612-4983-b5f2-0198b1bd5e75", "Unable to reset the password. Domain user does not have write privileges to the Organizational Units set in the registry '{0}'",
					ObjectFactory.Get<IADRegistry>().DomainCredentialsCollectionRegistryLocation));
			}
			catch (InvalidOperationException ex)
			{
				SetError(ex.Message);
			}
		}

		void SetError(string text)
		{
			ErrorLabel.Text = text;
		}

		void ClearError()
		{
			ErrorLabel.Text = "";
		}

		void Password_TextChanged(object sender, EventArgs e)
		{
			ClearError();
		}

		#endregion
	}
}
