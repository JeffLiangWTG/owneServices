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
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Module
{
	public class ShipmentReceivalFilterStrip : FilterStripBusinessObject, IAccountingFilterStripHolder
	{
		#region Descriptions

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public static class Descriptions
		{
			public const string HiddenFilter = "Hidden Filter";

			public const string CarrierRef = "Carriers Booking Ref #";
			public const string ClientRef = "Client Ref.";
			public const string ContainerNumber = "Container #";
			public const string ContainerJobNumber = "Container Job #";
			public const string CustomsEntryNumber = "Customs Entry #";
			public const string Housebill = "Housebill";
			public const string InterimReceipt = "Interim Receipt";
			public const string InvoiceNumber = "Invoice #";
			public const string LoadListNumber = "Load List #";
			public const string ShipmentNumber = "Shipment #";
			public const string WarehouseLocation = "Warehouse Location";
			public const string AdditionalReferenceNumbers = "Additional Reference #";

			public const string Client = "Client";

			public const string ServiceLevel = "Service Level";
			public const string TransportMode = "Transport Mode";
			public const string VesselVoyageFlight = "Voyage / Flight / Vessel";

			public const string LoadDischarge = "Load / Discharge";
			public const string LoadListEndPorts = "Load List End Ports";
			public const string OriginDestination = "Origin / Destination";

			public const string ATA = "ATA";
			public const string ATD = "ATD";
			public const string ETA = "ETA";
			public const string ETD = "ETD";
			public const string ReceivedDate = "Received Date";
			public const string RegisteredDate = "Registered Date";

			public const string ActiveStatus = "Active Status";
			public static string ChargesNotPosted { get { return Res.GetString("0e810dd6-779b-465f-b328-8bd4659bbd30", "Charges Not Posted"); } }
			public const string Flags = "Flags";
			public static string LoadListUnassigned { get { return Res.GetString("32edea3b-15d8-491c-832b-0c4cba95b28d", "Load List Unassigned"); } }
		}

		#endregion

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			ModuleNumberFilter filter = new ModuleNumberFilter(Descriptions.ShipmentNumber, JobShipmentSchema.JS_UniqueConsignRef);
			filter.MultilingualDescription = ResString.GetMultilingualString("410a04fd-f89c-4a44-9b72-8cfd3cc9f824", "Shipment #");
			filter.IsCommon = true;
			return filter;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			AddHiddenFilters(filters);
			AddNumberFilters(filters);
			AddOrganisationFilters(filters);
			AddTextFilters(filters);
			AddVoyageVesselFilter(filters);
			AddLocationFilters(filters);
			AddDateFilters(filters);
			AddStatusAndFlagsFilters(filters);

			AccountingFilterStrip.AddBillingFilters(filters);
			AccountingFilterStrip.AddJobManagementFilters(filters, Env.Security.CFSShipmentJobInvoicing);

			ObjectFactory.Get<Enterprise.Integration.Customs.CA.ICFSShipmentModuleColumnsAndFiltersProvider>().AddFilters(filters, Factory);

			return filters;
		}

		#region Add*Filters

		#region AddHiddenFilters

		void AddHiddenFilters(ModuleFilterCollection filters)
		{
			filters.AddFlagsFilter(
				Descriptions.HiddenFilter,
				new string[] { (NoResString)"<error>" },
				new GetFlagsQuery[] { HiddenFilter }
			).Visibility = FilterVisibility.AlwaysAppliedAndHidden;
		}

		#region HiddenFilter

		ZQuery HiddenFilter(ZBool thisIsIgnored)
		{
			return new ZQuery(JobShipmentSchema.JS_IsCFSRegistered, true);
		}

		#endregion

		#endregion

		#region AddNumberFilters

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(Descriptions.CarrierRef, GetCarriersRefFilter)
				.WithMaxLengthOf<ModuleNumberFilter>(JobShipmentSchema.JS_BookingReference)
				.MultilingualDescription = ResString.GetMultilingualString("c744fd83-851d-4870-84e2-c21a85e2629d", "Carriers Booking Ref #");
			filters.AddNumberFilter(Descriptions.ClientRef, JobShipmentSchema.JS_ConsolReference).MultilingualDescription = ResString.GetMultilingualString("ddf5e895-c196-447b-8748-a5f924a3e36d", "Client Ref.");

			var filter = filters.AddNumberFilter(Descriptions.ContainerNumber, GetContainerNumberFilter)
				.WithMaxLengthOf<ModuleNumberFilter>(JobContainerSchema.JC_ContainerNum);
			filter.MultilingualDescription = ResString.GetMultilingualString("115f22e0-fb98-4822-b405-668afd96543f", "Container #");
			filter.IsCommon = true;

			if (!Globals.IsWeb)
			{
				filters.AddFountainFilter(Descriptions.ContainerJobNumber, GetContainerJobNumberFilter, "D")
					.WithMaxLengthOf<ModuleFountainFilter>(JobContainerSchema.JC_ContainerJobID)
					.MultilingualDescription = ResString.GetMultilingualString("3726616e-53b3-4275-baf2-fbb0f3fb10ef", "Container Job #");
			}

			if (UsedInAustralia)
			{
				filter = filters.AddNumberFilter(Descriptions.CustomsEntryNumber, GetCustomsEntryNumberFilter)
					.WithMaxLengthOf<ModuleNumberFilter>(CusEntryNumSchema.CE_EntryNum);
				filter.MultilingualDescription = ResString.GetMultilingualString("6f1549a3-a060-4749-95f0-825a3b12945a", "Customs Entry #");
				filter.UseMultiSearch = false; //TODO: Needs to work with multiple values. (GetCustomsEntryNumberFilter)
			}

			filter = filters.AddNumberFilter(Descriptions.Housebill, JobShipmentSchema.JS_HouseBill);
			filter.MultilingualDescription = ResString.GetMultilingualString("23e0fee2-d2ff-4705-b77a-62211da5eb96", "House Bill");
			filter.IsCommon = true;

			filters.AddNumberFilter(Descriptions.InterimReceipt, JobShipmentSchema.JS_InterimReceipt).MultilingualDescription = ResString.GetMultilingualString("8eb2e18c-84e7-417b-bc64-bd6953503042", "Interim Receipt");

			if (!Globals.IsWeb)
			{
				filter = filters.AddNumberFilter(Descriptions.InvoiceNumber, GetInvoiceNumberFilter)
					.WithMaxLengthOf<ModuleNumberFilter>(JobComInvoiceHeaderSchema.JZ_InvoiceNumber);
				filter.MultilingualDescription = ResString.GetMultilingualString("b8357c2f-8ce3-4846-93b3-8fed278661a0", "Invoice #");
				filter.UseMultiSearch = false; //TODO: Needs to work with multiple values. (GetInvoiceNumberFilter)
			}

			filters.AddNumberFilter(Descriptions.LoadListNumber, GetLoadListNumberFilter)
				.WithMaxLengthOf<ModuleNumberFilter>(JobConsolSchema.JK_UniqueConsignRef)
				.MultilingualDescription = ResString.GetMultilingualString("9475f76f-8120-432e-bd06-38b2a192c8b4", "Load List #");

			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
			{
				filters.AddCustomFilter(new ReferenceNumberFilter(
					Descriptions.AdditionalReferenceNumbers,
					new ReferenceNumberFilterHelper<CFSShipment>().GetReferenceNumberFilter,
					new RefCountryCollection(Factory)
					)
				{ MultilingualDescription = ResString.GetMultilingualString("7fedf91d-f38b-4ab4-a586-4b709eb03f9c", "Additional Reference #") });
			}
		}

		#region GetCarriersRefFilter

		ZQuery GetCarriersRefFilter(SQLComparisonOperator sqlOperator, ZString value)
		{
			ZDBOnlySubQuery loadListFilter = new ZDBOnlySubQuery(typeof(CommonConsol), JobConShipLinkSchema.JN_JK);
			loadListFilter.AddToFilter_PossiblyCommaSeparated(JoinCondition.Or, JobConsolSchema.JK_BookingReference, sqlOperator, value);

			ZDBOnlySubQuery linkFilter = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			linkFilter.AddSubQuery(loadListFilter, JoinCondition.And);

			ZDBOnlyQuery shipmentFilter = new ZDBOnlyQuery(typeof(CommonShipment));
			shipmentFilter.AddSubQuery(linkFilter, JoinCondition.And);
			shipmentFilter.AddToFilter_PossiblyCommaSeparated(JoinCondition.Or, JobShipmentSchema.JS_BookingReference, sqlOperator, value);
			return shipmentFilter;
		}

		#endregion

		#region GetContainerNumberFilter

		ZQuery GetContainerNumberFilter(SQLComparisonOperator sqlOperator, ZString value)
		{
			ZDBOnlySubQuery containerFilter = new ZDBOnlySubQuery(typeof(CommonContainer), JobContainerSchema.JC_JK);
			containerFilter.AddToFilter_PossiblyCommaSeparated(JobContainerSchema.JC_ContainerNum, sqlOperator, value);

			ZDBOnlySubQuery linkFilter = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			linkFilter.AddSubQuery(JobConShipLinkSchema.JN_JK, containerFilter, JoinCondition.And);

			ZDBOnlyQuery shipmentFilter = new ZDBOnlyQuery(typeof(CommonShipment));
			shipmentFilter.AddSubQuery(linkFilter, JoinCondition.And);

			return shipmentFilter;
		}

		#endregion

		#region GetContainerJobNumberFilter

		ZQuery GetContainerJobNumberFilter(SQLComparisonOperator sqlOperator, ZString value)
		{
			ZDBOnlySubQuery containerFilter = new ZDBOnlySubQuery(typeof(CommonContainer), JobContainerSchema.JC_JK);
			containerFilter.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobContainerSchema.JC_ContainerJobID, sqlOperator, value);

			ZDBOnlySubQuery linkFilter = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			linkFilter.AddSubQuery(JobConShipLinkSchema.JN_JK, containerFilter, JoinCondition.And);

			ZDBOnlyQuery shipmentFilter = new ZDBOnlyQuery(typeof(CommonShipment));
			shipmentFilter.AddSubQuery(linkFilter, JoinCondition.And);
			return shipmentFilter;
		}

		#endregion

		#region GetCustomsEntryNumberFilter

		ZQuery GetCustomsEntryNumberFilter(SQLComparisonOperator sqlOperator, ZString value)
		{
			//TODO: Needs to work with multiple values.
			string sQL = CommonShipment.Schema.PK + " IN " +
				"(select " + JobDeclarationSchema.Constants.JE_JS + " from " + JobDeclarationSchema.Constants.SqlSchemaName + "." + JobDeclarationSchema.Constants.TableName + " where " +
				JobDeclarationSchema.Constants.PK + " in " +
				"	(select " + CusEntryHeaderSchema.Constants.CH_JE + " from " + CusEntryHeaderSchema.Constants.SqlSchemaName + "." + CusEntryHeaderSchema.Constants.TableName + " where " +
				CusEntryHeaderSchema.Constants.PK + " in " +
				"		(select " + CusEntryNumSchema.Constants.CE_ParentID + " from dbo.CusEntryNum where " +
				"		" + CusEntryNumSchema.Constants.CE_ParentTable + " = @CusEntryHeaderTable and " +
				"		" + CusEntryNumSchema.Constants.CE_EntryNum + " = @CE_EntryNum)))" +
				"or " + JobShipmentSchema.Constants.PK + " in (select " + CusEntryNumSchema.Constants.CE_ParentID + " from dbo.CusEntryNum where " +
				CusEntryNumSchema.Constants.CE_ParentTable + " = @JobShipmentTable " +
				"and " + CusEntryNumSchema.Constants.CE_EntryNum + " = @CE_EntryNum)";

			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add("@CusEntryHeaderTable", CusEntryHeaderSchema.Constants.TableName, CusEntryNumSchema.CE_ParentTable);
			@params.Add("@JobShipmentTable", JobShipmentSchema.Constants.TableName, CusEntryNumSchema.CE_ParentTable);
			@params.Add(ZSqlParameter.New("@CE_EntryNum", value, CusEntryNumSchema.CE_EntryNum, sqlOperator));

			ZDBOnlyQuery shipmentFilter = new ZDBOnlyQuery(typeof(CommonShipment));
			shipmentFilter.AddFilterAndZSQLParameterCollection(sQL, @params);
			return shipmentFilter;
		}

		#endregion

		#region GetInvoiceNumberFilter

		ZQuery GetInvoiceNumberFilter(SQLComparisonOperator sqlOperator, ZString value)
		{
			//TODO: Needs to work with multiple values.
			string sQL = JobShipmentSchema.Constants.PK + " IN" +
				" (SELECT " + JobDeclarationSchema.Constants.JE_JS + " FROM " + JobDeclarationSchema.Constants.SqlSchemaName + "." + JobDeclarationSchema.Constants.TableName +
				" WHERE " + JobDeclarationSchema.Constants.PK + " IN" +
				" (SELECT " + JobComInvoiceHeaderSchema.Constants.JZ_JE + " FROM " + JobComInvoiceHeaderSchema.Constants.SqlSchemaName + "." + JobComInvoiceHeaderSchema.Constants.TableName +
				" WHERE " + JobComInvoiceHeaderSchema.Constants.JZ_InvoiceNumber + " LIKE @JZ_InvoiceNumber))";

			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add(ZSqlParameter.New("@JZ_InvoiceNumber", value, JobComInvoiceHeaderSchema.JZ_InvoiceNumber, sqlOperator));

			ZDBOnlyQuery shipmentFilter = new ZDBOnlyQuery(typeof(CommonShipment));
			shipmentFilter.AddFilterAndZSQLParameterCollection(sQL, @params);
			return shipmentFilter;
		}

		#endregion

		#region GetLoadListNumberFilter

		ZQuery GetLoadListNumberFilter(SQLComparisonOperator sqlOperator, ZString value)
		{
			if (sqlOperator == SpecialComparisonOperator.IsBlank)
			{
				var shipmentIsBlankQuery = GetLoadListNumberFilterIsBlank();

				return shipmentIsBlankQuery;
			}

			var isNotQuery = sqlOperator.IsNegativeSQLOperator();

			var shipmentQuery = new ZDBOnlyQuery(typeof(CommonShipment));
			var pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS, isNotQuery);
			var consolSubQuery = new ZDBOnlySubQuery(typeof(CommonConsol), JobConShipLinkSchema.JN_JK);

			var operatorForConsolSubQuery = isNotQuery ? sqlOperator.GetNegatingSQLOperatorIfNotInSubquery() : sqlOperator;
			consolSubQuery.AddToFilter_PossiblyCommaSeparated(JobConsolSchema.JK_UniqueConsignRef, operatorForConsolSubQuery, value);
			consolSubQuery.AddToFilter(JoinCondition.And, JobConsolSchema.JK_IsCFS, SQLComparisonOperator.Equal, true);

			pivotSubQuery.AddSubQuery(consolSubQuery, JoinCondition.And);
			shipmentQuery.AddSubQuery(pivotSubQuery, JoinCondition.And);

			if (isNotQuery)
			{
				var hasConsolQuery = new ZDBOnlyQuery(typeof(CommonShipment));
				var allPivotsSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
				var allConsolSubQuery = new ZDBOnlySubQuery(typeof(CommonConsol), JobConShipLinkSchema.JN_JK);
				allConsolSubQuery.AddToFilter(JoinCondition.And, JobConsolSchema.JK_IsCFS, SQLComparisonOperator.Equal, true);
				allPivotsSubQuery.AddSubQuery(allConsolSubQuery, JoinCondition.And);

				hasConsolQuery.AddSubQuery(allPivotsSubQuery, JoinCondition.And);
				shipmentQuery.AddToFilter(hasConsolQuery, JoinCondition.And);
			}

			return shipmentQuery;
		}

		ZQuery GetLoadListNumberFilterIsBlank()
		{
			var shipmentQuery = new ZDBOnlyQuery(typeof(CommonShipment));

			var isCfsSubQuery = new ZDBOnlySubQuery(typeof(CommonConsol), JobConShipLinkSchema.JN_JK);
			isCfsSubQuery.AddToFilter(JoinCondition.And, JobConsolSchema.JK_IsCFS, SQLComparisonOperator.Equal, true);

			var notInJobConShipLinkSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS, true);
			notInJobConShipLinkSubQuery.AddSubQuery(isCfsSubQuery, JoinCondition.And);

			shipmentQuery.AddSubQuery(notInJobConShipLinkSubQuery, JoinCondition.And);

			return shipmentQuery;
		}

		#endregion

		#endregion

		#region AddOrganisationFilters

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter(Descriptions.Client, ModuleIDs.Organisation, JobShipmentSchema.JS_OH_HandledOnBehalfOfForwarder, ForwarderList).MultilingualDescription = ResString.GetMultilingualString("57a48ca1-bac1-41d6-b61f-5d80cc7c3442", "Client");

			var consignorTerminology = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value.IsEmpty ? (ZString)FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue : ((ZString)FreightDataRegistry.Instance.ConsignorShipperTerminology.Value).SubstringSafe(0, 15);
			var cnrCneFilter = filters.AddGuidFilter(FreightDataRegistry.Instance.ConsignorShipperTerminology.Value.GetUnresolvedString() + " / Consignee", ModuleIDs.Organisation, GetConsignorConsigneeFilter, ConsignorList, ConsigneeList);
			cnrCneFilter.MultilingualDescription = ResString.GetMultilingualString("f62a248b-fec0-466a-a7da-0433630d459e", "{0} / Consignee", FreightDataRegistry.Instance.ConsignorShipperTerminology.Value);
			cnrCneFilter.SetItemDescriptions(new ResourceStringData("", consignorTerminology), Res.GetData("7d0a735b-3230-4b99-88c2-41b421e4bb2d", "Consignee"));
		}

		#region GetConsignorConsigneeFilter

		ZQuery GetConsignorConsigneeFilter(ZGuid consignor, ZGuid consignee)
		{
			ZQuery shipmentFilter = new ZQuery();
			shipmentFilter.AddToFilter(AddressFilter(DocAddressType.ConsignorDocumentaryAddress, consignor));
			shipmentFilter.AddToFilter(AddressFilter(DocAddressType.ConsigneeDocumentaryAddress, consignee));
			return shipmentFilter;
		}

		#endregion

		#endregion

		#region AddTextFilters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddNkFilter(Descriptions.ServiceLevel, JobShipmentSchema.JS_RS_NKServiceLevel, ModuleIDs.ServiceLevel, ServiceLevelList);
			filter.MultilingualDescription = ResString.GetMultilingualString("1f80f856-921b-42f8-aed2-b558fe2fc5a0", "Service Level");
			filter.Category = FilterCategories.TextSearch;

			filters.AddTextFilter(Descriptions.TransportMode, JobShipmentSchema.JS_TransportMode, TransportModeList).MultilingualDescription = ResString.GetMultilingualString("1fedac88-58f7-4868-9c2c-62e1641dd33b", "Transport Mode");

			if (WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.Value)
			{
				var moduleWarehouseLocationFilter = new ModuleWarehouseLocationFilter(Descriptions.WarehouseLocation, GetWarehouseLocationFilter, WarehouseList)
							.WithMaxLengthOf<ModuleWarehouseLocationFilter>(JobShipmentSchema.JS_WarehouseLocation);
				moduleWarehouseLocationFilter.MultilingualDescription = ResString.GetMultilingualString("c0aa6fd0-b718-409e-9284-17777fee5613", "Warehouse Location");
				filters.AddCustomFilter(moduleWarehouseLocationFilter);
			}
			else
			{
				filters.AddTextFilter(Descriptions.WarehouseLocation, GetWarehouseLocationFilter)
					.WithMaxLengthOf<ModuleTextFilter>(JobShipmentSchema.JS_WarehouseLocation)
					.MultilingualDescription = ResString.GetMultilingualString("c0aa6fd0-b718-409e-9284-17777fee5613", "Warehouse Location");
			}

			// This should be later moved to the AccountingFilterStripCreator class in Accounting.sln. Which is the right place for JobHeader filters.
			// See method AccountingFilterStripCreator.GetBaseJobSubQuery()
			var jobHoldReasonFilter = filters.AddTextFilter("Job Status Hold Reason", JobHeaderSchema.JH_HoldReason);
			jobHoldReasonFilter.MultilingualDescription = ResString.GetMultilingualString("d0177219-2e6d-4be9-8cd0-1680e8dd9079", "Job Status Hold Reason");
			jobHoldReasonFilter.SubGroup = JobHeaderSubGroup;
		}

		ModuleFilterSubGroup JobHeaderSubGroup => jobHeaderSubGroup ??= new JobHeaderFilterSubGroup();
		JobHeaderFilterSubGroup jobHeaderSubGroup;

		class JobHeaderFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				if (filter.IsEmpty)
				{
					return filter;
				}

				var jobHeader = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
				jobHeader.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				jobHeader.AddToFilter(filter);

				var shipment = new ZDBOnlyQuery(typeof(CommonShipment));
				shipment.AddSubQuery(jobHeader, JoinCondition.And);

				return shipment;
			}
		}

		#region GetWarehouseLocationFilter

		ZQuery GetWarehouseLocationFilter(SQLComparisonOperator sqlOperator, ZString value)
		{
			if (sqlOperator.IsNegativeSQLOperator())
			{
				var negatedLocationFilter = new ZDBOnlySubQuery(typeof(PackLocation), JobPackLocSchema.JQ_JL);
				negatedLocationFilter.AddToFilter(JobPackLocSchema.JQ_WarehouseLocation, sqlOperator.GetNegatingSQLOperatorIfNotInSubquery(), value);

				var negatedPacklineFilter = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.JL_JS);
				negatedPacklineFilter.AddSubQuery(negatedLocationFilter, JoinCondition.And);

				var negatedShipmentFilter = new ZDBOnlySubQuery(typeof(CommonShipment), JobShipmentSchema.PK, notIn: true);
				negatedShipmentFilter.AddToFilter(JobShipmentSchema.JS_WarehouseLocation, sqlOperator.GetNegatingSQLOperatorIfNotInSubquery(), value);
				negatedShipmentFilter.AddSubQuery(negatedPacklineFilter, JoinCondition.Or);

				var shipmentFilter = new ZDBOnlyQuery(typeof(CommonShipment));
				shipmentFilter.AddSubQuery(negatedShipmentFilter, JoinCondition.And);

				return shipmentFilter;
			}
			else
			{
				var locationFilter = new ZDBOnlySubQuery(typeof(PackLocation), JobPackLocSchema.JQ_JL);
				locationFilter.AddToFilter(JobPackLocSchema.JQ_WarehouseLocation, sqlOperator, value);

				var packlineFilter = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.JL_JS);
				packlineFilter.AddSubQuery(locationFilter, JoinCondition.And);

				var shipmentFilter = new ZDBOnlyQuery(typeof(CommonShipment));
				shipmentFilter.AddToFilter(JobShipmentSchema.JS_WarehouseLocation, sqlOperator, value);
				shipmentFilter.AddSubQuery(packlineFilter, JoinCondition.Or);
				return shipmentFilter;
			}
		}

		ZQuery GetWarehouseLocationFilter(ZQuery packLocationFilter)
		{
			var whsLocationFilter = new ZDBOnlySubQuery(typeof(IWhsLocation), WhsLocationViewSchema.PK);
			whsLocationFilter.AddToFilter(packLocationFilter);

			var locationFilter = new ZDBOnlySubQuery(typeof(PackLocation), JobPackLocSchema.JQ_JL);
			locationFilter.AddSubQuery(JobPackLocSchema.JQ_WL, whsLocationFilter, JoinCondition.And);

			var packlineFilter = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.JL_JS);
			packlineFilter.AddSubQuery(locationFilter, JoinCondition.And);

			var shipmentFilter = new ZDBOnlyQuery(typeof(CommonShipment));
			shipmentFilter.AddSubQuery(JobShipmentSchema.JS_WL, whsLocationFilter, JoinCondition.And);
			shipmentFilter.AddSubQuery(packlineFilter, JoinCondition.Or);

			return shipmentFilter;
		}

		#endregion

		#endregion

		#region AddVoyageVesselFilter

		void AddVoyageVesselFilter(ModuleFilterCollection filters)
		{
			var filter = new VoyageVesselModuleFilter(Descriptions.VesselVoyageFlight, GetVoyageVesselFlightFilter, VesselList)
				.WithMaxLengthOf(JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
			filter.MultilingualDescription = ResString.GetMultilingualString("82a27e94-0cb5-46b0-87b2-9c5be218e02d", "Voyage / Flight / Vessel");
			filter.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(filter);
		}

		ZQuery GetVoyageVesselFlightFilter(SQLComparisonOperator sqlOperator, ZString voyageFlight, ZString vessel, ZBool includeArchived)
		{
			var builder = new SailingFilterBuilder(Factory);
			builder.Vessel = vessel;
			builder.VoyageFlight = voyageFlight;
			builder.VoyageFlightComparisonOperator = sqlOperator;
			builder.IncludeArchived = includeArchived;
			return builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.AllSchedules);
		}

		#endregion

		#region AddLocationFilters

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			ModuleLocationFilter originDestination = filters.AddLocationFilter(Descriptions.OriginDestination, JobShipmentSchema.JS_RL_NKOrigin, LocationList, JobShipmentSchema.JS_RL_NKDestination, LocationList);
			originDestination.MultilingualDescription = ResString.GetMultilingualString("a6e3ad01-55ff-451d-bb50-dc2a4af64d06", "Origin / Destination");
			originDestination.SetItemDescriptions(Res.GetData("2f41ab50-8144-4233-a56b-af400393fd99", "Origin"), Res.GetData("5556b3ed-2b38-4ba9-8928-79607e09ad07", "Destination"));

			ModuleLocationFilter loadListEndPorts = filters.AddLocationFilter(Descriptions.LoadListEndPorts, GetLoadListEndPortsFilter, LocationList, LocationList);
			loadListEndPorts.MultilingualDescription = ResString.GetMultilingualString("a6ae9f9b-7eb6-43b0-a350-b734109f5857", "Load List End Ports");
			loadListEndPorts.SetItemDescriptions(Res.GetData("b999cbe5-cd6e-49c3-aeca-782c7ebbe16f", "First Load"), Res.GetData("bf6c3f52-bfc3-4e8d-a478-7e00d2bbdb02", "Final Discharge"));

			ModuleLocationFilter loadDischarge = filters.AddLocationFilter(Descriptions.LoadDischarge, GetLoadDischargePorts, LocationList, LocationList);
			loadDischarge.MultilingualDescription = ResString.GetMultilingualString("f48e6c23-abd4-4bf1-95fd-a56b1cb57ad3", "Load / Discharge");
			loadDischarge.SetItemDescriptions(Res.GetData("653f7361-1d69-49af-a0cc-512caee260c8", "Load"), Res.GetData("15cb7a08-f60c-4db1-b1fa-5838e91a70d0", "Discharge"));
		}

		#region GetLoadListEndPortsFilter

		ZQuery GetLoadListEndPortsFilter(ZString firstLoad, ZString finalDischarge)
		{
			ZDBOnlySubQuery loadListFilter = new ZDBOnlySubQuery(typeof(CommonConsol), JobConShipLinkSchema.JN_JK);

			if (!firstLoad.IsEmpty)
			{
				SQLComparisonOperator loadOpp = (firstLoad.Length == 2 ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal);
				loadListFilter.AddToFilter(JobConsolSchema.JK_RL_NKLoadPort, loadOpp, firstLoad);
			}

			if (!finalDischarge.IsEmpty)
			{
				SQLComparisonOperator discOpp = (finalDischarge.Length == 2 ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal);
				loadListFilter.AddToFilter(JobConsolSchema.JK_RL_NKDischargePort, discOpp, finalDischarge);
			}

			ZDBOnlySubQuery linkFilter = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			linkFilter.AddSubQuery(loadListFilter, JoinCondition.And);

			ZDBOnlyQuery shipmentFilter = new ZDBOnlyQuery(typeof(CommonShipment));
			shipmentFilter.AddSubQuery(linkFilter, JoinCondition.And);
			return shipmentFilter;
		}

		#endregion

		#region GetLoadDischargePorts

		ZQuery GetLoadDischargePorts(ZString load, ZString discharge)
		{
			SailingFilterBuilder builder = new SailingFilterBuilder(Factory);
			builder.LoadPort = load;
			builder.DischargePort = discharge;
			return builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.ViaConsol);
		}

		#endregion

		#endregion

		#region AddDateFilters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			if (!Globals.IsWeb)
			{
				filters.AddDateFilter(Descriptions.ATA, ATAFilter).MultilingualDescription = ResString.GetMultilingualString("5e07bba1-bb6a-4bfa-8196-c3f0c8a0e886", "ATA");
				filters.AddDateFilter(Descriptions.ATD, ATDFilter).MultilingualDescription = ResString.GetMultilingualString("a3613aa7-b77a-4853-81d7-cf4607e37599", "ATD");
			}
			filters.AddDateFilter(Descriptions.ETA, ETAFilter).MultilingualDescription = ResString.GetMultilingualString("a195ded7-4593-480a-8d90-efbfea8079aa", "ETA");
			filters.AddDateFilter(Descriptions.ETD, ETDFilter).MultilingualDescription = ResString.GetMultilingualString("5CB7D08E-5004-4282-94F0-70BD986DD63F", "ETD");
			filters.AddDateFilter(Descriptions.ReceivedDate, JobShipmentSchema.JS_A_RCV).MultilingualDescription = ResString.GetMultilingualString("47510c6e-6dc7-4df1-a0af-8b72f6b997ab", "Received Date");
		}

		ZQuery ATAFilter(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			return DateFilter(comparisonOperator, SailingFilterBuilder.Dates.ATA, fromDate, toDate);
		}

		ZQuery ATDFilter(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			return DateFilter(comparisonOperator, SailingFilterBuilder.Dates.ATD, fromDate, toDate);
		}

		ZQuery ETAFilter(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			return DateFilter(comparisonOperator, SailingFilterBuilder.Dates.ETA, fromDate, toDate);
		}

		ZQuery ETDFilter(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			return DateFilter(comparisonOperator, SailingFilterBuilder.Dates.ETD, fromDate, toDate);
		}

		#endregion

		#region AddStatusAndFlagsFilters

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter activeFilter = filters.AddTextFilter(Descriptions.ActiveStatus, GetActiveFilter, ActiveFilterList);
			activeFilter.MultilingualDescription = ResString.GetMultilingualString("f89060d6-08df-4272-b2fd-cd07900b5338", "Active Status");
			activeFilter.Category = FilterCategories.StatusAndFlags;
			activeFilter.DefaultProperty = ActiveFilter.Active;

			if (!Globals.IsWeb)
			{
				var filter = filters.AddFlagsFilter(Descriptions.Flags,
					new string[] { Descriptions.LoadListUnassigned, Descriptions.ChargesNotPosted },
					new GetFlagsQuery[] { GetLoadListUnassignedFilter, GetChargesNotPostedFilter }
					);
				filter.MultilingualDescription = ResString.GetMultilingualString("62eeda61-e4cd-48a1-bb10-ba15c34e4399", "Flags");
				filter.Category = FilterCategories.StatusAndFlags;
			}
		}

		#region GetActiveFilter

		ZQuery GetActiveFilter(ZString value)
		{
			ZQuery result = new ZQuery();

			if (value == ActiveFilter.All.GetUnresolvedString())
			{
				result.IgnoreActiveFilter = true;
			}
			else if (value == ActiveFilter.Inactive.GetUnresolvedString())
			{
				result.IgnoreActiveFilter = true;
				result.AddToFilter(JobShipmentSchema.JS_IsCancelled, true);
			}

			return result;
		}

		#endregion

		#region GetChargesNotPostedFilter

		ZQuery GetChargesNotPostedFilter(ZBool value)
		{
			ZQuery shipmentFilter = new ZQuery();

			if (value)
			{
				string sqlText = String.Format(CultureInfo.InvariantCulture, @" JS_PK IN
					(SELECT {0} FROM dbo.JobHeader
					WHERE {1} = @CurrentCompany
					AND (JH_PK IN
						(SELECT {2} FROM dbo.AccTransactionLines WHERE {3} = 'ACR' AND {4} IS NULL)
					OR JH_PK NOT IN
						(SELECT {2} FROM dbo.AccTransactionLines WHERE {3} = 'CST' AND {2} IS NOT NULL)))",
						JobHeaderSchema.Constants.JH_ParentID, JobHeaderSchema.Constants.JH_GC,
						AccTransactionLinesSchema.Constants.AL_JH, AccTransactionLinesSchema.Constants.AL_LineType,
						AccTransactionLinesSchema.Constants.AL_ReverseDate);

				ZSqlParameterCollection @params = new ZSqlParameterCollection();
				@params.Add("@CurrentCompany", GlbCompany.CurrentCompany.PK, JobHeaderSchema.JH_GC);

				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CommonShipment));
				result.AddFilterAndZSQLParameterCollection(sqlText, @params);
				shipmentFilter.AddToFilter(result);
			}

			return shipmentFilter;
		}

		#endregion

		#region GetLoadListUnassignedFilter

		ZQuery GetLoadListUnassignedFilter(ZBool value)
		{
			ZQuery shipmentFilter = new ZQuery();

			if (value)
			{
				string sQL = "NOT EXISTS " +
					"(SELECT * FROM " + JobConShipLinkSchema.Constants.SqlSchemaName + "." + JobConShipLinkSchema.Constants.TableName +
					" WHERE " + AutoJobConShipLink.Schema.JN_JS + " = " + CommonShipment.Schema.PK + ")";

				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CommonShipment));
				result.AddFilterAndZSQLParameterCollection(sQL, null);

				shipmentFilter.AddToFilter(result);
			}

			return shipmentFilter;
		}

		#endregion

		#endregion

		#endregion

		#region workflow filter

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			helpers.AddRange(AddCustomFilterStripsHelper());
			return helpers;
		}

		protected virtual List<IFilterStripsHelper> AddCustomFilterStripsHelper()
		{
			return new List<IFilterStripsHelper>
			{
				new WorkflowFilterStripsHelperWithRoutingSupport(typeof(CFSShipment), JobInvoicingConsumerTypes.CFSShipment.Code, Factory)
			};
		}

		#endregion

		#region BindingLists

		#region ActiveFilterList

		public static class ActiveFilter
		{
			public static MultilingualString Active { get { return ResString.GetMultilingualString("0cea1ef8-bc94-4f38-9cbe-af3ba7c690cc", "Active"); } }
			public static MultilingualString All { get { return ResString.GetMultilingualString("9ace5fb7-9796-4117-8a24-928ca9ecc8f2", "All"); } }
			public static MultilingualString Inactive { get { return ResString.GetMultilingualString("c062a773-8ef7-4e4a-a493-9f6c8c33a93c", "Inactive"); } }
		}

		CodeDescriptionPairList ActiveFilterList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(ActiveFilter.Active, ResString.GetMultilingualString("ShipmentReceivalFilterStrip|ActiveStatusList|Active", "Show only active shipments (default)."));
				result.AddPair(ActiveFilter.Inactive, ResString.GetMultilingualString("ShipmentReceivalFilterStrip|ActiveStatusList|Inactive", "Show only inactive shipments."));
				result.AddPair(ActiveFilter.All, ResString.GetMultilingualString("ShipmentReceivalFilterStrip|ActiveStatusList|All", "Show both active and inactive shipments."));
				return result;
			}
		}

		#endregion

		#region ConsigneeList

		ConsigneeCollection ConsigneeList
		{
			get { return new ConsigneeCollection(Factory); }
		}

		#endregion

		#region ConsignorList

		ConsignorCollection ConsignorList
		{
			get { return new ConsignorCollection(Factory); }
		}

		#endregion

		#region ForwarderList

		ForwarderCollection ForwarderList
		{
			get { return new ForwarderCollection(Factory); }
		}

		#endregion

		#region LocationList

		public LocationCollection LocationList
		{
			get { return new LocationCollection(Factory); }
		}

		#endregion

		#region ServiceLevelCollection

		RefServiceLevelCollection ServiceLevelList
		{
			get { return new RefServiceLevelCollection(Factory); }
		}

		#endregion

		#region TransportModeList

		CodeDescriptionPairList TransportModeList
		{
			get { return FreightCodePairLists.LinkableTransportModeList(); }
		}

		#endregion

		#region VesselList

		RefVesselCollection VesselList
		{
			get { return new RefVesselCollection(Factory); }
		}

		#endregion

		#region WarehouseList

		public IWhsWarehouseCollection WarehouseList
		{
			get
			{
				if (warehouseList == null)
				{
					warehouseList = ObjectFactory.Get<IWhsWarehouseCollection>("IWhsWarehouseCollection", Factory);
					warehouseList.Load();
				}
				return warehouseList;
			}
		}
		IWhsWarehouseCollection warehouseList;

		#endregion

		#endregion

		#region Implementation

		ZQuery DateFilter(DateComparisonOperator comparisonOperator, SailingFilterBuilder.Dates dates, ZDateTime fromDate, ZDateTime toDate)
		{
			SailingFilterBuilder builder = new SailingFilterBuilder(Factory);
			builder.SetDateRange(dates, comparisonOperator, fromDate, toDate);
			return builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.ViaConsol);
		}

		ZQuery AddressFilter(DocAddressType addressType, ZGuid orgPK)
		{
			ZQuery result;

			if (orgPK.IsValid)
			{
				ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, orgPK);

				ZDBOnlySubQuery docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
				docAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, addressType));
				docAddressSubQuery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressSubQuery, JoinCondition.And);

				ZDBOnlyQuery jobShipmentQuery = new ZDBOnlyQuery(typeof(CommonShipment));
				jobShipmentQuery.AddSubQuery(JobShipmentSchema.PK, docAddressSubQuery, JoinCondition.And);

				result = jobShipmentQuery;
			}
			else
			{
				result = new ZQuery();
			}

			return result;
		}

		bool UsedInAustralia
		{
			get
			{
				var australia = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
				return GlbCompany.CurrentCompany.Country.PK == australia.PK;
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
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CFSShipment));
			result.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return result;
		}

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new Dictionary<string, object>()
		{
			{ AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride, ResString.GetMultilingualString("ad3487ea-d677-40cb-91c7-3a868a1934a1", "Job Status") }
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
	}
}
