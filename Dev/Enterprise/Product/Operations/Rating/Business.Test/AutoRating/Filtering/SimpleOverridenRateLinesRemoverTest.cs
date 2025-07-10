using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	public class SimpleOverriddenRateLinesRemoverTest : RatingTestCase
	{
		public void TestTransportProviderCostsVsGenericCosts()
		{
			var genericCosting = Helper.NewCosting(null);
			var genericEntry = genericCosting.AddRateEntry("FCL", "SEA", "CNSHA", "AUFRE", "STD", "20GP");
			genericEntry.TI_RH_NKCommodityCode = "GEN";
			genericEntry.TI_OH_TransportProvider = Helper.NewOrgHeader().PK;
			var line1 = genericEntry.RateLines[0];
			line1.GetCalculator<UnitCalculator>().PerUnit = 1000m;

			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var costEntry = costing.AddRateEntry("FCL", "SEA", "CNSHA", "AUFRE", "STD", "20GP");
			costEntry.TI_RH_NKCommodityCode = "GEN";
			var line2 = costEntry.RateLines[0];
			line2.GetCalculator<UnitCalculator>().PerUnit = 1200m;

			AssertGreaterThan(new SimpleOverriddenRateLinesRemover(null).CompareForTest(line1, line2, out _), 0);
			AssertLessThan(new SimpleOverriddenRateLinesRemover(null).CompareForTest(line2, line1, out _), 0);
		}

		public void TestRemove()
		{
			var remover = new SimpleOverriddenRateLinesRemover(null);

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line1 = testRate.AddRateEntry("ORG", "AIR", "AU", "US").AddRateLine("ODOC");
			var line2 = testRate.AddRateEntry("ORG", "ALL", "AUSYD", "USLAX").AddRateLine("ODOC");
			var line3 = testRate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX", "STD", "").AddRateLine("ODOC");
			var line4 = testRate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX", "", "").AddRateLine("ODOC");

			var list = new List<IRateLine>();
			list.Add(line1);
			list.Add(line2);
			list.Add(line3);
			list.Add(line4);

			AssertEquals(4, list.Count);
			remover.Remove(list);
			AssertEquals(1, list.Count);
			AssertEquals(line3, list[0]);
		}

		public void TestRemove_FMCTariffID()
		{
			var remover = new SimpleOverriddenRateLinesRemover(null);
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entrySpecific = testRate.AddRateEntry("ORG", "AIR", "AU", "US");
			entrySpecific.TI_FMCTariffID = "aaaa";
			var lineSpecific = entrySpecific.AddRateLine("BAF");

			var entryGeneric = testRate.AddRateEntry("ORG", "AIR", "AU", "US");
			entryGeneric.TI_FMCTariffID = string.Empty;
			var lineGeneric = entryGeneric.AddRateLine("BAF");

			var list1 = new List<IRateLine>() { lineSpecific, lineGeneric };
			var list2 = new List<IRateLine>() { lineGeneric, lineSpecific };
			remover.Remove(list1);
			remover.Remove(list2);

			var expected = new[] { lineSpecific };

			AssertContainsExactElementsInAnyOrder(
				"List1 should only contain the specific line after removal.",
				expected,
				list1
			);
			AssertContainsExactElementsInAnyOrder(
				"List2 should only contain the specific line after removal.",
				expected,
				list2
			);
		}

		[StressTest]
		public void TestRemove_ManyWithDifferentChargeCode()
		{
			var mockRemover = new Mock<SimpleOverriddenRateLinesRemover>(null);
			mockRemover.CallBase = true;

			var remover = mockRemover.Object;
			var list = new List<IRateLine>();
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntryAIR_AU_US = testRate.AddRateEntry("ORG", "AIR", "AU", "US");
			var rateEntryAll_AUSYD_USLAX = testRate.AddRateEntry("ORG", "ALL", "AUSYD", "USLAX");
			var rateEntryAIR_AUSYD_USLAX_STD = testRate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX", "STD", "");
			var rateEntryAIR_AUSYD_USLAX = testRate.AddRateEntry("ORG", "ALL", "AUSYD", "USLAX", "", "");

			for (int i = 0; i < 1500; i++)
			{
				var chargeCode = $"ORG{i}";
				Helper.ChargeCodes[chargeCode].AC_ChargeGroup = "ORG";
				list.Add(rateEntryAIR_AU_US.AddRateLine(chargeCode));
				list.Add(rateEntryAll_AUSYD_USLAX.AddRateLine(chargeCode));
				list.Add(rateEntryAIR_AUSYD_USLAX_STD.AddRateLine(chargeCode));
				list.Add(rateEntryAIR_AUSYD_USLAX.AddRateLine(chargeCode));
			}

			AssertEquals(6000, list.Count);
			remover.Remove(list);
			AssertEquals(1500, list.Count);

			mockRemover.Verify(r => r.IsComparable(It.IsAny<FastLine>(), It.IsAny<FastLine>()), Times.AtMost(4500));
			mockRemover.Verify(r => r.Compare(It.IsAny<FastLine>(), It.IsAny<FastLine>(), out It.Ref<BaseRateLineComparer>.IsAny), Times.AtMost(4500));
		}

		public void TestCompare_WhenIdenticalOnly()
		{
			var client = Helper.NewOrgHeader(1);
			var remover = new SimpleOverriddenRateLinesRemover(new TestRatingCriteria("AUSYD", "USLAX", FreightMode.AIR, 100, 2, client));

			var list = new List<IRateLine>();
			var rate = Helper.NewClientRate(client);
			list.Add(rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX").AddRateLine("ODOC"));
			list.Add(rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX").AddRateLine("ODOC"));

			AssertEquals(2, list.Count);
			remover.Remove(list);

			var reason = "When two identical RateLines are seen by the remover, existing functionality does not remove it." +
				" It is not expected for this case to actually happen in real life, since, there should be validation in the UI" +
				" and in the database to prevent these scenarios from occurring.";
			AssertEquals(reason, 2, list.Count);
		}

		public void TestCompare()
		{
			var client = Helper.NewOrgHeader(1);
			var remover = new SimpleOverriddenRateLinesRemover(new TestRatingCriteria("AUSYD", "USLAX", FreightMode.AIR, 100, 2, client));

			var rate = Helper.NewClientRate(client);
			var rateLine1 = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX").AddRateLine("ODOC");

			var quote = Helper.NewQuote(client);
			var quoteLine = quote.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX").AddRateLine("ODOC");

			var tariff = Helper.NewCompanyTariff();
			var tariffLine = tariff.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX").AddRateLine("ODOC");

			BaseRateLineComparer comparer;

			Assert(remover.CompareForTest(tariffLine, rateLine1, out comparer) < 0);
			Assert(remover.CompareForTest(tariffLine, quoteLine, out comparer) < 0);
			Assert(remover.CompareForTest(rateLine1, quoteLine, out comparer) < 0);
			Assert(remover.CompareForTest(quoteLine, rateLine1, out comparer) > 0);

			var rateLine2 = rate.AddRateEntry("ORG", "AIR", "AU", "US").AddRateLine("ODOC");
			Assert(remover.CompareForTest(rateLine2, rateLine1, out comparer) < 0);
			Assert(remover.CompareForTest(rateLine1, rateLine2, out comparer) > 0);

			var rateLine3 = rate.AddRateEntry("ORG", "ALL", "AUSYD", "USLAX", "", "").AddRateLine("ODOC");
			rateLine3.Parent.TI_RH_NKCommodityCode = ZString.Empty;
			Assert(remover.CompareForTest(rateLine3, rateLine1, out comparer) < 0);
			Assert(remover.CompareForTest(rateLine1, rateLine3, out comparer) > 0);

			var rateLine4 = rate.AddRateEntry("ORG", "ALL", "AUSYD", "USLAX", "", "").AddRateLine("ODOC");
			rateLine4.Parent.TI_RH_NKCommodityCode = ZString.Empty;

			AssertCompareNonEmptyBeatsEmpty(remover, rateLine3, rateLine4, RateEntrySchema.TI_RC);
			AssertCompareNonEmptyBeatsEmpty(remover, rateLine3, rateLine4, RateEntrySchema.TI_OH_ControllingCustomer);
			AssertCompareNonEmptyBeatsEmpty(remover, rateLine3, rateLine4, RateEntrySchema.TI_OH_Consignor);
			AssertCompareNonEmptyBeatsEmpty(remover, rateLine3, rateLine4, RateEntrySchema.TI_OH_Consignee);
			AssertCompareNonEmptyBeatsEmpty(remover, rateLine3, rateLine4, RateEntrySchema.TI_OA_CartagePickupAddressOverride);
			AssertCompareNonEmptyBeatsEmpty(remover, rateLine3, rateLine4, RateEntrySchema.TI_OA_CartageDeliveryAddressOverride);
			AssertCompareNonEmptyBeatsEmpty(remover, rateLine3, rateLine4, RateEntrySchema.TI_ParentID);
			AssertCompareNonEmptyBeatsEmpty(remover, rateLine3, rateLine4, RateEntrySchema.TI_ParentTableCode);
			AssertCompareNonEmptyBeatsEmpty(remover, rateLine3, rateLine4, RateEntrySchema.TI_RH_NKCommodityCode);
			AssertCompareNonEmptyBeatsEmpty(remover, rateLine3, rateLine4, RateEntrySchema.TI_RS_NKServiceLevel_NI);
			AssertCompareNonEmptyBeatsEmpty(remover, rateLine3, rateLine4, RateEntrySchema.TI_OH_TransportProvider);
			AssertCompareNonEmptyBeatsEmpty(remover, rateLine3, rateLine4, RateEntrySchema.TI_ViaLRC);
			AssertCompareNonEmptyBeatsEmpty(remover, rateLine3, rateLine4, RateEntrySchema.TI_OH_Supplier);

			AssertCompareBigWins(remover, rateLine3, rateLine4, RateEntrySchema.TI_CartagePickupAddressPostCode);
			AssertCompareBigWins(remover, rateLine3, rateLine4, RateEntrySchema.TI_CartageDeliveryAddressPostCode);

			Env.Registry.Rating.FreightSearchPriorities =
				RateEntrySchema.TI_RS_NKServiceLevel_NI.Name + "," +
				RateEntrySchema.TI_RH_NKCommodityCode.Name + "," +
				RateEntrySchema.TI_ViaLRC.Name + "," +
				RateEntrySchema.TI_OH_TransportProvider.Name + ",";
			remover = new SimpleOverriddenRateLinesRemover(null);
			AssertCompareNonEmptyBeatsEmpty(remover, rateLine3, rateLine4, RateEntrySchema.TI_RH_NKCommodityCode);
			AssertCompareNonEmptyBeatsEmpty(remover, rateLine3, rateLine4, RateEntrySchema.TI_RS_NKServiceLevel_NI);
			AssertCompareNonEmptyBeatsEmpty(remover, rateLine3, rateLine4, RateEntrySchema.TI_OH_TransportProvider);
			AssertCompareNonEmptyBeatsEmpty(remover, rateLine3, rateLine4, RateEntrySchema.TI_ViaLRC);
		}

		#region Commodity Comparison

		public void TestCommodityComparison()
		{
			var client = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(client);

			var rateEntry1 = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			rateEntry1.TI_RH_NKCommodityCode = string.Empty;

			rateEntry1.TI_IsTact = false;
			var rateLine1 = rateEntry1.AddRateLine("ODOC");
			var rateEntry2 = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			rateEntry2.TI_RH_NKCommodityCode = string.Empty;
			rateEntry2.TI_IsTact = false;
			var rateLine2 = rateEntry2.AddRateLine("ODOC");

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.AIR, 100, 2, client);

			var remover = new SimpleOverriddenRateLinesRemover(criteria);

			// Only rule now is not-empty > empty
			CombineAssertions(() =>
			{
				//default TACT False|False

				SetCommodityCriteria(criteria, string.Empty);
				rateEntry1.TI_RH_NKCommodityCode = string.Empty;
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand2, "commodity1 is empty, commodity2 is not matched");

				SetCommodityCriteria(criteria, string.Empty);
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry2.TI_RH_NKCommodityCode = string.Empty;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand1, "GEN commodity1 match empty criteria");

				SetCommodityCriteria(criteria, string.Empty);
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Equal, "GEN commodity1 match empty criteria");

				SetCommodityCriteria(criteria, "GEN");
				rateEntry1.TI_RH_NKCommodityCode = string.Empty;
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand2, "commodity1 is empty, commodity2 is not matched");

				SetCommodityCriteria(criteria, "GEN");
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry2.TI_RH_NKCommodityCode = string.Empty;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand1, "commodity1 is matched");

				SetCommodityCriteria(criteria, "GEN");
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Equal, "commmodity1 is matched");

				SetCommodityCriteria(criteria, "XXX");
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry2.TI_RH_NKCommodityCode = string.Empty;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand1, "commodity2 is empty, commodity1 is not matched");

				SetCommodityCriteria(criteria, "XXX");
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Equal, "commodity2 is matched");

				SetCommodityCriteria(criteria, "XXX");
				rateEntry1.TI_RH_NKCommodityCode = string.Empty;
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand2, "commodity2 is matched");

				SetCommodityCriteria(criteria, "YYY");
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Equal, "no commoditis is matched");

				SetCommodityCriteria(criteria, "YYY");
				rateEntry1.TI_RH_NKCommodityCode = string.Empty;
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand2, "commomdity1 is empty, commodity2 is not matched");

				//TACT True|True

				SetCommodityCriteria(criteria, string.Empty);
				rateEntry1.TI_RH_NKCommodityCode = string.Empty;
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				rateEntry2.TI_IsTact = true;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand2, "both rate is TACT but commodity2 is not empty");

				SetCommodityCriteria(criteria, string.Empty);
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = string.Empty;
				rateEntry2.TI_IsTact = true;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand1, "both rate is TACT but commodity1 is not empty");

				SetCommodityCriteria(criteria, string.Empty);
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				rateEntry2.TI_IsTact = true;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Equal, "both commodities are not empty");

				SetCommodityCriteria(criteria, "GEN");
				rateEntry1.TI_RH_NKCommodityCode = string.Empty;
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				rateEntry2.TI_IsTact = true;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand2, "both rate is TACT but commodity2 is not empty");

				SetCommodityCriteria(criteria, "GEN");
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = string.Empty;
				rateEntry2.TI_IsTact = true;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand1, "both rate is TACT but commodity1 is not empty");

				SetCommodityCriteria(criteria, "GEN");
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				rateEntry2.TI_IsTact = true;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Equal, "both commodities are not empty");

				SetCommodityCriteria(criteria, "XXX");
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = string.Empty;
				rateEntry2.TI_IsTact = true;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand1, "both rate is TACT but commodity1 is not empty");

				SetCommodityCriteria(criteria, "XXX");
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				rateEntry2.TI_IsTact = true;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Equal, "both commodities are not empty");

				SetCommodityCriteria(criteria, "XXX");
				rateEntry1.TI_RH_NKCommodityCode = string.Empty;
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				rateEntry2.TI_IsTact = true;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand2, "both rate is TACT but commodity2 is not empty");

				SetCommodityCriteria(criteria, "YYY");
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				rateEntry2.TI_IsTact = true;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Equal, "both commodities are not empty");

				SetCommodityCriteria(criteria, "YYY");
				rateEntry1.TI_RH_NKCommodityCode = string.Empty;
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				rateEntry2.TI_IsTact = true;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand2, "both rate is TACT but commmodity2 is not empty");

				//TACT True|False

				SetCommodityCriteria(criteria, string.Empty);
				rateEntry1.TI_RH_NKCommodityCode = string.Empty;
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				rateEntry2.TI_IsTact = false;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand2, "rate1 is TACT and commodity1 is empty, commodity2 is not matched");

				SetCommodityCriteria(criteria, string.Empty);
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = string.Empty;
				rateEntry2.TI_IsTact = false;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand1, "GEN commodity1 match empty criteria");

				SetCommodityCriteria(criteria, string.Empty);
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				rateEntry2.TI_IsTact = false;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Equal, "GEN commodity1 match empty criteria");

				SetCommodityCriteria(criteria, "GEN");
				rateEntry1.TI_RH_NKCommodityCode = string.Empty;
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				rateEntry2.TI_IsTact = false;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand2, "rate1 is TACT and commodity1 is empty, commodity2 is not matched");

				SetCommodityCriteria(criteria, "GEN");
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = string.Empty;
				rateEntry2.TI_IsTact = false;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand1, "commodity1 is matched");

				SetCommodityCriteria(criteria, "GEN");
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				rateEntry2.TI_IsTact = false;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Equal, "commodity1 is matched");

				SetCommodityCriteria(criteria, "XXX");
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = string.Empty;
				rateEntry2.TI_IsTact = false;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand1, "commodity1 is TACT");

				SetCommodityCriteria(criteria, "XXX");
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				rateEntry2.TI_IsTact = false;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Equal, "commodity2 is matched");

				SetCommodityCriteria(criteria, "XXX");
				rateEntry1.TI_RH_NKCommodityCode = string.Empty;
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				rateEntry2.TI_IsTact = false;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand2, "commodity2 is matched");

				SetCommodityCriteria(criteria, "YYY");
				rateEntry1.TI_RH_NKCommodityCode = "GEN";
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				rateEntry2.TI_IsTact = false;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Equal, " rate1 is TACT, commodity2 is not matched");

				SetCommodityCriteria(criteria, "YYY");
				rateEntry1.TI_RH_NKCommodityCode = string.Empty;
				rateEntry1.TI_IsTact = true;
				rateEntry2.TI_RH_NKCommodityCode = "XXX";
				rateEntry2.TI_IsTact = false;
				AssertCommodityComparison(remover, rateEntry1, rateEntry2, criteria, ComparisonResult.Operand2, "rate1 is TACT and commodity1 is empty, commodity2 is not matched");
			});
		}

		void SetCommodityCriteria(TestRatingCriteria criteria, string commodityCriteria)
		{
			var measures = criteria.RateableMeasures;
			measures.RemoveContainerList();
			measures.AddContainerGroup(ZGuid.Empty, commodityCriteria, new[] { new MeasureInfo.ContainerInfo() });
		}

		static void AssertCommodityComparison(SimpleOverriddenRateLinesRemover remover, RateEntry rateEntry1, RateEntry rateEntry2, TestRatingCriteria criteria, ComparisonResult expectedResult, string message)
		{
			var rateLine1 = rateEntry1.ChildRateLines.Single();
			var rateLine2 = rateEntry2.ChildRateLines.Single();

			var result1 = remover.CompareForTest(rateLine1, rateLine2, out _);
			var result2 = remover.CompareForTest(rateLine2, rateLine1, out _);

			switch (expectedResult)
			{
				case ComparisonResult.Equal:
					AssertEquals($"GIVEN {message}\r\n; WHEN rateLine1 vs rateLine2", 0, result1);
					AssertEquals($"GIVEN {message}\r\n; WHEN rateLine2 vs rateLine1", 0, result2);
					break;

				case ComparisonResult.Operand1:
					AssertGreaterThan($"GIVEN {message}\r\n; WHEN rateLine2 vs rateLine1", 0, result2);
					AssertLessThan($"GIVEN {message}\r\n; WHEN rateLine1 vs rateLine2", 0, result1);
					break;

				case ComparisonResult.Operand2:
					AssertGreaterThan($"GIVEN {message}\r\n; WHEN rateLine1 vs rateLine2", 0, result1);
					AssertLessThan($"GIVEN {message}\r\n; WHEN rateLine2 vs rateLine1", 0, result2);
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(expectedResult), expectedResult, null);
			}
		}

		enum ComparisonResult
		{
			Operand1,
			Operand2,
			Equal
		}

		#endregion

		#region Comparing Transport Zones

		public void TestComparingTransportZones_Origin()
		{
			var client = Helper.NewOrgHeader(1);
			var allZoneSet = Helper.CreateRateTransportZoneSet(client, CountryCodes.UnitedStates, RatingConstants.RatingZoneTypes.All);
			var allZone = allZoneSet.CreateRateTransportZoneForTest("0 to 149");
			allZone.CreateRateTransportZoneItemForTest(0, 150);

			var ratingZoneSet = Helper.CreateRateTransportZoneSet(client, CountryCodes.Australia);
			var ratingZone = ratingZoneSet.CreateRateTransportZoneForTest("0 to 149");
			ratingZone.CreateRateTransportZoneItemForTest(0, 150);

			Factory.Save();

			var rate = Helper.NewClientRate(client);
			var rateEntry1 = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			var rateEntry2 = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			rateEntry1.TI_TZ_OriginZone = allZone.PK;
			rateEntry2.TI_TZ_OriginZone = ratingZone.PK;

			var rateLine1 = rateEntry1.AddRateLine("ODOC");
			var rateLine2 = rateEntry2.AddRateLine("ODOC");

			var remover = new SimpleOverriddenRateLinesRemover(new TestRatingCriteria("AUSYD", "USLAX", FreightMode.AIR, 100, 2, client));

			BaseRateLineComparer comparer;

			Assert(remover.CompareForTest(rateLine1, rateLine2, out comparer) < 0);
			Assert(remover.CompareForTest(rateLine2, rateLine1, out comparer) > 0);

			rateEntry2.TI_TZ_OriginZone = allZone.PK;

			Assert(remover.CompareForTest(rateLine1, rateLine2, out comparer) == 0);
			Assert(remover.CompareForTest(rateLine2, rateLine1, out comparer) == 0);

			rateEntry2.TI_TZ_OriginZone = ZGuid.Empty;

			Assert(remover.CompareForTest(rateLine1, rateLine2, out comparer) > 0);
			Assert(remover.CompareForTest(rateLine2, rateLine1, out comparer) < 0);

			rateEntry1.TI_TZ_OriginZone = allZone.PK;

			Assert(remover.CompareForTest(rateLine1, rateLine2, out comparer) > 0);
			Assert(remover.CompareForTest(rateLine2, rateLine1, out comparer) < 0);
		}

		public void TestComparingTransportZones_Destination()
		{
			var client = Helper.NewOrgHeader(1);

			var allZoneSet = Helper.CreateRateTransportZoneSet(client, CountryCodes.UnitedStates, RatingConstants.RatingZoneTypes.All);
			var allZone = allZoneSet.CreateRateTransportZoneForTest("0 to 149");
			allZone.CreateRateTransportZoneItemForTest(0, 149);

			var ratingZoneSet = Helper.CreateRateTransportZoneSet(client, CountryCodes.UnitedStates, distances: new[] { 0, 150 });

			Factory.Save();

			var rate = Helper.NewClientRate(client);
			var rateEntry1 = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			var rateEntry2 = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			rateEntry1.TI_TZ_DestinationZone = allZone.PK;
			rateEntry2.TI_TZ_DestinationZone = ratingZoneSet.Zones[0].PK;

			var rateLine1 = rateEntry1.AddRateLine("ODOC");
			var rateLine2 = rateEntry2.AddRateLine("ODOC");

			var remover = new SimpleOverriddenRateLinesRemover(new TestRatingCriteria("AUSYD", "USLAX", FreightMode.AIR, 100, 2, client));

			BaseRateLineComparer comparer;

			Assert(remover.CompareForTest(rateLine1, rateLine2, out comparer) < 0);
			Assert(remover.CompareForTest(rateLine2, rateLine1, out comparer) > 0);

			rateEntry2.TI_TZ_DestinationZone = allZone.PK;

			Assert(remover.CompareForTest(rateLine1, rateLine2, out comparer) == 0);
			Assert(remover.CompareForTest(rateLine2, rateLine1, out comparer) == 0);

			rateEntry2.TI_TZ_DestinationZone = ZGuid.Empty;

			Assert(remover.CompareForTest(rateLine1, rateLine2, out comparer) > 0);
			Assert(remover.CompareForTest(rateLine2, rateLine1, out comparer) < 0);

			rateEntry1.TI_TZ_DestinationZone = allZone.PK;

			Assert(remover.CompareForTest(rateLine1, rateLine2, out comparer) > 0);
			Assert(remover.CompareForTest(rateLine2, rateLine1, out comparer) < 0);
		}

		#endregion

		#region Comparing Gateway Agent Types

		public void TestComparingGatewayAgentTypes()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var rateEntry = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			rateEntry.TI_GatewayAgentType = string.Empty;

			var rateEntrySAG = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			rateEntrySAG.TI_GatewayAgentType = GatewayAgentType.Codes.SendingAgent;

			var rateEntryRAG = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			rateEntryRAG.TI_GatewayAgentType = GatewayAgentType.Codes.ReceivingAgent;

			var rateLine = rateEntry.AddRateLine("ODOC");
			var rateLineSAG = rateEntrySAG.AddRateLine("ODOC");
			var rateLineRAG = rateEntryRAG.AddRateLine("ODOC");

			var remover = new SimpleOverriddenRateLinesRemover(null);

			var list = new List<IRateLine>();
			list.Add(rateLine);
			list.Add(rateLineSAG);

			Assert("rate line from gateway sending agent gets priority over rate line from empty gateway agent", remover.CompareForTest(rateLine, rateLineSAG, out _) < 0);

			remover.Remove(list);

			CombineAssertions("Should filter rate lines from empty gateway agent type and use rate line from sending gateway agent rate", () =>
			{
				AssertEquals(1, list.Count);
				AssertEquals(rateLineSAG, list[0]);
			});

			list.Clear();
			list.Add(rateLine);
			list.Add(rateLineRAG);

			Assert("rate line from gateway receiving agent gets priority over rate line from empty gateway agent", remover.CompareForTest(rateLine, rateLineRAG, out _) < 0);

			remover.Remove(list);

			CombineAssertions("Should filter rate lines from empty gateway agent type and use rate line from receiving gateway agent rate", () =>
			{
				AssertEquals(1, list.Count);
				AssertEquals(rateLineRAG, list[0]);
			});

			list.Clear();
			list.Add(rateLineRAG);
			list.Add(rateLineSAG);

			Assert("rate line from gateway sending agent is equal to rate line from receiving gateway agent", remover.CompareForTest(rateLineRAG, rateLineSAG, out _) == 0);

			remover.Remove(list);

			CombineAssertions("Should return both sending and receiving gateway agent types", () =>
			{
				AssertEquals(2, list.Count);
				AssertEquals(rateLineRAG, list[0]);
				AssertEquals(rateLineSAG, list[1]);
			});
		}

		#endregion

		#region Comparing Gateway Sevice Level

		public void TestGatewayServiceLevelComparer()
		{
			var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Code = "GWSL";
			globalChargeCode.AC_Desc = "GWSL Global Charge";
			globalChargeCode.AC_ChargeType = ChargeType.Margin;
			globalChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			globalChargeCode.AC_RateCalculator = FlatCalculator.Code;

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			var remover = new SimpleOverriddenRateLinesRemover(criteria);

			var comparers = remover.GetComparers();
			Assert(comparers.Contains(new ColumnComparer(RateEntrySchema.TI_RS_NKGatewayServiceLevel)));

			var intercompanyTariff1 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1 = intercompanyTariff1.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line1 = entry1.AddRateLine("GWSL", FlatCalculator.Code);

			var intercompanyTariff2 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry2 = intercompanyTariff2.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line2 = entry2.AddRateLine("GWSL", FlatCalculator.Code);

			var intercompanyTariff3 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry3 = intercompanyTariff3.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line3 = entry3.AddRateLine("GWSL", FlatCalculator.Code);

			AssertRemove(null, remover, line1, line2, line3);
			AssertRemove(null, remover, line3, line2, line1);

			// GatewayServiceLevel takes priority
			entry1.TI_RS_NKServiceLevel_NI = "DIR";
			entry2.TI_RS_NKGatewayServiceLevel = "STD";
			AssertRemove(line2, remover, line1, line2, line3);
			AssertRemove(line2, remover, line3, line2, line1);
		}

		#endregion

		#region Comparing Shipment Gateway Sevice Level

		public void TestShipmentGatewayServiceLevelComparer()
		{
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD").RS_IsGateway = true;

			var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Code = "GWSL";
			globalChargeCode.AC_Desc = "GWSL Global Charge";
			globalChargeCode.AC_ChargeType = ChargeType.Margin;
			globalChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			globalChargeCode.AC_RateCalculator = FlatCalculator.Code;

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			var remover = new SimpleOverriddenRateLinesRemover(criteria);

			var comparers = remover.GetComparers();
			Assert(comparers.Contains(new ColumnComparer(RateEntrySchema.TI_RS_NKShipmentGatewayServiceLevel)));

			var intercompanyTariff1 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1 = intercompanyTariff1.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line1 = entry1.AddRateLine("GWSL", FlatCalculator.Code);

			var intercompanyTariff2 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry2 = intercompanyTariff2.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line2 = entry2.AddRateLine("GWSL", FlatCalculator.Code);

			var intercompanyTariff3 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry3 = intercompanyTariff3.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line3 = entry3.AddRateLine("GWSL", FlatCalculator.Code);

			AssertRemove(null, remover, line1, line2, line3);
			AssertRemove(null, remover, line3, line2, line1);

			// ShipmentGatewayServiceLevel takes priority
			entry1.TI_RS_NKGatewayServiceLevel = "DIR";
			entry2.TI_RS_NKShipmentGatewayServiceLevel = "STD";
			AssertRemove(line2, remover, line1, line2, line3);
			AssertRemove(line2, remover, line3, line2, line1);
		}

		#endregion

		#region Comparing Container Types

		public void TestCompareByContainerType_FlatOrPercentageCalculator_EmptyContainerTakesPriority()
		{
			var remover = new SimpleOverriddenRateLinesRemover(null);
			var chargeCodeBAF = Helper.ChargeCodes["BAF"];
			var chargeCodeWAR = Helper.ChargeCodes["WAR"];
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry20GP = testRate.AddRateEntry("FCL", container: "20GP");
			var line20GPFLT = entry20GP.AddRateLine(chargeCodeBAF, "FLT");
			var line20GPPER = entry20GP.AddRateLine(chargeCodeWAR, "PER");

			var entry40GP = testRate.AddRateEntry("FCL", container: "40GP");
			var line40GPFLT = entry40GP.AddRateLine(chargeCodeBAF, "FLT");
			var line40GPPER = entry40GP.AddRateLine(chargeCodeWAR, "PER");

			var entryEmpty = testRate.AddRateEntry("FCL");
			var lineEmptyFLT = entryEmpty.AddRateLine(chargeCodeBAF, "FLT");
			var lineEmptyPER = entryEmpty.AddRateLine(chargeCodeWAR, "PER");

			BaseRateLineComparer comparer;

			CombineAssertions("Ratelines with non-empty container type, should have same priority.", () =>
			{
				AssertEquals("Both 20GP and 40GP ratelines should have same priority.", 0, remover.CompareForTest(line20GPFLT, line40GPFLT, out comparer));
				AssertEquals("Both 20GP and 40GP ratelines should have same priority.", 0, remover.CompareForTest(line20GPPER, line40GPPER, out comparer));
			});

			CombineAssertions("Applicable for FCL(FLT):- Rateline with empty container type should have higher priority against non-empty container type.", () =>
			{
				comparer = null;
				AssertLessThan("Non-empty container should have lesser priority against empty.", remover.CompareForTest(line20GPFLT, lineEmptyFLT, out comparer), 0);
				AssertEquals(typeof(ContainerTypeColumnComparer), comparer.GetType());

				comparer = null;
				AssertGreaterThan("Empty container should have higher priority than non-empty container.", remover.CompareForTest(lineEmptyFLT, line20GPFLT, out comparer), 0);
				AssertEquals(typeof(ContainerTypeColumnComparer), comparer.GetType());
			});

			CombineAssertions("Applicable for FCL(~FLT):- Rateline with empty container type should have lesser priority against non-empty container type.", () =>
			{
				comparer = null;
				AssertLessThan("Empty container should have lesser priority than non-empty container.", remover.CompareForTest(lineEmptyPER, line20GPPER, out comparer), 0);
				AssertEquals(typeof(ContainerTypeColumnComparer), comparer.GetType());

				comparer = null;
				AssertGreaterThan("Non-empty container should have higher priority against empty.", remover.CompareForTest(line20GPPER, lineEmptyPER, out comparer), 0);
				AssertEquals(typeof(ContainerTypeColumnComparer), comparer.GetType());
			});
		}

		#endregion

		#region Comparing Gateway Planned Load

		public void TestGatewayPlannedLoadFallBack()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var rateEntry1 = intercompanyTariff.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUSYD", "USLAX", "FRT", 33m, CurrencyCodes.Australia);
			rateEntry1.TI_PlannedLoadLRC = "AUSYD";
			var rateLine1 = rateEntry1.RateLines[0];

			var rateEntry2 = intercompanyTariff.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUSYD", "USLAX", "FRT", 44m, CurrencyCodes.Australia);
			rateEntry2.TI_PlannedLoadLRC = "AU";
			var rateLine2 = rateEntry2.RateLines[0];

			var agent1 = Helper.NewOrgHeader("Org1");
			var agent2 = Helper.NewOrgHeader("Org2");
			var agent3 = Helper.NewOrgHeader("Org3");

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartSell();

				var criteria = new TestRatingCriteria();
				criteria.PlannedLoadForTest = LocationHelper.GetLocationFromString("AUSYD", Factory);
				criteria.SortedOverridenPlannedLoadForTest = new List<ILocation>
				{
					LocationHelper.GetLocationFromString("HKHKG", Factory),
					LocationHelper.GetLocationFromString("CNSHA", Factory),
					LocationHelper.GetLocationFromString("SGSIN", Factory)
				};

				var remover = new SimpleOverriddenRateLinesRemover(criteria);

				Assert("Should use criteria planned load; entry 1 has higher priority", remover.CompareForTest(rateLine1, rateLine2, out _) > 0);

				rateEntry1.TI_PlannedLoadLRC = "CNSHA";
				rateEntry2.TI_PlannedLoadLRC = "SGSIN";
				Assert("should use overriden planned load; entry 1 has higher priority", remover.CompareForTest(rateLine1, rateLine2, out _) > 0);

				rateEntry1.TI_PlannedLoadLRC = "SGSIN";
				rateEntry2.TI_PlannedLoadLRC = "HKHKG";
				Assert("should use overriden planned load; entry 2 has higher priority", remover.CompareForTest(rateLine1, rateLine2, out _) < 0);

				rateEntry1.TI_PlannedLoadLRC = "";
				rateEntry2.TI_PlannedLoadLRC = "";
				Assert("Should not filter based on planned load", remover.CompareForTest(rateLine1, rateLine2, out _) == 0);
			}
		}

		public void TestGatewayPlannedLoadDischarge()
		{
			var agent1 = Helper.NewOrgHeader();
			var agent2 = Helper.NewOrgHeader();
			var agent3 = Helper.NewOrgHeader();
			var agent4 = Helper.NewOrgHeader();

			var intercompanyTariff = Helper.NewIntercompanyTariff(agent1);
			var rateEntry1 = intercompanyTariff.AddRateEntryWithFlatRateLine("AIR", "LSE", "CHBSL", "USHOU", "FRT", 33m, CurrencyCodes.Australia);
			rateEntry1.TI_PlannedLoadLRC = "CHBSM";
			rateEntry1.TI_PlannedDischargeLRC = "USCHS";
			var rateLine1 = rateEntry1.RateLines[0];

			var rateEntry2 = intercompanyTariff.AddRateEntryWithFlatRateLine("AIR", "LSE", "CHBSL", "USHOU", "FRT", 44m, CurrencyCodes.Australia);
			rateEntry2.TI_PlannedLoadLRC = "BEANR";
			rateEntry2.TI_PlannedDischargeLRC = "USCHS";
			var rateLine2 = rateEntry2.RateLines[0];

			var chbsm = LocationHelper.GetLocationFromString("CHBSM", Factory);
			var chbsl = LocationHelper.GetLocationFromString("CHBSL", Factory);
			var beanr = LocationHelper.GetLocationFromString("BEANR", Factory);
			var debre = LocationHelper.GetLocationFromString("DEBRE", Factory);
			var uschs = LocationHelper.GetLocationFromString("USCHS", Factory);
			var ushou = LocationHelper.GetLocationFromString("USHOU", Factory);
			var chbss = LocationHelper.GetLocationFromString("CHBSS", Factory);

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();

				var criteria = new TestRatingCriteria();
				criteria.SortedGatewayAgentPKs = new List<ZGuid> { agent1.PK, agent2.PK, agent3.PK, agent4.PK };
				criteria.PlannedLoadForTest = chbsm;
				criteria.PlannedDischargeForTest = uschs;
				criteria.SortedOverridenPlannedLoadForTest = new List<ILocation>
				{
					chbsl,
					beanr,
					debre,
					uschs
				};
				criteria.SortedOverridenPlannedDischargeForTest = new List<ILocation>
				{
					beanr,
					debre,
					uschs,
					ushou
				};

				var remover = new SimpleOverriddenRateLinesRemover(criteria);

				Assert("Should use criteria planned load discharge cost if there is a match", remover.CompareForTest(rateLine1, rateLine2, out _) > 0);

				rateEntry1.TI_PlannedLoadLRC = "CHBSL";
				rateEntry1.TI_PlannedDischargeLRC = "USCHS";

				rateEntry2.TI_PlannedLoadLRC = "BEANR";
				rateEntry2.TI_PlannedDischargeLRC = "USCHS";
				Assert("", remover.CompareForTest(rateLine1, rateLine2, out _) > 0);

				rateEntry1.TI_PlannedLoadLRC = "BEANR";
				rateEntry1.TI_PlannedDischargeLRC = "DEBRE";

				rateEntry2.TI_PlannedLoadLRC = "BEANR";
				rateEntry2.TI_PlannedDischargeLRC = "USCHS";
				Assert("entry 1 has higher priority", remover.CompareForTest(rateLine1, rateLine2, out _) > 0);

				rateEntry1.TI_PlannedLoadLRC = "BEANR";
				rateEntry1.TI_PlannedDischargeLRC = "DEBRE";

				rateEntry2.TI_PlannedLoadLRC = "CHBSM";
				rateEntry2.TI_PlannedDischargeLRC = "USCHS";
				Assert("entry 2 has higher priority", remover.CompareForTest(rateLine1, rateLine2, out _) < 0);

				rateEntry1.TI_PlannedLoadLRC = "";
				rateEntry1.TI_PlannedDischargeLRC = "";

				rateEntry2.TI_PlannedLoadLRC = "";
				rateEntry2.TI_PlannedDischargeLRC = "";
				Assert("There should not be any filtering", remover.CompareForTest(rateLine1, rateLine2, out _) == 0);
			}
		}

		#endregion

		#region Payment Term Override
		public void TestPaymentTermOverrideComparer()
		{
			var remover = new SimpleOverriddenRateLinesRemover(null);

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line1 = testRate.AddRateEntry("ORG", "ALL", "AUSYD", "USLAX");
			line1.TI_PaymentTerm = "PPD";
			line1.AddRateLine("ODOC");
			var line2 = testRate.AddRateEntry("ORG", "ALL", "AUSYD", "USLAX");
			line2.TI_PaymentTerm = "";
			line2.AddRateLine("ODOC");

			BaseRateLineComparer comparer;
			AssertGreaterThan(remover.CompareForTest(line1.RateLines[0], line2.RateLines[0], out comparer), 0);
			AssertEquals(typeof(PaymentTermOverrideComparer), comparer.GetType());
		}

		public void TestPaymentTermOverrideComparerOnCosting()
		{
			var remover = new SimpleOverriddenRateLinesRemover(null);

			var testRate = Helper.NewCosting(Helper.NewOrgHeader());
			var line1 = testRate.AddRateEntry("ORG", "ALL", "AUSYD", "USLAX");
			line1.TI_PaymentTerm = "PPD";
			line1.AddRateLine("ODOC");
			var line2 = testRate.AddRateEntry("ORG", "ALL", "AUSYD", "USLAX");
			line2.TI_PaymentTerm = "";
			line2.AddRateLine("ODOC");

			AssertEquals(remover.CompareForTest(line1.RateLines[0], line2.RateLines[0], out _), 0);
		}
		#endregion

		#region Cross Trade

		public void TestCrossTradeComparer()
		{
			var remover = new SimpleOverriddenRateLinesRemover(null);

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line1 = testRate.AddRateEntry("ORG", "ALL", "", "");
			line1.TI_IsCrossTrade = true;
			line1.AddRateLine("ODOC");
			var line2 = testRate.AddRateEntry("ORG", "ALL", "", "");
			line2.TI_IsCrossTrade = false;
			line2.AddRateLine("ODOC");

			BaseRateLineComparer comparer;
			AssertGreaterThan(remover.CompareForTest(line1.RateLines[0], line2.RateLines[0], out comparer), 0);
			AssertEquals(typeof(CrossTradeComparer), comparer.GetType());

			line1.TI_IsCrossTrade = false;
			line2.TI_IsCrossTrade = true;

			AssertLessThan(remover.CompareForTest(line1.RateLines[0], line2.RateLines[0], out comparer), 0);
			AssertEquals(typeof(CrossTradeComparer), comparer.GetType());

			line1.TI_IsCrossTrade = true;
			line2.TI_IsCrossTrade = true;

			AssertEquals(remover.CompareForTest(line1.RateLines[0], line2.RateLines[0], out _), 0);

			line1.TI_IsCrossTrade = false;
			line2.TI_IsCrossTrade = false;

			AssertEquals(remover.CompareForTest(line1.RateLines[0], line2.RateLines[0], out _), 0);
		}

		#endregion

		#region HBL Delivery Mode

		public void TestComparingHBLDeliveryMode()
		{
			var client = Helper.NewOrgHeader(1);
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.AIR, 100, 2, client);
			criteria.ContainerMode = "ULD";
			var remover = new SimpleOverriddenRateLinesRemover(criteria);

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var rateEntry = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			rateEntry.TI_GatewayAgentType = string.Empty;

			var rateEntryDoorDoor = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			rateEntryDoorDoor.TI_HBLDeliveryMode = HBLDeliveryModes.Codes.DOOR_DOOR;

			var rateEntryCfsCfs = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			rateEntryCfsCfs.TI_HBLDeliveryMode = HBLDeliveryModes.Codes.CFS_CFS;

			var rateLine = rateEntry.AddRateLine("ODOC");
			var rateLineDoorDoor = rateEntryDoorDoor.AddRateLine("ODOC");
			var rateLineCFS_CFS = rateEntryCfsCfs.AddRateLine("ODOC");

			var list = new List<IRateLine>();
			list.Add(rateLine);
			list.Add(rateLineDoorDoor);

			AssertEquals(string.Empty, criteria.HBLDeliveryMode);
			AssertEquals("when criteria.HBLDeliveryMode is empty, we should NOT filter any rate lines based on HBLDeliveryMode", 0, remover.CompareForTest(rateLineDoorDoor, rateLine, out _));

			remover.Remove(list);

			CombineAssertions("Should NOT filter rate lines based on HBLDeliveryMode when criteria.HBLDeliveryMode is empty", () =>
			{
				AssertEquals(2, list.Count);
				AssertEquals(rateLine, list[0]);
				AssertEquals(rateLineDoorDoor, list[1]);
			});

			list.Clear();
			list.Add(rateLine);
			list.Add(rateLineDoorDoor);
			list.Add(rateLineCFS_CFS);

			criteria.HBLDeliveryMode = HBLDeliveryModes.Codes.DOOR_DOOR;
			remover = new SimpleOverriddenRateLinesRemover(criteria);

			Assert("rate line with HBL Delivery Mode equal to criteria.HBLDeliveryMode gets priority over rate line with empty HBL Delivery Mode ", remover.CompareForTest(rateLine, rateLineDoorDoor, out _) < 0);
			Assert("rate line with HBL Delivery Mode equal to criteria.HBLDeliveryMode gets priority over rate line with others HBL Delivery Mode ", remover.CompareForTest(rateLineCFS_CFS, rateLineDoorDoor, out _) < 0);

			remover.Remove(list);

			CombineAssertions("Should filter rate HBL Delivery Modes not equal to non empty criteria.HBLDeliveryMode", () =>
			{
				AssertEquals(1, list.Count);
				AssertEquals(rateLineDoorDoor, list[0]);
			});

			var configurations = new Enterprise.Registry.Business.HBLDeliveryPriorityConfigCollection();
			var configuration = configurations.AddNew();
			configuration.ContainerMode = criteria.ContainerMode;
			configuration.HBLDeliveryMode = HBLDeliveryModes.Codes.DOOR_DOOR;
			var setting = configuration.Settings.AddNew();
			setting.HBLDeliveryModePriority = HBLDeliveryModes.Codes.DOOR_DOOR;
			var setting1 = configuration.Settings.AddNew();
			setting1.HBLDeliveryModePriority = HBLDeliveryModes.Codes.CFS_DOOR;
			var setting2 = configuration.Settings.AddNew();
			setting2.HBLDeliveryModePriority = HBLDeliveryModes.Codes.CFS_CFS;

			using (Enterprise.Registry.Business.RatingDataRegistry.Instance.HBLDeliveryPriority.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configurations))
			{
				list.Clear();
				list.Add(rateLine);
				list.Add(rateLineDoorDoor);
				list.Add(rateLineCFS_CFS);

				Assert("rate line with HBL Delivery Mode equal to Highest Priority gets priority over rate line with empty HBL Delivery Mode", remover.CompareForTest(rateLine, rateLineDoorDoor, out _) < 0);
				Assert("rate line with HBL Delivery Mode equal to Highest Priority gets priority over rate line with lower HBL Delivery Mode", remover.CompareForTest(rateLineCFS_CFS, rateLineDoorDoor, out _) < 0);

				remover.Remove(list);

				CombineAssertions("Should filter rate HBL Delivery Modes not equal to non empty criteria.HBLDeliveryMode", () =>
				{
					AssertEquals(1, list.Count);
					AssertEquals(rateLineDoorDoor, list[0]);
				});
			}

			setting.HBLDeliveryModePriority = HBLDeliveryModes.Codes.CFS_DOOR;
			setting1.HBLDeliveryModePriority = HBLDeliveryModes.Codes.CFS_CFS;
			setting2.HBLDeliveryModePriority = HBLDeliveryModes.Codes.DOOR_DOOR;

			using (Enterprise.Registry.Business.RatingDataRegistry.Instance.HBLDeliveryPriority.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configurations))
			{
				list.Clear();
				list.Add(rateLine);
				list.Add(rateLineDoorDoor);
				list.Add(rateLineCFS_CFS);

				Assert("rate line with HBL Delivery Mode equal to Highest Priority gets priority over rate line with empty HBL Delivery Mode", remover.CompareForTest(rateLine, rateLineCFS_CFS, out _) < 0);
				Assert("rate line with HBL Delivery Mode equal to Highest Priority gets priority over rate line with lower HBL Delivery Mode", remover.CompareForTest(rateLineDoorDoor, rateLineCFS_CFS, out _) < 0);

				remover.Remove(list);

				CombineAssertions("Should filter rate HBL Delivery Modes not equal to non empty criteria.HBLDeliveryMode", () =>
				{
					AssertEquals(1, list.Count);
					AssertEquals(rateLineCFS_CFS, list[0]);
				});
			}
		}

		public void TestHBLDeliveryModeInFreightSearchFreightSearchPriorities()
		{
			var client = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(client);
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.AIR, 100, 2, client);
			var remover = new SimpleOverriddenRateLinesRemover(criteria);
			var rateWithCommodity = rate.AddRateEntry("ORG", "ALL", "AUSYD", "USLAX", "", "").AddRateLine("ODOC");
			var rateWithHBLDeliveryMode = rate.AddRateEntry("ORG", "ALL", "AUSYD", "USLAX", "", "").AddRateLine("ODOC");
			rateWithCommodity.Parent.TI_RH_NKCommodityCode = "GEN";
			rateWithCommodity.Parent.TI_HBLDeliveryMode = ZString.Empty;

			rateWithHBLDeliveryMode.Parent.TI_RH_NKCommodityCode = ZString.Empty;
			rateWithHBLDeliveryMode.Parent.TI_HBLDeliveryMode = "DOOR/DOOR";

			Env.Registry.Rating.FreightSearchPriorities =
				RateEntrySchema.TI_RS_NKServiceLevel_NI.Name + "," +
				RateEntrySchema.TI_HBLDeliveryMode.Name + "," +
				RateEntrySchema.TI_RH_NKCommodityCode.Name + "," +
				RateEntrySchema.TI_ViaLRC.Name + "," +
				RateEntrySchema.TI_OH_TransportProvider.Name + ",";

			remover = new SimpleOverriddenRateLinesRemover(criteria);

			var list = new List<IRateLine>();
			list.Add(rateWithCommodity);
			list.Add(rateWithHBLDeliveryMode);

			AssertEquals(string.Empty, criteria.HBLDeliveryMode);
			Assert("criteria.HBLDeliveryMode is empty, we ignore HBLDeliveryMode even it has high priority", remover.CompareForTest(rateWithHBLDeliveryMode, rateWithCommodity, out _) < 0);

			remover.Remove(list);

			CombineAssertions("Should NOT filter rate lines based on HBLDeliveryMode when criteria.HBLDeliveryMode is empty", () =>
			{
				AssertEquals(1, list.Count);
				AssertEquals(rateWithCommodity, list[0]);
			});

			Env.Registry.Rating.FreightSearchPriorities =
				RateEntrySchema.TI_RS_NKServiceLevel_NI.Name + "," +
				RateEntrySchema.TI_RH_NKCommodityCode.Name + "," +
				RateEntrySchema.TI_HBLDeliveryMode.Name + "," +
				RateEntrySchema.TI_ViaLRC.Name + "," +
				RateEntrySchema.TI_OH_TransportProvider.Name + ",";

			criteria.HBLDeliveryMode = "DOOR/DOOR";
			remover = new SimpleOverriddenRateLinesRemover(criteria);

			list.Clear();
			list.Add(rateWithCommodity);
			list.Add(rateWithHBLDeliveryMode);

			Assert("Commodity has higher priority in FreightSearchPriorities than HBLDeliveryMode, we remove HBD Delivery one ", remover.CompareForTest(rateWithHBLDeliveryMode, rateWithCommodity, out _) < 0);

			remover.Remove(list);

			CombineAssertions("Should NOT filter rate lines based on HBLDeliveryMode when criteria.HBLDeliveryMode is empty", () =>
			{
				AssertEquals(1, list.Count);
				AssertEquals(rateWithCommodity, list[0]);
			});

			list.Clear();
			list.Add(rateWithCommodity);
			list.Add(rateWithHBLDeliveryMode);

			Env.Registry.Rating.FreightSearchPriorities =
				RateEntrySchema.TI_RS_NKServiceLevel_NI.Name + "," +
				RateEntrySchema.TI_HBLDeliveryMode.Name + "," +
				RateEntrySchema.TI_RH_NKCommodityCode.Name + "," +
				RateEntrySchema.TI_ViaLRC.Name + "," +
				RateEntrySchema.TI_OH_TransportProvider.Name + ",";

			remover = new SimpleOverriddenRateLinesRemover(criteria);

			Assert("HBLDeliveryMode has higher priority in FreightSearchPriorities than Commodity, we remove Commodity one ", remover.CompareForTest(rateWithCommodity, rateWithHBLDeliveryMode, out _) < 0);

			remover.Remove(list);

			CombineAssertions("Should NOT filter rate lines based on HBLDeliveryMode when criteria.HBLDeliveryMode is empty", () =>
			{
				AssertEquals(1, list.Count);
				AssertEquals(rateWithHBLDeliveryMode, list[0]);
			});
		}

		#endregion

		static void AssertCompareBigWins(SimpleOverriddenRateLinesRemover remover, RateLine line1, RateLine line2, SchemaColumn column)
		{
			var bizo1 = GetBizoForComparing(line1, column);
			var bizo2 = GetBizoForComparing(line2, column);
			if (column.DotNetType == typeof(string))
			{
				bizo1[column.Name] = "##";
				bizo2[column.Name] = "###";
			}
			else
			{
				throw new NotSupportedException();
			}

			BaseRateLineComparer comparer;
			Assert(remover.CompareForTest(line1, line2, out comparer) < 0);
			Assert(remover.CompareForTest(line2, line1, out comparer) > 0);

			if (column.DotNetType == typeof(string))
			{
				bizo1[column.Name] = ZString.Empty;
				bizo2[column.Name] = ZString.Empty;
			}
		}

		static void AssertCompareNonEmptyBeatsEmpty(SimpleOverriddenRateLinesRemover remover, RateLine line, RateLine possibleOverride, SchemaColumn column)
		{
			var bizobj = GetBizoForComparing(possibleOverride, column);
			if (column.DotNetType == typeof(Guid))
			{
				bizobj[column.Name] = ZGuid.NewZGuid();
			}
			else if (column.DotNetType == typeof(string))
			{
				bizobj[column.Name] = "##";
			}
			else
			{
				throw new NotSupportedException();
			}

			BaseRateLineComparer comparer;
			Assert(remover.CompareForTest(line, possibleOverride, out comparer) < 0);
			Assert(remover.CompareForTest(possibleOverride, line, out comparer) > 0);

			if (column.DotNetType == typeof(Guid))
			{
				bizobj[column.Name] = ZGuid.Empty;
			}
			else if (column.DotNetType == typeof(string))
			{
				bizobj[column.Name] = ZString.Empty;
			}
		}

		static BusinessObject GetBizoForComparing(RateLine rateLine, SchemaColumn column)
		{
			return column.TableName == RateEntrySchema.Constants.TableName ? rateLine.Parent : rateLine;
		}

		void AssertRemove(RateLine expectedRemainingLine, SimpleOverriddenRateLinesRemover remover, params RateLine[] sourceLines)
		{
			var lines = new List<IRateLine>(sourceLines);
			remover.Remove(lines);
			if (expectedRemainingLine != null)
			{
				AssertEquals(1, lines.Count);
				AssertEquals(expectedRemainingLine, lines[0]);
			}
			else
			{
				AssertEquals(sourceLines.Length, lines.Count);
			}
		}
	}
}
