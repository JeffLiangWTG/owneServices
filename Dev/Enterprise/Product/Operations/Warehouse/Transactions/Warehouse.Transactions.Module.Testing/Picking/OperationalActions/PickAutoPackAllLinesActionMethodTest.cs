using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(PickAutoPackAllLinesActionMethod))]
	public class PickAutoPackAllLinesActionMethodTest : OperationalActionMethodTest<PickAutoPackAllLinesActionMethod>
	{
		protected override PickAutoPackAllLinesActionMethod NewMethod()
		{
			return new PickAutoPackAllLinesActionMethod();
		}
	}
}
