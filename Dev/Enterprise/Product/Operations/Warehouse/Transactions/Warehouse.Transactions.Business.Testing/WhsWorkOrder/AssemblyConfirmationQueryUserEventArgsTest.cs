using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class AssemblyConfirmationQueryUserEventArgsTest : TestCase
	{
		public void TestConstructor()
		{
			var args1 = new AssemblyConfirmationQueryUserEventArgs(defaultResponse: false);
			AssertEquals("", args1.Caption);
			AssertEquals("", args1.Message);
			AssertEquals(false, args1.Response);

			var args2 = new AssemblyConfirmationQueryUserEventArgs(defaultResponse: true);
			AssertEquals("", args2.Caption);
			AssertEquals("", args2.Message);
			AssertEquals(true, args2.Response);
		}
	}
}
