using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	public class AutoRatingCalculatorParametersTest : RatingTestCase
	{
		#region Chargeable Amount

		public void TestGetChargeableAmount_CartageCalculator()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine = clientRate.AddRateEntry("ORG", "LCL", "AU", "").AddRateLine("OCART", CartageCalculator.Code, QuantityUnit.KG);
			rateLine.GetCalculator<CartageCalculator>().PerUnit = 1m;
			rateLine.ConversionFactor = new ConversionFactor(333m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			rateLine.TL_WeightVolume = "KG";

			var criteria = new TestRatingCriteria("", "", FreightMode.LCL, 500m, 1.8m, null);
			var chargeableAmount = new AutoRatingCalculatorParametersForTesting(criteria).GetChargeableAmount(rateLine);

			AssertEquals("Chargeable Unit", "KG", chargeableAmount.Unit);
			AssertEquals("Chargeable Amount", 599.4m, chargeableAmount.AmountFor("KG").Amount);
		}

		public void TestGetChargeableAmount_CartageCalculator_CC_ConversionFactor()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine = clientRate.AddRateEntry("LCL", "LCL", "AU", "").AddRateLine("OCART", CartageCalculator.Code, QuantityUnit.KG);
			rateLine.GetCalculator<CartageCalculator>().PerUnit = 1m;
			rateLine.ConversionFactor = new ConversionFactor(6000m, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms);
			rateLine.TL_WeightVolume = "KG";

			var criteria = new TestRatingCriteria("", "", FreightMode.LCL, 114.2m, 0.582m, null);
			var chargeableAmount = new AutoRatingCalculatorParametersForTesting(criteria).GetChargeableAmount(rateLine);

			AssertEquals("Chargeable Unit", "KG", chargeableAmount.Unit);
			AssertEquals("Chargeable Amount", 114.2m, chargeableAmount.AmountFor("KG").Amount);
		}

		public void TestGetChargeableAmount_CartageCalculator_Warehouse()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine = clientRate.AddRateEntry("WHS", "", "", "").AddRateLine("OCART", CartageCalculator.Code, QuantityUnit.KG);
			rateLine.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			rateLine.GetCalculator<CartageCalculator>().PerUnit = 1m;
			rateLine.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			rateLine.TL_WeightVolume = "KG";

			var criteria = new TestRatingCriteria("", "", FreightMode.UKN, 25m, 0.04, null);
			var chargeableAmount = new AutoRatingCalculatorParametersForTesting(criteria).GetChargeableAmount(rateLine);

			AssertEquals("Chargeable Unit", "KG", chargeableAmount.Unit);
			AssertEquals("Chargeable Amount", 25m, chargeableAmount.AmountFor("KG").Amount);
		}

		public void TestGetChargeableAmount()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line1 = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			var line2 = clientRate.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX").RateLines[0];
			var line3 = clientRate.AddRateEntry("LCL", "LCL", "AUBNE", "USLAX").RateLines[0];
			line3.ConversionFactor = new ConversionFactor(900m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			var criteria = new TestRatingCriteria("", "", FreightMode.LSE, 50M, 0.5M, null);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var chargeableAmount = parameters.GetChargeableAmount(line1);
			AssertEquals("Chargeable Amount", 83.333M, chargeableAmount.AmountFor("KG").Amount);
			AssertEquals("Chargeable Unit", "KG", chargeableAmount.Unit);

			criteria = new TestRatingCriteria("", "", FreightMode.LCL, 50M, 0.5M, null);
			parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			chargeableAmount = parameters.GetChargeableAmount(line2);
			AssertEquals("Chargeable Amount", .5M, chargeableAmount.AmountFor("M3").Amount);
			AssertEquals("Chargeable Unit", "M3", chargeableAmount.Unit);

			criteria = new TestRatingCriteria("", "", FreightMode.LCL, 659M, 0.5M, null);
			parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			chargeableAmount = parameters.GetChargeableAmount(line3);
			AssertEquals("Chargeable Amount", .732M, chargeableAmount.AmountFor("M3").Amount);
			AssertEquals("Chargeable Unit", "M3", chargeableAmount.Unit);

			line3.TL_WeightVolume = "T";
			chargeableAmount = parameters.GetChargeableAmount(line3);
			AssertEquals("Chargeable Amount", 659m, chargeableAmount.AmountFor("KG").Amount);
			AssertEquals("Chargeable Unit", "T", chargeableAmount.Unit);
		}

		public void TestGetChargeableAmount_ContainerSpotRate()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("FCL", "FCL", "AUSYD", "USLAX", "", "20GP");
			rateEntry.IsSpotEntry = true;
			var line = rateEntry.RateLines[0];
			line.TL_Rounding = RatingRoundingTypes.Chargeable;
			line.TL_WeightVolume = "CN";

			var criteria = new TestRatingCriteria("AUSYD", "AULAX", FreightMode.LCL, 50M, 0.5M, null);

			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var spotRateMoney = new Money(35m, usd);
			var containerInfo = new MeasureInfo.ContainerInfo(3m, Constants.Weight.Kilograms, 3m, Constants.Volume.CubicMetres, 3, 3, "DFDF1111116");
			var costSpotRateInfo = new SpotRateInfo(spotRateMoney, Constants.FreightRateAutoratingModes.Code.FreightPlusRate, AutoratedValueType.NegotiatedCost);
			var sellSpotRateInfo = new SpotRateInfo(spotRateMoney, Constants.FreightRateAutoratingModes.Code.FreightPlusRate, AutoratedValueType.SpotRate);

			var containerPK = ZGuid.NewZGuid();
			rateEntry.ContainerPKForSpotEntry = containerPK;

			containerInfo.SetSpotRates(costSpotRateInfo, sellSpotRateInfo, containerPK, gp20.PK);

			var measures = criteria.RateableMeasures;
			measures.AddContainerGroup(gp20.PK, (ZString)"GEN", new[] { containerInfo });
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var chargeableAmount = parameters.GetChargeableAmount(line);

			AssertEquals("Chargeable Amount should be 1 CN", 1m, chargeableAmount.AmountFor("CN").Amount);
		}

		public void TestGetChargeableAmount_Domestic()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "").AddRateLine("ODOC", UnitCalculator.Code, Constants.Weight.Pounds);
			rateLine.ConversionFactor = new ConversionFactor(194m, Constants.Volume.CubicInches, Constants.Weight.Pounds);

			var criteria = new TestRatingCriteria("AUSYD", "AUMEL", FreightMode.LSE, 0m, 0m, null);
			criteria.JobDirection = Directions.Domestic;

			var measures = criteria.RateableMeasures;
			measures.SetQuantity(MeasureType.Weight, 325m, "LB");
			measures.SetQuantity(MeasureType.Volume, 78125m, "CI");

			var chargeableAmount = new AutoRatingCalculatorParametersForTesting(criteria).GetChargeableAmount(rateLine);
			AssertEquals("Chargeable Amount", 402.706m, chargeableAmount.AmountFor("LB").Amount);
		}

		public void TestGetChargeableAmount_OverrideConversionFactor()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX").AddRateLine("OAQF", UnitCalculator.Code, QuantityUnit.KG);

			var criteria = new TestRatingCriteria("", "", FreightMode.LSE, 50M, 0.5M, null);

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var chargeableAmount = parameters.GetChargeableAmount(rateLine);
			AssertEquals("Chargeable Amount", 83.333M, chargeableAmount.AmountFor("KG").Amount);
			AssertEquals("Chargeable Unit", "KG", chargeableAmount.Unit);

			rateLine.ConversionFactor = new ConversionFactor(3m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			chargeableAmount = parameters.GetChargeableAmount(rateLine);
			AssertEquals("Chargeable Amount", 50M, chargeableAmount.AmountFor("KG").Amount);
			AssertEquals("Chargeable Unit", "KG", chargeableAmount.Unit);

			rateLine.ConversionFactor = ConversionFactor.Empty;
			chargeableAmount = parameters.GetChargeableAmount(rateLine);
			AssertEquals("Chargeable Amount", 83.333M, chargeableAmount.AmountFor("KG").Amount);
			AssertEquals("Chargeable Unit", "KG", chargeableAmount.Unit);
		}

		public void TestGetChargeableAmount_InvalidConversionFactor_FallsBackToDefault()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine = clientRate.AddRateEntry("LCL", "LCL", "AU", "").AddRateLine("OCART", CartageCalculator.Code, QuantityUnit.KG);

			var criteria = new TestRatingCriteria("", "", FreightMode.LSE, 50M, 0.5M, null);
			var logger = new TestLogger();

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria, new FreightAutoRater(new RatingContext(logger)));

			rateLine.TL_RateCalculator = UnitCalculator.Code;
			rateLine.ConversionFactor = new ConversionFactor(0m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			rateLine.TL_WeightVolume = "KG";
			var chargeableAmount = parameters.GetChargeableAmount(rateLine);
			AssertEquals("Chargeable Amount", 500M, chargeableAmount.AmountFor("KG").Amount);
			AssertEquals("Chargeable Unit", "KG", chargeableAmount.Unit);
			Assert("Should log warning", logger.Warnings.Any(x => x.Contains("Invalid Conversion Factor '0 KG/M3' on RateLine 'OCART-UNT-KG-Client Rate TESTORG1' has been replaced with default Conversion Factor '1000 KG/M3'")));
		}

		public void TestGetChargeableAmount_OverrideConversionFactorMultipleToTen()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line1 = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX").AddRateLine("OAQF", UnitCalculator.Code, QuantityUnit.KG);

			var testObject = new TestRatingCriteria("", "", FreightMode.LSE, 640M, 4M, null);

			var parameters = new AutoRatingCalculatorParametersForTesting(testObject);
			var chargeableAmount = parameters.GetChargeableAmount(line1);
			AssertEquals("Chargeable Amount", 666.667M, chargeableAmount.AmountFor("KG").Amount);
			AssertEquals("Chargeable Unit", "KG", chargeableAmount.Unit);

			line1.ConversionFactor = new ConversionFactor(300m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			chargeableAmount = parameters.GetChargeableAmount(line1);
			AssertEquals("Chargeable Amount", 1200M, chargeableAmount.AmountFor("KG").Amount);
			AssertEquals("Chargeable Unit", "KG", chargeableAmount.Unit);

			line1.ConversionFactor = ConversionFactor.Empty;
			chargeableAmount = parameters.GetChargeableAmount(line1);
			AssertEquals("Chargeable Amount", 666.667M, chargeableAmount.AmountFor("KG").Amount);
			AssertEquals("Chargeable Unit", "KG", chargeableAmount.Unit);
		}

		public void TestGetChargeableAmount_OverrideConversionFactor_SimilarFactor()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine = clientRate.AddRateEntry("ORG", "AIR", "USLAX", "AUSYD").AddRateLine("OAQF", UnitCalculator.Code, QuantityUnit.KG);

			var criteria = new TestRatingCriteria("USLAX", "AUSYD", FreightMode.LSE, 0m, 0m, null);
			var measures = criteria.RateableMeasures;
			measures.SetQuantity(MeasureType.Weight, 1000m, "LB");
			measures.SetQuantity(MeasureType.Volume, 305.556m, "CF");

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var chargeableAmount = parameters.GetChargeableAmount(rateLine);
			AssertEquals("Chargeable Amount with default factor (166)", 1442.064m, chargeableAmount.AmountFor("KG").Amount);

			rateLine.ConversionFactor = new ConversionFactor(250m, Constants.Volume.CubicInches, Constants.Weight.Pounds);
			chargeableAmount = parameters.GetChargeableAmount(rateLine);
			AssertEquals("Chargeable Amount with overidden factor (250) - CLOSE", 957.988m, chargeableAmount.AmountFor("KG").Amount);

			rateLine.ConversionFactor = new ConversionFactor(1200m, Constants.Volume.CubicInches, Constants.Weight.Pounds);
			chargeableAmount = parameters.GetChargeableAmount(rateLine);
			// target weight -> factor * CI = 1LB -> 1200 * CI  -> 1 LB -> 305.556 CF = 528000.768149 CI -> 528000.768149 / 1200 = 440.001 LB -> 1000 LB > 440.001 LB
			AssertEquals("Chargeable Amount with overidden factor (1200) - NOT CLOSE", 453.592m, chargeableAmount.AmountFor("KG").Amount);

			rateLine.ConversionFactor = ConversionFactor.Empty;
			chargeableAmount = parameters.GetChargeableAmount(rateLine);
			AssertEquals("Chargeable Amount with default factor (166)", 1442.064m, chargeableAmount.AmountFor("KG").Amount);
		}

		public void TestGetChargeableAmount_TeaChest()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine = clientRate.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX").RateLines[0];
			rateLine.TL_WeightVolume = Constants.Volume.TeaChest;

			var criteria = new TestRatingCriteria("", "", FreightMode.LCL, 0M, 10m, null);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			parameters.Criteria.PackageInformation = new List<PackageInformation>();
			parameters.Criteria.PackageInformation.Add(new PackageInformation(Guid.NewGuid(), "hello", 3, 130m, Core.Constants.Volume.CubicMetres, ""));
			parameters.Criteria.PackageInformation.Add(new PackageInformation(Guid.NewGuid(), "byebye", 2, 17m, Core.Constants.Volume.CubicMetres, ""));

			var chargeableAmount = parameters.GetChargeableAmount(rateLine);
			AssertEquals("Chargeable Amount", 147m, Utilities.Round(chargeableAmount.AmountFor("M3").Amount, 3));
			AssertEquals("Chargeable Unit", "M3", chargeableAmount.Unit);
		}

		public void TestGetChargeableAmount_DifferentUnits()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine1 = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			var rateLine2 = clientRate.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX").RateLines[0];

			Factory.Save();

			var criteria = new TestRatingCriteria("", "", FreightMode.LSE, 100m, 0.5m, null);
			var measures = criteria.RateableMeasures;
			measures.SetQuantity(MeasureType.Chargeable, 110m, "KG");
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var chargeableAmount = parameters.GetChargeableAmount(rateLine1);
			AssertEquals("Chargeable Amount", 100m, chargeableAmount.AmountFor("KG").Amount);
			AssertEquals("Chargeable Unit", "KG", chargeableAmount.Unit);
			rateLine1.TL_Rounding = RatingRoundingTypes.Chargeable;
			chargeableAmount = parameters.GetChargeableAmount(rateLine1);
			AssertEquals("Chargeable Amount", 110m, chargeableAmount.AmountFor("KG").Amount);
			AssertEquals("Chargeable Unit", "KG", chargeableAmount.Unit);

			criteria.RateableMeasures.SetQuantity(MeasureType.Chargeable, 0.6m, "M3");
			var wasExceptionCaught = false;
			try
			{
				chargeableAmount = parameters.GetChargeableAmount(rateLine1);
			}
			catch (AutoRaterException ex)
			{
				wasExceptionCaught = true;
				AssertEquals("Charge code FRT uses the Chargeable from the job, but the units (M3) specified on the job are different to the units specified in the rates (KG).\r\nThe units should be the same for the Rounding option of 'Use Chargeable from Job' to work.", ex.Message);
			}
			Assert(wasExceptionCaught);

			criteria.FreightMode = FreightMode.LCL;
			chargeableAmount = parameters.GetChargeableAmount(rateLine2);
			AssertEquals("Chargeable Amount", 0.5m, chargeableAmount.AmountFor("M3").Amount);
			AssertEquals("Chargeable Unit", "M3", chargeableAmount.Unit);
			rateLine2.TL_Rounding = RatingRoundingTypes.Chargeable;
			chargeableAmount = parameters.GetChargeableAmount(rateLine2);
			AssertEquals("Chargeable Amount", 0.6m, chargeableAmount.AmountFor("M3").Amount);
			AssertEquals("Chargeable Unit", "M3", chargeableAmount.Unit);

			criteria.RateableMeasures.SetQuantity(MeasureType.Chargeable, 110m, "KG");
			wasExceptionCaught = false;
			try
			{
				chargeableAmount = parameters.GetChargeableAmount(rateLine2);
			}
			catch (AutoRaterException ex)
			{
				wasExceptionCaught = true;
				AssertEquals("Charge code FRT uses the Chargeable from the job, but the units (KG) specified on the job are different to the units specified in the rates (M3).\r\nThe units should be the same for the Rounding option of 'Use Chargeable from Job' to work.", ex.Message);
			}
			Assert(wasExceptionCaught);

			measures.RemoveMeasureType(MeasureType.Chargeable);
			measures.SetQuantity(MeasureType.Weight, 1200m, "KG");
			measures.SetQuantity(MeasureType.Volume, 1m, "M3");

			chargeableAmount = parameters.GetChargeableAmount(rateLine2);
			AssertEquals("Chargeable Amount", 1.2m, chargeableAmount.AmountFor("M3").Amount);
			AssertEquals("Chargeable Unit", "M3", chargeableAmount.Unit);
		}

		public void TestChargeableAmount_ExceptionNotThrownIfRateLineUnitIsReadOnly()
		{
			var rateEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.FCL, "CNSHA", "AUSYD");
			var unitRateLine = rateEntry.AddRateLine("DDOC", UnitCalculator.Code, QuantityUnit.SV);
			unitRateLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)500m;
			unitRateLine.TL_Rounding = RatingRoundingTypes.Chargeable;

			var percentageRateLine = rateEntry.AddRateLine("DCART", PercentageCalculator.Code);
			percentageRateLine.Calculator[CalculatorConstants.Type.PER] = (ZDecimal)0.05m;
			percentageRateLine.TL_Rounding = RatingRoundingTypes.Chargeable;

			Factory.Save();

			Assert("Pre-condition: unit must be read only to avoid exception", percentageRateLine.TL_WeightVolumeInfo.ReadOnly);

			var criteria = new TestRatingCriteria("CNSHA", "AUSYD", FreightMode.FCL, 50m, 0.2m, null);
			var autoratingParams = new AutoRatingCalculatorParametersForTesting(criteria);

			AssertNoExceptionThrown("Percentage calculatior shouldn't throw an exception", () => percentageRateLine.Calculator.ChargeableAmount(autoratingParams));
		}

		public void TestChargeableAmount_CostOrCompanyTariffCalculatorExceptionAppliedToParentRateLine()
		{
			var tariffEntry = Helper.NewCompanyTariff().AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.FCL, "CNSHA", "AUSYD");
			var tariffRateLine1 = tariffEntry.AddRateLine("DDOC", FlatCalculator.Code);
			tariffRateLine1.GetCalculator<FlatCalculator>().BaseRate = 500m;
			var tariffRateLine2 = tariffEntry.AddRateLine("DCART", UnitCalculator.Code, QuantityUnit.M3);
			tariffRateLine2.GetCalculator<UnitCalculator>().PerUnit = 20m;
			var tariffRateLine3 = tariffEntry.AddRateLine("DSEC", UnitCalculator.Code, QuantityUnit.HR);
			tariffRateLine3.GetCalculator<UnitCalculator>().PerUnit = 20m;
			tariffEntry.Factory.Save();

			var client = Helper.NewOrgHeader(1);
			var clientRateEntry = Helper.NewClientRate(client).AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.FCL, "CNSHA", "AUSYD");
			var clientRateLine1 = clientRateEntry.AddRateLine("DDOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			clientRateLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 0.5;
			clientRateLine1.TL_Rounding = RatingRoundingTypes.Chargeable;
			var clientRateLine2 = clientRateEntry.AddRateLine("DCART", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			clientRateLine2.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 0.5;
			clientRateLine2.TL_Rounding = RatingRoundingTypes.Chargeable;
			var clientRateLine3 = clientRateEntry.AddRateLine("DSEC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			clientRateLine3.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 0.5;
			clientRateLine3.TL_Rounding = RatingRoundingTypes.Chargeable;
			Factory.Save();

			var criteria = new TestRatingCriteria("CNSHA", "AUSYD", FreightMode.FCL, 50m, 0.2m, client);
			criteria.SetTime(new TimeInfo(3, 0, 0));
			var autoratingParams = new AutoRatingCalculatorParametersForTesting(criteria);

			AssertNoExceptionThrown("Client Rate 1 is based on Percentage calculator so should not throw an exception", () => autoratingParams.GetChargeableAmount(clientRateLine1));
			AssertNoExceptionThrown("Client Rate 2 is based on tariff that uses M3 so no exception expected", () => autoratingParams.GetChargeableAmount(clientRateLine2));
		}

		public void TestGetChargeableAmount_DifferentUnitsOnCostOrCompanyTariffBasedCalc()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "321";
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;

			var tariff = Factory.New<CompanyTariff>();
			tariff.TH_GlobalRateLevel = 1;
			tariff.TH_GlobalRateDescription = "Company Tariff Level 1";

			var tariffRateEntry = tariff.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			var tariffRateLine = tariffRateEntry.AddRateLine(chargeCode, CombinedCalculator.Code, QuantityUnit.M3);
			tariffRateLine.RateLineItems.RemoveAndDeleteAll();

			var tariffCalc = tariffRateLine.Calculator as CombinedCalculator;
			tariffCalc["-50"] = (ZDecimal)47m;
			tariffCalc["+50"] = (ZDecimal)45m;

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var clientRateEntry = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");

			var clientRateLine = clientRateEntry.AddRateLine(chargeCode, CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			clientRateLine.RateLineItems.RemoveAndDeleteAll();

			var clientCalc = clientRateLine.Calculator as CompanyTariffOrCostBasedCalculator;
			clientCalc["-50"] = (ZDecimal)80m;
			clientCalc["+50"] = (ZDecimal)40m;

			Factory.Save();

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.AIR, 250000m, 0.05m, null);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var baseCalc = clientRateLine.Calculator.GetBaseCalculator(parameters);
			AssertEquals("Pre-condition: base calculator should belong to the tariff and not the client line", tariffRateLine.PK, baseCalc.Line.PK);

			var amount = clientRateLine.Calculator.ChargeableAmount(parameters);
			AssertEquals("Chargeable Amount should be in cubic metres", Constants.Volume.CubicMetres, amount.Unit);
		}

		public void TestChargeableAmountUsingActualPercentageToCalculate()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];

			var testObject = new TestRatingCriteria("", "", FreightMode.LSE, 50M, 0.5M, null);
			var autoratingParams = new AutoRatingCalculatorParametersForTesting(testObject);
			var chargeableAmount = autoratingParams.GetChargeableAmount(rateLine);
			AssertEquals("Chargeable Amount", 83.333M, chargeableAmount.AmountFor("KG").Amount);
			AssertEquals("Chargeable Unit", "KG", chargeableAmount.Unit);

			rateLine.TL_ActualPercentage = 20;
			chargeableAmount = autoratingParams.GetChargeableAmount(rateLine);
			AssertEquals("Chargeable Amount", 76.6664M, chargeableAmount.AmountFor("KG").Amount);
			AssertEquals("Chargeable Unit", "KG", chargeableAmount.Unit);

			rateLine.TL_ActualPercentage = 50;
			chargeableAmount = autoratingParams.GetChargeableAmount(rateLine);
			AssertEquals("Chargeable Amount", 66.6665M, chargeableAmount.AmountFor("KG").Amount);
			AssertEquals("Chargeable Unit", "KG", chargeableAmount.Unit);

			rateLine.TL_ActualPercentage = 100;
			chargeableAmount = autoratingParams.GetChargeableAmount(rateLine);
			AssertEquals("Chargeable Amount", 50.0M, chargeableAmount.AmountFor("KG").Amount);
			AssertEquals("Chargeable Unit", "KG", chargeableAmount.Unit);
		}

		public void TestChargeableAmount_ImperialAndMetricMeasurements()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine = rate.AddRateEntry("AIR", "LSE", "USNYC", "AUSYD").RateLines[0];
			rateLine.TL_WeightVolume = Constants.Weight.Pounds;
			rateLine.ConversionFactor = ConversionFactor.Standard.Imperial.Air;

			var testObject = new TestRatingCriteria("", "", FreightMode.LSE, 325m, 1.944m, null);
			var autoratingParams = new AutoRatingCalculatorParametersForTesting(testObject);
			var chargeableAmount = autoratingParams.GetChargeableAmount(rateLine);

			// 325 KG = 716.502 LB									- wins as 716.502 > 714.640
			// 1.944 M3 = 118630.16 CI = 714.640 LB (166 CI = 1 LB)
			AssertEquals("used default factor for int air -> 166 CI = 1 LB. Weight wins", 716.502m, chargeableAmount.Amount);

			rateLine.ConversionFactor = new ConversionFactor(100m, Volume.CubicInches, Weight.Pounds);
			chargeableAmount = autoratingParams.GetChargeableAmount(rateLine);

			// 324 KG = 714.298 LB
			// 1.944 M3 = 1186.302 LB (100 CI = 1 LB)				- wins as 1186.302 > 714.298
			AssertEquals("used custom factor -> 100 CI = 1 LB. Volume wins", 1186.302m, chargeableAmount.Amount);
		}

		public void TestChargeableAmount_WarehouseHandlingUnitConversionFactor()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS, "", "", "");
			var rateLine = rateEntry.AddRateLine("OAQF", UnitCalculator.Code, Constants.Weight.Pounds);

			// Warehouse Handling
			var testObject = new TestRatingCriteria("", "", FreightMode.UKN, 325m, 2.28m, null);
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			var autoratingParams = new AutoRatingCalculatorParametersForTesting(testObject);
			var chargeableAmount = autoratingParams.GetChargeableAmount(rateLine);

			// 325 KG = 716.502 LB
			// 2.28 M3 = 139134.37 CI = 717.187 LB (194 CI = 1 LB)	- wins as 717.186 > 716.502
			AssertEquals("used warehouse chargeable factor for warehouse handling. Volume wins", 717.186m, chargeableAmount.Amount);
		}

		public void TestChargeableAmount_WarehouseStorageUnitConversionFactor()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.WHS, "", "", "");
			var rateLine = rateEntry.AddRateLine("OAQF", UnitCalculator.Code, Constants.Weight.Pounds);

			var testObject = new TestRatingCriteria("", "", FreightMode.UKN, 325m, 2.28m, null);
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseStorage;
			var autoratingParams = new AutoRatingCalculatorParametersForTesting(testObject);
			var chargeableAmount = autoratingParams.GetChargeableAmount(rateLine);

			// 325 KG = 716.502 LB
			// 2.28 M3 = 139134.37 CI = 717.187 LB (194 CI = 1 LB)	- wins as 717.186 > 716.502
			AssertEquals("used transit chargeable factor for warehouse storage. Volume wins", 717.186m, chargeableAmount.Amount);
		}

		public void TestChargeableAmount_TransitTransportationUnitConversionFactor()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.TWU, Core.Constants.RateMode.AIR, "", "");
			var rateLine = rateEntry.AddRateLine("OAQF", UnitCalculator.Code, Constants.Weight.Pounds);

			// Transport Mode is AIR
			var testObject = new TestRatingCriteria("", "", FreightMode.AIR, 325m, 2.28m, null);
			testObject.ConsumerType = JobInvoicingConsumerTypes.TransitReceiveTransportationUnit;
			var autoratingParams = new AutoRatingCalculatorParametersForTesting(testObject);
			var chargeableAmount = autoratingParams.GetChargeableAmount(rateLine);

			// 325 KG = 716.502 LB
			// 2.28 M3 = 139134.37 CI = 717.187 LB (194 CI = 1 LB)	- wins as 717.186 > 716.502
			AssertEquals("used transit chargeable factor for air. Volume wins", 717.186m, chargeableAmount.Amount);

			// Transport Mode is Sea
			rateEntry.TI_Mode = Core.Constants.RateMode.SEA;
			testObject.FreightMode = FreightMode.SEA;
			chargeableAmount = autoratingParams.GetChargeableAmount(rateLine);

			// 325 KG = 716.502 LB
			// 2.28 M3 = 139134.37 CI = 717.187 LB (194 CI = 1 LB)	- wins as 717.186 > 716.502
			AssertEquals("used transit chargeable factor for sea. Volume wins", 717.186m, chargeableAmount.Amount);

			// Transport Mode is Road
			rateEntry.TI_Mode = Core.Constants.RateMode.ROA;
			rateLine.TL_WeightVolume = QuantityUnit.KG;
			testObject.FreightMode = FreightMode.ROA;
			chargeableAmount = autoratingParams.GetChargeableAmount(rateLine);

			// 325 KG 
			// 2.28 M3 = 759.24 kg (333 KG = 1 m3)					- wins as 759.424 > 325
			AssertEquals("used transit chargeable factor for road. Volume wins", 759.24m, chargeableAmount.Amount);
		}

		public void TestChargeableAmount_LoadingMeters()
		{
			FreightDataRegistry.Instance.RoadLoadingMetersWeightPerLDM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000m);
			FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine = rate.AddRateEntry("DST", "LSE", "USNYC", "AUSYD").AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.KG);

			var testObject = new TestRatingCriteria("", "", FreightMode.LSE, 800m, 2m, null);
			testObject.RateableMeasures.SetQuantity(MeasureType.LoadingMeters, 2.4m, "");

			var autoratingParams = new AutoRatingCalculatorParametersForTesting(testObject);
			var chargeableAmount = autoratingParams.GetChargeableAmount(rateLine);

			AssertEquals("Non-road freight criteria", false, testObject.IsRoadFreight);
			AssertEquals("Loading meters not used for non-road", 800m, chargeableAmount.AmountFor(Constants.Weight.Kilograms).Amount);

			rate = Helper.NewClientRate(Helper.NewOrgHeader());
			rateLine = rate.AddRateEntry("DST", "ROA", "AUMEL", "AUSYD").AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.KG);

			testObject = new TestRatingCriteria("", "", FreightMode.ROA, 800m, 2m, null);
			testObject.RateableMeasures.SetQuantity(MeasureType.LoadingMeters, 2.4m, "");

			autoratingParams = new AutoRatingCalculatorParametersForTesting(testObject);
			chargeableAmount = autoratingParams.GetChargeableAmount(rateLine);

			AssertEquals("Road freight criteria", true, testObject.IsRoadFreight);
			AssertEquals("Loading meters used for chargeable calculation", 2400m, chargeableAmount.AmountFor(Constants.Weight.Kilograms).Amount);
		}

		#endregion

		#region GetMeasureType

		public void GetMeasureType_JobCharge_ReturnJobMeasureType()
		{
			Helper.ChargeCodes.New("WOUTSTO", "Warehouse order storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards, ChargeCodeSubGroupList.Storage);

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = rate.AddRateEntry("WHS", "ALL", "", "").AddRateLine("WOUTSTO", UnitCalculator.Code, QuantityUnit.KG);
			line.TL_IsWhsJobLevelCharge = true;

			var criteria = new TestRatingCriteria("", "", FreightMode.UKN, 50M, 0.5M, null);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			line.TL_WeightVolume = "PLT";
			AssertEquals(MeasureType.ChargeablePallet, parameters.GetMeasureTypes(line).Single());

			line.TL_WeightVolume = "KG";
			AssertEquals(MeasureType.JobWeight, parameters.GetMeasureTypes(line).Single());

			line.TL_WeightVolume = "M3";
			AssertEquals(MeasureType.JobVolume, parameters.GetMeasureTypes(line).Single());

			line.TL_WeightVolume = "BOX";
			AssertEquals(MeasureType.JobUnit, parameters.GetMeasureTypes(line).Single());
		}

		public void TestGetMeasureType_StorageCharge_ReturnStorageMeasureType()
		{
			Helper.ChargeCodes.New("WOUTSTO", "Warehouse order storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards, ChargeCodeSubGroupList.Storage);
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = rate.AddRateEntry("WHS", "ALL", "", "").AddRateLine("WOUTSTO", UnitCalculator.Code, QuantityUnit.KG);

			var criteria = new TestRatingCriteria("", "", FreightMode.UKN, 50M, 0.5M, null);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			line.TL_WeightVolume = "PLT";
			AssertEquals(MeasureType.StorageUnit, parameters.GetMeasureTypes(line).Single());

			line.TL_WeightVolume = "KG";
			AssertEquals(MeasureType.StorageWeight, parameters.GetMeasureTypes(line).Single());

			line.TL_WeightVolume = "M3";
			AssertEquals(MeasureType.StorageVolume, parameters.GetMeasureTypes(line).Single());
		}

		public void TestGetMeasureType_PackageLineFactor_ReturnPackageLineMeasureType()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = rate.AddRateEntry("FCL", "ALL", "", "").AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			line.TL_UnitFactor = UnitFactorList.Codes.PackageLine;

			var criteria = new TestRatingCriteria("", "", FreightMode.UKN, 50M, 0.5M, null);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			line.TL_WeightVolume = "PK";
			AssertEquals(MeasureType.WarehousePackage, parameters.GetMeasureTypes(line).Single());

			line.TL_WeightVolume = "PLT";
			AssertEquals(MeasureType.WarehousePackage, parameters.GetMeasureTypes(line).Single());

			line.TL_WeightVolume = "KG";
			AssertEquals(MeasureType.WarehousePackageWeight, parameters.GetMeasureTypes(line).Single());

			line.TL_WeightVolume = "M3";
			AssertEquals(MeasureType.WarehousePackageVolume, parameters.GetMeasureTypes(line).Single());

			line.TL_RateCalculator = FlatCalculator.Code;
			line.TL_WeightVolume = ZString.Empty;
			AssertEquals(MeasureType.WarehousePackage, parameters.GetMeasureTypes(line).Single());
		}

		public void TestGetMeasureType_InnerPackFactor_ReturnInnerPackMeasureType()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = rate.AddRateEntry("FCL", "ALL", "", "").AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			line.TL_UnitFactor = UnitFactorList.Codes.InnerPack;

			var criteria = new TestRatingCriteria("", "", FreightMode.UKN, 50M, 0.5M, null);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			line.TL_WeightVolume = "PK";
			AssertEquals(MeasureType.InnerPacksPackage, parameters.GetMeasureTypes(line).Single());

			line.TL_WeightVolume = "KG";
			AssertEquals(MeasureType.InnerPacksWeight, parameters.GetMeasureTypes(line).Single());

			line.TL_WeightVolume = "M3";
			AssertEquals(MeasureType.InnerPacksVolume, parameters.GetMeasureTypes(line).Single());

			line.TL_WeightVolume = "BOX";
			AssertEquals(MeasureType.InnerPacksUnit, parameters.GetMeasureTypes(line).Single());
		}

		public void TestGetMeasureType()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = rate.AddRateEntry("FCL", "ALL", "", "").AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);

			var criteria = new TestRatingCriteria("", "", FreightMode.UKN, 50M, 0.5M, null);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			line.TL_WeightVolume = "PK";
			AssertEquals(MeasureType.Package, parameters.GetMeasureTypes(line).Single());

			line.TL_WeightVolume = "KG";
			AssertEquals(MeasureType.Weight, parameters.GetMeasureTypes(line).Single());

			line.TL_WeightVolume = "M3";
			AssertEquals(MeasureType.Volume, parameters.GetMeasureTypes(line).Single());

			line.TL_WeightVolume = "PLT";
			AssertEquals(MeasureType.Unit, parameters.GetMeasureTypes(line).Single());

			line.TL_RateCalculator = FlatCalculator.Code;
			line.TL_WeightVolume = ZString.Empty;
			AssertEquals(MeasureType.Unidentified, parameters.GetMeasureTypes(line).Single());
		}

		public void TestGetMeasureTypesForHighestRateCalculator()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.AIR, "", "");
			var line = entry.AddRateLine("FRT", HighestRateCalculator.Code);

			var minItem = line.RateLineItems.AddNew();
			minItem.TM_Type = Calculator.Items.Operator.MIN;
			minItem.TM_BreakWeightVolume = "";
			minItem.TM_RelevantValue = 100;
			minItem.TM_FlatAmount = 0;

			var unitItem1 = line.RateLineItems.AddNew();
			unitItem1.TM_Type = Calculator.Items.Operator.UNT;
			unitItem1.TM_BreakWeightVolume = QuantityUnit.KG;
			unitItem1.TM_RelevantValue = 100;
			unitItem1.TM_FlatAmount = 0;

			var unitItem2 = line.RateLineItems.AddNew();
			unitItem2.TM_Type = Calculator.Items.Operator.UNT;
			unitItem2.TM_BreakWeightVolume = QuantityUnit.M3;
			unitItem2.TM_RelevantValue = 100;
			unitItem2.TM_FlatAmount = 0;

			var criteria = new TestRatingCriteria("", "", FreightMode.UKN, 50M, 0.5M, null);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			AssertArrayEqualsByElements(new[] { MeasureType.Weight, MeasureType.Volume }, parameters.GetMeasureTypes(line).ToArray());
		}

		#endregion

		#region Packs Weight

		public void TestGetPacksWeightForBreakSearch()
		{
			var orgHeader = Helper.NewOrgHeader();
			var product = Helper.NewOrgSupplierPart(orgHeader);
			Helper.AddPartUnit(product, package: PkgUnit.Unit, parentPackage: PkgUnit.Pallet, quantityInParent: 2m);

			var clientRate = Helper.NewClientRate(orgHeader);
			var rateEntry = clientRate.AddRateEntry("WHS", "ALL", "AU", "US");
			var rateLine = rateEntry.AddRateLine("DDOC", CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;

			Factory.Save();

			var criteria = new TestRatingCriteria();
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false,
				optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine((0, null), (0, null), 100, ZGuid.Empty, product.PK, ProductAttributesMeasure.Empty, ZString.Empty, ZString.Empty, PkgUnit.Pallet);
			var parameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(new RatingContext()));
			parameters.AddLineMeasureMatch(MeasureType.Weight, rateLine, 0);
			var quantity = parameters.GetPacksWeightForBreakSearch(rateLine, PkgUnit.Unit);
			AssertEquals("GIVEN RateLine UnitFactor is PacksWeight THEN should get product's packsWeight i.e. 2 UNT/PLT", 2m, quantity.Amount);
		}

		public void TestGetPacksWeightForBreakSearch_Rounding()
		{
			var orgHeader = Helper.NewOrgHeader();
			var product = Helper.NewOrgSupplierPart(orgHeader);
			product.OP_Weight = 1;
			product.OP_WeightUQ = QuantityUnit.KG;
			Helper.AddPartUnit(product, package: PkgUnit.Unit, parentPackage: PkgUnit.Pallet, quantityInParent: 1.111111);

			var clientRate = Helper.NewClientRate(orgHeader);
			var rateEntry = clientRate.AddRateEntry("WHS", "ALL", "AU", "US");
			var rateLine = rateEntry.AddRateLine("DDOC", CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;

			Factory.Save();

			var criteria = new TestRatingCriteria();
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false,
				optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine((0, null), (0, null), 100, ZGuid.Empty, product.PK, ProductAttributesMeasure.Empty, ZString.Empty, ZString.Empty, PkgUnit.Pallet);
			var parameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(new RatingContext()));
			parameters.AddLineMeasureMatch(MeasureType.Weight, rateLine, 0);

			var quantity = parameters.GetPacksWeightForBreakSearch(rateLine, Weight.Kilograms);
			AssertEquals($"PacksWeight rounding should be 4 decimal places - round down", 1.1111m, quantity.Amount);

			var partUnit = product.PartUnits.GetUnitConversion(PkgUnit.Pallet, PkgUnit.Unit);
			partUnit.OF_QuantityInParent = 1.555555;
			Factory.Save();
			quantity = parameters.GetPacksWeightForBreakSearch(rateLine, Weight.Kilograms);
			AssertEquals($"PacksWeight rounding should be 4 decimal places - round up for 5", 1.5556m, quantity.Amount);

			partUnit.OF_QuantityInParent = 1.666666;
			Factory.Save();
			quantity = parameters.GetPacksWeightForBreakSearch(rateLine, Weight.Kilograms);
			AssertEquals($"PacksWeight rounding should be 4 decimal places - round up", 1.6667m, quantity.Amount);

			partUnit.OF_QuantityInParent = 1;
			product.OP_Weight = 1.111111;
			Factory.Save();
			quantity = parameters.GetPacksWeightForBreakSearch(rateLine, Weight.Kilograms);
			AssertEquals($"PacksWeight rounding should be 4 decimal places - but OP_Weight only has 3 decimal points", 1.111m, quantity.Amount);

			partUnit.OF_QuantityInParent = 2.222222;
			product.OP_Weight = 1.111111;
			Factory.Save();
			quantity = parameters.GetPacksWeightForBreakSearch(rateLine, Weight.Kilograms);
			AssertEquals($"PacksWeight rounding should be 4 decimal places - multiple OP_Weight 3 decimal points with OF_QuantityInParent 6 decimal points", 2.4689m, quantity.Amount);

			product.OP_Weight = 0;
			Factory.Save();
			quantity = parameters.GetPacksWeightForBreakSearch(rateLine, Weight.Kilograms);
			AssertEquals($"PacksWeight rounding should be 4 decimal places - OP_Weight is 0", 0m, quantity.Amount);
		}

		public void TestGetPacksWeightForBreakSearch_NoDocketLines_ShouldReturnZero()
		{
			var product = Helper.NewOrgSupplierPart(Helper.NewOrgHeader());
			Helper.AddPartUnit(product, package: PkgUnit.Unit, parentPackage: PkgUnit.Pallet, quantityInParent: 2m);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("WHS", "ALL", "AU", "US");
			var rateLine = rateEntry.AddRateLine("DDOC", CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;

			Factory.Save();

			var criteria = new TestRatingCriteria();
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false,
				optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			var parameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(new RatingContext()));

			var quantity = parameters.GetPacksWeightForBreakSearch(rateLine, PkgUnit.Unit);
			AssertEquals("GIVEN RateLine UnitFactor is PacksWeight AND no measure THEN should return 0", 0m, quantity.Amount);
		}

		public void TestGetPacksWeightForBreakSearch_PointHasInvalidPackType()
		{
			var product = Helper.NewOrgSupplierPart(Helper.NewOrgHeader());
			Helper.AddPartUnit(product, package: PkgUnit.Unit, parentPackage: PkgUnit.Pallet, quantityInParent: 2m);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("WHS", "ALL", "AU", "US");
			var rateLine = rateEntry.AddRateLine("DDOC", CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;

			Factory.Save();

			var criteria = new TestRatingCriteria();
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false,
				optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine((0, null), (0, null), 100, ZGuid.Empty, product.PK, ProductAttributesMeasure.Empty, ZString.Empty, ZString.Empty, "???");
			var parameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(new RatingContext()));
			parameters.AddLineMeasureMatch(MeasureType.Weight, rateLine, 0);

			var quantity = parameters.GetPacksWeightForBreakSearch(rateLine, PkgUnit.Unit);
			AssertEquals("GIVEN RateLine UnitFactor is PacksWeight AND invalid PackType THEN should return 0", 0m, quantity.Amount);
		}

		public void TestGetPacksWeightForBreakSearch_PointHasInvalidProduct()
		{
			var product = Helper.NewOrgSupplierPart(Helper.NewOrgHeader());
			Helper.AddPartUnit(product, package: PkgUnit.Unit, parentPackage: PkgUnit.Pallet, quantityInParent: 2m);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("WHS", "ALL", "AU", "US");
			var rateLine = rateEntry.AddRateLine("DDOC", CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;

			Factory.Save();

			var criteria = new TestRatingCriteria();
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false,
				optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);

			var invalidProductPk = ZGuid.NewZGuid();
			measures.AddWarehouseDocketNormalLine((0, null), (0, null), 100, ZGuid.Empty, invalidProductPk, ProductAttributesMeasure.Empty, ZString.Empty, ZString.Empty, PkgUnit.Pallet);
			var parameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(new RatingContext()));
			parameters.AddLineMeasureMatch(MeasureType.Weight, rateLine, 0);

			AssertExceptionThrown<Calculator.CalculationException>
			(
				"GIVEN RateLine UnitFactor is PacksWeight AND invalid product THEN should throw exception that appear as error message",
				"Cannot convert pack type 'PLT' for packs weight break search.",
				() => parameters.GetPacksWeightForBreakSearch(rateLine, PkgUnit.Unit)
			);
		}

		public void TestGetPacksWeightForBreakSearch_MultiplePointsWithSameProduct_ShouldNotAccumulateResult()
		{
			var product = Helper.NewOrgSupplierPart(Helper.NewOrgHeader());
			Helper.AddPartUnit(product, package: PkgUnit.Unit, parentPackage: PkgUnit.Pallet, quantityInParent: 2m);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("WHS", "ALL", "AU", "US");
			var rateLine = rateEntry.AddRateLine("DDOC", CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;

			Factory.Save();

			var criteria = new TestRatingCriteria();
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false,
				optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine((0, null), (0, null), 100, ZGuid.Empty, product.PK, ProductAttributesMeasure.Empty, ZString.Empty, ZString.Empty, PkgUnit.Pallet);
			var parameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(new RatingContext()));
			parameters.AddLineMeasureMatch(MeasureType.Weight, rateLine, 0);
			parameters.AddLineMeasureMatch(MeasureType.Weight, rateLine, 0);

			var quantity = parameters.GetPacksWeightForBreakSearch(rateLine, PkgUnit.Unit);
			AssertEquals("GIVEN RateLine UnitFactor is PacksWeight with 2 points for Weight THEN should not accumulate the result", 2m, quantity.Amount);
		}

		#endregion

		#region Unit Factor - Filtered Parameters

		public void TestShouldCreatedFilteredParameters_UnitFactorIsCTN()
		{
			var gp20 = Helper.Containers["20GP"];

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RatingConstants.TransportMode.SEA, container: gp20.RC_Code, removeLines: true);
			var lineFRTWithCTN = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			lineFRTWithCTN.TL_UnitFactor = UnitFactorList.Codes.CTN;
			var lineBAF = rateEntry.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.CN);
			var lineBAFWithSAM = rateEntry.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.CN);
			lineBAFWithSAM.TL_UnitFactor = UnitFactorList.Codes.SAM;

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var measures = criteria.RateableMeasures;

			measures.CreateContainerList();
			measures.AddContainerGroup(gp20.PK, new[] {
				new MeasureInfo.ContainerInfo(weight: 50, Weight.Kilograms, containerNumber: "CONT00001", container: gp20),
				new MeasureInfo.ContainerInfo(weight: 40, Weight.Kilograms, containerNumber: "CONT00002", container: gp20)
			});

			parameters.AddLineMeasureMatch(MeasureType.ContainerCount, lineFRTWithCTN, 0);
			parameters.AddLineMeasureMatch(MeasureType.ContainerCount, lineFRTWithCTN, 1);
			parameters.AddLineMeasureMatch(MeasureType.ContainerCount, lineBAF, 0);
			parameters.AddLineMeasureMatch(MeasureType.ContainerCount, lineBAF, 1);
			parameters.AddLineMeasureMatch(MeasureType.ContainerCount, lineBAFWithSAM, 0);
			parameters.AddLineMeasureMatch(MeasureType.ContainerCount, lineBAFWithSAM, 1);

			(var isSplit, var splitParameterList) = parameters.CreatedFilteredParametersIfNeeded(new FastLine(lineFRTWithCTN, criteria));
			AssertEquals("line1 should split as it has unit factor as Container", true, isSplit);
			AssertNotNull(splitParameterList);
			AssertEquals(2, splitParameterList.Count());

			(isSplit, splitParameterList) = parameters.CreatedFilteredParametersIfNeeded(new FastLine(lineBAF, criteria));
			AssertEquals("line2 should not split as it has no unit factor defined", false, isSplit);
			AssertNull(splitParameterList);

			(isSplit, splitParameterList) = parameters.CreatedFilteredParametersIfNeeded(new FastLine(lineBAFWithSAM, criteria));
			AssertEquals("line3 should not split as it has unit factor as SAM", false, isSplit);
			AssertNull(splitParameterList);
		}

		#endregion
	}
}
