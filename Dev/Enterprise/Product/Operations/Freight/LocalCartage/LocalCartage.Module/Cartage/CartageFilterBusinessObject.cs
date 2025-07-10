using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Module
{
	public class CartageFilterBusinessObject : FilterStripBusinessObject, IAccountingFilterStripHolder
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter constant.")]
		public static class FilterNameConstants
		{
			public const string VoyageVessel = "Vessel and Flight/Voyage #";
			public const string ParentJobType = "Parent Job Type";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();

			AddNumberFilters(result);
			AddDateFilters(result);
			AddOrganisationFilters(result);
			AddLocationFilters(result);
			AddModesAndTypesFilters(result);
			AddStatusAndFlagsFilters(result);

			AccountingFilterStrip.AddBillingFilters(result);
			AccountingFilterStrip.AddJobManagementFilters(result, Env.Security.TransportJobInvoicing);
			securityProvider.AddCRMSecurityFilterStrips(Factory, result);

			return result;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var helper = new WorkflowFilterStripsHelperWithRoutingSupport(typeof(CommonCartage), JobInvoicingConsumerTypes.LocalCartage.Code, Factory);
			helper.SetShouldAddWorkflowCustomFieldsFilters(true);
			helpers.Add(helper);

			return helpers;
		}

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			var consignmentIDFilter = new ModuleNumberFilter(NumberFilterTypes.JobNumber, GetConsignmentIDQuery)
			{
				MaxLength = JobCartageSchema.JJ_ConsignmentID.MaxLength,
				MultilingualDescription = ResString.GetMultilingualString("34A0D9D2-77B1-4C88-8D0A-5D328AB74D8A", "Job #"),
				IsCommon = true,
				UseMultiSearch = false //TODO: Needs to work with multiple values. (CartageQueryHelper.cs GetConsignmentIDQuery)
			};
			return consignmentIDFilter;
		}

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(NumberFilterTypes.OrderNumber, JobCartageSchema.JJ_OrderReferenceNumber).MultilingualDescription = ResString.GetMultilingualString("f44b2d0c-6bcc-4243-979a-24590ec00196", "Ref #");
			filters.AddNumberFilter(NumberFilterTypes.QuoteNumber, JobCartageSchema.JJ_QuoteNumber).MultilingualDescription = ResString.GetMultilingualString("6223093f-7b63-4290-a9d2-94b4edc53d70", "Quote #");
			filters.AddNumberFilterWithoutIsBlankAndIsNotBlank(NumberFilterTypes.ParentJobNumber, CartageQueryHelper.GetCartageParentJobNumberQuery).MultilingualDescription = ResString.GetMultilingualString("de425c56-91c3-4472-abdc-ab3379a64cfb", "Parent Job #");

			var wayBillNumberFilter = new ModuleNumberFilter(NumberFilterTypes.WaybillNumber, GetWayBillNumberQuery)
			{
				MultilingualDescription = ResString.GetMultilingualString("8d91137d-ec9c-48ac-b655-a84901e3cc5c", "Waybill #"),
				MaxLength = JobCartageSchema.JJ_WaybillNumber.MaxLength,
				IsCommon = true
			};
			filters.AddFilter(wayBillNumberFilter);

			var containerNumberFilter = new ModuleNumberFilter(NumberFilterTypes.ContainerNumber, GetContainerNumberQuery)
			{
				MultilingualDescription = ResString.GetMultilingualString("7f923c77-83c9-4f3c-8d1e-0c86337aa186", "Container #"),
				MaxLength = JobContainerSchema.JC_ContainerNum.MaxLength,
				IsCommon = true
			};
			filters.AddFilter(containerNumberFilter);

			var voyageVesselFilter = filters.AddTextAndNkFilter(FilterNameConstants.VoyageVessel, GetFlightVoyageNumberAndVesselQuery, ModuleIDs.RefVessel, BindingLists.RefVessels);
			voyageVesselFilter.MultilingualDescription = ResString.GetMultilingualString("5A94D483-60ED-4608-BCA3-C10F20A1B206", "Vessel and Flight/Voyage #");
			voyageVesselFilter.Category = FilterCategories.NumbersAndReferences;
			voyageVesselFilter.SubGroup = ScheduleSubGroup;
			voyageVesselFilter.NkMaxLength = ViewLocalTransportScheduleSchema.VLT_RV_NKVessel.MaxLength;
			voyageVesselFilter.MaxLength = ViewLocalTransportScheduleSchema.VLT_Voyage.MaxLength;

			filters.AddCustomFilter(new ReferenceNumberFilter(
				NumberFilterTypes.PortTransportAdditionalReference,
				new ReferenceNumberFilterHelper<CommonCartage>().GetReferenceNumberFilter,
				new RefCountryCollection(Factory),
				GetAdditionalReferenceNumberTypes())
			{
				MultilingualDescription = ResString.GetMultilingualString("bc65044e-d629-46e6-9421-02bc01651b70", "Additional Reference #"),
				MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength
			});
		}

		ZQuery GetWayBillNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CommonCartage));
			result.AddToFilter(new ZQuery().AddToFilter_PossiblyCommaSeparated(JobCartageSchema.JJ_WaybillNumber, @operator, value));

			return result;
		}

		ZQuery GetFlightVoyageNumberAndVesselQuery(SQLComparisonOperator @operator, ZString voyage, ZString vessel)
		{
			return VoyageVesselModuleFilterHelper.GetBasicVoyageVesselQuery(@operator, voyage, vessel, false, ViewLocalTransportScheduleSchema.VLT_Voyage, ViewLocalTransportScheduleSchema.VLT_RV_NKVessel);
		}

		ZQuery GetConsignmentIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var maxLength = JobCartageSchema.JJ_ConsignmentID.MaxLength;
			return CartageQueryHelper.GetConsignmentIDQuery(comparisonOperator, value.Left(maxLength));
		}

		ZQuery GetContainerNumberQuery(SQLComparisonOperator @operator, ZString containerNo)
		{
			return GetContainerQuery(JobContainerSchema.JC_ContainerNum, @operator, containerNo);
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(DateFilterTypes.CPD, JobCartageSchema.JJ_A_JCL).MultilingualDescription = ResString.GetMultilingualString("c9c37680-7460-41dd-b957-16e5929951f9", "Completion");
			filters.AddDateFilter(DateFilterTypes.EPD, GetPlannedPickupQuery).MultilingualDescription = ResString.GetMultilingualString("5a288a6a-fb56-4604-a601-b6c26044de00", "Est./Planned Pickup");
			filters.AddDateFilter(DateFilterTypes.EDD, GetEstimatedDeliveryQuery).MultilingualDescription = ResString.GetMultilingualString("e89270b3-531f-43b4-ad48-f3cb0e905c8d", "Estimated Delivery");
			filters.AddDateFilter(DateFilterTypes.APD, GetActualPickupQuery).MultilingualDescription = ResString.GetMultilingualString("9695f860-0a49-4315-bb04-cf6ff3009159", "Actual Pickup");
			filters.AddDateFilter(DateFilterTypes.ADD, GetActualDeliveryQuery).MultilingualDescription = ResString.GetMultilingualString("52601b76-f2dc-467a-8607-7fc05e3b1aa5", "Actual Delivery");

			AddScheduleDateFilter(filters, DateFilterTypes.ScheduleETA, ViewLocalTransportScheduleSchema.VLT_ETA, ResString.GetMultilingualString("7f7c12f3-69a7-4e65-9019-c60364de40d6", "Schedule ETA"));
			AddScheduleDateFilter(filters, DateFilterTypes.ScheduleATA, ViewLocalTransportScheduleSchema.VLT_ATA, ResString.GetMultilingualString("dbf1dc0b-686d-4d59-b032-b4991578125a", "Schedule ATA"));
			AddScheduleDateFilter(filters, DateFilterTypes.ScheduleETD, ViewLocalTransportScheduleSchema.VLT_ETD, ResString.GetMultilingualString("c17f9aef-97d7-4348-a49a-0162bcb0c5bb", "Schedule ETD"));
			AddScheduleDateFilter(filters, DateFilterTypes.ScheduleATD, ViewLocalTransportScheduleSchema.VLT_ATD, ResString.GetMultilingualString("4846d5f2-6904-4788-ac0f-0f8b61fef0e2", "Schedule ATD"));

			AddScheduleDateFilter(filters, DateFilterTypes.ScheduleLCLAvailability, ViewLocalTransportScheduleSchema.VLT_LCLAvailabilityDate, ResString.GetMultilingualString("ce73d5cf-c12f-4771-801a-4a8a70326073", "Schedule CFS Available"));
			AddScheduleDateFilter(filters, DateFilterTypes.ScheduleLCLCutOff, ViewLocalTransportScheduleSchema.VLT_LCLCutOff, ResString.GetMultilingualString("b082749e-be31-41f2-b30f-4092d7869b64", "Schedule CFS Cut Off"));
			AddScheduleDateFilter(filters, DateFilterTypes.ScheduleLCLReceivalCommences, ViewLocalTransportScheduleSchema.VLT_LCLReceivalCommences, ResString.GetMultilingualString("1d3c3227-b89a-4faf-ace1-70083853b2d7", "Schedule CFS Receival Start"));
			AddScheduleDateFilter(filters, DateFilterTypes.ScheduleLCLStorage, ViewLocalTransportScheduleSchema.VLT_LCLStorageDate, ResString.GetMultilingualString("9b076531-534f-4b27-8253-17b6b6e6fe44", "Schedule CFS Storage Start"));

			AddScheduleDateFilter(filters, DateFilterTypes.ScheduleFCLAvailability, ViewLocalTransportScheduleSchema.VLT_FCLAvailabilityDate, ResString.GetMultilingualString("7eafd047-da80-4dfa-b99f-a76dd2390aba", "Schedule CTO Available"));
			AddScheduleDateFilter(filters, DateFilterTypes.ScheduleFCLCutOff, ViewLocalTransportScheduleSchema.VLT_FCLCutOff, ResString.GetMultilingualString("f19bd6ab-5ca9-4960-97b9-b6d3ad45bd40", "Schedule CTO Cut Off"));
			AddScheduleDateFilter(filters, DateFilterTypes.ScheduleFCLReceivalCommences, ViewLocalTransportScheduleSchema.VLT_FCLReceivalCommences, ResString.GetMultilingualString("0a809270-5be8-4700-af73-b1cba1661e9c", "Schedule CTO Receival Start"));
			AddScheduleDateFilter(filters, DateFilterTypes.ScheduleFCLStorage, ViewLocalTransportScheduleSchema.VLT_FCLStorageDate, ResString.GetMultilingualString("00494959-9654-4eb2-8b40-0b358bcc271e", "Schedule CTO Storage"));
		}

		void AddScheduleDateFilter(ModuleFilterCollection filters, string name, SchemaDateTimeColumn column, MultilingualString caption)
		{
			var scheduleFilter = filters.AddDateFilter(name, column);
			scheduleFilter.MultilingualDescription = caption;
			scheduleFilter.SubGroup = ScheduleSubGroup;
		}

		ZQuery GetPlannedPickupQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var cartageFilter = new ZQuery();
			AddDateRange(cartageFilter, comparisonOperator, JoinCondition.And, JobCartageSchema.JJ_EstimatedPickup, fromDate.Date, toDate.Date);

			return cartageFilter;
		}

		ZQuery GetEstimatedDeliveryQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var cartageFilter = new ZQuery();
			AddDateRange(cartageFilter, comparisonOperator, JoinCondition.And, JobCartageSchema.JJ_EstimatedDelivery, fromDate.Date, toDate.Date);

			return cartageFilter;
		}

		ZQuery GetActualPickupQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var legFilter = new ZQuery();
			AddDateRange(legFilter, comparisonOperator, JoinCondition.Or, JobContainerLegsSchema.JU_PickupTimeIn, fromDate.Date, toDate.Date);
			AddDateRange(legFilter, comparisonOperator, JoinCondition.Or, JobContainerLegsSchema.JU_PickupTimeOut, fromDate.Date, toDate.Date);

			return GetContainerLegQuery(legFilter);
		}

		ZQuery GetActualDeliveryQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var legFilter = new ZQuery();
			AddDateRange(legFilter, comparisonOperator, JoinCondition.Or, JobContainerLegsSchema.JU_DeliverTimeIn, fromDate.Date, toDate.Date);
			AddDateRange(legFilter, comparisonOperator, JoinCondition.Or, JobContainerLegsSchema.JU_DeliverTimeOut, fromDate.Date, toDate.Date);

			return GetContainerLegQuery(legFilter);
		}

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter(OrganisationFilterTypes.Client, ModuleIDs.Organisation, CartageQueryHelper.GetLocalClientQuery, BindingLists.Organisations).MultilingualDescription = ResString.GetMultilingualString("b3a9a125-87af-4bb6-a260-79334e28c61f", "Client");

			var pickupDeliveryFilter = filters.AddGuidFilter(OrganisationFilterTypes.PickupDelivery, ModuleIDs.Organisation, GetPickupDeliveryOrganisationQuery, BindingLists.Organisations, BindingLists.Organisations);
			pickupDeliveryFilter.MultilingualDescription = ResString.GetMultilingualString("a8e1d27e-83a5-41cc-a190-2e65ef3e1a91", "Pickup From/Deliver To");
			pickupDeliveryFilter.SetItemDescriptions(Res.GetData("a1e748d3-80f5-4383-8595-10727b018758", "Pickup"), Res.GetData("4b813393-abd3-493a-9839-edbd6cf7ef55", "Delivery"));

			var driverFilter = filters.AddNkFilter("Driver", GetDriverQuery, ModuleIDs.GlbStaff, BindingLists.StaffDrivers);
			driverFilter.MultilingualDescription = ResString.GetMultilingualString("8f509486-5a0b-471f-8019-ca39d6c159b6", "Driver");
			driverFilter.IsPublishedOnWeb = false;
			driverFilter.Category = FilterCategories.Organisations;

			var branchFilter = filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, JobCartageSchema.JJ_GB, Branches);
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("e47b13a3-d99c-47d7-adbb-e1fe7a96a1dd", "Branch");
			branchFilter.IsPublishedOnWeb = false;

			var vehicleFilter = filters.AddGuidFilter("Vehicle", ModuleIDs.RefEquipment, GetVehicleQuery, BindingLists.Vehicles);
			vehicleFilter.MultilingualDescription = ResString.GetMultilingualString("7565c57a-49ea-4722-bf09-73f5c9538c29", "Vehicle");
			vehicleFilter.IsPublishedOnWeb = false;

			var extraEQFilter = filters.AddGuidFilter("Extra Equipment", ModuleIDs.RefEquipment, GetExtraEquip, BindingLists.Equipments);
			extraEQFilter.MultilingualDescription = ResString.GetMultilingualString("99ecd6de-c5d9-498f-84f2-0883447fc1c3", "Extra Equipment");
			extraEQFilter.IsPublishedOnWeb = false;
		}

		ZQuery GetPickupDeliveryOrganisationQuery(ZGuid organisation1PK, ZGuid organisation2PK)
		{
			ZQuery picLegQuery = new ZQuery();
			ZQuery dlvLegQuery = new ZQuery();

			if (organisation1PK.IsValid)
			{
				ZQuery picAddressQuery = new ZQuery();
				picAddressQuery.AddToFilter(CartageQueryHelper.GetDocAddressOnlyQuery(organisation1PK, Array.Empty<DocAddressType>(), Factory));
				picLegQuery = CartageLegFilterStripBusinessObject.GetDocAddressQuery(picAddressQuery, new SchemaColumn[] { JobContainerLegsSchema.JU_E2PickupAddressID, JobContainerLegsSchema.JU_E2WaitPointAddressID });
			}

			if (organisation2PK.IsValid)
			{
				ZQuery dlvAddressQuery = new ZQuery();
				dlvAddressQuery.AddToFilter(CartageQueryHelper.GetDocAddressOnlyQuery(organisation2PK, Array.Empty<DocAddressType>(), Factory));
				dlvLegQuery = CartageLegFilterStripBusinessObject.GetDocAddressQuery(dlvAddressQuery, new SchemaColumn[] { JobContainerLegsSchema.JU_E2WaitPointAddressID, JobContainerLegsSchema.JU_E2DeliveryAddressID });
			}

			ZQuery legQuery = new ZQuery();
			legQuery.AddToFilter(picLegQuery);
			legQuery.AddToFilter(dlvLegQuery);
			return GetContainerLegQuery(legQuery);
		}

		ZQuery GetDriverQuery(ZString driverPK)
		{
			return GetWorkSheetQuery(JobCartageRunSheetSchema.EY_GS_NKTruckDriver, SQLComparisonOperator.Equal, driverPK);
		}

		ZQuery GetVehicleQuery(ZGuid vehiclePK)
		{
			return GetWorkSheetQuery(JobCartageRunSheetSchema.EY_RQ_Truck, SQLComparisonOperator.Equal, vehiclePK);
		}

		ZQuery GetExtraEquip(ZGuid equipePK)
		{
			ZQuery containerLegQuery = new ZQuery(JobContainerLegsSchema.JU_RQ_ExtraEquip1, equipePK);
			containerLegQuery.AddToFilter(JoinCondition.Or, JobContainerLegsSchema.JU_RQ_ExtraEquip2, equipePK);
			return GetContainerLegQuery(containerLegQuery);
		}

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			var loadDischargeFilter = filters.AddLocationFilter("Load / Discharge", ViewLocalTransportScheduleSchema.VLT_RL_NKLoadPort, BindingLists.RefLocations, ViewLocalTransportScheduleSchema.VLT_RL_NKDischargePort, BindingLists.RefLocations);
			loadDischargeFilter.MultilingualDescription = ResString.GetMultilingualString("c42bd6fd-59f9-4f31-be40-bccc17419b52", "Load / Discharge");
			loadDischargeFilter.SetItemDescriptions(Res.GetData("LocalTransport|CartageFilter|Load", "Load"), Res.GetData("LocalTransport|CartageFilter|Discharge", "Discharge"));
			loadDischargeFilter.SubGroup = ScheduleSubGroup;

			var relatedPortFilter = filters.AddNkFilter("Related Port", GetRelatedPortQuery, ModuleIDs.RefUNLOCO, new RefUNLOCOCollection(Factory));
			relatedPortFilter.MultilingualDescription = ResString.GetMultilingualString("71c03709-c315-4d4b-9d9a-86bb7198e45f", "Related Port");
			relatedPortFilter.MaxLength = Math.Min(OrgAddressSchema.OA_RL_NKRelatedPortCode.MaxLength, OrgHeaderSchema.OH_RL_NKClosestPort.MaxLength);
			relatedPortFilter.Category = FilterCategories.Locations;
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

			return GetDocAddressQuery(docAddressWithOrgAddressQuery);
		}

		void AddModesAndTypesFilters(ModuleFilterCollection filters)
		{
			ModuleFilter containerModeFilter = filters.AddTextFilter("Drop Mode", GetDropModeQuery, BindingLists.DropModes);
			containerModeFilter.MultilingualDescription = ResString.GetMultilingualString("55121191-b60c-4b9e-9eea-2a41c0cac6ee", "Drop Mode");
			containerModeFilter.Category = FilterCategories.ModesAndTypes;

			ModuleFilter cartageJobTypeFilter = filters.AddTextFilter("Port Transport Job Type", JobCartageSchema.JJ_E3_NKJobType, BindingLists.NewCartageJobTypes);
			cartageJobTypeFilter.MultilingualDescription = ResString.GetMultilingualString("e7235ce1-5536-4a74-bfd4-2793465fc654", "Port Transport Job Type");
			cartageJobTypeFilter.Category = FilterCategories.ModesAndTypes;

			ModuleNkFilter serviceLevelFilter = filters.AddNkFilter("Service Level", JobCartageSchema.JJ_RS_NKServiceLevel, ModuleIDs.ServiceLevel, BindingLists.ServiceLevels);
			serviceLevelFilter.MultilingualDescription = ResString.GetMultilingualString("3dd19a3f-9596-4d9f-816b-4756fda57914", "Service Level");
			serviceLevelFilter.Category = FilterCategories.ModesAndTypes;

			ModuleFilter containerServiceFilter = filters.AddTextFilter("Container Service", GetContainerServiceQuery, ContainerServiceList);
			containerServiceFilter.MultilingualDescription = ResString.GetMultilingualString("2c6e9e0f-2106-4fcb-bc32-4baecb950edd", "Container Service");
			containerServiceFilter.Category = FilterCategories.ModesAndTypes;

			var parentTypeFilter = filters.AddTextFilter(FilterNameConstants.ParentJobType, CartageQueryHelper.GetCartageParentJobTypeQuery, BindingLists.ParentJobTypes);
			parentTypeFilter.Category = FilterCategories.ModesAndTypes;
			parentTypeFilter.MultilingualDescription = ResString.GetMultilingualString("JobCartage|JobCartageFilter|JobType", "Parent Job Type");
			parentTypeFilter.SubGroup = ParentsSubGroup;
			parentTypeFilter.MaxLength = 3;
		}

		ZQuery GetDropModeQuery(ZString dropMode)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CommonCartage));
			result.AddToFilter(GetBookedCtgMoveQuery(JobBookedCtgMoveSchema.EW_DropMode, SQLComparisonOperator.Equal, dropMode), JoinCondition.Or);
			result.AddToFilter(JoinCondition.Or, JobCartageSchema.JJ_DropMode, dropMode);
			return result;
		}

		ZQuery GetContainerServiceQuery(ZString serviceLevel)
		{
			ZDBOnlyQuery jobContainerQuery = new ZDBOnlyQuery(typeof(CommonContainer));
			ZDBOnlySubQuery serviceSubQuery = new ZDBOnlySubQuery(typeof(JobService), JobServiceSchema.ES_ParentID);

			ZQuery serviceQuery = new ZQuery(JobServiceSchema.ES_ParentTableCode, SQLComparisonOperator.Equal, JobContainerSchema.Constants.Prefix);

			if (!(serviceLevel == CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ContainerServiceAny || serviceLevel == CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ContainerServiceNone))
			{
				serviceQuery.AddToFilter(JobServiceSchema.ES_ServiceCode, SQLComparisonOperator.Equal, serviceLevel);
			}

			serviceSubQuery.AddToFilter(serviceQuery);
			jobContainerQuery.AddSubQuery(serviceSubQuery, JoinCondition.And);

			return GetContainerQuery(jobContainerQuery, serviceLevel == CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ContainerServiceNone);
		}

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Dangerous Goods", GetHasDangerousGoodsQuery, HasDangerousGoodsList);
			filter.MultilingualDescription = ResString.GetMultilingualString("81522a19-082b-4eed-a005-da81030c7d50", "Dangerous Goods");
			filter.Category = FilterCategories.StatusAndFlags;

			filter = filters.AddTextFilter("Message Status", GetMessageStatusQuery, BindingLists.MessageStatuses);
			filter.MultilingualDescription = ResString.GetMultilingualString("a2c4beb8-3bb1-441d-9478-6fa42e153547", "Message Status");
			filter.Category = FilterCategories.StatusAndFlags;
		}

		ZQuery GetMessageStatusQuery(ZString messageStatus)
		{
			ZQuery legQuery = new ZQuery(JobContainerLegsSchema.JU_MessageStatus, messageStatus);
			if (messageStatus == Constants.CartageLegDispatchStatusList.Codes.NotStarted)
			{
				legQuery.AddToFilter(JoinCondition.Or, JobContainerLegsSchema.JU_MessageStatus, "");
			}

			return GetContainerLegQuery(legQuery);
		}

		ZQuery GetHasDangerousGoodsQuery(ZString hasDangerousGoodsStatus)
		{
			if (hasDangerousGoodsStatus.Trim().EqualsIgnoringCase(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.DangerousGoodsBoth))
			{
				return new ZQuery();
			}

			ZDBOnlyQuery bookedMoveQuery = new ZDBOnlyQuery(typeof(CommonBookedCtgMove));

			//ZDBOnlyQuery goodsSubQuery = new ZDBOnlyQuery(typeof(UNDGDataItem));//, UNDGDataItemSchema.DI_ParentID);
			ZDBOnlySubQuery goodsSubQuery = new ZDBOnlySubQuery(typeof(UNDGDataItem), UNDGDataItemSchema.DI_ParentID);
			//ZQuery serviceQuery = new ZQuery(UNDGDataItemSchema.DI_ParentTableCode, SQLComparisonOperator.Equal, JobBookedCtgMoveSchema.Constants.Prefix);

			//goodsSubQuery.AddToFilter(serviceQuery);

			bookedMoveQuery.AddSubQuery(goodsSubQuery, JoinCondition.And);

			ZDBOnlyQuery result = GetBookedCtgMoveQuery(bookedMoveQuery, !hasDangerousGoodsStatus.Trim().EqualsIgnoringCase(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.DangerousGoodsHas));
			return result;
		}

		CodeDescriptionPairList GetAdditionalReferenceNumberTypes()
		{
			var list = new CodeDescriptionPairList();
			list.AddRange(WarehouseDataRegistry.Instance.AdditionalReferenceType.Value);
			return list;
		}

		public CodeDescriptionPairList HasDangerousGoodsList
		{
			get
			{
				if (hasDangerousGoodsList == null)
				{
					hasDangerousGoodsList = new CodeDescriptionPairList();
					hasDangerousGoodsList.AddPair(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.DangerousGoodsBoth, Res.GetString("138ee383-85d5-4696-96ff-4193f0b546df", "Show all jobs"));
					hasDangerousGoodsList.AddPair(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.DangerousGoodsHas, Res.GetString("2eaaae12-8448-4824-a23d-6cb50b92eefe", "Show only jobs with dangerous goods"));
					hasDangerousGoodsList.AddPair(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.DangerousGoodsNone, Res.GetString("a997e7ab-e64f-4aae-9cf4-54e6c647b34a", "Show only jobs without dangerous goods"));
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

					containerServiceList.AddPair(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ContainerServiceAny, Res.GetString("C1FCCC9D-2A61-4F6A-BCCA-F569368097A1", "Any Service"));
					containerServiceList.AddPair(CartageLegFilterStripBusinessObject.CartageLegFilterConstants.ContainerServiceNone, Res.GetString("C4046A2A-1D01-451C-AC42-5C323787AFF4", "No Service"));

					containerServiceList.AddRange(new FreightServiceTypes());
				}
				return containerServiceList;
			}
		}
		CodeDescriptionPairList containerServiceList;

		public CartageBindToLists BindingLists
		{
			get { return CartageBindToLists.GetCachedLists(Factory); }
		}

		public CodeDescriptionPairList JobStatus_List
		{
			get
			{
				if (fJobStatus_List == null)
				{
					fJobStatus_List = new JobHeaderStatusList();
				}
				return fJobStatus_List;
			}
		}
		JobHeaderStatusList fJobStatus_List;

		public GlbBranchCollection Branches
		{
			get { return new GlbBranchCollection(Factory); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter constant.")]
		public static class DateFilterTypes
		{
			public const string None = "None";
			public const string RST = "Rec. Start";
			public const string COF = "Cut Off";
			public const string AVF = "Available";
			public const string AVT = "Storage";
			public const string CPD = "Completion";
			public const string EPD = "Est./Planned Pickup";
			public const string EDD = "Estimated Delivery";
			public const string APD = "Actual Pickup";
			public const string ADD = "Actual Delivery";
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related.")]
		public static class OrganisationFilterTypes
		{
			public const String None = "None";
			public const String PickupDelivery = "Pickup From/Deliver To";
			public const String Client = "Client";
		}

		protected ZDBOnlyQuery GetContainerQuery(SchemaColumn containerSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			var query = new ZQuery();
			query.AddToFilter_PossiblyCommaSeparated(containerSchemaColumn, comparisonOperator, value);
			return GetContainerQuery(query);
		}

		protected ZDBOnlyQuery GetContainerQuery(ZQuery containerQuery)
		{
			return GetContainerQuery(containerQuery, false);
		}

		protected ZDBOnlyQuery GetContainerQuery(ZQuery containerQuery, bool notInContainerQuery)
		{
			var result = new ZDBOnlyQuery(typeof(CommonCartage));
			var pivotSubQuery = new ZDBOnlySubQuery(typeof(CommonBookedCtgMove), JobBookedCtgMoveSchema.EW_JJ, notInContainerQuery);
			var containerSubQuery = new ZDBOnlySubQuery(typeof(CommonContainer), JobBookedCtgMoveSchema.EW_JC_Container);

			containerSubQuery.AddToFilter(containerQuery);

			pivotSubQuery.AddSubQuery(containerSubQuery, JoinCondition.And);
			result.AddSubQuery(pivotSubQuery, JoinCondition.And);

			return result;
		}

		protected ZDBOnlyQuery GetWorkSheetQuery(SchemaColumn workSheetSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			ZDBOnlyQuery jobCartageQuery = new ZDBOnlyQuery(typeof(CommonCartage));
			ZDBOnlySubQuery moveSubQuery = new ZDBOnlySubQuery(typeof(CommonBookedCtgMove), JobBookedCtgMoveSchema.EW_JJ);
			ZDBOnlySubQuery containerLegSubQuery = new ZDBOnlySubQuery(typeof(CommonCartageLeg), JobContainerLegsSchema.JU_EW);
			ZDBOnlySubQuery workSheetSubQuery = new ZDBOnlySubQuery(typeof(CommonWorkSheet), JobContainerLegsSchema.JU_EY_RunSheet);
			workSheetSubQuery.AddToFilter(workSheetSchemaColumn, comparisonOperator, value);
			containerLegSubQuery.AddSubQuery(workSheetSubQuery, JoinCondition.And);
			moveSubQuery.AddSubQuery(containerLegSubQuery, JoinCondition.And);
			jobCartageQuery.AddSubQuery(moveSubQuery, JoinCondition.And);

			return jobCartageQuery;
		}

		protected ZDBOnlyQuery GetBookedCtgMoveQuery(ZQuery bookedMoveQuery)
		{
			return GetBookedCtgMoveQuery(bookedMoveQuery, false);
		}

		protected ZDBOnlyQuery GetBookedCtgMoveQuery(SchemaColumn bookedMoveSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			return GetBookedCtgMoveQuery(new ZQuery(bookedMoveSchemaColumn, comparisonOperator, value));
		}

		protected ZDBOnlyQuery GetBookedCtgMoveQuery(ZQuery bookedMoveQuery, bool notInBookedMoveQuery)
		{
			ZDBOnlyQuery jobCartageQuery = new ZDBOnlyQuery(typeof(CommonCartage));
			ZDBOnlySubQuery moveSubQuery = new ZDBOnlySubQuery(typeof(CommonBookedCtgMove), JobBookedCtgMoveSchema.EW_JJ, notInBookedMoveQuery);
			moveSubQuery.AddToFilter(bookedMoveQuery);
			jobCartageQuery.AddSubQuery(moveSubQuery, JoinCondition.And);

			return jobCartageQuery;
		}

		protected ZDBOnlyQuery GetContainerLegQuery(ZQuery containerLegQuery)
		{
			ZDBOnlyQuery jobCartageQuery = new ZDBOnlyQuery(typeof(CommonCartage));
			ZDBOnlySubQuery moveSubQuery = new ZDBOnlySubQuery(typeof(CommonBookedCtgMove), JobBookedCtgMoveSchema.EW_JJ);
			ZDBOnlySubQuery containerLegSubQuery = new ZDBOnlySubQuery(typeof(CommonCartageLeg), JobContainerLegsSchema.JU_EW);
			containerLegSubQuery.AddToFilter(containerLegQuery);
			moveSubQuery.AddSubQuery(containerLegSubQuery, JoinCondition.And);
			jobCartageQuery.AddSubQuery(moveSubQuery, JoinCondition.And);

			return jobCartageQuery;
		}

		protected ZDBOnlyQuery GetDocAddressQuery(ZQuery docAddressQuery)
		{
			ZDBOnlyQuery jobCartageQuery = new ZDBOnlyQuery(typeof(CommonCartage));
			ZDBOnlySubQuery docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			docAddressSubQuery.AddToFilter(docAddressQuery);
			jobCartageQuery.AddSubQuery(docAddressSubQuery, JoinCondition.And);
			return jobCartageQuery;
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

				var result = new ZDBOnlyQuery(typeof(CommonCartage));
				result.AddSubQuery(JobCartageSchema.PK, scheduleFilter, JoinCondition.And);

				return result;
			}
		}

		ModuleFilterSubGroup ParentsSubGroup
		{
			get { return parentsSubGroup ?? (parentsSubGroup = new ParentsFilterSubGroup()); }
		}
		ModuleFilterSubGroup parentsSubGroup;

		class ParentsFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return CartageQueryHelper.GetCartageParentSubQuery(filter);
			}
		}

		IAccountingFilterStrip AccountingFilterStrip
		{
			get
			{
				if (AccountingFilterStrip_innerValue == null)
				{
					AccountingFilterStrip_innerValue = (IAccountingFilterStrip)Activator.CreateInstance(ObjectFactory.GetType<IAccountingFilterStrip>(), this);
					AccountingFilterStrip_innerValue.Initialize(addProfitLossReasonFilters: true);
				}

				return AccountingFilterStrip_innerValue;
			}
		}

		IAccountingFilterStrip AccountingFilterStrip_innerValue;

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CommonCartage));
			result.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return result;
		}

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new Dictionary<string, object>()
		{
			{ AccountingFilterStripConfigurationKeys.BusinessObjectType, typeof(CommonCartage) },
			{ AccountingFilterStripConfigurationKeys.InvoicedChargesFilterOptionsSelected, InvoicedChargesFilterOptions.Default | InvoicedChargesFilterOptions.LocalBillingNotPaid },
			{ AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride, ResString.GetMultilingualString("71edd54d-c0bb-43fc-b427-64a4240bef64", "Invoice Status") },
			{ AccountingFilterStripConfigurationKeys.InvoicedChargesFilterNameOverride, ResString.GetMultilingualString("4cd1fcf2-7d15-4eb3-b30d-df68d1d8fc18", "Invoiced / Charges / Billing") }
		};

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride
		{
			get { return null; }
		}

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride
		{
			get { return null; }
		}

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories
		{
			get { return null; }
		}

		readonly CartageCRMSecurityProvider securityProvider = new CartageCRMSecurityProvider();
	}
}
