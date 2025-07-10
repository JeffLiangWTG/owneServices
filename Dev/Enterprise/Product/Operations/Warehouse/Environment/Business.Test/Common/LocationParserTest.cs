using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	internal class LocationParserTest : WhsTestCaseWithFactoryEnv
	{
		#region TestParse

		public void TestParse()
		{
			AssertEquals((short)2, Parser.Parse("2", false));
			AssertEquals((short)2, Parser.Parse("2", true));
			AssertEquals((short)2, Parser.Parse("B", true));
			AssertEquals((short)2, Parser.Parse("b", true));
			AssertEquals((short)22, Parser.Parse("22", false));
			AssertEquals((short)22, Parser.Parse("22", true));
			AssertEquals((short)-1, Parser.Parse("B", false));
			AssertEquals((short)-1, Parser.Parse("b", false));
			AssertEquals((short)-1, Parser.Parse("BB", true));
			AssertEquals((short)-1, Parser.Parse("321212121", false));
		}

		#endregion

		#region TestGetLocationComponent

		public void TestGetLocationComponent()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var whs = helper.CreateWarehouse("WHS");
			var rowA = helper.CreateRowAndGenerateLocations(whs, "A", 5, 2);

			var parser = new TestLocationParser();
			AssertEquals((short)2, parser.GetLocationComponent(rowA, rowA.WR_ColumnsInfo, "2", whs.WW_LocationColumnsAlpha, whs.WW_LocationColumnsZeroBased, rowA.WR_Columns, "Column"));
			AssertEquals(false, parser.IsErrorOccurred);

			AssertEquals((short)0, parser.GetLocationComponent(rowA, rowA.WR_ColumnsInfo, "0", whs.WW_LocationColumnsAlpha, whs.WW_LocationColumnsZeroBased, rowA.WR_Columns, "Column"));
			AssertEquals(true, parser.IsErrorOccurred);
			parser.IsErrorOccurred = false; // clean up

			whs.WW_LocationColumnsZeroBased = true;
			var rowB = helper.CreateRowAndGenerateLocations(whs, "B", 4, 3);
			AssertEquals((short)2, parser.GetLocationComponent(rowB, rowB.WR_ColumnsInfo, "2", whs.WW_LocationColumnsAlpha, whs.WW_LocationColumnsZeroBased, rowB.WR_Columns, "Column"));
			AssertEquals(false, parser.IsErrorOccurred);

			AssertEquals((short)4, parser.GetLocationComponent(rowB, rowB.WR_ColumnsInfo, "4", whs.WW_LocationColumnsAlpha, whs.WW_LocationColumnsZeroBased, rowB.WR_Columns, "Column"));
			AssertEquals(true, parser.IsErrorOccurred);
			parser.IsErrorOccurred = false; // clean up

			AssertEquals((short)-1, parser.GetLocationComponent(rowB, rowB.WR_ColumnsInfo, "fff", whs.WW_LocationColumnsAlpha, whs.WW_LocationColumnsZeroBased, rowB.WR_Columns, "Column"));
			AssertEquals(true, parser.IsErrorOccurred);
		}

		#endregion

		#region TestGetLocationComponentForValidationSuspended

		public void TestGetLocationComponentForValidationSuspended()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var bizObj = Factory.New<TestBusinessObject>();
			var parser = new TestLocationParser(bizObj) { IsErrorOccurred = false };

			// setup test data
			var whs = helper.CreateWarehouse("WHS");
			var rowA = helper.CreateRowAndGenerateLocations(whs, "A", 5, 2);

			parser.GetLocationComponent(rowA, rowA.WR_ColumnsInfo, "0", whs.WW_LocationColumnsAlpha, whs.WW_LocationColumnsZeroBased, rowA.WR_Columns, "Column");
			AssertEquals(true, parser.IsErrorOccurred);

			parser.IsErrorOccurred = false;
			Factory.SuspendValidation();
			parser.GetLocationComponent(rowA, rowA.WR_ColumnsInfo, "0", whs.WW_LocationColumnsAlpha, whs.WW_LocationColumnsZeroBased, rowA.WR_Columns, "Column");
			AssertEquals(false, parser.IsErrorOccurred);
		}

		#endregion

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			Parser = new LocationParser();
		}

		#endregion

		LocationParser Parser;

		class TestLocationParser : LocationParser
		{
			public TestLocationParser(IBusiness parent = null)
				: base(parent)
			{
			}

			public ZBool IsErrorOccurred { get; set; }

			protected override void GetLocationComponentError(WhsRow row, ZPropertyInfo info, ZString value, ZBool useAlpha, ZBool isZeroBased, ZShort max, string componentName)
			{
				IsErrorOccurred = true;
			}
		}

		class TestBusinessObject : DummyBusinessObject
		{
			public TestBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}
	}
}
