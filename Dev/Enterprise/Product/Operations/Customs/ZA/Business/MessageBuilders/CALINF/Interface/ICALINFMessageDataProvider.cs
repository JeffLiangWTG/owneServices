using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CALINF
{
	public interface ICALINFMessageDataProvider : IEDIFACTMessageAttachee
		, IBGM_BeginningOfMessage
		, IDTM_DocumentDateTime
		, IRFF_DocumentToBeAmended
		, INAD_MessageSender
	{
		ICALINFTransportInformation Transport { get; }
	}

	public interface IBGM_BeginningOfMessage
	{
		ZString CALINFMessageType { get; }
	}

	public interface IDTM_DocumentDateTime
	{
		ZDateTime DocumentIssueDateTime { get; }
	}

	public interface IRFF_DocumentToBeAmended
	{
		ZString DocumentToBeAmended { get; }
	}

	public interface INAD_MessageSender
	{
		ZString MessageSender { get; }
	}
}
