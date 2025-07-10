using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class SummaryForm : ZChildForm
	{
		public SummaryForm(List<IScreenedParty> screenedParties)
		{
			InitializeComponent();

			var vm = new SummaryViewModel(screenedParties, Close);
			var vc = new SummaryViewUserControl { ViewModel = vm };
			vc.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 524, true);

			Controls.Add(vc);
		}

		public override string FormCaption => ResString.GetMultilingualString("5A4CB4D8-B5EC-4255-BB01-6F96377807C9", "Screening Status Summary");
	}
}
