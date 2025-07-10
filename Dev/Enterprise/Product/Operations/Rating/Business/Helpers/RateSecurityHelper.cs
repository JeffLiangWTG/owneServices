using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Rating.Business
{
	public sealed class RateSecurityHelper : MasterFiles.Business.RateSecurityHelper
	{
		RateSecurityHelper()
		{
		}

		public static SecurityCheckResult GetFirstDeniedSecurityCheckPoint(ZGuid[] rateEntryPKs, ZGuid[] ratingHeaderPKs, BusinessObjectFactory factory)
		{
			var ratingHeadersExist = ratingHeaderPKs.Any();
			var ratingEntriesExist = rateEntryPKs.Any();

			if (!ratingHeadersExist && !ratingEntriesExist)
			{
				return null;
			}

			var cacheKey = Invariant($"RateSecurity|TI:{GetPKsAsString(rateEntryPKs)}|TH:{GetPKsAsString(ratingHeaderPKs)}"); // Hard-coded constant for cache key

			return factory.GetCachedValue(cacheKey, delegate
			{
				var ohSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);

				if (ratingHeadersExist)
				{
					var quoteOrgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);

					var quoteDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_OA_Address);
					var addressType = DocAddressTypes.GetCode(factory, DocAddressType.QuotationClientAddress);
					quoteDocAddressSubQuery.AddToFilter(JobDocAddress.GetFilter(addressType, RatingHeaderSchema.Constants.Prefix, 0), JoinCondition.And);

					var headerSubQuery = new ZDBOnlySubQuery(typeof(RatingHeader), RatingHeaderSchema.TH_OH);

					quoteDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_ParentID, ratingHeaderPKs);
					headerSubQuery.AddToFilter(RatingHeaderSchema.PK, ratingHeaderPKs);

					quoteOrgAddressSubQuery.AddSubQuery(quoteDocAddressSubQuery, JoinCondition.And);

					ohSubQuery.AddSubQuery(quoteOrgAddressSubQuery, JoinCondition.Or);
					ohSubQuery.AddSubQuery(headerSubQuery, JoinCondition.Or);
				}

				if (ratingEntriesExist)
				{
					var consigneeSubQuery = new ZDBOnlySubQuery(typeof(RateEntry), RateEntrySchema.TI_OH_Consignee);
					var consignorSubQuery = new ZDBOnlySubQuery(typeof(RateEntry), RateEntrySchema.TI_OH_Consignor);
					var supplierSubQuery = new ZDBOnlySubQuery(typeof(RateEntry), RateEntrySchema.TI_OH_Supplier);
					consigneeSubQuery.AddToFilter(RateEntrySchema.PK, rateEntryPKs);
					consignorSubQuery.AddToFilter(RateEntrySchema.PK, rateEntryPKs);
					supplierSubQuery.AddToFilter(RateEntrySchema.PK, rateEntryPKs);
					ohSubQuery.AddSubQuery(consigneeSubQuery, JoinCondition.Or);
					ohSubQuery.AddSubQuery(consignorSubQuery, JoinCondition.Or);
					ohSubQuery.AddSubQuery(supplierSubQuery, JoinCondition.Or);
				}

				return GetFirstDeniedSecurityCheckPoint(ohSubQuery, factory);
			});
		}

		static string GetPKsAsString(IEnumerable<ZGuid> pks)
		{
			return pks.Any() ? string.Join("", pks.Distinct().OrderBy(x => x).Select(x => x.ToString().Replace("-", ""))) : "";
		}
	}
}

