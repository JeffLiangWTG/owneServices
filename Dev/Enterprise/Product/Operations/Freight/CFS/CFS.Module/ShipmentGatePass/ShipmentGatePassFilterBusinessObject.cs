using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using JobContainerPackPivot = Enterprise.Freight.Business.JobContainerPackPivot;
using JobSailing = Enterprise.Freight.Business.JobSailing;
using JobVoyage = Enterprise.Freight.Business.JobVoyage;

namespace Enterprise.Freight.CFS.Module
{
	public class ShipmentGatePassFilterBusinessObject : FilterStripBusinessObject, IAccountingFilterStripHolder
	{
		public ShipmentGatePassFilterBusinessObject()
			: base()
		{
		}

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			ModuleNumberFilter filter = new ModuleNumberFilter(NumberFilterTypes.ShipmentID, JobShipmentSchema.JS_UniqueConsignRef);
			filter.MultilingualDescription = ResString.GetMultilingualString("44ad7fb9-8414-4b8b-90fb-34ce9018fb7f", "Shipment #");
			filter.IsCommon = true;
			return filter;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddNumberFilters(filters);
			AddDateFilters(filters);
			AddOrganisationFilters(filters);
			AddLocationFilters(filters);
			AddVoyageVesselFilters(filters);
			AddStatusFilters(filters);

			AccountingFilterStrip.AddJobManagementFilters(filters, Env.Security.CFSGatePassJobInvoicing);

			return filters;
		}

		#region Number Filter

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(NumberFilterTypes.ContainerNumber, GetContainerNumberQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobContainerSchema.JC_ContainerNum)
				.MultilingualDescription = ResString.GetMultilingualString("ddb0b67b-2a34-4eb6-aacb-eff6306b5e87", "Container #");

			var filter = filters.AddNumberFilter(NumberFilterTypes.HouseBill, JobShipmentSchema.JS_HouseBill);
			filter.MultilingualDescription = ResString.GetMultilingualString("51466cf2-7872-4b69-a07e-eae9433bc78a", "House Bill");
			filter.IsCommon = true;

			var gatePassIdFilter = filters.AddNumberFilter(NumberFilterTypes.GatePassID, GetGatePassIDQuery);
			gatePassIdFilter.MultilingualDescription = ResString.GetMultilingualString("890eb72a-7cdd-4220-8d2b-0ee83dd4699b", "Gate Pass ID");
			gatePassIdFilter.PropertyValidation += GatePassIdFilterValidation;
			gatePassIdFilter.UseMultiSearch = false; //TODO: Needs to work with multiple values. (AddGatePassIDTOquery)

			filters.AddNumberFilter(NumberFilterTypes.LoadListNo, GetLoadListQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobConsolSchema.JK_UniqueConsignRef)
				.MultilingualDescription = ResString.GetMultilingualString("ff6ad5d4-e822-451d-a417-092bfb1c370b", "Load List #");

			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
			{
				filters.AddNumberFilter(NumberFilterTypes.HouseCCN, GetHouseCCNQuery).MultilingualDescription = ResString.GetMultilingualString("8d126e66-d112-47b3-99d7-9a2e5a360b70", "House CCN");
			}
		}

		void GatePassIdFilterValidation(ZPropertyInfo info)
		{
			ZString valueAsString = (ZString)info.Value;

			ZString packPart = valueAsString.Contains(CommonPickupDeliveryConfirm.GatePassIDSeparator) ? valueAsString.SubstringSafe(valueAsString.IndexOf(CommonPickupDeliveryConfirm.GatePassIDSeparator) + 1) : valueAsString;

			if (CommonUtils.GetNumberRepresentation(packPart) > byte.MaxValue)
			{
				info.AddError(Res.GetString("39e7d158-1d52-4346-9942-26d7b293f77d", "The pack part of Gate Pass ID is invalid"));
			}
		}

		#endregion

		#region Number Filter Delegates

		ZQuery GetContainerNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddContainerNumberToQuery(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetGatePassIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddGatePassIDToQuery(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetLoadListQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddLoadListToQuery(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetHouseCCNQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ReferenceNumberFilterHelper<GatePassShipment>().GetReferenceNumberFilter(comparisonOperator,
				Constants.CountryCodes.Canada, CanadaAdditionalReferenceNumberTypes.Codes.CCN, value);
		}

		#endregion

		#region Number Filter Implementation

		protected void AddContainerNumberToQuery(ZQuery query, SQLComparisonOperator sqlOperator, object value)
		{
			var isBlankOperator = sqlOperator == SpecialComparisonOperator.IsBlank;
			var requiresNotIn = isBlankOperator || SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref sqlOperator);
			sqlOperator = isBlankOperator ? SpecialComparisonOperator.IsNotBlank : sqlOperator;

			var containerSubQuery = new ZDBOnlySubQuery(typeof(CFSContainer), JobContainerPackPivotSchema.J6_JC);
			containerSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobContainerSchema.JC_ContainerNum, sqlOperator, value);

			var containerPackLinePivotQuery = new ZDBOnlySubQuery(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JL);
			containerPackLinePivotQuery.AddSubQuery(containerSubQuery, JoinCondition.And);

			var packQuery = new ZDBOnlySubQuery(typeof(GatePassPackLine), JobPackLinesSchema.JL_JS, requiresNotIn);
			packQuery.AddSubQuery(containerPackLinePivotQuery, JoinCondition.And);

			var dbOnlyResult = new ZDBOnlyQuery(typeof(GatePassShipment));
			dbOnlyResult.AddSubQuery(packQuery, JoinCondition.And);

			query.AddToFilter(dbOnlyResult, JoinCondition.And);
		}

		protected void AddGatePassIDToQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			//TODO: Needs to work with multiple values.
			ZString valueAsString = (ZString)value;

			if (!valueAsString.IsEmpty)
			{
				ZString shipmentPart;
				ZString packPart;

				if (valueAsString.Contains(CommonPickupDeliveryConfirm.GatePassIDSeparator))
				{
					shipmentPart = valueAsString.SubstringSafe(0, valueAsString.IndexOf(CommonPickupDeliveryConfirm.GatePassIDSeparator));
					packPart = valueAsString.SubstringSafe(valueAsString.IndexOf(CommonPickupDeliveryConfirm.GatePassIDSeparator) + 1);
				}
				else
				{
					shipmentPart = "";
					packPart = valueAsString;
				}

				ZDBOnlySubQuery legs = new ZDBOnlySubQuery(typeof(CommonPickupDeliveryConfirm), JobTransportLegPackLineDivotSchema.J8_EU_PickupDeliverConfirm);

				if (!packPart.IsEmpty)
				{
					ZByte bite = (ZByte)Enterprise.Accounting.Integration.CommonUtils.GetNumberRepresentation(packPart);
					legs.AddToFilter(JobPickupDeliveryConfirmSchema.EU_GatePassCount, bite);
				}

				ZDBOnlySubQuery divots = new ZDBOnlySubQuery(typeof(CommonConfirmDivot), JobTransportLegPackLineDivotSchema.J8_JL);
				divots.AddSubQuery(legs, JoinCondition.And);

				ZDBOnlySubQuery packLines = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.JL_JS);
				packLines.AddSubQuery(divots, JoinCondition.And);

				ZDBOnlyQuery combine = new ZDBOnlyQuery(typeof(GatePassShipment));
				combine.AddSubQuery(packLines, JoinCondition.And);

				if (!shipmentPart.IsEmpty)
				{
					AddSubMatch(combine, JobShipmentSchema.JS_UniqueConsignRef, @operator, shipmentPart);
				}

				query.AddToFilter(combine, JoinCondition.And);
			}
		}

		protected void AddLoadListToQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			if (@operator == SpecialComparisonOperator.IsBlank)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(GatePassShipment));
				ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS, true);

				result.AddSubQuery(pivotSubQuery, JoinCondition.And);

				query.AddToFilter(result, JoinCondition.And);
			}
			else
			{
				var result = GetConsolQuery(JobConsolSchema.JK_UniqueConsignRef, @operator, (ZString)value);
				query.AddToFilter(result, JoinCondition.And);
			}
		}

		ZDBOnlyQuery GetConsolQuery(SchemaColumn consolSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			bool isNotQuery = comparisonOperator.IsNegativeSQLOperator();

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(GatePassShipment));
			ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS, isNotQuery);
			ZDBOnlySubQuery consolSubQuery = new ZDBOnlySubQuery(typeof(CFSLoadListConsol), JobConShipLinkSchema.JN_JK);

			var operatorForConsolSubQuery = isNotQuery ? comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery() : comparisonOperator;
			consolSubQuery.AddToFilter_PossiblyCommaSeparated(consolSchemaColumn, operatorForConsolSubQuery, value);
			pivotSubQuery.AddSubQuery(consolSubQuery, JoinCondition.And);
			result.AddSubQuery(pivotSubQuery, JoinCondition.And);

			if (isNotQuery)
			{
				ZDBOnlyQuery hasConsolQuery = new ZDBOnlyQuery(typeof(GatePassShipment));
				ZDBOnlySubQuery allPivotsSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
				hasConsolQuery.AddSubQuery(allPivotsSubQuery, JoinCondition.And);
				result.AddToFilter(hasConsolQuery, JoinCondition.And);
			}

			return result;
		}

		#endregion

		#region Date Filter

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(DateFilterTypes.Delivery, GetDeliveryDateQuery).MultilingualDescription = ResString.GetMultilingualString("f7691c6e-6d98-42fa-bfa9-a7e2a44ed4b0", "Delivery");
			filters.AddDateFilter(DateFilterTypes.ETD, GetETDQuery).MultilingualDescription = ResString.GetMultilingualString("b9eaee3a-e027-461b-a08b-5192c48af93e", "ETD");
		}

		#endregion

		#region Date Filter Delegates

		ZQuery GetDeliveryDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();
			AddDeliveryDateToQuery(query, comparisonOperator, fromDate, toDate);
			return query;
		}

		ZQuery GetETDQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();
			AddETDToQuery(query, comparisonOperator, fromDate, toDate);
			return query;
		}

		#endregion

		#region Date Filter Implementation

		protected void AddDeliveryDateToQuery(ZQuery query, DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(GatePassShipment));

			ZDBOnlySubQuery legs = new ZDBOnlySubQuery(typeof(CommonPickupDeliveryConfirm), JobTransportLegPackLineDivotSchema.J8_EU_PickupDeliverConfirm);
			AddDateTimeRange(legs, comparisonOperator, JoinCondition.And, JobPickupDeliveryConfirmSchema.EU_PickupDeliveryTime, fromDate, toDate, false, true);
			legs.AddToFilter(JoinCondition.And, JobPickupDeliveryConfirmSchema.EU_PickupDeliveryType, Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture);

			ZDBOnlySubQuery divots = new ZDBOnlySubQuery(typeof(CommonConfirmDivot), JobTransportLegPackLineDivotSchema.J8_JL);
			divots.AddSubQuery(legs, JoinCondition.And);

			ZDBOnlySubQuery packLines = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.JL_JS);
			packLines.AddSubQuery(divots, JoinCondition.And);

			dbOnlyResult.AddSubQuery(packLines, JoinCondition.And);
			query.AddToFilter(dbOnlyResult, JoinCondition.And);
		}

		protected void AddETDToQuery(ZQuery query, DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZDBOnlySubQuery originFilter = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
			AddDateTimeRange(originFilter, comparisonOperator, JoinCondition.And, JobVoyOriginSchema.JA_E_DEP, fromDate, toDate, false, true);

			ZDBOnlyQuery sailingFilter = new ZDBOnlyQuery(typeof(JobSailing));
			sailingFilter.AddSubQuery(originFilter, JoinCondition.And);

			query.AddToFilter(ShipmentFilterOnSailing(sailingFilter));
		}

		#endregion

		#region Organisation Filter

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddGuidFilter(OrgFilterTypes.Client, ModuleIDs.Organisation, JobShipmentSchema.JS_OH_HandledOnBehalfOfForwarder, Client_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("57a48ca1-bac1-41d6-b61f-5d80cc7c3442", "Client");
			filter.Category = FilterCategories.Organisations;

			var consignorTerminology = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value.IsEmpty ? (ZString)FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue : ((ZString)FreightDataRegistry.Instance.ConsignorShipperTerminology.Value).SubstringSafe(0, 15);
			var cnrCneFilter = filters.AddGuidFilter(String.Format(CultureInfo.InvariantCulture, "{0} / {1}", FreightDataRegistry.Instance.ConsignorShipperTerminology.Value.GetUnresolvedString(), OrgFilterTypes.Consignee), ModuleIDs.Organisation, GetConsignorConsigneeQuery, Consignor_List, Consignee_List);
			cnrCneFilter.MultilingualDescription = ResString.GetMultilingualString("f62a248b-fec0-466a-a7da-0433630d459e", "{0} / Consignee", FreightDataRegistry.Instance.ConsignorShipperTerminology.Value);
			cnrCneFilter.SetItemDescriptions(new ResourceStringData("", consignorTerminology), Res.GetData("0fd8fb29-5784-453d-a1fa-13eae31828fd", "Consignee"));
			cnrCneFilter.Category = FilterCategories.Organisations;
		}

		#endregion

		#region Organisation Filter Delegates

		ZQuery GetConsignorConsigneeQuery(ZGuid consignor, ZGuid consignee)
		{
			ZQuery query = new ZQuery();

			if (consignor.IsValid)
			{
				AddConsignorToQuery(query, consignor); //defaults to SQLComparisonOperator.Equal
			}

			if (consignee.IsValid)
			{
				AddConsigneeToQuery(query, consignee); //defaults to SQLComparisonOperator.Equal
			}

			return query;
		}

		#endregion

		#region Organisation Filter Implementation

		void AddConsignorToQuery(ZQuery query, object value)
		{
			query.AddToFilter(FilterByOrg((ZGuid)value, DocAddressTypes.Codes.ConsignorDocumentaryAddress));
		}

		void AddConsigneeToQuery(ZQuery query, object value)
		{
			query.AddToFilter(FilterByOrg((ZGuid)value, DocAddressTypes.Codes.ConsigneeDocumentaryAddress));
		}

		#endregion

		#region Location Filter

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddLocationFilter(PortFilterTypes.LoadDischarge, GetLoadDischargeQuery, Location_List, Location_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("3761a778-c393-426e-8a7b-408d4bb8d468", "Load / Discharge");
			filter.SetItemDescriptions(Res.GetData("46442f09-7f53-47fa-a023-b67c2cb29b0c", "Load"), Res.GetData("77919d83-ae68-45a1-acad-c3ed633c3971", "Discharge"));

			filter = filters.AddLocationFilter(PortFilterTypes.OriginDestination, GetOriginDestinationQuery, Location_List, Location_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("bd476326-afa1-424e-af34-e3a4ccf875e7", "Origin / Destination");
			filter.SetItemDescriptions(Res.GetData("bb426055-32f4-4534-a1dd-e3c74ad1d0af", "Origin"), Res.GetData("e3eda2e9-35ee-4d2b-9d86-7928d715109a", "Destination"));
		}

		#endregion

		#region Location Filter Delegates

		ZQuery GetLoadDischargeQuery(ZString loadNk, ZString dischargeNk)
		{
			ZQuery query = new ZQuery();

			if (!loadNk.IsEmpty)
			{
				bool isCountryCode = (loadNk.Length == 2);
				SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

				AddPortOfLoadingToQuery(query, comparisonOperator, loadNk);
			}

			if (!dischargeNk.IsEmpty)
			{
				bool isCountryCode = (dischargeNk.Length == 2);
				SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

				AddPortOfDischargeToQuery(query, comparisonOperator, dischargeNk);
			}

			return query;
		}

		ZQuery GetOriginDestinationQuery(ZString originNk, ZString destinationNk)
		{
			ZQuery query = new ZQuery();

			if (!originNk.IsEmpty)
			{
				bool isCountryCode = (originNk.Length == 2);
				SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

				query.AddToFilter(JobShipmentSchema.JS_RL_NKOrigin, comparisonOperator, originNk);
			}

			if (!destinationNk.IsEmpty)
			{
				bool isCountryCode = (destinationNk.Length == 2);
				SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

				query.AddToFilter(JobShipmentSchema.JS_RL_NKDestination, comparisonOperator, destinationNk);
			}

			return query;
		}

		#endregion

		#region Location Filter Implementation

		protected void AddPortOfLoadingToQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			if (!((ZString)value).IsEmpty)
			{
				ZDBOnlySubQuery originFilter = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
				originFilter.AddToFilter(JobVoyOriginSchema.JA_RL_NKPortOfLoading, @operator, value);

				ZDBOnlyQuery sailingFilter = new ZDBOnlyQuery(typeof(JobSailing));
				sailingFilter.AddSubQuery(originFilter, JoinCondition.And);

				query.AddToFilter(ShipmentFilterOnSailing(sailingFilter));
			}
		}

		protected void AddPortOfDischargeToQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			if (!((ZString)value).IsEmpty)
			{
				ZDBOnlySubQuery destinationFilter = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
				destinationFilter.AddToFilter(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, @operator, value);

				ZDBOnlyQuery sailingFilter = new ZDBOnlyQuery(typeof(JobSailing));
				sailingFilter.AddSubQuery(destinationFilter, JoinCondition.And);

				query.AddToFilter(ShipmentFilterOnSailing(sailingFilter));
			}
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
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GatePassShipment));

			if (!voyageFlight.IsEmpty || !vesselNk.IsEmpty || comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				ZDBOnlySubQuery voyageFilter = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyOriginSchema.JA_JV);

				VoyageVesselModuleFilterHelper.ApplyBasicVoyageVesselQuery(voyageFilter, comparisonOperator, voyageFlight, vesselNk, includeArchived, JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);

				ZDBOnlySubQuery originFilter = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
				originFilter.AddSubQuery(voyageFilter, JoinCondition.And);

				ZDBOnlyQuery sailingFilter = new ZDBOnlyQuery(typeof(JobSailing));
				sailingFilter.AddSubQuery(originFilter, JoinCondition.And);

				query.AddToFilter(ShipmentFilterOnSailing(sailingFilter));
			}

			return query;
		}

		#endregion

		#region Status Filter

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Delivery Status", GetDeliveryStatusQuery, DeliveryStatus_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("23cb59d4-1134-413e-8b21-b1f5246bf2c2", "Delivery Status");
			filter.Category = FilterCategories.StatusAndFlags;
		}

		#endregion

		#region Status Filter Delegates

		ZQuery GetDeliveryStatusQuery(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GatePassShipment));

			bool isFullyDelivered = (value == DeliveryStatus_List.FullyDeliveredShipments.Code);
			bool isNotFullyDelivered = (value == DeliveryStatus_List.NotFullyDeliveredShipments.Code);

			if (isFullyDelivered ^ isNotFullyDelivered)
			{
				ZString outturnIsEqualToDeliveredOrNot = isFullyDelivered ? "=" : "!=";
				ZString outturnIsZeroOrNot = isFullyDelivered ? "!=" : "=";
				ZString andOrOutturnIsZeroOrNot = isFullyDelivered ? (NoResString)"And" : "OR";

				ZString sQL = string.Format(CultureInfo.InvariantCulture, @"
									{3} In (
										SELECT {6}
										FROM dbo.JobShipment
										LEFT JOIN (
											SELECT {0} a, sum({1}) As Outturn
											FROM dbo.JobPackLines as pack
											WHERE {5} = @OUT
											GROUP BY {0}
										) As PackLines On a = {6}
										LEFT JOIN (
											SELECT {0} b, sum({2}) As Delivered
											FROM dbo.JobPackLines as packDiv
											JOIN dbo.JobTransportLegPackLineDivot On {12} = {13}
											LEFT JOIN dbo.JobPickupDeliveryConfirm On {7} = {8}
											WHERE {9} != 0 AND {14} = '{15}'
											GROUP BY {0}
										) As Divots On b = {6}
										WHERE IsNull(Outturn, 0) {4} IsNull(Delivered, 0) {10} IsNull(Outturn, 0) {11} 0)"
					,
					/* 0  */ JobPackLinesSchema.JL_JS.Name,
					/* 1  */ JobPackLinesSchema.JL_Outturn.Name,
					/* 2  */ JobTransportLegPackLineDivotSchema.J8_PackagesDelivered.Name,
					/* 3  */ JobShipmentSchema.PK.Name,
					/* 4  */ outturnIsEqualToDeliveredOrNot,
					/* 5  */ JobPackLinesSchema.JL_FreightMode.Name,
					/* 6  */ JobShipmentSchema.PK.Name,
					/* 7  */ JobPickupDeliveryConfirmSchema.PK.Name,
					/* 8  */ JobTransportLegPackLineDivotSchema.J8_EU_PickupDeliverConfirm.Name,
					/* 9  */ JobPickupDeliveryConfirmSchema.EU_GatePassCount.Name,
					/* 10 */ andOrOutturnIsZeroOrNot,
					/* 11 */ outturnIsZeroOrNot,
					/* 12 */ JobPackLinesSchema.PK.Name,
					/* 13 */ JobTransportLegPackLineDivotSchema.J8_JL.Name,
					/* 14 */ JobPickupDeliveryConfirmSchema.EU_PickupDeliveryType.Name,
					/* 15 */ Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture
					);

				ZSqlParameterCollection @params = new ZSqlParameterCollection();
				@params.Add("@OUT", "OUT", JobPackLinesSchema.JL_FreightMode);

				query.AddFilterAndZSQLParameterCollection(sQL, @params);
			}

			return query;
		}

		#endregion

		#region Filter Overrides

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(base.Filter);
				query.AddToFilter(JobShipmentSchema.JS_IsCFSRegistered, ZBool.True);
				query.AddToFilter(JobShipmentSchema.JS_TranshipToOtherCFS, ZBool.True);
				//query.AddToFilter(ExcludeShipmentsWithNoUnpackedContainers(), JoinCondition.And);
				return query;
			}
		}

		#endregion

		#region Filter Types

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related.")]
		public static class FilterTypes
		{
			public const string None = "None";
			public const string All = "All";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related.")]
		public static class NumberFilterTypes
		{
			public const string Common = "Common";
			public const string ContainerNumber = "Container #";
			public const string HouseBill = "House Bill";
			public const string ShipmentID = "Shipment #";
			public const string GatePassID = "Gate Pass ID";
			public const string LoadListNo = "Load List #";
			public const string HouseCCN = "House CCN";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related., Filter name")]
		public static class DateFilterTypes
		{
			public const string Delivery = "Delivery";
			public const string ETD = "ETD";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related.")]
		public static class OrgFilterTypes
		{
			public const string Client = "Client";
			public const string Consignee = "Consignee";
			public const string Consignor = "Consignor";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related.")]
		public static class PortFilterTypes
		{
			public const string LoadDischarge = "Load / Discharge";
			public const string OriginDestination = "Origin / Destination";
		}

		#endregion

		#region Lists

		DebtorCollection fClient_List;
		public DebtorCollection Client_List
		{
			get
			{
				if (fClient_List == null)
				{
					fClient_List = new DebtorCollection(Factory);
				}
				return fClient_List;
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

		FullyNotFullyDeliveredList fDeliveryStatus_List;
		public FullyNotFullyDeliveredList DeliveryStatus_List
		{
			get
			{
				if (fDeliveryStatus_List == null)
				{
					fDeliveryStatus_List = new FullyNotFullyDeliveredList();
				}
				return fDeliveryStatus_List;
			}
		}

		public class FullyNotFullyDeliveredList : CodeDescriptionPairList
		{
			public FullyNotFullyDeliveredList()
			{
				Add(AllShipments);
				Add(NotFullyDeliveredShipments);
				Add(FullyDeliveredShipments);
			}

			public readonly CodeDescriptionPair AllShipments = new CodeDescriptionPair("ALL", Res.GetString("109a7fb6-47e0-47c6-833b-023a97656584", "Show All Shipments"));
			public readonly CodeDescriptionPair NotFullyDeliveredShipments = new CodeDescriptionPair("NOT", Res.GetString("7a581143-0cc0-4f31-be98-3601f4f2f334", "Only not Fully Delivered Shipments"));
			public readonly CodeDescriptionPair FullyDeliveredShipments = new CodeDescriptionPair("DLV", Res.GetString("dcc162c3-072d-470a-8be0-e7e4ab387004", "Only Fully Delivered Shipments"));
		}

		#endregion

		#region Implementation

		#region ExcludeShipmentsWithNoUnpackedContainers (commented)

		//protected ZDBOnlyQuery ExcludeShipmentsWithNoUnpackedContainers()
		//{
		//    ZDBOnlySubQuery ContainerQuery = new ZDBOnlySubQuery(typeof(GatePassContainer), JobContainerPackPivotSchema.J6_JC);
		//    ContainerQuery.AddToFilter(JoinCondition.And, JobContainerSchema.JC_LCLUnpack, SQLComparisonOperator.NotEqual, null);

		//    ZDBOnlySubQuery PivotQuery = new ZDBOnlySubQuery(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JL);
		//    PivotQuery.AddSubQuery(ContainerQuery, JoinCondition.And);

		//    ZDBOnlySubQuery PackQuery = new ZDBOnlySubQuery(typeof(GatePassPackLine), JobPackLinesSchema.JL_JS);
		//    PackQuery.AddSubQuery(PivotQuery, JoinCondition.And);

		//    ZDBOnlyQuery ShipmentQuery = new ZDBOnlyQuery(typeof(GatePassShipment));
		//    ShipmentQuery.AddSubQuery(PackQuery, JoinCondition.And);

		//    return ShipmentQuery;
		//}

		#endregion

		void AddSubMatch(ZDBOnlyQuery query, SchemaStringColumn column, SQLComparisonOperator @operator, ZString subMatch)
		{
			if (subMatch.Length > 1 && Char.IsLetter(subMatch[0]))
			{
				query.AddToFilter(JoinCondition.And, column, @operator, subMatch);
				subMatch = subMatch.SubstringSafe(1);
			}
			if (subMatch.Length >= 1)
			{
				if (subMatch.Length < 5)
				{
					subMatch = subMatch.PadLeft(5, '0');
				}
				query.AddToFilter(new ZQuery(column, SQLComparisonOperator.EndsWith, subMatch.SubstringSafe(1)));
			}
		}

		ZDBOnlyQuery FilterByOrg(ZGuid orgPK, string addressType)
		{
			ZDBOnlySubQuery orgAddressFilter = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressFilter.AddToFilter(OrgAddressSchema.OA_OH, orgPK);

			ZDBOnlySubQuery docAddressFilter = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			docAddressFilter.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressFilter, JoinCondition.And);
			docAddressFilter.AddToFilter(JobDocAddressSchema.E2_AddressType, addressType);

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CFSShipment));
			result.AddSubQuery(JobShipmentSchema.PK, docAddressFilter, JoinCondition.And);

			return result;
		}

		ZDBOnlyQuery ShipmentFilterOnSailing(ZQuery sailingFilter)
		{
			ZDBOnlySubQuery sailingQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
			sailingQuery.AddToFilter(sailingFilter);

			ZDBOnlySubQuery transportFilter = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			transportFilter.AddSubQuery(JobConsolTransportSchema.JW_JX, sailingQuery, JoinCondition.And);

			ZDBOnlySubQuery consolFilter = new ZDBOnlySubQuery(typeof(CommonConsol), JobConsolSchema.PK);
			consolFilter.AddSubQuery(JobConsolSchema.PK, transportFilter, JoinCondition.And);

			ZDBOnlySubQuery conShipLinkFilter = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			conShipLinkFilter.AddSubQuery(JobConShipLinkSchema.JN_JK, consolFilter, JoinCondition.And);

			ZDBOnlySubQuery sailingSubFilter = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
			sailingSubFilter.AddToFilter(sailingFilter);

			ZDBOnlyQuery shipmentFilter = new ZDBOnlyQuery(typeof(CommonShipment));
			shipmentFilter.AddSubQuery(JobShipmentSchema.PK, conShipLinkFilter, JoinCondition.And);
			shipmentFilter.AddSubQuery(JobShipmentSchema.JS_JX, sailingSubFilter, JoinCondition.Or);

			return shipmentFilter;
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
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(GatePassShipment));
			result.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return result;
		}

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => null;

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

			var workflowHelper = new WorkflowFilterStripsHelperWithRoutingSupport(typeof(GatePassShipment), JobInvoicingConsumerTypes.CFSShipment.Code, Factory);
			helpers.Add(workflowHelper);

			return helpers;
		}

		#endregion
	}
}
