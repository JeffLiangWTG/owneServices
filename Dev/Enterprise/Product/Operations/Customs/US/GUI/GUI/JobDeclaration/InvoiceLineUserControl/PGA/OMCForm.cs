using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class OMCForm : ZChildForm
	{
		public OMCForm(OMCHeader header)
			: base(header)
		{
			InitializeComponent();
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
