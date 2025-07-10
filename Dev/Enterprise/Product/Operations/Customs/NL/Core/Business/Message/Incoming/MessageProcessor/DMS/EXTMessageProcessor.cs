using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business;

public class EXTMessageProcessor : DMSResponseMessageProcessor
{
	public EXTMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString InterpretMessage(EDIMessage message, IDMSIncomingDataProvider dataProvider)
		=> NLEDIMessageInterpreterHelper.FormatMessageContent(Description,
			dataProvider.Declaration?.ExpirationDate,
			NLConstants.StatementTypes.Customs,
			Description,
			Description,
			dataProvider.Statuses.FirstOrDefault()?.EffectiveDateTime);

	string Description => Res.GetString("77F28599-6930-44BA-BFC5-BE37369DD6A5", "External Message");
}
