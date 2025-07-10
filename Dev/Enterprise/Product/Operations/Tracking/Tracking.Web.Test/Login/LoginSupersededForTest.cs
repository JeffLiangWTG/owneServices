using System;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class LoginSupersededForTest : LoginSuperseded
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public LoginSupersededForTest()
		{
			SupersededInstructionsLabel = new ZTextLabel();
			SetMasterPasswordButton = new Button();
			ErrorMessage = new ZTextLabel();
		}

		public void OnLoadForTest()
		{
			OnLoad(EventArgs.Empty);
		}

		public OrgContactSupersededHelper HelperForTest => Helper;

		public NameValueCollection RequestQueryString_Exposed => RequestQueryString;

		public void SetMasterPasswordButton_ClickForTest()
		{
			SetMasterPasswordButton_Click(null, EventArgs.Empty);
		}

		public ZTextLabel SupersededInstructionsLabelForTest => SupersededInstructionsLabel;

		public Button SetMasterPasswordButtonForTest => SetMasterPasswordButton;

		public ZTextLabel ErrorMessageForTest => ErrorMessage;
	}
}
