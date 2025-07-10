using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	public class CalculationOrderResolverTest : RatingTestCase
	{
		public void TestSortingWithMinimumCalculator()
		{
			var testRate = Helper.NewClientRate(NewClient);
			var entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();

			var frt = Helper.ChargeCodes["FRT"];

			var line1 = entry.AddRateLine("BAF", PercentageCalculator.Code);
			line1.GetCalculator<PercentageCalculator>().Percent = 10;
			var item11 = line1.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			item11.TM_Text = "COD";
			item11.TM_AC = frt.PK;

			var line2 = entry.AddRateLine("FRT", MinimumCalculator.Code);
			((MinimumCalculator)line2.Calculator).IsChargeCodeMinimum = true;

			var line3 = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			line3.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var caf = Helper.ChargeCodes["CAF"];

			var line4 = entry.AddRateLine("FRT", PercentageCalculator.Code);
			var item41 = line4.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			item41.TM_Text = "COD";
			item41.TM_AC = caf.PK;

			var baf = Helper.ChargeCodes["BAF"];

			var line5 = entry.AddRateLine("CAF", PercentageCalculator.Code);
			line5.GetCalculator<PercentageCalculator>().Percent = 10m;
			var item51 = line5.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			item51.TM_Text = "COD";
			item51.TM_AC = baf.PK;

			Factory.Save();

			var ratingCriteria = new RatingCriteria(null, Factory);
			var parameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			var linesToCalculate = new List<IRateLine> { line1, line2, line3, line4, line5 };

			AssertEquals("BAF", linesToCalculate[0].ChargeCode.AC_Code);
			AssertEquals("FRT", linesToCalculate[1].ChargeCode.AC_Code);
			AssertEquals("FRT", linesToCalculate[2].ChargeCode.AC_Code);
			AssertEquals("FRT", linesToCalculate[3].ChargeCode.AC_Code);
			AssertEquals("CAF", linesToCalculate[4].ChargeCode.AC_Code);

			var linesRepo = new RateLinesRepository(ratingCriteria, new List<IRateEntry>(), new TestLogger());
			linesRepo.AddForTest(linesToCalculate.ToArray());
			parameters.SetLinesToCalculate(linesRepo);

			AssertEquals("FRT", parameters.LinesToCalculate[0].ChargeCode.AC_Code);
			AssertEquals("FRT", parameters.LinesToCalculate[1].ChargeCode.AC_Code);
			AssertEquals("FRT", parameters.LinesToCalculate[2].ChargeCode.AC_Code);
			AssertEquals("BAF", parameters.LinesToCalculate[3].ChargeCode.AC_Code);
			AssertEquals("CAF", parameters.LinesToCalculate[4].ChargeCode.AC_Code);

			AssertType<UnitCalculator>(parameters.LinesToCalculate[0].Calculator);
			AssertType<PercentageCalculator>(parameters.LinesToCalculate[1].Calculator);
			AssertType<MinimumCalculator>(parameters.LinesToCalculate[2].Calculator);
			AssertType<PercentageCalculator>(parameters.LinesToCalculate[3].Calculator);
			AssertType<PercentageCalculator>(parameters.LinesToCalculate[4].Calculator);

			var item12 = line1.GetCalculator<PercentageCalculator>().AddApplyToItem(Calculator.Items.Value.CalculationOrder);
			item12.TM_Text = Calculator.Items.Value.CalculationOrder;
			item12.TM_Value = 1;

			Factory.Save();

			parameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			linesRepo = new RateLinesRepository(ratingCriteria, new List<IRateEntry>(), new TestLogger());
			linesRepo.AddForTest(linesToCalculate.ToArray());
			parameters.SetLinesToCalculate(linesRepo);

			AssertEquals("FRT", parameters.LinesToCalculate[0].ChargeCode.AC_Code);
			AssertEquals("FRT", parameters.LinesToCalculate[1].ChargeCode.AC_Code);
			AssertEquals("BAF", parameters.LinesToCalculate[2].ChargeCode.AC_Code);
			AssertEquals("FRT", parameters.LinesToCalculate[3].ChargeCode.AC_Code);
			AssertEquals("CAF", parameters.LinesToCalculate[4].ChargeCode.AC_Code);

			AssertType<UnitCalculator>(parameters.LinesToCalculate[0].Calculator);
			AssertType<MinimumCalculator>(parameters.LinesToCalculate[1].Calculator);
			AssertType<PercentageCalculator>(parameters.LinesToCalculate[2].Calculator);
			AssertType<PercentageCalculator>(parameters.LinesToCalculate[3].Calculator);
			AssertType<PercentageCalculator>(parameters.LinesToCalculate[4].Calculator);
		}

		public void TestCalculationOrderResolvedBySequence_JobMimimum()
		{
			var testRate = Helper.NewClientRate(NewClient);
			var entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "NZAKL");
			entry.RateLines.RemoveAndDeleteAll();

			var line1 = entry.AddRateLine("FRT", FlatCalculator.Code);
			line1.GetCalculator<FlatCalculator>().BaseRate = 100m;
			line1.TL_RX_NKCurrency = "USD";

			var line2 = entry.AddRateLine("FRT", FlatCalculator.Code);
			line2.GetCalculator<FlatCalculator>().BaseRate = 200m;
			line2.TL_RX_NKCurrency = "USD";

			var line3 = entry.AddRateLine("FRT", MinimumCalculator.Code);
			line3.GetCalculator<MinimumCalculator>().MinimumValue = 300m;
			line3.TL_RX_NKCurrency = "USD";
			line3.GetCalculator<MinimumCalculator>().IsJobMinimum = true;

			var line4 = entry.AddRateLine("FRT", FlatCalculator.Code);
			line4.GetCalculator<FlatCalculator>().BaseRate = 400m;
			line4.TL_RX_NKCurrency = "USD";

			var line5 = entry.AddRateLine("FRT", FlatCalculator.Code);
			line5.GetCalculator<FlatCalculator>().BaseRate = 500m;
			line5.TL_RX_NKCurrency = "USD";

			Factory.Save();

			var testRatingCriteria = new TestRatingCriteria();
			var lineTable = new RateLinesRepository(testRatingCriteria, new List<IRateEntry>(), new TestLogger());
			lineTable.AddForTest(line1);
			lineTable.AddForTest(line2);
			lineTable.AddForTest(line3);
			lineTable.AddForTest(line4);
			lineTable.AddForTest(line5);
			var parameters = new AutoRatingCalculatorParametersForTesting(testRatingCriteria);
			parameters.SetLinesToCalculate(lineTable);

			var resolver = new CalculationOrderResolver(parameters, sort: true);

			var lastLine = parameters.LinesToCalculate.Last();
			AssertEquals("Mimimum Rate Line should in the last", line3, lastLine.Line);
		}

		public void TestIsItemApplicableToChargeCode_LoadingAndCustomsBrokerageCharges()
		{
			var testRate = Helper.NewClientRate(NewClient);
			var entry = testRate.AddRateEntry(RatingConstants.RateCategory.AIR);
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine(ChargeCodeGroupList.Codes.Freight, PercentageCalculator.Code);
			var resolver = new CalculationOrderResolver(new List<RateLine>());

			AssertChargeCodeMapping(CalculatorConstants.Text.LoadingCharges, ChargeCodeGroupList.Codes.Loading);
			AssertChargeCodeMapping(CalculatorConstants.Text.OriginCustomsBrokerageCharges, ChargeCodeGroupList.Codes.OriginBrokerage);
			AssertChargeCodeMapping(CalculatorConstants.Text.CustomsBrokerageCharges, ChargeCodeGroupList.Codes.Brokerage);
			AssertChargeCodeMapping(CalculatorConstants.Text.UnloadingCharges, ChargeCodeGroupList.Codes.Unloading);

			void AssertChargeCodeMapping(string chargeCode, string chargeGroup)
			{
				var lineItem = line.RateLineItems.AddNew();
				lineItem.TM_Type = CalculatorConstants.Type.ApplyTo;
				lineItem.TM_Text = chargeCode;

				var accChargeCode = Helper.ChargeCodes[chargeCode];
				accChargeCode.AC_ChargeGroup = chargeGroup;

				CombineAssertions($"Mapping failed for code: {chargeCode}", () =>
				{
					AssertEquals("should be mapped with correct charge group.", true, resolver.IsItemApplicableToChargeCode(lineItem, accChargeCode));
					AssertEquals("should not be mapped with incorrect charge group.", false, resolver.IsItemApplicableToChargeCode(lineItem, Helper.ChargeCodes[ChargeCodeGroupList.Codes.Freight]));
				});
			}
		}

		public void TestChargeCodeDependencyOrder_LoadingAndCustomsBrokerageCharges()
		{
			var testRate = Helper.NewClientRate(NewClient);
			var entry = testRate.AddRateEntry(RatingConstants.RateCategory.AIR);
			entry.RateLines.RemoveAndDeleteAll();

			AddDependentRateLine(entry, CalculatorConstants.Text.UnloadingCharges, CalculatorConstants.Text.CustomsBrokerageCharges);
			AddDependentRateLine(entry, CalculatorConstants.Text.LoadingCharges, ChargeCodeGroupList.Codes.Freight);
			entry.AddFlatRateLine(ChargeCodeGroupList.Codes.Freight, 1000);
			AddDependentRateLine(entry, CalculatorConstants.Text.CustomsBrokerageCharges, CalculatorConstants.Text.OriginCustomsBrokerageCharges);
			AddDependentRateLine(entry, CalculatorConstants.Text.OriginCustomsBrokerageCharges, CalculatorConstants.Text.LoadingCharges);

			Factory.Save();

			var testRatingCriteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(testRatingCriteria);
			var lineTable = new RateLinesRepository(testRatingCriteria, new List<IRateEntry>(), new TestLogger());
			lineTable.AddForTest(entry.ChildRateLines.ToArray());

			var linesToCalculate = lineTable.GetLines();

			CombineAssertions($"Before SetLinesToCalculate: Order should be based on AC_Code.", () =>
			{
				AssertEquals("BRK", CalculatorConstants.Text.CustomsBrokerageCharges, linesToCalculate[0].ChargeCode.AC_Code);
				AssertEquals("FRT", ChargeCodeGroupList.Codes.Freight, linesToCalculate[1].ChargeCode.AC_Code);
				AssertEquals("LOD", CalculatorConstants.Text.LoadingCharges, linesToCalculate[2].ChargeCode.AC_Code);
				AssertEquals("OBR", CalculatorConstants.Text.OriginCustomsBrokerageCharges, linesToCalculate[3].ChargeCode.AC_Code);
				AssertEquals("UNL", CalculatorConstants.Text.UnloadingCharges, linesToCalculate[4].ChargeCode.AC_Code);
			});

			parameters.SetLinesToCalculate(lineTable);

			CombineAssertions($"After SetLinesToCalculate: Order should be based on line item dependency.", () =>
			{
				AssertEquals("FRT", ChargeCodeGroupList.Codes.Freight, parameters.LinesToCalculate[0].ChargeCode.AC_Code);
				AssertEquals("LOD", CalculatorConstants.Text.LoadingCharges, parameters.LinesToCalculate[1].ChargeCode.AC_Code);
				AssertEquals("OBR", CalculatorConstants.Text.OriginCustomsBrokerageCharges, parameters.LinesToCalculate[2].ChargeCode.AC_Code);
				AssertEquals("BRK", CalculatorConstants.Text.CustomsBrokerageCharges, parameters.LinesToCalculate[3].ChargeCode.AC_Code);
				AssertEquals("UNL", CalculatorConstants.Text.UnloadingCharges, parameters.LinesToCalculate[4].ChargeCode.AC_Code);
			});
		}

		public void TestCalculationOrderResolver_DBHits()
		{
			var testRate = Helper.NewClientRate(NewClient);
			var entry = testRate.AddRateEntry(RatingConstants.RateCategory.AIR);
			entry.RateLines.RemoveAndDeleteAll();

			AddDependentRateLine(entry, CalculatorConstants.Text.UnloadingCharges, CalculatorConstants.Text.CustomsBrokerageCharges);
			AddDependentRateLine(entry, CalculatorConstants.Text.LoadingCharges, ChargeCodeGroupList.Codes.Freight);
			AddDependentRateLine(entry, CalculatorConstants.Text.CustomsBrokerageCharges, CalculatorConstants.Text.OriginCustomsBrokerageCharges);
			AddDependentRateLine(entry, CalculatorConstants.Text.OriginCustomsBrokerageCharges, CalculatorConstants.Text.LoadingCharges);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var loadedRateEntry = factory2.Load<RateEntry>(entry.PK);
			_ = new CalculationOrderResolver(loadedRateEntry.ChildRateLines);

			AssertDbHits(new Dictionary<string, int>
			{
				{ RatingHeaderSchema.Constants.TableName, 1 },
				{ RateEntrySchema.Constants.TableName, 1 },
				{ RateLinesSchema.Constants.TableName, 1 },
				{ RateLineItemsSchema.Constants.TableName, 1 },
				{ AccChargeCodeSchema.Constants.TableName, 1 }
			}, factory2);
		}

		RateLine AddDependentRateLine(RateEntry rateEntry, string chargeCode, string depedentChargeCode)
		{
			Helper.ChargeCodes[chargeCode].AC_ChargeGroup = chargeCode;

			var line = rateEntry.AddRateLine(chargeCode, PercentageCalculator.Code);
			var lineItem = line.RateLineItems.AddNew();
			lineItem.TM_Type = CalculatorConstants.Type.ApplyTo;
			lineItem.TM_Text = depedentChargeCode;

			return line;
		}
	}
}
