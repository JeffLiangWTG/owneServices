using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Business.Testing
{
	public class TimeChargeableCalculationStrategyTest : RatingTestCase
	{
		#region Time from AutoRatingCalculatorParameter's Chargeble Container Services

		public void TestGetChargeableAmount_LineUnitCN_EmptyBreakUnit()
		{
			var calculator = CreateTimeCalculator(QuantityUnit.CN, string.Empty);
			var amountCalculator = new TimeChargeableCalculationStrategy(calculator);

			var calculatorParameters = CreateParametersForTesting(new TimeSpan(0, 8, 0, 0));

			var chargeable = amountCalculator.GetChargeableAmount(calculatorParameters, QuantityUnit.HR);
			AssertEquals(8m, chargeable.Amount);
			AssertEquals(QuantityUnit.HR, chargeable.Unit);
		}

		public void TestGetChargeableAmount_LineUnitCN_BreakUnitHR()
		{
			var calculator = CreateTimeCalculator(QuantityUnit.CN, QuantityUnit.HR);
			var amountCalculator = new TimeChargeableCalculationStrategy(calculator);

			var calculatorParameters = CreateParametersForTesting(new TimeSpan(0, 8, 0, 0));

			AssertEquals(8m, amountCalculator.GetChargeableAmount(calculatorParameters, QuantityUnit.HR).Amount);
		}

		public void TestGetChargeableAmount_LineUnitCN_BreakUnitDY()
		{
			var calculator = CreateTimeCalculator(QuantityUnit.CN, QuantityUnit.DY);
			var amountCalculator = new TimeChargeableCalculationStrategy(calculator);

			var calculatorParameters = CreateParametersForTesting(new TimeSpan(2, 0, 0, 0));

			AssertEquals(2m, amountCalculator.GetChargeableAmount(calculatorParameters, QuantityUnit.DY).Amount);
		}

		public void TestGetChargeableAmount_LineUnitCN_BreakUnitWK()
		{
			var calculator = CreateTimeCalculator(QuantityUnit.CN, QuantityUnit.WK);
			var amountCalculator = new TimeChargeableCalculationStrategy(calculator);

			var calculatorParameters = CreateParametersForTesting(new TimeSpan(14, 0, 0, 0));

			AssertEquals(2m, amountCalculator.GetChargeableAmount(calculatorParameters, QuantityUnit.WK).Amount);
		}

		#endregion

		public void TestGetChargeableAmount_NoServiceFound()
		{
			var ratingCriteria = new RatingCriteria(null, Factory);
			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AU", "");
			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.HR);
			var amountCalculator = new TimeChargeableCalculationStrategy(rateLine.Calculator);

			AssertEquals(0m, amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);
		}

		public void TestGetChargeableAmount_WithFaultyJobServiceInfo_DescriptionListContainsFaultMessage()
		{
			var chargeCode = Helper.ChargeCodes.New("FUMSRV", "Fumigation Service", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation);

			var serviceInfo = JobServiceInfo.Faulty(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation, "Hello!");

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.JobServices.Add(serviceInfo);

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(new AutoRateInfo(Factory) { ChargeCode = chargeCode });

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AU", "");
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.HR);

			var amountCalculator = new TimeChargeableCalculationStrategy(rateLine.Calculator);
			var amount = amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty);

			AssertEquals("Fault message is never created with measures", 0m, amount.Amount);
		}

		public void TestGetChargeableAmount_HR()
		{
			var chargeCode = Helper.ChargeCodes.New("FUMSRV", "Fumigation Service", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation);

			var serviceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation, "Fumigation", 1m, new TimeSpan(4, 0, 0));

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.JobServices.Add(serviceInfo);

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(new AutoRateInfo(Factory) { ChargeCode = chargeCode });

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AU", "");
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.HR);

			var amountCalculator = new TimeChargeableCalculationStrategy(rateLine.Calculator);
			AssertEquals(4m, amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);
		}

		public void TestGetChargeableAmount_DY()
		{
			var chargeCode = Helper.ChargeCodes.New("FUMSRV", "Fumigation Service", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation);

			var serviceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation, "Fumigation", 1m, new TimeSpan(2, 0, 0, 0));

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.JobServices.Add(serviceInfo);

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(new AutoRateInfo(Factory) { ChargeCode = chargeCode });

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AU", "");
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.DY);

			var amountCalculator = new TimeChargeableCalculationStrategy(rateLine.Calculator);

			AssertEquals(2m, amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);

			serviceInfo.ServiceDuration += new TimeSpan(0, 12, 0, 0);

			AssertEquals("Is rounded up like TimeChargeableCalculationStrategy", 3m, amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);
		}

		public void TestGetChargeableAmountPerContainer()
		{
			var chargeCode = Helper.ChargeCodes.New("FUMSRV", "Fumigation Service", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation);

			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = chargeCode };

			var serviceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation, "Fumigation", 1m, new TimeSpan(23, 0, 0));

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.JobServices.Add(serviceInfo);

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(autoRateInfo);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AU", "");
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.HR);

			void AssertChargeableAmount(string message, ZDecimal expectedResult)
			{
				var amountCalculator = new TimeChargeableCalculationStrategy(rateLine.Calculator);
				var actualResult = amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty);
				AssertEquals(message, expectedResult, actualResult.Amount);
			}

			rateEntry.TI_RC = ZGuid.Empty;
			AssertChargeableAmount("Container type not specified on entry - returning total service duration. 1 FUMSRV 23HR", 23m);

			var containerType1PK = Helper.Containers["20GP"].PK;
			var containerType2PK = Helper.Containers["20RE"].PK;

			rateEntry.TI_RC = containerType1PK;
			// 20190131 - breaking change. 0m => 23m. Fallback to service count/time when no container links to service.
			AssertChargeableAmount("Container type specified on entry, but not found in service info", 23m);

			var containerServiceInfo1 = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation, "Fumigation", 1m, new TimeSpan(3, 0, 0), unit: QuantityUnit.HR);
			containerServiceInfo1.ContainerType = containerType2PK;
			ratingCriteria.JobServices.Add(containerServiceInfo1);

			// 20190131 - breaking change. 0m => 26m. Fallback to service count/time when no container links to service.
			AssertChargeableAmount("Container type specified on entry, but serviceInfo ServiceInfoPerContainerType does not match the type. 1 FUMSRV 23HR + 1 FUMSRV 3HR", 26m);

			var containerServiceInfo2 = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation, "Fumigation", 1m, new TimeSpan(2, 0, 0), unit: QuantityUnit.HR);
			containerServiceInfo2.ContainerType = containerType1PK;
			ratingCriteria.JobServices.Add(containerServiceInfo2);

			AssertChargeableAmount("Container type specified on entry and is now found in serviceInfo's ServiceInfoPerContainerType. 1 FUMSRV 2HR. Other 2 should be ignored", 2m);

			rateEntry.TI_RC = containerType2PK;
			AssertChargeableAmount("Container type specified on entry and found in service info. 1 FUMSRV 3HR. Other 2 should be ignored", 3m);

			rateEntry.TI_RC = ZGuid.Empty;
			AssertChargeableAmount("Should include all services", 28m);
		}

		public void TestGetMultipleChargeableAmounts()
		{
			var chargeCode = Helper.ChargeCodes.New("FUMSRV", "Fumigation Service", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation);

			var serviceInfo1 = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation, "Fumigation", 1m, new TimeSpan(2, 0, 0, 0));
			var serviceInfo2 = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation, "Fumigation", 2m, new TimeSpan(3, 0, 0, 0));

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.JobServices.Add(serviceInfo1);
			ratingCriteria.JobServices.Add(serviceInfo2);

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(new AutoRateInfo(Factory) { ChargeCode = chargeCode });

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AU", "");
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.DY);

			var amountCalculator = new TimeChargeableCalculationStrategy(rateLine.Calculator);

			AssertEquals("2x1, 3x2", string.Join(", ", amountCalculator.GetMultipleChargeableAmounts(calculatorParameters, QuantityUnit.DY)
				.OrderBy(x => x.Quantity.Amount)
				.Select(x => x.Quantity.Amount.ToString(0) + "x" + x.RepeatCount)));
		}

		public void TestGetMultipleChargeableAmounts_OneService_JobLevel()
		{
			var chargeCode = Helper.ChargeCodes.New("FUMSRV", "Fumigation Service", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation);

			var serviceInfo1 = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation, "Fumigation", 3m, new TimeSpan(2, 0, 0, 0));

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.JobServices.Add(serviceInfo1);

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(new AutoRateInfo(Factory) { ChargeCode = chargeCode });

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AU", "");
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.DY);

			var amountCalculator = new TimeChargeableCalculationStrategy(rateLine.Calculator);
			var actual = amountCalculator.GetMultipleChargeableAmounts(calculatorParameters, QuantityUnit.DY);
			AssertEquals(1, actual.Count);
			AssertEquals(2m, actual[0].Quantity.Amount);
			AssertEquals(3m, actual[0].RepeatCount);
			AssertEquals(0, actual[0].ContainerCount);
		}

		public void TestGetMultipleChargeableAmounts_OneService_PerContainer()
		{
			var chargeCode = Helper.ChargeCodes.New("FUMSRV", "Fumigation Service", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation);

			var serviceInfo1 = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation, "Fumigation", 3m, new TimeSpan(2, 0, 0, 0));
			serviceInfo1.ContainerCount = 5;

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.JobServices.Add(serviceInfo1);

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(new AutoRateInfo(Factory) { ChargeCode = chargeCode });

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AU", "");
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.DY);

			var amountCalculator = new TimeChargeableCalculationStrategy(rateLine.Calculator);
			var actual = amountCalculator.GetMultipleChargeableAmounts(calculatorParameters, QuantityUnit.DY);
			AssertEquals(1, actual.Count);
			AssertEquals(2m, actual[0].Quantity.Amount);
			AssertEquals(3m, actual[0].RepeatCount);
			AssertEquals(5, actual[0].ContainerCount);
		}

		#region Implemention

		AutoRatingCalculatorParameters CreateParametersForTesting(TimeSpan serviceDuration)
		{
			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = chargeCode };

			var serviceInfo = new JobServiceInfo(true, chargeCode.AC_ChargeGroup, chargeCode.AC_ChargeSubGroup, "Service", 1m, serviceDuration);
			serviceInfo.ContainerCount = 1;
			var containerInfo = new MeasureInfo.ContainerInfo(22, Constants.Weight.Kilograms, 33, Constants.Volume.CubicMetres, 1, 0, ZString.Empty);

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.ValuesCanBeSet = true;
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			ratingCriteria.RateableMeasures = measures;
			measures.AddContainerGroup(ZGuid.Empty, new[] { containerInfo });
			ratingCriteria.JobServices = new JobServicesCollection() { serviceInfo };

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(autoRateInfo);

			return calculatorParameters;
		}

		Calculator CreateTimeCalculator(string lineUnit, string breakUnit)
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.ALL, "AU", "");
			var rateLine = rateEntry.AddRateLine(ChargeCode, TimeCalculator.Code, lineUnit);
			rateLine.Calculator.AddRateLineItem(Calculator.Items.Operator.UNT, 0, 1, breakUnit);

			return rateLine.Calculator;
		}

		AccChargeCode ChargeCode
		{
			get { return chargeCode ?? (chargeCode = Helper.ChargeCodes.New("SRV", "Service", "", ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Cleaning)); }
		}
		AccChargeCode chargeCode;

		#endregion
	}
}
