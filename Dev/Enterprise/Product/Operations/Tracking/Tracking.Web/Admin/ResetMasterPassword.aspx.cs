using System;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Newtonsoft.Json;

namespace Enterprise.Tracking.Web.Admin
{
	public partial class ResetMasterPassword : RoutingEnabledPage
	{
		#region Binding

		protected override BusinessObject GetNewDataSource()
		{
			var result = new ResetMasterPasswordManager(Email, Factory, ResetInfo.OrgCode);
			return result;
		}

		protected ResetMasterPasswordManager Manager => DataSource as ResetMasterPasswordManager;

		protected override bool IsPersistDataSourceBetweenPostbacks => true;

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (IsPostBack)
			{
				return;
			}

			if (string.IsNullOrEmpty(QueryToken))
			{
				ShowInvalidTokenMessage();
				return;
			}

			if (string.IsNullOrEmpty(QueryToken) || string.IsNullOrEmpty(Email))
			{
				ShowInvalidTokenMessage();
				return;
			}

			var defaultItem = LoginContactsRepeater.Items.Cast<RepeaterItem>().FirstOrDefault();
			var defaultRadioButton = (ZRadioButton)defaultItem?.FindControl((NoResString)"Checked"); // an attribute name
			if (defaultRadioButton != null)
			{
				defaultRadioButton.Checked = true;
			}

			ResetMasterPasswordHeadingLabel.Visible = Manager.PersonsForBinding.Count > 1;
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

		protected void Update_Click(object sender, EventArgs e)
		{
			var defaultContact = GetSelectedContact();

			if (defaultContact != null)
			{
				if (Guid.TryParse(ResetInfo.EmailTemplateCompanyPk, out var companyPk))
				{
					defaultContact.CompanyPKForEmailTemplate = companyPk;
				}

				var webUserAdminManager = new WebUserAdminManager(defaultContact);

				var message = webUserAdminManager.SetMasterPassword(NewPassword.Text, NewPasswordConfirm.Text);
				PasswordChangeMessage.Text = message;

				if (message == webUserAdminManager.PasswordChangeSuccess)
				{
					WebApplicationLoginHelper.WriteLoginHashToCookie(defaultContact.OrgCode, defaultContact.OC_Email);

					if (AccessControl.TryConsume(QueryToken, AccessTokenTypes.ResetMasterPassword, out accessToken))
					{
						HideSetPasswordContentAndShowMessage(message);
						back.Visible = true;
						Factory.Save();
					}
				}
			}
			else
			{
				PasswordChangeMessage.Text = Res.GetString("0a7cfa3a-4d1f-4af8-9d44-924b0530d2dc", "Please select a contact for login");
				PasswordChangeMessage.Visible = true;
			}
		}

		protected OrgContact GetSelectedContact()
		{
			return GetSelectedPerson()?.ContactCollection?.Cast<OrgContact>().FirstOrDefault(x => x.OC_IsActive && x.OC_WebAccessEnabled);
		}

		protected virtual GlbPerson GetSelectedPerson()
		{
			var checkedItem = LoginContactsRepeater.Items.Cast<RepeaterItem>().FirstOrDefault(item =>
				(item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem) &&
				((ZRadioButton)item.FindControl((NoResString)"Checked")).Checked); // Untranslatable control name

			if (checkedItem == null)
			{
				return null;
			}

			var label = (ZTextLabel)checkedItem.FindControl("RelatedAccounts"); // Untranslatable control name
			var personWrapper = Manager.PersonsForBinding.FirstOrDefault(x => x.RelatedAccounts.Equals(label.Text));

			return personWrapper?.Person;
		}

		void ShowInvalidTokenMessage()
		{
			HideSetPasswordContentAndShowMessage(InvalidTokenMessage);
		}

		void HideSetPasswordContentAndShowMessage(string message)
		{
			HeadingMessageDiv.Visible = false;
			ContactsBox.Visible = false;
			setTable.Visible = false;
			PasswordChangeMessage.Text = message;
		}

		#region Properties

		PasswordResetInfo ResetInfo
		{
			get
			{
				if (resetInfo == null)
				{
					ITokenizedAccessControl accessControl = new TokenizedAccessControl();
					peekSuccessful = accessControl.TryPeek(QueryToken, AccessTokenTypes.ResetMasterPassword, out accessToken);
					if (!peekSuccessful || string.IsNullOrEmpty(accessToken.Scope))
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
							resetInfo = new PasswordResetInfo { ContactEmail = accessToken.Scope };
						}
					}
				}
				return resetInfo;
			}
		}

		PasswordResetInfo resetInfo;

		string Email => ResetInfo.ContactEmail;

		ITokenizedAccessControl AccessControl { get; } = new TokenizedAccessControl();

		string QueryToken
		{
			get
			{
				if (string.IsNullOrEmpty(queryToken) || !peekSuccessful)
				{
					queryToken = Request.QueryString[TrackingConstants.QueryStringKeys.ResetPasswordKey];
				}

				return queryToken;
			}
		}

		string queryToken;

		AccessTokenInfo accessToken;
		bool peekSuccessful;

		string InvalidTokenMessage => Res.GetString("07aa7f28-b5d1-4c3f-ac74-65fbfdb92c42", "The set link you have followed is invalid or expired.");

		protected override ZString ModuleNameForEventLogging => (NoResString)"Set Master Password"; // Event logging related

		#endregion

		protected override string GetPageName()
		{
			return WebTracker.Pages.ResetMasterPassword;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.ResetMasterPasswordPage;
		}
	}
}
