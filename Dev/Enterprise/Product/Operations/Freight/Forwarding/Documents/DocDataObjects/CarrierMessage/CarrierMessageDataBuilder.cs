using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CarrierMessageValidation;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Business.RequiredTaxNumbers;
using static Enterprise.Integration.Customs.Shared;
using Constants = Enterprise.Core.Constants;
using CountryCodes = Enterprise.Core.Constants.CountryCodes;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;
using ResString = Enterprise.Freight.Forwarding.Documents.DataObjects.ResString;
using ShippingLineMessagingRequirement = Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants.ShippingLineMessagingRequirement;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	abstract class CarrierMessageDataBuilder
	{
		public CarrierMessageDataBuilder(ForwardingConsol consol)
		{
			this.consol = consol;
			context = new ContextWithCarrierUnlocoMapping(consol.Factory.GetCachedReadOnlyFactory(), consol.IsCoLoad ? consol.CreditorPK : consol.ShippingLinePK);
			packageGroupingHelper = new PackageGroupingHelper(consol, context);
		}

		protected readonly ForwardingConsol consol;
		protected readonly IContext context;
		protected readonly PackageGroupingHelper packageGroupingHelper;

		protected abstract string IelMessageRequirementValidationContactErrorMessage { get; }

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public CarrierMessageData Build()
		{
			var wrapper = GetNewCarrierMessageData();

			wrapper.FreightForwarderReference = wrapper.SourceID;

			wrapper.TransportMode = new CodeDescription(context.TransportModes) { Code = consol.JK_TransportMode };
			wrapper.IsCoload = consol.IsCoLoad;
			wrapper.IsGatewayCoload = consol.IsCoLoad && consol.IsSendingOrReceivingForwarderGateway;
			wrapper.IsNVO = consol.ShippingLineIsNVOCC;
			wrapper.IsDirect = consol.IsDirect;
			wrapper.ShipperReference = GetShipperReference(wrapper);
			wrapper.Numbers = ReferenceNumber.Create(context, consol.Numbers);
			wrapper.AcidNumber = GetAcidNumber(wrapper);

			PopulateCarrierContractNumbersFormattedAndContractNamedAccount(wrapper);

			wrapper.ForwardingInstructions = consol.Notes.FindByDescription(PredefinedNoteTypes.Instance.ForwardingInstructionNotes.Description)?.FirstOrDefault()?.ST_NoteDataAsText ?? ZString.Empty;
			wrapper.ForwardingInstructionsInfo.AddAsciiCharactersValidation();

			PopulateSpecialInstructions(wrapper);
			wrapper.SpecialInstructionsInfo.AddAsciiCharactersValidation();

			PopulateGoodsHandlingInstructions(wrapper);
			PopulateBookingReferenceAndMasterBillNumber(wrapper);

			wrapper.ContainerMode = new CodeDescription(ContainerModesList)
			{
				Code = consol.JK_ConsolMode == Constants.ContainerModes.Groupage || consol.JK_ConsolMode == Constants.ContainerModes.BuyersConsol || consol.JK_ConsolMode == Constants.ContainerModes.ShippersConsol || (consol.JK_ConsolMode == Constants.ContainerModes.Other && consol.Containers.Any())
					? (ZString)(consol.IsCoLoad ? Constants.ContainerModes.LCL : Constants.ContainerModes.FCL)
					: consol.JK_ConsolMode
			};
			wrapper.IsNonContainerized = consol.JK_ConsolMode == Constants.ContainerModes.BreakBulk || consol.JK_ConsolMode == Constants.ContainerModes.Bulk || consol.JK_ConsolMode == Constants.ContainerModes.RollOnRollOff;
			wrapper.MustHaveContainerPackline = !wrapper.IsNonContainerized && !wrapper.IsColoadLCL && (consol.Containers.Count <= 0 ? wrapper.ContainerMode.Code != Constants.ContainerModes.LCL : consol.Containers.Any());
			wrapper.IsRORO = consol.JK_ConsolMode == Constants.ContainerModes.RollOnRollOff;
			wrapper.HasShipments = consol.Shipments.Any();

			wrapper.DateOfIssue = consol.JK_MasterBillIssueDate;
			wrapper.PortOfFirstArrivalDate = consol.JK_DatePortOfFirstArrival;
			wrapper.FirstForeignArrivalDate = consol.JK_DateFirstForeignPort;
			wrapper.LastForeignDepartureDate = consol.JK_DateLastForeignPort;
			wrapper.EarliestDepartureDate = Transports.FirstOrDefault()?.JW_ETD ?? ZDateTime.Empty;
			wrapper.LatestDeliveryDate = Transports.LastOrDefault()?.JW_ETA ?? ZDateTime.Empty;
			wrapper.IsDoorPickup = consol.IsDoorPickup();
			wrapper.IsDoorDelivery = consol.IsDoorDelivery();

			wrapper.AgentType = new CodeDescription(consol.JK_AgentType_List)
			{
				Code = consol.JK_AgentType
			};

			wrapper.PaymentMethod = new CodeDescription(consol.JK_PrepaidCollect_List)
			{
				Code = consol.JK_PrepaidCollect
			};

			wrapper.Transports = DocDataObjects.Transports.Create(context, Transports);
			wrapper.IsColoadLCL = consol.JK_ConsolMode == Constants.ContainerModes.LCL && (consol.IsCoLoad || wrapper.IsNVO);

			wrapper.PackageGrouping = new CodeDescription(consol.JK_PackageGrouping_List)
			{
				Code = !wrapper.IsRORO && FreightDataRegistry.Instance.EnablePackageGrouping.Value ? consol.JK_PackageGrouping : new ZString(Constants.PackageGrouping.Codes.DoNotGroup)
			};
			IsGroupAndConsolidatePackingLines = wrapper.PackageGrouping.Code == Constants.PackageGrouping.Codes.GroupByShipment || wrapper.PackageGrouping.Code == Constants.PackageGrouping.Codes.GroupByPackLine;

			wrapper.IsTaxIdModifiable = FreightDataRegistry.Instance.AllowCompanyTaxIDOverride.Value;

			PopulatePorts(wrapper);
			PopulateUnitsOfWeightVolumeAndDimension(wrapper);
			PopulateShipments(wrapper);
			PopulateContainers(wrapper);
			PopulateAddresses(wrapper);
			PopulateTaxInfo(wrapper);
			PopulateCharges(wrapper);
			PopulateAdditionalData(wrapper);
			PopulateContactNameAndEmail(wrapper);
			PupulateDetailedPortNameToUnlocos(wrapper);
			PopulatePreAllocatedUNDGCollection(wrapper);

			wrapper.ReleaseType = new CodeDescription(ReleaseTypesList)
			{
				Code = GetReleaseType(wrapper)
			};

			PopulateCarrierMessagingRequirements(wrapper);
			PopulateNumberOfOriginalsAndCopies(wrapper);
			PopulateBOLDocumentationProvider(wrapper);
			AddValueChangeHandlers(wrapper);
			AddValidation(wrapper);
			wrapper.ValidateAllIncludingChildren();

			return wrapper;
		}

		void PopulatePreAllocatedUNDGCollection(CarrierMessageData wrapper)
		{
			wrapper.IsHazardous = DoesLinkedShipmentsHaveDangerousGoods(wrapper) ? ZBool.True : consol.JK_IsHazardous;

			var preAllocatedUNDGs = new List<DGRestriction>();
			var dgRestrictionBuilder = new DGRestrictionBuilder();

			foreach (var consolDGRestriction in consol.ConsolDGRestrictionCollection)
			{
				var dgRestriction = dgRestrictionBuilder.Build(consolDGRestriction.JKD_UNNO, consolDGRestriction.JKD_Variant, context);

				if (dgRestriction != null)
				{
					preAllocatedUNDGs.Add(dgRestriction);
				}
			}

			wrapper.PreallocatedUNDGCollection = preAllocatedUNDGs;
		}

		bool DoesLinkedShipmentsHaveDangerousGoods(CarrierMessageData wrapper)
		{
			return wrapper.Shipments.Cast<Shipment>().Any(s => s.AllPackingLinesIncludeCoLoad.Any(p => p.DangerousGoods.Any()));
		}

		protected abstract CarrierMessageData GetNewCarrierMessageData();

		protected virtual void AddValidation(CarrierMessageData wrapper)
		{
			AddAddressValidation(wrapper);
			AddPortsValidation(wrapper);
			AddTransportsValidation(wrapper);
			AddContainerValidations(wrapper);
			AddPackingLinesValidation(wrapper);
			AddChargesValidation(wrapper);
			AddCarrierContractNumberValidation(wrapper);
			AddAcidNumberValidation(wrapper);
			AddCarrierMessagingRequirementsValidation(wrapper);
		}

		#region Population

		protected virtual void PopulateAdditionalData(CarrierMessageData wrapper)
		{
		}

		void PopulateCarrierMessagingRequirements(CarrierMessageData wrapper)
		{
			wrapper.ElectronicBillOfLadingProviderMandatory = IsElectronicBillOfLadingProviderMandatory(wrapper);
			wrapper.IsRequiredSendAttachment = GetIsRequiredSendAttachment();
		}

		void PopulateCarrierContractNumbersFormattedAndContractNamedAccount(CarrierMessageData wrapper)
		{
			var carrierContractNumbers = wrapper.Numbers.Where(num => num.Type != null && num.Type.Code == DocDataConstants.AdditionalReferences.Codes.CarrierQuoteNumber);
			if (carrierContractNumbers.Any())
			{
				wrapper.CarrierContractNumbersFormatted = ZString.Join(", ", carrierContractNumbers.Select(num => num.Value).ToArray());
			}
			else
			{
				wrapper.CarrierContractNumbersFormatted = consol?.JK_CarrierContractNumber ?? ZString.Empty;
			}

			var contractNamedAccount = wrapper.Numbers.FirstOrDefault(num => num.Type != null && num.Type.Code == DocDataConstants.AdditionalReferences.Codes.ContractNamedAccount);
			if (contractNamedAccount != null)
			{
				wrapper.ContractNamedAccount = contractNamedAccount.Value;
			}
		}

		void PopulateContactNameAndEmail(CarrierMessageData wrapper)
		{
			var carrier = consol.IsCoLoad ? consol.Creditor : consol.ShippingLine;
			var contact = GetDefaultContact(carrier?.FilteredContacts);
			if (carrier != null && carrier.ShippingLine != null &&
				wrapper.Recipient != null &&
				contact != null &&
				(!IsShippingLineRelevantMessageOptionEnabled(GetNewCarrierMessageData().DocumentName, carrier.ShippingLine) || CarrierHasIntegrationViaEmailToCarrierLocalOfficeTag()))
			{
				wrapper.Recipient.Email = contact.OC_Email;
				wrapper.Recipient.Contact = contact.OC_ContactName;
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

		void PopulateBookingReferenceAndMasterBillNumber(CarrierMessageData wrapper)
		{
			wrapper.BookingReference = consol.JK_BookingReference;
			wrapper.MasterBillNumber = consol.JK_MasterBillNum;

			if (consol.IsCoLoad)
			{
				wrapper.CoLoadBookingReference = consol.JK_CoLoadBookingReference;
				wrapper.CoLoadMasterBillNumber = consol.JK_CoLoadMasterBill;
			}
		}

		void PopulateGoodsHandlingInstructions(CarrierMessageData wrapper)
		{
			GetGoodsHandlingInstructionsFromNotesWithFallingBack(wrapper);

			var exclusiveUseText = GetExclusiveUseHandlingInformation(consol);
			if (!exclusiveUseText.IsNullOrEmpty())
			{
				if (!wrapper.GoodsHandlingInstructions.IsEmpty)
				{
					wrapper.GoodsHandlingInstructions += System.Environment.NewLine;
				}
				wrapper.GoodsHandlingInstructions += exclusiveUseText;
			}
			wrapper.GoodsHandlingInstructionsInfo.AddAsciiCharactersValidation();
		}

		protected virtual void GetGoodsHandlingInstructionsFromNotesWithFallingBack(CarrierMessageData wrapper)
		{
			wrapper.GoodsHandlingInstructions = GetNoteTextByDescription(consol.Notes, PredefinedNoteTypes.Instance.HandlingInstructions.Description);

			if (wrapper.GoodsHandlingInstructions.IsEmpty && wrapper.IsDirect)
			{
				wrapper.GoodsHandlingInstructions = GetNoteTextByDescription(DirectShipment?.Notes, PredefinedNoteTypes.Instance.HandlingInstructions.Description);
			}
		}

		protected virtual void PopulateSpecialInstructions(CarrierMessageData wrapper)
		{
			wrapper.SpecialInstructions = consol.IsDirect
				? GetNoteTextByDescription(DirectShipment?.Notes, PredefinedNoteTypes.Instance.SpecialInstructions.Description)
				: GetNoteTextByDescription(consol.Notes, PredefinedNoteTypes.Instance.SpecialInstructions.Description);
		}

		protected ZString GetNoteTextByDescription(Notes notes, string description)
		{
			return notes?.FindByDescription(description)?.FirstOrDefault()?.ST_NoteDataAsText ?? ZString.Empty;
		}

		string GetExclusiveUseHandlingInformation(ForwardingConsol consol)
		{
			var containsExclusiveUseUNDGDataItems = consol
				.Shipments
				.Cast<ForwardingShipment>()
				.SelectMany(shipment => shipment.OuterPackLines)
				.Cast<ForwardingPackLine>()
				.SelectMany(packline => packline.UNDGs)
				.Cast<UNDGDataItem>()
				.Any(undgDataItem => undgDataItem.DI_IsExclusiveUse);

			if (containsExclusiveUseUNDGDataItems)
			{
				return Res.GetString("C21EFA3A-C43F-43C5-8B3A-F1DE377EF690", "Exclusive Use");
			}

			return string.Empty;
		}

		void PopulatePorts(CarrierMessageData wrapper)
		{
			wrapper.PortOfLoading = Unloco.Create(context,
				Transports.FirstOrDefault(t => t.JW_TransportMode == Core.Constants.TransportModes.Sea)?.LoadPort, true)
				.WithCustomNameProvider(GetDetailedPortName);

			wrapper.PortOfDischarge = Unloco.Create(context,
				Transports.LastOrDefault(t => t.JW_TransportMode == Core.Constants.TransportModes.Sea)?.DiscPort, true)
				.WithCustomNameProvider(GetDetailedPortName);

			wrapper.Origin = Unloco.Create(context, Origin,true).WithCustomNameProvider(GetDetailedPortName);
			wrapper.Destination = Unloco.Create(context, Destination, true).WithCustomNameProvider(GetDetailedPortName);
			wrapper.PlaceOfReceipt = Unloco.Create(context, consol.LoadPort, true).WithCustomNameProvider(GetDetailedPortName);
			wrapper.PlaceOfDelivery = Unloco.Create(context, consol.DischargePort, true).WithCustomNameProvider(GetDetailedPortName);
			wrapper.PlaceOfIssue = Unloco.Create(context, consol.MasterBillIssuePlace, true).WithCustomNameProvider(GetDetailedPortName);
			wrapper.IsToEgypt = wrapper.PlaceOfDelivery.Country.Code == CountryCodes.Egypt;
			wrapper.IsToKenya = wrapper.PlaceOfDelivery.Country.Code == CountryCodes.Kenya;
			wrapper.IsToSpecificAfricanCountry = CountryCodes.IsSpecificAfricanCountryCode(wrapper.PlaceOfDelivery.Country.Code);
			wrapper.CarrierBookingOffice = Unloco.Create(context, consol.CarrierBookingOffice);
		}

		#region Shipments

		void PopulateShipments(CarrierMessageData wrapper)
		{
			var shipmentDOs = new List<Shipment>();
			var shipmentBuilder = new ShipmentBuilder(context);

			foreach (ForwardingShipment shipment in consol.TopLevelShipments.OrderBy(s => s.JS_UniqueConsignRef))
			{
				var topLevelShipmentPackType = packageGroupingHelper.GetTopLevelShipmentPackType(shipment);
				var shipmentDO = shipmentBuilder.Build(shipment, s => GetProcessedPackingLines(s, wrapper, topLevelShipmentPackType), shipmentBO =>
				{
					var ctkNumber = string.Join(", ", consol.Numbers.Cast<CusEntryNumber>().Where(x => x.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CargoTrackingNote && wrapper.PortOfDischarge.IsInCountry(x.CE_RN_NKCountryCode)).Select(x => x.CE_EntryNum));
					return !ctkNumber.IsNullOrEmpty() ? ctkNumber : null;
				});

				PopulateShipment(shipment, shipmentDO);

				shipmentDOs.Add(shipmentDO);
			}

			wrapper.Shipments = shipmentDOs;

			PopulateExportStatementFields(wrapper);

			packageGroupingHelper.PopulateGroupedAndConsolidatedPackingLines(wrapper.Shipments, wrapper.PackageGrouping.Code, wrapper.MustHaveContainerPackline, wrapper.DocumentName, wrapper.UnitOfWeight, wrapper.UnitOfVolume);
			packageGroupingHelper.PopulateConsolidatedPackingLineDangerousGoodsForDoNotGroup(wrapper.Shipments, wrapper.PackageGrouping.Code, wrapper.DocumentName, wrapper.UnitOfWeight, wrapper.UnitOfVolume);
		}

		protected virtual void PopulateShipment(ForwardingShipment shipment, Shipment shipmentDO)
		{
		}

		protected virtual void PopulateExportStatementFields(CarrierMessageData wrapper)
		{
		}

		#region GetProcessedPackingLines

		IEnumerable<PackingLine> GetProcessedPackingLines(ForwardingShipment shipment, CarrierMessageData wrapper, ZString topLevelShipmentPackType)
		{
			var packLineBuilder = new PackingLineBuilder();
			var packLineDOs = new List<PackingLine>();

			foreach (PackLine packLineBO in shipment.OuterPackLines)
			{
				var packingLineDO = packLineBuilder.Build(packLineBO, wrapper.UnitOfWeight, wrapper.UnitOfVolume, wrapper.UnitOfDimensions, consol);

				PopulatePackingShipmentEntryNumbers(wrapper, packLineBO, packingLineDO);
				packageGroupingHelper.PopulatePackingQuantityAndPackageType(wrapper.PackageGrouping.Code, packLineBO, packingLineDO, topLevelShipmentPackType);

				packLineDOs.Add(packingLineDO);
			}

			return packLineDOs;
		}

		void PopulatePackingShipmentEntryNumbers(CarrierMessageData wrapper, PackLine packLineBO, PackingLine packingLineDO)
		{
			if ((wrapper.PortOfLoading.IsInCountry(CountryCodes.UnitedStates) && !wrapper.PortOfDischarge.IsInCountry(CountryCodes.UnitedStates)) ||
					(wrapper.PlaceOfReceipt.IsInCountry(CountryCodes.UnitedKingdom)))
			{
				packingLineDO.ShipmentEntryNumbers = ZString.Join(", ", packLineBO.Shipment.CusEntryNumbers.Cast<CusEntryNumber>().Select(c => ZString.Format("{0}:{1}", c.CE_EntryType, c.CE_EntryNum)).ToArray());
			}
		}

		#endregion

		#endregion

		#region Containers

		protected virtual void PopulateContainers(CarrierMessageData wrapper)
		{
			var containerBuilder = new ContainerBuilder();
			var containerDOs = new List<Container>();
			var packLineDOs = wrapper.Shipments.OrderBy(s => s.ShipmentID)
				.SelectMany(x => x.AllPackingLinesIncludeCoLoad.OrderBy(p => p.PackingLineID));

			foreach (ForwardingContainer containerBO in consol.Containers)
			{
				var containerPackLinePKs = containerBO.PackLines.Cast<PackLine>().Select(p => p.PK).ToArray();
				var containerPackingLineDOs = IsGroupAndConsolidatePackingLines
					? packLineDOs.Where(x => containerBO.PK.ToString() == new ZString(x.Identifier).SubstringSafe(0, 36)).ToArray()
					: packLineDOs.Where(x => containerPackLinePKs.Contains((ZGuid)x.Identifier)).ToArray();
				var containerDO = containerBuilder.Build(containerBO, context, containerPackingLineDOs, wrapper.UnitOfWeight, wrapper.UnitOfVolume);
				containerDO.IsEmpty = containerBO.JC_IsEmptyContainer;

				PopulateContainerSealPartiesTypeDescription(containerDO);
				containerDOs.Add(containerDO);
			}

			wrapper.Containers = containerDOs
			.OrderBy(container => container.Number)
			.ToArray();

			wrapper.HasOverhangDimensionContainers = consol.Containers.OfType<ForwardingContainer>()
				.Any(x => !x.JC_Calc_OverhangHeight.IsEmpty || !x.JC_Calc_OverhangLength.IsEmpty
					|| !x.JC_OverhangBack.IsEmpty || !x.JC_Calc_OverhangWidth.IsEmpty || !x.JC_OverhangRight.IsEmpty);
			wrapper.IsOutOfGauge = wrapper.HasOverhangDimensionContainers;
		}

		void PopulateContainerSealPartiesTypeDescription(Container container)
		{
			container.SealPartyType = new CodeDescription(ContainerSealParties)
			{
				Code = container.SealPartyType.Code
			};

			container.SecondSealPartyType = new CodeDescription(ContainerSealParties)
			{
				Code = container.SecondSealPartyType.Code
			};

			container.ThirdSealPartyType = new CodeDescription(ContainerSealParties)
			{
				Code = container.ThirdSealPartyType.Code
			};
		}

		#endregion

		void PopulateUnitsOfWeightVolumeAndDimension(CarrierMessageData wrapper)
		{
			var allPackLines = consol.TopLevelShipments.Cast<ForwardingShipment>().SelectMany(x => x.OuterPackLines.Cast<PackLine>());
			var isLoadingOrDischargingInBrazil = wrapper.PortOfLoading.IsInCountry(CountryCodes.Brazil) || wrapper.PortOfDischarge.IsInCountry(CountryCodes.Brazil);

			wrapper.UnitOfWeight = !isLoadingOrDischargingInBrazil && allPackLines.Any() && allPackLines.All(p => Constants.Weight.IsImperial(p.JL_ActualWeightUQ)) ? Constants.Weight.Pounds : Constants.Weight.Kilograms;
			wrapper.UnitOfVolume = !isLoadingOrDischargingInBrazil && allPackLines.Any() && allPackLines.All(p => Constants.Volume.IsImperial(p.JL_ActualVolumeUQ)) ? Constants.Volume.CubicFeet : Constants.Volume.CubicMetres;
			wrapper.UnitOfDimensions = !isLoadingOrDischargingInBrazil && allPackLines.Any() && allPackLines.All(p => Constants.Length.IsImperial(p.JL_UnitOfDimension)) ? Constants.Dimension.Feet : Constants.Dimension.Metres;
		}

		protected virtual ZAddressWithContact ForwarderAddressWithContact => consol.SendingForwarderWithContact;

		void PopulateAddresses(CarrierMessageData wrapper)
		{
			wrapper.Shipper = AddressBuilder.Create(context, Shipper).AddAsAgentInfoToCompanyName(Shipper);
			wrapper.Carrier = AddressBuilder.Create(context, consol.ShippingLineAddress, false);
			wrapper.Creditor = AddressBuilder.Create(context, consol.CreditorAddress, false);
			wrapper.Forwarder = AddressBuilder.Create(context, ForwarderAddressWithContact);
			wrapper.SendingForwarder = AddressBuilder.Create(context, consol.SendingForwarderWithContact);
			wrapper.Buyer = AddressBuilder.Create(context, Buyer);
			wrapper.Consignee = AddressBuilder.Create(context, Consignee);
			wrapper.NotifyParty = AddressBuilder.Create(context, NotifyParty);
			wrapper.NotifyParty2 = AddressBuilder.Create(context, NotifyParty2);
			wrapper.NotifyParty3 = AddressBuilder.Create(context, NotifyParty3);
			if (consol.FreightPayerDocumentaryAddress.IsValidAddress)
			{
				wrapper.FreightPayer = AddressBuilder.Create(context, consol.FreightPayerDocumentaryAddress);
			}
			else
			{
				wrapper.FreightPayer = AddressBuilder.Create(context, FreightPayer);
			}

			wrapper.PickupFrom = consol.GetPickupFromAddress(context, consol.IsPickup(consol.IsDoorPickup()));
			wrapper.DeliverTo = consol.GetDeliverToAddress(context, consol.IsDeliver(consol.IsDoorDelivery()));

			if (consol.IsCoLoad)
			{
				wrapper.RecipientType = (NoResString)"Co-Load With"; // non-translatable registration number
				wrapper.Recipient = wrapper.Creditor;
			}
			else
			{
				if (wrapper.IsNVO)
				{
					wrapper.RecipientType = (NoResString)"Carrier (NVOCC)"; // non-translatable registration number
				}
				else
				{
					wrapper.RecipientType = (NoResString)"Carrier"; // non-translatable registration number
				}

				wrapper.Recipient = wrapper.Carrier;
			}

			wrapper.CurrentUser = AddressBuilder.CreateForCurrentUser(context);

			if (wrapper.DocumentName == DataContext.BookingRequest)
			{
				wrapper.CustomsBroker = consol.GetCustomsBrokerAddress(context);
			}
		}

		void PopulateTaxInfo(CarrierMessageData wrapper)
		{
			wrapper.ShipperTaxInfo = PopulateTaxInfoHelper(wrapper, wrapper.Shipper, Shipper, useExportCountry: true);
			wrapper.ConsigneeTaxInfo = PopulateTaxInfoHelper(wrapper, wrapper.Consignee, Consignee, useExportCountry: false);
			wrapper.ConsigneeTaxInfoOriginal = PopulateTaxInfoHelper(wrapper, wrapper.Consignee, Consignee, useExportCountry: false);
			wrapper.NotifyPartyTaxInfo = PopulateTaxInfoHelper(wrapper, wrapper.NotifyParty, NotifyParty, useExportCountry: false);
			wrapper.NotifyPartyTaxInfoOriginal = PopulateTaxInfoHelper(wrapper, wrapper.NotifyParty, NotifyParty, useExportCountry: false);
		}

		IReadOnlyCollection<TaxInfo> PopulateTaxInfoHelper(CarrierMessageData wrapper, Address address, object addressSource, bool useExportCountry)
		{
			if (address.IsEmpty())
			{
				return new List<TaxInfo>().AsReadOnly();
			}

			var provider = new OrgHeaderRegistrationNumberProvider(GetSourceHeader(addressSource));

			var primaryCountry = useExportCountry ? wrapper.PlaceOfReceipt?.Country : wrapper.PlaceOfDelivery?.Country;
			var primaryCountryCode = primaryCountry?.Code ?? ZString.Empty;

			var secondaryCountry = useExportCountry ? wrapper.PlaceOfDelivery?.Country : wrapper.PlaceOfReceipt?.Country;
			var secondaryCountryCode = secondaryCountry?.Code ?? ZString.Empty;

			var addressCountryCode = address.Country?.Code ?? ZString.Empty;

			var refDatas = addressCountryCode == primaryCountryCode
				? GetTaxInfoFromRefTable(addressCountryCode, provider, consol.Factory, primaryCountryCode)
				: new List<TaxCodeInformation>();
			var requiredTaxInfo = GetTaxInfoFromRefData(wrapper, refDatas, address.Country, useExportCountry).ToList();

			var secondaryRefDatas = GetTaxInfoFromRefTable(addressCountryCode, provider, consol.Factory, secondaryCountryCode);
			var secondaryRequiredTaxInfo = GetTaxInfoFromRefData(wrapper, secondaryRefDatas, address.Country, useExportCountry);

			requiredTaxInfo.AddRange(secondaryRequiredTaxInfo);

			GetAdditionalTaxInfo(wrapper, provider, primaryCountry, requiredTaxInfo, address.Country);

			if (!requiredTaxInfo.Any() && (primaryCountryCode == CountryCodes.China || secondaryCountryCode == CountryCodes.China))
			{
				return new List<TaxInfo>()
				{
					new TaxInfo
					{
						Code = "9999",
						LongLabel = "9999",
						ShortLabel = "9999",
						Description = "9999",
						IsPlaceHolder = true
					}
				};
			}

			return requiredTaxInfo.WhereNotNull().ToList().AsReadOnly();
		}

		protected IEnumerable<TaxInfo> GetTaxInfoFromRefData(CarrierMessageData wrapper, List<TaxCodeInformation> refTableData, Country country, bool useExportCountry = false) // we need to return a list since there are 2 tax id for India both with top priorty
		{
			if (refTableData == null)
			{
				return Enumerable.Empty<TaxInfo>();
			}

			string taxCode = null;
			if (!useExportCountry && CountryCodes.Bangladesh.Equals(country?.Code))
			{
				taxCode = consol.IsDirect ? OrgCusCode.CodeTypes.VATCode : OrgCusCode.BangladeshCodeTypes.AIN;
			}

			var filteredRefTableData = refTableData.Where(t =>
				t.DocumentType == Constants.TaxRelatedDocumentType.ShippingInstruction &&
				(taxCode == null || t.Code == taxCode));

			var nonEmptyList = filteredRefTableData.Where(t => !t.Number.IsEmpty);
			if (nonEmptyList.Any())
			{
				filteredRefTableData = nonEmptyList;
			}

			var countryCode = country?.Code;
			if (countryCode.HasValue && countryCode.Value != ZString.Empty)
			{
				filteredRefTableData = filteredRefTableData.Where(t => FilterDirection(t, countryCode, useExportCountry));
			}
			var taxCodeInformations = filteredRefTableData.OrderBy(t => t.Priority);
			if (!taxCodeInformations.Any())
			{
				return Enumerable.Empty<TaxInfo>();
			}

			return taxCodeInformations
				.Where(t => t.Priority == taxCodeInformations.First().Priority)
				.Select(t => GenerateTaxInfo(wrapper, t, country, useExportCountry));
		}

		bool FilterDirection(TaxCodeInformation taxCodeInformation, string countryCode, bool useExportCountry)
		{
			if (ShouldFilterDirection(taxCodeInformation, countryCode, useExportCountry))
			{
				return (useExportCountry && (taxCodeInformation.Direction == DocDataConstants.DocOrgCusCodeDirection.Export || taxCodeInformation.Direction == DocDataConstants.DocOrgCusCodeDirection.Both))
					|| (!useExportCountry && (taxCodeInformation.Direction == DocDataConstants.DocOrgCusCodeDirection.Import || taxCodeInformation.Direction == DocDataConstants.DocOrgCusCodeDirection.Both));
			}

			return true;
		}

		protected virtual bool ShouldFilterDirection(TaxCodeInformation taxCodeInformation, string countryCode, bool useExportCountry)
		{
			return taxCodeInformation.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori && IsCountryThatHasToDistinguishDirectionForEori(countryCode);
		}

		protected bool IsCountryThatHasToDistinguishDirectionForEori(string countryCode)
		{
			return (EuropeanUnionCustomsMembersProvider.IsMemberOfEU(countryCode) || countryCode.In(new String[] { CountryCodes.Norway, CountryCodes.Switzerland }));
		}

		protected virtual TaxInfo GenerateTaxInfo(CarrierMessageData wrapper, TaxCodeInformation t, Country country, bool useExportCountry)
		{
			return new TaxInfo
			{
				Code = t.Code,
				Description = t.Description,
				ShortLabel = t.ShortLabel,
				LongLabel = t.LongLabel,
				Number = t.Number,
				Country = country,
				IsChinaSpecific = t.RegulatingCountryCode == Constants.CountryCodes.China,
				RegulatingCountry = Country.Create(context, RefCountry.LoadFromCountryCode(consol.Factory, t.RegulatingCountryCode))
			};
		}

		protected virtual void GetAdditionalTaxInfo(CarrierMessageData wrapper, OrgHeaderRegistrationNumberProvider provider, Country primaryCountry, List<TaxInfo> taxInfo, Country addressCountry)
		{
		}

		protected virtual void PopulateCharges(CarrierMessageData wrapper)
		{
			wrapper.OptionalChargeBasicFreight = new OptionalCharge()
			{
				IsPrepaid = consol.JK_PrepaidCollect == Constants.PaymentType.Prepaid,
				IsCollect = consol.JK_PrepaidCollect == Constants.PaymentType.Collect
			};

			wrapper.OptionalChargeDestinationHaulage = new OptionalCharge();
			wrapper.OptionalChargeDestinationPort = new OptionalCharge();
			wrapper.OptionalChargeOriginHaulage = new OptionalCharge();
			wrapper.OptionalChargeOriginPort = new OptionalCharge();
		}

		protected ZString GetChinaTaxNumberTypes_RefDataTable(IReadOnlyCollection<TaxInfo> taxInfoCollection, ZString separator)
		{
			var resultList = taxInfoCollection.Where(t => (t.RegulatingCountry?.Code ?? ZString.Empty) == CountryCodes.China).Select(taxInfo =>
			{
				var cusCodeList = new OrgCodeLists().CustomsCodes_List(taxInfo.Country?.Code ?? ZString.Empty);
				return ZString.Format("{0} ({1})", taxInfo.Code, cusCodeList.GetDescriptionFromCode(taxInfo.Code)); // formatting string
			}).OrderBy(v => v).ToArray();
			return ZString.Join(separator, resultList);
		}

		OrgHeader GetSourceHeader(object addressSource)
		{
			OrgHeader header = null;
			if (addressSource is JobDocAddress jobDocAddressSource)
			{
				header = jobDocAddressSource.Organisation;
			}
			else if (addressSource is OrgAddress orgAddressSource)
			{
				header = orgAddressSource.Header;
			}
			else if (addressSource is ZAddressWithContact zAddressWithContact)
			{
				header = zAddressWithContact.OrgHeader as OrgHeader;
			}

			return header;
		}

		#endregion

		void PupulateDetailedPortNameToUnlocos(CarrierMessageData wrapper)
		{
			foreach (Transport transport in wrapper.Transports)
			{
				transport.PortOfLoading = Unloco.Create(context, transport.PortOfLoading).WithCustomNameProvider(GetDetailedPortName);
				transport.PortOfDischarge = Unloco.Create(context, transport.PortOfDischarge).WithCustomNameProvider(GetDetailedPortName);
				transport.Carrier.Unloco = Unloco.Create(context, transport.Carrier.Unloco).WithCustomNameProvider(GetDetailedPortName);
			}

			wrapper.Shipper.Unloco = Unloco.Create(context, wrapper.Shipper.Unloco).WithCustomNameProvider(GetDetailedPortName);
			wrapper.Carrier.Unloco = Unloco.Create(context, wrapper.Carrier.Unloco).WithCustomNameProvider(GetDetailedPortName);
			wrapper.Creditor.Unloco = Unloco.Create(context, wrapper.Creditor.Unloco).WithCustomNameProvider(GetDetailedPortName);
			wrapper.Forwarder.Unloco = Unloco.Create(context, wrapper.Forwarder.Unloco).WithCustomNameProvider(GetDetailedPortName);
			wrapper.SendingForwarder.Unloco = Unloco.Create(context, wrapper.SendingForwarder.Unloco).WithCustomNameProvider(GetDetailedPortName);
			wrapper.Buyer.Unloco = Unloco.Create(context, wrapper.Buyer.Unloco).WithCustomNameProvider(GetDetailedPortName);
			wrapper.Consignee.Unloco = Unloco.Create(context, wrapper.Consignee.Unloco).WithCustomNameProvider(GetDetailedPortName);
			wrapper.NotifyParty.Unloco = Unloco.Create(context, wrapper.NotifyParty.Unloco).WithCustomNameProvider(GetDetailedPortName);
			wrapper.NotifyParty2.Unloco = Unloco.Create(context, wrapper.NotifyParty2.Unloco).WithCustomNameProvider(GetDetailedPortName);
			wrapper.NotifyParty3.Unloco = Unloco.Create(context, wrapper.NotifyParty3.Unloco).WithCustomNameProvider(GetDetailedPortName);
			wrapper.FreightPayer.Unloco = Unloco.Create(context, wrapper.FreightPayer.Unloco).WithCustomNameProvider(GetDetailedPortName);
			wrapper.PickupFrom.Unloco = Unloco.Create(context, wrapper.PickupFrom.Unloco).WithCustomNameProvider(GetDetailedPortName);
			wrapper.DeliverTo.Unloco = Unloco.Create(context, wrapper.DeliverTo.Unloco).WithCustomNameProvider(GetDetailedPortName);
			wrapper.Recipient.Unloco = Unloco.Create(context, wrapper.Recipient.Unloco).WithCustomNameProvider(GetDetailedPortName);
			wrapper.CurrentUser.Unloco = Unloco.Create(context, wrapper.CurrentUser.Unloco).WithCustomNameProvider(GetDetailedPortName);

			foreach (var container in wrapper.Containers)
			{
				((Address)container.DepartureContainerYard).Unloco = Unloco.Create(context, container.DepartureContainerYard.Unloco).WithCustomNameProvider(GetDetailedPortName);
				container.VerifiedByAddress.Unloco = Unloco.Create(context, container.VerifiedByAddress.Unloco).WithCustomNameProvider(GetDetailedPortName);
			}

			foreach (var subShipment in wrapper.Shipments)
			{
				((Address)subShipment.Consignee).Unloco = Unloco.Create(context, subShipment.Consignee.Unloco).WithCustomNameProvider(GetDetailedPortName);
				((Address)subShipment.Consignor).Unloco = Unloco.Create(context, subShipment.Consignor.Unloco).WithCustomNameProvider(GetDetailedPortName);
				((Address)subShipment.PickupFrom).Unloco = Unloco.Create(context, subShipment.PickupFrom.Unloco).WithCustomNameProvider(GetDetailedPortName);
				((Address)subShipment.PickupCFS).Unloco = Unloco.Create(context, subShipment.PickupCFS.Unloco).WithCustomNameProvider(GetDetailedPortName);
				((Address)subShipment.DeliveryTo).Unloco = Unloco.Create(context, subShipment.DeliveryTo.Unloco).WithCustomNameProvider(GetDetailedPortName);
				((Address)subShipment.DeliveryCFS).Unloco = Unloco.Create(context, subShipment.DeliveryCFS.Unloco).WithCustomNameProvider(GetDetailedPortName);
			}
		}

		private protected string GetDetailedPortName(IRefUNLOCO refUnloco)
		{
			if (Registry.ForwardingConfigurationRegistry.Instance.ShowCountryStateBookingRequestShippingInstruction.Value)
			{
				return UnlocoExtensions.GetDetailedPortName(refUnloco);
			}

			return refUnloco?.RL_PortName ?? string.Empty;
		}

		#region Validation

		#region Addresses

		void AddAddressValidation(CarrierMessageData wrapper)
		{
			AddStandardAddressValidation(wrapper);
			AddConsigneeAddressValidation(wrapper);
			AddNotifyPartyAddressValidation(wrapper);
			AddShipperAddressValidation(wrapper);
			AddRecipientAddressValidation(wrapper);
			AddCarrierAddressValidation(wrapper);
			AddForwarderAddressValidation(wrapper);
			AddPickupFromAddressValidation(wrapper);
			AddDeliverToAddressValidation(wrapper);
			AddCurrentUserAddressValidation(wrapper);
			AddCustomsBrokerAddressValidation(wrapper);
		}

		void AddStandardAddressValidation(CarrierMessageData wrapper)
		{
			foreach (var address in new[] { wrapper.NotifyParty2, wrapper.NotifyParty3, wrapper.Buyer, wrapper.FreightPayer })
			{
				address.AddStandardAddressValidation();
			}
		}

		void AddConsigneeAddressValidation(CarrierMessageData wrapper)
		{
			var consignee = wrapper.Consignee;
			consignee
				.AddStandardAddressValidation()
				.AddPartyNameAndAddressValidation((NoResString)"Consignee", () => false) // non-translatable validation message
				.AddToOrderSupport(); // non-translatable validation message

			consignee.CompanyNameInfo.AddMessageError(() => (consignee.IsToOrder() || (string.IsNullOrWhiteSpace(consignee.CompanyName) || string.IsNullOrWhiteSpace(consignee.AddressLine1) || string.IsNullOrWhiteSpace(consignee.Country?.Name))) && (string.IsNullOrWhiteSpace(wrapper.NotifyParty.CompanyName) || wrapper.NotifyParty.IsSameAsConsignee()), (NoResString)"Consignee name and address information is required, when Notify Party is empty or SAME AS CONSIGNEE."); // non-translatable validation message
			consignee.AddValidationDependencies(consignee.CompanyNameInfo, wrapper.NotifyParty.CompanyNameInfo);
		}

		void AddNotifyPartyAddressValidation(CarrierMessageData wrapper)
		{
			var notifyParty = wrapper.NotifyParty;
			notifyParty
				.AddStandardAddressValidation()
				.AddPartyNameAndAddressValidation((NoResString)"Notify Party", () => false) // non-translatable validation message
				.AddSameAsConsigneeSupport(); // non-translatable validation message

			notifyParty.CompanyNameInfo.AddMessageError(() => (notifyParty.IsSameAsConsignee() || (string.IsNullOrWhiteSpace(notifyParty.CompanyName) || string.IsNullOrWhiteSpace(notifyParty.AddressLine1) || string.IsNullOrWhiteSpace(notifyParty.Country?.Name))) && (string.IsNullOrWhiteSpace(wrapper.Consignee.CompanyName) || wrapper.Consignee.IsToOrder()), (NoResString)"Notify Party name and address information is required, when Consignee is empty or TO ORDER."); // non-translatable validation message
			notifyParty.AddValidationDependencies(notifyParty.CompanyNameInfo, wrapper.Consignee.CompanyNameInfo);
		}

		void AddShipperAddressValidation(CarrierMessageData wrapper)
		{
			var shipper = wrapper.Shipper;
			shipper
				.AddStandardAddressValidation()
				.AddPartyNameAndAddressValidation((NoResString)"Shipper"); // non-translatable validation message
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		void AddRecipientAddressValidation(CarrierMessageData wrapper)
		{
			var recipientDescription = wrapper.IsGatewayCoload ? (NoResString)"Gateway Co-Loader"
				: wrapper.IsCoload ? "Co-Loader"
					: (NoResString)"Carrier";

			var carrier = consol.IsCoLoad ? consol.Creditor : consol.ShippingLine;
			var addContactDetailsValidation = true;
			if (carrier?.ShippingLine != null)
			{
				addContactDetailsValidation = IsShippingLineRelevantMessageOptionEnabled(GetNewCarrierMessageData().DocumentName, carrier.ShippingLine);
			}

			var recipient = wrapper.Recipient;
			recipient
				.AddStandardAddressValidation(addContactDetailsValidation: addContactDetailsValidation)
				.AddPartyNameAndAddressValidation(recipientDescription);

			var scac = recipient.GetRegistrationNumber(CountryCodes.UnitedStates, OrgCusCode.CodeTypes.CarrierCode).IsEmpty
					? recipient.GetRegistrationNumber(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode)
					: recipient.GetRegistrationNumber(CountryCodes.UnitedStates, OrgCusCode.CodeTypes.CarrierCode);

			recipient.CompanyNameInfo.AddMessageError(() => string.IsNullOrWhiteSpace(scac), FormattableString.Invariant($"{recipientDescription} SCAC is missing from {recipientDescription} organization record under Organisation > Config > Country US, Type CCC (or Type C1C)."));
			recipient.CompanyNameInfo.AddMessageError(() => scac.Length > 4, FormattableString.Invariant($"{recipientDescription} SCAC code must not exceed 4 characters."));
			recipient.CompanyNameInfo.AddMessageError(() => ValidationExtensions.HasNonAsciiCharacters(scac), FormattableString.Invariant($"Most messaging providers do not support non ASCII characters. Please edit {recipientDescription} SCAC value under Organisation > Config > Country US, Type CCC (or Type C1C)."));

			recipient.CompanyNameInfo.AddWarning(() => wrapper.ContainerMode.Code != Core.Constants.ContainerModes.FCL && wrapper.ContainerMode.Code != Core.Constants.ContainerModes.LCL,
				FormattableString.Invariant($"Please make sure this {wrapper.RecipientType} supports non-containerised messaging.  Alternatively, you can use the \"Deliver Document\" button to send this form as PDF."));

			var documentName = GetNewCarrierMessageData().DocumentName == DataContext.BookingRequest ? "Booking Request" : "Shipping Instruction";

			recipient.ContactInfo.AddMessageError(() =>
			{
				if (carrier?.ShippingLine != null)
				{
					return !IsShippingLineRelevantMessageOptionEnabled(GetNewCarrierMessageData().DocumentName, carrier.ShippingLine) && (wrapper.Recipient.Contact.IsEmpty || wrapper.Recipient.Email.IsEmpty);
				}
				else
				{
					return false;
				}
			}, consol.IsCoLoad ?
			$"This NVOCC does not support electronic {documentName}. Contact name and email address are required to send your {documentName} by email.\nPlease setup contact name and email address on Organization> Contact> Email and Receiving Documents> Group SHP" :
			$"This carrier does not support electronic {documentName}. Contact name and email address are required to send your {documentName} by email.\nPlease setup contact name and email address on Organization> Contact> Email and Receiving Documents> Group SHP");

			wrapper.Recipient.AddValidationDependencies(wrapper.Recipient.ContactInfo, wrapper.Recipient.EmailInfo);

			recipient.CompanyNameInfo.AddMessageError(() =>
			{
				return carrier != null && carrier.ShippingLine == null;
			}, consol.IsCoLoad ?
			ShippingLineMessagingRequirement.ValidationMessages.RecipientLinkShippingLine :
			ShippingLineMessagingRequirement.ValidationMessages.CarrierLinkShippingLine);

			recipient.ContactInfo.AddMessageError(() => CarrierHasIntegrationViaEmailToCarrierLocalOfficeTag() && (wrapper.Recipient.Contact.IsEmpty || wrapper.Recipient.Email.IsEmpty), IelMessageRequirementValidationContactErrorMessage);
		}

		bool IsShippingLineRelevantMessageOptionEnabled(string menuName, RefShippingLine shippingLine)
		{
			switch (menuName)
			{
				case DataContext.BookingRequest:
					return shippingLine.RSL_BookingRequestAvailable;
				case DataContext.ShippingInstruction:
					return shippingLine.RSL_ShippingInstructionAvailable;
			}
			return false;
		}

		void AddCarrierAddressValidation(CarrierMessageData wrapper)
		{
			if (wrapper.IsCoload || wrapper.IsGatewayCoload || wrapper.IsNVO)
			{
				wrapper.Carrier.AddStandardAddressValidation();
			}
		}

		void AddForwarderAddressValidation(CarrierMessageData wrapper)
		{
			var forwarder = wrapper.Forwarder;
			forwarder.AddStandardAddressValidation();
			forwarder.CompanyNameInfo.AddMessageError(() => GetType().Name == nameof(BookingRequestBuilder) && (string.IsNullOrWhiteSpace(forwarder.CompanyName) || string.IsNullOrWhiteSpace(forwarder.AddressLine1) || string.IsNullOrWhiteSpace(forwarder.Country?.Name)), (NoResString)"Forwarder party name and address information is required."); // non-translatable validation message
			forwarder.CompanyNameInfo.AddMessageError(() => GetType().Name == nameof(ShippingInstructionBuilder) && consol.IsDirect && (string.IsNullOrWhiteSpace(forwarder.CompanyName) || string.IsNullOrWhiteSpace(forwarder.AddressLine1) || string.IsNullOrWhiteSpace(forwarder.Country?.Name)), (NoResString)"Forwarder party name and address information is required for direct consolidations."); // non-translatable validation message
			forwarder.AddValidationDependencies(forwarder.CompanyNameInfo, forwarder.AddressLine1Info, forwarder.Country.NameInfo);
		}

		void AddPickupFromAddressValidation(CarrierMessageData wrapper)
		{
			var pickupFrom = wrapper.PickupFrom;
			Func<bool> precondition = () => wrapper.IsDoorPickup;

			pickupFrom.AddStandardAddressValidation(precondition);

			pickupFrom.CompanyNameInfo.AddMessageError(precondition.Then(() => string.IsNullOrEmpty(pickupFrom.CompanyName) || string.IsNullOrEmpty(pickupFrom.AddressLine1) || string.IsNullOrEmpty(pickupFrom.Country.Code)), (NoResString)"Pickup from name and address are mandatory when 'Door Pickup' is selected."); // non-translatable validation message
			pickupFrom.AddValidationDependencies(pickupFrom.CompanyNameInfo, pickupFrom.AddressLine1Info, pickupFrom.Country.CodeInfo);
		}

		void AddDeliverToAddressValidation(CarrierMessageData wrapper)
		{
			var deliverTo = wrapper.DeliverTo;
			Func<bool> precondition = () => wrapper.IsDoorDelivery;

			deliverTo.AddStandardAddressValidation(precondition);

			deliverTo.CompanyNameInfo.AddMessageError(precondition.Then(() => string.IsNullOrEmpty(deliverTo.CompanyName) || string.IsNullOrEmpty(deliverTo.AddressLine1) || string.IsNullOrEmpty(deliverTo.Country.Code)), (NoResString)"Deliver to name and address are mandatory when 'Door Delivery' is selected."); // non-translatable validation message

			deliverTo.AddValidationDependencies(deliverTo.CompanyNameInfo, deliverTo.AddressLine1Info, deliverTo.Country.CodeInfo);
		}

		void AddCurrentUserAddressValidation(CarrierMessageData wrapper)
		{
			wrapper.CurrentUser.AddAsciiCharactersValidation();
		}

		void AddAcidNumberValidation(CarrierMessageData wrapper)
		{
			if (wrapper.IsToEgypt)
			{
				wrapper.AcidNumberInfo.AddMessageErrorIfEmpty((NoResString)"ACID Number is mandatory for cargo with destination Egypt. Enter the ACI in the Additional References on the Consol.");// non-translatable validation message
			}
		}

		void AddCustomsBrokerAddressValidation(CarrierMessageData wrapper)
		{
			if (wrapper.DocumentName == DataContext.BookingRequest)
			{
				var customsBroker = wrapper.CustomsBroker;
				Func<bool> precondition = () => consol.IsDirect;

				customsBroker.AddStandardAddressValidation(precondition);

				customsBroker.CompanyNameInfo.AddWarning(precondition.Then(() => string.IsNullOrEmpty(customsBroker.CompanyName) || string.IsNullOrEmpty(customsBroker.AddressLine1) || string.IsNullOrEmpty(customsBroker.Country.Code)),
					Res.GetString("d617a020-7beb-4dfe-8f5b-06e49b7a2231", "Customs Broker details may be required by the Carrier for Exports from certain locations.\r\nConfirm with the Carrier and update this information on the Shipment if necessary."));
				customsBroker.AddValidationDependencies(customsBroker.CompanyNameInfo, customsBroker.AddressLine1Info, customsBroker.Country.CodeInfo);
			}
		}

		#endregion

		#region Ports Validation

		void AddPortsValidation(CarrierMessageData wrapper)
		{
			var enterValidUnlocoMessageError = (NoResString)"You have not entered a valid unloco."; // non-translatable validation message

			wrapper.PortOfLoading.CodeInfo.AddMessageErrorIfEmpty((NoResString)"Port of Loading is required."); // non-translatable validation message
			wrapper.PortOfLoading.CodeInfo.AddAsciiCharactersValidation();
			wrapper.PortOfLoading.CodeInfo.AddInvalidCodeValidation(enterValidUnlocoMessageError);

			wrapper.PortOfDischarge.CodeInfo.AddMessageErrorIfEmpty((NoResString)"Port of Discharge is required."); // non-translatable validation message
			wrapper.PortOfDischarge.CodeInfo.AddAsciiCharactersValidation();
			wrapper.PortOfDischarge.CodeInfo.AddInvalidCodeValidation(enterValidUnlocoMessageError);

			wrapper.Origin.CodeInfo.AddAsciiCharactersValidation();
			wrapper.Origin.CodeInfo.AddInvalidCodeValidation(enterValidUnlocoMessageError);
			wrapper.Destination.CodeInfo.AddAsciiCharactersValidation();
			wrapper.Destination.CodeInfo.AddInvalidCodeValidation(enterValidUnlocoMessageError);
			wrapper.PlaceOfReceipt.CodeInfo.AddAsciiCharactersValidation();
			wrapper.PlaceOfReceipt.CodeInfo.AddInvalidCodeValidation(enterValidUnlocoMessageError);
			wrapper.PlaceOfDelivery.CodeInfo.AddAsciiCharactersValidation();
			wrapper.PlaceOfDelivery.CodeInfo.AddInvalidCodeValidation(enterValidUnlocoMessageError);
			wrapper.PlaceOfIssue.CodeInfo.AddAsciiCharactersValidation();
			wrapper.PlaceOfIssue.CodeInfo.AddInvalidCodeValidation(enterValidUnlocoMessageError);
		}

		#endregion

		#region Transports Validation

		void AddTransportsValidation(CarrierMessageData wrapper)
		{
			var mainTransport = wrapper.Transports?.Main;
			if (mainTransport != null)
			{
				((CodeDescription)mainTransport.Mode)?.CodeInfo.AddMessageError(() => mainTransport.Mode.Code != Core.Constants.TransportModes.Sea, (NoResString)"Main transport leg mode must be SEA."); // non-translatable validation message
				mainTransport.ETDInfo.AddMessageError(() => mainTransport.ETD.IsEmpty, (NoResString)"ETD is required"); // non-translatable validation message
			}

			wrapper.ErrorPlaceHolderInfo.AddMessageError(() => mainTransport == null, Res.GetString("F85507DB-5B82-4B75-9E55-7FCB439F601B", "Main Sea leg is required."));
		}

		#endregion

		#region Container Validation

		void AddContainerValidations(CarrierMessageData wrapper)
		{
			foreach (Container container in wrapper.Containers)
			{
				AddContainerValidation(wrapper, container);
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected virtual void AddContainerValidation(CarrierMessageData wrapper, Container container)
		{
			if (!wrapper.HasShipments && !wrapper.IsDirect)
			{
				container.GoodsWeight.ValueInfo.AddMessageErrorIfEmpty((NoResString)"Total cargo weight (net weight) is required. Please enter a value.");  // non-translatable validation message.
				container.GoodsWeight.ValueInfo.AddWarning(() => true, (NoResString)"The value entered here will be lost, once shipments are packed to this container.\r\nBooking Request replacement message can be sent later if required, when correct value is known."); // non-translatable registration number
			}

			var mandatoryWarning = (NoResString)"You have not entered a value. This may result in processing delays."; // non-translatable registration number
			container.TareWeight.ValueInfo.AddWarning(() => !wrapper.IsNonContainerized && container.TareWeight.Value <= 0, mandatoryWarning);
			((Measurement)container.GrossWeight)?.ValueInfo.AddWarning(() => container.GrossWeight.Value <= 0, mandatoryWarning);
			((CodeDescription)container.AirVentFlow.Unit).CodeInfo.AddMessageError(() => container.HasControlledAtmosphere && container.AirVentFlow.Value != 0 && container.AirVentFlow.Unit.Code != "2L" && container.AirVentFlow.Unit.Code != "MQH", Res.GetString("1098DF0E-5242-4D5A-B2E9-76883E02305F", "Selected Air Vent Setting measurement type is not supported by the carrier. Ensure each temperature controlled container has selected either '2L' or 'MQH' on the Containers > Refrigeration tab."));
			((CodeDescription)container.AirVentFlow.Unit).CodeInfo.AddWarning(() => container.AirVentFlow.Value == 0 && !container.AirVentFlow.Unit.Code.IsEmpty, (NoResString)"Air Vent is marked as \"Open\".\r\nTo indicate \"Closed\", leave the Consol > Containers > Refrigeration > Air Vent Setting > Unit field blank.");
			((CodeDescription)container.AirVentFlow.Unit).CodeInfo.AddWarning(() => container.AirVentFlow.Value != 0 && container.AirVentFlow.Unit.Code == "2L", (NoResString)"Carriers accept airflow only in metric units.\r\nAny value entered as cubic feet per minute (2L) is automatically converted to cubic meters per hour (MQH) during transmission.");
			((Measurement)container.Volume).ValueInfo.AddWarning(() => container.Volume.Value <= 0, mandatoryWarning);
			container.SetTemperature.ValueInfo.AddMessageError(() => container.HasControlledAtmosphere && container.SetTemperature.Unit.Code.IsEmpty, (NoResString)"The default temperature has not yet been verified by the user. Please check the temperature and the unit of temperature against the container on the Consol"); // non-translatable registration number

			var messageContainerNumberValidation = Res.GetString("971659d1-8355-4934-b4d6-cad3ac8041b3", "Container number does not confirm to ISO standard of 4 letters followed by 6 digits and a check digit.");
			container.NumberInfo.AddWarning(() => !wrapper.IsNonContainerized && !container.Number.IsEmpty && !ContainerNumberValidation.IsValidContainerNumber(container.Number) && container.IsShipperOwned, messageContainerNumberValidation);
			container.NumberInfo.AddMessageError(() => !wrapper.IsNonContainerized && !container.Number.IsEmpty && !ContainerNumberValidation.IsValidContainerNumber(container.Number) && !container.IsShipperOwned, messageContainerNumberValidation);
			container.NumberInfo.AddAsciiCharactersValidation();

			if (!wrapper.IsColoadLCL)
			{
				container.Type.ISOCodeInfo.AddWarning(() =>
				{
					var containerISOType = new ContainerISOType();
					containerISOType.ISOCode = container.Type.ISOCode;
					return !container.Type.ISOCode.IsEmpty && !wrapper.IsNonContainerized && !containerISOType.IsKnown;
				}, (NoResString)"Container type entered does not have a valid ISO equipment code. Enter a valid ISO code to the matching Container Reference file."); // non-translatable registration number
				container.Type.ISOCodeInfo.AddMessageError(() => container.Type.ISOCode.IsEmpty, (NoResString)"This container does not have a valid ISO Code. Enter a Valid ISO Code to the Container Reference File via Consol > Container > Container Type."); // non-translatable registration number

				container.Type.ISOCodeInfo.AddAsciiCharactersValidation();
				container.Type.CodeInfo.AddAsciiCharactersValidation();
			}

			if (!wrapper.IsNonContainerized)
			{
				var refContainer = new RefContainer.Loader(consol.Factory).LoadFromCode(container.Type.Code);

				((Measurement)container.Volume).ValueInfo.AddWarning(() =>
					refContainer != null
					&& (consol.JK_ConsolMode != Constants.ContainerModes.FCL || !new Regex(@"^..[PU].$", RegexOptions.IgnoreCase).IsMatch(refContainer.RC_ISOType))
					&& container.Volume.Value > Constants.Volume.Convert(refContainer.RC_CubicCapacity, Constants.Volume.CubicMetres, wrapper.UnitOfVolume), (NoResString)"Volume exceeds the Capacity for this container type.");
			}
		}

		#endregion

		#region PackingLines Validation

		void AddPackingLinesValidation(CarrierMessageData wrapper)
		{
			if (IsGroupAndConsolidatePackingLines)
			{
				foreach (var packingLine in wrapper.Shipments.SelectMany(x => x.PackingLines))
				{
					AddPackingLineValidation(wrapper, packingLine);
				}

				foreach (var packingLine in wrapper.Shipments.SelectMany(x => x.AllPackingLinesIncludeCoLoad))
				{
					AddDangerousGoodsValidation(packingLine);

					packingLine.Weight.ValueInfo.AddMessageError(() => packingLine.Weight.IsNull || packingLine.Weight.Value <= 0,
(NoResString)"No weight has been allocated to this container.\r\nPlease verify in Shipment > Packing > Weight."); // non-translatable registration number
				}
			}
			else
			{
				foreach (var packingLine in wrapper.Shipments.SelectMany(x => x.AllPackingLinesIncludeCoLoad))
				{
					AddPackingLineValidation(wrapper, packingLine);
					AddDangerousGoodsValidation(packingLine);

					if (FreightDataRegistry.Instance.EnablePackageGrouping.Value)
					{
						packageGroupingHelper.AddNoInnerPackLineValidation(packingLine, new List<PackingLine> { packingLine });
					}
				}
			}

			if (HasUnAllocatedPackLines(wrapper))
			{
				var unpackedValidationMessage = Res.GetString("a90a45c1-5722-4b93-b44e-140ed335c9bc", "There are pack lines on the Consolidation not packed to a container, they may not be included in the message to the carrier. Please fix before sending the message.");
				wrapper.ErrorPlaceHolderInfo.AddMessageError(() => wrapper.Containers.Any() && (wrapper.ContainerMode.Code == Constants.ContainerModes.LCL || wrapper.ContainerMode.Code == Constants.ContainerModes.FCL), unpackedValidationMessage);
			}
		}

		protected virtual void AddPackingLineValidation(CarrierMessageData wrapper, PackingLine packingLine)
		{
			AddPackingLinesQuantityValidation(wrapper, packingLine);
			AddPackingWeightAndVolumeValidation(wrapper, packingLine);

			AddHarmonizedCodesValidation(packingLine);

			((CodeDescription)packingLine.PackageType)?.CodeInfo.AddAsciiCharactersValidation();

			packingLine.MarksAndNumbersInfo.AddAsciiCharactersValidation();

			packingLine.GoodsDescriptionInfo.AddMessageErrorIfEmpty((NoResString)"Goods Description is required."); // non-translatable registration number
			packingLine.GoodsDescriptionInfo.AddAsciiCharactersValidation();
			packingLine.DetailedGoodsDescriptionInfo.AddAsciiCharactersValidation();

			packingLine.ShipmentEntryNumbersInfo.AddAsciiCharactersValidation();
		}

		bool HasUnAllocatedPackLines(CarrierMessageData wrapper)
		{
			if (IsGroupAndConsolidatePackingLines)
			{
				return packageGroupingHelper.HasUnAllocatedPackLines;
			}

			var allContainerPackingLinesCount = wrapper.Containers.SelectMany(c => c.PackingLines).OfType<PackingLine>().Count();
			var allPackingLineCount = wrapper.Shipments.SelectMany(x => x.AllPackingLinesIncludeCoLoad).OfType<PackingLine>().Count();

			return allPackingLineCount > 0 && allPackingLineCount > allContainerPackingLinesCount;
		}

		void AddPackingLinesQuantityValidation(CarrierMessageData wrapper, PackingLine packingLine)
		{
			foreach (var container in wrapper.Containers)
			{
				packingLine.QuantityInfo.AddMessageError(() => !IsGroupAndConsolidatePackingLines && packingLine.Quantity <= 0 && (wrapper.IsNonContainerized || !container.IsEmpty), (NoResString)"You have not entered a value. If this is intended, please flag the container as empty."); // non-translatable validation message
				packingLine.QuantityInfo.AddMessageError(() => packingLine.AnyPackCountIsZeroInGroupedSubPackLines && (wrapper.IsNonContainerized || !container.IsEmpty), $"There are pack lines with zero (0) quantity.\r\nPlease verify in Shipment>Packing>Packs on following Shipments:\r\n{packingLine.ShipmentIDWithAnyPackCountIsZeroInGroupedSubPackLines}."); // non-translatable validation message
			}
		}

		void AddDangerousGoodsValidation(PackingLine packingLine)
		{
			foreach (var dangerousGood in packingLine.DangerousGoods)
			{
				dangerousGood.Validator = () => AddDangerousGoodsValidation(dangerousGood);
			}
		}

		static IEnumerable<string> AddDangerousGoodsValidation(DangerousGood dangerousGood)
		{
			if (dangerousGood.FlashPoint != null && dangerousGood.FlashPoint.Value.ToString().Trim('-').Length > 5)
			{
				yield return (NoResString)"Flashpoint temperature must contain up to 3 numeric digits (excluding plus/minus sign and decimal)."; // non-translatable registration number
			}

			if (dangerousGood.FlashPoint != null && Utilities.Round(Math.Abs(dangerousGood.FlashPoint.Value), 0) >= 1000m)
			{
				yield return (NoResString)"System does not support Flashpoint temperature greater than or equal to 1000 (including values round to 1000)."; // non-translatable registration number
			}

			if (dangerousGood.Contact == null && (dangerousGood.Standard == UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA || dangerousGood.Standard == UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO))
			{
				yield return (NoResString)"Contact is required for dangerous goods."; // non-translatable registration number
			}

			if (dangerousGood.Contact != null && dangerousGood.Contact.FullName.IsEmpty)
			{
				yield return (NoResString)"Contact Name is required for dangerous goods."; // non-translatable registration number
			}

			if (dangerousGood.Contact != null && dangerousGood.Contact.Phone.IsEmpty)
			{
				yield return (NoResString)"Contact Phone is required for dangerous goods."; // non-translatable registration number
			}

			if (dangerousGood.IMOClass.IsEmpty || dangerousGood.Code.IsEmpty || dangerousGood.ProperShippingName.IsEmpty)
			{
				yield return (NoResString)"DG Class, UNDG and Proper Shipping Name are required for dangerous goods.\r\nPlease enter Shipment > Packing > Pack Lines > Dangerous Goods > DG Substance."; // non-translatable registration number
			}

			if (ValidationExtensions.HasNonAsciiCharacters(dangerousGood.Contact?.FullName ?? ZString.Empty))
			{
				yield return (NoResString)"Contact Name do not support non ASCII characters."; // non-translatable validation message
			}

			if (ValidationExtensions.HasNonAsciiCharacters(dangerousGood.IMOClass))
			{
				yield return $"{dangerousGood.IMOClassInfo.HumanReadableName} do not support non ASCII characters."; // non-translatable validation message
			}

			if (ValidationExtensions.HasNonAsciiCharacters(dangerousGood.TechnicalName))
			{
				yield return $"{dangerousGood.TechnicalNameInfo.HumanReadableName} do not support non ASCII characters."; // non-translatable validation message
			}

			if (ValidationExtensions.HasNonAsciiCharacters(((CodeDescription)dangerousGood.MarinePollutant)?.Code ?? ZString.Empty))
			{
				yield return $"Marine Pollutant do not support non ASCII characters."; // non-translatable validation message
			}
		}

		void AddPackingWeightAndVolumeValidation(CarrierMessageData wrapper, PackingLine packingLine)
		{
			packingLine.Weight.ValueInfo.AddMessageError(() => packingLine.Weight.IsNull || packingLine.Weight.Value <= 0, (NoResString)"Total packing line weight is required. Please enter a value."); // non-translatable registration number
			packingLine.Weight.ValueInfo.AddWarning(() => packingLine.AnyPackWeightIsZeroInGroupedSubPackLines, $"There are pack lines with zero (0) Weight.\r\nPlease verify in Shipment>Packing>Weight on following Shipments:\r\n{packingLine.ShipmentIDWithAnyPackWeightIsZeroInGroupedSubPackLines}."); // non-translatable registration number

			packingLine.Volume.ValueInfo.AddMessageError(() => !IsGroupAndConsolidatePackingLines && (wrapper.IsCoload || wrapper.IsGatewayCoload || wrapper.IsNVO) && (packingLine.Volume.IsNull || packingLine.Volume.Value <= 0), (NoResString)"The total packing line volume is zero. Please enter a value."); // non-translatable registration number
			packingLine.Volume.ValueInfo.AddMessageError(() => (wrapper.IsCoload || wrapper.IsGatewayCoload || wrapper.IsNVO) && packingLine.AnyPackVolumeIsZeroInGroupedSubPackLines, $"There are pack lines with zero (0) Volume.\r\nPlease verify in Shipment>Packing>Volume on following Shipments:\r\n{packingLine.ShipmentIDWithAnyPackVolumeIsZeroInGroupedSubPackLines}."); // non-translatable registration number

			packingLine.Volume.ValueInfo.AddWarning(() =>
				(
					(consol.ShippingLineIsNVOCC && (consol.IsAgent || consol.IsDirect || consol.IsCoLoad))
					|| (consol.CreditorIsNVOCC && consol.IsCoLoad)
				)
				&& Constants.Weight.IsImperial(wrapper.UnitOfWeight) != Constants.Volume.IsImperial(wrapper.UnitOfVolume),
				Res.GetString("D33C0D75-06FA-45F8-91BC-6079D91F4AC9", "A mixed use of metric and imperial units has been detected, which may result in an inaccurate volume within the NVOCCs application.\r\nPlease ensure volume and weight are entered in the same unit system."));
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable registration number")]
		void AddHarmonizedCodesValidation(PackingLine packingLine)
		{
			const string errorMessage = "Harmonized Code should not start with dot or empty space.";

			if (IsGroupAndConsolidatePackingLines)
			{
				((HarmonizedCode)packingLine.HarmonizedCode)?.CodeInfo.AddMessageError(() =>
				{
					var harmonizedCodes = packingLine.HarmonizedCode.Code.Split(", ");

					return harmonizedCodes.Any(x => x.StartsWith(".", StringComparison.OrdinalIgnoreCase) || x.StartsWith(" ", StringComparison.OrdinalIgnoreCase));
				}, errorMessage);
			}
			else
			{
				((HarmonizedCode)packingLine.HarmonizedCode)?.CodeInfo.AddMessageError(() => packingLine.HarmonizedCode.Code.StartsWith(".", StringComparison.OrdinalIgnoreCase) || packingLine.HarmonizedCode.Code.StartsWith(" ", StringComparison.OrdinalIgnoreCase), errorMessage);
			}

			((HarmonizedCode)packingLine.HarmonizedCode)?.CodeInfo.AddAsciiCharactersValidation();
		}

		#endregion

		#region Charges Validation

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable registration number")]
		protected virtual void AddChargesValidation(CarrierMessageData wrapper)
		{
			var requirePPDOrCCXOrELSForFreightChargesMessageError = (NoResString)"One of the three types[Prepaid, Collect, PayableElsewhere] must be selected for 'Freight Charges'.";
			OptionalCharge optionalChargeBasicFreight = (OptionalCharge)wrapper.OptionalChargeBasicFreight;

			optionalChargeBasicFreight.IsPrepaidInfo.AddMessageError(() => !optionalChargeBasicFreight.IsPrepaid && !optionalChargeBasicFreight.IsCollect && !optionalChargeBasicFreight.IsPayableElsewhere, requirePPDOrCCXOrELSForFreightChargesMessageError);
			optionalChargeBasicFreight.IsCollectInfo.AddMessageError(() => !optionalChargeBasicFreight.IsPrepaid && !optionalChargeBasicFreight.IsCollect && !optionalChargeBasicFreight.IsPayableElsewhere, requirePPDOrCCXOrELSForFreightChargesMessageError);
			optionalChargeBasicFreight.IsPayableElsewhereInfo.AddMessageError(() => !optionalChargeBasicFreight.IsPrepaid && !optionalChargeBasicFreight.IsCollect && !optionalChargeBasicFreight.IsPayableElsewhere, requirePPDOrCCXOrELSForFreightChargesMessageError);
			optionalChargeBasicFreight.IsPayableElsewhereInfo.AddWarning(() => optionalChargeBasicFreight.IsPayableElsewhere, "For carriers not supporting 'Payable Elsewhere', 'Collect' will be sent with the chosen Freight Payable At location.");
			optionalChargeBasicFreight.AddValidationDependencies(optionalChargeBasicFreight.IsPrepaidInfo, optionalChargeBasicFreight.IsCollectInfo, optionalChargeBasicFreight.IsPayableElsewhereInfo);
			optionalChargeBasicFreight.AddValidationDependencies(optionalChargeBasicFreight.IsCollectInfo, optionalChargeBasicFreight.IsPrepaidInfo, optionalChargeBasicFreight.IsPayableElsewhereInfo);
			optionalChargeBasicFreight.AddValidationDependencies(optionalChargeBasicFreight.IsPayableElsewhereInfo, optionalChargeBasicFreight.IsPrepaidInfo, optionalChargeBasicFreight.IsCollectInfo);
		}

		#endregion

		#region Carrier Contract Number Validation

		void AddCarrierContractNumberValidation(CarrierMessageData wrapper)
		{
			wrapper.CarrierContractNumbersFormattedInfo.AddWarningIfEmpty((NoResString)"It is recommended to fill in Carrier Contract or Quote Number to assist with faster booking and reconciliation processes."); // non-translatable validation message
		}

		#endregion

		#region ShippingLineMessagingRequirements Validation

		protected void AddShippingLineMessagingRequirementsValidation(CarrierMessageData wrapper, Func<AutoRefShippingLineMessagingRequirement, bool> condition)
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
				foreach (var requirement in requirements.Where(condition))
				{
					switch (requirement.RSR_RST_NKType)
					{
						case ShippingLineMessagingRequirement.Types.ContractNumberMandatory:
							wrapper.CarrierContractNumbersFormattedInfo.AddMessageError(() => wrapper.CarrierContractNumbersFormatted.IsEmpty, ShippingLineMessagingRequirement.ValidationMessages.ContractNumberMandatory);
							break;
						case ShippingLineMessagingRequirement.Types.NamedAccountMandatory:
							wrapper.ContractNamedAccountInfo.AddMessageError(() => wrapper.ContractNamedAccount.IsEmpty, ShippingLineMessagingRequirement.ValidationMessages.NamedAccountMandatory);
							break;
						case ShippingLineMessagingRequirement.Types.DGNetWeightMandatory:
							AddDangerousGoodsNetWeightValidation(wrapper);
							break;
						case ShippingLineMessagingRequirement.Types.AcceptEitherAirflowOrHumidity:
							AddContainersAirflowOrHumidityValidation(wrapper);
							break;
						case ShippingLineMessagingRequirement.Types.DimensionsMandatoryForOOG:
							AddPackingLinesDimensionsValidation(wrapper);
							break;
						case ShippingLineMessagingRequirement.Types.SealNumberMandatory:
							AddContainersSealNumberValidation(wrapper);
							break;
					}
				}

				if (!IsGroupAndConsolidatePackingLines && !requirements.Where(condition).Any(r => r.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.DimensionsMandatoryForOOG))
				{
					AddContainerOverhangValidation(wrapper);
				}
			}
		}

		protected abstract void AddContainersSealNumberValidation(CarrierMessageData wrapper);

		void AddDangerousGoodsNetWeightValidation(CarrierMessageData wrapper)
		{
			foreach (var dangerousGood in wrapper.Shipments.SelectMany(s => s.AllPackingLinesIncludeCoLoad).SelectMany(p => p.DangerousGoods))
			{
				var weight = dangerousGood.Weight;
				weight.ValueInfo.AddMessageError(() => weight.Value <= 0 || weight.Unit.Code.IsEmpty, ShippingLineMessagingRequirement.ValidationMessages.DGNetWeightMandatory);
			}
		}

		void AddContainersAirflowOrHumidityValidation(CarrierMessageData wrapper)
		{
			foreach (var (humidity, airVentFlow) in wrapper.Containers.Where(c => c.HasControlledAtmosphere).Select(c => (c.Humidity, c.AirVentFlow)))
			{
				var hasHumidity = humidity.Value > 0;
				var hasAirVentFlow = !airVentFlow.Unit.Code.IsEmpty;
				var rule = (hasHumidity && hasAirVentFlow) || (!hasHumidity && !hasAirVentFlow && airVentFlow.Value != 0);

				humidity.ValueInfo.AddMessageError(() => rule, ShippingLineMessagingRequirement.ValidationMessages.AcceptEitherAirflowOrHumidity);
				airVentFlow.ValueInfo.AddMessageError(() => rule, ShippingLineMessagingRequirement.ValidationMessages.AcceptEitherAirflowOrHumidity);
			}
		}

		void AddPackingLinesDimensionsValidation(CarrierMessageData wrapper)
		{
			if (!IsGroupAndConsolidatePackingLines)
			{
				bool HasMissingOverhangValuesOnPackingLine(PackingLine packingLine)
				{
					return wrapper.IsOutOfGauge && (packingLine.Length.IsNull || packingLine.Length.Value <= 0) && (packingLine.Width.IsNull || packingLine.Width.Value <= 0) && (packingLine.Height.IsNull || packingLine.Height.Value <= 0);
				}

				var flatRackOrOpenTopContainers = wrapper.Containers.Where(c =>
					c.Type.ISOCode.Length > 2
					&& (c.Type.ISOCode[2].Equals(ShippingLineMessagingRequirement.ContainerTypes.OpenTop)
							|| c.Type.ISOCode[2].Equals(ShippingLineMessagingRequirement.ContainerTypes.FlatRack)));

				foreach (var packingLine in flatRackOrOpenTopContainers.SelectMany(c => c.PackingLines).OfType<PackingLine>())
				{
					packingLine.Length.ValueInfo.AddMessageError(() => HasMissingOverhangValuesOnPackingLine(packingLine), ShippingLineMessagingRequirement.ValidationMessages.DimensionsMandatoryForOOGPackline);
					packingLine.Width.ValueInfo.AddMessageError(() => HasMissingOverhangValuesOnPackingLine(packingLine), ShippingLineMessagingRequirement.ValidationMessages.DimensionsMandatoryForOOGPackline);
					packingLine.Height.ValueInfo.AddMessageError(() => HasMissingOverhangValuesOnPackingLine(packingLine), ShippingLineMessagingRequirement.ValidationMessages.DimensionsMandatoryForOOGPackline);

					packingLine.Length.AddValidationDependencies(packingLine.Length.ValueInfo, wrapper.IsOutOfGaugeInfo);
					packingLine.Width.AddValidationDependencies(packingLine.Width.ValueInfo, wrapper.IsOutOfGaugeInfo);
					packingLine.Height.AddValidationDependencies(packingLine.Height.ValueInfo, wrapper.IsOutOfGaugeInfo);
				}
			}

			wrapper.ErrorPlaceHolderInfo.AddMessageError(() => IsGroupAndConsolidatePackingLines && (packageGroupingHelper.AddDimensionsMandatoryErrorForOOG || wrapper.IsOutOfGauge), Res.GetString("d1088020-87df-45bf-a282-2d4f7fa34ef7", "This carrier requires Dimensions for out of gauge cargo for Open Top or Flat Rack containers.\r\nPlease ensure Consol > Details > Docs > Package Grouping is set to 'DNG' and dimensions have been entered against each pack line."));
			wrapper.AddValidationDependencies(wrapper.ErrorPlaceHolderInfo, wrapper.IsOutOfGaugeInfo);
		}

		void AddContainerOverhangValidation(CarrierMessageData wrapper)
		{
			bool HasMissingOverhangValuesOnContainer(Container container)
			{
				return wrapper.IsOutOfGauge && (container.OverhangLength.Value <= 0 || container.OverhangWidth.Value <= 0 || container.OverhangHeight.Value <= 0);
			}

			var flatRackOrOpenTopContainers = wrapper.Containers.Where(c =>
				c.Type.ISOCode.Length > 2
				&& (c.Type.ISOCode[2].Equals(ShippingLineMessagingRequirement.ContainerTypes.OpenTop)
						|| c.Type.ISOCode[2].Equals(ShippingLineMessagingRequirement.ContainerTypes.FlatRack)));

			wrapper.IsOutOfGaugeInfo.AddMessageError(() => flatRackOrOpenTopContainers.Any(c => HasMissingOverhangValuesOnContainer(c)), ShippingLineMessagingRequirement.ValidationMessages.DimensionsMandatoryForOOGContainer);
		}

		#endregion

		#region CarrierMessagingRequirements Validation

		protected virtual void AddCarrierMessagingRequirementsValidation(CarrierMessageData wrapper)
		{
			if (IsHarmonizedCodeMandatory())
			{
				var packingLines = IsGroupAndConsolidatePackingLines ? wrapper.Shipments.SelectMany(x => x.PackingLines) : wrapper.Shipments.SelectMany(x => x.AllPackingLinesIncludeCoLoad);
				foreach (var packingLine in packingLines)
				{
					((HarmonizedCode)packingLine.HarmonizedCode).CodeInfo.AddMessageError(() => ((HarmonizedCode)packingLine.HarmonizedCode).Code.IsEmpty
						&& ((HarmonizedCode)packingLine.ImportHarmonizedCode).Code.IsEmpty
						&& ((HarmonizedCode)packingLine.ExportHarmonizedCode).Code.IsEmpty,
						Res.GetString("b831c7ca-dcba-4a89-afeb-2eeee71c2e5d", "The carrier requires HS code for each pack line."));
				}
			}
		}

		#endregion

		#endregion

		#region OnValueChanged

		void AddValueChangeHandlers(CarrierMessageData wrapper)
		{
			AddPickupFromValueChangeHandlers(wrapper);
			AddDeliverToValueChangeHandlers(wrapper);
		}

		void AddPickupFromValueChangeHandlers(CarrierMessageData wrapper)
		{
			wrapper.OnValueChanged(nameof(wrapper.IsDoorPickup)).Do(() =>
			{
				UpdateAddress(wrapper.PickupFrom, consol.GetPickupFromAddress(context, consol.IsPickup(wrapper.IsDoorPickup)));
				AddPickupFromAddressValidation(wrapper);
				wrapper.PickupFrom.ValidateAllIncludingChildren();

				wrapper.ErrorPlaceHolderInfo.AddMessageError(() => wrapper.IsDoorPickup && wrapper.PickupFrom.HasMessageErrors, (NoResString)"Door Pickup is selected but the Pickup From address is incomplete."); // non-translatable validation message
				wrapper.Validate(nameof(wrapper.ErrorPlaceHolder));
			});

			wrapper.PickupFrom.PropertyValueChanged += (s, e) =>
			{
				AddPickupFromAddressValidation(wrapper);
				wrapper.PickupFrom.ValidateAllIncludingChildren();
				wrapper.Validate(nameof(wrapper.ErrorPlaceHolder));
			};
		}

		void AddDeliverToValueChangeHandlers(CarrierMessageData wrapper)
		{
			wrapper.OnValueChanged(nameof(wrapper.IsDoorDelivery)).Do(() =>
			{
				UpdateAddress(wrapper.DeliverTo, consol.GetDeliverToAddress(context, consol.IsDeliver(wrapper.IsDoorDelivery)));
				AddDeliverToAddressValidation(wrapper);
				wrapper.DeliverTo.ValidateAllIncludingChildren();

				wrapper.ErrorPlaceHolderInfo.AddMessageError(() => wrapper.IsDoorDelivery && wrapper.DeliverTo.HasMessageErrors, (NoResString)"Door Delivery is selected but the Deliver To address is incomplete."); // non-translatable validation message
				wrapper.Validate(nameof(wrapper.ErrorPlaceHolder));
			});

			wrapper.DeliverTo.PropertyValueChanged += (s, e) =>
			{
				AddDeliverToAddressValidation(wrapper);
				wrapper.DeliverTo.ValidateAllIncludingChildren();
				wrapper.Validate(nameof(wrapper.ErrorPlaceHolder));
			};
		}

		void UpdateAddress(Address address, Address newAddress)
		{
			address.CompanyName = newAddress.CompanyName;
			address.AddressLine1 = newAddress.AddressLine1;
			address.AddressLine2 = newAddress.AddressLine2;
			address.AdditionalAddressInformation = newAddress.AdditionalAddressInformation;
			address.City = newAddress.City;
			address.State = newAddress.State;
			address.Postcode = newAddress.Postcode;

			address.Phone = newAddress.Phone;
			address.Fax = newAddress.Fax;
			address.Email = newAddress.Email;
			address.Contact = newAddress.Contact;
			address.AddressFormatted = newAddress.AddressFormatted;

			address.TaxNumber = newAddress.TaxNumber;
			address.TaxNumberType = newAddress.TaxNumberType;

			address.Country = newAddress.Country;
			address.Unloco = newAddress.Unloco;
			address.RegistrationNumbers = newAddress.RegistrationNumbers;
		}

		#endregion

		protected object Shipper => GetShipper();

		object GetShipper()
		{
			if (consol.IsDirect)
			{
				return DirectShipment?.ConsignorDocumentaryAddress;
			}
			if (consol.MasterBillShipperOverrideDocumentaryAddress.IsValidAddress)
			{
				return consol.MasterBillShipperOverrideDocumentaryAddress;
			}
			return consol.SendingForwarderWithContact;
		}

		protected virtual object Consignee => GetConsignee();

		object GetConsignee()
		{
			if (consol.IsDirect)
			{
				return DirectShipment?.ConsigneeDocumentaryAddress;
			}
			if (consol.MasterBillConsigneeOverrideDocumentaryAddress.IsValidAddress)
			{
				return consol.MasterBillConsigneeOverrideDocumentaryAddress;
			}
			return consol.ReceivingForwarderWithContact;
		}

		JobDocAddress Buyer => consol.IsDirect
			? DirectShipment?.BuyerDocAddress
			: null;

		protected virtual JobDocAddress NotifyParty => consol.IsDirect
			? DirectShipment?.NotifyPartyDocumentaryAddress
			: consol.NotifyPartyDocumentaryAddress;

		JobDocAddress NotifyParty2 => consol.IsDirect
			? DirectShipment?.NotifyParty2DocumentaryAddress
			: consol.NotifyParty2DocumentaryAddress;

		JobDocAddress NotifyParty3 => consol.IsDirect
			? DirectShipment?.NotifyParty3DocumentaryAddress
			: consol.NotifyParty3DocumentaryAddress;

		#region Locations

		RefUNLOCO Origin
		{
			get
			{
				if (origin == null)
				{
					origin = GetOriginFromShipments() ?? consol.LoadPort;
				}

				return origin;
			}
		}

		RefUNLOCO origin;

		RefUNLOCO GetOriginFromShipments()
		{
			var shipments = consol
				.Shipments
				.OfType<ForwardingShipment>()
				.ToArray();

			var origin = shipments
				.FirstOrDefault()?
				.Origin;

			return origin != null && (consol.IsDirect || shipments.Skip(1).All(s => s.Origin != null && s.Origin.PK == origin.PK))
				? origin
				: null;
		}

		RefUNLOCO Destination
		{
			get
			{
				if (destination == null)
				{
					destination = GetDestinationFromShipments() ?? consol.DischargePort;
				}

				return destination;
			}
		}

		RefUNLOCO destination;

		RefUNLOCO GetDestinationFromShipments()
		{
			var shipments = consol
				.Shipments
				.OfType<ForwardingShipment>()
				.ToArray();

			var destination = shipments
				.FirstOrDefault()?
				.Destination;

			return destination != null && (consol.IsDirect || shipments.Skip(1).All(s => s.Destination != null && s.Destination.PK == destination.PK))
				? destination
				: null;
		}

		#endregion

		#region Release Type

		protected virtual ZString GetReleaseType(CarrierMessageData wrapper)
		{
			if (consol.IsCoLoad
				&& (consol.JK_ConsolMode == Core.Constants.ContainerModes.Groupage || consol.JK_ConsolMode == Core.Constants.ContainerModes.BuyersConsol || (consol.JK_ConsolMode == Core.Constants.ContainerModes.Other && consol.Containers.Any())))
			{
				return ShippingInstructionReleaseTypes.Codes.HouseBill;
			}

			return IsSeaWaybill
				? ShippingInstructionReleaseTypes.Codes.SeaWaybill
				: ShippingInstructionReleaseTypes.Codes.BOLOriginal;
		}

		ShippingInstructionReleaseTypes ReleaseTypesList => releaseTypes ?? (releaseTypes = new ShippingInstructionReleaseTypes());
		ShippingInstructionReleaseTypes releaseTypes;

		ZString GetShipperReference(CarrierMessageData wrapper)
		{
			if (wrapper == null || consol == null)
			{
				return ZString.Empty;
			}

			if (wrapper.IsDirect)
			{
				if (DirectShipment == null)
				{
					return ZString.Empty;
				}

				return DirectShipment.JS_BookingReference.IsEmpty ? DirectShipment.JS_UniqueConsignRef : DirectShipment.JS_BookingReference;
			}

			return consol.JK_AgentsReference;
		}

		#endregion

		#region Number Of Originals and Copies

		protected virtual void PopulateNumberOfOriginalsAndCopies(CarrierMessageData wrapper)
		{
			wrapper.NumberOfOriginals = DirectShipment?.JS_NoOriginalBills ?? consol.JK_NoOriginalBills;
			wrapper.NumberOfCopies = DirectShipment?.JS_NoCopyBills ?? consol.JK_NoCopyBills;
		}

		#endregion

		protected virtual void PopulateBOLDocumentationProvider(CarrierMessageData wrapper)
		{
			wrapper.EBLProvider = new CodeDescription(Lookups.EBLProviderList);
		}

		#region IsSeaWaybill

		protected ZBool IsSeaWaybill
		{
			get
			{
				if (isSeaWaybill == null)
				{
					var directShipmentOrConsolReleaseType = DirectShipment?.JS_ReleaseType ?? consol.JK_ReleaseType;
					isSeaWaybill = directShipmentOrConsolReleaseType == Core.Constants.ShipmentReleaseTypes.SeaWaybill || directShipmentOrConsolReleaseType == Core.Constants.ShipmentReleaseTypes.ExpressBofL || directShipmentOrConsolReleaseType == Core.Constants.ShipmentReleaseTypes.NonNegotiable;
				}

				return isSeaWaybill.Value;
			}
		}
		ZBool? isSeaWaybill;

		#endregion

		#region IsGroupAndConsolidatePackingLines

		protected ZBool IsGroupAndConsolidatePackingLines { get; set; }

		#endregion

		#region DirectShipment

		protected ForwardingShipment DirectShipment
		{
			get
			{
				if (!retrieveDirectShipment)
				{
					directShipment = consol.DirectShipment;
					retrieveDirectShipment = true;
				}

				return directShipment;
			}
		}

		bool retrieveDirectShipment;

		ForwardingShipment directShipment;

		#endregion

		#region Charges

		ZAddressWithContact FreightPayer
		{
			get
			{
				if (!FreightCharges.Any())
				{
					return null;
				}

				return consol.JK_PrepaidCollect == Constants.PaymentType.Prepaid
					? consol.SendingForwarderWithContact
					: consol.ReceivingForwarderWithContact;
			}
		}

		IJobConsolCost[] FreightCharges => freightCharges ?? (freightCharges = LoadConsolCosts());
		IJobConsolCost[] freightCharges;

		IJobConsolCost[] LoadConsolCosts()
		{
			var chargeCodeFilter = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			if (!string.IsNullOrEmpty(Env.Registry.FreightChargeCode.ToString()))
			{
				chargeCodeFilter.AddToFilter(AccChargeCodeSchema.PK, Env.Registry.FreightChargeCode);
			}
			else
			{
				chargeCodeFilter.AddToFilter(AccChargeCodeSchema.AC_Code, ChargeCodeGroupList.Codes.Freight);
			}

			var accChargeCode = consol.Factory.LoadTop1<IAccChargeCode>(chargeCodeFilter);

			if (accChargeCode != null)
			{
				var query = new ZQuery();
				query.AddToFilter(JobConsolCostSchema.E6_GC, GlbCompany.CurrentCompany.PK);
				query.AddToFilter(JobConsolCostSchema.E6_ParentID, consol.PK);
				query.AddToFilter(JobConsolCostSchema.E6_AC_ChargeCode, accChargeCode.PK);

				return consol.Factory.Load<IJobConsolCost>(query);
			}

			return Array.Empty<IJobConsolCost>();
		}

		#endregion

		#region Transports

		protected IReadOnlyCollection<Freight.Business.Transport> Transports => transports ?? (transports = GetTransports());
		IReadOnlyCollection<Freight.Business.Transport> transports;

		IReadOnlyCollection<Freight.Business.Transport> GetTransports()
		{
			consol.Transports.Sort(MovementLegComparer.PortsAndDatesBased(consol?.Transports));
			return consol
				.Transports
				.OfType<Freight.Business.Transport>()
				.ToArray();
		}

		#endregion

		#region Implementation

		CodeDescriptionPairList ContainerModesList
		{
			get
			{
				if (containerModesList == null)
				{
					containerModesList = new CodeDescriptionPairList();
					containerModesList.AddPair(Core.Constants.ContainerModes.FCL, ResString.GetMultilingualString("79802782-bc9c-4b4f-8303-4a87bd93796a", "Full Container Load"));
					containerModesList.AddPair(Core.Constants.ContainerModes.LCL, ResString.GetMultilingualString("ce5c7310-420e-4523-99e7-ae4136293afc", "Less Container Load"));
				}

				return containerModesList;
			}
		}

		CodeDescriptionPairList containerModesList;

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

		ZString GetAcidNumber(CarrierMessageData wrapper)
		{
			if (wrapper == null || consol == null)
			{
				return ZString.Empty;
			}

			var numbers = consol.Numbers
				.Cast<CusEntryNumber>()
				.Where(n => n.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference && n.CE_RN_NKCountryCode == Constants.CountryCodes.Egypt)
				.Select(n => n.CE_EntryNum).Distinct();

			return string.Join("\r\n", numbers);
		}

		protected OrgHeader GetCarrier() => consol.IsCoLoad ? consol.Creditor : consol.ShippingLine;

		#region Lookups

		protected CarrierMessageDataLookups Lookups => lookups ?? (lookups = new CarrierMessageDataLookups(GetCarrier()));
		CarrierMessageDataLookups lookups;

		#endregion

		#region CarrierMessagingRequirements

		protected virtual ZBool IsElectronicBillOfLadingProviderMandatory(CarrierMessageData wrapper) => false;

		protected virtual ZBool IsHarmonizedCodeMandatory() => false;

		protected virtual ZBool GetIsRequiredSendAttachment() => false;

		protected virtual ZBool CarrierHasIntegrationViaEmailToCarrierLocalOfficeTag() => false;

		protected RefShippingLineMessagingRequirement GetMessagingRequirement(ZString messageRequirementType)
		{
			var orgHeader = consol.IsCoLoad ? consol.Creditor : consol.ShippingLine;
			return OrgHeaderExtensions.GetShippingLineMessagingRequirement(orgHeader, messageRequirementType);
		}

		#endregion

		#region EuropeanUnionCustomsMembersProvider

		protected IEuropeanUnionCustomsMembersProvider EuropeanUnionCustomsMembersProvider => europeanUnionCustomsMembersProvider ?? (europeanUnionCustomsMembersProvider = ObjectFactory.Get<IEuropeanUnionCustomsMembersProvider>());

		IEuropeanUnionCustomsMembersProvider europeanUnionCustomsMembersProvider;

		#endregion
	}
}
