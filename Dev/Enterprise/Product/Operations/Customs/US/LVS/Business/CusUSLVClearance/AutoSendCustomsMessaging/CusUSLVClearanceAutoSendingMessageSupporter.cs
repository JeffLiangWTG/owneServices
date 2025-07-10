using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BatchProcessor;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.LVS.Business
{
	public partial class CusUSLVClearance : IJobDeclarationAutoSendingMessageSupporter
	{
		#region IJobDeclarationAutoSendingMessageSupporter Members

		ZBool IJobDeclarationAutoSendingMessageSupporter.SupportEntryDeclarationMessage => false;
		ZBool IJobDeclarationAutoSendingMessageSupporter.SupportReleaseMessage => true;
		ZString IJobDeclarationAutoSendingMessageSupporter.GetReasonForNotSupportEntryDeclarationMessage => "Send Entry/Declaration Message trigger action is not supported for Low Value Entries.";
		ZString IJobDeclarationAutoSendingMessageSupporter.GetReasonForNotSupportReleaseMessage => ZString.Empty;

		IProcessor IJobDeclarationAutoSendingMessageSupporter.CreateEntryDeclarationMessageProcessor() => null;

		IProcessor IJobDeclarationAutoSendingMessageSupporter.CreateReleaseMessageProcessor(string eventCode) => new LVSAutoSendCargoReleaseMessageProcessor(this, eventCode);

		IProcessor IBaseAutoSendingMessageSupporter.CreateStmProcessQueueProcessor(BusinessObject parent, ZString triggerActionCode) => new CustomsStmProcessQueueCreatorProcessor(this, CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, triggerActionCode);

		#endregion
	}
}
