using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.NL.Business;

public class MetaDataWrapper : IMetaData
{
	public MetaDataWrapper(JobDeclarationMessageSendingObject messageSendingObject)
	{
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		entryHeader = Argument.NotNull(messageSendingObject.Header, nameof(messageSendingObject.Header));
	}
	protected readonly JobDeclarationMessageSendingObject messageSendingObject;
	protected readonly CusEntryHeader entryHeader;

	public string WCOTypeCode => NLEDIMessage.WCOTypePlaceHolder;

	public ICommunicationMetaData CommunicationMetaData => CommunicationMetaDataCore;

	protected virtual ICommunicationMetaData CommunicationMetaDataCore => new CommunicationMetaDataWrapper(entryHeader);

	public IDeclaration Declaration => new DeclarationWrapper(messageSendingObject);

	protected NLEDIMessage GetSentMessageForComparison(EDIMessageCollection messages, ZString messageSubType)
	{
		return messages.Cast<NLEDIMessage>().OrderBy(x => x.EM_SystemCreateTimeUtc).LastOrDefault(x => x.EM_MessageSubType.EqualsIgnoringCase(messageSubType) && x.EM_Status.EqualsIgnoringCase(EDIMessageStatusList.Codes.Sent));
	}

	protected ZBool HasReleasedMessage(EDIMessageCollection messages, ZString messageSubType, params ZString[] entryStatusArray)
	{
		return messages.OfType<NLEDIMessage>().Any(x => x.EM_Status.EqualsIgnoringCase(EDIMessageStatusList.Codes.Received)
		&& x.EM_MessageSubType.EqualsIgnoringCase(messageSubType)
		&& entryStatusArray.Contains(DMSResponseMessageHelper.GetReleaseMessageStatuses(DMSResponseMessageHelper.CreateDMSIncomingDataProvider(x.EM_MessageText)).EntryStatus));
	}
}
