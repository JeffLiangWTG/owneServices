using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Edifact.D96B.Elements;
using Enterprise.Edifact.D96B.Messages.CUSDEC;
using Enterprise.Edifact.D96B.Segments;
using Enterprise.Edifact.Utilities;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public static partial class CUSDECMessageTextBuilder
	{
		public static void PopulateCUSDECMessage(CUSDECMessage message, ICUSDECMessageDataProvider source)
		{
			PopulateUNHSegment(message.UNH[0]);
			PopulateBGMSegment(message.BGM[0], source);
			PopulateCSTSegment(message.CST[0], source);
			PopulateLOCSegments(message.LOC, source);
			PopulateDTMSegments(message.DTM, source);
			PopulateGISSegments(message.GIS, source);
			PopulateMEASegments(message.MEA, source);
			PopulateEQDSegments(message.EQD, source);
			PopulateFTXSegments(message.FTX, source);
			PopulateSG1Groups(message.Group1, source);
			PopulateSG4Groups(message.Group4, source);
			PopulateSG5Groups(message.Group5, source);
			PopulateSG6Groups(message.Group6, source);
			PopulateUNS1Segment(message.UNS1[0]);
			PopulateSG30Groups(message.Group30, source);
			PopulateUNS2Segment(message.UNS2[0]);
			PopulateSG49Groups(message.Group49, source);
			PopulateSG10Groups(message.Group10, source);
			PopulateUNTSegment(message.UNT[0], message.CountIncludingUNT);
		}

		#region Fields Population

		static void PopulateUNHSegment(UNHSegment unhSegment)
		{
			unhSegment.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			unhSegment.MessageIdentifier.MessageType = Edifact.D96B.Elements.MessageTypeList.CustomsDeclarationMessage;
			unhSegment.MessageIdentifier.MessageVersionNumber = "D";
			unhSegment.MessageIdentifier.MessageReleaseNumber = "96B";
			unhSegment.MessageIdentifier.ControllingAgency = Edifact.D96B.Elements.ControllingAgencyList.UnEceTradeWp4;
			unhSegment.MessageIdentifier.AssociationAssignedCode = "ZZZ01";
		}

		static void PopulateBGMSegment(BGMSegment bgmSegment, ICUSDECMessageDataProvider source)
		{
			bgmSegment.DocumentMessageName.DocumentMessageNameCoded = DocumentMessageNameCodedList.GetFromString(source.ShipmentType);
			bgmSegment.DocumentMessageName.DocumentMessageName = source.DeclarationType;
			bgmSegment.DocumentMessageIdentification.DocumentMessageNumber = source.LocalReferenceNumber;
			var countOfEntryHeader = source.PartClearanceQuantity;
			if (!countOfEntryHeader.IsEmpty)
			{
				bgmSegment.DocumentMessageIdentification.RevisionNumber = countOfEntryHeader.ToString("00000", CultureInfo.InvariantCulture);
			}
			bgmSegment.MessageFunctionCoded = MessageFunctionCodedList.GetFromString(source.MessageType);
		}

		static void PopulateCSTSegment(CSTSegment cstSegment, ICUSDECMessageDataProvider source)
		{
			cstSegment.CustomsIdentityCodes1.CustomsCodeIdentification = source.CustomsProcedureCategory;
			cstSegment.CustomsIdentityCodes1.CodeListQualifier = CodeListQualifierList.CustomsProcedure;
			cstSegment.CustomsIdentityCodes1.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.MutuallyDefined;
		}

		static void PopulateLOCSegments(LOCSegmentMessageSection locSection, ICUSDECMessageDataProvider source)
		{
			AddNewLOCSegment(locSection, source.LocationOfGoods, PlaceLocationQualifierList.LocationOfGoods, CodeListResponsibleAgencyCodedList.MutuallyDefined); // LOC+14
			AddNewLOCSegment(locSection, source.FromWarehouse, PlaceLocationQualifierList.Warehouse, CodeListResponsibleAgencyCodedList.MutuallyDefined); // LOC+18
			CUSDECMessageBuilderHelper.BuildForMutualExclusiveValues(source.ToWarehouse, source.Consignee, (ZString effectiveValue) =>
				{ AddNewLOCSegment(locSection, effectiveValue, PlaceLocationQualifierList.CustomsOfficeOfDestination, CodeListResponsibleAgencyCodedList.MutuallyDefined); }); //LOC+122
			AddNewLOCSegment(locSection, source.PortOfExit, PlaceLocationQualifierList.CustomsOfficeOfDestinationTransit, CodeListResponsibleAgencyCodedList.MutuallyDefined); // LOC+45
			AddNewLOCSegment(locSection, source.CountryOfExport, PlaceLocationQualifierList.CountryOfExportationDespatch, CodeListResponsibleAgencyCodedList.IsoInternationalOrganizationForStandardization); // LOC+35
			AddNewLOCSegment(locSection, source.CountryOfDestination, PlaceLocationQualifierList.CountryOfUltimateDestination, CodeListResponsibleAgencyCodedList.IsoInternationalOrganizationForStandardization); // LOC+36
			AddNewLOCSegment(locSection, source.CustomsOfficeCode, PlaceLocationQualifierList.PlaceOfLodgementOfDocuments, CodeListResponsibleAgencyCodedList.MutuallyDefined); // LOC+96
			AddNewLOCSegment(locSection, source.TransportDocumentIssuedAt, PlaceLocationQualifierList.PlacePortOfLoading, CodeListResponsibleAgencyCodedList.IsoInternationalOrganizationForStandardization); // LOC+9
		}

		static void AddNewLOCSegment(LOCSegmentMessageSection locSecition, ZString placeIdentification, PlaceLocationQualifierList locType, CodeListResponsibleAgencyCodedList responsibleAgency = null)
		{
			if (!placeIdentification.IsEmpty)
			{
				var locSegment = locSecition.InstantiateAChildAndAddItToChildrenCollection();
				locSegment.PlaceLocationQualifier = locType;
				locSegment.LocationIdentification.PlaceLocationIdentification = placeIdentification;
				if (responsibleAgency != null)
				{
					locSegment.LocationIdentification.CodeListResponsibleAgencyCoded = responsibleAgency;
				}
			}
		}

		static void PopulateDTMSegments(DTMSegmentMessageSection dtmSection, ICUSDECMessageDataProvider source)
		{
			AddNewDTMSegment(dtmSection, source.DateOfAssessment, DateTimePeriodQualifierList.PresentationDateOfGoodsDeclarationCustoms, DateTimePeriodFormatQualifierList.Ccyymmdd);
			AddNewDTMSegment(dtmSection, source.DateOfArrival, DateTimePeriodQualifierList.ArrivalDateTimeEstimated, DateTimePeriodFormatQualifierList.Ccyymmdd);
			AddNewDTMSegment(dtmSection, source.DateOfDepartureOrDateOfFlight, DateTimePeriodQualifierList.ArrivalDateTimeActual, DateTimePeriodFormatQualifierList.Ccyymmdd);
			if (source.ShouldOutputInvoiceDetails)
			{
				AddNewDTMSegment(dtmSection, source.ShippedOnBoardDate, DateTimePeriodQualifierList.ManifestShipNoticeDate, DateTimePeriodFormatQualifierList.Ccyymmdd);
			}
		}

		static void AddNewDTMSegment(DTMSegmentMessageSection dtmSection, ZDateTime dateTime, DateTimePeriodQualifierList dateTimeType, DateTimePeriodFormatQualifierList dateTimeFormat)
		{
			if (dateTime.IsValid)
			{
				var dtmSegment = dtmSection.InstantiateAChildAndAddItToChildrenCollection();
				dtmSegment.DateTimePeriod.DateTimePeriodQualifier = dateTimeType;
				dtmSegment.DateTimePeriod.DateTimePeriod = dateTime.ToCCYYMMDD();
				dtmSegment.DateTimePeriod.DateTimePeriodFormatQualifier = dateTimeFormat;
			}
		}

		static void PopulateGISSegments(GISSegmentMessageSection gisSection, ICUSDECMessageDataProvider source)
		{
			AddNewGISSegment(gisSection, source.RelatedPartyIndicator + source.ValuationCode, CodeListQualifierList.CustomsValuationMethod, CodeListResponsibleAgencyCodedList.MutuallyDefined);
			AddNewGISSegment(gisSection, source.PaymentMethod, CodeListQualifierList.DutyTaxOrFeePaymentMethod, CodeListResponsibleAgencyCodedList.MutuallyDefined);
			AddNewGISSegment(gisSection, source.RefundAcknowledgementIndicator, CodeListQualifierList.CustomsIndicator, CodeListResponsibleAgencyCodedList.MutuallyDefined);
		}

		static void AddNewGISSegment(GISSegmentMessageSection gisSection, ZString indicatorCode, CodeListQualifierList indicatorQualifier, CodeListResponsibleAgencyCodedList responsibleAgency)
		{
			if (indicatorCode.Length > 0)
			{
				var gisSegment = gisSection.InstantiateAChildAndAddItToChildrenCollection();
				gisSegment.ProcessingIndicator.ProcessingIndicatorCoded = ProcessingIndicatorCodedList.GetFromString(indicatorCode);
				gisSegment.ProcessingIndicator.CodeListQualifier = indicatorQualifier;
				gisSegment.ProcessingIndicator.CodeListResponsibleAgencyCoded = responsibleAgency;
			}
		}

		static void PopulateMEASegments(MEASegmentMessageSection meaSection, ICUSDECMessageDataProvider source)
		{
			AddNewMEASegment(meaSection, source.GrossWeightInKG, "KGM", MeasurementApplicationQualifierList.Measurement, 2, MeasurementDimensionCodedList.TotalGrossWeight);
		}

		static void AddNewMEASegment(MEASegmentMessageSection meaSection, ZDecimal value, ZString unit, MeasurementApplicationQualifierList qualifier, int valueDecimalPlace = 2, MeasurementDimensionCodedList dimension = null, bool alwaysSendEvenIfValueEmpty = false)
		{
			if (!unit.IsEmpty && (alwaysSendEvenIfValueEmpty || !value.IsEmpty))
			{
				var meaSegment = meaSection.InstantiateAChildAndAddItToChildrenCollection();
				meaSegment.MeasurementApplicationQualifier = qualifier;
				if (dimension != null)
				{
					meaSegment.MeasurementDetails.MeasurementDimensionCoded = dimension;
				}
				meaSegment.ValueRange.MeasureUnitQualifier = unit;
				meaSegment.ValueRange.MeasurementValue = value.ToString(valueDecimalPlace);
			}
		}

		static void PopulateEQDSegments(EQDSegmentMessageSection eqdSection, ICUSDECMessageDataProvider source)
		{
			if (source.Containers != null)
			{
				foreach (var container in source.Containers)
				{
					AddNewEQDSegment(eqdSection, EquipmentQualifierList.Container, container.ContainerNumber, container.GetFormatContainerSealNumber(), container.ContainerMode);
				}
			}
		}

		static void AddNewEQDSegment(EQDSegmentMessageSection eqdSection, EquipmentQualifierList equipmentType, ZString equipmentID, ZString containerSealNumber, ZString containerMode)
		{
			if (!(equipmentID.IsEmpty && containerSealNumber.IsEmpty && containerMode.IsEmpty))
			{
				var eqdSegment = eqdSection.InstantiateAChildAndAddItToChildrenCollection();
				eqdSegment.EquipmentQualifier = equipmentType;
				eqdSegment.EquipmentIdentification.EquipmentIdentificationNumber = equipmentID;
				eqdSegment.EquipmentSizeAndType.EquipmentSizeAndType = containerSealNumber;
				eqdSegment.FullEmptyIndicatorCoded = FullEmptyIndicatorCodedList.GetFromString(containerMode);
			}
		}

		static void PopulateFTXSegments(FTXSegmentMessageSection ftxSection, ICUSDECMessageDataProvider source)
		{
			var ftxSegment = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
			ftxSegment.TextSubjectQualifier = TextSubjectQualifierList.LineItem;
			var ftxLiteral = ftxSegment.TextLiteral;
			ftxLiteral.FreeText1 = source.TotalLineCount.ToString();
			ftxLiteral.FreeText2 = source.CreditTerms;
			ftxLiteral.FreeText3 = source.VATIndicator;
			ftxLiteral.FreeText4 = source.TransactionBankCode;
			ftxLiteral.FreeText5 = source.ChangeAcknowledgementIndicator;
		}

		#region SG1 Population

		static void PopulateSG1Groups(SegmentGroup1MessageSection sg1Section, ICUSDECMessageDataProvider source)
		{
			AddNewSG1Group(sg1Section, ReferenceQualifierList.HouseBillOfLadingNumber, source.HouseBill, source.HouseBillIssuedDate); //RFF+BH
			AddNewSG1Group(sg1Section, ReferenceQualifierList.CustomsDeclarationNumber, source.OriginalMRN, ZDateTime.Empty); //RFF+ABT
			AddNewSG1Group(sg1Section, ReferenceQualifierList.InBondNumber, source.MRNToBeReplaced, ZDateTime.Empty); //RFF+IB
			AddNewSG1Group(sg1Section, ReferenceQualifierList.TransportDocumentNumber, source.TransportDocumentNumber, source.TransportDocumentDate); //RFF+AAS, DTM+137
			if (source.ShouldOutputFinancialAccountNumber)
			{
				AddNewSG1Group(sg1Section, ReferenceQualifierList.DeferredPaymentReference, source.FinancialAccountNumber, ZDateTime.Empty); //RFF+ABI
			}
			AddNewSG1Group(sg1Section, ReferenceQualifierList.UniqueConsignmentReferenceNumber, source.UniqueConsignmentReference, ZDateTime.Empty);
			AddNewSG1Group(sg1Section, ReferenceQualifierList.AdditionalReferenceNumber, source.MessageNumber, ZDateTime.Empty); //RFF+ACD
			AddNewSG1Group(sg1Section, ReferenceQualifierList.EnquiryNumber, source.CaseNumber, ZDateTime.Empty);
			if (sg1Section.Count > 0)
			{
				var lastSG1 = sg1Section[sg1Section.Count - 1];
				var noOfPacks = source.TotalNoOfPacks;
				if (!noOfPacks.IsEmpty)
				{
					PopulateSG2GroupOnSG1(lastSG1, ZInt.ParseSafe(noOfPacks, ZInt.Zero), source.MarksAndNumbers);
				}
			}
		}

		static void AddNewSG1Group(SegmentGroup1MessageSection sg1Section, ReferenceQualifierList referenceQualifier, ZString referenceNumber, ZDateTime relatedDate)
		{
			if (!referenceNumber.IsEmpty)
			{
				var sg1Segment = sg1Section.InstantiateAChildAndAddItToChildrenCollection();
				AddNewRFFSegment(sg1Segment.RFF, referenceQualifier, referenceNumber);
				if (relatedDate.IsValid)
				{
					AddNewDTMSegment(sg1Segment.DTM, relatedDate, DateTimePeriodQualifierList.DocumentMessageDateTime, DateTimePeriodFormatQualifierList.Ccyymmdd);
				}
			}
		}

		static void PopulateSG2GroupOnSG1(SegmentGroup1 sg1, ZInt totalNoOfPacks, IEnumerable<ZString> marksAndNumbers)
		{
			if (sg1 != null)
			{
				var sg2 = sg1.Group2[0];
				var pacSegment = sg2.PAC[0];
				pacSegment.NumberOfPackages = totalNoOfPacks.ToString();
				var sg3Section = sg2.Group3;
				var spliter = new TextSplitter(35);
				foreach (var marknum in marksAndNumbers.Where(x => !x.IsEmpty))
				{
					spliter.Text = marknum;
					var sg3 = sg3Section.InstantiateAChildAndAddItToChildrenCollection();
					var marksLabels = sg3.PCI[0].MarksLabels;
					marksLabels.ShippingMarks1 = spliter[0];
					marksLabels.ShippingMarks2 = spliter[1];
					marksLabels.ShippingMarks3 = spliter[2];
					marksLabels.ShippingMarks4 = spliter[3];
					marksLabels.ShippingMarks5 = spliter[4];
					marksLabels.ShippingMarks6 = spliter[5];
					marksLabels.ShippingMarks7 = spliter[6];
					marksLabels.ShippingMarks8 = spliter[7];
					marksLabels.ShippingMarks9 = spliter[8];
					marksLabels.ShippingMarks10 = spliter[9];
				}
			}
		}

		#endregion

		#region SG4 Population

		static void PopulateSG4Groups(SegmentGroup4MessageSection sg4Section, ICUSDECMessageDataProvider source)
		{
			AddNewSG4Group(sg4Section, source.VoyageFlightNo, source.TransportMode, source.RemovalTransportMode, TransportStageQualifierList.MainCarriageTransport, source.TransportName);
		}

		static void AddNewSG4Group(SegmentGroup4MessageSection group4, ZString voyageFlightNo, ZString transportMode, ZString removalTransportMode, TransportStageQualifierList qualifier, ZString transportName)
		{
			var sg4 = group4.InstantiateAChildAndAddItToChildrenCollection();
			var tdtSegment = sg4.TDT[0];
			tdtSegment.TransportStageQualifier = qualifier;
			tdtSegment.ConveyanceReferenceNumber = voyageFlightNo;
			tdtSegment.ModeOfTransport.ModeOfTransportCoded = transportMode;
			tdtSegment.ModeOfTransport.ModeOfTransport = removalTransportMode;
			tdtSegment.TransportIdentification.IdOfTheMeansOfTransport = transportName;
		}

		#endregion

		#region SG5 Population

		static void PopulateSG5Groups(SegmentGroup5MessageSection sg5Section, ICUSDECMessageDataProvider source)
		{
			if (source.InvoiceInformations != null && !source.ShouldOutputInvoiceDetails)
			{
				foreach (var invoiceInfo in source.InvoiceInformations)
				{
					AddNewSG5Group(sg5Section, DocumentMessageNameCodedList.CommercialInvoice, invoiceInfo.InvoiceNumber, invoiceInfo.InvoiceDate.ToCCYYMMDD(), DateTimePeriodQualifierList.InvoiceDateTime, DateTimePeriodFormatQualifierList.Ccyymmdd);
				}
			}
		}

		static void AddNewSG5Group(SegmentGroup5MessageSection sg5Section, DocumentMessageNameCodedList documentType, ZString documentNumber, ZString documentDate, DateTimePeriodQualifierList dateTimeType, DateTimePeriodFormatQualifierList dateTimeFormat)
		{
			if (!documentNumber.IsEmpty)
			{
				var sg5 = sg5Section.InstantiateAChildAndAddItToChildrenCollection();
				var docSegment = sg5.DOC[0];
				docSegment.DocumentMessageName.DocumentMessageNameCoded = documentType;
				docSegment.DocumentMessageDetails.DocumentMessageNumber = documentNumber;
				if (documentDate.IsValid)
				{
					var dtmSegment = sg5.DTM[0];
					dtmSegment.DateTimePeriod.DateTimePeriodQualifier = dateTimeType;
					dtmSegment.DateTimePeriod.DateTimePeriodFormatQualifier = dateTimeFormat;
					dtmSegment.DateTimePeriod.DateTimePeriod = documentDate;
				}
			}
		}

		#endregion

		#region SG6 Population

		static void PopulateSG6Groups(SegmentGroup6MessageSection sg6Section, ICUSDECMessageDataProvider source)
		{
			var importerSG6 = AddNewSG6GroupWithAddress(sg6Section, PartyQualifierList.Importer, source.Importer, 8); //NAD+IM
			AddNewRFFSegment(importerSG6?.RFF, ReferenceQualifierList.VatRegistrationNumber, source.Importer?.VATRegistrationNo ?? ZString.Empty);

			AddNewSG6GroupWithCodeOnly(sg6Section, PartyQualifierList.AgentRepresentative, source.AgentCode, 8); //NAD+AG
			AddNewSG6GroupWithCodeOnly(sg6Section, PartyQualifierList.TransitPrincipal, source.RemoverTransporterCode, 8); //NAD+AF
			AddNewSG6GroupWithAddress(sg6Section, PartyQualifierList.Supplier, source.Supplier, 8); //NAD+SU

			var exporterSG6 = AddNewSG6GroupWithAddress(sg6Section, PartyQualifierList.Exporter, source.Exporter, 8); //NAD+EX
			AddNewRFFSegment(exporterSG6?.RFF, ReferenceQualifierList.VatRegistrationNumber, source.Exporter?.VATRegistrationNo ?? ZString.Empty);

			AddNewSG6GroupWithAddress(sg6Section, PartyQualifierList.Consignee, source.ImporterForExportJob, 8); //NAD+CN

			AddNewSG6GroupWithCodeOnly(sg6Section, PartyQualifierList.DocumentMessageIssuerSender, source.AgentDualProfileCode, 8); //NAD+MS
			AddNewSG6GroupWithCodeOnly(sg6Section, PartyQualifierList.Buyer, source.OwnerCode, 8); //NAD+BY
			AddNewSG6GroupWithAddress(sg6Section, PartyQualifierList.Declarant, source.UnregisteredTrader, 35); //NAD+DT

			AddNewSG6GroupWithCodeOnly(sg6Section, PartyQualifierList.CarriersAgent, source.VesselAgent, 4); //NAD+CG
			AddNewSG6GroupWithCodeOnly(sg6Section, PartyQualifierList.ReportingCarrierCustoms, source.MasterCargoCarrier, 4); //NAD+RL
		}

		static void AddNewSG6GroupWithCodeOnly(SegmentGroup6MessageSection sg6Section, PartyQualifierList addressType, ZString addressCode, int addressCodeSize)
		{
			if (addressType != null && !addressCode.IsEmpty)
			{
				var sg6 = sg6Section.InstantiateAChildAndAddItToChildrenCollection();
				AddNewNADSegmentWithCodeOnly(sg6.NAD, addressType, addressCode, addressCodeSize);
			}
		}

		static SegmentGroup6 AddNewSG6GroupWithAddress(SegmentGroup6MessageSection sg6Section, PartyQualifierList addressType, IAddressInformation addressInfo, int addressCodeSize)
		{
			SegmentGroup6 result = null;
			if (addressType != null && addressInfo != null)
			{
				result = sg6Section.InstantiateAChildAndAddItToChildrenCollection();
				var nadSegment = AddNewNADSegmentWithCodeOnly(result.NAD, addressType, addressInfo.OrganizationCode, addressCodeSize);
				if (nadSegment != null)
				{
					nadSegment.PartyQualifier = addressType;
					nadSegment.PartyIdentificationDetails.PartyIdIdentification = addressInfo.OrganizationCode;
					var partyIdQualifier = addressInfo.OrganizationCodeQualifier;
					if (!partyIdQualifier.IsEmpty)
					{
						nadSegment.PartyIdentificationDetails.CodeListQualifier = CodeListQualifierList.GetFromString(partyIdQualifier);
						nadSegment.PartyIdentificationDetails.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.MutuallyDefined;
					}
					nadSegment.PartyName.PartyName1 = addressInfo.Name.Left(35);
					var textSplitter = new TextSplitter(35);
					textSplitter.Text = addressInfo.Address;
					nadSegment.Street.StreetAndNumberPOBox1 = textSplitter[0];
					nadSegment.Street.StreetAndNumberPOBox2 = textSplitter[1];
					nadSegment.Street.StreetAndNumberPOBox3 = textSplitter[2];
					nadSegment.CityName = addressInfo.City.Left(35);
					nadSegment.PostcodeIdentification = addressInfo.PostCode.Left(9);
				}
			}
			return result;
		}

		static NADSegment AddNewNADSegmentWithCodeOnly(NADSegmentMessageSection nadSection, PartyQualifierList addressType, ZString addressCode, int addressCodeSize)
		{
			var result = nadSection.InstantiateAChildAndAddItToChildrenCollection();
			result.PartyQualifier = addressType;
			result.PartyIdentificationDetails.PartyIdIdentification = addressCode.Left(addressCodeSize);
			return result;
		}

		static RFFSegment AddNewRFFSegment(RFFSegmentMessageSection rffSection, ReferenceQualifierList referenceType, ZString referenceNumber)
		{
			RFFSegment rffSegment = null;
			if (rffSection != null && referenceType != null && !referenceNumber.IsEmpty)
			{
				rffSegment = rffSection.InstantiateAChildAndAddItToChildrenCollection();
				rffSegment.Reference.ReferenceQualifier = referenceType;
				rffSegment.Reference.ReferenceNumber = referenceNumber;
			}
			return rffSegment;
		}

		#endregion

		#region SG10 - SG27 Population

		static void PopulateSG10Groups(SegmentGroup10MessageSection sg10Section, ICUSDECMessageDataProvider source)
		{
			if (source.InvoiceHeaderInformations != null && source.ShouldOutputInvoiceDetails)
			{
				foreach (var invoiceInfo in source.InvoiceHeaderInformations)
				{
					AddNewSG10Group(sg10Section, invoiceInfo, source);
				}
			}
		}

		static void AddNewSG10Group(SegmentGroup10MessageSection sg10Section, IInvoiceHeaderInformation source, ICUSDECMessageDataProvider provider)
		{
			if (!source.InvoiceNumber.IsEmpty)
			{
				var sg10 = sg10Section.InstantiateAChildAndAddItToChildrenCollection();
				var dmsSegment = sg10.DMS[0];
				dmsSegment.DocumentMessageNumber = source.InvoiceNumber;
				dmsSegment.DocumentMessageNameCoded = DocumentMessageNameCodedList.CommercialInvoice;
				if (source.InvoiceDate.IsValid)
				{
					var dtmSegment = sg10.DTM[0];
					dtmSegment.DateTimePeriod.DateTimePeriodQualifier = DateTimePeriodQualifierList.InvoiceDateTime;
					dtmSegment.DateTimePeriod.DateTimePeriodFormatQualifier = DateTimePeriodFormatQualifierList.Ccyymmdd;
					dtmSegment.DateTimePeriod.DateTimePeriod = source.InvoiceDate.ToCCYYMMDD();
				}

				PopulateSG11SegmentGroup(sg10.Group11, source, provider.ExchangeRateDateTime);
				PopulateSG13Segment(sg10.Group13[0], source);
				PopulateSG14Segment(sg10.Group14[0], source);
				PopulateSG18Segment(sg10.Group18[0], source, provider);
				if (source.InvoiceChargeInformations != null)
				{
					foreach (var charge in source.InvoiceChargeInformations)
					{
						AddNewSG19Group(sg10.Group19, charge, provider.DateForDuty);
					}
				}
				if (source.InvoiceLineInformations != null)
				{
					foreach (var invoiceLine in source.InvoiceLineInformations)
					{
						AddNewSG21Group(sg10.Group21, invoiceLine);
					}
				}
			}
		}

		static void PopulateSG11SegmentGroup(SegmentGroup11MessageSection sg11Section, IInvoiceHeaderInformation source, ZDateTime exchangeRateDateTime)
		{
			var moaSegment1 = sg11Section[0].MOA[0];
			moaSegment1.MonetaryAmount.MonetaryAmountTypeQualifier = MonetaryAmountTypeQualifierList.InvoiceTotalAmount;
			moaSegment1.MonetaryAmount.CurrencyCoded = source.InvoiceCurrencyCoded;
			moaSegment1.MonetaryAmount.MonetaryAmount = source.TotalInvoiceAmount.ToString(2);
			PopulateSG12ExchangeRateSegment(sg11Section[0].Group12[0], source, exchangeRateDateTime);

			var moaSegment2 = sg11Section[1].MOA[0];
			moaSegment2.MonetaryAmount.MonetaryAmountTypeQualifier = MonetaryAmountTypeQualifierList.TotalCharges;
			moaSegment2.MonetaryAmount.CurrencyCoded = Core.Constants.CurrencyCodes.SouthAfrica;
			moaSegment2.MonetaryAmount.MonetaryAmount = source.TotalChargesInLocalCurrency.ToString(2);
			PopulateSG12TotalChargesSegment(sg11Section[1].Group12[0], source);
		}

		static void PopulateSG12ExchangeRateSegment(SegmentGroup12 sg12, IInvoiceHeaderInformation source, ZDateTime exchangeRateDateTime)
		{
			var cuxSegment1 = sg12.CUX[0];
			cuxSegment1.RateOfExchange = source.ExchangeRate.ToString(4);

			var dtmSegment = sg12.DTM[0];
			dtmSegment.DateTimePeriod.DateTimePeriodQualifier = DateTimePeriodQualifierList.RateOfExchangeDateTime;
			dtmSegment.DateTimePeriod.DateTimePeriodFormatQualifier = DateTimePeriodFormatQualifierList.Ccyymmdd;
			dtmSegment.DateTimePeriod.DateTimePeriod = exchangeRateDateTime.ToCCYYMMDD();
		}

		static void PopulateSG12TotalChargesSegment(SegmentGroup12 sg12, IInvoiceHeaderInformation source)
		{
			var cuxSegment1 = sg12.CUX[0];
			cuxSegment1.CurrencyDetails2.CurrencyDetailsQualifier = CurrencyDetailsQualifierList.CalculationBaseCurrency;
			cuxSegment1.RateOfExchange = source.CommonFactor.ToStringTrimZeros(9).Left(15);
		}

		static void PopulateSG13Segment(SegmentGroup13 sg13, IInvoiceHeaderInformation source)
		{
			var todSegment = sg13.TOD[0];
			todSegment.TermsOfDeliveryOrTransportFunctionCoded = TermsOfDeliveryOrTransportFunctionCodedList.DeliveryCondition;
			todSegment.TermsOfDeliveryOrTransport.TermsOfDeliveryOrTransportCoded = source.TermsOfDelivery;
		}

		static void PopulateSG14Segment(SegmentGroup14 sg14, IInvoiceHeaderInformation source)
		{
			var nadSegment = sg14.NAD[0];
			nadSegment.PartyQualifier = PartyQualifierList.IssuerOfInvoice;
			nadSegment.NameAndAddress.NameAndAddressLine1 = source.NameOfIssuer.Left(35);
			nadSegment.NameAndAddress.NameAndAddressLine2 = source.Address1.Left(35);
			nadSegment.NameAndAddress.NameAndAddressLine3 = source.Address2.Left(35);
			nadSegment.NameAndAddress.NameAndAddressLine4 = source.Address3.Left(35);
			nadSegment.NameAndAddress.NameAndAddressLine5 = source.Address4.Left(35);
			nadSegment.CountryCoded = source.CountryOfIssuer;
		}

		static void PopulateSG18Segment(SegmentGroup18 sg18, IInvoiceHeaderInformation source, ICUSDECMessageDataProvider provider)
		{
			if (!source.PaymentTerms.IsEmpty || provider.ShouldOutputInvoiceDetails)
			{
				var patSegment = sg18.PAT[0];
				patSegment.PaymentTermsTypeQualifier = PaymentTermsTypeQualifierList.Basic;
				patSegment.TermsTimeInformation.PaymentTimeReferenceCoded = PaymentTimeReferenceCodedList.DateInvoiceReceived;
				patSegment.TermsTimeInformation.TypeOfPeriodCoded = TypeOfPeriodCodedList.Day;
				if (ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today))
				{
					patSegment.TermsTimeInformation.NumberOfPeriods = source.PaymentTerms;
				}
				else
				{
					patSegment.TermsTimeInformation.NumberOfPeriods = new List<ZString> { CreditTermsCodeList.Codes.ADV, CreditTermsCodeList.Codes.NEP }.Contains(provider.CreditTerms) ? "0" : provider.PaymentTerms;
				}
			}

			var moaSegment = sg18.MOA[0];
			moaSegment.MonetaryAmount.MonetaryAmountTypeQualifier = MonetaryAmountTypeQualifierList.AmountToBePaidInAdvance;
			moaSegment.MonetaryAmount.CurrencyCoded = source.AdvancePaymentCurrencyCode;
			moaSegment.MonetaryAmount.MonetaryAmount = source.AdvancePaymentAmount.ToString(2);

			if (source.AdvancePaymentNotificationDetails?.Length > 0)
			{
				var apnDetailsSegment = sg18.FTX[0];
				apnDetailsSegment.TextSubjectQualifier = TextSubjectQualifierList.PaymentInformation;
				apnDetailsSegment.TextLiteral.FreeText1 = source.AdvancePaymentNotificationDetails.ElementAtOrDefault(0);
				apnDetailsSegment.TextLiteral.FreeText2 = source.AdvancePaymentNotificationDetails.ElementAtOrDefault(1);
				apnDetailsSegment.TextLiteral.FreeText3 = source.AdvancePaymentNotificationDetails.ElementAtOrDefault(2);
				apnDetailsSegment.TextLiteral.FreeText4 = source.AdvancePaymentNotificationDetails.ElementAtOrDefault(3);
				apnDetailsSegment.TextLiteral.FreeText5 = source.AdvancePaymentNotificationDetails.ElementAtOrDefault(4);
			}
		}

		static void AddNewSG19Group(SegmentGroup19MessageSection sg19Section, IInvoiceChargeInformation source, ZDateTime dateForRate)
		{
			var monetaryAmountTypeQualifier = MonetaryAmountTypeQualifierList.GetFromString(source.MonetaryAmountChargeType);
			if (!source.ChargeCurrency.IsEmpty && !string.IsNullOrEmpty(monetaryAmountTypeQualifier.ToString()))
			{
				var sg19 = sg19Section.InstantiateAChildAndAddItToChildrenCollection();
				var alcSegment = sg19.ALC[0];
				alcSegment.AllowanceOrChargeQualifier = AllowanceOrChargeQualifierList.Charge;

				if (source.IsOtherCharge)
				{
					var descriptionSpliter = new TextSplitter(35);
					descriptionSpliter.Text = source.ChargeDescription;
					alcSegment.SpecialServicesIdentification.SpecialService1 = descriptionSpliter[0];
					alcSegment.SpecialServicesIdentification.SpecialService2 = descriptionSpliter[1];
				}

				var moaSegment = sg19.MOA[0];
				moaSegment.MonetaryAmount.MonetaryAmountTypeQualifier = monetaryAmountTypeQualifier;
				moaSegment.MonetaryAmount.MonetaryAmount = source.ChargeAmount.ToString(2);
				moaSegment.MonetaryAmount.CurrencyCoded = source.ChargeCurrency;

				PopulateSG20Segment(sg19.Group20[0], source, dateForRate);
			}
		}

		static void PopulateSG20Segment(SegmentGroup20 sg20, IInvoiceChargeInformation source, ZDateTime dateForRate)
		{
			var cuxSegment = sg20.CUX[0];
			cuxSegment.CurrencyDetails1.CurrencyDetailsQualifier = CurrencyDetailsQualifierList.ChargePaymentCurrency;
			cuxSegment.CurrencyDetails1.CurrencyCoded = source.ChargeCurrency;
			cuxSegment.RateOfExchange = source.GetChargeCurrencyConversionRate(dateForRate).ToString(4);
		}

		static void AddNewSG21Group(SegmentGroup21MessageSection sg21Section, IInvoiceLineInformation source)
		{
			var sg21 = sg21Section.InstantiateAChildAndAddItToChildrenCollection();

			var linSegment = sg21.LIN[0];
			linSegment.LineItemNumber = source.InvoiceLineNumber.ToString();
			linSegment.SubLineInformation.LineItemNumber = source.RelatedDeclarationLineNumber.ToString();

			var piaSegment = sg21.PIA[0];
			piaSegment.ProductIdFunctionQualifier = ProductIdFunctionQualifierList.ProductIdentification;
			piaSegment.ItemNumberIdentification1.ItemNumber = source.ProductCode;

			var qtySegment = sg21.QTY[0];
			qtySegment.QuantityDetails.QuantityQualifier = QuantityQualifierList.InvoicedQuantity;
			qtySegment.QuantityDetails.Quantity = source.Quantity.ToString(4);
			qtySegment.QuantityDetails.MeasureUnitQualifier = source.QuantityUnit;

			var priSegment = sg21.PRI[0];
			priSegment.PriceInformation.PriceQualifier = PriceQualifierList.InvoicePrice;
			priSegment.PriceInformation.Price = source.PriceDetails.ToString(4);

			var moaSegment = sg21.MOA[0];
			moaSegment.MonetaryAmount.MonetaryAmountTypeQualifier = MonetaryAmountTypeQualifierList.InvoiceItemAmount;
			moaSegment.MonetaryAmount.MonetaryAmount = source.ItemAmount.ToString(2);

			PopulateSG23Segment(sg21.Group23[0], source);
			PopulateSG27Segment(sg21.Group27[0], source);
		}

		static void PopulateSG23Segment(SegmentGroup23 sg23, IInvoiceLineInformation source)
		{
			var alcSegment = sg23.ALC[0];
			alcSegment.AllowanceOrChargeQualifier = AllowanceOrChargeQualifierList.Allowance;

			var rteSegment = sg23.RTE[0];
			rteSegment.RateDetails.RateTypeQualifier = RateTypeQualifierList.AllowanceRate;
			rteSegment.RateDetails.RatePerUnit = source.RateDetails.ToString(0);

			if (source.InvoiceLineChargeInformations != null)
			{
				var discountCharge = source.InvoiceLineChargeInformations.FirstOrDefault(charge => charge.MonetaryDiscountAmount > 0);
				if (discountCharge != null)
				{
					var moaSegement = sg23.MOA[0];
					moaSegement.MonetaryAmount.MonetaryAmountTypeQualifier = MonetaryAmountTypeQualifierList.DiscountAmount;
					moaSegement.MonetaryAmount.MonetaryAmount = discountCharge.MonetaryDiscountAmount.ToString(4);
					moaSegement.MonetaryAmount.CurrencyCoded = discountCharge.ChargeCurrency;
				}
			}
		}

		static void PopulateSG27Segment(SegmentGroup27 sg27, IInvoiceLineInformation source)
		{
			var imdSegment = sg27.IMD[0];
			imdSegment.ItemDescriptionTypeCoded = ItemDescriptionTypeCodedList.FreeFormLongDescription;
			if (!source.BrandName.IsEmpty)
			{
				var brandNameSpliter = new TextSplitter(35);
				brandNameSpliter.Text = source.BrandName;
				imdSegment.ItemDescription.ItemDescription1 = brandNameSpliter[0];
				imdSegment.ItemDescription.ItemDescription2 = brandNameSpliter[1];
			}

			var itemDescriptionSpliter = new TextSplitter(70);
			itemDescriptionSpliter.Text = source.CommercialInvoiceItemDescription;
			var ftxSegment = sg27.FTX[0];
			ftxSegment.TextSubjectQualifier = TextSubjectQualifierList.CommercialInvoiceItemDescription;
			ftxSegment.TextLiteral.FreeText1 = itemDescriptionSpliter[0];
			ftxSegment.TextLiteral.FreeText2 = itemDescriptionSpliter[1];
			ftxSegment.TextLiteral.FreeText3 = itemDescriptionSpliter[2];
			ftxSegment.TextLiteral.FreeText4 = itemDescriptionSpliter[3];
			ftxSegment.TextLiteral.FreeText5 = itemDescriptionSpliter[4];
		}

		#endregion

		static void PopulateUNS1Segment(UNSSegment unsSegment)
		{
			unsSegment.SectionIdentification = SectionIdentificationList.HeaderDetailSectionSeparation;
		}

		static void PopulateSG30Groups(SegmentGroup30MessageSection sg30Section, ICUSDECMessageDataProvider source)
		{
			if (source.LineLevelDetails != null)
			{
				foreach (var line in source.LineLevelDetails)
				{
					AddNewSG30Group(sg30Section, line);
				}
			}
		}

		static void PopulateUNS2Segment(UNSSegment unsSegment)
		{
			unsSegment.SectionIdentification = SectionIdentificationList.DetailSummarySectionSeparation;
		}

		#region SG49 Population

		static void PopulateSG49Groups(SegmentGroup49MessageSection sg49Section, ICUSDECMessageDataProvider source)
		{
			AddNewSG49GroupIfNotZero(sg49Section, source.TotalCIFCAmount, Constants.DutyTaxFeeTypeCodes.CIFAndCValue, string.Empty, 0);

			if (source.ShouldOutputTotalTransactionValueAndCurrency)
			{
				var decimalPlaces = Math.Min(source.TotalTransactionValue.DecimalPlaces, 2);
				AddNewSG49Group(sg49Section, source.TotalTransactionValue, Constants.DutyTaxFeeTypeCodes.TransactionValue, source.TotalTransactionValueCurrency, decimalPlaces);
			}

			if (!source.TotalDutiesDue.IsEmpty || source.ShouldOutputDutiesDueWhenZero)
			{
				AddNewSG49Group(sg49Section, source.TotalDutiesDue, Constants.DutyTaxFeeTypeCodes.TotalDutiesDue);
			}

			if (!source.TotalVATDue.IsEmpty || source.ShouldOutputVATDueWhenZero)
			{
				AddNewSG49Group(sg49Section, source.TotalVATDue, Constants.DutyTaxFeeTypeCodes.TotalVATDue);
			}

			AddNewSG49GroupIfNotZero(sg49Section, source.OverpaidExcise, Constants.DutyTaxFeeTypeCodes.OverpaidExcise);
			AddNewSG49GroupIfNotZero(sg49Section, source.UnderpaidExcise, Constants.DutyTaxFeeTypeCodes.UnpaidExcise);
			AddNewSG49GroupIfNotZero(sg49Section, source.TotalCustomsValue, Constants.DutyTaxFeeTypeCodes.CustomsValue, string.Empty, 0);
		}

		static void AddNewSG49GroupIfNotZero(SegmentGroup49MessageSection sg49Section, ZDecimal value, ZString feeType, string currencyCode = "", int decimalPlace = 2)
		{
			if (!value.IsEmpty)
			{
				AddNewSG49Group(sg49Section, value, feeType, currencyCode, decimalPlace);
			}
		}

		static void AddNewSG49Group(SegmentGroup49MessageSection sg49Section, ZDecimal value, ZString feeType, string currencyCode = "", int decimalPlace = 2)
		{
			if (!feeType.IsEmpty)
			{
				var sg49 = sg49Section.InstantiateAChildAndAddItToChildrenCollection();
				AddNewTAXSegment(sg49.TAX, DutyTaxFeeTypeCodedList.GetFromString(feeType), DutyTaxFeeFunctionQualifierList.TotalOfEachDutyTaxOrFeeTypeCustomsDeclaration);
				AddNewMOASegment(sg49.MOA, value, MonetaryAmountTypeQualifierList.DutyTaxOrFeeAmount, currencyCode, decimalPlace);
			}
		}
		#endregion

		static void PopulateUNTSegment(UNTSegment untSegment, int segmentCount)
		{
			untSegment.NumberOfSegmentsInTheMessage = segmentCount.ToString(CultureInfo.InvariantCulture);
			untSegment.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
		}

		#endregion
	}
}

