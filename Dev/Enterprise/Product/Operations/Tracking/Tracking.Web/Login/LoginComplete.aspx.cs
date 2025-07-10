using System;
using System.Linq;
using System.Net;
using System.Web.Security;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.Tracking.Web
{
	public partial class LoginComplete : RoutingEnabledPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (SiteUser == null)
			{
				RedirectToDefaultPage();
				return;
			}

			var originalUrl = LoginRouter.GetOriginalUrlFromRequest(Request);

			if (!IdentityManager.IsValidID())
			{
				Redirect(originalUrl);
				return;
			}

			if (SiteUser.IsLoggedIn)
			{
				IdentityManager.ConsumeToken();
				Redirect(originalUrl);
				return;
			}

			var contact = IdentityManager.Contact;
			var router = new TrackingLoginRouter(originalUrl, IdentityManager.Token);
			IdentityManager.ConsumeToken();

			if (router.HasAnyRoutingRequired || !contact.IsValidForWebLogin)
			{
				RedirectToDefaultPage();
				return;
			}

			var hash = ZArchitecture.Web.GUI.WebApplicationLoginHelper.RetrieveLoginHashFromCookie(contact.OrgCode, contact.OC_Email);

			SiteUser.Login(contact.OrgCode, contact.OC_Email, ZArchitecture.Environment.User.WebTransientPassword, hash, false);
			FormsAuthentication.SetAuthCookie(SiteUser.LoggedInUserName, false);

			if (SiteUser.IsLoggedIn)
			{
				EventLogHelper eventLogHelper = new EventLogHelper();
				eventLogHelper.CreateLogForUserLoggedIn(Page.SiteUser);

				if (WebEnv.AppInstance is ILicenceUsageLogWriter writer)
				{
					writer.WriteLicenceUsageLog(Environment.Env.Licence.WebTracker);
				}
			}

			Redirect(originalUrl);
		}

		void Redirect(Uri originalUrl)
		{
			var redirectUrl = GetRedirectUrl(originalUrl);

			if (UserShouldAgreeToTermsAndConditions())
			{
				var termsAndConditionsRedirect = !string.IsNullOrEmpty(redirectUrl) ? redirectUrl : AppInstance.DefaultPage;
				var termsAndConditions = string.Format("{0}?{1}={2}", AppInstance.TermsAndConditionsPage, Global.TermsAndConditionsRedirectUrlTag, WebUtility.HtmlEncode(termsAndConditionsRedirect));
				Response.Redirect(termsAndConditions);
				return;
			}

			if (!string.IsNullOrEmpty(redirectUrl))
			{
				Response.Redirect(redirectUrl);
				return;
			}

			RedirectToDefaultPage();
		}

		string GetRedirectUrl(Uri originalUrl)
		{
			var clientPortalUrl = SiteUser.GetClientPortalUrl();

			if (!string.IsNullOrEmpty(clientPortalUrl))
			{
				return clientPortalUrl;
			}

			var originalUrlString = originalUrl != null ? originalUrl.IsAbsoluteUri ? originalUrl.AbsoluteUri : originalUrl.OriginalString : string.Empty;

			return originalUrlString;
		}

		void RedirectToDefaultPage()
		{
			var defaultRedirectLocation = WebDataRegistry.Instance.WebTrackerUrl.GetFallBackValueAtAllLevels(SiteUser?.LoggedInOrgContact?.CompanyPKForLogin ?? Guid.Empty, Guid.Empty, Guid.Empty);

			if (!string.IsNullOrEmpty(defaultRedirectLocation))
			{
				Response.Redirect(defaultRedirectLocation);
				return;
			}

			RedirectToErrorPage();
		}

		void RedirectToErrorPage()
		{
			AppInstance.ReportError(Res.GetString("857e4682-4e00-41af-91a9-1bc3e7a505bf", "Login Failed"), Res.GetString("c21cf743-76d6-47dc-a19c-11a6e7485d0c", "Please attempt to login again."));
		}

		bool UserShouldAgreeToTermsAndConditions()
		{
			if (SiteUser == null || !SiteUser.IsLoggedIn || string.IsNullOrEmpty(WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions.Value))
			{
				return false;
			}

			var signedDateTime = SiteUser.LoggedInWebContact.OC_WebContractSignedDate;

			if (!signedDateTime.IsEmpty)
			{
				var logQuery = new ZQuery(StmDataSchema.SD_Name, WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions.Name);
				var stmData = Factory.LoadTop1<StmData>(logQuery);
				var latestEditLog = stmData.Logs.GetAllLogs().OfType<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.EditedARecord.Code).OrderByDescending(x => x.SL_PostedTimeUtc).FirstOrDefault();

				if (latestEditLog != null)
				{
					return Env.Time.GetUtcFromLocalTime(signedDateTime.ToDateTime()) < latestEditLog.SL_PostedTimeUtc;
				}
			}

			return signedDateTime.IsEmpty;
		}
	}
}
