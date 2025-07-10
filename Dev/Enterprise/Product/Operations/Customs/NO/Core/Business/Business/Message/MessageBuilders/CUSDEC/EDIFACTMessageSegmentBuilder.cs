using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.Types;
using Enterprise.Edifact.Utilities;
using Enterprise.Edifact.V902.Elements;
using Enterprise.Edifact.V902.Segments;

namespace Enterprise.Customs.NO.Business;

static class EDIFACTMessageSegmentBuilder
{
	public static void AddNewLOCSegment(LOCSegmentMessageSection locSection, PlaceLocationQualifierList locationQualifier, ZString locationIdentification, CodeListResponsibleAgencyCodedList responsibleAgency = null)
	{
		if (locationQualifier is not null && !locationIdentification.IsEmpty)
		{
			var locSegment = locSection.InstantiateAChildAndAddItToChildrenCollection();
			locSegment.LocationIdentification1.PlaceLocationQualifier = locationQualifier;
			locSegment.LocationIdentification1.PlaceLocationIdentification = locationIdentification;
			locSegment.LocationIdentification1.CodeListResponsibleAgencyCoded = responsibleAgency;
		}
	}

	public static void AddNewMEASegment(MEASegmentMessageSection meaSection, MeasurementDimensionCodedList measurementDimension, ZString measurementUnit, ZString measurementValue)
	{
		if (!measurementValue.IsEmpty)
		{
			var meaSegment = meaSection.InstantiateAChildAndAddItToChildrenCollection();
			meaSegment.MeasurementApplicationQualifier = MeasurementApplicationQualifierList.CustomsLineItemMeasurement;
			meaSegment.MeasurementDetails.MeasurementDimensionCoded = measurementDimension;
			meaSegment.ValueRange.MeasureUnitQualifier = measurementUnit;
			meaSegment.ValueRange.MeasurementValue = measurementValue;
		}
	}

	public readonly struct MonetaryAmountElementValues(MonetaryAmountTypeQualifierList monetaryAmountType, ZString monetaryAmount, ZString monetaryCurrency, ZBool isMandatory)
	{
		public MonetaryAmountElementValues(MonetaryAmountTypeQualifierList monetaryAmountType, ZString monetaryAmount)
			: this(monetaryAmountType, monetaryAmount, ZString.Empty, false)
		{ }

		public MonetaryAmountElementValues(MonetaryAmountTypeQualifierList monetaryAmountType, ZString monetaryAmount, ZBool isMandatory)
			: this(monetaryAmountType, monetaryAmount, ZString.Empty, isMandatory)
		{ }

		public ZBool IsMandatory { get; } = isMandatory;
		public MonetaryAmountTypeQualifierList MonetaryAmountType { get; } = monetaryAmountType;
		public ZString MonetaryAmount { get; } = monetaryAmount;
		public ZString MonetaryCurrency { get; } = monetaryCurrency;

		public static implicit operator MonetaryAmountElementValues((MonetaryAmountTypeQualifierList monetaryAmountType, ZString monetaryAmount) values)
			=> new(values.monetaryAmountType, values.monetaryAmount);

		public static implicit operator MonetaryAmountElementValues((MonetaryAmountTypeQualifierList monetaryAmountType, ZString monetaryAmount, ZBool isMandatory) values)
			=> new(values.monetaryAmountType, values.monetaryAmount, values.isMandatory);

		public static implicit operator MonetaryAmountElementValues((MonetaryAmountTypeQualifierList monetaryAmountType, ZString monetaryAmount, ZString monetaryCurrency) values)
			=> new(values.monetaryAmountType, values.monetaryAmount, values.monetaryCurrency, false);
	}

	public static void AddNewMOASegment(MOASegmentMessageSection moaSection, MonetaryAmountTypeQualifierList monetaryFunction, MonetaryAmountElementValues[] monetaryAmountElementValues)
	{
		if (monetaryAmountElementValues is not { Length: > 0 })
		{
			return;
		}
		var values = monetaryAmountElementValues
			.Where(x => x.IsMandatory || x.MonetaryAmount != "0");
		foreach (var chunk in values.Chunk(5).Select(x => x.ToArray()))
		{
			var moaSegment = moaSection.InstantiateAChildAndAddItToChildrenCollection();
			moaSegment.MonetaryFunctionQualifier = monetaryFunction;
			TryAssignMonetaryAmount(moaSegment.MonetaryAmount1, chunk, index: 0);
			TryAssignMonetaryAmount(moaSegment.MonetaryAmount2, chunk, index: 1);
			TryAssignMonetaryAmount(moaSegment.MonetaryAmount3, chunk, index: 2);
			TryAssignMonetaryAmount(moaSegment.MonetaryAmount4, chunk, index: 3);
			TryAssignMonetaryAmount(moaSegment.MonetaryAmount5, chunk, index: 4);
		}
		static void TryAssignMonetaryAmount(MonetaryAmountElements monetaryAmountElements, MonetaryAmountElementValues[] monetaryAmountElementValues, int index)
		{
			if (monetaryAmountElementValues.Length > index)
			{
				var values = monetaryAmountElementValues[index];
				monetaryAmountElements.MonetaryAmountTypeQualifier = values.MonetaryAmountType;
				monetaryAmountElements.MonetaryAmount = values.MonetaryAmount;
				monetaryAmountElements.CurrencyCoded = values.MonetaryCurrency;
			}
		}
	}

	public static void AddNewTAXSegment(TAXSegmentMessageSection taxSection, ZString feeAmount, ZString feeType, ZString feeTypeSequence, ZString feeBaseValue, ZString feeRate, CodeListResponsibleAgencyCodedList responsibleAgency)
	{
		if (!feeAmount.IsEmpty)
		{
			var taxSegment = taxSection.InstantiateAChildAndAddItToChildrenCollection();
			taxSegment.DutyTaxFeeFunctionQualifier = DutyTaxFeeFunctionQualifierList.IndividualDutyTaxOrFeeCustomsItem;
			taxSegment.MonetaryAmount1.MonetaryAmountTypeQualifier = MonetaryAmountTypeQualifierList.DutyTaxOrFeeAmount;
			taxSegment.MonetaryAmount1.MonetaryAmount = feeAmount;
			taxSegment.DutyTaxFeeType.DutyTaxFeeTypeCoded = DutyTaxFeeTypeCodedList.GetFromString(feeType);
			taxSegment.DutyTaxFeeType.CodeListQualifier = CodeListQualifierList.ExciseDuty;
			taxSegment.DutyTaxFeeType.CodeListResponsibleAgencyCoded = responsibleAgency;
			taxSegment.DutyTaxFeeAccountDetail.DutyTaxFeeAccountIdentification = feeTypeSequence;
			taxSegment.DutyTaxFeeAccountDetail.CodeListQualifier = CodeListQualifierList.CustomsDeclarationType;
			taxSegment.DutyTaxFeeAccountDetail.CodeListResponsibleAgencyCoded = responsibleAgency;
			taxSegment.DutyTaxFeeAssessmentBasis = feeBaseValue;
			taxSegment.DutyTaxFeeDetail.DutyTaxFeeRateIdentification = feeRate;
		}
	}

	public static void AddNewTAXSegment(TAXSegmentMessageSection taxSection, ZString feeAmount)
	{
		if (!feeAmount.IsEmpty)
		{
			var taxSegment = taxSection.InstantiateAChildAndAddItToChildrenCollection();
			taxSegment.DutyTaxFeeFunctionQualifier = DutyTaxFeeFunctionQualifierList.TotalOfAllDutiesTaxesAndFeeTypesCustomsDeclaration;
			taxSegment.MonetaryAmount1.MonetaryAmountTypeQualifier = MonetaryAmountTypeQualifierList.DutyTaxOrFeeAmount;
			taxSegment.MonetaryAmount1.MonetaryAmount = feeAmount;
		}
	}

	public static void AddNewTAXSegment(TAXSegmentMessageSection taxSection, ZString feeAmount, ZString feeType, CodeListResponsibleAgencyCodedList responsibleAgency)
	{
		if (!feeAmount.IsEmpty && !feeType.IsEmpty)
		{
			var taxSegment = taxSection.InstantiateAChildAndAddItToChildrenCollection();
			taxSegment.DutyTaxFeeFunctionQualifier = DutyTaxFeeFunctionQualifierList.TotalOfEachDutyTaxOrFeeTypeCustomsDeclaration;
			taxSegment.MonetaryAmount1.MonetaryAmountTypeQualifier = MonetaryAmountTypeQualifierList.DutyTaxOrFeeAmount;
			taxSegment.MonetaryAmount1.MonetaryAmount = feeAmount;
			taxSegment.DutyTaxFeeType.DutyTaxFeeTypeCoded = DutyTaxFeeTypeCodedList.GetFromString(feeType);
			taxSegment.DutyTaxFeeType.CodeListQualifier = CodeListQualifierList.ExciseDuty;
			taxSegment.DutyTaxFeeType.CodeListResponsibleAgencyCoded = responsibleAgency;
		}
	}

	public static void AddNewDTMSegment(DTMSegmentMessageSection dtmSection, ZString dateTime, DateTimePeriodQualifierList dateTimeType, DateTimePeriodFormatQualifierList dateTimeFormat)
	{
		if (!dateTime.IsEmpty)
		{
			var dtmSegment = dtmSection.InstantiateAChildAndAddItToChildrenCollection();
			dtmSegment.DateTimePeriod.DateTimePeriodQualifier = dateTimeType;
			dtmSegment.DateTimePeriod.DateTimePeriod = dateTime;
			dtmSegment.DateTimePeriod.DateTimePeriodFormatQualifier = dateTimeFormat;
		}
	}

	public static void AddNewGISSegment(GISSegmentMessageSection gisSection, ZBool indicatorCode, CodeListQualifierList indicatorQualifier, CodeListResponsibleAgencyCodedList responsibleAgency)
	{
		if (indicatorQualifier == CodeListQualifierList.CustomsIndicator)
		{
			var gisSegment = gisSection.InstantiateAChildAndAddItToChildrenCollection();
			gisSegment.ProcessingIndicator.ProcessingIndicatorCoded = indicatorCode ? ProcessingIndicatorCodedList.MessageContentAccepted : ProcessingIndicatorCodedList.GetFromString("0");
			gisSegment.ProcessingIndicator.CodeListQualifier = indicatorQualifier;
			gisSegment.ProcessingIndicator.CodeListResponsibleAgencyCoded = responsibleAgency;
		}
		else if (indicatorCode)
		{
			var gisSegment = gisSection.InstantiateAChildAndAddItToChildrenCollection();
			gisSegment.ProcessingIndicator.ProcessingIndicatorCoded = ProcessingIndicatorCodedList.MessageContentAccepted;
			gisSegment.ProcessingIndicator.CodeListQualifier = indicatorQualifier;
			gisSegment.ProcessingIndicator.CodeListResponsibleAgencyCoded = responsibleAgency;
		}
	}

	public static void AddNewFTXSegment(FTXSegmentMessageSection ftxSection, TextSubjectQualifierList qualifier, ZString[] freeTexts)
	{
		var values = freeTexts?.Where(x => !x.IsEmpty).ToArray() ?? [];
		if (values.Length != 0 && AddNewFTXSegment(ftxSection, qualifier) is { } ftxSegment)
		{
			TryAssignIndexedValue(ref ftxSegment.TextLiteral.FreeText1, values, index: 0);
			TryAssignIndexedValue(ref ftxSegment.TextLiteral.FreeText2, values, index: 1);
			TryAssignIndexedValue(ref ftxSegment.TextLiteral.FreeText3, values, index: 2);
			TryAssignIndexedValue(ref ftxSegment.TextLiteral.FreeText4, values, index: 3);
			TryAssignIndexedValue(ref ftxSegment.TextLiteral.FreeText5, values, index: 4);
		}

		static void TryAssignIndexedValue(ref string destination, ZString[] values, int index)
		{
			if (values.Length > index)
			{
				destination = values[index];
			}
		}
	}

	public static void AddNewFTXSegment(FTXSegmentMessageSection ftxSection, TextSubjectQualifierList qualifier, ZString freeText, int fieldSplitLength)
	{
		if (!freeText.IsEmpty && AddNewFTXSegment(ftxSection, qualifier) is { } ftxSegment)
		{
			var splitter = new TextSplitter(fieldSplitLength) { Text = freeText };
			ftxSegment.TextLiteral.FreeText1 = splitter[0];
			ftxSegment.TextLiteral.FreeText2 = splitter[1];
			ftxSegment.TextLiteral.FreeText3 = splitter[2];
			ftxSegment.TextLiteral.FreeText4 = splitter[3];
			ftxSegment.TextLiteral.FreeText5 = splitter[4];
		}
	}

	static FTXSegment AddNewFTXSegment(FTXSegmentMessageSection ftxSection, TextSubjectQualifierList qualifier)
	{
		if (qualifier != null)
		{
			var ftxSegment = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
			ftxSegment.TextSubjectQualifier = qualifier;
			return ftxSegment;
		}
		return null;
	}

	public static void AddNewRFFSegment(RFFSegmentMessageSection rffSection, ReferenceQualifierList referenceType, ZString referenceNumber)
	{
		if (rffSection != null && referenceType != null && !referenceNumber.IsEmpty)
		{
			var rffSegment = rffSection.InstantiateAChildAndAddItToChildrenCollection();
			rffSegment.Reference.ReferenceQualifier = referenceType;
			rffSegment.Reference.ReferenceNumber = referenceNumber;
		}
	}

	public static void AddNewNADSegment(NADSegmentMessageSection nadSection, PartyQualifierList partyQualifier, ZString partyIdentification, CodeListResponsibleAgencyCodedList responsibleAgency, bool isRequired = false)
	{
		if (!isRequired && partyIdentification.IsEmpty)
		{
			return;
		}
		if (AddNewNADSegment(nadSection, partyQualifier) is { PartyIdentificationDetails: { } partyIdentificationDetails })
		{
			partyIdentificationDetails.PartyIdIdentification = partyIdentification;
			partyIdentificationDetails.CodeListResponsibleAgencyCoded = responsibleAgency;
		}
	}

	public static void AddNewNADSegment(NADSegmentMessageSection nadSection, PartyQualifierList partyQualifier, ZString addressLine1, ZString addressLine2, ZString addressLine3, ZString addressLine4)
	{
		if (AddNewNADSegment(nadSection, partyQualifier) is { NameAndAddress: { } nameAndAddress })
		{
			nameAndAddress.NameAndAddressLine1 = addressLine1;
			nameAndAddress.NameAndAddressLine2 = addressLine2;
			nameAndAddress.NameAndAddressLine3 = addressLine3;
			nameAndAddress.NameAndAddressLine4 = addressLine4;
		}
	}

	static NADSegment AddNewNADSegment(NADSegmentMessageSection nadSection, PartyQualifierList partyQualifier)
	{
		if (partyQualifier is not null)
		{
			var nadSegment = nadSection.InstantiateAChildAndAddItToChildrenCollection();
			nadSegment.PartyQualifier = partyQualifier;
			return nadSegment;
		}
		return null;
	}

	public static void AddNewDMSSegment(DMSSegmentMessageSection dmsSection, ZString documentMessageNumber, ZString documentMessageDate)
	{
		var dmsSegment = dmsSection.InstantiateAChildAndAddItToChildrenCollection();
		dmsSegment.DocumentMessageNumber = documentMessageNumber;
		if (!documentMessageDate.IsEmpty)
		{
			dmsSegment.DateTimePeriod.DateTimePeriodQualifier = DateTimePeriodQualifierList.DocumentMessageDateTime;
			dmsSegment.DateTimePeriod.DateTimePeriod = documentMessageDate;
			dmsSegment.DateTimePeriod.DateTimePeriodFormatQualifier = DateTimePeriodFormatQualifierList.Ccyymmdd;
		}
		dmsSegment.DocumentMessageNameCoded = DocumentMessageNameCodedList.CommercialInvoice;
	}

	public static void AddNewCSTSegment(CSTSegmentMessageSection cstSection, ZString goodsItemNumber, ZString tariffNumber, ZString procedureCode, ZString preferenceCode, ZString valuationMethod, ZString reducedCustomsFlag, CodeListResponsibleAgencyCodedList responsibleAgency)
	{
		var cstSegment = cstSection.InstantiateAChildAndAddItToChildrenCollection();
		cstSegment.GoodsItemNumber = goodsItemNumber;
		cstSegment.CustomsIdentityCodes1.CustomsCodeIdentification = tariffNumber;
		cstSegment.CustomsIdentityCodes1.CodeListQualifier = CodeListQualifierList.Commodity;
		cstSegment.CustomsIdentityCodes1.CodeListResponsibleAgencyCoded = responsibleAgency;
		cstSegment.CustomsIdentityCodes2.CustomsCodeIdentification = procedureCode;
		cstSegment.CustomsIdentityCodes2.CodeListQualifier = CodeListQualifierList.CustomsProcedure;
		cstSegment.CustomsIdentityCodes2.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.CccCustomsCoOperationCouncil;
		cstSegment.CustomsIdentityCodes3.CustomsCodeIdentification = preferenceCode;
		cstSegment.CustomsIdentityCodes3.CodeListQualifier = CodeListQualifierList.CustomsPreference;
		cstSegment.CustomsIdentityCodes3.CodeListResponsibleAgencyCoded = responsibleAgency;
		cstSegment.CustomsIdentityCodes4.CustomsCodeIdentification = valuationMethod;
		cstSegment.CustomsIdentityCodes4.CodeListQualifier = CodeListQualifierList.CustomsIndicator;
		cstSegment.CustomsIdentityCodes4.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.CccCustomsCoOperationCouncil;
		if (!reducedCustomsFlag.IsEmpty)
		{
			cstSegment.CustomsIdentityCodes5.CustomsCodeIdentification = reducedCustomsFlag;
			cstSegment.CustomsIdentityCodes5.CodeListQualifier = CodeListQualifierList.CustomsSpecialCodes;
			cstSegment.CustomsIdentityCodes5.CodeListResponsibleAgencyCoded = responsibleAgency;
		}
	}

	public static void AddNewPACSegment(PACSegmentMessageSection pacSection, ZString numberOfPackages, ZString packageType)
	{
		var pacSegment = pacSection.InstantiateAChildAndAddItToChildrenCollection();
		pacSegment.NumberOfPackages = numberOfPackages;
		pacSegment.PackageType.TypeOfPackagesIdentification = packageType;
	}

	public static void AddNewPCISegment(PCISegmentMessageSection pciSection, ZString generalGoodsMarks)
	{
		var pciSegment = pciSection.InstantiateAChildAndAddItToChildrenCollection();
		pciSegment.MarksLabels.ShippingMarks1 = generalGoodsMarks;
	}

	public static void AddNewPCISegment(PCISegmentMessageSection pciSection, ReferenceQualifierList referenceQualifier, ZString referenceNumber)
	{
		if (!referenceNumber.IsEmpty)
		{
			var pciSegment = pciSection.InstantiateAChildAndAddItToChildrenCollection();
			pciSegment.Reference.ReferenceQualifier = referenceQualifier;
			pciSegment.Reference.ReferenceNumber = referenceNumber;
		}
	}

	public static void AddnewDCRSegment(DCRSegmentMessageSection dcrSection, ZString documentType, ZString documentValue)
	{
		if (!documentType.IsEmpty && !documentValue.IsEmpty)
		{
			var dcrSegment = dcrSection.InstantiateAChildAndAddItToChildrenCollection();
			dcrSegment.DocumentMessageNameCoded = DocumentMessageNameCodedList.ExportLicence;
			dcrSegment.DocumentElements.ReferenceQualifier = CodeListQualifierList.GetFromString(documentType);

			var numOfChars = documentType.ToUpper() == "TXT" ? 35 : 17;
			dcrSegment.DocumentElements.ReferenceNumber = CodeListResponsibleAgencyCodedList.GetFromString(documentValue.SubstringSafe(0, numOfChars));
		}
	}

	public static void AddnewGDSSegment(GDSSegmentMessageSection gdsSection)
	{
		var gdsSegment = gdsSection.InstantiateAChildAndAddItToChildrenCollection();
		gdsSegment.NatureOfCargo.NatureOfCargoCoded = "2";
	}

	public static void PopulateUNS1Segment(UNSSegment unsSegment)
	{
		unsSegment.SectionIdentification = SectionIdentificationList.HeaderDetailSectionSeparation;
	}

	public static void PopulateUNS2Segment(UNSSegment unsSegment)
	{
		unsSegment.SectionIdentification = SectionIdentificationList.DetailSummarySectionSeparation;
	}

	public static void AddNewCNTSegment(CNTSegmentMessageSection cntSection, ZString noOfLines, ZString noOfPackages)
	{
		if (!noOfLines.IsEmpty)
		{
			var cntSegment = cntSection.InstantiateAChildAndAddItToChildrenCollection();
			cntSegment.ControlNoLines.ControlQualifier = ControlQualifierList.NumberOfCustomsItemDetailLines;
			cntSegment.ControlNoLines.ControlValue = noOfLines;
			cntSegment.ControlNoPackages.ControlQualifier = ControlQualifierList.TotalNumberOfPackages;
			cntSegment.ControlNoPackages.ControlValue = noOfPackages;
		}
	}
}
