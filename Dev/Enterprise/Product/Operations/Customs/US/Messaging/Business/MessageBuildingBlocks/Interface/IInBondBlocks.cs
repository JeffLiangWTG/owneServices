using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IINBQP10
	{
		ZString USPortOfDestination { get; }
	}

	public interface IINBQP20
	{
	}

	public interface IINBQP30
	{
		ZString MasterBillNumber { get; }
		ZString SequenceNumber { get; }
	}

	public interface IINBQT95 : IStatusesAndErrors
	{
		ZString NarrativeMessageTypeCode { get; }
	}

	public interface IINBWT95 : IStatusesAndErrors
	{
		ZString NarrativeMessageTypeCode { get; }
	}

	public interface IINBBN01
	{
		ZString HeaderIdentifier { get; }
		ZString HeaderIdentifierKey { get; }
	}

	public interface IINBBN02
	{
		ZInt FDALine { get; }
		ZInt CBPLine { get; }
		ZString PriorNoticeLineRejectCode { get; }
		ZString PriorNoticeConfirmationNumber { get; }
		ZDate PriorNoticeClockStartDate { get; }
		ZString PriorNoticeClockStartTime { get; }
	}

	public interface IINBFD01BTAPriorNotice
	{
		ZInt FDALineNumber { get; }
	}

	public interface IINBWP10
	{
		ZString InbondNumber { get; }
		ZString ActionCode { get; }
	}

	public interface IINBWP20
	{
		ZString BondedCarrierID { get; }
		ZString InbondCarrierCode { get; }
		ZString PortOfArrival { get; }
	}
}
