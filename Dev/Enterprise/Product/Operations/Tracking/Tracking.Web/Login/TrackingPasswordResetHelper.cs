using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Security;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web
{
	public class TrackingPasswordResetHelper : PasswordResetHelper
	{
		public TrackingPasswordResetHelper(BusinessObjectFactory factory, string email) : base(factory, email)
		{
		}

		public TrackingPasswordResetHelper(BusinessObjectFactory factory) : base(factory)
		{
		}

		public string RequestPasswordReset(BasePage page)
		{
			RunPreSaveValidation();

			if (!HasErrors && DefaultOrgContact != null)
			{
				if (!DefaultOrgContact.OC_WebAccessEnabled || !DefaultOrgContact.OC_IsActive)
				{
					NotifyWebAdminNoWebAccount();
				}
				else
				{
					var shouldSendMasterPassword = ((IPasswordInstructionEmailSource)DefaultOrgContact).ShouldSendMasterPassword;
					var absoluteUrl = page.Request.Url.GetLeftPart(UriPartial.Path);
					absoluteUrl = absoluteUrl.Substring(0, absoluteUrl.Length - page.PageRelativePath.Length);

					var resetPasswordUri = new UriBuilder(absoluteUrl) { Port = -1 };
					if (!page.Request.IsSecureConnection && page.Request.Headers.Get(XForwardedProto) == Https)
					{
						resetPasswordUri.Scheme = Https;
					}

					var resetPasswordPage = shouldSendMasterPassword ? TrackingConstants.RelativePath.ResetMasterPasswordPage : TrackingConstants.RelativePath.ResetPasswordPage;
					resetPasswordUri.Path = $"{resetPasswordUri.Path.TrimEnd('/')}/{resetPasswordPage}";
					resetPasswordUri.Query = $"{TrackingConstants.QueryStringKeys.ResetPasswordKey}=";

					var contactWithoutCompanyInfo = new ContactWithoutCompanyInfo(DefaultOrgContact.Name, DefaultOrgContact.Email, resetPasswordUri.ToString(),
						DefaultOrgContact.Salutation, DefaultOrgContact.ExtraInstruction, DefaultOrgContact.OrgCode, DefaultOrgContact.EmailOrgCodes, null, shouldSendMasterPassword);
					PasswordInstructionEmailSender.SendPasswordResetEmail(contactWithoutCompanyInfo);
				}
			}

			return GenericEmailSentMessage;
		}

		string GenericEmailSentMessage => Res.GetString("3898289B-FFF1-4695-8A39-978539D254F8", "If the email address is linked to a valid account, a reset password link has been sent.");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "standardized constant")]
		const string Https = "https";
		const string XForwardedProto = "X-Forwarded-Proto";
	}
}
