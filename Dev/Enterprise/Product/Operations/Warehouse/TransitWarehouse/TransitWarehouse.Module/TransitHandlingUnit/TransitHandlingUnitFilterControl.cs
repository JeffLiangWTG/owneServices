using CargoWise.EntityFramework;
using Enterprise.Packing.Module;

namespace Enterprise.Warehouse.Transit.Module
{
	public partial class TransitHandlingUnitFilterControl : HandlingUnitFilterControl
	{
		// for the designer
		public TransitHandlingUnitFilterControl()
			: this(null, null)
		{
		}

		public TransitHandlingUnitFilterControl(IBusinessObjectCollection gridCollection, TransitHandlingUnitFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			this.filterBusinessObject = filterBusinessObject;
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
