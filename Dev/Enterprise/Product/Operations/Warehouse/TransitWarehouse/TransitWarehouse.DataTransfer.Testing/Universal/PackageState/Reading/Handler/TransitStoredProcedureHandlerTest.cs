namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class TransitStoredProcedureHandlerTest : TransitUniversalTestCase
	{
		public void TestInitializeAndExecuteWhenLoggerTopLevelDataContextIsNull()
		{
			Data.SetupForForwardingImport();
			var dataObject = Data.ShipmentDataObject;
			var handler = new TransitStoredProcedureHandler();
			Logger.TopLevelDataObject = null;
			AssertEquals(null, Logger.TopLevelDataContext);

			AssertNoExceptionThrown(() => handler.Initialize(dataObject, Logger, null));
			AssertNoExceptionThrown(() => handler.Execute(Factory, dataObject, Logger));
		}

		public void TestInitializeAndExecuteWhenDataTargetCollectionIsNull()
		{
			Data.SetupForForwardingImport();
			var dataObject = Data.ShipmentDataObject;
			var handler = new TransitStoredProcedureHandler();

			dataObject.DataContext.ClearDataSourceCollection();
			AssertEquals(null, dataObject.DataContext.DataTargetCollection);

			AssertNoExceptionThrown(() => handler.Initialize(dataObject, Logger, null));
			AssertNoExceptionThrown(() => handler.Execute(Factory, dataObject, Logger));
		}
	}
}
