using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(FinalisePicksActionMethod))]
	public class FinalisePicksActionMethodTest : OperationalActionMethodTest<FinalisePicksActionMethod>
	{
		protected override FinalisePicksActionMethod NewMethod()
		{
			return new FinalisePicksActionMethod();
		}
	}
}
