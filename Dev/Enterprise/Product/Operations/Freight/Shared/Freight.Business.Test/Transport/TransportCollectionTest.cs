using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TransportCollectionTest : TestCaseWithFactory
	{
		public void TestResetParentScreeningStatus_ParentIsJobClear()
		{
			var consol = Factory.New<IForwardingConsol>();
			((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var transport = consol.Transports_Get(0) as Transport;
			transport.JW_JX = sailing.PK;
			transport.JW_Vessel = "VESS";
			Factory.Save();

			(transport as IShouldUpdateScreeningStatus).ShouldUpdateScreeningStatus = true;
			transport.JW_IsLinked = true;
			UpdateVesselUnderTransport(vessel, transport, ScreeningStatusesList.Codes.Clear);
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			UpdateVesselUnderTransport(vessel, transport, ScreeningStatusesList.Codes.PermanentClear);
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			UpdateVesselUnderTransport(vessel, transport, ScreeningStatusesList.Codes.JobCleared);
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			UpdateVesselUnderTransport(vessel, transport, ScreeningStatusesList.Codes.Matched);
			AssertEquals(true, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
			UpdateVesselUnderTransport(vessel, transport, ScreeningStatusesList.Codes.Unknown);
			AssertEquals(true, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
			UpdateVesselUnderTransport(vessel, transport, ScreeningStatusesList.Codes.NotScreened);
			AssertEquals(true, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
			UpdateVesselUnderTransport(vessel, transport, ScreeningStatusesList.Codes.Canceled);
			AssertEquals(true, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			transport.JW_IsLinked = false;
			Factory.Save();
			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			(transport as IShouldUpdateScreeningStatus).ShouldUpdateScreeningStatus = true;

			transport.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			transport.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			transport.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			transport.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.Matched;
			AssertEquals(true, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
			transport.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			AssertEquals(true, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
			transport.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			AssertEquals(true, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);
		}

		public void TestResetParentScreeningStatus_TransportCodeEmpty()
		{
			var consol = Factory.New<IForwardingConsol>();
			((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var transport = consol.Transports_Get(0) as Transport;
			transport.JW_JX = sailing.PK;
			Factory.Save();

			AssertEquals("Pre-Condition: Transport Vessel Code is empty", string.Empty, transport.JW_Vessel);
			transport.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			(transport as IShouldUpdateScreeningStatus).ShouldUpdateScreeningStatus = true;
			UpdateVesselUnderTransport(vessel, transport, ScreeningStatusesList.Codes.Clear, true);
			transport.JW_IsLinked = true;
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			UpdateVesselUnderTransport(vessel, transport, ScreeningStatusesList.Codes.PermanentClear, true);
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			UpdateVesselUnderTransport(vessel, transport, ScreeningStatusesList.Codes.JobCleared, true);
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			UpdateVesselUnderTransport(vessel, transport, ScreeningStatusesList.Codes.Matched, true);
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
			UpdateVesselUnderTransport(vessel, transport, ScreeningStatusesList.Codes.Unknown, true);
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
			UpdateVesselUnderTransport(vessel, transport, ScreeningStatusesList.Codes.NotScreened, true);
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
			UpdateVesselUnderTransport(vessel, transport, ScreeningStatusesList.Codes.Canceled, true);
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			transport.JW_IsLinked = false;
			Factory.Save();
			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			(transport as IShouldUpdateScreeningStatus).ShouldUpdateScreeningStatus = true;

			transport.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			transport.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			transport.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			transport.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.Matched;
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
			transport.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);

			((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
			transport.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);
		}

		public void TestNewTransportRowDoesntPickUpInvalidTransportMode()
		{
			Consol.JK_TransportMode = "!_!";
			Transport transport = Consol.Transports.AddNew();
			AssertEquals("transport.JW_TransportMode", ZString.Empty, transport.JW_TransportMode);

			Consol.JK_TransportMode = "SEA";
			Transport transport2 = Consol.Transports.AddNew();
			AssertEquals("transport2.JW_TransportMode", "SEA", transport2.JW_TransportMode);
		}

		public void TestNewTransportRowShouldHaveStatusPLNForAirMode()
		{
			Consol.JK_TransportMode = "AIR";
			Transport transport = Consol.Transports.AddNew();
			AssertEquals("transport.JW_Status", "PLN", transport.JW_Status);
		}

		public void TestFindTransportByLoadPort()
		{
			AssertEquals(Transport1, Consol.Transports.FindByLoadPort("SGSIN"));
			AssertEquals(Transport2, Consol.Transports.FindByLoadPort("MYPKG"));
		}

		public void TestFindTransportByDischargePort()
		{
			AssertEquals(Transport1, Consol.Transports.FindByDischargePort("MYPKG"));
			AssertEquals(Transport2, Consol.Transports.FindByDischargePort("AUSYD"));
		}

		public void TestIsAnyDischargeInCountry()
		{
			TransportCollection transportCollection = Factory.New<CommonShipment>().Transports;
			Transport transport1 = transportCollection.AddNew();
			transport1.JW_RL_NKDiscPort = "CATOR";
			Transport transport2 = transportCollection.AddNew();
			transport2.JW_RL_NKDiscPort = "USNYK";
			Transport transport3 = transportCollection.AddNew();
			transport3.JW_RL_NKDiscPort = "AUSYD";
			Assert(transportCollection.IsAnyDischargeInCountry("CA"));
			Assert(transportCollection.IsAnyDischargeInCountry("US"));
			Assert(transportCollection.IsAnyDischargeInCountry("AU"));
			Assert(!transportCollection.IsAnyDischargeInCountry("GB"));
		}

		public void TestIsAnyLoadPortInCountry()
		{
			TransportCollection transportCollection = Factory.New<CommonShipment>().Transports;
			Transport transport1 = transportCollection.AddNew();
			transport1.JW_RL_NKLoadPort = "CATOR";
			Transport transport2 = transportCollection.AddNew();
			transport2.JW_RL_NKLoadPort = "USNYK";
			Transport transport3 = transportCollection.AddNew();
			transport3.JW_RL_NKLoadPort = "AUSYD";
			Assert(transportCollection.IsAnyLoadInCountry("CA"));
			Assert(transportCollection.IsAnyLoadInCountry("US"));
			Assert(transportCollection.IsAnyLoadInCountry("AU"));
			Assert(!transportCollection.IsAnyLoadInCountry("GB"));
		}

		public void TestHasIcs2ZoneDischarge()
		{
			SetupNorthernIrelandZone();

			List<string> ics2Zones = new List<string> { "GBBEL", "NOABE", "CHARF", "DEHAM" };
			foreach (var ics2Zone in ics2Zones)
			{
				var transportCollection = Factory.New<CommonShipment>().Transports;
				var transport1 = transportCollection.AddNew();
				transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport1.JW_RL_NKDiscPort = "CATOR";
				var transport2 = transportCollection.AddNew();
				transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport2.JW_RL_NKDiscPort = ics2Zone;
				var transport3 = transportCollection.AddNew();
				transport3.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport3.JW_RL_NKDiscPort = "AUSYD";

				Assert(transportCollection.HasIcs2ZoneAirDischarge);

				transport2.JW_RL_NKDiscPort = "USLAX";
				Assert(!transportCollection.HasIcs2ZoneAirDischarge);
			}

			void SetupNorthernIrelandZone()
			{
				var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
				if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
				{
					var ni = Factory.New<RefCountryStates>();
					belfast.RL_RW = ni.PK;
					ni.RW_RegionName = "NORTHERN IRELAND";
				}
			}
		}

		public void TestHasLoadPortOutsideOfIcs2MemberCountry()
		{
			var collection = Factory.New<CommonShipment>().Transports;

			var transport1 = collection.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "DEHAM";
			transport1.JW_RL_NKDiscPort = "ITSPE";
			Assert(!collection.HasLoadPortOutsideOfIcs2Zone);

			var transport2 = collection.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "DEHAM";
			Assert(!collection.HasLoadPortOutsideOfIcs2Zone);

			var transport3 = collection.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport3.JW_RL_NKLoadPort = "USLAX";
			transport3.JW_RL_NKDiscPort = "AUSYD";
			Assert(collection.HasLoadPortOutsideOfIcs2Zone);
		}

		public void TestIsAirImportToICS2Member()
		{
			var collection = Factory.New<CommonShipment>().Transports;

			var transport1 = collection.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "DEHAM";
			transport1.JW_RL_NKDiscPort = "ITSPE";
			Assert(!collection.IsAirImportOrTransitToICS2Zone);

			var transport2 = collection.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "DEHAM";
			Assert(!collection.IsAirImportOrTransitToICS2Zone);

			var transport3 = collection.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport3.JW_RL_NKLoadPort = "USLAX";
			transport3.JW_RL_NKDiscPort = "AUSYD";
			Assert(!collection.IsAirImportOrTransitToICS2Zone);

			var transport4 = collection.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport4.JW_RL_NKLoadPort = "USLAX";
			transport4.JW_RL_NKDiscPort = "DEHAM";
			Assert(collection.IsAirImportOrTransitToICS2Zone);
		}

		#region Implementation

		Transport Transport1
		{
			get
			{
				if (transport1 == null)
				{
					transport1 = Consol.Transports.AddNew();
					transport1.JW_RL_NKLoadPort = "SGSIN";
					transport1.JW_RL_NKDiscPort = "MYPKG";
				}
				return transport1;
			}
		}
		Transport transport1;

		Transport Transport2
		{
			get
			{
				if (transport2 == null)
				{
					transport2 = Consol.Transports.AddNew();
					transport2.JW_RL_NKLoadPort = "MYPKG";
					transport2.JW_RL_NKDiscPort = "AUSYD";
				}
				return transport2;
			}
		}
		Transport transport2;

		CommonConsol Consol
		{
			get { return consol ?? (consol = Factory.New<CommonConsol>()); }
		}
		CommonConsol consol;

		void UpdateVesselUnderTransport(RefVessel vessel, ITransport transport, string screeningStatus, bool isEmptyVesselCode = false)
		{
			vessel.RV_ScreeningStatus = screeningStatus;
			vessel.RV_Name = screeningStatus;
			transport.JW_Vessel = isEmptyVesselCode ? ZString.Empty : vessel.RV_FK;
		}

		#endregion
	}
}
