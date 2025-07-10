using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Security;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test;
public class ChangePasswordViewModelTest : TestCase
{
	public void TestUserName()
	{
		var uut = new ChangePasswordViewModel(GlbStaff.CurrentUser, true);

		AssertEquals(GlbStaff.CurrentUser.GS_LoginName, uut.UserName);
	}

	public void TestTitle_WhenRequireOldPassword()
	{
		var uut = new ChangePasswordViewModel(GlbStaff.CurrentUser, true);

		AssertEquals("Title", "Change Password", uut.Title);
	}

	public void TestTitle_WhenDoesNotRequireOldPassword()
	{
		var uut = new ChangePasswordViewModel(GlbStaff.CurrentUser, false);

		AssertEquals("Title", "Reset Password", uut.Title);
	}

	public void TestLabelsAndCaptions()
	{
		var uut = new ChangePasswordViewModel(GlbStaff.CurrentUser, true);
		CombineAssertions(() =>
		{
			AssertEquals("User Name Label", "Username", uut.UserNameLabel);
			AssertEquals("Old Password Label", "Old Password", uut.OldPasswordLabel);
			AssertEquals("New Password Label", "New Password", uut.NewPasswordLabel);
			AssertEquals("Confirm New Password Label", "Confirm New Password", uut.NewPasswordConfirmLabel);
			AssertEquals("Apply Caption", "Apply Changes", uut.ApplyCaption);
			AssertEquals("Cancel Caption", "Cancel", uut.CancelCaption);
		});
	}

	public void TestPropertyChangedEvent()
	{
		var events = new List<string>();
		var uut = new ChangePasswordViewModel(GlbStaff.CurrentUser, true);
		uut.PropertyChanged += (object sender, PropertyChangedEventArgs e) => events.Add(e.PropertyName);

		uut.OldPassword = new SecureString();
		uut.NewPassword = new SecureString();
		uut.NewPasswordConfirm = new SecureString();

		CombineAssertions(() =>
		{
			AssertEquals("Count of property changed events", 3, events.Count);
			AssertCollectionContains("Old Password Changed", events, (e) => e == "OldPassword");
			AssertCollectionContains("New Password Changed", events, (e) => e == "NewPassword");
			AssertCollectionContains("New Password Confirm Changed", events, (e) => e == "NewPasswordConfirm");
		});
	}

	public void TestCancel()
	{
		var uut = new ChangePasswordViewModel(GlbStaff.CurrentUser, true);

		AssertEquals("Cancel", true, uut.Cancel());
	}

	public void TestApply_WhenNoPasswordInformed()
	{
		var uut = new ChangePasswordViewModel(GlbStaff.CurrentUser, false);
		var events = new List<string>();
		uut.PropertyChanged += (object sender, PropertyChangedEventArgs e) => events.Add(e.PropertyName);

		var actual = uut.Apply();

		CombineAssertions(() =>
		{
			AssertEquals("Apply", false, actual);
			AssertEquals("Error Message", "Please enter New Password.", uut.ErrorMessage.Trim());

			AssertEquals("Count of property changed events", 2, events.Count);
			AssertCollectionContains("Error Message Changed", events, (e) => e == "ErrorMessage");
		});
	}

	public void TestApply_WhenOldPasswordRequired_WhenOldPasswordNotInformed()
	{
		var uut = new ChangePasswordViewModel(GlbStaff.CurrentUser, true);
		var events = new List<string>();
		uut.PropertyChanged += (object sender, PropertyChangedEventArgs e) => events.Add(e.PropertyName);

		uut.NewPassword = new NetworkCredential("", "#password123").SecurePassword;
		uut.NewPasswordConfirm = new NetworkCredential("", "#password123").SecurePassword;

		var actual = uut.Apply();

		CombineAssertions(() =>
		{
			AssertEquals("Apply", false, actual);
			AssertEquals("Error Message 1", "Old Password is incorrect. Passwords are case-sensitive.", uut.ErrorMessage);

			AssertEquals("Count of property changed events", 4, events.Count);
			AssertCollectionContains("Old Password Changed", events, (e) => e == "ErrorMessage");
		});
	}

	public void TestApply_WhenNewPasswordDoesntMatch()
	{
		var uut = new ChangePasswordViewModel(GlbStaff.CurrentUser, true);
		var events = new List<string>();
		uut.PropertyChanged += (object sender, PropertyChangedEventArgs e) => events.Add(e.PropertyName);

		uut.OldPassword = new NetworkCredential("", User.MasterPassword).SecurePassword;
		uut.NewPassword = new NetworkCredential("", "pwd123@").SecurePassword;
		uut.NewPasswordConfirm = new NetworkCredential("", "#password12?").SecurePassword;

		var actual = uut.Apply();

		CombineAssertions(() =>
		{
			AssertEquals("Apply", false, actual);
			AssertEquals("Error Message 1", "Confirm Password does not match New Password.", uut.ErrorMessage);

			AssertEquals("Count of property changed events", 5, events.Count);
			AssertCollectionContains("Password Changed", events, (e) => e == "ErrorMessage");
		});
	}

	public void TestApply_WhenOldPasswordNotRequired_WhenNewPasswordIsValid()
	{
		var uut = new ChangePasswordViewModel(GlbStaff.CurrentUser, false);
		var events = new List<string>();
		uut.PropertyChanged += (object sender, PropertyChangedEventArgs e) => events.Add(e.PropertyName);

		uut.NewPassword = new NetworkCredential("", "#password123").SecurePassword;
		uut.NewPasswordConfirm = new NetworkCredential("", "#password123").SecurePassword;

		var actual = uut.Apply();

		CombineAssertions(() =>
		{
			AssertEquals("Apply", true, actual);
			AssertEquals("Error Message", string.Empty, uut.ErrorMessage);

			AssertEquals("Count of property changed events", 4, events.Count);
			AssertCollectionContains("Password Changed", events, (e) => e == "ErrorMessage");
		});
	}
}
