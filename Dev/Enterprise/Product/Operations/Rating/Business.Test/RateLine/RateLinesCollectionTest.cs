using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.DataMapping;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class RateLinesCollectionTest : RatingTestCase
	{
		public void TestInsertAddsRateLineWhenIndexIsNotGood()
		{
			var rate = Factory.New<ClientRate>();
			var entry = rate.AIRRateEntriesForBinding.AddNew();
			var lines = entry.RateLines;
			lines.RemoveAndDeleteAll();
			lines.InsertNew(-1);
			lines.InsertNew(100);

			AssertEquals(2, lines.Count);
		}

		public void TestInsertNew()
		{
			var rate = Factory.New<ClientRate>();
			var entry = rate.AIRRateEntriesForBinding.AddNew();
			var lines = entry.RateLines;

			lines.RemoveAndDeleteAll();

			var line1 = lines.AddNew();
			var line2 = lines.AddNew();

			AssertEquals(2, lines.Count);

			var line3 = lines.InsertNew(1);
			AssertEquals(3, lines.Count);
			AssertEquals(line1, lines[0]);
			AssertEquals(line3, lines[1]);
			AssertEquals(line2, lines[2]);

			AssertEquals(entry.PK, line3.TL_TI);

			AssertEquals((byte)0, lines[0].TL_LineOrder);
			AssertEquals((byte)1, lines[1].TL_LineOrder);
			AssertEquals((byte)2, lines[2].TL_LineOrder);
		}

		// TL_LineOrder won't increment above 255, it will just remain fixed
		// at 255 for any subsequent entries
		public void TestInsertNew_MoreThan255EntriesSupported()
		{
			var rate = Factory.New<ClientRate>();
			var entry = rate.AIRRateEntriesForBinding.AddNew();
			var lines = entry.RateLines;

			lines.RemoveAndDeleteAll();

			for (int i = 0; i < 256; ++i)
			{
				var line1 = lines.InsertNew(i);
				AssertEquals("TL_LineOrder should match the index assigned to it", i, (int)line1.TL_LineOrder);
			}

			AssertEquals("Expected 256 entries in the RateLine list", 256, lines.Count);

			// insert a new RateLine at the end to overflow the byte index of TL_LineOrder
			var line = lines.InsertNew(256);
			AssertEquals("TL_LineOrder should clamp to 255 when the list size is greater than 255 items", 255, (int)line.TL_LineOrder);
			AssertEquals(257, lines.Count);
			AssertEquals(entry.PK, line.TL_TI);
		}

		public void TestRateLineIsPartOfCollection()
		{
			var testObjectFactory = new BusinessObjectFactory();
			var rateEntry = testObjectFactory.NewWithValidTestData<RateEntry>();
			var rateline1 = rateEntry.RateLines.AddNew();
			AssertEquals("There should be only one rate line", 1, rateEntry.RateLines.Count);

			var rateline2 = testObjectFactory.New<RateLine>();
			AssertEquals("Developer Exception should be raised", 1, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestRateLineItemDeletedFromCollection()
		{
			var testObjectFactory = new BusinessObjectFactory();
			var rateEntry = testObjectFactory.NewWithValidTestData<RateEntry>();
			var rateline = rateEntry.RateLines.AddNew();
			var ratelineitem1 = rateline.RateLineItems.AddNew();
			AssertEquals("There should be one RateLineItem", 1, rateline.RateLineItems.Count);

			rateline.RateLineItems.RemoveAndDeleteAll();
			AssertEquals("No Developer Exception should be raised", 0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestDataRefreshBusDisabled()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			testRate.AddRateEntry("ORG", "AIR", "", "");

			Factory.Save();

			var factory2 = new BusinessObjectFactory();

			var entryInFirstFactory = Factory.Load<ClientRate>(testRate.PK).GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];
			var entryInSecondFactory = factory2.Load<ClientRate>(testRate.PK).GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];

			AssertEquals("Precondition: No Lines in either rate", 0, entryInFirstFactory.RateLines.Count);
			AssertEquals("Precondition: No Lines in either rate", 0, entryInSecondFactory.RateLines.Count);

			entryInFirstFactory.AddRateLine("ODOC");
			Factory.Save();

			AssertEquals("Entry in first factory has 1 line", 1, entryInFirstFactory.RateLines.Count);
			AssertEquals("Entry in second factory has no lines as the collection is NOT published for datarefresh", 0, entryInSecondFactory.RateLines.Count);

			entryInSecondFactory.RateLines.Load();
			AssertEquals("Entry in first factory still has 1 line", 1, entryInFirstFactory.RateLines.Count);
			AssertEquals("Entry in second factory has 1 line as it's been reloaded", 1, entryInSecondFactory.RateLines.Count);
		}

		#region Default Value For Warehouse

		public void TestRateLineUsesActualWeightMeasure()
		{
			AssertRateLineUsesActualWeightMeasure(RatingConstants.RateCategory.WHS);
			AssertRateLineUsesActualWeightMeasure(RatingConstants.RateCategory.TRW);
			AssertRateLineUsesActualWeightMeasure(RatingConstants.RateCategory.TWU);
		}

		void AssertRateLineUsesActualWeightMeasure(string rateCategory)
		{
			var tariff1 = Factory.New<CompanyTariff>();
			var entry1 = tariff1.AddRateEntry(rateCategory);
			var line1 = entry1.RateLines.AddNew();
			Assert("Warehouse must have actual weight measure by default", line1.UseOnlyActualWeightMeasure);

			var rate1 = Factory.New<ClientRate>();
			entry1 = rate1.AddRateEntry(rateCategory);
			line1 = entry1.RateLines.AddNew();
			Assert("Warehouse must have actual weight measure by default", line1.UseOnlyActualWeightMeasure);
		}

		#endregion

		#region Currency

		public void TestCurrencySet()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.AIR, "AU", "");
			rateEntry.TI_RX_NKCurrency = "AUD";

			var rateLine1 = rateEntry.RateLines.AddNew();
			AssertEquals("Line Currency Set to AUD", "AUD", rateLine1.TL_RX_NKCurrency);

			var rateLine2 = rateEntry.RateLines.AddNew();
			AssertEquals("Line Currency Set to AUD", "AUD", rateLine2.TL_RX_NKCurrency);
		}

		#endregion

		#region ReadOnly

		public void TestRateLinesCollection_IsReadOnlyWhenMasterPublishedInAnotherCompany()
		{
			var globalTariffsIsAllowed = Env.Security.GlobalTariffRatesEditFromAnyCompany.IsAllowed;
			var globalRatesIsAllowed = Env.Security.GlobalClientRatesEditFromAnyCompany.IsAllowed;
			var globalCostsIsAllowed = Env.Security.GlobalCostingRatesEditFromAnyCompany.IsAllowed;

			try
			{
				Env.Security.GlobalTariffRatesEditFromAnyCompany.IsAllowed = false;
				Env.Security.GlobalClientRatesEditFromAnyCompany.IsAllowed = false;
				Env.Security.GlobalCostingRatesEditFromAnyCompany.IsAllowed = false;

				#region Set Up Companies and Charge Codes

				var company1 = Factory.NewWithValidTestData<GlbCompany>();
				company1.GC_Code = "NEW";
				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				company1.Branches.Add(branch1);

				var company2 = Factory.NewWithValidTestData<GlbCompany>();
				company2.GC_Code = "NUP";
				var branch2 = Factory.NewWithValidTestData<GlbBranch>();
				company2.Branches.Add(branch2);

				Factory.Save();

				AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out _, out var localFRTChargeCode, out var globalFRTChargeCode, true, false, "FRT", "FRT");

				#endregion

				var globalClientRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());
				var rateEntry1 = globalClientRate.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AU", "");
				var rateLine1 = rateEntry1.AddRateLine(globalFRTChargeCode, UnitCalculator.Code, Constants.Weight.Kilograms, Constants.CurrencyCodes.Australia);
				rateLine1.GetCalculator<UnitCalculator>().PerUnit = 8m;

				var globalCosting = Helper.NewGlobalCosting(Helper.NewOrgHeader());
				var costEntry1 = globalCosting.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "CN", "");
				var costLine1 = costEntry1.AddRateLine(globalFRTChargeCode, UnitCalculator.Code, Constants.Weight.Kilograms, Constants.CurrencyCodes.Australia);
				costLine1.GetCalculator<UnitCalculator>().PerUnit = 12m;

				var globalTariff = Factory.New<GlobalTariff>();
				var tariffEntry1 = globalTariff.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.SEA, "AUSYD", "");

				var localCosting = Helper.NewCosting(Helper.NewOrgHeader());
				var localCostEntry1 = localCosting.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "");
				var localCostLine1 = localCostEntry1.AddRateLine(localFRTChargeCode, UnitCalculator.Code, Constants.Weight.Kilograms, Constants.CurrencyCodes.Australia);
				localCostLine1.GetCalculator<UnitCalculator>().PerUnit = 12m;

				Factory.Save();

				RateEntry rateEntry2;
				RateEntry costEntry2;
				RateEntry tariffEntry2;
				RateEntry localCostEntry2;
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					rateEntry2 = globalClientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.SEA, "", "AU");
					costEntry2 = globalCosting.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "GB", "");
					tariffEntry2 = globalTariff.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.SEA, "AUMEL", "");
					localCostEntry2 = localCosting.AddRateEntry(RatingConstants.RateCategory.WHS, Core.Constants.RateMode.ALL, "", "");
					Factory.Save();

					AssertEquals(rateEntry1.RateLines.ReadOnly, true);
					AssertEquals(costEntry1.RateLines.ReadOnly, true);
					AssertEquals(tariffEntry1.RateLines.ReadOnly, true);
					AssertEquals(localCostEntry1.RateLines.ReadOnly, false);

					AssertEquals(rateEntry2.RateLines.ReadOnly, false);
					AssertEquals(costEntry2.RateLines.ReadOnly, false);
					AssertEquals(tariffEntry2.RateLines.ReadOnly, false);
					AssertEquals(localCostEntry2.RateLines.ReadOnly, false);
				}

				AssertEquals(rateEntry1.RateLines.ReadOnly, false);
				AssertEquals(costEntry1.RateLines.ReadOnly, false);
				AssertEquals(tariffEntry1.RateLines.ReadOnly, false);
				AssertEquals(localCostEntry1.RateLines.ReadOnly, false);

				AssertEquals(rateEntry2.RateLines.ReadOnly, true);
				AssertEquals(costEntry2.RateLines.ReadOnly, true);
				AssertEquals(tariffEntry2.RateLines.ReadOnly, true);
				AssertEquals(localCostEntry2.RateLines.ReadOnly, false);

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					AssertEquals(rateEntry1.RateLines.ReadOnly, true);
					AssertEquals(costEntry1.RateLines.ReadOnly, true);
					AssertEquals(tariffEntry1.RateLines.ReadOnly, true);
					AssertEquals(localCostEntry1.RateLines.ReadOnly, false);

					AssertEquals(rateEntry2.RateLines.ReadOnly, true);
					AssertEquals(costEntry2.RateLines.ReadOnly, true);
					AssertEquals(tariffEntry2.RateLines.ReadOnly, true);
					AssertEquals(localCostEntry2.RateLines.ReadOnly, false);
				}
			}
			finally
			{
				Env.Security.GlobalTariffRatesEditFromAnyCompany.IsAllowed = globalTariffsIsAllowed;
				Env.Security.GlobalClientRatesEditFromAnyCompany.IsAllowed = globalRatesIsAllowed;
				Env.Security.GlobalCostingRatesEditFromAnyCompany.IsAllowed = globalCostsIsAllowed;
			}
		}

		#endregion

		#region Company Tariff

		public void TestInheritedRateLinesLoaded()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			var tariff1 = Factory.New<CompanyTariff>();
			var entry1 = tariff1.AddRateEntry("ORG");
			var line1 = entry1.RateLines.AddNew();
			line1.TL_AC = TestCAF.PK;
			AssertEquals("RateLine set to correct tariff level", (ZByte)1, line1.TL_CompanyTariffLevel);
			Factory.Save();

			AssertEquals("1 rate line", 1, entry1.RateLines.Count);

			var factory2 = new BusinessObjectFactory();

			var tariff2 = factory2.New<CompanyTariff>();
			var entry2 = tariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];
			var line2 = entry2.RateLines.AddNew();
			line2.TL_AC = TestADF.PK;
			AssertEquals("RateLine set to correct tariff level", (ZByte)2, line2.TL_CompanyTariffLevel);
			factory2.Save();

			AssertEquals("Inherits Level 1 rate lines", entry1.RateLines.Count + 1, entry2.RateLines.Count);

			var factory3 = new BusinessObjectFactory();

			var tariff3 = factory3.New<CompanyTariff>();
			var entry3 = tariff3.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];
			var line3 = entry3.RateLines.AddNew();
			line3.TL_AC = TestAWB.PK;
			AssertEquals("RateLine set to correct tariff level", (ZByte)3, line3.TL_CompanyTariffLevel);
			factory3.Save();

			AssertEquals("Inherits Level 1 and 2 rate lines", entry2.RateLines.Count + 1, entry3.RateLines.Count);

			var factory4 = new BusinessObjectFactory();

			var tariff4 = factory4.New<CompanyTariff>();
			var entry4 = tariff4.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];
			var line4 = entry4.RateLines.AddNew();
			line4.TL_AC = TestBAF.PK;
			AssertEquals("RateLine set to correct tariff level", (ZByte)4, line4.TL_CompanyTariffLevel);
			factory4.Save();

			AssertEquals("Inherits Level 1 and 2 and 3 rate lines", entry3.RateLines.Count + 1, entry4.RateLines.Count);
		}

		public void TestTariffLevelsAndFlags()
		{
			var tariff1 = Factory.New<CompanyTariff>();
			var entry1 = tariff1.AddRateEntry("AIR");
			entry1.RateLines.RemoveAndDeleteAll();
			entry1.AddRateLine("CAF");

			Assert("Is Company Tariff", entry1.Parent.IsTariff());
			Assert("Not additional tariff", !entry1.RateLines.ParentIsAdditionalTariff);
			AssertEquals("Correct Company Tariff Header Level", (ZByte)1, entry1.Parent.TH_GlobalRateLevel);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var tariff2 = factory2.New<CompanyTariff>();
			var tariff2Entry = tariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR)[0];

			Assert("Is Company Tariff", tariff2Entry.Parent.IsTariff());
			Assert("Is additional tariff", tariff2Entry.RateLines.ParentIsAdditionalTariff);
			AssertEquals("Correct Company Tariff Header Level", (ZByte)2, tariff2Entry.Parent.TH_GlobalRateLevel);
		}

		public void TestRateLineInheritsAndOverridesAndDeletes()
		{
			var tariff1 = Factory.New<CompanyTariff>();
			var entry1 = tariff1.AddRateEntry("AIR");
			entry1.RateLines.RemoveAndDeleteAll();
			var line1 = entry1.AddRateLine("CAF");

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var tariff2 = factory2.New<CompanyTariff>();
			var tariff2Entry = tariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR)[0];

			AssertEquals("1 rate line", 1, tariff2Entry.RateLines.Count);
			AssertEquals("Tariff 1 RateLine is present in Tariff 2", line1.PK, tariff2Entry.RateLines[0].PK);
			Assert("Tariff 1 RateLine is present in Tariff 2 readonly", tariff2Entry.RateLines[0].ReadOnly);
			AssertEquals("Tariff 1 RateLine is present in Tariff 2 with correct charge code", "CAF", tariff2Entry.RateLines[0].ChargeCode.AC_Code);
			Assert("Tariff 1 RateLine is flagged as inherited on tariff 2", tariff2Entry.RateLines[0].IsTariffLineInherited);

			tariff2Entry.RateLines.OverrideTariffLines(new[] { tariff2Entry.RateLines[0] });

			Assert("Tariff 1 RateLine is NOT present in Tariff 2", line1.PK != tariff2Entry.RateLines[0].PK);
			Assert("Overridden line is not readonly", !tariff2Entry.RateLines[0].ReadOnly);
			AssertEquals("Tariff 2 has rate line with correct charge code", "CAF", tariff2Entry.RateLines[0].ChargeCode.AC_Code);
			AssertEquals("Tariff 2 has comp tariff based calc", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, tariff2Entry.RateLines[0].TL_RateCalculator);
			Assert("Tariff 2 RateLine is NOT flagged as inherited", !tariff2Entry.RateLines[0].IsTariffLineInherited);

			tariff2Entry.RateLines.RemoveAndDelete(tariff2Entry.RateLines[0]);
			AssertEquals("Tariff 1 RateLine is present in Tariff 2", line1.PK, tariff2Entry.RateLines[0].PK);
			Assert("Tariff 1 RateLine is present in Tariff 2 readonly", tariff2Entry.RateLines[0].ReadOnly);
			AssertEquals("Tariff 1 RateLine is present in Tariff 2 with correct charge code", "CAF", tariff2Entry.RateLines[0].ChargeCode.AC_Code);
			Assert("Tariff 1 RateLine is flagged as inherited on tariff 2", tariff2Entry.RateLines[0].IsTariffLineInherited);
		}

		public void TestOveriddingTariffLinesClearsFeesAndCharges()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			var tariff1 = Factory.New<CompanyTariff>();
			var tariff1Entry = tariff1.AddRateEntry("ORG");
			var tariff1Line = tariff1Entry.RateLines.AddNew();
			tariff1Line.TL_AC = TestFRT.PK;
			tariff1Line.TL_FeeChargeType = "FSE";
			tariff1Line.TL_FeeChargeLevel = "STD";

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var tariff2 = newFactory.New<CompanyTariff>();
			var tariff2Entry = tariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];
			var tariff2Line = tariff2Entry.RateLines.Cast<RateLine>().FirstOrDefault(x => x.TL_AC == TestFRT.PK);

			AssertEquals("Pre-condition", true, tariff2Line.IsTariffLineInherited);
			AssertEquals("Expected to have copied values across", "FSE", tariff2Line.TL_FeeChargeType);
			AssertEquals("Expected to have copied values across", "STD", tariff2Line.TL_FeeChargeLevel);

			tariff2Entry.RateLines.OverrideTariffLines(new[] { tariff2Line });
			var clonedTariff2Line = tariff2Entry.RateLines.Cast<RateLine>().FirstOrDefault(x => x.TL_AC == TestFRT.PK);

			AssertEquals("Expected to no longer be considered inherited if overriden", false, clonedTariff2Line.IsTariffLineInherited);
			Assert("Expected to have cleared fees and charges if overriden", clonedTariff2Line.TL_FeeChargeType.IsEmpty);
			Assert("Expected to have cleared fees and charges if overriden", clonedTariff2Line.TL_FeeChargeLevel.IsEmpty);
		}

		#endregion

		public void TestIImportCollectionElementMatchingSupporter_Defaults()
		{
			var org = Helper.NewOrgHeader("test1");
			var costing = Helper.NewCosting(org);
			var rateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.FCL, "AUSYD", "NZAKL", "FRT", 222.22);

			var matcher = rateEntry.RateLines as IImportCollectionElementMatchingSupporter;
			AssertEquals(string.Empty, matcher.MatchingColumnName);
			AssertEquals(false, matcher.IsGenericColumnMatchingAllowed);
			AssertNull(matcher.GetMatchingBizObject("whatever"));
			AssertNull(matcher.GetMatchingBizObject(string.Empty));
		}
	}

	[TestedType(typeof(RateLinesCollection))]
	public class RateLinesBizObjCollectionTest : BizObjectCollectionAddDeleteTestCase
	{
		protected override BusinessObjectCollection GetCollection()
		{
			return new RateLinesCollection(Factory.New<RateEntry>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Collection.AddNew();
		}
	}
}
