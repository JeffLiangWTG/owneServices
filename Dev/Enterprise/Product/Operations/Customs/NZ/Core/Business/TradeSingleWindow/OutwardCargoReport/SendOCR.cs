using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public abstract class SendOCR : SendTSW
	{
		protected SendOCR(BusinessObject hostEntity, IAdditionalInformation additionalMessageInformation, TSWTransactionTypes transactionType)
			: base(hostEntity, additionalMessageInformation, transactionType)
		{
		}

		public override ZString GetMessageText() => OCRBuilder.GetXMLMessage();

		public override ZString DeclarantPinEncrypted => OCRBuilder.DeclarantPinEncrypted;

		public override ZBool DeclarantPinRequired => false;

		protected override ZBool GetIsMessageInTestMode => NZCustomsDataRegistry.Instance.ExportOrnTestMode.Value;

		public override ZString ApplicationReference => ZString.Empty;

		protected override ZString MessageType => MessageTypeList.Codes.OCR;

		protected override ZString MessageSubType
		{
			get
			{
				var result = string.Empty;

				switch (TransactionType)
				{
					case TSWTransactionTypes.Original:
						result = NZCMessage.MessageTypes.OutwardReport.MessageSubTypes.Original;
						break;
					case TSWTransactionTypes.Cancel:
						result = NZCMessage.MessageTypes.OutwardReport.MessageSubTypes.Cancellation;
						break;
					case TSWTransactionTypes.Replace:
						result = NZCMessage.MessageTypes.OutwardReport.MessageSubTypes.Replacement;
						break;
				}

				return result;
			}
		}

		protected override void SetStatusOnSuccess(StatusTransactionScope scope)
		{
		}

		protected override void AddMessageToMessages(TSWMessage message)
		{
		}

		protected override void CheckErrorsBeforeGeneratingMessageCore()
		{
		}

		protected virtual OCRMessageBuilder OCRBuilder => ocrBuilder ?? (ocrBuilder = new OCRMessageBuilder(null, TransactionType, SubmitterCode));
		OCRMessageBuilder ocrBuilder;
	}
}
