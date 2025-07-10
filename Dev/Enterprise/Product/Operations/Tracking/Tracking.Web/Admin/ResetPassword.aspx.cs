using System;
using System.Linq;
using System.Web;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Newtonsoft.Json;
using PasswordResetHelper = Enterprise.ZArchitecture.Web.GUI.PasswordResetHelper;

namespace Enterprise.Tracking.Web.Admin
{
	public partial class ResetPassword : BasePage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				if (string.IsNullOrEmpty(QueryToken) || string.IsNullOrEmpty(Email))
				{
					ShowInvalidTokenMessage();
				}
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			PasswordExpiredMessageLabel.Visible = string.Equals(Request.QueryString[PasswordRotationRoutingDescriptor.RefKey], PasswordRotationRoutingDescriptor.RefValue, StringComparison.OrdinalIgnoreCase);
		}

		protected override void OnInit(EventArgs e)
		{
			if (!IsPostBack)
			{
				LogOff();
			}

			base.OnInit(e);
		}

		protected override BusinessObject GetNewDataSource()
		{
			var factory = new BusinessObjectFactory();
			factory.SuspendValidation();
			return new PasswordResetHelper(factory, Email, ResetInfo.OrgCode);
		}

		protected override bool Cacheable => false;

		protected override bool IsPersistDataSourceBetweenPostbacks => true;

		protected PasswordResetHelper PasswordResetHelper => DataSource as PasswordResetHelper;

		protected void Update_Click(object sender, EventArgs e)
		{
			var selectedWebUser = GetSelectedWebUser();
			if (selectedWebUser != null)
			{
				var webUserAdminManager = new WebUserAdminManager(selectedWebUser);
				var result = webUserAdminManager.ChangePassword(NewPassword.Text, NewPasswordConfirm.Text, ignoreEmailOverride: true);

				PasswordChangeMessage.Text = result.Message;

				if (result.IsSuccess)
				{
					WebApplicationLoginHelper.WriteLoginHashToCookie(selectedWebUser.OrgCode, selectedWebUser.OC_Email);

					ITokenizedAccessControl accessControl = new TokenizedAccessControl();
					if (accessControl.TryConsume(QueryToken, AccessTokenTypes.ResetPassword, out accessToken))
					{
						resetTable.Visible = false;
						HeadingMessageDiv.Visible = false;
						back.Visible = true;
						Factory.Save();
					}
				}
			}
			else
			{
				ShowInvalidTokenMessage();
			}
		}

		void ShowInvalidTokenMessage()
		{
			resetTable.Visible = false;
			HeadingMessageDiv.Visible = false;
			PasswordChangeMessage.Text = InvalidTokenMessage;
		}

		OrgContact GetSelectedWebUser()
		{
			if (!string.IsNullOrEmpty(Email))
			{
				var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK, OrgContactSchema.OC_OH);
				orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_Code, PasswordResetHelper.CompanyCode);

				var orgContactQuery = new ZDBOnlyQuery(typeof(OrgContact));
				orgContactQuery.AddToFilter(OrgContactSchema.OC_Email, Email);
				orgContactQuery.AddToFilter(OrgContactSchema.OC_IsActive, true);
				orgContactQuery.AddToFilter(OrgContactSchema.OC_WebAccessEnabled, true);
				orgContactQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
				orgContactQuery.OrderBy = OrgContactSchema.Constants.OC_ContactName;

				var contacts = Factory.Load<OrgContact>(orgContactQuery);
				return contacts.FirstOrDefault();
			}

			return null;
		}

		string QueryToken => HttpContext.Current.Request.QueryString[TrackingConstants.QueryStringKeys.ResetPasswordKey];

		string InvalidTokenMessage => Res.GetString("72ae1bf9-d086-4382-a080-ff035f12ae2e", "The reset link you have followed is invalid or expired.");

		AccessTokenInfo accessToken;

		PasswordResetInfo ResetInfo
		{
			get
			{
				if (resetInfo == null)
				{
					ITokenizedAccessControl accessControl = new TokenizedAccessControl();
					var peekSuccessfully = accessControl.TryPeek(QueryToken, AccessTokenTypes.ResetPassword, out accessToken);
					if (!peekSuccessfully || string.IsNullOrEmpty(accessToken.Scope))
					{
						resetInfo = new PasswordResetInfo();
					}
					else
					{
						if (accessToken.Scope.StartsWith("{", StringComparison.OrdinalIgnoreCase) && accessToken.Scope.EndsWith("}", StringComparison.OrdinalIgnoreCase))
						{
							try
							{
								resetInfo = JsonConvert.DeserializeObject<PasswordResetInfo>(accessToken.Scope);
							}
							catch (JsonReaderException)
							{
								resetInfo = new PasswordResetInfo();
								ErrorReporter.ReportOnce($"The PasswordResetInfo:{accessToken.Scope} should be valid json string");
							}
						}
						else
						{
							resetInfo = new PasswordResetInfo() { ContactEmail = accessToken.Scope };
						}
					}
				}
				return resetInfo;
			}
		}

		PasswordResetInfo resetInfo;

		string Email => ResetInfo.ContactEmail;

		protected override ZString ModuleNameForEventLogging => (NoResString)"Reset Password"; // Event logging related

		protected override string GetPageName()
		{
			return WebTracker.Pages.ResetPassword;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.ResetPasswordPage;
		}
	}
}
