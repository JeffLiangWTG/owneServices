using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public sealed partial class JobExRateSysConfigForm : ZForm
	{
		AccExRateConfigs accExRateConfigs;
		Core.Forms.ZPostingButtonsUserControl PostingButtons;

		public JobExRateSysConfigForm()
		{
		}

		public JobExRateSysConfigForm(AccExchangeRateConfigurationCollection accExRateConfigCollection) : base(accExRateConfigCollection)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, this.PostingButtons);
		}

		public override ODisplayMode DisplayMode
		{
			set
			{
				base.DisplayMode = value;
				if (base.DisplayMode == ODisplayMode.Browse)
				{
					fApplyButton.Visible = false;
				}
			}
			get { return base.DisplayMode; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
