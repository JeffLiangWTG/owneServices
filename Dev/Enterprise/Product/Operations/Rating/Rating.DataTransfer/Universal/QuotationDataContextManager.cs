using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.DataTransfer
{
	public class QuotationDataContextManager : RatingHeaderDataContextManager<Quote>
	{
		protected override DataContextType GetDataContextTypeCore() => DataContextType.Quotation;

		protected override ZQuery AddAdditionalMatchingQuery(ZDBOnlyQuery query, ZString[] contextKey)
		{
			// On a quotation and OOQ, we have client, but we don't store it in TH_OH (results in a null field).
			// The linking is the other way around. We keep the RatingHeader in JobDocAddress (E2) table linking through OrgAddress.

			// E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH = pk)
			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK, JobDocAddressSchema.E2_OA_Address);
			orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, new ZGuid(contextKey[0]));

			// TH_PK IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE .... above)
			var jobDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, RatingHeaderSchema.PK);
			jobDocAddressSubQuery.AddSubQuery(orgAddressQuery, JoinCondition.And);

			query.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);
			query.AddToFilter(RatingHeaderSchema.TH_QuoteNumber, SQLComparisonOperator.Equal, contextKey[3]);

			return query;
		}
	}
}


