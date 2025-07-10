using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transit.Module
{
	public partial class WhsItemReceiveTransportationUnitFilterControl : ZFilterStripControl<WhsTransitWorkflowFilterStrip>
	{
		public WhsItemReceiveTransportationUnitFilterControl()
			: this(null, null)
		{
		}

		public WhsItemReceiveTransportationUnitFilterControl(IBusinessObjectCollection gridCollection, WhsItemReceiveTransportationUnitFilterBusinessObject filterBusinessObject)
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
