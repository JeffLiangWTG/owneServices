using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	public class TrackingContainerFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class DateColumns
		{
			public static readonly SchemaDateTimeColumn ActualDehireColumn = JobContainerSchema.JC_ContainerYardEmptyReturnGateIn;
			public static readonly SchemaDateTimeColumn ActualDeliveryColumn = JobContainerSchema.JC_ArrivalCartageComplete;
			public static readonly SchemaDateTimeColumn AvailableColumn = JobContainerSchema.JC_FCLAvailable;
			public static readonly SchemaDateTimeColumn ConfirmedDeliveryColumn = JobContainerSchema.JC_ArrivalCartageAdvised;
			public static readonly SchemaDateTimeColumn EmptyReadyColumn = JobContainerSchema.JC_EmptyReadyForReturn;
			public static readonly SchemaDateTimeColumn EmptyReturnRequiredColumn = JobContainerSchema.JC_EmptyReturnedBy;
			public static readonly SchemaDateTimeColumn ActualEmptyPickupColumn = JobContainerSchema.JC_EmptyReturnedBy;
			public static readonly SchemaDateTimeColumn RequiredDeliveryColumn = JobContainerSchema.JC_ArrivalEstimatedDelivery;
			public static readonly SchemaDateTimeColumn SlotDateColumn = JobContainerSchema.JC_ArrivalSlotDateTime;
		}

		#region Module Filter Collection

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			ModuleFilter filter;

			// Number Filters
			var consolNumberFilter = filters.AddTextFilter("Consol Number", GetConsolNumberQuery);
			consolNumberFilter.MultilingualDescription = ResString.GetMultilingualString("042456dc-8213-413b-b2f4-bb08fa4dfcad", "Consol Number");
			consolNumberFilter.SubGroup = ConsolSubGroup;

			filters.AddTextFilter("Container Number", GetContainerNumberQuery).MultilingualDescription = ResString.GetMultilingualString("ec8cd7a1-70f4-47c6-af6e-b24a6068504d", "Container Number");
			var masterBillNumberFilter = filters.AddTextFilter("Master Bill Number", GetMasterBillNumberQuery);
			masterBillNumberFilter.MultilingualDescription = ResString.GetMultilingualString("ebb55812-55c6-45f3-ac0c-548d515c5301", "Master Bill Number");
			masterBillNumberFilter.SubGroup = ConsolSubGroup;

			filters.AddTextFilter("Shipment Number", GetShipmentNumberQuery).MultilingualDescription = ResString.GetMultilingualString("480af660-5dfa-4cc4-80be-c7dbbec85f08", "Shipment Number");
			filters.AddTextFilter("Port Transport Ref", GetCartageRefNumQuery).MultilingualDescription = ResString.GetMultilingualString("edb82d90-88b1-41e9-9693-9e25cf0f5dc8", "Port Transport Ref");
			filters.AddTextFilter("Container Status", GetContainerStatusQuery, FreightDataRegistry.Instance.ContainerStatusList.Value).MultilingualDescription = ResString.GetMultilingualString("63b91071-c4f9-40c8-9e6b-e75ac81667bd", "Container Status");

			// Date Filters
			filters.AddDateFilter("Actual De-hire", DateColumns.ActualDehireColumn).MultilingualDescription = ResString.GetMultilingualString("b6776be7-b011-403e-a821-32b406e4adba", "Actual De-hire");
			filters.AddDateFilter("Actual Delivery", DateColumns.ActualDeliveryColumn).MultilingualDescription = ResString.GetMultilingualString("066de67f-a64c-435e-bc1e-6530e1162be0", "Actual Delivery");
			filters.AddDateFilter("Actual Empty Pickup", DateColumns.ActualEmptyPickupColumn).MultilingualDescription = ResString.GetMultilingualString("cbc584ea-91ec-4921-9d88-c55372c049e2", "Actual Empty Pickup");
			filters.AddDateFilter("Available", GetAvailableQuery).MultilingualDescription = ResString.GetMultilingualString("75e4cf18-8759-441b-8fa9-8cb53014e64d", "Available");
			filters.AddDateFilter("Confirmed Delivery", DateColumns.ConfirmedDeliveryColumn).MultilingualDescription = ResString.GetMultilingualString("ffe7f98f-1521-4d4b-9222-d2cd1e8b2606", "Confirmed Delivery");
			filters.AddDateFilter("Empty Ready", DateColumns.EmptyReadyColumn).MultilingualDescription = ResString.GetMultilingualString("294c5d46-7805-4ddd-8fa9-ff37dadedcca", "Empty Ready");
			filters.AddDateFilter("Empty Return Required", DateColumns.EmptyReturnRequiredColumn).MultilingualDescription = ResString.GetMultilingualString("5c2d57d4-be19-4b67-913b-6ad6e9351a20", "Empty Return Required");
			filters.AddDateFilter("ETA", GetETADateQuery).MultilingualDescription = ResString.GetMultilingualString("c5dda668-9312-4b15-8893-1e3e002558c9", "ETA");
			filters.AddDateFilter("Required Delivery", DateColumns.RequiredDeliveryColumn).MultilingualDescription = ResString.GetMultilingualString("080ebd11-8ac1-409e-a2d7-af9560e1227f", "Required Delivery");
			filters.AddDateFilter("Slot Date", DateColumns.SlotDateColumn).MultilingualDescription = ResString.GetMultilingualString("c1f03bba-96ea-43a4-a3e4-ca2f122940e9", "Slot Date");

			// Location Filters
			var locationFilter = filters.AddLocationFilter("Load / Discharge", GetLoadDischargePortsQuery, LocationFilter_List, LocationFilter_List);
			locationFilter.MultilingualDescription = ResString.GetMultilingualString("d5d765cf-440f-4ab3-a70f-f428018b7f0a", "Load / Discharge");
			locationFilter.SetItemDescriptions(Res.GetData("b5165508-6151-4386-b3cd-919e8db3ce18", "Load"), Res.GetData("7326eb20-cb04-4ab8-9dcb-96d5349e7799", "Discharge"));

			// Mode and Type Filters
			filter = filters.AddTextFilter("Container Type", GetTypeQuery);
			filter.MultilingualDescription = ResString.GetMultilingualString("c04305bb-7d70-4dce-bc8b-d6e1485509b6", "Container Type");
			filter.SubGroup = new RefContainerSubGroup();
			filter.Category = FilterCategories.ModesAndTypes;

			filter = filters.AddTextFilter("Mode", GetModeQuery, ModeFilter_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("c7010f4b-bd4e-4e34-88be-194282212b68", "Mode");
			filter.Category = FilterCategories.ModesAndTypes;

			// Organisation/Staff Filters
			var consigneeFilter = filters.AddGuidFilter("Consignee", ModuleIDs.Organisation, GetConsigneeQuery, ConsigneeFilter_List);
			consigneeFilter.MultilingualDescription = ResString.GetMultilingualString("f2eb5cca-0b71-4472-a641-26a2f9b46788", "Consignee");
			consigneeFilter.SubGroup = new OrgAddressSubGroup(new JobDocAddressSubGroup(PackSubGroup));

			return filters;
		}

		TrackingConsolSubGroup ConsolSubGroup
		{
			get
			{
				if (consolSubGroup == null)
				{
					consolSubGroup = new TrackingConsolSubGroup();
				}
				return consolSubGroup;
			}
		}
		TrackingConsolSubGroup consolSubGroup;

		PackLineSubGroup PackSubGroup
		{
			get
			{
				if (packSubGroup == null)
				{
					packSubGroup = new PackLineSubGroup(new ContainerPackPivotSubGroup());
				}
				return packSubGroup;
			}
		}
		PackLineSubGroup packSubGroup;

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			helpers.Add(new WorkflowFilterStripsHelper(typeof(TrackingContainer), WorkflowDescriptors.ContainerWorkflowDescriptorCode, Factory));

			return helpers;
		}

		#endregion Module Filter Collection

		#region Queries

		#region Status Filter Queries

		ZQuery GetContainerStatusQuery(ZString value)
		{
			return new ZQuery(JobContainerSchema.JC_ContainerStatus, value);
		}

		#endregion

		#region Number Filter Queries

		ZQuery GetCartageRefNumQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(JobContainerSchema.JC_DepartureCartageRef, comparisonOperator, value);
		}

		ZQuery GetShipmentNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(TrackingContainer));
			ZDBOnlySubQuery shipmentNumberSubQuery = ContainerFilterFactory.Instance.GetContainerShipmentSubQuery(JobShipmentSchema.JS_UniqueConsignRef, comparisonOperator, value);
			ZDBOnlySubQuery declarationNumberSubQuery = ContainerFilterFactory.Instance.GetContainerStandAloneDeclarationSubQuery(JobDeclarationSchema.JE_DeclarationReference, comparisonOperator, value);
			filter.AddSubQuery(shipmentNumberSubQuery, JoinCondition.And);
			filter.AddSubQuery(declarationNumberSubQuery, JoinCondition.Or);
			return filter;
		}

		protected class PackLineSubGroup : ModuleFilterSubGroup
		{
			public PackLineSubGroup(ModuleFilterSubGroup parent) : base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(JobContainerPackPivot));

				var packlineSubQuery = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.PK);
				packlineSubQuery.AddToFilter(filter);
				result.AddSubQuery(JobContainerPackPivotSchema.J6_JL, packlineSubQuery, JoinCondition.And);

				return result;
			}
		}

		protected class ContainerPackPivotSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(TrackingContainer));

				var containerPackPivotSubQuery = new ZDBOnlySubQuery(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JC);
				containerPackPivotSubQuery.AddToFilter(filter);
				result.AddSubQuery(containerPackPivotSubQuery, JoinCondition.And);

				return result;
			}
		}

		ZQuery GetMasterBillNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(TrackingConsol));
			filter.AddToFilter(JobConsolSchema.JK_MasterBillNum, comparisonOperator, value);
			return filter;
		}

		ZQuery GetContainerNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(JobContainerSchema.JC_ContainerNum, comparisonOperator, value.SubstringSafe(0, JobContainerSchema.JC_ContainerNum.MaxLength));
		}

		ZQuery GetConsolNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(TrackingConsol));
			filter.AddToFilter(JobConsolSchema.JK_UniqueConsignRef, comparisonOperator, value);
			return filter;
		}

		protected class TrackingConsolSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(TrackingContainer));

				var consolSubQuery = new ZDBOnlySubQuery(typeof(TrackingConsol), JobContainerSchema.JC_JK);
				consolSubQuery.AddToFilter(filter);
				result.AddSubQuery(consolSubQuery, JoinCondition.And);

				return result;
			}
		}

		#endregion Number Filter Queries

		#region Date Filter Queries

		ZQuery GetETADateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			SailingFilterBuilder builder = new SailingFilterBuilder(Factory);
			builder.SetDateRange(SailingFilterBuilder.Dates.ETA, comparisonOperator, fromDate, toDate);
			ZQuery result = builder.ToContainerFilter();

			ZDBOnlySubQuery cusContainerFilter = new ZDBOnlySubQuery(typeof(BaseCusContainer), CusContainerSchema.CO_JC);
			ZDBOnlySubQuery declarationFilter = new ZDBOnlySubQuery(typeof(Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.PK);
			AddDateRange(declarationFilter, comparisonOperator, JoinCondition.Or, JobDeclarationSchema.JE_DateOfArrival, fromDate.Date, toDate.Date);

			cusContainerFilter.AddSubQuery(CusContainerSchema.CO_JE, declarationFilter, JoinCondition.And);

			ZDBOnlyQuery containerFilter = new ZDBOnlyQuery(typeof(TrackingContainer));
			containerFilter.AddSubQuery(JobContainerSchema.PK, cusContainerFilter, JoinCondition.And);

			result.AddToFilter(containerFilter, JoinCondition.Or);
			return result;
		}

		ZQuery GetAvailableQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var result = new ZQuery();

			var builder = new SailingFilterBuilder(Factory);
			builder.SetDateRange(SailingFilterBuilder.Dates.FCLAvailable, comparisonOperator, fromDate, toDate);

			var consolContainerQuery = builder.ToContainerFilter();
			consolContainerQuery.AddToFilter(JobContainerSchema.JC_OverrideFCLAvailableStorage, false);

			var containerFilter = new ZQuery();
			AddDateRange(containerFilter, comparisonOperator, JoinCondition.And, JobContainerSchema.JC_FCLAvailable, fromDate.Date, toDate.Date);
			containerFilter.AddToFilter(JobContainerSchema.JC_OverrideFCLAvailableStorage, true);

			var declarationContainerQuery = builder.ToDeclarationContainerFilter();
			declarationContainerQuery.AddToFilter(JobContainerSchema.JC_OverrideFCLAvailableStorage, false);

			result.AddToFilter(consolContainerQuery, JoinCondition.Or);
			result.AddToFilter(declarationContainerQuery, JoinCondition.Or);
			result.AddToFilter(containerFilter, JoinCondition.Or);

			return result;
		}

		#endregion Date Filter Queries

		#region Mode/Type Filter Queries

		ZQuery GetModeQuery(ZString value)
		{
			return new ZQuery(JobContainerSchema.JC_ContainerMode, value);
		}

		ZQuery GetTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(RefContainer));
			filter.AddToFilter(RefContainerSchema.RC_Code, comparisonOperator, value);
			return filter;
		}

		protected class RefContainerSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(TrackingContainer));

				var refContainerSubQuery = new ZDBOnlySubQuery(typeof(RefContainer), JobContainerSchema.JC_RC);
				refContainerSubQuery.AddToFilter(filter);
				result.AddSubQuery(refContainerSubQuery, JoinCondition.And);

				return result;
			}
		}

		#endregion Mode/Type Filter Queries

		#region Location Filter Queries

		ZQuery GetLoadDischargePortsQuery(ZString loadPort, ZString dischargePort)
		{
			var filter = new ZDBOnlyQuery(typeof(TrackingContainer));
			var consolSubQuery = new ZDBOnlySubQuery(typeof(TrackingConsol), JobContainerSchema.JC_JK);
			var declarationSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
			if (!loadPort.IsEmpty)
			{
				consolSubQuery.AddToFilter(GetLocationQuery(JobConsolSchema.JK_RL_NKLoadPort, loadPort, typeof(TrackingConsol)));
				declarationSubQuery.AddToFilter(GetLocationQuery(JobDeclarationSchema.JE_RL_NKPortOfLoading, loadPort, typeof(BaseJobDeclaration)));
			}
			if (!dischargePort.IsEmpty)
			{
				consolSubQuery.AddToFilter(GetLocationQuery(JobConsolSchema.JK_RL_NKDischargePort, dischargePort, typeof(TrackingConsol)));
				declarationSubQuery.AddToFilter(GetLocationQuery(JobDeclarationSchema.JE_RL_NKPortOfArrival, dischargePort, typeof(BaseJobDeclaration)));
			}

			filter.AddSubQuery(consolSubQuery, JoinCondition.And);
			filter.AddSubQuery(ContainerFilterFactory.Instance.GetContainerStandAloneDeclarationSubQuery(declarationSubQuery), JoinCondition.Or);
			return filter;
		}

		ZQuery GetLocationQuery(SchemaColumn schemaColumn, ZString location, Type typeToQuery)
		{
			return LocationHelper.GetLocationFilter(Factory, location, schemaColumn, typeToQuery);
		}

		#endregion Location Filter Queries

		#region Organisation/Staff Queries

		ZQuery GetConsigneeQuery(ZGuid value)
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(OrgAddress));
			filter.AddToFilter(OrgAddressSchema.OA_OH, value);
			return filter;
		}

		protected class OrgAddressSubGroup : ModuleFilterSubGroup
		{
			public OrgAddressSubGroup(ModuleFilterSubGroup parent) : base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(JobDocAddress));

				var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				orgAddressSubQuery.AddToFilter(filter);
				result.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressSubQuery, JoinCondition.And);
				result.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, "JS");
				result.AddToFilter(JobDocAddressSchema.E2_AddressType, new string[] {
				AutoDocAddressTypes.Codes.ConsigneeAddress,
				AutoDocAddressTypes.Codes.ConsigneeDocumentaryAddress,
				AutoDocAddressTypes.Codes.ConsigneePickupDeliveryAddress
			});

				return result;
			}
		}

		protected class JobDocAddressSubGroup : ModuleFilterSubGroup
		{
			public JobDocAddressSubGroup(ModuleFilterSubGroup parent) : base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(PackLine));

				var jobDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
				jobDocAddressSubQuery.AddToFilter(filter);
				result.AddSubQuery(JobPackLinesSchema.JL_JS, jobDocAddressSubQuery, JoinCondition.And);

				return result;
			}
		}

		#endregion Organisation/Staff Queries

		#endregion Queries

		#region Properties/Lookups

		#region Current Logged in Organisation

		public ZGuid CurrentOrg
		{
			get { return fCurrentOrg; }
			set { fCurrentOrg = value; }
		}
		ZGuid fCurrentOrg;

		#endregion CurrentOrg

		#region Mode Filter List

		public CodeDescriptionPairList ModeFilter_List
		{
			get
			{
				if (fModeFilter_List == null)
				{
					fModeFilter_List = new CodeDescriptionPairList(OLookUpEditType.ContainerMode);
				}

				return fModeFilter_List;
			}
		}
		CodeDescriptionPairList fModeFilter_List;

		#endregion Mode Filter List

		#region Type Filter List

		public RefContainerCollection TypeFilter_List
		{
			get
			{
				if (fTypeFilter_List == null)
				{
					fTypeFilter_List = new RefContainerCollection(Factory);
				}

				return fTypeFilter_List;
			}
		}
		RefContainerCollection fTypeFilter_List;

		#endregion Type Filter List

		#region Consignee Filter List

		public OrgHeaderCollection ConsigneeFilter_List
		{
			get
			{
				if (fConsigneeFilter_List == null)
				{
					fConsigneeFilter_List = new OrgHeaderCollection(Factory);
				}

				return fConsigneeFilter_List;
			}
		}
		OrgHeaderCollection fConsigneeFilter_List;

		#endregion Consignee Filter List

		#region Location Filter List

		public LocationCollection LocationFilter_List
		{
			get { return new LocationCollection(Factory); }
		}

		#endregion Locations

		#endregion Properties/Lookups

	}
}
