using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transit.Module
{
	public partial class WhsItemReceiveASNFilterControl : ZFilterStripControl
	{
		public WhsItemReceiveASNFilterControl()
			: this(null, null)
		{
		}

		public WhsItemReceiveASNFilterControl(IBusinessObjectCollection gridCollection, WhsItemReceiveASNFilterBusinessObject filterBusinessObject)
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
