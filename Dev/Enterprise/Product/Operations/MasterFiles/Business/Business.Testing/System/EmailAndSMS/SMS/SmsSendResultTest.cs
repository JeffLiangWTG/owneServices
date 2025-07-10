using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SmsSendResultTest : TestCase
	{
		public void TestConstructor()
		{
			SmsSendResult sendResult = new SmsSendResult(true, "message");
			AssertEquals(true, sendResult.Success);
			AssertEquals("message", sendResult.Message);
		}
	}
}
