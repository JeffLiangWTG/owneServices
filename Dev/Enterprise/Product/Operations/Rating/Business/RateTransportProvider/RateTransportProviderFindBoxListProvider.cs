using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	class RateTransportProviderFindBoxListProvider : FindBoxListProvider
	{
		public RateTransportProviderFindBoxListProvider(BusinessObjectCollection collection)
			: base(collection)
		{
		}

		protected override void AddCodeEqualsFilter(ZQuery query, string code)
		{
			string[] codes = code.Split('-');
			if (codes.Length == 3)
			{
				string relatedParty = codes[0].ToUpper();
				string country = codes[1].ToUpper();
				string zoneType = codes[2].ToUpper();
				if (relatedParty == Res.GetString("62e5a064-615c-4a62-9c5e-836dfb23683f", "Generic Zone").ToUpper())
				{
					query.AddToFilter(JoinCondition.And, RateTransportProviderSchema.TP_OH_RelatedParty, SQLComparisonOperator.Equal, null);
				}
				else
				{
					var relatedPartyQuery = new ZDBOnlyQuery(typeof(RateTransportProvider));
					var subQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
					subQuery.AddToFilter(OrgHeaderSchema.OH_Code, relatedParty);
					relatedPartyQuery.AddSubQuery(RateTransportProviderSchema.TP_OH_RelatedParty, subQuery, JoinCondition.And);
					query.AddToFilter(relatedPartyQuery);
				}
				query.AddToFilter(JoinCondition.And, RateTransportProviderSchema.TP_RN_NKCountry, SQLComparisonOperator.Equal, country);
				query.AddToFilter(JoinCondition.And, RateTransportProviderSchema.TP_ZoneType, SQLComparisonOperator.Equal, zoneType);
			}
			else
			{
				query.IsNoResultQuery = true;
			}
		}
	}
}
