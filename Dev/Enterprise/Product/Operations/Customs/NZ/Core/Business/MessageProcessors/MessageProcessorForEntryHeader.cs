using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MessageProcessors.ECIWriteOff;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NZ.Business.MessageProcessors
{
	public abstract class MessageProcessorForEntryHeader : MessageProcessor
	{
		protected MessageProcessorForEntryHeader(LoggingInformation logger, string messageFriendlyName)
			: base(logger, messageFriendlyName)
		{
		}

		protected override void SetupPropertiesForMessageProcessing(NZCMessage message)
		{
			entryHeader = message.EntryHeader;
			base.SetupPropertiesForMessageProcessing(message);
		}

		protected CusEntryHeader entryHeader;
		internal ECIMessageParser Parser;

		protected override void SendSuccessfullyProcessedResultEmail(EmailDef email)
		{
			if (entryHeader.LastCustomsStatusIsImpediment)
			{
				SendImpedimentReport(entryHeader, email);
			}
			else
			{
				SendAcknowledgementReport(entryHeader, email);
			}
		}

		protected override void SendProcessingFailureResultEmail(EmailDef email)
		{
			SendErrorReport(entryHeader, email);
		}

		internal override string GetMasterBill()
		{
			return entryHeader.Declaration.FormattedMasterBill;
		}

		internal override NZCMessage GetLastOutgoingMessage()
		{
			return (NZCMessage)entryHeader.Messages.LastOutgoingMessage;
		}

		public override ZString EntryNumber
		{
			get { return entryHeader.EntryNumber; }
			set { entryHeader.EntryNumber = value; }
		}

		public override ZString EntryStatus
		{
			get { return entryHeader.CH_EntryStatus; }
			set { entryHeader.CH_EntryStatus = value; }
		}

		public override ZString CustomsDeliveryInstructions
		{
			get { return entryHeader.CH_CustomsDeliveryInstructions; }
			set { entryHeader.CH_CustomsDeliveryInstructions = value; }
		}
	}
}
