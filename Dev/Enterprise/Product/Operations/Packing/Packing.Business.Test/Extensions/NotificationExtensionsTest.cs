using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	public class NotificationExtensionsTest : TestCase
	{
		public void TestAddInformation()
		{
			var notify = new NotifyForPacking();

			notify.AddInformation("moo");
			AssertEquals(true, notify.LastEvent.Type is InformationNotification);
			AssertEquals("moo", notify.LastEvent.Message);
			AssertEquals(false, notify.LastEvent.Type.IsFatal);
			AssertEquals(0, notify.LastEvent.Type.Severity);
		}
	}
}
