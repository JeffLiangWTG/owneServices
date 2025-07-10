using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.SG.Business.CustomsMessaging;
using Enterprise.Edifact.D05B.Elements;
using Enterprise.Edifact.D05B.Messages.TCODEC;
using Enterprise.Edifact.D05B.Segments;
using Enterprise.Edifact.Utilities;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	public class TCODEC : TcodecBase<TCODECMessage>
	{
		public TCODEC(ITCODEC coCusdec)
			: base(coCusdec)
		{
		}

		#region Implementation

		protected override string UnhAssociationAssignedCode
		{
			get { return "040"; }
		}

		#region Message Segments

		#region Header Section

		protected override void GenerateHeaderSection(TCODECMessage edifactMsg)
		{
			GenerateUNH(edifactMsg);
			GenerateBGM(edifactMsg);
			GenerateCST(edifactMsg);
			GenerateSegmentGroup1(edifactMsg);
			GenerateSegmentGroup2(edifactMsg);
			GenerateSegmentGroup3(edifactMsg);
		}

		#region Header Section Non Grouped Segments

		void GenerateUNH(TCODECMessage edifactMsg)
		{
			UNHSegment uNH = edifactMsg.UNH.InstantiateAChildAndAddItToChildrenCollection();
			uNH.MessageReferenceNumber = "1";  // This (& UNT.MessageReferenceNumber) need to be filled in by interchange creation
			uNH.MessageIdentifier.MessageType = UnhMessageTypeIdentifier;
			uNH.MessageIdentifier.MessageVersionNumber = UnhMessageVersionNumber;
			uNH.MessageIdentifier.MessageReleaseNumber = UnhMessageReleaseNumber;
			uNH.MessageIdentifier.ControllingAgency = UnhControllingAgency;
			uNH.MessageIdentifier.AssociationAssignedCode = UnhAssociationAssignedCode;
			uNH.CommonAccessReference = MessageType;
		}

		void GenerateBGM(TCODECMessage edifactMsg)
		{
			BGMSegment bGM = edifactMsg.BGM.InstantiateAChildAndAddItToChildrenCollection();
			bGM.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.CertificateOfOriginApplicationFor;
			bGM.DocumentMessageName.DocumentName = BgmApplicationType;
			bGM.DocumentMessageIdentification.DocumentIdentifier = "<<MSGNO PLACEHOLDER>>";
			bGM.MessageFunctionCode = MessageFunctionCodeList.Original;
		}

		void GenerateCST(TCODECMessage edifactMsg)
		{
			CSTSegment cST = edifactMsg.CST.InstantiateAChildAndAddItToChildrenCollection();
			cST.CustomsIdentityCodes1.CustomsGoodsIdentifier = CertificateOfOrigin.ApplicationProductType;
		}

		#endregion

		#region Segment Group 1

		void GenerateSegmentGroup1(TCODECMessage edifactMsg)
		{
			SegmentGroup1 sG1 = edifactMsg.Group1.InstantiateAChildAndAddItToChildrenCollection();
			GenerateMessageSenderMailboxSegment(sG1);
			if (!CertificateOfOrigin.AdditionalInformation.IsEmpty)
			{
				GenerateAdditionalInfoSegment(sG1);
			}

			GenerateSupportingDocsSegments(sG1);

			if (!CertificateOfOrigin.PreviousPermitNumber.IsEmpty)
			{
				sG1 = edifactMsg.Group1.InstantiateAChildAndAddItToChildrenCollection();
				GeneratePreviousPermitNumberSegment(sG1);
			}
		}

		void GenerateMessageSenderMailboxSegment(SegmentGroup1 sG1)
		{
			RFFSegment rFF = sG1.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rFF.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.MessageSender;
			rFF.Reference.ReferenceIdentifier = CertificateOfOrigin.DeclarantId.Left(4) + "." + CertificateOfOrigin.DeclarantId;
		}

		void GeneratePreviousPermitNumberSegment(SegmentGroup1 sG1)
		{
			RFFSegment rFF = sG1.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rFF.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.RelatedDocumentNumber;
			rFF.Reference.ReferenceIdentifier = CertificateOfOrigin.PreviousPermitNumber;
		}

		void GenerateAdditionalInfoSegment(SegmentGroup1 sG1)
		{
			FTXSegment fTX = sG1.FTX.InstantiateAChildAndAddItToChildrenCollection();
			fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.Declaration;
			TextSplitElegantly splitter = new TextSplitElegantly(35, 5);
			splitter.Text = CertificateOfOrigin.AdditionalInformation;
			fTX.TextLiteral.FreeText1 = splitter[0];
			fTX.TextLiteral.FreeText2 = splitter[1];
			fTX.TextLiteral.FreeText3 = splitter[2];
			fTX.TextLiteral.FreeText4 = splitter[3];
			fTX.TextLiteral.FreeText5 = splitter[4];
		}

		void GenerateSupportingDocsSegments(SegmentGroup1 sG1)
		{
			if (CertificateOfOrigin.AdditionalMessageInformation != null && CertificateOfOrigin.AdditionalMessageInformation.SupportingDocuments != null)
			{
				foreach (ICusAttachment attachment in CertificateOfOrigin.AdditionalMessageInformation.SupportingDocuments)
				{
					DOCSegment dOC = sG1.DOC.InstantiateAChildAndAddItToChildrenCollection();
					dOC.DocumentMessageName.DocumentName = DocDocumentAttachment;
					dOC.DocumentMessageDetails.DocumentIdentifier = attachment.DocType;
					dOC.DocumentMessageDetails.DocumentSourceDescription = attachment.FileName.ToASCII();
				}
			}
		}

		#endregion

		#region Segment Group 2

		void GenerateSegmentGroup2(TCODECMessage edifactMsg)
		{
			SegmentGroup2 sG2 = edifactMsg.Group2.InstantiateAChildAndAddItToChildrenCollection();
			GenerateTransportInfoSegment(sG2);
			GenerateDepartureDateSegment(sG2);
			if (CertificateOfOrigin.ApplicationProductType == ApplicationProductTypeCodeList.Codes.TX)
			{
				GenerateEntryYearSegment(sG2);
			}

			GeneratePortOfDischargeSegment(sG2);
			GenerateCountryOfUltimateDestinationSegment(sG2);
			GenerateDonorCountrySegment(sG2);

			GenerateBrokerDeclarationSegment(sG2);
		}

		void GenerateTransportInfoSegment(SegmentGroup2 sG2)
		{
			TDTSegment tDT = sG2.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tDT.TransportStageCodeQualifier = TransportStageCodeQualifierList.MainCarriageTransport;
			tDT.ModeOfTransport.TransportModeNameCode = CertificateOfOrigin.OutwardTransportCode.ToString();
			if (CertificateOfOrigin.IsSea || CertificateOfOrigin.IsAir)
			{
				tDT.TransportIdentification.TransportMeansIdentificationNameIdentifier = CertificateOfOrigin.OutwardJourneyIdentifier;
				if (tDT.ModeOfTransport.TransportModeNameCode == Constants.TransportCodes.Sea)
				{
					tDT.TransportIdentification.TransportMeansIdentificationName = CertificateOfOrigin.OutwardTransportIdentifier;
				}
			}
		}

		void GenerateDepartureDateSegment(SegmentGroup2 sG2)
		{
			if (!CertificateOfOrigin.DepartureDate.IsEmpty)
			{
				DTMSegment dTM = sG2.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dTM.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansDepartureDateTime;
				dTM.DateTimePeriod.DateOrTimeOrPeriodText = CertificateOfOrigin.DepartureDate.ToString("yyyyMMdd");
				dTM.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmdd;
			}
		}

		void GenerateEntryYearSegment(SegmentGroup2 sG2)
		{
			if (!CertificateOfOrigin.YearOfEntry.IsEmpty)
			{
				DTMSegment dTM = sG2.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dTM.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.YearOfOccurrence;
				dTM.DateTimePeriod.DateOrTimeOrPeriodText = CertificateOfOrigin.YearOfEntry.ToString();
				dTM.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyy;
			}
		}

		void GenerateBrokerDeclarationSegment(SegmentGroup2 sG2)
		{
			GEISegment gEI = sG2.GEI.InstantiateAChildAndAddItToChildrenCollection();
			gEI.ProcessingInformationCodeQualifier = ProcessingInformationCodeQualifierList.ConsignmentTypeInformation;
			gEI.ProcessingIndicator.CodeListIdentificationCode = "Y";
		}

		void GeneratePortOfDischargeSegment(SegmentGroup2 sG2)
		{
			LOCSegment lOC = sG2.LOC.InstantiateAChildAndAddItToChildrenCollection();
			lOC.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.PlacePortOfDischarge;
			lOC.LocationIdentification.LocationNameCode = CertificateOfOrigin.PortOfDischarge;
		}

		void GenerateCountryOfUltimateDestinationSegment(SegmentGroup2 sG2)
		{
			LOCSegment lOC = sG2.LOC.InstantiateAChildAndAddItToChildrenCollection();
			lOC.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.CountryOfUltimateDestination;
			lOC.LocationIdentification.LocationNameCode = CertificateOfOrigin.CountryOfFinalDestination;
		}

		void GenerateDonorCountrySegment(SegmentGroup2 sG2)
		{
			if (!CertificateOfOrigin.DonorCountryCode.IsEmpty)
			{
				LOCSegment lOC = sG2.LOC.InstantiateAChildAndAddItToChildrenCollection();
				lOC.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.DonationActingCountry;
				lOC.LocationIdentification.LocationNameCode = CertificateOfOrigin.DonorCountryCode;
			}
		}

		#endregion

		#region Segment Group 3

		void GenerateSegmentGroup3(TCODECMessage edifactMsg)
		{
			PopulateDeclaringAgentSegment(edifactMsg.Group3);
			PopulateDeclarantSegments(edifactMsg.Group3);
			PopulateConsigneeSegment(edifactMsg.Group3);
			PopulateExporterSegment(edifactMsg.Group3);
			PopulateManufacturerSegment(edifactMsg.Group3);
			PopulateOutwardCarrierAgentSegment(edifactMsg.Group3);
			PopulateFreightForwarderSegment(edifactMsg.Group3);
		}

		void PopulateDeclaringAgentSegment(SegmentGroup3MessageSection sg3Section)
		{
			SegmentGroup3 sG3 = sg3Section.InstantiateAChildAndAddItToChildrenCollection();
			NADSegment nAD = sG3.NAD.InstantiateAChildAndAddItToChildrenCollection();
			nAD.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.DeclarantsAgentRepresentative;
			nAD.PartyIdentificationDetails.PartyIdentifier = GlbCompany.CurrentCompany.GC_CustomsRegistrationNo;
			TextSplitElegantly splitter = new TextSplitElegantly(35, 3);
			splitter.Text = GlbCompany.CurrentCompany.GC_Name;
			nAD.PartyName.PartyName1 = splitter[0];
			nAD.PartyName.PartyName2 = splitter[1];
			nAD.PartyName.PartyName3 = splitter[2];
		}

		void PopulateDeclarantSegments(SegmentGroup3MessageSection sg3Section)
		{
			SegmentGroup3 sG3 = sg3Section.InstantiateAChildAndAddItToChildrenCollection();
			NADSegment nAD = sG3.NAD.InstantiateAChildAndAddItToChildrenCollection();
			nAD.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.Declarant;
			TextSplitElegantly splitter = new TextSplitElegantly(35, 3);
			splitter.Text = CertificateOfOrigin.Declarant.Name;
			nAD.NameAndAddress.NameAndAddressDescription1 = splitter[0];
			nAD.NameAndAddress.NameAndAddressDescription2 = splitter[1];
			nAD.NameAndAddress.NameAndAddressDescription3 = splitter[2];

			CTASegment cTA = sG3.CTA.InstantiateAChildAndAddItToChildrenCollection();
			cTA.ContactFunctionCode = ContactFunctionCodeList.InformationContact;
			cTA.DepartmentOrEmployeeDetails.DepartmentOrEmployeeName = CertificateOfOrigin.Declarant.Code;

			COMSegment cOM = sG3.COM.InstantiateAChildAndAddItToChildrenCollection();
			cOM.CommunicationContact.CommunicationAddressCodeQualifier = CommunicationAddressCodeQualifierList.Telephone;
			cOM.CommunicationContact.CommunicationAddressIdentifier = CertificateOfOrigin.Declarant.Phone;
		}

		void PopulateConsigneeSegment(SegmentGroup3MessageSection sg3Section)
		{
			if (CertificateOfOrigin.Consignee != null)
			{
				GenerateGroup3NadSegment(sg3Section, PartyFunctionCodeQualifierList.Consignee, "", CertificateOfOrigin.Consignee.Name, CertificateOfOrigin.Consignee.Address.FullAddress);
			}
		}

		void PopulateExporterSegment(SegmentGroup3MessageSection sg3Section)
		{
			if (CertificateOfOrigin.Exporter != null)
			{
				GenerateGroup3NadSegment(sg3Section, PartyFunctionCodeQualifierList.Exporter, CertificateOfOrigin.Exporter.UEN, CertificateOfOrigin.Exporter.Name, CertificateOfOrigin.Exporter.Address.FullAddress);
			}
		}

		void PopulateManufacturerSegment(SegmentGroup3MessageSection sg3Section)
		{
			if (CertificateOfOrigin.Manufacturer != null)
			{
				GenerateGroup3NadSegment(sg3Section, PartyFunctionCodeQualifierList.ManufacturerOfGoods, CertificateOfOrigin.Manufacturer.UEN, CertificateOfOrigin.Manufacturer.Name, CertificateOfOrigin.Manufacturer.Address.FullAddress);
			}
		}

		void PopulateOutwardCarrierAgentSegment(SegmentGroup3MessageSection sg3Section)
		{
			if (CertificateOfOrigin.OutwardCarrierAgent != null)
			{
				GenerateGroup3NadSegment(sg3Section, PartyFunctionCodeQualifierList.Carrier, CertificateOfOrigin.OutwardCarrierAgent.UEN, CertificateOfOrigin.OutwardCarrierAgent.Name, "");
			}
		}

		void PopulateFreightForwarderSegment(SegmentGroup3MessageSection sg3Section)
		{
			if (CertificateOfOrigin.FreightForwarder != null)
			{
				GenerateGroup3NadSegment(sg3Section, PartyFunctionCodeQualifierList.FreightForwarder, CertificateOfOrigin.FreightForwarder.UEN, CertificateOfOrigin.FreightForwarder.Name, "");
			}
		}

		void GenerateGroup3NadSegment(SegmentGroup3MessageSection sg3Section, PartyFunctionCodeQualifierList qualifier, string uEN, string name, string address)
		{
			if (!string.IsNullOrEmpty(uEN) || !string.IsNullOrEmpty(name))
			{
				SegmentGroup3 sG3 = sg3Section.InstantiateAChildAndAddItToChildrenCollection();
				NADSegment nAD = sG3.NAD.InstantiateAChildAndAddItToChildrenCollection();
				nAD.PartyFunctionCodeQualifier = qualifier;
				if (uEN.Length > 0)
				{
					nAD.PartyIdentificationDetails.PartyIdentifier = uEN;
				}

				int segmentsAllowed = 3;
				if (qualifier == PartyFunctionCodeQualifierList.Consignee ||
					qualifier == PartyFunctionCodeQualifierList.Exporter ||
					qualifier == PartyFunctionCodeQualifierList.ManufacturerOfGoods)
				{
					segmentsAllowed = 2;
				}

				TextSplitElegantly splitter = new TextSplitElegantly(35, segmentsAllowed);
				splitter.Text = name;
				nAD.PartyName.PartyName1 = splitter[0];
				nAD.PartyName.PartyName2 = splitter[1];
				if (segmentsAllowed > 2)
				{
					nAD.PartyName.PartyName3 = splitter[2];
				}

				if (!string.IsNullOrEmpty(address))
				{
					splitter = new TextSplitElegantly(35, 4);
					splitter.Text = address;
					nAD.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = splitter[0];
					nAD.Street.StreetAndNumberOrPostOfficeBoxIdentifier2 = splitter[1];
					nAD.Street.StreetAndNumberOrPostOfficeBoxIdentifier3 = splitter[2];
					nAD.Street.StreetAndNumberOrPostOfficeBoxIdentifier4 = splitter[3];
				}
			}
		}

		#endregion

		#endregion

		#region Detail Section

		protected override void GenerateDetailSection(TCODECMessage edifactMsg)
		{
			GenerateDetailUNS(edifactMsg);
			GenerateSegmentGroup4(edifactMsg);
			GenerateSegmentGroup5(edifactMsg);
		}

		void GenerateDetailUNS(TCODECMessage edifactMsg)
		{
			UNSSegment uNS = edifactMsg.UNS1.InstantiateAChildAndAddItToChildrenCollection();
			uNS.SectionIdentification = UnsDetail;
		}

		#region Segment Group 4

		void GenerateSegmentGroup4(TCODECMessage edifactMsg)
		{
			if (!CertificateOfOrigin.CertificateType1.IsEmpty || !CertificateOfOrigin.CertificateType2.IsEmpty)
			{
				SegmentGroup4 sG4 = edifactMsg.Group4.InstantiateAChildAndAddItToChildrenCollection();
				DMSSegment dMS = sG4.DMS.InstantiateAChildAndAddItToChildrenCollection();
				dMS.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.CertificateOfOriginApplicationFor;

				var percCommContent1 = CertificateOfOrigin.PercCommContent1;
				if (percCommContent1 != 0)
				{
					PCDSegment pCD = sG4.PCD.InstantiateAChildAndAddItToChildrenCollection();
					pCD.PercentageDetails.PercentageTypeCodeQualifier = PercentageTypeCodeQualifierList.QualityYield;
					pCD.PercentageDetails.Percentage = percCommContent1.ToString();
				}

				if (!CertificateOfOrigin.CurrencyCode.IsEmpty && CertificateOfOrigin.CurrencyCode != Core.Constants.CurrencyCodes.Singapore)
				{
					CUXSegment cUX = sG4.CUX.InstantiateAChildAndAddItToChildrenCollection();
					cUX.CurrencyDetails1.CurrencyUsageCodeQualifier = CurrencyUsageCodeQualifierList.ReferenceCurrency;
					cUX.CurrencyDetails1.CurrencyIdentificationCode = CertificateOfOrigin.CurrencyCode;
				}

				// First Certificate Details
				GenerateSegmentGroup4Detail(sG4,
					CertificateOfOrigin.CertificateType1, "1", CertificateOfOrigin.NumberOfCopies1,
					CertificateOfOrigin.AdditionalDetails1, CertificateOfOrigin.TransportDetails1
				);

				// Second Certificate Details
				GenerateSegmentGroup4Detail(sG4,
					CertificateOfOrigin.CertificateType2, "2", CertificateOfOrigin.NumberOfCopies2,
					"", ""
				);
			}
		}

		void GenerateSegmentGroup4Detail(SegmentGroup4 sG4, ZString certType, string docId, ZInt numberOfCopies, ZString aditionalDetails, ZString transportDetails)
		{
			if (!certType.IsEmpty)
			{
				DOCSegment dOC = sG4.DOC.InstantiateAChildAndAddItToChildrenCollection();
				dOC.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.CertificateOfOriginApplicationFor;
				dOC.DocumentMessageName.DocumentName = certType;
				dOC.DocumentMessageDetails.DocumentIdentifier = docId;

				if (numberOfCopies > 0)
				{
					dOC.DocumentCopiesRequiredQuantity = numberOfCopies.ToString();
				}

				if (!aditionalDetails.IsEmpty)
				{
					GenerateFTXSegment(sG4, TextSubjectCodeQualifierList.AdditionalExportInformation, aditionalDetails);
				}

				if (!transportDetails.IsEmpty)
				{
					GenerateFTXSegment(sG4, TextSubjectCodeQualifierList.TransportDetailsRemarks, transportDetails);
				}
			}
		}

		void GenerateFTXSegment(SegmentGroup4 sG4, TextSubjectCodeQualifierList qualifier, string text)
		{
			FTXSegment fTX = sG4.FTX.InstantiateAChildAndAddItToChildrenCollection();
			fTX.TextSubjectCodeQualifier = qualifier;
			TextSplitElegantly splitter = new TextSplitElegantly(35, 5);
			splitter.Text = text;
			fTX.TextLiteral.FreeText1 = splitter[0];
			fTX.TextLiteral.FreeText2 = splitter[1];
			fTX.TextLiteral.FreeText3 = splitter[2];
			fTX.TextLiteral.FreeText4 = splitter[3];
			fTX.TextLiteral.FreeText5 = splitter[4];
		}

		#endregion

		#region Invoice Lines

		void GenerateSegmentGroup5(TCODECMessage edifactMsg)
		{
			foreach (ICusCertItem item in CertificateOfOrigin.CertItems)
			{
				SegmentGroup5 sG5 = edifactMsg.Group5.InstantiateAChildAndAddItToChildrenCollection();
				LINSegment lIN = sG5.LIN.InstantiateAChildAndAddItToChildrenCollection();
				lIN.ActionRequestNotificationDescriptionCode = ActionRequestNotificationDescriptionCodeList.NoAction;

				CSTSegment cST = sG5.CST.InstantiateAChildAndAddItToChildrenCollection();
				cST.GoodsItemNumber = item.SerialNumber;
				cST.CustomsIdentityCodes1.CustomsGoodsIdentifier = item.HSCode;

				GenerateItemMeasurementSegments(sG5, item);
				GenerateCargoMarkingsSegment(sG5, item);
				GenerateMonetarySegments(sG5, item);
				GenerateCountryOfOriginSegment(sG5, item);
				GenerateDateOfManufactureSegment(sG5, item);
				GenerateItemDescriptionSegment(sG5, item);

				GenerateSegmentGroup6(sG5, item);
			}
		}

		#region Line Measurements

		void GenerateItemMeasurementSegments(SegmentGroup5 sG5, ICusCertItem item)
		{
			//note 1
			GenerateHSQuantitySegment(sG5, item);

			//note 2
			GenerateCertificateItemQty(sG5, item);
		}

		void GenerateHSQuantitySegment(SegmentGroup5 sG5, ICusCertItem item)
		{
			ZString hSUQ = item.HSQuantityUnitType == "VAL" || item.HSQuantityUnitType.IsEmpty ? item.InvoiceUQ : item.HSQuantityUnitType;
			GenerateLineMEASegment(sG5, MeasurementPurposeCodeQualifierList.CustomsLineItemMeasurement, hSUQ, item.HSQuantity);
		}

		void GenerateCertificateItemQty(SegmentGroup5 sG5, ICusCertItem item)
		{
			GenerateLineMEASegment(sG5, MeasurementPurposeCodeQualifierList.ConsignmentMeasurement, item.ItemQuantityUnitType, item.ItemQuantity);
		}

		void GenerateLineMEASegment(SegmentGroup5 sG5, MeasurementPurposeCodeQualifierList qualifier, string unitOfQty, decimal qtyValue)
		{
			MEASegment mEA = sG5.MEA.InstantiateAChildAndAddItToChildrenCollection();
			mEA.MeasurementPurposeCodeQualifier = qualifier;
			mEA.ValueRange.MeasurementUnitCode = unitOfQty;
			mEA.ValueRange.Measure = Utilities.FormatNumber(qtyValue, SGConstants.NumericFormatting.DecimalPlacesForMeasurementValues);
		}

		#endregion

		void GenerateCargoMarkingsSegment(SegmentGroup5 sG5, ICusCertItem item)
		{
			if (!item.MarksAndNumbers.IsEmpty)
			{
				PCISegment pCI = sG5.PCI.InstantiateAChildAndAddItToChildrenCollection();
				pCI.MarkingInstructionsCode = MarkingInstructionsCodeList.MarkFreeText;
				TextSplitElegantly splitter = new TextSplitElegantly(17, 10);
				splitter.Text = item.MarksAndNumbers;
				pCI.MarksLabels.ShippingMarksDescription1 = splitter[0];
				pCI.MarksLabels.ShippingMarksDescription2 = splitter[1];
				pCI.MarksLabels.ShippingMarksDescription3 = splitter[2];
				pCI.MarksLabels.ShippingMarksDescription4 = splitter[3];
				pCI.MarksLabels.ShippingMarksDescription5 = splitter[4];
				pCI.MarksLabels.ShippingMarksDescription6 = splitter[5];
				pCI.MarksLabels.ShippingMarksDescription7 = splitter[6];
				pCI.MarksLabels.ShippingMarksDescription8 = splitter[7];
				pCI.MarksLabels.ShippingMarksDescription9 = splitter[8];
				pCI.MarksLabels.ShippingMarksDescription10 = splitter[9];
			}
		}

		#region Line Monetary Amounts

		void GenerateMonetarySegments(SegmentGroup5 sG5, ICusCertItem item)
		{
			GenerateFOBSegment(sG5, item);
			GenerateCertificateFOBSegment(sG5, item);
		}

		void GenerateFOBSegment(SegmentGroup5 sG5, ICusCertItem item)
		{
			GenerateLineMOASegment(sG5, MonetaryAmountTypeCodeQualifierList.FobValue, item.CustomsValue);
		}

		void GenerateCertificateFOBSegment(SegmentGroup5 sG5, ICusCertItem item)
		{
			if (item.ItemValue > 0)
			{
				GenerateLineMOASegment(sG5, MonetaryAmountTypeCodeQualifierList.GoodsItemTotal, item.ItemValue);
			}
		}

		void GenerateLineMOASegment(SegmentGroup5 sG5, MonetaryAmountTypeCodeQualifierList qualifier, decimal amount)
		{
			MOASegment mOA = sG5.MOA.InstantiateAChildAndAddItToChildrenCollection();
			mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = qualifier;
			mOA.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(amount, SGConstants.NumericFormatting.DecimalPlacesForAmountValues);
		}

		#endregion

		void GenerateCountryOfOriginSegment(SegmentGroup5 sG5, ICusCertItem item)
		{
			LOCSegment lOC = sG5.LOC.InstantiateAChildAndAddItToChildrenCollection();
			lOC.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.CountryOfOrigin;
			lOC.LocationIdentification.LocationNameCode = item.CountryOfOriginCode;
		}

		void GenerateDateOfManufactureSegment(SegmentGroup5 sG5, ICusCertItem item)
		{
			if (!item.DateOfManufacturingCost.IsEmpty)
			{
				DTMSegment dTM = sG5.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dTM.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.ProductionManufactureDate;
				dTM.DateTimePeriod.DateOrTimeOrPeriodText = item.DateOfManufacturingCost.ToString("yyyyMMdd");
				dTM.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmdd;
			}
		}

		void GenerateItemDescriptionSegment(SegmentGroup5 sG5, ICusCertItem item)
		{
			TextSplitElegantly splitter = new TextSplitElegantly(35, 50);
			splitter.Text = item.ItemDescription;

			for (int i = 0; i < 50; i = i + 5)
			{
				if (splitter.Count > i && !string.IsNullOrEmpty(splitter[i]))
				{
					FTXSegment fTX = sG5.FTX.InstantiateAChildAndAddItToChildrenCollection();
					fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.AdditionalExportInformation;
					fTX.TextLiteral.FreeText1 = GetPaddedTextLiteral(splitter[i]);
					if (splitter[i + 1].Length > 0 || splitter[i + 2].Length > 0 || splitter[i + 3].Length > 0 || splitter[i + 4].Length > 0)
					{
						fTX.TextLiteral.FreeText2 = GetPaddedTextLiteral(splitter[i + 1]);
						if (splitter[i + 2].Length > 0 || splitter[i + 3].Length > 0 || splitter[i + 4].Length > 0)
						{
							fTX.TextLiteral.FreeText3 = GetPaddedTextLiteral(splitter[i + 2]);
							if (splitter[i + 3].Length > 0 || splitter[i + 4].Length > 0)
							{
								fTX.TextLiteral.FreeText4 = GetPaddedTextLiteral(splitter[i + 3]);
								fTX.TextLiteral.FreeText5 = splitter[i + 4];
							}
						}
					}
				}
			}
		}

		string GetPaddedTextLiteral(string text)
		{
			return text.Length > 0 ? text : " ";
		}

		#region Segment Group 6

		void GenerateSegmentGroup6(SegmentGroup5 sG5, ICusCertItem item)
		{
			SegmentGroup6 sG6 = sG5.Group6.InstantiateAChildAndAddItToChildrenCollection();
			GenerateInvoiceNumberSegment(sG6, item);
			GenerateInvoiceDateSegment(sG6, item);
			GenerateHSCodeSegment(sG6, item);
			GeneratePercentageContentSegment(sG6, item);
			GenerateAdditionalConditionsSegment(sG6, item);
			GenerateTextileCategorySegment(sG6, item);
			GenerateTextileQuotaSegment(sG6, item);
		}

		void GenerateInvoiceNumberSegment(SegmentGroup6 sG6, ICusCertItem item)
		{
			if (!item.InvoiceNumber.IsEmpty)
			{
				RFFSegment rFF = sG6.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rFF.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.InvoiceNumber;
				rFF.Reference.ReferenceIdentifier = item.InvoiceNumber;
			}
			else if (CertificateOfOrigin.ApplicationProductType == ApplicationProductTypeCodeList.Codes.TX)
			{
				RFFSegment rFF = sG6.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rFF.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.MutuallyDefinedReferenceNumber;
			}
		}

		void GenerateInvoiceDateSegment(SegmentGroup6 sG6, ICusCertItem item)
		{
			if (!item.InvoiceDate.IsEmpty)
			{
				DTMSegment dTM = sG6.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dTM.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.InvoiceDateTime;
				dTM.DateTimePeriod.DateOrTimeOrPeriodText = item.InvoiceDate.ToString("yyyyMMdd");
				dTM.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmdd;
			}
		}

		void GenerateHSCodeSegment(SegmentGroup6 sG6, ICusCertItem item)
		{
			if (!item.CertHSCode.IsEmpty)
			{
				CSTSegment cST = sG6.CST.InstantiateAChildAndAddItToChildrenCollection();
				cST.CustomsIdentityCodes1.CustomsGoodsIdentifier = item.CertHSCode;
			}
		}

		void GeneratePercentageContentSegment(SegmentGroup6 sG6, ICusCertItem item)
		{
			if (item.PercentageContent > 0)
			{
				PCDSegment pCD = sG6.PCD.InstantiateAChildAndAddItToChildrenCollection();
				pCD.PercentageDetails.PercentageTypeCodeQualifier = PercentageTypeCodeQualifierList.QualityYield;
				pCD.PercentageDetails.Percentage = Utilities.FormatNumberFromZInt(item.PercentageContent, 0);
			}
		}

		void GenerateAdditionalConditionsSegment(SegmentGroup6 sG6, ICusCertItem item)
		{
			if (!item.OriginCriterion1.IsEmpty)
			{
				FTXSegment fTX = sG6.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.AdditionalConditions;
				fTX.TextLiteral.FreeText1 = item.OriginCriterion1;
				fTX.TextLiteral.FreeText2 = item.OriginCriterion2;
				fTX.TextLiteral.FreeText3 = item.OriginCriterion3;
			}
		}

		void GenerateTextileCategorySegment(SegmentGroup6 sG6, ICusCertItem item)
		{
			if (!item.TextileCategoryCode.IsEmpty)
			{
				GINSegment gIN = sG6.GIN.InstantiateAChildAndAddItToChildrenCollection();
				gIN.ObjectIdentificationCodeQualifier = ObjectIdentificationCodeQualifierList.TransportPackingGroupNumber;
				gIN.IdentityNumberRange1.ObjectIdentifier1 = item.TextileCategoryCode;
			}
		}

		void GenerateTextileQuotaSegment(SegmentGroup6 sG6, ICusCertItem item)
		{
			if (item.TextileQuotaQty > 0)
			{
				QTYSegment qTY = sG6.QTY.InstantiateAChildAndAddItToChildrenCollection();
				qTY.QuantityDetails.QuantityTypeCodeQualifier = QuantityTypeCodeQualifierList.QuantityLoaded;
				qTY.QuantityDetails.Quantity = item.TextileQuotaQty.ToString();
				qTY.QuantityDetails.MeasurementUnitCode = item.TextileQuotaUnitCode;
			}
		}

		#endregion

		#endregion

		#endregion

		#region Summary Section

		protected override void GenerateSummarySection(TCODECMessage edifactMsg)
		{
			GenerateSummaryUNS(edifactMsg);
			GenerateCNT(edifactMsg);
			GenerateUNT(edifactMsg);
		}

		void GenerateSummaryUNS(TCODECMessage edifactMsg)
		{
			UNSSegment uNS = edifactMsg.UNS2.InstantiateAChildAndAddItToChildrenCollection();
			uNS.SectionIdentification = UnsSummary;
		}

		void GenerateCNT(TCODECMessage edifactMsg)
		{
			CNTSegment cNT = edifactMsg.CNT.InstantiateAChildAndAddItToChildrenCollection();
			cNT.Control.ControlTotalTypeCodeQualifier = ControlTotalTypeCodeQualifierList.NumberOfCustomsItemDetailLines;
			cNT.Control.ControlTotalQuantity = edifactMsg.Group5.Count.ToString();
		}

		void GenerateUNT(TCODECMessage edifactMsg)
		{
			UNTSegment uNT = edifactMsg.UNT.InstantiateAChildAndAddItToChildrenCollection();
			uNT.MessageReferenceNumber = edifactMsg.UNH[0].MessageReferenceNumber;
			uNT.NumberOfSegmentsInTheMessage = edifactMsg.CountIncludingUNT.ToString();
		}

		#endregion

		#region TcodecConstants

		const string DocDocumentAttachment = "DOCUMENT ATTACHMENT";

		#endregion

		#endregion

		#endregion
	}
}
