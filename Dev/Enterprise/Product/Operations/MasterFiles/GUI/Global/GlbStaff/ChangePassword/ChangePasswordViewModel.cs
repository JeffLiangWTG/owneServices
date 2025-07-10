using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI;

public class ChangePasswordViewModel : IChangePasswordViewModel
{
	readonly GlbStaff staff;
	readonly bool requireOldPassword;

	SecureString oldPassword;
	SecureString newPassword;
	SecureString newPasswordConfirm;
	string errorMessage = string.Empty;

	public event PropertyChangedEventHandler PropertyChanged;

	public ChangePasswordViewModel(GlbStaff staff, bool requireOldPassword)
	{
		this.staff = staff;
		this.requireOldPassword = requireOldPassword;
	}

	public MultilingualString Title => RequireOldPassword
		? ResString.GetMultilingualString("6eb170ba-1378-4b32-9245-c42f9850f182", "Change Password")
		: ResString.GetMultilingualString("40E8279B-A625-43BA-9A85-3E54AFA92047", "Reset Password");

	public MultilingualString UserNameLabel => ResString.GetMultilingualString("389f4309-0965-4ecd-9635-3e61d834aff2", "Username");
	public string UserName => staff.GS_LoginName;

	public bool RequireOldPassword => requireOldPassword;
	public MultilingualString OldPasswordLabel => ResString.GetMultilingualString("d8faf152-5443-4bb6-abf3-bbd0eaf341ac", "Old Password");
	public SecureString OldPassword
	{
		get => oldPassword;
		set
		{
			oldPassword = value;
			NotifyPropertyChanged();
		}
	}

	public MultilingualString NewPasswordLabel => ResString.GetMultilingualString("06a5a099-a773-41ff-8d8b-4a4c7e86e76f", "New Password");
	public SecureString NewPassword
	{
		get => newPassword;
		set
		{
			newPassword = value;
			NotifyPropertyChanged();
		}
	}

	public MultilingualString NewPasswordConfirmLabel => ResString.GetMultilingualString("eb8b165a-db5d-4194-ab8a-d69e866c71d3", "Confirm New Password");
	public SecureString NewPasswordConfirm
	{
		get => newPasswordConfirm;
		set
		{
			newPasswordConfirm = value;
			NotifyPropertyChanged();
		}
	}

	public MultilingualString CancelCaption => ResString.GetMultilingualString("6ef351af-f51e-4e15-90dd-0686cd301d16", "Cancel");
	public MultilingualString ApplyCaption => ResString.GetMultilingualString("14c3d114-3900-43ad-8a16-23284f6b812a", "Apply Changes");

	public string ErrorMessage
	{
		get => errorMessage;
		private set
		{
			errorMessage = value;
			NotifyPropertyChanged();
		}
	}

	public bool Apply()
	{
		ErrorMessage = string.Empty;
		string error;
		var passwordControl = new PasswordControl();
		try
		{
			var oldPassword = RequireOldPassword ? OldPassword.ToInsecureString() : null;
			var newPassword = NewPassword.ToInsecureString();
			var newPasswordConfirm = NewPasswordConfirm.ToInsecureString();

			if (passwordControl.NewPasswordIsValid(staff, oldPassword, newPassword, newPasswordConfirm, out error))
			{
				if (RequireOldPassword)
				{
					staff.ChangePassword(oldPassword, newPassword);
				}
				else
				{
					staff.ResetPassword(newPassword);
				}
			}
		}
		catch (UserLockedOutException)
		{
			error = Res.GetString("f1bd00e0-9a79-4198-a49d-407a70cbb7bb", "Your login is currently locked out due to a previous failed login. Please try again later or see your system administrator if your account remains locked out after an extended period of time.");
		}
		catch (PasswordDoesNotMatchPolicyException)
		{
			error = Res.GetString("b85429eb-5c9a-42b8-b6a0-170ef6677cd2", "Unable to update the password. Either the old password is wrong or the value provided for the new password does not meet the length, complexity, or history requirements of the domain.");
		}
		catch (NoDomainPrivilegeException)
		{
			error = Res.GetString(
				"0ea4044b-8612-4983-b5f2-0198b1bd5e75",
				"Unable to reset the password. Domain user does not have write privileges to the Organizational Units set in the registry '{0}'",
				ObjectFactory.Get<IADRegistry>().DomainCredentialsCollectionRegistryLocation);
		}
		catch (InvalidOperationException ex)
		{
			error = ex.Message;
		}

		ErrorMessage = error ?? string.Empty;

		return string.IsNullOrEmpty(ErrorMessage);
	}

	public bool Cancel() => true;

	void NotifyPropertyChanged([CallerMemberName] string property = null)
	{
		if (!string.IsNullOrEmpty(property))
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}
}
