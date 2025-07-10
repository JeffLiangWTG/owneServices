using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbCompany_HungaryCredentialUserControl : ZUserControl
	{
		GlbCompanyExternalPasswordHUI Credential;

		public GlbCompany_HungaryCredentialUserControl()
		{
			InitializeComponent();
		}

		#region Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();

				if (Credential != null)
				{
					Credential.Factory.Saved -= Factory_Saved;
					Credential = null;
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Event Handlers

		protected override void OnLoad(EventArgs e)
		{
			if (BindingSource.DataSource is GlbCompany company)
			{
				Credential = company.HungaryEInvoicingCredentials;
			}
			if (Credential == null)
			{
				return;
			}

			SetPasswordCharOnTextBoxes();
			Credential.Factory.Saved += Factory_Saved;
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				SetPasswordCharOnTextBoxes();
			}
		}

		void TextBox_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (sender is ZTextBox textbox)
			{
				SetNoPasswordCharOnTextBoxIfEmpty(textbox);
			}
		}

		#endregion

		#region Implementation

		void SetPasswordCharOnTextBoxes()
		{
			var passwordChar = Credential.GP_PasswordStatus == ZString.Empty ? '\0' : '*';
			TextBox_PasswordHash.PasswordChar = passwordChar;
			TextBox_ReplacementKey.PasswordChar = passwordChar;
			TextBox_SignatureKey.PasswordChar = passwordChar;
		}

		void SetNoPasswordCharOnTextBoxIfEmpty(ZTextBox textbox)
		{
			if (string.IsNullOrEmpty(textbox.Text))
			{
				textbox.PasswordChar = '\0';
			}
		}

		#endregion
	}
}
