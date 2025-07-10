using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Schema;
using Charge = WiseRates.Api.Model.Charge;
using CustomField = WiseRates.Api.Model.CustomField;
using Rate = WiseRates.Api.Model.Rate;

namespace Enterprise.Rating.Business.Testing
{
	public class ContainerChargeableCalculationStrategyTest : RatingTestCase
	{
		public void TestGetChargableAmountWithEmptyResult()
		{
			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.ValuesCanBeSet = true;
			// Is a container count with no containers really needed for this test?
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			measures.SetQuantity(MeasureType.ContainerCount, 0, string.Empty);
			ratingCriteria.RateableMeasures = measures;
			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_RateCalculator = UnitCalculator.Code;
			rateLine.TL_WeightVolume = QuantityUnit.CN;
			var amountCalculator = new ContainerChargeableCalculationStrategy(rateLine.Calculator);

			AssertEquals(0m, amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);
		}

		public void TestGetChargableAmountContainersCount()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "FRT";

			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = chargeCode };

			var ratingCriteria = new RatingCriteria(null, Factory);
			var containerInfo = new MeasureInfo.ContainerInfo(22, Constants.Weight.Kilograms, 33, Constants.Volume.CubicMetres, 44, 0, ZString.Empty);
			ratingCriteria.ValuesCanBeSet = true;
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			ratingCriteria.RateableMeasures = measures;
			measures.AddContainerGroup(ZGuid.Empty, new[] { containerInfo });
			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(autoRateInfo);

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = chargeCode.PK;
			rateLine.TL_RateCalculator = UnitCalculator.Code;
			rateLine.TL_WeightVolume = QuantityUnit.CN;
			var amountCalculator = new ContainerChargeableCalculationStrategy(rateLine.Calculator);

			AssertEquals(1m, amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);
		}

		public void TestGetChargableAmountContainersCountInTEU()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "FRT";

			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = chargeCode };

			var ratingCriteria = new RatingCriteria(null, Factory);
			var containerInfo = new MeasureInfo.ContainerInfo(22, Constants.Weight.Kilograms, 33, Constants.Volume.CubicMetres, 44, 6, ZString.Empty);
			ratingCriteria.ValuesCanBeSet = true;
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			ratingCriteria.RateableMeasures = measures;
			measures.AddContainerGroup(ZGuid.Empty, new[] { containerInfo });
			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(autoRateInfo);

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = chargeCode.PK;
			rateLine.TL_RateCalculator = UnitCalculator.Code;
			rateLine.TL_WeightVolume = QuantityUnit.TU;
			var amountCalculator = new ContainerChargeableCalculationStrategy(rateLine.Calculator);

			AssertEquals(6m, amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);
		}

		public void TestGetChargeableAmountByChargeableType()
		{
			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "FRT";

			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = chargeCode };

			var spotRateMoney = new Money(35m, usd);
			var costSpotRateInfo = new SpotRateInfo(spotRateMoney, Constants.FreightRateAutoratingModes.Code.FreightPlusRate, AutoratedValueType.NegotiatedCost);
			var sellSpotRateInfo = new SpotRateInfo(spotRateMoney, Constants.FreightRateAutoratingModes.Code.FreightPlusRate, AutoratedValueType.SpotRate);
			var invalidSpotRateInfo = new SpotRateInfo(Money.Invalid, Constants.FreightRateAutoratingModes.Code.StandardRate, AutoratedValueType.Cost);
			var allInSpotRateInfo = new SpotRateInfo(spotRateMoney, Constants.FreightRateAutoratingModes.Code.AllInRate, AutoratedValueType.SpotRate);

			var refContainerPK = ZGuid.NewZGuid();
			var container1PK = ZGuid.NewZGuid();
			var container2PK = ZGuid.NewZGuid();
			var container3PK = ZGuid.NewZGuid();
			var container4PK = ZGuid.NewZGuid();
			var container5PK = ZGuid.NewZGuid();

			var measureInfoNoSpotRates = new MeasureInfo.ContainerInfo(10000m, Constants.Weight.Kilograms, 8m, Constants.Volume.CubicMetres, 50, 1, ZString.Empty);
			var measureInfoCostSpotRate = new MeasureInfo.ContainerInfo(15000m, Constants.Weight.Kilograms, 12m, Constants.Volume.CubicMetres, 70, 1, ZString.Empty);
			measureInfoCostSpotRate.SetSpotRates(costSpotRateInfo, null, container1PK, refContainerPK);

			var measureInfoSellSpotRate = new MeasureInfo.ContainerInfo(20000m, Constants.Weight.Kilograms, 17m, Constants.Volume.CubicMetres, 100, 1, ZString.Empty);
			measureInfoSellSpotRate.SetSpotRates(null, sellSpotRateInfo, container2PK, refContainerPK);

			var measureInfoBothSpotRates = new MeasureInfo.ContainerInfo(25000m, Constants.Weight.Kilograms, 25m, Constants.Volume.CubicMetres, 160, 1, ZString.Empty);
			measureInfoBothSpotRates.SetSpotRates(costSpotRateInfo, sellSpotRateInfo, container3PK, refContainerPK);

			var measureInfoInvalidSpotRates = new MeasureInfo.ContainerInfo(25000m, Constants.Weight.Kilograms, 25m, Constants.Volume.CubicMetres, 160, 2, ZString.Empty);
			measureInfoInvalidSpotRates.SetSpotRates(invalidSpotRateInfo, invalidSpotRateInfo, container4PK, refContainerPK);

			var measureInfoAllInSpotRates = new MeasureInfo.ContainerInfo(25000m, Constants.Weight.Kilograms, 25m, Constants.Volume.CubicMetres, 160, 2, ZString.Empty);
			measureInfoAllInSpotRates.SetSpotRates(allInSpotRateInfo, allInSpotRateInfo, container5PK, refContainerPK);

			var containerInfos = new[]
			{
				measureInfoNoSpotRates,
				measureInfoCostSpotRate,
				measureInfoSellSpotRate,
				measureInfoBothSpotRates,
				measureInfoInvalidSpotRates,
				measureInfoAllInSpotRates
			};

			var ratingCriteria = new RatingCriteria(null, Factory)
			{
				ValuesCanBeSet = true,
			};
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			ratingCriteria.RateableMeasures = measures;
			measures.AddContainerGroup(ZGuid.Empty, containerInfos);

			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			calculatorParameters.Results.Add(autoRateInfo);

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = chargeCode.PK;
			rateLine.TL_RateCalculator = UnitCalculator.Code;
			rateLine.TL_WeightVolume = QuantityUnit.CN;

			var amountCalculator = new ContainerChargeableCalculationStrategy(rateLine.Calculator);

			AssertEquals(6m, amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);

			rateEntry.ContainerPKForSpotEntry = container1PK;
			AssertEquals(0m, amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);

			rateEntry.ContainerPKForSpotEntry = container2PK;
			AssertEquals(1m, amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);

			rateLine.TL_WeightVolume = QuantityUnit.TU;
			rateEntry.ContainerPKForSpotEntry = ZGuid.Empty;

			AssertEquals(8m, amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);

			var costing = Factory.New<Costing>();
			var costEntry = costing.AddRateEntry("AIR");
			var costLine = costEntry.RateLines.AddNew();
			costLine.TL_AC = chargeCode.PK;
			costLine.TL_RateCalculator = UnitCalculator.Code;
			costLine.TL_WeightVolume = QuantityUnit.CN;

			amountCalculator = new ContainerChargeableCalculationStrategy(costLine.Calculator);

			AssertEquals(6m, amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);

			costEntry.ContainerPKForSpotEntry = container1PK;
			AssertEquals(1m, amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);

			costEntry.ContainerPKForSpotEntry = container2PK;
			AssertEquals(0m, amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);

			costLine.TL_WeightVolume = QuantityUnit.TU;
			costEntry.ContainerPKForSpotEntry = ZGuid.Empty;

			AssertEquals(8m, amountCalculator.GetChargeableAmount(calculatorParameters, string.Empty).Amount);
		}

		public void TestGetChargeableAmount_IsMultipleEquipmentsOverMaxWeightVolumeMode_ShouldUseOccupiedContainerCount()
		{
			var gp20 = Helper.Containers["20GP"];
			gp20.RC_GrossWeight = 2000;
			gp20.RC_TareWeight = 1000;
			gp20.RC_CubicCapacity = 0; // Volume Capacity is not provided, but we expect to calculate required number of containers based on job's weight

			Factory.Save();

			var criteria = new TestRatingCriteria();
			criteria
				.RateableMeasures
				.AddContainerGroup(gp20.PK, "GEN", new[]
				{
					new MeasureInfo.ContainerInfo(weight: 3000, refNumber: "CONT00001", container: gp20, containerCount: 2)
				});
			criteria
				.RateableMeasures
				.AddContainerGroup(gp20.PK, "HAZ", new[]
				{
					new MeasureInfo.ContainerInfo(weight: 1000, refNumber: "CONT00002", container: gp20, containerCount: 1)
				});

			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_WeightVolume = QuantityUnit.CN;
			Line.Calculator.MultipleEquipmentsOverMaxWeightVolume = true;

			var context = new RatingContext();
			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(context));
			var strategy = new ContainerChargeableCalculationStrategy(Line.Calculator);

			var amount = strategy.GetChargeableAmount(calculatorParameters, QuantityUnit.CN);
			AssertEquals(
				"3 x CONT00001 (3000 KG / 1000KG capacity) + 1 x CONT00002 (500 KG / 1000 KG capacity)",
				4m,
				amount.Amount
			);
		}

		public void TestGetChargableAmount_WiseEntry_ContainerWithQuality()
		{
			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = Helper.ChargeCodes["FRT"] };

			var criteria = new TestRatingCriteria();
			criteria.RateableMeasures
				.AddContainerGroup(ZGuid.Empty, [
					new MeasureInfo.ContainerInfo(22, Constants.Weight.Kilograms, 33, Constants.Volume.CubicMetres, 44, 0, ZString.Empty)
				]);
			criteria.JobID = ZString.Empty;

			var rateLine = new WiseLine(Factory, new Charge());
			rateLine.TL_AC = Helper.ChargeCodes["FRT"].PK;
			rateLine.TL_RateCalculator = UnitCalculator.Code;
			rateLine.TL_WeightVolume = QuantityUnit.CN;
			rateLine.ChildRateLineItems = [
				new WiseLineItem(rateLine, Calculator.Items.Operator.UNT, string.Empty, 5m, ZString.Empty, 0m, 0m, null)
			];

			var entry = new WiseEntry(new Rate { Origin = "AUSYD", Destination = "UAIEV", ProviderRateId = "McLaren" }, Factory);
			entry.ChildRateLines = [rateLine];
			entry.CustomFields = [new CustomField() { Code = Rate.CustomFields.CargoSphere.ContainerQuality, Value = "NOR" }];

			var context = new RatingContext();
			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(context));
			var strategy = new ContainerChargeableCalculationStrategy(rateLine.Calculator);

			AssertEquals("Job Container should be filtered because it doesn't exactly match Empty", 0m, strategy.GetChargeableAmount(calculatorParameters, string.Empty).Amount);

			criteria.IsLooseRateSearchForCarrierConnect = true;
			AssertEquals("Job Container should not be filtered", 1m, strategy.GetChargeableAmount(calculatorParameters, string.Empty).Amount);
		}

		public void TestGetChargableAmount_WiseEntryWithContainerQuality_WithJob()
		{
			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = Helper.ChargeCodes["FRT"] };

			var criteria = new TestRatingCriteria();
			criteria.RateableMeasures
				.AddContainerGroup(ZGuid.Empty, [
					new MeasureInfo.ContainerInfo(22, Constants.Weight.Kilograms, 33, Constants.Volume.CubicMetres, 44, 0, ZString.Empty, containerQuality: "NOR")
				]);
			criteria.JobID = "C123";

			var rateLine = new WiseLine(Factory, new Charge());
			rateLine.TL_AC = Helper.ChargeCodes["FRT"].PK;
			rateLine.TL_RateCalculator = UnitCalculator.Code;
			rateLine.TL_WeightVolume = QuantityUnit.CN;
			rateLine.ChildRateLineItems = [
				new WiseLineItem(rateLine, Calculator.Items.Operator.UNT, string.Empty, 5m, ZString.Empty, 0m, 0m, null)
			];

			var entry = new WiseEntry(new Rate { Origin = "AUSYD", Destination = "UAIEV", ProviderRateId = "McLaren" }, Factory);
			entry.ChildRateLines = [rateLine];
			entry.CustomFields = [new CustomField() { Code = Rate.CustomFields.CargoSphere.ContainerQuality, Value = "REF" }];

			var context = new RatingContext();
			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(context));
			var strategy = new ContainerChargeableCalculationStrategy(rateLine.Calculator);

			criteria.IsLooseRateSearchForCarrierConnect = true;

			AssertEquals("Qualities don't match, container should be filtered out", 0m, strategy.GetChargeableAmount(calculatorParameters, string.Empty).Amount);

			entry.CustomFields = [];
			AssertEquals(
				"When autorating for a job in C3, blank container quality shouldn't count as 'any'",
				0m, strategy.GetChargeableAmount(calculatorParameters, string.Empty).Amount
			);

			entry.CustomFields = [new CustomField() { Code = Rate.CustomFields.CargoSphere.ContainerQuality, Value = "NOR" }];
			AssertEquals(1m, strategy.GetChargeableAmount(calculatorParameters, string.Empty).Amount);
		}

		public void TestGetChargableAmount_WiseEntryWithContainerQuality_NoJob()
		{
			var autoRateInfo = new AutoRateInfo(Factory) { ChargeCode = Helper.ChargeCodes["FRT"] };

			var criteria = new TestRatingCriteria();
			criteria.RateableMeasures
				.AddContainerGroup(ZGuid.Empty, [
					new MeasureInfo.ContainerInfo(22, Constants.Weight.Kilograms, 33, Constants.Volume.CubicMetres, 44, 0, ZString.Empty)
				]);
			criteria.JobID = ZString.Empty;

			var rateLine = new WiseLine(Factory, new Charge());
			rateLine.TL_AC = Helper.ChargeCodes["FRT"].PK;
			rateLine.TL_RateCalculator = UnitCalculator.Code;
			rateLine.TL_WeightVolume = QuantityUnit.CN;
			rateLine.ChildRateLineItems = [
				new WiseLineItem(rateLine, Calculator.Items.Operator.UNT, string.Empty, 5m, ZString.Empty, 0m, 0m, null)
			];

			var entry = new WiseEntry(new Rate { Origin = "AUSYD", Destination = "UAIEV", ProviderRateId = "McLaren" }, Factory);
			entry.ChildRateLines = [rateLine];
			entry.CustomFields = [new CustomField() { Code = Rate.CustomFields.CargoSphere.ContainerQuality, Value = "REF" }];

			var context = new RatingContext();
			var calculatorParameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(context));
			var strategy = new ContainerChargeableCalculationStrategy(rateLine.Calculator);

			criteria.IsLooseRateSearchForCarrierConnect = true;

			AssertEquals(
				"Container should still fall under the rate entry charges because BLANK container quality means any",
				1m, strategy.GetChargeableAmount(calculatorParameters, string.Empty).Amount
			);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry = clientRate.AddRateEntry("AIR");
			Line = rateEntry.RateLines.AddNew();
			Line.TL_AC = Helper.ChargeCodes["FRT"].PK;
			Line.TL_RateCalculator = UnitCalculator.Code;
			Line.TL_WeightVolume = QuantityUnit.CN;
		}

		RateLine Line { get; set; }
	}
}
