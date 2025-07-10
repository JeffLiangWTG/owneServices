using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class GoodsShipmentAdditionalDocumentWrapper : AdditionalDocumentWrapper
	{
		internal GoodsShipmentAdditionalDocumentWrapper() { }

		public GoodsShipmentAdditionalDocumentWrapper(SupportingDocument supportingDocument, IeDoc ieDoc)
			: base(supportingDocument, ieDoc)
		{
		}

		protected override ZString GetTypeCodeCore()
		{
			var documentType = IeDoc.DocType;
			var result = ZString.Empty;
			if (!documentType.IsEmpty)
			{
				switch (documentType)
				{
					case MessageConstants.DocumentTypes.PKL:
						result = MessageConstants.DocumentTypes.Codes.PKL;
						break;
					case MessageConstants.DocumentTypes.CAT:
						result = MessageConstants.DocumentTypes.Codes.CAT;
						break;
					case MessageConstants.DocumentTypes.CIV:
						result = MessageConstants.DocumentTypes.Codes.CIV;
						break;
					case MessageConstants.DocumentTypes.IEP:
						result = MessageConstants.DocumentTypes.Codes.IEP;
						break;
					case MessageConstants.DocumentTypes.TDM:
						result = MessageConstants.DocumentTypes.Codes.TDM;
						break;
					default:
						result = MessageConstants.DocumentTypes.Codes.Others;
						break;
				}
			}
			return result;
		}

		protected override IAdditionalDocument GenerateAdditionalDocument(SupportingDocument supDoc, IeDoc edoc)
		{
			return new GoodsShipmentAdditionalDocumentWrapper(supDoc, edoc);
		}
	}
}
