using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Module
{
	public class CartageLegFilterStripBusinessObject : FilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter constant.")]
		public static class FilterNameConstants
		{
			public const string VoyageVessel = "Vessel and Flight/Voyage #";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			AddHiddenFilters(filters);
			AddNumbersAndReferencesFilters(filters);
			AddDatesFilters(filters);
			AddOrganisationFilters(filters);
			AddModesAndTypesFilters(filters);
			AddStatusAndFlagsFilters(filters);
			AddLocationFilters(filters);

			return filters;
		}

		void AddHiddenFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("HiddenFilter", GetCartageOnlyLegsQuery).Visibility = FilterVisibility.AlwaysAppliedAndHidden;

			var activeStatusFilter = filters.AddTextFilter("Port Transport Job Active Status", GetActiveStatusQuery, CancelledStatusList);
			activeStatusFilter.DefaultProperty = StatusActive;
			activeStatusFilter.Category = FilterCategories.StatusAndFlags;
			activeStatusFilter.Visibility = FilterVisibility.AlwaysApplied;
			activeStatusFilter.MultilingualDescription = ResString.GetMultilingualString("LocalTransportJob|ActiveStatus", "Port Transport Job Active Status");
			activeStatusFilter.SubGroup = CartageSubGroup;
		}

		ZQuery GetCartageOnlyLegsQuery(SQLComparisonOperator comparisonToBeIgnored, ZString valueToBeIgnored)
		{
			return new ZQuery(JobContainerLegsSchema.JU_EW, SQLComparisonOperator.NotEqual, null);
		}

		ZQuery GetActiveStatusQuery(ZString status)
		{
			var query = new ZQuery();
			query.IgnoreActiveFilter = true;

			if (StatusInactive.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				query.AddToFilter(JobCartageSchema.JJ_IsCancelled, true);
			}
			else if (StatusActive.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				query.AddToFilter(JobCartageSchema.JJ_IsCancelled, false);
			}

			return query;
		}

		void AddNumbersAndReferencesFilters(ModuleFilterCollection filters)
		{
			SetupFilter(filters.AddNumberFilter("Container #", JobContainerSchema.JC_ContainerNum), ResString.GetMultilingualString("9919e3fe-2ef9-48f9-9e27-448958270eb4", "Container #"), FilterCategories.NumbersAndReferences, ContainerSubGroup);
			SetupFilter(filters.AddNumberFilter("Container Release #", JobContainerSchema.JC_ReleaseNum), ResString.GetMultilingualString("15093960-2c33-400f-8ab2-be2bc948082d", "Container Release #"), FilterCategories.NumbersAndReferences, ContainerSubGroup);
			var slotReferenceMaxLength = Math.Min(JobContainerSchema.JC_DepartureSlotReference.MaxLength, JobContainerSchema.JC_ArrivalSlotReference.MaxLength);
			SetupFilter(filters.AddNumberFilter("Slot Reference", GetSlotReferenceQuery), ResString.GetMultilingualString("9a42aa4a-ed62-402b-a4d7-7dda95ee9e4e", "Slot Reference"), FilterCategories.NumbersAndReferences, null, slotReferenceMaxLength);
			SetupFilter(filters.AddTextAndNkFilter(FilterNameConstants.VoyageVessel, GetFlightVoyageNumberAndVesselQuery, ModuleIDs.RefVessel, BindingLists.RefVessels).WithMaxLengthOf(ViewLocalTransportScheduleSchema.VLT_Voyage, ViewLocalTransportScheduleSchema.VLT_RV_NKVessel),
				ResString.GetMultilingualString("B6842A70-F3CB-4452-B0A6-395DB253E06B", "Vessel and Flight/Voyage #"), FilterCategories.NumbersAndReferences, ScheduleSubGroup);

			var filter = filters.AddNumberFilter("Port Transport #", GetCartageNumberQuery);
			filter.UseMultiSearch = false; //TODO: Needs to work with multiple values. (CartageQueryHelper.cs GetConsignmentIDQuery)
			SetupFilter(filter, ResString.GetMultilingualString("d02b6624-4903-4d51-880c-8dbed4b937f0", "Port Transport #"), FilterCategories.NumbersAndReferences, CartageSubGroup, JobCartageSchema.JJ_ConsignmentID.MaxLength);
			SetupFilter(filters.AddNumberFilter("Port Transport Order #", JobCartageSchema.JJ_OrderReferenceNumber), ResString.GetMultilingualString("85ff0b49-1bed-4c70-abb8-67a559c3d9cb", "Port Transport Order #"), FilterCategories.NumbersAndReferences, CartageSubGroup);
			SetupFilter(filters.AddNumberFilterWithoutIsBlankAndIsNotBlank((NoResString)"Port Transport Parent Job #", CartageQueryHelper.GetCartageParentJobNumberQuery), ResString.GetMultilingualString("cbc46624-065d-4ff7-afaa-04493f1ce71b", "Port Transport Parent Job #"), FilterCategories.NumbersAndReferences, CartageSubGroup);
			SetupFilter(filters.AddNumberFilter("Port Transport Quote #", JobCartageSchema.JJ_QuoteNumber), ResString.GetMultilingualString("0e76fb5a-8f37-4718-8e9e-abf36dc3aea3", "Port Transport Quote #"), FilterCategories.NumbersAndReferences, CartageSubGroup);
			SetupFilter(filters.AddNumberFilter("Port Transport Waybill #", JobCartageSchema.JJ_WaybillNumber), ResString.GetMultilingualString("4a2a42f7-a9b7-4951-a175-7e90fc6a24ff", "Port Transport Waybill #"), FilterCategories.NumbersAndReferences, CartageSubGroup);

			var runSheetNumberMaxLength = JobCartageRunSheetSchema.EY_RunSheetNumber.MaxLength;
			SetupFilter(filters.AddFountainFilter("RunSheet #", GetRunSheetNumberQuery, "RS"), ResString.GetMultilingualString("65dd2f6e-b8f9-4005-8726-259e9b836968", "Run Sheet #"), FilterCategories.NumbersAndReferences, null, runSheetNumberMaxLength);
		}

		ZQuery GetFlightVoyageNumberAndVesselQuery(SQLComparisonOperator comparisonOperator, ZString voyage, ZString vessel)
		{
			return VoyageVesselModuleFilterHelper.GetBasicVoyageVesselQuery(comparisonOperator, voyage, vessel, false, ViewLocalTransportScheduleSchema.VLT_Voyage, ViewLocalTransportScheduleSchema.VLT_RV_NKVessel);
		}

		ZQuery GetRunSheetNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CommonCartageLeg));

			if (@operator == Enterprise.ZArchitecture.Business.SpecialComparisonOperator.IsBlank)
			{
				query.AddToFilter(JobContainerLegsSchema.JU_EY_RunSheet, DBNull.Value);
			}
			else
			{
				var subQuery = new ZDBOnlySubQuery(typeof(CommonWorkSheet), JobContainerLegsSchema.JU_EY_RunSheet);
				subQuery.AddToFilter_PossiblyCommaSeparated(JobCartageRunSheetSchema.EY_RunSheetNumber, @operator, value);
				query.AddSubQuery(subQuery, JoinCondition.And);
			}

			return query;
		}

		ZQuery GetSlotReferenceQuery(SQLComparisonOperator @operator, ZString slotReference)
		{
			return GetSlotBookedQueryCore(@operator, slotReference);
		}

		ZQuery GetCartageNumberQuery(SQLComparisonOperator @operator, ZString cartageNo)
		{
			return CartageQueryHelper.GetConsignmentIDQuery(@operator, cartageNo);
		}

		void AddDatesFilters(ModuleFilterCollection filters)
		{
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.LocalTransportRegistrationDate, JobCartageSchema.JJ_SystemCreateTimeUtc, true), ResString.GetMultilingualString("8ef426d3-1c07-46a9-882e-013e18e071d6", "Port Transport Registration Date"), FilterCategories.Dates, CartageSubGroup);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.LocalTransportPlannedPickup, JobCartageSchema.JJ_EstimatedPickup), ResString.GetMultilingualString("a6ddc0a5-8408-42a2-b8d2-98e3970d3aed", "Port Transport Planned Pickup"), FilterCategories.Dates, CartageSubGroup);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.LocalTransportPlannedDelivery, JobCartageSchema.JJ_EstimatedDelivery), ResString.GetMultilingualString("a3e4ca7a-ef86-4d08-a18c-8e57f2c985b1", "Port Transport Planned Delivery"), FilterCategories.Dates, CartageSubGroup);

			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.PlannedPickup, JobContainerLegsSchema.JU_PlannedPickupTime), ResString.GetMultilingualString("660d9418-abe0-455c-ba26-3d6336a26244", "Planned Pickup"), FilterCategories.Dates);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.ActualPickupTimeIn, JobContainerLegsSchema.JU_PickupTimeIn), ResString.GetMultilingualString("a4353982-ae0a-4da0-9189-776770144047", "Actual Pickup Time In"), FilterCategories.Dates);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.ActualPickupTimeOut, JobContainerLegsSchema.JU_PickupTimeOut), ResString.GetMultilingualString("0c216a88-4dbd-48ff-9796-f4189ab7df3b", "Actual Pickup Time Out"), FilterCategories.Dates);

			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.PlannedDelivery, JobContainerLegsSchema.JU_EstimatedDeliveryTime), ResString.GetMultilingualString("b4a4b2eb-4fcc-4e0b-a67f-092cb81be41b", "Planned Delivery"), FilterCategories.Dates);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.ActualDeliveryTimeIn, JobContainerLegsSchema.JU_DeliverTimeIn), ResString.GetMultilingualString("73e30ef3-1057-409f-a2c9-9ebe9a72b7c6", "Actual Delivery Time In"), FilterCategories.Dates);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.ActualDeliveryTimeOut, JobContainerLegsSchema.JU_DeliverTimeOut), ResString.GetMultilingualString("e1c64f52-7d6e-4379-8670-246c56156a21", "Actual Delivery Time Out"), FilterCategories.Dates);

			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.EmptyReturnedOn, JobContainerSchema.JC_ContainerYardEmptyReturnGateIn), ResString.GetMultilingualString("747a3ef7-fe5c-471c-8733-2f8f1b8a4729", "Container Empty Returned On"), FilterCategories.Dates, ContainerSubGroup);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.EmptyRequired, JobContainerSchema.JC_EmptyRequired), ResString.GetMultilingualString("8705cd8e-1198-4129-b501-e64c7353b98c", "Container Empty Required"), FilterCategories.Dates, ContainerSubGroup);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.EmptyReturnBy, JobContainerSchema.JC_EmptyReturnedBy), ResString.GetMultilingualString("4a9746ef-0eaf-476a-b181-5e20eef253d5", "Container Empty Return By"), FilterCategories.Dates, ContainerSubGroup);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.SlotDate, GetSlotDateQuery), ResString.GetMultilingualString("7b49000d-4b05-4c35-9b0e-0549d2ecddc3", "Slot Date"), FilterCategories.Dates);

			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.PickupRequested, JobBookedCtgMoveSchema.EW_RequestedPickupTimeStart), ResString.GetMultilingualString("feff896d-e59d-411f-9b65-12dec99ea922", "Pickup Requested"), FilterCategories.Dates, BookedMoveSubGroup);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.PickupRequestedTo, JobBookedCtgMoveSchema.EW_RequestedPickupTimeEnd), ResString.GetMultilingualString("2327b26a-5e31-4fb4-b0a5-87db72fa8a12", "Pickup Requested To"), FilterCategories.Dates, BookedMoveSubGroup);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.DeliveryRequested, JobBookedCtgMoveSchema.EW_RequestedDeliveryTimeStart), ResString.GetMultilingualString("ac158b5a-be87-4ff1-9f78-048824a8c4d9", "Delivery Requested"), FilterCategories.Dates, BookedMoveSubGroup);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.DeliveryRequestedTo, JobBookedCtgMoveSchema.EW_RequestedDeliveryTimeEnd), ResString.GetMultilingualString("a5f7e237-d836-40c0-ab60-caca87962ff4", "Delivery Requested To"), FilterCategories.Dates, BookedMoveSubGroup);

			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.ScheduleETA, ViewLocalTransportScheduleSchema.VLT_ETA), ResString.GetMultilingualString("545aa580-ef93-40a7-a42e-9ae93dd4f825", "Schedule ETA"), FilterCategories.Dates, ScheduleSubGroup);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.ScheduleATA, ViewLocalTransportScheduleSchema.VLT_ATA), ResString.GetMultilingualString("30692f35-35ff-4718-a57f-d3ef8bc6f035", "Schedule ATA"), FilterCategories.Dates, ScheduleSubGroup);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.ScheduleETD, ViewLocalTransportScheduleSchema.VLT_ETD), ResString.GetMultilingualString("6bb426f5-1cb5-4eae-a10e-dea176a89c28", "Schedule ETD"), FilterCategories.Dates, ScheduleSubGroup);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.ScheduleATD, ViewLocalTransportScheduleSchema.VLT_ATD), ResString.GetMultilingualString("f0e33bd0-14b8-4243-be5a-ceab18ac2050", "Schedule ATD"), FilterCategories.Dates, ScheduleSubGroup);

			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.ScheduleLCLAvailability, ViewLocalTransportScheduleSchema.VLT_LCLAvailabilityDate), ResString.GetMultilingualString("f134861f-79ea-4f58-b9f5-81cb3c65235a", "Schedule CFS Available"), FilterCategories.Dates, ScheduleSubGroup);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.ScheduleLCLCutOff, ViewLocalTransportScheduleSchema.VLT_LCLCutOff), ResString.GetMultilingualString("20969d7e-8212-430d-a207-ef25a1024eac", "Schedule CFS Cut Off"), FilterCategories.Dates, ScheduleSubGroup);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.ScheduleLCLReceivalCommences, ViewLocalTransportScheduleSchema.VLT_LCLReceivalCommences), ResString.GetMultilingualString("13be84ac-1916-4dd8-b8da-0293e7033091", "Schedule CFS Receival Start"), FilterCategories.Dates, ScheduleSubGroup);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.ScheduleLCLStorage, ViewLocalTransportScheduleSchema.VLT_LCLStorageDate), ResString.GetMultilingualString("fa336e19-5f75-402e-b02d-27a8e3592ef3", "Schedule CFS Storage Start"), FilterCategories.Dates, ScheduleSubGroup);

			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.ScheduleFCLAvailability, ViewLocalTransportScheduleSchema.VLT_FCLAvailabilityDate), ResString.GetMultilingualString("c236bd1f-ec0f-4f32-b54c-6d7ce13457c9", "Schedule CTO Available"), FilterCategories.Dates, ScheduleSubGroup);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.ScheduleFCLCutOff, ViewLocalTransportScheduleSchema.VLT_FCLCutOff), ResString.GetMultilingualString("471aa963-f2f4-4e83-925e-d2704e31187e", "Schedule CTO Cut Off"), FilterCategories.Dates, ScheduleSubGroup);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.ScheduleFCLReceivalCommences, ViewLocalTransportScheduleSchema.VLT_FCLReceivalCommences), ResString.GetMultilingualString("4184e63d-a885-4c4a-848f-ad77fd11ea80", "Schedule CTO Receival Start"), FilterCategories.Dates, ScheduleSubGroup);
			SetupFilter(filters.AddDateFilter(CartageLegFilterConstants.ScheduleFCLStorage, ViewLocalTransportScheduleSchema.VLT_FCLStorageDate), ResString.GetMultilingualString("6e190c4d-eff4-4317-927c-496ebc730faf", "Schedule CTO Storage Start"), FilterCategories.Dates, ScheduleSubGroup);
		}

		ZQuery GetSlotDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetSlotBookedQueryCore(comparisonOperator, date1, date2);
		}

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			SetupFilter(filters.AddGuidFilter("Local Client", ModuleIDs.Organisation, GetLocalClientQuery, BindingLists.Organisations), ResString.GetMultilingualString("E9DEB84A-F32B-4799-A613-85B29C5ACD36", "Local Client"), FilterCategories.Organisations);

			ModuleGuidsFilter pickupDeliveryFilter = filters.AddGuidFilter("Pickup/Delivery", ModuleIDs.Organisation, GetPickupDeliveryOrganisationQuery, BindingLists.Organisations, BindingLists.Organisations);
			pickupDeliveryFilter.MultilingualDescription = ResString.GetMultilingualString("20E0AB7A-0E33-41FA-8BFE-1F7EDCBB6295", "Pickup/Delivery");
			pickupDeliveryFilter.Category = FilterCategories.Organisations;
			pickupDeliveryFilter.SetItemDescriptions(Res.GetData("486bdcaf-817e-4777-8271-ded40e98962f", "Pickup"), Res.GetData("a302b994-d400-4cd4-9cad-2aa339de7359", "Delivery"));

			SetupFilter(filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, JobCartageSchema.JJ_GB, BindingLists.Branches), ResString.GetMultilingualString("07b2af9e-94cf-41b8-8aee-3a66a5cccd0a", "Branch"), FilterCategories.Organisations, CartageSubGroup);
			SetupFilter(filters.AddGuidFilter("Vehicle", ModuleIDs.RefEquipment, JobCartageRunSheetSchema.EY_RQ_Truck, BindingLists.Vehicles), ResString.GetMultilingualString("fb38c533-e9f3-475d-a7b8-0c2ec969cb02", "Vehicle"), FilterCategories.Organisations, RunSheetSubGroup);
			SetupFilter(filters.AddTextFilter("Vehicle Registration", JobCartageRunSheetSchema.EY_TruckRegistration), ResString.GetMultilingualString("8b1744f0-3f35-46ee-80ad-3fa3b63d6771", "Vehicle Registration"), FilterCategories.Organisations, RunSheetSubGroup);

			var transportCompanyFilter = filters.AddGuidFilter("Transport Company", ModuleIDs.Organisation, GetTransportCompanyQuery, BindingLists.TransportProviders);
			transportCompanyFilter.Category = FilterCategories.Organisations;
			transportCompanyFilter.MultilingualDescription = ResString.GetMultilingualString("d1acf903-85ca-4467-9b26-a9569af98399", "Transport Company");

			var driverNKLength = JobCartageRunSheetSchema.EY_GS_NKTruckDriver.MaxLength;
			SetupFilter(filters.AddNkFilter("Staff Driver", GetStaffDriverQuery, ModuleIDs.GlbStaff, BindingLists.StaffDrivers), ResString.GetMultilingualString("f081bf7b-869c-487e-b432-a6d82c6fa478", "Staff Driver"), FilterCategories.Organisations, null, driverNKLength);

			SetupFilter(filters.AddTextFilter("Transport Company Name (Non-Org)", JobCartageRunSheetSchema.EY_TransportCoName), ResString.GetMultilingualString("95361bb0-cd6e-4d44-b30d-e5f687a03f3b", "Transport Company Name (Non-Org.)"), FilterCategories.Organisations, RunSheetSubGroup).MultilingualDescription = ResString.GetMultilingualString("LocalTransport|LegFilter|TransportCompanyName(Non-Organization)", "Transport Company Name (Non-Org.)");
			SetupFilter(filters.AddTextFilter("Drivers Name (Non-Staff)", JobCartageRunSheetSchema.EY_DriversName), ResString.GetMultilingualString("2238c517-5e3a-44e8-be79-49fb652d0be7", "Drivers Name (Non-Staff)"), FilterCategories.Organisations, RunSheetSubGroup).MultilingualDescription = ResString.GetMultilingualString("LocalTransport|LegFilter|DriversName(Non-Staff)", "Drivers Name (Non-Staff)");
			SetupFilter(filters.AddTextFilter("Drivers Licence (Non-Staff)", JobCartageRunSheetSchema.EY_DriversLicence), ResString.GetMultilingualString("d75c2cb4-21c5-4ef0-883f-ce534eb6bcc5", "Drivers License (Non-Staff)"), FilterCategories.Organisations, RunSheetSubGroup).MultilingualDescription = ResString.GetMultilingualString("LocalTransport|LegFilter|DriversLicence(Non-Staff)", "Drivers License (Non-Staff)");
		}

		ZQuery GetTransportCompanyQuery(SQLComparisonOperator comparisonOperator, object value)
		{
			var cartageLegQuery = new ZDBOnlyQuery(typeof(CommonCartageLeg));

			var runSheetSubQuery = new ZDBOnlySubQuery(typeof(CommonWorkSheet), JobContainerLegsSchema.JU_EY_RunSheet);
			runSheetSubQuery.AddToFilter(JobCartageRunSheetSchema.EY_OH_TransportCo, comparisonOperator, value);

			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				cartageLegQuery.AddToFilter(JobContainerLegsSchema.JU_EY_RunSheet, DBNull.Value);
				cartageLegQuery.AddSubQuery(runSheetSubQuery, JoinCondition.Or);
			}
			else if (comparisonOperator == SQLComparisonOperator.NotEqual)
			{
				runSheetSubQuery.AddToFilter(JoinCondition.Or, JobCartageRunSheetSchema.EY_OH_TransportCo, DBNull.Value);

				cartageLegQuery.AddToFilter(JobContainerLegsSchema.JU_EY_RunSheet, DBNull.Value);
				cartageLegQuery.AddSubQuery(runSheetSubQuery, JoinCondition.Or);
			}
			else
			{
				cartageLegQuery.AddSubQuery(runSheetSubQuery, JoinCondition.And);
			}

			return cartageLegQuery;
		}

		ZQuery GetLocalClientQuery(ZGuid organisationPK)
		{
			return GetCartageQuery(CartageQueryHelper.GetLocalClientQuery(organisationPK));
		}

		ZQuery GetPickupDeliveryOrganisationQuery(ZGuid organisation1PK, ZGuid organisation2PK)
		{
			ZQuery picLegQuery = new ZQuery();
			ZQuery dlvLegQuery = new ZQuery();

			if (organisation1PK.IsValid)
			{
				ZQuery picAddressQuery = new ZQuery();
				picAddressQuery.AddToFilter(CartageQueryHelper.GetDocAddressOnlyQuery(organisation1PK, Array.Empty<DocAddressType>(), Factory));
				picLegQuery = GetDocAddressQuery(picAddressQuery, new SchemaColumn[] { JobContainerLegsSchema.JU_E2PickupAddressID, JobContainerLegsSchema.JU_E2WaitPointAddressID });
			}

			if (organisation2PK.IsValid)
			{
				ZQuery dlvAddressQuery = new ZQuery();
				dlvAddressQuery.AddToFilter(CartageQueryHelper.GetDocAddressOnlyQuery(organisation2PK, Array.Empty<DocAddressType>(), Factory));
				dlvLegQuery = GetDocAddressQuery(dlvAddressQuery, new SchemaColumn[] { JobContainerLegsSchema.JU_E2WaitPointAddressID, JobContainerLegsSchema.JU_E2DeliveryAddressID });
			}

			ZQuery legQuery = new ZQuery();
			legQuery.AddToFilter(picLegQuery);
			legQuery.AddToFilter(dlvLegQuery);
			return legQuery;
		}

		void AddModesAndTypesFilters(ModuleFilterCollection filters)
		{
			SetupFilter(filters.AddTextFilter("Drop Mode", JobBookedCtgMoveSchema.EW_DropMode, BindingLists.DropModes), ResString.GetMultilingualString("3ae1b478-3179-4af1-a3d2-1a6e91cbe815", "Drop Mode"), FilterCategories.ModesAndTypes, BookedMoveSubGroup);
			SetupFilter(filters.AddTextFilter("Port Transport Job Type", JobCartageSchema.JJ_E3_NKJobType, BindingLists.NewCartageJobTypes), ResString.GetMultilingualString("8ba6e5c7-b45a-41b6-9e47-179bd79ba740", "Port Transport Job Type"), FilterCategories.ModesAndTypes, CartageSubGroup);
			SetupFilter(filters.AddTextFilter("Port Transport Parent Job Type", CartageQueryHelper.GetCartageParentJobTypeQuery, BindingLists.ParentJobTypes), ResString.GetMultilingualString("4d78993e-379a-4d77-a354-4f471461646b", "Port Transport Parent Job Type"), FilterCategories.ModesAndTypes, CartageSubGroup);
			SetupFilter(filters.AddNkFilter("Port Transport Service Level", JobCartageSchema.JJ_RS_NKServiceLevel, ModuleIDs.ServiceLevel, BindingLists.ServiceLevels), ResString.GetMultilingualString("9bdbae01-20d2-4476-88f0-594c1c17df4a", "Port Transport Service Level"), FilterCategories.ModesAndTypes, CartageSubGroup);
			SetupFilter(filters.AddTextFilter("Job Type", GetContainerConsolQuery, BindingLists.LoadListTypeList), ResString.GetMultilingualString("1f622770-82ce-4d9f-bbe6-6013e5c74996", "Job Type"), FilterCategories.ModesAndTypes, ContainerSubGroup);
			SetupFilter(filters.AddTextFilter("Direction", GetDirectionQuery, BindingLists.Directions), ResString.GetMultilingualString("fb906bfa-43c4-456c-815f-b27f6f843880", "Direction"), FilterCategories.ModesAndTypes, CartageSubGroup);
			SetupFilter(filters.AddTextFilter("Connecting Freight Mode", GetConnectingFreightModeQuery, BindingLists.ShippingTransportModeList), ResString.GetMultilingualString("18b68e1d-d9c3-4449-873a-696c9b1364c1", "Connecting Freight Mode"), FilterCategories.ModesAndTypes, CartageSubGroup);
			SetupFilter(filters.AddTextFilter("Container Status", JobContainerSchema.JC_ContainerStatus, FreightDataRegistry.Instance.ContainerStatusList.Value), ResString.GetMultilingualString("c69e5e9a-d09a-4441-b6c7-757a93979347", "Container Status"), FilterCategories.ModesAndTypes, ContainerSubGroup);
			SetupFilter(filters.AddTextFilter("Leg Type", GetLegType, LegTypeList), ResString.GetMultilingualString("e74e4a6f-39f8-4050-b59e-4442ac483d2a", "Leg Type"), FilterCategories.ModesAndTypes, BookedMoveSubGroup);

			SetupFilter(filters.AddTextFilter("Container Service", GetContainerServiceQuery, ContainerServiceList), ResString.GetMultilingualString("6C15B4D7-D207-485F-BE1A-3964ED06EAB3", "Container Service"), FilterCategories.ModesAndTypes);
		}

		ZQuery GetContainerServiceQuery(ZString serviceLevel)
		{
			ZDBOnlyQuery jobContainerQuery = new ZDBOnlyQuery(typeof(CommonContainer));

			ZQuery serviceQuery = new ZQuery(JobServiceSchema.ES_ParentTableCode, SQLComparisonOperator.Equal, JobContainerSchema.Constants.Prefix);

			ZDBOnlySubQuery serviceSubQuery = new ZDBOnlySubQuery(typeof(JobService), JobServiceSchema.ES_ParentID);

			if (!(serviceLevel == CartageLegFilterConstants.ContainerServiceAny || serviceLevel == CartageLegFilterConstants.ContainerServiceNone))
			{
				serviceQuery.AddToFilter(JobServiceSchema.ES_ServiceCode, SQLComparisonOperator.Equal, serviceLevel);
			}

			serviceSubQuery.AddToFilter(serviceQuery);
			jobContainerQuery.AddSubQuery(serviceSubQuery, JoinCondition.And);

			return GetContainerQuery(jobContainerQuery, serviceLevel == CartageLegFilterConstants.ContainerServiceNone);
		}

		ZQuery GetContainerConsolQuery(ZString jobType)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CommonContainer));

			if (jobType == BindingLists.LoadListTypeList_CFS)
			{
				query.AddToFilter(JobContainerSchema.JC_JK, SQLComparisonOperator.NotEqual, null);
			}
			else if (jobType == BindingLists.LoadListTypeList_TRS)
			{
				query.AddToFilter(JobContainerSchema.JC_JK, SQLComparisonOperator.Equal, null);
			}
			return query;
		}

		ZQuery GetDirectionQuery(ZString direction)
		{
			ZString oneCharDirection = BindingLists.OneCharDirections.GetCodeFromDescription(direction);

			var cartageQuery = new ZQuery();
			cartageQuery.AddToFilter(JobCartageSchema.JJ_E3_NKJobType, SQLComparisonOperator.StartsWith, oneCharDirection);
			return cartageQuery;
		}

		ZQuery GetStaffDriverQuery(SQLComparisonOperator comparisonOperator, ZString pk)
		{
			var legWithRunSheetQuery = GetWorkSheetQuery(new ZQuery(JobCartageRunSheetSchema.EY_GS_NKTruckDriver, comparisonOperator, pk));

			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				legWithRunSheetQuery.AddToFilter(JoinCondition.Or, JobContainerLegsSchema.JU_EY_RunSheet, SQLComparisonOperator.Equal, DBNull.Value);
			}

			return legWithRunSheetQuery;
		}

		ZQuery GetLegType(ZString legType)
		{
			ZQuery result = new ZQuery();

			if (legType.Trim().EqualsIgnoringCase(Constants.ContainerModes.Loose))
			{
				result.AddToFilter(JobBookedCtgMoveSchema.EW_JC_Container, SQLComparisonOperator.Equal, null);
			}
			else if (legType.Trim().EqualsIgnoringCase(Constants.ContainerModes.Containerised))
			{
				result.AddToFilter(JobBookedCtgMoveSchema.EW_JC_Container, SQLComparisonOperator.NotEqual, null);
			}

			return result;
		}

		ZQuery GetConnectingFreightModeQuery(ZString connectingFreightMode)
		{
			ZString oneCharFreightMode = BindingLists.OneCharConnectingFreightModes.GetCodeFromDescription(connectingFreightMode);

			var cartageQuery = new ZQuery();
			cartageQuery.AddFilterAndZSQLParameterCollection("SUBSTRING(" + JobCartageSchema.JJ_E3_NKJobType.Name + ", 2, 1) = '" + oneCharFreightMode + "'", null);
			return cartageQuery;
		}

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			SetupFilter(filters.AddTextFilter("Container/Leg Empty", GetEmptyStatusQuery, EmptyStatusList), ResString.GetMultilingualString("18b2a999-1bb0-4399-bd3d-b9c8ca1af4a2", "Container/Leg Empty"), FilterCategories.StatusAndFlags);
			SetupFilter(filters.AddTextFilter("Slot Booked", GetSlotBookedQuery, SlotStatusList), ResString.GetMultilingualString("75119ec6-ae43-4d20-9025-421d267ca1b4", "Slot Booked"), FilterCategories.StatusAndFlags);
			SetupFilter(filters.AddTextFilter("Completed", GetCompletedQuery, CompleteStatusList), ResString.GetMultilingualString("7a84ee00-7ef4-42e4-b7e1-2ac8e5f4a4e2", "Completed"), FilterCategories.StatusAndFlags);
			SetupFilter(filters.AddTextFilter("Message Status", GetMessageStatusQuery, BindingLists.MessageStatuses), ResString.GetMultilingualString("606fe82b-6a94-4c44-8045-1aed680c80d1", "Message Status"), FilterCategories.StatusAndFlags);
			SetupFilter(filters.AddTextFilter("RunSheet Allocated", GetWorkSheetAllocatedQuery, WorkSheetStatusList), ResString.GetMultilingualString("d97fb838-7786-4dad-8f2e-6ec52dd115c2", "Run Sheet Allocated"), FilterCategories.StatusAndFlags);

			SetupFilter(filters.AddTextFilter("Dangerous Goods", GetHasDangerousGoodsQuery, HasDangerousGoodsList), ResString.GetMultilingualString("65201dcc-eb1c-4c86-a0de-749877d1eb24", "Dangerous Goods"), FilterCategories.StatusAndFlags, BookedMoveSubGroup);
		}

		ZQuery GetMessageStatusQuery(ZString messageStatus)
		{
			ZQuery result = new ZQuery(JobContainerLegsSchema.JU_MessageStatus, messageStatus);
			if (messageStatus == Constants.CartageLegDispatchStatusList.Codes.NotStarted)
			{
				result.AddToFilter(JoinCondition.Or, JobContainerLegsSchema.JU_MessageStatus, "");
			}

			return result;
		}

		ZQuery GetEmptyStatusQuery(ZString emptyStatus)
		{
			ZQuery result = new ZQuery();

			if (emptyStatus.Trim().EqualsIgnoringCase(CartageLegFilterConstants.Empty))
			{
				result.AddToFilter(JobContainerLegsSchema.JU_IsEmptyContainer, true);
			}
			else if (emptyStatus.Trim().EqualsIgnoringCase(CartageLegFilterConstants.NonEmpty))
			{
				result.AddToFilter(JobContainerLegsSchema.JU_IsEmptyContainer, false);
			}

			return result;
		}

		ZQuery GetHasDangerousGoodsQuery(ZString hasDangerousGoodsStatus)
		{
			if (hasDangerousGoodsStatus.Trim().EqualsIgnoringCase(CartageLegFilterConstants.DangerousGoodsBoth))
			{
				return new ZQuery();
			}

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CommonBookedCtgMove));

			ZDBOnlySubQuery goodsSubQuery = new ZDBOnlySubQuery(typeof(UNDGDataItem), UNDGDataItemSchema.DI_ParentID,
				!hasDangerousGoodsStatus.Trim().EqualsIgnoringCase(CartageLegFilterConstants.DangerousGoodsHas));

			ZQuery serviceQuery = new ZQuery(UNDGDataItemSchema.DI_ParentTableCode, SQLComparisonOperator.Equal, JobBookedCtgMoveSchema.Constants.Prefix);

			goodsSubQuery.AddToFilter(serviceQuery);
			result.AddSubQuery(goodsSubQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetWorkSheetAllocatedQuery(ZString workSheetStatus)
		{
			ZQuery result = new ZQuery();

			if (workSheetStatus.Trim().EqualsIgnoringCase(CartageLegFilterConstants.Allocated))
			{
				result.AddToFilter(JobContainerLegsSchema.JU_EY_RunSheet, SQLComparisonOperator.NotEqual, null);
			}
			else if (workSheetStatus.Trim().EqualsIgnoringCase(CartageLegFilterConstants.Unallocated))
			{
				result.AddToFilter(JobContainerLegsSchema.JU_EY_RunSheet, SQLComparisonOperator.Equal, null);
			}

			return result;
		}

		ZQuery GetSlotBookedQuery(ZString slotStatus)
		{
			ZQuery result;

			if (slotStatus.EqualsIgnoringCase(CartageLegFilterConstants.Booked))
			{
				result = GetSlotBookedQueryCore(JoinCondition.And, true, true, DateComparisonOperator.HasDateEntered, ZDateTime.Empty, ZDateTime.Empty, SQLComparisonOperator.NotEqual, "");
			}
			else if (slotStatus.EqualsIgnoringCase(CartageLegFilterConstants.NonBooked))
			{
				result = GetSlotBookedQueryCore(JoinCondition.Or, true, true, DateComparisonOperator.HasNoDateEntered, ZDateTime.Empty, ZDateTime.Empty, SQLComparisonOperator.Equal, "");
			}
			else
			{
				result = new ZQuery();
			}

			return result;
		}

		ZQuery GetSlotBookedQueryCore(DateComparisonOperator dateComparisonOp, ZDateTime slotTime1, ZDateTime slotTime2)
		{
			return GetSlotBookedQueryCore(JoinCondition.And, true, false, dateComparisonOp, slotTime1, slotTime2, SQLComparisonOperator.Equal, "");
		}

		ZQuery GetSlotBookedQueryCore(SQLComparisonOperator @operator, ZString reference)
		{
			return GetSlotBookedQueryCore(JoinCondition.And, false, true, DateComparisonOperator.HasDateEntered, ZDateTime.Empty, ZDateTime.Empty, @operator, reference);
		}

		ZQuery GetSlotBookedQueryCore(JoinCondition dateAndReferenceJoinCondition, bool filterDate, bool filterReference, DateComparisonOperator dateComparisonOp, ZDateTime slotTime1, ZDateTime slotTime2, SQLComparisonOperator @operator, ZString reference)
		{
			var departureContainerQuery = new ZDBOnlyQuery(typeof(CommonContainer));
			var arrivalContainerQuery = new ZDBOnlyQuery(typeof(CommonContainer));

			if (filterDate)
			{
				AddDateRange(departureContainerQuery, dateComparisonOp, dateAndReferenceJoinCondition, JobContainerSchema.JC_DepartureSlotDateTime, slotTime1.Date, slotTime2.Date);
				AddDateRange(arrivalContainerQuery, dateComparisonOp, dateAndReferenceJoinCondition, JobContainerSchema.JC_ArrivalSlotDateTime, slotTime1.Date, slotTime2.Date);
			}

			if (filterReference)
			{
				departureContainerQuery.AddToFilter_PossiblyCommaSeparated(dateAndReferenceJoinCondition, JobContainerSchema.JC_DepartureSlotReference, @operator, reference);
				arrivalContainerQuery.AddToFilter_PossiblyCommaSeparated(dateAndReferenceJoinCondition, JobContainerSchema.JC_ArrivalSlotReference, @operator, reference);
			}

			// departure slot - delivery to CTO
			var departureAddressQuery = new ZQuery(CartageQueryHelper.GetDocAddressOnlyQuery(null, null, null, SQLComparisonOperator.Equal, DocAddressType.LocalCartageCTO, Factory));
			var departureQuery = GetDocAddressQuery(departureAddressQuery, new SchemaColumn[] { JobContainerLegsSchema.JU_E2DeliveryAddressID });
			departureQuery.AddToFilter(GetContainerQuery(departureContainerQuery), JoinCondition.And);

			// arrival slot - pickup from CTO
			var arrivalDocAddressQuery = new ZQuery(CartageQueryHelper.GetDocAddressOnlyQuery(null, null, null, SQLComparisonOperator.Equal, DocAddressType.LocalCartageCTO, Factory));
			var arrivalQuery = GetDocAddressQuery(arrivalDocAddressQuery, new SchemaColumn[] { JobContainerLegsSchema.JU_E2PickupAddressID });
			arrivalQuery.AddToFilter(GetContainerQuery(arrivalContainerQuery), JoinCondition.And);

			var result = new ZQuery();
			result.AddToFilter(arrivalQuery);
			result.AddToFilter(departureQuery, JoinCondition.Or);
			return result;
		}

		ZQuery GetCompletedQuery(ZString completeStatus)
		{
			ZQuery result = new ZQuery();

			if (completeStatus.Trim().EqualsIgnoringCase(CartageLegFilterConstants.Complete))
			{
				result.AddToFilter(JobContainerLegsSchema.JU_DeliverTimeOut, SQLComparisonOperator.NotEqual, null);
			}
			else if (completeStatus.Trim().EqualsIgnoringCase(CartageLegFilterConstants.Incomplete))
			{
				result.AddToFilter(JobContainerLegsSchema.JU_DeliverTimeOut, SQLComparisonOperator.Equal, null);
			}

			return result;
		}

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			var cityMaxLength = Math.Min(JobDocAddressSchema.E2_City.MaxLength, OrgAddressSchema.OA_City.MaxLength);
			SetupFilter(filters.AddTextFilter("Pickup City", GetPickupCityQuery), ResString.GetMultilingualString("6ae331bd-66e8-47cb-8d22-1c2c37ea70e7", "Pickup City"), FilterCategories.Locations, null, cityMaxLength);
			SetupFilter(filters.AddTextFilter("Delivery City", GetDeliveryCityQuery), ResString.GetMultilingualString("a3cdbfb9-ed74-453e-baca-c1d11cb5289f", "Delivery City"), FilterCategories.Locations, null, cityMaxLength);
			var postCodeMaxLength = Math.Min(JobDocAddressSchema.E2_Postcode.MaxLength, OrgAddressSchema.OA_PostCode.MaxLength);
			SetupFilter(filters.AddTextFilter("Pickup PostCode", GetPickupPostCodeQuery), ResString.GetMultilingualString("3e2688fc-4c20-4e3d-bde8-deccc847a094", "Pickup Postcode"), FilterCategories.Locations, null, postCodeMaxLength);
			SetupFilter(filters.AddTextFilter("Delivery PostCode", GetDeliveryPostCodeQuery), ResString.GetMultilingualString("776b30ee-8579-4ec2-a947-75991f2e002f", "Delivery Postcode"), FilterCategories.Locations, null, postCodeMaxLength);
			SetupFilter(filters.AddTextFilter("Pickup Address Type", GetPickupAddressTypeQuery, LocalCartageJobOrgTypeList.Instance), ResString.GetMultilingualString("da54f5e7-7389-435f-92bf-d81832bceb7e", "Pickup Address Type"), FilterCategories.Locations);
			SetupFilter(filters.AddTextFilter("Delivery Address Type", GetDeliveryAddressTypeQuery, LocalCartageJobOrgTypeList.Instance), ResString.GetMultilingualString("20f4beb5-4804-49e2-9cb7-40bf78d765f5", "Delivery Address Type"), FilterCategories.Locations);

			var loadDischFilter = filters.AddLocationFilter("Load / Discharge", ViewLocalTransportScheduleSchema.VLT_RL_NKLoadPort, BindingLists.RefLocations, ViewLocalTransportScheduleSchema.VLT_RL_NKDischargePort, BindingLists.RefLocations);
			loadDischFilter.MultilingualDescription = ResString.GetMultilingualString("2231F4E5-4D14-4913-9481-A23526D4A1E9", "Load / Discharge");
			loadDischFilter.SetItemDescriptions(Res.GetData("LocalTransport|CartageLegFilter|Load", "Load"), Res.GetData("LocalTransport|CartageLegFilter|Discharge", "Discharge"));
			loadDischFilter.SubGroup = ScheduleSubGroup;

			var relatedPortFilter = filters.AddNkFilter("Related Port", GetRelatedPortQuery, ModuleIDs.RefUNLOCO, new RefUNLOCOCollection(Factory));
			relatedPortFilter.Category = FilterCategories.Locations;
			relatedPortFilter.MultilingualDescription = ResString.GetMultilingualString("71c03709-c315-4d4b-9d9a-86bb7198e45f", "Related Port");
			relatedPortFilter.MaxLength = Math.Min(OrgAddressSchema.OA_RL_NKRelatedPortCode.MaxLength, OrgHeaderSchema.OH_RL_NKClosestPort.MaxLength);
		}

		ZQuery GetRelatedPortQuery(ZString relatedPort)
		{
			ZDBOnlySubQuery orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			orgAddressQuery.AddToFilter(OrgAddressSchema.OA_RL_NKRelatedPortCode, SQLComparisonOperator.StartsWith, relatedPort);

			ZDBOnlySubQuery orgAddress_WithHeaderQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			orgAddress_WithHeaderQuery.AddToFilter(OrgAddressSchema.OA_RL_NKRelatedPortCode, SQLComparisonOperator.Equal, ZString.Empty);
			ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
			orgSubQuery.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, relatedPort);
			orgAddress_WithHeaderQuery.AddSubQuery(orgSubQuery, JoinCondition.And);

			ZDBOnlyQuery docAddressWithOrgAddressQuery = new ZDBOnlyQuery(typeof(JobDocAddress));
			docAddressWithOrgAddressQuery.AddSubQuery(orgAddressQuery, JoinCondition.Or);
			docAddressWithOrgAddressQuery.AddSubQuery(orgAddress_WithHeaderQuery, JoinCondition.Or);

			return GetDocAddressQuery(docAddressWithOrgAddressQuery, new SchemaColumn[] { JobContainerLegsSchema.JU_E2PickupAddressID, JobContainerLegsSchema.JU_E2WaitPointAddressID, JobContainerLegsSchema.JU_E2DeliveryAddressID });
		}

		ZQuery GetPickupCityQuery(SQLComparisonOperator comparisonOperator, ZString city)
		{
			ZQuery docAddressQuery = new ZQuery();
			if (!city.IsEmpty)
			{
				docAddressQuery.AddToFilter(CartageQueryHelper.GetDocAddressOnlyQuery(JobDocAddressSchema.E2_City, OrgAddressSchema.OA_City, city, comparisonOperator, Array.Empty<DocAddressType>(), Factory));
			}

			return GetDocAddressQuery(docAddressQuery, new SchemaColumn[] { JobContainerLegsSchema.JU_E2PickupAddressID, JobContainerLegsSchema.JU_E2WaitPointAddressID });
		}

		ZQuery GetDeliveryCityQuery(SQLComparisonOperator comparisonOperator, ZString city)
		{
			ZQuery docAddressQuery = new ZQuery();
			if (!city.IsEmpty)
			{
				docAddressQuery.AddToFilter(CartageQueryHelper.GetDocAddressOnlyQuery(JobDocAddressSchema.E2_City, OrgAddressSchema.OA_City, city, comparisonOperator, Array.Empty<DocAddressType>(), Factory));
			}

			return GetDocAddressQuery(docAddressQuery, new SchemaColumn[] { JobContainerLegsSchema.JU_E2WaitPointAddressID, JobContainerLegsSchema.JU_E2DeliveryAddressID });
		}

		ZQuery GetPickupPostCodeQuery(SQLComparisonOperator comparisonOperator, ZString postCode)
		{
			ZQuery docAddressQuery = new ZQuery();
			if (!postCode.IsEmpty)
			{
				docAddressQuery.AddToFilter(CartageQueryHelper.GetDocAddressOnlyQuery(JobDocAddressSchema.E2_Postcode, OrgAddressSchema.OA_PostCode, postCode, comparisonOperator, Array.Empty<DocAddressType>(), Factory));
			}

			return GetDocAddressQuery(docAddressQuery, new SchemaColumn[] { JobContainerLegsSchema.JU_E2PickupAddressID, JobContainerLegsSchema.JU_E2WaitPointAddressID });
		}

		ZQuery GetDeliveryPostCodeQuery(SQLComparisonOperator comparisonOperator, ZString postCode)
		{
			ZQuery docAddressQuery = new ZQuery();
			if (!postCode.IsEmpty)
			{
				docAddressQuery.AddToFilter(CartageQueryHelper.GetDocAddressOnlyQuery(JobDocAddressSchema.E2_Postcode, OrgAddressSchema.OA_PostCode, postCode, comparisonOperator, Array.Empty<DocAddressType>(), Factory));
			}

			return GetDocAddressQuery(docAddressQuery, new SchemaColumn[] { JobContainerLegsSchema.JU_E2WaitPointAddressID, JobContainerLegsSchema.JU_E2DeliveryAddressID });
		}

		ZQuery GetPickupAddressTypeQuery(ZString orgType)
		{
			return GetDocAddressTypeQuery(orgType, JobContainerLegsSchema.JU_E2PickupAddressID, JobContainerLegsSchema.JU_E2WaitPointAddressID);
		}

		ZQuery GetDeliveryAddressTypeQuery(ZString orgType)
		{
			return GetDocAddressTypeQuery(orgType, JobContainerLegsSchema.JU_E2WaitPointAddressID, JobContainerLegsSchema.JU_E2DeliveryAddressID);
		}

		ZQuery GetDocAddressTypeQuery(ZString orgType, params SchemaColumn[] columns)
		{
			var query = new ZQuery();

			if (LocalCartageJobOrgTypeList.Instance.ContainsCode(orgType))
			{
				DocAddressType docAddressType = CommonCartageAddressHelper.GetCartageDocAddressTypeFromOrgType(orgType);

				query = GetDocAddressQuery(CartageQueryHelper.GetDocAddressOnlyQuery(null, null, ZString.Empty, SQLComparisonOperator.Equal, new DocAddressType[] { docAddressType }, Factory), columns);
			}

			return query;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();

			SchemaColumn columnOverride = null;
			var helper = new CartageWorkflowFilterStripsHelper(typeof(CommonCartageLeg), WorkflowDescriptors.CartageLegWorkflowDescriptorCode, Factory, columnOverride);
			helper.SetShouldAddWorkflowCustomFieldsFilters(true);

			helpers.Add(helper);

			return helpers;
		}

		ModuleFilter SetupFilter(ModuleFilter filter, MultilingualString multilingualDescription, FilterCategory category, ModuleFilterSubGroup subGroup = null, int maxLength = 0)
		{
			filter.MultilingualDescription = multilingualDescription;
			filter.Category = category;
			if (subGroup != null)
			{
				filter.SubGroup = subGroup;
			}
			if (maxLength != 0)
			{
				filter.MaxLength = maxLength;
			}
			return filter;
		}

		ZDBOnlyQuery GetContainerQuery(ZQuery addToContainerFilter)
		{
			return GetContainerQuery(addToContainerFilter, false);
		}

		ZDBOnlyQuery GetContainerQuery(ZQuery addToContainerFilter, bool notInContainerFilter)
		{
			ZDBOnlySubQuery containerSubQuery = new ZDBOnlySubQuery(typeof(CommonContainer), JobBookedCtgMoveSchema.EW_JC_Container);
			containerSubQuery.AddToFilter(addToContainerFilter);

			ZDBOnlySubQuery bookedSubQuery = new ZDBOnlySubQuery(typeof(CommonBookedCtgMove), JobContainerLegsSchema.JU_EW, notInContainerFilter);
			bookedSubQuery.AddSubQuery(containerSubQuery, JoinCondition.And);

			ZDBOnlyQuery cartageLegQuery = new ZDBOnlyQuery(typeof(CommonCartageLeg));
			cartageLegQuery.AddSubQuery(bookedSubQuery, JoinCondition.And);

			return cartageLegQuery;
		}

		ZDBOnlyQuery GetWorkSheetQuery(ZQuery addToWorkSheetFilter)
		{
			ZDBOnlySubQuery workSheetSubQuery = new ZDBOnlySubQuery(typeof(CommonWorkSheet), JobContainerLegsSchema.JU_EY_RunSheet);
			workSheetSubQuery.AddToFilter(addToWorkSheetFilter);

			ZDBOnlyQuery cartageLegQuery = new ZDBOnlyQuery(typeof(CommonCartageLeg));
			cartageLegQuery.AddSubQuery(workSheetSubQuery, JoinCondition.And);

			return cartageLegQuery;
		}

		ZDBOnlyQuery GetCartageQuery(ZQuery addToCartageFilter)
		{
			ZDBOnlySubQuery cartageSubQuery = new ZDBOnlySubQuery(typeof(CommonCartage), JobBookedCtgMoveSchema.EW_JJ);
			cartageSubQuery.AddToFilter(addToCartageFilter);

			ZDBOnlySubQuery bookedSubQuery = new ZDBOnlySubQuery(typeof(CommonBookedCtgMove), JobContainerLegsSchema.JU_EW);
			bookedSubQuery.IgnoreActiveFilter = true; // Adding a Cartage SubQuery will add the Cartage Active Filter
			bookedSubQuery.AddSubQuery(cartageSubQuery, JoinCondition.And);

			ZDBOnlyQuery cartageLegQuery = new ZDBOnlyQuery(typeof(CommonCartageLeg));
			cartageLegQuery.AddSubQuery(bookedSubQuery, JoinCondition.And);

			return cartageLegQuery;
		}

		ZDBOnlyQuery GetBookedMoveQuery(ZQuery addToBookedMoveFilter)
		{
			var bookedSubQuery = new ZDBOnlySubQuery(typeof(CommonBookedCtgMove), JobContainerLegsSchema.JU_EW);
			bookedSubQuery.AddToFilter(addToBookedMoveFilter);

			var cartageLegQuery = new ZDBOnlyQuery(typeof(CommonCartageLeg));
			cartageLegQuery.AddSubQuery(bookedSubQuery, JoinCondition.And);

			return cartageLegQuery;
		}

		public static ZDBOnlyQuery GetDocAddressQuery(ZQuery docAddressOnlyQuery, SchemaColumn[] legSchemaColumns)
		{
			ZDBOnlyQuery cartageLegQuery = new ZDBOnlyQuery(typeof(CommonCartageLeg));
			foreach (SchemaColumn schemaColumn in legSchemaColumns)
			{
				ZDBOnlySubQuery docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), schemaColumn);
				docAddressSubQuery.AddToFilter(docAddressOnlyQuery);
				cartageLegQuery.AddSubQuery(docAddressSubQuery, JoinCondition.Or);
			}
			return cartageLegQuery;
		}

		CartageBindToLists BindingLists
		{
			get { return CartageBindToLists.GetCachedLists(Factory); }
		}

		public CodeDescriptionPairList EmptyStatusList
		{
			get
			{
				if (emptyStatusList == null)
				{
					emptyStatusList = new CodeDescriptionPairList();
					emptyStatusList.AddPair(CartageLegFilterConstants.All, Res.GetString("95929d20-6d19-406c-9692-6da75e887418", "Show both Empty and Non-Empty Legs"));
					emptyStatusList.AddPair(CartageLegFilterConstants.Empty, Res.GetString("7316f524-4129-4943-b563-d32b9d6fc012", "Show only Legs marked as Empty"));
					emptyStatusList.AddPair(CartageLegFilterConstants.NonEmpty, Res.GetString("674452d8-a015-4ecd-b6ae-59b7880dd022", "Show only Legs not marked as Empty"));
				}
				return emptyStatusList;
			}
		}
		CodeDescriptionPairList emptyStatusList;

		public CodeDescriptionPairList HasDangerousGoodsList
		{
			get
			{
				if (hasDangerousGoodsList == null)
				{
					hasDangerousGoodsList = new CodeDescriptionPairList();
					hasDangerousGoodsList.AddPair(CartageLegFilterConstants.DangerousGoodsBoth, Res.GetString("78bf80bc-712c-4c56-9a3a-4be88da084d4", "Show all legs with and without dangerous goods"));
					hasDangerousGoodsList.AddPair(CartageLegFilterConstants.DangerousGoodsHas, Res.GetString("1f13b1e3-31a2-419a-8332-f2a805a83a01", "Show only legs with dangerous goods"));
					hasDangerousGoodsList.AddPair(CartageLegFilterConstants.DangerousGoodsNone, Res.GetString("9bdb5414-5f32-4ed7-9df2-1e7561a93a3b", "Show only legs without dangerous goods"));
				}
				return hasDangerousGoodsList;
			}
		}
		CodeDescriptionPairList hasDangerousGoodsList;

		public CodeDescriptionPairList ContainerServiceList
		{
			get
			{
				if (containerServiceList == null)
				{
					containerServiceList = new CodeDescriptionPairList();

					containerServiceList.AddPair(CartageLegFilterConstants.ContainerServiceAny, Res.GetString("b3080ad1-34fa-41a4-a4c6-3f7b457b8459", "Any Service"));
					containerServiceList.AddPair(CartageLegFilterConstants.ContainerServiceNone, Res.GetString("6188fdaa-50b8-45fc-93eb-185f632eb4a0", "No Service"));

					containerServiceList.AddRange(new FreightServiceTypes());
				}
				return containerServiceList;
			}
		}
		CodeDescriptionPairList containerServiceList;

		public CodeDescriptionPairList WorkSheetStatusList
		{
			get
			{
				if (workSheetStatusList == null)
				{
					workSheetStatusList = new CodeDescriptionPairList();
					workSheetStatusList.AddPair(CartageLegFilterConstants.All, Res.GetString("f4743cf2-4ea0-4582-b343-4d080d798c91", "Show both Allocated and Unallocated Legs"));
					workSheetStatusList.AddPair(CartageLegFilterConstants.Allocated, Res.GetString("4ec5e4c6-c114-479e-bfda-d3fd86a7270c", "Show only Legs allocated to a Run Sheet"));
					workSheetStatusList.AddPair(CartageLegFilterConstants.Unallocated, Res.GetString("ee726e3f-039e-49ff-8f42-bbdabf9e3094", "Show only Legs that are not allocated to a Run Sheet"));
				}
				return workSheetStatusList;
			}
		}
		CodeDescriptionPairList workSheetStatusList;

		public CodeDescriptionPairList SlotStatusList
		{
			get
			{
				if (slotStatusList == null)
				{
					slotStatusList = new CodeDescriptionPairList();
					slotStatusList.AddPair(CartageLegFilterConstants.All, Res.GetString("2095d386-21f3-43fc-9462-000d913ab8a2", "Show both Booked and Non-Booked Legs"));
					slotStatusList.AddPair(CartageLegFilterConstants.Booked, Res.GetString("d530451a-2933-4802-8522-c231ad12f83d", "Show only Legs that have a Slot Booked"));
					slotStatusList.AddPair(CartageLegFilterConstants.NonBooked, Res.GetString("9165cd3a-bbda-469e-8a2b-1c975f01f0af", "Show only Legs that don't have a Slot Booked"));
				}
				return slotStatusList;
			}
		}
		CodeDescriptionPairList slotStatusList;

		public CodeDescriptionPairList CompleteStatusList
		{
			get
			{
				if (completeStatusList == null)
				{
					completeStatusList = new CodeDescriptionPairList();
					completeStatusList.AddPair(CartageLegFilterConstants.All, Res.GetString("6798249e-e466-4c99-9a78-b8562f4ce0ba", "Show both Completed and Non-Completed Legs"));
					completeStatusList.AddPair(CartageLegFilterConstants.Complete, Res.GetString("d65f745b-b076-42b8-a0ea-5fe5242231be", "Show only Legs that are Completed"));
					completeStatusList.AddPair(CartageLegFilterConstants.Incomplete, Res.GetString("ecee757d-e431-49fb-92d4-ec394afcce6f", "Show only Legs that are not Completed"));
				}
				return completeStatusList;
			}
		}
		CodeDescriptionPairList completeStatusList;

		public CodeDescriptionPairList LegTypeList
		{
			get
			{
				if (legTypeList == null)
				{
					legTypeList = new CodeDescriptionPairList();
					legTypeList.AddPair(CartageLegFilterConstants.All, Res.GetString("066f32a0-7671-4144-9129-8c672f9c1647", "Show both Loose and Containerized Legs"));
					legTypeList.AddPair(Constants.ContainerModes.Containerised, Res.GetString("9cb02f98-8abe-4244-b798-45c49b6c32ad", "Show only Containerized Legs"));
					legTypeList.AddPair(Constants.ContainerModes.Loose, Res.GetString("4f09f4df-df7d-4e1e-9bab-ec65735e2b96", "Show only Loose Legs"));
				}
				return legTypeList;
			}
		}
		CodeDescriptionPairList legTypeList;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter constant.")]
		public static class CartageLegFilterConstants
		{
			public const string All = "All";
			public const string Complete = "Complete";
			public const string Incomplete = "Incomplete";
			public const string Booked = "Booked";
			public const string NonBooked = "Non-Booked";
			public const string Allocated = "Allocated";
			public const string Unallocated = "Unallocated";
			public const string Empty = "Empty";
			public const string NonEmpty = "Non-Empty";
			public const string DangerousGoodsBoth = "Both";
			public const string DangerousGoodsHas = "Has Dangerous Goods";
			public const string DangerousGoodsNone = "No Dangerous Goods";
			public const string ContainerServiceAny = "Any";
			public const string ContainerServiceNone = "None";
			public const string LocalTransportRegistrationDate = "Port Transport Registration Date";
			public const string LocalTransportPlannedPickup = "Port Transport Planned Pickup";
			public const string LocalTransportPlannedDelivery = "Port Transport Planned Delivery";
			public const string PlannedPickup = "Planned Pickup";
			public const string ActualPickupTimeIn = "Actual Pickup Time In";
			public const string ActualPickupTimeOut = "Actual Pickup Time Out";
			public const string PlannedDelivery = "Planned Delivery";
			public const string ActualDeliveryTimeIn = "Actual Delivery Time In";
			public const string ActualDeliveryTimeOut = "Actual Delivery Time Out";
			public const string EmptyReturnedOn = "Container Empty Returned On";
			public const string EmptyRequired = "Container Empty Required";
			public const string EmptyReturnBy = "Container Empty Return By";
			public const string PickupRequested = "Pickup Requested";
			public const string PickupRequestedTo = "Pickup Requested To";
			public const string DeliveryRequested = "Delivery Requested";
			public const string DeliveryRequestedTo = "Delivery Requested To";
			public const string SlotDate = "Slot Date";
			public const string ScheduleETA = "Schedule ETA";
			public const string ScheduleATA = "Schedule ATA";
			public const string ScheduleETD = "Schedule ETD";
			public const string ScheduleATD = "Schedule ATD";
			public const string ScheduleLCLAvailability = "Schedule LCL Availability";
			public const string ScheduleLCLCutOff = "Schedule LCL Cut Off";
			public const string ScheduleLCLReceivalCommences = "Schedule LCL Receival Commences";
			public const string ScheduleLCLStorage = "Schedule LCL Storage";
			public const string ScheduleFCLAvailability = "Schedule FCL Availability";
			public const string ScheduleFCLCutOff = "Schedule FCL Cut Off";
			public const string ScheduleFCLReceivalCommences = "Schedule FCL Receival Commences";
			public const string ScheduleFCLStorage = "Schedule FCL Storage";
		}

		ModuleFilterSubGroup CartageSubGroup
		{
			get { return cartageSubGroup ?? (cartageSubGroup = new CartageFilterSubGroup(this)); }
		}
		ModuleFilterSubGroup cartageSubGroup;

		class CartageFilterSubGroup : ModuleFilterSubGroup
		{
			public CartageFilterSubGroup(CartageLegFilterStripBusinessObject filterBO)
			{
				this.filterBO = filterBO;
			}
			readonly CartageLegFilterStripBusinessObject filterBO;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return filterBO.GetCartageQuery(filter);
			}
		}

		ModuleFilterSubGroup BookedMoveSubGroup
		{
			get { return bookedMoveSubGroup ?? (bookedMoveSubGroup = new BookedMoveFilterSubGroup(this)); }
		}

		ModuleFilterSubGroup bookedMoveSubGroup;

		class BookedMoveFilterSubGroup : ModuleFilterSubGroup
		{
			public BookedMoveFilterSubGroup(CartageLegFilterStripBusinessObject filterBO)
			{
				this.filterBO = filterBO;
			}
			readonly CartageLegFilterStripBusinessObject filterBO;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return filterBO.GetBookedMoveQuery(filter);
			}
		}

		ModuleFilterSubGroup ContainerSubGroup
		{
			get { return containerSubGroup ?? (containerSubGroup = new ContainerFilterSubGroup(this)); }
		}
		ModuleFilterSubGroup containerSubGroup;

		class ContainerFilterSubGroup : ModuleFilterSubGroup
		{
			public ContainerFilterSubGroup(CartageLegFilterStripBusinessObject filterBO)
			{
				this.filterBO = filterBO;
			}
			readonly CartageLegFilterStripBusinessObject filterBO;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return filterBO.GetContainerQuery(filter);
			}
		}

		ModuleFilterSubGroup RunSheetSubGroup
		{
			get { return runSheetSubGroup ?? (runSheetSubGroup = new RunSheetFilterSubGroup(this)); }
		}
		ModuleFilterSubGroup runSheetSubGroup;

		class RunSheetFilterSubGroup : ModuleFilterSubGroup
		{
			public RunSheetFilterSubGroup(CartageLegFilterStripBusinessObject filterBO)
			{
				this.filterBO = filterBO;
			}
			readonly CartageLegFilterStripBusinessObject filterBO;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return filterBO.GetWorkSheetQuery(filter);
			}
		}

		ModuleFilterSubGroup ScheduleSubGroup
		{
			get { return scheduleSubGroup ?? (scheduleSubGroup = new ScheduleFilterSubGroup()); }
		}

		ModuleFilterSubGroup scheduleSubGroup;

		class ScheduleFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var scheduleFilter = new ZDBOnlySubQuery(typeof(AutoViewLocalTransportSchedule), ViewLocalTransportScheduleSchema.PK);
				scheduleFilter.AddToFilter(filter);

				var cartageSubQuery = new ZDBOnlySubQuery(typeof(CommonCartage), JobBookedCtgMoveSchema.EW_JJ);
				cartageSubQuery.AddSubQuery(JobCartageSchema.PK, scheduleFilter, JoinCondition.And);

				var bookedSubQuery = new ZDBOnlySubQuery(typeof(CommonBookedCtgMove), JobContainerLegsSchema.JU_EW);
				bookedSubQuery.IgnoreActiveFilter = true; // Adding a Cartage SubQuery will add the Cartage Active Filter
				bookedSubQuery.AddSubQuery(cartageSubQuery, JoinCondition.And);

				var cartageLegQuery = new ZDBOnlyQuery(typeof(CommonCartageLeg));
				cartageLegQuery.AddSubQuery(bookedSubQuery, JoinCondition.And);

				return cartageLegQuery;
			}
		}
	}
}
