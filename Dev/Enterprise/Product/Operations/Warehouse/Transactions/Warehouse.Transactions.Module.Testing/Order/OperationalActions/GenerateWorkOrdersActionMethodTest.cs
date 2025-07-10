using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(GenerateWorkOrdersActionMethod))]
	public class GenerateWorkOrdersActionMethodTest : OperationalActionMethodTest<GenerateWorkOrdersActionMethod>
	{
		protected override GenerateWorkOrdersActionMethod NewMethod()
		{
			return new GenerateWorkOrdersActionMethod();
		}
	}
}
