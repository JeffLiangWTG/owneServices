using System;
using System.Reflection;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.eTail.Module.Testing
{
	[TestedType(typeof(HVLVBookingHeaderFilterBusinessObject))]
	class HVLVBookingHeaderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Numbers and References

		public void TestBookingReference()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader1.HVH_BookingReference = "M100";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader2.HVH_BookingReference = "M150";

			var bookingHeader3 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader3.HVH_BookingReference = "M200";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Booking Header #"];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "M1";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader1, bookingHeader2 }, results);
		}

		public void TestLoadListNo()
		{
			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_UniqueReference = "LOADLIST1";
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_UniqueReference = "LOADLIST2";

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item1 = bookingHeader1.Consignments.AddNew().Items.AddNew();
			item1.HVI_ItemId = "ITEM1";
			item1.HVI_HVL_LoadList = loadList1.PK;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item2 = bookingHeader2.Consignments.AddNew().Items.AddNew();
			item2.HVI_ItemId = "ITEM2";
			item2.HVI_HVL_LoadList = loadList2.PK;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Load List #"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "LOADLIST2";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestItemShipmentNo()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "SHIPMENT1";
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "SHIPMENT2";

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item1 = bookingHeader1.Consignments.AddNew().Items.AddNew();
			item1.HVI_ItemId = "ITEM1";
			item1.HVI_JS_LoadedOnShipment = shipment1.PK;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item2 = bookingHeader2.Consignments.AddNew().Items.AddNew();
			item2.HVI_ItemId = "ITEM2";
			item2.HVI_JS_LoadedOnShipment = shipment2.PK;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Item Shipment #"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "SHIPMENT2";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestConsolNo()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.Consols.AddNew().JK_UniqueConsignRef = "CONSOL1";
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.Consols.AddNew().JK_UniqueConsignRef = "CONSOL2";

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item1 = bookingHeader1.Consignments.AddNew().Items.AddNew();
			item1.HVI_ItemId = "ITEM1";
			item1.HVI_JS_LoadedOnShipment = shipment1.PK;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item2 = bookingHeader2.Consignments.AddNew().Items.AddNew();
			item2.HVI_ItemId = "ITEM2";
			item2.HVI_JS_LoadedOnShipment = shipment2.PK;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Consol #"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "CONSOL2";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestConsignmentId()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
				var consignment1 = bookingHeader1.Consignments.AddNew();
				consignment1.HVC_WaybillNumber = "CONSIGN1";

				var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
				var consignment2 = bookingHeader2.Consignments.AddNew();
				consignment2.HVC_WaybillNumber = "CONSIGN2";

				Factory.Save();

				consignment1.HVC_WaybillNumber = "WAYBILL1";
				consignment2.HVC_WaybillNumber = "WAYBILL2";

				Factory.Save();

				AssertEquals("Precondition: Consignment ID defaulted", "CONSIGN1", consignment1.HVC_ConsignmentId);
				AssertEquals("Precondition: Consignment ID defaulted", "CONSIGN2", consignment2.HVC_ConsignmentId);

				var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
				var filter = (ModuleTextFilter)filterBizO["Consignment ID"];
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "CONSIGN2";
				filter.IsActive = true;

				var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

				AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
			}
		}

		public void TestItemId()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item1 = bookingHeader1.Consignments.AddNew().Items.AddNew();

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item2 = bookingHeader2.Consignments.AddNew().Items.AddNew();

			Factory.Save();

			item1.HVI_ItemId = "ITEM1";
			item2.HVI_ItemId = "ITEM2";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Item ID"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "ITEM2";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestShipperReference_Consignment()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader1.Consignments.AddNew().HVC_ShipperReference = "SHIPREF1";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader2.Consignments.AddNew().HVC_ShipperReference = "SHIPREF2";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Consignment Shipper Reference"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "SHIPREF2";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestShipperReference_Item()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item1 = bookingHeader1.Consignments.AddNew().Items.AddNew();
			item1.HVI_ItemId = "Item1";
			item1.HVI_ShipperReference = "SHIPREF1";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item2 = bookingHeader2.Consignments.AddNew().Items.AddNew();
			item2.HVI_ItemId = "Item2";
			item2.HVI_ShipperReference = "SHIPREF2";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Item Shipper Reference"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "SHIPREF2";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestWaybillNumber_Consignment()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_WaybillNumber = "NUMBER1";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_WaybillNumber = "NUMBER2";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Consignment Waybill #"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "NUMBER2";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestCurrentBarcode_Item()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			var item1 = consignment1.Items.AddNew();
			item1.HVI_ItemId = "ITEM1";
			item1.HVI_CurrentBarcode = "BARCODE1";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			var item2 = consignment2.Items.AddNew();
			item2.HVI_ItemId = "ITEM2";
			item2.HVI_CurrentBarcode = "BARCODE2";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Item Current Barcode"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "BARCODE2";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestReferencesValidated_Consignment()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_IsValidatedForUniqueness = false;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_IsValidatedForUniqueness = true;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Consignment References Validated"];
			filter.Property0 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestReferencesValidated_Item()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item1 = bookingHeader1.Consignments.AddNew().Items.AddNew();
			item1.HVI_ItemId = "ITEM1";
			item1.HVI_IsValidatedForUniqueness = false;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item2 = bookingHeader2.Consignments.AddNew().Items.AddNew();
			item2.HVI_ItemId = "ITEM2";
			item2.HVI_IsValidatedForUniqueness = true;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Item References Validated"];
			filter.Property0 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		#endregion

		#region Status and Flags

		public void TestManifestStatus()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader1.HVH_IsBookingConfirmed = false;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader2.HVH_IsBookingConfirmed = true;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Manifest Status"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = HVLVBookingStatus.ConfirmedCode;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestReceivedAtOrigin()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader1.HVH_IsBookingReceived = false;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader2.HVH_IsBookingReceived = true;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Received at Origin"];
			filter.Property0 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);

			filter.Property0 = false;
			results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader1 }, results);
		}

		public void TestReceivedAtOrigin_HasDefaultModuleFilterSubGroup()
		{
			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Received at Origin"];
			var subGroupField = filter.GetType().GetField("moduleFilterSubGroup", BindingFlags.NonPublic | BindingFlags.Instance);

			AssertEquals(ModuleFilterSubGroup.Default, subGroupField.GetValue(filter));
		}

		public void TestConsignmentStatus()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_Status = "CNF";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_Status = "BKD";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Consignment Status"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "BKD";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestLoadListStatus()
		{
			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_Status = "CON";
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_Status = "LDG";

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item1 = bookingHeader1.Consignments.AddNew().Items.AddNew();
			item1.HVI_ItemId = "ITEM1";
			item1.HVI_HVL_LoadList = loadList1.PK;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item2 = bookingHeader2.Consignments.AddNew().Items.AddNew();
			item2.HVI_ItemId = "ITEM2";
			item2.HVI_HVL_LoadList = loadList2.PK;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Load List Status"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "LDG";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestConsignmentPreScreeningStatus()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Failed;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;

			var bookingHeader3 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment3 = bookingHeader3.Consignments.AddNew();
			consignment3.HVC_ConsignmentId = "CONSIGN3";
			consignment3.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Consignment Pre-Screening Status"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;
			filter.IsActive = true;

			var passedResults = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2, bookingHeader3 }, passedResults);

			filter.Property = HVLVConsignmentPreScreeningStatusCodes.Codes.Failed;

			var failedResults = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader1 }, failedResults);
		}

		public void TestExportCustomsClearanceStatus()
		{
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "%%%", "Code 1", "HLD", RefCusCodeListTypes.Codes.ExportCustomsStatus);
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "$$$", "Code 2", "HLD", RefCusCodeListTypes.Codes.ExportCustomsStatus);
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "&&&", "Code 3", "HLD", RefCusCodeListTypes.Codes.ExportCustomsStatus);

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader3 = Factory.NewWithValidTestData<HVLVBookingHeader>();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var consignment1 = bookingHeader1.Consignments.AddNew();
				consignment1.HVC_ConsignmentId = "CONSIGN1";
				consignment1.HVC_ExportCustomsClearanceStatus = "&&&";

				var consignment2 = bookingHeader2.Consignments.AddNew();
				consignment2.HVC_ConsignmentId = "CONSIGN2";
				consignment2.HVC_ExportCustomsClearanceStatus = "%%%";

				var consignment3 = bookingHeader3.Consignments.AddNew();
				consignment3.HVC_ConsignmentId = "CONSIGN3";
				consignment3.HVC_ExportCustomsClearanceStatus = "$$$";

				Factory.Save();

				var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
				var filter = (ModuleTextFilter)filterBizO["Export Customs Clearance Status"];
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "&&&";
				filter.IsActive = true;

				var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { bookingHeader1 }, results);
			}
		}

		public void TestImportCustomsClearanceStatus()
		{
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "%%%", "Code 1", "HLD");
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "$$$", "Code 2", "HLD");
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "&&&", "Code 3", "HLD");

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader3 = Factory.NewWithValidTestData<HVLVBookingHeader>();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var consignment1 = bookingHeader1.Consignments.AddNew();
				consignment1.HVC_ConsignmentId = "CONSIGN1";
				consignment1.HVC_ImportCustomsClearanceStatus = "&&&";

				var consignment2 = bookingHeader2.Consignments.AddNew();
				consignment2.HVC_ConsignmentId = "CONSIGN2";
				consignment2.HVC_ImportCustomsClearanceStatus = "%%%";

				var consignment3 = bookingHeader3.Consignments.AddNew();
				consignment3.HVC_ConsignmentId = "CONSIGN3";
				consignment3.HVC_ImportCustomsClearanceStatus = "$$$";

				Factory.Save();

				var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
				var filter = (ModuleTextFilter)filterBizO["Import Customs Clearance Status"];
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "&&&";
				filter.IsActive = true;

				var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { bookingHeader1 }, results);
			}
		}

		public void TestReleaseStatus_HasReleaseStatusCodesAsOption()
		{
			var releaseStatusCodeList = HVLVReleaseStatus.GetAll();
			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Release Status"];

			AssertEquals($"Release status filter should have {releaseStatusCodeList.Count} options.", releaseStatusCodeList.Count, filter.List.Count);
			AssertContainsExactElementsInAnyOrder("Elements should be same with code list.", releaseStatusCodeList.GetAllCodes(), ((CodeDescriptionPairList)filter.List).GetAllCodes());
		}

		public void TestReleaseStatus()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ReleaseStatus = HVLVReleaseStatus.Cleared;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ReleaseStatus = HVLVReleaseStatus.Held;

			var bookingHeader3 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment3 = bookingHeader3.Consignments.AddNew();
			consignment3.HVC_ConsignmentId = "CONSIGN3";
			consignment3.HVC_ReleaseStatus = HVLVReleaseStatus.None;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Release Status"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = HVLVReleaseStatus.Held;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestItemCarrierBookingStatus_HasBookingStatusCodesAsOption()
		{
			var bookingStatusCodeList = HVLVItemLookups.GetAllHVLVItemCarrierBookingStatuses();
			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Item Carrier Booking Status"];

			AssertEquals($"Release status filter should have {bookingStatusCodeList.Count} options.", bookingStatusCodeList.Count, filter.List.Count);
			AssertContainsExactElementsInAnyOrder("Elements should be same with code list.", bookingStatusCodeList.GetAllCodes(), ((CodeDescriptionPairList)filter.List).GetAllCodes());
		}

		public void TestItemCarrierBookingStatus()
		{
			var bookingHeaderNotBooked = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeaderNotBooked.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			var item1 = consignment1.Items.AddNew();
			item1.HVI_CarrierBookingStatus = HVLVItemCarrierBookingStatus.Codes.NotBooked;

			var bookingHeaderRequested = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeaderRequested.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			var item2 = consignment2.Items.AddNew();
			item2.HVI_CarrierBookingStatus = HVLVItemCarrierBookingStatus.Codes.BookingRequested;

			var bookingHeaderConfirmed = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment3 = bookingHeaderConfirmed.Consignments.AddNew();
			consignment3.HVC_ConsignmentId = "CONSIGN3";
			var item3 = consignment3.Items.AddNew();
			item3.HVI_CarrierBookingStatus = HVLVItemCarrierBookingStatus.Codes.BookingConfirmed;

			var bookingheaderRejected = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment4 = bookingheaderRejected.Consignments.AddNew();
			consignment4.HVC_ConsignmentId = "CONSIGN4";
			var item4 = consignment4.Items.AddNew();
			item4.HVI_CarrierBookingStatus = HVLVItemCarrierBookingStatus.Codes.BookingRejected;

			var bookingHeaderCancelled = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment5 = bookingHeaderCancelled.Consignments.AddNew();
			consignment5.HVC_ConsignmentId = "CONSIGN5";
			var item5 = consignment5.Items.AddNew();
			item5.HVI_CarrierBookingStatus = HVLVItemCarrierBookingStatus.Codes.BookingCancelled;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Item Carrier Booking Status"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = HVLVItemCarrierBookingStatus.Codes.NotBooked;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Expected bookingHeaderNotBooked to be found by the filter.", new[] { bookingHeaderNotBooked }, results);

			filter.Property = HVLVItemCarrierBookingStatus.Codes.BookingRequested;

			results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Expected bookingHeaderRequested to be found by the filter.", new[] { bookingHeaderRequested }, results);

			filter.Property = HVLVItemCarrierBookingStatus.Codes.BookingConfirmed;

			results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Expected bookingHeaderConfirmed to be found by the filter.", new[] { bookingHeaderConfirmed }, results);

			filter.Property = HVLVItemCarrierBookingStatus.Codes.BookingRejected;

			results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Expected bookingheaderRejected to be found by the filter.", new[] { bookingheaderRejected }, results);

			filter.Property = HVLVItemCarrierBookingStatus.Codes.BookingCancelled;

			results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Expected bookingHeaderCancelled to be found by the filter.", new[] { bookingHeaderCancelled }, results);
		}

		public void TestConfirmed()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader1.HVH_IsBookingConfirmed = true;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader2.HVH_IsBookingConfirmed = true;

			var bookingHeader3 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader3.HVH_IsBookingConfirmed = false;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Confirmed"];
			filter.Property0 = true;
			filter.IsActive = true;
			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder("Should find confirmed booking headers", new[] { bookingHeader1, bookingHeader2 }, results);

			filter.Property0 = false;
			results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder("Should find non confirmed booking header", new[] { bookingHeader3 }, results);
		}

		public void TestConfirmed_HasDefaultModuleFilterSubGroup()
		{
			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Confirmed"];
			var subGroupField = filter.GetType().GetField("moduleFilterSubGroup", BindingFlags.NonPublic | BindingFlags.Instance);

			AssertEquals(ModuleFilterSubGroup.Default, subGroupField.GetValue(filter));
		}

		#endregion

		#region Locations

		public void TestConsigneeCity()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ConsigneeCity = "Eichenwalde";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ConsigneeCity = "Hanamura";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Consignee City"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "HANAMURA";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestConsigneeState()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ConsigneeState = "QLD";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ConsigneeState = "NSW";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Consignee State"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "NSW";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestConsigneePostcode()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ConsigneePostcode = "3000";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ConsigneePostcode = "2000";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Consignee Postcode"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "2000";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestConsigneeCountry()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_RN_NKConsigneeCountryCode = "AU";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_RN_NKConsigneeCountryCode = "ZA";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleNkFilter)filterBizO["Consignee Country"];
			filter.Property = "ZA";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestShipperCity()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ShipperCity = "Eichenwalde";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ShipperCity = "Hanamura";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Shipper City"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "HANAMURA";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestShipperState()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ShipperState = "QLD";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ShipperState = "NSW";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Shipper State"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "NSW";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestShipperPostcode()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ShipperPostcode = "3000";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ShipperPostcode = "2000";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Shipper Postcode"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "2000";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestShipperCountry()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_RN_NKShipperCountryCode = "AU";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_RN_NKShipperCountryCode = "ZA";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleNkFilter)filterBizO["Shipper Country"];
			filter.Property = "ZA";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestReturnCity()
		{
			var bookingHeaderSydney = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignmentSydney = bookingHeaderSydney.Consignments.AddNew();
			consignmentSydney.HVC_ConsignmentId = "CONSIGN1";
			consignmentSydney.HVC_ReturnCity = "Sydney";

			var bookingHeaderMelbourne = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignmentMelbourne = bookingHeaderMelbourne.Consignments.AddNew();
			consignmentMelbourne.HVC_ConsignmentId = "CONSIGN2";
			consignmentMelbourne.HVC_ReturnCity = "Melbourne";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Return City"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "Melbourne";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeaderMelbourne }, results);
		}

		public void TestReturnState()
		{
			var bookingHeaderNSW = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignmentNSW = bookingHeaderNSW.Consignments.AddNew();
			consignmentNSW.HVC_ConsignmentId = "CONSIGN1";
			consignmentNSW.HVC_ReturnState = "NSW";

			var bookingHeaderVIC = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignmentVIC = bookingHeaderVIC.Consignments.AddNew();
			consignmentVIC.HVC_ConsignmentId = "CONSIGN2";
			consignmentVIC.HVC_ReturnState = "VIC";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Return State"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "VIC";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeaderVIC }, results);
		}

		public void TestReturnPostCode()
		{
			var bookingHeader2032 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2032 = bookingHeader2032.Consignments.AddNew();
			consignment2032.HVC_ConsignmentId = "CONSIGN1";
			consignment2032.HVC_ReturnPostcode = "2032";

			var bookingHeader2000 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2000 = bookingHeader2000.Consignments.AddNew();
			consignment2000.HVC_ConsignmentId = "CONSIGN2";
			consignment2000.HVC_ReturnPostcode = "2000";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Return Postcode"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "2000";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2000 }, results);
		}

		public void TestReturnCountry()
		{
			var bookingHeaderAU = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignmentAU = bookingHeaderAU.Consignments.AddNew();
			consignmentAU.HVC_ConsignmentId = "CONSIGN1";
			consignmentAU.HVC_RN_NKReturnCountryCode = "AU";

			var bookingHeaderUS = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignmentUS = bookingHeaderUS.Consignments.AddNew();
			consignmentUS.HVC_ConsignmentId = "CONSIGN2";
			consignmentUS.HVC_RN_NKReturnCountryCode = "US";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleNkFilter)filterBizO["Return Country"];
			filter.Property = "US";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeaderUS }, results);
		}

		#endregion

		#region Organisations and Staff

		public void TestBillToParty()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader1.HVH_OA_BillToParty = org1.MainAddress.PK;
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader2.HVH_OA_BillToParty = org2.MainAddress.PK;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO["Bill To Party"];
			filter.Property = org2.PK;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestOriginDepot()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader1.HVH_OA_OriginDepot = org1.MainAddress.PK;
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader2.HVH_OA_OriginDepot = org2.MainAddress.PK;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO["Origin Depot"];
			filter.Property = org2.PK;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestDispatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader1.HVH_OA_DispatchAddress = org1.MainAddress.PK;
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader2.HVH_OA_DispatchAddress = org2.MainAddress.PK;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO["Dispatch"];
			filter.Property = org2.PK;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestDestinationDepot()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";

			Factory.Save();

			consignment1.HVC_OA_DestinationDepot = org1.MainAddress.PK;
			consignment2.HVC_OA_DestinationDepot = org2.MainAddress.PK;
			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO["Destination Depot"];
			filter.Property = org2.PK;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestConsignee()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ConsigneeName = "FRANK";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ConsigneeName = "BARRY WONG";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Consignee"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "BARRY WONG";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestShipper()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ShipperName = "FRANK";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ShipperName = "BARRY WONG";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Shipper"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "BARRY WONG";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestReturnName()
		{
			var bookingHeaderBone = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignmentBone = bookingHeaderBone.Consignments.AddNew();
			consignmentBone.HVC_ConsignmentId = "CONSIGN1";
			consignmentBone.HVC_ReturnName = "Bone";

			var bookingHeaderJustin = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignmentJustin = bookingHeaderJustin.Consignments.AddNew();
			consignmentJustin.HVC_ConsignmentId = "CONSIGN2";
			consignmentJustin.HVC_ReturnName = "Justin";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Return Name"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "Justin";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeaderJustin }, results);
		}

		public void TestBookedBy()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader1.HVH_OC_BookedBy = contact1.PK;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader2.HVH_OC_BookedBy = contact2.PK;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO["Booked By"];
			filter.Property = contact2.PK;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestLastMileCarrier()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";

			Factory.Save();

			consignment1.HVC_OH_LastMileCarrier = org1.PK;
			consignment2.HVC_OH_LastMileCarrier = org2.PK;
			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO["Last Mile Carrier"];
			filter.Property = org2.PK;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		#endregion

		#region Modes and Types

		public void TestServiceLevel()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader1.HVH_RS_NKBookingServiceLevel = "STD";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader2.HVH_RS_NKBookingServiceLevel = "D2D";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Service Level"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "D2D";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestLastMileCarrierServiceLevel()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";

			Factory.Save();

			consignment1.HVC_PL_NKLastMileCarrierServiceLevel = "STD";
			consignment2.HVC_PL_NKLastMileCarrierServiceLevel = "D2D";
			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Last Mile Carrier Service Level"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "D2D";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		#endregion

		#region Related Shipments

		public void TestRelatedShipmentsFilter()
		{
			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader3 = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment1 = bookingHeader1.Consignments.AddNew();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			var consignment3 = bookingHeader3.Consignments.AddNew();

			var shipment1_1 = Factory.New<ForwardingShipment>();
			var shipment2_1 = Factory.New<ForwardingShipment>();

			var item1_1 = consignment1.Items.AddNew();
			var item2_1 = consignment1.Items.AddNew();

			shipment1_1.JS_TransportMode = TransportModes.Air;
			shipment1_1.JS_HouseBill = "HB1";
			item1_1.HVI_JS_LoadedOnShipment = shipment1_1.PK;

			shipment2_1.JS_TransportMode = TransportModes.Air;
			shipment2_1.JS_HouseBill = "HB1";
			item2_1.HVI_JS_LoadedOnShipment = shipment2_1.PK;

			var shipment1_2 = Factory.New<ForwardingShipment>();
			var shipment2_2 = Factory.New<ForwardingShipment>();

			var item1_2 = consignment2.Items.AddNew();
			var item2_2 = consignment2.Items.AddNew();

			shipment1_2.JS_TransportMode = TransportModes.Sea;
			shipment1_2.JS_HouseBill = "HB1";
			item1_2.HVI_JS_LoadedOnShipment = shipment1_2.PK;

			shipment2_2.JS_TransportMode = TransportModes.Sea;
			shipment2_2.JS_HouseBill = "HB2";
			item2_2.HVI_JS_LoadedOnShipment = shipment2_2.PK;

			var shipment1_3 = Factory.New<ForwardingShipment>();
			var shipment2_3 = Factory.New<ForwardingShipment>();

			var item1_3 = consignment3.Items.AddNew();
			var item2_3 = consignment3.Items.AddNew();

			shipment1_3.JS_TransportMode = TransportModes.Sea;
			shipment1_3.JS_HouseBill = "HB2";
			item1_3.HVI_JS_LoadedOnShipment = shipment1_3.PK;

			shipment2_3.JS_TransportMode = TransportModes.Sea;
			shipment2_3.JS_HouseBill = "HB2";
			item2_3.HVI_JS_LoadedOnShipment = shipment2_3.PK;

			Factory.Save();

			var relatedShipmentsFilter = (ModuleGuidFilter)filterBizO["Related Shipments"];
			relatedShipmentsFilter.IsActive = true;
			relatedShipmentsFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			relatedShipmentsFilter.Property = shipment1_1.PK;

			var bookingHeaderResults = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader1 }, bookingHeaderResults);

			relatedShipmentsFilter.Property = shipment1_2.PK;

			bookingHeaderResults = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, bookingHeaderResults);

			relatedShipmentsFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.FiltersMatch;
			relatedShipmentsFilter.SelectedFilters.AddTextFilterStrip("Transport Mode", TransportModes.Sea).SqlComparisonOperator = SQLComparisonOperator.Equal;
			relatedShipmentsFilter.SelectedFilters.AddTextFilterStrip("House Bill", "HB2").SqlComparisonOperator = SQLComparisonOperator.Equal;

			bookingHeaderResults = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2, bookingHeader3 }, bookingHeaderResults);
		}

		#endregion

		#region Related HVLVOriginLoadList

		public void TestRelatedOriginLoadListFilter()
		{
			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader3 = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment1 = bookingHeader1.Consignments.AddNew();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			var consignment3 = bookingHeader3.Consignments.AddNew();

			var loadList1_1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var loadList2_1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();

			var item1_1 = consignment1.Items.AddNew();
			var item2_1 = consignment1.Items.AddNew();

			loadList1_1.HVL_TransportMode = TransportModes.Air;
			loadList1_1.HVL_HouseBillNumber = "HB1";
			item1_1.HVI_HVL_LoadList = loadList1_1.PK;

			loadList2_1.HVL_TransportMode = TransportModes.Air;
			loadList2_1.HVL_HouseBillNumber = "HB1";
			item2_1.HVI_HVL_LoadList = loadList2_1.PK;

			var loadList1_2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var loadList2_2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();

			var item1_2 = consignment2.Items.AddNew();
			var item2_2 = consignment2.Items.AddNew();

			loadList1_2.HVL_TransportMode = TransportModes.Sea;
			loadList1_2.HVL_HouseBillNumber = "HB1";
			item1_2.HVI_HVL_LoadList = loadList1_2.PK;

			loadList2_2.HVL_TransportMode = TransportModes.Sea;
			loadList2_2.HVL_HouseBillNumber = "HB2";
			item2_2.HVI_HVL_LoadList = loadList2_2.PK;

			var loadList1_3 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var loadList2_3 = Factory.NewWithValidTestData<HVLVOriginLoadList>();

			var item1_3 = consignment3.Items.AddNew();
			var item2_3 = consignment3.Items.AddNew();

			loadList1_3.HVL_TransportMode = TransportModes.Sea;
			loadList1_3.HVL_HouseBillNumber = "HB2";
			item1_3.HVI_HVL_LoadList = loadList1_3.PK;

			loadList2_3.HVL_TransportMode = TransportModes.Sea;
			loadList2_3.HVL_HouseBillNumber = "HB2";
			item2_3.HVI_HVL_LoadList = loadList2_3.PK;

			Factory.Save();

			var relatedLoadListsFilter = (ModuleGuidFilter)filterBizO["Related HVLV Load Lists"];
			relatedLoadListsFilter.IsActive = true;
			relatedLoadListsFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			relatedLoadListsFilter.Property = loadList1_1.PK;

			var bookingHeaderResults = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader1 }, bookingHeaderResults);

			relatedLoadListsFilter.Property = loadList1_2.PK;

			bookingHeaderResults = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, bookingHeaderResults);

			relatedLoadListsFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.FiltersMatch;
			relatedLoadListsFilter.SelectedFilters.AddTextFilterStrip("Transport Mode", TransportModes.Sea).SqlComparisonOperator = SQLComparisonOperator.Equal;
			relatedLoadListsFilter.SelectedFilters.AddTextFilterStrip("House Bill #", "HB2").SqlComparisonOperator = SQLComparisonOperator.Equal;

			bookingHeaderResults = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2, bookingHeader3 }, bookingHeaderResults);
		}
		#endregion

		#region Other

		public void TestGoodsDescription()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_GoodsDescription = "BLAH";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_GoodsDescription = "HAY WHO FROO DAT";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Goods Description"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "HAY WHO FROO DAT";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestIsPerishable()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_IsPerishable = false;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_IsPerishable = true;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Customs Indicators"];
			filter.Property0 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestIsHazardous()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_IsHazardous = false;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_IsHazardous = true;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Customs Indicators"];
			filter.Property1 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestIsPersonalEffects()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_IsPersonalEffects = false;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_IsPersonalEffects = true;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Customs Indicators"];
			filter.Property2 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestIsTimber()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_IsTimber = false;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_IsTimber = true;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Customs Indicators"];
			filter.Property3 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestRequiresFumigation()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_RequiresFumigation = false;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_RequiresFumigation = true;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Customs Indicators"];
			filter.Property4 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestUNDGClass()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_UndgClass = "1.1";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_UndgClass = "1.2";

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["UNDG Class"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "1.2";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestIsSignatureRequired()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_IsSignatureRequired = false;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_IsSignatureRequired = true;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Signature Required"];
			filter.Property0 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestIsSelfBooked()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_IsSelfBooked = false;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_IsSelfBooked = true;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Self-booked"];
			filter.Property0 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestAuthorityToLeave()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_AuthorityToLeave = false;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_AuthorityToLeave = true;

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Authority to Leave"];
			filter.Property0 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		public void TestCreatedInPortal()
		{
			Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var elcEvent = bookingHeader2.Logs.AddNew();

			using (elcEvent.LockForUpdatingKeyFieldsForTesting())
			{
				elcEvent.SL_SE_NKEvent = AutoEvents.ELoadListConsolidatedCode;
			}

			Factory.Save();

			var filterBizO = new HVLVBookingHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Created in Portal"];
			filter.Property0 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVBookingHeader>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { bookingHeader2 }, results);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new HVLVBookingHeaderFilterBusinessObject();

		#endregion
	}
}
