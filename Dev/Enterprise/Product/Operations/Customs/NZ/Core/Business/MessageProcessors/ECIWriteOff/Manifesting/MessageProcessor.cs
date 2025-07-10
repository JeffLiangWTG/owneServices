using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.ECIWriteOff.Manifesting
{
	class MessageProcessor : ECIWriteOff.MessageProcessor
	{
		public MessageProcessor(LoggingInformation logger)
			: base(logger, MessageFriendlyName)
		{
		}

		public new const string MessageFriendlyName = "Manifest ECI Writeoff CUSRES";

		public override string JobTypeDescription
		{
			get { return "ECI Manifest"; }
		}

		protected override ECIMessageParser.ConsignmentWrapperCollection GetNewConsignmentWrapperCollection()
		{
			return new ECIMessageParser.ConsignmentWrapperCollection(entryHeader.Declarations);
		}

		protected new CusEntryHeader entryHeader
		{
			get { return (CusEntryHeader)base.entryHeader; }
			set { base.entryHeader = value; }
		}

		protected override void WriteEntryStatus()
		{
			Parser.WriteNewStatusesToManifestAndConsignments();
		}
	}
}
