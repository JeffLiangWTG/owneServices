using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class JobDocAddressQueryHelper
	{
		#region JobDocAddressFieldSearchDBSubQuery

		public static ZDBOnlySubQuery JobDocAddressFieldSearchDBSubQuery(ZString docAddressTypeCode, SQLComparisonOperator comparisonOperator, SchemaStringColumn orgColumn, SchemaStringColumn docAddColumnm, ZString paramValue)
		{
			var jobDocAddressSubQuery = JobDocAddressFieldSearchDBSubQuery(comparisonOperator, orgColumn, docAddColumnm, paramValue);
			jobDocAddressSubQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.Equal, docAddressTypeCode);
			return jobDocAddressSubQuery;
		}

		public static ZDBOnlySubQuery JobDocAddressFieldSearchDBSubQuery(SQLComparisonOperator comparisonOperator, SchemaStringColumn orgColumn, SchemaStringColumn docAddColumnm, ZString paramValue)
		{
			var notIn = SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref comparisonOperator);
			var jobDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn);

			var orgAddresses = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			if (orgColumn.Name.StartsWith(OrgHeaderSchema.Constants.Prefix, System.StringComparison.Ordinal))
			{
				var orgHeaders = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
				orgHeaders.AddToFilter(orgColumn, comparisonOperator, paramValue);
				orgAddresses.AddSubQuery(orgHeaders, JoinCondition.And);
			}
			else
			{
				orgAddresses.AddToFilter(orgColumn, comparisonOperator, paramValue);
			}
			jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressOverride, false);
			jobDocAddressSubQuery.AddSubQuery(orgAddresses, JoinCondition.And);

			// override
			var queryForOverride = new ZQuery();
			queryForOverride.AddToFilter(JobDocAddressSchema.E2_AddressOverride, true);
			queryForOverride.AddToFilter(JoinCondition.And, docAddColumnm, comparisonOperator, paramValue);
			jobDocAddressSubQuery.AddToFilter(queryForOverride, JoinCondition.Or);

			return jobDocAddressSubQuery;
		}

		#endregion

		#region JobDocAddressOrgHeaderParentSubQuery

		public static ZDBOnlySubQuery JobDocAddressOrgHeaderParentSubQuery(ZString docAddressTypeCode, ZGuid orgHeaderPK)
			=> JobDocAddressOrgHeaderParentSubQuery(docAddressTypeCode, orgHeaderPK, SQLComparisonOperator.Equal);

		public static ZDBOnlySubQuery JobDocAddressOrgHeaderParentSubQuery(ZGuid orgHeaderPK)
			=> JobDocAddressOrgHeaderParentSubQuery(string.Empty, orgHeaderPK, SQLComparisonOperator.Equal);

		public static ZDBOnlySubQuery JobDocAddressOrgHeaderParentSubQuery(ZString docAddressTypeCode, ZGuid orgHeaderPK, SQLComparisonOperator comparisonOperator)
		{
			var isBlank = comparisonOperator == SpecialComparisonOperator.IsBlank;
			var isNotBlank = comparisonOperator == SpecialComparisonOperator.IsNotBlank;

			var jobDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn: isBlank);
			if (!docAddressTypeCode.IsEmpty)
			{
				jobDocAddressSubQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.Equal, docAddressTypeCode);
			}

			if (!isBlank && !isNotBlank)
			{
				var orgAddresses = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
				var orgHeaders = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
				orgHeaders.AddToFilter(OrgHeaderSchema.PK, comparisonOperator, orgHeaderPK);
				orgAddresses.AddSubQuery(orgHeaders, JoinCondition.And);

				jobDocAddressSubQuery.AddSubQuery(orgAddresses, JoinCondition.And);
			}

			return jobDocAddressSubQuery;
		}

		#endregion
	}
}
