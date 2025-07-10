using System.Linq;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	using System.Globalization;
	using CargoWise.Types;
	using Enterprise.Customs.SG.Business.CustomsMessaging;
	using Enterprise.Customs.SG.V4.Business;
	using Enterprise.Edifact.D05B.Elements;
	using Enterprise.Edifact.D05B.Messages.CUSDEC;
	using Enterprise.Edifact.D05B.Segments;
	using Enterprise.Edifact.Utilities;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Messaging.Business;
	using Enterprise.ZArchitecture.Core;

	public abstract class CUSDEC : CusdecBase<CUSDECMessage>
	{
		public CUSDEC(ISGCUSDEC sgCusdec)
			: base(sgCusdec)
		{
		}

		#region Implementation

		protected override string UnhMessageReleaseNumber
		{
			get { return "05B"; }
		}

		protected override string UnhAssociationAssignedCode
		{
			get { return "040"; }
		}

		#region Message Segments

		#region Header Section

		protected override void GenerateHeaderSection(CUSDECMessage cusdecEdifactMsg)
		{
			GenerateUNH(cusdecEdifactMsg.UNH);
			GenerateBGM(cusdecEdifactMsg.BGM);
			GenerateCST(cusdecEdifactMsg.CST);
			GenerateLOCSegments(cusdecEdifactMsg.LOC);
			GenerateDTMSegments(cusdecEdifactMsg.DTM);
			GenerateGEI(cusdecEdifactMsg.GEI);
			GenerateMEASegments(cusdecEdifactMsg.MEA);
			GenerateEQDAndSELSegments(cusdecEdifactMsg.EQD, cusdecEdifactMsg.SEL);
			GenerateFTX(cusdecEdifactMsg.FTX);
			GenerateSegmentGroup1(cusdecEdifactMsg.Group1);
			GenerateSegmentGroup4(cusdecEdifactMsg.Group4);
			GenerateSegmentGroup5(cusdecEdifactMsg.Group5);
			GenerateSegmentGroup6(cusdecEdifactMsg.Group6);
		}

		#region Header Section Non Grouped Segments

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

		protected virtual void GenerateCST(CSTSegmentMessageSection cstSection)
		{
			var cst = cstSection.InstantiateAChildAndAddItToChildrenCollection();
			cst.CustomsIdentityCodes1.CustomsGoodsIdentifier = sgCusdec.CargoPackingType;
		}

		#region Locations

		protected virtual void GenerateLOCSegments(LOCSegmentMessageSection locSection)
		{
			if (sgCusdec.HasInwardTransport)
			{
				GenerateLOCSegment(
					locSection, LocationFunctionCodeQualifierList.PlacePortOfLoading,
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
					locSection, LocationFunctionCodeQualifierList.PlacePortOfDischarge,
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

		#region For subclasses

		protected void GeneratePlaceOfStorageSegment(LOCSegmentMessageSection locSection)
		{
			GenerateLOCSegment(locSection, LocationFunctionCodeQualifierList.LocationOfGoods, sgCusdec.PlaceOfStorage, string.Empty);
		}

		protected void GenerateInwardVesselLocationSegment(LOCSegmentMessageSection locSection)
		{
			if (sgCusdec.InwardVesselBerth != null)
			{
				GenerateLOCSegment(locSection, LocationFunctionCodeQualifierList.Berth, sgCusdec.InwardVesselBerth);
			}
		}

		protected void GenerateOutwardVesselLocationSegment(LOCSegmentMessageSection locSection)
		{
			if (sgCusdec.OutwardVesselBerth != null)
			{
				GenerateLOCSegment(locSection, LocationFunctionCodeQualifierList.ScheduledBerth, sgCusdec.OutwardVesselBerth);
			}
		}

		protected void GenerateCountryOfFinalDestinationSegment(LOCSegmentMessageSection locSection)
		{
			if (!sgCusdec.CountryOfFinalDestination.IsEmpty)
			{
				GenerateLOCSegment(locSection, LocationFunctionCodeQualifierList.CountryOfUltimateDestination, sgCusdec.CountryOfFinalDestination, string.Empty);
			}
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
				loc.LocationIdentification.LocationNameCode = locationCode;
				if (nameAndAddress.Length > 0)
				{
					loc.LocationIdentification.LocationName = nameAndAddress;
				}
			}
		}

		#endregion

		#endregion

		#region Dates

		protected virtual void GenerateDTMSegments(DTMSegmentMessageSection dtmSection)
		{
			if (sgCusdec.HasOutwardTransport && !sgCusdec.DepartureDate.IsEmpty)
			{
				GenerateDTMSegment(dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansDepartureDateTime, sgCusdec.DepartureDate);
			}

			if (
				(sgCusdec.HasInwardTransport || sgCusdec.DeclarationType == DeclarationTypeCodeList.Codes.BKT)
				&& !sgCusdec.ArrivalDate.IsEmpty)
			{
				GenerateDTMSegment(dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeActual, sgCusdec.ArrivalDate);
			}
		}

		#region DTMSegments

		protected void GenerateDTMSegment(DTMSegmentMessageSection dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList qualifier, ZDate segmentDate)
		{
			if (!segmentDate.IsEmpty)
			{
				var dTM = dtmSection.InstantiateAChildAndAddItToChildrenCollection();
				dTM.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = qualifier;
				dTM.DateTimePeriod.DateOrTimeOrPeriodText = segmentDate.ToString("yyyyMMdd");
				dTM.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmdd;
			}
		}

		#endregion

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

		protected virtual void GenerateMEASegments(MEASegmentMessageSection meaSection)
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
			GenerateMEASegment(meaSection, MeasurementPurposeCodeQualifierList.ExternalDimension, sgCusdec.TotalOuterPackUnitOfQty, sgCusdec.TotalOuterPack, SGConstants.NumericFormatting.DecimalPlacesForMeasurementValues);
		}

		void GenerateTotalGrossWeightSegment(MEASegmentMessageSection meaSection)
		{
			string weightUnitInRequiredSGCValue = SGConstants.Weight.Kilograms;
			if (((sgCusdec.IsInwardDeclaration || sgCusdec.IsTranshipmentDeclaration) && sgCusdec.InwardTransportCode == SGConstants.TransportCodes.Sea) ||
				(sgCusdec.IsOutwardDeclaration && sgCusdec.OutwardTransportCode == SGConstants.TransportCodes.Sea))
			{
				weightUnitInRequiredSGCValue = SGConstants.Weight.Tonnes;
			}

			var (totalWeight, totalWeightUnit) = ConvertToSingaporeCustomsRequiredWeightUnit(sgCusdec.TotalGrossWeight, sgCusdec.TotalGrossWeightUnitOfQty, weightUnitInRequiredSGCValue);
			GenerateMEASegment(meaSection, MeasurementPurposeCodeQualifierList.DimensionsTotalWeight, totalWeightUnit, totalWeight, SGConstants.NumericFormatting.DecimalPlacesForMeasurementValues);
		}

		void GenerateOutwardVesselNRT(MEASegmentMessageSection meaSection)
		{
			if (!sgCusdec.OutwardVesselNRT.IsEmpty)
			{
				GenerateMEASegment(meaSection, MeasurementPurposeCodeQualifierList.WeightOfConveyance, string.Empty, sgCusdec.OutwardVesselNRT, 2);
			}
		}

		void GenerateMEASegment(MEASegmentMessageSection meaSection, MeasurementPurposeCodeQualifierList qualifier, string unitOfQty, decimal qtyValue, int decimalPlaces)
		{
			var mea = meaSection.InstantiateAChildAndAddItToChildrenCollection();
			mea.MeasurementPurposeCodeQualifier = qualifier;
			mea.ValueRange.MeasurementUnitCode = unitOfQty;
			string qtyValueAsString = Utilities.FormatNumber(qtyValue, SGConstants.NumericFormatting.DecimalPlacesForMeasurementValues);
			if (qualifier == MeasurementPurposeCodeQualifierList.WeightOfConveyance)
			{
				qtyValueAsString = Utilities.FormatNumber(qtyValue, SGConstants.NumericFormatting.DecimalPlacesForNetRegisteredTonnage);
			}

			mea.ValueRange.Measure = qtyValueAsString;
		}

		#endregion

		#endregion

		protected virtual void GenerateEQDAndSELSegments(EQDSegmentMessageSection eqdSection, SELSegmentMessageSection selSection)
		{
			if (sgCusdec.Containers != null)
			{
				int i = 0;

				foreach (ICusContainer container in sgCusdec.Containers)
				{
					var eqd = eqdSection.InstantiateAChildAndAddItToChildrenCollection();
					eqd.EquipmentTypeCodeQualifier = EquipmentTypeCodeQualifierList.Container;
					eqd.EquipmentIdentification.EquipmentIdentifier = container.ContainerNumber;
					eqd.EquipmentIdentification.CodeListIdentificationCode = (++i).ToString();
					var (containerWeightInTonnes, _) = ConvertToSingaporeCustomsRequiredWeightUnit(container.ContainerWeight, container.ContainerWeightUnit, SGConstants.Weight.Tonnes);
					eqd.EquipmentSizeAndType.EquipmentSizeAndTypeDescription = container.ContainerType + container.ContainerSize.ToString() + ConvertContainerWeightTo3CharString(containerWeightInTonnes);

					var sel = selSection.InstantiateAChildAndAddItToChildrenCollection();
					sel.TransportUnitSealIdentifier = container.SealNumber.IsEmpty ? "NA" : container.SealNumber.ToString();
				}
			}
		}

		protected virtual void GenerateFTX(FTXSegmentMessageSection ftxSegment)
		{
			var traderRemarks = sgCusdec.TradersRemarksForMessage;
			if (traderRemarks != null && traderRemarks.Any())
			{
				var ftx = ftxSegment.InstantiateAChildAndAddItToChildrenCollection();
				ftx.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.GeneralInformation;
				ftx.TextLiteral.FreeText1 = traderRemarks.ElementAtOrDefault(0);
				ftx.TextLiteral.FreeText2 = traderRemarks.ElementAtOrDefault(1);
			}
		}

		#endregion

		#region Segment Group 1

		#region References

		protected virtual void GenerateSegmentGroup1(SegmentGroup1MessageSection sg1Section)
		{
			GenerateMessageSenderMailboxSegment(sg1Section);
		}

		#region Reference Segments

		protected void GenerateMessageSenderMailboxSegment(SegmentGroup1MessageSection sg1Section)
		{
			GenerateRFFSegment(sg1Section, ReferenceCodeQualifierList.MessageSender, sgCusdec.DeclarantId.Left(4) + "." + sgCusdec.DeclarantId);
		}

		protected virtual void PopulateLicensesAndDocumentsSegments(SegmentGroup1MessageSection sg1Section)
		{
			if (sgCusdec.LicencesAndDocuments != null)
			{
				foreach (ICusDocument licenceDocument in sgCusdec.LicencesAndDocuments)
				{
					GenerateRFFSegment(sg1Section, ReferenceCodeQualifierList.DocumentNumber, licenceDocument.LicenceNumber);
				}
			}
		}

		protected virtual void PopulateSupplyIndicatorSegment(SegmentGroup1MessageSection sg1Section)
		{
			if (!sgCusdec.SupplyIndicator.IsEmpty)
			{
				GenerateRFFSegment(sg1Section, ReferenceCodeQualifierList.TransactionReferenceNumber, sgCusdec.SupplyIndicator);
			}
		}

		protected virtual void PopulatePreviousPermitNumberSegment(SegmentGroup1MessageSection sg1Section)
		{
			if (!sgCusdec.PreviousPermitNumber.IsEmpty)
			{
				GenerateRFFSegment(sg1Section, ReferenceCodeQualifierList.RelatedDocumentNumber, sgCusdec.PreviousPermitNumber);
			}
		}

		protected void PopulatePermitNoToUpdateOrCancelSegment(SegmentGroup1MessageSection sg1Section)
		{
			if (!sgCusdec.PermitNoToUpdateOrCancel.IsEmpty)
			{
				GenerateRFFSegment(sg1Section, ReferenceCodeQualifierList.CustomsDeclarationNumber, sgCusdec.PermitNoToUpdateOrCancel);
			}
		}

		protected void PopulateReplacementPermitNoSegment(SegmentGroup1MessageSection sg1Section)
		{
			if (!sgCusdec.ReplacementPermitNumber.IsEmpty)
			{
				GenerateRFFSegment(sg1Section, ReferenceCodeQualifierList.GoodsDeclarationNumber, sgCusdec.ReplacementPermitNumber);
			}
		}

		protected void PopulateAdditionalRecipientsSegments(SegmentGroup1MessageSection sg1Section)
		{
			if (sgCusdec.AdditionalRecipients != null)
			{
				foreach (ZString recipientID in sgCusdec.AdditionalRecipients)
				{
					GenerateRFFSegment(sg1Section, ReferenceCodeQualifierList.MessageRecipient, recipientID);
				}
			}
		}

		protected void GenerateRFFSegment(SegmentGroup1MessageSection sg1Section, ReferenceCodeQualifierList qualifier, ZString identifier)
		{
			var sg1 = sg1Section.InstantiateAChildAndAddItToChildrenCollection();
			var rff = sg1.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rff.Reference.ReferenceCodeQualifier = qualifier;
			rff.Reference.ReferenceIdentifier = identifier;
		}

		#endregion

		#endregion

		#endregion

		#region Segment Group 4

		protected virtual void GenerateSegmentGroup4(SegmentGroup4MessageSection sg4Section)
		{
		}

		protected void PopulateInwardTransport(SegmentGroup4MessageSection sg4Section)
		{
			if (sgCusdec.HasInwardTransport)
			{
				GenerateTDTSegment(sg4Section, TransportStageCodeQualifierList.AtArrival, sgCusdec.InwardTransportCode.ToString(), sgCusdec.InwardJourneyIdentifier, sgCusdec.InwardTransportIdentifier, string.Empty);
			}
		}

		protected void PopulateOutwardTransport(SegmentGroup4MessageSection sg4Section)
		{
			if (sgCusdec.HasOutwardTransport)
			{
				var sg4 = GenerateTDTSegment(sg4Section, TransportStageCodeQualifierList.AtDeparture, sgCusdec.OutwardTransportCode.ToString(), sgCusdec.OutwardJourneyIdentifier, sgCusdec.OutwardTransportIdentifier, sgCusdec.OutwardVesselType);

				if (sgCusdec.OutwardTransportCode == SGConstants.TransportCodes.Sea && sgCusdec.IsSeaStoreDeclaration && !sgCusdec.OutwardVesselNationality.IsEmpty)
				{
					TPLSegment tPL = sg4.TPL.InstantiateAChildAndAddItToChildrenCollection();
					tPL.TransportIdentification.TransportMeansNationalityCode = sgCusdec.OutwardVesselNationality;
				}

				if (sgCusdec.OutwardTransportCode == SGConstants.TransportCodes.Sea)
				{
					PopulateTowingVesselIdentification(sg4Section);
				}
			}
		}

		void PopulateTowingVesselIdentification(SegmentGroup4MessageSection sg4Section)
		{
			if (!sgCusdec.TowingVesselName.IsEmpty)
			{
				GenerateTDTSegment(sg4Section, TransportStageCodeQualifierList.InlandWaterwayTransport, "", sgCusdec.TowingVesselVoyageNo, sgCusdec.TowingVesselName, "");
			}
		}

		SegmentGroup4 GenerateTDTSegment(SegmentGroup4MessageSection sg4Section, TransportStageCodeQualifierList qualifier, string transportMode, ZString voyageFlight, string vesselName, string vesselType)
		{
			var sg4 = sg4Section.InstantiateAChildAndAddItToChildrenCollection();
			var tdt = sg4.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tdt.TransportStageCodeQualifier = qualifier;
			tdt.ModeOfTransport.TransportModeNameCode = transportMode;

			if (tdt.ModeOfTransport.TransportModeNameCode == Enterprise.Core.Constants.TransportCodes.Sea || qualifier == TransportStageCodeQualifierList.InlandWaterwayTransport)
			{
				tdt.TransportIdentification.TransportMeansIdentificationNameIdentifier = voyageFlight.IsEmpty ? (ZString)"NA" : voyageFlight;
				tdt.TransportIdentification.TransportMeansIdentificationName = vesselName;
				tdt.TransportMeans.TransportMeansDescription = vesselType;
			}
			else if (tdt.ModeOfTransport.TransportModeNameCode == Enterprise.Core.Constants.TransportCodes.Air)
			{
				tdt.TransportIdentification.TransportMeansIdentificationNameIdentifier = voyageFlight;
			}

			return sg4;
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

		#region OceanBill MasterBill

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

		#endregion

		protected void PopulateSupportingDocs(SegmentGroup5MessageSection sg5Section)
		{
			if (sgCusdec.AdditionalMessageInformation != null && sgCusdec.AdditionalMessageInformation.SupportingDocuments != null)
			{
				foreach (ICusAttachment attachment in sgCusdec.AdditionalMessageInformation.SupportingDocuments)
				{
					var sg5 = sg5Section.InstantiateAChildAndAddItToChildrenCollection();
					var doc = sg5.DOC.InstantiateAChildAndAddItToChildrenCollection();
					doc.DocumentMessageName.DocumentName = DocDocumentAttachment;
					doc.DocumentMessageDetails.DocumentIdentifier = attachment.DocType;
					doc.DocumentMessageDetails.DocumentSourceDescription = attachment.FileName.ToASCII();
				}
			}
		}

		#endregion

		#region Segment Group 6

		protected virtual void GenerateSegmentGroup6(SegmentGroup6MessageSection sg6Section)
		{
			PopulateDeclaringAgentSegment(sg6Section);
			PopulateDeclarantSegments(sg6Section);
		}

		protected void PopulateDeclaringAgentSegment(SegmentGroup6MessageSection sg6Section)
		{
			var sg6 = sg6Section.InstantiateAChildAndAddItToChildrenCollection();
			var nad = sg6.NAD.InstantiateAChildAndAddItToChildrenCollection();
			nad.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.DeclarantsAgentRepresentative;
			nad.PartyIdentificationDetails.PartyIdentifier = GlbCompany.CurrentCompany.GC_CustomsRegistrationNo;
			TextSplitElegantly splitter = new TextSplitElegantly(35, 3);
			splitter.Text = GlbCompany.CurrentCompany.GC_Name;
			nad.PartyName.PartyName1 = splitter[0];
			nad.PartyName.PartyName2 = splitter[1];
			nad.PartyName.PartyName3 = splitter[2];
		}

		protected void PopulateDeclarantSegments(SegmentGroup6MessageSection sg6Section)
		{
			SegmentGroup6 sg6 = sg6Section.InstantiateAChildAndAddItToChildrenCollection();
			NADSegment nad = sg6.NAD.InstantiateAChildAndAddItToChildrenCollection();
			nad.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.Declarant;
			TextSplitElegantly splitter = new TextSplitElegantly(35, 3);
			splitter.Text = sgCusdec.Declarant.Name;
			nad.NameAndAddress.NameAndAddressDescription1 = splitter[0];
			nad.NameAndAddress.NameAndAddressDescription2 = splitter[1];
			nad.NameAndAddress.NameAndAddressDescription3 = splitter[2];

			CTASegment cTA = sg6.CTA.InstantiateAChildAndAddItToChildrenCollection();
			cTA.ContactFunctionCode = ContactFunctionCodeList.InformationContact;
			string declarantCode = sgCusdec.Declarant.Code.Replace("@", " ");
			cTA.DepartmentOrEmployeeDetails.DepartmentOrEmployeeName = declarantCode;
			if (sgCusdec.Declarant.Code.IsEmpty)
			{
				cTA.DepartmentOrEmployeeDetails.DepartmentOrEmployeeName = sgCusdec.Declarant.Passport;
			}

			COMSegment cOM = sg6.COM.InstantiateAChildAndAddItToChildrenCollection();
			cOM.CommunicationContact.CommunicationAddressCodeQualifier = CommunicationAddressCodeQualifierList.Telephone;
			cOM.CommunicationContact.CommunicationAddressIdentifier = sgCusdec.Declarant.Phone;
		}

		#region Related Parties

		protected void PopulateInwardCarrierAgentSegment(SegmentGroup6MessageSection sg6Section)
		{
			if (sgCusdec.InwardCarrierAgent != null)
			{
				GenerateNADSegment(sg6Section, PartyFunctionCodeQualifierList.CarriersAgent, sgCusdec.InwardCarrierAgent.UEN, string.Empty, sgCusdec.InwardCarrierAgent.Name, string.Empty);
			}
		}

		protected void PopulateOutwardCarrierAgentSegment(SegmentGroup6MessageSection sg6Section)
		{
			if (sgCusdec.OutwardCarrierAgent != null)
			{
				GenerateNADSegment(sg6Section, PartyFunctionCodeQualifierList.Carrier, sgCusdec.OutwardCarrierAgent.UEN, string.Empty, sgCusdec.OutwardCarrierAgent.Name, string.Empty);
			}
		}

		protected void PopulateHandlingAgentSegment(SegmentGroup6MessageSection sg6Section)
		{
			if (sgCusdec.HandlingAgent != null)
			{
				GenerateNADSegment(sg6Section, PartyFunctionCodeQualifierList.TransitPrincipalsAgentRepresentative, sgCusdec.HandlingAgent.UEN, string.Empty, sgCusdec.HandlingAgent.Name, string.Empty);
			}
		}

		protected void PopulateImporterSegment(SegmentGroup6MessageSection sg6Section)
		{
			if (sgCusdec.Importer != null)
			{
				GenerateNADSegment(sg6Section, PartyFunctionCodeQualifierList.Importer, sgCusdec.Importer.UEN, string.Empty, sgCusdec.Importer.Name, string.Empty);
			}
		}

		protected void PopulateExporterSegment(SegmentGroup6MessageSection sg6Section, bool includeAddress)
		{
			if (sgCusdec.Exporter != null)
			{
				if (includeAddress)
				{
					GenerateNADSegment(sg6Section, PartyFunctionCodeQualifierList.Exporter, sgCusdec.Exporter.UEN, string.Empty, sgCusdec.Exporter.Name, sgCusdec.Exporter.Address.FullAddress);
				}
				else
				{
					GenerateNADSegment(sg6Section, PartyFunctionCodeQualifierList.Exporter, sgCusdec.Exporter.UEN, string.Empty, sgCusdec.Exporter.Name, string.Empty);
				}
			}
		}

		protected void PopulateConsigneeSegment(SegmentGroup6MessageSection sg6Section)
		{
			if (sgCusdec.Consignee != null)
			{
				GenerateNADSegment(sg6Section, PartyFunctionCodeQualifierList.Consignee, string.Empty, string.Empty, sgCusdec.Consignee.Name, sgCusdec.Consignee.Address.FullAddress);
			}
		}

		protected void PopulateEndUserSegment(SegmentGroup6MessageSection sg6Section)
		{
			if (sgCusdec.EndUser != null)
			{
				GenerateNADSegment(sg6Section, PartyFunctionCodeQualifierList.UltimateConsignee, string.Empty, string.Empty, sgCusdec.EndUser.Name, sgCusdec.EndUser.Address.FullAddress);
			}
		}

		protected void PopulateClaimantSegments(SegmentGroup6MessageSection sg6Section)
		{
			if (sgCusdec.Claimant != null)
			{
				var sg6 = GenerateNADSegment(sg6Section, PartyFunctionCodeQualifierList.Claimant, sgCusdec.Claimant.UEN, sgCusdec.ClaimantName, sgCusdec.Claimant.Name, string.Empty);
				GenerateCTASegment(sg6, sgCusdec.ClaimantCode);
			}
		}

		protected void PopulateForwarderSegment(SegmentGroup6MessageSection sg6Section)
		{
			if (sgCusdec.FreightForwarder != null)
			{
				GenerateNADSegment(sg6Section, PartyFunctionCodeQualifierList.FreightForwarder, sgCusdec.FreightForwarder.UEN, string.Empty, sgCusdec.FreightForwarder.Name, string.Empty);
			}
		}

		protected void PopulateManufacturerSegment(SegmentGroup6MessageSection sg6Section, PartyFunctionCodeQualifierList qualifier, string uEN, string manufacturerName, string manufacturerAddress)
		{
			GenerateNADSegment(sg6Section, qualifier, uEN, string.Empty, manufacturerName, manufacturerAddress);
		}

		protected void PopulateBGIndicator(SegmentGroup6MessageSection sg6Section)
		{
			if (!sgCusdec.BGIndicator.IsEmpty)
			{
				SegmentGroup6 sG6 = GenerateNADSegment(sg6Section, PartyFunctionCodeQualifierList.BuyersBank, "", "", "", "");
				GenerateSG6RFFSegment(sG6, ReferenceCodeQualifierList.DebitAccountNumber, sgCusdec.BGIndicator);
			}
		}

		SegmentGroup6 GenerateNADSegment(SegmentGroup6MessageSection sg6Section, PartyFunctionCodeQualifierList qualifier, string uEN, string name, string organisationName, string address)
		{
			var sg6 = sg6Section.InstantiateAChildAndAddItToChildrenCollection();

			var nad = sg6.NAD.InstantiateAChildAndAddItToChildrenCollection();
			nad.PartyFunctionCodeQualifier = qualifier;
			nad.PartyIdentificationDetails.PartyIdentifier = uEN;

			int segmentsAllowed = 3;
			if (qualifier == PartyFunctionCodeQualifierList.Consignee ||
				qualifier == PartyFunctionCodeQualifierList.Exporter ||
				qualifier == PartyFunctionCodeQualifierList.Importer ||
				qualifier == PartyFunctionCodeQualifierList.ManufacturerOfGoods)
			{
				segmentsAllowed = 2;
			}

			TextSplitElegantly splitter = new TextSplitElegantly(35, segmentsAllowed);

			if (qualifier == PartyFunctionCodeQualifierList.Claimant)
			{
				splitter.Text = name;
				nad.NameAndAddress.NameAndAddressDescription1 = splitter[0];
				nad.NameAndAddress.NameAndAddressDescription2 = splitter[1];
				nad.NameAndAddress.NameAndAddressDescription3 = splitter[2];
			}

			splitter.Text = organisationName;
			nad.PartyName.PartyName1 = splitter[0];
			nad.PartyName.PartyName2 = splitter[1];
			if (segmentsAllowed > 2)
			{
				nad.PartyName.PartyName3 = splitter[2];
			}

			if (!string.IsNullOrEmpty(address))
			{
				splitter.Text = address;
				nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = splitter[0];
				nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier2 = splitter[1];
				nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier3 = splitter[2];
			}

			return sg6;
		}

		protected void GenerateSG6RFFSegment(SegmentGroup6 sg6, ReferenceCodeQualifierList qualifier, ZString identifier)
		{
			var rff = sg6.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rff.Reference.ReferenceCodeQualifier = qualifier;
			rff.Reference.ReferenceIdentifier = identifier;
		}

		protected void GenerateCTASegment(SegmentGroup6 sg6, ZString id)
		{
			var cta = sg6.CTA.InstantiateAChildAndAddItToChildrenCollection();
			cta.ContactFunctionCode = ContactFunctionCodeList.InformationContact;
			cta.DepartmentOrEmployeeDetails.DepartmentOrEmployeeName = id;
		}

		#endregion

		#endregion

		#endregion

		#region Detail Section

		protected override void GenerateDetailSection(CUSDECMessage cusdecEdifactMsg)
		{
			GenerateDetailUNS(cusdecEdifactMsg.UNS1);
			GenerateSegmentGroup10(cusdecEdifactMsg.Group10);
			GenerateSegmentGroup30(cusdecEdifactMsg.Group30);
		}

		void GenerateDetailUNS(UNSSegmentMessageSection uns1Section)
		{
			var uns = uns1Section.InstantiateAChildAndAddItToChildrenCollection();
			uns.SectionIdentification = UnsDetail;
		}

		#region Segment Group 10

		public virtual void GenerateSegmentGroup10(SegmentGroup10MessageSection sg10Section)
		{
			if (sgCusdec.Invoices != null)
			{
				ZGuid invoiceIncludeInMsg = ZGuid.Empty;
				foreach (ICusInvoice invoice in sgCusdec.Invoices)
				{
					if (invoice.InvoicePK != invoiceIncludeInMsg)
					{
						var sg10 = sg10Section.InstantiateAChildAndAddItToChildrenCollection();
						var dms = sg10.DMS.InstantiateAChildAndAddItToChildrenCollection();
						dms.DocumentMessageIdentification.DocumentIdentifier = DmsInvoiceDetails;

						GenerateSegmentGroup11(sg10, invoice);
						GenerateSegmentGroup13(sg10, invoice);
						GenerateSegmentGroup14(sg10, invoice);
						GenerateSegmentGroup19(sg10, invoice);
						invoiceIncludeInMsg = invoice.InvoicePK;
					}
				}
			}
		}

		#region Invoice Segments

		void GenerateSegmentGroup11(SegmentGroup10 sG10, ICusInvoice invoice)
		{
			SegmentGroup11 sG11 = sG10.Group11.InstantiateAChildAndAddItToChildrenCollection();
			GenerateInvoiceTotalSegment(sG11, invoice);
		}

		void GenerateSegmentGroup13(SegmentGroup10 sG10, ICusInvoice invoice)
		{
			SegmentGroup13 sG13 = sG10.Group13.InstantiateAChildAndAddItToChildrenCollection();
			GenerateInvoiceTermTypeSegment(sG13, invoice);
		}

		void GenerateSegmentGroup14(SegmentGroup10 sG10, ICusInvoice invoice)
		{
			SegmentGroup14 sG14 = sG10.Group14.InstantiateAChildAndAddItToChildrenCollection();
			GenerateSupplierCodeSegment(sG14, invoice);
			GenerateSegmentGroup15(sG14, invoice);
		}

		void GenerateSupplierCodeSegment(SegmentGroup14 sG14, ICusInvoice invoice)
		{
			GenerateSG14NADSegment(sG14, invoice.Supplier);
		}

		void GenerateSG14NADSegment(SegmentGroup14 sG14, IOrganisation supplier)
		{
			NADSegment nAD = sG14.NAD.InstantiateAChildAndAddItToChildrenCollection();
			if (supplier != null)
			{
				nAD.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.Supplier;
				nAD.NameAndAddress.NameAndAddressDescription1 = supplier.UEN;
				TextSplitElegantly splitter = new TextSplitElegantly(35, 3);
				splitter.Text = supplier.Name;
				nAD.PartyName.PartyName1 = splitter[0];
				nAD.PartyName.PartyName2 = splitter[1];
				nAD.PartyName.PartyName3 = splitter[2];
			}
			else
			{
				nAD.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.Seller;
			}
		}

		void GenerateSegmentGroup15(SegmentGroup14 sG14, ICusInvoice invoice)
		{
			SegmentGroup15 sG15 = sG14.Group15.InstantiateAChildAndAddItToChildrenCollection();
			GenerateInvoiceNumberAndDateSegments(sG15, invoice);
		}

		void GenerateSegmentGroup19(SegmentGroup10 sG10, ICusInvoice invoice)
		{
			if (invoice.FreightCharge != null)
			{
				if (invoice.FreightCharge.Amount > 0)
				{
					GenerateChargeSegments(sG10, MonetaryAmountTypeCodeQualifierList.FreightCharge, invoice.FreightCharge);
				}
			}

			if (invoice.InsuranceCharge != null)
			{
				if (invoice.InsuranceCharge.Amount > 0)
				{
					GenerateChargeSegments(sG10, MonetaryAmountTypeCodeQualifierList.InsuranceChargesCustoms, invoice.InsuranceCharge);
				}
			}

			if (invoice.OtherCharge != null)
			{
				if (invoice.OtherCharge.Amount > 0)
				{
					GenerateChargeSegments(sG10, MonetaryAmountTypeCodeQualifierList.OtherCharges, invoice.OtherCharge);
				}
			}
		}

		#region Monetary

		void GenerateInvoiceTotalSegment(SegmentGroup11 sG11, ICusInvoice invoice)
		{
			GenerateMOASegment(sG11, MonetaryAmountTypeCodeQualifierList.InvoiceTotalAmount, invoice.InvoiceTotalAmount, invoice.InvoiceCurrency);
			if (invoice.InvoiceCurrency != Core.Constants.CurrencyCodes.Singapore)
			{
				GenerateExchangeRateSegment(sG11, invoice);
			}
		}

		void GenerateMOASegment(SegmentGroup11 sG11, MonetaryAmountTypeCodeQualifierList qualifier, decimal monetaryValue, string currencyCode)
		{
			MOASegment mOA = sG11.MOA.InstantiateAChildAndAddItToChildrenCollection();
			mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = qualifier;
			mOA.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(monetaryValue, SGConstants.NumericFormatting.DecimalPlacesForAmountValues);
			mOA.MonetaryAmount.CurrencyIdentificationCode = currencyCode;
		}

		void GenerateExchangeRateSegment(SegmentGroup11 sG11, ICusInvoice invoice)
		{
			GenerateCUXSegment(sG11, invoice.InvoiceCurrExchangeRate);
		}

		void GenerateCUXSegment(SegmentGroup11 sG11, decimal exchangeRate)
		{
			if (exchangeRate > 0)
			{
				CUXSegment cUX = sG11.Group12.InstantiateAChildAndAddItToChildrenCollection().CUX.InstantiateAChildAndAddItToChildrenCollection();
				cUX.CurrencyExchangeRate = Utilities.FormatNumber(exchangeRate, SGConstants.NumericFormatting.DecimalPlacesForExchangeRateValues);
			}
		}

		#endregion

		#region Misc Detail Segments

		void GenerateInvoiceTermTypeSegment(SegmentGroup13 sG13, ICusInvoice invoice)
		{
			if (!invoice.IncoTerm.IsEmpty)
			{
				TODSegment tOD = sG13.TOD.InstantiateAChildAndAddItToChildrenCollection();
				tOD.TermsOfDeliveryOrTransport.DeliveryOrTransportTermsDescriptionCode = DeliveryOrTransportTermsDescriptionCodeList.GetFromString(invoice.IncoTerm);
			}
		}

		void GenerateInvoiceNumberAndDateSegments(SegmentGroup15 sG15, ICusInvoice invoice)
		{
			if (!invoice.InvoiceNumber.IsEmpty)
			{
				DOCSegment dOC = sG15.DOC.InstantiateAChildAndAddItToChildrenCollection();
				dOC.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.CommercialInvoice;
				dOC.DocumentMessageDetails.DocumentIdentifier = invoice.InvoiceNumber;
			}

			if (!invoice.InvoiceDate.IsEmpty)
			{
				DTMSegment dTM = sG15.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dTM.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.InvoiceDateTime;
				dTM.DateTimePeriod.DateOrTimeOrPeriodText = invoice.InvoiceDate.ToString("yyyyMMdd");
				dTM.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmdd;
			}
		}

		void GenerateChargeSegments(SegmentGroup10 sG10, MonetaryAmountTypeCodeQualifierList chargeQualifier, ICusCharge charge)
		{
			SegmentGroup19 sG19 = sG10.Group19.InstantiateAChildAndAddItToChildrenCollection();
			ALCSegment aLC = sG19.ALC.InstantiateAChildAndAddItToChildrenCollection();
			aLC.AllowanceOrChargeCodeQualifier = AllowanceOrChargeCodeQualifierList.Charge;

			MOASegment mOA = sG19.MOA.InstantiateAChildAndAddItToChildrenCollection();
			mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = chargeQualifier;
			decimal monetaryAmount = charge.Amount;
			mOA.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(monetaryAmount, SGConstants.NumericFormatting.DecimalPlacesForAmountValues);
			mOA.MonetaryAmount.CurrencyIdentificationCode = charge.CurrencyCode;

			if (charge.Percentage > 0m)
			{
				PCDSegment pCD = sG19.PCD.InstantiateAChildAndAddItToChildrenCollection();
				pCD.PercentageDetails.PercentageTypeCodeQualifier = PercentageTypeCodeQualifierList.EntryPercentage;
				decimal chargePercentage = charge.Percentage;
				pCD.PercentageDetails.Percentage = Utilities.FormatNumber(chargePercentage, SGConstants.NumericFormatting.DecimalPlacesForPercentageValues);
			}

			if (charge.CurrencyCode != Core.Constants.CurrencyCodes.Singapore)
			{
				CUXSegment cUX = sG19.Group20.InstantiateAChildAndAddItToChildrenCollection().CUX.InstantiateAChildAndAddItToChildrenCollection();
				decimal exchangeRate = charge.ExchangeRate;
				cUX.CurrencyExchangeRate = Utilities.FormatNumber(exchangeRate, SGConstants.NumericFormatting.DecimalPlacesForExchangeRateValues);
			}
		}

		#endregion

		#endregion

		#endregion

		#region Invoice Lines

		public virtual void GenerateSegmentGroup30(SegmentGroup30MessageSection sg30Section)
		{
			if (sgCusdec.Items != null)
			{
				foreach (ICusItem item in sgCusdec.Items)
				{
					var sg30 = sg30Section.InstantiateAChildAndAddItToChildrenCollection();

					var cst = sg30.CST.InstantiateAChildAndAddItToChildrenCollection();
					cst.GoodsItemNumber = item.SerialNumber;
					cst.CustomsIdentityCodes1.CustomsGoodsIdentifier = item.HSCode;

					GenerateGroup30_FTX(sg30, item);
					GenerateGroup30_LOC(sg30, item);
					GenerateGroup30_DTM(sg30, item);
					GenerateGroup30_MEA(sg30, item);

					GenerateSegmentGroup31(sg30, item);
					GenerateSegmentGroup33(sg30, item);
					GenerateSegmentGroup35(sg30, item);
					GenerateSegmentGroup37(sg30, item);
					GenerateSegmentGroup39(sg30, item);
					GenerateSegmentGroup41(sg30, item);
				}
			}
		}

		#region Line Item Segments

		#region Free Text

		protected virtual void GenerateGroup30_FTX(SegmentGroup30 sG30, ICusItem item)
		{
			GenerateInvoiceItemDescriptionSegment(sG30, item);
			GenerateBrandAndModelSegment(sG30, item);
			GenerateDangerousGoodsSegment(sG30, item);
		}

		protected virtual void GenerateInvoiceItemDescriptionSegment(SegmentGroup30 sG30, ICusItem item)
		{
			if (!item.GoodsDescription.IsEmpty)
			{
				GenerateFTXSegment(sG30, TextSubjectCodeQualifierList.GoodsDescription, item.GoodsDescription);
			}
		}

		void GenerateBrandAndModelSegment(SegmentGroup30 sG30, ICusItem item)
		{
			if (sgCusdec.IsImport)
			{
				string brandName = item.BrandName.IsEmpty ? (ZString)SGConstants.Unbranded : item.BrandName;
				GenerateFTXSegment(sG30, TextSubjectCodeQualifierList.ProductInformation, brandName, item.ModelDescription);
			}
			else if (!item.BrandName.IsEmpty)
			{
				GenerateFTXSegment(sG30, TextSubjectCodeQualifierList.ProductInformation, item.BrandName, item.ModelDescription);
			}
		}

		void GenerateDangerousGoodsSegment(SegmentGroup30 sG30, ICusItem item)
		{
			if (!item.DGIndicator.IsEmpty)
			{
				GenerateFTXSegment(sG30, TextSubjectCodeQualifierList.DangerousGoodsAdditionalInformation, item.DGIndicator);
			}
		}

		void GenerateFTXSegment(SegmentGroup30 sG30, TextSubjectCodeQualifierList qualifier, string text)
		{
			GenerateFTXSegment(sG30, qualifier, text, "");
		}

		void GenerateFTXSegment(SegmentGroup30 sG30, TextSubjectCodeQualifierList qualifier, string text1, string text2)
		{
			if (!string.IsNullOrEmpty(text1) || !string.IsNullOrEmpty(text2))
			{
				FTXSegment fTX = sG30.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectCodeQualifier = qualifier;
				if (text2.Length > 0)
				{
					fTX.TextLiteral.FreeText1 = text1;
					fTX.TextLiteral.FreeText2 = text2;
				}
				else
				{
					TextSplitElegantly splitter = new TextSplitElegantly(35, 5);
					splitter.Text = text1;
					fTX.TextLiteral.FreeText1 = splitter[0];
					fTX.TextLiteral.FreeText2 = splitter[1];
					fTX.TextLiteral.FreeText3 = splitter[2];
					fTX.TextLiteral.FreeText4 = splitter[3];
					fTX.TextLiteral.FreeText5 = splitter[4];
				}
			}
		}

		#endregion

		#region Dates

		protected virtual void GenerateGroup30_DTM(SegmentGroup30 sG30, ICusItem item)
		{
		}

		#endregion

		#region Line Locations

		protected virtual void GenerateGroup30_LOC(SegmentGroup30 sG30, ICusItem item)
		{
			GenerateCountryOfOriginSegment(sG30, item);
			GenerateLotNumberSegments(sG30, item);
		}

		protected virtual void GenerateCountryOfOriginSegment(SegmentGroup30 sG30, ICusItem item)
		{
			GenerateLineLOCSegment(sG30, LocationFunctionCodeQualifierList.CountryOfOrigin, item.CountryOfOriginCode);
		}

		protected virtual void GenerateLotNumberSegments(SegmentGroup30 sG30, ICusItem item)
		{
			GenerateCurrentLotNumberSegment(sG30, item);
			GeneratePreviousLotNumberSegment(sG30, item);
		}

		void GenerateCurrentLotNumberSegment(SegmentGroup30 sG30, ICusItem item)
		{
			if (!item.CurrentLotNumber.IsEmpty)
			{
				GenerateLineLOCSegment(sG30, LocationFunctionCodeQualifierList.Warehouse, item.CurrentLotNumber);
			}
		}

		void GeneratePreviousLotNumberSegment(SegmentGroup30 sG30, ICusItem item)
		{
			if (!item.PreviousLotNumber.IsEmpty)
			{
				GenerateLineLOCSegment(sG30, LocationFunctionCodeQualifierList.CargoFacilityLocation, item.PreviousLotNumber);
			}
		}

		void GenerateLineLOCSegment(SegmentGroup30 sG30, LocationFunctionCodeQualifierList qualifier, string locationCode)
		{
			if (!string.IsNullOrEmpty(locationCode))
			{
				LOCSegment lOC = sG30.LOC.InstantiateAChildAndAddItToChildrenCollection();
				lOC.LocationFunctionCodeQualifier = qualifier;
				lOC.LocationIdentification.LocationNameCode = locationCode;
			}
		}

		#endregion

		#region Line Measurements

		protected virtual void GenerateGroup30_MEA(SegmentGroup30 sG30, ICusItem item)
		{
			GenerateItemMeasurementSegments(sG30, item);
		}

		void GenerateItemMeasurementSegments(SegmentGroup30 sG30, ICusItem item)
		{
			GenerateHSQuantitySegment(sG30, item);
			if (item.TotalDutiableQuantity > 0 || item.UnitDutiableQuantity > 0)
			{
				GenerateTotalDutiableSegment(sG30, item);
			}

			if (item.PercentageOfAlcohol > 0m)
			{
				GeneratePercentageOfAlcoholSegment(sG30, item);
			}

			if (item.DutyUnitRate > 0m || item.ExciseUnitRate > 0m)
			{
				GenerateDutiableQtyWgtVolSegment(sG30, item);
			}

			if (item.ProductCodes != null)
			{
				foreach (ICusProductCode productCode in item.ProductCodes)
				{
					if (productCode.ProductCodeQty > 0)
					{
						GenerateCASCProductQtySegment(sG30, productCode.ProductCodeUnitType, productCode.ProductCodeQty);
					}
				}
			}
		}

		void GenerateHSQuantitySegment(SegmentGroup30 sG30, ICusItem item)
		{
			if (item.HSQuantity > 0)
			{
				string unitOfQty = (item.HSQuantityUnitType == "VAL" || item.HSQuantityUnitType.IsEmpty) ? item.InvoiceUQ : item.HSQuantityUnitType;
				GenerateLineMEASegment(sG30, MeasurementPurposeCodeQualifierList.CustomsLineItemMeasurement, unitOfQty, item.HSQuantity);
			}
		}

		protected void GenerateTotalDutiableSegment(SegmentGroup30 sG30, ICusItem item)
		{
			GenerateTotalDutiableSegment(sG30, item, false);
		}

		protected void GenerateTotalDutiableSegment(SegmentGroup30 sG30, ICusItem item, bool oneLot)
		{
			if (item.DutyUnitRate > 0m || item.ExciseUnitRate > 0m)
			{
				GenerateLineMEASegment(sG30, MeasurementPurposeCodeQualifierList.Measurement, item.TotalDutiableQuantityUnitType, item.TotalDutiableQuantity, oneLot);
			}
			else
			{
				GenerateLineMEASegment(sG30, MeasurementPurposeCodeQualifierList.Measurement, item.UnitDutiableQuantityUnitType, item.UnitDutiableQuantity, oneLot);
			}
		}

		void GeneratePercentageOfAlcoholSegment(SegmentGroup30 sG30, ICusItem item)
		{
			GenerateLineMEASegment(sG30, MeasurementPurposeCodeQualifierList.AlcoholContent, "LPA", item.PercentageOfAlcohol, SGConstants.NumericFormatting.DecimalPlacesForPercentageValues, false);
		}

		void GenerateDutiableQtyWgtVolSegment(SegmentGroup30 sG30, ICusItem item)
		{
			GenerateLineMEASegment(sG30, MeasurementPurposeCodeQualifierList.ItemWeight, item.UnitDutiableQuantityUnitType, item.UnitDutiableQuantity);
		}

		void GenerateCASCProductQtySegment(SegmentGroup30 sG30, string productQtyUnit, decimal productQty)
		{
			GenerateLineMEASegment(sG30, MeasurementPurposeCodeQualifierList.UnitOfMeasureUsedForOrderedQuantities, productQtyUnit, productQty);
		}

		protected void GenerateLineMEASegment(SegmentGroup30 sG30, MeasurementPurposeCodeQualifierList qualifier, string unitOfQty, decimal qtyValue)
		{
			GenerateLineMEASegment(sG30, qualifier, unitOfQty, qtyValue, false);
		}

		protected void GenerateLineMEASegment(SegmentGroup30 sG30, MeasurementPurposeCodeQualifierList qualifier, string unitOfQty, decimal qtyValue, bool oneLot)
		{
			GenerateLineMEASegment(sG30, qualifier, unitOfQty, qtyValue, SGConstants.NumericFormatting.DecimalPlacesForMeasurementValues, oneLot);
		}

		protected void GenerateLineMEASegment(SegmentGroup30 sG30, MeasurementPurposeCodeQualifierList qualifier, string unitOfQty, decimal qtyValue, int decimals, bool oneLot)
		{
			if (qtyValue > 0)
			{
				MEASegment mEA = sG30.MEA.InstantiateAChildAndAddItToChildrenCollection();
				mEA.MeasurementPurposeCodeQualifier = qualifier;
				mEA.ValueRange.MeasurementUnitCode = unitOfQty;
				mEA.ValueRange.Measure = oneLot ? SGConstants.OneLot : Utilities.FormatNumber(qtyValue, decimals);
			}
		}

		#endregion

		#region Segment Group 31

		protected virtual void GenerateSegmentGroup31(SegmentGroup30 sG30, ICusItem item)
		{
			SegmentGroup31 lastSG31 = null;
			if ((item.IsLiquor || item.IsTobacco) ||
							(item.PackOuterQuantity > 0 || item.PackInQuantity > 0 || item.PackInnerQuantity > 0 || item.PackInmostQuantity > 0))
			{
				//must be in this order
				if (item.PackOuterQuantity > 0)
				{
					lastSG31 = GenerateLinePACSegment(sG30, PackagingLevelCodeList.Outer, item.PackOuterUnitType, item.PackOuterQuantity);
				}

				if (item.PackInQuantity > 0)
				{
					lastSG31 = GenerateLinePACSegment(sG30, PackagingLevelCodeList.Intermediate, item.PackInUnitType, item.PackInQuantity);
				}

				if (item.PackInnerQuantity > 0)
				{
					lastSG31 = GenerateLinePACSegment(sG30, PackagingLevelCodeList.Inner, item.PackInnerUnitType, item.PackInnerQuantity);
				}

				if (item.PackInmostQuantity > 0)
				{
					lastSG31 = GenerateLinePACSegment(sG30, PackagingLevelCodeList.ShipmentLevel, item.PackInmostUnitType, item.PackInmostQuantity);
				}

				if (lastSG31 != null)
				{
					if (!item.E_SDNPIndicator.IsEmpty)
					{
						PCISegment pCI = lastSG31.Group32.InstantiateAChildAndAddItToChildrenCollection().PCI.InstantiateAChildAndAddItToChildrenCollection();
						pCI.MarkingInstructionsCode = MarkingInstructionsCodeList.LegalRequirements;
						pCI.MarksLabels.ShippingMarksDescription1 = item.E_SDNPIndicator;
					}
				}
			}

			if (!item.MarksAndNumbers.IsEmpty)
			{
				if (lastSG31 == null)
				{
					lastSG31 = sG30.Group31.InstantiateAChildAndAddItToChildrenCollection();
					PACSegment pAC = lastSG31.PAC.InstantiateAChildAndAddItToChildrenCollection();
					pAC.PackagingDetails.PackagingRelatedDescriptionCode = PackagingRelatedDescriptionCodeList.ProductMarking;
				}

				GenerateSegmentGroup32(lastSG31, item.MarksAndNumbers);
			}
		}

		SegmentGroup31 GenerateLinePACSegment(SegmentGroup30 sG30, PackagingLevelCodeList qualifier, string unitOfQty, decimal qtyValue)
		{
			SegmentGroup31 sG31 = sG30.Group31.InstantiateAChildAndAddItToChildrenCollection();
			PACSegment pAC = sG31.PAC.InstantiateAChildAndAddItToChildrenCollection();
			pAC.PackagingDetails.PackagingLevelCode = qualifier;
			pAC.PackageQuantity = qtyValue.ToString();
			pAC.PackageType.PackageTypeDescriptionCode = unitOfQty;
			return sG31;
		}

		#region Segment Group 32

		protected virtual void GenerateSegmentGroup32(SegmentGroup31 sG31, string marksAndNumbers)
		{
			SegmentGroup32 sG32 = sG31.Group32.InstantiateAChildAndAddItToChildrenCollection();
			PCISegment pCI = sG32.PCI.InstantiateAChildAndAddItToChildrenCollection();
			pCI.MarkingInstructionsCode = MarkingInstructionsCodeList.MarkFreeText;
			TextSplitElegantly splitter = new TextSplitElegantly(17, 3);
			splitter.Text = marksAndNumbers;
			pCI.MarksLabels.ShippingMarksDescription1 = splitter[0];
			pCI.MarksLabels.ShippingMarksDescription2 = splitter[1];
			pCI.MarksLabels.ShippingMarksDescription3 = splitter[2];
		}

		#endregion

		#endregion

		#region Segment Group 33

		protected virtual void GenerateSegmentGroup33(SegmentGroup30 sG30, ICusItem item)
		{
			GenerateCIF_FOBSegment(sG30, item);
			GenerateUnitPriceSegment(sG30, item);
			GenerateLastSellingPriceSegment(sG30, item);
			GenerateOtherCharges(sG30, item);
		}

		protected void GenerateCIF_FOBSegment(SegmentGroup30 sG30, ICusItem item)
		{
			SegmentGroup33 sG33 = sG30.Group33.InstantiateAChildAndAddItToChildrenCollection();
			GenerateLineMOASegment(sG33, MonetaryAmountTypeCodeQualifierList.FobValue, Core.Constants.CurrencyCodes.Singapore, item.CustomsValue, 1m);
		}

		void GenerateUnitPriceSegment(SegmentGroup30 sG30, ICusItem item)
		{
			if (item.DutyPercentageRate > 0 || item.ExcisePercentageRate > 0)//only required for goods that are subject to ad-valorem duties
			{
				SegmentGroup33 sG33 = sG30.Group33.InstantiateAChildAndAddItToChildrenCollection();
				GenerateLineMOASegment(sG33, MonetaryAmountTypeCodeQualifierList.UnitPrice, item.InvoiceCurrency, item.UnitPrice, item.InvoiceCurrExchangeRate);
			}
		}

		void GenerateLastSellingPriceSegment(SegmentGroup30 sG30, ICusItem item)
		{
			if (item.LSPValue > 0)
			{
				SegmentGroup33 sG33 = sG30.Group33.InstantiateAChildAndAddItToChildrenCollection();
				GenerateLineMOASegment(sG33, MonetaryAmountTypeCodeQualifierList.AssignedCustomsValue, Core.Constants.CurrencyCodes.Singapore, item.LSPValue, 1m);
			}
		}

		void GenerateOtherCharges(SegmentGroup30 sG30, ICusItem item)
		{
			if (item.OptionalItemCharge != null)
			{
				if (item.OptionalItemCharge.Amount > 0)
				{
					SegmentGroup33 sG33 = sG30.Group33.InstantiateAChildAndAddItToChildrenCollection();
					GenerateLineMOASegment(sG33, MonetaryAmountTypeCodeQualifierList.OtherCharges, item.OptionalItemCharge.CurrencyCode, item.OptionalItemCharge.Amount, item.OptionalItemCharge.ExchangeRate);
				}
			}
		}

		void GenerateLineMOASegment(SegmentGroup33 sG33, MonetaryAmountTypeCodeQualifierList qualifier, ZString currencyCode, ZDecimal amount, ZDecimal exchangeRate)
		{
			MOASegment mOA = sG33.MOA.InstantiateAChildAndAddItToChildrenCollection();
			mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = qualifier;

			int decimalPlaces = qualifier == MonetaryAmountTypeCodeQualifierList.UnitPrice ? SGConstants.NumericFormatting.DecimalPlacesForUnitPriceAmountValues : SGConstants.NumericFormatting.DecimalPlacesForAmountValues;

			mOA.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(amount, decimalPlaces);
			if (qualifier == MonetaryAmountTypeCodeQualifierList.UnitPrice || qualifier == MonetaryAmountTypeCodeQualifierList.OtherCharges)
			{
				mOA.MonetaryAmount.CurrencyIdentificationCode = currencyCode;
			}

			if (exchangeRate > 0 && exchangeRate != 1m)
			{
				GenerateLineExchangeRateSegment(sG33, exchangeRate);
			}
		}

		void GenerateLineExchangeRateSegment(SegmentGroup33 sG33, decimal currencyExchRate)
		{
			CUXSegment cUX = sG33.Group34.InstantiateAChildAndAddItToChildrenCollection().CUX.InstantiateAChildAndAddItToChildrenCollection();
			cUX.CurrencyExchangeRate = Utilities.FormatNumber(currencyExchRate, SGConstants.NumericFormatting.DecimalPlacesForExchangeRateValues);
		}

		#endregion

		#region Segment Group 35

		protected virtual void GenerateSegmentGroup35(SegmentGroup30 sG30, ICusItem item)
		{
			if (SupportsInvoiceNumberSegment)
			{
				GenerateInvoiceNoSegment(sG30, item);
			}

			SegmentGroup35 sG35 = null;
			bool gINSegmentAdded = false;

			//product code + gin
			foreach (ICusProductCode productCode in item.ProductCodes)
			{
				sG35 = sG30.Group35.InstantiateAChildAndAddItToChildrenCollection();
				GenerateProductCodeSegment(sG35, productCode.ProductCode);

				//first occurance only currently
				if (!gINSegmentAdded)
				{
					GenerateGoodsIdentityNumberSegment(sG35, item);
					gINSegmentAdded = true;
				}
			}

			//gin where no product code
			if (sG35 == null && item.CASCCodes1 != null && item.CASCCodes1.Count > 0)
			{
				sG35 = sG30.Group35.InstantiateAChildAndAddItToChildrenCollection();

				if (sG35.RFF.Count == 0) //Note 3
				{
					RFFSegment rFF = sG35.RFF.InstantiateAChildAndAddItToChildrenCollection();
					rFF.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.GoodsItemInformation;
				}

				GenerateGoodsIdentityNumberSegment(sG35, item);
			}

			//MV specific
			if (SupportsSESegment && item.IsMotorVehicle && item.EngineCapacity > 0)
			{
				SegmentGroup35 sG35_EngineCapacity = sG30.Group35.InstantiateAChildAndAddItToChildrenCollection();
				RFFSegment rFF = sG35_EngineCapacity.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rFF.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.SerialNumber;

				if (SupportsRegistrationDateSegment)
				{
					GenerateRegistrationDateSegment(sG35_EngineCapacity, item.DateOfFirstRegistration);
				}
				GenerateGroup36(sG35_EngineCapacity, item.EngineCapacity, item.EngineCapacityUnit);
			}

			if (SupportsVehicleRego && !item.RegistrationNumberSG.IsEmpty)
			{
				SegmentGroup35 sG35_RegoNo = sG30.Group35.InstantiateAChildAndAddItToChildrenCollection();
				GenerateMotorVehicleRegistrationSegment(sG35_RegoNo, item.RegistrationNumberSG);
			}

			//Sea Stores
			if (SupportsSeastoresSegments && sgCusdec.IsSeaStoreDeclaration)
			{
				AddSeaStoreDetails(sG30);
			}

			//Strategic Goods
			if (SupportsStrategicGoodsSegments && item.IsStrategic)
			{
				AddStrategicDetails(sG30, item);
			}
		}

		protected virtual bool SupportsInvoiceNumberSegment
		{
			get { return false; }
		}

		protected virtual bool SupportsRegistrationDateSegment
		{
			get { return false; }
		}

		protected virtual bool SupportsVehicleRego
		{
			get { return false; }
		}

		protected virtual bool SupportsSESegment
		{
			get { return true; }
		}

		protected virtual bool SupportsSeastoresSegments
		{
			get { return true; }
		}

		protected virtual bool SupportsStrategicGoodsSegments
		{
			get { return false; }
		}

		protected void GenerateInvoiceNoSegment(SegmentGroup30 sG30, ICusItem item)
		{
			if (!item.InvoiceNumber.IsEmpty)
			{
				SegmentGroup35 sG35 = sG30.Group35.InstantiateAChildAndAddItToChildrenCollection();
				GenerateGroup35RFFSegment(sG35, ReferenceCodeQualifierList.InvoiceNumber, item.InvoiceNumber);
			}
		}

		protected void GenerateProductCodeSegment(SegmentGroup35 sG35, string productCode)
		{
			GenerateGroup35RFFSegment(sG35, ReferenceCodeQualifierList.GovernmentAgencyReferenceNumber, productCode);
		}

		protected void GenerateSerialNoSegment(SegmentGroup35 sG35)
		{
			RFFSegment rFF = sG35.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rFF.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.SerialNumber;
		}

		protected virtual void GenerateMotorVehicleRegistrationSegment(SegmentGroup35 sG35, string regoNo)
		{
			GenerateGroup35RFFSegment(sG35, ReferenceCodeQualifierList.MotorVehicleIdentificationNumber, regoNo);
		}

		protected void GenerateRegistrationDateSegment(SegmentGroup35 sG35, ZDate regoDate)
		{
			if (!regoDate.IsEmpty)
			{
				GenerateGroup35DTMSegment(sG35, DateOrTimeOrPeriodFunctionCodeQualifierList.DateOfFirstRegistration, regoDate);
			}
		}

		protected void GenerateGroup35RFFSegment(SegmentGroup35 sG35, ReferenceCodeQualifierList qualifier, ZString identifier)
		{
			if (!identifier.IsEmpty)
			{
				RFFSegment rFF = sG35.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rFF.Reference.ReferenceCodeQualifier = qualifier;
				rFF.Reference.ReferenceIdentifier = identifier;
			}
		}

		protected void GenerateGroup35DTMSegment(SegmentGroup35 sG35, DateOrTimeOrPeriodFunctionCodeQualifierList qualifier, ZDate date)
		{
			if (!date.IsEmpty)
			{
				DTMSegment dTM = sG35.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dTM.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = qualifier;
				dTM.DateTimePeriod.DateOrTimeOrPeriodText = date.ToString("yyyyMMdd");
				dTM.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmdd;
			}
		}

		protected void GenerateEndUseCodesSegment(SegmentGroup35 sG35, ICusItem item)
		{
			GINSegment gIN = sG35.GIN.InstantiateAChildAndAddItToChildrenCollection();
			gIN.ObjectIdentificationCodeQualifier = ObjectIdentificationCodeQualifierList.ValueListSubset;
			gIN.IdentityNumberRange1.ObjectIdentifier1 = item.EndUseCode1;
			gIN.IdentityNumberRange2.ObjectIdentifier1 = item.EndUseCode2;
			gIN.IdentityNumberRange3.ObjectIdentifier1 = item.EndUseCode3;
		}

		protected void GenerateCrewAndVoyageDurationSegment(SegmentGroup35 sG35)
		{
			GINSegment gIN = sG35.GIN.InstantiateAChildAndAddItToChildrenCollection();
			gIN.ObjectIdentificationCodeQualifier = ObjectIdentificationCodeQualifierList.ValueListSubset;
			gIN.IdentityNumberRange1.ObjectIdentifier1 = sgCusdec.NumberOfCrew.ToString();
			gIN.IdentityNumberRange2.ObjectIdentifier1 = sgCusdec.VoyageDuration.ToString();
		}

		protected void GenerateGoodsIdentityNumberSegment(SegmentGroup35 sG35, ICusItem item)
		{
			if (item.CASCCodes1 != null)
			{
				for (int i = 0; i < item.CASCCodes1.Count; i++)
				{
					if (!item.CASCCodes1[i].CY_Data.IsEmpty)
					{
						GINSegment gIN = sG35.GIN.InstantiateAChildAndAddItToChildrenCollection();
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

		protected void GenerateGroup36(SegmentGroup35 g35, ZDecimal engineCapacity, ZString engineCapacityUnit)
		{
			if (engineCapacity > 0)
			{
				SegmentGroup36 sG36 = g35.Group36.InstantiateAChildAndAddItToChildrenCollection();
				IMDSegment iMD = sG36.IMD.InstantiateAChildAndAddItToChildrenCollection();
				iMD.ItemCharacteristic.ItemCharacteristicCode = ItemCharacteristicCodeList.Product;

				FTXSegment fTX = sG36.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.ProductInformation;
				fTX.TextLiteral.FreeText1 = engineCapacity.ToString("0.00", CultureInfo.InvariantCulture);
				fTX.TextLiteral.FreeText2 = engineCapacityUnit;
			}
		}

		protected void AddStrategicDetails(SegmentGroup30 g30, ICusItem item)
		{
			SegmentGroup35 g35 = g30.Group35.InstantiateAChildAndAddItToChildrenCollection();
			GenerateGroup35RFFSegment(g35, ReferenceCodeQualifierList.GovernmentAgencyReferenceNumber, item.CategoryCode);

			GenerateEndUseCodesSegment(g35, item);
			GenerateStrategicGoodsGroup36(g35, item);
		}

		protected void AddSeaStoreDetails(SegmentGroup30 sG30)
		{
			SegmentGroup35 sG35 = sG30.Group35.InstantiateAChildAndAddItToChildrenCollection();
			GenerateGroup35RFFSegment(sG35, ReferenceCodeQualifierList.GovernmentAgencyReferenceNumber, SGConstants.SeaStore);
			GenerateCrewAndVoyageDurationSegment(sG35);
		}

		protected void GenerateStrategicGoodsGroup36(SegmentGroup35 g35, ICusItem item)
		{
			if (!item.EndUseDescription.IsEmpty)
			{
				SegmentGroup36 g36 = g35.Group36.InstantiateAChildAndAddItToChildrenCollection();
				IMDSegment iMD = g36.IMD.InstantiateAChildAndAddItToChildrenCollection();
				iMD.ItemCharacteristic.ItemCharacteristicCode = ItemCharacteristicCodeList.EndUseApplication;

				FTXSegment fTX = g36.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.ProductApplication;
				TextSplitElegantly strings = new TextSplitElegantly(35, 3);
				strings.Text = item.EndUseDescription;
				fTX.TextLiteral.FreeText1 = strings[0];
				fTX.TextLiteral.FreeText2 = strings[1];
				fTX.TextLiteral.FreeText3 = strings[2];
			}
		}

		#endregion

		#region Segment Group 37

		protected virtual void GenerateSegmentGroup37(SegmentGroup30 sG30, ICusItem item)
		{
			if (sgCusdec.InwardTransportCode == SGConstants.TransportCodes.Sea || sgCusdec.InwardTransportCode == SGConstants.TransportCodes.Air)
			{
				GenerateGroup37DOCSegment(sG30, DocumentNameCodeList.HouseWaybill, item.InwardHAWB);
			}
		}

		protected void GenerateGroup37DOCSegment(SegmentGroup30 sG30, DocumentNameCodeList qualifier, ZString waybill)
		{
			if (!waybill.IsEmpty)
			{
				DOCSegment dOC = sG30.Group37.InstantiateAChildAndAddItToChildrenCollection().DOC.InstantiateAChildAndAddItToChildrenCollection();
				dOC.DocumentMessageName.DocumentNameCode = qualifier;
				dOC.DocumentMessageDetails.DocumentIdentifier = waybill;
			}
		}

		#endregion

		#region Segment Group 39

		protected virtual void GenerateSegmentGroup39(SegmentGroup30 sG30, ICusItem item)
		{
		}

		#endregion

		#region Segment Group 41

		protected virtual void GenerateSegmentGroup41(SegmentGroup30 sG30, ICusItem item)
		{
			if (!sgCusdec.AdditionalMessageInformation.RefundCode.IsEmpty)
			{
				PopulateRefundSegments(sG30, item);
			}
			else
			{
				if (item.DutyAmount > 0)
				{
					PopulateCustomsDutySegments(sG30, item);
				}

				if (item.ExciseAmount > 0)
				{
					PopulateCustomsExciseSegments(sG30, item);
				}

				PopulateGSTSegments(sG30, item);
			}
		}

		protected void PopulateCustomsDutySegments(SegmentGroup30 sG30, ICusItem item)
		{
			SegmentGroup41 sG41 = sG30.Group41.InstantiateAChildAndAddItToChildrenCollection();
			decimal dutyRate = !item.DutyPercentageRate.IsEmpty ? item.DutyPercentageRate : item.DutyUnitRate;
			GenerateTAXSegment(sG41, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, item.DutyRateUnit, dutyRate, item.PreferenceIndicator);
			GenerateSG41MOASegment(sG41, MonetaryAmountTypeCodeQualifierList.DutyAmount, item.DutyAmount);
		}

		protected void PopulateCustomsExciseSegments(SegmentGroup30 sG30, ICusItem item)
		{
			SegmentGroup41 sG41 = sG30.Group41.InstantiateAChildAndAddItToChildrenCollection();
			decimal exciseRate = !item.ExcisePercentageRate.IsEmpty ? item.ExcisePercentageRate : item.ExciseUnitRate;
			GenerateTAXSegment(sG41, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, item.DutyRateUnit, exciseRate, ZString.Empty);
			GenerateSG41MOASegment(sG41, MonetaryAmountTypeCodeQualifierList.DutyTaxOrFeeAmount, item.ExciseAmount);
		}

		protected void PopulateGSTSegments(SegmentGroup30 sG30, ICusItem item)
		{
			SegmentGroup41 sG41 = sG30.Group41.InstantiateAChildAndAddItToChildrenCollection();
			TAXSegment tAX = sG41.TAX.InstantiateAChildAndAddItToChildrenCollection();
			tAX.DutyOrTaxOrFeeFunctionCodeQualifier = DutyOrTaxOrFeeFunctionCodeQualifierList.Tax;
			tAX.DutyTaxFeeDetail.DutyOrTaxOrFeeRate = item.GSTRate.ToString();
			GenerateSG41MOASegment(sG41, MonetaryAmountTypeCodeQualifierList.TaxAmount, item.GSTPayable);
		}

		protected void GenerateTAXSegment(SegmentGroup41 sG41, DutyOrTaxOrFeeFunctionCodeQualifierList qualifier, string rateUnit, decimal rate, string preferenceIndicator)
		{
			TAXSegment tAX = sG41.TAX.InstantiateAChildAndAddItToChildrenCollection();
			tAX.DutyOrTaxOrFeeFunctionCodeQualifier = qualifier;
			if (qualifier == DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty || qualifier == DutyOrTaxOrFeeFunctionCodeQualifierList.TaxRelatedInformation)
			{
				tAX.DutyTaxFeeType.DutyOrTaxOrFeeTypeName = preferenceIndicator;
			}

			if (rate > 0)
			{
				tAX.DutyTaxFeeDetail.DutyOrTaxOrFeeRateCode = rateUnit;
				tAX.DutyTaxFeeDetail.DutyOrTaxOrFeeRate = Utilities.FormatNumber(rate, SGConstants.NumericFormatting.DecimalPlacesForTaxRateValues);
			}
		}

		void GenerateSG41MOASegment(SegmentGroup41 sG41, MonetaryAmountTypeCodeQualifierList qualifier, decimal monetaryValue)
		{
			MOASegment mOA = sG41.MOA.InstantiateAChildAndAddItToChildrenCollection();
			mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = qualifier;
			mOA.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(monetaryValue, SGConstants.NumericFormatting.DecimalPlacesForAmountValues);
		}

		void PopulateRefundSegments(SegmentGroup30 sG30, ICusItem item)
		{
			if (item.ItemDutyRefund > 0)
			{
				PopulateItemRefundAmount(sG30, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, DutyOrTaxOrFeeTypeNameCodeList.CustomsDuty, item.ItemDutyRefund);
			}

			if (item.ItemExciseRefund > 0)
			{
				PopulateItemRefundAmount(sG30, DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, DutyOrTaxOrFeeTypeNameCodeList.ExciseDuty, item.ItemExciseRefund);
			}

			if (item.ItemGSTRefund > 0)
			{
				PopulateItemRefundAmount(sG30, DutyOrTaxOrFeeFunctionCodeQualifierList.Tax, DutyOrTaxOrFeeTypeNameCodeList.GoodsAndServicesTax, item.ItemGSTRefund);
			}
		}

		void PopulateItemRefundAmount(SegmentGroup30 sG30, DutyOrTaxOrFeeFunctionCodeQualifierList qualifier, DutyOrTaxOrFeeTypeNameCodeList taxFeeType, decimal amount)
		{
			SegmentGroup41 sG41 = sG30.Group41.InstantiateAChildAndAddItToChildrenCollection();
			TAXSegment tAX = sG41.TAX.InstantiateAChildAndAddItToChildrenCollection();
			tAX.DutyOrTaxOrFeeFunctionCodeQualifier = qualifier;
			tAX.DutyTaxFeeType.DutyOrTaxOrFeeTypeNameCode = taxFeeType;
			GenerateSG41MOASegment(sG41, MonetaryAmountTypeCodeQualifierList.Refund, amount);
		}

		#endregion

		#endregion

		#endregion

		#endregion

		#region Summary Section

		protected override void GenerateSummarySection(CUSDECMessage cusdecEdifactMsg)
		{
			GenerateSummaryUNS(cusdecEdifactMsg.UNS2);
			GenerateCNT(cusdecEdifactMsg);
			GenerateSegmentGroup49(cusdecEdifactMsg.Group49);
			GenerateUNT(cusdecEdifactMsg);
		}

		void GenerateSummaryUNS(UNSSegmentMessageSection uns2Section)
		{
			var uns = uns2Section.InstantiateAChildAndAddItToChildrenCollection();
			uns.SectionIdentification = UnsSummary;
		}

		protected virtual void GenerateCNT(CUSDECMessage cusdecEdifactMsg)
		{
			var cntSection = cusdecEdifactMsg.CNT;
			var cnt = cntSection.InstantiateAChildAndAddItToChildrenCollection();
			cnt.Control.ControlTotalTypeCodeQualifier = ControlTotalTypeCodeQualifierList.NumberOfCustomsItemDetailLines;
			cnt.Control.ControlTotalQuantity = cusdecEdifactMsg.Group30.Count.ToString();
		}

		protected virtual void GenerateSegmentGroup49(SegmentGroup49MessageSection sg49Section)
		{
		}

		void GenerateUNT(CUSDECMessage cusdecEdifactMsg)
		{
			var untSection = cusdecEdifactMsg.UNT;
			var unt = untSection.InstantiateAChildAndAddItToChildrenCollection();
			unt.MessageReferenceNumber = cusdecEdifactMsg.UNH[0].MessageReferenceNumber;
			unt.NumberOfSegmentsInTheMessage = cusdecEdifactMsg.CountIncludingUNT.ToString();
		}

		#endregion

		#endregion

		public const string DocDocumentAttachment = "DOCUMENT ATTACHMENT";

		#endregion
	}
}
