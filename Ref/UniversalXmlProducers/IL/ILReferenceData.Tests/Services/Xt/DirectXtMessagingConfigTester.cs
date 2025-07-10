using CargoWise.RefDbRepo.ILReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.Services
{
	[TestFixture]
	sealed class DirectXtMessagingConfigTester
	{
		[Test]
		public void TestXTIdleConnectionKeepAliveInSecondsValue()
		{
			Assert.AreEqual(15, new DirectXtMessagingConfig().XTIdleConnectionKeepAliveInSecondsValue);
		}

		[Test]
		public void TestXTIdleConnectionRetryPauseInSecondsValue()
		{
			Assert.AreEqual(15, new DirectXtMessagingConfig().XTIdleConnectionRetryPauseInSecondsValue);
		}

		[Test]
		public void TestInterchangeCountPerBatchOnReceivingValue()
		{
			Assert.AreEqual(30, new DirectXtMessagingConfig().InterchangeCountPerBatchOnReceivingValue);
		}

		[Test]
		public void TestXTServerMessageChunkSizeWhenSendingValue()
		{
			Assert.AreEqual(8192, new DirectXtMessagingConfig().XTServerMessageChunkSizeWhenSendingValue);
		}
	}
}
