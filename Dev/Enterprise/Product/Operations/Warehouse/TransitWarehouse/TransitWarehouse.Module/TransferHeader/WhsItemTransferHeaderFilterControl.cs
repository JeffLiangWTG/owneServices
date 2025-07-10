using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transit.Module
{
	public partial class WhsItemTransferHeaderFilterControl : ZFilterStripControl
	{
		public WhsItemTransferHeaderFilterControl() : this(null, null)
		{
		}

		public WhsItemTransferHeaderFilterControl(IBusinessObjectCollection gridCollection, WhsItemTransferHeaderFilterBusinessObject filterBusinessObject)
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
