using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Module
{
	public class ManifestTallyFilterBusinessObject : FilterStripBusinessObject
	{
		public ManifestTallyFilterBusinessObject()
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

		#region Flag Filter

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Not Fully Unpacked", GetUnpackedQuery, UnpackedStatus_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("08eb98d0-69ac-4e9d-9da3-de0c3d09ad5c", "Not Fully Unpacked");
			filter.Category = FilterCategories.StatusAndFlags;

			filter = filters.AddTextFilter("Damaged / Pillaged", GetDamagedPillagedQuery, DamagedStatus_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("4ab14d70-dee3-458b-8890-76700a34cc64", "Damaged / Pillaged");
			filter.Category = FilterCategories.StatusAndFlags;

			filter = filters.AddTextFilter("Short / Surplus", GetShortSurplusQuery, ShortStatus_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("64e07ebc-737a-47b3-b07c-42691125c0cb", "Short / Surplus");
			filter.Category = FilterCategories.StatusAndFlags;
		}

		#endregion

		#region Flag Filter Delegates

		ZQuery GetUnpackedQuery(ZString value)
		{
			ZString sqlStatement = ZString.Empty;
			ZQuery query = new ZDBOnlyQuery(typeof(TallyContainer));

			switch (value)
			{
				case "IFU":
					sqlStatement = "NOT EXISTS " +
					"(SELECT * FROM " + GatePassPackLine.Schema.TableName + "  " +
					" JOIN " + JobContainerPackPivotSchema.Constants.SqlSchemaName + "." + JobContainerPackPivotSchema.Constants.TableName + "  ON " + JobContainerPackPivot.Schema.J6_JL + " = " + GatePassPackLine.Schema.PK +
					" WHERE " + JobContainerPackPivot.Schema.J6_JC + " = " + TallyContainer.Schema.PK +
					" AND " + GatePassPackLine.Schema.JL_Outturn + " < " + GatePassPackLine.Schema.JL_PackageCount + ")";
					break;
				case "NFU":
					sqlStatement = "EXISTS " +
					"(SELECT * FROM " + GatePassPackLine.Schema.TableName + "  " +
					" JOIN " + JobContainerPackPivotSchema.Constants.SqlSchemaName + "." + JobContainerPackPivotSchema.Constants.TableName + "  ON " + JobContainerPackPivot.Schema.J6_JL + " = " + GatePassPackLine.Schema.PK +
					" WHERE " + JobContainerPackPivot.Schema.J6_JC + " = " + TallyContainer.Schema.PK +
					" AND " + GatePassPackLine.Schema.JL_Outturn + " < " + GatePassPackLine.Schema.JL_PackageCount + ")";
					break;
			}

			if (!sqlStatement.IsEmpty)
			{
				query.AddFilterAndZSQLParameterCollection(sqlStatement, null);
			}

			return query;
		}

		ZQuery GetDamagedPillagedQuery(ZString value)
		{
			ZString sqlStatement = ZString.Empty;
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(TallyContainer));

			switch (value)
			{
				case "IDP":
					sqlStatement = "EXISTS " +
					"(SELECT * FROM " + GatePassPackLine.Schema.TableName + "  " +
					" JOIN " + JobContainerPackPivotSchema.Constants.SqlSchemaName + "." + JobContainerPackPivotSchema.Constants.TableName + "  ON " + JobContainerPackPivot.Schema.J6_JL + " = " + GatePassPackLine.Schema.PK +
					" WHERE " + JobContainerPackPivot.Schema.J6_JC + " = " + TallyContainer.Schema.PK +
					" AND (" + GatePassPackLine.Schema.JL_Damaged + " > 0 OR " + GatePassPackLine.Schema.JL_Pillaged + " > 0))";
					break;
				case "NDP":
					sqlStatement = "NOT EXISTS " +
					"(SELECT * FROM " + GatePassPackLine.Schema.TableName + "  " +
					" JOIN " + JobContainerPackPivotSchema.Constants.SqlSchemaName + "." + JobContainerPackPivotSchema.Constants.TableName + "  ON " + JobContainerPackPivot.Schema.J6_JL + " = " + GatePassPackLine.Schema.PK +
					" WHERE " + JobContainerPackPivot.Schema.J6_JC + " = " + TallyContainer.Schema.PK +
					" AND (" + GatePassPackLine.Schema.JL_Damaged + " > 0 OR " + GatePassPackLine.Schema.JL_Pillaged + " > 0))";
					break;
			}

			if (!sqlStatement.IsEmpty)
			{
				query.AddFilterAndZSQLParameterCollection(sqlStatement, null);
			}

			return query;
		}

		ZQuery GetShortSurplusQuery(ZString value)
		{
			ZString sqlStatement = ZString.Empty;
			ZQuery query = new ZDBOnlyQuery(typeof(TallyContainer));

			switch (value)
			{
				case "ISS":
					sqlStatement = "EXISTS " +
					"(SELECT * FROM " + GatePassPackLine.Schema.TableName + "  " +
					" JOIN " + JobContainerPackPivotSchema.Constants.SqlSchemaName + "." + JobContainerPackPivotSchema.Constants.TableName + "  ON " + JobContainerPackPivot.Schema.J6_JL + " = " + GatePassPackLine.Schema.PK +
					" WHERE " + JobContainerPackPivot.Schema.J6_JC + " = " + TallyContainer.Schema.PK +
					" AND " + GatePassPackLine.Schema.JL_Outturn + " <> " + GatePassPackLine.Schema.JL_PackageCount + ")";
					break;
				case "NSS":
					sqlStatement = "NOT EXISTS " +
					"(SELECT * FROM " + GatePassPackLine.Schema.TableName + "  " +
					" JOIN " + JobContainerPackPivotSchema.Constants.SqlSchemaName + "." + JobContainerPackPivotSchema.Constants.TableName + "  ON " + JobContainerPackPivot.Schema.J6_JL + " = " + GatePassPackLine.Schema.PK +
					" WHERE " + JobContainerPackPivot.Schema.J6_JC + " = " + TallyContainer.Schema.PK +
					" AND " + GatePassPackLine.Schema.JL_Outturn + " <> " + GatePassPackLine.Schema.JL_PackageCount + ")";
					break;
			}

			if (!sqlStatement.IsEmpty)
			{
				query.AddFilterAndZSQLParameterCollection(sqlStatement, null);
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

			filter = filters.AddNumberFilter(ConstantsAndReusables.NumberFilterTypes.ContainerJob, JobContainerSchema.JC_ContainerJobID);
			filter.MultilingualDescription = ResString.GetMultilingualString("c46dc177-a848-4031-8ad5-e6d5c013dabd", "Container Job #");
			filter.IsCommon = true;

			filters.AddNumberFilter(ConstantsAndReusables.NumberFilterTypes.Shipment, GetShipmentQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobShipmentSchema.JS_UniqueConsignRef)
				.MultilingualDescription = ResString.GetMultilingualString("44ad7fb9-8414-4b8b-90fb-34ce9018fb7f", "Shipment #");
			filters.AddNumberFilter(ConstantsAndReusables.NumberFilterTypes.HouseBill, GetHouseBillQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobShipmentSchema.JS_HouseBill)
				.MultilingualDescription = ResString.GetMultilingualString("51466cf2-7872-4b69-a07e-eae9433bc78a", "House Bill");
			filters.AddNumberFilter(ConstantsAndReusables.NumberFilterTypes.LoadList, GetLoadListQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobConsolSchema.JK_UniqueConsignRef)
				.MultilingualDescription = ResString.GetMultilingualString("e696f63a-13e4-4cbd-b267-7f61db60b5d8", "Load List #");
			filters.AddNumberFilter("Common Numbers", GetCommonNumbersQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobContainerSchema.JC_ContainerNum)
				.MultilingualDescription = ResString.GetMultilingualString("df43e144-abc3-4709-af1f-910bc14bdf62", "Common Numbers");

			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
			{
				filters.AddCustomFilter(new ReferenceNumberFilter(
					ConstantsAndReusables.NumberFilterTypes.AdditionalReferenceNumbers,
					GetReferenceNumberFilter,
					new RefCountryCollection(Factory)
					)
				{ MultilingualDescription = ResString.GetMultilingualString("f1fa99c7-e4f3-4d70-96e5-c7be79982752", "Additional Reference #") });
			}
		}

		#endregion

		#region Number Filter Delegates

		ZQuery GetShipmentQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddShipmentToQuery(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetHouseBillQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddHouseBillToQuery(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetLoadListQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddLoadListToQuery(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetCommonNumbersQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter_PossiblyCommaSeparated(JoinCondition.Or, JobContainerSchema.JC_ContainerNum, comparisonOperator, value);
			query.AddToFilter_PossiblyCommaSeparated(JoinCondition.Or, JobContainerSchema.JC_ContainerJobID, comparisonOperator, value);
			return query;
		}

		#endregion

		#region Number Filter Implementation

		protected void AddShipmentToQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(TallyContainer));

			ZDBOnlySubQuery jobContainerPackPivotQuery = new ZDBOnlySubQuery(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JC);
			ZDBOnlySubQuery jobPackLineQuery = new ZDBOnlySubQuery(typeof(GatePassPackLine), JobContainerPackPivotSchema.J6_JL);
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(GatePassShipment), JobPackLinesSchema.JL_JS);

			shipmentSubQuery.AddToFilter_PossiblyCommaSeparated(
				JoinCondition.And,
				JobShipmentSchema.JS_UniqueConsignRef, @operator, value);
			jobPackLineQuery.AddSubQuery(shipmentSubQuery, JoinCondition.And);
			jobContainerPackPivotQuery.AddSubQuery(jobPackLineQuery, JoinCondition.And);

			dbOnlyResult.AddSubQuery(jobContainerPackPivotQuery, JoinCondition.And);
			query.AddToFilter(dbOnlyResult, JoinCondition.And);
		}

		protected void AddHouseBillToQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(TallyContainer));

			ZDBOnlySubQuery jobContainerPackPivotQuery = new ZDBOnlySubQuery(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JC);
			ZDBOnlySubQuery jobPackLineQuery = new ZDBOnlySubQuery(typeof(GatePassPackLine), JobContainerPackPivotSchema.J6_JL);
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(GatePassShipment), JobPackLinesSchema.JL_JS);
			shipmentSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobShipmentSchema.JS_HouseBill, @operator, value);

			jobPackLineQuery.AddSubQuery(shipmentSubQuery, JoinCondition.And);
			jobContainerPackPivotQuery.AddSubQuery(jobPackLineQuery, JoinCondition.And);

			dbOnlyResult.AddSubQuery(jobContainerPackPivotQuery, JoinCondition.And);
			query.AddToFilter(dbOnlyResult, JoinCondition.And);
		}

		protected void AddLoadListToQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(TallyContainer));

			ZDBOnlySubQuery consolSubQuery = new ZDBOnlySubQuery(typeof(PackUnpackLoadListConsol), JobContainerSchema.JC_JK);
			consolSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobConsolSchema.JK_UniqueConsignRef, @operator, value);

			dbOnlyResult.AddSubQuery(consolSubQuery, JoinCondition.And);

			query.AddToFilter(dbOnlyResult, JoinCondition.And);
		}

		protected ZQuery GetReferenceNumberFilter(SQLComparisonOperator opp, ZString country, ZString type, ZString number)
		{
			var numberQuery = new ReferenceNumberFilterHelper<CFSLoadListConsol>().GetReferenceNumberFilter(opp, country, type, number);

			var dbOnlyResult = new ZDBOnlyQuery(typeof(TallyContainer));

			var consolSubQuery = new ZDBOnlySubQuery(typeof(PackUnpackLoadListConsol), JobContainerSchema.JC_JK);
			consolSubQuery.AddToFilter(numberQuery);

			dbOnlyResult.AddSubQuery(consolSubQuery, JoinCondition.And);

			return dbOnlyResult;
		}

		#endregion

		#region Date Filter

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(ConstantsAndReusables.DateFilterTypes.Unpack, GetLCLUnpackQuery).MultilingualDescription = ResString.GetMultilingualString("95ff6de9-43f7-44c4-9124-5f12b912279e", "Unpack");
			filters.AddDateFilter(ConstantsAndReusables.DateFilterTypes.Available, GetLCLAvailableQuery).MultilingualDescription = ResString.GetMultilingualString("5113754d-4ed1-41de-ac84-cea809689952", "Available");
			filters.AddDateFilter(ConstantsAndReusables.DateFilterTypes.Storage, GetLCLStorageCommencesQuery).MultilingualDescription = ResString.GetMultilingualString("c8d13a3d-0a87-42b4-a774-499b62b61d1f", "Storage");
		}

		#endregion

		#region Date Filter Delegates

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

		#endregion

		#region Date Filter Implementation

		protected void AddLCLUnpackToQuery(ZQuery query, DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(TallyContainer));
			AddDateTimeRange(dbOnlyResult, comparisonOperator, JoinCondition.And, JobContainerSchema.JC_LCLUnpack, fromDate, toDate);
			query.AddToFilter(dbOnlyResult, JoinCondition.And);
		}

		protected void AddLCLAvailableToQuery(ZQuery query, DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(TallyContainer));
			AddDateTimeRange(dbOnlyResult, comparisonOperator, JoinCondition.And, JobContainerSchema.JC_LCLAvailable, fromDate, toDate);
			query.AddToFilter(dbOnlyResult, JoinCondition.And);
		}

		protected void AddLCLStorageCommencesToQuery(ZQuery query, DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(TallyContainer));
			AddDateTimeRange(dbOnlyResult, comparisonOperator, JoinCondition.And, JobContainerSchema.JC_LCLStorageCommences, fromDate, toDate);
			query.AddToFilter(dbOnlyResult, JoinCondition.And);
		}

		#endregion

		#region Organisation Filter

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddGuidFilter(ConstantsAndReusables.OrgFilterTypes.Client, ModuleIDs.Organisation, JobContainerSchema.JC_OH_CFSClient, Forwarder_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("57a48ca1-bac1-41d6-b61f-5d80cc7c3442", "Client");
			filter.Category = FilterCategories.Organisations;
		}

		#endregion

		#region Location Filter

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddLocationFilter(ConstantsAndReusables.PortFilterTypes.LoadDischarge, GetLoadDischargeQuery, Location_List, Location_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("2cbff9f7-b05b-4882-a72b-6bef790f9745", "Load / Discharge");
			filter.SetItemDescriptions(Res.GetData("0408050d-4f70-4587-a9b8-5d0a64b12cef", "Load"), Res.GetData("1c032b33-eb09-4266-aee9-df1b728c5d4b", "Discharge"));
		}

		#endregion

		#region Location Filter Delegates

		ZQuery GetLoadDischargeQuery(ZString loadNk, ZString dischargeNk)
		{
			SailingFilterBuilder filterBuilder = new SailingFilterBuilder(Factory);
			filterBuilder.LoadPort = loadNk;
			filterBuilder.DischargePort = dischargeNk;
			return filterBuilder.ToContainerFilter();
		}

		#endregion

		#region Voyage / Vessel Filter

		void AddVoyageVesselFilters(ModuleFilterCollection filters)
		{
			var filter = new VoyageVesselModuleFilter("Voyage / Flight / Vessel", GetVoyageVesselQuery, Vessel_List)
				.WithMaxLengthOf(JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
			filter.MultilingualDescription = ResString.GetMultilingualString("c880c09d-d1e6-45c9-9445-f1119de4b331", "Voyage / Flight / Vessel");
			filter.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(filter);
		}

		#endregion

		#region Voyage / Vessel Filter Delegates

		ZQuery GetVoyageVesselQuery(SQLComparisonOperator comparisonOperator, ZString voyage, ZString vesselNk, ZBool includeArchived)
		{
			SailingFilterBuilder filterBuilder = new SailingFilterBuilder(Factory);
			filterBuilder.VoyageFlightComparisonOperator = comparisonOperator;
			filterBuilder.VoyageFlight = voyage;
			filterBuilder.Vessel = vesselNk;
			filterBuilder.IncludeArchived = includeArchived;
			return filterBuilder.ToContainerFilter();
		}

		#endregion

		#region Mode / Type Filter

		void AddModeTypeFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Container Mode", GetContainerModeQuery, ContainerMode_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("265f07fd-a3d8-4bab-9fdb-e74445448b59", "Container Mode");
			filter.Category = FilterCategories.ModesAndTypes;
		}

		#endregion

		#region Mode / Type Filter Delegates

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
				ZQuery query = base.Filter;
				query.AddToFilter(ContainerOfLoadListFilter);
				return query;
			}
		}

		ZDBOnlyQuery ContainerOfLoadListFilter
		{
			get
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CommonContainer));
				ZDBOnlySubQuery loadListQuery = new ZDBOnlySubQuery(typeof(CFSLoadListConsol), JobContainerSchema.JC_JK);
				loadListQuery.AddToFilter(JobConsolSchema.JK_IsCFS, true);
				result.AddSubQuery(loadListQuery, JoinCondition.And);

				return result;
			}
		}

		#endregion

		#region Lists

		CodeDescriptionPairList fUnpackedStatus_List;
		public CodeDescriptionPairList UnpackedStatus_List
		{
			get
			{
				if (fUnpackedStatus_List == null)
				{
					fUnpackedStatus_List = new CodeDescriptionPairList();
					fUnpackedStatus_List.AddPair("ALL", Res.GetString("c02c0a25-4320-45c2-97d1-850c4dcb0c3c", "All"));
					fUnpackedStatus_List.AddPair("IFU", Res.GetString("e54afed4-6c63-4b04-b5eb-6b790c284f7f", "Fully Unpacked"));
					fUnpackedStatus_List.AddPair("NFU", Res.GetString("08eb98d0-69ac-4e9d-9da3-de0c3d09ad5c", "Not Fully Unpacked"));
				}
				return fUnpackedStatus_List;
			}
		}

		CodeDescriptionPairList fDamagedStatus_List;
		public CodeDescriptionPairList DamagedStatus_List
		{
			get
			{
				if (fDamagedStatus_List == null)
				{
					fDamagedStatus_List = new CodeDescriptionPairList();
					fDamagedStatus_List.AddPair("ALL", Res.GetString("0c0f953e-b015-4e88-a9b1-982d750158fc", "All"));
					fDamagedStatus_List.AddPair("IDP", Res.GetString("4ab14d70-dee3-458b-8890-76700a34cc64", "Damaged / Pillaged"));
					fDamagedStatus_List.AddPair("NDP", Res.GetString("68c4d1a1-f88c-4122-8c77-9e2b02c7968e", "Not Damaged / Pillaged"));
				}
				return fDamagedStatus_List;
			}
		}

		CodeDescriptionPairList fShortStatus_List;
		public CodeDescriptionPairList ShortStatus_List
		{
			get
			{
				if (fShortStatus_List == null)
				{
					fShortStatus_List = new CodeDescriptionPairList();
					fShortStatus_List.AddPair("ALL", Res.GetString("bc528c12-1251-4970-a2cd-509cc0f5b867", "All"));
					fShortStatus_List.AddPair("ISS", Res.GetString("64e07ebc-737a-47b3-b07c-42691125c0cb", "Short / Surplus"));
					fShortStatus_List.AddPair("NSS", Res.GetString("71b2807a-0ed1-4986-975a-c369bb0f4a31", "Not Short / Surplus"));
				}
				return fShortStatus_List;
			}
		}

		public CodeDescriptionPairList ContainerMode_List
		{
			get
			{
				return new CodeDescriptionPairList(OLookUpEditType.ContainerMode);
			}
		}

		ForwarderCollection fForwarder_List;
		public ForwarderCollection Forwarder_List
		{
			get
			{
				if (fForwarder_List == null)
				{
					fForwarder_List = new ForwarderCollection(Factory);
				}

				return fForwarder_List;
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

		#endregion

		#region workflow filter

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();

			var workflowHelper = new WorkflowFilterStripsHelperWithRoutingSupport(typeof(TallyContainer), WorkflowDescriptors.ContainerWorkflowDescriptorCode, Factory);
			helpers.Add(workflowHelper);

			return helpers;
		}

		#endregion
	}
}
