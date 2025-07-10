using System;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business;

public class IE599MessageProcessor : DMSResponseMessageProcessor
{
	public IE599MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override void SetEntryInfos(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		var control = dataProvider.Controls.FirstOrDefault();
		if(control?.ControlResultExitTime is DateTime exitTime)
		{
			entryHeader.CH_ExitDate = exitTime;
			switch (entryHeader.CH_PhaseStatus)
			{
				case CustomsEntryPhaseStatusList.Codes._515:
					SetEntryStatuses(entryHeader, EntryStatusNew.GoodsExitedEU, StatusNew.Accepted);
					break;
				case CustomsEntryPhaseStatusList.Codes._583:
					SetEntryStatuses(entryHeader, EntryStatusNew.GoodsExitedEU, StatusNew.Accepted, CustomsEntryPhaseStatusList.Codes._515);
					break;
				default:
					SetEntryStatuses(entryHeader, dataProvider, NLConstants.Status.ExitConfirm);
					break;
			}
		}
	}

	protected override ZString InterpretMessage(EDIMessage message, IDMSIncomingDataProvider dataProvider) => new IE599MessageInterpreter().Interpret(dataProvider, message);
}
