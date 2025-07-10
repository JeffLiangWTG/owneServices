using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbCompanySignatureCredentialUserControl : ZUserControl
	{
		public GlbCompanySignatureCredentialUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			if (BindingSource.DataSource is GlbCompany company && company.SignatureCredentials != null)
			{
				company.SignatureCredentials.Factory.Saved += OnFactorySaved;
			}
		}

		void SignatureCredentialsGrid_BindingContextChanged(object sender, EventArgs e)
		{
			if (PasswordTextBox != null)
			{
				PasswordTextBox.KeyDown += PasswordTextBox_KeyDown;
				SetPasswordCharOnTextBoxes(PasswordTextBox);
			}
		}

		void PasswordTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (sender is DataGridTextBox textbox && textbox != null)
			{
				SetNoPasswordCharOnTextBoxIfEmpty(textbox);
			}
		}

		void OnFactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				SetPasswordCharOnTextBoxes(PasswordTextBox);
			}
		}

		void SetPasswordCharOnTextBoxes(DataGridTextBox textBox)
		{
			if (textBox != null)
			{
				textBox.PasswordChar = '*';
			}
		}

		void SetNoPasswordCharOnTextBoxIfEmpty(DataGridTextBox textbox)
		{
			if (string.IsNullOrEmpty(textbox.Text))
			{
				textbox.PasswordChar = '\0';
			}
		}

		protected DataGridTextBox PasswordTextBox
		{
			get
			{
				if (!SignatureCredentialsGrid.IsDesignMode())
				{
					var passwordColumn = SignatureCredentialsGrid.Columns[GlbCompanySignatureCredential.Schema.CurrentDecryptedPassword];
					var passwordColumnStyle = passwordColumn?.ColumnStyle as ZTextBoxColumnStyle;
					var textbox = passwordColumnStyle?.EditControl;
					return textbox as DataGridTextBox;
				}
				return null;
			}
		}
	}
}
