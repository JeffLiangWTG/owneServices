using System;
using System.Web;
using System.Web.Security.AntiXss;
using CargoWise.Definitions.Authentication;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Web.Admin
{
	public partial class SetPassword : BasePage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			accessControl = new TokenizedAccessControl();
			var peekSuccessfully = accessControl.TryPeek(QueryToken, AccessTokenTypes.SetPassword, out accessToken);

			if (!IsPostBack)
			{
				if (string.IsNullOrEmpty(QueryToken) || !peekSuccessfully || WebContact == null)
				{
					ShowInvalidTokenMessage();
					return;
				}

				OrgCodeText.Text = WebContact.Header.OH_Code;
				SetPasswordHeadingLabel.Text = Res.GetString("eca1135d-9dde-45b0-8079-cfa4bbceacdb", "Set your password for login to <b>{0}</b>", AntiXssEncoder.HtmlEncode((WebContact as IGlbPersonPrimarySource)?.CompanyName ?? "", false));
			}
		}

		protected override void OnInit(EventArgs e)
		{
			if (!IsPostBack)
			{
				LogOff();
			}

			base.OnInit(e);
		}

		protected override bool Cacheable => false;

		protected void Update_Click(object sender, EventArgs e)
		{
			if (WebContact != null)
			{
				var webUserAdminManager = new WebUserAdminManager(WebContact);
				var result = webUserAdminManager.ChangePassword(NewPassword.Text, NewPasswordConfirm.Text, PasswordInstructionType.Set, true);

				PasswordChangeMessage.Text = result.Message;

				if (result.IsSuccess)
				{
					if (accessControl.TryConsume(QueryToken, AccessTokenTypes.SetPassword, out accessToken))
					{
						setTable.Visible = false;
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
			setTable.Visible = false;
			PasswordChangeMessage.Text = InvalidTokenMessage;
		}

		internal OrgContact WebContact
		{
			get
			{
				if (webContact == null && accessToken.ParentId != Guid.Empty)
				{
					webContact = Factory.Load<OrgContact>(accessToken.ParentId);
				}

				return webContact;
			}
		}

		OrgContact webContact;

		ITokenizedAccessControl accessControl;

		string QueryToken => HttpContext.Current.Request.QueryString[TrackingConstants.QueryStringKeys.SetPasswordKey];

		AccessTokenInfo accessToken;

		string InvalidTokenMessage => Res.GetString("cc019681-2110-40e5-916e-847cb03c130a", "The set link you have followed is invalid or expired.");

		protected override ZString ModuleNameForEventLogging => (NoResString)"Set Password"; // Event logging related

		protected override string GetPageName()
		{
			return WebTracker.Pages.SetPassword;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.SetPasswordPage;
		}
	}
}
