using CargoWise.EntityFramework;
using Enterprise.Packing.Module;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class WhsHandlingUnitFilterControl : HandlingUnitFilterControl
	{
		public WhsHandlingUnitFilterControl()
			: this(null, null)
		{
		}

		public WhsHandlingUnitFilterControl(IBusinessObjectCollection gridCollection, WhsHandlingUnitFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
		}
	}
}
