using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;
using Constants = Enterprise.Core.Constants;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class BookingRequestBuilder : CarrierMessageDataBuilder
	{
		public BookingRequestBuilder(ForwardingConsol consol, IDocDataObjectParameters parameters)
			: base(consol)
		{
			this.logProvider = parameters?.LogProvider;
		}

		readonly IStmALogProvider logProvider;

		protected override CarrierMessageData GetNewCarrierMessageData()
		{
			return new CarrierMessageData(nameof(ForwardingConsol),
				consol.CarrierShipperReferenceWithFallback,
				DataContext.BookingRequest);
		}

		protected override void PopulateContainers(CarrierMessageData wrapper)
		{
			base.PopulateContainers(wrapper);

			var infos = GetTransportBookingPickupDeliveryInfos();
			foreach (var container in wrapper.Containers)
			{
				container.TransportBookingPickupDeliveryInfos = infos.Where(info =>
					info.Packages?.Any(package =>
						package.KP_PackageID.IsEmpty ? package.PackageIDWithFallback == container.ContainerCount + Constants.ContainerModes.Containerised + container.Type.Code
							: package.KP_PackageID == container.Number) ?? false
					)?.ToList()
					?? new List<TransportBookingPickupDeliveryInfo>();
			}
		}

		List<TransportBookingPickupDeliveryInfo> GetTransportBookingPickupDeliveryInfos()
		{
			var infos = new List<TransportBookingPickupDeliveryInfo>();
			var matchHeaderPK = consol.IsCoLoad ? consol.Creditor?.PK : consol.ShippingLine?.PK;
			var bookings = TransportBookingLoader.GetRelatedTransportBookingEvents(consol)
													.OfType<IDtbBooking>()
													.Where(b => b.Address?.OrganisationPK == matchHeaderPK);

			foreach (var booking in bookings.Where(b => b.KM_Direction == Constants.CartageDirection.Export || b.KM_Direction == Constants.CartageDirection.Origin))
			{
				foreach (var instruction in booking.Instructions.Where(i => (i.KN_InstructionType == InstructionTypes.Codes.PickUp || i.KN_InstructionType == InstructionTypes.Codes.Multi) && (i.OrganisationType == Constants.PaymentParty.Consignor || i.OrganisationType == WebPartyType.CFS) && (i.Packages?.Any() ?? false) && i.Address != null && !((JobDocAddress)i.Address).IsEmpty))
				{
					var pickupInfo = new TransportBookingPickupDeliveryInfo();
					pickupInfo.Type = "PickupFrom";
					pickupInfo.Packages = instruction.Packages?.Where(p => p.KP_F3_NKPackType == Constants.ContainerModes.Containerised);
					pickupInfo.Address = AddressBuilder.Create(context, instruction.Address);
					pickupInfo.AddressETD = instruction.EstimatedDate(true);
					infos.Add(pickupInfo);
				}
			}

			foreach (var booking in bookings.Where(b => b.KM_Direction == Constants.CartageDirection.Import || b.KM_Direction == Constants.CartageDirection.Destination))
			{
				foreach (var instruction in booking.Instructions.Where(i => (i.KN_InstructionType == InstructionTypes.Codes.Delivery || i.KN_InstructionType == InstructionTypes.Codes.Multi) && (i.OrganisationType == Constants.PaymentParty.Consignee || i.OrganisationType == WebPartyType.CFS) && (i.Packages?.Where(p => p.KP_F3_NKPackType == Constants.ContainerModes.Containerised)?.Any() ?? false) && i.Address != null && !((JobDocAddress)i.Address).IsEmpty))
				{
					var deliveryInfo = new TransportBookingPickupDeliveryInfo();
					deliveryInfo.Type = "DeliveryTo";
					deliveryInfo.Packages = instruction.Packages?.Where(p => p.KP_F3_NKPackType == Constants.ContainerModes.Containerised);
					deliveryInfo.Address = AddressBuilder.Create(context, instruction.Address);
					deliveryInfo.AddressETD = instruction.EstimatedDate(false);
					infos.Add(deliveryInfo);
				}
			}

			return infos;
		}

		protected override void PopulateAdditionalData(CarrierMessageData wrapper)
		{
			base.PopulateAdditionalData(wrapper);
			PopulatePorts(wrapper);

			wrapper.AddValidationDependencies(wrapper.EstCargoPickupDateTimeInfo, wrapper.IsDoorPickupInfo);

			wrapper.EstCargoPickupDateTime = consol.Containers.Cast<ForwardingContainer>().Select(conatainer => conatainer.JC_DepartureEstimatedPickup).Where(time => time.IsValid).OrderBy(time => time).FirstOrDefault();
			wrapper.EstCargoPickupDateTimeInfo.AddMessageError(() => (wrapper.IsCoload || wrapper.IsNVO) && wrapper.ContainerMode.Code == Constants.ContainerModes.LCL && wrapper.IsDoorPickup && !wrapper.EstCargoPickupDateTime.IsValid, (NoResString)"Est. Cargo Pickup Date Time is required."); // non-translatable registration number
		}

		void PopulatePorts(CarrierMessageData wrapper)
		{
			wrapper.OperationalPort = new Unloco(context.Factory, context.Unlocos, context.Countries)
			{
				Code = consol.JK_RL_NKLoadPort
			}.WithCustomNameProvider(GetDetailedPortName);
			wrapper.FreightPayableAt = new Unloco(context.Factory, context.Unlocos, context.Countries)
			{
				Code = wrapper.FreightPayer?.Unloco != null && !wrapper.FreightPayer.Unloco.Code.IsEmpty
					? wrapper.FreightPayer.Unloco.Code
					: consol.JK_PrepaidCollect == Constants.PaymentType.Collect
						? consol.JK_RL_NKDischargePort
						: consol.JK_RL_NKLoadPort
			}.WithCustomNameProvider(GetDetailedPortName);
		}

		protected override void AddValidation(CarrierMessageData wrapper)
		{
			base.AddValidation(wrapper);
			AddGeneralValidation(wrapper);
			AddTransportsValidation(wrapper);
			AddPackingLinesValidation(wrapper);
			AddVoyageValidation(wrapper);
			AddCarrierBookingReferenceValidation(wrapper);
			AddCarrierBookingOfficeValidation(wrapper);
			AddDepartureAndDeliveryDatesValidation(wrapper);
			AddShippingLineMessagingRequirementsValidation(wrapper, x => x.RSR_IsBookingRequest);
			AddTransportBookingPickupDeliveryInfosValidation(wrapper);
			AddPortsValidation(wrapper);
		}

		void AddPortsValidation(CarrierMessageData wrapper)
		{
			var enterValidUnlocoMessageError = Res.GetString("5828DA6B-BCC0-4267-80F6-E52F7473C08D", "You have not entered a valid un loco.");

			wrapper.OperationalPort?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("6D33FBC1-FF07-4CA1-B31B-6F488C161DBE", "Operational Port is required."));
			wrapper.OperationalPort?.CodeInfo.AddAsciiCharactersValidation();
			wrapper.OperationalPort?.CodeInfo.AddInvalidCodeValidation(enterValidUnlocoMessageError);

			wrapper.FreightPayableAt?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("65AC4CF8-7C5E-4B50-9E43-3136B5FD5FC7", "Freight Payable At is required."));
			wrapper.FreightPayableAt?.CodeInfo.AddAsciiCharactersValidation();
			wrapper.FreightPayableAt?.CodeInfo.AddInvalidCodeValidation(enterValidUnlocoMessageError);
		}

		void AddGeneralValidation(CarrierMessageData wrapper)
		{
			wrapper.ErrorPlaceHolderInfo.AddMessageError(() => wrapper.IsDirect && !wrapper.HasShipments, Res.GetString("33863401-AB3E-46FC-9D88-A4318192B511", "At least one Shipment must exist to send booking request when Consol type is Direct."));
			wrapper.ErrorPlaceHolderInfo.AddMessageError(() => wrapper.Containers.Any(c => c.IsEmpty), Res.GetString("E736554E-5B1A-4B7B-B62C-A98EBD3E6BF5", "Carrier does not support booking requests for empty containers."));

			wrapper.ErrorPlaceHolderInfo.AddMessageError(() => ((wrapper.IsCoload && consol.CreditorIsNVOCC) || (!wrapper.IsCoload && consol.ShippingLineIsNVOCC)) && !wrapper.HasShipments, Res.GetString("C1B61EC5-62EC-4AAF-A28A-BF26A5F7B435", "The Booking Request cannot be sent to NVOCC when there is no cargo details. Please attach at least one Shipment to the Consolidation."));
		}

		void AddTransportBookingPickupDeliveryInfosValidation(CarrierMessageData wrapper)
		{
			if (wrapper.Containers.Any(c => c.TransportBookingPickupDeliveryInfos?.Any(info => info.Type == "PickupFrom") ?? false))
			{
				var originalIsDoorPickup = wrapper.IsDoorPickup;
				wrapper.IsDoorPickupInfo.AddMessageError(() => wrapper.IsDoorPickup != originalIsDoorPickup && wrapper.IsDoorPickup, Res.GetString("F2247A66-526E-409E-A369-B6F74C3D24DA", "There are Pickup Transport Booking(s) available, please update Delivery mode via Consol > Containers > Delivery Mode field to default pickup addresses from transport booking(s)."));
				wrapper.IsDoorPickupInfo.AddMessageError(() => wrapper.IsDoorPickup != originalIsDoorPickup && !wrapper.IsDoorPickup, Res.GetString("78C5CB1F-C85D-4DCB-9948-2CBC5A944751", "The pickup addresses were defaulted from transport booking(s), please update Delivery mode via Consol > Containers > Delivery Mode field to remove pickup addresses from booking request."));
			}

			if (wrapper.Containers.Any(c => c.TransportBookingPickupDeliveryInfos?.Any(info => info.Type == "DeliveryTo") ?? false))
			{
				var originalIsDoorDelivery = wrapper.IsDoorDelivery;
				wrapper.IsDoorDeliveryInfo.AddMessageError(() => wrapper.IsDoorDelivery != originalIsDoorDelivery && wrapper.IsDoorDelivery, Res.GetString("E9950F98-49C4-494F-829C-AA611913C752", "There are Delivery Transport Booking(s) available, please update Delivery mode via Consol > Containers > Delivery Mode field to default delivery addresses from transport booking(s)."));
				wrapper.IsDoorDeliveryInfo.AddMessageError(() => wrapper.IsDoorDelivery != originalIsDoorDelivery && !wrapper.IsDoorDelivery, Res.GetString("A1DBC0CC-14A2-4602-B74C-7FB370BDE8A9", "The delivery addresses were defaulted from transport booking(s), please update Delivery mode via Consol > Containers > Delivery Mode field to remove delivery addresses from booking request."));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable registration number")]
		protected override void AddContainerValidation(CarrierMessageData wrapper, Container containerDO)
		{
			base.AddContainerValidation(wrapper, containerDO);

			var isContainerized = wrapper.ContainerMode.Code == Core.Constants.ContainerModes.LCL || wrapper.ContainerMode.Code == Core.Constants.ContainerModes.FCL;
			containerDO.NumberInfo.AddMessageError(() => consol.IsDirect && isContainerized && !containerDO.IsEmpty && !containerDO.PackingLines.Any(), Res.GetString("FFC83517-6A46-4116-9588-75F3E6914A54", "There are no packs in this container. Please pack this container."));

			if (!wrapper.HasShipments && !wrapper.IsDirect)
			{
				var commodityDescription = containerDO.Commodity?.Description.ToUpper().ToString() ?? string.Empty;
				var commodity = !commodityDescription.IsNullOrEmpty() ? commodityDescription : "Freight of All Kind";
				containerDO.PackCountInfo.AddWarning(() => true, ZString.Format((NoResString)"There are no shipments allocated to the container/s.\r\nIf you are to proceed with this Booking Request, 1 unit of {0} will be reported to the carriers.\r\nBooking Request replacement message can be sent later if required, when correct values are known.", commodity));
			}

			containerDO.PackCountInfo.AddMessageError(
				() => !wrapper.IsNonContainerized
					&& wrapper.ContainerMode.Code != Constants.ContainerModes.LCL
					&& !consol.IsCoLoad && !wrapper.IsNVO
					&& wrapper.Shipments.Any()
					&& !containerDO.IsEmpty
					&& (containerDO.PackCount == 0
						|| (IsGroupAndConsolidatePackingLines && packageGroupingHelper.HasPackLinesWithEmptyContainerNumberAndInvalidQuantity)
						|| (!IsGroupAndConsolidatePackingLines && wrapper.Shipments.SelectMany(x => x.AllPackingLinesIncludeCoLoad).Any(packingLine => packingLine.Quantity <= 0 && packingLine.ContainerNumber.IsEmpty)))
				, Res.GetString("0CA3BCB2-0C07-4EB3-82A9-404854856DB7", "You have not entered a value or there are unpacked packings with 0 quantity. If this is intended, please flag the container as empty."));

			containerDO.SetTemperature.ValueInfo.AddMessageError(() =>
				{
					var temperatureValue = containerDO.SetTemperature?.Value ?? 0m;
					return temperatureValue.IsInteger ? !temperatureValue.IsInRange(-999, 999) : !temperatureValue.IsInRange(-99.9, 99.9);
				},
				"Maximum three digits are allowed for temperature in Booking Request, when integer +-999, or when decimal +-99.9.\r\nPlease change temperature in Consol>Containers>Refrigeration.");
		}

		void AddTransportsValidation(CarrierMessageData wrapper)
		{
			wrapper.ErrorPlaceHolderInfo.AddMessageError(() => wrapper.Transports.Any(t => t.PortOfLoading.IsEmpty()), Res.GetString("7d25760b-8110-5b96-4f04-1a2076d381c6", "Port of Load is required in all Transport Legs."));
			wrapper.ErrorPlaceHolderInfo.AddMessageError(() => wrapper.Transports.Any(t => t.PortOfDischarge.IsEmpty()), Res.GetString("e362981a-ae38-37be-4e76-90eabd5f54b5", "Port of Discharge is required in all Transport Legs."));
		}

		void AddPackingLinesValidation(CarrierMessageData wrapper)
		{
			var totalContainers = consol.Containers.Count;
			var totalContainerizedPackingLines = totalContainers <= 0 ? 0 : consol.Containers.OfType<ForwardingContainer>().SelectMany(x => x.PackLines.OfType<PackLine>()).Count();
			var totalPackingLines = consol.Shipments.OfType<ForwardingShipment>().SelectMany(x => x.OuterPackLines).Count();

			wrapper.ErrorPlaceHolderInfo.AddMessageError(() => wrapper.MustHaveContainerPackline && totalContainers + totalContainerizedPackingLines <= 0, (NoResString)"Container and Packing Lines details are required for Booking Request and Amendment messages."); // non-translatable validation message
			wrapper.ErrorPlaceHolderInfo.AddMessageError(() => (!wrapper.IsNonContainerized && totalContainerizedPackingLines > 999) || (wrapper.IsNonContainerized && @totalPackingLines > 999), (NoResString)"Maximum 999 packlines can be included in a Booking Request message."); // non-translatable validation message
			wrapper.ErrorPlaceHolderInfo.AddMessageError(() => wrapper.IsNonContainerized && @totalPackingLines <= 0, (NoResString)"Packing Lines details are required for Booking Request and Amendment messages."); // non-translatable validation message
		}

		void AddVoyageValidation(CarrierMessageData wrapper)
		{
			var cantSendMessageError = (NoResString)@"The Booking Request can only be sent if the following data is entered:
1. Place of Receipt and Earliest Departure; OR 
2. Place of Delivery  and Latest Delivery; OR
3. Vessel Name (or Lloyds Code) and Voyage Number."; // non-translatable validation message

			wrapper.ErrorPlaceHolderInfo.AddMessageError(() =>
			{
				var mainTransport = wrapper.Transports.Main;
				var vessel = mainTransport?.Vessel;
				var isMainTransportInfoInvalid = mainTransport == null
										|| vessel == null
										|| (vessel.Name.IsEmpty && vessel.LloydsIMO.IsEmpty)
										|| mainTransport.VoyageFlightNumber.IsEmpty;

				return isMainTransportInfoInvalid
					&& (wrapper.PlaceOfReceipt.IsEmpty() || wrapper.EarliestDepartureDate.IsEmpty)
					&& (wrapper.PlaceOfDelivery.IsEmpty() || wrapper.LatestDeliveryDate.IsEmpty);
			}, cantSendMessageError);

			wrapper.AddValidationDependencies(wrapper.ErrorPlaceHolderInfo, wrapper.PlaceOfReceipt.CodeInfo, wrapper.PlaceOfDelivery.CodeInfo, wrapper.EarliestDepartureDateInfo, wrapper.LatestDeliveryDateInfo);

			if (wrapper.Transports.Main?.Vessel != null)
			{
				wrapper.AddValidationDependencies(wrapper.ErrorPlaceHolderInfo, wrapper.Transports.Main.Vessel.NameInfo, wrapper.Transports.Main.Vessel.LloydsIMOInfo, wrapper.Transports.Main.VoyageFlightNumberInfo);
			}
		}

		void AddCarrierBookingReferenceValidation(CarrierMessageData wrapper)
		{
			var carrierBookingReferenceShouldBeEmptyIfFirstTimeMessageWarning = Res.GetString("a87e7ee5-aa14-4ed1-851c-5c68d5c48d67", @"Carrier Booking Request Number is populated during booking confirmation.
If you need to send Carrier Booking Number at the time of a Booking Request, please ensure this is the number pre-assigned by the carrier in advance.");

			bool IsApplicable(StmALog log)
			{
				switch (log.SL_SE_NKEvent)
				{
					case Events.MessageSentCode:
					case Events.StatusUpdatedCode:
						return true;
					default:
						return false;
				}
			}

			bool IsThisMessageBeingSentForTheFirstTime()
			{
				var relevantLogs = logProvider?
					.Logs?
					.GetAllLogs()
					.OfType<StmALog>()
					.OrderBy(log => log.SL_PostedTimeUtc)
					.Where(log => IsApplicable(log))
					.ToArray();

				var state = MessageState.NotSent;

				if (relevantLogs != null && relevantLogs.Any())
				{
					foreach (var log in relevantLogs)
					{
						switch (log.SL_SE_NKEvent)
						{
							case Events.MessageSentCode:
								state = MessageState.OriginalSent;
								break;
							case Events.StatusUpdatedCode:
								state = MessageState.NotSent;
								break;
						}
					}
				}
				return (state == MessageState.NotSent);
			}

			wrapper.BookingReferenceInfo.AddWarning(() =>
				!string.IsNullOrEmpty(wrapper.BookingReference) && IsThisMessageBeingSentForTheFirstTime(),
				carrierBookingReferenceShouldBeEmptyIfFirstTimeMessageWarning);
		}

		void AddCarrierBookingOfficeValidation(CarrierMessageData wrapper)
		{
			wrapper.CarrierBookingOffice.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("DB9E50DD-0DD1-4E9E-9CE6-D2EFF6A26BC8", "The Carrier Booking Office is mandatory.\r\nPlease provide it on Consol > Details > Docs > Carrier Booking Office."));

			wrapper.CarrierBookingOffice.CodeInfo.AddWarning(() =>
					!wrapper.CarrierBookingOffice.Code.IsEmpty
					&& wrapper.CarrierBookingOffice.Code.SubstringSafe(0, 2) != wrapper.PlaceOfReceipt.Code.SubstringSafe(0, 2)
					&& wrapper.CarrierBookingOffice.Code.SubstringSafe(0, 2) != wrapper.PortOfLoading.Code.SubstringSafe(0, 2),
					Res.GetString("471a9dc1-51c1-47b7-985a-4a1ad48283cc", "Carrier booking office does not match to UNLOCO or country code of Place of Receipt/Port of Loading.\r\nPlease provide a valid Carrier Booking Office to avoid booking rejection by carrier."));

			wrapper.CarrierBookingOffice.AddValidationDependencies(wrapper.CarrierBookingOffice.CodeInfo, wrapper.PlaceOfReceipt.CodeInfo, wrapper.PortOfLoading.CodeInfo);
		}

		void AddDepartureAndDeliveryDatesValidation(CarrierMessageData wrapper)
		{
			var errorMessageForETDETA = Res.GetString("67e72829-01f1-4ae7-b746-514ed1cb1f63", "ETD/ETA must not be more than 400 days in advance.");
			var errorMessageForEarliestDeparture = Res.GetString("62a8b3f4-dc66-41c3-9478-ef412fb533cd", "Earlier Departure must not be more than 400 days in advance.");
			var errorMessageForLatestDelivery = Res.GetString("e38a2c5a-bf71-4140-n27e-21038bf3a69a", "Latest Delivery must not be more than 400 days in advance.");

			var advanceDateTimeLimit = ZDateTime.Now.AddDays(400);

			foreach (Transport transport in wrapper.Transports)
			{
				transport.ETAInfo.AddMessageError(() =>
					transport.ETA > advanceDateTimeLimit,
					errorMessageForETDETA);
				transport.ETDInfo.AddMessageError(() =>
					transport.ETD > advanceDateTimeLimit,
					errorMessageForETDETA);
			}

			wrapper.EarliestDepartureDateInfo.AddMessageError(() =>
				wrapper.EarliestDepartureDate > advanceDateTimeLimit,
				errorMessageForEarliestDeparture);
			wrapper.LatestDeliveryDateInfo.AddMessageError(() =>
				wrapper.LatestDeliveryDate > advanceDateTimeLimit,
				errorMessageForLatestDelivery);
		}

		protected override void AddPackingLineValidation(CarrierMessageData wrapper, PackingLine packingLine)
		{
			base.AddPackingLineValidation(wrapper, packingLine);

			var harmonizedCode = packingLine.HarmonizedCode as HarmonizedCode;
			var importHarmonizedCode = packingLine.ImportHarmonizedCode as HarmonizedCode;
			var exportHarmonizedCode = packingLine.ExportHarmonizedCode as HarmonizedCode;
			var harmonizedCodesHaveMessageWarnings = false;

			if ((importHarmonizedCode?.Country?.Code ?? ZString.Empty) != (exportHarmonizedCode?.Country?.Code ?? ZString.Empty))
			{
				if ((importHarmonizedCode?.Country?.Code ?? ZString.Empty) == Core.Constants.CountryCodes.Malaysia)
				{
					harmonizedCode.CodeInfo.AddWarning(() =>
					{
						if (harmonizedCode.Code.IsEmpty && importHarmonizedCode.Code.IsEmpty)
						{
							return harmonizedCodesHaveMessageWarnings = true;
						}
						else
						{
							return false;
						}
					}, (NoResString)"It is recommended to fill in Harmonized Code to assist with faster booking and reconciliation processes for Sea exports and imports to Malaysia."); // non-translatable validation message
				}
				else if ((exportHarmonizedCode?.Country?.Code ?? ZString.Empty) == Core.Constants.CountryCodes.Malaysia)
				{
					harmonizedCode.CodeInfo.AddWarning(() =>
					{
						if (harmonizedCode.Code.IsEmpty && exportHarmonizedCode.Code.IsEmpty)
						{
							return harmonizedCodesHaveMessageWarnings = true;
						}
						else
						{
							return false;
						}
					}, (NoResString)"It is recommended to fill in Harmonized Code to assist with faster booking and reconciliation processes for Sea exports and imports from Malaysia."); // non-translatable validation message
				}
			}

			harmonizedCode?.CodeInfo.AddWarning(() => !harmonizedCodesHaveMessageWarnings && string.IsNullOrWhiteSpace(harmonizedCode?.Code) && string.IsNullOrWhiteSpace(importHarmonizedCode?.Code) && string.IsNullOrWhiteSpace(exportHarmonizedCode?.Code),
(NoResString)"It is recommended to fill in Harmonized Code to assist with faster booking and reconciliation processes."); // non-translatable validation message.
		}

		#region Charges Validation

		protected override void AddChargesValidation(CarrierMessageData wrapper)
		{
			base.AddChargesValidation(wrapper);

			LinkChargesValue(wrapper);
		}

		void LinkChargesValue(CarrierMessageData wrapper)
		{
			var optionalChargeBasicFreight = (OptionalCharge)wrapper.OptionalChargeBasicFreight;

			optionalChargeBasicFreight.IsPayableElsewhereInfo.ValueChanged += (s, e) =>
			{
				wrapper.IsFreightAsAgreed = optionalChargeBasicFreight.IsPayableElsewhere;
			};
		}

		#endregion

		#region CarrierMessagingRequirements

		protected override ZBool IsHarmonizedCodeMandatory() => GetMessagingRequirement(ShippingLineMessagingRequirement.Types.HarmonisedCode)?.RSR_IsBookingRequest ?? false;

		protected override ZBool IsElectronicBillOfLadingProviderMandatory(CarrierMessageData wrapper) => GetMessagingRequirement(ShippingLineMessagingRequirement.Types.BillOfLadingProvider)?.RSR_IsBookingRequest ?? false;

		protected override ZBool GetIsRequiredSendAttachment() => GetMessagingRequirement(ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage)?.RSR_IsBookingRequest ?? false;

		protected override ZBool CarrierHasIntegrationViaEmailToCarrierLocalOfficeTag() => GetMessagingRequirement(ShippingLineMessagingRequirement.Types.IntegrationViaEmailToCarrierLocalOffice)?.RSR_IsBookingRequest ?? false;

		protected override void AddContainersSealNumberValidation(CarrierMessageData wrapper)
		{
		}

		protected override string IelMessageRequirementValidationContactErrorMessage => (NoResString)"This carrier only supports integration via email to local office.\r\nContact name and email address are required to send Booking Request.\r\nPlease maintain contact name and email address in carrier Organization > Contact > Email and Receiving Documents > Group SHP.";

		#endregion
	}
}
