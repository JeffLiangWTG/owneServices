using System;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Business.MessagingProcess;

namespace Enterprise.Customs.TR.Business
{
	public static class CusEntryHeaderCustomsMessenger
	{
		public static TRCustomsMessenger New(CusEntryHeader entryHeader, ZString messageType, TRMessageSigner signer = null)
		{
			switch (messageType)
			{
				case TRMessageTypes.Codes.DKO:
				case TRMessageTypes.Codes.DTE:
					return new TRCustomsMessenger(entryHeader, CreateMessageSender(entryHeader, messageType), signer);
				case TRMessageTypes.Codes.EUT:
					return new TRCustomsMessenger(entryHeader, CreateMessageSenderForExportUnion(entryHeader), signer);
				default:
					throw new NotSupportedException($"Unsupported message type: {messageType}");
			}
		}

		static TRBaseMessageGenerator<TRImportExportMessage> CreateMessageSender(CusEntryHeader entryHeader, ZString messageType)
		{
			switch (messageType)
			{
				case TRMessageTypes.Codes.DKO:
					return new CusEntryHeaderControlMessageGenerator(new CusEntryHeaderMessageSender(entryHeader));
				case TRMessageTypes.Codes.DTE:
					return new CusEntryHeaderRegistryMessageGenerator(new CusEntryHeaderMessageSender(entryHeader));
				default:
					throw new NotSupportedException($"Unsupported message type: {messageType}");
			}
		}

		static TRBaseMessageGenerator<ExportUnionMessage> CreateMessageSenderForExportUnion(CusEntryHeader entryHeader)
		{
			return new CusEntryHeaderExportUnionMessageGenerator(new ExportUnionMessageSender(entryHeader));
		}
	}
}
