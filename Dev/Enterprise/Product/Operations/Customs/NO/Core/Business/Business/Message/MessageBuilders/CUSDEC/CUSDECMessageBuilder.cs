using CargoWise.Common;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Edifact.V902.Messages.CUSDEC;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NO.Business;

sealed class CUSDECMessageBuilder : EDIFACTMessageBuilder<IEDIMessageCollectionProvider, CUSDECMessage, CUSDECEDIMessage>, ICustomsMessageGenerator
{
	public CUSDECMessageBuilder(ICUSDECMessageDataProvider provider, MessageSubTypes messageSubType)
		: this(provider, provider?.AsMessageCollectionProvider(), messageSubType)
	{ }

	CUSDECMessageBuilder(ICUSDECMessageDataProvider dataProvider, IEDIMessageCollectionProvider collectionProvider, MessageSubTypes messageSubType)
		: base(collectionProvider, messageSubType, new NOCharacterSet())
	{
		cusDecMessageDataProvider = Argument.NotNull(dataProvider, nameof(dataProvider));
		Argument.NotNull(collectionProvider, nameof(collectionProvider));
	}

	public EDIMessage GenerateMessage() => PopulateMessagesReturningResult();

	protected override void PopulateEdifactMessage()
	{
		CUSDECMessageSegmentBuilder.PopulateCUSDECMessage(edifactMessage, cusDecMessageDataProvider);
	}

	readonly ICUSDECMessageDataProvider cusDecMessageDataProvider;
}
