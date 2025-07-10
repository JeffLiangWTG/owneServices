using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.AWB.Messaging.Testing
{
	public abstract class CargoIMPTest : TestCaseWithFactory
	{
		protected abstract string Expected { get; }
		protected abstract CargoIMP CargoIMPMessage { get; }

		protected void AssertMessageCorrect()
		{
			string actual = CargoIMPMessage.ToString(); // This needs to be evaluated before the Expected is evaluated.
			AssertMultilineEquals("Check The Strings", Expected, actual, '\r');
		}
	}
}
