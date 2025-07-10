using System;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SetSQLPasswordForm : ZChildForm
	{
		public SetSQLPasswordForm(GlbStaff staff)
		{
			Staff = staff;
			this.BackgroundImage = BrandingFactory.Instance.SplashScreenImageWithoutProductName;
			this.MoveFormByMouseDrag(true);
		}

		readonly GlbStaff Staff;

		void OKButton_Click(object sender, EventArgs e)
		{
			try
			{
				new DbUserManager().SetPasswordForStaff(Staff, SqlPasswordTextBox.Text);
				DialogResult = DialogResult.OK;
			}
			catch (SqlException ex)
			{
				ErrorLabel.Text = ex.Message;
			}
		}

		void SetSQLPasswordForm_Load(object sender, EventArgs e)
		{
			ErrorLabel.Text = "";
		}		
	}
}
