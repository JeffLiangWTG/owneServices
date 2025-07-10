using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Edifact.D05B.Elements;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	public abstract class BaseTradeNetMessage<T> : ITradeNetMessage where T : ITradeNetInSection, new()
	{
		protected BaseTradeNetMessage(ISGCUSDEC cusDec)
		{
			CusDec = Argument.NotNull(cusDec, nameof(cusDec));
		}

		public ISGCUSDEC CusDec { get; }

		public void Build(ITradeNetSectionParent sectionParent)
		{
			if (sectionParent is TradenetDeclaration messageParent)
			{
				BuildTradenetDeclaration(messageParent);
			}
		}

		protected abstract void BuildTradenetDeclaration(TradenetDeclaration messageParent);

		#region Implement

		[SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
		public const string EmptyValue = "NA";

		[SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
		public const string DateTimeFormat = "yyyyMMdd";

		[SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
		public const string MessageReferencePrefix = "WTG";

		protected bool SetValueIfNotEmpty(ZString value, Action<ZString> setter)
		{
			var hasValue = false;

			if (!string.IsNullOrWhiteSpace(value))
			{
				setter?.Invoke(value.ToUpperInvariant());
				hasValue = true;
			}

			return hasValue;
		}

		#endregion

		#region Declaration

		public T BuildDeclaration()
		{
			return BuildDeclarationCore();
		}

		protected virtual T BuildDeclarationCore()
		{
			var declaration = new T();
			BuildHeader(declaration);
			BuildTransport(declaration);
			BuildParty(declaration);
			BuildItem(declaration);
			BuildSummary(declaration);
			BuildSupportingDocumentReference(declaration);

			BuildCargo(declaration as ITradeNetInSectionWithCargo);
			BuildLicence(declaration as ITradeNetInSectionWithLicence);
			BuildInvoice(declaration as ITradeNetInSectionWithInvoice);
			BuildCertificate(declaration as ITradeNetInSectionWithCertificate);

			return declaration;
		}

		#region Header

		public void BuildHeader(ITradeNetInSection message)
		{
			message.Header = BuildHeaderCore();
		}

		protected virtual Header BuildHeaderCore()
		{
			var header = new Header();
			BuildCommonHeaderSection(header);
			header.DeclarationType = CusDec.DeclarationType;

			SetValueIfNotEmpty(CusDec.PreviousPermitNumber, (s) => header.PreviousPermitNumber = s);
			SetValueIfNotEmpty(CusDec.BGIndicator, (s) => header.BankerGuaranteeCode = s);

			var tradersRemarks = CusDec.TradersRemarksForMessage;
			if (tradersRemarks != null && tradersRemarks.Any())
			{
				BuildRemarks(header, tradersRemarks);
			}

			BuildCustomsProcedureCodeInformation(header);

			return header;
		}

		protected void BuildCommonHeaderSection(ITradeNetHeaderSection headerSection)
		{
			headerSection.MessageReference = new ZString(MessageReferencePrefix + CusDec.JobNumber).Right(14);
			headerSection.UniqueReferenceNumber = BuildUniqueReferenceNumber();
			headerSection.DeclarantID = SGXmlEDIMessage.SendersReferencePlaceHolderXml;
			headerSection.CommonAccessReference = MessageType;
			headerSection.DeclarationIndicator = true;
			headerSection.DeclarationIndicatorSpecified = true;
			headerSection.AdditionalRecipientID = BuildAdditionalRecipients();
		}

		#region UniqueReferenceNumber

		protected UniqueReferenceNumber BuildUniqueReferenceNumber()
		{
			return BuildUniqueReferenceNumberCore();
		}

		protected virtual UniqueReferenceNumber BuildUniqueReferenceNumberCore()
		{
			return new UniqueReferenceNumber
			{
				ID = SGXmlEDIMessage.MessageNumberPlaceHolderXml,
				Date = SGXmlEDIMessage.MessageDateTimeCreatePlaceHolderXml,
				SequenceNumeric = SGXmlEDIMessage.UniqueBatchNumberPlaceHolderXml
			};
		}

		#endregion

		#region Remarks

		protected void BuildRemarks(Header header, IEnumerable<ZString> tradersRemarks)
		{
			var remarks = BuildRemarksCore(tradersRemarks);
			header.Remarks = remarks;
		}

		protected virtual string[] BuildRemarksCore(IEnumerable<ZString> tradersRemarks)
		{
			return (tradersRemarks.Take(2)).Where(remark => !remark.IsEmpty).Select(remark => remark.ToString().ToUpperInvariant()).ToArray();
		}

		#endregion

		#region CustomsProcedureCodeInformation

		protected void BuildCustomsProcedureCodeInformation(Header header)
		{
			var list = BuildCustomsProcedureCodeInformationCore();
			header.CustomsProcedureCodeInformation = list.Any() ? list.ToArray() : null;
		}

		protected virtual List<CustomsProcedureCodeInformation> BuildCustomsProcedureCodeInformationCore()
		{
			var informations = new List<CustomsProcedureCodeInformation>();
			var cpcs = CusDec.CPCs?.Take(5) ?? Array.Empty<ICusCPC>();
			foreach (var cpc in cpcs)
			{
				var information = new CustomsProcedureCodeInformation
				{
					CustomsProcedureCode = cpc.APCCodeName
				};

				var processingCodes = new List<CustomsProcedureCodeInformationCPCProcessingCode>();
				var pcoccurrences = cpc.PCOccurrences?.Where(c => c != null)?.Take(5) ?? Array.Empty<ICusProcessingCodes>();
				foreach (var pcs in pcoccurrences)
				{
					var cpcProcessingCode = new CustomsProcedureCodeInformationCPCProcessingCode();
					cpcProcessingCode.ProcessingCodeOne = pcs.ProcessingCode1;
					if (!pcs.ProcessingCode2.IsEmpty)
					{
						cpcProcessingCode.ProcessingCodeTwo = pcs.ProcessingCode2;
					}

					if (!pcs.ProcessingCode3.IsEmpty)
					{
						cpcProcessingCode.ProcessingCodeThree = pcs.ProcessingCode3;
					}

					processingCodes.Add(cpcProcessingCode);
				}

				information.CPCProcessingCode = processingCodes.Any() ? processingCodes.ToArray() : null;
				informations.Add(information);
			}

			return informations;
		}

		#endregion

		#region AdditionalRecipients

		protected string[] BuildAdditionalRecipients()
		{
			var additionalRecipients = CusDec.AdditionalRecipients?.Take(3).Select(c => c.Left(17).ToUpperInvariant().ToString()).ToArray();
			return additionalRecipients != null && additionalRecipients.Any() ? additionalRecipients : null;
		}

		#endregion

		#endregion

		#region Cargo

		public void BuildCargo(ITradeNetInSectionWithCargo message)
		{
			if (message != null)
			{
				message.Cargo = BuildCargoCore();
			}
		}

		protected virtual Cargo BuildCargoCore()
		{
			var cargo = new Cargo
			{
				CargoPackingType = CusDec.CargoPackingType
			};

			BuildSupplyIndicator(cargo);
			BuildReleaseLocation(cargo);
			BuildReceiptLocation(cargo);
			BuildTransportEquipment(cargo);

			return cargo;
		}

		protected virtual void BuildSupplyIndicator(Cargo cargo)
		{
			var isSupplyIndicatorApplicable = !string.IsNullOrWhiteSpace(CusDec.SupplyIndicator);
			cargo.SupplyIndicator = CusDec.SupplyIndicator == SupplyIndicatorCodeList.Codes.Y;
			cargo.SupplyIndicatorSpecified = isSupplyIndicatorApplicable;
		}

		#region ReleaseLocation

		protected void BuildReleaseLocation(Cargo cargo)
		{
			cargo.ReleaseLocation = BuildReleaseLocationCore();
		}

		protected virtual ReleaseLocation BuildReleaseLocationCore()
		{
			var releaseLocation = new ReleaseLocation();

			var place = CusDec.PlaceOfRelease;

			if (place != null)
			{
				releaseLocation.LocationCode = place.IsNonSystemNonLicenced ? place.Type : place.Code;
				SetValueIfNotEmpty(place.AddressRequired ? place.NameAndAddress : ZString.Empty, (c) => releaseLocation.LocationName = c);
			}
			else
			{
				releaseLocation.LocationCode = string.Empty;
			}

			return releaseLocation;
		}

		#endregion

		#region ReceiptLocation

		protected void BuildReceiptLocation(Cargo cargo)
		{
			cargo.ReceiptLocation = BuildReceiptLocationCore();
		}

		protected virtual ReceiptLocation BuildReceiptLocationCore()
		{
			var receiptLocation = new ReceiptLocation();
			var place = CusDec.PlaceOfReceipt;

			if (place != null)
			{
				receiptLocation.LocationCode = place.IsNonSystemNonLicenced ? place.Type : place.Code;
				SetValueIfNotEmpty(place.AddressRequired ? place.NameAndAddress : ZString.Empty, (c) => receiptLocation.LocationName = c);
			}
			else
			{
				receiptLocation.LocationCode = string.Empty;
			}

			return receiptLocation;
		}

		#endregion

		#region StorageLocation

		protected void BuildStorageLocation(Cargo cargo)
		{
			cargo.StorageLocation = BuildStorageLocationCore(CusDec.PlaceOfStorage);
		}

		protected virtual StorageLocation BuildStorageLocationCore(ZString placeOfStorage)
		{
			StorageLocation storageLocation = null;

			if (!string.IsNullOrWhiteSpace(placeOfStorage))
			{
				storageLocation = new StorageLocation() { LocationCode = placeOfStorage };
			}

			return storageLocation;
		}

		#endregion

		#region TransportEquipment

		protected void BuildTransportEquipment(Cargo cargo)
		{
			var list = BuildTransportEquipmentCore();
			cargo.TransportEquipment = list.Any() ? list.ToArray() : null;
		}

		protected virtual List<TransportEquipment> BuildTransportEquipmentCore()
		{
			var transportEquipments = new List<TransportEquipment>();

			if (CusDec.Containers != null)
			{
				var containerSequence = CusDec.GetPreviousMessageContainerSequence();
				var i = containerSequence.HighestSequenceNumber;

				foreach (var container in CusDec.Containers.Take(99))
				{
					var weightAndUnit = ConvertToSingaporeCustomsRequiredWeightUnit(container.ContainerWeight, container.ContainerWeightUnit, SGConstants.Weight.Tonnes);

					var numberSequence = containerSequence.GetSequenceNumber(container.ContainerNumber);
					if (numberSequence <= 0)
					{
						numberSequence = ++i;
					}

					transportEquipments.Add(new TransportEquipment
					{
						SequenceNumeric = numberSequence.ToString(CultureInfo.InvariantCulture),
						EquipmentID = container.ContainerNumber,
						SizeTypeCode = new ZString(container.ContainerType + container.ContainerSize.ToString()).SubstringSafe(0, 5),
						EquipmentWeightMeasureNumeric = ConvertContainerWeight(weightAndUnit.Item1),
						EquipmentWeightMeasureNumericSpecified = weightAndUnit.Item1 > 0,

						TransportEquipmentSeal = BuildTransportEquipmentSeal(container)
					});
				}
			}

			return transportEquipments;
		}

		public virtual TransportEquipmentTransportEquipmentSeal BuildTransportEquipmentSeal(ICusContainer container)
		{
			var result = new TransportEquipmentTransportEquipmentSeal
			{
				SealID = container.SealNumber.IsEmpty ? (ZString)EmptyValue : container.SealNumber
			};

			return result;
		}

		#region SG Weight Conversion

		/// <summary>
		/// SG Customs requires declarations to be submitted in only 2 weight measurements:
		///     TNE when transport by sea
		///     KGM when transport by air
		/// If the sourceWeightUnit is not valid, sourceWeight will be returned without conversion.
		/// </summary>
		public (decimal, string) ConvertToSingaporeCustomsRequiredWeightUnit(decimal sourceWeight, string sourceWeightUnit, string targetWeightUnit)
		{
			var resultWeight = sourceWeight;
			var resultWeightUnit = sourceWeightUnit;

			if (Core.Constants.Weight.ContainsCode(sourceWeightUnit))
			{
				if (targetWeightUnit == Core.Constants.Weight.Kilograms || targetWeightUnit == SGConstants.Weight.Kilograms)
				{
					resultWeight = Core.Constants.Weight.Convert(sourceWeight, sourceWeightUnit, Core.Constants.Weight.Kilograms);
					resultWeightUnit = targetWeightUnit;
				}
				else if (targetWeightUnit == Core.Constants.Weight.Tonnes || targetWeightUnit == SGConstants.Weight.Tonnes)
				{
					resultWeight = Core.Constants.Weight.Convert(sourceWeight, sourceWeightUnit, Core.Constants.Weight.Tonnes);
					resultWeightUnit = targetWeightUnit;
				}
			}

			return (resultWeight, resultWeightUnit);
		}

		protected decimal ConvertContainerWeight(decimal containerWeightInTonnes)
		{
			var roundedContainerWeightInTonnes = Utilities.Round(containerWeightInTonnes, 0);

			if (roundedContainerWeightInTonnes > 999)
			{
				roundedContainerWeightInTonnes = 999;
			}
			else if (roundedContainerWeightInTonnes < 1)
			{
				roundedContainerWeightInTonnes = 1;
			}

			return roundedContainerWeightInTonnes;
		}

		#endregion

		#endregion

		#region BlanketStartDate

		protected void BuildBlanketStartDate(Cargo cargo)
		{
			if (CusDec.DeclarationType == DeclarationTypeCodeList.Codes.BKT && !CusDec.StartDateOfBlanket.IsEmpty)
			{
				cargo.BlanketStartDate = CusDec.StartDateOfBlanket.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
			}
		}

		#endregion

		#region ExhibitionTemporaryImportPeriod

		protected void BuildExhibitionTemporaryImportPeriod(Cargo cargo)
		{
			cargo.ExhibitionTemporaryImportPeriod = BuildExhibitionTemporaryImportPeriodCore();
		}

		protected virtual ExhibitionTemporaryImportPeriod BuildExhibitionTemporaryImportPeriodCore()
		{
			ExhibitionTemporaryImportPeriod result = null;

			if (CusDec.DeclarationType == DeclarationTypeCodeList.Codes.BKT && !CusDec.StartDateOfBlanket.IsEmpty)
			{
				result = new ExhibitionTemporaryImportPeriod
				{
					StartDate = CusDec.StartDateOfBlanket.ToString(DateTimeFormat, CultureInfo.InvariantCulture)
				};
			}

			return result;
		}

		#endregion

		#endregion

		#region Transport

		public void BuildTransport(ITradeNetInSection message)
		{
			if (CusDec.HasInwardTransport || CusDec.HasOutwardTransport)
			{
				message.Transport = BuildTransportCore();
			}
		}

		protected Transport BuildTransportCore()
		{
			Transport transport = null;
			var inwardTransport = BuildInwardTransport();
			var outwardTransport = BuildOutwardTransport();
			if (inwardTransport != null || outwardTransport != null)
			{
				transport = new Transport()
				{
					InwardTransport = inwardTransport,
					OutwardTransport = outwardTransport
				};
			}

			return transport;
		}

		#region InwardTransport

		protected InwardTransport BuildInwardTransport()
		{
			InwardTransport inwardTransport = null;
			if (CusDec.HasInwardTransport)
			{
				inwardTransport = BuildInwardTransportCore();
				if (inwardTransport != null)
				{
					BuildInwardTransportTransportMeans(inwardTransport);
				}
			}

			return inwardTransport;
		}

		protected virtual InwardTransport BuildInwardTransportCore()
		{
			var inwardTransport = new InwardTransport();
			if (!CusDec.ArrivalDate.IsEmpty)
			{
				inwardTransport.ArrivalDate = CusDec.ArrivalDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
			}

			inwardTransport.LoadingPort = CusDec.PortOfLoading;

			return inwardTransport;
		}

		#region InwardTransportTransportMeans

		protected void BuildInwardTransportTransportMeans(InwardTransport inwardTransport)
		{
			inwardTransport.TransportMeans = BuildInwardTransportTransportMeansCore();
		}

		protected virtual InwardTransportTransportMeans BuildInwardTransportTransportMeansCore()
		{
			InwardTransportTransportMeans transportMeans = null;
			var modeCode = CusDec.InwardTransportCode;

			if (modeCode > 0)
			{
				transportMeans = new InwardTransportTransportMeans();

				var mode = transportMeans.TransportMode = new InwardTransportTransportMeansTransportMode();
				mode.ModeCode = modeCode;
				mode.ModeCodeSpecified = true;

				if (modeCode == SGConstants.TransportCodes.Sea || modeCode == SGConstants.TransportCodes.Air)
				{
					SetValueIfNotEmpty(CusDec.InwardMasterBill, (s) => transportMeans.MAWBOUCROBLNumber = s);
				}

				SetValueIfNotEmpty(CusDec.InwardJourneyIdentifier, (s) => mode.ConveyanceReferenceNumber = s);
				SetValueIfNotEmpty(CusDec.InwardTransportIdentifier, (s) => mode.TransportIdentifier = s);
			}

			return transportMeans;
		}

		#endregion

		#endregion

		#region OutwardTransport

		protected OutwardTransport BuildOutwardTransport()
		{
			OutwardTransport outwardTransport = null;

			if (CusDec.HasOutwardTransport)
			{
				outwardTransport = BuildOutwardTransportCore();
			}

			return outwardTransport;
		}

		protected virtual OutwardTransport BuildOutwardTransportCore()
		{
			var outwardTransport = new OutwardTransport();

			var departureDate = CusDec.DepartureDate;
			if (!departureDate.IsEmpty)
			{
				outwardTransport.DepartureDate = departureDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
			}

			outwardTransport.DischargePort = CusDec.PortOfDischarge;

			BuildOutwardTransportTransportMeans(outwardTransport);
			BuildOutwardTransportAdditionalVesselInformation(outwardTransport);

			return outwardTransport;
		}

		#region OutwardTransportTransportMeans

		protected void BuildOutwardTransportTransportMeans(OutwardTransport outwardTransport)
		{
			outwardTransport.TransportMeans = BuildOutwardTransportTransportMeansCore();
		}

		protected virtual OutwardTransportTransportMeans BuildOutwardTransportTransportMeansCore()
		{
			OutwardTransportTransportMeans transportMeans = null;

			var modeCode = CusDec.OutwardTransportCode;

			if (modeCode > 0)
			{
				transportMeans = new OutwardTransportTransportMeans();

				var mode = transportMeans.TransportMode = new OutwardTransportTransportMeansTransportMode();
				mode.ModeCode = modeCode;
				mode.ModeCodeSpecified = true;

				if (modeCode == SGConstants.TransportCodes.Sea || modeCode == SGConstants.TransportCodes.Air)
				{
					SetValueIfNotEmpty(CusDec.OutwardMasterBill, (s) => transportMeans.MAWBOUCROBLNumber = s);
				}

				SetValueIfNotEmpty(CusDec.OutwardJourneyIdentifier, (s) => mode.ConveyanceReferenceNumber = s);
				SetValueIfNotEmpty(CusDec.OutwardTransportIdentifier, (s) => mode.TransportIdentifier = s);
			}

			return transportMeans;
		}

		#endregion

		#region OutwardTransportAdditionalVesselInformation

		protected void BuildOutwardTransportAdditionalVesselInformation(OutwardTransport outwardTransport)
		{
			outwardTransport.AdditionalVesselInformation = BuildOutwardTransportAdditionalVesselInformationCore();
		}

		protected virtual OutwardTransportAdditionalVesselInformation BuildOutwardTransportAdditionalVesselInformationCore()
		{
			var hasData = false;
			var additionalVesselInformation = new OutwardTransportAdditionalVesselInformation();
			hasData |= SetValueIfNotEmpty(CusDec.OutwardVesselType, (c) => additionalVesselInformation.VesselType = c);
			hasData |= SetValueIfNotEmpty(CusDec.OutwardVesselNationality, (c) => additionalVesselInformation.VesselNationality = c);
			if (CusDec.OutwardTransportCode == SGConstants.TransportCodes.Sea)
			{
				var towingVessel = BuildTowingVesselCore();
				additionalVesselInformation.TowingVessel = towingVessel;
				hasData |= towingVessel != null;
				if (ZDecimal.TryParse(Utilities.FormatNumber(CusDec.OutwardVesselNRT.ToString(), SGConstants.NumericFormatting.TradeNet4Point1.DecimalPlacesForNetRegisteredTonnage, CultureInfo.InvariantCulture), out var outwardVesselNRT)
					&& outwardVesselNRT > 0)
				{
					additionalVesselInformation.NetRegisterTonnage = outwardVesselNRT;
					additionalVesselInformation.NetRegisterTonnageSpecified = true;
					hasData = true;
				}
			}

			return hasData ? additionalVesselInformation : null;
		}

		protected virtual OutwardTransportAdditionalVesselInformationTowingVessel BuildTowingVesselCore()
		{
			var towingVessel = new OutwardTransportAdditionalVesselInformationTowingVessel();

			return SetValueIfNotEmpty(CusDec.TowingVesselName, (s) =>
			{
				towingVessel.VesselID = CusDec.TowingVesselVoyageNo.IsEmpty ? (ZString)EmptyValue : CusDec.TowingVesselVoyageNo;
				towingVessel.VesselName = s;
			}) ? towingVessel : null;
		}

		protected virtual bool BuildLoadingNextPortCore(OutwardTransportAdditionalVesselInformation additionalVesselInformation)
		{
			return SetValueIfNotEmpty(CusDec.NextPortOfCall, (c) => additionalVesselInformation.LoadingNextPort = c);
		}

		protected virtual bool BuildLoadingFinalPortCore(OutwardTransportAdditionalVesselInformation additionalVesselInformation)
		{
			return SetValueIfNotEmpty(CusDec.FinalPortOfCall, (c) => additionalVesselInformation.LoadingFinalPort = c);
		}

		#endregion

		#endregion

		#endregion

		#region Party

		public void BuildParty(ITradeNetInSection message)
		{
			var party = message.Party = new Party();

			var declarant = CusDec.Declarant;
			if (declarant != null)
			{
				BuildDeclarantParty(party, declarant);
				BuildDeclaringAgentParty(party, declarant);
			}

			var freightForwarder = CusDec.FreightForwarder;
			if (freightForwarder != null)
			{
				BuildFreightForwarderParty(party, freightForwarder);
			}

			var inwardCarrierAgent = CusDec.InwardCarrierAgent;
			if (inwardCarrierAgent != null)
			{
				BuildInwardCarrierAgentParty(party, inwardCarrierAgent);
			}

			var importer = CusDec.Importer;
			if (importer != null)
			{
				BuildImporterParty(party, importer);
			}

			var outwardCarrierAgent = CusDec.OutwardCarrierAgent;
			if (outwardCarrierAgent != null)
			{
				BuildOutwardCarrierAgentParty(party, outwardCarrierAgent);
			}

			var exporter = CusDec.Exporter;
			if (exporter != null)
			{
				BuildExporterParty(party, exporter);
			}

			var consignee = CusDec.Consignee;
			if (consignee != null)
			{
				BuildConsigneeParty(party, consignee);
			}

			var endUser = SupportsEndUserParty ? CusDec.EndUser : null;
			if (endUser != null)
			{
				BuildEndUserParty(party, endUser);
			}

			var handlingAgent = SupportsHandlingAgentParty ? CusDec.HandlingAgent : null;
			if (handlingAgent != null)
			{
				BuildHandlingAgentParty(party, handlingAgent);
			}

			var manufacturer = CusDec.Manufacturer;
			if (manufacturer != null)
			{
				BuildManufacturerParty(party, manufacturer);
			}

			var claimant = CusDec.Claimant;
			if (claimant != null)
			{
				BuildClaimantParty(party, claimant);
			}
		}

		#region DeclarantParty

		protected void BuildDeclarantParty(Party party, ICusAgentInfo declarant)
		{
			var declarantParty = party.DeclarantParty = BuildDeclarantPartyCore(declarant);
			BuildDeclarantPartyPersonInformation(declarantParty, declarant);
		}

		protected virtual DeclarantParty BuildDeclarantPartyCore(ICusAgentInfo declarant)
		{
			return new DeclarantParty
			{
				Telephone = declarant.Phone
			};
		}

		#region DeclarantPartyPersonInformation

		protected void BuildDeclarantPartyPersonInformation(DeclarantParty declarantParty, ICusAgentInfo declarant)
		{
			declarantParty.PersonInformation = BuildDeclarantPartyPersonInformationCore(declarant);
		}

		protected virtual DeclarantPartyPersonInformation BuildDeclarantPartyPersonInformationCore(ICusAgentInfo declarant)
		{
			var declarantId = declarant.Code.IsEmpty ? declarant.Passport : declarant.Code;

			return new DeclarantPartyPersonInformation
			{
				CodeValue = declarantId.Replace("@", " ").SubstringSafe(0, 17).ToUpperInvariant(),
				Name = declarant.Name.Replace("'", " ").SubstringSafe(0, 100).ToUpperInvariant()
			};
		}

		#endregion

		#endregion

		#region DeclaringAgentParty

		protected void BuildDeclaringAgentParty(Party party, ICusAgentInfo declarant)
		{
			party.DeclaringAgentParty = BuildDeclaringAgentPartyCore(declarant);
		}

		protected virtual DeclaringAgentParty BuildDeclaringAgentPartyCore(ICusAgentInfo declarant)
		{
			return new DeclaringAgentParty
			{
				PartyIdentification = new DeclaringAgentPartyPartyIdentification { ID = GlbCompany.CurrentCompany.GC_CustomsRegistrationNo },
				PartyName = GlbCompany.CurrentCompany.GC_Name.SplitIntoArray(50, 2)
			};
		}

		#endregion

		#region FreightForwarderParty

		protected void BuildFreightForwarderParty(Party party, IOrganisation freightForwarder)
		{
			party.FreightForwarderParty = BuildFreightForwarderPartyCore(freightForwarder);
		}

		protected virtual FreightForwarderParty BuildFreightForwarderPartyCore(IOrganisation freightForwarder)
		{
			return new FreightForwarderParty
			{
				PartyIdentification = new FreightForwarderPartyPartyIdentification { ID = freightForwarder.UEN.ToUpperInvariant() },
				PartyName = freightForwarder.Name.SplitIntoArray(50, 2)
			};
		}

		#endregion

		#region InwardCarrierAgentParty

		protected void BuildInwardCarrierAgentParty(Party party, IOrganisation inwardCarrierAgent)
		{
			party.InwardCarrierAgentParty = BuildInwardCarrierAgentPartyCore(inwardCarrierAgent);
		}

		protected virtual InwardCarrierAgentParty BuildInwardCarrierAgentPartyCore(IOrganisation inwardCarrierAgent)
		{
			return new InwardCarrierAgentParty
			{
				PartyIdentification = new InwardCarrierAgentPartyPartyIdentification { ID = inwardCarrierAgent.UEN.ToUpperInvariant() },
				PartyName = inwardCarrierAgent.Name.SplitIntoArray(50, 2)
			};
		}

		#endregion

		#region ImporterParty

		protected void BuildImporterParty(Party party, IOrganisation importer)
		{
			party.ImporterParty = BuildImporterPartyCore(importer);
		}

		protected virtual ImporterParty BuildImporterPartyCore(IOrganisation importer)
		{
			return new ImporterParty
			{
				PartyIdentification = new ImporterPartyPartyIdentification { ID = importer.UEN.ToUpperInvariant() },
				PartyName = importer.Name.SplitIntoArray(35, 2)
			};
		}

		#endregion

		#region OutwardCarrierAgentParty

		protected void BuildOutwardCarrierAgentParty(Party party, IOrganisation outwardCarrierAgent)
		{
			party.OutwardCarrierAgentParty = BuildOutwardCarrierAgentPartyCore(outwardCarrierAgent);
		}

		protected virtual OutwardCarrierAgentParty BuildOutwardCarrierAgentPartyCore(IOrganisation outwardCarrierAgent)
		{
			return new OutwardCarrierAgentParty
			{
				PartyIdentification = new OutwardCarrierAgentPartyPartyIdentification { ID = outwardCarrierAgent.UEN.ToUpperInvariant() },
				PartyName = outwardCarrierAgent.Name.SplitIntoArray(50, 2)
			};
		}

		#endregion

		#region ExporterParty

		protected void BuildExporterParty(Party party, IOrganisation exporter)
		{
			party.ExporterParty = BuildExporterPartyCore(exporter);
		}

		protected virtual ExporterParty BuildExporterPartyCore(IOrganisation exporter)
		{
			var exporterParty = new ExporterParty
			{
				PartyDetail = new ExporterPartyPartyDetail
				{
					PartyIdentification = new ExporterPartyPartyDetailPartyIdentification { ID = exporter.UEN.ToUpperInvariant() },
					PartyName = exporter.Name.SplitIntoArray(35, 2)
				},
			};

			var address = exporter.Address;
			if (address != null)
			{
				BuildExporterPartyAddress(exporterParty, address);
			}

			return exporterParty;
		}

		#region ExporterPartyAddress

		protected void BuildExporterPartyAddress(ExporterParty exportParty, IAddress address)
		{
			exportParty.Address = BuildExporterPartyAddressCore(address);
		}

		protected virtual ExporterPartyAddress BuildExporterPartyAddressCore(IAddress address) => null;

		#endregion

		#endregion

		#region ManufacturerParty

		protected void BuildManufacturerParty(Party party, IOrganisation manufacturer)
		{
			party.ManufacturerParty = BuildManufacturerPartyCore(manufacturer);
		}

		protected virtual ManufacturerParty BuildManufacturerPartyCore(IOrganisation manufacturer)
		{
			var manufacturerParty = new ManufacturerParty
			{
				PartyDetail = new ManufacturerPartyPartyDetail
				{
					PartyIdentification = new ManufacturerPartyPartyDetailPartyIdentification { ID = manufacturer.UEN.ToUpperInvariant() },
					PartyName = manufacturer.Name.SplitIntoArray(35, 2)
				},
			};

			var address = manufacturer.Address;
			if (address != null)
			{
				BuildManufacturerPartyAddress(manufacturerParty, address);
			}

			return manufacturerParty;
		}

		#region ManufacturerPartyAddress

		protected void BuildManufacturerPartyAddress(ManufacturerParty exportParty, IAddress address)
		{
			exportParty.Address = BuildManufacturerPartyAddressCore(address);
		}

		protected virtual ManufacturerPartyAddress BuildManufacturerPartyAddressCore(IAddress address)
		{
			var addressLine = address.FullAddress;
			if (addressLine.Length > 70)
			{
				addressLine = address.FullAddressWithoutAdditionalInfo;
			}

			var manufacturerAddress = new ManufacturerPartyAddress
			{
				CountryCode = address.CountryCode,
				AddressLine = addressLine.SplitIntoArray(35, 2)
			};

			var postCode = address.PostCode;
			postCode = postCode.Length < 10 ? postCode : postCode.KeepAlphanumericCharacters().SubstringSafe(0, 9);

			SetValueIfNotEmpty(postCode, (s) => manufacturerAddress.PostalZone = s);
			SetValueIfNotEmpty(address.City, (s) => manufacturerAddress.CityName = s);
			SetValueIfNotEmpty(address.SubdivisionName, (s) => manufacturerAddress.CountrySubentity = s);
			SetValueIfNotEmpty(address.SubdivisionCode, (s) => manufacturerAddress.CountrySubentityCode = s);

			return manufacturerAddress;
		}

		#endregion

		#endregion

		#region ConsigneeParty

		protected void BuildConsigneeParty(Party party, IOrganisation consignee)
		{
			party.ConsigneeParty = BuildConsigneePartyCore(consignee);
		}

		protected virtual ConsigneeParty BuildConsigneePartyCore(IOrganisation consignee)
		{
			var consigneeParty = new ConsigneeParty
			{
				PartyName = consignee.Name.SplitIntoArray(35, 2)
			};

			var address = consignee.Address;
			if (address != null)
			{
				BuildConsigneePartyAddress(consigneeParty, address);
			}

			return consigneeParty;
		}

		#region ConsigneePartyAddress

		protected void BuildConsigneePartyAddress(ConsigneeParty consigneeParty, IAddress address)
		{
			consigneeParty.Address = BuildConsigneePartyAddressCore(address);
		}

		protected virtual ConsigneePartyAddress BuildConsigneePartyAddressCore(IAddress address)
		{
			var addressLine = address.FullAddress;
			if (addressLine.Length > 70)
			{
				addressLine = address.FullAddressWithoutAdditionalInfo;
			}

			var consigneeAddress = new ConsigneePartyAddress
			{
				CountryCode = address.CountryCode,
				AddressLine = addressLine.SplitIntoArray(35, 2)
			};

			var postCode = address.PostCode;
			postCode = postCode.Length < 10 ? postCode : postCode.KeepAlphanumericCharacters().SubstringSafe(0, 9);

			SetValueIfNotEmpty(postCode, (s) => consigneeAddress.PostalZone = s);
			SetValueIfNotEmpty(address.City, (s) => consigneeAddress.CityName = s);
			SetValueIfNotEmpty(address.SubdivisionName, (s) => consigneeAddress.CountrySubentity = s);
			SetValueIfNotEmpty(address.SubdivisionCode, (s) => consigneeAddress.CountrySubentityCode = s);

			return consigneeAddress;
		}

		#endregion

		#endregion

		#region EndUserParty

		protected void BuildEndUserParty(Party party, IOrganisation endUser)
		{
			party.EndUserParty = BuildEndUserPartyCore(endUser);
		}

		protected virtual EndUserParty BuildEndUserPartyCore(IOrganisation endUser)
		{
			var endUserParty = new EndUserParty
			{
				PartyName = endUser.Name.SplitIntoArray(35, 2)
			};

			var address = endUser.Address;
			if (address != null)
			{
				BuildEndUserPartyAddress(endUserParty, address);
			}

			return endUserParty;
		}

		#region EndUserPartyAddress

		protected void BuildEndUserPartyAddress(EndUserParty endUserParty, IAddress address)
		{
			endUserParty.Address = BuildEndUserPartyAddressCore(address);
		}

		protected virtual EndUserPartyAddress BuildEndUserPartyAddressCore(IAddress address)
		{
			var addressLine = address.FullAddress;
			if (addressLine.Length > 70)
			{
				addressLine = address.FullAddressWithoutAdditionalInfo;
			}

			var endUserAddress = new EndUserPartyAddress
			{
				CountryCode = address.CountryCode,
				AddressLine = addressLine.SplitIntoArray(35, 2)
			};

			var postCode = address.PostCode;
			postCode = postCode.Length < 10 ? postCode : postCode.KeepAlphanumericCharacters().SubstringSafe(0, 9);

			SetValueIfNotEmpty(postCode, (s) => endUserAddress.PostalZone = s);
			SetValueIfNotEmpty(address.City, (s) => endUserAddress.CityName = s);
			SetValueIfNotEmpty(address.SubdivisionName, (s) => endUserAddress.CountrySubentity = s);
			SetValueIfNotEmpty(address.SubdivisionCode, (s) => endUserAddress.CountrySubentityCode = s);

			return endUserAddress;
		}

		#endregion

		#endregion

		#region HandlingAgentParty

		protected void BuildHandlingAgentParty(Party party, IOrganisation handlingAgent)
		{
			party.HandlingAgentParty = BuildHandlingAgentPartyCore(handlingAgent);
		}

		protected virtual HandlingAgentParty BuildHandlingAgentPartyCore(IOrganisation handlingAgent)
		{
			return new HandlingAgentParty
			{
				PartyIdentification = new HandlingAgentPartyPartyIdentification { ID = handlingAgent.UEN.ToUpperInvariant() },
				PartyName = handlingAgent.Name.SplitIntoArray(50, 2)
			};
		}

		#endregion

		#region ClaimantParty

		protected void BuildClaimantParty(Party party, IOrganisation claimant)
		{
			party.ClaimantParty = BuildClaimantPartyCore(claimant);
		}

		protected virtual ClaimantParty BuildClaimantPartyCore(IOrganisation claimant)
		{
			return new ClaimantParty
			{
				PartyDetail = new ClaimantPartyPartyDetail
				{
					PartyIdentification = new ClaimantPartyPartyDetailPartyIdentification { ID = claimant.UEN.ToUpperInvariant() },
					PartyName = claimant.Name.SplitIntoArray(50, 2)
				},

				ClaimantInformation = new ClaimantPartyClaimantInformation
				{
					CodeValue = CusDec.ClaimantCode.SubstringSafe(0, 17).ToUpperInvariant(),
					Name = CusDec.ClaimantName.SubstringSafe(0, 100).ToUpperInvariant()
				}
			};
		}

		#endregion

		protected virtual bool SupportsEndUserParty => false;

		protected virtual bool SupportsHandlingAgentParty => false;

		#endregion

		#region Licence

		public void BuildLicence(ITradeNetInSectionWithLicence declaration)
		{
			if (declaration != null)
			{
				var list = BuildLicenceCore();
				declaration.Licence = list.Any() ? list.ToArray() : null;
			}
		}

		protected virtual List<Licence> BuildLicenceCore()
		{
			var licences = new List<Licence>();
			var licencesAndDocuments = CusDec.LicencesAndDocuments;

			if (licencesAndDocuments != null)
			{
				foreach (var licenceAndDocument in licencesAndDocuments.Take(5))
				{
					licences.Add(new Licence
					{
						ReferenceID = licenceAndDocument.LicenceNumber.Left(35).ToUpperInvariant()
					});
				}
			}

			return licences;
		}

		#endregion

		#region SupportingDocumentReference

		public void BuildSupportingDocumentReference(ITradeNetInSection message)
		{
			var list = BuildSupportingDocumentReferenceCore();
			message.SupportingDocumentReference = list.Any() ? list.ToArray() : null;
		}

		protected virtual List<SupportingDocumentReference> BuildSupportingDocumentReferenceCore()
		{
			var references = new List<SupportingDocumentReference>();
			var documents = CusDec.AdditionalMessageInformation?.SupportingDocuments;

			if (documents != null)
			{
				foreach (var attachment in documents.Take(10))
				{
					references.Add(new SupportingDocumentReference
					{
						DocumentID = attachment.DocType.ToUpperInvariant(),
						Filename = attachment.FileName.ToASCII().ToUpperInvariant()
					});
				}
			}

			return references;
		}

		#endregion

		#region Certificate

		public void BuildCertificate(ITradeNetInSectionWithCertificate message)
		{
			if (message != null)
			{
				message.Certificate = BuildCertificateCore();
			}
		}

		protected virtual Certificate BuildCertificateCore()
		{
			return null;
		}

		protected virtual CertificateDetail[] BuildCertificateDetailCore()
		{
			return null;
		}

		#endregion

		#region Invoice

		public void BuildInvoice(ITradeNetInSectionWithInvoice declaration)
		{
			if (declaration != null)
			{
				var invoices = new List<Invoice>();

				foreach (var cusInvoice in CusDec.Invoices.Where(c => !c.InvoicePK.IsEmpty).Take(20))
				{
					var invoice = BuildInvoiceCore(cusInvoice);

					var supplier = cusInvoice.Supplier;
					if (supplier != null)
					{
						BuildInvoiceSupplierManufacturerParty(invoice, supplier);
					}

					BuildInvoiceTotalInvoiceValue(invoice, cusInvoice);

					var freightCharge = cusInvoice.FreightCharge;
					if (freightCharge != null)
					{
						BuildInvoiceFreightCharge(invoice, freightCharge);
					}

					var insuranceCharge = cusInvoice.InsuranceCharge;
					if (insuranceCharge != null)
					{
						BuildInvoiceInsuranceCharge(invoice, insuranceCharge);
					}

					var otherCharge = cusInvoice.OtherCharge;
					if (otherCharge != null)
					{
						BuildInvoiceOtherTaxableCharge(invoice, otherCharge);
					}

					invoices.Add(invoice);
				}

				declaration.Invoice = invoices.Any() ? invoices.ToArray() : null;
			}
		}

		protected virtual Invoice BuildInvoiceCore(ICusInvoice cusInvoice)
		{
			var invoice = new Invoice();

			if (SupportsInvoiceNumberSegment)
			{
				SetValueIfNotEmpty(cusInvoice.InvoiceNumber, (s) => invoice.InvoiceNumber = s);
			}

			var invoiceDate = cusInvoice.InvoiceDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
			SetValueIfNotEmpty(invoiceDate, (s) => invoice.InvoiceDate = s);

			SetValueIfNotEmpty(cusInvoice.IncoTerm, (s) => invoice.UnitPriceTermType = DeliveryOrTransportTermsDescriptionCodeList.GetFromString(s));

			return invoice;
		}

		protected virtual bool SupportsInvoiceNumberSegment => false;

		#region InvoiceSupplierManufacturerParty

		protected void BuildInvoiceSupplierManufacturerParty(Invoice invoice, IOrganisation supplier)
		{
			invoice.SupplierManufacturerParty = BuildInvoiceSupplierManufacturerPartyCore(supplier);
		}

		protected virtual InvoiceSupplierManufacturerParty BuildInvoiceSupplierManufacturerPartyCore(IOrganisation supplier)
		{
			InvoiceSupplierManufacturerParty party = null;

			var name = supplier.Name;

			if (!string.IsNullOrWhiteSpace(name))
			{
				party = new InvoiceSupplierManufacturerParty
				{
					Name = supplier.Name.SplitIntoArray(50, 2)
				};

				SetValueIfNotEmpty(supplier.UEN.ToUpperInvariant().SubstringSafe(0, 17), (c) => party.CodeValue = c);
			}

			return party;
		}

		#endregion

		#region InvoiceTotalInvoiceValue

		protected void BuildInvoiceTotalInvoiceValue(Invoice invoice, ICusInvoice cusInvoice)
		{
			invoice.TotalInvoiceValue = BuildInvoiceTotalInvoiceValueCore(cusInvoice);
		}

		protected virtual InvoiceTotalInvoiceValue BuildInvoiceTotalInvoiceValueCore(ICusInvoice cusInvoice)
		{
			InvoiceTotalInvoiceValue totalInvoiceValue = null;

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(cusInvoice.InvoiceTotalAmount, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var amount) && amount > 0)
			{
				totalInvoiceValue = new InvoiceTotalInvoiceValue
				{
					Amount = new Amount
					{
						Value = amount,
						currencyID = cusInvoice.InvoiceCurrency
					}
				};

				if (cusInvoice.InvoiceCurrency != Core.Constants.CurrencyCodes.Singapore
					&& ZDecimal.TryParse(Utilities.FormatNumberNational(cusInvoice.InvoiceCurrExchangeRate, SGConstants.NumericFormatting.DecimalPlacesForExchangeRateValues), out var exchangeRate)
					&& exchangeRate > 0)
				{
					totalInvoiceValue.ExchangeRate = exchangeRate;
					totalInvoiceValue.ExchangeRateSpecified = true;
				}
			}

			return totalInvoiceValue;
		}

		#endregion

		#region InvoiceFreightCharge

		protected void BuildInvoiceFreightCharge(Invoice invoice, ICusCharge charge)
		{
			invoice.FreightCharge = BuildInvoiceFreightChargeCore(charge);
		}

		protected virtual InvoiceFreightCharge BuildInvoiceFreightChargeCore(ICusCharge charge)
		{
			InvoiceFreightCharge freightCharge = null;

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(charge.Amount, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var amount) && amount > 0)
			{
				freightCharge = new InvoiceFreightCharge
				{
					Amount = new Amount
					{
						Value = amount,
						currencyID = charge.CurrencyCode
					}
				};

				if (ZDecimal.TryParse(Utilities.FormatNumberNational(charge.Percentage, SGConstants.NumericFormatting.DecimalPlacesForPercentageValues), out var percentage) && percentage > 0)
				{
					freightCharge.ChargePercent = percentage;
					freightCharge.ChargePercentSpecified = true;
				}

				if (charge.CurrencyCode != Core.Constants.CurrencyCodes.Singapore
					&& ZDecimal.TryParse(Utilities.FormatNumberNational(charge.ExchangeRate, SGConstants.NumericFormatting.DecimalPlacesForExchangeRateValues), out var exchangeRate)
					&& exchangeRate > 0)
				{
					freightCharge.ExchangeRate = exchangeRate;
					freightCharge.ExchangeRateSpecified = true;
				}
			}

			return freightCharge;
		}

		#endregion

		#region InvoiceInsuranceCharge

		protected void BuildInvoiceInsuranceCharge(Invoice invoice, ICusCharge charge)
		{
			invoice.InsuranceCharge = BuildInvoiceInsuranceChargeCore(charge);
		}

		protected virtual InvoiceInsuranceCharge BuildInvoiceInsuranceChargeCore(ICusCharge charge)
		{
			InvoiceInsuranceCharge insuranceCharge = null;

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(charge.Amount, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var amount) && amount > 0)
			{
				insuranceCharge = new InvoiceInsuranceCharge
				{
					Amount = new Amount
					{
						Value = amount,
						currencyID = charge.CurrencyCode
					}
				};

				if (ZDecimal.TryParse(Utilities.FormatNumberNational(charge.Percentage, SGConstants.NumericFormatting.DecimalPlacesForPercentageValues), out var percentage) && percentage > 0)
				{
					insuranceCharge.ChargePercent = percentage;
					insuranceCharge.ChargePercentSpecified = true;
				}

				if (charge.CurrencyCode != Core.Constants.CurrencyCodes.Singapore
					&& ZDecimal.TryParse(Utilities.FormatNumberNational(charge.ExchangeRate, SGConstants.NumericFormatting.DecimalPlacesForExchangeRateValues), out var exchangeRate)
					&& exchangeRate > 0)
				{
					insuranceCharge.ExchangeRate = exchangeRate;
					insuranceCharge.ExchangeRateSpecified = true;
				}
			}

			return insuranceCharge;
		}

		#endregion

		#region InvoiceOtherTaxableCharge

		protected void BuildInvoiceOtherTaxableCharge(Invoice invoice, ICusCharge charge)
		{
			invoice.OtherTaxableCharge = BuildInvoiceOtherTaxableChargeCore(charge);
		}

		protected virtual InvoiceOtherTaxableCharge BuildInvoiceOtherTaxableChargeCore(ICusCharge charge)
		{
			InvoiceOtherTaxableCharge otherTaxableCharge = null;

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(charge.Amount, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var amount) && amount > 0)
			{
				otherTaxableCharge = new InvoiceOtherTaxableCharge
				{
					Amount = new Amount
					{
						Value = amount,
						currencyID = charge.CurrencyCode
					}
				};

				if (ZDecimal.TryParse(Utilities.FormatNumberNational(charge.Percentage, SGConstants.NumericFormatting.DecimalPlacesForPercentageValues), out var percentage) && percentage > 0)
				{
					otherTaxableCharge.ChargePercent = percentage;
					otherTaxableCharge.ChargePercentSpecified = true;
				}

				if (charge.CurrencyCode != Core.Constants.CurrencyCodes.Singapore
					&& ZDecimal.TryParse(Utilities.FormatNumberNational(charge.ExchangeRate, SGConstants.NumericFormatting.DecimalPlacesForExchangeRateValues), out var exchangeRate)
					&& exchangeRate > 0)
				{
					otherTaxableCharge.ExchangeRate = exchangeRate;
					otherTaxableCharge.ExchangeRateSpecified = true;
				}
			}

			return otherTaxableCharge;
		}

		#endregion

		#endregion

		#region Item

		public virtual void BuildItem(ITradeNetInSection message)
		{
			var items = new List<Item>();
			var index = 0;

			foreach (var cusItem in CusItems)
			{
				index++;

				var item = BuildItemCore(index, cusItem);

				BuildItemQuantity(item, cusItem);
				BuildTransactionValue(item, cusItem);

				BuildCASCProduct(item, cusItem);
				BuildPackingDescription(item, cusItem);
				BuildShippingMarksInformation(item, cusItem);
				BuildLotIdentification(item, cusItem);

				if (CusDec.InwardTransportCode == SGConstants.TransportCodes.Sea || CusDec.InwardTransportCode == SGConstants.TransportCodes.Air)
				{
					SetValueIfNotEmpty(cusItem.InwardHAWB, (s) => item.InHAWBHUCRHBLNumber = s);
				}

				if (SupportsOutHAWBHUCRHBLNumber)
				{
					if (CusDec.OutwardTransportCode == SGConstants.TransportCodes.Sea || CusDec.OutwardTransportCode == SGConstants.TransportCodes.Air)
					{
						SetValueIfNotEmpty(cusItem.OutwardHAWB, (s) => item.OutHAWBHUCRHBLNumber = s);
					}
				}

				BuildItemCertificate(item, cusItem);
				BuildInvoiceDetails(item, cusItem);

				if (cusItem.IsMotorVehicle)
				{
					BuildMotorVehicleCore(item, cusItem);
				}

				BuildTariff(item, cusItem);

				items.Add(item);
			}

			message.Item = items.ToArray();
		}

		protected virtual bool SupportsOutHAWBHUCRHBLNumber => true;

		protected IEnumerable<ICusItem> CusItems => GetLineItemCollection().Where(c => !IsPartialRefundWithNoRefundThisItem(c)).Take(50);

		protected virtual IEnumerable<ICusItem> GetLineItemCollection() => CusDec.Items;

		protected bool IsPartialRefundWithNoRefundThisItem(ICusItem item) => IsPRS
			&& item.ItemDutyRefund == 0
			&& item.ItemExciseRefund == 0
			&& item.ItemGSTRefund == 0;

		protected bool IsPRS => isPRS ?? (isPRS = (CusDec.AdditionalMessageInformation?.UpdateIndicator ?? ZString.Empty) == UpdateIndicatorCodeList.Codes.PRS).Value;
		bool? isPRS;

		protected virtual Item BuildItemCore(int index, ICusItem cusItem)
		{
			var item = new Item
			{
				ItemSequenceNumeric = index,
				ItemSequenceNumericSpecified = true,

				ItemHarmonizedSystemCode = cusItem.HSCode,
				OriginCountry = cusItem.CountryOfOriginCode,

				BrandName = CusDec.IsImport && cusItem.BrandName.IsEmpty ? (ZString)SGConstants.Unbranded : cusItem.BrandName,
				GoodsDescription = cusItem.GoodsDescription.Replace("\r\n", " ").SubstringSafe(0, 512),

				DangerousGoodsIndicator = cusItem.DGIndicator == DGIndicatorCodeList.Codes.Y,
				DangerousGoodsIndicatorSpecified = !cusItem.DGIndicator.IsEmpty
			};

			SetValueIfNotEmpty(cusItem.ModelDescription, (s) => item.ModelDescription = s);

			return item;
		}

		protected virtual void BuildInvoiceDetails(Item item, ICusItem cusItem)
		{
			SetValueIfNotEmpty(cusItem.InvoiceNumber, (s) => item.ItemInvoiceNumber = s);
		}

		#region ItemQuantity

		protected void BuildItemQuantity(Item item, ICusItem cusItem)
		{
			item.ItemQuantity = BuildItemQuantityCore(cusItem);
		}

		protected virtual ItemQuantity BuildItemQuantityCore(ICusItem cusItem)
		{
			var itemQuantity = new ItemQuantity();
			var quantity = ZDecimal.Parse(Utilities.FormatNumberNational(cusItem.HSQuantity, SGConstants.NumericFormatting.DecimalPlacesForMeasurementValues));

			itemQuantity.HarmonizedSystemQuantity = new HarmonizedSystemQuantity
			{
				Value = quantity,
				unitCode = cusItem.HSQuantityUnitType
			};

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(cusItem.PercentageOfAlcohol, SGConstants.NumericFormatting.DecimalPlacesForPercentageValues), out var percentage) && percentage > 0)
			{
				itemQuantity.AlcoholPercent = percentage;
				itemQuantity.AlcoholPercentSpecified = true;
			}

			var isTotal = cusItem.DutyUnitRate > 0m || cusItem.ExciseUnitRate > 0m;

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(isTotal ? cusItem.TotalDutiableQuantity : cusItem.UnitDutiableQuantity, SGConstants.NumericFormatting.DecimalPlacesForMeasurementValues), out var totalDutiableQuantity) && totalDutiableQuantity > 0)
			{
				itemQuantity.TotalDutiableQuantity = new TotalDutiableQuantity
				{
					Value = totalDutiableQuantity,
					unitCode = isTotal ? cusItem.TotalDutiableQuantityUnitType : cusItem.UnitDutiableQuantityUnitType
				};
			}

			if (isTotal || cusItem.DutyPercentageRate > 0 || cusItem.ExcisePercentageRate > 0)
			{
				if (ZDecimal.TryParse(Utilities.FormatNumberNational(cusItem.UnitDutiableQuantity, SGConstants.NumericFormatting.DecimalPlacesForMeasurementValues), out var unitDutiableQuantity) && unitDutiableQuantity > 0)
				{
					itemQuantity.DutiableQuantity = new DutiableQuantity
					{
						Value = unitDutiableQuantity,
						unitCode = cusItem.UnitDutiableQuantityUnitType
					};
				}
			}

			return itemQuantity;
		}

		#endregion

		#region TransactionValue

		protected virtual void BuildTransactionValue(Item item, ICusItem cusItem)
		{
			var transactionValue = item.TransactionValue = BuildTransactionValueCore(cusItem);

			var optionalItemCharge = cusItem.OptionalItemCharge;
			if (optionalItemCharge != null)
			{
				var itemCharge = BuildOptionalItemChargeCore(optionalItemCharge);

				if (itemCharge != null)
				{
					transactionValue = transactionValue ?? new TransactionValue();
					transactionValue.OptionalItemCharge = itemCharge;
				}
			}
		}

		protected virtual TransactionValue BuildTransactionValueCore(ICusItem cusItem)
		{
			var hasData = false;
			var transactionValue = new TransactionValue();
			if (ZDecimal.TryParse(Utilities.FormatNumberNational(cusItem.CustomsValue, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var customsValue) && customsValue > 0)
			{
				transactionValue.ItemCIFFOBValue = customsValue;
				transactionValue.ItemCIFFOBValueSpecified = true;
				hasData = true;
			}

			if (cusItem.LSPValue > 0)
			{
				var lspValueBuilt = BuildLastSellingPrice(cusItem, transactionValue);
				hasData = hasData || lspValueBuilt;
			}

			if (cusItem.UnitPrice > 0)
			{
				var unitPriceBuilt = BuildUnitPriceValue(cusItem, transactionValue);
				hasData = hasData || unitPriceBuilt;
			}

			return hasData ? transactionValue : null;
		}

		protected virtual bool BuildLastSellingPrice(ICusItem cusItem, TransactionValue transactionValue)
		{
			var hasData = false;
			if (ZDecimal.TryParse(Utilities.FormatNumberNational(cusItem.LSPValue, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var lspValue) && lspValue > 0)
			{
				transactionValue.LastSellingPriceValue = lspValue;
				transactionValue.LastSellingPriceValueSpecified = true;
				hasData = true;
			}

			return hasData;
		}

		protected virtual bool BuildUnitPriceValue(ICusItem cusItem, TransactionValue transactionValue)
		{
			var hasData = false;
			if (cusItem.IsMotorVehicle && ZDecimal.TryParse(Utilities.FormatNumberNational(cusItem.UnitPrice, SGConstants.NumericFormatting.DecimalPlacesForUnitPriceAmountValues), out var unitPrice) && unitPrice > 0)
			{
				transactionValue.UnitPriceValue = new TransactionValueUnitPriceValue
				{
					Amount = new Amount { Value = unitPrice, currencyID = cusItem.InvoiceCurrency },
				};

				if (RequiresUnitPriceExchangeRate(cusItem))
				{
					if (cusItem.InvoiceCurrency == Core.Constants.CurrencyCodes.Singapore)
					{
						transactionValue.UnitPriceValue.ExchangeRate = 1.00m;
						transactionValue.UnitPriceValue.ExchangeRateSpecified = true;
					}
					else if (ZDecimal.TryParse(Utilities.FormatNumberNational(cusItem.InvoiceCurrExchangeRate, SGConstants.NumericFormatting.DecimalPlacesForExchangeRateValues), out var exchangeRate)
					&& exchangeRate > 0)
					{
						transactionValue.UnitPriceValue.ExchangeRate = exchangeRate;
						transactionValue.UnitPriceValue.ExchangeRateSpecified = true;
					}
				}

				hasData = true;
			}

			return hasData;
		}

		protected virtual bool RequiresUnitPriceExchangeRate(ICusItem cusItem)
		{
			return cusItem.InvoiceCurrency != Core.Constants.CurrencyCodes.Singapore;
		}

		protected virtual TransactionValueOptionalItemCharge BuildOptionalItemChargeCore(ICusCharge cusCharge)
		{
			TransactionValueOptionalItemCharge optionalItemCharge = null;

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(cusCharge.Amount, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var amount) && amount > 0)
			{
				optionalItemCharge = new TransactionValueOptionalItemCharge()
				{
					Amount = new Amount
					{
						Value = amount,
						currencyID = cusCharge.CurrencyCode
					}
				};

				if (cusCharge.ExchangeRate != 1m && ZDecimal.TryParse(Utilities.FormatNumberNational(cusCharge.ExchangeRate, SGConstants.NumericFormatting.DecimalPlacesForExchangeRateValues), out var exchangeRate) && exchangeRate > 0)
				{
					optionalItemCharge.ExchangeRate = exchangeRate;
					optionalItemCharge.ExchangeRateSpecified = true;
				}
			}

			return optionalItemCharge;
		}

		#endregion

		#region CASCProduct

		protected void BuildCASCProduct(Item item, ICusItem cusItem)
		{
			var products = new List<CASCProduct>();
			if (cusItem.ProductCodes.Any())
			{
				foreach (var cusProduct in cusItem.ProductCodes.Take(5))
				{
					if (cusProduct != null)
					{
						var product = BuildCASCProductCore(cusProduct);
						BuildCASCProductAdditionalCASCIdentification(product, cusItem);
						products.Add(product);
					}
				}
			}
			else if (cusItem.CASCCodes1 != null)
			{
				products.Add(BuildIndependantCASC(cusItem));
			}

			if (cusItem.IsStrategic)
			{
				products.Add(BuildStrategicGoodsData(cusItem));
			}

			item.CASCProduct = products.Any() ? products.ToArray() : null;
		}

		protected virtual CASCProduct BuildCASCProductCore(ICusProductCode cusProduct)
		{
			var product = new CASCProduct
			{
				CASCProductCode = cusProduct.ProductCode
			};

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(cusProduct.ProductCodeQty, SGConstants.NumericFormatting.DecimalPlacesForMeasurementValues), out var productCodeQty) && productCodeQty > 0)
			{
				product.CASCProductQuantity = new CASCProductQuantity
				{
					Value = productCodeQty,
					unitCode = cusProduct.ProductCodeUnitType
				};
			}

			return product;
		}

		#region CASCProductAdditionalCASCIdentification

		protected void BuildCASCProductAdditionalCASCIdentification(CASCProduct product, ICusItem cusItem)
		{
			if (cusItem.CASCCodes1 != null)
			{
				var list = BuildAdditionalCASCIdentificationCore(cusItem);
				product.AdditionalCASCIdentification = list.Any() ? list.ToArray() : null;
			}
		}

		protected CASCProduct BuildIndependantCASC(ICusItem cusItem)
		{
			var product = new CASCProduct { };
			var list = BuildAdditionalCASCIdentificationCore(cusItem);
			product.AdditionalCASCIdentification = list.Any() ? list.ToArray() : null;
			return product;
		}

		protected virtual List<CASCProductAdditionalCASCIdentification> BuildAdditionalCASCIdentificationCore(ICusItem cusItem)
		{
			var additionalCASCIdentifications = new List<CASCProductAdditionalCASCIdentification>();

			var codes2Count = cusItem.CASCCodes2?.Count ?? 0;
			var codes3Count = cusItem.CASCCodes3?.Count ?? 0;

			cusItem.CASCCodes1.Sort(CusCodeDataSchema.Constants.CY_Order, ListSortDirection.Ascending);
			if (codes2Count > 0)
			{
				cusItem.CASCCodes2.Sort(CusCodeDataSchema.Constants.CY_Order, ListSortDirection.Ascending);
			}

			if (codes3Count > 0)
			{
				cusItem.CASCCodes3.Sort(CusCodeDataSchema.Constants.CY_Order, ListSortDirection.Ascending);
			}

			for (int i = 0; i < cusItem.CASCCodes1.Count; i++)
			{
				if (!cusItem.CASCCodes1[i].CY_Data.IsEmpty)
				{
					var additionalCASCIdentification = new CASCProductAdditionalCASCIdentification
					{
						CASCCodeOne = cusItem.CASCCodes1[i].CY_Data.SubstringSafe(0, 35)
					};

					if (codes2Count > i && !cusItem.CASCCodes2[i].CY_Data.IsEmpty)
					{
						additionalCASCIdentification.CASCCodeTwo = cusItem.CASCCodes2[i].CY_Data.SubstringSafe(0, 35);
					}

					if (codes3Count > i && !cusItem.CASCCodes3[i].CY_Data.IsEmpty)
					{
						additionalCASCIdentification.CASCCodeThree = cusItem.CASCCodes3[i].CY_Data.SubstringSafe(0, 35);
					}

					additionalCASCIdentifications.Add(additionalCASCIdentification);
				}
			}

			return additionalCASCIdentifications;
		}

		#endregion

		protected virtual CASCProduct BuildStrategicGoodsData(ICusItem cusItem)
		{
			return null;
		}

		#region PackingDescription

		protected void BuildPackingDescription(Item item, ICusItem cusItem)
		{
			item.PackingDescription = BuildPackingDescriptionCore(cusItem);
		}

		protected virtual PackingDescription BuildPackingDescriptionCore(ICusItem cusItem)
		{
			var packingDescription = new PackingDescription();
			var hasData = false;

			var packInQuantity = cusItem.PackInQuantity;

			if (packInQuantity > 0)
			{
				hasData = true;

				packingDescription.InPackQuantity = new InPackQuantity
				{
					Value = packInQuantity,
					unitCode = cusItem.PackInUnitType
				};
			}

			var packOuterQuantity = cusItem.PackOuterQuantity;

			if (packOuterQuantity > 0)
			{
				hasData = true;

				packingDescription.OuterPackQuantity = new OuterPackQuantity
				{
					Value = packOuterQuantity,
					unitCode = cusItem.PackOuterUnitType
				};
			}

			var packInnerQuantity = cusItem.PackInnerQuantity;

			if (packInnerQuantity > 0)
			{
				hasData = true;

				packingDescription.InnerPackQuantity = new InnerPackQuantity
				{
					Value = packInnerQuantity,
					unitCode = cusItem.PackInnerUnitType
				};
			}

			var packInmostQuantity = cusItem.PackInmostQuantity;

			if (packInmostQuantity > 0)
			{
				hasData = true;

				packingDescription.InmostPackQuantity = new InmostPackQuantity
				{
					Value = packInmostQuantity,
					unitCode = cusItem.PackInmostUnitType
				};
			}

			return hasData ? packingDescription : null;
		}

		#endregion

		#endregion

		#region ShippingMarksInformation

		protected void BuildShippingMarksInformation(Item item, ICusItem cusItem)
		{
			var list = BuildShippingMarksInformationCore(cusItem);
			item.ShippingMarksInformation = list.Any() ? list.ToArray() : null;
		}

		protected virtual ShippingMarksInformation[] BuildShippingMarksInformationCore(ICusItem cusItem)
		{
			var shippingMarksLines = new List<ShippingMarksInformation>();

			var marks = cusItem.MarksAndNumbers.ToUpperInvariant();
			var length = marks.Length;
			if (length > 0)
			{
				var marksAndNumbers = marks.SubstringSafe(0, 340).SplitIntoArray(17, 20).ToList();
				while (marksAndNumbers.Any())
				{
					var currentMarks = marksAndNumbers.Take(10).ToArray();
					shippingMarksLines.Add(new ShippingMarksInformation { ShippingMarks = currentMarks });
					marksAndNumbers.RemoveRange(0, currentMarks.Length);
				}

				if (length > 340)
				{
					marksAndNumbers = marks.SubstringSafe(340, 136).SplitIntoArray(17, 8).ToList();
					while (marksAndNumbers.Any())
					{
						var currentMarks = marksAndNumbers.Take(8).ToArray();
						shippingMarksLines.Add(new ShippingMarksInformation { ShippingMarks = currentMarks });
						marksAndNumbers.RemoveRange(0, currentMarks.Length);
					}

					if (length > 476)
					{
						marksAndNumbers = marks.SubstringSafe(476, 36).SplitIntoArray(12, 3).ToList();
						while (marksAndNumbers.Any())
						{
							var currentMarks = marksAndNumbers.Take(3).ToArray();
							shippingMarksLines.Add(new ShippingMarksInformation { ShippingMarks = currentMarks });
							marksAndNumbers.RemoveRange(0, currentMarks.Length);
						}
					}
				}
			}

			return shippingMarksLines.ToArray();
		}

		#endregion

		#region LotIdentification

		protected void BuildLotIdentification(Item item, ICusItem cusItem)
		{
			item.LotIdentification = BuildLotIdentificationCore(cusItem);
		}

		protected virtual LotIdentification BuildLotIdentificationCore(ICusItem cusItem)
		{
			var lotIdentification = new LotIdentification();

			var hasData = SetValueIfNotEmpty(cusItem.CurrentLotNumber, (s) => lotIdentification.CurrentLotNumber = s);
			hasData |= SetValueIfNotEmpty(cusItem.PreviousLotNumber, (s) => lotIdentification.PreviousLotNumber = s);
			hasData |= SetValueIfNotEmpty(cusItem.E_SDNPIndicator, (s) => lotIdentification.Marking = s);

			return hasData ? lotIdentification : null;
		}

		#endregion

		#region ItemCertificate

		protected void BuildItemCertificate(Item item, ICusItem cusItem)
		{
			item.ItemCertificate = BuildItemCertificateCore(cusItem);
		}

		protected virtual ItemCertificate BuildItemCertificateCore(ICusItem cusItem)
		{
			return null;
		}

		#region CertificateItemDescription

		protected void BuildCertificateItemDescription(ItemCertificate itemCertificate, ICusCertItem cusCertItem)
		{
			var list = BuildCertificateItemDescriptionCore(cusCertItem);
			itemCertificate.ItemCertificateDescription = list.Any() ? list.ToArray() : null;
		}

		protected ItemCertificateItemCertificateDescription[] BuildCertificateItemDescriptionCore(ICusCertItem cusCertItem)
		{
			var itemDescLines = new List<ItemCertificateItemCertificateDescription>();
			var descriptions = cusCertItem.ItemDescription.ToUpperInvariant().SplitIntoArray(35, 50).ToList();
			while (descriptions.Any())
			{
				var currentDescriptions = descriptions.Take(5).ToArray();
				itemDescLines.Add(new ItemCertificateItemCertificateDescription { Line = currentDescriptions });
				descriptions.RemoveRange(0, currentDescriptions.Length);
			}

			return itemDescLines.ToArray();
		}

		#endregion

		#region OriginCriterion

		protected void BuildOriginCriterion(ItemCertificate itemCertificate, ICusCertItem certItem, string certificateType)
		{
			var list = BuildOriginCriterionCore(certItem, certificateType);
			itemCertificate.OriginCriterion = list.Any() ? list.ToArray() : null;
		}

		List<string> BuildOriginCriterionCore(ICusCertItem certItem, string certificateType)
		{
			var originCriterion = new List<string>();
			var originCriterion1 = certItem.OriginCriterion1;

			if (!originCriterion1.IsEmpty)
			{
				originCriterion.Add(originCriterion1);
				SetValueIfNotEmpty(certItem.OriginCriterion2, (s) => originCriterion.Add(s));
				SetValueIfNotEmpty(certItem.OriginCriterion3, (s) => originCriterion.Add(s));
			}

			return originCriterion;
		}

		#endregion

		#endregion

		#region MotorVehicle

		protected void BuildMotorVehicleCore(Item item, ICusItem cusItem)
		{
			if (SupportsMotorVehicleSection)
			{
				item.MotorVehicle = BuildMotorVehicleCore(cusItem);
			}
		}

		protected virtual MotorVehicle BuildMotorVehicleCore(ICusItem cusItem)
		{
			var hasData = false;
			var motorVehicle = new MotorVehicle();

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(cusItem.EngineCapacity, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var engineCapacity) && engineCapacity > 0)
			{
				motorVehicle.EngineCapacity = new EngineCapacity
				{
					Value = engineCapacity,
					unitCode = cusItem.EngineCapacityUnit,
				};

				hasData = true;
			}

			if (SupportsRegistrationDateSection)
			{
				motorVehicle.OriginalRegistrationDate = cusItem.DateOfFirstRegistration.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
				hasData = true;
			}

			return hasData ? motorVehicle : null;
		}

		protected virtual bool SupportsMotorVehicleSection => true;

		protected virtual bool SupportsRegistrationDateSection => false;

		#endregion

		#region Tariff

		protected virtual void BuildTariff(Item item, ICusItem cusItem)
		{
			var tariff = item.Tariff = BuildTariffCore(cusItem);

			BuildTariffGoodsAndServicesTax(tariff, cusItem);

			if (cusItem.ExciseAmount > 0)
			{
				BuildTariffExciseDuty(tariff, cusItem);
			}

			if (cusItem.DutyAmount > 0)
			{
				BuildTariffCustomsDuty(tariff, cusItem);
			}

			if (SupportsOtherTax && cusItem.OtherTaxAmount > 0)
			{
				BuildTariffOtherTax(tariff, cusItem);
			}
		}

		protected virtual Tariff BuildTariffCore(ICusItem cusItem)
		{
			var tariff = new Tariff();

			var preferenceIndicator = cusItem.PreferenceIndicator;

			if (!string.IsNullOrWhiteSpace(preferenceIndicator) && preferenceIndicator != PreferentialIndicatorCodeList.Codes.STD)
			{
				tariff.PreferentialCode = preferenceIndicator;
			}

			return tariff;
		}

		#region TariffGoodsAndServicesTax

		protected void BuildTariffGoodsAndServicesTax(Tariff tariff, ICusItem cusItem)
		{
			tariff.GoodsAndServicesTax = BuildTariffGoodsAndServicesTaxCore(cusItem);
		}

		protected virtual TariffGoodsAndServicesTax BuildTariffGoodsAndServicesTaxCore(ICusItem cusItem)
		{
			TariffGoodsAndServicesTax goodsAndServicesTax = null;

			if (cusItem.GSTRate > 0)
			{
				var amount = ZDecimal.Parse(Utilities.FormatNumberNational(cusItem.GSTPayable, SGConstants.NumericFormatting.DecimalPlacesForAmountValues));

				goodsAndServicesTax = new TariffGoodsAndServicesTax
				{
					GoodsAndServicesTaxPercent = cusItem.GSTRate,
					GoodsAndServicesTaxPercentSpecified = true,

					GoodsAndServicesTaxAmount = amount,
					GoodsAndServicesTaxAmountSpecified = true
				};
			}

			return goodsAndServicesTax;
		}

		#endregion

		#region TariffExciseDuty

		protected void BuildTariffExciseDuty(Tariff tariff, ICusItem cusItem)
		{
			tariff.ExciseDuty = BuildTariffExciseDutyCore(cusItem);
		}

		protected virtual TariffExciseDuty BuildTariffExciseDutyCore(ICusItem cusItem)
		{
			var exciseDuty = new TariffExciseDuty();

			var exciseRate = cusItem.ExcisePercentageRate.IsEmpty ? cusItem.ExciseUnitRate : cusItem.ExcisePercentageRate;

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(exciseRate, SGConstants.NumericFormatting.DecimalPlacesForTaxRateValues), out var rate))
			{
				exciseDuty.DutyRate = rate;
				exciseDuty.DutyRateSpecified = rate >= 0;
			}

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(cusItem.ExciseAmount, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var amount))
			{
				exciseDuty.DutyAmount = amount;
				exciseDuty.DutyAmountSpecified = amount >= 0;
			}

			exciseDuty.DutyRateUnit = cusItem.DutyRateUnit;

			return exciseDuty;
		}

		#endregion

		#region TariffCustomsDuty

		protected void BuildTariffCustomsDuty(Tariff tariff, ICusItem cusItem)
		{
			tariff.CustomsDuty = BuildTariffCustomsDutyCore(cusItem);
		}

		protected virtual TariffCustomsDuty BuildTariffCustomsDutyCore(ICusItem cusItem)
		{
			var customsDuty = new TariffCustomsDuty();

			var dutyRate = cusItem.DutyPercentageRate.IsEmpty ? cusItem.DutyUnitRate : cusItem.DutyPercentageRate;

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(dutyRate, SGConstants.NumericFormatting.DecimalPlacesForTaxRateValues), out var rate))
			{
				customsDuty.DutyRate = rate;
				customsDuty.DutyRateSpecified = rate >= 0;
			}

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(cusItem.DutyAmount, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var amount))
			{
				customsDuty.DutyAmount = amount;
				customsDuty.DutyAmountSpecified = amount >= 0;
			}

			customsDuty.DutyRateUnit = cusItem.DutyRateUnit;

			return customsDuty;
		}

		#endregion

		#region TariffOtherTax

		protected virtual bool SupportsOtherTax => false;

		protected void BuildTariffOtherTax(Tariff tariff, ICusItem cusItem)
		{
			tariff.OtherTax = BuildTariffOtherTaxCore(cusItem);
		}

		protected virtual TariffOtherTax BuildTariffOtherTaxCore(ICusItem cusItem)
		{
			TariffOtherTax result = null;

			ZDecimal.TryParse(Utilities.FormatNumberNational(cusItem.OtherTaxAmount, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var otherTaxAmount);

			if (otherTaxAmount > 0)
			{
				result = new TariffOtherTax();

				ZDecimal.TryParse(Utilities.FormatNumberNational(cusItem.OtherTaxPercentageRate.IsEmpty ? cusItem.OtherTaxUnitRate : cusItem.OtherTaxPercentageRate, SGConstants.NumericFormatting.DecimalPlacesForTaxRateValues), out var otherTaxRate);

				result.DutyRate = otherTaxRate;
				result.DutyRateSpecified = otherTaxRate >= 0;

				result.DutyAmount = otherTaxAmount;
				result.DutyAmountSpecified = true;

				result.DutyRateUnit = cusItem.DutyRateUnit;
			}

			return result;
		}

		#endregion

		#endregion

		#endregion

		#region Summary

		public virtual void BuildSummary(ITradeNetInSection message)
		{
			var summary = message.Summary = BuildSummaryCore();

			BuildTotalGrossWeight(summary);
			BuildTotalTariff(summary);
		}

		protected virtual Summary BuildSummaryCore()
		{
			var summary = new Summary
			{
				NumberOfItems = CusDec.Items.Count(),
				NumberOfItemsSpecified = true
			};

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(CusDec.TotalCustomsValue, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var totalCustomsValue) && totalCustomsValue > 0)
			{
				summary.TotalCIFFOBValue = totalCustomsValue;
				summary.TotalCIFFOBValueSpecified = true;
			}

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(CusDec.TotalOuterPack, SGConstants.NumericFormatting.DecimalPlacesNone), out var totalOuterPack))
			{
				summary.TotalOuterPack = new TotalOuterPack
				{
					Value = totalOuterPack,
					unitCode = CusDec.TotalOuterPackUnitOfQty
				};
			}

			return summary;
		}

		#region TotalGrossWeight

		protected void BuildTotalGrossWeight(Summary summary)
		{
			summary.TotalGrossWeight = BuildTotalGrossWeightCore();
		}

		protected virtual TotalGrossWeight BuildTotalGrossWeightCore()
		{
			TotalGrossWeight result = null;

			var weightUnitInRequiredSGCValue =
			(
				((CusDec.IsInwardDeclaration || CusDec.IsTranshipmentDeclaration) && CusDec.InwardTransportCode == SGConstants.TransportCodes.Sea) || (CusDec.IsOutwardDeclaration && CusDec.OutwardTransportCode == SGConstants.TransportCodes.Sea)
			) ? SGConstants.Weight.Tonnes : SGConstants.Weight.Kilograms;

			var (totalWeight, totalWeightUnit) = ConvertToSingaporeCustomsRequiredWeightUnit(CusDec.TotalGrossWeight, CusDec.TotalGrossWeightUnitOfQty, weightUnitInRequiredSGCValue);

			if (totalWeight > 0)
			{
				result = new TotalGrossWeight
				{
					Value = ZDecimal.Parse(Utilities.FormatNumberNational(totalWeight, SGConstants.NumericFormatting.TradeNet4Point1.DecimalPlacesForMeasurementValues)),
					unitCode = totalWeightUnit
				};
			}

			return result;
		}

		#endregion

		#region TotalTariff

		protected void BuildTotalTariff(Summary summary)
		{
			summary.TotalTariff = BuildTotalTariffCore();
		}

		protected virtual TotalTariff BuildTotalTariffCore()
		{
			var totalTariff = new TotalTariff();

			var hasData = false;

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(CusDec.TotalGSTPayable, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var totalGSTPayable) && totalGSTPayable > 0)
			{
				totalTariff.TotalGoodsAndServicesTaxAmount = totalGSTPayable;
				totalTariff.TotalGoodsAndServicesTaxAmountSpecified = true;
				hasData = true;
			}

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(CusDec.TotalExcisePayable, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var totalExcisePayable) && totalExcisePayable > 0)
			{
				totalTariff.TotalExciseDutyAmount = totalExcisePayable;
				totalTariff.TotalExciseDutyAmountSpecified = true;
				hasData = true;
			}

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(CusDec.TotalDutyPayable, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var totalDutyPayable) && totalDutyPayable > 0)
			{
				totalTariff.TotalCustomsDutyAmount = totalDutyPayable;
				totalTariff.TotalCustomsDutyAmountSpecified = true;
				hasData = true;
			}

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(CusDec.TotalOtherTaxPayable, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var totalOtherTaxPayable) && totalOtherTaxPayable > 0)
			{
				totalTariff.TotalOtherTaxAmount = totalOtherTaxPayable;
				totalTariff.TotalOtherTaxAmountSpecified = true;
				hasData = true;
			}

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(CusDec.TotalPayable, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var totalPayable) && totalPayable > 0)
			{
				totalTariff.TotalAmountPayable = totalPayable;
				totalTariff.TotalAmountPayableSpecified = true;
				hasData = true;
			}

			return hasData ? totalTariff : null;
		}

		#endregion

		#endregion

		#endregion

		#region Update

		public Update BuildUpdate()
		{
			return BuildUpdateCore();
		}

		protected Update BuildUpdateCore()
		{
			var update = new Update
			{
				UpdateIndicatorCode = UpdateIndicatorCode,
				UpdatePermitNumber = CusDec.PermitNoToUpdateOrCancel,
				UpdateRequestNumber = CusDec.NumberOfRequestsForUpdate,
				UpdateRequestNumberSpecified = CusDec.NumberOfRequestsForUpdate > 0
			};

			SetValueIfNotEmpty(CusDec.ReplacementPermitNumber, (s) => update.ReplacementPermitNumber = s);

			if (SupportsUpdateAmendmentSection)
			{
				update.Amendment = BuildUpdateAmendment();
			}

			if (SupportsUpdateRefundSection)
			{
				update.Refund = BuildUpdateRefund();
			}

			return update;
		}

		protected UpdateAmendment BuildUpdateAmendment()
		{
			return BuildUpdateAmendmentCore();
		}

		protected virtual UpdateAmendment BuildUpdateAmendmentCore()
		{
			UpdateAmendment amendment = null;

			var additionalInfo = CusDec.AdditionalMessageInformation;
			var amendmentReason = additionalInfo.ReasonForAmending;
			var extensionRequested = additionalInfo.ExtendingTemporaryImportPeriod;

			if (!amendmentReason.IsEmpty || extensionRequested)
			{
				amendment = new UpdateAmendment();

				if (!amendmentReason.IsEmpty)
				{
					amendment.AmendmentReason = amendmentReason.SplitIntoArray(70, 4);
				}

				if (extensionRequested)
				{
					amendment.PermitValidityExtensionIndicator = true;
					amendment.PermitValidityExtensionIndicatorSpecified = true;

					var extensionReason = additionalInfo.ReasonForExtendingTemporaryImportPeriod;
					if (SupportsUpdateAmendmentExtensionReasonSection && !extensionReason.IsEmpty)
					{
						amendment.ExtensionReason = extensionReason.SplitIntoArray(70, 4);
					}
				}
			}

			return amendment;
		}

		protected UpdateRefund BuildUpdateRefund()
		{
			return BuildUpdateRefundCore();
		}

		protected virtual UpdateRefund BuildUpdateRefundCore()
		{
			UpdateRefund refund = null;

			var additionalInfo = CusDec.AdditionalMessageInformation;
			var reasonCode = additionalInfo.RefundCode;

			if (!reasonCode.IsEmpty)
			{
				refund = new UpdateRefund()
				{
					ReasonCode = reasonCode
				};

				var updateReason = additionalInfo.ReasonForRefund;
				if (!updateReason.IsEmpty)
				{
					refund.Reason = updateReason.SplitIntoArray(70, 4);
				}
			}

			return refund;
		}

		protected virtual ZString UpdateIndicatorCode => Enterprise.Customs.SG.V4.Business.SGConstants.UpdateIndicators.AME;
		protected virtual bool SupportsUpdateAmendmentSection => false;
		protected virtual bool SupportsUpdateAmendmentExtensionReasonSection => false;
		protected virtual bool SupportsUpdateRefundSection => false;

		#endregion

		#region Cancellation

		public Cancellation BuildCancellation()
		{
			return BuildCancellationCore();
		}

		protected Cancellation BuildCancellationCore()
		{
			var cancellation = new Cancellation();

			cancellation.CancellationHeader = BuildCancellationHeader();
			cancellation.DeclarantParty = BuildCancellationDeclarantParty();
			cancellation.SupportingDocumentReference = BuildCancellationSupportingDocumentReference();
			return cancellation;
		}

		protected CancellationCancellationHeader BuildCancellationHeader()
		{
			var header = new CancellationCancellationHeader();
			BuildCommonHeaderSection(header);
			header.CancellationReasonCode = CusDec.AdditionalMessageInformation?.CancellationCode;

			return header;
		}

		DeclarantParty BuildCancellationDeclarantParty()
		{
			DeclarantParty declarantParty = null;

			var declarant = CusDec.Declarant;
			if (declarant != null)
			{
				declarantParty = BuildDeclarantPartyCore(declarant);
				declarantParty.PersonInformation = BuildDeclarantPartyPersonInformationCore(declarant);
			}

			return declarantParty;
		}

		SupportingDocumentReference[] BuildCancellationSupportingDocumentReference()
		{
			var references = BuildSupportingDocumentReferenceCore();
			return references.Any() ? references.ToArray() : null;
		}

		#endregion

		#region ITradeNetMessage

		void ITradeNetMessage.SetMessageText(string text)
		{
			MessageText = text;
		}

		#endregion

		#region ICusMessage

		public string MessageText { get; private set; } = string.Empty;

		public abstract string MessageType { get; }

		public abstract string MessageSubType { get; }

		#endregion
	}
}
