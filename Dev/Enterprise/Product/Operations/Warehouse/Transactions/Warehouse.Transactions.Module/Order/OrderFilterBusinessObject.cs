using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportConsignment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
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
	public class OrderFilterBusinessObject : PickableDocketFilterBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			var carrierBookingAgentFilter = filters.AddGuidFilter("Carrier Booking Agent", ModuleIDs.Organisation, value => GetDocAddressQuery(value, DocAddressTypes.Codes.CarrierBookingAgent), CarrierBookingAgents);
			carrierBookingAgentFilter.MultilingualDescription = ResString.GetMultilingualString("da29864a-da73-4aa7-a954-75daa56a825c", "Carrier Booking Agent");

			var filterFlag = filters.AddFlagsFilter(HasHeldPackagesFilterName, new string[] { HasHeldPackagesFilterDescription }, new GetFlagsQuery[] { GetOrdersWithHeldPackages });
			filterFlag.MultilingualDescription = HasHeldPackagesFilterDescription;
			filterFlag.Category = FilterCategories.StatusAndFlags;

			var filter = filters.AddTextFilter(AuditStatusFilterName, GetAuditStatusQuery, AuditStatuses);
			filter.MultilingualDescription = AuditStatusFilterDescription;
			filter.Category = FilterCategories.StatusAndFlags;

			var packingRequiredResourceString = ResString.GetMultilingualString("AEF65632-58FD-47DE-8CD4-615D58495892", "Packing Required");
			var packingRequiredFlag = filters.AddFlagsFilter(Schema.PackingRequired, new string[] { packingRequiredResourceString }, new GetFlagsQuery[] { v => new ZQuery(WhsDocketSchema.WD_PackingAfterPickingRequired, v) });
			packingRequiredFlag.MultilingualDescription = packingRequiredResourceString;
			packingRequiredFlag.Category = FilterCategories.StatusAndFlags;

			filter = filters.AddTextFilter(Schema.TransportJobNumber, GetTransportJobQuery);
			filter.MaxLength = Math.Min(DtbBookingSchema.KM_JobID.MaxLength, JobCartageSchema.JJ_ConsignmentID.MaxLength);
			filter.MultilingualDescription = ResString.GetMultilingualString("387ca1ad-ff00-449c-a0af-e8d03a767cfe", "Transport Job Number");
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MaxLength = Math.Min(DtbBookingSchema.KM_JobID.MaxLength, JobCartageSchema.JJ_ConsignmentID.MaxLength);

			AddTextFilters(filters);
			AddOthersFilters(filters);
			filters.AddFlagsFilter("Not Invoiced", new string[] { Res.GetString("3668f3cf-68c7-4017-a9a7-f85b865de3ac", "Not Invoiced") }, new GetFlagsQuery[] { GetNotInvoicedQuery }).MultilingualDescription = ResString.GetMultilingualString("3668f3cf-68c7-4017-a9a7-f85b865de3ac", "Not Invoiced");
			AddWorkflowFilterStripsHelper(typeof(WhsOrder), WorkflowDescriptors.WhsOrderWorkflowDescriptorCode);
			SecurityProvider.AddCRMSecurityFilterStrips(Factory, filters);

			AddTransportZoneFilter(filters);
			AddOutboundLocationFilter(filters);
			AddServiceTypeDateFilter(filters);

			if (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.Value)
			{
				AddHeldInventoryFilter(filters);
			}

			return filters;
		}

		protected override bool IncludeTransportCoFilter => true;

		protected override bool IncludeConsigneeFilter => true;

		protected override bool IncludeDeliveryRouteFilters => true;

		ZQuery GetOrdersWithHeldPackages(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(WhsDocket));
			var packageSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob);
			var packageJobSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageJobSchema.KJ_ParentID);

			packageSubQuery.AddToFilter(PkgPackageSchema.KP_IsHeld, value);
			packageJobSubQuery.AddSubQuery(packageSubQuery, JoinCondition.And);
			query.AddSubQuery(packageJobSubQuery, JoinCondition.And);

			return query;
		}

		#region GetTransportJobQuery

		ZQuery GetTransportJobQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var isBlank = comparisonOperator == SpecialComparisonOperator.IsBlank; // Is Blank = NO transport jobs, impossible for job numbers to be blank
			var notIn = SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref comparisonOperator);
			var docketResult = new ZDBOnlyQuery(typeof(WhsDocket));

			var consignmentQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<IDtbBookingConsignment>(), DtbBookingSchema.KM_KB_Booking);
			consignmentQuery.AddToFilter(DtbBookingSchema.KM_JobID, comparisonOperator, value);

			var consignmentConsolidationQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<IDtbConsignmentConsolidation>(), DtbBookingConsolidationSchema.KB_ParentID);
			consignmentConsolidationQuery.AddSubQuery(consignmentQuery, JoinCondition.And);

			var transportBookingQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<IDtbBooking>(), DtbBookingSchema.KM_KB_Booking);
			transportBookingQuery.AddToFilter(DtbBookingSchema.KM_JobID, comparisonOperator, value);

			var tbConsolidationQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<IDtbBookingConsolidation>(), DtbBookingConsolidationSchema.KB_ParentID);

			var portTransportQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<ICommonCartage>(), JobCartageSchema.JJ_ParentID);

			if (!isBlank)
			{
				portTransportQuery.AddToFilter(JobCartageSchema.JJ_ConsignmentID, comparisonOperator, value);

				transportBookingQuery.AddSubQuery(portTransportQuery, JoinCondition.Or);
				transportBookingQuery.AddSubQuery(consignmentConsolidationQuery, JoinCondition.Or);

				tbConsolidationQuery.AddSubQuery(transportBookingQuery, JoinCondition.And);
			}

			var subQueries = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.PK, notIn || isBlank);
			subQueries.AddSubQuery(portTransportQuery, JoinCondition.Or);
			subQueries.AddSubQuery(tbConsolidationQuery, JoinCondition.Or);

			docketResult.AddSubQuery(subQueries, JoinCondition.And);
			return docketResult;
		}

		#endregion

		#region GetAuditStatusQuery

		#region SuppressResourceStringsCheckRegion

		ZQuery GetAuditStatusQuery(ZString value)
		{
			switch (value)
			{
				case OrderAuditStatus.Codes.NotRequired:
					return GetAuditStatusNotRequired();
				case OrderAuditStatus.Codes.Failed:
					return GetAuditStatusFailed();
				case OrderAuditStatus.Codes.Passed:
					return GetAuditStatusPassed();
				case OrderAuditStatus.Codes.Pending:
					return GetAuditStatusPending();
				default:
					return ZQuery.NoResultQuery;
			}
		}

		ZDBOnlyQuery GetAuditStatusNotRequired()
		{
			var query = new ZDBOnlyQuery(typeof(WhsDocket));
			var sqlFilterString = string.Format(CultureInfo.InvariantCulture, "{0} NOT IN ({1})", WhsDocketSchema.PK.Name, GetSqlForOrdersWithFailures());

			query.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order);
			query.AddToFilter(WhsDocketSchema.WD_QualityAuditRequired, false);
			query.AddFilterAndZSQLParameterCollection(sqlFilterString, new ZSqlParameterCollection());

			return query;
		}

		ZDBOnlyQuery GetAuditStatusFailed()
		{
			var query = new ZDBOnlyQuery(typeof(WhsDocket));
			var allPackagesWithFailuresFilter = string.Format(CultureInfo.InvariantCulture, "{0} IN ({1})", WhsDocketSchema.PK.Name, GetSqlForOrdersWithFailures());
			var sqlFilterParameters = new ZSqlParameterCollection();
			var allPackagesAuditedSubQuery = string.Format(CultureInfo.InvariantCulture, @"(WD_QualityAuditRequired = 0 OR (WD_QualityAuditRequired = 1 AND WD_PK NOT IN({0})))", GetSqlForOrdersWithPackagesWithoutAudit());

			query.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order);
			query.AddFilterAndZSQLParameterCollection(allPackagesWithFailuresFilter, sqlFilterParameters, JoinCondition.And);
			query.AddFilterAndZSQLParameterCollection(allPackagesAuditedSubQuery, sqlFilterParameters, JoinCondition.And);

			return query;
		}

		ZDBOnlyQuery GetAuditStatusPassed()
		{
			var query = new ZDBOnlyQuery(typeof(WhsDocket));
			query.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order);
			query.AddToFilter(WhsDocketSchema.WD_QualityAuditRequired, true);

			var sqlFilterParameters = new ZSqlParameterCollection();
			var ordersWithPackagesFilter = string.Format(CultureInfo.InvariantCulture, "{0} IN ({1})", WhsDocketSchema.PK.Name, GetSqlForOrdersWithPackages());
			var ordersWithoutErrorsFilter = string.Format(CultureInfo.InvariantCulture, "{0} NOT IN ({1})", WhsDocketSchema.PK.Name, GetSqlForOrdersWithFailures());
			var allPackagesAuditedSubQuery = string.Format(CultureInfo.InvariantCulture, "{0} NOT IN ({1})", WhsDocketSchema.PK.Name, GetSqlForOrdersWithPackagesWithoutAudit());

			query.AddFilterAndZSQLParameterCollection(ordersWithPackagesFilter, sqlFilterParameters, JoinCondition.And);
			query.AddFilterAndZSQLParameterCollection(ordersWithoutErrorsFilter, sqlFilterParameters, JoinCondition.And);
			query.AddFilterAndZSQLParameterCollection(allPackagesAuditedSubQuery, sqlFilterParameters, JoinCondition.And);

			return query;
		}

		ZDBOnlyQuery GetAuditStatusPending()
		{
			var query = new ZDBOnlyQuery(typeof(WhsDocket));
			var ordersWithUnauditedPackagesOrOrdersWithoutPackagesSubQuery = string.Format(CultureInfo.InvariantCulture, "({0} IN ({1}) OR {0} NOT IN ({2}))", WhsDocketSchema.PK.Name, GetSqlForOrdersWithPackagesWithoutAudit(), GetSqlForOrdersWithPackages());

			query.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order);
			query.AddToFilter(WhsDocketSchema.WD_QualityAuditRequired, true);
			query.AddFilterAndZSQLParameterCollection(ordersWithUnauditedPackagesOrOrdersWithoutPackagesSubQuery, new ZSqlParameterCollection(), JoinCondition.And);

			return query;
		}

		ZString GetSqlForOrdersWithFailures()
		{
			return $@" SELECT WPA_WD_Order FROM (
							SELECT PackageAudit.*
							FROM dbo.WhsPackageAudit PackageAudit
							JOIN (
								SELECT WPA_WD_Order, WPA_PackageID, MAX(WPA_AuditCompleteTime) AS WPA_AuditCompleteTime
								FROM dbo.WhsPackageAudit
								GROUP BY WPA_WD_Order, WPA_PackageID
							) AS PackageAuditNewest
							ON PackageAudit.WPA_WD_Order = PackageAuditNewest.WPA_WD_Order
							AND PackageAudit.WPA_PackageID = PackageAuditNewest.WPA_PackageID
							AND PackageAudit.WPA_AuditCompleteTime = PackageAuditNewest.WPA_AuditCompleteTime
						) AS LatestAudits
						JOIN
						dbo.WhsPackageAuditLineFailure ON WhsPackageAuditLineFailure.WPF_WPA_WhsPackageAudit = LatestAudits.WPA_PK ";
		}

		ZString GetSqlForOrdersWithPackagesWithoutAudit()
		{
			return $@"SELECT KJ_ParentID
			FROM dbo.PkgPackage
			JOIN dbo.PkgPackageHeader ON KP_KPH_PackageHeader = KPH_PK
			JOIN dbo.PkgPackageJob ON KP_KJ_ParentPackageJob = KJ_PK
			WHERE KJ_ParentID = WD_PK
			AND NOT EXISTS(SELECT * FROM dbo.WhsPackageAudit WHERE WPA_WD_Order = WD_PK AND WPA_PackageID = KPH_PackageID)";
		}

		ZString GetSqlForOrdersWithPackages()
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				SELECT KJ_ParentID
				FROM dbo.PkgPackage
				JOIN dbo.PkgPackageHeader ON KP_KPH_PackageHeader = KPH_PK
				JOIN dbo.PkgPackageJob ON KP_KJ_ParentPackageJob = KJ_PK
				WHERE KJ_ParentTableCode = '{0}'", WhsDocketSchema.Constants.Prefix);
		}

		#endregion

		#endregion

		#region AddServiceTypeDateFilter

		void AddServiceTypeDateFilter(ModuleFilterCollection filters)
		{
			var serviceTypeDateBookedFilter = new ServiceTypeDateFilter(this, typeof(WhsDocket), true);
			filters.AddCustomFilter(serviceTypeDateBookedFilter);

			var serviceTypeDateCompletedFilter = new ServiceTypeDateFilter(this, typeof(WhsDocket), false);
			filters.AddCustomFilter(serviceTypeDateCompletedFilter);
		}

		#endregion

		#region AddConsigneeCountryFilter

		void AddConsigneeCountryFilter(ModuleFilterCollection filters)
		{
			var consigneeCountryFilter = filters.AddNkFilter("Consignee Country / Region", GetConsigneeCountryQuery, ModuleIDs.RefCountry, Countries);
			consigneeCountryFilter.MultilingualDescription = ResString.GetMultilingualString("aeaaa034-5985-46cf-b7a9-f069d6ea8935", "Consignee Country / Region");
			consigneeCountryFilter.Category = FilterCategories.Organisations;
			consigneeCountryFilter.MaxLength = OrgAddressSchema.OA_RN_NKCountryCode.MaxLength;
		}

		RefCountryCollection Countries => countries ??= new RefCountryCollection(Factory);
		RefCountryCollection countries;

		#region GetConsigneeCountryQuery

		protected ZQuery GetConsigneeCountryQuery(SQLComparisonOperator comparisonOperator, ZString country)
		{
			return GetAddressQuery(comparisonOperator, OrgAddressSchema.OA_RN_NKCountryCode, JobDocAddressSchema.E2_RN_NKCountryCode, country, DocAddressType.ConsigneeAddress);
		}

		#endregion

		#endregion

		#region AddHeldInventoryFilter

		void AddHeldInventoryFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddFlagsFilter(IsOrderForHeldInventoryFilterName, new string[] { Res.GetString("ddb5dda1-5430-41a8-abed-38c13fc0c1e5", "Is Order For Held Inventory") }, new GetFlagsQuery[] { GetHeldInventoryQuery });
			filter.MultilingualDescription = ResString.GetMultilingualString("ddb5dda1-5430-41a8-abed-38c13fc0c1e5", IsOrderForHeldInventoryFilterName);
		}

		ZQuery GetHeldInventoryQuery(ZBool value)
		{
			var docketQuery = new ZDBOnlyQuery(typeof(WhsDocket));

			var docketLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD, notIn: !value);
			docketLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_WHC_NKOrderedHeldCode, SQLComparisonOperator.NotEqual, null);
			docketQuery.AddSubQuery(docketLineSubQuery, JoinCondition.And);

			return docketQuery;
		}

		#endregion

		void AddOthersFilters(ModuleFilterCollection filters)
		{
			AddConsigneeCountryFilter(filters);
			var serviceFilter = filters.AddTextFilter("Carrier Service Level", WhsDocketSchema.WD_PL_NKCarrierServiceLevel, GetCarrierServiceLevels);
			serviceFilter.MultilingualDescription = ResString.GetMultilingualString("77bf8a85-aa4c-4677-9fa8-5b27df6d9384", "Carrier Service Level");
			serviceFilter.IsPublishedOnWeb = false;
			serviceFilter.Category = FilterCategories.Other;

			var filterNumber = filters.AddNumberRangeFilter("Number of Order Lines", GetOrdersBetweenRange);
			filterNumber.PropertyType = ZCalcEditPropertyType.Int;
			filterNumber.MultilingualDescription = ResString.GetMultilingualString("6c080921-7743-4506-a703-9496b82a6df3", "Number of Order Lines");
			filterNumber.Category = FilterCategories.Other;

			var filterFlag = filters.AddFlagsFilter("Single Item Orders", new string[] { Res.GetString("fc2fd327-f270-4d8b-8c76-a44bf38538f0", "Single Item Orders") },
				new GetFlagsQuery[] { GetOrdersWithSingleLine });
			filterFlag.MultilingualDescription = ResString.GetMultilingualString("da29864a-da73-4aa7-a996-75daa56a825c", "Single Item Orders");
			filterFlag.Category = FilterCategories.Other;

			var pickGroupfilter = filters.AddTextFilter("Pick Group", GetPickGroup, PickGroups);
			pickGroupfilter.MultilingualDescription = ResString.GetMultilingualString("abb08693-6a97-47a5-945f-7456c6ce6f66", "Pick Group");
			pickGroupfilter.Category = FilterCategories.Other;
			pickGroupfilter.MaxLength = 2;                      // WhsDocketLineSchema.WE_PickGroup.MaxLength == -1, but cdctableconfig.xml sets MaxLength for smallints to 2.

			var unitRangeTextFilter = GetNewNumberRangeByUnitFilter((NoResString)"Order Weight", GetWeightUnitRangeQuery, WeightUnits); // Filter Title
			unitRangeTextFilter.MultilingualDescription = ResString.GetMultilingualString("4bf3b1fa-a463-42bd-acb7-a938fc1e7620", "Order Weight");
			unitRangeTextFilter.Category = FilterCategories.Other;
			unitRangeTextFilter.Property = Env.Registry.PackageWeightUnit;
			filters.AddCustomFilter(unitRangeTextFilter);

			unitRangeTextFilter = GetNewNumberRangeByUnitFilter((NoResString)"Order Volume", GetVolumeUnitRangeQuery, VolumeUnits); // Filter Title
			unitRangeTextFilter.MultilingualDescription = ResString.GetMultilingualString("78c32a6d-135f-4cc7-b338-a9890bb2a69a", "Order Volume");
			unitRangeTextFilter.Category = FilterCategories.Other;
			unitRangeTextFilter.Property = Env.Registry.PackageVolumeUnit;
			filters.AddCustomFilter(unitRangeTextFilter);

			var productCountNumberRangeFilter = filters.AddNumberRangeFilter("Product Count", GetOrdersWithProductCountsInRange); // Filter Title
			productCountNumberRangeFilter.PropertyType = ZCalcEditPropertyType.Int;
			productCountNumberRangeFilter.MultilingualDescription = ResString.GetMultilingualString("39e9a13f-2520-40c8-9fb7-9eab4cca8b75", "Number of Products");
			productCountNumberRangeFilter.Category = FilterCategories.Other;

			var salesChannelGuidFilter = filters.AddGuidFilter("Sales Channel", ModuleIDs.WhsSalesChannel, WhsDocketSchema.WD_WSH_SalesChannel, new WhsSalesChannelCollection(Factory));
			salesChannelGuidFilter.MultilingualDescription = ResString.GetMultilingualString("7ba7137a-36d2-4738-a9d1-df47932a9eb6", "Sales Channel");
			salesChannelGuidFilter.Category = FilterCategories.Other;
			salesChannelGuidFilter.IsPublishedOnWeb = false;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var pickSlipReceiveRefFilter = filters.AddTextFilter("Receive Reference (Pick Slip)", GetReceiveReferencePickSlipQuery);
			pickSlipReceiveRefFilter.MaxLength = WhsDocketSchema.WD_ExternalReference.MaxLength;
			pickSlipReceiveRefFilter.MultilingualDescription = ResString.GetMultilingualString("7f5add96-4c9f-4c9c-90b0-2057c9c3cd8e", "Receive Reference (Pick Slip)");
			pickSlipReceiveRefFilter.Category = FilterCategories.NumbersAndReferences;
			pickSlipReceiveRefFilter.MaxLength = WhsDocketSchema.WD_ExternalReference.MaxLength;

			var crossDockReceiveRefFilter = filters.AddTextFilter("Receive Reference (Cross Dock)", GetReceiveReferenceCrossDockQuery);
			crossDockReceiveRefFilter.MaxLength = WhsDocketSchema.WD_ExternalReference.MaxLength;
			crossDockReceiveRefFilter.MultilingualDescription = ResString.GetMultilingualString("5dd69d50-a78e-4d02-880a-bea92e66544c", "Receive Reference (Cross Dock)");
			crossDockReceiveRefFilter.Category = FilterCategories.NumbersAndReferences;
			crossDockReceiveRefFilter.MaxLength = WhsDocketSchema.WD_ExternalReference.MaxLength;

			var trolleyNumberfilter = filters.AddTextFilter(Schema.TrolleyNumber, GetTrolleyNumberQuery);
			trolleyNumberfilter.MultilingualDescription = ResString.GetMultilingualString("4AA83ABA-7970-4BFD-8651-3426A43B6F91", "Trolley Number");
			trolleyNumberfilter.Category = FilterCategories.NumbersAndReferences;
			trolleyNumberfilter.MaxLength = RefEquipmentSchema.RQ_Registration.MaxLength;

			var consigneeCompanyFilter = filters.AddTextFilter("Consignee Company Name", GetConsigneeCompanyNameQuery);
			consigneeCompanyFilter.MultilingualDescription = ResString.GetMultilingualString("6a04dfd8-3adc-4684-91fd-e4134cc02b70", "Consignee Company Name");
			consigneeCompanyFilter.Category = FilterCategories.Organisations;
			consigneeCompanyFilter.MaxLength = Math.Min(OrgHeaderSchema.OH_FullName.MaxLength, JobDocAddressSchema.E2_CompanyName.MaxLength);

			var consigneeStateFilter = filters.AddTextFilter("Consignee State", GetConsigneeCompanyStateQuery);
			consigneeStateFilter.MultilingualDescription = ResString.GetMultilingualString("2a6b7a4b-485c-4bbd-bbab-83b77839550a", "Consignee State");
			consigneeStateFilter.Category = FilterCategories.Organisations;
			consigneeStateFilter.MaxLength = OrgAddressSchema.OA_State.MaxLength;

			var hasDangerousGoodsStatusResourceString = ResString.GetMultilingualString("1937e607-2b52-4c3e-a1eb-c0b200024652", "Has Dangerous Goods");
			var hasDangerousGoodsStatusFilter = filters.AddFlagsFilter(Schema.HasDangerousGoods, new string[] { hasDangerousGoodsStatusResourceString }, new GetFlagsQuery[] { GetOrdersWithDangerousGoodsQuery });
			hasDangerousGoodsStatusFilter.MultilingualDescription = hasDangerousGoodsStatusResourceString;
			hasDangerousGoodsStatusFilter.Category = FilterCategories.StatusAndFlags;

			var loadIdfilter = filters.AddTextFilter(Schema.LoadID, GetLoadIDQuery);
			loadIdfilter.MultilingualDescription = ResString.GetMultilingualString("FB9C1DE3-1BFE-4246-8802-A53892BADB48", "Load ID");
			loadIdfilter.Category = FilterCategories.TextSearch;
			loadIdfilter.MaxLength = WhsLoadSchema.WLO_JobID.MaxLength;

			AddFinalisedStatusFilter(filters);
		}

		protected override bool AddPackageIdFilter => true;

		protected override bool AddHandlingUnitFilter => true;

		protected override bool AddPackageTypeFilter => true;

		#region GetNewNumberRangeByUnitFilter

		protected virtual NumberRangeByUnitFilter GetNewNumberRangeByUnitFilter(string description, GetUnitRangeQuery queryDelegate, IList unitList)
		{
			return new NumberRangeByUnitFilter(description, queryDelegate, unitList);
		}

		#endregion

		#region GetConsigneeCompanyStateQuery

		protected ZQuery GetConsigneeCompanyStateQuery(SQLComparisonOperator comparisonOperator, ZString state)
		{
			return GetAddressQuery(comparisonOperator, OrgAddressSchema.OA_State, JobDocAddressSchema.E2_State, state, DocAddressType.ConsigneeAddress);
		}

		#endregion

		#region Docket Status Filter

		protected override ZQuery GetStatusQueryCore(ZString value)
		{
			var docketStatusByOrderStatus = GetDocketStatusByOrderStatus();
			var docketStatuses = docketStatusByOrderStatus.TryGetValue(value, out var result) && result.Length > 0 ? result : new[] { value.ToString() };

			var query = new ZDBOnlyQuery(typeof(WhsDocket));
			query.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order);
			query.AddToFilter(WhsDocketSchema.WD_DocketStatus, docketStatuses);

			var orderStatusSubQuery = new ZDBOnlySubQuery(typeof(WhsOrderStatusView), WhsOrderStatusViewSchema.PK);
			orderStatusSubQuery.AddToFilter(WhsOrderStatusViewSchema.WOS_OrderStatus, value);
			orderStatusSubQuery.AddToFilter(WhsOrderStatusViewSchema.WOS_DocketStatus, docketStatuses);

			query.AddSubQuery(WhsDocketSchema.PK, orderStatusSubQuery, JoinCondition.And);

			return query;
		}

		protected override CodeDescriptionPairList GetDocketStatusCore()
		{
			var statuses = WhsOrderHelper.OrderStatuses;
			statuses.RemoveAt(statuses.IndexOfCode(DocketStatus.Codes.New));
			return statuses;
		}

		Dictionary<string, string[]> GetDocketStatusByOrderStatus()
		{
			var pickingStatus = new[] { DocketStatus.Codes.Picking };
			return new Dictionary<string, string[]>
			{
				{ DocketStatus.Codes.AttachedToPick, Array.Empty<string>() },
				{ DocketStatus.Codes.Cancelled, Array.Empty<string>() },
				{ DocketStatus.Codes.Entered, Array.Empty<string>() },
				{ DocketStatus.Codes.Error, Array.Empty<string>() },
				{ DocketStatus.Codes.Held, Array.Empty<string>() },
				{ DocketStatus.Codes.Picking, Array.Empty<string>() },
				{ WhsOrderStatus.Codes.Loading, pickingStatus },
				{ WhsOrderStatus.Codes.Loaded, pickingStatus },
				{ WhsOrderStatus.Codes.ReadyToPack, pickingStatus },
				{ WhsOrderStatus.Codes.Staged, pickingStatus },
				{ WhsOrderStatus.Codes.Departed, new[] { DocketStatus.Codes.Picking, WhsOrderStatus.Codes.Departed } },
			};
		}

		#endregion

		#region Transport Zone Filter

		void AddTransportZoneFilter(ModuleFilterCollection filters)
		{
			var transportZoneFilter = filters.AddGuidFilter(Schema.TransportZone, ModuleIDs.RateTransportZone, WhsDocketSchema.WD_TZ_TransportZone, TransportZones);
			transportZoneFilter.MultilingualDescription = ResString.GetMultilingualString("cfb360a5-12de-4dea-a5de-1a1c4cd6e025", "Transport Zone");
			transportZoneFilter.IsPublishedOnWeb = false;
		}

		#endregion

		#region Consignee Filter

		protected override void AddConsigneeFilter(ModuleFilterCollection filters)
		{
			var consigneeFilter = new ModuleGuidFilterForOrg((NoResString)"Consignee", ModuleIDs.Organisation, (comparisonOperator, value)
			  => GetDocAddressQuery(value is ZGuid guid ? guid : ZGuid.Empty, DocAddressTypes.Codes.ConsigneeAddress, comparisonOperator), FilterConsignees);
			consigneeFilter.MultilingualDescription = ResString.GetMultilingualString("42daa3bc-d36c-448b-b1e2-eb047b63486b", "Consignee");

			consigneeFilter.SupportsBlankComparisonOperators = false;

			filters.AddFilter(consigneeFilter);
		}

		#endregion

		#region TransportZones

		public RateTransportZonesCollection TransportZones => new RateTransportZonesCollection(Factory);

		#endregion

		#region Outbound Location

		void AddOutboundLocationFilter(ModuleFilterCollection filters)
		{
			var outboundLocationFilter = filters.AddGuidFilter(Schema.OutboundLocation, ModuleIDs.WhsConfigLocation, GetOutboundLocationQuery, GetLocations, WhsOutboundLocationViewSchema.WOU_WD_Order);
			outboundLocationFilter.IsBlankFilterQueryDelegate = GetOutboundLocationIsBlankQuery;
			outboundLocationFilter.PropertyValidation = OutboundLocationFilterValidation;
			outboundLocationFilter.MultilingualDescription = ResString.GetMultilingualString("D1382C6E-BDF8-474C-8B60-4CC3EA62C6CA", "Outbound Location");
			outboundLocationFilter.IsPublishedOnWeb = false;
		}

		void OutboundLocationFilterValidation(ZPropertyInfo info)
		{
			if (!OutboundLocationFilter.Property.IsEmpty &&
				(!WarehouseFilter.IsActive || !WarehouseFilter.Property.IsValid))
			{
				info.AddError(Res.GetString("8465E5E5-1BD0-4FC4-A88A-881D58B0F38C", "Outbound Location can not be entered without a warehouse."));
			}
		}

		WhsLocationCollection GetLocations()
		{
			if (whsLocations == null)
			{
				var warehouse = WI_WW_Whs.IsValid ? Warehouse : null;
				whsLocations = warehouse == null ? new WhsLocationCollection(Factory) : new WhsLocationCollection(warehouse);
			}

			return whsLocations;
		}

		ModuleGuidFilter OutboundLocationFilter => (ModuleGuidFilter)ModuleFilters[OrderFilterBusinessObject.Schema.OutboundLocation];

		WhsLocationCollection whsLocations;

		public WhsWarehouse Warehouse => Factory.Load<WhsWarehouse>(WI_WW_Whs);

		ZGuid WI_WW_Whs => WarehouseFilter != null && WarehouseFilter.IsActive ? WarehouseFilter.Property : ZGuid.Empty;

		ZQuery GetOutboundLocationIsBlankQuery()
		{
			var orderStatusSubQuery = new ZDBOnlySubQuery(typeof(WhsOrderStatusView), WhsOrderStatusViewSchema.WOS_WD_Docket);

			var statusCodes = new[] {
				WhsOrderStatus.Codes.Loading,
				WhsOrderStatus.Codes.Loaded,
				WhsOrderStatus.Codes.ReadyToPack,
				WhsOrderStatus.Codes.Staged
			};

			orderStatusSubQuery.AddToFilter(WhsOrderStatusViewSchema.WOS_OrderStatus, SQLComparisonOperator.NotEqual, statusCodes);

			var orderQuery = new ZDBOnlyQuery(typeof(WhsDocket));
			orderQuery.AddSubQuery(orderStatusSubQuery, JoinCondition.And);

			return orderQuery;
		}

		ZQuery GetOutboundLocationQuery(ZDBOnlySubQuery filtersMatchQuery, SQLComparisonOperator comparisonOperator, object value)
		{
			var orderQuery = new ZDBOnlyQuery(typeof(WhsDocket));
			var outboundLocationViewSubQuery = new ZDBOnlySubQuery(typeof(WhsOutboundLocationView), WhsOutboundLocationViewSchema.WOU_WD_Order);

			if (comparisonOperator != SpecialComparisonOperator.IsNotBlank)
			{
				if (filtersMatchQuery != null)
				{
					outboundLocationViewSubQuery.AddSubQuery(WhsOutboundLocationViewSchema.WOU_WL_OutboundLocation, WhsLocationViewSchema.PK, filtersMatchQuery, JoinCondition.And);
				}
				else
				{
					outboundLocationViewSubQuery.AddToFilter(WhsOutboundLocationViewSchema.WOU_WL_OutboundLocation, comparisonOperator, value);
				}
			}
			orderQuery.AddSubQuery(outboundLocationViewSubQuery, JoinCondition.And);

			return orderQuery;
		}
		#endregion

		#region SupportsDistributionCentreFilters

		protected override bool SupportsDistributionCentreFilters => true;

		#endregion

		#endregion

		#region Lookups

		#region PickGroups

		ReadOnlyCodeDescriptionPairList PickGroups
		{
			get
			{
				var pickGroupCollection = Factory.GetCachedValue("OrderFilterBusinessObject|PickGroups", () => WarehouseDataRegistry.Instance.PickGroups.Value);

				var codeDescriptionPairList = new CodeDescriptionPairList();
				foreach (PickGroup pickGroup in pickGroupCollection)
				{
					codeDescriptionPairList.Add(new CodeDescriptionPair(pickGroup.Code, pickGroup.Description));
				}

				return codeDescriptionPairList;
			}
		}

		#endregion

		#region Units

		ReadOnlyCodeDescriptionPairList WeightUnits
		{
			get
			{
				var weightUnitollection = Factory.GetCachedValue("OrderFilterBusinessObject|WeightUnits", () => new CodeDescriptionPairList(OLookUpEditType.Weight));
				return weightUnitollection;
			}
		}

		ReadOnlyCodeDescriptionPairList VolumeUnits
		{
			get
			{
				var volumeUnitollection = Factory.GetCachedValue("OrderFilterBusinessObject|VolumeUnits", () => new CodeDescriptionPairList(OLookUpEditType.Volume));
				return volumeUnitollection;
			}
		}

		#endregion

		#region CarrierBookingAgents

		public OrgHeaderCollection CarrierBookingAgents => new OrgHeaderCollection(Factory);

		#endregion

		#region AddFinalisedStatusFilter

		void AddFinalisedStatusFilter(ModuleFilterCollection filters)
		{
			var finalisedStatusFilter = filters.AddTextFilter(Schema.FinalizedStatus, GetFinalisedStatusQuery, new FinalisedStatus());
			finalisedStatusFilter.MultilingualDescription = ResString.GetMultilingualString("cef63e45-6c71-4195-8e34-0ffa1156b8eb", "Finalized Status");
			finalisedStatusFilter.Category = FilterCategories.StatusAndFlags;
		}

		ZQuery GetFinalisedStatusQuery(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(WhsDocket));
			query.AddToFilter(WhsDocketSchema.WD_DocketType, SQLComparisonOperator.Equal, DocketType.Codes.Order);
			switch (value)
			{
				case FinalisedStatus.Codes.IsFinalised:
					query.AddToFilter(WhsDocketSchema.WD_FinalisedDate, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
					break;
				case FinalisedStatus.Codes.IsNotFinalised:
					query.AddToFilter(WhsDocketSchema.WD_FinalisedDate, SQLComparisonOperator.Equal, ZDateTime.Empty);
					query.AddToFilter(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Cancelled);
					break;
				default:
					break;
			}
			return query;
		}

		#endregion

		protected override CodeDescriptionPairList OrderTypes
		{
			get { return new OrderType(); }
		}

		#endregion

		#region Queries

		#region GetWeightUnitRangeQuery

		ZQuery GetWeightUnitRangeQuery(INumericZType value1, INumericZType value2, ZString unit)
		{
			return GetVolumeOrWeightUnitRangeQuery(value1, value2, unit, false);
		}

		#endregion

		#region GetVolumeUnitRangeQuery

		ZQuery GetVolumeUnitRangeQuery(INumericZType value1, INumericZType value2, ZString unit)
		{
			return GetVolumeOrWeightUnitRangeQuery(value1, value2, unit, true);
		}

		#endregion

		#region GetVolumeOrWeightUnitRangeQuery

		ZQuery GetVolumeOrWeightUnitRangeQuery(INumericZType value1, INumericZType value2, ZString unit, bool isVolume)
		{
			var result = new ZDBOnlyQuery(typeof(WhsDocket));
			var totalUnitColumnName = isVolume ? WhsDocketSchema.WD_TotalCubicUnit.Name : WhsDocketSchema.WD_TotalWeightUnit.Name;
			var totalColumnName = isVolume ? WhsDocketSchema.WD_TotalCubic.Name : WhsDocketSchema.WD_TotalWeight.Name;
			var dimentionType = isVolume ? "ConvertVolume" : "ConvertWeight";

			var sqlFilter = string.Format(CultureInfo.InvariantCulture, @" 
						{1} IN
						(
							SELECT
								{1}
							FROM 
								{0}
								CROSS APPLY dbo.{9}({2}, {3}, '{5}') AS ConvertedTotal
							WHERE 
								{4} = '{8}'
								AND ConvertedTotal.Value BETWEEN {6} AND {7}							
						)
						AND {4} = '{8}'
						", WhsDocketSchema.Constants.TableName, WhsDocketSchema.PK.Name, totalColumnName, totalUnitColumnName, // SQL
						 WhsDocketSchema.WD_DocketType.Name, unit, value1.ToString(), value2.ToString(), DocketType.Codes.Order, dimentionType);

			var sqlFilterParameters = new ZSqlParameterCollection();
			result.AddFilterAndZSQLParameterCollection(sqlFilter, sqlFilterParameters);
			return result;
		}

		#endregion

		#region GetOrdersBetweenRange

		ZQuery GetOrdersBetweenRange(INumericZType value1, INumericZType value2)
		{
			var result = new ZDBOnlyQuery(typeof(WhsDocket));
			var sqlFilter = string.Format(CultureInfo.InvariantCulture, @"
						{2} IN
						(
							SELECT
								{2}
							FROM 
								{0}
								LEFT JOIN {1} ON {2} = {3}
							WHERE 
								{4} = '{7}'
							GROUP BY 
								{2}
							HAVING
								COUNT({3}) Between {5} AND {6}
						)
						AND {4} = '{7}'
						", WhsDocketSchema.Constants.TableName, WhsDocketLineSchema.Constants.TableName, WhsDocketSchema.PK.Name,
						 WhsDocketLineSchema.WE_WD.Name, WhsDocketSchema.WD_DocketType.Name, value1.ToString(), value2.ToString(), DocketType.Codes.Order);

			var sqlFilterParameters = new ZSqlParameterCollection();
			result.AddFilterAndZSQLParameterCollection(sqlFilter, sqlFilterParameters);
			return result;
		}

		#endregion

		#region GetOrdersWithProductCountsInRange

		ZQuery GetOrdersWithProductCountsInRange(INumericZType value1, INumericZType value2)
		{
			var result = new ZDBOnlyQuery(typeof(WhsDocket));
			var sqlFilter = FormattableString.Invariant($@"
				{WhsDocketSchema.PK.Name} IN
				(
					SELECT
						{WhsDocketSchema.PK.Name}
					FROM 
						{WhsDocketLineSchema.Constants.SqlSchemaName}.{WhsDocketLineSchema.Constants.TableName}
					WHERE 
						{WhsDocketSchema.PK.Name} = {WhsDocketLineSchema.WE_WD.Name}
					GROUP BY 
						{WhsDocketLineSchema.WE_WD.Name}
					HAVING
						COUNT(DISTINCT {WhsDocketLineSchema.WE_OP.Name}) BETWEEN {value1} AND {value2}
				)
				AND {WhsDocketSchema.WD_DocketType.Name} = '{DocketType.Codes.Order}'");

			var sqlFilterParameters = new ZSqlParameterCollection();
			result.AddFilterAndZSQLParameterCollection(sqlFilter, sqlFilterParameters);

			return result;
		}

		#endregion

		#region GetOrdersWithSingleLine

		ZQuery GetOrdersWithSingleLine(ZBool value)
		{
			var result = new ZDBOnlyQuery(typeof(WhsDocket));
			var notIn = value ? " IN " : (NoResString)" NOT IN ";    // SQL

			var sqlFilter = string.Format(CultureInfo.InvariantCulture, @"
						{3} {0}
						(
							SELECT
								{3}
							FROM 
								{1}
								LEFT JOIN {2} ON {3} = {4}
							WHERE 
								{5} = '{7}'
							GROUP BY 
								{3}
							HAVING
								COUNT({4}) = 1
								AND SUM({6}) = 1
						)
						AND {5} = '{7}'
						", notIn, WhsDocketSchema.Constants.TableName, WhsDocketLineSchema.Constants.TableName, WhsDocketSchema.PK.Name,
						 WhsDocketLineSchema.WE_WD.Name, WhsDocketSchema.WD_DocketType.Name, WhsDocketLineSchema.WE_TransactionQuantity.Name, DocketType.Codes.Order);

			var sqlFilterParameters = new ZSqlParameterCollection();
			result.AddFilterAndZSQLParameterCollection(sqlFilter, sqlFilterParameters);
			return result;
		}

		#endregion

		#region GetPickGroup

		ZQuery GetPickGroup(ZString value)
		{
			var docketResult = new ZDBOnlyQuery(typeof(WhsDocket));
			var docketLine = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
			docketLine.AddToFilter(WhsDocketLineSchema.WE_PickGroup, ZShort.Parse(value.ToString()));
			docketResult.AddSubQuery(docketLine, JoinCondition.And);

			var result = new ZQuery();
			result.AddToFilter(docketResult);

			return result;
		}

		#endregion

		#region GetLoadIdQuery

		// tested in OrderFilterBusinessObject
		ZQuery GetLoadIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var isBlank = comparisonOperator == SpecialComparisonOperator.IsBlank;
			var query = new ZDBOnlyQuery(typeof(WhsDocket));
			var loadSubQuery = new ZDBOnlySubQuery(typeof(WhsLoad), WhsLoadSchema.PK, WhsLoadOrderSchema.WOV_WLO_Load);
			if (!isBlank)
			{
				loadSubQuery.AddToFilter(WhsLoadSchema.WLO_JobID, comparisonOperator, value);
			}
			var loadOrderQuery = new ZDBOnlySubQuery(typeof(WhsLoadOrder), WhsLoadOrderSchema.WOV_WD_Docket, isBlank);
			loadOrderQuery.AddSubQuery(loadSubQuery, JoinCondition.And);

			var orderQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.PK);
			orderQuery.AddSubQuery(loadOrderQuery, JoinCondition.And);
			query.AddSubQuery(orderQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region GetReceiveReferenceCrossDockQuery

		ZQuery GetReceiveReferenceCrossDockQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var isBlank = comparisonOperator == SpecialComparisonOperator.IsBlank;
			var query = new ZDBOnlyQuery(typeof(WhsDocket));

			var pickLineQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			pickLineQuery.AddToFilter(WhsPickLineSchema.WZ_OriginalReservedQty, SQLComparisonOperator.GreaterThan, 0m);

			if (!isBlank)
			{
				var receiptQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketLineSchema.WE_WD);
				receiptQuery.AddToFilter(WhsDocketSchema.WD_ExternalReference, comparisonOperator, value);

				var inventoryLineQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsPickLineSchema.WZ_WE_InventoryLine);
				inventoryLineQuery.AddSubQuery(receiptQuery, JoinCondition.And);

				pickLineQuery.AddSubQuery(inventoryLineQuery, JoinCondition.And);

				var orderLineQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
				orderLineQuery.AddSubQuery(pickLineQuery, JoinCondition.And);

				query.AddToFilter(WhsDocketSchema.WD_WP, null);
				query.AddSubQuery(orderLineQuery, JoinCondition.And);
			}
			else
			{
				var orderLineQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD, isBlank);
				orderLineQuery.AddSubQuery(pickLineQuery, JoinCondition.And);

				var orQuery = new ZDBOnlyQuery(typeof(WhsDocket));
				orQuery.AddToFilter(WhsDocketSchema.WD_WP, null);
				orQuery.AddSubQuery(orderLineQuery, JoinCondition.And);

				query.AddToFilter(WhsDocketSchema.WD_WP, SQLComparisonOperator.NotEqual, null);
				query.AddToFilter(orQuery, JoinCondition.Or);
			}

			return query;
		}

		#endregion

		#region GetReceiveReferencePickSlipQuery

		ZQuery GetReceiveReferencePickSlipQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var receiptQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketLineSchema.WE_WD);
			receiptQuery.AddToFilter(WhsDocketSchema.WD_ExternalReference, comparisonOperator, value);

			var pickLineQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			pickLineQuery.AddToFilter(WhsPickLineSchema.WZ_Units, SQLComparisonOperator.GreaterThan, 0m);
			pickLineQuery.AddToFilter(GetPickedFromSourceLocationQuery(receiptQuery));

			var orderLineQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
			orderLineQuery.AddSubQuery(pickLineQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(WhsDocket));
			// picklines used for reserving stock should not be considered for filtering receive reference for pick slip
			query.AddToFilter(WhsDocketSchema.WD_WP, SQLComparisonOperator.NotEqual, null);
			query.AddSubQuery(orderLineQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region JobInvoicingSecurity

		protected override Security.SecurityCheckpoint JobInvoicingSecurity
		{
			get { return Env.Security.WhsOrderJobInvoicing; }
		}

		#endregion

		#region GetOrdersWithDangerousGoods

		ZQuery GetOrdersWithDangerousGoodsQuery(ZBool value)
		{
			var docketResult = new ZDBOnlyQuery(typeof(WhsDocket));
			var docketLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD, !value);
			var orgSupplierSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), WhsDocketLineSchema.WE_OP);
			var undgSubQuery = new ZDBOnlySubQuery(typeof(UNDGDataItem), UNDGDataItemSchema.DI_ParentID);

			orgSupplierSubQuery.AddSubQuery(undgSubQuery, JoinCondition.And);
			docketLineSubQuery.AddSubQuery(orgSupplierSubQuery, JoinCondition.And);
			docketResult.AddSubQuery(docketLineSubQuery, JoinCondition.And);

			return docketResult;
		}

		#endregion

		#region AuditStatuses

		OrderAuditStatus AuditStatuses => new OrderAuditStatus();

		#endregion

		#endregion

		readonly OrderCRMSecurityProvider SecurityProvider = new OrderCRMSecurityProvider();

		#region Filter Strings

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter ID used only in code")]
		public const string AuditStatusFilterName = "Audit Status";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter ID used only in code")]
		public const string HasHeldPackagesFilterName = "Has Held Packages";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter ID used only in code")]
		public const string IsOrderForHeldInventoryFilterName = "Is Order For Held Inventory";

		MultilingualString AuditStatusFilterDescription
			=> ResString.GetMultilingualString("ccb628e8-3aa4-4cca-8640-959c11a6bea7", "Audit Status");

		MultilingualString HasHeldPackagesFilterDescription
			=> ResString.GetMultilingualString("035a00d9-6eed-4231-8c52-c01be5362904", "Has Held Packages");

		#endregion

		#region IsAddServiceLevelFilter

		protected override bool IsAddServiceLevelFilter => true;

		protected override void OnWarehouseChanged(object sender, EventArgs e)
		{
			OnWarehouseChangeUpdateLocationCache();
		}

		void OnWarehouseChangeUpdateLocationCache()
		{
			var outboundLocationFilter = OutboundLocationFilter;
			if (outboundLocationFilter.Property.IsValid)
			{
				var location = Factory.Load<WhsLocation>(outboundLocationFilter.Property);
				if (location == null || WD_WW_Whs != location.WLV_WW_Whs)
				{
					ClearLocationCache();
				}
				else
				{
					outboundLocationFilter.Validation.ValidateProperty();
				}
			}
			else
			{
				ClearLocationCache();
			}

			void ClearLocationCache()
			{
				outboundLocationFilter.Property = ZGuid.Empty;
				whsLocations = null;
			}
		}

		#endregion
	}
}
