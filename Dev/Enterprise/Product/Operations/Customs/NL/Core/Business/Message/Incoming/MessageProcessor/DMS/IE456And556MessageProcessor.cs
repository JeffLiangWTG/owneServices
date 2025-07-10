using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business;

public class IE456And556MessageProcessor : DMSResponseMessageProcessor
{
	public IE456And556MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override void SetEntryInfos(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		if (dataProvider.WCOTypeCode.EqualIgnoringOrder(WCoTypeCodes.ExportRejection))
		{
			switch (entryHeader.CH_PhaseStatus)
			{
				case CustomsEntryPhaseStatusList.Codes._511:
				case CustomsEntryPhaseStatusList.Codes._513 when entryHeader.CH_Status == StatusNew.SentToCustoms:
				case CustomsEntryPhaseStatusList.Codes._514 when entryHeader.CH_Status == StatusNew.SentToCustoms:
				case CustomsEntryPhaseStatusList.Codes._515:
				case CustomsEntryPhaseStatusList.Codes._583:
				case CustomsEntryPhaseStatusList.Codes.CRE:
				case CustomsEntryPhaseStatusList.Codes.SUP:
					SetEntryStatuses(entryHeader, status: StatusNew.Invalid);
					break;
				case CustomsEntryPhaseStatusList.Codes._513 when entryHeader.CH_Status == StatusNew.Accepted:
				case CustomsEntryPhaseStatusList.Codes._514 when entryHeader.CH_Status == StatusNew.Accepted:
					break;
				case CustomsEntryPhaseStatusList.Codes.REG:
					SetEntryStatuses(entryHeader, EntryStatusNew.NoRelease, StatusNew.Cancelled);
					break;
				default:
					SetEntryStatuses(entryHeader, dataProvider, Status.Rejection);
					break;
			}
		}
		else
		{
			SetEntryStatuses(entryHeader, dataProvider, Status.Rejection);
		}

		var mrnEntryNumber = GetMRNCusEntryNumber(entryHeader);
		SetEntryNumberIssueDateAndNum(mrnEntryNumber, dataProvider);
	}

	protected override ZString InterpretMessage(EDIMessage message, IDMSIncomingDataProvider dataProvider) => new IE456And556MessageInterpreter().Interpret(dataProvider, message);
}
