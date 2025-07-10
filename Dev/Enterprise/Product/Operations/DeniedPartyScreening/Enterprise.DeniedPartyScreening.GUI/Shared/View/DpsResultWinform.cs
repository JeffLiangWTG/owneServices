using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class DpsResultWinform : ZChildForm
	{
		public DpsResultWinform(DpsResultWinModel resultWinModel)
		{
			InitializeComponent();
			this.resultWinModel = resultWinModel;
			DpsResultControl.SetDataBinding(resultWinModel, "");
		}

		readonly DpsResultWinModel resultWinModel;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			resultWinModel.Close = Close;
		}
	}
}
