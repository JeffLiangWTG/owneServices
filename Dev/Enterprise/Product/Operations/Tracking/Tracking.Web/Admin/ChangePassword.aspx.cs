using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.Tracking.Web.Admin
{
	public partial class ChangePassword : BasePage
	{
		#region Binding

		protected override BusinessObject GetNewDataSource()
		{
			var result = ChangeMasterPasswordPerson.New(SiteUser?.LoggedInUser?.Person);
			return result;
		}

		protected ChangeMasterPasswordPerson PersonWrapper => DataSource as ChangeMasterPasswordPerson;

		protected override bool IsPersistDataSourceBetweenPostbacks => true;

		#endregion

		protected void Page_Load(object sender, EventArgs e)
		{
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (SiteUser == null || !SiteUser.IsLoggedIn)
			{
				HideSetPasswordContentAndShowMessage(Res.GetString("efffb72f-3bf0-4f21-8ee7-60655456cb6c", "Please log in before attempting to change your password."));
			}
			else if (!SiteUser.LoggedInUser.Person.HasPassword)
			{
				ContactsBox.Visible = false;
				ChangePasswordInstructionsLabel.Visible = false;
			}
			else
			{
				RelatedAccounts.Text = PersonWrapper.RelatedAccounts;
			}
		}

		protected override bool Cacheable => false;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		protected void Update_Click(object sender, EventArgs e)
		{
			if (SiteUser != null && SiteUser.IsLoggedIn)
			{
				if (SiteUser.IsSuperUser)
				{
#pragma warning disable EDI012
					PasswordChangeMessage.Text = Res.GetString("95854351-e4dd-414f-8fe3-38cc04ed6e19", "Cannot change password for CargoWise Support");
#pragma warning restore EDI012
				}
				else
				{
					var contact = SiteUser.LoggedInUser;
					var manager = new WebUserAdminManager(contact);
					PasswordChangeMessage.Text = contact.Person.HasPassword
						? manager.ChangeMasterPassword(CurrentPassword.Text, NewPassword.Text, NewPasswordConfirm.Text)
						: manager.ChangePassword(CurrentPassword.Text, NewPassword.Text, NewPasswordConfirm.Text);

					if (PasswordChangeMessage.Text == manager.PasswordChangeSuccess)
					{
						HideSetPasswordContent();
					}
				}
			}
			else
			{
				HideSetPasswordContentAndShowMessage(Res.GetString("db91b457-3945-44e8-ba30-ac1bfb830a16", "Your session has expired. Please login again before attempting to change your password."));
				Logout();
				Session.Abandon();
			}
		}

		void HideSetPasswordContentAndShowMessage(string message)
		{
			HideSetPasswordContent();

			PasswordChangeMessage.Text = message;
		}

		void HideSetPasswordContent()
		{
			ContactsBox.Visible = false;
			ChangePasswordInstructionsLabel.Visible = false;
			SetPasswordBox.Visible = false;
		}

		public void Logout()
		{
			var loginStatus = GetLoginStatus();
			loginStatus?.LogOff();
		}

		protected override ZString ModuleNameForEventLogging
		{
			get { return (NoResString)"Change Password"; } // Event logging related
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.ChangePassword;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.ChangePasswordPage;
		}
	}
}
