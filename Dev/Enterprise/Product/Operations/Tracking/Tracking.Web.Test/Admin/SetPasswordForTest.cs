using System;
using System.Collections.Specialized;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Enterprise.Tracking.Web.Admin;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class SetPasswordForTest : SetPassword
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		protected override Uri RequestUrl => new Uri("http://www.test.com/WebTracker/Admin/SetPassword.aspx");

		public SetPasswordForTest()
		{
			SetTable_Expose = new HtmlTable();
			OrgCodeText_Expose = new ZTextLabel();
			PasswordChangeMessage_Expose = new Label();
			NewPassword_Expose = new TextBox();
			NewPasswordConfirm_Expose = new TextBox();
			Back_Expose = new HtmlGenericControl();
			SetPasswordHeadingLabel_Expose = new ZTextLabelNoEncode();
		}

		public void OnLoad()
		{
			OnLoad(EventArgs.Empty);
		}

		public void OnInit()
		{
			OnInit(EventArgs.Empty);
		}

		public bool Cacheable_Expose => Cacheable;

		public NameValueCollection RequestQueryString_Expose => RequestQueryString;

		public HtmlTable SetTable_Expose
		{
			get => setTable;
			private set => setTable = value;
		}

		public ZTextLabel OrgCodeText_Expose
		{
			get => OrgCodeText;
			private set => OrgCodeText = value;
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

		public ZTextLabelNoEncode SetPasswordHeadingLabel_Expose
		{
			get => SetPasswordHeadingLabel;
			private set => SetPasswordHeadingLabel = value;
		}

		public void OnUpdate()
		{
			Update_Click(this, EventArgs.Empty);
		}
	}
}
