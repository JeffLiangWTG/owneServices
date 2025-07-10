using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class DocketUNDGValidationCacheTest : WhsTestCaseWithFactory
	{
		public void TestConstructor()
		{
			var docketOverLimitUNDGCache = Array.Empty<WhsWarehouseUNDGTotalsInfo>();
			var docketOverThresholdUNDGCache = Array.Empty<WhsWarehouseUNDGTotalsInfo>();
			var productUNDGInfosCache = new Dictionary<ZGuid, WhsProductUNDGInfo[]>();
			var dgCodeCache = new Dictionary<ZGuid, ZString>();
			var dcrCodeCache = new Dictionary<ZGuid, ZString>();

			var cache = new DocketUNDGValidationCache(
				docketOverLimitUNDGCache,
				docketOverThresholdUNDGCache,
				productUNDGInfosCache,
				dgCodeCache,
				dcrCodeCache);

			CombineAssertions(() =>
			{
				AssertEquals(cache.DocketOverLimitUNDGs, docketOverLimitUNDGCache);
				AssertEquals(cache.DocketOverThresholdUNDGs, docketOverThresholdUNDGCache);
				AssertEquals(cache.ProductUNDGInfos, productUNDGInfosCache);
				AssertEquals(cache.DGCodes, dgCodeCache);
				AssertEquals(cache.DCRCodes, dcrCodeCache);
			});
		}
	}
}
