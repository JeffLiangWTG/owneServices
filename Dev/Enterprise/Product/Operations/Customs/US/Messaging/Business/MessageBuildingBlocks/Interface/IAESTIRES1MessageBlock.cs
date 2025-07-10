using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES
{
	public interface IAESTIRES1MessageBlock
	{
		ZString ResponseCode { get; }
		ZString FinalDispositionIndicator { get; }
		ZString SeverityIndicator { get; }
		ZString NarrativeText { get; }
		ZString AESInternalTransactionNumberITN { get; }
		ZString ReasonCode { get; }
	}
}
