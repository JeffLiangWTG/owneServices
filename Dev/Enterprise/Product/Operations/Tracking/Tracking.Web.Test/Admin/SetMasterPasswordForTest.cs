using System;
using System.Collections.Specialized;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Enterprise.Tracking.Web.Admin;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class SetMasterPasswordForTest : SetMasterPassword
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		protected override Uri RequestUrl => new Uri("http://www.test.com/WebTracker/Admin/SetMasterPassword.aspx");

		public SetMasterPasswordForTest()
		{
			SetTable_Expose = new HtmlTable();
			PasswordChangeMessage_Expose = new Label();
			NewPassword_Expose = new TextBox();
			NewPasswordConfirm_Expose = new TextBox();
			Back_Expose = new HtmlGenericControl();
			SetMasterPasswordHeadingLabel_Expose = new ZTextLabel();
		}

		public void OnLoad()
		{
			OnLoad(EventArgs.Empty);
		}

		public void OnInit()
		{
			OnInit(EventArgs.Empty);
		}

		public NameValueCollection RequestQueryString_Expose => RequestQueryString;

		public HtmlTable SetTable_Expose
		{
			get => setTable;
			private set => setTable = value;
		}

		public Label PasswordChangeMessage_Expose
		{
			get => PasswordChangeMessage;
			private set => PasswordChangeMessage = value;
		}

		public TextBox NewPassword_Expose
		{
			get => NewPassword;
			private set => NewPassword = value;
		}

		public TextBox NewPasswordConfirm_Expose
		{
			get => NewPasswordConfirm;
			private set => NewPasswordConfirm = value;
		}

		public HtmlGenericControl Back_Expose
		{
			get => back;
			private set => back = value;
		}

		public ZTextLabel SetMasterPasswordHeadingLabel_Expose
		{
			get => SetMasterPasswordHeadingLabel;
			private set => SetMasterPasswordHeadingLabel = value;
		}

		public void OnUpdate()
		{
			Update_Click(this, EventArgs.Empty);
		}
	}
}
