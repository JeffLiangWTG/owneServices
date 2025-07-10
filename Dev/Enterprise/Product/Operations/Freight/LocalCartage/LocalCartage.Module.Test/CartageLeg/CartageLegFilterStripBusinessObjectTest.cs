using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Freight.Integration.CFS;
using static Enterprise.Integration.Customs;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	[TestedType(typeof(CartageLegFilterStripBusinessObject))]
	public class CartageLegFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestContainerNo()
		{
			TestContainerTextFilter("Container #", JobContainerSchema.JC_ContainerNum);
		}

		public void TestSlotReference()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			// departure cartage
			var departureCartage = Factory.New<CommonCartage>();
			departureCartage.JJ_GB = branch.PK;
			var departureMove = departureCartage.ContainerBookedMoves.AddNew();
			var departureContainer = departureMove.Container;
			var departureLegEmpty = departureMove.CartageLegs.AddNew();
			var departureLegFull = departureMove.CartageLegs.AddNew();
			Helper.CreateAndAssignAddresses(departureLegEmpty, departureLegFull, DocAddressType.LocalCartageYard, DocAddressType.LocalCartageExporter, DocAddressType.LocalCartageCTO);
			// arrival cartage
			var arrivalCartage = Factory.New<CommonCartage>();
			arrivalCartage.JJ_GB = branch.PK;
			var arrivalMove = arrivalCartage.ContainerBookedMoves.AddNew();
			var arrivalContainer = arrivalMove.Container;
			var arrivalLegFull = arrivalMove.CartageLegs.AddNew();
			var arrivalLegEmpty = arrivalMove.CartageLegs.AddNew();
			Helper.CreateAndAssignAddresses(arrivalLegFull, arrivalLegEmpty, DocAddressType.LocalCartageCTO, DocAddressType.LocalCartageImporter, DocAddressType.LocalCartageYard);
			Factory.Save();
			var now = ZDateTime.Now;
			var filters = new CartageLegFilterStripBusinessObject();
			var filter = ((ModuleTextFilter)filters["Slot Reference"]);
			filter.Property = "Hello";
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			var collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filters.Filter;
			AssertContainsExactElementsInAnyOrder(Array.Empty<CommonCartageLeg>(), collection);
			departureContainer.JC_DepartureSlotReference = "Hello Bob";
			Factory.Save();
			collection.AdditionalFilter = filters.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { departureLegFull }, collection);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection.AdditionalFilter = filters.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { departureLegFull, arrivalLegFull }, collection);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			collection.AdditionalFilter = filters.Filter;
			AssertContainsExactElementsInAnyOrder(Array.Empty<CommonCartageLeg>(), collection);
			arrivalContainer.JC_ArrivalSlotReference = "Hello Tom";
			Factory.Save();
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			collection.AdditionalFilter = filters.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { departureLegFull, arrivalLegFull }, collection);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			collection.AdditionalFilter = filters.Filter;
			AssertContainsExactElementsInAnyOrder(Array.Empty<CommonCartageLeg>(), collection);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			collection.AdditionalFilter = filters.Filter;
			AssertContainsExactElementsInAnyOrder(Array.Empty<CommonCartageLeg>(), collection);
			filter.Property = "Hello Bob";
			collection.AdditionalFilter = filters.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { departureLegFull }, collection);
			filter.Property = "Hello Tom";
			collection.AdditionalFilter = filters.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { arrivalLegFull }, collection);
		}

		public void TestWorkSheetNo()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = branch.PK;
			var booked1 = cartage1.LooseBookedMoves.AddNew();
			var cartageLeg1 = booked1.CartageLegs.AddNew();
			var workSheet1 = Factory.New<CommonWorkSheet>();
			cartageLeg1.JU_EY_RunSheet = workSheet1.PK;
			var cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_GB = branch.PK;
			var booked2 = cartage2.LooseBookedMoves.AddNew();
			var cartageLeg2 = booked2.CartageLegs.AddNew();
			var workSheet2 = Factory.New<CommonWorkSheet>();
			cartageLeg2.JU_EY_RunSheet = workSheet2.PK;
			var cartage3 = Factory.New<CommonCartage>();
			cartage3.JJ_GB = branch.PK;
			var booked3 = cartage3.LooseBookedMoves.AddNew();
			var cartageLeg3 = booked3.CartageLegs.AddNew();
			workSheet1.EY_RunSheetNumber = "RS12345678";
			workSheet2.EY_RunSheetNumber = "RS87654321";
			Factory.Save();
			var filter = new CartageLegFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filter["RunSheet #"];
			textFilter.Property = "RS87654321";
			textFilter.IsActive = true;
			var collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartageLeg1, collection);
			AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartageLeg3, collection);
			textFilter.Property = "RS12345678";
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartageLeg2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartageLeg3, collection);
			textFilter.Property = "";
			textFilter.SqlComparisonOperator = Enterprise.ZArchitecture.Business.SpecialComparisonOperator.IsBlank;
			collection.AdditionalFilter = filter.Filter;
			AssertCollectionContains("Should have Cartage3", cartageLeg3, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartageLeg2, collection);
			AssertCollectionNotContains("Should not have Cartage1", cartageLeg1, collection);
		}

		public void TestCartageNo()
		{
			TestCartageTextFilter("Port Transport #", JobCartageSchema.JJ_ConsignmentID);
		}

		public void TestCartageOrderNo()
		{
			TestCartageTextFilter("Port Transport Order #", JobCartageSchema.JJ_OrderReferenceNumber);
		}

		public void TestCartageQuoteNo()
		{
			TestCartageTextFilter("Port Transport Quote #", JobCartageSchema.JJ_QuoteNumber);
		}

		public void TestCartageWaybillNo()
		{
			TestCartageTextFilter("Port Transport Waybill #", JobCartageSchema.JJ_WaybillNumber);
		}

		public void TestVoyageVesselFilter()
		{
			var vessel3 = Factory.New<RefVessel>();
			vessel3.RV_Name = "Black Beauty";
			var vessel4 = Factory.New<RefVessel>();
			vessel4.RV_Name = "Flying Dutchman";
			JobSailing sailing1 = Helper.CreateSailing(Helper.TestVessel1, "111", LocalCartageTestHelper.HomePort, LocalCartageTestHelper.OverseasPort, ZDateTime.Today);
			JobSailing sailing2 = Helper.CreateSailing(Helper.TestVessel1, "22211", LocalCartageTestHelper.HomePort, LocalCartageTestHelper.OverseasPort, ZDateTime.Today);
			JobSailing sailing3 = Helper.CreateSailing(vessel3, "1111", LocalCartageTestHelper.HomePort, LocalCartageTestHelper.OverseasPort, ZDateTime.Today);
			JobSailing sailing4 = Helper.CreateSailing(Helper.TestVessel2, "111", LocalCartageTestHelper.HomePort, LocalCartageTestHelper.OverseasPort, ZDateTime.Today);
			JobSailing sailing5 = Helper.CreateSailing(vessel4, "222", LocalCartageTestHelper.HomePort, LocalCartageTestHelper.OverseasPort, ZDateTime.Today);
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			CommonBookedCtgMove booked1 = cartage1.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg1 = booked1.CartageLegs.AddNew();
			cartage1.JJ_JX_Sailing = sailing1.PK;
			CommonCartage cartage2 = Factory.New<CommonCartage>();
			CommonBookedCtgMove booked2 = cartage2.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg2 = booked2.CartageLegs.AddNew();
			cartage2.JJ_JX_Sailing = sailing2.PK;
			CommonCartage cartage3 = Factory.New<CommonCartage>();
			CommonBookedCtgMove booked3 = cartage3.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg3 = booked3.CartageLegs.AddNew();
			cartage3.JJ_JX_Sailing = sailing3.PK;
			CommonCartage cartage4 = Factory.New<CommonCartage>();
			CommonBookedCtgMove booked4 = cartage4.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg4 = booked4.CartageLegs.AddNew();
			cartage4.JJ_JX_Sailing = sailing4.PK;
			CommonCartage cartage5 = Factory.New<CommonCartage>();
			CommonBookedCtgMove booked5 = cartage5.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg5 = booked5.CartageLegs.AddNew();
			cartage5.JJ_JX_Sailing = sailing5.PK;
			Factory.Save();
			CartageLegFilterStripBusinessObject filters = new CartageLegFilterStripBusinessObject();
			ModuleTextAndNkFilter filter = (ModuleTextAndNkFilter)filters[CartageLegFilterStripBusinessObject.FilterNameConstants.VoyageVessel];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "111";
			filter.IsActive = true;
			CommonCartageLegCollection cartageLegs = new CommonCartageLegCollection(Factory);
			cartageLegs.AdditionalFilter = filters.Filter; //fix these tests to test vessel comparitors
			AssertCollectionContains(cartageLeg1, cartageLegs);
			AssertCollectionNotContains(cartageLeg2, cartageLegs);
			AssertCollectionNotContains(cartageLeg3, cartageLegs);
			AssertCollectionContains(cartageLeg4, cartageLegs);
			AssertCollectionNotContains(cartageLeg5, cartageLegs);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionContains(cartageLeg1, cartageLegs);
			AssertCollectionNotContains(cartageLeg2, cartageLegs);
			AssertCollectionContains(cartageLeg3, cartageLegs);
			AssertCollectionContains(cartageLeg4, cartageLegs);
			AssertCollectionNotContains(cartageLeg5, cartageLegs);
			filter.NkProperty = "Fly";
			filter.Property = "";
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionNotContains(cartageLeg1, cartageLegs);
			AssertCollectionNotContains(cartageLeg2, cartageLegs);
			AssertCollectionNotContains(cartageLeg3, cartageLegs);
			AssertCollectionNotContains(cartageLeg4, cartageLegs);
			AssertCollectionContains(cartageLeg5, cartageLegs);
			filter.NkProperty = "eau";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionNotContains(cartageLeg1, cartageLegs);
			AssertCollectionNotContains(cartageLeg2, cartageLegs);
			AssertCollectionContains(cartageLeg3, cartageLegs);
			AssertCollectionNotContains(cartageLeg4, cartageLegs);
			AssertCollectionNotContains(cartageLeg5, cartageLegs);
			filter.NkProperty = "";
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionNotContains(cartageLeg1, cartageLegs);
			AssertCollectionNotContains(cartageLeg2, cartageLegs);
			AssertCollectionNotContains(cartageLeg3, cartageLegs);
			AssertCollectionNotContains(cartageLeg4, cartageLegs);
			AssertCollectionNotContains(cartageLeg5, cartageLegs);
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionContains(cartageLeg1, cartageLegs);
			AssertCollectionContains(cartageLeg2, cartageLegs);
			AssertCollectionContains(cartageLeg3, cartageLegs);
			AssertCollectionContains(cartageLeg4, cartageLegs);
			AssertCollectionContains(cartageLeg5, cartageLegs);
		}

		public void TestVesselVoyageFilter_MaxLength()
		{
			var filters = new CartageLegFilterStripBusinessObject();
			var filter = (ModuleTextAndNkFilter)filters[CartageLegFilterStripBusinessObject.FilterNameConstants.VoyageVessel];
			CombineAssertions(() =>
			{
				AssertEquals("Vessel Max Length", ViewLocalTransportScheduleSchema.VLT_RV_NKVessel.MaxLength, filter.NkMaxLength);
				AssertEquals("Voyage Max Length", ViewLocalTransportScheduleSchema.VLT_Voyage.MaxLength, filter.MaxLength);
			});
		}

		public void TestVesselVoyageFilter_CaptionAndDescription()
		{
			var filters = new CartageLegFilterStripBusinessObject();
			var filter = (ModuleTextAndNkFilter)filters[CartageLegFilterStripBusinessObject.FilterNameConstants.VoyageVessel];
			AssertEquals("Vessel and Flight/Voyage #", filter.Description);
			AssertEquals("Vessel and Flight/Voyage #", filter.MultilingualDescription);
		}

		public void TestContainerReleaseNumber()
		{
			TestContainerTextFilter("Container Release #", JobContainerSchema.JC_ReleaseNum);
		}

		public void TestContainerStatus()
		{
			TestContainerTextFilter("Container Status", JobContainerSchema.JC_ContainerStatus);
		}

		public void TestPickupETD()
		{
			TestCartageLegDateFilter(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.PlannedPickup, JobContainerLegsSchema.JU_PlannedPickupTime);
		}

		public void TestPickupTimeIn()
		{
			TestCartageLegDateFilter(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ActualPickupTimeIn, JobContainerLegsSchema.JU_PickupTimeIn);
		}

		public void TestPickupTimeOut()
		{
			TestCartageLegDateFilter(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ActualPickupTimeOut, JobContainerLegsSchema.JU_PickupTimeOut);
		}

		public void TestDeliveryETA()
		{
			TestCartageLegDateFilter(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.PlannedDelivery, JobContainerLegsSchema.JU_EstimatedDeliveryTime);
		}

		public void TestDeliveryTimeIn()
		{
			TestCartageLegDateFilter(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ActualDeliveryTimeIn, JobContainerLegsSchema.JU_DeliverTimeIn);
		}

		public void TestDeliveryTimeOut()
		{
			TestCartageLegDateFilter(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ActualDeliveryTimeOut, JobContainerLegsSchema.JU_DeliverTimeOut);
		}

		public void TestCartageRegistrationDate()
		{
			TestCartageDateFilter("Port Transport Registration Date", JobCartageSchema.JJ_SystemCreateTimeUtc, requiresUtcAdjustment: true);
		}

		public void TestCartageEstimatedPickup()
		{
			TestCartageDateFilter(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.LocalTransportPlannedPickup, JobCartageSchema.JJ_EstimatedPickup);
		}

		public void TestCartageEstimatedDelivery()
		{
			TestCartageDateFilter(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.LocalTransportPlannedDelivery, JobCartageSchema.JJ_EstimatedDelivery);
		}

		public void TestSlotDate()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			// departure cartage
			var departureCartage = Factory.New<CommonCartage>();
			departureCartage.JJ_GB = branch.PK;
			var departureMove = departureCartage.ContainerBookedMoves.AddNew();
			var departureContainer = departureMove.Container;
			var departureLegEmpty = departureMove.CartageLegs.AddNew();
			var departureLegFull = departureMove.CartageLegs.AddNew();
			Helper.CreateAndAssignAddresses(departureLegEmpty, departureLegFull, DocAddressType.LocalCartageYard, DocAddressType.LocalCartageExporter, DocAddressType.LocalCartageCTO);
			// arrival cartage
			var arrivalCartage = Factory.New<CommonCartage>();
			arrivalCartage.JJ_GB = branch.PK;
			var arrivalMove = arrivalCartage.ContainerBookedMoves.AddNew();
			var arrivalContainer = arrivalMove.Container;
			var arrivalLegFull = arrivalMove.CartageLegs.AddNew();
			var arrivalLegEmpty = arrivalMove.CartageLegs.AddNew();
			Helper.CreateAndAssignAddresses(arrivalLegFull, arrivalLegEmpty, DocAddressType.LocalCartageCTO, DocAddressType.LocalCartageImporter, DocAddressType.LocalCartageYard);
			Factory.Save();
			var now = ZDateTime.Now;
			var filters = new CartageLegFilterStripBusinessObject();
			var filter = ((ModuleDateFilter)filters["Slot Date"]);
			filter.Property1 = now.AddDays(6);
			filter.Property2 = now.AddDays(8);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			var collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filters.Filter;
			AssertContainsExactElementsInAnyOrder(Array.Empty<CommonCartageLeg>(), collection);
			departureContainer.JC_DepartureSlotDateTime = now.AddDays(7);
			Factory.Save();
			collection.AdditionalFilter = filters.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { departureLegFull }, collection);
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			collection.AdditionalFilter = filters.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { arrivalLegFull }, collection);
			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			collection.AdditionalFilter = filters.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { departureLegFull }, collection);
			arrivalContainer.JC_ArrivalSlotDateTime = now.AddDays(4);
			Factory.Save();
			filter.Property1 = now.AddDays(6);
			filter.Property2 = now.AddDays(8);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			collection.AdditionalFilter = filters.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { departureLegFull }, collection);
			filter.Property1 = now.AddDays(3);
			filter.Property2 = now.AddDays(8);
			collection.AdditionalFilter = filters.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { departureLegFull, arrivalLegFull }, collection);
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			collection.AdditionalFilter = filters.Filter;
			AssertContainsExactElementsInAnyOrder(Array.Empty<CommonCartageLeg>(), collection);
			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			collection.AdditionalFilter = filters.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { departureLegFull, arrivalLegFull }, collection);
		}

		public void TestContainerEmptyReturnedOn()
		{
			TestContainerDateFilter("Container Empty Returned On", JobContainerSchema.JC_ContainerYardEmptyReturnGateIn);
		}

		public void TestContainerEmptyRequired()
		{
			TestContainerDateFilter("Container Empty Required", JobContainerSchema.JC_EmptyRequired);
		}

		public void TestContainerEmptyReturnBy()
		{
			TestContainerDateFilter("Container Empty Return By", JobContainerSchema.JC_EmptyReturnedBy);
		}

		public void TestSailingDates()
		{
			TestSailingDateFilter(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleETA, JobVoyDestinationSchema.JB_E_ARV);
			TestSailingDateFilter(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleATA, JobVoyDestinationSchema.JB_A_ARV);
			TestSailingDateFilter(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleETD, JobVoyOriginSchema.JA_E_DEP);
			TestSailingDateFilter(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleATD, JobVoyOriginSchema.JA_A_DEP);
			TestSailingDateFilter(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleFCLAvailability, JobVoyDestinationSchema.JB_AvailabilityDate);
			TestSailingDateFilter(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleFCLStorage, JobVoyDestinationSchema.JB_StorageDate);
			TestSailingDateFilter(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleFCLCutOff, JobVoyOriginSchema.JA_CutOff);
			TestSailingDateFilter(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleFCLReceivalCommences, JobVoyOriginSchema.JA_ReceivalCommences);
			TestSailingDateFilter(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleLCLAvailability, JobSailingSchema.JX_DepotAvailabilityDate);
			TestSailingDateFilter(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleLCLStorage, JobSailingSchema.JX_DepotStorageDate);
			TestSailingDateFilter(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleLCLCutOff, JobSailingSchema.JX_DepotCutOff);
			TestSailingDateFilter(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleLCLReceivalCommences, JobSailingSchema.JX_DepotReceivalCommences);
		}

		public void TestScheduleForTransportJobLinkingToTransportBooking_DirectSailing()
		{
			AssertScheduleFiltersForTransportBooking(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleETA, JobVoyDestinationSchema.JB_E_ARV);
			AssertScheduleFiltersForTransportBooking(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleATA, JobVoyDestinationSchema.JB_A_ARV);
			AssertScheduleFiltersForTransportBooking(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleETD, JobVoyOriginSchema.JA_E_DEP);
			AssertScheduleFiltersForTransportBooking(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleATD, JobVoyOriginSchema.JA_A_DEP);
			AssertScheduleFiltersForTransportBooking(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleFCLAvailability, JobVoyDestinationSchema.JB_AvailabilityDate);
			AssertScheduleFiltersForTransportBooking(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleFCLStorage, JobVoyDestinationSchema.JB_StorageDate);
			AssertScheduleFiltersForTransportBooking(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleFCLCutOff, JobVoyOriginSchema.JA_CutOff);
			AssertScheduleFiltersForTransportBooking(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleFCLReceivalCommences, JobVoyOriginSchema.JA_ReceivalCommences);
			AssertScheduleFiltersForTransportBooking(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleLCLAvailability, JobSailingSchema.JX_DepotAvailabilityDate);
			AssertScheduleFiltersForTransportBooking(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleLCLStorage, JobSailingSchema.JX_DepotStorageDate);
			AssertScheduleFiltersForTransportBooking(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleLCLCutOff, JobSailingSchema.JX_DepotCutOff);
			AssertScheduleFiltersForTransportBooking(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleLCLReceivalCommences, JobSailingSchema.JX_DepotReceivalCommences);
		}

		void AssertScheduleFiltersForTransportBooking(ZString filterName, SchemaColumn schemaColumn)
		{
			voyageNumber++;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "abc" + voyageNumber.ToString();
			var today = ZDateTime.Now;
			var sailing = Helper.CreateSailing(vessel, "123", "NZAKL", "AUSYD", ZDateTime.Empty);
			switch (schemaColumn.TableName)
			{
				case JobVoyOriginSchema.Constants.TableName:
					sailing.Origin[schemaColumn.Name] = today;
					break;
				case JobVoyDestinationSchema.Constants.TableName:
					sailing.Destination[schemaColumn.Name] = today;
					break;
				case JobSailingSchema.Constants.TableName:
					sailing[schemaColumn.Name] = today;
					break;
			}

			var parentBookingConsolidation = Factory.New<IDtbBookingConsolidation>();
			var parentBooking = Factory.New<IDtbBooking>();
			parentBooking.KM_KB_Booking = parentBookingConsolidation.PK;
			var cartage = Factory.New<CommonCartage>();
			var bookedMove = cartage.LooseBookedMoves.AddNew();
			var cartageLeg = bookedMove.CartageLegs.AddNew();
			cartage.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			cartage.JJ_ParentID = parentBooking.PK;
			cartage.JJ_JX_Sailing = sailing.PK;
			Factory.Save();
			var filters = new CartageLegFilterStripBusinessObject();
			var filter = (ModuleDateFilter)filters[filterName];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = today.AddDays(-1);
			AssertEquals("ScheduleFilterSubGroup", filter.SubGroup.GetType().Name);
			var cartageLegs = new CommonCartageLegCollection(Factory);
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionContains(cartageLeg, cartageLegs);
			filter.Property2 = today.AddDays(-1);
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionNotContains(cartageLeg, cartageLegs);
			filter.Property2 = today.AddDays(1);
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionContains(cartageLeg, cartageLegs);
			filter.Property1 = today.AddDays(1);
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionNotContains(cartageLeg, cartageLegs);
			filter.Property1 = ZDateTime.Empty;
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionContains(cartageLeg, cartageLegs);
		}

		static int voyageNumber;

		public void TestScheduleForTransportJobLinkingToTransportBooking_ETA()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "abc";
			var sailing = Helper.CreateSailing(vessel, "123", "NZAKL", "AUSYD", ZDateTime.Empty);
			sailing.Destination.JB_E_ARV = ZDateTime.Now.AddDays(2);
			AssertScheduleFiltersForShipmentTransportBooking(sailing, JobConsolTransportSchema.JW_ETA, CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleETA);
		}

		public void TestScheduleForTransportJobLinkingToTransportBooking_ATA()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "abc";
			var sailing = Helper.CreateSailing(vessel, "123", "NZAKL", "AUSYD", ZDateTime.Empty);
			sailing.Destination.JB_A_ARV = ZDateTime.Now.AddDays(2);
			AssertScheduleFiltersForShipmentTransportBooking(sailing, JobConsolTransportSchema.JW_ATA, CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleATA);
		}

		public void TestScheduleForTransportJobLinkingToTransportBooking_ETD()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "abc";
			var sailing = Helper.CreateSailing(vessel, "123", "NZAKL", "AUSYD", ZDateTime.Empty);
			sailing.Origin.JA_E_DEP = ZDateTime.Now.AddDays(2);
			AssertScheduleFiltersForShipmentTransportBooking(sailing, JobConsolTransportSchema.JW_ETD, CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleETD);
		}

		public void TestScheduleForTransportJobLinkingToTransportBooking_ATD()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "abc";
			var sailing = Helper.CreateSailing(vessel, "123", "NZAKL", "AUSYD", ZDateTime.Empty);
			sailing.Origin.JA_A_DEP = ZDateTime.Now.AddDays(2);
			AssertScheduleFiltersForShipmentTransportBooking(sailing, JobConsolTransportSchema.JW_ATD, CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleATD);
		}

		void AssertScheduleFiltersForShipmentTransportBooking(JobSailing sailing, SchemaDateTimeColumn schemaColumn, string filterName)
		{
			var today = ZDateTime.Now;
			var shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var parentBookingConsolidation = Factory.New<IDtbBookingConsolidation>();
			parentBookingConsolidation.KB_ParentID = shipment.PK;
			parentBookingConsolidation.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			var parentBooking = Factory.New<IDtbBooking>();
			parentBooking.KM_KB_Booking = parentBookingConsolidation.PK;
			var cartage = Factory.New<CommonCartage>();
			var bookedMove = cartage.LooseBookedMoves.AddNew();
			var cartageLeg = bookedMove.CartageLegs.AddNew();
			cartage.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			cartage.JJ_ParentID = parentBooking.PK;
			var routingInfo = shipment.Transports.AddNew();
			routingInfo[JobConsolTransportSchema.JW_IsLinked] = true;
			routingInfo[JobConsolTransportSchema.JW_JX] = sailing.PK;
			Factory.Save();
			var filters = new CartageLegFilterStripBusinessObject();
			var filter = (ModuleDateFilter)filters[filterName];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = today.AddDays(-1);
			filter.Property2 = today.AddDays(3);
			var cartageLegs = new CommonCartageLegCollection(Factory);
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionContains(cartageLeg, cartageLegs);
			filter.Property1 = today.AddDays(5);
			filter.Property2 = today.AddDays(6);
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertEquals(0, cartageLegs.Count);
		}

		public void TestScheduleFCLAvailabilityFilterForTransportBooking_Imports()
		{
			AssertScheduleFiltersForDeclarationTransportBooking(JobVoyDestinationSchema.JB_AvailabilityDate, CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleFCLAvailability, "IMP");
		}

		public void TestScheduleFCLStorageFilterForTransportBooking_Imports()
		{
			AssertScheduleFiltersForDeclarationTransportBooking(JobVoyDestinationSchema.JB_StorageDate, CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleFCLStorage, "IMP");
		}

		public void TestScheduleFCLAvailabilityFilterForTransportBooking_Exports()
		{
			AssertScheduleFiltersForDeclarationTransportBooking(JobVoyDestinationSchema.JB_AvailabilityDate, CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleFCLAvailability, "EXP");
		}

		public void TestScheduleFCLStorageFilterForTransportBooking_Exports()
		{
			AssertScheduleFiltersForDeclarationTransportBooking(JobVoyDestinationSchema.JB_StorageDate, CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleFCLStorage, "EXP");
		}

		void AssertScheduleFiltersForDeclarationTransportBooking(SchemaDateTimeColumn schemaColumn, string filterName, string direction)
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "abc";
			var today = ZDateTime.Now;
			var sailing = Helper.CreateSailing(vessel, "123", "NZAKL", "AUSYD", ZDateTime.Empty);
			sailing.Destination[schemaColumn] = today.AddDays(2);
			var declaration = CreateDeclaration(sailing, direction);
			declaration.RunPreSaveValidation();
			Factory.Save();
			var parentBookingConsolidation = Factory.New<IDtbBookingConsolidation>();
			parentBookingConsolidation.KB_ParentID = declaration.PK;
			parentBookingConsolidation.KB_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			parentBookingConsolidation.KB_JobDirection = direction == "IMP" ? "PIC" : "DLV";
			var parentBooking = Factory.New<IDtbBooking>();
			parentBooking.KM_KB_Booking = parentBookingConsolidation.PK;
			var cartage = Factory.New<CommonCartage>();
			var bookedMove = cartage.LooseBookedMoves.AddNew();
			var cartageLeg = bookedMove.CartageLegs.AddNew();
			cartage.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			cartage.JJ_ParentID = parentBooking.PK;
			Factory.Save();
			var filters = new CartageLegFilterStripBusinessObject();
			var filter = (ModuleDateFilter)filters[filterName];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = today.AddDays(-1);
			filter.Property2 = today.AddDays(3);
			var cartageLegs = new CommonCartageLegCollection(Factory);
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionContains(cartageLeg, cartageLegs);
			filter.Property1 = today.AddDays(5);
			filter.Property2 = today.AddDays(6);
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertEquals(0, cartageLegs.Count);
		}

		public void TestScheduleFCLAvailabilityFilterForCustoms_Imports()
		{
			AssertFCLAvailabilityAndStorageFiltersForCustoms(JobVoyDestinationSchema.JB_AvailabilityDate, CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleFCLAvailability, "IMP");
		}

		public void TestScheduleFCLStorageFilterForCustoms_Imports()
		{
			AssertFCLAvailabilityAndStorageFiltersForCustoms(JobVoyDestinationSchema.JB_StorageDate, CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleFCLStorage, "IMP");
		}

		public void TestScheduleFCLAvailabilityFilterForCustoms_Exports()
		{
			AssertFCLAvailabilityAndStorageFiltersForCustoms(JobVoyDestinationSchema.JB_AvailabilityDate, CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleFCLAvailability, "EXP");
		}

		public void TestScheduleFCLStorageFilterForCustoms_Exports()
		{
			AssertFCLAvailabilityAndStorageFiltersForCustoms(JobVoyDestinationSchema.JB_StorageDate, CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ScheduleFCLStorage, "EXP");
		}

		void AssertFCLAvailabilityAndStorageFiltersForCustoms(SchemaDateTimeColumn schemaColumn, string filterName, string direction)
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "abc";
			var now = ZDateTime.Now;
			var sailing = Helper.CreateSailing(vessel, "123", "NZAKL", "AUSYD", ZDateTime.Empty);
			sailing.Destination[schemaColumn] = now.AddDays(2);
			var declaration = CreateDeclaration(sailing, direction);
			declaration.RunPreSaveValidation();
			Factory.Save();
			var cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove bookedMove = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg = bookedMove.CartageLegs.AddNew();
			cartage.JJ_JX_Sailing = sailing.PK;
			cartage.JJ_ParentID = declaration.PK;
			cartage.JJ_ParentTableCode = declaration.TablePrefix;
			cartage.JJ_ConsignmentID = string.Format("CON1/{0}", direction.Substring(0, 1));
			Factory.Save();
			var filters = new CartageLegFilterStripBusinessObject();
			var filter = (ModuleDateFilter)filters[filterName];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = now.AddDays(-1);
			filter.Property2 = now.AddDays(3);
			var cartageLegs = new CommonCartageLegCollection(Factory);
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionContains(cartageLeg, cartageLegs);
			filter.Property1 = now.AddDays(5);
			filter.Property2 = now.AddDays(6);
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertEquals(0, cartageLegs.Count);
		}

		BusinessObject CreateDeclaration(JobSailing sailing, string direction)
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_TransportMode] = Constants.TransportModes.Sea;
			declaration[JobDeclarationSchema.JE_MessageType] = direction;
			declaration[JobDeclarationSchema.JE_OH_ShippingLine] = shippingLine.PK;
			declaration[JobDeclarationSchema.JE_VesselName] = sailing.JX_JV_NKVessel;
			declaration[JobDeclarationSchema.JE_VoyageFlightNo] = sailing.JX_JV_VoyageFlight;
			declaration[JobDeclarationSchema.JE_RL_NKPortOfLoading] = "NZAKL";
			declaration[JobDeclarationSchema.JE_RL_NKPortOfArrival] = "AUSYD";
			declaration[JobDeclarationSchema.JE_DateOfArrival] = ZDateTime.Now;
			var routingInfo = (TransportCollection)declaration["Transports"];
			AssertEquals("Precondition: declaration.Transports.Count", 1, routingInfo.Count);
			var routingObj = routingInfo[0];
			routingObj[JobConsolTransportSchema.JW_JX] = sailing.PK;
			routingObj[JobConsolTransportSchema.JW_IsLinked] = true;
			return declaration;
		}

		public void TestRequestedPickup_Start()
		{
			TestBookedCtgMoveDateFilter("Pickup Requested", JobBookedCtgMoveSchema.EW_RequestedPickupTimeStart);
		}

		public void TestRequestedPickup_End()
		{
			TestBookedCtgMoveDateFilter("Pickup Requested To", JobBookedCtgMoveSchema.EW_RequestedPickupTimeEnd);
		}

		public void TestRequestedDelivery_Start()
		{
			TestBookedCtgMoveDateFilter("Delivery Requested", JobBookedCtgMoveSchema.EW_RequestedDeliveryTimeStart);
		}

		public void TestRequestedDelivery_End()
		{
			TestBookedCtgMoveDateFilter("Delivery Requested To", JobBookedCtgMoveSchema.EW_RequestedDeliveryTimeEnd);
		}

		public void TestVehicleRegistration()
		{
			TestWorkSheetTextFilter("Vehicle Registration", JobCartageRunSheetSchema.EY_TruckRegistration);
		}

		public void TestTransportCompanyName()
		{
			TestWorkSheetTextFilter("Transport Company Name (Non-Org)", JobCartageRunSheetSchema.EY_TransportCoName);
		}

		public void TestDriversName()
		{
			TestWorkSheetTextFilter("Drivers Name (Non-Staff)", JobCartageRunSheetSchema.EY_DriversName);
		}

		public void TestDriversLicence()
		{
			TestWorkSheetTextFilter("Drivers Licence (Non-Staff)", JobCartageRunSheetSchema.EY_DriversLicence);
		}

		public void TestStaffDriver()
		{
			GlbStaff bob = Factory.New<GlbStaff>();
			bob.GS_Code = "BB1";
			bob.GS_LoginName = "Bob111";
			GlbStaff david = Factory.New<GlbStaff>();
			david.GS_Code = "DD2";
			david.GS_LoginName = "David222";
			TestWorkSheetNkFilter("Staff Driver", JobCartageRunSheetSchema.EY_GS_NKTruckDriver, bob.GS_Code, david.GS_Code);
		}

		public void TestStaffDriver_IsBlankOrNot()
		{
			var bob = Factory.New<GlbStaff>();
			bob.GS_Code = "BB1";
			bob.GS_LoginName = "Bob111";
			var david = Factory.New<GlbStaff>();
			david.GS_Code = "DD2";
			david.GS_LoginName = "David222";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = branch.PK;
			var booked1 = cartage1.LooseBookedMoves.AddNew();
			var cartageLeg11 = booked1.CartageLegs.AddNew();
			var cartageLeg12 = booked1.CartageLegs.AddNew();
			var cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_GB = branch.PK;
			var booked2 = cartage2.LooseBookedMoves.AddNew();
			var cartageLeg21 = booked2.CartageLegs.AddNew();
			var cartageLeg22 = booked2.CartageLegs.AddNew();
			var workSheet2 = Factory.New<CommonWorkSheet>();
			cartageLeg21.JU_EY_RunSheet = workSheet2.PK;
			cartageLeg22.JU_EY_RunSheet = workSheet2.PK;
			var cartage3 = Factory.New<CommonCartage>();
			cartage3.JJ_GB = branch.PK;
			var booked3 = cartage3.LooseBookedMoves.AddNew();
			var cartageLeg31 = booked3.CartageLegs.AddNew();
			var cartageLeg32 = booked3.CartageLegs.AddNew();
			var workSheet3 = Factory.New<CommonWorkSheet>();
			cartageLeg31.JU_EY_RunSheet = workSheet3.PK;
			cartageLeg32.JU_EY_RunSheet = workSheet3.PK;
			workSheet3[JobCartageRunSheetSchema.EY_GS_NKTruckDriver.Name] = david.GS_Code;
			Factory.Save();
			var filter = new CartageLegFilterStripBusinessObject();
			((ModuleNkFilter)filter["Staff Driver"]).SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			((ModuleNkFilter)filter["Staff Driver"]).IsActive = true;
			var collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { cartageLeg11, cartageLeg12, cartageLeg21, cartageLeg22 }, collection);
			((ModuleNkFilter)filter["Staff Driver"]).SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { cartageLeg31, cartageLeg32 }, collection);
		}

		public void TestTransportCompany()
		{
			var bobsCo = Helper.CreateOrgHeader("bobsCo", "bobsCo Address");
			var davidsCo = Helper.CreateOrgHeader("davidsCo", "davidsCo Address");
			TestWorkSheetGuidFilter("Transport Company", JobCartageRunSheetSchema.EY_OH_TransportCo, bobsCo.PK, davidsCo.PK);
			var filter = new CartageLegFilterStripBusinessObject();
			AssertEquals("Should be of type LocalTransportCollection.", typeof(LocalTransportCollection), ((ModuleGuidFilter)filter["Transport Company"]).List.GetType());
		}

		public void TestTransportCompany_IsBlank()
		{
			// no transport co (not linking to runsheet)
			var legNotLinkToRunSheet = CreateCommonCartageLeg();
			// no transport co (linking to runsheet but runsheet has no transport co)
			var runSheetNoTransportCo = Factory.New<CommonWorkSheet>();
			var legHasRunSheetButNoTransportCo = CreateCommonCartageLeg(runSheetNoTransportCo);
			// has transport co
			var transportCompany = Helper.CreateOrgHeader("TransCo", "Transport Company Address");
			var runSheetHasTransportCo = Factory.New<CommonWorkSheet>();
			runSheetHasTransportCo.EY_OH_TransportCo = transportCompany.PK;
			var legHasTransportCo = CreateCommonCartageLeg(runSheetHasTransportCo);
			Factory.Save();
			var filterObj = new CartageLegFilterStripBusinessObject();
			var transportCompanyFilter = (ModuleGuidFilter)filterObj["Transport Company"];
			transportCompanyFilter.IsActive = true;
			transportCompanyFilter.Property = ZGuid.Empty;
			transportCompanyFilter.ComparisonOperator = "is blank";
			var collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filterObj.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { legNotLinkToRunSheet, legHasRunSheetButNoTransportCo }, collection);
		}

		public void TestTransportCompany_IsNotBlank()
		{
			// no transport co (not linking to runsheet)
			var legNotLinkToRunSheet = CreateCommonCartageLeg();
			// no transport co (linking to runsheet but runsheet has no transport co)
			var runSheetNoTransportCo = Factory.New<CommonWorkSheet>();
			var legHasRunSheetButNoTransportCo = CreateCommonCartageLeg(runSheetNoTransportCo);
			// has transport co
			var transportCompany = Helper.CreateOrgHeader("TransCo", "Transport Company Address");
			var runSheetHasTransportCo = Factory.New<CommonWorkSheet>();
			runSheetHasTransportCo.EY_OH_TransportCo = transportCompany.PK;
			var legHasTransportCo = CreateCommonCartageLeg(runSheetHasTransportCo);
			Factory.Save();
			var filterObj = new CartageLegFilterStripBusinessObject();
			var transportCompanyFilter = (ModuleGuidFilter)filterObj["Transport Company"];
			transportCompanyFilter.IsActive = true;
			transportCompanyFilter.Property = ZGuid.Empty;
			transportCompanyFilter.ComparisonOperator = "is not blank";
			var collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filterObj.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { legHasTransportCo }, collection);
		}

		public void TestTransportCompany_ExactMatch()
		{
			// no transport co (not linking to runsheet)
			var legNoTransportCo = CreateCommonCartageLeg();
			// no transport co (linking to runsheet but runsheet has no transport co)
			var runSheetNoTransportCo = Factory.New<CommonWorkSheet>();
			var legHasRunSheetButNoTransportCo = CreateCommonCartageLeg(runSheetNoTransportCo);
			// has transport company
			var transportCompany1 = Helper.CreateOrgHeader("TransCo1", "Transport Company 1 Address");
			var workSheet1 = Factory.New<CommonWorkSheet>();
			workSheet1.EY_OH_TransportCo = transportCompany1.PK;
			var legHasTransportCo1 = CreateCommonCartageLeg(workSheet1);
			// has transport co
			var transportCompany2 = Helper.CreateOrgHeader("TransCo2", "Transport Company 2 Address");
			var workSheet2 = Factory.New<CommonWorkSheet>();
			workSheet2.EY_OH_TransportCo = transportCompany2.PK;
			var legHasTransportCo2 = CreateCommonCartageLeg(workSheet2);
			Factory.Save();
			var filterObj = new CartageLegFilterStripBusinessObject();
			var transportCompanyFilter = (ModuleGuidFilter)filterObj["Transport Company"];
			transportCompanyFilter.IsActive = true;
			transportCompanyFilter.Property = ZGuid.Empty;
			transportCompanyFilter.ComparisonOperator = "exact"; // exact empty
																 // filter's Property is empty, it means an empty filter, don't filter anything, should return everything.
			var collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filterObj.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { legNoTransportCo, legHasRunSheetButNoTransportCo, legHasTransportCo1, legHasTransportCo2 }, collection);
			transportCompanyFilter.Property = transportCompany1.PK; // exact match transportCompany1
			collection.AdditionalFilter = filterObj.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { legHasTransportCo1 }, collection);
			transportCompanyFilter.Property = transportCompany2.PK; // exact match transportCompany2
			collection.AdditionalFilter = filterObj.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { legHasTransportCo2 }, collection);
		}

		public void TestTransportCompany_NotEqual()
		{
			// no transport co (not linking to runsheet)
			var legNoTransportCo = CreateCommonCartageLeg();
			// no transport co (linking to runsheet but runsheet has no transport co)
			var runSheetNoTransportCo = Factory.New<CommonWorkSheet>();
			var legHasRunSheetButNoTransportCo = CreateCommonCartageLeg(runSheetNoTransportCo);
			// has transport company
			var transportCompany1 = Helper.CreateOrgHeader("TransCo1", "Transport Company Address 1");
			var workSheet1 = Factory.New<CommonWorkSheet>();
			workSheet1.EY_OH_TransportCo = transportCompany1.PK;
			var legHasTransportCo1 = CreateCommonCartageLeg(workSheet1);
			var transportCompany2 = Helper.CreateOrgHeader("TransCo2", "Transport Company Address 2");
			var workSheet2 = Factory.New<CommonWorkSheet>();
			workSheet2.EY_OH_TransportCo = transportCompany2.PK;
			var legHasTransportCo2 = CreateCommonCartageLeg(workSheet2);
			Factory.Save();
			var filterObj = new CartageLegFilterStripBusinessObject();
			var transportCompanyFilter = (ModuleGuidFilter)filterObj["Transport Company"];
			transportCompanyFilter.IsActive = true;
			transportCompanyFilter.Property = ZGuid.Empty;
			transportCompanyFilter.ComparisonOperator = "not equal";
			// filter's Property is empty, it means an empty filter, don't filter anything, should return everything.
			var collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filterObj.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { legNoTransportCo, legHasRunSheetButNoTransportCo, legHasTransportCo1, legHasTransportCo2 }, collection);
			transportCompanyFilter.Property = transportCompany1.PK;
			collection.AdditionalFilter = filterObj.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { legNoTransportCo, legHasRunSheetButNoTransportCo, legHasTransportCo2 }, collection);
			transportCompanyFilter.Property = transportCompany2.PK;
			collection.AdditionalFilter = filterObj.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { legNoTransportCo, legHasRunSheetButNoTransportCo, legHasTransportCo1 }, collection);
		}

		CommonCartageLeg CreateCommonCartageLeg(CommonWorkSheet runSheet = null)
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			if (runSheet != null)
			{
				leg.JU_EY_RunSheet = runSheet.PK;
			}

			return leg;
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

		public void TestContainerService()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = branch.PK;
			CommonBookedCtgMove booked1 = cartage1.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg1 = booked1.CartageLegs.AddNew();
			CommonCartageLeg cartageLeg2 = GetContainerCartageLeg(branch);
			CommonCartageLeg cartageLeg3 = GetContainerCartageLeg(branch);
			CommonCartageLeg cartageLeg4 = GetContainerCartageLeg(branch);
			JobService service1 = Factory.New<JobService>();
			JobService service2 = Factory.New<JobService>();
			JobService service3 = Factory.New<JobService>();
			service1.ES_ParentTableCode = JobContainerSchema.Constants.Prefix;
			service1.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			service1.ES_ParentID = cartageLeg2.Container.PK;
			service2.ES_ParentTableCode = JobContainerSchema.Constants.Prefix;
			service2.ES_ServiceCode = Constants.FreightServiceType.Codes.ExtraInspection;
			service2.ES_ParentID = cartageLeg3.Container.PK;
			service3.ES_ParentTableCode = JobScheduleChangeSchema.Constants.Prefix;
			service3.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			Factory.Save();
			CartageLegFilterStripBusinessObject filter = new CartageLegFilterStripBusinessObject();
			((ModuleTextFilter)filter["Container Service"]).Property = Constants.FreightServiceType.Codes.Cleaning;
			((ModuleTextFilter)filter["Container Service"]).IsActive = true;
			CommonCartageLegCollection collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have CartageLeg2", cartageLeg2, collection);
			((ModuleTextFilter)filter["Container Service"]).Property = Constants.FreightServiceType.Codes.ExtraInspection;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have CartageLeg3", cartageLeg3, collection);
			((ModuleTextFilter)filter["Container Service"]).Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ContainerServiceAny;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 2 CommonCartageLeg", 2, collection.Count);
			AssertCollectionContains("Should have CartageLeg2", cartageLeg2, collection);
			AssertCollectionContains("Should have CartageLeg3", cartageLeg3, collection);
			((ModuleTextFilter)filter["Container Service"]).Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ContainerServiceNone;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 2 CommonCartageLeg", 2, collection.Count);
			AssertCollectionContains("Should have CartageLeg1", cartageLeg1, collection);
			AssertCollectionContains("Should have CartageLeg4", cartageLeg4, collection);
			((ModuleTextFilter)filter["Container Service"]).Property = "";
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 4 CommonCartageLeg", 4, collection.Count);
		}

		CommonCartageLeg GetContainerCartageLeg(GlbBranch branch)
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_GB = branch.PK;
			CommonBookedCtgMove booked = cartage.ContainerBookedMoves.AddNew();
			CommonContainer container = booked.Container;
			CommonCartageLeg cartageLeg = booked.CartageLegs.AddNew();
			return cartageLeg;
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
			CartageLegFilterStripBusinessObject filter = new CartageLegFilterStripBusinessObject();
			((ModuleNkFilter)filter["Port Transport Service Level"]).Property = "def";
			((ModuleNkFilter)filter["Port Transport Service Level"]).IsActive = true;
			CommonCartageLegCollection collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartageLeg1, collection);
			AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
			((ModuleNkFilter)filter["Port Transport Service Level"]).Property = "abc";
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartageLeg2, collection);
		}

		public void TestJobDirection()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var cartage1 = Factory.New<CommonCartage>();
			var cartage2 = Factory.New<CommonCartage>();
			cartage1.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			cartage2.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			cartage1.BookedMovesCollection.DeleteAll();
			cartage2.BookedMovesCollection.DeleteAll();
			cartage1.JJ_GB = branch.PK;
			cartage2.JJ_GB = branch.PK;
			var booked1 = cartage1.LooseBookedMoves.AddNew();
			var cartageLeg1 = booked1.CartageLegs.AddNew();
			var booked2 = cartage2.LooseBookedMoves.AddNew();
			var cartageLeg2 = booked2.CartageLegs.AddNew();
			Factory.Save();
			var filter = new CartageLegFilterStripBusinessObject();
			var moduleFilter = (ModuleTextFilter)filter["Direction"];
			moduleFilter.Property = "IMP";
			moduleFilter.IsActive = true;
			CommonCartageLegCollection collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartageLeg1, collection);
			AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
			moduleFilter.Property = "EXP";
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartageLeg2, collection);
		}

		public void TestConnectingFreightMode()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var cartage1 = Factory.New<CommonCartage>();
			var cartage2 = Factory.New<CommonCartage>();
			cartage1.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			cartage2.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			cartage1.BookedMovesCollection.DeleteAll();
			cartage2.BookedMovesCollection.DeleteAll();
			cartage1.JJ_GB = branch.PK;
			cartage2.JJ_GB = branch.PK;
			var booked1 = cartage1.LooseBookedMoves.AddNew();
			var cartageLeg1 = booked1.CartageLegs.AddNew();
			var booked2 = cartage2.LooseBookedMoves.AddNew();
			var cartageLeg2 = booked2.CartageLegs.AddNew();
			Factory.Save();
			var filter = new CartageLegFilterStripBusinessObject();
			var moduleFilter = (ModuleTextFilter)filter["Connecting Freight Mode"];
			moduleFilter.Property = "SEA";
			moduleFilter.IsActive = true;
			CommonCartageLegCollection collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartageLeg1, collection);
			AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
			moduleFilter.Property = "AIR";
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartageLeg2, collection);
		}

		public void TestMessageStatus()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			CommonCartageLeg cartageLeg1 = GetCartageLegWithMessageStatus(branch, Constants.CartageLegDispatchStatusList.Codes.Delivered);
			CommonCartageLeg cartageLeg4 = GetCartageLegWithMessageStatus(branch, Constants.CartageLegDispatchStatusList.Codes.Futile);
			CommonCartageLeg cartageLeg6 = GetCartageLegWithMessageStatus(branch, Constants.CartageLegDispatchStatusList.Codes.PickedUp);
			CommonCartageLeg cartageLeg7 = GetCartageLegWithMessageStatus(branch, Constants.CartageLegDispatchStatusList.Codes.Rejected);
			CommonCartageLeg cartageLeg8 = GetCartageLegWithMessageStatus(branch, Constants.CartageLegDispatchStatusList.Codes.Runsheet);
			Factory.Save();
			CartageLegFilterStripBusinessObject filter = new CartageLegFilterStripBusinessObject();
			CommonCartageLegCollection collection = new CommonCartageLegCollection(Factory);
			((ModuleTextFilter)filter["Message Status"]).IsActive = true;
			((ModuleTextFilter)filter["Message Status"]).Property = Constants.CartageLegDispatchStatusList.Codes.Delivered;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have CartageLeg1", cartageLeg1, collection);
			((ModuleTextFilter)filter["Message Status"]).Property = Constants.CartageLegDispatchStatusList.Codes.Futile;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have CartageLeg4", cartageLeg4, collection);
			((ModuleTextFilter)filter["Message Status"]).Property = Constants.CartageLegDispatchStatusList.Codes.PickedUp;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have CartageLeg6", cartageLeg6, collection);
			((ModuleTextFilter)filter["Message Status"]).Property = Constants.CartageLegDispatchStatusList.Codes.Rejected;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have CartageLeg7", cartageLeg7, collection);
			((ModuleTextFilter)filter["Message Status"]).Property = Constants.CartageLegDispatchStatusList.Codes.Runsheet;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have CartageLeg8", cartageLeg8, collection);
			((ModuleTextFilter)filter["Message Status"]).Property = "";
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 5 CommonCartageLeg", 5, collection.Count);
		}

		CommonCartageLeg GetCartageLegWithMessageStatus(GlbBranch branch, ZString messageStatus)
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_GB = branch.PK;
			CommonBookedCtgMove booked = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg = booked.CartageLegs.AddNew();
			cartageLeg.JU_MessageStatus = messageStatus;
			return cartageLeg;
		}

		public void TestDangerousGoods()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = branch.PK;
			CommonBookedCtgMove booked1 = cartage1.LooseBookedMoves.AddNew();
			CommonCartageLeg leg1 = booked1.CartageLegs.AddNew();
			CommonCartage cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_GB = branch.PK;
			CommonBookedCtgMove booked2 = cartage2.LooseBookedMoves.AddNew();
			CommonCartageLeg leg2 = booked2.CartageLegs.AddNew();
			UNDGDataItem dangerousGood = Factory.New<UNDGDataItem>();
			dangerousGood.DI_ParentTableCode = JobBookedCtgMoveSchema.Constants.Prefix;
			dangerousGood.DI_ParentID = booked1.PK;
			Factory.Save();
			CartageLegFilterStripBusinessObject filter = new CartageLegFilterStripBusinessObject();
			CommonCartageLegCollection collection = new CommonCartageLegCollection(Factory);
			((ModuleTextFilter)filter["Dangerous Goods"]).IsActive = true;
			((ModuleTextFilter)filter["Dangerous Goods"]).Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.DangerousGoodsBoth;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 2 CommonCartageLegs", 2, collection.Count);
			AssertCollectionContains("Should have leg1", leg1, collection);
			AssertCollectionContains("Should have leg2", leg2, collection);
			((ModuleTextFilter)filter["Dangerous Goods"]).Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.DangerousGoodsHas;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have Leg1", leg1, collection);
			((ModuleTextFilter)filter["Dangerous Goods"]).Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.DangerousGoodsNone;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have Leg2", leg2, collection);
			((ModuleTextFilter)filter["Dangerous Goods"]).Property = "";
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 2, collection.Count);
		}

		public void TestCartageLegEmpty()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_GB = branch.PK;
			CommonBookedCtgMove booked = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg1 = booked.CartageLegs.AddNew();
			CommonCartageLeg cartageLeg2 = booked.CartageLegs.AddNew();
			CommonCartageLeg cartageLeg3 = booked.CartageLegs.AddNew();
			cartageLeg1.JU_IsEmptyContainer = false;
			cartageLeg2.JU_IsEmptyContainer = true;
			cartageLeg3.JU_IsEmptyContainer = false;
			Factory.Save();
			CartageLegFilterStripBusinessObject filter = new CartageLegFilterStripBusinessObject();
			((ModuleTextFilter)filter["Container/Leg Empty"]).Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.All;
			((ModuleTextFilter)filter["Container/Leg Empty"]).IsActive = true;
			CommonCartageLegCollection collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 3 CartageLegs", 3, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
			AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
			AssertCollectionContains("Should have Cartage3", cartageLeg3, collection);
			((ModuleTextFilter)filter["Container/Leg Empty"]).Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.Empty;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartageLeg1, collection);
			AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartageLeg3, collection);
			((ModuleTextFilter)filter["Container/Leg Empty"]).Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.NonEmpty;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 2 CommonCartageLeg", 2, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartageLeg2, collection);
			AssertCollectionContains("Should have Cartage3", cartageLeg3, collection);
		}

		public void TestCartageLegAllocated()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_GB = branch.PK;
			CommonBookedCtgMove booked = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg1 = booked.CartageLegs.AddNew();
			CommonWorkSheet workSheet1 = Factory.New<CommonWorkSheet>();
			cartageLeg1.JU_EY_RunSheet = workSheet1.PK;
			CommonCartageLeg cartageLeg2 = booked.CartageLegs.AddNew();
			CommonWorkSheet workSheet2 = Factory.New<CommonWorkSheet>();
			cartageLeg2.JU_EY_RunSheet = workSheet2.PK;
			CommonCartageLeg cartageLeg3 = booked.CartageLegs.AddNew(); //has no worksheet
			Factory.Save();
			CartageLegFilterStripBusinessObject filter = new CartageLegFilterStripBusinessObject();
			((ModuleTextFilter)filter["RunSheet Allocated"]).Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.All;
			((ModuleTextFilter)filter["RunSheet Allocated"]).IsActive = true;
			CommonCartageLegCollection collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 3 CartageLegs", 3, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
			AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
			AssertCollectionContains("Should have Cartage3", cartageLeg3, collection);
			((ModuleTextFilter)filter["RunSheet Allocated"]).Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.Allocated;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 2 CommonCartageLeg", 2, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
			AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartageLeg3, collection);
			((ModuleTextFilter)filter["RunSheet Allocated"]).Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.Unallocated;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartageLeg1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartageLeg2, collection);
			AssertCollectionContains("Should have Cartage3", cartageLeg3, collection);
		}

		public void TestContainerSlotBooked()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			// departure cartage
			var departureCartage = Factory.New<CommonCartage>();
			departureCartage.JJ_GB = branch.PK;
			var departureMove = departureCartage.ContainerBookedMoves.AddNew();
			var departureContainer = departureMove.Container;
			var departureLegEmpty = departureMove.CartageLegs.AddNew();
			var departureLegFull = departureMove.CartageLegs.AddNew();
			Helper.CreateAndAssignAddresses(departureLegEmpty, departureLegFull, DocAddressType.LocalCartageYard, DocAddressType.LocalCartageExporter, DocAddressType.LocalCartageCTO);
			// arrival cartage
			var arrivalCartage = Factory.New<CommonCartage>();
			arrivalCartage.JJ_GB = branch.PK;
			var arrivalMove = arrivalCartage.ContainerBookedMoves.AddNew();
			var arrivalContainer = arrivalMove.Container;
			var arrivalLegFull = arrivalMove.CartageLegs.AddNew();
			var arrivalLegEmpty = arrivalMove.CartageLegs.AddNew();
			Helper.CreateAndAssignAddresses(arrivalLegFull, arrivalLegEmpty, DocAddressType.LocalCartageCTO, DocAddressType.LocalCartageImporter, DocAddressType.LocalCartageYard);
			Factory.Save();
			// setup filter
			var collection = new CommonCartageLegCollection(Factory);
			var filterStripBO = new CartageLegFilterStripBusinessObject();
			var filter = ((ModuleTextFilter)filterStripBO["Slot Booked"]);
			filter.IsActive = true;
			// no container booked
			filter.Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.All;
			collection.AdditionalFilter = filterStripBO.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { departureLegEmpty, departureLegFull, arrivalLegFull, arrivalLegEmpty }, collection);
			filter.Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.Booked;
			collection.AdditionalFilter = filterStripBO.Filter;
			AssertContainsExactElementsInAnyOrder(Array.Empty<CommonCartageLeg>(), collection);
			filter.Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.NonBooked;
			collection.AdditionalFilter = filterStripBO.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { departureLegFull, arrivalLegFull }, collection);
			// departure container booked (half)
			departureContainer.JC_DepartureSlotReference = "DepSlot";
			Factory.Save();
			filter.Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.All;
			collection.AdditionalFilter = filterStripBO.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { departureLegEmpty, departureLegFull, arrivalLegFull, arrivalLegEmpty }, collection);
			filter.Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.Booked;
			collection.AdditionalFilter = filterStripBO.Filter;
			AssertContainsExactElementsInAnyOrder(Array.Empty<CommonCartageLeg>(), collection);
			filter.Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.NonBooked;
			collection.AdditionalFilter = filterStripBO.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { departureLegFull, arrivalLegFull }, collection);
			// departure container booked (fully)
			departureContainer.JC_DepartureSlotDateTime = ZDateTime.Now;
			Factory.Save();
			filter.Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.All;
			collection.AdditionalFilter = filterStripBO.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { departureLegEmpty, departureLegFull, arrivalLegFull, arrivalLegEmpty }, collection);
			filter.Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.Booked;
			collection.AdditionalFilter = filterStripBO.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { departureLegFull }, collection);
			filter.Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.NonBooked;
			collection.AdditionalFilter = filterStripBO.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { arrivalLegFull }, collection);
			// arrival container booked (half)
			arrivalContainer.JC_ArrivalSlotDateTime = ZDateTime.Now;
			Factory.Save();
			filter.Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.All;
			collection.AdditionalFilter = filterStripBO.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { departureLegEmpty, departureLegFull, arrivalLegFull, arrivalLegEmpty }, collection);
			filter.Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.Booked;
			collection.AdditionalFilter = filterStripBO.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { departureLegFull }, collection);
			filter.Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.NonBooked;
			collection.AdditionalFilter = filterStripBO.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { arrivalLegFull }, collection);
			// arrival container booked (fully)
			arrivalContainer.JC_ArrivalSlotReference = "ArrSlot";
			Factory.Save();
			filter.Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.All;
			collection.AdditionalFilter = filterStripBO.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { departureLegEmpty, departureLegFull, arrivalLegFull, arrivalLegEmpty }, collection);
			filter.Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.Booked;
			collection.AdditionalFilter = filterStripBO.Filter;
			AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { departureLegFull, arrivalLegFull }, collection);
			filter.Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.NonBooked;
			collection.AdditionalFilter = filterStripBO.Filter;
			AssertContainsExactElementsInAnyOrder(Array.Empty<CommonCartageLeg>(), collection);
		}

		public void TestCompleted()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_GB = branch.PK;
			CommonBookedCtgMove booked = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg1 = booked.CartageLegs.AddNew();
			CommonCartageLeg cartageLeg2 = booked.CartageLegs.AddNew();
			CommonCartageLeg cartageLeg3 = booked.CartageLegs.AddNew();
			cartageLeg1.JU_DeliverTimeOut = ZDateTime.Empty;
			cartageLeg2.JU_DeliverTimeOut = ZDateTime.Now;
			cartageLeg3.JU_DeliverTimeOut = ZDateTime.Empty;
			Factory.Save();
			CartageLegFilterStripBusinessObject filter = new CartageLegFilterStripBusinessObject();
			((ModuleTextFilter)filter["Completed"]).Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.All;
			((ModuleTextFilter)filter["Completed"]).IsActive = true;
			CommonCartageLegCollection collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 3 CartageLegs", 3, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
			AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
			AssertCollectionContains("Should have Cartage3", cartageLeg3, collection);
			((ModuleTextFilter)filter["Completed"]).Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.Complete;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartageLeg1, collection);
			AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartageLeg3, collection);
			((ModuleTextFilter)filter["Completed"]).Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.Incomplete;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 2 CommonCartageLeg", 2, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartageLeg2, collection);
			AssertCollectionContains("Should have Cartage3", cartageLeg3, collection);
		}

		public void TestLegType()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var looseCartage = Factory.New<CommonCartage>();
			var containerizedCartage = Factory.New<CommonCartage>();
			looseCartage.JJ_GB = branch.PK;
			containerizedCartage.JJ_GB = branch.PK;
			looseCartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			containerizedCartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLCTOtoCNEWAITtoCYD;
			CommonBookedCtgMove looseBookMove = looseCartage.LooseBookedMoves.AddNew();
			CommonBookedCtgMove containerBookMove = containerizedCartage.ContainerBookedMoves.AddNew();
			CommonContainer container = containerBookMove.Container;
			CommonCartageLeg looseBookLeg1 = looseBookMove.CartageLegs.AddNew();
			CommonCartageLeg containerLeg1 = containerBookMove.CartageLegs.AddNew();
			Factory.Save();
			var filter = new CartageLegFilterStripBusinessObject();
			var legTypeFilter = (ModuleTextFilter)filter["Leg Type"];
			legTypeFilter.Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.All;
			legTypeFilter.IsActive = true;
			AssertEquals("Leg type filter should be ModesAndTypes category type.", FilterCategories.ModesAndTypes, legTypeFilter.Category);
			var collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 2 CartageLegs.", 2, collection.Count);
			AssertCollectionContains("Should have looseBookLeg1.", looseBookLeg1, collection);
			AssertCollectionContains("Should have containerLeg1.", containerLeg1, collection);
			legTypeFilter.Property = Constants.ContainerModes.Containerised;
			collection.AdditionalFilter = filter.Filter;
			legTypeFilter.IsActive = true;
			AssertEquals("Should have 1 CommonCartageLeg.", 1, collection.Count);
			AssertCollectionContains("Should have containerLeg1 only.", containerLeg1, collection);
			legTypeFilter.Property = Constants.ContainerModes.Loose;
			collection.AdditionalFilter = filter.Filter;
			legTypeFilter.IsActive = true;
			AssertEquals("Should have 1 CommonCartageLeg.", 1, collection.Count);
			AssertCollectionContains("Should have looseBookLeg1 only.", looseBookLeg1, collection);
		}

		public void TestPickupCity()
		{
			TestJobDocAddressTextFilter("Pickup City", JobContainerLegsSchema.JU_E2PickupAddressID, JobDocAddressSchema.E2_City, OrgAddressSchema.OA_City);
			TestJobDocAddressTextFilter("Pickup City", JobContainerLegsSchema.JU_E2WaitPointAddressID, JobDocAddressSchema.E2_City, OrgAddressSchema.OA_City);
		}

		public void TestDeliveryCity()
		{
			TestJobDocAddressTextFilter("Delivery City", JobContainerLegsSchema.JU_E2WaitPointAddressID, JobDocAddressSchema.E2_City, OrgAddressSchema.OA_City);
			TestJobDocAddressTextFilter("Delivery City", JobContainerLegsSchema.JU_E2DeliveryAddressID, JobDocAddressSchema.E2_City, OrgAddressSchema.OA_City);
		}

		public void TestPickupPostCode()
		{
			TestJobDocAddressTextFilter("Pickup PostCode", JobContainerLegsSchema.JU_E2PickupAddressID, JobDocAddressSchema.E2_Postcode, OrgAddressSchema.OA_PostCode);
			TestJobDocAddressTextFilter("Pickup PostCode", JobContainerLegsSchema.JU_E2WaitPointAddressID, JobDocAddressSchema.E2_Postcode, OrgAddressSchema.OA_PostCode);
		}

		public void TestDeliveryPostCode()
		{
			TestJobDocAddressTextFilter("Delivery PostCode", JobContainerLegsSchema.JU_E2WaitPointAddressID, JobDocAddressSchema.E2_Postcode, OrgAddressSchema.OA_PostCode);
			TestJobDocAddressTextFilter("Delivery PostCode", JobContainerLegsSchema.JU_E2DeliveryAddressID, JobDocAddressSchema.E2_Postcode, OrgAddressSchema.OA_PostCode);
		}

		public void TestPickupAddressType()
		{
			TestJobDocAddressTypeFilter("Pickup Address Type", JobContainerLegsSchema.JU_E2PickupAddressID, JobDocAddressSchema.E2_AddressType);
			TestJobDocAddressTypeFilter("Pickup Address Type", JobContainerLegsSchema.JU_E2WaitPointAddressID, JobDocAddressSchema.E2_AddressType);
		}

		public void TestDeliveryAddressType()
		{
			TestJobDocAddressTypeFilter("Delivery Address Type", JobContainerLegsSchema.JU_E2WaitPointAddressID, JobDocAddressSchema.E2_AddressType);
			TestJobDocAddressTypeFilter("Delivery Address Type", JobContainerLegsSchema.JU_E2DeliveryAddressID, JobDocAddressSchema.E2_AddressType);
		}

		public void TestLoadFilter()
		{
			JobSailing sailing1 = Helper.CreateSailing(Helper.TestVessel1, "111", LocalCartageTestHelper.HomePort, LocalCartageTestHelper.OverseasPort, ZDateTime.Today);
			JobSailing sailing2 = Helper.CreateSailing(Helper.TestVessel1, "111", LocalCartageTestHelper.AlternateHomePort, LocalCartageTestHelper.OverseasPort, ZDateTime.Today);
			JobSailing sailing3 = Helper.CreateSailing(Helper.TestVessel1, "111", LocalCartageTestHelper.OverseasPort2, LocalCartageTestHelper.OverseasPort, ZDateTime.Today);
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			CommonBookedCtgMove booked1 = cartage1.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg1 = booked1.CartageLegs.AddNew();
			cartage1.JJ_JX_Sailing = sailing1.PK;
			CommonCartage cartage2 = Factory.New<CommonCartage>();
			CommonBookedCtgMove booked2 = cartage2.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg2 = booked2.CartageLegs.AddNew();
			cartage2.JJ_JX_Sailing = sailing2.PK;
			CommonCartage cartage3 = Factory.New<CommonCartage>();
			CommonBookedCtgMove booked3 = cartage3.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg3 = booked3.CartageLegs.AddNew();
			cartage3.JJ_JX_Sailing = sailing3.PK;
			Factory.Save();
			CartageLegFilterStripBusinessObject filters = new CartageLegFilterStripBusinessObject();
			ModuleLocationFilter filter = (ModuleLocationFilter)filters["Load / Discharge"];
			filter.IsActive = true;
			filter.Property1 = LocalCartageTestHelper.HomePort;
			CommonCartageLegCollection cartageLegs = new CommonCartageLegCollection(Factory);
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionContains(cartageLeg1, cartageLegs);
			AssertCollectionNotContains(cartageLeg2, cartageLegs);
			AssertCollectionNotContains(cartageLeg3, cartageLegs);
			filter.Property1 = LocalCartageTestHelper.HomePort.SubstringSafe(0, 2);
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionContains(cartageLeg1, cartageLegs);
			AssertCollectionContains(cartageLeg2, cartageLegs);
			AssertCollectionNotContains(cartageLeg3, cartageLegs);
			filter.Property1 = "";
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionContains(cartageLeg1, cartageLegs);
			AssertCollectionContains(cartageLeg2, cartageLegs);
			AssertCollectionContains(cartageLeg3, cartageLegs);
		}

		public void TestDischargeFilter()
		{
			JobSailing sailing1 = Helper.CreateSailing(Helper.TestVessel1, "111", LocalCartageTestHelper.OverseasPort, LocalCartageTestHelper.HomePort, ZDateTime.Today);
			JobSailing sailing2 = Helper.CreateSailing(Helper.TestVessel1, "111", LocalCartageTestHelper.OverseasPort, LocalCartageTestHelper.AlternateHomePort, ZDateTime.Today);
			JobSailing sailing3 = Helper.CreateSailing(Helper.TestVessel1, "111", LocalCartageTestHelper.OverseasPort, LocalCartageTestHelper.OverseasPort2, ZDateTime.Today);
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			CommonBookedCtgMove booked1 = cartage1.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg1 = booked1.CartageLegs.AddNew();
			cartage1.JJ_JX_Sailing = sailing1.PK;
			CommonCartage cartage2 = Factory.New<CommonCartage>();
			CommonBookedCtgMove booked2 = cartage2.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg2 = booked2.CartageLegs.AddNew();
			cartage2.JJ_JX_Sailing = sailing2.PK;
			CommonCartage cartage3 = Factory.New<CommonCartage>();
			CommonBookedCtgMove booked3 = cartage3.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg3 = booked3.CartageLegs.AddNew();
			cartage3.JJ_JX_Sailing = sailing3.PK;
			Factory.Save();
			CartageLegFilterStripBusinessObject filters = new CartageLegFilterStripBusinessObject();
			ModuleLocationFilter filter = (ModuleLocationFilter)filters["Load / Discharge"];
			filter.IsActive = true;
			filter.Property2 = LocalCartageTestHelper.HomePort;
			CommonCartageLegCollection cartageLegs = new CommonCartageLegCollection(Factory);
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionContains(cartageLeg1, cartageLegs);
			AssertCollectionNotContains(cartageLeg2, cartageLegs);
			AssertCollectionNotContains(cartageLeg3, cartageLegs);
			filter.Property2 = LocalCartageTestHelper.HomePort.SubstringSafe(0, 2);
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionContains(cartageLeg1, cartageLegs);
			AssertCollectionContains(cartageLeg2, cartageLegs);
			AssertCollectionNotContains(cartageLeg3, cartageLegs);
			filter.Property2 = "";
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionContains(cartageLeg1, cartageLegs);
			AssertCollectionContains(cartageLeg2, cartageLegs);
			AssertCollectionContains(cartageLeg3, cartageLegs);
		}

		public void TestRelatedPort()
		{
			TestJobDocAddressTextFilter("Related Port", JobContainerLegsSchema.JU_E2PickupAddressID, null, OrgAddressSchema.OA_RL_NKRelatedPortCode, "JPOSA", "USLAX");
			TestJobDocAddressTextFilter("Related Port", JobContainerLegsSchema.JU_E2WaitPointAddressID, null, OrgAddressSchema.OA_RL_NKRelatedPortCode, "JPOSA", "USLAX");
			TestJobDocAddressTextFilter("Related Port", JobContainerLegsSchema.JU_E2DeliveryAddressID, null, OrgAddressSchema.OA_RL_NKRelatedPortCode, "JPOSA", "USLAX");
		}

		public void TestWorkflowCustomFields()
		{
			ModuleFilterCollection filterCollection = new CartageLegFilterStripBusinessObject().ModuleFilters;
			AssertNull(filterCollection["stringField"]);
			AssertNull(filterCollection["intField"]);
			AssertNull(filterCollection["dateTimeField"]);
			AssertNull(filterCollection["boolField"]);
			var template = Helper.CreateWorkflowTemplate(WorkflowDescriptors.CartageLegWorkflowDescriptorCode);
			Helper.AddCustomField(template, "stringField", AddOnColumnDataType.Codes.String);
			Helper.AddCustomField(template, "intField", AddOnColumnDataType.Codes.Integer);
			Helper.AddCustomField(template, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
			Helper.AddCustomField(template, "boolField", AddOnColumnDataType.Codes.Boolean);
			Factory.Save();
			WorkflowCustomFieldsFilter.ClearCache();
			filterCollection = new CartageLegFilterStripBusinessObject().ModuleFilters;
			AssertEquals(typeof(ModuleTextFilter), filterCollection["stringField"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), filterCollection["intField"].GetType());
			AssertEquals(typeof(ModuleDateFilter), filterCollection["dateTimeField"].GetType());
			AssertEquals(typeof(ModuleFlagsFilter), filterCollection["Workflow Flags"].GetType());
		}

		public void TestCartageJobActiveStatus()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var activeCartage = Factory.New<CommonCartage>();
			var activeBooking = activeCartage.LooseBookedMoves.AddNew();
			var activeLeg = activeBooking.CartageLegs.AddNew();
			var inactiveCartage = Factory.New<CommonCartage>();
			var inactiveBooking = inactiveCartage.LooseBookedMoves.AddNew();
			var inactiveLeg = inactiveBooking.CartageLegs.AddNew();
			activeCartage.JJ_IsCancelled = false;
			activeCartage.JJ_GB = branch.PK;
			inactiveCartage.JJ_IsCancelled = true;
			inactiveCartage.JJ_GB = branch.PK;
			Factory.Save();
			var filter = new CartageLegFilterStripBusinessObject();
			var activeStatusFilter = ((ModuleTextFilter)filter["Port Transport Job Active Status"]);
			activeStatusFilter.IsActive = true;

			AllLanguages.ForEach(lan =>
			{
				using (Res.TemporarilySwitchLanguage(lan))
				{
					activeStatusFilter.Property = FilterStripBusinessObject.StatusActive;
					var collection = new CommonCartageLegCollection(Factory);
					collection.AdditionalFilter = filter.Filter;
					AssertContainsExactElementsInAnyOrder(activeLeg, collection);
					activeStatusFilter.Property = FilterStripBusinessObject.StatusInactive;
					collection.AdditionalFilter = filter.Filter;
					AssertContainsExactElementsInAnyOrder(inactiveLeg, collection);
					activeStatusFilter.Property = FilterStripBusinessObject.StatusAll;
					collection.AdditionalFilter = filter.Filter;
					AssertContainsExactElementsInAnyOrder(new CommonCartageLeg[] { activeLeg, inactiveLeg }, collection);
				}
			});
		}

		public void TestCartageParentJobNumberFilterExists()
		{
			AssertNotNull("Filter for Port Transport Parent Job Number should exist", new CartageLegFilterStripBusinessObject()["Port Transport Parent Job #"]);
		}

		public void TestParentJobNumberFilterWithContainsOperator()
		{
			var cartageLegData = CreateCartageLegsWithDifferentParents();
			var filterBO = new CartageLegFilterStripBusinessObject();
			ModuleNumberFilter filter = (ModuleNumberFilter)filterBO["Port Transport Parent Job #"];
			filter.Property = "BX";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			var cartageLegs = new CommonCartageLegCollection(Factory);
			cartageLegs.AdditionalFilter = filterBO.Filter;
			AssertContainsExactElementsInAnyOrder("Cartage with parent job BX0000001 is in Collection", new[] { cartageLegData["CUS"] }, cartageLegs);
			filter.Property = "00000";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.IsActive = true;
			cartageLegs = new CommonCartageLegCollection(Factory);
			cartageLegs.AdditionalFilter = filterBO.Filter;
			AssertContainsExactElementsInAnyOrder("Should get all cartages with parents containing '00000' in their unique consignment ID - ie except for cartage with forwarding shipment parent with JS_UniqueConsignRef = 'ABC0123'", new[] { cartageLegData["CFS"], cartageLegData["CFC"], cartageLegData["WHO"], cartageLegData["CUS"], cartageLegData["TBK"], }, cartageLegs);
		}

		public void TestParentJobNumberFilterWithNotContainsOperator()
		{
			var cartageLegData = CreateCartageLegsWithDifferentParents();
			var filterBO = new CartageLegFilterStripBusinessObject();
			ModuleNumberFilter filter = (ModuleNumberFilter)filterBO["Port Transport Parent Job #"];
			filter.Property = "BX";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			var cartageLegs = new CommonCartageLegCollection(Factory);
			cartageLegs.AdditionalFilter = filterBO.Filter;
			AssertContainsExactElementsInAnyOrder("Cartage with parent job BX0000001 is in Collection", new[] { cartageLegData["CUS"] }, cartageLegs);
			filter.Property = "00000";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.IsActive = true;
			cartageLegs = new CommonCartageLegCollection(Factory);
			cartageLegs.AdditionalFilter = filterBO.Filter;
			AssertContainsExactElementsInAnyOrder("Should get all cartages with parents not containing '00000' in their unique consignment ID - ie standalone cartage and cartage with forwarding shipment parent with JS_UniqueConsignRef = 'ABC0123'", new[] { cartageLegData["STC"], cartageLegData["SHP"], }, cartageLegs);
		}

		public void TestParentJobTypeFilterExists()
		{
			AssertNotNull("Filter for Port Transport Parent Job Type should exist", new CartageLegFilterStripBusinessObject()["Port Transport Parent Job Type"]);
		}

		public void TestParentJobTypeFilterForwardingShipment()
		{
			var cartageLegData = CreateCartageLegsWithDifferentParents();
			var cartageLegs = GetCartageLegsWithParentJobTypeFilter("SHP");
			AssertContainsExactElementsInAnyOrder("Cartage with parent Forwarding Shipment is in Collection", new[] { cartageLegData["SHP"] }, cartageLegs);
		}

		public void TestParentJobTypeFilterCFSShipment()
		{
			var cartageLegData = CreateCartageLegsWithDifferentParents();
			var cartageLegs = GetCartageLegsWithParentJobTypeFilter("CFS");
			AssertContainsExactElementsInAnyOrder("Cartage with parent CFS Shipment is in Collection", new[] { cartageLegData["CFS"] }, cartageLegs);
		}

		public void TestParentJobTypeFilterCFSConsol()
		{
			var cartageLegData = CreateCartageLegsWithDifferentParents();
			var cartageLegs = GetCartageLegsWithParentJobTypeFilter("CFC");
			AssertContainsExactElementsInAnyOrder("Cartage with parent CFS Consol is in Collection", new[] { cartageLegData["CFC"] }, cartageLegs);
		}

		public void TestParentJobTypeFilterWhsOrder()
		{
			var cartageLegData = CreateCartageLegsWithDifferentParents();
			var cartageLegs = GetCartageLegsWithParentJobTypeFilter("WHO");
			AssertContainsExactElementsInAnyOrder("Cartage with parent Warehouse Order is in Collection", new[] { cartageLegData["WHO"] }, cartageLegs);
		}

		public void TestParentJobTypeFilterCustomsDeclaration()
		{
			var cartageLegData = CreateCartageLegsWithDifferentParents();
			var cartageLegs = GetCartageLegsWithParentJobTypeFilter("CUS");
			AssertContainsExactElementsInAnyOrder("Cartage with parent Customs Declaration is in Collection", new[] { cartageLegData["CUS"] }, cartageLegs);
		}

		public void TestParentJobTypeFilterTransportBooking()
		{
			var cartageLegData = CreateCartageLegsWithDifferentParents();
			var cartageLegs = GetCartageLegsWithParentJobTypeFilter("TBK");
			AssertContainsExactElementsInAnyOrder("Cartage with parent Transport Booking is in Collection", new[] { cartageLegData["TBK"] }, cartageLegs);
		}

		public void TestParentJobTypeFilterStandalone()
		{
			var cartageLegData = CreateCartageLegsWithDifferentParents();
			var cartageLegs = GetCartageLegsWithParentJobTypeFilter("STC");
			AssertContainsExactElementsInAnyOrder("Stand-alone Cartage is in Collection", new[] { cartageLegData["STC"] }, cartageLegs);
		}

		CartageLegFilterStripBusinessObject CreateParentJobTypeFilter(string typeCode)
		{
			var filterBO = new CartageLegFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO["Port Transport Parent Job Type"];
			filter.Property = typeCode;
			filter.IsActive = true;
			return filterBO;
		}
		CommonCartageLegCollection GetCartageLegsWithParentJobTypeFilter(string typeCode)
		{
			var filterBO = CreateParentJobTypeFilter(typeCode);
			var cartageLegs = new CommonCartageLegCollection(Factory);
			cartageLegs.AdditionalFilter = filterBO.Filter;
			return cartageLegs;
		}

		CartageLegFilterStripBusinessObject CreateMilestoneDateFilter(int fromDateDaysAfterCurrentTime, int toDateDaysAfterCurrentTime)
		{
			var currentTime = ZDateTime.UtcNow;

			var filterBO = new CartageLegFilterStripBusinessObject();
			var filter = (WorkflowModuleFilter)filterBO.ModuleFilters["Milestone Date"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(currentTime.AddDays(fromDateDaysAfterCurrentTime));
			filter.Property2 = new ZDateTime(currentTime.AddDays(toDateDaysAfterCurrentTime));
			filter.IsActive = true;
			return filterBO;
		}

		CommonCartageLegCollection GetCartageLegsWithMilestoneDateFilter(int fromDateDaysAfterCurrentTime, int toDateDaysAfterCurrentTime)
		{
			var filterBO = CreateMilestoneDateFilter(fromDateDaysAfterCurrentTime, toDateDaysAfterCurrentTime);
			var cartageLegs = new CommonCartageLegCollection(Factory);
			cartageLegs.AdditionalFilter = filterBO.Filter;
			return cartageLegs;
		}

		CartageLegFilterStripBusinessObject CreateMilestoneCompletedFilter(string filterProperty)
		{
			var filterBO = new CartageLegFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO.ModuleFilters["Milestone Completed"];
			filter.Property = filterProperty;
			filter.IsActive = true;
			return filterBO;
		}

		CommonCartageLegCollection GetCartageLegsWithMilestoneCompletedFilter(string filterProperty)
		{
			var filterBO = CreateMilestoneCompletedFilter(filterProperty);
			var cartageLegs = new CommonCartageLegCollection(Factory);
			cartageLegs.AdditionalFilter = filterBO.Filter;
			return cartageLegs;
		}

		CartageLegFilterStripBusinessObject CreateMilestoneNextFilter(int fromDateDaysAfterCurrentTime, int toDateDaysAfterCurrentTime)
		{
			var currentTime = ZDateTime.UtcNow;

			var filterBO = new CartageLegFilterStripBusinessObject();
			var filter = (WorkflowModuleFilter)filterBO.ModuleFilters["Next Milestone"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(currentTime.AddDays(fromDateDaysAfterCurrentTime));
			filter.Property2 = new ZDateTime(currentTime.AddDays(toDateDaysAfterCurrentTime));
			filter.IsActive = true;
			return filterBO;
		}

		CommonCartageLegCollection GetCartageLegsWithMilestoneNextFilter(int fromDateDaysAfterCurrentTime, int toDateDaysAfterCurrentTime)
		{
			var filterBO = CreateMilestoneNextFilter(fromDateDaysAfterCurrentTime, toDateDaysAfterCurrentTime);
			var cartageLegs = new CommonCartageLegCollection(Factory);
			cartageLegs.AdditionalFilter = filterBO.Filter;
			return cartageLegs;
		}

		CartageLegFilterStripBusinessObject CreateMilestoneLastCompletedFilter(int fromDateDaysAfterCurrentTime, int toDateDaysAfterCurrentTime)
		{
			var currentTime = ZDateTime.UtcNow;

			var filterBO = new CartageLegFilterStripBusinessObject();
			var filter = (WorkflowModuleFilter)filterBO.ModuleFilters["Last Completed Milestone"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(currentTime.AddDays(fromDateDaysAfterCurrentTime));
			filter.Property2 = new ZDateTime(currentTime.AddDays(toDateDaysAfterCurrentTime));
			filter.IsActive = true;
			return filterBO;
		}

		CommonCartageLegCollection GetCartageLegsWithMilestoneLastCompletedFilter(int fromDateDaysAfterCurrentTime, int toDateDaysAfterCurrentTime)
		{
			var filterBO = CreateMilestoneLastCompletedFilter(fromDateDaysAfterCurrentTime, toDateDaysAfterCurrentTime);
			var cartageLegs = new CommonCartageLegCollection(Factory);
			cartageLegs.AdditionalFilter = filterBO.Filter;
			return cartageLegs;
		}

		Dictionary<string, CommonCartageLeg> CreateCartageLegsWithDifferentParents()
		{
			var cartageLegs = new Dictionary<string, CommonCartageLeg>();
			(var cartage1, var _, var _, var _) = LocalCartageTestHelper.CreateTestCartageWithParentForwardingShipment(Factory);
			cartage1.JJ_ConsignmentID = "TSHP1";
			AddLegRecordsToNewCartage(cartage1);
			cartageLegs["SHP"] = cartage1.CartageLegs.First();
			var cfsConsol = CreateTestCfsConsolParent();
			cartageLegs["CFC"] = CreateTestCartageLegWithParent((ICartageParent)cfsConsol, "TCFC1");
			var cfsShipment = CreateTestCfsShipmentParent();
			cartageLegs["CFS"] = CreateTestCartageLegWithParent((ICartageParent)cfsShipment, "TCFS1");
			var whsOrder = CreateTestWhsOrderParent();
			cartageLegs["WHO"] = CreateTestCartageLegWithParent((ICartageParent)whsOrder, "TWHO1");
			var customsDeclaration = CreateTestCustomsDeclarationParent();
			cartageLegs["CUS"] = CreateTestCartageLegWithParent((ICartageParent)customsDeclaration, "TCUS1");
			var dtbBooking = CreateTestDtbBooking();
			cartageLegs["TBK"] = CreateTestCartageLegWithDtbBookingParent(dtbBooking, "TTBK1");
			cartageLegs["STC"] = CreateTestCartage("TSTC1").CartageLegs.First();
			Factory.Save();
			return cartageLegs;
		}

		void AddLegRecordsToNewCartage(CommonCartage cartage)
		{
			CommonBookedCtgMove booked = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg = booked.CartageLegs.AddNew();
			CommonWorkSheet workSheet = Factory.New<CommonWorkSheet>();
			cartageLeg.JU_EY_RunSheet = workSheet.PK;
			Factory.Save();
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
			var whsTestHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var clientPK = whsTestHelper.CreateClient("ClientOrg2");
			var consigneePK = whsTestHelper.CreateClient("ConsgneeOrg2");
			var whs = whsTestHelper.CreateWarehouse("WHS2", "WHSRow");
			var part = whsTestHelper.CreateProduct(clientPK, "Product2");
			Factory.Save();
			var whsOrder = whsTestHelper.CreateWhsOrder(clientPK, whs.PK, consigneePK, orderNumber);
			whsOrder[WhsDocketSchema.WD_DocketID] = orderNumber;
			whsOrder[WhsDocketSchema.WD_DocketSubType] = "ORD";
			var whsOrderLine = whsTestHelper.CreateWhsOrderLine(whsOrder.PK, part.PK, 1);
			Factory.Save();
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

		CommonCartageLeg CreateTestCartageLegWithParent(ICartageParent parent, string jobID = "T0000001")
		{
			var cartage = CreateTestCartage(jobID);
			cartage.SetParent(parent);
			var parentBO = (BusinessObject)parent;
			cartage.JJ_ParentID = (parentBO).PK;
			cartage.JJ_ParentTableCode = parentBO.TablePrefix;
			return cartage.CartageLegs.First();
		}

		CommonCartageLeg CreateTestCartageLegWithDtbBookingParent(IDtbBooking dtbBooking, string jobID = "T0000001")
		{
			var cartage = CreateTestCartage(jobID);
			cartage.JJ_ParentID = dtbBooking.PK;
			cartage.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			cartage.FillWithValidTestData();
			return cartage.CartageLegs.First();
		}

		CommonCartage CreateTestCartage(string jobID = "T0000001")
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_GB = GlbBranch.CurrentBranch.PK;
			cartage.JJ_ConsignmentID = jobID;
			Factory.Save();
			AddLegRecordsToNewCartage(cartage);
			return cartage;
		}

		void TestBookedCtgMoveTextFilter(ZString filterName, SchemaColumn schemaColumn)
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_GB = branch.PK;
			CommonBookedCtgMove booked1 = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg1 = booked1.CartageLegs.AddNew();
			CommonBookedCtgMove booked2 = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg2 = booked2.CartageLegs.AddNew();
			booked1[schemaColumn.Name] = "abc";
			booked2[schemaColumn.Name] = "def";
			Factory.Save();
			CartageLegFilterStripBusinessObject filters = new CartageLegFilterStripBusinessObject();
			var filter = ((ModuleTextFilter)filters[filterName]);
			filter.Property = "def";
			filter.IsActive = true;
			AssertEquals("BookedMoveFilterSubGroup", filter.SubGroup.GetType().Name);
			CommonCartageLegCollection collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filters.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartageLeg1, collection);
			AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
			filter.Property = "abc";
			collection.AdditionalFilter = filters.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartageLeg2, collection);
		}

		void TestContainerTextFilter(ZString filterName, SchemaColumn schemaColumn)
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = branch.PK;
			CommonBookedCtgMove booked1 = cartage1.ContainerBookedMoves.AddNew();
			CommonContainer container1 = booked1.Container;
			CommonCartageLeg cartageLeg1 = booked1.CartageLegs.AddNew();
			CommonCartage cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_GB = branch.PK;
			CommonBookedCtgMove booked2 = cartage2.ContainerBookedMoves.AddNew();
			CommonContainer container2 = booked2.Container;
			CommonCartageLeg cartageLeg2 = booked2.CartageLegs.AddNew();
			container1[schemaColumn.Name] = "abc";
			container2[schemaColumn.Name] = "def";
			Factory.Save();
			CartageLegFilterStripBusinessObject filters = new CartageLegFilterStripBusinessObject();
			var filter = ((ModuleTextFilter)filters[filterName]);
			filter.Property = "def";
			filter.IsActive = true;
			AssertEquals("ContainerFilterSubGroup", filter.SubGroup.GetType().Name);
			CommonCartageLegCollection collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filters.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartageLeg1, collection);
			AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
			filter.Property = "abc";
			collection.AdditionalFilter = filters.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartageLeg2, collection);
		}

		void TestCartageTextFilter(ZString filterName, SchemaColumn schemaColumn)
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
			cartage1[schemaColumn.Name] = "abc";
			cartage2[schemaColumn.Name] = "def";
			Factory.Save();
			CartageLegFilterStripBusinessObject filters = new CartageLegFilterStripBusinessObject();
			var filter = ((ModuleTextFilter)filters[filterName]);
			filter.Property = "def";
			filter.IsActive = true;
			AssertEquals("CartageFilterSubGroup", filter.SubGroup.GetType().Name);
			CommonCartageLegCollection collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filters.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartageLeg1, collection);
			AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
			filter.Property = "abc";
			collection.AdditionalFilter = filters.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartageLeg2, collection);
		}

		void TestWorkSheetTextFilter(ZString filterName, SchemaColumn schemaColumn)
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
			workSheet1[schemaColumn.Name] = "abc";
			workSheet2[schemaColumn.Name] = "def";
			Factory.Save();
			CartageLegFilterStripBusinessObject filters = new CartageLegFilterStripBusinessObject();
			var filter = ((ModuleTextFilter)filters[filterName]);
			filter.Property = "def";
			filter.IsActive = true;
			AssertEquals("RunSheetFilterSubGroup", filter.SubGroup.GetType().Name);
			CommonCartageLegCollection collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filters.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartageLeg1, collection);
			AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartageLeg3, collection);
			filter.Property = "abc";
			collection.AdditionalFilter = filters.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartageLeg2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartageLeg3, collection);
		}

		void TestJobDocAddressTextFilter(ZString filterName, SchemaColumn foreignKeyColumn, SchemaColumn docAddressSchemaColumn, SchemaColumn orgAddressSchemaColumn)
		{
			TestJobDocAddressTextFilter(filterName, foreignKeyColumn, docAddressSchemaColumn, orgAddressSchemaColumn, "ABC", "DEF");
		}

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
				JobDocAddress docAddress2 = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCFS, "org2", "add2", "2000", "SYDNEY", "AUSYD", false);
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

		void TestJobDocAddressTypeFilter(ZString filterName, SchemaColumn foreignKeyColumn, SchemaColumn docAddressSchemaColumn)
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
				cartageLeg1[foreignKeyColumn.Name] = docAddress1.PK;
				JobDocAddress docAddress2 = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCFS, "org2", "add2", "2000", "SYDNEY", "AUSYD", true);
				cartageLeg2[foreignKeyColumn.Name] = docAddress2.PK;
				Factory.Save();
				CartageLegFilterStripBusinessObject filter = new CartageLegFilterStripBusinessObject();
				((ModuleTextBaseFilter)filter[filterName]).Property = "CFS";
				((ModuleTextBaseFilter)filter[filterName]).IsActive = true;
				CommonCartageLegCollection collection = new CommonCartageLegCollection(Factory);
				collection.AdditionalFilter = filter.Filter;
				AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
				AssertCollectionNotContains("Should not have Cartage1", cartageLeg1, collection);
				AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
				((ModuleTextBaseFilter)filter[filterName]).Property = "CNE";
				collection.AdditionalFilter = filter.Filter;
				AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
				AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
				AssertCollectionNotContains("Should not have Cartage2", cartageLeg2, collection);
				((ModuleTextBaseFilter)filter[filterName]).Property = "p";
				collection.AdditionalFilter = filter.Filter;
				AssertEquals("Should have 2 CommonCartageLegs", 2, collection.Count);
			}

			cartage.Delete();
			Factory.Save();
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
			CartageLegFilterStripBusinessObject filter = new CartageLegFilterStripBusinessObject();
			((ModuleDateFilter)filter[filterName]).Property1 = now.AddDays(6);
			((ModuleDateFilter)filter[filterName]).Property2 = now.AddDays(8);
			((ModuleDateFilter)filter[filterName]).IsActive = true;
			((ModuleDateFilter)filter[filterName]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			CommonCartageLegCollection collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartageLeg1, collection);
			AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartageLeg3, collection);
			((ModuleDateFilter)filter[filterName]).Property1 = now.AddDays(4);
			((ModuleDateFilter)filter[filterName]).Property2 = now.AddDays(6);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartageLeg2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartageLeg3, collection);
		}

		void TestBookedCtgMoveDateFilter(ZString filterName, SchemaColumn schemaColumn)
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
			ZDateTime now = ZDateTime.Now;
			booked1[schemaColumn.Name] = now.AddDays(5);
			booked2[schemaColumn.Name] = now.AddDays(7);
			Factory.Save();
			CartageLegFilterStripBusinessObject filters = new CartageLegFilterStripBusinessObject();
			var filter = ((ModuleDateFilter)filters[filterName]);
			filter.Property1 = now.AddDays(6);
			filter.Property2 = now.AddDays(8);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertEquals("BookedMoveFilterSubGroup", filter.SubGroup.GetType().Name);
			CommonCartageLegCollection collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filters.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartageLeg1, collection);
			AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
			filter.Property1 = now.AddDays(4);
			filter.Property2 = now.AddDays(6);
			collection.AdditionalFilter = filters.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartageLeg2, collection);
		}

		void TestContainerDateFilter(ZString filterName, SchemaColumn schemaColumn)
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = branch.PK;
			CommonBookedCtgMove booked1 = cartage1.ContainerBookedMoves.AddNew();
			CommonContainer container1 = booked1.Container;
			CommonCartageLeg cartageLeg1 = booked1.CartageLegs.AddNew();
			CommonCartage cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_GB = branch.PK;
			CommonBookedCtgMove booked2 = cartage2.ContainerBookedMoves.AddNew();
			CommonContainer container2 = booked2.Container;
			CommonCartageLeg cartageLeg2 = booked2.CartageLegs.AddNew();
			ZDateTime now = ZDateTime.Now;
			container1[schemaColumn.Name] = now.AddDays(5);
			container2[schemaColumn.Name] = now.AddDays(7);
			Factory.Save();
			CartageLegFilterStripBusinessObject filters = new CartageLegFilterStripBusinessObject();
			var filter = ((ModuleDateFilter)filters[filterName]);
			filter.Property1 = now.AddDays(6);
			filter.Property2 = now.AddDays(8);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertEquals("ContainerFilterSubGroup", filter.SubGroup.GetType().Name);
			CommonCartageLegCollection collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filters.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartageLeg1, collection);
			AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
			filter.Property1 = now.AddDays(4);
			filter.Property2 = now.AddDays(6);
			collection.AdditionalFilter = filters.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartageLeg2, collection);
		}

		void TestCartageDateFilter(ZString filterName, SchemaColumn schemaColumn, bool requiresUtcAdjustment = false)
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_GB = branch.PK;
			var booked1 = cartage1.LooseBookedMoves.AddNew();
			var cartageLeg1 = booked1.CartageLegs.AddNew();
			var cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_GB = branch.PK;
			var booked2 = cartage2.LooseBookedMoves.AddNew();
			var cartageLeg2 = booked2.CartageLegs.AddNew();
			var now = ZDateTime.Today;
			cartage1[schemaColumn.Name] = requiresUtcAdjustment ? Env.Time.GetUtcFromLocalTime(now.AddDays(5).ToDateTime()) : now.AddDays(5);
			cartage2[schemaColumn.Name] = requiresUtcAdjustment ? Env.Time.GetUtcFromLocalTime(now.AddDays(7).ToDateTime()) : now.AddDays(7);
			Factory.Save();
			var filters = new CartageLegFilterStripBusinessObject();
			var filter = ((ModuleDateFilter)filters[filterName]);
			filter.Property1 = now.AddDays(6);
			filter.Property2 = now.AddDays(8);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertEquals("CartageFilterSubGroup", filter.SubGroup.GetType().Name);
			var collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filters.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartageLeg1, collection);
			AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
			filter.Property1 = now.AddDays(4);
			filter.Property2 = now.AddDays(6);
			collection.AdditionalFilter = filters.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartageLeg2, collection);
		}

		public void TestScheduleETA()
		{
			TestHasNoDateEnteredWorksWhenNoScheduleCreated("Schedule ETA");
		}

		public void TestScheduleATA()
		{
			TestHasNoDateEnteredWorksWhenNoScheduleCreated("Schedule ATA");
		}

		public void TestScheduleETD()
		{
			TestHasNoDateEnteredWorksWhenNoScheduleCreated("Schedule ETD");
		}

		public void TestScheduleATD()
		{
			TestHasNoDateEnteredWorksWhenNoScheduleCreated("Schedule ATD");
		}

		public void TestScheduleLCLAvailability()
		{
			TestHasNoDateEnteredWorksWhenNoScheduleCreated("Schedule LCL Availability");
		}

		public void TestScheduleLCLCutOff()
		{
			TestHasNoDateEnteredWorksWhenNoScheduleCreated("Schedule LCL Cut Off");
		}

		public void TestScheduleLCLReceivalCommences()
		{
			TestHasNoDateEnteredWorksWhenNoScheduleCreated("Schedule LCL Receival Commences");
		}

		public void TestScheduleLCLStorage()
		{
			TestHasNoDateEnteredWorksWhenNoScheduleCreated("Schedule LCL Storage");
		}

		public void TestScheduleFCLAvailability()
		{
			TestHasNoDateEnteredWorksWhenNoScheduleCreated("Schedule FCL Availability");
		}

		public void TestScheduleFCLCutOff()
		{
			TestHasNoDateEnteredWorksWhenNoScheduleCreated("Schedule FCL Cut Off");
		}

		public void TestScheduleFCLReceivalCommences()
		{
			TestHasNoDateEnteredWorksWhenNoScheduleCreated("Schedule FCL Receival Commences");
		}

		public void TestScheduleFCLStorage()
		{
			TestHasNoDateEnteredWorksWhenNoScheduleCreated("Schedule FCL Storage");
		}

		void TestHasNoDateEnteredWorksWhenNoScheduleCreated(ZString filterName)
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_GB = branch.PK;
			var move = cartage.BookedMovesCollection.AddNew();
			move.CartageLegs.AddNew();
			Factory.Save();
			var filters = new CartageLegFilterStripBusinessObject();
			var filter = ((ModuleDateFilter)filters[filterName]);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			var collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filters.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			collection.AdditionalFilter = filters.Filter;
			AssertEquals("Should have 0 CommonCartageLeg", 0, collection.Count);
		}

		void TestSailingDateFilter(ZString filterName, SchemaColumn schemaColumn)
		{
			voyageNumber++;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "abc" + voyageNumber.ToString();
			ZDateTime now = ZDateTime.Now;
			JobSailing sailing = Helper.CreateSailing(vessel, "123", "AUSYD", "NZAKL", ZDateTime.Empty);
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

			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove booked1 = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg cartageLeg1 = booked1.CartageLegs.AddNew();
			cartage.JJ_JX_Sailing = sailing.PK;
			Factory.Save();
			CartageLegFilterStripBusinessObject filters = new CartageLegFilterStripBusinessObject();
			ModuleDateFilter filter = (ModuleDateFilter)filters[filterName];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = now.AddDays(-1);
			AssertEquals("ScheduleFilterSubGroup", filter.SubGroup.GetType().Name);
			var cartageLegs = new CommonCartageLegCollection(Factory);
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionContains(cartageLeg1, cartageLegs);
			filter.Property2 = now.AddDays(-1);
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionNotContains(cartageLeg1, cartageLegs);
			filter.Property2 = now.AddDays(1);
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionContains(cartageLeg1, cartageLegs);
			filter.Property1 = now.AddDays(1);
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionNotContains(cartageLeg1, cartageLegs);
			filter.Property1 = ZDateTime.Empty;
			cartageLegs.AdditionalFilter = filters.Filter;
			AssertCollectionContains(cartageLeg1, cartageLegs);
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
			CartageLegFilterStripBusinessObject filter = new CartageLegFilterStripBusinessObject();
			if (typeof(T) == typeof(ZString))
			{
				((ModuleNkFilter)filter[filterName]).Property = new ZString(key2);
				((ModuleNkFilter)filter[filterName]).IsActive = true;
			}
			else if (typeof(T) == typeof(ZGuid))
			{
				((ModuleGuidFilter)filter[filterName]).Property = new ZGuid(key2);
				((ModuleGuidFilter)filter[filterName]).IsActive = true;
			}

			CommonCartageLegCollection collection = new CommonCartageLegCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", cartageLeg1, collection);
			AssertCollectionContains("Should have Cartage2", cartageLeg2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartageLeg3, collection);
			if (typeof(T) == typeof(ZString))
			{
				((ModuleNkFilter)filter[filterName]).Property = new ZString(key1);
			}
			else if (typeof(T) == typeof(ZGuid))
			{
				((ModuleGuidFilter)filter[filterName]).Property = new ZGuid(key1);
			}

			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", cartageLeg1, collection);
			AssertCollectionNotContains("Should not have Cartage2", cartageLeg2, collection);
			AssertCollectionNotContains("Should not have Cartage3", cartageLeg3, collection);
		}

		void TestWorkSheetNkFilter(ZString filterName, SchemaColumn schemaColumn, ZString key1, ZString key2)
		{
			TestWorkSheetFilter(filterName, schemaColumn, key1, key2);
		}

		void TestWorkSheetGuidFilter(ZString filterName, SchemaColumn schemaColumn, ZGuid pk1, ZGuid pk2)
		{
			TestWorkSheetFilter(filterName, schemaColumn, pk1, pk2);
		}

		public void TestFilterMaxLength()
		{
			var filterBizO = new CartageLegFilterStripBusinessObject();
			CombineAssertions(() =>
			{
				var slotReferenceMaxLength = ModuleNumberFilter.MultiplyMaxLength(Math.Min(JobContainerSchema.JC_DepartureSlotReference.MaxLength, JobContainerSchema.JC_ArrivalSlotReference.MaxLength));
				AssertEquals("MaxLength of Slot Reference should be set correctly.", slotReferenceMaxLength, filterBizO["Slot Reference"].MaxLength);
				AssertEquals("MaxLength of Port Transport # should be set correctly.", JobCartageSchema.JJ_ConsignmentID.MaxLength, filterBizO["Port Transport #"].MaxLength);
				AssertEquals("MaxLength of RunSheet # should be set correctly.", ModuleNumberFilter.MultiplyMaxLength(JobCartageRunSheetSchema.EY_RunSheetNumber.MaxLength), filterBizO["RunSheet #"].MaxLength);
				AssertEquals("MaxLength of Staff Driver should be set correctly.", JobCartageRunSheetSchema.EY_GS_NKTruckDriver.MaxLength, filterBizO["Staff Driver"].MaxLength);
				AssertEquals("MaxLength of Pickup City should be set correctly.", Math.Min(JobDocAddressSchema.E2_City.MaxLength, OrgAddressSchema.OA_City.MaxLength), filterBizO["Pickup City"].MaxLength);
				AssertEquals("MaxLength of Pickup PostCode should be set correctly.", Math.Min(JobDocAddressSchema.E2_Postcode.MaxLength, OrgAddressSchema.OA_PostCode.MaxLength), filterBizO["Pickup PostCode"].MaxLength);
				AssertEquals("MaxLength of Delivery City should be set correctly.", Math.Min(JobDocAddressSchema.E2_City.MaxLength, OrgAddressSchema.OA_City.MaxLength), filterBizO["Delivery City"].MaxLength);
				AssertEquals("MaxLength of Delivery PostCode should be set correctly.", Math.Min(JobDocAddressSchema.E2_Postcode.MaxLength, OrgAddressSchema.OA_PostCode.MaxLength), filterBizO["Delivery PostCode"].MaxLength);
				AssertEquals("MaxLength of Related Port should be set correctly.", Math.Min(OrgAddressSchema.OA_RL_NKRelatedPortCode.MaxLength, OrgHeaderSchema.OH_RL_NKClosestPort.MaxLength), filterBizO["Related Port"].MaxLength);
			});
		}

		[TestDate(2023, 5, 12)]
		public void TestMilestoneDateFilter()
		{
			var currentTime = ZDateTime.UtcNow;

			var cartageLegData = CreateCartageLegsWithDifferentParents();
			var milestone1 = cartageLegData["STC"].WorkflowItems.Milestones.AddNew();
			var milestone2 = cartageLegData["CFC"].WorkflowItems.Milestones.AddNew();
			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(currentTime));
			milestone2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(currentTime.AddMonths(6)));
			Factory.Save();

			var cartageLegs = GetCartageLegsWithMilestoneDateFilter(-1, 3);

			AssertContainsExactElementsInAnyOrder("Only Milestone 1 Leg should be found by the Milestone Date Range filter", new[] { cartageLegData["STC"] }, cartageLegs);
		}

		[TestDate(2023, 5, 12)]
		public void TestMilestoneCompletedFilter()
		{
			var currentTime = ZDateTime.UtcNow;

			var cartageLegData = CreateCartageLegsWithDifferentParents();
			var milestone1 = cartageLegData["STC"].WorkflowItems.Milestones.AddNew();
			var milestone2 = cartageLegData["CFC"].WorkflowItems.Milestones.AddNew();
			milestone1.SetMilestoneActualDateForTest(currentTime);
			milestone2.SetMilestoneActualDateForTest(ZDateTime.Empty);
			Factory.Save();

			var cartageLegs = GetCartageLegsWithMilestoneCompletedFilter("Completed");

			AssertContainsExactElementsInAnyOrder("Only returns milestones with a non-empty actual date completed", new[] { cartageLegData["STC"] }, cartageLegs);
		}

		[TestDate(2023, 5, 12)]
		public void TestMilestoneNextFilter()
		{
			var currentTime = ZDateTime.UtcNow;

			var cartageLegData = CreateCartageLegsWithDifferentParents();
			var milestone1 = cartageLegData["STC"].WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = "AID";
			var milestone2 = cartageLegData["CFC"].WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = "AID";
			milestone1.P9_Type = "MIL";
			milestone2.P9_Type = "MIL";
			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(currentTime));
			milestone2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(currentTime.AddMonths(6)));
			Factory.Save();

			var cartageLegs = GetCartageLegsWithMilestoneNextFilter(-1, 3);

			AssertContainsExactElementsInAnyOrder("Next scheduled is in Collection", new[] { cartageLegData["STC"] }, cartageLegs);
		}

		[TestDate(2023, 5, 12)]
		public void TestMilestoneLastCompletedFilter()
		{
			var currentTime = ZDateTime.UtcNow;

			var cartageLegData = CreateCartageLegsWithDifferentParents();
			var milestone1 = cartageLegData["STC"].WorkflowItems.Milestones.AddNew();
			var milestone2 = cartageLegData["CFC"].WorkflowItems.Milestones.AddNew();
			milestone1.P9_Status = "LST";
			milestone2.P9_Status = "LST";
			milestone1.SetMilestoneActualDateForTest(currentTime);
			milestone2.SetMilestoneActualDateForTest(currentTime.AddMonths(6));
			Factory.Save();

			var cartageLegs = GetCartageLegsWithMilestoneLastCompletedFilter(-1, 3);

			AssertContainsExactElementsInAnyOrder("Stand-alone Cartage is in Collection", new[] { cartageLegData["STC"] }, cartageLegs);
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return testHelper ?? (testHelper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper testHelper;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CartageLegFilterStripBusinessObject();
		}
	}
}
