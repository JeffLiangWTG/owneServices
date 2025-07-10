using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(FixedMessageNumberStrategy))]
	sealed class FixedMessageNumberStrategyTest : TestCase
	{
		public void TestGetMessageReferenceNumber()
		{
			IMessageNumberStrategy strategy = new FixedMessageNumberStrategy();
			AssertEquals("Number", "1", strategy.GetMessageReferenceNumber());
		}
	}
}
