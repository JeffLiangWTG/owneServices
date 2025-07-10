using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class TrackingStatForm : ZChildForm
	{
		public TrackingStatForm()
		{
			InitializeComponent();
		}

		public TrackingStatForm(ClickStatModel statModel)
			: base(statModel)
		{
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
