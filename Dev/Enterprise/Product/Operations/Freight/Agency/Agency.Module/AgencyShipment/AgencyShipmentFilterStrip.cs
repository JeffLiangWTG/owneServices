using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Module
{
	public abstract class AgencyShipmentFilterStrip : FilterStripBusinessObject, IAccountingFilterStripHolder
	{
		#region SuppressResourceStringsCheckRegion

		public abstract class Descriptions
		{
			public const string HiddenFilter = "Hidden Filter";

			public const string BookingRef = "Booking Ref / Release";
			public const string ContainerNumber = "Container #";
			public const string GoodsItemID = "Goods Item #";
			public const string EntryTypeAndNumber = "Entry Type & Number";
			public const string OceanBillOfLading = "Ocean Bill Of Lading";
			public const string AdditionalReferenceNumbers = "Additional Reference #";
			public const string ShipmentNumber = "Shipment #";
			public const string ShippersRef = "Shippers Ref";

			public const string BookingParty = "Booking Party";
			public const string Carrier = "Carrier";
			public const string ConsignorConsignee = "Consignor / Consignee";
			public const string NotifyParty = "Notify Party";
			public const string Principal = "Principal";
			public const string ConsignorCompanyName = "Consignor Company Name";
			public const string ConsigneeCompanyName = "Consignee Company Name";
			public const string LocalClient = "Local Client (Billing)";
			public const string LocalClientCompanyName = "Local Client (Billing) Company Name";
			public const string BookingPartyCompanyName = "Booking Party Company Name";
			public const string NotifyPartyCompanyName = "Notify Party Company Name";
			public const string ConsignorRelatedParties = "Consignor Related Parties";
			public const string ConsigneeRelatedParties = "Consignee Related Parties";
			public const string LocalClientRelatedParties = "Local Client Related Parties";

			public const string VoyageVessel = "Voyage / Vessel";

			public const string LoadDischarge = "Load / Discharge";
			public const string OriginDestination = "Origin / Destination";
			public const string BranchRelatedPorts = "Branch's Related Ports";
			public const string TradeLane = "Trade Lane";

			public const string ATA = "ATA";
			public const string ATD = "ATD";
			public const string BookedDate = "Booked Date";
			public const string ETA = "ETA";
			public const string ETD = "ETD";

			public const string SailingStatus = "Sailing Status";
			public const string ShipmentStatus = "Shipment Status";
			public const string ActiveStatus = "Active Status";

			public const string CargoType = "Cargo Type";
			public const string Commodity = "Commodity";
			public const string ServiceLevel = "Service Level";

			public const string LocalSellAmount = "Local Sell Amount";
			public const string ChargeDescription = "Charge Description";
		}

		#endregion

		#region SubGroups

		protected ModuleFilterSubGroup ContainerFilterProcessor => containerFilterProcessor ?? (containerFilterProcessor = new ContainerSubGroup());
		ContainerSubGroup containerFilterProcessor;

		protected ModuleFilterSubGroup JobHeaderFilterProcessor => jobHeaderFilterProcessor ?? (jobHeaderFilterProcessor = new JobHeaderSubGroup());
		JobHeaderSubGroup jobHeaderFilterProcessor;

		protected ModuleFilterSubGroup SailingQueryFilterProcessor => sailingQueryFilterProcessor ?? (sailingQueryFilterProcessor = new SailingQuerySubGroup());
		SailingQuerySubGroup sailingQueryFilterProcessor;

		protected ModuleFilterSubGroup JobHeaderFromOrgAddressProcessor => jobHeaderFromOrgAddressProcessor ?? (jobHeaderFromOrgAddressProcessor = new JobHeaderFromOrgAddressSubGroup(JobHeaderFilterProcessor));
		JobHeaderFromOrgAddressSubGroup jobHeaderFromOrgAddressProcessor;

		class ContainerSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var container = new ZDBOnlySubQuery(typeof(AgencyShipmentContainer), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
				container.AddToFilter(filter);
				var shipment = new ZDBOnlyQuery(typeof(AgencyShipment));
				shipment.AddSubQuery(container, JoinCondition.And);

				return shipment;
			}
		}

		class JobHeaderSubGroup : ModuleFilterSubGroup
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

				var shipment = new ZDBOnlyQuery(typeof(AgencyShipment));
				shipment.AddSubQuery(jobHeader, JoinCondition.And);

				return shipment;
			}
		}
		class SailingQuerySubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var sailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobShipmentSchema.JS_JX);
				sailingSubQuery.AddToFilter(filter);

				var result = new ZDBOnlyQuery(typeof(AgencyShipment));
				result.AddSubQuery(sailingSubQuery, JoinCondition.And);
				return result;
			}
		}

		class JobHeaderFromOrgAddressSubGroup : ModuleFilterSubGroup
		{
			public JobHeaderFromOrgAddressSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				if (filter.IsEmpty)
				{
					return filter;
				}

				var jobHeader = new ZDBOnlyQuery(typeof(JobHeader));
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr);
				orgAddressQuery.AddToFilter(filter);
				jobHeader.AddSubQuery(orgAddressQuery, JoinCondition.And);

				return jobHeader;
			}
		}

		#endregion

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			ModuleFountainFilter filter = new ModuleFountainFilter(Descriptions.ShipmentNumber, JobShipmentSchema.JS_UniqueConsignRef, "V");
			filter.IsCommon = true;
			filter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|ShipmentNumber", "Shipment #");

			return filter;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			AddNumberFilters(filters);
			AddOrganisationFilters(filters);
			AddVoyageVesselFilters(filters);
			AddLocationFilters(filters);
			AddDateFilters(filters);
			AddStatusAndFlagsFilters(filters);
			AddModesAndTypesFilters(filters);
			AddAccountingFilters(filters);
			return filters;
		}

		public override ZQuery Filter
		{
			get
			{
				ZQuery baseFilter = new ZQuery(JobShipmentSchema.JS_IsShipping, true);
				return new ZQuery(baseFilter, base.Filter);
			}
		}

		#region AddNumberFilters

		protected virtual void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(Descriptions.BookingRef, JobShipmentSchema.JS_CFSReference).MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|BookingRef", "Booking Ref / Release");
			var containerNumberFilter = filters.AddNumberFilter(Descriptions.ContainerNumber, ContainerNumberFilter)
				.WithMaxLengthOf<ModuleNumberFilter>(JobContainerSchema.JC_ContainerNum);
			containerNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|ContainerNumber", "Container #");
			containerNumberFilter.SubGroup = ContainerFilterProcessor;

			var goodsItemIDFilter = filters.AddNumberFilter(Descriptions.GoodsItemID, GoodsItemIDFilter)
				.WithMaxLengthOf<ModuleNumberFilter>(JobContainerSchema.JC_ContainerNum);
			goodsItemIDFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|GoodsItemID", "VIN / Pack ID");
			goodsItemIDFilter.SubGroup = ContainerFilterProcessor;

			var filter = filters.AddNumberFilter(Descriptions.OceanBillOfLading, JobShipmentSchema.JS_HouseBill);
			filter.IsCommon = true;
			filter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|OceanBillOfLading", "Ocean Bill Of Lading");

			filters.AddNumberFilter(Descriptions.ShippersRef, JobShipmentSchema.JS_BookingReference).MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|ShippersRef", "Shippers Ref");

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Australia)
			{
				var entryTypeAndNumberFilter = new EntryNumberModuleFilter(Descriptions.EntryTypeAndNumber, GetEntryNumberFilter)
					.WithMaxLengthOf<EntryNumberModuleFilter>(CusEntryNumSchema.CE_EntryNum);
				entryTypeAndNumberFilter.Category = FilterCategories.NumbersAndReferences;
				entryTypeAndNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|EntryTypeAndNumber", "Entry Type & Number");

				filters.AddCustomFilter(entryTypeAndNumberFilter);
			}

			var referenceNumberFilter = new ReferenceNumberFilter(Descriptions.AdditionalReferenceNumbers,
				new ReferenceNumberFilterHelper<AgencyShipment>().GetReferenceNumberFilter,
				new RefCountryCollection(Factory))
				.WithMaxLengthOf<ReferenceNumberFilter>(CusEntryNumSchema.CE_EntryNum);
			referenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("51d767eb-ed80-489c-a89a-2e0227372287", "Additional Reference #");

			filters.AddCustomFilter(referenceNumberFilter);
		}

		ZQuery ContainerNumberFilter(SQLComparisonOperator opp, ZString containerNumber)
		{
			var containerFilter = new ZDBOnlyQuery(typeof(AgencyShipmentContainer));
			containerFilter.AddToFilter_PossiblyCommaSeparated(JobContainerSchema.JC_ContainerNum, opp, containerNumber);
			containerFilter.AddToFilter(JobContainerSchema.JC_ContainerMode, SQLComparisonOperator.Equal, AgencyShipmentContainerModeList.GetContainerModes(Constants.ContainerModes.FCL));

			return containerFilter;
		}

		ZQuery GoodsItemIDFilter(SQLComparisonOperator opp, ZString goodsItemID)
		{
			var containerFilter = new ZDBOnlyQuery(typeof(AgencyShipmentContainer));
			containerFilter.AddToFilter_PossiblyCommaSeparated(JobContainerSchema.JC_ContainerNum, opp, goodsItemID);
			containerFilter.AddToFilter(JobContainerSchema.JC_ContainerMode, SQLComparisonOperator.Equal, AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes);

			return containerFilter;
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

			ZDBOnlySubQuery containerFilter = new ZDBOnlySubQuery(typeof(AgencyShipmentContainer), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
			containerFilter.AddSubQuery(entryFilter, JoinCondition.And);

			ZDBOnlyQuery shipmentFilter = new ZDBOnlyQuery(typeof(AgencyShipment));
			shipmentFilter.AddSubQuery(entryFilter, JoinCondition.Or);
			shipmentFilter.AddSubQuery(containerFilter, JoinCondition.Or);

			return shipmentFilter;
		}

		#endregion

		#region AddOrganisationFilters

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			ModuleFilter bookingPartyFilter = filters.AddGuidFilter(Descriptions.BookingParty, ModuleIDs.Organisation, BookingPartyFilter, Consignor_List);
			bookingPartyFilter.IsPublishedOnWeb = false;
			bookingPartyFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|BookingParty", "Booking Party");

			ModuleFilter carrierFilter = filters.AddGuidFilter(Descriptions.Carrier, ModuleIDs.Organisation, GetOrgAddressColumnQueryWithOperatorDelegate(JobShipmentSchema.JS_OA_BookedShippingLineAddress), Carrier_List);
			carrierFilter.IsPublishedOnWeb = false;
			carrierFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|Carrier", "Carrier");

			ModuleGuidsFilter cnrCneFilter = filters.AddGuidFilter(Descriptions.ConsignorConsignee, ModuleIDs.Organisation, ConsignorConsigneeFilter, Consignor_List, Consignee_List);
			cnrCneFilter.SetItemDescriptions(Res.GetData("f35909fd-6ac2-447f-a056-b72d5473bf01", "Consignor"), Res.GetData("f18cfcc1-8d2a-42f2-9ad2-e1f094c3b324", "Consignee"));
			cnrCneFilter.IsPublishedOnWeb = false;
			cnrCneFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|ConsignorConsignee", "Consignor / Consignee");

			ModuleFilter notifyPartyFilter = filters.AddGuidFilter(Descriptions.NotifyParty, ModuleIDs.Organisation, NotifyPartyFilter, Consignee_List);
			notifyPartyFilter.IsPublishedOnWeb = false;
			notifyPartyFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|NotifyParty", "Notify Party");

			ModuleGuidFilter principalFilter = filters.AddGuidFilter(Descriptions.Principal, ModuleIDs.Organisation, JobShipmentSchema.JS_OH_DeliveryAgent, Principal_List);
			principalFilter.IsPublishedOnWeb = false;
			principalFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|Principal", "Principal");

			if (!Env.Security.AgencyPrincipalAccess.IsAllowed && !Globals.IsWeb)
			{
				principalFilter.Visibility = FilterVisibility.AlwaysVisible;
				principalFilter.PropertyValidation = PrincipalFilterValidation;
			}

			ModuleFilter consignorCompanyNameFilter = filters.AddTextFilter(Descriptions.ConsignorCompanyName, GetConsignorCompanyNameQuery)
				.WithMaxLengthOf<ModuleFilter>(JobDocAddressSchema.E2_CompanyName);
			consignorCompanyNameFilter.Category = FilterCategories.Organisations;
			consignorCompanyNameFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|ConsignorCompanyName", "Consignor Company Name");

			ModuleFilter consigneeCompanyNameFilter = filters.AddTextFilter(Descriptions.ConsigneeCompanyName, GetConsigneeCompanyNameQuery)
				.WithMaxLengthOf<ModuleFilter>(JobDocAddressSchema.E2_CompanyName);
			consigneeCompanyNameFilter.Category = FilterCategories.Organisations;
			consigneeCompanyNameFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|ConsigneeCompanyName", "Consignee Company Name");

			ModuleGuidFilter localClientFilter = filters.AddGuidFilter(Descriptions.LocalClient, ModuleIDs.Organisation, GetLocalClientQuery, OrgHeader_List);
			localClientFilter.Category = FilterCategories.Organisations;
			localClientFilter.SubGroup = JobHeaderFromOrgAddressProcessor;
			localClientFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|LocalClient", "Local Client (Billing)");
			localClientFilter.MultiValueQueryDelegate = GetMultiValueLocalClientQuery;

			ModuleFilter localClientCompanyNameFilter = filters.AddTextFilter(Descriptions.LocalClientCompanyName, GetLocalClientCompanyNameQuery)
				.WithMaxLengthOf<ModuleFilter>(OrgHeaderSchema.OH_FullName);
			localClientCompanyNameFilter.Category = FilterCategories.Organisations;
			localClientCompanyNameFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|LocalClientCompanyName", "Local Client (Billing) Company Name");

			ModuleFilter bookingPartyCompanyNameFilter = filters.AddTextFilter(Descriptions.BookingPartyCompanyName, GetBookingPartyCompanyNameQuery)
				.WithMaxLengthOf<ModuleFilter>(JobDocAddressSchema.E2_CompanyName);
			bookingPartyCompanyNameFilter.Category = FilterCategories.Organisations;
			bookingPartyCompanyNameFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|BookingPartyCompanyName", "Booking Party Company Name");

			ModuleFilter notifyPartyCompanyNameFilter = filters.AddTextFilter(Descriptions.NotifyPartyCompanyName, GetNotifyPartyCompanyNameQuery)
				.WithMaxLengthOf<ModuleFilter>(JobDocAddressSchema.E2_CompanyName);
			notifyPartyCompanyNameFilter.Category = FilterCategories.Organisations;
			notifyPartyCompanyNameFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|NotifyPartyCompanyName", "Notify Party Company Name");

			ModuleFilter consignorRelatedPartiesFilter = new OrgRelatedPartiesModuleFilter(Descriptions.ConsignorRelatedParties, GetConsignorRelatedPartiesQuery);
			consignorRelatedPartiesFilter.Category = FilterCategories.Organisations;
			consignorRelatedPartiesFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|ConsignorRelatedParties", "Consignor Related Parties");
			filters.AddCustomFilter(consignorRelatedPartiesFilter);

			ModuleFilter consigneeRelatedPartiesFilter = new OrgRelatedPartiesModuleFilter(Descriptions.ConsigneeRelatedParties, GetConsigneeRelatedPartiesQuery);
			consigneeRelatedPartiesFilter.Category = FilterCategories.Organisations;
			consigneeRelatedPartiesFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|ConsigneeRelatedParties", "Consignee Related Parties");
			filters.AddCustomFilter(consigneeRelatedPartiesFilter);

			ModuleFilter localClientRelatedPartiesFilter = new OrgRelatedPartiesModuleFilter(Descriptions.LocalClientRelatedParties, GetLocalClientRelatedPartiesQuery);
			localClientRelatedPartiesFilter.Category = FilterCategories.Organisations;
			localClientRelatedPartiesFilter.SubGroup = JobHeaderFromOrgAddressProcessor;
			localClientRelatedPartiesFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|LocalClientRelatedParties", "Local Client Related Parties");
			filters.AddCustomFilter(localClientRelatedPartiesFilter);
		}

		ZQuery BookingPartyFilter(ZGuid bookingPartyPK)
		{
			return AddressFilter(DocAddressType.BookingPartyDocumentaryAddress, bookingPartyPK);
		}

		ZQuery ConsignorConsigneeFilter(ZGuid consignorPK, ZGuid consigneePK)
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(AddressFilter(DocAddressType.ConsignorDocumentaryAddress, consignorPK));
			result.AddToFilter(AddressFilter(DocAddressType.ConsigneeDocumentaryAddress, consigneePK));
			return result;
		}

		ZQuery NotifyPartyFilter(ZGuid notifyPartyPK)
		{
			return AddressFilter(DocAddressType.NotifyParty, notifyPartyPK);
		}

		ZQuery GetConsignorCompanyNameQuery(SQLComparisonOperator comparison, ZString text)
		{
			if (text.IsEmpty)
			{
				return new ZQuery();
			}

			return GetCompanyNameQuery(comparison, text, DocAddressTypes.Codes.ConsignorDocumentaryAddress);
		}

		ZQuery GetConsigneeCompanyNameQuery(SQLComparisonOperator comparison, ZString text)
		{
			if (text.IsEmpty)
			{
				return new ZQuery();
			}

			return GetCompanyNameQuery(comparison, text, DocAddressTypes.Codes.ConsigneeDocumentaryAddress);
		}

		ZQuery GetLocalClientCompanyNameQuery(SQLComparisonOperator comparison, ZString text)
		{
			if (text.IsEmpty)
			{
				return new ZQuery();
			}

			bool notIn = SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref comparison);

			var orgHeader = new ZQuery(OrgHeaderSchema.OH_FullName, comparison, text);
			var jobHeader = OrgRelatedPartiesFilterHelper.GetJobHeaderFromOrgHeaderFilter(orgHeader, new ZQuery(), false, false);

			return FromJobHeaderFilter(jobHeader, notIn);
		}

		ZQuery GetLocalClientQuery(ZGuid localClientPK)
		{
			if (!localClientPK.IsValid)
			{
				return new ZQuery();
			}

			ZQuery orgAddress = new ZQuery(OrgAddressSchema.OA_OH, localClientPK);

			return orgAddress;
		}

		ZQuery GetMultiValueLocalClientQuery(object localClientPKs, SQLComparisonOperator comparisonOperator)
		{
			ZQuery orgAddress = new ZQuery(OrgAddressSchema.OA_OH, localClientPKs);
			ZQuery jobHeader = OrgRelatedPartiesFilterHelper.GetJobHeaderFromOrgAddressFilter(orgAddress, false);

			return FromJobHeaderFilter(jobHeader, false);
		}

		ZQuery GetBookingPartyCompanyNameQuery(SQLComparisonOperator comparison, ZString text)
		{
			if (text.IsEmpty)
			{
				return new ZQuery();
			}

			return GetCompanyNameQuery(comparison, text, DocAddressTypes.Codes.BookingPartyDocumentaryAddress);
		}

		ZQuery GetNotifyPartyCompanyNameQuery(SQLComparisonOperator comparison, ZString text)
		{
			if (text.IsEmpty)
			{
				return new ZQuery();
			}

			return GetCompanyNameQuery(comparison, text, DocAddressTypes.Codes.NotifyParty);
		}

		ZQuery GetConsignorRelatedPartiesQuery(ZQuery orgHeaderFilter)
		{
			ZQuery docAddressFilter = OrgRelatedPartiesFilterHelper.GetDocAddressFromOrgHeaderFilter(orgHeaderFilter, new ZQuery(), false, false);
			docAddressFilter.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.ConsignorDocumentaryAddress);

			return FromDocAddressFilter(docAddressFilter, false);
		}

		ZQuery GetConsigneeRelatedPartiesQuery(ZQuery orgHeaderFilter)
		{
			ZQuery docAddressFilter = OrgRelatedPartiesFilterHelper.GetDocAddressFromOrgHeaderFilter(orgHeaderFilter, new ZQuery(), false, false);
			docAddressFilter.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.ConsigneeDocumentaryAddress);

			return FromDocAddressFilter(docAddressFilter, false);
		}

		ZQuery GetLocalClientRelatedPartiesQuery(ZQuery orgHeaderFilter)
		{
			return OrgRelatedPartiesFilterHelper.GetOrgAddressFromOrgHeaderFilter(orgHeaderFilter, false);
		}

		GetGuidQueryWithOperator GetOrgAddressColumnQueryWithOperatorDelegate(SchemaColumn orgAddressColumn)
		{
			return (SQLComparisonOperator comparisonOperator, object pK) => GetOrgAddressColumnQueryWithOperator(pK, orgAddressColumn, comparisonOperator);
		}

		ZQuery GetOrgAddressColumnQueryWithOperator(object pK, SchemaColumn orgAddressColumn, SQLComparisonOperator comparisonOperator)
		{
			var result = new ZDBOnlyQuery(typeof(AgencyShipment));
			var notIn = comparisonOperator == SQLComparisonOperator.IsBlank;
			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), orgAddressColumn, notIn);

			if (comparisonOperator == SQLComparisonOperator.IsBlank || comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				result.AddToFilter(orgAddressColumn, comparisonOperator == SQLComparisonOperator.IsBlank ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, null);
			}
			else
			{
				orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, comparisonOperator, (ZGuid)pK);
				result.AddSubQuery(orgAddressQuery, JoinCondition.And);
			}
			return result;
		}

		#endregion

		#region AddTextFilters

		void AddVoyageVesselFilters(ModuleFilterCollection filters)
		{
			var filter = new VoyageVesselModuleFilter(Descriptions.VoyageVessel, VoyageVesselFilter, Vessel_List)
				.WithMaxLengthOf(JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
			filter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|VesselVoyage", "Voyage / Vessel");
			filter.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(filter);
		}

		ZQuery VoyageVesselFilter(SQLComparisonOperator comparison, ZString voyage, ZString vessel, ZBool includeArchived)
		{
			var builder = new SailingFilterBuilder(Factory);
			builder.VoyageFlightComparisonOperator = comparison;
			builder.Vessel = vessel;
			builder.VoyageFlight = voyage;
			builder.IncludeArchived = includeArchived;
			return builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.Direct | SailingFilterBuilder.RelationshipFlags.ViaDirectTransports);
		}

		#endregion

		#region AddLocationFilters

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			ModuleLocationFilter loadDischargeFilter = filters.AddLocationFilter(Descriptions.LoadDischarge, LoadDischargeFilter, UNLOCO_List, UNLOCO_List);
			loadDischargeFilter.SetItemDescriptions(Res.GetData("472af06b-ec44-49a2-966c-b1447ec88bda", "Load"), Res.GetData("9a4b281d-a7be-4d91-84f4-ad73ac7f8fed", "Discharge"));
			loadDischargeFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|LoadDischarge", "Load / Discharge");

			ModuleLocationFilter originDestinationFilter = filters.AddLocationFilter(Descriptions.OriginDestination, OriginDestinationFilter, UNLOCO_List, UNLOCO_List);
			originDestinationFilter.SetItemDescriptions(Res.GetData("a04bd740-b5cf-4b24-9189-74480f640bae", "Origin"), Res.GetData("e76c4257-62cc-4319-8554-8b2588cf20ea", "Destination"));
			originDestinationFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|OriginDestination", "Origin / Destination");

			ModuleGuidFilter branchRelatedPortsFilter = filters.AddGuidFilter(Descriptions.BranchRelatedPorts, ModuleIDs.GlbBranch, GetBranchRelatedPorts, BranchList);
			branchRelatedPortsFilter.Category = FilterCategories.Locations;
			branchRelatedPortsFilter.IsPublishedOnWeb = false;
			branchRelatedPortsFilter.SubGroup = SailingQueryFilterProcessor;
			branchRelatedPortsFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|BranchRelatedPorts", "Branch's Related Ports");

			ModuleGuidFilter tradeLaneFilter = filters.AddGuidFilter(Descriptions.TradeLane, ModuleIDs.TradeLane, GetTradeLaneFilter, TradeLaneList);
			tradeLaneFilter.Category = FilterCategories.Locations;
			tradeLaneFilter.IsPublishedOnWeb = false;
			tradeLaneFilter.SubGroup = SailingQueryFilterProcessor;
			tradeLaneFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|TradeLane", "Trade Lane");

			if (!AllowSearchOfUnlocoOutsideLoginBranch && !Globals.IsWeb)
			{
				branchRelatedPortsFilter.Visibility = FilterVisibility.AlwaysVisible;
				branchRelatedPortsFilter.DefaultProperty = GlbBranch.CurrentBranch.PK;
				branchRelatedPortsFilter.PropertyValidation = BranchFilterValidation;
			}
		}

		ZQuery GetTradeLaneFilter(ZGuid value)
		{
			var result = new ZDBOnlyQuery(typeof(JobSailing));

			ZDBOnlySubQuery tradeLaneVoyageQuery = new ZDBOnlySubQuery(typeof(JobTradeLaneVoyage), JobTradeLaneVoyageSchema.NB_JV);
			tradeLaneVoyageQuery.AddToFilter(JobTradeLaneVoyageSchema.NB_EJ, value);
			ZDBOnlySubQuery voyageQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyOriginSchema.JA_JV);
			voyageQuery.AddSubQuery(tradeLaneVoyageQuery, JoinCondition.And);
			ZDBOnlySubQuery originQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
			originQuery.AddSubQuery(voyageQuery, JoinCondition.And);
			result.AddSubQuery(originQuery, JoinCondition.And);

			return result;
		}

		ZQuery LoadDischargeFilter(ZString load, ZString discharge)
		{
			SailingFilterBuilder builder = new SailingFilterBuilder(Factory);
			builder.LoadPort = load;
			builder.DischargePort = discharge;
			return builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.ViaDirectTransports);
		}

		ZQuery OriginDestinationFilter(ZString origin, ZString destination)
		{
			ZQuery result = new ZQuery();

			if (origin.Length == 2)
			{
				result.AddToFilter(JobShipmentSchema.JS_RL_NKOrigin, SQLComparisonOperator.StartsWith, origin);
			}
			else if (!origin.IsEmpty)
			{
				result.AddToFilter(JobShipmentSchema.JS_RL_NKOrigin, origin);
			}

			if (destination.Length == 2)
			{
				result.AddToFilter(JobShipmentSchema.JS_RL_NKDestination, SQLComparisonOperator.StartsWith, destination);
			}
			else if (!destination.IsEmpty)
			{
				result.AddToFilter(JobShipmentSchema.JS_RL_NKDestination, destination);
			}

			return result;
		}

		ZQuery GetBranchRelatedPorts(ZGuid value)
		{
			ZDBOnlySubQuery voyOriginQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobVoyOriginSchema.PK);
			ZDBOnlySubQuery voyDestinationQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobVoyDestinationSchema.PK);
			ZDBOnlySubQuery branchRelatedPortsQuery = new ZDBOnlySubQuery(typeof(GlbBranchExtraPorts), GlbBranchExtraPortsSchema.GY_RL_NKAdditionalBranchRelatedPort);
			ZDBOnlySubQuery branchHomePortQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.GB_RL_NKHomePort);

			branchRelatedPortsQuery.AddToFilter(GlbBranchExtraPortsSchema.GY_GB, value);
			branchHomePortQuery.AddToFilter(GlbBranchSchema.PK, value);

			voyOriginQuery.AddSubQuery(JobVoyOriginSchema.JA_RL_NKPortOfLoading, branchHomePortQuery, JoinCondition.And);
			voyOriginQuery.AddSubQuery(JobVoyOriginSchema.JA_RL_NKPortOfLoading, branchRelatedPortsQuery, JoinCondition.Or);

			voyDestinationQuery.AddSubQuery(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, branchHomePortQuery, JoinCondition.And);
			voyDestinationQuery.AddSubQuery(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, branchRelatedPortsQuery, JoinCondition.Or);

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobSailing));

			result.AddSubQuery(JobSailingSchema.JX_JA, voyOriginQuery, JoinCondition.And);
			result.AddSubQuery(JobSailingSchema.JX_JB, voyDestinationQuery, JoinCondition.Or);

			return result;
		}

		#endregion

		#region AddDateFilters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(Descriptions.ATA, ATAFilter).MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|ATA", "ATA");
			filters.AddDateFilter(Descriptions.ATD, ATDFilter).MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|ATD", "ATD");
			filters.AddDateFilter(Descriptions.BookedDate, JobShipmentSchema.JS_A_BKD).MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|BookedDate", "Booked Date");
			filters.AddDateFilter(Descriptions.ETA, ETAFilter).MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|ETA", "ETA");
			filters.AddDateFilter(Descriptions.ETD, ETDFilter).MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|ETD", "ETD");
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

		protected virtual void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter sailingStatusFilter = filters.AddTextFilter(Descriptions.SailingStatus, SailingStatusFilter, SailingStatus_List);
			sailingStatusFilter.Category = FilterCategories.StatusAndFlags;
			sailingStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|SailingStatus", "Sailing Status");

			ModuleTextFilter shipmentStatus = filters.AddTextFilter(Descriptions.ShipmentStatus, ShipmentStatusFilter, ShipmentStatus_List);
			shipmentStatus.Visibility = FilterVisibility.AlwaysApplied;
			shipmentStatus.DefaultProperty = DefaultShipmentStatusFilter;
			shipmentStatus.Category = FilterCategories.StatusAndFlags;
			shipmentStatus.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|ShipmentStatus", "Shipment Status");
		}

		ZQuery SailingStatusFilter(ZString code)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AgencyShipment));

			switch (code)
			{
				case SailingStatus.Current:
					ZDBOnlySubQuery subQuery1 = new ZDBOnlySubQuery(typeof(JobSailing), JobShipmentSchema.JS_JX);
					ZDBOnlySubQuery subSubQuery1 = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
					subSubQuery1.AddToFilter(JobVoyOriginSchema.JA_E_DEP, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Today);
					subQuery1.AddSubQuery(subSubQuery1, JoinCondition.And);
					query.AddSubQuery(subQuery1, JoinCondition.And);
					query.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_JX, SQLComparisonOperator.Equal, null);
					break;

				case SailingStatus.Departed:
					ZDBOnlySubQuery subQuery2 = new ZDBOnlySubQuery(typeof(JobSailing), JobShipmentSchema.JS_JX);
					ZDBOnlySubQuery subSubQuery2 = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
					subSubQuery2.AddToFilter(JobVoyOriginSchema.JA_E_DEP, SQLComparisonOperator.LessThan, ZDateTime.Now);
					subQuery2.AddSubQuery(subSubQuery2, JoinCondition.And);
					query.AddSubQuery(subQuery2, JoinCondition.And);
					break;
			}

			return query;
		}

		ZQuery ShipmentStatusFilter(ZString code)
		{
			switch (code)
			{
				case "ALL":
					return new ZQuery();
				case "UCF":
					return new ZQuery(JobShipmentSchema.JS_ShipmentStatus, ShipmentStatusHelperMethods.GetBookingStageStatus());
				case "BOL-All":
					return new ZQuery(JobShipmentSchema.JS_ShipmentStatus, ShipmentStatusHelperMethods.GetBillOfLadingStageStatus());

				default:
					return new ZQuery(JobShipmentSchema.JS_ShipmentStatus, code);
			}
		}

		#endregion

		#region AddModesAndTypesFilters

		void AddModesAndTypesFilters(ModuleFilterCollection filters)
		{
			ModuleFilter cargoTypeFilter = filters.AddTextFilter(Descriptions.CargoType, JobShipmentSchema.JS_PackingMode, CargoType_List);
			cargoTypeFilter.Category = FilterCategories.ModesAndTypes;
			cargoTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|CargoType", "Cargo Type");

			ModuleFilter commodityFilter = filters.AddNkFilter(Descriptions.Commodity, GetCommodityQuery, ModuleIDs.RefCommodityCode, CommodityCode_List);
			commodityFilter.Category = FilterCategories.ModesAndTypes;
			commodityFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|Commodity", "Commodity");

			ModuleNkFilter serviceLevelFilter = filters.AddNkFilter(Descriptions.ServiceLevel, JobShipmentSchema.JS_RS_NKServiceLevel, ModuleIDs.ServiceLevel, BindingLists.RefServiceLevel_List);
			serviceLevelFilter.Category = FilterCategories.ModesAndTypes;
			serviceLevelFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ServiceLevel", "Service Level");
		}

		ZQuery GetCommodityQuery(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CommonShipment));

			ZDBOnlySubQuery commodityPackLineSubQuery = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.JL_JS);
			commodityPackLineSubQuery.AddToFilter(JobPackLinesSchema.JL_RH_NKCommodityCode, value);

			ZDBOnlySubQuery commodityContainerSubQuery = new ZDBOnlySubQuery(typeof(CommonContainer), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
			commodityContainerSubQuery.AddToFilter(JobContainerSchema.JC_RH_NKContainerCommodityCode, value);

			query.AddSubQuery(commodityPackLineSubQuery, JoinCondition.And);
			query.AddSubQuery(commodityContainerSubQuery, JoinCondition.Or);

			return query;
		}

		#endregion

		#region AddAccountingFilters

		void AddAccountingFilters(ModuleFilterCollection filters)
		{
			AccountingFilterStrip.AddBillingFilters(filters);
			AccountingFilterStrip.AddJobManagementFilters(filters, JobInvoicingSecurity);

			filters.AddCustomFilter(new ChargeModuleFilter(Descriptions.LocalSellAmount, FromJobHeaderFilter) { Category = (FilterCategory)AccountingFilterStrip.BillingFilterCategory, MultilingualDescription = ResString.GetMultilingualString("2C76DDDE-C26B-4050-9284-DDCA31B12229", "Local Sell Amount") });
			var filter = filters.AddTextFilter(Descriptions.ChargeDescription, ChargeDescriptionFilter);
			filter.SubGroup = JobHeaderFilterProcessor;
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			filter.MultilingualDescription = ResString.GetMultilingualString("3951270B-9857-4220-9A27-21E599FCD6F7", "Charge Description");
			filter.Category = (FilterCategory)AccountingFilterStrip.BillingFilterCategory;
		}

		ZQuery ChargeDescriptionFilter(SQLComparisonOperator opp, ZString text)
		{
			bool isNotQuery = opp.IsNegativeSQLOperator();
			if (isNotQuery)
			{
				opp = opp.GetNegatingSQLOperatorIfNotInSubquery();
			}

			return FromChargeFilter(new ZQuery(JobChargeSchema.JR_Desc, opp, text), isNotQuery);
		}

		#endregion

		#region IAccountingFilterStripHolder Members

		internal IAccountingFilterStrip AccountingFilterStrip
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

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new Dictionary<string, object>()
		{
			{ AccountingFilterStripConfigurationKeys.BusinessObjectType, typeof(AgencyShipment) },
			{ AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride, ResString.GetMultilingualString("Shipping|AgencyShipmentFilter|InvoiceStatus", "Invoice Status") }
		};

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(AgencyShipment));
			result.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return result;
		}

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

		#region Implementation

		#region Validation

		void PrincipalFilterValidation(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				info.AddError(Res.GetString("9bfd1cb9-b161-4c69-9c45-9941b9d5ceaa", "Please select a principal to filter by"));
			}
		}

		void BranchFilterValidation(ZPropertyInfo info)
		{
			String errorMessage = Res.GetString("fa4a9e0d-028e-482f-b68e-338787655751", "Your current security rights only allow you to view shipments relating to your current login branch.\r\nIf you think this is incorrect, please contact your system administrator.");
			if ((ZGuid)info.Value != GlbBranch.CurrentBranch.PK)
			{
				info.AddError(errorMessage);
			}
		}

		#endregion

		ZQuery DateFilter(DateComparisonOperator comparisonOperator, SailingFilterBuilder.Dates dates, ZDateTime fromDate, ZDateTime toDate)
		{
			SailingFilterBuilder builder = new SailingFilterBuilder(Factory);
			builder.SetDateRange(dates, comparisonOperator, fromDate, toDate);
			return builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.Direct);
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

				ZDBOnlyQuery jobShipmentQuery = new ZDBOnlyQuery(typeof(AgencyShipment));
				jobShipmentQuery.AddSubQuery(JobShipmentSchema.PK, docAddressSubQuery, JoinCondition.And);

				result = jobShipmentQuery;
			}
			else
			{
				result = new ZQuery();
			}

			return result;
		}

		ZQuery FromDocAddressFilter(ZQuery docAddressFilter, bool notIn)
		{
			ZDBOnlySubQuery docAddress = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn);
			docAddress.AddToFilter(docAddressFilter);

			ZDBOnlyQuery agencyShipment = new ZDBOnlyQuery(typeof(AgencyShipment));
			agencyShipment.AddSubQuery(docAddress, JoinCondition.And);

			return agencyShipment;
		}

		ZQuery FromJobHeaderFilter(ZQuery jobHeaderFilter, bool notIn)
		{
			ZDBOnlySubQuery jobHeader = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID, notIn);
			jobHeader.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			jobHeader.AddToFilter(jobHeaderFilter);

			ZDBOnlyQuery shipment = new ZDBOnlyQuery(typeof(AgencyShipment));
			shipment.AddSubQuery(jobHeader, JoinCondition.And);

			return shipment;
		}

		ZQuery FromChargeFilter(ZQuery chargeFilter, bool isNotQuery)
		{
			var chargeQuery = new ZDBOnlySubQuery(typeof(JobCharge), JobChargeSchema.JR_JH, isNotQuery);
			chargeQuery.AddToFilter(chargeFilter);

			var headerQuery = new ZDBOnlyQuery(typeof(JobHeader));
			headerQuery.AddSubQuery(chargeQuery, JoinCondition.And);

			if (isNotQuery)
			{
				var hasChargesQuery = new ZDBOnlyQuery(typeof(JobHeader));
				var allChargesQuery = new ZDBOnlySubQuery(typeof(JobCharge), JobChargeSchema.JR_JH);
				hasChargesQuery.AddSubQuery(allChargesQuery, JoinCondition.And);
				headerQuery.AddToFilter(hasChargesQuery, JoinCondition.And);
			}

			return headerQuery;
		}

		ZQuery GetCompanyNameQuery(SQLComparisonOperator comparison, ZString text, ZString addressType)
		{
			bool notIn = SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref comparison);

			ZQuery docAddressOverridenFilter = new ZQuery(JobDocAddressSchema.E2_AddressType, addressType);
			docAddressOverridenFilter.AddToFilter(JobDocAddressSchema.E2_AddressOverride, ZBool.True);
			docAddressOverridenFilter.AddToFilter(JobDocAddressSchema.E2_CompanyName, comparison, text);

			ZQuery orgHeaderFilter = new ZQuery(OrgHeaderSchema.OH_FullName, comparison, text);

			ZQuery docAddressFilter = OrgRelatedPartiesFilterHelper.GetDocAddressFromOrgHeaderFilter(orgHeaderFilter, new ZQuery(), false, false);
			docAddressFilter.AddToFilter(JobDocAddressSchema.E2_AddressType, addressType);
			docAddressFilter.AddToFilter(JobDocAddressSchema.E2_AddressOverride, ZBool.False);

			ZQuery result = new ZQuery();
			result.AddToFilter(FromDocAddressFilter(docAddressOverridenFilter, notIn));
			result.AddToFilter(FromDocAddressFilter(docAddressFilter, notIn), notIn ? JoinCondition.And : JoinCondition.Or);

			return result;
		}

		#region Lists

		#region BindingLists

		BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#endregion

		#region BranchList

		GlbBranchCollection BranchList
		{
			get
			{
				if (branchList == null)
				{
					branchList = new GlbBranchCollection(Factory);
				}

				return branchList;
			}
		}
		GlbBranchCollection branchList;

		#endregion

		#region CargoType_List

		AgencyCargoTypeCodeDescriptionPairList CargoType_List
		{
			get { return Factory.GetCachedValue<AgencyCargoTypeCodeDescriptionPairList>(); }
		}

		#endregion

		#region Carrier_List

		TransportShippingProviderCollection Carrier_List
		{
			get
			{
				if (carrier_List == null)
				{
					carrier_List = new TransportShippingProviderCollection(Factory);
				}

				return carrier_List;
			}
		}
		TransportShippingProviderCollection carrier_List;

		#endregion

		#region Consignee_List

		OrganisationsFindBoxCollection Consignee_List
		{
			get { return new ConsigneeOrForwarderCollection(Factory); }
		}

		#endregion

		#region Consignor_List

		OrganisationsFindBoxCollection Consignor_List
		{
			get { return new ConsignorOrForwarderCollection(Factory); }
		}

		#endregion

		#region Principal_List

		ShipsAgencyPrincipalCollectionWithSecurityCheck Principal_List
		{
			get
			{
				if (principal_List == null)
				{
					principal_List = new ShipsAgencyPrincipalCollectionWithSecurityCheck(Factory);
				}

				return principal_List;
			}
		}

		ShipsAgencyPrincipalCollectionWithSecurityCheck principal_List;

		#endregion

		#region OrgHeader_List

		OrgHeaderCollection OrgHeader_List
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		#endregion

		#region SailingStatus_List

		public CodeDescriptionPairList SailingStatus_List
		{
			get
			{
				if (sailingStatus_List == null)
				{
					sailingStatus_List = new CodeDescriptionPairList();
					sailingStatus_List.AddPair(SailingStatus.Current, Res.GetString("51c1ba86-a48c-446c-9936-4af7dcc2e035", "Current"));
					sailingStatus_List.AddPair(SailingStatus.Departed, Res.GetString("8a37ae6a-442c-4728-9930-a1f45ad1845d", "Departed"));
				}

				return sailingStatus_List;
			}
		}

		public static class SailingStatus
		{
			public const string Current = "CURRENT";
			public const string Departed = "DEPARTED";
		}

		CodeDescriptionPairList sailingStatus_List;

		#endregion

		#region ShipmentStatus_List

		public CodeDescriptionPairList ShipmentStatus_List
		{
			get
			{
				if (shipmentStatus_List == null)
				{
					shipmentStatus_List = NewShipmentStatusList();
				}
				return shipmentStatus_List;
			}
		}

		CodeDescriptionPairList shipmentStatus_List;

		#endregion

		#region UNLOCO_List

		LocationCollection UNLOCO_List
		{
			get
			{
				if (unloco_List == null)
				{
					unloco_List = new LocationCollection(Factory);
				}

				return unloco_List;
			}
		}
		LocationCollection unloco_List;

		#endregion

		#region Vessel_List

		RefVesselCollection Vessel_List
		{
			get
			{
				if (vessel_List == null)
				{
					vessel_List = new RefVesselCollection(Factory);
				}

				return vessel_List;
			}
		}

		RefVesselCollection vessel_List;

		#endregion

		#region CommodityCode_List

		public RefCommodityCodeCollection CommodityCode_List
		{
			get { return commodityCode_List ?? (commodityCode_List = new RefCommodityCodeCollection(Factory)); }
		}
		RefCommodityCodeCollection commodityCode_List;

		#endregion

		#region TradeLaneList

		JobTradeLaneCollection TradeLaneList
		{
			get { return tradeLaneList ?? (tradeLaneList = new JobTradeLaneCollection(Factory)); }
		}
		JobTradeLaneCollection tradeLaneList;

		#endregion

		#endregion

		#endregion

		protected abstract CodeDescriptionPairList NewShipmentStatusList();
		protected abstract ZString DefaultShipmentStatusFilter { get; }
		protected abstract ZBool AllowSearchOfUnlocoOutsideLoginBranch { get; }
		protected abstract SecurityCheckpoint JobInvoicingSecurity { get; }
	}
}



