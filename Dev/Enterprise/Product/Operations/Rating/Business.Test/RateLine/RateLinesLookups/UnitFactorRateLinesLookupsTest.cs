using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	public class UnitFactorRateLinesLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUnitFactor()
		{
			var ratingHeaderTypes = new[] { RatingConstants.RatingHeaderTypes.ClientRate, RatingConstants.RatingHeaderTypes.Costing, RatingConstants.RatingHeaderTypes.Tariff, RatingConstants.RatingHeaderTypes.Quote };
			foreach (var ratingHeaderType in ratingHeaderTypes)
			{
				var ratingHeader = Helper.NewRatingHeader(ratingHeaderType, Helper.NewOrgHeader());
				var rateEntry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.AIR);
				var rateLine = rateEntry.RateLines.AddNew();

				foreach (var rateCategory in RatingConstants.RateCategory.RateCategories)
				{
					foreach (var calculatorCode in Helper.CalcCodeList)
					{
						rateEntry.TI_RateCategory = rateCategory;
						rateLine.TL_RateCalculator = calculatorCode;

						foreach (var enableWarehouseUnitFactorRatesDevelopment in new[] { true, false })
						{
							using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableWarehouseUnitFactorRatesDevelopment))
							{
								var testCase = UnitFactorTestCase.Find(UnitFactorTestCases, enableWarehouseUnitFactorRatesDevelopment, ratingHeaderType, rateCategory, calculatorCode);
								var rateLinesLookups = new RateLinesLookups(rateLine);
								if (testCase != null)
								{
									AssertUnitFactor(enableWarehouseUnitFactorRatesDevelopment, rateLinesLookups, ratingHeader.RateTypeSafe(), rateCategory, calculatorCode, expectedUnitFactors: testCase.ExpectedUnitFactors);
								}
								else
								{
									AssertUnitFactor(enableWarehouseUnitFactorRatesDevelopment, rateLinesLookups, ratingHeader.RateTypeSafe(), rateCategory, calculatorCode, expectedUnitFactors: Array.Empty<string>());
								}
							}
						}
					}
				}
			}
		}

		static readonly UnitFactorTestCase[] UnitFactorTestCases = new[]
		{
			// Client Rate
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.ClientRate, RatingConstants.RateCategory.WHS, UnitCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine, UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: true, RatingConstants.RatingHeaderTypes.ClientRate, RatingConstants.RateCategory.WHS, CombinedCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.PacksWeight, UnitFactorList.Codes.ProductLine, UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: false, RatingConstants.RatingHeaderTypes.ClientRate, RatingConstants.RateCategory.WHS, CombinedCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine, UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.ClientRate, RatingConstants.RateCategory.WHS, CartageCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine, UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.ClientRate, RatingConstants.RateCategory.WHS, CartageZoneDistanceCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine, UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.ClientRate, RatingConstants.RateCategory.WHS, FlatCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.ClientRate, RatingConstants.RateCategory.WHS, FlatPlusPerUnitCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.ClientRate, RatingConstants.RateCategory.WHS, NoteCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.ClientRate, RatingConstants.RateCategory.WHS, FirstPlusAdditionalCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.ClientRate, RatingConstants.RateCategory.WHS, ExcludeCompanyTariffsCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.ClientRate, string.Empty, string.Empty, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN }),

			// CompanyTariff
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Tariff, RatingConstants.RateCategory.WHS, UnitCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine, UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Tariff, RatingConstants.RateCategory.WHS, CombinedCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine, UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Tariff, RatingConstants.RateCategory.WHS, CartageCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine, UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Tariff, RatingConstants.RateCategory.WHS, CartageZoneDistanceCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine, UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Tariff, RatingConstants.RateCategory.WHS, FlatCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Tariff, RatingConstants.RateCategory.WHS, FlatPlusPerUnitCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Tariff, RatingConstants.RateCategory.WHS, NoteCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Tariff, RatingConstants.RateCategory.WHS, FirstPlusAdditionalCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Tariff, RatingConstants.RateCategory.WHS, ExcludeCompanyTariffsCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Tariff, string.Empty, string.Empty, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN }),

			// Costing
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Costing, RatingConstants.RateCategory.WHS, UnitCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.ProductLine, UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Costing, RatingConstants.RateCategory.WHS, CombinedCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.ProductLine, UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Costing, RatingConstants.RateCategory.WHS, CartageCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.ProductLine, UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Costing, RatingConstants.RateCategory.WHS, CartageZoneDistanceCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.ProductLine, UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Costing, RatingConstants.RateCategory.WHS, FlatCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Costing, RatingConstants.RateCategory.WHS, FlatPlusPerUnitCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.ProductLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Costing, RatingConstants.RateCategory.WHS, NoteCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.ProductLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Costing, RatingConstants.RateCategory.WHS, FirstPlusAdditionalCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.ProductLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Costing, RatingConstants.RateCategory.WHS, ExcludeCompanyTariffsCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.ProductLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Costing, string.Empty, string.Empty, expectedUnitFactors: Array.Empty<string>()),

			// Quote
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Quote, RatingConstants.RateCategory.WHS, UnitCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine, UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Quote, RatingConstants.RateCategory.WHS, CombinedCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine, UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Quote, RatingConstants.RateCategory.WHS, CartageCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine, UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Quote, RatingConstants.RateCategory.WHS, CartageZoneDistanceCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine, UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Quote, RatingConstants.RateCategory.WHS, FlatCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.PackageLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Quote, RatingConstants.RateCategory.WHS, FlatPlusPerUnitCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Quote, RatingConstants.RateCategory.WHS, NoteCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Quote, RatingConstants.RateCategory.WHS, FirstPlusAdditionalCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Quote, RatingConstants.RateCategory.WHS, ExcludeCompanyTariffsCalculator.Code, expectedUnitFactors: new[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN, UnitFactorList.Codes.ProductLine }),
			new UnitFactorTestCase(enableRegister: null, RatingConstants.RatingHeaderTypes.Quote, string.Empty, string.Empty, expectedUnitFactors: new string[] { UnitFactorList.Codes.BCN, UnitFactorList.Codes.SCN }),
		};

		public void TestUnitFactor_IntercompanyTariff()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(null);
			var entry = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR);
			var line = entry.RateLines.AddNew();
			var lookups = new RateLinesLookups(line);

			entry.TI_RateCategory = RatingConstants.RateCategory.AIR;

			AssertContainsExactElementsInAnyOrder(new[]
			{
				new CodeDescriptionPair("SAM", "Sub-Shipment of ASM")
			}, lookups.UnitFactors);
		}

		#region TestUnitFactor_CTN

		public void TestUnitFactor_CTN_ClientRate()
			=> AssertUnitFactor_CTN(Helper.NewClientRate(Helper.NewOrgHeader()), true);

		public void TestUnitFactor_CTN_CompanyTariff()
			=> AssertUnitFactor_CTN(Helper.NewCompanyTariff(), true);

		public void TestUnitFactor_CTN_Quote()
			=> AssertUnitFactor_CTN(Helper.NewQuote(Helper.NewOrgHeader()), true);

		public void TestUnitFactor_CTN_Costing()
			=> AssertUnitFactor_CTN(Helper.NewCosting(Helper.NewOrgHeader()), false);

		public void TestUnitFactor_CTN_IntercompanyTariff()
			=> AssertUnitFactor_CTN(Helper.NewIntercompanyTariff(), false);

		public static readonly HashSet<string> UnitFactorCNTExpectedList = new HashSet<string>()
		{
			//This list, we are using in RateLineModuleFiltersTest class
			RatingConstants.RateCategory.AIR,
			RatingConstants.RateCategory.FCL,
			RatingConstants.RateCategory.ORG,
			RatingConstants.RateCategory.DST,
			RatingConstants.RateCategory.CAI,
			RatingConstants.RateCategory.CFC,
			RatingConstants.RateCategory.COR,
			RatingConstants.RateCategory.CDS,
			RatingConstants.RateCategory.TBC
		};

		static readonly HashSet<string> unitFactorCNTExpectedList = new HashSet<string>()
		{
			RateType.Forwarding + RatingConstants.RateCategory.AIR + RateMode.BCN,
			RateType.Forwarding + RatingConstants.RateCategory.AIR + RateMode.SCN,
			RateType.Forwarding + RatingConstants.RateCategory.AIR + RateMode.ULD,
			RateType.Forwarding + RatingConstants.RateCategory.FCL + RateMode.SEA,
			RateType.Forwarding + RatingConstants.RateCategory.FCL + RateMode.ROA,
			RateType.Forwarding + RatingConstants.RateCategory.FCL + RateMode.RAI,
			RateType.Forwarding + RatingConstants.RateCategory.FCL + RateMode.BCN,
			RateType.Forwarding + RatingConstants.RateCategory.FCL + RateMode.SCN,
			RateType.Forwarding + RatingConstants.RateCategory.ORG + RateMode.ULD,
			RateType.Forwarding + RatingConstants.RateCategory.ORG + RateMode.FCL,
			RateType.Forwarding + RatingConstants.RateCategory.ORG + RateMode.FRO,
			RateType.Forwarding + RatingConstants.RateCategory.ORG + RateMode.FRA,
			RateType.Forwarding + RatingConstants.RateCategory.ORG + RateMode.BCN,
			RateType.Forwarding + RatingConstants.RateCategory.ORG + RateMode.SCN,
			RateType.Forwarding + RatingConstants.RateCategory.DST + RateMode.ULD,
			RateType.Forwarding + RatingConstants.RateCategory.DST + RateMode.FCL,
			RateType.Forwarding + RatingConstants.RateCategory.DST + RateMode.FRO,
			RateType.Forwarding + RatingConstants.RateCategory.DST + RateMode.FRA,
			RateType.Forwarding + RatingConstants.RateCategory.DST + RateMode.BCN,
			RateType.Forwarding + RatingConstants.RateCategory.DST + RateMode.SCN,
			RateType.Customs + RatingConstants.RateCategory.CAI + RateMode.ULD,
			RateType.Customs + RatingConstants.RateCategory.CFC + RateMode.SEA,
			RateType.Customs + RatingConstants.RateCategory.CFC + RateMode.ROA,
			RateType.Customs + RatingConstants.RateCategory.CFC + RateMode.RAI,
			RateType.Customs + RatingConstants.RateCategory.CFC + RateMode.BCN,
			RateType.Customs + RatingConstants.RateCategory.CFC + RateMode.SCN,
			RateType.Customs + RatingConstants.RateCategory.COR + RateMode.ULD,
			RateType.Customs + RatingConstants.RateCategory.COR + RateMode.FCL,
			RateType.Customs + RatingConstants.RateCategory.COR + RateMode.FRO,
			RateType.Customs + RatingConstants.RateCategory.COR + RateMode.FRA,
			RateType.Customs + RatingConstants.RateCategory.CDS + RateMode.ULD,
			RateType.Customs + RatingConstants.RateCategory.CDS + RateMode.FCL,
			RateType.Customs + RatingConstants.RateCategory.CDS + RateMode.FRO,
			RateType.Customs + RatingConstants.RateCategory.CDS + RateMode.FRA,
			RateType.TransportBookings + RatingConstants.RateCategory.TBC + RateMode.FRO,
			RateType.TransportBookings + RatingConstants.RateCategory.TBC + RateMode.FRA
		};

		public void TestUnitFactor_ModuleFilterImplementation()
		{
			//This test ensures that UnitFactorCNTExpectedList(which is used in filters in RateLineModuleFiltersTest class)
			//are in sync with unitFactorCNTExpectedList's categories list
			foreach (var item in unitFactorCNTExpectedList)
			{
				var category = item.Substring(item.Length - 6, 3);
				Assert($"{category} should present in UnitFactorCNTExpectedList", UnitFactorCNTExpectedList.Contains(category));
			}
		}

		void AssertUnitFactor_CTN(RatingHeader ratingHeader, bool isExpectedUnitFactorOnHeader)
		{
			var expected = isExpectedUnitFactorOnHeader;
			var expectedCodeDescUnitFactors = new CodeDescriptionPair("CTN", "Individual Container");
			var expectedCount = 0;

			CombineAssertions(() =>
			{
				foreach (var rateCategory in RatingConstants.RateCategory.RateCategories)
				{
					var rateType = RatingConstants.RateCategory.GetRateType(rateCategory);
					var transportModes = RateEntryLookups.GetTransportModesByRateCategory(rateCategory).ToArray();
					foreach (var mode in transportModes)
					{
						if (isExpectedUnitFactorOnHeader)
						{
							expected = unitFactorCNTExpectedList.Contains($"{rateType}{rateCategory}{mode}");
							if (expected)
							{
								expectedCount++;
							}
						}
						AssertUnitFactor(rateCategory, mode.Code, "CN", "20GP", expected);
						AssertUnitFactor(rateCategory, mode.Code, "TU", "40GP", expected);
						AssertUnitFactor(rateCategory, mode.Code, "CN", "", false);
						AssertUnitFactor(rateCategory, mode.Code, "TU", "", false);
						AssertUnitFactor(rateCategory, mode.Code, "KG", "20GP", false);
						AssertUnitFactor(rateCategory, mode.Code, "M3", "40GP", false);
					}
				}
			});

			if (isExpectedUnitFactorOnHeader)
			{
				AssertEquals($"All items in list{nameof(unitFactorCNTExpectedList)} should match", unitFactorCNTExpectedList.Count, expectedCount);
			}

			void AssertUnitFactor(string category, string mode, string unit, string container, bool isExpectedUnitFactor)
			{
				var entry = ratingHeader.AddRateEntry(category);
				var line = entry.RateLines.AddNew();
				var lookups = new RateLinesLookups(line);

				entry.TI_Mode = mode;

				if (!string.IsNullOrEmpty(container))
				{
					entry.TI_RC = Helper.Containers[container].PK;
				}

				line.TL_WeightVolume = unit;

				var unitFactors = lookups.UnitFactors.Cast<ICodeDescription>();
				if (isExpectedUnitFactor)
				{
					AssertCollectionContains($"CTN Unit factor not found for category:{category}, mode:{mode}, unit:{unit}, container:{container}",
						unitFactors, (unitFactor) => unitFactor.Equals(expectedCodeDescUnitFactors));
				}
				else
				{
					AssertCollectionNotContains($"CTN Unit factor is not expected for category:{category}, mode:{mode}, unit:{unit}, container:{container}",
						unitFactors, (unitFactor) => unitFactor.Equals(expectedCodeDescUnitFactors));
				}
			}
		}

		#endregion

		#region TestUnitFactor_LPO

		public void TestUnitFactor_LPO()
		{
			var ratingHeader = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.WHS);
			var line = entry.RateLines.AddNew();
			var lookups = new RateLinesLookups(line);

			entry.TI_RateCategory = RatingConstants.RateCategory.WHS;
			line.TL_RateCalculator = WarehousePackCalculator.Code;
			line.TL_WeightVolume = QuantityUnit.PK;

			var expectedItem = new CodeDescriptionPair("LPO", "Loaded Packages Only");
			AssertCollectionContains(expectedItem, lookups.UnitFactors.ToList<CodeDescriptionPair>());
		}

		#endregion

		#region UnitFactors

		public void TestUnitFactors_InnerPacks()
		{
			AssertFactor(rateType: "SAL", rateCategory: "FCL", calculator: "UNT", expectedFactor: UnitFactorList.Codes.InnerPack);
			AssertFactor(rateType: "SAL", rateCategory: "FCL", calculator: "FLT", expectedFactor: null, because: "Calculator doesn't require unit at all");
			AssertFactor(rateType: "SAL", rateCategory: "FCL", containerType: "20GP", calculator: "UNT", expectedFactor: null, because: "Inner pack lines allowed only on rates without container as they don't depend on containers");
			AssertFactor(rateType: "SAL", rateCategory: "WHS", calculator: "UNT", expectedFactor: null, because: "Inner pack lines allowed only Forwarding rates");

			AssertFactor(rateType: "GLB", rateCategory: "FCL", calculator: "UNT", expectedFactor: UnitFactorList.Codes.InnerPack);
			AssertFactor(rateType: "GLB", rateCategory: "FCL", calculator: "FLT", expectedFactor: null, because: "Calculator doesn't require unit at all");
			AssertFactor(rateType: "GLB", rateCategory: "FCL", containerType: "20GP", calculator: "UNT", expectedFactor: null, because: "Inner pack lines allowed only on rates without container as they don't depend on containers");
			AssertFactor(rateType: "GLB", rateCategory: "WHS", calculator: "UNT", expectedFactor: null, because: "Inner pack lines allowed only Forwarding rates");

			AssertFactor(rateType: "QTE", rateCategory: "FCL", calculator: "UNT", expectedFactor: UnitFactorList.Codes.InnerPack);
			AssertFactor(rateType: "QTE", rateCategory: "FCL", calculator: "FLT", expectedFactor: null, because: "Calculator doesn't require unit at all");
			AssertFactor(rateType: "QTE", rateCategory: "FCL", containerType: "20GP", calculator: "UNT", expectedFactor: null, because: "Inner pack lines allowed only on rates without container as they don't depend on containers");
			AssertFactor(rateType: "QTE", rateCategory: "WHS", calculator: "UNT", expectedFactor: null, because: "Inner pack lines allowed only Forwarding rates");

			AssertFactor(rateType: "COS", rateCategory: "FCL", calculator: "UNT", expectedFactor: null, because: "InnerPacks not supported in costing");

			Assert(true);
		}

		void AssertFactor(string rateType = "COS", string rateCategory = "FCL", string containerType = null, string calculator = "UNT", string expectedFactor = null, string because = null)
		{
			Header.TH_RateType = rateType;
			Entry.TI_RateCategory = rateCategory;
			Entry.TI_RC = containerType != null ? Helper.Containers[containerType].PK : ZGuid.Empty;
			Line.TL_RateCalculator = calculator;

			var actual = Line.Lookups.UnitFactors
				.Cast<CodeDescriptionPair>()
				.Select(c => c.Code)
				.ToArray();

			if (!string.IsNullOrEmpty(expectedFactor))
			{
				AssertCollectionContains(
					because ?? "Expected factor is missing.",
					expectedFactor,
					actual
				);
			}
			else
			{
				AssertCollectionNotContains(
					because ?? "Unexpected factor found.",
					expectedFactor,
					actual
				);
			}
		}

		public void TestUnitFactors_ShouldNotThrowException_WhenParentRateEntryIsNull()
		{
			var rateLine = new Mock<IRateLine>();
			rateLine.Setup(l => l.ParentRateEntry).Returns((IRateEntry)null);
			var lookup = new RateLinesLookups(rateLine.Object, Factory);

			AssertNoExceptionThrown( () => _ = lookup.UnitFactors);
		}

		#endregion

		#region Implementation

		TestHelper Helper => helper ?? (helper = new TestHelper(Factory));
		TestHelper helper;

		protected RatingHeader Header => header ?? (header = Factory.New<RatingHeader>());
		RatingHeader header;

		protected RateEntry Entry => entry ?? (entry = Header.AddRateEntry("FCL"));
		RateEntry entry;

		protected RateLine Line => Entry.RateLines[0];

		class UnitFactorTestCase
		{
			bool? EnableRegister { get; }

			string RateType { get; }

			string RateCategory { get; }

			string Calculator { get; }

			public string[] ExpectedUnitFactors { get; }

			public UnitFactorTestCase(bool? enableRegister, string rateType, string rateCategory, string calculator, string[] expectedUnitFactors)
			{
				EnableRegister = enableRegister;
				RateType = rateType;
				RateCategory = rateCategory;
				Calculator = calculator;
				ExpectedUnitFactors = expectedUnitFactors;
			}

			int Rank => (EnableRegister != null ? 1 : 0)
				+ (!string.IsNullOrEmpty(RateType) ? 10 : 0)
				+ (!string.IsNullOrEmpty(RateCategory) ? 100 : 0)
				+ (!string.IsNullOrEmpty(Calculator) ? 1000 : 0);

			public static UnitFactorTestCase Find(IEnumerable<UnitFactorTestCase> testCases, bool enableRegister, string rateType, string rateCategory, string calculator)
			{
				var results = testCases
					.Where(testCase =>
						(testCase.EnableRegister == null || testCase.EnableRegister == enableRegister)
						&& (string.IsNullOrEmpty(testCase.RateType) || testCase.RateType == rateType)
						&& (string.IsNullOrEmpty(testCase.RateCategory) || testCase.RateCategory == rateCategory)
						&& (string.IsNullOrEmpty(testCase.Calculator) || testCase.Calculator == calculator)
					)
					.OrderByDescending(testCase => testCase.Rank);

				return results.FirstOrDefault();
			}
		}

		void AssertUnitFactor(bool enableRegistry, RateLinesLookups lookups, string ratingHeaderType, string category, string calculator, string[] expectedUnitFactors)
		{
			var codes = lookups.UnitFactors.Cast<ICodeDescription>().Select(lookup => lookup.Code).ToList();

			// There is a dedicated test for InnerPack
			codes.Remove(UnitFactorList.Codes.InnerPack);

			AssertContainsExactElementsInAnyOrder
			(
				$"EnableRegistry: '{enableRegistry}', RatingHeader: '{ratingHeaderType}', category: '{category}', calculator: '{calculator}'",
				expectedUnitFactors,
				codes
			);
		}

		#endregion
	}
}
