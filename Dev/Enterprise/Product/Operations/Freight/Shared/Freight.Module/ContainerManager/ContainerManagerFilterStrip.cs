using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Module
{
	public class ContainerManagerFilterStrip : FilterStripBusinessObject
	{
		#region Descriptions

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public static class Descriptions
		{
			public const string ArrivalSlotReference = "Arrival Slot Reference";
			public const string ContainerNumber = "Container #";
			public const string DepartureSlotReference = "Departure Slot Reference";
			public const string Seal = "Seal";
			public const string AdditionalReferenceNumbers = "Additional Reference #";

			public const string ContainerLocation = "Container Location";
			public const string ContainerStatus = "Container Status";
			public const string VGMStatus = "VGM Status";
			public const string ContainerQuality = "Container Quality";

			public const string CartageCompany = "Local Transport Company";

			public const string OriginDestination = "Origin / Destination";

			public const string Availiable = "Availiable";
			public const string ContainerYardGateIn = "Container Yard Gate In";
			public const string ContainerYardGateOut = "Container Yard Gate Out";
			public const string OnBoard = "On Board";
			public const string Storage = "Storage";
			public const string Unloaded = "Unloaded";
			public const string WharfGateIn = "Wharf Gate In";
			public const string WharfGateOut = "Wharf Gate Out";
			public const string RelatedConsol = "Related Consolidation";

			public const string AllocationID = "Allocation ID";
		}

		#endregion

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			AddNumberFilters(filters);
			AddTextFilters(filters);
			AddLocationFilters(filters);
			AddDateFilters(filters);
			AddRelatedConsolFilter(filters);
			AddModeFilters(filters);

			return filters;
		}

		#region AddFilters

		#region AddNumberFilters

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(Descriptions.ArrivalSlotReference, JobContainerSchema.JC_ArrivalSlotReference).MultilingualDescription = ResString.GetMultilingualString("ContainerManagerFilter|ArrivalSlotReference", "Arrival Slot Reference");
			filters.AddNumberFilter(Descriptions.ContainerNumber, JobContainerSchema.JC_ContainerNum).MultilingualDescription = ResString.GetMultilingualString("ContainerManagerFilter|ContainerNumber", "Container #");
			filters.AddNumberFilter(Descriptions.DepartureSlotReference, JobContainerSchema.JC_DepartureSlotReference).MultilingualDescription = ResString.GetMultilingualString("ContainerManagerFilter|DepartureSlotReference", "Departure Slot Reference");
			filters.AddNumberFilter(Descriptions.Seal, JobContainerSchema.JC_SealNum).MultilingualDescription = ResString.GetMultilingualString("ContainerManagerFilter|Seal", "Seal");
			if (ContractsPermissions.IsAllocationsVisible())
			{
				filters.AddNumberFilter(Descriptions.AllocationID, GetAllocationQuery)
					.WithMaxLengthOf<ModuleNumberFilter>(RatingContractAllocationLineSchema.RCA_AllocationLineID)
					.MultilingualDescription = ResString.GetMultilingualString("ContainerManagerFilter|AllocationID", "Allocation ID");
			}

			var referenceNumberFilter = new ReferenceNumberFilter(
				Descriptions.AdditionalReferenceNumbers,
				new ReferenceNumberFilterHelper<CommonContainer>().GetReferenceNumberFilter,
				new RefCountryCollection(Factory))
				.WithMaxLengthOf<ReferenceNumberFilter>(CusEntryNumSchema.CE_EntryNum);
			referenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("ContainerManagerFilter|ReferenceNumbers", "Additional Reference #");
			filters.AddCustomFilter(referenceNumberFilter);
		}

		#endregion

		ZQuery GetAllocationQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CommonContainer));

			if (comparisonOperator == SQLComparisonOperator.IsBlank || comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				result.AddToFilter(JobContainerSchema.JC_RCA_AllocationLine, comparisonOperator == SQLComparisonOperator.IsBlank ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, null);
			}
			else
			{
				var allocationSubQuery = new ZDBOnlySubQuery(typeof(IRatingContractAllocationLine), JobContainerSchema.JC_RCA_AllocationLine);
				allocationSubQuery.AddToFilter_PossiblyCommaSeparated(RatingContractAllocationLineSchema.RCA_AllocationLineID, comparisonOperator, value);

				result.AddSubQuery(allocationSubQuery, JoinCondition.And);
			}

			return result;
		}

		#region AddTextFilters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(Descriptions.ContainerLocation, GetContainerLocationFilter, ContainerLocation_List).MultilingualDescription = ResString.GetMultilingualString("ContainerManagerFilter|ContainerLocation", "Container Location");
			filters.AddTextFilter(Descriptions.ContainerStatus, JobContainerSchema.JC_ContainerStatus, FreightDataRegistry.Instance.ContainerStatusList.Value).MultilingualDescription = ResString.GetMultilingualString("ContainerManagerFilter|ContainerStatus", "Container Status");
			filters.AddTextFilter(Descriptions.VGMStatus, JobContainerSchema.JC_GrossWeightVerificationStatus, VGMStatus_List).MultilingualDescription = ResString.GetMultilingualString("ContainerManagerFilter|VGMStatus", "VGM Status");
		}

		#region GetContainerLocationFilter

		ZQuery GetContainerLocationFilter(ZString status)
		{
			ZQuery result = new ZQuery();

			switch (status)
			{
				case ContainerLocation.ImpReturnToCY:
					result.AddToFilter(JobContainerSchema.JC_ContainerYardEmptyReturnGateIn, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
					break;

				case ContainerLocation.ImpNotReturnToCY:
					result.AddToFilter(JobContainerSchema.JC_ContainerYardEmptyReturnGateIn, SQLComparisonOperator.Equal, ZDateTime.Empty);
					break;

				case ContainerLocation.ExpOnboard:
					result.AddToFilter(JobContainerSchema.JC_FCLOnBoardVessel, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
					break;

				case ContainerLocation.ExpNotOnboard:
					result.AddToFilter(JobContainerSchema.JC_FCLOnBoardVessel, SQLComparisonOperator.Equal, ZDateTime.Empty);
					break;
			}

			return result;
		}

		#endregion

		#endregion

		#region AddLocationFilters

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			ModuleLocationFilter originDestination = filters.AddLocationFilter(Descriptions.OriginDestination, GetOriginDestinationFilter, Locations, Locations);
			originDestination.SetItemDescriptions(Res.GetData("ModuleFilter|Common|Location|Origin", "Origin"), Res.GetData("ModuleFilter|Common|Location|Destination", "Destination"));
			originDestination.MultilingualDescription = ResString.GetMultilingualString("ContainerManagerFilter|OriginDestination", "Origin / Destination");
		}

		#region GetOriginDestinationFilter

		ZQuery GetOriginDestinationFilter(ZString origin, ZString destination)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CommonContainer));

			result.AddSubQuery(GetOriginDestinationFilter_JobDeclaration(origin, destination), JoinCondition.Or);
			result.AddSubQuery(GetOriginDestinationFilter_Consol(origin, destination), JoinCondition.Or);
			result.AddSubQuery(GetOriginDestinationFilter_Shipment(origin, destination), JoinCondition.Or);
			result.AddSubQuery(GetOriginDestinationFilter_Sailing(origin, destination), JoinCondition.Or);
			result.AddSubQuery(GetOriginDestinationFilter_Cartage_Declaration(origin, destination), JoinCondition.Or);
			result.AddSubQuery(GetOriginDestinationFilter_Cartage_Shipment(origin, destination), JoinCondition.Or);

			return result;
		}

		/// <summary>
		/// JobContainer -> CusContainer -> JobDeclaration
		/// </summary>
		ZDBOnlySubQuery GetOriginDestinationFilter_JobDeclaration(ZString origin, ZString destination)
		{
			ZDBOnlySubQuery declarationFilter = new ZDBOnlySubQuery(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>(), CusContainerSchema.CO_JE);
			declarationFilter.AddToFilter(PortFilter(JobDeclarationSchema.JE_RL_NKPortOfLoading, origin));
			declarationFilter.AddToFilter(PortFilter(JobDeclarationSchema.JE_RL_NKFinalDestination, destination));

			ZDBOnlySubQuery containerFilter = new ZDBOnlySubQuery(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(), CusContainerSchema.CO_JC);
			containerFilter.AddSubQuery(declarationFilter, JoinCondition.And);

			return containerFilter;
		}

		/// <summary>
		/// JobContainer -> JobConsol
		/// </summary>
		ZDBOnlySubQuery GetOriginDestinationFilter_Consol(ZString origin, ZString destination)
		{
			ZDBOnlySubQuery consolFilter = new ZDBOnlySubQuery(typeof(CommonConsol), JobContainerSchema.JC_JK);
			consolFilter.AddToFilter(PortFilter(JobConsolSchema.JK_RL_NKLoadPort, origin));
			consolFilter.AddToFilter(PortFilter(JobConsolSchema.JK_RL_NKDischargePort, destination));

			ZDBOnlySubQuery containerFilter = new ZDBOnlySubQuery(typeof(CommonContainer), JobContainerSchema.PK);
			containerFilter.AddSubQuery(consolFilter, JoinCondition.And);

			return containerFilter;
		}

		/// <summary>
		/// JobContainer -> JobShipment
		/// </summary>
		ZDBOnlySubQuery GetOriginDestinationFilter_Shipment(ZString origin, ZString destination)
		{
			ZDBOnlySubQuery shipmentFilter = new ZDBOnlySubQuery(typeof(CommonShipment), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
			shipmentFilter.AddToFilter(PortFilter(JobShipmentSchema.JS_RL_NKOrigin, origin));
			shipmentFilter.AddToFilter(PortFilter(JobShipmentSchema.JS_RL_NKDestination, destination));

			ZDBOnlySubQuery containerFilter = new ZDBOnlySubQuery(typeof(CommonContainer), JobContainerSchema.PK);
			containerFilter.AddSubQuery(shipmentFilter, JoinCondition.And);

			return containerFilter;
		}

		/// <summary>
		/// JobContainer -> JobSailing -> JobVoyOrigin
		/// </summary>
		ZDBOnlySubQuery GetOriginDestinationFilter_Sailing(ZString origin, ZString destination)
		{
			SailingFilterBuilder builder = new SailingFilterBuilder(Factory);
			builder.LoadPort = origin;
			builder.DischargePort = destination;

			ZDBOnlySubQuery sailingFilter = new ZDBOnlySubQuery(typeof(JobSailing), JobContainerSchema.JC_JX);
			sailingFilter.AddToFilter(builder.ToSailingFilter());

			ZDBOnlySubQuery containerFilter = new ZDBOnlySubQuery(typeof(CommonContainer), JobContainerSchema.PK);
			containerFilter.AddSubQuery(sailingFilter, JoinCondition.And);

			return containerFilter;
		}

		/// <summary>
		/// JobCartage-> JobBookedCtgMove -> JobCartage -> JobDocsAndCartage -> JobDeclaration
		/// </summary>
		ZDBOnlySubQuery GetOriginDestinationFilter_Cartage_Declaration(ZString origin, ZString destination)
		{
			ZDBOnlySubQuery declarationFilter = new ZDBOnlySubQuery(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>(), JobCartageSchema.JJ_ParentID);
			declarationFilter.AddToFilter(PortFilter(JobDeclarationSchema.JE_RL_NKPortOfLoading, origin));
			declarationFilter.AddToFilter(PortFilter(JobDeclarationSchema.JE_RL_NKFinalDestination, destination));

			ZDBOnlySubQuery cartageFilter = new ZDBOnlySubQuery(ObjectFactory.GetType<ICommonCartage>(), JobBookedCtgMoveSchema.EW_JJ);
			cartageFilter.AddToFilter(JobCartageSchema.JJ_ParentTableCode, JobDeclarationSchema.Constants.Prefix);
			cartageFilter.AddSubQuery(declarationFilter, JoinCondition.And);

			ZDBOnlySubQuery pivotFilter = new ZDBOnlySubQuery(ObjectFactory.GetType<ICommonBookedCtgMove>(), JobBookedCtgMoveSchema.EW_JC_Container);
			pivotFilter.AddSubQuery(cartageFilter, JoinCondition.And);

			ZDBOnlySubQuery containerFilter = new ZDBOnlySubQuery(typeof(CommonContainer), JobContainerSchema.PK);
			containerFilter.AddSubQuery(pivotFilter, JoinCondition.And);

			return containerFilter;
		}

		/// <summary>
		/// JobContainer -> JobBookedCtgMove -> JobCartage -> JobDocsAndCartage -> JobShipment
		/// </summary>
		ZDBOnlySubQuery GetOriginDestinationFilter_Cartage_Shipment(ZString origin, ZString destination)
		{
			ZDBOnlySubQuery shipmentFilter = new ZDBOnlySubQuery(typeof(CommonShipment), JobCartageSchema.JJ_ParentID);
			shipmentFilter.AddToFilter(PortFilter(JobShipmentSchema.JS_RL_NKOrigin, origin));
			shipmentFilter.AddToFilter(PortFilter(JobShipmentSchema.JS_RL_NKDestination, destination));

			ZDBOnlySubQuery cartageFilter = new ZDBOnlySubQuery(ObjectFactory.GetType<ICommonCartage>(), JobBookedCtgMoveSchema.EW_JJ);
			cartageFilter.AddToFilter(JobCartageSchema.JJ_ParentTableCode, JobShipmentSchema.Constants.Prefix);
			cartageFilter.AddSubQuery(shipmentFilter, JoinCondition.And);

			ZDBOnlySubQuery pivotFilter = new ZDBOnlySubQuery(ObjectFactory.GetType<ICommonBookedCtgMove>(), JobBookedCtgMoveSchema.EW_JC_Container);
			pivotFilter.AddSubQuery(cartageFilter, JoinCondition.And);

			ZDBOnlySubQuery containerFilter = new ZDBOnlySubQuery(typeof(CommonContainer), JobContainerSchema.PK);
			containerFilter.AddSubQuery(pivotFilter, JoinCondition.And);

			return containerFilter;
		}

		#endregion

		#endregion

		#region AddDateFilters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(Descriptions.Availiable, GetAvailableQuery).MultilingualDescription = ResString.GetMultilingualString("ContainerManagerFilter|Availiable", "Available");
			filters.AddDateFilter(Descriptions.ContainerYardGateOut, JobContainerSchema.JC_ContainerYardEmptyPickupGateOut).MultilingualDescription = ResString.GetMultilingualString("ContainerManagerFilter|ContainerYardGateOut", "Container Yard Gate Out");
			filters.AddDateFilter(Descriptions.ContainerYardGateIn, JobContainerSchema.JC_ContainerYardEmptyReturnGateIn).MultilingualDescription = ResString.GetMultilingualString("ContainerManagerFilter|ContainerYardGateIn", "Container Yard Gate In");
			filters.AddDateFilter(Descriptions.OnBoard, JobContainerSchema.JC_FCLOnBoardVessel).MultilingualDescription = ResString.GetMultilingualString("ContainerManagerFilter|OnBoard", "On Board");
			filters.AddDateFilter(Descriptions.Storage, JobContainerSchema.JC_ArrivalCTOStorageStartDate).MultilingualDescription = ResString.GetMultilingualString("ContainerManagerFilter|Storage", "Storage");
			filters.AddDateFilter(Descriptions.Unloaded, JobContainerSchema.JC_FCLUnloadFromVessel).MultilingualDescription = ResString.GetMultilingualString("ContainerManagerFilter|Unloaded", "Unloaded");
			filters.AddDateFilter(Descriptions.WharfGateIn, JobContainerSchema.JC_FCLWharfGateIn).MultilingualDescription = ResString.GetMultilingualString("ContainerManagerFilter|WharfGateIn", "Wharf Gate In");
			filters.AddDateFilter(Descriptions.WharfGateOut, JobContainerSchema.JC_FCLWharfGateOut).MultilingualDescription = ResString.GetMultilingualString("ContainerManagerFilter|WharfGateOut", "Wharf Gate Out");
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

		#endregion

		#region AddRelatedConsolFilter

		void AddRelatedConsolFilter(ModuleFilterCollection filters)
		{
			var filter = new ModuleGuidFilter(Descriptions.RelatedConsol, ModuleIDs.JobConsol, JobContainerSchema.JC_JK, Consol_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("ContainerManagerFilter|RelatedConsolidation", "Related Consolidation");
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.FiltersMatch;

			var description = ResString.GetMultilingualString("ContainerManagerFilter|ConsolCategory", "Consol");
			filter.Category = FilterCategories.GetOrCreateFilterCategory(description);

			filters.AddFilter(filter);
		}

		#endregion

		#region AddModeFilters

		void AddModeFilters(ModuleFilterCollection filters)
		{
			filters.AddCustomFilter(new ServiceTypeDateFilter(this, typeof(CommonContainer), true));
			filters.AddCustomFilter(new ServiceTypeDateFilter(this, typeof(CommonContainer), false));
		}

		#endregion

		#endregion

		#region Lookups

		#region Location_List

		LocationCollection Locations
		{
			get { return locations ?? (locations = new LocationCollection(Factory)); }
		}
		LocationCollection locations;

		#endregion

		#region ContainerLocation_List

		public static class ContainerLocation
		{
			public const string All = "ALL";

			public const string ImpNotReturnToCY = "NRCY";
			public const string ImpReturnToCY = "RCY";

			public const string ExpNotOnboard = "NOBRD";
			public const string ExpOnboard = "OBRD";
		}

		CodeDescriptionPairList ContainerLocation_List
		{
			get
			{
				if (fContainerLocation_List == null)
				{
					fContainerLocation_List = new CodeDescriptionPairList();
					fContainerLocation_List.AddPair(ContainerLocation.All, Res.GetString("ContainerFilter|Location|All", "All"));
					fContainerLocation_List.AddPair(ContainerLocation.ImpReturnToCY, Res.GetString("ContainerFilter|Location|ImpReturnToCY", "Return to Yard"));
					fContainerLocation_List.AddPair(ContainerLocation.ImpNotReturnToCY, Res.GetString("ContainerFilter|Location|ImpNotReturnToCY", "Not return to Yard"));
					fContainerLocation_List.AddPair(ContainerLocation.ExpOnboard, Res.GetString("ContainerFilter|Location|ExpOnboard", "On Board"));
					fContainerLocation_List.AddPair(ContainerLocation.ExpNotOnboard, Res.GetString("ContainerFilter|Location|ExpNotOnboard", "Not On Board"));
				}
				return fContainerLocation_List;
			}
		}
		CodeDescriptionPairList fContainerLocation_List;

		#endregion

		#region VGMStatus_List

		CodeDescriptionPairList VGMStatus_List
		{
			get
			{
				if (fVGMStatus_List == null)
				{
					fVGMStatus_List = new CodeDescriptionPairList();
					fVGMStatus_List.AddPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.NotVerified, Constants.ContainerGrossWeightVerificationStatuses.Descriptions.NotVerified);
					fVGMStatus_List.AddPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.NotRequired, Constants.ContainerGrossWeightVerificationStatuses.Descriptions.NotRequired);
					fVGMStatus_List.AddPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent, Constants.ContainerGrossWeightVerificationStatuses.Descriptions.NotSent);
					fVGMStatus_List.AddPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.Sent, Constants.ContainerGrossWeightVerificationStatuses.Descriptions.Sent);
					fVGMStatus_List.AddPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.AmendedNotSent, Constants.ContainerGrossWeightVerificationStatuses.Descriptions.AmendedNotSent);
					fVGMStatus_List.AddPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.Acknowledged, Constants.ContainerGrossWeightVerificationStatuses.Descriptions.Acknowledged);
					fVGMStatus_List.AddPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.Rejected, Constants.ContainerGrossWeightVerificationStatuses.Descriptions.Rejected);
					fVGMStatus_List.AddPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.Accepted, Constants.ContainerGrossWeightVerificationStatuses.Descriptions.Accepted);
					fVGMStatus_List.AddPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawSent, Constants.ContainerGrossWeightVerificationStatuses.Descriptions.WithdrawSent);
					fVGMStatus_List.AddPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawAcknowledged, Constants.ContainerGrossWeightVerificationStatuses.Descriptions.WithdrawAcknowledged);
					fVGMStatus_List.AddPair(Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawRejected, Constants.ContainerGrossWeightVerificationStatuses.Descriptions.WithdrawRejected);
				}
				return fVGMStatus_List;
			}
		}
		CodeDescriptionPairList fVGMStatus_List;

		#endregion

		#region CartageCompany_List

		public OrgHeaderCollection CartageCompany_List
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		#endregion

		#region Consol_List

		public MainFormConsolCollection Consol_List
		{
			get { return new MainFormConsolCollection(Factory); }
		}

		#endregion

		#endregion

		#region Implementation

		ZQuery PortFilter(SchemaStringColumn column, ZString port)
		{
			if (port.IsEmpty)
			{
				return new ZQuery();
			}
			else if (port.Length == 2)
			{
				return new ZQuery(column, SQLComparisonOperator.StartsWith, port);
			}
			else
			{
				return new ZQuery(column, SQLComparisonOperator.Equal, port);
			}
		}

		#endregion
	}
}
