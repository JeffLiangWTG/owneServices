using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.GlobalCommercialInvoice.Business;
using Enterprise.GlobalCommercialInvoice.Business.Test;
using Enterprise.Integration.Rating;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportConsignment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;
using ErrorReporter = CargoWise.Common.ErrorReporter;

namespace Enterprise.Freight.Business.Testing
{
	public class BuildConsolHelperTest : BaseFreightTest
	{
		#region PacklineUsage

		public void TestNoPackLines()
		{
			GenericPackLinesAndContainersTest(0, 3);
		}

		public void TestFewerPackLinesThanContainers()
		{
			GenericPackLinesAndContainersTest(2, 3);
		}

		public void TestEqualPackLinesAndContainers()
		{
			GenericPackLinesAndContainersTest(3, 3);
		}

		public void TestMorePackLinesThanContainers()
		{
			GenericPackLinesAndContainersTest(3, 2);
		}

		public void TestNoContainers()
		{
			GenericPackLinesAndContainersTest(3, 0);
		}

		public void GenericPackLinesAndContainersTest(int packLinesCount, int containersCount)
		{
			CommonShipment booking1 = CreateBooking();
			booking1.JS_TransportMode = Constants.TransportModes.Sea;
			booking1.JS_PackingMode = Constants.ContainerModes.FCL;

			IQuotedBooking quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(booking1.PK, Factory);

			RefContainer refCon = Factory.New<RefContainer>();
			refCon.RC_TareWeight = 2000;

			CommonContainer[] containers = new CommonContainer[containersCount];
			for (int i = 0; i < containers.Length; i++)
			{
				containers[i] = (CommonContainer)quotedBooking.QuotedBookingContainers.AddNew();
				containers[i].JC_ContainerCount = 1;
				containers[i].JC_RC = refCon.PK;
				containers[i].JC_GrossWeight = 10000;
				containers[i].JC_DepartureEstimatedPickup = ZDateTime.Today.AddDays(5);
				containers[i].JC_EmptyRequired = ZDateTime.Today;
			}

			PackLine[] packLines = new PackLine[packLinesCount];
			for (int i = 0; i < packLines.Length; i++)
			{
				packLines[i] = booking1.OuterPackLines.AddNew();
			}

			BuildConsolHelper helper = new BuildConsolHelper();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonConsol consol = (CommonConsol)newFactory.New<IForwardingConsol>();

			Factory.Save();
			helper.MakeConsolFromBookingOrStandaloneShipment(consol, booking1.PK);

			AssertEquals("Number of Shipments", 1, consol.Shipments.Count);

			CommonShipment shipment = consol.Shipments[0];
			AssertEquals("Number of Containers", containers.Length, shipment.Containers.Count());
			AssertEquals("Number of OuterPackLines", Math.Max(packLines.Length, containers.Length), shipment.OuterPackLines.Count);
		}

		public void TestMakeConsolFromBookingOrStandaloneShipment_ShouldNotAddContainer_WhenBookingContainerAlreadyExistsInConsolContainers()
		{
			var booking = CreateBooking();
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			booking.JS_PackingMode = Constants.ContainerModes.FCL;

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(booking.PK, Factory);

			var refCon = Factory.New<RefContainer>();
			refCon.RC_TareWeight = 2000;
			var containerNumber = "IRSU5676476";

			var container = (CommonContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = containerNumber;
			container.JC_RC = refCon.PK;

			var packLine = booking.OuterPackLines.AddNew();

			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			var consolContainer = consol.Containers.AddNew();
			consolContainer.JC_ContainerNum = containerNumber;
			consolContainer.JC_RC = refCon.PK;
			AssertEquals("Container packing lines before Convert", 0, consol.Containers[0].PackLines.Count);

			Factory.Save();

			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK);

			AssertNoExceptionThrown("Should save without any exception.", Factory.Save);
			AssertEquals("Expecting consol to have 1 container.", 1, consol.Containers.Count);
			AssertEquals("Expecting consol existing container to have 1 packing line.", 1, consol.Containers[0].PackLines.Count);
		}

		#endregion

		#region TestDontAllowConversionOfNonBookings

		public void TestDontAllowConversionOfNonBookings()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_IsBooking = false;
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CommonConsol consol = (CommonConsol)factory2.New<IForwardingConsol>();

			BuildConsolHelper helper = new BuildConsolHelper();

			try
			{
				helper.MakeConsolFromBookingOrStandaloneShipment(consol, shipment.PK);
				Fail("Should have thrown a NotSupportedException");
			}
			catch (NotSupportedException ex)
			{
				AssertEquals("This method can only be applied to bookings", ex.Message);
			}
		}

		#endregion

		#region TestBuildConsolFromPackContainers

		public void TestBuildConsolFromPackContainers()
		{
			AssertEquals("Precondition: Expecting CommonShipment 1 to have 2 outer packlines.", 2, Shipment1.OuterPackLines.Count);
			AssertEquals("Precondition: Expecting CommonShipment 2 to have 2 outer packlines.", 2, Shipment2.OuterPackLines.Count);
			AssertEquals("Expecting Shipment3 to have 3 outer packlines.", 3, Shipment3.OuterPackLines.Count);
			AssertEquals("Precondition: Expecting Container 1 to have 2 outer packlines.", 1, Container1.PackLines.Count);
			AssertEquals("Precondition: Expecting Container 2 to have 2 outer packlines.", 2, Container2.PackLines.Count);
			AssertEquals("Precondition: Expecting Container 3 to have 2 outer packlines.", 2, Container3.PackLines.Count);

			var selectedContainers = new CommonContainer[2];
			selectedContainers[0] = Container3;
			selectedContainers[1] = Container2;

			Helper = new BuildConsolHelper();

			CommonConsol newConsol = (CommonConsol)Factory.New<IForwardingConsol>();
			Helper.BuildConsolFromPackContainers(newConsol, selectedContainers, Sailing);

			CommonShipment shipment3InConsolsFactory = LoadShipmentInConsolsFactory(newConsol, Shipment3.PK);

			AssertEquals("Expecting Shipment3 to have 2 outer packlines.", 2, shipment3InConsolsFactory.OuterPackLines.Count);
			AssertEquals("Expecting Consol to have 2 containers.", 2, newConsol.Containers.Count);
			AssertEquals("Expecting Consol to have 2 shipments.", 2, newConsol.Shipments.Count);
			AssertEquals(2, newConsol.Containers[0].PackLines.Count);
			AssertEquals("Expecting Container's reference to the sailing to be empty.", ZGuid.Empty, newConsol.Containers[0].JC_JX);
			AssertEquals(2, newConsol.Containers[1].PackLines.Count);
			AssertEquals(2, newConsol.Shipments[0].OuterPackLines.Count);
			AssertEquals(2, newConsol.Shipments[1].OuterPackLines.Count);
			AssertEquals(newConsol.PK, newConsol.Containers[0].JC_JK);
			AssertEquals("Parent split CommonShipment should have JS_IsSplitShipment = true", true, Shipment3.JS_IsSplitShipment);

			Factory.Save();

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			BusinessObject[] shipments = loadingFactory.Load(typeof(CommonShipment), new ZQuery(JobShipmentSchema.JS_JS_SplitSwitchShipment, Shipment3.PK));
			Assert("Should have returned shipments from factory", shipments.Length > 0);

			CommonShipment splitShipment = shipments[0] as CommonShipment;
			AssertEquals("Expecting split CommonShipment to have 1 outer packline", 4, splitShipment.OuterPackLines.Count);
			AssertEquals("Parent split CommonShipment should have JS_IsSplitShipment = true", true, splitShipment.JS_IsSplitShipment);
		}

		#endregion

		#region TestBuildConsolFromSailing

		public void TestBuildConsolFromSailing()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			SailingsForTestClasses sailingsHelper = new SailingsForTestClasses(newFactory);

			var shippingLine = newFactory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_IsShippingProvider = true;
			shippingLine.OH_IsShippingLine = true;
			shippingLine.OH_FullName = "ShippingLine";
			shippingLine.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			JobSailing exportSailing = sailingsHelper.SydLaxSailing;
			exportSailing.Voyage.JV_OH_Line = shippingLine.PK;

			CommonShipment booking1 = CreateBooking(newFactory);
			CommonShipment booking2 = CreateBooking(newFactory);
			CommonShipment booking3 = CreateBooking(newFactory);

			booking1.JS_JX = exportSailing.PK;
			booking2.JS_JX = exportSailing.PK;
			booking3.JS_JX = exportSailing.PK;

			newFactory.Save();

			BuildConsolHelper consolHelper = new BuildConsolHelper();
			CommonConsol newConsol = (CommonConsol)newFactory.New<IForwardingConsol>();
			consolHelper.BuildConsolFromSailing(newConsol, exportSailing);

			AssertEquals("Expecting consol to have 1 transport", 1, newConsol.Transports.Count);
			AssertEquals("Expecting sailing of consol to be set to selected sailing.", exportSailing.PK, newConsol.Transports[0].JW_JX);
			AssertEquals("Expecting transport mode of consol to be set to sailing's transport mode.", exportSailing.Voyage.JV_AirSeaRoad, newConsol.JK_TransportMode);
			AssertEquals("Expecting consol's carrier to be sailing's carrier.", shippingLine.PK, newConsol.ShippingLinePK);

			AssertEquals("Expecting Booking1 to be converted to a shipment.", ZBool.True, booking1.JS_IsBooking);
			AssertEquals("Expecting Booking1 to be converted to a shipment.", ZBool.True, booking1.JS_IsForwardRegistered);
			AssertEquals("Expecting etd to be set on booking1", newConsol.JK_JX_JA_E_DEP, booking1.JS_E_DEP);
			AssertEquals("Expecting eta to be set on booking1", newConsol.JK_JX_JB_E_ARV, booking1.JS_E_ARV);

			AssertEquals("Expecting Booking2 to be converted to a shipment.", ZBool.True, booking2.JS_IsBooking);
			AssertEquals("Expecting Booking2 to be converted to a shipment.", ZBool.True, booking2.JS_IsForwardRegistered);
			AssertEquals("Expecting etd to be set on booking2", newConsol.JK_JX_JA_E_DEP, booking2.JS_E_DEP);
			AssertEquals("Expecting eta to be set on booking2", newConsol.JK_JX_JB_E_ARV, booking2.JS_E_ARV);

			AssertEquals("Expecting Booking3 to be converted to a shipment.", ZBool.True, booking3.JS_IsBooking);
			AssertEquals("Expecting Booking3 to be converted to a shipment.", ZBool.True, booking3.JS_IsForwardRegistered);
			AssertEquals("Expecting etd to be set on booking3", newConsol.JK_JX_JA_E_DEP, booking3.JS_E_DEP);
			AssertEquals("Expecting eta to be set on booking3", newConsol.JK_JX_JB_E_ARV, booking3.JS_E_ARV);
		}

		public void TestBuildConsolFromSailingResetsIsNeutralMasterIfNotAir()
		{
			var factory = new BusinessObjectFactory();
			var sailingsHelper = new SailingsForTestClasses(factory);

			var shippingLine = factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_IsShippingProvider = true;
			shippingLine.OH_IsShippingLine = true;
			shippingLine.OH_FullName = "ShippingLine";
			shippingLine.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var exportSailing = sailingsHelper.SydLaxSailing;
			exportSailing.Voyage.JV_OH_Line = shippingLine.PK;
			exportSailing.Voyage.JV_AirSeaRoad = "SEA";

			var seaBooking = CreateBooking(factory);
			seaBooking.JS_JX = exportSailing.PK;

			factory.Save();

			var consolHelper = new BuildConsolHelper();
			var newConsol = (CommonConsol)factory.New<IForwardingConsol>();
			newConsol.Transports[0].JW_IsLinked = false;
			newConsol.JK_IsNeutralMaster = true;

			consolHelper.BuildConsolFromSailing(newConsol, exportSailing);

			AssertEquals("Expecting consol to have 1 transport", 1, newConsol.Transports.Count);
			AssertEquals("Expecting sailing of consol to be set to selected sailing.", exportSailing.PK, newConsol.Transports[0].JW_JX);
			AssertEquals("Expecting transport to be linked.", ZBool.True, newConsol.Transports[0].JW_IsLinked);
			AssertEquals("Expecting transport mode of consol to be set to sailing's transport mode.", exportSailing.Voyage.JV_AirSeaRoad, newConsol.JK_TransportMode);
			AssertEquals("Expecting consol's carrier to be sailing's carrier.", shippingLine.PK, newConsol.ShippingLinePK);
			AssertEquals("Expecting IsNeutralMaster to be reset", ZBool.False, newConsol.JK_IsNeutralMaster);
		}

		public void TestBuildConsolFromSailingDoesNotResetIsNeutralMasterIfAir()
		{
			var factory = new BusinessObjectFactory();
			var sailingsHelper = new SailingsForTestClasses(factory);

			var airline = factory.NewWithValidTestData<OrgHeader>();
			airline.OH_IsAirLine = true;
			airline.OH_FullName = "Airline";
			airline.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var exportSailing = sailingsHelper.SydLaxSailing;
			exportSailing.Voyage.JV_OH_Line = airline.PK;
			exportSailing.Voyage.JV_AirSeaRoad = "AIR";

			var airBooking = CreateBooking(factory);
			airBooking.JS_JX = exportSailing.PK;

			factory.Save();

			var consolHelper = new BuildConsolHelper();
			var newConsol = (CommonConsol)factory.New<IForwardingConsol>();
			newConsol.Transports[0].JW_IsLinked = false;
			newConsol.JK_IsNeutralMaster = true;

			consolHelper.BuildConsolFromSailing(newConsol, exportSailing);

			AssertEquals("Expecting consol to have 1 transport", 1, newConsol.Transports.Count);
			AssertEquals("Expecting sailing of consol to be set to selected sailing.", exportSailing.PK, newConsol.Transports[0].JW_JX);
			AssertEquals("Expecting transport to be linked.", ZBool.True, newConsol.Transports[0].JW_IsLinked);
			AssertEquals("Expecting transport mode of consol to be set to sailing's transport mode.", exportSailing.Voyage.JV_AirSeaRoad, newConsol.JK_TransportMode);
			AssertEquals("Expecting consol's carrier to be sailing's carrier.", airline.PK, newConsol.ShippingLinePK);
			AssertEquals("Expecting IsNeutralMaster to be true", ZBool.True, newConsol.JK_IsNeutralMaster);
		}

		public void TestBuildConsolFromSailingSetsCFS_LCL()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			SailingsForTestClasses sailingsHelper = new SailingsForTestClasses(newFactory);
			JobSailing exportSailing = sailingsHelper.SydLaxSailing;
			CommonShipment booking1 = CreateBooking(newFactory);

			OrgAddress cFS = newFactory.NewWithValidTestData<OrgAddress>();
			booking1.JS_OA_ExportReceivingDepot = cFS.PK;

			booking1.JS_TransportMode = Constants.TransportModes.Sea;
			booking1.JS_PackingMode = Constants.ContainerModes.LCL;
			booking1.JS_JX = exportSailing.PK;

			newFactory.Save();

			BuildConsolHelper consolHelper = new BuildConsolHelper();
			CommonConsol newConsol = (CommonConsol)Factory.New<IForwardingConsol>();

			consolHelper.MakeConsolFromBookingOrStandaloneShipment(newConsol, booking1.PK);

			AssertEquals("Expecting consol's pack cfs to be Booking's cfs.", booking1.JS_OA_ExportReceivingDepot, newConsol.JK_OA_PackDepotAddress);
		}

		public void TestAirConsolWouldBeCreatedFromCourierBooking()
		{
			CommonShipment booking = CreateBooking(Factory);
			booking.JS_TransportMode = Constants.TransportModes.Courier;
			booking.JS_PackingMode = Constants.ContainerModes.OnBoardCourier;

			Factory.Save();

			BuildConsolHelper consolHelper = new BuildConsolHelper();
			CommonConsol newConsol = (CommonConsol)Factory.New<IForwardingConsol>();

			consolHelper.MakeConsolFromBookingOrStandaloneShipment(newConsol, booking.PK);

			AssertEquals(Constants.TransportModes.Air, newConsol.JK_TransportMode);
			AssertEquals(Constants.ContainerModes.Other, newConsol.JK_ConsolMode);

			booking.JS_TransportMode = Constants.TransportModes.Air;
			booking.JS_PackingMode = newConsol.JK_ConsolMode_List[0].Code;
			consolHelper.MakeConsolFromBookingOrStandaloneShipment(newConsol, booking.PK);
			AssertEquals(booking.JS_PackingMode, newConsol.JK_ConsolMode);
		}

		public void TestBuildConsolFromSailingSetsCFS_LTL()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			SailingsForTestClasses sailingsHelper = new SailingsForTestClasses(newFactory);
			JobSailing exportSailing = sailingsHelper.SydLaxSector;
			CommonShipment booking1 = CreateBooking(newFactory);

			OrgAddress cFS = newFactory.NewWithValidTestData<OrgAddress>();
			booking1.JS_OA_ExportReceivingDepot = cFS.PK;

			booking1.JS_TransportMode = Constants.TransportModes.Road;
			booking1.JS_PackingMode = Constants.ContainerModes.LTL;
			booking1.JS_JX = exportSailing.PK;

			newFactory.Save();

			BuildConsolHelper consolHelper = new BuildConsolHelper();
			CommonConsol newConsol = (CommonConsol)newFactory.New<IForwardingConsol>();

			consolHelper.MakeConsolFromBookingOrStandaloneShipment(newConsol, booking1.PK);

			AssertEquals("Expecting consol's pack cfs to be Booking's cfs.", booking1.JS_OA_ExportReceivingDepot, newConsol.JK_OA_PackDepotAddress);
		}

		public void TestBuildConsolFromSailingSetsCFS_FTL()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			SailingsForTestClasses sailingsHelper = new SailingsForTestClasses(newFactory);
			JobSailing exportSailing = sailingsHelper.SydLaxSector;
			CommonShipment booking1 = CreateBooking(newFactory);

			OrgAddress cFS = newFactory.NewWithValidTestData<OrgAddress>();
			booking1.JS_OA_ExportReceivingDepot = cFS.PK;

			booking1.JS_TransportMode = Constants.TransportModes.Road;
			booking1.JS_PackingMode = Constants.ContainerModes.FTL;
			booking1.JS_JX = exportSailing.PK;

			newFactory.Save();

			BuildConsolHelper consolHelper = new BuildConsolHelper();
			CommonConsol newConsol = (CommonConsol)newFactory.New<IForwardingConsol>();

			consolHelper.MakeConsolFromBookingOrStandaloneShipment(newConsol, booking1.PK);

			AssertEquals("Expecting consol's pack cfs to be Booking's cfs.", booking1.JS_OA_ExportReceivingDepot, newConsol.JK_OA_PackDepotAddress);
		}

		public void TestBuildConsolFromSailingSetsCFS_FCL()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			SailingsForTestClasses sailingsHelper = new SailingsForTestClasses(newFactory);
			JobSailing exportSailing = sailingsHelper.SydLaxSailing;
			CommonShipment booking1 = CreateBooking(newFactory);

			OrgAddress cFS = newFactory.NewWithValidTestData<OrgAddress>();
			booking1.JS_OA_ExportReceivingDepot = cFS.PK;

			booking1.JS_TransportMode = Constants.TransportModes.Sea;
			booking1.JS_PackingMode = Constants.ContainerModes.FCL;
			booking1.JS_JX = exportSailing.PK;

			newFactory.Save();

			BuildConsolHelper consolHelper = new BuildConsolHelper();
			CommonConsol newConsol = (CommonConsol)newFactory.New<IForwardingConsol>();

			consolHelper.MakeConsolFromBookingOrStandaloneShipment(newConsol, booking1.PK);

			AssertEquals("Expecting consol's departure cfs to be Booking's cto.", booking1.JS_OA_ExportReceivingDepot, newConsol.JK_OA_DepartureCTOAddress);
		}

		public void TestBuildConsolFromSailingSetsCFS_AIR()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			SailingsForTestClasses sailingsHelper = new SailingsForTestClasses(newFactory);
			JobSailing exportSailing = sailingsHelper.SydLaxFlightLeg;
			CommonShipment booking1 = CreateBooking(newFactory);

			OrgAddress cFS = newFactory.NewWithValidTestData<OrgAddress>();
			booking1.JS_OA_ExportReceivingDepot = cFS.PK;
			booking1.JS_TransportMode = Constants.TransportModes.Air;
			booking1.JS_JX = exportSailing.PK;

			newFactory.Save();

			BuildConsolHelper consolHelper = new BuildConsolHelper();
			CommonConsol newConsol = (CommonConsol)newFactory.New<IForwardingConsol>();

			consolHelper.MakeConsolFromBookingOrStandaloneShipment(newConsol, booking1.PK);

			AssertEquals("Expecting consol's pack cfs to be Booking's cfs.", booking1.JS_OA_ExportReceivingDepot, newConsol.JK_OA_PackDepotAddress);
		}

		#endregion

		#region TestCheckForConsolidatedContainers

		public void TestCheckForConsolidatedContainers()
		{
			SailingsForTestClasses sailingsHelper = new SailingsForTestClasses(Factory);
			JobSailing sydLaxSailing = sailingsHelper.SydLaxSailing;
			BuildConsolHelper consolHelper = new BuildConsolHelper();
			Assert("Expecting to be able to build a consol from the containers.", !consolHelper.CheckHasConsolidatedContainer(sydLaxSailing.Containers.OfType<CommonContainer>().ToArray()));

			CommonContainer containerA = Factory.New<CommonContainer>();
			containerA.JC_JX = sydLaxSailing.PK;
			CommonContainer containerB = Factory.New<CommonContainer>();
			containerB.JC_JX = sydLaxSailing.PK;

			var selectedContainers = new CommonContainer[2];
			selectedContainers[0] = containerA;
			selectedContainers[1] = containerB;

			Assert("Expecting to be able to build a consol from the containers.", !consolHelper.CheckHasConsolidatedContainer(selectedContainers));

			containerB.JC_JK = ZGuid.NewZGuid();

			Assert("Not expecting to be able to build a consol from the containers.", consolHelper.CheckHasConsolidatedContainer(selectedContainers));
		}

		#endregion

		#region TestCheckCanBuildConsolFromSailing

		public void TestCheckCanBuildConsolFromSailing()
		{
			BuildConsolHelper consolHelper = new BuildConsolHelper();
			Assert("Not expecting to be able to build a consol from the sailing.", !consolHelper.CheckCanBuildConsolFromSailing(Sailing));

			SailingsForTestClasses sailingsHelper = new SailingsForTestClasses(Factory);

			consolHelper = new BuildConsolHelper();
			Assert("Expecting to be able to build a consol from the sailing.", consolHelper.CheckCanBuildConsolFromSailing(sailingsHelper.SydLaxSailing));
		}

		#endregion

		#region TestConvertToStandaloneShipmentContainsSailingDetails

		public void TestConvertToStandaloneShipmentContainsSailingDetails()
		{
			var voyage = Factory.New<JobVoyage>();
			var origin = Factory.New<VoyageOrigin>();
			origin.JA_JV = voyage.PK;
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			var destination = Factory.New<VoyageDestination>();
			destination.JB_JV = voyage.PK;
			destination.JB_RL_NKPortOfDischarge = "USLAX";

			var sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			CommonShipment booking = CreateBooking();
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			booking.JS_PackingMode = Constants.ContainerModes.Bulk;
			booking.JS_ActualWeight = 10000m;
			booking.JS_JX = sailing.PK;
			booking.JS_IsDirectBooking = false;
			Factory.Save();

			BuildConsolHelper helper = new BuildConsolHelper();
			helper.TurnBookingIntoShipment(booking, null);

			AssertEquals("Expect JS_JX to be equal after", sailing.PK, booking.JS_JX);
			AssertNotNull("Sailing is not null", booking.Sailing);
			AssertEquals("Expected Sailing Port of Loading to be accessible after conversion", "AUSYD", booking.Sailing.PortOfLoading.Code);
			AssertEquals("Expected Sailing Port of Discharge to be accessible after conversion", "USLAX", booking.Sailing.PortOfDischarge.Code);
		}

		#endregion

		#region TestAddBookingsToConsol

		public void TestAddBookingsToConsol()
		{
			var containerRef1 = Factory.New<RefContainer>();
			containerRef1.RC_Code = "20GM";
			containerRef1.RC_CubicCapacity = 16m;

			var containerRef2 = Factory.New<RefContainer>();
			containerRef2.RC_Code = "40GM";
			containerRef2.RC_CubicCapacity = 32m;

			var consolHelper = new BuildConsolHelper();

			var newFactory = new BusinessObjectFactory();
			var consol = (CommonConsol)newFactory.New<IForwardingConsol>();

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			var transport = consol.Transports[0];
			transport.JW_JX = Sailing.PK;

			consol.Containers.AddNew();
			consol.Containers[0].JC_RC = containerRef1.PK;
			consol.Containers.AddNew();
			consol.Containers[1].JC_RC = containerRef2.PK;

			var shipment1 = CreateBooking();
			shipment1.JS_ActualVolume = 25m;
			shipment1.JS_OuterPacks = 1;
			shipment1.JS_JX = Sailing.PK;

			var shipment2 = CreateBooking();
			shipment2.JS_ActualVolume = 12m;
			shipment2.JS_OuterPacks = 2;
			shipment2.JS_JX = Sailing.PK;

			var shipment3 = CreateBooking();
			shipment3.JS_ActualVolume = 5m;
			shipment3.JS_OuterPacks = 3;
			shipment3.JS_JX = Sailing.PK;

			var shipment4 = CreateBooking();
			shipment4.JS_ActualVolume = 3m;
			shipment4.JS_OuterPacks = 4;
			shipment4.JS_JX = Sailing.PK;

			Factory.Save();

			var selectedBookings = new ZGuid[]
			{
				shipment1.PK,
				shipment2.PK,
				shipment3.PK,
				shipment4.PK
			};

			consolHelper.AddBookingsToConsol(consol, selectedBookings);

			AssertEquals("Expecting 4 shipments on the consol", 4, consol.Shipments.Count);
			AssertEquals("Expecting 2 packlines in the 1st container", 2, consol.Containers[0].PackLines.Count);
			AssertEquals("Expecting 2 packlines in the 2nd container", 2, consol.Containers[1].PackLines.Count);
		}

		[GuiTest]
		public void TestAddBookingsToConsolTemplateLink()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var testHelper = ObjectFactory.Get<IBMTestHelper>();
			testHelper.CreateSystem(Factory, "CON", "SHP");

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "CON";
			var templateJobHeader1 = template1.ProcessHeaders[0];

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "SHP";
			var templateJobHeader2 = template2.ProcessHeaders[0];

			var templateLink1 = (IProcessHeaderLink)template1.ProcessHeaderLinks.AddNew();
			templateLink1.FP_FH_HeaderFrom = templateJobHeader1.PK;
			templateLink1.FP_FH_HeaderTo = templateJobHeader2.PK;
			templateLink1.FP_LinkType = "DEP";
			templateLink1.FromWorkflowExternalTemplatePK = template2.PK;
			Factory.Save();

			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			CommonShipment shipment = (CommonShipment)Factory.New<IForwardingShipment>();
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_ActualVolume = 25m;
			shipment.JS_OuterPacks = 1;
			shipment.JS_JX = Sailing.PK;

			var consolHelper = new BuildConsolHelper();
			var selectedBookings = new ZGuid[] { shipment.PK };

			consolHelper.AddBookingsToConsol(consol, selectedBookings);

			Factory.Save();

			var consolJobHeader = ProcessJobHeaderProvider.GetForParent((IWorkflowProvider)consol, Factory);
			var shipmentJobHeader = ProcessJobHeaderProvider.GetForParent((IWorkflowProvider)shipment, Factory);

			testHelper.AssertIsPrerequisite(consolJobHeader, shipmentJobHeader);
		}

		public void TestAddBookingsToConsol_ConsolMode()
		{
			BuildConsolHelper consolHelper = new BuildConsolHelper();

			BusinessObjectFactory consolFactory = new BusinessObjectFactory();
			CommonConsol consol1 = (CommonConsol)consolFactory.New<IForwardingConsol>();
			CommonConsol consol2 = (CommonConsol)consolFactory.New<IForwardingConsol>();

			Transport transport1 = consol1.Transports[0];
			transport1.JW_JX = Sailing.PK;

			consol1.Containers.AddNew();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_ConsolMode = "GRP";

			Transport transport2 = consol2.Transports[0];
			transport2.JW_JX = Sailing.PK;

			consol2.Containers.AddNew();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_ConsolMode = "GRP";

			CommonShipment booking1 = CreateBooking();
			booking1.JS_ActualVolume = 25m;
			booking1.JS_JX = Sailing.PK;
			booking1.JS_PackingMode = "LCL";

			CommonShipment booking2 = CreateBooking();
			booking2.JS_ActualVolume = 30m;
			booking2.JS_JX = Sailing.PK;
			booking2.JS_PackingMode = "LCL";

			Factory.Save();

			ZGuid[] selectedBookings1 = new ZGuid[]
			{
				booking1.PK,
			};

			consolHelper.AddBookingsToConsol(consol1, selectedBookings1);

			AssertEquals("Expecting 1 shipment on the consol", 1, consol1.Shipments.Count);
			AssertEquals("Consol is unsaved so should have taken Mode from Booking", "LCL", consol1.JK_ConsolMode);

			consolFactory.Save();

			ZGuid[] selectedBookings2 = new ZGuid[]
			{
				booking2.PK,
			};

			consolHelper.AddBookingsToConsol(consol2, selectedBookings2);

			AssertEquals("Expecting 1 shipment on the consol", 1, consol2.Shipments.Count);
			AssertEquals("Consol is already saved so Mode should be unchanged", "GRP", consol2.JK_ConsolMode);
		}

		public void TestAddBookingsToConsol_ShipmentContainerTypeIsSetFromDefaultContainerModeFromSystemRegistry()
		{
			var setupCollection = new DefaultContainerModesCollection();
			var registryItem = new DefaultContainerModes();
			registryItem.TransportMode = Constants.TransportModes.Courier;
			registryItem.ContainerMode = Constants.ContainerModes.Unaccompanied;
			setupCollection.Add(registryItem);

			FreightConfigurationRegistry.Instance.DefaultContainerModes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, setupCollection);

			CommonShipment booking = CreateBooking();
			booking.JS_TransportMode = Constants.TransportModes.Courier;
			booking.JS_PackingMode = Constants.ContainerModes.OnBoardCourier;

			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			BuildConsolHelper helper = new BuildConsolHelper();
			helper.AddBookingsToConsol(consol, new ZGuid[] { booking.PK });

			AssertEquals("Expecting converted shipment transport mode to be COU", Constants.TransportModes.Courier, booking.JS_TransportMode);
			AssertEquals("Expecting converted shipment packing mode to be UNA", Constants.ContainerModes.Unaccompanied, booking.JS_PackingMode);
		}

		public void TestAddBookingsToConsol_ShipmentAndQuotedBooking_OneToOneMatch()
		{
			var quotedBooking1 = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.BookingWithQuote, Factory);

			var customValue1 = Factory.New<GenCustomAddOnValue>();
			customValue1.XV_Type = "STR";
			customValue1.XV_Name = "Field 1";
			customValue1.XV_Data = "Test Value1";
			customValue1.XV_ParentID = quotedBooking1.ViewPK;
			customValue1.XV_ParentTableCode = ViewQuotedBookingSchema.Constants.Prefix;

			var customValue2 = Factory.New<GenCustomAddOnValue>();
			customValue2.XV_Type = "DAT";
			customValue2.XV_Name = "Field 2";
			customValue2.XV_Data = new ZDateTime(2023, 07, 07).ToString();
			customValue2.XV_ParentID = quotedBooking1.ViewPK;
			customValue2.XV_ParentTableCode = ViewQuotedBookingSchema.Constants.Prefix;

			var quotedBooking2 = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.BookingWithQuote, Factory);

			var customValue3 = Factory.New<GenCustomAddOnValue>();
			customValue3.XV_Type = "STR";
			customValue3.XV_Name = "Field 3";
			customValue3.XV_Data = "Test Value3";
			customValue3.XV_ParentID = quotedBooking2.ViewPK;
			customValue3.XV_ParentTableCode = ViewQuotedBookingSchema.Constants.Prefix;

			var quickBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory);

			var customValue4 = Factory.New<GenCustomAddOnValue>();
			customValue4.XV_Type = "STR";
			customValue4.XV_Name = "Field 4";
			customValue4.XV_Data = "Test Value4";
			customValue4.XV_ParentID = quickBooking.ViewPK;
			customValue4.XV_ParentTableCode = ViewQuotedBookingSchema.Constants.Prefix;

			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			consol.Containers.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = "GRP";

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			consol = newFactory.Load<CommonConsol>(consol.PK);

			new BuildConsolHelper().AddBookingsToConsol(consol, new[]
			{
				Tuple.Create(quotedBooking1.ForwardingShipment.PK, quotedBooking1.ViewPK),
				Tuple.Create(quotedBooking2.ForwardingShipment.PK, quotedBooking2.ViewPK),
				Tuple.Create(quickBooking.ForwardingShipment.PK, quickBooking.ViewPK),
			});

			var quotedBookingShipment1 = newFactory.Load<CommonShipment>(quotedBooking1.ForwardingShipment.PK);
			var quotedBookingShipment2 = newFactory.Load<CommonShipment>(quotedBooking2.ForwardingShipment.PK);
			var quickBookingShipment = newFactory.Load<CommonShipment>(quickBooking.ForwardingShipment.PK);

			AssertEquals(2, quotedBookingShipment1.GetUserDefinedValues().Count());
			AssertEquals("Test Value1", quotedBookingShipment1.GetUserDefinedProperty("Field 1", "STR").XV_Data);
			AssertEquals(new ZDateTime(2023, 07, 07).ToString(), quotedBookingShipment1.GetUserDefinedProperty("Field 2", "DAT").XV_Data);

			AssertEquals(1, quotedBookingShipment2.GetUserDefinedValues().Count());
			AssertEquals("Test Value3", quotedBookingShipment2.GetUserDefinedProperty("Field 3", "STR").XV_Data);

			AssertEquals(1, quickBookingShipment.GetUserDefinedValues().Count());
			AssertEquals("Test Value4", quickBookingShipment.GetUserDefinedProperty("Field 4", "STR").XV_Data);
		}

		[ExpectNoExceptions]
		public void TestAddBookingsToConsol_ShipmentCannotBeAttachedToConsol()
		{
			var booking = CreateBooking();
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			booking.JS_PackingMode = Constants.ContainerModes.FCL;
			booking.JS_RL_NKLoadPort = "AUSYD";
			booking.JS_RL_NKDischargePort = "CNSHA";
			booking.JS_UniqueConsignRef = "S00001";

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(booking.PK, Factory);
			var container = (CommonContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerCount = 1;
			container.JC_ContainerNum = "CT00001";

			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";

			var helper = new BuildConsolHelper();
			var isShipmentCannotBeAttachedToConsolCalled = false;
			helper.ShipmentCannotBeAttachedToConsol += (sender, args) => isShipmentCannotBeAttachedToConsolCalled = true;
			helper.MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newConsol = (CommonConsol)newFactory.New<IForwardingConsol>();
			newConsol.JK_RL_NKLoadPort = "CNSHA";
			newConsol.JK_RL_NKDischargePort = "AUSYD";
			helper.AddBookingsToConsol(newConsol, new[] { booking.PK });
			newFactory.Save();

			AssertEquals(consol.Shipments[0].PK, newConsol.Shipments[0].PK);
			AssertEquals(1, consol.Containers.Count);
			AssertEquals(0, newConsol.Containers.Count);
			AssertEquals(expected: false, isShipmentCannotBeAttachedToConsolCalled);

			var existingConsol = (CommonConsol)newFactory.New<IForwardingConsol>();
			existingConsol.JK_UniqueConsignRef = "C00001";
			existingConsol.JK_RL_NKLoadPort = "AUSYD";
			existingConsol.JK_RL_NKDischargePort = "CNSHA";
			newFactory.Save();
			helper.AddBookingsToConsol(existingConsol, new[] { booking.PK });
			newFactory.Save();

			AssertEquals(0, existingConsol.Shipments.Count);
			AssertEquals(expected: true, isShipmentCannotBeAttachedToConsolCalled);
		}

		public void TestAddBookingToConsol_WhenAddBookingWithCarrier_ThenCarrierShouldBePreservedAndCopiedToConsol()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			carrier.MainAddress.OA_Address1 = "Address 1";

			var booking = CreateBooking();
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			booking.JS_PackingMode = Constants.ContainerModes.FCL;
			booking.JS_RL_NKLoadPort = "AUSYD";
			booking.JS_RL_NKDischargePort = "CNSHA";
			booking.JS_UniqueConsignRef = "S00001";
			booking.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";

			var helper = new BuildConsolHelper();
			helper.AddBookingsToConsol(consol, new ZGuid[] { booking.PK });

			AssertEquals("Carrier on the booking should be preserved after adding to consol", carrier.MainAddress.PK, booking.JS_OA_BookedShippingLineAddress);
		}

		#endregion

		#region TestMakeConsolFromBooking

		public void TestMakeConsolFromBooking_Blk()
		{
			CommonShipment booking = CreateBooking();
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			booking.JS_PackingMode = Constants.ContainerModes.Bulk;
			booking.JS_ActualWeight = 10000m;
			booking.JS_JX = Sailing.PK;
			booking.JS_IsDirectBooking = false;

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonConsol newConsol = (CommonConsol)newFactory.New<IForwardingConsol>();

			BuildConsolHelper helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, booking.PK);

			CommonShipment loadedShipment = LoadShipmentInConsolsFactory(newConsol, booking.PK);
			Assert("Expecting JS_IsBooking to be true.", loadedShipment.JS_IsBooking);
			Assert("Expecting JS_IsForwardRegistered to be true.", loadedShipment.JS_IsForwardRegistered);

			AssertEquals("Expecting consol to be Agent, booking wasn't direct", Constants.AgentType.Agent, newConsol.JK_AgentType);
			AssertEquals("Expecting new consol to have one shipment.", 1, newConsol.Shipments.Count);

			AssertEquals("Expecting consol to be sea", Constants.TransportModes.Sea, newConsol.JK_TransportMode);
			AssertEquals("Expecting consol to be blk", "BLK", newConsol.JK_ConsolMode);
			AssertEquals("Expecting consol to have 1 transport", 1, newConsol.Transports.Count);
			AssertEquals("Expecting consol's load to be set", Sailing.JX_JA_RL_NKPortOfLoading, newConsol.JK_RL_NKLoadPort);
			AssertEquals("Expecting consol's discharge to be set", Sailing.JX_JB_RL_NKPortOfDischarge, newConsol.JK_RL_NKDischargePort);
			AssertEquals("Expecting consol's sailing to be sailing.", Sailing.PK, newConsol.Transports[0].JW_JX);
		}

		public void TestMakeConsolFromBooking_ContractNumberDefaulted()
		{
			var booking = CreateBooking();
			booking.JS_CarrierContractNumber = "WHAT";

			var helper = new BuildConsolHelper();
			var consol = Factory.New<IForwardingConsol>() as CommonConsol;

			helper.MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK);

			AssertEquals("Contract Number Defaulted", "WHAT", consol.JK_CarrierContractNumber);
		}

		public void TestMakeConsolFromBooking_AllocationRouteDefaulted()
		{
			var allocationRoute = Factory.New<IRatingContractAllocationLine>();

			var booking = CreateBooking();
			booking.JS_RCA_BookingAllocationLine = allocationRoute.PK;

			var helper = new BuildConsolHelper();
			var consol = Factory.New<IForwardingConsol>() as CommonConsol;

			helper.MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK);
			AssertNotEquals("Precondition: Allocation Line PK not empty", allocationRoute.PK, ZGuid.Empty);
			AssertEquals("Allocation line info has defaulted to consolidation", allocationRoute.PK, consol.JK_RCA_AllocationLine);
		}

		public void TestMakeConsolFromBooking_ConsolTypeIsSetToCourierWhenBookingIsCourier()
		{
			CommonShipment booking = CreateBooking();
			booking.JS_TransportMode = Constants.TransportModes.Courier;
			booking.JS_PackingMode = Constants.ContainerModes.Bulk;
			booking.JS_ActualWeight = 10000m;
			booking.JS_JX = Sailing.PK;
			booking.JS_IsDirectBooking = false;

			Factory.Save();

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			Assert(!consol.IsCourier);
			AssertNotEquals(Constants.ContainerModes.Other, consol.JK_ConsolMode);

			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK);
			Assert(consol.IsCourier);
			AssertEquals(Constants.ContainerModes.Other, consol.JK_ConsolMode);
		}

		public void TestMakeConsolFromBooking_ShipmentContainerTypeIsSetFromDefaultContainerModeFromSystemRegistry()
		{
			var setupCollection = new DefaultContainerModesCollection();
			var registryItem = new DefaultContainerModes();
			registryItem.TransportMode = Constants.TransportModes.Courier;
			registryItem.ContainerMode = Constants.ContainerModes.Unaccompanied;
			setupCollection.Add(registryItem);

			FreightConfigurationRegistry.Instance.DefaultContainerModes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, setupCollection);

			CommonShipment booking = CreateBooking();
			booking.JS_TransportMode = Constants.TransportModes.Courier;
			booking.JS_PackingMode = Constants.ContainerModes.OnBoardCourier;

			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			BuildConsolHelper helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK);

			AssertEquals("Expecting converted shipment transport mode to be COU", Constants.TransportModes.Courier, booking.JS_TransportMode);
			AssertEquals("Expecting converted shipment packing mode to be UNA", Constants.ContainerModes.Unaccompanied, booking.JS_PackingMode);
		}

		public void TestMakeConsolFromBooking_Fcl_NoSailing()
		{
			BusinessObjectFactory bookingFactory = new BusinessObjectFactory();
			OrgHeader carrier = bookingFactory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			carrier.MainAddress.OA_Address1 = "Address 1";

			CommonShipment booking = CreateBooking(bookingFactory);
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			booking.JS_PackingMode = Constants.ContainerModes.FCL;
			booking.JS_RL_NKDestination = "MAAGA";
			booking.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;
			booking.JS_CFSReference = "Carrier Ref.";
			booking.JS_IsDirectBooking = true;
			booking.JS_AWBServiceLevel = "D12";

			IQuotedBooking quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(booking.PK, bookingFactory);

			RefContainer refCon = bookingFactory.New<RefContainer>();
			refCon.RC_TareWeight = 2000;

			CommonContainer container = (CommonContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "GLMR1231111";
			container.JC_RC = refCon.PK;
			container.JC_GrossWeight = 10000;
			container.JC_DepartureEstimatedPickup = ZDateTime.Today.AddDays(5);
			container.JC_EmptyRequired = ZDateTime.Today;

			CommonContainer container2 = (CommonContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container2.JC_ContainerCount = 7;
			container2.JC_RC = refCon.PK;
			container2.JC_GrossWeight = 10000;
			container2.JC_DepartureEstimatedPickup = ZDateTime.Today.AddDays(5);
			container2.JC_EmptyRequired = ZDateTime.Today;
			bookingFactory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonConsol newConsol = (CommonConsol)newFactory.New<IForwardingConsol>();

			BuildConsolHelper helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, booking.PK);

			CommonShipment loadedShipment = LoadShipmentInConsolsFactory(newConsol, booking.PK);

			Assert("Expecting JS_IsBooking to be true.", loadedShipment.JS_IsBooking);
			Assert("Expecting JS_IsForwardRegistered to be true.", loadedShipment.JS_IsForwardRegistered);

			AssertEquals("Expecting service level to be true.", "D12", newConsol.JK_AWBServiceLevel);
			AssertEquals("Expecting consol to be direct", Constants.AgentType.Direct, newConsol.JK_AgentType);
			AssertEquals("Expecting new consol to have one shipment.", 1, newConsol.Shipments.Count);
			AssertEquals("Expecting consol to have two containers.", 2, newConsol.Containers.Count);

			AssertEquals("Expecting consol to be sea", Constants.TransportModes.Sea, newConsol.JK_TransportMode);
			AssertEquals("Expecting consol to be fcl", Constants.ContainerModes.FCL, newConsol.JK_ConsolMode);
			AssertEquals("Expecting consol to have 1 transport", 1, newConsol.Transports.Count);
			AssertEquals("Expecting consol's sailing to be empty.", ZGuid.Empty, newConsol.Transports[0].JW_JX);
			AssertEquals("Expecting consols' carrier to be carrier.", carrier.PK, newConsol.ShippingLinePK);
			AssertEquals("Expecting consol's booking ref to be carrier ref.", "Carrier Ref.", newConsol.JK_BookingReference);
		}

		public void TestMakeConsolFromDirectBooking_Fcl_ReceivingSendingAgentBlank()
		{
			var bookingFactory = new BusinessObjectFactory();
			var carrier = bookingFactory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			carrier.MainAddress.OA_Address1 = "Address 1";

			var consignee = bookingFactory.New<OrgHeader>();
			consignee.OH_IsShippingLine = true;
			consignee.OH_IsShippingProvider = true;
			consignee.OH_FullName = "Consignee";
			consignee.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consignee.MainAddress.OA_Address1 = "Address 1";

			var consignor = bookingFactory.New<OrgHeader>();
			consignor.OH_IsShippingLine = true;
			consignor.OH_IsShippingProvider = true;
			consignor.OH_FullName = "Consignor";
			consignor.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consignor.MainAddress.OA_Address1 = "Address 1";

			bookingFactory.Save();

			var booking = CreateBooking(bookingFactory);
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			booking.JS_PackingMode = Constants.ContainerModes.FCL;
			booking.JS_RL_NKDestination = "MAAGA";
			booking.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;
			booking.JS_CFSReference = "Carrier Ref.";
			booking.JS_IsDirectBooking = true;
			booking.JS_AWBServiceLevel = "D12";
			booking.ConsignorPK = consignor.PK;
			booking.ConsigneePK = consignee.PK;

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(booking.PK, bookingFactory);

			var refCon = bookingFactory.New<RefContainer>();
			refCon.RC_TareWeight = 2000;

			var container = (CommonContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "GLMR1231111";
			container.JC_RC = refCon.PK;
			container.JC_GrossWeight = 10000;
			container.JC_DepartureEstimatedPickup = ZDateTime.Today.AddDays(5);
			container.JC_EmptyRequired = ZDateTime.Today;

			var container2 = (CommonContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container2.JC_ContainerCount = 7;
			container2.JC_RC = refCon.PK;
			container2.JC_GrossWeight = 10000;
			container2.JC_DepartureEstimatedPickup = ZDateTime.Today.AddDays(5);
			container2.JC_EmptyRequired = ZDateTime.Today;
			bookingFactory.Save();

			var newFactory = new BusinessObjectFactory();
			var newConsol = (CommonConsol)newFactory.New<IForwardingConsol>();

			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, booking.PK);

			var loadedShipment = LoadShipmentInConsolsFactory(newConsol, booking.PK);

			Assert("Expecting JS_IsBooking to be true.", loadedShipment.JS_IsBooking);
			Assert("Expecting JS_IsForwardRegistered to be true.", loadedShipment.JS_IsForwardRegistered);

			AssertEquals("Expecting service level to be true.", "D12", newConsol.JK_AWBServiceLevel);
			AssertEquals("Expecting consol to be direct", Constants.AgentType.Direct, newConsol.JK_AgentType);
			AssertEquals("Expecting new consol to have one shipment.", 1, newConsol.Shipments.Count);
			AssertEquals("Expecting consol to have two containers.", 2, newConsol.Containers.Count);

			AssertEquals("Expecting consol to be sea", Constants.TransportModes.Sea, newConsol.JK_TransportMode);
			AssertEquals("Expecting consol to be fcl", Constants.ContainerModes.FCL, newConsol.JK_ConsolMode);
			AssertEquals("Expecting consol to have 1 transport", 1, newConsol.Transports.Count);
			AssertEquals("Expecting consol's sailing to be empty.", ZGuid.Empty, newConsol.Transports[0].JW_JX);
			AssertEquals("Expecting consols' carrier to be carrier.", carrier.PK, newConsol.ShippingLinePK);
			AssertEquals("Expecting consol's booking ref to be carrier ref.", "Carrier Ref.", newConsol.JK_BookingReference);

			AssertEquals("Expecting consol's receiving agent to be empty", ZGuid.Empty, newConsol.JK_OA_ReceivingForwarderAddress);
			AssertEquals("Expecting consol's sending agent to be empty", ZGuid.Empty, newConsol.JK_OA_SendingForwarderAddress);

			foreach (var commonContainer in newConsol.Containers)
			{
				AssertEquals("Expecting consol's container mode to be fcl", Constants.ContainerModes.FCL, ((CommonContainer)commonContainer).JC_ContainerMode);
			}
		}

		public void TestMakeConsolFromBooking_CopiesCarrier()
		{
			SailingsForTestClasses sailingsHelper = new SailingsForTestClasses(Factory);

			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_IsShippingProvider = true;
			shippingLine.OH_IsShippingLine = true;
			shippingLine.OH_FullName = "ShippingLine";
			shippingLine.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shippingLine.MainAddress.OA_Address1 = "Address 1";

			Factory.Save();

			JobSailing exportSailing = sailingsHelper.SydLaxFlightLeg;
			exportSailing.Voyage.JV_OH_Line = shippingLine.PK;

			JobMawb jobMawb = Factory.New<JobMawb>();
			jobMawb.JM_Airline3DigitPrefix = "176";
			jobMawb.JM_MAWB = "10000001";
			jobMawb.JM_ServiceLevel = "STD";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			jobMawb.JM_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();

			OrgHeader carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			carrier.MainAddress.OA_Address1 = "Address 1";

			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_IsShippingLine = true;
			consignee.OH_IsShippingProvider = true;
			consignee.OH_FullName = "Consignee";
			consignee.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consignee.MainAddress.OA_Address1 = "Address 1";

			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_IsShippingLine = true;
			consignor.OH_IsShippingProvider = true;
			consignor.OH_FullName = "Consignor";
			consignor.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consignor.MainAddress.OA_Address1 = "Address 1";

			CommonShipment shipment = CreateBooking();

			shipment.JS_JX = exportSailing.PK;

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			shipment.JS_RL_NKDestination = "MAAGA";
			shipment.JS_CFSReference = "Carrier Ref.";
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			shipment.JS_IsDirectBooking = false;
			shipment.JS_AWBServiceLevel = "STD";
			shipment.JS_IsNeutralMaster = true;

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonConsol newConsol = (CommonConsol)newFactory.New<IForwardingConsol>();

			BuildConsolHelper helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, shipment.PK);

			CommonShipment loadedShipment = LoadShipmentInConsolsFactory(newConsol, shipment.PK);

			Assert("Expecting JS_IsBooking to be true.", loadedShipment.JS_IsBooking);
			Assert("Expecting JS_IsForwardRegistered to be true.", loadedShipment.JS_IsForwardRegistered);

			AssertEquals("Expecting new consol to have one shipment.", 1, newConsol.Shipments.Count);
			AssertEquals("Expecting consol to be sea", Constants.TransportModes.Air, newConsol.JK_TransportMode);
			AssertEquals("Expecting consol to be fcl", Constants.ContainerModes.Loose, newConsol.JK_ConsolMode);
			AssertEquals("Expecting consols' carrier to be carrier.", shippingLine.PK, newConsol.ShippingLinePK);
			AssertEquals("Expecting consol's booking ref to be carrier ref.", "Carrier Ref.", newConsol.JK_BookingReference);
		}

		public void TestMakeConsolFromBooking_CopiesCreditor_Export()
		{
			SailingsForTestClasses sailingsHelper = new SailingsForTestClasses(Factory);
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			JobSailing exportSailing = sailingsHelper.SydLaxFlightLeg;
			exportSailing.Voyage.JV_OH_Line = shippingLine.PK;

			Factory.Save();

			OrgHeader creditor = Factory.New<OrgHeader>();
			creditor.OH_IsShippingLine = true;
			creditor.OH_IsShippingProvider = true;
			creditor.OH_FullName = "Creditor";
			creditor.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort; 
			creditor.MainAddress.OA_Address1 = "Address 1";

			CommonShipment shipment = CreateBooking();

			shipment.JS_JX = exportSailing.PK;
			shipment.JS_OH_Creditor = creditor.PK;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonConsol newConsol = (CommonConsol)newFactory.New<IForwardingConsol>();
			newConsol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			BuildConsolHelper helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, shipment.PK);

			AssertEquals("Expecting consols' carrier to be carrier.", creditor.PK, newConsol.Creditor.PK); 
			AssertEquals("Expecting consols' carrier to be carrier.", creditor.PK, newConsol.CarrierExportCreditor.PK);
		}

		public void TestMakeConsolFromBooking_CopiesCreditor_Import()
		{
			SailingsForTestClasses sailingsHelper = new SailingsForTestClasses(Factory);
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			JobSailing importSailing = sailingsHelper.LaxMelSailing;
			importSailing.Voyage.JV_OH_Line = shippingLine.PK;

			Factory.Save();

			OrgHeader creditor = Factory.New<OrgHeader>();
			creditor.OH_IsShippingLine = true;
			creditor.OH_IsShippingProvider = true;
			creditor.OH_FullName = "Creditor";
			creditor.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort; 
			creditor.MainAddress.OA_Address1 = "Address 1";

			CommonShipment shipment = CreateBooking();

			shipment.JS_JX = importSailing.PK;
			shipment.JS_OH_Creditor = creditor.PK;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonConsol newConsol = (CommonConsol)newFactory.New<IForwardingConsol>();
			newConsol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			BuildConsolHelper helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, shipment.PK);

			AssertEquals("Expecting consols' carrier to be carrier.", creditor.PK, newConsol.Creditor.PK); 
			AssertEquals("Expecting consols' carrier to be carrier.", creditor.PK, newConsol.CarrierImportCreditor.PK);
		}

		public void TestMakeConsolFromBooking_DefaultAviationSecurity()
		{
			ZString oldShipmentInspectionTypeDefault = FreightDataRegistry.Instance.ShipmentInspectionTypeDefault.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			FreightDataRegistry.Instance.ShipmentInspectionTypeDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, FreightDataRegistry.AviationSecurity_Unknown_Code);

			try
			{
				CommonShipment booking = CreateBooking();
				booking.JS_TransportMode = Constants.TransportModes.Air;
				booking.JS_JX = Sailing.PK;

				Factory.Save();

				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				CommonConsol newConsol = (CommonConsol)newFactory.New<IForwardingConsol>();

				BuildConsolHelper helper = new BuildConsolHelper();
				helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, booking.PK);

				CommonShipment loadedShipment = LoadShipmentInConsolsFactory(newConsol, booking.PK);
				AssertEquals("Expecting JS_InspectionTypeCode to be initialized by registry setting.", FreightDataRegistry.AviationSecurity_Unknown_Code, loadedShipment.JS_InspectionTypeCode);
			}
			finally
			{
				FreightDataRegistry.Instance.ShipmentInspectionTypeDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldShipmentInspectionTypeDefault);
			}
		}

		public void TestMakeConsolFromBooking_Fcl()
		{
			BusinessObjectFactory bookingFactory = new BusinessObjectFactory();
			OrgHeader shippingLine = bookingFactory.New<OrgHeader>();
			shippingLine.OH_IsShippingProvider = true;
			shippingLine.OH_IsShippingLine = true;
			shippingLine.OH_FullName = "ShippingLine";
			shippingLine.OH_RL_NKClosestPort = HomePort;
			shippingLine.MainAddress.OA_Address1 = "Address 1";

			CommonShipment shipment = bookingFactory.New<CommonShipment>();
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_JX = Sailing.PK;
			shipment.JS_OA_BookedShippingLineAddress = shippingLine.MainAddress.PK;

			IQuotedBooking quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(shipment.PK, bookingFactory);

			RefContainer refCon = bookingFactory.New<RefContainer>();
			refCon.RC_TareWeight = 2000;

			CommonContainer container = (CommonContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "GLMR1231111";
			container.JC_RC = refCon.PK;
			container.JC_GrossWeight = 10000;
			container.JC_DepartureEstimatedPickup = ZDateTime.Today.AddDays(5);
			container.JC_EmptyRequired = ZDateTime.Today;

			CommonContainer container2 = (CommonContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container2.JC_ContainerCount = 7;
			container2.JC_RC = refCon.PK;
			container2.JC_GrossWeight = 10000;
			container2.JC_DepartureEstimatedPickup = ZDateTime.Today.AddDays(5);
			container2.JC_EmptyRequired = ZDateTime.Today;
			Factory.Save();
			bookingFactory.Save();

			Voyage.JV_OH_Line = shippingLine.PK;

			CommonConsol newConsol = (CommonConsol)Factory.New<IForwardingConsol>();

			BuildConsolHelper helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, shipment.PK);
			CommonShipment loadedShipment = (CommonShipment)Factory.Load(newConsol.Shipments.TypeOfElements, shipment.PK);

			Assert("Expecting JS_IsBooking to be true.", loadedShipment.JS_IsBooking);
			Assert("Expecting JS_IsForwardRegistered to be true.", loadedShipment.JS_IsForwardRegistered);

			AssertEquals("Expecting new consol to have one shipment.", 1, newConsol.Shipments.Count);
			AssertEquals("Expecting consol to have two containers.", 2, newConsol.Containers.Count);

			AssertEquals("Expecting consol to be sea", Constants.TransportModes.Sea, newConsol.JK_TransportMode);
			AssertEquals("Expecting consol to be fcl", Constants.ContainerModes.FCL, newConsol.JK_ConsolMode);
			AssertEquals("Expecting consol to have 1 transport", 1, newConsol.Transports.Count);
			AssertEquals("Expecting consol's sailing to be sailing.", Sailing.PK, newConsol.Transports[0].JW_JX);
			AssertEquals("Expecting consol's carrier to be shipping line", shippingLine.PK, newConsol.ShippingLinePK);
		}

		public void TestMakeConsolFromBooking_LoadAndDischargePorts_NoSailing()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.New<CommonShipment>();
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "NZAKL";

			factory.Save();
			var newConsol = (CommonConsol)Factory.New<IForwardingConsol>();

			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, shipment.PK);

			AssertEquals("Load Port populated From booking", newConsol.JK_RL_NKLoadPort, shipment.JS_RL_NKLoadPort);
			AssertEquals("Discharge Port populated From booking", newConsol.JK_RL_NKDischargePort, shipment.JS_RL_NKDischargePort);
		}

		public void TestMakeConsolFromBooking_LoadAndDischargePorts_SailingLinked()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.New<CommonShipment>();
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "NZAKL";

			var sailing = factory.New<JobSailing>();
			var origin = factory.New<VoyageOrigin>();
			var destination = factory.New<VoyageDestination>();
			var voyage = factory.New<JobVoyage>();

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;

			origin.JA_RL_NKPortOfLoading = "USLAX";
			destination.JB_RL_NKPortOfDischarge = "SGSIN";

			shipment.JS_JX = sailing.PK;

			factory.Save();

			var newConsol = (CommonConsol)Factory.New<IForwardingConsol>();

			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, shipment.PK);

			AssertEquals("Load Port populated from booking", shipment.JS_RL_NKLoadPort, newConsol.JK_RL_NKLoadPort);
			AssertEquals("Discharge Port populated from booking", shipment.JS_RL_NKDischargePort, newConsol.JK_RL_NKDischargePort);
		}

		public void TestMakeConsolFromBooking_LoadAndDischargePorts_SailingLinked_PortsEmpty()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.New<CommonShipment>();
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKLoadPort = ZString.Empty;
			shipment.JS_RL_NKDischargePort = ZString.Empty;

			var sailing = factory.New<JobSailing>();
			var origin = factory.New<VoyageOrigin>();
			var destination = factory.New<VoyageDestination>();
			var voyage = factory.New<JobVoyage>();

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;

			origin.JA_RL_NKPortOfLoading = "USLAX";
			destination.JB_RL_NKPortOfDischarge = "SGSIN";

			shipment.JS_JX = sailing.PK;

			factory.Save();

			var newConsol = (CommonConsol)Factory.New<IForwardingConsol>();

			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, shipment.PK);

			AssertEquals("Load Port populated from voyage origin", origin.JA_RL_NKPortOfLoading, newConsol.JK_RL_NKLoadPort);
			AssertEquals("Discharge Port populated from voyage destination", destination.JB_RL_NKPortOfDischarge, newConsol.JK_RL_NKDischargePort);
		}

		public void TestMakeConsolFromBooking_LoadAndDischargePorts_NoSailing_PortsEmpty()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.New<CommonShipment>();
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKLoadPort = ZString.Empty;
			shipment.JS_RL_NKDischargePort = ZString.Empty;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			shipment.JS_JX = ZGuid.Empty;

			factory.Save();

			var newConsol = (CommonConsol)Factory.New<IForwardingConsol>();

			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, shipment.PK);

			AssertEquals("Load Port populated from shipment origin", shipment.JS_RL_NKOrigin, newConsol.JK_RL_NKLoadPort);
			AssertEquals("Discharge Port populated from shipment destination", shipment.JS_RL_NKDestination, newConsol.JK_RL_NKDischargePort);
		}

		public void TestMakeConsolFromBooking_LoadAndDischargePorts_TransportsMaintainFields()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.New<CommonShipment>();
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "NZAKL";

			var sailing = factory.New<JobSailing>();
			var origin = factory.New<VoyageOrigin>();
			var destination = factory.New<VoyageDestination>();
			var voyage = factory.New<JobVoyage>();

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;

			origin.JA_RL_NKPortOfLoading = "USLAX";
			destination.JB_RL_NKPortOfDischarge = "SGSIN";

			shipment.JS_JX = sailing.PK;

			factory.Save();

			var newConsol = (CommonConsol)Factory.New<IForwardingConsol>();

			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, shipment.PK);

			AssertEquals("Load Port populated from shipment Load Port", shipment.JS_RL_NKLoadPort, newConsol.JK_RL_NKLoadPort);
			AssertEquals("Discharge Port populated from shipment Discharge Port", shipment.JS_RL_NKDischargePort, newConsol.JK_RL_NKDischargePort);

			AssertEquals("Consol transport created", 1, newConsol.Transports.Count);

			var transport = newConsol.Transports.First() as Transport;
			AssertEquals(transport.JW_RL_NKLoadPort, "USLAX");
			AssertEquals(transport.JW_RL_NKDiscPort, "SGSIN");
		}

		public void TestMakeConsolFromBooking_LoadAndDischargePorts_TransportFieldsSyncWithConsolWhenNotLinked()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.New<CommonShipment>();
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "NZAKL";

			shipment.JS_RL_NKOrigin = "USJFK";
			shipment.JS_RL_NKDestination = "SGSIN";

			factory.Save();

			var newConsol = (CommonConsol)Factory.New<IForwardingConsol>();

			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, shipment.PK);

			AssertEquals("Load Port populated from shipment Load Port", shipment.JS_RL_NKLoadPort, newConsol.JK_RL_NKLoadPort);
			AssertEquals("Discharge Port populated from shipment Discharge Port", shipment.JS_RL_NKDischargePort, newConsol.JK_RL_NKDischargePort);

			AssertEquals("Consol transport created", 1, newConsol.Transports.Count);

			var transport = newConsol.Transports.First() as Transport;
			AssertEquals(transport.JW_RL_NKLoadPort, "AUSYD");
			AssertEquals(transport.JW_RL_NKDiscPort, "NZAKL");
		}

		public void TestMakeConsolFromStandAloneShipment()
		{
			var bookingFactory = new BusinessObjectFactory();
			var shippingLine = bookingFactory.New<OrgHeader>();
			shippingLine.OH_IsShippingProvider = true;
			shippingLine.OH_IsShippingLine = true;
			shippingLine.OH_FullName = "ShippingLine";
			shippingLine.OH_RL_NKClosestPort = HomePort;
			shippingLine.MainAddress.OA_Address1 = "Address 1";
			var carrierServiceLevel = shippingLine.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "CSL";
			carrierServiceLevel.PL_CarrierServiceLevelDescription = "DESC";

			var shipment = bookingFactory.New<CommonShipment>();
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = true;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_JX = Sailing.PK;
			shipment.JS_OA_BookedShippingLineAddress = shippingLine.MainAddress.PK;
			shipment.JS_PL_NKCarrierServiceLevel = "CSL";
			shipment.JS_CFSReference = "CFS Ref";

			var contractNumber = shipment.Numbers.AddNew();
			contractNumber.CE_RN_NKCountryCode = "AU";
			contractNumber.CE_EntryType = "CON";
			contractNumber.CE_EntryNum = "CN123456789";

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(shipment.PK, bookingFactory);

			var refCon = bookingFactory.New<RefContainer>();
			refCon.RC_TareWeight = 2000;

			var container = (CommonContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "GLMR1231111";
			container.JC_RC = refCon.PK;
			container.JC_GrossWeight = 10000;
			container.JC_DepartureEstimatedPickup = ZDateTime.Today.AddDays(5);
			container.JC_EmptyRequired = ZDateTime.Today;

			var container2 = (CommonContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container2.JC_ContainerCount = 7;
			container2.JC_RC = refCon.PK;
			container2.JC_GrossWeight = 10000;
			container2.JC_DepartureEstimatedPickup = ZDateTime.Today.AddDays(5);
			container2.JC_EmptyRequired = ZDateTime.Today;
			Factory.Save();
			bookingFactory.Save();

			Voyage.JV_OH_Line = shippingLine.PK;

			var newConsol = (CommonConsol)Factory.New<IForwardingConsol>();

			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, shipment.PK);
			var loadedShipment = (CommonShipment)Factory.Load(newConsol.Shipments.TypeOfElements, shipment.PK);

			Assert("Expecting JS_IsBooking to be true.", loadedShipment.JS_IsBooking);
			Assert("Expecting JS_IsForwardRegistered to be true.", loadedShipment.JS_IsForwardRegistered);

			AssertEquals("Expecting new consol to have one shipment.", 1, newConsol.Shipments.Count);
			AssertEquals("Expecting consol to have two containers.", 2, newConsol.Containers.Count);

			AssertEquals("Expecting consol to be sea", Constants.TransportModes.Sea, newConsol.JK_TransportMode);
			AssertEquals("Expecting consol to be fcl", Constants.ContainerModes.FCL, newConsol.JK_ConsolMode);
			AssertEquals("Expecting consol to have 1 transport", 1, newConsol.Transports.Count);
			AssertEquals("Expecting consol's sailing to be sailing.", Sailing.PK, newConsol.Transports[0].JW_JX);
			AssertEquals("Expecting carrier's service level to be copied to consol", "CSL", newConsol.Transports[0].JW_PL_NKCarrierServiceLevel);
			AssertEquals("Expecting consol's carrier to be shipping line", shippingLine.PK, newConsol.ShippingLinePK);
			AssertEquals("Expecting carrier's service level to be copied to JK_AWBServiceLevel", "CSL", newConsol.JK_AWBServiceLevel);
			AssertEquals("Expecting consol's JK_BookingReference", "CFS Ref", newConsol.JK_BookingReference);

			AssertEquals("LoadedShipment.JS_JX", ZGuid.Empty, loadedShipment.JS_JX);
			AssertEquals("LoadedShipment.JS_OA_BookedShippingLineAddress", shippingLine.MainAddress.PK, loadedShipment.JS_OA_BookedShippingLineAddress);

			CombineAssertions("Numbers Should be Copied", () =>
			{
				AssertEquals("CN123456789", newConsol.Numbers[0].CE_EntryNum);
				AssertEquals("CON", newConsol.Numbers[0].CE_EntryType);
				AssertEquals("AU", newConsol.Numbers[0].CE_RN_NKCountryCode);
			});

			AssertEquals(contractNumber.CE_EntryNum, newConsol.JK_CarrierContractNumber);
		}

		public void TestAddStandAloneShipmentToConsol()
		{
			var bookingFactory = new BusinessObjectFactory();
			var shippingLine = bookingFactory.New<OrgHeader>();
			shippingLine.OH_IsShippingProvider = true;
			shippingLine.OH_IsShippingLine = true;
			shippingLine.OH_FullName = "ShippingLine";
			shippingLine.OH_RL_NKClosestPort = HomePort;
			shippingLine.MainAddress.OA_Address1 = "Address 1";

			var shipment = bookingFactory.New<CommonShipment>();
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = true;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_JX = Sailing.PK;
			shipment.JS_OA_BookedShippingLineAddress = shippingLine.MainAddress.PK;
			shipment.JS_CFSReference = "CFS Ref";

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(shipment.PK, bookingFactory);

			var refCon = bookingFactory.New<RefContainer>();
			refCon.RC_TareWeight = 2000;

			var container = (CommonContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "GLMR1231111";
			container.JC_RC = refCon.PK;
			container.JC_GrossWeight = 10000;
			container.JC_DepartureEstimatedPickup = ZDateTime.Today.AddDays(5);
			container.JC_EmptyRequired = ZDateTime.Today;

			var container2 = (CommonContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container2.JC_ContainerCount = 7;
			container2.JC_RC = refCon.PK;
			container2.JC_GrossWeight = 10000;
			container2.JC_DepartureEstimatedPickup = ZDateTime.Today.AddDays(5);
			container2.JC_EmptyRequired = ZDateTime.Today;

			var newConsol = (CommonConsol)Factory.New<IForwardingConsol>();
			newConsol.JK_TransportMode = Constants.TransportModes.Sea;
			newConsol.JK_ConsolMode = Constants.ContainerModes.FCL;
			newConsol.JK_BookingReference = "Consol Ref";

			var consolVoyage = Factory.New<JobVoyage>();
			var consolVoyageOrigin = Factory.New<VoyageOrigin>();
			consolVoyageOrigin.JA_JV = consolVoyage.PK;
			consolVoyageOrigin.JA_RL_NKPortOfLoading = HomePort;

			var consolVoyageDestination = Factory.New<VoyageDestination>();
			consolVoyageDestination.JB_JV = consolVoyage.PK;
			consolVoyageDestination.JB_RL_NKPortOfDischarge = OverseasPort;

			var consolSailing = Factory.New<JobSailing>();
			consolSailing.JX_JA = consolVoyageOrigin.PK;
			consolSailing.JX_JB = consolVoyageDestination.PK;

			newConsol.Transports[0].JW_JX = consolSailing.PK;

			Factory.Save();
			bookingFactory.Save();

			Voyage.JV_OH_Line = shippingLine.PK;

			var helper = new BuildConsolHelper();
			helper.AddStandAloneShipmentToConsol(newConsol, shipment);
			var loadedShipment = (CommonShipment)Factory.Load(newConsol.Shipments.TypeOfElements, shipment.PK);

			Assert("Expecting JS_IsBooking to be true.", loadedShipment.JS_IsBooking);
			Assert("Expecting JS_IsForwardRegistered to be true.", loadedShipment.JS_IsForwardRegistered);

			AssertEquals("Expecting new consol to have one shipment.", 1, newConsol.Shipments.Count);
			AssertEquals("Expecting consol to have two containers.", 2, newConsol.Containers.Count);

			AssertEquals("Expecting consol to have 1 transport", 1, newConsol.Transports.Count);
			AssertEquals("Expecting consol's sailing to be unchanged.", consolSailing.PK, newConsol.Transports[0].JW_JX);
			AssertEquals("Expecting consol's JK_BookingReference to be unchanged", "Consol Ref", newConsol.JK_BookingReference);

			AssertEquals("LoadedShipment.JS_JX", ZGuid.Empty, loadedShipment.JS_JX);
			AssertEquals("LoadedShipment.JS_OA_BookedShippingLineAddress", shippingLine.MainAddress.PK, loadedShipment.JS_OA_BookedShippingLineAddress);
		}

		public void TestMakeConsolFromBooking_EnsureConsolShipmentsAreForwarding()
		{
			BusinessObjectFactory bookingFactory = new BusinessObjectFactory();

			CommonShipment shipment = bookingFactory.New<CommonShipment>();
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_JX = Sailing.PK;

			Factory.Save();
			bookingFactory.Save();

			CommonConsol newConsol = (CommonConsol)Factory.New<IForwardingConsol>();

			BuildConsolHelper helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, shipment.PK);

			AssertEquals("Expecting new consol to have one shipment.", 1, newConsol.Shipments.Count);
			AssertEquals("Expecting CommonShipment to be of type forwarding", ObjectFactory.GetType<IForwardingShipment>(), newConsol.Shipments[0].GetType());
		}

		public void TestMakeConsolFromBooking_PullsApprovedShipper()
		{
			var booking = CreateBooking();
			booking.JS_TransportMode = "AIR";
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_InspectionTypeCode = "PHS";
			Factory.Save();
			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK);
			var shipment = consol.Factory.Load<CommonShipment>(booking.PK);
			AssertEquals("PHS", shipment.JS_InspectionTypeCode);
		}

		public void TestAddNewConsolidation_WhenFromBookingConvertedToShipment_CopiesOnlyConAndNacNumbers()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = true;

			var contractNumber = shipment.Numbers.AddNew();
			contractNumber.CE_RN_NKCountryCode = "AU";
			contractNumber.CE_EntryType = "CON";
			contractNumber.CE_EntryNum = "CN123456789";

			contractNumber = shipment.Numbers.AddNew();
			contractNumber.CE_RN_NKCountryCode = "US";
			contractNumber.CE_EntryType = "NAC";
			contractNumber.CE_EntryNum = "NC123456789";

			contractNumber = shipment.Numbers.AddNew();
			contractNumber.CE_RN_NKCountryCode = "US";
			contractNumber.CE_EntryType = "AMS";
			contractNumber.CE_EntryNum = "AM123456789";

			Factory.Save();

			var newConsol = (CommonConsol)Factory.New<IForwardingConsol>();
			ConsolStandardAloneShipmentRelationshipHelper.MakeConsolFromStandaloneShipment(newConsol, shipment);

			CombineAssertions("Booking > Convert to Shipment > New Consol > Only CON and NAC numbers should be copied", () =>
			{
				AssertEquals(2, newConsol.Numbers.Count);

				AssertEquals("CN123456789", newConsol.Numbers[0].CE_EntryNum);
				AssertEquals("CON", newConsol.Numbers[0].CE_EntryType);
				AssertEquals("AU", newConsol.Numbers[0].CE_RN_NKCountryCode);

				AssertEquals("NC123456789", newConsol.Numbers[1].CE_EntryNum);
				AssertEquals("NAC", newConsol.Numbers[1].CE_EntryType);
				AssertEquals("US", newConsol.Numbers[1].CE_RN_NKCountryCode);
			});
		}

		public void TestConsolidation_WhenFromBooking_CopiesOnlyConAndNacNumbers()
		{
			var booking = CreateBooking();
			booking.JS_HouseBill = "BILL999";
			booking.JS_IsDirectBooking = ZBool.True;

			var contractNumber = booking.Numbers.AddNew();
			contractNumber.CE_RN_NKCountryCode = "AU";
			contractNumber.CE_EntryType = "CON";
			contractNumber.CE_EntryNum = "CN123456789";

			contractNumber = booking.Numbers.AddNew();
			contractNumber.CE_RN_NKCountryCode = "US";
			contractNumber.CE_EntryType = "NAC";
			contractNumber.CE_EntryNum = "NC123456789";

			contractNumber = booking.Numbers.AddNew();
			contractNumber.CE_RN_NKCountryCode = "US";
			contractNumber.CE_EntryType = "AMS";
			contractNumber.CE_EntryNum = "AM123456789";

			Factory.Save();

			var helper = new BuildConsolHelper();
			var newConsol = (CommonConsol)Factory.New<IForwardingConsol>();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, booking.PK);

			CombineAssertions("Booking > Consolidation > Only CON and NAC numbers should be copied", () =>
			{
				AssertEquals(2, newConsol.Numbers.Count);

				AssertEquals("CN123456789", newConsol.Numbers[0].CE_EntryNum);
				AssertEquals("CON", newConsol.Numbers[0].CE_EntryType);
				AssertEquals("AU", newConsol.Numbers[0].CE_RN_NKCountryCode);

				AssertEquals("NC123456789", newConsol.Numbers[1].CE_EntryNum);
				AssertEquals("NAC", newConsol.Numbers[1].CE_EntryType);
				AssertEquals("US", newConsol.Numbers[1].CE_RN_NKCountryCode);
			});
		}

		public void TestAddNewConsolidation_WhenFromForwardingShipment_CopiesNoReferenceNumbers()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_IsBooking = false;
			shipment.JS_IsForwardRegistered = true;

			var contractNumber = shipment.Numbers.AddNew();
			contractNumber.CE_RN_NKCountryCode = "AU";
			contractNumber.CE_EntryType = "CON";
			contractNumber.CE_EntryNum = "CN123456789";

			contractNumber = shipment.Numbers.AddNew();
			contractNumber.CE_RN_NKCountryCode = "US";
			contractNumber.CE_EntryType = "NAC";
			contractNumber.CE_EntryNum = "NC123456789";

			contractNumber = shipment.Numbers.AddNew();
			contractNumber.CE_RN_NKCountryCode = "US";
			contractNumber.CE_EntryType = "AMS";
			contractNumber.CE_EntryNum = "AM123456789";

			Factory.Save();

			var newConsol = (CommonConsol)Factory.New<IForwardingConsol>();
			ConsolStandardAloneShipmentRelationshipHelper.MakeConsolFromStandaloneShipment(newConsol, shipment);

			AssertEquals("Shipment(Forwarding) > New Consol > No reference numbers should be copied", 0, newConsol.Numbers.Count);
		}

		public void TestMakeConsolFromBooking_BillNumbers()
		{
			var consol = Factory.New<CommonConsol>();
			var booking = CreateBooking();
			booking.JS_HouseBill = "BILL456";
			booking.JS_IsDirectBooking = ZBool.False;

			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK);
			AssertEquals(ZString.Empty, consol.JK_MasterBillNum);
			AssertEquals("BILL456", booking.JS_HouseBill);

			booking = CreateBooking();
			booking.JS_HouseBill = "BILL999";
			booking.JS_IsDirectBooking = ZBool.True;

			helper.MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK);
			AssertEquals("BILL999", consol.JK_MasterBillNum);
			AssertEquals(ZString.Empty, booking.JS_HouseBill);
		}

		public void TestChangeBookingConsolidationParent()
		{
			var consol = Factory.New<CommonConsol>();
			var booking = CreateBooking();
			booking.JS_IsForwardRegistered = false;
			booking.JS_IsBooking = true;
			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(booking.PK, Factory);
			var bookingConsolidation = Factory.New<IDtbBookingConsolidation>();
			bookingConsolidation.KB_ParentID = quotedBooking.ViewPK;
			bookingConsolidation.KB_ParentTableCode = ((BusinessObject)quotedBooking).TablePrefix;

			Factory.Save();

			AssertEquals("Precondition", bookingConsolidation.KB_ParentID, quotedBooking.ViewPK);
			AssertEquals("Precondition", bookingConsolidation.KB_ParentTableCode, ((BusinessObject)quotedBooking).TablePrefix);

			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK);

			AssertEquals(bookingConsolidation.KB_ParentID, booking.PK);
			AssertEquals(bookingConsolidation.KB_ParentTableCode, booking.TablePrefix);
		}

		public void TestMakeConsolFromBooking_WithConsignments()
		{
			var consignmentConsolidation = Factory.New<IDtbConsignmentConsolidation>();
			var spotQuote = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.SpotQuote, Factory);
			AssertNull("Precondition - 'Quote' not 'Booking'", spotQuote.ForwardingShipment);
			var consol = Factory.New<CommonConsol>();
			Factory.Save();
			AssertEquals("Precondition: Consignment is standalone.", ZGuid.Empty, consignmentConsolidation.KB_ParentID);
			AssertEquals("Precondition: Consignment is standalone.", true, consignmentConsolidation.KB_ParentTableCode.IsEmpty);

			var helper = new BuildConsolHelper();
			var booking = Factory.New<CommonShipment>(); // Quoted booking form turns the quote into a QuotedBooking using another factory? Emulate this.
			booking.JS_IsBooking = true;
			AssertNoExceptionThrown(() => helper.MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK));
			AssertEquals("Should not have linked consignment.", ZGuid.Empty, consignmentConsolidation.KB_ParentID);
			AssertEquals("Should not have linked consignment.", true, consignmentConsolidation.KB_ParentTableCode.IsEmpty);
		}

		public void TestMakeConsolFromBooking_WithStandaloneTransportBookings()
		{
			var otherShipment = Factory.New<CommonShipment>();
			var standaloneBookingConsolidation = Factory.New<IDtbBookingConsolidation>();
			var linkedBookingConsolidation = Factory.New<IDtbBookingConsolidation>();
			linkedBookingConsolidation.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			linkedBookingConsolidation.KB_ParentID = otherShipment.PK;

			var spotQuote = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.SpotQuote, Factory);
			AssertNull("Precondition - 'Quote' not 'Booking'", spotQuote.ForwardingShipment);
			var consol = Factory.New<CommonConsol>();
			Factory.Save();

			var helper = new BuildConsolHelper();
			var booking = Factory.New<CommonShipment>(); // Quoted booking form turns the quote into a QuotedBooking using another factory? Emulate this.
			booking.JS_IsBooking = true;
			AssertNoExceptionThrown(() => helper.MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK));
			AssertEquals("Should not have linked standalone TB.", ZGuid.Empty, standaloneBookingConsolidation.KB_ParentID);
			AssertEquals("Should not have linked standalone TB.", true, standaloneBookingConsolidation.KB_ParentTableCode.IsEmpty);
			AssertEquals("Should not have relinked TB linked to another parent.", linkedBookingConsolidation.KB_ParentID, otherShipment.PK);
		}

		public void TestMakeConsolFromBooking_DisableMilestonesAndTriggersInBooking()
		{
			var consol = Factory.New<CommonConsol>();
			var booking = CreateBooking();
			booking.JS_IsBooking = true;
			booking.JS_IsDirectBooking = false;

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(booking.PK, Factory);
			var bookingWorkflowProvider = (IWorkflowProvider)quotedBooking;

			var bookingMilestone = bookingWorkflowProvider.WorkflowItems.Milestones.AddNew();
			var bookingTrigger = bookingWorkflowProvider.WorkflowItems.Triggers.AddNew();
			var bookingTask = bookingWorkflowProvider.WorkflowItems.Tasks.AddNew();
			var bookingException = bookingWorkflowProvider.WorkflowItems.Exceptions.AddNew();

			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK);

			AssertEquals(EventReferenceConditionList.Codes.ConditionWithMacros, bookingMilestone.TriggerConditions.TriggerCondition);
			AssertEquals("false", bookingMilestone.TriggerConditions.TriggerConditionValue);

			AssertEquals(EventReferenceConditionList.Codes.ConditionWithMacros, bookingTrigger.TriggerConditions.TriggerCondition);
			AssertEquals("false", bookingMilestone.TriggerConditions.TriggerConditionValue);

			AssertEquals(string.Empty, bookingTask.TriggerConditions.TriggerCondition);
			AssertEquals(string.Empty, bookingTask.TriggerConditions.TriggerConditionValue);

			AssertEquals(string.Empty, bookingException.TriggerConditions.TriggerCondition);
			AssertEquals(string.Empty, bookingException.TriggerConditions.TriggerConditionValue);
		}

		public void TestMakeConsolFromBooking_PopulateETDAndETA()
		{
			var etd = new ZDateTime(2019, 05, 01);
			var eta = new ZDateTime(2019, 05, 07);

			var consol = Factory.New<CommonConsol>();
			var booking = CreateBooking();
			booking.JS_E_DEP = etd;
			booking.JS_E_ARV = eta;

			Factory.Save();

			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK);

			AssertEquals("Shipment ETD should match consol's most interesting transport ETD", etd, consol.Transports.MostInterestingTransport.JW_ETD);
			AssertEquals("Shipment ETA should match consol's most interesting transport ETA", eta, consol.Transports.MostInterestingTransport.JW_ETA);
		}

		[ExpectNoExceptions]
		public void TestMakeConsolFromBooking_ShipmentCannotBeAttachedToConsol()
		{
			var booking = CreateBooking();
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			booking.JS_PackingMode = Constants.ContainerModes.FCL;
			booking.JS_RL_NKLoadPort = "AUSYD";
			booking.JS_RL_NKDischargePort = "CNSHA";
			booking.JS_UniqueConsignRef = "S00001";

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(booking.PK, Factory);
			var container = (CommonContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerCount = 1;
			container.JC_ContainerNum = "CT00001";

			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";

			var helper = new BuildConsolHelper();
			var isShipmentCannotBeAttachedToConsolCalled = false;
			helper.ShipmentCannotBeAttachedToConsol += (sender, args) => isShipmentCannotBeAttachedToConsolCalled = true;
			helper.MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK);
			Factory.Save();

			AssertEquals(1, consol.Shipments.Count);

			var newFactory = new BusinessObjectFactory();
			var anotherConsol = (CommonConsol)newFactory.New<IForwardingConsol>();
			helper.MakeConsolFromBookingOrStandaloneShipment(anotherConsol, booking.PK);
			newFactory.Save();

			AssertEquals(0, anotherConsol.Shipments.Count);
			AssertEquals(expected: true, isShipmentCannotBeAttachedToConsolCalled);
		}

		#endregion

		#region Defaulting Sending and Receiving Agents for Non-direct

		public void TestDefaultSendingForwarderAddress()
		{
			OrgAddress agent = GetForwarderAgentAddress(AgentStatusPublished, "AUSYD", AgentDirectionExport, Constants.TransportModes.Air);

			SailingsForTestClasses sailingsHelper = new SailingsForTestClasses(Factory);
			JobSailing exportSailing = sailingsHelper.SydLaxFlightLeg;

			Factory.Save();

			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_IsShippingLine = true;
			consignee.OH_IsShippingProvider = true;
			consignee.OH_FullName = "Consignee";
			consignee.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consignee.MainAddress.OA_Address1 = "Address 1";

			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_IsShippingLine = true;
			consignor.OH_IsShippingProvider = true;
			consignor.OH_FullName = "Consignor";
			consignor.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consignor.MainAddress.OA_Address1 = "Address 1";

			CommonShipment shipment = CreateBooking();

			shipment.JS_JX = exportSailing.PK;

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			shipment.JS_RL_NKDestination = "MAAGA";
			shipment.JS_CFSReference = "Carrier Ref.";
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonConsol newConsol = (CommonConsol)newFactory.New<IForwardingConsol>();

			BuildConsolHelper helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, shipment.PK);

			CommonShipment loadedShipment = LoadShipmentInConsolsFactory(newConsol, shipment.PK);

			Assert("Expecting JS_IsBooking to be true.", loadedShipment.JS_IsBooking);
			Assert("Expecting JS_IsForwardRegistered to be true.", loadedShipment.JS_IsForwardRegistered);

			AssertEquals("Expecting new consol to have one shipment.", 1, newConsol.Shipments.Count);
			AssertEquals("Expecting consol to be sea", Constants.TransportModes.Air, newConsol.JK_TransportMode);
			AssertEquals("Expecting consol to be fcl", Constants.ContainerModes.Loose, newConsol.JK_ConsolMode);

			AssertEquals("Expecting consol's SendingForwarder to be defaulted from the port AUSYD", agent.PK, newConsol.JK_OA_SendingForwarderAddress);
		}

		public void TestDefaultJK_OA_ReceivingForwarderAddress()
		{
			OrgAddress org = GetForwarderAgentAddress(AgentStatusPublished, "USLAX", AgentDirectionImport, Constants.TransportModes.Air);

			SailingsForTestClasses sailingsHelper = new SailingsForTestClasses(Factory);
			JobSailing exportSailing = sailingsHelper.SydLaxFlightLeg;

			Factory.Save();

			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_IsShippingLine = true;
			consignee.OH_IsShippingProvider = true;
			consignee.OH_FullName = "Consignee";
			consignee.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consignee.MainAddress.OA_Address1 = "Address 1";

			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_IsShippingLine = true;
			consignor.OH_IsShippingProvider = true;
			consignor.OH_FullName = "Consignor";
			consignor.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consignor.MainAddress.OA_Address1 = "Address 1";

			CommonShipment shipment = CreateBooking();

			shipment.JS_JX = exportSailing.PK;

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			shipment.JS_RL_NKDestination = "MAAGA";
			shipment.JS_CFSReference = "Carrier Ref.";
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonConsol newConsol = (CommonConsol)newFactory.New<IForwardingConsol>();

			BuildConsolHelper helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, shipment.PK);

			CommonShipment loadedShipment = LoadShipmentInConsolsFactory(newConsol, shipment.PK);

			Assert("Expecting JS_IsBooking to be true.", loadedShipment.JS_IsBooking);
			Assert("Expecting JS_IsForwardRegistered to be true.", loadedShipment.JS_IsForwardRegistered);

			AssertEquals("Expecting new consol to have one shipment.", 1, newConsol.Shipments.Count);
			AssertEquals("Expecting consol to be sea", Constants.TransportModes.Air, newConsol.JK_TransportMode);
			AssertEquals("Expecting consol to be fcl", Constants.ContainerModes.Loose, newConsol.JK_ConsolMode);

			AssertEquals("Expecting consol's ReceivingForwarder to be defaulted from the port USLAX", org.PK, newConsol.JK_OA_ReceivingForwarderAddress);
		}

		public void TestBookingRelatedPartiesDefaultToConsolReceivingAndSendingAgents()
		{
			var relatedParty1 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty2 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty3 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty4 = Factory.NewWithValidTestData<OrgHeader>();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "ABC Co.";
			consignee.SetRelatedParty(relatedParty1, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			consignee.SetRelatedParty(relatedParty2, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "SDF Co.";
			consignor.SetRelatedParty(relatedParty3, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			consignor.SetRelatedParty(relatedParty4, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newConsol = (CommonConsol)newFactory.New<IForwardingConsol>();

			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, shipment.PK);
			CommonShipment loadedShipment = LoadShipmentInConsolsFactory(newConsol, shipment.PK);

			AssertEquals("Consignee related party agent from booking should default to receiving agent on the consol", newConsol.JK_OA_ReceivingForwarderAddress, relatedParty1.MainAddress.PK);
			AssertEquals("Consignor related party agent from booking should default to sending agent on the consol", newConsol.JK_OA_SendingForwarderAddress, relatedParty4.MainAddress.PK);
		}

		#endregion

		#region TestBuildingConsolShouldNotOverwriteShipmentDates

		public void TestBuildingConsolShouldNotOverwriteShipmentDates()
		{
			ZDateTime today = ZDateTime.Today;

			JobVoyage voyage = Factory.New<JobVoyage>();

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = HomePort;
			origin.JA_E_DEP = today.AddDays(10);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = OverseasPort;
			destination.JB_E_ARV = today.AddDays(15);

			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];

			Factory.Save(); // Prevent db-only hits on dirty tables.

			CommonShipment booking = CreateBooking();
			booking.JS_RL_NKOrigin = AlternateHomePort;
			booking.JS_RL_NKDestination = OverseasPort2;
			booking.JS_JX = sailing.PK;
			booking.JS_E_DEP = today.AddDays(5);
			booking.JS_E_ARV = today.AddDays(20);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonConsol newConsol = (CommonConsol)newFactory.New<IForwardingConsol>();

			BuildConsolHelper helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, booking.PK);

			CommonShipment shipment = LoadShipmentInConsolsFactory(newConsol, booking.PK);

			AssertEquals("JS_E_DEP should not have changed", today.AddDays(5), shipment.JS_E_DEP);
			AssertEquals("JS_E_ARV should not have changed", today.AddDays(20), shipment.JS_E_ARV);
		}

		#endregion

		#region TestFinalDestETAIsNotCleared

		public void TestFinalDestETAIsNotCleared()
		{
			ZDateTime eTA = ZDateTime.Today;

			BusinessObjectFactory bookingFactory = new BusinessObjectFactory();

			CommonShipment booking = CreateBooking(bookingFactory);
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			booking.JS_PackingMode = Constants.ContainerModes.FCL;
			booking.JS_RL_NKOrigin = "AUBNE";
			booking.JS_RL_NKDestination = "SGSIN";
			booking.JS_E_ARV = eTA;
			bookingFactory.Save();

			CommonConsol newConsol = (CommonConsol)Factory.New<IForwardingConsol>();

			BuildConsolHelper helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, booking.PK);

			CommonShipment shipment = (CommonShipment)Factory.Load(newConsol.Shipments.TypeOfElements, booking.PK);

			AssertEquals(booking.PK, shipment.PK);
			AssertEquals("Final Dest. ETA", eTA, booking.JS_E_ARV);
			AssertEquals("CommonShipment ETA should be set.", eTA, shipment.JS_E_ARV);
		}

		#endregion

		#region TestSetLoadAndDischarge

		public void TestSetLoadAndDischargeWhenTransportModeIsAIR()
		{
			var bookingFactory = new BusinessObjectFactory();

			var booking1 = CreateBooking(bookingFactory);
			booking1.JS_TransportMode = Constants.TransportModes.Air;
			booking1.JS_PackingMode = Constants.ContainerModes.Loose;
			booking1.JS_RL_NKDestination = "CNSHA";
			booking1.JS_RL_NKOrigin = "AUSYD";

			bookingFactory.Save();

			var newFactory1 = new BusinessObjectFactory();
			var newConsol1 = (CommonConsol)newFactory1.New<IForwardingConsol>();

			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol1, booking1.PK);

			AssertEquals("Expecting consol's Load to be set", "AUSYD", newConsol1.JK_RL_NKLoadPort);
			AssertEquals("Expecting consol's Discharge to be set", "CNSHA", newConsol1.JK_RL_NKDischargePort);

			var sailingsHelper = new SailingsForTestClasses(bookingFactory);
			var exportSailing = sailingsHelper.SydLaxFlightLeg;
			exportSailing.Origin.JA_RL_NKPortOfLoading = "AUSYD";
			exportSailing.Destination.JB_RL_NKPortOfDischarge = "CNSHA";

			var booking2 = CreateBooking(bookingFactory);
			var cfs = bookingFactory.NewWithValidTestData<OrgAddress>();
			booking2.JS_TransportMode = Constants.TransportModes.Air;
			booking2.JS_PackingMode = Constants.ContainerModes.Loose;
			booking2.JS_OA_ExportReceivingDepot = cfs.PK;
			booking2.JS_JX = exportSailing.PK;
			booking2.JS_RL_NKDestination = "USLAX";
			booking2.JS_RL_NKOrigin = "SGSIN";

			bookingFactory.Save();

			var newFactory2 = new BusinessObjectFactory();
			var newConsol2 = (CommonConsol)newFactory2.New<IForwardingConsol>();

			helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol2, booking2.PK);

			var loadedShipment = LoadShipmentInConsolsFactory(newConsol2, booking2.PK);

			AssertEquals("Expecting consol's Load to be set", loadedShipment.JS_Calc_CurrentLoadPort, newConsol2.JK_RL_NKLoadPort);
			AssertEquals("Expecting consol's Discharge to be set", loadedShipment.JS_Calc_CurrentDischargePort, newConsol2.JK_RL_NKDischargePort);

			var originalHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			var originalExport = GlbDepartment.CurrentDepartment.GE_Export;
			var originalImport = GlbDepartment.CurrentDepartment.GE_Import;

			try
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
				GlbDepartment.CurrentDepartment.GE_Export = true;
				GlbDepartment.CurrentDepartment.GE_Import = false;

				var newConsol3 = (CommonConsol)newFactory1.New<IForwardingConsol>();
				helper.MakeConsolFromBookingOrStandaloneShipment(newConsol3, booking1.PK);

				AssertEquals("Expecting consol's Load to be set", "AUSYD", newConsol3.JK_RL_NKLoadPort);
				AssertEquals("Expecting consol's Discharge to be set", "CNSHA", newConsol3.JK_RL_NKDischargePort);

				var newConsol4 = (CommonConsol)newFactory1.New<IForwardingConsol>();
				helper.MakeConsolFromBookingOrStandaloneShipment(newConsol4, booking2.PK);

				loadedShipment = LoadShipmentInConsolsFactory(newConsol4, booking2.PK);

				AssertEquals("Expecting consol's Load to be set", loadedShipment.JS_Calc_CurrentLoadPort,
					newConsol4.JK_RL_NKLoadPort);
				AssertEquals("Expecting consol's Discharge to be set", loadedShipment.JS_Calc_CurrentDischargePort,
					newConsol4.JK_RL_NKDischargePort);

				GlbDepartment.CurrentDepartment.GE_Export = false;
				GlbDepartment.CurrentDepartment.GE_Import = true;

				var newConsol5 = (CommonConsol)newFactory1.New<IForwardingConsol>();
				helper.MakeConsolFromBookingOrStandaloneShipment(newConsol5, booking1.PK);

				AssertEquals("Expecting consol's Load to be set", "AUSYD", newConsol5.JK_RL_NKLoadPort);
				AssertEquals("Expecting consol's Discharge to be set", "CNSHA", newConsol5.JK_RL_NKDischargePort);

				var newConsol6 = (CommonConsol)newFactory1.New<IForwardingConsol>();
				helper.MakeConsolFromBookingOrStandaloneShipment(newConsol6, booking2.PK);

				loadedShipment = LoadShipmentInConsolsFactory(newConsol6, booking2.PK);

				AssertEquals("Expecting consol's Load to be set", loadedShipment.JS_Calc_CurrentLoadPort,
					newConsol6.JK_RL_NKLoadPort);
				AssertEquals("Expecting consol's Discharge to be set", loadedShipment.JS_Calc_CurrentDischargePort,
					newConsol6.JK_RL_NKDischargePort);
			}
			finally
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = originalHomePort;
				GlbDepartment.CurrentDepartment.GE_Export = originalExport;
				GlbDepartment.CurrentDepartment.GE_Import = originalImport;
			}
		}

		public void TestSetLoadAndDischargeWhenTransportModeIsNotAIR()
		{
			var bookingFactory = new BusinessObjectFactory();

			var booking1 = CreateBooking(bookingFactory);
			booking1.JS_TransportMode = Constants.TransportModes.Sea;
			booking1.JS_PackingMode = Constants.ContainerModes.FCL;
			booking1.JS_RL_NKDestination = "CNSHA";
			booking1.JS_RL_NKOrigin = "AUSYD";

			bookingFactory.Save();

			var newFactory1 = new BusinessObjectFactory();
			var newConsol1 = (CommonConsol)newFactory1.New<IForwardingConsol>();

			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol1, booking1.PK);

			var loadedShipment1 = LoadShipmentInConsolsFactory(newConsol1, booking1.PK);

			AssertEquals("Expecting consol's Load to be set", loadedShipment1.JS_RL_NKOrigin, newConsol1.JK_RL_NKLoadPort);
			AssertEquals("Expecting consol's Discharge to be set", loadedShipment1.JS_RL_NKDestination, newConsol1.JK_RL_NKDischargePort);

			var sailingsHelper = new SailingsForTestClasses(bookingFactory);
			var exportSailing = sailingsHelper.SydLaxFlightLeg;
			exportSailing.Origin.JA_RL_NKPortOfLoading = "AUSYD";
			exportSailing.Destination.JB_RL_NKPortOfDischarge = "CNSHA";

			var booking2 = CreateBooking(bookingFactory);
			var cfs = bookingFactory.NewWithValidTestData<OrgAddress>();
			booking2.JS_TransportMode = Constants.TransportModes.Sea;
			booking2.JS_PackingMode = Constants.ContainerModes.FCL;
			booking2.JS_OA_ExportReceivingDepot = cfs.PK;
			booking2.JS_JX = exportSailing.PK;
			booking2.JS_RL_NKDestination = "USLAX";
			booking2.JS_RL_NKOrigin = "SGSIN";

			bookingFactory.Save();

			var newFactory2 = new BusinessObjectFactory();
			var newConsol2 = (CommonConsol)newFactory2.New<IForwardingConsol>();

			helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol2, booking2.PK);

			var loadedShipment = LoadShipmentInConsolsFactory(newConsol2, booking2.PK);

			AssertEquals("Expecting consol's Load to be set", loadedShipment.JS_Calc_CurrentLoadPort, newConsol2.JK_RL_NKLoadPort);
			AssertEquals("Expecting consol's Discharge to be set", loadedShipment.JS_Calc_CurrentDischargePort, newConsol2.JK_RL_NKDischargePort);

			var originalHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			var originalExport = GlbDepartment.CurrentDepartment.GE_Export;
			var originalImport = GlbDepartment.CurrentDepartment.GE_Import;

			try
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
				GlbDepartment.CurrentDepartment.GE_Export = true;
				GlbDepartment.CurrentDepartment.GE_Import = false;

				var newConsol3 = (CommonConsol)newFactory1.New<IForwardingConsol>();
				helper.MakeConsolFromBookingOrStandaloneShipment(newConsol3, booking1.PK);

				loadedShipment1 = LoadShipmentInConsolsFactory(newConsol3, booking1.PK);

				AssertEquals("Expecting consol's Load to be set", loadedShipment1.JS_RL_NKOrigin, newConsol3.JK_RL_NKLoadPort);
				AssertEquals("Expecting consol's Discharge to be set", loadedShipment1.JS_RL_NKDestination, newConsol3.JK_RL_NKDischargePort);

				var newConsol4 = (CommonConsol)newFactory1.New<IForwardingConsol>();
				helper.MakeConsolFromBookingOrStandaloneShipment(newConsol4, booking2.PK);

				loadedShipment = LoadShipmentInConsolsFactory(newConsol4, booking2.PK);

				AssertEquals("Expecting consol's Load to be set", loadedShipment.JS_Calc_CurrentLoadPort, newConsol4.JK_RL_NKLoadPort);
				AssertEquals("Expecting consol's Discharge to be set", loadedShipment.JS_Calc_CurrentDischargePort, newConsol4.JK_RL_NKDischargePort);

				GlbDepartment.CurrentDepartment.GE_Export = false;
				GlbDepartment.CurrentDepartment.GE_Import = true;

				var newConsol5 = (CommonConsol)newFactory1.New<IForwardingConsol>();
				helper.MakeConsolFromBookingOrStandaloneShipment(newConsol5, booking1.PK);

				loadedShipment = LoadShipmentInConsolsFactory(newConsol5, booking1.PK);

				AssertEquals("Expecting consol's Load to be set", loadedShipment.JS_Calc_CurrentLoadPort, newConsol5.JK_RL_NKLoadPort);
				AssertEquals("Expecting consol's Discharge to be set", loadedShipment.JS_Calc_CurrentDischargePort, newConsol5.JK_RL_NKDischargePort);

				var newConsol6 = (CommonConsol)newFactory1.New<IForwardingConsol>();
				helper.MakeConsolFromBookingOrStandaloneShipment(newConsol6, booking2.PK);

				loadedShipment = LoadShipmentInConsolsFactory(newConsol6, booking2.PK);

				AssertEquals("Expecting consol's Load to be set", loadedShipment.JS_Calc_CurrentLoadPort, newConsol6.JK_RL_NKLoadPort);
				AssertEquals("Expecting consol's Discharge to be set", loadedShipment.JS_Calc_CurrentDischargePort, newConsol6.JK_RL_NKDischargePort);
			}
			finally
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = originalHomePort;
				GlbDepartment.CurrentDepartment.GE_Export = originalExport;
				GlbDepartment.CurrentDepartment.GE_Import = originalImport;
			}
		}

		public void TestDisableProcessTasks()
		{
			var booking = CreateBooking();
			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(booking.PK, Factory);

			var bookingWorkflowProvider = (IWorkflowProvider)quotedBooking;

			var bookingTask1 = bookingWorkflowProvider.WorkflowItems.Tasks.AddNew();
			bookingTask1.P9_Description = "task 1";
			bookingTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			bookingTask1.P9_RespondToCascadedEvents = false;

			var bookingTask2 = bookingWorkflowProvider.WorkflowItems.Tasks.AddNew();
			bookingTask2.P9_Description = "task 2";
			bookingTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			bookingTask2.P9_RespondToCascadedEvents = true;
			bookingTask2.P9_SE_NKTaskCompletionEvent = Events.CustomisableEvent99.Code;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedBooking = newFactory.Load<CommonShipment>(booking.PK);
			Assert("Booking is a ForwardingShipment", reloadedBooking is IForwardingShipment);

			var helper = new BuildConsolHelper();
			helper.TurnBookingIntoShipment(reloadedBooking, null);
			newFactory.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, bookingTask1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, bookingTask2.P9_Status);
		}

		public void TestDisableProcessTasks_ConsolidateDifferentCountry()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var booking = CreateBooking();
			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(booking.PK, Factory);

			var bookingWorkflowProvider = (IWorkflowProvider)quotedBooking;

			var bookingTask1 = bookingWorkflowProvider.WorkflowItems.Tasks.AddNew();
			bookingTask1.P9_Description = "test1";
			bookingTask1.P9_GC = company.PK;
			bookingTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			bookingTask1.P9_RespondToCascadedEvents = false;

			var bookingTask2 = bookingWorkflowProvider.WorkflowItems.Tasks.AddNew();
			bookingTask2.P9_Description = "test2";
			bookingTask2.P9_GC = company.PK;
			bookingTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			bookingTask2.P9_RespondToCascadedEvents = true;

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedBooking = newFactory.Load<CommonShipment>(booking.PK);
				Assert("Booking is a ForwardingShipment", reloadedBooking is IForwardingShipment);
				var helper = new BuildConsolHelper();
				helper.TurnBookingIntoShipment(reloadedBooking, null);
				newFactory.Save();
			}

			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, bookingTask1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, bookingTask2.P9_Status);
		}

		#endregion

		#region TestUpdateCFSDepot

		public void TestUpdateCFSDepot_ExportReceiveDepot_MakeConsolFromBookingOrStandaloneShipment()
		{
			var booking = CreateBooking();
			booking.JS_IsCFSRegistered = false;
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			booking.JS_RL_NKOrigin = ZString.Empty;

			var orgAddress = CreateAddressWithProxyOrg();
			booking.JS_OA_ExportReceivingDepot = orgAddress.PK;

			var consol = Factory.New<CommonConsol>();
			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK);

			Assert(booking.JS_IsCFSRegistered);
			AssertEquals(orgAddress.PK, consol.JK_OA_DepartureCTOAddress);
		}

		public void TestUpdateCFSDepot_ImportReleaseDepot_MakeConsolFromBookingOrStandaloneShipment()
		{
			var booking = CreateBooking();
			booking.JS_IsCFSRegistered = false;
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			booking.JS_RL_NKOrigin = ZString.Empty;

			var orgAddress = CreateAddressWithProxyOrg();
			booking.JS_OA_ImportReleaseDepot = orgAddress.PK;

			var consol = Factory.New<CommonConsol>();
			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK);

			Assert(booking.JS_IsCFSRegistered);
		}

		public void TestUpdateCFSDepot_ExportReceiveDepot_TureBookingIntoShipment()
		{
			var booking = CreateBooking();
			booking.JS_IsCFSRegistered = false;
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			booking.JS_RL_NKOrigin = ZString.Empty;

			var orgAddress = CreateAddressWithProxyOrg();
			booking.JS_OA_ExportReceivingDepot = orgAddress.PK;

			var consol = Factory.New<CommonConsol>();
			var helper = new BuildConsolHelper();
			helper.TurnBookingIntoShipment(booking, null);

			Assert(booking.JS_IsCFSRegistered);
		}

		public void TestUpdateCFSDepot_ImportReleaseDepot_TureBookingIntoShipment()
		{
			var booking = CreateBooking();
			booking.JS_IsCFSRegistered = false;
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			booking.JS_RL_NKOrigin = ZString.Empty;

			var orgAddress = CreateAddressWithProxyOrg();
			booking.JS_OA_ImportReleaseDepot = orgAddress.PK;

			var consol = Factory.New<CommonConsol>();
			var helper = new BuildConsolHelper();
			helper.TurnBookingIntoShipment(booking, null);

			Assert(booking.JS_IsCFSRegistered);
		}

		OrgAddress CreateAddressWithProxyOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = org.PK;
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = org.PK;
			return orgAddress;
		}

		#endregion

		#region Custom Fields

		public void TestCustomFieldsAreCopiedByConsolidating()
		{
			var booking = CreateBooking();

			var customValue1 = AddBookingCustomValue(booking, "custom1", "AAA");
			var customValue2 = AddBookingCustomValue(booking, "custom2", "111");

			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			new BuildConsolHelper().MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK);

			var shipment = consol.Shipments[0];
			var query = new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, shipment.PK);
			query.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, shipment.TablePrefix);
			var result = Factory.Load<GenCustomAddOnValue>(query);

			AssertEquals(2, result.Length);
			CustomFieldTestHelper.AssertDifferentCustomFieldsButSameValues(result.First(x => x.XV_Name == "custom1"), customValue1);
			CustomFieldTestHelper.AssertDifferentCustomFieldsButSameValues(result.First(x => x.XV_Name == "custom2"), customValue2);
		}

		public void TestCustomFieldsAreCopiedFromQuotedBookingToShipment() =>
			CustomFieldsAreCopiedFromBookingToShipment(QuoteBookingType.BookingWithQuote);

		public void TestCustomFieldsAreCopiedFromSpotBookingToShipment() =>
			CustomFieldsAreCopiedFromBookingToShipment(QuoteBookingType.SpotQuote);

		public void TestCustomFieldsAreCopiedFromQuickBookingToShipment() =>
			CustomFieldsAreCopiedFromBookingToShipment(QuoteBookingType.QuickBooking);

		void CustomFieldsAreCopiedFromBookingToShipment(QuoteBookingType bookingType)
		{
			var booking = CreateBooking();
			var customBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(bookingType, Factory);
			var customBookingPK = (customBooking as BusinessObject).PK;

			var customValue1 = AddBookingCustomValue(booking, "custom1", "AAA");
			var customValue2 = AddBookingCustomValue(booking, "custom2", "111");

			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			new BuildConsolHelper().MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK, customBookingPK);

			AssertEquals(1, consol.Shipments.Count);

			var shipment = consol.Shipments[0];
			var query = new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, shipment.PK);
			query.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, shipment.TablePrefix);
			var result = Factory.Load<GenCustomAddOnValue>(query);

			AssertEquals(2, result.Length);
			CustomFieldTestHelper.AssertDifferentCustomFieldsButSameValues(result.First(x => x.XV_Name == "custom1"), customValue1);
			CustomFieldTestHelper.AssertDifferentCustomFieldsButSameValues(result.First(x => x.XV_Name == "custom2"), customValue2);
		}

		public void TestCustomFieldsAreCopiedByAddToExistingConsol()
		{
			var booking = CreateBooking();
			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.BookingWithQuote, Factory);
			var quotedBookingPK = (quotedBooking as BusinessObject).PK;

			var customValue1 = AddBookingCustomValue(booking, "custom1", "AAA");
			var customValue2 = AddBookingCustomValue(booking, "custom2", "111");

			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			new BuildConsolHelper().AddBookingsToConsol(consol, new[] { booking.PK }, quotedBookingPK: quotedBookingPK);

			AssertEquals(1, consol.Shipments.Count);

			var shipment = consol.Shipments[0];
			var query = new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, shipment.PK);
			query.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, shipment.TablePrefix);
			var result = Factory.Load<GenCustomAddOnValue>(query);

			AssertEquals(2, result.Length);
			CustomFieldTestHelper.AssertDifferentCustomFieldsButSameValues(result.First(x => x.XV_Name == "custom1"), customValue1);
			CustomFieldTestHelper.AssertDifferentCustomFieldsButSameValues(result.First(x => x.XV_Name == "custom2"), customValue2);
		}

		public void TestCustomFieldsAreNotDuplicatedWhenConvertingStandaloneShipment()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = true;

			var customValue1 = AddBookingCustomValue(shipment, "custom", "AAA");
			var customValue2 = AddBookingCustomValue(shipment, "custom", "AAA");
			customValue2.XV_ParentTableCode = "JS";

			AssertEquals("Pre: Standalone Shipment", true, shipment.IsStandAloneShipmentFromBooking);

			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			new BuildConsolHelper().MakeConsolFromBookingOrStandaloneShipment(consol, shipment.PK);

			AssertEquals(1, consol.Shipments.Count);

			var query = new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, shipment.PK);
			query.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, shipment.TablePrefix);
			var result = Factory.Load<GenCustomAddOnValue>(query);

			AssertEquals("Custom fields should not be duplicated when converting standalone shipments.", 1, result.Length);
		}

		public void TestBookingCustomFieldsAreFrozenAfterConsolidation()
		{
			var booking = CreateBooking();
			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.BookingWithQuote, Factory);
			var quotedBookingPK = (quotedBooking as BusinessObject).PK;

			var bookingCustomField = AddBookingCustomValue(booking, "custom1", "AAA");

			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			new BuildConsolHelper().MakeConsolFromBookingOrStandaloneShipment(consol, booking.PK, quotedBookingPK);

			AssertEquals(1, consol.Shipments.Count);

			var shipment = consol.Shipments[0];
			var query = new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, shipment.PK);
			query.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, shipment.TablePrefix);
			var result = Factory.Load<GenCustomAddOnValue>(query);

			AssertEquals(1, result.Length);

			var shipmentCustomField = result[0];

			AssertNotEquals(shipmentCustomField, bookingCustomField);
			AssertEquals(shipmentCustomField.XV_ParentID, shipment.PK);
			AssertEquals(bookingCustomField.XV_ParentID, booking.PK);

			shipmentCustomField.XV_Data = "BBB";

			AssertNotEquals("BBB", bookingCustomField.XV_Data);
		}

		GenCustomAddOnValue AddBookingCustomValue(BusinessObject booking, ZString customValueName, ZString value)
		{
			GenCustomAddOnValue customAddOnValue = Factory.New<GenCustomAddOnValue>();
			customAddOnValue.XV_ParentID = booking.PK;
			customAddOnValue.XV_ParentTableCode = ViewQuotedBookingSchema.Constants.Prefix;
			customAddOnValue.XV_Name = customValueName;
			customAddOnValue.XV_Type = AddOnColumnDataType.Codes.String;
			customAddOnValue.XV_Data = value;

			return customAddOnValue;
		}

		#endregion

		#region DocumentaryOverrides

		public void TestMoveDocumentaryOverrides_Bookings()
		{
			CommonShipment booking1 = CreateBooking();
			CommonShipment booking2 = CreateBooking();

			var docOverride1 = CreateDocumentaryOverride(booking1.PK, "aaa");
			var docOverride2 = CreateDocumentaryOverride(booking1.PK, "bbb");
			var docOverride3 = CreateDocumentaryOverride(booking2.PK, "ccc");

			Factory.Save();

			var helper = new BuildConsolHelper();
			helper.TurnBookingIntoShipment(booking1, null);

			AssertEquals("doc override 1 for booking has been updated",
				JobShipmentSchema.Constants.Prefix, docOverride1.JDD_ParentTableCode);

			AssertEquals("doc override 2 for booking has been updated",
				JobShipmentSchema.Constants.Prefix, docOverride2.JDD_ParentTableCode);

			AssertEquals("doc override for another booking has not been updated",
				ViewQuotedBookingSchema.Constants.Prefix, docOverride3.JDD_ParentTableCode);
		}

		public void TestMoveDocumentaryOverrides_QuotedBookings()
		{
			IQuotedBooking quotedBooking1 = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.BookingWithQuote, Factory);
			IQuotedBooking quotedBooking2 = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.BookingWithQuote, Factory);

			var quotedBooking1PK = (quotedBooking1 as BusinessObject).PK;
			var quotedBooking2PK = (quotedBooking2 as BusinessObject).PK;

			var docOverride1 = CreateDocumentaryOverride(quotedBooking1PK, "aaa");
			var docOverride2 = CreateDocumentaryOverride(quotedBooking1PK, "bbb");
			var docOverride3 = CreateDocumentaryOverride(quotedBooking2PK, "ccc");

			Factory.Save();

			var helper = new BuildConsolHelper();
			helper.TurnBookingIntoShipment(quotedBooking1.ForwardingShipment as CommonShipment, null, quotedBooking1PK);

			AssertEquals("doc override 1 for booking has been updated",
				JobShipmentSchema.Constants.Prefix, docOverride1.JDD_ParentTableCode);

			AssertEquals("doc override 2 for booking has been updated",
				JobShipmentSchema.Constants.Prefix, docOverride2.JDD_ParentTableCode);

			AssertEquals("doc override for another booking has not been updated",
				ViewQuotedBookingSchema.Constants.Prefix, docOverride3.JDD_ParentTableCode);
		}

		public void TestMoveDocAddresses_QuotedBookings()
		{
			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.BookingWithQuote, Factory);

			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			var notifyPartyAddress = ((IDocAddresses)quotedBooking).DocAddresses.AddNew();
			notifyPartyAddress.DocAddressType = DocAddressType.NotifyParty;
			notifyPartyAddress.E2_OA_Address = orgAddress1.PK;

			var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			var notifyPartyAddress2 = ((IDocAddresses)quotedBooking).DocAddresses.AddNew();
			notifyPartyAddress2.DocAddressType = DocAddressType.NotifyParty2;
			notifyPartyAddress2.OrganisationPK = orgAddress2.PK;

			var orgAddress3 = Factory.NewWithValidTestData<OrgAddress>();
			var notifyPartyAddress3 = ((IDocAddresses)quotedBooking).DocAddresses.AddNew();
			notifyPartyAddress3.DocAddressType = DocAddressType.NotifyParty3;
			notifyPartyAddress3.OrganisationPK = orgAddress3.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var shipment = newFactory.Load<CommonShipment>(quotedBooking.ForwardingShipment.PK);
			var quotedBookingPK = (quotedBooking as BusinessObject).PK;

			new BuildConsolHelper().TurnBookingIntoShipment(shipment, null, quotedBookingPK);

			AssertNotNull("Expected shipment to have set notify party", shipment.DocAddresses.Cast<JobDocAddress>().Where(jd => jd.E2_OA_Address == orgAddress1.PK));
			AssertNotNull("Expected shipment to have set notify party 2", shipment.DocAddresses.Cast<JobDocAddress>().Where(jd => jd.E2_OA_Address == orgAddress2.PK));
			AssertNotNull("Expected shipment to have set notify party 3", shipment.DocAddresses.Cast<JobDocAddress>().Where(jd => jd.E2_OA_Address == orgAddress3.PK));
		}

		public void TestMoveDocAddresses_QuotedBookingsOverrides()
		{
			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.BookingWithQuote, Factory);

			var notifyParty1Address = "QWER";
			var notifyParty2Address = "ASDF";
			var notifyParty3Address = "ZXCV";

			var notifyPartyAddress = ((IDocAddresses)quotedBooking).DocAddresses.AddNew();
			notifyPartyAddress.DocAddressType = DocAddressType.NotifyParty;
			notifyPartyAddress.E2_AddressOverride = true;
			notifyPartyAddress.E2_Address1 = notifyParty1Address;

			var notifyPartyAddress2 = ((IDocAddresses)quotedBooking).DocAddresses.AddNew();
			notifyPartyAddress2.DocAddressType = DocAddressType.NotifyParty2;
			notifyPartyAddress2.E2_AddressOverride = true;
			notifyPartyAddress2.E2_Address1 = notifyParty2Address;

			var notifyPartyAddress3 = ((IDocAddresses)quotedBooking).DocAddresses.AddNew();
			notifyPartyAddress3.DocAddressType = DocAddressType.NotifyParty3;
			notifyPartyAddress3.E2_AddressOverride = true;
			notifyPartyAddress3.E2_Address1 = notifyParty3Address;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var shipment = newFactory.Load<CommonShipment>(quotedBooking.ForwardingShipment.PK);
			var quotedBookingPK = (quotedBooking as BusinessObject).PK;

			new BuildConsolHelper().TurnBookingIntoShipment(shipment, null, quotedBookingPK);

			var notifyPartyJobDocAddress = shipment.DocAddresses.Cast<JobDocAddress>().FirstOrDefault(x => x.DocAddressType == DocAddressType.NotifyParty);
			AssertNotNull(notifyPartyJobDocAddress);
			AssertEquals("Expected override", true, notifyPartyJobDocAddress.E2_AddressOverride);
			AssertEquals("Expected the correct address override", notifyPartyJobDocAddress.E2_Address1, notifyParty1Address);

			var notifyParty2JobDocAddress = shipment.DocAddresses.Cast<JobDocAddress>().FirstOrDefault(x => x.DocAddressType == DocAddressType.NotifyParty2);
			AssertNotNull(notifyParty2JobDocAddress);
			AssertEquals("Expected override", true, notifyParty2JobDocAddress.E2_AddressOverride);
			AssertEquals("Expected the correct address override", notifyParty2JobDocAddress.E2_Address1, notifyParty2Address);

			var notifyParty3JobDocAddress = shipment.DocAddresses.Cast<JobDocAddress>().FirstOrDefault(x => x.DocAddressType == DocAddressType.NotifyParty3);
			AssertNotNull(notifyParty3JobDocAddress);
			AssertEquals("Expected override", true, notifyParty3JobDocAddress.E2_AddressOverride);
			AssertEquals("Expected the correct address override", notifyParty3JobDocAddress.E2_Address1, notifyParty3Address);
		}

		VisualizerDocumentData CreateDocumentaryOverride(ZGuid parentPK, string name)
		{
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentID = parentPK;
			documentData.JDD_ParentTableCode = ViewQuotedBookingSchema.Constants.Prefix;
			documentData.JDD_Name = name;
			return documentData;
		}

		#endregion

		#region TestReportCannotAddToCollectionException

		public void TestReportCannotAddToCollectionException()
		{
			var booking = CreateBooking();
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			booking.JS_PackingMode = Constants.ContainerModes.FCL;

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(booking.PK, Factory);
			var container = (CommonContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerCount = 1;

			var newConsol = (CommonConsol)Factory.New<IForwardingConsol>();
			var otherConsol = (CommonConsol)Factory.New<IForwardingConsol>();
			otherConsol.Containers.Add(container);
			Factory.Save();

			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, booking.PK);

			Assert(ErrorReporter.HasBeenReported("Could not add container to Consol"));
			ErrorReporter.Clear();
		}

		#endregion

		public void TestGlobalCommercialInvoiceTurnBookingIntoShipment()
		{
			var ratingHeader = Factory.New(ObjectFactory.Get<IRating>().QuoteType);
			ratingHeader[RatingHeaderSchema.TH_RateType] = "QTE";
			ratingHeader[RatingHeaderSchema.TH_QuoteDate] = ZDateTime.UtcNow.Date;

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.BookingWithQuote, Factory);
			var shipmentBooking = (CommonShipment)quotedBooking.ForwardingShipment;
			shipmentBooking.JS_IsBooking = true;
			shipmentBooking.JS_IsForwardRegistered = false;
			shipmentBooking.JS_TH_OneTimeQuote = ratingHeader.PK;

			var bookingInvoiceHeader = GlobalCommercialInvoiceHelperTest.CreateInvoiceHeader(Factory, (quotedBooking as BusinessObject));
			bookingInvoiceHeader.GIH_Description = "BOOKING INV.HEADER";
			bookingInvoiceHeader.GIH_ParentTableCode =  ratingHeader.TablePrefix;
			var bookingInvoiceLine = GlobalCommercialInvoiceHelperTest.CreateInvoiceLine(Factory, bookingInvoiceHeader);
			bookingInvoiceLine.Headers = new GlobalCommercialInvoiceHeaderCollection(Factory, shipmentBooking.PK, shipmentBooking.TablePrefix);
			bookingInvoiceLine.GIL_Description = "BOOKING INV.LINE";

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var shipment = newFactory.Load<CommonShipment>(quotedBooking.ForwardingShipment.PK);

			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(true)))
			{
				new BuildConsolHelper().TurnBookingIntoShipment(shipment, null, (quotedBooking as BusinessObject).PK);

				var shipmentInvoiceHeader = newFactory.Load<GlobalCommercialInvoiceHeader>(new ZQuery(GlobalCommercialInvoiceHeaderSchema.GIH_ParentID, shipment.PK)).FirstOrDefault();
				CombineAssertions("Shipment invoice header should be created.", () =>
				{
					AssertNotNull(shipmentInvoiceHeader);
					AssertEquals(bookingInvoiceHeader.GIH_Description, shipmentInvoiceHeader.GIH_Description);
				});

				var shipmentInvoiceLine = newFactory.Load<GlobalCommercialInvoiceLine>(new ZQuery(GlobalCommercialInvoiceLineSchema.GIL_GIH_Header, shipmentInvoiceHeader.PK)).FirstOrDefault();
				CombineAssertions("Shipment invoice line should be created.", () =>
				{
					AssertNotNull(shipmentInvoiceLine);
					AssertEquals(bookingInvoiceLine.GIL_Description, shipmentInvoiceLine.GIL_Description);
				});
			}
		}

		#region Implementation

		CommonShipment CreateBooking()
		{
			return CreateBooking(Factory);
		}

		CommonShipment CreateBooking(BusinessObjectFactory factory)
		{
			CommonShipment booking = factory.New<CommonShipment>();
			booking.JS_IsBooking = true;
			booking.JS_IsForwardRegistered = false;
			return booking;
		}

		protected CommonShipment LoadShipmentInConsolsFactory(CommonConsol consol, ZGuid shipmentPK)
		{
			return (CommonShipment)consol.Factory.Load(consol.Shipments.TypeOfElements, shipmentPK);
		}

		protected override void SetUp()
		{
			base.SetUp();

			Voyage = Factory.New<JobVoyage>();
			Origin = Factory.New<VoyageOrigin>();
			Origin.JA_JV = Voyage.PK;
			Origin.JA_RL_NKPortOfLoading = HomePort;

			Destination = Factory.New<VoyageDestination>();
			Destination.JB_JV = Voyage.PK;
			Destination.JB_RL_NKPortOfDischarge = OverseasPort;

			Sailing = Factory.New<JobSailing>();
			Sailing.JX_JA = Origin.PK;
			Sailing.JX_JB = Destination.PK;

			Shipment1 = CreateBooking();
			Shipment1.JS_JX = Sailing.PK;
			Shipment2 = CreateBooking();
			Shipment2.JS_JX = Sailing.PK;
			Shipment3 = CreateBooking();
			Shipment3.JS_JX = Sailing.PK;

			Container1 = Sailing.AddSailingContainer();
			Container2 = Sailing.AddSailingContainer();
			Container3 = Sailing.AddSailingContainer();

			Line1a = Shipment1.OuterPackLines.AddNew();
			Line1b = Shipment1.OuterPackLines.AddNew();

			Line2a = Shipment2.OuterPackLines.AddNew();
			Line2a.JL_F3_NKPackType = "PLT";
			Line2b = Shipment2.OuterPackLines.AddNew();
			Line2b.JL_F3_NKPackType = "DRM";

			Line3a = Shipment3.OuterPackLines.AddNew();
			Line3a.JL_F3_NKPackType = "PLT";
			Line3b = Shipment3.OuterPackLines.AddNew();
			Line3b.JL_F3_NKPackType = "BOX";
			Line3c = Shipment3.OuterPackLines.AddNew();
			Line3c.JL_F3_NKPackType = "DRM";

			Container1.AddPackLine(Line1a);

			Container3.AddPackLine(Line3a);
			Container3.AddPackLine(Line2b);
			Container2.AddPackLine(Line3b);
			Container2.AddPackLine(Line2a);
		}

		BuildConsolHelper Helper;

		CommonShipment Shipment1;
		CommonShipment Shipment2;
		CommonShipment Shipment3;
		JobVoyage Voyage;
		VoyageOrigin Origin;
		VoyageDestination Destination;
		JobSailing Sailing;
		PackLine Line1a;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in SetUp")]
		PackLine Line1b;
		PackLine Line2a;
		PackLine Line2b;
		PackLine Line3a;
		PackLine Line3b;
		PackLine Line3c;
		CommonContainer Container1;
		CommonContainer Container2;
		CommonContainer Container3;

		#endregion
	}
}
