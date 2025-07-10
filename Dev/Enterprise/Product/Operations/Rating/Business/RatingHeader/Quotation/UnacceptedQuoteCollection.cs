using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// A collection of all quotes for a specific client that are:
	///		- Not accepted
	///		- Not expired
	///		- Not cancelled
	/// </summary>
	public class UnacceptedQuotesCollection : QuoteCollection
	{
		public UnacceptedQuotesCollection(BusinessObjectFactory factory, GlbCompany company, ZGuid organisationPK)
			: base(factory, company)
		{
			currentOrgPK = organisationPK;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			// DocAddress
			var addressCode = DocAddressTypes.GetCode(Factory, DocAddressType.QuotationClientAddress);

			var docAddressesFilter = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			var orgAddressesFilter = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

			orgAddressesFilter.AddToFilter(OrgAddressSchema.OA_OH, currentOrgPK);
			docAddressesFilter.AddSubQuery(orgAddressesFilter, JoinCondition.And);
			docAddressesFilter.AddToFilter(JobDocAddress.GetFilter(addressCode, RatingHeaderSchema.Constants.Prefix, 0), JoinCondition.And);

			var unacceptedFilter = new ZDBOnlyQuery(typeof(Quote));
			unacceptedFilter.AddSubQuery(RatingHeaderSchema.PK, docAddressesFilter, JoinCondition.And);

			unacceptedFilter.AddToFilter(RatingHeaderSchema.TH_GC, Company.PK);
			unacceptedFilter.AddToFilter(RatingHeaderSchema.TH_Accepted, null);
			unacceptedFilter.AddToFilter(RatingHeaderSchema.TH_QuoteEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
			unacceptedFilter.AddToFilter(RatingHeaderSchema.TH_IsCancelled, ZBool.False);
			unacceptedFilter.AddToFilter(RatingHeaderSchema.TH_OneTimeQuote, ZBool.False);

			return unacceptedFilter;
		}

		readonly ZGuid currentOrgPK;
	}
}

