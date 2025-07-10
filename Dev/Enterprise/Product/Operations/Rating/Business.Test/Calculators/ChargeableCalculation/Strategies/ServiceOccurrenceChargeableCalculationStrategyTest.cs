using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	public class ServiceOccurrenceChargeableCalculationStrategyTest : RatingTestCase
	{
		public void TestGetChargeableAmount_NoServiceFound()
		{
			var ratingCriteria = new RatingCriteria(null, Factory);
			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AU", "");
			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.SV);
			var amountCalculator = new ServiceOccurrenceChargeableCalculationStrategy(rateLine.Calculator);

			AssertEquals(0m, amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);
		}

		public void TestGetChargeableAmount()
		{
			var chargeCode = Helper.ChargeCodes.New("FUMSRV", "Fumigation Service", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, Core.Constants.FreightServiceType.Codes.Fumigation);

			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = chargeCode };

			var serviceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, Core.Constants.FreightServiceType.Codes.Fumigation, "Fumigation", 23);

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.JobServices.Add(serviceInfo);

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(autoRateInfo);

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = chargeCode.PK;
			rateLine.TL_RateCalculator = UnitCalculator.Code;
			rateLine.TL_WeightVolume = QuantityUnit.SV;

			var amountCalculator = new ServiceOccurrenceChargeableCalculationStrategy(rateLine.Calculator);
			AssertEquals(23m, amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);

			var amount = amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty);
			AssertEquals("Even when fault message is present, service count still returned from info", 23m, amount.Amount);
		}

		public void TestGetChargeableAmount_WithFaultyJobServiceInfo_DescriptionListContainsFaultMessage()
		{
			var chargeCode = Helper.ChargeCodes.New("FUMSRV", "Fumigation Service", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, Core.Constants.FreightServiceType.Codes.Fumigation);

			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = chargeCode };

			var serviceInfo = JobServiceInfo.Faulty(ChargeCodeGroupList.Codes.Origin, Core.Constants.FreightServiceType.Codes.Fumigation, "Hello!");

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.JobServices.Add(serviceInfo);

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(autoRateInfo);

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = chargeCode.PK;
			rateLine.TL_RateCalculator = UnitCalculator.Code;
			rateLine.TL_WeightVolume = QuantityUnit.SV;

			var amountCalculator = new ServiceOccurrenceChargeableCalculationStrategy(rateLine.Calculator);

			var amount = amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty);
			AssertEquals("Even when fault message is present, service count still returned from info", 0m, amount.Amount);
		}

		public void TestGetChargeableAmountPerContainer()
		{
			var chargeCode = Helper.ChargeCodes.New("FUMSRV", "Fumigation Service", UnitCalculator.Code, ChargeCodeGroupList.Codes.Origin, Core.Constants.FreightServiceType.Codes.Fumigation);

			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = chargeCode };

			var serviceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, Core.Constants.FreightServiceType.Codes.Fumigation, "Fumigation", 23);

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.JobServices.Add(serviceInfo);

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(autoRateInfo);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AU", "");
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.SV);

			Action<string, ZDecimal> assertChargeableAmount = (message, expectedResult) =>
			{
				var amountCalculator = new ServiceOccurrenceChargeableCalculationStrategy(rateLine.Calculator);
				var actualResult = amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount;
				AssertEquals(message, expectedResult, actualResult);
			};

			rateEntry.TI_RC = ZGuid.Empty;
			assertChargeableAmount("Container type not specified on entry - returning total service count", 23m);

			var containerType1PK = Helper.Containers["20GP"].PK;
			var containerType2PK = Helper.Containers["20RE"].PK;

			rateEntry.TI_RC = containerType1PK;

			var containerServiceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, Core.Constants.FreightServiceType.Codes.Fumigation, "Fumigation", 2, unit: QuantityUnit.SV);
			containerServiceInfo.ContainerType = containerType2PK;
			ratingCriteria.JobServices.Add(containerServiceInfo);

			containerServiceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, Core.Constants.FreightServiceType.Codes.Fumigation, "Fumigation", 4, unit: QuantityUnit.SV);
			containerServiceInfo.ContainerType = containerType1PK;
			ratingCriteria.JobServices.Add(containerServiceInfo);

			assertChargeableAmount("Container type specified on entry and is now found in serviceInfo's ServiceInfoPerContainerType", 4m);

			rateEntry.TI_RC = containerType2PK;
			assertChargeableAmount("Container type specified on entry and found in service info", 2m);

			rateEntry.TI_RC = ZGuid.Empty;
			assertChargeableAmount("Should include all services", 29m);
		}
	}
}
