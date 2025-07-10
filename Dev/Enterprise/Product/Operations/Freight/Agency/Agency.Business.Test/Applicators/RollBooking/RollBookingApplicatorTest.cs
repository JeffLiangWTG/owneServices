using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(RollBookingApplicator))]
	internal class RollBookingApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestRollingBooking
		public void TestRollingBooking_NewSailing()
		{
			AssertRollingBooking(false);
		}

		public void TestRollingBooking_ExistedSailing()
		{
			AssertRollingBooking(true);
		}

		public void TestRollingBooking_WithoutMainLeg()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			var sailing1 = CreateSailing("MAJAPAHIT", "x42", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			var sailing2 = CreateSailing("COSCO NEBULA", "85475", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			Factory.Save();
			var booking1 = Factory.New<AgencyBooking>();
			booking1.JS_JX = sailing1.PK;
			var booking2 = Factory.New<AgencyBooking>();
			booking2.JS_JX = sailing2.PK;
			Factory.Save();
			var factory = new BusinessObjectFactory();
			var booking1a = factory.Load<AgencyBooking>(booking1.PK);
			var booking2a = factory.Load<AgencyBooking>(booking2.PK);
			var transport1 = Applicator.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport1.JW_Vessel = "XI FENG KOU";
			transport1.JW_VoyageFlight = "9955";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_ETD = ZDateTime.Today.AddDays(3);
			transport1.JW_ETA = ZDateTime.Today.AddDays(10);
			transport1.CarrierPK = carrier.PK;
			transport1.JW_IsLinked = true;
			var transport2 = Applicator.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport2.JW_Vessel = "ADMIRALENGRACHT";
			transport2.JW_VoyageFlight = "66777";
			transport2.JW_RL_NKLoadPort = "NZAKL";
			transport2.JW_RL_NKDiscPort = "SGSIN";
			transport2.JW_ETD = ZDateTime.Today.AddDays(11);
			transport2.JW_ETA = ZDateTime.Today.AddDays(13);
			transport2.CarrierPK = carrier.PK;
			transport2.JW_IsLinked = true;
			var targets = new BusinessObject[] { booking1a, booking2a };
			var expectedLog = @"WARNING: Agency Booking V00001000 has the following warnings:
WARNING: The Discharge Port on the booking does not match the Discharge Port of the last routing leg.
INFO: [HL Shipping Booking V00001000] does not have a job
WARNING: Agency Booking V00001001 has the following warnings:
WARNING: The Discharge Port on the booking does not match the Discharge Port of the last routing leg.
INFO: [HL Shipping Booking V00001001] does not have a job";
			ApplyApplicator(targets, expectedLog, true);
			var factory2 = new BusinessObjectFactory();
			var booking1b = factory2.Load<AgencyBooking>(booking1.PK);
			var booking2b = factory2.Load<AgencyBooking>(booking2.PK);
			foreach (var booking in new[] { booking1b, booking2b })
			{
				Assert(booking.JS_JX.IsEmpty);
				var createdTransports = booking.Transports.OfType<Transport>().Where(t => t.JW_TransportType == Core.Constants.TransportPlanningType.Other).ToArray();
				AssertEquals(2, createdTransports.Length);
				var createdTransport1 = createdTransports[0];
				AssertNotNull(createdTransport1);
				AssertEquals("XI FENG KOU", createdTransport1.JW_Vessel);
				AssertEquals("9955", createdTransport1.JW_VoyageFlight);
				AssertEquals("AUSYD", createdTransport1.JW_RL_NKLoadPort);
				AssertEquals("NZAKL", createdTransport1.JW_RL_NKDiscPort);
				AssertEquals(ZDateTime.Today.AddDays(3), createdTransport1.JW_ETD);
				AssertEquals(ZDateTime.Today.AddDays(10), createdTransport1.JW_ETA);
				AssertEquals(true, createdTransport1.JW_IsLinked);
				var createdTransport2 = createdTransports[1];
				AssertNotNull(createdTransport2);
				AssertEquals("ADMIRALENGRACHT", createdTransport2.JW_Vessel);
				AssertEquals("66777", createdTransport2.JW_VoyageFlight);
				AssertEquals("NZAKL", createdTransport2.JW_RL_NKLoadPort);
				AssertEquals("SGSIN", createdTransport2.JW_RL_NKDiscPort);
				AssertEquals(ZDateTime.Today.AddDays(11), createdTransport2.JW_ETD);
				AssertEquals(ZDateTime.Today.AddDays(13), createdTransport2.JW_ETA);
				AssertEquals(true, createdTransport2.JW_IsLinked);
			}
		}

		void AssertRollingBooking(bool existedSailing)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			var sailing1 = CreateSailing("MAJAPAHIT", "x42", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			var sailing2 = CreateSailing("COSCO NEBULA", "85475", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			if (existedSailing)
			{
				CreateSailing("XI FENG KOU", "9955", "AUSYD", "NZAKL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(10), carrier);
			}

			Factory.Save();
			var booking1 = Factory.New<AgencyBooking>();
			booking1.JS_JX = sailing1.PK;
			var booking2 = Factory.New<AgencyBooking>();
			booking2.JS_JX = sailing2.PK;
			Factory.Save();
			var factory = new BusinessObjectFactory();
			var booking1a = factory.Load<AgencyBooking>(booking1.PK);
			var booking2a = factory.Load<AgencyBooking>(booking2.PK);
			var transport1 = Applicator.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_Vessel = "XI FENG KOU";
			transport1.JW_VoyageFlight = "9955";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_ETD = ZDateTime.Today.AddDays(3);
			transport1.JW_ETA = ZDateTime.Today.AddDays(10);
			transport1.CarrierPK = carrier.PK;
			transport1.JW_IsLinked = true;
			var transport2 = Applicator.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport2.JW_Vessel = "ADMIRALENGRACHT";
			transport2.JW_VoyageFlight = "66777";
			transport2.JW_RL_NKLoadPort = "NZAKL";
			transport2.JW_RL_NKDiscPort = "SGSIN";
			transport2.JW_ETD = ZDateTime.Today.AddDays(11);
			transport2.JW_ETA = ZDateTime.Today.AddDays(13);
			transport2.CarrierPK = carrier.PK;
			transport2.JW_IsLinked = false;
			var targets = new BusinessObject[] { booking1a, booking2a };
			var expectedLog = @"WARNING: Agency Booking V00001000 has the following warnings:
WARNING: The Discharge Port on the booking does not match the Discharge Port of the last routing leg.
INFO: [HL Shipping Booking V00001000] does not have a job
WARNING: Agency Booking V00001001 has the following warnings:
WARNING: The Discharge Port on the booking does not match the Discharge Port of the last routing leg.
INFO: [HL Shipping Booking V00001001] does not have a job";
			ApplyApplicator(targets, expectedLog, true);
			var factory2 = new BusinessObjectFactory();
			var booking1b = factory2.Load<AgencyBooking>(booking1.PK);
			var booking2b = factory2.Load<AgencyBooking>(booking2.PK);
			foreach (var booking in new[] { booking1b, booking2b })
			{
				Assert(!booking.JS_JX.IsEmpty);
				AssertEquals(2, booking.Transports.Count);
				var createdTransport1 = booking.Transports.OfType<Transport>().FirstOrDefault(t => t.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel);
				AssertNotNull(createdTransport1);
				AssertEquals("XI FENG KOU", createdTransport1.JW_Vessel);
				AssertEquals("9955", createdTransport1.JW_VoyageFlight);
				AssertEquals("AUSYD", createdTransport1.JW_RL_NKLoadPort);
				AssertEquals("NZAKL", createdTransport1.JW_RL_NKDiscPort);
				AssertEquals(ZDateTime.Today.AddDays(3), createdTransport1.JW_ETD);
				AssertEquals(ZDateTime.Today.AddDays(10), createdTransport1.JW_ETA);
				AssertEquals(true, createdTransport1.JW_IsLinked);
				var createdTransport2 = booking.Transports.OfType<Transport>().FirstOrDefault(t => t.JW_TransportType == Core.Constants.TransportPlanningType.Other);
				AssertNotNull(createdTransport2);
				AssertEquals("ADMIRALENGRACHT", createdTransport2.JW_Vessel);
				AssertEquals("66777", createdTransport2.JW_VoyageFlight);
				AssertEquals("NZAKL", createdTransport2.JW_RL_NKLoadPort);
				AssertEquals("SGSIN", createdTransport2.JW_RL_NKDiscPort);
				AssertEquals(ZDateTime.Today.AddDays(11), createdTransport2.JW_ETD);
				AssertEquals(ZDateTime.Today.AddDays(13), createdTransport2.JW_ETA);
				AssertEquals(false, createdTransport2.JW_IsLinked);
			}
		}

		#endregion
		#region ErrorMessageBeforeRunning
		public void TestErrorMessage_TransportValidation()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			var sailing1 = CreateSailing("MAJAPAHIT", "x42", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			var sailing2 = CreateSailing("COSCO NEBULA", "85475", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			Factory.Save();
			var booking1 = Factory.New<AgencyBooking>();
			booking1.JS_JX = sailing1.PK;
			var booking2 = Factory.New<AgencyBooking>();
			booking2.JS_JX = sailing2.PK;
			var transport1 = Applicator.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_Vessel = "XI FENG KOU";
			transport1.JW_VoyageFlight = "9955";
			transport1.JW_IsLinked = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ApplyApplicator(new[] { booking1, booking2 }, string.Empty);
			AssertEquals("Please fix the transport errors first and run this again.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion
		#region Validation Log
		public void TestValidationLog_MultipeMainLinkedLegsFound()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			var sailing1 = CreateSailing("MAJAPAHIT", "x42", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			var sailing2 = CreateSailing("COSCO NEBULA", "85475", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			Factory.Save();
			var booking1 = Factory.New<AgencyBooking>();
			booking1.JS_JX = sailing1.PK;
			var booking2 = Factory.New<AgencyBooking>();
			booking2.JS_JX = sailing2.PK;
			var transport1 = Applicator.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_Vessel = "XI FENG KOU";
			transport1.JW_VoyageFlight = "9955";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_ETD = ZDateTime.Today.AddDays(3);
			transport1.JW_ETA = ZDateTime.Today.AddDays(10);
			transport1.CarrierPK = carrier.PK;
			transport1.JW_IsLinked = true;
			var transport2 = Applicator.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport2.JW_Vessel = "XI FENG KOU";
			transport2.JW_VoyageFlight = "9955";
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_ETD = ZDateTime.Today.AddDays(-3);
			transport2.JW_ETA = ZDateTime.Today.AddDays(10);
			transport2.CarrierPK = carrier.PK;
			transport2.JW_IsLinked = false;
			var expectedLog = "ERROR: Can't have more than one MAI Transport Type.";
			ApplyApplicator(new[] { booking1, booking2 }, expectedLog);
		}

		public void TestValidationLog_ETDIsEarlierThanBookingETD()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			var sailing1 = CreateSailing("MAJAPAHIT", "x42", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			var sailing2 = CreateSailing("COSCO NEBULA", "85475", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			Factory.Save();
			var booking1 = Factory.New<AgencyBooking>();
			booking1.JS_JX = sailing1.PK;
			var booking2 = Factory.New<AgencyBooking>();
			booking2.JS_JX = sailing2.PK;
			Factory.Save();
			var factory = new BusinessObjectFactory();
			var booking1a = factory.Load<AgencyBooking>(booking1.PK);
			var booking2a = factory.Load<AgencyBooking>(booking2.PK);
			var transport1 = Applicator.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_Vessel = "XI FENG KOU";
			transport1.JW_VoyageFlight = "9955";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_ETD = ZDateTime.Today.AddDays(-3);
			transport1.JW_ETA = ZDateTime.Today.AddDays(10);
			transport1.CarrierPK = carrier.PK;
			transport1.JW_IsLinked = true;
			var targets = new BusinessObject[] { booking1a, booking2a };
			var expectedLog = @"WARNING: Agency Booking V00001000 has the following warnings:
WARNING: The ETD of the new voyage is earlier than Origin ETD of the shipment.
INFO: [HL Shipping Booking V00001000] does not have a job
WARNING: Agency Booking V00001001 has the following warnings:
WARNING: The ETD of the new voyage is earlier than Origin ETD of the shipment.
INFO: [HL Shipping Booking V00001001] does not have a job";
			ApplyApplicator(targets, expectedLog, true);
		}

		public void TestValidationLog_ETDIsEarlierThanBookingETD_ForMultipleLegs()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			var sailing1 = CreateSailing("MAJAPAHIT", "x42", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			var sailing2 = CreateSailing("COSCO NEBULA", "85475", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			Factory.Save();
			var booking1 = Factory.New<AgencyBooking>();
			booking1.JS_JX = sailing1.PK;
			var booking2 = Factory.New<AgencyBooking>();
			booking2.JS_JX = sailing2.PK;
			Factory.Save();
			var factory = new BusinessObjectFactory();
			var booking1a = factory.Load<AgencyBooking>(booking1.PK);
			var booking2a = factory.Load<AgencyBooking>(booking2.PK);
			var transport1 = Applicator.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport1.JW_Vessel = "TAIKO";
			transport1.JW_VoyageFlight = "4477";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_ETD = ZDateTime.Today.AddDays(-3);
			transport1.JW_ETA = ZDateTime.Today.AddDays(10);
			transport1.JW_IsLinked = false;
			var transport2 = Applicator.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport2.JW_Vessel = "XI FENG KOU";
			transport2.JW_VoyageFlight = "9955";
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_ETD = ZDateTime.Today.AddDays(15);
			transport2.JW_ETA = ZDateTime.Today.AddDays(20);
			transport2.CarrierPK = carrier.PK;
			transport2.JW_IsLinked = true;
			var targets = new BusinessObject[] { booking1a, booking2a };
			var expectedLog = @"WARNING: Agency Booking V00001000 has the following warnings:
WARNING: The ETD of the new voyage is earlier than Origin ETD of the shipment.
INFO: [HL Shipping Booking V00001000] does not have a job
WARNING: Agency Booking V00001001 has the following warnings:
WARNING: The ETD of the new voyage is earlier than Origin ETD of the shipment.
INFO: [HL Shipping Booking V00001001] does not have a job";
			ApplyApplicator(targets, expectedLog, true);
		}

		public void TestValidationLog_CarrierMismatch()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_IsShippingLine = true;
			carrier1.OH_IsShippingProvider = true;
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier2.OH_IsShippingProvider = true;
			var sailing1 = CreateSailing("MAJAPAHIT", "x42", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier1);
			var sailing2 = CreateSailing("COSCO NEBULA", "85475", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier1);
			Factory.Save();
			var booking1 = Factory.New<AgencyBooking>();
			booking1.JS_JX = sailing1.PK;
			var booking2 = Factory.New<AgencyBooking>();
			booking2.JS_JX = sailing2.PK;
			Factory.Save();
			var factory = new BusinessObjectFactory();
			var booking1a = factory.Load<AgencyBooking>(booking1.PK);
			var booking2a = factory.Load<AgencyBooking>(booking2.PK);
			var transport1 = Applicator.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_Vessel = "XI FENG KOU";
			transport1.JW_VoyageFlight = "9955";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_ETD = ZDateTime.Today.AddDays(3);
			transport1.JW_ETA = ZDateTime.Today.AddDays(10);
			transport1.CarrierPK = carrier2.PK;
			transport1.JW_IsLinked = true;
			var targets = new BusinessObject[] { booking1a, booking2a };
			var expectedLog = @"WARNING: Agency Booking V00001000 has the following warnings:
WARNING: The Carrier is not the same.
INFO: [HL Shipping Booking V00001000] does not have a job
WARNING: Agency Booking V00001001 has the following warnings:
WARNING: The Carrier is not the same.
INFO: [HL Shipping Booking V00001001] does not have a job";
			ApplyApplicator(targets, expectedLog, true);
		}

		public void TestValidationLog_DischargePortMismatch()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			var sailing1 = CreateSailing("MAJAPAHIT", "x42", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			var sailing2 = CreateSailing("COSCO NEBULA", "85475", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			Factory.Save();
			var booking1 = Factory.New<AgencyBooking>();
			booking1.JS_JX = sailing1.PK;
			var transport1 = booking1.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport1.JW_Vessel = "OLIVIA";
			transport1.JW_VoyageFlight = "1234";
			transport1.JW_RL_NKLoadPort = "NZAKL";
			transport1.JW_RL_NKDiscPort = "HKHKG";
			transport1.JW_ETD = ZDateTime.Today.AddDays(2);
			transport1.JW_ETA = ZDateTime.Today.AddDays(10);
			var booking2 = Factory.New<AgencyBooking>();
			booking2.JS_JX = sailing2.PK;
			var transport2 = booking2.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport2.JW_Vessel = "FANAL MARINER";
			transport2.JW_VoyageFlight = "5678";
			transport2.JW_RL_NKLoadPort = "NZAKL";
			transport2.JW_RL_NKDiscPort = "SGSIN";
			transport2.JW_ETD = ZDateTime.Today.AddDays(2);
			transport2.JW_ETA = ZDateTime.Today.AddDays(10);
			Factory.Save();
			var factory = new BusinessObjectFactory();
			var booking1a = factory.Load<AgencyBooking>(booking1.PK);
			var booking2a = factory.Load<AgencyBooking>(booking2.PK);
			var transport3 = Applicator.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport3.JW_Vessel = "XI FENG KOU";
			transport3.JW_VoyageFlight = "9955";
			transport3.JW_RL_NKLoadPort = "AUSYD";
			transport3.JW_RL_NKDiscPort = "NZAKL";
			transport3.JW_ETD = ZDateTime.Today.AddDays(3);
			transport3.JW_ETA = ZDateTime.Today.AddDays(10);
			transport3.CarrierPK = carrier.PK;
			transport3.JW_IsLinked = true;
			var transport4 = Applicator.Transports.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport4.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport4.JW_Vessel = "TAIKO";
			transport4.JW_VoyageFlight = "4477";
			transport4.JW_RL_NKLoadPort = "NZAKL";
			transport4.JW_RL_NKDiscPort = "CNSHA";
			transport4.JW_ETD = ZDateTime.Today.AddDays(11);
			transport4.JW_ETA = ZDateTime.Today.AddDays(20);
			transport4.JW_IsLinked = false;
			var expectedLog = @"WARNING: Agency Booking V00001000 has the following warnings:
WARNING: The Discharge Port on the booking does not match the Discharge Port of the last routing leg.
INFO: [HL Shipping Booking V00001000] does not have a job
WARNING: Agency Booking V00001001 has the following warnings:
WARNING: The Discharge Port on the booking does not match the Discharge Port of the last routing leg.
INFO: [HL Shipping Booking V00001001] does not have a job";
			ApplyApplicator(new[] { booking1a, booking2a }, expectedLog);
		}

		public void TestValidationLog_NewETAShouldBePriorThanBookingETA()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			var sailing1 = CreateSailing("MAJAPAHIT", "x42", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			var sailing2 = CreateSailing("COSCO NEBULA", "85475", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			Factory.Save();
			var booking1 = Factory.New<AgencyBooking>();
			booking1.JS_JX = sailing1.PK;
			booking1.JS_E_ARV = ZDateTime.Today.AddDays(10);
			var transport1 = booking1.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport1.JW_Vessel = "OLIVIA";
			transport1.JW_VoyageFlight = "1234";
			transport1.JW_RL_NKLoadPort = "NZAKL";
			transport1.JW_RL_NKDiscPort = "HKHKG";
			transport1.JW_ETD = ZDateTime.Today.AddDays(2);
			transport1.JW_ETA = ZDateTime.Today.AddDays(10);
			var booking2 = Factory.New<AgencyBooking>();
			booking2.JS_JX = sailing2.PK;
			booking2.JS_E_ARV = ZDateTime.Today.AddDays(10);
			var transport2 = booking2.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport2.JW_Vessel = "FANAL MARINER";
			transport2.JW_VoyageFlight = "5678";
			transport2.JW_RL_NKLoadPort = "NZAKL";
			transport2.JW_RL_NKDiscPort = "HKHKG";
			transport2.JW_ETD = ZDateTime.Today.AddDays(2);
			transport2.JW_ETA = ZDateTime.Today.AddDays(10);
			Factory.Save();
			var factory = new BusinessObjectFactory();
			var booking1a = factory.Load<AgencyBooking>(booking1.PK);
			var booking2a = factory.Load<AgencyBooking>(booking2.PK);
			var transport3 = Applicator.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport3.JW_Vessel = "XI FENG KOU";
			transport3.JW_VoyageFlight = "9955";
			transport3.JW_RL_NKLoadPort = "AUSYD";
			transport3.JW_RL_NKDiscPort = "NZAKL";
			transport3.JW_ETD = ZDateTime.Today.AddDays(3);
			transport3.JW_ETA = ZDateTime.Today.AddDays(5);
			transport3.CarrierPK = carrier.PK;
			transport3.JW_IsLinked = true;
			var transport4 = Applicator.Transports.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport4.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport4.JW_Vessel = "TAIKO";
			transport4.JW_VoyageFlight = "4477";
			transport4.JW_RL_NKLoadPort = "NZAKL";
			transport4.JW_RL_NKDiscPort = "HKHKG";
			transport4.JW_ETD = ZDateTime.Today.AddDays(6);
			transport4.JW_ETA = ZDateTime.Today.AddDays(20);
			transport4.JW_IsLinked = false;
			var expectedLog = @"WARNING: Agency Booking V00001000 has the following warnings:
WARNING: The Discharge ETA of the new voyage must be prior to the Destination ETA of the Shipment.
INFO: [HL Shipping Booking V00001000] does not have a job
WARNING: Agency Booking V00001001 has the following warnings:
WARNING: The Discharge ETA of the new voyage must be prior to the Destination ETA of the Shipment.
INFO: [HL Shipping Booking V00001001] does not have a job";
			ApplyApplicator(new[] { booking1a, booking2a }, expectedLog);
		}

		#endregion
		#region TestValidationLog_Allocations
		public void TestValidationLog_Allocations_NotSet()
		{
			AssertValidationLog_Allocations(false, AllocationMethodList.Codes.NotSet, @"INFO: [HL Shipping Booking V00001000] does not have a job
INFO: [HL Shipping Booking V00001001] does not have a job");
		}

		public void TestValidationLog_Allocations_Ignore()
		{
			AssertValidationLog_Allocations(true, AllocationMethodList.Codes.Ignore, @"INFO: [HL Shipping Booking V00001000] does not have a job
INFO: [HL Shipping Booking V00001001] does not have a job");
		}

		public void TestValidationLog_Allocations_Sailing()
		{
			var expectedLog = @"ERROR: Agency Booking V00001000 has the following errors:
ERROR: Allocations on the new schedule exceed the size of the bookings.";
			AssertValidationLog_Allocations(true, AllocationMethodList.Codes.Sailing, expectedLog);
		}

		void AssertValidationLog_Allocations(bool existedSailing, ZString allocationMethod, string expectedLog)
		{
			var rc_40RE_PK = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			var sailing1 = CreateSailing("MAJAPAHIT", "x42", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			var sailing2 = CreateSailing("COSCO NEBULA", "85475", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			if (existedSailing)
			{
				var sailing3 = CreateSailing("XI FENG KOU", "9955", "AUSYD", "NZAKL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(10), carrier);
				var originCountry = sailing3.Origin.VoyageCountry;
				originCountry.J0_AllocationMethod = allocationMethod;
			}

			var booking1 = Factory.New<AgencyBooking>();
			booking1.JS_JX = sailing1.PK;
			var container1 = booking1.BookedContainers.AddNew();
			container1.JC_ContainerNum = "TEST4100029";
			container1.JC_RC = rc_40RE_PK;
			container1.JC_ContainerCount = 1;
			container1.JC_TareWeight = 3500;
			container1.JC_Calc_NetWeight = 1000;
			var booking2 = Factory.New<AgencyBooking>();
			booking2.JS_JX = sailing2.PK;
			var container2 = booking2.BookedContainers.AddNew();
			container2.JC_ContainerNum = "TEST4100030";
			container2.JC_RC = rc_40RE_PK;
			container2.JC_ContainerCount = 1;
			container2.JC_TareWeight = 3500;
			container2.JC_Calc_NetWeight = 1000;
			Factory.Save();
			var factory = new BusinessObjectFactory();
			var booking1a = factory.Load<AgencyBooking>(booking1.PK);
			var booking2a = factory.Load<AgencyBooking>(booking2.PK);
			var transport1 = Applicator.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_Vessel = "XI FENG KOU";
			transport1.JW_VoyageFlight = "9955";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_ETD = ZDateTime.Today.AddDays(3);
			transport1.JW_ETA = ZDateTime.Today.AddDays(10);
			transport1.CarrierPK = carrier.PK;
			transport1.JW_IsLinked = true;
			var targets = new BusinessObject[] { booking1a, booking2a };
			ApplyApplicator(targets, expectedLog, true);
		}

		public void TestValidationLog_Allocations_CanFit()
		{
			var expectedLog = @"INFO: [HL Shipping Booking V00001000] does not have a job
INFO: [HL Shipping Booking V00001001] does not have a job";
			AssertValidationLog_Allocations_CanFit(10m, 10m, 10m, 10m, 10m, expectedLog);
		}

		public void TestValidationLog_Allocations_CannotFit()
		{
			var expectedLog = @"INFO: [HL Shipping Booking V00001000] does not have a job
ERROR: Agency Booking V00001001 has the following errors:
ERROR: Allocations on the new schedule exceed the size of the bookings.";
			AssertValidationLog_Allocations_CanFit(1.5m, 2m, 5m, 2m, 3m, expectedLog);
		}

		void AssertValidationLog_Allocations_CanFit(decimal teu, decimal powerPoints, decimal tonnes, decimal volume, decimal area, string expectedLog)
		{
			var rc_20RE_PK = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			var rc_40RE_PK = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			var sailing1 = CreateSailing("MAJAPAHIT", "x42", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			var sailing2 = CreateSailing("XI FENG KOU", "9955", "AUSYD", "NZAKL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(10), carrier);
			sailing2.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Sailing;
			var slotAllocation = sailing2.SlotAllocations.AddNew();
			slotAllocation.SetAspect(AllocationAspectTypes.TEU, teu);
			slotAllocation.SetAspect(AllocationAspectTypes.PowerPoints, powerPoints);
			slotAllocation.SetAspect(AllocationAspectTypes.Tonnes, tonnes);
			slotAllocation.SetAspect(AllocationAspectTypes.Volume, volume);
			slotAllocation.SetAspect(AllocationAspectTypes.Area, area);
			var booking1 = Factory.New<AgencyBooking>();
			booking1.JS_JX = sailing1.PK;
			var container1 = booking1.BookedContainers.AddNew();
			container1.JC_ContainerNum = "TEST4100029";
			container1.JC_RC = rc_20RE_PK;
			container1.JC_ContainerCount = 1;
			container1.JC_TareWeight = 3500;
			container1.JC_Calc_NetWeight = 1000;
			var booking2 = Factory.New<AgencyBooking>();
			booking2.JS_JX = sailing1.PK;
			var container2 = booking2.BookedContainers.AddNew();
			container2.JC_ContainerNum = "TEST4100030";
			container2.JC_RC = rc_40RE_PK;
			container2.JC_ContainerCount = 1;
			container2.JC_TareWeight = 3500;
			container2.JC_Calc_NetWeight = 1000;
			Factory.Save();
			var factory = new BusinessObjectFactory();
			var booking1a = factory.Load<AgencyBooking>(booking1.PK);
			var booking2a = factory.Load<AgencyBooking>(booking2.PK);
			var transport1 = Applicator.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_Vessel = "XI FENG KOU";
			transport1.JW_VoyageFlight = "9955";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_ETD = ZDateTime.Today.AddDays(3);
			transport1.JW_ETA = ZDateTime.Today.AddDays(10);
			transport1.CarrierPK = carrier.PK;
			transport1.JW_IsLinked = true;
			var targets = new BusinessObject[] { booking1a, booking2a };
			ApplyApplicator(targets, expectedLog, true);
		}

		#endregion
		#region Apply Exchange Rate
		public void TestApplyExchangeRate()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			var sailing1 = CreateSailing("MAJAPAHIT", "x42", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			var sailing2 = CreateSailing("COSCO NEBULA", "85475", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			var sailing3 = CreateSailing("XI FENG KOU", "9955", "AUSYD", "NZAKL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(10), carrier);
			SetVoyageExchangeRate(sailing3.Voyage, "USD", 1.3m);
			SetVoyageExchangeRate(sailing3.Voyage, "EUR", 1.5m);
			Factory.Save();
			var booking1 = Factory.New<AgencyBooking>();
			booking1.JS_JX = sailing1.PK;
			var job1 = CreateJobHeader(booking1, "number1");
			CreateExchangeRate(job1, "USD", 2.5m);
			var booking2 = Factory.New<AgencyBooking>();
			booking2.JS_JX = sailing2.PK;
			var job2 = CreateJobHeader(booking2, "number2");
			CreateExchangeRate(job2, "EUR", 10m);
			Factory.Save();
			var factory = new BusinessObjectFactory();
			var booking1a = factory.Load<AgencyBooking>(booking1.PK);
			var booking2a = factory.Load<AgencyBooking>(booking2.PK);
			var transport1 = Applicator.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_Vessel = "XI FENG KOU";
			transport1.JW_VoyageFlight = "9955";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_ETD = ZDateTime.Today.AddDays(3);
			transport1.JW_ETA = ZDateTime.Today.AddDays(10);
			transport1.CarrierPK = carrier.PK;
			transport1.JW_IsLinked = true;
			var targets = new BusinessObject[] { booking1a, booking2a };
			var expectedLog = @"INFO: [HL Shipping Booking V00001000] exchange rates were updated from [HL XI FENG KOU/9955]:
INFO:   • Updated exchange rate for 'USD' from 2.500 to 1.300
INFO: [HL Shipping Booking V00001001] exchange rates were updated from [HL XI FENG KOU/9955]:
INFO:   • Updated exchange rate for 'EUR' from 10.000 to 1.500";
			ApplyApplicator(targets, expectedLog, true);
			var factory2 = new BusinessObjectFactory();
			var booking1b = factory2.Load<AgencyBooking>(booking1.PK);
			var booking2b = factory2.Load<AgencyBooking>(booking2.PK);
			foreach (var booking in new[] { booking1b, booking2b })
			{
				Assert(!booking.JS_JX.IsEmpty);
				AssertEquals(1, booking.Transports.Count);
				var transport = booking.Transports.OfType<Transport>().FirstOrDefault(t => t.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel);
				AssertNotNull(transport);
				AssertEquals("XI FENG KOU", transport.JW_Vessel);
				AssertEquals("9955", transport.JW_VoyageFlight);
				AssertEquals(2, ((BusinessObjectCollection)booking.Job["ExchangeRates"]).Count);
				AssertexchangeRate(booking, "USD", 1.3m);
				AssertexchangeRate(booking, "EUR", 1.5m);
			}
		}

		public void TestApplyExchangeRate_NoJobOrNoExchangeRates()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			var sailing1 = CreateSailing("MAJAPAHIT", "x42", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			var sailing2 = CreateSailing("COSCO NEBULA", "85475", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			CreateSailing("XI FENG KOU", "9955", "AUSYD", "NZAKL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(10), carrier);
			Factory.Save();
			var booking1 = Factory.New<AgencyBooking>();
			booking1.JS_JX = sailing1.PK;
			var booking2 = Factory.New<AgencyBooking>();
			booking2.JS_JX = sailing2.PK;
			CreateJobHeader(booking2, "number2");
			Factory.Save();
			var factory = new BusinessObjectFactory();
			var booking1a = factory.Load<AgencyBooking>(booking1.PK);
			var booking2a = factory.Load<AgencyBooking>(booking2.PK);
			var transport1 = Applicator.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_Vessel = "XI FENG KOU";
			transport1.JW_VoyageFlight = "9955";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_ETD = ZDateTime.Today.AddDays(3);
			transport1.JW_ETA = ZDateTime.Today.AddDays(10);
			transport1.CarrierPK = carrier.PK;
			transport1.JW_IsLinked = true;
			var targets = new BusinessObject[] { booking1a, booking2a /*, booking3a*/ };
			var expectedLog = @"INFO: [HL Shipping Booking V00001000] does not have a job
INFO: [HL Shipping Booking V00001001] does not have any exchange rates to update";
			ApplyApplicator(targets, expectedLog, true);
		}

		public void TestApplyExchangeRate_NoExRateInSource()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			var sailing1 = CreateSailing("MAJAPAHIT", "x42", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			var sailing2 = CreateSailing("COSCO NEBULA", "85475", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			var sailing3 = CreateSailing("XI FENG KOU", "9955", "AUSYD", "NZAKL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(10), carrier);
			SetVoyageExchangeRate(sailing3.Voyage, "USD", 1.3m);
			SetVoyageExchangeRate(sailing3.Voyage, "EUR", 1.5m);
			Factory.Save();
			var booking1 = Factory.New<AgencyBooking>();
			booking1.JS_JX = sailing1.PK;
			var job1 = CreateJobHeader(booking1, "number1");
			CreateExchangeRate(job1, "SGD", 2.5m);
			var booking2 = Factory.New<AgencyBooking>();
			booking2.JS_JX = sailing2.PK;
			var job2 = CreateJobHeader(booking2, "number2");
			CreateExchangeRate(job2, "EUR", 10m);
			Factory.Save();
			var factory = new BusinessObjectFactory();
			var booking1a = factory.Load<AgencyBooking>(booking1.PK);
			var booking2a = factory.Load<AgencyBooking>(booking2.PK);
			var transport1 = Applicator.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_Vessel = "XI FENG KOU";
			transport1.JW_VoyageFlight = "9955";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_ETD = ZDateTime.Today.AddDays(3);
			transport1.JW_ETA = ZDateTime.Today.AddDays(10);
			transport1.CarrierPK = carrier.PK;
			transport1.JW_IsLinked = true;
			var targets = new BusinessObject[] { booking1a, booking2a };
			var expectedLog = @"INFO: [HL Shipping Booking V00001000] exchange rates were updated from [HL XI FENG KOU/9955]:
WARNING:   • Exchange rate for 'SGD' does not exist in [HL XI FENG KOU/9955]
INFO: [HL Shipping Booking V00001001] exchange rates were updated from [HL XI FENG KOU/9955]:
INFO:   • Updated exchange rate for 'EUR' from 10.000 to 1.500";
			ApplyApplicator(targets, expectedLog, true);
		}

		public void TestApplyExchangeRate_RateUnchanged()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			var sailing1 = CreateSailing("MAJAPAHIT", "x42", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			var sailing2 = CreateSailing("COSCO NEBULA", "85475", "AUSYD", "NZAKL", ZDateTime.Today, ZDateTime.Empty, carrier);
			var sailing3 = CreateSailing("XI FENG KOU", "9955", "AUSYD", "NZAKL", ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(10), carrier);
			SetVoyageExchangeRate(sailing3.Voyage, "USD", 1.3m);
			SetVoyageExchangeRate(sailing3.Voyage, "EUR", 1.5m);
			Factory.Save();
			var booking1 = Factory.New<AgencyBooking>();
			booking1.JS_JX = sailing1.PK;
			var job1 = CreateJobHeader(booking1, "number1");
			CreateExchangeRate(job1, "USD", 1.3m);
			var booking2 = Factory.New<AgencyBooking>();
			booking2.JS_JX = sailing2.PK;
			var job2 = CreateJobHeader(booking2, "number2");
			CreateExchangeRate(job2, "EUR", 10m);
			Factory.Save();
			var factory = new BusinessObjectFactory();
			var booking1a = factory.Load<AgencyBooking>(booking1.PK);
			var booking2a = factory.Load<AgencyBooking>(booking2.PK);
			var transport1 = Applicator.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_Vessel = "XI FENG KOU";
			transport1.JW_VoyageFlight = "9955";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_ETD = ZDateTime.Today.AddDays(3);
			transport1.JW_ETA = ZDateTime.Today.AddDays(10);
			transport1.CarrierPK = carrier.PK;
			transport1.JW_IsLinked = true;
			var targets = new BusinessObject[] { booking1a, booking2a };
			var expectedLog = @"INFO: [HL Shipping Booking V00001000] exchange rates were updated from [HL XI FENG KOU/9955]:
INFO:   • Exchange rate for 'USD' remains unchanged at 1.300
INFO: [HL Shipping Booking V00001001] exchange rates were updated from [HL XI FENG KOU/9955]:
INFO:   • Updated exchange rate for 'EUR' from 10.000 to 1.500";
			ApplyApplicator(targets, expectedLog, true);
			var factory2 = new BusinessObjectFactory();
			var booking1b = factory2.Load<AgencyBooking>(booking1.PK);
			var booking2b = factory2.Load<AgencyBooking>(booking2.PK);
			foreach (var booking in new[] { booking1b, booking2b })
			{
				Assert(!booking.JS_JX.IsEmpty);
				AssertEquals(1, booking.Transports.Count);
				var transport = booking.Transports.OfType<Transport>().FirstOrDefault(t => t.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel);
				AssertNotNull(transport);
				AssertEquals("XI FENG KOU", transport.JW_Vessel);
				AssertEquals("9955", transport.JW_VoyageFlight);
			}

			AssertEquals("No exchange rate defaulting when the rate is same", 1, ((BusinessObjectCollection)booking1b.Job["ExchangeRates"]).Count);
			AssertexchangeRate(booking1b, "USD", 1.3m);
			AssertEquals(2, ((BusinessObjectCollection)booking2b.Job["ExchangeRates"]).Count);
			AssertexchangeRate(booking2b, "USD", 1.3m);
			AssertexchangeRate(booking2b, "EUR", 1.5m);
		}

		#region Helpers
		void AssertexchangeRate(AgencyBooking booking, string currencyCode, ZDecimal baseRate)
		{
			var job = booking.Job;
			var currency = RefCurrency.LoadFromCurrencyCode(booking.Factory, currencyCode);
			var exchangeRates = (BusinessObjectCollection)job["ExchangeRates"];
			var rate = exchangeRates.FirstOrDefault(r => (ZString)r["JF_RX_NKRateCurrency"] == currency.RX_Code);
			AssertNotNull(rate);
			AssertEquals(baseRate, (ZDecimal)rate["JF_BaseRate"]);
		}

		JobHeader CreateJobHeader(AgencyBooking booking, ZString jobNum)
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = booking.PK;
			job.JH_JobNum = jobNum;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			return job;
		}

		BusinessObject CreateExchangeRate(JobHeader job, string currencyCode, ZDecimal baseRate /*string orgType, */ /*ZGuid orgHeaderPK,*/)
		{
			var exRate = (BusinessObject)Factory.New<IExchangeRate>();
			exRate["JF_RX_NKRateCurrency"] = currencyCode;
			exRate["JF_BaseRate"] = baseRate;
			exRate["JF_JH"] = job.PK;
			return exRate;
		}

		void SetVoyageExchangeRate(JobVoyage voyage, ZString currency, ZDecimal rate)
		{
			VoyageExRate rateToUpdate = null;
			foreach (VoyageExRate voyageExRate in voyage.ExRates)
			{
				if (voyageExRate.E8_RX_NKExCurrency == currency)
				{
					rateToUpdate = voyageExRate;
					break;
				}
			}

			if (rateToUpdate == null)
			{
				rateToUpdate = voyage.ExRates.AddNew();
				rateToUpdate.E8_RX_NKExCurrency = currency;
			}

			rateToUpdate.E8_VoyageExchangeRate = rate;
		}

		#endregion
		#endregion
		#region Implementation
		JobSailing CreateSailing(ZString vessel, ZString voyageFlight, ZString load, ZString discharge, ZDateTime etd, ZDateTime eta, OrgHeader carrier)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel;
			voyage.JV_VoyageFlight = voyageFlight;
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_OH_Line = carrier.PK;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = etd;
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;
			destination.JB_E_ARV = eta;
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			sailing.JX_IsPublished = true;
			return sailing;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RollBookingApplicator(Factory);
		}
		#endregion

		new RollBookingApplicator Applicator => (RollBookingApplicator)base.Applicator;
	}
}
