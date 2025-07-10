using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class DtbConsignmentActionParentEventFinderTest : TestCaseWithFactory
	{
		#region TestGetLogParentsForEventUsingContext_InValidEventCode
		public void TestGetLogParentsForEventUsingContext_InValidEventCode()
		{
			AssertGetLogParentsForEventUsingContext_InValidEventCode(ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery);
			AssertGetLogParentsForEventUsingContext_InValidEventCode(ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
		}

		void AssertGetLogParentsForEventUsingContext_InValidEventCode(string addressType, string actionType)
		{
			var nowDTO = ZDateTimeOffset.Now;
			var now = nowDTO.ToLocalZDateTime();
			var depot1 = Helper.CreateOrganisation("D1");
			depot1.MainAddress.OA_City = "Sydney";
			var consignment = Helper.CreateConsignment();
			consignment.PackageJob.Packages.AddNew("PLT", "P1");
			var address = Helper.CreateConsignmentAddress(consignment, addressType, depot1.MainAddress);
			var action = Helper.CreateConsignmentAction(address, actionType);
			var universalEvent = CreateXMLEvent(AutoEvents.FreightLoadedCode, now.AddHours(11), "P1", "Sydney", CargoWise.EventReference.Constants.Facilities.Code.Depot);
			var runSheet = Helper.CreateRunSheet(null, "A", null, nowDTO.AddHours(-1));
			var instruction = Helper.CreateRunSheetInstruction(runSheet, action);
			var eventFinder = new DtbConsignmentActionParentEventFinder(new DtbConsignmentActionDataContextManager(), new BusinessObjectFactory()
			{ RefreshEnabled = false }, new DummyLogger());
			AssertEquals(0, eventFinder.GetLogParentsForEvent(universalEvent).Length);
		}

		#endregion
		#region TestGetLogParentForEventUsingContext_Actions
		public void TestGetLogParentForEventUsingContext_Arrival_Actions()
		{
			AssertGetLogParentsForEventUsingContext_Actions(AutoEvents.ArrivalCode, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery);
		}

		public void TestGetLogParentForEventUsingContext_Delivery_Actions()
		{
			AssertGetLogParentsForEventUsingContext_Actions(AutoEvents.DepartureCode, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
		}

		void AssertGetLogParentsForEventUsingContext_Actions(string eventCode, string addressType, string actionType)
		{
			var nowDTO = ZDateTimeOffset.Now;
			var now = nowDTO.ToLocalZDateTime();
			var depot1 = Helper.CreateOrganisation("D1");
			depot1.MainAddress.OA_City = "Sydney";
			var consignment1 = Helper.CreateConsignment("C1");
			var consignment2 = Helper.CreateConsignment("C2");
			var package1InConsignment1 = helper.CreatePackage(consignment1.PackageJob, "PLT", "P1");
			var package2InConsignment2 = helper.CreatePackage(consignment2.PackageJob, "PLT", "P2");
			var addressInConsignment1 = Helper.CreateConsignmentAddress(consignment1, addressType, depot1.MainAddress);
			var addressInConsignment2 = Helper.CreateConsignmentAddress(consignment2, addressType, depot1.MainAddress);
			var action1 = Helper.CreateConsignmentAction(addressInConsignment1, actionType);
			var action2 = Helper.CreateConsignmentAction(addressInConsignment2, actionType == ConsignmentAddressTypes.Codes.PickUp ? ConsignmentAddressTypes.Codes.Delivery : ConsignmentAddressTypes.Codes.PickUp);
			var action3 = Helper.CreateConsignmentAction(addressInConsignment1, actionType == ConsignmentAddressTypes.Codes.PickUp ? ConsignmentAddressTypes.Codes.Delivery : ConsignmentAddressTypes.Codes.PickUp);
			var action4 = Helper.CreateConsignmentAction(addressInConsignment1, actionType);
			var action5 = Helper.CreateConsignmentAction(addressInConsignment1, actionType);
			Helper.CreatePackageDivot(action1, package1InConsignment1);
			Helper.CreatePackageDivot(action2, package2InConsignment2);
			Helper.CreatePackageDivot(action3, package1InConsignment1);
			Helper.CreatePackageDivot(action4, package1InConsignment1);
			Helper.CreatePackageDivot(action5, package1InConsignment1);
			var latestRunSheet = Helper.CreateRunSheet(null, "A", null, nowDTO.AddHours(-2));
			var instruction1InLatestRunSheet = Helper.CreateRunSheetInstruction(latestRunSheet, action1, action2);
			var instruction2InLatestRunSheet = Helper.CreateRunSheetInstruction(latestRunSheet, action3);
			var runSheetStartedFirst = Helper.CreateRunSheet(null, "B", null, nowDTO.AddHours(-3));
			var instruction1InRunSheetStartedFirst = Helper.CreateRunSheetInstruction(runSheetStartedFirst, action4);
			var runSheetStartsInFuture = Helper.CreateRunSheet(null, "C", null, nowDTO.AddHours(8));
			var instruction1InFutureRunSheet = Helper.CreateRunSheetInstruction(runSheetStartsInFuture, action5);
			Factory.Save();
			var eventFinder = new DtbConsignmentActionParentEventFinder(new DtbConsignmentActionDataContextManager(), new BusinessObjectFactory()
			{ RefreshEnabled = false }, new DummyLogger());
			var matchingEvent = CreateXMLEvent(eventCode, now.AddHours(11), "P1", "Sydney", CargoWise.EventReference.Constants.Facilities.Code.Depot);
			var unMatchingEvent = CreateXMLEvent(eventCode, now.AddHours(11), "P2", "Sydney", CargoWise.EventReference.Constants.Facilities.Code.Depot);
			AssertContainsExactElementsInAnyOrder(new[] { action1.PK, package1InConsignment1.PK }, eventFinder.GetLogParentsForEvent(matchingEvent).Select(b => b.PK));
			AssertEquals(0, eventFinder.GetLogParentsForEvent(unMatchingEvent).Length);
		}

		#endregion
		#region TestGetLogParentForEventUsingContext_PackageHasMultipleDepots
		public void TestGetLogParentForEventUsingContext_PackageHasMultipleDepots()
		{
			var nowDTO = ZDateTimeOffset.Now;
			var now = nowDTO.ToLocalZDateTime();
			var depot1 = Helper.CreateOrganisation("D1");
			var depot2 = Helper.CreateOrganisation("D2");
			depot1.MainAddress.OA_City = "Sydney";
			depot2.MainAddress.OA_City = "Melbourne";
			var consignment1 = Helper.CreateConsignment("C1");
			var package = helper.CreatePackage(consignment1.PackageJob, "PLT", "P1");
			var pickupAddress = Helper.CreateConsignmentAddress(consignment1, ConsignmentAddressTypes.Codes.PickUp);
			var depot1Address = Helper.CreateConsignmentAddress(consignment1, ConsignmentAddressTypes.Codes.Multi, depot1.MainAddress, ConsignmentAddressStatus.Codes.Allocated, 2);
			var depot2Address = Helper.CreateConsignmentAddress(consignment1, ConsignmentAddressTypes.Codes.Multi, depot2.MainAddress, ConsignmentAddressStatus.Codes.Allocated, 3);
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment1, ConsignmentAddressTypes.Codes.Delivery);
			var pickup = Helper.CreateConsignmentAction(pickupAddress, ActionTypes.Codes.PickUp);
			var deliveryToDepot1 = Helper.CreateConsignmentAction(depot1Address, ActionTypes.Codes.Delivery);
			var pickupFromDepot1 = Helper.CreateConsignmentAction(depot1Address, ActionTypes.Codes.PickUp);
			var deliveryToDepot2 = Helper.CreateConsignmentAction(depot2Address, ActionTypes.Codes.Delivery);
			var pickupFromDepot2 = Helper.CreateConsignmentAction(depot2Address, ActionTypes.Codes.PickUp);
			var delivery = Helper.CreateConsignmentAction(deliveryAddress, ActionTypes.Codes.Delivery);
			Helper.CreatePackageDivot(pickup, package);
			Helper.CreatePackageDivot(deliveryToDepot1, package);
			Helper.CreatePackageDivot(pickupFromDepot1, package);
			Helper.CreatePackageDivot(deliveryToDepot2, package);
			Helper.CreatePackageDivot(pickupFromDepot2, package);
			Helper.CreatePackageDivot(delivery, package);
			var runSheetPickupAndDeliverToDepot1 = Helper.CreateRunSheet(null, "A", null, nowDTO.AddHours(-2));
			Helper.CreateRunSheetInstruction(runSheetPickupAndDeliverToDepot1, pickup);
			Helper.CreateRunSheetInstruction(runSheetPickupAndDeliverToDepot1, deliveryToDepot1);
			var runSheetPickupDepot1AndDeliverToDepot2 = Helper.CreateRunSheet(null, "B", null, nowDTO.AddHours(-1));
			Helper.CreateRunSheetInstruction(runSheetPickupDepot1AndDeliverToDepot2, pickupFromDepot1);
			Helper.CreateRunSheetInstruction(runSheetPickupDepot1AndDeliverToDepot2, deliveryToDepot2);
			var runSheetPickupFromDepot2AndDeliver = Helper.CreateRunSheet(null, "c", null, nowDTO);
			Helper.CreateRunSheetInstruction(runSheetPickupFromDepot2AndDeliver, pickupFromDepot2);
			Helper.CreateRunSheetInstruction(runSheetPickupFromDepot2AndDeliver, delivery);
			Factory.Save();
			var eventFinder = new DtbConsignmentActionParentEventFinder(new DtbConsignmentActionDataContextManager(), new BusinessObjectFactory()
			{ RefreshEnabled = false }, new DummyLogger());
			var matchingArrivalEventForDepot1 = CreateXMLEvent(AutoEvents.ArrivalCode, now.AddHours(11), "P1", "Sydney", CargoWise.EventReference.Constants.Facilities.Code.Depot);
			var unMatchingArrivalEventForDepot1 = CreateXMLEvent(AutoEvents.ArrivalCode, now.AddHours(11), "P1", "Sydney", CargoWise.EventReference.Constants.Facilities.Code.Warehouse);
			var matchingArrivalEventForDepot2 = CreateXMLEvent(AutoEvents.ArrivalCode, now.AddHours(11), "P1", "Melbourne", CargoWise.EventReference.Constants.Facilities.Code.Depot);
			var unMatchingArrivalEventForDepot2 = CreateXMLEvent(AutoEvents.ArrivalCode, now.AddHours(11), "P1", "Melbourne", CargoWise.EventReference.Constants.Facilities.Code.Warehouse);
			AssertContainsExactElementsInAnyOrder(new[] { deliveryToDepot1.PK, package.PK }, eventFinder.GetLogParentsForEvent(matchingArrivalEventForDepot1).Select(b => b.PK));
			AssertEquals(0, eventFinder.GetLogParentsForEvent(unMatchingArrivalEventForDepot1).Length);
			AssertContainsExactElementsInAnyOrder(new[] { deliveryToDepot2.PK, package.PK }, eventFinder.GetLogParentsForEvent(matchingArrivalEventForDepot2).Select(b => b.PK));
			AssertEquals(0, eventFinder.GetLogParentsForEvent(unMatchingArrivalEventForDepot2).Length);
			var matchingDepartureEventForDepot1 = CreateXMLEvent(AutoEvents.DepartureCode, now.AddHours(11), "P1", "Sydney", CargoWise.EventReference.Constants.Facilities.Code.Depot);
			var unMatchingDepartureEventForDepot1 = CreateXMLEvent(AutoEvents.DepartureCode, now.AddHours(11), "P1", "Sydney", CargoWise.EventReference.Constants.Facilities.Code.Warehouse);
			var matchingDepartureEventForDepot2 = CreateXMLEvent(AutoEvents.DepartureCode, now.AddHours(11), "P1", "Melbourne", CargoWise.EventReference.Constants.Facilities.Code.Depot);
			var unMatchingDepartureEventForDepot2 = CreateXMLEvent(AutoEvents.DepartureCode, now.AddHours(11), "P1", "Melbourne", CargoWise.EventReference.Constants.Facilities.Code.Warehouse);
			AssertContainsExactElementsInAnyOrder(new[] { pickupFromDepot1.PK, package.PK }, eventFinder.GetLogParentsForEvent(matchingDepartureEventForDepot1).Select(b => b.PK));
			AssertEquals(0, eventFinder.GetLogParentsForEvent(unMatchingDepartureEventForDepot1).Length);
			AssertContainsExactElementsInAnyOrder(new[] { pickupFromDepot2.PK, package.PK }, eventFinder.GetLogParentsForEvent(matchingDepartureEventForDepot2).Select(b => b.PK));
			AssertEquals(0, eventFinder.GetLogParentsForEvent(unMatchingDepartureEventForDepot2).Length);
		}

		#endregion
		#region TestGetLogParentForEventUsingContext_Packages
		public void TestGetLogParentForEventUsingContext_Arrival_Packages()
		{
			AssertGetLogParentsForEventUsingContext_Packages(AutoEvents.ArrivalCode, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery);
		}

		public void TestGetLogParentForEventUsingContext_Delivery_Packages()
		{
			AssertGetLogParentsForEventUsingContext_Packages(AutoEvents.DepartureCode, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
		}

		void AssertGetLogParentsForEventUsingContext_Packages(string eventCode, string addressType, string actionType)
		{
			var now = ZDateTime.Now;
			var depot1 = Helper.CreateOrganisation("D1");
			depot1.MainAddress.OA_City = "Sydney";
			var consignment1 = Helper.CreateConsignment("C1");
			var consignment2 = Helper.CreateConsignment("C2");
			var package1InConsignment1 = helper.CreatePackage(consignment1.PackageJob, "PLT", "C1P1");
			var package2InConsignment1 = helper.CreatePackage(consignment1.PackageJob, "PLT", "C1P2");
			var package1InConsignment2 = helper.CreatePackage(consignment2.PackageJob, "PLT", "C2P1");
			var addressInConsignment1 = Helper.CreateConsignmentAddress(consignment1, addressType, depot1.MainAddress);
			var addressInConsignment2 = Helper.CreateConsignmentAddress(consignment2, addressType, depot1.MainAddress);
			var action1 = Helper.CreateConsignmentAction(addressInConsignment1, actionType);
			var action2 = Helper.CreateConsignmentAction(addressInConsignment1, actionType == ConsignmentAddressTypes.Codes.PickUp ? ConsignmentAddressTypes.Codes.Delivery : ConsignmentAddressTypes.Codes.PickUp);
			var action3 = Helper.CreateConsignmentAction(addressInConsignment2, actionType == ConsignmentAddressTypes.Codes.PickUp ? ConsignmentAddressTypes.Codes.Delivery : ConsignmentAddressTypes.Codes.PickUp);
			Helper.CreatePackageDivot(action1, package1InConsignment1);
			Helper.CreatePackageDivot(action1, package2InConsignment1);
			Helper.CreatePackageDivot(action2, package1InConsignment1);
			Helper.CreatePackageDivot(action2, package2InConsignment1);
			Helper.CreatePackageDivot(action3, package1InConsignment2);
			var runSheetA = Helper.CreateRunSheet(null, "A");
			var instruction1InRunSheetA = Helper.CreateRunSheetInstruction(runSheetA, action1, action2);
			var instruction2InRunSheetA = Helper.CreateRunSheetInstruction(runSheetA, action3);
			Factory.Save();
			var eventFinder = new DtbConsignmentActionParentEventFinder(new DtbConsignmentActionDataContextManager(), new BusinessObjectFactory()
			{ RefreshEnabled = false }, new DummyLogger());
			var matchingEvent = CreateXMLEvent(eventCode, now.AddHours(11), "C1P1", "Sydney", CargoWise.EventReference.Constants.Facilities.Code.Depot);
			var packageIdForDifferentActionType = CreateXMLEvent(eventCode, now.AddHours(11), "C2P1", "Sydney", CargoWise.EventReference.Constants.Facilities.Code.Depot);
			var unMatchingNonExistingPackageId = CreateXMLEvent(eventCode, now.AddHours(11), "XXX", "Sydney", CargoWise.EventReference.Constants.Facilities.Code.Depot);
			AssertContainsExactElementsInAnyOrder(new[] { action1.PK, package1InConsignment1.PK }, eventFinder.GetLogParentsForEvent(matchingEvent).Select(b => b.PK));
			AssertEquals(0, eventFinder.GetLogParentsForEvent(packageIdForDifferentActionType).Length);
			AssertEquals(0, eventFinder.GetLogParentsForEvent(unMatchingNonExistingPackageId).Length);
		}

		#endregion
		#region CreateXMLEvent
		Event CreateXMLEvent(string eventCode, ZDateTime eventTime, string packageId, string city, string facility)
		{
			var xmlEvent = new Event();
			xmlEvent.EventType = eventCode;
			xmlEvent.EventTime = eventTime.ToOffset();
			xmlEvent.ContextCollection = new List<Context>() { new Context() { Type = nameof(Event.ContextTypes.TransportBookingPackageID), Value = packageId } };
			xmlEvent.EventParameters = new EventParameters();
			xmlEvent.EventParameters.Location = city;
			xmlEvent.EventParameters.Facility = facility;
			return xmlEvent;
		}

		#endregion
		#region Helpers
		TransportConsignmentTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new TransportConsignmentTestHelper(Factory));
			}
		}

		TransportConsignmentTestHelper helper;
		#endregion
	}
}
