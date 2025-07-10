using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business;

public class IE429And529MessageProcessor : DMSResponseMessageProcessor
{
	public IE429And529MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override void SetEntryInfos(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		var mrnEntryNumber = GetMRNCusEntryNumber(entryHeader);
		SetEntryNumberExpiryDate(mrnEntryNumber, dataProvider.Controls.FirstOrDefault()?.LimitDate);

		var entryStatus = string.Empty;
		var messageStatus = string.Empty;
		var phaseStatus = string.Empty;
		if (entryHeader.Declaration.IsExport)
		{
			(entryStatus, messageStatus) = DMSResponseMessageHelper.GetReleaseMessageStatusesForExport(dataProvider);
			phaseStatus = CustomsEntryPhaseStatusList.Codes._515;
		}
		else
		{
			(entryStatus, messageStatus) = DMSResponseMessageHelper.GetReleaseMessageStatuses(dataProvider);
		}
		SetEntryStatuses(entryHeader, entryStatus, messageStatus, phaseStatus);

		SetEntryHeaderReleaseDate(entryHeader, dataProvider);
	}

	protected override ZString InterpretMessage(EDIMessage message, IDMSIncomingDataProvider dataProvider) => new NLResponseEDIMessagePrettier().Interpret(dataProvider, message);
}
