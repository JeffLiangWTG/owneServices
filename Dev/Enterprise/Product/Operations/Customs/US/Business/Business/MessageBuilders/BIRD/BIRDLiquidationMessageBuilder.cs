using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class BIRDLiquidationMessageBuilder : MessageBuilder<BIRDInputBlockControlGenerator>
	{
		public BIRDLiquidationMessageBuilder(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override string ApplicationIdentifier
		{
			get { return ApplicationIdentifierCodeList.Codes.BIRDTransaction; }
		}

		protected override BIRDInputBlockControlGenerator GetNewInputBlockControlGenerator()
		{
			ZString refNo = Declaration.US_BRDRefNo.IsEmpty ? Declaration.JE_DeclarationReference : Declaration.US_BRDRefNo;
			BIRDInputBlockControlGenerator result = new BIRDInputBlockControlGenerator(Declaration, BIRDApplicationCodeList.Codes.EntrySummaryQueryOutput, refNo.Right(20));

			return result;
		}

		protected override void UpdateMessageBlocks(BIRDInputBlockControlGenerator block)
		{
			CusLiquidation lastLiquidation = Declaration.Liquidations.GetMostRecentLiquidation();
			MQEDIMessage lastMessage = lastLiquidation != null ? lastLiquidation.Message : null;

			if (lastMessage != null)
			{
				block.AddMessageBlocks(lastMessage.MessageBlock.MessageBlocks);
			}
			else
			{
				ErrorReporter.ReportOnce("BIRDLiquidationMessageBuilder", "This builder is triggered while there is no recent message to build from.");
			}
		}

		protected override void SetMessageSubType(MQEDIMessage message)
		{
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BIRDLiquidationNotice;

			message.EM_Status = Declaration.HasBIRDCommunicationMode() ? MQEDIMessage.Status.Pending : MQEDIMessage.Status.Acknowledged;
		}

		JobDeclaration Declaration
		{
			get { return (JobDeclaration)messageAttachee; }
		}
	}
}
