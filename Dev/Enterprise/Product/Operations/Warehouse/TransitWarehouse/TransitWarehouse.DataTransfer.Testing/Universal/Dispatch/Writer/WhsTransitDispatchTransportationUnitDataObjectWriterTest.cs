using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsTransitDispatchTransportationUnitDataObjectWriterTest : TransitUniversalTestCase
	{
		#region TestTopLevelDataContextType

		public void TestTopLevelDataContextType()
		{
			AssertEquals(DataContextType.TransitDispatchHeader,
				((ITopLevelDataObjectWriter)new WhsTransitDispatchTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).TopLevelDataContextType);
		}

		#endregion

		#region TestEDIMessageSubType

		public void TestEDIMessageSubType()
		{
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalShipment,
				((ITopLevelDataObjectWriter)new WhsTransitDispatchTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).EDIMessageSubType);
		}

		#endregion

		#region PopulateDataObject Tests

		#region TestBasicFieldMappings

		public void TestBasicFieldMappings()
		{
			var location = helper.CreateLocation(Data.Warehouse, code: "Dock");
			var loadList = helper.CreateDispatchLoadList("LoadList", Data.Warehouse.PK, location);
			var dispatchTransportationUnit = helper.CreateDispatchTransportationUnit("Good Vehicle", Data.Warehouse.PK);
			dispatchTransportationUnit.WDH_VehicleReference = "Good Vehicle";
			helper.CreateDispatchDLLDTUPivot(loadList.PK, dispatchTransportationUnit.PK);
			Factory.SaveForTesting();

			var writer = new WhsTransitDispatchTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(dispatchTransportationUnit);

			AssertNotNull(dataObject);
			AssertEquals("Should be the related load list's staging location", "Dock", dataObject.WarehouseLocation.ToString());
			AssertEquals("Should be the dispatch transportation unit's vehicle reference", "Good Vehicle", dataObject.VesselName.ToString());
		}

		#endregion

		#region TestPopulateDataObject_Container

		public void TestPopulateDataObject_Container()
		{
			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var dispatchTransportationUnit = helper.CreateDispatchTransportationUnitWithContainerType("DTU1", Data.Warehouse.PK, "Container1");

			helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, dispatchUnit: dispatchTransportationUnit);

			var writer = new WhsTransitDispatchTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(dispatchTransportationUnit);

			AssertEquals("DTU should not write its packages", dataObject.PackingLineCollection.Count == 0, ZBool.True);
			AssertNotNull(dataObject.ContainerCollection);
			AssertEquals("Container1", dataObject.ContainerCollection.Single().ContainerNumber);
		}

		#endregion

		#region TestPopulateDataObject_NonContainer

		public void TestPopulateDataObject_NonContainer()
		{
			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var dispatchTransportationUnit = helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK, "Truck1");

			helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, dispatchUnit: dispatchTransportationUnit);

			var writer = new WhsTransitDispatchTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(dispatchTransportationUnit);

			AssertEquals("DTU should not write its packages", dataObject.PackingLineCollection.Count == 0, ZBool.True);
			AssertNull(dataObject.ContainerCollection);
			AssertEquals("Truck1", dataObject.VesselName);
		}

		#endregion

		#region TestPopulateDataObject_Addresses

		public void TestPopulateDataObject_Addresses()
		{
			var dispatchTransportationUnit = helper.CreateDispatchTransportationUnit("Good Vehicle", Data.Warehouse.PK);

			var transportCompany = Data.Orgs.CRAHOLSYD;
			helper.CreateJobDocAddressFromAddress(dispatchTransportationUnit, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);

			var writer = new WhsTransitDispatchTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(dispatchTransportationUnit);

			AssertContainsExactElementsInAnyOrder(
				"Should include transport company and warehouse addresses.",
				new string[] { transportCompany.OH_Code, warehouse.WarehouseAddress.Header.OH_Code },
				dataObject.OrganizationAddressCollection?.Select(a => a.OrganizationCode.ToString()));
		}

		public void TestPopulateDataObject_Addresses_GivenNoJobDocAddress()
		{
			var dispatchTransportationUnit = helper.CreateDispatchTransportationUnit("Good Vehicle", Data.Warehouse.PK);

			var writer = new WhsTransitDispatchTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(dispatchTransportationUnit);

			AssertNotNull(dataObject.OrganizationAddressCollection);
			AssertContainsExactElementsInAnyOrder(
				"Should include transport company and warehouse addresses.",
				new string[] { warehouse.WarehouseAddress.Header.OH_Code },
				dataObject.OrganizationAddressCollection?.Select(a => a.OrganizationCode.ToString()));
		}

		#endregion

		#region TestPopulateDataObject_Dates

		[TestDate(2020, 08, 28, 04, 39, 50)]
		public void TestPopulateDataObject_Vehicle_Dates()
		{
			var dispatchTransportationUnit = helper.CreateDispatchTransportationUnit("Good Vehicle", Data.Warehouse.PK);
			var currentTime = ZDateTimeOffset.Now;
			dispatchTransportationUnit.WDH_LoadCompleteTime = currentTime;

			var writer = new WhsTransitDispatchTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(dispatchTransportationUnit);

			AssertNotNull(dataObject?.DateCollection);
			AssertContainsExactElementsInAnyOrder("Should be the dispatch transportation unit's load time", new[] { DateType.Pack }, dataObject.DateCollection.Select(d => d.Type));
			var packDate = dataObject.DateCollection.Single();
			AssertEquals(currentTime.ToZDateTime(), packDate.Value);
			AssertEquals(ZBool.False, packDate.IsEstimate);
		}

		[TestDate(2020, 08, 28, 04, 39, 50)]
		public void TestPopulateDataObject_Container_Dates()
		{
			var dispatchTransportationUnit = helper.CreateDispatchTransportationUnitWithContainerType("Good Vehicle", Data.Warehouse.PK);
			var currentTime = ZDateTimeOffset.Now;
			dispatchTransportationUnit.WDH_LoadCompleteTime = currentTime;

			var writer = new WhsTransitDispatchTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(dispatchTransportationUnit);

			AssertNotNull(dataObject?.DateCollection);
			AssertContainsExactElementsInAnyOrder("Should be the dispatch transportation unit's load time", new[] { DateType.Pack }, dataObject.DateCollection.Select(d => d.Type));
			AssertNotNull(dataObject.ContainerCollection);
			AssertContainsExactElementsInAnyOrder("Should be the dispatch transportation unit's load time", new[] { currentTime.ToZDateTime() }, dataObject.ContainerCollection.Select(c => c.PackDate.Value));
		}

		public void TestPopulateDataObject_InvalidDate()
		{
			var dispatchTransportationUnit = helper.CreateDispatchTransportationUnitWithContainerType("Good Vehicle", Data.Warehouse.PK);
			dispatchTransportationUnit.WDH_LoadCompleteTime = ZDateTimeOffset.Invalid;

			var writer = new WhsTransitDispatchTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(dispatchTransportationUnit);

			AssertNull(dataObject.DateCollection);
		}

		public void TestPopulateDataObject_NoDates()
		{
			var dispatchTransportationUnit = helper.CreateDispatchTransportationUnitWithContainerType("Good Vehicle", Data.Warehouse.PK);
			var writer = new WhsTransitDispatchTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(dispatchTransportationUnit);

			AssertNull(dataObject.DateCollection);
		}

		#endregion

		#region TestPopulateDataObject_AdditionalReferences

		public void TestPopulateDataObject_AdditionalReferences()
		{
			var dispatchTransportationUnit = helper.CreateDispatchTransportationUnit("Good Vehicle", Data.Warehouse.PK);

			var reference1 = helper.CreateAdditionalReference(dispatchTransportationUnit, "Good Name", entryType: WarehouseAdditionalReferenceTypes.Codes.DriverName);
			var reference2 = helper.CreateAdditionalReference(dispatchTransportationUnit, "Good License", entryType: WarehouseAdditionalReferenceTypes.Codes.DriverLicense);

			var writer = new WhsTransitDispatchTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(dispatchTransportationUnit);

			AssertEquals(CollectionContent.Partial, dataObject.AdditionalReferenceCollection.Content);
			AssertContainsExactElementsInAnyOrder(
				"Should include all additional references.",
				new string[] { "Good Name", "Good License" },
				dataObject.AdditionalReferenceCollection?.Select(r => r.ReferenceNumber.ToString()));
		}

		#endregion

		#region TestPopulateDataObject_Notes

		public void TestPopulateDataObject_Notes()
		{
			var dispatchTransportationUnit = helper.CreateDispatchTransportationUnit("Good Vehicle", Data.Warehouse.PK);
			dispatchTransportationUnit.Notes.AddNew(true, "Good Description1", "Good Text");
			dispatchTransportationUnit.Notes.AddNew(true, "Good Description3", "Good Text");
			dispatchTransportationUnit.Notes.AddNew(true, "Good Description2", "Good Text");

			var writer = new WhsTransitDispatchTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(dispatchTransportationUnit);

			AssertArrayEqualsByElements(
				"Should include all additional references.",
				new string[] { "Good Description1", "Good Description2", "Good Description3" },
				dataObject.NoteCollection.Select(n => n.Description.ToString()).ToArray());
		}

		#endregion

		#region TestPopulateDataObject_Container_MultipleLoadLists

		public void TestPopulateDataObject_ContainerLinkedToMultipleLoadLists_DifferentLocations_ShouldGetTheFirstLocationBasedOnAlphabeticalOrder()
		{
			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var loadList1 = helper.CreateDispatchLoadList("DLL1", Data.Warehouse.PK, stagingLocation: helper.CreateLocation(Data.Warehouse, code: "Dock-2"));
			var loadList2 = helper.CreateDispatchLoadList("DLL2", Data.Warehouse.PK, stagingLocation: helper.CreateLocation(Data.Warehouse, code: "Dock-1"));
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			var receiveTransportationUnit = helper.CreateReceiveTransportationUnit("RTU1", Data.Warehouse.PK, Data.Warehouse.DefaultInboundDockDoorLocation.PK);
			var dispatchTransportationUnit = helper.CreateDispatchTransportationUnitWithContainerType("DTU1", Data.Warehouse.PK, "Container1");

			var packageState = helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, dispatchLoadList: loadList1, dispatchConsignment: dispatchConsignment, receiveUnit: receiveTransportationUnit, dispatchUnit: dispatchTransportationUnit);
			packageState.WPS_WL_LastLocation = Data.Warehouse.DefaultOutboundDockDoorLocation.PK;
			helper.CreateDispatchDLLDTUPivot(loadList1.PK, dispatchTransportationUnit.PK);
			helper.CreateDispatchDLLDTUPivot(loadList2.PK, dispatchTransportationUnit.PK);

			Factory.SaveForTesting();

			AssertNoExceptionThrown("Should not throw exception", () =>
			{
				var writer = new WhsTransitDispatchTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
				var dataObject = writer.GetDataObject(dispatchTransportationUnit);
				AssertNotNull(dataObject);
				AssertEquals("Should be load list DLL1's staging location (by alphabetical order)", "Dock-1", dataObject.WarehouseLocation.ToString());
			});
		}

		public void TestPopulateDataObject_ContainerLinkedToMultipleLoadLists_NoLocations_NotSetWarehouseLocation()
		{
			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var loadList1 = helper.CreateDispatchLoadList("DLL1", Data.Warehouse.PK);
			var loadList2 = helper.CreateDispatchLoadList("DLL2", Data.Warehouse.PK);
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			var receiveTransportationUnit = helper.CreateReceiveTransportationUnit("RTU1", Data.Warehouse.PK, Data.Warehouse.DefaultInboundDockDoorLocation.PK);
			var dispatchTransportationUnit = helper.CreateDispatchTransportationUnitWithContainerType("DTU1", Data.Warehouse.PK, "Container1");

			var packageState = helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, dispatchLoadList: loadList1, dispatchConsignment: dispatchConsignment, receiveUnit: receiveTransportationUnit, dispatchUnit: dispatchTransportationUnit);
			packageState.WPS_WL_LastLocation = Data.Warehouse.DefaultOutboundDockDoorLocation.PK;
			helper.CreateDispatchDLLDTUPivot(loadList1.PK, dispatchTransportationUnit.PK);
			helper.CreateDispatchDLLDTUPivot(loadList2.PK, dispatchTransportationUnit.PK);

			Factory.SaveForTesting();

			AssertNoExceptionThrown("Should not throw exception", () =>
			{
				var writer = new WhsTransitDispatchTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
				var dataObject = writer.GetDataObject(dispatchTransportationUnit);
				AssertNotNull(dataObject);
				AssertEquals("Should be empty as non of the DTU's load lists have a staging location", string.Empty, dataObject.WarehouseLocation.ToString());
			});
		}

		#endregion

		#region TestPopulateDataObject_EmptyPackingLineCollection

		public void TestPopulateDataObject_EmptyPackingLineCollection()
		{
			var dtu = helper.CreateDispatchTransportationUnitWithContainerType("DTU1", Data.Warehouse.PK, "Container1");
			var writer = new WhsTransitDispatchTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(dtu);

			AssertNotNull(dataObject.PackingLineCollection);
			AssertEquals("Content should be completed", CollectionContent.Complete, dataObject.PackingLineCollection.Content);
		}

		#endregion

		#endregion

		#region Implementation

		WhsTransitTestHelper helper => new WhsTransitTestHelper(Factory.BOFactory);
		WhsWarehouse warehouse => Data.Warehouse;

		#endregion
	}
}
