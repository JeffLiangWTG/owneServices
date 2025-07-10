using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105Declaration_GoodsShipmentTest : NX5105Declaration_GoodsShipmentAbstractTest<NX5105Declaration_GoodsShipment>
	{
		protected override IGoodsShipment GetGoodsShipment(CusEntryHeader entryHeader, SupportingDocumentCollection supportingDocuments, IStorageDocsBaseCollection[] allEDocs, bool includeControllingMessageInformation)
		{
			return new NX5105Declaration_GoodsShipment(entryHeader, supportingDocuments, allEDocs, includeControllingMessageInformation);
		}
	}
}
