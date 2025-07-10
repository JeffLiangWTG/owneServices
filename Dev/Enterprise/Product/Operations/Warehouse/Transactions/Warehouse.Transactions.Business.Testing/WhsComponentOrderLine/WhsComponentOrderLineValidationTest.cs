namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class WhsComponentOrderLineValidationTest<TDocketLine, TDocket> : WhsPickableDocketLineValidationTest<TDocketLine, TDocket>
		where TDocketLine : WhsComponentOrderLine
		where TDocket : WhsComponentOrder
	{
	}
}
