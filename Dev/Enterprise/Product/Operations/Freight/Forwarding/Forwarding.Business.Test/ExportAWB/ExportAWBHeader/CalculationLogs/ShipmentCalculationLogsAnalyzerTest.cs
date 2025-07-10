using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class ShipmentCalculationLogsAnalyzerTest : TestCaseWithFactory
	{
		public void TestLogsWrapper()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var header = Factory.New<ShipmentExportAWBHeader>();
			header.EH_ParentID = shipment.PK;

			var analyzer = new ShipmentCalculationLogsAnalyzer(header);
			AssertNull("LogsWrapper is null by default", analyzer.LogsWrapper);

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = shipment.PK;

			var randomChargeCode = Factory.New<AccChargeCode>();

			var jobCharge1 = Factory.New<JobCharge>();
			jobCharge1.JR_AC = randomChargeCode.PK;
			jobCharge1.JR_JH = jobHeader.PK;

			var jobCharge2 = Factory.New<JobCharge>();
			jobCharge2.JR_AC = Env.Registry.FreightChargeCode;
			jobCharge2.JR_JH = jobHeader.PK;

			var jobCharge3 = Factory.New<JobCharge>();
			jobCharge3.JR_AC = Env.Registry.FreightChargeCode;
			jobCharge3.JR_JH = jobHeader.PK;

			var calculationLog = new CalculationLog();
			calculationLog.CalculatorCode = "AAA";

			var logsWrapper = new CalculationLogsWrapper();
			logsWrapper.Logs.Add(calculationLog);

			CalculationLogsLoader.Save(jobCharge2, logsWrapper);
			AssertNull("LogsWrapper exists if there is one freight charge only", analyzer.LogsWrapper);

			jobCharge3.JR_AC = randomChargeCode.PK;
			AssertEquals("Calculation logs from the relevant job charge", "AAA", analyzer.LogsWrapper.Logs[0].CalculatorCode);
		}

		public void TestGetAmountInAWBCurrency()
		{
			ShipmentExportAWBHeaderForTesting header = Factory.New<ShipmentExportAWBHeaderForTesting>();
			header.EH_ParentID = Factory.New<ForwardingShipment>().PK;

			MockShipmentAnalyzer analyzer = new MockShipmentAnalyzer(header);

			header.MonetaryAmountParameter = null;
			analyzer.GetAmountInAWBCurrency(10m, "USD");
			AssertEquals(10m, header.MonetaryAmountParameter.Amount);
			AssertEquals("USD", header.MonetaryAmountParameter.Currency.Code);

			header.MonetaryAmountParameter = null;
			analyzer.GetAmountInAWBCurrency(20m, "NZD");
			AssertEquals(20m, header.MonetaryAmountParameter.Amount);
			AssertEquals("NZD", header.MonetaryAmountParameter.Currency.Code);

			header.MonetaryAmountParameter = null;
			analyzer.GetAmountInAWBCurrency(30m, "XXX");
			AssertNull("Not calling AWBHeader method when currency code is invalid", header.MonetaryAmountParameter);
		}

		#region TestGetAmountInAWBCurrency_WithDebtor

		public void TestGetAmountInAWBCurrency_WithDebtor()
		{
			var header = Factory.New<ShipmentExportAWBHeaderForTesting>();
			header.EH_ParentID = Factory.New<ForwardingShipment>().PK;
			var jobHeader = new JobHeader.Loader(header.Shipment).TryCreate();

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			CreateExchangeRate("USD", 2.5, "DEB", debtor.PK, jobHeader.PK);

			var freightChargeCode = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			AssertNotNull(freightChargeCode);
			CreateShipmentCharge(header.Shipment, freightChargeCode, 200, 400, debtor);

			var analyzer = new MockShipmentAnalyzer(header);

			header.MonetaryAmountParameter = null;
			AssertEquals(4m, analyzer.GetAmountInAWBCurrency(10m, "USD"));
			AssertEquals(10m, header.MonetaryAmountParameter.Amount);
			AssertEquals("USD", header.MonetaryAmountParameter.Currency.Code);
		}

		void CreateExchangeRate(string currencyCode, ZDecimal baseRate, string orgType, ZGuid orgHeaderPK, ZGuid jobHeaderPK)
		{
			var exRate = (BusinessObject)Factory.New<IExchangeRate>();
			exRate["JF_RX_NKRateCurrency"] = currencyCode;
			exRate["JF_BaseRate"] = baseRate;
			exRate["JF_OH_Org"] = orgHeaderPK;
			exRate["JF_OrgType"] = orgType;
			exRate["JF_JH"] = jobHeaderPK;
		}

		void CreateShipmentCharge(ForwardingShipment shipment,
			AccChargeCode chargeCode, ZDecimal sell, ZDecimal cost,
			OrgHeader sellAccount)
		{
			var charge = Factory.New<JobCharge>();
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_AC = chargeCode != null ? chargeCode.PK : ZGuid.Empty;
			charge.JR_JH = shipment.Job.PK;
			charge.JR_OH_SellAccount = sellAccount != null ? sellAccount.PK : ZGuid.Empty;
			charge.JR_LocalSellAmt = sell;
			charge.JR_OSSellAmt = sell;
			charge.JR_LocalCostAmt = cost;
			charge.JR_OSCostAmt = cost;
		}

		#endregion

		public void TestULDContainers()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var header = Factory.New<ShipmentExportAWBHeader>();
			header.EH_ParentID = shipment.PK;

			var analyzer = new MockShipmentAnalyzer(header);
			AssertEquals("Default", 0, header.ULDContainers.Count());

			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.Add(shipment);

			var uldContainer1 = consol.Containers.AddNew();
			uldContainer1.JC_ContainerMode = Core.Constants.ContainerModes.ULD;

			var uldContainer2 = consol.Containers.AddNew();
			uldContainer2.JC_ContainerMode = Core.Constants.ContainerModes.ULD;

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerMode = "XXX";

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.SetContainer(uldContainer1.PK);

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.SetContainer(uldContainer2.PK);

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.SetContainer(container3.PK);

			analyzer = new MockShipmentAnalyzer(header);
			AssertEquals(2, header.ULDContainers.Count());
			AssertContainsExactElementsInAnyOrder(new CommonContainer[] { uldContainer1, uldContainer2 }, header.ULDContainers);

			shipment.OuterPackLines.Remove(packline1);

			analyzer = new MockShipmentAnalyzer(header);
			AssertEquals(1, header.ULDContainers.Count());
			AssertContainsExactElementsInAnyOrder(new CommonContainer[] { uldContainer2 }, header.ULDContainers);

			AssertNoExceptionThrown("No exception when accessing ULDContainers while enumerating ULDContainers", () =>
				{
					foreach (CommonContainer container in header.ULDContainers)
					{
						int containersCount = header.ULDContainers.Count();
					}
				}
			);
		}

		public void TestGetNumberOfPieces()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			ShipmentExportAWBHeader header = Factory.New<ShipmentExportAWBHeader>();
			header.EH_ParentID = shipment.PK;

			MockShipmentAnalyzer analyzer = new MockShipmentAnalyzer(header);
			AssertEquals(0, analyzer.GetNumberOfPieces());

			shipment.JS_OuterPacks = 10;
			AssertEquals(10, analyzer.GetNumberOfPieces());

			shipment.JS_OuterPacks = 20;
			AssertEquals(20, analyzer.GetNumberOfPieces());
		}

		#region Implementation

		class MockShipmentAnalyzer : ShipmentCalculationLogsAnalyzer
		{
			public MockShipmentAnalyzer(ShipmentExportAWBHeader exportAWBHeader)
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

		class ShipmentExportAWBHeaderForTesting : ShipmentExportAWBHeader
		{
			public ShipmentExportAWBHeaderForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			internal override ZDecimal GetAmountInHAWBCurrency(Money initialMonetaryAmount, ZGuid orgPK, CostSell costOrSell)
			{
				MonetaryAmountParameter = initialMonetaryAmount;
				return base.GetAmountInHAWBCurrency(initialMonetaryAmount, orgPK, costOrSell);
			}
			public Money MonetaryAmountParameter;
		}

		#endregion
	}
}
