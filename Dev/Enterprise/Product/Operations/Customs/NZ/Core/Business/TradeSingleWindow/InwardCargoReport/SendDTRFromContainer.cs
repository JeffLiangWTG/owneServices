using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class SendDTRFromContainer : SendICR
	{
		public SendDTRFromContainer(
			TranshipmentRequest transhipmentRequest,
			IAdditionalInformation additionalMessageInformation,
			TSWTransactionTypes transactionType) : base(transhipmentRequest, additionalMessageInformation, transactionType)
		{
			Container = transhipmentRequest.Parent as CusSCAContainer;
			OceanBill = Container.OceanBill;
		}

		CusSCAContainer Container { get; }
		CusSCAOceanBill OceanBill { get; }
		TranshipmentRequest TranshipmentRequest => (TranshipmentRequest)HostEntity;

		public override ZString ApplicationReference => TranshipmentRequest.C4_SendersMessageReference;

		protected override void AddMessageToMessages(TSWMessage message) => TranshipmentRequest.Messages.Add(message);

		protected override ICRMessageBuilder GetICRMessageBuilder() => new ICRMessageBuilder(new DTRContainerWrapper(Container, TranshipmentRequest, AdditionalMessageInformation), TransactionType, SubmitterCode);

		protected override void SetStatusOnSuccess(StatusTransactionScope scope)
		{
			scope.Add(TranshipmentRequest.C4_StatusInfo);
			TranshipmentRequest.C4_Status = CombinedMovementStatus.Codes.STC;
		}

		protected override void CheckErrorsBeforeGeneratingMessageCore()
		{
			OceanBill.LoadChildEditableObjects();
			using (((IBusinessObjectInternals)OceanBill).ResumeValidationForAllDescendantsTemporarily())
			{
				OceanBill.RunPreSaveValidation();
			}

			if (OceanBill.HasErrors)
			{
				var errorCollector = new Customs.Business.CustomsNotificationCollector(OceanBill, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors();
				errorList.Add(errorCollector.ToUniqueMessageListString() + System.Environment.NewLine);
			}

			if (OceanBill.HasMessageErrors)
			{
				var messageErrorCollector = new Customs.Business.CustomsNotificationCollector(OceanBill, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors();
				errorList.Add(messageErrorCollector.ToUniqueMessageListString() + System.Environment.NewLine);
			}

			if (OceanBill.SendingAgentAddress.IsEmpty)
			{
				errorList.Add(Res.GetString("B79A9564-4156-4632-9D30-887221B8FF16", "Sending Agent: Sending Agent is required for Container DTRs."));
			}
		}
	}
}
