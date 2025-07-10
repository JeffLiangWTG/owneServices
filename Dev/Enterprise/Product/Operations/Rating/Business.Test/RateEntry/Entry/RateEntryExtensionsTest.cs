using System.Collections.Generic;
using System.Linq;
using WiseRates.Api.Model;
using Category = Enterprise.Rating.Business.RatingConstants.RateCategory;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	internal class RateEntryExtensionsTest : RatingTestCase
	{
		public void TestIsLCLFreight()
		{
			AssertLCLFreight(Category.AIR, expectedIsLCLFreight: false);
			AssertLCLFreight(Category.FCL, expectedIsLCLFreight: false);
			AssertLCLFreight(Category.LCL, expectedIsLCLFreight: true);
			AssertLCLFreight(Category.ORG, expectedIsLCLFreight: false);
			AssertLCLFreight(Category.DST, expectedIsLCLFreight: false);

			AssertLCLFreight(Category.CAI, expectedIsLCLFreight: false);
			AssertLCLFreight(Category.CFC, expectedIsLCLFreight: false);
			AssertLCLFreight(Category.CLC, expectedIsLCLFreight: true);
			AssertLCLFreight(Category.COR, expectedIsLCLFreight: false);
			AssertLCLFreight(Category.CDS, expectedIsLCLFreight: false);

			void AssertLCLFreight(string category, bool expectedIsLCLFreight)
			{
				var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
				var rateEntry = clientRate.AddRateEntry(category);
				AssertEquals($"Category: {category}", expectedIsLCLFreight, rateEntry.IsLCLFreight());
			}
		}

		public void TestWhereRateEntryCanBeFilteredByContractNumber()
		{
			Helper.ChargeCodes.CreateGlobalCharge("GFRT");

			var clientRate = Helper.NewClientRate(NewClient);
			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(Category.AIR, Constants.RateMode.LSE, "AU", "US", "FRT", 100);

			var globalClientRate = Helper.NewGlobalClientRate(NewClient);
			var globalClientRateEntry = globalClientRate.AddRateEntryWithFlatRateLine(Category.AIR, Constants.RateMode.LSE, "AU", "US", "GFRT", 100);

			var costing = Helper.NewCosting(null);
			var costingEntry = costing.AddRateEntryWithFlatRateLine(Category.AIR, Constants.RateMode.LSE, "AU", "US", "FRT", 100);
			var costingTACTEntry = costing.AddRateEntryWithFlatRateLine(Category.AIR, Constants.RateMode.LSE, "AU", "US", "FRT", 100);
			costingTACTEntry.TI_IsTact = true;

			var globalCosting = Helper.NewGlobalCosting(null);
			var globalCostingEntry = globalCosting.AddRateEntryWithFlatRateLine(Category.AIR, Constants.RateMode.LSE, "AU", "US", "GFRT", 100);
			var globalCostingTACTEntry = globalCosting.AddRateEntryWithFlatRateLine(Category.AIR, Constants.RateMode.LSE, "AU", "US", "GFRT", 100);
			globalCostingTACTEntry.TI_IsTact = true;

			var companyTariff = Helper.NewCompanyTariff();
			var companyTariffEntry = companyTariff.AddRateEntryWithFlatRateLine(Category.AIR, Constants.RateMode.LSE, "AU", "US", "FRT", 100);

			var globalTariff = Helper.NewGlobalTariff();
			var globalTariffEntry = globalTariff.AddRateEntryWithFlatRateLine(Category.AIR, Constants.RateMode.LSE, "AU", "US", "GFRT", 100);

			var intercompanyTariff = Helper.NewIntercompanyTariff();
			var intercompanyTariffEntry = intercompanyTariff.AddRateEntryWithFlatRateLine(Category.AIR, Constants.RateMode.LSE, "AU", "US", "GFRT", 100);

			var quote = Helper.NewQuote(NewClient);
			var quoteEntry = quote.AddRateEntryWithFlatRateLine(Category.AIR, Constants.RateMode.LSE, "AU", "US", "FRT", 100);

			var wiseHeader = new WiseHeader(Factory);
			var wiseEntry = new WiseEntry((Rate)null, Factory) { ParentRatingHeader = wiseHeader };

			// All types of entries above
			var testEntries = new List<IRateEntry>
			{
				clientRateEntry,
				globalClientRateEntry,
				costingEntry,
				costingTACTEntry,
				globalCostingEntry,
				globalCostingTACTEntry,
				companyTariffEntry,
				globalTariffEntry,
				intercompanyTariffEntry,
				quoteEntry,
				wiseEntry
			};

			var expectedEntries = new IRateEntry[]
			{
				costingEntry,
				globalCostingEntry,
				wiseEntry,
			}
			.Select(e => $"{e.ParentRatingHeader.IsGlobal()}|{e.ParentRatingHeader.RateTypeSafe()}|{e.TI_IsTact}")
			.ToArray();

			var actualFiltered = testEntries
				.FilterByContractNumber(isCosting: true)
				.Select(e => $"{e.ParentRatingHeader.IsGlobal()}|{e.ParentRatingHeader.RateTypeSafe()}|{e.TI_IsTact}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"Expected entries for costing",
				expectedEntries,
				actualFiltered
			);

			expectedEntries = new IRateEntry[]
			{
				clientRateEntry,
				globalClientRateEntry,
				companyTariffEntry,
				globalTariffEntry,
				quoteEntry,
			}.Select(e => $"{e.ParentRatingHeader.IsGlobal()}|{e.ParentRatingHeader.RateTypeSafe()}|{e.TI_IsTact}")
			.ToArray();

			actualFiltered = testEntries
				.FilterByContractNumber(isCosting: false)
				.Select(e => $"{e.ParentRatingHeader.IsGlobal()}|{e.ParentRatingHeader.RateTypeSafe()}|{e.TI_IsTact}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"Expected entries for non-costing",
				expectedEntries,
				actualFiltered
			);
		}
	}
}
