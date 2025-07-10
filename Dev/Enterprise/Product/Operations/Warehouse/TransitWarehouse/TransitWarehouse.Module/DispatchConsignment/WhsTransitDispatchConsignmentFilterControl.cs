using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transit.Module
{
	public partial class WhsTransitDispatchConsignmentFilterControl : ZFilterStripControl<WhsTransitWorkflowFilterStrip>
	{
		// for the designer
		public WhsTransitDispatchConsignmentFilterControl()
			: this(null, null)
		{
		}

		public WhsTransitDispatchConsignmentFilterControl(IBusinessObjectCollection gridCollection, WhsTransitDispatchConsignmentFilterBusinessObject filterBusinessObject)
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
