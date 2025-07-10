using Enterprise.Environment;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.PortHubs.GUI
{
	public partial class PortDepotCarrierSelectionDetailsForm : ZForm
	{
		public PortDepotCarrierSelectionDetailsForm(PortHubSelection selection)
			: base(selection)
		{
			InitializeComponent();

			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtons);

			if (!Env.Security.PortDepotSelectionModify.IsAllowed)
			{
				SetReadOnlyIncludingChildren();
			}
		}

		public override string FormHeading => Res.GetString("6caf4be2-f452-4ce1-9e02-9df3ea032f47", "Port/Depot/Carrier Selection");

		public override string FormCaption => Res.GetString("c693a206-29ed-4d50-9460-0c09e6c8998b", "Port/Depot/Carrier Selection");

		protected override bool AllowNew => false;
	}
}

