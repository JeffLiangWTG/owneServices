using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class ConsolCalculationLogsAnalyzerTest : TestCaseWithFactory
	{
		public void TestLogsWrapper()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AccChargeCode randomChargeCode = Factory.New<AccChargeCode>();

			BusinessObject consolCost1 = CreateConsolCost(consol, randomChargeCode.PK);
			BusinessObject consolCost2 = CreateConsolCost(consol, Env.Registry.FreightChargeCode);
			BusinessObject consolCost3 = CreateConsolCost(consol, randomChargeCode.PK);

			CalculationLog calculationLog = new CalculationLog();
			calculationLog.CalculatorCode = "AAA";

			CalculationLogsWrapper logsWrapper = new CalculationLogsWrapper();
			logsWrapper.Logs.Add(calculationLog);

			CalculationLogsLoader.Save(consolCost2, logsWrapper);

			ConsolExportAWBHeader header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			ConsolCalculationLogsAnalyzer analyzer = new ConsolCalculationLogsAnalyzer(header);
			AssertEquals("Calculation logs from the relevant consol cost", "AAA", analyzer.LogsWrapper.Logs[0].CalculatorCode);

			CalculationLog calculationLogOnConsol = new CalculationLog();
			calculationLogOnConsol.CalculatorCode = "CON";

			CalculationLogsWrapper logsWrapperOnConsol = new CalculationLogsWrapper();
			logsWrapperOnConsol.Logs.Add(calculationLogOnConsol);

			CalculationLogsLoader.Save(consol, logsWrapperOnConsol);

			analyzer = new ConsolCalculationLogsAnalyzer(header);
			AssertEquals("Calculation logs from consol itself", "CON", analyzer.LogsWrapper.Logs[0].CalculatorCode);

			CalculationLogsLoader.Disable(consol);

			analyzer = new ConsolCalculationLogsAnalyzer(header);
			AssertEquals("Calculation logs from consol cost when consol's own logs are disabled", "AAA", analyzer.LogsWrapper.Logs[0].CalculatorCode);
		}

		public void TestLogsWrapperCollection()
		{
			var consol = Factory.New<ForwardingConsol>();

			var consolCost1 = CreateConsolCost(consol, Env.Registry.FreightChargeCode);
			var consolCost2 = CreateConsolCost(consol, Env.Registry.FreightChargeCode);
			var consolCost3 = CreateConsolCost(consol, Env.Registry.FreightChargeCode);

			var calculationLog1 = new CalculationLog();
			var calculationLog2 = new CalculationLog();
			var calculationLog3 = new CalculationLog();

			calculationLog1.CalculatorCode = "AAA";
			calculationLog2.CalculatorCode = "BBB";
			calculationLog3.CalculatorCode = "CCC";

			var logsWrapper1 = new CalculationLogsWrapper();
			var logsWrapper2 = new CalculationLogsWrapper();
			var logsWrapper3 = new CalculationLogsWrapper();
			logsWrapper1.Logs.Add(calculationLog1);
			logsWrapper2.Logs.Add(calculationLog2);
			logsWrapper3.Logs.Add(calculationLog3);

			CalculationLogsLoader.Save(consolCost1, logsWrapper1);
			CalculationLogsLoader.Save(consolCost2, logsWrapper2);
			CalculationLogsLoader.Save(consolCost3, logsWrapper3);

			var header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			var analyzer = new ConsolCalculationLogsAnalyzer(header);
			AssertEquals("Multiple logs wrappers from consol costs", 3, analyzer.LogsWrapperCollection.Count());
			AssertEquals("Original logs wrapper uses first result", "AAA", analyzer.LogsWrapper.Logs[0].CalculatorCode);
		}

		public void TestPopulateRateLines_MultipleLogs()
		{
			var consol = Factory.New<ForwardingConsol>();

			var consolCost1 = CreateConsolCost(consol, Env.Registry.FreightChargeCode);
			var consolCost2 = CreateConsolCost(consol, Env.Registry.FreightChargeCode);
			var consolCost3 = CreateConsolCost(consol, Env.Registry.FreightChargeCode);

			CalculationLog CreateCalculationLog(string unit, decimal weight, string rateMode, string containerCode, decimal unitCount, decimal unitPrice)
			{
				var log = new CalculationLog();
				log.Unit = unit;
				log.Weight = weight;
				log.RateMode = rateMode;
				log.ContainerCode = containerCode;
				log.AddPerUnitCalculation(unitCount, unit, unitPrice);
				return log;
			}

			var calculationLog1 = CreateCalculationLog(Constants.Weight.Kilograms, 200m, Constants.ContainerModes.ULD, "LD1", 200m, 2m);
			var calculationLog2 = CreateCalculationLog(Constants.Weight.Kilograms, 300m, Constants.ContainerModes.ULD, "LD2", 300m, 3m);
			var calculationLog3 = CreateCalculationLog(Constants.Weight.Kilograms, 400m, Constants.ContainerModes.ULD, "LD3", 400m, 4m);

			calculationLog1.CalculatorCode = "AAA";
			calculationLog2.CalculatorCode = "BBB";
			calculationLog3.CalculatorCode = "CCC";

			var logsWrapper1 = new CalculationLogsWrapper();
			var logsWrapper2 = new CalculationLogsWrapper();
			var logsWrapper3 = new CalculationLogsWrapper();
			logsWrapper1.Logs.Add(calculationLog1);
			logsWrapper2.Logs.Add(calculationLog2);
			logsWrapper3.Logs.Add(calculationLog3);

			CalculationLogsLoader.Save(consolCost1, logsWrapper1);
			CalculationLogsLoader.Save(consolCost2, logsWrapper2);
			CalculationLogsLoader.Save(consolCost3, logsWrapper3);

			var header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			var analyzer = new ConsolCalculationLogsAnalyzer(header);

			AssertEquals("lines populated", 3, analyzer.PopulateRateLines());

			AssertRateLine(header, 1, "0", 200m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 200m, 2m, 400m);
			AssertRateLine(header, 2, "0", 300m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 300m, 3m, 900m);
			AssertRateLine(header, 3, "0", 400m, Constants.AWB.RateLineUQ.Kilos, Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, 400m, 4m, 1600m);
		}

		public void TestDisableLogs()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			BusinessObject consolCost = CreateConsolCost(consol, Env.Registry.FreightChargeCode);

			CalculationLogsLoader.Save(consol, new CalculationLogsWrapper());
			CalculationLogsLoader.Save(consolCost, new CalculationLogsWrapper());

			AssertEquals("Precondition: log wrapper enabled", false, CalculationLogsLoader.Load(consol).IsDisabled);
			AssertEquals("Precondition: log wrapper enabled", false, CalculationLogsLoader.Load(consolCost).IsDisabled);

			ConsolExportAWBHeader header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			ConsolCalculationLogsAnalyzer analyzer = new ConsolCalculationLogsAnalyzer(header);
			analyzer.DisableLogs();

			AssertEquals("Log wrapper disabled", true, CalculationLogsLoader.Load(consol).IsDisabled);
			AssertEquals("Log wrapper disabled", true, CalculationLogsLoader.Load(consolCost).IsDisabled);
		}

		public void TestULDContainers()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Containers.AddNew().JC_ContainerMode = Constants.ContainerModes.ULD;
			consol.Containers.AddNew().JC_ContainerMode = Constants.ContainerModes.AIR;
			consol.Containers.AddNew().JC_ContainerMode = "XXX";
			consol.Containers.AddNew().JC_ContainerMode = Constants.ContainerModes.ULD;

			ConsolExportAWBHeader header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			MockConsolAnalyzer analyzer = new MockConsolAnalyzer(header);
			AssertEquals(2, header.ULDContainers.Count());
		}

		public void TestGetNumberOfPieces()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			ConsolExportAWBHeader header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			MockConsolAnalyzer analyzer = new MockConsolAnalyzer(header);
			AssertEquals(0, analyzer.GetNumberOfPieces());

			consol.Shipments.AddNew().JS_OuterPacks = 10;
			AssertEquals(10, analyzer.GetNumberOfPieces());

			consol.Shipments.AddNew().JS_OuterPacks = 20;
			AssertEquals(30, analyzer.GetNumberOfPieces());
		}

		public void TestGetAmountInAWBCurrency()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;

				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";

				var voyageExRate1 = voyage.ExRates.AddNew();
				voyageExRate1.E8_RX_NKExCurrency = "USD";
				voyageExRate1.E8_VoyageExchangeRate = 1.024m;

				var voyageExRate2 = voyage.ExRates.AddNew();
				voyageExRate2.E8_RX_NKExCurrency = "EUR";
				voyageExRate2.E8_VoyageExchangeRate = 2.222m;

				var voyageExRate3 = voyage.ExRates.AddNew();
				voyageExRate3.E8_RX_NKExCurrency = "NZD";
				voyageExRate3.E8_VoyageExchangeRate = 2.048m;

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.Transports[0].JW_JX = voyage.Sailings[0].PK;

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;
				header.EH_Currency = "USD";

				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				CreateConsolCost(consol, chargeCode.PK, "USD", 1.111m);

				var analyzer = new MockConsolAnalyzer(header);

				var amount = analyzer.GetAmountInAWBCurrency(100, "USD");
				AssertEquals("Precondition: Charge: USD, Header: USD, should not convert", 100m, amount);

				amount = analyzer.GetAmountInAWBCurrency(100, "AUD");
				AssertEquals("Charge: AUD, Header: USD, convert amount from Local to Foreign", 102.40m, amount);

				header.EH_Currency = "AUD";
				amount = analyzer.GetAmountInAWBCurrency(100, "AUD");
				AssertEquals("Charge: AUD, Header: AUD, should not convert", 100m, amount);

				amount = analyzer.GetAmountInAWBCurrency(100, "USD");
				AssertEquals("Charge: USD, Header: AUD, convert amount from Foreign to Local", 97.66m, amount);

				header.EH_Currency = "NZD";
				amount = analyzer.GetAmountInAWBCurrency(100, "EUR");
				AssertEquals("Charge: EUR, Header: NZD, convert amount from Foreign to Foreign", 92.17m, amount);
			}
		}

		public void TestPopulateRateLines_ULD_AdditionalSLACInformation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var refContainerPK = CalculationLogsAnalyzerTest.CreateReferenceContainer(Factory, "LD1", 64, "1");
			var slacContainer1 = CalculationLogsAnalyzerTest.CreateULDContainer(consol, refContainerPK, 111, "CONT001", "2");
			var slacContainer2 = CalculationLogsAnalyzerTest.CreateULDContainer(consol, refContainerPK, 222, "CONT002", "3");
			var slacContainer3 = CalculationLogsAnalyzerTest.CreateULDContainer(consol, refContainerPK, 333, "CONT003", "5");

			var shipment = consol.Shipments.AddNew();
			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 15;
			slacContainer1.AddPackLines(new PackLine[] { packline1 });
			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 55;
			slacContainer2.AddPackLines(new PackLine[] { packline2 });

			var costing = CreateConsolCost(consol, Env.Registry.FreightChargeCode, "USD", 1.111m);

			var log = new CalculationLog();
			log.Unit = Constants.Weight.Kilograms;
			log.Weight = 200m;
			log.Chargeable = 222m;
			log.ChargeableUnit = Constants.Weight.Kilograms;
			log.RateMode = Constants.ContainerModes.ULD;
			log.ContainerCode = "LD1";
			log.BaseRate = 32m;

			var logsWrapper = new CalculationLogsWrapper();
			logsWrapper.Logs.Add(log);

			CalculationLogsLoader.Save(costing, logsWrapper);

			var header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;
			header.EH_Currency = "USD";

			var analyzer = new MockConsolAnalyzer(header);

			AssertEquals(6, analyzer.PopulateRateLines());
			var rateline = 1;

			AssertRateLine(header, rateline++, "3", 200, "K", "U", 222, 32, 32);                        // Main Rate Line
			AssertEquals("Slac Info Line 2", "15 SLAC", GetNatureAndQtyOfGoods(header, rateline++));    // SLAC for cont1
			AssertRateLine(header, rateline++, "", 192, "", "X", 0, 0, 0);                              // ULD info for cont1
			AssertEquals("Slac Info Line 4", "55 SLAC", GetNatureAndQtyOfGoods(header, rateline++));    // SLAC for cont2
			AssertRateLine(header, rateline++, "", 0, "", "X", 0, 0, 0);                                // ULD info for cont2
			AssertRateLine(header, rateline++, "", 0, "", "X", 0, 0, 0);                                // ULD info for cont3
		}

		#region Implementation

		class MockConsolAnalyzer : ConsolCalculationLogsAnalyzer
		{
			public MockConsolAnalyzer(ConsolExportAWBHeader exportAWBHeader)
				: base(exportAWBHeader)
			{
			}

			public new ZDecimal GetAmountInAWBCurrency(ZDecimal originalAmount, ZString originalCurrencyCode)
			{
				return base.GetAmountInAWBCurrency(originalAmount, originalCurrencyCode);
			}

			public new ZInt GetNumberOfPieces()
			{
				return base.GetNumberOfPieces();
			}
		}

		BusinessObject CreateConsolCost(ForwardingConsol consol, ZGuid chargeCodePK)
		{
			return CreateConsolCost(consol, chargeCodePK, string.Empty, 0m);
		}

		BusinessObject CreateConsolCost(ForwardingConsol consol, ZGuid chargeCodePK, ZString currencyCode, ZDecimal exchangeRate)
		{
			var result = (BusinessObject)Factory.New<IJobConsolCost>();
			result[JobConsolCostSchema.E6_AC_ChargeCode] = chargeCodePK;
			result[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;

			result.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				result[JobConsolCostSchema.E6_ParentID] = consol.PK;
				result[JobConsolCostSchema.E6_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			}
			finally
			{
				result.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}

			if (!currencyCode.IsEmpty)
			{
				result[JobConsolCostSchema.E6_RX_NKCurrency] = currencyCode;
				result[JobConsolCostSchema.E6_ExchangeRate] = exchangeRate;
			}

			return result;
		}

		void AssertRateLine(ExportAWBHeader header, int lineNumber, ZString numberOfPieces, ZDecimal grossWeight, ZString ratelineWeightUnit, ZString rateClass, ZDecimal chargeableWeight, ZDecimal rateCharge, ZDecimal total)
		{
			CalculationLogsAnalyzerTest.AssertRateLine(header, lineNumber, numberOfPieces, grossWeight, ratelineWeightUnit, rateClass, chargeableWeight, rateCharge, total);
		}

		ZString GetNatureAndQtyOfGoods(ExportAWBHeader header, int lineNumber)
		{
			return CalculationLogsAnalyzerTest.GetNatureAndQtyOfGoods(header, lineNumber);
		}

		#endregion
	}
}