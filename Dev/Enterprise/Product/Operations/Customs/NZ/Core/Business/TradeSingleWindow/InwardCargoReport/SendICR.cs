using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public abstract class SendICR : SendTSW
	{
		public SendICR(BusinessObject hostEntity, IAdditionalInformation additionalMessageInformation, TSWTransactionTypes transactionType)
			: base(hostEntity, additionalMessageInformation, transactionType)
		{
		}
		public override ZString GetMessageText() => ICRBuilder.GetXMLMessage();

		protected override IEnumerable<ITSWAttachment> GetAdditionalSupportingDocuments() => ICRBuilder?.SupportingDocuments() ?? base.GetAdditionalSupportingDocuments();
		public override ZString DeclarantPinEncrypted => ICRBuilder.DeclarantPinEncrypted;

		public override ZBool DeclarantPinRequired => ICRBuilder.DeclarantPinRequired;

		protected override ZString MessageType => MessageTypeList.Codes.ICR;

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

		protected override ZBool GetIsMessageInTestMode => NZCustomsDataRegistry.Instance.ImportEciTestMode.Value;

		ICRMessageBuilder ICRBuilder => icrBuilder ?? (icrBuilder = GetICRMessageBuilder());
		ICRMessageBuilder icrBuilder;

		protected abstract ICRMessageBuilder GetICRMessageBuilder();
	}
}
