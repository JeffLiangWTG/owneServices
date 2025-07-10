using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ConsolChargeableCalculationHelperTest : TestCaseWithFactory
	{
		public void TestCalculateConsolChargeables()
		{
			Consol.JK_OverrideConsolChargeable = true;
			Consol.JK_ConsolChargeable = 100m;
			Factory.Save();
			var allResult = Helper.CalculateChargeables(new HashSet<ZString>() { "TESTCONSOL" }, weightUnit, volumeUnit);
			AssertEquals("If JK_OverrideConsolChargeable is True, CalculateChargeables return JK_ConsolChargeable", 100m, allResult["TESTCONSOL"]);

			Consol.JK_OverrideConsolChargeable = false;
			Factory.Save();
			allResult = Helper.CalculateChargeables(new HashSet<ZString>() { "TESTCONSOL" }, weightUnit, volumeUnit);
			AssertEquals("Precondition: chargeable calculated by consol business object", 933.333m, Consol.JK_ConsolChargeable);
			AssertEquals("Chargeable calculated by helper", 933.333m, allResult["TESTCONSOL"]);

			Consol.Shipments[0].JS_LoadingMeters = 2m;
			Consol.Shipments[1].JS_LoadingMeters = 8m;
			Factory.Save();
			allResult = Helper.CalculateChargeables(new HashSet<ZString>() { "TESTCONSOL" }, weightUnit, volumeUnit);
			AssertEquals("Not a ROAD consol => loading meters not used", 933.333m, allResult["TESTCONSOL"]);

			FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.RoadLoadingMetersWeightPerLDM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1024m);

			Consol.JK_TransportMode = Constants.TransportModes.Road;
			Factory.Save();
			allResult = Helper.CalculateChargeables(new HashSet<ZString>() { "TESTCONSOL" }, weightUnit, volumeUnit);
			AssertEquals("Precondition: chargeable calculated by consol business object, loading meters used for calculation", 10240m, Consol.JK_ConsolChargeable);
			AssertEquals("Chargeable calculated by helper", 10240m, allResult["TESTCONSOL"]);
		}

		public void TestNoColoadOrCancelledShipments()
		{
			CommonShipment ship3 = Consol.Shipments.AddNew();
			ship3.JS_ActualWeight = 520;
			ship3.JS_UnitOfWeight = "KG";
			ship3.JS_ActualVolume = 2.8;
			ship3.JS_UnitOfVolume = "M3";
			ship3.JS_JS_ColoadMasterShipment = Consol.Shipments[0].PK;

			Factory.Save();
			var allResult = Helper.CalculateChargeables(new HashSet<ZString>() { "TESTCONSOL" }, weightUnit, volumeUnit);
			AssertEquals("Coloaded shipments does not counts", 933.333m, allResult["TESTCONSOL"]);

			CommonShipment ship4 = Consol.Shipments.AddNew();
			ship4.JS_ActualWeight = 520;
			ship4.JS_UnitOfWeight = "KG";
			ship4.JS_ActualVolume = 2.8;
			ship4.JS_UnitOfVolume = "M3";
			ship4.JS_IsCancelled = true;

			Factory.Save();
			allResult = Helper.CalculateChargeables(new HashSet<ZString>() { "TESTCONSOL" }, weightUnit, volumeUnit);
			AssertEquals("Cancelled shipments does not counts", 933.333m, allResult["TESTCONSOL"]);
		}

		CommonConsol Consol;
		ZString weightUnit;
		ZString volumeUnit;

		protected override void SetUp()
		{
			base.SetUp();

			weightUnit = Env.Registry.FreightWeightUnit;
			volumeUnit = Env.Registry.FreightVolumeUnit;

			Consol = Factory.NewWithValidTestData<CommonConsol>();
			CommonShipment shipment1 = Consol.Shipments.AddNew();
			CommonShipment shipment2 = Consol.Shipments.AddNew();

			Consol.JK_UniqueConsignRef = "TESTCONSOL";
			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.JK_RL_NKLoadPort = "NZAKL";
			Consol.JK_RL_NKDischargePort = "AUSYD";

			shipment1.JS_ActualWeight = 520;
			shipment1.JS_UnitOfWeight = "KG";
			shipment1.JS_ActualVolume = 2.8;
			shipment1.JS_UnitOfVolume = "M3";

			shipment2.JS_ActualWeight = 300;
			shipment2.JS_UnitOfWeight = "KG";
			shipment2.JS_ActualVolume = 2.8;
			shipment2.JS_UnitOfVolume = "M3";
			shipment2.JS_ActualChargeable = 466.667m;
		}

		ConsolChargeableCalculationHelper Helper
		{
			get { return fHelper ?? (fHelper = new ConsolChargeableCalculationHelper()); }
		}
		ConsolChargeableCalculationHelper fHelper;
	}
}
