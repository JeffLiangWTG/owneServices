using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(BillContainersFilterStrip))]
	internal class BillContainersFilterStripTest : FilterStripBusinessObjectTestCase
	{
		public void TestBlankAndFCLShipmentContainersAreFound()
		{
			var billOfLading = Factory.New<AgencyShipment>();
			billOfLading.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			var blankContainer = billOfLading.RealContainers.AddNew();
			blankContainer.JC_ContainerMode = ZString.Empty;
			var fclContainer = billOfLading.RealContainers.AddNew();
			fclContainer.JC_ContainerMode = Constants.ContainerModes.FCL;
			var groupageContainer = billOfLading.RealContainers.AddNew();
			groupageContainer.JC_ContainerMode = Constants.ContainerModes.Groupage;
			var lclContainer = billOfLading.RealContainers.AddNew();
			lclContainer.JC_ContainerMode = Constants.ContainerModes.LCL;
			var buyersConsolContainer = billOfLading.RealContainers.AddNew();
			buyersConsolContainer.JC_ContainerMode = Constants.ContainerModes.BuyersConsol;
			Factory.Save();
			AssertContainsExactElementsInAnyOrder("filter should find FCL shipment and empty mode containers", new[] { blankContainer, fclContainer, lclContainer, groupageContainer, buyersConsolContainer }, Factory.Load<AgencyShipmentContainer>(FilterStrip.Filter));
		}

		public void TestBaseFilter()
		{
			Shipment1.JS_IsShipping = true;
			Shipment1.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			AgencyShipmentContainer bookedContainer1 = Shipment1.BookedContainers.AddNew();
			bookedContainer1.JC_ContainerNum = "bookedContainer1";
			Asserter.AddToScope(bookedContainer1);
			Shipment2.JS_IsShipping = true;
			Shipment2.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			AgencyShipmentContainer bookedContainer2 = Shipment2.BookedContainers.AddNew();
			bookedContainer2.JC_ContainerNum = "bookedContainer2";
			Asserter.AddToScope(bookedContainer2);
			Shipment3.JS_IsShipping = false;
			Shipment3.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			AgencyShipmentContainer bookedContainer3 = Shipment3.BookedContainers.AddNew();
			bookedContainer3.JC_ContainerNum = "bookedContainer3";
			Asserter.AddToScope(bookedContainer3);
			Container1A.JC_IsShipperOwned = true;
			Container1B.JC_IsShipperOwned = false;
			Container2A.JC_IsShipperOwned = true;
			Container2B.JC_IsShipperOwned = false;
			Container3A.JC_IsShipperOwned = true;
			Container3B.JC_IsShipperOwned = false;
			Factory.Save();
			Asserter.AssertMatches("Base Filter", FilterStrip.Filter, Container1A, Container1B);
		}

		public void TestContainerNumber()
		{
			Container1A.JC_ContainerNum = "FAKE4100011";
			Container1B.JC_ContainerNum = "FAKE4100027";
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[BillContainersFilterStrip.Descriptions.ContainerNumber];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B);
			filter.Property = "FAKE4100027";
			Asserter.AssertMatches("Non-Empty Filter", filter, Container1B);
		}

		public void TestBookingRefFilter()
		{
			Shipment1.JS_CFSReference = "BookingRef1";
			Shipment2.JS_CFSReference = "BookingRef2";
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[BillContainersFilterStrip.Descriptions.BookingRef];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B);
			filter.Property = Shipment1.JS_CFSReference;
			Asserter.AssertMatches("Booking Ref", filter, Container1A, Container1B);
		}

		public void TestBillOfLading()
		{
			Shipment1.JS_HouseBill = "Bill1";
			Shipment2.JS_HouseBill = "Bill2";
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[BillContainersFilterStrip.Descriptions.BillOfLading];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B);
			filter.Property = "Bill2";
			Asserter.AssertMatches("Non-Empty Filter", filter, Container2A, Container2B);
		}

		public void TestInvoiceNumber()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.GenerateSailings();
			Shipment1.JS_JX = voyage.Sailings[0].PK;
			Shipment2.JS_JX = voyage.Sailings[0].PK;
			Container1A.JC_ContainerNum = Stock1.R6_ContainerNum;
			Container1B.JC_ContainerNum = Stock2.R6_ContainerNum;
			Container2A.JC_ContainerNum = Stock3.R6_ContainerNum;
			Container2B.JC_ContainerNum = Stock4.R6_ContainerNum;
			ContainerMovement movement1 = Stock1.Movements.AddNew();
			movement1.E9_JV = voyage.PK;
			movement1.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			ContainerMovement movement2 = Stock3.Movements.AddNew();
			movement2.E9_JV = voyage.PK;
			movement2.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			ContainerDetention invoice1 = Factory.New<ContainerDetention>();
			invoice1.NC_JobNumber = "DI00000100";
			invoice1.NC_OH_Client = Factory.NewWithValidTestData<OrgHeader>().PK;
			invoice1.NC_OH_Principal = Factory.NewWithValidTestData<OrgHeader>().PK;
			invoice1.NC_DetentionType = DetentionInvoiceType.Codes.Import;
			invoice1.Movements.Add(movement1);
			ContainerDetention invoice2 = Factory.New<ContainerDetention>();
			invoice2.NC_JobNumber = "DI00000101";
			invoice2.NC_OH_Client = Factory.NewWithValidTestData<OrgHeader>().PK;
			invoice2.NC_OH_Principal = Factory.NewWithValidTestData<OrgHeader>().PK;
			invoice2.NC_DetentionType = DetentionInvoiceType.Codes.Export;
			invoice2.Movements.Add(movement2);
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[BillContainersFilterStrip.Descriptions.DetentionInvoiceNumber];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B);
			filter.Property = "DI00000100";
			Asserter.AssertMatches("DI00000100", filter, Container1A);
			filter.Property = "DI00000101";
			Asserter.AssertMatches("DI00000101", filter, Container2A);
		}

		public void TestContainerDockReceiptNumberFilter()
		{
			Container1A.JC_DepartureDockReceipt = "DR0001";
			Container1B.JC_DepartureDockReceipt = "DR0002";
			Container2A.JC_DepartureDockReceipt = "DR0003";
			Container2B.JC_DepartureDockReceipt = "n/a";
			Factory.Save();
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[BillOfLadingFilterStrip.Descriptions.DockReceiptNumber];
			filter.Property = "";
			Asserter.AssertMatches("Empty", filter, Container1A, Container1B, Container2A, Container2B, Container3A, Container3B, Container4A, Container4B);
			filter.Property = "DR";
			Asserter.AssertMatches("DR", filter, Container1A, Container1B, Container2A);
			filter.Property = "DR0003";
			Asserter.AssertMatches("DR0003", filter, Container2A);
			filter.Property = "n/a";
			Asserter.AssertMatches("n/a", filter, Container2B);
		}

		public void TestShipmentNumber()
		{
			Shipment1.JS_UniqueConsignRef = "Shipment1";
			Shipment2.JS_UniqueConsignRef = "Shipment2";
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[BillContainersFilterStrip.Descriptions.ShipmentNumber];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B);
			filter.Property = "Shipment2";
			Asserter.AssertMatches("Non-Empty Filter", filter, Container2A, Container2B);
		}

		public void TestAvailabilityDate()
		{
			ZDateTime today = ZDateTime.Today;
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage1.GenerateSailings();
			voyage1.Destinations[0].JB_AvailabilityDate = today.AddDays(-2);
			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "SGSIN";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage2.GenerateSailings();
			Shipment1.JS_JX = voyage1.Sailings[0].PK;
			Shipment2.JS_JX = voyage2.Sailings[0].PK;
			Factory.Save();
			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[BillContainersFilterStrip.Descriptions.AvailabilityDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B);
			filter.Property1 = today.AddDays(-1);
			Asserter.AssertMatches("-1 < -2", filter);
			filter.Property1 = today.AddDays(-3);
			Asserter.AssertMatches("-3 < -2", filter, Container1A, Container1B);
			filter.Property2 = today.AddDays(-1);
			Asserter.AssertMatches("-3 < -2 < -1", filter, Container1A, Container1B);
			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("-2 < -1", filter, Container1A, Container1B);
			filter.Property2 = today.AddDays(-3);
			Asserter.AssertMatches("-2 < -3", filter);
		}

		public void TestEmptyRequiredBy()
		{
			ZDateTime now = ZDateTime.Now;
			Container1A.JC_EmptyReturnedBy = now.AddDays(1);
			Container1B.JC_EmptyReturnedBy = now.AddDays(2);
			Container2A.JC_EmptyReturnedBy = now.AddDays(3);
			Container2B.JC_EmptyReturnedBy = now.AddDays(4);
			Factory.Save();
			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[BillContainersFilterStrip.Descriptions.EmptyRequiredBy];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B);
			filter.Property1 = now.AddDays(2);
			Asserter.AssertMatches("from-", filter, Container1B, Container2A, Container2B);
			filter.Property2 = now.AddDays(3);
			Asserter.AssertMatches("from-to", filter, Container1B, Container2A);
			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("-to", filter, Container1A, Container1B, Container2A);
		}

		public void TestEmptyReturned()
		{
			ZDateTime now = ZDateTime.Now;
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.GenerateSailings();
			Shipment1.JS_JX = voyage.Sailings[0].PK;
			Shipment2.JS_JX = voyage.Sailings[0].PK;
			Container1A.JC_ContainerNum = Stock1.R6_ContainerNum;
			Container1B.JC_ContainerNum = Stock2.R6_ContainerNum;
			Container2A.JC_ContainerNum = Stock3.R6_ContainerNum;
			Container2B.JC_ContainerNum = Stock4.R6_ContainerNum;
			Container3A.JC_ContainerNum = Stock5.R6_ContainerNum;
			Container3B.JC_ContainerNum = Stock6.R6_ContainerNum;
			ContainerMovement movement1 = stock1.Movements.AddNew();
			movement1.E9_JV = voyage.PK;
			movement1.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement1.E9_MovementDate = now.AddDays(1);
			ContainerMovement movement2 = Stock2.Movements.AddNew();
			movement2.E9_JV = voyage.PK;
			movement2.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement2.E9_MovementDate = now.AddDays(2);
			ContainerMovement movement3 = Stock3.Movements.AddNew();
			movement3.E9_JV = voyage.PK;
			movement3.E9_MovementType = ContainerMovementTypes.Codes.ReturnToWharf;
			movement3.E9_MovementDate = now.AddDays(3);
			ContainerMovement movement4 = Stock4.Movements.AddNew();
			movement4.E9_JV = voyage.PK;
			movement4.E9_MovementType = ContainerMovementTypes.Codes.ReturnToWharf;
			movement4.E9_MovementDate = now.AddDays(4);
			var movement5 = Stock5.Movements.AddNew();
			movement5.E9_JV = voyage.PK;
			movement5.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			movement5.E9_MovementDate = now.AddDays(1);
			var movement6 = Stock6.Movements.AddNew();
			movement6.E9_JV = voyage.PK;
			movement6.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement6.E9_MovementDate = now.AddDays(2);
			Factory.Save();
			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[BillContainersFilterStrip.Descriptions.EmptyReturned];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B, Container3A, Container3B);
			filter.Property1 = now.AddDays(2);
			Asserter.AssertMatches("", filter, Container1B, Container2A, Container2B);
			filter.Property2 = now.AddDays(3);
			Asserter.AssertMatches("", filter, Container1B, Container2A);
			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("", filter, Container1A, Container1B, Container2A);
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("HasNoDateEntered should return containers with no linked movements", filter, Container3A, Container3B);
		}

		public void TestWharfGateOut()
		{
			ZDateTime now = ZDateTime.Now;
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.GenerateSailings();
			Shipment1.JS_JX = voyage.Sailings[0].PK;
			Shipment2.JS_JX = voyage.Sailings[0].PK;
			Container1A.JC_ContainerNum = Stock1.R6_ContainerNum;
			Container1B.JC_ContainerNum = Stock2.R6_ContainerNum;
			Container2A.JC_ContainerNum = Stock3.R6_ContainerNum;
			Container2B.JC_ContainerNum = Stock4.R6_ContainerNum;
			Container3A.JC_ContainerNum = Stock5.R6_ContainerNum;
			Container3B.JC_ContainerNum = Stock6.R6_ContainerNum;
			ContainerMovement movement1 = Stock1.Movements.AddNew();
			movement1.E9_JV = voyage.PK;
			movement1.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			movement1.E9_MovementDate = now.AddDays(1);
			ContainerMovement movement2 = Stock2.Movements.AddNew();
			movement2.E9_JV = voyage.PK;
			movement2.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			movement2.E9_MovementDate = now.AddDays(2);
			ContainerMovement movement3 = Stock3.Movements.AddNew();
			movement3.E9_JV = voyage.PK;
			movement3.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			movement3.E9_MovementDate = now.AddDays(3);
			ContainerMovement movement4 = Stock4.Movements.AddNew();
			movement4.E9_JV = voyage.PK;
			movement4.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			movement4.E9_MovementDate = now.AddDays(4);
			var movement5 = Stock5.Movements.AddNew();
			movement5.E9_JV = voyage.PK;
			movement5.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			movement5.E9_MovementDate = now.AddDays(1);
			var movement6 = Stock6.Movements.AddNew();
			movement6.E9_JV = voyage.PK;
			movement6.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			movement6.E9_MovementDate = now.AddDays(2);
			Factory.Save();
			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[BillContainersFilterStrip.Descriptions.WharfGateOut];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B, Container3A, Container3B);
			filter.Property1 = now.AddDays(2);
			Asserter.AssertMatches("", filter, Container1B, Container2A, Container2B);
			filter.Property2 = now.AddDays(3);
			Asserter.AssertMatches("", filter, Container1B, Container2A);
			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("", filter, Container1A, Container1B, Container2A);
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("HasNoDateEntered should return containers with no linked movements", filter, Container3A, Container3B);
		}

		public void TestEstimatedAndActualTimeDeparture()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			var sailing1 = voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "NLAMS");
			sailing1.Origin.JA_E_DEP = new ZDateTime(2010, 1, 1);
			sailing1.Origin.JA_A_DEP = new ZDateTime(2011, 1, 1);
			var sailing2 = voyage.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "NLAMS");
			sailing2.Origin.JA_E_DEP = new ZDateTime(2010, 2, 2);
			sailing2.Origin.JA_A_DEP = new ZDateTime(2011, 2, 2);
			var sailing3 = voyage.Sailings.GetSailingFromLoadAndDischarge("AUMEL", "NLAMS");
			sailing3.Origin.JA_E_DEP = ZDateTime.Empty;
			sailing3.Origin.JA_A_DEP = ZDateTime.Empty;
			Shipment1.JS_JX = sailing1.PK;
			Shipment2.JS_JX = sailing2.PK;
			Shipment3.JS_JX = sailing3.PK;
			Shipment4.JS_JX = ZGuid.Empty; // Shipment with no matching sailing
			Factory.Save();
			// Actual Time Departure
			var actualFilter = (ModuleDateFilter)FilterStrip[BillContainersFilterStrip.Descriptions.ActualTimeDeparture];
			actualFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			actualFilter.Property1 = ZDateTime.Empty;
			actualFilter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty ATD Filter Matches All Containers.", actualFilter, Container1A, Container1B, Container2A, Container2B, Container3A, Container3B, Container4A, Container4B);
			actualFilter.Property1 = sailing1.Origin.JA_A_DEP;
			actualFilter.Property2 = sailing1.Origin.JA_A_DEP.AddHours(1);
			Asserter.AssertMatches("Sailing1 ATD matches Shipment1 Containers.", actualFilter, Container1A, Container1B);
			actualFilter.Property1 = sailing2.Origin.JA_A_DEP;
			actualFilter.Property2 = sailing2.Origin.JA_A_DEP.AddHours(1);
			Asserter.AssertMatches("Sailing2 ATD matches Shipment2 Containers.", actualFilter, Container2A, Container2B);
			actualFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has ATA Date Entered matches Shipment1 and Shipment2 Containers.", actualFilter, Container1A, Container1B, Container2A, Container2B);
			actualFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No ATA Date Enetered matches Shipment3 and Shipment4 Containers.", actualFilter, Container3A, Container3B, Container4A, Container4B);
			// Estimated Time Departure
			var estimatedFilter = (ModuleDateFilter)FilterStrip[BillContainersFilterStrip.Descriptions.EstimatedTimeDeparture];
			estimatedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			estimatedFilter.Property1 = ZDateTime.Empty;
			estimatedFilter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty ETD Filter Matches All Containers.", estimatedFilter, Container1A, Container1B, Container2A, Container2B, Container3A, Container3B, Container4A, Container4B);
			estimatedFilter.Property1 = sailing1.Origin.JA_E_DEP;
			estimatedFilter.Property2 = sailing1.Origin.JA_E_DEP.AddHours(1);
			Asserter.AssertMatches("Sailing1 ETD matches Shipment1 Containers.", estimatedFilter, Container1A, Container1B);
			AssertEquals(sailing1.Origin.JA_E_DEP, Container1A.Booking.Sailing.JX_JA_E_DEP);
			AssertEquals(sailing1.Origin.JA_E_DEP, Container1B.Booking.Sailing.JX_JA_E_DEP);
			estimatedFilter.Property1 = sailing2.Origin.JA_E_DEP;
			estimatedFilter.Property2 = sailing2.Origin.JA_E_DEP.AddHours(1);
			Asserter.AssertMatches("Sailing2 ETD matches Shipment2 Containers.", estimatedFilter, Container2A, Container2B);
			AssertEquals(sailing2.Origin.JA_E_DEP, Container2A.Booking.Sailing.JX_JA_E_DEP);
			AssertEquals(sailing2.Origin.JA_E_DEP, Container2B.Booking.Sailing.JX_JA_E_DEP);
			estimatedFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has ETD Date Entered matches Shipment1 and Shipment2 Containers.", estimatedFilter, Container1A, Container1B, Container2A, Container2B);
			estimatedFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No ETD Date Entered matches Shipment3 and Shipment4 Containers.", estimatedFilter, Container3A, Container3B, Container4A, Container4B);
			AssertEquals(ZDateTime.Empty, Container3A.Booking.Sailing.JX_JA_E_DEP);
			AssertEquals(ZDateTime.Empty, Container3B.Booking.Sailing.JX_JA_E_DEP);
			AssertEquals(null, Container4A.Booking.Sailing);
			AssertEquals(null, Container4B.Booking.Sailing);
		}

		public void TestEstimatedAndActualTimeArrival()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUMEL";
			var sailing1 = voyage.Sailings.GetSailingFromLoadAndDischarge("NLAMS", "AUSYD");
			sailing1.Destination.JB_A_ARV = new ZDateTime(2011, 1, 1);
			sailing1.Destination.JB_E_ARV = new ZDateTime(2010, 1, 1);
			var sailing2 = voyage.Sailings.GetSailingFromLoadAndDischarge("NLAMS", "AUBNE");
			sailing2.Destination.JB_A_ARV = new ZDateTime(2011, 2, 2);
			sailing2.Destination.JB_E_ARV = new ZDateTime(2010, 2, 2);
			var sailing3 = voyage.Sailings.GetSailingFromLoadAndDischarge("NLAMS", "AUMEL");
			sailing3.Destination.JB_A_ARV = ZDateTime.Empty;
			sailing3.Destination.JB_E_ARV = ZDateTime.Empty;
			Shipment1.JS_JX = sailing1.PK;
			Shipment2.JS_JX = sailing2.PK;
			Shipment3.JS_JX = sailing3.PK;
			Shipment4.JS_JX = ZGuid.Empty; // Shipment with no matching sailing
			Factory.Save();
			// Actual Time of Arrival
			var actualFilter = (ModuleDateFilter)FilterStrip[BillContainersFilterStrip.Descriptions.ActualTimeArrival];
			actualFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			actualFilter.Property1 = ZDateTime.Empty;
			actualFilter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty ATA Filter Matches All Containers.", actualFilter, Container1A, Container1B, Container2A, Container2B, Container3A, Container3B, Container4A, Container4B);
			actualFilter.Property1 = sailing1.Destination.JB_A_ARV;
			actualFilter.Property2 = sailing1.Destination.JB_A_ARV.AddHours(1);
			Asserter.AssertMatches("Sailing1 ATA matches Shipment1 Containers.", actualFilter, Container1A, Container1B);
			actualFilter.Property1 = sailing2.Destination.JB_A_ARV;
			actualFilter.Property2 = sailing2.Destination.JB_A_ARV.AddHours(1);
			Asserter.AssertMatches("Sailing2 ATA matches Shipment2 Containers.", actualFilter, Container2A, Container2B);
			actualFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has ATA Date Entered matches Shipment1 and Shipment2 Containers.", actualFilter, Container1A, Container1B, Container2A, Container2B);
			actualFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No ATA Date Entered matches Shipment3 and Shipment4 Containers.", actualFilter, Container3A, Container3B, Container4A, Container4B);
			// Estimated Time of Arrival
			var estimatedFilter = (ModuleDateFilter)FilterStrip[BillContainersFilterStrip.Descriptions.EstimatedTimeArrival];
			estimatedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			estimatedFilter.Property1 = ZDateTime.Empty;
			estimatedFilter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty ETA Filter Matches All Containers.", estimatedFilter, Container1A, Container1B, Container2A, Container2B, Container3A, Container3B, Container4A, Container4B);
			estimatedFilter.Property1 = sailing1.Destination.JB_E_ARV;
			estimatedFilter.Property2 = sailing1.Destination.JB_E_ARV.AddHours(1);
			Asserter.AssertMatches("Sailing1 ETA matches Shipment1 Containers.", estimatedFilter, Container1A, Container1B);
			AssertEquals(sailing1.Destination.JB_E_ARV, Container1A.Booking.Sailing.JX_JB_E_ARV);
			AssertEquals(sailing1.Destination.JB_E_ARV, Container1B.Booking.Sailing.JX_JB_E_ARV);
			estimatedFilter.Property1 = sailing2.Destination.JB_E_ARV;
			estimatedFilter.Property2 = sailing2.Destination.JB_E_ARV.AddHours(1);
			Asserter.AssertMatches("Sailing2 ETA matches Shipment2 Containers.", estimatedFilter, Container2A, Container2B);
			AssertEquals(sailing2.Destination.JB_E_ARV, Container2A.Booking.Sailing.JX_JB_E_ARV);
			AssertEquals(sailing2.Destination.JB_E_ARV, Container2B.Booking.Sailing.JX_JB_E_ARV);
			estimatedFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has Date ETA Entered matches Shipment1 and Shipment2 Containers.", estimatedFilter, Container1A, Container1B, Container2A, Container2B);
			estimatedFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No ETA Date Enetered matches Shipment3 and Shipment4 Containers.", estimatedFilter, Container3A, Container3B, Container4A, Container4B);
			AssertEquals(ZDateTime.Empty, Container3A.Booking.Sailing.JX_JB_E_ARV);
			AssertEquals(ZDateTime.Empty, Container3B.Booking.Sailing.JX_JB_E_ARV);
			AssertEquals(null, Container4A.Booking.Sailing);
			AssertEquals(null, Container4B.Booking.Sailing);
		}

		public void TestVoyageVessel()
		{
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "vessel1";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "vessel2";
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_VoyageFlight = "voy1";
			voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage1.GenerateSailings();
			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_VoyageFlight = "voy2";
			voyage2.JV_RV_NKVessel = vessel2.RV_FK;
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "SGSIN";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage2.GenerateSailings();
			Shipment1.JS_JX = voyage1.Sailings[0].PK;
			Shipment2.JS_JX = voyage2.Sailings[0].PK;
			Transport transport3 = Shipment3.Transports.AddNew();
			transport3.JW_VoyageFlight = "voy1";
			transport3.JW_Vessel = "vessel1";
			Transport transport4 = Shipment4.Transports.AddNew();
			transport4.JW_VoyageFlight = "voy2";
			transport4.JW_Vessel = "vessel2";
			Factory.Save();
			var filter = (VoyageVesselModuleFilter)FilterStrip[BillContainersFilterStrip.Descriptions.VoyageVessel];
			filter.VoyageFlightNo = "";
			filter.Vessel = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B, Container3A, Container3B, Container4A, Container4B);
			filter.VoyageFlightNo = "voy1";
			Asserter.AssertMatches("voyage", filter, Container1A, Container1B, Container3A, Container3B);
			filter.VoyageFlightNo = "";
			filter.Vessel = "vessel2";
			Asserter.AssertMatches("vessel2", filter, Container2A, Container2B, Container4A, Container4B);
		}

		public void TestConsignee()
		{
			OrgHeader consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			consignee1.OH_FullName = "Consignee1";
			OrgHeader consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			consignee2.OH_FullName = "Consignee2";
			Shipment1.ConsigneeDocumentaryAddress.OrganisationPK = consignee1.PK;
			Shipment2.ConsigneeDocumentaryAddress.OrganisationPK = consignee2.PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[BillContainersFilterStrip.Descriptions.Consignee];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B);
			filter.Property = consignee1.PK;
			Asserter.AssertMatches("Non-Empty Filter", filter, Container1A, Container1B);
		}

		public void TestConsignor()
		{
			OrgHeader consignor1 = Factory.NewWithValidTestData<OrgHeader>();
			consignor1.OH_FullName = "Consignor1";
			OrgHeader consignor2 = Factory.NewWithValidTestData<OrgHeader>();
			consignor2.OH_FullName = "Consignor2";
			Shipment1.ConsignorDocumentaryAddress.OrganisationPK = consignor1.PK;
			Shipment2.ConsignorDocumentaryAddress.OrganisationPK = consignor2.PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[BillContainersFilterStrip.Descriptions.Consignor];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B);
			filter.Property = consignor1.PK;
			Asserter.AssertMatches("Non-Empty Filter", filter, Container1A, Container1B);
		}

		public void TestEmptyPickupFrom()
		{
			OrgHeader header1 = Factory.NewWithValidTestData<OrgHeader>();
			header1.OH_FullName = "Header1";
			OrgHeader header2 = Factory.NewWithValidTestData<OrgHeader>();
			header2.OH_FullName = "Header2";
			Container1A.JC_OA_DepartureContainerYardAddress = header1.MainAddress.PK;
			Container2B.JC_OA_DepartureContainerYardAddress = header2.MainAddress.PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[BillContainersFilterStrip.Descriptions.EmptyPickupFrom];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B);
			filter.Property = header1.PK;
			Asserter.AssertMatches("Non-Empty Filter", filter, Container1A);
		}

		public void TestEmptyReturnTo()
		{
			OrgHeader header1 = Factory.NewWithValidTestData<OrgHeader>();
			header1.OH_FullName = "Header1";
			OrgHeader header2 = Factory.NewWithValidTestData<OrgHeader>();
			header2.OH_FullName = "Header2";
			Container1A.JC_OA_ArrivalContainerYardAddress = header1.MainAddress.PK;
			Container2B.JC_OA_ArrivalContainerYardAddress = header2.MainAddress.PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[BillContainersFilterStrip.Descriptions.EmptyReturnTo];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B);
			filter.Property = header2.PK;
			Asserter.AssertMatches("Non-Empty Filter", filter, Container2B);
		}

		public void TestLocalClient()
		{
			OrgHeader consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			consignee1.OH_FullName = "Consignee1";
			OrgHeader consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			consignee2.OH_FullName = "Consignee2";
			JobHeader header1 = new JobHeader.Loader(Shipment1).TryLoadOrCreate();
			header1.JH_OA_LocalChargesAddr = consignee1.MainAddress.PK;
			header1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			JobHeader header2 = new JobHeader.Loader(Shipment2).TryLoadOrCreate();
			header2.JH_OA_LocalChargesAddr = consignee2.MainAddress.PK;
			header2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[BillContainersFilterStrip.Descriptions.LocalClient];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B);
			filter.Property = consignee1.PK;
			Asserter.AssertMatches("Non-Empty Filter", filter, Container1A, Container1B);
		}

		public void TestPrincipal()
		{
			OrgHeader consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			consignee1.OH_FullName = "Consignee1";
			OrgHeader consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			consignee2.OH_FullName = "Consignee2";
			Shipment1.JS_OH_DeliveryAgent = consignee1.PK;
			Shipment2.JS_OH_DeliveryAgent = consignee2.PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[BillContainersFilterStrip.Descriptions.Principal];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B);
			filter.Property = consignee1.PK;
			Asserter.AssertMatches("Non-Empty Filter", filter, Container1A, Container1B);
		}

		public void TestPrincipalSecurity()
		{
			const string expectedError = "Please select a principal to filter by";
			OrgHeader principal = Factory.New<OrgHeader>();
			principal.OH_Code = "Principal";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			GlbStaff staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			((IOrgsAndWarehousesAccessProvider)staff).AddSecurityToAccessOrgOrWarehouse(principal.OH_Code);
			Factory.Save();
			{
				Env.Security.AgencyPrincipalAccess.IsAllowed = true;
				filterStrip = null;
				ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[BillContainersFilterStrip.Descriptions.Principal];
				AssertEquals(FilterVisibility.Visible, filter.Visibility);
				filter.Property = principal.PK;
				AssertNoNotifications(filter.PropertyInfo);
				filter.Property = ZGuid.Empty;
				AssertNoNotifications(filter.PropertyInfo);
			}

			{
				Env.Security.AgencyPrincipalAccess.IsAllowed = false;
				filterStrip = null;
				ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[BillContainersFilterStrip.Descriptions.Principal];
				AssertEquals(FilterVisibility.AlwaysVisible, filter.Visibility);
				filter.Property = principal.PK;
				AssertNoNotifications(filter.PropertyInfo);
				filter.Property = ZGuid.Empty;
				AssertHasError(filter.PropertyInfo, expectedError);
			}
		}

		public void TestBranchRelatedPorts()
		{
			GlbCompany newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Code = "BLT";
			GlbBranch branch1 = newCompany.Branches.AddNew();
			branch1.GB_Code = "BN1";
			branch1.GB_RL_NKHomePort = "AUSYD";
			GlbBranch branch2 = newCompany.Branches.AddNew();
			branch2.GB_Code = "BN2";
			branch2.GB_RL_NKHomePort = "AUMEL";
			branch2.ExtraPorts.AddNew().GY_RL_NKAdditionalBranchRelatedPort = "AUSYD";
			GlbBranch branch3 = newCompany.Branches.AddNew();
			branch3.GB_Code = "BN3";
			branch3.GB_RL_NKHomePort = "AUADL";
			branch3.ExtraPorts.AddNew().GY_RL_NKAdditionalBranchRelatedPort = "AUCNS";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			JobSailing sailing1 = voyage.Sailings.GetSailingFromLoadAndDischarge("NLAMS", "AUSYD");
			JobSailing sailing2 = voyage.Sailings.GetSailingFromLoadAndDischarge("NLAMS", "AUBNE");
			Shipment1.JS_JX = sailing1.PK;
			Shipment2.JS_JX = sailing2.PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[BillContainersFilterStrip.Descriptions.BranchRelatedPorts];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B);
			filter.Property = branch1.PK;
			Asserter.AssertMatches("branch1", filter, Container1A, Container1B);
			filter.Property = branch2.PK;
			Asserter.AssertMatches("branch2", filter, Container1A, Container1B);
			filter.Property = branch3.PK;
			Asserter.AssertMatches("branch3", filter);
		}

		public void TestBranchRelatedPortsSecurity()
		{
			const string error = "Your current security rights only allow you to view shipments relating to your current login branch.\r\n" + "If you think this is incorrect, please contact your system administrator." + "";
			GlbBranch otherBranch = Factory.Load<GlbCompany>(Env.CurrentCompany.PK).Branches.AddNew();
			otherBranch.GB_Code = "OBN";
			Factory.Save();
			{
				Env.Security.AgencyBillContainersAllowSearchOfUnlocoOutsideLoginBranch.IsAllowed = true;
				filterStrip = null;
				ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[BillContainersFilterStrip.Descriptions.BranchRelatedPorts];
				AssertEquals(FilterVisibility.Visible, filter.Visibility);
				AssertEquals(ZGuid.Empty, filter.DefaultProperty);
				filter.Property = otherBranch.PK;
				AssertNoNotifications(filter.PropertyInfo);
				filter.Property = Env.CurrentBranch.PK;
				AssertNoNotifications(filter.PropertyInfo);
				filter.Property = ZGuid.Empty;
				AssertNoNotifications(filter.PropertyInfo);
			}

			{
				Env.Security.AgencyBillContainersAllowSearchOfUnlocoOutsideLoginBranch.IsAllowed = false;
				filterStrip = null;
				ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[BillContainersFilterStrip.Descriptions.BranchRelatedPorts];
				AssertEquals(FilterVisibility.AlwaysVisible, filter.Visibility);
				AssertEquals(Env.CurrentBranch.PK, filter.DefaultProperty);
				filter.Property = otherBranch.PK;
				AssertHasError(filter.PropertyInfo, error);
				filter.Property = Env.CurrentBranch.PK;
				AssertNoNotifications(filter.PropertyInfo);
				filter.Property = ZGuid.Empty;
				AssertHasError(filter.PropertyInfo, error);
			}
		}

		public void TestLoadDischarge()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "SGSIN";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLNEC";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			JobSailing sailing1 = voyage.Sailings.GetSailingFromLoadAndDischarge("SGSIN", "AUSYD");
			JobSailing sailing2 = voyage.Sailings.GetSailingFromLoadAndDischarge("NLAMS", "AUBNE");
			JobSailing sailing3 = voyage.Sailings.GetSailingFromLoadAndDischarge("NLNEC", "SGSIN");
			Shipment1.JS_JX = sailing1.PK;
			Shipment2.JS_JX = sailing2.PK;
			Shipment3.JS_JX = sailing3.PK;
			Factory.Save();
			ModuleLocationFilter filter = (ModuleLocationFilter)FilterStrip[BillContainersFilterStrip.Descriptions.LoadDischarge];
			Asserter.AddFieldOfInterest("Booking.JS_NKLoadPort");
			Asserter.AddFieldOfInterest("Booking.JS_NKDischargePort");
			filter.Property1 = "";
			filter.Property2 = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B, Container3A, Container3B);
			filter.Property1 = "NL";
			Asserter.AssertMatches("Load Country", filter, Container2A, Container2B, Container3A, Container3B);
			filter.Property1 = "SGSIN";
			Asserter.AssertMatches("Load Port", filter, Container1A, Container1B);
			filter.Property1 = "";
			filter.Property2 = "AU";
			Asserter.AssertMatches("Discharge Country", filter, Container1A, Container1B, Container2A, Container2B);
			filter.Property2 = "AUBNE";
			Asserter.AssertMatches("Discharge Port", filter, Container2A, Container2B);
		}

		public void TestOriginDestination()
		{
			Shipment1.JS_RL_NKOrigin = "SGSIN";
			Shipment1.JS_RL_NKDestination = "AUSYD";
			Shipment2.JS_RL_NKOrigin = "NLAMS";
			Shipment2.JS_RL_NKDestination = "AUBNE";
			Shipment3.JS_RL_NKOrigin = "NLNEC";
			Shipment3.JS_RL_NKDestination = "SGSIN";
			Factory.Save();
			ModuleLocationFilter filter = (ModuleLocationFilter)FilterStrip[BillContainersFilterStrip.Descriptions.OriginDestination];
			Asserter.AddFieldOfInterest("Booking+JS_RL_NKOrigin");
			Asserter.AddFieldOfInterest("Booking+JS_RL_NKDestination");
			filter.Property1 = "";
			filter.Property2 = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B, Container3A, Container3B);
			filter.Property1 = "NL";
			Asserter.AssertMatches("Origin Country", filter, Container2A, Container2B, Container3A, Container3B);
			filter.Property1 = "SGSIN";
			Asserter.AssertMatches("Origin Port", filter, Container1A, Container1B);
			filter.Property1 = "";
			filter.Property2 = "AU";
			Asserter.AssertMatches("Destination Country", filter, Container1A, Container1B, Container2A, Container2B);
			filter.Property2 = "AUBNE";
			Asserter.AssertMatches("Destination Port", filter, Container2A, Container2B);
		}

		public void TestAvailabilityStatus()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			JobSailing sailing1 = voyage.Sailings.GetSailingFromLoadAndDischarge("NLAMS", "AUSYD");
			sailing1.Destination.JB_A_ARV = ZDateTime.Empty;
			JobSailing sailing2 = voyage.Sailings.GetSailingFromLoadAndDischarge("NLAMS", "AUBNE");
			sailing2.Destination.JB_A_ARV = ZDateTime.Now;
			Shipment1.JS_JX = sailing1.PK;
			Shipment2.JS_JX = sailing2.PK;
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[BillContainersFilterStrip.Descriptions.AvailabilityStatus];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B);
			filter.Property = BillContainersFilterStrip.AvailabilityStatus.NotAvailable;
			Asserter.AssertMatches("Not Available", filter, Container1A, Container1B);
			filter.Property = BillContainersFilterStrip.AvailabilityStatus.Available;
			Asserter.AssertMatches("Available", filter, Container2A, Container2B);
		}

		[EIDOMessagingConfiguration]
		public void TestEIDOStatusFilter()
		{
			ZDateTime now = ZDateTime.Now;
			SendEIDOMessage(Container1B, EIDOMessageFunction.Original, now, 1);
			ResponseMessage(SendEIDOMessage(Container2A, EIDOMessageFunction.Original, now, 1), EIDOResponseType.Accepted);
			ResponseMessage(SendEIDOMessage(Container2B, EIDOMessageFunction.Original, now, 1), EIDOResponseType.Rejected);
			ResponseMessage(SendEIDOMessage(Container3A, EIDOMessageFunction.Original, now, 1), EIDOResponseType.Accepted);
			ResponseMessage(SendEIDOMessage(Container3B, EIDOMessageFunction.Original, now, 1), EIDOResponseType.Accepted);
			ResponseMessage(SendEIDOMessage(Container3A, EIDOMessageFunction.Cancelation, now, 2), EIDOResponseType.Rejected);
			ResponseMessage(SendEIDOMessage(Container3B, EIDOMessageFunction.Cancelation, now, 2), EIDOResponseType.Accepted);
			ResponseMessage(SendEIDOMessage(Container4A, EIDOMessageFunction.Original, now, 1), EIDOResponseType.Received);
			ResponseMessage(SendEIDOMessage(Container4B, EIDOMessageFunction.Original, now, 1), EIDOResponseType.Accepted);
			ResponseMessage(SendEIDOMessage(Container4B, EIDOMessageFunction.Cancelation, now, 2), EIDOResponseType.Received);
			Factory.Save();
			Asserter.AddFieldOfInterest("JC_ImportReleaseOrderStatus");
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[BillContainersFilterStrip.Descriptions.EIDOStatus];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B, Container3A, Container3B, Container4A, Container4B);
			filter.Property = EIDOFilterList.Codes.Accepted;
			Asserter.AssertMatches("Accepted", filter, Container4A);
			filter.Property = EIDOFilterList.Codes.Acknowledged;
			asserter.AssertMatches("Acknowledged", filter, container2A);
			filter.Property = EIDOFilterList.Codes.NotSent;
			Asserter.AssertMatches("NotSent", filter, Container1A, Container3B, Container4B);
			filter.Property = EIDOFilterList.Codes.PendingResponse;
			Asserter.AssertMatches("PendingResponse", filter, Container1B);
			filter.Property = EIDOFilterList.Codes.Rejected;
			Asserter.AssertMatches("Rejected", filter, Container2B, Container3A);
		}

		public void TestReturnedStatus()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.GenerateSailings();
			Shipment1.JS_JX = voyage.Sailings[0].PK;
			Shipment2.JS_JX = voyage.Sailings[0].PK;
			Shipment3.JS_JX = voyage.Sailings[0].PK;
			Container1A.JC_ContainerNum = Stock1.R6_ContainerNum;
			Container1B.JC_ContainerNum = Stock2.R6_ContainerNum;
			Container2A.JC_ContainerNum = Stock3.R6_ContainerNum;
			Container2B.JC_ContainerNum = Stock4.R6_ContainerNum;
			Container3A.JC_ContainerNum = Stock5.R6_ContainerNum;
			Container3B.JC_ContainerNum = Stock6.R6_ContainerNum;
			ContainerMovement movement1 = Stock1.Movements.AddNew();
			movement1.E9_JV = voyage.PK;
			movement1.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement1.E9_MovementDate = ZDateTime.Now;
			ContainerMovement movement2 = Stock2.Movements.AddNew();
			movement2.E9_JV = voyage.PK;
			movement2.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement2.E9_MovementDate = ZDateTime.Empty;
			ContainerMovement movement3 = Stock3.Movements.AddNew();
			movement2.E9_JV = voyage.PK;
			movement3.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
			movement3.E9_MovementDate = ZDateTime.Now;
			ContainerMovement movement5 = Stock5.Movements.AddNew();
			movement5.E9_JV = voyage.PK;
			movement5.E9_MovementType = ContainerMovementTypes.Codes.ReturnToWharf;
			movement5.E9_MovementDate = ZDateTime.Now;
			ContainerMovement movement6 = Stock6.Movements.AddNew();
			movement6.E9_MovementType = ContainerMovementTypes.Codes.ReturnToWharf;
			movement6.E9_MovementDate = ZDateTime.Now;
			Container2B.Movements.DeleteAll();
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[BillContainersFilterStrip.Descriptions.ReturnedStatus];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B, Container3A, Container3B);
			filter.Property = BillContainersFilterStrip.ReturnedStatus.NotReturned;
			Asserter.AssertMatches("Not Returned", filter, Container1B, Container2A, Container2B, Container3B);
			filter.Property = BillContainersFilterStrip.ReturnedStatus.Returned;
			Asserter.AssertMatches("Returned", filter, Container1A, Container3A);
		}

		public void TestImportInvoiceStatus()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.GenerateSailings();
			Shipment1.JS_JX = voyage.Sailings[0].PK;
			Shipment2.JS_JX = voyage.Sailings[0].PK;
			Container1A.JC_ContainerNum = Stock1.R6_ContainerNum;
			Container1B.JC_ContainerNum = Stock2.R6_ContainerNum;
			Container2A.JC_ContainerNum = Stock3.R6_ContainerNum;
			Container2B.JC_ContainerNum = Stock4.R6_ContainerNum;
			ContainerMovement movement1 = Stock1.Movements.AddNew();
			movement1.E9_JV = voyage.PK;
			movement1.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			ContainerMovement movement2 = Stock3.Movements.AddNew();
			movement2.E9_JV = voyage.PK;
			movement2.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			ContainerDetention invoice1 = Factory.New<ContainerDetention>();
			invoice1.NC_JobNumber = "DI00000100";
			invoice1.NC_OH_Client = Factory.NewWithValidTestData<OrgHeader>().PK;
			invoice1.NC_OH_Principal = Factory.NewWithValidTestData<OrgHeader>().PK;
			invoice1.NC_DetentionType = DetentionInvoiceType.Codes.Import;
			invoice1.Movements.Add(movement1);
			ContainerDetention invoice2 = Factory.New<ContainerDetention>();
			invoice2.NC_JobNumber = "DI00000101";
			invoice2.NC_OH_Client = Factory.NewWithValidTestData<OrgHeader>().PK;
			invoice2.NC_OH_Principal = Factory.NewWithValidTestData<OrgHeader>().PK;
			invoice2.NC_DetentionType = DetentionInvoiceType.Codes.Export;
			invoice2.Movements.Add(movement2);
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[BillContainersFilterStrip.Descriptions.ImportInvoiceStatus];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B);
			filter.Property = BillContainersFilterStrip.InvoiceStatus.Invoiced;
			Asserter.AssertMatches("Invoiced", filter, Container1A);
			filter.Property = BillContainersFilterStrip.InvoiceStatus.NotInvoiced;
			Asserter.AssertMatches("Not Invoiced", filter, Container1B, Container2A, Container2B);
		}

		public void TestExportInvoiceStatus()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.GenerateSailings();
			Shipment1.JS_JX = voyage.Sailings[0].PK;
			Shipment2.JS_JX = voyage.Sailings[0].PK;
			Container1A.JC_ContainerNum = Stock1.R6_ContainerNum;
			Container1B.JC_ContainerNum = Stock2.R6_ContainerNum;
			Container2A.JC_ContainerNum = Stock3.R6_ContainerNum;
			Container2B.JC_ContainerNum = Stock4.R6_ContainerNum;
			ContainerMovement movement1 = Stock1.Movements.AddNew();
			movement1.E9_JV = voyage.PK;
			movement1.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			ContainerMovement movement2 = Stock3.Movements.AddNew();
			movement2.E9_JV = voyage.PK;
			movement2.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			ContainerDetention invoice1 = Factory.New<ContainerDetention>();
			invoice1.NC_JobNumber = "DI00000100";
			invoice1.NC_OH_Client = Factory.NewWithValidTestData<OrgHeader>().PK;
			invoice1.NC_OH_Principal = Factory.NewWithValidTestData<OrgHeader>().PK;
			invoice1.NC_DetentionType = DetentionInvoiceType.Codes.Import;
			invoice1.Movements.Add(movement1);
			ContainerDetention invoice2 = Factory.New<ContainerDetention>();
			invoice2.NC_JobNumber = "DI00000101";
			invoice2.NC_OH_Client = Factory.NewWithValidTestData<OrgHeader>().PK;
			invoice2.NC_OH_Principal = Factory.NewWithValidTestData<OrgHeader>().PK;
			invoice2.NC_DetentionType = DetentionInvoiceType.Codes.Export;
			invoice2.Movements.Add(movement2);
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[BillContainersFilterStrip.Descriptions.ExportInvoiceStatus];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B);
			filter.Property = BillContainersFilterStrip.InvoiceStatus.Invoiced;
			Asserter.AssertMatches("Invoiced", filter, Container2A);
			filter.Property = BillContainersFilterStrip.InvoiceStatus.NotInvoiced;
			Asserter.AssertMatches("Not Invoiced", filter, Container1A, Container1B, Container2B);
		}

		public void TestContinerType()
		{
			Container1A.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Container1B.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[BillContainersFilterStrip.Descriptions.ContainerType];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B);
			filter.Property = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Asserter.AssertMatches("20GP", filter, Container1A);
		}

		public void TestContainerTypeCategory()
		{
			Container1A.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Container1B.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[BillContainersFilterStrip.Descriptions.ContainerTypeCategory];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B);
			filter.Property = "DRY";
			Asserter.AssertMatches("DRY", filter, Container1A);
		}

		public void TestContainerQuality()
		{
			Container1A.JC_ContainerQuality = "AA";
			Container1B.JC_ContainerQuality = "BB";
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[BillContainersFilterStrip.Descriptions.ContainerQuality];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B);
			filter.Property = "AA";
			Asserter.AssertMatches("AA", filter, Container1A);
		}

		public void TestEmptyContainer()
		{
			Container1A.JC_IsEmptyContainer = true;
			Container1B.JC_IsEmptyContainer = false;
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[BillContainersFilterStrip.Descriptions.EmptyContainer];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B);
			filter.Property = BillContainersFilterStrip.EmptyStatus.Empty;
			Asserter.AssertMatches("Empty Container", filter, Container1A);
			filter.Property = BillContainersFilterStrip.EmptyStatus.NotEmpty;
			Asserter.AssertMatches("Non-Empty Container", filter, Container1B);
		}

		public void TestShipperOwned()
		{
			Container1A.JC_IsShipperOwned = true;
			Container1B.JC_IsShipperOwned = false;
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[BillContainersFilterStrip.Descriptions.ShipperOwned];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B);
			filter.Property = BillContainersFilterStrip.ShipperOwnedStatus.ShipperOwned;
			Asserter.AssertMatches("Shipper Owned", filter, Container1A);
			filter.Property = BillContainersFilterStrip.ShipperOwnedStatus.NotShipperOwned;
			Asserter.AssertMatches("Not Shipper Owned", filter, Container1B);
		}

		public void TestStorageClass()
		{
			Container1A.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Container1B.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[BillContainersFilterStrip.Descriptions.StorageClass];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B);
			filter.Property = "20F";
			Asserter.AssertMatches("20F", filter, Container1A);
		}

		public void TestEntryTypeAndNumber()
		{
			Container1A.CustomsEntryNumberType = "CAN";
			Container1A.CustomsEntryNumber = "123";
			Container1B.CustomsEntryNumberType = "CCN";
			Container1B.CustomsEntryNumber = "456";
			CusEntryNumber shipmentNumber = Shipment2.CusEntryNumbers.AddNew();
			shipmentNumber.CE_ParentID = Shipment2.PK;
			shipmentNumber.CE_ParentTable = Shipment2.TableName;
			shipmentNumber.CE_EntryType = "CAN";
			shipmentNumber.CE_EntryNum = "456";
			SetupShipment3();
			Container4A.CustomsEntryNumberType = "EXDC";
			Factory.Save();
			Asserter.AddFieldOfInterest("CustomsEntryNumberType");
			Asserter.AddFieldOfInterest("CustomsEntryNumber");
			Asserter.AddFieldOfInterest("Booking.CustomsEntryNumberType");
			Asserter.AddFieldOfInterest("Booking.CustomsEntryNumber");
			EntryNumberModuleFilter filter = (EntryNumberModuleFilter)FilterStrip[BillContainersFilterStrip.Descriptions.EntryTypeAndNumber];
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B, Container3A, Container3B, Container4A, Container4B);
			filter.EntryType = "CAN";
			Asserter.AssertMatches("Non-Empty Filter", filter, Container1A, Container2A, Container2B);
			filter.EntryType = "CCN";
			Asserter.AssertMatches("Non-Empty Filter", filter, Container1B);
			filter.EntryType = "";
			filter.Property = "456";
			Asserter.AssertMatches("Non-Empty Filter", filter, Container1B, Container2A, Container2B);
			filter.EntryType = "EXDC";
			Asserter.AssertMatches("Non-Empty Filter", filter, Container4A);
		}

		#region TestAdditionalReferenceNumbers
		public void TestAdditionalReferenceNumbers()
		{
			NewReferenceNumber(Container1A, "AU", "COC", "MUNDANE");
			NewReferenceNumber(Container1B, "US", "COC", "MAGIC");
			NewReferenceNumber(Container2A, "AU", "ASL", "APPLE");
			NewReferenceNumber(Container2B, "US", "ASL", "MAGIC");
			Factory.Save();
			var filter = (ReferenceNumberFilter)FilterStrip[BillContainersFilterStrip.Descriptions.AdditionalReferenceNumbers];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B, Container2A, Container2B);
			SetFilter(filter, "US", "COC", "MAGIC");
			Asserter.AssertMatches("US:COC:MAGIC", filter, Container1B);
			SetFilter(filter, "US", string.Empty, "MAGIC");
			Asserter.AssertMatches("US:MAGIC", filter, Container1B, Container2B);
			SetFilter(filter, "AU", string.Empty, "A");
			Asserter.AssertMatches("AU:A*", filter, Container2A);
			SetFilter(filter, string.Empty, "COC", "M");
			Asserter.AssertMatches("COC:M*", filter, Container1A, Container1B);
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			SetFilter(filter, string.Empty, "AMS", string.Empty);
			Asserter.AssertMatches("IsBlank AMS", filter, Container1A, Container1B, Container2A, Container2B);
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			SetFilter(filter, string.Empty, "ASL", string.Empty);
			Asserter.AssertMatches("IsNotBlank ASL", filter, Container2A, Container2B);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			SetFilter(filter, string.Empty, "ASL", string.Empty);
			Asserter.AssertMatches("NotEqual ASL", filter, Container1A, Container1B);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			SetFilter(filter, string.Empty, "COC", "ANE");
			Asserter.AssertMatches("NotContains COC:ANE", filter, Container1B, Container2A, Container2B);
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			SetFilter(filter, "", "ASL", "M");
			Asserter.AssertMatches("DoesNotStartWith ASL:M", filter, Container1A, Container1B, Container2A);
		}

		static CusEntryNumber NewReferenceNumber(CommonContainer container, string countryCode, string type, string number)
		{
			var result = container.AdditionalReferenceNumbers.AddNew();
			result.CE_RN_NKCountryCode = countryCode;
			result.CE_EntryType = type;
			result.CE_EntryNum = number;
			return result;
		}

		static void SetFilter(ReferenceNumberFilter filter, string country, string type, string number)
		{
			filter.Country = country;
			filter.Type = type;
			filter.Property = number;
		}

		#endregion
		public void TestBOLGroupedFilters()
		{
			string[] filterNames = { BillContainersFilterStrip.Descriptions.AvailabilityDate, BillContainersFilterStrip.Descriptions.AvailabilityStatus, BillContainersFilterStrip.Descriptions.BillOfLading, BillContainersFilterStrip.Descriptions.BookingRef, BillContainersFilterStrip.Descriptions.BranchRelatedPorts, BillContainersFilterStrip.Descriptions.Hidden, BillContainersFilterStrip.Descriptions.LoadDischarge, BillContainersFilterStrip.Descriptions.Principal, BillContainersFilterStrip.Descriptions.ShipmentNumber, BillContainersFilterStrip.Descriptions.VoyageVessel, };
			List<string> failedFilterNames = new List<string>();
			foreach (string filterName in filterNames)
			{
				ModuleFilter filter = FilterStrip[filterName];
				if (filter.SubGroup != FilterStrip.BillOfLadingFilterProcessor)
				{
					failedFilterNames.Add(filterName);
				}
			}

			Dictionary<string, IList<string>> result = new Dictionary<string, IList<string>>();
			if (failedFilterNames.Count > 0)
			{
				result.Add("These filters should have been using the BillOfLadingFilterProcessor but wern't.", failedFilterNames);
			}

			AssertGroupedErrorList(result);
		}

		public void TestContainerMode()
		{
			Container1A.JC_ContainerMode = Constants.ContainerModes.Groupage;
			Container1B.JC_ContainerMode = Constants.ContainerModes.LCL;
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[BillContainersFilterStrip.Descriptions.ContainerMode];
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Empty Filter", filter, Container1A, Container1B);
			filter.Property = Constants.ContainerModes.Groupage;
			Asserter.AssertMatches("20GP", filter, Container1A);
		}

		public void TestComparisonOperatorForGuidFilters()
		{
			var errorBuilder = new ZStringBuilder();
			var filtersHideComparisonOperator = new List<string> { "Consignee", "Consignor", "Local Client", "Empty Pickup From", "Empty Return To", "Branch Related Ports" };
			foreach (var filter in FilterStrip.ModuleFilters.Where(x => filtersHideComparisonOperator.Contains(x.Description)))
			{
				var guidFilter = filter as ModuleGuidFilter;
				if (guidFilter != null && guidFilter.HasComparisonOperator)
				{
					errorBuilder.AppendLine($"Filter: {filter.Description}");
				}
			}

			if (errorBuilder.Length > 0)
			{
				errorBuilder.Prepend("Those filters should hide the comparison operator:\r\n");
				Fail(errorBuilder.ToString());
			}

			Assert(true);
		}

		public void TestPackLineRefNumber()
		{
			AssertPackLineRefNumber(JobPackLinesSchema.JL_ImportRefNumber, BillContainersFilterStrip.Descriptions.PackLineImportReference);
			AssertPackLineRefNumber(JobPackLinesSchema.JL_ExportRefNumber, BillContainersFilterStrip.Descriptions.PackLineExportReference);
		}

		void AssertPackLineRefNumber(SchemaStringColumn refNumColumn, string filterName)
		{
			var billOfLading = Factory.NewWithValidTestData<BillOfLading>();
			var container1 = billOfLading.FCLContainers.AddNew(typeof(AgencyShipmentContainer));
			container1.JC_ContainerNum = "CONT0001";
			Asserter.AddToScope(container1);
			var packline1 = billOfLading.OuterPackLines.AddNew();
			packline1[refNumColumn] = "REF-00001";
			container1.AddPackLine(packline1);
			var packline2 = billOfLading.OuterPackLines.AddNew();
			packline2[refNumColumn] = "REF-00002";
			container1.AddPackLine(packline2);
			var container2 = billOfLading.FCLContainers.AddNew(typeof(AgencyShipmentContainer));
			container2.JC_ContainerNum = "CONT002";
			Asserter.AddToScope(container2);
			var packline3 = billOfLading.OuterPackLines.AddNew();
			packline3[refNumColumn] = "REF-00002";
			container2.AddPackLine(packline3);
			var packline4 = billOfLading.OuterPackLines.AddNew();
			packline4[refNumColumn] = "REF-00003";
			container2.AddPackLine(packline4);
			var billOfLading2 = Factory.NewWithValidTestData<BillOfLading>();
			var container3 = billOfLading2.FCLContainers.AddNew(typeof(AgencyShipmentContainer));
			container3.JC_ContainerNum = "CONT003";
			Asserter.AddToScope(container3);
			var packline5 = billOfLading2.OuterPackLines.AddNew();
			packline5[refNumColumn] = "REF-00003";
			container3.AddPackLine(packline5);
			var packline6 = billOfLading2.OuterPackLines.AddNew();
			packline6[refNumColumn] = "REF-00004";
			container3.AddPackLine(packline6);
			Factory.Save();
			var filter = (ModuleTextFilter)FilterStrip[filterName];
			filter.Property = "REF-00001";
			Asserter.AssertMatches(filterName, filter, new[] { container1 });
			filter.Property = "REF-00002";
			Asserter.AssertMatches(filterName, filter, new[] { container1, container2 });
			filter.Property = "REF-00003";
			Asserter.AssertMatches(filterName, filter, new[] { container2, container3 });
			filter.Property = "REF-00004";
			Asserter.AssertMatches(filterName, filter, new[] { container3 });
		}

		#region Implementation
		FilterStripAsserter<AgencyShipmentContainer> Asserter
		{
			get
			{
				return asserter ?? (asserter = new FilterStripAsserter<AgencyShipmentContainer>(Factory, (c) => c.JC_ContainerNum));
			}
		}

		FilterStripAsserter<AgencyShipmentContainer> asserter;
		EIDOMessage SendEIDOMessage(AgencyShipmentContainer container, EIDOMessageFunction function, ZDateTime now, int offset)
		{
			EIDOMessage message = EIDOMessage.New(container, function, EDIMessage.MessageNumberPlaceHolder);
			message.EM_SystemCreateTimeUtc = now.AddMinutes(offset);
			return message;
		}

		void ResponseMessage(EIDOMessage sentMessage, EIDOResponseType responseType)
		{
			EIDOMessage message = Factory.New<EIDOMessage>();
			message.EM_ApplicationCode = EIDOMessage.ApplicationCodes.EIDO;
			message.EM_ReceiveTransmit = EIDOMessage.Direction.Receive;
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			message.EM_LinkTable = sentMessage.EM_LinkTable;
			message.EM_LinkUniqueID = sentMessage.EM_LinkUniqueID;
			message.EM_Status = EIDOMessage.Status.Recognised;
			switch (responseType)
			{
				case EIDOResponseType.Accepted:
					sentMessage.EM_Status = EIDOMessage.Status.Acknowledged;
					break;
				case EIDOResponseType.Received:
					sentMessage.EM_Status = EIDOMessage.Status.Received;
					break;
				case EIDOResponseType.Rejected:
					sentMessage.EM_Status = EIDOMessage.Status.Rejected;
					break;
			}
		}

		AgencyShipment Shipment1
		{
			get
			{
				if (shipment1 == null)
				{
					SetupShipment1();
				}

				return shipment1;
			}
		}

		AgencyShipmentContainer Container1A
		{
			get
			{
				if (container1A == null)
				{
					SetupShipment1();
				}

				return container1A;
			}
		}

		AgencyShipmentContainer Container1B
		{
			get
			{
				if (container1B == null)
				{
					SetupShipment1();
				}

				return container1B;
			}
		}

		void SetupShipment1()
		{
			shipment1 = Factory.New<AgencyShipment>();
			shipment1.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			container1A = Shipment1.RealContainers.AddNew();
			container1A.JC_ContainerNum = "Container1A";
			container1B = Shipment1.RealContainers.AddNew();
			container1B.JC_ContainerNum = "Container1B";
			Asserter.AddToScope(container1A);
			Asserter.AddToScope(container1B);
		}

		AgencyShipment shipment1;
		AgencyShipmentContainer container1A;
		AgencyShipmentContainer container1B;
		AgencyShipment Shipment2
		{
			get
			{
				if (shipment2 == null)
				{
					SetupShipment2();
				}

				return shipment2;
			}
		}

		AgencyShipmentContainer Container2A
		{
			get
			{
				if (container2A == null)
				{
					SetupShipment2();
				}

				return container2A;
			}
		}

		AgencyShipmentContainer Container2B
		{
			get
			{
				if (container2B == null)
				{
					SetupShipment2();
				}

				return container2B;
			}
		}

		void SetupShipment2()
		{
			shipment2 = Factory.New<AgencyShipment>();
			shipment2.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			container2A = Shipment2.RealContainers.AddNew();
			container2A.JC_ContainerNum = "Conatiner2A";
			container2B = Shipment2.RealContainers.AddNew();
			container2B.JC_ContainerNum = "Conatiner2B";
			Asserter.AddToScope(container2A);
			Asserter.AddToScope(container2B);
		}

		AgencyShipment shipment2;
		AgencyShipmentContainer container2A;
		AgencyShipmentContainer container2B;
		AgencyShipment Shipment3
		{
			get
			{
				if (shipment3 == null)
				{
					SetupShipment3();
				}

				return shipment3;
			}
		}

		AgencyShipmentContainer Container3A
		{
			get
			{
				if (container3A == null)
				{
					SetupShipment3();
				}

				return container3A;
			}
		}

		AgencyShipmentContainer Container3B
		{
			get
			{
				if (container3B == null)
				{
					SetupShipment3();
				}

				return container3B;
			}
		}

		void SetupShipment3()
		{
			shipment3 = Factory.New<AgencyShipment>();
			shipment3.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			container3A = Shipment3.RealContainers.AddNew();
			container3A.JC_ContainerNum = "Conatiner3A";
			container3B = Shipment3.RealContainers.AddNew();
			container3B.JC_ContainerNum = "Conatiner3B";
			Asserter.AddToScope(container3A);
			Asserter.AddToScope(container3B);
		}

		AgencyShipment shipment3;
		AgencyShipmentContainer container3A;
		AgencyShipmentContainer container3B;
		AgencyShipment Shipment4
		{
			get
			{
				if (shipment4 == null)
				{
					SetupShipment4();
				}

				return shipment4;
			}
		}

		AgencyShipmentContainer Container4A
		{
			get
			{
				if (container4A == null)
				{
					SetupShipment4();
				}

				return container4A;
			}
		}

		AgencyShipmentContainer Container4B
		{
			get
			{
				if (container4B == null)
				{
					SetupShipment4();
				}

				return container4B;
			}
		}

		void SetupShipment4()
		{
			shipment4 = Factory.New<AgencyShipment>();
			shipment4.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			container4A = Shipment4.RealContainers.AddNew();
			container4A.JC_ContainerNum = "Conatiner4A";
			container4B = Shipment4.RealContainers.AddNew();
			container4B.JC_ContainerNum = "Conatiner4B";
			Asserter.AddToScope(container4A);
			Asserter.AddToScope(container4B);
		}

		AgencyShipment shipment4;
		AgencyShipmentContainer container4A;
		AgencyShipmentContainer container4B;
		BillContainersFilterStrip FilterStrip
		{
			get
			{
				if (filterStrip == null)
				{
					filterStrip = new BillContainersFilterStrip();
				}

				return filterStrip;
			}
		}

		BillContainersFilterStrip filterStrip;
		RefContainerStock Stock1
		{
			get
			{
				if (stock1 == null)
				{
					stock1 = Factory.New<RefContainerStock>();
					stock1.R6_ContainerNum = "TEST4100013";
					stock1.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				}

				return stock1;
			}
		}

		RefContainerStock stock1;
		RefContainerStock Stock2
		{
			get
			{
				if (stock2 == null)
				{
					stock2 = Factory.New<RefContainerStock>();
					stock2.R6_ContainerNum = "TEST4100029";
					stock2.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				}

				return stock2;
			}
		}

		RefContainerStock stock2;
		RefContainerStock Stock3
		{
			get
			{
				if (stock3 == null)
				{
					stock3 = Factory.New<RefContainerStock>();
					stock3.R6_ContainerNum = "TEST4100034";
					stock3.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				}

				return stock3;
			}
		}

		RefContainerStock stock3;
		RefContainerStock Stock4
		{
			get
			{
				if (stock4 == null)
				{
					stock4 = Factory.New<RefContainerStock>();
					stock4.R6_ContainerNum = "TEST4100048";
					stock4.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				}

				return stock4;
			}
		}

		RefContainerStock stock4;
		RefContainerStock Stock5
		{
			get
			{
				if (stock5 == null)
				{
					stock5 = Factory.New<RefContainerStock>();
					stock5.R6_ContainerNum = "TEST4100050";
					stock5.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				}

				return stock5;
			}
		}

		RefContainerStock stock5;
		RefContainerStock Stock6
		{
			get
			{
				if (stock6 == null)
				{
					stock6 = Factory.New<RefContainerStock>();
					stock6.R6_ContainerNum = "TEST4100069";
					stock6.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				}

				return stock6;
			}
		}

		RefContainerStock stock6;
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new BillContainersFilterStrip();
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = new List<Tuple<string, string>>();
			result.Add(TableFilter("JobSailing", "Availability Status"));
			result.Add(TableFilter("JobVoyDestination", "Availability Status"));
			result.Add(TableFilter("JobContainerMove", "Export Detention Invoice Status"));
			result.Add(TableFilter("JobSailing", "Export Detention Invoice Status"));
			result.Add(TableFilter("JobShipment", "Export Detention Invoice Status"));
			result.Add(TableFilter("JobVoyOrigin", "Export Detention Invoice Status"));
			result.Add(TableFilter("RefContainerStock", "Export Detention Invoice Status"));
			result.Add(TableFilter("JobContainerMove", "Import Detention Invoice Status"));
			result.Add(TableFilter("JobSailing", "Import Detention Invoice Status"));
			result.Add(TableFilter("JobShipment", "Import Detention Invoice Status"));
			result.Add(TableFilter("JobVoyOrigin", "Import Detention Invoice Status"));
			result.Add(TableFilter("RefContainerStock", "Import Detention Invoice Status"));
			result.Add(TableFilter("JobContainerMove", "Returned Status"));
			result.Add(TableFilter("JobSailing", "Returned Status"));
			result.Add(TableFilter("JobShipment", "Returned Status"));
			result.Add(TableFilter("JobVoyOrigin", "Returned Status"));
			result.Add(TableFilter("RefContainerStock", "Returned Status"));
			result.Add(TableFilter("CusEntryNum", "Entry Type & Number"));
			result.Add(TableFilter("JobShipment", "Entry Type & Number"));
			result.Add(TableFilter("JobConsolTransport", "Voyage / Vessel"));
			result.Add(TableFilter("JobSailing", "Voyage / Vessel"));
			result.Add(TableFilter("JobShipment", "Voyage / Vessel"));
			result.Add(TableFilter("JobVoyage", "Voyage / Vessel"));
			result.Add(TableFilter("JobVoyDestination", "Voyage / Vessel"));
			result.Add(TableFilter("GlbBranch", "Branch Related Ports"));
			result.Add(TableFilter("GlbBranchExtraPorts", "Branch Related Ports"));
			result.Add(TableFilter("JobSailing", "Branch Related Ports"));
			result.Add(TableFilter("JobVoyDestination", "Branch Related Ports"));
			result.Add(TableFilter("JobSailing", "Load / Discharge"));
			result.Add(TableFilter("JobVoyOrigin", "Load / Discharge"));
			result.Add(TableFilter("RefCommodityCode", "Commodity Code"));
			result.Add(TableFilter("JobShipment", "Detention Invoice #"));
			result.Add(TableFilter("OrgAddress", "Empty Pickup From"));
			result.Add(TableFilter("OrgAddress", "Empty Return To"));
			result.Add(TableFilter("OrgAddress", "Consignor"));
			result.Add(TableFilter("OrgAddress", "Local Client"));
			return result;
		}
		#endregion
	}
}
