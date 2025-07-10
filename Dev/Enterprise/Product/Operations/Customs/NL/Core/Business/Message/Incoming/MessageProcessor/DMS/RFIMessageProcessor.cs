using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business;

public class RFIMessageProcessor : DMSResponseMessageProcessor
{
	public RFIMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override void SetEntryInfos(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		if (entryHeader.CH_PhaseStatus != CustomsEntryPhaseStatusList.Codes._513)
		{
			SetEntryStatuses(entryHeader, NLConstants.EntryStatusNew.RequestForInformation, NLConstants.StatusNew.Accepted);
		}
	}

	protected override ZString InterpretMessage(EDIMessage message, IDMSIncomingDataProvider dataProvider) => new RFIMessageInterpreter().Interpret(dataProvider, message);
}
