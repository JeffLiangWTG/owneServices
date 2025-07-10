using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonConsolComparisonHelperTest : TestCaseWithFactory
	{
		public void TestCompareConsolsLoadAndDischargePorts_ShouldReturnDifferentDischargeAndLoadPort_WhenBothDischargePortAndLoadPortsAreDifferent()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_UniqueConsignRef = "C0000000";
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.Consols.Add(consol);

			Factory.Save();

			var anotherConsol = Factory.NewWithValidTestData<CommonConsol>();
			anotherConsol.JK_RL_NKLoadPort = "SGSIN";
			anotherConsol.JK_RL_NKDischargePort = "NZAKL";
			anotherConsol.JK_UniqueConsignRef = "C0000001";
			anotherConsol.JK_TransportMode = Constants.TransportModes.Sea;

			AssertEquals(CommonConsolComparisonHelper.ConsolsPortComparisonResult.DifferentDischargeAndLoadPort,
				CommonConsolComparisonHelper.CompareConsolsLoadAndDischargePorts(consol, anotherConsol));
		}

		public void TestCompareConsolsLoadAndDischargePorts_ShouldReturnHasSameLoadPort_WhenLoadPortsAreTheSameAndBothTransportModesAreNotRailOrRoad()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_UniqueConsignRef = "C0000000";
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.Consols.Add(consol);

			Factory.Save();

			var anotherConsol = Factory.NewWithValidTestData<CommonConsol>();
			anotherConsol.JK_RL_NKLoadPort = "AUSYD";
			anotherConsol.JK_RL_NKDischargePort = "NZAKL";
			anotherConsol.JK_UniqueConsignRef = "C0000001";
			anotherConsol.JK_TransportMode = Constants.TransportModes.Sea;

			AssertEquals(CommonConsolComparisonHelper.ConsolsPortComparisonResult.HasSameLoadPort,
				CommonConsolComparisonHelper.CompareConsolsLoadAndDischargePorts(consol, anotherConsol));
		}

		public void TestCompareConsolsLoadAndDischargePorts_ShouldReturnHasSameLoadPort_WhenLoadPortsAreTheSameAndTransportModeIsRailOrRoadAndIsNotDomestic()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_UniqueConsignRef = "C0000000";
			consol.JK_TransportMode = Constants.TransportModes.Road;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FTL;
			consol.IsDomesticFreight = false;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.Consols.Add(consol);

			Factory.Save();

			var anotherConsol = Factory.NewWithValidTestData<CommonConsol>();
			anotherConsol.JK_RL_NKLoadPort = "AUSYD";
			anotherConsol.JK_RL_NKDischargePort = "NZAKL";
			anotherConsol.JK_UniqueConsignRef = "C0000001";
			anotherConsol.JK_TransportMode = Constants.TransportModes.Rail;
			anotherConsol.JK_ConsolMode = Constants.ContainerModes.Bulk;
			anotherConsol.IsDomesticFreight = false;

			AssertEquals(CommonConsolComparisonHelper.ConsolsPortComparisonResult.HasSameLoadPort,
				CommonConsolComparisonHelper.CompareConsolsLoadAndDischargePorts(consol, anotherConsol));
		}

		public void TestCompareConsolsLoadAndDischargePorts_ShouldReturnAcceptableSameLoadOrDischargePort_WhenOnlyDischargePortsAreTheSameAndTransportModeIsRailOrRoadAndIsDomestic()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_UniqueConsignRef = "C0000000";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.IsDomesticFreight = false;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.Consols.Add(consol);

			Factory.Save();

			var anotherConsol = Factory.NewWithValidTestData<CommonConsol>();
			anotherConsol.JK_RL_NKLoadPort = "AUSYD";
			anotherConsol.JK_RL_NKDischargePort = "AUMEL";
			anotherConsol.JK_UniqueConsignRef = "C0000001";
			anotherConsol.JK_TransportMode = Constants.TransportModes.Rail;
			anotherConsol.JK_ConsolMode = Constants.ContainerModes.Bulk;
			anotherConsol.IsDomesticFreight = true;

			AssertEquals(CommonConsolComparisonHelper.ConsolsPortComparisonResult.AcceptableSameLoadOrDischargePort,
				CommonConsolComparisonHelper.CompareConsolsLoadAndDischargePorts(consol, anotherConsol));
		}

		public void TestCompareConsolsLoadAndDischargePorts_ShouldReturnHasSameLoadPort_WhenOnlyLoadPortsAreTheSameAndTransportModeIsRailOrRoadAndIsDomesticAndIgnoreDomesticIsFalse()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_UniqueConsignRef = "C0000000";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.IsDomesticFreight = false;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.Consols.Add(consol);

			Factory.Save();

			var anotherConsol = Factory.NewWithValidTestData<CommonConsol>();
			anotherConsol.JK_RL_NKLoadPort = "AUSYD";
			anotherConsol.JK_RL_NKDischargePort = "AUMEL";
			anotherConsol.JK_UniqueConsignRef = "C0000001";
			anotherConsol.JK_TransportMode = Constants.TransportModes.Rail;
			anotherConsol.JK_ConsolMode = Constants.ContainerModes.Bulk;
			anotherConsol.IsDomesticFreight = true;

			AssertEquals(CommonConsolComparisonHelper.ConsolsPortComparisonResult.HasSameLoadPort,
				CommonConsolComparisonHelper.CompareConsolsLoadAndDischargePorts(consol, anotherConsol, false));
		}

		public void TestCompareConsolsLoadAndDischargePorts_ShouldReturnInOnePortWithTimeOverlap_WhenBothAreInOnePortAndTransportModeIsRailOrRoadAndIsDomesticAndTheyHaveOverlapATDATA()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_UniqueConsignRef = "C0000000";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Now.AddDays(-2);
			consol.Transports.MostInterestingTransport.JW_ATA = ZDateTime.Now;
			consol.IsDomesticFreight = false;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.Consols.Add(consol);

			Factory.Save();

			var anotherConsol = Factory.NewWithValidTestData<CommonConsol>();
			anotherConsol.JK_RL_NKLoadPort = "AUSYD";
			anotherConsol.JK_RL_NKDischargePort = "AUSYD";
			anotherConsol.JK_UniqueConsignRef = "C0000001";
			anotherConsol.JK_TransportMode = Constants.TransportModes.Rail;
			anotherConsol.JK_ConsolMode = Constants.ContainerModes.Bulk;
			anotherConsol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Now.AddDays(-1);
			anotherConsol.Transports.MostInterestingTransport.JW_ATA = ZDateTime.Now;
			anotherConsol.IsDomesticFreight = true;

			AssertEquals(CommonConsolComparisonHelper.ConsolsPortComparisonResult.InOnePortWithTimeOverlap,
				CommonConsolComparisonHelper.CompareConsolsLoadAndDischargePorts(consol, anotherConsol));
		}

		public void TestCompareConsolsLoadAndDischargePorts_ShouldReturnAcceptableSameLoadOrDischargePort_WhenBothAreInOnePortAndTransportModeIsRailOrRoadAndIsDomesticAndTheyDontHaveOverlap()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_UniqueConsignRef = "C0000000";
			consol.JK_TransportMode = Constants.TransportModes.Rail;
			consol.JK_ConsolMode = Constants.ContainerModes.Bulk;
			consol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Now.AddDays(-4);
			consol.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Now.AddDays(-3);
			consol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Now.AddDays(-4);
			consol.Transports.MostInterestingTransport.JW_ATA = ZDateTime.Now.AddDays(-2);
			consol.IsDomesticFreight = false;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.Consols.Add(consol);

			Factory.Save();

			var anotherConsol = Factory.NewWithValidTestData<CommonConsol>();
			anotherConsol.JK_RL_NKLoadPort = "AUSYD";
			anotherConsol.JK_RL_NKDischargePort = "AUSYD";
			anotherConsol.JK_UniqueConsignRef = "C0000001";
			anotherConsol.JK_TransportMode = Constants.TransportModes.Rail;
			anotherConsol.JK_ConsolMode = Constants.ContainerModes.Bulk;
			anotherConsol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Now.AddDays(-2);
			anotherConsol.Transports.MostInterestingTransport.JW_ATA = ZDateTime.Now.AddDays(-1);
			anotherConsol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Now.AddDays(-1);
			anotherConsol.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Now;
			anotherConsol.IsDomesticFreight = true;

			AssertEquals(CommonConsolComparisonHelper.ConsolsPortComparisonResult.AcceptableSameLoadOrDischargePort,
				CommonConsolComparisonHelper.CompareConsolsLoadAndDischargePorts(consol, anotherConsol));
		}
	}
}
