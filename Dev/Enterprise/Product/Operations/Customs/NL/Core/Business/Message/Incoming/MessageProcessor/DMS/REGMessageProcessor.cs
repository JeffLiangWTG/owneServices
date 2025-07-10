using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business;

public class REGMessageProcessor : DMSResponseMessageProcessor
{
	public REGMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override void SetEntryInfos(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		SetEntryStatuses(entryHeader, dataProvider, NLConstants.Status.Received);

		var mrnEntryNumber = GetMRNCusEntryNumber(entryHeader);
		SetEntryNumberExpiryDate(mrnEntryNumber, dataProvider.Declaration?.AcceptanceDate?.AddDays(30));
	}

	protected override ZString InterpretMessage(EDIMessage message, IDMSIncomingDataProvider dataProvider) => new REGMessageInterpreter().Interpret(dataProvider, message);
}
