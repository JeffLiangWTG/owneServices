using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Module.ImportReleaseOrder;
using Enterprise.Freight.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Module
{
	public class BillContainersFilterStrip : FilterStripBusinessObject
	{
		#region Descriptions

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "FilterStrip's Key")]
		public static class Descriptions
		{
			public const string Hidden = "<HIDDEN>";

			// Numbers & Refs
			public const string ContainerNumber = "Container #";
			public const string EntryTypeAndNumber = "Entry Type & Number";
			public const string BookingRef = "Booking Ref";
			public const string BillOfLading = "Bill of Lading";
			public const string DockReceipt = "Dock Receipt #";
			public const string ShipmentNumber = "Shipment #";
			public const string DetentionInvoiceNumber = "Detention Invoice #";
			public const string AdditionalReferenceNumbers = "Additional Reference #";
			public const string PackLineExportReference = "Pack Line Export Reference #";
			public const string PackLineImportReference = "Pack Line Import Reference #";

			// Dates
			public const string AvailabilityDate = "Availability Date";
			public const string EmptyRequiredBy = "Empty Required By";
			public const string EmptyReturned = "Empty Returned";
			public const string WharfGateOut = "Wharf Gate Out";
			public const string EstimatedTimeArrival = "ETA";
			public const string EstimatedTimeDeparture = "ETD";
			public const string ActualTimeArrival = "ATA";
			public const string ActualTimeDeparture = "ATD";

			// Text
			public const string VoyageVessel = "Voyage / Vessel";

			// Organisations / Staff
			public const string Consignee = "Consignee";
			public const string Consignor = "Consignor";
			public const string EmptyPickupFrom = "Empty Pickup From";
			public const string EmptyReturnTo = "Empty Return To";
			public const string LocalClient = "Local Client";
			public const string Principal = "Principal";

			// Locations
			public const string BranchRelatedPorts = "Branch Related Ports";
			public const string LoadDischarge = "Load / Discharge";
			public const string OriginDestination = "Origin / Destination";

			// Status and Flags
			public const string AvailabilityStatus = "Availability Status";
			public const string EIDOStatus = "E-IDO Status";
			public const string ImportReleaseOrderStatus = "Import Release Order Status";
			public const string ExportInvoiceStatus = "Export Detention Invoice Status";
			public const string ImportInvoiceStatus = "Import Detention Invoice Status";
			public const string ReturnedStatus = "Returned Status";
			public const string InvoiceStatus = "Invoice Status";

			// Modes And Types
			public const string CommodityCode = "Commodity Code";
			public const string ContainerType = "Container Type";
			public const string ContainerTypeCategory = "Container Type Category";
			public const string ContainerQuality = "Container Quality";
			public const string EmptyContainer = "Empty Container";
			public const string ShipperOwned = "Shipper Owned";
			public const string StorageClass = "Storage Class";
			public const string ContainerMode = "Container Mode";
		}

		#endregion

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection collection = new ModuleFilterCollection();

			AddHiddenFilters(collection);
			AddNumbersAndRefsFilters(collection);
			AddDateTimeFilters(collection);
			AddVoyageVesselFilters(collection);
			AddOrganisationsAndStaffFilters(collection);
			AddLocationsFilters(collection);
			AddStatusAndFlagsFilters(collection);
			AddModesAndTypesFilters(collection);

			return collection;
		}

		public override ZQuery Filter
		{
			get
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(base.Filter);
				if (!Globals.IsWeb)
				{
					filter.AddToFilter(JobContainerSchema.JC_Purpose, ContainerBookedStatus.Codes.Real);
					filter.AddToFilter(JobContainerSchema.JC_ContainerMode, SQLComparisonOperator.Equal, AgencyShipmentContainerModeList.GetContainerModes(Constants.ContainerModes.FCL));
				}
				return filter;
			}
		}

		#region AddHiddenFilters

		void AddHiddenFilters(ModuleFilterCollection collection)
		{
			ModuleFilter filter = collection.AddFlagsFilter(Descriptions.Hidden, new string[] { Descriptions.Hidden }, new GetFlagsQuery[] { GetBaseBillFilter });
			filter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			filter.SubGroup = BillOfLadingFilterProcessor;
		}

		ZQuery GetBaseBillFilter(ZBool ignored)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobShipmentSchema.JS_IsShipping, true);
			if (!Globals.IsWeb)
			{
				filter.AddToFilter(JobShipmentSchema.JS_ShipmentStatus, ShipmentStatusHelperMethods.GetBillOfLadingStageStatus());
			}

			return filter;
		}

		#endregion

		#region AddNumbersAndRefsFilters

		void AddNumbersAndRefsFilters(ModuleFilterCollection collection)
		{
			ModuleFilter filter;

			filter = collection.AddNumberFilter(Descriptions.BillOfLading, JobShipmentSchema.JS_HouseBill);
			filter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|BillOfLading", "Bill of Lading");
			filter.SubGroup = BillOfLadingFilterProcessor;

			filter = collection.AddNumberFilter(Descriptions.BookingRef, JobShipmentSchema.JS_CFSReference);
			filter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|BookingRef", "Booking Ref");
			filter.SubGroup = BillOfLadingFilterProcessor;

			collection.AddNumberFilter(Descriptions.ContainerNumber, JobContainerSchema.JC_ContainerNum).MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|ContainerNumber", "Container #");
			if (!Globals.IsWeb)
			{
				var detentionInvoiceFilter = collection.AddFountainFilter(Descriptions.DetentionInvoiceNumber, JobContainerDetentionSchema.NC_JobNumber, "DI");
				detentionInvoiceFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|DetentionInvoiceNumber", "Detention Invoice #");
				detentionInvoiceFilter.SubGroup = DetentionSubGroup;

				collection.AddNumberFilter(Descriptions.DockReceipt, JobContainerSchema.JC_DepartureDockReceipt).MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|DockReceiptNumber", "Dock Receipt #");
			}

			filter = collection.AddFountainFilter(Descriptions.ShipmentNumber, JobShipmentSchema.JS_UniqueConsignRef, "V");
			filter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|ShipmentNumber", "Shipment #");
			filter.SubGroup = BillOfLadingFilterProcessor;

			if (!Globals.IsWeb & GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Australia)
			{
				filter = new EntryNumberModuleFilter(Descriptions.EntryTypeAndNumber, GetEntryNumberFilter)
					.WithMaxLengthOf<EntryNumberModuleFilter>(CusEntryNumSchema.CE_EntryNum);
				filter.Category = FilterCategories.NumbersAndReferences;
				filter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|EntryTypeAndNumber", "Entry Type & Number");
				collection.AddCustomFilter(filter);
			}

			var referenceNumberFilter = new ReferenceNumberFilter(
				Descriptions.AdditionalReferenceNumbers,
				new ReferenceNumberFilterHelper<CommonContainer>().GetReferenceNumberFilter,
				new RefCountryCollection(Factory))
				.WithMaxLengthOf<ReferenceNumberFilter>(CusEntryNumSchema.CE_EntryNum);
			referenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|ReferenceNumbers", "Additional Reference #");
			collection.AddCustomFilter(referenceNumberFilter);

			var importRefNumberFilter = collection.AddTextFilter(Descriptions.PackLineImportReference, JobPackLinesSchema.JL_ImportRefNumber);
			importRefNumberFilter.Category = FilterCategories.NumbersAndReferences;
			importRefNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|PackLineImportReference", "Pack Line Import Reference #");
			importRefNumberFilter.SubGroup = PackLineSubGroup;

			var exportRefNumberFilter = collection.AddTextFilter(Descriptions.PackLineExportReference, JobPackLinesSchema.JL_ExportRefNumber);
			exportRefNumberFilter.Category = FilterCategories.NumbersAndReferences;
			exportRefNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|PackLineExportReference", "Pack Line Export Reference #");
			exportRefNumberFilter.SubGroup = PackLineSubGroup;
		}

		ZQuery GetEntryNumberFilter(SQLComparisonOperator opp, ZString type, ZString number)
		{
			ZDBOnlySubQuery entryFilter = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryFilter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			if (!type.IsEmpty)
			{
				entryFilter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CMRExportExemptionCodes.Get3CharCode(type));
			}

			if (!number.IsEmpty)
			{
				entryFilter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryNum, opp, number);
			}

			ZDBOnlySubQuery shipmentFilter = new ZDBOnlySubQuery(typeof(AgencyShipment), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
			shipmentFilter.AddSubQuery(entryFilter, JoinCondition.And);

			ZDBOnlyQuery containerFilter = new ZDBOnlyQuery(typeof(AgencyShipmentContainer));
			containerFilter.AddSubQuery(shipmentFilter, JoinCondition.Or);
			containerFilter.AddSubQuery(entryFilter, JoinCondition.Or);

			return containerFilter;
		}

		#endregion

		#region AddDateTimeFilters

		void AddDateTimeFilters(ModuleFilterCollection collection)
		{
			ModuleFilter filter;

			filter = collection.AddDateFilter(Descriptions.AvailabilityDate, GetAvailabilityDateFilter);
			filter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|AvailabilityDate", "Availability Date");
			filter.SubGroup = BillOfLadingFilterProcessor;

			collection.AddDateFilter(Descriptions.EmptyRequiredBy, JobContainerSchema.JC_EmptyReturnedBy).MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|EmptyRequiredBy", "Empty Required By");
			collection.AddDateFilter(Descriptions.EmptyReturned, GetEmptyReturnedDateFilter).MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|EmptyReturned", "Empty Returned");
			collection.AddDateFilter(Descriptions.WharfGateOut, GetWharfGateOutDateFilter).MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|WharfGateOut", "Wharf Gate Out");

			filter = collection.AddDateFilter(Descriptions.EstimatedTimeDeparture, GetEstimatedTimeDepartureFilter);
			filter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|EstimatedTimeDeparture", "ETD");
			filter.SubGroup = BillOfLadingFilterProcessor;

			filter = collection.AddDateFilter(Descriptions.EstimatedTimeArrival, GetEstimatedTimeArrivalFilter);
			filter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|EstimatedTimeArrival", "ETA");
			filter.SubGroup = BillOfLadingFilterProcessor;

			filter = collection.AddDateFilter(Descriptions.ActualTimeDeparture, GetActualTimeDepartureFilter);
			filter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|ActualTimeDeparture", "ATD");
			filter.SubGroup = BillOfLadingFilterProcessor;

			filter = collection.AddDateFilter(Descriptions.ActualTimeArrival, GetActualTimeArrivalFilter);
			filter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|ActualTimeArrival", "ATA");
			filter.SubGroup = BillOfLadingFilterProcessor;
		}

		ZQuery GetAvailabilityDateFilter(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery filter = new ZQuery();
			AddDateTimeRange(filter, comparisonOperator, JoinCondition.And, JobVoyDestinationSchema.JB_AvailabilityDate, fromDate, toDate);
			return ShipmentFilterFromVoyageDestinationFilter(filter);
		}

		ZQuery GetEmptyReturnedDateFilter(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery result = new ZQuery();
			var noDateEntered = comparisonOperator == DateComparisonOperator.HasNoDateEntered;
			var comparisonOperatorToUse = noDateEntered ? DateComparisonOperator.HasDateEntered : comparisonOperator;
			AddDateTimeRange(result, comparisonOperatorToUse, JoinCondition.And, JobContainerMoveSchema.E9_MovementDate, fromDate, toDate);

			if (result.IsEmpty)
			{
				return result;
			}
			else
			{
				result.AddToFilter(JobContainerMoveSchema.E9_MovementType, ContainerMovementTypes.EmptyReturnMovements);
				return FromContainerMovementFilter(result, noDateEntered);
			}
		}

		ZQuery GetWharfGateOutDateFilter(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery result = new ZQuery();
			var noDateEntered = comparisonOperator == DateComparisonOperator.HasNoDateEntered;
			var comparisonOperatorToUse = noDateEntered ? DateComparisonOperator.HasDateEntered : comparisonOperator;
			AddDateRange(result, comparisonOperatorToUse, JoinCondition.And, JobContainerMoveSchema.E9_MovementDate, fromDate.Date, toDate.Date);

			if (result.IsEmpty)
			{
				return result;
			}
			else
			{
				result.AddToFilter(JobContainerMoveSchema.E9_MovementType, ContainerMovementTypes.Codes.WharfGateOut);
				return FromContainerMovementFilter(result, noDateEntered);
			}
		}

		ZQuery GetEstimatedTimeDepartureFilter(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			return GetShipmentDateFilter(SailingFilterBuilder.Dates.ETD, comparisonOperator, fromDate, toDate);
		}

		ZQuery GetEstimatedTimeArrivalFilter(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			return GetShipmentDateFilter(SailingFilterBuilder.Dates.ETA, comparisonOperator, fromDate, toDate);
		}

		ZQuery GetActualTimeDepartureFilter(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			return GetShipmentDateFilter(SailingFilterBuilder.Dates.ATD, comparisonOperator, fromDate, toDate);
		}

		ZQuery GetActualTimeArrivalFilter(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			return GetShipmentDateFilter(SailingFilterBuilder.Dates.ATA, comparisonOperator, fromDate, toDate);
		}

		ZQuery GetShipmentDateFilter(SailingFilterBuilder.Dates dateType, DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var filterBuilder = new SailingFilterBuilder(Factory);
			filterBuilder.SetDateRange(dateType, comparisonOperator, fromDate, toDate);
			var shipmentFilter = filterBuilder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.Direct);

			// Include jobs with no sailing is attached
			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				shipmentFilter.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_JX, null);
			}

			return shipmentFilter;
		}

		#endregion

		#region AddTextFilters

		void AddVoyageVesselFilters(ModuleFilterCollection collection)
		{
			var filter = new VoyageVesselModuleFilter(Descriptions.VoyageVessel, GetVoyageVesselFilter, new RefVesselCollection(Factory))
				.WithMaxLengthOf(JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
			filter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|VoyageVessel", "Voyage / Vessel");
			filter.SubGroup = BillOfLadingFilterProcessor;
			collection.AddCustomFilter(filter);
		}

		ZQuery GetVoyageVesselFilter(SQLComparisonOperator opp, ZString voyage, ZString vessel, ZBool includeArchived)
		{
			var builder = new SailingFilterBuilder(Factory);
			builder.VoyageFlight = voyage;
			builder.Vessel = vessel;
			builder.VoyageFlightComparisonOperator = opp;

			return builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.Direct | SailingFilterBuilder.RelationshipFlags.ViaDirectTransports);
		}

		#endregion

		#region AddOrganisationsAndStaffFilters

		void AddOrganisationsAndStaffFilters(ModuleFilterCollection collection)
		{
			var consigneeFilter = collection.AddGuidFilter(Descriptions.Consignee, ModuleIDs.Organisation, GetOrgAddressQuery, new ConsigneeCollection(Factory));
			consigneeFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|Consignee", "Consignee");
			consigneeFilter.SubGroup = ConsigneeSubGroup;
			consigneeFilter.SupportsFiltersMatchComparisonOperator = false;

			var consignorFilter = collection.AddGuidFilter(Descriptions.Consignor, ModuleIDs.Organisation, GetOrgAddressQuery, new ConsignorCollection(Factory));
			consignorFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|Consignor", "Consignor");
			consignorFilter.SubGroup = ConsignorSubGroup;

			var emptyPickupFromFilter = collection.AddGuidFilter(Descriptions.EmptyPickupFrom, ModuleIDs.Organisation, GetOrgAddressQuery, new OrganisationsFindBoxCollection(Factory));
			emptyPickupFromFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|EmptyPickupFrom", "Empty Pickup From");
			emptyPickupFromFilter.SubGroup = PickupFromSubGroup;

			var emptyReturnTo = collection.AddGuidFilter(Descriptions.EmptyReturnTo, ModuleIDs.Organisation, GetOrgAddressQuery, new OrganisationsFindBoxCollection(Factory));
			emptyReturnTo.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|EmptyReturnTo", "Empty Return To");
			emptyReturnTo.SubGroup = ReturnToSubGroup;

			if (!Globals.IsWeb)
			{
				ModuleGuidFilter filter;

				filter = collection.AddGuidFilter(Descriptions.LocalClient, ModuleIDs.Organisation, GetOrgAddressQuery, new OrganisationsFindBoxCollection(Factory));
				filter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|LocalClient", "Local Client");
				filter.SubGroup = LocalClientSubGroup;

				filter = collection.AddGuidFilter(Descriptions.Principal, ModuleIDs.Organisation, JobShipmentSchema.JS_OH_DeliveryAgent, new ShipsAgencyPrincipalCollectionWithSecurityCheck(Factory));
				filter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|Principal", "Principal");
				filter.SubGroup = BillOfLadingFilterProcessor;

				if (!Env.Security.AgencyPrincipalAccess.IsAllowed)
				{
					filter.Visibility = FilterVisibility.AlwaysVisible;
					filter.PropertyValidation = PrincipalFilterValidation;
				}
			}
		}

		ZQuery GetOrgAddressQuery(ZGuid orgPK)
		{
			return new ZQuery(OrgAddressSchema.OA_OH, orgPK);
		}

		#endregion

		#region AddLocationsFilters

		void AddLocationsFilters(ModuleFilterCollection collection)
		{
			LocationCollection locations = new LocationCollection(Factory);

			if (!Globals.IsWeb)
			{
				ModuleGuidFilter branchRelatedPortsFilter = collection.AddGuidFilter(Descriptions.BranchRelatedPorts, ModuleIDs.GlbBranch, GetBranchRelatedPortsFilter, new GlbBranchCollection(Factory));
				branchRelatedPortsFilter.Category = FilterCategories.Locations;
				branchRelatedPortsFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|BranchRelatedPorts", "Branch Related Ports");
				branchRelatedPortsFilter.SubGroup = BillOfLadingFilterProcessor;

				if (!AllowSearchOfUnlocoOutsideLoginBranch)
				{
					branchRelatedPortsFilter.Visibility = FilterVisibility.AlwaysVisible;
					branchRelatedPortsFilter.DefaultProperty = GlbBranch.CurrentBranch.PK;
					branchRelatedPortsFilter.PropertyValidation = BranchFilterValidation;
				}
			}

			ModuleLocationFilter loadDischargeFilter = collection.AddLocationFilter(Descriptions.LoadDischarge, GetLoadDischargeFilter, locations, locations);
			loadDischargeFilter.SetItemDescriptions(Res.GetData("6b27de10-b636-47a7-b6a4-866280eea6a5", "Load"), Res.GetData("fa15df73-19b2-4bc5-8922-a66db1f56514", "Discharge"));
			loadDischargeFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|LoadDischarge", "Load / Discharge");
			loadDischargeFilter.SubGroup = BillOfLadingFilterProcessor;

			if (!Globals.IsWeb)
			{
				ModuleLocationFilter originDestinationFilter = collection.AddLocationFilter(Descriptions.OriginDestination, JobShipmentSchema.JS_RL_NKOrigin, locations, JobShipmentSchema.JS_RL_NKDestination, locations);
				originDestinationFilter.SetItemDescriptions(Res.GetData("4867e879-2a7b-4891-aacc-abe147717569", "Origin"), Res.GetData("5730e844-ef7d-4383-87cb-98aa94b04bef", "Destination"));
				originDestinationFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|OriginDestination", "Origin / Destination");
				originDestinationFilter.SubGroup = BillOfLadingFilterProcessor;
			}
		}

		ZQuery GetBranchRelatedPortsFilter(ZGuid branch)
		{
			ZDBOnlySubQuery homeSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.GB_RL_NKHomePort);
			homeSubQuery.AddToFilter(GlbBranchSchema.PK, branch);

			ZDBOnlySubQuery relatedSubQuery = new ZDBOnlySubQuery(typeof(GlbBranchExtraPorts), GlbBranchExtraPortsSchema.GY_RL_NKAdditionalBranchRelatedPort);
			relatedSubQuery.AddToFilter(GlbBranchExtraPortsSchema.GY_GB, branch);
			relatedSubQuery.AddAsUnionQuery(homeSubQuery);

			ZDBOnlyQuery dischargeFilter = new ZDBOnlyQuery(typeof(VoyageDestination));
			dischargeFilter.AddSubQuery(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, relatedSubQuery, JoinCondition.And);

			return ShipmentFilterFromVoyageDestinationFilter(dischargeFilter);
		}

		ZQuery GetLoadDischargeFilter(ZString load, ZString discharge)
		{
			SailingFilterBuilder builder = new SailingFilterBuilder(Factory);
			builder.LoadPort = load;
			builder.DischargePort = discharge;
			return builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.Direct);
		}

		#endregion

		#region AddStatusAndFlagsFilters

		void AddStatusAndFlagsFilters(ModuleFilterCollection collection)
		{
			if (!Globals.IsWeb)
			{
				ModuleTextFilter availabilityStatusFilter = collection.AddTextFilter(Descriptions.AvailabilityStatus, GetAvailabilityStatusFilter, new AvailabilityStatus());
				availabilityStatusFilter.Category = FilterCategories.StatusAndFlags;
				availabilityStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|AvailabilityStatus", "Availability Status");
				availabilityStatusFilter.SubGroup = BillOfLadingFilterProcessor;
			}

			if (AgencyRegistry.Instance.EIDOMessagingDetails.Value.Identities.Count > 0)
			{
				ModuleTextFilter eIDOStatusFilter = collection.AddTextFilter(Descriptions.EIDOStatus, EIDOStatusFilterHelper.GetEIDOStatusFilter, new EIDOFilterList());
				eIDOStatusFilter.Category = FilterCategories.StatusAndFlags;
				eIDOStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|EIDOStatus", "E-IDO Status");
			}

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.NewZealand)
			{
				var importReleaseOrderStatusFilter = collection.AddTextFilter(Descriptions.ImportReleaseOrderStatus, ImportReleaseOrderFilterHelper.GetFilterQuery, new ImportReleaseOrderFilterList());
				importReleaseOrderStatusFilter.Category = FilterCategories.StatusAndFlags;
				importReleaseOrderStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|ImportReleaseOrderStatus", "Import Release Order Status");
			}

			if (!Globals.IsWeb)
			{
				ModuleTextFilter exportInvoiceStatusFilter = collection.AddTextFilter(Descriptions.ExportInvoiceStatus, GetExportInvoiceStatusFilter, new InvoiceStatus());
				exportInvoiceStatusFilter.Category = FilterCategories.StatusAndFlags;
				exportInvoiceStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|ExportInvoiceStatus", "Export Detention Invoice Status");

				ModuleTextFilter importInvoiceStatusFilter = collection.AddTextFilter(Descriptions.ImportInvoiceStatus, GetImportInvoiceStatusFilter, new InvoiceStatus());
				importInvoiceStatusFilter.Category = FilterCategories.StatusAndFlags;
				importInvoiceStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|ImportInvoiceStatus", "Import Detention Invoice Status");

				ModuleTextFilter returnedStatusFilter = collection.AddTextFilter(Descriptions.ReturnedStatus, GetReturnedStatusFilter, new ReturnedStatus());
				returnedStatusFilter.Category = FilterCategories.StatusAndFlags;
				returnedStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|ReturnedStatus", "Returned Status");
			}
		}

		ZQuery GetAvailabilityStatusFilter(ZString availabilityStatus)
		{
			switch (availabilityStatus)
			{
				case AvailabilityStatus.Available:
					return ShipmentFilterFromVoyageDestinationFilter(new ZQuery(JobVoyDestinationSchema.JB_A_ARV, SQLComparisonOperator.NotEqual, null));

				case AvailabilityStatus.NotAvailable:
					return ShipmentFilterFromVoyageDestinationFilter(new ZQuery(JobVoyDestinationSchema.JB_A_ARV, SQLComparisonOperator.Equal, null));

				default:
					return new ZQuery();
			}
		}

		ZQuery GetExportInvoiceStatusFilter(ZString invoicedStatus)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobContainerMoveSchema.E9_NC, SQLComparisonOperator.NotEqual, null);
			filter.AddToFilter(JobContainerMoveSchema.E9_MovementType, ContainerMovementTypes.Codes.WharfGateIn);

			switch (invoicedStatus)
			{
				case InvoiceStatus.Invoiced:
					return FromContainerMovementFilter(filter);
				case InvoiceStatus.NotInvoiced:
					return FromContainerMovementFilter(filter, true);
				default:
					return new ZQuery();
			}
		}

		ZQuery GetImportInvoiceStatusFilter(ZString invoicedStatus)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobContainerMoveSchema.E9_NC, SQLComparisonOperator.NotEqual, null);
			filter.AddToFilter(JobContainerMoveSchema.E9_MovementType, ContainerMovementTypes.Codes.YardGateIn);

			switch (invoicedStatus)
			{
				case InvoiceStatus.Invoiced:
					return FromContainerMovementFilter(filter);
				case InvoiceStatus.NotInvoiced:
					return FromContainerMovementFilter(filter, true);
				default:
					return new ZQuery();
			}
		}

		ZQuery GetReturnedStatusFilter(ZString returnedStatus)
		{
			switch (returnedStatus)
			{
				case ReturnedStatus.Returned:
					{
						ZQuery movementQuery = new ZQuery();
						movementQuery.AddToFilter(JobContainerMoveSchema.E9_MovementDate, SQLComparisonOperator.NotEqual, null);
						movementQuery.AddToFilter(JobContainerMoveSchema.E9_MovementType, ContainerMovementTypes.EmptyReturnMovements);

						return FromContainerMovementFilter(movementQuery);
					}

				case ReturnedStatus.NotReturned:
					{
						ZQuery movementQuery = new ZQuery();
						movementQuery.AddToFilter(JobContainerMoveSchema.E9_MovementDate, SQLComparisonOperator.NotEqual, null);
						movementQuery.AddToFilter(JobContainerMoveSchema.E9_MovementType, ContainerMovementTypes.EmptyReturnMovements);

						return FromContainerMovementFilter(movementQuery, true);
					}

				default:
					return new ZQuery();
			}
		}

		#endregion

		#region ModesAndTypesFilters

		void AddModesAndTypesFilters(ModuleFilterCollection collection)
		{
			if (!Globals.IsWeb)
			{
				var commodityCodeFilter = collection.AddTextFilter(Descriptions.CommodityCode, GetCommodityCodeFilter, new RefCommodityCodeCollection(Factory))
					.WithMaxLengthOf<ModuleTextFilter>(JobPackLinesSchema.JL_RH_NKCommodityCode);
				commodityCodeFilter.Category = FilterCategories.ModesAndTypes;
				commodityCodeFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|CommodityCode", "Commodity Code");
				commodityCodeFilter.SubGroup = CommodityCodeSubGroup;

				var containerTypeFilter = collection.AddGuidFilter(Descriptions.ContainerType, ModuleIDs.RefContainer, JobContainerSchema.JC_RC, new RefContainerCollection(Factory));
				containerTypeFilter.Category = FilterCategories.ModesAndTypes;
				containerTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|ContainerType", "Container Type");

				var containerTypeCategoryFilter = collection.AddTextFilter(Descriptions.ContainerTypeCategory, RefContainerSchema.RC_ContainerType, new CodeDescriptionPairList(OLookUpEditType.ContainerType));
				containerTypeCategoryFilter.Category = FilterCategories.ModesAndTypes;
				containerTypeCategoryFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|ContainerTypeCategory", "Container Type Category");
				containerTypeCategoryFilter.SubGroup = ContainerTypeSubGroup;

				var containerQualityFilter = collection.AddTextFilter(Descriptions.ContainerQuality, JobContainerSchema.JC_ContainerQuality, FreightDataRegistry.Instance.ContainerQualityList.Value);
				containerQualityFilter.Category = FilterCategories.ModesAndTypes;
				containerQualityFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|ContainerQuality", "Container Quality");

				var emptyContainerFilter = collection.AddTextFilter(Descriptions.EmptyContainer, GetEmptyStatusFilter, new EmptyStatus());
				emptyContainerFilter.Category = FilterCategories.ModesAndTypes;
				emptyContainerFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|EmptyContainer", "Empty Container");

				var shipperOwnedFilter = collection.AddTextFilter(Descriptions.ShipperOwned, GetShipperOwnedStatusFilter, new ShipperOwnedStatus());
				shipperOwnedFilter.Category = FilterCategories.ModesAndTypes;
				shipperOwnedFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|ShipperOwned", "Shipper Owned");

				var storageClassFilter = collection.AddTextFilter(Descriptions.StorageClass, RefContainerSchema.RC_StorageClass, new CodeDescriptionPairList(OLookUpEditType.ContainerStorageClass));
				storageClassFilter.Category = FilterCategories.ModesAndTypes;
				storageClassFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|StorageClass", "Storage Class");
				storageClassFilter.SubGroup = ContainerTypeSubGroup;
			}

			ModuleFilter containerModeFilter = collection.AddTextFilter(Descriptions.ContainerMode, JobContainerSchema.JC_ContainerMode, new AgencyShipmentContainerModeList(Core.Constants.ContainerModes.FCL));
			containerModeFilter.Category = FilterCategories.ModesAndTypes;
			containerModeFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|BillContainersFilter|ContainerMode", "Container Mode");
		}

		ZQuery GetCommodityCodeFilter(SQLComparisonOperator opp, ZString commodityCode)
		{
			var result = new ZDBOnlyQuery(typeof(PackLine));
			result.AddToFilter(JobPackLinesSchema.JL_RH_NKCommodityCode, opp, commodityCode);

			return result;
		}

		ZQuery GetEmptyStatusFilter(ZString emptyStatus)
		{
			switch (emptyStatus)
			{
				case EmptyStatus.Empty:
					return new ZQuery(JobContainerSchema.JC_IsEmptyContainer, true);
				case EmptyStatus.NotEmpty:
					return new ZQuery(JobContainerSchema.JC_IsEmptyContainer, false);
				default:
					return new ZQuery();
			}
		}

		ZQuery GetShipperOwnedStatusFilter(ZString shipperOwned)
		{
			switch (shipperOwned)
			{
				case ShipperOwnedStatus.ShipperOwned:
					return new ZQuery(JobContainerSchema.JC_IsShipperOwned, true);

				case ShipperOwnedStatus.NotShipperOwned:
					return new ZQuery(JobContainerSchema.JC_IsShipperOwned, false);

				default:
					return new ZQuery();
			}
		}

		#endregion

		#region Sub Groups

		public ModuleFilterSubGroup BillOfLadingFilterProcessor
		{
			get { return billOfLadingFilterProcessor ?? (billOfLadingFilterProcessor = new BillOfLadingSubGroup()); }
		}
		BillOfLadingSubGroup billOfLadingFilterProcessor;

		ModuleFilterSubGroup DetentionSubGroup
		{
			get { return detentionSubGroup ?? (detentionSubGroup = new DetentionFilterSubGroup()); }
		}
		DetentionFilterSubGroup detentionSubGroup;

		ModuleFilterSubGroup DocAddressSubGroup
		{
			get { return docAddressSubGroup ?? (docAddressSubGroup = new DocAddressFilterSubGroup(BillOfLadingFilterProcessor)); }
		}
		DocAddressFilterSubGroup docAddressSubGroup;

		ModuleFilterSubGroup ConsignorSubGroup
		{
			get { return consignorSubGroup ?? (consignorSubGroup = new ConsignorFilterSubGroup(DocAddressSubGroup)); }
		}
		ConsignorFilterSubGroup consignorSubGroup;

		ModuleFilterSubGroup ConsigneeSubGroup
		{
			get { return consigneeSubGroup ?? (consigneeSubGroup = new ConsigneeFilterSubGroup(DocAddressSubGroup)); }
		}
		ConsigneeFilterSubGroup consigneeSubGroup;

		ModuleFilterSubGroup PickupFromSubGroup
		{
			get { return pickupFromSubGroup ?? (pickupFromSubGroup = new PickupFromFilterSubGroup()); }
		}
		PickupFromFilterSubGroup pickupFromSubGroup;

		ModuleFilterSubGroup ReturnToSubGroup
		{
			get { return returnToSubGroup ?? (returnToSubGroup = new ReturnToFilterSubGroup()); }
		}
		ReturnToFilterSubGroup returnToSubGroup;

		ModuleFilterSubGroup LocalClientSubGroup
		{
			get { return localClientSubGroup ?? (localClientSubGroup = new LocalClientFilterSubGroup(BillOfLadingFilterProcessor)); }
		}
		LocalClientFilterSubGroup localClientSubGroup;

		ModuleFilterSubGroup CommodityCodeSubGroup
		{
			get { return commodityCodeSubGroup ?? (commodityCodeSubGroup = new CommodityCodeFilterSubGroup()); }
		}
		CommodityCodeFilterSubGroup commodityCodeSubGroup;

		ModuleFilterSubGroup ContainerTypeSubGroup
		{
			get { return containerTypeSubGroup ?? (containerTypeSubGroup = new ContainerTypeFilterSubGroup()); }
		}
		ContainerTypeFilterSubGroup containerTypeSubGroup;

		ModuleFilterSubGroup PackLineSubGroup
		{
			get { return packLineSubGroup ?? (packLineSubGroup = new PackLineFilterSubGroup()); }
		}
		PackLineFilterSubGroup packLineSubGroup;

		class BillOfLadingSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var jobShipment = new ZDBOnlySubQuery(typeof(AgencyShipment), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
				jobShipment.AddToFilter(filter);

				var jobContainer = new ZDBOnlyQuery(typeof(AgencyShipmentContainer));
				jobContainer.AddSubQuery(jobShipment, JoinCondition.And);

				return jobContainer;
			}
		}

		class DetentionFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var detentionQuery = new ZDBOnlySubQuery(typeof(ContainerDetention), JobContainerMoveSchema.E9_NC);

				detentionQuery.AddToFilter(filter);
				detentionQuery.AddToFilter(JobContainerDetentionSchema.NC_GC, GlbCompany.CurrentCompany.PK);

				var movementQuery = new ZDBOnlyQuery(typeof(ContainerMovement));
				movementQuery.AddSubQuery(detentionQuery, JoinCondition.And);

				return FromContainerMovementFilter(movementQuery);
			}
		}

		class DocAddressFilterSubGroup : ModuleFilterSubGroup
		{
			public DocAddressFilterSubGroup(ModuleFilterSubGroup parent)
				: base(parent) { }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var shipmentQuery = new ZDBOnlyQuery(typeof(AgencyShipment));

				var docAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
				docAddressQuery.AddToFilter(filter);

				shipmentQuery.AddSubQuery(docAddressQuery, JoinCondition.And);

				return shipmentQuery;
			}
		}

		class ConsigneeFilterSubGroup : ModuleFilterSubGroup
		{
			public ConsigneeFilterSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return GetDocAddressQueryForSubGroup(DocAddressTypes.Codes.ConsigneeDocumentaryAddress, filter);
			}
		}

		class ConsignorFilterSubGroup : ModuleFilterSubGroup
		{
			public ConsignorFilterSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return GetDocAddressQueryForSubGroup(DocAddressTypes.Codes.ConsignorDocumentaryAddress, filter);
			}
		}

		class PickupFromFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobContainerSchema.JC_OA_DepartureContainerYardAddress);
				orgAddressQuery.AddToFilter(filter);

				var containerQuery = new ZDBOnlyQuery(typeof(AgencyShipmentContainer));
				containerQuery.AddSubQuery(orgAddressQuery, JoinCondition.And);

				return containerQuery;
			}
		}

		class ReturnToFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobContainerSchema.JC_OA_ArrivalContainerYardAddress);
				orgAddressQuery.AddToFilter(filter);

				var containerQuery = new ZDBOnlyQuery(typeof(AgencyShipmentContainer));
				containerQuery.AddSubQuery(orgAddressQuery, JoinCondition.And);

				return containerQuery;
			}
		}

		class LocalClientFilterSubGroup : ModuleFilterSubGroup
		{
			public LocalClientFilterSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var shipmentQuery = new ZDBOnlyQuery(typeof(AgencyShipment));

				var addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr);
				addressQuery.AddToFilter(filter);

				var jobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
				jobHeaderQuery.AddSubQuery(addressQuery, JoinCondition.And);

				shipmentQuery.AddSubQuery(JobShipmentSchema.PK, jobHeaderQuery, JoinCondition.And);

				return shipmentQuery;
			}
		}

		class CommodityCodeFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var containerCommodityCodeFilterText = filter.LiteralTextADO.Replace(JobPackLinesSchema.JL_RH_NKCommodityCode.Name, JobContainerSchema.JC_RH_NKContainerCommodityCode.Name);
				var containerCommodityCodeFilter = new ZQuery().AddFilterAndZSQLParameterCollection(containerCommodityCodeFilterText, new ZSqlParameterCollection());

				var packlineQuery = new ZDBOnlySubQuery(typeof(AgencyShipmentPackLine), JobContainerPackPivotSchema.J6_JL);
				packlineQuery.AddToFilter(filter);

				var pivotQuery = new ZDBOnlySubQuery(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JC);
				pivotQuery.AddSubQuery(packlineQuery, JoinCondition.And);

				var containerQuery = new ZDBOnlyQuery(typeof(AgencyShipmentContainer));
				containerQuery.AddSubQuery(pivotQuery, JoinCondition.And);
				containerQuery.AddToFilter(containerCommodityCodeFilter, JoinCondition.Or);

				return containerQuery;
			}
		}

		class ContainerTypeFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var containerTypeQUery = new ZDBOnlySubQuery(typeof(RefContainer), JobContainerSchema.JC_RC);
				containerTypeQUery.AddToFilter(filter);

				var containerQuery = new ZDBOnlyQuery(typeof(AgencyShipmentContainer));
				containerQuery.AddSubQuery(containerTypeQUery, JoinCondition.And);

				return containerQuery;
			}
		}

		class PackLineFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(AgencyShipmentContainer));

				var packLineSubQuery = new ZDBOnlySubQuery(typeof(AgencyShipmentPackLine), JobContainerPackPivotSchema.J6_JL);
				packLineSubQuery.AddToFilter(filter);

				var packLinePivotQuery = new ZDBOnlySubQuery(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JC);
				packLinePivotQuery.AddSubQuery(packLineSubQuery, JoinCondition.And);

				result.AddSubQuery(packLinePivotQuery, JoinCondition.And);

				return result;
			}
		}

		#endregion

		#region Implementation

		bool AllowSearchOfUnlocoOutsideLoginBranch
		{
			get { return Env.Security.AgencyBillContainersAllowSearchOfUnlocoOutsideLoginBranch.IsAllowed; }
		}

		#region Validation

		void PrincipalFilterValidation(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				info.AddError(Res.GetString("a823b4c5-5266-4581-9547-69b370e2b67a", "Please select a principal to filter by"));
			}
		}

		void BranchFilterValidation(ZPropertyInfo info)
		{
			if (!info.Value.Equals(GlbBranch.CurrentBranch.PK))
			{
				info.AddError(Res.GetString("7737bcf2-4136-459e-b1cd-52439f40fb5f", "Your current security rights only allow you to view shipments relating to your current login branch.\r\nIf you think this is incorrect, please contact your system administrator."));
			}
		}

		#endregion

		#region From*Filter

		static ZQuery ShipmentFilterFromSailingFilter(ZQuery sailingFilter)
		{
			ZDBOnlySubQuery sailing = new ZDBOnlySubQuery(typeof(JobSailing), JobShipmentSchema.JS_JX);
			sailing.AddToFilter(sailingFilter);

			ZDBOnlyQuery jobShipment = new ZDBOnlyQuery(typeof(AgencyShipment));
			jobShipment.AddSubQuery(sailing, JoinCondition.And);

			return jobShipment;
		}

		static ZQuery ShipmentFilterFromVoyageDestinationFilter(ZQuery voyageDestinationFilter)
		{
			ZDBOnlySubQuery destination = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
			destination.AddToFilter(voyageDestinationFilter);

			ZDBOnlyQuery sailing = new ZDBOnlyQuery(typeof(JobSailing));
			sailing.AddSubQuery(destination, JoinCondition.And);

			return ShipmentFilterFromSailingFilter(sailing);
		}

		static ZQuery FromContainerMovementFilter(ZQuery containerMovementFilter)
		{
			return FromContainerMovementFilter(containerMovementFilter, false);
		}

		static ZQuery FromContainerMovementFilter(ZQuery containerMovementFilter, bool notIn)
		{
			ZDBOnlySubQuery stock = new ZDBOnlySubQuery(typeof(RefContainerStock), JobContainerMoveSchema.E9_R6);
			stock.AddToFilter(RefContainerStockSchema.R6_ContainerNum, SQLComparisonOperator.Equal, JobContainerSchema.JC_ContainerNum);

			ZDBOnlySubQuery movement = new ZDBOnlySubQuery(typeof(ContainerMovement), JobContainerMoveSchema.E9_JV);
			movement.AddToFilter(containerMovementFilter);
			movement.AddSubQuery(stock, JoinCondition.And);

			ZDBOnlySubQuery origin = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
			origin.AddSubQuery(JobVoyOriginSchema.JA_JV, movement, JoinCondition.And);

			ZDBOnlySubQuery sailing = new ZDBOnlySubQuery(typeof(JobSailing), JobShipmentSchema.JS_JX);
			sailing.AddSubQuery(origin, JoinCondition.And);

			ZDBOnlySubQuery shipment = new ZDBOnlySubQuery(typeof(AgencyShipment), JobContainerSchema.JC_JS_FCLBookingOnlyLink, notIn);
			shipment.AddSubQuery(sailing, JoinCondition.And);

			ZDBOnlyQuery container = new ZDBOnlyQuery(typeof(AgencyShipmentContainer));
			container.AddSubQuery(shipment, JoinCondition.And);

			return container;
		}

		static ZQuery GetDocAddressQueryForSubGroup(ZString addressType, ZQuery addressSubQuery)
		{
			var docAddressSubQuery = new ZDBOnlyQuery(typeof(JobDocAddress));
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

			orgAddressSubQuery.AddToFilter(addressSubQuery);
			docAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, addressType);
			docAddressSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

			return docAddressSubQuery;
		}

		#endregion

		#region Code Lists

		public class ShipperOwnedStatus : ReadOnlyCodeDescriptionPairList
		{
			public const string ShipperOwned = "SHP";
			public const string NotShipperOwned = "NSH";

			public ShipperOwnedStatus()
			{
				Elements.Add(new CodeDescriptionPair(ShipperOwned, Res.GetString("901e39cd-0874-4e97-b7a1-6177ac22b41e", "Shipper Owned")));
				Elements.Add(new CodeDescriptionPair(NotShipperOwned, Res.GetString("4744b13f-b523-49a9-95e2-c4415c1c2dfb", "Not Shipper Owned")));
			}
		}

		public class AvailabilityStatus : ReadOnlyCodeDescriptionPairList
		{
			public const string Available = "AVL";
			public const string NotAvailable = "NAV";

			public AvailabilityStatus()
			{
				Elements.Add(new CodeDescriptionPair(Available, Res.GetString("65ddcc75-7193-467d-8ef4-2366fc1d6f94", "Available")));
				Elements.Add(new CodeDescriptionPair(NotAvailable, Res.GetString("2213e12e-653f-4ba4-a9b6-3fb688746112", "Not Available")));
			}
		}

		public class ReturnedStatus : ReadOnlyCodeDescriptionPairList
		{
			public const string Returned = "RET";
			public const string NotReturned = "NRE";

			public ReturnedStatus()
			{
				Elements.Add(new CodeDescriptionPair(Returned, Res.GetString("4ef8ee11-c55c-4e9f-9bf3-ad26a9ba971b", "Returned")));
				Elements.Add(new CodeDescriptionPair(NotReturned, Res.GetString("02b63f60-e3fc-47a4-bb1c-ad4b9f9b9d85", "Not Returned")));
			}
		}

		public class InvoiceStatus : ReadOnlyCodeDescriptionPairList
		{
			public const string Invoiced = "INV";
			public const string NotInvoiced = "NIV";

			public InvoiceStatus()
			{
				Elements.Add(new CodeDescriptionPair(Invoiced, Res.GetString("1355f0d0-3e04-4719-b1e6-c67b962c60c5", "Invoiced")));
				Elements.Add(new CodeDescriptionPair(NotInvoiced, Res.GetString("21748e7b-8ead-4947-9f59-2551cd561cac", "Not Invoiced")));
			}
		}

		public class EmptyStatus : ReadOnlyCodeDescriptionPairList
		{
			public const string Empty = "EMP";
			public const string NotEmpty = "NTE";

			public EmptyStatus()
			{
				Elements.Add(new CodeDescriptionPair(Empty, Res.GetString("7b7a4e40-3cd7-46d9-980c-84cd50fc51ee", "Empty")));
				Elements.Add(new CodeDescriptionPair(NotEmpty, Res.GetString("0674b205-014a-4323-adf8-71466f56ac0b", "Not Empty")));
			}
		}

		#endregion

		#endregion
	}
}



