using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class CPCForm : ZChildForm
	{
		public CPCForm()
		{
		}

		public CPCForm(SGCPC cpc) : base(cpc)
		{
		}

		SGCPC CPC
		{
			get { return (SGCPC)BusinessEntity; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			aPCDescription.Text = CPC.SG_APCCodeDescription;
			pC1.Text = CPC.SG_PC1;
			pC2.Text = CPC.SG_PC2;
			pC3.Text = CPC.SG_PC3;
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
