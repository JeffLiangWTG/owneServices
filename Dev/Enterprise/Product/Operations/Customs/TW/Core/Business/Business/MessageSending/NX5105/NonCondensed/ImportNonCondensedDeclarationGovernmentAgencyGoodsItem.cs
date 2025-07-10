using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class ImportNonCondensedDeclarationGovernmentAgencyGoodsItem : NX5105GoodsShipment_GovernmentAgencyGoodsItem
	{
		public ImportNonCondensedDeclarationGovernmentAgencyGoodsItem(CusEntryLine cusEntryLine, JobComInvoiceLine invoiceLine, int sequenceNumeric)
			: base(cusEntryLine, invoiceLine)
		{
			this.sequenceNumeric = sequenceNumeric;
		}

		readonly int sequenceNumeric;

		protected override IGoodsMeasure GetGoodsMeasure()
		{
			return new ImportNonCondensedDeclarationGoodsMeasure(EntryLine, InvoiceLine);
		}

		protected override ICommodity GetCommodityCore()
		{
			return new ImportNonCondensedDeclarationCommodity(EntryLine, InvoiceLine);
		}

		public override ZString ExtraInfoForClassification => InvoiceLine.JI_ExtraInfoForClassification;

		public override IGoodsStatisticalMeasure GoodsStatisticalMeasure
		{
			get
			{
				IGoodsStatisticalMeasure goodsStatisticalMeasure = null;
				var customsSecondQty = InvoiceLine.JI_CustomsSecondQuantity;
				var customsSecondUnitQty = InvoiceLine.JI_CustomsSecondUnitQty;
				if (!customsSecondQty.IsEmpty && !customsSecondUnitQty.IsEmpty)
				{
					goodsStatisticalMeasure = new GoodsStatisticalMeasureWrapper(customsSecondQty, customsSecondUnitQty);
				}
				return goodsStatisticalMeasure;
			}
		}

		protected override ZInt SequenceNumericCore => sequenceNumeric;

		#region Properties For Documents
		public override ZString EntryLineGroupForDocument => InvoiceLine.JI_Group;

		public override ZString CommodityDescriptionForDocument => InvoiceLine.JI_DeclarationGoodsDescription;
		#endregion
	}
}
