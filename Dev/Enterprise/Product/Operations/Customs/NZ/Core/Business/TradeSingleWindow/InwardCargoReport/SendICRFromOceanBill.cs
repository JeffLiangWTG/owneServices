using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class SendICRFromOceanBill : SendICR
	{
		public SendICRFromOceanBill(CusSCAOceanBill oceanBill, IAdditionalInformation additionalMessageInformation, TSWTransactionTypes transactionType) : base(oceanBill, additionalMessageInformation, transactionType)
		{
		}

		CusSCAOceanBill OceanBill => (CusSCAOceanBill)HostEntity;

		public override ZString ApplicationReference => OceanBill.CB_MessageReference;

		protected override void AddMessageToMessages(TSWMessage message) => OceanBill.Messages.Add(message);

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
				errorList.Add(messageErrorCollector.ToUniqueMessageListString());
			}
		}

		protected override void SetStatusOnSuccess(StatusTransactionScope scope)
		{
			scope.Add(OceanBill.CB_MessageStatusInfo, OceanBill.CB_CustomsStatusInfo);
			OceanBill.CB_MessageStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			OceanBill.CB_CustomsStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			OceanBill.HouseBills.Cast<CusSCAHouse>().ForEach(x =>
			{
				scope.Add(x.CA_MessageStatusInfo);
				x.CA_MessageStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			});

			scope.AddLog(CreateMessageSentLogEntry());
		}

		StmALog CreateMessageSentLogEntry()
		{
			if (TransactionType == TSWTransactionTypes.Cancel)
			{
				return OceanBill.Logs.AddNew(Events.MessageWithdrawCancelRequest, LogReferenceCancel, ZDateTimeOffset.Now, false);
			}
			else
			{
				return OceanBill.Logs.AddNew(Events.MessageSent, LogReferenceSent, ZDateTimeOffset.Now, false);
			}
		}

		protected override ICRMessageBuilder GetICRMessageBuilder() => new ICRMessageBuilder(new ICROceanBillWrapper(OceanBill, AdditionalMessageInformation), TransactionType, SubmitterCode);
	}
}
