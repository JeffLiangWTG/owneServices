using CargoWise.EntityFramework;
using Enterprise.Warehouse.Invoicing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDPeriodicInvoicingStorageLinesCollection : ActiveBusinessObjectCollection<CYDYardStorageLines>
	{
		public CYDPeriodicInvoicingStorageLinesCollection(PeriodicInvoicing yardUnit)
			: base(yardUnit.Factory, yardUnit, null, CYDYardStorageLinesSchema.YSL_ET_JobStorage)
		{
		}
	}
}
