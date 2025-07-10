using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class ExportNonCondensedDeclarationMessageSendingObject : N5203MessageSendingObject
	{
		public ExportNonCondensedDeclarationMessageSendingObject(CusEntryHeader header) : base(header)
		{
		}

		protected override IGoodsShipment GoodsShipmentCore => new ExportNonCondensedDeclarationGoodsShipment(Header, SupportingDocuments, GetAllEDocs());
	}
}
