using System;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefPremisesGateCodeForm : ZForm
	{
		protected RefPremisesGateCode PremisesGateCode;

		public RefPremisesGateCodeForm(RefPremisesGateCode bO)
			: base(bO)
		{
			this.PremisesGateCode = bO;
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ControlDpiScalingHelper.SetTop(ref ButtonsUserControl, MainStatusBar.Top - ButtonsUserControl.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
			if (PremisesGateCode != null && PremisesGateCode.R5_IsSystem)
			{
				DisplayMode = ODisplayMode.ReadOnly;
			}
		}
	}
}
