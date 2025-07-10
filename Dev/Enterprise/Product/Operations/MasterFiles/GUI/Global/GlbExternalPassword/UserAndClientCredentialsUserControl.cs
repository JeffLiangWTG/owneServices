using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public abstract partial class UserAndClientCredentialsUserControl : ZUserControl
	{
		public UserAndClientCredentialsUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			var dataSource = GetDataSource();
			if (dataSource == null)
			{
				return;
			}

			TextBox_UserCredentialPasswordStatusReason.Visible = !dataSource.UserCredentialPasswordStatusReason.IsEmpty;
			TextBox_ClientCredentialPasswordStatusReason.Visible = !dataSource.ClientCredentialPasswordStatusReason.IsEmpty;
		}

		protected virtual UserAndClientCredentials GetDataSource()
		{
			if (BindingSource.DataSource is UserAndClientCredentials credential)
			{
				return credential;
			}
			else
			{
				return null;
			}
		}
	}
}
