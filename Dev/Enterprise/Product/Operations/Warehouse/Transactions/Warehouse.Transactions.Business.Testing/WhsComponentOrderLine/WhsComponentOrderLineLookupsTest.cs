namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class WhsComponentOrderLineLookupsTest<TPickableDocket, TPickableDocketLine> : WhsPickableDocketLineLookupsTest<TPickableDocket, TPickableDocketLine>
		where TPickableDocket : WhsComponentOrder
		where TPickableDocketLine : WhsComponentOrderLine
	{
	}
}
