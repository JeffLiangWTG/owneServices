using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class DtbConsignmentRunSheetInstructionEventParentFinderTest : TestCaseWithFactory
	{
		#region TestGetLogParentsForEventUsingContext
		public void TestGetLogParentsForEventUsingContext_Delivery()
		{
			var nowDTO = ZDateTimeOffset.Now;
			var now = nowDTO.ToLocalZDateTime();
			var warehouseOrg = Helper.CreateOrganisation("O1");
			warehouseOrg.MainAddress.OA_City = "Orange";
			var pickupOrg = Helper.CreateOrganisation("O3");
			pickupOrg.MainAddress.OA_City = "Orange";
			var differentOrg = Helper.CreateOrganisation("O4");
			differentOrg.MainAddress.OA_City = "Sydney";
			var transportOrg = Helper.CreateOrganisation("O5");
			var consignment = Helper.CreateConsignment("1");
			var pickupAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, pickupOrg.MainAddress, ConsignmentAddressStatus.Codes.Allocated, 2, DocAddressType.LocalCartageCFS);
			var deliveryForDepot = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Multi, warehouseOrg.MainAddress, ConsignmentAddressStatus.Codes.Allocated, 3, DocAddressType.LocalCartageCFS);
			var deliveryForDifferentOrg = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Multi, differentOrg.MainAddress, ConsignmentAddressStatus.Codes.Allocated, 4, DocAddressType.LocalCartageCFS);
			var package1 = consignment.PackageJob.Packages.AddNew("PKG", "1");
			var pickupAction = Helper.CreateConsignmentAction(pickupAddress, ActionTypes.Codes.PickUp);
			var deliveryAction1 = Helper.CreateConsignmentAction(deliveryForDepot, ActionTypes.Codes.Delivery);
			var deliveryAction2 = Helper.CreateConsignmentAction(deliveryForDifferentOrg, ActionTypes.Codes.Delivery);
			var truck = Helper.CreateVehicleWithEquipmentType("V1", "XXXX");
			truck.RQ_IsVehicle = true;
			var runSheet = Helper.CreateRunSheet(transportOrg, "RS3", null, nowDTO.AddHours(10), nowDTO.AddHours(14));
			Factory.Save();
			CreateConsignmentRunSheetEquipmentItem(runSheet.PK, truck);
			var deliveryInstruction = runSheet.RunSheetInstructions.AddNew();
			deliveryInstruction.K1_Sequence = 2;
			deliveryInstruction.Actions.Add(deliveryAction1);
			var anotherInstruction = runSheet.RunSheetInstructions.AddNew();
			anotherInstruction.K1_Sequence = 1;
			anotherInstruction.Actions.Add(deliveryAction2);
			Factory.Save();
			var matchingFrieghtUnLoadedEventWithRunSheetNumber = CreateXMLEvent(AutoEvents.FreightUnloadedCode, "RS3", now.AddHours(22), CargoWise.EventReference.Constants.Facilities.Code.Depot, "Orange");
			var matchingFrieghtUnLoadedEvent = CreateXMLEvent(AutoEvents.FreightUnloadedCode, "ABCD1", now.AddHours(11), CargoWise.EventReference.Constants.Facilities.Code.Depot, "Orange");
			var unMatchingVehicleReference = CreateXMLEvent(AutoEvents.FreightUnloadedCode, "XYZ1", now.AddHours(11), CargoWise.EventReference.Constants.Facilities.Code.Depot, "Orange");
			var unMatchingRunSheet = CreateXMLEvent(AutoEvents.FreightUnloadedCode, "ABCD1", now.AddHours(9), CargoWise.EventReference.Constants.Facilities.Code.Depot, "Orange");
			var unMatchingFacility = CreateXMLEvent(AutoEvents.FreightUnloadedCode, "RS3", now.AddHours(11), CargoWise.EventReference.Constants.Facilities.Code.Consignee, "Orange");
			var unMatchingCity = CreateXMLEvent(AutoEvents.FreightUnloadedCode, "RS3", now.AddHours(11), CargoWise.EventReference.Constants.Facilities.Code.Depot, "Lithgow");
			var withFacilityAndNoCity = CreateXMLEvent(AutoEvents.FreightUnloadedCode, "RS3", now.AddHours(11), CargoWise.EventReference.Constants.Facilities.Code.Depot, "");
			var withoutFacilityAndWithCity = CreateXMLEvent(AutoEvents.FreightUnloadedCode, "RS3", now.AddHours(11), "", "Orange");
			var noFacilityAndCity = CreateXMLEvent(AutoEvents.FreightUnloadedCode, "RS3", now.AddHours(11), "", "");
			var eventFinder = new DtbConsignmentRunSheetInstructionEventParentFinder(new DtbConsignmentRunSheetInstructionDataContextManager(), new BusinessObjectFactory() { RefreshEnabled = false }, new DummyLogger());
			AssertEquals(deliveryInstruction.PK, eventFinder.GetLogParentsForEvent(matchingFrieghtUnLoadedEventWithRunSheetNumber).Single().PK);
			AssertEquals(0, eventFinder.GetLogParentsForEvent(matchingFrieghtUnLoadedEvent).Length);
			AssertEquals(0, eventFinder.GetLogParentsForEvent(unMatchingVehicleReference).Length);
			AssertEquals(0, eventFinder.GetLogParentsForEvent(unMatchingRunSheet).Length);
			AssertEquals(0, eventFinder.GetLogParentsForEvent(unMatchingFacility).Length);
			AssertEquals(0, eventFinder.GetLogParentsForEvent(unMatchingCity).Length);
			AssertEquals(anotherInstruction.PK, eventFinder.GetLogParentsForEvent(withFacilityAndNoCity).Single().PK);
			AssertEquals(deliveryInstruction.PK, eventFinder.GetLogParentsForEvent(withoutFacilityAndWithCity).Single().PK);
			AssertEquals(anotherInstruction.PK, eventFinder.GetLogParentsForEvent(noFacilityAndCity).Single().PK);
		}

		public void TestGetLogParentsForEventUsingContext_Pickup()
		{
			var nowDTO = ZDateTimeOffset.Now;
			var now = nowDTO.ToLocalZDateTime();
			var warehouseOrg = Helper.CreateOrganisation("O1");
			warehouseOrg.MainAddress.OA_City = "Sydney";
			var deliveryOrg = Helper.CreateOrganisation("O2");
			deliveryOrg.MainAddress.OA_City = "Sydney";
			var differentPickUpDepot = Helper.CreateOrganisation("O3");
			differentPickUpDepot.MainAddress.OA_City = "Orange";
			var transportOrg = Helper.CreateOrganisation("O4");
			var consignment = Helper.CreateConsignment("1");
			var pickUpDepotAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Multi, warehouseOrg.MainAddress, ConsignmentAddressStatus.Codes.Allocated, 2, DocAddressType.LocalCartageCFS);
			var pickingUpFromDifferentDepot = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Multi, differentPickUpDepot.MainAddress, ConsignmentAddressStatus.Codes.Allocated, 3, DocAddressType.LocalCartageCFS);
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, deliveryOrg.MainAddress, ConsignmentAddressStatus.Codes.Allocated, 4, DocAddressType.LocalCartageCFS);
			var package1 = consignment.PackageJob.Packages.AddNew("PKG", "1");
			var pickupAction1 = Helper.CreateConsignmentAction(pickUpDepotAddress, ActionTypes.Codes.PickUp);
			var pickupAction2 = Helper.CreateConsignmentAction(pickingUpFromDifferentDepot, ActionTypes.Codes.PickUp);
			var deliveryAction = Helper.CreateConsignmentAction(deliveryAddress, ActionTypes.Codes.Delivery);
			var truck = Helper.CreateVehicleWithEquipmentType("V1", "XXXX");
			truck.RQ_IsVehicle = true;
			var runSheet = Helper.CreateRunSheet(transportOrg, "RS3", null, nowDTO.AddHours(10), nowDTO.AddHours(14));
			Factory.Save();
			CreateConsignmentRunSheetEquipmentItem(runSheet.PK, truck);
			var anotherInstruction = runSheet.RunSheetInstructions.AddNew();
			var pickupInstruction = runSheet.RunSheetInstructions.AddNew();
			pickupInstruction.Actions.Add(pickupAction1);
			anotherInstruction.Actions.Add(pickupAction2);
			anotherInstruction.K1_Sequence = 1;
			pickupInstruction.K1_Sequence = 2;
			Factory.Save();
			var matchingFrieghtLoadedEventWithRunSheetNumber = CreateXMLEvent(AutoEvents.FreightLoadedCode, "RS3", now.AddHours(22), CargoWise.EventReference.Constants.Facilities.Code.Depot, "Sydney");
			var matchingFrieghtLoadedEvent = CreateXMLEvent(AutoEvents.FreightLoadedCode, "ABCD1", now.AddHours(11), CargoWise.EventReference.Constants.Facilities.Code.Depot, "Sydney");
			var unMatchingVehicleReference = CreateXMLEvent(AutoEvents.FreightLoadedCode, "XYZ1", now.AddHours(11), CargoWise.EventReference.Constants.Facilities.Code.Depot, "Sydney");
			var unMatchingRunSheet = CreateXMLEvent(AutoEvents.FreightLoadedCode, "ABCD1", now.AddHours(9), CargoWise.EventReference.Constants.Facilities.Code.Depot, "Sydney");
			var unMatchingFacility = CreateXMLEvent(AutoEvents.FreightLoadedCode, "RS3", now.AddHours(11), CargoWise.EventReference.Constants.Facilities.Code.Consignor, "Sydney");
			var unMatchingLocation = CreateXMLEvent(AutoEvents.FreightLoadedCode, "RS3", now.AddHours(11), CargoWise.EventReference.Constants.Facilities.Code.Depot, "Arncliffe");
			var withFacilityAndNoCity = CreateXMLEvent(AutoEvents.FreightLoadedCode, "RS3", now.AddHours(11), CargoWise.EventReference.Constants.Facilities.Code.Depot, "");
			var withoutFacilityAndWithCity = CreateXMLEvent(AutoEvents.FreightLoadedCode, "RS3", now.AddHours(11), "", "Sydney");
			var noFacilityAndCity = CreateXMLEvent(AutoEvents.FreightLoadedCode, "RS3", now.AddHours(11), "", "");
			var eventFinder = new DtbConsignmentRunSheetInstructionEventParentFinder(new DtbConsignmentRunSheetInstructionDataContextManager(), new BusinessObjectFactory()
			{ RefreshEnabled = false }, new DummyLogger());
			AssertEquals(pickupInstruction.PK, eventFinder.GetLogParentsForEvent(matchingFrieghtLoadedEventWithRunSheetNumber).Single().PK);
			AssertEquals(0, eventFinder.GetLogParentsForEvent(matchingFrieghtLoadedEvent).Length);
			AssertEquals(0, eventFinder.GetLogParentsForEvent(unMatchingVehicleReference).Length);
			AssertEquals(0, eventFinder.GetLogParentsForEvent(unMatchingRunSheet).Length);
			AssertEquals(0, eventFinder.GetLogParentsForEvent(unMatchingFacility).Length);
			AssertEquals(0, eventFinder.GetLogParentsForEvent(unMatchingLocation).Length);
			AssertEquals(anotherInstruction.PK, eventFinder.GetLogParentsForEvent(withFacilityAndNoCity).Single().PK);
			AssertEquals(pickupInstruction.PK, eventFinder.GetLogParentsForEvent(withoutFacilityAndWithCity).Single().PK);
			AssertEquals(anotherInstruction.PK, eventFinder.GetLogParentsForEvent(noFacilityAndCity).Single().PK);
		}

		#endregion
		#region CreateXMLEvent
		Event CreateXMLEvent(string eventCode, string transportReference, ZDateTime eventTime, string facility, string location)
		{
			var xmlEvent = new Event();
			xmlEvent.EventType = eventCode;
			xmlEvent.EventTime = eventTime.ToOffset();
			xmlEvent.ContextCollection = new List<Context>() { new Context() { Type = nameof(Event.ContextTypes.TransportReference), Value = transportReference } };
			xmlEvent.EventParameters = new EventParameters();
			xmlEvent.EventParameters.Facility = facility;
			xmlEvent.EventParameters.Location = location;
			return xmlEvent;
		}

		#endregion
		#region CreateConsignmentRunSheetEquipmentItem
		void CreateConsignmentRunSheetEquipmentItem(ZGuid consignmentRunSheetPK, RefEquipment equipment)
		{
			new CargoWise.Database.TestFramework.ObjectModel.DtbEquipmentItem()
			{ LTE_ParentID = consignmentRunSheetPK.ToGuid(), LTE_ParentTableCode = DtbConsignmentRunSheetSchema.Constants.Prefix, LTE_RQ_Equipment = equipment.PK.ToGuid(), LTE_RC_EquipmentType = equipment.RQ_RC_RoadContainerType.ToGuid() }.Insert(TestConnection);
		}

		#endregion
		#region Helper
		protected TransportConsignmentTestHelper Helper
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
