using System;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefCarrierConsortiumForm : ZForm
	{
		public RefCarrierConsortiumForm(RefCarrierConsortium consortium) : base(consortium)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ControlDpiScalingHelper.SetTop(ref PostingButtonsUserControl, MainStatusBar.Top - PostingButtonsUserControl.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
		}
	}
}
