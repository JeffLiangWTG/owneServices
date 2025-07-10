using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class ExportNonCondensedDeclarationGovernmentAgencyGoodsItem : GovernmentAgencyGoodsItem
	{
		public ExportNonCondensedDeclarationGovernmentAgencyGoodsItem(CusEntryLine cusEntryLine, JobComInvoiceLine invoiceLine, int sequenceNumeric) : base(cusEntryLine, invoiceLine)
		{
			this.sequenceNumeric = sequenceNumeric;
		}

		readonly int sequenceNumeric;

		public override ZInt SequenceNumeric => sequenceNumeric;

		protected override ICommodity CommodityCore => new ExportNonCondensedDeclarationCommodity(EntryLine, InvoiceLine);

		public override IEnumerable<IAdditionalDocument> AdditionalDocuments => GetAdditionalDocuments(InvoiceLine);

		public override IGoodsMeasure GoodsMeasure => new ExportNonCondensedDeclarationGoodsMeasure(EntryLine, InvoiceLine);

		public override IGoodsStatisticalMeasure GoodsStatisticalMeasure => new ExportNonCondensedDeclarationGoodsStatisticalMeasure(EntryLine, InvoiceLine);

		public override ZString EntryLineGroupForDocument => InvoiceLine?.JI_Group ?? ZString.Empty;

		public override ZString CommodityDescriptionForDocument => InvoiceLine?.JI_DeclarationGoodsDescription ?? ZString.Empty;
	}
}
