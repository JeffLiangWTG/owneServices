using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class CommodityRelatedPackagingWrapper : ICommodityRelatedPackaging
	{
		public CommodityRelatedPackagingWrapper(ZString packingMethodDescription, ZString materialCode, ZString specification)
		{
			this.packingMethodDescription = packingMethodDescription;
			this.materialCode = materialCode;
			this.specification = specification;
		}

		ZString ICommodityRelatedPackaging.PackingMethodDescription => packingMethodDescription;

		ZString ICommodityRelatedPackaging.MaterialCode => materialCode;

		ZString ICommodityRelatedPackaging.Specification => specification;

		readonly ZString packingMethodDescription;

		readonly ZString materialCode;

		readonly ZString specification;

		public static ICommodityRelatedPackaging GetCommodityRelatedPackaging(JobComInvoiceLine line)
		{
			ICommodityRelatedPackaging result = null;
			var packingMethodDescription = line.JI_InnerPackType;
			var materialCode = line.JI_InnerPackingMaterial;
			var specification = line.JI_InnerPackDescription;
			if (!packingMethodDescription.IsEmpty || !materialCode.IsEmpty || !specification.IsEmpty)
			{
				result = new CommodityRelatedPackagingWrapper(packingMethodDescription, materialCode, specification);
			}
			return result;
		}
	}
}
