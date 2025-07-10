using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.eTail.DataTransfer.Testing
{
	[TestedType(typeof(HVLVOriginLoadListConsolXUSAllocator))]
	sealed class HVLVOriginLoadListConsolXUSAllocatorTest : TestCaseWithFactory
	{
		public void TestTryCreateConsolAndAttachLoadLists()
		{
			var logger = new DummyLogger();
			var allocator = new HVLVOriginLoadListConsolXUSAllocator(logger);
			var loadList = GetLoadList("Dummy");
			var succeed = allocator.TryCreateConsolAndAttachLoadLists(new[] { loadList }, out var errorMessage);

			CombineAssertions(() =>
			{
				Assert("Processing succeed", succeed);
				Assert("consol is not saved", allocator.AllocatedConsol is BusinessObject consol && !consol.IsInDatabase);
				AssertNullOrEmpty("No error message", errorMessage);
			});
		}

		public void TestTryAttachToConsol()
		{
			const string MasterBillNumber = "MAWB";
			var logger = new DummyLogger();
			var allocator = new HVLVOriginLoadListConsolXUSAllocator(logger);
			var loadList = GetLoadList(MasterBillNumber);
			var existingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			existingConsol.JK_MasterBillNum = MasterBillNumber;

			var existingConsolWithMismatchedWayBillNumber = Factory.NewWithValidTestData<ForwardingConsol>();
			existingConsolWithMismatchedWayBillNumber.JK_MasterBillNum = "Dummy";

			Factory.Save();

			var succeed = allocator.TryAttachToConsol(null, new[] { loadList }, out var errorMessage);

			CombineAssertions("Attaching when target consol is not specified", () =>
			{
				Assert("Attaching succeed", succeed);
				AssertEquals("Should be able to find the existing consol to attach to by waybill matching", existingConsol.PK, allocator.AllocatedConsol.PK);
				AssertNullOrEmpty("No error message", errorMessage);
			});

			((IBusinessObjectFactoryInternals)Factory).Rollback();
			loadList.Reload();

			AssertEquals("precondition: no shipments", 0, existingConsolWithMismatchedWayBillNumber.Shipments.Count);

			succeed = allocator.TryAttachToConsol(existingConsolWithMismatchedWayBillNumber, new[] { loadList }, out errorMessage);

			CombineAssertions("Attah to target consol when specified dispite waybill number does not match", () =>
			{
				Assert("Attaching succeed", succeed);
				AssertEquals("Should be attached to the target consol", existingConsolWithMismatchedWayBillNumber.PK, allocator.AllocatedConsol.PK);
				AssertEquals("New shipment is created", 1, existingConsolWithMismatchedWayBillNumber.Shipments.Count);
				AssertNullOrEmpty("No error message", errorMessage);
			});
		}

		public void TestTryAttachToConsol_PopulateMaterBillNumberWhenLoadlistIsNeutralMaster()
		{
			const string MasterBillNumber = "MAWB";
			var logger = new DummyLogger();
			var allocator = new HVLVOriginLoadListConsolXUSAllocator(logger);
			var loadList = GetLoadList(MasterBillNumber);
			loadList.HVL_IsNeutralMaster = true;
			var consolForLoadListNeutralMaster = Factory.NewWithValidTestData<ForwardingConsol>();
			consolForLoadListNeutralMaster.JK_MasterBillNum = MasterBillNumber;
			Factory.Save();

			var succeed = allocator.TryAttachToConsol(consolForLoadListNeutralMaster, new[] { loadList }, out var errorMessage);
			var service = Factory.ServiceContainer.GetAfterOnSavingService<PopulateMasterBillNumberForNeutralMasterLoadListService>();
			AssertNotNull("Before save, the service should be available in the service container", service);
			Factory.Save();

			service = Factory.ServiceContainer.GetAfterOnSavingService<PopulateMasterBillNumberForNeutralMasterLoadListService>();
			CombineAssertions("Load list is attached to consol and the Master bill number is successfully populated", () =>
			{
				Assert("Attaching succeed", succeed);
				AssertEquals("Should be attached to the target consol", consolForLoadListNeutralMaster.PK, allocator.AllocatedConsol.PK);
				AssertEquals("New shipment is created", 1, consolForLoadListNeutralMaster.Shipments.Count);
				AssertNull("after save, the service should be unavailable in the service container", service);
				AssertEquals("Load list master bill number is populated", consolForLoadListNeutralMaster.JK_MasterBillNum, loadList.HVL_MasterBillNumber);
				AssertNullOrEmpty("No error message", errorMessage);
			});
		}

		public void TestTryAttachToConsol_SetsDefaultCommodityOnOuterPackLinesForHVLShipments()
		{
			var commodityCode = Factory.New<RefCommodityCode>();
			commodityCode.RH_Code = "YYY";
			Env.Registry.CommodityCode = commodityCode.PK.ToGuid();

			var allocator = new HVLVOriginLoadListConsolXUSAllocator(new DummyLogger());
			var loadList = GetLoadList("Dummy");
			loadList.HVL_IsMasterHouse = true;
			loadList.OuterPackages.AddNew();

			_ = allocator.TryCreateConsolAndAttachLoadLists([loadList], out _);

			var hvlShipment = allocator.AllocatedConsol.Shipments.First(x => x.JS_ShipmentType == "HVL") as ForwardingShipment;
			var hvmShipment = allocator.AllocatedConsol.Shipments.First(x => x.JS_ShipmentType == "HVM") as ForwardingShipment;

			CombineAssertions(() =>
			{
				AssertEquals("HVL shipment packline commodityCode", "YYY", hvlShipment.OuterPackLines[0].JL_RH_NKCommodityCode);
				AssertEquals("HVM shipment packline commodityCode", ZString.Empty, hvmShipment.OuterPackLines[0].JL_RH_NKCommodityCode);
			});
		}

		HVLVOriginLoadList GetLoadList(string masterBillNumber)
		{
			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_Status = "LDG";
			originLoadList.HVL_MasterBillNumber = masterBillNumber;

			var bookingHeader = GetBookingHeader();
			var consignment = bookingHeader.Consignments.AddNew();
			var item = consignment.Items.AddNew();
			item.HVI_HVL_LoadList = originLoadList.PK;

			return originLoadList;
		}

		HVLVBookingHeader GetBookingHeader()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_IsBookingConfirmed = true;
			bookingHeader.HVH_IsBookingReceived = true;

			return bookingHeader;
		}
	}
}
