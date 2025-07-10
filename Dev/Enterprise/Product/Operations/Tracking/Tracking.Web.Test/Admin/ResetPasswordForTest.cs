using System;
using System.Collections.Specialized;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Enterprise.Tracking.Web.Admin;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ResetPasswordForTest : ResetPassword
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		protected override Uri RequestUrl => new Uri("http://www.test.com/WebTracker/Admin/ResetPassword.aspx");
		public ResetPasswordForTest()
		{
			ResetTable_Expose = new HtmlTable();
			PasswordChangeMessage_Expose = new Label();
			NewPassword_Expose = new TextBox();
			NewPasswordConfirm_Expose = new TextBox();
			Back_Expose = new HtmlGenericControl();
			PasswordExpiredMessageLabel_Expose = new ZTextLabel { Visible = false };
			HeadingMessageDiv_Expose = new HtmlGenericControl();
		}

		public void OnLoad()
		{
			OnLoad(EventArgs.Empty);
		}

		public void OnInit()
		{
			OnInit(EventArgs.Empty);
		}

		protected override bool ShouldTraverseControlTreeAndExtractOnInit => false;

		public bool Cacheable_Expose => Cacheable;

		public NameValueCollection RequestQueryString_Expose => RequestQueryString;

		public HtmlTable ResetTable_Expose
		{
			get => resetTable;
			private set => resetTable = value;
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

		public ZTextLabel PasswordExpiredMessageLabel_Expose
		{
			get => PasswordExpiredMessageLabel;
			private set => PasswordExpiredMessageLabel = value;
		}

		public HtmlGenericControl HeadingMessageDiv_Expose
		{
			get => HeadingMessageDiv;
			private set => HeadingMessageDiv = value;
		}

		public void OnUpdate()
		{
			Update_Click(this, EventArgs.Empty);
		}
	}
}
