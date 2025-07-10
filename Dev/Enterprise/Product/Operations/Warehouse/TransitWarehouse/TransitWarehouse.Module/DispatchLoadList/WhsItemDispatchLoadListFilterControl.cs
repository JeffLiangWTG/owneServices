using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transit.Module
{
	public partial class WhsItemDispatchLoadListFilterControl : ZFilterStripControl
	{
		public WhsItemDispatchLoadListFilterControl()
			: this(null, null)
		{
		}

		public WhsItemDispatchLoadListFilterControl(IBusinessObjectCollection gridCollection, WhsItemDispatchLoadListFilterBusinessObject filterBusinessObject)
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
