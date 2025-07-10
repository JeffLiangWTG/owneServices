using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class ItemProvider : IGoodsInformation
	{
		public ItemProvider(AsycudaPackedItem asycudaPackedItem, ZInt itemSeq)
		{
			this.packedItem = asycudaPackedItem;
			this.itemSeq = itemSeq;
		}
		readonly AsycudaPackedItem packedItem;
		readonly ZInt itemSeq;

		public ZString UNGoodCode => packedItem.UNDGs.FirstOrDefault()?.SubstanceCode ?? ZString.Empty;
		public ZDecimal GrossWeight => Core.Constants.Weight.ConvertSafe(packedItem.API_GrossWeight, packedItem.API_GrossWeightUQ, Core.Constants.Weight.Kilograms);
		public ZString TariffCode => packedItem.API_Tariff;
		public ZString GoodsDescription => packedItem.API_GoodsDescription.SubstringSafe(0, 100);
		public ZDecimal GoodsValue => packedItem.API_GoodsValue;
		public ZString GoodsValueCurrency => packedItem.API_RX_NKGoodsValueCurrency;
		public ZInt OrderNo => itemSeq;
		public ZDecimal NetWeight => Core.Constants.Weight.ConvertSafe(packedItem.API_NetWeight, packedItem.API_NetWeightUQ, Core.Constants.Weight.Kilograms);
		public ZString CustomsUQ => TurkishConstants.WeightUnitType;
	}
}
