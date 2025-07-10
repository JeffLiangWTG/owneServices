using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Integration
{
	public static class CCAValidationMessageProvider
	{
		#region Contract Header Messages

		public static string InvalidTransportMode(IRatingContract contract, ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"0380775d-a077-a1aa-4753-b0a443f54b7d",
			"Transport Mode '{0}' of {1} is NOT the same as the Transport Mode '{2}' of Carrier Contract for allocation.",
			validationData.TransportMode,
			validationData.Name,
			contract.RCT_TransportMode);

		public static string InvalidBookingTransportMode(IRatingContract contract, IQuotedBooking booking) => Res.GetString(
			"c5de9152-fd2e-8e8d-4624-3457541ed965",
			"Mode '{0}' of this Booking is NOT under or aligned with the Transport mode '{1}' of Carrier Contract for allocation.",
			booking.Mode,
			contract.RCT_TransportMode);

		public static string MissingContractRecord() => Res.GetString(
			"5490c0cc-5af0-8da6-4d69-c4897c3c7f1f",
			"This number does not have a corresponding Carrier Contract & Allocations record.");

		public static string CarrierContractMismatch(ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"354fc062-fa73-7188-44c4-7373367248c7",
			"Carrier '{0}' of {1} does not match Service Provider(s) of any Carrier Contract(s) with this number.",
			validationData.ContractServiceProvider.OH_Code,
			validationData.Name);

		public static string MissingCarrierOnJob(ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"276aa64a-f163-70b1-498c-9930a8a5a937",
			"Carrier of {0} does not match Service Provider(s) of any Carrier Contract(s) with this number.",
			validationData.Name);

		public static string ContractStartDateViolated(IRatingContract contract, ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"c5c70215-5fa0-7884-41a4-cd2f16dd1c30",
			"Start Date ({0}) of Carrier Contract {1} is later than the ETD ({2}) of this {3}. {3} departure should be within the validity period of the selected Carrier Contract.",
			contract.RCT_StartDate.ToString(),
			contract.RCT_ContractNumber.ToString(),
			validationData.ETD.Date.ToString(),
			validationData.Name);

		public static string ContractExpiryDateViolated(IRatingContract contract, ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"3f07c5fd-a99a-84a6-4d7b-6f559e70e4da",
			"Expiry Date ({0}) of Carrier Contract {1} is earlier than the ETD ({2}) of this {3}. {3} departure should be within the validity period of the selected Carrier Contract.",
			contract.RCT_EndDate.ToString(),
			contract.RCT_ContractNumber,
			validationData.ETD.Date.ToString(),
			validationData.Name);

		public static string ContainerTypesInvalid(IRatingContract contract, ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"ea4dea1e-47b0-48b8-47b2-88e2d7a05d70",
			"NOT all Containers on {0} {1} share the same Container Type ({2}) of the selected Carrier Contract {3}. {0} Container(s) and selected Carrier Contract should share the same Container Type.",
			validationData.Name,
			validationData.UniqueConsignRef,
			contract.RCT_ContainerType,
			contract.RCT_ContractNumber);

		public static string HazardousContainerCommoditiesInvalid(IRatingContract contract, ICCACommonAssignmentValidationData validationData)
		{
			var hazardousContainer = validationData.Containers.FirstOrDefault(container =>
			{
				var commodity = validationData.Factory.LoadFromNaturalKey<IRefCommodityCode>(RefCommodityCodeSchema.RH_Code, container.JC_RH_NKContainerCommodityCode);
				return commodity.RH_IsHazardous;
			});

			var readableContainerName = hazardousContainer.JC_ContainerNum.IsEmpty
				? Res.GetString("077c5d23-23e0-ea95-4f7f-e99316425ea4", "Container")
				: Res.GetString("79616d6a-b3a2-4492-4c6f-aa3f78c6fcb7", "Container {0}", hazardousContainer.JC_ContainerNum);

			return Res.GetString(
				"1f3f0156-53e6-0085-4c62-5d54cd7b5047",
				"Hazardous Commodities are not allowed for Carrier Contract {0} but {1} has Commodity {2} with 'Is this Commodity Hazardous' checked. Only {3}(s) and Container(s) without Hazardous Commodities can be allocated.",
				contract.RCT_ContractNumber,
				readableContainerName,
				hazardousContainer.JC_RH_NKContainerCommodityCode,
				validationData.Name);
		}

		public static string InvalidConsolForContractNamedAccounts(IRatingContract contract) => Res.GetString(
			"0ba2e19e-09ab-ba9f-4477-077951ee38c9",
			"All of the Consol's Shipments should have at least one client that matches a Named Account of Carrier Contract {0} (i.e. no matching Clients, Consignors, Consignees, or Controlling Customers for a Consol Shipment).",
			contract.RCT_ContractNumber);

		public static string InvalidConsolForNonHazardousContract(IRatingContract contract, IForwardingConsol consol) => Res.GetString(
			"e6135286-3ee1-a28a-4a6a-0a594af5c7c8",
			"Hazardous Commodities are not allowed for Carrier Contract {0} but Consol {1} has Pre-Allocation > Is Hazardous checked. Only Consol(s) with 'Is Hazardous' not checked can be allocated.",
			contract.RCT_ContractNumber,
			consol.JK_UniqueConsignRef);

		public static string InvalidBookingForContractNamedAccounts(IRatingContract contract) => Res.GetString(
			"4fdb2484-5660-51be-4215-38256e889de3",
			"At least one of the Booking clients should match a Named Account of Carrier Contract {0} (i.e. no matching Clients, Consignors, Consignees, or Controlling Customers).",
			contract.RCT_ContractNumber);

		public static string InvalidConsolDatesForContract(IRatingContract contract) => Res.GetString(
			"217c263a-d118-2f9c-4b89-8708ff1380ed",
			"None of the ETDs under the relevant Consol Routing Leg(s) fall within the validity period between the Start Date ({0}) and the Expiry Date ({1}) of Contract {2}.",
			contract.RCT_StartDate.ToShortDateString(),
			contract.RCT_EndDate.ToShortDateString(),
			contract.RCT_ContractNumber);

		#endregion

		#region Allocation Route Messages

		public static string MissingContractWhenAllocatedToRoute(ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"576339a3-92f8-0690-44d4-0e9b35d62ad4",
			"This {0} doesn't have any Carrier Contracts allocated. Allocation Route ID selected for the {0} and/or its Containers should be from the same Carrier Contract that this {0} is allocated to.",
			validationData.Name);

		public static string InvalidAllocationRoute(ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"7899189a-2969-4793-4b6f-a46d77a1c344",
			"Allocation Route ID selected for the {0} and/or its Containers should be valid and from the same Carrier Contract ({1}) that this {0} is allocated to.",
			validationData.Name,
			validationData.CarrierContract?.RCT_ContractNumber);

		public static string AllocationRouteAndContractMismatch(ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"f26a5973-2b6c-0e8c-40eb-1586827c2dd4",
			"Allocation Route ID selected for the {0} and/or its Containers should be from the same Carrier Contract ({1}) that this {0} is allocated to.",
			validationData.Name,
			validationData.CarrierContract?.RCT_ContractNumber);

		public static string InvalidEmptyRouteOnContainer(ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"6d19fcb3-1bd3-c5bf-46eb-5100877f427c",
			"Allocation Route cannot be empty on a Container while an Allocation Route is selected for the {0}.",
			validationData.Name);

		public static string JobAndContainerAllocationRouteMismatch(IRatingContractAllocationLine allocationRoute, ICCACommonAssignmentValidationData validationData, ZString containerAllocationID) => Res.GetString(
			"33c6490c-d697-f284-4998-0d4cc6d81e2f",
			"Allocation Route {0} selected for the {1} does not match the Allocation Route {2} selected for the Container.",
			allocationRoute.RCA_AllocationLineID,
			validationData.Name,
			containerAllocationID);

		public static string AllocationRouteStartDateViolated(IRatingContractAllocationLine allocationRoute, ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"0cf6f735-5024-04b3-4b61-8b3ba2bc091a",
			"Start Date ({0}) of Allocation Route {1} is later than the ETD ({2}) of this {3}. {3} departure should be within the validity period of the selected Allocation Route.",
			allocationRoute.StartDateWithContractFallback.ToString(),
			allocationRoute.RCA_AllocationLineID,
			validationData.ETD.Date.ToString(),
			validationData.Name);

		public static string AllocationRouteExpiryDateViolated(IRatingContractAllocationLine allocationRoute, ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"f0cc8501-47e9-839d-43a9-4d0f6e37c0b9",
			"Expiry Date ({0}) of Allocation Route {1} is earlier than the ETD ({2}) of this {3}. {3} departure should be within the validity period of the selected Allocation Route.",
			allocationRoute.ExpiryDateWithContractFallback.ToString(),
			allocationRoute.RCA_AllocationLineID,
			validationData.ETD.Date.ToString(),
			validationData.Name);

		public static string InvalidLoadPort(IRatingContractAllocationLine allocationRoute, ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"51073767-ff71-198a-4943-f950dfb41898",
			"Load Port ({0}) of Allocation Route {1} does not match the Load Port ({2}) of this {3}. {3} Load Port should match the selected Allocation Route's Load Port.",
			allocationRoute.RCA_LoadLocation,
			allocationRoute.RCA_AllocationLineID,
			validationData.LoadPort,
			validationData.Name);

		public static string InvalidDischargePort(IRatingContractAllocationLine allocationRoute, ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"264bcd8a-b059-faaf-4f61-828570ddb5cc",
			"Discharge Port ({0}) of Allocation Route {1} does not match the Discharge Port ({2}) of this {3}. {3} Discharge Port should match the selected Allocation Route's Discharge Port.",
			allocationRoute.RCA_DischargeLocation,
			allocationRoute.RCA_AllocationLineID,
			validationData.DischargePort,
			validationData.Name);

		public static string InvalidVoyageNumber(IRatingContractAllocationLine allocationRoute, ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"fa7aa540-8a08-428c-4ce0-43218ec39901",
			"Voyage ({0}) of Allocation Route {1} does not match the Voyage ({2}) of this {3}. {3} Voyage should match the selected Allocation Route's Voyage.",
			allocationRoute.RCA_VoyageNumber,
			allocationRoute.RCA_AllocationLineID,
			validationData.VoyageFlight,
			validationData.Name);

		public static string InvalidVessel(IRatingContractAllocationLine allocationRoute, ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"c26daf89-7bf8-feb8-4153-dd9b6182bad5",
			"Vessel ({0}) of Allocation Route {1} does not match the Vessel ({2}) of this {3}. {3} Vessel should match the selected Allocation Route's Vessel.",
			allocationRoute.RCA_RV_NKVessel,
			allocationRoute.RCA_AllocationLineID,
			validationData.Vessel,
			validationData.Name);

		public static string ContainerBookingLimitExceeded(IRatingContractAllocationLine allocationRoute, ZDecimal exceededUtilisation) => Res.GetString(
			"b57813eb-0046-c6b3-4fe6-1949547d77c6",
			"Number of Containers to be allocated exceeds available capacity of the selected Allocation Route {0} by {1}. Number of Containers should be within the Booking Limit of the selected Allocation Route.",
			allocationRoute.RCA_AllocationLineID,
			exceededUtilisation);

		public static string TEUBookingLimitExceeded(IRatingContractAllocationLine allocationRoute, ZDecimal exceededUtilisation) => Res.GetString(
			"5680b311-251e-889a-4cef-9d572e3d62a0",
			"Number of TEUs to be allocated exceeds available capacity of the selected Allocation Route {0} by {1} TEUs. Number of TEUs should be within the Booking Limit of the selected Allocation Route.",
			allocationRoute.RCA_AllocationLineID,
			exceededUtilisation);

		public static string ContainerTypeMismatch(IRatingContractAllocationLine allocationRoute, IRefContainer refContainer, ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"a180171e-ac6c-4996-4651-d70f8d13ff34",
			"Container Code ({0}) of Allocation Route {1} does not match the Container Code ({2}) of this Container. Container on a {3} and on a selected Allocation Route should share the same Container Code.",
			allocationRoute.RefContainerType?.RC_Code,
			allocationRoute.RCA_AllocationLineID,
			refContainer.RC_Code,
			validationData.Name);

		public static string ContainerClassMismatch(IRatingContractAllocationLine allocationRoute, IRefContainer refContainer, ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"9aa87fc6-e1c1-8693-498e-998ae84f5999",
			"Container Class ({0}) of Allocation Route {1} does not match the Container Class ({2}) of this Container. Container on a {3} and on a selected Allocation Route should share the same Container Class.",
			allocationRoute.RCA_StorageOrFreightRateClass,
			allocationRoute.RCA_AllocationLineID,
			GetHumanReadableContainerClass(refContainer),
			validationData.Name);

		public static string InvalidContainerCodeInCollection(IRatingContractAllocationLine allocationRoute, ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"85eaed00-7bf8-c482-4e42-8bf09391383a",
			"NOT all Containers on this {0} share the same Container Code ({1}) of the selected Allocation Route {2}. {0} Container Codes should match the selected Allocation Route's Container Code.",
			validationData.Name,
			allocationRoute.RefContainerType?.RC_Code,
			allocationRoute.RCA_AllocationLineID);

		public static string InvalidContainerClassInCollection(IRatingContractAllocationLine allocationRoute, ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"9a074543-be82-709c-49c4-0f0ad3f0edcf",
			"NOT all Containers on this {0} share the same Container Class ({1}) of the selected Allocation Route {2}. Container Class of {0} Containers should match the selected Allocation Route's Container Class.",
			validationData.Name,
			allocationRoute.RCA_StorageOrFreightRateClass,
			allocationRoute.RCA_AllocationLineID);

		public static string InvalidConsolForAllocationRouteNamedAccounts(IRatingContractAllocationLine allocationRoute) => Res.GetString(
			"855e58f3-23b5-2299-45c1-e7890926649e",
			"All of the Consol's Shipments should have at least one client that matches a Named Account of Carrier Contract {0} and Allocation Route {1} (i.e. no matching Clients, Consignors, Consignees, or Controlling Customers for a Consol Shipment).",
			allocationRoute.Contract.RCT_ContractNumber, allocationRoute.RCA_AllocationLineID);

		public static string InvalidContainerTypeForContract(IRatingContractAllocationLine allocationRoute, IRefContainer refContainer, ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"f0d192ba-4116-659a-433c-61f283a56dea",
			"Container Type ({0}) of Allocation Route's Parent Contract {1} does not match the Container Type ({2}) of this Container. Container(s) on a {3} and on a selected Allocation Route's Parent Contract should share the same Container Type.",
			allocationRoute.Contract.RCT_ContainerType,
			allocationRoute.Contract.RCT_ContractNumber,
			refContainer?.RC_ContainerType ?? ZString.Empty,
			validationData.Name);

		public static string InvalidContainerCodeForRoute(IRatingContractAllocationLine allocationRoute, IRefContainer refContainer, ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"24255702-8120-97b2-4ac8-e90bd6f289dd",
			"Container Code ({0}) of Allocation Route {1} does not match the Container Code ({2}) of this Container. Container on a {3} and on a selected Allocation Route should share the same Container Code.",
			allocationRoute.RefContainerType?.RC_Code,
			allocationRoute.RCA_AllocationLineID,
			refContainer?.RC_Code ?? ZString.Empty,
			validationData.Name);

		public static string InvalidContainerClassForRoute(IRatingContractAllocationLine allocationRoute, IRefContainer refContainer, ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"579f47f2-572c-c299-425a-1f16b9316639",
			"Container Class ({0}) of Allocation Route {1} does not match the Container Class ({2}) of this Container. Container on a {3} and on a selected Allocation Route should share the same Container Class.",
			allocationRoute.RCA_StorageOrFreightRateClass,
			allocationRoute.RCA_AllocationLineID,
			GetHumanReadableContainerClass(refContainer),
			validationData.Name);

		public static string InvalidServiceStringOnConsol(IRatingContractAllocationLine allocationRoute, IForwardingConsol consol) => Res.GetString(
			"6998c860-0d0b-4ab7-4021-399802e75d3b",
			"Service String ({0}) of Allocation Route {1} does not match the Service String of any Routing Leg on Consol {2}. Consol Service String should match the selected Allocation Route's Service String.",
			allocationRoute.RCA_ServiceLoop,
			allocationRoute.RCA_AllocationLineID,
			consol.JK_UniqueConsignRef);

		public static string InvalidConsolContainerForAllocationRouteNamedAccounts(IRatingContractAllocationLine allocationRoute) => Res.GetString(
			"8b78703f-c22a-e2a8-4f4c-43c4621c2c8e",
			"At least one of the Container's Consol clients should match a Named Account of Carrier Contract {0} and Allocation Route {1} (i.e. no matching Clients, Consignors, Consignees, or Controlling Customers).",
			allocationRoute.Contract.RCT_ContractNumber, allocationRoute.RCA_AllocationLineID);

		public static string InvalidBookingForAllocationRouteNamedAccounts(IRatingContractAllocationLine allocationRoute) => Res.GetString(
			"b3b6e51f-17d2-f19e-4221-dacd8a21a508",
			"At least one of the Booking clients should match a Named Account of Carrier Contract {0} and Allocation Route {1} (i.e. no matching Clients, Consignors, Consignees, or Controlling Customers).",
			allocationRoute.Contract.RCT_ContractNumber, allocationRoute.RCA_AllocationLineID);

		public static string InvalidBookingContainerForAllocationRouteNamedAccounts(IRatingContractAllocationLine allocationRoute) => Res.GetString(
			"9a06f113-33a5-21ba-4416-1e8386457508",
			"At least one of the Container's Booking clients should match a Named Account of Carrier Contract {0} and Allocation Route {1} (i.e. no matching Clients, Consignors, Consignees, or Controlling Customers).",
			allocationRoute.Contract.RCT_ContractNumber, allocationRoute.RCA_AllocationLineID);

		public static string InvalidBookingForLinkedAllocationRoute(IRatingContractAllocationLine allocationRoute) => Res.GetString(
			"f66394b8-72b2-d680-4df8-3895c0a6ead7",
			"Allocation Route {0} is linked to Schedule {1}. However, this Booking is not linked to the corresponding Schedule.",
			allocationRoute.RCA_AllocationLineID,
			allocationRoute.JobSailing?.JX_UniqueReference);

		public static string InvalidBookingContainerForLinkedAllocationRoute(IRatingContractAllocationLine allocationRoute) => Res.GetString(
			"9710bc2f-bcf1-0582-470b-2a8732c4aa12",
			"Allocation Route {0} is linked to Schedule {1}. However, the Booking for this Container is not linked to the corresponding Schedule.",
			allocationRoute.RCA_AllocationLineID,
			allocationRoute.JobSailing?.JX_UniqueReference);

		public static string InvalidConsolForLinkedAllocationRoute(IRatingContractAllocationLine allocationRoute) => Res.GetString(
			"1c7ed071-e05b-51b1-4dfd-7cf1b50617f4",
			"Allocation Route {0} is linked to Schedule {1}. However, this Consol has no legs matching Schedule details.",
			allocationRoute.RCA_AllocationLineID,
			allocationRoute.JobSailing?.JX_UniqueReference);

		public static string InvalidConsolContainerForLinkedAllocationRoute(IRatingContractAllocationLine allocationRoute) => Res.GetString(
			"d5f6dbff-ab19-eba9-400d-13ff564b4fed",
			"Allocation Route {0} is linked to Schedule {1}. However, the Consol for this Container has no legs matching Schedule details.",
			allocationRoute.RCA_AllocationLineID,
			allocationRoute.JobSailing?.JX_UniqueReference);

		public static string InvalidLoadPortForConsol(IRatingContractAllocationLine allocationRoute, IForwardingConsol consol) => Res.GetString(
			"8dae638e-108b-d1bd-49fd-b51a2666b65a",
			"The Load Port ({0}) of Allocation Route {1} does not match the First Load Port nor any Load Port of Consol {2}. The First Load Port or the Load Port of one of the Consol Routing Leg(s) should match the selected Allocation Route's Load Port.",
			allocationRoute.RCA_LoadLocation,
			allocationRoute.RCA_AllocationLineID,
			consol.JK_UniqueConsignRef);

		public static string InvalidLoadPortForConsolContainer(IRatingContractAllocationLine allocationRoute) => Res.GetString(
			"80527b01-88d0-2e86-465e-74040dcfa7a6",
			"The Load Port ({0}) of Allocation Route {1} does not match the First Load Port nor any Load Port of the Consol for this Container. The First Load Port of the Load Port of one of the Consol Routing Leg(s) should match the selected Allocation Route's Load Port.",
			allocationRoute.RCA_LoadLocation,
			allocationRoute.RCA_AllocationLineID);

		public static string InvalidDischargePortForConsol(IRatingContractAllocationLine allocationRoute, IForwardingConsol consol) => Res.GetString(
			"24597e68-d184-3a93-4bc2-85e6418c507d",
			"The Discharge Port ({0}) of Allocation Route {1} does not match the Last Discharge Port nor any Discharge Port of Consol {2}. The Last Discharge Port or the Discharge Port of one of the Consol Routing Leg(s) should match the selected Allocation Route's Discharge Port.",
			allocationRoute.RCA_DischargeLocation,
			allocationRoute.RCA_AllocationLineID,
			consol.JK_UniqueConsignRef);

		public static string InvalidDischargePortForConsolContainer(IRatingContractAllocationLine allocationRoute) => Res.GetString(
			"379d5170-3865-bdb4-4b98-188a78122161",
			"The Discharge Port ({0}) of Allocation Route {1} does not match the Last Discharge Port nor any Discharge Port of the Consol for this Container. The Last Discharge Port or the Discharge Port of one of the Consol Routing Leg(s) should match the selected Allocation Route's Discharge Port.",
			allocationRoute.RCA_DischargeLocation,
			allocationRoute.RCA_AllocationLineID);

		public static string InvalidConsolDatesForAllocationRoute(IRatingContractAllocationLine allocationRoute) => Res.GetString(
			"a303f535-0545-edb9-42d9-81298d13a64c",
			"None of the ETDs under the relevant Consol Routing Leg(s) fall within the validity period between the Start Date ({0}) and the Expiry Date ({1}) of Allocation Route {2}.",
			allocationRoute.StartDateWithContractFallback.ToShortDateString(),
			allocationRoute.ExpiryDateWithContractFallback.ToShortDateString(),
			allocationRoute.RCA_AllocationLineID
			);

		public static string InvalidConsolContainerDatesForAllocationRoute(IRatingContractAllocationLine allocationRoute) => Res.GetString(
			"464cc947-b439-3d8d-4129-5576dcf0a675",
			"None of the ETDs under the relevant Routing Leg(s) of the Consol for this Container fall within the validity period between the Start Date ({0}) and the Expiry Date ({1}) of Allocation Route {2}.",
			allocationRoute.StartDateWithContractFallback.ToShortDateString(),
			allocationRoute.ExpiryDateWithContractFallback.ToShortDateString(),
			allocationRoute.RCA_AllocationLineID);

		public static string InvalidConsolETDForAllocationRoute(IRatingContractAllocationLine allocationRoute, ICCACommonAssignmentValidationData validationData) => Res.GetString(
			"1351cdf7-a247-4d6b-b900-fe3e00f3b2aa",
			"Consol's departure date ({0}) is outside the Allocation Route {1} period from {2} to {3}.",
			validationData.ETD.Date.ToShortDateString(),
			allocationRoute.RCA_AllocationLineID,
			allocationRoute.StartDateWithContractFallback.ToShortDateString(),
			allocationRoute.ExpiryDateWithContractFallback.ToShortDateString());

		public static string InvalidScheduleDetailsOnConsol(IRatingContractAllocationLine allocationRoute, IForwardingConsol consol) => Res.GetString(
			"dd7116a9-7bd1-e5b2-4533-7d85f8e0961a",
			"There is no relevant Routing Leg on Consol {0} that match the Voyage, Vessel, and Service String of Allocation Route {1}.",
			consol.JK_UniqueConsignRef,
			allocationRoute.RCA_AllocationLineID);

		public static string InvalidScheduleDetailsOnConsolContainer(IRatingContractAllocationLine allocationRoute) => Res.GetString(
			"6bdad7f2-6d71-eaab-49f2-623fb1428c86",
			"There is no relevant Routing Leg on the Consol for this Container that match the Voyage, Vessel, and Service String of Allocation Route {0}.",
			allocationRoute.RCA_AllocationLineID);

		public static string MissingGatewayAgent(IRatingContractAllocationLine allocationRoute) => Res.GetString(
			"7c3e35b1-48ec-9da9-4ff3-f121db7fc810",
			"Allocation Route {0} is restricted to Gateway Consols only, but this Consol has no assigned Gateway Agent. Either the Sending or Receiving Agent of this Consol must be assigned as a Gateway Agent.",
			allocationRoute.RCA_AllocationLineID);

		public static string InvalidContainerOwnerInCollection(IRatingContractAllocationLine allocationRoute)
		{
			return allocationRoute.RCA_ContainerOwner == Core.Constants.ContainerOwnership.Codes.ShipperOwned
				? Res.GetString("b26a3469-b9c3-3c85-4983-71743c808af6", "Only Shipper Owned Containers are eligible to consume Allocation Route {0} but one or more Container(s) to be allocated are not Shipper Owned.", allocationRoute.RCA_AllocationLineID)
				: Res.GetString("12a61fdf-7373-2187-4129-9a3b4b1df081", "Only Non-Shipper Owned Containers are eligible to consume Allocation Route {0} but one or more Container(s) to be allocated are Shipper Owned.", allocationRoute.RCA_AllocationLineID);
		}

		public static string InvalidContainerOwnerForRoute(IRatingContractAllocationLine allocationRoute)
		{
			return allocationRoute.RCA_ContainerOwner == Core.Constants.ContainerOwnership.Codes.ShipperOwned
				? Res.GetString("c0c2b69c-a312-4d2f-a6ec-c5f26a087572", "Only Shipper Owned Containers are eligible to consume Allocation Route {0} but this container is not Shipper Owned.", allocationRoute.RCA_AllocationLineID)
				: Res.GetString("a45b1088-0a29-4b3b-a6b8-69ba6a064420", "Only Non-Shipper Owned Containers are eligible to consume Allocation Route {0} but this container is Shipper Owned.", allocationRoute.RCA_AllocationLineID);
		}

		public static string GroupageContainerModeConsolsOnly(IRatingContractAllocationLine allocationRoute, IForwardingConsol consol) => Res.GetString(
			"574141b5-16de-4607-8885-f3387097ee67",
			"Allocation Route {0} is restricted to Consols with Groupage container mode. The Consol's current container mode is {1}.",
			allocationRoute.RCA_AllocationLineID,
			consol.JK_ConsolMode);

		public static string InvalidConsolForAllocationRouteAgents(IRatingContractAllocationLine allocationRoute, IForwardingConsol consol, IOrgHeader sendingForwarder, IOrgHeader receivingForwarder)
		{
			if (sendingForwarder == null && receivingForwarder == null)
			{
				return Res.GetString(
				"c7e1917a-b090-2086-4161-db00f5d6859c",
				"Neither Sending Agent nor Receiving Agent of the Consol matches with the Agents specified on Allocation Route {0} under Carrier Contract {1}.",
				allocationRoute.RCA_AllocationLineID,
				allocationRoute.Contract.RCT_ContractNumber);
			}

			return Res.GetString(
				"c64319c3-14e7-9193-4589-774e4f8d35e8",
				"Neither Sending Agent {0} nor Receiving Agent {1} of the Consol matches with the Agents specified on Allocation Route {2} under Carrier Contract {3}.",
				sendingForwarder.OH_Code,
				receivingForwarder.OH_Code,
				allocationRoute.RCA_AllocationLineID,
				allocationRoute.Contract.RCT_ContractNumber);
		}

		public static string InvalidConsolForAllocationRouteAgents(IRatingContractAllocationLine allocationRoute, IForwardingConsol consol, string forwarder)
		{
			return Res.GetString(
				"f606d453-41ca-cfa2-4e55-e6f2e55c0651",
				"{0} of the Consol does not match with the Agents specified on Allocation Route {1} under Carrier Contract {2}.",
				forwarder,
				allocationRoute.RCA_AllocationLineID,
				allocationRoute.Contract.RCT_ContractNumber);
		}

		public static string InvalidPlaceOfReceiptAndFirstLoadForConsol(IRatingContractAllocationLine allocationRoute, IForwardingConsol consol)
		{
			return Res.GetString(
				"cc71b87a-7265-1386-421f-066a566b3fc3",
				"Mismatch: ‘Place of receipt’ ({0}) must match ‘1st Load’ ({1}) in ‘Allocated consol’ ({2}).",
				allocationRoute.RCA_PlaceOfReceipt,
				consol.JK_RL_NKLoadPort,
				consol.JK_UniqueConsignRef
			);
		}

		public static string InvalidPlaceOfDeliveryAndLastDischargeForConsol(IRatingContractAllocationLine allocationRoute, IForwardingConsol consol)
		{
			return Res.GetString(
				"7b602a07-2a2d-548b-4c06-047283ec31f1",
				"Mismatch: ‘Place of delivery’ ({0}) must match ‘Last  discharge’ ({1}) in ‘Allocated consol’ ({2}).",
				allocationRoute.RCA_PlaceOfDelivery,
				consol.JK_RL_NKDischargePort,
				consol.JK_UniqueConsignRef
			);
		}

		public static string InvalidPlaceOfReceiptLoadPortForBooking(IRatingContractAllocationLine allocationRoute, IQuotedBooking booking)
		{
			return Res.GetString(
				"18c3fe84-f1ec-1882-4a82-c4f475202d19",
				"Mismatch: ‘Place of receipt’ ({0}) must match ‘Load port’ ({1}) or ‘Origin’ ({2}) in ‘Allocated booking’ ({3}).",
				allocationRoute.RCA_PlaceOfReceipt,
				booking.LoadPort,
				booking.Origin,
				booking.UniqueConsignRef
			);
		}

		public static string InvalidPlaceOfDeliveryDestinationForBooking(IRatingContractAllocationLine allocationRoute, IQuotedBooking booking)
		{
			return Res.GetString(
				"1d0304b5-3ea8-bd9a-437e-3be044eb5610",
				"Mismatch: ‘Place of delivery’ ({0}) must match ‘Discharge port’ ({1}) or ‘Destination’ ({2}) in ‘Allocated booking’ ({3}).",
				allocationRoute.RCA_PlaceOfDelivery,
				booking.DischargePort,
				booking.Destination,
				booking.UniqueConsignRef
			);
		}

		public static string MissingAllocationAtRouteLevel() => Res.GetString(
			"e997fc5c-a91f-f885-4321-d70c3cc177b5",
			"Consol can only be allocated to a Carrier Contract down to Allocation Route level but not just at header level. As Consol is allocated to Carrier Contract, either the Allocation ID under Details > Pre-Allocation must have a valid Allocation Route ID, or the Allocation ID under all Containers must have valid Allocation Route IDs.");

		public static string GrossContainerWeightExceedsAllocationLimit(IRatingContractAllocationLine allocationRoute)
		{
			if (allocationRoute.RCA_ContainerWeightLimitType == ContainerWeightLimitType.AbsolutePerTEU)
			{
				return Res.GetString(
					"4ff075c4-c6d5-442d-907a-f5911ee16eb4",
					"Over-limit. Per-TEU gross weight exceeds the {0}{1} limit on allocation route {2}.",
					Utilities.Round(allocationRoute.RCA_ContainerWeightLimit, 2).ToString("F2"),
					allocationRoute.RCA_ContainerWeightLimitUQ,
					allocationRoute.RCA_AllocationLineID);
			}
			else
			{
				return Res.GetString(
					"8e415a42-8e97-478e-a6d7-32c7fec00990",
					"Over-limit. Avg per-TEU weight exceeds the {0}{1} limit on allocation route {2}.",
					Utilities.Round(allocationRoute.RCA_ContainerWeightLimit, 2).ToString("F2"),
					allocationRoute.RCA_ContainerWeightLimitUQ,
					allocationRoute.RCA_AllocationLineID);
			}
		}

		#endregion

		static string GetHumanReadableContainerClass(IRefContainer refContainer)
		{
			if (refContainer == null)
			{
				return string.Empty;
			}

			var storageClassIsSet = !refContainer.RC_StorageClass.IsEmpty;
			var freightRateClassIsSet = !refContainer.RC_FreightRateClass.IsEmpty;
			var classes = new string[] { refContainer.RC_StorageClass, refContainer.RC_FreightRateClass };

			return string.Join("/", classes.Where(c => !string.IsNullOrEmpty(c)));
		}
	}
}
