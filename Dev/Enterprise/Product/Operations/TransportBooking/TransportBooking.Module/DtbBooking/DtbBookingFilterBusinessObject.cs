using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Module;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Packing.Business;
using Enterprise.Packing.Module;
using Enterprise.Security;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportBookings.Shared.Lists;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Module;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Module
{
	public sealed class DtbBookingFilterBusinessObject : FilterStripBusinessObject,
		IAccountingFilterStripHolder
	{
		void AddModuleFilters(ModuleFilterCollection filters)
		{
			AddTextFilters(filters);
			AddDateFilters(filters);
			AddNumbersAndRefsFilters(filters);
			AddStatusAndFlagsFilters(filters);
			AddLocationFilters(filters);
			AddModesAndTypesFilters(filters);
			SecurityProvider.AddCRMSecurityFilterStrips(Factory, filters);
		}

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			var jobIDFilter = new ModuleFountainFilter(FilterNameConstants.BookingID, DtbBookingSchema.KM_JobID, "TB");
			jobIDFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|BookingID", "Booking ID");

			return jobIDFilter;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var bookingConsolidationTemplateFilter = filters.AddTextFilter(FilterNameConstants.BookingConsolidationTemplate, BookingTemplateQuery);
			bookingConsolidationTemplateFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|BookingTemplate", "Booking Template");
			bookingConsolidationTemplateFilter.MaxLength = DtbBookingSchema.KM_KT_NKBookingTemplate.MaxLength;

			var bookingConsolidationDirectionFilter = filters.AddTextFilter(FilterNameConstants.BookingConsolidationJobDirection, BookingConsolidationDirectionQuery, BindToLists.BookingConsolidationJobDirections);
			bookingConsolidationDirectionFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|BookingJobDirection", "Booking Job Direction");
			bookingConsolidationDirectionFilter.MaxLength = DtbBookingConsolidationSchema.KB_JobDirection.MaxLength;

			var bookingDirectionFilter = filters.AddTextFilter(FilterNameConstants.BookingDirection, BookingDirectionQuery, BindToLists.Directions);
			bookingDirectionFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|MovementJobDirection", "Booking Direction");
			bookingDirectionFilter.MaxLength = DtbBookingSchema.KM_Direction.MaxLength;

			var instructionTypeFilter = filters.AddTextFilter(FilterNameConstants.InstructionType, InstructionTypeQuery);
			instructionTypeFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|InstructionType", "Instruction Type");
			instructionTypeFilter.MaxLength = DtbBookingInstructionSchema.KN_InstructionType.MaxLength;

			filters.AddTextFilter(FilterNameConstants.ConfirmationType, ConfirmationDescriptionQuery, BindToLists.ConfirmationTypes).MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|ConfirmationDescription", "Confirmation Type");

			var confirmationReferenceNumberFilter = filters.AddTextFilter(FilterNameConstants.ConfirmationReferenceNumber, ConfirmationReferenceNumberQuery);
			confirmationReferenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|ConfirmationReferenceNumber", "Confirmation Reference Number");
			confirmationReferenceNumberFilter.MaxLength = DtbBookingConfirmationSchema.KK_ReferenceNum.MaxLength;

			var confirmationReceivedByFilter = filters.AddTextFilter(FilterNameConstants.ConfirmationReceivedBy, ConfirmationReceivedByQuery);
			confirmationReceivedByFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|ConfirmationReceivedBy", "Confirmation Received By");
			confirmationReceivedByFilter.MaxLength = DtbBookingConfirmationSchema.KK_ReceivedBy.MaxLength;

			var confirmationSlotReferenceFilter = filters.AddTextFilter(FilterNameConstants.ConfirmationSlotReference, ConfirmationSlotReferenceQuery);
			confirmationSlotReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|ConfirmationSlotReference", "Confirmation Slot Reference");
			confirmationSlotReferenceFilter.MaxLength = DtbBookingConfirmationSchema.KK_SlotReference.MaxLength;

			var voyageVesselFilter = filters.AddTextAndNkFilter(FilterNameConstants.VoyageVessel, GetFlightVoyageAndVesselQuery, ModuleIDs.RefVessel, new RefVesselCollection(Factory));
			voyageVesselFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|VesselVoyage", "Vessel and Flight/Voyage #");
			voyageVesselFilter.SubGroup = ParentLegsSubGroup;
			voyageVesselFilter.NkMaxLength = ViewTransportBookingParentLegsSchema.VL_Vessel.MaxLength;
			voyageVesselFilter.MaxLength = ViewTransportBookingParentLegsSchema.VL_VoyageFlight.MaxLength;

			var instructionNotesFilter = filters.AddTextFilter(FilterNameConstants.InstructionNotes, InstructionNotesQuery);
			instructionNotesFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|InstructionNotes", "Instruction Notes");
			instructionNotesFilter.MaxLength = Math.Min(ModuleFilter.MaxMaximumLength, DtbBookingInstructionSchema.KN_ServiceInstruction.MaxLength);

			var ctoReferenceFilter = filters.AddTextFilter(FilterNameConstants.CTOReference, CTOReferenceQuery);
			ctoReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|CTOReference", "CTO Reference");
			ctoReferenceFilter.MaxLength = DtbBookingConfirmationSchema.KK_ReferenceNum.MaxLength;

			var cydReferenceFilter = filters.AddTextFilter(FilterNameConstants.CYDReference, CYDReferenceQuery);
			cydReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|CYDReference", "CYD Reference");
			cydReferenceFilter.MaxLength = DtbBookingConfirmationSchema.KK_ReferenceNum.MaxLength;
		}

		ZQuery CTOReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetReferenceQuery(comparisonOperator, value, AutoDocAddressTypes.Codes.LocalCartageCTO);
		}

		ZQuery CYDReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetReferenceQuery(comparisonOperator, value, AutoDocAddressTypes.Codes.LocalCartageYard);
		}

		static ZQuery GetReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value, string addressType)
		{
			var booking = new ZDBOnlyQuery(typeof(DtbBooking));
			var instruction = new ZDBOnlySubQuery(typeof(DtbBookingInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement);
			var jobDocAddress = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			var confirmation = new ZDBOnlySubQuery(typeof(DtbBookingConfirmation), DtbBookingConfirmationSchema.KK_KN_BookingInstruction);
			confirmation.AddToFilter(DtbBookingConfirmationSchema.KK_ReferenceNum, comparisonOperator, value);
			jobDocAddress.AddToFilter(JobDocAddressSchema.E2_AddressType, addressType);
			instruction.AddSubQuery(jobDocAddress, JoinCondition.And);
			instruction.AddSubQuery(confirmation, JoinCondition.And);
			booking.AddSubQuery(instruction, JoinCondition.And);
			return booking;
		}

		ZQuery GetFlightVoyageAndVesselQuery(SQLComparisonOperator comparisonOperator, ZString voyage, ZString vessel)
		{
			var query = new ZQuery();

			if (!voyage.IsEmpty || comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				query.AddToFilter(ViewTransportBookingParentLegsSchema.VL_VoyageFlight, comparisonOperator, voyage);
			}

			if (!vessel.IsEmpty || comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				query.AddToFilter(ViewTransportBookingParentLegsSchema.VL_Vessel, comparisonOperator, vessel);
			}

			return query;
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(FilterNameConstants.DeliveryEstimated, DeliveryEstimatedQuery).MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|DeliveryEstimated", "Delivery Estimated");
			filters.AddDateFilter(FilterNameConstants.DeliveryActual, DeliveryActualQuery).MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|DeliveryActual", "Delivery Actual");
			filters.AddDateFilter(FilterNameConstants.DeliveryRequiredFrom, DeliveryRequiredFromQuery).MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|DeliveryRequiredFrom", "Delivery Required From");
			filters.AddDateFilter(FilterNameConstants.DeliveryRequiredTo, DeliveryRequiredToQuery).MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|DeliveryRequiredTo", "Delivery Required To");
			filters.AddDateFilter(FilterNameConstants.DeliverySlotDate, DeliverySlotDateQuery).MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|DeliverySlotDate", "Delivery Slot Date");

			filters.AddDateFilter(FilterNameConstants.PickupEstimated, PickupEstimatedQuery).MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|PickupEstimated", "Pickup Estimated");
			filters.AddDateFilter(FilterNameConstants.PickupActual, PickupActualQuery).MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|PickupActual", "Pickup Actual");
			filters.AddDateFilter(FilterNameConstants.PickupRequiredFrom, PickupRequiredFromQuery).MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|PickupRequiredFrom", "Pickup Required From");
			filters.AddDateFilter(FilterNameConstants.PickupRequiredTo, PickupRequiredToQuery).MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|PickupRequiredTo", "Pickup Required To");
			filters.AddDateFilter(FilterNameConstants.PickupSlotDate, PickupSlotDateQuery).MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|PickupSlotDate", "Pickup Slot Date");

			filters.AddDateFilter(FilterNameConstants.BookingRequestedDate, DtbBookingSchema.KM_BookingOfTransportRequestedDate).MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|BookingRequested", "Booking Requested Date");

			ModuleDateFilter etdFilter = filters.AddDateFilter(FilterNameConstants.ETD, ViewTransportBookingParentLegsSchema.VL_ETD);
			etdFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|ETD", "Load ETD");
			etdFilter.SubGroup = ParentLegsSubGroup;

			ModuleDateFilter atdFilter = filters.AddDateFilter(FilterNameConstants.ATD, ViewTransportBookingParentLegsSchema.VL_ATD);
			atdFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|ATD", "Load ATD");
			atdFilter.SubGroup = ParentLegsSubGroup;

			ModuleDateFilter etaFilter = filters.AddDateFilter(FilterNameConstants.ETA, ViewTransportBookingParentLegsSchema.VL_ETA);
			etaFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|ETA", "Discharge ETA");
			etaFilter.SubGroup = ParentLegsSubGroup;

			ModuleDateFilter ataFilter = filters.AddDateFilter(FilterNameConstants.ATA, ViewTransportBookingParentLegsSchema.VL_ATA);
			ataFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|ATA", "Discharge ATA");
			ataFilter.SubGroup = ParentLegsSubGroup;

			ModuleDateFilter fclReceivalFilter = filters.AddDateFilter(FilterNameConstants.FclReceival, ViewTransportBookingParentLegsSchema.VL_FCLReceivalCommences);
			fclReceivalFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|CTOReceivalCommences", "CTO Receival Start");
			fclReceivalFilter.SubGroup = ParentLegsSubGroup;

			ModuleDateFilter fclCutOffFilter = filters.AddDateFilter(FilterNameConstants.FclCutOff, ViewTransportBookingParentLegsSchema.VL_FCLCutOff);
			fclCutOffFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|CTOCutOff", "CTO Cut Off");
			fclCutOffFilter.SubGroup = ParentLegsSubGroup;

			ModuleDateFilter fclDgReceivalFilter = filters.AddDateFilter(FilterNameConstants.FclDgReceival, ViewTransportBookingParentLegsSchema.VL_FCLDGReceivalCommences);
			fclDgReceivalFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|CTODGReceivalCommences", "CTO DG Receival Start");
			fclDgReceivalFilter.SubGroup = ParentLegsSubGroup;

			ModuleDateFilter fclDgCutOffFilter = filters.AddDateFilter(FilterNameConstants.FclDgCutOff, ViewTransportBookingParentLegsSchema.VL_FCLDGCutOff);
			fclDgCutOffFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|CTODGCutOff", "CTO DG Cut Off");
			fclDgCutOffFilter.SubGroup = ParentLegsSubGroup;

			ModuleDateFilter fclAvailFilter = filters.AddDateFilter(FilterNameConstants.FclAvailability, ViewTransportBookingParentLegsSchema.VL_FCLAvailabilityDate);
			fclAvailFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|CTOAvailability", "CTO Availability");
			fclAvailFilter.SubGroup = ParentLegsSubGroup;

			ModuleDateFilter fclStorageFilter = filters.AddDateFilter(FilterNameConstants.FclStorage, ViewTransportBookingParentLegsSchema.VL_FCLStorageDate);
			fclStorageFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|CTOStorage", "CTO Storage Start");
			fclStorageFilter.SubGroup = ParentLegsSubGroup;

			ModuleDateFilter lclReceivalFilter = filters.AddDateFilter(FilterNameConstants.LclReceival, ViewTransportBookingParentLegsSchema.VL_LCLReceivalCommences);
			lclReceivalFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|CFSReceivalCommences", "CFS Receival Start");
			lclReceivalFilter.SubGroup = ParentLegsSubGroup;

			ModuleDateFilter lclCutOffFilter = filters.AddDateFilter(FilterNameConstants.LclCutOff, ViewTransportBookingParentLegsSchema.VL_LCLCutOff);
			lclCutOffFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|CFSCutOff", "CFS Cut Off");
			lclCutOffFilter.SubGroup = ParentLegsSubGroup;

			ModuleDateFilter lclAvailFilter = filters.AddDateFilter(FilterNameConstants.LclAvailability, ViewTransportBookingParentLegsSchema.VL_LCLAvailabilityDate);
			lclAvailFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|CFSAvailability", "CFS Availability");
			lclAvailFilter.SubGroup = ParentLegsSubGroup;

			ModuleDateFilter lclStorageFilter = filters.AddDateFilter(FilterNameConstants.LclStorage, ViewTransportBookingParentLegsSchema.VL_LCLStorageDate);
			lclStorageFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|CFSStorage", "CFS Storage Start");
			lclStorageFilter.SubGroup = ParentLegsSubGroup;
		}

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var forwarders = new ForwarderCollection(Factory);
			var organisations = new OrgHeaderCollection(Factory);
			var staffs = new GlbStaffCollection(Factory);

			var bookingCompanyCodeFilter = filters.AddGuidFilter(FilterNameConstants.TransportCompany, ModuleIDs.Organisation, GetJobDocAddressQueryWithOperatorDelegate(DocAddressType.TransportCompanyDocumentaryAddress), BindToLists.LocalTransportOrganisations);
			var instructionCompanyNameFilter = filters.AddTextFilter(FilterNameConstants.InstructionCompanyName, InstructionCompanyNameQuery);
			instructionCompanyNameFilter.MaxLength = Math.Min(OrgHeaderSchema.OH_FullName.MaxLength, JobDocAddressSchema.E2_CompanyName.MaxLength);
			var instructionCompanyCodeFilter = filters.AddGuidFilter(FilterNameConstants.InstructionCompanyCode, ModuleIDs.Organisation, InstructionCompanyCodeQuery, organisations);

			var instructionAddressTypeFilter = filters.AddTextFilter(FilterNameConstants.InstructionOrgType, InstructionAddressTypeQuery, BindToLists.OrganisationTypes);
			var instructionCompanyRelatedPortFilter = filters.AddTextFilter(FilterNameConstants.InstructionCompanyRelatedPort, InstructionCompanyRelatedPortQuery);
			instructionCompanyRelatedPortFilter.MaxLength = Math.Min(OrgHeaderSchema.OH_RL_NKClosestPort.MaxLength, OrgAddressSchema.OA_RL_NKRelatedPortCode.MaxLength);

			bookingCompanyCodeFilter.Category = FilterCategories.Organisations;
			bookingCompanyCodeFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|BookingTransportCompanyCode", "Transport Company");
			instructionCompanyNameFilter.Category = FilterCategories.Organisations;
			instructionCompanyNameFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|InstructionCompanyName", "Instruction Company Name");
			instructionCompanyCodeFilter.Category = FilterCategories.Organisations;
			instructionCompanyCodeFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|InstructionCompanyCode", "Instruction Company Code");

			instructionCompanyRelatedPortFilter.Category = FilterCategories.Organisations;
			instructionCompanyRelatedPortFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|RelatedPort", "Instruction Company Related Port");
			instructionAddressTypeFilter.Category = FilterCategories.Organisations;
			instructionAddressTypeFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|InstructionAddressType", "Instruction Organization Type");

			var clientFilter = filters.AddGuidFilter(FilterNameConstants.LocalClient, ModuleIDs.Organisation, GetBillingPartyFilter, organisations);
			clientFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|BillingParty", "Billing Party");

			var brokerFilter = filters.AddGuidFilter(FilterNameConstants.CustomsBroker, ModuleIDs.Organisation, GetCustomsBrokerFilter, organisations);
			brokerFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|CustomsBroker", "Customs Broker");
			brokerFilter.SubGroup = ParentLegsSubGroup;

			var consignorConsigneeFilter = filters.AddGuidFilter(FilterNameConstants.ConsignorConsignee, ModuleIDs.Organisation, GetConsignorConsigneeFilter, BindToLists.ConsignorOrganisations, BindToLists.ConsigneeOrganisations);
			consignorConsigneeFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|ConsignorConsignee", "Consignor / Consignee");
			consignorConsigneeFilter.SetItemDescriptions(Res.GetData("ad995213-1434-4379-b49a-835277b9e8fa", "Consignor"), Res.GetData("47ba64eb-9f41-4701-984d-7169339705dd", "Consignee"));

			var controllingPartyFilter = filters.AddGuidFilter(FilterNameConstants.ControllingCustomer, ModuleIDs.Organisation, GetControllingParty, organisations);
			controllingPartyFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|ControllingParty", "Controlling Party");

			var carrierFilter = filters.AddGuidFilter(FilterNameConstants.Carrier, ModuleIDs.Organisation, ViewTransportBookingParentLegsSchema.VL_OH_Carrier, new ShippingProviderCollection(Factory));
			carrierFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|Carrier", "Carrier");
			carrierFilter.SubGroup = ParentLegsSubGroup;
			carrierFilter.SupportsFiltersMatchComparisonOperator = false; // Doesn't seem to work well since the business objects are from a view

			var bookedByFilter = filters.AddGuidFilter(FilterNameConstants.BookedBy, ModuleIDs.Organisation, GetBookedBy, organisations);
			bookedByFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|BookedBy", "Booked By");

			var sendingReceivingAgentFilter = filters.AddGuidFilter(FilterNameConstants.SendingReceivingAgent, ModuleIDs.Organisation, GetSendingReceivingAgentFilter, forwarders, forwarders);
			sendingReceivingAgentFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|SendingReceivingAgent", "Sending / Receiving Agent");
			sendingReceivingAgentFilter.SetItemDescriptions(Res.GetData("b611f2e6-4bb0-4fd2-b419-91c6abaf2d81", "Sending Agent"), Res.GetData("8a3c7f46-3335-4dbe-aa78-97f105321f63", "Receiving Agent"));

			var cartageCoordFilter = filters.AddGuidFilter(FilterNameConstants.CartageCoordinator, ModuleIDs.GlbStaff, GetCartageCoordinatorQuery, staffs);
			cartageCoordFilter.Category = FilterCategories.Organisations;
			cartageCoordFilter.MultilingualDescription = ResString.GetMultilingualString("77336891-272d-4a1d-8407-c1def6539177", "Cartage Coordinator");

			var carrierBookingAgentFilter = filters.AddGuidFilter(FilterNameConstants.CarrierBookingAgent, ModuleIDs.Organisation, GetJobDocAddressQueryWithOperatorDelegate(DocAddressType.CarrierBookingAgent), BindToLists.AllOrganisations);
			carrierBookingAgentFilter.Category = FilterCategories.Organisations;
			carrierBookingAgentFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|CarrierBookingAgent", "Carrier Booking Agent");

			var branchFilter = filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, DtbBookingSchema.KM_GB_Branch, new GlbBranchCollection(Factory));
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("DtbBookingFilterBusinessObject|Branch", "Branch");
			branchFilter.SupportsFiltersMatchComparisonOperator = false;
		}

		ZQuery GetBillingPartyFilter(ZGuid client)
		{
			// ClientReqBillToParty
			var clientReqBillToPartyFilter = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(DocAddressTypes.Codes.ClientRequestedBillingParty, client);

			// Billing Job
			var billingJobFilter = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr);
			billingJobFilter.AddToFilter(OrgAddressSchema.OA_OH, client);

			var headerFilter = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			headerFilter.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			headerFilter.AddSubQuery(billingJobFilter, JoinCondition.And);

			var headerNotInFilter = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID, true);
			headerNotInFilter.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			clientReqBillToPartyFilter.AddSubQuery(JobDocAddressSchema.E2_ParentID, headerNotInFilter, JoinCondition.And);

			// Bookings
			var bookingConsolidation = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking);
			bookingConsolidation.AddSubQuery(DtbBookingConsolidationSchema.KB_ParentID, headerFilter, JoinCondition.And);

			var standaloneBooking = new ZDBOnlySubQuery(typeof(DtbBooking), DtbBookingSchema.PK);
			standaloneBooking.AddSubQuery(headerFilter, JoinCondition.Or);
			standaloneBooking.AddSubQuery(clientReqBillToPartyFilter, JoinCondition.Or);

			var result = new ZDBOnlyQuery(typeof(DtbBooking));
			result.AddSubQuery(bookingConsolidation, JoinCondition.And);
			result.AddSubQuery(standaloneBooking, JoinCondition.Or);

			return result;
		}

		ZQuery GetCustomsBrokerFilter(ZGuid customsBroker)
		{
			ZQuery result = new ZQuery();

			if (!customsBroker.IsEmpty)
			{
				result.DefaultJoinCondition = JoinCondition.Or;
				result.AddToFilter(ViewTransportBookingParentLegsSchema.VL_OH_ExportBroker, customsBroker);
				result.AddToFilter(ViewTransportBookingParentLegsSchema.VL_OH_ImportBroker, customsBroker);
			}

			return result;
		}

		ZQuery GetConsignorConsigneeFilter(ZGuid consignor, ZGuid consignee)
		{
			ZQuery result = new ZQuery();

			if (!consignor.IsEmpty)
			{
				result.AddToFilter(GetConsignorConsigneeFilter(consignor, DocAddressTypes.Codes.ConsignorDocumentaryAddress, DocAddressTypes.Codes.LocalCartageExporter));
			}

			if (!consignee.IsEmpty)
			{
				result.AddToFilter(GetConsignorConsigneeFilter(consignee, DocAddressTypes.Codes.ConsigneeDocumentaryAddress, DocAddressTypes.Codes.LocalCartageImporter));
			}

			return result;
		}

		ZQuery GetControllingParty(ZGuid controllingParty)
		{
			ZQuery result = new ZQuery();

			if (!controllingParty.IsEmpty)
			{
				result.AddToFilter(GetParentJobDocAddressFilter(controllingParty, DocAddressTypes.Codes.ControllingCustomer));
			}

			return result;
		}

		ZQuery GetBookedBy(ZGuid bookedByParty)
		{
			var result = new ZQuery();

			if (!bookedByParty.IsEmpty)
			{
				result.AddToFilter(GetBookedByJobDocAddressFilter(bookedByParty, DocAddressTypes.Codes.BookingPartyDocumentaryAddress));
			}

			return result;
		}

		ZQuery GetBookedByJobDocAddressFilter(ZGuid orgPK, ZString type)
		{
			var docAddressFilter = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(type, orgPK);

			var bookingConsolidation = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking);
			bookingConsolidation.AddSubQuery(DtbBookingConsolidationSchema.PK, docAddressFilter, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(DtbBooking));
			result.AddSubQuery(bookingConsolidation, JoinCondition.And);

			return result;
		}

		ZQuery GetParentJobDocAddressFilter(ZGuid orgPK, ZString type)
		{
			var docAddressFilter = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(type, orgPK);

			var bookingConsolidation = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking);
			bookingConsolidation.AddSubQuery(DtbBookingConsolidationSchema.KB_ParentID, docAddressFilter, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(DtbBooking));
			result.AddSubQuery(bookingConsolidation, JoinCondition.And);

			return result;
		}

		ZQuery GetConsignorConsigneeFilter(ZGuid orgPK, ZString docTypeOfConsolidation, ZString docTypeOfStandalone)
		{
			var docAddressFilter = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(docTypeOfConsolidation, orgPK);

			var bookingConsolidation = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking);
			bookingConsolidation.AddSubQuery(DtbBookingConsolidationSchema.KB_ParentID, docAddressFilter, JoinCondition.And);

			// standalone
			var docAddressFilter4Standalone = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(docTypeOfStandalone, orgPK);

			// DtbBookingInstruction
			var instruction = new ZDBOnlySubQuery(typeof(DtbBookingInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement);
			instruction.AddSubQuery(DtbBookingInstructionSchema.PK, docAddressFilter4Standalone, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(DtbBooking));
			result.AddSubQuery(bookingConsolidation, JoinCondition.And);
			result.AddSubQuery(instruction, JoinCondition.Or);

			return result;
		}

		ZQuery GetSendingReceivingAgentFilter(ZGuid sendingAgent, ZGuid receivingAgent)
		{
			if (sendingAgent.IsEmpty && receivingAgent.IsEmpty)
			{
				return new ZQuery();
			}
			else
			{
				const string LeadIn =
					" KM_KB_Booking IN (SELECT KB_PK FROM dbo.DtbBookingConsolidation WHERE KB_ParentID in (" +
					"select JN_JS " +
					"from dbo.JobConShipLink " +
					"join dbo.JobConsol on JN_JK = JK_PK ";

				const string SendingPart1 =
					"join dbo.OrgAddress SendingAgent on SendingAgent.OA_PK = JK_OA_SendingForwarderAddress ";

				const string ReceivingPart1 =
					"join dbo.OrgAddress ReceivingAgent on ReceivingAgent.OA_PK = JK_OA_ReceivingForwarderAddress ";
				const string Where =
					"where JK_IsCancelled = 0 ";

				const string SendingPart2 =
					"and SendingAgent.OA_OH = @Sending ";

				const string ReceivingPart2 =
					"and ReceivingAgent.OA_OH = @Receiving";

				const string LeadOut = "))";

				ZSqlParameterCollection parameters = new ZSqlParameterCollection();
				StringBuilder builder = new StringBuilder();

				builder.Append(LeadIn);

				if (!sendingAgent.IsEmpty)
				{
					builder.Append(SendingPart1);
				}

				if (!receivingAgent.IsEmpty)
				{
					builder.Append(ReceivingPart1);
				}

				builder.Append(Where);

				if (!sendingAgent.IsEmpty)
				{
					builder.Append(SendingPart2);
					parameters.Add("@Sending", sendingAgent, JobConsolSchema.JK_OA_SendingForwarderAddress);
				}

				if (!receivingAgent.IsEmpty)
				{
					builder.Append(ReceivingPart2);
					parameters.Add("@Receiving", receivingAgent, JobConsolSchema.JK_OA_ReceivingForwarderAddress);
				}

				builder.Append(LeadOut);

				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DtbBooking));
				query.AddFilterAndZSQLParameterCollection(builder.ToString(), parameters);
				return query;
			}
		}

		ZQuery GetCartageCoordinatorQuery(ZGuid cartageCoordinatorPK)
		{
			//Staff
			var staffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
			staffSubQuery.AddToFilter(GlbStaffSchema.PK, cartageCoordinatorPK);

			var staffAssignmentsQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH);
			staffAssignmentsQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GC, GlbCompany.CurrentCompany.PK);
			staffAssignmentsQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_Role, StaffAssignmentRoles.Codes.CartageCoordinator);
			staffAssignmentsQuery.AddSubQuery(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, staffSubQuery, JoinCondition.And);

			// OrgAddress
			var addressFilter = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			addressFilter.AddSubQuery(OrgAddressSchema.OA_OH, staffAssignmentsQuery, JoinCondition.And);

			// Consignor and Consignee
			var consignorandConsigneeFilter = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			consignorandConsigneeFilter.AddToFilter(JoinCondition.Or, JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.LocalCartageExporter); // Consignor
			consignorandConsigneeFilter.AddToFilter(JoinCondition.Or, JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.LocalCartageImporter); // Consignee
			consignorandConsigneeFilter.AddSubQuery(addressFilter, JoinCondition.And);

			var instruction = new ZDBOnlySubQuery(typeof(DtbBookingInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement);
			instruction.AddSubQuery(DtbBookingInstructionSchema.PK, consignorandConsigneeFilter, JoinCondition.And);

			// BookedBy
			var bookedByFilter = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			bookedByFilter.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.BookingPartyDocumentaryAddress); // BookedBy
			bookedByFilter.AddSubQuery(addressFilter, JoinCondition.And);

			var bookingConsolidation = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking);
			bookingConsolidation.AddSubQuery(DtbBookingConsolidationSchema.PK, bookedByFilter, JoinCondition.And);

			// Billing Party
			var billingPartyFilter = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr);
			billingPartyFilter.AddSubQuery(OrgAddressSchema.OA_OH, staffAssignmentsQuery, JoinCondition.And);

			var headerFilter = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			headerFilter.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			headerFilter.AddSubQuery(billingPartyFilter, JoinCondition.And);

			// Billing Party (when Transport Booking is created from other module)
			var bookingConsolidationBilling = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking);
			bookingConsolidationBilling.AddSubQuery(DtbBookingConsolidationSchema.KB_ParentID, headerFilter, JoinCondition.And);

			// Query
			var result = new ZDBOnlyQuery(typeof(DtbBooking));
			result.AddSubQuery(instruction, JoinCondition.Or);
			result.AddSubQuery(bookingConsolidation, JoinCondition.Or);
			result.AddSubQuery(headerFilter, JoinCondition.Or);
			result.AddSubQuery(bookingConsolidationBilling, JoinCondition.Or);

			return result;
		}

		void AddNumbersAndRefsFilters(ModuleFilterCollection filters)
		{
			var bookingTransportReferenceFilter = filters.AddTextFilter(FilterNameConstants.BookingTransportReference, DtbBookingSchema.KM_TransportReference);
			bookingTransportReferenceFilter.Category = FilterCategories.Organisations;
			bookingTransportReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|TransportReference", "Booking Transport Reference");

			var carrierAccountFilter = filters.AddTextFilter(FilterNameConstants.CarrierAccount, QueryHelper.BookingCarrierAccountQuery, new OrgCarrierAccountCollection(Factory));
			carrierAccountFilter.MultilingualDescription = ResString.GetMultilingualString("22a38bdc-8f8b-417a-7856-a6cf9a3693aa", "Carrier Account");
			carrierAccountFilter.Category = FilterCategories.NumbersAndReferences;

			var parentJobNumberFilter = filters.AddNumberFilterWithoutIsBlankAndIsNotBlank(FilterNameConstants.ParentJobNumber, GetJobNumberQuery);
			parentJobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|ParentJobNumber", "Parent Job #");
			parentJobNumberFilter.MaxLength = ViewTransportBookingParentsSchema.VP_JobNumber.MaxLength;

			var consolNumberFilter = filters.AddNumberFilterWithoutIsBlankAndIsNotBlank(FilterNameConstants.ConsolNumber, GetConsolNumberQuery);
			consolNumberFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|ConsolNumber", "Consol #");
			consolNumberFilter.MaxLength = ViewTransportBookingParentsSchema.VP_JobNumber.MaxLength;

			var shipmentNumberFilter = filters.AddNumberFilterWithoutIsBlankAndIsNotBlank(FilterNameConstants.ShipmentNumber, GetShipmentNumberQuery);
			shipmentNumberFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|ShipmentNumber", "Shipment #");
			shipmentNumberFilter.MaxLength = ViewTransportBookingParentsSchema.VP_JobNumber.MaxLength;

			var masterBillNumberFilter = filters.AddNumberFilter(FilterNameConstants.MasterBillNumber, GetAdditionalReferenceFilterDelegate(TransportCommonAdditionalReferenceTypes.Codes.MasterBill));
			masterBillNumberFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|MasterBillNumber", "Master Bill #");
			masterBillNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;

			var houseBillNumberFilter = filters.AddNumberFilter(FilterNameConstants.HouseBillNumber, GetAdditionalReferenceFilterDelegate(TransportCommonAdditionalReferenceTypes.Codes.HouseBill));
			houseBillNumberFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|HouseBillNumber", "House Bill #");
			houseBillNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;

			var multiBookingIDFilter = filters.AddNumberFilter(FilterNameConstants.MultiBookingID, GetMultiBookingIDQuery);
			multiBookingIDFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|Multi Booking ID", "Multi Booking ID");
			multiBookingIDFilter.MaxLength = DtbBookingConsolidationSchema.KB_JobID.MaxLength;

			var colsolidatedBookingIDFilter = filters.AddNumberFilter(FilterNameConstants.ConsolidatedBookingID, GetConsolidatedBookingIDQuery);
			colsolidatedBookingIDFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|Consolidated Booking ID", "Consolidated Booking ID");
			colsolidatedBookingIDFilter.MaxLength = DtbBookingConsolidationSchema.KB_JobID.MaxLength;

			var helper = new PackageFilterQueryHelper(typeof(DtbBooking), typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking, PkgPackageHeaderSchema.KPH_PackageID);
			var packageIdFilter = filters.AddTextFilter(FilterNameConstants.PackageID, helper.QueryDelegate);
			packageIdFilter.Category = FilterCategories.NumbersAndReferences;
			packageIdFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|PackageIDContainerNo", "Package ID/Container #");
			packageIdFilter.MaxLength = PkgPackageHeaderSchema.KPH_PackageID.MaxLength;

			var assignedPackageIDFilter = filters.AddNumberFilter(FilterNameConstants.PackageID_Assigned, GetAssignedPackageIDQuery);
			assignedPackageIDFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|PackageIDAssigned", "Package ID/Container # (Assigned)");
			assignedPackageIDFilter.MaxLength = PkgPackageHeaderSchema.KPH_PackageID.MaxLength;

			if (ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled)
			{
				var co2eFilter = new CO2eStatusAndCO2eKgRangeNumberFilter(FilterNameConstants.CO2e, typeof(DtbBooking))
				{
					MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|CO2e", "CO2e (kg)"),
					Category = FilterCategories.NumbersAndReferences
				};
				filters.AddCustomFilter(co2eFilter);
			}

			filters.AddCustomFilter(new ReferenceNumberFilter(
				FilterNameConstants.TransportAdditionalReference,
				GetAdditionalReferenceFilter,
				new RefCountryCollection(Factory),
				GetAdditionalReferenceNumberTypes()
				)
			{ MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|ReferenceNumbers", "Additional Reference #") });
		}

		ZQuery GetAdditionalReferenceFilter(SQLComparisonOperator @operator, ZString country, ZString type, ZString value)
		{
			var notIn = isNotIn(@operator);
			@operator = notInComparisonOperator(@operator);

			var consolidationSubQuery = GetConsolidationReferenceNumberSubQuery(@operator, country, type, value);
			var bookingSubQuery = GetBookingReferenceNumberSubQuery(@operator, country, type, value);

			var topSubQuery = new ZDBOnlySubQuery(typeof(DtbBooking), DtbBookingSchema.PK, notIn);
			topSubQuery.AddSubQuery(consolidationSubQuery, JoinCondition.And);
			topSubQuery.AddSubQuery(bookingSubQuery, @operator == SpecialComparisonOperator.IsBlank ? JoinCondition.And : JoinCondition.Or);

			var result = new ZDBOnlyQuery(typeof(DtbBooking));
			result.AddSubQuery(topSubQuery, JoinCondition.And);

			return result;
		}

		ZDBOnlySubQuery GetConsolidationReferenceNumberSubQuery(SQLComparisonOperator @operator, ZString country, ZString type, ZString value)
		{
			var consolidationSubQuery = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking);
			var consolidationFilter = new ReferenceNumberFilterHelper<DtbBookingConsolidation>().GetReferenceNumberFilter(@operator, country, type, value);
			consolidationSubQuery.AddToFilter(consolidationFilter);

			return consolidationSubQuery;
		}

		ZDBOnlySubQuery GetBookingReferenceNumberSubQuery(SQLComparisonOperator @operator, ZString country, ZString type, ZString value)
		{
			var bookingSubQuery = new ZDBOnlySubQuery(typeof(DtbBooking), DtbBookingSchema.PK);
			var bookingFilter = new ReferenceNumberFilterHelper<DtbBooking>().GetReferenceNumberFilter(@operator, country, type, value);
			bookingSubQuery.AddToFilter(bookingFilter);

			return bookingSubQuery;
		}

		ZQuery GetJobNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			var notIn = isNotIn(@operator);
			@operator = notInComparisonOperator(@operator);
			var result = new ZDBOnlyQuery(typeof(DtbBooking));

			var topSubQuery = new ZDBOnlySubQuery(typeof(DtbBooking), DtbBookingSchema.PK, notIn);
			topSubQuery.AddToFilter(ParentsSubGroup.GetSubQuery(new ZQuery().AddToFilter_PossiblyCommaSeparated(ViewTransportBookingParentsSchema.VP_JobNumber, @operator, value)), JoinCondition.And);

			var bookingReferenceNumberSubQuery = GetBookingReferenceNumberSubQuery(@operator, "", TransportCommonAdditionalReferenceTypes.Codes.BookingPartyReference, value);
			var consolidationReferenceNumberSubQuery = new ZDBOnlySubQuery(typeof(DtbBooking), DtbBookingSchema.PK);
			consolidationReferenceNumberSubQuery.AddSubQuery(GetConsolidationReferenceNumberSubQuery(@operator, "", TransportCommonAdditionalReferenceTypes.Codes.BookingPartyReference, value), JoinCondition.Or);

			if (notIn)
			{
				var notInBookingTopSubQuery = new ZDBOnlySubQuery(typeof(DtbBooking), DtbBookingSchema.PK, notIn);
				notInBookingTopSubQuery.AddSubQuery(bookingReferenceNumberSubQuery, JoinCondition.Or);

				var notInConsolidationTopSubQuery = new ZDBOnlySubQuery(typeof(DtbBooking), DtbBookingSchema.PK, notIn);
				notInConsolidationTopSubQuery.AddSubQuery(consolidationReferenceNumberSubQuery, JoinCondition.Or);

				result.AddSubQuery(notInBookingTopSubQuery, JoinCondition.And);
				result.AddSubQuery(notInConsolidationTopSubQuery, JoinCondition.And);
			}
			else
			{
				result.AddSubQuery(bookingReferenceNumberSubQuery, JoinCondition.Or);
				result.AddSubQuery(consolidationReferenceNumberSubQuery, JoinCondition.Or);
			}

			result.AddSubQuery(topSubQuery, notIn ? JoinCondition.And : JoinCondition.Or);

			return result;
		}

		ZQuery GetConsolNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			return GetConsolShipmentNumberQueryCore(@operator, value, true);
		}

		ZQuery GetShipmentNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			return GetConsolShipmentNumberQueryCore(@operator, value, false);
		}

		ZQuery GetConsolShipmentNumberQueryCore(SQLComparisonOperator @operator, ZString value, bool isFilterOnConsolNumber)
		{
			var notIn = isNotIn(@operator);
			@operator = notInComparisonOperator(@operator);

			var topSubQuery = new ZDBOnlySubQuery(typeof(DtbBooking), DtbBookingSchema.PK, notIn);
			topSubQuery.AddToFilter(GetComplexParentsFilterWithParamSubQuery(new ZQuery().AddToFilter_PossiblyCommaSeparated(ViewTransportBookingParentsSchema.VP_JobNumber, @operator, value), isFilterOnConsolNumber), JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(DtbBooking));
			result.AddSubQuery(topSubQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetAssignedPackageIDQuery(SQLComparisonOperator @operator, ZString value)
		{
			return InstructionPackageFieldQuery(@operator, PkgPackageHeaderSchema.KPH_PackageID, value);
		}

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			var bookingStatusFilter = filters.AddTextFilter(FilterNameConstants.BookingStatus, BookingStatusQuery, BindToLists.BookingStatuses);
			bookingStatusFilter.Category = FilterCategories.StatusAndFlags;
			bookingStatusFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|BookingStatus", "Booking Status");
			bookingStatusFilter.MaxLength = DtbBookingSchema.KM_Status.MaxLength;

			var instructionStatusFilter = filters.AddTextFilter(FilterNameConstants.InstructionStatus, InstructionStatusQuery, BindToLists.BookingInstructionStatuses);
			instructionStatusFilter.Category = FilterCategories.StatusAndFlags;
			instructionStatusFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|InstructionStatusFilter", "Instruction Status");
			instructionStatusFilter.MaxLength = DtbBookingInstructionSchema.KN_Status.MaxLength;

			var bookingConsolidatedFilter = filters.AddTextFilter(FilterNameConstants.BookingConsolidated, BookingConsolidatedQuery, BindToLists.BookingConsolidatedStatuses);
			bookingConsolidatedFilter.Category = FilterCategories.StatusAndFlags;
			bookingConsolidatedFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|BookingConsolidated", "Consolidation Status");

			var showStandaloneBookingsFilter = filters.AddTextFilter(FilterNameConstants.BookingShowStandalone, BookingShowStandaloneQuery, BindToLists.BookingShowStandaloneValues.List);
			showStandaloneBookingsFilter.Category = FilterCategories.StatusAndFlags;
			showStandaloneBookingsFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|BookingShowStandalone", "Show Standalone Bookings");

			var isOverriddenFilter = filters.AddTextFilter(FilterNameConstants.IsOverridden, IsOverriddenQuery, BindToLists.IsOverriddenStatuses);
			isOverriddenFilter.Category = FilterCategories.StatusAndFlags;
			isOverriddenFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|IsOverridden", "Is Overridden");
			isOverriddenFilter.DefaultProperty = IsOverriddenStatuses.Codes.OverridenOrStandalone;
			isOverriddenFilter.Visibility = FilterVisibility.AlwaysVisible;

			var quoteChargesFilter = filters.AddTextFilter(FilterNameConstants.BookingShowQuotes, BookingQuoteChargesQuery, BindToLists.BookingQuoteStatuses);
			quoteChargesFilter.Category = FilterCategories.StatusAndFlags;
			quoteChargesFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|BookingQuotes", "Show Quotes");

			AddIsHazardeousFilter(filters);
			AddRequiresRefrigerationFilter(filters);
			if (TransportRegistry.Instance.MasterBookingsEnabled.Value)
			{
				AddIsMasterBookingFilter(filters);
				AddIsSubBookingFilter(filters);
			}
		}

		void AddIsHazardeousFilter(ModuleFilterCollection filters)
		{
			var isHazardeousFilter = filters.AddTextFilter(FilterNameConstants.BookingIsHazardous, IsHazardeousQuery, BindToLists.IsHazardousStatuses);
			isHazardeousFilter.Category = FilterCategories.StatusAndFlags;
			isHazardeousFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|IsHazardous", "Hazardous");
		}

		ZQuery IsHazardeousQuery(ZString isHazardeousCode)
		{
			var bookingQuery = new ZQuery();
			if (isHazardeousCode == IsHazardousStatuses.Codes.Hazardous)
			{
				bookingQuery.AddToFilter(DtbBookingSchema.KM_IsHazardous, true);
			}
			else if (isHazardeousCode == IsHazardousStatuses.Codes.NotHazardous)
			{
				bookingQuery.AddToFilter(DtbBookingSchema.KM_IsHazardous, false);
			}
			return bookingQuery;
		}

		void AddRequiresRefrigerationFilter(ModuleFilterCollection filters)
		{
			var requiresRefrigerationFilter = filters.AddTextFilter(FilterNameConstants.BookingRequiresRefrigeration, RequiresRefrigerationQuery, BindToLists.RequiresRefrigerationStatuses);
			requiresRefrigerationFilter.Category = FilterCategories.StatusAndFlags;
			requiresRefrigerationFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|RequiresRefrigeration", "Refrigeration");
		}

		ZQuery RequiresRefrigerationQuery(ZString requiresRefrigerationCode)
		{
			var bookingQuery = new ZQuery();
			if (requiresRefrigerationCode == RequiresRefrigerationStatuses.Codes.RequiresRefrigeration)
			{
				bookingQuery.AddToFilter(DtbBookingSchema.KM_RequiresRefrigeration, true);
			}
			else if (requiresRefrigerationCode == RequiresRefrigerationStatuses.Codes.NotRequiresRefrigeration)
			{
				bookingQuery.AddToFilter(DtbBookingSchema.KM_RequiresRefrigeration, false);
			}
			return bookingQuery;
		}

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			var instructionAddressCityFilter = filters.AddTextFilter(FilterNameConstants.InstructionAddressCity, InstructionAddressCityQuery);
			instructionAddressCityFilter.Category = FilterCategories.Locations;
			instructionAddressCityFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|InstructionAddressCity", "Instruction Address City");
			instructionAddressCityFilter.MaxLength = Math.Min(JobDocAddressSchema.E2_City.MaxLength, OrgAddressSchema.OA_City.MaxLength);

			var instructionAddressStateFilter = filters.AddTextFilter(FilterNameConstants.InstructionAddressState, InstructionAddressStateQuery);
			instructionAddressStateFilter.Category = FilterCategories.Locations;
			instructionAddressStateFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|InstructionAddressState", "Instruction Address State");
			instructionAddressStateFilter.MaxLength = Math.Min(JobDocAddressSchema.E2_State.MaxLength, OrgAddressSchema.OA_State.MaxLength);

			var instructionAddressPostCodeFilter = filters.AddNumberFilter(FilterNameConstants.InstructionAddressPostCode, InstructionAddressPostcodeQuery);
			instructionAddressPostCodeFilter.Category = FilterCategories.Locations;
			instructionAddressPostCodeFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|InstructionAddressPostcode", "Instruction Address Postcode");
			instructionAddressPostCodeFilter.MaxLength = Math.Min(JobDocAddressSchema.E2_Postcode.MaxLength, OrgAddressSchema.OA_PostCode.MaxLength);

			LocationCollection locations = new LocationCollection(Factory);

			ModuleLocationFilter originDestinationFilter = filters.AddLocationFilter(FilterNameConstants.OriginDestination, ViewTransportBookingParentsSchema.VP_RL_NKOrigin, locations, ViewTransportBookingParentsSchema.VP_RL_NKDestination, locations);
			originDestinationFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|OriginDestination", "Origin / Destination");
			originDestinationFilter.SetItemDescriptions(Res.GetData("1f218c9e-d8fc-4387-ba7e-4fb77dab819c", "Origin"), Res.GetData("03d5bae2-c3e9-4a6f-9025-b3f9a6f45b13", "Destination"));
			originDestinationFilter.SubGroup = ParentsSubGroup;

			ModuleLocationFilter loadDischargeFilter = filters.AddLocationFilter(FilterNameConstants.LoadDischarge, ViewTransportBookingParentLegsSchema.VL_RL_NKLoad, locations, ViewTransportBookingParentLegsSchema.VL_RL_NKDischarge, locations);
			loadDischargeFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|LoadDischarge", "Load / Discharge");
			loadDischargeFilter.SetItemDescriptions(Res.GetData("afe55d51-cbaf-41ff-bd5e-48fb2323ed9f", "Load"), Res.GetData("d0d2d396-4aa7-4387-b1d4-e36030bd609c", "Discharge"));
			loadDischargeFilter.SubGroup = ParentLegsSubGroup;
		}

		void AddModesAndTypesFilters(ModuleFilterCollection filters)
		{
			var parentTypeFilter = filters.AddTextFilter(FilterNameConstants.ParentJobType, ParentJobTypeQuery, BindToLists.ParentJobTypes);
			parentTypeFilter.Category = FilterCategories.ModesAndTypes;
			parentTypeFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|JobType", "Parent Job Type");
			parentTypeFilter.SubGroup = ParentsSubGroup;

			var instructionDropModeFilter = filters.AddTextFilter(FilterNameConstants.InstructionDropMode, InstructionDropModeQuery, BindToLists.DropModes);
			instructionDropModeFilter.Category = FilterCategories.ModesAndTypes;
			instructionDropModeFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|InstructionDropMode", "Instruction Drop Mode");
			instructionDropModeFilter.MaxLength = DtbBookingInstructionSchema.KN_DropMode.MaxLength;

			var bookingTransportModeFilter = filters.AddTextFilter(FilterNameConstants.BookingTransportMode, BookingTransportModeQuery, BindToLists.BookingTransportModes.List);
			bookingTransportModeFilter.Category = FilterCategories.ModesAndTypes;
			bookingTransportModeFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|BookingTransportMode", "Booking Transport Mode");
			bookingTransportModeFilter.MaxLength = DtbBookingSchema.KM_TransportMode.MaxLength;

			var helper = new PackageFilterQueryHelper(typeof(DtbBooking), typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking, PkgPackageSchema.KP_F3_NKPackType);
			var packageTypeFilter = filters.AddTextFilter(FilterNameConstants.PackageType, helper.QueryDelegateWithoutOperator, BindToLists.PackageTypes);
			packageTypeFilter.Category = FilterCategories.ModesAndTypes;
			packageTypeFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|PackageType", "Package Type");

			var containerTypeFilter = filters.AddTextFilter(FilterNameConstants.ContainerType, ContainerTypeQuery, BindToLists.ContainerTypes);
			containerTypeFilter.Category = FilterCategories.ModesAndTypes;
			containerTypeFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|ContainerType", "Container Type");
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();

			var helper1 = new TransportWorkflowFilterStripsHelper(typeof(DtbBooking), WorkflowDescriptors.DtbBookingWorkflowDescriptorCode, Factory);
			helper1.SetShouldAddWorkflowCustomFieldsFilters(true);

			helpers.Add(helper1);

			var bookingParentSubQuery = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking);
			var helper2 = new TransportWorkflowFilterStripsHelper(typeof(DtbBooking), JobInvoicingConsumerTypes.TransportBooking.Code, Factory, DtbBookingConsolidationSchema.KB_ParentID, bookingParentSubQuery)
			{
				ShouldAddMilestoneFilters = false,
				ShouldAddRelatedMilestoneFilters = true,
				ShouldAddMiscFilters = false
			};
			helper2.SetShouldAddWorkflowCustomFieldsFilters(false);

			helpers.Add(helper2);

			return helpers;
		}

		IAccountingFilterStrip AccountingFilterStrip
		{
			get
			{
				if (accountingFilterStrip == null)
				{
					accountingFilterStrip = ObjectFactory.New<IAccountingFilterStrip>(this);
					accountingFilterStrip.Initialize(addRevenueFilters: false, addWIPAccrualHasFilters: false);
				}

				return accountingFilterStrip;
			}
		}

		IAccountingFilterStrip accountingFilterStrip;

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(DtbBooking));
			result.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return result;
		}

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride
		{
			get { return ResString.GetMultilingualString("062eb630-d14b-474e-b596-0db2c6ed50ea", "Standalone Booking Amounts"); }
		}

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride
		{
			get { return ResString.GetMultilingualString("08faf8d2-6104-4463-ac45-d96f9d9b85cb", "Standalone Billing"); }
		}

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories
		{
			get { return ResString.GetMultilingualString("ef7dc5f3-2bdf-4531-92d7-8a4a6f6a4790", "(Standalone Booking)"); }
		}

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration
		{
			get
			{
				var config = new Dictionary<string, object>()
				{
					{ AccountingFilterStripConfigurationKeys.BusinessObjectType, typeof(DtbBooking) },
					{ AccountingFilterStripConfigurationKeys.InvoicedChargesFilterNameOverride, ResString.GetMultilingualString("DtbBookingFilterBusinessObject|InvoicedChargesBilling", "Invoiced / Charges / Billing") },
					{ AccountingFilterStripConfigurationKeys.InvoicedChargesFilterOptionsSelected, InvoicedChargesFilterOptions.Default | InvoicedChargesFilterOptions.LocalBillingNotPaid },
					{ AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride, ResString.GetMultilingualString("DtbBookingFilterBusinessObject|InvoiceStatus", "Invoice Status") },
				};
				config.Add(
					AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterMakeCustomFilter,
					(Func<ZDBOnlyQuery, ZDBOnlyQuery>)(baseJobQuery =>
					{
						var baseJobSubQuery = (ZDBOnlySubQuery)baseJobQuery;

						var bookingQuery = new ZDBOnlyQuery(typeof(DtbBooking));
						bookingQuery.AddSubQuery(DtbBookingSchema.PK, baseJobSubQuery, JoinCondition.And);

						var consolidationSubQuery = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingConsolidationSchema.PK);
						consolidationSubQuery.AddSubQuery(DtbBookingConsolidationSchema.KB_ParentID, baseJobSubQuery, JoinCondition.And);

						bookingQuery.AddSubQuery(DtbBookingSchema.KM_KB_Booking, consolidationSubQuery, JoinCondition.Or);

						return bookingQuery;
					}));
				return config;
			}
		}

		// queries

		ZQuery BookingConsolidationDirectionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return BookingConsolidationStringAttributeQuery(DtbBookingConsolidationSchema.KB_JobDirection, DtbBookingSchema.KM_KB_Booking, comparisonOperator, value);
		}

		ZQuery BookingDirectionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return QueryHelper.BookingStringAttributeQuery(DtbBookingSchema.KM_Direction, comparisonOperator, value);
		}

		ZQuery BookingTemplateQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return QueryHelper.BookingStringAttributeQuery(DtbBookingSchema.KM_KT_NKBookingTemplate, comparisonOperator, value);
		}

		ZQuery ParentJobTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			const string StandaloneBookingsIncludedSql = "KB_ParentID IS NULL OR KB_ParentTableCode = 'LTB'";
			const string StandaloneBookingsExcludedSql = "KB_ParentID IS NOT NULL AND KB_ParentTableCode <> 'LTB'";

			var shouldIncludeBookingsFromShipments = false;
			var shouldExcludeBookingsFromShipments = false;
			var shouldIncludeStandaloneBookings = false;
			var shouldExcludeStandaloneBookings = false;

			SetSqlVariablesBasedOnOperatorAndValue();

			var shipmentParentTypeMatchSql = GenerateShipmentParentTypeMatchSql();

			var conjunction1 = string.Empty;

			if (!string.IsNullOrEmpty(shipmentParentTypeMatchSql))
			{
				if (shouldIncludeBookingsFromShipments)
				{
					conjunction1 = " OR ";
				}
				else if (shouldExcludeBookingsFromShipments)
				{
					conjunction1 = " AND ";
				}
			}

			var generalParentTypeMatchSql = GenerateGeneralJobTypeMatchSql();

			var conjunction2 = string.Empty;

			if (!string.IsNullOrEmpty(generalParentTypeMatchSql))
			{
				if (shouldIncludeStandaloneBookings)
				{
					conjunction2 = " OR ";
				}
				else if (shouldExcludeStandaloneBookings)
				{
					conjunction2 = " AND ";
				}
			}

			var standaloneParentTypeMatchSql = GenerateStandaloneParentTypeMatchSql();

			var customFilterSQL = String.Format(CultureInfo.InvariantCulture, shipmentParentTypeMatchSql + conjunction1 + generalParentTypeMatchSql + conjunction2 + standaloneParentTypeMatchSql);

			var query = new ZDBOnlyQuery(typeof(ViewTransportBookingParents));
			query.AddFilterAndZSQLParameterCollection(customFilterSQL, null);

			return query;

			string GenerateGeneralJobTypeMatchSql()
			{
				var result = string.Empty;

				var jobTypeMatchQuery = new ZQuery(ViewTransportBookingParentsSchema.VP_JobType, comparisonOperator, value);
				var jobTypeMatchSql = "SELECT VP_PK FROM dbo.ViewTransportBookingParents WHERE " + jobTypeMatchQuery.LiteralTextSqlFormatted;

				result = "KB_ParentID In (" + jobTypeMatchSql + ")";

				return result;
			}

			string GenerateShipmentParentTypeMatchSql()
			{
				var result = string.Empty;

				const string ForwardingShipmentMatchSql = "SELECT VP_PK FROM dbo.ViewTransportBookingParents WHERE (VP_JobType = 'SHP' OR VP_JobType = 'VB'))";

				if (shouldIncludeBookingsFromShipments)
				{
					result = "KB_ParentID In (" + ForwardingShipmentMatchSql;
				}
				else if (shouldExcludeBookingsFromShipments)
				{
					result = "KB_ParentID Not In (" + ForwardingShipmentMatchSql;
				}

				return result;
			}

			string GenerateStandaloneParentTypeMatchSql()
			{
				var result = string.Empty;

				if (shouldIncludeStandaloneBookings)
				{
					result = StandaloneBookingsIncludedSql;
				}
				else if (shouldExcludeStandaloneBookings)
				{
					result = StandaloneBookingsExcludedSql;
				}

				return result;
			}

			void SetSqlVariablesBasedOnOperatorAndValue()
			{
				switch (comparisonOperator)
				{
					case IsNotBlankComparisonOperator _:
						shouldIncludeBookingsFromShipments = true;
						shouldIncludeStandaloneBookings = true;
						break;
					case IsBlankComparisonOperator _:
						shouldExcludeStandaloneBookings = true;
						shouldExcludeBookingsFromShipments = true;
						break;
					case EqualComparisonOperator _:
						if (BindToLists.ForwardingShipmentCode.Equals(value))
						{
							shouldIncludeBookingsFromShipments = true;
						}

						if (BindToLists.StandAloneBookingCode.Equals(value))
						{
							shouldIncludeStandaloneBookings = true;
						}
						break;
					case NotEqualComparisonOperator _:
						if (BindToLists.ForwardingShipmentCode.Equals(value))
						{
							shouldExcludeBookingsFromShipments = true;
						}
						else
						{
							shouldIncludeBookingsFromShipments = true;
						}

						if (BindToLists.StandAloneBookingCode.Equals(value))
						{
							shouldExcludeStandaloneBookings = true;
						}
						else
						{
							shouldIncludeStandaloneBookings = true;
						}
						break;
					case StartsWithComparisonOperator _:
						if (BindToLists.ForwardingShipmentCode.StartsWith(value))
						{
							shouldIncludeBookingsFromShipments = true;
						}

						if (BindToLists.StandAloneBookingCode.StartsWith(value))
						{
							shouldIncludeStandaloneBookings = true;
						}
						break;
					case DoesNotStartWithComparisonOperator _:
						if (BindToLists.ForwardingShipmentCode.StartsWith(value))
						{
							shouldExcludeBookingsFromShipments = true;
						}
						else
						{
							shouldIncludeBookingsFromShipments = true;
						}

						if (BindToLists.StandAloneBookingCode.StartsWith(value))
						{
							shouldExcludeStandaloneBookings = true;
						}
						else
						{
							shouldIncludeStandaloneBookings = true;
						}
						break;
					case ContainsComparisonOperator _:
						if (BindToLists.ForwardingShipmentCode.Contains(value))
						{
							shouldIncludeBookingsFromShipments = true;
						}

						if (BindToLists.StandAloneBookingCode.Contains(value))
						{
							shouldIncludeStandaloneBookings = true;
						}
						break;
					case NotContainsComparisonOperator _:
						if (BindToLists.ForwardingShipmentCode.Contains(value))
						{
							shouldExcludeBookingsFromShipments = true;
						}
						else
						{
							shouldIncludeBookingsFromShipments = true;
						}

						if (BindToLists.StandAloneBookingCode.Contains(value))
						{
							shouldExcludeStandaloneBookings = true;
						}
						else
						{
							shouldIncludeStandaloneBookings = true;
						}
						break;
				}
			}
		}

		ZQuery InstructionTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return InstructionStringAttributeQuery(DtbBookingInstructionSchema.KN_InstructionType, comparisonOperator, value);
		}

		ZQuery InstructionDropModeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return InstructionStringAttributeQuery(DtbBookingInstructionSchema.KN_DropMode, comparisonOperator, value);
		}

		ZQuery InstructionNotesQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return InstructionStringAttributeQuery(DtbBookingInstructionSchema.KN_ServiceInstruction, comparisonOperator, value);
		}

		ZQuery ConfirmationDescriptionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return ConfirmationStringAttributeQuery(DtbBookingConfirmationSchema.KK_ConfirmationType, comparisonOperator, value);
		}

		ZQuery ConfirmationReferenceNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return ConfirmationStringAttributeQuery(DtbBookingConfirmationSchema.KK_ReferenceNum, comparisonOperator, value);
		}

		ZQuery ConfirmationSlotReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return ConfirmationStringAttributeQuery(DtbBookingConfirmationSchema.KK_SlotReference, comparisonOperator, value);
		}

		ZQuery ConfirmationReceivedByQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return ConfirmationStringAttributeQuery(DtbBookingConfirmationSchema.KK_ReceivedBy, comparisonOperator, value);
		}

		ZQuery GetMultiBookingIDQuery(SQLComparisonOperator @operator, ZString value)
		{
			return BookingConsolidationStringAttributeQuery(DtbBookingConsolidationSchema.KB_JobID, DtbBookingSchema.KM_KB_Booking, @operator, value);
		}

		ZQuery GetConsolidatedBookingIDQuery(SQLComparisonOperator @operator, ZString value)
		{
			return BookingConsolidationStringAttributeQuery(DtbBookingConsolidationSchema.KB_JobID, DtbBookingSchema.KM_KB_BookingConsolidationMultiJob, @operator, value);
		}

		ZQuery DeliveryEstimatedQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return ConfirmationTypeAndDateTimeAttributeQuery(ConfirmationTypes.Codes.Delivery, DtbBookingConfirmationSchema.KK_Estimated, comparisonOperator, date1, date2);
		}

		ZQuery DeliveryActualQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return ConfirmationTypeAndDateTimeAttributeQuery(ConfirmationTypes.Codes.Delivery, DtbBookingConfirmationSchema.KK_Actual, comparisonOperator, date1, date2);
		}

		ZQuery DeliveryRequiredFromQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return ConfirmationTypeAndDateTimeAttributeQuery(ConfirmationTypes.Codes.Delivery, DtbBookingConfirmationSchema.KK_RequiredFrom, comparisonOperator, date1, date2);
		}

		ZQuery DeliveryRequiredToQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return ConfirmationTypeAndDateTimeAttributeQuery(ConfirmationTypes.Codes.Delivery, DtbBookingConfirmationSchema.KK_RequiredTo, comparisonOperator, date1, date2);
		}

		ZQuery DeliverySlotDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return ConfirmationTypeAndDateTimeAttributeQuery(ConfirmationTypes.Codes.Delivery, DtbBookingConfirmationSchema.KK_SlotDateTime, comparisonOperator, date1, date2);
		}

		ZQuery PickupEstimatedQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return ConfirmationTypeAndDateTimeAttributeQuery(ConfirmationTypes.Codes.PickUp, DtbBookingConfirmationSchema.KK_Estimated, comparisonOperator, date1, date2);
		}

		ZQuery PickupActualQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return ConfirmationTypeAndDateTimeAttributeQuery(ConfirmationTypes.Codes.PickUp, DtbBookingConfirmationSchema.KK_Actual, comparisonOperator, date1, date2);
		}

		ZQuery PickupRequiredFromQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return ConfirmationTypeAndDateTimeAttributeQuery(ConfirmationTypes.Codes.PickUp, DtbBookingConfirmationSchema.KK_RequiredFrom, comparisonOperator, date1, date2);
		}

		ZQuery PickupRequiredToQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return ConfirmationTypeAndDateTimeAttributeQuery(ConfirmationTypes.Codes.PickUp, DtbBookingConfirmationSchema.KK_RequiredTo, comparisonOperator, date1, date2);
		}

		ZQuery PickupSlotDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return ConfirmationTypeAndDateTimeAttributeQuery(ConfirmationTypes.Codes.PickUp, DtbBookingConfirmationSchema.KK_SlotDateTime, comparisonOperator, date1, date2);
		}

		ZQuery InstructionAddressTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var docAddressType = CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(value);
			var searchValue = DocAddressTypes.GetCode(Factory, docAddressType);

			return InstructionJobDocAddressFieldQuery(comparisonOperator, JobDocAddressSchema.E2_AddressType, searchValue);
		}

		ZQuery InstructionCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var instructionOrgHeaderQuery = InstructionOrgHeaderFieldQuery(comparisonOperator, OrgHeaderSchema.OH_FullName, value);
			var instructionJobDocAddressQuery = InstructionJobDocAddressFieldQuery(comparisonOperator, JobDocAddressSchema.E2_CompanyName, value);
			return AddTwoQueries(instructionJobDocAddressQuery, instructionOrgHeaderQuery, comparisonOperator);
		}

		ZQuery InstructionCompanyCodeQuery(ZGuid value)
		{
			return InstructionOrgAddressFieldQuery(SQLComparisonOperator.Equal, OrgAddressSchema.OA_OH, value);
		}

		ZQuery InstructionCompanyRelatedPortQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			bool notIn = isNotIn(comparisonOperator);

			var orgHeaderPortQuery = InstructionOrgHeaderFieldQuery(comparisonOperator, OrgHeaderSchema.OH_RL_NKClosestPort, value);
			var orgAddressPortQuery_isBlank = InstructionOrgAddressFieldQuery(comparisonOperator, OrgAddressSchema.OA_RL_NKRelatedPortCode, ZString.Empty);
			orgHeaderPortQuery.AddToFilter(orgAddressPortQuery_isBlank, notIn ? JoinCondition.Or : JoinCondition.And);
			var orgAddressPortQuery = InstructionOrgAddressFieldQuery(comparisonOperator, OrgAddressSchema.OA_RL_NKRelatedPortCode, value);
			return AddTwoQueries(orgHeaderPortQuery, orgAddressPortQuery, comparisonOperator);
		}

		ZQuery BookingStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return QueryHelper.BookingStringAttributeQuery(DtbBookingSchema.KM_Status, comparisonOperator, value);
		}

		ZQuery InstructionStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return InstructionStringAttributeQuery(DtbBookingInstructionSchema.KN_Status, comparisonOperator, value);
		}

		ZQuery BookingConsolidatedQuery(ZString consolidatedStatusCode)
		{
			var query = new ZQuery();

			if (consolidatedStatusCode == BookingConsolidatedStatuses.Codes.Unconsolidated)
			{
				query.AddToFilter(DtbBookingSchema.KM_KB_BookingConsolidationMultiJob, SQLComparisonOperator.Equal, null);
			}
			else if (consolidatedStatusCode == BookingConsolidatedStatuses.Codes.Consolidated)
			{
				query.AddToFilter(DtbBookingSchema.KM_KB_BookingConsolidationMultiJob, SQLComparisonOperator.NotEqual, null);
			}
			return query;
		}

		ZQuery BookingShowStandaloneQuery(ZString value)
		{
			var query = new ZQuery();

			if (value != BookingShowStandaloneValues.Codes.All)
			{
				var booking = new ZDBOnlyQuery(typeof(DtbBooking));
				var bookingConsolidation = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking);

				if (value == BookingShowStandaloneValues.Codes.ShowStandaloneBookingsOnly)
				{
					bookingConsolidation.AddToFilter(DtbBookingConsolidationSchema.KB_ParentID, SQLComparisonOperator.Equal, null);
				}
				else if (value == BookingShowStandaloneValues.Codes.ExcludeStandaloneBookings)
				{
					bookingConsolidation.AddToFilter(DtbBookingConsolidationSchema.KB_ParentID, SQLComparisonOperator.NotEqual, null);
				}

				booking.AddSubQuery(bookingConsolidation, JoinCondition.And);
				query.AddToFilter(booking);
			}

			return query;
		}

		ZQuery BookingQuoteChargesQuery(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(DtbBooking));

			if (value == BookingQuoteStatuses.Codes.QuotesOnly)
			{
				var jobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
				var jobChargeSubQuery = new ZDBOnlySubQuery(typeof(JobCharge), JobChargeSchema.JR_JH);
				jobHeaderQuery.AddSubQuery(jobChargeSubQuery, JoinCondition.And);
				query.AddSubQuery(jobHeaderQuery, JoinCondition.And);
			}
			else if (value == BookingQuoteStatuses.Codes.BookingsOnly)
			{
				var jobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
				var jobChargeSubQuery = new ZDBOnlySubQuery(typeof(JobCharge), JobChargeSchema.JR_JH, true);
				jobHeaderQuery.AddSubQuery(jobChargeSubQuery, JoinCondition.And);

				var noJobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID, true);

				query.AddSubQuery(jobHeaderQuery, JoinCondition.Or);
				query.AddSubQuery(noJobHeaderQuery, JoinCondition.Or);
			}

			return query;
		}

		ZQuery IsOverriddenQuery(ZString isOverriddenCode)
		{
			var booking = new ZDBOnlyQuery(typeof(DtbBooking));

			var bookingConsolidation = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking);

			if (isOverriddenCode == IsOverriddenStatuses.Codes.OverridenOrStandalone)
			{
				bookingConsolidation.AddToFilter(DtbBookingConsolidationSchema.KB_IsOverridden, SQLComparisonOperator.Equal, true);
				bookingConsolidation.AddToFilter(JoinCondition.Or, DtbBookingConsolidationSchema.KB_ParentID, SQLComparisonOperator.Equal, null);
				booking.AddSubQuery(bookingConsolidation, JoinCondition.And);
			}
			else if (isOverriddenCode == IsOverriddenStatuses.Codes.NotOverridden)
			{
				bookingConsolidation.AddToFilter(DtbBookingConsolidationSchema.KB_IsOverridden, SQLComparisonOperator.Equal, false);
				bookingConsolidation.AddToFilter(JoinCondition.And, DtbBookingConsolidationSchema.KB_ParentID, SQLComparisonOperator.NotEqual, null);
				booking.AddSubQuery(bookingConsolidation, JoinCondition.And);
			}
			return booking;
		}

		ZQuery InstructionAddressCityQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var jobDocAddressQuery = InstructionJobDocAddressFieldQuery(comparisonOperator, JobDocAddressSchema.E2_City, value);
			var orgAddressQuery = InstructionOrgAddressFieldQuery(comparisonOperator, OrgAddressSchema.OA_City, value);
			return AddTwoQueries(jobDocAddressQuery, orgAddressQuery, comparisonOperator);
		}

		ZQuery InstructionAddressStateQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var jobDocAddressQuery = InstructionJobDocAddressFieldQuery(comparisonOperator, JobDocAddressSchema.E2_State, value);
			var orgAddressQuery = InstructionOrgAddressFieldQuery(comparisonOperator, OrgAddressSchema.OA_State, value);
			return AddTwoQueries(jobDocAddressQuery, orgAddressQuery, comparisonOperator);
		}

		ZQuery InstructionAddressPostcodeQuery(SQLComparisonOperator @operator, ZString value)
		{
			var jobDocAddressQuery = InstructionJobDocAddressFieldQuery(@operator, JobDocAddressSchema.E2_Postcode, value);
			var orgAddressQuery = InstructionOrgAddressFieldQuery(@operator, OrgAddressSchema.OA_PostCode, value);
			return AddTwoQueries(jobDocAddressQuery, orgAddressQuery, @operator);
		}

		ZQuery ContainerTypeQuery(ZString containerType)
		{
			var refContainer = new ZDBOnlySubQuery(typeof(RefContainer), PkgPackageContainerSchema.K0_RC_ContainerType);
			refContainer.AddToFilter(RefContainerSchema.RC_ContainerType, containerType);

			var packageContainer = new ZDBOnlySubQuery(typeof(PkgPackageContainer), PkgPackageContainerSchema.K0_KP_Package);
			var package = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob);
			var packageJob = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageJobSchema.KJ_ParentID);
			var bookingConsolidation = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking);
			var booking = new ZDBOnlyQuery(typeof(DtbBooking));

			packageContainer.AddSubQuery(refContainer, JoinCondition.And);
			package.AddSubQuery(packageContainer, JoinCondition.And);
			packageJob.AddSubQuery(package, JoinCondition.And);
			bookingConsolidation.AddSubQuery(packageJob, JoinCondition.And);
			booking.AddSubQuery(bookingConsolidation, JoinCondition.And);

			return booking;
		}

		void AddIsMasterBookingFilter(ModuleFilterCollection filters)
		{
			var isMasterBookingFilter = filters.AddTextFilter(FilterNameConstants.BookingIsMaster, IsMasterBookingQuery, BindToLists.IsMasterBookingStatuses);
			isMasterBookingFilter.Category = FilterCategories.StatusAndFlags;
			isMasterBookingFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|IsMasterBooking", "Is Master");
		}

		ZQuery IsMasterBookingQuery(ZString isMasterBookingCode)
		{
			var bookingQuery = new ZQuery();
			if (isMasterBookingCode == IsMasterBookingStatuses.Codes.IsMasterBooking)
			{
				bookingQuery.AddToFilter(DtbBookingSchema.KM_IsMaster, true);
			}
			else if (isMasterBookingCode == IsMasterBookingStatuses.Codes.NotMasterBooking)
			{
				bookingQuery.AddToFilter(DtbBookingSchema.KM_IsMaster, false);
			}
			return bookingQuery;
		}

		void AddIsSubBookingFilter(ModuleFilterCollection filters)
		{
			var isSubBookingFilter = filters.AddTextFilter(FilterNameConstants.BookingIsSub, IsSubBookingQuery, BindToLists.IsSubBookingStatuses);
			isSubBookingFilter.Category = FilterCategories.StatusAndFlags;
			isSubBookingFilter.MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingFilter|IsSubBooking", "Is Sub");
		}

		ZQuery IsSubBookingQuery(ZString isSubBookingCode)
		{
			var bookingQuery = new ZQuery();
			if (isSubBookingCode == IsSubBookingStatuses.Codes.IsSubBooking)
			{
				bookingQuery.AddToFilter(DtbBookingSchema.KM_KM_MasterBooking, SQLComparisonOperator.NotEqual, DBNull.Value);
			}
			else if (isSubBookingCode == IsSubBookingStatuses.Codes.NotSubBooking)
			{
				bookingQuery.AddToFilter(DtbBookingSchema.KM_KM_MasterBooking, DBNull.Value);
			}
			return bookingQuery;
		}

		ZQuery BookingTransportModeQuery(ZString transportModeCode)
		{
			var bookingTransportModeQuery = new ZQuery();

			bookingTransportModeQuery.AddToFilter(DtbBookingSchema.KM_TransportMode, transportModeCode);

			return bookingTransportModeQuery;
		}

		// helpers -- PLEASE REUSE when adding new queries.

		GetTextQueryWithOperator GetAdditionalReferenceFilterDelegate(ZString additionalReferenceType) =>
			(@operator, value) => GetAdditionalReferenceFilter(@operator, "", additionalReferenceType, value);

		ZQuery BookingConsolidationStringAttributeQuery(SchemaColumn schemaColumn, SchemaColumn key, SQLComparisonOperator @operator, ZString value)
		{
			ZDBOnlyQuery booking = null;

			var notIn = isNotIn(@operator);
			@operator = notInComparisonOperator(@operator);

			var bookingConsolidation = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), key, notIn);
			bookingConsolidation.AddToFilter_PossiblyCommaSeparated(schemaColumn, @operator, value);

			booking = new ZDBOnlyQuery(typeof(DtbBooking));
			booking.AddSubQuery(bookingConsolidation, JoinCondition.And);

			if (@operator == SQLComparisonOperator.IsBlank && key.IsNullable)
			{
				booking.AddToFilter(JoinCondition.Or, key, null);
			}

			return booking;
		}

		ZQuery InstructionStringAttributeQuery(SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery booking = null;

			var notIn = isNotIn(comparisonOperator);
			comparisonOperator = notInComparisonOperator(comparisonOperator);

			var instruction = new ZDBOnlySubQuery(typeof(DtbBookingInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement, notIn);
			instruction.AddToFilter(schemaColumn, comparisonOperator, value);

			booking = new ZDBOnlyQuery(typeof(DtbBooking));
			booking.AddSubQuery(instruction, JoinCondition.And);

			return booking;
		}

		ZQuery InstructionJobDocAddressFieldQuery(SQLComparisonOperator comparisonOperator, SchemaColumn schemaColumn, ZString value)
		{
			bool notIn = isNotIn(comparisonOperator);
			comparisonOperator = notInComparisonOperator(comparisonOperator);

			var booking = new ZDBOnlyQuery(typeof(DtbBooking));
			var instruction = new ZDBOnlySubQuery(typeof(DtbBookingInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement, notIn);
			var jobDocAddress = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			jobDocAddress.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, schemaColumn, comparisonOperator, value);

			instruction.AddSubQuery(jobDocAddress, JoinCondition.And);
			booking.AddSubQuery(instruction, JoinCondition.And);

			return booking;
		}

		ZQuery InstructionOrgHeaderFieldQuery(SQLComparisonOperator comparisonOperator, SchemaColumn schemaColumn, ZString value)
		{
			bool notIn = isNotIn(comparisonOperator);
			comparisonOperator = notInComparisonOperator(comparisonOperator);

			var booking = new ZDBOnlyQuery(typeof(DtbBooking));
			var instruction = new ZDBOnlySubQuery(typeof(DtbBookingInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement, notIn);
			var jobDocAddress = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			var orgAddress = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			var orgHeader = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
			orgHeader.AddToFilter(schemaColumn, comparisonOperator, value);

			orgAddress.AddSubQuery(orgHeader, JoinCondition.And);
			jobDocAddress.AddSubQuery(orgAddress, JoinCondition.And);
			instruction.AddSubQuery(jobDocAddress, JoinCondition.And);
			booking.AddSubQuery(instruction, JoinCondition.And);

			return booking;
		}

		ZQuery InstructionOrgAddressFieldQuery(SQLComparisonOperator comparisonOperator, SchemaColumn schemaColumn, IZType value)
		{
			bool notIn = isNotIn(comparisonOperator);
			comparisonOperator = notInComparisonOperator(comparisonOperator);

			var booking = new ZDBOnlyQuery(typeof(DtbBooking));
			var instruction = new ZDBOnlySubQuery(typeof(DtbBookingInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement, notIn);
			var jobDocAddress = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			var orgAddress = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			orgAddress.AddToFilter(schemaColumn, comparisonOperator, value);

			jobDocAddress.AddSubQuery(orgAddress, JoinCondition.And);
			instruction.AddSubQuery(jobDocAddress, JoinCondition.And);
			booking.AddSubQuery(instruction, JoinCondition.And);

			return booking;
		}

		ZQuery InstructionPackageFieldQuery(SQLComparisonOperator @operator, SchemaColumn schemaColumn, IZType value)
		{
			bool notIn = isNotIn(@operator);
			@operator = notInComparisonOperator(@operator);

			var booking = new ZDBOnlyQuery(typeof(DtbBooking));
			var instruction = new ZDBOnlySubQuery(typeof(DtbBookingInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement, notIn);
			var instructionPkgDivot = new ZDBOnlySubQuery(typeof(DtbBookingInstructionPkgDivot), DtbBookingInstructionPkgDivotSchema.KD_KN_BookingInstruction);
			var package = new ZDBOnlySubQuery(typeof(PkgPackage), DtbBookingInstructionPkgDivotSchema.KD_KP_Package);
			if (schemaColumn.TableName == PkgPackageSchema.Constants.TableName)
			{
				package.AddToFilter_PossiblyCommaSeparated(schemaColumn, @operator, value);
			}
			else
			{
				var packageID = new ZDBOnlySubQuery(typeof(PkgPackageHeader), PkgPackageSchema.KP_KPH_PackageHeader);
				packageID.AddToFilter_PossiblyCommaSeparated(schemaColumn, @operator, value);
				package.AddSubQuery(packageID, JoinCondition.And);
			}

			instructionPkgDivot.AddSubQuery(package, JoinCondition.And);
			instruction.AddSubQuery(instructionPkgDivot, JoinCondition.And);
			booking.AddSubQuery(instruction, JoinCondition.And);

			return booking;
		}

		ZQuery ConfirmationStringAttributeQuery(SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery booking = null;
			var notIn = isNotIn(comparisonOperator);
			comparisonOperator = notInComparisonOperator(comparisonOperator);

			var confirmation = new ZDBOnlySubQuery(typeof(DtbBookingConfirmation), DtbBookingConfirmationSchema.KK_KN_BookingInstruction);
			confirmation.AddToFilter(schemaColumn, comparisonOperator, value);

			var instruction = new ZDBOnlySubQuery(typeof(DtbBookingInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement, notIn);
			instruction.AddSubQuery(confirmation, JoinCondition.And);

			booking = new ZDBOnlyQuery(typeof(DtbBooking));
			booking.AddSubQuery(instruction, JoinCondition.And);

			return booking;
		}

		ZQuery ConfirmationTypeAndDateTimeAttributeQuery(ZString confirmationType, SchemaDateTimeColumn schemaColumn, DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				return ConfirmationTypeAndDateTimeAttributeHasNoDateQuery(confirmationType, schemaColumn);
			}
			else
			{
				var confirmation = new ZDBOnlySubQuery(typeof(DtbBookingConfirmation), DtbBookingConfirmationSchema.KK_KN_BookingInstruction);
				var instruction = new ZDBOnlySubQuery(typeof(DtbBookingInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement);
				var booking = new ZDBOnlyQuery(typeof(DtbBooking));

				AddDateTimeRange(confirmation, comparisonOperator, JoinCondition.And, schemaColumn, date1, date2);
				confirmation.AddToFilter(JoinCondition.And, DtbBookingConfirmationSchema.KK_ConfirmationType, confirmationType);
				instruction.AddSubQuery(confirmation, JoinCondition.And);
				booking.AddSubQuery(instruction, JoinCondition.And);
				return booking;
			}
		}

		ZQuery ConfirmationTypeAndDateTimeAttributeHasNoDateQuery(ZString confirmationType, SchemaDateTimeColumn schemaColumn)
		{
			var notInConfirmation = new ZDBOnlySubQuery(typeof(DtbBookingConfirmation), DtbBookingConfirmationSchema.KK_KN_BookingInstruction);
			var notInInstruction = new ZDBOnlySubQuery(typeof(DtbBookingInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement, true);
			AddDateRange(notInConfirmation, DateComparisonOperator.HasDateEntered, JoinCondition.And, schemaColumn, ZDate.Empty, ZDate.Empty);
			notInConfirmation.AddToFilter(JoinCondition.And, DtbBookingConfirmationSchema.KK_ConfirmationType, confirmationType);
			notInInstruction.AddSubQuery(notInConfirmation, JoinCondition.And);

			var existsConfirmation = new ZDBOnlySubQuery(typeof(DtbBookingConfirmation), DtbBookingConfirmationSchema.KK_KN_BookingInstruction);
			var existsInstruction = new ZDBOnlySubQuery(typeof(DtbBookingInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement);
			AddDateRange(existsConfirmation, DateComparisonOperator.HasNoDateEntered, JoinCondition.And, schemaColumn, ZDate.Empty, ZDate.Empty);
			existsConfirmation.AddToFilter(JoinCondition.And, DtbBookingConfirmationSchema.KK_ConfirmationType, confirmationType);
			existsInstruction.AddSubQuery(existsConfirmation, JoinCondition.And);

			var booking = new ZDBOnlyQuery(typeof(DtbBooking));

			booking.AddSubQuery(notInInstruction, JoinCondition.And);
			booking.AddSubQuery(existsInstruction, JoinCondition.Or);
			return booking;
		}

		GetGuidQueryWithOperator GetJobDocAddressQueryWithOperatorDelegate(DocAddressType addressType) => (comparisonOperator, pK) => QueryHelper.GetJobDocAddressQueryWithOperator(Factory, pK, addressType, comparisonOperator);

		ZQuery AddTwoQueries(ZQuery query1, ZQuery query2, SQLComparisonOperator comparisonOperator)
		{
			bool notIn = isNotIn(comparisonOperator);

			var result = new ZDBOnlyQuery(typeof(DtbBooking));
			result.AddToFilter(query2);
			result.AddToFilter(query1, notIn ? JoinCondition.And : JoinCondition.Or);

			return result;
		}

		bool isNotIn(SQLComparisonOperator comparisonOperator)
		{
			return
				comparisonOperator == SQLComparisonOperator.NotContains ||
				comparisonOperator == SQLComparisonOperator.NotEqual ||
				comparisonOperator == SQLComparisonOperator.DoesNotStartWith;
		}

		SQLComparisonOperator notInComparisonOperator(SQLComparisonOperator comparisonOperator)
		{
			if (isNotIn(comparisonOperator))
			{
				if (comparisonOperator == SQLComparisonOperator.NotContains)
				{
					comparisonOperator = SQLComparisonOperator.Contains;
				}
				else if (comparisonOperator == SQLComparisonOperator.NotEqual)
				{
					comparisonOperator = SQLComparisonOperator.Equal;
				}
				else if (comparisonOperator == SQLComparisonOperator.DoesNotStartWith)
				{
					comparisonOperator = SQLComparisonOperator.StartsWith;
				}
			}
			return comparisonOperator;
		}

		TransportBookingsQueryHelper QueryHelper => queryHelper ?? (queryHelper = new TransportBookingsQueryHelper());

		TransportBookingsQueryHelper queryHelper;

		ModuleFilterSubGroup ParentsSubGroup
		{
			get { return parentsSubGroup ?? (parentsSubGroup = new ParentsFilterSubGroup()); }
		}

		ModuleFilterSubGroup ParentLegsSubGroup
		{
			get { return parentLegsSubGroup ?? (parentLegsSubGroup = new ParentLegsFilterSubGroup()); }
		}

		ModuleFilterSubGroup parentsSubGroup;
		ModuleFilterSubGroup parentLegsSubGroup;

		class ParentsFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var bookingFilter = new ZDBOnlyQuery(typeof(DtbBooking));

				if (filter.Params.Any() || ColumnsThatMayExceedMaximumElementsForParameterisation.Any(c => filter.FilterPartsHashKey.Contains(c)))
				{
					var subFilter = new ZDBOnlySubQuery(typeof(AutoViewTransportBookingParents), ViewTransportBookingParentsSchema.PK);
					subFilter.AddToFilter(filter);

					var bookingConsolidationSubFilter = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking);
					bookingConsolidationSubFilter.AddSubQuery(DtbBookingConsolidationSchema.KB_ParentID, subFilter, JoinCondition.And);
					bookingFilter.AddSubQuery(bookingConsolidationSubFilter, JoinCondition.And);
				}
				else
				{
					var bookingConsolidationSubFilter = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking);
					bookingConsolidationSubFilter.AddToFilter(filter);
					bookingFilter.AddSubQuery(bookingConsolidationSubFilter, JoinCondition.And);
				}

				return bookingFilter;
			}

			List<ZString> ColumnsThatMayExceedMaximumElementsForParameterisation
			{
				get
				{
					return new List<ZString> { ViewTransportBookingParentsSchema.Constants.VP_JobNumber };
				}
			}
		}

		ZQuery GetComplexParentsFilterWithParamSubQuery(ZQuery parentsFilter, bool isFilterOnConsolNumber)
		{
			var conShipLinkSubQueryFieldSelected = isFilterOnConsolNumber ? JobConShipLinkSchema.JN_JS : JobConShipLinkSchema.JN_JK;
			var conShipLinkSubQueryFilterField = isFilterOnConsolNumber ? JobConShipLinkSchema.JN_JK : JobConShipLinkSchema.JN_JS;
			var parentsTypeFilter = isFilterOnConsolNumber ? new ZQuery(ViewTransportBookingParentsSchema.VP_JobType, "CON") : new ZQuery(ViewTransportBookingParentsSchema.VP_JobType, "SHP").AddToFilter(JoinCondition.Or, ViewTransportBookingParentsSchema.VP_JobType, "ASH");

			var bookingFilter = new ZDBOnlyQuery(typeof(DtbBooking));
			var subFilter = new ZDBOnlySubQuery(typeof(AutoViewTransportBookingParents), ViewTransportBookingParentsSchema.PK);
			subFilter.AddToFilter(parentsFilter);
			subFilter.AddToFilter(parentsTypeFilter, JoinCondition.And);

			var subFilterPart2 = new ZDBOnlySubQuery(typeof(AutoJobConShipLink), conShipLinkSubQueryFieldSelected);
			subFilterPart2.AddSubQuery(conShipLinkSubQueryFilterField, (ZDBOnlySubQuery)subFilter.DeepClone(), JoinCondition.And);

			subFilter.AddAsUnionQuery(subFilterPart2, true);
			var bookingConsolidationSubFilter = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking);
			bookingConsolidationSubFilter.AddSubQuery(DtbBookingConsolidationSchema.KB_ParentID, subFilter, JoinCondition.And);
			bookingFilter.AddSubQuery(bookingConsolidationSubFilter, JoinCondition.And);

			return bookingFilter;
		}

		class ParentLegsFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlySubQuery subFilter = new ZDBOnlySubQuery(typeof(AutoViewTransportBookingParentLegs), ViewTransportBookingParentLegsSchema.VL_KB);
				subFilter.AddToFilter(filter);

				var result = new ZDBOnlyQuery(typeof(DtbBooking));
				result.AddSubQuery(DtbBookingSchema.KM_KB_Booking, subFilter, JoinCondition.And);

				return result;
			}
		}

		CodeDescriptionPairList GetAdditionalReferenceNumberTypes()
		{
			var list = new CodeDescriptionPairList();
			list.AddRange(TransportRegistry.Instance.AdditionalReferenceNumbers.Value);
			return list;
		}

		BindToLists BindToLists
		{
			get { return bindToLists ?? (bindToLists = new BindToLists(Factory)); }
		}

		BindToLists bindToLists;

		readonly DtbBookingCRMSecurityProvider SecurityProvider = new DtbBookingCRMSecurityProvider();

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddAccountingFilters(filters);
			AddOrganisationFilters(filters);
			AddModuleFilters(filters);

			return filters;
		}

		void AddAccountingFilters(ModuleFilterCollection result)
		{
			AccountingFilterStrip.AddBillingFilters(result);
			AccountingFilterStrip.AddJobManagementFilters(result, GetSecurityCheckPoint());
		}

		SecurityCheckpoint GetSecurityCheckPoint()
		{
			return Env.Security.DtbBookingJobInvoicing;
		}
	}
}
