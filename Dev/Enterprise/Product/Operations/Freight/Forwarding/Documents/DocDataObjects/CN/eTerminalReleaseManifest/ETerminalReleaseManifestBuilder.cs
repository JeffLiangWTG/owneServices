using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	sealed class ETerminalReleaseManifestBuilder
	{
		public ETerminalReleaseManifestBuilder(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
			this.context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());
		}
		readonly IContext context;
		readonly ForwardingConsol consol;

		public ETerminalReleaseManifest Build()
		{
			var eTerminalReleaseManifest = new ETerminalReleaseManifest(
				nameof(ForwardingConsol),
				consol.JK_UniqueConsignRef,
				DataContext.ETerminalReleaseManifest);

			var directShipment = consol.DirectShipment;
			consol.Transports.Sort(MovementLegComparer.PortsAndDatesBased(consol.Transports));

			PopulateConsolData(eTerminalReleaseManifest);
			PopulateAddresses(eTerminalReleaseManifest, directShipment);
			PopulateCharges(eTerminalReleaseManifest);
			PopulateRoutingPorts(eTerminalReleaseManifest);
			PopulateReferenceNumbers(eTerminalReleaseManifest, directShipment);
			PopulateContainerAndGoodsDetails(eTerminalReleaseManifest);

			EnsureThatDateofIssueAndPortOfIssueAreValidatedWhenEitherChanges(eTerminalReleaseManifest);
			eTerminalReleaseManifest.ValidateAllIncludingChildren();

			return eTerminalReleaseManifest;
		}

		#region Consol Data

		void PopulateConsolData(ETerminalReleaseManifest eTerminalReleaseManifest)
		{
			var releaseTypes = new ChinaReleaseTypes();

			var seaWayBillCodes = new[]
			{
				Core.Constants.ShipmentReleaseTypes.SeaWaybill,
				Core.Constants.ShipmentReleaseTypes.ExpressBofL,
				Core.Constants.ShipmentReleaseTypes.NonNegotiable
			};

			var isSeawaybill = seaWayBillCodes.Any(code => String.CompareOrdinal(code, consol.JK_ReleaseType) == 0);

			eTerminalReleaseManifest.ReleaseType = new CodeDescription(releaseTypes)
			{
				Code = isSeawaybill
					? ShippingInstructionReleaseTypes.Codes.SeaWaybill
					: ShippingInstructionReleaseTypes.Codes.BOLOriginal
			};

			if (!isSeawaybill)
			{
				eTerminalReleaseManifest.NumberOfOriginals = consol.JK_NoOriginalBills;
			}
			eTerminalReleaseManifest.NumberOfCopies = consol.JK_NoCopyBills;

			eTerminalReleaseManifest.RequestedDateOfIssue = consol.JK_MasterBillIssueDate;
			eTerminalReleaseManifest.RequestedDateOfIssueInfo.AddMessageError(() => !eTerminalReleaseManifest.PlaceOfIssue.Code.IsEmpty && eTerminalReleaseManifest.RequestedDateOfIssue.IsEmpty,
				(NoResString)"Date of Issue is required if Place of Issue is entered."); // non-translatable validation message

			const string cfs = "CFS";

			eTerminalReleaseManifest.IsDoorPickup = consol
				.Containers
				.OfType<CommonContainer>()
				.Any(container => container.JC_DeliveryMode.StartsWith(cfs, StringComparison.OrdinalIgnoreCase));

			eTerminalReleaseManifest.IsDoorDelivery = consol
				.Containers
				.OfType<CommonContainer>()
				.Any(container => container.JC_DeliveryMode.EndsWith(cfs, StringComparison.OrdinalIgnoreCase));

			eTerminalReleaseManifest.ContainerMode = new CodeDescription(consol.JK_ConsolMode_List)
			{
				Code = GetContainerModeCode()
			};

			eTerminalReleaseManifest.SpecialInstructions = GetNoteText(consol, PredefinedNoteTypes.Instance.SpecialInstructions.Description) ?? ZString.Empty;
			eTerminalReleaseManifest.SpecialInstructionsInfo.AddAsciiCharactersValidation();
		}

		string GetContainerModeCode()
		{
			switch (consol.JK_ConsolMode)
			{
				case Core.Constants.ContainerModes.FCL:
				case Core.Constants.ContainerModes.Groupage:
				case Core.Constants.ContainerModes.BuyersConsol:
				case Core.Constants.ContainerModes.ShippersConsol:
					return Core.Constants.ContainerModes.FCL;
				case Core.Constants.ContainerModes.Other:
					return consol.Containers.Any()
						? Core.Constants.ContainerModes.FCL
						: Core.Constants.ContainerModes.LCL;

				default:
					return consol.JK_ConsolMode;
			}
		}

		ZString? GetNoteText(IStmNoteParent parent, string description)
		{
			return parent
				?.Notes
				.FindByDescription(description)
				.FirstOrDefault()
				?.ST_NoteDataAsText;
		}

		#endregion

		#region Addresses

		void PopulateAddresses(ETerminalReleaseManifest eTerminalReleaseManifest, ForwardingShipment directShipment)
		{
			eTerminalReleaseManifest.Shipper = (consol.IsDirect
					? AddressBuilder.Create(context, directShipment?.ConsignorDocumentaryAddress).AddAsAgentInfoToCompanyName(directShipment?.ConsignorDocumentaryAddress)
					: AddressBuilder.Create(context, consol.SendingForwarderWithContact).AddAsAgentInfoToCompanyName(consol.SendingForwarderAddress))
				.AddPartyNameAndAddressValidation((NoResString)"Shipper") // non-translatable validation message
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation();

			eTerminalReleaseManifest.Carrier = AddressBuilder.Create(context, consol.ShippingLineAddress);
			var hasScacNumber = eTerminalReleaseManifest.Carrier.HasRegistrationNumber(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.CodeTypes.CarrierCode);
			eTerminalReleaseManifest.Carrier.AddPartyNameAndAddressValidation((NoResString)"Carrier") // non-translatable validation message
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation()
				.CompanyNameInfo.AddMessageError(() => !hasScacNumber, (NoResString)"Carrier SCAC is missing from Carrier organization record under Organisation > Config > Country US, Type CCC."); // non-translatable validation message

			eTerminalReleaseManifest.Consignee = (consol.IsDirect
					? AddressBuilder.Create(context, directShipment?.ConsigneeDocumentaryAddress)
					: AddressBuilder.Create(context, consol.ReceivingForwarderWithContact))
				.AddPartyNameAndAddressValidation((NoResString)"Consignee", () => !eTerminalReleaseManifest.Consignee.IsToOrder()) // non-translatable validation message
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation()
				.AddToOrderSupport();

			eTerminalReleaseManifest.NotifyParty = (consol.IsDirect
					? AddressBuilder.Create(context, directShipment?.NotifyPartyDocumentaryAddress)
					: AddressBuilder.Create(context, consol.NotifyPartyDocumentaryAddress))
				.AddPartyNameAndAddressValidation((NoResString)"Notify Party", () => !eTerminalReleaseManifest.NotifyParty.IsSameAsConsignee()) // non-translatable validation message
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation()
				.AddSameAsConsigneeSupport()
				.AddPostcodeValidationForUSImports((NoResString)"Notify Party"); // non-translatable validation message;

			eTerminalReleaseManifest.NotifyParty2 = (consol.IsDirect
					? AddressBuilder.Create(context, directShipment?.NotifyParty2DocumentaryAddress)
					: AddressBuilder.Create(context, consol.NotifyParty2DocumentaryAddress))
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation();

			eTerminalReleaseManifest.Forwarder = AddressBuilder.Create(context, consol.SendingForwarderWithContact)
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation();

			eTerminalReleaseManifest.CurrentUser = AddressBuilder.CreateForCurrentUser(context)
				.AddAsciiCharactersValidation();
		}

		#endregion

		#region PopulateRoutingPorts

		void PopulateRoutingPorts(ETerminalReleaseManifest eTerminalReleaseManifest)
		{
			eTerminalReleaseManifest.Transports = Transports.Create(context, consol.Transports?.OfType<Freight.Business.Transport>());
			AddTransportsValidation(eTerminalReleaseManifest);

			eTerminalReleaseManifest.PlaceOfReceipt = Unloco.Create(context, consol.LoadPort)
				.AddAsciiCharactersValidation();

			eTerminalReleaseManifest.PlaceOfIssue = Unloco.Create(context, consol.MasterBillIssuePlace)
				.AddAsciiCharactersValidation();
			eTerminalReleaseManifest.PlaceOfIssue.CodeInfo.AddMessageErrorIfEmpty((NoResString)"Place of Issue is required."); // non-translatable validation message

			eTerminalReleaseManifest.PlaceOfDelivery = Unloco.Create(context, consol.DischargePort)
				.AddAsciiCharactersValidation();
			eTerminalReleaseManifest.OperationalPort = consol.GetOperationalPort(context);

			var firstSeaLeg = consol.Transports?.FirstTransportWithTransportMode(Core.Constants.TransportModes.Sea);
			var lastSeaLeg = consol.Transports?.LastTransportWithTransportMode(Core.Constants.TransportModes.Sea);

			eTerminalReleaseManifest.PortOfLoad = Unloco.Create(context, firstSeaLeg?.LoadPort)
				.AddAsciiCharactersValidation();

			eTerminalReleaseManifest.PortOfDischarge = Unloco.Create(context, lastSeaLeg?.DiscPort)
				.AddAsciiCharactersValidation();

			eTerminalReleaseManifest.PortOfDischarge.CodeInfo.AddMessageErrorIfEmpty((NoResString)"Port of Discharge is required."); // non-translatable validation message

			eTerminalReleaseManifest.PortOfDischarge.CodeInfo.ValueChanged += DefaultAndValidateImportHarmonizedCode;

			void DefaultAndValidateImportHarmonizedCode(object sender, EventArgs args)
			{
				foreach (var packingLine in eTerminalReleaseManifest.Containers.SelectMany(c => c.PackingLines).OfType<PackingLine>())
				{
					packingLine.ImportHarmonizedCode = PackingLineBuilder.GetHarmonizedCodesDesc(consol.Factory, packingLine, eTerminalReleaseManifest.PortOfDischarge.Code, context);
				}
			}

			var freightPayableAt = eTerminalReleaseManifest.IsFreightPrepaid ? consol.LoadPort : (eTerminalReleaseManifest.IsFreightCollect ? consol.DischargePort : null);
			eTerminalReleaseManifest.FreightPayableAt = Unloco.Create(context, freightPayableAt)
				.AddAsciiCharactersValidation();
			eTerminalReleaseManifest.FreightPayableAt.CodeInfo.AddMessageErrorIfEmpty((NoResString)"The location of where freight is paid is required."); // non-translatable validation message
		}

		void AddTransportsValidation(ETerminalReleaseManifest eTerminalReleaseManifest)
		{
			foreach (var transport in eTerminalReleaseManifest.Transports.Collection)
			{
				transport.Vessel.NameInfo.AddMessageError(() => transport.Mode.Code == Core.Constants.TransportModes.Sea && string.IsNullOrWhiteSpace(transport.Vessel.Name), (NoResString)"Vessel Name is required."); // non-translatable validation message
				transport.Vessel.LloydsIMOInfo.AddMessageError(() => transport.Mode.Code == Core.Constants.TransportModes.Sea && string.IsNullOrWhiteSpace(transport.Vessel.LloydsIMO), (NoResString)"Vessel's Lloyds/IMO is mandatory."); // non-translatable validation message
				transport.VoyageFlightNumberInfo.AddMessageError(() => transport.Mode.Code == Core.Constants.TransportModes.Sea && string.IsNullOrWhiteSpace(transport.VoyageFlightNumber), (NoResString)"Voyage is required."); // non-translatable validation message
				transport.PortOfDischarge.CodeInfo.AddMessageErrorIfEmpty((NoResString)"Port of Discharge is required."); // non-translatable validation message
				transport.VoyageFlightNumberInfo.AddAsciiCharactersValidation();
				transport.Vessel.NameInfo.AddAsciiCharactersValidation();
				transport.Carrier.AddAsciiCharactersValidation();
			}

			eTerminalReleaseManifest.ErrorPlaceHolderInfo.AddMessageError(() => !eTerminalReleaseManifest.Transports.Collection.Any(t => t.Mode.Code == Core.Constants.TransportModes.Sea && t.PortOfLoading.Code.IsNingboPort()), Enterprise.Freight.Forwarding.Documents.DataObjects.Res.GetString("774840BD-A849-4E53-AB94-CAA9946A63E0", "eTerminal Release Manifest can only be sent when there is at least one sea leg Loading from Ningbo port."));
		}

		#endregion

		#region References

		void PopulateReferenceNumbers(ETerminalReleaseManifest eTerminalReleaseManifest, ForwardingShipment directShipment)
		{
			eTerminalReleaseManifest.BillOfLadingNumber = consol.JK_MasterBillNum;
			eTerminalReleaseManifest.CarrierBookingReference = consol.JK_BookingReference;
			eTerminalReleaseManifest.CarrierContractNumber = consol?.JK_CarrierContractNumber ?? ZString.Empty;
			eTerminalReleaseManifest.ShipperReference = directShipment?.JS_BookingReference ?? consol.JK_AgentsReference;
			eTerminalReleaseManifest.FreightForwarderReference = consol.JK_UniqueConsignRef;

			if (eTerminalReleaseManifest.CarrierBookingReference.IsEmpty || eTerminalReleaseManifest.BillOfLadingNumber.IsEmpty)
			{
				eTerminalReleaseManifest.UseBkgRefAsMasterSO = !eTerminalReleaseManifest.CarrierBookingReference.IsEmpty;
				eTerminalReleaseManifest.UseMasterBillAsMasterSO = !eTerminalReleaseManifest.BillOfLadingNumber.IsEmpty;
			}
			else if (eTerminalReleaseManifest.CarrierBookingReference == eTerminalReleaseManifest.BillOfLadingNumber)
			{
				eTerminalReleaseManifest.UseBkgRefAsMasterSO = true;
				eTerminalReleaseManifest.UseMasterBillAsMasterSO = false;
			}
			else
			{
				eTerminalReleaseManifest.UseBkgRefAsMasterSO = false;
				eTerminalReleaseManifest.UseMasterBillAsMasterSO = false;
			}

			AddMasterSOValidation(eTerminalReleaseManifest);
		}

		void AddMasterSOValidation(ETerminalReleaseManifest eTerminalReleaseManifest)
		{
			var errorMessage = (NoResString)"At least one of these checkboxes must be ticked to indicate which the carrier uses as the master SO#"; // non-translatable validation message

			eTerminalReleaseManifest.UseBkgRefAsMasterSOInfo.AddMessageError(() => !eTerminalReleaseManifest.UseBkgRefAsMasterSO && !eTerminalReleaseManifest.UseMasterBillAsMasterSO, errorMessage);
			eTerminalReleaseManifest.UseMasterBillAsMasterSOInfo.AddMessageError(() => !eTerminalReleaseManifest.UseBkgRefAsMasterSO && !eTerminalReleaseManifest.UseMasterBillAsMasterSO, errorMessage);

			eTerminalReleaseManifest.AddValidationDependencies(eTerminalReleaseManifest.UseMasterBillAsMasterSOInfo, eTerminalReleaseManifest.UseBkgRefAsMasterSOInfo);
			eTerminalReleaseManifest.AddValidationDependencies(eTerminalReleaseManifest.UseBkgRefAsMasterSOInfo, eTerminalReleaseManifest.UseMasterBillAsMasterSOInfo);
		}

		#endregion

		#region Charges

		void PopulateCharges(ETerminalReleaseManifest eTerminalReleaseManifest)
		{
			if (!consol.JK_PrepaidCollect.IsEmpty)
			{
				eTerminalReleaseManifest.IsFreightPrepaid = consol.JK_PrepaidCollect == Core.Constants.PaymentType.Prepaid;
				eTerminalReleaseManifest.IsFreightCollect = consol.JK_PrepaidCollect == Core.Constants.PaymentType.Collect;
			}

			var otherCharges = new OtherCharges
			{
				IsPrepaid = !consol.JK_PrepaidCollect.IsEmpty && eTerminalReleaseManifest.IsFreightPrepaid,
				IsCollect = !consol.JK_PrepaidCollect.IsEmpty && eTerminalReleaseManifest.IsFreightCollect
			};

			eTerminalReleaseManifest.OtherCharges = otherCharges;

			AddChargesValidation(eTerminalReleaseManifest);
		}

		void AddChargesValidation(ETerminalReleaseManifest eTerminalReleaseManifest)
		{
			var otherCharges = eTerminalReleaseManifest.OtherCharges;

			bool emptyPaymentDetails() => !otherCharges.IsPrepaid && !otherCharges.IsCollect && !otherCharges.IsFree && !otherCharges.IsPayableElsewhere && !otherCharges.IsFirstLinePrepaidLineSecondCollect && string.IsNullOrWhiteSpace(otherCharges.Remarks);

			var paymentDetailsErrorMessage = (NoResString)"At least one type must be selected for Payment Details."; // non-translatable validation message

			otherCharges.IsPrepaidInfo.AddMessageError(emptyPaymentDetails, paymentDetailsErrorMessage);
			otherCharges.IsCollectInfo.AddMessageError(emptyPaymentDetails, paymentDetailsErrorMessage);
			otherCharges.IsFreeInfo.AddMessageError(emptyPaymentDetails, paymentDetailsErrorMessage);
			otherCharges.IsPayableElsewhereInfo.AddMessageError(emptyPaymentDetails, paymentDetailsErrorMessage);
			otherCharges.IsFirstLinePrepaidLineSecondCollectInfo.AddMessageError(emptyPaymentDetails, paymentDetailsErrorMessage);
			otherCharges.RemarksInfo.AddMessageError(emptyPaymentDetails, paymentDetailsErrorMessage);
			otherCharges.RemarksInfo.AddAsciiCharactersValidation();

			var prepaidCollectErrorMessage = (NoResString)"Either Prepaid or Collect payment type must be selected."; // non-translatable validation message

			eTerminalReleaseManifest.IsFreightPrepaidInfo.AddMessageError(() => !eTerminalReleaseManifest.IsFreightPrepaid && !eTerminalReleaseManifest.IsFreightCollect, prepaidCollectErrorMessage);
			eTerminalReleaseManifest.IsFreightCollectInfo.AddMessageError(() => !eTerminalReleaseManifest.IsFreightPrepaid && !eTerminalReleaseManifest.IsFreightCollect, prepaidCollectErrorMessage);
		}

		#endregion

		#region Container And Goods Details

		void PopulateContainerAndGoodsDetails(ETerminalReleaseManifest eTerminalReleaseManifest)
		{
			var containerBuilder = new ContainerBuilder();
			var packlineBuilder = new PackingLineBuilder();

			var containerBizObjs = new Dictionary<ZGuid, CommonContainer>();
			var containerPackingLines = new Dictionary<ZGuid, List<PackingLine>>();

			var bookings = new Dictionary<ZString, Booking>();
			var bookingRelatedContainers = new Dictionary<ZString, HashSet<ZGuid>>();

			var packingLineList = new List<PackLine>();
			var shipments = consol.Shipments.OfType<ForwardingShipment>();

			eTerminalReleaseManifest.ErrorPlaceHolderInfo.AddMessageError(() => !shipments.Any(), (NoResString)"There are no shipments attached to the Consolidation"); // non-translatable validation message

			foreach (var shipmentBizObj in shipments)
			{
				packingLineList.AddRange(shipmentBizObj.OuterPackLines.Cast<PackLine>());
			}

			foreach (var packlineBizObj in packingLineList)
			{
				var containerBizObj = packlineBizObj.GetContainer(consol);

				if (containerBizObj == null)
				{
					continue;
				}

				if (!containerBizObjs.ContainsKey(containerBizObj.PK))
				{
					containerBizObjs.Add(containerBizObj.PK, containerBizObj);
				}

				var packingLine = packlineBuilder.Build(packlineBizObj);

				var bookingNumber = Helpers.GetBookingNumberWithFallback(packlineBizObj);
				packingLine.ExportReferenceNumber = bookingNumber;

				if (!containerPackingLines.TryGetValue(containerBizObj.PK, out List<PackingLine> packlines))
				{
					packlines = new List<PackingLine>();
					containerPackingLines.Add(containerBizObj.PK, packlines);
				}

				packlines.Add(packingLine);

				if (!bookings.TryGetValue(bookingNumber, out Booking booking))
				{
					booking = new Booking(bookingNumber)
					{
						BookingNumber = bookingNumber
					};

					bookings.Add(bookingNumber, booking);
				}

				if (!bookingRelatedContainers.TryGetValue(bookingNumber, out HashSet<ZGuid> bookingContainerIDs))
				{
					bookingContainerIDs = new HashSet<ZGuid>();
					bookingRelatedContainers.Add(bookingNumber, bookingContainerIDs);
				}

				bookingContainerIDs.Add(containerBizObj.PK);
			}

			var containers = new Dictionary<ZGuid, Container>();

			foreach (var containerPackingLine in containerPackingLines)
			{
				if (containerBizObjs.TryGetValue(containerPackingLine.Key, out CommonContainer containerBizObj))
				{
					var packingLines = containerPackingLine
						.Value
						.ToArray();

					var container = containerBuilder.Build(containerBizObj, context, packingLines);

					container.PackingLines = containerPackingLine
						.Value
						.ToArray();

					packingLines.ForEach(p => AddPackLineValidation(p));

					containers[containerBizObj.PK] = container;
				}
			}

			eTerminalReleaseManifest.Containers = containers.Values.ToArray();

			foreach (var bookingNumber in bookings.Keys)
			{
				if (!bookings.TryGetValue(bookingNumber, out Booking booking)
					|| !bookingRelatedContainers.TryGetValue(bookingNumber, out HashSet<ZGuid> bookingContainerIDs))
				{
					continue;
				}

				var bookingContainers = new List<BookingContainer>();

				foreach (var containerID in bookingContainerIDs)
				{
					if (containers.TryGetValue(containerID, out Container container))
					{
						var uniqueContainerID = $"{containerID}-{bookingNumber}"; // Non-translatable Identifier
						var bookingContainer = new BookingContainerBuilder(context, container, bookingNumber, uniqueContainerID).Build();
						AddContainerValidation(bookingContainer);
						bookingContainers.Add(bookingContainer);
					}
				}

				booking.Containers = bookingContainers;
				booking.BookingNumberInfo.AddMessageErrorIfEmpty((NoResString)"Shipping Order Number is required. Enter it on Shipment level (Reference Type SLD) or packs (Shipping Order/Shi Lian Dan Number)."); // non-translatable validation message
			}

			eTerminalReleaseManifest.Bookings = bookings.Values.ToArray();
			eTerminalReleaseManifest.ErrorPlaceHolderInfo.AddMessageError(() => !eTerminalReleaseManifest.Bookings.Any(), (NoResString)"Packs need to be allocated to containers."); // non-translatable validation message
		}

		void AddPackLineValidation(PackingLine packLine)
		{
			packLine.QuantityInfo.AddMessageError(() => packLine.Quantity == 0, (NoResString)"Pack count is mandatory."); // non-translatable validation message
			packLine.Weight.ValueInfo.AddMessageError(() => packLine.Weight.Value == 0, (NoResString)"Pack weight is mandatory."); // non-translatable validation message
			packLine.Volume.ValueInfo.AddMessageError(() => packLine.Volume.Value == 0, (NoResString)"Pack volume is mandatory."); // non-translatable validation message
			packLine.GoodsDescriptionInfo.AddMessageErrorIfEmpty((NoResString)"Goods Description is mandatory."); // non-translatable validation message
			packLine.MarksAndNumbersInfo.AddMessageErrorIfEmpty((NoResString)"Marks are mandatory."); // non-translatable validation message
			((HarmonizedCode)packLine.HarmonizedCode).CodeInfo.AddMessageErrorIfEmpty((NoResString)"Harmonised Code is required."); // non-translatable validation message

			foreach (var dg in packLine.DangerousGoods)
			{
				dg.Contact.FullNameInfo.AddMessageErrorIfEmpty((NoResString)"Contact Name is required for dangerous goods."); // non-translatable validation message
				dg.Contact.PhoneInfo.AddMessageErrorIfEmpty((NoResString)"Contact Phone is required for dangerous goods."); // non-translatable validation message
			}
		}

		void AddContainerValidation(BookingContainer bookingContainer)
		{
			bookingContainer.NumberInfo.AddMessageErrorIfEmpty((NoResString)"Please enter a Container Number."); // non-translatable validation message
			bookingContainer.SealInfo.AddMessageErrorIfEmpty((NoResString)"Seal number is mandatory."); // non-translatable validation message

			bookingContainer.Type.ISOCodeInfo.AddMessageErrorIfEmpty((NoResString)"Container type entered does not have a valid ISO code."); // non-translatable validation message
			bookingContainer.GoodsWeight.ValueInfo.AddMessageError(() => bookingContainer.GoodsWeight.Value == 0, (NoResString)"Cargo weight is mandatory."); // non-translatable validation message

			((CodeDescription)bookingContainer.SetTemperature?.Unit).CodeInfo.AddMessageError(() => !bookingContainer.IsNonOperativeReefer && (bookingContainer.Type?.Type?.Code ?? ZString.Empty) == Core.Constants.ContainerTypes.Refrigerated && string.IsNullOrWhiteSpace(bookingContainer.SetTemperature?.Unit?.Code), (NoResString)"Temperature is required when container is a reefer."); // non-translatable validation message
			((CodeDescription)bookingContainer.AirVentFlow?.Unit).CodeInfo.AddMessageError(() => !bookingContainer.IsNonOperativeReefer && (bookingContainer.Type?.Type?.Code ?? ZString.Empty) == Core.Constants.ContainerTypes.Refrigerated && string.IsNullOrWhiteSpace(bookingContainer.AirVentFlow?.Unit?.Code), (NoResString)"No Air Vent Setting measurement type exists. Ensure each temperature controlled container has one entered on the Containers > Refrigeration tab."); // non-translatable validation message
			bookingContainer.Type.Type.CodeInfo.ValueChanged += (e, a) =>
			{
				bookingContainer.SetTemperature.Validate(nameof(bookingContainer.SetTemperature.Value));
				bookingContainer.AirVentFlow.Validate(nameof(bookingContainer.AirVentFlow.Value));
			};
		}

		#endregion

		#region validation

		void EnsureThatDateofIssueAndPortOfIssueAreValidatedWhenEitherChanges(ETerminalReleaseManifest eTerminalReleaseManifest)
		{
			if (eTerminalReleaseManifest.PlaceOfIssue is Unloco placeOfIssue)
			{
				eTerminalReleaseManifest.OnValueChanged(nameof(eTerminalReleaseManifest.RequestedDateOfIssue)).Do(ValidateDateAndPlaceOfIssue);
				placeOfIssue.OnValueChanged(nameof(placeOfIssue.Code)).Do(ValidateDateAndPlaceOfIssue);

				void ValidateDateAndPlaceOfIssue()
				{
					eTerminalReleaseManifest.Validate(nameof(eTerminalReleaseManifest.RequestedDateOfIssue));
					placeOfIssue.Validate(nameof(placeOfIssue.Code));
				}
			}
		}

		#endregion
	}
}
