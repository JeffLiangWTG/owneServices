using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class LocationStockOnHandInfo
	{
		public LocationStockOnHandInfo(ZGuid docketPK, ZString palletId, ZString clientCode, ZGuid clientPK, ZString productCode, ZGuid productPK)
		{
			DocketPK = docketPK;
			PalletId = palletId;
			ClientCode = clientCode;
			ClientPK = clientPK;
			ProductCode = productCode;
			ProductPK = productPK;
		}

		public ZGuid DocketPK { get; }
		public ZString PalletId { get; }
		public ZString ClientCode { get; }
		public ZGuid ClientPK { get; }
		public ZString ProductCode { get; }
		public ZGuid ProductPK { get; }
	}
}
