using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class TransferFilterBusinessObject : DocketFilterBusinessObject
	{
		#region FilterConstants

		public static class FilterConstants
		{
			public const string SourceLocation = nameof(SourceLocation);
		}

		#endregion

		#region TransferTypeOptions

		public static class TransferTypeOptions
		{
			public const string InterWarehouseTransfers = "IWT"; // TransferType Option
		}

		#endregion

		#region Fields

		public WhsWarehouse Warehouse => Factory.Load<WhsWarehouse>(WD_WW_Whs);
		ModuleGuidFilter SourceLocationFilter => (ModuleGuidFilter)ModuleFilters[FilterConstants.SourceLocation];

		#endregion

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			AddDateFilters(filters);
			AddExternalReferenceFilter(filters);
			AddTransferTypeFilter(filters);
			AddPickedByFilter(filters);
			AddPutawayByFilter(filters);
			AddSourcePalletIdFilter(filters);
			AddSourceLocationFilter(filters);

			return filters;
		}

		#region AddDateFilters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			AddBookingDateFilter(filters);
			AddRFSourceDatePicked(filters);
			AddRFDestDateTransferred(filters);
		}

		#region AddBookingDateFilter

		void AddBookingDateFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddDateFilter("Date", WhsDocketSchema.WD_BookingDate);
			filter.MultilingualDescription = ResString.GetMultilingualString("6fd920fa-d6ff-4e68-8cd7-591c9afe76c5", "Date");
		}

		#endregion

		#region AddRFSourceDatePicked

		void AddRFSourceDatePicked(ModuleFilterCollection filters)
		{
			var filter = filters.AddDateFilter("RF Source Date Picked", GetRFSourceDatePickedQuery);
			filter.MultilingualDescription = ResString.GetMultilingualString("294e6a69-3902-4e9e-a337-cae7f052a5e7", "RF Source Date Picked");
			filter.Category = FilterCategories.Dates;
		}

		#endregion

		#region AddRFDestDateTransferred

		void AddRFDestDateTransferred(ModuleFilterCollection filters)
		{
			var filter = filters.AddDateFilter("RF Dest. Date Transferred", GetRFDestDateTransferredQuery);
			filter.MultilingualDescription = ResString.GetMultilingualString("59f01f78-fd0e-4cd4-aba7-4aeca53a68c5", "RF Dest. Date Transferred");
			filter.Category = FilterCategories.Dates;
		}

		#endregion

		#endregion

		#region AddExternalReferenceFilter

		void AddExternalReferenceFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Reference", WhsDocketSchema.WD_ExternalReference);
			filter.MultilingualDescription = ResString.GetMultilingualString("22177993-dd50-4313-9218-ab4f275d5adb", "Reference");
			filter.Category = FilterCategories.NumbersAndReferences;
		}

		#endregion

		#region AddTransferTypeFilter

		void AddTransferTypeFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Transfer Type", GetSubTypeQuery, TransferTypes);
			filter.MultilingualDescription = ResString.GetMultilingualString("f7d9b9d7-0aa0-4b0f-8e50-ee69e47fef00", "Transfer Type");
			filter.Category = FilterCategories.StatusAndFlags;
		}

		protected override ZQuery GetSubTypeQuery(ZString value)
		{
			var result = new ZQuery();
			if (value == TransferTypeOptions.InterWarehouseTransfers)
			{
				result.AddToFilter(WhsDocketSchema.WD_DocketSubType, TransferType.Codes.InterWhsSource);
				result.AddToFilter(JoinCondition.Or, WhsDocketSchema.WD_DocketSubType, TransferType.Codes.InterWhsDest);
			}
			else if (value == NonPersistentTransferType.Codes.Putaway)
			{
				result.AddToFilter(WhsDocketSchema.WD_IsPutawayTransfer, true);
			}
			else if (value == NonPersistentTransferType.Codes.OutboundDockDoor)
			{
				result.AddToFilter(WhsDocketSchema.WD_WP_ParentPickForTransfer, SQLComparisonOperator.NotEqual, DBNull.Value);
			}
			else if (value == NonPersistentTransferType.Codes.AutoCreatedReplenishment)
			{
				result.AddToFilter(WhsDocketSchema.WD_IsPickFaceReplenishment, true);
			}
			else
			{
				result.AddToFilter(WhsDocketSchema.WD_DocketSubType, value);
				result.AddToFilter(WhsDocketSchema.WD_IsPutawayTransfer, false);
				result.AddToFilter(WhsDocketSchema.WD_WP_ParentPickForTransfer, DBNull.Value);
				result.AddToFilter(WhsDocketSchema.WD_IsPickFaceReplenishment, false);
			}

			return result;
		}

		#endregion

		#region OnWarehouseChanged

		protected override void OnWarehouseChanged(object sender, EventArgs e)
		{
			OnWarehouseChangeUpdateLocationCache();
		}

		#endregion

		#region AddSourceLocationFilter

		void AddSourceLocationFilter(ModuleFilterCollection filters)
		{
			var sourceLocationFilter = filters.AddGuidFilter(FilterConstants.SourceLocation, ModuleIDs.WhsConfigLocation, WhsDocketLineSchema.WE_WL_TransferFrom, GetLocations);
			sourceLocationFilter.MultilingualDescription = ResString.GetMultilingualString("6828f73d-5d19-416e-89f9-f7136db2e581", "Source Location");
			sourceLocationFilter.PropertyValidation = SourceLocationFilterValidation;
			sourceLocationFilter.Category = FilterCategories.Locations;
			sourceLocationFilter.SubGroup = new DocketLineSubGroup();
			sourceLocationFilter.IsPublishedOnWeb = false;
		}

		void SourceLocationFilterValidation(ZPropertyInfo info)
		{
			if (!SourceLocationFilter.Property.IsEmpty &&
				(!WarehouseFilter.IsActive || !WarehouseFilter.Property.IsValid))
			{
				info.AddError(Res.GetString("2b9ba3e5-60c3-4728-8f06-f359e13831cf", "Source Location can not be entered without a warehouse."));
			}
		}

		WhsLocationCollection GetLocations()
		{
			if (whsLocations == null)
			{
				var warehouse = WD_WW_Whs.IsValid ? Warehouse : null;
				whsLocations = warehouse == null ? new WhsLocationCollection(Factory) : new WhsLocationCollection(warehouse);
			}

			return whsLocations;
		}

		void OnWarehouseChangeUpdateLocationCache()
		{
			var sourceLocationFilter = SourceLocationFilter;
			if (sourceLocationFilter.Property.IsValid)
			{
				var location = Factory.Load<WhsLocation>(sourceLocationFilter.Property);
				if (location == null || WD_WW_Whs != location.WLV_WW_Whs)
				{
					ClearLocationCache();
				}
				else
				{
					sourceLocationFilter.Validation.ValidateProperty();
				}
			}
			else
			{
				ClearLocationCache();
			}

			void ClearLocationCache()
			{
				sourceLocationFilter.Property = ZGuid.Empty;
				whsLocations = null;
			}
		}

		WhsLocationCollection whsLocations;

		#endregion

		#region AddStaffFilters

		#region AddPickedByFilter

		void AddPickedByFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddNkFilter("Picked By", GetRFSourcePickerQuery, ModuleIDs.GlbStaff, GlobalStaffs);
			filter.MultilingualDescription = ResString.GetMultilingualString("42026a1b-9e80-468a-9ddb-fe0b5115d824", "Picked By");
		}

		#endregion

		#region AddPutawayByFilter

		void AddPutawayByFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddNkFilter("Putaway By", GetRFDestPlacerQuery, ModuleIDs.GlbStaff, GlobalStaffs);
			filter.MultilingualDescription = ResString.GetMultilingualString("ab562c5b-3a9c-4ef8-8d58-cb73286490d4", "Putaway By");
		}

		#endregion

		#endregion

		#region AddDestinationPalletIdFilter

		protected override void AddPalletIdFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Pallet ID (Destination)", GetPalletIDQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MaxLength = WhsDocketLineSchema.WE_PalletID.MaxLength;
			filter.MultilingualDescription = ResString.GetMultilingualString("DocketFilterBusinessObject|PalletID(Destination)", "Pallet ID (Destination)");
		}

		#endregion

		#region AddSourcePalletIdFilter

		void AddSourcePalletIdFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Pallet ID (Source)", GetSourcePalletIDQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MaxLength = WhsDocketLineSchema.WE_TransferFromPalletId.MaxLength;
			filter.MultilingualDescription = ResString.GetMultilingualString("DocketFilterBusinessObject|PalletID(Source)", "Pallet ID (Source)");
		}

		#endregion

		#endregion

		#region Queries

		#region StaffQueries

		ZQuery GetRFSourcePickerQuery(ZString value)
		{
			var pickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			pickLineSubQuery.AddToFilter(WhsPickLineSchema.WZ_GS_NKAssignedTo, value);

			var transferLineSubQuery = new ZDBOnlySubQuery(typeof(WhsTransferLine), WhsDocketLineSchema.WE_WD);
			transferLineSubQuery.AddSubQuery(pickLineSubQuery, JoinCondition.And);

			var transferQuery = new ZDBOnlyQuery(typeof(WhsTransfer));
			transferQuery.AddSubQuery(transferLineSubQuery, JoinCondition.And);

			return transferQuery;
		}

		ZQuery GetRFDestPlacerQuery(ZString value)
		{
			var transferLineSubQuery = new ZDBOnlySubQuery(typeof(WhsTransferLine), WhsDocketLineSchema.WE_WD);
			transferLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_GS_NKPutawayBy, value);

			var transferQuery = new ZDBOnlyQuery(typeof(WhsTransfer));
			transferQuery.AddSubQuery(transferLineSubQuery, JoinCondition.And);

			return transferQuery;
		}

		#endregion

		#region DateQueries

		ZQuery GetRFSourceDatePickedQuery(DateComparisonOperator comparisonOperator, ZDateTimeOffset value1, ZDateTimeOffset value2)
		{
			var pickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			AddDateTimeOffsetRange(pickLineSubQuery, comparisonOperator, JoinCondition.And, WhsPickLineSchema.WZ_PickedDateTime, value1.DateAndOffset, value2.DateAndOffset, false, false);

			var transferLineSubQuery = new ZDBOnlySubQuery(typeof(WhsTransferLine), WhsDocketLineSchema.WE_WD);
			transferLineSubQuery.AddSubQuery(pickLineSubQuery, JoinCondition.And);

			var transferQuery = new ZDBOnlyQuery(typeof(WhsTransfer));
			transferQuery.AddSubQuery(transferLineSubQuery, JoinCondition.And);

			return transferQuery;
		}

		ZQuery GetRFDestDateTransferredQuery(DateComparisonOperator comparisonOperator, ZDateTimeOffset value1, ZDateTimeOffset value2)
		{
			var transferLineSubQuery = new ZDBOnlySubQuery(typeof(WhsTransferLine), WhsDocketLineSchema.WE_WD);
			AddDateTimeOffsetRange(transferLineSubQuery, comparisonOperator, JoinCondition.And, WhsDocketLineSchema.WE_PutawayTime, value1.DateAndOffset, value2.DateAndOffset, false, false);

			var transferQuery = new ZDBOnlyQuery(typeof(WhsTransfer));
			transferQuery.AddSubQuery(transferLineSubQuery, JoinCondition.And);

			return transferQuery;
		}

		#endregion

		#region PalletQueries

		ZQuery GetSourcePalletIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			bool notIn = SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref comparisonOperator);

			var palletSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD, notIn);
			palletSubQuery.AddToFilter(WhsDocketLineSchema.WE_TransferFromPalletId, comparisonOperator, value);

			var docketQuery = new ZDBOnlyQuery(typeof(WhsDocket));
			docketQuery.AddSubQuery(palletSubQuery, JoinCondition.And);
			return docketQuery;
		}

		#endregion

		#endregion

		#region Docket Status Filter

		protected override CodeDescriptionPairList GetDocketStatusCore()
		{
			var status = base.GetDocketStatusCore();
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.New));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.AttachedToPick));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Picking));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Putaway));
			return status;
		}

		#endregion

		#region SupportsDocketPlanningStatus

		protected override bool SupportsDocketPlanningStatus => true;

		#endregion

		#region IncludeTransportCoFilter

		protected override bool IncludeTransportCoFilter => false;

		#endregion

		#region Lookups

		public TransferType TransferTypes
		{
			get
			{
				var result = new TransferType();
				result.AddPair(TransferTypeOptions.InterWarehouseTransfers, Res.GetString("f138445b-433b-4c32-b70d-d15cd15c393e", "Inter-Warehouse"));
				result.AddPair(NonPersistentTransferType.Codes.Putaway, NonPersistentTransferType.Descriptions.Putaway);
				result.AddPair(NonPersistentTransferType.Codes.OutboundDockDoor, NonPersistentTransferType.Descriptions.OutboundDockDoor);
				result.AddPair(NonPersistentTransferType.Codes.AutoCreatedReplenishment, NonPersistentTransferType.Descriptions.AutoCreatedReplenishment);
				result.SortByDescription();
				return result;
			}
		}

		public GlbStaffCollection GlobalStaffs => new GlbStaffCollection(Factory);

		#endregion
	}
}
