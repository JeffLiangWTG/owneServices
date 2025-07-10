using System;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static CargoWise.EventReference.Constants;
using static Enterprise.Core.Constants;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.eTail.Module.Testing
{
	[TestedType(typeof(HVLVConsignmentFilterBusinessObject))]
	class HVLVConsignmentFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestActiveStatusFilter_FilterForActive()
		{
			var activeConsignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var inactiveConsignment = Factory.NewWithValidTestData<HVLVConsignment>();

			activeConsignment.HVC_IsActive = true;
			inactiveConsignment.HVC_IsActive = false;

			var filterBizo = GetNewFilterStripBusinessObject();
			var activeStatusFilter = (ModuleTextFilter)filterBizo["Active Status"];
			activeStatusFilter.IsActive = true;
			activeStatusFilter.Property = "Active";

			CombineAssertions("Should only match active consignments", () =>
			{
				AssertEquals("Active consignment", true, activeConsignment.MatchesFilter(activeStatusFilter.Query));
				AssertEquals("Inactive consignment", false, inactiveConsignment.MatchesFilter(activeStatusFilter.Query));
			});
		}

		public void TestActiveStatusFilter_FilterForInactive()
		{
			var activeConsignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var inactiveConsignment = Factory.NewWithValidTestData<HVLVConsignment>();

			activeConsignment.HVC_IsActive = true;
			inactiveConsignment.HVC_IsActive = false;

			var filterBizo = GetNewFilterStripBusinessObject();
			var activeStatusFilter = (ModuleTextFilter)filterBizo["Active Status"];
			activeStatusFilter.IsActive = true;
			activeStatusFilter.Property = "Inactive";

			CombineAssertions("Should only match inactive consignments", () =>
			{
				AssertEquals("Active consignment", false, activeConsignment.MatchesFilter(activeStatusFilter.Query));
				AssertEquals("Inactive consignment", true, inactiveConsignment.MatchesFilter(activeStatusFilter.Query));
			});
		}

		public void TestActiveStatusFilter_FilterForAny()
		{
			var activeConsignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var inactiveConsignment = Factory.NewWithValidTestData<HVLVConsignment>();

			activeConsignment.HVC_IsActive = true;
			inactiveConsignment.HVC_IsActive = false;

			var filterBizo = GetNewFilterStripBusinessObject();
			var activeStatusFilter = (ModuleTextFilter)filterBizo["Active Status"];
			activeStatusFilter.IsActive = true;
			activeStatusFilter.Property = "All";

			CombineAssertions("Should match all consignments", () =>
			{
				AssertEquals("Active consignment", true, activeConsignment.MatchesFilter(activeStatusFilter.Query));
				AssertEquals("Inactive consignment", true, inactiveConsignment.MatchesFilter(activeStatusFilter.Query));
			});
		}

		public void TestACASStatusFilter_FilterForEachOption()
		{
			CombineAssertions("All option of filter should be found", () =>
			{
				AssertACASStatusFilterForStatus(ACASActions.Code.SecurityFilingAssessmentInProgress);
				AssertACASStatusFilterForStatus(ACASActions.Code.SecurityFilingAssessmentComplete);
				AssertACASStatusFilterForStatus(ACASActions.Code.DoNotLoadHold);
				AssertACASStatusFilterForStatus(ACASActions.Code.SelecteeDataIssueHold);
				AssertACASStatusFilterForStatus(ACASActions.Code.SelecteeScreeningOrVerificationRequiredHold);
				AssertACASStatusFilterForStatus(ACASActions.Code.DoNotLoadHoldCurrentlyInPlace);
				AssertACASStatusFilterForStatus(ACASActions.Code.SelecteeDataIssueHoldCurrentlyInPlace);
				AssertACASStatusFilterForStatus(ACASActions.Code.SelecteeScreeningOrVerificationRequiredHoldCurrentlyInPlace);
				AssertACASStatusFilterForStatus(ACASActions.Code.DoNotLoadHoldRemoved);
				AssertACASStatusFilterForStatus(ACASActions.Code.SelecteeDataIssueHoldRemoved);
				AssertACASStatusFilterForStatus(ACASActions.Code.SelecteeScreeningOrVerificationRequiredHoldRemoved);
			});
		}

		void AssertACASStatusFilterForStatus(string status)
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ACASStatus = status;

			var filterBizo = GetNewFilterStripBusinessObject();
			var acasStatusFilter = (ModuleTextFilter)filterBizo["ACAS Status"];
			acasStatusFilter.IsActive = true;
			acasStatusFilter.Property = status;

			Assert(status + "ACAS Status", consignment.MatchesFilter(acasStatusFilter.Query));
		}

		#region Numbers and References
		public void TestConsignmentId()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment1.HVC_ConsignmentId = "CONSIGN1";
				consignment1.HVC_WaybillNumber = "CONSIGN1";

				var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment2.HVC_ConsignmentId = "CONSIGN2";
				consignment2.HVC_WaybillNumber = "CONSIGN2";

				Factory.Save();

				AssertEquals("Precondition: Consignment ID defaulted", "CONSIGN1", consignment1.HVC_ConsignmentId);
				AssertEquals("Precondition: Consignment ID defaulted", "CONSIGN2", consignment2.HVC_ConsignmentId);

				var filterBizO = GetNewFilterStripBusinessObject();
				var filter = (ModuleTextFilter)filterBizO["Consignment ID"];
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "CONSIGN2";
				filter.IsActive = true;

				var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

				AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
			}
		}

		public void TestItemId()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var item1 = consignment1.Items.AddNew();

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var item2 = consignment2.Items.AddNew();

			Factory.Save();

			item1.HVI_ItemId = "ITEM1";
			item2.HVI_ItemId = "ITEM2";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Item ID"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "ITEM2";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestShipperReference_Consignment()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ShipperReference = "SHIPREF1";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ShipperReference = "SHIPREF2";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Consignment Shipper Reference"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "SHIPREF2";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestShipperReference_Item()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var item1 = consignment1.Items.AddNew();
			item1.HVI_ItemId = "Item1";
			item1.HVI_ShipperReference = "SHIPREF1";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var item2 = consignment2.Items.AddNew();
			item2.HVI_ItemId = "Item2";
			item2.HVI_ShipperReference = "SHIPREF2";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Item Shipper Reference"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "SHIPREF2";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestWaybillNumber_Consignment()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_WaybillNumber = "NUMBER1";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_WaybillNumber = "NUMBER2";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Consignment Waybill #"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "NUMBER2";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestCurrentBarcode_Item()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var item1 = consignment1.Items.AddNew();
			item1.HVI_ItemId = "ITEM1";
			item1.HVI_CurrentBarcode = "BARCODE1";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var item2 = consignment2.Items.AddNew();
			item2.HVI_ItemId = "ITEM2";
			item2.HVI_CurrentBarcode = "BARCODE2";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Item Current Barcode"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "BARCODE2";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestConsignmentAdditionalReference()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var reference1 = consignment1.CustomsReferenceNumbers.AddNew();
			reference1.CE_EntryNum = "Num1";
			reference1.CE_EntryType = "COO";
			reference1.CE_RN_NKCountryCode = "AU";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var reference2 = consignment2.CustomsReferenceNumbers.AddNew();
			reference2.CE_EntryNum = "Num2";
			reference2.CE_EntryType = "COO";
			reference1.CE_RN_NKCountryCode = "AU";

			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			var reference3 = consignment2.CustomsReferenceNumbers.AddNew();
			reference3.CE_EntryNum = "Num1";
			reference3.CE_EntryType = "COO";
			reference3.CE_RN_NKCountryCode = "ES";

			var consignment4 = Factory.NewWithValidTestData<HVLVConsignment>();
			var reference4 = consignment2.CustomsReferenceNumbers.AddNew();
			reference3.CE_EntryNum = "Num1";
			reference3.CE_EntryType = "NNN";
			reference3.CE_RN_NKCountryCode = "AU";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ReferenceNumberFilter)filterBizO["Consignment Additional Reference #"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "Num1";
			filter.Type = "COO";
			filter.Country = "AU";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment1 }, results);
		}

		#endregion

		#region Status and Flags

		public void TestConsignmentStatus()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			bookingHeader1.Consignments.Add(consignment1);
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_Status = "CNF";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			bookingHeader2.Consignments.Add(consignment2);
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_Status = "BKD";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Consignment Status"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "BKD";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestConsignmentPreScreeningStatus()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Failed;

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;

			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment3.HVC_ConsignmentId = "CONSIGN3";
			consignment3.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Consignment Pre-Screening Status"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;
			filter.IsActive = true;

			var passedResults = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2, consignment3 }, passedResults);

			filter.Property = HVLVConsignmentPreScreeningStatusCodes.Codes.Failed;

			var failedResults = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consignment1 }, failedResults);
		}

		public void TestExportCustomsClearanceStatus()
		{
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "%%%", "Code 1", "HLD", RefCusCodeListTypes.Codes.ExportCustomsStatus);
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "$$$", "Code 2", "HLD", RefCusCodeListTypes.Codes.ExportCustomsStatus);
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "&&&", "Code 3", "HLD", RefCusCodeListTypes.Codes.ExportCustomsStatus);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment1.HVC_ConsignmentId = "CONSIGN1";
				consignment1.HVC_ExportCustomsClearanceStatus = "&&&";

				var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment2.HVC_ConsignmentId = "CONSIGN2";
				consignment2.HVC_ExportCustomsClearanceStatus = "%%%";

				var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment3.HVC_ConsignmentId = "CONSIGN3";
				consignment3.HVC_ExportCustomsClearanceStatus = "$$$";

				Factory.Save();

				var filterBizO = GetNewFilterStripBusinessObject();
				var filter = (ModuleTextFilter)filterBizO["Export Customs Clearance Status"];
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "&&&";
				filter.IsActive = true;

				var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { consignment1 }, results);
			}
		}

		public void TestImportCustomsClearanceStatus()
		{
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "%%%", "Code 1", "HLD");
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "$$$", "Code 2", "HLD");
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "&&&", "Code 3", "HLD");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment1.HVC_ConsignmentId = "CONSIGN1";
				consignment1.HVC_ImportCustomsClearanceStatus = "&&&";

				var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment2.HVC_ConsignmentId = "CONSIGN2";
				consignment2.HVC_ImportCustomsClearanceStatus = "%%%";

				var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment3.HVC_ConsignmentId = "CONSIGN3";
				consignment3.HVC_ImportCustomsClearanceStatus = "$$$";

				Factory.Save();

				var filterBizO = GetNewFilterStripBusinessObject();
				var filter = (ModuleTextFilter)filterBizO["Import Customs Clearance Status"];
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "&&&";
				filter.IsActive = true;

				var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { consignment1 }, results);
			}
		}

		public void TestReleaseStatus_HasReleaseStatusCodesAsOption()
		{
			AssertStatus_HasReleaseStatusCodesAsOption("Release Status");
		}

		public void TestReleaseStatus()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ReleaseStatus = HVLVReleaseStatus.Cleared;

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ReleaseStatus = HVLVReleaseStatus.Held;

			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment3.HVC_ConsignmentId = "CONSIGN3";
			consignment3.HVC_ReleaseStatus = HVLVReleaseStatus.None;

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Release Status"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = HVLVReleaseStatus.Held;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestImportReleaseStatus_HasReleaseStatusCodesAsOption()
		{
			AssertStatus_HasReleaseStatusCodesAsOption("Import Release Status");
		}

		public void TestImportReleaseStatus()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ImportReleaseStatus = HVLVReleaseStatus.Cleared;

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ImportReleaseStatus = HVLVReleaseStatus.Held;

			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment3.HVC_ConsignmentId = "CONSIGN3";
			consignment3.HVC_ImportReleaseStatus = HVLVReleaseStatus.None;

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Import Release Status"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = HVLVReleaseStatus.Held;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestExportReleaseStatus_HasReleaseStatusCodesAsOption()
		{
			AssertStatus_HasReleaseStatusCodesAsOption("Export Release Status");
		}

		public void TestExportReleaseStatus()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ExportReleaseStatus = HVLVReleaseStatus.Cleared;

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ExportReleaseStatus = HVLVReleaseStatus.Held;

			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment3.HVC_ConsignmentId = "CONSIGN3";
			consignment3.HVC_ExportReleaseStatus = HVLVReleaseStatus.None;

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Export Release Status"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = HVLVReleaseStatus.Held;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestItemCarrierBookingStatus_HasBookingStatusCodesAsOption()
		{
			var bookingStatusCodeList = HVLVItemLookups.GetAllHVLVItemCarrierBookingStatuses();
			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Item Carrier Booking Status"];

			AssertEquals($"Release status filter should have {bookingStatusCodeList.Count} options.", bookingStatusCodeList.Count, filter.List.Count);
			AssertContainsExactElementsInAnyOrder("Elements should be same with code list.", bookingStatusCodeList.GetAllCodes(), ((CodeDescriptionPairList)filter.List).GetAllCodes());
		}

		public void TestItemCarrierBookingStatus()
		{
			var consignmentNotBooked = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentNotBooked.HVC_ConsignmentId = "CONSIGN1";
			var item1 = consignmentNotBooked.Items.AddNew();
			item1.HVI_CarrierBookingStatus = HVLVItemCarrierBookingStatus.Codes.NotBooked;

			var consignmentRequested = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentRequested.HVC_ConsignmentId = "CONSIGN2";
			var item2 = consignmentRequested.Items.AddNew();
			item2.HVI_CarrierBookingStatus = HVLVItemCarrierBookingStatus.Codes.BookingRequested;

			var consignmentConfirmed = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentConfirmed.HVC_ConsignmentId = "CONSIGN3";
			var item3 = consignmentConfirmed.Items.AddNew();
			item3.HVI_CarrierBookingStatus = HVLVItemCarrierBookingStatus.Codes.BookingConfirmed;

			var consignmentRejected = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentRejected.HVC_ConsignmentId = "CONSIGN4";
			var item4 = consignmentRejected.Items.AddNew();
			item4.HVI_CarrierBookingStatus = HVLVItemCarrierBookingStatus.Codes.BookingRejected;

			var consignmentCancelled = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentCancelled.HVC_ConsignmentId = "CONSIGN5";
			var item5 = consignmentCancelled.Items.AddNew();
			item5.HVI_CarrierBookingStatus = HVLVItemCarrierBookingStatus.Codes.BookingCancelled;

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Item Carrier Booking Status"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = HVLVItemCarrierBookingStatus.Codes.NotBooked;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Expected consignmentNotBooked to be found by the filter.", new[] { consignmentNotBooked }, results);

			filter.Property = HVLVItemCarrierBookingStatus.Codes.BookingRequested;

			results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Expected consignmentRequested to be found by the filter.", new[] { consignmentRequested }, results);

			filter.Property = HVLVItemCarrierBookingStatus.Codes.BookingConfirmed;

			results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Expected consignmentConfirmed to be found by the filter.", new[] { consignmentConfirmed }, results);

			filter.Property = HVLVItemCarrierBookingStatus.Codes.BookingRejected;

			results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Expected consignmentRejected to be found by the filter.", new[] { consignmentRejected }, results);

			filter.Property = HVLVItemCarrierBookingStatus.Codes.BookingCancelled;

			results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Expected consignmentCancelled to be found by the filter.", new[] { consignmentCancelled }, results);
		}

		void AssertStatus_HasReleaseStatusCodesAsOption(string filterPropName)
		{
			var releaseStatusCodeList = HVLVReleaseStatus.GetAll();
			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO[filterPropName];

			AssertEquals($"Release status filter should have {releaseStatusCodeList.Count} options.", releaseStatusCodeList.Count, filter.List.Count);
			AssertContainsExactElementsInAnyOrder("Elements should be same with code list.", releaseStatusCodeList.GetAllCodes(), ((CodeDescriptionPairList)filter.List).GetAllCodes());
		}

		#endregion

		#region Locations

		public void TestConsigneeCity()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ConsigneeCity = "Eichenwalde";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ConsigneeCity = "Hanamura";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Consignee City"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "HANAMURA";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestConsigneeState()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ConsigneeState = "QLD";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ConsigneeState = "NSW";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Consignee State"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "NSW";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestConsigneePostcode()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ConsigneePostcode = "3000";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ConsigneePostcode = "2000";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Consignee Postcode"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "2000";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestConsigneeCountry()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_RN_NKConsigneeCountryCode = "AU";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_RN_NKConsigneeCountryCode = "ZA";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleNkFilter)filterBizO["Consignee Country"];
			filter.Property = "ZA";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestShipperCity()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ShipperCity = "Eichenwalde";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ShipperCity = "Hanamura";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Shipper City"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "HANAMURA";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestShipperState()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ShipperState = "QLD";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ShipperState = "NSW";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Shipper State"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "NSW";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestShipperPostcode()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ShipperPostcode = "3000";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ShipperPostcode = "2000";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Shipper Postcode"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "2000";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestShipperCountry()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_RN_NKShipperCountryCode = "AU";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_RN_NKShipperCountryCode = "ZA";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleNkFilter)filterBizO["Shipper Country"];
			filter.Property = "ZA";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestReturnCity()
		{
			var consignmentSydney = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentSydney.HVC_ConsignmentId = "CONSIGN1";
			consignmentSydney.HVC_ReturnCity = "Sydney";

			var consignmentMelbourne = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentMelbourne.HVC_ConsignmentId = "CONSIGN2";
			consignmentMelbourne.HVC_ReturnCity = "Melbourne";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Return City"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "Melbourne";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignmentMelbourne }, results);
		}

		public void TestReturnState()
		{
			var consignmentNSW = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentNSW.HVC_ConsignmentId = "CONSIGN1";
			consignmentNSW.HVC_ReturnState = "NSW";

			var consignmentVIC = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentVIC.HVC_ConsignmentId = "CONSIGN2";
			consignmentVIC.HVC_ReturnState = "VIC";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Return State"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "VIC";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignmentVIC }, results);
		}

		public void TestReturnPostCode()
		{
			var consignment2032 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2032.HVC_ConsignmentId = "CONSIGN1";
			consignment2032.HVC_ReturnPostcode = "2032";

			var consignment2000 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2000.HVC_ConsignmentId = "CONSIGN2";
			consignment2000.HVC_ReturnPostcode = "2000";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Return Postcode"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "2000";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2000 }, results);
		}

		public void TestReturnCountry()
		{
			var consignmentAU = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentAU.HVC_ConsignmentId = "CONSIGN1";
			consignmentAU.HVC_RN_NKReturnCountryCode = "AU";

			var consignmentUS = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentUS.HVC_ConsignmentId = "CONSIGN2";
			consignmentUS.HVC_RN_NKReturnCountryCode = "US";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleNkFilter)filterBizO["Return Country"];
			filter.Property = "US";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignmentUS }, results);
		}

		#endregion

		#region Organisations and Staff

		public void TestDestinationDepot()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";

			Factory.Save();

			consignment1.HVC_OA_DestinationDepot = org1.MainAddress.PK;
			consignment2.HVC_OA_DestinationDepot = org2.MainAddress.PK;
			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO["Destination Depot"];
			filter.Property = org2.PK;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestConsignee()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ConsigneeName = "FRANK";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ConsigneeName = "BARRY WONG";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Consignee"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "BARRY WONG";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestShipper()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ShipperName = "FRANK";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ShipperName = "BARRY WONG";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Shipper"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "BARRY WONG";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestReturnName()
		{
			var consignmentBone = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentBone.HVC_ConsignmentId = "CONSIGN1";
			consignmentBone.HVC_ReturnName = "Bone";

			var consignmentJustin = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentJustin.HVC_ConsignmentId = "CONSIGN2";
			consignmentJustin.HVC_ReturnName = "Justin";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Return Name"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "Justin";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignmentJustin }, results);
		}

		public void TestLastMileCarrier()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";

			Factory.Save();

			consignment1.HVC_OH_LastMileCarrier = org1.PK;
			consignment2.HVC_OH_LastMileCarrier = org2.PK;
			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO["Last Mile Carrier"];
			filter.Property = org2.PK;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		#endregion

		#region Modes and Types

		public void TestLastMileCarrierServiceLevel()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";

			Factory.Save();

			consignment1.HVC_PL_NKLastMileCarrierServiceLevel = "STD";
			consignment2.HVC_PL_NKLastMileCarrierServiceLevel = "D2D";
			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Last Mile Carrier Service Level"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "D2D";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		#endregion

		#region Related Shipments

		public void TestRelatedShipmentsFilter()
		{
			var filterBizO = GetNewFilterStripBusinessObject();

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();

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

			var consignmentResults = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consignment1 }, consignmentResults);

			relatedShipmentsFilter.Property = shipment1_2.PK;

			consignmentResults = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, consignmentResults);

			relatedShipmentsFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.FiltersMatch;
			relatedShipmentsFilter.SelectedFilters.AddTextFilterStrip("Transport Mode", TransportModes.Sea).SqlComparisonOperator = SQLComparisonOperator.Equal;
			relatedShipmentsFilter.SelectedFilters.AddTextFilterStrip("House Bill", "HB2").SqlComparisonOperator = SQLComparisonOperator.Equal;

			consignmentResults = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consignment2, consignment3 }, consignmentResults);
		}

		#endregion

		#region Related Brokerage Jobs

		public void TestRelatedBrokerageJobsFilter()
		{
			var filterBizO = GetNewFilterStripBusinessObject();

			var consignmentWithExportDeclaration = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignmentWithImportDeclaration = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignmentWithNoDeclaration = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignmentWithBothExportImportDeclaration = Factory.NewWithValidTestData<HVLVConsignment>();

			var exportDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var importDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			consignmentWithExportDeclaration.HVC_JE_ExportDeclaration = exportDeclaration.PK;
			consignmentWithImportDeclaration.HVC_JE_ImportDeclaration = importDeclaration.PK;
			consignmentWithNoDeclaration.HVC_JE_ExportDeclaration = ZGuid.Empty;

			var anotherExportDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var anotherImportDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			consignmentWithBothExportImportDeclaration.HVC_JE_ExportDeclaration = anotherExportDeclaration.PK;
			consignmentWithBothExportImportDeclaration.HVC_JE_ImportDeclaration = anotherImportDeclaration.PK;

			Factory.Save();

			var relatedBrokerageJobsFilter = (ModuleGuidFilter)filterBizO["Related Brokerage Jobs"];
			relatedBrokerageJobsFilter.IsActive = true;
			relatedBrokerageJobsFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			relatedBrokerageJobsFilter.Property = exportDeclaration.PK;

			var consignmentResults = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should be able to filter on HVC_JE_ExportDeclaration", new[] { consignmentWithExportDeclaration }, consignmentResults);

			relatedBrokerageJobsFilter.Property = importDeclaration.PK;
			consignmentResults = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should be able to filter on HVC_JE_ImportDeclaration", new[] { consignmentWithImportDeclaration }, consignmentResults);

			relatedBrokerageJobsFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			consignmentResults = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(
				"Should be able to filter out consignments with both HVC_JE_ExportDeclaration and HVC_JE_ImportDeclaration not equal to importDeclaration.PK",
				new[] { consignmentWithBothExportImportDeclaration, consignmentWithNoDeclaration, consignmentWithExportDeclaration }, consignmentResults);

			relatedBrokerageJobsFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsBlank;
			consignmentResults = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should be able to filter both HVC_JE_ImportDeclaration and HVC_JE_ExportDeclaration are blank", new[] { consignmentWithNoDeclaration }, consignmentResults);

			relatedBrokerageJobsFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsNotBlank;
			consignmentResults = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(
				"Should be able to filter either HVC_JE_ImportDeclaration or HVC_JE_ExportDeclaration is not blank",
				new[] { consignmentWithExportDeclaration, consignmentWithImportDeclaration, consignmentWithBothExportImportDeclaration }, consignmentResults);

			exportDeclaration.JE_SystemCreateTimeUtc = DateTime.UtcNow.AddMonths(-1);
			importDeclaration.JE_SystemCreateTimeUtc = DateTime.UtcNow.AddMonths(-2);
			anotherExportDeclaration.JE_SystemCreateTimeUtc = DateTime.UtcNow.AddMonths(-5);
			anotherImportDeclaration.JE_SystemCreateTimeUtc = DateTime.UtcNow.AddMonths(1);
			Factory.Save();

			relatedBrokerageJobsFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.FiltersMatch;
			var addedDateFilter = relatedBrokerageJobsFilter.SelectedFilters.AddDateFilterStrip("Created Time");
			addedDateFilter.Property1 = ZDateTime.UtcNow.AddMonths(-3);
			addedDateFilter.Property2 = ZDateTime.UtcNow;

			consignmentResults = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consignmentWithExportDeclaration, consignmentWithImportDeclaration }, consignmentResults);
		}

		#endregion

		#region Shipments

		public void TestShipmentsFilter()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();
			var header1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			var header2 = shipment2.GetOrCreateHVLVConsignmentHeader();
			var consignment1 = header1.Consignments.AddNew();
			header2.Consignments.AddNew();
			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var relatedShipmentsFilter = (ModuleGuidFilter)filterBizO["Shipments"];
			relatedShipmentsFilter.IsActive = true;
			relatedShipmentsFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			relatedShipmentsFilter.Property = shipment1.PK;

			var consignmentResults = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consignment1 }, consignmentResults);
		}

		#endregion

		#region Related HVLVOriginLoadList

		public void TestRelatedOriginLoadListFilter()
		{
			var filterBizO = GetNewFilterStripBusinessObject();

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();

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

			var consignmentResults = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consignment1 }, consignmentResults);

			relatedLoadListsFilter.Property = loadList1_2.PK;

			consignmentResults = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, consignmentResults);

			relatedLoadListsFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.FiltersMatch;
			relatedLoadListsFilter.SelectedFilters.AddTextFilterStrip("Transport Mode", TransportModes.Sea).SqlComparisonOperator = SQLComparisonOperator.Equal;
			relatedLoadListsFilter.SelectedFilters.AddTextFilterStrip("House Bill #", "HB2").SqlComparisonOperator = SQLComparisonOperator.Equal;

			consignmentResults = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consignment2, consignment3 }, consignmentResults);
		}
		#endregion

		#region Other

		public void TestGoodsDescription()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_GoodsDescription = "BLAH";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_GoodsDescription = "HAY WHO FROO DAT";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Goods Description"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "HAY WHO FROO DAT";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestIsPerishable()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_IsPerishable = false;

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_IsPerishable = true;

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Customs Indicators"];
			filter.Property0 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestIsHazardous()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_IsHazardous = false;

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_IsHazardous = true;

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Customs Indicators"];
			filter.Property1 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestIsPersonalEffects()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_IsPersonalEffects = false;

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_IsPersonalEffects = true;

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Customs Indicators"];
			filter.Property2 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestIsTimber()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_IsTimber = false;

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_IsTimber = true;

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Customs Indicators"];
			filter.Property3 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestRequiresFumigation()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_RequiresFumigation = false;

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_RequiresFumigation = true;

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Customs Indicators"];
			filter.Property4 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestUNDGClass()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_UndgClass = "1.1";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_UndgClass = "1.2";

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["UNDG Class"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "1.2";
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestIsSignatureRequired()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_IsSignatureRequired = false;

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_IsSignatureRequired = true;

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Signature Required"];
			filter.Property0 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestIsSelfBooked()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_IsSelfBooked = false;

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_IsSelfBooked = true;

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Self-booked"];
			filter.Property0 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestAuthorityToLeave()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_AuthorityToLeave = false;

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_AuthorityToLeave = true;

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Authority to Leave"];
			filter.Property0 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestReferencesValidated_Consignment()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_IsValidatedForUniqueness = false;

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_IsValidatedForUniqueness = true;

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Consignment References Validated"];
			filter.Property0 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		public void TestReferencesValidated_Item()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var item1 = consignment1.Items.AddNew();
			item1.HVI_ItemId = "ITEM1";
			item1.HVI_IsValidatedForUniqueness = false;

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var item2 = consignment2.Items.AddNew();
			item2.HVI_ItemId = "ITEM2";
			item2.HVI_IsValidatedForUniqueness = true;

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Item References Validated"];
			filter.Property0 = true;
			filter.IsActive = true;

			var results = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, results);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new HVLVConsignmentFilterBusinessObject();

		#endregion
	}
}
