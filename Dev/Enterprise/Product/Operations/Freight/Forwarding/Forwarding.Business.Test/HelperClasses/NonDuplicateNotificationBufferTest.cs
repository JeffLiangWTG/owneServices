using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class NonDuplicateNotificationBufferTest : TestCaseWithFactory
	{
		public void TestDuplicateMessages()
		{
			NotificationBuffer innerBuffer = new NotificationBuffer();
			NonDuplicateNotificationBuffer buffer = new NonDuplicateNotificationBuffer(innerBuffer);

			buffer.Notify(null);
			AssertEquals("", buffer.AsString);
			AssertEquals("", innerBuffer.AsString);

			buffer.AddWarning("message");
			AssertEquals("message\r\n", buffer.AsString);
			AssertEquals("message\r\n", innerBuffer.AsString);

			buffer.AddError("message");
			AssertEquals("message\r\nmessage\r\n", buffer.AsString);
			AssertEquals("message\r\nmessage\r\n", innerBuffer.AsString);

			buffer.AddWarning("message");
			AssertEquals("message\r\nmessage\r\n", buffer.AsString);
			AssertEquals("message\r\nmessage\r\n", innerBuffer.AsString);

			buffer.AddWarning("message1");
			AssertEquals("message\r\nmessage\r\nmessage1\r\n", buffer.AsString);
			AssertEquals("message\r\nmessage\r\nmessage1\r\n", innerBuffer.AsString);

			buffer.Notify(new WarningNotification(WarningType.MaxLengthExceeded, "message"));
			AssertEquals("message\r\nmessage\r\nmessage1\r\nWarning: Maximum length of this field has been exceeded (message)\r\n", buffer.AsString);
			AssertEquals("message\r\nmessage\r\nmessage1\r\nWarning: Maximum length of this field has been exceeded (message)\r\n", innerBuffer.AsString);

			buffer.Notify(new WarningNotification(WarningType.MaxLengthExceeded, "message"));
			AssertEquals("message\r\nmessage\r\nmessage1\r\nWarning: Maximum length of this field has been exceeded (message)\r\n", buffer.AsString);
			AssertEquals("message\r\nmessage\r\nmessage1\r\nWarning: Maximum length of this field has been exceeded (message)\r\n", innerBuffer.AsString);

			buffer.Notify(new ErrorNotification(ErrorType.Error, "message"));
			AssertEquals("message\r\nmessage\r\nmessage1\r\nWarning: Maximum length of this field has been exceeded (message)\r\nError: message\r\n", buffer.AsString);
			AssertEquals("message\r\nmessage\r\nmessage1\r\nWarning: Maximum length of this field has been exceeded (message)\r\nError: message\r\n", innerBuffer.AsString);
		}
	}
}
