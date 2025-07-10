using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(WebShipmentTransport))]
	sealed class WebShipmentTransportTest : PersistentBusinessObjectTestCase
	{
		public void TestJW_LegOrder()
		{
			TrackingShipment shipment = GetNewTrackingShipment();
			Transport transport = shipment.Consols[0].Transports[0];
			transport.JW_LegOrder = 3;

			WebShipmentTransport webTransport = LoadTransport(shipment.PK);
			AssertEquals("JW_LegOrder", transport.JW_LegOrder, webTransport.JW_LegOrder);
		}

		public void TestJW_TransportMode()
		{
			TrackingShipment shipment = GetNewTrackingShipment();
			Transport transport = shipment.Consols[0].Transports[0];
			transport.JW_TransportMode = "AIR";

			WebShipmentTransport webTransport = LoadTransport(shipment.PK);
			AssertEquals("JW_TransportMode", transport.JW_TransportMode, webTransport.JW_TransportMode);
		}

		public void TestJW_TransportType()
		{
			TrackingShipment shipment = GetNewTrackingShipment();
			Transport transport = shipment.Consols[0].Transports[0];
			transport.JW_TransportType = "FL1";

			WebShipmentTransport webTransport = LoadTransport(shipment.PK);
			AssertEquals("JW_LegOrder", transport.JW_TransportType, webTransport.JW_TransportType);
		}

		public void TestJW_ETD()
		{
			TrackingShipment shipment = GetNewTrackingShipment();
			Transport transport = shipment.Consols[0].Transports[0];
			transport.JW_ETD = ZDateTime.Now.Date;

			WebShipmentTransport webTransport = LoadTransport(shipment.PK);
			AssertEquals("JW_ETD", transport.JW_ETD, webTransport.JW_ETD);
		}

		public void TestJW_ETA()
		{
			TrackingShipment shipment = GetNewTrackingShipment();
			Transport transport = shipment.Consols[0].Transports[0];
			transport.JW_ETA = ZDateTime.Now.Date;

			WebShipmentTransport webTransport = LoadTransport(shipment.PK);
			AssertEquals("JW_ETA", transport.JW_ETA, webTransport.JW_ETA);
		}

		public void TestJW_ATD()
		{
			TrackingShipment shipment = GetNewTrackingShipment();
			Transport transport = shipment.Consols[0].Transports[0];
			transport.JW_ATD = ZDateTime.Now.Date;

			WebShipmentTransport webTransport = LoadTransport(shipment.PK);
			AssertEquals("JW_ATD", transport.JW_ATD, webTransport.JW_ATD);
		}

		public void TestJW_ATA()
		{
			TrackingShipment shipment = GetNewTrackingShipment();
			Transport transport = shipment.Consols[0].Transports[0];
			transport.JW_ATA = ZDateTime.Now.Date;

			WebShipmentTransport webTransport = LoadTransport(shipment.PK);
			AssertEquals("JW_ATA", transport.JW_ATA, webTransport.JW_ATA);
		}

		public void TestJK_UniqueConsignRef()
		{
			TrackingShipment shipment = GetNewTrackingShipment();
			TrackingConsol consol = shipment.Consols[0];
			consol.JK_UniqueConsignRef = "123";

			WebShipmentTransport webTransport = LoadTransport(shipment.PK);
			AssertEquals("JK_UniqueConsignRef", consol.JK_UniqueConsignRef, webTransport.JK_UniqueConsignRef);
		}

		public void TestJK_MasterBillNum()
		{
			TrackingShipment shipment = GetNewTrackingShipment();
			TrackingConsol consol = shipment.Consols[0];
			consol.JK_MasterBillNum = "12345";

			WebShipmentTransport webTransport = LoadTransport(shipment.PK);
			AssertEquals("JK_MasterBillNum", consol.JK_MasterBillNum, webTransport.JK_MasterBillNum);
		}

		public void TestJK_OA_SendingForwarderAddress()
		{
			TrackingShipment shipment = GetNewTrackingShipment();
			TrackingConsol consol = shipment.Consols[0];
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			consol.SetDefaultSendingForwarderAddress(org);

			WebShipmentTransport webTransport = LoadTransport(shipment.PK);
			AssertEquals("JK_OA_SendingForwarderAddress", consol.JK_OA_SendingForwarderAddress, webTransport.JK_OA_SendingForwarderAddress);
		}

		public void TestJK_OA_ReceivingForwarderAddress()
		{
			TrackingShipment shipment = GetNewTrackingShipment();
			TrackingConsol consol = shipment.Consols[0];
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			consol.SetDefaultReceivingForwarderAddress(org);

			WebShipmentTransport webTransport = LoadTransport(shipment.PK);
			AssertEquals("JK_OA_ReceivingForwarderAddress", consol.JK_OA_ReceivingForwarderAddress, webTransport.JK_OA_ReceivingForwarderAddress);
		}

		public void TestJA_RL_NKPortOfLoading()
		{
			TrackingShipment shipment = GetNewTrackingShipment();
			RefUNLOCO port = Factory.NewWithValidTestData<RefUNLOCO>();
			Transport transport = shipment.Consols[0].Transports[0];
			JobSailing sailing = Factory.NewWithValidTestData<JobSailing>();
			transport.JW_JX = sailing.PK;
			sailing.Origin.JA_RL_NKPortOfLoading = port.RL_Code;

			WebShipmentTransport webTransport = LoadTransport(shipment.PK);
			AssertEquals("JA_RL_NKPortOfLoading", port.RL_Code, webTransport.JA_RL_NKPortOfLoading);
		}

		public void TestJB_RL_NKPortOfDischarge()
		{
			TrackingShipment shipment = GetNewTrackingShipment();
			RefUNLOCO port = Factory.NewWithValidTestData<RefUNLOCO>();
			Transport transport = shipment.Consols[0].Transports[0];
			JobSailing sailing = Factory.NewWithValidTestData<JobSailing>();
			transport.JW_JX = sailing.PK;
			sailing.Destination.JB_RL_NKPortOfDischarge = port.RL_Code;

			WebShipmentTransport webTransport = LoadTransport(shipment.PK);
			AssertEquals("JB_RL_NKPortOfDischarge", port.RL_Code, webTransport.JB_RL_NKPortOfDischarge);
		}

		public void TestJV_RV_NKVessel()
		{
			TrackingShipment shipment = GetNewTrackingShipment();
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "123";
			Transport transport = shipment.Consols[0].Transports[0];
			transport.JW_Vessel = vessel.RV_Code;

			WebShipmentTransport webTransport = LoadTransport(shipment.PK);
			AssertEquals("JV_RV_NKVessel", transport.JW_Vessel, webTransport.JV_RV_NKVessel);
		}

		public void TestJV_VoyageFlight()
		{
			TrackingShipment shipment = GetNewTrackingShipment();
			Transport transport = shipment.Consols[0].Transports[0];
			transport.JW_VoyageFlight = "AU123";

			WebShipmentTransport webTransport = LoadTransport(shipment.PK);
			AssertEquals("JV_VoyageFlight", transport.JW_VoyageFlight, webTransport.JV_VoyageFlight);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return LoadTransport(GetNewTrackingShipment().PK);
		}

		protected override bool IsDeleteSupported()
		{
			return false;
		}

		protected override bool CanPersistedObjectBeDeleted
		{
			get { return false; }
		}

		TrackingShipment GetNewTrackingShipment()
		{
			TrackingShipment result = Factory.New<TrackingShipment>();

			TrackingConsol consol = result.Consols.AddNew();
			AssertEquals("Should be one consol", 1, result.Consols.Count);
			AssertEquals("Should be one transport on consol", 1, result.Consols[0].Transports.Count);

			return result;
		}

		WebShipmentTransport LoadTransport(ZGuid shipmentPK)
		{
			Factory.Save();

			WebShipmentTransportCollection collection = new WebShipmentTransportCollection(Factory, shipmentPK);
			AssertEquals("Should be no Transports", 0, collection.Count);

			collection.Load();
			AssertEquals("Should be one Transport", 1, collection.Count);

			return collection[0];
		}
		#endregion
	}
}
