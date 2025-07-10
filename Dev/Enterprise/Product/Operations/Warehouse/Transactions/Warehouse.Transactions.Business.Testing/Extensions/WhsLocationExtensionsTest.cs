using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;

// Extracted from Warehouse\Transactions\Business\Extensions\WhsLocationExtensions.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsLocationExtensionsTest : WhsTestCaseWithFactory
	{
		#region TestCanTransferToOrFromLocation

		public void TestCanTransferToOrFromLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var area1 = data.Whs1.Areas[0];
			var area2 = Helper.CreateArea(data.Whs1, "OTHER");
			location1.WLV_WA_PickingArea = area1.PK;
			location2.WLV_WA_PickingArea = area2.PK;
			Factory.Save();

			AssertEquals(false, ((WhsLocation)null).CanTransferToOrFromLocation(location2));
			AssertEquals(false, location1.CanTransferToOrFromLocation(null));
			AssertEquals(true, location1.CanTransferToOrFromLocation(location2));
			AssertEquals(true, location2.CanTransferToOrFromLocation(location1));

			area1.WA_AreaType = AreaTypes.Codes.Bonded;
			Factory.Save();
			AssertEquals(false, location1.CanTransferToOrFromLocation(location2));
			AssertEquals(false, location2.CanTransferToOrFromLocation(location1));

			area2.WA_AreaType = AreaTypes.Codes.Excise;
			Factory.Save();
			AssertEquals(true, location1.CanTransferToOrFromLocation(location2));
			AssertEquals(true, location2.CanTransferToOrFromLocation(location1));

			area1.WA_AreaType = AreaTypes.Codes.FreeStore;
			Factory.Save();
			AssertEquals(false, location1.CanTransferToOrFromLocation(location2));
			AssertEquals(false, location2.CanTransferToOrFromLocation(location1));
		}

		public void TestCanTransferToOrFromLocation_DynamicAreas()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var otherLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation = data.Whs1.FindLocation("A-2");
			var otherArea = Helper.CreateArea(data.Whs1, "AREA");
			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC");
			otherLocation.WLV_WA_PickingArea = otherArea.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			dynamicArea.WA_IsPutawayArea = false;
			dynamicArea.WA_AreaType = AreaTypes.Codes.DynamicPickFace;
			Factory.Save();

			CombineAssertions(() =>
			{
				otherArea.WA_AreaType = AreaTypes.Codes.DynamicPickFace;
				otherArea.WA_IsPutawayArea = false;
				otherArea.WA_IsDefaultPutawayArea = false;
				Factory.Save();
				AssertEquals("Can transfer from Dynamic to Dynamic", true,
					otherLocation.CanTransferToOrFromLocation(dynamicLocation));

				otherArea.WA_AreaType = AreaTypes.Codes.FreeStore;
				Factory.Save();
				AssertEquals("Can transfer from FreeStore to Dynamic", true,
					otherLocation.CanTransferToOrFromLocation(dynamicLocation));
				AssertEquals("Can transfer from Dynamic to FreeStore", true,
					dynamicLocation.CanTransferToOrFromLocation(otherLocation));

				otherArea.WA_AreaType = AreaTypes.Codes.DockDoor;
				Factory.Save();
				AssertEquals("Can transfer from DockDoor to Dynamic", true,
					otherLocation.CanTransferToOrFromLocation(dynamicLocation));
				AssertEquals("Can transfer from Dynamic to DockDoor", true,
					dynamicLocation.CanTransferToOrFromLocation(otherLocation));

				otherArea.WA_AreaType = AreaTypes.Codes.Bonded;
				Factory.Save();
				AssertEquals("Cannot transfer from Bonded to Dynamic", false,
					otherLocation.CanTransferToOrFromLocation(dynamicLocation));
				AssertEquals("Cannot transfer from Dynamic to Bonded", false,
					dynamicLocation.CanTransferToOrFromLocation(otherLocation));

				otherArea.WA_AreaType = AreaTypes.Codes.Excise;
				Factory.Save();
				AssertEquals("Cannot transfer from Excise to Dynamic", false,
					otherLocation.CanTransferToOrFromLocation(dynamicLocation));
				AssertEquals("Cannot transfer from Dynamic to Excise", false,
					dynamicLocation.CanTransferToOrFromLocation(otherLocation));
			});
		}

		#endregion
	}
}
