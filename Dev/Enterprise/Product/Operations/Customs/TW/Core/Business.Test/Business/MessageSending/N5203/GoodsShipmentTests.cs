using Enterprise.Customs.TW.Business.N5203;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class GoodsShipmentTests : GoodsShipmentAbstractTests<GoodsShipment, N5203MessageSendingObject>
	{
		protected override GoodsShipment GetGoodsShipment(CusEntryHeader entryHeader, SupportingDocumentCollection supportingDocuments, IStorageDocsBaseCollection[] allEDocs) => new GoodsShipment(entryHeader, supportingDocuments, allEDocs);

		protected override N5203MessageSendingObject GetMessageSendingObject(CusEntryHeader entryHeader) => new N5203MessageSendingObject(entryHeader);
	}
}
