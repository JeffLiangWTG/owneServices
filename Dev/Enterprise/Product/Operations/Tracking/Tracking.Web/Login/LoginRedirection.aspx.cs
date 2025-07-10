using System;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	/// <summary>
	/// LoginRedirection page for the web site
	/// </summary>
	public partial class LoginRedirection : RoutingEnabledPage
	{
		#region Binding

		protected override BusinessObject GetNewDataSource()
		{
			var result = new OrgContactSupersededHelper(IdentityManager.Contact);
			return result;
		}

		protected OrgContactSupersededHelper Helper => DataSource as OrgContactSupersededHelper;

		protected override bool IsPersistDataSourceBetweenPostbacks => true;

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!Helper.HasValidContacts || !Helper.ContactsForLogin[0].Person.HasPassword)
			{
				HideLabelsAndShowError(Res.GetString("926a1d61-d278-41fd-bb41-15369c647379", "You have been redirected here incorrectly. Please attempt login again and if this issue is recurring, contact your system administrator."));
				return;
			}

			ErrorMessage.Visible = false;

			var defaultItem = LoginContactRepeater.Items.Cast<RepeaterItem>().FirstOrDefault(item =>
			{
				var companyCodeLabel = (ZTextLabel)item.FindControl("CompanyCode");
				if (item.ItemType != ListItemType.Item && item.ItemType != ListItemType.AlternatingItem || companyCodeLabel == null)
				{
					return false;
				}

				return Helper.DefaultLoginContact.OrganisationCode.EqualsIgnoringCase(companyCodeLabel.Text);
			});
			var defaultRadioButton = (ZRadioButton)defaultItem?.FindControl((NoResString)"Checked"); // an attribute name
			if (defaultRadioButton != null)
			{
				defaultRadioButton.Checked = true;
			}
		}

		void HideLabelsAndShowError(string errorMessage)
		{
			ContactsBox.Visible = false;
			ErrorMessage.Text = errorMessage;
		}

		protected void ContinueButton_Click(object sender, EventArgs e)
		{
			var checkedItem = LoginContactRepeater.Items.Cast<RepeaterItem>().FirstOrDefault(item =>
				(item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem) &&
				((ZRadioButton)item.FindControl((NoResString)"Checked")).Checked); // an attribute name

			if (checkedItem != null)
			{
				var label = (ZTextLabel)checkedItem.FindControl("CompanyCode");
				var loginContact = Helper.ContactsForLogin.Cast<OrgContact>().FirstOrDefault(x => x.OrganisationCode.Equals(label.Text));

				if (loginContact == null)
				{
					ShowContactError();
					return;
				}

				Helper.DeactivateRedirectionContacts();

				RedirectViaLoginRouter(LoginRouter.GetOriginalUrlFromRequest(Request), loginContact);
			}
			else
			{
				ShowContactError();
			}

			void ShowContactError()
			{
				ErrorMessage.Text = Res.GetString("ef8e16a9-c9dd-4dde-aae3-8b2ad6d61172", "Please select a contact for login");
				ErrorMessage.Visible = true;
			}
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.LoginRedirection;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.LoginRedirectionPage;
		}

		protected void LoginContactRepeater_OnItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
			{
				return;
			}

			var radioButton = (ZRadioButton)e.Item.FindControl((NoResString)"Checked"); // A control name
			const string script = "SetUniqueRadioButton('LoginContactRepeater.*LoginCheckBoxes', this)";
			radioButton.Attributes.Add((NoResString)"onclick", script); // an attribute name
		}
	}
}
