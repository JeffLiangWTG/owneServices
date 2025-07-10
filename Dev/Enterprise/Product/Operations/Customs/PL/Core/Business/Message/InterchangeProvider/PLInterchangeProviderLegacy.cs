using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.PL.Business;

public class PLInterchangeProviderLegacy : InterchangeProviderBase
{
	public PLInterchangeProviderLegacy(NonDependentEDIMessageCollection messages, LoggingInformation logging) : base(messages)
	{
		logger = Argument.NotNull(logging, nameof(logging));
	}

	protected sealed override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages) => ZString.Empty;

	protected override string GetCollationKey(Messaging.Business.EDIMessage message) => DoNotCollateType;

	protected override string InstructionHowToSetInterchangeSenderID => ZString.Empty;

	protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
	{
		var message = GetOneAndOnlyMessageFromCollection(messages);
		var errorMessage = packer.Pack(message, interchange, logger);
		if (!errorMessage.IsEmpty)
		{
			interchange.Delete();
		}

		if (!interchange.IsDeleted)
		{
			message.EM_EI = interchange.PK;
			message.EM_Status = EDIMessageStatusList.Codes.Sent;
		}
	}

	readonly LoggingInformation logger;

	PLMessagePacker packer => _packer.Value;
	readonly Lazy<PLMessagePacker> _packer = new(() => new PLMessagePacker());

	static BaseEDIMessage GetOneAndOnlyMessageFromCollection(NonDependentEDIMessageCollection messages) => messages.Cast<BaseEDIMessage>().Single();
}
