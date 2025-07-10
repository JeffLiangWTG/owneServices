using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using JobSailing = Enterprise.Freight.Business.JobSailing;
using JobVoyage = Enterprise.Freight.Business.JobVoyage;

namespace Enterprise.Freight.CFS.Module
{
	public class PackContainerRegistrationFilterBusinessObject : FilterStripBusinessObject
	{
		public PackContainerRegistrationFilterBusinessObject()
			: base()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddFlagFilters(filters);
			AddNumberFilters(filters);
			AddDateFilters(filters);
			AddOrganisationFilters(filters);
			AddLocationFilters(filters);
			AddVoyageVesselFilters(filters);
			AddModeTypeFilters(filters);
			return filters;
		}

		#region Flags Filter

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Attached To Load List", GetAttachedQuery, AttachedStatus_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("f819952d-dab2-4e49-8d19-07f5a48134ac", "Attached To Load List");
			filter.Category = FilterCategories.StatusAndFlags;
		}

		#endregion

		#region Flags Filter Deledates

		ZQuery GetAttachedQuery(ZString value)
		{
			ZQuery query = new ZQuery();

			switch (value)
			{
				case "IAL":
					query.AddToFilter(JobContainerSchema.JC_JK, SQLComparisonOperator.NotEqual, null);
					break;
				case "NAL":
					query.AddToFilter(JobContainerSchema.JC_JK, SQLComparisonOperator.Equal, null);
					break;
			}

			return query;
		}

		#endregion

		#region Number Filter

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddNumberFilter(ConstantsAndReusables.NumberFilterTypes.Container, JobContainerSchema.JC_ContainerNum);
			filter.MultilingualDescription = ResString.GetMultilingualString("ddb0b67b-2a34-4eb6-aacb-eff6306b5e87", "Container #");
			filter.IsCommon = true;

			filters.AddNumberFilter(ConstantsAndReusables.NumberFilterTypes.Release, GetBookingReferenceQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobContainerSchema.JC_FCLStorageModuleOnlyMaster)
				.MultilingualDescription = ResString.GetMultilingualString("7f774377-5590-4d20-8ed9-8bf71648e497", "Release #");

			filter = filters.AddNumberFilter(ConstantsAndReusables.NumberFilterTypes.Job, JobContainerSchema.JC_ContainerJobID);
			filter.MultilingualDescription = ResString.GetMultilingualString("93fd5718-142d-4e2f-a8b2-0fb8962e2925", "Job #");
			filter.IsCommon = true;

			filters.AddNumberFilter(ConstantsAndReusables.NumberFilterTypes.MasterBill, GetMasterBillQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobContainerSchema.JC_FCLStorageModuleOnlyMaster)
				.MultilingualDescription = ResString.GetMultilingualString("edaf133c-4483-401f-b286-af1b989bd1ab", "Master Bill");

			filters.AddNumberFilter(ConstantsAndReusables.NumberFilterTypes.CustomsEntryNumber, GetCustomsEntryQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobConsolSchema.JK_CustomsReference)
				.MultilingualDescription = ResString.GetMultilingualString("463e7002-76f1-4ab0-9336-c1d23502ddbd", "Customs Entry #");

			filters.AddNumberFilter("Common Numbers", GetCommonNumbersQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobContainerSchema.JC_ContainerJobID)
				.MultilingualDescription = ResString.GetMultilingualString("df43e144-abc3-4709-af1f-910bc14bdf62", "Common Numbers");
		}

		#endregion

		#region Number Filter Delegates

		ZQuery GetBookingReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddBookingReferenceToFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetMasterBillQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddMasterBillToFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetCustomsEntryQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddCustomsEntryNoToFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetCommonNumbersQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();

			query.AddToFilter_PossiblyCommaSeparated(JoinCondition.Or, JobContainerSchema.JC_ContainerJobID, comparisonOperator, value);
			query.AddToFilter_PossiblyCommaSeparated(JoinCondition.Or, JobContainerSchema.JC_ContainerNum, comparisonOperator, value);

			return query;
		}

		#endregion

		#region Number Filter Implementation

		protected void AddBookingReferenceToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobContainerSchema.JC_FCLStorageModuleOnlyMaster, @operator, value);

			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(CFSContainer));
			ZDBOnlySubQuery consolQuery = new ZDBOnlySubQuery(typeof(CFSLoadListConsol), JobContainerSchema.JC_JK);
			consolQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobConsolSchema.JK_BookingReference, @operator, value);

			dbOnlyResult.AddSubQuery(consolQuery, JoinCondition.And);

			query.AddToFilter(dbOnlyResult, JoinCondition.Or);
		}

		protected void AddMasterBillToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobContainerSchema.JC_FCLStorageModuleOnlyMaster, @operator, value);

			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(CFSContainer));
			ZDBOnlySubQuery consolQuery = new ZDBOnlySubQuery(typeof(CFSLoadListConsol), JobContainerSchema.JC_JK);
			consolQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobConsolSchema.JK_MasterBillNum, @operator, value);

			dbOnlyResult.AddSubQuery(consolQuery, JoinCondition.And);

			query.AddToFilter(dbOnlyResult, JoinCondition.Or);
		}

		protected void AddCustomsEntryNoToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(CFSContainer));

			ZDBOnlySubQuery consolQuery = new ZDBOnlySubQuery(typeof(CFSLoadListConsol), JobContainerSchema.JC_JK);
			consolQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobConsolSchema.JK_CustomsReference, @operator, value);

			dbOnlyResult.AddSubQuery(consolQuery, JoinCondition.And);

			query.AddToFilter(dbOnlyResult, JoinCondition.And);
		}

		#endregion

		#region Date Filter

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(ConstantsAndReusables.DateFilterTypes.ETD, GetETDQuery).MultilingualDescription = ResString.GetMultilingualString("83b397ba-4ec2-4504-b84d-da210154fad8", "ETD");
			filters.AddDateFilter(ConstantsAndReusables.DateFilterTypes.ETA, GetETAQuery).MultilingualDescription = ResString.GetMultilingualString("08868741-c670-484f-8688-dba901bf2e7a", "ETA");
			filters.AddDateFilter(ConstantsAndReusables.DateFilterTypes.Unpack, GetLCLUnpackQuery).MultilingualDescription = ResString.GetMultilingualString("95ff6de9-43f7-44c4-9124-5f12b912279e", "Unpack");
			filters.AddDateFilter(ConstantsAndReusables.DateFilterTypes.Available, GetLCLAvailableQuery).MultilingualDescription = ResString.GetMultilingualString("5113754d-4ed1-41de-ac84-cea809689952", "Available");
			filters.AddDateFilter(ConstantsAndReusables.DateFilterTypes.Storage, GetLCLStorageCommencesQuery).MultilingualDescription = ResString.GetMultilingualString("c8d13a3d-0a87-42b4-a774-499b62b61d1f", "Storage");
			filters.AddDateFilter(ConstantsAndReusables.DateFilterTypes.Pack, GetPackDateQuery).MultilingualDescription = ResString.GetMultilingualString("4d54a3e4-09fe-45ad-abfc-6a8e3e84887d", "Pack");
			filters.AddDateFilter(ConstantsAndReusables.DateFilterTypes.Arrival, GetArrivalTimeQuery).MultilingualDescription = ResString.GetMultilingualString("af1ee995-8067-49b5-b0a9-fef8f1064cf3", "Arrival");
			filters.AddDateFilter(ConstantsAndReusables.DateFilterTypes.Departure, GetDepartureTimeQuery).MultilingualDescription = ResString.GetMultilingualString("592df203-ccef-4a3c-9976-469afb5dbd1c", "Departure");
		}

		#endregion

		#region Date Filter Delegates

		ZQuery GetETDQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();
			AddETDToQuery(query, comparisonOperator, fromDate, toDate);

			return query;
		}

		ZQuery GetETAQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();
			AddETAToQuery(query, comparisonOperator, fromDate, toDate);
			return query;
		}

		ZQuery GetLCLUnpackQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();
			AddLCLUnpackToQuery(query, comparisonOperator, fromDate, toDate);
			return query;
		}

		ZQuery GetLCLAvailableQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();
			AddLCLAvailableToQuery(query, comparisonOperator, fromDate, toDate);
			return query;
		}

		ZQuery GetLCLStorageCommencesQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();
			AddLCLStorageCommencesToQuery(query, comparisonOperator, fromDate, toDate);
			return query;
		}

		ZQuery GetPackDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();
			AddDateTimeRange(query, comparisonOperator, JoinCondition.And, JobContainerSchema.JC_PackDate, fromDate, toDate, false, true);
			return query;
		}

		ZQuery GetArrivalTimeQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();
			AddArrivalTimeToQuery(query, comparisonOperator, fromDate, toDate);
			return query;
		}

		ZQuery GetDepartureTimeQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();
			AddDepartureTimeToQuery(query, comparisonOperator, fromDate, toDate);
			return query;
		}

		#endregion

		#region Date Filter Implementation

		protected void AddETDToQuery(ZQuery query, DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var originFilter = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);

			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				AddDateTimeRange(originFilter, DateComparisonOperator.HasDateEntered, JoinCondition.And, JobVoyOriginSchema.JA_E_DEP, fromDate, toDate, false, true);
				query.AddToFilter(HasNoDateContainerFilterOnSailing(originFilter));
			}
			else
			{
				AddDateTimeRange(originFilter, comparisonOperator, JoinCondition.And, JobVoyOriginSchema.JA_E_DEP, fromDate, toDate, false, true);

				var sailingFilter = new ZDBOnlyQuery(typeof(JobSailing));
				sailingFilter.AddSubQuery(originFilter, JoinCondition.And);
				query.AddToFilter(ContainerFilterOnSailing(sailingFilter));
			}
		}

		protected void AddETAToQuery(ZQuery query, DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var originFilter = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				AddDateTimeRange(originFilter, DateComparisonOperator.HasDateEntered, JoinCondition.And, JobVoyDestinationSchema.JB_E_ARV, ZDateTime.Empty, ZDateTime.Empty, false, true);
				query.AddToFilter(HasNoDateContainerFilterOnSailing(originFilter));
			}
			else
			{
				AddDateTimeRange(originFilter, comparisonOperator, JoinCondition.And, JobVoyDestinationSchema.JB_E_ARV, fromDate, toDate, false, true);

				var sailingFilter = new ZDBOnlyQuery(typeof(JobSailing));
				sailingFilter.AddSubQuery(originFilter, JoinCondition.And);

				query.AddToFilter(ContainerFilterOnSailing(sailingFilter));
			}
		}

		protected void AddLCLUnpackToQuery(ZQuery query, DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(CFSContainer));
			AddDateTimeRange(dbOnlyResult, comparisonOperator, JoinCondition.And, JobContainerSchema.JC_LCLUnpack, fromDate, toDate, false, true);

			query.AddToFilter(dbOnlyResult, JoinCondition.And);
		}

		protected void AddLCLAvailableToQuery(ZQuery query, DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(CFSContainer));
			AddDateTimeRange(dbOnlyResult, comparisonOperator, JoinCondition.And, JobContainerSchema.JC_LCLAvailable, fromDate, toDate, false, true);

			query.AddToFilter(dbOnlyResult, JoinCondition.And);
		}

		protected void AddLCLStorageCommencesToQuery(ZQuery query, DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(TallyContainer));
			AddDateTimeRange(dbOnlyResult, comparisonOperator, JoinCondition.And, JobContainerSchema.JC_LCLStorageCommences, fromDate, toDate, false, true);

			query.AddToFilter(dbOnlyResult, JoinCondition.And);
		}

		protected void AddArrivalTimeToQuery(ZQuery query, DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var dbOnlyResult = new ZDBOnlyQuery(typeof(TallyContainer));

			var arrivalLegs = new ZDBOnlySubQuery(typeof(CommonPickupDeliveryConfirm), JobPickupDeliveryConfirmSchema.EU_JC);
			AddDateTimeRange(arrivalLegs, comparisonOperator, JoinCondition.And, JobPickupDeliveryConfirmSchema.EU_PickupDeliveryTime, fromDate, toDate, false, true);

			var arrivalLegsTypeQuery = new ZQuery(JobPickupDeliveryConfirmSchema.EU_PickupDeliveryType, Constants.PickupDeliveryConfirmTypes.OriginCFSArrival);
			arrivalLegsTypeQuery.AddToFilter(JoinCondition.Or, JobPickupDeliveryConfirmSchema.EU_PickupDeliveryType, Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival);
			arrivalLegs.AddToFilter(arrivalLegsTypeQuery, JoinCondition.And);

			dbOnlyResult.AddSubQuery(arrivalLegs, JoinCondition.And);
			query.AddToFilter(dbOnlyResult, JoinCondition.And);

			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				AddNotAttachedJobPickupDeliveryToQuery(query, new string[] { Constants.PickupDeliveryConfirmTypes.OriginCFSArrival, Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival });
			}
		}

		protected void AddDepartureTimeToQuery(ZQuery query, DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var dbOnlyResult = new ZDBOnlyQuery(typeof(TallyContainer));

			var departureLegs = new ZDBOnlySubQuery(typeof(CommonPickupDeliveryConfirm), JobPickupDeliveryConfirmSchema.EU_JC);
			AddDateTimeRange(departureLegs, comparisonOperator, JoinCondition.And, JobPickupDeliveryConfirmSchema.EU_PickupDeliveryTime, fromDate, toDate, false, true);

			var departureLegsTypeQuery = new ZQuery(JobPickupDeliveryConfirmSchema.EU_PickupDeliveryType, Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture);
			departureLegsTypeQuery.AddToFilter(JoinCondition.Or, JobPickupDeliveryConfirmSchema.EU_PickupDeliveryType, Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture);
			departureLegs.AddToFilter(departureLegsTypeQuery, JoinCondition.And);

			dbOnlyResult.AddSubQuery(departureLegs, JoinCondition.And);
			query.AddToFilter(dbOnlyResult, JoinCondition.And);

			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				AddNotAttachedJobPickupDeliveryToQuery(query, new string[] { Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture, Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture });
			}
		}

		void AddNotAttachedJobPickupDeliveryToQuery(ZQuery query, string[] pickupDeliveryConfirmTypes)
		{
			var dbOnlyResult = new ZDBOnlyQuery(typeof(TallyContainer));

			var departureLegs = new ZDBOnlySubQuery(typeof(CommonPickupDeliveryConfirm), JobPickupDeliveryConfirmSchema.EU_JC, true);

			var departureLegsTypeQuery = new ZQuery(JobPickupDeliveryConfirmSchema.EU_PickupDeliveryType, pickupDeliveryConfirmTypes);
			departureLegs.AddToFilter(departureLegsTypeQuery, JoinCondition.And);

			dbOnlyResult.AddSubQuery(departureLegs, JoinCondition.And);
			query.AddToFilter(dbOnlyResult, JoinCondition.Or);
		}

		#endregion

		#region Organisation Filter

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddGuidFilter(ConstantsAndReusables.OrgFilterTypes.Client, ModuleIDs.Organisation, JobContainerSchema.JC_OH_CFSClient, Client_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("57a48ca1-bac1-41d6-b61f-5d80cc7c3442", "Client");
			filter.Category = FilterCategories.Organisations;

			filter = filters.AddGuidFilter(ConstantsAndReusables.OrgFilterTypes.Line, ModuleIDs.Organisation, GetShippingLineQuery, ShippingLine_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("96b1692b-72f0-4877-abeb-deb8879cc561", "Shipping Line");
			filter.Category = FilterCategories.Organisations;
		}

		#endregion

		#region Organisation Filter Delegates

		ZQuery GetShippingLineQuery(ZGuid value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CFSContainer));
			ZDBOnlySubQuery consolQuery = new ZDBOnlySubQuery(typeof(CFSLoadListConsol), JobContainerSchema.JC_JK);
			ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobConsolSchema.JK_OA_ShippingLineAddress);
			orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, value);
			consolQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

			result.AddSubQuery(consolQuery, JoinCondition.And);

			return result;
		}

		#endregion
		#region Location Filter

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddLocationFilter(ConstantsAndReusables.PortFilterTypes.LoadDischarge, GetLoadDischargeQuery, Location_List, Location_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("2cbff9f7-b05b-4882-a72b-6bef790f9745", "Load / Discharge");
			filter.SetItemDescriptions(Res.GetData("3a6e8043-9afc-4d98-bc16-ea8bfededffb", "Load"), Res.GetData("e46dd846-8998-44f4-b2a3-86fcc2e3993c", "Discharge"));
		}

		#endregion

		#region Location Filter Delegates

		ZQuery GetLoadDischargeQuery(ZString loadNk, ZString dischargeNk)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CFSContainer));

			ZDBOnlyQuery sailingFilter = new ZDBOnlyQuery(typeof(JobSailing));

			if (!loadNk.IsEmpty)
			{
				bool isCountryCode = (loadNk.Length == 2);
				SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

				ZDBOnlySubQuery originFilter = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
				originFilter.AddToFilter(JobVoyOriginSchema.JA_RL_NKPortOfLoading, comparisonOperator, loadNk);

				sailingFilter.AddSubQuery(originFilter, JoinCondition.And);
			}

			if (!dischargeNk.IsEmpty)
			{
				bool isCountryCode = (loadNk.Length == 2);
				SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

				ZDBOnlySubQuery destinationFilter = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
				destinationFilter.AddToFilter(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, comparisonOperator, dischargeNk);

				sailingFilter.AddSubQuery(destinationFilter, JoinCondition.And);
			}

			query.AddToFilter(ContainerFilterOnSailing(sailingFilter));

			return query;
		}

		#endregion

		#region Vessel / Voyage Filter

		void AddVoyageVesselFilters(ModuleFilterCollection filters)
		{
			var filter = new VoyageVesselModuleFilter("Voyage / Flight / Vessel", GetVoyageVesselQuery, Vessel_List)
				.WithMaxLengthOf(JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
			filter.MultilingualDescription = ResString.GetMultilingualString("c880c09d-d1e6-45c9-9445-f1119de4b331", "Voyage / Flight / Vessel");
			filter.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(filter);
		}

		#endregion

		#region Vessel / Voyage Filter Delegates

		ZQuery GetVoyageVesselQuery(SQLComparisonOperator comparisonOperator, ZString voyageFlight, ZString vesselNk, ZBool includeArchived)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CFSContainer));

			if (!voyageFlight.IsEmpty || !vesselNk.IsEmpty || comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				ZDBOnlySubQuery voyageFilter = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyOriginSchema.JA_JV);

				VoyageVesselModuleFilterHelper.ApplyBasicVoyageVesselQuery(voyageFilter, comparisonOperator, voyageFlight, vesselNk, !includeArchived, JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);

				ZDBOnlySubQuery originFilter = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
				originFilter.AddSubQuery(voyageFilter, JoinCondition.And);

				ZDBOnlyQuery sailingFilter = new ZDBOnlyQuery(typeof(JobSailing));
				sailingFilter.AddSubQuery(originFilter, JoinCondition.And);

				query.AddToFilter(ContainerFilterOnSailing(sailingFilter));
			}

			return query;
		}

		#endregion

		#region Mode / Type Filter

		void AddModeTypeFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Purpose Type", GetPurposeTypeQuery, PurposeType_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("878e0497-c571-4bbf-a5c5-2a8a1ab375b2", "Purpose Type");
			filter.Category = FilterCategories.ModesAndTypes;

			filter = filters.AddTextFilter("Container Mode", GetContainerModeQuery, ContainerMode_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("265f07fd-a3d8-4bab-9fdb-e74445448b59", "Container Mode");
			filter.Category = FilterCategories.ModesAndTypes;
		}

		#endregion

		#region Mode / Type Filter Delegates

		ZQuery GetPurposeTypeQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(JobContainerSchema.JC_Purpose, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetContainerModeQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(JobContainerSchema.JC_ContainerMode, SQLComparisonOperator.Equal, value);
			return query;
		}

		#endregion

		#region Filter Overrides

		public override ZQuery Filter
		{
			get
			{
				ZQuery result = new ZQuery();
				result.AddToFilter(base.Filter);
				result.AddToFilter(JobContainerSchema.JC_IsCFSRegistered, SQLComparisonOperator.Equal, ZBool.True);
				return result;
			}
		}

		#endregion

		#region Implementation

		ZDBOnlyQuery ContainerFilterOnSailing(ZQuery sailingFilter)
		{
			var sailingQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
			sailingQuery.AddToFilter(sailingFilter);

			var transportFilter = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			transportFilter.AddSubQuery(JobConsolTransportSchema.JW_JX, sailingQuery, JoinCondition.And);

			var consolFilter = new ZDBOnlySubQuery(typeof(CommonConsol), JobConsolSchema.PK);
			consolFilter.AddSubQuery(JobConsolSchema.PK, transportFilter, JoinCondition.And);

			var sailingSubFilter = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
			sailingSubFilter.AddToFilter(sailingFilter);

			var containerFilter = new ZDBOnlyQuery(typeof(CommonContainer));
			containerFilter.AddSubQuery(JobContainerSchema.JC_JK, consolFilter, JoinCondition.And);
			containerFilter.AddSubQuery(JobContainerSchema.JC_JX, sailingSubFilter, JoinCondition.Or);

			return containerFilter;
		}

		ZDBOnlyQuery HasNoDateContainerFilterOnSailing(ZDBOnlySubQuery originFilter)
		{
			var sailingFilter = new ZDBOnlyQuery(typeof(JobSailing));
			sailingFilter.AddSubQuery(originFilter, JoinCondition.And);

			var sailingQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
			sailingQuery.AddToFilter(sailingFilter);

			var transportFilter = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			transportFilter.AddSubQuery(JobConsolTransportSchema.JW_JX, sailingQuery, JoinCondition.And);

			var consolFilter = new ZDBOnlySubQuery(typeof(CommonConsol), JobContainerSchema.JC_JK, true);
			consolFilter.AddSubQuery(JobConsolSchema.PK, transportFilter, JoinCondition.And);

			var sailingSubFilter = new ZDBOnlySubQuery(typeof(JobSailing), JobContainerSchema.JC_JX, true);
			sailingSubFilter.AddToFilter(sailingFilter);

			var result = new ZDBOnlyQuery(typeof(CommonContainer));
			result.AddSubQuery(sailingSubFilter, JoinCondition.And);
			result.AddSubQuery(consolFilter, JoinCondition.Or);

			var filter1 = new ZQuery(JobContainerSchema.JC_JX, SQLComparisonOperator.Equal, null);
			var filter2 = new ZQuery(JobContainerSchema.JC_JK, SQLComparisonOperator.Equal, null);
			var combinedQuery = new ZQuery(filter1, JoinCondition.And, filter2);
			result.AddToFilter(combinedQuery, JoinCondition.Or);

			return result;
		}

		#endregion

		#region Lists

		CodeDescriptionPairList fAttachedStatus_List;
		public CodeDescriptionPairList AttachedStatus_List
		{
			get
			{
				if (fAttachedStatus_List == null)
				{
					fAttachedStatus_List = new CodeDescriptionPairList();
					fAttachedStatus_List.AddPair("ALL", Res.GetString("50f7cbb4-8e6f-412f-bd27-ab7826f79ed9", "All"));
					fAttachedStatus_List.AddPair("IAL", Res.GetString("f819952d-dab2-4e49-8d19-07f5a48134ac", "Attached To Load List"));
					fAttachedStatus_List.AddPair("NAL", Res.GetString("edf841bc-42b7-4a51-8999-b2b4a454cade", "Not Attached To Load List"));
				}
				return fAttachedStatus_List;
			}
		}

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		DebtorCollection fClient_List;
		public DebtorCollection Client_List
		{
			get
			{
				if (fClient_List == null)
				{
					fClient_List = BindingLists.OrgDebtor_List;
				}
				return fClient_List;
			}
		}

		SeaShippingProviderCollection fShippingLine_List;
		public SeaShippingProviderCollection ShippingLine_List
		{
			get
			{
				if (fShippingLine_List == null)
				{
					fShippingLine_List = BindingLists.SeaShippingProvider_List;
				}
				return fShippingLine_List;
			}
		}

		OrgHeaderCollection fOrganisation_List;
		public OrgHeaderCollection Organisation_List
		{
			get
			{
				if (fOrganisation_List == null)
				{
					fOrganisation_List = BindingLists.OrgHeader_List;
				}
				return fOrganisation_List;
			}
		}

		LocationCollection fLocation_List;
		public LocationCollection Location_List
		{
			get
			{
				if (fLocation_List == null)
				{
					fLocation_List = new LocationCollection(Factory);
				}
				return fLocation_List;
			}
		}

		RefVesselCollection fVessel_List;
		public RefVesselCollection Vessel_List
		{
			get
			{
				if (fVessel_List == null)
				{
					fVessel_List = new RefVesselCollection(Factory);
				}
				return fVessel_List;
			}
		}

		CodeDescriptionPairList fPurposeType_List;
		public CodeDescriptionPairList PurposeType_List
		{
			get
			{
				if (fPurposeType_List == null)
				{
					fPurposeType_List = new ContainerPurposeTypeCodeDescriptionPairList();
				}
				return fPurposeType_List;
			}
		}

		CodeDescriptionPairList fContainerMode_List;
		public CodeDescriptionPairList ContainerMode_List
		{
			get
			{
				if (fContainerMode_List == null)
				{
					fContainerMode_List = new ContainerModeCodeDescriptionPairList(Constants.TransportModes.All);
				}
				return fContainerMode_List;
			}
		}

		#endregion

		#region workflow filter

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();

			var workflowHelper = new WorkflowFilterStripsHelperWithRoutingSupport(typeof(CFSContainer), WorkflowDescriptors.ContainerWorkflowDescriptorCode, Factory);
			helpers.Add(workflowHelper);

			return helpers;
		}

		#endregion
	}
}
