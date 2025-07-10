using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public abstract class LocationValidationBaseTest : WhsTestCaseWithFactoryEnv
	{
		protected abstract LocationCrashTestDummy LocationCrashTestDummy { get; }

		#region TestErrorIfLocationStringInvalid

		public void TestErrorIfLocationStringInvalid()
		{
			var whs = Helper.CreateWarehouse("1");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 4, 3, 2);
			var dummy = LocationCrashTestDummy;
			Factory.Save();

			// no warehouse
			dummy.LocationString = "SHEEP1";
			AssertHasError(dummy.LocationStringInfo, "A valid warehouse has not been selected for this Object.");
			dummy.LocationWhsGuid = whs.PK;

			// empty LocationString
			Assert("LocationTitle should be Empty", dummy.LocationTitle.IsEmpty);
			dummy.LocationString = ZString.Empty;
			AssertHasError(dummy.LocationStringInfo, "Please enter a valid Location.");

			// Empty LocationString with set LocationTitle
			dummy.LocationTitle = "Destination Location";
			dummy.LocationString = "SHEEP1";
			dummy.LocationString = ZString.Empty;
			AssertHasError(dummy.LocationStringInfo, "No Destination Location has been entered. Please enter a valid Location.");

			// row validation
			dummy.LocationTitle = ZString.Empty;
			dummy.LocationString = "SHEEP2";
			AssertHasError(dummy.LocationStringInfo, "Please enter a valid Location Row.");

			dummy.LocationTitle = "Destination Location";
			dummy.LocationString = "SHEEP3";
			AssertHasError(dummy.LocationStringInfo, "Please enter a valid Location Row for Destination Location.");

			dummy.LocationString = "A";
			AssertEquals("Valid row has caused error.", false, dummy.LocationStringInfo.HasErrors());
			AssertEquals("Guid should be set", row.Locations[0].PK, dummy.WLV_FK);
			dummy.LocationString = ZString.Empty;
			AssertEquals("Guid should be empty", ZGuid.Empty, dummy.WLV_FK);

			// column validation - Numeric
			dummy.LocationString = "A-4";
			AssertEquals("Valid column has caused error.", false, dummy.LocationStringInfo.HasErrors());
			AssertEquals("Guid should be set", row.Locations[18].PK, dummy.WLV_FK);
			dummy.LocationString = "A-5";
			AssertHasError(dummy.LocationStringInfo, row.RangeMessage("Column", row.WR_Columns, whs.WW_LocationColumnsZeroBased));

			// column validation - Alpha
			whs.WW_LocationColumnsAlpha = true;
			Factory.Save();

			dummy.LocationString = "A-D";
			AssertEquals("Valid column has caused error.", false, dummy.LocationStringInfo.HasErrors());
			dummy.LocationString = "A-E";
			AssertHasError(dummy.LocationStringInfo, row.RangeMessage("Column", row.WR_Columns, whs.WW_LocationColumnsZeroBased));
			dummy.LocationString = "A-d";
			AssertEquals("Valid column has caused error.", false, dummy.LocationStringInfo.HasErrors());
			dummy.LocationString = "A-e";
			AssertHasError(dummy.LocationStringInfo, row.RangeMessage("Column", row.WR_Columns, whs.WW_LocationColumnsZeroBased));

			// level validation - Numeric
			dummy.LocationString = "A-A-3";
			AssertEquals("Valid level has caused error.", false, dummy.LocationStringInfo.HasErrors());
			dummy.LocationString = "A-A-4";
			AssertHasError(dummy.LocationStringInfo, row.RangeMessage("Level", row.WR_Levels, whs.WW_LocationLevelsZeroBased));

			// level validation - Alpha
			whs.WW_LocationLevelsAlpha = true;
			Factory.Save();

			dummy.LocationString = "A-A-C";
			AssertEquals("Valid level has caused error.", false, dummy.LocationStringInfo.HasErrors());
			dummy.LocationString = "A-A-D";
			AssertHasError(dummy.LocationStringInfo, row.RangeMessage("Level", row.WR_Levels, whs.WW_LocationLevelsZeroBased));
			dummy.LocationString = "A-A-c";
			AssertEquals("Valid level has caused error.", false, dummy.LocationStringInfo.HasErrors());
			dummy.LocationString = "A-A-d";
			AssertHasError(dummy.LocationStringInfo, row.RangeMessage("Level", row.WR_Levels, whs.WW_LocationLevelsZeroBased));

			// tray validation - Numeric
			dummy.LocationString = "A-A-A-2";
			AssertEquals("Valid tray has caused error.", false, dummy.LocationStringInfo.HasErrors());
			dummy.LocationString = "A-A-A-3";
			AssertHasError(dummy.LocationStringInfo, row.RangeMessage("Tray", row.WR_Trays, whs.WW_LocationTraysZeroBased));

			// tray validation - Alpha
			whs.WW_LocationTraysAlpha = true;
			Factory.Save();

			dummy.LocationString = "A-A-A-B";
			AssertEquals("Valid tray has caused error.", false, dummy.LocationStringInfo.HasErrors());
			dummy.LocationString = "A-A-A-C";
			AssertHasError(dummy.LocationStringInfo, row.RangeMessage("Tray", row.WR_Trays, whs.WW_LocationTraysZeroBased));
			dummy.LocationString = "A-A-A-b";
			AssertEquals("Valid tray has caused error.", false, dummy.LocationStringInfo.HasErrors());
			dummy.LocationString = "A-A-A-c";
			AssertHasError(dummy.LocationStringInfo, row.RangeMessage("Tray", row.WR_Trays, whs.WW_LocationTraysZeroBased));

			// delimiters
			dummy.LocationString = "A D C B";
			AssertEquals(dummy.LocationString + " has not caused error.", true, dummy.LocationStringInfo.HasErrors());
			dummy.LocationString = "A.D.C.B";
			AssertEquals(dummy.LocationString + " has not caused error.", true, dummy.LocationStringInfo.HasErrors());
			dummy.LocationString = "A,D,C,B";
			AssertEquals(dummy.LocationString + " has not caused error.", true, dummy.LocationStringInfo.HasErrors());
			whs.WW_LocationComponentDelimiter = "*";
			Factory.Save();

			dummy.LocationString = "A-D-C-B";
			AssertEquals(dummy.LocationString + " has not caused error.", true, dummy.LocationStringInfo.HasErrors());
			dummy.LocationString = "A*D*C*B";
			AssertEquals(dummy.LocationString + " has caused error.", false, dummy.LocationStringInfo.HasErrors());
			dummy.LocationString = "A!D!C!B";
			AssertEquals(dummy.LocationString + " has not caused error.", true, dummy.LocationStringInfo.HasErrors());
		}

		#endregion

		#region TestErrorIfLocationStringWithZeroBaseInvalid

		public void TestErrorIfLocationStringWithZeroBaseInvalid()
		{
			var whs = Helper.CreateWarehouse("1");
			whs.WW_LocationColumnsZeroBased = true;
			whs.WW_LocationLevelsZeroBased = true;
			whs.WW_LocationTraysZeroBased = true;

			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 4, 3, 2);
			var dummy = LocationCrashTestDummy;
			Factory.Save();

			dummy.LocationWhsGuid = whs.PK;

			// row validation

			dummy.LocationString = "A";
			AssertEquals("Valid row has caused error.", false, dummy.LocationStringInfo.HasErrors());
			AssertEquals("Guid should be set", row.Locations[0].PK, dummy.WLV_FK);
			dummy.LocationString = ZString.Empty;
			AssertEquals("Guid should be empty.", ZGuid.Empty, dummy.WLV_FK);

			// column validation
			dummy.LocationString = "A-0";
			AssertEquals("Valid column has caused error.", false, dummy.LocationStringInfo.HasErrors());
			AssertEquals("Guid should be set.", row.Locations[0].PK, dummy.WLV_FK);
			dummy.LocationString = "A-3";
			AssertEquals("Valid column has caused error.", false, dummy.LocationStringInfo.HasErrors());
			AssertEquals("Guid should be set.", row.Locations[18].PK, dummy.WLV_FK);
			dummy.LocationString = "A-4";
			AssertHasError(dummy.LocationStringInfo, row.RangeMessage("Column", row.WR_Columns, whs.WW_LocationColumnsZeroBased));

			// level validation
			dummy.LocationString = "A-0-0";
			AssertEquals("Valid level has caused error.", false, dummy.LocationStringInfo.HasErrors());
			AssertEquals("Guid should be set.", row.Locations[0].PK, dummy.WLV_FK);
			dummy.LocationString = "A-0-2";
			AssertEquals("Valid level has caused error.", false, dummy.LocationStringInfo.HasErrors());
			AssertEquals("Guid should be set.", row.Locations[4].PK, dummy.WLV_FK);
			dummy.LocationString = "A-0-3";
			AssertHasError(dummy.LocationStringInfo, row.RangeMessage("Level", row.WR_Levels, whs.WW_LocationLevelsZeroBased));

			// tray validation
			dummy.LocationString = "A-0-0-0";
			AssertEquals("Valid tray has caused error.", false, dummy.LocationStringInfo.HasErrors());
			AssertEquals("Guid should be set.", row.Locations[0].PK, dummy.WLV_FK);
			dummy.LocationString = "A-0-0-1";
			AssertEquals("Valid tray has caused error.", false, dummy.LocationStringInfo.HasErrors());
			AssertEquals("Guid should be set.", row.Locations[1].PK, dummy.WLV_FK);
			dummy.LocationString = "A-0-0-2";
			AssertHasError(dummy.LocationStringInfo, row.RangeMessage("Tray", row.WR_Trays, whs.WW_LocationTraysZeroBased));
		}

		#endregion

		#region TestErrorIfLocationStringWithZeroBaseInvalid

		public void TestErrorIfLocationStringInvalid_FixedWidthWarehouse()
		{
			var whs = Helper.CreateFixedWidthLocationWarehouse("1", 2, 2, 2);
			Helper.CreateRowAndGenerateLocations(whs, "A", 4, 3, 2);
			var dummy = LocationCrashTestDummy;
			Factory.Save();

			dummy.LocationWhsGuid = whs.PK;
			AssertNoErrors("Precondition", dummy.LocationStringInfo);
			dummy.LocationString = "A040302";
			AssertNoErrors(dummy.LocationStringInfo);

			dummy.LocationString = "A-04-03-03";
			AssertHasError(dummy.LocationStringInfo, "Please enter a valid Location.");

			dummy.LocationString = "A-04-03-02";
			AssertNoErrors(dummy.LocationStringInfo);

			dummy.LocationString = "A05";
			AssertHasError(dummy.LocationStringInfo, "Please enter a valid Location.");

			dummy.LocationString = "A04";
			AssertNoErrors(dummy.LocationStringInfo);

			dummy.LocationString = "BLAH";
			AssertHasError(dummy.LocationStringInfo, "Please enter a valid Location.");
		}

		#endregion
	}
}
