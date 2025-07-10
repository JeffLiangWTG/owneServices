using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public record BOMPartCacheKey
	{
		public BOMPartCacheKey(ZGuid productPK, ZString packType)
		{
			ProductPK = productPK;
			PackType = packType;
		}

		public BOMPartCacheKey(ZGuid parentProductPK, ZGuid bomProductPK, ZString packType)
		{
			ParentProductPK = parentProductPK;
			ProductPK = bomProductPK;
			PackType = packType;
		}

		public ZGuid ProductPK;
		public ZString PackType;
		public ZGuid? ParentProductPK;
	}
}
