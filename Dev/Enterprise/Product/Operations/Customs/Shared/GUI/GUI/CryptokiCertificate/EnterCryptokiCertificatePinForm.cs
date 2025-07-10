using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Certificates
{
	public partial class EnterCryptokiCertificatePinForm : ZChildForm
	{
		public EnterCryptokiCertificatePinForm(UserEnterableTokenPin userEnterableTokenPin) : base(userEnterableTokenPin)
		{
			InitializeComponent();
		}

		public new UserEnterableTokenPin BusinessEntity => (UserEnterableTokenPin)base.BusinessEntity;

		public override string FormHeading => Res.GetString("ED4F1A3D-A32F-4450-AA31-2F6FD22F5EB7", "Enter Certificate PIN");

		void OkButton_Click(object sender, System.EventArgs e)
		{
			PerformValidation();
			if (BusinessEntity.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		void AbortButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
