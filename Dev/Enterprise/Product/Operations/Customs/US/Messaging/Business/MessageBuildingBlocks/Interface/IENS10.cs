using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IENSDeferredTaxIndicator
	{
		ZString DeferredTaxIndicator { get; }
	}

	public interface IENS10
	{
		ZString EntryType { get; }
	}
}
