using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradePeriodDestinationStateGrouperTest : TradePeriodGrouperTestCase
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

			var grouper = new TradePeriodDestinationStateGrouper();
			var anotherFactory = new BusinessObjectFactory();
			var tradePeriods = GetInAnotherFactory(anotherFactory, new[] { period1, period2, period3, period4, period5, period6 });
			anotherFactory.ResetDatabaseLoadCount();
			grouper.AddFetchHints(tradePeriods);

			using (RowFactory.SetCachedTables()) // do not cache RefCountry to test fetch hints are working
			{
				var groupings = grouper.GetGroupings(tradePeriods);

				AssertContainsExactElementsInAnyOrder(
					new ZString[]
				{
					"Destination: AU, South Australia",
					"Destination: IT, Salerno",
					"Destination: (Unknown)",
				},
					groupings.Select(x => x.GroupKey));

				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
					new[] { period1, period3, period6 },
					groupings.Single(x => x.GroupKey == "Destination: AU, South Australia").GroupedTradePeriods);

				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
					new[] { period2, period4 },
					groupings.Single(x => x.GroupKey == "Destination: IT, Salerno").GroupedTradePeriods);

				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
					new[] { period5 },
					groupings.Single(x => x.GroupKey == "Destination: (Unknown)").GroupedTradePeriods);

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
