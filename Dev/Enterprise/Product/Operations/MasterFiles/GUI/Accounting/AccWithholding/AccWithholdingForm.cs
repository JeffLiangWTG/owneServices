using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccWithholdingForm : ZForm
	{
		public AccWithholdingForm()
		{
		}

		public AccWithholdingForm(AccWithholding bO) : base(bO)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
		}
	}
}
