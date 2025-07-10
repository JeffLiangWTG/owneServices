using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(CostsComparerEntryCollection))]
	public class CostsComparerEntryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CostsComparerEntryCollection>
	{
		[TestDate(2015, 09, 01)]
		public void TestTempTableIsCleared()
		{
			var costing = Factory.New<Costing>();
			var genericDestinationRate = costing.AddRateEntry(RatingConstants.RateCategory.DST, RatingConstants.RateCategory.FCL, "", "AUSYD");
			genericDestinationRate.TI_RateStartDate = ZDateTime.Today.Date.AddDays(-10);

			var sunderlandRate = costing.AddRateEntry(RatingConstants.RateCategory.DST, RatingConstants.RateCategory.FCL, "GBSUN", "AUSYD");
			sunderlandRate.TI_RateStartDate = ZDateTime.Today.Date;

			var shanghaiRate = costing.AddRateEntry(RatingConstants.RateCategory.DST, RatingConstants.RateCategory.FCL, "CNSHA", "AUSYD");
			shanghaiRate.TI_RateStartDate = ZDateTime.Today.Date;

			var genericOriginRate = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.FCL, "", "");
			var sydneyRate = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.FCL, "AUSYD", "");

			Factory.Save();

			var query = new ZQuery(RateEntrySchema.TI_RateCategory, RatingConstants.RateCategory.DST);
			AssertCostsComparerEntryCollection(query,
				new[] { sunderlandRate.PK, shanghaiRate.PK, genericDestinationRate.PK },
				"Expected to find all three destination rate entries");

			query.AddToFilter(RateEntrySchema.TI_RateStartDate, ZDateTime.Today.Date);

			AssertCostsComparerEntryCollection(query, new[] { sunderlandRate.PK, shanghaiRate.PK }, "Should find by date");

			query.AddToFilter(RateEntrySchema.TI_OriginLRC, "CNSHA");

			AssertCostsComparerEntryCollection(query, new[] { shanghaiRate.PK }, "Should only return shanghai origin rate");

			query = new ZQuery(RateEntrySchema.TI_RateCategory, RatingConstants.RateCategory.ORG);

			AssertCostsComparerEntryCollection(query, new[] { genericOriginRate.PK, sydneyRate.PK }, "New query looks at only origin rates");

			query.AddToFilter(RateEntrySchema.TI_OriginLRC, "CNSHA");

			AssertCostsComparerEntryCollection(query, System.Array.Empty<ZGuid>(), "No entries match this criteria");
		}

		//Test for incident #CS00682476
		public void TestLoadRateEntriesWithControllingCustomer()
		{
			var testChargeCode1 = Factory.New<AccChargeCode>();
			testChargeCode1.AC_Code = "ZUB";

			var testChargeCode2 = Factory.New<AccChargeCode>();
			testChargeCode2.AC_Code = "EVZ";

			var costing = Factory.New<Costing>();

			var controllingCustomerEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL);
			controllingCustomerEntry.RateLines.AddNew();
			var controllingCustomerRateLine1 = controllingCustomerEntry.RateLines[0];
			var controllingCustomerRateLine2 = controllingCustomerEntry.RateLines[1];

			controllingCustomerRateLine1.TL_AC = testChargeCode1.PK;
			controllingCustomerRateLine1.TL_RateCalculator = FlatCalculator.Code;
			controllingCustomerRateLine1.GetCalculator<FlatCalculator>().BaseRate = 100m;

			controllingCustomerRateLine2.TL_AC = testChargeCode2.PK;
			controllingCustomerRateLine2.TL_RateCalculator = FlatCalculator.Code;
			controllingCustomerRateLine2.GetCalculator<FlatCalculator>().BaseRate = 300m;

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomerEntry.TI_OH_ControllingCustomer = controllingCustomer.PK;

			var standardEntry = controllingCustomerEntry.DeepClone(costing.GetRateEntryCollectionForCategory("FCL"));
			standardEntry.TI_OH_ControllingCustomer = ZGuid.Empty;
			standardEntry.RateLines[0].GetCalculator<FlatCalculator>().BaseRate = 500m;
			standardEntry.RateLines[1].GetCalculator<FlatCalculator>().BaseRate = 800m;

			Factory.Save();

			var collection = new CostsComparerEntryCollection(new CostsComparer(), Factory);
			var query = new ZQuery(RateEntrySchema.TI_RateCategory, RatingConstants.RateCategory.FCL);

			collection.Load(query);

			AssertEquals("Number of CostComparerEntries", 2, collection.Count);

			var result1 = collection.Cast<CostsComparerEntry>().First(item => item.Entry.PK == controllingCustomerEntry.PK);
			AssertContainsExactElementsInAnyOrder(controllingCustomerEntry.RateLines, result1.RateLines);

			var result2 = collection.Cast<CostsComparerEntry>().First(item => item.Entry.PK == standardEntry.PK);
			AssertContainsExactElementsInAnyOrder(standardEntry.RateLines, result2.RateLines);
		}

		void AssertCostsComparerEntryCollection(ZQuery query, IEnumerable<ZGuid> expectedResults, string message = "")
		{
			var collection = new CostsComparerEntryCollection(new CostsComparer(), Factory);
			collection.Load(query);

			var results = collection.Cast<CostsComparerEntry>().Select(x => x.Entry.PK);

			AssertContainsExactElementsInAnyOrder(message, expectedResults, results);
		}

		public void TestLoadRateEntriesWithDifferentContractNumbers()
		{
			var costing = Factory.New<Costing>();

			var entry1 = costing.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "GBSUN", "FRT", 100m, "CN", container: "20GP");
			entry1.TI_ContractNumber = "111";

			var entry2 = costing.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "GBSUN", "FRT", 200m, "CN", container: "20GP");
			entry2.TI_ContractNumber = "222";

			var entry3 = costing.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "GBSUN", "FRT", 300m, "CN", container: "20GP");
			entry3.TI_ContractNumber = "333";

			Factory.Save();

			var collection = new CostsComparerEntryCollection(new CostsComparer(), Factory);
			var query = new ZQuery(RateEntrySchema.TI_RateCategory, RatingConstants.RateCategory.FCL);

			collection.Load(query);

			AssertEquals("Number of CostComparerEntries", 3, collection.Count);

			var result1 = collection.Cast<CostsComparerEntry>().Single(item => item.Entry.TI_ContractNumber == "111");
			AssertContainsExactElementsInAnyOrder(entry1.RateLines, result1.RateLines);

			var result2 = collection.Cast<CostsComparerEntry>().Single(item => item.Entry.TI_ContractNumber == "222");
			AssertContainsExactElementsInAnyOrder(entry2.RateLines, result2.RateLines);

			var result3 = collection.Cast<CostsComparerEntry>().Single(item => item.Entry.TI_ContractNumber == "333");
			AssertContainsExactElementsInAnyOrder(entry3.RateLines, result3.RateLines);
		}

		protected override CostsComparerEntryCollection GetCollectionToTest()
		{
			return new CostsComparerEntryCollection(new CostsComparer(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entry = Factory.New<RateEntry>();
			return new CostsComparerEntry(new CostsComparer(), entry, new List<RateLine>());
		}
	}
}
