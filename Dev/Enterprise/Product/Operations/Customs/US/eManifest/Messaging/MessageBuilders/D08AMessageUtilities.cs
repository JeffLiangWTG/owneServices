using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Edifact.D08A.Elements;
using Enterprise.Edifact.D08A.Messages.CUSRES;
using Enterprise.Edifact.D08A.Segments;
using Enterprise.Edifact.Utilities;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	//TODO: Move to base
	class D08AMessageUtilities
	{
		#region Populate Details

		#region PopulateUNH

		internal static void PopulateUNH(UNHSegment unh, string messageReferenceNumber, string messageType,
									   string messageVersionNumber, string messageReleaseNumber, string controllingAgency)
		{
			unh.MessageReferenceNumber = messageReferenceNumber;
			unh.MessageIdentifier.MessageType = messageType;
			unh.MessageIdentifier.MessageVersionNumber = messageVersionNumber;
			unh.MessageIdentifier.MessageReleaseNumber = messageReleaseNumber;
			unh.MessageIdentifier.ControllingAgency = controllingAgency;
		}

		#endregion

		#region PopulateBGM

		internal static void PopulateBGM(BGMSegment bgm, DocumentNameCodeList documentCode, string documentName,
									   string documentIdentifier, MessageFunctionCodeList messageFunctionCode)
		{
			bgm.DocumentMessageName.DocumentNameCode = documentCode;
			bgm.DocumentMessageName.DocumentName = documentName;
			bgm.DocumentMessageIdentification.DocumentIdentifier = documentIdentifier;
			bgm.MessageFunctionCode = messageFunctionCode;
		}

		#endregion

		#region PopulateDTM

		internal static void PopulateDTM(DTMSegment dtm, DateOrTimeOrPeriodFunctionCodeQualifierList dateTimePeriodQualifier, ZInt year)
		{
			dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = dateTimePeriodQualifier;
			dtm.DateTimePeriod.DateOrTimeOrPeriodText = year.ToString();
			dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyy;
		}

		internal static void PopulateDTM(DTMSegment dtm, DateOrTimeOrPeriodFunctionCodeQualifierList dateTimePeriodQualifier, ZDate date, bool invertedFormat = false)
		{
			dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = dateTimePeriodQualifier;
			dtm.DateTimePeriod.DateOrTimeOrPeriodText = invertedFormat ? date.ToString("ddMMyyyy", CultureInfo.InvariantCulture) : date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = invertedFormat ? DateOrTimeOrPeriodFormatCodeList.Ddmmccyy : DateOrTimeOrPeriodFormatCodeList.Ccyymmdd;
		}

		internal static void PopulateDTM(DTMSegment dtm, DateOrTimeOrPeriodFunctionCodeQualifierList dateTimePeriodQualifier, ZDateTime dateTime)
		{
			dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = dateTimePeriodQualifier;
			dtm.DateTimePeriod.DateOrTimeOrPeriodText = dateTime.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);
			dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmddhhmm;
		}

		#endregion

		#region PopulateLOC

		internal static void PopulateLOC(LOCSegment loc, LocationFunctionCodeQualifierList qualifier, string locationIdentifier, string identificationCode)
		{
			loc.LocationFunctionCodeQualifier = qualifier;
			loc.LocationIdentification.LocationIdentifier = locationIdentifier;
			loc.LocationIdentification.CodeListIdentificationCode = identificationCode;
		}

		internal static void PopulateLOC(LOCSegment loc, LocationFunctionCodeQualifierList qualifier,
			string locationName, string identificationCode,
			string locationName2, string identificationCode2,
			string locationName3 = null, string identificationCode3 = null)
		{
			loc.LocationFunctionCodeQualifier = qualifier;

			loc.LocationIdentification.LocationName = locationName;
			loc.LocationIdentification.CodeListIdentificationCode = identificationCode;

			if (!string.IsNullOrEmpty(locationName2))
			{
				loc.RelatedLocationOneIdentification.FirstRelatedLocationName = locationName2;
				loc.RelatedLocationOneIdentification.CodeListIdentificationCode = identificationCode2;
			}

			if (!string.IsNullOrEmpty(locationName3))
			{
				loc.RelatedLocationTwoIdentification.SecondRelatedLocationName = locationName3;
				loc.RelatedLocationTwoIdentification.CodeListIdentificationCode = identificationCode3;
			}
		}

		#endregion

		#region PopulateRFF

		internal static void PopulateRFF(RFFSegment rff, ReferenceCodeQualifierList referenceQualifier, string referenceIdentifier = null)
		{
			rff.Reference.ReferenceCodeQualifier = referenceQualifier;
			rff.Reference.ReferenceIdentifier = referenceIdentifier;
		}

		#endregion

		#region PopulateNAD

		internal static void PopulateNAD(NADSegment nad, PartyFunctionCodeQualifierList qualifier, string id, string identificationType, string id2 = null, string partyName = null)
		{
			nad.IsSpecialReplacingBehaviourOfInvalidCharacterOn = true;
			nad.PartyFunctionCodeQualifier = qualifier;
			nad.PartyIdentificationDetails.IsSpecialReplacingBehaviourOfInvalidCharacterOn = true;
			nad.PartyIdentificationDetails.PartyIdentifier = id;
			nad.PartyIdentificationDetails.CodeListIdentificationCode = identificationType;

			if (!string.IsNullOrEmpty(id2))
			{
				nad.NameAndAddress.IsSpecialReplacingBehaviourOfInvalidCharacterOn = true;
				nad.NameAndAddress.NameAndAddressDescription1 = id2;
			}

			if (!string.IsNullOrEmpty(partyName))
			{
				nad.PartyName.IsSpecialReplacingBehaviourOfInvalidCharacterOn = true;
				var splitter = new TextSplitter(35) { Text = partyName };
				if (splitter.Count > 0)
				{
					nad.PartyName.PartyName1 = splitter[0];
				}

				if (splitter.Count > 1)
				{
					nad.PartyName.PartyName2 = splitter[1];
				}
			}
		}

		internal static void PopulateNAD(NADSegment nad, PartyNameFormatCodeList format, string lastName, string firstName, string middleName)
		{
			nad.IsSpecialReplacingBehaviourOfInvalidCharacterOn = true;
			nad.PartyName.IsSpecialReplacingBehaviourOfInvalidCharacterOn = true;
			nad.PartyName.PartyName1 = lastName;
			nad.PartyName.PartyName2 = firstName;
			if (!string.IsNullOrEmpty(middleName))
			{
				nad.PartyName.PartyName3 = middleName;
			}

			nad.PartyName.PartyNameFormatCode = format;
		}

		#endregion

		#region PopulateATT

		internal static void PopulateATT(ATTSegment att, AttributeFunctionCodeQualifierList qualifier, string code)
		{
			att.AttributeFunctionCodeQualifier = qualifier;
			att.AttributeDetail.AttributeDescriptionCode = code;
		}

		#endregion

		#region PopulateEMP

		public static void PopulateEMP(EMPSegment emp, EmploymentDetailsCodeQualifierList qualifier, string description)
		{
			emp.EmploymentDetailsCodeQualifier = qualifier;
			emp.QualificationClassification.QualificationClassificationDescriptionCode = "1";
			emp.QualificationClassification.QualificationClassificationDescription1 = description;
			//TODO: There is a difference between the example and the spect, test which one is correct
		}

		#endregion

		#region PopulateNAT

		public static void PopulateNAT(NATSegment nat, NationalityCodeQualifierList qualifier, string countryCode, CodeListResponsibleAgencyCodeList responsibleAgency = null)
		{
			nat.NationalityCodeQualifier = qualifier;
			nat.NationalityDetails.NationalityNameCode = countryCode;
			nat.NationalityDetails.CodeListResponsibleAgencyCode = responsibleAgency;
		}

		#endregion

		#region PopulateCTA

		internal static void PopulateCTA(CTASegment cta, ContactFunctionCodeList contactType, string contactName = null)
		{
			cta.IsSpecialReplacingBehaviourOfInvalidCharacterOn = true;
			cta.ContactFunctionCode = contactType;
			cta.ContactDetails.IsSpecialReplacingBehaviourOfInvalidCharacterOn = true;
			cta.ContactDetails.ContactName = contactName;
		}

		#endregion

		#region PopulateCOM

		internal static void PopulateCOM(COMSegment com, CommunicationMeansTypeCodeList code, ZString number)
		{
			com.CommunicationContact.CommunicationAddressIdentifier = number;
			com.CommunicationContact.CommunicationMeansTypeCode = code;
		}

		#endregion

		#region PopulateFTX

		public static void PopulateFTX(FTXSegment ftx, TextSubjectCodeQualifierList textSubjectCode, IEnumerable<ZString> freeTextParts)
		{
			PopulateFTX(
				ftx,
				textSubjectCode,
				freeTextParts.ElementAtOrDefault(0),
				freeTextParts.ElementAtOrDefault(1),
				freeTextParts.ElementAtOrDefault(2),
				freeTextParts.ElementAtOrDefault(3),
				freeTextParts.ElementAtOrDefault(4));
		}

		public static void PopulateFTX(FTXSegment ftx, TextSubjectCodeQualifierList textSubjectCode,
			string freeText1, string freeText2 = null, string freeText3 = null, string freeText4 = null, string freeText5 = null)
		{
			ftx.TextSubjectCodeQualifier = textSubjectCode;
			ftx.TextLiteral.FreeText1 = freeText1;
			ftx.TextLiteral.FreeText2 = freeText2;
			ftx.TextLiteral.FreeText3 = freeText3;
			ftx.TextLiteral.FreeText4 = freeText4;
			ftx.TextLiteral.FreeText5 = freeText5;
		}

		#endregion

		#region PopulateTDT

		public static void PopulateTDT(TDTSegment tdt, TransportStageCodeQualifierList transportStageQualifier)
		{
			PopulateTDT(tdt, transportStageQualifier, null, null, null, null, null);
		}

		public static void PopulateTDT(TDTSegment tdt, TransportStageCodeQualifierList transportStageQualifier, string transportMode, string conveyanceType,
									   string transitDirectionCode, string identificationCode, string identifier, string registrationCountry = null)
		{
			tdt.TransportStageCodeQualifier = transportStageQualifier;
			tdt.ModeOfTransport.TransportModeNameCode = transportMode;
			tdt.TransportMeans.TransportMeansDescription = conveyanceType;
			tdt.TransitDirectionIndicatorCode = TransitDirectionIndicatorCodeList.GetFromString(transitDirectionCode);
			tdt.TransportIdentification.CodeListIdentificationCode = identificationCode;
			tdt.TransportIdentification.TransportMeansIdentificationName = identifier;
			tdt.TransportIdentification.TransportMeansNationalityCode = registrationCountry;
		}

		public static void PopulateTDT(TDTSegment tdt, TransportStageCodeQualifierList transportStageQualifier, string transportMode, string carrierCode, string identificationCode)
		{
			tdt.TransportStageCodeQualifier = transportStageQualifier;
			tdt.ModeOfTransport.TransportModeNameCode = transportMode;
			tdt.Carrier.CarrierIdentifier = carrierCode;
			tdt.Carrier.CodeListIdentificationCode = identificationCode;
		}

		#endregion

		#region PopulateEQD

		public static void PopulateEQD(EQDSegment eqd, EquipmentTypeCodeQualifierList qualifier, string number, string identificationCode)
		{
			eqd.EquipmentTypeCodeQualifier = qualifier;
			eqd.EquipmentIdentification.EquipmentIdentifier = number;
			eqd.EquipmentIdentification.CodeListIdentificationCode = identificationCode;
		}

		#endregion

		#region PopulateSEL

		internal static void PopulateSEL(SELSegment sel, string sealNumber)
		{
			sel.TransportUnitSealIdentifier = sealNumber;
		}

		#endregion

		#region PopulateCNI

		internal static void PopulateCNI(CNISegment cni, string number, DocumentStatusCodeList action)
		{
			cni.ConsolidationItemNumber = number;
			cni.DocumentMessageDetails.DocumentStatusCode = action;
		}

		#endregion

		#region PopulateDOC

		public static void PopulateDOC(DOCSegment doc, DocumentNameCodeList code, string name, string identifier, string sourceDescription = null, DocumentStatusCodeList statusCode = null)
		{
			doc.DocumentMessageName.DocumentNameCode = code;
			doc.DocumentMessageName.DocumentName = name;

			doc.DocumentMessageDetails.DocumentStatusCode = statusCode;
			if (!string.IsNullOrEmpty(identifier))
			{
				doc.DocumentMessageDetails.DocumentIdentifier = identifier;
				doc.DocumentMessageDetails.DocumentSourceDescription = sourceDescription;
			}
		}

		#endregion

		#region PopulateCNT

		internal static void PopulateCNT(CNTSegment cnt, ControlTotalTypeCodeQualifierList qualifier, decimal quantity)
		{
			cnt.Control.ControlTotalTypeCodeQualifier = qualifier;
			cnt.Control.ControlTotalQuantity = quantity.ToString("0", CultureInfo.InvariantCulture);
		}

		#endregion

		#region PopulateQTY

		internal static void PopulateQTY(QTYSegment qty, QuantityTypeCodeQualifierList qualifier, int quantity)
		{
			qty.QuantityDetails.QuantityTypeCodeQualifier = qualifier;
			qty.QuantityDetails.Quantity = quantity.ToString("0", CultureInfo.InvariantCulture);
		}

		#endregion

		#region PopulateGEI

		public static void PopulateGEI(GEISegment gei, ProcessingInformationCodeQualifierList qualifier, string customsProcedureCode)
		{
			gei.ProcessingInformationCodeQualifier = qualifier;
			gei.ProcessingIndicator.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(customsProcedureCode);
		}

		#endregion

		#region PopulateTSR

		public static void PopulateTSR(TSRSegment tsr, ContractAndCarriageConditionCodeList code)
		{
			tsr.ContractAndCarriageCondition.ContractAndCarriageConditionCode = code;
		}

		#endregion

		#region PopulateGID

		public static void PopulateGID(GIDSegment gid, int number)
		{
			gid.GoodsItemNumber = number.ToString(CultureInfo.InvariantCulture);
		}

		#endregion

		#region PopulatePAC

		public static void PopulatePAC(PACSegment pac, ZInt packages, string packType)
		{
			pac.PackageQuantity = packages.ToString();
			pac.PackageType.PackageTypeDescriptionCode = packType;
		}

		#endregion

		#region PopulateMEA

		public static void PopulateMEA(MEASegment mea, MeasurementPurposeCodeQualifierList qualifier, ZDecimal value, ZString units)
		{
			mea.MeasurementPurposeCodeQualifier = qualifier;
			mea.ValueRange.MeasurementUnitCode = units;
			mea.ValueRange.Measure = value.ToString("0.######", CultureInfo.InvariantCulture);
		}

		#endregion

		#region PopulateMOA

		public static void PopulateMOA(MOASegment moa, MonetaryAmountTypeCodeQualifierList qualifier, ZDecimal amount, int decimals = 2, string currencyCode = null)
		{
			moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier = qualifier;
			moa.MonetaryAmount.MonetaryAmount = amount.Round(decimals).ToString(decimals);
			moa.MonetaryAmount.CurrencyIdentificationCode = currencyCode;
		}

		#endregion

		#region PopulateSGP

		public static void PopulateSGP(SGPSegment sgp, string equipmentId, string idType)
		{
			sgp.EquipmentIdentification.EquipmentIdentifier = equipmentId;
			sgp.EquipmentIdentification.CodeListIdentificationCode = idType;
		}

		#endregion

		#region PopulateDGS

		public static void PopulateDGS(DGSSegment dgs, string dgCode)
		{
			dgs.UndgInformation.UnitedNationsDangerousGoods = dgCode;
		}

		#endregion

		#region PopulatePCI

		public static void PopulatePCI(PCISegment pci, IEnumerable<ZString> marks)
		{
			pci.MarksLabels.ShippingMarksDescription1 = marks.ElementAtOrDefault(0);
			pci.MarksLabels.ShippingMarksDescription2 = marks.ElementAtOrDefault(1);
			pci.MarksLabels.ShippingMarksDescription3 = marks.ElementAtOrDefault(2);
			pci.MarksLabels.ShippingMarksDescription4 = marks.ElementAtOrDefault(3);
			pci.MarksLabels.ShippingMarksDescription5 = marks.ElementAtOrDefault(4);
			pci.MarksLabels.ShippingMarksDescription6 = marks.ElementAtOrDefault(5);
			pci.MarksLabels.ShippingMarksDescription7 = marks.ElementAtOrDefault(6);
			pci.MarksLabels.ShippingMarksDescription8 = marks.ElementAtOrDefault(7);
			pci.MarksLabels.ShippingMarksDescription9 = marks.ElementAtOrDefault(8);
			pci.MarksLabels.ShippingMarksDescription10 = marks.ElementAtOrDefault(9);
		}

		#endregion

		#region PopulateCST

		public static void PopulateCST(CSTSegment cst, IEnumerable<ZString> codes, string idCode)
		{
			PopulateCST(cst.CustomsIdentityCodes1, codes.ElementAtOrDefault(0), idCode);
			PopulateCST(cst.CustomsIdentityCodes2, codes.ElementAtOrDefault(1), idCode);
			PopulateCST(cst.CustomsIdentityCodes3, codes.ElementAtOrDefault(2), idCode);
			PopulateCST(cst.CustomsIdentityCodes4, codes.ElementAtOrDefault(3), idCode);
			PopulateCST(cst.CustomsIdentityCodes5, codes.ElementAtOrDefault(4), idCode);
		}

		static void PopulateCST(CustomsIdentityCodesElements element, ZString id, string idCode)
		{
			if (!id.IsEmpty)
			{
				element.CustomsGoodsIdentifier = id;
				element.CodeListIdentificationCode = idCode;
			}
		}

		#endregion

		#region PopulateUNT

		public static void PopulateUNT(UNTSegment unt, string numberOfSegmentsInTheMessage, string messageReferenceNumber)
		{
			unt.NumberOfSegmentsInTheMessage = numberOfSegmentsInTheMessage;
			unt.MessageReferenceNumber = messageReferenceNumber;
		}

		#endregion

		#region PopulateTAX

		internal static void PopulateTAX(TAXSegment tax, DutyOrTaxOrFeeFunctionCodeQualifierList qualifier)
		{
			tax.DutyOrTaxOrFeeFunctionCodeQualifier = qualifier;
		}

		#endregion

		#region PopulateFII

		internal static void PopulateFII(FIISegment fii, PartyFunctionCodeQualifierList qualifier, ZString name)
		{
			fii.PartyFunctionCodeQualifier = qualifier;
			fii.InstitutionIdentification.InstitutionName = name;
		}

		#endregion

		#region PopulatePNA

		public static void PopulatePNA(PNASegment pna, PartyFunctionCodeQualifierList qualifier,
			string name1, NameComponentTypeCodeQualifierList nameComponent1,
			string name2 = null, NameComponentTypeCodeQualifierList nameComponent2 = null,
			string name3 = null, NameComponentTypeCodeQualifierList nameComponent3 = null)
		{
			pna.PartyFunctionCodeQualifier = qualifier;
			pna.NameComponentDetails1.NameComponentTypeCodeQualifier = nameComponent1;
			pna.NameComponentDetails1.NameComponentDescription = name1;
			pna.NameComponentDetails2.NameComponentTypeCodeQualifier = nameComponent2;
			pna.NameComponentDetails2.NameComponentDescription = name2;
			pna.NameComponentDetails3.NameComponentTypeCodeQualifier = nameComponent3;
			pna.NameComponentDetails3.NameComponentDescription = name3;
		}

		#endregion

		#region PopulateGIS

		public static void PopulateGIS(GISSegment gis, ProcessingIndicatorDescriptionCodeList processingIndicator, string processType)
		{
			gis.ProcessingIndicator.ProcessingIndicatorDescriptionCode = processingIndicator;
			gis.ProcessingIndicator.ProcessingIndicatorDescription = processType;
		}

		#endregion

		#region PopulatePDI

		internal static void PopulatePDI(PDISegment pdi, string genderCode)
		{
			pdi.GenderCode = genderCode;
		}

		#endregion

		#region PopulateIHC

		public static void PopulateIHC(IHCSegment ihc, PersonCharacteristicCodeQualifierList qualifier, string equipmentType)
		{
			ihc.PersonCharacteristicCodeQualifier = qualifier;
			ihc.PersonInheritedCharacteristicDetails.InheritedCharacteristicDescription = equipmentType;
		}

		#endregion

		#endregion

		#region Get Details

		public static ZDecimal GetAmount(SegmentGroup9MessageSection group9Section, MonetaryAmountTypeCodeQualifierList qualifier)
		{
			return (from SegmentGroup9 group9 in group9Section
					from MOASegment moa in group9.MOA
					where moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == qualifier
					select ZDecimal.ParseSafe(moa.MonetaryAmount.MonetaryAmount, 0)).FirstOrDefault();
		}

		internal static ZString GetContact(SegmentGroup8MessageSection group8Section, CommunicationMeansTypeCodeList qualifier)
		{
			return (from SegmentGroup8 group8 in group8Section
					from COMSegment com in group8.COM
					where com.CommunicationContact.CommunicationMeansTypeCode == qualifier
					select com.CommunicationContact.CommunicationAddressIdentifier).FirstOrDefault();
		}

		internal static IEnumerable<ZString> GetCodes(SegmentGroup12MessageSection group12Section, string identificationCode)
		{
			return from SegmentGroup12 group12 in group12Section
				   from CSTSegment cst in group12.CST
				   where cst.CustomsIdentityCodes1.CodeListIdentificationCode == identificationCode
				   select (ZString)cst.CustomsIdentityCodes1.CustomsGoodsIdentifier;
		}

		internal static ZString GetLocation(LOCSegmentMessageSection locSection, LocationFunctionCodeQualifierList qualifier, string identificationCode = "")
		{
			return (from LOCSegment loc in locSection
					where loc.LocationFunctionCodeQualifier == qualifier
						  && (string.IsNullOrEmpty(identificationCode) || loc.LocationIdentification.CodeListIdentificationCode == identificationCode)
					select loc.LocationIdentification.LocationIdentifier).FirstOrDefault();
		}

		internal static ZString GetProcessingIndicator(GEISegmentMessageSection geiSection, ProcessingInformationCodeQualifierList qualifier)
		{
			return (from GEISegment gei in geiSection
					where gei.ProcessingInformationCodeQualifier == qualifier
					select gei.ProcessingIndicator.ProcessingIndicatorDescriptionCode.ToString()).FirstOrDefault();
		}

		internal static ZString GetShippingMarks(PCISegmentMessageSection pciSection)
		{
			return (from PCISegment pci in pciSection select (ZString)pci.MarksLabels.ShippingMarksDescription1).ToStringDelimited(string.Empty);
			//TODO: Test that they always return single description per segment
		}

		#region Notifications

		internal static IEnumerable<string[]> GetErrorCodesAndRejectComments(SegmentGroup4MessageSection group4Section)
		{
			return from SegmentGroup4 group4 in group4Section
				   from pair in GetErrorCodesAndRejectComments(group4.ERC, group4.FTX)
				   select pair;
		}

		internal static IEnumerable<string[]> GetErrorCodesAndRejectComments(SegmentGroup14MessageSection group14Section)
		{
			return from SegmentGroup14 group14 in group14Section
				   from pair in GetErrorCodesAndRejectComments(group14.ERC, group14.FTX)
				   select pair;
		}

		static IEnumerable<string[]> GetErrorCodesAndRejectComments(ERCSegmentMessageSection ercSection, FTXSegmentMessageSection ftxSection)
		{
			for (var i = 0; i < Math.Max(ercSection.Count, ftxSection.Count); i++)
			{
				var code = i < ercSection.Count ? ercSection[i].ApplicationErrorDetail.ApplicationErrorCode : string.Empty;
				var comments = i < ftxSection.Count ? ftxSection[i].TextLiteral.FreeText1 : string.Empty;
				yield return new[] { code, comments };
			}
		}

		#endregion

		#region Free Text

		internal static ZString GetFreeText(FTXSegmentMessageSection ftxSection, TextSubjectCodeQualifierList qualifier)
		{
			return (from FTXSegment ftx in ftxSection
					where ftx.TextSubjectCodeQualifier == qualifier
					let text = ftx.TextLiteral
					let parts = new ZString[] { text.FreeText1, text.FreeText2, text.FreeText3, text.FreeText4, text.FreeText5 }
					select parts.ToStringDelimited(System.Environment.NewLine).TrimEnd()).FirstOrDefault();
		}

		internal static IEnumerable<ZString> GetFreeTextGroupBySegment(SegmentGroup12MessageSection group12Section, TextSubjectCodeQualifierList qualifier)
		{
			return from SegmentGroup12 group12 in group12Section
				   from FTXSegment ftx in group12.FTX
				   where ftx.TextSubjectCodeQualifier == qualifier
				   let text = ftx.TextLiteral
				   let parts = new ZString[] { text.FreeText1, text.FreeText2, text.FreeText3, text.FreeText4, text.FreeText5 }
				   select parts.ToStringDelimited(System.Environment.NewLine).TrimEnd();
		}

		internal static IEnumerable<ZString> GetFreeTextGroupBySegment(FTXSegmentMessageSection ftxSection, TextSubjectCodeQualifierList qualifier)
		{
			return from FTXSegment ftx in ftxSection
				   where ftx.TextSubjectCodeQualifier == qualifier
				   let text = ftx.TextLiteral
				   let parts = new ZString[] { text.FreeText1, text.FreeText2, text.FreeText3, text.FreeText4, text.FreeText5 }
				   select parts.ToStringDelimited(System.Environment.NewLine).TrimEnd();
		}

		internal static IEnumerable<ZString> GetFreeTextAsEnumerable(SegmentGroup12MessageSection group12Section, TextSubjectCodeQualifierList qualifier)
		{
			foreach (var text in from SegmentGroup12 group12 in group12Section
								 from FTXSegment ftx in group12.FTX
								 where ftx.TextSubjectCodeQualifier == qualifier
								 select ftx.TextLiteral)
			{
				if (!string.IsNullOrEmpty(text.FreeText1))
				{
					yield return text.FreeText1;
				}

				if (!string.IsNullOrEmpty(text.FreeText2))
				{
					yield return text.FreeText2;
				}

				if (!string.IsNullOrEmpty(text.FreeText3))
				{
					yield return text.FreeText3;
				}

				if (!string.IsNullOrEmpty(text.FreeText4))
				{
					yield return text.FreeText4;
				}

				if (!string.IsNullOrEmpty(text.FreeText5))
				{
					yield return text.FreeText5;
				}
			}
		}

		#endregion

		#region Quantity

		internal static ZDecimal GetQuantity(MEASegmentMessageSection meaSection, MeasurementPurposeCodeQualifierList qualifier)
		{
			return (from MEASegment mea in meaSection
					where mea.MeasurementPurposeCodeQualifier == qualifier
					select ZDecimal.ParseSafe(mea.ValueRange.Measure, 0)).FirstOrDefault();
		}

		internal static ZString GetUnitOfMeasure(MEASegmentMessageSection meaSection, MeasurementPurposeCodeQualifierList qualifier)
		{
			return (from MEASegment mea in meaSection
					where mea.MeasurementPurposeCodeQualifier == qualifier
					select mea.ValueRange.MeasurementUnitCode).FirstOrDefault();
		}

		internal static ZDecimal GetQuantity(PACSegmentMessageSection pacSection)
		{
			return (from PACSegment pac in pacSection
					select ZDecimal.ParseSafe(pac.PackageQuantity, 0)).FirstOrDefault();
		}

		internal static ZString GetUnitOfMeasure(PACSegmentMessageSection pacSection)
		{
			return (from PACSegment mea in pacSection
					select mea.PackageType.PackageTypeDescriptionCode).FirstOrDefault();
		}

		#endregion

		#region Doc References

		internal static ZString GetDocType(DOCSegmentMessageSection docSection, DocumentNameCodeList qualifier)
		{
			return (from DOCSegment doc in docSection
					where doc.DocumentMessageName.DocumentNameCode == qualifier
					select doc.DocumentMessageName.DocumentName).FirstOrDefault();
		}

		internal static ZString GetDocReference(DOCSegmentMessageSection docSection, DocumentNameCodeList qualifier)
		{
			return (from DOCSegment doc in docSection
					where doc.DocumentMessageName.DocumentNameCode == qualifier
					select doc.DocumentMessageDetails.DocumentIdentifier).FirstOrDefault();
		}

		internal static ZString GetDocReferenceWithSource(DOCSegmentMessageSection docSection, DocumentNameCodeList qualifier)
		{
			return (from DOCSegment doc in docSection
					where doc.DocumentMessageName.DocumentNameCode == qualifier
					select doc.DocumentMessageDetails.DocumentSourceDescription + doc.DocumentMessageDetails.DocumentIdentifier).FirstOrDefault();
		}

		#endregion

		#region Message References

		internal static ZString GetMessageReference(BGMSegmentMessageSection bgmSection)
		{
			return (from BGMSegment bgm in bgmSection select bgm.DocumentMessageIdentification.DocumentIdentifier).FirstOrDefault();
		}

		internal static DocumentNameCodeList GetMessageCode(BGMSegmentMessageSection bgmSection)
		{
			return (from BGMSegment bgm in bgmSection select bgm.DocumentMessageName.DocumentNameCode).FirstOrDefault();
		}

		internal static MessageFunctionCodeList GetMessageFunction(BGMSegmentMessageSection bgmSection)
		{
			return (from BGMSegment bgm in bgmSection select bgm.MessageFunctionCode).FirstOrDefault();
		}

		#endregion

		#region Party References

		internal static ZString GetPartyReference(SegmentGroup7MessageSection group7Section, PartyFunctionCodeQualifierList qualifier)
		{
			return (from SegmentGroup7 group7 in group7Section
					from NADSegment nad in group7.NAD
					where nad.PartyFunctionCodeQualifier == qualifier
					select nad.PartyIdentificationDetails.PartyIdentifier).FirstOrDefault();
		}

		internal static ZString GetPartyReference(SegmentGroup1MessageSection group1Section, PartyFunctionCodeQualifierList qualifier)
		{
			return (from SegmentGroup1 group1 in group1Section
					from NADSegment nad in group1.NAD
					where nad.PartyFunctionCodeQualifier == qualifier
					select nad.PartyIdentificationDetails.PartyIdentifier).FirstOrDefault();
		}

		#endregion

		#region References

		internal static ZString GetReference(RFFSegmentMessageSection rffSection, ReferenceCodeQualifierList qualifier)
		{
			return (from RFFSegment rff in rffSection
					where rff.Reference.ReferenceCodeQualifier == qualifier
					select rff.Reference.ReferenceIdentifier).FirstOrDefault();
		}

		internal static ZString GetReference(IEnumerable group3Section, ReferenceCodeQualifierList qualifier)
		{
			return GetReferences(group3Section, qualifier).FirstOrDefault();
		}

		internal static IEnumerable<ZString> GetReferences(IEnumerable group3Section, ReferenceCodeQualifierList qualifier)
		{
			return from SegmentGroup3 group3 in group3Section
				   from RFFSegment rff in group3.RFF
				   where rff.Reference.ReferenceCodeQualifier == qualifier
				   select (ZString)rff.Reference.ReferenceIdentifier;
		}

		internal static IEnumerable<SegmentGroup3> GetReferenceGroups(IEnumerable group3Section, ReferenceCodeQualifierList qualifier)
		{
			return from SegmentGroup3 group3 in group3Section
				   where group3.RFF.Cast<RFFSegment>().Any(rff => rff.Reference.ReferenceCodeQualifier == qualifier)
				   select group3;
		}

		#endregion

		#region Transport Details

		internal static TDTSegment GetTransportDetails(TDTSegmentMessageSection tdtSection, string identificationCode)
		{
			return (from TDTSegment tdt in tdtSection
					where tdt.TransportIdentification.CodeListIdentificationCode == identificationCode
					select tdt).FirstOrDefault();
		}

		internal static string GetTransportIdentifier(TDTSegmentMessageSection tdtSection, string identificationCode)
		{
			var details = GetTransportDetails(tdtSection, identificationCode);
			return details == null ? string.Empty : details.TransportIdentification.TransportMeansIdentificationName;
		}

		#endregion

		#region Date

		internal static ZDateTime GetDateTime(DTMSegmentMessageSection dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList qualifier)
		{
			return (from DTMSegment dtm in dtmSection
					where dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == qualifier
					select ParseDateTime(dtm.DateTimePeriod.DateOrTimeOrPeriodText)).FirstOrDefault();
		}

		static ZDateTime ParseDateTime(string dateTime)
		{
			ZDateTime result;
			if (ZDateTime.TryParseExact(dateTime, out result, "yyyyMMddHHmm")
				|| ZDateTime.TryParseExact(dateTime, out result, "yyyyMMdd")
				|| ZDateTime.TryParseExact(dateTime, out result, "ddMMyyyy"))
			{
			}
			return result;
		}

		#endregion

		#endregion
	}
}
