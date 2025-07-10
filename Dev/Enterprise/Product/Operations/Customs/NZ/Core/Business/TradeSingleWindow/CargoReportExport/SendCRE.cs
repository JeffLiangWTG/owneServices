using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public abstract class SendCRE : SendTSW
	{
		public SendCRE(BusinessObject hostEntity, IAdditionalInformation additionalMessageInformation, TSWTransactionTypes transactionType)
			: base(hostEntity, additionalMessageInformation, transactionType)
		{
		}

		public override ZString GetMessageText() => CREBuilder.GetXMLMessage();

		protected override IEnumerable<ITSWAttachment> GetAdditionalSupportingDocuments() => CREBuilder?.SupportingDocuments() ?? base.GetAdditionalSupportingDocuments();

		public override ZString DeclarantPinEncrypted => CREBuilder.DeclarantPinEncrypted;

		public override ZBool DeclarantPinRequired => CREBuilder.DeclarantPinRequired;

		protected override ZString MessageType => MessageTypeList.Codes.CRE;

		protected override ZString MessageSubType
		{
			get
			{
				var result = string.Empty;
				switch (TransactionType)
				{
					case TSWTransactionTypes.Original:
						result = Declaration.NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Original;
						break;
					case TSWTransactionTypes.Cancel:
						result = Declaration.NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Cancellation;
						break;
					case TSWTransactionTypes.Replace:
						result = Declaration.NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Replacement;
						break;
				}

				return result;
			}
		}

		protected override ZBool GetIsMessageInTestMode => NZCustomsDataRegistry.Instance.ExportEciTestMode.Value;

		CREMessageBuilder CREBuilder => creBuilder ?? (creBuilder = GetCREMessageBuilder());
		CREMessageBuilder creBuilder;

		protected abstract CREMessageBuilder GetCREMessageBuilder();
	}
}
