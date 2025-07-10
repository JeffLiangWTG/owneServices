using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class GatePassShipmentJobDateProviderTest : TestCaseWithFactory
	{
		[TestDate(2014, 5, 11)]
		public void TestArrivalDate()
		{
			var gatePassShipment = Factory.NewWithValidTestData<GatePassShipment>();
			var gatePassShipmentJobDateProvider = new GatePassShipmentJobDateProvider(gatePassShipment);

			AssertEquals(ZDateTime.Today, gatePassShipmentJobDateProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

			var pack = gatePassShipment.OuterPackLines.AddNew();
			pack.JL_PackageCount = 20;
			pack.JL_Outturn = 15;
			pack.JL_F3_NKPackType = Core.Constants.PkgUnit.Box;

			var container = Factory.New<GatePassContainer>();
			container.JC_JX = gatePassShipment.JS_JX;
			pack.SetContainer(container.PK);
			container.JC_LCLAvailable = ZDateTime.Today.AddDays(-3);
			container.JC_LCLStorageCommences = ZDateTime.Today;

			var commonPickupDeliveryConfirm = gatePassShipment.DestinationCFSDepartures.AddNew();
			commonPickupDeliveryConfirm.EU_PickupDeliveryTime = ZDateTime.Today.AddDays(-1);

			AssertEquals(ZDateTime.Today.AddDays(-1), gatePassShipmentJobDateProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

			commonPickupDeliveryConfirm = gatePassShipment.DestinationCFSDepartures.AddNew();
			commonPickupDeliveryConfirm.EU_PickupDeliveryTime = ZDateTime.Today.AddDays(-2);

			AssertEquals(ZDateTime.Today.AddDays(-2), gatePassShipmentJobDateProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		[TestDate(2014, 5, 11)]
		public void TestDepartureDate()
		{
			var gatePassShipment = Factory.New<GatePassShipment>();
			var gatePassShipmentJobDateProvider = new GatePassShipmentJobDateProvider(gatePassShipment);

			AssertEquals(new ZDateTime(2014, 5, 11), gatePassShipmentJobDateProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));

			var pack = gatePassShipment.OuterPackLines.AddNew();
			pack.JL_PackageCount = 20;
			pack.JL_Outturn = 15;
			pack.JL_F3_NKPackType = Core.Constants.PkgUnit.Box;

			var container = Factory.New<GatePassContainer>();
			container.JC_JX = gatePassShipment.JS_JX;
			pack.SetContainer(container.PK);
			container.JC_LCLAvailable = ZDateTime.Today.AddDays(-3);
			container.JC_LCLStorageCommences = ZDateTime.Today;

			var commonPickupDeliveryConfirm = gatePassShipment.DestinationCFSDepartures.AddNew();
			commonPickupDeliveryConfirm.EU_PickupDeliveryTime = ZDateTime.Today.AddDays(-1);

			AssertEquals(ZDateTime.Today.AddDays(-1), gatePassShipmentJobDateProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));

			commonPickupDeliveryConfirm = gatePassShipment.DestinationCFSDepartures.AddNew();
			commonPickupDeliveryConfirm.EU_PickupDeliveryTime = ZDateTime.Today.AddDays(-2);

			AssertEquals(ZDateTime.Today.AddDays(-2), gatePassShipmentJobDateProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
		}

		public void TestJobOpenDate()
		{
			var gatePassShipment = Factory.New<GatePassShipment>();
			var gatePassShipmentJobDateProvider = new GatePassShipmentJobDateProvider(gatePassShipment);

			AssertEquals(ZDateTime.Empty, gatePassShipmentJobDateProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));

			var dateToTest = ZDateTime.Today;
			var jobHeader = new JobHeader.Loader(gatePassShipment).TryLoadOrCreate();
			jobHeader.JH_A_JOP = dateToTest;

			AssertEquals(dateToTest, gatePassShipmentJobDateProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}

		public void TestFirstContainerGateInDate()
		{
			var dateToTest = ZDateTime.Today;

			var consol = Factory.New<GatePassLoadListConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "UCR001";
			shipment.JS_HouseBill = "HB001";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var container1 = consol.Containers.AddNew();
			container1.JC_FCLWharfGateIn = dateToTest;

			var container2 = consol.Containers.AddNew();
			container2.JC_FCLWharfGateIn = dateToTest.AddDays(8);

			var container3 = consol.Containers.AddNew();
			container3.JC_FCLWharfGateIn = ZDateTime.Empty;

			var jobDatesProvider = new GatePassShipmentJobDateProvider(shipment);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.FirstContainerGateInDate));

			shipment.OuterPackLines.AddNew().Containers.Add(container1);
			shipment.OuterPackLines.AddNew().Containers.Add(container2);

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.FirstContainerGateInDate));
		}

		public void TestLastContainerGateInDate()
		{
			var dateToTest = ZDateTime.Today.AddDays(2);

			var consol = Factory.New<GatePassLoadListConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var gatePassShipment = consol.Shipments.AddNew();
			gatePassShipment.JS_UniqueConsignRef = "UCR001";
			gatePassShipment.JS_HouseBill = "HB001";
			gatePassShipment.JS_RL_NKOrigin = "AUSYD";
			gatePassShipment.JS_RL_NKDestination = "NZAKL";

			var container1 = consol.Containers.AddNew();
			container1.JC_FCLWharfGateIn = dateToTest;

			var container2 = consol.Containers.AddNew();
			container2.JC_FCLWharfGateIn = ZDateTime.Empty;

			var container3 = consol.Containers.AddNew();
			container3.JC_FCLWharfGateIn = dateToTest.AddDays(-5);

			var jobDatesProvider = new GatePassShipmentJobDateProvider(gatePassShipment);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.LastContainerGateInDate));

			gatePassShipment.OuterPackLines.AddNew().Containers.Add(container1);
			gatePassShipment.OuterPackLines.AddNew().Containers.Add(container2);

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.LastContainerGateInDate));
		}
	}
}
