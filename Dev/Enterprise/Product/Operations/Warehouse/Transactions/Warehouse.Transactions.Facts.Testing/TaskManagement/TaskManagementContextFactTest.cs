using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	class TaskManagementContextFactTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var contextFact = new TaskManagementContextFact(42);
			AssertEquals(nameof(contextFact.MaxNumberOfLinesFallBack), 42, contextFact.MaxNumberOfLinesFallBack);
		}
	}
}
