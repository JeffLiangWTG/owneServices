using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Module
{
	public class LoadListConsolFilterBusinessObject : FilterStripBusinessObject, IAccountingFilterStripHolder
	{
		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			var filter = new ModuleNumberFilter(ConstantsAndReusables.NumberFilterTypes.LoadList, GetLoadListNoQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobConsolSchema.JK_UniqueConsignRef);
			filter.MultilingualDescription = ResString.GetMultilingualString("e696f63a-13e4-4cbd-b267-7f61db60b5d8", "Load List #");
			filter.IsCommon = true;
			return filter;
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
			AddTypeModeFilters(filters);

			AccountingFilterStrip.AddBillingFilters(filters);
			AccountingFilterStrip.AddJobManagementFilters(filters, Env.Security.CFSLoadListJobInvoicing);

			return filters;
		}

		#region Filter

		public override ZQuery Filter
		{
			get
			{
				ZQuery result = base.Filter;
				result.AddToFilter(JobConsolSchema.JK_IsCFS, ZBool.True);
				return result;
			}
		}

		#endregion

		#region Flag Filter

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("All / With / Without Shipments", GetNoShipmentsQuery, NoShipmentsState_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("911fff35-d7a4-45aa-a218-6cfbee329d58", "All / With / Without Shipments");
			filter.Category = FilterCategories.StatusAndFlags;
		}

		#endregion

		#region Flag Filter Delegates

		ZQuery GetNoShipmentsQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			ZString sqlStatement = ZString.Empty;

			switch (value)
			{
				case "LWS":
					sqlStatement = "EXISTS " +
					"(SELECT * FROM " + JobConShipLinkSchema.Constants.SqlSchemaName + "." + JobConShipLinkSchema.Constants.TableName +
					" WHERE " + AutoJobConShipLink.Schema.JN_JK + " = " + AutoJobConsol.Schema.PK + ")";
					break;

				case "LNS":
					sqlStatement = "NOT EXISTS " +
					"(SELECT * FROM " + JobConShipLinkSchema.Constants.SqlSchemaName + "." + JobConShipLinkSchema.Constants.TableName +
					" WHERE " + AutoJobConShipLink.Schema.JN_JK + " = " + AutoJobConsol.Schema.PK + ")";
					break;
			}

			if (!sqlStatement.IsEmpty)
			{
				ZDBOnlyQuery subQuery = new ZDBOnlyQuery(typeof(CFSShipment));
				subQuery.AddFilterAndZSQLParameterCollection(sqlStatement, null);
				query.AddToFilter(subQuery);
			}

			return query;
		}

		#endregion

		#region Number Filter

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(ConstantsAndReusables.NumberFilterTypes.HouseBill, GetHouseBillQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobShipmentSchema.JS_HouseBill)
				.MultilingualDescription = ResString.GetMultilingualString("51466cf2-7872-4b69-a07e-eae9433bc78a", "House Bill");
			filters.AddNumberFilter(ConstantsAndReusables.NumberFilterTypes.Shipment, GetShipmentNoQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobShipmentSchema.JS_UniqueConsignRef)
				.MultilingualDescription = ResString.GetMultilingualString("44ad7fb9-8414-4b8b-90fb-34ce9018fb7f", "Shipment #");

			var filter = filters.AddNumberFilter(ConstantsAndReusables.NumberFilterTypes.Container, GetContainerNoQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobContainerSchema.JC_ContainerNum);
			filter.MultilingualDescription = ResString.GetMultilingualString("ddb0b67b-2a34-4eb6-aacb-eff6306b5e87", "Container #");
			filter.IsCommon = true;

			filters.AddNumberFilter(ConstantsAndReusables.NumberFilterTypes.ContainerJob, GetContainerJobNoQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobContainerSchema.JC_ContainerJobID)
				.MultilingualDescription = ResString.GetMultilingualString("c46dc177-a848-4031-8ad5-e6d5c013dabd", "Container Job #");
			filters.AddNumberFilter(ConstantsAndReusables.NumberFilterTypes.InterimReceipt, GetInterimReceiptNoQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobShipmentSchema.JS_InterimReceipt)
				.MultilingualDescription = ResString.GetMultilingualString("b1ad4e51-a2e5-4450-91d1-5c0ad0dc05c4", "Interim Receipt #");
			filters.AddNumberFilter(ConstantsAndReusables.NumberFilterTypes.CustomsEntryNumber, GetCustomsEntryNoQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobConsolSchema.JK_CustomsReference)
				.MultilingualDescription = ResString.GetMultilingualString("463e7002-76f1-4ab0-9336-c1d23502ddbd", "Customs Entry #");
			filters.AddNumberFilter(ConstantsAndReusables.NumberFilterTypes.CarrierBookingRef, GetCarrierBookingRefQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobConsolSchema.JK_BookingReference)
				.MultilingualDescription = ResString.GetMultilingualString("c7d4b326-e732-46ad-abe5-f2bdaf57b2d4", "Carriers Booking Ref #");
			filters.AddNumberFilter(ConstantsAndReusables.NumberFilterTypes.ClientRef, GetClientRefQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobConsolSchema.JK_AgentsReference)
				.MultilingualDescription = ResString.GetMultilingualString("8daabd44-4295-4372-b59f-0720e04b1b4c", "Client Ref");

			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
			{
				filters.AddCustomFilter(new ReferenceNumberFilter(
					ConstantsAndReusables.NumberFilterTypes.AdditionalReferenceNumbers,
					new ReferenceNumberFilterHelper<CFSLoadListConsol>().GetReferenceNumberFilter,
					new RefCountryCollection(Factory)
					)
				{ MultilingualDescription = ResString.GetMultilingualString("f1fa99c7-e4f3-4d70-96e5-c7be79982752", "Additional Reference #") });
			}
		}

		#endregion

		#region Number Filter Delegates

		ZQuery GetLoadListNoQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddLoadListNoToFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetHouseBillQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddHouseBillToFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetShipmentNoQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddShipmentNoToFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetContainerNoQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddContainerNoToFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetContainerJobNoQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddContainerJobNoToFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetInterimReceiptNoQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddInterimReceiptNoToFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetCustomsEntryNoQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddCustomsEntryNoToFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetCarrierBookingRefQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddCarrierBookingRefToFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetClientRefQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddClientRefToFilter(query, comparisonOperator, value);
			return query;
		}

		#endregion

		#region Number Filter Implementation

		protected void AddLoadListNoToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter_PossiblyCommaSeparated(JobConsolSchema.JK_UniqueConsignRef, @operator, value);
		}

		protected void AddHouseBillToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(CFSLoadListConsol));

			ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JK);
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(CFSShipment), JobConShipLinkSchema.JN_JS);

			shipmentSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobShipmentSchema.JS_HouseBill, @operator, value);
			pivotSubQuery.AddSubQuery(shipmentSubQuery, JoinCondition.And);
			dbOnlyResult.AddSubQuery(pivotSubQuery, JoinCondition.And);

			query.AddToFilter(dbOnlyResult, JoinCondition.And);
		}

		protected void AddShipmentNoToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(CFSLoadListConsol));
			ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JK);
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(CFSShipment), JobConShipLinkSchema.JN_JS);

			shipmentSubQuery.AddToFilter_PossiblyCommaSeparated(JobShipmentSchema.JS_UniqueConsignRef, @operator, value);

			pivotSubQuery.AddSubQuery(shipmentSubQuery, JoinCondition.And);
			dbOnlyResult.AddSubQuery(pivotSubQuery, JoinCondition.And);
			query.AddToFilter(dbOnlyResult, JoinCondition.And);
		}

		protected void AddContainerNoToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(CFSLoadListConsol));
			ZDBOnlySubQuery containerSubQuery = new ZDBOnlySubQuery(typeof(CFSContainer), JobContainerSchema.JC_JK);

			containerSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobContainerSchema.JC_ContainerNum, @operator, value);
			dbOnlyResult.AddSubQuery(containerSubQuery, JoinCondition.And);

			query.AddToFilter(dbOnlyResult, JoinCondition.And);
		}

		protected void AddContainerJobNoToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(CFSLoadListConsol));
			ZDBOnlySubQuery containerSubQuery = new ZDBOnlySubQuery(typeof(CFSContainer), JobContainerSchema.JC_JK);

			containerSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobContainerSchema.JC_ContainerJobID, @operator, value);
			dbOnlyResult.AddSubQuery(containerSubQuery, JoinCondition.And);

			query.AddToFilter(dbOnlyResult, JoinCondition.And);
		}

		protected void AddInterimReceiptNoToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(CFSLoadListConsol));
			ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JK);
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(CFSShipment), JobConShipLinkSchema.JN_JS);

			shipmentSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobShipmentSchema.JS_InterimReceipt, @operator, value);
			pivotSubQuery.AddSubQuery(shipmentSubQuery, JoinCondition.And);
			dbOnlyResult.AddSubQuery(pivotSubQuery, JoinCondition.And);

			query.AddToFilter(dbOnlyResult, JoinCondition.And);
		}

		protected void AddCustomsEntryNoToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(new ZQuery().AddToFilter_PossiblyCommaSeparated(JobConsolSchema.JK_CustomsReference, @operator, value));
		}

		protected void AddCarrierBookingRefToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(new ZQuery().AddToFilter_PossiblyCommaSeparated(JobConsolSchema.JK_BookingReference, @operator, value));
		}

		protected void AddClientRefToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(new ZQuery().AddToFilter_PossiblyCommaSeparated(JobConsolSchema.JK_AgentsReference, @operator, value));
		}

		#endregion

		#region Date Filter

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(ConstantsAndReusables.DateFilterTypes.ETA, GetETAQuery).MultilingualDescription = ResString.GetMultilingualString("08868741-c670-484f-8688-dba901bf2e7a", "ETA");
			filters.AddDateFilter(ConstantsAndReusables.DateFilterTypes.ETD, GetETDQuery).MultilingualDescription = ResString.GetMultilingualString("83b397ba-4ec2-4504-b84d-da210154fad8", "ETD");
		}

		#endregion

		#region Date Filter Delegates

		ZQuery GetETAQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			SailingFilterBuilder filterBuilder = new SailingFilterBuilder(Factory);
			filterBuilder.SetDateRange(SailingFilterBuilder.Dates.ETA, comparisonOperator, fromDate, toDate);
			return filterBuilder.ToConsolFilter();
		}

		ZQuery GetETDQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			SailingFilterBuilder filterBuilder = new SailingFilterBuilder(Factory);
			filterBuilder.SetDateRange(SailingFilterBuilder.Dates.ETD, comparisonOperator, fromDate, toDate);
			return filterBuilder.ToConsolFilter();
		}

		#endregion

		#region Organisation Filter

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter(ConstantsAndReusables.OrgFilterTypes.Client, ModuleIDs.Organisation, GetClientQuery, Organisation_List).MultilingualDescription = ResString.GetMultilingualString("57a48ca1-bac1-41d6-b61f-5d80cc7c3442", "Client");

			ModuleGuidsFilter cnrCneFilter = filters.AddGuidFilter(
				String.Format(CultureInfo.InvariantCulture, "{0} / {1}", FreightDataRegistry.Instance.ConsignorShipperTerminology.Value.GetUnresolvedString(), ConstantsAndReusables.OrgFilterTypes.Consignee),
				ModuleIDs.Organisation, GetConsignorConsigneeQuery, Consignor_List, Consignee_List);
			var consignorTerminology = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value.IsEmpty ? (ZString)FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue : ((ZString)FreightDataRegistry.Instance.ConsignorShipperTerminology.Value).SubstringSafe(0, 15);
			cnrCneFilter.MultilingualDescription = ResString.GetMultilingualString("f62a248b-fec0-466a-a7da-0433630d459e", "{0} / Consignee", FreightDataRegistry.Instance.ConsignorShipperTerminology.Value);
			cnrCneFilter.SetItemDescriptions(new ResourceStringData("", consignorTerminology), Res.GetData("0fd8fb29-5784-453d-a1fa-13eae31828fd", "Consignee"));
			cnrCneFilter.Category = FilterCategories.Organisations;
		}

		#endregion

		#region Organisation Filter Delegates

		ZQuery GetClientQuery(ZGuid value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CFSLoadListConsol));

			ZDBOnlySubQuery sfSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobConsolSchema.JK_OA_SendingForwarderAddress);
			ZDBOnlySubQuery rfSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobConsolSchema.JK_OA_ReceivingForwarderAddress);
			sfSubQuery.AddToFilter(OrgAddressSchema.OA_OH, value);
			rfSubQuery.AddToFilter(OrgAddressSchema.OA_OH, value);

			result.AddSubQuery(sfSubQuery, JoinCondition.Or);
			result.AddSubQuery(rfSubQuery, JoinCondition.Or);

			return result;
		}

		ZQuery GetConsignorConsigneeQuery(ZGuid consignor, ZGuid consignee)
		{
			ZQuery query = new ZQuery();

			if (consignor.IsValid || consignee.IsValid)
			{
				ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(CFSLoadListConsol));

				ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JK);
				ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(CFSShipment), JobConShipLinkSchema.JN_JS);

				if (consignor.IsValid)
				{
					shipmentSubQuery.AddToFilter(FilterByOrg(consignor, DocAddressTypes.Codes.ConsignorDocumentaryAddress));
				}

				if (consignee.IsValid)
				{
					shipmentSubQuery.AddToFilter(FilterByOrg(consignee, DocAddressTypes.Codes.ConsigneeDocumentaryAddress));
				}

				pivotSubQuery.AddSubQuery(shipmentSubQuery, JoinCondition.And);
				dbOnlyResult.AddSubQuery(pivotSubQuery, JoinCondition.And);

				query.AddToFilter(dbOnlyResult, JoinCondition.And);
			}

			return query;
		}

		#endregion

		#region Organisation Filter Implementation

		ZDBOnlyQuery FilterByOrg(ZGuid orgPK, string addressType)
		{
			ZDBOnlySubQuery orgAddressFilter = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressFilter.AddToFilter(OrgAddressSchema.OA_OH, orgPK);

			ZDBOnlySubQuery docAddressFilter = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			docAddressFilter.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressFilter, JoinCondition.And);
			docAddressFilter.AddToFilter(JobDocAddressSchema.E2_AddressType, addressType);

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CommonShipment));
			result.AddSubQuery(JobShipmentSchema.PK, docAddressFilter, JoinCondition.And);

			return result;
		}

		#endregion

		#region Location Filter

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddLocationFilter(ConstantsAndReusables.PortFilterTypes.LoadDischarge, GetLoadDischargeQuery, Location_List, Location_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("2cbff9f7-b05b-4882-a72b-6bef790f9745", "Load / Discharge");
			filter.SetItemDescriptions(Res.GetData("3a6e8043-9afc-4d98-bc16-ea8bfededffb", "Load"), Res.GetData("e46dd846-8998-44f4-b2a3-86fcc2e3993c", "Discharge"));

			filter = filters.AddLocationFilter(ConstantsAndReusables.PortFilterTypes.OriginDestination, GetOriginDestinationQuery, Location_List, Location_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("60cd02cf-9425-4bbb-86d4-4c8b5c2fb91d", "Origin / Destination");
			filter.SetItemDescriptions(Res.GetData("b99c4a7a-80bc-4d23-a226-e9d2b8929a37", "Origin"), Res.GetData("084c04fa-c9ce-4a3c-b54f-19dad4b31f6c", "Destination"));
		}

		#endregion

		#region Location Filter Delegates

		ZQuery GetLoadDischargeQuery(ZString loadNk, ZString dischargeNk)
		{
			SailingFilterBuilder filterBuilder = new SailingFilterBuilder(Factory);
			filterBuilder.LoadPort = loadNk;
			filterBuilder.DischargePort = dischargeNk;
			return filterBuilder.ToConsolFilter();
		}

		ZQuery GetOriginDestinationQuery(ZString originNk, ZString destinationNk)
		{
			ZQuery query = new ZQuery();

			if (!originNk.IsEmpty || !destinationNk.IsEmpty)
			{
				ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(CFSLoadListConsol));

				ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JK);
				ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(CFSShipment), JobConShipLinkSchema.JN_JS);

				if (!originNk.IsEmpty)
				{
					bool isCountryCode = (originNk.Length == 2);
					SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

					shipmentSubQuery.AddToFilter(JoinCondition.And, JobShipmentSchema.JS_RL_NKOrigin, comparisonOperator, originNk);
				}

				if (!destinationNk.IsEmpty)
				{
					bool isCountryCode = (destinationNk.Length == 2);
					SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

					shipmentSubQuery.AddToFilter(JoinCondition.And, JobShipmentSchema.JS_RL_NKDestination, comparisonOperator, destinationNk);
				}

				pivotSubQuery.AddSubQuery(shipmentSubQuery, JoinCondition.And);
				dbOnlyResult.AddSubQuery(pivotSubQuery, JoinCondition.And);

				query.AddToFilter(dbOnlyResult, JoinCondition.And);
			}

			return query;
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
			var filterBuilder = new SailingFilterBuilder(Factory);
			filterBuilder.VoyageFlightComparisonOperator = comparisonOperator;
			filterBuilder.VoyageFlight = voyage;
			filterBuilder.Vessel = vesselNk;
			filterBuilder.IncludeArchived = includeArchived;

			return filterBuilder.ToConsolFilter();
		}

		#endregion

		#region Type / Mode Filter

		void AddTypeModeFilters(ModuleFilterCollection filters)
		{
			filters.AddCustomFilter(new LoadListModeFilter("Transport / Container Modes", GetTransportContainerModeQuery, TransportMode_List, GetContainerModeList) { MultilingualDescription = ResString.GetMultilingualString("93c58949-74f3-4307-879d-fe764b6f5324", "Transport / Container Modes") });
		}

		#endregion

		#region Type / Mode / Status Filter Delegates

		ZQuery GetTransportContainerModeQuery(ZString value1, ZString value2)
		{
			ZQuery query = new ZQuery();

			if (!value1.IsEmpty)
			{
				query.AddToFilter(JobConsolSchema.JK_TransportMode, SQLComparisonOperator.Equal, value1);
			}

			if (!value2.IsEmpty)
			{
				query.AddToFilter(JobConsolSchema.JK_ConsolMode, SQLComparisonOperator.Equal, value2);
			}

			return query;
		}

		#endregion

		#region Lists

		CodeDescriptionPairList fNoShipmentsState_List;
		public CodeDescriptionPairList NoShipmentsState_List
		{
			get
			{
				if (fNoShipmentsState_List == null)
				{
					fNoShipmentsState_List = new CodeDescriptionPairList();
					fNoShipmentsState_List.AddPair("ALL", Res.GetString("0cbec0ff-6b7a-42e1-b2d9-ef9c379b988d", "All Load Lists"));
					fNoShipmentsState_List.AddPair("LWS", Res.GetString("d85377df-34d9-4462-8177-179c5aab2c90", "Load Lists With Shipments"));
					fNoShipmentsState_List.AddPair("LNS", Res.GetString("81e5b74e-92ff-4d54-8d5e-c49dd00c52f3", "Load Lists Without Shipments"));
				}
				return fNoShipmentsState_List;
			}
		}

		OrgHeaderCollection fOrganisation_List;
		public OrgHeaderCollection Organisation_List
		{
			get
			{
				if (fOrganisation_List == null)
				{
					fOrganisation_List = new OrgHeaderCollection(Factory);
				}
				return fOrganisation_List;
			}
		}

		ConsigneeCollection fConsignee_List;
		public ConsigneeCollection Consignee_List
		{
			get
			{
				if (fConsignee_List == null)
				{
					fConsignee_List = new ConsigneeCollection(Factory);
				}
				return fConsignee_List;
			}
		}

		ConsignorCollection fConsignor_List;
		public ConsignorCollection Consignor_List
		{
			get
			{
				if (fConsignor_List == null)
				{
					fConsignor_List = new ConsignorCollection(Factory);
				}
				return fConsignor_List;
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

		public CodeDescriptionPairList TransportMode_List
		{
			get { return FreightCodePairLists.LinkableTransportModeList(); }
		}

		CodeDescriptionPairList fContainerMode_List;
		public CodeDescriptionPairList GetContainerModeList(ZString value)
		{
			if (!value.IsEmpty)
			{
				return FreightCodePairLists.ConsolModeList(Core.Constants.AgentType.Agent, value);
			}
			else
			{
				if (fContainerMode_List == null)
				{
					fContainerMode_List = new CodeDescriptionPairList(OLookUpEditType.FreightContainerMode);
				}
				return fContainerMode_List;
			}
		}

		JobHeaderStatusList fJobStatus_List;
		public JobHeaderStatusList JobStatus_List
		{
			get
			{
				if (fJobStatus_List == null)
				{
					fJobStatus_List = new JobHeaderStatusList();
				}
				return fJobStatus_List;
			}
		}

		#endregion

		#region IAccountingFilterStripHolder Members

		IAccountingFilterStrip AccountingFilterStrip
		{
			get
			{
				if (AccountingFilterStrip_innerValue == null)
				{
					AccountingFilterStrip_innerValue = (IAccountingFilterStrip)Activator.CreateInstance(ObjectFactory.GetType<IAccountingFilterStrip>(), this);
					AccountingFilterStrip_innerValue.Initialize(addProfitLossReasonFilters: true);
				}

				return AccountingFilterStrip_innerValue;
			}
		}
		IAccountingFilterStrip AccountingFilterStrip_innerValue;

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CFSLoadListConsol));
			result.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return result;
		}

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new Dictionary<string, object>()
		{
			{ AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride, ResString.GetMultilingualString("FreightCSF|LoadListConsolFilter|JobStatus", "Job Status") }
		};

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride
		{
			get { return null; }
		}

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride
		{
			get { return null; }
		}

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories
		{
			get { return null; }
		}

		#endregion

		#region workflow filter

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();

			var workflowHelper = new WorkflowFilterStripsHelperWithRoutingSupport(typeof(CFSLoadListConsol), JobInvoicingConsumerTypes.CFSLoadList.Code, Factory);
			helpers.Add(workflowHelper);

			return helpers;
		}

		#endregion
	}
}
