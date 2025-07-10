using CargoWise.Schema;

namespace Enterprise.Rating.Business
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using MasterFiles.Business;
	using MasterFiles.Integration;
	using ZArchitecture.Schema;

	public class RateQueryHelper
	{
		public RateQueryHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		protected readonly BusinessObjectFactory factory;

		public ZQuery GetOrganisationQueryForQuotation(ZGuid pkValue)
		{
			return GetOrganisationQueryForQuotation(pkValue, ZString.Empty, SQLComparisonOperator.NotSpecified);
		}

		public ZQuery GetOrganisationQueryForQuotation(SQLComparisonOperator organizationNameComparisonOperator, ZString organizationName)
		{
			return GetOrganisationQueryForQuotation(ZGuid.Empty, organizationName, organizationNameComparisonOperator);
		}

		ZQuery GetOrganisationQueryForQuotation(ZGuid pkValue, ZString organizationName, SQLComparisonOperator organizationNameComparisonOperator)
		{
			var query = new ZDBOnlyQuery(typeof(RatingHeader));

			// DocAddress
			var addressCode = DocAddressTypes.GetCode(factory, DocAddressType.QuotationClientAddress);

			var docAddressesFilter = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			var orgAddressesFilter = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

			if (!pkValue.IsEmpty)
			{
				orgAddressesFilter.AddToFilter(OrgAddressSchema.OA_OH, pkValue);
			}

			if (!organizationName.IsEmpty)
			{
				orgAddressesFilter.AddSubQuery(GetOrganizationNameSubQuery(organizationNameComparisonOperator, OrgAddressSchema.OA_OH, organizationName), JoinCondition.And);
			}

			docAddressesFilter.AddSubQuery(orgAddressesFilter, JoinCondition.And);
			docAddressesFilter.AddToFilter(JobDocAddress.GetFilter(addressCode, RatingHeaderSchema.Constants.Prefix, 0), JoinCondition.And);

			//Query
			query.AddSubQuery(RatingHeaderSchema.PK, docAddressesFilter, JoinCondition.And);

			return query;
		}

		public ZDBOnlySubQuery GetOrganizationNameSubQuery(SQLComparisonOperator comparisonOperator, SchemaGuidColumn comparisonColumn, ZString organizationName)
		{
			var operatorReversed = SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref comparisonOperator);
			var result = new ZDBOnlySubQuery(typeof(OrgHeader), comparisonColumn, operatorReversed);
			result.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, organizationName);

			return result;
		}
	}
}
