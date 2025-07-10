using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.InwardCargoReport
{
	public class SendDTRFromMAWB : SendICR
	{
		public SendDTRFromMAWB(
			TranshipmentRequest transhipmentRequest,
			IAdditionalInformation additionalMessageInformation,
			TSWTransactionTypes transactionType) : base(transhipmentRequest, additionalMessageInformation, transactionType)
		{
		}

		TranshipmentRequest TranshipmentRequest => (TranshipmentRequest)HostEntity;
		protected CusMAWB MAWB => TranshipmentRequest.Parent as CusMAWB;

		public override ZString ApplicationReference => TranshipmentRequest.C4_SendersMessageReference;

		protected override void AddMessageToMessages(TSWMessage message) => TranshipmentRequest.Messages.Add(message);

		protected override ICRMessageBuilder GetICRMessageBuilder() => new ICRMessageBuilder(new DTRMawbWrapper(MAWB, TranshipmentRequest, AdditionalMessageInformation), TransactionType, SubmitterCode);

		protected override void SetStatusOnSuccess(StatusTransactionScope scope)
		{
			scope.Add(TranshipmentRequest.C4_StatusInfo);
			TranshipmentRequest.C4_Status = CombinedMovementStatus.Codes.STC;
		}

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
				errorList.Add(messageErrorCollector.ToUniqueMessageListString() + System.Environment.NewLine);
			}

			if (MAWB.SendingAgentAddress.IsEmpty)
			{
				errorList.Add(Res.GetString("032F5F35-41C5-46DC-A7FC-7E42D424805D", "Sending Agent: Sending Agent is required for Master DTRs."));
			}
		}
	}
}
