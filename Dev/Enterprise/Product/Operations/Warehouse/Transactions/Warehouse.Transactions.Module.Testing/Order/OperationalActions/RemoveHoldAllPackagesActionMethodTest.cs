using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(RemoveHoldAllPackagesActionMethod))]
	public class RemoveHoldAllPackagesActionMethodTest : OperationalActionMethodTest<RemoveHoldAllPackagesActionMethod>
	{
		protected override RemoveHoldAllPackagesActionMethod NewMethod()
		{
			return new RemoveHoldAllPackagesActionMethod();
		}
	}
}
