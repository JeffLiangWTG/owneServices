using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.PortHubs.GUI
{
	public partial class PortHubSelectionForm : ZForm
	{
		public PortHubSelectionForm(PortHubSelectionCollectionWrapper portHubsWrapper)
			: base(portHubsWrapper)
		{
			InitializeComponent();

			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtons);

			if (!Env.Security.PortDepotSelectionModify.IsAllowed)
			{
				SetReadOnlyIncludingChildren();
			}
		}

		public override string FormHeading => Res.GetString("596ffa72-0c09-4017-a063-023d5ec77655", "Port and Depot Selection");

		protected override ContinueWithSave ValidateAndSave()
		{
			portHubFilterStripControl.ClearAllFilterStripValuesAndReload();
			return base.ValidateAndSave();
		}
	}
}
