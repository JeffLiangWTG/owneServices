using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Customs.TW.Business.LineMerger;

namespace Enterprise.Customs.TW.Business
{
	class ImportNonCondensedDeclarationGoodsShipment : NX5105Declaration_GoodsShipment
	{
		public ImportNonCondensedDeclarationGoodsShipment(CusEntryHeader entryHeader, SupportingDocumentCollection supportingDocuments, IStorageDocsBaseCollection[] allEDocs)
			: base(entryHeader, supportingDocuments, allEDocs, false)
		{
		}

		protected override IEnumerable<IGovernmentAgencyGoodsItem> GetGovernmentAgencyGoodsItemsCore()
		{
			return CalSequenceNumGenerator.SortToList(EntryHeader.Declaration.InvoiceLines.Cast<JobComInvoiceLine>()).Select((l, idx) => new ImportNonCondensedDeclarationGovernmentAgencyGoodsItem(l.CusEntryLine, l, idx + 1));
		}
	}
}
