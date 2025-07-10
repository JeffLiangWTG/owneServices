using System;
using System.Net;
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
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.Tracking.Web.Admin
{
	public partial class SetMasterPassword : RoutingEnabledPage
	{
		#region Binding

		protected override BusinessObject GetNewDataSource()
		{
			var result = new MasterPasswordManager(Person);
			return result;
		}

		protected MasterPasswordManager Manager => DataSource as MasterPasswordManager;

		protected override bool IsPersistDataSourceBetweenPostbacks => true;

		#endregion

		protected void Page_Load(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(QueryToken))
			{
				ShowInvalidTokenMessage();
				return;
			}

			if (!IsPostBack)
			{
				if (Person == null || !AccessControl.TryPeek(QueryToken, AccessTokenTypes.SetMasterPassword, out accessToken))
				{
					ShowInvalidTokenMessage();
				}
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

		protected void Update_Click(object sender, EventArgs e)
		{
			if (Contact != null && Person != null)
			{
				var webUserAdminManager = new WebUserAdminManager(Contact);

				var message = webUserAdminManager.SetMasterPassword(NewPassword.Text, NewPasswordConfirm.Text, PasswordInstructionType.Set);
				PasswordChangeMessage.Text = message;

				if (message == webUserAdminManager.PasswordChangeSuccess)
				{
					if (AccessControl.TryConsume(QueryToken, AccessTokenTypes.SetMasterPassword, out accessToken))
					{
						setTable.Visible = false;
						back.Visible = true;
						Factory.Save();

						if (!SiteUser.IsLoggedIn)
						{
							RedirectViaLoginRouter(RedirectUrl, Contact);
						}
						else
						{
							if (!string.IsNullOrEmpty(RedirectUrl))
							{
								Response.Redirect(RedirectUrl);
							}
						}
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

		internal GlbPerson Person
		{
			get
			{
				if (person == null)
				{
					PopulateContactAndPersonFromToken();
				}

				return person;
			}
		}

		GlbPerson person;

		internal OrgContact Contact
		{
			get
			{
				if (contact == null)
				{
					PopulateContactAndPersonFromToken();
				}

				return contact;
			}
		}

		OrgContact contact;

		void PopulateContactAndPersonFromToken()
		{
			if (AccessToken.ParentId != Guid.Empty && AccessToken.ParentTableCode == OrgContactSchema.Constants.Prefix)
			{
				contact = Factory.Load<OrgContact>(AccessToken.ParentId);
				person = contact?.Person;
			}
		}

		string RedirectUrl => AccessToken.Scope;

		ITokenizedAccessControl AccessControl { get; } = new TokenizedAccessControl();

		string QueryToken
		{
			get
			{
				if (string.IsNullOrEmpty(queryToken) || !peekSuccessful)
				{
					var queryStringData = Request.QueryString[SecureQueryString.QueryStringKey];
					if (!string.IsNullOrEmpty(queryStringData))
					{
						SecureQueryString queryString = null;

						try
						{
							queryString = new SecureQueryString(WebUtility.UrlDecode(queryStringData));
						}
						catch (QueryStringException)
						{
						}

						if (queryString != null)
						{
							queryToken = queryString[WebUserAdminManager.SetMasterPasswordKey];
						}
					}
					else
					{
						queryToken = Request.QueryString[WebUserAdminManager.SetMasterPasswordKey];
					}
				}

				return queryToken;
			}
		}

		string queryToken;

		AccessTokenInfo AccessToken
		{
			get
			{
				if (!peekSuccessful && !string.IsNullOrEmpty(QueryToken))
				{
					peekSuccessful = AccessControl.TryPeek(QueryToken, AccessTokenTypes.SetMasterPassword, out accessToken);
				}

				return accessToken;
			}
		}
		AccessTokenInfo accessToken;
		bool peekSuccessful;

		string InvalidTokenMessage => Res.GetString("cc019681-2110-40e5-916e-847cb03c130a", "The set link you have followed is invalid or expired.");

		protected override ZString ModuleNameForEventLogging => (NoResString)"Set Master Password"; // Event logging related

		protected override string GetPageName()
		{
			return WebTracker.Pages.SetMasterPassword;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.SetMasterPasswordPage;
		}
	}
}
