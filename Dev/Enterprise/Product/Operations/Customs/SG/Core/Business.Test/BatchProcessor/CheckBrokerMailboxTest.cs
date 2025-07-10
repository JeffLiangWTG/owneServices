using System;
using System.Linq;
using CargoWise.BrandManager;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.MHUB;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor.Testing
{
	sealed class CheckBrokerMailboxTest : TestCaseWithFactory
	{
		[TestDate(2017, 07, 20)]
		public void TestEDIServlet()
		{
			SGCustomsDataRegistry.Instance.SendTestMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var mock = new Mock<CheckBrokerMailbox>(new object[] { Logger, new BatchSG4InterchangeHelper(Logger) });
			CheckBrokerMailbox mailboxChecker = mock.Object;
			AssertEquals("The command string for ediservlet is correct", "https://www.tradexchange.gov.sg/txmhbweb/mhb/EDIServlet", mailboxChecker.EDIServlet);
			SGCustomsDataRegistry.Instance.SendTestMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("The command string for ediservlet is correct", "https://trial.tradexchange.gov.sg/txmhbweb/mhb/EDIServlet", mailboxChecker.EDIServlet);
		}

		public void TestChangePasswordAutomatically()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			//first login
			var loginCommand = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand.LoginState = LoginCommand.LoginStateType.NotLoggedIn;
			loginCommand.BrokerAccountError = new BrokerAccountError("Password has expired", SGDeactivationCodes.Codes.PEX);
			//change password login
			var loginCommand2 = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand2.LoginState = LoginCommand.LoginStateType.LoggedIn;

			var mock = new Mock<CheckBrokerMailbox>(Logger, new BatchSG4InterchangeHelper(Logger));
			mock.CallBase = true;
			mock.Protected().SetupSequence<LoginCommand>("Login", ItExpr.IsAny<LoginDetails>()).Returns(loginCommand).Returns(loginCommand2);

			CheckBrokerMailbox mailboxChecker = mock.Object;
			mailboxChecker.Execute(Broker);
			AssertEquals("Broker password status", Core.Constants.PasswordOK, Wrapper.Tradenetv4Password.GP_PasswordStatus);
			AssertEquals("Broker next password", "", Wrapper.Tradenetv4Password.NextDecryptedPassword);
			AssertEquals("Broker current password length must now be between 12 & 15 characters", 14, Wrapper.Tradenetv4Password.CurrentDecryptedPassword.Length);
			AssertEquals("email created", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("subject", "Customs Broker: v13t000 TradeNet Password Changed", email.Subject);
			AssertContains("body", "<strong>Staff Member Name : TEST USER<br />", email.Body);
			AssertContains("body", "TradeNet User ID : v13t000<br />", email.Body);
			AssertContains("body", "Your TradeNet Password has expired. " + BrandingFactory.Instance.ProductName + " has automatically renewed your password. The new password is included in this email.", email.Body);
			AssertContains("body", "<strong>New Password : ", email.Body);
			AssertEquals("recipient", 1, email.Recipients.Count);
			AssertEquals("recipient", "test1@hotmail.com", email.Recipients[0].Email);
			AssertEquals("attachments", 2, email.Attachments.Count);
			mock.Protected().Verify("Login", Times.Exactly(2), ItExpr.IsAny<LoginDetails>());
			mock.VerifyAll();
		}

		public void TestLoggedInFailedButNotChangePassword()
		{
			var loginCommand = new LoginCommand(new LoginDetails("SG", "TEST", "123456"), new MHUBSettingsProvider(), Logger, false);
			loginCommand.LoginState = LoginCommand.LoginStateType.NotLoggedIn;
			loginCommand.BrokerAccountError = null;

			var mock = new Mock<CheckBrokerMailbox>(new object[] { Logger, new BatchSG4InterchangeHelper(Logger) });
			mock.CallBase = true;
			mock.Protected().Setup<LoginCommand>("Login", ItExpr.IsAny<LoginDetails>()).Returns(loginCommand);

			var mailboxChecker = mock.Object;
			mailboxChecker.Execute(Broker);

			Assert("Should log when the status is not login in but the password is not updated.", Logger.Logs.Any(c => c.Message == "v13t000 login failed but system don't update the password as the deactivationCode is ''."));
			mock.Protected().Verify("Login", Times.Exactly(1), ItExpr.IsAny<LoginDetails>());
			mock.VerifyAll();
		}

		public void TestRandomPasswordChanges()
		{
			//first login
			var loginCommand = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand.LoginState = LoginCommand.LoginStateType.NotLoggedIn;
			loginCommand.BrokerAccountError = new BrokerAccountError("Password has expired", SGDeactivationCodes.Codes.PEX);
			//change password login
			var loginCommand2 = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand2.LoginState = LoginCommand.LoginStateType.LoggedIn;

			var mock1 = new Mock<CheckBrokerMailbox>(new object[] { Logger, new BatchSG4InterchangeHelper(Logger) });
			mock1.CallBase = true;
			mock1.Protected().SetupSequence<LoginCommand>("Login", ItExpr.IsAny<LoginDetails>()).Returns(loginCommand).Returns(loginCommand2);

			CheckBrokerMailbox mailboxChecker = mock1.Object;
			mailboxChecker.Execute(Broker);
			System.Threading.Thread.Sleep(1000);
			var randomPassword1 = Wrapper.Tradenetv4Password.CurrentDecryptedPassword;
			loginCommand = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand.LoginState = LoginCommand.LoginStateType.NotLoggedIn;
			loginCommand.BrokerAccountError = new BrokerAccountError("Password has expired", SGDeactivationCodes.Codes.PEX);
			loginCommand2 = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand2.LoginState = LoginCommand.LoginStateType.LoggedIn;

			var mock2 = new Mock<CheckBrokerMailbox>(new object[] { Logger, new BatchSG4InterchangeHelper(Logger) });
			mock2.CallBase = true;
			mock2.Protected().SetupSequence<LoginCommand>("Login", ItExpr.IsAny<LoginDetails>()).Returns(loginCommand).Returns(loginCommand2);

			mailboxChecker = mock2.Object;
			mailboxChecker.Execute(Broker);
			System.Threading.Thread.Sleep(1000);
			var randomPassword2 = Wrapper.Tradenetv4Password.CurrentDecryptedPassword;
			loginCommand = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand.LoginState = LoginCommand.LoginStateType.NotLoggedIn;
			loginCommand.BrokerAccountError = new BrokerAccountError("Password has expired", SGDeactivationCodes.Codes.PEX);
			loginCommand2 = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand2.LoginState = LoginCommand.LoginStateType.LoggedIn;

			var mock3 = new Mock<CheckBrokerMailbox>(new object[] { Logger, new BatchSG4InterchangeHelper(Logger) });
			mock3.CallBase = true;
			mock3.Protected().SetupSequence<LoginCommand>("Login", ItExpr.IsAny<LoginDetails>()).Returns(loginCommand).Returns(loginCommand2);

			mailboxChecker = mock3.Object;
			mailboxChecker.Execute(Broker);
			var randomPassword3 = Wrapper.Tradenetv4Password.CurrentDecryptedPassword;
			AssertNotEquals("Random password changes", randomPassword1, randomPassword2);
			AssertNotEquals("Random password changes", randomPassword1, randomPassword3);
			AssertNotEquals("Random password changes", randomPassword3, randomPassword2);
			mock1.Protected().Verify("Login", Times.Exactly(2), ItExpr.IsAny<LoginDetails>());
			mock2.Protected().Verify("Login", Times.Exactly(2), ItExpr.IsAny<LoginDetails>());
			mock3.Protected().Verify("Login", Times.Exactly(2), ItExpr.IsAny<LoginDetails>());
			mock1.VerifyAll();
			mock2.VerifyAll();
			mock3.VerifyAll();
		}

		public void TestExecuteChangePassword()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var loginCommand = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand.LoginState = LoginCommand.LoginStateType.LoggedIn;

			var mock = new Mock<CheckBrokerMailbox>(new object[] { Logger, new BatchSG4InterchangeHelper(Logger) });
			mock.CallBase = true;
			mock.Protected().Setup<LoginCommand>("Login", ItExpr.IsAny<LoginDetails>()).Returns(loginCommand);

			CheckBrokerMailbox mailboxChecker = mock.Object;
			mailboxChecker.Execute(Broker);
			AssertEquals("Broker password status", Core.Constants.PasswordOK, Wrapper.Tradenetv4Password.GP_PasswordStatus);
			AssertEquals("Broker next password", "", Wrapper.Tradenetv4Password.NextDecryptedPassword);
			AssertEquals("Broker current password", "Next", Wrapper.Tradenetv4Password.CurrentDecryptedPassword);
			AssertEquals("email created", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("subject", "Customs Broker: v13t000 TradeNet Password Changed", email.Subject);
			AssertContains("body", "<strong>Staff Member Name : TEST USER<br />", email.Body);
			AssertContains("body", "TradeNet User ID : v13t000<br />", email.Body);
			AssertContains("body", $"Your TradeNet Password has expired. {BrandingFactory.Instance.ProductName} has automatically renewed your password. The new password is included in this email.", email.Body);
			AssertContains("body", "<strong>New Password : Next</strong>", email.Body);
			AssertEquals("recipient", 1, email.Recipients.Count);
			AssertEquals("recipient", "test1@hotmail.com", email.Recipients[0].Email);
			AssertEquals("attachments", 2, email.Attachments.Count);
			mock.Protected().Verify("Login", Times.Exactly(1), ItExpr.IsAny<LoginDetails>());
			mock.VerifyAll();
		}

		public void TestExecuteForExpiredPassword()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var loginCommand = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand.BrokerAccountError = new BrokerAccountError("Password has expired", SGDeactivationCodes.Codes.PEX);

			var mock = new Mock<CheckBrokerMailbox>(new object[] { Logger, new BatchSG4InterchangeHelper(Logger) });
			mock.Protected().Setup<LoginCommand>("Login", ItExpr.IsAny<LoginDetails>()).Returns(loginCommand);
			mock.Protected().Setup<LoginCommand>("ChangePassword", ItExpr.IsAny<LoginCommand>(), ItExpr.IsAny<GlbStaff>()).Returns(loginCommand);

			CheckBrokerMailbox mailboxChecker = mock.Object;
			mailboxChecker.Execute(Broker);
			AssertEquals("Broker password status", SGDeactivationCodes.Codes.PEX, Wrapper.Tradenetv4Password.GP_PasswordStatus);
			AssertDeactivationEmailSent("v13t000 Password has expired", ExpiredPasswordBody);
			mock.VerifyAll();
		}

		public void TestExecuteForChangePassword()
		{
			ExecuteCheckBrokerMailbox("User needs to change password", SGDeactivationCodes.Codes.PCH);
			AssertEquals("Broker password status", SGDeactivationCodes.Codes.PCH, Wrapper.Tradenetv4Password.GP_PasswordStatus);
			AssertDeactivationEmailSent("v13t000 User needs to change password", ChangePasswordBody);
		}

		public void TestExecuteForFrozen()
		{
			ExecuteCheckBrokerMailbox("Account Frozen", SGDeactivationCodes.Codes.FRZ);
			AssertEquals("Broker password status", SGDeactivationCodes.Codes.FRZ, Wrapper.Tradenetv4Password.GP_PasswordStatus);
			AssertDeactivationEmailSent("v13t000 Account Frozen", FrozenBody, 3);
		}

		public void TestExecuteForLoginNonExistent()
		{
			ExecuteCheckBrokerMailbox("Login ID doesn't exist for MHUB user", SGDeactivationCodes.Codes.ANE);
			AssertEquals("Broker password status", SGDeactivationCodes.Codes.ANE, Wrapper.Tradenetv4Password.GP_PasswordStatus);
			AssertDeactivationEmailSent("v13t000 Login ID doesn't exist for MHUB user", LoginNonExistentBody);
		}

		public void TestExecuteForInvalidUserIDOrPassword()
		{
			ExecuteCheckBrokerMailbox("Invalid User ID / Password", SGDeactivationCodes.Codes.IID);
			AssertEquals("Broker password status", SGDeactivationCodes.Codes.IID, Wrapper.Tradenetv4Password.GP_PasswordStatus);
			AssertDeactivationEmailSent("v13t000 Invalid User ID / Password", InvalidLoginBody);
		}

		public void TestExecuteForIncorrectPassword()
		{
			ExecuteCheckBrokerMailbox("Password is incorrect", SGDeactivationCodes.Codes.PIC);
			AssertEquals("Broker password status", SGDeactivationCodes.Codes.PIC, Wrapper.Tradenetv4Password.GP_PasswordStatus);
			AssertDeactivationEmailSent("v13t000 Password is incorrect", IncorrectPasswordBody);
		}

		public void TestExecuteForInvalidNewPassword()
		{
			ExecuteCheckBrokerMailbox("New password is invalid", SGDeactivationCodes.Codes.PCS);
			AssertEquals("Broker password status", SGDeactivationCodes.Codes.PCS, Wrapper.Tradenetv4Password.GP_PasswordStatus);
			AssertDeactivationEmailSent("v13t000 New password is invalid", InvalidNewPasswordBody);
		}

		void AssertDeactivationEmailSent(string subject, string body)
		{
			AssertDeactivationEmailSent(subject, body, 2);
		}

		void AssertDeactivationEmailSent(string subject, string body, int attachmentCount)
		{
			AssertEquals("email created", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("subject", subject, email.Subject);
			AssertContains("body", body, email.Body);
			AssertEquals("recipient", 1, email.Recipients.Count);
			AssertEquals("recipient", "test1@hotmail.com", email.Recipients[0].Email);
			AssertEquals("attachments", attachmentCount, email.Attachments.Count);
		}

		#region Password Policy Enhancement in TradeNet
		public void TestChangeOldPasswordFormatOnFirstUse()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			Wrapper.Tradenetv4Password.NextDecryptedPassword = "";
			Factory.Save();
			//first login
			var loginCommand = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand.LoginState = LoginCommand.LoginStateType.LoggedIn;

			var mock = new Mock<CheckBrokerMailbox>(new object[] { Logger, new BatchSG4InterchangeHelper(Logger) });
			mock.CallBase = true;
			mock.Protected().Setup<LoginCommand>("Login", ItExpr.IsAny<LoginDetails>()).Returns(loginCommand);

			CheckBrokerMailbox mailboxChecker = mock.Object;
			mailboxChecker.Execute(Broker);
			AssertEquals("Broker password status", Core.Constants.PasswordOK, Wrapper.Tradenetv4Password.GP_PasswordStatus);
			AssertEquals("Broker next password", "", Wrapper.Tradenetv4Password.NextDecryptedPassword);
			AssertEquals("Broker TradeNet password has been updated to the new format", 14, Wrapper.Tradenetv4Password.CurrentDecryptedPassword.Length);
			AssertEquals("Change TradeNet password email has been created & sent to broker", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("subject", "Customs Broker: v13t000 TradeNet Password Changed", email.Subject);
			AssertContains("body", "<strong>Staff Member Name : TEST USER<br />", email.Body);
			AssertContains("body", "TradeNet User ID : v13t000<br />", email.Body);
			AssertContains("body", "Your TradeNet Password has expired. " + BrandingFactory.Instance.ProductName + " has automatically renewed your password. The new password is included in this email.", email.Body);
			AssertContains("body", "<strong>New Password : " + Wrapper.Tradenetv4Password.CurrentDecryptedPassword, email.Body);
			AssertEquals("recipient", 1, email.Recipients.Count);
			AssertEquals("recipient", "test1@hotmail.com", email.Recipients[0].Email);
			AssertEquals("attachments", 2, email.Attachments.Count);
			AssertEquals("Broker password status", Core.Constants.PasswordOK, Wrapper.Tradenetv4Password.GP_PasswordStatus);
			mock.Protected().Verify("Login", Times.Exactly(1), ItExpr.IsAny<LoginDetails>());
			mock.VerifyAll();
		}

		public void TestChangeEnhancedPasswordAutomatically()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			//first login
			var loginCommand = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand.LoginState = LoginCommand.LoginStateType.NotLoggedIn;
			loginCommand.BrokerAccountError = new BrokerAccountError("Password has expired", SGDeactivationCodes.Codes.PEX);
			//change password login
			var loginCommand2 = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand2.LoginState = LoginCommand.LoginStateType.LoggedIn;

			var mock = new Mock<CheckBrokerMailbox>(new object[] { Logger, new BatchSG4InterchangeHelper(Logger) });
			mock.CallBase = true;
			mock.Protected().SetupSequence<LoginCommand>("Login", ItExpr.IsAny<LoginDetails>()).Returns(loginCommand).Returns(loginCommand2);

			CheckBrokerMailbox mailboxChecker = mock.Object;
			mailboxChecker.Execute(Broker);
			AssertEquals("Broker password status", Core.Constants.PasswordOK, Wrapper.Tradenetv4Password.GP_PasswordStatus);
			AssertEquals("Broker next password", "", Wrapper.Tradenetv4Password.NextDecryptedPassword);
			AssertEquals("Broker password length must be between 12 and 15 characters", true, Wrapper.Tradenetv4Password.CurrentDecryptedPassword.Length > 11);
			AssertEquals("Broker password length must be between 12 and 15 characters", true, Wrapper.Tradenetv4Password.CurrentDecryptedPassword.Length < 16);
			AssertEquals("email created", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("subject", "Customs Broker: v13t000 TradeNet Password Changed", email.Subject);
			AssertContains("body", "<strong>Staff Member Name : TEST USER<br />", email.Body);
			AssertContains("body", "TradeNet User ID : v13t000<br />", email.Body);
			AssertContains("body", $"Your TradeNet Password has expired. {BrandingFactory.Instance.ProductName} has automatically renewed your password. The new password is included in this email.", email.Body);
			AssertContains("body", "<strong>New Password : ", email.Body);
			AssertEquals("recipient", 1, email.Recipients.Count);
			AssertEquals("recipient", "test1@hotmail.com", email.Recipients[0].Email);
			AssertEquals("attachments", 2, email.Attachments.Count);
			mock.Protected().Verify("Login", Times.Exactly(2), ItExpr.IsAny<LoginDetails>());
			mock.VerifyAll();
		}

		public void TestEnhancedPasswordContainsMandatoryCharacters()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			//first login
			var loginCommand = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand.LoginState = LoginCommand.LoginStateType.NotLoggedIn;
			loginCommand.BrokerAccountError = new BrokerAccountError("Password has expired", SGDeactivationCodes.Codes.PEX);
			//change password login
			var loginCommand2 = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand2.LoginState = LoginCommand.LoginStateType.LoggedIn;

			var mock = new Mock<CheckBrokerMailbox>(new object[] { Logger, new BatchSG4InterchangeHelper(Logger) });
			mock.CallBase = true;
			mock.Protected().SetupSequence<LoginCommand>("Login", ItExpr.IsAny<LoginDetails>()).Returns(loginCommand).Returns(loginCommand2);

			CheckBrokerMailbox mailboxChecker = mock.Object;
			mailboxChecker.Execute(Broker);
			var generatedPassword = Wrapper.Tradenetv4Password.CurrentDecryptedPassword;
			AssertEquals("Generated password (" + generatedPassword + ") must contain at least 1 numeric character", true, generatedPassword.KeepNumericCharacters().Length > 0);
			AssertEquals("Generated password (" + generatedPassword + ")  must contain at least 1 uppercase character", true, generatedPassword.ContainsAnyChar("ABCDEFGHIJKLMNOPQRSTUVWXYZ"));
			AssertEquals("Generated password (" + generatedPassword + ")  must contain at least 1 lowercase character", true, generatedPassword.ContainsAnyChar("abcdefghijklmnopqrstuvwxyz"));
			mock.Protected().Verify("Login", Times.Exactly(2), ItExpr.IsAny<LoginDetails>());
			mock.VerifyAll();
		}

		public void TestRandomEnhancedPasswordChanges()
		{
			//first login
			var loginCommand = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand.LoginState = LoginCommand.LoginStateType.NotLoggedIn;
			loginCommand.BrokerAccountError = new BrokerAccountError("Password has expired", SGDeactivationCodes.Codes.PEX);
			//change password login
			var loginCommand2 = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand2.LoginState = LoginCommand.LoginStateType.LoggedIn;

			var mock1 = new Mock<CheckBrokerMailbox>(new object[] { Logger, new BatchSG4InterchangeHelper(Logger) });
			mock1.CallBase = true;
			mock1.Protected().SetupSequence<LoginCommand>("Login", ItExpr.IsAny<LoginDetails>()).Returns(loginCommand).Returns(loginCommand2);

			CheckBrokerMailbox mailboxChecker = mock1.Object;
			mailboxChecker.Execute(Broker);
			System.Threading.Thread.Sleep(1000);
			var randomPassword1 = Wrapper.Tradenetv4Password.CurrentDecryptedPassword;
			loginCommand = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand.LoginState = LoginCommand.LoginStateType.NotLoggedIn;
			loginCommand.BrokerAccountError = new BrokerAccountError("Password has expired", SGDeactivationCodes.Codes.PEX);
			loginCommand2 = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand2.LoginState = LoginCommand.LoginStateType.LoggedIn;

			var mock2 = new Mock<CheckBrokerMailbox>(new object[] { Logger, new BatchSG4InterchangeHelper(Logger) });
			mock2.CallBase = true;
			mock2.Protected().SetupSequence<LoginCommand>("Login", ItExpr.IsAny<LoginDetails>()).Returns(loginCommand).Returns(loginCommand2);

			mailboxChecker = mock2.Object;
			mailboxChecker.Execute(Broker);
			System.Threading.Thread.Sleep(1000);
			var randomPassword2 = Wrapper.Tradenetv4Password.CurrentDecryptedPassword;
			loginCommand = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand.LoginState = LoginCommand.LoginStateType.NotLoggedIn;
			loginCommand.BrokerAccountError = new BrokerAccountError("Password has expired", SGDeactivationCodes.Codes.PEX);
			loginCommand2 = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand2.LoginState = LoginCommand.LoginStateType.LoggedIn;

			var mock3 = new Mock<CheckBrokerMailbox>(new object[] { Logger, new BatchSG4InterchangeHelper(Logger) });
			mock3.CallBase = true;
			mock3.Protected().SetupSequence<LoginCommand>("Login", ItExpr.IsAny<LoginDetails>()).Returns(loginCommand).Returns(loginCommand2);

			mailboxChecker = mock3.Object;
			mailboxChecker.Execute(Broker);
			var randomPassword3 = Wrapper.Tradenetv4Password.CurrentDecryptedPassword;
			AssertNotEquals("Random password changes", randomPassword1, randomPassword2);
			AssertNotEquals("Random password changes", randomPassword1, randomPassword3);
			AssertNotEquals("Random password changes", randomPassword3, randomPassword2);
			mock1.Protected().Verify("Login", Times.Exactly(2), ItExpr.IsAny<LoginDetails>());
			mock2.Protected().Verify("Login", Times.Exactly(2), ItExpr.IsAny<LoginDetails>());
			mock3.Protected().Verify("Login", Times.Exactly(2), ItExpr.IsAny<LoginDetails>());
			mock1.VerifyAll();
			mock2.VerifyAll();
			mock3.VerifyAll();
		}

		#endregion
		void ExecuteCheckBrokerMailbox(string brokerAccountErrorMessage, ZString deactivationCode)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var loginCommand = new LoginCommand(new LoginDetails("", "", ""), new MHUBSettingsProvider(), Logger, false);
			loginCommand.BrokerAccountError = new BrokerAccountError(brokerAccountErrorMessage, deactivationCode);
			CheckBrokerMailbox mailboxChecker = GetMockCheckBrokerMailbox(loginCommand);
			mailboxChecker.Execute(Broker);
		}

		CheckBrokerMailbox GetMockCheckBrokerMailbox(LoginCommand loginCommand)
		{
			var mock = new Mock<CheckBrokerMailbox>(new object[] { Logger, new BatchSG4InterchangeHelper(Logger) });
			mock.Protected().Setup<LoginCommand>("Login", ItExpr.IsAny<LoginDetails>()).Returns(loginCommand);
			mock.Protected().Setup<LoginCommand>("ChangePassword", ItExpr.IsAny<LoginCommand>(), ItExpr.IsAny<GlbStaff>()).Returns(loginCommand);
			CheckBrokerMailbox result = mock.Object;
			return result;
		}

		#region email Constants
		const string ExpiredPasswordBody = @"<br />
<strong>TradeNet User ID : v13t000<br />
TradeNet User ID Status : Password has expired<br />
Staff Member Name : TEST USER<br />
<br />
</strong>An error has been found when logging into MHUB for the above broker<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
</p>
<br />
<br />
Your account has temporarily been deactivated, because your password has expired.<br /><br />
Your Singapore Customs mailbox has been deactivated to stop it from becoming frozen. 
To use your account again, you need to update your broker password.<br /><br />
Choose a new password and edit your user details within CargoWise One.
On the staff form, select the brokerage tab.
Enter your new password and save your changes.<br /><br />
The Batch Processor on its next submission will login and update the system with your new password and will re-activate your mailbox.
<br />";
		const string ChangePasswordBody = @"<br />
<strong>TradeNet User ID : v13t000<br />
TradeNet User ID Status : User needs to change password<br />
Staff Member Name : TEST USER<br />
<br />
</strong>An error has been found when logging into MHUB for the above broker<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
</p>
<br />
<br />
Your account has temporarily been deactivated, because your password needs to be changed.<br /><br />
Your Singapore Customs mailbox has been deactivated to stop it from becoming frozen.
To use your account again, you need to update your broker password.<br /><br />
Choose a new password and edit your user details within CargoWise One.
On the staff form, select the brokerage tab.
Enter your new password and save your changes.<br /><br />
The Batch Processor on its next submission will login and update the system with your new password and will re-activate your mailbox.
<br />";
		const string InvalidNewPasswordBody = @"<br />
<strong>TradeNet User ID : v13t000<br />
TradeNet User ID Status : New password is invalid<br />
Staff Member Name : TEST USER<br />
<br />
</strong>An error has been found when logging into MHUB for the above broker<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
</p>
<br />
<br />
Your account has been deactivated, because an invalid password has been used.<br /><br />
Your new password must not be the same as a previously used password. Password was not changed.
Please enter a valid new password.
The Batch Processor on its next submission will login using the newly entered details.
<br />";
		const string IncorrectPasswordBody = @"<br />
<strong>TradeNet User ID : v13t000<br />
TradeNet User ID Status : Password is incorrect<br />
Staff Member Name : TEST USER<br />
<br />
</strong>An error has been found when logging into MHUB for the above broker<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
</p>
<br />
<br />
Your account has been deactivated, because the entered password is incorrect<br /><br />
Please enter a valid password.
The Batch Processor on its next submission will login using the newly entered details.
<br />";
		const string InvalidLoginBody = @"<br />
<strong>TradeNet User ID : v13t000<br />
TradeNet User ID Status : Invalid User ID / Password<br />
Staff Member Name : TEST USER<br />
<br />
</strong>An error has been found when logging into MHUB for the above broker<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
</p>
<br />
<br />
Your account has been deactivated, because the entered edi user mailbox/password is invalid<br /><br />
Please enter a valid edi user mailbox/password.
The Batch Processor on its next submission will login using the newly entered details.
<br />";
		const string LoginNonExistentBody = @"<br />
<strong>TradeNet User ID : v13t000<br />
TradeNet User ID Status : Login ID doesn't exist for MHUB user<br />
Staff Member Name : TEST USER<br />
<br />
</strong>An error has been found when logging into MHUB for the above broker<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
</p>
<br />
<br />
Your account has been deactivated, because the entered account does not exist at Singapore Customs.<br /><br />
Please enter a valid edi user mailbox/password.
The Batch Processor on its next submission will login using the newly entered details.
<br />";
		const string FrozenBody = @"<br />
<strong>TradeNet User ID : v13t000<br />
TradeNet User ID Status : Account Frozen<br />
Staff Member Name : TEST USER<br />
<br />
</strong>An error has been found when logging into MHUB for the above broker<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
</p>
<br />
<br />
Your account has been deactivated, because your TradeNet id has been frozen.<br /><br />
Your Singapore Customs mailbox has been frozen due to the fact that an expired password was used three times to login.
To use your account again, you need to complete the “Request Form for Password Reset” and fax it to Crimson Logic.
The “Request Form for Password Reset” is attached to this email.<br /><br />
When Crimson Logic receives your Password Reset Request, they will reactivate your mailbox, and send you a temporary password.<br /><br />
Update your staff account in CargoWise One with the temporary password and set a new password.
On the staff form, select the brokerage tab and then choose Establish New Password.
Enter the temporary password provided by Crimson Logic and your new permanent password.<br /><br />
The Batch Processor on its next submission will login using the temporary password and update the TradeNet system with your new password and re-activate your mailbox.
<br />";
		#endregion
		#region implementation
		#region Broker
		SGGlbStaffWrapper Wrapper => wrapper ?? (wrapper = SGGlbStaffWrapper.Get(Broker));
		SGGlbStaffWrapper wrapper;
		GlbStaff Broker
		{
			get
			{
				if (broker == null)
				{
					broker = Factory.New<GlbStaff>();
					broker.GS_IsActive = true;
					broker.GS_FullName = "TEST USER";
					var wrapper = SGGlbStaffWrapper.Get(broker);
					wrapper.Tradenetv4Password.GP_MailBoxID = "v13t000";
					wrapper.Tradenetv4Password.GP_UserID = "v13t000";
					wrapper.Tradenetv4Password.CurrentDecryptedPassword = "Current";
					wrapper.Tradenetv4Password.NextDecryptedPassword = "Next";
					broker.GS_EmailAddress = "test1@hotmail.com";
					Factory.Save();
				}

				return broker;
			}
		}

		GlbStaff broker;
		#endregion
		#region Logger
		LoggingInformation Logger
		{
			get
			{
				return logger ?? (logger = new LoggingInformation());
			}
		}

		LoggingInformation logger;
		#endregion
		#endregion
	}
}
