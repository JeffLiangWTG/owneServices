using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class EqualizationCalculatorTest : CalculatorTest
	{
		public override void TestCheckOrCreateItems()
		{
			AssertNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.Plus));
			AssertNull(Line.RateLineItems.FindByTM_Type(EqualizationCalculator.Items.UseInclusiveBreaks));
			InitialiseTestCalculator();
			AssertNotNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.Plus));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(EqualizationCalculator.Items.UseInclusiveBreaks));
		}

		public override void TestMapping()
		{
			TestMapping(EqualizationCalculator.Items.UseInclusiveBreaks, "Bool1");
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode(CalculatorCode);
		}

		public override void TestGetCloneLineItems()
		{
			Assert(true); //TODO
		}

		public override void TestQuotationLines()
		{
			Assert(true); //TODO
		}

		public override void TestDocLineAmount()
		{
			Assert("Only for costing, not Quotation", true);
		}

		protected override Type CalculatorType
		{
			get { return typeof(EqualizationCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return EqualizationCalculator.Code; }
		}

		List<RateLine> TestRateLines
		{
			get
			{
				var costing = Helper.NewCosting(Helper.NewOrgHeader());
				var line1 = costing.AddRateEntry("AIR", "LSE", "AUSYD", "UAODS").RateLines[0];
				line1.TL_RateCalculator = EqualizationCalculator.Code;

				var line2 = costing.AddRateEntry("FCL", "SEA", "AUSYD", "UAODS").RateLines[0];
				line2.TL_RateCalculator = EqualizationCalculator.Code;
				line2.TL_WeightVolume = QuantityUnit.M3;

				var line3 = costing.AddRateEntry(RatingConstants.RateCategory.CAI, Core.Constants.RateMode.LSE, "AUSYD", "UAODS").RateLines[0];
				line3.TL_RateCalculator = EqualizationCalculator.Code;

				return new List<RateLine> { line1, line2, line3 };
			}
		}

		public void TestModeContractAndExpiryDateValidation()
		{
			foreach (var line in TestRateLines)
			{
				if (line.Parent.TI_RateCategory.ToString().In(RatingConstants.RateCategory.AIR, RatingConstants.RateCategory.CAI))
				{
					AssertEquals($"Category: {line.Parent.TI_RateCategory}, Mode should be defaulted to ULD", Core.Constants.RateMode.ULD, line.Parent.TI_Mode);
					AssertEquals($"Category: {line.Parent.TI_RateCategory}, Mode column should be read-only", true, line.Parent.TI_ModeInfo.ReadOnly);
				}
				else
				{
					AssertEquals($"Category: {line.Parent.TI_RateCategory}, Mode column should not be read-only", false, line.Parent.TI_ModeInfo.ReadOnly);
				}

				Assert("Contract No. should be mandatory", line.Parent.TI_ContractNumberInfo.HasErrors());
				line.Parent.TI_RateEndDate = ZDate.Empty;
				Assert("Expiry Date should be mandatory", line.Parent.TI_RateEndDateInfo.HasErrors());
			}
		}

		public void TestTL_WeightVolume()
		{
			foreach (var line in TestRateLines)
			{
				var lookups = new RateLinesLookups(line);
				if (line.Parent.TI_RateCategory == RatingConstants.RateCategory.AIR)
				{
					Assert(line.TL_WeightVolumeInfo.ReadOnly);
					Assert("Lookups should not be filtered", lookups.WeightVolumes.ContainsCode("TN"));
					AssertEquals(QuantityUnit.KG, line.TL_WeightVolume);
				}
				else if (line.Parent.TI_RateCategory == RatingConstants.RateCategory.CAI)
				{
					CombineAssertions($"Category: {line.Parent.TI_RateCategory}", () =>
					{
						Assert("Unit readonly", line.TL_WeightVolumeInfo.ReadOnly);
						Assert("Lookups should not be filtered", lookups.WeightVolumes.ContainsCode("TN"));
						AssertEquals("Unit", QuantityUnit.KG, line.TL_WeightVolume);
					});
				}
				else
				{
					Assert(!line.TL_WeightVolumeInfo.ReadOnly);
					Assert("Should contain only certain units", lookups.WeightVolumes.ContainsOnly(QuantityUnit.CN, QuantityUnit.M3, QuantityUnit.TU));
					AssertEquals(QuantityUnit.M3, line.TL_WeightVolume);
				}
			}
		}

		public void TestTL_WeightVolume_Complex()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());

			// Forwarding
			var airRateLine = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.ULD, "AUSYD", "UAODS").RateLines[0];
			airRateLine.TL_RateCalculator = EqualizationCalculator.Code;
			AssertTL_WeightVolume(airRateLine, expectedReadOnly: true, expectedValue: QuantityUnit.KG, expectedLookup: "BAG, BLC, BLU, BSK, BOT, BOX, BBK, BBG, BND, CTN, CAS, COI, CN, CRD, CRT, CC, D3, CF, CI, M3, CY, CYL, DY, DT, DOZ, DRM, ENV, G, GRS, HG, HR, HB, GI, KEG, KG, KM, KT, L, LM, TL, LW, ML, MC, MI, MG, MIX, OZ, OT, PK, PKG, PAI, PLT, PCE, LB, LT, REL, RLL, ROR, SV, SHT, TN, SKD, SPL, TE, T, TOT, TUB, UNT, GA, WK");

			var fclRateLine = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "AUSYD", "UAODS").RateLines[0];
			fclRateLine.TL_RateCalculator = EqualizationCalculator.Code;
			fclRateLine.TL_WeightVolume = QuantityUnit.M3;
			AssertTL_WeightVolume(fclRateLine, expectedReadOnly: false, expectedValue: QuantityUnit.M3, expectedLookup: "M3, CN, TU");

			var lclRateLine = costing.AddRateEntry(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "AUSYD", "UAODS").RateLines[0];
			lclRateLine.TL_RateCalculator = EqualizationCalculator.Code;
			AssertTL_WeightVolume(lclRateLine, expectedReadOnly: false, expectedValue: QuantityUnit.M3, expectedLookup: "BAG, BLC, BLU, BSK, BOT, BOX, BBK, BBG, BND, CTN, CAS, COI, CN, CRD, CRT, CC, D3, CF, CI, M3, CY, CYL, DY, DT, DOZ, DRM, ENV, G, GRS, HG, HR, HB, GI, KEG, KG, KM, KT, L, LM, TL, LW, ML, MC, MI, MG, MIX, OZ, OT, PK, PKG, PAI, PLT, PCE, LB, LT, REL, RLL, ROR, SV, SHT, TN, SKD, SPL, TE, T, TOT, TUB, UNT, GA, WK");

			var orgRateEntry = costing.AddRateEntry(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "UAODS");
			var orgRateLine = orgRateEntry.AddRateLine("ODOC", EqualizationCalculator.Code, QuantityUnit.M3);
			AssertTL_WeightVolume(orgRateLine, expectedReadOnly: false, expectedValue: QuantityUnit.M3, expectedLookup: "BAG, BLC, BLU, BSK, BOT, BOX, BBK, BBG, BND, CTN, CAS, COI, CN, CRD, CRT, CC, D3, CF, CI, M3, CY, CYL, DY, DT, DOZ, DRM, ENV, G, GRS, HG, HR, HB, GI, KEG, KG, KM, KT, L, LM, TL, LW, ML, MC, MI, MG, MIX, OZ, OT, PK, PKG, PAI, PLT, PCE, LB, LT, REL, RLL, ROR, SV, SHT, TN, SKD, SPL, TE, T, TOT, TUB, UNT, GA, WK");

			var dstRateEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, Constants.RateMode.BLK, "AUSYD", "UAODS");
			var dstRateLine = dstRateEntry.AddRateLine("ODOC", EqualizationCalculator.Code, QuantityUnit.M3);
			AssertTL_WeightVolume(dstRateLine, expectedReadOnly: false, expectedValue: QuantityUnit.M3, expectedLookup: "BAG, BLC, BLU, BSK, BOT, BOX, BBK, BBG, BND, CTN, CAS, COI, CN, CRD, CRT, CC, D3, CF, CI, M3, CY, CYL, DY, DT, DOZ, DRM, ENV, G, GRS, HG, HR, HB, GI, KEG, KG, KM, KT, L, LM, TL, LW, ML, MC, MI, MG, MIX, OZ, OT, PK, PKG, PAI, PLT, PCE, LB, LT, REL, RLL, ROR, SV, SHT, TN, SKD, SPL, TE, T, TOT, TUB, TU, UNT, GA, WK");

			// Customs
			var caiRateLine = costing.AddRateEntry(RatingConstants.RateCategory.CAI, Constants.RateMode.ULD, "AUSYD", "UAODS").RateLines[0];
			caiRateLine.TL_RateCalculator = EqualizationCalculator.Code;
			AssertTL_WeightVolume(caiRateLine, expectedReadOnly: true, expectedValue: QuantityUnit.KG, expectedLookup: "BAG, BLC, BLU, BSK, BOT, BOX, BBK, BBG, BND, CTN, CAS, COI, CN, CRD, CRT, CC, D3, CF, CI, M3, CY, CYL, DY, DT, DOZ, DRM, ENV, G, GRS, HG, HR, HB, GI, KEG, KG, KM, KT, L, LM, TL, LW, ML, MC, MI, MG, MIX, OZ, OT, PK, PKG, PAI, PLT, PCE, LB, LT, REL, RLL, ROR, SV, SHT, TN, SKD, SPL, TE, T, TOT, TUB, UNT, GA, WK");

			var cfcRateLine = costing.AddRateEntry(RatingConstants.RateCategory.CFC, Constants.RateMode.SEA, "AUSYD", "UAODS").RateLines[0];
			cfcRateLine.TL_RateCalculator = EqualizationCalculator.Code;
			cfcRateLine.TL_WeightVolume = QuantityUnit.M3;
			AssertTL_WeightVolume(cfcRateLine, expectedReadOnly: false, expectedValue: QuantityUnit.M3, expectedLookup: "M3, CN, TU");

			var clcRateLine = costing.AddRateEntry(RatingConstants.RateCategory.CLC, Constants.RateMode.LCL, "AUSYD", "UAODS").RateLines[0];
			clcRateLine.TL_RateCalculator = EqualizationCalculator.Code;
			AssertTL_WeightVolume(clcRateLine, expectedReadOnly: false, expectedValue: QuantityUnit.M3, expectedLookup: "BAG, BLC, BLU, BSK, BOT, BOX, BBK, BBG, BND, CTN, CAS, COI, CN, CRD, CRT, CC, D3, CF, CI, M3, CY, CYL, DY, DT, DOZ, DRM, ENV, G, GRS, HG, HR, HB, GI, KEG, KG, KM, KT, L, LM, TL, LW, ML, MC, MI, MG, MIX, OZ, OT, PK, PKG, PAI, PLT, PCE, LB, LT, REL, RLL, ROR, SV, SHT, TN, SKD, SPL, TE, T, TOT, TUB, UNT, GA, WK");

			var corRateEntry = costing.AddRateEntry(RatingConstants.RateCategory.COR, Constants.RateMode.AIR, "AUSYD", "UAODS");
			var corRateLine = corRateEntry.AddRateLine("ODOC", EqualizationCalculator.Code, QuantityUnit.M3);
			AssertTL_WeightVolume(corRateLine, expectedReadOnly: false, expectedValue: QuantityUnit.M3, expectedLookup: "BAG, BLC, BLU, BSK, BOT, BOX, BBK, BBG, BND, CTN, CAS, COI, CN, CRD, CRT, CC, D3, CF, CI, M3, CY, CYL, DY, DT, DOZ, DRM, ENV, G, GRS, HG, HR, HB, GI, KEG, KG, KM, KT, L, LM, TL, LW, ML, MC, MI, MG, MIX, OZ, OT, PK, PKG, PAI, PLT, PCE, LB, LT, REL, RLL, ROR, SV, SHT, TN, SKD, SPL, TE, T, TOT, TUB, UNT, GA, WK");

			var cdsRateEntry = costing.AddRateEntry(RatingConstants.RateCategory.CDS, Constants.RateMode.BLK, "AUSYD", "UAODS");
			var cdsRateLine = cdsRateEntry.AddRateLine("ODOC", EqualizationCalculator.Code, QuantityUnit.M3);
			AssertTL_WeightVolume(cdsRateLine, expectedReadOnly: false, expectedValue: QuantityUnit.M3, expectedLookup: "BAG, BLC, BLU, BSK, BOT, BOX, BBK, BBG, BND, CTN, CAS, COI, CN, CRD, CRT, CC, D3, CF, CI, M3, CY, CYL, DY, DT, DOZ, DRM, ENV, G, GRS, HG, HR, HB, GI, KEG, KG, KM, KT, L, LM, TL, LW, ML, MC, MI, MG, MIX, OZ, OT, PK, PKG, PAI, PLT, PCE, LB, LT, REL, RLL, ROR, SV, SHT, TN, SKD, SPL, TE, T, TOT, TUB, TU, UNT, GA, WK");

			void AssertTL_WeightVolume(RateLine rateLine, bool expectedReadOnly, string expectedValue, string expectedLookup)
			{
				var rateLinesLookups = new RateLinesLookups(rateLine);
				CombineAssertions("WeightVolume", () =>
				{
					AssertEquals("ReadOnly", expectedReadOnly, rateLine.TL_WeightVolumeInfo.ReadOnly);
					AssertEquals("Lookups", expectedLookup, rateLinesLookups.WeightVolumes.CodesAsString);
					AssertEquals("Value", expectedValue, rateLine.TL_WeightVolume);
				});
			}
		}

		public void TestActualWeightVolumeTickedAndReadOnly()
		{
			foreach (var line in TestRateLines)
			{
				Assert(line.UseOnlyActualWeightMeasureInfo.ReadOnly);
				AssertEquals("Actual should be checked", true, line.UseOnlyActualWeightMeasure);
			}
		}

		public void TestFirstCalculatorLineHasPlusOperator()
		{
			foreach (var line in TestRateLines)
			{
				var testCalculator = line.Calculator as EqualizationCalculator;

				AssertEquals("First rate line item should have Plus operator", true, line.Calculator.RateLineItems[0].RateOperatorIsPlus());
			}
		}

		public void TestHasAtLeastTwoRows()
		{
			foreach (var line in TestRateLines)
			{
				AssertEquals("Should have at least two rows", 2, line.RateLineItems.Count);
			}
		}

		public void TestAddingSecondCalculatorLine()
		{
			foreach (var line in TestRateLines)
			{
				AssertEquals("First rate line item should have Plus operator", true, line.Calculator.RateLineItems[0].RateOperatorIsPlus());
				var ratelineitem2 = line.Calculator.RateLineItems.AddNew();

				AssertEquals("First rate line item should have Minus operator", true, line.Calculator.RateLineItems[0].RateOperatorIsMinus());
				AssertEquals("Second rate line item should have Plus operator", true, line.Calculator.RateLineItems[1].RateOperatorIsPlus());
				AssertEquals("First rate line Break should be Equal to Second rate line Break", line.Calculator.RateLineItems[0].TM_Break, line.Calculator.RateLineItems[1].TM_Break);
				AssertEquals("Second line Break cell should be readonly", true, line.Calculator.RateLineItems[1].TM_BreakInfo.ReadOnly);
				if (line.Parent.TI_RateCategory.ToString().In(RatingConstants.RateCategory.AIR, RatingConstants.RateCategory.CAI))
				{
					AssertEquals("Volume Discount is achieved if Average KG across selected jobs reaches the Pivot Break, then the second rate is applied to the actual KG. Otherwise the first rate is applied to the actual KG.", ((EqualizationCalculator)line.Calculator).ContractHint);
				}
				else
				{
					AssertEquals("Volume Discount is achieved if Average M3 across selected jobs reaches the Pivot Break, then the second rate is applied to the actual M3. Otherwise the first rate is applied to the actual M3.", ((EqualizationCalculator)line.Calculator).ContractHint);
				}

				line.Calculator.RateLineItems[0].TM_Break = 122.0d;
				AssertEquals(line.Calculator.RateLineItems[0].TM_Break, line.Calculator.RateLineItems[1].TM_Break);

				line.Calculator.RateLineItems.Remove(ratelineitem2.PK);
				AssertEquals("First rate line item should have Plus operator", true, line.Calculator.RateLineItems[0].RateOperatorIsPlus());
				if (line.Parent.TI_RateCategory.ToString().In(RatingConstants.RateCategory.AIR, RatingConstants.RateCategory.CAI))
				{
					AssertEquals("Volume Discount is achieved if Average KG across selected jobs reaches the Pivot Break, then the rate is applied to the actual job KG. Otherwise the rate is applied to the Pivot Break KG per job.", ((EqualizationCalculator)line.Calculator).ContractHint);
				}
				else
				{
					AssertEquals("Volume Discount is achieved if Average M3 across selected jobs reaches the Pivot Break, then the rate is applied to the actual job M3. Otherwise the rate is applied to the Pivot Break M3 per job.", ((EqualizationCalculator)line.Calculator).ContractHint);
				}

				Assert("Do not allow to remove the only line", !line.Calculator.RateLineItems.AllowRemove);
			}
		}

		public void TestNoPivotWeight()
		{
			Line.Parent.TI_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-8").PK;
			Line.TL_WeightVolume = QuantityUnit.KG;
			var containerInfos = new[]
			{
				new MeasureInfo.ContainerInfo(100m, Constants.Weight.Kilograms, 0m, Constants.Volume.CubicMetres, 0, 0, "CONT00001"),
				new MeasureInfo.ContainerInfo(500m, Constants.Weight.Kilograms, 0m, Constants.Volume.CubicMetres, 0, 0, "CONT00002")
			};

			var measures = Criteria.RateableMeasures;
			new TestContainers(Factory, "LD-8", containerInfos).PopulateContainerList(measures);
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false,
				optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			var productPk = ZGuid.NewZGuid();
			var commodityCode = "HAZ";
			measures.AddWarehouseDocketNormalLine((3, null), (0, null), 1, ZGuid.Empty, productPk, ProductAttributesMeasure.Empty, commodityCode, ZString.Empty, ZString.Empty);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.AddLineMeasureMatch(MeasureType.Weight, Line, 0);

			using (_Rating.Start(new LoggerDecorator(), isEqualization: true))
			using (_Rating.StartSubSession(Criteria.AutoRatedFor.First()))
			{
				AssertCalculation(parameters, "pivot value was not specified for this Rate. Volume Equalization cannot be performed.");
			}
		}
	}
}
