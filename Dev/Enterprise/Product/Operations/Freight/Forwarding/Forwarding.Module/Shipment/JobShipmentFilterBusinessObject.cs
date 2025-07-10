using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.Module;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Module;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.TransportBookings.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using CustomsUniversal = Enterprise.Customs.Universal;

namespace Enterprise.Freight.Forwarding.Module
{
	public class JobShipmentFilterBusinessObject : FilterStripBusinessObject, IAccountingFilterStripHolder
	{
		public JobShipmentFilterBusinessObject()
			: this(false)
		{
		}

		public JobShipmentFilterBusinessObject(bool allowTemplateRecords)
		{
			AllowTemplateRecords = allowTemplateRecords;
		}

		#region Description Constants

		public static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string CoLoadStatus = "Co-Load Status";
			public const string ShowTranshipCrossTradeNoConsol = "Show Tranship, Cross Trade, No Consol";
			public const string HiddenFwdRegistered = "Hidden ForwardRegistered Filter";

			public const string WarehouseLocation = "Warehouse Location";

			public const string NAFTADutyDeferralStatus = "NAF (NAFTA Duty Deferral) Status";
			public const string IncoTerms = "INCO Term";

			public const string ETA = "ETA";
			public const string ETD = "ETD";
			public const string FirstPortOfArrivalDate = "First Port of Arrival Date";

			public const string ATALoad = "ATA / Load Port";
			public const string ATDLoad = "ATD / Load Port";
			public const string ETALoad = "ETA / Load Port";
			public const string ETDLoad = "ETD / Load Port";
			public const string ATADischarge = "ATA / Discharge Port";
			public const string ETADischarge = "ETA / Discharge Port";
			public const string DeliveryDueDate = "Delivery Due Date";
			public const string RevisedDeliveryDueDate = "Revised Delivery Due Date";

			public const string FlightVoyageAndVessel = "Flight/Voyage # and Vessel";

			public const string BookingReferenceNum = "Booking Reference #";
			public const string ConsolNum = "Consol #";
			public const string HouseBill = "House Bill";
			public const string MasterBill = "Master Bill";
			public const string OrderNum = "Order #";
			public const string ShipmentNum = "Shipment #";
			public const string InteriumReceiptNum = "Interim Receipt #";
			public const string PackLineReferenceNum = "PackLine Reference #";
			public const string ContainerNum = "Container #";
			public const string AdditionalReferenceNumbers = "Additional Reference #";
			public const string CO2e = "CO2e";
			public const string CompanyTariffLevelOverride = "Company Tariff Level Override";
			public const string FMCTariffID = "FMC Tariff ID";
			public const string RateCommodity = "Rate Commodity";

			public const string PickupAgent = "Pickup Agent";
			public const string DeliveryAgent = "Delivery Agent";
			public const string ImportBroker = "Import Broker";
			public const string ExportBroker = "Export Broker";
			public const string ExportBrokerCartage = "Export Broker / Cartage";
			public const string ImportBrokerCartage = "Import Broker / Cartage";

			public const string PickupTransportCompany = "Pickup Transport Company";
			public const string DeliveryTransportCompany = "Delivery Transport Company";
			public const string ShipmentSendReceiveForwarders = "Shipment Send / Receive Forwarders";
			public const string TranshipmentAgent = "Transhipment Agent";
			public const string ConsolSendReceiveAgents = "Consol Send / Receive Agents";
			public const string PickupCFS = "Pickup CFS";
			public const string DeliveryCFS = "Delivery CFS";

			public const string PickupCFSReceiptRequested = "Pickup CFS/TW / Receipt Requested Date";
			public const string DeliveryCFSReceiptRequested = "Delivery CFS/TW / Receipt Requested Date";
			public const string PickupCFSDispatchRequested = "Pickup CFS/TW / Dispatch Requested Date";
			public const string DeliveryCFSDispatchRequested = "Delivery CFS/TW / Dispatch Requested Date";

			public const string InterimReceiptDate = "Interim Receipt Date";

			public const string ActualPickupDate = "Actual Pickup Date";
			public const string ActualDeliveryDate = "Actual Delivery Date";

			public static string ConsignorConsignee
			{
				get { return FreightDataRegistry.Instance.ConsignorShipperTerminology.Value.GetUnresolvedString() + " / Consignee"; }
			}
			public const string ConsignorRelatedParties = "Consignor Related Parties";
			public const string ConsigneeRelatedParties = "Consignee Related Parties";
			public const string LocalClientRelatedParties = "Local Client Related Parties";
			public const string ClientAssignedStaff = "Client Assigned Staff";
			public const string Consignor = "Consignor";
			public const string Consignee = "Consignee";
			public const string LocalClientBilling = "Local Client (Billing)";
			public const string Buyer = "Order - Buyer";
			public const string Supplier = "Order - Supplier";
			public const string Gateway = "Gateway";

			public const string LoadDischarge = "Load / Discharge";
			public const string OriginDestination = "Origin / Destination";
			public const string PlannedLoadDischarge = "Planned Load / Planned Discharge";

			public const string ReleaseType = "Release Type";
			public const string EFreightStatus = "EFreight Status";
			public const string BookingStatus = "Booking Status";

			public const string IsHazardous = "Is Hazardous";

			public const string RelatedConsols = "Related Consolidations";

			public const string IsTemperatureControlled = "Is Temperature Controlled";

			public const string CTStatus = "CT Status";

			public const string RelatedTransportBookings = "Related Transport Booking";

			public const string HasDamagedPackages = "Has Damaged Packages";
			public const string HasPillagedPackages = "Has Pillaged Packages";
			public const string OriginTransitWarehouseStatus = "TW Matching Status";

			public const string ExitStatus = "Exit Status";

			public const string HBLStatus = "Electronic HBL Status";
			public const string HBLType = "Electronic HBL Type";
			public const string HBLTerms = "Electronic HBL Terms";

			#endregion
		}

		public static class CusEntryHeaderMessageType
		{
			public const string BorderCargoRelease = "BCR";
			public const string CargoRelease = "CRL";
			public const string SimplifiedEntry = "SE";
			public const string EntrySummary = "ENS";
			public const string Export = "ITN";
			public const string InBond = "INB";
			public const string NAFTADutyDeferral = "NAF";
		}

		#endregion

		public static class FilterStatus
		{
			public const string NotSentCustomsStatusForFilter = "NOT";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "US Code list filter value")]
			public const string NotSentForFilterDescription = "Not Sent - only valid for exact match";
			public const string MultipleEntriesStatus = "MES";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "US Code list filter value")]
			public const string MultipleBillsStatus = "Multiple bills have different statuses.";
		}

		protected sealed override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = GetJobShipmentModuleFiltersCore();
			ObjectFactory.Get<Enterprise.Integration.Customs.CA.IShipmentModuleColumnsAndFiltersProvider>().AddFilters(result, Factory);
			if (ShouldAddCRMSecurityFilters)
			{
				securityProvider.AddCRMSecurityFilterStrips(Factory, result);
			}

			return result;
		}

		protected virtual bool ShouldAddCRMSecurityFilters => true;

		protected virtual ModuleFilterCollection GetJobShipmentModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			AddNumberFilters(result);
			AddTextFilters(result);
			AddDateFilters(result);
			AddOrganisationFilters(result);
			AddLocationFilters(result);
			AddModeFilters(result);
			AddStatusAndFlagsFilters(result);
			AddRelatedConsolidationsFilter(result);
			AddRelatedTransportBookingsFilter(result);
			AddCustomsFilter(result);
			AddTemplateRecordFilter(result);
			AddPickUpDeliveryDropModeFilters(result);

			CusEntryNumberFilterStripHelper.AddReferenceNumberDateFilter(result, typeof(ForwardingShipment));

			if (!Globals.IsWeb)
			{
				var attributeMan = new AttributeManager();
				result.AddAttributeFilters(attributeMan.GetAllAttributes(AttributeManager.AttributeModules.Order, null, LoggedInWebUsersOrg), GetAttributeFilter, FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("a483fa40-040b-4396-9477-6b7b0f6d3509", "Order Manager Attribute Search")));
				result.AddAttributeFilters(attributeMan.GetAllAttributes(AttributeManager.AttributeModules.CommercialInvoice, null, LoggedInWebUsersOrg), GetAttributeFilter, FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("a2c13512-49c8-47ee-91d0-58bedba8b4ad", "Commercial Invoice Attribute Search")));
			}

			AccountingFilterStrip.AddBillingFilters(result);
			AccountingFilterStrip.AddJobManagementFilters(result, Env.Security.MaintainShipmentJobInvoicing);

			return result;
		}

		#region GetModuleFilterThatOverridesAllOtherFilters

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			ModuleFountainFilter shipmentNoFilter = new ModuleFountainFilter(Descriptions.ShipmentNum, GetShipmentNumberQuery, "S");
			shipmentNoFilter.WithMaxLengthOf<ModuleFountainFilter>(JobShipmentSchema.JS_UniqueConsignRef);
			shipmentNoFilter.IsCommon = true;
			shipmentNoFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|Shipment", "Shipment #");
			shipmentNoFilter.SupportsXQuery = true;
			shipmentNoFilter.SupportsEndsWithComparisonOperator = true;
			return shipmentNoFilter;
		}

		ZQuery GetShipmentNumberQuery(SQLComparisonOperator comparisonOperator, ZString shipmentNumber)
		{
			if (comparisonOperator == SQLComparisonOperator.EndsWith)
			{
				var result = new ZDBOnlyQuery(typeof(ForwardingShipment));

				var sql = "JS_UniqueConsignRef_Reversed LIKE @shipmentNumber";
				var shipmentNumberReversed = new string(shipmentNumber.ToString().Reverse().ToArray());

				var parameters = new ZSqlParameterCollection();
				parameters.Add("@shipmentNumber", $"{shipmentNumberReversed}%", JobShipmentSchema.JS_UniqueConsignRef);
				result.AddFilterAndZSQLParameterCollection(sql, parameters);

				return result;
			}

			var filter = new ZDBOnlyQuery(typeof(ForwardingShipment));
			filter.AddToFilter_PossiblyCommaSeparated(JobShipmentSchema.JS_UniqueConsignRef, comparisonOperator, shipmentNumber);
			return filter;
		}

		#endregion

		#region Number Filters

		protected virtual void AddNumberFilters(ModuleFilterCollection filters)
		{
			var orderLineProductCodeFilter = filters.AddNumberFilter("Order Line Product Code", JobOrderLineSchema.JO_Partno);
			orderLineProductCodeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|RoutingRequestFilter|OrderLineProductCode", "Order Line Product Code");
			orderLineProductCodeFilter.SubGroup = OrderLineSubGroup;

			filters.AddNumberFilter("Invoice Line Product Code", GetInvoiceLineProductCode)
				.WithMaxLengthOf<ModuleNumberFilter>(JobComInvoiceLineSchema.JI_PartNo)
				.MultilingualDescription = ResString.GetMultilingualString("Forwarding|RoutingRequestFilter|InvoiceLineProductCode", "Invoice Line Product Code");

			var bookingReferenceFilter = filters.AddNumberFilter(Descriptions.BookingReferenceNum, JobConsolSchema.JK_BookingReference);
			bookingReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|BookingReference", "Booking Reference #");
			bookingReferenceFilter.SubGroup = ConsolSubGroup;

			var filter = filters.AddNumberFilter("Quote #", GetQuoteNoQuery);
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|QuoteNumber", "Quote #");
			filter.UseMultiSearch = false; //TODO: Needs to work with multiple values. (GetQuoteNoQuery)

			var commercialInvoiceFilter = filters.AddNumberFilter("Commercial Invoice #", JobComInvoiceHeaderSchema.JZ_InvoiceNumber);
			commercialInvoiceFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|CommercialInvoice", "Commercial Invoice #");
			commercialInvoiceFilter.SubGroup = CommercialInvoiceSubGroup;

			var consolFilter = filters.AddFountainFilter(Descriptions.ConsolNum, GetConsolNoQuery, "C")
				.WithMaxLengthOf<ModuleFountainFilter>(JobConsolSchema.JK_UniqueConsignRef);
			consolFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|Consol", "Consol #");
			consolFilter.MultiValueQueryDelegate = ConsolMultiValueQuery;

			filters.AddNumberFilter(Descriptions.ContainerNum, GetContainerNoQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobContainerSchema.JC_ContainerNum)
				.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|Container", "Container #");

			ModuleNumberFilter houseBillFilter = filters.AddNumberFilter(Descriptions.HouseBill, GetHouseBillQuery);
			houseBillFilter.WithMaxLengthOf<ModuleNumberFilter>(JobShipmentSchema.JS_HouseBill);
			houseBillFilter.IsCommon = true;
			houseBillFilter.SupportsEndsWithComparisonOperator = true;
			houseBillFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|HouseBill", "House Bill");

			filters.AddNumberFilter(Descriptions.InteriumReceiptNum, JobShipmentSchema.JS_InterimReceipt).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|InterimReceipt", "Interim Receipt #");
			filters.AddNumberFilter(Descriptions.MasterBill, GetConsolMasterBillQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobConsolSchema.JK_MasterBillNum)
				.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|MasterBill", "Master Bill");

			filters.AddNumberFilter(Descriptions.OrderNum, GetOrderNoQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobOrderItemSchema.JT_OrderReference)
				.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|Order", "Order #");

			var packLineReferenceFilter = filters.AddNumberFilter(Descriptions.PackLineReferenceNum, JobPackLinesSchema.JL_RefNumber);
			packLineReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|PackLineReference", "Pack Line Reference #");
			packLineReferenceFilter.SubGroup = PackLineSubGroup;

			var exportRefNumberFilter = filters.AddTextFilter("Pack Line Export Reference #", JobPackLinesSchema.JL_ExportRefNumber);
			exportRefNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|PackLineExportRefNumber", "Pack Line Export Reference #");
			exportRefNumberFilter.Category = FilterCategories.NumbersAndReferences;
			exportRefNumberFilter.SubGroup = PackLineSubGroup;
			exportRefNumberFilter.UseMultiSearch = true;

			var importRefNumberFilter = filters.AddTextFilter("Pack Line Import Reference #", JobPackLinesSchema.JL_ImportRefNumber);
			importRefNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|PackLineImportRefNumber", "Pack Line Import Reference #");
			importRefNumberFilter.Category = FilterCategories.NumbersAndReferences;
			importRefNumberFilter.SubGroup = PackLineSubGroup;
			importRefNumberFilter.UseMultiSearch = true;

			filters.AddNumberFilter("Shipper's Reference #", JobShipmentSchema.JS_BookingReference).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|Commodity", "Shipper's Reference #");
			filters.AddNumberFilter("CFS Reference #", JobShipmentSchema.JS_CFSReference).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|CFSReference", "CFS Reference #");

			var voyageVesselFilter = new VoyageVesselModuleFilter(Descriptions.FlightVoyageAndVessel, GetFlightVoyageNumberAndVesselQuery, BindingLists.RefVessel_List)
				.WithMaxLengthOf(JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
			voyageVesselFilter.Category = FilterCategories.NumbersAndReferences;
			voyageVesselFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|FlightVoyageAndVessel", "Flight/Voyage # and Vessel");
			filters.AddCustomFilter(voyageVesselFilter);

			var filter2 = filters.AddNkFilter("Commodity Code", GetCommodityCodeQuery, ModuleIDs.RefCommodityCode, BindingLists.RefCommodityCode_List);
			filter2.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|CommodityCode", "Commodity Code");
			filter2.Category = FilterCategories.NumbersAndReferences;
			filter2.MultiValueQueryDelegate = GetMultiCommodityCodesQuery;

			if (WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.Value)
			{
				filters.AddCustomFilter(
					new ModuleWarehouseLocationFilter(Descriptions.WarehouseLocation, GetWarehouseLocationFilter, WarehouseList) { MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|WarehouseLocation", "Warehouse Location") });
			}
			else
			{
				var warehouseLocationFilter = filters.AddTextFilter(Descriptions.WarehouseLocation, JobPackLocSchema.JQ_WarehouseLocation);
				warehouseLocationFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|WarehouseLocation", "Warehouse Location");
				warehouseLocationFilter.SubGroup = WarehouseLocationSubGroup;
			}

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Iceland)
			{
				var sendingarnumerFilter = filters.AddTextFilter("Sendingarnumer", CusEntryNumSchema.CE_EntryNum);
				sendingarnumerFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|Sendingarnumer", "Sendingarnumer");
				sendingarnumerFilter.SubGroup = SendingarnumerSubGroup;
			}

			if (FreightConfigurationRegistry.Instance.EnableClientContractNumber.Value)
			{
				var clientContractNumberFilter = filters.AddNumberFilter("Client Contract #", GetClientContractNoQuery);
				clientContractNumberFilter.Category = FilterCategories.NumbersAndReferences;
				clientContractNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ClientContractNumber", "Client Contract #");
			}

			if (ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled)
			{
				var co2eFilter = new CO2eStatusAndCO2eKgRangeNumberFilter(Descriptions.CO2e, typeof(ForwardingShipment))
				{
					MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|CO2e", "CO2e (kg)"),
					Category = FilterCategories.NumbersAndReferences,
				};
				filters.AddCustomFilter(co2eFilter);
			}

			AddHVLVNumbersFilters(filters);
			AddCompanyTariffLevelOverrideFilters(filters);
			AddRateCommodityFilter(filters);
			AddFMCTariffIDFilter(filters);

			filters.AddFountainFilter("Direct Master/Lead Shipment #", GetMasterLeadShipmentNumber, "S")
				.WithMaxLengthOf<ModuleFountainFilter>(JobShipmentSchema.JS_UniqueConsignRef)
				.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|DirectMasterLeadShipmentNumber", "Direct Master/Lead Shipment #");

			var referenceNumberFilter = new ReferenceNumberFilter(Descriptions.AdditionalReferenceNumbers,
				new ReferenceNumberFilterHelper<ForwardingShipment>().GetReferenceNumberFilter,
				new RefCountryCollection(Factory))
				.WithMaxLengthOf<ReferenceNumberFilter>(CusEntryNumSchema.CE_EntryNum);

			referenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ReferenceNumbers", "Additional Reference #");
			filters.AddCustomFilter(referenceNumberFilter);
		}

		void AddCompanyTariffLevelOverrideFilters(ModuleFilterCollection filters)
		{
			var companyTariffLevelOverrideFilter = filters.AddTextFilter(Descriptions.CompanyTariffLevelOverride, GetCompanyTariffLevelOverride, CompanyTariffLevelOverrideList);
			companyTariffLevelOverrideFilter.Category = FilterCategories.NumbersAndReferences;
			companyTariffLevelOverrideFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|CompanyTariffLevelOverride", "Company Tariff Level Override");
		}

		ZQuery GetCompanyTariffLevelOverride(ZString value)
		{
			var result = new ZQuery();
			if (!value.IsEmpty)
			{
				ZByte.TryParse(value, out var companyTariffLevelOverride);
				result.AddToFilter(JobShipmentSchema.JS_CompanyTariffLevelOverride, companyTariffLevelOverride);
			}
			return result;
		}

		void AddFMCTariffIDFilter(ModuleFilterCollection filters)
		{
			var fmcTariffIDFilter = filters.AddTextFilter(Descriptions.FMCTariffID, JobShipmentSchema.JS_FMCTariffID);
			fmcTariffIDFilter.MaxLength = JobShipmentSchema.JS_FMCTariffID.MaxLength;
			fmcTariffIDFilter.Category = FilterCategories.NumbersAndReferences;
			fmcTariffIDFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|FMCTariffID", "FMC Tariff ID");
		}

		void AddRateCommodityFilter(ModuleFilterCollection filters)
		{
			var rateCommodityFilter = filters.AddNkFilter(Descriptions.RateCommodity, JobShipmentSchema.JS_RH_NKRateCommodity, ModuleIDs.RefCommodityCode, BindingLists.RefCommodityCode_List);
			rateCommodityFilter.Category = FilterCategories.NumbersAndReferences;
			rateCommodityFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|RateCommodity", "Rate Commodity");
		}

		#region GeCommodityCodeQuery

		ZQuery GetCommodityCodeQuery(ZString commodityCode) => GetMultiCommodityCodesQuery(new List<ZString> { commodityCode }, SQLComparisonOperator.Equal);

		ZQuery GetMultiCommodityCodesQuery(object value, SQLComparisonOperator filterOperator)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			if (value is List<ZString> codeList && codeList.Count > 0)
			{
				var commodityCodeList = codeList.Where(x => Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, x) != null);
				if (commodityCodeList.Any())
				{
					var packSubQuery = new ZDBOnlySubQuery(typeof(ForwardingPackLine), JobPackLinesSchema.JL_JS);
					var subQuery = new ZQuery();
					subQuery.AddToFilter(JoinCondition.Or, JobPackLinesSchema.JL_RH_NKCommodityCode, SQLComparisonOperator.Equal, commodityCodeList);
					packSubQuery.AddToFilter(subQuery, JoinCondition.And);

					result.AddSubQuery(packSubQuery, JoinCondition.And);

					var invoiceLineSubQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.IBaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
					var subQuery2 = new ZQuery();
					subQuery2.AddToFilter(JoinCondition.Or, JobComInvoiceLineSchema.JI_RH_NKCommodity_Code, SQLComparisonOperator.Equal, commodityCodeList);
					invoiceLineSubQuery.AddToFilter(subQuery2);
					invoiceLineSubQuery.TableIndexHints.Add(new TableIndexHint("NR_RC__JI_ClusterKey")); // Schema Constant doesn't contain this index
					invoiceLineSubQuery.IsForceSeek = true;

					var declarationSubQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.JE_JS);
					declarationSubQuery.TableIndexHints.Add(new TableIndexHint("FK_RX__JE_JS_JE_GC"));
					declarationSubQuery.IsForceSeek = true;
					declarationSubQuery.AddSubQuery(invoiceLineSubQuery, JoinCondition.And);

					result.AddSubQuery(declarationSubQuery, JoinCondition.Or);
					result.IsForceSeek = false;
				}
			}

			return result;
		}

		#endregion

		#region GetConsolMasterBillQuery

		ZQuery GetConsolMasterBillQuery(SQLComparisonOperator @operator, ZString masterBill)
		{
			return GetConsolQuery(JobConsolSchema.JK_MasterBillNum, @operator, masterBill);
		}

		#endregion

		#region GetConsolNoQuery

		ZQuery GetConsolNoQuery(SQLComparisonOperator comparisonOperator, ZString consolID)
		{
			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));
				ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS, true);

				result.AddSubQuery(pivotSubQuery, JoinCondition.And);

				return result;
			}

			return GetConsolQuery(JobConsolSchema.JK_UniqueConsignRef, comparisonOperator, consolID);
		}

		ZQuery ConsolMultiValueQuery(object value, SQLComparisonOperator filterOperator)
		{
			var result = new ZDBOnlyQuery(typeof(CommonShipment));

			if (value is List<ZString> consolIdList && consolIdList.Count > 0)
			{
				var jobConShipLinkQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);

				var jobConsolQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConsolSchema.PK);
				jobConsolQuery.AddToFilter(JoinCondition.Or, JobConsolSchema.JK_UniqueConsignRef, filterOperator, consolIdList);
				jobConShipLinkQuery.AddSubQuery(JobConShipLinkSchema.JN_JK, jobConsolQuery, JoinCondition.And);

				result.AddSubQuery(jobConShipLinkQuery, JoinCondition.And);
			}

			return result;
		}

		#endregion

		#region GetHouseBillQuery

		ZQuery GetHouseBillQuery(SQLComparisonOperator comparisonOperator, ZString houseBillNumber)
		{
			if (comparisonOperator == SQLComparisonOperator.EndsWith)
			{
				var result = new ZDBOnlyQuery(typeof(ForwardingShipment));

				var sql = "JS_HouseBill_Reversed LIKE @houseBillNumber";
				var houseBillNumberReversed = new string(houseBillNumber.ToString().Reverse().ToArray());

				var parameters = new ZSqlParameterCollection();
				parameters.Add("@houseBillNumber", $"{houseBillNumberReversed}%", JobShipmentSchema.JS_HouseBill);
				result.AddFilterAndZSQLParameterCollection(sql, parameters);

				return result;
			}

			var filter = new ZDBOnlyQuery(typeof(ForwardingShipment));
			filter.AddToFilter_PossiblyCommaSeparated(JobShipmentSchema.JS_HouseBill, comparisonOperator, houseBillNumber);
			return filter;
		}

		#endregion

		#region GetContainerNoQuery

		protected virtual ZQuery GetContainerNoQuery(SQLComparisonOperator comparisonOperator, ZString containerNo)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			ZDBOnlySubQuery packLineQuery = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.JL_JS);
			ZDBOnlySubQuery containerPackPivotQuery = new ZDBOnlySubQuery(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JL);
			ZDBOnlySubQuery containerSubQuery = new ZDBOnlySubQuery(typeof(CommonContainer), JobContainerPackPivotSchema.J6_JC);

			containerSubQuery.AddToFilter_PossiblyCommaSeparated(
				JoinCondition.And,
				JobContainerSchema.JC_ContainerNum, comparisonOperator, containerNo);

			containerPackPivotQuery.AddSubQuery(containerSubQuery, JoinCondition.And);
			packLineQuery.AddSubQuery(containerPackPivotQuery, JoinCondition.And);
			result.AddSubQuery(packLineQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region GetInvoiceLineProductCode

		ZQuery GetInvoiceLineProductCode(SQLComparisonOperator @operator, ZString lineProductCode)
		{
			ZDBOnlyQuery partSubQuery = ObjectFactory.Get<ICustomsFilterProvider>().GetInvoiceLineProductCodeQuery(@operator, lineProductCode);
			return GetCommercialInvoiceQuery(partSubQuery);
		}

		#endregion

		#region GetOrderNoQuery

		ZQuery GetOrderNoQuery(SQLComparisonOperator comparisonOperator, ZString orderNo)
		{
			ZQuery result = new ZQuery();

			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				ZDBOnlyQuery orderResult = new ZDBOnlyQuery(typeof(ForwardingShipment));
				ZDBOnlySubQuery orderSubQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.JD_JS, true);
				orderSubQuery.AddToFilter(JoinCondition.And, JobOrderHeaderSchema.JD_JS, SQLComparisonOperator.NotEqual, null);

				ZDBOnlySubQuery whsPivotSubQuery = new ZDBOnlySubQuery(typeof(IWhsDocketJobPivot), WhsDocketJobPivotSchema.WV_ParentId, true);
				ZDBOnlySubQuery docsAndCartageOrderSubQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
				ZDBOnlySubQuery orderItemSubQuery = new ZDBOnlySubQuery(typeof(OrderItem), JobOrderItemSchema.JT_JP, true);
				docsAndCartageOrderSubQuery.AddSubQuery(orderItemSubQuery, JoinCondition.And);

				orderResult.AddSubQuery(orderSubQuery, JoinCondition.And);
				orderResult.AddSubQuery(whsPivotSubQuery, JoinCondition.And);
				orderResult.AddSubQuery(docsAndCartageOrderSubQuery, JoinCondition.And);
				result.AddToFilter(orderResult);
			}
			else
			{
				ZDBOnlyQuery orderResult = new ZDBOnlyQuery(typeof(ForwardingShipment));
				ZDBOnlySubQuery orderSubQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.JD_JS);

				orderSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobOrderHeaderSchema.JD_OrderNumber, comparisonOperator, orderNo);
				orderResult.AddSubQuery(orderSubQuery, JoinCondition.And);
				result.AddToFilter(orderResult);

				ZDBOnlyQuery whsOrderResult = new ZDBOnlyQuery(typeof(ForwardingShipment));
				ZDBOnlySubQuery whsPivotSubQuery = new ZDBOnlySubQuery(typeof(IWhsDocketJobPivot), WhsDocketJobPivotSchema.WV_ParentId);
				ZDBOnlySubQuery whsOrderSubQuery = new ZDBOnlySubQuery(typeof(IWhsOrder), WhsDocketJobPivotSchema.WV_WD_Docket);

				whsOrderSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, WhsDocketSchema.WD_ExternalReference, comparisonOperator, orderNo);
				whsPivotSubQuery.AddToFilter(JoinCondition.And, WhsDocketJobPivotSchema.WV_DocketType, "ORD");
				whsPivotSubQuery.AddSubQuery(whsOrderSubQuery, JoinCondition.And);
				whsOrderResult.AddSubQuery(whsPivotSubQuery, JoinCondition.And);
				result.AddToFilter(whsOrderResult, JoinCondition.Or);

				ZDBOnlyQuery docsAndCartageOrderResult = new ZDBOnlyQuery(typeof(ForwardingShipment));
				ZDBOnlySubQuery cartageSubQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
				ZDBOnlySubQuery orderItemSubQuery = new ZDBOnlySubQuery(typeof(OrderItem), JobOrderItemSchema.JT_JP);

				orderItemSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobOrderItemSchema.JT_OrderReference, comparisonOperator, orderNo);
				cartageSubQuery.AddSubQuery(orderItemSubQuery, JoinCondition.And);
				docsAndCartageOrderResult.AddSubQuery(cartageSubQuery, JoinCondition.And);
				result.AddToFilter(docsAndCartageOrderResult, JoinCondition.Or);

				//JobShipment > JobPackLine > ContainerLoadListLine > JobSupplierBookingLine > JobOrderLIne > JobOrderHeader
				ZDBOnlyQuery packLineResult = new ZDBOnlyQuery(typeof(ForwardingShipment));
				ZDBOnlySubQuery packLineSubQuery = new ZDBOnlySubQuery(typeof(ForwardingPackLine), JobPackLinesSchema.JL_JS);
				ZDBOnlySubQuery containerLoadListLineSubQuery = new ZDBOnlySubQuery(typeof(ContainerLoadListLine), ContainerLoadListLineSchema.CLL_JL_PackLine);
				ZDBOnlySubQuery supplierBookingLineSubQuery = new ZDBOnlySubQuery(typeof(JobSupplierBookingLine), JobSupplierBookingLineSchema.PK);
				ZDBOnlySubQuery orderLineSubQuery1 = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.PK);
				ZDBOnlySubQuery plOrderQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.PK);

				plOrderQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobOrderHeaderSchema.JD_OrderNumber, comparisonOperator, orderNo);
				orderLineSubQuery1.AddSubQuery(JobOrderLineSchema.JO_JD, plOrderQuery, JoinCondition.And);
				supplierBookingLineSubQuery.AddSubQuery(JobSupplierBookingLineSchema.JSL_JO_OrderLine, orderLineSubQuery1, JoinCondition.And);
				containerLoadListLineSubQuery.AddSubQuery(ContainerLoadListLineSchema.CLL_JSL_BookingLine, supplierBookingLineSubQuery, JoinCondition.And);
				packLineSubQuery.AddSubQuery(containerLoadListLineSubQuery, JoinCondition.And);
				packLineResult.AddSubQuery(packLineSubQuery, JoinCondition.And);

				result.AddToFilter(packLineResult, JoinCondition.Or);

				//JobShipment > JobPackLine > JobSupplierBookingLine > JobOrderLine > JobOrderHeader
				ZDBOnlyQuery sptPackLineResult = new ZDBOnlyQuery(typeof(ForwardingShipment));
				ZDBOnlySubQuery sptPackLineSubQuery = new ZDBOnlySubQuery(typeof(ForwardingPackLine), JobPackLinesSchema.JL_JS);
				ZDBOnlySubQuery sptSupplierBookingLineSubQuery = new ZDBOnlySubQuery(typeof(JobSupplierBookingLine), JobSupplierBookingLineSchema.PK);
				ZDBOnlySubQuery sptOrderLineSubQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.PK);
				ZDBOnlySubQuery sptOrderQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.PK);
				sptOrderQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobOrderHeaderSchema.JD_OrderNumber, comparisonOperator, orderNo);
				sptOrderLineSubQuery.AddSubQuery(JobOrderLineSchema.JO_JD, sptOrderQuery, JoinCondition.And);
				sptSupplierBookingLineSubQuery.AddSubQuery(JobSupplierBookingLineSchema.JSL_JO_OrderLine, sptOrderLineSubQuery, JoinCondition.And);
				sptPackLineSubQuery.AddSubQuery(JobPackLinesSchema.JL_JSL_BookingLine, sptSupplierBookingLineSubQuery, JoinCondition.And);
				sptPackLineResult.AddSubQuery(sptPackLineSubQuery, JoinCondition.And);
				result.AddToFilter(sptPackLineResult, JoinCondition.Or);

				ZDBOnlyQuery lsePackLineResult = new ZDBOnlyQuery(typeof(ForwardingShipment));
				ZDBOnlySubQuery lsePackLineSubQuery = new ZDBOnlySubQuery(typeof(ForwardingPackLine), JobPackLinesSchema.JL_JS);
				ZDBOnlySubQuery lseSupplierBookingLineSubQuery = new ZDBOnlySubQuery(typeof(JobSupplierBookingLine), JobSupplierBookingLineSchema.JSL_JL_LooseCargo);
				ZDBOnlySubQuery lseOrderLineSubQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.PK);
				ZDBOnlySubQuery lseOrderQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.PK);
				lseOrderQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobOrderHeaderSchema.JD_OrderNumber, comparisonOperator, orderNo);
				lseOrderLineSubQuery.AddSubQuery(JobOrderLineSchema.JO_JD, lseOrderQuery, JoinCondition.And);
				lseSupplierBookingLineSubQuery.AddSubQuery(JobSupplierBookingLineSchema.JSL_JO_OrderLine, lseOrderLineSubQuery, JoinCondition.And);
				lsePackLineSubQuery.AddSubQuery(JobPackLinesSchema.PK, lseSupplierBookingLineSubQuery, JoinCondition.And);
				lsePackLineResult.AddSubQuery(lsePackLineSubQuery, JoinCondition.And);
				result.AddToFilter(lsePackLineResult, JoinCondition.Or);
			}

			return result;
		}

		#endregion

		#region GetFlightVoyageNumberAndVesselQuery

		protected virtual ZQuery GetFlightVoyageNumberAndVesselQuery(SQLComparisonOperator flightOrVoyageNoComparisonOperator, ZString flightOrVoyageNo, ZString vesselNK, ZBool includeArchived)
		{
			var builder = new SailingFilterBuilder(Factory);
			builder.Vessel = vesselNK;
			builder.VoyageFlight = flightOrVoyageNo;
			builder.VoyageFlightComparisonOperator = flightOrVoyageNoComparisonOperator;
			builder.IncludeArchived = includeArchived;

			return builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.ViaConsol | SailingFilterBuilder.RelationshipFlags.ViaDirectTransports);
		}

		#endregion

		#region GetWarehouseLocationFilter

		ZQuery GetWarehouseLocationFilter(ZQuery packLocationFilter)
		{
			var whsLocationFilter = new ZDBOnlySubQuery(typeof(IWhsLocation), WhsLocationViewSchema.PK);
			whsLocationFilter.AddToFilter(packLocationFilter);

			var locationFilter = new ZDBOnlySubQuery(typeof(PackLocation), JobPackLocSchema.JQ_JL);
			locationFilter.AddSubQuery(JobPackLocSchema.JQ_WL, whsLocationFilter, JoinCondition.And);

			var packlineFilter = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.JL_JS);
			packlineFilter.AddSubQuery(locationFilter, JoinCondition.And);

			var shipmentFilter = new ZDBOnlyQuery(typeof(CommonShipment));
			shipmentFilter.AddSubQuery(packlineFilter, JoinCondition.And);

			return shipmentFilter;
		}

		#endregion

		#region HVLV Numbers Filters

		void AddHVLVNumbersFilters(ModuleFilterCollection filters)
		{
			ObjectFactory.Get<IHVLVFilterProviderForShipment>().AddHVLVFilters(filters);
		}

		#endregion

		ZQuery GetMasterLeadShipmentNumber(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ForwardingShipment));

			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				query.AddToFilter(JobShipmentSchema.JS_JS_ColoadMasterShipment, DBNull.Value);
			}
			else
			{
				ZDBOnlySubQuery subShipmentsFilter = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
				subShipmentsFilter.AddToFilter_PossiblyCommaSeparated(JobShipmentSchema.JS_UniqueConsignRef, comparisonOperator, value);
				query.AddSubQuery(JobShipmentSchema.JS_JS_ColoadMasterShipment, subShipmentsFilter, JoinCondition.And);
			}

			return query;
		}

		#region GetClientContractNoQuery

		ZQuery GetClientContractNoQuery(SQLComparisonOperator comparisonOperator, ZString clientContractNum)
		{
			var result = new ZQuery();

			var isBlankOperator = comparisonOperator == SpecialComparisonOperator.IsBlank
				   || comparisonOperator == SQLComparisonOperator.DoesNotStartWith
				   || comparisonOperator == SQLComparisonOperator.NotContains
				   || comparisonOperator == SQLComparisonOperator.NotEqual;

			var jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_ClientContractNumber, comparisonOperator, clientContractNum);
			jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_ParentTableCode, JobShipmentSchema.Constants.Prefix);
			jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

			var shipmentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
			shipmentQuery.AddSubQuery(jobHeaderSubQuery, JoinCondition.And);

			result.AddToFilter(shipmentQuery, JoinCondition.Or);

			if (isBlankOperator)
			{
				var blankOperatorJobSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID, true);

				var blankOperatorShipmentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
				blankOperatorShipmentQuery.AddSubQuery(blankOperatorJobSubQuery, JoinCondition.And);

				result.AddToFilter(blankOperatorShipmentQuery, JoinCondition.Or);
			}

			return result;
		}

		#endregion

		#endregion

		#region Text Filters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			// This should be later moved to the AccountingFilterStripCreator class in Accounting.sln. Which is the right place for JobHeader filters.
			// See method AccountingFilterStripCreator.GetBaseJobSubQuery()

			var jobHoldReasonFilter = filters.AddTextFilter("Job Status Hold Reason", JobHeaderSchema.JH_HoldReason);
			jobHoldReasonFilter.MultilingualDescription = ResString.GetMultilingualString("0898b998-afe4-44bb-8a09-77d5b2e0d361", "Job Status Hold Reason");
			jobHoldReasonFilter.SubGroup = JobHeaderSubGroup;
		}

		#endregion

		#region Date Filters

		protected virtual void AddDateFilters(ModuleFilterCollection filters)
		{
			var catageAdvisedFilter = filters.AddDateFilter("Cartage Advised", JobDocsAndCartageSchema.JP_DeliveryCartageAdvised);
			catageAdvisedFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|CartageAdvised", "Port Transport Advised");
			catageAdvisedFilter.SubGroup = JobDocsAndCartageSubGroup;

			var estDeliveryDateFilter = filters.AddDateFilter("Est Delivery Date", JobDocsAndCartageSchema.JP_EstimatedDelivery);
			estDeliveryDateFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|EstDeliveryDate", "Est Delivery Date");
			estDeliveryDateFilter.SubGroup = JobDocsAndCartageSubGroup;

			var actualDeliveryDateFilter = filters.AddDateFilter("Actual Delivery Date", JobDocsAndCartageSchema.JP_DeliveryCartageCompleted);
			actualDeliveryDateFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ActualDeliveryDate", "Actual Delivery Date");
			actualDeliveryDateFilter.SubGroup = JobDocsAndCartageSubGroup;

			filters.AddDateFilter(Descriptions.ETA, JobShipmentSchema.JS_E_ARV).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ETA", "ETA");
			filters.AddDateFilter(Descriptions.ETD, JobShipmentSchema.JS_E_DEP).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ETD", "ETD");
			filters.AddDateFilter(Descriptions.InterimReceiptDate, JobShipmentSchema.JS_A_RCV).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|InterimReceiptDate", "Interim Receipt Date");

			var firstPortOfArrivalDateFilter = filters.AddDateFilter(Descriptions.FirstPortOfArrivalDate, JobConsolSchema.JK_DatePortOfFirstArrival);
			firstPortOfArrivalDateFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|FirstPortOfArrivalDate", "First Port of Arrival Date");
			firstPortOfArrivalDateFilter.SubGroup = ConsolSubGroup;

			var goodsDeliveredFilter = filters.AddDateFilter("Goods Delivered", JobDocsAndCartageSchema.JP_DeliveryCartageCompleted);
			goodsDeliveredFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|GoodsDelivered", "Goods Delivered");
			goodsDeliveredFilter.SubGroup = JobDocsAndCartageSubGroup;

			var estPickupDateFilter = filters.AddDateFilter("Est Pickup Date", JobDocsAndCartageSchema.JP_EstimatedPickup);
			estPickupDateFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|EstPickupDate", "Est Pickup Date");
			estPickupDateFilter.SubGroup = JobDocsAndCartageSubGroup;

			var actualPickupDateFilter = filters.AddDateFilter("Actual Pickup Date", JobDocsAndCartageSchema.JP_PickupCartageCompleted);
			actualPickupDateFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ActualPickupDate", "Actual Pickup Date");
			actualPickupDateFilter.SubGroup = JobDocsAndCartageSubGroup;

			var customsEntryAuthorisationFilter = filters.AddDateFilter("Customs Entry Authorisation", JobDeclarationSchema.JE_EntryAuthorisationDate);
			customsEntryAuthorisationFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|CustomsEntryAuthorisation", "Customs Entry Authorization");
			customsEntryAuthorisationFilter.SubGroup = DeclarationSubGroup;

			filters.AddDateFilter("House Bill Issue Date", GetHouseBillIssueDateQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|HouseBillIssueDate", "House Bill Issue Date");

			var cfsResourceString = Res.GetData("0b5af3f3-9042-4760-8de7-7b4b66b1f444", "CFS");

			var pickupCFSReceiptRequested = new DateOrganizationFilter(Descriptions.PickupCFSReceiptRequested, JobShipmentSchema.JS_ExportReceivingDepotReceiptRequested, GetOrgAddressColumnQueryWithOperatorDelegate(JobShipmentSchema.JS_OA_ExportReceivingDepot), BindingLists.PackDepot_List, cfsResourceString);
			pickupCFSReceiptRequested.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|PickupCFSReceiptRequested", "Pickup CFS/TW / Receipt Requested Date");
			filters.AddCustomFilter(pickupCFSReceiptRequested);

			var deliveryCFSReceiptRequested = new DateOrganizationFilter(Descriptions.DeliveryCFSReceiptRequested, JobShipmentSchema.JS_ImportReleaseDepotReceiptRequested, GetOrgAddressColumnQueryWithOperatorDelegate(JobShipmentSchema.JS_OA_ImportReleaseDepot), BindingLists.UnpackDepot_List, cfsResourceString);
			deliveryCFSReceiptRequested.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|DeliveryCFSReceiptRequested", "Delivery CFS/TW / Receipt Requested Date");
			filters.AddCustomFilter(deliveryCFSReceiptRequested);

			var pickupCFSDispatchRequested = new DateOrganizationFilter(Descriptions.PickupCFSDispatchRequested, JobShipmentSchema.JS_ExportReceivingDepotDispatchRequested, GetOrgAddressColumnQueryWithOperatorDelegate(JobShipmentSchema.JS_OA_ExportReceivingDepot), BindingLists.PackDepot_List, cfsResourceString);
			pickupCFSDispatchRequested.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|PickupCFSDispatchRequested", "Pickup CFS/TW / Dispatch Requested Date");
			filters.AddCustomFilter(pickupCFSDispatchRequested);

			var deliveryCFSDispatchRequested = new DateOrganizationFilter(Descriptions.DeliveryCFSDispatchRequested, JobShipmentSchema.JS_ImportReleaseDepotDispatchRequested, GetOrgAddressColumnQueryWithOperatorDelegate(JobShipmentSchema.JS_OA_ImportReleaseDepot), BindingLists.UnpackDepot_List, cfsResourceString);
			deliveryCFSDispatchRequested.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|DeliveryCFSDispatchRequested", "Delivery CFS/TW / Dispatch Requested Date");
			filters.AddCustomFilter(deliveryCFSDispatchRequested);

			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive)
			{
				var deliveryDueDateFilter = filters.AddDateFilter(Descriptions.DeliveryDueDate, JobShipmentSchema.JS_DeliveryDueDate);
				deliveryDueDateFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|DeliveryDueDate", "Delivery Due Date");

				var revisedDeliveryDueDateFilter = filters.AddDateFilter(Descriptions.RevisedDeliveryDueDate, JobShipmentSchema.JS_RevisedDeliveryDueDate);
				revisedDeliveryDueDateFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|RevisedDeliveryDueDate", "Revised Delivery Due Date");
			}

			if (!Globals.IsWeb)
			{
				var isLoadETA = SailingFilterBuilder.Dates.LoadETA;
				etaLoadFilter = new DateLocationFilter(Descriptions.ETALoad, isLoadETA, BindingLists.RefLocation_List, DateLocationFilter.LocationTypes.Load, DateLocationFilter.TargetFilterTypes.Shipment, Factory);
				etaLoadFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ETALoad", "ETA / Load Port");
				filters.AddCustomFilter(etaLoadFilter);

				ataLoadFilter = new DateLocationFilter(Descriptions.ATALoad, SailingFilterBuilder.Dates.LoadATA, BindingLists.RefLocation_List, DateLocationFilter.LocationTypes.Load, DateLocationFilter.TargetFilterTypes.Shipment, Factory);
				ataLoadFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ATALoad", "ATA / Load Port");
				filters.AddCustomFilter(ataLoadFilter);

				etdLoadFilter = new DateLocationFilter(Descriptions.ETDLoad, SailingFilterBuilder.Dates.ETD, BindingLists.RefLocation_List, DateLocationFilter.LocationTypes.Load, DateLocationFilter.TargetFilterTypes.Shipment, Factory);
				etdLoadFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ETDLoad", "ETD / Load Port");
				filters.AddCustomFilter(etdLoadFilter);

				atdLoadFilter = new DateLocationFilter(Descriptions.ATDLoad, SailingFilterBuilder.Dates.ATD, BindingLists.RefLocation_List, DateLocationFilter.LocationTypes.Load, DateLocationFilter.TargetFilterTypes.Shipment, Factory);
				atdLoadFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ATDLoad", "ATD / Load Port");
				filters.AddCustomFilter(atdLoadFilter);

				etaDischargeFilter = new DateLocationFilter(Descriptions.ETADischarge, SailingFilterBuilder.Dates.ETA, BindingLists.RefLocation_List, DateLocationFilter.LocationTypes.Discharge, DateLocationFilter.TargetFilterTypes.Shipment, Factory);
				etaDischargeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ETADischarge", "ETA / Discharge Port");
				filters.AddCustomFilter(etaDischargeFilter);

				ataDischargeFilter = new DateLocationFilter(Descriptions.ATADischarge, SailingFilterBuilder.Dates.ATA, BindingLists.RefLocation_List, DateLocationFilter.LocationTypes.Discharge, DateLocationFilter.TargetFilterTypes.Shipment, Factory);
				ataDischargeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ATADischarge", "ATA / Discharge Port");
				filters.AddCustomFilter(ataDischargeFilter);

				AddPortValidationForDateLocationFilter();
			}
		}

		#region Port Validation For Date Location Filters

		void AddPortValidationForDateLocationFilter()
		{
			Validation portValidation = info => PortFilterSecurityValidator.ValidatePort((DateLocationFilter)info.BizObj);

			etaLoadFilter.Property3Validation = portValidation;
			ataLoadFilter.Property3Validation = portValidation;
			etdLoadFilter.Property3Validation = portValidation;
			atdLoadFilter.Property3Validation = portValidation;
			etaDischargeFilter.Property3Validation = portValidation;
			ataDischargeFilter.Property3Validation = portValidation;
		}

		DateLocationFilter etaLoadFilter;
		DateLocationFilter ataLoadFilter;
		DateLocationFilter etdLoadFilter;
		DateLocationFilter atdLoadFilter;
		DateLocationFilter etaDischargeFilter;
		DateLocationFilter ataDischargeFilter;

		#endregion

		#region GetHouseBillIssueDateQuery

		ZQuery GetHouseBillIssueDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				result.AddSubQuery(GetNoIssueDateSubQuery(), JoinCondition.And);
			}
			else
			{
				result.AddToFilter(GetShipmentIssueDateQuery(comparisonOperator, date1, date2), JoinCondition.And);
				result.AddToFilter(GetHAWBQuery(comparisonOperator, date1, date2), JoinCondition.Or);
			}

			return result;
		}

		ZDBOnlySubQuery GetNoIssueDateSubQuery()
		{
			var result = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			result.AddToFilter(JobShipmentSchema.JS_HouseBillIssueDate, DBNull.Value);

			var hawbSubQuery = new ZDBOnlySubQuery(typeof(ExportAWBHeader), ExportAWBHeaderSchema.EH_ParentID);
			hawbSubQuery.AddToFilter(ExportAWBHeaderSchema.EH_AWBIssueDate, SQLComparisonOperator.NotEqual, DBNull.Value);

			var shipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK, true);
			shipmentSubQuery.AddSubQuery(hawbSubQuery, JoinCondition.And);
			result.AddSubQuery(shipmentSubQuery, JoinCondition.And);

			return result;
		}

		ZDBOnlyQuery GetShipmentIssueDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			AddDateRange(result, comparisonOperator, JoinCondition.And, JobShipmentSchema.JS_HouseBillIssueDate, date1.Date, date2.Date);

			return result;
		}

		ZDBOnlyQuery GetHAWBQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			result.AddToFilter(JobShipmentSchema.JS_HouseBillIssueDate, DBNull.Value);

			var hawbSubQuery = new ZDBOnlySubQuery(typeof(ExportAWBHeader), ExportAWBHeaderSchema.EH_ParentID);
			AddDateRange(hawbSubQuery, comparisonOperator, JoinCondition.And, ExportAWBHeaderSchema.EH_AWBIssueDate, date1.Date, date2.Date);
			result.AddSubQuery(hawbSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#endregion

		#region Organisation Filters

		protected virtual void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var carrierFilter = new ModuleGuidFilterForOrg((NoResString)"Carrier", ModuleIDs.Organisation, OrgAddressSchema.OA_OH, BindingLists.ShippingProvider_FilterList);
			carrierFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|Carrier", "Carrier");
			carrierFilter.SupportsBlankComparisonOperators = false;
			carrierFilter.SubGroup = CarrierSubGroup;
			filters.AddFilter(carrierFilter);

			var pickupCFSFilter = new ModuleGuidFilterForOrg("PickupCFS", ModuleIDs.Organisation, GetOrgAddressColumnQueryWithOperatorDelegate(JobShipmentSchema.JS_OA_ExportReceivingDepot), BindingLists.PackDepot_FilterList);
			pickupCFSFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|PickupCFS", "Pickup CFS");
			pickupCFSFilter.SupportsBlankComparisonOperators = true;
			filters.AddFilter(pickupCFSFilter);

			var deliveryCFSFilter = new ModuleGuidFilterForOrg("DeliveryCFS", ModuleIDs.Organisation, GetOrgAddressColumnQueryWithOperatorDelegate(JobShipmentSchema.JS_OA_ImportReleaseDepot), BindingLists.UnpackDepot_FilterList);
			deliveryCFSFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|DeliveryCFS", "Delivery CFS");
			deliveryCFSFilter.SupportsBlankComparisonOperators = true;
			filters.AddFilter(deliveryCFSFilter);

			var consignorTerminology = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value.IsEmpty ? (ZString)FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue : ((ZString)FreightDataRegistry.Instance.ConsignorShipperTerminology.Value).SubstringSafe(0, 15);
			var consignorConsigneeFilter = new ModuleGuidsFilterForOrg(Descriptions.ConsignorConsignee, ModuleIDs.Organisation, GetConsignorConsigneeQuery, BindingLists.OrgConsignor_FilterList, BindingLists.OrgConsignee_FilterList);
			consignorConsigneeFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.ConsignorDocumentaryAddress, ShipmentXQueryPaths.ConsigneeDocumentaryAddress, OrgHeaderSchema.OH_Code.MaxLength, OrgHeaderSchema.OH_Code.MaxLength);
			consignorConsigneeFilter.SetItemDescriptions(new ResourceStringData("", consignorTerminology), Res.GetData("Forwarding|JobShipmentFilter|Consignee", "Consignee"));
			consignorConsigneeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ConsignorConsignee", "{0} / Consignee", FreightDataRegistry.Instance.ConsignorShipperTerminology.Value);
			filters.AddFilter(consignorConsigneeFilter);

			var consolAgentsFilter = new ModuleGuidsFilterForOrg(Descriptions.ConsolSendReceiveAgents, ModuleIDs.Organisation, GetConsolSendingReceivingAgentQuery, BindingLists.OrgForwarder_FilterList, BindingLists.OrgForwarder_FilterList);
			consolAgentsFilter.SetItemDescriptions(Res.GetData("Forwarding|JobShipmentFilter|SendAgent", "Send Agent"), Res.GetData("Forwarding|JobShipmentFilter|RecAgent", "Rec. Agent"));
			consolAgentsFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ConsolSendReceiveAgents", "Consol Send / Receive Agents");
			filters.AddFilter(consolAgentsFilter);

			var deliveryAgentFilter = new ModuleGuidFilterForOrg(Descriptions.DeliveryAgent, ModuleIDs.Organisation, JobShipmentSchema.JS_OH_DeliveryAgent, BindingLists.OrgForwarder_FilterList);
			deliveryAgentFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.DeliveryAgent, OrgHeaderSchema.OH_Code.MaxLength);
			deliveryAgentFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|DeliveryAgent", "Delivery Agent");
			deliveryAgentFilter.SupportsFiltersMatchComparisonOperator = false;
			filters.AddFilter(deliveryAgentFilter);

			var pickupAgentFilter = new ModuleGuidFilterForOrg(Descriptions.PickupAgent, ModuleIDs.Organisation, GetDocAddressQueryWithOperatorDelegate(DocAddressType.PickupAgent), BindingLists.OrgForwarder_FilterList);
			pickupAgentFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.PickupAgent, OrgHeaderSchema.OH_Code.MaxLength);
			pickupAgentFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|PickupAgent", "Pickup Agent");
			pickupAgentFilter.SupportsBlankComparisonOperators = true;
			pickupAgentFilter.MultiValueQueryDelegate = PickupAgentMultiValueQuery;
			filters.AddFilter(pickupAgentFilter);

			var transhipmentAgent = new ModuleGuidFilterForOrg(Descriptions.TranshipmentAgent, ModuleIDs.Organisation, JobShipmentSchema.JS_OH_TranshipAgent, BindingLists.OrgForwarder_FilterList);
			transhipmentAgent.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|TranshipmentAgent", "Transhipment Agent");
			filters.AddFilter(transhipmentAgent);

			var exportBrokerFilter = new ModuleGuidFilterForOrg(Descriptions.ExportBroker, ModuleIDs.Organisation, JobShipmentSchema.JS_OH_ExportBroker, BindingLists.OrgMiscServBroker_FilterList);
			exportBrokerFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.ExportBroker, OrgHeaderSchema.OH_Code.MaxLength);
			exportBrokerFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ExportBroker", "Export Broker");
			exportBrokerFilter.SupportsBlankComparisonOperators = true;
			exportBrokerFilter.SupportsFiltersMatchComparisonOperator = false;
			filters.AddFilter(exportBrokerFilter);

			var pickupTransportCompany = new ModuleGuidFilterForOrg(Descriptions.PickupTransportCompany, ModuleIDs.Organisation, GetCartageCompanyQueryComparisonDelegate(JobDocsAndCartageSchema.JP_OA_PickupCartageCoAddr), BindingLists.ShippingProvider_FilterList);
			pickupTransportCompany.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|PickupTransportCompany", "Pickup Transport Company");
			pickupTransportCompany.SupportsBlankComparisonOperators = true;
			pickupTransportCompany.MultiValueQueryDelegate = PickupTransportCompanyMultiValueQuery;
			filters.AddFilter(pickupTransportCompany);

			var deliveryTransportCompany = new ModuleGuidFilterForOrg(Descriptions.DeliveryTransportCompany, ModuleIDs.Organisation, GetCartageCompanyQueryComparisonDelegate(JobDocsAndCartageSchema.JP_OA_DeliveryCartageCoAddr), BindingLists.ShippingProvider_FilterList);
			deliveryTransportCompany.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|DeliveryTransportCompany", "Delivery Transport Company");
			deliveryTransportCompany.SupportsBlankComparisonOperators = true;
			deliveryTransportCompany.MultiValueQueryDelegate = DeliveryTransportCompanyMultiValueQuery;
			filters.AddFilter(deliveryTransportCompany);

			var localClientFilter = new ModuleGuidFilterForOrg(Descriptions.LocalClientBilling, ModuleIDs.Organisation, GetLocalClientFilter, BindingLists.OrgHeader_List);
			localClientFilter.Category = FilterCategories.Organisations;
			localClientFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|LocalClientBilling", "Local Client (Billing)");
			localClientFilter.SupportsBlankComparisonOperators = true;
			localClientFilter.MultiValueQueryDelegate = LocalClientMultiValueQuery;
			filters.AddFilter(localClientFilter);

			var overseasAgentFilter = filters.AddGuidFilter("Overseas Agent (Billing)", ModuleIDs.Organisation, OrgAddressSchema.OA_OH, BindingLists.OrgHeader_List);
			overseasAgentFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|OverseasAgentBilling", "Overseas Agent (Billing)");
			overseasAgentFilter.SupportsBlankComparisonOperators = false;
			overseasAgentFilter.SubGroup = OverseasAgentSubGroup;

			var importBrokerFilter = new ModuleGuidFilterForOrg(Descriptions.ImportBroker, ModuleIDs.Organisation, JobShipmentSchema.JS_OH_ImportBroker, BindingLists.OrgMiscServBroker_FilterList);
			importBrokerFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.ImportBroker, OrgHeaderSchema.OH_Code.MaxLength);
			importBrokerFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ImportBroker", "Import Broker");
			importBrokerFilter.SupportsBlankComparisonOperators = true;
			importBrokerFilter.SupportsFiltersMatchComparisonOperator = false;
			filters.AddFilter(importBrokerFilter);

			var shipmentAgentsFilter = new ModuleGuidsFilterForOrg(Descriptions.ShipmentSendReceiveForwarders, ModuleIDs.Organisation, GetShipmentSendingReceivingAgentQuery, BindingLists.OrgForwarder_FilterList, BindingLists.OrgForwarder_FilterList);
			shipmentAgentsFilter.SetItemDescriptions(Res.GetData("Forwarding|JobShipmentFilter|SendForwarder", "Send Forwarder"), Res.GetData("Forwarding|JobShipmentFilter|RecForwarder", "Rec. Forwarder"));
			shipmentAgentsFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ShipmentSendReceiveForwarders", "Shipment Send / Receive Forwarders");
			filters.AddFilter(shipmentAgentsFilter);

			var companyNameFilter = filters.AddTextFilter("Company Name", GetCompanyNameQuery)
				.WithMaxLengthOf<ModuleTextFilter>(OrgHeaderSchema.OH_FullName);
			companyNameFilter.Category = FilterCategories.Organisations;
			companyNameFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|CompanyName", "Company Name");

			var consignorCompanyNameFilter = filters.AddTextFilter(FreightDataRegistry.Instance.ConsignorShipperTerminology.Value.GetUnresolvedString() + " Company Name", GetConsignorCompanyNameQuery)
				.WithMaxLengthOf<ModuleTextFilter>(OrgHeaderSchema.OH_FullName);
			consignorCompanyNameFilter.Category = FilterCategories.Organisations;
			consignorCompanyNameFilter.SupportsBlankComparisonOperators = false;
			consignorCompanyNameFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ConsignorCompanyName", "{0} Company Name", FreightDataRegistry.Instance.ConsignorShipperTerminology.Value);

			var consigneeCompanyNameFilter = filters.AddTextFilter("Consignee Company Name", GetConsigneeCompanyNameQuery)
				.WithMaxLengthOf<ModuleTextFilter>(OrgHeaderSchema.OH_FullName);
			consigneeCompanyNameFilter.SupportsBlankComparisonOperators = false;
			consigneeCompanyNameFilter.Category = FilterCategories.Organisations;
			consigneeCompanyNameFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ConsigneeCompanyName", "Consignee Company Name");

			var notifyPartyCompanyNameFilter = filters.AddTextFilter("Notify Party Company Name", GetNotifyPartyCompanyNameQuery)
				.WithMaxLengthOf<ModuleTextFilter>(OrgHeaderSchema.OH_FullName);
			notifyPartyCompanyNameFilter.Category = FilterCategories.Organisations;
			notifyPartyCompanyNameFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|NotifyPartyCompanyName", "Notify Party Company Name");

			var controllingAgentFilter = filters.AddGuidFilter("Controlling Agent", ModuleIDs.Organisation, GetDocAddressQueryWithOperatorDelegate(DocAddressType.ControllingAgent), BindingLists.OrgHeader_List);
			controllingAgentFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.ControllingAgent, OrgHeaderSchema.OH_Code.MaxLength);
			controllingAgentFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ControllingAgent", "Controlling Agent");
			controllingAgentFilter.SupportsBlankComparisonOperators = true;
			controllingAgentFilter.MultiValueQueryDelegate = ControllingAgentMultiValueQuery;

			var controllingCustomerFilter = filters.AddGuidFilter("Controlling Customer", ModuleIDs.Organisation, GetDocAddressQueryWithOperatorDelegate(DocAddressType.ControllingCustomer), BindingLists.OrgHeader_List);
			controllingCustomerFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.ControllingCustomer, OrgHeaderSchema.OH_Code.MaxLength);
			controllingCustomerFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ControllingCustomer", "Controlling Customer");
			controllingCustomerFilter.SupportsBlankComparisonOperators = true;
			controllingCustomerFilter.MultiValueQueryDelegate = ControllingCustomerMultiValueQuery;

			ModuleNkFilter cartageCoordFilter = filters.AddNkFilter("Cartage Coordinator", GetCartageCoordinatorQuery, ModuleIDs.GlbStaff, JS_StaffFilter_List);
			//cartageCoordFilter.SetItemDescription = Res.GetString("Forwarding|JobShipmentFilter|CartageCoord", "Local Transport Coord.");
			cartageCoordFilter.Category = FilterCategories.Organisations;
			cartageCoordFilter.IsPublishedOnWeb = false;
			cartageCoordFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|CartageCoord", "Cartage Coordinator");

			ModuleNkFilter salesRepFilter = filters.AddNkFilter("Sales Rep", GetSalesRepQuery, ModuleIDs.GlbStaff, JS_StaffFilter_List);
			//salesRepFilter.SetItemDescription = Res.GetString("Forwarding|JobShipmentFilter|SalesRep", "Sales Rep");;
			salesRepFilter.Category = FilterCategories.Organisations;
			salesRepFilter.IsPublishedOnWeb = false;
			salesRepFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|SalesRep", "Sales Rep");

			var customsBrokerFilter = filters.AddNkFilter("Customs Broker", GetCustomsBrokerQuery, ModuleIDs.GlbStaff, JS_StaffFilter_List);
			customsBrokerFilter.Category = FilterCategories.Organisations;
			customsBrokerFilter.IsPublishedOnWeb = false;
			customsBrokerFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|CustomsBroker", "Customs Broker");

			ModuleGuidFilter branchFilter = filters.AddGuidFilter("Branch (Current Co.)", ModuleIDs.GlbBranch, JobHeaderSchema.JH_GB, BindingLists.Branches);
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.IsPublishedOnWeb = false;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|BranchCurrentCo", "Branch (Current Co.)");
			branchFilter.SubGroup = BranchSubGroup;

			ModuleFilter consignorRelatedPartiesFilter = new OrgRelatedPartiesModuleFilter(Descriptions.ConsignorRelatedParties, GetConsignorRelatedPartiesQuery);
			consignorRelatedPartiesFilter.Category = FilterCategories.Organisations;
			consignorRelatedPartiesFilter.MultilingualDescription = ResString.GetMultilingualString("5FD156C9-665C-4B17-BF4E-5AA01EE7E98F", "{0} Related Parties", FreightDataRegistry.Instance.ConsignorShipperTerminology.Value);
			filters.AddCustomFilter(consignorRelatedPartiesFilter);

			ModuleFilter consigneeRelatedPartiesFilter = new OrgRelatedPartiesModuleFilter(Descriptions.ConsigneeRelatedParties, GetConsigneeRelatedPartiesQuery);
			consigneeRelatedPartiesFilter.Category = FilterCategories.Organisations;
			consigneeRelatedPartiesFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ConsigneeRelatedParties", "Consignee Related Parties");
			filters.AddCustomFilter(consigneeRelatedPartiesFilter);

			ModuleFilter localClientRelatedPartiesFilter = new OrgRelatedPartiesModuleFilter(Descriptions.LocalClientRelatedParties, GetLocalClientRelatedPartiesQuery);
			localClientRelatedPartiesFilter.Category = FilterCategories.Organisations;
			localClientRelatedPartiesFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|LocalClientRelatedParties", "Local Client Related Parties");
			filters.AddCustomFilter(localClientRelatedPartiesFilter);

			OrgClientAssignedStaffModuleFilter clientAssignedStaffFilter = new OrgClientAssignedStaffModuleFilter(Descriptions.ClientAssignedStaff, GetClientAssignedStaffQuery);
			CustomizeClientAssignedStaffFilter(clientAssignedStaffFilter);
			clientAssignedStaffFilter.Category = FilterCategories.Organisations;
			clientAssignedStaffFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ClientAssignedStaff", "Client Assigned Staff");
			filters.AddCustomFilter(clientAssignedStaffFilter);

			var consignorFilter = new ModuleGuidFilterForOrg(Descriptions.Consignor, ModuleIDs.Organisation, GetDocAddressQueryWithOperatorDelegate(DocAddressType.ConsignorDocumentaryAddress), BindingLists.OrgConsignor_FilterList);
			consignorFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.ConsignorDocumentaryAddress, OrgHeaderSchema.OH_Code.MaxLength);
			consignorFilter.Category = FilterCategories.Organisations;
			consignorFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|Consignor", "Consignor");
			consignorFilter.SupportsBlankComparisonOperators = true;
			consignorFilter.MultiValueQueryDelegate = ConsignorMultiValueQuery;
			filters.AddFilter(consignorFilter);

			var consigneeFilter = new ModuleGuidFilterForOrg(Descriptions.Consignee, ModuleIDs.Organisation, GetDocAddressQueryWithOperatorDelegate(DocAddressType.ConsigneeDocumentaryAddress), BindingLists.OrgConsignee_FilterList);
			consigneeFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.ConsigneeDocumentaryAddress, OrgHeaderSchema.OH_Code.MaxLength);
			consigneeFilter.Category = FilterCategories.Organisations;
			consigneeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|Consignee", "Consignee");
			consigneeFilter.SupportsBlankComparisonOperators = true;
			consigneeFilter.MultiValueQueryDelegate = ConsigneeMultiValueQuery;
			filters.AddFilter(consigneeFilter);

			var orderBuyerFilter = new ModuleGuidFilterForOrg(Descriptions.Buyer, ModuleIDs.Organisation, GetOrderBuyerFilter, BindingLists.OrgBuyer_FilterList);
			orderBuyerFilter.Category = FilterCategories.Organisations;
			orderBuyerFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|OrderBuyer", "Order - Buyer");
			filters.AddFilter(orderBuyerFilter);

			var orderSupplierFilter = new ModuleGuidFilterForOrg(Descriptions.Supplier, ModuleIDs.Organisation, GetOrderSupplierFilter, BindingLists.OrgSupplier_FilterList);
			orderSupplierFilter.Category = FilterCategories.Organisations;
			orderSupplierFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|OrderSupplier", "Order - Supplier");
			filters.AddFilter(orderSupplierFilter);

			var shipmentGatewayFilter = new ModuleGuidFilterForOrg(Descriptions.Gateway, ModuleIDs.Organisation, GetShipmentGatewayFilter, BindingLists.OrgForwarder_FilterList);
			shipmentGatewayFilter.Category = FilterCategories.Organisations;
			shipmentGatewayFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|Gateway", "Gateway");
			shipmentGatewayFilter.SupportsBlankComparisonOperators = true;
			filters.AddFilter(shipmentGatewayFilter);
		}

		#region GetOrderBuyerFilter, GetOrderSupplierFilter

		ZQuery GetOrderBuyerFilter(ZGuid orgPK)
		{
			var result = new ZQuery();

			result.AddToFilter(GetCompanyOrdersQuery(orgPK, JobOrderHeaderSchema.JD_OA_BuyerAddress));

			return result;
		}

		ZQuery GetOrderSupplierFilter(ZGuid orgPK)
		{
			var result = new ZQuery();

			result.AddToFilter(GetCompanyOrdersQuery(orgPK, JobOrderHeaderSchema.JD_OA_SupplierAddress));

			return result;
		}

		ZQuery GetCompanyOrdersQuery(ZGuid orgPK, SchemaGuidColumn orderAddressPKSchemaColumn)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			var orderSubQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.JD_JS);
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), orderAddressPKSchemaColumn);
			var orgQuery = new ZQuery(OrgAddressSchema.OA_OH, orgPK);

			orgAddressSubQuery.AddToFilter(orgQuery);
			orderSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

			result.AddSubQuery(orderSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region Gateway

		ZQuery GetShipmentGatewayFilter(SQLComparisonOperator comparisonOperator, object pK)
		{
			var notIn = comparisonOperator == SQLComparisonOperator.IsBlank;
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			var gatewaySubQuery = new ZDBOnlySubQuery(typeof(ShipmentGateway), JobShipmentGatewaySchema.JSG_JS_Shipment, notIn);
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobShipmentGatewaySchema.JSG_OA_ForwarderAddress);

			if (comparisonOperator != SQLComparisonOperator.IsBlank && comparisonOperator != SQLComparisonOperator.IsNotBlank)
			{
				var orgQuery = new ZQuery(OrgAddressSchema.OA_OH, comparisonOperator, (ZGuid)pK);
				orgAddressSubQuery.AddToFilter(orgQuery);
			}

			gatewaySubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

			result.AddSubQuery(gatewaySubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region Local Client

		ZQuery GetLocalClientFilter(SQLComparisonOperator comparisonOperator, object pK)
		{
			var notIn = comparisonOperator == SQLComparisonOperator.IsBlank;
			var query = new ZDBOnlyQuery(typeof(ForwardingShipment));
			var subQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID, notIn);
			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr);

			if (comparisonOperator != SQLComparisonOperator.IsBlank && comparisonOperator != SQLComparisonOperator.IsNotBlank)
			{
				var orgQuery = new ZQuery(OrgAddressSchema.OA_OH, comparisonOperator, (ZGuid)pK);
				orgAddressQuery.AddToFilter(orgQuery);
			}

			subQuery.AddSubQuery(orgAddressQuery, JoinCondition.And);
			subQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		ZQuery LocalClientMultiValueQuery(object value, SQLComparisonOperator filterOperator)
		{
			var result = new ZDBOnlyQuery(typeof(CommonShipment));

			if (value is List<ZGuid> orgList && orgList.Count > 0)
			{
				var jobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);

				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				orgAddressQuery.AddToFilter(JoinCondition.Or, OrgAddressSchema.OA_OH, filterOperator, orgList);
				jobHeaderQuery.AddSubQuery(JobHeaderSchema.JH_OA_LocalChargesAddr, orgAddressQuery, JoinCondition.And);
				jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

				result.AddSubQuery(jobHeaderQuery, JoinCondition.And);
			}

			return result;
		}

		#endregion

		#region GetConsignorConsigneeQuery

		ZQuery GetConsignorConsigneeQuery(ZGuid consignorPK, ZGuid consigneePK)
		{
			ZQuery result = new ZQuery();

			if (consignorPK.IsValid)
			{
				result.AddToFilter(GetDocAddressQuery(DocAddressType.ConsignorDocumentaryAddress, consignorPK));
			}
			if (consigneePK.IsValid)
			{
				result.AddToFilter(GetDocAddressQuery(DocAddressType.ConsigneeDocumentaryAddress, consigneePK));
			}

			return result;
		}

		#endregion

		#region GetConsolSendingReceivingAgentQuery

		ZQuery GetConsolSendingReceivingAgentQuery(ZGuid sendingAgentPK, ZGuid receivingAgentPK)
		{
			ZQuery result = new ZQuery();

			if (sendingAgentPK.IsValid)
			{
				result.AddToFilter(GetConsolProxyQuery(JobConsolSchema.JK_OA_SendingForwarderAddress, sendingAgentPK));
			}
			if (receivingAgentPK.IsValid)
			{
				result.AddToFilter(GetConsolProxyQuery(JobConsolSchema.JK_OA_ReceivingForwarderAddress, receivingAgentPK));
			}

			return result;
		}

		ZDBOnlySubQuery GetConsolSubQuery(ZGuid orgPK, SchemaColumn consolAddressForeignKeyColumn)
		{
			ZDBOnlySubQuery result = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConShipLinkSchema.JN_JK);
			ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), consolAddressForeignKeyColumn);
			orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, orgPK);
			result.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
			return result;
		}

		ZDBOnlyQuery GetConsolProxyQuery(SchemaGuidColumn column, ZGuid value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			pivotSubQuery.AddSubQuery(GetConsolSubQuery(value, column), JoinCondition.And);
			result.AddSubQuery(pivotSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region GetShipmentSendingReceivingAgentQuery

		ZQuery GetShipmentSendingReceivingAgentQuery(ZGuid sendingAgentPK, ZGuid receivingAgentPK)
		{
			ZQuery result = new ZQuery(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.Equal, Constants.ShipmentTypes.CoLoadMaster);
			if (sendingAgentPK.IsValid)
			{
				result.AddToFilter(GetDocAddressQuery(DocAddressType.ConsignorDocumentaryAddress, sendingAgentPK));
			}
			if (receivingAgentPK.IsValid)
			{
				result.AddToFilter(GetDocAddressQuery(DocAddressType.ConsigneeDocumentaryAddress, receivingAgentPK));
			}
			return result;
		}

		#endregion

		#region GetShipmentDocAddressQuery(helper)

		ZDBOnlyQuery GetDocAddressQuery(DocAddressType addressType, ZGuid orgPK)
		{
			var orgQuery = new ZQuery(OrgAddressSchema.OA_OH, orgPK);
			return GetDocAddressQuery(Factory, addressType, orgQuery);
		}

		GetGuidQueryWithOperator GetDocAddressQueryWithOperatorDelegate(DocAddressType addressType)
		{
			return (SQLComparisonOperator comparisonOperator, object pK) => GetDocAddressQueryWithOperator(Factory, pK, addressType, comparisonOperator);
		}

		GetGuidQueryWithOperator GetOrgAddressColumnQueryWithOperatorDelegate(SchemaColumn orgAddressColumn)
		{
			return (SQLComparisonOperator comparisonOperator, object pK) => GetOrgAddressColumnQueryWithOperator(pK, orgAddressColumn, comparisonOperator);
		}

		ZQuery GetDocAddressQueryWithOperator(BusinessObjectFactory factory, object pK, DocAddressType addressType, SQLComparisonOperator comparisonOperator)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			var notIn = comparisonOperator == SQLComparisonOperator.IsBlank;
			var docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn);
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

			docAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(factory, addressType));
			if (comparisonOperator != SQLComparisonOperator.IsBlank && comparisonOperator != SQLComparisonOperator.IsNotBlank)
			{
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, comparisonOperator, (ZGuid)pK);
			}
			docAddressSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
			result.AddSubQuery(docAddressSubQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetOrgAddressColumnQueryWithOperator(object pK, SchemaColumn orgAddressColumn, SQLComparisonOperator comparisonOperator)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
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

		ZDBOnlyQuery GetDocAddressQuery(BusinessObjectFactory factory, DocAddressType addressType, ZQuery addressSubQuery)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			var docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

			orgAddressSubQuery.AddToFilter(addressSubQuery);
			docAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(factory, addressType));
			docAddressSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

			result.AddSubQuery(docAddressSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region GetQuoteNoQuery

		ZQuery GetQuoteNoQuery(SQLComparisonOperator comparisonOperator, ZString quoteNum)
		{
			//TODO: Needs to work with multiple values.
			var query = new ZDBOnlyQuery(typeof(CommonShipment));

			var sqlOperator = comparisonOperator.ComparisonText("");
			quoteNum = comparisonOperator.ValueForLiteralADO(quoteNum).ToString();

			var sql = CommonShipment.Schema.PK + " IN " +
				 "((SELECT " + JobHeader.Schema.JH_ParentID + " FROM " + JobHeaderSchema.Constants.SqlSchemaName + "." + JobHeaderSchema.Constants.TableName +
				 " WHERE " + JobHeader.Schema.JH_TH_NKQuoteNumber + " " + sqlOperator + " @JH_TH_NKQuoteNumber " +
				 (!quoteNum.IsEmpty ? " AND " + JobHeader.Schema.JH_TH_NKQuoteNumber + " <> ''" : "") + // Part of SQL
				 " AND " + JobHeader.Schema.JH_ParentTableCode + " = @JH_ParentTableCode" +
				 " AND " + JobHeader.Schema.JH_GC + " = @JH_GC)" +
				 " UNION (SELECT " + ViewQuotedBookingSchema.Constants.VB_JS +
				 " FROM " + ViewQuotedBookingSchema.Constants.SqlSchemaName + "." + ViewQuotedBookingSchema.Constants.TableName +
				 " WHERE " + ViewQuotedBookingSchema.Constants.VB_QuoteNumber + " " + sqlOperator + " @VB_QuoteNumber" +
				 " AND " + ViewQuotedBookingSchema.Constants.VB_GC + " = @VB_GC))";

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@JH_TH_NKQuoteNumber", quoteNum, JobHeaderSchema.JH_TH_NKQuoteNumber);
			parameters.Add("@JH_ParentTableCode", JobShipmentSchema.Constants.Prefix, JobHeaderSchema.JH_ParentTableCode);
			parameters.Add("@JH_GC", GlbCompany.CurrentCompany.PK, JobHeaderSchema.JH_GC);
			parameters.Add("@VB_QuoteNumber", quoteNum, ViewQuotedBookingSchema.VB_QuoteNumber);
			parameters.Add("@VB_GC", GlbCompany.CurrentCompany.PK, ViewQuotedBookingSchema.VB_GC);
			query.AddFilterAndZSQLParameterCollection(sql, parameters);

			return query;
		}

		#endregion

		#region GetCartageCompanyQueryComparisonDelegate

		GetGuidQueryWithOperator GetCartageCompanyQueryComparisonDelegate(SchemaColumn addressColumn)
		{
			return (SQLComparisonOperator comparisonOperator, object pK) =>
			{
				var notIn = comparisonOperator == SpecialComparisonOperator.IsBlank;
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));
				ZDBOnlySubQuery jobDocsAndCartageQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID, notIn);

				ZDBOnlySubQuery addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), addressColumn);
				if (comparisonOperator != SpecialComparisonOperator.IsBlank && comparisonOperator != SpecialComparisonOperator.IsNotBlank)
				{
					addressQuery.AddToFilter(OrgAddressSchema.OA_OH, comparisonOperator, (ZGuid)pK);
				}

				jobDocsAndCartageQuery.AddSubQuery(addressQuery, JoinCondition.And);
				result.AddSubQuery(jobDocsAndCartageQuery, JoinCondition.And);
				return result;
			};
		}

		#region CartageCompany Multi Value Queries

		ZQuery PickupTransportCompanyMultiValueQuery(object value, SQLComparisonOperator filterOperator) =>
			CartageCompanyMultiValueQuery(value, filterOperator, JobDocsAndCartageSchema.JP_OA_PickupCartageCoAddr);

		ZQuery DeliveryTransportCompanyMultiValueQuery(object value, SQLComparisonOperator filterOperator) =>
			CartageCompanyMultiValueQuery(value, filterOperator, JobDocsAndCartageSchema.JP_OA_DeliveryCartageCoAddr);

		ZQuery CartageCompanyMultiValueQuery(object value, SQLComparisonOperator filterOperator, SchemaColumn addressColumn)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			if (value is List<ZGuid> orgList && orgList.Count > 0)
			{
				var jobDocsAndCartageQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);

				var addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), addressColumn);
				addressQuery.AddToFilter(JoinCondition.Or, OrgAddressSchema.OA_OH, filterOperator, orgList);

				jobDocsAndCartageQuery.AddSubQuery(addressQuery, JoinCondition.And);

				result.AddSubQuery(jobDocsAndCartageQuery, JoinCondition.And);
			}

			return result;
		}

		#endregion

		#endregion

		#region GetCompanyNameQuery, GetConsignorCompanyNameQuery, GetConsigneeCompanyNameQuery

		protected ZQuery GetCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			return GetCompanyNameQuery(comparisonOperator, companyName, DocAddressType.ConsignorDocumentaryAddress, DocAddressType.ConsigneeDocumentaryAddress);
		}

		protected ZQuery GetConsignorCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			return GetCompanyNameQuery(comparisonOperator, companyName, DocAddressType.ConsignorDocumentaryAddress);
		}

		protected ZQuery GetConsigneeCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			return GetCompanyNameQuery(comparisonOperator, companyName, DocAddressType.ConsigneeDocumentaryAddress);
		}

		protected ZQuery GetNotifyPartyCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			return GetCompanyNameQuery(comparisonOperator, companyName, DocAddressType.NotifyParty, DocAddressType.NotifyParty2, DocAddressType.NotifyParty3);
		}

		protected ZQuery GetCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName, params DocAddressType[] addressTypes)
		{
			var result = new ZQuery();
			var notIn = SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref comparisonOperator);

			var shipmentResult = new ZDBOnlyQuery(typeof(ForwardingShipment));
			var jobDocAddressFilter = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn);
			var orgAddresses = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

			var orgHeaders = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
			orgHeaders.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, companyName);
			orgAddresses.AddSubQuery(orgHeaders, JoinCondition.And);
			jobDocAddressFilter.AddSubQuery(orgAddresses, JoinCondition.And);

			var addressTypeFilter = new ZQuery();

			addressTypeFilter.AddToFilter(JobDocAddressSchema.E2_AddressType, addressTypes.Select(addressType => DocAddressTypes.GetCode(Factory, addressType)));

			jobDocAddressFilter.AddToFilter(addressTypeFilter);
			jobDocAddressFilter.AddToFilter(JobDocAddressSchema.E2_AddressOverride, false);

			var jobDocAddressOverrideFilter = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn);
			jobDocAddressOverrideFilter.AddToFilter(JobDocAddressSchema.E2_CompanyName, comparisonOperator, companyName);
			jobDocAddressOverrideFilter.AddToFilter(addressTypeFilter);
			jobDocAddressOverrideFilter.AddToFilter(JobDocAddressSchema.E2_AddressOverride, true);

			if (notIn)
			{
				shipmentResult.AddSubQuery(jobDocAddressFilter, JoinCondition.And);
				shipmentResult.AddSubQuery(jobDocAddressOverrideFilter, JoinCondition.And);

				result.AddToFilter(shipmentResult);
			}
			else
			{
				jobDocAddressFilter.AddAsUnionQuery(jobDocAddressOverrideFilter, true);

				shipmentResult.AddSubQuery(jobDocAddressFilter, JoinCondition.And);
				result.AddToFilter(shipmentResult);
			}

			return result;
		}

		#endregion

		#region GetSalesRepCartageCoordinatorQuery

		ZQuery GetSalesRepQuery(ZDBOnlySubQuery filtersMatchQuery, SQLComparisonOperator comparisonOperator, ZString salesRepNK)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			bool notIn = comparisonOperator == SQLComparisonOperator.NotEqual || comparisonOperator == SQLComparisonOperator.IsBlank;
			ZDBOnlySubQuery jobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID, notIn);
			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_ParentTableCode, JobShipmentSchema.Constants.Prefix);
			jobHeaderQuery.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

			if (filtersMatchQuery != null)
			{
				jobHeaderQuery.AddSubQuery(JobHeaderSchema.JH_GS_NKRepSales, GlbStaffSchema.GS_Code, filtersMatchQuery, JoinCondition.And);
			}
			else
			{
				if (comparisonOperator == SQLComparisonOperator.NotEqual)
				{
					jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GS_NKRepSales, SQLComparisonOperator.Equal, salesRepNK);
				}
				else if (comparisonOperator == SQLComparisonOperator.IsBlank)
				{
					jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GS_NKRepSales, SQLComparisonOperator.NotEqual, ZString.Empty);
				}
				else
				{
					jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GS_NKRepSales, comparisonOperator, salesRepNK);
				}
			}

			result.AddSubQuery(jobHeaderQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetCartageCoordinatorQuery(ZDBOnlySubQuery filtersMatchQuery, SQLComparisonOperator comparisonOperator, ZString cartageCoordinatorNK)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			ZDBOnlyQuery coordinatorQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));

			ZDBOnlySubQuery consigneeDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, comparisonOperator == SQLComparisonOperator.NotEqual);
			ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			ZDBOnlySubQuery orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);

			ZDBOnlySubQuery consigneeStaffQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH);
			consigneeStaffQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GC, GlbCompany.CurrentCompany.PK);
			consigneeStaffQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_Role, StaffAssignmentRoles.Codes.CartageCoordinator);

			if (filtersMatchQuery != null)
			{
				consigneeStaffQuery.AddSubQuery(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, GlbStaffSchema.GS_Code, filtersMatchQuery, JoinCondition.And);
			}
			else
			{
				consigneeStaffQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible,
					comparisonOperator == SQLComparisonOperator.NotEqual ? SQLComparisonOperator.Equal : comparisonOperator,
					cartageCoordinatorNK);
			}

			orgHeaderSubQuery.AddSubQuery(consigneeStaffQuery, JoinCondition.And);

			orgAddressSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
			consigneeDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, DocAddressType.ConsigneePickupDeliveryAddress));
			consigneeDocAddressSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

			ZDBOnlySubQuery consignorDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, comparisonOperator == SQLComparisonOperator.NotEqual);
			orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);

			ZDBOnlySubQuery consignorStaffQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH);
			consignorStaffQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GC, GlbCompany.CurrentCompany.PK);
			consignorStaffQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_Role, StaffAssignmentRoles.Codes.CartageCoordinator);

			if (filtersMatchQuery != null)
			{
				consignorStaffQuery.AddSubQuery(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, GlbStaffSchema.GS_Code, filtersMatchQuery, JoinCondition.And);
			}
			else
			{
				consignorStaffQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible,
					comparisonOperator == SQLComparisonOperator.NotEqual ? SQLComparisonOperator.Equal : comparisonOperator,
					cartageCoordinatorNK);
			}

			orgHeaderSubQuery.AddSubQuery(consignorStaffQuery, JoinCondition.And);

			orgAddressSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
			consignorDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, DocAddressType.ConsignorPickupDeliveryAddress));
			consignorDocAddressSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

			JoinCondition filterJoinCondition;
			if (comparisonOperator == SQLComparisonOperator.NotEqual)
			{
				filterJoinCondition = JoinCondition.And;
			}
			else
			{
				filterJoinCondition = JoinCondition.Or;
			}

			coordinatorQuery.AddSubQuery(consigneeDocAddressSubQuery, filterJoinCondition);
			coordinatorQuery.AddSubQuery(consignorDocAddressSubQuery, filterJoinCondition);
			result.AddToFilter(coordinatorQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region GetCustomsBrokerQuery

		ZQuery GetCustomsBrokerQuery(ZDBOnlySubQuery filtersMatchQuery, SQLComparisonOperator comparisonOperator, ZString customsBrokerNK)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			bool notIn = comparisonOperator == SQLComparisonOperator.NotEqual || comparisonOperator == SQLComparisonOperator.IsBlank;
			ZDBOnlySubQuery declarationQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.JE_JS, notIn);

			if (filtersMatchQuery != null)
			{
				declarationQuery.AddSubQuery(JobDeclarationSchema.JE_GS_NKCusAgent, GlbStaffSchema.GS_Code, filtersMatchQuery, JoinCondition.And);
			}
			else
			{
				if (comparisonOperator == SQLComparisonOperator.IsBlank)
				{
					declarationQuery.AddToFilter(JobDeclarationSchema.JE_GS_NKCusAgent, SQLComparisonOperator.NotEqual, ZString.Empty);
				}
				else if (comparisonOperator == SQLComparisonOperator.NotEqual)
				{
					declarationQuery.AddToFilter(JobDeclarationSchema.JE_GS_NKCusAgent, SQLComparisonOperator.Equal, customsBrokerNK);
				}
				else
				{
					declarationQuery.AddToFilter(JobDeclarationSchema.JE_GS_NKCusAgent, comparisonOperator, customsBrokerNK);
				}
			}

			result.AddSubQuery(declarationQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#region GetRelatedPartiesQuery

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
			ZQuery jobHeader = OrgRelatedPartiesFilterHelper.GetJobHeaderFromOrgHeaderFilter(orgHeaderFilter, new ZQuery(), false, false);

			return FromJobHeaderFilter(jobHeader, false);
		}

		#endregion

		#region ClientAssignedStaffFilter

		protected virtual void CustomizeClientAssignedStaffFilter(OrgClientAssignedStaffModuleFilter filter)
		{
			filter.SupportsFiltersMatchComparisonOperator = true;
		}

		ZQuery GetClientAssignedStaffQuery(ZDBOnlySubQuery orgFilter)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ForwardingShipment));
			query.AddSubQuery(orgFilter, JoinCondition.And);

			return query;
		}

		#endregion

		#endregion

		#region Location Filters

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			ModuleLocationFilter loadDischargeFilter = filters.AddLocationFilter(Descriptions.LoadDischarge, GetLoadDischargePortsQuery, BindingLists.RefLocation_List, BindingLists.RefLocation_List);
			loadDischargeFilter.GetXQuery = GetPlannedLoadDischargeXQuery; //it's not exactly the same thing but I don't think a template CAN store related consol data (the relevant grid on the form is read-only)
			loadDischargeFilter.SetItemDescriptions(Res.GetData("Forwarding|JobShipmentFilter|Load", "Load"), Res.GetData("Forwarding|JobShipmentFilter|Discharge", "Discharge"));
			loadDischargeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|LoadDischarge", "Load / Discharge");
			if (!Env.Security.MaintainShipmentAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed)
			{
				loadDischargeFilter.Visibility = FilterVisibility.AlwaysVisible;
			}

			ModuleLocationFilter originDestFiter = filters.AddLocationFilter(Descriptions.OriginDestination, GetOriginDestinationQuery, BindingLists.RefLocation_List, BindingLists.RefLocation_List);
			originDestFiter.GetXQuery = GetOriginDestinationXQuery;
			originDestFiter.SetItemDescriptions(Res.GetData("Forwarding|JobShipmentFilter|Origin", "Origin"), Res.GetData("Forwarding|JobShipmentFilter|Dest", "Dest."));
			originDestFiter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|OriginDestination", "Origin / Destination");
			if (!Env.Security.MaintainShipmentAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed)
			{
				originDestFiter.Visibility = FilterVisibility.AlwaysVisible;
			}

			loadDischargeFilter.Property1Validation = info => PortFilterSecurityValidator.ValidatePort(info, ((ModuleLocationFilter)info.BizObj).Property2Info, filters, Descriptions.LoadDischarge, Descriptions.OriginDestination);
			loadDischargeFilter.Property2Validation = info => PortFilterSecurityValidator.ValidatePort(info, ((ModuleLocationFilter)info.BizObj).Property1Info, filters, Descriptions.LoadDischarge, Descriptions.OriginDestination);

			originDestFiter.Property1Validation = info => PortFilterSecurityValidator.ValidatePort(info, ((ModuleLocationFilter)info.BizObj).Property2Info, filters, Descriptions.LoadDischarge, Descriptions.OriginDestination);
			originDestFiter.Property2Validation = info => PortFilterSecurityValidator.ValidatePort(info, ((ModuleLocationFilter)info.BizObj).Property1Info, filters, Descriptions.LoadDischarge, Descriptions.OriginDestination);

			var domesticFilter = filters.AddTextFilter("Domestic / International", GetDomesticInternationalQuery, DomesticInternationalList);
			domesticFilter.Category = originDestFiter.Category;
			domesticFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|DomesticInternational", "Domestic / International");

			var pickupPostCode = filters.AddTextFilter("Pickup Address Post Code", GetPickupPostCodeQuery)
				.WithMaxLengthOf<ModuleTextFilter>(OrgAddressSchema.OA_PostCode);
			pickupPostCode.Category = originDestFiter.Category;
			pickupPostCode.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|PickupPostCode", "Pickup Address Post Code");

			var deliveryPostCode = filters.AddTextFilter("Delivery Address Post Code", GetDeliveryPostCodeQuery)
				.WithMaxLengthOf<ModuleTextFilter>(OrgAddressSchema.OA_PostCode);
			deliveryPostCode.Category = originDestFiter.Category;
			deliveryPostCode.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|DeliveryPostCode", "Delivery Address Post Code");

			var plannedLoadDischargePortFilter = filters.AddLocationFilter(Descriptions.PlannedLoadDischarge, GetPlannedLoadDischargeQuery, BindingLists.RefLocation_List, BindingLists.RefLocation_List);
			plannedLoadDischargePortFilter.GetXQuery = GetPlannedLoadDischargeXQuery;
			plannedLoadDischargePortFilter.SetItemDescriptions(Res.GetData("Forwarding|JobShipmentFilter|PlannedLoad", "Planned Load"), Res.GetData("Forwarding|JobShipmentFilter|PlannedDischarge", "Planned Disch."));
			plannedLoadDischargePortFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|PlannedLoadDischarge", "Planned Load / Planned Discharge");
		}

		PortFilterSecurityValidator PortFilterSecurityValidator
		{
			get { return portFilterSecurityValidator ?? (portFilterSecurityValidator = new PortFilterSecurityValidator(Factory, Env.Security.MaintainShipmentAllowSearchOfUnlocoOutsideLoginBranches)); }
		}
		PortFilterSecurityValidator portFilterSecurityValidator;

		#region GetLoadDischargePortsQuery

		ZQuery GetLoadDischargePortsQuery(ZString loadPortNK, ZString dischargePortPK)
		{
			SailingFilterBuilder builder = new SailingFilterBuilder(Factory);
			builder.LoadPort = loadPortNK;
			builder.DischargePort = dischargePortPK;

			return builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.ViaConsol);
		}

		#endregion

		#region GetOriginDestinationQuery

		ZQuery GetOriginDestinationQuery(ZString originNK, ZString destinationNK)
		{
			ZQuery result = new ZQuery();

			if (!originNK.IsEmpty)
			{
				result.AddToFilter(LocationHelper.GetLocationFilter(Factory, originNK, JobShipmentSchema.JS_RL_NKOrigin, typeof(CommonShipment)));
			}

			if (!destinationNK.IsEmpty)
			{
				result.AddToFilter(LocationHelper.GetLocationFilter(Factory, destinationNK, JobShipmentSchema.JS_RL_NKDestination, typeof(CommonShipment)));
			}

			return result;
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

		#endregion

		#region GetDomesticInternationalQuery

		ZQuery GetDomesticInternationalQuery(ZString value)
		{
			ZQuery result = new ZQuery();

			if (value == DomesticInternationalFilterItems.Domestic)
			{
				string sQL = string.Format(CultureInfo.InvariantCulture, "SUBSTRING({0}, 1, 2) = SUBSTRING({1}, 1, 2)", JobShipmentSchema.JS_RL_NKOrigin.Name, JobShipmentSchema.JS_RL_NKDestination.Name);
				result = new ZDBOnlyQuery(typeof(ForwardingShipment));
				result.AddFilterAndZSQLParameterCollection(sQL, null);
			}
			else if (value == DomesticInternationalFilterItems.International)
			{
				string sQL = string.Format(CultureInfo.InvariantCulture, "SUBSTRING({0}, 1, 2) <> SUBSTRING({1}, 1, 2)", JobShipmentSchema.JS_RL_NKOrigin.Name, JobShipmentSchema.JS_RL_NKDestination.Name);
				result = new ZDBOnlyQuery(typeof(ForwardingShipment));
				result.AddFilterAndZSQLParameterCollection(sQL, null);
			}

			return result;
		}

		#endregion

		#region Pickup/Delivery Post Codes Queries

		ZQuery GetPickupPostCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetPostCodeQuery(AutoDocAddressTypes.Codes.ConsignorPickupDeliveryAddress, comparisonOperator, value);
		}

		ZQuery GetDeliveryPostCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetPostCodeQuery(AutoDocAddressTypes.Codes.ConsigneePickupDeliveryAddress, comparisonOperator, value);
		}

		ZQuery GetPostCodeQuery(string addressType, SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ForwardingShipment));

			ZDBOnlySubQuery jobDocAddressFilter = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			jobDocAddressFilter.AddToFilter(JobDocAddressSchema.E2_AddressOverride, "N");
			jobDocAddressFilter.AddToFilter(JobDocAddressSchema.E2_AddressType, addressType);
			jobDocAddressFilter.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, "JS");

			ZDBOnlySubQuery orgAddressFilter = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressFilter.AddToFilter(OrgAddressSchema.OA_PostCode, comparisonOperator, value);
			jobDocAddressFilter.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressFilter, JoinCondition.And);

			query.AddSubQuery(jobDocAddressFilter, JoinCondition.And);

			ZDBOnlySubQuery jobDocAddressOverrideFilter = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			jobDocAddressOverrideFilter.AddToFilter(JobDocAddressSchema.E2_AddressOverride, "Y");
			jobDocAddressOverrideFilter.AddToFilter(JobDocAddressSchema.E2_AddressType, addressType);
			jobDocAddressOverrideFilter.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, "JS");
			jobDocAddressOverrideFilter.AddToFilter(JobDocAddressSchema.E2_Postcode, comparisonOperator, value);

			query.AddSubQuery(jobDocAddressOverrideFilter, JoinCondition.Or);

			return query;
		}

		#endregion

		#region GetPlannedLoadDischargeQuery

		ZQuery GetPlannedLoadDischargeQuery(ZString loadNK, ZString dischargeNK)
		{
			ZQuery result = new ZQuery();

			if (!loadNK.IsEmpty)
			{
				result.AddToFilter(LocationHelper.GetLocationFilter(Factory, loadNK, JobShipmentSchema.JS_RL_NKLoadPort, typeof(CommonShipment)));
			}

			if (!dischargeNK.IsEmpty)
			{
				result.AddToFilter(LocationHelper.GetLocationFilter(Factory, dischargeNK, JobShipmentSchema.JS_RL_NKDischargePort, typeof(CommonShipment)));
			}

			return result;
		}

		ZQuery GetPlannedLoadDischargeXQuery(ModuleFilter moduleFilter)
		{
			var filter = moduleFilter as ModuleLocationFilter;
			var result = new ZQuery();

			if (!filter.Property1.IsEmpty)
			{
				result.AddToFilter(XQueryFilterHelper.GenerateXQuery((column) => new ZQuery(column, SQLComparisonOperator.StartsWith, filter.Property1), new XQueryFilterInfo(ShipmentXQueryPaths.PortOfLoading, JobShipmentSchema.JS_RL_NKLoadPort.MaxLength)));
			}

			if (!filter.Property2.IsEmpty)
			{
				result.AddToFilter(XQueryFilterHelper.GenerateXQuery((column) => new ZQuery(column, SQLComparisonOperator.StartsWith, filter.Property2), new XQueryFilterInfo(ShipmentXQueryPaths.PortOfDischarge, JobShipmentSchema.JS_RL_NKDischargePort.MaxLength)));
			}

			return result;
		}

		#endregion

		#endregion

		#region Modes Filters

		void AddModeFilters(ModuleFilterCollection filters)
		{
			var containerModeFilter = filters.AddTextFilter("Container Mode", JobShipmentSchema.JS_PackingMode, JS_PackingMode_List);
			containerModeFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.ContainerMode, JobShipmentSchema.JS_PackingMode.MaxLength);
			containerModeFilter.Category = FilterCategories.ModesAndTypes;
			containerModeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ContainerMode", "Container Mode");

			var consolContainerModeFilter = filters.AddTextFilter("Consol Container Mode", (comparisonOperator, value) => GetConsolQuery(JobConsolSchema.JK_ConsolMode, comparisonOperator, value), ConsolMode_List)
				.WithMaxLengthOf<ModuleTextFilter>(JobConsolSchema.JK_ConsolMode);
			consolContainerModeFilter.Category = FilterCategories.ModesAndTypes;
			consolContainerModeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ConsolContainerMode", "Consol Container Mode");

			var transportModeFilter = filters.AddTextFilter("Transport Mode", JobShipmentSchema.JS_TransportMode, JS_TransportMode_List);
			transportModeFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.TransportMode, JobShipmentSchema.JS_TransportMode.MaxLength);
			transportModeFilter.Category = FilterCategories.ModesAndTypes;
			transportModeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|TransportMode", "Transport Mode");

			var consolTransportModeFilter = filters.AddTextFilter("Consol Transport Mode", (comparisonOperator, value) => GetConsolQuery(JobConsolSchema.JK_TransportMode, comparisonOperator, value), ConsolTransportModes)
				.WithMaxLengthOf<ModuleTextFilter>(JobConsolSchema.JK_TransportMode);
			consolTransportModeFilter.Category = FilterCategories.ModesAndTypes;
			consolTransportModeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ConsolTransportMode", "Consol Transport Mode");

			ModuleFilter shipmentTypeFilter = filters.AddFlagsFilter("Shipment Type",
				new string[]
				{
					Constants.ShipmentTypeDescriptions.AssemblyMaster,
					Constants.ShipmentTypeDescriptions.BuyersConsolLead,
					Constants.ShipmentTypeDescriptions.CoLoadMaster,
					Constants.ShipmentTypeDescriptions.BlindCoLoadMaster,
					Constants.ShipmentTypeDescriptions.StandardHouse,
					Constants.ShipmentTypeDescriptions.HighVolumeLowValue,
					Constants.ShipmentTypeDescriptions.HighVolumeLowValueMaster,
					Constants.ShipmentTypeDescriptions.ThirdPartyOwnershipHouse
				},
				new GetFlagsQuery[]
				{
					GetAssemblyMasterFilter,
					GetBuyersConsolLeadFilter,
					GetCoLoadMasterFilter,
					GetBlindCoLoadMasterFilter,
					GetStandardHouseFilter,
					GetHighVolumeLowValueFilter,
					GetHighVolumeLowValueMasterFilter,
					GetThirdPartyOwnershipHouseFilter
				});
			shipmentTypeFilter.GetXQuery = GetShipmentTypeXQuery;
			shipmentTypeFilter.Category = FilterCategories.ModesAndTypes;
			shipmentTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ShipmentType", "Shipment Type");

			ModuleNkFilter serviceLevelFilter = filters.AddNkFilter("Service Level", JobShipmentSchema.JS_RS_NKServiceLevel, ModuleIDs.ServiceLevel, BindingLists.RefServiceLevel_List);
			serviceLevelFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.ServiceLevel, JobShipmentSchema.JS_RS_NKServiceLevel.MaxLength);
			serviceLevelFilter.Category = FilterCategories.ModesAndTypes;
			serviceLevelFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ServiceLevel", "Service Level");

			var dgSubstanceDGClassFilter = new DGClassDGSubstanceFilter("DG Class / DG Substance", GetDGClassDGSubstanceQuery);
			dgSubstanceDGClassFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|DGClassDGSubstance", "DG Class / DG Substance");
			dgSubstanceDGClassFilter.Category = FilterCategories.ModesAndTypes;
			filters.AddCustomFilter(dgSubstanceDGClassFilter);

			var serviceTypeDateBookedFilter = new ServiceTypeDateFilter(this, typeof(JobDocsAndCartage), true);
			serviceTypeDateBookedFilter.SubGroup = JobDocsAndCartageSubGroup;
			filters.AddCustomFilter(serviceTypeDateBookedFilter);

			var serviceTypeDateCompletedFilter = new ServiceTypeDateFilter(this, typeof(JobDocsAndCartage), false);
			serviceTypeDateCompletedFilter.SubGroup = JobDocsAndCartageSubGroup;
			filters.AddCustomFilter(serviceTypeDateCompletedFilter);
		}

		#region GetDGClassDGSubstanceQuery

		ZQuery GetDGClassDGSubstanceQuery(SQLComparisonOperator dgOperator, ZString dgClass, ZString dgSubstance)
		{
			var notInOperations = new SQLComparisonOperator[] { SQLComparisonOperator.NotContains, SQLComparisonOperator.DoesNotStartWith, SQLComparisonOperator.NotEqual, SQLComparisonOperator.IsBlank };
			var isBlankOperation = dgOperator == SQLComparisonOperator.IsBlank || dgOperator == SQLComparisonOperator.IsNotBlank;

			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
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
			result.AddSubQuery(packLineQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#endregion

		#region StatusAndFlags Filters

		protected virtual void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			ModuleFilter phaseFilter = filters.AddTextFilter("Phase", JobShipmentSchema.JS_Phase, PhaseList);
			phaseFilter.Category = FilterCategories.StatusAndFlags;
			phaseFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|Phase", "Phase");

			ModuleFilter podFilter = filters.AddTextFilter("Proof-of-Delivery (POD) information", GetPodQuery, PodFilterList);
			podFilter.Category = FilterCategories.StatusAndFlags;
			podFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ProofofDeliveryPODInformation", "Proof-of-Delivery (POD) information");

			ModuleFilter coLoadFilter = filters.AddTextFilter(Descriptions.CoLoadStatus, GetCoLoadStatusQuery, CoLoadStatus_List);
			coLoadFilter.Category = FilterCategories.StatusAndFlags;
			coLoadFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|CoLoadStatus", "Co-Load Status");

			ModuleFlagsFilter flagRestrictionsFilter = filters.AddFlagsFilter(Descriptions.HiddenFwdRegistered, new string[] { Res.GetString("Forwarding|JobShipmentFilter|HiddenFilter", "Hidden Filter") }, new GetFlagsQuery[] { GetShowForwardRegisteredQuery });
			flagRestrictionsFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			flagRestrictionsFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|HiddenForwardRegisteredFilter", "Hidden Forward Registered Filter");

			ModuleFlagsFilter flagsFilter = filters.AddFlagsFilter(Descriptions.ShowTranshipCrossTradeNoConsol,
				new string[] { Res.GetString("Forwarding|JobShipmentFilter|ShowTranshipmentsOnly", "Show Transhipments Only"), Res.GetString("Forwarding|JobShipmentFilter|CrossTradeShipmentsOnly", "Cross Trade Shipments Only"), Res.GetString("Forwarding|JobShipmentFilter|ShipWoutConsolsOnly", "Ship. w/out Consols Only") },
				new GetFlagsQuery[] { GetOnlyShowTranshipmentsQuery, GetOnlyShowCrossTradeShipmentsQuery, GetOnlyShowShipmentsWithoutConsolsQuery });
			flagsFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ShowTranshipCrossTradeNoConsol", "Show Tranship, Cross Trade, No Consol");
			flagsFilter.Visibility = FilterVisibility.AlwaysApplied;

			ModuleFilter securityInspectionFlagFilter = filters.AddTextFilter("Security Inspection", GetSecurityInspectionQuery, SecurityInspectionTypeList);
			securityInspectionFlagFilter.Category = FilterCategories.StatusAndFlags;
			securityInspectionFlagFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|SecurityInspection", "Security Inspection");

			ModuleFilter releaseTypeFilter = filters.AddTextFilter(Descriptions.ReleaseType, JobShipmentSchema.JS_ReleaseType, FreightDataRegistry.Instance.ReleaseTypes.Value.GetCodeDescriptionPairList());
			releaseTypeFilter.Category = FilterCategories.StatusAndFlags;
			releaseTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ReleaseType", "Release Type");

			ModuleFilter incoTermFilter = filters.AddTextFilter(Descriptions.IncoTerms, JobShipmentSchema.JS_INCO, IncoTermList);
			incoTermFilter.Category = FilterCategories.StatusAndFlags;
			incoTermFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|Incoterms", "Incoterm");

			var efreightStatusFilter = filters.AddTextFilter(Descriptions.EFreightStatus, GetEFreightStatusQuery, EFreightStatus_List);
			efreightStatusFilter.Category = FilterCategories.StatusAndFlags;
			efreightStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|EFreightStatus", "e-freight Status");

			var bookingStatusFilter = filters.AddTextFilter(Descriptions.BookingStatus, JobShipmentSchema.JS_ShipmentStatus, ShipmentStatusList);
			bookingStatusFilter.Category = FilterCategories.StatusAndFlags;
			bookingStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|HBLBookingStatus", "HBL Booking Status");

			var isTemperatureControlledFilter = filters.AddFlagsFilter(
				Descriptions.IsTemperatureControlled,
				new string[] { Res.GetString("Forwarding|JobShipmentFilter|IsTemperatureControlled", "Is Temperature Controlled") },
				new GetFlagsQuery[] { GetIsTemperatureControlledQuery }
			);
			isTemperatureControlledFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|IsTemperatureControlled", "Is Temperature Controlled");

			filters.AddFlagsFilter(Descriptions.IsHazardous,
				new string[] { Res.GetString("Forwarding|JobShipmentFilter|IsHazardous", "Is Hazardous") },
				new GetFlagsQuery[] { GetIsHazardous }).
				MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|IsHazardous", "Is Hazardous");

			var hasDamagedPackagesFilter = filters.AddFlagsFilter(Descriptions.HasDamagedPackages,
				new string[] { Res.GetString("Forwarding|JobShipmentFilter|HasDamagedPackages", "Has Damaged Packages") },
				new GetFlagsQuery[] { GetHasDamagedPackages });
			hasDamagedPackagesFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|HasDamagedPackages", Descriptions.HasDamagedPackages);

			var hasPillagedPackagesFilter = filters.AddFlagsFilter(Descriptions.HasPillagedPackages,
				new string[] { Res.GetString("Forwarding|JobShipmentFilter|HasPillagedPackages", "Has Pillaged Packages") },
				new GetFlagsQuery[] { GetHasPillagedPackages });
			hasPillagedPackagesFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|HasPillagedPackages", Descriptions.HasPillagedPackages);

			var originTransitWarehouseStatusFilter = filters.AddTextFilter(Descriptions.OriginTransitWarehouseStatus, GetOriginTransitWarehouseStatus, OriginTransitWarehouseStatuses);
			originTransitWarehouseStatusFilter.Category = FilterCategories.StatusAndFlags;
			originTransitWarehouseStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|OriginTransitWarehouseStatus", Descriptions.OriginTransitWarehouseStatus);

			if (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsCountryEuOrCtCountry(GlbCompany.CurrentCompany.Country.Code))
			{
				var ctStatusFilter = filters.AddTextFilter(Descriptions.CTStatus, JobShipmentSchema.JS_CommunityTransitStatus, CTStatusList);
				ctStatusFilter.Category = FilterCategories.StatusAndFlags;
				ctStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|CTStatus", Descriptions.CTStatus);

				var exitStatusFilter = filters.AddTextFilter(Descriptions.ExitStatus, GetExitStatusQuery, ExitStatusCodesList).WithMaxLengthOf<ModuleTextFilter>(CusExitReportSchema.CER_Status);
				exitStatusFilter.Category = FilterCategories.StatusAndFlags;
				exitStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ExitStatus", Descriptions.ExitStatus);
			}

			if (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.Value.EnableEBLIntegration)
			{
				var hblStatusFilter = filters.AddTextFilter(Descriptions.HBLStatus, JobShipmentSchema.JS_ElectronicBillOfLadingStatus, FreightCodePairLists.HouseBillOfLadingBillStatusList());
				hblStatusFilter.Category = FilterCategories.StatusAndFlags;
				hblStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|HBLStatus", Descriptions.HBLStatus);

				var hblTypeFilter = filters.AddTextFilter(Descriptions.HBLType, JobShipmentSchema.JS_ElectronicBillOfLadingType, FreightCodePairLists.BillOfLadingBillTypeList());
				hblTypeFilter.Category = FilterCategories.StatusAndFlags;
				hblTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|HBLType", Descriptions.HBLType);

				var hblTermsFilter = filters.AddTextFilter(Descriptions.HBLTerms, JobShipmentSchema.JS_ElectronicBillOfLadingTerms, FreightCodePairLists.BillOfLadingBillTermsList());
				hblTermsFilter.Category = FilterCategories.StatusAndFlags;
				hblTermsFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|HBLTerms", Descriptions.HBLTerms);
			}
		}

		internal static void SetFilterConstraints(ModuleTextFilter moduleFilter, FilterCategory category)
		{
			moduleFilter.Category = category;
			moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
		}

		#region EFreightStatus_List

		public CodeDescriptionPairList EFreightStatus_List
		{
			get
			{
				if (efreightStatus_List == null)
				{
					efreightStatus_List = new CodeDescriptionPairList();
					efreightStatus_List.AddPair(EfreightStatusFilterItems.All, Res.GetString("Forwarding|JobShipmentFilter|ShowAllEFreightStatusDesc", "e-freight enabled"));
					efreightStatus_List.AddRange(new CodeDescriptionPairList(OLookUpEditType.EFreightStatus));
				}

				return efreightStatus_List;
			}
		}
		CodeDescriptionPairList efreightStatus_List;

		public static class EfreightStatusFilterItems
		{
			public const string All = "ALL";
		}

		#endregion

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

		#region ShipmentStatusList

		public CodeDescriptionPairList ShipmentStatusList
		{
			get
			{
				if (shipmentStatusList == null)
				{
					shipmentStatusList = new CodeDescriptionPairList();
					shipmentStatusList.Add(new CodeDescriptionPair(Integration.ShipmentStatusList.Codes.ElectronicBooking, Res.GetString("37b76f93-4f33-4f32-810d-2d7c64c2248a", "eBooking Request Received")));
					shipmentStatusList.Add(new CodeDescriptionPair(Integration.ShipmentStatusList.Codes.EBookingCancellationRequest, Integration.ShipmentStatusList.Descriptions.EBookingCancellationRequest));
					shipmentStatusList.Add(new CodeDescriptionPair(Integration.ShipmentStatusList.Codes.Booked, Res.GetString("04e1759e-12d5-447f-b59e-7b817f0abbf5", "Booking Confirmed")));
					shipmentStatusList.Add(new CodeDescriptionPair(Integration.ShipmentStatusList.Codes.BookingCancelled, Integration.ShipmentStatusList.Descriptions.BookingCancelled));
					shipmentStatusList.Add(new CodeDescriptionPair(Integration.ShipmentStatusList.Codes.BookingRejected, Integration.ShipmentStatusList.Descriptions.BookingRejected));
					shipmentStatusList.Add(new CodeDescriptionPair(Integration.ShipmentStatusList.Codes.ElectronicShippingInstruction, Res.GetString("f0c26090-5c54-42ea-9faa-3e52bbfc8901", "eSI Received")));
					shipmentStatusList.Add(new CodeDescriptionPair(Integration.ShipmentStatusList.Codes.Confirmed, Res.GetString("3339767a-909a-44ae-ba97-23ce78f0c987", "SI Confirmed")));
					shipmentStatusList.Add(new CodeDescriptionPair(Integration.ShipmentStatusList.Codes.SIRejected, Res.GetString("b4a4dc36-2fff-4d7b-a1f4-94e2e9c35c2e", "SI Rejected")));
					shipmentStatusList.Add(new CodeDescriptionPair(Integration.ShipmentStatusList.Codes.WebBooking, Integration.ShipmentStatusList.Descriptions.WebBooking));
				}

				return shipmentStatusList;
			}
		}
		CodeDescriptionPairList shipmentStatusList;

		#endregion

		#region OriginTransitWarehouseStatuses

		CodeDescriptionPairList OriginTransitWarehouseStatuses
		{
			get
			{
				if (originTransitWarehouseStatuses == null)
				{
					originTransitWarehouseStatuses = new CodeDescriptionPairList
					{
						new CodeDescriptionPair(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, FreightConstants.PacklineOriginTransitWarehouseStatus.Descriptions.Unknown),
						new CodeDescriptionPair(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, FreightConstants.PacklineOriginTransitWarehouseStatus.Descriptions.Discrepencies),
						new CodeDescriptionPair(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.ShortShipped, FreightConstants.PacklineOriginTransitWarehouseStatus.Descriptions.ShortShipped),
						new CodeDescriptionPair(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus, FreightConstants.PacklineOriginTransitWarehouseStatus.Descriptions.Surplus),
						new CodeDescriptionPair(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, FreightConstants.PacklineOriginTransitWarehouseStatus.Descriptions.Confirmed)
					};
				}

				return originTransitWarehouseStatuses;
			}
		}
		CodeDescriptionPairList originTransitWarehouseStatuses;

		#endregion

		#region Security Inspection Filter

		public CodeDescriptionPairList SecurityInspectionTypeList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();

				list.AddPair(BaseJobShipmentLookups.InspectionType_Approved, FreightUtilities.InspectionType_Approved_Description);
				list.AddPair(BaseJobShipmentLookups.InspectionType_Screened, ResString.GetMultilingualString("5e286dfb-4cd3-456f-a96a-9ced4b526b65", "Screened")); // Inspection Type Description
				foreach (CodeDescriptionPair item in SupplyChainSecurityConfiguration.InspectionTypeList)
				{
					list.Add(item);
				}

				list.AddPair(FreightDataRegistry.AviationSecurity_Unknown_Code, Res.GetString("Forwarding|JobShipmentFilter|AviationSecurityUnknown", "Unknown - No Security Measures Taken"));

				return list;
			}
		}

		ZQuery GetSecurityInspectionQuery(ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			if (!value.IsEmpty)
			{
				string webExceptionSQL = string.Empty;
				string unkUnionSQL = string.Empty;
				string countryCode = Enterprise.Core.Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
					? Enterprise.Core.Constants.CountryCodes.EuropeanUnion
					: GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				if (value == SupplyChainSecurityConfiguration.InspectionTypeDefault)
				{
					webExceptionSQL = string.Format(CultureInfo.InvariantCulture, "OR {0} = '{1}'", // Raw SQL used for performance improvement
						CusEntryNumber.Schema.CE_EntryNum,
						BaseJobShipmentLookups.InspectionType_Web);

					unkUnionSQL = string.Format(CultureInfo.InvariantCulture, @"
UNION
SELECT {0}
FROM {1} LEFT JOIN {2}
	ON {3} = {0}
	AND {4} = '{1}'
		AND {5} = '{6}'
		AND {7} = '{8}'
		AND {9} IN ('{10}', '')
WHERE {3} IS NULL
",
						ForwardingShipment.Schema.PK,
						ForwardingShipment.Schema.TableName,
						CusEntryNumber.Schema.TableName,
						CusEntryNumber.Schema.CE_ParentID,
						CusEntryNumber.Schema.CE_ParentTable,
						CusEntryNumber.Schema.CE_Category,
						CusEntryNumber.Categories.InspectionStatus,
						CusEntryNumber.Schema.CE_EntryType,
						CusEntryNumber.EntryType.InspectionStatus,
						CusEntryNumber.Schema.CE_RN_NKCountryCode,
						countryCode);
				}

				string sql = string.Format(CultureInfo.InvariantCulture, $@"
				{ForwardingShipment.Schema.PK} IN
					(SELECT {CusEntryNumber.Schema.CE_ParentID}
					FROM
						(SELECT {CusEntryNumber.Schema.CE_ParentID}
						FROM {CusEntryNumber.Schema.TableName} C1
						WHERE {CusEntryNumber.Schema.CE_ParentTable} = '{ForwardingShipment.Schema.TableName}'
							AND {CusEntryNumber.Schema.CE_Category} = '{CusEntryNumber.Categories.InspectionStatus}'
							AND {CusEntryNumber.Schema.CE_EntryType} = '{CusEntryNumber.Categories.InspectionStatus}'
							AND
							(
								({CusEntryNumber.Schema.CE_RN_NKCountryCode} = '{countryCode}' AND ({CusEntryNumber.Schema.CE_EntryNum} = @value {webExceptionSQL}))
								OR
								(
									{CusEntryNumber.Schema.CE_RN_NKCountryCode} = ''
									AND ({CusEntryNumber.Schema.CE_EntryNum} = @value {webExceptionSQL})
									AND {CusEntryNumber.Schema.CE_ParentID} NOT IN
									(
										SELECT {CusEntryNumber.Schema.CE_ParentID} FROM {CusEntryNumber.Schema.TableName}
										WHERE {CusEntryNumber.Schema.CE_ParentTable} = '{ForwardingShipment.Schema.TableName}'
											AND {CusEntryNumber.Schema.CE_Category} = '{CusEntryNumber.Categories.InspectionStatus}'
											AND {CusEntryNumber.Schema.CE_EntryType} = '{CusEntryNumber.Categories.InspectionStatus}'
											AND {CusEntryNumber.Schema.CE_RN_NKCountryCode} = '{countryCode}'
									)
								)
							)
						) AS Sub
					{unkUnionSQL}
					)"
				);

				var parameters = new ZSqlParameterCollection();
				parameters.Add("@value", value, CusEntryNumSchema.CE_EntryNum);
				result.AddFilterAndZSQLParameterCollection(sql, parameters);
			}

			return result;
		}

		#endregion

		protected GenAddOnColumnQueryHelper QueryHelper
		{
			get
			{
				if (queryHelper == null)
				{
					queryHelper = new GenAddOnColumnQueryHelper(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>(), ObjectFactory.GetType<Enterprise.Integration.Customs.US.IBill>(), CusDecHouseBillSchema.CU_JE);
				}
				return queryHelper;
			}
		}
		GenAddOnColumnQueryHelper queryHelper;

		protected GenAddOnColumnQueryHelper SimpleQueryHelper
		{
			get
			{
				if (simpleQueryHelper == null)
				{
					simpleQueryHelper = new GenAddOnColumnQueryHelper(typeof(ForwardingShipment), ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>(), JobDeclarationSchema.JE_JS);
				}
				return simpleQueryHelper;
			}
		}
		GenAddOnColumnQueryHelper simpleQueryHelper;

		protected GenAddOnColumnQueryHelper JobShipmentGenAddOnColumnQueryBuilder
		{
			get
			{
				if (jobShipmentGenAddOnColumnQueryBuilder == null)
				{
					jobShipmentGenAddOnColumnQueryBuilder = new GenAddOnColumnQueryHelper(typeof(ForwardingShipment));
				}
				return jobShipmentGenAddOnColumnQueryBuilder;
			}
		}
		GenAddOnColumnQueryHelper jobShipmentGenAddOnColumnQueryBuilder;

		#region GetEFreihtStatusQuery

		ZQuery GetEFreightStatusQuery(ZString efreightStatus)
		{
			var result = new ZQuery();
			if (efreightStatus == EfreightStatusFilterItems.All)
			{
				result.AddToFilter(JobShipmentSchema.JS_EFreightStatus, SQLComparisonOperator.NotEqual, string.Empty);
			}
			else
			{
				result.AddToFilter(JobShipmentSchema.JS_EFreightStatus, efreightStatus);
			}

			return result;
		}

		#endregion

		#region GetIsTemperatureControlledQuery

		ZQuery GetIsTemperatureControlledQuery(ZBool isTemperatureControlled)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			ZDBOnlySubQuery packLineQuery = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.JL_JS);
			packLineQuery.AddToFilter(JobPackLinesSchema.JL_RequiresTemperatureControl, isTemperatureControlled);
			packLineQuery.AddToFilter(JobPackLinesSchema.JL_FreightMode, FreightConstants.OuterPackType);
			result.AddSubQuery(packLineQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#region GetCoLoadStatusQuery

		ZQuery GetCoLoadStatusQuery(ZString coLoadStatus)
		{
			ZQuery result = new ZQuery();

			if (coLoadStatus.EqualsIgnoringCase(FreightConstants.CoLoadStatus.CoLoad))
			{
				result.AddToFilter(JobShipmentSchema.JS_JS_ColoadMasterShipment, SQLComparisonOperator.NotEqual, null);
			}
			else if (coLoadStatus.EqualsIgnoringCase(FreightConstants.CoLoadStatus.CoLoadMaster))
			{
				result.AddToFilter(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.Equal, Constants.ShipmentTypes.CoLoadMaster);
				result.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.Equal, Constants.ShipmentTypes.BlindCoLoadMaster);
			}
			else if (coLoadStatus.EqualsIgnoringCase(FreightConstants.CoLoadStatus.CoLoadAndCoLoadMaster))
			{
				result.AddToFilter(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.Equal, Constants.ShipmentTypes.CoLoadMaster);
				result.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.Equal, Constants.ShipmentTypes.BlindCoLoadMaster);
				result.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_JS_ColoadMasterShipment, SQLComparisonOperator.NotEqual, null);
			}
			else if (coLoadStatus.EqualsIgnoringCase(FreightConstants.CoLoadStatus.NeitherCoLoadNorCoLoadMaster))
			{
				result.AddToFilter(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.NotEqual, Constants.ShipmentTypes.CoLoadMaster);
				result.AddToFilter(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.NotEqual, Constants.ShipmentTypes.BlindCoLoadMaster);
				result.AddToFilter(JobShipmentSchema.JS_JS_ColoadMasterShipment, null);
			}

			return result;
		}

		#endregion

		#region Shipment Types

		ZQuery GetStandardHouseFilter(ZBool include)
		{
			ZQuery result = new ZQuery();

			if (!include)
			{
				result.AddToFilter(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.NotEqual, Constants.ShipmentTypes.StandardHouse);
			}
			return result;
		}

		ZQuery GetAssemblyMasterFilter(ZBool include)
		{
			ZQuery result = new ZQuery();

			if (!include)
			{
				result.AddToFilter(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.NotEqual, Constants.ShipmentTypes.AssemblyMaster);
			}
			return result;
		}

		ZQuery GetCoLoadMasterFilter(ZBool include)
		{
			ZQuery result = new ZQuery();

			if (!include)
			{
				result.AddToFilter(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.NotEqual, Constants.ShipmentTypes.CoLoadMaster);
			}
			return result;
		}

		ZQuery GetBlindCoLoadMasterFilter(ZBool include)
		{
			ZQuery result = new ZQuery();

			if (!include)
			{
				result.AddToFilter(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.NotEqual, Constants.ShipmentTypes.BlindCoLoadMaster);
			}

			return result;
		}

		ZQuery GetBuyersConsolLeadFilter(ZBool include)
		{
			ZQuery result = new ZQuery();

			if (!include)
			{
				result.AddToFilter(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.NotEqual, Constants.ShipmentTypes.BuyersConsolLead);
			}
			return result;
		}

		ZQuery GetHighVolumeLowValueFilter(ZBool include)
		{
			ZQuery result = new ZQuery();

			if (!include)
			{
				result.AddToFilter(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.NotEqual, Constants.ShipmentTypes.HighVolumeLowValue);
			}

			return result;
		}

		ZQuery GetHighVolumeLowValueMasterFilter(ZBool include)
		{
			ZQuery result = new ZQuery();

			if (!include)
			{
				result.AddToFilter(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.NotEqual, Constants.ShipmentTypes.HighVolumeLowValueMaster);
			}

			return result;
		}

		ZQuery GetThirdPartyOwnershipHouseFilter(ZBool include)
		{
			ZQuery result = new ZQuery();

			if (!include)
			{
				result.AddToFilter(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.NotEqual, Constants.ShipmentTypes.ThirdPartyOwnershipHouse);
			}
			return result;
		}

		ZQuery GetShipmentTypeXQuery(ModuleFilter moduleFilter)
		{
			var filter = moduleFilter as ModuleFlagsFilter;
			var result = new ZQuery();
			var shipmentTypes = new CodeDescriptionPairList(OLookUpEditType.ShipmentType);
			var selectedCodes = new List<string>();

			foreach (var name in filter.FlagNames)
			{
				if (filter[name])
				{
					var code = shipmentTypes.GetCodeFromDescription(name);
					if (!string.IsNullOrEmpty(code))
					{
						selectedCodes.Add(code);
					}
				}
			}

			if (selectedCodes.Any())
			{
				return result.AddToFilter(XQueryFilterHelper.GenerateXQuery((column) => new ZQuery(column, selectedCodes), new XQueryFilterInfo(ShipmentXQueryPaths.ShipmentType, JobShipmentSchema.JS_ShipmentType.MaxLength)));
			}

			return result;
		}

		#endregion

		#region GetPodQuery

		ZQuery GetPodQuery(ZString value)
		{
			var podQuery = new ZDBOnlyQuery(typeof(CommonShipment));

			if (value != PodFilterItems.All)
			{
				var isOpen = value == PodFilterItems.Open;

				var airQuery = CreateAirPODSubQuery(isOpen);
				podQuery.AddSubQuery(airQuery, isOpen ? JoinCondition.And : JoinCondition.Or);
				var nonAirQuery = CreateNonAirPODSubQuery(isOpen);
				podQuery.AddSubQuery(nonAirQuery, isOpen ? JoinCondition.And : JoinCondition.Or);
			}

			return podQuery;
		}

		ZDBOnlySubQuery CreateAirPODSubQuery(bool isOpen)
		{
			var query = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK, notIn: isOpen);
			query.AddToFilter(JoinCondition.And, JobShipmentSchema.JS_PackingMode, SQLComparisonOperator.NotEqual, Constants.ContainerModes.FCL);
			query.AddToFilter(JoinCondition.And, JobShipmentSchema.JS_PackingMode, SQLComparisonOperator.NotEqual, Constants.ContainerModes.BuyersConsol);
			query.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_TransportMode, SQLComparisonOperator.Equal, Constants.TransportModes.Air);

			var jobDocsSubQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
			jobDocsSubQuery.AddToFilter(JobDocsAndCartageSchema.JP_DeliveryCartageCompleted, SQLComparisonOperator.NotEqual, null);
			query.AddSubQuery(jobDocsSubQuery, JoinCondition.And);

			var packlinesSubQuery = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.JL_JS);
			var transportPacklineDivotSubQuery = new ZDBOnlySubQuery(typeof(CommonConfirmDivot), JobTransportLegPackLineDivotSchema.J8_JL);
			var pickupDeliverySubQuery = new ZDBOnlySubQuery(typeof(CommonPickupDeliveryConfirm), JobPickupDeliveryConfirmSchema.PK);
			pickupDeliverySubQuery.AddToFilter(JobPickupDeliveryConfirmSchema.EU_GoodsSignForBy, SQLComparisonOperator.NotEqual, ZString.Empty);
			pickupDeliverySubQuery.AddToFilter(JobPickupDeliveryConfirmSchema.EU_PickupDeliveryTime, SQLComparisonOperator.NotEqual, null);
			pickupDeliverySubQuery.AddToFilter(JobPickupDeliveryConfirmSchema.EU_PickupDeliveryType, Constants.PickupDeliveryConfirmTypes.DestinationDelivery);
			transportPacklineDivotSubQuery.AddSubQuery(JobTransportLegPackLineDivotSchema.J8_EU_PickupDeliverConfirm, pickupDeliverySubQuery, JoinCondition.And);
			packlinesSubQuery.AddSubQuery(transportPacklineDivotSubQuery, JoinCondition.And);
			query.AddSubQuery(packlinesSubQuery, JoinCondition.And);
			return query;
		}

		ZDBOnlySubQuery CreateNonAirPODSubQuery(bool isOpen)
		{
			var query = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK, notIn: isOpen);
			query.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_PackingMode, SQLComparisonOperator.Equal, Constants.ContainerModes.FCL);
			query.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_PackingMode, SQLComparisonOperator.Equal, Constants.ContainerModes.BuyersConsol);
			query.AddToFilter(JoinCondition.And, JobShipmentSchema.JS_TransportMode, SQLComparisonOperator.NotEqual, Constants.TransportModes.Air);

			var jobDocsSubQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
			query.AddSubQuery(jobDocsSubQuery, JoinCondition.And);

			var packlinesSubQuery = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.JL_JS);
			var containerPacklineDivotSubQuery = new ZDBOnlySubQuery(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JL);
			var pickupDeliverySubQuery = new ZDBOnlySubQuery(typeof(CommonPickupDeliveryConfirm), JobPickupDeliveryConfirmSchema.EU_JC);
			pickupDeliverySubQuery.AddToFilter(JobPickupDeliveryConfirmSchema.EU_GoodsSignForBy, SQLComparisonOperator.NotEqual, ZString.Empty);
			pickupDeliverySubQuery.AddToFilter(JobPickupDeliveryConfirmSchema.EU_PickupDeliveryTime, SQLComparisonOperator.NotEqual, null);
			pickupDeliverySubQuery.AddToFilter(JobPickupDeliveryConfirmSchema.EU_PickupDeliveryType, Constants.PickupDeliveryConfirmTypes.DestinationDelivery);
			containerPacklineDivotSubQuery.AddSubQuery(JobContainerPackPivotSchema.J6_JC, pickupDeliverySubQuery, JoinCondition.And);
			packlinesSubQuery.AddSubQuery(containerPacklineDivotSubQuery, JoinCondition.And);
			query.AddSubQuery(packlinesSubQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region GetShowForwardRegisteredAndBookingQuery

		ZQuery GetShowForwardRegisteredQuery(ZBool thisValueIsIgnored)
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, ZBool.True);

			return result;
		}

		#endregion

		#region GetOnlyShowTranshipmentsQuery

		ZQuery GetOnlyShowTranshipmentsQuery(ZBool onlyShowTranshipments)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			if (onlyShowTranshipments)
			{
				ZDBOnlySubQuery originSubQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
				originSubQuery.AddToFilter(JobVoyOriginSchema.JA_RL_NKPortOfLoading, SQLComparisonOperator.StartsWith, CurrentCountry);

				ZDBOnlySubQuery destinationSubQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
				destinationSubQuery.AddToFilter(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, SQLComparisonOperator.StartsWith, CurrentCountry);

				ZDBOnlySubQuery sailingQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
				sailingQuery.AddSubQuery(originSubQuery, JoinCondition.And);
				sailingQuery.AddSubQuery(destinationSubQuery, JoinCondition.Or);

				ZDBOnlySubQuery transportQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
				transportQuery.AddSubQuery(JobConsolTransportSchema.JW_JX, sailingQuery, JoinCondition.And);

				ZDBOnlySubQuery consolSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConShipLinkSchema.JN_JK);
				consolSubQuery.AddSubQuery(JobConsolSchema.PK, transportQuery, JoinCondition.And);

				ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
				pivotSubQuery.AddSubQuery(consolSubQuery, JoinCondition.And);

				result.AddToFilter(JobShipmentSchema.JS_RL_NKOrigin, SQLComparisonOperator.DoesNotStartWith, CurrentCountry);
				result.AddToFilter(JobShipmentSchema.JS_RL_NKDestination, SQLComparisonOperator.DoesNotStartWith, CurrentCountry);

				result.AddSubQuery(pivotSubQuery, JoinCondition.And);
			}

			return result;
		}

		#endregion

		#region GetOnlyShowCrossTradeShipmentsQuery

		ZQuery GetOnlyShowCrossTradeShipmentsQuery(ZBool onlyShowCrossTradeShipments)
		{
			ZQuery result;

			if (onlyShowCrossTradeShipments)
			{
				ZDBOnlySubQuery origins = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobVoyOriginSchema.PK);
				origins.AddToFilter(JobVoyOriginSchema.JA_RL_NKPortOfLoading, SQLComparisonOperator.StartsWith, CurrentCountry);

				ZDBOnlySubQuery destinations = new ZDBOnlySubQuery(typeof(VoyageDestination), JobVoyDestinationSchema.PK);
				destinations.AddToFilter(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, SQLComparisonOperator.StartsWith, CurrentCountry);

				ZDBOnlySubQuery sailings = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
				sailings.AddSubQuery(JobSailingSchema.JX_JA, origins, JoinCondition.Or);
				sailings.AddSubQuery(JobSailingSchema.JX_JB, destinations, JoinCondition.Or);

				ZDBOnlySubQuery transportQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
				transportQuery.AddSubQuery(JobConsolTransportSchema.JW_JX, sailings, JoinCondition.And);

				ZDBOnlySubQuery consols = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConsolSchema.PK);
				consols.AddSubQuery(JobConsolSchema.PK, transportQuery, JoinCondition.And);

				ZDBOnlyQuery conShipLink = new ZDBOnlyQuery(typeof(JobConShipLink));
				conShipLink.AddSubQuery(JobConShipLinkSchema.JN_JK, consols, JoinCondition.And);

				string sQL = @"((select count(*) from " +
					JobConShipLinkSchema.Constants.SqlSchemaName + "." + JobConShipLinkSchema.Constants.TableName +
					" where " + JobConShipLinkSchema.Constants.JN_JS +
					" = " + JobShipmentSchema.Constants.PK +
					" and " + conShipLink.LiteralTextADO +
					") = 0) ";

				result = new ZDBOnlyQuery(typeof(JobConShipLink));
				result.AddToFilter(JobShipmentSchema.JS_RL_NKOrigin, SQLComparisonOperator.DoesNotStartWith, CurrentCountry);
				result.AddToFilter(JobShipmentSchema.JS_RL_NKDestination, SQLComparisonOperator.DoesNotStartWith, CurrentCountry);
				result.AddFilterAndZSQLParameterCollection(sQL, null);
			}
			else
			{
				result = new ZQuery(JobShipmentSchema.JS_IsForwardRegistered, ZBool.True);
			}

			return result;
		}

		#endregion

		#region GetOnlyShowShipmentsWithoutConsolsQuery

		ZQuery GetOnlyShowShipmentsWithoutConsolsQuery(ZBool onlyShowShipmentsWithoutConsols)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			if (onlyShowShipmentsWithoutConsols)
			{
				string sQL = "NOT EXISTS " +
					"(SELECT * FROM " + JobConShipLinkSchema.Constants.SqlSchemaName + "." + JobConShipLinkSchema.Constants.TableName +
					" WHERE " + JobConShipLinkSchema.Constants.JN_JS + " = " + ForwardingShipment.Schema.PK + ")";

				result.AddFilterAndZSQLParameterCollection(sQL, null);
			}

			return result;
		}

		#endregion

		#region SupplyChainSecurityConfiguration

		SupplyChainSecurityConfiguration SupplyChainSecurityConfiguration
		{
			get { return supplyChainSecurityConfiguration ?? (supplyChainSecurityConfiguration = SupplyChainSecurityConfiguration.New()); }
		}
		SupplyChainSecurityConfiguration supplyChainSecurityConfiguration;

		#endregion

		#region GetIsHazardous

		ZQuery GetIsHazardous(ZBool isHazardous)
		{
			var isHazardousQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));

			isHazardousQuery.AddSubQuery(
				ForwardingShipmentQueryHelper.GetHazardousShipmentsAgainstQuery(JobShipmentSchema.PK, isHazardous),
				JoinCondition.And);
			isHazardousQuery.AddSubQuery(
				ForwardingShipmentQueryHelper.GetHazardousShipmentsAgainstQuery(JobShipmentSchema.JS_JS_ColoadMasterShipment, isHazardous),
				isHazardous ? JoinCondition.Or : JoinCondition.And);

			return isHazardousQuery;
		}

		#endregion

		#region GetHasDamagedPackages

		ZQuery GetHasDamagedPackages(ZBool hasDamagedPackages)
		{
			var packLineFilter = new ZDBOnlySubQuery(typeof(ForwardingPackLine), JobPackLinesSchema.JL_JS);
			packLineFilter.AddToFilter(JobPackLinesSchema.JL_FreightMode, FreightConstants.OuterPackType);
			packLineFilter.AddToFilter(JobPackLinesSchema.JL_Damaged, SQLComparisonOperator.GreaterThan, 0);

			var shipmentFilter = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK, notIn: !hasDamagedPackages);
			shipmentFilter.AddSubQuery(packLineFilter, JoinCondition.And);

			var hasDamagedPackagesQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
			hasDamagedPackagesQuery.AddSubQuery(shipmentFilter, JoinCondition.And);
			return hasDamagedPackagesQuery;
		}

		#endregion

		#region GetHasPillagedPackages

		ZQuery GetHasPillagedPackages(ZBool hasPillagedPackages)
		{
			var packLineFilter = new ZDBOnlySubQuery(typeof(ForwardingPackLine), JobPackLinesSchema.JL_JS);
			packLineFilter.AddToFilter(JobPackLinesSchema.JL_FreightMode, FreightConstants.OuterPackType);
			packLineFilter.AddToFilter(JobPackLinesSchema.JL_Pillaged, SQLComparisonOperator.GreaterThan, 0);

			var shipmentFilter = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK, notIn: !hasPillagedPackages);
			shipmentFilter.AddSubQuery(packLineFilter, JoinCondition.And);

			var hasPillagedPackagesQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
			hasPillagedPackagesQuery.AddSubQuery(shipmentFilter, JoinCondition.And);
			return hasPillagedPackagesQuery;
		}

		#endregion

		#region GetOriginTransitWarehouseStatus

		ZQuery GetOriginTransitWarehouseStatus(ZString originTransitWarehouseStatus)
		{
			var packLineFilter = new ZDBOnlySubQuery(typeof(ForwardingPackLine), JobPackLinesSchema.JL_JS);
			packLineFilter.AddToFilter(JobPackLinesSchema.JL_FreightMode, FreightConstants.OuterPackType);
			packLineFilter.AddToFilter(JobPackLinesSchema.JL_OriginTransitWarehouseStatus, originTransitWarehouseStatus);

			var shipmentFilter = new ZDBOnlyQuery(typeof(ForwardingShipment));
			shipmentFilter.AddSubQuery(packLineFilter, JoinCondition.And);
			return shipmentFilter;
		}

		#endregion

		#region GetExitStatusQuery

		ZQuery GetExitStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			ZString sqlString;
			var sqlParams = new ZSqlParameterCollection();
			if (!value.EqualsIgnoringCase(CommonEntryStatusList.Codes.MultipleEntryStatus))
			{
				sqlString = string.Format(@"JS_PK IN (SELECT CXH_ParentID FROM dbo.CusExitHeader INNER JOIN dbo.CusExitReport ON CXH_PK = CER_CXH_Header AND CXH_ParentTableCode = 'JS' WHERE {0})", SQLAndParametersForOneCondition(comparisonOperator, value, CusExitReportSchema.CER_Status, sqlParams));
			}
			else
			{
				sqlString = string.Format(@"JS_PK IN (SELECT CXH_ParentID FROM dbo.CusExitHeader WHERE {0} AND CXH_PK IN (SELECT CER_CXH_Header FROM dbo.CusExitReport GROUP BY CER_CXH_Header HAVING COUNT(DISTINCT(CER_Status)) > 1))", SQLAndParametersForOneCondition(SQLComparisonOperator.Equal, JobShipmentSchema.Constants.Prefix, CusExitHeaderSchema.CXH_ParentTableCode, sqlParams));
			}
			result.AddFilterAndZSQLParameterCollection(sqlString, sqlParams);

			return result;
		}

		protected internal static string SQLAndParametersForOneCondition(SQLComparisonOperator @operator, ZString value, SchemaStringColumn column, ZSqlParameterCollection sqlParameters)
		{
			var query = new ZQuery();
			query.AddToFilter_PossiblyCommaSeparated(column, @operator, value);
			var result = "(" + query.FilterString + ")";
			var counter = 0;
			foreach (var param in query.Params)
			{
				var oldName = param.ParameterName;
				param.Rename("@" + column.Name.Replace("_", "q") + "_" + (counter++));
				result = result.Replace(oldName, param.ParameterName);
				sqlParameters.Add(param);
			}
			return result;
		}

		#endregion

		#endregion

		#region Workflow Filters

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();

			if (ShouldAddWorkflowFilters)
			{
				var workflowHelper = new WorkflowFilterStripsHelperWithRoutingSupport(typeof(ForwardingShipment), JobInvoicingConsumerTypes.Shipment.Code, Factory);
				workflowHelper.SetShouldAddWorkflowCustomFieldsFilters(ShouldAddWorkflowCustomFieldsFilters);
				helpers.Add(workflowHelper);

				var linkSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
				var relatedWorkflowFilterStripsHelper = new WorkflowFilterStripsHelperWithRoutingSupport(typeof(ForwardingShipment), JobInvoicingConsumerTypes.Consol.Code, JobConShipLinkSchema.JN_JK, JobConsolSchema.Constants.Prefix, Factory, linkSubQuery)
				{
					ShouldAddMilestoneFilters = false,
					ShouldAddRelatedMilestoneFilters = true,
					ShouldAddMiscFilters = false
				};
				relatedWorkflowFilterStripsHelper.SetShouldAddWorkflowCustomFieldsFilters(false);
				helpers.Add(relatedWorkflowFilterStripsHelper);
			}

			helpers.Add(new ShipmentRegistryCustomFieldsFilterStripHelper());

			return helpers;
		}

		protected virtual bool ShouldAddWorkflowCustomFieldsFilters => true;

		protected virtual bool ShouldAddWorkflowFilters => true;

		#endregion

		#region ModuleFilter Lists

		BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		CodeDescriptionPairList DomesticInternationalList
		{
			get
			{
				if (fDomesticInternationalList == null)
				{
					fDomesticInternationalList = new CodeDescriptionPairList();
					fDomesticInternationalList.AddPair(DomesticInternationalFilterItems.All, Res.GetString("Forwarding|JobShipmentFilter|InternationalAndDomesticShipments", "International and Domestic Shipments"));
					fDomesticInternationalList.AddPair(DomesticInternationalFilterItems.International, Res.GetString("Forwarding|JobShipmentFilter|InternationalShipmentsOnly", "International Shipments Only"));
					fDomesticInternationalList.AddPair(DomesticInternationalFilterItems.Domestic, Res.GetString("Forwarding|JobShipmentFilter|DomesticShipmentsOnly", "Domestic Shipments Only"));
				}

				return fDomesticInternationalList;
			}
		}

		public static class DomesticInternationalFilterItems
		{
			public const string All = "ALL";
			public const string International = "INT";
			public const string Domestic = "DOM";
		}

		CodeDescriptionPairList fDomesticInternationalList;

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

		public CodeDescriptionPairList CoLoadStatus_List
		{
			get
			{
				if (fCoLoadStatus_List == null)
				{
					fCoLoadStatus_List = new CodeDescriptionPairList();
					fCoLoadStatus_List.Add(new CodeDescriptionPair(FreightConstants.CoLoadStatus.All, Res.GetString("Forwarding|JobShipmentFilter|ShowAllShipments", "Show All Shipments")));
					fCoLoadStatus_List.Add(new CodeDescriptionPair(FreightConstants.CoLoadStatus.CoLoad, Res.GetString("Forwarding|JobShipmentFilter|OnlyShowCoLoadShipments", "Only Show Co-Load Shipments")));
					fCoLoadStatus_List.Add(new CodeDescriptionPair(FreightConstants.CoLoadStatus.CoLoadMaster, Res.GetString("Forwarding|JobShipmentFilter|OnlyShowCoLoadMasterShipments", "Only Show Co-Load Master Shipments")));
					fCoLoadStatus_List.Add(new CodeDescriptionPair(FreightConstants.CoLoadStatus.CoLoadAndCoLoadMaster, Res.GetString("Forwarding|JobShipmentFilter|ShowAllCoLoadAndCoLoadMasterShipments", "Show all Co-Load and Co-Load Master Shipments")));
					fCoLoadStatus_List.Add(new CodeDescriptionPair(FreightConstants.CoLoadStatus.NeitherCoLoadNorCoLoadMaster, Res.GetString("Forwarding|JobShipmentFilter|HideAllCoLoadAndCoLoadMasterShipments", "Hide all Co-Load and Co-Load Master Shipments")));
				}
				return fCoLoadStatus_List;
			}
		}

		#region PodFilterList

		CodeDescriptionPairList PodFilterList
		{
			get
			{
				if (fPodList == null)
				{
					fPodList = new CodeDescriptionPairList();
					fPodList.Add(new CodeDescriptionPair(PodFilterItems.All, Res.GetString("Forwarding|JobShipmentFilter|ShowAllShipmentsDesc", "Show all shipments")));
					fPodList.Add(new CodeDescriptionPair(PodFilterItems.Open, Res.GetString("Forwarding|JobShipmentFilter|OnlyShowShipmentsWithoutAPOD", "Only show shipments without a POD")));
					fPodList.Add(new CodeDescriptionPair(PodFilterItems.Closed, Res.GetString("Forwarding|JobShipmentFilter|OnlyShowShipmentsWithAPOD", "Only show shipments with a POD")));
				}

				return fPodList;
			}
		}

		public static class PodFilterItems
		{
			public const string All = "ALL";
			public const string Open = "OPEN";
			public const string Closed = "CLOSED";
		}

		CodeDescriptionPairList fPodList;

		#endregion

		public GlbStaffCollection JS_StaffFilter_List
		{
			get
			{
				if (fJS_StaffFilter_List == null)
				{
					fJS_StaffFilter_List = new GlbStaffCollection(Factory);
				}
				return fJS_StaffFilter_List;
			}
		}

		public CodeDescriptionPairList JS_TransportMode_List
		{
			get { return FreightCodePairLists.JS_TransportModeList(); }
		}

		public CodeDescriptionPairList DropMode_List
		{
			get { return Factory.GetCachedValue("JobShipmentFilterBusinessObject|DropModeList", () => new CombinedEquipmentNeededList()); }
		}

		CodeDescriptionPairList ConsolMode_List
		{
			get { return consolMode_List ?? (consolMode_List = FreightCodePairLists.ConsolModeList(string.Empty, string.Empty)); }
		}
		CodeDescriptionPairList consolMode_List;

		CodeDescriptionPairList JS_PackingMode_List
		{
			get { return packingMode_List ?? (packingMode_List = FreightCodePairLists.JS_PackingModeList(string.Empty)); }
		}
		CodeDescriptionPairList packingMode_List;

		CodeDescriptionPairList ConsolTransportModes
		{
			get { return consolTransportModes ?? (consolTransportModes = FreightCodePairLists.LinkableTransportModeList()); }
		}
		CodeDescriptionPairList consolTransportModes;

		public CodeDescriptionPairList JS_ShipmentType_List
		{
			get { return FreightCodePairLists.JS_ShipmentTypeList(); }
		}

		CodeDescriptionPairList PhaseList
		{
			get
			{
				if (phaseList == null)
				{
					phaseList = PhaseConstants.GetCommonPhaseList();
					IPhaseSecurity phaseSecurity = ForwardingConfigurationRegistry.Instance.ShipmentPhaseSecurity.Value;
					if (phaseSecurity != null)
					{
						foreach (IPhase phase in phaseSecurity.Phases)
						{
							phaseList.AddPairIfNotExist(phase.Code, phase.Description);
						}
					}
				}

				return phaseList;
			}
		}
		CodeDescriptionPairList phaseList;

		CodeDescriptionPairList IncoTermList
		{
			get { return incoTermList ?? (incoTermList = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms)); }
		}

		CodeDescriptionPairList incoTermList;

		CodeDescriptionPairList fCoLoadStatus_List;
		GlbStaffCollection fJS_StaffFilter_List;
		IWhsWarehouseCollection warehouseList;

		CodeDescriptionPairList CTStatusList
		{
			get { return ctStatusList ?? (ctStatusList = new ExportCommunityTransitStatusList()); }
		}

		CodeDescriptionPairList ctStatusList;

		CodeDescriptionPairList ExitStatusCodesList
		{
			get
			{
				return Factory.GetCachedValue("StatusCodesList_EU", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(CustomsUniversal.ZZRefCusCodeListCombined.Loader.Load(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
							Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, ZDate.Today)
						.OrderBy(x => x.ZZD_Code).ToArray());
					return result;
				});
			}
		}

		#endregion

		#region Atttribute Filters

		public OrgHeader LoggedInWebUsersOrg
		{
			get { return fLoggedInWebUsersOrg; }
			set { fLoggedInWebUsersOrg = value; }
		}
		OrgHeader fLoggedInWebUsersOrg;

		ZQuery GetAttributeFilter(ZQuery filter, SchemaColumn column)
		{
			if (column.TableSchema == JobOrderHeaderSchema.Instance)
			{
				return GetOrderHeaderQuery(filter);
			}
			else if (column.TableSchema == JobOrderLineSchema.Instance)
			{
				return GetOrderLineQuery(filter);
			}
			else if (column.TableSchema == JobComInvoiceLineSchema.Instance)
			{
				return GetCommercialInvoiceQuery(filter);
			}

			throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "{0} schema is not supported", column.TableSchema));
		}

		protected virtual ZQuery GetOrderLineQuery(ZQuery filter)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			ZDBOnlySubQuery orderHeaderQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.JD_JS);
			ZDBOnlySubQuery orderLineQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.JO_JD);
			orderLineQuery.AddToFilter(filter);
			orderHeaderQuery.AddSubQuery(orderLineQuery, JoinCondition.And);
			result.AddSubQuery(orderHeaderQuery, JoinCondition.And);

			return result;
		}

		protected virtual ZQuery GetOrderHeaderQuery(ZQuery filter)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			ZDBOnlySubQuery orderQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.JD_JS);
			orderQuery.AddToFilter(filter);
			result.AddSubQuery(orderQuery, JoinCondition.And);

			return result;
		}

		protected virtual ZQuery GetCommercialInvoiceQuery(ZQuery invoiceLineFilter)
		{
			ZDBOnlySubQuery jobShipmentSubQuery = ObjectFactory.Get<ICustomsFilterProvider>().GetJobDeclarationFromCommercialInvoiceQueryWithLineFilterAttachedToJobShipment(invoiceLineFilter);
			ZDBOnlyQuery shipmentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
			shipmentQuery.AddSubQuery(jobShipmentSubQuery, JoinCondition.And);
			return shipmentQuery;
		}

		#endregion

		#region Add Customs Filters

		void AddCustomsFilter(ModuleFilterCollection filters)
		{
			ObjectFactory.Get<Enterprise.Integration.Customs.IForwardingShipmentModuleCustomColumnsAndFiltersProvider>().AddFilters(filters, Factory);
		}

		#endregion

		#region Add Pickup/Delivery Drop Mode Filters

		void AddPickUpDeliveryDropModeFilters(ModuleFilterCollection filters)
		{
			var pickupDropModeFilter = filters.AddTextFilter("Pickup Drop Mode", JobDocsAndCartageSchema.JP_FCLPickupEquipmentNeeded, DropMode_List);
			pickupDropModeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|PickupDropMode", "Pickup Drop Mode");
			pickupDropModeFilter.Category = FilterCategories.ModesAndTypes;
			pickupDropModeFilter.SubGroup = JobDocsAndCartageSubGroup;

			var deliveryDropModeFilter = filters.AddTextFilter("Delivery Drop Mode", JobDocsAndCartageSchema.JP_FCLDeliveryEquipmentNeeded, DropMode_List);
			deliveryDropModeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|DeliveryDropMode", "Delivery Drop Mode");
			deliveryDropModeFilter.Category = FilterCategories.ModesAndTypes;
			deliveryDropModeFilter.SubGroup = JobDocsAndCartageSubGroup;
		}

		#endregion

		#region Related Consolidations

		void AddRelatedConsolidationsFilter(ModuleFilterCollection filters)
		{
			var filter = new ConsolsOfShipmentFilter(Descriptions.RelatedConsols, () => new ForwardingConsolCollection(Factory));
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|RelatedConsols", "Related Consolidations");
			filter.IsPublishedOnWeb = false;

			var description = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|Consols", "Consols");
			filter.Category = FilterCategories.GetOrCreateFilterCategory(description);

			filters.AddFilter(filter);
		}

		#endregion

		#region Related Transport Bookings

		void AddRelatedTransportBookingsFilter(ModuleFilterCollection filters)
		{
			var filter = new RelatedTransportBookingsOfShipmentFilter(Descriptions.RelatedTransportBookings, () => ObjectFactory.Get<IDtbBookingCollection>(nameof(IDtbBookingCollection), Factory));
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|RelatedTransportBookings", "Related Transport Bookings");
			filter.IsPublishedOnWeb = false;

			var description = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|TransportBookings", "Transport Bookings");
			filter.Category = FilterCategories.GetOrCreateFilterCategory(description);

			filters.AddFilter(filter);
		}

		#endregion

		#region Sub Groups

		ModuleFilterSubGroup SendingarnumerSubGroup
		{
			get { return sendingarnumerSubGroup ?? (sendingarnumerSubGroup = new SendingarnumerFilterSubGroup()); }
		}
		SendingarnumerFilterSubGroup sendingarnumerSubGroup;

		ModuleFilterSubGroup ConsolSubGroup
		{
			get { return consolSubGroup ?? (consolSubGroup = new ConsolFilterSubGroup()); }
		}
		ConsolFilterSubGroup consolSubGroup;

		ModuleFilterSubGroup DeclarationSubGroup
		{
			get { return declarationSubGroup ?? (declarationSubGroup = new DeclarationFilterSubGroup()); }
		}
		DeclarationFilterSubGroup declarationSubGroup;

		ModuleFilterSubGroup CommercialInvoiceSubGroup
		{
			get { return commercialInvoiceSubGroup ?? (commercialInvoiceSubGroup = new CommercialInvoiceFilterSubGroup(DeclarationSubGroup)); }
		}
		CommercialInvoiceFilterSubGroup commercialInvoiceSubGroup;

		ModuleFilterSubGroup OrderLineSubGroup
		{
			get { return orderLineSubGroup ?? (orderLineSubGroup = new OrderLineFilterSubGroup()); }
		}
		OrderLineFilterSubGroup orderLineSubGroup;

		ModuleFilterSubGroup WarehouseLocationSubGroup
		{
			get { return warehouseLocationSubGroup ?? (warehouseLocationSubGroup = new WarehouseLocationFilterSubGroup(PackLineSubGroup)); }
		}
		WarehouseLocationFilterSubGroup warehouseLocationSubGroup;

		ModuleFilterSubGroup PackLineSubGroup
		{
			get { return packLineSubGroup ?? (packLineSubGroup = new PackLineFilterSubGroup()); }
		}
		PackLineFilterSubGroup packLineSubGroup;

		ModuleFilterSubGroup JobDocsAndCartageSubGroup
		{
			get { return jobDocsAndCartageSubGroup ?? (jobDocsAndCartageSubGroup = new JobDocsAndCartageFilterSubGroup()); }
		}
		JobDocsAndCartageFilterSubGroup jobDocsAndCartageSubGroup;

		ModuleFilterSubGroup CarrierSubGroup
		{
			get { return carrierSubGroup ?? (carrierSubGroup = new CarrierFilterSubGroup()); }
		}
		CarrierFilterSubGroup carrierSubGroup;

		ModuleFilterSubGroup BranchSubGroup
		{
			get { return branchSubGroup ?? (branchSubGroup = new BranchFilterSubGroup()); }
		}
		BranchFilterSubGroup branchSubGroup;

		ModuleFilterSubGroup OverseasAgentSubGroup
		{
			get { return overseasAgentSubGroup ?? (overseasAgentSubGroup = new OverseasAgentFilterSubGroup()); }
		}
		OverseasAgentFilterSubGroup overseasAgentSubGroup;

		ModuleFilterSubGroup JobHeaderSubGroup
		{
			get { return jobHeaderSubGroup ?? (jobHeaderSubGroup = new JobHeaderFilterSubGroup()); }
		}
		JobHeaderFilterSubGroup jobHeaderSubGroup;

		class SendingarnumerFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(ForwardingShipment));

				var cusSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				cusSubQuery.AddToFilter(filter);
				cusSubQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, JobShipmentSchema.Constants.TableName);
				cusSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Iceland.CRN);
				cusSubQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				result.AddSubQuery(cusSubQuery, JoinCondition.And);

				return result;
			}
		}

		class ConsolFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(ForwardingShipment));

				var pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
				var consolSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConShipLinkSchema.JN_JK);

				consolSubQuery.AddToFilter(filter);
				pivotSubQuery.AddSubQuery(consolSubQuery, JoinCondition.And);
				result.AddSubQuery(pivotSubQuery, JoinCondition.And);

				return result;
			}
		}

		class DeclarationFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
				var declarationQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.JE_JS);
				declarationQuery.AddToFilter(filter);

				result.AddSubQuery(declarationQuery, JoinCondition.And);

				return result;
			}
		}

		class CommercialInvoiceFilterSubGroup : ModuleFilterSubGroup
		{
			public CommercialInvoiceFilterSubGroup(ModuleFilterSubGroup parent)
				: base(parent) { }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var invoiceHeaderQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
				invoiceHeaderQuery.AddToFilter(filter);

				var declarationQuery = new ZDBOnlyQuery(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration));
				declarationQuery.AddSubQuery(invoiceHeaderQuery, JoinCondition.And);

				return declarationQuery;
			}
		}

		class OrderLineFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(ForwardingShipment));
				var orderSubQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.JD_JS);
				var orderLineSubQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.JO_JD);

				orderLineSubQuery.AddToFilter(filter);
				orderSubQuery.AddSubQuery(orderLineSubQuery, JoinCondition.And);
				query.AddSubQuery(orderSubQuery, JoinCondition.And);

				return query;
			}
		}

		public class PackLineFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
				var subQuery = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.JL_JS);

				subQuery.AddToFilter(filter);
				result.AddSubQuery(subQuery, JoinCondition.And);

				return result;
			}
		}

		class WarehouseLocationFilterSubGroup : ModuleFilterSubGroup
		{
			public WarehouseLocationFilterSubGroup(ModuleFilterSubGroup parent)
				: base(parent) { }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var locationFilter = new ZDBOnlySubQuery(typeof(PackLocation), JobPackLocSchema.JQ_JL);
				locationFilter.AddToFilter(filter);

				var packlineFilter = new ZDBOnlyQuery(typeof(PackLine));
				packlineFilter.AddSubQuery(locationFilter, JoinCondition.And);

				return packlineFilter;
			}
		}

		class JobDocsAndCartageFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var jobDocsAndCartageQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
				jobDocsAndCartageQuery.AddToFilter(filter);

				var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
				result.AddSubQuery(jobDocsAndCartageQuery, JoinCondition.And);

				return result;
			}
		}

		class CarrierFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(ForwardingShipment));

				var consolPivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
				var consolSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConShipLinkSchema.JN_JK);
				var consolOrgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobConsolSchema.JK_OA_ShippingLineAddress);

				consolOrgAddressQuery.AddToFilter(filter);
				consolSubQuery.AddSubQuery(consolOrgAddressQuery, JoinCondition.And);
				consolPivotSubQuery.AddSubQuery(consolSubQuery, JoinCondition.And);
				result.AddSubQuery(consolPivotSubQuery, JoinCondition.And);

				var shipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
				var shipmentOrgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobShipmentSchema.JS_OA_BookedShippingLineAddress);

				shipmentOrgAddressQuery.AddToFilter(filter);

				var notInConsolPivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS, true);
				var notInConsolSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConShipLinkSchema.JN_JK);

				notInConsolSubQuery.AddToFilter(new ZQuery(JobConsolSchema.JK_OA_ShippingLineAddress, SQLComparisonOperator.NotEqual, null));
				notInConsolPivotSubQuery.AddSubQuery(notInConsolSubQuery, JoinCondition.And);

				shipmentSubQuery.AddSubQuery(shipmentOrgAddressQuery, JoinCondition.And);
				shipmentSubQuery.AddSubQuery(notInConsolPivotSubQuery, JoinCondition.And);

				result.AddSubQuery(shipmentSubQuery, JoinCondition.Or);

				return result;
			}
		}

		class BranchFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
				var subQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);

				subQuery.AddToFilter(filter);
				subQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				result.AddSubQuery(subQuery, JoinCondition.And);

				return result;
			}
		}

		class OverseasAgentFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(CommonShipment));
				var subQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
				var addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_AgentCollectAddr);

				addressQuery.AddToFilter(filter);
				subQuery.AddSubQuery(addressQuery, JoinCondition.And);
				subQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return query;
			}
		}

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

		#endregion

		#region Implementation

		string CurrentCountry
		{
			get { return GlbBranch.CurrentBranch.GB_RL_NKHomePort.SubstringSafe(0, RefCountry.Schema.RN_CodeMaxLength); }
		}

		ZDBOnlyQuery GetConsolQuery(SchemaColumn consolSchemaColumn, SQLComparisonOperator @operator, IZType value)
		{
			bool isNotQuery = @operator.IsNegativeSQLOperator();

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS, isNotQuery);
			ZDBOnlySubQuery consolSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConShipLinkSchema.JN_JK);

			var operatorForConsolSubQuery = isNotQuery ? @operator.GetNegatingSQLOperatorIfNotInSubquery() : @operator;
			consolSubQuery.AddToFilter_PossiblyCommaSeparated(consolSchemaColumn, operatorForConsolSubQuery, value);
			pivotSubQuery.AddSubQuery(consolSubQuery, JoinCondition.And);
			result.AddSubQuery(pivotSubQuery, JoinCondition.And);

			if (isNotQuery)
			{
				ZDBOnlyQuery hasConsolQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
				ZDBOnlySubQuery allPivotsSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
				hasConsolQuery.AddSubQuery(allPivotsSubQuery, JoinCondition.And);
				result.AddToFilter(hasConsolQuery, JoinCondition.And);
			}

			return result;
		}

		ZQuery FromDocAddressFilter(ZQuery docAddressFilter, bool notIn)
		{
			ZDBOnlySubQuery docAddress = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn);
			docAddress.AddToFilter(docAddressFilter);

			ZDBOnlyQuery shipment = new ZDBOnlyQuery(typeof(ForwardingShipment));
			shipment.AddSubQuery(docAddress, JoinCondition.And);

			return shipment;
		}

		ZQuery FromJobHeaderFilter(ZQuery jobHeaderFilter, bool notIn)
		{
			ZDBOnlySubQuery jobHeader = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID, notIn);
			jobHeader.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			jobHeader.AddToFilter(jobHeaderFilter);

			ZDBOnlyQuery shipment = new ZDBOnlyQuery(typeof(ForwardingShipment));
			shipment.AddSubQuery(jobHeader, JoinCondition.And);

			return shipment;
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
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			result.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return result;
		}

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new Dictionary<string, object>()
		{
			{ AccountingFilterStripConfigurationKeys.BusinessObjectType, typeof(ForwardingShipment) },
			{ AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride, ResString.GetMultilingualString("Forwarding|JobShipmentFilter|InvoiceStatus", "Invoice Status") }
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

		#region US Declaration Business Object Filter

		internal Enterprise.Integration.Customs.US.IJobDeclarationFilterBusinessObject USDeclarationFilter
		{
			get
			{
				if (fUSDeclarationFilter == null)
				{
					fUSDeclarationFilter = (Enterprise.Integration.Customs.US.IJobDeclarationFilterBusinessObject)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IJobDeclarationFilterBusinessObject>());
				}
				return fUSDeclarationFilter;
			}
		}
		Enterprise.Integration.Customs.US.IJobDeclarationFilterBusinessObject fUSDeclarationFilter;

		#endregion

		#region CRM Security

		readonly JobShipmentCRMSecurityProvider securityProvider = new JobShipmentCRMSecurityProvider();

		#endregion

		#region Doc Address Multi Value Queries

		ZQuery PickupAgentMultiValueQuery(object value, SQLComparisonOperator filterOperator) =>
			DocAddressMultiValueQuery(value, filterOperator, DocAddressTypes.Codes.PickupAgent);

		ZQuery ControllingAgentMultiValueQuery(object value, SQLComparisonOperator filterOperator) =>
			DocAddressMultiValueQuery(value, filterOperator, DocAddressTypes.Codes.ControllingAgent);

		ZQuery ControllingCustomerMultiValueQuery(object value, SQLComparisonOperator filterOperator) =>
			DocAddressMultiValueQuery(value, filterOperator, DocAddressTypes.Codes.ControllingCustomer);

		ZQuery ConsigneeMultiValueQuery(object value, SQLComparisonOperator filterOperator) =>
			DocAddressMultiValueQuery(value, filterOperator, DocAddressTypes.Codes.ConsigneeDocumentaryAddress);

		ZQuery ConsignorMultiValueQuery(object value, SQLComparisonOperator filterOperator) =>
			DocAddressMultiValueQuery(value, filterOperator, DocAddressTypes.Codes.ConsignorDocumentaryAddress);

		ZQuery DocAddressMultiValueQuery(object value, SQLComparisonOperator filterOperator, string docAddressType)
		{
			var result = new ZDBOnlyQuery(typeof(CommonShipment));

			if (value is List<ZGuid> orgList && orgList.Count > 0)
			{
				var docAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
				docAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, docAddressType);

				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				orgAddressQuery.AddToFilter(JoinCondition.Or, OrgAddressSchema.OA_OH, filterOperator, orgList);
				docAddressQuery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressQuery, JoinCondition.And);

				result.AddSubQuery(docAddressQuery, JoinCondition.And);
			}

			return result;
		}

		#endregion
	}
}
