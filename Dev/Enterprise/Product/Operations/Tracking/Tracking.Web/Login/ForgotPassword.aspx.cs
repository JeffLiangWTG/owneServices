using CargoWise.EntityFramework;

using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;

namespace Enterprise.Tracking.Web
{
	public partial class ForgotPassword : BasePage
	{
		protected override bool Cacheable => false;

		#region Binding

		protected override BusinessObject GetNewDataSource()
		{
			return new TrackingPasswordResetHelper(Factory);
		}

		protected TrackingPasswordResetHelper PasswordResetHelper => DataSource as TrackingPasswordResetHelper;

		protected override bool IsPersistDataSourceBetweenPostbacks => true;

		#endregion

		protected void RemindBtn_Click(object sender, System.EventArgs e)
		{
			Message.Text = PasswordResetHelper.RequestPasswordReset(this);
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.ForgotPassword;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.ForgotPasswordPage;
		}
	}
}
