using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public record DocketUNDGValidationCache
	{
		public IReadOnlyCollection<WhsWarehouseUNDGTotalsInfo> DocketOverLimitUNDGs { get; }
		public IReadOnlyCollection<WhsWarehouseUNDGTotalsInfo> DocketOverThresholdUNDGs { get; }
		public Dictionary<ZGuid, WhsProductUNDGInfo[]> ProductUNDGInfos { get; }
		public Dictionary<ZGuid, ZString> DGCodes { get; }
		public Dictionary<ZGuid, ZString> DCRCodes { get; }

		public DocketUNDGValidationCache(
			IReadOnlyCollection<WhsWarehouseUNDGTotalsInfo> docketOverLimitUNDGs,
			IReadOnlyCollection<WhsWarehouseUNDGTotalsInfo> docketOverThresholdUNDGs,
			Dictionary<ZGuid, WhsProductUNDGInfo[]> productUndgInfos,
			Dictionary<ZGuid, ZString> dgCodes,
			Dictionary<ZGuid, ZString> dcrCodes)
		{
			DocketOverLimitUNDGs = docketOverLimitUNDGs;
			DocketOverThresholdUNDGs = docketOverThresholdUNDGs;
			ProductUNDGInfos = productUndgInfos;
			DGCodes = dgCodes;
			DCRCodes = dcrCodes;
		}
	}
}
