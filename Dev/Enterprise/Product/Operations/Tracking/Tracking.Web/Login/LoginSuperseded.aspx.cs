using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.Tracking.Web
{
	/// <summary>
	/// LoginSuperseded page for the web site
	/// </summary>
	public partial class LoginSuperseded : RoutingEnabledPage
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

			if (Helper.HasValidContacts && IdentityManager.IsValidID())
			{
				ErrorMessage.Visible = false;
			}
			else
			{
				SupersededInstructionsLabel.Visible = false;
				SetMasterPasswordButton.Visible = false;
				ErrorMessage.Text = Res.GetString("dff4a3f4-94f9-4784-81ff-2d87304e7ed9", "You have been redirected here because your account was deactivated. However, the link was invalid. Please attempt login again and if this issue is recurring, contact your system administrator.");
			}
		}

		protected void SetMasterPasswordButton_Click(object sender, EventArgs e)
		{
			var masterPasswordPage = new Uri(FormattableString.Invariant($"~/{TrackingConstants.RelativePath.SetMasterPasswordPage}"), UriKind.Relative); // Url string
			var setMasterPasswordUrl = Helper.GenerateSetMasterPasswordUrl(masterPasswordPage, LoginRouter.GetOriginalUrlFromRequest(Request), IdentityManager.Token);
			Response.Redirect(setMasterPasswordUrl.IsAbsoluteUri ? setMasterPasswordUrl.AbsoluteUri : setMasterPasswordUrl.OriginalString);
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.LoginSuperseded;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.LoginSupersededPage;
		}
	}
}
