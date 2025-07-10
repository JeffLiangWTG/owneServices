using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Edifact.D09B.Elements;
using Enterprise.Edifact.D09B.Messages.CUSDEC;
using Enterprise.Edifact.D09B.Segments;
using Enterprise.Edifact.Utilities;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.Business.CustomsMessaging.D09B
{
	public abstract class Cusdec09b : CusdecBase<CUSDECMessage>
	{
		public Cusdec09b(ISGCUSDEC sgCusdec)
			: base(sgCusdec)
		{
		}

		protected override string UnhMessageReleaseNumber
		{
			get { return "09B"; }
		}

		protected override string UnhAssociationAssignedCode
		{
			get { return "041"; }
		}

		protected abstract string CommonAccessReferenceCode { get; }

		#region Message Segments

		#region Header Section

		protected override sealed void GenerateHeaderSection(CUSDECMessage cusdecEdifactMsg)
		{
			GenerateUNH(cusdecEdifactMsg.UNH);
			GenerateBGM(cusdecEdifactMsg.BGM);
			GenerateCST(cusdecEdifactMsg.CST);
			GenerateHeaderLocSegments(cusdecEdifactMsg.LOC);
			GenerateHeaderDtmSegments(cusdecEdifactMsg.DTM);
			GenerateGEI(cusdecEdifactMsg.GEI);
			GenerateHeaderMeaSegments(cusdecEdifactMsg.MEA);
			GenerateEQDAndSELSegments(cusdecEdifactMsg.EQD, cusdecEdifactMsg.SEL);
			GenerateHeaderFtxSegment(cusdecEdifactMsg.FTX);
			GenerateHeaderSegmentGroup1(cusdecEdifactMsg.Group1);
			GenerateSegmentGroup4(cusdecEdifactMsg.Group4);
			GenerateSegmentGroup5(cusdecEdifactMsg.Group5);
			GenerateHeaderSegmentGroup6(cusdecEdifactMsg.Group6);
		}

		void GenerateUNH(UNHSegmentMessageSection unhSection)
		{
			var unh = unhSection.InstantiateAChildAndAddItToChildrenCollection();
			var messageReference = new ZString("WTG" + sgCusdec.JobNumber);
			unh.MessageReferenceNumber = messageReference.Right(14);
			unh.MessageIdentifier.MessageType = CusdecConstants.UnhMessageTypeIdentifier;
			unh.MessageIdentifier.MessageVersionNumber = UnhMessageVersionNumber;
			unh.MessageIdentifier.MessageReleaseNumber = UnhMessageReleaseNumber;
			unh.MessageIdentifier.ControllingAgency = UnhControllingAgency;
			unh.MessageIdentifier.AssociationAssignedCode = UnhAssociationAssignedCode;
			unh.CommonAccessReference = MessageType;
		}

		void GenerateBGM(BGMSegmentMessageSection bgmSection)
		{
			var bgm = bgmSection.InstantiateAChildAndAddItToChildrenCollection();
			bgm.DocumentMessageName.DocumentNameCode = DocumentNameCode;

			if (DocumentNameCode == DocumentNameCodeList.CustomsDeclarationWithCommercialAndItemDetail)
			{
				bgm.DocumentMessageName.DocumentName = sgCusdec.DeclarationType;
			}

			bgm.DocumentMessageIdentification.DocumentIdentifier = EDIMessage.MessageNumberPlaceHolder;
			bgm.MessageFunctionCode = MessageFunctionCode;
		}

		protected virtual DocumentNameCodeList DocumentNameCode
		{
			get { return DocumentNameCodeList.CustomsDeclarationWithCommercialAndItemDetail; }
		}

		protected virtual MessageFunctionCodeList MessageFunctionCode
		{
			get { return MessageFunctionCodeList.Original; }
		}

		protected virtual CSTSegment GenerateCST(CSTSegmentMessageSection cstSection)
		{
			var cst = cstSection.InstantiateAChildAndAddItToChildrenCollection();
			cst.CustomsIdentityCodes1.CustomsGoodsIdentifier = sgCusdec.CargoPackingType;
			return cst;
		}

		#region Locations

		protected virtual void GenerateHeaderLocSegments(LOCSegmentMessageSection locSection)
		{
			if (sgCusdec.HasInwardTransport)
			{
				GenerateLOCSegment(
					locSection, LocationFunctionCodeQualifierList.PlaceOfLoading,
					sgCusdec.PortOfLoading, string.Empty);
			}

			if (sgCusdec.HasOutwardTransport && !sgCusdec.IsSeaStoreDeclaration)
			{
				GenerateLOCSegment(
					locSection, LocationFunctionCodeQualifierList.PortOfDischarge,
					sgCusdec.PortOfDischarge, string.Empty);
			}

			if (sgCusdec.PlaceOfRelease != null)
			{
				GenerateLOCSegment(
					locSection, LocationFunctionCodeQualifierList.PlaceOfDischarge,
					sgCusdec.PlaceOfRelease);
			}

			if (sgCusdec.PlaceOfReceipt != null)
			{
				GenerateLOCSegment(
					locSection, LocationFunctionCodeQualifierList.PlaceOfReceipt,
					sgCusdec.PlaceOfReceipt);
			}
		}

		#region LOC Segments

		void GenerateLOCSegment(LOCSegmentMessageSection locSection, LocationFunctionCodeQualifierList qualifier, ISGCPlace location)
		{
			var nameAndAddress = location.AddressRequired ? location.NameAndAddress : ZString.Empty;
			var locationCode = location.IsNonSystemNonLicenced ? location.Type : location.Code;
			GenerateLOCSegment(locSection, qualifier, locationCode, nameAndAddress);
		}

		protected void GenerateLOCSegment(LOCSegmentMessageSection locSection, LocationFunctionCodeQualifierList qualifier, ZString locationCode, string nameAndAddress)
		{
			if (!locationCode.IsEmpty)
			{
				var loc = locSection.InstantiateAChildAndAddItToChildrenCollection();
				loc.LocationFunctionCodeQualifier = qualifier;
				loc.LocationIdentification.LocationIdentifier = locationCode;
				if (nameAndAddress.Length > 0)
				{
					loc.LocationIdentification.LocationName = nameAndAddress;
				}
			}
		}

		#region For subclasses

		protected void GeneratePlaceOfStorageSegment(LOCSegmentMessageSection locSection)
		{
			GenerateLOCSegment(locSection, LocationFunctionCodeQualifierList.GoodsItemStorageLocation, sgCusdec.PlaceOfStorage, string.Empty);
		}

		protected void GenerateCountryOfFinalDestinationSegment(LOCSegmentMessageSection locSection)
		{
			GenerateLOCSegment(locSection, LocationFunctionCodeQualifierList.CountryOfUltimateDestination, sgCusdec.CountryOfFinalDestination, string.Empty);
		}

		protected void GenerateNextPortOfCallSegment(LOCSegmentMessageSection locSection)
		{
			GenerateLOCSegment(locSection, LocationFunctionCodeQualifierList.NextPortOfCall, sgCusdec.NextPortOfCall, string.Empty);
		}

		protected void GenerateFinalPortOfCallSegment(LOCSegmentMessageSection locSection)
		{
			GenerateLOCSegment(locSection, LocationFunctionCodeQualifierList.PlaceOfUltimateDestinationOfConveyance, sgCusdec.FinalPortOfCall, string.Empty);
		}

		#endregion

		#endregion

		#endregion

		#region Dates

		protected virtual void GenerateHeaderDtmSegments(DTMSegmentMessageSection dtmSection)
		{
			if (sgCusdec.HasOutwardTransport && !sgCusdec.DepartureDate.IsEmpty)
			{
				GenerateDtmSegment(dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansDepartureDateTimeActual_136, sgCusdec.DepartureDate);
			}

			if (sgCusdec.HasInwardTransport && !sgCusdec.ArrivalDate.IsEmpty)
			{
				GenerateDtmSegment(dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeActual, sgCusdec.ArrivalDate);
			}

			if (sgCusdec.DeclarationType == DeclarationTypeCodeList.Codes.BKT && !sgCusdec.StartDateOfBlanket.IsEmpty)
			{
				GenerateDtmSegment(dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList.StartDateTime, sgCusdec.StartDateOfBlanket);
			}
		}

		protected void GenerateDtmSegment(DTMSegmentMessageSection dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList qualifier, ZDate segmentDate)
		{
			if (!segmentDate.IsEmpty)
			{
				var dtm = dtmSection.InstantiateAChildAndAddItToChildrenCollection();
				dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = qualifier;
				dtm.DateTimePeriod.DateOrTimeOrPeriodText = segmentDate.ToString("yyyyMMdd");
				dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmdd;
			}
		}

		#endregion

		#region GEI

		void GenerateGEI(GEISegmentMessageSection geiSection)
		{
			var gei = geiSection.InstantiateAChildAndAddItToChildrenCollection();
			gei.ProcessingInformationCodeQualifier = ProcessingInformationCodeQualifierList.ConsignmentTypeInformation;
			gei.ProcessingIndicator.CodeListIdentificationCode = "Y";
		}

		#endregion

		#region Measurements

		protected virtual void GenerateHeaderMeaSegments(MEASegmentMessageSection meaSection)
		{
			GenerateTotalOuterPackSegment(meaSection);
			GenerateTotalGrossWeightSegment(meaSection);

			if (sgCusdec.HasOutwardTransport && sgCusdec.OutwardTransportCode == SGConstants.TransportCodes.Sea)
			{
				GenerateOutwardVesselNRT(meaSection);
			}
		}

		#region Measurement Segments

		void GenerateTotalOuterPackSegment(MEASegmentMessageSection meaSection)
		{
			GenerateMEASegment(meaSection, MeasurementPurposeCodeQualifierList.ExternalDimension, sgCusdec.TotalOuterPackUnitOfQty, sgCusdec.TotalOuterPack);
		}

		void GenerateTotalGrossWeightSegment(MEASegmentMessageSection meaSection)
		{
			var weightUnitInRequiredSGCValue =
				(
					((sgCusdec.IsInwardDeclaration || sgCusdec.IsTranshipmentDeclaration) && sgCusdec.InwardTransportCode == SGConstants.TransportCodes.Sea)
					|| (sgCusdec.IsOutwardDeclaration && sgCusdec.OutwardTransportCode == SGConstants.TransportCodes.Sea)
				) ?
				SGConstants.Weight.Tonnes :
				SGConstants.Weight.Kilograms;

			var (totalWeight, totalWeightUnit) = ConvertToSingaporeCustomsRequiredWeightUnit(sgCusdec.TotalGrossWeight, sgCusdec.TotalGrossWeightUnitOfQty, weightUnitInRequiredSGCValue);
			GenerateMEASegment(meaSection, MeasurementPurposeCodeQualifierList.DimensionsTotalWeight, totalWeightUnit, totalWeight);
		}

		void GenerateOutwardVesselNRT(MEASegmentMessageSection meaSection)
		{
			if (!sgCusdec.OutwardVesselNRT.IsEmpty)
			{
				GenerateMEASegment(meaSection, MeasurementPurposeCodeQualifierList.WeightOfConveyance, UnitOfQuantityCodeList.Codes.TNE, sgCusdec.OutwardVesselNRT);
			}
		}

		void GenerateMEASegment(MEASegmentMessageSection meaSection, MeasurementPurposeCodeQualifierList qualifier, string unitOfQty, decimal qtyValue)
		{
			var mea = meaSection.InstantiateAChildAndAddItToChildrenCollection();
			mea.MeasurementPurposeCodeQualifier = qualifier;
			mea.ValueRange.MeasurementUnitCode = unitOfQty;
			int decimalPlaces = (qualifier == MeasurementPurposeCodeQualifierList.WeightOfConveyance) ?
				SGConstants.NumericFormatting.TradeNet4Point1.DecimalPlacesForNetRegisteredTonnage :
				(qualifier == MeasurementPurposeCodeQualifierList.ExternalDimension) ?
				SGConstants.NumericFormatting.DecimalPlacesNone :
				SGConstants.NumericFormatting.TradeNet4Point1.DecimalPlacesForMeasurementValues;

			var qtyValueAsString = Utilities.FormatNumber(qtyValue, decimalPlaces);
			mea.ValueRange.Measure = qtyValueAsString;
		}

		#endregion

		#endregion

		#region Equipment and Seals

		protected virtual void GenerateEQDAndSELSegments(EQDSegmentMessageSection eqdSection, SELSegmentMessageSection selSection)
		{
			if (sgCusdec.Containers != null)
			{
				var containerSequence = sgCusdec.GetPreviousMessageContainerSequence();
				int i = containerSequence.HighestSequenceNumber;

				foreach (ICusContainer container in sgCusdec.Containers)
				{
					var numberSequence = containerSequence.GetSequenceNumber(container.ContainerNumber);
					if (numberSequence <= 0)
					{
						numberSequence = ++i;
					}

					GenerateEqdSegment(eqdSection, container, numberSequence);
					GenerateSelSegment(selSection, container);
				}
			}
		}

		void GenerateEqdSegment(EQDSegmentMessageSection eqdSection, ICusContainer container, int sequenceNumber)
		{
			var eqd = eqdSection.InstantiateAChildAndAddItToChildrenCollection();
			eqd.EquipmentTypeCodeQualifier = EquipmentTypeCodeQualifierList.Container;
			eqd.EquipmentIdentification.EquipmentIdentifier = container.ContainerNumber;
			eqd.EquipmentIdentification.CodeListIdentificationCode = (sequenceNumber).ToString();
			var (containerWeightInTonnes, _) = ConvertToSingaporeCustomsRequiredWeightUnit(container.ContainerWeight, container.ContainerWeightUnit, SGConstants.Weight.Tonnes);
			eqd.EquipmentSizeAndType.EquipmentSizeAndTypeDescriptionCode = EquipmentSizeAndTypeDescriptionCodeList.GetFromString(ConvertContainerWeightTo3CharString(containerWeightInTonnes));
			eqd.EquipmentSizeAndType.EquipmentSizeAndTypeDescription = container.ContainerType + container.ContainerSize.ToString();
		}

		void GenerateSelSegment(SELSegmentMessageSection selSection, ICusContainer container)
		{
			var sel = selSection.InstantiateAChildAndAddItToChildrenCollection();
			sel.TransportUnitSealIdentifier = container.SealNumber.IsEmpty ? "NA" : container.SealNumber.ToString();
		}

		#endregion

		#region FTX

		protected virtual void GenerateHeaderFtxSegment(FTXSegmentMessageSection ftxSection)
		{
			var traderRemarks = sgCusdec.TradersRemarksForMessage;
			if (traderRemarks != null && traderRemarks.Any())
			{
				var ftx = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
				ftx.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.GeneralInformation;
				ftx.TextLiteral.FreeText1 = traderRemarks.ElementAtOrDefault(0);
				ftx.TextLiteral.FreeText2 = traderRemarks.ElementAtOrDefault(1);
			}
		}

		#endregion

		#region Segment Group 1

		protected virtual void GenerateHeaderSegmentGroup1(SegmentGroup1MessageSection sg1Section)
		{
			GenerateMessageSenderMailboxSegment(sg1Section);
			GenerateCPCSegments(sg1Section);
		}

		#region Reference Segments

		/// <summary>
		/// MS = MessageSender
		/// </summary>
		protected void GenerateMessageSenderMailboxSegment(SegmentGroup1MessageSection sg1Section)
		{
			GenerateRFFSegment(sg1Section, ReferenceCodeQualifierList.MessageSender, sgCusdec.DeclarantId.Left(4) + "." + sgCusdec.DeclarantId);
		}

		#region For Subclasses

		/// <summary>
		/// DM = DocumentIdentifier
		/// </summary>
		protected virtual void PopulateLicensesAndDocumentsSegments(SegmentGroup1MessageSection sg1Section)
		{
			if (sgCusdec.LicencesAndDocuments != null)
			{
				foreach (ICusDocument licenceDoc in sgCusdec.LicencesAndDocuments)
				{
					GenerateRFFSegment(sg1Section, ReferenceCodeQualifierList.DocumentIdentifier, licenceDoc.LicenceNumber);
				}
			}
		}

		/// <summary>
		/// ACE = RelatedDocumentNumber
		/// </summary>
		protected virtual void PopulatePreviousPermitNumberSegment(SegmentGroup1MessageSection sg1Section)
		{
			if (!sgCusdec.PreviousPermitNumber.IsEmpty)
			{
				GenerateRFFSegment(sg1Section, ReferenceCodeQualifierList.RelatedDocumentNumber, sgCusdec.PreviousPermitNumber);
			}
		}

		/// <summary>
		/// ABT = GoodsDeclarationDocumentIdentifierCustoms
		/// </summary>
		protected void PopulatePermitNoToUpdateOrCancelSegment(SegmentGroup1MessageSection sg1Section)
		{
			if (!sgCusdec.PermitNoToUpdateOrCancel.IsEmpty)
			{
				GenerateRFFSegment(sg1Section, ReferenceCodeQualifierList.GoodsDeclarationDocumentIdentifierCustoms, sgCusdec.PermitNoToUpdateOrCancel);
			}
		}

		/// <summary>
		/// AAE = ReplacementPermitNumber
		/// </summary>
		/// <param name="sg1Section"></param>
		protected void PopulateReplacementPermitNoSegment(SegmentGroup1MessageSection sg1Section)
		{
			if (!sgCusdec.ReplacementPermitNumber.IsEmpty)
			{
				GenerateRFFSegment(sg1Section, ReferenceCodeQualifierList.GoodsDeclarationNumber, sgCusdec.ReplacementPermitNumber);
			}
		}

		/// <summary>
		/// TN
		/// </summary>
		protected virtual void PopulateSupplyIndicatorSegment(SegmentGroup1MessageSection sg1Section)
		{
			if (!sgCusdec.SupplyIndicator.IsEmpty)
			{
				GenerateRFFSegment(sg1Section, ReferenceCodeQualifierList.TransactionReferenceNumber, sgCusdec.SupplyIndicator);
			}
		}

		/// <summary>
		/// MR = MessageRecipient
		/// </summary>
		protected void PopulateAdditionalRecipientsSegments(SegmentGroup1MessageSection sg1Section)
		{
			if (sgCusdec.AdditionalRecipients != null)
			{
				foreach (var recipientId in sgCusdec.AdditionalRecipients)
				{
					GenerateRFFSegment(sg1Section, ReferenceCodeQualifierList.MessageRecipient, recipientId);
				}
			}
		}

		#endregion

		protected void GenerateRFFSegment(SegmentGroup1MessageSection sg1Section, ReferenceCodeQualifierList qualifier, ZString identifier)
		{
			var sg1 = sg1Section.InstantiateAChildAndAddItToChildrenCollection();
			var rff = sg1.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rff.Reference.ReferenceCodeQualifier = qualifier;
			rff.Reference.ReferenceIdentifier = identifier;
		}

		#endregion

		#region CPC Segments
		/// <summary>
		/// ABE = DeclarantsReferenceNumber - for use with new CPC codes 
		///		if CPC codes exist (when determined)
		///			create Group 1 (up to 5 occurrences)
		///				RFF with ReferenceCodeQualifierList.DeclarantsReferenceNumber
		///				foreach CPC
		///				Group 2
		///					PAC with ZZZ - Mutually defined
		///					Group 3
		///						PCI ZZZ - Mutually defined
		///						FTX	AAX - License information. plus processing code 1, 2 & 3 (up to 5 occurrences)
		/// </summary>
		protected virtual void GenerateCPCSegments(SegmentGroup1MessageSection sg1Section)
		{
			if (sgCusdec.CPCs != null)
			{
				foreach (ICusCPC cpc in sgCusdec.CPCs)
				{
					GenerateCPCDetails(sg1Section, cpc);
				}
			}
		}

		protected void GenerateCPCDetails(SegmentGroup1MessageSection sg1Section, ICusCPC cpc)
		{
			var sg1 = sg1Section.InstantiateAChildAndAddItToChildrenCollection();
			var rff = sg1.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rff.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.DeclarantsReferenceNumber;
			rff.Reference.ReferenceIdentifier = cpc.APCCodeName;
			GenerateCPCDetailsSG2(sg1.Group2, cpc);
		}

		protected void GenerateCPCDetailsSG2(SegmentGroup2MessageSection sg2Section, ICusCPC cpc)
		{
			var sg2 = GenerateSegmentGroup2(sg2Section);
			foreach (ICusProcessingCodes pcs in cpc.PCOccurrences)
			{
				if (pcs != null)
				{
					GenerateSegmentGroup3(sg2.Group3, pcs.ProcessingCode1, pcs.ProcessingCode2, pcs.ProcessingCode3);
				}
			}
		}

		#region Segment Group 2

		SegmentGroup2 GenerateSegmentGroup2(SegmentGroup2MessageSection sg2Section)
		{
			var sg2 = sg2Section.InstantiateAChildAndAddItToChildrenCollection();
			var pac = sg2.PAC.InstantiateAChildAndAddItToChildrenCollection();
			pac.ReturnablePackageDetails.ReturnablePackageFreightPaymentResponsibilityCode = ReturnablePackageFreightPaymentResponsibilityCodeList.MutuallyDefined;
			return sg2;
		}

		#endregion

		#region Segment Group 3

		void GenerateSegmentGroup3(SegmentGroup3MessageSection sg3Section, string processingCode1, string processingCode2, string processingCode3)
		{
			var sg3 = sg3Section.InstantiateAChildAndAddItToChildrenCollection();

			var pci = sg3.PCI.InstantiateAChildAndAddItToChildrenCollection();
			pci.MarkingInstructionsCode = MarkingInstructionsCodeList.MutuallyDefined;

			var ftx = sg3.FTX.InstantiateAChildAndAddItToChildrenCollection();
			ftx.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.LicenseInformation;
			ftx.TextLiteral.FreeText1 = processingCode1;
			ftx.TextLiteral.FreeText2 = processingCode2;
			ftx.TextLiteral.FreeText3 = processingCode3;
		}

		#endregion

		#endregion

		#endregion

		#region Segment Group 4

		protected virtual void GenerateSegmentGroup4(SegmentGroup4MessageSection sg4Section)
		{
		}

		/// <summary>
		/// Journey Identifier is used to declare the following:
		///		(1) for transport mode=1, specify for Voyage Number
		///								  specify for Towing Vessel ID
		///		(2) for transport mode=4, specify for Flight Number
		/// 
		/// Transport Identifier is used to declare the following:
		///		(1) for transport mode=1, specify Vessel Name / Towing Vessel Name
		///		(2) for transport mode=3, specify Vehicle Licence/Registration Number, if any
		///		(3) for transport mode=4, specify Aircraft Registration Number for chartered flights, if any
		/// 
		/// Note: InwardVoyageFlight & OutwardVoyageFlight will contain Vehicle Registration where appropriate for Road (transport mode=3) transport type.
		/// </summary>
		/// <param name="sg4Section"></param>
		protected void GenerateGroup4SegmentForInwardTransport(SegmentGroup4MessageSection sg4Section)
		{
			if (sgCusdec.HasInwardTransport)
			{
				var sg4 = sg4Section.InstantiateAChildAndAddItToChildrenCollection();
				GenerateTdtArrivalSegment(sg4, sgCusdec.InwardTransportCode.ToString(), sgCusdec.InwardJourneyIdentifier, sgCusdec.InwardTransportIdentifier);
			}
		}

		protected void GenerateGroup4SegmentsForOutwardTransport(SegmentGroup4MessageSection sg4Section)
		{
			if (sgCusdec.HasOutwardTransport)
			{
				// Group 4 Segment - Departure
				var sg4 = sg4Section.InstantiateAChildAndAddItToChildrenCollection();
				GenerateTdtDepartureSegment(sg4, sgCusdec.OutwardTransportCode.ToString(), sgCusdec.OutwardJourneyIdentifier, sgCusdec.OutwardTransportIdentifier, sgCusdec.OutwardVesselType);
				GenerateTplSegmentIfApplicable(sg4);

				// Group 4 Segment - Inland Waterway Transport
				GenerateGroup4SegmentForInlandWaterway(sg4Section);
			}
		}

		void GenerateGroup4SegmentForInlandWaterway(SegmentGroup4MessageSection sg4Section)
		{
			if (sgCusdec.OutwardTransportCode == SGConstants.TransportCodes.Sea && !sgCusdec.TowingVesselName.IsEmpty)
			{
				var sg4 = sg4Section.InstantiateAChildAndAddItToChildrenCollection();
				GenerateTdtInlandWaterwaySegment(sg4, sgCusdec.TowingVesselVoyageNo, sgCusdec.TowingVesselName);
			}
		}

		void GenerateTdtArrivalSegment(SegmentGroup4 sg4, string transportMode, string journeyIdentifier, string transportIdentifier)
		{
			var tdt = sg4.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tdt.TransportStageCodeQualifier = TransportStageCodeQualifierList.AtArrival;
			tdt.MeansOfTransportJourneyIdentifier = journeyIdentifier;
			tdt.ModeOfTransport.TransportModeNameCode = transportMode;
			tdt.TransportIdentification.TransportMeansIdentificationName = transportIdentifier;
		}

		void GenerateTdtDepartureSegment(SegmentGroup4 sg4, string transportMode, string journeyIdentifier, string transportIdentifier, string vesselType)
		{
			var tdt = sg4.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tdt.TransportStageCodeQualifier = TransportStageCodeQualifierList.AtDeparture;
			tdt.MeansOfTransportJourneyIdentifier = journeyIdentifier;
			tdt.ModeOfTransport.TransportModeNameCode = transportMode;
			tdt.TransportMeans.TransportMeansDescriptionCode = vesselType;
			tdt.TransportIdentification.TransportMeansIdentificationName = transportIdentifier;
		}

		void GenerateTdtInlandWaterwaySegment(SegmentGroup4 sg4, string voyage, string vesselName)
		{
			var tdt = sg4.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tdt.TransportStageCodeQualifier = TransportStageCodeQualifierList.InlandWaterwayTransport;
			tdt.MeansOfTransportJourneyIdentifier = (string.IsNullOrEmpty(voyage)) ? "NA" : voyage;
			tdt.TransportIdentification.TransportMeansIdentificationName = vesselName;
		}

		void GenerateTplSegmentIfApplicable(SegmentGroup4 sg4)
		{
			if (sgCusdec.OutwardTransportCode == SGConstants.TransportCodes.Sea && sgCusdec.IsSeaStoreDeclaration && !sgCusdec.OutwardVesselNationality.IsEmpty)
			{
				var tpl = sg4.TPL.InstantiateAChildAndAddItToChildrenCollection();
				tpl.TransportIdentification.TransportMeansNationalityCode = sgCusdec.OutwardVesselNationality;
			}
		}

		#endregion

		#region Segment Group 5

		protected virtual void GenerateSegmentGroup5(SegmentGroup5MessageSection sg5Section)
		{
			if ((sgCusdec.InwardTransportCode == SGConstants.TransportCodes.Sea || sgCusdec.InwardTransportCode == SGConstants.TransportCodes.Air))
			{
				PopulateInwardMasterBill(sg5Section);
			}

			if (sgCusdec.OutwardTransportCode == SGConstants.TransportCodes.Sea || sgCusdec.OutwardTransportCode == SGConstants.TransportCodes.Air)
			{
				PopulateOutwardMasterBill(sg5Section);
			}

			PopulateSupportingDocs(sg5Section);
		}

		protected void PopulateInwardMasterBill(SegmentGroup5MessageSection sg5Section)
		{
			if (!sgCusdec.InwardMasterBill.IsEmpty)
			{
				var sg5 = sg5Section.InstantiateAChildAndAddItToChildrenCollection();
				var doc = sg5.DOC.InstantiateAChildAndAddItToChildrenCollection();
				doc.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.MasterBillOfLading;
				doc.DocumentMessageDetails.DocumentIdentifier = sgCusdec.InwardMasterBill;
			}
		}

		protected void PopulateOutwardMasterBill(SegmentGroup5MessageSection sg5Section)
		{
			if (!sgCusdec.OutwardMasterBill.IsEmpty)
			{
				var sg5 = sg5Section.InstantiateAChildAndAddItToChildrenCollection();
				var doc = sg5.DOC.InstantiateAChildAndAddItToChildrenCollection();
				doc.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.MasterAirWaybill;
				doc.DocumentMessageDetails.DocumentIdentifier = sgCusdec.OutwardMasterBill;
			}
		}

		protected void PopulateSupportingDocs(SegmentGroup5MessageSection sg5Section)
		{
			if (sgCusdec.AdditionalMessageInformation != null && sgCusdec.AdditionalMessageInformation.SupportingDocuments != null)
			{
				foreach (ICusAttachment attachment in sgCusdec.AdditionalMessageInformation.SupportingDocuments)
				{
					var sg5 = sg5Section.InstantiateAChildAndAddItToChildrenCollection();
					var doc = sg5.DOC.InstantiateAChildAndAddItToChildrenCollection();
					doc.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.RelatedDocument;
					doc.DocumentMessageDetails.DocumentIdentifier = attachment.DocType;
					doc.DocumentMessageDetails.DocumentSourceDescription = attachment.FileName.ToASCII();
				}
			}
		}

		#endregion

		#region Segment Group 6

		protected virtual void GenerateHeaderSegmentGroup6(SegmentGroup6MessageSection sg6Section)
		{
			PopulateDeclaringAgentSegment(sg6Section);
			PopulateDeclarantSegments(sg6Section);
		}

		/// <summary>
		/// DT = Declarant
		/// </summary>
		protected void PopulateDeclarantSegments(SegmentGroup6MessageSection sg6Section)
		{
			var sg6 = sg6Section.InstantiateAChildAndAddItToChildrenCollection();

			var nad = sg6.NAD.InstantiateAChildAndAddItToChildrenCollection();
			nad.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.Declarant;

			var cta = sg6.CTA.InstantiateAChildAndAddItToChildrenCollection();
			cta.ContactFunctionCode = ContactFunctionCodeList.InformationContact;
			string declarantId = sgCusdec.Declarant.Code.IsEmpty ? sgCusdec.Declarant.Passport : sgCusdec.Declarant.Code;
			cta.ContactDetails.ContactIdentifier = declarantId.Replace("@", " ");
			cta.ContactDetails.ContactName = sgCusdec.Declarant.Name.Left(100);

			var com = sg6.COM.InstantiateAChildAndAddItToChildrenCollection();
			com.CommunicationContact.CommunicationAddressIdentifier = sgCusdec.Declarant.Phone;
			com.CommunicationContact.CommunicationMeansTypeCode = CommunicationMeansTypeCodeList.Telephone;
		}

		#region Related Parties

		/// <summary>
		/// BB = BuyerBankIdentification
		/// </summary>
		protected void PopulateBGIndicator(SegmentGroup6MessageSection sg6Section)
		{
			if (!sgCusdec.BGIndicator.IsEmpty)
			{
				var sg6 = sg6Section.InstantiateAChildAndAddItToChildrenCollection();

				var nad = sg6.NAD.InstantiateAChildAndAddItToChildrenCollection();
				nad.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.BuyerBankIdentification;

				var rff = sg6.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rff.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.DebitAccountNumber;
				rff.Reference.ReferenceIdentifier = sgCusdec.BGIndicator;
			}
		}

		#region Parties with Name and Identifier only

		/// <summary>
		/// AE = DeclarantsAgentRepresentative
		/// </summary>
		protected void PopulateDeclaringAgentSegment(SegmentGroup6MessageSection sg6Section)
		{
			GenerateNadSegmentPartyNameFormat1(sg6Section, PartyFunctionCodeQualifierList.DeclarantsAgentRepresentative, GlbCompany.CurrentCompany.GC_CustomsRegistrationNo, GlbCompany.CurrentCompany.GC_Name);
		}

		/// <summary>
		/// CG = CarriersAgent
		/// </summary>
		protected void PopulateInwardCarrierAgentSegment(SegmentGroup6MessageSection sg6Section)
		{
			if (sgCusdec.InwardCarrierAgent != null)
			{
				GenerateNadSegmentPartyNameFormat1(sg6Section, PartyFunctionCodeQualifierList.CarriersAgent, sgCusdec.InwardCarrierAgent.UEN, sgCusdec.InwardCarrierAgent.Name);
			}
		}

		/// <summary>
		/// CA = Carrier
		/// </summary>
		protected void PopulateOutwardCarrierAgentSegment(SegmentGroup6MessageSection sg6Section)
		{
			if (sgCusdec.OutwardCarrierAgent != null)
			{
				GenerateNadSegmentPartyNameFormat1(sg6Section, PartyFunctionCodeQualifierList.Carrier, sgCusdec.OutwardCarrierAgent.UEN, sgCusdec.OutwardCarrierAgent.Name);
			}
		}

		/// <summary>
		/// FW = FreightForwarder
		/// </summary>
		protected void PopulateForwarderSegment(SegmentGroup6MessageSection sg6Section)
		{
			if (sgCusdec.FreightForwarder != null)
			{
				GenerateNadSegmentPartyNameFormat1(sg6Section, PartyFunctionCodeQualifierList.FreightForwarder, sgCusdec.FreightForwarder.UEN, sgCusdec.FreightForwarder.Name);
			}
		}

		/// <summary>
		/// AH = Handling Agent
		/// </summary>
		protected void PopulateHandlingAgentSegment(SegmentGroup6MessageSection sg6Section)
		{
			if (sgCusdec.HandlingAgent != null)
			{
				GenerateNadSegmentPartyNameFormat1(sg6Section, PartyFunctionCodeQualifierList.TransitPrincipalsAgentRepresentative, sgCusdec.HandlingAgent.UEN, sgCusdec.HandlingAgent.Name);
			}
		}

		/// <summary>
		/// AE = DeclarantsAgentRepresentative
		/// CG = CarriersAgent
		/// CA = Carrier
		/// FW = FreightForwarder
		/// AH = Handling Agent
		/// </summary>
		void GenerateNadSegmentPartyNameFormat1(SegmentGroup6MessageSection sg6Section, PartyFunctionCodeQualifierList partyQualifier, string partyIdentifier, string partyName)
		{
			GenerateNadSegmentWithPartyInfoOnly(
				sg6Section, partyQualifier, partyIdentifier, partyName,
				PartyNameFormatCodeList.NameComponentsInSequenceAsDefinedInDescriptionBelow, 100);
		}

		/// <summary>
		/// IM = Importer
		/// </summary>
		protected void PopulateImporterSegment(SegmentGroup6MessageSection sg6Section)
		{
			if (sgCusdec.Importer != null)
			{
				GenerateNadSegmentWithPartyInfoOnly(
					sg6Section, PartyFunctionCodeQualifierList.Importer, sgCusdec.Importer.UEN, sgCusdec.Importer.Name,
					PartyNameFormatCodeList.NameComponentSequence2SequenceAsDefinedInDescription, 70);
			}
		}

		/// <summary>
		/// CC = Claimant
		/// </summary>
		protected void PopulateClaimantSegments(SegmentGroup6MessageSection sg6Section)
		{
			if (sgCusdec.Claimant != null)
			{
				var sg6 = sg6Section.InstantiateAChildAndAddItToChildrenCollection();

				var nad = sg6.NAD.InstantiateAChildAndAddItToChildrenCollection();
				nad.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.Claimant;
				nad.PartyIdentificationDetails.PartyIdentifier = sgCusdec.Claimant.UEN.Replace("@", " ");
				var splitter = new TextSplitElegantly(50, 2);
				splitter.Text = sgCusdec.Claimant.Name;
				nad.PartyName.PartyName1 = splitter[0];
				nad.PartyName.PartyName2 = splitter[1];
				nad.PartyName.PartyNameFormatCode = PartyNameFormatCodeList.NameComponentsInSequenceAsDefinedInDescriptionBelow;

				var cta = sg6.CTA.InstantiateAChildAndAddItToChildrenCollection();
				cta.ContactFunctionCode = ContactFunctionCodeList.InformationContact;
				cta.ContactDetails.ContactIdentifier = sgCusdec.ClaimantCode;
				cta.ContactDetails.ContactName = sgCusdec.ClaimantName;
			}
		}

		void GenerateNadSegmentWithPartyInfoOnly(SegmentGroup6MessageSection sg6Section, PartyFunctionCodeQualifierList partyQualifier, string partyIdentifier, string partyName, PartyNameFormatCodeList partyNameFormat, int nameLengh)
		{
			var nad = CreateNewSg6NadSegment(sg6Section);

			nad.PartyFunctionCodeQualifier = partyQualifier;
			nad.PartyIdentificationDetails.PartyIdentifier = partyIdentifier;

			var splitter = new TextSplitElegantly(nameLengh / 2, 2);
			splitter.Text = partyName;
			nad.PartyName.PartyName1 = splitter[0];
			nad.PartyName.PartyName2 = splitter[1];
			nad.PartyName.PartyNameFormatCode = partyNameFormat;
		}

		#endregion

		#region Parties with name, identifier and address info

		/// <summary>
		/// EX = Exporter
		/// </summary>
		protected void PopulateExporterSegment(SegmentGroup6MessageSection sg6Section, bool includeAddress)
		{
			if (sgCusdec.Exporter != null)
			{
				if (includeAddress)
				{
					GenerateNadSegmentWithAddressInfo(
						sg6Section, PartyFunctionCodeQualifierList.Exporter, sgCusdec.Exporter.UEN, sgCusdec.Exporter.Name,
						PartyNameFormatCodeList.NameComponentSequence2SequenceAsDefinedInDescription,
						sgCusdec.Exporter.Address, 70);
				}
				else
				{
					GenerateNadSegmentWithPartyInfoOnly(
						sg6Section, PartyFunctionCodeQualifierList.Exporter, sgCusdec.Exporter.UEN, sgCusdec.Exporter.Name,
						PartyNameFormatCodeList.NameComponentSequence2SequenceAsDefinedInDescription, 70);
				}
			}
		}

		/// <summary>
		/// CN = Consignee
		/// </summary>
		protected void PopulateConsigneeSegment(SegmentGroup6MessageSection sg6Section)
		{
			if (sgCusdec.Consignee != null)
			{
				GenerateNadSegmentWithAddressInfo(
					sg6Section, PartyFunctionCodeQualifierList.Consignee, "", sgCusdec.Consignee.Name,
					PartyNameFormatCodeList.NameComponentSequence2SequenceAsDefinedInDescription,
					sgCusdec.Consignee.Address, 70);
			}
		}

		/// <summary>
		/// MF = ManufacturerOfGoods
		/// </summary>
		//protected void PopulateManufacturerSegment(SegmentGroup6MessageSection sg6Section, PartyFunctionCodeQualifierList qualifier, string UEN, string ManufacturerName, string ManufacturerAddress)
		protected void PopulateManufacturerSegment(SegmentGroup6MessageSection sg6Section)
		{
			GenerateNadSegmentWithAddressInfo(
				sg6Section, PartyFunctionCodeQualifierList.ManufacturerOfGoods, sgCusdec.Manufacturer.UEN, sgCusdec.Manufacturer.Name,
				PartyNameFormatCodeList.NameComponentSequence2SequenceAsDefinedInDescription,
				sgCusdec.Manufacturer.Address, 70);
		}

		/// <summary>
		/// UC = UltimateConsignee
		/// </summary>
		protected void PopulateEndUserSegment(SegmentGroup6MessageSection sg6Section)
		{
			if (sgCusdec.EndUser != null)
			{
				GenerateNadSegmentWithAddressInfo(
					sg6Section, PartyFunctionCodeQualifierList.UltimateConsignee, "", sgCusdec.EndUser.Name,
					PartyNameFormatCodeList.NameComponentsInSequenceAsDefinedInDescriptionBelow,
					sgCusdec.EndUser.Address, 100);
			}
		}

		/// <summary>
		/// CN = Consignee
		/// EX = Exporter
		/// MF = ManufacturerOfGoods
		/// UC = UltimateConsignee
		/// </summary>
		void GenerateNadSegmentWithAddressInfo(SegmentGroup6MessageSection sg6Section, PartyFunctionCodeQualifierList carrierQualifier, string partyIdentifier, string partyName, PartyNameFormatCodeList partyNameFormat, IAddress address, int nameLength)
		{
			var nad = CreateNewSg6NadSegment(sg6Section);

			nad.PartyFunctionCodeQualifier = carrierQualifier;
			nad.PartyIdentificationDetails.PartyIdentifier = partyIdentifier;

			var splitter = new TextSplitElegantly(nameLength / 2, 2);
			splitter.Text = partyName;
			nad.PartyName.PartyName1 = splitter[0];
			nad.PartyName.PartyName2 = splitter[1];
			nad.PartyName.PartyNameFormatCode = partyNameFormat;

			if (!string.IsNullOrEmpty(address.FullAddress))
			{
				var specifyAddressInFirst3LinesOnly = SpecifyAddressInFirst3LinesOnly(carrierQualifier);
				splitter = new TextSplitElegantly(35, (specifyAddressInFirst3LinesOnly) ? 3 : 2);
				splitter.Text = address.FullAddress;
				nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = splitter[0];
				nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier2 = splitter[1];
				nad.CityName = (specifyAddressInFirst3LinesOnly) ? splitter[2] : address.City.ToString();

				if (!specifyAddressInFirst3LinesOnly)
				{
					nad.CountrySubdivisionDetails.CountrySubdivisionIdentifier = address.SubdivisionCode;
					nad.CountrySubdivisionDetails.CountrySubdivisionName = address.SubdivisionName;
					nad.PostalIdentificationCode = address.PostCode.Length < 10 ? address.PostCode : address.PostCode.KeepAlphanumericCharacters().SubstringSafe(0, 9);
				}
				nad.CountryIdentifier = address.CountryCode;
			}
		}

		protected virtual bool SpecifyAddressInFirst3LinesOnly(PartyFunctionCodeQualifierList carrierQualifier)
		{
			return false;
		}

		#endregion

		#endregion

		NADSegment CreateNewSg6NadSegment(SegmentGroup6MessageSection sg6Section)
		{
			var sg6 = sg6Section.InstantiateAChildAndAddItToChildrenCollection();
			return sg6.NAD.InstantiateAChildAndAddItToChildrenCollection();
		}

		#endregion

		#endregion

		#region Detail Section

		protected override void GenerateDetailSection(CUSDECMessage cusdecEdifactMsg)
		{
			GenerateDetailUnsSegment(cusdecEdifactMsg.UNS1);
			GenerateInvoiceOrCertificateOfOriginGroups(cusdecEdifactMsg.Group11);
			GenerateInvoiceLineDetailSegments(cusdecEdifactMsg.Group32);
		}

		void GenerateDetailUnsSegment(UNSSegmentMessageSection uns1Section)
		{
			var uns = uns1Section.InstantiateAChildAndAddItToChildrenCollection();
			uns.SectionIdentification = UnsDetail;
		}

		#region Segment Group 11..30 - Invoice Information

		protected virtual void GenerateInvoiceOrCertificateOfOriginGroups(SegmentGroup11MessageSection sg11Section)
		{
			if (sgCusdec.Invoices != null)
			{
				var invoiceIncludeInMsg = ZGuid.Empty;

				foreach (ICusInvoice invoice in sgCusdec.Invoices)
				{
					if (invoice.InvoicePK != invoiceIncludeInMsg)
					{
						var sg11 = sg11Section.InstantiateAChildAndAddItToChildrenCollection();
						var dms = sg11.DMS.InstantiateAChildAndAddItToChildrenCollection();
						dms.DocumentMessageIdentification.DocumentIdentifier = DmsInvoiceDetails;

						GenerateSegmentGroup12(sg11.Group12, invoice);
						GenerateSegmentGroup14(sg11.Group14, invoice);
						GenerateSegmentGroup15(sg11.Group15, invoice);
						GenerateSegmentGroups20And21(sg11.Group20, invoice);

						invoiceIncludeInMsg = invoice.InvoicePK;
					}
				}
			}
		}

		#region Invoice Segments

		void GenerateSegmentGroup12(SegmentGroup12MessageSection sg12Section, ICusInvoice invoice)
		{
			var sg12 = sg12Section.InstantiateAChildAndAddItToChildrenCollection();
			var moa = sg12.MOA.InstantiateAChildAndAddItToChildrenCollection();
			moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.InvoiceTotalAmount;
			moa.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(invoice.InvoiceTotalAmount, SGConstants.NumericFormatting.DecimalPlacesForAmountValues);
			moa.MonetaryAmount.CurrencyIdentificationCode = invoice.InvoiceCurrency;

			GenerateSegmentGroup13(sg12.Group13, invoice);
		}

		void GenerateSegmentGroup13(SegmentGroup13MessageSection sg13Section, ICusInvoice invoice)
		{
			if (invoice.InvoiceCurrency != Core.Constants.CurrencyCodes.Singapore && invoice.InvoiceCurrExchangeRate > 0)
			{
				var sg13 = sg13Section.InstantiateAChildAndAddItToChildrenCollection();
				var cux = sg13.CUX.InstantiateAChildAndAddItToChildrenCollection();
				cux.CurrencyExchangeRate = Utilities.FormatNumber(invoice.InvoiceCurrExchangeRate, SGConstants.NumericFormatting.DecimalPlacesForExchangeRateValues);
			}
		}

		void GenerateSegmentGroup14(SegmentGroup14MessageSection sg14Section, ICusInvoice invoice)
		{
			if (!invoice.IncoTerm.IsEmpty)
			{
				var sg14 = sg14Section.InstantiateAChildAndAddItToChildrenCollection();
				var tod = sg14.TOD.InstantiateAChildAndAddItToChildrenCollection();
				tod.TermsOfDeliveryOrTransport.DeliveryOrTransportTermsDescriptionCode = DeliveryOrTransportTermsDescriptionCodeList.GetFromString(invoice.IncoTerm);
			}
		}

		void GenerateSegmentGroup15(SegmentGroup15MessageSection sg15Section, ICusInvoice invoice)
		{
			var sg15 = sg15Section.InstantiateAChildAndAddItToChildrenCollection();
			var nad = sg15.NAD.InstantiateAChildAndAddItToChildrenCollection();

			if (invoice.Supplier != null)
			{
				nad.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.Supplier;
				nad.NameAndAddress.NameAndAddressDescription1 = invoice.Supplier.UEN;
				TextSplitElegantly splitter = new TextSplitElegantly(50, 2);
				splitter.Text = invoice.Supplier.Name;
				nad.PartyName.PartyName1 = splitter[0];
				nad.PartyName.PartyName2 = splitter[1];
			}
			else
			{
				nad.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.Seller;
			}

			GenerateSegmentGroup16(sg15.Group16, invoice);
		}

		void GenerateSegmentGroup16(SegmentGroup16MessageSection sg16Section, ICusInvoice invoice)
		{
			if (!invoice.InvoiceNumber.IsEmpty || !invoice.InvoiceDate.IsEmpty)
			{
				var sg16 = sg16Section.InstantiateAChildAndAddItToChildrenCollection();

				if (!invoice.InvoiceNumber.IsEmpty)
				{
					var doc = sg16.DOC.InstantiateAChildAndAddItToChildrenCollection();
					doc.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.CommercialInvoice;
					doc.DocumentMessageDetails.DocumentIdentifier = invoice.InvoiceNumber;
				}

				if (!invoice.InvoiceDate.IsEmpty)
				{
					var dtm = sg16.DTM.InstantiateAChildAndAddItToChildrenCollection();
					dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.InvoiceDocumentIssueDateTime;
					dtm.DateTimePeriod.DateOrTimeOrPeriodText = invoice.InvoiceDate.ToString("yyyyMMdd");
					dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmdd;
				}
			}
		}

		void GenerateSegmentGroups20And21(SegmentGroup20MessageSection sg20Section, ICusInvoice invoice)
		{
			if (invoice.FreightCharge != null && invoice.FreightCharge.Amount > 0)
			{
				GenerateChargeSegments(sg20Section, MonetaryAmountTypeCodeQualifierList.FreightCharge, invoice.FreightCharge);
			}

			if (invoice.InsuranceCharge != null && invoice.InsuranceCharge.Amount > 0)
			{
				GenerateChargeSegments(sg20Section, MonetaryAmountTypeCodeQualifierList.ConsignmentToImportationLocationInsuranceAmount, invoice.InsuranceCharge);
			}

			if (invoice.OtherCharge != null && invoice.OtherCharge.Amount > 0)
			{
				GenerateChargeSegments(sg20Section, MonetaryAmountTypeCodeQualifierList.OtherCharges, invoice.OtherCharge);
			}
		}

		void GenerateChargeSegments(SegmentGroup20MessageSection sg20Section, MonetaryAmountTypeCodeQualifierList chargeQualifier, ICusCharge charge)
		{
			var sg20 = sg20Section.InstantiateAChildAndAddItToChildrenCollection();
			var alc = sg20.ALC.InstantiateAChildAndAddItToChildrenCollection();
			alc.AllowanceOrChargeCodeQualifier = AllowanceOrChargeCodeQualifierList.Charge;

			var moa = sg20.MOA.InstantiateAChildAndAddItToChildrenCollection();
			moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier = chargeQualifier;
			decimal monetaryAmount = charge.Amount;
			moa.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(monetaryAmount, SGConstants.NumericFormatting.DecimalPlacesForAmountValues);
			moa.MonetaryAmount.CurrencyIdentificationCode = charge.CurrencyCode;

			if (charge.Percentage > 0m)
			{
				var pdc = sg20.PCD.InstantiateAChildAndAddItToChildrenCollection();
				pdc.PercentageDetails.PercentageTypeCodeQualifier = PercentageTypeCodeQualifierList.EntryPercentage;
				decimal chargePercentage = charge.Percentage;
				pdc.PercentageDetails.Percentage = Utilities.FormatNumber(chargePercentage, SGConstants.NumericFormatting.DecimalPlacesForPercentageValues);
			}

			if (charge.CurrencyCode != Core.Constants.CurrencyCodes.Singapore)
			{
				var sg21 = sg20.Group21.InstantiateAChildAndAddItToChildrenCollection();
				var cux = sg21.CUX.InstantiateAChildAndAddItToChildrenCollection();
				decimal exchangeRate = charge.ExchangeRate;
				cux.CurrencyExchangeRate = Utilities.FormatNumber(exchangeRate, SGConstants.NumericFormatting.DecimalPlacesForExchangeRateValues);
			}
		}

		#endregion

		#endregion

		#region Segment Group 32..50 - Invoice Lines

		protected virtual void GenerateInvoiceLineDetailSegments(SegmentGroup32MessageSection sg32Section)
		{
			IEnumerable<ICusItem> items = GetLineItemCollection();

			if (items != null)
			{
				foreach (ICusItem item in items)
				{
					if (!PartialRefundWithNoRefundThisItem(item))
					{
						var sg32 = sg32Section.InstantiateAChildAndAddItToChildrenCollection();

						GenerateSegmentGroup32(sg32, item);
						GenerateSegmentGroups33And34(sg32.Group33, item);
						GenerateSegmentGroups35And36(sg32.Group35, item);
						GenerateSegmentGroups37And38(sg32.Group37, item);
						GenerateSegmentGroup39(sg32.Group39, item);
						GenerateSegmentGroup41(sg32.Group41, item);
						GenerateSegmentGroup42(sg32.Group42, item);
						GenerateSegmentGroup43(sg32.Group43, item);
					}
				}
			}
		}

		bool PartialRefundWithNoRefundThisItem(ICusItem item)
		{
			bool partialRefundNotForThisLine = false;
			if (sgCusdec.AdditionalMessageInformation.UpdateIndicator == UpdateIndicatorCodeList.Codes.PRS)
			{
				partialRefundNotForThisLine = (item.ItemDutyRefund == 0 && item.ItemExciseRefund == 0 && item.ItemGSTRefund == 0);
			}

			return partialRefundNotForThisLine;
		}

		protected virtual IEnumerable<ICusItem> GetLineItemCollection()
		{
			return sgCusdec.Items;
		}

		#region Group 32

		public virtual void GenerateSegmentGroup32(SegmentGroup32 sg32, ICusItem item)
		{
			var cst = sg32.CST.InstantiateAChildAndAddItToChildrenCollection();
			cst.GoodsItemNumber = item.SerialNumber;
			cst.CustomsIdentityCodes1.CustomsGoodsIdentifier = item.HSCode;
			SpecifyCstTextileCategoryCode(cst, item);

			GenerateGroup32Ftx(sg32, item);
			GenerateGroup32Loc(sg32, item);
			GenerateGroup32Dtm(sg32, item);
			GenerateGroup32Mea(sg32, item);
		}

		protected virtual void SpecifyCstTextileCategoryCode(CSTSegment cst, ICusItem item)
		{
		}

		#region Free Text

		protected virtual void GenerateGroup32Ftx(SegmentGroup32 sg32, ICusItem item)
		{
			GenerateInvoiceItemDescriptionSegment(sg32, item);
			GenerateBrandAndModelSegment(sg32, item);
			GenerateDangerousGoodsSegment(sg32, item);
		}

		protected virtual void GenerateInvoiceItemDescriptionSegment(SegmentGroup32 sg32, ICusItem item)
		{
			GenerateGroup32FtxSegment(sg32, TextSubjectCodeQualifierList.GoodsItemDescription, item.GoodsDescription.Replace("\r\n", " "), "");
		}

		void GenerateDangerousGoodsSegment(SegmentGroup32 sg32, ICusItem item)
		{
			GenerateGroup32FtxSegment(sg32, TextSubjectCodeQualifierList.DangerousGoodsAdditionalInformation, item.DGIndicator, "");
		}

		void GenerateBrandAndModelSegment(SegmentGroup32 sg32, ICusItem item)
		{
			string brandName = sgCusdec.IsImport && item.BrandName.IsEmpty ? (ZString)SGConstants.Unbranded : item.BrandName;
			GenerateGroup32FtxSegment(sg32, TextSubjectCodeQualifierList.ProductInformation, brandName, item.ModelDescription);
		}

		void GenerateGroup32FtxSegment(SegmentGroup32 sg32, TextSubjectCodeQualifierList subjectQualifier, string freeText1, string freeText2)
		{
			if (!string.IsNullOrEmpty(freeText1) || !string.IsNullOrEmpty(freeText2))
			{
				var ftx = sg32.FTX.InstantiateAChildAndAddItToChildrenCollection();
				ftx.TextSubjectCodeQualifier = subjectQualifier;
				ftx.TextLiteral.FreeText1 = freeText1;
				ftx.TextLiteral.FreeText2 = freeText2;
			}
		}

		#endregion

		#region Locations

		protected virtual void GenerateGroup32Loc(SegmentGroup32 sg32, ICusItem item)
		{
			GenerateCountryOfOriginSegment(sg32, item);
			GenerateLotNumberSegments(sg32, item);
		}

		protected virtual void GenerateCountryOfOriginSegment(SegmentGroup32 sg32, ICusItem item)
		{
			GenerateGroup32LocSegment(sg32, LocationFunctionCodeQualifierList.CountryOfOrigin, item.CountryOfOriginCode);
		}

		protected virtual void GenerateLotNumberSegments(SegmentGroup32 sg32, ICusItem item)
		{
			GenerateGroup32LocSegment(sg32, LocationFunctionCodeQualifierList.Warehouse, item.CurrentLotNumber);
			GenerateGroup32LocSegment(sg32, LocationFunctionCodeQualifierList.BondedWarehouse, item.PreviousLotNumber);
		}

		void GenerateGroup32LocSegment(SegmentGroup32 sg32, LocationFunctionCodeQualifierList functionQualifier, ZString locationCode)
		{
			if (!locationCode.IsEmpty)
			{
				var loc = sg32.LOC.InstantiateAChildAndAddItToChildrenCollection();
				loc.LocationFunctionCodeQualifier = functionQualifier;
				loc.LocationIdentification.LocationIdentifier = locationCode;
			}
		}

		#endregion

		#region Dates

		protected virtual void GenerateGroup32Dtm(SegmentGroup32 sg32, ICusItem item)
		{
		}

		#endregion

		#region Line Measurements

		protected virtual void GenerateGroup32Mea(SegmentGroup32 sg32, ICusItem item)
		{
			GenerateItemMeasurementSegments(sg32, item);
		}

		void GenerateItemMeasurementSegments(SegmentGroup32 sg32, ICusItem item)
		{
			GenerateHSQuantitySegment(sg32, item);
			GenerateTotalDutiableSegment(sg32, item);
			GeneratePercentageOfAlcoholSegment(sg32, item);
			GenerateDutiableQtyWgtVolSegment(sg32, item);
			GenerateCASCProductQtySegment(sg32, item);
		}

		void GenerateHSQuantitySegment(SegmentGroup32 sg32, ICusItem item)
		{
			if (item.HSQuantity > 0)
			{
				var unitOfQty = (item.HSQuantityUnitType == "VAL" || item.HSQuantityUnitType.IsEmpty) ? item.InvoiceUQ : item.HSQuantityUnitType;
				GenerateLineMeaSegment(sg32, MeasurementPurposeCodeQualifierList.CustomsLineItemMeasurement, unitOfQty, item.HSQuantity);
			}
		}

		protected void GenerateTotalDutiableSegment(SegmentGroup32 sg32, ICusItem item)
		{
			if (item.TotalDutiableQuantity > 0 || item.UnitDutiableQuantity > 0)
			{
				GenerateTotalDutiableSegment(sg32, item, false);
			}
		}

		protected void GenerateTotalDutiableSegment(SegmentGroup32 sg32, ICusItem item, bool isOneLot)
		{
			if (item.DutyUnitRate > 0m || item.ExciseUnitRate > 0m)
			{
				GenerateLineMeaSegment(sg32, MeasurementPurposeCodeQualifierList.Measurement, item.TotalDutiableQuantityUnitType, item.TotalDutiableQuantity, isOneLot);
			}
			else
			{
				GenerateLineMeaSegment(sg32, MeasurementPurposeCodeQualifierList.Measurement, item.UnitDutiableQuantityUnitType, item.UnitDutiableQuantity, isOneLot);
			}
		}

		void GeneratePercentageOfAlcoholSegment(SegmentGroup32 sg32, ICusItem item)
		{
			if (item.PercentageOfAlcohol > 0m)
			{
				GenerateLineMeaSegment(sg32, MeasurementPurposeCodeQualifierList.AlcoholContent, "PER", item.PercentageOfAlcohol, SGConstants.NumericFormatting.DecimalPlacesForPercentageValues, false);
			}
		}

		void GenerateDutiableQtyWgtVolSegment(SegmentGroup32 sg32, ICusItem item)
		{
			if (item.DutyUnitRate > 0m || item.ExciseUnitRate > 0m || item.DutyPercentageRate > 0 || item.ExcisePercentageRate > 0)
			{
				GenerateLineMeaSegment(sg32, MeasurementPurposeCodeQualifierList.ItemWeight, item.UnitDutiableQuantityUnitType, item.UnitDutiableQuantity);
			}
		}

		void GenerateCASCProductQtySegment(SegmentGroup32 sg32, ICusItem item)
		{
			if (item.ProductCodes != null)
			{
				foreach (var productCode in item.ProductCodes)
				{
					if (productCode.ProductCodeQty > 0)
					{
						GenerateLineMeaSegment(
							sg32, MeasurementPurposeCodeQualifierList.UnitOfMeasureUsedForOrderedQuantities,
							productCode.ProductCodeUnitType, productCode.ProductCodeQty);
					}
				}
			}
		}

		protected void GenerateLineMeaSegment(SegmentGroup32 sg32, MeasurementPurposeCodeQualifierList purposeQualifier, string unitOfQty, decimal qtyValue)
		{
			GenerateLineMeaSegment(sg32, purposeQualifier, unitOfQty, qtyValue, false);
		}

		protected void GenerateLineMeaSegment(SegmentGroup32 sg32, MeasurementPurposeCodeQualifierList purposeQualifier, string unitOfQty, decimal qtyValue, bool oneLot)
		{
			GenerateLineMeaSegment(sg32, purposeQualifier, unitOfQty, qtyValue, SGConstants.NumericFormatting.DecimalPlacesForMeasurementValues, oneLot);
		}

		protected void GenerateLineMeaSegment(SegmentGroup32 sg32, MeasurementPurposeCodeQualifierList purposeQualifier, string unitOfQty, decimal qtyValue, int decimals, bool oneLot)
		{
			if (qtyValue > 0)
			{
				var mea = sg32.MEA.InstantiateAChildAndAddItToChildrenCollection();
				mea.MeasurementPurposeCodeQualifier = purposeQualifier;
				mea.ValueRange.MeasurementUnitCode = unitOfQty;
				mea.ValueRange.Measure = (oneLot) ? SGConstants.OneLot : Utilities.FormatNumber(qtyValue, decimals);
			}
		}

		#endregion

		#endregion

		#region Segment Groups 33 & 34

		protected virtual void GenerateSegmentGroups33And34(SegmentGroup33MessageSection sg33Section, ICusItem item)
		{
			GeneratePackageDescriptionSegments(sg33Section, item);

			if (ShouldSpecifyProductMarking(item) && sg33Section.Count == 0)
			{
				var sg33 = sg33Section.InstantiateAChildAndAddItToChildrenCollection();
				var pac = sg33.PAC.InstantiateAChildAndAddItToChildrenCollection();
				pac.PackagingDetails.PackagingRelatedDescriptionCode = PackagingRelatedDescriptionCodeList.ProductMarking;
			}

			if (sg33Section.Count > 0)
			{
				GenerateSegmentGroup34(sg33Section[sg33Section.Count - 1].Group34, item);
			}
		}

		/// <summary>
		/// Specify qualifier code “34” for DE7233 if Marks and Numbers is to be declared but the packing description is not required
		/// For CO application, specify qualifier code “34” for DE7233 if Certificate item description is to be declared.
		/// </summary>
		protected virtual bool ShouldSpecifyProductMarking(ICusItem item)
		{
			return !item.MarksAndNumbers.IsEmpty;
		}

		void GeneratePackageDescriptionSegments(SegmentGroup33MessageSection sg33Section, ICusItem item)
		{
			if ((item.IsLiquor || item.IsTobacco)
				|| (item.PackOuterQuantity > 0 || item.PackInQuantity > 0 || item.PackInnerQuantity > 0 || item.PackInmostQuantity > 0))
			{
				//must be in this order
				if (item.PackOuterQuantity > 0)
				{
					GenerateLinePacSegment(sg33Section, PackagingLevelCodeList.Outer, item.PackOuterUnitType, item.PackOuterQuantity);
				}

				if (item.PackInQuantity > 0)
				{
					GenerateLinePacSegment(sg33Section, PackagingLevelCodeList.Intermediate, item.PackInUnitType, item.PackInQuantity);
				}

				if (item.PackInnerQuantity > 0)
				{
					GenerateLinePacSegment(sg33Section, PackagingLevelCodeList.Inner, item.PackInnerUnitType, item.PackInnerQuantity);
				}

				if (item.PackInmostQuantity > 0)
				{
					GenerateLinePacSegment(sg33Section, PackagingLevelCodeList.ShipmentLevel, item.PackInmostUnitType, item.PackInmostQuantity);
				}
			}
		}

		void GenerateLinePacSegment(SegmentGroup33MessageSection sg33Section, PackagingLevelCodeList levelQualifier, string unitOfQty, decimal qtyValue)
		{
			var sg33 = sg33Section.InstantiateAChildAndAddItToChildrenCollection();
			var pac = sg33.PAC.InstantiateAChildAndAddItToChildrenCollection();
			pac.PackagingDetails.PackagingLevelCode = levelQualifier;
			pac.PackageQuantity = qtyValue.ToString();
			pac.PackageType.PackageTypeDescriptionCode = unitOfQty;
		}

		#region Segment Group 34

		protected virtual void GenerateSegmentGroup34(SegmentGroup34MessageSection sg34Section, ICusItem item)
		{
			GenerateGroup34LegalRequirements(sg34Section, item);
			GenerateGroup34MarksAndNumbers(sg34Section, item);
		}

		void GenerateGroup34LegalRequirements(SegmentGroup34MessageSection sg34Section, ICusItem item)
		{
			if (!item.E_SDNPIndicator.IsEmpty)
			{
				var sg34 = sg34Section.InstantiateAChildAndAddItToChildrenCollection();
				var pci = sg34.PCI.InstantiateAChildAndAddItToChildrenCollection();
				pci.MarkingInstructionsCode = MarkingInstructionsCodeList.LegalRequirements;
				pci.MarksLabels.ShippingMarksDescription1 = item.E_SDNPIndicator;
			}
		}

		/// <summary>
		///  Specify markings on cargo for marks & numbers, if any.
		///  a) Repeat at most 4 times and specify:
		///    1st occurrence : 10 lines x 17 = 170 chars
		///    2nd occurrence : 10 lines x 17 = 170 chars
		///    3rd occurrence : 8 lines x 17 = 136 chars
		///    4th occurrence : 3 lines x 12 = 36 chars
		/// </summary>
		void GenerateGroup34MarksAndNumbers(SegmentGroup34MessageSection sg34Section, ICusItem item)
		{
			if (!item.MarksAndNumbers.IsEmpty)
			{
				var sg34 = sg34Section.InstantiateAChildAndAddItToChildrenCollection();

				CreateMarksAndNumbersPciSegment(sg34, item.MarksAndNumbers.Left(170), 17, 10);

				if (item.MarksAndNumbers.Length > 170)
				{
					CreateMarksAndNumbersPciSegment(sg34, item.MarksAndNumbers.SubstringSafe(170, 170), 17, 10);
				}

				if (item.MarksAndNumbers.Length > 340)
				{
					CreateMarksAndNumbersPciSegment(sg34, item.MarksAndNumbers.SubstringSafe(340, 136), 17, 8);
				}

				if (item.MarksAndNumbers.Length > 476)
				{
					CreateMarksAndNumbersPciSegment(sg34, item.MarksAndNumbers.SubstringSafe(476, 36), 12, 3);
				}
			}
		}

		/// <summary>
		/// Populate marks based on the number of lines
		/// </summary>
		/// <param name="numOfLines">Domain = {3, 8, 10}</param>
		void CreateMarksAndNumbersPciSegment(SegmentGroup34 sg34, string marksText, int lineSize, int numOfLines)
		{
			var pci = sg34.PCI.InstantiateAChildAndAddItToChildrenCollection();
			pci.MarkingInstructionsCode = MarkingInstructionsCodeList.MarkFreeText;
			var splitter = new TextSplitElegantly(lineSize, numOfLines);
			splitter.Text = marksText;

			pci.MarksLabels.ShippingMarksDescription1 = splitter[0];
			pci.MarksLabels.ShippingMarksDescription2 = splitter[1];
			pci.MarksLabels.ShippingMarksDescription3 = splitter[2];

			if (numOfLines > 3)
			{
				pci.MarksLabels.ShippingMarksDescription4 = splitter[3];
				pci.MarksLabels.ShippingMarksDescription5 = splitter[4];
				pci.MarksLabels.ShippingMarksDescription6 = splitter[5];
				pci.MarksLabels.ShippingMarksDescription7 = splitter[6];
				pci.MarksLabels.ShippingMarksDescription8 = splitter[7];

				if (numOfLines > 8)
				{
					pci.MarksLabels.ShippingMarksDescription9 = splitter[8];
					pci.MarksLabels.ShippingMarksDescription10 = splitter[9];
				}
			}
		}

		#endregion

		#endregion

		#region Segment Groups 35 & 36

		protected virtual void GenerateSegmentGroups35And36(SegmentGroup35MessageSection sg35Section, ICusItem item)
		{
			GenerateCifFobSegment(sg35Section, item);
			GenerateUnitPriceSegment(sg35Section, item);
			GenerateLastSellingPriceSegment(sg35Section, item);
			GenerateOtherCharges(sg35Section, item);
		}

		protected void GenerateCifFobSegment(SegmentGroup35MessageSection sg35Section, ICusItem item)
		{
			var sg35 = sg35Section.InstantiateAChildAndAddItToChildrenCollection();
			GenerateLineMoaSegment(sg35, MonetaryAmountTypeCodeQualifierList.FobValue, Core.Constants.CurrencyCodes.Singapore, item.CustomsValue);
		}

		protected virtual void GenerateUnitPriceSegment(SegmentGroup35MessageSection sg35Section, ICusItem item)
		{
			if (item.IsMotorVehicle && (item.DutyAmount > 0 || item.ExciseAmount > 0)) // only required for dutiable motor vehicles (WI00033330)
			{
				if (item.UnitPrice > 0)
				{
					var sg35 = sg35Section.InstantiateAChildAndAddItToChildrenCollection();
					GenerateLineMoaSegment(sg35, MonetaryAmountTypeCodeQualifierList.UnitPrice, item.InvoiceCurrency, item.UnitPrice);
					var sg36 = sg35.Group36.InstantiateAChildAndAddItToChildrenCollection();
					GenerateLineExchangeRateSegment(sg36, item.InvoiceCurrExchangeRate);
				}
			}
		}

		void GenerateLastSellingPriceSegment(SegmentGroup35MessageSection sg35Section, ICusItem item)
		{
			if (item.LSPValue > 0)
			{
				var sg35 = sg35Section.InstantiateAChildAndAddItToChildrenCollection();
				GenerateLineMoaSegment(sg35, MonetaryAmountTypeCodeQualifierList.AssignedCustomsValue, Core.Constants.CurrencyCodes.Singapore, item.LSPValue);
			}
		}

		void GenerateOtherCharges(SegmentGroup35MessageSection sg35Section, ICusItem item)
		{
			if (item.OptionalItemCharge != null)
			{
				if (item.OptionalItemCharge.Amount > 0)
				{
					var sg35 = sg35Section.InstantiateAChildAndAddItToChildrenCollection();
					GenerateLineMoaSegment(sg35, MonetaryAmountTypeCodeQualifierList.OtherCharges, item.OptionalItemCharge.CurrencyCode, item.OptionalItemCharge.Amount);
					var sg36 = sg35.Group36.InstantiateAChildAndAddItToChildrenCollection();
					GenerateLineExchangeRateSegment(sg36, item.OptionalItemCharge.ExchangeRate);
				}
			}
		}

		void GenerateLineMoaSegment(SegmentGroup35 sg35, MonetaryAmountTypeCodeQualifierList qualifier, ZString currencyCode, ZDecimal amount)
		{
			var moa = sg35.MOA.InstantiateAChildAndAddItToChildrenCollection();
			moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier = qualifier;

			int decimalPlaces = qualifier == MonetaryAmountTypeCodeQualifierList.UnitPrice ? SGConstants.NumericFormatting.DecimalPlacesForUnitPriceAmountValues : SGConstants.NumericFormatting.DecimalPlacesForAmountValues;

			moa.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(amount, decimalPlaces);
			if (qualifier == MonetaryAmountTypeCodeQualifierList.UnitPrice || qualifier == MonetaryAmountTypeCodeQualifierList.OtherCharges)
			{
				moa.MonetaryAmount.CurrencyIdentificationCode = currencyCode;
			}
		}

		void GenerateLineExchangeRateSegment(SegmentGroup36 sg36, ZDecimal exchangeRate)
		{
			if (exchangeRate > 0 && exchangeRate != 1m)
			{
				var cux = sg36.CUX.InstantiateAChildAndAddItToChildrenCollection();
				cux.CurrencyExchangeRate = Utilities.FormatNumber(exchangeRate, SGConstants.NumericFormatting.DecimalPlacesForExchangeRateValues);
			}
		}

		#endregion

		#region Segment Groups 37 & 38

		protected virtual void GenerateSegmentGroups37And38(SegmentGroup37MessageSection sg37Section, ICusItem item)
		{
			if (SupportsInvoiceNumberSegment)
			{
				GenerateInvoiceNoSegment(sg37Section, item);
			}

			SegmentGroup37 sg37 = null;
			bool ginSegmentAdded = false;

			// Product Code + GIN
			foreach (ICusProductCode productCode in item.ProductCodes)
			{
				sg37 = sg37Section.InstantiateAChildAndAddItToChildrenCollection();
				GenerateProductCodeSegment(sg37, productCode.ProductCode);

				//first occurance only currently
				if (!ginSegmentAdded)
				{
					GenerateGoodsIdentityNumberSegment(sg37, item);
					ginSegmentAdded = true;
				}
			}

			// GIN where no product code
			if (sg37 == null && item.CASCCodes1 != null && item.CASCCodes1.Count > 0)
			{
				sg37 = sg37Section.InstantiateAChildAndAddItToChildrenCollection();

				if (sg37.RFF.Count == 0) //Note 3
				{
					RFFSegment rFF = sg37.RFF.InstantiateAChildAndAddItToChildrenCollection();
					rFF.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.GoodsItemInformation;
				}

				GenerateGoodsIdentityNumberSegment(sg37, item);
			}

			// Motor Vehicle specific
			if (SupportsSeSegment && item.IsMotorVehicle && item.EngineCapacity > 0)
			{
				var sg37EngineCapacity = sg37Section.InstantiateAChildAndAddItToChildrenCollection();
				RFFSegment rFF = sg37EngineCapacity.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rFF.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.SerialNumber;

				if (SupportsRegistrationDateSegment)
				{
					GenerateRegistrationDateSegment(sg37EngineCapacity, item.DateOfFirstRegistration);
				}

				GenerateGroup38SegmentForMotorVehicles(sg37EngineCapacity.Group38, item);
			}

			if (SupportsVehicleRego && !item.RegistrationNumberSG.IsEmpty)
			{
				var sg37RegoNo = sg37Section.InstantiateAChildAndAddItToChildrenCollection();
				GenerateMotorVehicleRegistrationSegment(sg37RegoNo, item.RegistrationNumberSG);
			}

			//Strategic Goods
			if (SupportsStrategicGoodsSegments && item.IsStrategic)
			{
				AddStrategicDetails(sg37Section, item);
			}
		}

		protected virtual bool SupportsInvoiceNumberSegment { get { return false; } }
		protected virtual bool SupportsRegistrationDateSegment { get { return false; } }
		protected virtual bool SupportsVehicleRego { get { return false; } }
		protected virtual bool SupportsSeSegment { get { return true; } }
		protected virtual bool SupportsSeastoresSegments { get { return true; } }
		protected virtual bool SupportsStrategicGoodsSegments { get { return false; } }

		protected void GenerateInvoiceNoSegment(SegmentGroup37MessageSection sg37Section, ICusItem item)
		{
			if (!item.InvoiceNumber.IsEmpty)
			{
				var sg37 = sg37Section.InstantiateAChildAndAddItToChildrenCollection();
				GenerateGroup37RffSegment(sg37, ReferenceCodeQualifierList.InvoiceDocumentIdentifier, item.InvoiceNumber);
			}
		}

		protected void GenerateProductCodeSegment(SegmentGroup37 sg37, string productCode)
		{
			GenerateGroup37RffSegment(sg37, ReferenceCodeQualifierList.GovernmentAgencyReferenceNumber, productCode);
		}

		protected void GenerateSerialNoSegment(SegmentGroup37 sg37)
		{
			RFFSegment rFF = sg37.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rFF.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.SerialNumber;
		}

		protected virtual void GenerateMotorVehicleRegistrationSegment(SegmentGroup37 sg37, string regoNo)
		{
			GenerateGroup37RffSegment(sg37, ReferenceCodeQualifierList.MotorVehicleIdentificationNumber, regoNo);
		}

		protected void AddStrategicDetails(SegmentGroup37MessageSection sg37Section, ICusItem item)
		{
			var sg37 = sg37Section.InstantiateAChildAndAddItToChildrenCollection();
			GenerateGroup37RffSegment(sg37, ReferenceCodeQualifierList.GovernmentAgencyReferenceNumber, item.CategoryCode);
			GenerateEndUseCodesSegment(sg37, item);
			GenerateGroup38SegmentForStrategicGoods(sg37.Group38, item);
		}

		protected void GenerateRegistrationDateSegment(SegmentGroup37 sg37, ZDate regoDate)
		{
			if (!regoDate.IsEmpty)
			{
				GenerateGroup37DtmSegment(sg37, DateOrTimeOrPeriodFunctionCodeQualifierList.DateOfFirstRegistration, regoDate);
			}
		}

		protected void GenerateGroup37RffSegment(SegmentGroup37 sg37, ReferenceCodeQualifierList qualifier, ZString identifier)
		{
			if (!identifier.IsEmpty)
			{
				var rff = sg37.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rff.Reference.ReferenceCodeQualifier = qualifier;
				rff.Reference.ReferenceIdentifier = identifier;
			}
		}

		protected void GenerateGroup37DtmSegment(SegmentGroup37 sg37, DateOrTimeOrPeriodFunctionCodeQualifierList qualifier, ZDate date)
		{
			if (!date.IsEmpty)
			{
				DTMSegment dTM = sg37.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dTM.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = qualifier;
				dTM.DateTimePeriod.DateOrTimeOrPeriodText = date.ToString("yyyyMMdd");
				dTM.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmdd;
			}
		}

		protected void GenerateEndUseCodesSegment(SegmentGroup37 sg37, ICusItem item)
		{
			GINSegment gIN = sg37.GIN.InstantiateAChildAndAddItToChildrenCollection();
			gIN.ObjectIdentificationCodeQualifier = ObjectIdentificationCodeQualifierList.ValueListSubset;
			gIN.IdentityNumberRange1.ObjectIdentifier1 = item.EndUseCode1;
			gIN.IdentityNumberRange2.ObjectIdentifier1 = item.EndUseCode2;
			gIN.IdentityNumberRange3.ObjectIdentifier1 = item.EndUseCode3;
		}

		protected void GenerateGoodsIdentityNumberSegment(SegmentGroup37 sg37, ICusItem item)
		{
			if (item.CASCCodes1 != null)
			{
				for (int i = 0; i < item.CASCCodes1.Count; i++)
				{
					if (!item.CASCCodes1[i].CY_Data.IsEmpty)
					{
						GINSegment gIN = sg37.GIN.InstantiateAChildAndAddItToChildrenCollection();
						gIN.ObjectIdentificationCodeQualifier = ObjectIdentificationCodeQualifierList.ValueListSubset;
						gIN.IdentityNumberRange1.ObjectIdentifier1 = item.CASCCodes1[i].CY_Data;

						if (item.CASCCodes2 != null && item.CASCCodes2.Count > i && !item.CASCCodes2[i].CY_Data.IsEmpty)
						{
							gIN.IdentityNumberRange2.ObjectIdentifier1 = item.CASCCodes2[i].CY_Data;
						}

						if (item.CASCCodes3 != null && item.CASCCodes3.Count > i && !item.CASCCodes3[i].CY_Data.IsEmpty)
						{
							gIN.IdentityNumberRange3.ObjectIdentifier1 = item.CASCCodes3[i].CY_Data;
						}
					}
				}
			}
		}

		void GenerateGroup38SegmentForMotorVehicles(SegmentGroup38MessageSection sg38Section, ICusItem item)
		{
			if (item.EngineCapacity > 0)
			{
				var sg38 = sg38Section.InstantiateAChildAndAddItToChildrenCollection();
				var imd = sg38.IMD.InstantiateAChildAndAddItToChildrenCollection();
				imd.ItemCharacteristic.ItemCharacteristicCode = ItemCharacteristicCodeList.Product;
				imd.ItemDescription.ItemDescription1 = item.EngineCapacity.ToString("0.00", CultureInfo.InvariantCulture);
				imd.ItemDescription.ItemDescription2 = item.EngineCapacityUnit;
			}
		}

		void GenerateGroup38SegmentForStrategicGoods(SegmentGroup38MessageSection sg38Section, ICusItem item)
		{
			if (!item.EndUseDescription.IsEmpty)
			{
				var sg38 = sg38Section.InstantiateAChildAndAddItToChildrenCollection();
				var imd = sg38.IMD.InstantiateAChildAndAddItToChildrenCollection();
				imd.ItemCharacteristic.ItemCharacteristicCode = ItemCharacteristicCodeList.EndUseApplication;
				imd.ItemDescription.ItemDescription1 = item.EndUseDescription;
			}
		}

		#endregion

		#region Segment Group 39

		protected virtual void GenerateSegmentGroup39(SegmentGroup39MessageSection sg39Section, ICusItem item)
		{
			if (sgCusdec.InwardTransportCode == SGConstants.TransportCodes.Sea || sgCusdec.InwardTransportCode == SGConstants.TransportCodes.Air)
			{
				GenerateGroup39DocSegment(sg39Section, DocumentNameCodeList.HouseWaybill, item.InwardHAWB);
			}

			if (sgCusdec.OutwardTransportCode == SGConstants.TransportCodes.Sea || sgCusdec.OutwardTransportCode == SGConstants.TransportCodes.Air)
			{
				if (!item.OutwardHAWB.IsEmpty)
				{
					GenerateGroup39DocSegment(sg39Section, DocumentNameCodeList.HouseBillOfLading, item.OutwardHAWB);
				}
			}
		}

		protected void GenerateGroup39DocSegment(SegmentGroup39MessageSection sg39Section, DocumentNameCodeList docQualifier, ZString waybill)
		{
			if (!waybill.IsEmpty)
			{
				var sg39 = sg39Section.InstantiateAChildAndAddItToChildrenCollection();
				var doc = sg39.DOC.InstantiateAChildAndAddItToChildrenCollection();
				doc.DocumentMessageName.DocumentNameCode = docQualifier;
				doc.DocumentMessageDetails.DocumentIdentifier = waybill;
			}
		}

		#endregion

		#region Segment Group 41

		protected virtual void GenerateSegmentGroup41(SegmentGroup41MessageSection sg41Section, ICusItem item)
		{
		}

		#endregion

		#region Segment Group 42

		protected virtual void GenerateSegmentGroup42(SegmentGroup42MessageSection sg42Section, ICusItem item)
		{
		}

		#endregion

		#region Segment Group 43

		protected virtual void GenerateSegmentGroup43(SegmentGroup43MessageSection sg43Section, ICusItem item)
		{
			if (!sgCusdec.AdditionalMessageInformation.RefundCode.IsEmpty)
			{
				PopulateRefundSegments(sg43Section, item);
			}
			else
			{
				var preferenceIndicator = item.PreferenceIndicator;
				if (item.DutyAmount > 0 || (!preferenceIndicator.IsEmpty && preferenceIndicator != PreferentialIndicatorCodeList.Codes.STD))
				{
					PopulateCustomsDutySegments(sg43Section, item);
				}

				if (item.ExciseAmount > 0)
				{
					PopulateCustomsExciseSegments(sg43Section, item);
				}

				PopulateGSTSegments(sg43Section, item);
			}
		}

		protected void PopulateCustomsDutySegments(SegmentGroup43MessageSection sg43Section, ICusItem item)
		{
			var sg43 = sg43Section.InstantiateAChildAndAddItToChildrenCollection();
			decimal dutyRate = !item.DutyPercentageRate.IsEmpty ? item.DutyPercentageRate : item.DutyUnitRate;
			GenerateSegment43TaxSegment(sg43, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, item.DutyRateUnit, dutyRate, item.PreferenceIndicator);
			if (item.DutyAmount > 0)
			{
				GenerateSegment43MoaSegment(sg43, MonetaryAmountTypeCodeQualifierList.DutyAmount, item.DutyAmount);
			}
		}

		protected void PopulateCustomsExciseSegments(SegmentGroup43MessageSection sg43Section, ICusItem item)
		{
			var sg43 = sg43Section.InstantiateAChildAndAddItToChildrenCollection();
			decimal exciseRate = !item.ExcisePercentageRate.IsEmpty ? item.ExcisePercentageRate : item.ExciseUnitRate;
			GenerateSegment43TaxSegment(sg43, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, item.DutyRateUnit, exciseRate, item.PreferenceIndicator);
			GenerateSegment43MoaSegment(sg43, MonetaryAmountTypeCodeQualifierList.DutyTaxOrFeeAmount, item.ExciseAmount);
		}

		///// <summary>
		///// Note 2:
		/////   For all Declaration Types, specify item Other tax amount, if applicable.
		/////   TAX: 5=Customs duty (refer to Notes 1, 2 and 5)
		/////   MOA: 124=Tax amount (refer to Note 2)
		///// </summary>
		//void PoulateOtherTaxSegments(SegmentGroup43MessageSection sg43Section, ICusItem item)
		//{
		//    var sg43 = sg43Section.InstantiateAChildAndAddItToChildrenCollection();
		//    GenerateSegment43TaxSegment(sg43, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, "", item.OtherCharge.Percentage, item.PreferenceIndicator);
		//    GenerateSegment43MoaSegment(sg43, MonetaryAmountTypeCodeQualifierList.TaxAmount, item.OtherCharge.Amount);
		//}

		/// <summary>
		/// Note 3:
		/// Mandatory for all Declaration Types, except for cases facilitated by Customs.
		/// Example:
		///   i) Goods received into Class 2 Yard or Container Freight Warehouse.
		///   ii) Goods received into or released from Freeport.
		/// a) For Declaration Type = GTR, the declared item GST payable amount must be equal to (CIF/LSP x GST rate).
		/// b) For other Declaration Types, the declared item GST payable amount must equal to
		///    (CIF/LSP + Customs Duty + Excise Duty + Other tax) X GST rate, where applicable.
		/// </summary>
		protected void PopulateGSTSegments(SegmentGroup43MessageSection sg43Section, ICusItem item)
		{
			var sg43 = sg43Section.InstantiateAChildAndAddItToChildrenCollection();
			GenerateSegment43TaxSegment(sg43, DutyOrTaxOrFeeFunctionCodeQualifierList.Tax, "", item.GSTRate, "", SGConstants.NumericFormatting.TradeNet4Point1.DecimalPlacesForGSTPercentage);
			GenerateSegment43MoaSegment(sg43, MonetaryAmountTypeCodeQualifierList.GoodsAndServicesTax, item.GSTPayable);
		}

		protected void GenerateSegment43TaxSegment(SegmentGroup43 sg43, DutyOrTaxOrFeeFunctionCodeQualifierList qualifier, string rateUnit, decimal rate, string preferenceIndicator, int decimalPlaces = SGConstants.NumericFormatting.DecimalPlacesForTaxRateValues)
		{
			var tax = sg43.TAX.InstantiateAChildAndAddItToChildrenCollection();
			tax.DutyOrTaxOrFeeFunctionCodeQualifier = qualifier;

			if (qualifier == DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty || qualifier == DutyOrTaxOrFeeFunctionCodeQualifierList.TaxRelatedInformation)
			{
				if (!string.IsNullOrEmpty(preferenceIndicator) && preferenceIndicator != PreferentialIndicatorCodeList.Codes.STD)
				{
					tax.DutyTaxFeeType.DutyOrTaxOrFeeTypeName = preferenceIndicator;
				}
			}

			if (rate > 0)
			{
				tax.DutyTaxFeeDetail.DutyOrTaxOrFeeRateCode = rateUnit;
				tax.DutyTaxFeeDetail.DutyOrTaxOrFeeRate = Utilities.FormatNumber(rate, decimalPlaces);
			}
		}

		void GenerateSegment43MoaSegment(SegmentGroup43 sG43, MonetaryAmountTypeCodeQualifierList qualifier, decimal monetaryValue)
		{
			MOASegment mOA = sG43.MOA.InstantiateAChildAndAddItToChildrenCollection();
			mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = qualifier;
			mOA.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(monetaryValue, SGConstants.NumericFormatting.DecimalPlacesForAmountValues);
		}

		void PopulateRefundSegments(SegmentGroup43MessageSection sg43Section, ICusItem item)
		{
			if (item.ItemDutyRefund > 0)
			{
				PopulateItemRefundAmount(sg43Section, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, DutyOrTaxOrFeeTypeNameCodeList.CustomsDuty, item.ItemDutyRefund);
			}

			if (item.ItemExciseRefund > 0)
			{
				PopulateItemRefundAmount(sg43Section, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, DutyOrTaxOrFeeTypeNameCodeList.ExciseDuty, item.ItemExciseRefund);
			}

			if (item.ItemGSTRefund > 0)
			{
				PopulateItemRefundAmount(sg43Section, DutyOrTaxOrFeeFunctionCodeQualifierList.Tax, DutyOrTaxOrFeeTypeNameCodeList.GoodsAndServicesTax, item.ItemGSTRefund);
			}
		}

		void PopulateItemRefundAmount(SegmentGroup43MessageSection sg43Section, DutyOrTaxOrFeeFunctionCodeQualifierList qualifier, DutyOrTaxOrFeeTypeNameCodeList taxFeeType, decimal amount)
		{
			var sG43 = sg43Section.InstantiateAChildAndAddItToChildrenCollection();
			TAXSegment tAX = sG43.TAX.InstantiateAChildAndAddItToChildrenCollection();
			tAX.DutyOrTaxOrFeeFunctionCodeQualifier = qualifier;
			tAX.DutyTaxFeeType.DutyOrTaxOrFeeTypeNameCode = taxFeeType;
			GenerateSegment43MoaSegment(sG43, MonetaryAmountTypeCodeQualifierList.Refund, amount);
		}

		#endregion

		#endregion

		#endregion

		#region Summary Section

		protected override void GenerateSummarySection(CUSDECMessage cusdecEdifactMsg)
		{
			GenerateSummaryUns(cusdecEdifactMsg.UNS2);
			GenerateSummaryCnt(cusdecEdifactMsg);
			GenerateSegmentGroup51(cusdecEdifactMsg.Group51);
			GenerateSummaryUnt(cusdecEdifactMsg);
		}

		void GenerateSummaryUns(UNSSegmentMessageSection uns2Section)
		{
			var uns = uns2Section.InstantiateAChildAndAddItToChildrenCollection();
			uns.SectionIdentification = UnsSummary;
		}

		protected virtual void GenerateSummaryCnt(CUSDECMessage cusdecEdifactMsg)
		{
			var cntSection = cusdecEdifactMsg.CNT;
			var cnt = cntSection.InstantiateAChildAndAddItToChildrenCollection();
			cnt.Control.ControlTotalTypeCodeQualifier = ControlTotalTypeCodeQualifierList.NumberOfCustomsItemDetailLines;
			cnt.Control.ControlTotalQuantity = cusdecEdifactMsg.Group32.Count.ToString();
		}

		protected virtual void GenerateSegmentGroup51(SegmentGroup51MessageSection sg51Section)
		{
			// 55 Duty = CustomsDuty/DutyAmount
			GenerateGroup51TaxAndMoa(sg51Section, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, MonetaryAmountTypeCodeQualifierList.DutyAmount, sgCusdec.TotalDutyPayable);
			// 161 Excise = CustomsDuty/DutyTaxOrFeeAmount
			GenerateGroup51TaxAndMoa(sg51Section, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, MonetaryAmountTypeCodeQualifierList.DutyTaxOrFeeAmount, sgCusdec.TotalExcisePayable);
			// 63 FOB = IndividualDutyTaxOrFeeCustomsItem/FobValue
			GenerateGroup51TaxAndMoa(sg51Section, DutyOrTaxOrFeeFunctionCodeQualifierList.IndividualDutyTaxOrFeeCustomsItem, MonetaryAmountTypeCodeQualifierList.FobValue, sgCusdec.TotalCustomsValue);
			// 9 Amount Payable = TotalOfAllDutiesTaxesAndFeesCustomsItem/AmountDueAmountPayable
			GenerateGroup51TaxAndMoa(sg51Section, DutyOrTaxOrFeeFunctionCodeQualifierList.TotalOfAllDutiesTaxesAndFeesCustomsItem, MonetaryAmountTypeCodeQualifierList.AmountDueAmountPayable, sgCusdec.TotalPayable);
			// 124 TaxAmount = CustomsDuty/TaxAmount
			GenerateGroup51TaxAndMoa(sg51Section, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, MonetaryAmountTypeCodeQualifierList.TaxAmount, sgCusdec.TotalOtherTaxPayable);
			// 369 GST = Tax/Goods and services tax
			GenerateGroup51TaxAndMoa(sg51Section, DutyOrTaxOrFeeFunctionCodeQualifierList.Tax, MonetaryAmountTypeCodeQualifierList.GoodsAndServicesTax, sgCusdec.TotalGSTPayable);
		}

		protected void GenerateGroup51TaxAndMoa(SegmentGroup51MessageSection sg51Section, DutyOrTaxOrFeeFunctionCodeQualifierList taxQualifier, MonetaryAmountTypeCodeQualifierList moaQualifier, ZDecimal amount)
		{
			if (amount > 0)
			{
				var sg51 = sg51Section.InstantiateAChildAndAddItToChildrenCollection();
				var tax = sg51.TAX.InstantiateAChildAndAddItToChildrenCollection();
				tax.DutyOrTaxOrFeeFunctionCodeQualifier = taxQualifier;
				var moa = sg51.MOA.InstantiateAChildAndAddItToChildrenCollection();
				moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier = moaQualifier;
				moa.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(amount, SGConstants.NumericFormatting.DecimalPlacesForAmountValues);
			}
		}

		void GenerateSummaryUnt(CUSDECMessage cusdecEdifactMsg)
		{
			var untSection = cusdecEdifactMsg.UNT;
			var unt = untSection.InstantiateAChildAndAddItToChildrenCollection();
			unt.MessageReferenceNumber = cusdecEdifactMsg.UNH[0].MessageReferenceNumber;
			unt.NumberOfSegmentsInTheMessage = cusdecEdifactMsg.CountIncludingUNT.ToString();
		}

		#endregion

		#endregion
	}
}
