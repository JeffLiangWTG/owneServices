using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class CalculationLogsAnalyzerTest : TestCaseWithFactory
	{
		public void TestCtorDoesNotAcceptNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new CalculationLogsAnalyzer(null));
		}

		public void TestLogsWrapper()
		{
			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			CalculationLogsAnalyzer analyzer = new CalculationLogsAnalyzer(header);
			AssertNull(analyzer.LogsWrapper);
		}

		public void TestDisableLogs()
		{
			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			CalculationLogsLoader.Save(bizo, new CalculationLogsWrapper());

			MockAnalyzer analyzer = new MockAnalyzer(Factory.New<MockExportAWBHeader>());
			analyzer.BizoWithLogs = bizo;
			AssertEquals("Precondition: logs not null and enabled", false, analyzer.LogsWrapper.IsDisabled);

			analyzer.DisableLogs();
			AssertEquals("Logs not null but disabled", true, analyzer.LogsWrapper.IsDisabled);
		}

		public void TestDisableLogsNoExceptionThrownWhenHaveNullLogsWrapper()
		{
			var bizoWithLogs = Factory.New<DummyEnterpriseBusinessObject>();
			var bizoWithoutLogs = Factory.New<DummyEnterpriseBusinessObject>();
			CalculationLogsLoader.Save(bizoWithLogs, new CalculationLogsWrapper());

			var analyzer = new MockAnalyzer(Factory.New<MockExportAWBHeader>());
			analyzer.BizosWithLogs = new List<BusinessObject> { bizoWithLogs, bizoWithoutLogs };
			Assert("Precondition: the logs of bizoWithLogs is not null and enabled", !analyzer.LogsWrapper.IsDisabled);
			AssertNull("The logs of bizoWithoutLogs is null", CalculationLogsLoader.Load(bizoWithoutLogs));

			AssertNoExceptionThrown(() => analyzer.DisableLogs());
			Assert("The logs of bizoWithLogs is not null but disabled", analyzer.LogsWrapper.IsDisabled);
			AssertNull("The logs of bizoWithoutLogs is still null", CalculationLogsLoader.Load(bizoWithoutLogs));
		}

		public void TestPopulateRateLines()
		{
			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			MockAnalyzer analyzer = new MockAnalyzer(header);

			AssertEquals("No calculation logs", 0, analyzer.PopulateRateLines());
			AssertEquals("Currrency amount conversion not called", null, analyzer.ConvertMonetaryAmountsParameters);

			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();
			AssertNotNull(analyzer.LogsWrapper);

			analyzer.LogsWrapper.Logs[0].Minimum = 10m;
			analyzer.LogsWrapper.Logs[0].Unit = Constants.Weight.Kilograms;
			analyzer.LogsWrapper.Logs[0].Currency = "NZD";

			AssertEquals("Precondition", false, analyzer.LogsWrapper.IsDisabled);
			AssertEquals("Minimum line populated", 1, analyzer.PopulateRateLines());
			AssertEquals("Number of lines passed for currency amount conversion", 1, analyzer.ConvertMonetaryAmountsParameters[0]);
			AssertEquals("Currency code passed for currency amount conversion", "NZD", analyzer.ConvertMonetaryAmountsParameters[1]);

			analyzer.LogsWrapper.IsDisabled = true;
			AssertEquals("No fun for disabled logs", 0, analyzer.PopulateRateLines());
		}

		public void TestPopulateRateLines_GrossWeight()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_SeaWeight = collection.AddNew();
			defaultNumberOfDecimals_SeaWeight.UnitOfMeasure = Constants.Weight.Kilograms;
			defaultNumberOfDecimals_SeaWeight.TransportMode = Constants.TransportModes.Air;
			defaultNumberOfDecimals_SeaWeight.NumberOfDecimals = 3;
			defaultNumberOfDecimals_SeaWeight.RoundingMode = RoundingModes.Down;

			using (FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var header = Factory.New<MockExportAWBHeader>();
				var analyzer = new MockAnalyzer(header);
				analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();
				analyzer.NumberOfPieces_Exposed = 10;

				var log1 = analyzer.LogsWrapper.Logs[0];
				log1.Weight = 41.77m;
				log1.Unit = Constants.Weight.Kilograms;
				log1.Chargeable = 40m;
				log1.ChargeableUnit = Constants.Weight.Kilograms;
				log1.AddPerUnitCalculation(40m, Constants.Weight.Kilograms, 160m);

				AssertEquals("Rate1 charge line populated", 1, analyzer.PopulateRateLines());
				AssertRateLine(header, 1, "10", 41.77m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.NormalCharge, 40m, 160m, 6400m);
			}
		}

		public void TestMaximumRatelineCountExceeded()
		{
			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			MockAnalyzer analyzer = new MockAnalyzer(header);

			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();

			CalculationLog log = analyzer.LogsWrapper.Logs[0];
			log.Minimum = 10m;
			log.Unit = Constants.Weight.Kilograms;
			log.Currency = "NZD";

			for (int i = 0; i < ExportAWBHeader.Constants.NumberOfRateLines - 1; i++)
			{
				analyzer.LogsWrapper.Logs.Add(log);
			}

			AssertEquals("Precondition", ExportAWBHeader.Constants.NumberOfRateLines, analyzer.LogsWrapper.Logs.Count);
			AssertEquals("All lines populated", ExportAWBHeader.Constants.NumberOfRateLines, analyzer.PopulateRateLines());

			analyzer.LogsWrapper.Logs.Add(log);
			AssertEquals("Precondition", true, analyzer.LogsWrapper.Logs.Count > ExportAWBHeader.Constants.NumberOfRateLines);
			AssertEquals("Maximum rateline count exceeded - no lines populated", 0, analyzer.PopulateRateLines());
		}

		#region Populate NonULD

		public void TestPopulateRateLines_NonULD_Minimum()
		{
			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			MockAnalyzer analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();

			CalculationLog log = analyzer.LogsWrapper.Logs[0];
			log.Minimum = 100m;
			log.Unit = Constants.Weight.Kilograms;
			log.Weight = 200m;
			log.Chargeable = 300m;
			log.ChargeableUnit = Constants.Weight.Kilograms;
			log.AddPerUnitCalculation(10m, Constants.Weight.Kilograms, 2m);

			AssertEquals("Preconditon", true, log.HasMinimum);
			AssertEquals("Preconditon", true, log.Result < log.Minimum);
			AssertEquals("Minimum line populated", 1, analyzer.PopulateRateLines());
			AssertRateLine(header, 1, "1", 200m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.MinimumCharge, 300m, 100m, 100m);
		}

		public void TestPopulateRateLines_NonULD_Basic()
		{
			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			MockAnalyzer analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();

			CalculationLog log = analyzer.LogsWrapper.Logs[0];
			log.Unit = Constants.Weight.Kilograms;
			log.Weight = 200m;
			log.Chargeable = 300m;
			log.ChargeableUnit = Constants.Weight.Kilograms;
			log.BaseRate = 32m;

			AssertEquals("Preconditon", true, log.HasBaseRate);
			AssertEquals("Preconditon", 0, log.Steps.Count);
			AssertEquals("BasicCharge line populated", 1, analyzer.PopulateRateLines());
			AssertRateLine(header, 1, "1", 200m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.BasicCharge, 300m, 32m, 32m);
		}

		public void TestPopulateRateLines_NonULD_SpecificCommodityCode()
		{
			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AAAA";
			commodity.RH_IATACommodityItem = "1111";

			Factory.Save();

			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			MockAnalyzer analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();

			CalculationLog log = analyzer.LogsWrapper.Logs[0];
			log.Unit = Constants.Weight.Kilograms;
			log.CommodityCode = "AAAA";
			log.Weight = 200m;
			log.Chargeable = 300m;
			log.ChargeableUnit = Constants.Weight.Kilograms;
			log.BaseRate = 32m;
			log.AddPerUnitCalculation(100m, Constants.Weight.Kilograms, 1.28m);

			AssertEquals("Preconditon", true, log.HasBaseRate);
			AssertEquals("Preconditon", 1, log.Steps.Count);
			AssertEquals("BasicCharge+RatePerKilogram lines populated", 2, analyzer.PopulateRateLines());
			AssertRateLine(header, 1, "1", 200m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.BasicCharge, 300m, 32m, 32m);
			AssertRateLine(header, 2, "", 0m, "", Constants.AWB.RateClass.SpecificCommodityRate, 100m, 1.28m, 128m);
		}

		public void TestPopulateRateLines_NonULD_Basic_PerKilogram()
		{
			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			MockAnalyzer analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();

			CalculationLog log = analyzer.LogsWrapper.Logs[0];
			log.Unit = Constants.Weight.Kilograms;
			log.Weight = 200m;
			log.Chargeable = 300m;
			log.ChargeableUnit = Constants.Weight.Kilograms;
			log.BaseRate = 32m;
			log.AddPerUnitCalculation(100m, Constants.Weight.Kilograms, 1.28m);

			AssertEquals("Preconditon", true, log.HasBaseRate);
			AssertEquals("Preconditon", 1, log.Steps.Count);
			AssertEquals("BasicCharge+RatePerKilogram lines populated", 2, analyzer.PopulateRateLines());
			AssertRateLine(header, 1, "1", 200m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.BasicCharge, 300m, 32m, 32m);
			AssertRateLine(header, 2, "", 0m, "", Constants.AWB.RateClass.RatePerKilogram, 100m, 1.28m, 128m);
		}

		public void TestPopulateRateLines_NonULD_NormalCharge()
		{
			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			MockAnalyzer analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();

			CalculationLog log = analyzer.LogsWrapper.Logs[0];
			log.Unit = Constants.Weight.Kilograms;
			log.Weight = 200m;
			log.AddPerUnitCalculation(40m, Constants.Weight.Kilograms, 1.28m);

			AssertEquals("Preconditon", 1, log.Steps.Count);
			AssertEquals("NormalCharge lines populated", 1, analyzer.PopulateRateLines());
			AssertRateLine(header, 1, "1", 200m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.NormalCharge, 40m, 1.28m, 51.2m);
		}

		public void TestPopulateRateLines_NonULD_Quantity()
		{
			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			MockAnalyzer analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();

			CalculationLog log = analyzer.LogsWrapper.Logs[0];
			log.Unit = Constants.Weight.Kilograms;
			log.Weight = 200m;
			log.AddPerUnitCalculation(100m, Constants.Weight.Kilograms, 1.28m);

			AssertEquals("Preconditon", 1, log.Steps.Count);
			AssertEquals("QuantityRate lines populated", 1, analyzer.PopulateRateLines());
			AssertRateLine(header, 1, "1", 200m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.QuantityRate, 100m, 1.28m, 128m);
		}

		public void TestPopulateRateLines_NonULD_RateClassDependsOnWeightInKilograms()
		{
			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			MockAnalyzer analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();

			CalculationLog log = analyzer.LogsWrapper.Logs[0];
			log.Unit = Constants.Weight.Pounds;
			log.Weight = 200m;
			log.AddPerUnitCalculation(80m, Constants.Weight.Pounds, 1.5m);

			AssertEquals("Preconditon", 1, log.Steps.Count);
			AssertEquals("NormalCharge lines populated: 80 pounds < 45 kilograms", 1, analyzer.PopulateRateLines());
			AssertRateLine(header, 1, "1", 200m, Constants.AWB.RateLineUQ.Pounds, Constants.AWB.RateClass.NormalCharge, 80m, 1.5m, 120m);

			log.Steps.Clear();
			log.AddPerUnitCalculation(120m, Constants.Weight.Kilograms, 1.5m);

			AssertEquals("Preconditon", 1, log.Steps.Count);
			AssertEquals("QuantityRate lines populated: 120 pounds > 45 kilograms", 1, analyzer.PopulateRateLines());
			AssertRateLine(header, 1, "1", 200m, Constants.AWB.RateLineUQ.Pounds, Constants.AWB.RateClass.QuantityRate, 120m, 1.5m, 180m);
		}

		public void TestPopulateRateLines_NonULD_NumberOfPiecesAndWeightOnlyOnFirstLine_PackSplitWithUniqueRates()
		{
			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			MockAnalyzer analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();
			analyzer.NumberOfPieces_Exposed = 10;

			CalculationLog log1 = analyzer.LogsWrapper.Logs[0];
			log1.Weight = 40m;
			log1.Unit = Constants.Weight.Kilograms;
			log1.Chargeable = 40m;
			log1.ChargeableUnit = Constants.Weight.Kilograms;
			log1.AddPerUnitCalculation(40m, Constants.Weight.Kilograms, 160m);

			CalculationLog log2 = new CalculationLog();
			analyzer.LogsWrapper.Logs.Add(log2);
			log2.Weight = 40m;
			log2.Unit = Constants.Weight.Kilograms;
			log2.Chargeable = 60m;
			log2.ChargeableUnit = Constants.Weight.Kilograms;
			log2.AddPerUnitCalculation(60m, Constants.Weight.Kilograms, 150m);

			AssertEquals("Rate1 charge and Rate2 charge lines populated", 2, analyzer.PopulateRateLines());

			AssertRateLine(header, 1, "10", 40m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.NormalCharge, 40m, 160m, 6400m);
			AssertRateLine(header, 2, "", 0m, "", Constants.AWB.RateClass.QuantityRate, 60m, 150m, 9000m);
		}

		public void TestPopulateRateLines_NonULD_NumberOfPiecesAndWeightOnlyOnFirstLine_PackSplitWithBaseRateAndUniqueRate()
		{
			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			MockAnalyzer analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();
			analyzer.NumberOfPieces_Exposed = 10;

			CalculationLog log = analyzer.LogsWrapper.Logs[0];
			log.Unit = Constants.Weight.Kilograms;
			log.Weight = 40m;
			log.Chargeable = 40m;
			log.ChargeableUnit = Constants.Weight.Kilograms;
			log.BaseRate = 32m;
			log.AddPerUnitCalculation(40m, Constants.Weight.Kilograms, 160m);

			CalculationLog log2 = new CalculationLog();
			analyzer.LogsWrapper.Logs.Add(log2);
			log2.Weight = 40m;
			log2.Unit = Constants.Weight.Kilograms;
			log2.Chargeable = 60m;
			log2.ChargeableUnit = Constants.Weight.Kilograms;
			log2.AddPerUnitCalculation(60m, Constants.Weight.Kilograms, 150m);

			AssertEquals("BaseRate charge + Rate1PerKilogram and Rate2 charge lines populated", 3, analyzer.PopulateRateLines());
			AssertRateLine(header, 1, "10", 40m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.BasicCharge, 40m, 32m, 32m);
			AssertRateLine(header, 2, "", 0m, "", Constants.AWB.RateClass.RatePerKilogram, 40m, 160m, 6400m);
			AssertRateLine(header, 3, "", 0m, "", Constants.AWB.RateClass.QuantityRate, 60m, 150m, 9000m);
		}

		#endregion

		#region Populate ULD

		public void TestPopulateRateLines_ULD_PivotBased_UnderPivot_HasPerKG_WithoutBaseCharge()
		{
			// Given 1 container with a weight of 600kg
			// When the breaks are:
			//  baserate $0
			//  <= 1000kg, $5/kg
			// Then expect a charge of $3000
			var refContainer1PK = CreateReferenceContainer("LD1", 64, "1");

			var consol = Factory.New<ForwardingConsol>();

			CreateULDContainer(consol, refContainer1PK, 600, "CONT001", "2");

			var header = Factory.New<MockExportAWBHeader>();
			var analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();
			header.ULDContainers_Exposed = consol.Containers.Cast<CommonContainer>();

			var log1 = analyzer.LogsWrapper.Logs[0];
			log1.Unit = Constants.Weight.Kilograms;
			log1.Weight = 600m;
			log1.Chargeable = 600m;
			log1.ChargeableUnit = Constants.Weight.Kilograms;
			log1.RateMode = Constants.ContainerModes.ULD;
			log1.ContainerCode = "LD1";
			log1.BaseRate = 0m;
			log1.AddPerUnitCalculation(600m, Constants.Weight.Kilograms, 5);
			log1.AddFlatAmountToLastCalculation(0);

			analyzer.PopulateRateLines();

			AssertRateLine(header, 1, "1", 600m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 600m, 5m, 3000m);
			AssertContainerInfoLine(header, 2, 64m, "1", "CONT001");
		}

		public void TestPopulateRateLines_ULD_PivotBased_UnderPivot_HasPerKG_WithBaseCharge()
		{
			// Given 1 container with a weight of 600kg
			// When the breaks are:
			//  baserate $1000
			//  <= 1000kg, $5/kg
			// Then expect a charge of $4000
			var refContainer1PK = CreateReferenceContainer("LD1", 64, "1");

			var consol = Factory.New<ForwardingConsol>();

			CreateULDContainer(consol, refContainer1PK, 600, "CONT001", "2");

			var header = Factory.New<MockExportAWBHeader>();
			var analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();
			header.ULDContainers_Exposed = consol.Containers.Cast<CommonContainer>();

			var log1 = analyzer.LogsWrapper.Logs[0];
			log1.Unit = Constants.Weight.Kilograms;
			log1.Weight = 600m;
			log1.Chargeable = 600m;
			log1.ChargeableUnit = Constants.Weight.Kilograms;
			log1.RateMode = Constants.ContainerModes.ULD;
			log1.ContainerCode = "LD1";
			log1.BaseRate = 1000m;
			log1.AddPerUnitCalculation(600m, Constants.Weight.Kilograms, 5);
			log1.AddFlatAmountToLastCalculation(0);

			analyzer.PopulateRateLines();

			AssertRateLine(header, 1, "1", 600m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 600m, 1000m, 1000m);
			AssertRateLine(header, 2, "", 0m, "", Constants.AWB.RateClass.UnitLoadDeviceAdditionalCharge, 600m, 5m, 3000m);
			AssertContainerInfoLine(header, 3, 64m, "1", "CONT001");
		}

		public void TestPopulateRateLines_ULD_PivotBased_UnderPivot_HasFlatRateAndPerKG()
		{
			// Given 1 container with a weight of 600kg
			// When the breaks are:
			//  baserate $0
			//  <= 1000kg, $100, $5/kg
			// Then expect a charge of $3000
			//
			// note: this is a silly-input caes. Ideally, you either put a
			// per-kg rate or a flag rate, but not both. This test documents
			// the explicit decision of what this silly case does.
			var refContainer1PK = CreateReferenceContainer("LD1", 64, "1");

			var consol = Factory.New<ForwardingConsol>();

			CreateULDContainer(consol, refContainer1PK, 600, "CONT001", "2");

			var header = Factory.New<MockExportAWBHeader>();
			var analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();
			header.ULDContainers_Exposed = consol.Containers.Cast<CommonContainer>();

			var log1 = analyzer.LogsWrapper.Logs[0];
			log1.Unit = Constants.Weight.Kilograms;
			log1.Weight = 600m;
			log1.Chargeable = 600m;
			log1.ChargeableUnit = Constants.Weight.Kilograms;
			log1.RateMode = Constants.ContainerModes.ULD;
			log1.ContainerCode = "LD1";
			log1.BaseRate = 0m;
			log1.AddPerUnitCalculation(600m, Constants.Weight.Kilograms, 5m);
			log1.AddFlatAmountToLastCalculation(100);

			analyzer.PopulateRateLines();

			AssertRateLine(header, 1, "1", 600m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 600m, 5m, 3000m);
			AssertContainerInfoLine(header, 2, 64m, "1", "CONT001");
		}

		public void TestPopulateRateLines_ULD_PivotBased_UnderPivot_HasFlatRate()
		{
			// Given 1 container with a weight of 600kg
			// When the breaks are:
			//  baserate $1000
			//  <= 1000kg, $100
			//  >  1000kg, $100 + $2/kg
			// Then expect a charge of $1100
			var refContainer1PK = CreateReferenceContainer("LD1", 64, "1");

			var consol = Factory.New<ForwardingConsol>();

			CreateULDContainer(consol, refContainer1PK, 600, "CONT001", "2");

			var header = Factory.New<MockExportAWBHeader>();
			var analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();
			header.ULDContainers_Exposed = consol.Containers.Cast<CommonContainer>();

			var log1 = analyzer.LogsWrapper.Logs[0];
			log1.Unit = Constants.Weight.Kilograms;
			log1.Weight = 600m;
			log1.Chargeable = 600m;
			log1.ChargeableUnit = Constants.Weight.Kilograms;
			log1.RateMode = Constants.ContainerModes.ULD;
			log1.ContainerCode = "LD1";
			log1.BaseRate = 1000m;
			log1.AddPerUnitCalculation(600m, Constants.Weight.Kilograms, 0m);
			log1.AddFlatAmountToLastCalculation(100);

			analyzer.PopulateRateLines();

			AssertRateLine(header, 1, "1", 600m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 600m, 1100m, 1100m);
			AssertContainerInfoLine(header, 2, 64m, "1", "CONT001");
		}

		public void TestPopulateRateLines_ULD_PivotBased_PivotExceeded()
		{
			// Basically, it is for CMB calculator setup for over pivot rate:
			//	Base Rate: $1000
			//	<= 100 KG: Flat Rate $500
			//   > 100 KG: Flat Rate $500, $2 per KG
			//	IsAccumulated is ticked
			//
			// Which means that the client pays $500 per container plus $2 per each kilogram over $100.
			// Well, I've added $1000 base rate as well. Even though it is unlikely in real world as the user will use
			// flat rate in the break, the system allows adding base rate and we need to test this scenario.
			//
			// So, this will result to the following calculation if container weight is 150KG:
			// Base Rate $1000 + Base Rate $500 + 50 KG * $2 per KG
			//
			// So, on the AWB we expect:
			//	Basic Charge to be $1500
			//	Additional Charge for 50 KG * $2
			var refContainer1PK = CreateReferenceContainer("LD1", 64, "1");
			var refContainer2PK = CreateReferenceContainer("LD2", 256, "4");

			var consol = Factory.New<ForwardingConsol>();

			CreateULDContainer(consol, refContainer1PK, 80, "CONT001", "2");
			CreateULDContainer(consol, refContainer1PK, 70, "CONT002", "3");
			CreateULDContainer(consol, refContainer2PK, 400, "CONT004", "5");

			var header = Factory.New<MockExportAWBHeader>();
			var analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();
			header.ULDContainers_Exposed = consol.Containers.Cast<CommonContainer>();

			var log1 = analyzer.LogsWrapper.Logs[0];
			log1.Unit = Constants.Weight.Kilograms;
			log1.Weight = 150m;
			log1.Chargeable = 150m;
			log1.ChargeableUnit = Constants.Weight.Kilograms;
			log1.RateMode = Constants.ContainerModes.ULD;
			log1.ContainerCode = "LD1";
			log1.BaseRate = 1000m;
			log1.AddPerUnitCalculation(100m, Constants.Weight.Kilograms, 0m);
			log1.AddPerUnitCalculation(50m, Constants.Weight.Kilograms, 2m);
			log1.AddFlatAmountToLastCalculation(500);

			var log2 = new CalculationLog();
			analyzer.LogsWrapper.Logs.Add(log2);
			log2.Unit = Constants.Weight.Kilograms;
			log2.Weight = 400m;
			log2.Chargeable = 400m;
			log2.ChargeableUnit = Constants.Weight.Kilograms;
			log2.RateMode = Constants.ContainerModes.ULD;
			log2.ContainerCode = "LD2";
			log2.AddPerUnitCalculation(300m, Constants.Weight.Kilograms, 0m);
			log2.AddPerUnitCalculation(100m, Constants.Weight.Kilograms, 3m);
			log2.AddFlatAmountToLastCalculation(1000);

			analyzer.PopulateRateLines();

			AssertRateLine(header, 1, "2", 150m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 100m, 1500m, 1500m);
			AssertRateLine(header, 2, "", 0m, "", Constants.AWB.RateClass.UnitLoadDeviceAdditionalCharge, 50m, 2m, 100m);

			AssertContainerInfoLine(header, 3, 128m, "1", "CONT001");
			AssertContainerInfoLine(header, 4, 0m, ZString.Empty, "CONT002");

			AssertRateLine(header, 5, "1", 400m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 300m, 1000m, 1000m);
			AssertRateLine(header, 6, "", 0m, "", Constants.AWB.RateClass.UnitLoadDeviceAdditionalCharge, 100m, 3m, 300m);

			AssertContainerInfoLine(header, 7, 256m, "4", "CONT004");
		}

		/// <summary>
		///		This is for ULD setup when the client is charged per container and then per additional kilogram over a pivot defined by a rate for the container.
		///		Also, a container has max payload weight, and if goods weight exceeds container max payload weight, additional container will be charged
		///		and the weight will be split between 2 containers.
		///
		///		For example, assume a job has AKE ULD container with max payload 1400 KG, the user packed 1600 KG, and the rate has pivot 700 KG, i.e.:
		///		$5000 per ULD
		///		Less than 700 KG: $0
		///		More than 700 KG: $2 per KG over 700 KG
		///
		///		Since 1600 KG exceeds 1400 KG max payload, 2 containers will be charged instead of 1. Which means, each of them will include 800 KG of goods.
		///		Since pivot per container is 700 KG, each container will charge 2$ per 100 KG over the pivot (800 KG goods - 700 KG pivot)
		///
		///		So, this setup will lead to the following calculations in calculation log:
		///		Calculation Log:
		///			Weight: 1600 KG
		///			ContainerCode: ULD
		///			Unit: CN (because it is ULD, but in this setup it should be ignored by AWB and use units on steps instead)
		///			Steps:
		///				Step1: UnitCount=1, Unit=CN, UnitPrice=$5000
		///				Step2: UnitCount=700, Unit=KG, UnitPrice=$0
		///				Step3: UnitCount=100, Unit=KG, UnitPrice=$2
		///				Step4: UnitCount=1, Unit=CN, UnitPrice=$5000
		///				Step5: UnitCount=700, Unit=KG, UnitPrice=$0
		///				Step6: UnitCount=100, Unit=KG, UnitPrice=$2
		///
		///		I.e. Step 4-6 repeat Step 1-3 as we have 2 containers and we have steps for each individual container.
		///		It will be merged here, and adjusted to the same weight unit if necessary (although it should have the same unit).
		/// </summary>
		public void TestPopulateRateLines_ULD_PerContainerPerWeight()
		{
			var refContainer1PK = CreateReferenceContainer("LD1", 64, "1");

			var consol = Factory.New<ForwardingConsol>();

			CreateULDContainer(consol, refContainer1PK, 80, "CONT001", "2");

			var header = Factory.New<MockExportAWBHeader>();
			var analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();
			header.ULDContainers_Exposed = consol.Containers.Cast<CommonContainer>();

			var log1 = analyzer.LogsWrapper.Logs[0];
			log1.Unit = Constants.Weight.Kilograms;
			log1.Weight = 1600m;
			log1.ChargeableUnit = "CN";
			log1.RateMode = Constants.ContainerModes.ULD;
			log1.ContainerCode = "LD1";
			log1.AddPerUnitCalculation(1m, "CN", 5000m);
			log1.AddPerUnitCalculation(700m, "KG", 0m);		// Pivot weight
			log1.AddPerUnitCalculation(100m, "KG", 2m);		// Over pivot weight and rate
			log1.AddPerUnitCalculation(1m, "CN", 5000m);
			log1.AddPerUnitCalculation(700m, "KG", 0m);		// Pivot weight
			log1.AddPerUnitCalculation(100m, "KG", 2m);		// Over pivot weight and rate

			analyzer.PopulateRateLines();

			// Per container calculation result
			// 1600 KG - total weight
			// 1400 KG (700 KG + 700 KG) - pivot weight, i.e. included in container price
			// $5000 - per container price
			// $10000 - total for containers ($5000 * 2 containers)
			AssertRateLine(header, 1, "2", 1600m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 1400m, 5000m, 10000m);

			// Over pivot calculation result (100 KG * 2 containers) * $2 per KG = 400 KG
			AssertRateLine(header, 2, "", 0m, "", Constants.AWB.RateClass.UnitLoadDeviceAdditionalCharge, 200m, 2m, 400m);

			AssertContainerInfoLine(header, 3, 64m, "1", "CONT001");
		}

		public void TestPopulateRateLines_ULD_ContainerBased()
		{
			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			MockAnalyzer analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();
			header.ULDContainers_Exposed = CreateULDContainers();

			CalculationLog log1 = analyzer.LogsWrapper.Logs[0];
			log1.Unit = Constants.Weight.Kilograms;
			log1.Weight = 200m;
			log1.Chargeable = 222m;
			log1.ChargeableUnit = Constants.Weight.Kilograms;
			log1.RateMode = Constants.ContainerModes.ULD;
			log1.ContainerCode = "LD1";
			log1.BaseRate = 32m;
			log1.AddPerUnitCalculation(100m, Constants.Weight.Kilograms, 1.1m);
			log1.AddPerUnitCalculation(200m, Constants.Weight.Kilograms, 2.2m);

			CalculationLog log2 = new CalculationLog();
			analyzer.LogsWrapper.Logs.Add(log2);
			log2.Unit = Constants.Weight.Kilograms;
			log2.Weight = 400m;
			log2.RateMode = Constants.ContainerModes.ULD;
			log2.ContainerCode = "LD2";
			log2.AddPerUnitCalculation(300m, Constants.Weight.Kilograms, 3m);

			AssertEquals("Preconditon", Constants.ContainerModes.ULD, log1.RateMode);
			AssertEquals("Preconditon", Constants.ContainerModes.ULD, log2.RateMode);
			AssertEquals("UnitLoadDevice: U, E, X lines populated", 7, analyzer.PopulateRateLines());

			AssertRateLine(header, 1, "2", 200m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 222m, 32m, 32m);
			AssertRateLine(header, 2, "", 0m, "", Constants.AWB.RateClass.UnitLoadDeviceAdditionalCharge, 100m, 1.1m, 110m);
			AssertRateLine(header, 3, "", 0m, "", Constants.AWB.RateClass.UnitLoadDeviceAdditionalCharge, 200m, 2.2m, 440m);

			AssertContainerInfoLine(header, 4, 128m, "1", "CONT001");
			AssertContainerInfoLine(header, 5, 0m, ZString.Empty, "CONT002");

			AssertRateLine(header, 6, "1", 400m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 300m, 3m, 900m);

			AssertContainerInfoLine(header, 7, 256m, "4", "CONT004");
		}

		public void TestPopulateRateLines_ULD_WeightBased()
		{
			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			MockAnalyzer analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();
			header.ULDContainers_Exposed = CreateULDContainers();

			CalculationLog log1 = analyzer.LogsWrapper.Logs[0];
			log1.Unit = Constants.Weight.Kilograms;
			log1.Weight = 200m;
			log1.RateMode = Constants.ContainerModes.ULD;
			log1.ContainerCode = "LD1";
			log1.AddPerUnitCalculation(100m, Constants.Weight.Kilograms, 1.1m);
			log1.AddPerUnitCalculation(200m, Constants.Weight.Kilograms, 2.2m);

			CalculationLog log2 = new CalculationLog();
			analyzer.LogsWrapper.Logs.Add(log2);
			log2.Unit = Constants.Weight.Kilograms;
			log2.Weight = 400m;
			log2.RateMode = Constants.ContainerModes.ULD;
			log2.ContainerCode = "LD2";
			log2.AddPerUnitCalculation(300m, Constants.Weight.Kilograms, 3m);

			AssertEquals("Preconditon", Constants.ContainerModes.ULD, log1.RateMode);
			AssertEquals("Preconditon", Constants.ContainerModes.ULD, log2.RateMode);
			AssertEquals("UnitLoadDevice: U, E, X lines populated", 6, analyzer.PopulateRateLines());

			AssertRateLine(header, 1, "2", 200m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 100m, 1.1m, 110m);
			AssertRateLine(header, 2, "", 0m, "", Constants.AWB.RateClass.UnitLoadDeviceAdditionalCharge, 200m, 2.2m, 440m);

			AssertContainerInfoLine(header, 3, 128m, "1", "CONT001");
			AssertContainerInfoLine(header, 4, 0m, ZString.Empty, "CONT002");

			AssertRateLine(header, 5, "1", 400m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 300m, 3m, 900m);

			AssertContainerInfoLine(header, 6, 256m, "4", "CONT004");
		}

		public void TestPopulateRateLines_ULD_WithSecurityStatus()
		{
			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			header.EnableSecurityStatusDisplay = true;

			MockAnalyzer analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();
			header.ULDContainers_Exposed = CreateULDContainers();

			CalculationLog log1 = analyzer.LogsWrapper.Logs[0];
			log1.Unit = Constants.Weight.Kilograms;
			log1.Weight = 200m;
			log1.Chargeable = 222m;
			log1.ChargeableUnit = Constants.Weight.Kilograms;
			log1.RateMode = Constants.ContainerModes.ULD;
			log1.ContainerCode = "LD1";
			log1.BaseRate = 32m;
			log1.AddPerUnitCalculation(100m, Constants.Weight.Kilograms, 1.1m);
			log1.AddPerUnitCalculation(200m, Constants.Weight.Kilograms, 2.2m);

			CalculationLog log2 = new CalculationLog();
			analyzer.LogsWrapper.Logs.Add(log2);
			log2.Unit = Constants.Weight.Kilograms;
			log2.Weight = 400m;
			log2.RateMode = Constants.ContainerModes.ULD;
			log2.ContainerCode = "LD2";
			log2.AddPerUnitCalculation(300m, Constants.Weight.Kilograms, 3m);

			AssertEquals("Preconditon", Constants.ContainerModes.ULD, log1.RateMode);
			AssertEquals("Preconditon", Constants.ContainerModes.ULD, log2.RateMode);
			AssertEquals("UnitLoadDevice: U, E, X, G lines populated", 10, analyzer.PopulateRateLines());

			AssertRateLine(header, 1, "2", 200m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 222m, 32m, 32m);
			AssertRateLine(header, 2, "", 0m, "", Constants.AWB.RateClass.UnitLoadDeviceAdditionalCharge, 100m, 1.1m, 110m);
			AssertRateLine(header, 3, "", 0m, "", Constants.AWB.RateClass.UnitLoadDeviceAdditionalCharge, 200m, 2.2m, 440m);

			AssertContainerInfoLine(header, 4, 128m, "1", "CONT001");
			AssertContainerSecurityStatusLine(header, 5, "SPX");

			AssertContainerInfoLine(header, 6, 0m, ZString.Empty, "CONT002");
			AssertContainerSecurityStatusLine(header, 7, "SPX");

			AssertRateLine(header, 8, "1", 400m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 300m, 3m, 900m);

			AssertContainerInfoLine(header, 9, 256m, "4", "CONT004");
			AssertContainerSecurityStatusLine(header, 10, "SPX");
		}

		public void TestPopulateRateLines_ULD_DoesNotContainSLACInfo()
		{
			var consol = Factory.New<ForwardingConsol>();
			var refContainerPK = CreateReferenceContainer("LD1", 64, "1");
			var slacContainer1 = CreateULDContainer(consol, refContainerPK, 111, "CONT001", "2");
			var slacContainer2 = CreateULDContainer(consol, refContainerPK, 222, "CONT002", "3");
			var slacContainer3 = CreateULDContainer(consol, refContainerPK, 333, "CONT003", "5");
			var shipment = consol.Shipments.AddNew();
			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 15;
			slacContainer1.AddPackLines(new PackLine[] { packline1 });
			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 55;
			slacContainer2.AddPackLines(new PackLine[] { packline2 });

			var header = Factory.New<MockExportAWBHeader>();
			header.ULDContainers_Exposed = consol.Containers.Cast<CommonContainer>();

			var analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();

			var log = analyzer.LogsWrapper.Logs[0];
			log.Unit = Constants.Weight.Kilograms;
			log.Weight = 200m;
			log.Chargeable = 222m;
			log.ChargeableUnit = Constants.Weight.Kilograms;
			log.RateMode = Constants.ContainerModes.ULD;
			log.ContainerCode = "LD1";
			log.BaseRate = 32m;

			AssertEquals(4, analyzer.PopulateRateLines());
			var rateline = 1;

			AssertRateLine(header, rateline++, "3", 200, "K", "U", 222, 32, 32);    // Main Rate Line
			AssertRateLine(header, rateline++, "", 192, "", "X", 0, 0, 0);          // ULD info for cont1
			AssertRateLine(header, rateline++, "", 0, "", "X", 0, 0, 0);            // ULD info for cont2
			AssertRateLine(header, rateline++, "", 0, "", "X", 0, 0, 0);            // ULD info for cont3
		}

		public void TestPopulateULD_ContainerInformation_12thRateLine()
		{
			var header = Factory.New<MockExportAWBHeader>();
			var analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();

			var consol = Factory.New<ForwardingConsol>();
			var refContainerPK = CreateReferenceContainer("LD1", 64, "1");
			CreateULDContainer(consol, refContainerPK, 11, "CONT001", "1");
			CreateULDContainer(consol, refContainerPK, 22, "CONT002", "2");
			CreateULDContainer(consol, refContainerPK, 33, "CONT003", "3");
			CreateULDContainer(consol, refContainerPK, 44, "CONT004", "4");
			CreateULDContainer(consol, refContainerPK, 55, "CONT005", "5");
			CreateULDContainer(consol, refContainerPK, 66, "CONT006", "6");
			CreateULDContainer(consol, refContainerPK, 77, "CONT007", "7");
			CreateULDContainer(consol, refContainerPK, 88, "CONT008", "8");
			CreateULDContainer(consol, refContainerPK, 99, "CONT009", "9");
			CreateULDContainer(consol, refContainerPK, 110, "CONT010", "10");
			CreateULDContainer(consol, refContainerPK, 110, "CONT011", "11");
			CreateULDContainer(consol, refContainerPK, 120, "CONT012", "12");

			header.ULDContainers_Exposed = consol.Containers.Cast<CommonContainer>();

			var log = analyzer.LogsWrapper.Logs[0];
			log.Unit = Constants.Weight.Kilograms;
			log.Weight = 200m;
			log.Chargeable = 222m;
			log.ChargeableUnit = Constants.Weight.Kilograms;
			log.RateMode = Constants.ContainerModes.ULD;
			log.ContainerCode = "LD1";
			log.BaseRate = 32m;

			AssertEquals(11, analyzer.PopulateRateLines());
			var rateline = 1;

			AssertRateLine(header, rateline++, "12", 200, "K", "U", 222, 32, 32);   //Main Rate Line
			AssertRateLine(header, rateline++, "", 768, "", "X", 0, 0, 0);          //ULD info for cont1
			AssertRateLine(header, rateline++, "", 0, "", "X", 0, 0, 0);            //ULD info for cont2
			AssertRateLine(header, rateline++, "", 0, "", "X", 0, 0, 0);            //ULD info for cont3
			AssertRateLine(header, rateline++, "", 0, "", "X", 0, 0, 0);            //ULD info for cont4
			AssertRateLine(header, rateline++, "", 0, "", "X", 0, 0, 0);            //ULD info for cont5
			AssertRateLine(header, rateline++, "", 0, "", "X", 0, 0, 0);            //ULD info for cont6
			AssertRateLine(header, rateline++, "", 0, "", "X", 0, 0, 0);            //ULD info for cont7
			AssertRateLine(header, rateline++, "", 0, "", "X", 0, 0, 0);            //ULD info for cont8
			AssertRateLine(header, rateline++, "", 0, "", "X", 0, 0, 0);            //ULD info for cont9
			AssertRateLine(header, rateline++, "", 0, "", "X", 0, 0, 0);            //ULD info for cont10
			AssertEquals(12, rateline);
			AssertRateLine(header, rateline++, "", 0, "", "", 0, 0, 0);             //Should not contain X for ULD info
		}

		public void TestPopulateRateLines_ULD_CommodityItemNumber_EmptyCommodities()
		{
			RefContainer refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "LD1";
			refContainer.RC_TareWeight = 64;
			refContainer.RC_IATARateClass = "1";

			var consol = Factory.New<ForwardingConsol>();

			CommonContainer uldContainer = consol.Containers.AddNew();
			uldContainer.JC_ContainerMode = Constants.ContainerModes.ULD;
			uldContainer.JC_RC = refContainer.PK;
			uldContainer.JC_GrossWeight = 111;
			uldContainer.JC_ContainerNum = "CONT001";
			uldContainer.JC_SealNum = "2";
			uldContainer.JC_RH_NKContainerCommodityCode = ZString.Empty;

			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			MockAnalyzer analyzer = new MockAnalyzer(header);
			analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();

			CalculationLog log = analyzer.LogsWrapper.Logs[0];
			log.Unit = Constants.Weight.Kilograms;
			log.CommodityCode = ZString.Empty;
			log.Weight = 200m;
			log.Chargeable = 222m;
			log.ChargeableUnit = Constants.Weight.Kilograms;
			log.RateMode = Constants.ContainerModes.ULD;
			log.ContainerCode = "LD1";
			log.BaseRate = 32m;
			log.AddPerUnitCalculation(100m, Constants.Weight.Kilograms, 1.1m);
			log.AddPerUnitCalculation(200m, Constants.Weight.Kilograms, 2.2m);

			AssertEquals("Preconditon", Constants.ContainerModes.ULD, log.RateMode);
			analyzer.PopulateRateLines();

			var line = header.AWBRateLine1;
			AssertEquals("Rate class", Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, line.ER_RateClass);
			AssertEquals("Rate charge", ZString.Empty, line.ER_CommodityItemNumber);
		}

		public void TestPopulateRateLines_ULD_CommodityItemNumber_SameCommodities()
		{
			var commodity1 = Factory.New<RefCommodityCode>();
			commodity1.RH_Code = "AAAA";
			commodity1.RH_IATACommodityItem = "1111";

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "LD1";
			refContainer.RC_TareWeight = 64;
			refContainer.RC_IATARateClass = "1";

			Factory.Save();

			var header = Factory.New<MockExportAWBHeader>();
			var analyzer = new MockAnalyzer(header);

			var log = new CalculationLog();
			log.Unit = Constants.Weight.Kilograms;
			log.CommodityCode = "AAAA";
			log.Weight = 200m;
			log.Chargeable = 222m;
			log.ChargeableUnit = Constants.Weight.Kilograms;
			log.RateMode = Constants.ContainerModes.ULD;
			log.ContainerCode = "LD1";
			log.BaseRate = 32m;
			log.AddPerUnitCalculation(100m, Constants.Weight.Kilograms, 1.1m);
			log.AddPerUnitCalculation(200m, Constants.Weight.Kilograms, 2.2m);

			var log2 = new CalculationLog();
			log2.Unit = Constants.Weight.Kilograms;
			log2.CommodityCode = "AAAA";
			log2.Weight = 200m;
			log2.Chargeable = 222m;
			log2.ChargeableUnit = Constants.Weight.Kilograms;
			log2.RateMode = Constants.ContainerModes.ULD;
			log2.ContainerCode = "LD1";
			log2.BaseRate = 32m;
			log2.AddPerUnitCalculation(100m, Constants.Weight.Kilograms, 1.1m);
			log2.AddPerUnitCalculation(200m, Constants.Weight.Kilograms, 2.2m);

			CalculationLogsWrapper wrapper = new CalculationLogsWrapper();
			wrapper.Logs.Add(log);
			wrapper.Logs.Add(log2);

			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			CalculationLogsLoader.Save(bizo, wrapper);

			analyzer.BizoWithLogs = bizo;

			AssertEquals("Preconditon", Constants.ContainerModes.ULD, log.RateMode);
			analyzer.PopulateRateLines();

			var line = header.AWBRateLine1;
			AssertEquals("Rate class", Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, line.ER_RateClass);
			AssertEquals("Rate charge", "1111", line.ER_CommodityItemNumber);
		}

		public void TestPopulateRateLines_ULD_CommodityItemNumber_DifferentCommodities()
		{
			var commodity1 = Factory.New<RefCommodityCode>();
			commodity1.RH_Code = "AAAA";
			commodity1.RH_IATACommodityItem = "1111";

			var commodity2 = Factory.New<RefCommodityCode>();
			commodity2.RH_Code = "BBBB";
			commodity2.RH_IATACommodityItem = "2222";

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "LD1";
			refContainer.RC_TareWeight = 64;
			refContainer.RC_IATARateClass = "1";

			Factory.Save();

			var header = Factory.New<MockExportAWBHeader>();
			var analyzer = new MockAnalyzer(header);

			var log = new CalculationLog();
			log.Unit = Constants.Weight.Kilograms;
			log.CommodityCode = "AAAA";
			log.Weight = 200m;
			log.Chargeable = 222m;
			log.ChargeableUnit = Constants.Weight.Kilograms;
			log.RateMode = Constants.ContainerModes.ULD;
			log.ContainerCode = "LD1";
			log.BaseRate = 32m;
			log.AddPerUnitCalculation(100m, Constants.Weight.Kilograms, 1.1m);
			log.AddPerUnitCalculation(200m, Constants.Weight.Kilograms, 2.2m);

			var log2 = new CalculationLog();
			log2.Unit = Constants.Weight.Kilograms;
			log2.CommodityCode = "BBBB";
			log2.Weight = 200m;
			log2.Chargeable = 222m;
			log2.ChargeableUnit = Constants.Weight.Kilograms;
			log2.RateMode = Constants.ContainerModes.ULD;
			log2.ContainerCode = "LD1";
			log2.BaseRate = 32m;
			log2.AddPerUnitCalculation(100m, Constants.Weight.Kilograms, 1.1m);
			log2.AddPerUnitCalculation(200m, Constants.Weight.Kilograms, 2.2m);

			CalculationLogsWrapper wrapper = new CalculationLogsWrapper();
			wrapper.Logs.Add(log);
			wrapper.Logs.Add(log2);

			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			CalculationLogsLoader.Save(bizo, wrapper);

			analyzer.BizoWithLogs = bizo;

			AssertEquals("Preconditon", Constants.ContainerModes.ULD, log.RateMode);
			analyzer.PopulateRateLines();

			AssertEquals("Rate class", Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, header.AWBRateLine1.ER_RateClass);
			AssertEquals("Rate charge", "1111", header.AWBRateLine1.ER_CommodityItemNumber);

			AssertEquals("Rate class", Constants.AWB.RateClass.UnitLoadDeviceAdditionalCharge, header.AWBRateLine2.ER_RateClass);
			AssertEquals("Rate charge", "2222", header.AWBRateLine4.ER_CommodityItemNumber);
		}

		public void TestPopulateRateLines_ULD_WeightBased_MAWBSuppressULDTareWeight()
		{
			using (FreightDataRegistry.Instance.MAWBSuppressULDTareWeight.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var header = Factory.New<MockExportAWBHeader>();
				var analyzer = new MockAnalyzer(header);
				analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();

				var log = analyzer.LogsWrapper.Logs[0];
				log.RateMode = Constants.ContainerModes.ULD;
				log.Minimum = 100m;
				log.Unit = Constants.Weight.Kilograms;
				log.Weight = 200m;
				log.AddPerUnitCalculation(10m, Constants.Weight.Kilograms, 2m);

				AssertEquals("Minimum line populated", 1, analyzer.PopulateRateLines());
				AssertRateLine(header, 1, "0", 200m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 10m, 2m, 20m);
			}

			using (FreightDataRegistry.Instance.MAWBSuppressULDTareWeight.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var header = Factory.New<MockExportAWBHeader>();
				var analyzer = new MockAnalyzer(header);
				analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();

				var log = analyzer.LogsWrapper.Logs[0];
				log.RateMode = Constants.ContainerModes.ULD;
				log.Minimum = 100m;
				log.Unit = Constants.Weight.Kilograms;
				log.Weight = 200m;
				log.AddPerUnitCalculation(10m, Constants.Weight.Kilograms, 2m);

				AssertEquals("Minimum line populated", 1, analyzer.PopulateRateLines());
				AssertRateLine(header, 1, "0", 200m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 10m, 2m, 20m);
			}
		}

		public void TestPopulateRateLines_ULD_ContainerBased_MAWBSuppressULDTareWeight()
		{
			using (FreightDataRegistry.Instance.MAWBSuppressULDTareWeight.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var header = Factory.New<MockExportAWBHeader>();
				var analyzer = new MockAnalyzer(header);
				analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();
				header.ULDContainers_Exposed = CreateULDContainers();

				var log = analyzer.LogsWrapper.Logs[0];
				log.Unit = Constants.Weight.Kilograms;
				log.Weight = 200m;
				log.Chargeable = 222m;
				log.ChargeableUnit = Constants.Weight.Kilograms;
				log.RateMode = Constants.ContainerModes.ULD;
				log.ContainerCode = "LD1";
				log.BaseRate = 32m;
				log.AddPerUnitCalculation(100m, Constants.Weight.Kilograms, 1.1m);
				AssertEquals("UnitLoadDevice: U, E, X lines populated", 4, analyzer.PopulateRateLines());

				AssertRateLine(header, 1, "2", 200m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 222m, 32m, 32m);
				AssertRateLine(header, 2, "", 0m, "", Constants.AWB.RateClass.UnitLoadDeviceAdditionalCharge, 100m, 1.1m, 110m);
				AssertRateLine(header, 3, "", 128m, "", Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation, 0m, 0m, 0m);
				AssertContainerInfoLine(header, 4, 0m, "", "CONT002");
			}

			using (FreightDataRegistry.Instance.MAWBSuppressULDTareWeight.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var header = Factory.New<MockExportAWBHeader>();
				var analyzer = new MockAnalyzer(header);
				analyzer.BizoWithLogs = CreateBizoWithLogsWrapper();
				header.ULDContainers_Exposed = CreateULDContainers();

				var log = analyzer.LogsWrapper.Logs[0];
				log.Unit = Constants.Weight.Kilograms;
				log.Weight = 200m;
				log.Chargeable = 222m;
				log.ChargeableUnit = Constants.Weight.Kilograms;
				log.RateMode = Constants.ContainerModes.ULD;
				log.ContainerCode = "LD1";
				log.BaseRate = 32m;
				log.AddPerUnitCalculation(100m, Constants.Weight.Kilograms, 1.1m);
				AssertEquals("UnitLoadDevice: U, E, X lines populated", 4, analyzer.PopulateRateLines());

				AssertRateLine(header, 1, "2", 200m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 222m, 32m, 32m);
				AssertRateLine(header, 2, "", 0m, "", Constants.AWB.RateClass.UnitLoadDeviceAdditionalCharge, 100m, 1.1m, 110m);
				AssertRateLine(header, 3, "", 0m, "", Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation, 0m, 0m, 0m);
				AssertContainerInfoLine(header, 4, 0m, "", "CONT002");
			}
		}

		public void TestPopulateRateLines_ULD_PerContainer()
		{
			using (FreightDataRegistry.Instance.MAWBSuppressULDTareWeight.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var refContainerLD1Pk = CreateReferenceContainer("LD1", 60, "1");
				var refContainerLD2Pk = CreateReferenceContainer("LD2", 120, "2");

				var consol = Factory.New<ForwardingConsol>();
				var containerLD1_1 = CreateULDContainer(consol, refContainerLD1Pk, 111, "CONT001", "1");
				var containerLD1_2 = CreateULDContainer(consol, refContainerLD1Pk, 333, "", "2", 3);
				var containerLD2_1 = CreateULDContainer(consol, refContainerLD2Pk, 222, "CONT002", "3");
				var containerLD2_2 = CreateULDContainer(consol, refContainerLD2Pk, 444, "", "4", 2);

				var shipment = consol.Shipments.AddNew();
				var packline1 = shipment.OuterPackLines.AddNew();
				packline1.JL_PackageCount = 2;
				packline1.JL_ActualWeight = 100m;
				packline1.JL_ActualWeightUQ = "KG";
				containerLD1_1.AddPackLines(new PackLine[] { packline1 });

				var packline2 = shipment.OuterPackLines.AddNew();
				packline2.JL_PackageCount = 6;
				packline2.JL_ActualWeight = 300m;
				packline2.JL_ActualWeightUQ = "KG";
				containerLD1_2.AddPackLines(new PackLine[] { packline2 });

				var packline3 = shipment.OuterPackLines.AddNew();
				packline3.JL_PackageCount = 4;
				packline3.JL_ActualWeight = 500m;
				packline3.JL_ActualWeightUQ = "KG";
				containerLD2_1.AddPackLines(new PackLine[] { packline3 });

				var packline4 = shipment.OuterPackLines.AddNew();
				packline4.JL_PackageCount = 8;
				packline4.JL_ActualWeight = 1000m;
				packline4.JL_ActualWeightUQ = "KG";
				containerLD2_2.AddPackLines(new PackLine[] { packline4 });

				var header = Factory.New<MockExportAWBHeader>();
				header.OverrideShouldPopulateScacLine = true;
				header.ULDContainers_Exposed = consol.Containers.Cast<CommonContainer>();
				var analyzer = new MockAnalyzer(header);

				var log1 = new CalculationLog();
				log1.Unit = QuantityUnit.CN;
				log1.CommodityCode = "AAAA";
				log1.RateMode = Constants.ContainerModes.ULD;
				log1.ContainerCode = "LD1";
				var log1Step1 = log1.AddPerUnitCalculation(1, QuantityUnit.CN, 100m);
				log1.AddPerUnitCalculation(3, QuantityUnit.CN, 100m);

				var log2 = new CalculationLog();
				log2.Unit = QuantityUnit.CN;
				log2.CommodityCode = "BBBB";
				log2.RateMode = Constants.ContainerModes.ULD;
				log2.ContainerCode = "LD2";
				var log2Step1 = log2.AddPerUnitCalculation(1, QuantityUnit.CN, 200m);
				log2.AddPerUnitCalculation(2, QuantityUnit.CN, 200m);

				CalculationLogsWrapper wrapper = new CalculationLogsWrapper();
				wrapper.Logs.Add(log1);
				wrapper.Logs.Add(log2);

				DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
				CalculationLogsLoader.Save(bizo, wrapper);

				analyzer.BizoWithLogs = bizo;
				AssertEquals(9, analyzer.PopulateRateLines());

				AssertRateLine(header, 1, "4", 400m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 0m, 100m, 400m);
				AssertRateLine(header, 2, "", 0m, "", "", 0m, 0m, 0m, true, 2);
				AssertContainerInfoLine(header, 3, 240m, "1", "CONT001");
				AssertRateLine(header, 4, "", 0m, "", "", 0m, 0m, 0m, true, 6);
				AssertContainerInfoLine(header, 5, 0m, "", "");
				AssertRateLine(header, 6, "3", 1500m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 0m, 200m, 600m, true, 4);
				AssertContainerInfoLine(header, 7, 360m, "2", "CONT002");
				AssertRateLine(header, 8, "", 0m, "", "", 0m, 0m, 0m, true,8);
				AssertContainerInfoLine(header, 9, 0m, "", "");
			}
		}

		#endregion

		#region ConvertMoneyAmountsToAWBCurrency

		public void TestGetMoneyAmountInAWBCurrency()
		{
			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			MockAnalyzer analyzer = new MockAnalyzer(header);

			Action<ExportAWBHeader, int, ZDecimal, ZDecimal> populateRateLine = (header1, lineNumber, rateCharge, rateTotal) =>
			{
				ExportAWBRateLine rateLine = header1.AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)lineNumber];
				using (rateLine.SuspendSettingHasChanges())
				using (rateLine.SuppressTotalAndRateChargeCalculation())
				{
					rateLine.ER_Total = rateTotal;
					rateLine.ER_RateChargeOrDiscount = rateCharge;
				}
			};

			populateRateLine(header, 1, 10, 100);
			populateRateLine(header, 2, 20, 200);

			analyzer.ConvertMonetaryAmountsToAWBCurrency_Exposed(2, "");
			AssertRateLine(header, 1, "", 0m, "", "", 0m, 10m, 100m);
			AssertRateLine(header, 2, "", 0m, "", "", 0m, 20m, 200m);

			analyzer.AWBCurrencyExchangeRate = 1.024m;

			analyzer.ConvertMonetaryAmountsToAWBCurrency_Exposed(2, "");
			AssertRateLine(header, 1, "", 0m, "", "", 0m, 10.24m, 102.4m);
			AssertRateLine(header, 2, "", 0m, "", "", 0m, 20.48m, 204.8m);
		}

		#endregion

		#region Get* methods

		public void TestGetLineWeightUnit()
		{
			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			MockAnalyzer analyzer = new MockAnalyzer(header);

			AssertEquals(Constants.AWB.RateLineUQ.Kilos, analyzer.GetLineWeightUnit(""));
			AssertEquals(Constants.AWB.RateLineUQ.Kilos, analyzer.GetLineWeightUnit(Constants.Weight.Kilograms));
			AssertEquals(Constants.AWB.RateLineUQ.Kilos, analyzer.GetLineWeightUnit(Constants.Weight.Tonnes));
			AssertEquals(Constants.AWB.RateLineUQ.Pounds, analyzer.GetLineWeightUnit(Constants.Weight.Pounds));
			AssertEquals(Constants.AWB.RateLineUQ.Pounds, analyzer.GetLineWeightUnit(Constants.Weight.Ounces));
		}

		public void TestGetTargetWeightUnit()
		{
			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			MockAnalyzer analyzer = new MockAnalyzer(header);

			AssertEquals(Constants.Weight.Kilograms, analyzer.GetTargetWeightUnit(""));
			AssertEquals(Constants.Weight.Kilograms, analyzer.GetTargetWeightUnit(Constants.Weight.Kilograms));
			AssertEquals(Constants.Weight.Kilograms, analyzer.GetTargetWeightUnit(Constants.Weight.Tonnes));
			AssertEquals(Constants.Weight.Pounds, analyzer.GetTargetWeightUnit(Constants.Weight.Pounds));
			AssertEquals(Constants.Weight.Pounds, analyzer.GetTargetWeightUnit(Constants.Weight.Ounces));
		}

		public void TestGetConvertedValue()
		{
			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			MockAnalyzer analyzer = new MockAnalyzer(header);

			AssertEquals(10m, analyzer.GetConvertedValue(10m, ""));
			AssertExceptionThrown(typeof(ArgumentException), () => analyzer.GetConvertedValue(10m, "XXX"));

			AssertEquals(10m, analyzer.GetConvertedValue(10m, Constants.Weight.Kilograms));
			AssertEquals(10000m, analyzer.GetConvertedValue(10m, Constants.Weight.Tonnes));

			AssertEquals(10m, analyzer.GetConvertedValue(10m, Constants.Weight.Pounds));
			AssertEquals(0.625m, analyzer.GetConvertedValue(10m, Constants.Weight.Ounces));
		}

		public void TestGetULDContainers()
		{
			MockExportAWBHeader header = Factory.New<MockExportAWBHeader>();
			MockAnalyzer analyzer = new MockAnalyzer(header);
			header.ULDContainers_Exposed = CreateULDContainers();

			AssertEquals(2, analyzer.GetULDContainers("LD1").Count());
			AssertEquals(2, analyzer.GetULDContainers("LD1").Count(x => x.RefContainer.RC_Code == "LD1"));

			AssertEquals(1, analyzer.GetULDContainers("LD2").Count());
			AssertEquals(1, analyzer.GetULDContainers("LD2").Count(x => x.RefContainer.RC_Code == "LD2"));

			AssertEquals(0, analyzer.GetULDContainers("").Count());
			AssertEquals(0, analyzer.GetULDContainers("XXX").Count());
		}

		#endregion

		#region Implementation

		public class MockAnalyzer : CalculationLogsAnalyzer
		{
			public MockAnalyzer(ExportAWBHeader exportAWBHeader)
				: base(exportAWBHeader)
			{
				isSingleBizoWithLogs = true;
			}

			bool isSingleBizoWithLogs;
			public BusinessObject BizoWithLogs
			{
				get
				{
					return bizoWithLogs;
				}
				set
				{
					isSingleBizoWithLogs = true;
					bizoWithLogs = value;
				}
			}
			BusinessObject bizoWithLogs;

			public List<BusinessObject> BizosWithLogs
			{
				get
				{
					if (bizosWithLogs.IsNullOrEmpty())
					{
						bizosWithLogs = new List<BusinessObject>();
					}
					return bizosWithLogs;
				}
				set
				{
					isSingleBizoWithLogs = false;
					bizosWithLogs = value;
				}
			}
			List<BusinessObject> bizosWithLogs;

			protected override IEnumerable<BusinessObject> GetAllBusinessObjectsWithCalculationLogs()
			{
				if (isSingleBizoWithLogs)
				{
					return new List<BusinessObject>() { BizoWithLogs };
				}
				return BizosWithLogs;
			}

			public new IEnumerable<CommonContainer> GetULDContainers(ZString containerCode)
			{
				return base.GetULDContainers(containerCode);
			}

			public ZInt NumberOfPieces_Exposed { get; set; }

			protected override ZInt GetNumberOfPieces()
			{
				return NumberOfPieces_Exposed != 0 ? NumberOfPieces_Exposed : base.GetNumberOfPieces();
			}

			public new ZString GetLineWeightUnit(ZString weightUnit)
			{
				return base.GetLineWeightUnit(weightUnit);
			}

			public new ZString GetTargetWeightUnit(ZString weightUnit)
			{
				return base.GetTargetWeightUnit(weightUnit);
			}

			public new ZDecimal GetConvertedValue(ZDecimal originalValue, ZString weightUnit, ZString? targetUnit = null)
			{
				return base.GetConvertedValue(originalValue, weightUnit, targetUnit);
			}

			public void ConvertMonetaryAmountsToAWBCurrency_Exposed(int numberOfLinesPopulated, ZString currencyCode)
			{
				ConvertMonetaryAmountsToAWBCurrency(numberOfLinesPopulated, currencyCode);
			}

			protected override void ConvertMonetaryAmountsToAWBCurrency(int numberOfLinesPopulated, ZString currencyCode)
			{
				base.ConvertMonetaryAmountsToAWBCurrency(numberOfLinesPopulated, currencyCode);
				ConvertMonetaryAmountsParameters = new object[2];
				ConvertMonetaryAmountsParameters[0] = numberOfLinesPopulated;
				ConvertMonetaryAmountsParameters[1] = currencyCode;
			}
			public object[] ConvertMonetaryAmountsParameters;

			protected override ZDecimal GetAmountInAWBCurrency(ZDecimal originalAmount, ZString originalCurrencyCode)
			{
				return base.GetAmountInAWBCurrency(originalAmount, originalCurrencyCode) * AWBCurrencyExchangeRate;
			}
			public ZDecimal AWBCurrencyExchangeRate = 1m;
		}

		internal static void AssertRateLine(ExportAWBHeader header, int lineNumber, ZString numberOfPieces, ZDecimal grossWeight, ZString ratelineWeightUnit, ZString rateClass, ZDecimal chargeableWeight, ZDecimal rateCharge, ZDecimal total, bool hasSLAC = false, int slacCount = 0)
		{
			ExportAWBRateLine line = header.AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)lineNumber];
			CombineAssertions(delegate
			{
				AssertEquals("HasChanges", false, line.HasChanges);
				AssertEquals("Number of pieces", numberOfPieces, line.ER_NoOfPiecesOrRCP);
				AssertEquals("Gross weight", grossWeight, line.ER_GrossWeight);
				AssertEquals("Weight unit", ratelineWeightUnit, line.ER_WeightInLBsOrKGs);
				AssertEquals("Rate class", rateClass, line.ER_RateClass);
				AssertEquals("Chargeable weight", chargeableWeight, line.ER_ChargeableWeight);
				AssertEquals("Rate charge", rateCharge, line.ER_RateChargeOrDiscount);
				AssertEquals("Total", total, line.ER_Total);

				if (hasSLAC)
				{
					AssertEquals("NatureAndQtyOfGoodsType", Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount, line.ER_NatureAndQtyOfGoodsType);
					AssertEquals("NatureAndQtyOfGoods SLAC Count", slacCount, line.NatureAndQtyOfGoodsSLAC.Count);
				}
			});
		}

		void AssertContainerInfoLine(ExportAWBHeader header, int lineNumber, ZDecimal grossWeight, ZString commodityItemNumber, ZString natureAndQtyOfGoods)
		{
			AssertRateLine(header, lineNumber, "", grossWeight, "", Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation, 0m, 0m, 0m);

			ExportAWBRateLine line = header.AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)lineNumber];
			AssertEquals("Commodity Item Number", commodityItemNumber, line.ER_CommodityItemNumber);
			AssertEquals("Nature And Qty Of Goods", natureAndQtyOfGoods, GetNatureAndQtyOfGoods(header, lineNumber));
		}

		void AssertContainerSecurityStatusLine(ExportAWBHeader header, int lineNumber, ZString natureAndQtyOfGoods)
		{
			AssertRateLine(header, lineNumber, "", 0m, "", "", 0m, 0m, 0m);

			var line = header.AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)lineNumber];
			AssertEquals("Nature and Qty of Goods Type", Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, line.ER_NatureAndQtyOfGoodsType);
			AssertEquals("Nature and Qty of Goods", natureAndQtyOfGoods, line.NatureAndQtyOfGoods.Text);
		}

		internal static ZString GetNatureAndQtyOfGoods(ExportAWBHeader header, int lineNumber)
		{
			ZString result = "";

			if (lineNumber > 0 && lineNumber <= header.AWBRateLines.Count)
			{
				result = header.AWBRateLines[lineNumber - 1].NatureAndQtyOfGoods.Text;
			}
			else
			{
				Fail("Invalid line number for NatureAndQtyOfGoods");
			}

			return result;
		}

		BusinessObject CreateBizoWithLogsWrapper()
		{
			CalculationLog calculationLog = new CalculationLog();
			calculationLog.Unit = Constants.Weight.Kilograms;

			CalculationLogsWrapper wrapper = new CalculationLogsWrapper();
			wrapper.Logs.Add(calculationLog);

			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			CalculationLogsLoader.Save(bizo, wrapper);

			return bizo;
		}

		IEnumerable<CommonContainer> CreateULDContainers()
		{
			var refContainer1PK = CreateReferenceContainer("LD1", 64, "1");
			var refContainer2PK = CreateReferenceContainer("LD2", 256, "4");

			var consol = Factory.New<ForwardingConsol>();

			CreateULDContainer(consol, refContainer1PK, 111, "CONT001", "2");
			CreateULDContainer(consol, refContainer1PK, 222, "CONT002", "3");
			CreateULDContainer(consol, refContainer2PK, 444, "CONT004", "5");

			CommonContainer uldContainer3 = consol.Containers.AddNew();
			uldContainer3.JC_ContainerMode = Constants.ContainerModes.ULD;

			return consol.Containers.Cast<CommonContainer>();
		}

		ZGuid CreateReferenceContainer(string code, decimal tare, string iataRateClass)
		{
			return CreateReferenceContainer(Factory, code, tare, iataRateClass);
		}

		internal static ZGuid CreateReferenceContainer(BusinessObjectFactory factory, string code, decimal tare, string iataRateClass)
		{
			var refContainer = factory.New<RefContainer>();
			refContainer.RC_Code = code;
			refContainer.RC_TareWeight = tare;
			refContainer.RC_IATARateClass = iataRateClass;
			return refContainer.PK;
		}

		internal static CommonContainer CreateULDContainer(ForwardingConsol consol, ZGuid refPK, decimal grossWeight, string containerNum, string sealNum, short containerCount = 1)
		{
			var container = consol.Containers.AddNew();
			container.JC_RC = refPK;
			container.JC_GrossWeight = grossWeight;
			container.JC_ContainerNum = containerNum;
			container.JC_SealNum = sealNum;
			container.JC_ContainerMode = Constants.ContainerModes.ULD;
			container.JC_ContainerCount = containerCount;
			return container;
		}

		#endregion
	}
}
