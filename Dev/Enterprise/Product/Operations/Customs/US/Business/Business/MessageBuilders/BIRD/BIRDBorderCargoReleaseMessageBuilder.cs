using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class BIRDBorderCargoReleaseMessageBuilder : BorderCargoReleaseMessageBuilderBase<BIRDInputBlockControlGenerator>
	{
		public BIRDBorderCargoReleaseMessageBuilder(ICargoReleaseCusEntryHeader entryHeader)
			: base(entryHeader, UpdateActionCode.Add)
		{
		}

		protected override string ApplicationIdentifier
		{
			get { return ApplicationIdentifierCodeList.Codes.BIRDTransaction; }
		}

		protected override BIRDInputBlockControlGenerator GetNewInputBlockControlGenerator()
		{
			JobDeclaration declaration = entryHeader.Factory.Load<JobDeclaration>(entryHeader.DeclarationPK);

			ZString refNo = declaration.US_BRDRefNo.IsEmpty ? declaration.JE_DeclarationReference : declaration.US_BRDRefNo;
			return new BIRDInputBlockControlGenerator(entryHeader, BIRDApplicationCodeList.Codes.CargoRelease, refNo.Right(20));
		}

		protected override void SetMessageSubType(MQEDIMessage message)
		{
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BIRDCargoRelease;

			JobDeclaration declaration = entryHeader.Factory.Load<JobDeclaration>(entryHeader.DeclarationPK);
			message.EM_Status = declaration.HasBIRDCommunicationMode() ? MQEDIMessage.Status.Pending : MQEDIMessage.Status.Acknowledged;
		}
	}
}
