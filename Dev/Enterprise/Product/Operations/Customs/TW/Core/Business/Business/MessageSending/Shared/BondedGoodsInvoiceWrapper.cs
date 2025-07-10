using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class BondedGoodsInvoiceWrapper : IBondedGoodsInvoice
	{
		readonly ZString id;
		readonly ZDecimal valueAmount;

		public BondedGoodsInvoiceWrapper(ZString id, ZDecimal valueAmount)
		{
			this.id = id;
			this.valueAmount = valueAmount;
		}

		ZString IBondedGoodsInvoice.ID => id;

		ZDecimal IBondedGoodsInvoice.ValueAmount => valueAmount;
	}
}
