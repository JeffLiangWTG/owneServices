using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business;

public class IE438MessageProcessor : DMSResponseMessageProcessor
{
	public IE438MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString InterpretMessage(EDIMessage message, IDMSIncomingDataProvider dataProvider)
		=> NLEDIMessageInterpreterHelper.FormatMessageContent(NLEDIMessageInterpreterHelper.ResStrings.CustomsReminder,
			dataProvider.Declaration?.ExpirationDate,
			NLConstants.StatementTypes.Customs,
			Res.GetString("03F0E811-5BDA-4075-979A-0C1E539D454E", "Information request"),
			Res.GetString("E5A28C24-457A-4E38-83AB-158A42CA200D", "Outstanding request for information (RFI)"),
			dataProvider.Statuses.FirstOrDefault()?.EffectiveDateTime);
}
