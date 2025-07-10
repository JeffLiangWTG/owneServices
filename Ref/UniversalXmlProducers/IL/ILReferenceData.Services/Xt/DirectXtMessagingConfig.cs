using CargoWise.xTMessaging.Integration;

namespace CargoWise.RefDbRepo.ILReferenceData.Services
{
	public sealed class DirectXtMessagingConfig : IDirectxTMessagingConfig
	{
		public double XTIdleConnectionKeepAliveInSecondsValue => XTMessagingConfig.Instance.XTIdleConnectionKeepAliveInSecondsValue;
		public double XTIdleConnectionRetryPauseInSecondsValue => XTMessagingConfig.Instance.XTIdleConnectionRetryPauseInSecondsValue;
		public int InterchangeCountPerBatchOnReceivingValue => XTMessagingConfig.Instance.InterchangeCountPerBatchOnReceivingValue;
		public int XTServerMessageChunkSizeWhenSendingValue => XTMessagingConfig.Instance.XTServerMessageChunkSizeWhenSendingValue;
	}
}
