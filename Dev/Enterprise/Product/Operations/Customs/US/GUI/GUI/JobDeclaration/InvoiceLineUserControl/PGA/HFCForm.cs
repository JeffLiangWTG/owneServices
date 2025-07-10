using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class HFCForm : ZChildForm
	{
		public HFCForm(USHFCHeader header)
			: base(header)
		{
			InitializeComponent();
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		public void ChangeVisibilityOfControlsForProduct()
		{
			this.HFCImageSentCheckBox.Visible = false;
		}
	}
}
