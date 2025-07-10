using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class PrintFailedEventArgsTest : TestCase
	{
		public void TestPrintFailedEventArgs()
		{
			var e = new PrintFailedEventArgs("Some message");
			AssertEquals("Some message", e.Message);
		}
	}
}
