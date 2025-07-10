using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business;

public class IE451MessageProcessor : DMSResponseMessageProcessor
{
	public IE451MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override void SetEntryInfos(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		SetEntryStatuses(entryHeader, dataProvider, NLConstants.Status.NoRelease_NRE);

		var mrnEntryNumber = GetMRNCusEntryNumber(entryHeader);
		SetEntryNumberExpiryDate(mrnEntryNumber, dataProvider.Controls.FirstOrDefault()?.LimitDate);
	}

	protected override ZString InterpretMessage(EDIMessage message, IDMSIncomingDataProvider dataProvider) => new NLResponseEDIMessagePrettier().Interpret(dataProvider, message);
}
