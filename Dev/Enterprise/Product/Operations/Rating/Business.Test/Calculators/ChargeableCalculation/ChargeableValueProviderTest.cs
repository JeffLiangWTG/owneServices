using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture;
using Moq;

namespace Enterprise.Rating.Business.Testing
{
	public class ChargeableValueProviderTest : RatingTestCase
	{
		public void TestCalculateChargeable_ContainerChargeableCalculationStrategy_CN()
		{
			var chargeCode = Helper.ChargeCodes["FRT"];

			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = chargeCode };

			var ratingCriteria = new RatingCriteria(null, Factory);
			var containerInfo = new MeasureInfo.ContainerInfo(22, Constants.Weight.Kilograms, 33, Constants.Volume.CubicMetres, 44, 0, ZString.Empty);
			ratingCriteria.ValuesCanBeSet = true;
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			ratingCriteria.RateableMeasures = measures;
			measures.AddContainerGroup(ZGuid.Empty, new[] { containerInfo });
			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(autoRateInfo);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AU", "");
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);

			var chargeableValueProvider = new ChargeableValueProvider(rateLine.Calculator);
			AssertEquals(1m, chargeableValueProvider.CalculateChargeable(calculatorParameters).Amount);
		}

		public void TestCalculateChargeable_ContainerChargeableCalculationStrategy_TU()
		{
			var chargeCode = Helper.ChargeCodes["FRT"];

			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = chargeCode };

			var ratingCriteria = new RatingCriteria(null, Factory);
			var containerInfo = new MeasureInfo.ContainerInfo(22, Constants.Weight.Kilograms, 33, Constants.Volume.CubicMetres, 44, 7, ZString.Empty);
			ratingCriteria.ValuesCanBeSet = true;
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			ratingCriteria.RateableMeasures = measures;
			measures.AddContainerGroup(ZGuid.Empty, new[] { containerInfo });
			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(autoRateInfo);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AU", "");
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.TU);

			var chargeableValueProvider = new ChargeableValueProvider(rateLine.Calculator);
			AssertEquals(7m, chargeableValueProvider.CalculateChargeable(calculatorParameters).Amount);
		}

		public void TestCalculateChargeable_ServiceTimeChargeableCalculationStrategy_HR()
		{
			var chargeCode = Helper.ChargeCodes["OFUMI"];
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCode.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.Fumigation;

			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = chargeCode };

			var serviceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, Core.Constants.FreightServiceType.Codes.Fumigation, "Fumigation", 1m, new TimeSpan(2, 0, 0, 0));

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.JobServices.Add(serviceInfo);

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(autoRateInfo);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.ALL, "AU", "");
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.HR);

			var chargeableValueProvider = new ChargeableValueProvider(rateLine.Calculator);
			AssertEquals(48m, chargeableValueProvider.CalculateChargeable(calculatorParameters).Amount);
		}

		public void TestCalculateChargeable_ServiceTimeChargeableCalculationStrategy_DY()
		{
			var chargeCode = Helper.ChargeCodes["OFUMI"];
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCode.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.Fumigation;

			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = chargeCode };

			var serviceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, Core.Constants.FreightServiceType.Codes.Fumigation, "Fumigation", 1m, new TimeSpan(2, 0, 0, 0));

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.JobServices.Add(serviceInfo);

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(autoRateInfo);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.ALL, "AU", "");
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.DY);

			var chargeableValueProvider = new ChargeableValueProvider(rateLine.Calculator);
			AssertEquals(2m, chargeableValueProvider.CalculateChargeable(calculatorParameters).Amount);
		}

		public void TestCalculateChargeable_ServiceTimeChargeableCalculationStrategy_WK()
		{
			var chargeCode = Helper.ChargeCodes["OFUMI"];
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCode.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.Fumigation;

			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = chargeCode };

			var serviceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, Core.Constants.FreightServiceType.Codes.Fumigation, "Fumigation", 1m, new TimeSpan(2, 0, 0, 0));

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.JobServices.Add(serviceInfo);

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(autoRateInfo);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.ALL, "AU", "");
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.WK);

			var chargeableValueProvider = new ChargeableValueProvider(rateLine.Calculator);
			AssertEquals("Expected to round up", 1m, chargeableValueProvider.CalculateChargeable(calculatorParameters).Amount);

			serviceInfo.ServiceDuration = new TimeSpan(21, 0, 0, 0);

			chargeableValueProvider = new ChargeableValueProvider(rateLine.Calculator);
			AssertEquals(3m, chargeableValueProvider.CalculateChargeable(calculatorParameters).Amount);
		}

		public void TestCalculateChargeable_DistanceChargeableCalculationStrategy_KM()
		{
			var chargeCode = Helper.ChargeCodes["ODOC"];

			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = chargeCode };

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.ValuesCanBeSet = true;
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			ratingCriteria.RateableMeasures = measures;
			measures.SetPickupDistance(22m, RatingConstants.Units.KM);
			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(autoRateInfo);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.ALL, "AU", "");
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, RatingConstants.Units.KM);

			var chargableValueProvider = new ChargeableValueProvider(rateLine.Calculator);
			AssertEquals(22m, chargableValueProvider.CalculateChargeable(calculatorParameters).Amount);
		}

		public void TestCalculateChargeable_DistanceChargeableCalculationStrategy_MI()
		{
			var chargeCode = Helper.ChargeCodes["ODOC"];

			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = chargeCode };

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.ValuesCanBeSet = true;
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			ratingCriteria.RateableMeasures = measures;
			measures.SetPickupDistance(20m, RatingConstants.Units.KM);
			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(autoRateInfo);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.ALL, "AU", "");
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, RatingConstants.Units.MI);

			var chargeableValueProvider = new ChargeableValueProvider(rateLine.Calculator);
			AssertEquals(12.427424m, chargeableValueProvider.CalculateChargeable(calculatorParameters).Amount);
		}

		public void TestCalculateChargeable_ServiceOccurrenceChargeableCalculationStrategy_SV()
		{
			var chargeCode = Helper.ChargeCodes["OFUMI"];
			chargeCode.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.Fumigation;

			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = chargeCode };

			var serviceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, Core.Constants.FreightServiceType.Codes.Fumigation, "Fumigation", 23m);

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.JobServices.Add(serviceInfo);

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(autoRateInfo);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.ALL, "AU", "");
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.SV);

			var chargeableValueProvider = new ChargeableValueProvider(rateLine.Calculator);
			AssertEquals(23m, chargeableValueProvider.CalculateChargeable(calculatorParameters).Amount);
		}

		public void TestCalculateChargeable_DefaultChargeableCalculationStrategy_KG()
		{
			var chargeCode = Helper.ChargeCodes["OFUMI"];
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCode.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.Fumigation;
			Factory.Save();

			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = chargeCode };

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.ValuesCanBeSet = true;
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			ratingCriteria.RateableMeasures = measures;
			measures.SetQuantity(MeasureType.Weight, 120m, "KG");
			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(autoRateInfo);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.ALL, "AU", "");
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, RatingConstants.Units.KG);

			var chargeableValueProvider = new ChargeableValueProvider(rateLine.Calculator);
			var message = "Expected to use DefaultChargeableCalculationStrategy. Although the charge code has a job service type, the unit does not measure any amount on job services.";

			AssertEquals(message, 120m, chargeableValueProvider.CalculateChargeable(calculatorParameters).Amount);
		}

		public void TestCalculateChargeable_DefaultChargeableCalculationStrategy()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AU", "");
			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);

			var ratingCriteria = new RatingCriteria(null, Factory);

			var parameters = new Mock<AutoRatingCalculatorParametersWithoutFilter>(ratingCriteria, new FreightAutoRater(new RatingContext()));
			parameters.Setup(m => m.GetChargeableAmount(rateLine)).Returns(new Quantity(20m, QuantityUnit.KG));

			var chargeableValueProvider = new ChargeableValueProvider(rateLine.Calculator);
			AssertEquals(20m, chargeableValueProvider.CalculateChargeable(parameters.Object).Amount);
		}
	}
}
