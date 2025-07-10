using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Edifact.D98A.Messages.CUSRES;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.ECIWriteOff
{
	class MessageProcessor : MessageProcessorForEntryHeader
	{
		protected MessageProcessor(LoggingInformation logger, string messageFriendlyName) : base(logger, messageFriendlyName) { }
		public MessageProcessor(LoggingInformation logger)
			: this(logger, "ECI Writeoff CUSRES")
		{
		}

		protected new CusEntryHeader entryHeader
		{
			get { return (CusEntryHeader)base.entryHeader; }
			set { base.entryHeader = value; }
		}

		public override string JobTypeDescription
		{
			get { return "ECI Write-Off"; }
		}

		public override string ResponseTypeForEmailSubject
		{
			get { return new LowValueManifestStatusList().GetDescriptionFromCode(entryHeader.CH_EntryStatus); }
		}

		protected virtual ECIMessageParser.ConsignmentWrapperCollection GetNewConsignmentWrapperCollection()
		{
			return new ECIMessageParser.ConsignmentWrapperCollection(entryHeader.Declaration);
		}

		protected override void ProcessGroup0(CUSRESMessage cUSRESMessage)
		{
			Parser = new ECIMessageParser(this, builder, GetNewConsignmentWrapperCollection());
			Parser.ProcessGroup0(cUSRESMessage);
		}

		protected override void WriteEntryStatus()
		{
			if (!entryHeader.Declaration.IsFormalEntry)
			{
				Parser.WriteNewStatusesToManifestAndConsignments();
			}
		}

		internal override void FinaliseCancellation()
		{
			base.FinaliseCancellation();
			entryHeader.CH_IsActive = false;
		}

		protected override string GetResponseTypeFromCode(string responseTypeCode)
		{
			return ECIProcessing.GetResponseTypeFromCode(responseTypeCode);
		}

		protected override string GetErrorPointFromCode(string errorPointCode)
		{
			return ECIProcessing.GetErrorPointFromCode(errorPointCode);
		}

		protected override string GetItemNumberDescription(string errorItemNumber, string errorSection)
		{
			return ECIProcessing.GetItemNumberDescription(errorItemNumber, errorSection);
		}

		#region Messaging Email Handler Overrides
		bool WriteOffBeingProcessedIsImport => entryHeader?.IsImport ?? false;

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return WriteOffBeingProcessedIsImport ? NZCustomsDataRegistry.Instance.ImportEciSendAcknowledgementsToGroup.Value : NZCustomsDataRegistry.Instance.ExportEciSendAcknowledgementsToGroup.Value; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return WriteOffBeingProcessedIsImport ? NZCustomsDataRegistry.Instance.ImportEciSendAcknowledgements.Value : NZCustomsDataRegistry.Instance.ExportEciSendAcknowledgements.Value; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return WriteOffBeingProcessedIsImport ? NZCustomsDataRegistry.Instance.ImportEciSendImpedimentsToGroup.Value : NZCustomsDataRegistry.Instance.ExportEciSendImpedimentsToGroup.Value; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return WriteOffBeingProcessedIsImport ? NZCustomsDataRegistry.Instance.ImportEciSendImpediments.Value : NZCustomsDataRegistry.Instance.ExportEciSendImpediments.Value; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return WriteOffBeingProcessedIsImport ? NZCustomsDataRegistry.Instance.ImportEciSendErrorsToGroup.Value : NZCustomsDataRegistry.Instance.ExportEciSendErrorsToGroup.Value; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return WriteOffBeingProcessedIsImport ? NZCustomsDataRegistry.Instance.ImportEciSendErrors.Value : NZCustomsDataRegistry.Instance.ExportEciSendErrors.Value; }
		}
		#endregion
	}
}
