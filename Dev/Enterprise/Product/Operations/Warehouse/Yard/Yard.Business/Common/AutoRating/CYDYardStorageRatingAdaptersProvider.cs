using Enterprise.Warehouse.Invoicing.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDYardStorageRatingAdaptersProvider(PeriodicInvoicing periodicInvoicing, CYDPeriodicInvoicingYardUnits unitsForRating)
		: CYDYardUnitsRatingAdaptersProvider<PeriodicInvoicing, CYDPeriodicInvoicingYardUnits>(periodicInvoicing, unitsForRating)
	{
		protected override CYDYardUnitsRatingAdapter<PeriodicInvoicing, CYDPeriodicInvoicingYardUnits> GetRatingAdpater(PeriodicInvoicing periodicInvoicing, CYDPeriodicInvoicingYardUnits unitsForRating, CYDYardUnitState yardUnit)
		{
			return new CYDYardStorageRatingAdapter(periodicInvoicing, unitsForRating, yardUnit);
		}
	}
}
