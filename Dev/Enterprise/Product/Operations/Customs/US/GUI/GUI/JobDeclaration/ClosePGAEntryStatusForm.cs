using System;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ClosePGAEntryStatusForm : ZChildForm
	{
		public ClosePGAEntryStatusForm(CusDisposition bo)
			: base(bo)
		{
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			this.Close();
			DialogResult = DialogResult.OK;
		}
	}
}
