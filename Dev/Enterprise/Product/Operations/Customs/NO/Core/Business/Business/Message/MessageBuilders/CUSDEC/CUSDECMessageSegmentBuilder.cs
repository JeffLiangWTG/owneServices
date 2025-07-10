using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Edifact.V902.Elements;
using Enterprise.Edifact.V902.Messages.CUSDEC;
using Enterprise.Edifact.V902.Segments;

namespace Enterprise.Customs.NO.Business;

static class CUSDECMessageSegmentBuilder
{
	public static void PopulateCUSDECMessage(CUSDECMessage message, ICUSDECMessageDataProvider source)
	{
		PopulateUNHSegment(message.UNH[0], source);
		PopulateCUSDECMessageHeader(message, source);
		EDIFACTMessageSegmentBuilder.PopulateUNS1Segment(message.UNS1[0]);
		PopulateCUSDECMessageDetails(message, source);
		EDIFACTMessageSegmentBuilder.PopulateUNS2Segment(message.UNS2[0]);
		PopulateTAXSegment(message, source);
		PopulateCNTSegment(message, source);
		PopulateUNTSegment(message.UNT[0], message.CountIncludingUNT, source);
	}

	static void PopulateCUSDECMessageHeader(CUSDECMessage message, ICUSDECMessageDataProvider source)
	{
		PopulateBGMSegment(message.BGM[0], source);
		PopulateCSTSegment(message.CST[0], source);
		PopulateSG1Group(message.Group1, source);
		PopulateLOCSegments(message.LOC, source);
		PopulateTDTSegment(message.TDT[0], source);
		PopulateGISSegment(message.GIS[0], source);
		PopulateSG4Group(message.Group4, source);
		PopulateSG5Group(message.Group5, source);
		PopulateSG6Group(message.Group6, source);
	}

	static void PopulateCUSDECMessageDetails(CUSDECMessage message, ICUSDECMessageDataProvider source)
	{
		foreach (var summary in source.DocumentMessageSummaryCollection)
		{
			PopulateSG7Group(message.Group7, summary);
			PopulateSG23Group(message.Group23, summary);
		}
	}

	static readonly ImmutableDictionary<string, string> msgCat = ImmutableDictionary.CreateRange(new Dictionary<string, string>
	{
		{ MessageSendingMessageTypes.Codes.CompleteOrdinaryDeclaration, "9" },
		{ MessageSendingMessageTypes.Codes.ManualDeclaration, "ZZ" },
		{ MessageSendingMessageTypes.Codes.PreliminaryDeclaration, "14" },
		{ MessageSendingMessageTypes.Codes.FinalDeclaration, "2" },
		{ MessageSendingMessageTypes.Codes.Correction, "4" },
		{ MessageSendingMessageTypes.Codes.PostDeclaration, "18" },
		{ MessageSendingMessageTypes.Codes.RefundDeclaration, "19" },
		{ MessageSendingMessageTypes.Codes.StatisticalRecalculatedDeclaration, "43" },
		{ MessageSendingMessageTypes.Codes.CollectiveCustomClearance, "42" }
	});

	#region Fields Population

	static void PopulateUNHSegment(UNHSegment unhSegment, ICUSDECMessageDataProvider source)
	{
		unhSegment.MessageReferenceNumber = source.DeclarationReferenceNumber;
		unhSegment.MessageIdentifier.MessageType = MessageTypeList.CustomsDeclarationMessage;
		unhSegment.MessageIdentifier.MessageVersionNumber = MessageVersionNumberList.Status1Version;
		unhSegment.MessageIdentifier.MessageReleaseNumber = MessageReleaseNumberList.TrialRelease1990;
		unhSegment.MessageIdentifier.ControllingAgency = ControllingAgencyList.UnCefact;
		unhSegment.MessageIdentifier.AssociationAssignedCode = source.AssociationAssignedCode;
		unhSegment.CommonAccessReference = source.LocalReferenceNumber;
	}

	static void PopulateBGMSegment(BGMSegment bgmSegment, ICUSDECMessageDataProvider source)
	{
		bgmSegment.DocumentMessageName.DocumentMessageNameCoded = source.DocumentMessageName;
		bgmSegment.DateTimePeriod1.DateTimePeriodQualifier = DateTimePeriodQualifierList.DocumentMessageDateTime;
		bgmSegment.DateTimePeriod1.DateTimePeriod = ZDateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
		bgmSegment.DateTimePeriod1.DateTimePeriodFormatQualifier = DateTimePeriodFormatQualifierList.Ccyymmdd;

		if (msgCat.TryGetValue(source.MessageType, out var messageFunctionCode))
		{
			bgmSegment.MessageFunctionCoded = MessageFunctionCodedList.GetFromString(messageFunctionCode);
		}
	}

	static void PopulateCSTSegment(CSTSegment cstSegment, ICUSDECMessageDataProvider source)
	{
		cstSegment.CustomsIdentityCodes1.CustomsCodeIdentification = source.DeclarationType;
		cstSegment.CustomsIdentityCodes1.CodeListQualifier = CodeListQualifierList.CustomsAreaOfTransaction;
		cstSegment.CustomsIdentityCodes1.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.CccCustomsCoOperationCouncil;

		cstSegment.CustomsIdentityCodes2.CustomsCodeIdentification = source.TransactionNature;
		cstSegment.CustomsIdentityCodes2.CodeListQualifier = CodeListQualifierList.CustomsNatureOfTheTransaction;
		cstSegment.CustomsIdentityCodes2.CodeListResponsibleAgencyCoded = ResponsibleAgency.NorwegianCustoms;
	}

	static void PopulateSG1Group(SegmentGroup1MessageSection sg1Section, ICUSDECMessageDataProvider source)
	{
		AddNewRFFInSG1Group(sg1Section, ReferenceQualifierList.BuyersOrderNumber, source.ControlNumber);
		AddNewRFFInSG1Group(sg1Section, ReferenceQualifierList.CargoControlNumber, source.GoodsNumber);
		AddNewRFFInSG1Group(sg1Section, ReferenceQualifierList.LowerArticleNumberOfRange, source.GoodsNumberPosition);
		AddNewRFFInSG1Group(sg1Section, ReferenceQualifierList.RegistrationNumberOfPreviousCustomsDeclaration, source.RelatedDeclaration);
	}

	static void AddNewRFFInSG1Group(SegmentGroup1MessageSection sg1Section, ReferenceQualifierList referenceQualifier, ZString referenceNumber)
	{
		if (!referenceNumber.IsEmpty)
		{
			var sg1Segment = sg1Section.InstantiateAChildAndAddItToChildrenCollection();
			EDIFACTMessageSegmentBuilder.AddNewRFFSegment(sg1Segment.RFF, referenceQualifier, referenceNumber);
		}
	}

	static void PopulateLOCSegments(LOCSegmentMessageSection locSection, ICUSDECMessageDataProvider source)
	{
		EDIFACTMessageSegmentBuilder.AddNewLOCSegment(locSection, PlaceLocationQualifierList.CountryOfUltimateDestination, source.GoodsDestination);
		EDIFACTMessageSegmentBuilder.AddNewLOCSegment(locSection, PlaceLocationQualifierList.CountryOfExportationDespatch, source.GoodsOrigin);
		EDIFACTMessageSegmentBuilder.AddNewLOCSegment(locSection, PlaceLocationQualifierList.CustomsOfficeOfExit, source.CustomsOfficeOfExit, ResponsibleAgency.NorwegianCustoms);
		EDIFACTMessageSegmentBuilder.AddNewLOCSegment(locSection, PlaceLocationQualifierList.LocationOfGoods, source.LocationOfGoods, ResponsibleAgency.NorwegianCustoms);
	}

	static void PopulateTDTSegment(TDTSegment tdtSegment, ICUSDECMessageDataProvider source)
	{
		tdtSegment.TransportStageQualifier = TransportStageQualifierList.AtBorder;
		tdtSegment.ModeOfTransport.ModeOfTransportCoded = source.TransportMode;
		tdtSegment.TransportIdentification.NationalityOfMeansOfTransportCoded = source.TransportNationality;
	}

	static void PopulateGISSegment(GISSegment gisSegment, ICUSDECMessageDataProvider source)
	{
		var containerCode = (string)source.ContainerMode switch
		{
			Core.Constants.ContainerModes.Containerised or
				Core.Constants.ContainerModes.FCL or
				Core.Constants.ContainerModes.LCL or
				Core.Constants.ContainerModes.ULD => ProcessingIndicatorCodedList.GetFromString("1"),
			_ => ProcessingIndicatorCodedList.GetFromString("0"),
		};
		gisSegment.ProcessingIndicator.ProcessingIndicatorCoded = containerCode;
		gisSegment.ProcessingIndicator.CodeListResponsibleAgencyCoded = ResponsibleAgency.NorwegianCustoms;
	}

	static void PopulateSG4Group(SegmentGroup4MessageSection sg4Section, ICUSDECMessageDataProvider source)
	{
		var sg4SegmentExporter = sg4Section.InstantiateAChildAndAddItToChildrenCollection();
		EDIFACTMessageSegmentBuilder.AddNewNADSegment(sg4SegmentExporter.NAD, PartyQualifierList.Exporter, source.ExporterCustomsRegNo, ResponsibleAgency.NorwegianCustoms);

		var sg4SegmentImporter = sg4Section.InstantiateAChildAndAddItToChildrenCollection();
		EDIFACTMessageSegmentBuilder.AddNewNADSegment(sg4SegmentImporter.NAD, PartyQualifierList.Importer, source.ImporterCustomsRegNo, ResponsibleAgency.NorwegianCustoms);

		var sg4SegmentSeller = sg4Section.InstantiateAChildAndAddItToChildrenCollection();
		EDIFACTMessageSegmentBuilder.AddNewNADSegment(sg4SegmentSeller.NAD, source.SenderAddressType, source.SenderFullName, source.SenderAddress1, source.SenderAddress2, source.SenderAddress3);

		var sg4SegmentDeclarant = sg4Section.InstantiateAChildAndAddItToChildrenCollection();
		EDIFACTMessageSegmentBuilder.AddNewNADSegment(sg4SegmentDeclarant.NAD, PartyQualifierList.Declarant, source.DeclarantCustomsRegNo, ResponsibleAgency.NorwegianCustoms);
		sg4SegmentDeclarant.CTA[0].DepartmentOrEmployeeDetails.DepartmentOrEmployeeIdentification = source.InitialsOfDeclarant;

		var sg4SegmentDeclarantPaymentMethod = sg4Section.InstantiateAChildAndAddItToChildrenCollection();
		var paymentMethod = (string)source.PaymentMethod switch
		{
			NOPaymentMethodCodeList.Codes.NoDutiesOrVatPayable => " ",
			var x => x,
		};
		EDIFACTMessageSegmentBuilder.AddNewNADSegment(sg4SegmentDeclarantPaymentMethod.NAD, PartyQualifierList.PartyToBeBilledAarAccountingRule11, paymentMethod, ResponsibleAgency.NorwegianCustoms, isRequired: true);

		var sg4SegmentDeclarantControllingUnit = sg4Section.InstantiateAChildAndAddItToChildrenCollection();
		EDIFACTMessageSegmentBuilder.AddNewNADSegment(sg4SegmentDeclarantControllingUnit.NAD, PartyQualifierList.LocationOfGoodsForCustomsExaminationBeforeClearance, source.CustomsControllingUnit, ResponsibleAgency.NorwegianCustoms);
	}

	static void PopulateSG5Group(SegmentGroup5MessageSection sg5Section, ICUSDECMessageDataProvider source)
	{
		var sg5Segment = sg5Section.InstantiateAChildAndAddItToChildrenCollection();
		PopulateTODSegmentInSG5Group(sg5Segment.TOD[0], source);
		EDIFACTMessageSegmentBuilder.AddNewFTXSegment(sg5Segment.FTX, TextSubjectQualifier.ReExportReason, source.ReExportReason, fieldSplitLength: 70);
	}

	static void PopulateTODSegmentInSG5Group(TODSegment todSegment, ICUSDECMessageDataProvider source)
	{
		todSegment.TermsOfDelivery.TermsOfDeliveryCoded = source.IncoTerm;
		todSegment.LocationIdentification1.PlaceLocationQualifier = "1";
		todSegment.LocationIdentification1.PlaceLocation = source.IncoTermPlace;
	}

	static void PopulateSG6Group(SegmentGroup6MessageSection sg6Section, ICUSDECMessageDataProvider source)
	{
		var sg6Segment = sg6Section.InstantiateAChildAndAddItToChildrenCollection();
		EDIFACTMessageSegmentBuilder.AddNewMOASegment(sg6Segment.MOA, MonetaryAmountTypeQualifierList.AdditionalRoyaltiesCustoms, [
			(MonetaryAmountTypeQualifierList.InvoiceTotalAmount, source.CommercialInvoiceAmount, source.CommercialInvoiceCurrency),
			(MonetaryAmountTypeQualifierList.TransportChargesCustoms, source.FreightAmountNOK),
		]);
		PopulateCUXSegmentInSG6Group(sg6Segment.CUX[0], source);
	}

	static void PopulateCUXSegmentInSG6Group(CUXSegment cuxSegment, ICUSDECMessageDataProvider source)
	{
		cuxSegment.CurrencyDetails1.CurrencyDetailsQualifier = CurrencyDetailsQualifierList.ReferenceCurrency;
		cuxSegment.RateOfExchange = source.CurrencyExchangeRate;
	}

	public static void PopulateUNTSegment(UNTSegment untSegment, int segmentCount, ICUSDECMessageDataProvider source)
	{
		untSegment.NumberOfSegmentsInTheMessage = segmentCount.ToString(CultureInfo.CurrentCulture);
		untSegment.MessageReferenceNumber = source.DeclarationReferenceNumber;
	}

	static void PopulateSG7Group(SegmentGroup7MessageSection sg7Section, ICUSDECMessageDataProvider.IDocumentMessageSummary summary)
	{
		var sg7Segment = sg7Section.InstantiateAChildAndAddItToChildrenCollection();
		EDIFACTMessageSegmentBuilder.AddNewDMSSegment(sg7Segment.DMS, summary.InvoiceNumber, summary.InvoiceDate);
	}

	static void PopulateSG23Group(SegmentGroup23MessageSection sg23Section, ICUSDECMessageDataProvider.IDocumentMessageSummary summary)
	{
		foreach (var itemDetails in summary.ItemDetailsCollection)
		{
			var sg23Segment = sg23Section.InstantiateAChildAndAddItToChildrenCollection();
			PopulateSG23Group(sg23Segment, itemDetails);
		}
	}

	static void PopulateSG23Group(SegmentGroup23 sg23Segment, ICUSDECMessageDataProvider.IItemDetails itemDetails)
	{
		EDIFACTMessageSegmentBuilder.AddNewCSTSegment(sg23Segment.CST,
			itemDetails.DeclarationLineNumber,
			itemDetails.TariffNumber,
			itemDetails.ProcedureCode,
			itemDetails.PreferenceCode,
			itemDetails.ValuationMethod,
			itemDetails.ReducedCustomsFlag,
			ResponsibleAgency.NorwegianCustoms);
		PopulateSG24Group(sg23Segment.Group24[0], itemDetails);
		EDIFACTMessageSegmentBuilder.AddNewLOCSegment(sg23Segment.LOC, PlaceLocationQualifierList.CountryOfOrigin, itemDetails.CountryOfOrigin);
		EDIFACTMessageSegmentBuilder.AddNewLOCSegment(sg23Segment.LOC, PlaceLocationQualifierList.RegionOfOrigin, itemDetails.RegionOfOrigin);
		PopulateSG26Group(sg23Segment.Group26[0], itemDetails);
		EDIFACTMessageSegmentBuilder.AddNewMEASegment(sg23Segment.MEA, MeasurementDimensionCodedList.GrossWeight, NOCustomsFormulaUnitCodeList.Codes.KGM, itemDetails.GrossWeight);
		EDIFACTMessageSegmentBuilder.AddNewMEASegment(sg23Segment.MEA, MeasurementDimensionCodedList.ActualNetWeight, NOCustomsFormulaUnitCodeList.Codes.KGM, itemDetails.NetWeight);
		EDIFACTMessageSegmentBuilder.AddNewMEASegment(sg23Segment.MEA, MeasurementDimensionCodedList.GetFromString("ZZ"), itemDetails.CustomsQtyUnitOfMeasurement, itemDetails.CustomsQtyAmount);
		foreach (var fee in itemDetails.ItemDetailsFeeCollection)
		{
			EDIFACTMessageSegmentBuilder.AddNewTAXSegment(sg23Segment.TAX, fee.FeeAmount, fee.FeeType, fee.FeeTypeSequence, fee.FeeBaseValueForCalculation, fee.FeeRate, ResponsibleAgency.NorwegianCustoms);
		}
		PopulateSG29Group(sg23Segment.Group29[0], itemDetails);
		PopulateSG31Group(sg23Segment.Group31[0], itemDetails);
	}

	static void PopulateSG24Group(SegmentGroup24 sg24Segment, ICUSDECMessageDataProvider.IItemDetails itemDetails)
	{
		EDIFACTMessageSegmentBuilder.AddNewPACSegment(sg24Segment.PAC, itemDetails.NumberOfPackages, itemDetails.PackageType);
		PopulateSG25Group(sg24Segment.Group25[0], itemDetails);
	}

	static void PopulateSG25Group(SegmentGroup25 sg25Segment, ICUSDECMessageDataProvider.IItemDetails itemDetails)
	{
		EDIFACTMessageSegmentBuilder.AddNewPCISegment(sg25Segment.PCI, itemDetails.GeneralGoodsMarks);
		EDIFACTMessageSegmentBuilder.AddNewPCISegment(sg25Segment.PCI, ReferenceQualifierList.MotorVehicleIdentificationNumber, itemDetails.ChassisNumber);
		foreach (var containerNumber in itemDetails.ContainerNumbers)
		{
			EDIFACTMessageSegmentBuilder.AddNewPCISegment(sg25Segment.PCI, ReferenceQualifierList.UnitLoadDeviceEGContainerIdentificationNumber, containerNumber);
		}
	}

	static void PopulateSG26Group(SegmentGroup26 sg26Segment, ICUSDECMessageDataProvider.IItemDetails itemDetails)
	{
		var monetaryAmountTypeForAdjustedValue = itemDetails.AdjustedValueIsDiscount
			? MonetaryAmountTypeQualifierList.CashDiscount
			: MonetaryAmountTypeQualifierList.OtherCosts;
		EDIFACTMessageSegmentBuilder.AddNewMOASegment(sg26Segment.MOA, MonetaryAmountTypeQualifierList.AmountTargetCurrency, [
			(MonetaryAmountTypeQualifierList.StatisticalValue, itemDetails.StatisticValue, isMandatory: true),
			(monetaryAmountTypeForAdjustedValue, itemDetails.AdjustedValue),
			(MonetaryAmountTypeQualifierList.CustomsValue, itemDetails.AddedValueDueToProcessingAbroad),
		]);
	}

	static void PopulateSG29Group(SegmentGroup29 sg29Segment, ICUSDECMessageDataProvider.IItemDetails itemDetails)
	{
		foreach (var documents in itemDetails.ItemDetailsSupportingDocumentsCollection)
		{
			EDIFACTMessageSegmentBuilder.AddnewDCRSegment(sg29Segment.DCR, documents.DocumentCode, documents.DocumentNumberOrText);
		}
	}

	static void PopulateSG31Group(SegmentGroup31 sg31Segment, ICUSDECMessageDataProvider.IItemDetails itemDetails)
	{
		EDIFACTMessageSegmentBuilder.AddnewGDSSegment(sg31Segment.GDS);
		EDIFACTMessageSegmentBuilder.AddNewFTXSegment(sg31Segment.FTX, TextSubjectQualifierList.GoodsDescription, itemDetails.GoodsDescriptions);
	}

	static void PopulateTAXSegment(CUSDECMessage message, ICUSDECMessageDataProvider source)
	{
		EDIFACTMessageSegmentBuilder.AddNewTAXSegment(message.TAX, source.TotalFeeAmount);

		foreach (var fee in source.TotalFeeCollection)
		{
			EDIFACTMessageSegmentBuilder.AddNewTAXSegment(message.TAX, fee.Amount.ToNorwegianAmountString(), fee.FeeCode, ResponsibleAgency.NorwegianCustoms);
		}
	}

	static void PopulateCNTSegment(CUSDECMessage message, ICUSDECMessageDataProvider source)
	{
		EDIFACTMessageSegmentBuilder.AddNewCNTSegment(message.CNT, source.TotalNoOfItemLines, source.TotalNoOfPackages);
	}
	#endregion

	#region Constants

	static class ResponsibleAgency
	{
		const string NO1 = "NO1";

		public static CodeListResponsibleAgencyCodedList NorwegianCustoms => CodeListResponsibleAgencyCodedList.GetFromString(NO1);
	}

	static class TextSubjectQualifier
	{
		public static TextSubjectQualifierList ReExportReason => TextSubjectQualifierList.GetFromString("AHZ");
	}

	#endregion
}
