using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class TransitDataObjectReaderHandlerManagerTest : TransitUniversalTestCase
	{
		public void TestRegisterHandler()
		{
			SetupForBaseTests();
			AssertNull(HandlerManager.GetHandler<TransitReceiveConsolHandler>());

			var handler1 = HandlerManager.BuildHandler(TransitDataObjectReaderHandlerManager.HandlerType.Container, Data.ShipmentDataObject);
			HandlerManager.RegisterHandler(handler1);
			AssertNotNull(HandlerManager.GetHandler<TransitReceiveContainerHandler>());

			var handler2 = HandlerManager.BuildHandler(TransitDataObjectReaderHandlerManager.HandlerType.Container, Data.ShipmentDataObject);
			HandlerManager.RegisterHandler(handler2);
			AssertEquals(handler2, HandlerManager.GetHandler<TransitReceiveContainerHandler>());
		}

		public void TestWarehouse()
		{
			SetupForBaseTests();
			AssertNull(HandlerManager.GetWarehouse());

			Data.ShipmentDataObject.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C0001");
			Logger.TopLevelDataObject = Data.ShipmentDataObject;
			HandlerManager.Init(Logger, Factory, new WhsTransitReceiveConsolDataObjectReader(Data.ShipmentDataObject, Logger, Factory), Data.Warehouse);

			var warehouse = HandlerManager.GetWarehouse();
			AssertNotNull(warehouse);
		}

		void SetupForBaseTests()
		{
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { Content = CollectionContent.Complete });
		}

		TransitDataObjectReaderHandlerManager HandlerManager => handlerManager ??= new TransitDataObjectReaderHandlerManager();
		TransitDataObjectReaderHandlerManager handlerManager;
	}
}
