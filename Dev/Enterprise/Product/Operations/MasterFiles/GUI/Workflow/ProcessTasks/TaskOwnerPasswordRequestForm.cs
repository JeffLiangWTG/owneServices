using System;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class TaskOwnerPasswordRequestForm : KForm, ICaptionRenderingSupport
	{
		public TaskOwnerPasswordRequestForm()
		{
			InitializeComponent();
			this.Text = Res.GetString("14d8dc0b-7738-4942-b9ed-89ae57ffdcdf", "Password");
		}

		public string LoginName { get; set; }
		public bool IsValidPassword { get; private set; }

		void btnOK_Click(object sender, EventArgs e)
		{
			var loginResult = Env.LoginController.ValidateUserLoginAndPassword(LoginName, PasswordTextBox.Text);
			IsValidPassword = loginResult.LoginValidated;

			if (!IsValidPassword)
			{
				Globals.Message.ShowError(loginResult.FailureMessage);
			}
		}

#if DEBUG

		public void SetPasswordTextForTest(string password)
		{
			PasswordTextBox.Text = password;
		}

#endif

		bool? ICaptionRenderingSupport.CaptionRenderingEnabled
		{
			get { return true; }
		}

		event EventHandler ICaptionRenderingSupport.CaptionRenderingEnabledChanged
		{
			add { }
			remove { }
		}
	}
}
