using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transit.Module
{
	public partial class WhsItemDispatchTransportationUnitFilterControl : ZFilterStripControl<WhsTransitWorkflowFilterStrip>
	{
		// for the designer
		public WhsItemDispatchTransportationUnitFilterControl()
			: this(null, null)
		{
		}

		public WhsItemDispatchTransportationUnitFilterControl(IBusinessObjectCollection gridCollection, WhsItemDispatchTransportationUnitFilterBusinessObject filterBusinessObject)
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
