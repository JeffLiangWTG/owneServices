using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class ImportNonCondensedDeclarationMessageSendingObject : NX5105MessageSendingObject
	{
		public ImportNonCondensedDeclarationMessageSendingObject(CusEntryHeader header) : base(header)
		{
		}

		protected override IGoodsShipment GetGoodsShipment()
		{
			return new ImportNonCondensedDeclarationGoodsShipment(Header, SupportingDocuments, GetAllEDocs());
		}
	}
}
