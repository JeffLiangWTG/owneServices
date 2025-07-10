namespace Enterprise.Warehouse.Environment.Business.Testing
{
	internal class LocationParserWithInfoErrorTestCase : WhsTestCaseWithFactoryEnv
	{
		//		Cant run this test as testlistener will generate and error
		//
		//		public void TestGetLocationComponent()
		//		{
		//			WhsTestHelperFunctionsEnvUS Helper = new WhsTestHelperFunctionsEnvUS(Factory);
		//			WhsWarehouse Whs = Helper.CreateSaveableWarehouse("WHS");
		//			WhsRow RowA = Helper.CreateSaveableRowAndGenerateLocations(Whs, "A", 5, 2);
		//
		//			ZShort Result;
		//			LocationParserWithInfoError Parser = new LocationParserWithInfoError();
		//			Result = Parser.GetLocationComponent(RowA, RowA.WR_ColumnsInfo, "1", Whs.WW_LocationColumnsAlpha, RowA.WR_Columns, "Column");
		//			AssertEquals((short)1, Result);
		//			AssertEquals("Should be no error", false, RowA.WR_ColumnsInfo.HasErrors());
		//
		//			Result = Parser.GetLocationComponent(RowA, RowA.WR_ColumnsInfo, "0", Whs.WW_LocationColumnsAlpha, RowA.WR_Columns, "Column");
		//			AssertEquals((short)0, Result);
		//			AssertHasError(RowA.WR_ColumnsInfo, RowA.RangeMessage("Column", RowA.WR_Columns));
		//		}
	}
}
