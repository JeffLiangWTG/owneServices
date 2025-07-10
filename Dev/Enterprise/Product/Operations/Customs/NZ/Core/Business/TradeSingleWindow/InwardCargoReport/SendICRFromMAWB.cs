using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class SendICRFromMAWB : SendICR
	{
		public SendICRFromMAWB(CusMAWB mawb, IAdditionalInformation additionalMessageInformation, TSWTransactionTypes transactionType)
			: base(mawb, additionalMessageInformation, transactionType)
		{
		}

		protected CusMAWB MAWB => (CusMAWB)HostEntity;

		public override ZString ApplicationReference => MAWB.CM_MessageReference;

		protected override void SetStatusOnSuccess(StatusTransactionScope scope)
		{
			scope.Add(MAWB.CM_CustomsStatusInfo);
			MAWB.CM_CustomsStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			foreach (CusHAWB hawb in MAWB.ChildBills)
			{
				scope.Add(hawb.CS_CustomsStatusInfo, hawb.CS_MsgStatusInfo);
				hawb.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
				hawb.CS_MsgStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			}

			scope.AddLog(CreateMessageSentLogEntry());
		}

		StmALog CreateMessageSentLogEntry()
		{
			if (TransactionType == TSWTransactionTypes.Cancel)
			{
				return MAWB.Logs.AddNew(Events.MessageWithdrawCancelRequest, LogReferenceCancel, ZDateTimeOffset.Now, false);
			}
			else
			{
				return MAWB.Logs.AddNew(Events.MessageSent, LogReferenceSent, ZDateTimeOffset.Now, false);
			}
		}

		protected override void AddMessageToMessages(TSWMessage message) => MAWB.Messages.Add(message);

		protected override void CheckErrorsBeforeGeneratingMessageCore()
		{
			MAWB.LoadChildEditableObjects();
			using (((IBusinessObjectInternals)MAWB).ResumeValidationForAllDescendantsTemporarily())
			{
				MAWB.RunPreSaveValidation();
			}

			if (MAWB.HasErrors)
			{
				var errorCollector = new Customs.Business.CustomsNotificationCollector(MAWB, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors();
				errorList.Add(errorCollector.ToUniqueMessageListString() + System.Environment.NewLine);
			}

			if (MAWB.HasMessageErrors)
			{
				var messageErrorCollector = new Customs.Business.CustomsNotificationCollector(MAWB, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors();
				errorList.Add(messageErrorCollector.ToUniqueMessageListString());
			}
		}

		protected override ICRMessageBuilder GetICRMessageBuilder() => new ICRMessageBuilder(new ICRMawbWrapper(MAWB, AdditionalMessageInformation), TransactionType, SubmitterCode);
	}
}
