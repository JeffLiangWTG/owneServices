using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(ReleaseOrdersActionMethod))]
	public class ReleaseOrdersActionMethodTest : OperationalActionMethodTest<ReleaseOrdersActionMethod>
	{
		protected override ReleaseOrdersActionMethod NewMethod()
		{
			return new ReleaseOrdersActionMethod();
		}
	}
}
