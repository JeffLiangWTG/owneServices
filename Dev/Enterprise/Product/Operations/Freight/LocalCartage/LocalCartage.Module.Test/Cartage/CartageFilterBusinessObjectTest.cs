using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Freight.Integration.CFS;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	[TestedType(typeof(CartageFilterBusinessObject))]
	public class CartageFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilterMaxLength()
		{
			CombineAssertions(() =>
			{
				AssertEquals("MaxLength of Additional Reference # should be set correctly.", CusEntryNumSchema.CE_EntryNum.MaxLength, FilterStripBizO[NumberFilterTypes.PortTransportAdditionalReference].MaxLength);
				AssertEquals("MaxLength of Container # should be set correctly.", ModuleNumberFilter.MultiplyMaxLength(JobContainerSchema.JC_ContainerNum.MaxLength), FilterStripBizO[NumberFilterTypes.ContainerNumber].MaxLength);
				AssertEquals("MaxLength of Vessel and Flight/Voyage # should be set correctly.", ViewLocalTransportScheduleSchema.VLT_Voyage.MaxLength, FilterStripBizO[CartageFilterBusinessObject.FilterNameConstants.VoyageVessel].MaxLength);
				AssertEquals("NkMaxLength of Vessel and Flight/Voyage # should be set correctly.", ViewLocalTransportScheduleSchema.VLT_RV_NKVessel.MaxLength, ((ModuleTextAndNkFilter)FilterStripBizO[CartageFilterBusinessObject.FilterNameConstants.VoyageVessel]).NkMaxLength);
				AssertEquals("MaxLength of Job # should be set correctly.", JobCartageSchema.JJ_ConsignmentID.MaxLength, FilterStripBizO[NumberFilterTypes.JobNumber].MaxLength);
				AssertEquals("MaxLength of Quote # should be set correctly.", ModuleNumberFilter.MultiplyMaxLength(JobCartageSchema.JJ_QuoteNumber.MaxLength), FilterStripBizO[NumberFilterTypes.QuoteNumber].MaxLength);
				AssertEquals("MaxLength of Ref # should be set correctly.", ModuleNumberFilter.MultiplyMaxLength(JobCartageSchema.JJ_OrderReferenceNumber.MaxLength), FilterStripBizO[NumberFilterTypes.OrderNumber].MaxLength);
				AssertEquals("MaxLength of Waybill # should be set correctly.", ModuleNumberFilter.MultiplyMaxLength(JobCartageSchema.JJ_WaybillNumber.MaxLength), FilterStripBizO[NumberFilterTypes.WaybillNumber].MaxLength);
				AssertEquals("MaxLength of Related Port should be set correctly.", Math.Min(OrgAddressSchema.OA_RL_NKRelatedPortCode.MaxLength, OrgHeaderSchema.OH_RL_NKClosestPort.MaxLength), FilterStripBizO["Related Port"].MaxLength);
			});
		}

		public void TestReferenceNumberFilter()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = branch.PK;
			var cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_GB = branch.PK;
			var cartage3 = Factory.New<CommonCartage>();
			cartage3.JJ_GB = branch.PK;
			NewReferenceNumber(cartage1, "AU", WarehouseAdditionalReferenceTypes.Codes.TransportReference, "TRF1");
			NewReferenceNumber(cartage2, "US", WarehouseAdditionalReferenceTypes.Codes.TransportReference, "MTRF2");
			NewReferenceNumber(cartage3, "AU", WarehouseAdditionalReferenceTypes.Codes.TransportReference, "MTRF2");
			NewReferenceNumber(cartage1, "AU", WarehouseAdditionalReferenceTypes.Codes.HouseBill, "HSB1");
			NewReferenceNumber(cartage2, "AU", WarehouseAdditionalReferenceTypes.Codes.HouseBill, "HSB2");
			NewReferenceNumber(cartage1, "AU", WarehouseAdditionalReferenceTypes.Codes.MasterBill, "MSB1");
			NewReferenceNumber(cartage2, "AU", WarehouseAdditionalReferenceTypes.Codes.MasterBill, "");
			Factory.Save();
			Asserter.AddToScope(cartage1);
			Asserter.AddToScope(cartage2);
			Asserter.AddToScope(cartage3);
			Asserter.AddFieldOfInterest("AU:" + WarehouseAdditionalReferenceTypes.Codes.TransportReference, (b) => GetValue(b, "AU", WarehouseAdditionalReferenceTypes.Codes.TransportReference));
			Asserter.AddFieldOfInterest("US:" + WarehouseAdditionalReferenceTypes.Codes.TransportReference, (b) => GetValue(b, "US", WarehouseAdditionalReferenceTypes.Codes.TransportReference));
			Asserter.AddFieldOfInterest("AU:" + WarehouseAdditionalReferenceTypes.Codes.HouseBill, (b) => GetValue(b, "AU", WarehouseAdditionalReferenceTypes.Codes.HouseBill));
			Asserter.AddFieldOfInterest("AU:" + WarehouseAdditionalReferenceTypes.Codes.MasterBill, (b) => GetValue(b, "AU", WarehouseAdditionalReferenceTypes.Codes.MasterBill));
			var filterBizO = new CartageFilterBusinessObject();
			var filter = (ReferenceNumberFilter)filterBizO[NumberFilterTypes.PortTransportAdditionalReference];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("empty", filter, cartage1, cartage2, cartage3);
			SetFilter(filter, "AU", WarehouseAdditionalReferenceTypes.Codes.TransportReference, "TRF1");
			Asserter.AssertMatches("AU:" + WarehouseAdditionalReferenceTypes.Codes.TransportReference + ":TRF1*", filter, cartage1);
			SetFilter(filter, "", WarehouseAdditionalReferenceTypes.Codes.TransportReference, "MTRF2");
			Asserter.AssertMatches(WarehouseAdditionalReferenceTypes.Codes.TransportReference + ":MTRF2*", filter, cartage2, cartage3);
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			SetFilter(filter, "", WarehouseAdditionalReferenceTypes.Codes.MasterBill, "");
			Asserter.AssertMatches("Without " + WarehouseAdditionalReferenceTypes.Codes.MasterBill, filter, cartage2, cartage3);
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			SetFilter(filter, "", WarehouseAdditionalReferenceTypes.Codes.MasterBill, "");
			Asserter.AssertMatches("With " + WarehouseAdditionalReferenceTypes.Codes.MasterBill, filter, cartage1);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			SetFilter(filter, "", WarehouseAdditionalReferenceTypes.Codes.TransportReference, "TRF1");
			Asserter.AssertMatches("Without " + WarehouseAdditionalReferenceTypes.Codes.TransportReference + ":TRF1", filter, cartage2, cartage3);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			SetFilter(filter, "", WarehouseAdditionalReferenceTypes.Codes.TransportReference, "MTRF2");
			Asserter.AssertMatches("Without " + WarehouseAdditionalReferenceTypes.Codes.TransportReference + ":*Test", filter, cartage1);
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			SetFilter(filter, "", WarehouseAdditionalReferenceTypes.Codes.TransportReference, "M");
			Asserter.AssertMatches("Without " + WarehouseAdditionalReferenceTypes.Codes.TransportReference + ":M*", filter, cartage1);
		}

		void NewReferenceNumber(CommonCartage cartage, string countryCode, string type, string number)
		{
			var result = cartage.AdditionalReferenceNumbers.AddNew();
			result.CE_RN_NKCountryCode = countryCode;
			result.CE_EntryType = type;
			result.CE_EntryNum = number;
		}

		string GetValue(CommonCartage cartage, string country, string type)
		{
			foreach (ICusEntryNumber number in cartage.AdditionalReferenceNumbers)
			{
				if (number.CE_RN_NKCountryCode == country && number.CE_EntryType == type)
				{
					return number.CE_EntryNum;
				}
			}

			return null;
		}

		void SetFilter(ReferenceNumberFilter filter, string country, string type, string number)
		{
			filter.Country = country;
			filter.Type = type;
			filter.Property = number;
		}

		public void TestOrderNumberQuery()
		{
			TestCartageTextFilter(NumberFilterTypes.OrderNumber, JobCartageSchema.JJ_OrderReferenceNumber);
		}

		public void TestQuoteNumberQuery()
		{
			TestCartageTextFilter(NumberFilterTypes.QuoteNumber, JobCartageSchema.JJ_QuoteNumber);
		}

		public void TestWaybillNumberQuery()
		{
			TestCartageTextFilter(NumberFilterTypes.WaybillNumber, JobCartageSchema.JJ_WaybillNumber);
		}

		public void TestContainerNumberQuery()
		{
			TestContainerTextFilter(NumberFilterTypes.ContainerNumber, JobContainerSchema.JC_ContainerNum);
		}

		public void TestVoyageVesselFilter()
		{
			var vessel3 = Factory.New<RefVessel>();
			vessel3.RV_Name = "Dancing in the moonlight";
			var vessel4 = Factory.New<RefVessel>();
			vessel4.RV_Name = "Black Beauty";
			var sailing1 = Helper.CreateSailing(Helper.TestVessel1, "111", LocalCartageTestHelper.HomePort, LocalCartageTestHelper.OverseasPort, ZDateTime.Today);
			var sailing2 = Helper.CreateSailing(Helper.TestVessel1, "222", LocalCartageTestHelper.HomePort, LocalCartageTestHelper.OverseasPort, ZDateTime.Today);
			var sailing3 = Helper.CreateSailing(Helper.TestVessel1, "11122", LocalCartageTestHelper.HomePort, LocalCartageTestHelper.OverseasPort, ZDateTime.Today);
			var sailing4 = Helper.CreateSailing(vessel3, "111", LocalCartageTestHelper.HomePort, LocalCartageTestHelper.OverseasPort, ZDateTime.Today);
			var sailing5 = Helper.CreateSailing(vessel4, "222", LocalCartageTestHelper.HomePort, LocalCartageTestHelper.OverseasPort, ZDateTime.Today);
			var cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_JX_Sailing = sailing1.PK;
			Asserter.AddToScope(cartage1);
			var cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_JX_Sailing = sailing2.PK;
			Asserter.AddToScope(cartage2);
			var cartage3 = Factory.New<CommonCartage>();
			cartage3.JJ_JX_Sailing = sailing3.PK;
			Asserter.AddToScope(cartage3);
			var cartage4 = Factory.New<CommonCartage>();
			cartage4.JJ_JX_Sailing = sailing4.PK;
			Asserter.AddToScope(cartage4);
			var cartage5 = Factory.New<CommonCartage>();
			cartage5.JJ_JX_Sailing = sailing5.PK;
			Asserter.AddToScope(cartage5);
			Factory.Save();
			var filters = new CartageFilterBusinessObject();
			var filter = (ModuleTextAndNkFilter)filters[CartageFilterBusinessObject.FilterNameConstants.VoyageVessel];
			AssertEquals("ScheduleFilterSubGroup", filter.SubGroup.GetType().Name);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "111";
			Asserter.AssertMatches("voyage equals 111", filter, cartage1, cartage4);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("voyage starts with 111", filter, cartage1, cartage3, cartage4);
			filter.NkProperty = Helper.TestVessel1.RV_Name;
			Asserter.AssertMatches("test vessel1 code", filter, cartage1, cartage3);
			filter.Property = "";
			Asserter.AssertMatches("Empty", filter, cartage1, cartage2, cartage3);
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "";
			filter.NkProperty = "the";
			Asserter.AssertMatches("vessel contains the", filter, cartage4);
			filter.NkProperty = "eau";
			Asserter.AssertMatches("vessel contains eau", filter, cartage5);
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			filter.Property = "";
			filter.NkProperty = "";
			Asserter.AssertMatches("isnotblank test", filter, cartage1, cartage2, cartage3, cartage4, cartage5);
		}

		public void TestVesselVoyageFilter_CaptionAndDescription()
		{
			var filters = new CartageFilterBusinessObject();
			var filter = (ModuleTextAndNkFilter)filters[CartageFilterBusinessObject.FilterNameConstants.VoyageVessel];
			AssertEquals("Vessel and Flight/Voyage #", filter.Description);
			AssertEquals("Vessel and Flight/Voyage #", filter.MultilingualDescription);
		}

		public void TestCompletionQuery()
		{
			TestCartageDateFilter(CartageFilterBusinessObject.DateFilterTypes.CPD, JobCartageSchema.JJ_A_JCL);
		}

		public void TestActualPickupQuery_In()
		{
			TestCartageLegDateFilter(CartageFilterBusinessObject.DateFilterTypes.APD, JobContainerLegsSchema.JU_PickupTimeIn);
		}

		public void TestActualPickupQuery_Out()
		{
			TestCartageLegDateFilter(CartageFilterBusinessObject.DateFilterTypes.APD, JobContainerLegsSchema.JU_PickupTimeOut);
		}

		public void TestActualDeliveryQuery_In()
		{
			TestCartageLegDateFilter(CartageFilterBusinessObject.DateFilterTypes.ADD, JobContainerLegsSchema.JU_DeliverTimeIn);
		}

		public void TestActualDeliveryQuery_Out()
		{
			TestCartageLegDateFilter(CartageFilterBusinessObject.DateFilterTypes.ADD, JobContainerLegsSchema.JU_DeliverTimeOut);
		}

		public void TestEstimatedPickupQuery()
		{
			TestCartageDateFilter(CartageFilterBusinessObject.DateFilterTypes.EPD, JobCartageSchema.JJ_EstimatedPickup);
		}

		public void TestEstimatedDeliveryQuery()
		{
			TestCartageDateFilter(CartageFilterBusinessObject.DateFilterTypes.EDD, JobCartageSchema.JJ_EstimatedDelivery);
		}

		public void TestSailingDates()
		{
			TestSailingDateFilter(CartageFilterBusinessObject.DateFilterTypes.ScheduleETA, JobVoyDestinationSchema.JB_E_ARV);
			TestSailingDateFilter(CartageFilterBusinessObject.DateFilterTypes.ScheduleATA, JobVoyDestinationSchema.JB_A_ARV);
			TestSailingDateFilter(CartageFilterBusinessObject.DateFilterTypes.ScheduleETD, JobVoyOriginSchema.JA_E_DEP);
			TestSailingDateFilter(CartageFilterBusinessObject.DateFilterTypes.ScheduleATD, JobVoyOriginSchema.JA_A_DEP);
			TestSailingDateFilter(CartageFilterBusinessObject.DateFilterTypes.ScheduleFCLAvailability, JobVoyDestinationSchema.JB_AvailabilityDate);
			TestSailingDateFilter(CartageFilterBusinessObject.DateFilterTypes.ScheduleFCLStorage, JobVoyDestinationSchema.JB_StorageDate);
			TestSailingDateFilter(CartageFilterBusinessObject.DateFilterTypes.ScheduleFCLCutOff, JobVoyOriginSchema.JA_CutOff);
			TestSailingDateFilter(CartageFilterBusinessObject.DateFilterTypes.ScheduleFCLReceivalCommences, JobVoyOriginSchema.JA_ReceivalCommences);
			TestSailingDateFilter(CartageFilterBusinessObject.DateFilterTypes.ScheduleLCLAvailability, JobSailingSchema.JX_DepotAvailabilityDate);
			TestSailingDateFilter(CartageFilterBusinessObject.DateFilterTypes.ScheduleLCLStorage, JobSailingSchema.JX_DepotStorageDate);
			TestSailingDateFilter(CartageFilterBusinessObject.DateFilterTypes.ScheduleLCLCutOff, JobSailingSchema.JX_DepotCutOff);
			TestSailingDateFilter(CartageFilterBusinessObject.DateFilterTypes.ScheduleLCLReceivalCommences, JobSailingSchema.JX_DepotReceivalCommences);
		}

		public void TestStaffDriver()
		{
			GlbStaff bob = Factory.New<GlbStaff>();
			bob.GS_Code = "BB1";
			bob.GS_LoginName = "Bob111";
			GlbStaff david = Factory.New<GlbStaff>();
			david.GS_Code = "DD2";
			david.GS_LoginName = "David222";
			TestWorkSheetNkFilter("Driver", JobCartageRunSheetSchema.EY_GS_NKTruckDriver, bob.GS_Code, david.GS_Code);
		}

		public void TestVehicle()
		{
			RefEquipment car = Factory.New<RefEquipment>();
			car.RQ_IsVehicle = true;
			car.RQ_ShortCode = "Car111";
			car.RQ_Registration = "111";
			RefEquipment truck = Factory.New<RefEquipment>();
			truck.RQ_IsVehicle = true;
			truck.RQ_ShortCode = "Truck222";
			truck.RQ_Registration = "222";
			TestWorkSheetGuidFilter("Vehicle", JobCartageRunSheetSchema.EY_RQ_Truck, car.PK, truck.PK);
		}

		public void TestClient()
		{
			var filter = (ModuleGuidFilter)FilterStripBizO["Client"];
			var org = Helper.CreateOrgHeader("TESTORG", "Address");
			Factory.Save();
			filter.Property = org.PK;
			AssertEquals(false, filter.HasErrors);
		}

		public void TestContainerService()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = branch.PK;
			CommonBookedCtgMove booked1 = cartage1.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg1 = booked1.CartageLegs.AddNew();
			CommonCartage cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_GB = branch.PK;
			CommonContainer container2 = cartage2.ContainerBookedMoves.AddNew().Container;
			CommonCartage cartage3 = Factory.New<CommonCartage>();
			cartage3.JJ_GB = branch.PK;
			CommonContainer container3 = cartage3.ContainerBookedMoves.AddNew().Container;
			CommonCartage cartage4 = Factory.New<CommonCartage>();
			cartage4.JJ_GB = branch.PK;
			CommonContainer container4 = cartage4.ContainerBookedMoves.AddNew().Container;
			JobService service1 = Factory.New<JobService>();
			JobService service2 = Factory.New<JobService>();
			JobService service3 = Factory.New<JobService>();
			service1.ES_ParentTableCode = JobContainerSchema.Constants.Prefix;
			service1.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			service1.ES_ParentID = container2.PK;
			service2.ES_ParentTableCode = JobContainerSchema.Constants.Prefix;
			service2.ES_ServiceCode = Constants.FreightServiceType.Codes.ExtraInspection;
			service2.ES_ParentID = container3.PK;
			service3.ES_ParentTableCode = JobScheduleChangeSchema.Constants.Prefix;
			service3.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			Factory.Save();
			CartageFilterBusinessObject filter = new CartageFilterBusinessObject();
			((ModuleTextFilter)filter["Container Service"]).Property = Constants.FreightServiceType.Codes.Cleaning;
			((ModuleTextFilter)filter["Container Service"]).IsActive = true;
			CommonCartageCollection collection = new CommonCartageCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionContains("Should have Cartage2", cartage2, collection);
			((ModuleTextFilter)filter["Container Service"]).Property = Constants.FreightServiceType.Codes.ExtraInspection;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionContains("Should have Cartage3", cartage3, collection);
			((ModuleTextFilter)filter["Container Service"]).Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ContainerServiceAny;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 2 CommonCartage", 2, collection.Count);
			AssertCollectionContains("Should have Cartage2", cartage2, collection);
			AssertCollectionContains("Should have Cartage3", cartage3, collection);
			((ModuleTextFilter)filter["Container Service"]).Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ContainerServiceNone;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 2 CommonCartage", 2, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartage1, collection);
			AssertCollectionContains("Should have Cartage4", cartage4, collection);
			((ModuleTextFilter)filter["Container Service"]).Property = "";
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 4 CommonCartage", 4, collection.Count);
		}

		public void TestDropMode()
		{
			TestBookedCtgMoveTextFilter("Drop Mode", JobBookedCtgMoveSchema.EW_DropMode);
		}

		public void TestCartageJobType()
		{
			TestCartageTextFilter("Port Transport Job Type", JobCartageSchema.JJ_E3_NKJobType);
		}

		public void TestCartageServiceLevel()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = branch.PK;
			CommonBookedCtgMove booked1 = cartage1.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg1 = booked1.CartageLegs.AddNew();
			CommonCartage cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_GB = branch.PK;
			CommonBookedCtgMove booked2 = cartage2.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg2 = booked2.CartageLegs.AddNew();
			cartage1.JJ_RS_NKServiceLevel = "abc";
			cartage2.JJ_RS_NKServiceLevel = "def";
			Factory.Save();
			CartageFilterBusinessObject filter = new CartageFilterBusinessObject();
			((ModuleNkFilter)filter["Service Level"]).Property = "def";
			((ModuleNkFilter)filter["Service Level"]).IsActive = true;
			CommonCartageCollection collection = new CommonCartageCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartage1, collection);
			AssertCollectionContains("Should have Cartage2", cartage2, collection);
			((ModuleNkFilter)filter["Service Level"]).Property = "abc";
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartage1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartage2, collection);
		}

		public void TestDangerousGoods()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = branch.PK;
			CommonBookedCtgMove booked1 = cartage1.LooseBookedMoves.AddNew();
			CommonCartage cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_GB = branch.PK;
			CommonBookedCtgMove booked2 = cartage2.LooseBookedMoves.AddNew();
			UNDGDataItem dangerousGood = Factory.New<UNDGDataItem>();
			dangerousGood.DI_ParentTableCode = JobBookedCtgMoveSchema.Constants.Prefix;
			dangerousGood.DI_ParentID = booked1.PK;
			Factory.Save();
			CartageFilterBusinessObject filter = new CartageFilterBusinessObject();
			CommonCartageCollection collection = new CommonCartageCollection(Factory);
			((ModuleTextFilter)filter["Dangerous Goods"]).IsActive = true;
			((ModuleTextFilter)filter["Dangerous Goods"]).Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.DangerousGoodsBoth;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 2 CommonCartage", 2, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartage1, collection);
			AssertCollectionContains("Should have Cartage2", cartage2, collection);
			((ModuleTextFilter)filter["Dangerous Goods"]).Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.DangerousGoodsHas;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartage1, collection);
			((ModuleTextFilter)filter["Dangerous Goods"]).Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.DangerousGoodsNone;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionContains("Should have Cartage2", cartage2, collection);
			((ModuleTextFilter)filter["Dangerous Goods"]).Property = "";
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 2, collection.Count);
		}

		public void TestMessageStatus()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			CommonCartage cartage1 = GetCartageWithLegMessageStatus(branch, Constants.CartageLegDispatchStatusList.Codes.Delivered);
			CommonCartage cartage4 = GetCartageWithLegMessageStatus(branch, Constants.CartageLegDispatchStatusList.Codes.Futile);
			CommonCartage cartage6 = GetCartageWithLegMessageStatus(branch, Constants.CartageLegDispatchStatusList.Codes.PickedUp);
			CommonCartage cartage7 = GetCartageWithLegMessageStatus(branch, Constants.CartageLegDispatchStatusList.Codes.Rejected);
			CommonCartage cartage8 = GetCartageWithLegMessageStatus(branch, Constants.CartageLegDispatchStatusList.Codes.Runsheet);
			Factory.Save();
			CartageFilterBusinessObject filter = new CartageFilterBusinessObject();
			CommonCartageCollection collection = new CommonCartageCollection(Factory);
			((ModuleTextFilter)filter["Message Status"]).IsActive = true;
			((ModuleTextFilter)filter["Message Status"]).Property = Constants.CartageLegDispatchStatusList.Codes.Delivered;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartage1, collection);
			((ModuleTextFilter)filter["Message Status"]).Property = Constants.CartageLegDispatchStatusList.Codes.Futile;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionContains("Should have Cartage4", cartage4, collection);
			((ModuleTextFilter)filter["Message Status"]).Property = Constants.CartageLegDispatchStatusList.Codes.PickedUp;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionContains("Should have Cartage6", cartage6, collection);
			((ModuleTextFilter)filter["Message Status"]).Property = Constants.CartageLegDispatchStatusList.Codes.Rejected;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionContains("Should have Cartage7", cartage7, collection);
			((ModuleTextFilter)filter["Message Status"]).Property = Constants.CartageLegDispatchStatusList.Codes.Runsheet;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionContains("Should have Cartage8", cartage8, collection);
			((ModuleTextFilter)filter["Message Status"]).Property = "";
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 5 CommonCartage", 5, collection.Count);
		}

		CommonCartage GetCartageWithLegMessageStatus(GlbBranch branch, ZString messageStatus)
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_GB = branch.PK;
			CommonBookedCtgMove booked = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg = booked.CartageLegs.AddNew();
			cartageLeg.JU_MessageStatus = messageStatus;
			return cartage;
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestStandaloneJobCartageChargesNotPostedFilter()
		{
			CommonCartage cartage1 = Factory.NewWithValidTestData<CommonCartage>();
			CommonCartage cartage2 = Factory.NewWithValidTestData<CommonCartage>();
			CommonCartage cartage3 = Factory.NewWithValidTestData<CommonCartage>();
			CommonCartage cartage4 = Factory.NewWithValidTestData<CommonCartage>();
			CheckJobCartageChargesNotPosted(cartage1, cartage2, cartage3, cartage4, cartage1, cartage2, cartage3, cartage4);
		}

		void CheckJobCartageChargesNotPosted(BusinessObject objectThatHasInvoicing1, BusinessObject objectThatHasInvoicing2, BusinessObject objectThatHasInvoicing3, BusinessObject objectThatHasInvoicing4, CommonCartage objectToFind1, CommonCartage objectToFind2, CommonCartage objectToFind3, CommonCartage objectToFind4)
		{
			ModuleFlagsFilter filter = (ModuleFlagsFilter)FilterStripBizO["Invoiced / Charges / Billing"];
			filter["Costs Not Posted"] = true;
			filter.IsActive = true;
			JobHeader job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job1.JH_ParentID = objectThatHasInvoicing1.PK;
			job1.JH_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(objectThatHasInvoicing1.TableName);
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			var line1 = Factory.NewWithValidTestData<AccTransactionLines>();
			line1.AL_JH = job1.PK;
			line1.AL_LineType = "ACR";
			line1.AL_ReverseDate = ZDateTime.Empty;
			line1.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_APLine = line1.PK;
			charge.JR_JH = job1.PK;
			JobHeader job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job2.JH_ParentID = objectThatHasInvoicing2.PK;
			job2.JH_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(objectThatHasInvoicing2.TableName);
			job2.JH_GC = GlbCompany.CurrentCompany.PK;
			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.Invoice;
			AccTransactionLines line2 = Factory.NewWithValidTestData<AccTransactionLines>();
			line2.AL_AH = header.PK;
			line2.AL_JH = job2.PK;
			line2.AL_LineType = "CST";
			line2.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_APLine = line2.PK;
			charge.JR_JH = job2.PK;
			JobHeader job3 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job3.JH_ParentID = objectThatHasInvoicing3.PK;
			job3.JH_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(objectThatHasInvoicing3.TableName);
			ZQuery companyFilter = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			var notCurrentCompany = Factory.LoadTop1<GlbCompany>(companyFilter);
			job3.JH_GC = notCurrentCompany.PK;
			JobHeader job4 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job4.JH_ParentID = objectThatHasInvoicing4.PK;
			job4.JH_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(objectThatHasInvoicing4.TableName);
			job4.JH_GC = GlbCompany.CurrentCompany.PK;
			AccTransactionHeader reversedHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			reversedHeader.AH_IsCancelled = ZBool.True;
			reversedHeader.AH_Ledger = LedgerTypes.AccountsPayable;
			reversedHeader.AH_TransactionType = TransactionTypes.Invoice;
			AccTransactionLines reversedLine = Factory.NewWithValidTestData<AccTransactionLines>();
			reversedLine.AL_AH = reversedHeader.PK;
			reversedLine.AL_JH = job4.PK;
			reversedLine.AL_LineType = "CST";
			reversedLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job4.PK;
			Factory.Save();
			CommonCartageCollection allJobs = new CommonCartageCollection(Factory);
			allJobs.AdditionalFilter = FilterStripBizO.Filter;
			AssertEquals("Collection should contain shipment 1", true, allJobs.Contains(objectToFind1));
			AssertEquals("Collection should NOT contain shipment 2", false, allJobs.Contains(objectToFind2));
			AssertEquals("Collection should NOT contain shipment 3", false, allJobs.Contains(objectToFind3));
			AssertEquals("Collection should contain shipment 4", true, allJobs.Contains(objectToFind4));
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestStandAloneJobCartageNotInvoicedFilter()
		{
			var cartage1 = Factory.NewWithValidTestData<CommonCartage>();
			var cartage2 = Factory.NewWithValidTestData<CommonCartage>();
			var cartage3 = Factory.NewWithValidTestData<CommonCartage>();
			var cartage4 = Factory.NewWithValidTestData<CommonCartage>();
			CheckJobCartageNotInvoicedFilter(cartage1, cartage2, cartage3, cartage4, cartage1, cartage2, cartage3, cartage4);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestInternalJobCartageNotInvoicedFilter()
		{
			var shipment1 = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var shipment2 = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var shipment3 = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var shipment4 = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S1";
			shipment2.JS_UniqueConsignRef = "S2";
			shipment3.JS_UniqueConsignRef = "S3";
			shipment4.JS_UniqueConsignRef = "S4";
			var cartage1 = Helper.CreateInternalCartageWithNoJobHeader(shipment1);
			var cartage2 = Helper.CreateInternalCartageWithNoJobHeader(shipment2);
			var cartage3 = Helper.CreateInternalCartageWithNoJobHeader(shipment3);
			var cartage4 = Helper.CreateInternalCartageWithNoJobHeader(shipment4);
			CheckJobCartageNotInvoicedFilter(cartage1, cartage2, cartage3, cartage4, cartage1, cartage2, cartage3, cartage4);
		}

		void CheckJobCartageNotInvoicedFilter(BusinessObject objectThatHasInvoicing1, BusinessObject objectThatHasInvoicing2, BusinessObject objectThatHasInvoicing3, BusinessObject objectThatHasInvoicing4, CommonCartage objectToFind1, CommonCartage objectToFind2, CommonCartage objectToFind3, CommonCartage objectToFind4)
		{
			ModuleFlagsFilter filter = (ModuleFlagsFilter)FilterStripBizO["Invoiced / Charges / Billing"];
			filter["Not Invoiced"] = true;
			filter.IsActive = true;
			JobHeader job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job1.JH_ParentID = objectThatHasInvoicing1.PK;
			job1.JH_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(objectThatHasInvoicing1.TableName);
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			AccTransactionLines line1 = Factory.NewWithValidTestData<AccTransactionLines>();
			line1.AL_JH = job1.PK;
			line1.AL_LineType = "WIP";
			line1.AL_ReverseDate = ZDateTime.Empty;
			line1.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_ARLine = line1.PK;
			charge.JR_JH = job1.PK;
			JobHeader job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job2.JH_ParentID = objectThatHasInvoicing2.PK;
			job2.JH_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(objectThatHasInvoicing2.TableName);
			job2.JH_GC = GlbCompany.CurrentCompany.PK;
			AccTransactionLines line2 = Factory.NewWithValidTestData<AccTransactionLines>();
			AccTransactionHeader headerForLine2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerForLine2.AH_Ledger = LedgerTypes.AccountsReceivable;
			headerForLine2.AH_TransactionType = TransactionTypes.Invoice;
			line2.AL_JH = job2.PK;
			line2.AL_LineType = "REV";
			line2.AL_AH = headerForLine2.PK;
			line2.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			headerForLine2.AH_GB = GlbBranch.CurrentBranch.PK;
			charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_ARLine = line2.PK;
			charge.JR_JH = job2.PK;
			JobHeader job3 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job3.JH_ParentID = objectThatHasInvoicing3.PK;
			job3.JH_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(objectThatHasInvoicing3.TableName);
			ZQuery companyFilter = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			GlbCompany notCurrentCompany = Factory.LoadTop1<GlbCompany>(companyFilter);
			job3.JH_GC = notCurrentCompany.PK;
			JobHeader job4 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job4.JH_ParentID = objectThatHasInvoicing4.PK;
			job4.JH_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(objectThatHasInvoicing4.TableName);
			job4.JH_GC = GlbCompany.CurrentCompany.PK;
			AccTransactionLines reversedLine = Factory.NewWithValidTestData<AccTransactionLines>();
			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = TransactionTypes.Invoice;
			reversedLine.AL_LineType = "REV";
			reversedLine.AL_JH = job4.PK;
			reversedLine.AL_AH = header.PK;
			reversedLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			header.AH_IsCancelled = ZBool.True;
			header.AH_GB = GlbBranch.CurrentBranch.PK;
			charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job4.PK;
			Factory.Save();
			CommonCartageCollection allJobs = new CommonCartageCollection(Factory);
			allJobs.AdditionalFilter = FilterStripBizO.Filter;
			AssertEquals("Collection should contain shipment 1", true, allJobs.Contains(objectToFind1));
			AssertEquals("Collection should NOT contain shipment 2", false, allJobs.Contains(objectToFind2));
			AssertEquals("Collection should NOT contain shipment 3", false, allJobs.Contains(objectToFind3));
			AssertEquals("Collection should contain 4th shipment because rev is cancelled", true, allJobs.Contains(objectToFind4));
		}

		public void TestStandAloneJobCartageLocalBillingNotPaidFilter()
		{
			string transactionNum = "123";
			string jobNum = "ABC";
			string china = Enterprise.Core.Constants.CountryCodes.China;
			GlbCompany.CurrentCompany.SetCountry(china);
			CommonCartage cartage = Factory.New<CommonCartage>();
			OrgHeader organisation = GetJCOrganisationByPort(china);
			JobHeader job = GetJCJobHeader(jobNum);
			Factory.Save();
			CommonCartageCollection allJobs = new CommonCartageCollection(Factory);
			ModuleFlagsFilter filter = (ModuleFlagsFilter)FilterStripBizO["Invoiced / Charges / Billing"];
			filter["Local Billing Not Paid"] = true;
			filter.IsActive = true;
			allJobs.AdditionalFilter = FilterStripBizO.Filter;
			AssertEquals("There should be no jobs found", 0, allJobs.Count);
			AccTransactionHeader transaction = GetJCTransactionHeader(ZArchitecture.Core.TransactionTypes.Invoice, ZArchitecture.Core.LedgerTypes.AccountsReceivable, transactionNum);
			job.JH_ParentID = cartage.PK;
			job.JH_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(cartage.TableName);
			transaction.AH_OH = organisation.PK;
			transaction.AH_JH = job.PK;
			transaction.AH_OutstandingAmount = 100m;
			transaction.AH_InvoiceAmount = 100m;
			Factory.Save();
			allJobs.AdditionalFilter = FilterStripBizO.Filter;
			AssertEquals("There should only be 1 job found", 1, allJobs.Count);
			CreateReceiptAndPayARInvoice(transaction);
			Factory.Save();
			allJobs.AdditionalFilter = FilterStripBizO.Filter;
			AssertEquals("There should be no jobs found", 0, allJobs.Count);
		}

		void CreateReceiptAndPayARInvoice(AccTransactionHeader invoice)
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_AccountNum = "999.888.88";
			glHeader.AG_AccountType = "BSH";
			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_Code = "TSTBNK";
			bankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			bankAccount.AB_AG = glHeader.PK;
			var receipt = Factory.NewWithValidTestData<AccTransactionHeader>();
			receipt.AH_Ledger = LedgerTypes.AccountsReceivable;
			receipt.AH_TransactionType = TransactionTypes.Receipt;
			receipt.AH_PostDate = ZDateTime.Today;
			receipt.AH_DueDate = ZDateTime.Today;
			receipt.AH_OH = invoice.AH_OH;
			receipt.AH_ReceiptType = ReceiptTypes.Cash;
			receipt.AH_AB = bankAccount.PK;
			receipt.AH_ChequeOrReference = "CASH";
			receipt.AH_InvoiceAmount = -invoice.AH_InvoiceAmount;
			receipt.AH_OutstandingAmount = 0m;
			var matchLink1 = Factory.New<AccTransactionMatchLink>();
			matchLink1.AP_AH = receipt.PK;
			matchLink1.AP_Amount = -invoice.AH_InvoiceAmount;
			matchLink1.AP_MatchDate = ZDateTime.Today;
			matchLink1.AP_MatchGroupNum = "M001";
			invoice.AH_FullyPaidDate = ZDateTime.Today;
			invoice.AH_OutstandingAmount = 0m;
			var matchLink2 = Factory.New<AccTransactionMatchLink>();
			matchLink2.AP_AH = invoice.PK;
			matchLink2.AP_Amount = invoice.AH_InvoiceAmount;
			matchLink2.AP_MatchDate = ZDateTime.Today;
			matchLink2.AP_MatchGroupNum = "M001";
			var matchLinkGroup = new MatchLinkGroupForTest(Factory);
			matchLinkGroup.Add(matchLink1);
			matchLinkGroup.Add(matchLink2);
		}

		class MatchLinkGroupForTest : BusinessObjectCollection<AccTransactionMatchLink>, ISupportCriticalValidation
		{
			public MatchLinkGroupForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			ICriticalValidation ISupportCriticalValidation.CriticalValidation
			{
				get
				{
					return new DummyCriticalValidation();
				}
			}

			void IConflictWithCriticalFields.SetConflictWithCriticalFieldsBusinessContext()
			{
				throw new NotImplementedException();
			}
		}

		class DummyCriticalValidation : ICriticalValidation
		{
			void ICriticalValidation.RegisterOnSavingCheck()
			{
			}

			void ICriticalValidation.RunOnSavingCheck()
			{
			}

			void ICriticalValidation.RunDeletedObjectOnSavingCheck()
			{
			}

			public void RunAfterSavingCheck()
			{
			}
		}

		AccTransactionHeader GetJCTransactionHeader(string transactionType, string ledger, string transactionRef)
		{
			AccTransactionHeader result = Factory.New<AccTransactionHeader>();
			result.AH_TransactionType = transactionType;
			result.AH_GB = GlbBranch.CurrentBranch.PK;
			result.AH_GE = GlbDepartment.CurrentDepartment.PK;
			result.AH_PostDate = ZDateTime.Now;
			result.AH_InvoiceDate = ZDateTime.Now;
			result.AH_Ledger = ledger;
			result.AH_TransactionNum = transactionRef;
			return result;
		}

		JobHeader GetJCJobHeader(string jobNum)
		{
			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_JobNum = jobNum;
			return job;
		}

		OrgHeader GetJCOrganisationByPort(string countryCode)
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, countryCode);
			return Factory.LoadTop1<OrgHeader>(filter);
		}

		public void TestJobInvoicingStatusFilter()
		{
			var shipment1 = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S1";
			var cartage1 = Helper.CreateInternalCartageWithNoJobHeader(shipment1);
			AssertNotNull(FilterStripBizO["Invoice Status"]);
			var jobstatusFilter = (ModuleTextFilter)FilterStripBizO["Invoice Status"];
			var job = new JobHeader.Loader(cartage1).TryLoadOrCreate();
			AssertEquals(JobHeaderStatus.Working.Code, job.JH_Status);
			Factory.Save();
			jobstatusFilter.Property = JobHeaderStatus.Working.Code;
			jobstatusFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			jobstatusFilter.IsActive = true;
			var cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = FilterStripBizO.Filter;
			AssertCollectionContains("Cartage1 is in Collection", cartage1, cartages);
			cartages = new CommonCartageCollection(Factory);
			jobstatusFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			cartages.AdditionalFilter = FilterStripBizO.Filter;
			AssertCollectionNotContains("Cartage1 is not in the Collection", cartage1, cartages);
		}

		public void TestLoadFilter()
		{
			var sailing1 = Helper.CreateSailing(Helper.TestVessel1, "111", LocalCartageTestHelper.HomePort, LocalCartageTestHelper.OverseasPort, ZDateTime.Today);
			var sailing2 = Helper.CreateSailing(Helper.TestVessel1, "111", LocalCartageTestHelper.AlternateHomePort, LocalCartageTestHelper.OverseasPort, ZDateTime.Today);
			var sailing3 = Helper.CreateSailing(Helper.TestVessel1, "111", LocalCartageTestHelper.OverseasPort2, LocalCartageTestHelper.OverseasPort, ZDateTime.Today);
			var cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_JX_Sailing = sailing1.PK;
			Asserter.AddToScope(cartage1);
			var cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_JX_Sailing = sailing2.PK;
			Asserter.AddToScope(cartage2);
			var cartage3 = Factory.New<CommonCartage>();
			cartage3.JJ_JX_Sailing = sailing3.PK;
			Asserter.AddToScope(cartage3);
			Factory.Save();
			var filters = new CartageFilterBusinessObject();
			var filter = (ModuleLocationFilter)filters["Load / Discharge"];
			AssertEquals("ScheduleFilterSubGroup", filter.SubGroup.GetType().Name);
			filter.Property1 = LocalCartageTestHelper.HomePort;
			Asserter.AssertMatches("Port", filter, cartage1);
			filter.Property1 = LocalCartageTestHelper.HomePort.SubstringSafe(0, 2);
			Asserter.AssertMatches("Country", filter, cartage1, cartage2);
			filter.Property1 = "";
			Asserter.AssertMatches("Empty", filter, cartage1, cartage2, cartage3);
		}

		public void TestDischargeFilter()
		{
			var sailing1 = Helper.CreateSailing(Helper.TestVessel1, "111", LocalCartageTestHelper.OverseasPort, LocalCartageTestHelper.HomePort, ZDateTime.Today);
			var sailing2 = Helper.CreateSailing(Helper.TestVessel1, "111", LocalCartageTestHelper.OverseasPort, LocalCartageTestHelper.AlternateHomePort, ZDateTime.Today);
			var sailing3 = Helper.CreateSailing(Helper.TestVessel1, "111", LocalCartageTestHelper.OverseasPort, LocalCartageTestHelper.OverseasPort2, ZDateTime.Today);
			var cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_JX_Sailing = sailing1.PK;
			Asserter.AddToScope(cartage1);
			var cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_JX_Sailing = sailing2.PK;
			Asserter.AddToScope(cartage2);
			var cartage3 = Factory.New<CommonCartage>();
			cartage3.JJ_JX_Sailing = sailing3.PK;
			Asserter.AddToScope(cartage3);
			Factory.Save();
			var filters = new CartageFilterBusinessObject();
			var filter = (ModuleLocationFilter)filters["Load / Discharge"];
			AssertEquals("ScheduleFilterSubGroup", filter.SubGroup.GetType().Name);
			filter.Property2 = LocalCartageTestHelper.HomePort;
			Asserter.AssertMatches("Port", filter, cartage1);
			filter.Property2 = LocalCartageTestHelper.HomePort.SubstringSafe(0, 2);
			Asserter.AssertMatches("Country", filter, cartage1, cartage2);
			filter.Property2 = "";
			Asserter.AssertMatches("Empty", filter, cartage1, cartage2, cartage3);
		}

		public void TestRelatedPort()
		{
			TestJobDocAddressTextFilter("Related Port", JobContainerLegsSchema.JU_E2PickupAddressID, null, OrgAddressSchema.OA_RL_NKRelatedPortCode, "AUSYD", "NZAKL");
			TestJobDocAddressTextFilter("Related Port", JobContainerLegsSchema.JU_E2WaitPointAddressID, null, OrgAddressSchema.OA_RL_NKRelatedPortCode, "AUSYD", "NZAKL");
			TestJobDocAddressTextFilter("Related Port", JobContainerLegsSchema.JU_E2PickupAddressID, null, OrgAddressSchema.OA_RL_NKRelatedPortCode, "AUSYD", "NZAKL");
		}

		public void TestPickupDelivery()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_GB = branch.PK;
			CommonBookedCtgMove booked1 = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg1 = booked1.CartageLegs.AddNew();
			CommonBookedCtgMove booked2 = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg2 = booked2.CartageLegs.AddNew();
			JobDocAddress docAddress1 = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageImporter, "org1", "add1", "2000", "SYDNEY", "AUSYD", false);
			JobDocAddress docAddress2 = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCFS, "org2", "add2", "2000", "SYDNEY", "AUSYD", false);
			cartageLeg1.JU_E2PickupAddressID = docAddress1.PK;
			cartageLeg1.JU_E2DeliveryAddressID = docAddress2.PK;
			cartageLeg2.JU_E2PickupAddressID = docAddress1.PK;
			cartageLeg2.JU_E2DeliveryAddressID = docAddress2.PK;
			Factory.Save();
			CartageFilterBusinessObject filter = new CartageFilterBusinessObject();
			((ModuleGuidsFilter)filter[CartageFilterBusinessObject.OrganisationFilterTypes.PickupDelivery]).Property1 = docAddress1.Organisation.PK;
			((ModuleGuidsFilter)filter[CartageFilterBusinessObject.OrganisationFilterTypes.PickupDelivery]).Property2 = ZGuid.Empty;
			((ModuleGuidsFilter)filter[CartageFilterBusinessObject.OrganisationFilterTypes.PickupDelivery]).IsActive = true;
			CommonCartageCollection collection = new CommonCartageCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionContains("Should have Cartage", cartage, collection);
			((ModuleGuidsFilter)filter[CartageFilterBusinessObject.OrganisationFilterTypes.PickupDelivery]).Property1 = ZGuid.Empty;
			((ModuleGuidsFilter)filter[CartageFilterBusinessObject.OrganisationFilterTypes.PickupDelivery]).Property2 = docAddress2.Organisation.PK;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionContains("Should have Cartage", cartage, collection);
			((ModuleGuidsFilter)filter[CartageFilterBusinessObject.OrganisationFilterTypes.PickupDelivery]).Property1 = docAddress1.Organisation.PK;
			((ModuleGuidsFilter)filter[CartageFilterBusinessObject.OrganisationFilterTypes.PickupDelivery]).Property2 = docAddress2.Organisation.PK;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionContains("Should have Cartage", cartage, collection);
			((ModuleGuidsFilter)filter[CartageFilterBusinessObject.OrganisationFilterTypes.PickupDelivery]).Property1 = docAddress2.Organisation.PK;
			((ModuleGuidsFilter)filter[CartageFilterBusinessObject.OrganisationFilterTypes.PickupDelivery]).Property2 = ZGuid.Empty;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 0 CommonCartage", 0, collection.Count);
			AssertCollectionNotContains("Should have Cartage", cartage, collection);
			((ModuleGuidsFilter)filter[CartageFilterBusinessObject.OrganisationFilterTypes.PickupDelivery]).Property1 = ZGuid.Empty;
			((ModuleGuidsFilter)filter[CartageFilterBusinessObject.OrganisationFilterTypes.PickupDelivery]).Property2 = docAddress1.Organisation.PK;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 0 CommonCartage", 0, collection.Count);
			AssertCollectionNotContains("Should have Cartage", cartage, collection);
			((ModuleGuidsFilter)filter[CartageFilterBusinessObject.OrganisationFilterTypes.PickupDelivery]).Property1 = ZGuid.Empty;
			((ModuleGuidsFilter)filter[CartageFilterBusinessObject.OrganisationFilterTypes.PickupDelivery]).Property2 = ZGuid.Empty;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionContains("Should have Cartage", cartage, collection);
		}

		void TestCartageTextFilter(ZString filterName, SchemaColumn schemaColumn)
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = branch.PK;
			CommonCartage cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_GB = branch.PK;
			CommonCartage cartage3 = Factory.New<CommonCartage>();
			cartage3.JJ_GB = branch.PK;
			ZDateTime now = ZDateTime.Now;
			cartage1[schemaColumn.Name] = "123";
			cartage2[schemaColumn.Name] = "456";
			cartage3[schemaColumn.Name] = "789";
			Factory.Save();
			CartageFilterBusinessObject filter = new CartageFilterBusinessObject();
			((ModuleTextFilter)filter[filterName]).Property = "456";
			((ModuleTextFilter)filter[filterName]).IsActive = true;
			CommonCartageCollection collection = new CommonCartageCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartage1, collection);
			AssertCollectionContains("Should have Cartage2", cartage2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartage3, collection);
			((ModuleTextFilter)filter[filterName]).Property = "123";
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartage1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartage2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartage3, collection);
		}

		void TestCartageDateFilter(ZString filterName, SchemaColumn schemaColumn)
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = branch.PK;
			CommonCartage cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_GB = branch.PK;
			CommonCartage cartage3 = Factory.New<CommonCartage>();
			cartage3.JJ_GB = branch.PK;
			ZDateTime now = ZDateTime.Now;
			cartage1[schemaColumn.Name] = now.AddDays(5);
			cartage2[schemaColumn.Name] = now.AddDays(7);
			cartage3[schemaColumn.Name] = now.AddDays(9);
			Factory.Save();
			CartageFilterBusinessObject filter = new CartageFilterBusinessObject();
			((ModuleDateFilter)filter[filterName]).Property1 = now.AddDays(6);
			((ModuleDateFilter)filter[filterName]).Property2 = now.AddDays(8);
			((ModuleDateFilter)filter[filterName]).IsActive = true;
			((ModuleDateFilter)filter[filterName]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			CommonCartageCollection collection = new CommonCartageCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartage1, collection);
			AssertCollectionContains("Should have Cartage2", cartage2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartage3, collection);
			((ModuleDateFilter)filter[filterName]).Property1 = now.AddDays(4);
			((ModuleDateFilter)filter[filterName]).Property2 = now.AddDays(6);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartage1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartage2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartage3, collection);
		}

		void TestCartageLegDateFilter(ZString filterName, SchemaColumn schemaColumn)
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = branch.PK;
			CommonBookedCtgMove booked1 = cartage1.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg1 = booked1.CartageLegs.AddNew();
			CommonCartage cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_GB = branch.PK;
			CommonBookedCtgMove booked2 = cartage2.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg2 = booked2.CartageLegs.AddNew();
			CommonCartage cartage3 = Factory.New<CommonCartage>();
			cartage3.JJ_GB = branch.PK;
			CommonBookedCtgMove booked3 = cartage3.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg3 = booked3.CartageLegs.AddNew();
			ZDateTime now = ZDateTime.Now;
			cartageLeg1[schemaColumn.Name] = now.AddDays(5);
			cartageLeg2[schemaColumn.Name] = now.AddDays(7);
			cartageLeg3[schemaColumn.Name] = now.AddDays(9);
			Factory.Save();
			CartageFilterBusinessObject filter = new CartageFilterBusinessObject();
			((ModuleDateFilter)filter[filterName]).Property1 = now.AddDays(6);
			((ModuleDateFilter)filter[filterName]).Property2 = now.AddDays(8);
			((ModuleDateFilter)filter[filterName]).IsActive = true;
			((ModuleDateFilter)filter[filterName]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			CommonCartageCollection collection = new CommonCartageCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartage1, collection);
			AssertCollectionContains("Should have Cartage2", cartage2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartage3, collection);
			((ModuleDateFilter)filter[filterName]).Property1 = now.AddDays(4);
			((ModuleDateFilter)filter[filterName]).Property2 = now.AddDays(6);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartage1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartage2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartage3, collection);
		}

		void TestContainerTextFilter(ZString filterName, SchemaColumn schemaColumn)
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = branch.PK;
			var container1 = cartage1.ContainerBookedMoves.AddNew().Container;
			var cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_GB = branch.PK;
			var container2 = cartage2.ContainerBookedMoves.AddNew().Container;
			var cartage3 = Factory.New<CommonCartage>();
			cartage3.JJ_GB = branch.PK;
			var container3 = cartage3.ContainerBookedMoves.AddNew().Container;
			ZDateTime now = ZDateTime.Now;
			container1[schemaColumn.Name] = "123";
			container2[schemaColumn.Name] = "456";
			container3[schemaColumn.Name] = "789";
			Factory.Save();
			var filterBizO = new CartageFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO[filterName];
			filter.Property = "456";
			filter.IsActive = true;
			var collection = new CommonCartageCollection(Factory);
			collection.AdditionalFilter = filterBizO.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartage1, collection);
			AssertCollectionContains("Should have Cartage2", cartage2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartage3, collection);
			filter.Property = "123";
			collection.AdditionalFilter = filterBizO.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartage1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartage2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartage3, collection);
			AssertEquals("If max length is not set, we may attempt to save data that exceeds the DB field which will result in a SQL error.", ModuleNumberFilter.MultiplyMaxLength(schemaColumn.MaxLength), filter.MaxLength);
		}

		void TestWorkSheetGuidFilter(ZString filterName, SchemaColumn schemaColumn, ZGuid pk1, ZGuid pk2)
		{
			TestWorkSheetFilter(filterName, schemaColumn, pk1, pk2);
		}

		void TestWorkSheetNkFilter(ZString filterName, SchemaColumn schemaColumn, ZString key1, ZString key2)
		{
			TestWorkSheetFilter(filterName, schemaColumn, key1, key2);
		}

		void TestWorkSheetFilter<T>(ZString filterName, SchemaColumn schemaColumn, T key1, T key2)
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = branch.PK;
			CommonBookedCtgMove booked1 = cartage1.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg1 = booked1.CartageLegs.AddNew();
			CommonWorkSheet workSheet1 = Factory.New<CommonWorkSheet>();
			cartageLeg1.JU_EY_RunSheet = workSheet1.PK;
			CommonCartage cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_GB = branch.PK;
			CommonBookedCtgMove booked2 = cartage2.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg2 = booked2.CartageLegs.AddNew();
			CommonWorkSheet workSheet2 = Factory.New<CommonWorkSheet>();
			cartageLeg2.JU_EY_RunSheet = workSheet2.PK;
			CommonCartage cartage3 = Factory.New<CommonCartage>();
			cartage3.JJ_GB = branch.PK;
			CommonBookedCtgMove booked3 = cartage3.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg3 = booked3.CartageLegs.AddNew();
			workSheet1[schemaColumn.Name] = key1;
			workSheet2[schemaColumn.Name] = key2;
			Factory.Save();
			CartageFilterBusinessObject filter = new CartageFilterBusinessObject();
			if (typeof(T) == typeof(ZGuid))
			{
				((ModuleGuidFilter)filter[filterName]).Property = new ZGuid(key2);
				((ModuleGuidFilter)filter[filterName]).IsActive = true;
			}
			else if (typeof(T) == typeof(ZString))
			{
				((ModuleTextBaseFilter)filter[filterName]).Property = new ZString(key2);
				((ModuleTextBaseFilter)filter[filterName]).IsActive = true;
			}

			CommonCartageCollection collection = new CommonCartageCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartage1, collection);
			AssertCollectionContains("Should have Cartage2", cartage2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartage3, collection);
			if (typeof(T) == typeof(ZGuid))
			{
				((ModuleGuidFilter)filter[filterName]).Property = new ZGuid(key1);
			}
			else if (typeof(T) == typeof(ZString))
			{
				((ModuleTextBaseFilter)filter[filterName]).Property = new ZString(key1);
			}

			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartage1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartage2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartage3, collection);
		}

		void TestBookedCtgMoveTextFilter(ZString filterName, SchemaColumn schemaColumn)
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = branch.PK;
			CommonBookedCtgMove booked1 = cartage1.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg1 = booked1.CartageLegs.AddNew();
			CommonCartage cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_GB = branch.PK;
			CommonBookedCtgMove booked2 = cartage2.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg2 = booked2.CartageLegs.AddNew();
			booked1[schemaColumn.Name] = "abc";
			booked2[schemaColumn.Name] = "def";
			Factory.Save();
			CartageFilterBusinessObject filter = new CartageFilterBusinessObject();
			((ModuleTextFilter)filter[filterName]).Property = "def";
			((ModuleTextFilter)filter[filterName]).IsActive = true;
			CommonCartageCollection collection = new CommonCartageCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartage1, collection);
			AssertCollectionContains("Should have Cartage2", cartage2, collection);
			((ModuleTextFilter)filter[filterName]).Property = "abc";
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartage1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartage2, collection);
		}

		void TestSailingDateFilter(ZString filterName, SchemaColumn schemaColumn)
		{
			voyageNumber++;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "abc" + voyageNumber.ToString();
			string uniqueVoyageNumber = voyageNumber.ToString();
			var sailing = Helper.CreateSailing(vessel, uniqueVoyageNumber, "AUSYD", "NZAKL", ZDateTime.Empty);
			var now = ZDateTime.Now;
			switch (schemaColumn.TableName)
			{
				case JobVoyOriginSchema.Constants.TableName:
					sailing.Origin[schemaColumn.Name] = now;
					break;
				case JobVoyDestinationSchema.Constants.TableName:
					sailing.Destination[schemaColumn.Name] = now;
					break;
				case JobSailingSchema.Constants.TableName:
					sailing[schemaColumn.Name] = now;
					break;
			}

			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_JX_Sailing = sailing.PK;
			Asserter.AddToScope(cartage);
			Factory.Save();
			var filters = new CartageFilterBusinessObject();
			var filter = (ModuleDateFilter)filters[filterName];
			AssertEquals("ScheduleFilterSubGroup", filter.SubGroup.GetType().Name);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = now.AddDays(-1);
			Asserter.AssertMatches("from -1", filter, cartage);
			filter.Property2 = now.AddDays(-1);
			Asserter.AssertMatches("from -1 to -1", filter);
			filter.Property2 = now.AddDays(1);
			Asserter.AssertMatches("from -1 to 1", filter, cartage);
			filter.Property1 = now.AddDays(1);
			Asserter.AssertMatches("from 1 to 1", filter);
			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("to 1", filter, cartage);
		}

		static int voyageNumber;

		void TestJobDocAddressTextFilter(ZString filterName, SchemaColumn foreignKeyColumn, SchemaColumn docAddressSchemaColumn, SchemaColumn orgAddressSchemaColumn, ZString value1, ZString value2)
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_GB = branch.PK;
			CommonBookedCtgMove booked1 = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg1 = booked1.CartageLegs.AddNew();
			CommonBookedCtgMove booked2 = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg2 = booked2.CartageLegs.AddNew();
			if (docAddressSchemaColumn != null)
			{
				JobDocAddress docAddress1 = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageImporter, "org1", "add1", "2000", "SYDNEY", "AUSYD", true);
				docAddress1[docAddressSchemaColumn] = value1;
				cartageLeg1[foreignKeyColumn.Name] = docAddress1.PK;
				JobDocAddress docAddress2 = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCFS, "org2", "add2", "2000", "SYDNEY", "AUSYD", true);
				docAddress2[docAddressSchemaColumn] = value2;
				cartageLeg2[foreignKeyColumn.Name] = docAddress2.PK;
				Factory.Save();
				CartageLegFilterStripBusinessObject filter = new CartageLegFilterStripBusinessObject();
				((ModuleTextBaseFilter)filter[filterName]).Property = value2;
				((ModuleTextBaseFilter)filter[filterName]).IsActive = true;
				CommonCartageLegCollection collection = new CommonCartageLegCollection(Factory);
				collection.AdditionalFilter = filter.Filter;
				AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
				AssertCollectionNotContains("Should not have Cartage1", cartageLeg1, collection);
				AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
				((ModuleTextBaseFilter)filter[filterName]).Property = value1;
				collection.AdditionalFilter = filter.Filter;
				AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
				AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
				AssertCollectionNotContains("Should not have Cartage2", cartageLeg2, collection);
			}

			if (orgAddressSchemaColumn != null)
			{
				JobDocAddress docAddress1 = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageImporter, "org1", "add1", "2000", "SYDNEY", "AUSYD", false);
				docAddress1.Address[orgAddressSchemaColumn] = value1;
				cartageLeg1[foreignKeyColumn.Name] = docAddress1.PK;
				JobDocAddress docAddress2 = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCFS, "org2", "add1", "2000", "SYDNEY", "AUSYD", false);
				docAddress2.Address[orgAddressSchemaColumn] = value2;
				cartageLeg2[foreignKeyColumn.Name] = docAddress2.PK;
				Factory.Save();
				CartageLegFilterStripBusinessObject filter = new CartageLegFilterStripBusinessObject();
				((ModuleTextBaseFilter)filter[filterName]).Property = value2;
				((ModuleTextBaseFilter)filter[filterName]).IsActive = true;
				CommonCartageLegCollection collection = new CommonCartageLegCollection(Factory);
				collection.AdditionalFilter = filter.Filter;
				AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
				AssertCollectionNotContains("Should not have Cartage1", cartageLeg1, collection);
				AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
				((ModuleTextBaseFilter)filter[filterName]).Property = value1;
				collection.AdditionalFilter = filter.Filter;
				AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
				AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
				AssertCollectionNotContains("Should not have Cartage2", cartageLeg2, collection);
			}

			cartage.Delete();
			Factory.Save();
		}

		public void TestCustomFieldFilter()
		{
			var template = Helper.CreateWorkflowTemplate(JobInvoicingConsumerTypes.LocalCartage.Code);
			Helper.AddCustomField(template, "Custom1", AddOnColumnDataType.Codes.String);
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = branch.PK;
			cartage1.SetUserDefinedValue("Custom1", (ZString)"111");
			var cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_GB = branch.PK;
			cartage2.SetUserDefinedValue("Custom1", (ZString)"222");
			Factory.Save();
			var filter = new CartageFilterBusinessObject();
			((ModuleTextFilter)filter["Custom1"]).Property = "111";
			((ModuleTextFilter)filter["Custom1"]).IsActive = true;
			var collection = new CommonCartageCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage2", cartage2, collection);
			AssertCollectionContains("Should have Cartage1", cartage1, collection);
		}

		public void TestAPInvoiceNumberFilter()
		{
			AssertNotNull(FilterStripBizO["AP Invoice #"]);
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			CommonCartage cartage2 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = GlbBranch.CurrentBranch.PK;
			cartage2.JJ_GB = GlbBranch.CurrentBranch.PK;
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = cartage1.PK;
			job.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.Parent = cartage1;
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "00001001";
			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_OH = org.PK;
			newInvoice.AH_TransactionNum = "00001001";
			newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_ConsolidatedInvoiceRef = "S00009999";
			AccTransactionLines newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			newInvoiceLine.AL_AH = newInvoice.PK;
			newInvoiceLine.AL_JH = job.PK;
			newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Factory.Save();
			CartageFilterBusinessObject filterBO = new CartageFilterBusinessObject();
			ModuleNumberFilter filter = (ModuleNumberFilter)filterBO["AP Invoice #"];
			filter.Property = "00001001";
			filter.IsActive = true;
			CommonCartageCollection cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, cartages.Count);
			AssertCollectionContains("Cartage1 is in Collection", cartage1, cartages);
			AssertCollectionNotContains("Cartage2 is not in Collection", cartage2, cartages);
			filter.Property = "00001002";
			filter.IsActive = true;
			cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			AssertEquals("Should have 0 CommonCartage", 0, cartages.Count);
			AssertCollectionNotContains("Cartage1 is not in Collection", cartage1, cartages);
			AssertCollectionNotContains("Cartage2 is not in Collection", cartage2, cartages);
			filter.Property = "";
			filter.IsActive = true;
			cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			AssertEquals("Should have 2 CommonCartage", 2, cartages.Count);
			AssertCollectionContains("Cartage1 is in Collection", cartage1, cartages);
			AssertCollectionContains("Cartage2 is in Collection", cartage2, cartages);
		}

		public void TestARTransactionFilter()
		{
			AssertNotNull(FilterStripBizO["AR Transaction #"]);
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			CommonCartage cartage2 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = GlbBranch.CurrentBranch.PK;
			cartage2.JJ_GB = GlbBranch.CurrentBranch.PK;
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = cartage1.PK;
			job.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.Parent = cartage1;
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "00001005";
			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_OH = org.PK;
			newInvoice.AH_TransactionNum = "00001005";
			newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_ConsolidatedInvoiceRef = "S00009999";
			AccTransactionLines newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			newInvoiceLine.AL_AH = newInvoice.PK;
			newInvoiceLine.AL_JH = job.PK;
			newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Factory.Save();
			CartageFilterBusinessObject filterBO = new CartageFilterBusinessObject();
			ModuleFountainFilter filter = (ModuleFountainFilter)filterBO["AR Transaction #"];
			filter.Property = "00001005";
			filter.IsActive = true;
			CommonCartageCollection cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			AssertEquals("Should have 1 CommonCartage", 1, cartages.Count);
			AssertCollectionContains("Cartage1 is in Collection", cartage1, cartages);
			AssertCollectionNotContains("Cartage2 is not in Collection", cartage2, cartages);
			filter.Property = "00001002";
			filter.IsActive = true;
			cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			AssertEquals("Should have 0 CommonCartage", 0, cartages.Count);
			AssertCollectionNotContains("Cartage1 is not in Collection", cartage1, cartages);
			AssertCollectionNotContains("Cartage2 is not in Collection", cartage2, cartages);
			filter.Property = "";
			filter.IsActive = true;
			cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			AssertEquals("Should have 2 CommonCartage", 2, cartages.Count);
			AssertCollectionContains("Cartage1 is in Collection", cartage1, cartages);
			AssertCollectionContains("Cartage2 is in Collection", cartage2, cartages);
		}

		public void TestAddWorkflowCustomFieldsFilters()
		{
			ModuleFilterCollection filterCollection = new CartageFilterBusinessObject().ModuleFilters;
			AssertNull(filterCollection["C11"]);
			AssertNull(filterCollection["C12"]);
			AssertNull(filterCollection["C21"]);
			AssertNull(filterCollection["C22"]);
			AssertNull(filterCollection["Workflow Flags"]);
			AssertNull(filterCollection["C31"]);
			PrepareTemplates();
			filterCollection = new CartageFilterBusinessObject().ModuleFilters;
			AssertEquals(typeof(ModuleTextFilter), filterCollection["C11"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), filterCollection["C12"].GetType());
			AssertEquals(typeof(ModuleDateFilter), filterCollection["C21"].GetType());
			AssertEquals(typeof(ModuleTextFilter), filterCollection["C22"].GetType());
			AssertEquals(typeof(ModuleFlagsFilter), filterCollection["Workflow Flags"].GetType());
			AssertNull(filterCollection["C31"]);
		}

		void PrepareTemplates()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = JobInvoicingConsumerTypes.LocalCartage.Code;
			var def11 = template1.GenCustomColumnDefinitions.AddNew();
			def11.XC_Name = "C11";
			def11.XC_Type = AddOnColumnDataType.Codes.String;
			var def12 = template1.GenCustomColumnDefinitions.AddNew();
			def12.XC_Name = "C12";
			def12.XC_Type = AddOnColumnDataType.Codes.Integer;
			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = JobInvoicingConsumerTypes.LocalCartage.Code;
			var def21 = template2.GenCustomColumnDefinitions.AddNew();
			def21.XC_Name = "C21";
			def21.XC_Type = AddOnColumnDataType.Codes.Datetime;
			var def22 = template2.GenCustomColumnDefinitions.AddNew();
			def22.XC_Name = "C22";
			def22.XC_Type = AddOnColumnDataType.Codes.Boolean;
			var defDuplicate = template2.GenCustomColumnDefinitions.AddNew();
			defDuplicate.XC_Name = "C11";
			defDuplicate.XC_Type = AddOnColumnDataType.Codes.String;
			var template3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template3.P0_ProcessType = "YYY";
			var def31 = template3.GenCustomColumnDefinitions.AddNew();
			def31.XC_Name = "C31";
			def31.XC_Type = AddOnColumnDataType.Codes.String;
			Factory.Save();
			WorkflowCustomFieldsFilter.ClearCache();
		}

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<CommonCartage>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.TransportJobCRMSecurity);
		}

		public void TestParentJobNumberFilterExists()
		{
			AssertNotNull("Filter for Parent Job Number should exist", FilterStripBizO["Parent Job #"]);
		}

		public void TestParentJobNumberFilterWithContainsOperator()
		{
			var cartageData = CreateCartagesWithDifferentParents();
			CartageFilterBusinessObject filterBO = new CartageFilterBusinessObject();
			ModuleNumberFilter filter = (ModuleNumberFilter)filterBO["Parent Job #"];
			filter.Property = "BX";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			CommonCartageCollection cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			AssertContainsExactElementsInAnyOrder("Cartage with parent job BX0000001 is in Collection", new[] { cartageData["CUS"] }, cartages);
			filter.Property = "00000";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.IsActive = true;
			cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			AssertContainsExactElementsInAnyOrder("Should get all cartages with parents containing '00000' in their unique consignment ID - ie except for cartage with forwarding shipment parent with JS_UniqueConsignRef = 'ABC0123'", new[] { cartageData["CFS"], cartageData["CFC"], cartageData["WHO"], cartageData["CUS"], cartageData["TBK"], }, cartages);
		}

		public void TestParentJobNumberFilterWithNotContainsOperator()
		{
			var cartageData = CreateCartagesWithDifferentParents();
			CartageFilterBusinessObject filterBO = new CartageFilterBusinessObject();
			ModuleNumberFilter filter = (ModuleNumberFilter)filterBO["Parent Job #"];
			filter.Property = "BX";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			CommonCartageCollection cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			AssertContainsExactElementsInAnyOrder("Cartage with parent job BX0000001 is in Collection", new[] { cartageData["CUS"] }, cartages);
			filter.Property = "00000";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.IsActive = true;
			cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			AssertContainsExactElementsInAnyOrder("Should get all cartages with parents not containing '00000' in their unique consignment ID - ie standalone cartage and cartage with forwarding shipment parent with JS_UniqueConsignRef = 'ABC0123'", new[] { cartageData["STC"], cartageData["SHP"], }, cartages);
		}

		public void TestParentJobTypeFilterExists()
		{
			AssertNotNull("Filter for Parent Job Type should exist", FilterStripBizO["Parent Job Type"]);
		}

		public void TestParentJobTypeFilterForwardingShipment()
		{
			var cartageData = CreateCartagesWithDifferentParents();
			var cartages = GetCartagesWithParentJobTypeFilter("SHP");
			AssertContainsExactElementsInAnyOrder("Cartage with parent Forwarding Shipment is in Collection", new[] { cartageData["SHP"] }, cartages);
		}

		public void TestParentJobTypeFilterCFSShipment()
		{
			var cartageData = CreateCartagesWithDifferentParents();
			var cartages = GetCartagesWithParentJobTypeFilter("CFS");
			AssertContainsExactElementsInAnyOrder("Cartage with parent CFS Shipment is in Collection", new[] { cartageData["CFS"] }, cartages);
		}

		public void TestParentJobTypeFilterCFSConsol()
		{
			var cartageData = CreateCartagesWithDifferentParents();
			var cartages = GetCartagesWithParentJobTypeFilter("CFC");
			AssertContainsExactElementsInAnyOrder("Cartage with parent CFS Consol is in Collection", new[] { cartageData["CFC"] }, cartages);
		}

		public void TestParentJobTypeFilterWhsOrder()
		{
			var cartageData = CreateCartagesWithDifferentParents();
			var cartages = GetCartagesWithParentJobTypeFilter("WHO");
			AssertContainsExactElementsInAnyOrder("Cartage with parent Warehouse Order is in Collection", new[] { cartageData["WHO"] }, cartages);
		}

		public void TestParentJobTypeFilterCustomsDeclaration()
		{
			var cartageData = CreateCartagesWithDifferentParents();
			var cartages = GetCartagesWithParentJobTypeFilter("CUS");
			AssertContainsExactElementsInAnyOrder("Cartage with parent Customs Declaration is in Collection", new[] { cartageData["CUS"] }, cartages);
		}

		public void TestParentJobTypeFilterTransportBooking()
		{
			var cartageData = CreateCartagesWithDifferentParents();
			var cartages = GetCartagesWithParentJobTypeFilter("TBK");
			AssertContainsExactElementsInAnyOrder("Cartage with parent Transport Booking is in Collection", new[] { cartageData["TBK"] }, cartages);
		}

		public void TestParentJobTypeFilterStandalone()
		{
			var cartageData = CreateCartagesWithDifferentParents();
			var cartages = GetCartagesWithParentJobTypeFilter("STC");
			AssertContainsExactElementsInAnyOrder("Stand-alone Cartage is in Collection", new[] { cartageData["STC"] }, cartages);
		}

		CartageFilterBusinessObject CreateParentJobTypeFilter(string typeCode)
		{
			CartageFilterBusinessObject filterBO = new CartageFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBO["Parent Job Type"];
			filter.Property = typeCode;
			filter.IsActive = true;
			return filterBO;
		}

		CommonCartageCollection GetCartagesWithParentJobTypeFilter(string typeCode)
		{
			var filterBO = CreateParentJobTypeFilter(typeCode);
			var cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			return cartages;
		}

		Dictionary<string, CommonCartage> CreateCartagesWithDifferentParents()
		{
			var cartages = new Dictionary<string, CommonCartage>();
			(var cartage1, var _, var _, var _) = LocalCartageTestHelper.CreateTestCartageWithParentForwardingShipment(Factory);
			cartage1.JJ_ConsignmentID = "TSHP1";
			cartages["SHP"] = cartage1;
			var cfsConsol = CreateTestCfsConsolParent();
			cartages["CFC"] = CreateTestCartageWithParent((ICartageParent)cfsConsol, "TCFC1");
			var cfsShipment = CreateTestCfsShipmentParent();
			cartages["CFS"] = CreateTestCartageWithParent((ICartageParent)cfsShipment, "TCFS1");
			var whsOrder = CreateTestWhsOrderParent();
			cartages["WHO"] = CreateTestCartageWithParent((ICartageParent)whsOrder, "TWHO1");
			var customsDeclaration = CreateTestCustomsDeclarationParent();
			cartages["CUS"] = CreateTestCartageWithParent((ICartageParent)customsDeclaration, "TCUS1");
			var dtbBooking = CreateTestDtbBooking();
			cartages["TBK"] = CreateTestCartageWithDtbBookingParent(dtbBooking, "TTBK1");
			cartages["STC"] = CreateTestCartage("TSTC1");
			Factory.Save();
			return cartages;
		}

		BusinessObject CreateTestCfsConsolParent()
		{
			var cfsConsol = Factory.New(ObjectFactory.GetType<ICFSLoadListConsol>());
			cfsConsol[JobConsolSchema.JK_UniqueConsignRef] = "CX0000001";
			cfsConsol[JobConsolSchema.JK_ConsolMode] = "FCL";
			cfsConsol[JobConsolSchema.JK_TransportMode] = "SEA";
			cfsConsol[JobConsolSchema.JK_MasterBillNum] = "BILL123";
			cfsConsol[JobConsolSchema.JK_IsCFS] = true;
			cfsConsol[JobConsolSchema.JK_IsForwarding] = false;
			return cfsConsol;
		}

		BusinessObject CreateTestCfsShipmentParent()
		{
			var cfsShipment = Factory.New(ObjectFactory.GetType<ICFSShipment>());
			cfsShipment[JobShipmentSchema.JS_UniqueConsignRef] = "SX000001";
			cfsShipment[JobShipmentSchema.JS_IsCFSRegistered] = true;
			cfsShipment[JobShipmentSchema.JS_IsForwardRegistered] = false;
			cfsShipment[JobShipmentSchema.JS_IsBooking] = false;
			return cfsShipment;
		}

		BusinessObject CreateTestWhsOrderParent(string orderNumber = "IX0000001")
		{
			var whsOrder = (BusinessObject)Factory.New<IWhsOrder>();
			whsOrder.FillWithValidTestData();
			whsOrder[WhsDocketSchema.WD_DocketID] = orderNumber;
			whsOrder[WhsDocketSchema.WD_DocketSubType] = "ORD";
			return whsOrder;
		}

		BusinessObject CreateTestCustomsDeclarationParent()
		{
			var customsDeclaration = Factory.New(ObjectFactory.GetType<IBaseJobDeclaration>());
			customsDeclaration.FillWithValidTestData();
			customsDeclaration[JobDeclarationSchema.JE_DeclarationReference] = "BX0000001";
			return customsDeclaration;
		}

		IDtbBooking CreateTestDtbBooking()
		{
			var dtbBooking = Factory.New(ObjectFactory.GetType<IDtbBooking>());
			dtbBooking.FillWithValidTestData();
			dtbBooking[DtbBookingSchema.KM_JobID] = "TB000001";
			return (IDtbBooking)dtbBooking;
		}

		CommonCartage CreateTestCartageWithParent(ICartageParent parent, string jobID = "T0000001")
		{
			var cartage = CreateTestCartage(jobID);
			cartage.SetParent(parent);
			var parentBO = (BusinessObject)parent;
			cartage.JJ_ParentID = (parentBO).PK;
			cartage.JJ_ParentTableCode = parentBO.TablePrefix;
			return cartage;
		}

		CommonCartage CreateTestCartageWithDtbBookingParent(IDtbBooking dtbBooking, string jobID = "T0000001")
		{
			var cartage = CreateTestCartage(jobID);
			cartage.JJ_ParentID = dtbBooking.PK;
			cartage.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			cartage.FillWithValidTestData();
			return cartage;
		}

		CommonCartage CreateTestCartage(string jobID = "T0000001")
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_GB = GlbBranch.CurrentBranch.PK;
			cartage.JJ_ConsignmentID = jobID;
			Factory.Save();
			return cartage;
		}

		CartageFilterBusinessObject CreateMilestoneDateFilter(int fromDateDaysAfterCurrentTime, int toDateDaysAfterCurrentTime)
		{
			var currentTime = ZDateTime.UtcNow;

			var filterBO = new CartageFilterBusinessObject();
			var filter = (WorkflowModuleFilter)filterBO.ModuleFilters["Milestone Date"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(currentTime.AddDays(fromDateDaysAfterCurrentTime));
			filter.Property2 = new ZDateTime(currentTime.AddDays(toDateDaysAfterCurrentTime));
			filter.IsActive = true;
			return filterBO;
		}

		CommonCartageCollection GetCartagesWithMilestoneDateFilter(int fromDateDaysAfterCurrentTime, int toDateDaysAfterCurrentTime)
		{
			var filterBO = CreateMilestoneDateFilter(fromDateDaysAfterCurrentTime, toDateDaysAfterCurrentTime);
			var cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			return cartages;
		}

		CartageFilterBusinessObject CreateMilestoneCompletedFilter(string filterProperty)
		{
			var currentTime = ZDateTime.UtcNow;

			var filterBO = new CartageFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBO.ModuleFilters["Milestone Completed"];
			filter.Property = filterProperty;
			filter.IsActive = true;
			return filterBO;
		}

		CommonCartageCollection GetCartagesWithMilestoneCompletedFilter(string filterProperty)
		{
			var filterBO = CreateMilestoneCompletedFilter(filterProperty);
			var cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			return cartages;
		}

		CartageFilterBusinessObject CreateMilestoneNextFilter(int fromDateDaysAfterCurrentTime, int toDateDaysAfterCurrentTime, string milestoneEvent)
		{
			var currentTime = ZDateTime.UtcNow;

			var filterBO = new CartageFilterBusinessObject();
			var filter = (WorkflowModuleFilter)filterBO.ModuleFilters["Next Milestone"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(currentTime.AddDays(fromDateDaysAfterCurrentTime));
			filter.Property2 = new ZDateTime(currentTime.AddDays(toDateDaysAfterCurrentTime));
			filter.MilestoneEvent = milestoneEvent;
			filter.IsActive = true;
			return filterBO;
		}

		CommonCartageCollection GetCartagesWithMilestoneNextFilter(int fromDateDaysAfterCurrentTime, int toDateDaysAfterCurrentTime, string milestoneEvent)
		{
			var filterBO = CreateMilestoneNextFilter(fromDateDaysAfterCurrentTime, toDateDaysAfterCurrentTime, milestoneEvent);
			var cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			return cartages;
		}

		CartageFilterBusinessObject CreateMilestoneLastCompletedFilter(int fromDateDaysAfterCurrentTime, int toDateDaysAfterCurrentTime, string milestoneEvent)
		{
			var currentTime = ZDateTime.UtcNow;

			var filterBO = new CartageFilterBusinessObject();
			var filter = (WorkflowModuleFilter)filterBO.ModuleFilters["Last Completed Milestone"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(currentTime.AddDays(fromDateDaysAfterCurrentTime));
			filter.Property2 = new ZDateTime(currentTime.AddDays(toDateDaysAfterCurrentTime));
			filter.MilestoneEvent = milestoneEvent;
			filter.IsActive = true;
			return filterBO;
		}

		CommonCartageCollection GetCartagesWithMilestoneLastCompletedFilter(int fromDateDaysAfterCurrentTime, int toDateDaysAfterCurrentTime, string milestoneEvent)
		{
			var filterBO = CreateMilestoneLastCompletedFilter(fromDateDaysAfterCurrentTime, toDateDaysAfterCurrentTime, milestoneEvent);
			var cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			return cartages;
		}

		[TestDate(2023, 5, 12)]
		public void TestMilestoneDateFilter()
		{
			var currentTime = ZDateTime.UtcNow;

			var cartageLegData = CreateCartagesWithDifferentParents();
			var milestone1 = cartageLegData["STC"].WorkflowItems.Milestones.AddNew();
			var milestone2 = cartageLegData["CFC"].WorkflowItems.Milestones.AddNew();
			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(currentTime));
			milestone2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(currentTime.AddMonths(6)));
			Factory.Save();

			var cartageLegs = GetCartagesWithMilestoneDateFilter(-1, 3);

			AssertContainsExactElementsInAnyOrder("Stand-alone Cartage is in Collection", new[] { cartageLegData["STC"] }, cartageLegs);
		}

		[TestDate(2023, 5, 12)]
		public void TestMilestoneCompletedFilter()
		{
			var currentTime = ZDateTime.UtcNow;

			var cartageLegData = CreateCartagesWithDifferentParents();
			var milestone1 = cartageLegData["STC"].WorkflowItems.Milestones.AddNew();
			var milestone2 = cartageLegData["CFC"].WorkflowItems.Milestones.AddNew();
			milestone1.SetMilestoneActualDateForTest(currentTime.AddDays(-1));
			milestone2.SetMilestoneActualDateForTest(ZDateTime.Empty);
			Factory.Save();

			var cartageLegs = GetCartagesWithMilestoneCompletedFilter("Completed");

			AssertContainsExactElementsInAnyOrder("Only returns milestones with a non-empty actual date completed", new[] { cartageLegData["STC"] }, cartageLegs);
		}

		[TestDate(2023, 5, 12)]
		public void TestMilestoneNextFilter()
		{
			var currentTime = ZDateTime.UtcNow;

			var cartageLegData = CreateCartagesWithDifferentParents();
			var milestone1 = cartageLegData["STC"].WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = "AID";
			var milestone2 = cartageLegData["CFC"].WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = "DEP";
			milestone1.P9_Type = "MIL";
			milestone2.P9_Type = "MIL";
			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(currentTime));
			milestone2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(currentTime.AddDays(1)));
			Factory.Save();

			var cartageLegs = GetCartagesWithMilestoneNextFilter(-1, 3, "");

			AssertContainsExactElementsInAnyOrder("Next scheduled milestones are in Collection", new[] { cartageLegData["STC"], cartageLegData["CFC"] }, cartageLegs);

			cartageLegs = GetCartagesWithMilestoneNextFilter(-1, 3, "DEP");

			AssertContainsExactElementsInAnyOrder("Stand-alone Cartage with milestone type DEP is in Collection", new[] { cartageLegData["CFC"] }, cartageLegs);
		}

		[TestDate(2023, 5, 12)]
		public void TestMilestoneLastCompletedFilter()
		{
			var currentTime = ZDateTime.UtcNow;

			var cartageLegData = CreateCartagesWithDifferentParents();
			var milestone1 = cartageLegData["STC"].WorkflowItems.Milestones.AddNew();
			var milestone2 = cartageLegData["CFC"].WorkflowItems.Milestones.AddNew();
			milestone1.P9_Status = "LST";
			milestone2.P9_Status = "LST";
			milestone1.SetMilestoneActualDateForTest(currentTime);
			milestone2.SetMilestoneActualDateForTest(currentTime.AddMonths(6));
			Factory.Save();

			var cartageLegs = GetCartagesWithMilestoneLastCompletedFilter(-1, 3, "");

			AssertContainsExactElementsInAnyOrder("Stand-alone Cartage is in Collection", new[] { cartageLegData["STC"] }, cartageLegs);
		}

		public void TestProfitLossReasonFilterWithOperators()
		{
			var cartage1 = Factory.New<CommonCartage>();
			var cartage2 = Factory.New<CommonCartage>();
			var cartage3 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = GlbBranch.CurrentBranch.PK;
			cartage2.JJ_GB = GlbBranch.CurrentBranch.PK;
			cartage3.JJ_GB = GlbBranch.CurrentBranch.PK;

			var job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job1.JH_ParentID = cartage1.PK;
			job1.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
			job1.JH_GB = GlbBranch.CurrentBranch.PK;
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job1.JH_ProfitLossReasonCode = "ND1";
			job1.Parent = cartage1;

			var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job2.JH_ParentID = cartage2.PK;
			job2.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
			job2.JH_GB = GlbBranch.CurrentBranch.PK;
			job2.JH_GC = GlbCompany.CurrentCompany.PK;
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job2.JH_ProfitLossReasonCode = "CD1";
			job2.Parent = cartage2;

			var job3 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job3.JH_ParentID = cartage3.PK;
			job3.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
			job3.JH_GB = GlbBranch.CurrentBranch.PK;
			job3.JH_GC = GlbCompany.CurrentCompany.PK;
			job3.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job3.JH_ProfitLossReasonCode = string.Empty;
			job3.Parent = cartage3;

			Factory.Save();

			var filterBO = new CartageFilterBusinessObject();
			var profitLossReasonFilter = (ModuleTextFilter)filterBO["Profit/Loss Reason"];
			profitLossReasonFilter.IsActive = true;

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			profitLossReasonFilter.Property = "ND1";

			var cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { cartage1 }, cartages);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			profitLossReasonFilter.Property = "N";

			cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { cartage1 }, cartages);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			profitLossReasonFilter.Property = "D";

			cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { cartage1, cartage2 }, cartages);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			profitLossReasonFilter.Property = "N";

			cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { cartage2, cartage3 }, cartages);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			profitLossReasonFilter.Property = "N";

			cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { cartage2, cartage3 }, cartages);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			profitLossReasonFilter.Property = "ND1";

			cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { cartage2, cartage3 }, cartages);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			profitLossReasonFilter.Property = "ND1";

			cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { cartage3 }, cartages);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			profitLossReasonFilter.Property = "ND1";

			cartages = new CommonCartageCollection(Factory);
			cartages.AdditionalFilter = filterBO.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { cartage1, cartage2 }, cartages);
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CartageFilterBusinessObject();
		}

		protected void ResetFilterStripBizO()
		{
			fFilterStripBizO = null;
		}

		protected FilterStripBusinessObject FilterStripBizO
		{
			get
			{
				return fFilterStripBizO ?? (fFilterStripBizO = GetNewFilterStripBusinessObject());
			}
		}

		FilterStripBusinessObject fFilterStripBizO;
		FilterStripAsserter<CommonCartage> Asserter
		{
			get
			{
				return asserter ?? (asserter = new FilterStripAsserter<CommonCartage>(Factory, c => c.JJ_ConsignmentID));
			}
		}

		FilterStripAsserter<CommonCartage> asserter;
	}
}
