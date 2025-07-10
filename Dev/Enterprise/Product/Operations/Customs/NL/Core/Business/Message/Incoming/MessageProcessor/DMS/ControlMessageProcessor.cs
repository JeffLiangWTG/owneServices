using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business;

public class ControlMessageProcessor : DMSResponseMessageProcessor
{
	public ControlMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected sealed override string MessageFriendlyNameCore => Res.GetString("26A72C9A-EB56-4719-92AC-B39412E6C9A8", "Control Message");

	protected override void SetEntryInfos(CusEntryHeader entryHeader, IControlIncomingDataProvider dataProvider)
	{
		entryHeader.CH_Status = NLConstants.StatusNew.Error;
	}

	protected override ZString InterpretMessage(EDIMessage message, IControlIncomingDataProvider dataProvider) => new ControlMessageInterpreter().Interpret(dataProvider, message);
}
