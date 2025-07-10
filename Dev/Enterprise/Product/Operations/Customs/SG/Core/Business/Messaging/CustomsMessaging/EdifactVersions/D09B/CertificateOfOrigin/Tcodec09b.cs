namespace Enterprise.Customs.SG.Business.CustomsMessaging.D09B
{
	using CargoWise.Types;
	using Enterprise.Customs.SG.V4.Business;
	using Enterprise.Edifact.D09B.Elements;
	using Enterprise.Edifact.D09B.Messages.TCODEC;
	using Enterprise.Edifact.Utilities;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;

	class Tcodec09b : TcodecBase<TCODECMessage>
	{
		public Tcodec09b(ITCODEC coCusdec)
			: base(coCusdec)
		{
		}

		protected override string UnhAssociationAssignedCode
		{
			get { return "041"; }
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
			var unh = edifactMsg.UNH.InstantiateAChildAndAddItToChildrenCollection();
			unh.MessageReferenceNumber = "1";  // This (and UNT.MessageReferenceNumber) need to be filled in by interchange creation
			unh.MessageIdentifier.MessageType = UnhMessageTypeIdentifier;
			unh.MessageIdentifier.MessageVersionNumber = UnhMessageVersionNumber;
			unh.MessageIdentifier.MessageReleaseNumber = UnhMessageReleaseNumber;
			unh.MessageIdentifier.ControllingAgency = UnhControllingAgency;
			unh.MessageIdentifier.AssociationAssignedCode = UnhAssociationAssignedCode;
			unh.CommonAccessReference = MessageType;
		}

		void GenerateBGM(TCODECMessage edifactMsg)
		{
			var bgm = edifactMsg.BGM.InstantiateAChildAndAddItToChildrenCollection();
			bgm.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.CertificateOfOriginApplicationFor;
			bgm.DocumentMessageName.DocumentName = BgmApplicationType;
			bgm.DocumentMessageIdentification.DocumentIdentifier = "<<MSGNO PLACEHOLDER>>";
			bgm.MessageFunctionCode = MessageFunctionCodeList.Original;
		}

		void GenerateCST(TCODECMessage edifactMsg)
		{
			var cst = edifactMsg.CST.InstantiateAChildAndAddItToChildrenCollection();
			cst.CustomsIdentityCodes1.CustomsGoodsIdentifier = CertificateOfOrigin.ApplicationProductType;
		}

		#endregion

		#region Segment Group 1

		void GenerateSegmentGroup1(TCODECMessage edifactMsg)
		{
			var sg1 = edifactMsg.Group1.InstantiateAChildAndAddItToChildrenCollection();
			GenerateMessageSenderMailboxSegment(sg1);
			GenerateAdditionalInfoSegment(sg1);
			GenerateSupportingDocsSegments(sg1);

			// Generates it in another Group1 segment
			GeneratePreviousPermitNumberSegment(edifactMsg.Group1);
		}

		void GenerateMessageSenderMailboxSegment(SegmentGroup1 sg1)
		{
			var rff = sg1.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rff.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.MessageSender;
			rff.Reference.ReferenceIdentifier = CertificateOfOrigin.DeclarantId.Left(4) + "." + CertificateOfOrigin.DeclarantId;
		}

		void GeneratePreviousPermitNumberSegment(SegmentGroup1MessageSection sg1Section)
		{
			if (!CertificateOfOrigin.PreviousPermitNumber.IsEmpty)
			{
				var sg1 = sg1Section.InstantiateAChildAndAddItToChildrenCollection();
				var rff = sg1.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rff.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.RelatedDocumentNumber;
				rff.Reference.ReferenceIdentifier = CertificateOfOrigin.PreviousPermitNumber;
			}
		}

		void GenerateAdditionalInfoSegment(SegmentGroup1 sg1)
		{
			if (!CertificateOfOrigin.AdditionalInformation.IsEmpty)
			{
				var ftx = sg1.FTX.InstantiateAChildAndAddItToChildrenCollection();
				ftx.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.DocumentIssuerDeclaration;
				var splitter = new TextSplitElegantly(35, 5);
				splitter.Text = CertificateOfOrigin.AdditionalInformation;
				ftx.TextLiteral.FreeText1 = splitter[0];
				ftx.TextLiteral.FreeText2 = splitter[1];
				ftx.TextLiteral.FreeText3 = splitter[2];
				ftx.TextLiteral.FreeText4 = splitter[3];
				ftx.TextLiteral.FreeText5 = splitter[4];
			}
		}

		void GenerateSupportingDocsSegments(SegmentGroup1 sg1)
		{
			if (CertificateOfOrigin.AdditionalMessageInformation != null && CertificateOfOrigin.AdditionalMessageInformation.SupportingDocuments != null)
			{
				foreach (ICusAttachment attachment in CertificateOfOrigin.AdditionalMessageInformation.SupportingDocuments)
				{
					var doc = sg1.DOC.InstantiateAChildAndAddItToChildrenCollection();
					doc.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.RelatedDocument;
					doc.DocumentMessageDetails.DocumentIdentifier = attachment.DocType;
					doc.DocumentMessageDetails.DocumentSourceDescription = attachment.FileName.ToASCII();
				}
			}
		}

		#endregion

		#region Segment Group 2

		void GenerateSegmentGroup2(TCODECMessage edifactMsg)
		{
			var sg2 = edifactMsg.Group2.InstantiateAChildAndAddItToChildrenCollection();

			GenerateTransportInfoSegment(sg2);
			GenerateDepartureDateSegment(sg2);
			GenerateEntryYearSegment(sg2);

			GeneratePortOfDischargeSegment(sg2);
			GenerateCountryOfUltimateDestinationSegment(sg2);
			GenerateDonorCountrySegment(sg2);

			GenerateBrokerDeclarationSegment(sg2);
		}

		void GenerateTransportInfoSegment(SegmentGroup2 sg2)
		{
			var tdt = sg2.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tdt.TransportStageCodeQualifier = TransportStageCodeQualifierList.MainCarriageTransport;
			tdt.ModeOfTransport.TransportModeNameCode = CertificateOfOrigin.OutwardTransportCode.ToString();

			if (CertificateOfOrigin.IsSea || CertificateOfOrigin.IsAir)
			{
				tdt.MeansOfTransportJourneyIdentifier = CertificateOfOrigin.OutwardJourneyIdentifier;

				tdt.TransportIdentification.TransportMeansIdentificationName =
					(tdt.ModeOfTransport.TransportModeNameCode == Core.Constants.TransportCodes.Sea) ?
					CertificateOfOrigin.OutwardTransportIdentifier :
					CertificateOfOrigin.OutwardJourneyIdentifier;
			}
		}

		void GenerateDepartureDateSegment(SegmentGroup2 sg2)
		{
			if (!CertificateOfOrigin.DepartureDate.IsEmpty)
			{
				var dtm = sg2.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansDepartureDateTimeActual_136;
				dtm.DateTimePeriod.DateOrTimeOrPeriodText = CertificateOfOrigin.DepartureDate.ToString("yyyyMMdd");
				dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmdd;
			}
		}

		void GenerateEntryYearSegment(SegmentGroup2 sg2)
		{
			if (CertificateOfOrigin.ApplicationProductType == ApplicationProductTypeCodeList.Codes.TX && !CertificateOfOrigin.YearOfEntry.IsEmpty)
			{
				var dtm = sg2.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.YearOfOccurrence;
				dtm.DateTimePeriod.DateOrTimeOrPeriodText = CertificateOfOrigin.YearOfEntry.ToString();
				dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyy;
			}
		}

		void GenerateBrokerDeclarationSegment(SegmentGroup2 sg2)
		{
			var gei = sg2.GEI.InstantiateAChildAndAddItToChildrenCollection();
			gei.ProcessingInformationCodeQualifier = ProcessingInformationCodeQualifierList.ConsignmentTypeInformation;
			gei.ProcessingIndicator.CodeListIdentificationCode = "Y";
		}

		void GeneratePortOfDischargeSegment(SegmentGroup2 sg2)
		{
			var loc = sg2.LOC.InstantiateAChildAndAddItToChildrenCollection();
			loc.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.PlaceOfDischarge;
			loc.LocationIdentification.LocationIdentifier = CertificateOfOrigin.PortOfDischarge;
		}

		void GenerateCountryOfUltimateDestinationSegment(SegmentGroup2 sg2)
		{
			var loc = sg2.LOC.InstantiateAChildAndAddItToChildrenCollection();
			loc.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.CountryOfUltimateDestination;
			loc.LocationIdentification.LocationIdentifier = CertificateOfOrigin.CountryOfFinalDestination;
		}

		void GenerateDonorCountrySegment(SegmentGroup2 sg2)
		{
			if (!CertificateOfOrigin.DonorCountryCode.IsEmpty)
			{
				var loc = sg2.LOC.InstantiateAChildAndAddItToChildrenCollection();
				loc.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.DonationActingCountry;
				loc.LocationIdentification.LocationIdentifier = CertificateOfOrigin.DonorCountryCode;
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
			GenerateGroup3NadSegment(sg3Section, PartyFunctionCodeQualifierList.DeclarantsAgentRepresentative, GlbCompany.CurrentCompany.GC_Name, GlbCompany.CurrentCompany.GC_CustomsRegistrationNo, null);
		}

		void PopulateDeclarantSegments(SegmentGroup3MessageSection sg3Section)
		{
			var sg3 = sg3Section.InstantiateAChildAndAddItToChildrenCollection();
			var nad = sg3.NAD.InstantiateAChildAndAddItToChildrenCollection();
			nad.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.Declarant;

			var cta = sg3.CTA.InstantiateAChildAndAddItToChildrenCollection();
			cta.ContactFunctionCode = ContactFunctionCodeList.InformationContact;
			cta.ContactDetails.ContactIdentifier = CertificateOfOrigin.Declarant.Code;
			cta.ContactDetails.ContactName = CertificateOfOrigin.Declarant.Name;

			var com = sg3.COM.InstantiateAChildAndAddItToChildrenCollection();
			com.CommunicationContact.CommunicationAddressIdentifier = CertificateOfOrigin.Declarant.Phone;
			com.CommunicationContact.CommunicationMeansTypeCode = CommunicationMeansTypeCodeList.Telephone;
		}

		void PopulateConsigneeSegment(SegmentGroup3MessageSection sg3Section)
		{
			if (CertificateOfOrigin.Consignee != null)
			{
				GenerateGroup3NadSegment(sg3Section, PartyFunctionCodeQualifierList.Consignee, CertificateOfOrigin.Consignee.Name, null, CertificateOfOrigin.Consignee.Address);
			}
		}

		void PopulateExporterSegment(SegmentGroup3MessageSection sg3Section)
		{
			if (CertificateOfOrigin.Exporter != null)
			{
				GenerateGroup3NadSegment(sg3Section, PartyFunctionCodeQualifierList.Exporter, CertificateOfOrigin.Exporter.Name, CertificateOfOrigin.Exporter.UEN, CertificateOfOrigin.Exporter.Address);
			}
		}

		void PopulateManufacturerSegment(SegmentGroup3MessageSection sg3Section)
		{
			if (CertificateOfOrigin.Manufacturer != null)
			{
				GenerateGroup3NadSegment(sg3Section, PartyFunctionCodeQualifierList.ManufacturerOfGoods, CertificateOfOrigin.Manufacturer.Name, CertificateOfOrigin.Manufacturer.UEN, CertificateOfOrigin.Manufacturer.Address);
			}
		}

		void PopulateOutwardCarrierAgentSegment(SegmentGroup3MessageSection sg3Section)
		{
			if (CertificateOfOrigin.OutwardCarrierAgent != null)
			{
				GenerateGroup3NadSegment(sg3Section, PartyFunctionCodeQualifierList.Carrier, CertificateOfOrigin.OutwardCarrierAgent.Name, CertificateOfOrigin.OutwardCarrierAgent.UEN, null);
			}
		}

		void PopulateFreightForwarderSegment(SegmentGroup3MessageSection sg3Section)
		{
			if (CertificateOfOrigin.FreightForwarder != null)
			{
				GenerateGroup3NadSegment(sg3Section, PartyFunctionCodeQualifierList.FreightForwarder, CertificateOfOrigin.FreightForwarder.Name, CertificateOfOrigin.FreightForwarder.UEN, null);
			}
		}

		void GenerateGroup3NadSegment(SegmentGroup3MessageSection sg3Section, PartyFunctionCodeQualifierList qualifier, string name, string uen, IAddress address)
		{
			if (!string.IsNullOrEmpty(name))
			{
				var sg3 = sg3Section.InstantiateAChildAndAddItToChildrenCollection();
				var nad = sg3.NAD.InstantiateAChildAndAddItToChildrenCollection();
				nad.PartyFunctionCodeQualifier = qualifier;

				if (!string.IsNullOrEmpty(uen))
				{
					nad.PartyIdentificationDetails.PartyIdentifier = uen;
				}

				bool shouldUseShortNameFormat = (
						  qualifier == PartyFunctionCodeQualifierList.Consignee ||
						  qualifier == PartyFunctionCodeQualifierList.Exporter ||
						  qualifier == PartyFunctionCodeQualifierList.ManufacturerOfGoods
					 );

				nad.PartyName.PartyNameFormatCode = (shouldUseShortNameFormat) ?
				PartyNameFormatCodeList.NameComponentSequence2SequenceAsDefinedInDescription :
				PartyNameFormatCodeList.NameComponentsInSequenceAsDefinedInDescriptionBelow;

				var splitter = new TextSplitElegantly((shouldUseShortNameFormat) ? 35 : 50, 2);
				splitter.Text = name;
				nad.PartyName.PartyName1 = splitter[0];
				nad.PartyName.PartyName2 = splitter[1];

				if (address != null && !address.FullAddress.IsEmpty)
				{
					bool shouldUseAddressCityField = (qualifier == PartyFunctionCodeQualifierList.ManufacturerOfGoods);
					splitter = new TextSplitElegantly(35, (shouldUseAddressCityField) ? 2 : 3);
					splitter.Text = address.FullAddress;
					nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = splitter[0];
					nad.Street.StreetAndNumberOrPostOfficeBoxIdentifier2 = splitter[1];
					nad.CityName = (shouldUseAddressCityField) ? address.City.ToString() : splitter[2];

					if (qualifier == PartyFunctionCodeQualifierList.ManufacturerOfGoods)
					{
						nad.CountrySubdivisionDetails.CountrySubdivisionIdentifier = address.SubdivisionCode;
						nad.CountrySubdivisionDetails.CountrySubdivisionName = address.SubdivisionName;
						nad.PostalIdentificationCode = address.PostCode.Length < 10 ? address.PostCode : address.PostCode.KeepAlphanumericCharacters().SubstringSafe(0, 9);
						nad.CountryIdentifier = address.CountryCode;
					}
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
			var uns = edifactMsg.UNS1.InstantiateAChildAndAddItToChildrenCollection();
			uns.SectionIdentification = UnsDetail;
		}

		#region Segment Group 4

		void GenerateSegmentGroup4(TCODECMessage edifactMsg)
		{
			if (!CertificateOfOrigin.CertificateType1.IsEmpty || !CertificateOfOrigin.CertificateType2.IsEmpty)
			{
				var sg4 = edifactMsg.Group4.InstantiateAChildAndAddItToChildrenCollection();
				var dms = sg4.DMS.InstantiateAChildAndAddItToChildrenCollection();
				dms.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.CertificateOfOriginApplicationFor;

				var percCommContent1 = CertificateOfOrigin.PercCommContent1;
				if (percCommContent1 != 0)
				{
					var pcd = sg4.PCD.InstantiateAChildAndAddItToChildrenCollection();
					pcd.PercentageDetails.PercentageTypeCodeQualifier = PercentageTypeCodeQualifierList.QualityYield;
					pcd.PercentageDetails.Percentage = percCommContent1.ToString();
				}

				if (!CertificateOfOrigin.CurrencyCode.IsEmpty)
				{
					var cux = sg4.CUX.InstantiateAChildAndAddItToChildrenCollection();
					cux.CurrencyDetails1.CurrencyUsageCodeQualifier = CurrencyUsageCodeQualifierList.ReferenceCurrency;
					cux.CurrencyDetails1.CurrencyIdentificationCode = CertificateOfOrigin.CurrencyCode;
				}

				// First Certificate Details
				GenerateSegmentGroup4Detail(sg4,
					CertificateOfOrigin.CertificateType1, "1", CertificateOfOrigin.NumberOfCopies1,
					CertificateOfOrigin.AdditionalDetails1, CertificateOfOrigin.TransportDetails1
				);

				// Second Certificate Details
				GenerateSegmentGroup4Detail(sg4,
					CertificateOfOrigin.CertificateType2, "2", CertificateOfOrigin.NumberOfCopies2,
					"", ""
				);
			}
		}

		void GenerateSegmentGroup4Detail(SegmentGroup4 sg4, ZString certType, string docId, ZInt qtyCopies, ZString aditionalDetails, ZString transportDetails)
		{
			if (!certType.IsEmpty)
			{
				var doc = sg4.DOC.InstantiateAChildAndAddItToChildrenCollection();
				doc.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.CertificateOfOriginApplicationFor;
				doc.DocumentMessageName.DocumentName = certType;
				doc.DocumentMessageDetails.DocumentIdentifier = docId;

				if (qtyCopies > 0)
				{
					doc.DocumentCopiesRequiredQuantity = qtyCopies.ToString();
				}

				if (!aditionalDetails.IsEmpty)
				{
					GenerateFTXSegment(sg4, TextSubjectCodeQualifierList.AdditionalExportInformation, aditionalDetails);
				}

				if (!transportDetails.IsEmpty)
				{
					GenerateFTXSegment(sg4, TextSubjectCodeQualifierList.ConsignmentTransport, transportDetails);
				}
			}
		}

		void GenerateFTXSegment(SegmentGroup4 sg4, TextSubjectCodeQualifierList qualifier, string text)
		{
			var ftx = sg4.FTX.InstantiateAChildAndAddItToChildrenCollection();
			ftx.TextSubjectCodeQualifier = qualifier;
			var splitter = new TextSplitElegantly(35, 5);
			splitter.Text = text;
			ftx.TextLiteral.FreeText1 = splitter[0];
			ftx.TextLiteral.FreeText2 = splitter[1];
			ftx.TextLiteral.FreeText3 = splitter[2];
			ftx.TextLiteral.FreeText4 = splitter[3];
			ftx.TextLiteral.FreeText5 = splitter[4];
		}

		#endregion

		#region Invoice Lines

		void GenerateSegmentGroup5(TCODECMessage edifactMsg)
		{
			foreach (ICusCertItem item in CertificateOfOrigin.CertItems)
			{
				var sg5 = edifactMsg.Group5.InstantiateAChildAndAddItToChildrenCollection();
				var lin = sg5.LIN.InstantiateAChildAndAddItToChildrenCollection();
				lin.ActionCode = ActionCodeList.NoAction;

				var cst = sg5.CST.InstantiateAChildAndAddItToChildrenCollection();
				cst.GoodsItemNumber = item.SerialNumber;
				cst.CustomsIdentityCodes1.CustomsGoodsIdentifier = item.HSCode;

				GenerateItemMeasurementSegments(sg5, item);
				GenerateCargoMarkingsSegment(sg5, item);
				GenerateMonetarySegments(sg5, item);
				GenerateCountryOfOriginSegment(sg5, item);
				GenerateDateOfManufactureSegment(sg5, item);
				GenerateItemDescriptionSegment(sg5, item);

				GenerateSegmentGroup6(sg5, item);
			}
		}

		#region Line Measurements

		void GenerateItemMeasurementSegments(SegmentGroup5 sg5, ICusCertItem item)
		{
			//note 1
			GenerateHSQuantitySegment(sg5, item);

			//note 2
			GenerateCertificateItemQty(sg5, item);
		}

		void GenerateHSQuantitySegment(SegmentGroup5 sg5, ICusCertItem item)
		{
			var hsUnitOfQty = (item.HSQuantityUnitType == "VAL" || item.HSQuantityUnitType.IsEmpty) ? item.InvoiceUQ : item.HSQuantityUnitType;
			GenerateLineMEASegment(sg5, MeasurementPurposeCodeQualifierList.CustomsLineItemMeasurement, hsUnitOfQty, item.HSQuantity);
		}

		void GenerateCertificateItemQty(SegmentGroup5 sg5, ICusCertItem item)
		{
			GenerateLineMEASegment(sg5, MeasurementPurposeCodeQualifierList.ConsignmentMeasurement, item.ItemQuantityUnitType, item.ItemQuantity);
		}

		void GenerateLineMEASegment(SegmentGroup5 sg5, MeasurementPurposeCodeQualifierList qualifier, string unitOfQty, decimal qtyValue)
		{
			if (qtyValue > 0)
			{
				var mea = sg5.MEA.InstantiateAChildAndAddItToChildrenCollection();
				mea.MeasurementPurposeCodeQualifier = qualifier;
				mea.ValueRange.MeasurementUnitCode = unitOfQty;
				mea.ValueRange.Measure = Utilities.FormatNumber(qtyValue, SGConstants.NumericFormatting.DecimalPlacesForMeasurementValues);
			}
		}

		#endregion

		void GenerateCargoMarkingsSegment(SegmentGroup5 sg5, ICusCertItem item)
		{
			if (!item.MarksAndNumbers.IsEmpty)
			{
				var pci = sg5.PCI.InstantiateAChildAndAddItToChildrenCollection();
				pci.MarkingInstructionsCode = MarkingInstructionsCodeList.MarkFreeText;
				var splitter = new TextSplitElegantly(17, 10);
				splitter.Text = item.MarksAndNumbers;
				pci.MarksLabels.ShippingMarksDescription1 = splitter[0];
				pci.MarksLabels.ShippingMarksDescription2 = splitter[1];
				pci.MarksLabels.ShippingMarksDescription3 = splitter[2];
				pci.MarksLabels.ShippingMarksDescription4 = splitter[3];
				pci.MarksLabels.ShippingMarksDescription5 = splitter[4];
				pci.MarksLabels.ShippingMarksDescription6 = splitter[5];
				pci.MarksLabels.ShippingMarksDescription7 = splitter[6];
				pci.MarksLabels.ShippingMarksDescription8 = splitter[7];
				pci.MarksLabels.ShippingMarksDescription9 = splitter[8];
				pci.MarksLabels.ShippingMarksDescription10 = splitter[9];
			}
		}

		#region Line Monetary Amounts

		void GenerateMonetarySegments(SegmentGroup5 sg5, ICusCertItem item)
		{
			GenerateFOBSegment(sg5, item);
			GenerateCertificateFOBSegment(sg5, item);
		}

		void GenerateFOBSegment(SegmentGroup5 sg5, ICusCertItem item)
		{
			GenerateLineMOASegment(sg5, MonetaryAmountTypeCodeQualifierList.FobValue, item.CustomsValue);
		}

		void GenerateCertificateFOBSegment(SegmentGroup5 sg5, ICusCertItem item)
		{
			if (item.ItemValue > 0)
			{
				GenerateLineMOASegment(sg5, MonetaryAmountTypeCodeQualifierList.GoodsItemTotal, item.ItemValue);
			}
		}

		void GenerateLineMOASegment(SegmentGroup5 sg5, MonetaryAmountTypeCodeQualifierList qualifier, decimal amount)
		{
			var moa = sg5.MOA.InstantiateAChildAndAddItToChildrenCollection();
			moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier = qualifier;
			moa.MonetaryAmount.MonetaryAmount = Utilities.FormatNumber(amount, SGConstants.NumericFormatting.DecimalPlacesForAmountValues);
		}

		#endregion

		void GenerateCountryOfOriginSegment(SegmentGroup5 sg5, ICusCertItem item)
		{
			var loc = sg5.LOC.InstantiateAChildAndAddItToChildrenCollection();
			loc.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.CountryOfOrigin;
			loc.LocationIdentification.LocationIdentifier = item.CountryOfOriginCode;
		}

		void GenerateDateOfManufactureSegment(SegmentGroup5 sg5, ICusCertItem item)
		{
			if (!item.DateOfManufacturingCost.IsEmpty)
			{
				var dtm = sg5.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.ProductionManufactureDate;
				dtm.DateTimePeriod.DateOrTimeOrPeriodText = item.DateOfManufacturingCost.ToString("yyyyMMdd");
				dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmdd;
			}
		}

		#region Line Description

		void GenerateItemDescriptionSegment(SegmentGroup5 sg5, ICusCertItem item)
		{
			var splitter = new TextSplitElegantly(35, 50);
			splitter.Text = item.ItemDescription;

			for (int i = 0; i < 50; i += 5)
			{
				if (splitter.Count > i && !string.IsNullOrEmpty(splitter[i]))
				{
					var ftx = sg5.FTX.InstantiateAChildAndAddItToChildrenCollection();
					ftx.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.AdditionalExportInformation;
					ftx.TextLiteral.FreeText1 = GetPaddedTextLiteral(splitter[i]);

					if (IsAnyNotEmpty(splitter[i + 1], splitter[i + 2], splitter[i + 3], splitter[i + 4]))
					{
						ftx.TextLiteral.FreeText2 = GetPaddedTextLiteral(splitter[i + 1]);

						if (IsAnyNotEmpty(splitter[i + 2], splitter[i + 3], splitter[i + 4]))
						{
							ftx.TextLiteral.FreeText3 = GetPaddedTextLiteral(splitter[i + 2]);

							if (IsAnyNotEmpty(splitter[i + 3], splitter[i + 4]))
							{
								ftx.TextLiteral.FreeText4 = GetPaddedTextLiteral(splitter[i + 3]);
								ftx.TextLiteral.FreeText5 = splitter[i + 4];
							}
						}
					}
				}
			}
		}

		bool IsAnyNotEmpty(params string[] texts)
		{
			for (int i = 0; i < texts.Length; i++)
			{
				if (texts[i].Length > 0)
				{
					return true;
				}
			}

			return false;
		}

		string GetPaddedTextLiteral(string text)
		{
			return text.Length > 0 ? text : " ";
		}

		#endregion

		#region Segment Group 6

		void GenerateSegmentGroup6(SegmentGroup5 sg5, ICusCertItem item)
		{
			var sg6 = sg5.Group6.InstantiateAChildAndAddItToChildrenCollection();
			GenerateInvoiceNumberSegment(sg6, item);
			GenerateInvoiceDateSegment(sg6, item);
			GenerateHSCodeSegment(sg6, item);
			GeneratePercentageContentSegment(sg6, item);
			GenerateAdditionalConditionsSegment(sg6, item);
			GenerateTextileCategorySegment(sg6, item);
			GenerateTextileQuotaSegment(sg6, item);
		}

		void GenerateInvoiceNumberSegment(SegmentGroup6 sg6, ICusCertItem item)
		{
			if (!item.InvoiceNumber.IsEmpty)
			{
				var rff = sg6.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rff.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.InvoiceDocumentIdentifier;
				rff.Reference.ReferenceIdentifier = item.InvoiceNumber;
			}
			else
			{
				var rff = sg6.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rff.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.MutuallyDefinedReferenceNumber;
			}
		}

		void GenerateInvoiceDateSegment(SegmentGroup6 sg6, ICusCertItem item)
		{
			if (!item.InvoiceDate.IsEmpty)
			{
				var dtm = sg6.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.InvoiceDocumentIssueDateTime;
				dtm.DateTimePeriod.DateOrTimeOrPeriodText = item.InvoiceDate.ToString("yyyyMMdd");
				dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmdd;
			}
		}

		void GenerateHSCodeSegment(SegmentGroup6 sg6, ICusCertItem item)
		{
			if (!item.CertHSCode.IsEmpty)
			{
				var cst = sg6.CST.InstantiateAChildAndAddItToChildrenCollection();
				cst.CustomsIdentityCodes1.CustomsGoodsIdentifier = item.CertHSCode;
			}
		}

		void GeneratePercentageContentSegment(SegmentGroup6 sg6, ICusCertItem item)
		{
			if (item.PercentageContent > 0)
			{
				var pcd = sg6.PCD.InstantiateAChildAndAddItToChildrenCollection();
				pcd.PercentageDetails.PercentageTypeCodeQualifier = PercentageTypeCodeQualifierList.QualityYield;
				pcd.PercentageDetails.Percentage = Utilities.FormatNumberFromZInt(item.PercentageContent, 0);
			}
		}

		void GenerateAdditionalConditionsSegment(SegmentGroup6 sg6, ICusCertItem item)
		{
			if (!item.OriginCriterion1.IsEmpty)
			{
				var ftx = sg6.FTX.InstantiateAChildAndAddItToChildrenCollection();
				ftx.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.AdditionalConditions;
				ftx.TextLiteral.FreeText1 = item.OriginCriterion1;
				ftx.TextLiteral.FreeText2 = item.OriginCriterion2;
				ftx.TextLiteral.FreeText3 = item.OriginCriterion3;
			}
		}

		void GenerateTextileCategorySegment(SegmentGroup6 sg6, ICusCertItem item)
		{
			if (!item.TextileCategoryCode.IsEmpty)
			{
				var gin = sg6.GIN.InstantiateAChildAndAddItToChildrenCollection();
				gin.ObjectIdentificationCodeQualifier = ObjectIdentificationCodeQualifierList.TransportPackingGroupNumber;
				gin.IdentityNumberRange1.ObjectIdentifier1 = item.TextileCategoryCode;
			}
		}

		void GenerateTextileQuotaSegment(SegmentGroup6 sg6, ICusCertItem item)
		{
			if (item.TextileQuotaQty > 0)
			{
				var qty = sg6.QTY.InstantiateAChildAndAddItToChildrenCollection();
				qty.QuantityDetails.QuantityTypeCodeQualifier = QuantityTypeCodeQualifierList.QuantityLoaded;
				qty.QuantityDetails.Quantity = Utilities.FormatNumber(item.TextileQuotaQty, SGConstants.NumericFormatting.DecimalPlacesForMeasurementValues);
				qty.QuantityDetails.MeasurementUnitCode = item.TextileQuotaUnitCode;
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
			var uns = edifactMsg.UNS2.InstantiateAChildAndAddItToChildrenCollection();
			uns.SectionIdentification = UnsSummary;
		}

		void GenerateCNT(TCODECMessage edifactMsg)
		{
			var cnt = edifactMsg.CNT.InstantiateAChildAndAddItToChildrenCollection();
			cnt.Control.ControlTotalTypeCodeQualifier = ControlTotalTypeCodeQualifierList.NumberOfCustomsItemDetailLines;
			cnt.Control.ControlTotalQuantity = edifactMsg.Group5.Count.ToString();
		}

		void GenerateUNT(TCODECMessage edifactMsg)
		{
			var unt = edifactMsg.UNT.InstantiateAChildAndAddItToChildrenCollection();
			unt.MessageReferenceNumber = edifactMsg.UNH[0].MessageReferenceNumber;
			unt.NumberOfSegmentsInTheMessage = edifactMsg.CountIncludingUNT.ToString();
		}

		#endregion

		#endregion
	}
}
