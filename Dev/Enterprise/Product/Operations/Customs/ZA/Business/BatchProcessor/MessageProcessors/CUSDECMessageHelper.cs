using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Customs.ZA.Business.DocumentWrappers;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Edifact.D96B.Elements;
using Enterprise.Edifact.D96B.Messages.CUSDEC;
using Enterprise.Edifact.D96B.Segments;
using Constants = Enterprise.Customs.ZA.Business.MessageBuilders.Constants;
using D96BMessageFactory = Enterprise.Edifact.D96B.EdifactD96BMessageFactory;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	public class CUSDECMessageHelper : NonPersistentBusinessObject, ICUSDECMessageDataProvider
	{
		public CUSDECMessageHelper(CUSDECMessage message)
		{
			cusdecMessage = Argument.NotNull(message, "message");
			interchangeTime = ZDateTime.Invalid;
		}

		CUSDECMessageHelper(CUSDECMessage message, BusinessObjectFactory factory) : base(factory)
		{
			cusdecMessage = Argument.NotNull(message, "message");
			interchangeTime = ZDateTime.Invalid;
		}

		readonly CUSDECMessage cusdecMessage;

		public static CUSDECMessageHelper New(ZAMessage message)
		{
			CUSDECMessageHelper result = null;
			if (message != null)
			{
				var d96bMessageFactory = new D96BMessageFactory();
				var zaCharSet = new ZACharacterSet();
				CUSDECMessage cusdecMessage = message.GetAutoEdifactMessageUsingNamedFactory(d96bMessageFactory, zaCharSet) as CUSDECMessage;
				if (cusdecMessage != null)
				{
					result = new CUSDECMessageHelper(cusdecMessage, message.Factory);
					result.interchangeTime = message.EM_DateTimeInterchangeSent;
				}
			}
			return result;
		}

		#region Caching Property

		BGMSegment BGMSegment => bgmSemgent ?? (bgmSemgent = cusdecMessage.BGM[0]);
		BGMSegment bgmSemgent;

		IEnumerable<SegmentGroup1> SG1Enumerable => sg1Enumerable ?? (sg1Enumerable = cusdecMessage.Group1.Cast<SegmentGroup1>());
		IEnumerable<SegmentGroup1> sg1Enumerable;

		IEnumerable<LOCSegment> LOCSegmentEnumerable => locSegmentEnumerable ?? (locSegmentEnumerable = cusdecMessage.LOC.Cast<LOCSegment>());
		IEnumerable<LOCSegment> locSegmentEnumerable;

		IEnumerable<DTMSegment> DTMSegmentEnumerable => dtmSegmentEnumerable ?? (dtmSegmentEnumerable = cusdecMessage.DTM.Cast<DTMSegment>());
		IEnumerable<DTMSegment> dtmSegmentEnumerable;

		TDTSegment SG4TDTSegment => sg4TDTSemgent ?? (sg4TDTSemgent = cusdecMessage.Group4[0].TDT[0]);
		TDTSegment sg4TDTSemgent;

		#region Caching for SG6 NAD

		IEnumerable<SegmentGroup6> SG6Enumerable => sg6Enumerable ?? (sg6Enumerable = cusdecMessage.Group6.Cast<SegmentGroup6>());
		IEnumerable<SegmentGroup6> sg6Enumerable;

		IEnumerable<NADSegment> SG6NADSegmentEnumerable => sg6NADSegmentEnumerable ?? (sg6NADSegmentEnumerable = cusdecMessage.Group6.Cast<SegmentGroup6>().Select(sg6 => sg6.NAD[0]));
		IEnumerable<NADSegment> sg6NADSegmentEnumerable;

		SegmentGroup6 SupplierSG6 => supplierSG6 ?? (supplierSG6 = SG6Enumerable?.FirstOrDefault(sg6 => sg6.NAD[0].PartyQualifier == PartyQualifierList.Supplier));
		SegmentGroup6 supplierSG6;

		SegmentGroup6 ExporterSG6 => exporterSG6 ?? (exporterSG6 = SG6Enumerable?.FirstOrDefault(sg6 => sg6.NAD[0].PartyQualifier == PartyQualifierList.Exporter));
		SegmentGroup6 exporterSG6;

		SegmentGroup6 ImporterSG6 => importerSG6 ?? (importerSG6 = SG6Enumerable?.FirstOrDefault(sg6 => sg6.NAD[0].PartyQualifier == PartyQualifierList.Importer));
		SegmentGroup6 importerSG6;

		SegmentGroup6 ImporterForExportSG6 => importerForExportSG6 ?? (importerForExportSG6 = SG6Enumerable?.FirstOrDefault(sg6 => sg6.NAD[0].PartyQualifier == PartyQualifierList.Consignee));
		SegmentGroup6 importerForExportSG6;

		SegmentGroup6 UnregisteredTraderSG6 => unregisteredTraderSG6 ?? (unregisteredTraderSG6 = SG6Enumerable?.FirstOrDefault(sg6 => sg6.NAD[0].PartyQualifier == PartyQualifierList.Declarant));
		SegmentGroup6 unregisteredTraderSG6;

		#endregion

		IEnumerable<SegmentGroup49> SG49Enumerable => sg49enumerable ?? (sg49enumerable = cusdecMessage.Group49.Cast<SegmentGroup49>());
		IEnumerable<SegmentGroup49> sg49enumerable;

		#endregion

		#region Implementation

		public ZDateTime InterchangeTime => interchangeTime;
		ZDateTime interchangeTime;

		#region GIS Info

		ZString CustomsValuationMethod => customsValuationMethod ?? (customsValuationMethod = cusdecMessage.GIS.Cast<GISSegment>().FirstOrDefault(gis => gis.ProcessingIndicator.CodeListQualifier == CodeListQualifierList.CustomsValuationMethod)?.ProcessingIndicator.ProcessingIndicatorCoded.ToString() ?? ZString.Empty);
		string customsValuationMethod;

		public ZString RelatedPartyIndicator => CustomsValuationMethod.SubstringSafe(0, 1);

		public ZString RelatedPartyIndicatorForSADDocumentPackPurposes => RelatedPartyIndicator;

		public ZString ValuationCode => CustomsValuationMethod.SubstringSafe(1, 1);

		public ZString ValuationCodeForSADDocumentPackPurposes => ValuationCode;

		public ZString PaymentMethod => cusdecMessage.GIS.Cast<GISSegment>().FirstOrDefault(gis => gis.ProcessingIndicator.CodeListQualifier == CodeListQualifierList.DutyTaxOrFeePaymentMethod)?.ProcessingIndicator.ProcessingIndicatorCoded.ToString() ?? ZString.Empty;

		public ZString RefundAcknowledgementIndicator => cusdecMessage.GIS.Cast<GISSegment>().FirstOrDefault(gis => gis.ProcessingIndicator.CodeListQualifier == CodeListQualifierList.CustomsIndicator)?.ProcessingIndicator.ProcessingIndicatorCoded.ToString() ?? ZString.Empty;

		#endregion

		#region BGM Info

		public ZString ShipmentType => shipmentType ?? (shipmentType = BGMSegment.DocumentMessageName.DocumentMessageNameCoded);
		string shipmentType;

		public ZString LocalReferenceNumber => BGMSegment.DocumentMessageIdentification.DocumentMessageNumber;

		public ZString MessageType => messageFunctionCode ?? (messageFunctionCode = BGMSegment.MessageFunctionCoded.ToString());
		string messageFunctionCode;

		public ZString DeclarationType => declarationType ?? (declarationType = BGMSegment.DocumentMessageName.DocumentMessageName);
		string declarationType;

		public ZString MessageFunctionDescription => Factory.GetCachedValue<MessageFunctionCodeList>().GetDescriptionFromCode(MessageType);

		public ZInt PartClearanceQuantity
		{
			get
			{
				ZInt result;
				var checkresult = ZInt.TryParse(BGMSegment.DocumentMessageIdentification.RevisionNumber, out result);
				if (!checkresult)
				{
					result = ZInt.Zero;
				}
				return result;
			}
		}

		#endregion

		public ZDecimal GrossWeightInKG
		{
			get
			{
				var valueString = cusdecMessage.MEA.Cast<MEASegment>().FirstOrDefault(mea => mea.MeasurementApplicationQualifier == MeasurementApplicationQualifierList.Measurement && mea.MeasurementDetails.MeasurementDimensionCoded == MeasurementDimensionCodedList.TotalGrossWeight)?.ValueRange.MeasurementValue;
				var result = ZDecimal.ParseSafe(valueString ?? ZString.Empty, ZDecimal.Zero);
				return result;
			}
		}

		public ZString TotalWeightInKG => cusdecMessage.MEA.Cast<MEASegment>().FirstOrDefault(mea => mea.MeasurementApplicationQualifier == MeasurementApplicationQualifierList.Measurement)?.ValueRange?.MeasurementValue ?? ZString.Empty;

		#region SG1 - Bill Number Info

		ZString GettingReferenceNumberFromSG1(ReferenceQualifierList rffType) => SG1Enumerable.FirstOrDefault(sg1 => sg1.RFF[0].Reference.ReferenceQualifier == rffType)?.RFF[0]?.Reference?.ReferenceNumber ?? ZString.Empty;

		public ZString MessageNumber => GettingReferenceNumberFromSG1(ReferenceQualifierList.AdditionalReferenceNumber);

		public ZString CaseNumber => GettingReferenceNumberFromSG1(ReferenceQualifierList.EnquiryNumber);

		public ZString TransportDocumentNumber => GettingReferenceNumberFromSG1(ReferenceQualifierList.TransportDocumentNumber);

		public ZString TransportDocumentNumberInBusinessLogic => this.GetTransportDocumentNumberInBusinessLogic();

		public ZDateTime TransportDocumentDate
		{
			get
			{
				var result = ZDateTime.Invalid;
				var dateString = SG1Enumerable.FirstOrDefault(sg1 => sg1.RFF[0].Reference.ReferenceQualifier == ReferenceQualifierList.TransportDocumentNumber)?.DTM[0]?.DateTimePeriod?.DateTimePeriod;
				ZDateTime.TryParseExact(dateString, out result, Constants.DateFormatCCYYMMDD);
				return result;
			}
		}

		public ZString HouseBill => GettingReferenceNumberFromSG1(ReferenceQualifierList.HouseBillOfLadingNumber);

		public ZString UniqueConsignmentReference => GettingReferenceNumberFromSG1(ReferenceQualifierList.UniqueConsignmentReferenceNumber);

		public ZDateTime HouseBillIssuedDate
		{
			get
			{
				var result = ZDateTime.Invalid;
				var dateString = SG1Enumerable.FirstOrDefault(sg1 => sg1.RFF[0].Reference.ReferenceQualifier == ReferenceQualifierList.HouseBillOfLadingNumber)?.DTM[0]?.DateTimePeriod?.DateTimePeriod;
				ZDateTime.TryParseExact(dateString, out result, Constants.DateFormatCCYYMMDD);
				return result;
			}
		}

		public ZString FinancialAccountNumber => GettingReferenceNumberFromSG1(ReferenceQualifierList.DeferredPaymentReference);

		public ZString TotalNoOfPacks => SG1Enumerable.LastOrDefault()?.Group2[0]?.PAC[0]?.NumberOfPackages ?? ZString.Empty;

		public ZString OriginalMRN => GettingReferenceNumberFromSG1(ReferenceQualifierList.CustomsDeclarationNumber);

		public ZString MRNToBeReplaced => GettingReferenceNumberFromSG1(ReferenceQualifierList.InBondNumber);

		public IEnumerable<ZString> MarksAndNumbers
		{
			get
			{
				var sg3 = SG1Enumerable.LastOrDefault()?.Group2[0]?.Group3?.Cast<SegmentGroup3>();
				if (sg3 != null)
				{
					foreach (var group3 in sg3)
					{
						var marksLabel = group3?.PCI[0]?.MarksLabels;
						if (marksLabel != null)
						{
							yield return ZString.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}"
								, marksLabel.ShippingMarks1, marksLabel.ShippingMarks2
								, marksLabel.ShippingMarks3, marksLabel.ShippingMarks4
								, marksLabel.ShippingMarks5, marksLabel.ShippingMarks6
								, marksLabel.ShippingMarks7, marksLabel.ShippingMarks8
								, marksLabel.ShippingMarks9, marksLabel.ShippingMarks10);
						}
					}
				}
			}
		}

		#endregion

		#region LOC - Location Info

		public ZString CustomsOfficeCode => customsOfficeCode ?? (customsOfficeCode = LOCSegmentEnumerable.FirstOrDefault(loc => loc.PlaceLocationQualifier == PlaceLocationQualifierList.PlaceOfLodgementOfDocuments)?.LocationIdentification?.PlaceLocationIdentification ?? ZString.Empty);
		string customsOfficeCode;

		public ZString CustomsOfficeDescription => ZARefCusCodeListTypes.GetCustomsOfficeList(Factory).GetDescriptionFromCode(CustomsOfficeCode);

		public ZString PortOfExit => LOCSegmentEnumerable.FirstOrDefault(loc => loc.PlaceLocationQualifier == PlaceLocationQualifierList.CustomsOfficeOfDestinationTransit)?.LocationIdentification?.PlaceLocationIdentification ?? ZString.Empty;

		public ZString CountryOfExport => LOCSegmentEnumerable.FirstOrDefault(loc => loc.PlaceLocationQualifier == PlaceLocationQualifierList.CountryOfExportationDespatch)?.LocationIdentification?.PlaceLocationIdentification ?? ZString.Empty;

		public ZString CountryOfDestination => LOCSegmentEnumerable.FirstOrDefault(loc => loc.PlaceLocationQualifier == PlaceLocationQualifierList.CountryOfUltimateDestination)?.LocationIdentification?.PlaceLocationIdentification ?? ZString.Empty;

		public ZString LocationOfGoods => locationOfGoods ?? (locationOfGoods = LOCSegmentEnumerable.FirstOrDefault(loc => loc.PlaceLocationQualifier == PlaceLocationQualifierList.LocationOfGoods)?.LocationIdentification?.PlaceLocationIdentification ?? ZString.Empty);
		string locationOfGoods;

		public ZString LocationOfGoodsName => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, LocationOfGoods, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty;

		public ZString FromWarehouse => LOCSegmentEnumerable.FirstOrDefault(loc => loc.PlaceLocationQualifier == PlaceLocationQualifierList.Warehouse)?.LocationIdentification?.PlaceLocationIdentification ?? ZString.Empty;

		public ZString ToWarehouse => LOCSegmentEnumerable.FirstOrDefault(loc => loc.PlaceLocationQualifier == PlaceLocationQualifierList.CustomsOfficeOfDestination)?.LocationIdentification?.PlaceLocationIdentification ?? ZString.Empty;

		public ZString TransportDocumentIssuedAt => LOCSegmentEnumerable.FirstOrDefault(loc => loc.PlaceLocationQualifier == PlaceLocationQualifierList.PlacePortOfLoading)?.LocationIdentification?.PlaceLocationIdentification ?? ZString.Empty;

		#endregion

		#region DTM - DateTime Info

		ZDateTime LoadDateTimeFromDTM(DateTimePeriodQualifierList dateTimeType)
		{
			var result = ZDateTime.Invalid;
			var dateString = DTMSegmentEnumerable.FirstOrDefault(dtm => dtm.DateTimePeriod.DateTimePeriodQualifier == dateTimeType)?.DateTimePeriod?.DateTimePeriod;
			ZDateTime.TryParseExact(dateString, out result, Constants.DateFormatCCYYMMDD);
			return result;
		}

		public ZDateTime ETD => LoadDateTimeFromDTM(DateTimePeriodQualifierList.ArrivalDateTimeEstimated);

		public ZDateTime ActualTimeDeparture => LoadDateTimeFromDTM(DateTimePeriodQualifierList.ArrivalDateTimeActual);

		public ZDateTime ETA => LoadDateTimeFromDTM(DateTimePeriodQualifierList.ArrivalDateTimeEstimated);

		public ZDateTime ActualTimeArrival => LoadDateTimeFromDTM(DateTimePeriodQualifierList.ArrivalDateTimeActual);

		public ZDateTime DateOfDepartureOrDateOfFlight => LoadDateTimeFromDTM(DateTimePeriodQualifierList.ArrivalDateTimeActual);

		public ZDateTime DateOfAssessment => LoadDateTimeFromDTM(DateTimePeriodQualifierList.PresentationDateOfGoodsDeclarationCustoms);

		public ZDateTime DateOfArrival => LoadDateTimeFromDTM(DateTimePeriodQualifierList.ArrivalDateTimeEstimated);

		public ZDateTime ShippedOnBoardDate => LoadDateTimeFromDTM(DateTimePeriodQualifierList.ManifestShipNoticeDate);

		#endregion

		#region SG4 TDT - Transport Information

		public ZString TransportMode => transportMode ?? (transportMode = SG4TDTSegment?.ModeOfTransport?.ModeOfTransportCoded ?? ZString.Empty);
		string transportMode;

		public ZString TransportDescription => Factory.GetCachedValue<TransportModeCodeList>().GetDescriptionFromCode(TransportMode);

		public ZString RemovalTransportMode => removalTransportMode ?? (removalTransportMode = SG4TDTSegment?.ModeOfTransport?.ModeOfTransport ?? ZString.Empty);
		string removalTransportMode;

		public ZString RemovalTransportDescription => Factory.GetCachedValue<TransportModeCodeList>().GetDescriptionFromCode(RemovalTransportMode);

		public ZString VoyageFlightNo => SG4TDTSegment?.ConveyanceReferenceNumber ?? ZString.Empty;

		public ZString TransportName => SG4TDTSegment?.TransportIdentification.IdOfTheMeansOfTransport ?? ZString.Empty;

		#endregion

		#region SG5 - InvoiceInfo

		public IEnumerable<IInvoiceInformation> InvoiceInformations => invoiceInformations ??= GetInvoiceInformations();
		IEnumerable<IInvoiceInformation> invoiceInformations;

		IEnumerable<IInvoiceInformation> GetInvoiceInformations() => cusdecMessage.Group10.Count > 0
			? cusdecMessage.Group10.Cast<SegmentGroup10>().Select(sg10 => new InvoiceInformationDocWrapper(sg10))
			: cusdecMessage.Group5.Cast<SegmentGroup5>().Select(sg5 => new InvoiceInformationDocWrapper(sg5));

		#endregion

		#region SG6 NAD - Organization Info

		public ZString AgentCode => SG6NADSegmentEnumerable?.FirstOrDefault(nad => nad.PartyQualifier == PartyQualifierList.AgentRepresentative)?.PartyIdentificationDetails?.PartyIdIdentification ?? ZString.Empty;

		public ZString AgentDualProfileCode => SG6NADSegmentEnumerable?.FirstOrDefault(nad => nad.PartyQualifier == PartyQualifierList.DocumentMessageIssuerSender)?.PartyIdentificationDetails?.PartyIdIdentification ?? ZString.Empty;

		public ZString RemoverTransporterCode => SG6NADSegmentEnumerable?.FirstOrDefault(nad => nad.PartyQualifier == PartyQualifierList.TransitPrincipal)?.PartyIdentificationDetails?.PartyIdIdentification ?? ZString.Empty;

		public ZString OwnerCode => SG6NADSegmentEnumerable?.FirstOrDefault(nad => nad.PartyQualifier == PartyQualifierList.Buyer)?.PartyIdentificationDetails?.PartyIdIdentification ?? ZString.Empty;

		public IAddressInformation Supplier => supplier ?? (supplier = new AddressInformationDocWrapper(SupplierSG6));
		IAddressInformation supplier;

		public IAddressInformation Exporter => exporter ?? (exporter = new AddressInformationDocWrapper(ExporterSG6));
		IAddressInformation exporter;

		public IAddressInformation Importer => importer ?? (importer = new AddressInformationDocWrapper(ImporterSG6));
		IAddressInformation importer;

		public IAddressInformation ImporterForExportJob => importerForExportJob ?? (importerForExportJob = new AddressInformationDocWrapper(ImporterForExportSG6));
		IAddressInformation importerForExportJob;

		public IAddressInformation UnregisteredTrader => unregisteredTrader ?? (unregisteredTrader = new AddressInformationDocWrapper(UnregisteredTraderSG6));
		IAddressInformation unregisteredTrader;

		public ZString VesselAgent => SG6NADSegmentEnumerable?.FirstOrDefault(nad => nad.PartyQualifier == PartyQualifierList.CarriersAgent)?.PartyIdentificationDetails?.PartyIdIdentification ?? ZString.Empty;

		public ZString MasterCargoCarrier => SG6NADSegmentEnumerable?.FirstOrDefault(nad => nad.PartyQualifier == PartyQualifierList.ReportingCarrierCustoms)?.PartyIdentificationDetails?.PartyIdIdentification ?? ZString.Empty;

		#endregion

		public ZString CustomsProcedureCategory => cusdecMessage.CST[0].CustomsIdentityCodes1.CustomsCodeIdentification;

		public ZString CustomsProcedureCode => cusdecMessage.Group30[0].FTX.Cast<FTXSegment>().FirstOrDefault(ftx => ftx.TextSubjectQualifier == TextSubjectQualifierList.CustomsClearanceInstructions)?.TextLiteral?.FreeText1 ?? ZString.Empty;

		#region MOA Info

		public ZDecimal GetDutyOrTaxAmount(ZString type)
		{
			var result = ZDecimal.Zero;
			var amountString = SG49Enumerable.FirstOrDefault(sg49 => sg49.TAX[0].DutyTaxFeeType.DutyTaxFeeTypeCoded == DutyTaxFeeTypeCodedList.GetFromString(type))?.MOA[0].MonetaryAmount.MonetaryAmount;
			result = ZDecimal.TryParse(amountString, out result) ? result : ZDecimal.Zero;
			return result;
		}

		public ZDecimal TotalCIFCAmount => Factory.GetValue(ref cachedTotalCIFCAmount, () => GetDutyOrTaxAmount(Constants.DutyTaxFeeTypeCodes.CIFAndCValue));

		CachedProperty<ZDecimal> cachedTotalCIFCAmount;

		public ZDecimal TotalCustomsValue => Factory.GetValue(ref cachedTotalCustomsValue, () => GetDutyOrTaxAmount(Constants.DutyTaxFeeTypeCodes.CustomsValue));

		CachedProperty<ZDecimal> cachedTotalCustomsValue;

		public ZDecimal TotalDutiesAndTaxes => TotalDutiesDue + TotalVATDue;

		public ZDecimal TotalDutiesDue => Factory.GetValue(ref cachedTotalDutiesDue, () => GetDutyOrTaxAmount(Constants.DutyTaxFeeTypeCodes.TotalDutiesDue));

		CachedProperty<ZDecimal> cachedTotalDutiesDue;

		public ZDecimal TotalVATDue => Factory.GetValue(ref cachedTotalVATDue, () => GetDutyOrTaxAmount(Constants.DutyTaxFeeTypeCodes.TotalVATDue));

		CachedProperty<ZDecimal> cachedTotalVATDue;

		public ZDecimal OverpaidExcise => Factory.GetValue(ref cachedOverpaidExcise, () => GetDutyOrTaxAmount(Constants.DutyTaxFeeTypeCodes.OverpaidExcise));

		CachedProperty<ZDecimal> cachedOverpaidExcise;

		public ZDecimal UnderpaidExcise => Factory.GetValue(ref cachedUnderpaidExcise, () => GetDutyOrTaxAmount(Constants.DutyTaxFeeTypeCodes.UnpaidExcise));

		CachedProperty<ZDecimal> cachedUnderpaidExcise;

		public ZDecimal TotalTransactionValue => Factory.GetValue(ref cachedTotalTransactionValue, () => GetDutyOrTaxAmount(Constants.DutyTaxFeeTypeCodes.TransactionValue));

		CachedProperty<ZDecimal> cachedTotalTransactionValue;

		public ZString TotalTransactionValueCurrency => SG49Enumerable.FirstOrDefault(sg49 => sg49.TAX[0].DutyTaxFeeType.DutyTaxFeeTypeCoded == DutyTaxFeeTypeCodedList.GetFromString(Constants.DutyTaxFeeTypeCodes.TransactionValue))?.MOA[0].MonetaryAmount.CurrencyCoded ?? ZString.Empty;

		#endregion

		#region EQD Info

		public ZDecimal ContainersCount => Containers?.Count() ?? ZDecimal.Zero;

		public IEnumerable<IContainerInformation> Containers => containers ?? (containers = cusdecMessage.EQD.Cast<EQDSegment>().Select(eqd => new CusContainerDocWrapper(eqd, Factory)));
		IEnumerable<CusContainerDocWrapper> containers;

		public BusinessObjectCollectionWrapper<CusContainerDocWrapper> ContainersCollection => containersCollection ?? (containersCollection = new BusinessObjectCollectionWrapper<CusContainerDocWrapper>(Containers.Select(x => x as CusContainerDocWrapper ?? (new CusContainerDocWrapper(x, Factory)))));
		BusinessObjectCollectionWrapper<CusContainerDocWrapper> containersCollection;

		#endregion

		#region FTX Info

		FTXSegment FTXSegment => ftxSegment ?? (ftxSegment = cusdecMessage.FTX[0]);
		FTXSegment ftxSegment;

		public ZInt TotalLineCount => ZInt.ParseSafe(FTXSegment.TextLiteral.FreeText1, ZInt.Zero);

		public ZString CreditTerms => FTXSegment.TextLiteral.FreeText2;

		public ZString VATIndicator => FTXSegment.TextLiteral.FreeText3;

		public ZString TransactionBankCode => FTXSegment.TextLiteral.FreeText4;

		public ZString ChangeAcknowledgementIndicator => FTXSegment.TextLiteral.FreeText5;

		#endregion

		#region SG 10 (to be implemented)

		ZBool ICUSDECMessageDataProvider.ShouldOutputInvoiceDetails => false;

		ZDateTime ICUSDECMessageDataProvider.ExchangeRateDateTime => ZDateTime.Empty;

		ZDateTime ICUSDECMessageDataProvider.DateForDuty => ZDateTime.Empty;

		ZString ICUSDECMessageDataProvider.PaymentTerms => ZString.Empty;

		IEnumerable<IInvoiceHeaderInformation> ICUSDECMessageDataProvider.InvoiceHeaderInformations => Array.Empty<IInvoiceHeaderInformation>();

		#endregion

		#region SG30 - Line Level Detail

		public BusinessObjectCollectionWrapper<CUSDECMessageSG30Helper> LineLevelInformations
		{
			get
			{
				if (lineLevelInformations == null)
				{
					var date = DateOfAssessment;
					var sg30List = cusdecMessage.Group30.Cast<SegmentGroup30>().Select(sg30 => new CUSDECMessageSG30Helper(sg30, date.IsValid ? date.Date : ZDate.Today, Factory));
					lineLevelInformations = new BusinessObjectCollectionWrapper<CUSDECMessageSG30Helper>(sg30List);
				}
				return lineLevelInformations;
			}
		}
		BusinessObjectCollectionWrapper<CUSDECMessageSG30Helper> lineLevelInformations;

		IEnumerable<ILineLevelInformation> ICUSDECMessageDataProvider.LineLevelDetails
		{
			get
			{
				return LineLevelInformations.ToArray().Select(x => x as ILineLevelInformation);
			}
		}

		#endregion

		#endregion

		#region ICUSDECMessageDataProvider.Interface

		ZString ICUSDECMessageDataProvider.Consignee => ZString.Empty;

		ZBool ICUSDECMessageDataProvider.ShouldOutputDutiesDueWhenZero => ZBool.False;
		ZBool ICUSDECMessageDataProvider.ShouldOutputVATDueWhenZero => ZBool.False;
		ZBool ICUSDECMessageDataProvider.IsIntoWarehouseWarehousing => ZBool.False;
		ZBool ICUSDECMessageDataProvider.ShouldOutputFinancialAccountNumber => ZBool.False;
		ZBool ICUSDECMessageDataProvider.ShouldOutputTotalTransactionValueAndCurrency => ZBool.True;

		#endregion
	}
}
