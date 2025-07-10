using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class ProcessTaskTemplateFilterBusinessObject : FilterStripBusinessObject
	{
		public ProcessTaskTemplateFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			AddTextFilters(result);
			AddDateFilters(result);
			AddRelatedItemFilters(result);
			AddCriteriaFilters(result);
			AddStatusAndFlagsFilters(result);

			return result;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var helper = new WorkflowFilterStripsHelper(typeof(ProcessTaskTemplate), WorkflowDescriptors.WorkItemWorkflowDescriptorCode, Factory);
			helper.SetShouldAddWorkflowCustomFieldsFilters(true);
			helper.ShouldAddMilestoneFilters = false;
			helper.ShouldAddExceptionsInMiscFilters = false;
			helpers.Add(helper);

			return helpers;
		}

		#region Status and Flags

		void AddStatusAndFlagsFilters(ModuleFilterCollection result)
		{
			AddStatusFilter(result, "Is Partial",
				GetPartialTemplateQuery, () => new PartialTypeList(), ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|Partial", "Partial"));

			AddStatusFilter(result, "Is Universal",
				GetUniversalTemplateQuery, () => new UniversalTemplateTypeList(), ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|Universal", "Universal"));
		}

		static void AddStatusFilter(ModuleFilterCollection filters, string description, GetTextQuery queryDelegate, GetList listDelegate, MultilingualString multilingualDescription)
		{
			var filter = filters.AddTextFilter(description, queryDelegate, listDelegate);
			filter.MultilingualDescription = multilingualDescription;
			filter.DefaultProperty = UniversalTemplateTypeList.Codes.All;
			filter.Category = FilterCategories.StatusAndFlags;
		}

		ZQuery GetPartialTemplateQuery(ZString value)
		{
			switch (value)
			{
				case PartialTypeList.Codes.Partial:
					return new ZQuery(ProcessTaskTemplateSchema.P0_IsPartialTemplate, true);

				case PartialTypeList.Codes.NonPartial:
					return new ZQuery(ProcessTaskTemplateSchema.P0_IsPartialTemplate, false);

				default:
					return new ZQuery();
			}
		}

		ZQuery GetUniversalTemplateQuery(ZString value)
		{
			switch (value)
			{
				case UniversalTemplateTypeList.Codes.Universal:
					return new ZQuery(ProcessTaskTemplateSchema.P0_IsUniversal, true);

				case UniversalTemplateTypeList.Codes.NonUniversal:
					return new ZQuery(ProcessTaskTemplateSchema.P0_IsUniversal, false);

				default:
					return new ZQuery();
			}
		}

		#endregion

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Workflow Type", ProcessTaskTemplateSchema.P0_ProcessType, WorkflowTypeList).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|WorkflowType", "Workflow Type");
			filters.AddTextFilter("Name", ProcessTaskTemplateSchema.P0_Name, ComparisonOptions.Default).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|Name", "Name");
			filters.AddTextFilter("Description", ProcessTaskTemplateSchema.P0_Description, ComparisonOptions.Default).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|Description", "Description");
			filters.AddTextFilter("Task Fallback Method", ProcessTaskTemplateSchema.P0_TaskFallbackMethod, FallbackMethodTypeList).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|TaskFallbackMethod", "Task Fallback Method");
			filters.AddTextFilter("Milestone Fallback Method", ProcessTaskTemplateSchema.P0_MilestoneFallbackMethod, FallbackMethodTypeList).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|MilestoneFallbackMethod", "Milestone Fallback Method");
			filters.AddTextFilter("Trigger Fallback Method", ProcessTaskTemplateSchema.P0_TriggerFallbackMethod, FallbackMethodTypeList).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|TriggerFallbackMethod", "Trigger Fallback Method");
		}

		#endregion

		#region Date

		void AddDateFilters(ModuleFilterCollection filters)
		{
			if (WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.Value)
			{
				filters.AddDateFilter("Effective Start", ProcessTaskTemplateSchema.P0_EffectiveStartDateUtc).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|EffectiveStartDate", "Effective Start");
				filters.AddDateFilter("Effective End", ProcessTaskTemplateSchema.P0_EffectiveEndDateUtc).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|EffectiveEndDate", "Effective End");
			}
		}

		#endregion

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			ModuleNkFilter loadPortFilter = filters.AddNkFilter("Load Port", ProcessTaskTemplateSchema.P0_LoadPortCountry, ModuleIDs.Location, Locations);
			loadPortFilter.Category = FilterCategories.Other;
			loadPortFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|LoadPort", "Load Port");
			loadPortFilter.SupportsFiltersMatchComparisonOperator = false; // Can't determine the module to reference

			ModuleNkFilter dischargePortFilter = filters.AddNkFilter("Discharge Port", ProcessTaskTemplateSchema.P0_DischargePortCountry, ModuleIDs.Location, Locations);
			dischargePortFilter.Category = FilterCategories.Other;
			dischargePortFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|DischargePort", "Discharge Port");
			dischargePortFilter.SupportsFiltersMatchComparisonOperator = false; // Can't determine the module to reference

			ModuleGuidFilter clientFilter = filters.AddGuidFilter("Client", ModuleIDs.Organisation, ProcessTaskTemplateSchema.P0_OH_Client, Clients);
			clientFilter.Category = FilterCategories.Other;
			clientFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|Client", "Client");

			ModuleGuidFilter warehouseFilter = filters.AddGuidFilter("Warehouse", ModuleIDs.WhsConfigWarehouse, ProcessTaskTemplateSchema.P0_WW, Warehouses);
			warehouseFilter.Category = FilterCategories.Other;
			warehouseFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|Warehouse", "Warehouse");

			ModuleGuidFilter bmsFilter = filters.AddGuidFilter("Buffer Management System", ModuleIDs.BMSystems, ProcessTaskTemplateSchema.P0_FS_BufferManagementSystem, BMSystems);
			bmsFilter.Category = FilterCategories.Other;
			bmsFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|BMSystem", "Buffer Management System");
		}

		#endregion

		#region Criteria Filters

		static void AddCriteriaFilters(ModuleFilterCollection filters)
		{
			var category = new FilterCategory(ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|SubCriteriaCategory", "Criteria"));

			AddCriteriaFilter(filters, ProcessTaskTemplateSchema.P0_SubType1, "Criteria 1 Code", ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|Criteria1", "Criteria 1 Code")).Category = category;
			AddCriteriaFilter(filters, ProcessTaskTemplateSchema.P0_SubType2, "Criteria 2 Code", ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|Criteria2", "Criteria 2 Code")).Category = category;
			AddCriteriaFilter(filters, ProcessTaskTemplateSchema.P0_SubType3, "Criteria 3 Code", ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|Criteria3", "Criteria 3 Code")).Category = category;
			AddCriteriaFilter(filters, ProcessTaskTemplateSchema.P0_SubType4, "Criteria 4 Code", ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|Criteria4", "Criteria 4 Code")).Category = category;
			AddCriteriaFilter(filters, ProcessTaskTemplateSchema.P0_SubType5, "Criteria 5 Code", ResString.GetMultilingualString("MasterFiles|ProcessTaskTemplateFilter|Criteria5", "Criteria 5 Code")).Category = category;
		}

		static ModuleTextFilter AddCriteriaFilter(ModuleFilterCollection filters, SchemaStringColumn column, string description, MultilingualString multilingualDescription)
		{
			var filter = filters.AddTextFilter(description, column, ComparisonOptions.Default);
			filter.MultilingualDescription = multilingualDescription;

			return filter;
		}

		#endregion

		#endregion

		#region Lookups

		#region Locations

		public LocationCollection Locations
		{
			get
			{
				if (fLocations == null)
				{
					fLocations = new LocationCollection(Factory);
				}
				return fLocations;
			}
		}

		LocationCollection fLocations;

		#endregion

		#region Warehouses

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Factory Cache Key")]
		public BusinessObjectCollection Warehouses
		{
			get
			{
				return Factory.GetCachedValue("Warehouses", delegate
				{
					Type whsWarehouseCollectionType = ObjectFactory.GetType<Warehouse.Integration.IWhsWarehouseCollection>();
					BusinessObjectCollection result = (BusinessObjectCollection)Activator.CreateInstance(whsWarehouseCollectionType, Factory);
					return result;
				});
			}
		}

		#endregion

		#region Buffer Management Systems

		public IBusinessObjectCollection BMSystems
		{
			get
			{
				return Factory.GetCachedValue("BMSystems", delegate
				{
					Type bmsCollectionType = ObjectFactory.GetType<BufferManagement.Integration.IBMSystemCollection>();
					IActiveBusinessObjectCollection result = (IActiveBusinessObjectCollection)Activator.CreateInstance(bmsCollectionType, Factory);
					return result;
				});
			}
		}

		#endregion

		#region Clients

		public OrganisationsFindBoxCollection Clients
		{
			get
			{
				if (fClients == null)
				{
					fClients = new OrganisationsFindBoxCollection(Factory);
				}
				return fClients;
			}
		}

		OrganisationsFindBoxCollection fClients;

		#endregion

		#region Workflow Types

		public CodeDescriptionPairList WorkflowTypeList => Factory.GetCachedValue("ITemplateWorkflowDescriptorList", () => (CodeDescriptionPairList)ObjectFactory.Get<ITemplateWorkflowDescriptorList>());

		#endregion

		#region Fallback Method Types

		public CodeDescriptionPairList FallbackMethodTypeList
		{
			get
			{
				if (fallbackMethodTypeList == null)
				{
					fallbackMethodTypeList = new FallbackTypeList();
				}
				return fallbackMethodTypeList;
			}
		}

		CodeDescriptionPairList fallbackMethodTypeList;

		#endregion

		#endregion
	}
}
