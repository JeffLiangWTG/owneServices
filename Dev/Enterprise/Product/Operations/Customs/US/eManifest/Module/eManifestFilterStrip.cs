using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Module
{
	public class eManifestFilterStrip : FilterStripBusinessObject
	{
		#region Descriptions

		internal static class Descriptions
		{
			internal static MultilingualString TripReferenceMultilingualDescription
			{
				get { return ResString.GetMultilingualString("704faf93-2f6a-4ff0-b0bd-bf03ab609c44", "Trip Reference"); }
			}
			internal const string TripReferenceFilterId = "Trip Reference";

			internal static MultilingualString JobReferenceMultilingualDescription
			{
				get { return ResString.GetMultilingualString("12345678-2f6a-4ff0-b0bd-bf03ab609c44", "Job Reference"); }
			}
			internal const string JobReferenceFilterId = "Job Reference";

			internal static MultilingualString ClientMultilingualDescription
			{
				get { return ResString.GetMultilingualString("87654321-2f6a-4ff0-b0bd-bf03ab609c44", "Client"); }
			}
			internal const string ClientFilterId = "Client";

			internal static MultilingualString MethodOfTransportationMultilingualDescription
			{
				get { return ResString.GetMultilingualString("6ba50913-582c-4439-858f-f324e35b718b", "Method Of Transportation"); }
			}
			internal const string MethodOfTransportationFilterId = "Method Of Transportation";

			internal static MultilingualString CarrierCodeMultilingualDescription
			{
				get { return ResString.GetMultilingualString("3231ba4c-2a54-4787-a459-7a3144ea5694", "Carrier Code (SCAC)"); }
			}
			internal const string CarrierCodeFilterId = "Carrier Code (SCAC)";

			internal static MultilingualString EstimatedDateOfArrivalMultilingualDescription
			{
				get { return ResString.GetMultilingualString("ed775944-337f-4858-8be7-869aa7ab46f3", "Estimated Date Of Arrival"); }
			}
			internal const string EstimatedDateOfArrivalFilterId = "Estimated Date Of Arrival";

			internal static MultilingualString FirstExpectedPortOfArrivalMultilingualDescription
			{
				get { return ResString.GetMultilingualString("545546f6-d0ee-42e2-9caf-30e23b550f1c", "Port Of Arrival"); }
			}
			internal const string FirstExpectedPortOfArrivalFilterId = "Port Of Arrival";

			internal static MultilingualString FirstExpectedPortOfArrivalScheduleDMultilingualDescription
			{
				get { return ResString.GetMultilingualString("6feb30f8-7840-46c4-9fe1-619c857a9a9b", "Port Of Arrival (Schedule D)"); }
			}
			internal const string FirstExpectedPortOfArrivalScheduleDFilterId = "Port Of Arrival (Schedule D)";

			internal static MultilingualString TransitDirectionMultilingualDescription
			{
				get { return ResString.GetMultilingualString("2f63d28b-5321-4c61-bee1-5cc965b3f6de", "Transit Direction"); }
			}
			internal const string TransitDirectionFilterId = "Transit Direction";

			internal static MultilingualString MessageStatusMultilingualDescription
			{
				get { return ResString.GetMultilingualString("2268fbc7-9ed7-4e4f-9bed-cf91bebc8b2d", "Message Status"); }
			}
			internal const string MessageStatusFilterId = "Message Status";

			internal static MultilingualString ReleaseStatusMultilingualDescription
			{
				get { return ResString.GetMultilingualString("6cc833f4-0e9b-4829-adc7-1bb2882ae7b4", "Release Status"); }
			}
			internal const string ReleaseStatusFilterId = "Release Status";

			internal static MultilingualString ShipmentReleaseStatusMultilingualDescription
			{
				get { return ResString.GetMultilingualString("a77a280e-3bec-4774-b11c-5c5d896076ee", "Shipment Release Status"); }
			}
			internal const string ShipmentReleaseStatusFilterId = "Shipment Release Status";

			internal static MultilingualString ConveyanceMultilingualDescription
			{
				get { return ResString.GetMultilingualString("97956115-4b25-4923-b9be-02bd7837d9dc", "Conveyance"); }
			}
			internal const string ConveyanceFilterId = "Conveyance";
		}

		#endregion

		#region Module Filters

		public override ZQuery Filter
		{
			get
			{
				var result = base.Filter;
				result.AddToFilter(GetCompanyQuery());
				return result;
			}
		}

		ZQuery GetCompanyQuery()
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			query.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.eManifest);

			var refBranch = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
			refBranch.AddToFilter(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK);
			query.AddSubQuery(CusInBondHeaderSchema.BH_GB, refBranch, JoinCondition.And);
			return query;
		}

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return new NoBlankModuleNumberFilter(
				Descriptions.JobReferenceFilterId,
				Descriptions.JobReferenceMultilingualDescription,
				CusInBondHeaderSchema.BH_JobReference);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var collection = new ModuleFilterCollection();

			var tripFilter = collection.AddNumberFilter(Descriptions.TripReferenceFilterId, CusInBondHeaderSchema.BH_VoyageNumber);
			tripFilter.MultilingualDescription = Descriptions.TripReferenceMultilingualDescription;

			var clientFilter = collection.AddGuidFilter(Descriptions.ClientFilterId, ModuleIDs.Organisation, GetImporterQuery, () => new DebtorCollection(Factory));
			clientFilter.MultilingualDescription = Descriptions.ClientMultilingualDescription;

			#region Carrier Code

			AddNumberFilter(
				collection,
				Descriptions.CarrierCodeFilterId,
				Descriptions.CarrierCodeMultilingualDescription,
				CusInBondHeaderSchema.BH_CarrierSCAC);

			#endregion

			#region Conveyance

			AddNumberFilter(
				collection,
				Descriptions.ConveyanceFilterId,
				Descriptions.ConveyanceMultilingualDescription,
				GetConveyanceQuery);
			collection[Descriptions.ConveyanceFilterId].MaxLength = RefEquipmentSchema.RQ_Registration.MaxLength;

			#endregion

			#region Estimated Date Of Arrival

			AddDateFilter(
				collection,
				Descriptions.EstimatedDateOfArrivalFilterId,
				Descriptions.EstimatedDateOfArrivalMultilingualDescription,
				CusInBondHeaderSchema.BH_ETA);

			#endregion

			#region First Expected Port Of Arrival

			AddLocation(
				collection,
				Descriptions.FirstExpectedPortOfArrivalFilterId,
				Descriptions.FirstExpectedPortOfArrivalMultilingualDescription,
				CusInBondHeaderSchema.BH_RL_NKPortUnlading,
				ModuleIDs.RefUNLOCO,
				RefUNLOCOSchema.RL_Code,
				Lookups.PortUnladings);

			AddLocation(
				collection,
				Descriptions.FirstExpectedPortOfArrivalScheduleDFilterId,
				Descriptions.FirstExpectedPortOfArrivalScheduleDMultilingualDescription,
				CusInBondHeaderSchema.BH_PortUnladingDCode,
				ModuleIDs.Customs.Universal.ZZRefCusCodeList,
				ZZRefCusCodeListCombinedSchema.ZZD_Code,
				Lookups.ScheduleDPortCodes);

			#endregion

			#region Transit Direction

			AddStatusFilter(
				collection,
				Descriptions.TransitDirectionFilterId,
				Descriptions.TransitDirectionMultilingualDescription,
				CusInBondHeaderSchema.BH_TransitDirection,
				() => Lookups.TransitDirectionCodes);

			#endregion

			#region Message Status

			AddStatusFilter(
				collection,
				Descriptions.MessageStatusFilterId,
				Descriptions.MessageStatusMultilingualDescription,
				CusInBondHeaderSchema.BH_MessageStatus,
				() => Lookups.MessageStatusList,
				ignoreEmpty: false);

			#endregion

			#region Release Status

			AddStatusFilter(
				collection,
				Descriptions.ReleaseStatusFilterId,
				Descriptions.ReleaseStatusMultilingualDescription,
				CusInBondHeaderSchema.BH_ReleaseStatus,
				() => Lookups.ReleaseStatusList);

			AddStatusFilter(
				collection,
				Descriptions.ShipmentReleaseStatusFilterId,
				Descriptions.ShipmentReleaseStatusMultilingualDescription,
				GetShipmentReleaseStatusQuery,
				() => Lookups.ReleaseStatusList);
			collection[Descriptions.ShipmentReleaseStatusFilterId].MaxLength = CusInBondBillSchema.B0_ReleaseStatus.MaxLength;
			#endregion

			return collection;
		}

		#region Queries
		ZQuery GetImporterQuery(ZGuid client)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(Trip));
			if (client.IsValid)
			{
				ZDBOnlySubQuery addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				ZDBOnlySubQuery headerQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
				headerQuery.AddToFilter(OrgHeaderSchema.PK, client);
				addressQuery.AddSubQuery(headerQuery, JoinCondition.And);
				query.AddSubQuery(CusInBondHeaderSchema.BH_OA_Importer, addressQuery, JoinCondition.And);
			}

			return query;
		}

		#region Conveyance Query

		ZQuery GetConveyanceQuery(SQLComparisonOperator op, ZString value)
		{
			return GetTripQueryWithSubQuery(op, value, GetConveyanceSubQuery);
		}

		ZDBOnlySubQuery GetConveyanceSubQuery(SQLComparisonOperator op, ZString value, bool notIn, bool ignoreFilter)
		{
			var conveyanceSubQuery = new ZDBOnlySubQuery(typeof(Equipment), CusInBondEquipmentSchema.BJ_BH_Header, notIn);
			conveyanceSubQuery.AddToFilter(CusInBondEquipmentSchema.BJ_IsConveyance, true);
			if (!ignoreFilter)
			{
				var refEquipmentSubQuery = new ZDBOnlySubQuery(typeof(RefEquipment), RefEquipmentSchema.PK);
				refEquipmentSubQuery.AddToFilter_PossiblyCommaSeparated(RefEquipmentSchema.RQ_Registration, op, value);
				conveyanceSubQuery.AddSubQuery(CusInBondEquipmentSchema.BJ_RQ_Equipment, refEquipmentSubQuery, JoinCondition.And);
			}
			return conveyanceSubQuery;
		}

		#endregion

		#region Shipment Release Status Query

		ZQuery GetShipmentReleaseStatusQuery(SQLComparisonOperator op, ZString value)
		{
			return GetTripQueryWithSubQuery(op, value, GetShipmentReleaseStatusSubQuery);
		}

		ZDBOnlySubQuery GetShipmentReleaseStatusSubQuery(SQLComparisonOperator op, ZString value, bool notIn, bool ignoreFilter)
		{
			var shipmentSubQuery = new ZDBOnlySubQuery(typeof(Shipment), CusInBondBillSchema.B0_BH, notIn);
			if (!ignoreFilter)
			{
				shipmentSubQuery.AddToFilter(CusInBondBillSchema.B0_ReleaseStatus, op, value);
			}

			return shipmentSubQuery;
		}

		#endregion

		#region Implementation

		ZDBOnlyQuery GetTripQueryWithSubQuery(SQLComparisonOperator op, ZString value, GetSubQueryDelegate getSubQuery)
		{
			var tripQuery = new ZDBOnlyQuery(typeof(Trip));
			if (op == SpecialComparisonOperator.IsNotBlank)
			{
				var subQuery = getSubQuery(SQLComparisonOperator.NotEqual, ZString.Empty, false, false);
				tripQuery.AddSubQuery(subQuery, JoinCondition.And);
			}
			if (op == SpecialComparisonOperator.IsBlank)
			{
				tripQuery.AddSubQuery(getSubQuery(SQLComparisonOperator.Equal, ZString.Empty, false, false), JoinCondition.And);
				tripQuery.AddSubQuery(getSubQuery(SQLComparisonOperator.Equal, ZString.Empty, true, true), JoinCondition.Or);
			}
			else if (!value.IsEmpty)
			{
				var isNegativeComparisonOperator = Helper.OperatorsDictionary.ContainsKey(op);
				if (isNegativeComparisonOperator)
				{
					op = Helper.OperatorsDictionary[op];
				}

				tripQuery.AddSubQuery(getSubQuery(op, value, isNegativeComparisonOperator, false), JoinCondition.And);
			}
			return tripQuery;
		}

		delegate ZDBOnlySubQuery GetSubQueryDelegate(SQLComparisonOperator op, ZString value, bool notIn, bool ignoreFilter);

		#endregion

		#endregion

		#endregion

		#region Implementation

		internal static void AddNumberFilter(ModuleFilterCollection collection, string filterId, MultilingualString description, SchemaStringColumn column)
		{
			var filter = collection.AddNumberFilter(filterId, column);
			filter.MultilingualDescription = description;
		}

		internal static void AddNumberFilter(ModuleFilterCollection collection, string filterId, MultilingualString description, GetTextQueryWithOperator getQuery)
		{
			var filter = collection.AddNumberFilter(filterId, getQuery);
			filter.MultilingualDescription = description;
		}

		static void AddDateFilter(ModuleFilterCollection collection, string filterId, MultilingualString description, SchemaDateTimeColumn column)
		{
			var filter = collection.AddDateFilter(filterId, column);
			filter.MultilingualDescription = description;
		}

		internal static void AddLocation(ModuleFilterCollection collection, string filterId, MultilingualString description, SchemaStringColumn column, ModuleIdentifier moduleId, SchemaStringColumn foreignCodeColumn, IBusinessObjectCollection list)
		{
			var filter = collection.AddTextAndNkFilter(filterId, column, moduleId, column, list);
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = description;
			filter.ForeignCodeColumnOverride = foreignCodeColumn;
		}

		internal static void AddStatusFilter(ModuleFilterCollection collection, string filterId, MultilingualString description, SchemaStringColumn column,
									 GetList listDelegate, bool ignoreEmpty = true)
		{
			var filter = new EqualModuleTextFilter(
				filterId,
				description,
				FilterCategories.StatusAndFlags,
				column,
				listDelegate,
				ignoreEmpty);
			collection.AddFilter(filter);
		}

		internal static void AddStatusFilter(ModuleFilterCollection collection, string filterId, MultilingualString description, GetTextQueryWithOperator query,
									 GetList listDelegate, bool ignoreEmpty = true)
		{
			var filter = new EqualModuleTextFilter(
				filterId,
				description,
				FilterCategories.StatusAndFlags,
				query,
				listDelegate,
				ignoreEmpty);
			collection.AddFilter(filter);
		}

		GenAddOnColumnQueryHelper Helper
		{
			get { return helper ?? (helper = new GenAddOnColumnQueryHelper(typeof(Trip))); }
		}
		GenAddOnColumnQueryHelper helper;

		TripLookups Lookups
		{
			get { return lookups ?? (lookups = Factory.GetNull<Trip>().Lookups); }
		}
		TripLookups lookups;

		#region Module Filters

		internal class EqualModuleTextFilter : ModuleTextFilter
		{
			internal EqualModuleTextFilter(string filterName, MultilingualString description, FilterCategory category, SchemaStringColumn column, GetList listDelegate, bool ignoreEmpty)
				: base(filterName, column, listDelegate)
			{
				this.ignoreEmpty = ignoreEmpty;
				MultilingualDescription = description;
				Category = category;
			}

			internal EqualModuleTextFilter(string filterName, MultilingualString description, FilterCategory category, GetTextQueryWithOperator query, GetList listDelegate, bool ignoreEmpty)
				: base(filterName, query, listDelegate)
			{
				this.ignoreEmpty = ignoreEmpty;
				MultilingualDescription = description;
				Category = category;
			}

			public override IReadOnlyList<string> AllowedComparisonOperators
			{
				get
				{
					return new[]
									 {
										 string.Empty,
										 ComparisonConstants.Exact,
										 ComparisonConstants.NotEqual,
										 ComparisonConstants.IsBlank,
										 ComparisonConstants.IsNotBlank
									 };
				}
			}

			protected override bool IsEmptyCore => ignoreEmpty && base.IsEmptyCore;

			readonly bool ignoreEmpty;
		}

		internal class NoBlankModuleNumberFilter : ModuleNumberFilter
		{
			internal NoBlankModuleNumberFilter(string filterName, MultilingualString description, SchemaStringColumn filterColumn)
				: base(filterName, filterColumn)
			{
				MultilingualDescription = description;
			}

			public override IReadOnlyList<string> AllowedComparisonOperators
			{
				get
				{
					return new[]
									 {
										 string.Empty,
										 ComparisonConstants.Exact,
										 ComparisonConstants.StartsWith,
										 ComparisonConstants.Contains,
										 ComparisonConstants.NotEqual,
										 ComparisonConstants.NotStartsWith,
										 ComparisonConstants.NotContain
									 };
				}
			}
		}

		#endregion

		#endregion
	}
}
