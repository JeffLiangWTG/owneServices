using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Module;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;
using ForwardingShipment = Enterprise.Freight.Forwarding.Business.ForwardingShipment;

namespace Enterprise.Freight.Forwarding.Module
{
	public class JobConsolFilterBusinessObject : FilterStripBusinessObject, IAccountingFilterStripHolder, ITemplateRecordFilterProvider
	{
		public JobConsolFilterBusinessObject()
		{
		}

		public JobConsolFilterBusinessObject(bool allowTemplateRecords = false)
		{
			AllowTemplateRecords = allowTemplateRecords;
		}

		public static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string ConsolNum = "Consol #";

			public const string BookingReferenceNum = "Booking Reference #";
			public const string CoLoadMasterBillNum = "Co-Load Master Bill #";
			public const string CoLoadBookingReference = "Co-Load Booking Reference #";
			public const string ContainerNum = "Container #";
			public const string HouseBill = "House Bill";
			public const string MasterBill = "Master Bill";
			public const string ShipmentNum = "Shipment #";
			public const string FlightVoyageNumAndVessel = "Flight/Voyage # and Vessel";
			public const string SendingarNumber = "Sendingarnumer";
			public const string AdditionalReferenceNumbers = "Additional Reference #";
			public const string AgentReference = "Agent Reference #";
			public const string CarrierContractNumber = "Carrier Contract #";
			public const string AllocationID = "Allocation ID";

			public const string ETA = "ETA";
			public const string ATA = "ATA";
			public const string ETD = "ETD";
			public const string ATD = "ATD";
			public const string CutOffDate = "Cut Off Date";

			public const string ETALoad = "ETA / Load Port";
			public const string ATALoad = "ATA / Load Port";
			public const string ETDLoad = "ETD / Load Port";
			public const string ATDLoad = "ATD / Load Port";
			public const string ETADischarge = "ETA / Discharge Port";
			public const string ATADischarge = "ATA / Discharge Port";

			public const string SendingAgentType = "Sending Agent Type";
			public const string ReceivingAgentType = "Receiving Agent Type";

			public static string ConsignorConsignee
			{
				get { return FreightDataRegistry.Instance.ConsignorShipperTerminology.Value.GetUnresolvedString() + " / Consignee"; }
			}
			public const string SendReceiveAgents = "Send / Receive Agents";
			public const string Carrier = "Carrier";
			public const string SendingAgentRelatedParties = "Sending Agent Related Parties";
			public const string ReceivingAgentRelatedParties = "Receiving Agent Related Parties";
			public const string CarrierRelatedParties = "Carrier Related Parties";

			public const string EndPorts = "End Ports (First Load / Last Disch.)";
			public const string LoadDischarge = "Load / Discharge";
			public const string OriginDestination = "Origin / Destination";
			public const string CarrierBookingOffice = "Carrier Booking Office";

			public const string ContainerMode = "Container Mode";
			public const string ContainerType = "Container Type";
			public const string TransportMode = "Transport Mode";
			public const string CarrierServiceLevel = "Carrier Service Level";
			public const string GatewayServiceLevel = "Gateway Service Level";
			public const string PreAllocatedAmountExceeded = "Pre-Allocated Amount Exceeded";
			public const string ConsolType = "Consol Type";
			public const string DGClassDGSubstance = "DG Class / DG Substance";

			public const string RoutingStatus = "Routing Status";
			public const string HiddenLoadLists = "Hidden LoadLists Filter";
			public const string ConsolsWithoutShipments = "Consols without Shipments";
			public const string ConsolsWithShipments = "Consols with Shipments";
			public const string electronicBillStatus = "Electronic Bill Status";
			public const string electronicBillTerms = "Electronic Bill Terms";
			public const string electronicBillType = "Electronic Bill Type";
			public const string Phase = "Phase";
			public const string IsCargoOnly = "Is Cargo Only";
			public const string IsHazardous = "Is Hazardous";
			public const string VGMStatus = "VGM Status";
			public const string AirfreightSecurityStatus = "Airfreight Security Status";
			public const string AirBookingStatus = "Carrier Booking Status - Air";
			public const string FlightStatus = "Flight Status";
			public const string CarrierBookingStatus = "Carrier Booking Status - Ocean";
			public const string ReleaseType = "Release Type";
			public const string PossibleOversize = "Possible Oversize";

			public const string RelatedShipments = "Related Shipments";
			public const string RelatedShipmentsSecurity = "Related Shipments Security";
			public const string RelatedContainers = "Related Containers";
			public const string RelatedTransportLegs = "Related Transport Legs";

			public const string IsTemperatureControlled = "Is Temperature Controlled";

			public const string DepartureCTO = "Departure CTO";
			public const string DepartureCFS = "Departure CFS";
			public const string DepartureContainerYard = "Departure Container Yard";
			public const string DepartureTransportProvider = "Departure Transport Provider";

			public const string ArrivalCTO = "Arrival CTO";
			public const string ArrivalCFS = "Arrival CFS";
			public const string ArrivalContainerYard = "Arrival Container Yard";
			public const string ArrivalTransportProvider = "Arrival Transport Provider";

			public const string ArrivalCFSReceiptRequested = "Arrival CFS/TW / Receipt Requested Date";
			public const string DepartureCFSReceiptRequested = "Departure CFS/TW / Receipt Requested Date";
			public const string ArrivalCFSDispatchRequested = "Arrival CFS/TW / Dispatch Requested Date";
			public const string DepartureCFSDispatchRequested = "Departure CFS/TW / Dispatch Requested Date";

			public const string CO2e = "CO2e";
			public const string CTOStorageStart = "CTO Storage Start";
			public const string EmptyReturnReqBy = "Empty Return Req. By";

			#endregion
		}

		#region GetModuleFilters

		protected sealed override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = GetJobConsolModuleFiltersCore();
			AddRelatedContainersFilter(result);
			AddRelatedTransportLegsFilter(result);
			AddAccountingFilter(result);
			AddTemplateRecordFilter(result);
			ApplyTemplateRecordFiltersLayout(result);

			return result;
		}

		void AddAccountingFilter(ModuleFilterCollection filters)
		{
			AccountingFilterStrip.Initialize(
				addRevenueFilters: true,
				addWIPAccrualHasFilters: false,
				addSupplierCostReferenceFilters: true,
				addOrganisationFilters: true,
				addDateFilters: true,
				addAmountFilters: false,
				addNumbersAndReferencesFilters: false);

			AccountingFilterStrip.AddBillingFilters(filters);
			AccountingFilterStrip.AddJobManagementFilters(filters, Env.Security.GatewayConsolJobInvoicing);
		}

		protected virtual ModuleFilterCollection GetJobConsolModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();

			AddNumberFilters(result);
			AddDateFilters(result);
			AddOrganisationFilters(result);
			AddLocationFilters(result);
			AddModeFilters(result);
			AddStatusAndFlagsFilters(result);
			AddRelatedShipmentsFilters(result);
			AddRelatedShipmentsOSMGFilter(result);
			AddCustomsFilters(result);

			return result;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			helpers.Add(new WorkflowFilterStripsHelperWithRoutingSupport(typeof(ForwardingConsol), JobInvoicingConsumerTypes.Consol.Code, Factory));

			return helpers;
		}

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			ModuleFountainFilter consolNoFilter = new ModuleFountainFilter(Descriptions.ConsolNum, JobConsolSchema.JK_UniqueConsignRef, "C");
			consolNoFilter.IsCommon = true;
			consolNoFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|Consol", "Consol #");
			consolNoFilter.Prefix = "C";
			return consolNoFilter;
		}

		#endregion

		#region SubGroups

		public ModuleFilterSubGroup ShipmentFilterProcessor
		{
			get { return shipmentFilterProcessor ?? (shipmentFilterProcessor = new ShipmentSubGroup()); }
		}

		ShipmentSubGroup shipmentFilterProcessor;

		class ShipmentSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingConsol));

				ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JK);
				ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobConShipLinkSchema.JN_JS);

				shipmentSubQuery.AddToFilter(filter);

				pivotSubQuery.AddSubQuery(shipmentSubQuery, JoinCondition.And);
				result.AddSubQuery(pivotSubQuery, JoinCondition.And);

				return result;
			}
		}

		ModuleFilterSubGroup SendingarNumberFilterProcessor => sendingarNumberFilterProcessor ?? (sendingarNumberFilterProcessor = new SendingarNumberSubGroup());
		SendingarNumberSubGroup sendingarNumberFilterProcessor;

		class SendingarNumberSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(ForwardingConsol));
				var cusSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				cusSubQuery.AddToFilter(filter);
				cusSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Iceland.CRN);
				cusSubQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				result.AddSubQuery(cusSubQuery, JoinCondition.And);

				return result;
			}
		}

		ModuleFilterSubGroup ContainerNumFilterProcessor => containerNumFilterProcessor ?? (containerNumFilterProcessor = new ContainerNumSubGroup());
		ContainerNumSubGroup containerNumFilterProcessor;

		class ContainerNumSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(ForwardingConsol));

				var containerSubQuery = new ZDBOnlySubQuery(typeof(CommonContainer), JobContainerSchema.JC_JK);
				containerSubQuery.AddToFilter(filter, JoinCondition.And);
				result.AddSubQuery(containerSubQuery, JoinCondition.And);

				return result;
			}
		}

		ModuleFilterSubGroup ContainerTypeFilterProcessor => containerTypeFilterProcessor ?? (containerTypeFilterProcessor = new ContainerTypeSubGroup());
		ContainerTypeSubGroup containerTypeFilterProcessor;

		class ContainerTypeSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(ForwardingConsol));

				var containersQuery = new ZDBOnlySubQuery(typeof(ForwardingContainer), JobContainerSchema.JC_JK);
				var refContainerQuery = new ZDBOnlySubQuery(typeof(RefContainer), JobContainerSchema.JC_RC);
				refContainerQuery.AddToFilter(filter);
				containersQuery.AddSubQuery(refContainerQuery, JoinCondition.And);
				result.AddSubQuery(containersQuery, JoinCondition.And);

				return result;
			}
		}

		#endregion

		#region Number Filters

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddNumberFilter(Descriptions.BookingReferenceNum, GetBookingReferenceQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobConsolSchema.JK_BookingReference);
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|BookingReference", "Booking Reference #");
			filter.Prefix = "B";
			filter = filters.AddNumberFilter(Descriptions.CoLoadMasterBillNum, GetCoLoadMasterBillNoQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobConsolSchema.JK_CoLoadMasterBill);
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|CoLoadMasterBill", "Co-Load Master Bill #");
			filter.Prefix = "L";

			filter = filters.AddNumberFilter(Descriptions.CoLoadBookingReference, GetCoLoadBookingRefQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobConsolSchema.JK_CoLoadBookingReference);
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|CoLoadBookingReference", "Co-Load Booking Reference #");

			filter = filters.AddNumberFilter(Descriptions.ContainerNum, JobContainerSchema.JC_ContainerNum);
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|Container", "Container #");
			filter.SubGroup = ContainerNumFilterProcessor;
			filter.Prefix = "T";
			filter = filters.AddNumberFilter(Descriptions.HouseBill, JobShipmentSchema.JS_HouseBill);
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|HouseBill", "House Bill");
			filter.Prefix = "H";
			filter.SubGroup = ShipmentFilterProcessor;
			filter = filters.AddNumberFilter(Descriptions.AgentReference, GetAgentReferenceQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobConsolSchema.JK_AgentsReference);
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|AgentReference", "Agent Reference #");

			ModuleNumberFilter masterBillFilter = filters.AddNumberFilter(Descriptions.MasterBill, GetMasterBillQuery);
			masterBillFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|MasterBill", "Master Bill");
			masterBillFilter.IsCommon = true;
			masterBillFilter.Prefix = "M";
			masterBillFilter.MaxLength = GetMasterBillQuery(SQLComparisonOperator.Equal, "C66666666").GetMaxLengthOfUsedColumns(); // Need to send fake data

			filter = filters.AddFountainFilter(Descriptions.ShipmentNum, JobShipmentSchema.JS_UniqueConsignRef, "S");
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|Shipment", "Shipment #");
			filter.Prefix = "S";
			filter.SubGroup = ShipmentFilterProcessor;

			var flightVoyageNoFilter = new VoyageVesselModuleFilter(Descriptions.FlightVoyageNumAndVessel, GetVoyageVesselQuery, BindingLists.RefVessel_List)
				.WithMaxLengthOf(JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
			flightVoyageNoFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|FlightVoyageAndVessel", "Flight/Voyage # and Vessel");
			flightVoyageNoFilter.Category = FilterCategories.NumbersAndReferences;
			flightVoyageNoFilter.Prefix = "V";
			filters.AddCustomFilter(flightVoyageNoFilter);

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Iceland)
			{
				var sendingarNumberFilter = filters.AddTextFilter(Descriptions.SendingarNumber, CusEntryNumSchema.CE_EntryNum);
				sendingarNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|Sendingarnumer", "Sendingarnumer");
				sendingarNumberFilter.SubGroup = SendingarNumberFilterProcessor;
			}

			filter = filters.AddNumberFilter(Descriptions.CarrierContractNumber, GetCarrierContractQuery).WithMaxLengthOf<ModuleNumberFilter>(JobConsolSchema.JK_CarrierContractNumber);
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|CarrierContract", "Carrier Contract #");

			if (ContractsPermissions.IsAllocationsVisible())
			{
				filter = filters.AddNumberFilter(Descriptions.AllocationID, GetAllocationQuery);
				filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|AllocationID", "Allocation ID");
			}

			var referenceNumberFilter = new ReferenceNumberFilter(
				Descriptions.AdditionalReferenceNumbers,
				new ReferenceNumberFilterHelper<ForwardingConsol>().GetReferenceNumberFilter,
				new RefCountryCollection(Factory)
			).WithMaxLengthOf<ReferenceNumberFilter>(CusEntryNumSchema.CE_EntryNum);
			referenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ReferenceNumbers", "Additional Reference #");

			filters.AddCustomFilter(referenceNumberFilter);

			if (ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled)
			{
				var co2eFilter = new CO2eStatusAndCO2eKgRangeNumberFilter(Descriptions.CO2e, typeof(ForwardingConsol))
				{
					MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|CO2e", "CO2e (kg)"),
					Category = FilterCategories.NumbersAndReferences
				};
				filters.AddCustomFilter(co2eFilter);
			}
		}

		ZQuery GetAllocationQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingConsol));

			if (comparisonOperator == SQLComparisonOperator.IsBlank || comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				result.AddToFilter(JobConsolSchema.JK_RCA_AllocationLine, comparisonOperator == SQLComparisonOperator.IsBlank ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, null);
			}
			else
			{
				var allocationSubQuery = new ZDBOnlySubQuery(typeof(IRatingContractAllocationLine), JobConsolSchema.JK_RCA_AllocationLine);
				allocationSubQuery.AddToFilter_PossiblyCommaSeparated(RatingContractAllocationLineSchema.RCA_AllocationLineID, comparisonOperator, value);

				result.AddSubQuery(allocationSubQuery, JoinCondition.And);
			}

			return result;
		}

		#region GetBookingReferenceQuery

		ZQuery GetBookingReferenceQuery(SQLComparisonOperator @operator, ZString bookingReference)
		{
			ZQuery result = new ZQuery();

			result.AddToFilter_PossiblyCommaSeparated(JobConsolSchema.JK_BookingReference, @operator, bookingReference);

			return result;
		}

		#endregion

		#region GetAgentReferenceQuery

		ZQuery GetAgentReferenceQuery(SQLComparisonOperator @operator, ZString agentReference)
		{
			var result = new ZQuery();
			result.AddToFilter_PossiblyCommaSeparated(JobConsolSchema.JK_AgentsReference, @operator, agentReference);
			return result;
		}

		#endregion

		#region GetCoLoadMasterBillNoQuery

		ZQuery GetCoLoadMasterBillNoQuery(SQLComparisonOperator @operator, ZString coLoadMasterBillNo)
		{
			ZQuery bookingQuery = new ZQuery();
			ZQuery result = new ZQuery();

			ZString alteredNoLoadMasterBillNo = coLoadMasterBillNo.Replace(" ", "").Replace("-", "");
			bookingQuery.AddToFilter_PossiblyCommaSeparated(JobConsolSchema.JK_CoLoadMasterBill, @operator, coLoadMasterBillNo);
			if (alteredNoLoadMasterBillNo != coLoadMasterBillNo)
			{
				bookingQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.Or, JobConsolSchema.JK_CoLoadMasterBill, @operator, alteredNoLoadMasterBillNo);
			}

			result.AddToFilter(bookingQuery);
			result.AddToFilter(JoinCondition.And, JobConsolSchema.JK_AgentType, SQLComparisonOperator.Equal, Constants.AgentType.CoLoad);

			return result;
		}

		#endregion

		#region GetCoLoadBookingRefQuery

		ZQuery GetCoLoadBookingRefQuery(SQLComparisonOperator @operator, ZString coLoadBookingRef)
		{
			var result = new ZQuery();

			result.AddToFilter_PossiblyCommaSeparated(JobConsolSchema.JK_CoLoadBookingReference, @operator, coLoadBookingRef);
			result.AddToFilter(JoinCondition.And, JobConsolSchema.JK_AgentType, SQLComparisonOperator.Equal, Constants.AgentType.CoLoad);

			return result;
		}

		#endregion

		#region GetMasterBillQuery

		[SuppressMessage("Enterprise", "EDI003", Justification = "This is the property I want to use")]
		ZQuery GetMasterBillQuery(SQLComparisonOperator @operator, ZString masterBill)
		{
			ZQuery result = new ZQuery();

			ZString alteredMasterBill = masterBill.Replace(" ", "").Replace("-", "");
			result.AddToFilter_PossiblyCommaSeparated(JobConsolSchema.JK_MasterBillNum, @operator, masterBill);
			result.AddToFilter_PossiblyCommaSeparated(JoinCondition.Or, JobConsolSchema.JK_MasterBillNum, @operator, alteredMasterBill);

			return result;
		}

		#endregion

		#region GetFlightVoyageAndVesselQuery

		ZQuery GetVoyageVesselQuery(SQLComparisonOperator @operator, ZString voyageFlight, ZString vessel, ZBool includeArchived)
		{
			var builder = new SailingFilterBuilder(Factory);
			builder.Vessel = vessel;
			builder.VoyageFlight = voyageFlight;
			builder.VoyageFlightComparisonOperator = @operator;
			builder.IncludeArchived = includeArchived;

			return builder.ToConsolFilter();
		}

		#endregion

		#region GetCarrierContractQuery

		ZQuery GetCarrierContractQuery(SQLComparisonOperator @operator, ZString carrierContractNo)
		{
			var result = new ZQuery();
			result.AddToFilter_PossiblyCommaSeparated(JobConsolSchema.JK_CarrierContractNumber, @operator, carrierContractNo);
			return result;
		}

		#endregion

		#endregion

		#region Date Filters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(Descriptions.ETA, GetEtaQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ETA", "ETA");
			filters.AddDateFilter(Descriptions.ATA, GetAtaQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ATA", "ATA");
			filters.AddDateFilter(Descriptions.ETD, GetEtdQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ETD", "ETD");
			filters.AddDateFilter(Descriptions.ATD, GetAtdQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ATD", "ATD");
			filters.AddDateFilter(Descriptions.CutOffDate, JobConsolSchema.JK_ConsolCutOffDate, true).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|CutOffDate", "Cut Off Date");
			filters.AddDateFilter(Descriptions.CTOStorageStart, GetCTOStorageQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|CTOStorageStart", "CTO Storage Start");
			filters.AddDateFilter(Descriptions.EmptyReturnReqBy, GetEmptyReturnReqQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|EmptyReturnReqBy", "Empty Return Req. By");

			var etaLoadFilter = new DateLocationFilter(Descriptions.ETALoad, SailingFilterBuilder.Dates.LoadETA, BindingLists.RefLocation_List, DateLocationFilter.LocationTypes.Load, DateLocationFilter.TargetFilterTypes.Consol, Factory);
			etaLoadFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ETALoad", "ETA / Load Port");
			etaLoadFilter.Property3Validation = PortValidation;
			filters.AddCustomFilter(etaLoadFilter);

			var ataLoadFilter = new DateLocationFilter(Descriptions.ATALoad, SailingFilterBuilder.Dates.LoadATA, BindingLists.RefLocation_List, DateLocationFilter.LocationTypes.Load, DateLocationFilter.TargetFilterTypes.Consol, Factory);
			ataLoadFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ATALoad", "ATA / Load Port");
			ataLoadFilter.Property3Validation = PortValidation;
			filters.AddCustomFilter(ataLoadFilter);

			var etdLoadFilter = new DateLocationFilter(Descriptions.ETDLoad, SailingFilterBuilder.Dates.ETD, BindingLists.RefLocation_List, DateLocationFilter.LocationTypes.Load, DateLocationFilter.TargetFilterTypes.Consol, Factory);
			etdLoadFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ETDLoad", "ETD / Load Port");
			etdLoadFilter.Property3Validation = PortValidation;
			filters.AddCustomFilter(etdLoadFilter);

			var atdLoadFilter = new DateLocationFilter(Descriptions.ATDLoad, SailingFilterBuilder.Dates.ATD, BindingLists.RefLocation_List, DateLocationFilter.LocationTypes.Load, DateLocationFilter.TargetFilterTypes.Consol, Factory);
			atdLoadFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ATDLoad", "ATD / Load Port");
			atdLoadFilter.Property3Validation = PortValidation;
			filters.AddCustomFilter(atdLoadFilter);

			var etaDischargeFilter = new DateLocationFilter(Descriptions.ETADischarge, SailingFilterBuilder.Dates.ETA, BindingLists.RefLocation_List, DateLocationFilter.LocationTypes.Discharge, DateLocationFilter.TargetFilterTypes.Consol, Factory);
			etaDischargeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ETADischarge", "ETA / Discharge Port");
			etaDischargeFilter.Property3Validation = PortValidation;
			filters.AddCustomFilter(etaDischargeFilter);

			var ataDischargeFilter = new DateLocationFilter(Descriptions.ATADischarge, SailingFilterBuilder.Dates.ATA, BindingLists.RefLocation_List, DateLocationFilter.LocationTypes.Discharge, DateLocationFilter.TargetFilterTypes.Consol, Factory);
			ataDischargeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ATADischarge", "ATA / Discharge Port");
			ataDischargeFilter.Property3Validation = PortValidation;
			filters.AddCustomFilter(ataDischargeFilter);

			var cfsResourceString = Res.GetData("0b5af3f3-9042-4760-8de7-7b4b66b1f444", "CFS");

			var arrivalCFSReceiptRequested = new DateOrganizationFilter(Descriptions.ArrivalCFSReceiptRequested, JobConsolSchema.JK_UnpackDepotReceiptRequested, GetOrgAddressColumnQueryWithOperatorDelegate(JobConsolSchema.JK_OA_UnpackDepotAddress), BindingLists.UnpackDepot_List, cfsResourceString);
			arrivalCFSReceiptRequested.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ArrivalCFSReceiptRequested", "Arrival CFS/TW / Receipt Requested Date");
			filters.AddCustomFilter(arrivalCFSReceiptRequested);

			var departureCFSReceiptRequested = new DateOrganizationFilter(Descriptions.DepartureCFSReceiptRequested, JobConsolSchema.JK_PackDepotReceiptRequested, GetOrgAddressColumnQueryWithOperatorDelegate(JobConsolSchema.JK_OA_PackDepotAddress), BindingLists.PackDepot_List, cfsResourceString);
			departureCFSReceiptRequested.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|DepartureCFSReceiptRequested", "Departure CFS/TW / Receipt Requested Date");
			filters.AddCustomFilter(departureCFSReceiptRequested);

			var arrivalCFSDispatchRequested = new DateOrganizationFilter(Descriptions.ArrivalCFSDispatchRequested, JobConsolSchema.JK_UnpackDepotDispatchRequested, GetOrgAddressColumnQueryWithOperatorDelegate(JobConsolSchema.JK_OA_UnpackDepotAddress), BindingLists.UnpackDepot_List, cfsResourceString);
			arrivalCFSDispatchRequested.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ArrivalCFSDispatchRequested", "Arrival CFS/TW / Dispatch Requested Date");
			filters.AddCustomFilter(arrivalCFSDispatchRequested);

			var departureCFSDispatchRequested = new DateOrganizationFilter(Descriptions.DepartureCFSDispatchRequested, JobConsolSchema.JK_PackDepotDispatchRequested, GetOrgAddressColumnQueryWithOperatorDelegate(JobConsolSchema.JK_OA_PackDepotAddress), BindingLists.PackDepot_List, cfsResourceString);
			departureCFSDispatchRequested.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|DepartureCFSDispatchRequested", "Departure CFS/TW / Dispatch Requested Date");
			filters.AddCustomFilter(departureCFSDispatchRequested);

			void PortValidation(ZPropertyInfo info) => PortFilterSecurityValidatorForConsol.ValidatePort((DateLocationFilter)info.BizObj);
		}

		ZQuery GetEtaQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetConsolDateQuery(comparisonOperator, SailingFilterBuilder.Dates.ETA, date1, date2);
		}

		ZQuery GetAtaQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetConsolDateQuery(comparisonOperator, SailingFilterBuilder.Dates.ATA, date1, date2);
		}

		ZQuery GetEtdQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetConsolDateQuery(comparisonOperator, SailingFilterBuilder.Dates.ETD, date1, date2);
		}

		ZQuery GetAtdQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetConsolDateQuery(comparisonOperator, SailingFilterBuilder.Dates.ATD, date1, date2);
		}

		ZQuery GetConsolDateQuery(DateComparisonOperator comparisonOperator, SailingFilterBuilder.Dates dateType, ZDateTime date1, ZDateTime date2)
		{
			var builder = new SailingFilterBuilder(Factory);
			builder.SetDateRange(dateType, comparisonOperator, date1, date2);
			return builder.ToConsolFilter();
		}

		ZQuery GetCTOStorageQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingConsol));
			var containerSubQuery = new ZDBOnlySubQuery(typeof(CommonContainer), JobContainerSchema.JC_JK);
			AddDateRange(containerSubQuery, comparisonOperator, JoinCondition.And, JobContainerSchema.JC_ArrivalCTOStorageStartDate, date1.Date, date2.Date);
			result.AddSubQuery(containerSubQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetEmptyReturnReqQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingConsol));
			var containerSubQuery = new ZDBOnlySubQuery(typeof(CommonContainer), JobContainerSchema.JC_JK);
			AddDateRange(containerSubQuery, comparisonOperator, JoinCondition.And, JobContainerSchema.JC_EmptyReturnedBy, date1.Date, date2.Date);
			result.AddSubQuery(containerSubQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#region Organisation Filters

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var consignorTerminology = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value.IsEmpty ? (ZString)FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue : ((ZString)FreightDataRegistry.Instance.ConsignorShipperTerminology.Value).SubstringSafe(0, 15);
			var consignorConsigneeFilter = new ModuleGuidsFilterForOrg(Descriptions.ConsignorConsignee, ModuleIDs.Organisation, GetConsignorConsigneeQuery, BindingLists.OrgConsignor_FilterList, BindingLists.OrgConsignee_FilterList);
			consignorConsigneeFilter.SetItemDescriptions(new ResourceStringData("", consignorTerminology), Res.GetData("Forwarding|JobConsolFilter|Consignee", "Consignee"));
			consignorConsigneeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ConsignorConsignee", "{0} / Consignee", consignorTerminology);
			consignorConsigneeFilter.SubGroup = shipmentFilterProcessor;
			filters.AddFilter(consignorConsigneeFilter);

			var sendReceiveAgentsFilter = new ModuleGuidsFilterForOrg(Descriptions.SendReceiveAgents, ModuleIDs.Organisation, GetForwardersQuery, BindingLists.OrgForwarder_FilterList, BindingLists.OrgForwarder_FilterList);
			sendReceiveAgentsFilter.SetItemDescriptions(Res.GetData("Forwarding|JobConsolFilter|SendAgent", "Send Agent"), Res.GetData("Forwarding|JobConsolFilter|RecAgent", "Rec. Agent"));
			sendReceiveAgentsFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|SendReceiveAgents", "Send / Receive Agents");
			filters.AddFilter(sendReceiveAgentsFilter);

			var carrierFilter = new ModuleGuidFilterForOrg(Descriptions.Carrier, ModuleIDs.Organisation, GetOrgAddressColumnQueryWithOperatorDelegate(JobConsolSchema.JK_OA_ShippingLineAddress), BindingLists.ShippingProvider_FilterList);
			carrierFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|Carrier", "Carrier");
			carrierFilter.SupportsBlankComparisonOperators = true;
			filters.AddFilter(carrierFilter);

			var sendingAgentRelatedPartiesFilter = new OrgRelatedPartiesModuleFilter(Descriptions.SendingAgentRelatedParties, GetSendingAgentRelatedPartiesQuery);
			sendingAgentRelatedPartiesFilter.Category = FilterCategories.Organisations;
			sendingAgentRelatedPartiesFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|SendingAgentRelatedParties", "Sending Agent Related Parties");
			filters.AddCustomFilter(sendingAgentRelatedPartiesFilter);

			var receivingAgentRelatedPartiesFilter = new OrgRelatedPartiesModuleFilter(Descriptions.ReceivingAgentRelatedParties, GetReceivingAgentRelatedPartiesQuery);
			receivingAgentRelatedPartiesFilter.Category = FilterCategories.Organisations;
			receivingAgentRelatedPartiesFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ReceivingAgentRelatedParties", "Receiving Agent Related Parties");
			filters.AddCustomFilter(receivingAgentRelatedPartiesFilter);

			var carrierRelatedPartiesFilter = new OrgRelatedPartiesModuleFilter(Descriptions.CarrierRelatedParties, GetCarrierRelatedPartiesQuery);
			carrierRelatedPartiesFilter.Category = FilterCategories.Organisations;
			carrierRelatedPartiesFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|CarrierRelatedParties", "Carrier Related Parties");
			filters.AddCustomFilter(carrierRelatedPartiesFilter);

			var departureCTOFilter = new ModuleGuidFilterForOrg(Descriptions.DepartureCTO, ModuleIDs.Organisation, GetOrgAddressColumnQueryWithOperatorDelegate(JobConsolSchema.JK_OA_DepartureCTOAddress), BindingLists.OrgCTO_FilterList);
			departureCTOFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|DepartureCTO", "Departure CTO");
			departureCTOFilter.SupportsBlankComparisonOperators = true;
			filters.AddFilter(departureCTOFilter);

			var departureCFSFilter = new ModuleGuidFilterForOrg(Descriptions.DepartureCFS, ModuleIDs.Organisation, GetOrgAddressColumnQueryWithOperatorDelegate(JobConsolSchema.JK_OA_PackDepotAddress), BindingLists.PackDepot_FilterList);
			departureCFSFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|DepartureCFS", "Departure CFS");
			departureCFSFilter.SupportsBlankComparisonOperators = true;
			filters.AddFilter(departureCFSFilter);

			var departureContainerYardFilter = new ModuleGuidFilterForOrg(Descriptions.DepartureContainerYard, ModuleIDs.Organisation, GetOrgAddressColumnQueryWithOperatorDelegate(JobConsolSchema.JK_OA_ContainerYardEmptyPickupAddress), BindingLists.OrgContainerYard_FilterList);
			departureContainerYardFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|DepartureContainerYard", "Departure Container Yard");
			departureContainerYardFilter.SupportsBlankComparisonOperators = true;
			filters.AddFilter(departureContainerYardFilter);

			var departureTransportProviderFilter = new ModuleGuidFilterForOrg(Descriptions.DepartureTransportProvider, ModuleIDs.Organisation, GetOrgAddressColumnQueryWithOperatorDelegate(JobConsolSchema.JK_OA_DeparturePackCFSTransportAddress), BindingLists.OrgMiscServLocalTransport_FilterList);
			departureTransportProviderFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|DepartureTransportProvider", "Departure Transport Provider");
			departureTransportProviderFilter.SupportsBlankComparisonOperators = true;
			filters.AddFilter(departureTransportProviderFilter);

			var arrivalCTOFilter = new ModuleGuidFilterForOrg(Descriptions.ArrivalCTO, ModuleIDs.Organisation, GetOrgAddressColumnQueryWithOperatorDelegate(JobConsolSchema.JK_OA_ArrivalCTOAddress), BindingLists.OrgCTO_FilterList);
			arrivalCTOFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ArrivalCTO", "Arrival CTO");
			arrivalCTOFilter.SupportsBlankComparisonOperators = true;
			filters.AddFilter(arrivalCTOFilter);

			var arrivalCFSFilter = new ModuleGuidFilterForOrg(Descriptions.ArrivalCFS, ModuleIDs.Organisation, GetOrgAddressColumnQueryWithOperatorDelegate(JobConsolSchema.JK_OA_UnpackDepotAddress), BindingLists.UnpackDepot_FilterList);
			arrivalCFSFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ArrivalCFS", "Arrival CFS");
			arrivalCFSFilter.SupportsBlankComparisonOperators = true;
			filters.AddFilter(arrivalCFSFilter);

			var arrivalContainerYardFilter = new ModuleGuidFilterForOrg(Descriptions.ArrivalContainerYard, ModuleIDs.Organisation, GetOrgAddressColumnQueryWithOperatorDelegate(JobConsolSchema.JK_OA_ContainerYardEmptyReturnAddress), BindingLists.OrgContainerYard_FilterList);
			arrivalContainerYardFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ArrivalContainerYard", "Arrival Container Yard");
			arrivalContainerYardFilter.SupportsBlankComparisonOperators = true;
			filters.AddFilter(arrivalContainerYardFilter);

			var arrivalTransportProviderFilter = new ModuleGuidFilterForOrg(Descriptions.ArrivalTransportProvider, ModuleIDs.Organisation, GetOrgAddressColumnQueryWithOperatorDelegate(JobConsolSchema.JK_OA_ArrivalUnpackCFSTransportAddress), BindingLists.OrgMiscServLocalTransport_FilterList);
			arrivalTransportProviderFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ArrivalTransportProvider", "Arrival Transport Provider");
			arrivalTransportProviderFilter.SupportsBlankComparisonOperators = true;
			filters.AddFilter(arrivalTransportProviderFilter);
		}

		static GetGuidQueryWithOperator GetOrgAddressColumnQueryWithOperatorDelegate(SchemaColumn orgAddressColumn)
		{
			return (SQLComparisonOperator comparisonOperator, object pK) => GetOrgAddressColumnQueryWithOperator(pK, orgAddressColumn, comparisonOperator);
		}

		static ZQuery GetOrgAddressColumnQueryWithOperator(object pK, SchemaColumn orgAddressColumn, SQLComparisonOperator comparisonOperator)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingConsol));
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

		#region Get Org Queries

		ZQuery GetForwardersQuery(ZGuid sendingForwarderPK, ZGuid receivingForwarderPK)
		{
			ZQuery result = new ZQuery();

			if (sendingForwarderPK.IsValid)
			{
				result.AddToFilter(GetOrgAddressQuery(sendingForwarderPK, JobConsolSchema.JK_OA_SendingForwarderAddress));
			}
			if (receivingForwarderPK.IsValid)
			{
				result.AddToFilter(GetOrgAddressQuery(receivingForwarderPK, JobConsolSchema.JK_OA_ReceivingForwarderAddress));
			}

			return result;
		}

		ZQuery GetOrgAddressQuery(ZGuid orgPK, SchemaColumn consolAddressForeignKeyColumn)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingConsol));
			ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), consolAddressForeignKeyColumn);
			orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, orgPK);
			result.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetConsignorConsigneeQuery(ZGuid consignorPK, ZGuid consigneePK)
		{
			ZQuery result = new ZQuery();

			if (consignorPK.IsValid)
			{
				result.AddToFilter(GetShipmentConsignorOrConsigneeQuery(DocAddressType.ConsignorDocumentaryAddress, consignorPK));
			}

			if (consigneePK.IsValid)
			{
				result.AddToFilter(GetShipmentConsignorOrConsigneeQuery(DocAddressType.ConsigneeDocumentaryAddress, consigneePK));
			}

			return result;
		}

		ZQuery GetShipmentConsignorOrConsigneeQuery(DocAddressType addressType, ZGuid orgPK)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			ZDBOnlySubQuery docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

			orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, orgPK);
			docAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, addressType));
			docAddressSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

			result.AddSubQuery(docAddressSubQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#region GetRelatedPartiesQuery

		ZQuery GetSendingAgentRelatedPartiesQuery(ZQuery orgHeaderFilter)
		{
			ZQuery orgAddressFilter = OrgRelatedPartiesFilterHelper.GetOrgAddressFromOrgHeaderFilter(orgHeaderFilter, false);

			return FromOrgAddressFilter(orgAddressFilter, JobConsolSchema.JK_OA_SendingForwarderAddress, false);
		}

		ZQuery GetReceivingAgentRelatedPartiesQuery(ZQuery orgHeaderFilter)
		{
			ZQuery orgAddressFilter = OrgRelatedPartiesFilterHelper.GetOrgAddressFromOrgHeaderFilter(orgHeaderFilter, false);

			return FromOrgAddressFilter(orgAddressFilter, JobConsolSchema.JK_OA_ReceivingForwarderAddress, false);
		}

		ZQuery GetCarrierRelatedPartiesQuery(ZQuery orgHeaderFilter)
		{
			ZQuery orgAddressFilter = OrgRelatedPartiesFilterHelper.GetOrgAddressFromOrgHeaderFilter(orgHeaderFilter, false);

			return FromOrgAddressFilter(orgAddressFilter, JobConsolSchema.JK_OA_ShippingLineAddress, false);
		}

		ZQuery FromOrgAddressFilter(ZQuery orgAddressFilter, SchemaColumn consolAddressForeignKeyColumn, bool notIn)
		{
			ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), consolAddressForeignKeyColumn, notIn);
			orgAddressSubQuery.AddToFilter(orgAddressFilter);

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingConsol));
			result.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#endregion

		#region Location Filters

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			ModuleLocationFilter endPortsFilter = filters.AddLocationFilter(Descriptions.EndPorts, GetEndPortsQuery, BindingLists.RefLocation_List, BindingLists.RefLocation_List);
			endPortsFilter.GetXQuery = GetEndPortsXQuery;
			endPortsFilter.SetItemDescriptions(Res.GetData("Forwarding|JobConsolFilter|FirstLoad", "First Load"), Res.GetData("Forwarding|JobConsolFilter|LastDischarge", "Last Discharge"));
			endPortsFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|EndPortsFirstLoadLastDisch", "End Ports (First Load / Last Disch.)");
			if (!Env.Security.MaintainConsolAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed || !Env.Security.MaintainShipmentAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed)
			{
				endPortsFilter.Visibility = FilterVisibility.AlwaysVisible;
			}

			ModuleLocationFilter loadDischargePortsFilter = filters.AddLocationFilter(Descriptions.LoadDischarge, GetLoadDischargePortsQuery, BindingLists.RefLocation_List, BindingLists.RefLocation_List);
			loadDischargePortsFilter.GetXQuery = GetLoadDischargePortsXQuery;
			loadDischargePortsFilter.SetItemDescriptions(Res.GetData("Forwarding|JobConsolFilter|Load", "Load"), Res.GetData("Forwarding|JobConsolFilter|Discharge", "Discharge"));
			loadDischargePortsFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|LoadDischarge", "Load / Discharge");
			if (!Env.Security.MaintainConsolAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed)
			{
				loadDischargePortsFilter.Visibility = FilterVisibility.AlwaysVisible;
			}

			ModuleLocationFilter originDestFiter = filters.AddLocationFilter(Descriptions.OriginDestination, GetOriginDestinationQuery, BindingLists.RefLocation_List, BindingLists.RefLocation_List);
			originDestFiter.SetItemDescriptions(Res.GetData("Forwarding|JobConsolFilter|Origin", "Origin"), Res.GetData("Forwarding|JobConsolFilter|Dest", "Dest."));
			originDestFiter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|OriginDestination", "Origin / Destination");
			originDestFiter.SubGroup = shipmentFilterProcessor;
			if (!Env.Security.MaintainShipmentAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed)
			{
				originDestFiter.Visibility = FilterVisibility.AlwaysVisible;
			}

			if (!Env.Security.MaintainShipmentAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed)
			{
				endPortsFilter.Property1Validation = info => PortFilterSecurityValidatorForShipment.ValidatePort(info, ((ModuleLocationFilter)info.BizObj).Property2Info, filters, Descriptions.EndPorts, Descriptions.OriginDestination);
				endPortsFilter.Property2Validation = info => PortFilterSecurityValidatorForShipment.ValidatePort(info, ((ModuleLocationFilter)info.BizObj).Property1Info, filters, Descriptions.EndPorts, Descriptions.OriginDestination);
			}
			if (!Env.Security.MaintainConsolAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed)
			{
				endPortsFilter.Property1Validation = info => PortFilterSecurityValidatorForConsol.ValidatePort(info, ((ModuleLocationFilter)info.BizObj).Property2Info, filters, Descriptions.EndPorts, Descriptions.LoadDischarge);
				endPortsFilter.Property2Validation = info => PortFilterSecurityValidatorForConsol.ValidatePort(info, ((ModuleLocationFilter)info.BizObj).Property1Info, filters, Descriptions.EndPorts, Descriptions.LoadDischarge);
			}

			loadDischargePortsFilter.Property1Validation = info => PortFilterSecurityValidatorForConsol.ValidatePort(info, ((ModuleLocationFilter)info.BizObj).Property2Info, filters, Descriptions.LoadDischarge, Descriptions.EndPorts);
			loadDischargePortsFilter.Property2Validation = info => PortFilterSecurityValidatorForConsol.ValidatePort(info, ((ModuleLocationFilter)info.BizObj).Property1Info, filters, Descriptions.LoadDischarge, Descriptions.EndPorts);

			originDestFiter.Property1Validation = info => PortFilterSecurityValidatorForShipment.ValidatePort(info, ((ModuleLocationFilter)info.BizObj).Property2Info, filters, Descriptions.OriginDestination, Descriptions.EndPorts);
			originDestFiter.Property2Validation = info => PortFilterSecurityValidatorForShipment.ValidatePort(info, ((ModuleLocationFilter)info.BizObj).Property1Info, filters, Descriptions.OriginDestination, Descriptions.EndPorts);

			var carrierBookingOfficeFilter = filters.AddGuidFilter(Descriptions.CarrierBookingOffice, ModuleIDs.RefUNLOCO, GetCarrierBookingOfficeQuery(), BindingLists.RefUNLOCO_List);
			carrierBookingOfficeFilter.MultilingualDescription = ResString.GetMultilingualString("57f9d9c4-fbef-4648-bd70-f4985520b75a", "Carrier Booking Office");
			carrierBookingOfficeFilter.Category = FilterCategories.Locations;
		}

		PortFilterSecurityValidator PortFilterSecurityValidatorForConsol
		{
			get { return fPortFilterSecurityValidatorForConsol ?? (fPortFilterSecurityValidatorForConsol = new PortFilterSecurityValidator(Factory, Env.Security.MaintainConsolAllowSearchOfUnlocoOutsideLoginBranches)); }
		}
		PortFilterSecurityValidator fPortFilterSecurityValidatorForConsol;

		PortFilterSecurityValidator PortFilterSecurityValidatorForShipment
		{
			get { return fPortFilterSecurityValidatorForShipment ?? (fPortFilterSecurityValidatorForShipment = new PortFilterSecurityValidator(Factory, Env.Security.MaintainShipmentAllowSearchOfUnlocoOutsideLoginBranches)); }
		}
		PortFilterSecurityValidator fPortFilterSecurityValidatorForShipment;

		#region GetEndPortsQuery

		ZQuery GetEndPortsQuery(ZString origin, ZString destination)
		{
			ZQuery result = new ZQuery();

			if (!origin.IsEmpty)
			{
				result.AddToFilter(LocationHelper.GetLocationFilter(Factory, origin, JobConsolSchema.JK_RL_NKLoadPort, typeof(ForwardingConsol)));
			}
			if (!destination.IsEmpty)
			{
				result.AddToFilter(LocationHelper.GetLocationFilter(Factory, destination, JobConsolSchema.JK_RL_NKDischargePort, typeof(ForwardingConsol)));
			}

			return result;
		}

		ZQuery GetEndPortsXQuery(ModuleFilter moduleFilter)
		{
			var filter = moduleFilter as ModuleLocationFilter;
			var result = new ZQuery();

			if (!filter.Property1.IsEmpty)
			{
				result.AddToFilter(XQueryFilterHelper.GenerateXQuery((column) => new ZQuery(column, SQLComparisonOperator.StartsWith, filter.Property1), new XQueryFilterInfo(ShipmentXQueryPaths.PortOfLoading, JobConsolSchema.JK_RL_NKLoadPort.MaxLength)));
			}

			if (!filter.Property2.IsEmpty)
			{
				result.AddToFilter(XQueryFilterHelper.GenerateXQuery((column) => new ZQuery(column, SQLComparisonOperator.StartsWith, filter.Property2), new XQueryFilterInfo(ShipmentXQueryPaths.PortOfDischarge, JobConsolSchema.JK_RL_NKDischargePort.MaxLength)));
			}

			return result;
		}

		#endregion

		#region GetLoadDischargePortsQuery

		ZQuery GetLoadDischargePortsQuery(ZString loadPortNK, ZString dischargePortPK)
		{
			SailingFilterBuilder builder = new SailingFilterBuilder(Factory);
			builder.LoadPort = loadPortNK;
			builder.DischargePort = dischargePortPK;

			return builder.ToConsolFilter();
		}

		#endregion

		#region GetOriginDestinationQuery

		ZQuery GetOriginDestinationQuery(ZString origin, ZString destination)
		{
			ZQuery result = new ZQuery();

			if (!origin.IsEmpty)
			{
				result.AddToFilter(LocationHelper.GetLocationFilter(Factory, origin, JobShipmentSchema.JS_RL_NKOrigin, typeof(ForwardingShipment)));
			}

			if (!destination.IsEmpty)
			{
				result.AddToFilter(LocationHelper.GetLocationFilter(Factory, destination, JobShipmentSchema.JS_RL_NKDestination, typeof(ForwardingShipment)));
			}

			return result;
		}

		#endregion

		#region GetLoadDischargePortsXQuery

		ZQuery GetLoadDischargePortsXQuery(ModuleFilter moduleFilter)
		{
			var filter = moduleFilter as ModuleLocationFilter;
			var result = new ZQuery();

			if (!filter.Property1.IsEmpty)
			{
				result.AddToFilter(XQueryFilterHelper.GenerateXQuery((column) => new ZQuery(column, SQLComparisonOperator.StartsWith, filter.Property1), new XQueryFilterInfo(ShipmentXQueryPaths.PortOfLoading, JobConsolSchema.JK_RL_NKLoadPort.MaxLength)));
			}

			if (!filter.Property2.IsEmpty)
			{
				result.AddToFilter(XQueryFilterHelper.GenerateXQuery((column) => new ZQuery(column, SQLComparisonOperator.StartsWith, filter.Property2), new XQueryFilterInfo(ShipmentXQueryPaths.PortOfDischarge, JobConsolSchema.JK_RL_NKDischargePort.MaxLength)));
			}

			return result;
		}

		#endregion

		#region GetCarrierBookingOfficeQuery

		static GetGuidQueryWithOperator GetCarrierBookingOfficeQuery()
		{
			return (SQLComparisonOperator comparisonOperator, object pK) => GetCarrierBookingOfficeQueryWithOperator(pK, comparisonOperator);
		}

		static ZQuery GetCarrierBookingOfficeQueryWithOperator(object pK, SQLComparisonOperator comparisonOperator)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingConsol));

			result.AddToFilter(JobConsolSchema.JK_TransportMode, Constants.TransportModes.Sea);
			if (comparisonOperator == SQLComparisonOperator.IsBlank || comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				result.AddToFilter(JobConsolSchema.JK_RL_NKCarrierBookingOffice, comparisonOperator == SQLComparisonOperator.IsBlank ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, ZString.Empty);
			}
			else
			{
				var refUNLOCOQuery = new ZDBOnlySubQuery(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, comparisonOperator == SQLComparisonOperator.NotEqual);
				refUNLOCOQuery.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.Equal, (ZGuid)pK);

				result.AddSubQuery(JobConsolSchema.JK_RL_NKCarrierBookingOffice, refUNLOCOQuery, JoinCondition.And);
			}

			return result;
		}

		#endregion

		#endregion

		#region Modes Filters

		void AddModeFilters(ModuleFilterCollection filters)
		{
			var containerModeFilter = filters.AddTextFilter(Descriptions.ContainerMode, JobConsolSchema.JK_ConsolMode, ContainerMode_List);
			containerModeFilter.Category = FilterCategories.ModesAndTypes;
			containerModeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ContainerMode", "Container Mode");

			var containerTypeFilter = filters.AddGuidFilter(Descriptions.ContainerType, ModuleIDs.RefContainer, GetContainerType, BindingLists.RefContainer_List);
			containerTypeFilter.Category = FilterCategories.ModesAndTypes;
			containerTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ContainerType", "Container Type");
			containerTypeFilter.SubGroup = ContainerTypeFilterProcessor;

			var transportModeFilter = filters.AddTextFilter(Descriptions.TransportMode, JobConsolSchema.JK_TransportMode, TransportMode_List);
			transportModeFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.TransportMode, JobConsolSchema.JK_TransportMode.MaxLength);
			transportModeFilter.Category = FilterCategories.ModesAndTypes;
			transportModeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|TransportMode", "Transport Mode");

			var serviceLevelFilter = filters.AddTextFilter(Descriptions.CarrierServiceLevel, JobConsolSchema.JK_AWBServiceLevel, GetCarrierServiceLevelList);
			serviceLevelFilter.Category = FilterCategories.ModesAndTypes;
			serviceLevelFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|CarrierServiceLevel", "Carrier Service Level");

			var gatewayServiceLevelFilter = filters.AddNkFilter(Descriptions.GatewayServiceLevel, JobConsolSchema.JK_RS_NKGatewayServiceLevel, ModuleIDs.ServiceLevel, BindingLists.RefServiceLevel_List);
			gatewayServiceLevelFilter.Category = FilterCategories.ModesAndTypes;
			gatewayServiceLevelFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|GatewayServiceLevel", "Gateway Service Level");

			var preAllocatedAmountExceededTypeFilter = filters.AddTextFilter(Descriptions.PreAllocatedAmountExceeded, GetPreAllocatedAmountExceededQuery, PreAllocatedAmountExceeded_List);
			preAllocatedAmountExceededTypeFilter.Category = FilterCategories.ModesAndTypes;
			preAllocatedAmountExceededTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|Pre-AllocatedAmountExceeded", "Pre-Allocated Amount Exceeded");

			var consolTypeFilter = filters.AddTextFilter(Descriptions.ConsolType, JobConsolSchema.JK_AgentType, ConsolType_List);
			consolTypeFilter.Category = FilterCategories.ModesAndTypes;
			consolTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ConsolType", "Consol Type");

			var sendingAgentTypeFilter = filters.AddTextFilter(Descriptions.SendingAgentType, JobConsolSchema.JK_SendingForwarderHandlingType, GatewayType_List);
			sendingAgentTypeFilter.Category = FilterCategories.ModesAndTypes;
			sendingAgentTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|SendingAgentType", "Sending Agent Type");

			var receivingAgentType = filters.AddTextFilter(Descriptions.ReceivingAgentType, JobConsolSchema.JK_ReceivingForwarderHandlingType, GatewayType_List);
			receivingAgentType.Category = FilterCategories.ModesAndTypes;
			receivingAgentType.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ReceivingAgentType", "Receiving Agent Type");

			var dgSubstanceDGClassFilter = new DGClassDGSubstanceFilter(Descriptions.DGClassDGSubstance, GetDGClassDGSubstanceQuery);
			dgSubstanceDGClassFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|DGClassDGSubstance", "DG Class / DG Substance");
			dgSubstanceDGClassFilter.Category = FilterCategories.ModesAndTypes;
			filters.AddCustomFilter(dgSubstanceDGClassFilter);
		}

		ZQuery GetContainerType(ZGuid refContainerPK)
		{
			return new ZQuery(RefContainerSchema.PK, refContainerPK);
		}

		ZQuery GetPreAllocatedAmountExceededQuery(ZString paaStatus)
		{
			ZDBOnlyQuery result = null;

			if (paaStatus != PreAllocatedAmountExceededCodes.All)
			{
				result = new ZDBOnlyQuery(typeof(ForwardingConsol));
				var eventsQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
				eventsQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.PreAllocatedAmountExceeded.Code);
				eventsQuery.AddToFilter(StmALogSchema.SL_IsCancelled, paaStatus == PreAllocatedAmountExceededCodes.NotExceeded || paaStatus == PreAllocatedAmountExceededCodes.NotExceededUnderCurrentBranchAndDepartment);

				if (paaStatus == PreAllocatedAmountExceededCodes.ExceededUnderCurrentBranchAndDepartment || paaStatus == PreAllocatedAmountExceededCodes.NotExceededUnderCurrentBranchAndDepartment)
				{
					eventsQuery.AddToFilter(StmALogSchema.SL_GB_NKBranch, GlbBranch.CurrentBranch.GB_Code);
					eventsQuery.AddToFilter(StmALogSchema.SL_GE_NKDepartment, GlbDepartment.CurrentDepartment.GE_Code);
				}

				result.AddSubQuery(eventsQuery, JoinCondition.And);
			}

			return result ?? new ZQuery();
		}

		internal static class PreAllocatedAmountExceededCodes
		{
			public const string All = "ALL";
			public const string Exceeded = "EXC";
			public const string NotExceeded = "NOE";
			public const string ExceededUnderCurrentBranchAndDepartment = "CEX";
			public const string NotExceededUnderCurrentBranchAndDepartment = "CNE";
		}

		#region GetDGClassDGSubstanceQuery

		ZQuery GetDGClassDGSubstanceQuery(SQLComparisonOperator dgOperator, ZString dgClass, ZString dgSubstance)
		{
			var notInOperations = new SQLComparisonOperator[] { SQLComparisonOperator.NotContains, SQLComparisonOperator.DoesNotStartWith, SQLComparisonOperator.NotEqual, SQLComparisonOperator.IsBlank };
			var isBlankOperation = dgOperator == SQLComparisonOperator.IsBlank || dgOperator == SQLComparisonOperator.IsNotBlank;

			var result = new ZDBOnlyQuery(typeof(ForwardingConsol));
			var dgRestrictionsQuery = new ZDBOnlySubQuery(typeof(ConsolDGRestrictions), JobConsolDGRestrictionsSchema.JKD_JK, notInOperations.Contains(dgOperator));
			var dgOperatorIfNegated = notInOperations.Contains(dgOperator) ? dgOperator.GetNegatingSQLOperatorIfNotInSubquery() : dgOperator;

			if (!isBlankOperation)
			{
				if (!dgClass.IsEmpty)
				{
					dgRestrictionsQuery.AddToFilter(JobConsolDGRestrictionsSchema.JKD_Class, dgOperatorIfNegated, dgClass);
				}

				if (!dgSubstance.IsEmpty)
				{
					var unno = dgSubstance;

					if (dgSubstance.Length >= 5)
					{
						dgRestrictionsQuery.AddToFilter(JobConsolDGRestrictionsSchema.JKD_Variant, dgSubstance.Substring(4));
						unno = dgSubstance.Substring(0, 4);
					}

					dgRestrictionsQuery.AddToFilter(JobConsolDGRestrictionsSchema.JKD_UNNO, unno);
				}
			}

			result.AddSubQuery(dgRestrictionsQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#endregion

		#region Add Customs Filters

		void AddCustomsFilters(ModuleFilterCollection filters)
		{
			ObjectFactory.Get<Enterprise.Integration.Customs.IForwardingConsolModuleCustomColumnsAndFiltersProvider>().AddFilters(filters, Factory, this);
		}

		#endregion

		#region Related Containers Filter

		void AddRelatedContainersFilter(ModuleFilterCollection filters)
		{
			var filter = new RelatedContainersOfConsolFilter(Descriptions.RelatedContainers, Factory);
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|RelatedContainers", "Related Containers");

			var description = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ContainerCategory", "Container");
			filter.Category = FilterCategories.GetOrCreateFilterCategory(description);

			filters.AddFilter(filter);
		}

		#endregion

		#region Related Transport Legs Filter

		void AddRelatedTransportLegsFilter(ModuleFilterCollection filters)
		{
			var filter = new RelatedTransportLegsOfConsolFilter(Descriptions.RelatedTransportLegs, Factory);
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|RelatedTransportLegs", "Related Transport Legs");

			var description = ResString.GetMultilingualString("Forwarding|JobConsolFilter|TransportsCategory", "Transports");
			filter.Category = FilterCategories.GetOrCreateFilterCategory(description);

			filters.AddFilter(filter);
		}

		#endregion

		#region Status and Flags Filters

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			ModuleFilter routingStatusFilter = filters.AddTextFilter(Descriptions.RoutingStatus, GetRoutingStatusQuery, RoutingStatus_List);
			routingStatusFilter.Category = FilterCategories.StatusAndFlags;
			routingStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|RoutingStatus", "Routing Status");

			ModuleFlagsFilter loadListFilter = filters.AddFlagsFilter(Descriptions.HiddenLoadLists, new string[] { Res.GetString("Forwarding|JobConsolFilter|HiddenFilter", "Hidden Filter") }, new GetFlagsQuery[] { GetShowLoadListsQuery });
			loadListFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			loadListFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|HiddenLoadListsFilter", "Hidden Load Lists Filter");

			filters.AddFlagsFilter(Descriptions.ConsolsWithoutShipments,
				new string[] { Res.GetString("Forwarding|JobConsolFilter|OnlyShowConsolsWithNoShipments", "Only show Consols with no Shipments") },
				new GetFlagsQuery[] { GetConsolsWithoutShipmentsQuery }).
				MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ConsolsWithoutShipments", "Consols without Shipments");

			filters.AddFlagsFilter(Descriptions.ConsolsWithShipments,
				new string[] { Res.GetString("Forwarding|JobConsolFilter|OnlyShowConsolsWithShipments", "Only show Consols with Shipments") },
				new GetFlagsQuery[] { GetConsolsWithShipmentsQuery }).
				MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ConsolsWithShipments", "Consols with Shipments");

			if (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.Value.EnableEBLIntegration)
			{
				var electronicBillStatusFilter = filters.AddTextFilter(Descriptions.electronicBillStatus, GetElectronicBillStatusQueryWithOperator, ElectronicBillStatus_List);
				electronicBillStatusFilter.Category = FilterCategories.StatusAndFlags;
				electronicBillStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ElectronicBillStatus", "Electronic Bill Status");

				var electronicBillTermsFilter = filters.AddTextFilter(Descriptions.electronicBillTerms, JobConsolSchema.JK_ElectronicBillOfLadingTerms, ElectronicBillTerms_List);
				electronicBillTermsFilter.Category = FilterCategories.StatusAndFlags;
				electronicBillTermsFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ElectronicBillTerms", "Electronic Bill Terms");

				var electronicBillTypeFilter = filters.AddTextFilter(Descriptions.electronicBillType, JobConsolSchema.JK_ElectronicBillOfLadingType, ElectronicBillType_List);
				electronicBillTypeFilter.Category = FilterCategories.StatusAndFlags;
				electronicBillTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ElectronicBillType", "Electronic Bill Type");
			}

			ModuleFilter phaseFilter = filters.AddTextFilter(Descriptions.Phase, JobConsolSchema.JK_Phase, PhaseList);
			phaseFilter.Category = FilterCategories.StatusAndFlags;
			phaseFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|Phase", "Phase");

			filters.AddFlagsFilter(Descriptions.IsCargoOnly,
				new string[] { Res.GetString("Forwarding|JobConsolFilter|IsCargoOnly", "Is Cargo Only") },
				new GetFlagsQuery[] { GetIsCargoOnly }).
				MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|IsCargoOnly", "Is Cargo Only");

			filters.AddFlagsFilter(Descriptions.IsHazardous,
				new string[] { Res.GetString("Forwarding|JobConsolFilter|IsHazardous", "Is Hazardous") },
				new GetFlagsQuery[] { GetIsHazardousQuery }).
				MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|IsHazardous", "Is Hazardous");

			var vgmStatusFilter = filters.AddTextFilter(Descriptions.VGMStatus, GetVGMStatusQuery, VGMStatus_List);
			vgmStatusFilter.Category = FilterCategories.StatusAndFlags;
			vgmStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|VGMStatus", "VGM Status");

			var isTemperatureControlledFilter = filters.AddFlagsFilter(
				Descriptions.IsTemperatureControlled,
				new string[] { Res.GetString("Forwarding|JobConsolFilter|IsTemperatureControlled", "Is Temperature Controlled") },
				new GetFlagsQuery[] { GetIsTemperatureControlledQuery }
			);
			isTemperatureControlledFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|IsTemperatureControlled", "Is Temperature Controlled");

			var securityStatusFilter = filters.AddTextFilter(Descriptions.AirfreightSecurityStatus, GetAirfreightSecurityStatusQuery, AirfreightSecurityStatusList);
			securityStatusFilter.Category = FilterCategories.StatusAndFlags;
			securityStatusFilter.ErrorOnCodeNotPresent = true;
			securityStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|AirfreightSecurityStatus", "Airfreight Security Status");

			var airBookingStatusFilter = filters.AddTextFilter(Descriptions.AirBookingStatus, GetAirBookingStatusQuery, AirBookingStatusList);
			airBookingStatusFilter.Category = FilterCategories.StatusAndFlags;
			airBookingStatusFilter.ErrorOnCodeNotPresent = true;
			airBookingStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|AirBookingStatus", "Carrier Booking Status - Air");

			var flightStatusFilter = filters.AddTextFilter(Descriptions.FlightStatus, GetFlightStatusQuery, FlightStatusList);
			flightStatusFilter.Category = FilterCategories.StatusAndFlags;
			flightStatusFilter.ErrorOnCodeNotPresent = true;
			flightStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|FlightStatus", "Flight Status");

			var carrierBookingStatusFilter = filters.AddTextFilter(Descriptions.CarrierBookingStatus, GetCarrierBookingStatusQuery, CarrierBookingStatusList);
			carrierBookingStatusFilter.Category = FilterCategories.StatusAndFlags;
			carrierBookingStatusFilter.ErrorOnCodeNotPresent = true;
			carrierBookingStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|CarrierBookingStatus", "Carrier Booking Status - Ocean");

			var releaseTypeFilter = filters.AddTextFilter(Descriptions.ReleaseType, GetReleaseTypeQuery, ReleaseTypeList);
			releaseTypeFilter.Category = FilterCategories.StatusAndFlags;
			releaseTypeFilter.ErrorOnCodeNotPresent = true;
			releaseTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|ReleaseType", "Release Type");

			filters.AddFlagsFilter(Descriptions.PossibleOversize,
				new string[] { Res.GetString("Forwarding|JobConsolFilter|PossibleOversize", "Possible Oversize") },
				new GetFlagsQuery[] { GetPossibleOversizeQuery }).
				MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|PossibleOversize", "Possible Oversize");
		}

		#region GetRoutingStatusQuery

		ZQuery GetRoutingStatusQuery(ZString routingStatus)
		{
			ZQuery result;

			switch (routingStatus)
			{
				case RoutingStatusIncomplete:
					result = GetRoutingStatusQueryCore(false);
					break;

				case RoutingStatusComplete:
					result = GetRoutingStatusQueryCore(true);
					break;

				default:
					result = new ZQuery();
					break;
			}

			return result;
		}

		ZQuery GetRoutingStatusQueryCore(bool complete)
		{
			#region SQL

			string sQL = string.Format(CultureInfo.InvariantCulture, @"
					{0} {20} in
					(
						select {0}
						from {1}

						join {2} as LoadTransport
							on LoadTransport.{3} = {0}
							and LoadTransport.{4} = @CON

						join {2} as DiscTransport
							on DiscTransport.{3} = {0}
							and DiscTransport.{4} = @CON

						left join {5} as LoadSailing
							on LoadSailing.{6} = LoadTransport.{7}

						left join {5} as DiscSailing
							on DiscSailing.{6} = DiscTransport.{7}

						left join {8}
							on {9} = LoadSailing.{10}

						left join {11}
							on {12} = DiscSailing.{13}

						where
							isnull({14}, LoadTransport.{15}) = {16} and
							isnull({17}, DiscTransport.{18}) = {19}
					)
					",
				 JobConsolSchema.Constants.PK,                              // 0
				 JobConsolSchema.Constants.TableName,                       // 1
				 JobConsolTransportSchema.Constants.TableName,              // 2
				 JobConsolTransportSchema.Constants.JW_ParentGUID,          // 3
				 JobConsolTransportSchema.Constants.JW_ParentType,          // 4
				 JobSailingSchema.Constants.TableName,                      // 5
				 JobSailingSchema.Constants.PK,                             // 6
				 JobConsolTransportSchema.Constants.JW_JX,                  // 7
				 JobVoyOriginSchema.Constants.TableName,                    // 8
				 JobVoyOriginSchema.Constants.PK,                           // 9
				 JobSailingSchema.Constants.JX_JA,                          // 10
				 JobVoyDestinationSchema.Constants.TableName,               // 11
				 JobVoyDestinationSchema.Constants.PK,                      // 12
				 JobSailingSchema.Constants.JX_JB,                          // 13
				 JobVoyOriginSchema.Constants.JA_RL_NKPortOfLoading,        // 14
				 JobConsolTransportSchema.Constants.JW_RL_NKLoadPort,       // 15
				 JobConsolSchema.Constants.JK_RL_NKLoadPort,                // 16
				 JobVoyDestinationSchema.Constants.JB_RL_NKPortOfDischarge, // 17
				 JobConsolTransportSchema.Constants.JW_RL_NKDiscPort,       // 18
				 JobConsolSchema.Constants.JK_RL_NKDischargePort,           // 19
				 (complete ? "" : (NoResString)"not")                                    // May be a part of SQL expression.
				);

			#endregion

			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add("@CON", "CON", JobConsolTransportSchema.JW_ParentType);

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingConsol));
			result.AddFilterAndZSQLParameterCollection(sQL, @params);

			return result;
		}

		#endregion

		#region GetShowLoadListsQuery

		ZQuery GetShowLoadListsQuery(ZBool thisValueIsIgnored)
		{
			return new ZQuery(JobConsolSchema.JK_IsForwarding, true);
		}

		#endregion

		#region GetConsolsWithoutShipmentsQuery

		ZQuery GetConsolsWithoutShipmentsQuery(ZBool showConsolsWithoutShipmentsOnly)
		{
			ZQuery result;

			if (showConsolsWithoutShipmentsOnly)
			{
				string sQL =
					"NOT EXISTS " +
					"(SELECT * FROM " + JobConShipLinkSchema.Constants.SqlSchemaName + "." + JobConShipLinkSchema.Constants.TableName +
					" WHERE " + JobConShipLinkSchema.Constants.JN_JK + " = " + ForwardingConsol.Schema.PK + ")";

				result = new ZDBOnlyQuery(typeof(ForwardingShipment));
				result.AddFilterAndZSQLParameterCollection(sQL, null);
			}
			else
			{
				result = new ZQuery();
			}

			return result;
		}

		#endregion

		#region GetConsolsWithShipmentsQuery

		ZQuery GetConsolsWithShipmentsQuery(ZBool showConsolsWithShipmentsOnly)
		{
			ZQuery result;

			if (showConsolsWithShipmentsOnly)
			{
				string sQL =
					"EXISTS " +
					"(SELECT * FROM " + JobConShipLinkSchema.Constants.SqlSchemaName + "." + JobConShipLinkSchema.Constants.TableName +
					" WHERE " + JobConShipLinkSchema.Constants.JN_JK + " = " + ForwardingConsol.Schema.PK + ")";

				result = new ZDBOnlyQuery(typeof(ForwardingShipment));
				result.AddFilterAndZSQLParameterCollection(sQL, null);
			}
			else
			{
				result = new ZQuery();
			}

			return result;
		}

		#endregion

		#region GetElectronicBillStatusQuery

		ZQuery GetElectronicBillStatusQueryWithOperator(SQLComparisonOperator comparisonOperator, ZString electronicBillStatus)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingConsol));

			SelectElectronicBillStatus(result, GetReferenceClauseByElectronicBillStatus(comparisonOperator, electronicBillStatus),
				comparisonOperator == SQLComparisonOperator.IsBlank || comparisonOperator == SQLComparisonOperator.NotEqual
				|| comparisonOperator == SQLComparisonOperator.DoesNotStartWith || comparisonOperator == SQLComparisonOperator.NotContains);

			return result;
		}

		void SelectElectronicBillStatus(ZDBOnlyQuery query, ZString referenceClause, ZBool notIn)
		{
			var latestBLUStatusSQL = $@"
SELECT AA.{StmALog.Schema.SL_Parent} FROM {StmALogSchema.Constants.SqlSchemaName}.{StmALogSchema.Constants.TableName} AA
INNER JOIN
(
	SELECT {StmALog.Schema.SL_Parent}, MAX({StmALog.Schema.SL_PostedTimeUtc}) maxTime FROM {StmALogSchema.Constants.SqlSchemaName}.{StmALogSchema.Constants.TableName}
	WHERE {StmALog.Schema.SL_Table} = @jobConsolTableName
	AND {StmALog.Schema.SL_IsCancelled} = @isCancelled
	AND {StmALog.Schema.SL_SE_NKEvent} = @eventCode
	GROUP BY {StmALog.Schema.SL_Parent}
) BB
ON AA.{StmALog.Schema.SL_Parent} = BB.{StmALog.Schema.SL_Parent} AND AA.{StmALog.Schema.SL_PostedTimeUtc} = maxTime
WHERE ({referenceClause})
";

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@jobConsolTableName", AutoJobConsol.Schema.TableName, StmALogSchema.SL_Table);
			parameters.Add("@isCancelled", "N", StmALogSchema.SL_IsCancelled);
			parameters.Add("@eventCode", Events.BillStatusUpdated.Code, StmALogSchema.SL_SE_NKEvent);

			query.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "{0}{1} IN ({2})", ForwardingConsol.Schema.PK, notIn ? " NOT" : ZString.Empty, latestBLUStatusSQL), parameters);
		}

		ZString GetReferenceClauseByElectronicBillStatus(SQLComparisonOperator comparisonOperator, ZString electronicBillStatus)
		{
			var result = new List<ZString>();

			Func<string, bool> isApplicable = (billOfLadingBillStatusCode) =>
				comparisonOperator == SQLComparisonOperator.IsBlank
				|| comparisonOperator == SQLComparisonOperator.IsNotBlank
				|| ((comparisonOperator == SQLComparisonOperator.Equal || comparisonOperator == SQLComparisonOperator.NotEqual) && electronicBillStatus == billOfLadingBillStatusCode)
				|| ((comparisonOperator == SQLComparisonOperator.StartsWith || comparisonOperator == SQLComparisonOperator.DoesNotStartWith) && billOfLadingBillStatusCode.StartsWith(electronicBillStatus))
				|| ((comparisonOperator == SQLComparisonOperator.Contains || comparisonOperator == SQLComparisonOperator.NotContains) && billOfLadingBillStatusCode.Contains(electronicBillStatus));

			if (isApplicable(FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillReceived))
			{
				result.Add($"{StmALog.Schema.SL_Reference} LIKE '%{EventConstants.EventReferenceParameters.Codes.Type}={BillStatusUpdatedTypes.OriginalBillPublished}%'");
				result.Add($"{StmALog.Schema.SL_Reference} LIKE '%{EventConstants.EventReferenceParameters.Codes.Type}={BillStatusUpdatedTypes.AmendmentDenied}%'");
				result.Add($"{StmALog.Schema.SL_Reference} LIKE '%{EventConstants.EventReferenceParameters.Codes.Type}={BillStatusUpdatedTypes.AmendmentBillReceived}%'");
			}

			if (isApplicable(FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress))
			{
				result.Add($"{StmALog.Schema.SL_Reference} LIKE '%{EventConstants.EventReferenceParameters.Codes.Type}={BillStatusUpdatedTypes.AmendmentRequested}%'");
				result.Add($"{StmALog.Schema.SL_Reference} LIKE '%{EventConstants.EventReferenceParameters.Codes.Type}={BillStatusUpdatedTypes.AmendmentGranted}%'");
			}

			if (isApplicable(FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillTransferred))
			{
				result.Add($"{StmALog.Schema.SL_Reference} LIKE '%{EventConstants.EventReferenceParameters.Codes.Type}={BillStatusUpdatedTypes.OriginalBillTransferred}%'");
			}

			if (isApplicable(FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper))
			{
				result.Add($"{StmALog.Schema.SL_Reference} LIKE '%{EventConstants.EventReferenceParameters.Codes.Type}={BillStatusUpdatedTypes.SwitchedToPaper}%'");
			}

			if (isApplicable(FreightConstants.BillOfLadingBillStatus.Codes.Surrendered))
			{
				result.Add($"{StmALog.Schema.SL_Reference} LIKE '%{EventConstants.EventReferenceParameters.Codes.Type}={BillStatusUpdatedTypes.Surrendered}%'");
			}

			return result.Any() ? string.Join(" OR ", result) : "1 = 2";
		}

		#endregion

		#region GetIsCargoOnly

		ZQuery GetIsCargoOnly(ZBool showCargoOnly)
		{
			var result = new ZDBOnlyQuery(typeof(CommonConsol));

			var sailingFilter = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
			var destinationQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobVoyDestinationSchema.PK);
			var transportQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID, !showCargoOnly);
			transportQuery.AddToFilter(JobConsolTransportSchema.JW_TransportMode, Core.Constants.TransportModes.Air);

			var voyageQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyageSchema.PK);
			voyageQuery.AddToFilter(JobVoyageSchema.JV_IsCargoOnly, true);

			destinationQuery.AddSubQuery(JobVoyDestinationSchema.JB_JV, voyageQuery, JoinCondition.And);
			sailingFilter.AddSubQuery(JobSailingSchema.JX_JB, destinationQuery, JoinCondition.And);
			transportQuery.AddSubQuery(JobConsolTransportSchema.JW_JX, sailingFilter, JoinCondition.And);
			result.AddSubQuery(JobConsolSchema.PK, transportQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region GetIsTemperatureControlledQuery

		ZQuery GetIsTemperatureControlledQuery(ZBool isTemperatureControlled)
		{
			var query = new ZQuery();
			query.AddToFilter(JobConsolSchema.JK_RequiresTemperatureControl, isTemperatureControlled);
			return query;
		}

		#endregion

		#region GetIsHazardous

		ZQuery GetIsHazardousQuery(ZBool showHazardous)
			=> new ZDBOnlyQuery(typeof(CommonConsol)).AddToFilter(JobConsolSchema.JK_IsHazardous, showHazardous);

		#endregion

		#region GetVGMStatusQuery

		ZQuery GetVGMStatusQuery(ZString vgmStatus)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingConsol));

			var containerSubQuery = new ZDBOnlySubQuery(typeof(CommonContainer), JobContainerSchema.JC_JK);
			containerSubQuery.AddToFilter(JobContainerSchema.JC_GrossWeightVerificationStatus, vgmStatus);
			result.AddSubQuery(containerSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region GetAirfreightSecurityStatusQuery

		ZQuery GetAirfreightSecurityStatusQuery(ZString airfreightSecurityStatus)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingConsol));

			var specialHandlingSubQuery = new ZDBOnlySubQuery(typeof(JobConsolAWBSpecialHandling), JobConsolAWBSpecialHandlingSchema.JKH_JK_Consol);
			specialHandlingSubQuery.AddToFilter(JobConsolAWBSpecialHandlingSchema.JKH_Code, airfreightSecurityStatus);
			result.AddSubQuery(specialHandlingSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region GetFlightStatusQuery

		ZQuery GetFlightStatusQuery(ZString flightStatus)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingConsol));

			var statusQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			statusQuery.AddToFilter(JobConsolTransportSchema.JW_OnlineScheduleStatus, flightStatus);
			statusQuery.AddToFilter(JobConsolTransportSchema.JW_TransportMode, Constants.TransportModes.Air);

			result.AddSubQuery(statusQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region GetCarrierBookingStatusQuery

		ZQuery GetCarrierBookingStatusQuery(ZString flightStatus)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingConsol));
			result.AddToFilter(JobConsolSchema.JK_TransportMode, Constants.TransportModes.Sea);

			if (flightStatus == FreightConstants.CarrierBookingStatus.Codes.NotSent)
			{
				SelectNotSent(result);
				return result;
			}

			var messageType = GetMessageTypeByFlightStatus(flightStatus);
			var eventCode = GetEventCodeByFlightStauts(flightStatus);

			if (eventCode != ZString.Empty)
			{
				SelectLatestStatus(result, eventCode, messageType);
				ExceptContainerSILogsForBrMessage(result, messageType);
				AdditionalCheckForBrRejected(result, flightStatus);
			}

			return result;
		}

		void SelectNotSent(ZDBOnlyQuery query)
		{
			var containerBROrSISQL = $@"
SELECT {AutoJobDocumentData.Schema.JDD_ParentID} FROM {JobDocumentDataSchema.Constants.SqlSchemaName}.{JobDocumentDataSchema.Constants.TableName}
WHERE {AutoJobDocumentData.Schema.JDD_Name} = @documentDataName
AND {AutoJobDocumentData.Schema.PK} IN
(
	SELECT DISTINCT {StmALog.Schema.SL_Parent} FROM {StmALogSchema.Constants.SqlSchemaName}.{StmALogSchema.Constants.TableName}
	WHERE {StmALog.Schema.SL_Table} = @jobDocumentDataTableName
	AND {StmALog.Schema.SL_IsCancelled} = @isCancelled
	AND 
	(
		{StmALog.Schema.SL_Reference} LIKE @fuzzyBookingRequest
		OR {StmALog.Schema.SL_Reference} LIKE @fuzzyShippingInstruction
	)
)";
			var latestStatusIsReset = $@"
SELECT {AutoJobDocumentData.Schema.JDD_ParentID} FROM {JobDocumentDataSchema.Constants.SqlSchemaName}.{JobDocumentDataSchema.Constants.TableName}
WHERE {AutoJobDocumentData.Schema.JDD_Name} = @documentDataName
AND {AutoJobDocumentData.Schema.PK} in 
(
	SELECT AA.{StmALog.Schema.SL_Parent} FROM {StmALogSchema.Constants.SqlSchemaName}.{StmALogSchema.Constants.TableName} AA
	INNER JOIN
	(
		SELECT {StmALog.Schema.SL_Parent}, MAX({StmALog.Schema.SL_PostedTimeUtc}) maxTime FROM {StmALogSchema.Constants.SqlSchemaName}.{StmALogSchema.Constants.TableName}
		WHERE {StmALog.Schema.SL_Table} = @jobDocumentDataTableName
		AND {StmALog.Schema.SL_IsCancelled} = @isCancelled
		AND 
		(
			{StmALog.Schema.SL_Reference} LIKE @fuzzyBookingRequest
			OR {StmALog.Schema.SL_Reference} LIKE @fuzzyShippingInstruction
		)
		GROUP BY {StmALog.Schema.SL_Parent}
	) BB
	ON AA.{StmALog.Schema.SL_Parent} = BB.{StmALog.Schema.SL_Parent} AND AA.{StmALog.Schema.SL_PostedTimeUtc} = maxTime
	WHERE {StmALog.Schema.SL_SE_NKEvent} = @statusUpdatedCode
)";

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@documentDataName", ConsolDocumentDataStoreNames.SeaBookingRequest2, JobDocumentDataSchema.JDD_Name);
			parameters.Add("@jobDocumentDataTableName", AutoJobDocumentData.Schema.TableName, StmALogSchema.SL_Table);
			parameters.Add("@isCancelled", "N", StmALogSchema.SL_IsCancelled);
			parameters.Add("@fuzzyBookingRequest", $"%{EventConstants.EventReferenceParameters.Codes.MessageType}={ConsolDocumentNames.BookingRequest}%", StmALogSchema.SL_Reference);
			parameters.Add("@fuzzyShippingInstruction", $"%{EventConstants.EventReferenceParameters.Codes.MessageType}={ConsolDocumentNames.ShippingInstruction}%", StmALogSchema.SL_Reference);
			parameters.Add("@statusUpdatedCode", Events.StatusUpdatedCode, StmALogSchema.SL_SE_NKEvent);

			query.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "{0} NOT IN ({1}) OR {0} IN ({2})",
				ForwardingConsol.Schema.PK, containerBROrSISQL, latestStatusIsReset),
				parameters);
		}

		void SelectLatestStatus(ZDBOnlyQuery query, ZString eventCode, ZString messageType)
		{
			var latestStatusSQL = $@"
SELECT {AutoJobDocumentData.Schema.JDD_ParentID} FROM {JobDocumentDataSchema.Constants.SqlSchemaName}.{JobDocumentDataSchema.Constants.TableName}
WHERE {AutoJobDocumentData.Schema.JDD_Name} = @documentDataName
AND {AutoJobDocumentData.Schema.PK} IN 
(
	SELECT AA.{StmALog.Schema.SL_Parent} FROM {StmALogSchema.Constants.SqlSchemaName}.{StmALogSchema.Constants.TableName} AA
	INNER JOIN
	(
		SELECT {StmALog.Schema.SL_Parent}, MAX({StmALog.Schema.SL_PostedTimeUtc}) maxTime FROM {StmALogSchema.Constants.SqlSchemaName}.{StmALogSchema.Constants.TableName}
		WHERE {StmALog.Schema.SL_Table} = @jobDocumentDataTableName
		AND {StmALog.Schema.SL_IsCancelled} = @isCancelled
		AND {StmALog.Schema.SL_Reference} LIKE @fuzzyMessageType
		GROUP BY {StmALog.Schema.SL_Parent}
	) BB
	ON AA.{StmALog.Schema.SL_Parent} = BB.{StmALog.Schema.SL_Parent} AND AA.{StmALog.Schema.SL_PostedTimeUtc} = maxTime
	WHERE {StmALog.Schema.SL_SE_NKEvent} = @statusCode
)";

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@documentDataName", ConsolDocumentDataStoreNames.SeaBookingRequest2, JobDocumentDataSchema.JDD_Name);
			parameters.Add("@jobDocumentDataTableName", AutoJobDocumentData.Schema.TableName, StmALogSchema.SL_Table);
			parameters.Add("@isCancelled", "N", StmALogSchema.SL_IsCancelled);
			parameters.Add("@fuzzyMessageType", $"%{EventConstants.EventReferenceParameters.Codes.MessageType}={messageType}%", StmALogSchema.SL_Reference);
			parameters.Add("@statusCode", eventCode, StmALogSchema.SL_SE_NKEvent);

			query.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "{0} IN ({1})", ForwardingConsol.Schema.PK, latestStatusSQL), parameters);
		}

		void ExceptContainerSILogsForBrMessage(ZDBOnlyQuery query, ZString messageType)
		{
			if (messageType == ConsolDocumentNames.BookingRequest)
			{
				var containerSILogsSQL = $@"
SELECT {AutoJobDocumentData.Schema.JDD_ParentID} FROM {JobDocumentDataSchema.Constants.SqlSchemaName}.{JobDocumentDataSchema.Constants.TableName}
WHERE {AutoJobDocumentData.Schema.JDD_Name} = @documentDataName
AND {AutoJobDocumentData.Schema.PK} IN
(
	SELECT DISTINCT {StmALog.Schema.SL_Parent} FROM {StmALogSchema.Constants.SqlSchemaName}.{StmALogSchema.Constants.TableName}
	WHERE {StmALog.Schema.SL_Table} = @jobDocumentDataTableName
	AND {StmALog.Schema.SL_IsCancelled} = @isCancelled
	AND {StmALog.Schema.SL_Reference} LIKE @fuzzyShippingInstruction
)";

				var parameters = new ZSqlParameterCollection();
				parameters.Add("@documentDataName", ConsolDocumentDataStoreNames.SeaBookingRequest2, JobDocumentDataSchema.JDD_Name);
				parameters.Add("@jobDocumentDataTableName", AutoJobDocumentData.Schema.TableName, StmALogSchema.SL_Table);
				parameters.Add("@isCancelled", "N", StmALogSchema.SL_IsCancelled);
				parameters.Add("@fuzzyShippingInstruction", $"%{EventConstants.EventReferenceParameters.Codes.MessageType}={ConsolDocumentNames.ShippingInstruction}%", StmALogSchema.SL_Reference);

				query.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "{0} NOT IN ({1})", ForwardingConsol.Schema.PK, containerSILogsSQL), parameters);
			}
		}

		void AdditionalCheckForBrRejected(ZDBOnlyQuery query, ZString flightStatus)
		{
			if (flightStatus == FreightConstants.CarrierBookingStatus.Codes.BookingRequest.WithdrawalRejected || flightStatus == FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Rejected)
			{
				var notIn = flightStatus == FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Rejected;

				var hasMessageWithdrawCancelRequestCodeSQL = $@"
SELECT {AutoJobDocumentData.Schema.JDD_ParentID} FROM {JobDocumentDataSchema.Constants.SqlSchemaName}.{JobDocumentDataSchema.Constants.TableName}
WHERE {AutoJobDocumentData.Schema.JDD_Name} = @documentDataName
AND {AutoJobDocumentData.Schema.PK} IN
(
	SELECT AA.{StmALog.Schema.SL_Parent} FROM {StmALogSchema.Constants.SqlSchemaName}.{StmALogSchema.Constants.TableName} AA
	INNER JOIN
	(
		SELECT {StmALog.Schema.SL_Parent}, MAX({StmALog.Schema.SL_PostedTimeUtc}) maxTime FROM {StmALogSchema.Constants.SqlSchemaName}.{StmALogSchema.Constants.TableName}
		WHERE {StmALog.Schema.SL_Table} = @jobDocumentDataTableName
		AND {StmALog.Schema.SL_IsCancelled} = @isCancelled
		AND {StmALog.Schema.SL_Reference} LIKE @fuzzyBookingRequest
		AND
		(
			{StmALog.Schema.SL_SE_NKEvent} = @statusUpdatedCode
			OR {StmALog.Schema.SL_SE_NKEvent} = @messageWithdrawCancelRequestCode
		)
		GROUP BY {StmALog.Schema.SL_Parent}
	) BB
	ON AA.{StmALog.Schema.SL_Parent} = BB.{StmALog.Schema.SL_Parent} AND AA.{StmALog.Schema.SL_PostedTimeUtc} = maxTime
	WHERE {StmALog.Schema.SL_SE_NKEvent} = @messageWithdrawCancelRequestCode
)";

				var parameters = new ZSqlParameterCollection();
				parameters.Add("@documentDataName", ConsolDocumentDataStoreNames.SeaBookingRequest2, JobDocumentDataSchema.JDD_Name);
				parameters.Add("@jobDocumentDataTableName", AutoJobDocumentData.Schema.TableName, StmALogSchema.SL_Table);
				parameters.Add("@isCancelled", "N", StmALogSchema.SL_IsCancelled);
				parameters.Add("@statusUpdatedCode", Events.StatusUpdatedCode, StmALogSchema.SL_SE_NKEvent);
				parameters.Add("@messageWithdrawCancelRequestCode", Events.MessageWithdrawCancelRequestCode, StmALogSchema.SL_SE_NKEvent);
				parameters.Add("@fuzzyBookingRequest", $"%{EventConstants.EventReferenceParameters.Codes.MessageType}={ConsolDocumentNames.BookingRequest}%", StmALogSchema.SL_Reference);

				query.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "{0} {1} ({2})", ForwardingConsol.Schema.PK, notIn ? (NoResString)"NOT IN" : "IN", hasMessageWithdrawCancelRequestCodeSQL), parameters);
			}
		}

		ZString GetMessageTypeByFlightStatus(ZString flightStatus)
		{
			var bookingRequestCodes = new List<ZString>()
			{
				FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Sent,
				FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Acknowledged,
				FreightConstants.CarrierBookingStatus.Codes.BookingRequest.RejectedByInterchange,
				FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Confirmed,
				FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Rejected,
				FreightConstants.CarrierBookingStatus.Codes.BookingRequest.WithdrawalSent,
				FreightConstants.CarrierBookingStatus.Codes.BookingRequest.WithdrawalRejected,
				FreightConstants.CarrierBookingStatus.Codes.BookingRequest.WithdrawalAccepted,
				FreightConstants.CarrierBookingStatus.Codes.BookingRequest.PendingProcessing,
			};

			return bookingRequestCodes.Contains(flightStatus) ? ConsolDocumentNames.BookingRequest : ConsolDocumentNames.ShippingInstruction;
		}

		ZString GetEventCodeByFlightStauts(ZString flightStatus)
		{
			switch (flightStatus)
			{
				case FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Sent:
				case FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Sent:
					return Events.MessageSentCode;

				case FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Acknowledged:
				case FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Acknowledged:
					return Events.InterchangeReceiptAcknowledgedCode;

				case FreightConstants.CarrierBookingStatus.Codes.BookingRequest.RejectedByInterchange:
				case FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.RejectedByInterchange:
					return Events.InterchangeRejectedCode;

				case FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Confirmed:
				case FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Confirmed:
					return Events.MessageAcceptedCode;

				case FreightConstants.CarrierBookingStatus.Codes.BookingRequest.WithdrawalSent:
					return Events.MessageWithdrawCancelRequestCode;

				case FreightConstants.CarrierBookingStatus.Codes.BookingRequest.WithdrawalAccepted:
					return Events.MessageWithdrawCancelAcceptedCode;

				case FreightConstants.CarrierBookingStatus.Codes.BookingRequest.WithdrawalRejected:
				case FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Rejected:
				case FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Rejected:
					return Events.MessageRejectedCode;

				case FreightConstants.CarrierBookingStatus.Codes.BookingRequest.PendingProcessing:
				case FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.PendingProcessing:
					return Events.MessagePendingProcessingCode;

				default:
					return ZString.Empty;
			}
		}

		#endregion

		#region GetAirBookingStatusQuery

		ZQuery GetAirBookingStatusQuery(ZString airBookingStatus)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingConsol));
			var airBookingSubQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			airBookingSubQuery.AddToFilter(JobConsolTransportSchema.JW_TransportMode, Constants.TransportModes.Air);

			switch (airBookingStatus)
			{
				case Constants.AirBookingStatus.Code.All:
					result.AddSubQuery(airBookingSubQuery, JoinCondition.And);
					break;

				case Constants.AirBookingStatus.Code.AllFlightsConfirmed:
					var inQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
					inQuery.AddToFilter(JobConsolTransportSchema.JW_Status, Constants.TransportStatus.Confirmed);
					inQuery.AddToFilter(JobConsolTransportSchema.JW_TransportMode, Constants.TransportModes.Air);
					result.AddSubQuery(inQuery, JoinCondition.And);

					var notInQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID, true);
					notInQuery.AddToFilter(JobConsolTransportSchema.JW_Status, SQLComparisonOperator.NotEqual, Constants.TransportStatus.Confirmed);
					notInQuery.AddToFilter(JobConsolTransportSchema.JW_TransportMode, Constants.TransportModes.Air);
					result.AddSubQuery(notInQuery, JoinCondition.And);
					break;

				case Constants.AirBookingStatus.Code.ConfirmationPending:
					airBookingSubQuery.AddToFilter(JobConsolTransportSchema.JW_Status, new string[]
					{
						Constants.TransportStatus.Requested,
						Constants.TransportStatus.Queued
					});
					result.AddSubQuery(airBookingSubQuery, JoinCondition.And);
					break;

				case Constants.AirBookingStatus.Code.CancellationPending:
					airBookingSubQuery.AddToFilter(JobConsolTransportSchema.JW_Status, Constants.TransportStatus.CancellationRequested);
					result.AddSubQuery(airBookingSubQuery, JoinCondition.And);
					break;

				case Constants.AirBookingStatus.Code.NotRequested:
					airBookingSubQuery.AddToFilter(JobConsolTransportSchema.JW_Status, new string[]
					{
						Constants.TransportStatus.Planned,
						Constants.TransportStatus.Cancelled,
						string.Empty
					});
					result.AddSubQuery(airBookingSubQuery, JoinCondition.And);
					break;

				case Constants.AirBookingStatus.Code.Rejected:
					airBookingSubQuery.AddToFilter(JobConsolTransportSchema.JW_Status, new string[]
					{
						Constants.TransportStatus.Unable,
						Constants.TransportStatus.FlightNotOperating
					});
					result.AddSubQuery(airBookingSubQuery, JoinCondition.And);
					break;
			}

			return result;
		}

		#endregion

		#region GetReleaseType

		ZQuery GetReleaseTypeQuery(ZString releaseType)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingConsol));
			result.AddToFilter(JobConsolSchema.JK_ReleaseType, releaseType);
			return result;
		}

		CodeDescriptionPairList ReleaseTypeList => FreightDataRegistry.Instance.ReleaseTypes.Value.GetCodeDescriptionPairList();

		#endregion

		#region GetPossibleOversize

		ZQuery GetPossibleOversizeQuery(ZBool isPossibleOversize)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingConsol));
			var queryOperator = isPossibleOversize ? string.Empty : "NOT";
			var sqlFilter = string.Format(CultureInfo.InvariantCulture, @"
JK_PK {0} IN (SELECT DISTINCT JK.JK_PK
			  FROM dbo.JobConsol AS JK
			  JOIN vw_ConsolPackLinesDimension AS CPD ON CPD.JK_PK = JK.JK_PK
			  WHERE
					(CPD.JS_TransportMode = 'AIR' AND (
						(CPD.maxLength <> 0 AND CPD.maxWidth <> 0 AND CPD.maxHeight <> 0 AND CPD.maxDimensionUnits <> '' AND 
							((NOT (CPD.ConvertedLength <= CPD.maxLength AND CPD.ConvertedWidth <= CPD.maxWidth) AND 
							NOT (CPD.ConvertedWidth <= CPD.maxLength AND CPD.ConvertedLength <= CPD.maxWidth)) OR 
							CPD.ConvertedHeight > CPD.maxHeight)) 
						OR
						(CPD.maxLength = 0 AND CPD.maxWidth = 0 AND CPD.maxHeight = 0 AND CPD.maxDimensionUnits = '' AND 
							((NOT (CPD.ConvertedLength <= 300 AND CPD.ConvertedWidth <= 200) AND 
							NOT (CPD.ConvertedWidth <= 300 AND CPD.ConvertedLength <= 200)) OR 
							CPD.ConvertedHeight > 160))))
					OR (CPD.JS_TransportMode = 'SEA' AND 
						((NOT (CPD.ConvertedLength <= 240 AND CPD.ConvertedWidth <= 96) AND 
							NOT (CPD.ConvertedWidth <= 240 AND CPD.ConvertedLength <= 96)) OR 
							CPD.ConvertedHeight> 102))
)", queryOperator); // SQL
			var sqlFilterParameters = new ZSqlParameterCollection();
			result.AddFilterAndZSQLParameterCollection(sqlFilter, sqlFilterParameters);

			return result;
		}

		#endregion

		#endregion

		#region Related Shipments Filter

		void AddRelatedShipmentsFilters(ModuleFilterCollection filters)
		{
			var filter = new ShipmentsOfConsolFilter(Descriptions.RelatedShipments, () => new ForwardingShipmentCollection(Factory));
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|RelatedShipments", "Related Shipments");
			filter.IsPublishedOnWeb = false;

			var description = ResString.GetMultilingualString("Forwarding|JobConsolFilter|Shipments", "Shipments");
			filter.Category = FilterCategories.GetOrCreateFilterCategory(description);

			filters.AddFilter(filter);
		}

		#endregion

		void AddRelatedShipmentsOSMGFilter(ModuleFilterCollection filters)
		{
			if (!FreightDataRegistry.Instance.ConsolAllowAccessRegardlessOfShipmentsOSMGRights.Value && new JobShipmentCRMSecurityProvider().HasRestrictions)
			{
				var filter = new ShipmentsOfConsolFilter(Descriptions.RelatedShipmentsSecurity, () => new ForwardingShipmentCollection(Factory));
				filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|RelatedShipmentsSecurityFilter", "Related Shipments Security Filter");
				filter.IsPublishedOnWeb = false;
				filter.Category = FilterCategories.CRMSecurity;
				filter.GroupOrCategory = FilterOrCategory.None;
				filter.IsGroupOrCategoryReadOnly = true;
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
				filter.Visibility = FilterVisibility.AlwaysApplied | FilterVisibility.AlwaysVisible;
				filter.ReadOnly = true;
				filter.OrCategory = FilterOrCategory.MandatoryFilterOrCategory;
				filter.IsOrCategoryReadOnly = true;
				filter.IsMandatorySecurityFilter = true;
				filters.AddFilter(filter);
			}
		}

		#region ModuleFilter Lists

		CodeDescriptionPairList GatewayType_List => gatewayType_List ?? (gatewayType_List = FreightCodePairLists.GatewayForwarderHandlingTypeList());

		CodeDescriptionPairList gatewayType_List;

		CodeDescriptionPairList ContainerMode_List
		{
			get { return containerMode_List ?? (containerMode_List = FreightCodePairLists.ConsolModeList(string.Empty, string.Empty)); }
		}
		CodeDescriptionPairList containerMode_List;

		CodeDescriptionPairList TransportMode_List
		{
			get { return FreightCodePairLists.LinkableTransportModeList(); }
		}

		CodeDescriptionPairList ConsolType_List
		{
			get
			{
				if (consolType_List == null)
				{
					consolType_List = new CodeDescriptionPairList(OLookUpEditType.AgentType);
					consolType_List.AddPair(Constants.AgentType.AWBCoload, Constants.AgentTypeDescriptions.AWBCoload);
					consolType_List.AddPair(Constants.AgentType.AWBMaster, Constants.AgentTypeDescriptions.AWBMaster);
				}

				return consolType_List;
			}
		}
		CodeDescriptionPairList consolType_List;

		IList GetCarrierServiceLevelList()
		{
			ModuleGuidFilter carrierFilter = (ModuleGuidFilter)this["Carrier"];
			if (carrierFilter.IsActive && carrierFilter.Property.IsValid)
			{
				OrgHeader carrier = Factory.Load<OrgHeader>(carrierFilter.Property);
				carrier.MiscServ.CarrierServiceLevels.Load();
				return carrier.MiscServ.CarrierServiceLevels;
			}
			return new CodeDescriptionPairList();
		}

		CodeDescriptionPairList ElectronicBillStatus_List => electronicBillStatus_List ?? (electronicBillStatus_List = FreightCodePairLists.BillOfLadingBillStatusList());
		CodeDescriptionPairList electronicBillStatus_List;

		CodeDescriptionPairList ElectronicBillTerms_List => electronicBillTerms_List ?? (electronicBillTerms_List = FreightCodePairLists.BillOfLadingBillTermsList());
		CodeDescriptionPairList electronicBillTerms_List;

		CodeDescriptionPairList ElectronicBillType_List => electronicBillType_List ?? (electronicBillType_List = FreightCodePairLists.BillOfLadingBillTypeList());
		CodeDescriptionPairList electronicBillType_List;

		CodeDescriptionPairList RoutingStatus_List
		{
			get
			{
				if (routingStatus_List == null)
				{
					routingStatus_List = new CodeDescriptionPairList
					{
						new CodeDescriptionPair(
							RoutingStatusAll,
							Res.GetString("JobConsolFilter|RoutingStatusList|All", "All")),
						new CodeDescriptionPair(
							RoutingStatusIncomplete,
							Res.GetString("JobConsolFilter|RoutingStatusList|Incomplete", "Only Consols with Incomplete Routing Information")),
						new CodeDescriptionPair(
							RoutingStatusComplete,
							Res.GetString("JobConsolFilter|RoutingStatusList|Complete", "Only Consols with Complete Routing Information"))
					};
				}

				return routingStatus_List;
			}
		}
		CodeDescriptionPairList routingStatus_List;

		const string RoutingStatusAll = "ALL";
		const string RoutingStatusIncomplete = "INC";
		const string RoutingStatusComplete = "COM";

		CodeDescriptionPairList PreAllocatedAmountExceeded_List
		{
			get
			{
				if (preAllocatedAmountExceeded_List == null)
				{
					preAllocatedAmountExceeded_List = new CodeDescriptionPairList();
					preAllocatedAmountExceeded_List.AddPair(PreAllocatedAmountExceededCodes.All, Res.GetString("JobConsolFilter|Pre-AllocatedAmountExceededList|All", "All"));
					preAllocatedAmountExceeded_List.AddPair(PreAllocatedAmountExceededCodes.Exceeded, Res.GetString("JobConsolFilter|Pre-AllocatedAmountExceededList|Exceeded", "Consols with Pre-Allocated Amount Exceeded"));
					preAllocatedAmountExceeded_List.AddPair(PreAllocatedAmountExceededCodes.NotExceeded, Res.GetString("JobConsolFilter|Pre-AllocatedAmountExceededList|NotExceeded", "Consols with Pre-Allocated Amount Not Exceeded"));
					preAllocatedAmountExceeded_List.AddPair(PreAllocatedAmountExceededCodes.ExceededUnderCurrentBranchAndDepartment, Res.GetString("JobConsolFilter|ExceededUnderCurrentBranchDepartment", "Consols with Pre-Allocated Amount Exceeded under current Branch/Dept"));
					preAllocatedAmountExceeded_List.AddPair(PreAllocatedAmountExceededCodes.NotExceededUnderCurrentBranchAndDepartment, Res.GetString("JobConsolFilter|NotExceededUnderCurrentBranchDepartment", "Consols with Pre-Allocated Amount Not Exceeded under current Branch/Dept"));
				}

				return preAllocatedAmountExceeded_List;
			}
		}
		CodeDescriptionPairList preAllocatedAmountExceeded_List;

		CodeDescriptionPairList PhaseList
		{
			get
			{
				if (phaseList == null)
				{
					phaseList = PhaseConstants.GetCommonPhaseList();
					IPhaseSecurity phaseSecurity = ForwardingConfigurationRegistry.Instance.ConsolPhaseSecurity.Value;
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

		BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

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

		CodeDescriptionPairList AirfreightSecurityStatusList => Factory.GetCachedValue("JobConsolFilterBusinessObject_AirfreightSecurityStatusList", () =>
		{
			var result = new CodeDescriptionPairList();
			foreach (ICodeDescription codePair in new AWBSpecialHandlingCodeDescriptionPairList())
			{
				if (AWBSpecialHandlingCodeDescriptionPairList.IsCargoSecurityStatusCode(codePair.Code))
				{
					result.Add(codePair);
				}
			}

			return result;
		});

		CodeDescriptionPairList AirBookingStatusList => Factory.GetCachedValue("JobConsolFilterBusinessObject_AirBookingStatusList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Constants.AirBookingStatus.Code.All, Constants.AirBookingStatus.Description.All);
			result.AddPair(Constants.AirBookingStatus.Code.AllFlightsConfirmed, Constants.AirBookingStatus.Description.AllFlightsConfirmed);
			result.AddPair(Constants.AirBookingStatus.Code.ConfirmationPending, Constants.AirBookingStatus.Description.ConfirmationPending);
			result.AddPair(Constants.AirBookingStatus.Code.CancellationPending, Constants.AirBookingStatus.Description.CancellationPending);
			result.AddPair(Constants.AirBookingStatus.Code.NotRequested, Constants.AirBookingStatus.Description.NotRequested);
			result.AddPair(Constants.AirBookingStatus.Code.Rejected, Constants.AirBookingStatus.Description.Rejected);

			return result;
		});

		CodeDescriptionPairList FlightStatusList => JobConsolTransportLookups.GetFlightStatusList(Factory);

		CodeDescriptionPairList CarrierBookingStatusList => FreightCodePairLists.CarrierBookingStatusList();

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

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingConsol));
			result.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			var gatewayConsolBillingJobSubQueryParams = new ZSqlParameterCollection();
			gatewayConsolBillingJobSubQueryParams.Add("@CurrentCompany", GlbCompany.CurrentCompany.PK, GlbBranchSchema.GB_GC);
			var gatewayConsolBillingQuery = string.Format(CultureInfo.InvariantCulture,
			@"{0} IN (
SELECT {0}
From dbo.JobConsol
INNER JOIN dbo.JobHeader ON JK_PK = JH_ParentID AND JH_GC = @CurrentCompany
INNER JOIN dbo.OrgAddress AS SendingAgentAddress ON JK_OA_SendingForwarderAddress = SendingAgentAddress.OA_PK
INNER JOIN dbo.OrgHeader AS SendingAgent ON SendingAgentAddress.OA_OH = SendingAgent.OH_PK
WHERE
JK_OA_SendingForwarderAddress IS NOT NULL
AND JK_SendingForwarderHandlingType IN ('GTA', 'GTT')
AND OH_PK IN (@PlaceHolderForPKs)

UNION 

SELECT {0}
FROM dbo.JobConsol
INNER JOIN dbo.JobHeader ON JK_PK = JH_ParentID AND JH_GC = @CurrentCompany
INNER JOIN dbo.OrgAddress AS ReceivingAgentAddress ON JK_OA_ReceivingForwarderAddress = ReceivingAgentAddress.OA_PK
INNER JOIN dbo.OrgHeader AS ReceivingAgent ON ReceivingAgentAddress.OA_OH = ReceivingAgent.OH_PK
WHERE
JK_OA_ReceivingForwarderAddress IS NOT NULL
AND JK_ReceivingForwarderHandlingType IN ('GTA', 'GTT')
AND OH_PK IN (@PlaceHolderForPKs)
)".Replace("@PlaceHolderForPKs", ConvertGuidListToString(OrgProxiesOfCurrentCompanyAndItsBranches)),
			JobConsolSchema.PK.Name);

			result.AddFilterAndZSQLParameterCollection(gatewayConsolBillingQuery, gatewayConsolBillingJobSubQueryParams);

			return result;
		}

		ZString ConvertGuidListToString(Guid[] list)
		{
			var builder = new ZStringBuilder();
			list.ForEach(x => builder.Append(x.ToSqlGuid()));
			return builder.ToStringWithDelimiterBetweenAppends(",");
		}

		Guid[] OrgProxiesOfCurrentCompanyAndItsBranches
		{
			get
			{
				if (orgProxiesOfCurrentCompanyAndItsBranches == null)
				{
					var branchOrgProxies = GlbCompany.CurrentCompany.Branches.Where(x => !x.GB_OH_OrgProxy.IsEmpty).Select(x => x.GB_OH_OrgProxy.ToGuid());
					if (!GlbCompany.CurrentCompany.GC_OH_OrgProxy.IsEmpty)
					{
						branchOrgProxies = branchOrgProxies.Append(GlbCompany.CurrentCompany.GC_OH_OrgProxy.ToGuid());
					}

					orgProxiesOfCurrentCompanyAndItsBranches = branchOrgProxies.Distinct().OrderBy(x => x).ToArray();
				}
				return orgProxiesOfCurrentCompanyAndItsBranches;
			}
		}
		Guid[] orgProxiesOfCurrentCompanyAndItsBranches;

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride => null;

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride => null;

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories => null;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new Dictionary<string, object>()
		{
			{ AccountingFilterStripConfigurationKeys.BusinessObjectType, typeof(ForwardingConsol) }
		};

		#endregion

		#region ITemplateRecordFilterProvider

		bool ITemplateRecordFilterProvider.ShouldApplyTemplateRecordFiltersLayout { get; set; }

		void ApplyTemplateRecordFiltersLayout(ModuleFilterCollection filters)
		{
			if (AllowTemplateRecords && ((ITemplateRecordFilterProvider)this).ShouldApplyTemplateRecordFiltersLayout)
			{
				var templateRecordsFilter = (ModuleTextFilter)filters[TemplateRecordsDescription];
				if (templateRecordsFilter != null)
				{
					templateRecordsFilter.Visibility = FilterVisibility.AlwaysVisible;
					templateRecordsFilter.ReadOnly = true;
				}

				var templateActiveFilter = (ModuleTextFilter)filters[TemplateRecordsActive];
				if (templateActiveFilter != null)
				{
					templateActiveFilter.Visibility = FilterVisibility.AlwaysVisible;
					templateActiveFilter.ReadOnly = true;
				}
			}
		}

		#endregion
	}
}
