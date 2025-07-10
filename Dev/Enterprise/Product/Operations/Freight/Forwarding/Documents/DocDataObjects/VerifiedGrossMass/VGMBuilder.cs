using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.VGMValidation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;
using ResString = Enterprise.Freight.Forwarding.Documents.DataObjects.ResString;
using ShippingLineMessagingRequirement = Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants.ShippingLineMessagingRequirement;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class VGMBuilder
	{
		public VGMBuilder(ForwardingConsol consol, IDocDataObjectParameters parameters)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
			context = new ContextWithCarrierUnlocoMapping(consol.Factory.GetCachedReadOnlyFactory(), consol.IsCoLoad ? consol.CreditorPK : consol.ShippingLinePK);

			this.parameters = parameters;
		}

		readonly ForwardingConsol consol;
		readonly IContext context;
		readonly IDocDataObjectParameters parameters;

		public VerifiedGrossMass Build()
		{
			var containersToShow = parameters?.Data as IReadOnlyCollection<ForwardingContainer> ?? System.Array.Empty<ForwardingContainer>();
			var title = parameters?.DocumentTitle ?? (NoResString)"Verified Gross Container Weight"; // no need for translation of title

			var verifiedGrossMass = new VerifiedGrossMass(nameof(ForwardingConsol), consol.CarrierShipperReferenceWithFallback)
			{
				Title = title,
				FreightForwardersReference = consol.JK_UniqueConsignRef,
				BillOfLadingNumber = consol.JK_MasterBillNum,
				CarrierBookingReference = consol.JK_BookingReference
			};

			var fclModes = new[]
			{
				ContainerModes.FCL,
				ContainerModes.Groupage,
				ContainerModes.BuyersConsol,
				ContainerModes.ShippersConsol,
				ContainerModes.Other
			};

			verifiedGrossMass.ContainerMode = new CodeDescription(consol.JK_ConsolMode_List)
			{
				Code = fclModes.Contains<string>(consol?.JK_ConsolMode)
					? (ZString)ContainerModes.FCL
					: (consol?.JK_ConsolMode ?? ZString.Empty)
			};

			verifiedGrossMass.ShipmentType = new CodeDescription(consol.JK_AgentType_List)
			{
				Code = consol.JK_AgentType
			};

			verifiedGrossMass.OperationalPort = consol.GetOperationalPort(context);

			if (FirstSeaTransport?.LoadPort?.RL_RN_NKCountryCode.ToString() == CountryCodes.China)
			{
				var firstSeaLegForChina = Transport.Create(context, FirstSeaTransport);
				verifiedGrossMass.FirstSeaLegForChina = firstSeaLegForChina;

				firstSeaLegForChina.Vessel.LloydsIMOInfo.AddMessageError(() => string.IsNullOrWhiteSpace(firstSeaLegForChina.Vessel.LloydsIMO) && consol.LoadPort.Code.IsNingboPort(), (NoResString)"Vessel's Lloyds/IMO is mandatory."); // Non-Translatable validation message
				firstSeaLegForChina.VoyageFlightNumberInfo.AddAsciiCharactersValidation();
			}

			PopulateAddresses(verifiedGrossMass);
			PopulateSignature(verifiedGrossMass);
			PopulateContainers(verifiedGrossMass, containersToShow, consol.GetExportLegFromChina() != null);
			PopulateCarrierMessagingRequirements(verifiedGrossMass);

			verifiedGrossMass.TitleInfo.AddMessageError(() => !verifiedGrossMass.Containers.Any(), (NoResString)"You need at least one non empty container selected in order to send this document."); // Non-Translatable validation message

			verifiedGrossMass.CarrierBookingReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("30FD8D9C-6328-4F7B-84B8-8A663099357F", "Carrier Booking Reference is required."));

			AddShippingLineMessagingRequirementsValidation(verifiedGrossMass);

			verifiedGrossMass.ValidateAllIncludingChildren();

			return verifiedGrossMass;
		}

		#region Addresses

		void PopulateAddresses(VerifiedGrossMass verifiedGrossMass)
		{
			verifiedGrossMass.Shipper = consol.IsDirect
				? AddressBuilder.Create(context, consol.DirectShipment?.ConsignorDocumentaryAddress).AddAsAgentInfoToCompanyName(consol.DirectShipment?.ConsignorDocumentaryAddress)
				: AddressBuilder.Create(context, consol.SendingForwarderAddress).AddAsAgentInfoToCompanyName(consol.SendingForwarderAddress);

			verifiedGrossMass.FreightForwarder = consol.IsCoLoad
				? AddressBuilder.Create(context, consol.CreditorAddress)
				: AddressBuilder.Create(context, consol.SendingForwarderAddress);

			verifiedGrossMass.Consignee = consol.IsDirect
				? AddressBuilder.Create(context, consol.DirectShipment?.ConsigneeDocumentaryAddress)
				: AddressBuilder.Create(context, consol.ReceivingForwarderAddress);

			verifiedGrossMass.Carrier = AddressBuilder.Create(context, consol.ShippingLineAddress);
			verifiedGrossMass.CarrierHandlingAgent = AddressBuilder.Create(context, consol.CarrierHandlingAgentDocumentaryAddress);
			verifiedGrossMass.CarrierBookingAgent = AddressBuilder.Create(context, consol.CarrierBookingAgentDocumentaryAddress);
			verifiedGrossMass.CurrentUser = AddressBuilder.CreateForCurrentUser(context);

			PopulateContactNameAndEmail(verifiedGrossMass);

			AddAddressValidation(verifiedGrossMass);
		}

		void PopulateContactNameAndEmail(VerifiedGrossMass verifiedGrossMass)
		{
			var carrier = consol.IsCoLoad ? consol.Creditor : consol.ShippingLine;
			var vgmCarrier = consol.IsCoLoad ? verifiedGrossMass.FreightForwarder : verifiedGrossMass.Carrier;
			var contact = GetDefaultContact(carrier?.FilteredContacts);
			if (contact != null && ((!carrier?.ShippingLine?.RSL_VerifiedGrossContainerWeightAvailable ?? false) || (!consol.IsCoLoad && CarrierHasIntegrationViaEmailToCarrierLocalOfficeTag(consol.ShippingLine))))
			{
				vgmCarrier.Email = contact.OC_Email;
				vgmCarrier.Contact = contact.OC_ContactName;
			}
		}

		OrgContact GetDefaultContact(FilteredContactsCollection contacts)
		{
			OrgContact matched = null;
			if (contacts != null)
			{
				var matchedVGMdocumentGroupContracts = contacts.Cast<OrgContact>().Where(contact => !contact.OC_Email.IsEmpty && !contact.OC_ContactName.IsEmpty &&
					contact.Documents.Cast<OrgDocument>().Any(document => document.OD_DocumentGroup == "VGM"));

				var matchedSHPdocumentGroupContracts = contacts.Cast<OrgContact>().Where(contact => !contact.OC_Email.IsEmpty && !contact.OC_ContactName.IsEmpty &&
					contact.Documents.Cast<OrgDocument>().Any(document => document.OD_DocumentGroup == "SHP"));

				var matchedALLdocumnetGroupContracts = contacts.Cast<OrgContact>().Where(contact => !contact.OC_Email.IsEmpty && !contact.OC_ContactName.IsEmpty &&
					contact.Documents.Cast<OrgDocument>().Any(document => document.OD_DocumentGroup == "ALL"));

				matched = matchedVGMdocumentGroupContracts.Any() ? matchedVGMdocumentGroupContracts.First() :
							matchedSHPdocumentGroupContracts.Any() ? matchedSHPdocumentGroupContracts.First() :
							matchedALLdocumnetGroupContracts.Any() ? matchedALLdocumnetGroupContracts.First() : null;
			}

			return matched;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message, Non-Translatable validation message")]
		void AddAddressValidation(VerifiedGrossMass verifiedGrossMass)
		{
			var shipperErrors = string.Join(System.Environment.NewLine, GetErrorsForShipper(verifiedGrossMass.Shipper).Where(error => !string.IsNullOrWhiteSpace(error)));
			verifiedGrossMass.Shipper.AddressFormattedInfo.AddMessageError(() => !string.IsNullOrWhiteSpace(shipperErrors), shipperErrors);
			verifiedGrossMass.Shipper.AddressFormattedInfo.AddWarning(() => verifiedGrossMass.Shipper.CompanyName.Length > 70, (NoResString)"Party name should not exceed 70 characters. Please note that any excess characters might be truncated by the message recipient.");
			verifiedGrossMass.Shipper.AddAsciiCharactersValidation();

			var freightForwarderErrors = string.Join(System.Environment.NewLine, GetErrorsForFreightForwarder(verifiedGrossMass.FreightForwarder).Where(error => !string.IsNullOrWhiteSpace(error)));
			verifiedGrossMass.FreightForwarder.AddressFormattedInfo.AddMessageError(() => !string.IsNullOrWhiteSpace(freightForwarderErrors), freightForwarderErrors);
			verifiedGrossMass.FreightForwarder.AddressFormattedInfo.AddWarning(() => verifiedGrossMass.FreightForwarder.CompanyName.Length > 70, (NoResString)"Party name should not exceed 70 characters. Please note that any excess characters might be truncated by the message recipient.");
			verifiedGrossMass.FreightForwarder.AddAsciiCharactersValidation();

			if (consol.IsCoLoad)
			{
				var uSCCCCode = ZString.Empty;
				if (consol.CreditorAddress?.Header != null)
				{
					var customsCodes = consol.CreditorAddress?.Header.CustomsCodes;

					uSCCCCode = customsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Constants.CountryCodes.UnitedStates)?.OK_CustomsRegNo ?? ZString.Empty;

					if (uSCCCCode.IsEmpty)
					{
						uSCCCCode = customsCodes?.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, ZString.Empty)?.OK_CustomsRegNo ?? ZString.Empty;
					}
				}

				verifiedGrossMass.FreightForwarder.AddressFormattedInfo.AddMessageError(() => uSCCCCode.IsEmpty, (NoResString)"Freight Forwarder SCAC is missing from Carrier organization record under Organisation > Config > Country US, Type CCC (or Type C1C).");
				verifiedGrossMass.FreightForwarder.AddressFormattedInfo.AddMessageError(() =>
				{
					return consol.Creditor != null && consol.Creditor.ShippingLine == null;
				}, "This Co-Load With Organisation is not linked to a Shipping Line record. Please link this Co-Loader to Shipping Line record on Organization > Carrier > Shipping Line/NVOCC/Agent > Shipping Line.");
				verifiedGrossMass.FreightForwarder.AddressFormattedInfo.AddMessageError(() =>
				{
					var carrier = consol.Creditor;
					return carrier != null && carrier.ShippingLine != null
					&& (verifiedGrossMass.FreightForwarder.Email.IsEmpty || verifiedGrossMass.FreightForwarder.Contact.IsEmpty)
					&& FindMissingConfig(carrier) && !carrier.ShippingLine.RSL_VerifiedGrossContainerWeightAvailable;
				}, "This NVOCC does not support electronic Verified Gross Container Weight. Contact name and email address are required to send your Verified Gross Container Weight by email.\nPlease setup contact name and email address on Organization> Contact> Email and Receiving Documents> Group VGM");

				verifiedGrossMass.FreightForwarder.AddValidationDependencies(verifiedGrossMass.FreightForwarder.AddressFormattedInfo, verifiedGrossMass.FreightForwarder.ContactInfo, verifiedGrossMass.FreightForwarder.EmailInfo);
			}
			else
			{
				verifiedGrossMass.Carrier.AddressFormattedInfo.AddMessageError(() =>
				{
					return consol.ShippingLine != null && consol.ShippingLine.ShippingLine == null;
				}, "This Carrier Organisation is not linked to a Shipping Line record. Please link this Carrier to Shipping Line record on Organization > Carrier > Shipping Line/NVOCC/Agent > Shipping Line.");
				verifiedGrossMass.Carrier.AddressFormattedInfo.AddMessageError(() =>
				{
					var carrier = consol.ShippingLine;
					return carrier != null && carrier.ShippingLine != null
					&& (verifiedGrossMass.Carrier.Email.IsEmpty || verifiedGrossMass.Carrier.Contact.IsEmpty)
					&& FindMissingConfig(carrier) && !carrier.ShippingLine.RSL_VerifiedGrossContainerWeightAvailable;
				}, "This carrier does not support electronic Verified Gross Container Weight. Contact name and email address are required to send your Verified Gross Container Weight by email.\nPlease setup contact name and email address on Organization> Contact> Email and Receiving Documents> Group VGM");

				verifiedGrossMass.Carrier.AddValidationDependencies(verifiedGrossMass.Carrier.AddressFormattedInfo, verifiedGrossMass.Carrier.ContactInfo, verifiedGrossMass.Carrier.EmailInfo);
			}

			var carrierErrors = string.Join(System.Environment.NewLine, GetErrorsForCarrier(verifiedGrossMass.Carrier).Where(error => !string.IsNullOrWhiteSpace(error)));
			verifiedGrossMass.Carrier.AddressFormattedInfo.AddMessageError(() => !string.IsNullOrWhiteSpace(carrierErrors), carrierErrors);
			verifiedGrossMass.Carrier.AddressFormattedInfo.AddWarning(() => verifiedGrossMass.Carrier.CompanyName.Length > 70, (NoResString)"Party name should not exceed 70 characters. Please note that any excess characters might be truncated by the message recipient.");
			verifiedGrossMass.Carrier.AddAsciiCharactersValidation();

			var carrierHandlingAgentHasC1C = verifiedGrossMass.CarrierHandlingAgent.HasRegistrationNumber(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode);
			verifiedGrossMass.CarrierHandlingAgent.AddressFormattedInfo.AddMessageError(() => !verifiedGrossMass.CarrierHandlingAgent.CompanyName.IsEmpty && (verifiedGrossMass.OperationalPort?.Code.IsNingboPort() ?? false) && !carrierHandlingAgentHasC1C,
(NoResString)"C1C Code is missing from Carrier Handling Agent record under Organization > Config > Type C1C.");
			verifiedGrossMass.CarrierHandlingAgent.AddAsciiCharactersValidation();

			var carrierBookingAgentHasC1C = verifiedGrossMass.CarrierBookingAgent.HasRegistrationNumber(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode);
			verifiedGrossMass.CarrierBookingAgent.AddressFormattedInfo.AddMessageError(() => !verifiedGrossMass.CarrierBookingAgent.CompanyName.IsEmpty && (verifiedGrossMass.OperationalPort?.Code.IsNingboPort() ?? false) && !carrierBookingAgentHasC1C,
(NoResString)"C1C Code is missing from Carrier Booking Agent record under Organization > Config > Type C1C.");
			verifiedGrossMass.CarrierBookingAgent.AddAsciiCharactersValidation();

			verifiedGrossMass.Consignee.AddAsciiCharactersValidation();
			verifiedGrossMass.CurrentUser.AddAsciiCharactersValidation();

			if (!consol.IsCoLoad)
			{
				verifiedGrossMass.Carrier.AddressFormattedInfo.AddMessageError(() =>
					{
						var orgHeader = consol.IsCoLoad ? consol.Creditor : consol.ShippingLine;
						if (orgHeader != null)
						{
							return CarrierHasIntegrationViaEmailToCarrierLocalOfficeTag(orgHeader) && (verifiedGrossMass.Carrier.Contact.IsEmpty || verifiedGrossMass.Carrier.Email.IsEmpty);
						}
						return false;
					}
					, "This carrier only supports integration via email to local office. \r\nContact name and email address are required to send Verified Gross Container Weight.\r\nPlease maintain contact name and email address in carrier Organization > Contact > Email and Receiving Documents > Group VGM.");
			}
		}

		bool FindMissingConfig(OrgHeader carrier)
		{
			var hasMissingConfig = carrier.FilteredContacts.Count == 0 ||
				carrier.FilteredContacts[0].OC_ContactName.IsEmpty ||
				carrier.FilteredContacts[0].OC_Email.IsEmpty ||
				!carrier.FilteredContacts[0].Documents.Cast<OrgDocument>().Any(document => document.OD_DocumentGroup == "ALL" || document.OD_DocumentGroup == "SHP");
			return hasMissingConfig;
		}

		List<ZString> GetErrorsForShipper(Address shipper)
		{
			return GetCommonErrorsForAddress(shipper, (NoResString)"Shipper"); // non-translatable validation message
		}

		List<ZString> GetErrorsForFreightForwarder(Address freightForwarder)
		{
			return GetCommonErrorsForAddress(freightForwarder, (NoResString)"Freight Forwarder"); // non-translatable validation message
		}

		List<ZString> GetErrorsForCarrier(Address carrier)
		{
			var errors = GetCommonErrorsForAddress(carrier, (NoResString)"Carrier"); // non-translatable validation message

			var scac = ZString.Empty;
			if (consol.ShippingLineAddress?.Header != null)
			{
				scac = consol.ShippingLineAddress?.Header.CustomsCodes.OfType<OrgCusCode>()
							.FirstOrDefault(c => c.OK_RN_NKCodeCountry == Constants.CountryCodes.UnitedStates && c.OK_CodeType == OrgCusCode.CodeTypes.CarrierCode)
							?.OK_CustomsRegNo ?? ZString.Empty;

				if (scac.IsEmpty)
				{
					scac = consol.ShippingLineAddress.Header
								.CustomsCodes.OfType<OrgCusCode>()
								.FirstOrDefault(c => c.OK_CodeType == OrgCusCode.CodeTypes.CargoWiseOneCarrierCode)
								?.OK_CustomsRegNo ?? ZString.Empty;
				}
			}

			errors.Add(scac.IsEmpty ? (ZString)(NoResString)"Carrier SCAC is missing from Carrier organization record under Organisation > Config > Country US, Type CCC (or Type C1C)." : ZString.Empty); // Non-Translatable validation message
			errors.Add(scac.Length > 4 ? (ZString)(NoResString)"Carrier SCAC code must not exceed 4 characters." : ZString.Empty); // non-translatable validation message

			return errors;
		}

		#region Common Address Validation

		List<ZString> GetCommonErrorsForAddress(Address address, string partyName)
		{
			return new List<ZString>
			{
				GetPartyNameAndAddressValidationError(address, partyName),
				GetAsciiValidationMessageError(address)
			};
		}

		ZString GetPartyNameAndAddressValidationError(Address address, string partyName)
		{
			if (address.IsPartyNameAndAddressEmpty())
			{
				return string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} party name and address information is required.", partyName); // non-translatable validation message
			}

			return ZString.Empty;
		}

		ZString GetAsciiValidationMessageError(Address address)
		{
			if (ValidationExtensions.HasNonAsciiCharacters(address.AddressFormatted))
			{
				return (NoResString)"Most messaging providers do not support non ASCII characters."; // non-translatable validation message
			}

			return ZString.Empty;
		}

		#endregion

		#endregion

		#region Signature

		void PopulateSignature(VerifiedGrossMass verifiedGrossMass)
		{
			var signature = new SignatureDetails
			{
				Signature = GlbStaff.CurrentUser.SignatureImage,
				Name = GlbStaff.CurrentUser.GS_FullName,
				DateTime = ZDateTime.Now
			};

			verifiedGrossMass.SignedBy = signature.AddCurrentUserValidation(verifiedGrossMass.CurrentUser);
			AddSignatureValidation(verifiedGrossMass, signature);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		void AddSignatureValidation(VerifiedGrossMass verifiedGrossMass, SignatureDetails signature)
		{
			const string validNameErrorMessage = "Your username contains invalid characters. Please use basic ASCII characters only.";
			verifiedGrossMass.ErrorPlaceHolderInfo.AddMessageError(() => ValidationExtensions.HasUnsupportedBasicAsciiCharacter(signature.Name), validNameErrorMessage);
		}

		#endregion

		#region Containers

		void PopulateContainers(VerifiedGrossMass verifiedGrossMass, IReadOnlyCollection<ForwardingContainer> containers, bool isExportFromChina)
		{
			var containerBuilder = new ContainerBuilder();
			var containerCollection = new List<VGMMessagingContainer>();
			var packLineBuilder = new PackingLineBuilder();

			foreach (var containerBizObj in containers)
			{
				var packlines = containerBizObj.PackLines?.Cast<Freight.Business.PackLine>()?.Select(p => packLineBuilder.Build(p))?.ToArray();
				var container = containerBuilder.Build(containerBizObj, context, packlines);

				container.IsEmpty = containerBizObj.JC_IsEmptyContainer;

				var vgmContainer = new VGMMessagingContainer(container, containerBizObj)
				{
					ShiLianDan = GetShiLianDan(containerBizObj),
					Statement = GetStatement(containerBizObj)
				};

				PopulateContainerSealPartiesTypeDescription(vgmContainer);

				AddContainerValidation(vgmContainer, containerBizObj, isExportFromChina);

				containerCollection.Add(vgmContainer);
			}

			verifiedGrossMass.Containers = containerCollection
				.OrderBy(container => GetContainerOrder(container))
				.ThenBy(container => container.Number)
				.ToArray();
		}

		void PopulateContainerSealPartiesTypeDescription(VGMMessagingContainer vgmContainer)
		{
			vgmContainer.SealPartyType = new CodeDescription(ContainerSealParties)
			{
				Code = vgmContainer.SealPartyType.Code
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Non-Translatable validation message, non-translatable validation message, seal")]
		void AddContainerValidation(VGMMessagingContainer vgmContainer, ForwardingContainer container, bool isExportFromChina)
		{
			vgmContainer.NumberInfo.AddMessageErrorIfEmpty((NoResString)"Container Number is required");
			vgmContainer.NumberInfo.AddMessageError(() => !(vgmContainer.PackingLines?.Any() ?? false) || vgmContainer.IsEmpty, (NoResString)"Container must have at least one non-empty Packline.");
			vgmContainer.NumberInfo.AddMessageError(() => !ContainerNumberValidation.IsValidContainerNumber(vgmContainer.Number) && !vgmContainer.Number.IsEmpty && !vgmContainer.IsShipperOwned,
(NoResString)"Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");
			vgmContainer.NumberInfo.AddWarning(() => !ContainerNumberValidation.IsValidContainerNumber(vgmContainer.Number) && !vgmContainer.Number.IsEmpty && vgmContainer.IsShipperOwned,
(NoResString)"Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");

			var unsupportedMethods = new[]
			{
				ContainerGrossWeightVerificationTypes.Codes.RationalMethod,
				ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal,
				ContainerGrossWeightVerificationTypes.Codes.NotRequired
			};

			vgmContainer.VerifiedMethod.CodeInfo.AddMessageError(() => unsupportedMethods.Contains(vgmContainer.VerifiedMethod.Code.ToString()), (NoResString)"This Verified Method cannot be sent electronically.");
			vgmContainer.VerifiedMethod.CodeInfo.AddMessageError(() => vgmContainer.VerifiedMethod.Code.ToString() == ContainerGrossWeightVerificationTypes.Codes.NotVerified, string.Format(CultureInfo.CurrentCulture, (NoResString)"Container {0} requires a Verified Method to be entered.", vgmContainer.Number));
			vgmContainer.VerifiedByAddress.CompanyNameInfo.AddWarning(() => !isExportFromChina && vgmContainer.VerifiedByAddress.IsPartyNameAndAddressEmpty(), (NoResString)"The party designated to ascertain the weight is not mandatory but is recommended to be sent in the VGM message.");

			vgmContainer.VerifiedByAddress
				.AddPartyNameAndAddressValidation((NoResString)"Verified By", () => isExportFromChina)
				.AddAsciiCharactersValidation()
				.AddContactDetailsValidation()
				.AddVerifiedByAddressValidation(vgmContainer.VerifiedMethod.Code);

			vgmContainer.VerifiedByAddress
				.ContactInfo
				.AddMessageError(() => !vgmContainer.VerifiedByAddress.IsPartyNameAndAddressEmpty() && string.IsNullOrWhiteSpace(vgmContainer.VerifiedByAddress.Contact), (NoResString)"Contact name is mandatory");

			const string contactNameErrorMessage = "Your Contact Name contains invalid characters. Please use basic ASCII characters only.";
			vgmContainer.VerifiedByAddress.ContactInfo
				.AddMessageError(() => ValidationExtensions.HasUnsupportedBasicAsciiCharacter(vgmContainer.VerifiedByAddress.Contact), contactNameErrorMessage);

			vgmContainer.VerifiedMethod
				.AddEmptyCodeDescriptionValidation((NoResString)"Verified Method");

			vgmContainer.StatementInfo.AddWarning(() => IsLoadingInHK && GetVGMRegistrationNumber(container.GrossWeightVerifiedByAddress).IsEmpty && IsMethod2(container),
(NoResString)"For exports from Hong Kong using Method 2 weight verification, the weighing party's approval number must be declared in this statement.  The VGM Verified By organization does not have a VGM code recorded for HK. Either enter it manually here or update the Organization with the VGM.");
			vgmContainer.StatementInfo.AddWarning(() => !vgmContainer.Statement.IsEmpty, (NoResString)"This statement is for documentary purposes only, it is not supported and will not be included in the message sent by EDI.");

			if (vgmContainer.Type is ContainerType containerType)
			{
				containerType.ISOCodeInfo.AddMessageErrorIfEmpty((NoResString)"The container ISO code is required.");

				var refContainer = container.RefContainer;
				if (consol.JK_ConsolMode == ContainerModes.FCL && !containerType.ISOCode.IsEmpty && refContainer != null
					&& refContainer.Lookups.ISOTypes.Cast<ContainerISOType>().Any(x => x.ISOCode == containerType.ISOCode))
				{
					((Measurement)vgmContainer.GrossWeight).ValueInfo.AddWarning(
						() => vgmContainer.GrossWeight.Value < container.RefContainer.RC_TareWeight,
						(NoResString)"Verified Gross Weight should not be less than Container tare weight.");
					((Measurement)vgmContainer.GrossWeight).ValueInfo.AddWarning(
						() => vgmContainer.GrossWeight.Value > container.RefContainer.RC_GrossWeight,
						(NoResString)"Verified Gross Weight exceeds the Max Gross Wt. for this container type.");
				}
			}

			((CodeDescription)vgmContainer.SealPartyType).CodeInfo.AddMessageError(() => !vgmContainer.Seal.IsEmpty && vgmContainer.SealPartyType.Code.IsEmpty, (NoResString)"Sealed By is mandatory when Seal Number exists.");
		}

		int GetContainerOrder(VGMMessagingContainer container)
		{
			if (containerStatusOrder.TryGetValue(container.VerifiedStatus.Code, out int order))
			{
				return order;
			}

			return -1;
		}

		string GetShiLianDan(ForwardingContainer container)
		{
			return container
				.AdditionalReferenceNumbers
				.OfType<CusEntryNumber>()
				.FirstOrDefault(n => n.CE_EntryType == ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber && n.CE_RN_NKCountryCode == Core.Constants.CountryCodes.China)
				?.CE_EntryNum;
		}

		string GetStatement(ForwardingContainer container)
		{
			if (IsLoadingInHK || IsPackedInChinaForHongKong)
			{
				return GetStatementForHK(container);
			}
			else if (IsLoadingInCN)
			{
				return GetStatementForCN(container);
			}

			return string.Empty;
		}

		string GetStatementForHK(ForwardingContainer container)
		{
			var vgmRegistrationNumber = GetVGMRegistrationNumber(container.GrossWeightVerifiedByAddress);

			if (IsPackedInChinaForHongKong
				&& vgmRegistrationNumber.IsEmpty)
			{
				if (IsTransportedFromCNToHKByInlandWaterway)
				{
					return PackedInCNForHKByInlandWaterwayStatement;
				}
				return PackedInCNForHKByTruckStatement;
			}

			if (IsMethod1(container))
			{
				return Method1StatementForHK;
			}

			if (IsMethod2(container))
			{
				var signedBy = !vgmRegistrationNumber.IsEmpty
					? vgmRegistrationNumber.ToString()
					: "____________";

				return string.Format(CultureInfo.InvariantCulture,
					Method2StatementForHK, signedBy);
			}

			return string.Empty;
		}

		string GetStatementForCN(ForwardingContainer container)
		{
			if (IsMethod1(container))
			{
				return Method1StatementForCN;
			}

			if (IsMethod2(container))
			{
				return Method2StatementForCN;
			}

			return string.Empty;
		}

		bool IsTransportedFromCNToHKByInlandWaterway => isTransportedFromCNToHKByInlandWaterway ??
			(isTransportedFromCNToHKByInlandWaterway = FirstSeaTransport != null
				&& FirstSeaTransport.JW_TransportMode == TransportModes.InlandWaterwayTransport
				&& FirstSeaTransport.JW_TransportType == TransportPlanningType.PreCarriage
				&& FirstSeaTransport.LoadPort?.RL_RN_NKCountryCode.ToString() == CountryCodes.China
				&& FirstSeaTransport.DiscPort?.RL_RN_NKCountryCode.ToString() == CountryCodes.HongKong).Value;
		bool? isTransportedFromCNToHKByInlandWaterway;

		bool IsMethod1(ForwardingContainer container) => container.JC_GrossWeightVerificationType == Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;

		bool IsMethod2(ForwardingContainer container) => container.JC_GrossWeightVerificationType == Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;

		ZString GetVGMRegistrationNumber(JobDocAddress address)
		{
			if (address == null
				|| !address.HasRealAddress)
			{
				return ZString.Empty;
			}

			return address
				.Organisation
				.CustomsCodes
				.OfType<OrgCusCode>()
				.FirstOrDefault(number => number.OK_RN_NKCodeCountry == Constants.CountryCodes.HongKong
					&& number.OK_CodeType == OrgCusCode.CodeTypes.VGMRegistrationNumber
					&& !number.OK_CustomsRegNo.IsEmpty)
				?.OK_CustomsRegNo
				?? ZString.Empty;
		}

		readonly IReadOnlyDictionary<string, int> containerStatusOrder = new Dictionary<string, int>
		{
			[Constants.ContainerGrossWeightVerificationStatuses.Codes.NotVerified] = 0,
			[Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent] = 1,
			[Constants.ContainerGrossWeightVerificationStatuses.Codes.AmendedNotSent] = 2,
			[Constants.ContainerGrossWeightVerificationStatuses.Codes.Rejected] = 3,
			[Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawRejected] = 4,
			[Constants.ContainerGrossWeightVerificationStatuses.Codes.Sent] = 5,
			[Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawSent] = 6,
			[Constants.ContainerGrossWeightVerificationStatuses.Codes.Acknowledged] = 7,
			[Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawAcknowledged] = 8,
			[Constants.ContainerGrossWeightVerificationStatuses.Codes.Accepted] = 9,
			[Constants.ContainerGrossWeightVerificationStatuses.Codes.NotRequired] = 10
		};

		IReadOnlyCollection<Freight.Business.Transport> SeaTransportsInLegOrder
		{
			get
			{
				if (seaTransportsInLegOrder == null)
				{
					seaTransportsInLegOrder = consol.Transports
						.OfType<Freight.Business.Transport>()
						.Where(t => t.JW_TransportMode == TransportModes.Sea || t.JW_TransportMode == TransportModes.InlandWaterwayTransport)
						.OrderBy(t => t.JW_LegOrder)
						.ToArray();
				}

				return seaTransportsInLegOrder;
			}
		}
		IReadOnlyCollection<Freight.Business.Transport> seaTransportsInLegOrder;

		Freight.Business.Transport FirstSeaTransport => firstSeaTransport ?? (firstSeaTransport = SeaTransportsInLegOrder.FirstOrDefault(leg => leg.TransportMode == TransportModes.Sea || leg.TransportMode == TransportModes.InlandWaterwayTransport));
		Freight.Business.Transport firstSeaTransport;

		Freight.Business.Transport FirstSeaTransportExcludingIWT => firstSeaTransportExcludingIWT ?? (firstSeaTransportExcludingIWT = SeaTransportsInLegOrder.FirstOrDefault(leg => leg.TransportMode == TransportModes.Sea));
		Freight.Business.Transport firstSeaTransportExcludingIWT;

		bool IsLoadingInHK => isLoadingInHK ?? (isLoadingInHK = consol?.LoadPort?.RL_RN_NKCountryCode.ToString() == CountryCodes.HongKong).Value;
		bool? isLoadingInHK;

		bool IsLoadingInCN => isLoadingInCN ?? (isLoadingInCN = consol?.LoadPort?.RL_RN_NKCountryCode.ToString() == CountryCodes.China).Value;
		bool? isLoadingInCN;

		bool IsPackedInChinaForHongKong => isPackedInChinaForHongKong ?? (isPackedInChinaForHongKong = IsLoadingInCN && FirstSeaTransportExcludingIWT?.LoadPort?.RL_RN_NKCountryCode.ToString() == CountryCodes.HongKong).Value;
		bool? isPackedInChinaForHongKong;

		#region SuppressResourceStringsCheckRegion

		const string Method1StatementForHK = "The verified gross mass of the packed container(s) declared in this shipping document was obtained in accordance with Method 1 stipulated in SOLAS Chapter VI Regulation 2.";
		const string Method2StatementForHK = "The verified gross mass of the packed container(s) declared in this shipping document was obtained in accordance with Method 2 stipulated in SOLAS Chapter VI Regulation 2. The procedure of this method has been approved or recognised by Hong Kong Marine Department with registration number {0}.";
		const string Method1StatementForCN = "I, the shipper, declare that the verified gross mass information of the packed container in this document is obtained based on the stated method in the International Convention for the Safety of Life at Sea (SOLAS) 1974 Chapter VI Regulation 2.4.2. The weighing instrument in the weighing station has received the certificate from a metrological supervision organization and the date receiving the verified weight is within the validity period of the certificate.";
		const string Method2StatementForCN = "I, the shipper, declare that the verified gross mass information of the packed container in this document is obtained based on the stated method in the International Convention for the Safety of Life at Sea (SOLAS) 1974 Chapter VI Regulation 2.4.2. This method follows the requirements in the 'Guidelines of Method 2 for Obtaining the Verified Gross Mass of a Packed Container' published by the enforcement agency.";
		const string PackedInCNForHKByTruckStatement = "The container stated in this document was packed and sealed in Mainland China then trucked to Hong Kong for shipping overseas under our name as shipper, which was not a business entity registered in Hong Kong. The verified gross mass of the packed container(s) herein declared was obtained in accordance with the requirement, which complied with Method 1/Method 2 stipulated in SOLAS Chapter VI Regulation 2, laid down by China MSA.";
		const string PackedInCNForHKByInlandWaterwayStatement = "The container stated in this document was packed and sealed in Mainland China then transported to Hong Kong by Inland Waterway Transport for shipping overseas under our name as shipper, which was not a business entity registered in Hong Kong. The verified gross mass of the packed container(s) herein declared was obtained in accordance with the requirement, which complied with Method 1/Method 2 stipulated in SOLAS Chapter VI Regulation 2, laid down by China MSA.";

		#endregion

		#endregion

		#region IsRequiredSendAttachment

		void PopulateCarrierMessagingRequirements(VerifiedGrossMass verifiedGrossMass)
		{
			var orgHeader = consol.IsCoLoad ? consol.Creditor : consol.ShippingLine;
			verifiedGrossMass.IsRequiredSendAttachment = OrgHeaderExtensions.GetShippingLineMessagingRequirement(orgHeader, ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage)?.RSR_IsVerifiedGrossContainerWeight ?? false;
		}

		#endregion

		#region Implementation

		CodeDescriptionPairList ContainerSealParties
		{
			get
			{
				if (containerSealPartiesList == null)
				{
					containerSealPartiesList = new CodeDescriptionPairList();
					containerSealPartiesList.AddPair(Constants.ContainerSealParties.Codes.CarrierShippingLine, ResString.GetMultilingualString("5FC6244F-960C-4E4C-A535-FA81114760A2", "Carrier"));
					containerSealPartiesList.AddPair(Constants.ContainerSealParties.Codes.ConsignorShipper, ResString.GetMultilingualString("8E468B35-7349-4D28-8DB0-18DE07F34AB5", "Shipper"));
					containerSealPartiesList.AddPair(Constants.ContainerSealParties.Codes.Customs, ResString.GetMultilingualString("429D1080-EC8F-4942-AE98-FEE2EC0C3180", "Customs"));
					containerSealPartiesList.AddPair(Constants.ContainerSealParties.Codes.Terminal, ResString.GetMultilingualString("91EB4811-7075-49BD-A065-4AADF1B0F124", "Terminal"));
					containerSealPartiesList.AddPair(Constants.ContainerSealParties.Codes.Quarantine, ResString.GetMultilingualString("505972CC-E529-4B0D-AB63-6BA255202B18", "Quarantine"));
				}

				return containerSealPartiesList;
			}
		}

		CodeDescriptionPairList containerSealPartiesList;

		#endregion

		#region Shipping Line Validation

		void AddShippingLineMessagingRequirementsValidation(VerifiedGrossMass verifiedGrossMass)
		{
			AddContainersSealNumberValidation(verifiedGrossMass);
		}

		void AddContainersSealNumberValidation(VerifiedGrossMass verifiedGrossMass)
		{
			var shippingLineOrNvocc = consol.ShippingLine?.ShippingLine;
			if (consol.IsCoLoad)
			{
				var nvocc = consol.Creditor?.ShippingLine;
				shippingLineOrNvocc = (nvocc != null && nvocc.RSL_IsNVO) ? nvocc : null;
			}

			if (shippingLineOrNvocc != null && shippingLineOrNvocc.RSL_OceanCarrierMessagingAvailable)
			{
				var requirements = shippingLineOrNvocc.ShippingLineMessagingRequirements;
				if (requirements.Any(x => x.RSR_IsVerifiedGrossContainerWeight && x.RSR_RST_NKType.Equals(ShippingLineMessagingRequirement.Types.SealNumberMandatory)))
				{
					foreach (var container in verifiedGrossMass.Containers)
					{
						((CodeDescription)container.SealPartyType).CodeInfo.AddMessageError(() => container.Seal.IsEmpty, ShippingLineMessagingRequirement.ValidationMessages.SealNumberMandatoryForExists);
						((CodeDescription)container.SealPartyType).CodeInfo.AddMessageError(() => !container.Seal.IsEmpty && container.Seal.Length > 15, ShippingLineMessagingRequirement.ValidationMessages.SealNumberMandatoryForLength);
					}
				}
			}
		}

		#endregion

		ZBool CarrierHasIntegrationViaEmailToCarrierLocalOfficeTag(OrgHeader org)
		{
			return org.GetShippingLineMessagingRequirement(ShippingLineMessagingRequirement.Types.IntegrationViaEmailToCarrierLocalOffice)?.RSR_IsVerifiedGrossContainerWeight ?? ZBool.False;
		}
	}
}
