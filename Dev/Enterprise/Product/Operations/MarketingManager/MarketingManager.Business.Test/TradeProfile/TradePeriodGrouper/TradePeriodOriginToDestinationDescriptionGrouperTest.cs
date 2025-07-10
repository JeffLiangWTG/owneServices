using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradePeriodOriginToDestinationDescriptionGrouperTest : TradePeriodGrouperTestCase
	{
		public void TestGetGroupings()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var australia = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			var nswQuery = new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, australia.RN_Code);
			nswQuery.AddToFilter(RefCountryStatesSchema.RW_Code, "NSW");
			var nsw = Factory.LoadTop1<RefCountryStates>(nswQuery);

			var sydneyCityQuery = new ZQuery(RefCityTownSchema.R9_RN_NKCountry, "AU");
			sydneyCityQuery.AddToFilter(RefCityTownSchema.R9_RW_NKState, "NSW");
			sydneyCityQuery.AddToFilter(RefCityTownSchema.R9_InternationalName, "Sydney");
			var sydneyCity = Factory.LoadTop1<RefCityTown>(sydneyCityQuery);

			var sydneyUnloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			var period1 = NewTradePeriodWithOriginAndDestination(org.PK, australia, sydneyCity);
			var period2 = NewTradePeriodWithOriginAndDestination(org.PK, australia, sydneyUnloco);
			var period3 = NewTradePeriodWithOriginAndDestination(org.PK, sydneyCity, sydneyUnloco);
			var period4 = NewTradePeriodWithOriginAndDestination(org.PK, australia, nsw);
			var period5 = NewTradePeriodWithOriginAndDestination(org.PK, australia, null);
			var period6 = NewTradePeriodWithOriginAndDestination(org.PK, null, australia);

			Factory.Save();

			var grouper = new TradePeriodOriginToDestinationDescriptionGrouper();
			var anotherFactory = new BusinessObjectFactory();
			var tradeDetails = GetInAnotherFactory(anotherFactory, new[] { period1, period2, period3, period4, period5, period6 });
			anotherFactory.ResetDatabaseLoadCount();
			grouper.AddFetchHints(tradeDetails);
			var groupings = grouper.GetGroupings(tradeDetails);

			AssertContainsExactElementsInAnyOrder(
				new ZString[]
				{
					"Australia -> Sydney, NSW",
					"Australia -> SYDNEY, NSW",
					"SYDNEY, NSW -> Sydney, NSW",
					"Australia -> AU, New South Wales",
					"Australia -> (Unknown)",
					"(Unknown) -> Australia",
				},
				groupings.Select(x => x.GroupKey));

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period2 },
				groupings.Single(x => x.GroupKey == "Australia -> Sydney, NSW").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period1 },
				groupings.Single(x => x.GroupKey == "Australia -> SYDNEY, NSW").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period3 },
				groupings.Single(x => x.GroupKey == "SYDNEY, NSW -> Sydney, NSW").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period4 },
				groupings.Single(x => x.GroupKey == "Australia -> AU, New South Wales").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period5 },
				groupings.Single(x => x.GroupKey == "Australia -> (Unknown)").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period6 },
				groupings.Single(x => x.GroupKey == "(Unknown) -> Australia").GroupedTradePeriods);

			var expectedDbHits = new Dictionary<string, int>
			{
				{ OrgSalesSchema.Constants.TableName, 1 },
				{ OrgTradeDetailSchema.Constants.TableName, 1 },
				{ ViewLocationSchema.Constants.TableName, 1 }
			};
			AssertDbHits(expectedDbHits, anotherFactory);
		}
	}
}
