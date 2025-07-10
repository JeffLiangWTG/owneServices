using System.Collections.Generic;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business
{
	public class RequestToExtendTIBMessageBuilder
	{
		public RequestToExtendTIBMessageBuilder(ICusEntryHeaderMessageAttachee entry, ImportMessageSendingMessageType messageSendingMessageType)
		{
			this.entry = entry;
			this.messageSendingMessageType = messageSendingMessageType;
		}
		readonly ICusEntryHeaderMessageAttachee entry;
		readonly ImportMessageSendingMessageType messageSendingMessageType;

		public void GenerateMessages()
		{
			BlockControlGenerator block = null;

			var aCEInputBlockControlGenerator = new ACEInputBlockControlGenerator(entry);
			aCEInputBlockControlGenerator.B.SetupBlockBDetails(entry);
			block = aCEInputBlockControlGenerator;

			block.B.ApplicationIdentifier = ACEApplicationIdentifierCodeList.Codes.TIBExtensionOrClosure;
			block.AddMessageBlock(GenerateXA());

			var message = block.CreateMessage<MQEDIMessage>(entry.Factory);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.TemporaryImportationBondRequestToExtend;
			entry.Messages.Add(message);
		}

		internal IEnumerable<MessageBlock> Build()
		{
			yield return GenerateXA();
		}

		MessageBlock GenerateXA()
		{
			var xaMessage = new ATIBXA();
			xaMessage.DistrictPortOfEntrySummary = entry.DistrictPortOfEntry;
			xaMessage.BrokerNumberOrEntryFilerCode = entry.EntryFilerCode;
			xaMessage.EntryNumber = ((ICusEntryHeader)entry).EntryNumber;
			xaMessage.ExtensionClosureCode = messageSendingMessageType == ImportMessageSendingMessageType.ExtendTIB ? "1"
											: messageSendingMessageType == ImportMessageSendingMessageType.ClosureTIB ? "2"
											: string.Empty;
			return xaMessage;
		}
	}
}
