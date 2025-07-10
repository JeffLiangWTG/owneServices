using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class BIRDEntrySummaryQueryResponseMessageBuilder : MessageBuilder<BIRDInputBlockControlGenerator>
	{
		public BIRDEntrySummaryQueryResponseMessageBuilder(ICusEntryHeaderMessageAttachee entryHeader)
			: base(entryHeader)
		{
		}

		protected override string ApplicationIdentifier
		{
			get { return ApplicationIdentifierCodeList.Codes.BIRDTransaction; }
		}

		protected override BIRDInputBlockControlGenerator GetNewInputBlockControlGenerator()
		{
			JobDeclaration declaration = Entry.Factory.Load<JobDeclaration>(Entry.DeclarationPK);

			ZString refNo = declaration.US_BRDRefNo.IsEmpty ? declaration.JE_DeclarationReference : declaration.US_BRDRefNo;
			BIRDInputBlockControlGenerator result = new BIRDInputBlockControlGenerator(Entry, BIRDApplicationCodeList.Codes.EntrySummaryQueryOutput, refNo.Right(20));

			return result;
		}

		protected override void UpdateMessageBlocks(BIRDInputBlockControlGenerator block)
		{
			MQEDIMessage lastMessage = (MQEDIMessage)Entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse);

			if (lastMessage != null)
			{
				block.AddMessageBlocks(lastMessage.MessageBlock.MessageBlocks);
			}
			else
			{
				ErrorReporter.ReportOnce("BIRDEntrySummaryQueryResponseMessageBuilder", "This builder is triggered while there is no recent message to build from.");
			}
		}

		protected override void SetMessageSubType(MQEDIMessage message)
		{
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BIRDEntrySummaryQueryOutput;

			JobDeclaration declaration = Entry.Factory.Load<JobDeclaration>(Entry.DeclarationPK);
			message.EM_Status = declaration.HasBIRDCommunicationMode() ? MQEDIMessage.Status.Pending : MQEDIMessage.Status.Acknowledged;
		}

		ICusEntryHeaderMessageAttachee Entry
		{
			get { return (ICusEntryHeaderMessageAttachee)messageAttachee; }
		}
	}
}
