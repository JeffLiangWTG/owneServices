using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(ChangePriorityActionMethod))]
	public class ChangePriorityActionMethodTest : OperationalActionMethodTest<ChangePriorityActionMethod>
	{
		protected override ChangePriorityActionMethod NewMethod()
		{
			return new ChangePriorityActionMethod();
		}
	}
}
