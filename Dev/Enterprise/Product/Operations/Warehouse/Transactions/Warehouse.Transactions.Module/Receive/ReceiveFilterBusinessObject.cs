using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ReceiveFilterBusinessObject : ExtendedDocketFilterBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			filters.AddDateFilter("Booking Date", WhsDocketSchema.WD_BookingDate).MultilingualDescription = ResString.GetMultilingualString("c5208153-1b81-4f66-a0e0-3bd06b27ae28", "Booking Date");
			filters.AddDateFilter("Arrival Date", WhsDocketSchema.WD_ArrivalDate).MultilingualDescription = ResString.GetMultilingualString("085a7ada-91f4-4ad1-873e-c164310d7706", "Arrival Date");
			filters.AddDateFilter("ETA", WhsDocketSchema.WD_ETA).MultilingualDescription = ResString.GetMultilingualString("00df2719-edc6-4f00-9cfa-1d9258521f2a", "ETA");
			filters.AddDateFilter("Unload Completed Time", WhsDocketSchema.WD_UnloadCompletedTime).MultilingualDescription = ResString.GetMultilingualString("d780ef71-397b-4163-85b8-9d3c108297ac", "Unload Completed Time");

			var filter = filters.AddTextFilter("Type", WhsDocketSchema.WD_DocketSubType, ReceiveTypes);
			filter.MultilingualDescription = ResString.GetMultilingualString("1157590e-0154-4f36-902d-ab8e3ef14dd6", "Type");
			filter.Category = FilterCategories.StatusAndFlags;

			filter = filters.AddTextFilter("Supplier Company Name", GetSupplierCompanyNameQuery);
			filter.MultilingualDescription = ResString.GetMultilingualString("1d031927-7653-4a16-a6e8-8f1d36308663", "Supplier Company Name");
			filter.MaxLength = Math.Min(OrgHeaderSchema.OH_FullName.MaxLength, JobDocAddressSchema.E2_CompanyName.MaxLength);
			filter.Category = FilterCategories.Organisations;

			filters.AddFlagsFilter("Not Invoiced", new string[] { Res.GetString("03238dfd-a4f1-4f27-88f6-fd7458dfdaa2", "Not Invoiced") }, new GetFlagsQuery[] { GetNotInvoicedQuery }).MultilingualDescription = ResString.GetMultilingualString("03238dfd-a4f1-4f27-88f6-fd7458dfdaa2", "Not Invoiced");
			AddReferenceFilters(filters);

			AddReceiveCategoryFilter(filters);
			AddTotalUnitsMismatchFilter(filters);
			AddASNActualVsExpectedMismatchFilter(filters);
			AddTotalPalletsMismatchFilter(filters);
			AddWorkflowFilterStripsHelper(typeof(WhsReceive), WorkflowDescriptors.WhsReceiveWorkflowDescriptorCode);
			AddProductCountFilter(filters);
			AddServiceTypeDateFilter(filters);
			AddIsHeldForPutawayFilter(filters);
			AddIsCreatedFromPickOnSalesOrderFilter(filters);

			SecurityProvider.AddCRMSecurityFilterStrips(Factory, filters);

			return filters;
		}

		protected override bool IncludeTransportCoFilter => true;

		public static class Schema
		{
			public const string IsHeldForPutaway = "Held For Putaway"; // Filter description

			public static class IsHeldForPutawayTypeCodes
			{
				public const string HeldForPutaway = "YES"; // Filter Constant
				public const string NotHeldForPutaway = "NO"; 
				public const string All = "ALL";
			}
		}

		#region IsAwaitingReplenishmentTypes

		CodeDescriptionPairList IsHeldForPutawayTypes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Schema.IsHeldForPutawayTypeCodes.All, Res.GetString("6531528F-2A8E-422F-9D12-E67A78044E75", "All"));
				result.AddPair(Schema.IsHeldForPutawayTypeCodes.HeldForPutaway, Res.GetString("160A6137-89FB-4841-A7F1-528914C263AB", "Putaway Held"));
				result.AddPair(Schema.IsHeldForPutawayTypeCodes.NotHeldForPutaway, Res.GetString("57B124D1-F991-4948-9F47-FF26BB60E919", "Putaway Not Held"));
				return result;
			}
		}

		#endregion

		#region AddIsHeldForPutawayFilter

		void AddIsHeldForPutawayFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(Schema.IsHeldForPutaway, GetHoldPutawayQuery, IsHeldForPutawayTypes);
			filter.MultilingualDescription = ResString.GetMultilingualString("8BBDF63F-6F19-4047-B2B1-949D8CD7D661", "Is Held For Putaway");
			filter.Category = FilterCategories.StatusAndFlags;
		}

		#endregion

		#region GetHoldPutawayQuery

		ZQuery GetHoldPutawayQuery(ZString value)
		{
			var query = new ZQuery();

			if (value != Schema.IsHeldForPutawayTypeCodes.All)
			{
				query.AddToFilter(WhsDocketSchema.WD_HoldPalletIDPutaway, value == Schema.IsHeldForPutawayTypeCodes.HeldForPutaway);
			}

			return query;
		}

		#endregion

		#region Docket Status Filter

		protected override CodeDescriptionPairList GetDocketStatusCore()
		{
			var status = base.GetDocketStatusCore();
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.AttachedToPick));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Picking));
			return status;
		}

		#endregion

		#region ReceiveCategoryFilter

		void AddReceiveCategoryFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("ReceiveCategories", WhsDocketSchema.WD_ReceiveCategory, ReceiveCategoryList);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("f45e854b-e65c-4442-85d5-013a9fb69fb3", "Receive Category");
		}

		ICodeDescriptionPairListWithDefaultCode ReceiveCategoryList => WarehouseDataRegistry.Instance.ReceiveCategories.Value;

		#endregion

		#region AddTotalUnitsMismatchFilter

		void AddTotalUnitsMismatchFilter(ModuleFilterCollection filters)
		{
			var mismatchedDescription = new string[] { Res.GetString("5C1D0955-DF62-4C6E-B8E5-0C0B1BF3BBF0", "Mismatched") };
			var mismatchedQuery = new GetFlagsQuery[] { GetTotalUnitsMismatchQuery };
			var flagFilter = filters.AddFlagsFilter("Total Units Mismatch", mismatchedDescription, mismatchedQuery);
			flagFilter.MultilingualDescription = ResString.GetMultilingualString("6B2BFA87-ECFB-46EE-AE94-B79DECBBF4A5", "Total Units Mismatch");
			flagFilter.Category = FilterCategories.Other;
		}

		protected ZQuery GetTotalUnitsMismatchQuery(ZBool value)
		{
			var strGroupBy = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", WhsDocketSchema.Constants.PK, WhsDocketSchema.Constants.WD_TotalUnits);  // sql clause
			var strHaving = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} != SUM(ISNULL({1}, 0)) ", WhsDocketSchema.Constants.WD_TotalUnits, WhsDocketLineSchema.Constants.WE_TransactionQuantity); // sql clause

			return GetAggregateMismatchQuery(value, strGroupBy, strHaving);
		}

		#endregion

		#region AddASNActualVsExpectedMismatchFilter

		void AddASNActualVsExpectedMismatchFilter(ModuleFilterCollection filters)
		{
			var mismatchedDescription = new string[] { Res.GetString("5C1D0955-DF62-4C6E-B8E5-0C0B1BF3BBF0", "Mismatched") };
			var mismatchedQuery = new GetFlagsQuery[] { GetASNActualVsExpectedMismatchQuery };
			var flagFilter = filters.AddFlagsFilter("ASN Actual vs. Expected Mismatch", mismatchedDescription, mismatchedQuery);
			flagFilter.MultilingualDescription = ResString.GetMultilingualString("1CD80443-9D85-4E2F-81F8-50464A9AF0C1", "ASN Actual vs. Expected Mismatch");
			flagFilter.Category = FilterCategories.Other;
		}

		protected ZQuery GetASNActualVsExpectedMismatchQuery(ZBool value)
		{
			var strGroupBy = string.Format(CultureInfo.InvariantCulture, "{0}", WhsDocketSchema.Constants.PK);  // sql clause
			var strHaving = string.Format(CultureInfo.InvariantCulture, (NoResString)"SUM( {0} ) != SUM( {1} )", WhsDocketLineSchema.Constants.WE_ClientOrderedUnits, WhsDocketLineSchema.Constants.WE_TransactionQuantity); // sql clause

			return GetAggregateMismatchQuery(value, strGroupBy, strHaving);
		}

		#endregion

		#region AddTotalPalletsMismatchFilter

		void AddTotalPalletsMismatchFilter(ModuleFilterCollection filters)
		{
			var mismatchedDescription = new string[] { Res.GetString("eec8bdf0-72cb-488a-9f76-d58aac9f9b93", "Mismatched") };
			var mismatchedQuery = new GetFlagsQuery[] { GetTotalPalletsMismatchQuery };
			var flagFilter = filters.AddFlagsFilter("Total Pallets Mismatch", mismatchedDescription, mismatchedQuery);
			flagFilter.MultilingualDescription = ResString.GetMultilingualString("00ca87ed-d96c-4cde-9518-13a7bf0a05be", "Total Pallets Mismatch");
			flagFilter.Category = FilterCategories.Other;
		}

		protected ZQuery GetTotalPalletsMismatchQuery(ZBool value)
		{
			var strGroupBy = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", WhsDocketSchema.Constants.PK, WhsDocketSchema.Constants.WD_TotalPallets);  // sql clause
			var strHaving = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} != COUNT(DISTINCT CASE WHEN {1} <> '' THEN {1} ELSE null END) ", WhsDocketSchema.Constants.WD_TotalPallets, WhsDocketLineSchema.Constants.WE_PalletID); // sql clause

			return GetAggregateMismatchQuery(value, strGroupBy, strHaving);
		}

		ZQuery GetAggregateMismatchQuery(ZBool value, ZString strGroupBy, ZString strHaving)
		{
			var result = new ZDBOnlyQuery(typeof(WhsDocket));
			var notIn = value ? " IN " : (NoResString)" NOT IN ";    // sql operator
			var sqlFilter = string.Format(CultureInfo.InvariantCulture, @"
				WD_PK {0}
						(
							SELECT
								WD_PK
							FROM 
								dbo.WhsDocket
								LEFT JOIN dbo.WhsDocketLine ON WD_PK = WE_WD
							WHERE 
								WD_DocketType = 'INW' 
							GROUP BY 
								{1}
							HAVING
								{2}
						)
				", notIn, strGroupBy, strHaving);

			var sqlFilterParameters = new ZSqlParameterCollection();
			result.AddFilterAndZSQLParameterCollection(sqlFilter, sqlFilterParameters);

			return result;
		}

		#endregion

		#region AddIsCreatedFromPickOnSalesOrderFilter

		void AddIsCreatedFromPickOnSalesOrderFilter(ModuleFilterCollection filters)
		{
			var description = new string[] { Res.GetString("0d1a3ad2-a16c-4ee6-8e61-d47ff782272f", "Is Created From Pick On Sales Order") };
			var query = new GetFlagsQuery[] { GetIsCreatedFromPickOnSalesOrderQuery };
			var flagFilter = filters.AddFlagsFilter("Is Created From Pick On Sales Order", description, query);
			flagFilter.MultilingualDescription = ResString.GetMultilingualString("362a560e-7fca-4413-a2d6-01498d35b7be", "Is Created From Pick On Sales Order");
			flagFilter.Category = FilterCategories.StatusAndFlags;
			flagFilter.Visibility = FilterVisibility.AlwaysApplied;
		}

		ZQuery GetIsCreatedFromPickOnSalesOrderQuery(ZBool isCreatedFromPickOnSalesOrder)
			=> new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, isCreatedFromPickOnSalesOrder ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal, null);

		#endregion

		protected override IList<FilterGroupMember> ReferenceFields
		{
			get
			{
				var result = base.ReferenceFields;
				result.Add(new FilterGroupMember(ResString.GetMultilingualString("FB1E8356-CC39-41A8-9267-28ACAADA5DCF", "Receive Reference"), WhsDocketSchema.WD_ExternalReference));

				return result;
			}
		}

		#endregion

		#region Lookups

		public ReceiveType ReceiveTypes => new ReceiveType();

		#endregion

		#region JobInvoicingSecurity

		protected override SecurityCheckpoint JobInvoicingSecurity => Env.Security.WhsReceiveJobInvoicing;

		#endregion

		#region IsAddServiceLevelFilter

		protected override bool IsAddServiceLevelFilter => true;

		#endregion

		#region SupportsDocketPlanningStatus

		protected override bool SupportsDocketPlanningStatus => true;

		#endregion

		#region AddProductCountFilter

		void AddProductCountFilter(ModuleFilterCollection filters)
		{
			var productCountNumberRangeFilter = filters.AddNumberRangeFilter("Product Count", GetReceivesWithProductCountsInRange); // Filter Title
			productCountNumberRangeFilter.PropertyType = ZCalcEditPropertyType.Int;
			productCountNumberRangeFilter.MultilingualDescription = ResString.GetMultilingualString("39e9a13f-2520-40c8-9fb7-9eab4cca8b75", "Number of Products");
			productCountNumberRangeFilter.Category = FilterCategories.Other;
		}

		ZQuery GetReceivesWithProductCountsInRange(INumericZType value1, INumericZType value2)
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
				AND {WhsDocketSchema.WD_DocketType.Name} = '{DocketType.Codes.Receive}'");

			var sqlFilterParameters = new ZSqlParameterCollection();
			result.AddFilterAndZSQLParameterCollection(sqlFilter, sqlFilterParameters);

			return result;
		}

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

		readonly ReceiveCRMSecurityProvider SecurityProvider = new ReceiveCRMSecurityProvider();
	}
}
