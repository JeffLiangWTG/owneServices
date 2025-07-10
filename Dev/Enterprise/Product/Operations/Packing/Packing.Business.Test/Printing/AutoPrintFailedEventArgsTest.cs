using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	public class AutoPrintFailedEventArgsTest : TestCase
	{
		public void TestAutoPrintFailedEventArgs()
		{
			var e = new AutoPrintFailedEventArgs("hello", true);
			AssertEquals("hello", e.Message);
			AssertEquals(true, e.CanContinueWithManualPrint);
		}
	}
}
