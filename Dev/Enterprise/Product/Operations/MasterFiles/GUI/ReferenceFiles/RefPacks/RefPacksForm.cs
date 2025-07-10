using System;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefPacksForm : ZForm
	{
		public RefPacksForm(BaseRefPacks topBizObj)
			: base(topBizObj)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, SaveButton);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ControlDpiScalingHelper.SetTop(ref SaveButton, MainStatusBar.Top - SaveButton.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
		}
	}
}
