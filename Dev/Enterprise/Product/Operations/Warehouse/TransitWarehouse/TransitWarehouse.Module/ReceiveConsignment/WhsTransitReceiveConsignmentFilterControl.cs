using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transit.Module
{
	public partial class WhsTransitReceiveConsignmentFilterControl : ZFilterStripControl<WhsTransitWorkflowFilterStrip>
	{
		// for the designer
		public WhsTransitReceiveConsignmentFilterControl()
			: this(null, null)
		{
		}

		public WhsTransitReceiveConsignmentFilterControl(IBusinessObjectCollection gridCollection, WhsTransitReceiveConsignmentFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			this.filterBusinessObject = filterBusinessObject;

			InitializeComponent();
		}

		public override void Find(bool isManualSearch = true)
		{
			if (filterBusinessObject.HasTransitWarehouseInCurrentBranch())
			{
				base.Find(isManualSearch);
			}
		}

		readonly WhsTransitFilterBusinessObject filterBusinessObject;
	}
}
