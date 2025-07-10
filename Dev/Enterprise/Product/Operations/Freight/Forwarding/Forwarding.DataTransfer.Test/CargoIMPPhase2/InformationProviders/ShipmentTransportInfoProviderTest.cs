using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class ShipmentTransportInfoProviderTest : TestCaseWithFactory
	{
		public void TestDepartureLocation()
		{
			ShipmentTransportInfoProvider provider = new ShipmentTransportInfoProvider(shipment, transport);

			transport.JW_RL_NKLoadPort = "AUSYD";
			AssertEquals("SYD", provider.DepartureLocation.Value);
			AssertEquals(transport.JW_RL_NKLoadPortInfo.HumanReadableName, provider.DepartureLocation.Name);
			AssertEquals(transport, provider.DepartureLocation.BusinessEntity);

			transport.JW_RL_NKLoadPort = "AUMEL";
			AssertEquals("MEL", provider.DepartureLocation.Value);
		}

		public void TestVoyageMode()
		{
			ShipmentTransportInfoProvider provider = new ShipmentTransportInfoProvider(shipment, transport);

			transport.JW_TransportMode = Constants.TransportModes.Air;
			AssertEquals(VoyageModes.Air, provider.VoyageMode.Value);
			AssertEquals(transport.JW_TransportModeInfo.HumanReadableName, provider.VoyageMode.Name);
			AssertEquals(transport, provider.VoyageMode.BusinessEntity);

			transport.JW_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(VoyageModes.Sea, provider.VoyageMode.Value);
		}

		public void TestArrivalLocation()
		{
			ShipmentTransportInfoProvider provider = new ShipmentTransportInfoProvider(shipment, transport);

			transport.JW_RL_NKDiscPort = "AUSYD";
			AssertEquals("SYD", provider.ArrivalLocation.Value);
			AssertEquals(transport.JW_RL_NKDiscPortInfo.HumanReadableName, provider.ArrivalLocation.Name);
			AssertEquals(transport, provider.ArrivalLocation.BusinessEntity);

			transport.JW_RL_NKDiscPort = "AUMEL";
			AssertEquals("MEL", provider.ArrivalLocation.Value);
		}

		public void TestETD()
		{
			ShipmentTransportInfoProvider provider = new ShipmentTransportInfoProvider(shipment, transport);

			transport.JW_ETD = ZDateTime.Now;
			AssertEquals(transport.JW_ETD, provider.ETD.Value);
			AssertEquals(transport.JW_ETDInfo.HumanReadableName, provider.ETD.Name);
			AssertEquals(transport, provider.ETD.BusinessEntity);

			transport.JW_ETD = transport.JW_ETD.AddDays(1);
			AssertEquals(transport.JW_ETD, provider.ETD.Value);
		}

		public void TestETA()
		{
			ShipmentTransportInfoProvider provider = new ShipmentTransportInfoProvider(shipment, transport);

			transport.JW_ETA = ZDateTime.Now;
			AssertEquals(transport.JW_ETA, provider.ETA.Value);
			AssertEquals(transport.JW_ETAInfo.HumanReadableName, provider.ETA.Name);
			AssertEquals(transport, provider.ETA.BusinessEntity);

			transport.JW_ETA = transport.JW_ETA.AddDays(1);
			AssertEquals(transport.JW_ETA, provider.ETA.Value);
		}

		public void TestMasterBillNumber()
		{
			ShipmentTransportInfoProvider provider = new ShipmentTransportInfoProvider(shipment, transport);

			consol.JK_TransportMode = Constants.TransportModes.Air;
			transport.JW_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "027123456";
			AssertEquals("027-123456", provider.MasterBillNumber.Value);
			AssertEquals(consol.JK_MasterBillNumInfo.HumanReadableName, provider.MasterBillNumber.Name);
			AssertEquals(consol, provider.MasterBillNumber.BusinessEntity);

			transport.JW_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(ZString.Empty, provider.MasterBillNumber.Value);
			AssertEquals("Master Bill Number", provider.MasterBillNumber.Name);

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportMode = Constants.TransportModes.Air;
			AssertEquals(ZString.Empty, provider.MasterBillNumber.Value);
			AssertEquals("Master Bill Number", provider.MasterBillNumber.Name);

			consol.JK_TransportMode = Constants.TransportModes.Air;
			transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Air;
			provider = new ShipmentTransportInfoProvider(shipment, transport);
			AssertEquals(ZString.Empty, provider.MasterBillNumber.Value);
			AssertEquals("Master Bill Number", provider.MasterBillNumber.Name);
		}

		public void TestCarrierCode()
		{
			ShipmentTransportInfoProvider provider = new ShipmentTransportInfoProvider(shipment, transport);

			AssertEquals(ZString.Empty, provider.CarrierCode.Value);
			AssertEquals("Carrier Code", provider.CarrierCode.Name);
			AssertEquals(null, provider.CarrierCode.BusinessEntity);
		}

		protected override void SetUp()
		{
			consol = Factory.New<ForwardingConsol>();
			shipment = Factory.New<ForwardingShipment>();
			shipment.Consols.Add(consol);
			transport = consol.Transports.AddNew();
		}

		ForwardingConsol consol;
		ForwardingShipment shipment;
		Transport transport;
	}
}
