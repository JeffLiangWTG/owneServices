using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class VAT404DocumentUserControl : ZUserControl
	{
		public VAT404DocumentUserControl()
		{
			InitializeComponent();
		}

		void PreviewButton_Click(object sender, System.EventArgs e)
		{
			(this.ImportersGrid.GetCurrent() as VAT404Document)?.PreviewDocument();
		}
	}
}
