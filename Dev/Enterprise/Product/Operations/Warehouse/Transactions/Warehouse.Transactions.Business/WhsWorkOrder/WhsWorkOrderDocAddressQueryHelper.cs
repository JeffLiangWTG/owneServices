using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class WhsWorkOrderDocAddressQueryHelper
	{
		public static ZDBOnlySubQuery GetWorkOrderAddressQuery(ZString docAddressTypeCode, ZGuid orgHeaderPK)
		{
			return GetWorkOrderAddressQuery(docAddressTypeCode, orgHeaderPK, SQLComparisonOperator.Equal);
		}

		public static ZDBOnlySubQuery GetWorkOrderAddressQuery(ZString docAddressTypeCode, ZGuid orgHeaderPK, SQLComparisonOperator comparisonOperator)
		{
			var jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(docAddressTypeCode, orgHeaderPK, comparisonOperator);

			var workOrderRelatedOrderSubQuery = new ZDBOnlySubQuery(typeof(AutoWhsWorkOrderWithRelatedOrderView), WhsWorkOrderWithRelatedOrderViewSchema.PK);
			workOrderRelatedOrderSubQuery.AddSubQuery(WhsWorkOrderWithRelatedOrderViewSchema.WRO_RelatedOrderPK, jobDocAddressSubQuery, JoinCondition.And);

			var docketSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.PK);
			docketSubQuery.AddSubQuery(workOrderRelatedOrderSubQuery, JoinCondition.And);

			return docketSubQuery;
		}

		public static ZDBOnlySubQuery GetWorkOrderConsigneeCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			return GetWorkOrderCompanyNameQuery(DocAddressTypes.Codes.ConsigneeAddress, comparisonOperator, companyName);
		}

		public static ZDBOnlySubQuery GetWorkOrderTransportCoCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			return GetWorkOrderCompanyNameQuery(TransportCoConstants.AddressTypeCode, comparisonOperator, companyName);
		}

		public static ZDBOnlySubQuery GetWorkOrderCompanyNameQuery(ZString docAddressTypeCode, SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			var jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressFieldSearchDBSubQuery(docAddressTypeCode, comparisonOperator, OrgHeaderSchema.OH_FullName, JobDocAddressSchema.E2_CompanyName, companyName);

			var workOrderRelatedOrderSubQuery = new ZDBOnlySubQuery(typeof(AutoWhsWorkOrderWithRelatedOrderView), WhsWorkOrderWithRelatedOrderViewSchema.PK);
			workOrderRelatedOrderSubQuery.AddSubQuery(WhsWorkOrderWithRelatedOrderViewSchema.WRO_RelatedOrderPK, jobDocAddressSubQuery, JoinCondition.And);

			return workOrderRelatedOrderSubQuery;
		}
	}
}
