using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.DataTransfer.Gate
{
	public static class FacilityMatchingHelper
	{
		public static WhsWarehouse GetFacilityFromCommunityCode(BusinessObjectFactory factory, string communityCode, string facilityType)
		{
			var communityCodeQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OA_PremisesAddress);
			_ = communityCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, communityCode ?? string.Empty);
			_ = communityCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.ContainerChainCommunityCode);

			var warehouseQuery = new ZDBOnlyQuery(typeof(WhsWarehouse));
			_ = warehouseQuery.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, facilityType);
			warehouseQuery.AddSubQuery(WhsWarehouseSchema.WW_OA_WarehouseAddress, communityCodeQuery, JoinCondition.And);

			var containerYard = factory.LoadTop1<WhsWarehouse>(warehouseQuery);
			return containerYard;
		}

		public static WhsWarehouse GetFacilityFromOrgCodeAndAddressCode(BusinessObjectFactory factory, string orgCode, string addressCode, string facilityType)
		{
			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
			_ = orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_Code, orgCode ?? string.Empty);

			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), WhsWarehouseSchema.WW_OA_WarehouseAddress);
			_ = orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_Code, addressCode ?? string.Empty);
			orgAddressSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);

			var warehouseQuery = new ZDBOnlyQuery(typeof(WhsWarehouse));
			_ = warehouseQuery.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, facilityType);
			warehouseQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

			var containerYard = factory.LoadTop1<WhsWarehouse>(warehouseQuery);
			return containerYard;
		}
	}
}
