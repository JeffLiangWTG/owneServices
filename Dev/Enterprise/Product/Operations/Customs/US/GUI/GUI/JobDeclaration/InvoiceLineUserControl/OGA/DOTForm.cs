using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class DOTForm : ZChildForm
	{
		public DOTForm()
		{
		}

		public DOTForm(DOT dot)
			: base(dot)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
