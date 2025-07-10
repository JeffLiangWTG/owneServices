using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Rating.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business
{
	public class LineMeasureMatcherTest : TestCaseWithFactory
	{
		public void TestRemoveSimilarCharges_PercentageCalculatorWithMultipleApplyTo_SameCharges()
		{
			var gp20 = Helper.Containers["20GP"].PK;
			var gp40 = Helper.Containers["40GP"].PK;

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var rateEntry20GP = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.SCO, RateMode.SEA, "AUSYD", "USLAX", "FRT", 20m, "CN", container: "20GP");
			var rateLine11 = rateEntry20GP.AddUnitCharge("BAF", 21m, "CN");
			var rateLine12 = rateEntry20GP.AddRateLine("CAF", PercentageCalculator.Code);
			rateLine12.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["FRT"].PK;
			rateLine12.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["BAF"].PK;
			rateLine12.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_Value = 20m;

			var rateEntry40GP = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.SCO, RateMode.SEA, "AUSYD", "USLAX", "FRT", 40m, "CN", container: "40GP");
			var rateLine21 = rateEntry40GP.AddUnitCharge("BAF", 41m, "CN");
			var rateLine22 = rateEntry40GP.AddRateLine("CAF", PercentageCalculator.Code);
			rateLine22.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["FRT"].PK;
			rateLine22.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["BAF"].PK;
			rateLine22.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_Value = 20m;

			var criteria = new TestRatingCriteria();
			criteria.RateableMeasures.AddContainerGroup(gp20, new[] { new MeasureInfo.ContainerInfo(100m, "KG", 1m, "M3", 1, 1, "12345") });
			criteria.RateableMeasures.AddContainerGroup(gp40, new[] { new MeasureInfo.ContainerInfo(100m, "KG", 1m, "M3", 1, 1, "5678") });

			var linesRepository = new RateLinesRepository(criteria, new[] { (IRateEntry)rateEntry20GP, rateEntry40GP }.ToList(), new DummyLogger());
			var matcher = new LineMeasureMatcher(criteria);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var amountByLineTable = new AmountByLineTable(parameters);
			matcher.RemoveSimilarCharges(parameters, linesRepository, amountByLineTable);

			var leftLines = linesRepository.GetLines().Select(l => ToString(l.Line));
			AssertContainsExactElementsInAnyOrder
			(
				"20GP and 40GP rates should come through",
				new[]
				{
					ToString(rateEntry20GP.RateLines[0]),
					ToString(rateLine11),
					ToString(rateLine12),
					ToString(rateEntry40GP.RateLines[0]),
					ToString(rateLine21),
					ToString(rateLine22)
				},
				leftLines
			);
		}

		public void TestRemoveSimilarCharges_LinesWithAndWithoutContainer_LinesWithoutContainerRemoved()
		{
			var gP20PK = Helper.Containers["20GP"].PK;
			var gP40PK = Helper.Containers["40GP"].PK;

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry1 = rate.AddRateEntry("FCL", mode: "SEA", container: "20GP");
			entry1.RateLines.RemoveAll();
			var line1 = entry1.AddFlatCharge("BAF", 100);

			var entry2 = rate.AddRateEntry("FCL", mode: "SEA", container: null);
			entry2.RateLines.RemoveAll();
			var line2 = entry2.AddPercentageCharge("BAF", Helper.ChargeCodes["FRT"], 10);

			var criteria = new TestRatingCriteria();
			criteria.RateableMeasures.AddContainerGroup(gP20PK, new[]
			{
				new MeasureInfo.ContainerInfo(100m, "KG", 1m, "M3", 1, 1, "12345")
			});
			criteria.RateableMeasures.AddContainerGroup(gP40PK, new[]
			{
				new MeasureInfo.ContainerInfo(100m, "KG", 1m, "M3", 1, 1, "5678")
			});

			var linesRepository = new RateLinesRepository(criteria, new[] { (IRateEntry)entry1, entry2 }.ToList(), new DummyLogger());
			var matcher = new LineMeasureMatcher(criteria);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var amountByLineTable = new AmountByLineTable(parameters);
			matcher.RemoveSimilarCharges(parameters, linesRepository, amountByLineTable);

			var leftLines = linesRepository.GetLines().Select(l => ToString(l.Line));
			AssertContainsExactElementsInAnyOrder(
				"Only line1 should come though as both lines match the job measures but line1 has exact match by container",
				new[] { ToString(line1) },
				leftLines);
		}

		public void TestRemoveSimilarCharges_WhenProviderIsDifferent_KeepIfRegistryPermits()
		{
			var collection = new SameChargeCodeDifferentProviderCollection();
			var rule = collection.AddNew();
			rule.JobType = "SHP";
			rule.TransportMode = "SEA";
			rule.Direction = "ALL";
			rule.IsEnabled = true;

			var provider = Helper.NewAirlineOrg("QF");

			var standardCost = Helper.NewCosting(null);
			var standardCostEntry = standardCost.AddRateEntry("FCL", mode: "SEA", serviceLevel: "STD", removeLines: true);
			var standardCostLine = standardCostEntry.AddFlatCharge("DFSC", 250);
			standardCostEntry.TI_OH_TransportProvider = provider.PK;

			var costing1 = Helper.NewCosting(provider);
			var costEntry1 = costing1.AddRateEntry("FCL", mode: "SEA", serviceLevel: "STD", removeLines: true);
			var costLine1 = costEntry1.AddFlatCharge("DFSC", 100);

			var costing2 = Helper.NewCosting(Helper.NewOrgHeader());
			var costEntry2 = costing2.AddRateEntry("FCL", mode: "SEA", removeLines: true);
			var costLine2 = costEntry2.AddUnitCharge("DFSC", 100, "CN");

			var criteria = new TestRatingCriteria { FreightMode = FreightMode.FCL, Creditors = Creditors.New(OrgWithSource.New(provider, new List<string> { "provider" })) };
			criteria.RateableMeasures.AddContainerGroup(Helper.Containers["20GP"].PK, new[]
			{
				new MeasureInfo.ContainerInfo(100m, "KG", 1m, "M3", 1, 1, "12345")
			});

			var linesRepository = new RateLinesRepository(criteria, new[] { (IRateEntry)costEntry1, costEntry2 }.ToList(), new DummyLogger());
			var matcher = new LineMeasureMatcher(criteria);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var amountByLineTable = new AmountByLineTable(parameters);

			// No registry value set - only one line should be returned.
			matcher.RemoveSimilarCharges(parameters, linesRepository, amountByLineTable);
			var leftLines = linesRepository.GetLines().Select(l => l.Line.PK).ToList();

			AssertEquals(1, leftLines.Count);
			AssertContainsExactElementsInAnyOrder(
				"Only one cost line should come through as the registry does not allow duplicates.",
				new[] { costLine1.PK },
				leftLines);

			// Registry value set - both lines should be returned.
			using (DataRegistryRating.Instance.AllowChargesWithSameChargeCodeForDifferentProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				linesRepository = new RateLinesRepository(criteria, new[] { (IRateEntry)costEntry1, costEntry2 }.ToList(), new DummyLogger());
				matcher.RemoveSimilarCharges(parameters, linesRepository, amountByLineTable);
				leftLines = linesRepository.GetLines().Select(l => l.Line.PK).ToList();

				AssertEquals(2, leftLines.Count);
				AssertContainsExactElementsInAnyOrder(
					"Both cost lines should come through as they have different service providers and the registry allows it.",
					new[] { costLine1.PK, costLine2.PK },
					leftLines);
			}
		}

		public void TestRemoveSimilarCharges_WhenProviderIsDifferent_DontGroupIfFreightOrRevenue()
		{
			var collection = new SameChargeCodeDifferentProviderCollection();
			var rule = collection.AddNew();
			rule.JobType = "SHP";
			rule.TransportMode = "SEA";
			rule.Direction = "ALL";
			rule.IsEnabled = true;

			var costing1 = Helper.NewCosting(Helper.NewOrgHeader());
			var costing2 = Helper.NewCosting(Helper.NewOrgHeader());

			var costEntry1 = costing1.AddRateEntry("FCL", mode: "SEA", serviceLevel: "STD", removeLines: true);
			var costLine1 = costEntry1.AddFlatCharge("BAF", 100);

			var costEntry2 = costing2.AddRateEntry("FCL", mode: "SEA", removeLines: true);
			var costLine2 = costEntry2.AddUnitCharge("BAF", 100, "CN");

			var clientRate1 = Helper.NewClientRate(Helper.NewOrgHeader());
			var clientEntry1 = clientRate1.AddRateEntry("FCL", mode: "SEA", serviceLevel: "STD", removeLines: true);
			var clientLine1 = clientEntry1.AddFlatCharge("DFSC", 200);

			var clientRate2 = Helper.NewClientRate(Helper.NewOrgHeader());
			var clientEntry2 = clientRate2.AddRateEntry("FCL", mode: "SEA", removeLines: true);
			var clientLine2 = clientEntry2.AddPercentageCharge("DFSC", Helper.ChargeCodes["CAF"], 5);

			var criteria = new TestRatingCriteria { FreightMode = FreightMode.FCL };
			criteria.RateableMeasures.AddContainerGroup(Helper.Containers["20GP"].PK, new[]
			{
				new MeasureInfo.ContainerInfo(100m, "KG", 1m, "M3", 1, 1, "12345")
			});

			var linesRepository = new RateLinesRepository(criteria, new[] { (IRateEntry)costEntry1, costEntry2 }.ToList(), new DummyLogger());
			var matcher = new LineMeasureMatcher(criteria);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var amountByLineTable = new AmountByLineTable(parameters);

			matcher.RemoveSimilarCharges(parameters, linesRepository, amountByLineTable);
			var leftLines = linesRepository.GetLines().Select(l => l.Line.PK).ToList();

			AssertEquals(1, leftLines.Count);
			AssertContainsExactElementsInAnyOrder(
				"Only one cost line should come through as the registry does not allow duplicates.",
				new[] { costLine1.PK },
				leftLines);

			using (DataRegistryRating.Instance.AllowChargesWithSameChargeCodeForDifferentProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				linesRepository = new RateLinesRepository(criteria, new[] { (IRateEntry)costEntry1, costEntry2 }.ToList(), new DummyLogger());
				matcher.RemoveSimilarCharges(parameters, linesRepository, amountByLineTable);
				leftLines = linesRepository.GetLines().Select(l => l.Line.PK).ToList();

				AssertEquals(1, leftLines.Count);
				AssertContainsExactElementsInAnyOrder(
					"Only one line should come through as they are freight charges.",
					new[] { costLine1.PK },
					leftLines);

				linesRepository = new RateLinesRepository(criteria, new[] { (IRateEntry)clientEntry1, clientEntry2 }.ToList(), new DummyLogger());
				matcher.RemoveSimilarCharges(parameters, linesRepository, amountByLineTable);
				leftLines = linesRepository.GetLines().Select(l => l.Line.PK).ToList();

				AssertEquals(1, leftLines.Count);
				AssertContainsExactElementsInAnyOrder(
					"Only one line should come through as they are revenue.",
					new[] { clientLine1.PK },
					leftLines);
			}
		}

		public void TestRemoveSimilarCharges_ConflictingLines_BothPercentageAppliedToCharge_KeepBoth()
		{
			var gP20PK = Helper.Containers["20GP"].PK;
			var gP40PK = Helper.Containers["40GP"].PK;

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry1 = rate.AddRateEntry("FCL", mode: "SEA", container: "20GP");
			entry1.RateLines.RemoveAll();
			var line1 = entry1.AddPercentageCharge("BAF", Helper.ChargeCodes["CAF"], 5);

			var entry2 = rate.AddRateEntry("FCL", mode: "SEA", container: "40GP");
			entry2.RateLines.RemoveAll();
			var line2 = entry2.AddPercentageCharge("BAF", Helper.ChargeCodes["FRT"], 10);

			var criteria = new TestRatingCriteria();
			criteria.RateableMeasures.AddContainerGroup(gP20PK, new[]
			{
				new MeasureInfo.ContainerInfo(100m, "KG", 1m, "M3", 1, 1, "12345")
			});
			criteria.RateableMeasures.AddContainerGroup(gP40PK, new[]
			{
				new MeasureInfo.ContainerInfo(100m, "KG", 1m, "M3", 1, 1, "5678")
			});

			var linesRepository = new RateLinesRepository(criteria, new[] { (IRateEntry)entry1, entry2 }.ToList(), new DummyLogger());
			var matcher = new LineMeasureMatcher(criteria);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var amountByLineTable = new AmountByLineTable(parameters);
			matcher.RemoveSimilarCharges(parameters, linesRepository, amountByLineTable);

			var leftLines = linesRepository.GetLines().Select(l => ToString(l.Line));
			AssertContainsExactElementsInAnyOrder(
				"Both lines should come though as they are both per charge code and only calculator itself knows whether it applies to  charge(s) or not",
				new[] { ToString(line1), ToString(line2) },
				leftLines);
		}

		public void TestRemoveSimilarCharges_ConflictingLines_OnePercentagePerChargeAnotherPerValue_RemoveBoth()
		{
			var gP20PK = Helper.Containers["20GP"].PK;
			var gP40PK = Helper.Containers["40GP"].PK;

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry1 = rate.AddRateEntry("FCL", mode: "SEA", container: "20GP");
			entry1.RateLines.RemoveAll();
			entry1.AddPercentageCharge("BAF", CalculatorConstants.Text.ApplyTo.Value.ValueOfGoods, 5);

			var entry2 = rate.AddRateEntry("FCL", mode: "SEA", container: "40GP");
			entry2.RateLines.RemoveAll();
			entry2.AddPercentageCharge("BAF", Helper.ChargeCodes["FRT"], 10);

			var criteria = new TestRatingCriteria();
			criteria.RateableMeasures.AddContainerGroup(gP20PK, new[]
			{
				new MeasureInfo.ContainerInfo(100m, "KG", 1m, "M3", 1, 1, "12345")
			});
			criteria.RateableMeasures.AddContainerGroup(gP40PK, new[]
			{
				new MeasureInfo.ContainerInfo(100m, "KG", 1m, "M3", 1, 1, "5678")
			});

			var linesRepository = new RateLinesRepository(criteria, new[] { (IRateEntry)entry1, entry2 }.ToList(), new DummyLogger());
			var matcher = new LineMeasureMatcher(criteria);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var amountByLineTable = new AmountByLineTable(parameters);
			matcher.RemoveSimilarCharges(parameters, linesRepository, amountByLineTable);

			var leftLines = linesRepository.GetLines().Select(l => ToString(l.Line));
			AssertContainsExactElementsInAnyOrder(
				"It is conflicting charges, we don't know which one to pick. Empty charge created instead.",
				new[] { "20GP BAF NTE" },
				leftLines);
		}

		public void TestRemoveSimilarCharges_MixOfCommodityGENandNonGEN_RetainPercentageLineWithNonGEN()
		{
			var gp20PK = Helper.Containers["20GP"].PK;
			// Some existing destination charge codes...
			var chargeCode1 = Helper.ChargeCodes["DCART"];
			var chargeCode2 = Helper.ChargeCodes["DFSC"];

			var rate = Helper.NewCosting(Helper.NewOrgHeader());

			// Commodity GEN - $1000 per container and a 10% surcharge
			var entryGEN = rate.AddRateEntry(RatingConstants.RateCategory.DST, mode: Core.Constants.RateMode.FCL, container: "20GP", commodity: "GEN");
			entryGEN.RateLines.RemoveAll();
			entryGEN.AddUnitCharge(chargeCode1.AC_Code, 1000m, RatingConstants.Units.CN);
			entryGEN.AddPercentageCharge(chargeCode2.AC_Code, chargeCode1, 10);

			// Commodity HAZ - $1200 per container and a 10% surcharge
			var entryHAZ = rate.AddRateEntry(RatingConstants.RateCategory.DST, mode: Core.Constants.RateMode.FCL, container: "20GP", commodity: "HAZ");
			entryHAZ.RateLines.RemoveAll();
			entryHAZ.AddUnitCharge(chargeCode1.AC_Code, 1200m, RatingConstants.Units.CN);
			entryHAZ.AddPercentageCharge(chargeCode2.AC_Code, chargeCode1, 10);

			var criteria = new TestRatingCriteria();
			var measures = criteria.RateableMeasures;
			// For shipment rating, if the user doesn't give the consol containers a commodity and the packlines all have the the same commodity
			// then that commodity is set on the measures container list. See FreightRatingHelper.SetContainersFromPackLines.
			var container1 = new MeasureInfo.ContainerInfo(containerNumber: "AAAA1111113");
			var container2 = new MeasureInfo.ContainerInfo(containerNumber: "BBBB2222227");
			measures.AddContainerWithCommodityAndNumber(gp20PK, "GEN", container1.ContainerNumber, container1);
			measures.AddContainerWithCommodityAndNumber(gp20PK, "HAZ", container2.ContainerNumber, container2);

			// Packlines have the commodity
			var packages = new RateablePartList { HasCommodity = true, HasContainerType = true };
			packages.AddPart(new RateablePart { ContainerTypePk = gp20PK.ToGuid(), CommodityCode = "GEN", PackageCount = 1 });
			packages.AddPart(new RateablePart { ContainerTypePk = gp20PK.ToGuid(), CommodityCode = "GEN", PackageCount = 2 });
			measures.AddPartList(MeasureType.Package, packages);

			var linesRepository = new RateLinesRepository(criteria, new[] { (IRateEntry)entryGEN, entryHAZ }.ToList(), new DummyLogger());
			var matcher = new LineMeasureMatcher(criteria);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var amountByLineTable = new AmountByLineTable(parameters);
			matcher.RemoveSimilarCharges(parameters, linesRepository, amountByLineTable);

			string ToStringWithCommodity(IRateLine line)
			{
				return FormattableString.Invariant(
					$"{line.ParentRateEntry.Container?.RC_Code} {line.ParentRateEntry.TI_RH_NKCommodityCode} {line.ChargeCode.AC_Code} {line.TL_RateCalculator}");
			}

			var lines = linesRepository.GetLines().Select(l => ToStringWithCommodity(l.Line));
			AssertContainsExactElementsInAnyOrder(
				"All rates are still present.",
				new[]
				{
					"20GP GEN DCART UNT",
					"20GP HAZ DCART UNT",
					"20GP GEN DFSC PER",
					"20GP HAZ DFSC PER",
				},
				lines);
		}

		public void TestRemoveSimilarCharges_DuplicateServiceType_MixedCostingAndSpotRate()
		{
			var criteria = new TestRatingCriteria();
			var chargeCode = Helper.ChargeCodes["OFUMI"];
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCode.AC_ChargeSubGroup = FreightServiceType.Codes.Fumigation;
			chargeCode.AC_IsAdhocServiceCharge = true;
			chargeCode.AC_GC = criteria.Company.PK;
			chargeCode.Factory.Save();

			var contractor = Helper.NewOrgHeader();
			var costingRate = Helper.NewCosting(contractor);
			var costingRateEntry = costingRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.ORG, TransportModes.All, "", "", "OFUMI", 250m, "SV", "AUD");

			var adhocWithSpecifiedRate = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Fumigation, "Job Service", serviceCount: 1, rate: 500m, unit: "SV", id: "1", contractor: contractor);
			adhocWithSpecifiedRate.IsCostForSpotRate = true;
			criteria.JobServices.Add(adhocWithSpecifiedRate);

			var adhocWithCostRate = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Fumigation, "Job Service", serviceCount: 1, rate: 0m, id: "2", contractor: contractor);
			criteria.JobServices.Add(adhocWithCostRate);

			var spotRateEntryCreator = new SpotRateEntryCreator(criteria);
			var serviceEntries = spotRateEntryCreator.GetAllJobServiceRates(true).ToList();
			var entries = new List<IRateEntry> { costingRateEntry };
			entries.AddRange(serviceEntries);

			var linesRepository = new RateLinesRepository(criteria, entries, new TestLogger());
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var amountByLineTable = new AmountByLineTable(parameters);
			var matcher = new LineMeasureMatcher(criteria);

			var expectedRateLines = new [] { costingRateEntry.RateLines[0].PK, serviceEntries[0].ChildRateLines.First().PK };

			AssertEquals(2, linesRepository.GetLines().Count);
			AssertContainsExactElementsInAnyOrder(expectedRateLines, linesRepository.GetLines().Select(l => l.Line.PK));

			matcher.RemoveSimilarCharges(parameters, linesRepository, amountByLineTable);

			AssertEquals(2, linesRepository.GetLines().Count);
			AssertContainsExactElementsInAnyOrder(expectedRateLines, linesRepository.GetLines().Select(l => l.Line.PK));
		}

		public void TestGetSimilarity_ProductCommodityContainerType()
		{
			var gP20PK = Helper.Containers["20GP"].PK;
			var gP40PK = Helper.Containers["40GP"].PK;
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var productPK = Helper.NewOrgSupplierPart(rate.Header).PK;
			// Creating rates that only differ in Product, Commodity and ContainerType
			var entry1 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "20GP");
			entry1.TI_RH_NKCommodityCode = "GEN";
			var entry2 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "40GP");
			entry2.TI_RH_NKCommodityCode = ZString.Empty;
			var entry3 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "");
			entry3.TI_RH_NKCommodityCode = "HAZ";
			var entry4 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "");
			entry4.TI_RH_NKCommodityCode = ZString.Empty;
			var entry5 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "STD", "");
			entry5.TI_RH_NKCommodityCode = "HAZ";
			entry5.TI_IsTact = true;

			var criteria = new TestRatingCriteria();
			var partList = new RateablePartList();
			partList.HasProduct = true;
			partList.HasCommodity = true;
			partList.HasContainerType = true;

			var lineProvider = new FastLineProvider(criteria);
			var line1 = lineProvider.GetOrCreate(entry1.RateLines[0]);
			var line2 = lineProvider.GetOrCreate(entry2.RateLines[0]);
			var line3 = lineProvider.GetOrCreate(entry3.RateLines[0]);
			var line4 = lineProvider.GetOrCreate(entry4.RateLines[0]);
			var line5 = lineProvider.GetOrCreate(entry5.RateLines[0]);

			var comparer = new LineMeasureMatcher(criteria);

			AssertEquals(999, Adjust(comparer.GetSimilarity(line1, partList, CreatePart(ZGuid.Empty, (ZString)"GEN", gP20PK))));
			AssertEquals(-1, Adjust(comparer.GetSimilarity(line2, partList, CreatePart(ZGuid.Empty, (ZString)"GEN", gP20PK))));
			AssertEquals(-1, Adjust(comparer.GetSimilarity(line3, partList, CreatePart(ZGuid.Empty, (ZString)"GEN", gP20PK))));
			AssertEquals(901, Adjust(comparer.GetSimilarity(line4, partList, CreatePart(ZGuid.Empty, (ZString)"GEN", gP20PK))));
			AssertEquals(101, Adjust(comparer.GetSimilarity(line4, partList, CreatePart(productPK, (ZString)"GEN", gP20PK))));
			AssertEquals(109, Adjust(comparer.GetSimilarity(line2, partList, CreatePart(productPK, (ZString)"GEN", gP40PK))));
			AssertEquals(109, Adjust(comparer.GetSimilarity(line2, partList, CreatePart(productPK, ZString.Empty, gP40PK))));
			AssertEquals(191, Adjust(comparer.GetSimilarity(line3, partList, CreatePart(productPK, (ZString)"HAZ", gP40PK))));
			AssertEquals(909, Adjust(comparer.GetSimilarity(line5, partList, CreatePart(ZGuid.Empty, (ZString)"GEN", ZGuid.Empty))));

			entry3.RateLines[0].TL_OP_ProductNumber = productPK;
			AssertEquals(991, Adjust(comparer.GetSimilarity(line3, partList, CreatePart(productPK, (ZString)"HAZ", gP40PK))));
			AssertEquals(-1, Adjust(comparer.GetSimilarity(line3, partList, CreatePart(ZGuid.Empty, (ZString)"HAZ", gP40PK))));
		}

		/// <summary>
		/// Adjust similarity to be just product > commodity > container type
		/// </summary>
		static long Adjust(long similarity)
		{
			if (similarity == -1)
			{
				return similarity;
			}

			const int SimilarityBase = LineMeasureMatcher.SimilarityBase;

			var product = (long)(similarity / Math.Pow(SimilarityBase, LineMeasureMatcher.ProductSimilarityRank)) % SimilarityBase;
			var commodity = (long)(similarity / Math.Pow(SimilarityBase, LineMeasureMatcher.CommoditySimilarityRank)) % SimilarityBase;
			var containerType = (long)(similarity / Math.Pow(SimilarityBase, LineMeasureMatcher.ContainerTypeSimilarityRank)) % SimilarityBase;

			return product * 100 + commodity * 10 + containerType;
		}

		static string ToString(IRateLine line)
		{
			return FormattableString.Invariant(
				$"{line.ParentRateEntry.Container?.RC_Code} {line.ChargeCode.AC_Code} {line.TL_RateCalculator}");
		}

		IRateablePart CreatePart(ZGuid productPk, ZString commodity, ZGuid containerTypePk)
		{
			var part = new RateablePart();
			if (!productPk.IsEmpty)
			{
				part.ProductPk = productPk.ToGuid();
			}
			part.CommodityCode = commodity;
			if (!containerTypePk.IsEmpty)
			{
				part.ContainerTypePk = containerTypePk.ToGuid();
			}
			return part;
		}

		protected TestHelper Helper => testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;
	}
}
