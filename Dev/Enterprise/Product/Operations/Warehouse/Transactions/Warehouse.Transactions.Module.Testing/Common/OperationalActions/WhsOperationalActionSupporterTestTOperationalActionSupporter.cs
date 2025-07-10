using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public abstract class WhsOperationalActionSupporterTest<TOperationalActionSupporter> : OperationalActionSupporterTest<TOperationalActionSupporter>
		where TOperationalActionSupporter : WhsOperationalActionSupporter
	{
	}
}
