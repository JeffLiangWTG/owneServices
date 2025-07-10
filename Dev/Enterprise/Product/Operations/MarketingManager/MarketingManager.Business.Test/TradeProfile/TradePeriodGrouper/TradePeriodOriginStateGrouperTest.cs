using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradePeriodOriginStateGrouperTest : TradePeriodGrouperTestCase
	{
		public void TestGetGroupings()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var australia = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var italy = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "IT");

			var southAustraliaQuery = new ZQuery(RefCountryStatesSchema.RW_Code, "SA");
			southAustraliaQuery.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, australia.RN_Code);
			var southAustralia = Factory.LoadTop1<RefCountryStates>(southAustraliaQuery);

			var salernoQuery = new ZQuery(RefCountryStatesSchema.RW_Code, "SA");
			salernoQuery.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, italy.RN_Code);
			var salern = Factory.LoadTop1<RefCountryStates>(salernoQuery);

			var period1 = NewTradePeriodForStates(org.PK, southAustralia, southAustralia);
			var period2 = NewTradePeriodForStates(org.PK, southAustralia, salern);
			var period3 = NewTradePeriodForStates(org.PK, southAustralia, southAustralia);
			var period4 = NewTradePeriodForStates(org.PK, salern, salern);
			var period5 = NewTradePeriodForStates(org.PK, southAustralia, null);
			var period6 = NewTradePeriodForStates(org.PK, null, southAustralia);

			Factory.Save();

			var grouper = new TradePeriodOriginStateGrouper();
			var anotherFactory = new BusinessObjectFactory();
			var tradeDetails = GetInAnotherFactory(anotherFactory, new[] { period1, period2, period3, period4, period5, period6 });
			anotherFactory.ResetDatabaseLoadCount();
			grouper.AddFetchHints(tradeDetails);

			using (RowFactory.SetCachedTables()) // do not cache RefCountry to test fetch hints are working
			{
				var groupings = grouper.GetGroupings(tradeDetails);

				AssertContainsExactElementsInAnyOrder(
					new ZString[]
					{
						"Origin: AU, South Australia",
						"Origin: IT, Salerno",
						"Origin: (Unknown)",
					},
					groupings.Select(x => x.GroupKey));

				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
					new[] { period1, period2, period3, period5 },
					groupings.Single(x => x.GroupKey == "Origin: AU, South Australia").GroupedTradePeriods);

				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
					new[] { period4 },
					groupings.Single(x => x.GroupKey == "Origin: IT, Salerno").GroupedTradePeriods);

				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
					new[] { period6 },
					groupings.Single(x => x.GroupKey == "Origin: (Unknown)").GroupedTradePeriods);

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
}
