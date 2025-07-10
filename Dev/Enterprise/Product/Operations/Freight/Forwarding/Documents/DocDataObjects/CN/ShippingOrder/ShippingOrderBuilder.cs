using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CarrierMessageValidation;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;
using ShippingLineMessagingRequirement = Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants.ShippingLineMessagingRequirement;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	sealed class ShippingOrderBuilder
	{
		public ShippingOrderBuilder(ForwardingConsol consol)
		{
			this.consol = consol;
			context = new ContextWithCarrierUnlocoMapping(consol.Factory.GetCachedReadOnlyFactory(), consol.IsCoLoad ? consol.CreditorPK : consol.ShippingLinePK);
			packageGroupingHelper = new PackageGroupingHelper(consol, context);
		}

		readonly ForwardingConsol consol;
		readonly IContext context;
		readonly PackageGroupingHelper packageGroupingHelper;

		public ShippingOrder Build()
		{
			consol.Transports.Sort(MovementLegComparer.PortsAndDatesBased(consol.Transports));

			var shippingOrder = new ShippingOrder(
				nameof(ForwardingConsol),
				consol.CarrierShipperReferenceWithFallback,
				DataContext.ShippingOrder);

			var directShipment = consol.DirectShipment;

			shippingOrder.CarrierBookingReference = consol?.JK_BookingReference ?? consol?.JK_UniqueConsignRef ?? ZString.Empty;
			shippingOrder.NumberOfOriginals = directShipment?.JS_NoOriginalBills ?? consol.JK_NoOriginalBills;
			shippingOrder.NumberOfCopies = directShipment?.JS_NoCopyBills ?? consol.JK_NoCopyBills;

			shippingOrder.RequestedDateOfIssue = consol.JK_MasterBillIssueDate;

			var releaseTypes = new ChinaReleaseTypes();

			var seaWyaBillCodes = new[]
			{
				Core.Constants.ShipmentReleaseTypes.SeaWaybill,
				Core.Constants.ShipmentReleaseTypes.ExpressBofL,
				Core.Constants.ShipmentReleaseTypes.NonNegotiable
			};

			var isSeawaybill = directShipment != null && seaWyaBillCodes.Any(code => string.CompareOrdinal(code, directShipment.JS_ReleaseType) == 0)
				|| seaWyaBillCodes.Any(code => string.CompareOrdinal(code, consol.JK_ReleaseType) == 0);

			shippingOrder.ShipmentType = new CodeDescription(consol.JK_AgentType_List)
			{
				Code = consol.JK_AgentType
			};

			shippingOrder.ReleaseType = new CodeDescription(releaseTypes)
			{
				Code = isSeawaybill
					? ShippingInstructionReleaseTypes.Codes.SeaWaybill
					: ShippingInstructionReleaseTypes.Codes.BOLOriginal
			};

			shippingOrder.ContainerMode = new CodeDescription(consol.JK_ConsolMode_List)
			{
				Code = GetContainerModeCode()
			};

			const string cfs = "CFS";

			shippingOrder.IsDoorPickup = consol
				.Containers
				.OfType<CommonContainer>()
				.Any(container => container.JC_DeliveryMode.StartsWith(cfs, StringComparison.OrdinalIgnoreCase));

			shippingOrder.OnValueChanged(nameof(shippingOrder.IsDoorPickup)).Do(() =>
			{
				shippingOrder.PickupFrom = consol.GetPickupFromAddress(context, consol.IsPickup(shippingOrder.IsDoorPickup));
				AddAddressValidation(shippingOrder);
				ValidateContactAndAddress(shippingOrder, (Address)shippingOrder.PickupFrom, true);
			});

			shippingOrder.IsDoorDelivery = consol
				.Containers
				.OfType<CommonContainer>()
				.Any(container => container.JC_DeliveryMode.EndsWith(cfs, StringComparison.OrdinalIgnoreCase));

			shippingOrder.OnValueChanged(nameof(shippingOrder.IsDoorDelivery)).Do(() =>
			{
				shippingOrder.DeliverTo = consol.GetDeliverToAddress(context, consol.IsDeliver(shippingOrder.IsDoorDelivery));
				AddAddressValidation(shippingOrder);
				ValidateContactAndAddress(shippingOrder, (Address)shippingOrder.DeliverTo, false);
			});

			shippingOrder.ForwardingInstructions = GetNoteText(consol, PredefinedNoteTypes.Instance.ForwardingInstructionNotes.Description) ?? ZString.Empty;
			shippingOrder.ForwardingInstructionsInfo.AddAsciiCharactersValidation();

			shippingOrder.GoodsHandlingInstructions = GetNoteTextWithFallback(PredefinedNoteTypes.Instance.HandlingInstructions.Description, consol, directShipment);
			shippingOrder.GoodsHandlingInstructionsInfo.AddAsciiCharactersValidation();

			shippingOrder.SpecialInstructions = consol.IsDirect
				? GetNoteText(directShipment, PredefinedNoteTypes.Instance.SpecialInstructions.Description) ?? ZString.Empty
				: GetNoteText(consol, PredefinedNoteTypes.Instance.SpecialInstructions.Description) ?? ZString.Empty;
			shippingOrder.SpecialInstructionsInfo.AddAsciiCharactersValidation();

			shippingOrder.IsRequiredSendAttachment =
				OrgHeaderExtensions.GetShippingLineMessagingRequirement(consol.IsCoLoad ? consol.Creditor : consol.ShippingLine, ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage)?.RSR_IsShippingOrder ?? false;

			shippingOrder.PackageGrouping = new CodeDescription(consol.JK_PackageGrouping_List)
			{
				Code = FreightDataRegistry.Instance.EnablePackageGrouping.Value ? consol.JK_PackageGrouping : new ZString(Core.Constants.PackageGrouping.Codes.DoNotGroup)
			};
			IsGroupAndConsolidatePackingLines = shippingOrder.PackageGrouping.Code == Core.Constants.PackageGrouping.Codes.GroupByShipment || shippingOrder.PackageGrouping.Code == Core.Constants.PackageGrouping.Codes.GroupByPackLine;

			PopulateReferences(shippingOrder);
			PopulateCountrySpecificFields(shippingOrder);
			PopulateRouting(shippingOrder);
			PopulateAddresses(shippingOrder, directShipment);
			PopulatePorts(shippingOrder);
			PopulateCharges(shippingOrder);
			PopulateShipments(shippingOrder);
			PopulateContainers(shippingOrder);

			AddValidation(shippingOrder);
			shippingOrder.ValidateAllIncludingChildren();

			return shippingOrder;
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

		#region PopulateReferences

		void PopulateReferences(ShippingOrder shippingOrder)
		{
			shippingOrder.BillOfLadingNumber = consol.JK_MasterBillNum;
			shippingOrder.ShipperReference = consol.JK_AgentsReference;
			shippingOrder.FreightForwarderReference = consol.JK_UniqueConsignRef;

			var quotationNumber = consol?.Numbers?.OfType<CusEntryNumber>().FirstOrDefault(n => n.CE_EntryType == AdditionalReferences.Codes.CarrierQuoteNumber);
			shippingOrder.CarrierContractNumber = quotationNumber != null ? quotationNumber.CE_EntryNum : consol?.JK_CarrierContractNumber ?? ZString.Empty;
			shippingOrder.CarrierContractNumberIsQuotationNumber = quotationNumber != null;

			foreach (var number in consol.Numbers.OfType<CusEntryNumber>())
			{
				switch (number.CE_EntryType)
				{
					case CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount:
						shippingOrder.ContractNamedAccount = number.CE_EntryNum;
						break;

					case CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.LetterOfCreditNumber:
						shippingOrder.LetterOfCredit = number.CE_EntryNum;
						break;

					case ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber:
						shippingOrder.CarrierBookingPrefix = number.CE_EntryNum;
						break;
				}
			}

			shippingOrder.NVOCCReference = GetNVOCCReferenceFromBrancheOrCompany(GlbBranch.CurrentBranch.OrgProxy?.MainAddress, GlbCompany.CurrentCompany.OrgProxy?.MainAddress, consol.Factory, context);
		}

		RegistrationNumber GetNVOCCReferenceFromBrancheOrCompany(OrgAddress address, OrgAddress addressToFallback, BusinessObjectFactory factory, IContext context)
		{
			var result = GetNVOCCReference(address, factory, context);
			if (result != null && result.Value.IsEmpty)
			{
				result = GetNVOCCReference(addressToFallback, factory, context);
			}

			return result;
		}

		RegistrationNumber GetNVOCCReference(OrgAddress address, BusinessObjectFactory factory, IContext context)
		{
			var nvoccLookup = new CodeDescriptionPairList();
			nvoccLookup.AddPair(OrgCusCode.CodeTypes.NVOCCReference, Res.GetString("7625EE0D-DB36-4A9F-9E5D-179A499973AD", "NVOCC Reference"));

			return new RegistrationNumber()
			{
				Value = address.GetRegistrationNumberWithFallbackToOrgHeader(OrgCusCode.CodeTypes.NVOCCReference, Core.Constants.CountryCodes.China),
				Type = new CodeDescription(nvoccLookup)
				{
					Code = OrgCusCode.CodeTypes.NVOCCReference
				},
				CountryOfIssue = new Country(factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.China
				}
			};
		}

		#endregion

		#region PopulateCountrySpecificFields

		public void PopulateCountrySpecificFields(ShippingOrder shippingOrder)
		{
			var lastSeaTransport = consol.Transports?.LastTransportWithTransportMode(Core.Constants.TransportModes.Sea);
			var discCountry = lastSeaTransport?.DiscPort?.RL_RN_NKCountryCode ?? ZString.Empty;

			shippingOrder.IsDischargeInCanadaUSOrUSTerritory = new string[]
			{
				Core.Constants.CountryCodes.UnitedStates,
				Core.Constants.CountryCodes.Canada,
				Core.Constants.CountryCodes.PuertoRico,
				Core.Constants.CountryCodes.Guam,
				Core.Constants.CountryCodes.NorthernMarianaIslands,
				Core.Constants.CountryCodes.VirginIslands,
				Core.Constants.CountryCodes.AmericanSamoa
			}
			.Contains(discCountry.ToString());

			if (shippingOrder.IsDischargeInCanadaUSOrUSTerritory)
			{
				shippingOrder.USCanadaManifestSelfFilerID = GetUSCanadaManifestSelfFilerID(discCountry, consol.SendingForwarder);
				shippingOrder.USCanadaManifestSelfFilerIDInfo.AddAsciiCharactersValidation();
			}
		}

		ZString GetUSCanadaManifestSelfFilerID(ZString discCountry, OrgHeader sendingForwarder)
		{
			if (consol.IsDirect)
			{
				return ZString.Empty;
			}

			var isToUS = discCountry == Core.Constants.CountryCodes.UnitedStates;
			var usSCACCode = sendingForwarder.GetRegistrationNumber(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.CodeTypes.CarrierCode);

			if (isToUS)
			{
				return usSCACCode;
			}
			else
			{
				switch (discCountry)
				{
					case Core.Constants.CountryCodes.Canada:
						var caSCACCode = sendingForwarder.GetRegistrationNumber(Core.Constants.CountryCodes.Canada, OrgCusCode.CodeTypes.CarrierCode);
						return caSCACCode.IsEmpty ? usSCACCode : caSCACCode;
					case Core.Constants.CountryCodes.PuertoRico:
						var prSCACCode = sendingForwarder.GetRegistrationNumber(Core.Constants.CountryCodes.PuertoRico, OrgCusCode.CodeTypes.CarrierCode);
						return prSCACCode.IsEmpty ? usSCACCode : prSCACCode;
					case Core.Constants.CountryCodes.Guam:
						var guSCACCode = sendingForwarder.GetRegistrationNumber(Core.Constants.CountryCodes.Guam, OrgCusCode.CodeTypes.CarrierCode);
						return guSCACCode.IsEmpty ? usSCACCode : guSCACCode;
					case Core.Constants.CountryCodes.NorthernMarianaIslands:
						var mpSCACCode = sendingForwarder.GetRegistrationNumber(Core.Constants.CountryCodes.NorthernMarianaIslands, OrgCusCode.CodeTypes.CarrierCode);
						return mpSCACCode.IsEmpty ? usSCACCode : mpSCACCode;
					case Core.Constants.CountryCodes.VirginIslands:
						var viSCACCode = sendingForwarder.GetRegistrationNumber(Core.Constants.CountryCodes.VirginIslands, OrgCusCode.CodeTypes.CarrierCode);
						return viSCACCode.IsEmpty ? usSCACCode : viSCACCode;
					case Core.Constants.CountryCodes.AmericanSamoa:
						var asSCACCode = sendingForwarder.GetRegistrationNumber(Core.Constants.CountryCodes.AmericanSamoa, OrgCusCode.CodeTypes.CarrierCode);
						return asSCACCode.IsEmpty ? usSCACCode : asSCACCode;
					default:
						return ZString.Empty;
				}
			}
		}

		#endregion

		#region Addresses

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message, non-translatable constants")]
		void PopulateAddresses(ShippingOrder shippingOrder, ForwardingShipment directShipment)
		{
			var shipperAddress = GetShipperAddress(directShipment);
			var shipper = AddressBuilder.Create(context, shipperAddress).AddAsAgentInfoToCompanyName(GetShipperAgentInfo(shipperAddress));

			shippingOrder.Shipper = shipper
				.AddPartyNameAndAddressValidation((NoResString)"Shipper")
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation()
				.AddEmptyCountryCodeValidation();

			var carrier = AddressBuilder.Create(context, consol.ShippingLineAddress, false);

			shippingOrder.Carrier = carrier
				.AddPartyNameAndAddressValidation((NoResString)"Carrier")
				.AddAsciiCharactersValidation()
				.AddEmptyCountryCodeValidation();

			shippingOrder.RecipientType = consol.ShippingLineIsShippingLine ? "Carrier" : "Carrier(NVOCC)";

			var consignee = AddressBuilder.Create(context, GetConsigneeAddress(directShipment));

			shippingOrder.Consignee = consignee
				.AddPartyNameAndAddressValidation((NoResString)"Consignee", () => !consignee.IsToOrder())
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation()
				.AddEmptyCountryCodeValidation()
				.AddToOrderSupport();

			shippingOrder.CarrierHandlingAgent = AddressBuilder.Create(context, consol.CarrierHandlingAgentDocumentaryAddress)
				.AddAsciiCharactersValidation()
				.AddEmptyCountryCodeValidation();

			var carrierBookingAgent = AddressBuilder.Create(context, consol.CarrierBookingAgentDocumentaryAddress);
			AddCarrierBookingAgentValidation(carrierBookingAgent);

			shippingOrder.CarrierBookingAgent = carrierBookingAgent
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation()
				.AddEmptyCountryCodeValidation();

			var doesCarrierHaveENP = HasRegistrationNumber(consol.ShippingLineAddress?.Header,
				Core.Constants.CountryCodes.China,
				OrgCusCode.ChinaCodeTypes.ENP);

			carrier.CompanyNameInfo.AddWarning(() => shippingOrder.CarrierBookingAgent.IsEmpty() && !doesCarrierHaveENP, (NoResString)"This party does not have an ENP Easipass Code in their Organization > Config which is required if the message is to be sent via Easipass.");

			var notifyParty = (consol.IsDirect
				? AddressBuilder.Create(context, directShipment?.NotifyPartyDocumentaryAddress)
				: AddressBuilder.Create(context, consol.NotifyPartyDocumentaryAddress));

			shippingOrder.NotifyParty = notifyParty
				.AddPartyNameAndAddressValidation((NoResString)"Notify Party", () => !notifyParty.IsSameAsConsignee())
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation()
				.AddEmptyCountryCodeValidation()
				.AddSameAsConsigneeSupport();

			EnsureConsigneeAndNotifiPartyRevalidatedwhenSameAsConsignee(consignee, notifyParty);
			EnsureConsigneeAndNotifiPartyRevalidatedwhenToOrder(consignee, notifyParty);

			shippingOrder.NotifyParty2 = (consol.IsDirect
				? AddressBuilder.Create(context, directShipment?.NotifyParty2DocumentaryAddress)
				: AddressBuilder.Create(context, consol.NotifyParty2DocumentaryAddress))
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation()
				.AddEmptyCountryCodeValidation();

			shippingOrder.Forwarder = AddressBuilder.Create(context, consol.SendingForwarderWithContact)
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation()
				.AddEmptyCountryCodeValidation();

			shippingOrder.PickupFrom = consol.GetPickupFromAddress(context, consol.IsPickup(consol.IsDoorPickup()))
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation()
				.AddEmptyCountryCodeValidation();

			shippingOrder.DeliverTo = consol.GetDeliverToAddress(context, consol.IsDeliver(consol.IsDoorDelivery()))
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation()
				.AddEmptyCountryCodeValidation();

			shippingOrder.CurrentUser = AddressBuilder.CreateForCurrentUser(context)
				.AddAsciiCharactersValidation();

			PopulateContactNameAndEmail(shippingOrder);
		}

		void PopulateContactNameAndEmail(ShippingOrder shippingOrder)
		{
			if (!consol.IsCoLoad && shippingOrder.Carrier != null)
			{
				var carrier = consol.ShippingLine;
				var contact = GetDefaultContact(carrier?.FilteredContacts);
				if (carrier?.ShippingLine != null && (!carrier.ShippingLine.RSL_ShippingOrderAvailable || CarrierHasIntegrationViaEmailToCarrierLocalOfficeTag(carrier)) && contact != null)
				{
					shippingOrder.Carrier.Email = contact.OC_Email;
					shippingOrder.Carrier.Contact = contact.OC_ContactName;
				}
			}

			if (!consol.IsCoLoad && shippingOrder.CarrierBookingAgent != null)
			{
				var carrier = consol.CarrierBookingAgent;
				var contact = GetDefaultContact(carrier?.FilteredContacts);
				if (carrier?.ShippingLine != null && (!carrier.ShippingLine.RSL_ShippingOrderAvailable || CarrierHasIntegrationViaEmailToCarrierLocalOfficeTag(carrier)) && contact != null)
				{
					shippingOrder.CarrierBookingAgent.Email = contact.OC_Email;
					shippingOrder.CarrierBookingAgent.Contact = contact.OC_ContactName;
				}
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

		object GetShipperAddress(ForwardingShipment directShipment)
		{
			if (consol.IsDirect)
			{
				return directShipment?.ConsignorDocumentaryAddress;
			}

			if (consol.MasterBillShipperOverrideDocumentaryAddress.IsValidAddress)
			{
				return consol.MasterBillShipperOverrideDocumentaryAddress;
			}
			return consol.SendingForwarderWithContact;
		}

		object GetShipperAgentInfo(object shipperAddress)
		{
			if (consol.IsDirect)
			{
				return shipperAddress;
			}
			return (shipperAddress as ZAddressWithContact)?.OrgAddress as OrgAddress;
		}

		object GetConsigneeAddress(ForwardingShipment directShipment)
		{
			if (consol.IsDirect)
			{
				return directShipment?.ConsigneeDocumentaryAddress;
			}

			if (consol.MasterBillConsigneeOverrideDocumentaryAddress.IsValidAddress)
			{
				return consol.MasterBillConsigneeOverrideDocumentaryAddress;
			}
			return consol.ReceivingForwarderWithContact;
		}

		void AddCarrierBookingAgentValidation(Address carrierBookingAgent)
		{
			var doesCarrierBookingAgentHaveENP = HasRegistrationNumber(consol.CarrierBookingAgentDocumentaryAddress?.Organisation,
				Core.Constants.CountryCodes.China,
				OrgCusCode.ChinaCodeTypes.ENP);

			var doesCarrierBookingAgentHaveEC1C = carrierBookingAgent.HasRegistrationNumber(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode);
			carrierBookingAgent.CompanyNameInfo.AddMessageError(() => !carrierBookingAgent.IsEmpty() && !doesCarrierBookingAgentHaveEC1C, (NoResString)"This party does not have a C1C CargoWiseOne Carrier Code in their Organization > Config, which is required for successful message routing."); // non-translatable validation message

			carrierBookingAgent.CompanyNameInfo.AddWarning(() => carrierBookingAgent.IsEmpty(), (NoResString)"In the absence of Carrier Handling Agent, the message may be routed to the Carrier (subject to eHub configuration)."); // non-translatable validation message
			carrierBookingAgent.CompanyNameInfo.AddWarning(() => !carrierBookingAgent.IsEmpty() && !doesCarrierBookingAgentHaveENP, (NoResString)"This party does not have an ENP Easipass Code in their Organization > Config which is required if the message is to be sent via Easipass.");   // non-translatable validation message
		}

		static void EnsureConsigneeAndNotifiPartyRevalidatedwhenToOrder(Address consignee, Address notifyParty)
		{
			consignee.CompanyNameInfo.AddMessageError(() => (consignee.IsToOrder() || (string.IsNullOrWhiteSpace(consignee.CompanyName) || string.IsNullOrWhiteSpace(consignee.AddressLine1) || string.IsNullOrWhiteSpace(consignee.Country?.Name))) && (string.IsNullOrWhiteSpace(notifyParty.CompanyName) || notifyParty.IsSameAsConsignee()), (NoResString)"Consignee name and address information is required, when Notify Party is empty or SAME AS CONSIGNEE."); // non-translatable validation message
			consignee.CompanyNameInfo.ValueChanged += (e, a) => { consignee.Validate(nameof(consignee.CompanyName)); notifyParty.Validate(nameof(notifyParty.CompanyName)); };
			consignee.AddressLine1Info.ValueChanged += (e, a) => { consignee.Validate(nameof(consignee.CompanyName)); notifyParty.Validate(nameof(notifyParty.CompanyName)); };

			if (consignee.Country is Country consigneeCountry)
			{
				consigneeCountry.OnValueChanged(nameof(consigneeCountry.Name)).Do(() =>
				{
					consignee.Validate(nameof(consignee.CompanyName));
					notifyParty.Validate(nameof(notifyParty.CompanyName));
				});
			}
		}

		static void EnsureConsigneeAndNotifiPartyRevalidatedwhenSameAsConsignee(Address consignee, Address notifyParty)
		{
			notifyParty.CompanyNameInfo.AddMessageError(() => (notifyParty.IsSameAsConsignee() || (string.IsNullOrWhiteSpace(notifyParty.CompanyName) || string.IsNullOrWhiteSpace(notifyParty.AddressLine1) || string.IsNullOrWhiteSpace(notifyParty.Country?.Name))) && (string.IsNullOrWhiteSpace(consignee.CompanyName) || consignee.IsToOrder()), (NoResString)"Notify Party name and address information is required, when Consignee is empty or TO ORDER."); // non-translatable validation message
			notifyParty.CompanyNameInfo.ValueChanged += (e, a) => { consignee.Validate(nameof(consignee.CompanyName)); notifyParty.Validate(nameof(notifyParty.CompanyName)); };
			notifyParty.AddressLine1Info.ValueChanged += (e, a) => { consignee.Validate(nameof(consignee.CompanyName)); notifyParty.Validate(nameof(notifyParty.CompanyName)); };

			if (notifyParty.Country is Country notifyPartyCountry)
			{
				notifyPartyCountry.OnValueChanged(nameof(notifyPartyCountry.Name)).Do(() =>
				{
					consignee.Validate(nameof(consignee.CompanyName));
					notifyParty.Validate(nameof(notifyParty.CompanyName));
				});
			}
		}

		bool HasRegistrationNumber(OrgHeader org, ZString countryCode, ZString type)
		{
			return org != null
				&& org
				.CustomsCodes
				.OfType<OrgCusCode>()
				.Any(c => c.OK_RN_NKCodeCountry == countryCode
					&& c.OK_CodeType == type);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		void AddAddressValidation(ShippingOrder shippingOrder)
		{
			var notifyParty = (Address)shippingOrder.NotifyParty;
			notifyParty.AddPostcodeValidationForUSImports((NoResString)"Notify Party");

			var consignee = (Address)shippingOrder.Consignee;
			notifyParty.AddValidationDependencies(notifyParty.ContactInfo, consignee.CompanyNameInfo);

			var carrier = (Address)shippingOrder.Carrier;
			var carrierBookingAgent = (Address)shippingOrder.CarrierBookingAgent;
			carrier.AddValidationDependencies(carrier.CompanyNameInfo, carrierBookingAgent.CompanyNameInfo);

			const string doorPickupErrorMessage = "Pickup From name and address are mandatory when 'Door Pickup' is selected";
			var pickupFrom = (Address)shippingOrder.PickupFrom;
			pickupFrom.CompanyNameInfo.AddMessageError(() => shippingOrder.IsDoorPickup && pickupFrom.CompanyName.IsEmpty, doorPickupErrorMessage);
			pickupFrom.AddressLine1Info.AddMessageError(() => shippingOrder.IsDoorPickup && pickupFrom.AddressLine1.IsEmpty, doorPickupErrorMessage);

			if (pickupFrom.Country is Country pickupCountry)
			{
				pickupCountry.NameInfo.AddMessageError(() => shippingOrder.IsDoorPickup && pickupCountry.Name.IsEmpty, doorPickupErrorMessage);
			}

			const string doorPickupContactErrorMeesage = "Contact name and Telephone number are mandatory when ‘Door Pickup’ is selected.";
			pickupFrom.ContactInfo.AddMessageError(() => shippingOrder.IsDoorPickup && (string.IsNullOrWhiteSpace(pickupFrom.Contact) || string.IsNullOrWhiteSpace(pickupFrom.Phone)), doorPickupContactErrorMeesage);

			const string doorDeliveryErrorMessage = "Deliver To name and address are mandatory when 'Door Delivery' is selected.";
			var deliverTo = (Address)shippingOrder.DeliverTo;
			deliverTo.CompanyNameInfo.AddMessageError(() => shippingOrder.IsDoorDelivery && deliverTo.CompanyName.IsEmpty, doorDeliveryErrorMessage);
			deliverTo.AddressLine1Info.AddMessageError(() => shippingOrder.IsDoorDelivery && deliverTo.AddressLine1.IsEmpty, doorDeliveryErrorMessage);

			if (deliverTo.Country is Country deliverCountry)
			{
				deliverCountry.NameInfo.AddMessageError(() => shippingOrder.IsDoorDelivery && deliverCountry.Name.IsEmpty, doorDeliveryErrorMessage);
			}

			const string doorDeliveryErrorContactMessage = "Contact name and Telephone number are mandatory when ‘Door Delivery’ is selected.";
			deliverTo.ContactInfo.AddMessageError(() => shippingOrder.IsDoorDelivery && (string.IsNullOrWhiteSpace(deliverTo.Contact) || string.IsNullOrWhiteSpace(deliverTo.Phone)), doorDeliveryErrorContactMessage);

			EnsurePickupDeliveryAddressRevalidatesRelatedPropertiesWhenChanges(shippingOrder, pickupFrom, true);
			EnsurePickupDeliveryAddressRevalidatesRelatedPropertiesWhenChanges(shippingOrder, deliverTo, false);

			if (!consol.IsCoLoad)
			{
				carrier.CompanyNameInfo.AddMessageError(() =>
				{
					return consol.ShippingLine != null && consol.ShippingLine.ShippingLine == null;
				}, ShippingLineMessagingRequirement.ValidationMessages.CarrierLinkShippingLine);

				if (consol.CarrierBookingAgent != null)
				{
					carrierBookingAgent.ContactInfo.AddMessageError(() =>
					{
						if(consol.CarrierBookingAgent?.ShippingLine != null)
						{
							return !consol.CarrierBookingAgent.ShippingLine.RSL_ShippingOrderAvailable && (carrierBookingAgent.Contact.IsEmpty || carrierBookingAgent.Email.IsEmpty);
						}
						else
						{
							return false;
						}
					}, "This carrier does not support electronic Shipping Order. Contact name and email address are required to send your Shipping Order by email.\nPlease setup contact name and email address on Organization> Contact> Email and Receiving Documents> Group SHP");

					((Address)shippingOrder.CarrierBookingAgent).AddValidationDependencies(((Address)shippingOrder.CarrierBookingAgent).ContactInfo, ((Address)shippingOrder.CarrierBookingAgent).EmailInfo);
				}
				else
				{
					carrier.ContactInfo.AddMessageError(() =>
					{
						var consolCarrier = consol.ShippingLine;
						if (consolCarrier?.ShippingLine != null)
						{
							return !consolCarrier.ShippingLine.RSL_ShippingOrderAvailable && (carrier.Contact.IsEmpty || carrier.Email.IsEmpty);
						}
						else
						{
							return false;
						}
					}, "This carrier does not support electronic Shipping Order. Contact name and email address are required to send your Shipping Order by email.\nPlease setup contact name and email address on Organization> Contact> Email and Receiving Documents> Group SHP");

					((Address)shippingOrder.Carrier).AddValidationDependencies(((Address)shippingOrder.Carrier).ContactInfo, ((Address)shippingOrder.Carrier).EmailInfo);
				}
			}

			if (!consol.IsCoLoad && consol?.ShippingLine != null)
			{
				carrier.ContactInfo.AddMessageError(
					() => CarrierHasIntegrationViaEmailToCarrierLocalOfficeTag(consol.ShippingLine) && (carrier.Contact.IsEmpty || carrier.Email.IsEmpty)
					, "This carrier only supports integration via email to local office.\r\nContact name and email address are required to send Shipping Order.\r\nPlease maintain contact name and email address in carrier Organization > Contact > Email and Receiving Documents > Group SHP.");
			}
		}

		#endregion

		#region Routing

		void PopulateRouting(ShippingOrder shippingOrder)
		{
			var transports = consol
					.Transports
					.OfType<Freight.Business.Transport>()
					.ToArray();

			shippingOrder.Transports = Transports.Create(context, transports);

			var firstSeaPort = transports
				.FirstOrDefault(t => t.JW_TransportMode == Core.Constants.TransportModes.Sea);

			var lastSeaPort = transports
				.LastOrDefault(t => t.JW_TransportMode == Core.Constants.TransportModes.Sea);

			shippingOrder.PortOfLoading = Unloco.Create(context, firstSeaPort?.LoadPort, true)
				.AddAsciiCharactersValidation()
				.AddRequiredValidation((NoResString)"Port Of Loading"); // non-translatable validation message

			shippingOrder.PortOfLoading.CodeInfo.ValueChanged += DefaultAndValidateExportHarmonizedCode;

			void DefaultAndValidateExportHarmonizedCode(object sender, EventArgs args)
			{
				foreach (var packingLine in shippingOrder.Containers.SelectMany(c => c.PackingLines).OfType<PackingLine>())
				{
					packingLine.ExportHarmonizedCode = PackingLineBuilder.GetHarmonizedCodesDesc(consol.Factory, packingLine, shippingOrder.PortOfLoading.Code, context);
					packingLine.Validate(nameof(packingLine.ExportHarmonizedCode));
				}
			}

			shippingOrder.PortOfDischarge = Unloco.Create(context, lastSeaPort?.DiscPort, true)
				.AddAsciiCharactersValidation()
				.AddRequiredValidation((NoResString)"Port Of Discharge"); // non-translatable validation message

			shippingOrder.PortOfDischarge.OnValueChanged(nameof(shippingOrder.PortOfDischarge.Code)).Do(() =>
			{
				foreach (var packingLine in shippingOrder.Containers.SelectMany(c => c.PackingLines).OfType<PackingLine>())
				{
					packingLine.ImportHarmonizedCode = PackingLineBuilder.GetHarmonizedCodesDesc(consol.Factory, packingLine, shippingOrder.PortOfDischarge.Code, context);
					packingLine.Validate(nameof(packingLine.ImportHarmonizedCode));
				}
			});

			shippingOrder.Vessel = shippingOrder.Transports.Main?.Vessel;
			if (shippingOrder.Vessel != null && shippingOrder.Vessel is Vessel vessel)
			{
				vessel.NameInfo.AddAsciiCharactersValidation();
			}

			shippingOrder.VoyageFlightNumber = shippingOrder.Transports.Main?.VoyageFlightNumber ?? ZString.Empty;
			shippingOrder.VoyageFlightNumberInfo.AddAsciiCharactersValidation();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		void AddRoutingValidation(ShippingOrder shippingOrder)
		{
			if (shippingOrder.Transports?.Main is Transport mainLeg)
			{
				mainLeg.ETDInfo.AddMessageErrorIfEmpty((NoResString)"ETD is required.");
			}

			foreach (var transport in shippingOrder.Transports.OfType<Transport>())
			{
				if (transport.Mode.Code == "SEA")
				{
					const string vesselVoyageError = "Vessel and Voyage are required when ETD is blank.";

					transport.Vessel.NameInfo.AddMessageError(() => transport.ETD.IsEmpty && transport.Vessel.Name.IsEmpty, vesselVoyageError);
					transport.VoyageFlightNumberInfo.AddMessageError(() => transport.ETD.IsEmpty && transport.VoyageFlightNumber.IsEmpty, vesselVoyageError);
					transport.ETDInfo.AddMessageError(() => transport.ETD.IsEmpty && transport.Vessel.Name.IsEmpty && transport.VoyageFlightNumber.IsEmpty,
(NoResString)"ETD is required when Vessel and Voyage are blank.");
				}

				transport.PortOfLoading.AddRequiredValidation((NoResString)"Port Of Loading");
				transport.PortOfDischarge.AddRequiredValidation((NoResString)"Port Of Discharge");

				EnsureThatVesselVoyageAndETDAreValidatedWhenAnyChanges(transport);
				transport.Carrier.AddAsciiCharactersValidation();

				transport.ETDInfo.AddMessageError(() => transport.ETD > ZDateTime.Now.AddDays(400),
					Res.GetString("a7949072-ba64-437e-837b-16525374fa71", "ETD must not be more than 400 days in advance"));
			}
		}

		void AddTransportsValidation(ShippingOrder shippingOrder)
		{
			var mainTransport = shippingOrder.Transports?.Main;
			if (mainTransport != null)
			{
				((CodeDescription)mainTransport.Mode)?.CodeInfo.AddMessageError(() => mainTransport.Mode.Code != Core.Constants.TransportModes.Sea, (NoResString)"Main transport leg mode must be SEA."); // non-translatable validation message
			}

			foreach (Transport transport in shippingOrder.Transports)
			{
				((CodeDescription)transport.Type)?.DescriptionInfo.AddMessageError(() => mainTransport == null, Res.GetString("73572EE4-99FF-4B80-8911-E4597FE65917", "Main Sea leg is required."));
			}
		}

		#endregion

		#region Ports

		void PopulatePorts(ShippingOrder shippingOrder)
		{
			shippingOrder.CarrierBookingOffice = Unloco.Create(context, consol.CarrierBookingOffice)
				.AddAsciiCharactersValidation();

			shippingOrder.PlaceOfReceipt = Unloco.Create(context, consol.LoadPort, true)
				.AddAsciiCharactersValidation();

			var placeOfIssue = Unloco.Create(context, consol.MasterBillIssuePlace,true)
				.AddAsciiCharactersValidation();

			placeOfIssue.CodeInfo.AddMessageError(() => shippingOrder.PlaceOfIssue.Code.IsEmpty && !shippingOrder.RequestedDateOfIssue.IsEmpty,
(NoResString)"Place of Issue is required if Date of Issue is entered."); // non-translatable validation message
			shippingOrder.PlaceOfIssue = placeOfIssue;

			shippingOrder.PlaceOfDelivery = Unloco.Create(context, consol.DischargePort, true)
				.AddAsciiCharactersValidation();

			shippingOrder.FreightPayableAt = (consol.JK_PrepaidCollect == Core.Constants.PaymentType.Prepaid
				? Unloco.Create(context, consol.LoadPort)
				: Unloco.Create(context, consol.DischargePort))
				.AddAsciiCharactersValidation();

			shippingOrder.OperationalPort = consol.GetOperationalPort(context)
				.AddAsciiCharactersValidation();
		}

		void AddCarrierBookingOfficeValidation(ShippingOrder shippingOrder)
		{
			((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo.AddMessageErrorIfEmpty(Res.GetString("03C3C91F-FA01-40E8-8FC3-BB4CFF591158", "The Carrier Booking Office is mandatory.\r\nPlease provide it on Consol > Details > Docs > Carrier Booking Office."));

			((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo.AddWarning(() =>
				!shippingOrder.CarrierBookingOffice.Code.IsEmpty
				&& shippingOrder.CarrierBookingOffice.Code.SubstringSafe(0, 2) != shippingOrder.PlaceOfReceipt.Code.SubstringSafe(0, 2)
				&& shippingOrder.CarrierBookingOffice.Code.SubstringSafe(0, 2) != shippingOrder.PortOfLoading.Code.SubstringSafe(0, 2),
				Res.GetString("471a9dc1-51c1-47b7-985a-4a1ad48283cc", "Carrier booking office does not match to UNLOCO or country code of Place of Receipt/Port of Loading.\r\nPlease provide a valid Carrier Booking Office to avoid booking rejection by carrier."));

			((Unloco)shippingOrder.CarrierBookingOffice).AddValidationDependencies(((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo, shippingOrder.PlaceOfReceipt.CodeInfo, shippingOrder.PortOfLoading.CodeInfo);
		}

		#endregion

		#region Populate Containers

		void PopulateContainers(ShippingOrder shippingOrder)
		{
			var containers = new List<Container>();
			var containerBuilder = new ContainerBuilder();
			var packLineDOs = shippingOrder.Shipments?.SelectMany(x => x.AllPackingLinesIncludeCoLoad) ?? Array.Empty<PackingLine>();

			foreach (var containerBizObj in consol.Containers.OfType<ForwardingContainer>())
			{
				var container = containerBuilder.Build(containerBizObj, context, GetPackingLines(shippingOrder.PackageGrouping.Code, containerBizObj, packLineDOs));
				container.IsEmpty = containerBizObj.JC_IsEmptyContainer;

				containers.Add(container);
			}

			shippingOrder.Containers = new ReadOnlyCollection<Container>(containers);
		}

		PackingLine[] GetPackingLines(ZString packageGroupingCode, ForwardingContainer containerBizObj, IEnumerable<PackingLine> packLineDOs)
		{
			if (FreightDataRegistry.Instance.EnablePackageGrouping.Value)
			{
				var containerPackLinePKs = containerBizObj.PackLines.Cast<PackLine>().Select(p => p.PK).ToArray();

				return packageGroupingCode == Core.Constants.PackageGrouping.Codes.DoNotGroup
					? packLineDOs.Where(x => containerPackLinePKs.Contains((ZGuid)x.Identifier)).ToArray()
					: packLineDOs.Where(x => containerBizObj.PK.ToString() == new ZString(x.Identifier).SubstringSafe(0, 36)).ToArray();
			}
			else
			{
				var packingLineBuilder = new PackingLineBuilder();
				var packingLines = new List<PackingLine>();

				foreach (var packlineBizObj in containerBizObj.PackLines.OfType<ForwardingPackLine>())
				{
					var packingLine = packingLineBuilder.Build(packlineBizObj);
					packingLine.ExportReferenceNumber = GetBookingNumberWithFallback(packlineBizObj);

					packingLines.Add(packingLine);
				}

				return packingLines.ToArray();
			}
		}

		ZString GetBookingNumberWithFallback(PackLine packLine)
		{
			var number = packLine
				.Shipment
				.Numbers
				.OfType<CusEntryNumber>()
				.FirstOrDefault(n => n.CE_EntryNum == ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber);

			return number?.CE_EntryNum
				?? packLine.JL_ExportRefNumber;
		}

		void AddContainersValidation(ShippingOrder shippingOrder)
		{
			foreach (var container in shippingOrder.Containers.OfType<Container>())
			{
				container.Type.CodeInfo.AddWarning(() =>
				{
					var containerISOType = new ContainerISOType();
					containerISOType.ISOCode = container.Type.ISOCode;
					return !container.Type.ISOCode.IsEmpty && !containerISOType.IsKnown;
				}, (NoResString)"Container type entered does not have a valid ISO equipment code. Enter a valid ISO code to the matching Container Reference file."); // non-translatable validation message
				container.Type.CodeInfo.AddMessageError(() => container.Type.ISOCode.IsEmpty, (NoResString)"This container does not have a valid ISO Code. Enter a Valid ISO Code to the Container Reference File via Consol > Container > Container Type."); // non-translatable validation message
				container.NumberInfo.AddMessageError(() => container.Number.IsEmpty && container.IsShipperOwned, (NoResString)"Container number is required when Shipper Owned."); // non-translatable validation message
				container.NumberInfo.AddMessageError(() => !container.Number.IsEmpty && !container.IsShipperOwned && !ContainerNumberValidation.IsValidContainerNumber(container.Number),
					Res.GetString("44de0705-9825-48eb-9209-65611e458182", "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit."));
				container.PackCountInfo.AddMessageError(() => !container.IsEmpty && container.PackCount == 0, (NoResString)"You have not entered a value or there are unpacked packings with 0 quantity. If this is intended, please flag the container as empty.."); // non-translatable validation message
				((CodeDescription)container.SetTemperature.Unit).CodeInfo.AddMessageError(() => container.HasControlledAtmosphere && container.SetTemperature.Unit.Code.IsEmpty,
(NoResString)"The default temperature has not yet been verified by the user.Please check the temperature and the unit of temperature against the container on the Consol"); // non-translatable validation message
				((CodeDescription)container.AirVentFlow.Unit).CodeInfo.AddMessageError(() => container.HasControlledAtmosphere && container.AirVentFlow.Value != 0 && container.AirVentFlow.Unit.Code.IsEmpty,
(NoResString)"No Air Vent Setting measurement type exists. Ensure each temperature controlled container has one entered on the Containers > Refrigeration tab."); // non-translatable validation message
				((CodeDescription)container.AirVentFlow.Unit).CodeInfo.AddWarning(() => container.AirVentFlow.Value == 0 && !container.AirVentFlow.Unit.Code.IsEmpty,
					(NoResString)"Air Vent is marked as \"Open\".\r\nTo indicate \"Closed\", leave the Consol > Containers > Refrigeration > Air Vent Setting > Unit field blank."); // non-translatable validation message
				((CodeDescription)container.AirVentFlow.Unit).CodeInfo.AddWarning(() => container.AirVentFlow.Value != 0 && container.AirVentFlow.Unit.Code == "2L",
					(NoResString)"Carriers accept airflow only in metric units.\r\nAny value entered as cubic feet per minute (2L) is automatically converted to cubic meters per hour (MQH) during transmission."); // non-translatable validation message

				EnsureThatContainerNumberAndShipperOwnedAreValidatedWhenEitherChanges(container);
				EnsureThatContainerPackCountAndIsEmptyAreValidatedWhenEitherChanges(container);
				EnsureThatContainerHasControlledAtmosphereAndSetTemperatureUnitAreValidatedWhenEitherChanges(container);
				EnsureThatContainerHasControlledAtmosphereAndAirVentFlowUnitAreValidatedWhenEitherChanges(container);

				container.VerifiedByAddress.AddAsciiCharactersValidation();

				AddPacksValidation(container.PackingLines, shippingOrder.PackageGrouping.Code, IsGroupAndConsolidatePackingLines);
			}

			var totalContainers = consol.Containers.Count;
			var totalContainerizedPackingLines = totalContainers <= 0 ? 0 : consol.Containers.OfType<ForwardingContainer>().SelectMany(x => x.PackLines.OfType<PackLine>()).Count();
			shippingOrder.ErrorPlaceholderInfo.AddMessageError(() => totalContainers + totalContainerizedPackingLines <= 0, (NoResString)"Container and Packing Lines details are required for Shipping Order and Amendment messages."); // non-translatable validation message
			shippingOrder.ErrorPlaceholderInfo.AddMessageError(() => totalContainerizedPackingLines > 999, (NoResString)"Maximum 999 packlines can be included in a Shipping Order message."); // non-translatable validation message
		}

		void AddPacksValidation(IEnumerable<IPackingLine> packingLines, ZString packageGroupingCode, ZBool isConsolidatedPackingLine)
		{
			foreach (var pack in packingLines.OfType<PackingLine>())
			{
				((CodeDescription)pack.Weight.Unit).CodeInfo.AddMessageError(() => pack.Weight.Unit.Code.IsEmpty, (NoResString)"Pack weight unit is mandatory."); // non-translatable validation message
				pack.MarksAndNumbersInfo.AddMessageError(() => pack.MarksAndNumbers.IsEmpty, (NoResString)"Marks are required."); // non-translatable validation message
				pack.GoodsDescriptionInfo.AddMessageError(() => pack.GoodsDescription.IsEmpty, (NoResString)"Goods Description is required."); // non-translatable validation message
				pack.MarksAndNumbersInfo.AddAsciiCharactersValidation();
				pack.GoodsDescriptionInfo.AddAsciiCharactersValidation();

				pack.QuantityInfo.AddMessageError(() => pack.AnyPackCountIsZeroInGroupedSubPackLines, $"There are pack lines with zero (0) quantity.\r\nPlease verify in Shipment>Packing>Packs on following Shipments:\r\n{pack.ShipmentIDWithAnyPackCountIsZeroInGroupedSubPackLines}."); // non-translatable validation message
				pack.QuantityInfo.AddMessageError(() => packageGroupingCode == Core.Constants.PackageGrouping.Codes.DoNotGroup && pack.Quantity == 0, (NoResString)"Pack quantity is mandatory."); // non-translatable validation message

				if (!isConsolidatedPackingLine)
				{
					pack.Weight.ValueInfo.AddMessageError(() => pack.Weight.IsNull || pack.Weight.Value <= 0, (NoResString)"Total packing line weight is required. Please enter a value."); // non-translatable registration number
					pack.Weight.ValueInfo.AddWarning(() => pack.AnyPackWeightIsZeroInGroupedSubPackLines, $"There are pack lines with zero (0) Weight.\r\nPlease verify in Shipment>Packing>Weight on following Shipments:\r\n{pack.ShipmentIDWithAnyPackWeightIsZeroInGroupedSubPackLines}."); // non-translatable registration number
				}

				pack.Volume.ValueInfo.AddMessageError(() => packageGroupingCode == Core.Constants.PackageGrouping.Codes.DoNotGroup && (pack.Volume.IsNull || pack.Volume.Value <= 0), (NoResString)"The total packing line volume is zero. Please enter a value."); // non-translatable registration number
				pack.Volume.ValueInfo.AddMessageError(() => pack.AnyPackVolumeIsZeroInGroupedSubPackLines, $"There are pack lines with zero (0) Volume.\r\nPlease verify in Shipment>Packing>Volume on following Shipments:\r\n{pack.ShipmentIDWithAnyPackVolumeIsZeroInGroupedSubPackLines}."); // non-translatable registration number

				if (pack.DangerousGoods != null && pack.DangerousGoods.Any())
				{
					foreach (var dg in pack.DangerousGoods)
					{
						dg.Contact.FullNameInfo.AddMessageError(() => string.IsNullOrWhiteSpace(dg.Contact.FullName), (NoResString)"Contact Name is required for dangerous goods."); // non-translatable validation message
						dg.Contact.PhoneInfo.AddMessageError(() => string.IsNullOrWhiteSpace(dg.Contact.Phone), (NoResString)"Contact Phone is required for dangerous goods."); // non-translatable validation message

						dg.Validator = () => AddDangerousGoodsValidation(dg);
					}
				}

				var harmonizedCode = pack.HarmonizedCode as HarmonizedCode;
				var importHarmonizedCode = pack.ImportHarmonizedCode as HarmonizedCode;
				var exportHarmonizedCode = pack.ExportHarmonizedCode as HarmonizedCode;

				if ((importHarmonizedCode?.Country?.Code ?? ZString.Empty) != (exportHarmonizedCode?.Country?.Code ?? ZString.Empty)
					&& (importHarmonizedCode?.Country?.Code ?? ZString.Empty) == Core.Constants.CountryCodes.Malaysia)
				{
					harmonizedCode.CodeInfo.AddWarning(() => harmonizedCode.Code.IsEmpty && importHarmonizedCode.Code.IsEmpty, (NoResString)"It is recommended to fill in Harmonized Code for Sea imports to Malaysia."); // non-translatable validation message
				}
			}
		}

		static IEnumerable<string> AddDangerousGoodsValidation(DangerousGood dangerousGood)
		{
			if (dangerousGood.IMOClass.IsEmpty || dangerousGood.Code.IsEmpty || dangerousGood.ProperShippingName.IsEmpty)
			{
				yield return (NoResString)"DG Class, UNDG and Proper Shipping Name are required for dangerous goods.\r\nPlease enter Shipment > Packing > Pack Lines > Dangerous Goods > DG Substance."; // non-translatable registration number
			}
		}

		#endregion

		#region Shipments

		void PopulateShipments(ShippingOrder shippingOrder)
		{
			if (FreightDataRegistry.Instance.EnablePackageGrouping.Value)
			{
				var shipmentDOs = new List<Shipment>();
				var shipmentBuilder = new ShipmentBuilder(context);

				foreach (ForwardingShipment shipment in consol.TopLevelShipments)
				{
					var topLevelShipmentPackType = packageGroupingHelper.GetTopLevelShipmentPackType(shipment);

					shipmentDOs.Add(shipmentBuilder.Build(shipment, s => GetProcessedPackingLines(s, shippingOrder, topLevelShipmentPackType)));
				}

				shippingOrder.Shipments = shipmentDOs;

				packageGroupingHelper.PopulateGroupedAndConsolidatedPackingLines(shippingOrder.Shipments, shippingOrder.PackageGrouping.Code, true, shippingOrder.DocumentName);
				packageGroupingHelper.PopulateConsolidatedPackingLineDangerousGoodsForDoNotGroup(shippingOrder.Shipments, shippingOrder.PackageGrouping.Code, shippingOrder.DocumentName);
			}
		}

		IEnumerable<PackingLine> GetProcessedPackingLines(ForwardingShipment shipment, ShippingOrder shippingOrder, ZString topLevelShipmentPackType)
		{
			var packLineBuilder = new PackingLineBuilder();
			var packLineDOs = new List<PackingLine>();

			foreach (PackLine packLineBO in shipment.OuterPackLines)
			{
				var packingLineDO = packLineBuilder.Build(packLineBO, consol: consol);
				packingLineDO.ExportReferenceNumber = GetBookingNumberWithFallback(packLineBO);
				packageGroupingHelper.PopulatePackingQuantityAndPackageType(shippingOrder.PackageGrouping.Code, packLineBO, packingLineDO, topLevelShipmentPackType);

				packLineDOs.Add(packingLineDO);
			}

			return packLineDOs;
		}

		void AddPackingLinesValidation(ShippingOrder shippingOrder)
		{
			if (shippingOrder.Shipments != null && shippingOrder.Shipments.Any())
			{
				AddPacksValidation(shippingOrder.Shipments.SelectMany(x => x.AllPackingLinesIncludeCoLoad), shippingOrder.PackageGrouping.Code, IsGroupAndConsolidatePackingLines);

				if (IsGroupAndConsolidatePackingLines)
				{
					AddPacksValidation(shippingOrder.Shipments.SelectMany(x => x.PackingLines), shippingOrder.PackageGrouping.Code, false);

					foreach (var packingLine in shippingOrder.Shipments.SelectMany(x => x.AllPackingLinesIncludeCoLoad))
					{
						packingLine.Weight.ValueInfo.AddMessageError(() => packingLine.Weight.IsNull || packingLine.Weight.Value <= 0,
(NoResString)"No weight has been allocated to this container.\r\nPlease verify in Shipment > Packing > Weight."); // non-translatable registration number
					}
				}
				else
				{
					foreach (var packingLine in shippingOrder.Shipments.SelectMany(x => x.AllPackingLinesIncludeCoLoad))
					{
						if (FreightDataRegistry.Instance.EnablePackageGrouping.Value)
						{
							packageGroupingHelper.AddNoInnerPackLineValidation(packingLine, new List<PackingLine> { packingLine });
						}
					}
				}
			}
		}

		#endregion

		#region IsGroupAndConsolidatePackingLines

		ZBool IsGroupAndConsolidatePackingLines { get; set; }

		#endregion

		#region Charges

		void PopulateCharges(ShippingOrder shippingOrder)
		{
			if (!consol.JK_PrepaidCollect.IsEmpty)
			{
				shippingOrder.IsFreightPrepaid = consol.JK_PrepaidCollect == Core.Constants.PaymentType.Prepaid;
				shippingOrder.IsFreightCollect = consol.JK_PrepaidCollect == Core.Constants.PaymentType.Collect;
			}

			shippingOrder.OtherCharges = new OtherCharges
			{
				IsPrepaid = !consol.JK_PrepaidCollect.IsEmpty && shippingOrder.IsFreightPrepaid,
				IsCollect = !consol.JK_PrepaidCollect.IsEmpty && shippingOrder.IsFreightCollect
			};

			shippingOrder.OptionalChargeDestinationHaulage = new OptionalCharge();
			shippingOrder.OptionalChargeDestinationPort = new OptionalCharge();
			shippingOrder.OptionalChargeOriginHaulage = new OptionalCharge();
			shippingOrder.OptionalChargeOriginPort = new OptionalCharge();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		void AddChargesValidation(ShippingOrder shippingOrder)
		{
			var otherCharges = (OtherCharges)shippingOrder.OtherCharges;

			const string paymentDetailsTypeErrorMessage = "At least one Payment Details type must be selected.";
			bool IsPaymentDetailsTypeMissing() => otherCharges.Remarks.IsEmpty && !otherCharges.IsPrepaid && !otherCharges.IsCollect && !otherCharges.IsFree && !otherCharges.IsPayableElsewhere && !otherCharges.IsFirstLinePrepaidLineSecondCollect;

			otherCharges.IsPrepaidInfo.AddMessageError(IsPaymentDetailsTypeMissing, paymentDetailsTypeErrorMessage);
			otherCharges.IsCollectInfo.AddMessageError(IsPaymentDetailsTypeMissing, paymentDetailsTypeErrorMessage);
			otherCharges.IsFreeInfo.AddMessageError(IsPaymentDetailsTypeMissing, paymentDetailsTypeErrorMessage);
			otherCharges.IsPayableElsewhereInfo.AddMessageError(IsPaymentDetailsTypeMissing, paymentDetailsTypeErrorMessage);
			otherCharges.IsFirstLinePrepaidLineSecondCollectInfo.AddMessageError(IsPaymentDetailsTypeMissing, paymentDetailsTypeErrorMessage);
			otherCharges.RemarksInfo.AddMessageError(IsPaymentDetailsTypeMissing, paymentDetailsTypeErrorMessage);

			var freightPayableAt = (Unloco)shippingOrder.FreightPayableAt;
			freightPayableAt.CodeInfo.AddMessageErrorIfEmpty((NoResString)"The location of where freight is paid is required.");

			EnsureThatAllPaymentDetailsTypesAreValidatedWhenAnyChanges(otherCharges);
		}

		#endregion

		#region Notes

		ZString GetNoteTextWithFallback(string description, params IStmNoteParent[] parents)
		{
			return parents
				.Select(p => GetNoteText(p, description))
				.FirstOrDefault(t => t.HasValue)
				?? ZString.Empty;
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

		#region Validation

		void AddValidation(ShippingOrder shippingOrder)
		{
			var carrierSCAC = consol.ShippingLineAddress?.Header.GetRegistrationNumber(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.CodeTypes.CarrierCode) ?? ZString.Empty;
			if (carrierSCAC == DocDataConstants.CarrierSCACCodes.One)
			{
				shippingOrder.CarrierBookingReferenceInfo.AddMessageErrorIfEmpty(FormattableString.Invariant($"For carrier {consol.ShippingLineAddress?.Header.OH_Code} Carrier Booking Reference is mandatory.")); // non-translatable validation message
			}

			shippingOrder.ErrorPlaceholderInfo.AddMessageError(() => !consol.Shipments.OfType<ForwardingShipment>().Any(), (NoResString)"There are no shipments attached to the Consolidation."); // non-translatable validation message
			shippingOrder.NumberOfCopiesInfo.AddMessageError(() => shippingOrder.ReleaseType.Code == ShippingInstructionReleaseTypes.Codes.SeaWaybill && shippingOrder.NumberOfCopies == 0, (NoResString)"No of Copies are required."); // non-translatable validation message
			shippingOrder.RequestedDateOfIssueInfo.AddMessageError(() => !shippingOrder.PlaceOfIssue.Code.IsEmpty && shippingOrder.RequestedDateOfIssue.IsEmpty,
(NoResString)"Date of Issue is required if Place of Issue is entered."); // non-translatable validation message

			AddRoutingValidation(shippingOrder);
			AddTransportsValidation(shippingOrder);
			AddContainersValidation(shippingOrder);
			AddPackingLinesValidation(shippingOrder);
			AddChargesValidation(shippingOrder);
			AddAddressValidation(shippingOrder);
			AddCarrierBookingOfficeValidation(shippingOrder);

			EnsureThatDateofIssueAndPortOfIssueAreValidatedWhenEitherChanges(shippingOrder);

			AddShippingLineMessagingRequirementsValidation(shippingOrder);

			AddCarrierMessagingRequirementsValidation(shippingOrder);
		}

		void AddCarrierMessagingRequirementsValidation(ShippingOrder shippingOrder)
		{
			var messagingRequirement = ((consol.IsCoLoad ? consol.JK_OA_CreditorAddress_ZAddress.OrgHeader : consol.JK_OA_ShippingLineAddress_ZAddress.OrgHeader) as OrgHeader)
				?.ShippingLine?.ShippingLineMessagingRequirements.OfType<RefShippingLineMessagingRequirement>().FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.HarmonisedCode);

			if (messagingRequirement?.RSR_IsShippingOrder ?? false)
			{
				shippingOrder.Containers.ForEach(container =>
				{
					container.PackingLines.ForEach(packingLine =>
					{
						((HarmonizedCode)packingLine.HarmonizedCode).CodeInfo.AddMessageError(() => ((HarmonizedCode)packingLine.HarmonizedCode).Code.IsEmpty
						&& ((HarmonizedCode)packingLine.ImportHarmonizedCode).Code.IsEmpty
						&& ((HarmonizedCode)packingLine.ExportHarmonizedCode).Code.IsEmpty,
						Res.GetString("a8335d83-a9ae-425c-8919-da88f0debf29", "The carrier requires HS code for each pack line."));
					});
				});
			}
		}

		void EnsureThatDateofIssueAndPortOfIssueAreValidatedWhenEitherChanges(ShippingOrder shippingOrder)
		{
			if (shippingOrder.PlaceOfIssue is Unloco placeOfIssue)
			{
				shippingOrder.OnValueChanged(nameof(shippingOrder.RequestedDateOfIssue)).Do(ValidateDateAndPlaceOfIssue);
				placeOfIssue.OnValueChanged(nameof(placeOfIssue.Code)).Do(ValidateDateAndPlaceOfIssue);

				void ValidateDateAndPlaceOfIssue()
				{
					shippingOrder.Validate(nameof(shippingOrder.RequestedDateOfIssue));
					placeOfIssue.Validate(nameof(placeOfIssue.Code));
				}
			}
		}

		void EnsureThatVesselVoyageAndETDAreValidatedWhenAnyChanges(Transport transport)
		{
			transport.Vessel.OnValueChanged(nameof(transport.Vessel.Name)).Do(ValidateVesselVoyageAndETD);
			transport.OnValueChanged(nameof(transport.VoyageFlightNumber)).Do(ValidateVesselVoyageAndETD);
			transport.OnValueChanged(nameof(transport.ETD)).Do(ValidateVesselVoyageAndETD);

			void ValidateVesselVoyageAndETD()
			{
				transport.Vessel.Validate(nameof(transport.Vessel.Name));
				transport.Validate(nameof(transport.VoyageFlightNumber));
				transport.Validate(nameof(transport.ETD));
			}
		}

		void EnsureThatContainerNumberAndShipperOwnedAreValidatedWhenEitherChanges(Container container)
		{
			container.OnValueChanged(nameof(container.Number)).Validate(nameof(container.Number), nameof(container.IsShipperOwned));
			container.OnValueChanged(nameof(container.IsShipperOwned)).Validate(nameof(container.Number), nameof(container.IsShipperOwned));
		}

		void EnsureThatContainerPackCountAndIsEmptyAreValidatedWhenEitherChanges(Container container)
		{
			void ValidateContainerPackCountAndIsEmpty(object sender, EventArgs args)
			{
				container.Validate(nameof(container.PackCount));
				container.Validate(nameof(container.IsEmpty));
			}

			container.PackCountInfo.ValueChanged += ValidateContainerPackCountAndIsEmpty;
			container.IsEmptyInfo.ValueChanged += ValidateContainerPackCountAndIsEmpty;
		}

		void EnsureThatContainerHasControlledAtmosphereAndSetTemperatureUnitAreValidatedWhenEitherChanges(Container container)
		{
			var setTemperatureUnit = (CodeDescription)container.SetTemperature.Unit;
			container.OnValueChanged(nameof(container.HasControlledAtmosphere)).Do(ValidateContainerHasControlledAtmosphereAndSetTemperatureUnit);
			setTemperatureUnit.OnValueChanged(nameof(StevedoreTerminalType.Codes)).Do(ValidateContainerHasControlledAtmosphereAndSetTemperatureUnit);

			void ValidateContainerHasControlledAtmosphereAndSetTemperatureUnit()
			{
				container.Validate(nameof(container.HasControlledAtmosphere));
				setTemperatureUnit.Validate(nameof(setTemperatureUnit.Code));
			}
		}

		void EnsureThatContainerHasControlledAtmosphereAndAirVentFlowUnitAreValidatedWhenEitherChanges(Container container)
		{
			var airVentFlowUnit = (CodeDescription)container.AirVentFlow.Unit;

			container.OnValueChanged(nameof(container.HasControlledAtmosphere)).Do(ValidateContainerHasControlledAtmosphereAndAirVentFlowUnit);
			container.AirVentFlow.OnValueChanged(nameof(container.AirVentFlow.Value)).Do(ValidateContainerHasControlledAtmosphereAndAirVentFlowUnit);
			airVentFlowUnit.OnValueChanged(nameof(airVentFlowUnit.Code)).Do(ValidateContainerHasControlledAtmosphereAndAirVentFlowUnit);

			void ValidateContainerHasControlledAtmosphereAndAirVentFlowUnit()
			{
				container.Validate(nameof(container.HasControlledAtmosphere));
				airVentFlowUnit.Validate(nameof(airVentFlowUnit.Code));
			}

			airVentFlowUnit.CodeInfo.AddMessageError(() => container.HasControlledAtmosphere && container.AirVentFlow.Value != 0 && container.AirVentFlow.Unit.Code != "2L" && container.AirVentFlow.Unit.Code != "MQH" && container.AirVentFlow.Unit.Code != ZString.Empty, Res.GetString("FD34D96E-8621-47E7-A695-7993D3A09972", "Selected Air Vent Setting measurement type is not supported by the carrier. Ensure each temperature controlled container has selected either '2L' or 'MQH' on the Containers > Refrigeration tab."));
		}

		void EnsurePickupDeliveryAddressRevalidatesRelatedPropertiesWhenChanges(ShippingOrder shippingOrder, Address address, bool isPickup)
		{
			address.OnValueChanged(nameof(address.CompanyName)).Do(() => ValidateContactAndAddress(shippingOrder, address, isPickup));
			address.OnValueChanged(nameof(address.AddressLine1)).Do(() => ValidateContactAndAddress(shippingOrder, address, isPickup));
			address.OnValueChanged(nameof(address.Contact)).Do(() => ValidateContactAndAddress(shippingOrder, address, isPickup));
			address.OnValueChanged(nameof(address.Phone)).Do(() => ValidateContactAndAddress(shippingOrder, address, isPickup));

			if (address.Country is Country fromCountry)
			{
				fromCountry.OnValueChanged(nameof(fromCountry.Name)).Do(() => ValidateContactAndAddress(shippingOrder, address, isPickup));
			}
		}

		void ValidateContactAndAddress(ShippingOrder shippingOrder, Address address, bool isPickup)
		{
			address.Validate(nameof(address.CompanyName));
			address.Validate(nameof(address.AddressLine1));
			address.Validate(nameof(address.Contact));
			address.Validate(nameof(address.Phone));

			if (address.Country is Country country)
			{
				country.Validate(nameof(country.Name));
			}

			shippingOrder.Validate(isPickup ? nameof(shippingOrder.IsDoorPickup) : nameof(shippingOrder.IsDoorDelivery));
		}

		void EnsureThatAllPaymentDetailsTypesAreValidatedWhenAnyChanges(OtherCharges otherCharges)
		{
			otherCharges.OnValueChanged(nameof(otherCharges.IsPrepaid)).Validate(nameof(otherCharges.IsPrepaid), nameof(otherCharges.IsCollect), nameof(otherCharges.IsFree), nameof(otherCharges.IsPayableElsewhere), nameof(otherCharges.IsFirstLinePrepaidLineSecondCollect), nameof(otherCharges.Remarks));
			otherCharges.OnValueChanged(nameof(otherCharges.IsCollect)).Validate(nameof(otherCharges.IsPrepaid), nameof(otherCharges.IsCollect), nameof(otherCharges.IsFree), nameof(otherCharges.IsPayableElsewhere), nameof(otherCharges.IsFirstLinePrepaidLineSecondCollect), nameof(otherCharges.Remarks));
			otherCharges.OnValueChanged(nameof(otherCharges.IsFree)).Validate(nameof(otherCharges.IsPrepaid), nameof(otherCharges.IsCollect), nameof(otherCharges.IsFree), nameof(otherCharges.IsPayableElsewhere), nameof(otherCharges.IsFirstLinePrepaidLineSecondCollect), nameof(otherCharges.Remarks));
			otherCharges.OnValueChanged(nameof(otherCharges.IsPayableElsewhere)).Validate(nameof(otherCharges.IsPrepaid), nameof(otherCharges.IsCollect), nameof(otherCharges.IsFree), nameof(otherCharges.IsPayableElsewhere), nameof(otherCharges.IsFirstLinePrepaidLineSecondCollect), nameof(otherCharges.Remarks));
			otherCharges.OnValueChanged(nameof(otherCharges.IsFirstLinePrepaidLineSecondCollect)).Validate(nameof(otherCharges.IsPrepaid), nameof(otherCharges.IsCollect), nameof(otherCharges.IsFree), nameof(otherCharges.IsPayableElsewhere), nameof(otherCharges.IsFirstLinePrepaidLineSecondCollect), nameof(otherCharges.Remarks));
			otherCharges.OnValueChanged(nameof(otherCharges.Remarks)).Validate(nameof(otherCharges.IsPrepaid), nameof(otherCharges.IsCollect), nameof(otherCharges.IsFree), nameof(otherCharges.IsPayableElsewhere), nameof(otherCharges.IsFirstLinePrepaidLineSecondCollect), nameof(otherCharges.Remarks));
		}

		#region ShippingLineMessagingRequirements Validation

		void AddShippingLineMessagingRequirementsValidation(ShippingOrder shippingOrder)
		{
			var isContractNumberMandatory = false;
			var shippingLine = consol.ShippingLine?.ShippingLine;

			if (shippingLine != null && shippingLine.RSL_ShippingOrderAvailable)
			{
				var requirements = shippingLine.ShippingLineMessagingRequirements;
				foreach (var requirement in requirements.Where(x => x.RSR_IsShippingOrder))
				{
					switch (requirement.RSR_RST_NKType)
					{
						case ShippingLineMessagingRequirement.Types.ContractNumberMandatory:
							shippingOrder.CarrierContractNumberInfo.AddMessageError(() => shippingOrder.CarrierContractNumber.IsEmpty, ShippingLineMessagingRequirement.ValidationMessages.ContractNumberMandatory);
							isContractNumberMandatory = true;
							break;
						case ShippingLineMessagingRequirement.Types.NamedAccountMandatory:
							shippingOrder.ContractNamedAccountInfo.AddMessageError(() => shippingOrder.ContractNamedAccount.IsEmpty, ShippingLineMessagingRequirement.ValidationMessages.NamedAccountMandatory);
							break;
						case ShippingLineMessagingRequirement.Types.DGNetWeightMandatory:
							AddDangerousGoodsNetWeightValidation(shippingOrder);
							break;
						case ShippingLineMessagingRequirement.Types.AcceptEitherAirflowOrHumidity:
							AddContainersAirflowOrHumidityValidation(shippingOrder);
							break;
						case ShippingLineMessagingRequirement.Types.DimensionsMandatoryForOOG:
							AddPackingLinesDimensionsValidation(shippingOrder);
							break;
					}
				}
			}

			if (!isContractNumberMandatory)
			{
				shippingOrder.CarrierContractNumberInfo.AddWarningIfEmpty(Res.GetString("B45CEBE9-FC6B-4D4B-AD3D-58F894D5EBBB", "It is recommended to fill in Carrier Contract or Quote Number to assist with faster booking and reconciliation processes."));
			}
		}

		void AddDangerousGoodsNetWeightValidation(ShippingOrder shippingOrder)
		{
			foreach (DangerousGood dangerousGood in shippingOrder.Containers.SelectMany(s => s.PackingLines).SelectMany(p => p.DangerousGoods))
			{
				var weight = dangerousGood.Weight;
				weight.ValueInfo.AddMessageError(() => weight.Value <= 0 || weight.Unit.Code.IsEmpty, ShippingLineMessagingRequirement.ValidationMessages.DGNetWeightMandatory);
			}
		}

		void AddContainersAirflowOrHumidityValidation(ShippingOrder shippingOrder)
		{
			foreach (var (humidity, airVentFlow) in shippingOrder.Containers.OfType<Container>().Where(c => c.HasControlledAtmosphere).Select(c => (c.Humidity, c.AirVentFlow)))
			{
				var hasHumidity = humidity.Value > 0;
				var hasAirVentFlow = !airVentFlow.Unit.Code.IsEmpty;
				var rule = (hasHumidity && hasAirVentFlow) || (!hasHumidity && !hasAirVentFlow && airVentFlow.Value != 0);

				humidity.ValueInfo.AddMessageError(() => rule, ShippingLineMessagingRequirement.ValidationMessages.AcceptEitherAirflowOrHumidity);
				airVentFlow.ValueInfo.AddMessageError(() => rule, ShippingLineMessagingRequirement.ValidationMessages.AcceptEitherAirflowOrHumidity);
			}
		}

		void AddPackingLinesDimensionsValidation(ShippingOrder shippingOrder)
		{
			var flatRackOrOpenTopContainers = shippingOrder.Containers
				.Where(c => c.Type.ISOCode.Length > 2 && (c.Type.ISOCode[2].Equals(ShippingLineMessagingRequirement.ContainerTypes.OpenTop) || c.Type.ISOCode[2].Equals(ShippingLineMessagingRequirement.ContainerTypes.FlatRack)));

			foreach (var (length, width, height) in flatRackOrOpenTopContainers.SelectMany(c => c.PackingLines).OfType<PackingLine>().Select(p => (p.Length, p.Width, p.Height)))
			{
				var rule = length.IsNull || length.Value <= 0 || width.IsNull || width.Value <= 0 || height.IsNull || height.Value <= 0;

				length.ValueInfo.AddMessageError(() => rule, ShippingLineMessagingRequirement.ValidationMessages.DimensionsMandatoryForOOG);
				width.ValueInfo.AddMessageError(() => rule, ShippingLineMessagingRequirement.ValidationMessages.DimensionsMandatoryForOOG);
				height.ValueInfo.AddMessageError(() => rule, ShippingLineMessagingRequirement.ValidationMessages.DimensionsMandatoryForOOG);
			}
		}

		#endregion

		#endregion

		ZBool CarrierHasIntegrationViaEmailToCarrierLocalOfficeTag(OrgHeader org)
		{
			return org.GetShippingLineMessagingRequirement(ShippingLineMessagingRequirement.Types.IntegrationViaEmailToCarrierLocalOffice)?.RSR_IsShippingOrder ?? ZBool.False;
		}
	}
}
