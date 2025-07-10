using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.BIRD;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class BIRDEntrySummaryQueryMessageBuilder : MessageBuilder<BIRDInputBlockControlGenerator>
	{
		public BIRDEntrySummaryQueryMessageBuilder(ICusEntryHeaderMessageAttachee entryHeader)
			: base(entryHeader)
		{
		}

		protected override string ApplicationIdentifier
		{
			get { return ApplicationIdentifierCodeList.Codes.BIRDEntrySummaryQuery; }
		}

		protected override BIRDInputBlockControlGenerator GetNewInputBlockControlGenerator()
		{
			JobDeclaration declaration = Entry.Factory.Load<JobDeclaration>(Entry.DeclarationPK);

			ZString refNo = declaration.US_BRDRefNo.IsEmpty ? declaration.JE_DeclarationReference : declaration.US_BRDRefNo;
			BIRDInputBlockControlGenerator result = new BIRDInputBlockControlGenerator(Entry, BIRDApplicationCodeList.Codes.EntrySummaryQueryInput, refNo.Right(20));

			return result;
		}

		protected override void UpdateMessageBlocks(BIRDInputBlockControlGenerator block)
		{
			BRDJ1 ji = new BRDJ1();
			ji.Filer = Entry.EntryFilerCode;
			ji.EntryNumber = ((ICusEntryHeader)Entry).EntryNumber;
			ji.CollectionBillCode = CollectionBillInformationCodesList.Codes._3;

			block.AddMessageBlock(ji);
		}

		protected override void SetMessageSubType(MQEDIMessage message)
		{
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BIRDEntrySummaryQueryInput;

			JobDeclaration declaration = Entry.Factory.Load<JobDeclaration>(Entry.DeclarationPK);
			message.EM_Status = declaration.HasBIRDCommunicationMode() ? MQEDIMessage.Status.Pending : MQEDIMessage.Status.Acknowledged;
		}

		ICusEntryHeaderMessageAttachee Entry
		{
			get { return (ICusEntryHeaderMessageAttachee)messageAttachee; }
		}
	}
}
