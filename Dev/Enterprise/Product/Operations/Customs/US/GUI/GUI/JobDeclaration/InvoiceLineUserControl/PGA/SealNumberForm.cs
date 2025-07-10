using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class SealNumberForm : ZChildForm
	{
		public SealNumberForm()
		{
			InitializeComponent();
		}

		public SealNumberForm(SealNumberBusinessObjectCollection dataSource)
			: base(dataSource)
		{
			InitializeComponent();
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		void GaveUpButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
