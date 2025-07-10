using System;
using System.Globalization;
using System.IO;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.MHUB;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor
{
	public class CheckBrokerMailbox
	{
		public CheckBrokerMailbox(LoggingInformation logger, BatchSGInterchangeHelper helper)
		{
			this.logger = logger;
			this.helper = helper;
		}

		public LoginCommand Execute(GlbStaff broker)
		{
			var password = helper.GetGlbExternalPassword(broker);
			logger.Log(string.Format(CultureInfo.InvariantCulture, "Checking {0} Credentials for Broker: {1}", helper.ApplicationDescription, password.GP_UserID));
			UpdatePasswordFormat(password);

			var loginDetails = new LoginDetails(EDIServlet, password.GP_UserID, password.CurrentDecryptedPassword, password.NextDecryptedPassword);
			LoginCommand loginCommand = Login(loginDetails);
			loginCommand = ChangePassword(loginCommand, broker);
			DeactivateMailbox(loginCommand, broker);

			return loginCommand;
		}

		internal string EDIServlet
		{
			get
			{
				var partialPathForServlet = SGCustomsDataRegistry.Instance.Mhx4EndpointPartialPathForServlet.Value;
				return SGCustomsDataRegistry.Instance.SendTestMessages.Value
					? SGCustomsDataRegistry.Instance.WebAddressTrial.Value.EffectiveValueForToday + partialPathForServlet
					: SGCustomsDataRegistry.Instance.WebAddress.Value.EffectiveValueForToday + partialPathForServlet;
			}
		}

		protected virtual LoginCommand Login(LoginDetails loginDetails)
		{
			var result = new LoginCommand(loginDetails, new MHUBSettingsProvider(), logger, helper.ShowVerboseLogging);
			result.Execute();
			return result;
		}

		#region Change Password

		void UpdatePasswordFormat(GlbExternalPassword_SGv4 password)
		{
			if (password.CurrentDecryptedPassword.Length < 12 && password.NextDecryptedPassword.IsEmpty)    // Update broker password to new format on first login, if user has not already updated password
			{
				password.NextDecryptedPassword = GenerateNewPassword();
			}
		}

		protected virtual LoginCommand ChangePassword(LoginCommand login, GlbStaff broker)
		{
			LoginCommand result = login;

			var password = helper.GetGlbExternalPassword(broker);

			if (result.LoginState == LoginCommand.LoginStateType.NotLoggedIn)
			{
				var deactivationCode = result.BrokerAccountError?.DeactivationCode ?? string.Empty;

				if (deactivationCode == SGDeactivationCodes.Codes.PEX || deactivationCode == SGDeactivationCodes.Codes.PCH)
				{
					result.BrokerAccountError = null;
					password.NextDecryptedPassword = GenerateNewPassword();
					result = Login(new LoginDetails(login.loginDetails.EdiServlet, password.GP_UserID, password.CurrentDecryptedPassword, password.NextDecryptedPassword));
				}
				else
				{
					logger.Log(password.GP_UserID + $" login failed but system don't update the password as the deactivationCode is '{deactivationCode}'.");
				}
			}

			if (result.LoginState == LoginCommand.LoginStateType.LoggedIn)
			{
				if (!password.NextDecryptedPassword.IsEmpty)
				{
					password.CurrentDecryptedPassword = password.NextDecryptedPassword;
					password.NextDecryptedPassword = "";
					broker.Factory.Save();

					TrySendChangedPasswordEmail(broker);

					logger.Log(password.GP_UserID + " password has been successfully updated.");
				}
			}

			return result;
		}

		string GenerateNewPassword()
		{
			if (randomGenerator == null)
			{
				randomGenerator = new Random();
			}

			return GetRandomChar() + GetRandomInt() + GetRandomInt() + GetRandomUppercaseChar() + GetRandomInt() + GetRandomChar() + GetRandomInt() + GetRandomInt() + GetRandomUppercaseChar() + GetRandomChar() + GetRandomChar() + GetRandomUppercaseChar() + GetRandomInt() + GetRandomChar();
		}

		string GetRandomChar()
		{
			if (randomGenerator == null)
			{
				randomGenerator = new Random();
			}

			return ((char)randomGenerator.Next(97, 122)).ToString();
		}

		string GetRandomUppercaseChar()
		{
			if (randomGenerator == null)
			{
				randomGenerator = new Random();
			}

			return ((char)randomGenerator.Next(65, 90)).ToString();
		}

		string GetRandomInt()
		{
			return (randomGenerator.Next(0, 9)).ToString();
		}

		Random randomGenerator;

		void TrySendChangedPasswordEmail(GlbStaff broker)
		{
			try
			{
				if (!broker.GS_EmailAddress.IsEmpty)
				{
					var password = helper.GetGlbExternalPassword(broker);
					string emailTemplateHtml;
					using (Stream stream = typeof(CheckBrokerMailbox).Assembly.GetManifestResourceStream("Enterprise.Customs.SG.V4.Business.BatchProcessor.HtmlTemplates.ChangedPasswordAdvice.htm"))
					{
						emailTemplateHtml = new StreamReader(stream).ReadToEnd();
					}
					emailTemplateHtml = emailTemplateHtml.Replace("{0}", password.GP_UserID);
					emailTemplateHtml = emailTemplateHtml.Replace("{1}", password.CurrentDecryptedPassword);
					emailTemplateHtml = emailTemplateHtml.Replace("{2}", broker.GS_FullName);
					emailTemplateHtml = emailTemplateHtml.Replace("{3}", BrandingFactory.Instance.ProductName);

					HtmlNotificationEmailSender emailSender = new HtmlNotificationEmailSender();
					EmailDef email = emailSender.CreateEmail(string.Format(CultureInfo.InvariantCulture, "Customs Broker: {0} {1} Password Changed", password.GP_UserID, helper.ApplicationDescription), emailTemplateHtml);

					email.AddRecipientForSystemCommunication(broker.GS_EmailAddress);
					Env.OutgoingCustomsMailManager.CreateAndSave(email);
				}
			}
			catch (Exception e) when (!e.IsCriticalException()) //if email fails, not a critical problem, inform us and Batch Processor continues working
			{
				ErrorReporter.ReportOnce(e.Message);
			}
		}

		#endregion

		#region Deactivation

		void DeactivateMailbox(LoginCommand login, GlbStaff broker)
		{
			if (login.LoginState == LoginCommand.LoginStateType.NotLoggedIn && login.BrokerAccountError != null)
			{
				var password = helper.GetGlbExternalPassword(broker);
				password.GP_PasswordStatus = login.BrokerAccountError.DeactivationCode;
				broker.Factory.Save();

				logger.AddBlankLine();
				logger.Log(string.Format(CultureInfo.InvariantCulture, "{0} - ({1} UserID: {2}) has been deactivated.", broker.GS_FullName, helper.ApplicationDescription, password.GP_UserID));
				logger.Log(login.BrokerAccountError.ErrorMessage);
				logger.AddBlankLine();

				SendDeactivationEmail(login.BrokerAccountError, broker);
			}
		}

		void SendDeactivationEmail(BrokerAccountError error, GlbStaff broker)
		{
			try
			{
				string emailTemplateHtml;
				var password = helper.GetGlbExternalPassword(broker);
				using (Stream stream = typeof(CheckBrokerMailbox).Assembly.GetManifestResourceStream("Enterprise.Customs.SG.V4.Business.BatchProcessor.HtmlTemplates.BrokerMailboxError.htm"))
				{
					emailTemplateHtml = new StreamReader(stream).ReadToEnd();
				}
				emailTemplateHtml = emailTemplateHtml.Replace("{0}", password.GP_UserID);
				emailTemplateHtml = emailTemplateHtml.Replace("{1}", error.ErrorMessage);
				emailTemplateHtml = emailTemplateHtml.Replace("{2}", broker.GS_FullName);
				emailTemplateHtml = emailTemplateHtml.Replace("<!--DynamicHtml-->", IncludeReActivationInstructions(error.DeactivationCode));

				HtmlNotificationEmailSender emailSender = new HtmlNotificationEmailSender();
				EmailDef email = emailSender.CreateEmail(string.Format(CultureInfo.InvariantCulture, "{0} {1}", password.GP_UserID, error.ErrorMessage), emailTemplateHtml);

				if (error.DeactivationCode == SGDeactivationCodes.Codes.FRZ)
				{
					using (Stream passwordRequestPDFStream = typeof(CheckBrokerMailbox).Assembly.GetManifestResourceStream("Enterprise.Customs.SG.V4.Business.BatchProcessor.PasswordResetRequest.pdf"))
					{
						byte[] bytes = AttachmentDef.StreamToByteArray(passwordRequestPDFStream);
						AttachmentDef attachment = new AttachmentDef("PasswordResetRequest.pdf", bytes);
						email.Attachments.Add(attachment);
					}
				}
				if (!broker.GS_EmailAddress.IsEmpty)
				{
					email.AddRecipientForSystemCommunication(broker.GS_EmailAddress);
				}
				else
				{
					var collection = new EmailGroupUtility().GetCompanyNotificationGroupEmails();
					string[] array = new string[collection.Count];
					collection.CopyTo(array, 0);
					email.AddRecipientForSystemCommunication(array);
				}
				Env.OutgoingCustomsMailManager.CreateAndSave(email);
			}
			catch (Exception e) when (!e.IsCriticalException()) //if email fails, not a critical problem, inform us and Batch Processor continues working
			{
				ErrorReporter.ReportOnce(e.Message);
			}
		}

		string IncludeReActivationInstructions(ZString deactivationCode)
		{
			string result;
			switch (deactivationCode)
			{
				case SGDeactivationCodes.Codes.PEX:
					result = AdviseExpiredPassword;
					break;
				case SGDeactivationCodes.Codes.PCH:
					result = AdviseChangePassword;
					break;
				case SGDeactivationCodes.Codes.FRZ:
					result = AdviseAccountFrozen;
					break;
				case SGDeactivationCodes.Codes.ANE:
					result = AdviseNonExistentAccount;
					break;
				case SGDeactivationCodes.Codes.IID:
					result = AdviseInvalidUserIdOrPasswordAccount;
					break;
				case SGDeactivationCodes.Codes.PIC:
					result = AdviseIncorrectPassword;
					break;
				case SGDeactivationCodes.Codes.PCS:
					result = AdviseNewPasswordInvalid;
					break;
				default:
					result = string.Empty;
					break;
			}
			return result;
		}

		#endregion

		#region Fields

		readonly BatchSGInterchangeHelper helper;
		readonly LoggingInformation logger;
		#endregion

		#region Login Error Descriptions

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		string AdviseExpiredPassword
		{
			get
			{
				return @"Your account has temporarily been deactivated, because your password has expired.<br /><br />
Your Singapore Customs mailbox has been deactivated to stop it from becoming frozen. 
To use your account again, you need to update your broker password.<br /><br />
Choose a new password and edit your user details within CargoWise One.
On the staff form, select the brokerage tab.
Enter your new password and save your changes.<br /><br />
The Batch Processor on its next submission will login and update the system with your new password and will re-activate your mailbox.";
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		string AdviseChangePassword
		{
			get
			{
				return @"Your account has temporarily been deactivated, because your password needs to be changed.<br /><br />
Your Singapore Customs mailbox has been deactivated to stop it from becoming frozen.
To use your account again, you need to update your broker password.<br /><br />
Choose a new password and edit your user details within CargoWise One.
On the staff form, select the brokerage tab.
Enter your new password and save your changes.<br /><br />
The Batch Processor on its next submission will login and update the system with your new password and will re-activate your mailbox.";
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		string AdviseAccountFrozen
		{
			get
			{
				return @"Your account has been deactivated, because your TradeNet id has been frozen.<br /><br />
Your Singapore Customs mailbox has been frozen due to the fact that an expired password was used three times to login.
To use your account again, you need to complete the “Request Form for Password Reset” and fax it to Crimson Logic.
The “Request Form for Password Reset” is attached to this email.<br /><br />
When Crimson Logic receives your Password Reset Request, they will reactivate your mailbox, and send you a temporary password.<br /><br />
Update your staff account in CargoWise One with the temporary password and set a new password.
On the staff form, select the brokerage tab and then choose Establish New Password.
Enter the temporary password provided by Crimson Logic and your new permanent password.<br /><br />
The Batch Processor on its next submission will login using the temporary password and update the TradeNet system with your new password and re-activate your mailbox.";
			}
		}

		string AdviseNonExistentAccount
		{
			get
			{
				return @"Your account has been deactivated, because the entered account does not exist at Singapore Customs.<br /><br />
Please enter a valid edi user mailbox/password.
The Batch Processor on its next submission will login using the newly entered details.";
			}
		}

		string AdviseInvalidUserIdOrPasswordAccount
		{
			get
			{
				return @"Your account has been deactivated, because the entered edi user mailbox/password is invalid<br /><br />
Please enter a valid edi user mailbox/password.
The Batch Processor on its next submission will login using the newly entered details.";
			}
		}

		string AdviseIncorrectPassword
		{
			get
			{
				return @"Your account has been deactivated, because the entered password is incorrect<br /><br />
Please enter a valid password.
The Batch Processor on its next submission will login using the newly entered details.";
			}
		}

		string AdviseNewPasswordInvalid
		{
			get
			{
				return @"Your account has been deactivated, because an invalid password has been used.<br /><br />
Your new password must not be the same as a previously used password. Password was not changed.
Please enter a valid new password.
The Batch Processor on its next submission will login using the newly entered details.";
			}
		}

		#endregion
	}
}
