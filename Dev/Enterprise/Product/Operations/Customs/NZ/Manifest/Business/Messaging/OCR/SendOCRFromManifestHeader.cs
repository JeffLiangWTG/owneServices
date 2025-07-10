using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public class SendOCRFromManifestHeader : SendOCR
	{
		public SendOCRFromManifestHeader(AsycudaManifestHeader manifestHeader, IAdditionalInformation additionalMessageInformation, TSWTransactionTypes transactionType)
			: base(manifestHeader, additionalMessageInformation, transactionType)
		{
		}

		AsycudaManifestHeader ManifestHeader => (AsycudaManifestHeader)HostEntity;

		public override ZString ApplicationReference => ManifestHeader.AMA_JobReference;

		protected override void AddMessageToMessages(TSWMessage message) => ManifestHeader.Messages.Add(message);

		protected override void CheckErrorsBeforeGeneratingMessageCore()
		{
			ManifestHeader.LoadChildEditableObjects();
			using (((IBusinessObjectInternals)ManifestHeader).ResumeValidationForAllDescendantsTemporarily())
			{
				ManifestHeader.RunPreSaveValidation();
			}

			if (ManifestHeader.HasErrors)
			{
				var errorCollector = new CustomsNotificationCollector(ManifestHeader, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors();
				errorList.Add(errorCollector.ToUniqueMessageListString() + System.Environment.NewLine);
			}
		}

		public override ZString GetBOValidationMessageErrors()
		{
			var result = ZString.Empty;
			var messageErrorCollector = new CustomsNotificationCollector(ManifestHeader, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors();
			result = messageErrorCollector.ToUniqueMessageListString();

			if (!result.IsEmpty)
			{
				result = MessageSendingValidation.MessageErrorsExistHeaderText + System.Environment.NewLine + result + System.Environment.NewLine + MessageSendingValidation.MessageErrorConfirmationQuestionText;
			}
			return result;
		}

		protected override void SetStatusOnSuccess(StatusTransactionScope scope)
		{
			base.SetStatusOnSuccess(scope);
			scope.Add(ManifestHeader.AMA_MessageStatusInfo);
			ManifestHeader.AMA_MessageStatus = NZMessageStatusList.Codes.Sent;
			ManifestHeader.Bills.Cast<AsycudaBill>().ForEach(x =>
			{
				scope.Add(x.ABL_MessageStatusInfo);
				x.ABL_MessageStatus = NZMessageStatusList.Codes.Sent;
			});
		}

		protected override OCRMessageBuilder OCRBuilder => ocrBuilder ?? (ocrBuilder = GetOCRMessageBuilder());
		OCRMessageBuilder ocrBuilder;

		protected OCRMessageBuilder GetOCRMessageBuilder()
		{
			return new OCRMessageBuilder(new OCRManifestHeaderWrapper(ManifestHeader, AdditionalMessageInformation), TransactionType, SubmitterCode);
		}
	}
}
