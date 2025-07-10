using CargoWise.Types;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class SendOCRFromConsol : SendOCR
	{
		public SendOCRFromConsol(ForwardingConsol consol, IAdditionalInformation additionalMessageInformation, OutwardReportManifestStatus manifestStatus, TSWTransactionTypes transactionType)
			: base(consol, additionalMessageInformation, transactionType)
		{
			this.consol = consol;
			this.manifestStatus = manifestStatus;
		}
		readonly ForwardingConsol consol;
		readonly OutwardReportManifestStatus manifestStatus;

		public override ZString ApplicationReference
		{
			get
			{
				var msgRefNum = consol.Factory.LoadTop1<Declaration.OutwardReport.CusEntryNumber>(Business.Declaration.OutwardReport.CusEntryNumber.GetEntryNumberFilter(consol));
				return msgRefNum == null || msgRefNum.CE_EntryLineReference.IsEmpty ? consol.JK_UniqueConsignRef : msgRefNum.CE_EntryLineReference;
			}
		}

		protected override void SetStatusOnSuccess(StatusTransactionScope scope)
		{
			scope.Add(manifestStatus.E2_MessageStatusInfo);
			manifestStatus.E2_MessageStatus = OutwardReportStatusList.Codes.AwaitingResponse;
		}

		protected override void AddMessageToMessages(TSWMessage message)
		{
			consol.Messages.Add(message);
			consol.Messages.RefreshBinding();
		}

		protected override void CheckErrorsBeforeGeneratingMessageCore()
		{
			if (TransactionType != TSWTransactionTypes.Cancel)
			{
				errorList.AddRange(OutwardReportValidation.CheckErrorsBeforeGeneratingMessage(GetTypeForValidation(TransactionType)));
			}
		}

		OutwardReportValidation OutwardReportValidation => outwardReportValidation ?? (outwardReportValidation = new OutwardReportManifestStatusValidation(manifestStatus));
		OutwardReportValidation outwardReportValidation;

		MessageBuilders.OutwardReport.MessageBuilder.MessageTypes GetTypeForValidation(TSWTransactionTypes transactionType)
		{
			switch (transactionType)
			{
				case TSWTransactionTypes.Cancel:
					return Business.MessageBuilders.OutwardReport.MessageBuilder.MessageTypes.Cancellation;
				case TSWTransactionTypes.Replace:
					return Business.MessageBuilders.OutwardReport.MessageBuilder.MessageTypes.Replacement;
				default:
					return Business.MessageBuilders.OutwardReport.MessageBuilder.MessageTypes.Original;
			}
		}

		protected override OCRMessageBuilder OCRBuilder => ocrBuilder ?? (ocrBuilder = new OCRMessageBuilder(new OCRConsolWrapper(consol, AdditionalMessageInformation, manifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty_ZAddress, manifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyName, manifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyEmail, manifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyPort), TransactionType, SubmitterCode));
		OCRMessageBuilder ocrBuilder;
	}
}
