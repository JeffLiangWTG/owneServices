using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class MessageWithSeverityTest : TestCase
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var messageWithSeverityError = new MessageWithSeverity(MessageWithSeverity.MessageTypes.Error, "test1");
			AssertEquals(MessageWithSeverity.MessageTypes.Error, messageWithSeverityError.MessageType);
			AssertEquals("test1", messageWithSeverityError.Message);
			AssertEquals(false, messageWithSeverityError.IsNone);

			var messageWithSeverityWarning = new MessageWithSeverity(MessageWithSeverity.MessageTypes.Warning, "test2");
			AssertEquals(MessageWithSeverity.MessageTypes.Warning, messageWithSeverityWarning.MessageType);
			AssertEquals("test2", messageWithSeverityWarning.Message);
			AssertEquals(false, messageWithSeverityWarning.IsNone);

			var messageWithSeverityNone = new MessageWithSeverity(MessageWithSeverity.MessageTypes.None, "test3");
			AssertEquals(MessageWithSeverity.MessageTypes.None, messageWithSeverityNone.MessageType);
			AssertEquals("test3", messageWithSeverityNone.Message);
			AssertEquals(true, messageWithSeverityNone.IsNone);
		}

		#endregion
	}
}
