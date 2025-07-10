using System.Linq;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.GUI
{
	public partial class IntercompanyTariffForm : RatingForm
	{
		public IntercompanyTariffForm(IntercompanyTariff intercompanyTariff)
			: base(intercompanyTariff)
		{
			InitializeComponent();

			clientInformationUserControl.SetOrgHeaderLabelToSupplier();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);

			PlugIns.Add(ControllerIDs.eDocsPlugIn);

			InitializeRatesServiceMenuItems();
		}

		protected override ZTabControl TopLevelTabControl =>
			intercompanyTariffTabControl.TopLevelTabControl;

		protected override void SetReadOnlyIncludingChildren() =>
			this.SetReadOnlyIncludingChildren(new[] { rateEntryFilterStripControl.Name }.ToList());
	}
}
