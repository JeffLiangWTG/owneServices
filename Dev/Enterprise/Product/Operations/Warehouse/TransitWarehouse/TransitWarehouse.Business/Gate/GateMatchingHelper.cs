using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business
{
	public static class GateMatchingHelper
	{
		public static WhsWarehouse GetFacilityFromCommunityCode(BusinessObjectFactory factory, string communityCode, string facilityType)
		{
			var zDBOnlySubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OA_PremisesAddress);
			zDBOnlySubQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, communityCode ?? string.Empty);
			zDBOnlySubQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, "CC1");
			var zDBOnlyQuery = new ZDBOnlyQuery(typeof(WhsWarehouse));
			zDBOnlyQuery.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, facilityType);
			zDBOnlyQuery.AddSubQuery(WhsWarehouseSchema.WW_OA_WarehouseAddress, zDBOnlySubQuery, JoinCondition.And);
			return factory.LoadTop1<WhsWarehouse>(zDBOnlyQuery);
		}

		public static WhsWarehouse GetFacilityFromOrgCodeAndAddressCode(BusinessObjectFactory factory, string orgCode, string addressCode, string facilityType)
		{
			var zDBOnlySubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
			zDBOnlySubQuery.AddToFilter(OrgHeaderSchema.OH_Code, orgCode ?? string.Empty);
			var zDBOnlySubQuery2 = new ZDBOnlySubQuery(typeof(OrgAddress), WhsWarehouseSchema.WW_OA_WarehouseAddress);
			zDBOnlySubQuery2.AddToFilter(OrgAddressSchema.OA_Code, addressCode ?? string.Empty);
			zDBOnlySubQuery2.AddSubQuery(zDBOnlySubQuery, JoinCondition.And);
			var zDBOnlyQuery = new ZDBOnlyQuery(typeof(WhsWarehouse));
			zDBOnlyQuery.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, facilityType);
			zDBOnlyQuery.AddSubQuery(zDBOnlySubQuery2, JoinCondition.And);
			return factory.LoadTop1<WhsWarehouse>(zDBOnlyQuery);
		}

		public static WhsItemReceiveTransportationUnit FindMatchingRTUByJobLink(BusinessObjectFactory factory, string sourceType, string sourceKey) => FindMatchingTransportationUnitByJobLink<WhsItemReceiveTransportationUnit>(factory, sourceType, sourceKey, WhsItemReceiveTransportationUnitSchema.Constants.Prefix);

		public static WhsItemDispatchTransportationUnit FindMatchingDTUByJobLink(BusinessObjectFactory factory, string sourceType, string sourceKey) => FindMatchingTransportationUnitByJobLink<WhsItemDispatchTransportationUnit>(factory, sourceType, sourceKey, WhsItemDispatchTransportationUnitSchema.Constants.Prefix);

		static T FindMatchingTransportationUnitByJobLink<T>(BusinessObjectFactory factory, string sourceType, string sourceKey, string parentTableCode) where T : BusinessObject
		{
			var stmUniversalJobLinkQuery = new ZDBOnlySubQuery(typeof(StmUniversalJobLink), StmUniversalJobLinkSchema.UCL_ParentID);
			stmUniversalJobLinkQuery.AddToFilter(StmUniversalJobLinkSchema.UCL_ParentTableCode, parentTableCode);
			stmUniversalJobLinkQuery.AddToFilter(StmUniversalJobLinkSchema.UCL_SourceType, sourceType);
			stmUniversalJobLinkQuery.AddToFilter(StmUniversalJobLinkSchema.UCL_SourceKey, sourceKey);

			var query = new ZDBOnlyQuery(typeof(T));
			query.AddSubQuery(stmUniversalJobLinkQuery, JoinCondition.And);
			return string.IsNullOrEmpty(sourceKey) ? null : factory.LoadTop1<T>(query);
		}
	}
}
