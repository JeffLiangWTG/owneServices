using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class PN1UserControl : ZUserControl, IAMSControlIdentity
	{
		public PN1UserControl()
		{
			InitializeComponent();
		}

		public void SetInvisibleLotGrid()
		{
			this.Controls.Add(this.LinesGroupBox);

			LotCodeGroupBox.Dispose();
			LotCodesGrid.Dispose();
			splitContainer1.Dispose();
		}

		public string IdentityCode => AMSProgramList.Codes.PN1;
	}
}
