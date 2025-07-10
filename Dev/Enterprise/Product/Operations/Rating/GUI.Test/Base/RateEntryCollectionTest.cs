using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Rating.GUI.Testing;

class RateEntryCollectionTest : TestCaseWithFactory
{
	public void TestLoadAndSortForGUI_RateLineFilter()
	{
		var clientRate = Helper.NewClientRate(NewClient);
		var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, origin: "AUSYD", removeLines: true);
		var entry1line1 = entry1.AddFlatRateLine("FRT", 100);
		entry1line1.TL_Condition = RateLineConditions.OwnBrokerage;
		var entry1line2 = entry1.AddFlatRateLine("FRT", 100);
		entry1line2.TL_Condition = RateLineConditions.OwnControllingAgent;

		var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, origin: "AUMEL", removeLines: true);
		var entry2line1 = entry2.AddFlatRateLine("FRT", 100);
		entry2line1.TL_Condition = RateLineConditions.DangerousGoods;
		var entry2line2 = entry2.AddFlatRateLine("FRT", 100);
		entry2line2.TL_Condition = RateLineConditions.OwnControllingAgent;

		Factory.Save();

		var entries = clientRate.EntryCollections[RatingConstants.RateCategory.ORG].LazyLoadingCollection;
		var filterBiz = new RateEntryFilterStripBusinessObject(entries);
		var lineFilter = (ModuleTextFilter)filterBiz[RateLineModuleFilters.Constants.Codes.Condition];
		lineFilter.IsActive = true;
		lineFilter.Property = RateLineConditions.OwnControllingAgent;

		entries.LoadAndSortForGUI();
		CombineAssertions(() =>
		{
			AssertEquals("entry1 match", true, entries.Any(x => x == entry1));
			AssertEquals("entry2 match", true, entries.Any(x => x == entry2));
			var actualLines = entry1.FilteredRateLinesForBinding.Cast<RateLine>().ToList();
			AssertEquals("line1 no match", 0, actualLines.Where(x => x == entry1line1).Count());
			AssertEquals("line2 match", 1, actualLines.Where(x => x == entry1line2).Count());
		});

		lineFilter.Property = RateLineConditions.OwnBrokerage;
		entries.LoadAndSortForGUI();
		CombineAssertions(() =>
		{
			AssertEquals("entry1 match", true, entries.Any(x => x == entry1));
			AssertEquals("entry2 no match", false, entries.Any(x => x == entry2));
			var actualLines = entry1.FilteredRateLinesForBinding.Cast<RateLine>().ToList();
			AssertEquals("line1 match", 1, actualLines.Where(x => x == entry1line1).Count());
			AssertEquals("line2 no match", 0, actualLines.Where(x => x == entry1line2).Count());
		});

		lineFilter.Property = RateLineConditions.DangerousGoods;
		entries.LoadAndSortForGUI();
		AssertEquals("LoadAndSortForGUI doesn't load lines", false, entry2.IsFilteredRateLinesForBindingLoaded_ForTest);
		CombineAssertions(() =>
		{
			AssertEquals("entry1 no match", false, entries.Any(x => x == entry1));
			AssertEquals("entry2 match", true, entries.Any(x => x == entry2));
			var actualLines = entry2.FilteredRateLinesForBinding.Cast<RateLine>().ToList();
			AssertEquals("entry2line1 match", 1, actualLines.Where(x => x == entry2line1).Count());
			AssertEquals("entry2line2 no match", 0, actualLines.Where(x => x == entry2line2).Count());
		});
	}

	#region Helpers

	protected TestHelper Helper
	{
		get { return helper ?? (helper = new TestHelper(Factory)); }
	}
	TestHelper helper;

	#endregion

	#region New Client

	OrgHeader fNewClient;
	public OrgHeader NewClient
	{
		get
		{
			if (fNewClient == null)
			{
				fNewClient = Factory.New<OrgHeader>();
				fNewClient.OH_Code = "NTC1";
				fNewClient.OH_FullName = "New Test Client";
				fNewClient.OH_RL_NKClosestPort = "AUSYD";

				fNewClient.OH_IsDebtor = true;
				fNewClient.OH_IsConsignor = true;
				fNewClient.OH_IsConsignee = true;

				var address = fNewClient.MainAddress;
				address.OA_Address1 = "123 Fake Street";
				address.OA_City = "Sydney";
				address.OA_State = "NSW";
				address.OA_PostCode = "2000";
			}
			return fNewClient;
		}
	}

	#endregion
}
