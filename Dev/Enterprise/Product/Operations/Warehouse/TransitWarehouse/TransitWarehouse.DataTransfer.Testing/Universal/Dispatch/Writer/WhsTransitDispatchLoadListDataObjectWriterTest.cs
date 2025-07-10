using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business.Testing;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsTransitDispatchLoadListDataObjectWriterTest : TransitUniversalTestCase
	{
		#region TestTopLevelDataContextType

		public void TestTopLevelDataContextType()
		{
			AssertEquals(DataContextType.TransitDispatchLoadList,
				((ITopLevelDataObjectWriter)new WhsTransitDispatchLoadListDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).TopLevelDataContextType);
		}

		#endregion

		#region TestEDIMessageSubType

		public void TestEDIMessageSubType()
		{
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalShipment,
				((ITopLevelDataObjectWriter)new WhsTransitDispatchLoadListDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).EDIMessageSubType);
		}

		#endregion

		#region PopulateDataObject Tests

		#region TestPopulateDataObject_AdditionalReferences

		public void TestPopulateDataObject_AdditionalReferences()
		{
			var dispatchLoadList = helper.CreateDispatchLoadList("DLL", Data.Warehouse.PK);

			var reference1 = helper.CreateAdditionalReference(dispatchLoadList, "Consol1", entryType: WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			var reference2 = helper.CreateAdditionalReference(dispatchLoadList, "MasterBill1", entryType: WarehouseAdditionalReferenceTypes.Codes.MasterBill);

			var writer = new WhsTransitDispatchLoadListDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(dispatchLoadList);

			AssertEquals(CollectionContent.Partial, dataObject.AdditionalReferenceCollection.Content);
			AssertEquals("Consol1", dataObject.AdditionalReferenceCollection.Single(a => a.Type.Code.ToString() == WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber).ReferenceNumber);
			AssertEquals("MasterBill1", dataObject.AdditionalReferenceCollection.Single(a => a.Type.Code.ToString() == WarehouseAdditionalReferenceTypes.Codes.MasterBill).ReferenceNumber);
		}

		#endregion

		#region TestPopulateDataObject_Notes

		public void TestPopulateDataObject_Notes()
		{
			var dispatchLoadList = helper.CreateDispatchLoadList("DLL", Data.Warehouse.PK);
			dispatchLoadList.Notes.AddNew(true, "Good Description1", "Good Text");
			dispatchLoadList.Notes.AddNew(true, "Good Description2", "Good Text");

			var writer = new WhsTransitDispatchLoadListDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(dispatchLoadList);

			AssertArrayEqualsByElements(
				"Should include all notes.",
				new string[] { "Good Description1", "Good Description2" },
				dataObject.NoteCollection.Select(n => n.Description.ToString()).ToArray());
		}

		#endregion

		#region TestPopulateDataObject_TransportMode

		public void TestPopulateDataObject_TransportMode_Sea() => TestPopulateDataObject_TransportMode(Core.Constants.TransportModes.Sea, "Sea Freight");

		public void TestPopulateDataObject_TransportMode_Air() => TestPopulateDataObject_TransportMode(Core.Constants.TransportModes.Air, "Air Freight");

		public void TestPopulateDataObject_TransportMode_AirSea() => TestPopulateDataObject_TransportMode(Core.Constants.TransportModes.AirSea, null);

		public void TestPopulateDataObject_TransportMode_Rail() => TestPopulateDataObject_TransportMode(Core.Constants.TransportModes.Rail, "Rail Freight");

		public void TestPopulateDataObject_TransportMode_Pedestrian() => TestPopulateDataObject_TransportMode(Core.Constants.TransportModes.Pedestrian, null);

		void TestPopulateDataObject_TransportMode(string transportMode, string outputDescription)
		{
			var dispatchLoadList = helper.CreateDispatchLoadList("DLL", Data.Warehouse.PK);
			dispatchLoadList.WDL_TransportMode = transportMode;

			var writer = new WhsTransitDispatchLoadListDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(dispatchLoadList);

			AssertEquals(transportMode, dataObject.TransportMode.Code);
			AssertEquals(outputDescription, dataObject.TransportMode.Description);

			dispatchLoadList.WDL_TransportMode = "";
			var writerForEmptyTransportMode = new WhsTransitDispatchLoadListDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObjectForEmptyTransportMode = writerForEmptyTransportMode.GetDataObject(dispatchLoadList);
			AssertNull("TransportMode property must be null.", dataObjectForEmptyTransportMode.TransportMode);
		}

		#endregion

		#endregion

		#region Implementation

		WhsTransitTestHelper helper => new WhsTransitTestHelper(Factory.BOFactory);

		#endregion
	}
}
