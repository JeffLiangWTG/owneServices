using CargoWise.RefDbRepo.ILReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.Services
{
	[TestFixture]
	public class XTMessagingConfigTest
	{
		[Test]
		public void TestApplicationSettings()
		{
			XTMessagingConfig.ReLoad();
			Assert.AreEqual(15, XTMessagingConfig.Instance.XTIdleConnectionKeepAliveInSecondsValue);
			Assert.AreEqual(15, XTMessagingConfig.Instance.XTIdleConnectionRetryPauseInSecondsValue);
			Assert.AreEqual(30, XTMessagingConfig.Instance.InterchangeCountPerBatchOnReceivingValue);
			Assert.AreEqual(8192, XTMessagingConfig.Instance.XTServerMessageChunkSizeWhenSendingValue);
		}
	}
}
