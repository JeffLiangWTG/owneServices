using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[HttpContextEnabledTest]
	sealed class TrackingShipmentTest : ForwardingShipmentTest
	{
		#region TestRelatedOrg

		public void TestRelatedOrg()
		{
			var testNotifier = (IBizOChangesEmailNotification)TestShipment;

			Assert(!TestShipment.IsImport());
			AssertNull(testNotifier.RelatedOrg);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();

			TestShipment.ConsignorDocumentaryAddress.OrganisationPK = org1.PK;
			TestShipment.ConsigneeDocumentaryAddress.OrganisationPK = org2.PK;
			AssertEquals(org1, testNotifier.RelatedOrg);

			TestShipment.ConsignorPickupAddress.OrganisationPK = org3.PK;
			AssertEquals(org3, testNotifier.RelatedOrg);

			TestShipment.JS_RL_NKOrigin = "MYPKG";
			TestShipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Assert(TestShipment.IsImport());
			AssertEquals(org2, testNotifier.RelatedOrg);

			TestShipment.ConsigneeDocumentaryAddress.OrganisationPK = org4.PK;
			AssertEquals(org4, testNotifier.RelatedOrg);
		}

		#endregion

		#region Setup & Control Overrides

		protected override CommonShipment GetShipment()
		{
			return (CommonShipment)Factory.New(typeof(TrackingShipment));
		}

		protected override Order GetOrder()
		{
			return Factory.NewWithValidTestData<TrackingOrder>();
		}

		protected override ForwardingConsol GetConsol()
		{
			return (ForwardingConsol)Factory.New(typeof(TrackingConsol));
		}

		TrackingShipment TestShipment
		{
			get
			{
				if (testShipment == null)
				{
					testShipment = (TrackingShipment)GetShipment();
				}
				return testShipment;
			}
		}
		TrackingShipment testShipment;

		OrgHeader TestOrg
		{
			get
			{
				if (testOrg == null)
				{
					testOrg = Factory.NewWithValidTestData<OrgHeader>();
				}
				return testOrg;
			}
		}
		OrgHeader testOrg;

		OrgAddress TestOrgAddress
		{
			get
			{
				if (testOrgAddress == null)
				{
					testOrgAddress = Factory.New<OrgAddress>();
					testOrgAddress.OA_Address1 = "Test Address Line 1";
					testOrgAddress.OA_Address2 = "Test Address Line 2";
					testOrgAddress.OA_City = "Test Address City";
				}
				return testOrgAddress;
			}
		}

		OrgAddress testOrgAddress;

		JobDocAddress TestJobDocAddress
		{
			get
			{
				if (testJobDocAddress == null)
				{
					testJobDocAddress = Factory.New<JobDocAddress>();
					testJobDocAddress.E2_OA_Address = TestOrgAddress.PK;
				}
				return testJobDocAddress;
			}
		}

		JobDocAddress testJobDocAddress;

		OrgContact TestContact
		{
			get
			{
				if (testContact == null)
				{
					testContact = TestOrg.Contacts.AddNew();
				}
				return testContact;
			}
		}
		OrgContact testContact;

		protected override Type ExpectedContainerType
		{
			get { return typeof(TrackingContainer); }
		}

		protected override void TearDown()
		{
			SuppressionForTest.CacheObjectClear();
			base.TearDown();
		}

		#endregion Setup & Control Overrides

		#region Related BizO Tests

		public void TestRelatedBizOReturnCorrectType()
		{
			AssertEquals("", typeof(TrackingConsol), TestShipment.Consols.TypeOfElements);
			AssertEquals("", typeof(TrackingShipment), TestShipment.CoLoadShipments.TypeOfElements);
			AssertEquals("", typeof(TrackingPackLine), TestShipment.OuterPackLines.TypeOfElements);
		}

		#endregion Related BizO Tests

		public void TestJobDocsAndCartage()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();
			AssertNotNull(shipment.DocsAndCartage);
		}

		#region Orders

		public void TestOrders()
		{
			AssertNotNull(TestShipment.AttachedOrders);
		}

		#endregion Orders

		#region Order reference

		public void TestOrderReference()
		{
			AssertEquals("Initially order reference is null", ZString.Empty, TestShipment.OrderReference);

			Order order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_OrderNumber = "AO1";
			Order order2 = Factory.NewWithValidTestData<Order>();
			order2.JD_OrderNumber = "AO2";

			order1.JD_JS = TestShipment.PK;
			AssertEquals(1, TestShipment.AttachedOrders.Count);
			AssertEquals("AO1", TestShipment.OrderReference);

			order2.JD_JS = TestShipment.PK;
			AssertEquals(2, TestShipment.AttachedOrders.Count);
			AssertEquals("AO1, AO2", TestShipment.OrderReference);

			TestShipment.DocsAndCartage.JP_OrderItemsAsString = "Orders1, Orders2";
			AssertEquals("if only OrderRef set, display it", "Orders1, Orders2", TestShipment.OrderReference);

			BaseJobDeclaration expDeclaration = Factory.New<BaseJobDeclaration>();
			expDeclaration.JE_MessageType = "EXP";
			expDeclaration.JE_JS = TestShipment.PK;
			expDeclaration.JE_OwnerRef = "EXP";

			AssertEquals("if OrderRef on dec is set, display", "Orders1, Orders2", TestShipment.OrderReference);
			AssertEquals("if OwnerRef on dec is set, display", "EXP", TestShipment.OwnerReference);

			BaseJobDeclaration impDeclaration = Factory.New<BaseJobDeclaration>();
			impDeclaration.JE_MessageType = "IMP";
			impDeclaration.JE_GB = Factory.NewWithValidTestData<GlbCompany>().Branches.AddNew().PK;
			impDeclaration.JE_JS = TestShipment.PK;
			impDeclaration.JE_OwnerRef = "IMP";
			AssertEquals("if OrderRef on dec is set, display", "Orders1, Orders2", TestShipment.OrderReference);
			AssertEquals("if OwnerRef on dec is set, display", "IMP", TestShipment.OwnerReference);

			impDeclaration.JE_OwnerRef = "IMP,Orders1,Orders2";
			AssertEquals("OwnerRef, display", "IMP,Orders1,Orders2", TestShipment.OwnerReference);
		}
		#endregion

		#region Last declaration

		public void TestLastDeclaration()
		{
			BaseJobDeclaration expDeclaration = Factory.New<BaseJobDeclaration>();
			expDeclaration.JE_MessageType = "EXP";
			expDeclaration.JE_JS = TestShipment.PK;

			AssertSame(expDeclaration, TestShipment.LastDeclaration);

			BaseJobDeclaration impDeclaration = Factory.New<BaseJobDeclaration>();
			impDeclaration.JE_MessageType = "IMP";
			impDeclaration.JE_GB = Factory.NewWithValidTestData<GlbCompany>().Branches.AddNew().PK;
			impDeclaration.JE_JS = TestShipment.PK;

			AssertSame("if both import and export decs exist, take Import", impDeclaration, TestShipment.LastDeclaration);
		}
		#endregion

		#region Test additional points

		public void TestPickupAddressAsTest()
		{
			TrackingShipment shipmentWithoutPickupAddress = Factory.New<TrackingShipment>();
			AssertEquals("PickupAddressAsText is empty", ZString.Empty, shipmentWithoutPickupAddress.PickupAddressAsText);

			shipmentWithoutPickupAddress.DocAddresses.FindOrCreateWithRequirement(shipmentWithoutPickupAddress.ConsignorPickupDeliveryAddressRequirement).E2_OA_Address = TestOrgAddress.PK;
			AssertEquals("PickupAddressAsText", TestJobDocAddress.AddressAsASingleLine, shipmentWithoutPickupAddress.PickupAddressAsText);
			AssertNotEquals("PickupAddressAsText should not be empty now", ZString.Empty, shipmentWithoutPickupAddress.PickupAddressAsText);
		}

		public void TestDeliveryAddressAsTest()
		{
			TrackingShipment shipmentWithoutDeliveryAddress = Factory.New<TrackingShipment>();
			AssertEquals("DeliveryAddressAsText is empty", ZString.Empty, shipmentWithoutDeliveryAddress.DeliveryAddressAsText);

			shipmentWithoutDeliveryAddress.DocAddresses.FindOrCreateWithRequirement(shipmentWithoutDeliveryAddress.ConsigneePickupDeliveryAddressRequirement).E2_OA_Address = TestOrgAddress.PK;
			AssertEquals("DeliveryAddressAsText", TestJobDocAddress.AddressAsASingleLine, shipmentWithoutDeliveryAddress.DeliveryAddressAsText);
			AssertNotEquals("DeliveryAddressAsText should not be empty now", ZString.Empty, shipmentWithoutDeliveryAddress.DeliveryAddressAsText);
		}

		public void TestAvailableDate()
		{
			TestShipment.JS_RL_NKOrigin = "AUSYD";
			TestShipment.JS_RL_NKDestination = "USLAX";

			ZDateTime date1 = new ZDateTime(2005, 8, 5);
			ZDateTime date2 = new ZDateTime(2005, 7, 5);
			ZDateTime date3 = new ZDateTime(2005, 6, 5);
			ZDateTime date4 = new ZDateTime(2005, 5, 5);

			AssertEquals(ZDateTime.Empty, TestShipment.AvailableDate);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "ABC";
			transport.JW_Vessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_Code;
			transport.JW_DepotAvailabilityDate = date1;
			transport.JW_TerminalAvailabilityDate = date3;

			TestShipment.Consols.Add(consol);

			AssertNotNull(TestShipment.ArrivalConsol);

			TestShipment.JS_PackingMode = Core.Constants.ContainerModes.AIR;
			AssertEquals(date1, TestShipment.AvailableDate);

			TestShipment.DocsAndCartage.JP_LCLAvailable = date2;
			AssertEquals(date2, TestShipment.AvailableDate);

			TestShipment.JS_PackingMode = Core.Constants.ContainerModes.Combination;
			AssertEquals(date3, TestShipment.AvailableDate);

			TestShipment.DocsAndCartage.JP_FCLAvailable = date4;
			AssertEquals(date4, TestShipment.AvailableDate);
		}

		public void TestStorageDate()
		{
			TestShipment.JS_RL_NKOrigin = "AUSYD";
			TestShipment.JS_RL_NKDestination = "USLAX";

			ZDateTime date1 = new ZDateTime(2005, 8, 5);
			ZDateTime date2 = new ZDateTime(2005, 7, 5);
			ZDateTime date3 = new ZDateTime(2005, 6, 5);
			ZDateTime date4 = new ZDateTime(2005, 5, 5);

			AssertEquals(ZDateTime.Empty, TestShipment.StorageDate);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "ABC";
			transport.JW_Vessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_Code;
			transport.JW_DepotStorageDate = date1;
			transport.JW_TerminalStorageDate = date3;
			TestShipment.Consols.Add(consol);

			AssertNotNull(TestShipment.ArrivalConsol);

			TestShipment.JS_PackingMode = Core.Constants.ContainerModes.AIR;
			AssertEquals(date1, TestShipment.StorageDate);

			TestShipment.DocsAndCartage.JP_LCLStorageCommences = date2;
			AssertEquals(date2, TestShipment.StorageDate);

			TestShipment.JS_PackingMode = Core.Constants.ContainerModes.Combination;
			AssertEquals(date3, TestShipment.StorageDate);

			TestShipment.DocsAndCartage.JP_FCLStorageCommences = date4;
			AssertEquals(date4, TestShipment.StorageDate);
		}

		#endregion

		#region DischargeETA and LoadETD

		public void TestLoadETD()
		{
			bool previousState = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				ZDateTime testDate = new ZDateTime(2006, 7, 28);

				TrackingShipment shipment1 = GetShipmentWithAttachedConsolAndTransport("AUSYD", "USLAX", "ABC", "ORIENTAL PHOENIX", testDate,
																										 Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL);

				AssertNotNull("Shipment should have a departure consol", shipment1.DepartureConsol);
				AssertEquals("Shipment LoadETD should be the ETD of the departure transport on the departure consol", testDate.ToShortDateString(),
							 shipment1.LoadETDWithSuppression.ToShortDateString());
			}
			finally
			{
				Globals.IsWeb = previousState;
			}
		}

		#endregion

		#region AvailableAtAddressAsText

		public void TestAvailableAtAddressAsText()
		{
			TrackingShipment shipment = (TrackingShipment)GetShipment();
			TrackingConsol consol = shipment.Consols.AddNew();
			AssertEquals("One Consol", 1, shipment.Consols.Count);
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			OrgAddress orgAdr1 = Factory.NewWithValidTestData<OrgAddress>();
			OrgAddress orgAdr2 = Factory.NewWithValidTestData<OrgAddress>();

			Assert(shipment.JS_OA_ImportReleaseDepot.IsEmpty);

			consol.JK_OA_UnpackDepotAddress = ZGuid.Empty;
			AssertNull("Consol.UnpackDepotAddress", consol.UnpackDepotAddress);
			AssertEquals("Should be an empty string", ZString.Empty, shipment.AvailableAtAddressAsText);

			consol.JK_OA_UnpackDepotAddress = orgAdr1.PK;
			AssertNotNull("Consol.UnpackDepotAddress", consol.UnpackDepotAddress);
			AssertEquals("Should be Consol Address", orgAdr1.AddressAsASingleLine, shipment.AvailableAtAddressAsText);

			shipment.JS_OA_ImportReleaseDepot = orgAdr2.PK;
			AssertEquals("Should be Consol Address", orgAdr2.AddressAsASingleLine, shipment.AvailableAtAddressAsText);
		}

		#endregion

		#region Suppressed Fields

		TrackingShipment GetShipmentWithAttachedConsolAndTransport(ZString loadPort, ZString destPort, ZString voyageFlight, ZString vessel, ZDateTime depDate, ZString transportMode, ZString containerMode)
		{
			TrackingShipment result = (TrackingShipment)GetShipment();
			result.JS_E_DEP = depDate;
			result.JS_TransportMode = transportMode;
			result.JS_PackingMode = containerMode;

			TrackingConsol consol = result.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = transportMode;

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = loadPort;
			transport.JW_ETD = depDate;

			return result;
		}

		public void TestSuppressedFields()
		{
			bool initialValue = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				SuppressionForTest.CacheObjectClear();
				SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForDomestic, false);

				RefVessel testVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
				AssertNotNull("Should have loaded a vessel", testVessel);
				Assert("VesselName should not be empty", !testVessel.RV_Code.IsEmpty);

				ZDateTime tomorrow = ZDateTime.Now.AddDays(1);
				TrackingShipment shipment1 = GetShipmentWithAttachedConsolAndTransport("AUSYD", "USLAX", "ABC", testVessel.RV_Code, tomorrow,
					Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL);

				AssertNotNull(shipment1.DepartureConsol);
				shipment1.DepartureConsol.Transports.DepartureTransport.JW_ETA = tomorrow;

				AssertEquals(tomorrow.ToShortDateString(), shipment1.LoadETDWithSuppression.ToShortDateString());
				AssertEquals(tomorrow.ToShortDateString(), shipment1.ETDWithSuppression.ToShortDateString());
				AssertEquals(tomorrow.ToShortDateString(), shipment1.DischargeETAWithSuppression.ToShortDateString());

				TrackingShipment shipment2 = GetShipmentWithAttachedConsolAndTransport("AUSYD", "USLAX", "ABC", testVessel.RV_Code, tomorrow,
					Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.AIR);

				AssertNotNull(shipment2.DepartureConsol);
				shipment2.DepartureConsol.Transports.DepartureTransport.JW_ETA = tomorrow;

				AssertEquals(tomorrow.ToLongTimeString(), shipment2.LoadETDWithSuppression.ToLongTimeString());
				AssertEquals(tomorrow.ToLongTimeString(), shipment2.ETDWithSuppression.ToLongTimeString());
				AssertEquals(tomorrow.ToLongTimeString(), shipment2.DischargeETAWithSuppression.ToLongTimeString());

				SuppressionForTest.CacheObjectClear();
				SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForDomestic, true);

				TrackingShipment shipment3 = GetShipmentWithAttachedConsolAndTransport("AUSYD", "USLAX", "ABC", testVessel.RV_Code, tomorrow,
					Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.AIR);

				AssertNotNull(shipment3.DepartureConsol);
				shipment3.DepartureConsol.Transports.DepartureTransport.JW_ETA = tomorrow;

				AssertEquals(Suppression.SuppressedDate, shipment3.LoadETDWithSuppression);
				AssertEquals(Suppression.SuppressedDate, shipment3.ETDWithSuppression);
				AssertEquals(Suppression.SuppressedDate, shipment3.DischargeETAWithSuppression);

				TrackingShipment shipment4 = GetShipmentWithAttachedConsolAndTransport("AUSYD", "USLAX", "ABC", testVessel.RV_Code, tomorrow,
					Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.LCL);

				AssertNotNull(shipment4.DepartureConsol);
				shipment4.DepartureConsol.Transports.DepartureTransport.JW_ETA = tomorrow;

				AssertEquals(tomorrow.ToShortDateString(), shipment4.LoadETDWithSuppression.ToShortDateString());
				AssertEquals(tomorrow.ToShortDateString(), shipment4.ETDWithSuppression.ToShortDateString());
				AssertEquals(tomorrow.ToShortDateString(), shipment4.DischargeETAWithSuppression.ToShortDateString());
			}
			finally
			{
				Globals.IsWeb = initialValue;
			}
		}

		#endregion

		#region DischargeETAWithSuppression

		public void TestDischargeETAWithSuppression()
		{
			bool initial = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				AssertEquals(ZDateTime.Empty, TestShipment.DischargeETAWithSuppression);

				TestShipment.JS_RL_NKOrigin = "AUSYD";
				TestShipment.JS_RL_NKDestination = "USLAX";
				TestShipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = Core.Constants.AgentType.Direct;
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USLAX";

				Transport transport = consol.Transports[0];
				transport.JW_VoyageFlight = "ABC";
				transport.JW_Vessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_Code;
				transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
				TestShipment.Consols.Add(consol);

				ZDateTime testDate = DateTime.Now.AddDays(1);
				transport.JW_ETA = testDate;

				AssertEquals(testDate.ToShortDateString(), TestShipment.DischargeETAWithSuppression.ToShortDateString());

				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				TestShipment.JS_TransportMode = Core.Constants.TransportModes.Air;

				AssertEquals(testDate, TestShipment.DischargeETAWithSuppression);

				SuppressionForTest.CacheObjectClear();
				SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForExport, true);

				AssertEquals(Suppression.SuppressedDate, TestShipment.DischargeETAWithSuppression);
			}
			finally
			{
				Globals.IsWeb = initial;
			}
		}

		#endregion

		#region FirstTransportLoadPort

		public void TestFirstTransportLoadPort()
		{
			AssertEquals(ZString.Empty, TestShipment.FirstTransportLoadPort);

			TestShipment.JS_RL_NKOrigin = "AUSYD";
			TestShipment.JS_RL_NKDestination = "USLAX";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUADL";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_VoyageFlight = "ABC";
			transport.JW_Vessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_Code;
			TestShipment.Consols.Add(consol);

			TestShipment.JS_PackingMode = Core.Constants.ContainerModes.AIR;

			AssertNotNull(TestShipment.DepartureConsol);
			AssertEquals("AUADL", TestShipment.FirstTransportLoadPort);
		}

		#endregion

		#region LastTransportDischargePort

		public void TestLastTransportDischargePort()
		{
			AssertEquals(ZString.Empty, TestShipment.LastTransportDischargePort);

			TestShipment.JS_RL_NKOrigin = "AUSYD";
			TestShipment.JS_RL_NKDestination = "USLAX";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUADL";
			consol.JK_RL_NKDischargePort = "USNYC";

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "ABC";
			transport.JW_Vessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_Code;
			TestShipment.Consols.Add(consol);

			TestShipment.JS_PackingMode = Core.Constants.ContainerModes.AIR;
			AssertNotNull(TestShipment.ArrivalConsol);
			AssertEquals("USNYC", TestShipment.LastTransportDischargePort);
		}

		#endregion

		#region TestDirectConsolPKAdded

		public void TestDirectConsolPKAdded()
		{
			TrackingShipment shipment = Factory.NewWithValidTestData<TrackingShipment>();
			TrackingConsol consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AssertEquals(0, shipment.DocRelatedPKs.Count);
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals(1, shipment.DocRelatedPKs.Count);
			AssertEquals(consol.PK, shipment.DocRelatedPKs[0]);
		}

		#endregion

		#region TestRelatedTransportBookingPKsAdded

		public void TestRelatedTransportBookingPKsAdded()
		{
			TrackingShipment shipment = Factory.NewWithValidTestData<TrackingShipment>();
			AssertEquals(0, shipment.DocRelatedPKs.Count);

			var consolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidation.KB_ParentID = shipment.PK;
			consolidation.KB_ParentTableCode = shipment.TablePrefix;

			var booking1 = Factory.NewWithValidTestData<DtbBooking>();
			var booking2 = Factory.NewWithValidTestData<DtbBooking>();
			var booking3 = Factory.NewWithValidTestData<DtbBooking>();

			consolidation.Bookings.AddRange(new List<DtbBooking> { booking1, booking2, booking3 });

			Factory.Save();

			AssertEquals(3, shipment.DocRelatedPKs.Count);
			AssertContainsExactElementsInAnyOrder("DocRelatedPKs should return all 3 booking PKs", new[] { booking1.PK, booking2.PK, booking3.PK }, shipment.DocRelatedPKs);
		}

		#endregion

		#region TestRelatedWhsReceivePKAdded()

		public void TestRelatedWhsReceivePKAdded()
		{
			var shipment = Factory.NewWithValidTestData<TrackingShipment>();
			AssertEquals(0, shipment.DocRelatedPKs.Count);

			var receive = Factory.NewWithValidTestData<WhsReceive>();

			var receiveJobPivot = Factory.NewWithValidTestData<WhsDocketJobPivot>();
			receiveJobPivot.WV_DocketType = receive.WD_DocketType;
			receiveJobPivot.WV_WD_Docket = receive.PK;
			receiveJobPivot.WV_ParentTableCode = shipment.TablePrefix;
			receiveJobPivot.WV_ParentId = shipment.PK;

			AssertEquals(1, shipment.DocRelatedPKs.Count);
			AssertContainsExactElementsInAnyOrder(new[] { receive.PK }, shipment.DocRelatedPKs);
		}

		#endregion

		#region TestMasterBillForDirectConsol

		public void TestMasterBillForDirectConsol()
		{
			TrackingShipment shipment = Factory.NewWithValidTestData<TrackingShipment>();
			shipment.JS_HouseBill = "1111111";
			TrackingConsol consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_MasterBillNum = "9999999";
			AssertEquals("1111111", shipment.JS_HouseBill);
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("9999999", shipment.JS_HouseBill);
		}

		#endregion

		#region ITemplateCopyable

		public void TestShouldCustomsDataBeCopied()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_JS = TestShipment.PK;

			var copiedShipment = (TrackingShipment)TestShipment.TemplateCopy();
			Assert(!copiedShipment.Declarations.Any());
		}

		[TestDate(2010, 1, 11)]
		public void TestTemplateCopy()
		{
			AssertNotNull(TestShipment.TemplateCopy());
			AssertEquals(typeof(TrackingShipment), TestShipment.TemplateCopy().GetType());

			TrackingShipment newShipment = TestShipment.TemplateCopy() as TrackingShipment;
			AssertNotNull(newShipment);
			AssertEquals(ZDateTime.Now, newShipment.JS_A_BKD);
			AssertEquals(true, newShipment.JS_IsBooking);
			AssertEquals(false, newShipment.JS_IsForwardRegistered);
		}

		public void TestTemplateCopy_DoesNotModifyJS_Phase()
		{
			TestShipment.JS_Phase = "TST";

			var newShipment = (TrackingShipment)TestShipment.TemplateCopy();
			AssertNotNull(newShipment);
			AssertEquals("ALL", newShipment.JS_Phase);
		}

		public void TestLoadingMetersAreNotCopied()
		{
			TestShipment.JS_LoadingMeters = 5.5;
			var packLine1 = TestShipment.OuterPackLines.Count > 0 ? TestShipment.OuterPackLines[0] : TestShipment.OuterPackLines.AddNew();
			packLine1.JL_LoadingMeters = 3.5;
			var packLine2 = TestShipment.OuterPackLines.Count > 1 ? TestShipment.OuterPackLines[1] : TestShipment.OuterPackLines.AddNew();
			packLine2.JL_LoadingMeters = 2.0;
			TestShipment.Factory.Save();

			var newShipment = (TrackingShipment)TestShipment.TemplateCopy();
			var newPackLine1 = newShipment.OuterPackLines[0];
			var newPackLine2 = newShipment.OuterPackLines[1];

			AssertEquals(default(ZDecimal), newShipment.JS_LoadingMeters);
			AssertEquals(default(ZDecimal), newPackLine1.JL_LoadingMeters);
			AssertEquals(default(ZDecimal), newPackLine2.JL_LoadingMeters);
		}

		public void TestStatusIsSetToBookedAfterCopying()
		{
			TestShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			TestShipment.Factory.Save();

			var newShipment = (TrackingShipment)TestShipment.TemplateCopy();
			AssertEquals(ShipmentStatusList.Codes.Booked, newShipment.JS_ShipmentStatus);
		}

		#endregion

		#region Containers

		public void TestContainersOnConsols()
		{
			var shipment1 = Factory.NewWithValidTestData<TrackingShipment>();
			shipment1.JS_IsForwardRegistered = false;
			shipment1.JS_IsCFSRegistered = false;

			var consol1 = shipment1.Consols.AddNew();
			var consol2 = shipment1.Consols.AddNew();
			var container1 = consol1.Containers.AddNew();
			var container2 = consol2.Containers.AddNew();

			var packLine = shipment1.OuterPackLines.AddNew();
			packLine.SetContainer(consol1, container1);
			packLine.SetContainer(consol2, container2);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { container1, container2 }, shipment1.ContainersOnConsols);

			shipment1.Consols.Remove(consol2);

			AssertContainsExactElementsInAnyOrder(new[] { container1 }, shipment1.ContainersOnConsols);
		}

		#endregion
	}
}
