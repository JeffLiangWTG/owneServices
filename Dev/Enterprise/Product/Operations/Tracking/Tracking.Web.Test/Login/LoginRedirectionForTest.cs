using System;
using System.Web.UI.HtmlControls;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class LoginRedirectionForTest : LoginRedirection
	{
		public LoginRedirectionForTest()
		{
			ContactsBox = new HtmlGenericControl();
			LoginContactRepeater = new ZRepeater();
			LoginContactRepeater.BindTo = "ContactsForLogin";
			Controls.Add(LoginContactRepeater);
			ErrorMessage = new ZTextLabel();
		}

		public void OnLoadForTest()
		{
			OnLoad(EventArgs.Empty);
		}

		public OrgContactSupersededHelper HelperForTest => Helper;

		public HtmlGenericControl ContactsBoxForTest => ContactsBox;

		public ZTextLabel ErrorMessageForTest => ErrorMessage;

		public void ContinueButton_ClickExposed()
		{
			ContinueButton_Click(null, EventArgs.Empty);
		}

		protected override ZGlobal GetNewTestGlobal()
		{
			var result = new GlobalForLoginRedirectionPageTest();
			result.OnCustomSessionStart();
			return result;
		}
	}
}
