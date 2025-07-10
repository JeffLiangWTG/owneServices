using Enterprise.Core.Forms;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.GUI
{
	public partial class CargoGuideCredentialsControl : RegistryBusinessObjectTemplateZUserControl
	{
		public CargoGuideCredentialsControl()
		{
			InitializeComponent();
		}

		#region Viewing the Password

		void ViewButton_Click(object sender, System.EventArgs e)
		{
			ViewPassword();
		}

		void ViewPassword()
		{
			if (DeveloperLoginForm.TryAuthenticate())
			{
				Globals.Message.ShowInformation(PasswordTextBox.Text, Res.GetString("CargoGuideCredentialsControl|05d5dffd-7472-495d-85fa-44889c9c64d8", "Password"));
			}
		}

		#endregion

		readonly System.ComponentModel.IContainer components;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
