using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	sealed class EManifestBuilder
	{
		public EManifestBuilder(ForwardingShipment shipment, IDocDataObjectParameters parameters)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			this.logProvider = parameters?.LogProvider;
			this.context = new CommonContext(shipment.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingShipment shipment;
		readonly IContext context;
		readonly IStmALogProvider logProvider;

		public EManifest Build()
		{
			var consol = shipment.GetExportConsolFromChina();

			var eManifest = new EManifest(
				nameof(ForwardingShipment),
				shipment.JS_UniqueConsignRef,
				DataContext.EManifest);

			eManifest.Numbers = ReferenceNumber.Create(context, consol?.Numbers);
			eManifest.IsDirect = shipment.IsDirectShipment;
			eManifest.ShipmentType = new CodeDescription(consol?.JK_AgentType_List ?? new CodeDescriptionPairList())
			{
				Code = consol?.JK_AgentType ?? ZString.Empty
			};

			eManifest.IsRequiredSendAttachment = consol != null &&
				(OrgHeaderExtensions.GetShippingLineMessagingRequirement(consol.IsCoLoad ? consol.Creditor : consol.ShippingLine, ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage)?.RSR_IsEManifest ?? false);

			if (shipment.IsDirectShipment)
			{
				eManifest.NumberOfOriginals = shipment.JS_NoOriginalBills;
				eManifest.NumberOfCopies = shipment.JS_NoCopyBills;
			}
			else if (consol != null)
			{
				eManifest.NumberOfOriginals = consol.JK_NoOriginalBills;
				eManifest.NumberOfCopies = consol.JK_NoCopyBills;
			}
			eManifest.NumberOfOriginalsInfo.AddMessageError(() => eManifest.NumberOfOriginals == 0 && eManifest.NumberOfCopies == 0, (NoResString)"Copies/Originals are required."); // non-translatable validation message
			eManifest.NumberOfCopiesInfo.AddMessageError(() => eManifest.NumberOfOriginals == 0 && eManifest.NumberOfCopies == 0, (NoResString)"Copies/Originals are required."); // non-translatable validation message
			eManifest.RequestedDateOfIssue = consol?.JK_MasterBillIssueDate ?? ZDateTime.Empty;
			eManifest.RequestedDateOfIssueInfo.AddWarningIfEmpty((NoResString)"The Requested Date of Issue is required by some Handling Agents.\r\nIt is advisable to provide this value for faster processing."); // non-translatable validation message
			eManifest.SpecialInstructions = consol?
				.Notes
				.FindByDescription(PredefinedNoteTypes.Instance.SpecialInstructions.Description)
				.FirstOrDefault()?
				.ST_NoteDataAsText
				?? ZString.Empty;
			eManifest.SpecialInstructionsInfo.AddAsciiCharactersValidation();

			var fclModes = new[] {
				Core.Constants.ContainerModes.FCL,
				Core.Constants.ContainerModes.Groupage,
				Core.Constants.ContainerModes.BuyersConsol,
				Core.Constants.ContainerModes.ShippersConsol,
				Core.Constants.ContainerModes.Other
			};

			eManifest.ContainerMode = new CodeDescription(consol?.JK_ConsolMode_List ?? new CodeDescriptionPairList())
			{
				Code = fclModes.Contains<string>(consol?.JK_ConsolMode)
					? (ZString)Core.Constants.ContainerModes.FCL
					: (consol?.JK_ConsolMode ?? ZString.Empty)
			};

			var regionalSpecificValidateFunc = (consol?.JK_RL_NKLoadPort.IsNingboPort() ?? false)
				? (Func<ZPropertyInfo, Func<bool>, string, bool>)DocumentVisualizer.DocDataObjects.ValidationExtensions.AddMessageError
				: DocumentVisualizer.DocDataObjects.ValidationExtensions.AddWarning;

			eManifest.SendAllBookings = true;

			PopulateReleaseType(consol, eManifest);
			PopulateRouting(consol, eManifest);
			PopulateLocations(consol, eManifest);
			PopulateReferenceNumbers(eManifest, consol);
			PopulateAddresses(eManifest, consol);
			PopulatePickupDeliveryInfo(eManifest, consol);
			PopulateFreightCharges(eManifest, consol);
			PopulateOtherCharges(eManifest);
			PopulateGoodsDetails(eManifest, consol, regionalSpecificValidateFunc);
			PopulateTaxInfo(eManifest, consol);
			AddTaxInfoValidations(eManifest, regionalSpecificValidateFunc);

			AddSendingValidation(eManifest);

			eManifest.ValidateAllIncludingChildren();

			return eManifest;
		}

		void PopulateReleaseType(ForwardingConsol consol, EManifest eManifest)
		{
			var seaWyaBillCodes = new[]
			{
				Core.Constants.ShipmentReleaseTypes.SeaWaybill,
				Core.Constants.ShipmentReleaseTypes.ExpressBofL,
				Core.Constants.ShipmentReleaseTypes.NonNegotiable
			};

			var releaseType = shipment.IsDirectShipment ? shipment.JS_ReleaseType : consol?.JK_ReleaseType ?? string.Empty;
			var isSeawaybill = seaWyaBillCodes.Contains<string>(releaseType);

			var releaseTypeCodeDescription = new CodeDescription(new ChinaReleaseTypes())
			{
				Code = isSeawaybill
						? ShippingInstructionReleaseTypes.Codes.SeaWaybill
						: consol != null && consol.JK_AgentType == Core.Constants.AgentType.Agent && releaseType.IsEmpty
						? string.Empty
						: ShippingInstructionReleaseTypes.Codes.BOLOriginal
			};

			eManifest.ReleaseType = releaseTypeCodeDescription;
			releaseTypeCodeDescription.DescriptionInfo.AddMessageError(() => consol != null && consol.JK_AgentType == Core.Constants.AgentType.Agent && eManifest.ReleaseType.Code.IsEmpty, (NoResString)"Release Type is required."); // non-translatable validation message
		}

		void PopulateRouting(ForwardingConsol consol, EManifest eManifest)
		{
			consol?.Transports.Sort(MovementLegComparer.PortsAndDatesBased(consol?.Transports));
			var routing = consol?.Transports
				.OfType<Freight.Business.Transport>();

			var transports = Transports.Create(context, routing);
			AddTransportsValidation(transports);

			eManifest.Transports = transports;
			eManifest.OperationalPort = consol.GetOperationalPort(context)
				.AddAsciiCharactersValidation();
		}

		void AddTransportsValidation(Transports transports)
		{
			foreach (var transport in transports.Collection)
			{
				if (transport.Mode.Code == Core.Constants.TransportModes.Sea)
				{
					transport.Vessel.NameInfo.AddMessageError(() => transport.ETD.IsEmpty && string.IsNullOrWhiteSpace(transport.Vessel.Name), (NoResString)"Either Vessel Name or ETD is mandatory."); // non-translatable validation message
					transport.ETDInfo.AddMessageError(() => transport.ETD.IsEmpty && string.IsNullOrWhiteSpace(transport.Vessel.Name), (NoResString)"Either Vessel Name or ETD is mandatory."); // non-translatable validation message
					transport.AddValidationDependencies(transport.ETDInfo, transport.Vessel.NameInfo);

					transport.PortOfDischarge.CodeInfo.AddMessageErrorIfEmpty((NoResString)"Port of Discharge is mandatory."); // non-translatable validation message
				}

				transport.Vessel?.NameInfo.AddAsciiCharactersValidation();
				transport.VoyageFlightNumberInfo.AddAsciiCharactersValidation();
				transport.Carrier?.AddAsciiCharactersValidation();
			}
		}

		void AddSendingValidation(EManifest eManifest)
		{
			var packLines = shipment.OuterPackLines.Cast<PackLine>().ToArray();

			eManifest.SendAllBookingsInfo.AddMessageError(() => packLines.All(pl => pl.JL_JC.IsEmpty), (NoResString)"Packs need to be allocated to Containers."); // non-translatable validation message
			eManifest.SendAllBookingsInfo.AddWarning(() => eManifest.Bookings.Any() && packLines.Any(pl => pl.JL_JC.IsEmpty), (NoResString)"There are Packs which do not have a container linked on this Shipment. Only packs shown in this eManifest form (that have a container linked) with a Shipping Order Number will be sent."); // non-translatable validation message
			eManifest.SendAllBookingsInfo.AddMessageError(() => eManifest.Bookings.Any() && !eManifest.Bookings.Any(booking => booking.Send), (NoResString)"You must select at least one SLD to send the message."); // non-translatable validation message

			AddForEachBookingValidation(eManifest);
		}

		void AddForEachBookingValidation(EManifest eManifest)
		{
			var bookings = eManifest.Bookings;
			var containers = bookings.SelectMany(b => b.Containers);
			var packingLines = containers.SelectMany(c => c.PackingLines);
			var bookingsExistsDuplicatedShipments = bookings.Where(booking => booking.Send && !booking.BookingNumber.IsEmpty).ToDictionary(booking => booking, booking => ShippingOrderNumberValidationHelper.GetDuplicatedShipmentsByShippingOrderNumber(booking.BookingNumber, shipment.PK, shipment.Factory));

			bookings.ForEach(booking => booking.BookingNumberInfo.AddMessageError(() => booking.Send && booking.BookingNumber.IsEmpty, (NoResString)"Shipping Order Number is required. Enter it on Shipment level (Reference Type SLD) or packs (Shipping Order/Shi Lian Dan Number).")); // non-translatable validation message
			bookingsExistsDuplicatedShipments.Keys.ForEach(booking => booking.BookingNumberInfo.AddWarning(() => bookingsExistsDuplicatedShipments[booking].Length > 0, string.Format((NoResString)"This Shi Lian Dan/Shipping Order Number is already in use on: {0}", string.Join(", ", bookingsExistsDuplicatedShipments[booking].Select(otherShipment => otherShipment.JS_UniqueConsignRef))))); // non-translatable validation message
			containers.ForEach(container => container.NumberInfo.AddMessageError(() => container.IsEmpty, (NoResString)"This container is flagged as Empty. Remove that flag if incorrect or remove this container from linked packlines.")); // non-translatable validation message
			packingLines.ForEach(packingLine => ((HarmonizedCode)packingLine.HarmonizedCode).CodeInfo.AddWarning(() => eManifest.PortOfDischarge.Code.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Brazil && packingLine.HarmonizedCode.Code.IsEmpty, (NoResString)"HS Code is required for Brazil.")); // non-translatable validation message
		}

		void PopulateLocations(ForwardingConsol consol, EManifest eManifest)
		{
			var placeOfReceipt = Unloco.Create(context, consol?.LoadPort)
				.AddAsciiCharactersValidation();
			placeOfReceipt.CodeInfo.AddMessageErrorIfEmpty((NoResString)"Place of Receipt is required."); // non-translatable validation message
			eManifest.PlaceOfReceipt = placeOfReceipt;

			var placeOfIssue = Unloco.Create(context, consol?.MasterBillIssuePlace)
				.AddAsciiCharactersValidation();
			placeOfIssue.CodeInfo.AddMessageErrorIfEmpty((NoResString)"Place of Issue is required."); // non-translatable validation message
			eManifest.PlaceOfIssue = placeOfIssue;

			var placeOfDelivery = Unloco.Create(context, consol?.DischargePort)
				.AddAsciiCharactersValidation();
			placeOfDelivery.CodeInfo.AddMessageErrorIfEmpty((NoResString)"Place of Delivery is required."); // non-translatable validation message
			eManifest.PlaceOfDelivery = placeOfDelivery;

			var firstSeaLeg = consol?.Transports
				.OfType<Freight.Business.Transport>()
				.FirstOrDefault(t => t.TransportMode == Core.Constants.TransportModes.Sea);

			var lastSeaLeg = consol?.Transports
				.OfType<Freight.Business.Transport>()
				.LastOrDefault(t => t.TransportMode == Core.Constants.TransportModes.Sea);

			var portOfLoad = Unloco.Create(context, firstSeaLeg?.LoadPort)
				.AddAsciiCharactersValidation();

			portOfLoad.CodeInfo.AddMessageErrorIfEmpty((NoResString)"Port of Loading is required."); // non-translatable validation message
			eManifest.PortOfLoad = portOfLoad;

			((Unloco)eManifest.PortOfLoad).OnValueChanged(nameof(eManifest.PortOfLoad.Code)).Do(() =>
			{
				foreach (var packingLine in eManifest.Containers.SelectMany(c => c.PackingLines).OfType<PackingLine>())
				{
					packingLine.ExportHarmonizedCode = PackingLineBuilder.GetHarmonizedCodesDesc(consol.Factory, packingLine, eManifest.PortOfLoad.Code, context);
					packingLine.Validate(nameof(packingLine.ExportHarmonizedCode));
				}
			});

			var portOfDischarge = Unloco.Create(context, lastSeaLeg?.DiscPort)
				.AddAsciiCharactersValidation();

			portOfDischarge.CodeInfo.AddMessageErrorIfEmpty((NoResString)"Port of Discharge is required."); // non-translatable validation message
			eManifest.PortOfDischarge = portOfDischarge;

			((Unloco)eManifest.PortOfDischarge).OnValueChanged(nameof(eManifest.PortOfDischarge.Code)).Do(() =>
			{
				foreach (var packingLine in eManifest.Containers.SelectMany(c => c.PackingLines).OfType<PackingLine>())
				{
					packingLine.ImportHarmonizedCode = PackingLineBuilder.GetHarmonizedCodesDesc(consol.Factory, packingLine, eManifest.PortOfDischarge.Code, context);
					packingLine.Validate(nameof(packingLine.ImportHarmonizedCode));
				}
			});

			var portOfDestination = Unloco.Create(context, shipment.Destination)
				.AddAsciiCharactersValidation();

			portOfDestination.CodeInfo.AddMessageErrorIfEmpty((NoResString)"Final Destination is required."); // non-translatable validation message
			eManifest.PortOfDestination = portOfDestination;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		void PopulateAddresses(EManifest eManifest, ForwardingConsol consol)
		{
			var sendingAgent = shipment.IsDirectShipment
				? AddressBuilder.Create(context, shipment.ConsignorDocumentaryAddress).AddAsAgentInfoToCompanyName(shipment.ConsignorDocumentaryAddress)
				: AddressBuilder.Create(context, consol?.SendingForwarderWithContact).AddAsAgentInfoToCompanyName(consol?.SendingForwarderAddress);
			sendingAgent.Country.NameInfo.AddMessageErrorIfEmpty((NoResString)"Country is required.");

			eManifest.SendingAgent = sendingAgent
				.AddPartyNameAndAddressValidation((NoResString)"Shipper/Sending Agent")
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation();

			var carrier = AddressBuilder.Create(context, consol?.ShippingLineAddress);
			carrier.AddPartyNameAndAddressValidation((NoResString)"Carrier")
					.AddContactDetailsValidation()
					.AddAsciiCharactersValidation();

			AddCarrierCodeValidations(eManifest, carrier);

			PopulateContactNameAndEmail(carrier, consol);

			eManifest.Carrier = carrier;

			var receivingAgent = shipment.IsDirectShipment
				? AddressBuilder.Create(context, shipment.ConsigneeDocumentaryAddress)
				: AddressBuilder.Create(context, consol?.ReceivingForwarderWithContact);
			receivingAgent.Country.NameInfo.AddMessageError(() => receivingAgent.Country.Name.IsEmpty && !receivingAgent.IsToOrder(), Res.GetString("0B477BE9-269F-49E9-BC7D-2A0D63293F2A", "Country is required."));

			eManifest.ReceivingAgent = receivingAgent
				.AddPartyNameAndAddressValidation((NoResString)"Consignee/Receiving Agent", () => !receivingAgent.IsToOrder(), false)
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation()
				.AddToOrderSupport();

			var notifyParty = shipment.IsDirectShipment
				? AddressBuilder.Create(context, shipment.NotifyPartyDocumentaryAddress)
				: AddressBuilder.Create(context, consol?.NotifyPartyDocumentaryAddress);
			notifyParty.Country.NameInfo.AddMessageError(() => !notifyParty.IsEmpty() && !notifyParty.CompanyName.IsEmpty && notifyParty.Country.Name.IsEmpty, (NoResString)"Country is required.");
			notifyParty.Country.AddValidationDependencies(notifyParty.Country.NameInfo, notifyParty.CompanyNameInfo);

			eManifest.NotifyParty = notifyParty
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation()
				.AddPostcodeValidationForUSImports((NoResString)"Notify Party")
				.AddPartyNameAndAddressValidation((NoResString)"Notify Party", () => !notifyParty.CompanyName.IsEmpty)
				.AddSameAsConsigneeSupport();

			var notifyParty2 = shipment.IsDirectShipment
				? AddressBuilder.Create(context, shipment.NotifyParty2DocumentaryAddress)
				: AddressBuilder.Create(context, consol?.NotifyParty2DocumentaryAddress);
			notifyParty2.Country.NameInfo.AddMessageError(() => !notifyParty2.IsEmpty() && notifyParty2.Country.Name.IsEmpty, (NoResString)"Country is required.");
			notifyParty2.Country.AddValidationDependencies(notifyParty2.Country.NameInfo, notifyParty2.CompanyNameInfo);

			eManifest.NotifyParty2 = notifyParty2
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation();

			eManifest.CarrierHandlingAgent = AddressBuilder.Create(context, consol?.CarrierHandlingAgentDocumentaryAddress)
				.AddAsciiCharactersValidation();

			var carrierBookingAgent = AddressBuilder.Create(context, consol?.CarrierBookingAgentDocumentaryAddress);
			eManifest.CarrierBookingAgent = carrierBookingAgent
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation();

			eManifest.Forwarder = AddressBuilder.Create(context, consol?.SendingForwarderWithContact)
				.AddPartyNameAndAddressValidation((NoResString)"Forwarder")
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation();

			eManifest.CurrentUser = AddressBuilder.CreateForCurrentUser(context)
				.AddAsciiCharactersValidation();

			AddCarrierHandlingAgentValidationRules(eManifest);

			((Address)eManifest.Carrier).ContactInfo.AddMessageError(() =>
				{
					var orgHeader = consol != null ? (consol.IsCoLoad ? consol.Creditor : consol.ShippingLine) : null;
					if (orgHeader != null)
					{
						return CarrierHasIntegrationViaEmailToCarrierLocalOfficeTag(orgHeader) && (carrier.Contact.IsEmpty || carrier.Email.IsEmpty);
					}
					return false;
				}
				, "This carrier only supports integration via email to local office.\r\nContact name and email address are required to send eManifest.\r\nPlease maintain contact name and email address in carrier Organization > Contact > Email and Receiving Documents > Group SHP.");
		}

		void PopulateTaxInfo(EManifest eManifest, ForwardingConsol consol)
		{
			var isDirectShipment = shipment.IsDirectShipment;

			eManifest.SendingAgentTaxInfo = new TaxInfo();
			eManifest.ReceivingAgentTaxInfo = new TaxInfo();
			eManifest.NotifyPartyTaxInfo = new TaxInfo();
			eManifest.NotifyParty2TaxInfo = new TaxInfo();

			PopulateTaxInfo(eManifest.SendingAgentTaxInfo, isDirectShipment ? shipment.Consignor : consol?.SendingForwarder);
			PopulateTaxInfo(eManifest.ReceivingAgentTaxInfo, isDirectShipment ? shipment.Consignee : consol?.ReceivingForwarder);
			PopulateTaxInfo(eManifest.NotifyPartyTaxInfo, isDirectShipment ? shipment.NotifyParty : consol?.NotifyParty);
			PopulateTaxInfo(eManifest.NotifyParty2TaxInfo, isDirectShipment ? shipment.NotifyParty2DocumentaryAddress.Organisation : consol?.NotifyParty2);
		}

		void PopulateTaxInfo(TaxInfo taxInfo, OrgHeader orgHeader)
		{
			if (orgHeader == null)
			{
				return;
			}

			var orgHeaderRegistrationNumberProvider = new OrgHeaderRegistrationNumberProvider(orgHeader);
			TaxCodeInformation taxNumberInfo = null;
			var taxInfos = ChinaCustomsTaxNumberHelper.GetTaxInfoFromRefTable(orgHeader.CountryCode, orgHeaderRegistrationNumberProvider);
			if (taxInfos.Count == 0)
			{
				taxInfo.IsPlaceHolder = true;
				taxInfo.IsChinaSpecific = false;
				taxNumberInfo = new TaxCodeInformation("9999", "9999", "9999", "9999", ZString.Empty, ZString.Empty, ZInt.Zero, ZString.Empty, ZString.Empty, ZString.Empty);
				AddTaxInfoNotFoundValidation(taxInfo);
			}
			else
			{
				taxInfo.IsPlaceHolder = false;
				taxInfo.IsChinaSpecific = true;
				taxNumberInfo = taxInfos.Where(t => (t.DocumentType == Core.Constants.TaxRelatedDocumentType.ShippingInstruction) && !t.Number.IsEmpty).OrderBy(t => t.Priority).FirstOrDefault();
				if (taxNumberInfo == null)
				{
					taxNumberInfo = taxInfos.OrderBy(t => t.Priority).FirstOrDefault();
				}
			}

			taxInfo.Code = taxNumberInfo.Code;
			taxInfo.Description = taxNumberInfo.Description;
			taxInfo.LongLabel = taxNumberInfo.LongLabel;
			taxInfo.ShortLabel = taxNumberInfo.ShortLabel;
			taxInfo.Number = taxNumberInfo.Number;
			taxInfo.Country = new Country(orgHeader.Factory, context.Countries) { Code = taxNumberInfo.CountryCode };
		}

		void PopulateContactNameAndEmail(Address carrierAddress, ForwardingConsol consol)
		{
			if (carrierAddress == null || consol == null)
			{
				return;
			}
			var carrierOrgHeader = consol.IsCoLoad ? consol.Creditor : consol.ShippingLine;
			var contact = GetDefaultContact(carrierOrgHeader?.FilteredContacts);
			if (carrierOrgHeader != null && carrierOrgHeader.ShippingLine != null
										 && CarrierHasIntegrationViaEmailToCarrierLocalOfficeTag(carrierOrgHeader) && contact != null)
			{
				carrierAddress.Email = contact.OC_Email;
				carrierAddress.Contact = contact.OC_ContactName;
			}
		}

		OrgContact GetDefaultContact(FilteredContactsCollection contacts)
		{
			OrgContact matched = null;
			if (contacts != null)
			{
				var matchedSHPdocumentGroupContracts = contacts.Cast<OrgContact>().Where(contact => !contact.OC_Email.IsEmpty && !contact.OC_ContactName.IsEmpty &&
					contact.Documents.Cast<OrgDocument>().Any(document => document.OD_DocumentGroup == "SHP"));

				var matchedALLdocumnetGroupContracts = contacts.Cast<OrgContact>().Where(contact => !contact.OC_Email.IsEmpty && !contact.OC_ContactName.IsEmpty &&
					contact.Documents.Cast<OrgDocument>().Any(document => document.OD_DocumentGroup == "ALL"));

				matched = matchedSHPdocumentGroupContracts.Any() ? matchedSHPdocumentGroupContracts.First() :
					matchedALLdocumnetGroupContracts.Any() ? matchedALLdocumnetGroupContracts.First() : null;
			}
			return matched;
		}

		static void AddCarrierCodeValidations(EManifest eManifest, Address carrier)
		{
			var hasScacNumber = carrier.HasRegistrationNumber(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.CodeTypes.CarrierCode);
			var hasNGBNumber = carrier.HasRegistrationNumber(Core.Constants.CountryCodes.China, OrgCusCode.ChinaCodeTypes.NGB);
			var hasENPNumber = carrier.HasRegistrationNumber(Core.Constants.CountryCodes.China, OrgCusCode.ChinaCodeTypes.ENP);
			var isLoadingInNingboPort = eManifest.IsLoadingInNingboPort();
			var isLoadingInShanghaiPort = eManifest.IsLoadingInShanghaiPort();

			carrier.CompanyNameInfo.AddMessageError(() => !hasScacNumber, (NoResString)"Carrier SCAC is missing from Carrier organization record under Organisation > Config > Country US, Type CCC."); // non-translatable validation message
			carrier.CompanyNameInfo.AddMessageError(() => isLoadingInNingboPort && !hasNGBNumber, CreateCarrierCodeValidationMessage("NGB", (NoResString)"Ningbo", false)); // non-translatable validation message
			carrier.CompanyNameInfo.AddWarning(() => isLoadingInShanghaiPort && !hasENPNumber && eManifest.CarrierHandlingAgent.IsEmpty(), CreateCarrierCodeValidationMessage("ENP", (NoResString)"Easipass")); // non-translatable validation message
			carrier.CompanyNameInfo.AddWarning(() => !isLoadingInNingboPort && !isLoadingInShanghaiPort && (!hasNGBNumber || !hasENPNumber) && eManifest.CarrierHandlingAgent.IsEmpty(), CreateCarrierCodeValidationMessage((NoResString)"ENP or NGB", (NoResString)"Easipass and Ningbo")); // non-translatable validation message

			string CreateCarrierCodeValidationMessage(string regCodeName, string portName, bool appendCarrierHandlingAgentMessage = true)
			{
				return $@"Carrier Organization does not have {regCodeName} code types saved, 
which are required to route the message via port systems ({portName} EDI Centre)" // non-translatable validation message
+ (!appendCarrierHandlingAgentMessage ? "." : $@", 
in the absence of the Carrier Handling Agent. The message can still be routed to the carrier either directly or via other service providers."); // non-translatable validation message
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305")]
		static void AddTaxInfoValidations(EManifest eManifest, Func<ZPropertyInfo, Func<bool>, string, bool> regionalSpecificValidateFunc)
		{
			if (eManifest == null)
			{
				return;
			}

			bool IsTaxInfoInvalid(Address address, TaxInfo taxInfo)
			{
				return address.IsEmpty() || (!taxInfo.Code.IsEmpty && taxInfo.Code != "9999" && taxInfo.Number.IsEmpty);
			}

			if (eManifest.SendingAgentTaxInfo != null)
			{
				regionalSpecificValidateFunc(eManifest.SendingAgentTaxInfo.NumberInfo,
					() => IsTaxInfoInvalid((Address)eManifest.SendingAgent, eManifest.SendingAgentTaxInfo),
					$"Company ID, {ChinaCustomsTaxNumberHelper.GetTaxNumberTypesForTaxCodeInformation(eManifest.SendingAgentTaxInfo.Code, eManifest.SendingAgentTaxInfo.Country?.Code ?? ZString.Empty)} is required to comply with CCAM (China Customs Advanced Manifest) reporting in line with Customs Circular No.56."); // non-translatable validation message
			}

			if (eManifest.ReceivingAgentTaxInfo != null)
			{
				regionalSpecificValidateFunc(eManifest.ReceivingAgentTaxInfo.NumberInfo,
					() => IsTaxInfoInvalid((Address)eManifest.ReceivingAgent, eManifest.ReceivingAgentTaxInfo) && IsTaxInfoInvalid((Address)eManifest.NotifyParty, eManifest.NotifyPartyTaxInfo),
					$"Company ID, {ChinaCustomsTaxNumberHelper.GetTaxNumberTypesForTaxCodeInformation(eManifest.ReceivingAgentTaxInfo.Code, eManifest.ReceivingAgentTaxInfo.Country?.Code ?? ZString.Empty)} is required to comply with CCAM (China Customs Advanced Manifest) reporting in line with Customs Circular No.56."); // non-translatable validation message
			}

			if (eManifest.NotifyPartyTaxInfo != null)
			{
				regionalSpecificValidateFunc(eManifest.NotifyPartyTaxInfo.NumberInfo,
					() => IsTaxInfoInvalid((Address)eManifest.ReceivingAgent, eManifest.ReceivingAgentTaxInfo) && IsTaxInfoInvalid((Address)eManifest.NotifyParty, eManifest.NotifyPartyTaxInfo),
					$"Company ID, {ChinaCustomsTaxNumberHelper.GetTaxNumberTypesForTaxCodeInformation(eManifest.NotifyPartyTaxInfo.Code, eManifest.NotifyPartyTaxInfo.Country?.Code ?? ZString.Empty)} is required to comply with CCAM (China Customs Advanced Manifest) reporting in line with Customs Circular No.56."); // non-translatable validation message
				regionalSpecificValidateFunc(eManifest.NotifyPartyTaxInfo.NumberInfo,
					() => ((Address)eManifest.ReceivingAgent).IsToOrder() && IsTaxInfoInvalid((Address)eManifest.NotifyParty, eManifest.NotifyPartyTaxInfo),
(NoResString)"In line with China Customs Circular No.56, the Notify Party’s Company ID number \r\nis required when the Consignee is \"TO ORDER\"."); // non-translatable validation message
			}

			eManifest.NotifyPartyTaxInfo.AddValidationDependencies(eManifest.NotifyPartyTaxInfo.NumberInfo, ((Address)eManifest.ReceivingAgent).CompanyNameInfo);
		}

		void AddTaxInfoNotFoundValidation(TaxInfo taxInfo)
		{
			Func<ZString, bool> validLabel = label => label == "8888" || label == "9999";
			taxInfo.ShortLabelInfo.AddWarning(() =>
			{
				return taxInfo.IsPlaceHolder && validLabel(taxInfo.ShortLabel);
			}, taxInfoNoneFoundWarning);
			taxInfo.ShortLabelInfo.AddMessageError(() => taxInfo.IsPlaceHolder && !validLabel(taxInfo.ShortLabel), taxInfoNoneFoundInvalidValue);
		}

		void AddCarrierHandlingAgentValidationRules(EManifest eManifest)
		{
			var carrierHandlingAgent = (Address)eManifest.CarrierHandlingAgent;

			var hasC1CNumber = carrierHandlingAgent.HasRegistrationNumber(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode);
			var hasNGBNumber = carrierHandlingAgent.HasRegistrationNumber(Core.Constants.CountryCodes.China, OrgCusCode.ChinaCodeTypes.NGB);
			var hasENPNumber = carrierHandlingAgent.HasRegistrationNumber(Core.Constants.CountryCodes.China, OrgCusCode.ChinaCodeTypes.ENP);

			carrierHandlingAgent.CompanyNameInfo.AddMessageError(() => !carrierHandlingAgent.CompanyName.IsEmpty && !hasC1CNumber,
(NoResString)"C1C Code is missing from Carrier Handling Agent record under Organization > Config > Type C1C."); // non-translatable validation message

			carrierHandlingAgent.CompanyNameInfo.AddWarning(() => !carrierHandlingAgent.IsEmpty() && eManifest.IsLoadingInNingboPort() && (!hasNGBNumber || !hasENPNumber),
(NoResString)"NGB or ENP code might be required for eManifest routing via Ningbo EDI Centre or Easipass (entered via Organization > Config > Country CN Type NGB or ENP).\r\nWithout these codes eManifest may still be routed to the Carrier Handling Agent (subject to eHub configuration)."); // non-translatable validation message

			carrierHandlingAgent.CompanyNameInfo.AddWarning(() => !carrierHandlingAgent.IsEmpty() && !eManifest.IsLoadingInNingboPort() && eManifest.IsLoadingInChina() && !hasENPNumber,
(NoResString)"ENP code might be required for eManifest routing via Easipass (entered via Organization > Config > Country CN Type ENP).\r\nWithout this code, eManifest may still be routed to the Carrier Handling Agent directly, or through another service provider (subject to eHub configuration)."); // non-translatable validation message

			carrierHandlingAgent.CompanyNameInfo.AddWarning(() => carrierHandlingAgent.IsEmpty() && eManifest.IsLoadingInChina(),
(NoResString)"In the absence of Carrier Handling Agent, the message may be routed to the Carrier (subject to eHub configuration)."); // non-translatable validation message

			if (eManifest.GetFirstSeaTransport() is Transport transport)
			{
				transport.PortOfDischarge.OnValueChanged(nameof(transport.PortOfLoading.Code)).Do(() => carrierHandlingAgent.ValidateAllIncludingChildren());
			}
		}

		void PopulateReferenceNumbers(EManifest eManifest, ForwardingConsol consol)
		{
			eManifest.CarrierBookingReference = consol?.JK_BookingReference ?? ZString.Empty;
			eManifest.CarrierBookingReferenceInfo.AddWarningIfEmpty(string.Concat((NoResString)"Although Carrier Booking Reference is not mandatory for messages routed via Ports (Easipass and Ningbo EDI Centre),",  // non-translatable validation message
				System.Environment.NewLine,
(NoResString)"the message may be rejected if routed to the Carrier or Agent directly or via other service providers (such as INTTRA).")); // non-translatable validation message
			eManifest.CarrierBookingReferenceInfo.AddAsciiCharactersValidation();

			eManifest.BillOfLadingNumber = consol?.JK_MasterBillNum ?? ZString.Empty;
			eManifest.CarrierContractNumber = consol?.JK_CarrierContractNumber ?? ZString.Empty;

			var quotationNumber = eManifest.Numbers?.FirstOrDefault(n => n.Type != null && n.Type.Code == AdditionalReferences.Codes.CarrierQuoteNumber);
			eManifest.QuotationNumber = quotationNumber != null ? quotationNumber.Value : ZString.Empty;

			eManifest.ShipperReference = shipment.IsDirectShipment ? shipment.JS_BookingReference : (consol?.JK_AgentsReference ?? string.Empty);
			eManifest.FreightForwarderReference = consol?.JK_UniqueConsignRef ?? ZString.Empty;

			if (eManifest.CarrierBookingReference.IsEmpty || eManifest.BillOfLadingNumber.IsEmpty)
			{
				eManifest.UseBkgRefAsMasterSO = !eManifest.CarrierBookingReference.IsEmpty;
				eManifest.UseMasterBillAsMasterSO = !eManifest.BillOfLadingNumber.IsEmpty;
			}
			else if (eManifest.CarrierBookingReference == eManifest.BillOfLadingNumber)
			{
				eManifest.UseBkgRefAsMasterSO = true;
				eManifest.UseMasterBillAsMasterSO = false;
			}
			else
			{
				eManifest.UseBkgRefAsMasterSO = false;
				eManifest.UseMasterBillAsMasterSO = false;
			}

			eManifest.BillOfLadingNumberInfo.AddMessageErrorIfEmpty((NoResString)"BOL Number is required."); // non-translatable validation message
			eManifest.UseBkgRefAsMasterSOInfo.AddMessageError(() => !eManifest.UseBkgRefAsMasterSO && !eManifest.UseMasterBillAsMasterSO, (NoResString)"At least one of these checkboxes must be ticked to indicate which the carrier uses as the master SO#"); // non-translatable validation message
			eManifest.UseMasterBillAsMasterSOInfo.AddMessageError(() => !eManifest.UseBkgRefAsMasterSO && !eManifest.UseMasterBillAsMasterSO, (NoResString)"At least one of these checkboxes must be ticked to indicate which the carrier uses as the master SO#"); // non-translatable validation message
		}

		void PopulatePickupDeliveryInfo(EManifest eManifest, ForwardingConsol consol)
		{
			const string cfs = "CFS";

			eManifest.IsDoorPickup = consol
				?.Containers
				.OfType<CommonContainer>()
				.Any(container => container.JC_DeliveryMode.StartsWith(cfs, StringComparison.OrdinalIgnoreCase))
				?? false;

			eManifest.IsDoorDelivery = consol
				?.Containers
				.OfType<CommonContainer>()
				.Any(container => container.JC_DeliveryMode.EndsWith(cfs, StringComparison.OrdinalIgnoreCase))
				?? false;

			var pickupFrom = AddressBuilder.Create(context, shipment.IsDirectShipment ? shipment.ConsignorPickupAddress : consol?.PackDepotAddress)
				.AddPartyNameAndAddressValidation((NoResString)"Pickup From", () => eManifest.IsDoorPickup) // non-translatable validation message
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation();

			pickupFrom.ContactInfo.AddMessageError(() => eManifest.IsDoorPickup && (pickupFrom.Contact.IsEmpty || pickupFrom.Phone.IsEmpty), (NoResString)"Contact name and Telephone number are mandatory when 'Door Pickup' is selected."); // non-translatable validation message

			eManifest.PickupFrom = pickupFrom;
			pickupFrom.AddValidationDependencies(pickupFrom.ContactInfo, eManifest.IsDoorPickupInfo, pickupFrom.PhoneInfo);

			var deliverTo = AddressBuilder.Create(context, shipment.IsDirectShipment ? shipment.ConsigneeDeliveryAddress : consol?.UnpackDepotAddress)
				.AddPartyNameAndAddressValidation((NoResString)"Deliver To", () => eManifest.IsDoorDelivery) // non-translatable validation message
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation();

			deliverTo.ContactInfo.AddMessageError(() => eManifest.IsDoorDelivery && (deliverTo.Contact.IsEmpty || deliverTo.Phone.IsEmpty), (NoResString)"Contact name and Telephone number are mandatory when 'Door Delivery' is selected."); // non-translatable validation message
			deliverTo.AddValidationDependencies(deliverTo.ContactInfo, deliverTo.PhoneInfo);

			eManifest.DeliverTo = deliverTo;
			eManifest.IsDoorDeliveryInfo.ValueChanged += (s, e) => deliverTo.Validate(nameof(deliverTo.Contact));
		}

		void PopulateGoodsDetails(EManifest eManifest, ForwardingConsol consol, Func<ZPropertyInfo, Func<bool>, string, bool> regionalSpecificValidateFunc)
		{
			var containerBuilder = new ContainerBuilder();
			var packlineBuilder = new PackingLineBuilder();

			var containerBizObjs = new Dictionary<ZGuid, CommonContainer>();
			var containerPackingLines = new Dictionary<ZGuid, List<PackingLine>>();

			var bookings = new Dictionary<ZString, Booking>();
			var bookingRelatedContainers = new Dictionary<ZString, HashSet<ZGuid>>();

			foreach (var packlineBizObj in shipment.OuterPackLines.OfType<PackLine>())
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
					booking = new Booking(bookingNumber, logProvider)
					{
						BookingNumber = bookingNumber
					};

					booking.Send = booking.MessageStatus.AllowSendOriginal
						|| booking.MessageStatus.AllowSendAmendment;

					booking.OnValueChanged(nameof(booking.Send)).Do(() =>
					{
						eManifest.Validate(nameof(eManifest.SendAllBookings));
						booking.ValidateAllIncludingChildren();
					});

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
					container.VerifiedByAddress.AddAsciiCharactersValidation();

					containers[containerBizObj.PK] = container;
				}
			}

			eManifest.Containers = containers.Values.ToArray();

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

						AddContainerValidation(booking, bookingContainer, consol, regionalSpecificValidateFunc);
						bookingContainer.PackingLines.Cast<PackingLine>().ForEach(p => AddPackLineValidation(booking, p));

						bookingContainers.Add(bookingContainer);
					}
				}

				booking.Containers = bookingContainers;
			}

			eManifest.Bookings = bookings
				.Values
				.OrderBy(GetBookingSortKey)
				.ToArray();
		}

		int GetBookingSortKey(Booking booking)
		{
			if (booking.MessageStatus.AllowSendOriginal)
			{
				return 0;
			}
			if (booking.MessageStatus.AllowSendAmendment)
			{
				return 1;
			}
			return 2;
		}

		void AddPackLineValidation(Booking booking, PackingLine packLine)
		{
			packLine.QuantityInfo.AddMessageError(() => booking.Send && packLine.Quantity == 0, (NoResString)"Pack count is mandatory."); // non-translatable validation message
			packLine.Weight.ValueInfo.AddMessageError(() => booking.Send && packLine.Weight.Value == 0, (NoResString)"Pack weight is mandatory."); // non-translatable validation message
			packLine.GoodsDescriptionInfo.AddMessageError(() => booking.Send && packLine.GoodsDescription.IsEmpty, (NoResString)"Goods Description is mandatory."); // non-translatable validation message
			packLine.GoodsDescriptionInfo.AddMessageError(() => packLine.GoodsDescription.Length > 256, (NoResString)"Goods Description is too long so will be cut off in the message - Maximum characters 256."); // non-translatable validation message
			packLine.GoodsDescriptionInfo.AddAsciiCharactersValidation();
			packLine.MarksAndNumbersInfo.AddMessageError(() => booking.Send && packLine.MarksAndNumbers.IsEmpty, (NoResString)"Marks are mandatory."); // non-translatable validation message
			packLine.MarksAndNumbersInfo.AddAsciiCharactersValidation();

			foreach (var dg in packLine.DangerousGoods)
			{
				dg.Contact.FullNameInfo.AddMessageError(() => booking.Send && string.IsNullOrWhiteSpace(dg.Contact.FullName), (NoResString)"Contact Name is required for dangerous goods."); // non-translatable validation message
				dg.Contact.PhoneInfo.AddMessageError(() => booking.Send && string.IsNullOrWhiteSpace(dg.Contact.Phone), (NoResString)"Contact Phone is required for dangerous goods."); // non-translatable validation message

				dg.Validator = () => AddDangerousGoodsValidation(dg);
			}
		}

		static IEnumerable<string> AddDangerousGoodsValidation(DangerousGood dangerousGood)
		{
			if (dangerousGood.IMOClass.IsEmpty || dangerousGood.Code.IsEmpty || dangerousGood.ProperShippingName.IsEmpty)
			{
				yield return (NoResString)"DG Class, UNDG and Proper Shipping Name are required for dangerous goods.\r\nPlease enter Shipment > Packing > Pack Lines > Dangerous Goods > DG Substance."; // non-translatable registration number
			}
		}

		void AddContainerValidation(Booking booking, BookingContainer bookingContainer, ForwardingConsol consol, Func<ZPropertyInfo, Func<bool>, string, bool> validationFunc)
		{
			var isLoadInChina = consol?.JK_RL_NKLoadPort.IsLoadingInChina() ?? false;
			var isRefrigerated = new Func<bool>(() => (bookingContainer.Type?.Type?.Code ?? ZString.Empty) == Core.Constants.ContainerTypes.Refrigerated);

			validationFunc(bookingContainer.NumberInfo, () => booking.Send && isLoadInChina && string.IsNullOrWhiteSpace(bookingContainer.Number), (NoResString)"In line with China Customs requirements, container number is required."); // non-translatable validation
			validationFunc(bookingContainer.SealInfo, () => booking.Send && isLoadInChina && bookingContainer.Seal.IsEmpty, (NoResString)"In line with China Customs requirements, seal number is required."); // non-translatable validation message

			bookingContainer.Type.ISOCodeInfo.AddMessageError(() => booking.Send && bookingContainer.Type.ISOCode.IsEmpty, (NoResString)"Container type entered does not have a valid ISO code. Enter a valid ISO code here or add it to the matching Container Reference file."); // non-translatable validation message
			bookingContainer.GoodsWeight.ValueInfo.AddMessageError(() => booking.Send && bookingContainer.GoodsWeight.Value == 0, (NoResString)"Cargo weight is mandatory."); // non-translatable validation message
			((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo.AddMessageError(() => booking.Send && !bookingContainer.IsNonOperativeReefer && isRefrigerated() && bookingContainer.AirVentFlow.Unit.Code.IsEmpty && bookingContainer.AirVentFlow.Value != 0, (NoResString)"No Air Vent Setting measurement type exists. Ensure each temperature controlled container has one entered on the Containers > Refrigeration tab."); // non-translatable validation message
			((CodeDescription)bookingContainer.SetTemperature.Unit).CodeInfo.AddMessageError(() => booking.Send && !bookingContainer.IsNonOperativeReefer && isRefrigerated() && string.IsNullOrWhiteSpace(bookingContainer.SetTemperature.Unit?.Code), (NoResString)"Temperature is required when container is a reefer."); // non-translatable validation message
			((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo.AddMessageError(() => bookingContainer.HasControlledAtmosphere && bookingContainer.AirVentFlow.Value != 0 && bookingContainer.AirVentFlow.Unit.Code != "2L" && bookingContainer.AirVentFlow.Unit.Code != "MQH", Res.GetString("0522D0B6-DDA7-4518-8A83-4D9694C18452", "Selected Air Vent Setting measurement type is not supported by the carrier. Ensure each temperature controlled container has selected either '2L' or 'MQH' on the Containers > Refrigeration tab."));
			((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo.AddWarning(() => bookingContainer.AirVentFlow.Value == 0 && !bookingContainer.AirVentFlow.Unit.Code.IsEmpty, (NoResString)"Air Vent is marked as \"Open\".\r\nTo indicate \"Closed\", leave the Consol > Containers > Refrigeration > Air Vent Setting > Unit field blank.");
			((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo.AddWarning(() => bookingContainer.AirVentFlow.Value != 0 && bookingContainer.AirVentFlow.Unit.Code == "2L", (NoResString)"Carriers accept airflow only in metric units.\r\nAny value entered as cubic feet per minute (2L) is automatically converted to cubic meters per hour (MQH) during transmission.");
		}

		void PopulateFreightCharges(EManifest eManifest, ForwardingConsol consol)
		{
			if (consol == null)
			{
				return;
			}

			eManifest.IsFreightPrepaid = consol?.JK_PrepaidCollect.ToString() == Core.Constants.PaymentType.Prepaid;
			eManifest.IsFreightCollect = consol?.JK_PrepaidCollect.ToString() == Core.Constants.PaymentType.Collect;

			eManifest.IsFreightPrepaidInfo.AddMessageError(() => !eManifest.IsFreightPrepaid && !eManifest.IsFreightCollect, (NoResString)"Either Prepaid or Collect payment type must be selected."); // non-translatable validation message
			eManifest.IsFreightCollectInfo.AddMessageError(() => !eManifest.IsFreightPrepaid && !eManifest.IsFreightCollect, (NoResString)"Either Prepaid or Collect payment type must be selected."); // non-translatable validation message

			var freightPayableAt = eManifest.IsFreightPrepaid ? consol.LoadPort : (eManifest.IsFreightCollect ? consol.DischargePort : null);
			var freightPayableAtDo = Unloco.Create(context, freightPayableAt)
				.AddAsciiCharactersValidation();

			freightPayableAtDo.CodeInfo.AddMessageErrorIfEmpty((NoResString)"The location of where freight is paid is required."); // non-translatable validation message

			eManifest.FreightPayableAt = freightPayableAtDo;
		}

		static void PopulateOtherCharges(EManifest eManifest)
		{
			var otherCharges = new OtherCharges
			{
				IsPrepaid = eManifest.IsFreightPrepaid,
				IsCollect = eManifest.IsFreightCollect
			};

			bool emptyPaymentDetails() => !otherCharges.IsPrepaid && !otherCharges.IsCollect && !otherCharges.IsFree && !otherCharges.IsPayableElsewhere && !otherCharges.IsFirstLinePrepaidLineSecondCollect && string.IsNullOrWhiteSpace(otherCharges.Remarks);

			otherCharges.IsPrepaidInfo.AddMessageError(emptyPaymentDetails, (NoResString)"At least one type must be selected for Payment Details."); // non-translatable validation message
			otherCharges.IsCollectInfo.AddMessageError(emptyPaymentDetails, (NoResString)"At least one type must be selected for Payment Details."); // non-translatable validation message
			otherCharges.IsFreeInfo.AddMessageError(emptyPaymentDetails, (NoResString)"At least one type must be selected for Payment Details."); // non-translatable validation message
			otherCharges.IsPayableElsewhereInfo.AddMessageError(emptyPaymentDetails, (NoResString)"At least one type must be selected for Payment Details."); // non-translatable validation message
			otherCharges.IsFirstLinePrepaidLineSecondCollectInfo.AddMessageError(emptyPaymentDetails, (NoResString)"At least one type must be selected for Payment Details."); // non-translatable validation message
			otherCharges.RemarksInfo.AddMessageError(emptyPaymentDetails, (NoResString)"At least one type must be selected for Payment Details."); // non-translatable validation message
			otherCharges.RemarksInfo.AddAsciiCharactersValidation();

			eManifest.OtherCharges = otherCharges;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string taxInfoNoneFoundWarning = "9999 is used for company, but you can modify it to 8888 for an individual";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		const string taxInfoNoneFoundInvalidValue = "Must be 9999 for company, or 8888 for an individual";

		ZBool CarrierHasIntegrationViaEmailToCarrierLocalOfficeTag(OrgHeader org)
		{
			return org.GetShippingLineMessagingRequirement(ShippingLineMessagingRequirement.Types.IntegrationViaEmailToCarrierLocalOffice)?.RSR_IsEManifest ?? ZBool.False;
		}
	}
}
