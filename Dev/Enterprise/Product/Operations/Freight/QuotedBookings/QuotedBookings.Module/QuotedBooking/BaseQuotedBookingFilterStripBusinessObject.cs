using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.Module;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Module
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class BaseQuotedBookingFilterStripBusinessObject : FilterStripBusinessObject, IAccountingFilterStripHolder
	{
		#region SuppressResourceStringsCheckRegion

		public static class Descriptions
		{
			public static class WebQuote
			{
				public const string QuoteNumber = "Quote #";
				public const string Status = "Status";
				public const string QuotationDate = "Quotation Date";
				public const string ExpiryDate = "Expiry Date";
				public const string AcceptanceDate = "Acceptance Date";
			}

			public static class NumbersAndReferences
			{
				public const string AdditionalReferenceNumber = "Additional Reference #";
				public const string BookingNumber = "Booking #";
				public const string BookingRefNumber = "Booking Ref #";
				public const string CfsRefNumber = "CFS Ref #";
				public const string ClientContractNumber = "Client Contract #";
				public const string ContainerNumber = "Container #";
				public const string DirectMawbNumber = "Direct MAWB #";
				public const string FlightVoyageNumber = "Flight/Voyage # and Vessel";
				public const string ShippersRefNumber = "Shippers Ref #";
				public const string HouseBillNumber = "House Bill #";
				public const string QuoteNumber = "Quote #";
				public const string CO2e = "CO2e (kg)";
				public const string CompanyTariffLevelOverride = "Company Tariff Level Override";
				public const string CarrierContractNumber = "Carrier Contract #";
				public const string AllocationID = "Allocation ID";
				public const string FMCTariffID = "FMC Tariff ID";
				public const string CommodityCode = "Commodity Code";
			}

			public static class StatusAndFlags
			{
				public const string Status = "Status";
				public const string Used = "Used";
				public const string ActiveStatus = "Active Status";
				public const string ConsolidatedOrConverted = "Consolidated/Converted";
				public const string IsHazardous = "Is Hazardous";
				public const string QuoteAndOrBooking = "Quote And/Or Booking";
				public const string ShipmentStatus = "Shipment Status";
				public const string OneOffQuoteApprovalStatus = "One Off Quote Approval Status";
				public const string OneOffQuoteKPI = "One Off Quote KPI";
				public const string OneOffQuoteSource = "One Off Quote Source";
				public const string OneOffQuoteRevisionReason = "One Off Quote Revision Reason";
				public const string FinalPrint = "Final Print";
			}

			public static class Dates
			{
				public const string AdditionalRefNumIssueDate = "Additional Ref Num Issue Date";
				public const string BookingDate = "Booking Date";
				public const string ClientReqETA = "Client Req. ETA";
				public const string ClientAcceptedDate = "Client Accepted Date";
				public const string EstimatedPickup = "Estimated Pickup";
				public const string PickupRequiredBy = "Pickup Required By";
				public const string EstimatedDeliveryDate = "Estimated Delivery Date";
				public const string DeliveryRequiredBy = "Delivery Required By";
				public const string ETD = "ETD";
				public const string ETA = "ETA";
				public const string DeliveryDueDate = "Delivery Due Date";
				public const string StartDate = "Start Date";
				public const string EndDate = "End Date";
			}

			public static class Locations
			{
				public const string LoadDischarge = "Load / Discharge";
				public const string OriginDestination = "Origin / Destination";
			}

			public static class OrganisationsStaff
			{
				public const string Carrier = "Carrier";
				public const string Client = "Client";
				public const string ClientName = "Client Name";
				public const string BookingPartyName = "Booking Party Name";
				public const string BookingParty = "Booking Party";
				public const string SalesRepresentative = "Sales Representative";
				public const string Consignee = "Consignee";
				public const string Consignor = "Consignor";
				public const string ConsignorConsignee = "Consignor / Consignee";
				public const string PickupDelivery = "Pickup / Delivery";
				public const string ClientRelatedParties = "Client Related Parties";
				public const string ConsignorRelatedParties = "Consignor Related Parties";
				public const string ConsigneeRelatedParties = "Consignee Related Parties";
				public const string DeliveryAgent = "Delivery Agent";
				public const string PickupAgent = "Pickup Agent";
				public const string ControllingCustomer = "Controlling Customer";
				public const string ControllingAgent = "Controlling Agent";
				public const string Creditor = "Creditor";
				public const string ImportBroker = "Import Broker";
				public const string ExportBroker = "Export Broker";
				public const string PickupTransport = "Pickup Transport";
				public const string CFS = "CFS";
				public const string Branch = "Branch";
				public const string OperationsRepresentative = "Operations Representative";
				public const string ConsignorContact = "Consignor Contact";
				public const string ConsigneeContact = "Consignee Contact";
				public const string ClientContact = "Client Contact";
			}

			public static class AuditInformation
			{
				public const string CreatingUser = "Creating User";
				public const string LastEditUser = "Last Edit User";
				public const string CreatedTime = "Created Time";
				public const string LastEditTime = "Last Edit Time";
				public const string CreatedOnWebInternal = "Created On Web/Internal";
			}

			public static class ModesAndTypes
			{
				public const string ServiceLevel = "Service Level";
				public const string Mode = "Mode";
				public const string DGClassDGSubstance = "DG Class / DG Substance";
				public const string TransportMode = "Transport Mode";
				public const string ContainerMode = "Container Mode";
				public const string HBLDeliveryMode = "HBL Delivery Mode";
			}

			public static class PotentialCarriers
			{
				public const string PotentialCarrier = "Potential Carrier";
				public const string PotentialCreditor = "Potential Creditor";
			}
		}

		#endregion

		#region Overrides

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			return new ModuleFilterCollection();
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var helper = new WorkflowFilterStripsHelperWithRoutingSupport(typeof(ViewQuotedBooking), WorkflowDescriptors.QuotedBookingWorkflowDescriptorCode, Factory);
			helper.SetShouldAddWorkflowCustomFieldsFilters(true);
			helpers.Add(helper);

			return helpers;
		}

		#endregion

		#region Numbers And References

		protected virtual void AddNumbersAndReferencesFilters(ModuleFilterCollection filters)
		{
			AddCompanyTariffLevelOverrideFilter(filters);
			AddFMCTariffIDFilter(filters);
			AddCommodityCodeFilter(filters);
		}

		protected virtual void AddFMCTariffIDFilter(ModuleFilterCollection filters)
		{ }

		protected virtual void AddCommodityCodeFilter(ModuleFilterCollection filters)
		{ }

		protected void AddCompanyTariffLevelOverrideFilter(ModuleFilterCollection filters)
		{
			var companyTariffLevelOverrideFilter = filters.AddTextFilter(Descriptions.NumbersAndReferences.CompanyTariffLevelOverride, GetCompanyTariffLevelOverride, CompanyTariffLevelOverrideList);
			companyTariffLevelOverrideFilter.Category = FilterCategories.NumbersAndReferences;
			companyTariffLevelOverrideFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|CompanyTariffLevelOverride", "Company Tariff Level Override");
		}
		protected virtual ZQuery GetCompanyTariffLevelOverride(ZString value)
		{
			return new ZQuery();
		}

		public CodeDescriptionPairList CompanyTariffLevelOverrideList
		{
			get
			{
				if (companyTariffLevelList == null)
				{
					companyTariffLevelList = new CompanyTariffLevelList(Factory);
				}
				return companyTariffLevelList.CompanyTariffLevelOverrideList;
			}
		}

		CompanyTariffLevelList companyTariffLevelList;

		#region GetQuoteNoQuery

		protected virtual ZQuery GetQuoteNoQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery result = new ZQuery();
			if (!value.IsEmpty)
			{
				result.AddToFilter_PossiblyCommaSeparated(ViewQuotedBookingSchema.VB_QuoteNumber, comparisonOperator, value);
			}

			return result;
		}

		#endregion

		#endregion

		#region Status And Flags

		protected virtual void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter activeStatusFilter = filters.AddTextFilter(Descriptions.StatusAndFlags.ActiveStatus, GetActiveStatusQuery, ActiveStatusList);
			activeStatusFilter.Category = FilterCategories.StatusAndFlags;
			activeStatusFilter.DefaultProperty = OrgConstants.FilterControl.ActiveStatus.Code.ActiveClients;
			activeStatusFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|ActiveStatus", "Active Status");
			activeStatusFilter.Visibility = FilterVisibility.AlwaysApplied;
		}

		#region GetActiveStatusQuery

		ZQuery GetActiveStatusQuery(ZString value)
		{
			var result = new ZQuery();
			if (!value.IsEmpty && value != OrgConstants.FilterControl.ActiveStatus.Code.AllClients)
			{
				result.IgnoreActiveFilter = true;
				bool isCanceled = value == OrgConstants.FilterControl.ActiveStatus.Code.InactiveClients;

				result.AddToFilter(ViewQuotedBookingSchema.VB_IsCanceled, SQLComparisonOperator.Equal, isCanceled);
			}

			return result;
		}

		#endregion

		#endregion

		#region Dates

		protected virtual void AddDatesFilters(ModuleFilterCollection filters) { }

		#endregion

		#region Locations

		protected virtual void AddLocationsFilters(ModuleFilterCollection filters)
		{
			AddOriginDestinationFilter(filters);
		}

		protected void AddLoadDischargeFilter(ModuleFilterCollection filters)
		{
			ModuleLocationFilter loadDischargeFilter = filters.AddLocationFilter(Descriptions.Locations.LoadDischarge, GetLoadDischargeQuery, BindingLists.RefLocation_List, BindingLists.RefLocation_List);
			loadDischargeFilter.Category = FilterCategories.Locations;
			loadDischargeFilter.SetItemDescriptions(Res.GetData("2627152e-2f76-4652-a3e0-69170d12bfa3", "Load"), Res.GetData("5927e446-a581-450f-a862-cfdd747167e9", "Discharge"));
			loadDischargeFilter.Property1Validation = info => PortFilterSecurityValidator.ValidatePort(info, ((ModuleLocationFilter)info.BizObj).Property2Info, filters, Descriptions.Locations.LoadDischarge, Descriptions.Locations.OriginDestination);
			loadDischargeFilter.Property2Validation = info => PortFilterSecurityValidator.ValidatePort(info, ((ModuleLocationFilter)info.BizObj).Property1Info, filters, Descriptions.Locations.LoadDischarge, Descriptions.Locations.OriginDestination);
			loadDischargeFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|LoadDischarge", "Load / Discharge");
		}

		protected void AddOriginDestinationFilter(ModuleFilterCollection filters)
		{
			ModuleLocationFilter originDestFilter = filters.AddLocationFilter(Descriptions.Locations.OriginDestination, GetOriginDestinationQuery, BindingLists.RefLocation_List, BindingLists.RefLocation_List);
			originDestFilter.GetXQuery = GetOriginDestinationXQuery;
			originDestFilter.Category = FilterCategories.Locations;
			originDestFilter.SetItemDescriptions(Res.GetData("bf2c161a-2157-4a4c-a446-0809ee07bd70", "Origin"), Res.GetData("47a709f4-27ce-4125-8a7b-f2c058947afc", "Destination"));
			originDestFilter.Property1Validation = info => PortFilterSecurityValidator.ValidatePort(info, ((ModuleLocationFilter)info.BizObj).Property2Info, filters, Descriptions.Locations.LoadDischarge, Descriptions.Locations.OriginDestination);
			originDestFilter.Property2Validation = info => PortFilterSecurityValidator.ValidatePort(info, ((ModuleLocationFilter)info.BizObj).Property1Info, filters, Descriptions.Locations.LoadDischarge, Descriptions.Locations.OriginDestination);
			originDestFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|OriginDestination", "Origin / Destination");
		}

		ZQuery GetOriginDestinationXQuery(ModuleFilter moduleFilter)
		{
			var filter = moduleFilter as ModuleLocationFilter;
			var result = new ZQuery();

			if (!filter.Property1.IsEmpty)
			{
				result.AddToFilter(XQueryFilterHelper.GenerateXQuery((column) => new ZQuery(column, SQLComparisonOperator.StartsWith, filter.Property1), new XQueryFilterInfo(ShipmentXQueryPaths.PortOfOrigin, JobShipmentSchema.JS_RL_NKOrigin.MaxLength)));
			}

			if (!filter.Property2.IsEmpty)
			{
				result.AddToFilter(XQueryFilterHelper.GenerateXQuery((column) => new ZQuery(column, SQLComparisonOperator.StartsWith, filter.Property2), new XQueryFilterInfo(ShipmentXQueryPaths.PortOfDestination, JobShipmentSchema.JS_RL_NKDestination.MaxLength)));
			}

			return result;
		}

		PortFilterSecurityValidator PortFilterSecurityValidator
		{
			get { return fPortFilterSecurityValidator ?? (fPortFilterSecurityValidator = new PortFilterSecurityValidator(Factory, Env.Security.MaintainShipmentAllowSearchOfUnlocoOutsideLoginBranches)); }
		}

		PortFilterSecurityValidator fPortFilterSecurityValidator;

		#region GetLoadDischargeQuery

		ZQuery GetLoadDischargeQuery(ZString loadPortNK, ZString dischargePortPK)
		{
			ZQuery result = new ZQuery();

			if (!loadPortNK.IsEmpty || !dischargePortPK.IsEmpty)
			{
				var shipmentFilter = new ZDBOnlyQuery(typeof(CommonShipment));
				if (!loadPortNK.IsEmpty)
				{
					shipmentFilter.AddToFilter(JobShipmentSchema.JS_RL_NKLoadPort, SQLComparisonOperator.Equal, loadPortNK);
				}

				if (!dischargePortPK.IsEmpty)
				{
					shipmentFilter.AddToFilter(JobShipmentSchema.JS_RL_NKDischargePort, SQLComparisonOperator.Equal, dischargePortPK);
				}

				result.AddToFilter(GetWithBookingQuery(shipmentFilter));
			}

			return result;
		}

		#endregion

		#region GetOriginDestinationQuery

		protected virtual ZQuery GetOriginDestinationQuery(ZString originNK, ZString destinationNK)
		{
			return new ZQuery();
		}

		#endregion

		#endregion

		#region Organisations Staff

		protected virtual void AddOrganisationsStaffFilters(ModuleFilterCollection filters)
		{
			var clientNameFilter = filters.AddTextFilter(Descriptions.OrganisationsStaff.ClientName, GetClientNameQuery)
				.WithMaxLengthOf<ModuleTextFilter>(OrgHeaderSchema.OH_FullName);
			clientNameFilter.Category = FilterCategories.Organisations;
			clientNameFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|ClientName", "Client Name");

			var staffFilter = GetSalesRepFilter();
			filters.AddFilter(staffFilter);
			staffFilter.IsPublishedOnWeb = false;
			staffFilter.Category = FilterCategories.Organisations;
			staffFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|SalesRepresentative", "Sales Representative");

			var clientRelatedPartiesFilter = new OrgRelatedPartiesModuleFilter(Descriptions.OrganisationsStaff.ClientRelatedParties, GetClientRelatedPartiesQuery);
			clientRelatedPartiesFilter.Category = FilterCategories.Organisations;
			clientRelatedPartiesFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|ClientRelatedParties", "Client Related Parties");
			filters.AddCustomFilter(clientRelatedPartiesFilter);

			var consignorTerminology = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value.IsEmpty ? (ZString)FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue : ((ZString)FreightDataRegistry.Instance.ConsignorShipperTerminology.Value).SubstringSafe(0, 15);
			var consignorRelatedPartiesFilter = new OrgRelatedPartiesModuleFilter(Descriptions.OrganisationsStaff.ConsignorRelatedParties, GetConsignorRelatedPartiesQuery);
			consignorRelatedPartiesFilter.Category = FilterCategories.Organisations;
			consignorRelatedPartiesFilter.MultilingualDescription = ResString.GetMultilingualString("E0BCCB64-B2A8-48C4-A09F-CCE8F6522D15", "{0} Related Parties", consignorTerminology);
			filters.AddCustomFilter(consignorRelatedPartiesFilter);

			var consigneeRelatedPartiesFilter = new OrgRelatedPartiesModuleFilter(Descriptions.OrganisationsStaff.ConsigneeRelatedParties, GetConsigneeRelatedPartiesQuery);
			consigneeRelatedPartiesFilter.Category = FilterCategories.Organisations;
			consigneeRelatedPartiesFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|ConsigneeRelatedParties", "Consignee Related Parties");
			filters.AddCustomFilter(consigneeRelatedPartiesFilter);
		}

		#region GetSalesRepFilter

		protected virtual ModuleFilter GetSalesRepFilter()
		{
			return new ModuleNkFilter(Descriptions.OrganisationsStaff.SalesRepresentative, GetSalesRepQuery, ModuleIDs.GlbStaff, new GlbStaffCollection(Factory));
		}

		ZQuery GetSalesRepQuery(ZString salesRepNK)
		{
			ZDBOnlyQuery salesRepQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			ZDBOnlySubQuery jobSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			ZDBOnlySubQuery quoteSubQuery = new ZDBOnlySubQuery(typeof(Quote), RatingHeaderSchema.PK);

			jobSubQuery.AddToFilter(JobHeaderSchema.JH_GS_NKRepSales, salesRepNK);
			jobSubQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			quoteSubQuery.AddSubQuery(jobSubQuery, JoinCondition.And);

			salesRepQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteSubQuery, JoinCondition.And);
			salesRepQuery.AddSubQuery(ViewQuotedBookingSchema.VB_JS, jobSubQuery, JoinCondition.Or);

			return salesRepQuery;
		}

		#endregion

		#region GetClientQuery

		protected virtual ZQuery GetClientNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			return new ZQuery();
		}
		#endregion

		#region GetRelatedPartiesQuery

		protected virtual ZQuery GetClientRelatedPartiesQuery(ZQuery orgHeaderFilter)
		{
			return new ZQuery();
		}

		protected virtual ZQuery GetConsignorRelatedPartiesQuery(ZQuery orgHeaderFilter)
		{
			return new ZQuery();
		}

		protected virtual ZQuery GetConsigneeRelatedPartiesQuery(ZQuery orgHeaderFilter)
		{
			return new ZQuery();
		}

		#endregion

		#endregion

		#region Modes And Types

		protected virtual void AddModesAndTypesFilters(ModuleFilterCollection filters)
		{
			AddModeFilter(filters);
			AddTransportModeFilter(filters);
			AddContainerModeFilter(filters);

			var serviceLevelFilter = filters.AddNkFilter(Descriptions.ModesAndTypes.ServiceLevel, GetServiceLevelQuery, ModuleIDs.ServiceLevel, BindingLists.RefServiceLevel_List);
			serviceLevelFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.ServiceLevel, JobShipmentSchema.JS_RS_NKServiceLevel.MaxLength);
			serviceLevelFilter.Category = FilterCategories.ModesAndTypes;
			serviceLevelFilter.IsPublishedOnWeb = false;
			serviceLevelFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|ServiceLevel", "Service Level");

			var dgSubstanceDGClassFilter = new DGClassDGSubstanceFilter(Descriptions.ModesAndTypes.DGClassDGSubstance, GetDGClassDGSubstanceQuery);
			dgSubstanceDGClassFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|DGClassDGSubstance", "DG Class / DG Substance");
			dgSubstanceDGClassFilter.Category = FilterCategories.ModesAndTypes;
			filters.AddCustomFilter(dgSubstanceDGClassFilter);
		}

		protected void AddModeFilter(ModuleFilterCollection filters)
		{
			var modeFilter = filters.AddTextFilter(Descriptions.ModesAndTypes.Mode, GetModeQuery, QuotedBooking.GetOldModes(false));
			modeFilter.GetXQuery = GetModeXQuery;
			modeFilter.Category = FilterCategories.ModesAndTypes;
			modeFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|Mode", "Mode");
		}

		protected void AddTransportModeFilter(ModuleFilterCollection filters)
		{
			var transportModeFilter = filters.AddTextFilter(Descriptions.ModesAndTypes.TransportMode, GetTransportModeQuery, QuotedBooking.GetNewTransportModes());
			transportModeFilter.GetXQuery = GetTransportModeXQuery;
			transportModeFilter.Category = FilterCategories.ModesAndTypes;
			transportModeFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|TransportMode", "Transport Mode");
		}

		protected void AddContainerModeFilter(ModuleFilterCollection filters)
		{
			var containerModeFilter = filters.AddTextFilter(Descriptions.ModesAndTypes.ContainerMode, GetContainerModeQuery, QuotedBooking.GetContainerModes(string.Empty, IsBookingOnly));
			containerModeFilter.GetXQuery = GetContainerModeXQuery;
			containerModeFilter.Category = FilterCategories.ModesAndTypes;
			containerModeFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|ContainerMode", "Container Mode");
		}

		#region GetModeQuery

		protected virtual ZQuery GetModeQuery(ZString value)
		{
			return new ZQuery();
		}

		protected virtual ZQuery GetModeXQuery(ModuleFilter moduleFilter)
		{
			return new ZQuery();
		}

		protected virtual ZQuery GetTransportModeQuery(ZString value) => new ZQuery();

		protected virtual ZQuery GetTransportModeXQuery(ModuleFilter moduleFilter) => new ZQuery();

		protected virtual ZQuery GetContainerModeQuery(ZString value) => new ZQuery();

		protected virtual ZQuery GetContainerModeXQuery(ModuleFilter moduleFilter) => new ZQuery();

		protected virtual bool IsBookingOnly => true;

		#endregion

		#region GetServiceLevelQuery

		protected virtual ZQuery GetServiceLevelQuery(ZString value)
		{
			return new ZQuery();
		}

		#endregion

		#region GetDGClassDGSubstanceQuery

		ZQuery GetDGClassDGSubstanceQuery(SQLComparisonOperator dgOperator, ZString dgClass, ZString dgSubstance)
		{
			var notInOperations = new SQLComparisonOperator[] { SQLComparisonOperator.NotContains, SQLComparisonOperator.DoesNotStartWith, SQLComparisonOperator.NotEqual, SQLComparisonOperator.IsBlank };
			var isBlankOperation = dgOperator == SQLComparisonOperator.IsBlank || dgOperator == SQLComparisonOperator.IsNotBlank;

			var result = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			result.IgnoreActiveFilter = true;

			var bookingSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			var packLineQuery = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.JL_JS, notInOperations.Contains(dgOperator));
			var undgQuery = new ZDBOnlySubQuery(typeof(UNDGDataItem), UNDGDataItemSchema.DI_ParentID);
			var dgOperatorIfNegated = notInOperations.Contains(dgOperator) ? dgOperator.GetNegatingSQLOperatorIfNotInSubquery() : dgOperator;

			if (!isBlankOperation)
			{
				if (!dgClass.IsEmpty)
				{
					undgQuery.AddToFilter(UNDGDataItemSchema.DI_IMOClass, dgOperatorIfNegated, dgClass);
				}

				if (!dgSubstance.IsEmpty)
				{
					var substanceQuery = new ZDBOnlySubQuery(typeof(UNDGSubstancePivot), UNDGSubstancePivotSchema.PK);

					var unno = dgSubstance;

					if (dgSubstance.Length >= 5)
					{
						substanceQuery.AddToFilter(UNDGSubstancePivotSchema.DP_Variant, dgSubstance.Substring(4));
						unno = dgSubstance.Substring(0, 4);
					}

					substanceQuery.AddToFilter(UNDGSubstancePivotSchema.DP_UNNO, unno);
					undgQuery.AddSubQuery(UNDGDataItemSchema.PK, UNDGSubstancePivotSchema.DP_ParentId, substanceQuery, JoinCondition.And);
				}
			}

			packLineQuery.AddSubQuery(undgQuery, JoinCondition.And);
			bookingSubQuery.AddSubQuery(packLineQuery, JoinCondition.And);
			result.AddSubQuery(ViewQuotedBookingSchema.VB_JS, bookingSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#endregion

		#region Implementation

		#region GetOrgDocAddressQuery

		protected ZDBOnlySubQuery GetOrgDocAddressQuery(DocAddressType addressType, ZString tablePrefix, ZGuid value)
		{
			ZDBOnlySubQuery docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);

			orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, value);
			docAddressSubQuery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressSubQuery, JoinCondition.And);
			docAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, addressType));
			docAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, tablePrefix);

			return docAddressSubQuery;
		}

		#endregion

		#region GetWithBookingQuery

		static protected ZDBOnlyQuery GetWithBookingQuery(SchemaColumn bookingColumn, SQLComparisonOperator comparisonOperator, object value, bool disableCommaSeparation = false)
		{
			return disableCommaSeparation
				? GetWithBookingQuery(new ZQuery().AddToFilter(bookingColumn, comparisonOperator, value))
				: GetWithBookingQuery(new ZQuery().AddToFilter_PossiblyCommaSeparated(bookingColumn, comparisonOperator, value));
		}

		static protected ZDBOnlyQuery GetWithBookingQuery(ZQuery bookingQuery, bool notIn = false)
		{
			var viewFilter = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			viewFilter.IgnoreActiveFilter = true;

			var bookingSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK, notIn);
			bookingSubQuery.AddToFilter(bookingQuery);

			viewFilter.AddSubQuery(ViewQuotedBookingSchema.VB_JS, bookingSubQuery, JoinCondition.And);

			return viewFilter;
		}

		#endregion

		#region GetWithQuoteHeaderQuery

		protected ZDBOnlyQuery GetWithQuoteHeaderQuery(SchemaColumn quoteHeaderColumn, SQLComparisonOperator comparisonOperator, object value, bool disableCommaSeparation = false)
		{
			var quoteSubQuery = new ZDBOnlySubQuery(typeof(Quote), RatingHeaderSchema.PK);
			if (disableCommaSeparation)
			{
				quoteSubQuery.AddToFilter(quoteHeaderColumn, comparisonOperator, value);
			}
			else
			{
				quoteSubQuery.AddToFilter_PossiblyCommaSeparated(quoteHeaderColumn, comparisonOperator, value);
			}

			var viewFilter = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			viewFilter.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteSubQuery, JoinCondition.And);

			return viewFilter;
		}

		#endregion

		protected ZDBOnlyQuery GetWithJobHeaderQuery(SchemaColumn jobHeaderColumn, SchemaColumn quotedBookingColumn, SQLComparisonOperator comparisonOperator, object value)
		{
			var viewFilter = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			var headerSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			headerSubQuery.AddToFilter(jobHeaderColumn, comparisonOperator, value);

			viewFilter.AddSubQuery(quotedBookingColumn, headerSubQuery, JoinCondition.And);
			return viewFilter;
		}

		#region GetWithOneOffQuery

		protected ZDBOnlyQuery GetWithOneOffQuery(SchemaColumn oneOffColumn, SQLComparisonOperator comparisonOperator, object value)
		{
			var viewFilter = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			var quoteSubQuery = new ZDBOnlySubQuery(typeof(Quote), RatingHeaderSchema.PK);
			var oneOffSubQuery = new ZDBOnlySubQuery(typeof(RateOneOffShipment), RateOneOffShipmentSchema.TT_TH);

			oneOffSubQuery.AddToFilter(oneOffColumn, comparisonOperator, value);

			quoteSubQuery.AddSubQuery(oneOffSubQuery, JoinCondition.And);
			viewFilter.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteSubQuery, JoinCondition.And);
			return viewFilter;
		}

		#endregion

		protected ZQuery GetBookingFromDocAddressFilter(ZQuery docAddressFilter)
		{
			ZDBOnlySubQuery docAddress = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			docAddress.AddToFilter(docAddressFilter);

			ZDBOnlySubQuery resultSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			resultSubQuery.AddSubQuery(docAddress, JoinCondition.And);

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			result.AddSubQuery(ViewQuotedBookingSchema.VB_JS, resultSubQuery, JoinCondition.And);

			return result;
		}

		protected ZQuery GetQuoteFromDocAddressFilter(ZQuery docAddressFilter)
		{
			ZDBOnlySubQuery docAddress = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			docAddress.AddToFilter(docAddressFilter);

			ZDBOnlySubQuery quoteSubQuery = new ZDBOnlySubQuery(typeof(Quote), RatingHeaderSchema.PK);
			quoteSubQuery.AddSubQuery(docAddress, JoinCondition.And);

			ZDBOnlyQuery quoteQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			quoteQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteSubQuery, JoinCondition.And);

			return quoteQuery;
		}

		protected ZQuery GetRateOneOffShipmentFromDocAddressFilter(ZQuery docAddressFilter)
		{
			ZDBOnlySubQuery docAddress = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			docAddress.AddToFilter(docAddressFilter);

			ZDBOnlySubQuery quoteSubQuery = new ZDBOnlySubQuery(typeof(RateOneOffShipment), RateOneOffShipmentSchema.TT_TH);
			quoteSubQuery.AddSubQuery(docAddress, JoinCondition.And);

			ZDBOnlyQuery quoteQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			quoteQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteSubQuery, JoinCondition.And);

			return quoteQuery;
		}

		protected ZDBOnlyQuery GetQuoteOrgDocAddressQuery(DocAddressType addressType, ZGuid value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(Quote));
			ZDBOnlySubQuery oneOffSubQuery = new ZDBOnlySubQuery(typeof(RateOneOffShipment), RateOneOffShipmentSchema.TT_TH);

			oneOffSubQuery.AddSubQuery(GetOrgDocAddressQuery(addressType, RateOneOffShipmentSchema.Constants.Prefix, value), JoinCondition.And);
			result.AddSubQuery(oneOffSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region Lookups

		#region BindingLists

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#endregion

		#region ActiveStatusList

		public CodeDescriptionPairList ActiveStatusList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();

				list.AddPair(OrgConstants.FilterControl.ActiveStatus.Code.AllClients, Res.GetString("QuotedBookings|QuotedBookingFilterControl|ActiveStatusList|AllClients", "All Active and Inactive"));
				list.AddPair(OrgConstants.FilterControl.ActiveStatus.Code.ActiveClients, Res.GetString("QuotedBookings|QuotedBookingFilterControl|ActiveStatusList|ActiveClients", "Active Only"));
				list.AddPair(OrgConstants.FilterControl.ActiveStatus.Code.InactiveClients, Res.GetString("QuotedBookings|QuotedBookingFilterControl|ActiveStatusList|InactiveClients", "Inactive Only"));

				return list;
			}
		}

		#endregion

		#region ConsolidatedStatusList

		public CodeDescriptionPairList ConsolidatedStatusList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();

				list.AddPair(ConsolidatedStatus.Code.All, Res.GetString("QuotedBookings|QuotedBookingFilterControl|ConsolidatedStatusList|All", "Show All"));
				list.AddPair(ConsolidatedStatus.Code.Cons, Res.GetString("QuotedBookings|QuotedBookingFilterControl|ConsolidatedStatusList|Consolidated", "Show Consolidated/Converted Only"));
				list.AddPair(ConsolidatedStatus.Code.Ncons, Res.GetString("QuotedBookings|QuotedBookingFilterControl|ConsolidatedStatusList|NotConsolidated", "Show Not Consolidated/Converted Only"));

				return list;
			}
		}

		public static class ConsolidatedStatus
		{
			public static class Code
			{
				public const string All = "ALL";
				public const string Cons = "CONS";
				public const string Ncons = "NCONS";
			}
		}

		#endregion

		#region QuoteAndOrBookingList

		protected const string bookingOnly = "BOO";
		protected const string bookingWithQuote = "BWQ";
		protected const string bookingOnlyBookingWithQuote = "BOQ";

		protected CodeDescriptionPairList QuoteAndOrBookingList
		{
			get
			{
				if (quoteAndOrBookingList == null)
				{
					quoteAndOrBookingList = new CodeDescriptionPairList();
					quoteAndOrBookingList.AddPair(bookingOnly, Res.GetString("QuotedBookings|QuotedBookingFilterControl|QuoteBookingList|BookingOnly", "Booking Only"));
					quoteAndOrBookingList.AddPair(bookingWithQuote, Res.GetString("QuotedBookings|QuotedBookingFilterControl|QuoteBookingList|BookingWithQuote", "Booking With Quote"));
					quoteAndOrBookingList.AddPair(bookingOnlyBookingWithQuote, Res.GetString("QuotedBookings|QuotedBookingFilterControl|QuoteBookingList|BookingOnlyBookingWithQuote", "Booking Only / Booking With Quote"));
				}
				return quoteAndOrBookingList;
			}
		}
		CodeDescriptionPairList quoteAndOrBookingList;

		#endregion

		#region CreatedOnWebList

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		protected CodeDescriptionPairList CreatedOnWebList
		{
			get
			{
				if (fCreatedOnWebList == null)
				{
					fCreatedOnWebList = new CodeDescriptionPairList();
					fCreatedOnWebList.AddPair(CreatedOnCodes.All, Res.GetString("de52da63-ef62-4ff5-bb09-a04512af422d", "All Records"));
					fCreatedOnWebList.AddPair(CreatedOnCodes.WebTracker, Res.GetString("0460d845-e945-4605-aa00-691dc4c0bfeb", "Created using WebTracker"));
					fCreatedOnWebList.AddPair(CreatedOnCodes.Enterprise, Res.GetString("ec5ab000-b16a-43e1-b3a1-7682e0877957", "Created using {0}", "CargoWise"));
				}

				return fCreatedOnWebList;
			}
		}

		CodeDescriptionPairList fCreatedOnWebList;

		protected static class CreatedOnCodes
		{
			public const string All = "ALL";
			public const string WebTracker = "WEB";
			public const string Enterprise = "ENT";
		}

		#endregion

		#region StaffList

		protected GlbStaffCollection StaffList
		{
			get
			{
				if (fStaffList == null)
				{
					fStaffList = new GlbStaffCollection(Factory);
				}

				return fStaffList;
			}
		}

		GlbStaffCollection fStaffList;

		#endregion

		#region ShipmentStatusList

		public CodeDescriptionPairList ShipmentStatusList
		{
			get
			{
				if (shipmentStatusList == null)
				{
					shipmentStatusList = new CodeDescriptionPairList();
					shipmentStatusList.Add(new CodeDescriptionPair(Integration.ShipmentStatusList.Codes.ElectronicBooking, Res.GetString("468b226b-6e0e-45bb-9aeb-1c75894cd565", "eBooking Request Received")));
					shipmentStatusList.Add(new CodeDescriptionPair(Integration.ShipmentStatusList.Codes.EBookingCancellationRequest, Integration.ShipmentStatusList.Descriptions.EBookingCancellationRequest));
					shipmentStatusList.Add(new CodeDescriptionPair(Integration.ShipmentStatusList.Codes.Booked, Res.GetString("5c8c6227-d237-49ef-bb6c-b18515582ff8", "Booking Confirmed")));
					shipmentStatusList.Add(new CodeDescriptionPair(Integration.ShipmentStatusList.Codes.BookingCancelled, Integration.ShipmentStatusList.Descriptions.BookingCancelled));
					shipmentStatusList.Add(new CodeDescriptionPair(Integration.ShipmentStatusList.Codes.BookingRejected, Integration.ShipmentStatusList.Descriptions.BookingRejected));
					shipmentStatusList.Add(new CodeDescriptionPair(Integration.ShipmentStatusList.Codes.ElectronicShippingInstruction, Res.GetString("9a63f9a0-ba58-4fed-b770-4c02b3dcb12a", "eSI Received")));
					shipmentStatusList.Add(new CodeDescriptionPair(Integration.ShipmentStatusList.Codes.Confirmed, Res.GetString("ae0ae0a1-3e14-4b15-a0ac-b323c305d538", "SI Confirmed")));
					shipmentStatusList.Add(new CodeDescriptionPair(Integration.ShipmentStatusList.Codes.SIRejected, Res.GetString("22b1f3c9-b730-4a0b-b920-8b7e7ae5b3a5", "SI Rejected")));
					shipmentStatusList.Add(new CodeDescriptionPair(Integration.ShipmentStatusList.Codes.WebBooking, Integration.ShipmentStatusList.Descriptions.WebBooking));
				}

				return shipmentStatusList;
			}
		}
		CodeDescriptionPairList shipmentStatusList;

		#endregion

		#region QuoteApprovalStatusList

		public CodeDescriptionPairList OneOffQuoteApprovalStatusList
		{
			get
			{
				var list = new CodeDescriptionPairList();

				list.AddPair(OrgConstants.FilterControl.ActiveStatus.Code.AllClients, Res.GetString("QuotedBookings|QuotedBookingFilterControl|OneOffQuoteStatusList|AllClients", "All One Off Quotes"));
				list.AddPair(OrgConstants.FilterControl.ActiveStatus.Code.ActiveClients, Res.GetString("QuotedBookings|QuotedBookingFilterControl|OneOffQuoteStatusList|ActiveClients", "Approved One Off Quotes Only"));
				list.AddPair(OrgConstants.FilterControl.ActiveStatus.Code.InactiveClients, Res.GetString("QuotedBookings|QuotedBookingFilterControl|OneOffQuoteStatusList|InactiveClients", "Not Approved One Off Quotes Only"));

				return list;
			}
		}

		#endregion

		#region One Off Quote KPI List

		public OneOffQuoteKPIList OneOffQuoteKPIList
		{
			get { return Factory.GetCachedValue<OneOffQuoteKPIList>(); }
		}

		#endregion

		#region One Off Quote Source List

		public OneOffQuoteSourceList OneOffQuoteSourceList
		{
			get { return Factory.GetCachedValue<OneOffQuoteSourceList>(); }
		}

		#endregion

		#region One Off Quote Revision Reason List

		public OneOffQuoteRevisionReasonList OneOffQuoteRevisionReasonList
		{
			get { return Factory.GetCachedValue<OneOffQuoteRevisionReasonList>(); }
		}

		#endregion

		#endregion

		#region Support

		public class FilterBuilder
		{
			readonly ModuleFilterCollection filters;
			readonly FilterCategory category;

			ModuleFilter lastFilter;

			public FilterBuilder(ModuleFilterCollection filters, FilterCategory category)
			{
				this.filters = filters;
				this.category = category;
			}

			public FilterBuilder WithMaxLengthOf(SchemaColumn column)
			{
				lastFilter.MaxLength = column.MaxLength;
				return this;
			}

			public FilterBuilder MultilingualDescription(MultilingualString description)
			{
				lastFilter.MultilingualDescription = description;
				return this;
			}

			public FilterBuilder AddFountainFilter(ZString description, GetTextQueryWithOperator queryDelegate, ZString fountainPrefix)
			{
				lastFilter = filters.AddFountainFilter(description, queryDelegate, fountainPrefix);
				lastFilter.Category = category;
				return this;
			}

			public FilterBuilder AddNumberFilter(ZString description, GetTextQueryWithOperator queryDelegate)
			{
				lastFilter = filters.AddNumberFilter(description, queryDelegate);
				lastFilter.Category = category;
				return this;
			}

			public FilterBuilder AddTextFilter(ZString description, GetTextQueryWithOperator queryDelegate)
			{
				lastFilter = filters.AddTextFilter(description, queryDelegate);
				lastFilter.Category = category;
				return this;
			}

			public FilterBuilder AddTextAndNkFilter(ZString description, GetTextAndNkQuery queryDelegate, ModuleIdentifier iD, IBusinessObjectCollection list)
			{
				lastFilter = filters.AddTextAndNkFilter(description, queryDelegate, iD, list);
				lastFilter.Category = category;
				return this;
			}

			public FilterBuilder AddDateFilter(ZString description, GetDateQuery queryDelegate)
			{
				lastFilter = filters.AddDateFilter(description, queryDelegate);
				lastFilter.Category = category;
				return this;
			}

			public FilterBuilder AddDateTimeFilter(ZString description, SchemaDateTimeColumn dateTimeColumn, BlueprintModuleFilterSubGroup subGroup = null)
			{
				lastFilter = filters.AddDateFilter(description, dateTimeColumn);
				lastFilter.Category = category;
				if (subGroup != null)
				{
					lastFilter.SubGroup = subGroup;
				}
				return this;
			}

			public FilterBuilder AddStatusFilter(ZString description, GetTextQuery queryDelegate, CodeDescriptionPairList statusList, ZString defaultCode)
			{
				lastFilter = filters.AddTextFilter(description, queryDelegate, statusList);
				((ModuleTextFilter)lastFilter).DefaultProperty = defaultCode;
				lastFilter.Category = category;
				return this;
			}
		}

		#endregion

		#region QuotedBookingReferenceNumberFilterHelper

		protected sealed class QuotedBookingReferenceNumberFilterHelper : ReferenceNumberFilterHelper<ViewQuotedBooking>
		{
			protected override ZQuery GetReferenceNumberFilterCore(SQLComparisonOperator opp, ZString country, ZString type, ZString number, bool notIn, bool filterOnEmptyNumber)
			{
				ZDBOnlySubQuery entryNumFilter = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, notIn);
				entryNumFilter.AddToFilter(GetCusEntryNumFilter(opp, country, type, number, filterOnEmptyNumber));

				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
				result.AddSubQuery(ViewQuotedBookingSchema.VB_JS, entryNumFilter, JoinCondition.And);

				if (notIn)
				{
					result.AddToFilter(JoinCondition.Or, ViewQuotedBookingSchema.VB_JS, null);
				}

				return result;
			}
		}

		#endregion

		#region IAccountingFilterStripHolder Members

		protected IAccountingFilterStrip AccountingFilterStrip
		{
			get
			{
				if (accountingFilterStrip == null)
				{
					accountingFilterStrip = accountingFilterStrip = (IAccountingFilterStrip)Activator.CreateInstance(ObjectFactory.GetType<IAccountingFilterStrip>(), this);
					accountingFilterStrip.Initialize(addProfitLossReasonFilters: true);
				}

				return accountingFilterStrip;
			}
		}

		IAccountingFilterStrip accountingFilterStrip;

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			var result = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			result.AddSubQuery(billingPKSubQuery, JoinCondition.And);
			return result;
		}

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new Dictionary<string, object>()
		{
			{ AccountingFilterStripConfigurationKeys.BusinessObjectType, typeof(QuotedBooking) }
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

		#region CO2e

		protected void AddCO2Filters(ModuleFilterCollection filters)
		{
			var co2eFilter = new CO2eStatusAndCO2eKgRangeNumberFilter(Descriptions.NumbersAndReferences.CO2e, typeof(QuotedBooking))
			{
				MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|CO2e", "CO2e (kg)"),
				Category = FilterCategories.NumbersAndReferences
			};
			filters.AddFilter(co2eFilter);
		}

		#endregion
	}
}
