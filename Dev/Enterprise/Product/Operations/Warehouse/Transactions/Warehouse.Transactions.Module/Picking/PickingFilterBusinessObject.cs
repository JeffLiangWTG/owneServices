using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Module;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class PickingFilterBusinessObject : DocketFilterBusinessObject
	{
		public static class Schema
		{
			public const string TrolleyNumber = "Trolley Number"; // Filter description
			public const string DynamicPickAreaOverride = "Dynamic Pick Area Override"; // Filter description
			public const string IsAwaitingReplenishment = "Is Awaiting Replenishment"; // Filter description

			public static class IsAwaitingReplenishmentTypeCodes
			{
				public const string AwaitingReplenishment = "YES"; // Filter Constant
				public const string NotAwaitingReplenishment = "NO"; // Filter Constant
				public const string All = "ALL"; // Filter Constant
			}
		}

		#region Fields

		ModuleGuidFilter DynamicPickAreaOverrideFilter => (ModuleGuidFilter)ModuleFilters[Schema.DynamicPickAreaOverride];

		#endregion

		#region Filters

		protected override void AddExtraFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddTextFilter("Carrier Service Level", value => GetFilteredDocketQuery(new ZQuery(WhsDocketSchema.WD_PL_NKCarrierServiceLevel, value)), GetCarrierServiceLevels);
			filter.MultilingualDescription = ResString.GetMultilingualString("PickingFilterBusinessObject|CarrierServiceLevel", "Carrier Service Level");
			filter.Category = FilterCategories.Other;

			filter = filters.AddTextFilter("Pick Option", WhsPickSchema.WP_PickOption, PickOptions);
			filter.MultilingualDescription = ResString.GetMultilingualString("d99108b3-3549-46b0-9513-cd9c25249d7d", "Pick Option");
			filter.Category = FilterCategories.StatusAndFlags;

			filter = filters.AddTextFilter("Receive Reference (Pick Slip)", GetReceiveReferencePickSlipQuery);
			filter.MultilingualDescription = ResString.GetMultilingualString("418d323d-c160-4c7f-ad02-158020bf5a99", "Receive Reference (Pick Slip)");
			filter.MaxLength = WhsDocketSchema.WD_ExternalReference.MaxLength;
			filter.Category = FilterCategories.NumbersAndReferences;

			filter = filters.AddDateFilter("Finalized Date", GetFinalizedDateQuery);
			filter.MultilingualDescription = ResString.GetMultilingualString("09146987-d871-441d-9365-90a77842d73c", "Finalized Date");

			filter = filters.AddTextFilter("Order Type", GetOrderTypeQuery, OrderTypes);
			filter.MultilingualDescription = ResString.GetMultilingualString("2b9785dd-c7c2-4887-82d3-25c14bb773af", "Order Type");
			filter.Category = FilterCategories.StatusAndFlags;

			filter = filters.AddTextFilter("Consignee Company Name", GetPickableDocketConsigneeCompanyNameQuery);
			filter.MultilingualDescription = ResString.GetMultilingualString("9b5f3aa4-2830-4ca4-b73c-beb2fa69acda", "Consignee Company Name");
			filter.MaxLength = Math.Min(OrgHeaderSchema.OH_FullName.MaxLength, JobDocAddressSchema.E2_CompanyName.MaxLength);
			filter.Category = FilterCategories.Organisations;

			filter = filters.AddTextFilter("Transport Company Name", GetPickableDocketTransportCompanyNameQuery);
			filter.MultilingualDescription = ResString.GetMultilingualString("64e4d215-ab13-4e95-bd08-34c9e3a03f57", "Transport Company Name");
			filter.MaxLength = Math.Min(OrgHeaderSchema.OH_FullName.MaxLength, JobDocAddressSchema.E2_CompanyName.MaxLength);
			filter.Category = FilterCategories.Organisations;

			filter = filters.AddNumberRangeFilter("Pick % Complete", WhsPickSchema.WP_PercentageComplete);
			filter.MultilingualDescription = ResString.GetMultilingualString("62ec7daf-d652-4a57-8924-95e361cacdc2", "Pick % Complete");
			filter.Category = FilterCategories.StatusAndFlags;

			var filterNumber = filters.AddNumberRangeFilter("Priority", GetPriority);
			filterNumber.PropertyType = ZCalcEditPropertyType.Byte;
			filterNumber.MultilingualDescription = ResString.GetMultilingualString("C380220A-3239-4308-8DAE-23546010111", "Pick Priority");
			filterNumber.Category = FilterCategories.NumbersAndReferences;

			var trolleyNumberfilter = filters.AddTextFilter(Schema.TrolleyNumber, GetTrolleyNumberQuery);
			trolleyNumberfilter.MultilingualDescription = ResString.GetMultilingualString("68517591-C8A9-4831-A856-CB90364767C4", "Trolley Number");
			trolleyNumberfilter.MaxLength = RefEquipmentSchema.RQ_Registration.MaxLength;
			trolleyNumberfilter.Category = FilterCategories.NumbersAndReferences;

			var dynamicAreaOverridefilter = filters.AddGuidFilter(Schema.DynamicPickAreaOverride, ModuleIDs.WhsConfigArea, WhsPickSchema.WP_WA_DynamicPickAreaOverride, GetDynamicPickAreas);
			dynamicAreaOverridefilter.MultilingualDescription = ResString.GetMultilingualString("d4bd8142-5873-4597-8749-8689bf8a8a1d", "Dynamic Pick Area Override");
			dynamicAreaOverridefilter.PropertyValidation = DynamicPickAreaOverrideFilterValidation;

			var salesChannelGuidFilter = filters.AddGuidFilter("Sales Channel", ModuleIDs.WhsSalesChannel, GetSalesChannelQuery, new WhsSalesChannelCollection(Factory));
			salesChannelGuidFilter.MultilingualDescription = ResString.GetMultilingualString("7ba7137a-36d2-4738-a9d1-df47932a9eb6", "Sales Channel");
			salesChannelGuidFilter.Category = FilterCategories.Other;

			AddReferenceFilters(filters);
			AddConsigneeFilters(filters);
			AddIsAwaitingReplenishmentFilter(filters);
			if (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.Value)
			{
				AddPickTaskPlanningStatusFilter(filters);
			}

			if (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.Value)
			{
				AddPickTypeFilter(filters);
			}
		}

		#region AddPickTaskPlanningStatusFilter

		void AddPickTaskPlanningStatusFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("PickTaskPlanningStatus", WhsPickSchema.WP_TaskPlanningStatus, PlanningStatus);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("c6742268-d2ae-400f-b7b1-b3316f1fdd04", "Task Planning Status");
		}

		#endregion

		#region AddTransportCoFilter

		protected override void AddTransportCoFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddGuidFilter("Transport Co", ModuleIDs.Organisation, (comparisonOperator, value) => GetDocketDocAddressQuery(TransportCoConstants.AddressTypeCode, value is ZGuid guid ? guid : ZGuid.Empty, comparisonOperator), TransportCos);
			filter.MultilingualDescription = ResString.GetMultilingualString("010c64f5-4d1a-42d6-96bf-6ae7fff54f20", "Transport Co");
		}

		#endregion

		#region AddPickTypeFilter

		void AddPickTypeFilter(ModuleFilterCollection filters)
		{
			var filter = new ModuleTextFilter("Pick Type", WhsPickSchema.WP_PickType, PickTypes);
			filter.MultilingualDescription = ResString.GetMultilingualString("7f75a6ca-b11a-43a2-b5ad-abc6e5a989b4", "Pick Type");
			filter.ComparisonOperator_List.Clear();

			var exactOperator = ModuleTextFilter.ComparisonConstants.GetComparisonOperatorPair(ModuleTextFilter.ComparisonConstants.Exact);
			var notEqualOperator = ModuleTextFilter.ComparisonConstants.GetComparisonOperatorPair(ModuleTextFilter.ComparisonConstants.NotEqual);

			filter.ComparisonOperator_List.Add(exactOperator);
			filter.ComparisonOperator_List.Add(notEqualOperator);

			filters.AddFilter(filter);
		}

		#endregion

		#region GetDocketDocAddressQuery

		protected ZQuery GetDocketDocAddressQuery(ZString docAddressTypeCode, ZGuid orgPK)
		{
			return GetDocketDocAddressQueryCore(docAddressTypeCode, orgPK, SQLComparisonOperator.Equal);
		}

		protected ZQuery GetDocketDocAddressQuery(ZString docAddressTypeCode, ZGuid orgPK, SQLComparisonOperator comparisonOperator)
		{
			return GetDocketDocAddressQueryCore(docAddressTypeCode, orgPK, comparisonOperator);
		}

		protected virtual ZQuery GetDocketDocAddressQueryCore(ZString docAddressTypeCode, ZGuid orgPK, SQLComparisonOperator comparisonOperator)
		{
			var workOrderAddressQuery = WhsWorkOrderDocAddressQueryHelper.GetWorkOrderAddressQuery(docAddressTypeCode, orgPK, comparisonOperator);
			var docketSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.WD_WP);
			docketSubQuery.AddSubQuery(GetJobDocAddressOrgHeaderParentSubQuery(docAddressTypeCode, orgPK, comparisonOperator), JoinCondition.And);
			docketSubQuery.AddSubQuery(workOrderAddressQuery, JoinCondition.Or);

			var result = new ZDBOnlyQuery(typeof(WhsPick));
			result.AddSubQuery(docketSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region AddConsigneeFilters

		protected void AddConsigneeFilters(ModuleFilterCollection filters)
		{
			var consigneeFilter = filters.AddGuidFilter("Consignee", ModuleIDs.Organisation, (comparisonOperator, value)
			  => GetDocketDocAddressQuery(DocAddressTypes.Codes.ConsigneeAddress, value is ZGuid guid ? guid : ZGuid.Empty, comparisonOperator), Consignees);
			consigneeFilter.MultilingualDescription = ResString.GetMultilingualString("42daa3bc-d36c-448b-b1e2-eb047b63486b", "Consignee");

			consigneeFilter.SupportsBlankComparisonOperators = false;
		}

		#endregion

		#region AddIsAwaitingReplenishmentFilter

		void AddIsAwaitingReplenishmentFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(Schema.IsAwaitingReplenishment, GetAwaitingReplenishmentQuery, IsAwaitingReplenishmentTypes);
			filter.MultilingualDescription = ResString.GetMultilingualString("78fbffc4-8bc8-4cee-9995-3d0bef7c4deb", "Is Awaiting Replenishment");
			filter.Category = FilterCategories.StatusAndFlags;
		}

		#endregion

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return new ModuleFountainFilter("Pick No", WhsPickSchema.WP_PickNo, "P") { MultilingualDescription = ResString.GetMultilingualString("09bdf7fd-37d2-4597-a795-ba4ba7d9597d", "Pick No") };
		}

		protected override Type TypeOfBusinessObjectToQuery => typeof(WhsPick);

		protected override SchemaColumn DocketForeignKey => WhsDocketSchema.WD_WP;

		protected override IList<FilterGroupMember> ReferenceFields
		{
			get
			{
				IList<FilterGroupMember> result = base.ReferenceFields;
				result.Add(new FilterGroupMember(ResString.GetMultilingualString("D2DACB1D-7571-4E8E-9FA2-797515F66F1E", "Order No."), WhsDocketSchema.WD_ExternalReference));

				return result;
			}
		}

		protected override ModuleGuidFilter TransportCoFilter => (ModuleGuidFilter)ModuleFilters["Transport Co"];

		protected override ModuleGuidFilter ConsigneeFilter => (ModuleGuidFilter)ModuleFilters["Consignee"];

		#region PackageID Filter

		protected override bool AddPackageIdFilter => true;

		protected override bool AddHandlingUnitFilter => true;

		#endregion

		#region Package Type Filter

		protected override bool AddPackageTypeFilter => true;

		#endregion

		#region GetPackageFilterQueryHelper

		protected override PackageFilterQueryHelper GetPackageFilterQueryHelper(SchemaColumn packageColumnToFilterOn) => new PackageFilterQueryHelper(typeof(WhsPick), typeof(WhsOrder), WhsDocketSchema.WD_WP, packageColumnToFilterOn);

		#endregion

		#region GetPalletIDQuery

		protected override ZQuery GetPalletIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var notIn = SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref comparisonOperator);
			var query = new ZDBOnlyQuery(typeof(WhsPick));

			var orderLineQuery = new ZDBOnlySubQuery(typeof(WhsOrderLine), WhsDocketLineSchema.WE_WD);
			orderLineQuery.AddSubQuery(GetPickLineQueryForPalletID(comparisonOperator, value), JoinCondition.And);

			var orderQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.WD_WP, notIn);
			orderQuery.AddSubQuery(orderLineQuery, JoinCondition.And);

			query.AddSubQuery(orderQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#endregion

		#region Attributes

		#region GetDelegate

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Loop over filters not lookup values, cannot be written as an IN statement")]
		protected override GetTextQueryWithOperator GetDelegate(params FilterGroupMember[] filterGroupMembers)
		{
			switch (filterGroupMembers[0].Column.TableName)
			{
				case WhsPickLineSchema.Constants.TableName:
					return (comparisonOperator, value) =>
					{
						var subQuery = new ZQuery();
						foreach (FilterGroupMember filterGroupMember in filterGroupMembers) // Loop over filters not lookup values, cannot be written as an IN statement
						{
							subQuery.AddToFilter(JoinCondition.Or, filterGroupMember.Column, comparisonOperator, value);
						}
						return GetPickLineQuery(subQuery);
					};
				default:
					return base.GetDelegate(filterGroupMembers);
			}
		}

		#endregion

		#region ExtendDocketQuery

		protected override void ExtendDocketQuery(ZDBOnlyQuery query, ZQuery filter)
		{
			var inventoryLineQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsPickLineSchema.WZ_WE_InventoryLine);
			inventoryLineQuery.AddToFilter(filter);

			var pickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			pickLineSubQuery.AddToFilter(WhsPickLineSchema.WZ_Units, SQLComparisonOperator.GreaterThan, 0m);
			pickLineSubQuery.AddSubQuery(inventoryLineQuery, JoinCondition.And);

			var orderLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
			orderLineSubQuery.AddSubQuery(pickLineSubQuery, JoinCondition.And);

			var orderSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.WD_WP);
			orderSubQuery.AddSubQuery(orderLineSubQuery, JoinCondition.And);

			query.AddSubQuery(orderSubQuery, JoinCondition.Or);
		}

		#endregion

		#endregion

		#region Lookups

		#region OrderTypes

		public OrderType OrderTypes => new OrderType();

		#endregion

		#region IsAwaitingReplenishmentTypes

		public CodeDescriptionPairList IsAwaitingReplenishmentTypes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Schema.IsAwaitingReplenishmentTypeCodes.All, Res.GetString("bacf144d-1008-485b-ae7f-2e420fd46229", "All"));
				result.AddPair(Schema.IsAwaitingReplenishmentTypeCodes.AwaitingReplenishment, Res.GetString("42d02b5d-10c7-416e-9ed2-ab0c345260e1", "Awaiting Replenishment"));
				result.AddPair(Schema.IsAwaitingReplenishmentTypeCodes.NotAwaitingReplenishment, Res.GetString("76d12b1e-a358-4bc1-939b-d69c81f351a1", "Not Awaiting Replenishment"));
				return result;
			}
		}

		#endregion

		#region TaskPlanningStatus

		public TaskPlanningStatus PlanningStatus => new TaskPlanningStatus();

		#endregion

		#region PickTypes

		public CodeDescriptionPairList PickTypes => PickTypesCore();

		protected virtual CodeDescriptionPairList PickTypesCore() => new PickType();

		#endregion

		#endregion

		#region Queries

		#region GetDynamicPickAreas

		WhsDynamicAreaCollection GetDynamicPickAreas()
		{
			return Factory.GetCachedValue("PickingFilterBusinessObject|GetDynamicPickAreas|" + WD_WW_Whs,
				() =>
				{
					return !WD_WW_Whs.IsValid
									? new WhsDynamicAreaCollection(Factory)
									: WhsDynamicAreaCollection.GetDynamicPickingAreas(Factory, Factory.Load<WhsWarehouse>(WD_WW_Whs));
				});
		}

		protected override void OnWarehouseChanged(object sender, EventArgs e)
		{
			var area = Factory.Load<WhsArea>(DynamicPickAreaOverrideFilter.Property);
			if (area == null || WD_WW_Whs != area.WA_WW_Whs)
			{
				DynamicPickAreaOverrideFilter.Property = ZGuid.Empty;
			}
		}

		#endregion

		#region GetWarehouseQuery

		protected override ZQuery GetWarehouseQuery(ZGuid value)
		{
			return new ZQuery(WhsPickSchema.WP_WW_Whs, value);
		}

		#endregion

		#region GetProductQuery

		protected override ZQuery GetProductQuery(ZGuid value)
		{
			var product = Factory.Load<OrgSupplierPart>(value);
			var partNum = product != null ? product.OP_PartNum : ZString.Empty;
			var query = new ZDBOnlyQuery(typeof(WhsPick));
			var orderQuery = new ZDBOnlySubQuery(typeof(WhsOrder), WhsDocketSchema.WD_WP);
			var orderLineQuery = new ZDBOnlySubQuery(typeof(WhsOrderLine), WhsDocketLineSchema.WE_WD);
			var productQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), WhsDocketLineSchema.WE_OP);
			productQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, partNum);
			orderLineQuery.AddSubQuery(productQuery, JoinCondition.And);
			orderQuery.AddSubQuery(orderLineQuery, JoinCondition.And);
			query.AddSubQuery(orderQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region GetCommodityCodeQuery

		protected override ZQuery GetCommodityCodeQuery(ZString value)
		{
			var productSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), WhsDocketLineSchema.WE_OP);
			productSubQuery.AddToFilter(OrgSupplierPartSchema.OP_RH_NKCommodityCode, value);

			return GetOrderLineQuery(productSubQuery);
		}

		#endregion

		#region GetPickableDocketConsigneeCompanyNameQuery

		protected ZQuery GetPickableDocketConsigneeCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			var workOrderSubQuery = WorkOrderConsigneeCompanyNameQuery(comparisonOperator, companyName);
			var orderQuery = OrderConsigneeCompanyNameQuery(comparisonOperator, companyName);

			var docketSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.WD_WP);
			if (workOrderSubQuery != null)
			{
				docketSubQuery.AddSubQuery(workOrderSubQuery, JoinCondition.And);
			}
			docketSubQuery.AddToFilter(orderQuery, JoinCondition.Or);

			var result = new ZDBOnlyQuery(typeof(WhsPick));
			result.AddSubQuery(docketSubQuery, JoinCondition.And);

			return result;
		}

		protected virtual ZDBOnlySubQuery WorkOrderConsigneeCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			return WhsWorkOrderDocAddressQueryHelper.GetWorkOrderConsigneeCompanyNameQuery(comparisonOperator, companyName);
		}

		protected virtual ZQuery OrderConsigneeCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			return base.GetConsigneeCompanyNameQuery(comparisonOperator, companyName);
		}

		#endregion

		#region GetTransportCompanyNameQuery

		protected ZQuery GetPickableDocketTransportCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			var workOrderSubQuery = WorkOrderTransportCompanyNameQuery(comparisonOperator, companyName);
			var orderQuery = OrderTransportCompanyNameQuery(comparisonOperator, companyName);

			var docketSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.WD_WP);
			if (workOrderSubQuery != null)
			{
				docketSubQuery.AddSubQuery(workOrderSubQuery, JoinCondition.And);
			}
			docketSubQuery.AddToFilter(orderQuery, JoinCondition.Or);

			var result = new ZDBOnlyQuery(typeof(WhsPick));
			result.AddSubQuery(docketSubQuery, JoinCondition.And);

			return result;
		}

		protected virtual ZDBOnlySubQuery WorkOrderTransportCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			return WhsWorkOrderDocAddressQueryHelper.GetWorkOrderTransportCoCompanyNameQuery(comparisonOperator, companyName);
		}

		protected virtual ZQuery OrderTransportCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			return base.GetTransportCompanyNameQuery(comparisonOperator, companyName);
		}

		#endregion

		#region GetPrority

		ZQuery GetPriority(INumericZType value1, INumericZType value2)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.WD_WP);
			ModuleNumberRangeFilter.AddToFilters(subQuery, WhsDocketSchema.WD_PickPriority, value1, value2);
			var query = new ZDBOnlyQuery(typeof(WhsPick));
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Docket Status Filter

		protected override ZQuery GetStatusQueryCore(ZString value)
		{
			if (value != "OPN")
			{
				return new ZQuery(WhsPickSchema.WP_PickStatus, value);
			}
			else
			{
				var query = new ZQuery(WhsPickSchema.WP_PickStatus, SQLComparisonOperator.NotEqual, PickStatus.Codes.Cancelled);
				query.AddToFilter(WhsPickSchema.WP_PickStatus, SQLComparisonOperator.NotEqual, PickStatus.Codes.Finalised);

				return query;
			}
		}

		protected override CodeDescriptionPairList GetDocketStatusCore()
		{
			var status = new PickStatus();
			status.AddPair("OPN", Res.GetString("069afbce-da12-4096-9fe8-c0998d937fb2", "Open - Not Finalized or Canceled"));
			return status;
		}

		#endregion

		#region GetFinalizedDateQuery

		ZQuery GetFinalizedDateQuery(DateComparisonOperator comparisonOperator, ZDateTimeOffset fromDate, ZDateTimeOffset toDate)
		{
			var result = new ZDBOnlyQuery(typeof(WhsPick));
			var subQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.WD_WP);
			AddDateTimeOffsetRange(subQuery, comparisonOperator, JoinCondition.And, WhsDocketSchema.WD_FinalisedDate, fromDate.DateAndOffset, toDate.DateAndOffset, false, false);
			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region GetReceiveReferencePickSlipQuery

		ZQuery GetReceiveReferencePickSlipQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var receiptQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketLineSchema.WE_WD);
			receiptQuery.AddToFilter(WhsDocketSchema.WD_ExternalReference, comparisonOperator, value);

			var pickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			pickLineSubQuery.AddToFilter(WhsPickLineSchema.WZ_Units, SQLComparisonOperator.GreaterThan, 0m);
			pickLineSubQuery.AddToFilter(GetPickedFromSourceLocationQuery(receiptQuery));

			return GetOrderLineQuery(pickLineSubQuery);
		}

		#endregion

		#region  GetOrderTypeQuery

		ZQuery GetOrderTypeQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(WhsPick));
			var subQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.WD_WP);

			subQuery.AddToFilter(WhsDocketSchema.WD_DocketSubType, SQLComparisonOperator.Equal, value);

			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region GetOrderLineQuery

		static ZDBOnlyQuery GetOrderLineQuery(ZDBOnlySubQuery subQuery)
		{
			var orderLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
			orderLineSubQuery.AddSubQuery(subQuery, JoinCondition.And);

			var orderSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.WD_WP);
			orderSubQuery.AddSubQuery(orderLineSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(WhsPick));
			query.AddSubQuery(orderSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region GetPickLineQuery

		ZQuery GetPickLineQuery(ZQuery pickLineFilter)
		{
			var pickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			pickLineSubQuery.AddToFilter(pickLineFilter);

			var query = GetOrderLineQuery(pickLineSubQuery);

			return query;
		}

		#endregion

		#region GetAwaitingReplenishmentQuery

		ZQuery GetAwaitingReplenishmentQuery(ZString value)
		{
			var query = new ZQuery();
			if (value == Schema.IsAwaitingReplenishmentTypeCodes.AwaitingReplenishment)
			{
				query.AddToFilter(WhsPickSchema.WP_IsAwaitingReplenishment, true);
			}
			else if (value == Schema.IsAwaitingReplenishmentTypeCodes.NotAwaitingReplenishment)
			{
				query.AddToFilter(WhsPickSchema.WP_IsAwaitingReplenishment, false);
			}
			return query;
		}

		#endregion

		#region GetSalesChannelQuery

		ZQuery GetSalesChannelQuery(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(WhsPick));
			var orderQuery = new ZDBOnlySubQuery(typeof(WhsOrder), WhsDocketSchema.WD_WP);
			orderQuery.AddToFilter(WhsDocketSchema.WD_WSH_SalesChannel, value);
			query.AddSubQuery(orderQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region GetDistributionCentreNameQuery

		protected override ZQuery GetDistributionCentreNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			var orderQuery = base.GetDistributionCentreNameQuery(comparisonOperator, companyName);

			var docketSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.WD_WP);
			docketSubQuery.AddToFilter(orderQuery, JoinCondition.Or);

			var result = new ZDBOnlyQuery(typeof(WhsPick));
			result.AddSubQuery(docketSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region SupportsDistributionCentreFilters

		protected override bool SupportsDistributionCentreFilters => true;

		#endregion

		#endregion

		#region Validations

		#region DynamicPickAreaOverrideFilterValidation

		void DynamicPickAreaOverrideFilterValidation(ZPropertyInfo info)
		{
			if (!DynamicPickAreaOverrideFilter.Property.IsEmpty &&
				(!WarehouseFilter.IsActive || !WarehouseFilter.Property.IsValid))
			{
				info.AddError(Res.GetString("4049ef36-fd05-48ca-8fb2-e1293de355fa", "Dynamic Pick Area Override can not be entered without a warehouse."));
			}
		}

		#endregion

		#endregion

		#region IsAddServiceLevelFilter

		protected override bool IsAddServiceLevelFilter => true;

		protected override ZQuery GetServiceLevelQuery(ZString serviceLevel)
		{
			var docketSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.WD_WP);
			docketSubQuery.AddToFilter(WhsDocketSchema.WD_RS_NKServiceLevel, serviceLevel);

			var pickQuery = new ZDBOnlyQuery(typeof(WhsPick));
			pickQuery.AddSubQuery(docketSubQuery, JoinCondition.And);

			return pickQuery;
		}

		#endregion

		#region IncludeTransportCoFilter

		protected override bool IncludeTransportCoFilter => true;

		#endregion
	}
}
