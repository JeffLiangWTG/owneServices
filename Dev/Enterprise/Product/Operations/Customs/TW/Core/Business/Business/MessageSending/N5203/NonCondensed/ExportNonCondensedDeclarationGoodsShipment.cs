using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Customs.TW.Business.LineMerger;

namespace Enterprise.Customs.TW.Business
{
	class ExportNonCondensedDeclarationGoodsShipment : GoodsShipment
	{
		public ExportNonCondensedDeclarationGoodsShipment(CusEntryHeader entryHeader, SupportingDocumentCollection supportingDocuments, IStorageDocsBaseCollection[] allEDocs) : base(entryHeader, supportingDocuments, allEDocs)
		{
		}

		protected override IEnumerable<IGovernmentAgencyGoodsItem> GovernmentAgencyGoodsItemsCore => CalSequenceNumGenerator.SortToList(EntryHeader.Declaration.InvoiceLines.Cast<JobComInvoiceLine>()).Select((l, idx) => new ExportNonCondensedDeclarationGovernmentAgencyGoodsItem(l.CusEntryLine, l, idx + 1));

		protected override ZDecimal ItemChargeAmountCore => EntryHeader.CH_TotalCustomsValueInLocalCurrency;
	}
}
