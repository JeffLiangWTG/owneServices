using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	class ExportCustomsDeclarationDocumentWrapper : DocumentWrapper
	{
		public ExportCustomsDeclarationDocumentWrapper(CusEntryHeader entryHeader, BusinessObjectFactory factory)
			: base(entryHeader, factory)
		{
			Argument.NotNull(entryHeader, nameof(entryHeader));
			cusEntryHeader = entryHeader;
			jobDeclarationDocumentAddressConfig = entryHeader.Declaration?.DocumentSupporter?.JobDeclarationDocumentAddressConfig;
			entryInstruction = cusEntryHeader.EntryInstruction;
			cusEntryHeader.AllEntryLines.Sort(CusEntryLine.Schema.CL_LineNumber);
			Part2Sections = new BusinessObjectCollectionWrapper<ExportPart2SectionBodyWrapper>();
			goodsItems = N5203Declaration.GoodsShipment?.GovernmentAgencyGoodsItems.Cast<GovernmentAgencyGoodsItem>().ToList();
			if (goodsItems != null)
			{
				SetSectionBody();
			}
		}

		public static ExportCustomsDeclarationDocumentWrapper New(BusinessObject bizO, BusinessObjectFactory factoryToWrap)
		{
			return bizO is CusEntryHeader entryHeader ? new ExportCustomsDeclarationDocumentWrapper(entryHeader, factoryToWrap) : null;
		}

		readonly JobDeclarationDocumentAddressConfig jobDeclarationDocumentAddressConfig;
		readonly CusEntryHeader cusEntryHeader;
		readonly CusEntryInstruction entryInstruction;
		const int MaxLineCountForPart1 = 18;
		const int firstPageMaxSectionBodyRowCount = 16;
		const int otherPageMaxSectionBodyRowCount = 58;
		const int MarksLinesInPat1ForAir = 12;
		const int MarksLinesInPat1ForSea = 10;
		const int ContainerLinesInPat1ForSea = 2;
		public const int MAX_CHAR_PER_LINE_WITH_TRADEMARK = 28;
		public const int MAX_CHAR_PER_LINE = 39;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no translation needed")]
		const string MarksPrintedAtEnd = "標記資料列印於後";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no translation needed")]
		const string ContinueMarksPrinting = "(*****續列標記資料*****)";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no translation needed")]
		const string ContainersPrintedAtEnd = "貨櫃號碼資料列印於後";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no translation needed")]
		const string ContinueContainersPrinting = "(*****續列貨櫃號碼資料*****)";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no translation needed")]
		const string OtherDeclarationParticulars = "其他申報事項：";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no translation needed")]
		const string OtherDeclarationParticularsPrintedAtEnd = "其他申報事項資料列印於後";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no translation needed")]
		const string ContinueOtherDeclarationParticularsPrinting = "(*****續列其他申報事項資料*****)";

		const int MaxLineWidthForPart1 = 480;

		public static Font DocumentDefaultFont = new Font("MingLiU-ExtB", 9);
		readonly List<GovernmentAgencyGoodsItem> goodsItems;

		void SetSectionBody()
		{
			_ = GoodsItemList;
			FillUpFirstPage();
			SetPart1AndPart2();
			GenerateSectionsForPart2();
		}

		void FillUpFirstPage()
		{
			var currentRowCountToShow = GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().Sum(c => c.RowCountToShow);
			for (int i = currentRowCountToShow; i < firstPageMaxSectionBodyRowCount; i++)
			{
				AddBlankGoodsItemListSection();
			}
		}

		void AddBlankGoodsItemListSection()
		{
			var section = new GoodsItemList7Col4RowSection();
			section.AddLineToBox33List(ZString.Empty, true);
			GoodsItemListSections.Add(section);
		}

		void SetPart1AndPart2()
		{
			var part1 = new List<ZString>();
			var part2 = new List<ZString>();
			var marksLines = DocumentWrapperHelper.SplitTextByWidth(MarksNumbers, MaxLineWidthForPart1, DocumentDefaultFont);
			var containerLines = DocumentWrapperHelper.SplitTextByWidth(ContainerNumbers, MaxLineWidthForPart1, DocumentDefaultFont);
			var otherInfoLines = DocumentWrapperHelper.SplitTextByWidth(OtherDeclarationInfo, MaxLineWidthForPart1, DocumentDefaultFont);

			var marksCount = marksLines.SubRowTexts.Count;
			var containersCount = containerLines.SubRowTexts.Count;
			var othersCount = otherInfoLines.SubRowTexts.Count;
			var isSea = TransportModeCode == TransportTypeList.Codes.Sea;
			int marksLinesInPat1 = isSea ? MarksLinesInPat1ForSea : MarksLinesInPat1ForAir;
			int containersLinesInPat1 = isSea ? ContainerLinesInPat1ForSea : 0;
			var otherInfoLinesInPat1 = MaxLineCountForPart1 - marksLinesInPat1 - containersLinesInPat1 - 1;

			if (marksCount > marksLinesInPat1)
			{
				var marksNumbersSplitResult = DocumentWrapperHelper.SplitTextByWidth(MarksNumbers, MaxLineWidthForPart1, DocumentDefaultFont, marksLinesInPat1 - 1);
				part1.AddRange(marksNumbersSplitResult.SubRowTexts);
				part1.Add(MarksPrintedAtEnd);

				part2.Add(ContinueMarksPrinting);
				part2.Add(marksNumbersSplitResult.RemainingText.Trim());
			}
			else
			{
				part1.AddRange(marksLines.SubRowTexts);
				for (int i = 0; i < marksLinesInPat1 - marksCount; i++)
				{
					part1.Add(ZString.Empty);
				}
			}

			if (isSea)
			{
				if (containersCount > containersLinesInPat1)
				{
					var containersSplitResult = DocumentWrapperHelper.SplitTextByWidth(ContainerNumbers, MaxLineWidthForPart1, DocumentDefaultFont, containersLinesInPat1 - 1);
					part1.AddRange(containersSplitResult.SubRowTexts);
					part1.Add(ContainersPrintedAtEnd);

					part2.Add(ContinueContainersPrinting);
					part2.Add(containersSplitResult.RemainingText.Trim());
				}
				else
				{
					part1.AddRange(containerLines.SubRowTexts);
					for (int i = 0; i < containersLinesInPat1 - containersCount; i++)
					{
						part1.Add(ZString.Empty);
					}
				}
			}

			if (othersCount > otherInfoLinesInPat1)
			{
				var containersSplitResult = DocumentWrapperHelper.SplitTextByWidth(OtherDeclarationInfo, MaxLineWidthForPart1, DocumentDefaultFont, otherInfoLinesInPat1 - 1);
				part1.Add(OtherDeclarationParticulars);
				part1.AddRange(containersSplitResult.SubRowTexts);
				part1.Add(OtherDeclarationParticularsPrintedAtEnd);

				part2.Add(ContinueOtherDeclarationParticularsPrinting);
				part2.Add(containersSplitResult.RemainingText.Trim());
			}
			else if (othersCount > 0)
			{
				part1.Add(OtherDeclarationParticulars);
				part1.AddRange(otherInfoLines.SubRowTexts);
			}

			Part1 = part1;
			Part2 = part2;
		}

		void GenerateSectionsForPart2()
		{
			if (Part2.Any())
			{
				var section = new ExportPart2SectionBodyWrapper() { OtherDeclarationsFront = ZString.Join("\r\n", Part2.ToArray()) };
				Part2Sections.Add(section);
			}
		}

		GoodsItemListPrintWrapper fGoodsItemList;
		protected GoodsItemListPrintWrapper GoodsItemList
		{
			get
			{
				if (fGoodsItemList == null)
				{
					fGoodsItemList = new GoodsItemListPrintWrapper(N5203Declaration, goodsItems, jobDeclarationDocumentAddressConfig);
					fGoodsItemList.ProcessData();
				}
				return fGoodsItemList;
			}
		}

		IN5203Declaration fN5203Declaration;
		internal IN5203Declaration N5203Declaration => fN5203Declaration ??= GetMessageSendingObject(cusEntryHeader);
		protected virtual IN5203Declaration GetMessageSendingObject(CusEntryHeader entryHeader) => new N5203MessageSendingObject(entryHeader);

		ICustomsValuation CustomsValuation => N5203Declaration.GoodsShipment.CustomsValuation;

		ICurrencyExchange CurrencyExchange => N5203Declaration.CurrencyExchange;

		IPartyDetails Exporter => N5203Declaration.GoodsShipment.Exporter;

		IPartyDetails Buyer => N5203Declaration.GoodsShipment.Buyer;

		IConsignment Consignment => N5203Declaration.GoodsShipment.Consignment;

		IEnumerable<DeclarationDuplicate> DeclarationDuplicates => entryInstruction?.DeclarationDuplicates.Cast<DeclarationDuplicate>().Where(x => x.CY_Type == CusCodeDataTypeList.Codes.DeclarationDuplicate);

		List<ZString> fLocationOfGoods;
		List<ZString> LocationOfGoods
		{
			get
			{
				if (fLocationOfGoods == null)
				{
					fLocationOfGoods = new List<ZString>();
					ProcessLocationOfGoods();
				}
				return fLocationOfGoods;
			}
		}
		#region Properties

		public ZString TransportMode
		{
			get
			{
				switch (TransportModeCode)
				{
					case TransportTypeList.Codes.Sea:
						return (NoResString)"海運";
					case TransportTypeList.Codes.Air:
						return (NoResString)"空運";
					default:
						return ZString.Empty;
				}
			}
		}

		List<ZString> Part1 { get; set; }

		List<ZString> Part2 { get; set; }

		public ZString DeclDocTypeCodeAndDescription => cusEntryHeader.Declaration?.DeclDocTypeCodeAndDescription.InsertSafe(7, "\r\n") ?? ZString.Empty;

		ZString TransportModeCode => cusEntryHeader.Declaration?.TransportMode ?? ZString.Empty;

		public ZString DeclarationTypeCode => N5203Declaration?.TypeCode ?? ZString.Empty;

		public ZString DeclarationTypeDescription => entryInstruction?.CEI_StyleDescription.InsertSafe(9, "\r\n") ?? ZString.Empty;

		public ZInt BOMPageCount => entryInstruction?.CEI_BOMPageCount ?? ZInt.Zero;

		public ZString DeclarationID => CommonHelper.RemoveEntryNumberPlaceHolder(N5203Declaration?.ID ?? ZString.Empty);

		public ZString DeclarationIDFormatted => DocumentWrapperHelper.GetDeclarationIDFormat(DeclarationID);

		public ZString DeclarationIDBarCode => DocumentWrapperHelper.FormatBarCode(DeclarationID);

		public ZString VesselRegNum1 => Consignment?.BorderTransportMeans?.Registration ?? ZString.Empty;

		public ZString VesselRegNum2 => Consignment?.ShippingOrderNumber ?? ZString.Empty;

		public ZString ExportTransportID
		{
			get
			{
				switch (TransportModeCode)
				{
					case TransportTypeList.Codes.Sea:
						return Consignment?.BorderTransportMeans?.CallSignID ?? ZString.Empty;
					case TransportTypeList.Codes.Air:
						return Consignment?.BorderTransportMeans?.JourneyID ?? ZString.Empty;
					default:
						return ZString.Empty;
				}
			}
		}

		public ZString JourneyID => Consignment?.BorderTransportMeans?.JourneyID ?? ZString.Empty;

		public ZString DateOfDeclaration => (entryInstruction?.DateOfValuation ?? ZDateTime.Empty).ToTaiWanDateString();

		public ZString PlaceOfLoadingCode => Consignment?.LoadingLocation?.ID ?? ZString.Empty;

		public ZString PlaceOfLoadingName => cusEntryHeader.Declaration?.PortOfOriginName ?? ZString.Empty;

		public ZString DestinationCode => Consignment?.UnloadingLocation?.ID ?? ZString.Empty;

		public ZString DestinationName => cusEntryHeader.Declaration?.FinalDestinationName ?? ZString.Empty;

		public ZString LocationOfGoodsCode1 => GetLocationOfGoodsAt(0);

		public ZString LocationOfGoodsCode2 => GetLocationOfGoodsAt(1);

		public ZString MasterAirWaybill => Consignment?.TransportContractDocuments?.FirstOrDefault((x) => x.TypeCode == MessageConstants.TransportContractDocumentTypeCodes._741 || x.TypeCode == MessageConstants.TransportContractDocumentTypeCodes._704)?.ID ?? ZString.Empty;

		public ZString HouseAirWaybill => Consignment?.TransportContractDocuments?.FirstOrDefault((x) => x.TypeCode == MessageConstants.TransportContractDocumentTypeCodes._714 || x.TypeCode == MessageConstants.TransportContractDocumentTypeCodes._703)?.ID ?? ZString.Empty;

		public ZString DutyDrawback => Consignment?.BondedGoods?.Refundable ?? ZString.Empty;

		public ZString ModeOfTransport => Consignment?.DepartureTransportMeans?.TypeCode ?? ZString.Empty;

		public ZString ExportVesselName => Consignment?.DepartureTransportMeans?.Name ?? ZString.Empty;

		public ZString TotalInvoiceAmountCurrencyCode => CurrencyTypeCode;

		public ZDecimal TotalInvoiceAmount => N5203Declaration?.InvoiceAmount ?? ZDecimal.Zero;

		ZDecimal FreightChargeAmount => CustomsValuation?.FreightChargeAmount ?? ZDecimal.Zero;

		ZString CurrencyTypeCode => CurrencyExchange?.CurrencyTypeCode ?? ZString.Empty;

		public ZString FreightCurrencyCode => GetEmptyIfNoAmount(FreightChargeAmount, CurrencyTypeCode);

		public ZString Freight => FormatChargeMacro(FreightChargeAmount);

		ZDecimal ExitToEntryChargeAmount => CustomsValuation?.ExitToEntryChargeAmount ?? ZDecimal.Zero;

		public ZString InsuranceFeeCurrencyCode => GetEmptyIfNoAmount(ExitToEntryChargeAmount, CurrencyTypeCode);

		public ZString InsuranceFee => FormatChargeMacro(ExitToEntryChargeAmount);

		ZDecimal OtherChargeAmountCore => CustomsValuation?.OtherChargeAmount ?? ZDecimal.Zero;

		public ZString OtherChargeAmountCurrencyCode => GetEmptyIfNoAmount(OtherChargeAmountCore, CurrencyTypeCode);

		public ZString OtherChargeAmount => FormatChargeMacro(OtherChargeAmountCore);

		ZDecimal OtherDeductionAmountCore => CustomsValuation?.OtherDeductionAmount ?? ZDecimal.Zero;

		public ZString OtherDeductionAmountCurrencyCode => GetEmptyIfNoAmount(OtherDeductionAmountCore, CurrencyTypeCode);

		public ZString OtherDeductionAmount => FormatChargeMacro(OtherDeductionAmountCore);

		public ZString ItemChargeAmountCurrencyCode => CurrencyTypeCode;

		public ZDecimal ItemChargeAmount => cusEntryHeader.CH_TotalCustomsValueInInvoiceCurrency;

		public ZString ItemChargeAmountLocalCurrencyCode => Core.Constants.CurrencyCodes.Taiwan;

		public ZDecimal ItemChargeAmountLocalCurrency => N5203Declaration.GoodsShipment.ItemChargeAmount;

		public ZString ExporterID => Exporter?.ID ?? ZString.Empty;

		public ZString ExporterChineseName => Exporter?.ChineseName ?? ZString.Empty;

		public ZString ExporterName => Exporter?.Name ?? ZString.Empty;

		public ZString ExporterChineseAddress => jobDeclarationDocumentAddressConfig?.HideEXPExporterTradChineseAddr ?? false ? ZString.Empty : Exporter?.Address.ChineseLine ?? ZString.Empty;

		public ZString ExporterAddress => jobDeclarationDocumentAddressConfig?.HideEXPExporterEnglishAddr ?? false ? ZString.Empty : Exporter?.Address.Line ?? ZString.Empty;

		public ZString ExporterAddressLine => DocumentWrapperHelper.GetAddressLine(ExporterChineseAddress, ExporterAddress);

		public ZString ExporterAEOCode => Exporter?.LPCOAuthorizedParty?.ID ?? ZString.Empty;

		public ZString ExporterCustomsControlID => Exporter?.CustomsControlID ?? ZString.Empty;

		public ZString ExporterPaymentOnAccountBusinessID => Exporter?.PaymentOnAccountBusinessID ?? ZString.Empty;

		public ZString DutyMethodCode => N5203Declaration?.DutyTaxFee?.DutyMethodCode ?? ZString.Empty;

		public ZString BuyerChineseName => Buyer?.ChineseName ?? ZString.Empty;

		public ZString BuyerName => Buyer?.Name ?? ZString.Empty;

		public ZString BuyerChineseAddress => jobDeclarationDocumentAddressConfig?.HideEXPBuyerTradChineseAddr ?? false ? ZString.Empty : Buyer?.Address.ChineseLine ?? ZString.Empty;

		public ZString BuyerAddress => jobDeclarationDocumentAddressConfig?.HideEXPBuyerEnglishAddr ?? false ? ZString.Empty : Buyer?.Address.Line ?? ZString.Empty;

		public ZString BuyerAddressLine => DocumentWrapperHelper.GetAddressLine(BuyerChineseAddress, BuyerAddress);

		public ZString BuyerAEOCode => Buyer?.LPCOAuthorizedParty?.ID ?? ZString.Empty;

		public ZString BuyerCountryCode => Buyer?.Address.CountryCode ?? ZString.Empty;

		public ZString BuyerID => Buyer?.ID ?? ZString.Empty;

		public ZString BuyerCustomControlID => Buyer?.CustomsControlID ?? ZString.Empty;

		public ZString TradeTermsConditionCode => N5203Declaration?.GoodsShipment?.TradeTermsConditionCode ?? ZString.Empty;

		public ZDecimal ExchangeRate => N5203Declaration?.CurrencyExchange?.RateNumeric ?? ZDecimal.Zero;

		public ZString TotalNumPackagesWithUnit => string.Format(CultureInfo.InvariantCulture, "{0} / {1}", DocumentWrapperHelper.FormatQuantityNumber(N5203Declaration?.TotalPackageQuantity ?? ZInt.Zero), N5203Declaration?.Packaging?.TypeCode ?? ZString.Empty);

		public ZString PackagingMaterialDescription => N5203Declaration?.Packaging?.PackagingMaterialDescription ?? ZString.Empty;

		public ZString GrossWeight => DocumentWrapperHelper.FormatWeightNumber(N5203Declaration?.TotalGrossMassMeasure ?? ZDecimal.Zero);

		public ZString TradePromotionFeeCurrencyCode => Core.Constants.CurrencyCodes.Taiwan;

		public ZDecimal TradePromotionFee => cusEntryHeader.DutyTaxFeeCharges?.Cast<DutyTaxFeeCharge>().FirstOrDefault(x => x.ChargeType == DutyTaxFeeCodeList.Codes.B51 || x.ChargeType == DutyTaxFeeCodeList.Codes.B52 || x.ChargeType == DutyTaxFeeCodeList.Codes.B59)?.ChargeAmount ?? ZDecimal.Zero;

		public ZString TotalFeeCurrencyCode => Core.Constants.CurrencyCodes.Taiwan;

		public ZDecimal TotalFee => TradePromotionFee;

		public ZString ClearanceType => cusEntryHeader.Declaration?.ClearanceStatus ?? ZString.Empty;

		public ZString ClearanceCode => N5203Declaration?.AssociatedGovernmentProcedureCode ?? ZString.Empty;

		public ZString DuplicateNum
		{
			get
			{
				var result = ZString.Empty;
				if (DeclarationDuplicates != null)
				{
					var stringBuilder = new ZStringBuilder();
					GetNumbersOfItem(DeclarationDuplicates, 2).ToList().ForEach(item => stringBuilder.Append(item.CY_Code));
					result = stringBuilder.ToStringWithNewLineBetweenAppends();
				}
				return result;
			}
		}

		public ZString NumOfCopies
		{
			get
			{
				var result = ZString.Empty;
				if (DeclarationDuplicates != null)
				{
					var stringBuilder = new ZStringBuilder();
					GetNumbersOfItem(DeclarationDuplicates, 2).ToList().ForEach(item => stringBuilder.Append(item.CY_Data));
					result = stringBuilder.ToStringWithNewLineBetweenAppends();
				}
				return result;
			}
		}

		public ZString Declarant
		{
			get
			{
				var declarantBuilder = new ZStringBuilder();
				declarantBuilder.AppendIfNotEmpty(GetPartyName(cusEntryHeader.Declaration?.DeclarantAddress, Core.SharedConstants.Languages.ChineseTraditional));
				declarantBuilder.Append(" ");
				declarantBuilder.AppendIfNotEmpty(N5203Declaration?.Agent?.ID ?? ZString.Empty);
				declarantBuilder.AppendLine();
				declarantBuilder.AppendIfNotEmpty(N5203Declaration?.Agent?.LPCOAuthorizedParty?.ID ?? ZString.Empty);
				return declarantBuilder.ToString().Trim();
			}
		}

		public ZString DedicatedStaff => (cusEntryHeader.Declaration?.BrokerName ?? ZString.Empty) + System.Environment.NewLine + (N5203Declaration?.RepresentativePersonName ?? ZString.Empty);

		public ZString MarksAndNumbersFront1 => GetOtherDeclarationFrontAtLine(0);

		public ZString MarksAndNumbersFront2 => GetOtherDeclarationFrontAtLine(1);

		public ZString MarksAndNumbersFront3 => GetOtherDeclarationFrontAtLine(2);

		public ZString MarksAndNumbersFront4 => GetOtherDeclarationFrontAtLine(3);

		public ZString MarksAndNumbersFront5 => GetOtherDeclarationFrontAtLine(4);

		public ZString MarksAndNumbersFront6 => GetOtherDeclarationFrontAtLine(5);

		public ZString MarksAndNumbersFront7 => GetOtherDeclarationFrontAtLine(6);

		public ZString MarksAndNumbersFront8 => GetOtherDeclarationFrontAtLine(7);

		public ZString MarksAndNumbersFront9 => GetOtherDeclarationFrontAtLine(8);

		public ZString MarksAndNumbersFront10 => GetOtherDeclarationFrontAtLine(9);

		public ZString MarksAndNumbersFront11 => GetOtherDeclarationFrontAtLine(10);

		public ZString MarksAndNumbersFront12 => GetOtherDeclarationFrontAtLine(11);

		public ZString MarksAndNumbersFront13 => GetOtherDeclarationFrontAtLine(12);

		public ZString MarksAndNumbersFront14 => GetOtherDeclarationFrontAtLine(13);

		public ZString MarksAndNumbersFront15 => GetOtherDeclarationFrontAtLine(14);

		public ZString MarksAndNumbersFront16 => GetOtherDeclarationFrontAtLine(15);

		public ZString MarksAndNumbersFront17 => GetOtherDeclarationFrontAtLine(16);

		public ZString MarksAndNumbersFront18 => GetOtherDeclarationFrontAtLine(17);

		ZString MarksNumbers => N5203Declaration?.Packaging?.MarksNumbers ?? ZString.Empty;

		ZString ContainerNumbers
		{
			get
			{
				var result = ZString.Empty;
				var containers = N5203Declaration?.GoodsShipment?.Consignment?.TransportEquipments?.Select(x => GetContainerCodesForPrinting(x));
				if (containers != null && containers.Any())
				{
					result = ZString.Join("\r\n", containers.ToArray());
				}
				return result;
			}
		}

		ZString GetContainerCodesForPrinting(ITransportEquipment container)
		{
			var result = new ZStringBuilder(container.ID);
			result.AppendIfNotEmpty(container.CharacteristicCode);
			result.AppendIfNotEmpty(container.UsedCapacityCode);
			container.Seals.Take(2).ForEach(x => result.AppendIfNotEmpty(x));
			return result.ToStringWithDelimiterBetweenAppends("/");
		}

		ZString OtherDeclarationInfo => entryInstruction?.TW_TradersRemarks ?? ZString.Empty;

		public ZString JobNumber => cusEntryHeader.Declaration?.JE_DeclarationReference ?? ZString.Empty;

		public ZString OwnerReference => cusEntryHeader.Declaration?.JE_OwnerRef ?? ZString.Empty;

		public ZString EntryReleaseDate => cusEntryHeader.CH_EntryReleaseDate.ToISO8601ShortDateString() ?? ZString.Empty;

		public ZString CreateUser
		{
			get
			{
				var createUser = cusEntryHeader.Declaration?.JE_SystemCreateUser ?? ZString.Empty;
				return !createUser.IsEmpty ? cusEntryHeader.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, createUser)?.GS_FullName ?? ZString.Empty : ZString.Empty;
			}
		}

		public BusinessObjectCollectionWrapper<GoodsItemList7Col4RowSection> FirstPageGoodsItemListSections
		{
			get
			{
				if (firstPageGoodsItemListSections == null)
				{
					firstPageGoodsItemListSections = new BusinessObjectCollectionWrapper<GoodsItemList7Col4RowSection>();
					var goodsItemListSections = GoodsItemListSections;
					int remainingPageRowCount = firstPageMaxSectionBodyRowCount;
					foreach (GoodsItemList7Col4RowSection goodsItem in goodsItemListSections)
					{
						var goodsItemRowCountToShow = goodsItem.RowCountToShow;
						if (goodsItem.IsTotal && remainingPageRowCount < GoodsItemList.TotalRowCount)
						{
							FillUpRemainingRows(firstPageGoodsItemListSections, remainingPageRowCount);
							break;
						}
						else if (remainingPageRowCount >= goodsItemRowCountToShow)
						{
							firstPageGoodsItemListSections.Add(goodsItem);
						}
						remainingPageRowCount -= goodsItemRowCountToShow;
					}
				}
				return firstPageGoodsItemListSections;
			}
		}
		BusinessObjectCollectionWrapper<GoodsItemList7Col4RowSection> firstPageGoodsItemListSections;

		public BusinessObjectCollectionWrapper<GoodsItemList7Col4RowSection> OtherPageGoodsItemListSections
		{
			get
			{
				if (otherPageGoodsItemListSections == null)
				{
					otherPageGoodsItemListSections = new BusinessObjectCollectionWrapper<GoodsItemList7Col4RowSection>();
					var goodsItemListSections = GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>();
					var firstPageGoodsItemListSections = FirstPageGoodsItemListSections.Cast<GoodsItemList7Col4RowSection>();
					if (firstPageGoodsItemListSections.Sum(c => c.RowCountToShow) < goodsItemListSections.Sum(c => c.RowCountToShow))
					{
						var pageGoodsItemListSections = new BusinessObjectCollectionWrapper<GoodsItemList7Col4RowSection>();
						pageGoodsItemListSections.AddRange(goodsItemListSections);
						pageGoodsItemListSections.RemoveRange(firstPageGoodsItemListSections);
						int remainingPageRowCount = otherPageMaxSectionBodyRowCount;
						foreach (GoodsItemList7Col4RowSection goodsItem in pageGoodsItemListSections)
						{
							var goodsItemRowCountToShow = goodsItem.RowCountToShow;
							if (remainingPageRowCount < goodsItemRowCountToShow || (goodsItem.IsTotal && remainingPageRowCount < GoodsItemList.TotalRowCount))
							{
								FillUpRemainingRows(otherPageGoodsItemListSections, remainingPageRowCount);
								remainingPageRowCount = otherPageMaxSectionBodyRowCount;
							}
							remainingPageRowCount -= goodsItemRowCountToShow;
							otherPageGoodsItemListSections.Add(goodsItem);
						}
					}
				}
				return otherPageGoodsItemListSections;
			}
		}
		BusinessObjectCollectionWrapper<GoodsItemList7Col4RowSection> otherPageGoodsItemListSections;

		void FillUpRemainingRows(BusinessObjectCollectionWrapper<GoodsItemList7Col4RowSection> pageGoodsItemListSections, int remainingPageCount)
		{
			for (int i = 0; i < remainingPageCount; i++)
			{
				pageGoodsItemListSections.Add(new GoodsItemList7Col4RowSection() { ShowEvenRowIsEmpty = true });
			}
		}

		public BusinessObjectCollectionWrapper<ExportPart2SectionBodyWrapper> Part2Sections { get; }

		public BusinessObjectCollectionWrapper<GoodsItemList7Col4RowSection> GoodsItemListSections => GoodsItemList?.Sections ?? new BusinessObjectCollectionWrapper<GoodsItemList7Col4RowSection>();

		public ZInt CommonDecimalPlaces => 2;

		public CusEntryHeader CusEntryHeader => cusEntryHeader;

		#endregion

		#region Helper Functions

		IEnumerable<T> GetNumbersOfItem<T>(IEnumerable<T> originalCollection, int numbersNeeded)
		{
			int count = 0;
			foreach (var item in originalCollection)
			{
				count++;
				if (count > numbersNeeded)
				{
					break;
				}

				yield return item;
			}
		}

		ZString GetPartyName(OrgAddress partyAddress, string language)
		{
			var name = ZString.Empty;

			if (partyAddress != null)
			{
				if (partyAddress.OA_Language == language)
				{
					name = partyAddress.CompanyName;
				}
				else
				{
					name = partyAddress.GetTranslatedAddressInSpecificLanguage(language)?.CompanyName ?? ZString.Empty;
				}
			}

			return name;
		}

		ZString GetOtherDeclarationFrontAtLine(int idx)
		{
			if (Part1.Count > idx)
			{
				return Part1[idx];
			}
			return ZString.Empty;
		}

		ZString GetEmptyIfNoAmount(ZDecimal amount, ZString code) => amount > 0 ? code : ZString.Empty;

		ZString FormatChargeMacro(ZDecimal amount) => amount <= 0 ? new ZString(Constants.NIL) : DocumentWrapperHelper.FormatFormatNumberMacro(amount, CommonDecimalPlaces);

		ZString GetLocationOfGoodsAt(int idx) => LocationOfGoods.Count > idx ? LocationOfGoods[idx] : ZString.Empty;

		void ProcessLocationOfGoods()
		{
			foreach (var loc in N5203Declaration.GoodsShipment.Consignment.GoodsLocations)
			{
				if (loc.IsEmpty)
				{
					LocationOfGoods.Add(ZString.Empty);
					continue;
				}

				var codeList = (ICodeDescription)TWRefCusCodeListLoader.GetLocationOfGoods(Factory, loc, ZDateTime.Today);
				if (codeList != null && RegistryHelper.DefaultPrintingGoodsLocationDescription)
				{
					LocationOfGoods.Add(loc + " " + codeList.Description);
				}
				else
				{
					LocationOfGoods.Add(loc);
				}
			}
		}

		#endregion
	}

	public class ExportPart2SectionBodyWrapper : DocumentWrapper
	{
		public ExportPart2SectionBodyWrapper()
		{
		}

		public ZString OtherDeclarationsFront { get; set; }
	}

	#region Sub Wrapper Classes

	internal class GoodsItemPrintWrapper
	{
		readonly JobDeclarationDocumentAddressConfig jobDeclarationDocumentAddressConfig;

		public GoodsItemPrintWrapper(IN5203Declaration declaration, GovernmentAgencyGoodsItem item, JobDeclarationDocumentAddressConfig jobDeclarationDocumentAddressConfig)
		{
			n5203Declaration = declaration;
			goodsItem = item;
			this.jobDeclarationDocumentAddressConfig = jobDeclarationDocumentAddressConfig;
		}

		ZString GetCommodityNumberByType(ZString typeCode) => goodsItem.Commodity.CommodityNumbers.FirstOrDefault(commodityNum => commodityNum.IdentifierTypeCode == typeCode)?.ID ?? ZString.Empty;

		IEnumerable<ZString> GetCommodityAdditionalDocumentLines(IAdditionalDocument[] permitNumbers)
		{
			if (permitNumbers.Length > 2)
			{
				var thirdPermitNumberId = permitNumbers[2].ID;
				if (!thirdPermitNumberId.IsEmpty)
				{
					int documentCount = Math.Min(permitNumbers.Length, 5);
					for (int i = 2; i < documentCount; i++)
					{
						var docStr = DocumentWrapperHelper.FormatAdditionalDocument(permitNumbers[i].ID, permitNumbers[i].SequenceNumeric);
						if (!docStr.IsEmpty)
						{
							yield return docStr;
						}
					}
				}
			}
		}

		ZString GetOriginDocumentLine(IAdditionalDocument[] permitNumbers)
		{
			var result = ZString.Empty;
			var firstPermitNumberValid = (!permitNumbers.FirstOrDefault()?.ID.IsEmpty) ?? false;
			if (firstPermitNumberValid)
			{
				var secondPermitNumberValid = (!permitNumbers.ElementAtOrDefault(1)?.ID.IsEmpty) ?? false;
				if (secondPermitNumberValid)
				{
					var originDocument = goodsItem.Origin.AdditionalDocument;
					var originDocumentID = originDocument.ID;
					if (!originDocumentID.IsEmpty)
					{
						result = DocumentWrapperHelper.FormatAdditionalDocument(originDocumentID, originDocument.SequenceNumeric);
					}
				}
			}
			return result;
		}

		IEnumerable<ZString> GetGoodsItemAdditionalDocumentLines()
		{
			var assignNumAy = goodsItem.AdditionalDocuments.ToArray();

			int docNum = Math.Min(assignNumAy.Length, 10);
			for (int i = 1; i < docNum; i++)
			{
				var id = assignNumAy[i].ID;
				if (!id.IsEmpty)
				{
					yield return id;
				}
			}
		}

		readonly IN5203Declaration n5203Declaration;
		readonly GovernmentAgencyGoodsItem goodsItem;

		bool TrademarkImageAvailable => goodsItem.InvoiceLine.TrademarkImage != null;

		internal ZString SequenceNum => goodsItem.SequenceNumeric.ToString();

		internal ZString Brand => goodsItem.Commodity.Name;

		internal IEnumerable<ZString> GetDetails(GoodsItemList7Col4RowSection currentSection)
		{
			var lines = new List<ZString>();
			if (jobDeclarationDocumentAddressConfig != null)
			{
				var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().OrderBy(c => c.Position);
				var commodity = goodsItem.Commodity;
				var commodityAdditionalDocuments = commodity.AdditionalDocuments.ToArray();

				foreach (JobDeclarationDocumentGoodsDescriptionConfig goodsDescriptionConfig in goodsDescriptionConfigs)
				{
					var description = ZString.Empty;
					var field = goodsDescriptionConfig.Field;
					var isMultipleData = false;
					switch (field)
					{
						case ExportDeclarationDocumentFieldList.Codes.SupplierPartNumber:
							description = GetCommodityNumberByType(MessageConstants.IdentificationTypeCodes.SA);
							break;
						case ExportDeclarationDocumentFieldList.Codes.OwnerPartNumber:
							description = GetCommodityNumberByType(MessageConstants.IdentificationTypeCodes.BP);
							break;
						case ExportDeclarationDocumentFieldList.Codes.GoodsDescription:
							description = goodsItem.CommodityDescriptionForDocument;
							break;
						case ExportDeclarationDocumentFieldList.Codes.Model:
							description = commodity.CommercialCategorizationID;
							break;
						case ExportDeclarationDocumentFieldList.Codes.Specification:
							description = commodity.Constituent.ElementDescription;
							break;
						case ExportDeclarationDocumentFieldList.Codes.PreviousBondedEntryNumber:
							var preBondedDoc = goodsItem.PreBondedDocument;
							if (preBondedDoc != null)
							{
								description = DocumentWrapperHelper.FormatAdditionalDocument(preBondedDoc.ID, preBondedDoc.LineNumeric);
							}
							break;
						case ExportDeclarationDocumentFieldList.Codes.PreviousEntryNumber:
							var previousDoc = goodsItem.PreviousDocument;
							if (previousDoc != null)
							{
								description = DocumentWrapperHelper.FormatAdditionalDocument(previousDoc.ID, previousDoc.LineNumeric);
							}
							break;
						case ExportDeclarationDocumentFieldList.Codes.Permits:
							var additionalDocuments = GetCommodityAdditionalDocumentLines(commodityAdditionalDocuments);
							isMultipleData = additionalDocuments.Count() > 1;
							description = ZString.Join("\r\n", additionalDocuments.ToArray());
							break;
						case ExportDeclarationDocumentFieldList.Codes.CertificateOfOrigin:
							description = GetOriginDocumentLine(commodityAdditionalDocuments);
							break;
						case ExportDeclarationDocumentFieldList.Codes.AssignedNumbers:
							var goodsItemAdditionalDocuments = GetGoodsItemAdditionalDocumentLines();
							isMultipleData = goodsItemAdditionalDocuments.Count() > 1;
							description = ZString.Join("\r\n", goodsItemAdditionalDocuments.ToArray());
							break;
						case ExportDeclarationDocumentFieldList.Codes.GoodsOrigin:
							description = goodsItem.Origin.CountryCode;
							break;
						default:
							break;
					}

					if (!description.IsEmpty)
					{
						var hasBrandLine2 = !currentSection.Box33BrandLine2.IsEmpty;
						var trademarkImageAvailableLine = hasBrandLine2 ? 4 : 3;
						var numberOfLinesShortenedForTrademark = TrademarkImageAvailable ? Math.Max(0, trademarkImageAvailableLine - lines.Count) : (hasBrandLine2 ? 1 : 0);
						var maxCharPerLine = numberOfLinesShortenedForTrademark > 0 ? ExportCustomsDeclarationDocumentWrapper.MAX_CHAR_PER_LINE_WITH_TRADEMARK : ExportCustomsDeclarationDocumentWrapper.MAX_CHAR_PER_LINE;
						var goodsDescriptionConfigCaption = goodsDescriptionConfig.Caption;
						var captionAndDescription = string.Format("{0}{1}", goodsDescriptionConfigCaption, description);
						if (isMultipleData || DocumentWrapperHelper.IsTextExceedsMaxChar(captionAndDescription, maxCharPerLine))
						{
							SplitTextByWidthAndAddToList(string.Format("{0}\r\n{1}", goodsDescriptionConfigCaption, description), lines, numberOfLinesShortenedForTrademark);
						}
						else
						{
							SplitTextByWidthAndAddToList(captionAndDescription, lines, numberOfLinesShortenedForTrademark);
						}
					}
				}
			}
			return lines;
		}

		void SplitTextByWidthAndAddToList(ZString text, List<ZString> listToAdd, int numberOfLinesShortenedForTrademark)
		{
			var textToProcess = text.Trim();
			if (numberOfLinesShortenedForTrademark > 0)
			{
				var splitResultWithTradeMark = DocumentWrapperHelper.SplitTextByLineBreak(textToProcess, ExportCustomsDeclarationDocumentWrapper.MAX_CHAR_PER_LINE_WITH_TRADEMARK, true);
				var textToProcessIndex = 0;
				var splitResultWithTradeMarkIndex = 0;
				foreach (var str in splitResultWithTradeMark)
				{
					listToAdd.Add(str);
					textToProcessIndex = textToProcess.IndexOf(str, textToProcessIndex, StringComparison.Ordinal) + str.Length;
					if (++splitResultWithTradeMarkIndex == numberOfLinesShortenedForTrademark)
					{
						break;
					}
				}
				textToProcess = textToProcess.Remove(0, textToProcessIndex).Trim();
			}
			var splitResult = DocumentWrapperHelper.SplitTextByLineBreak(textToProcess, ExportCustomsDeclarationDocumentWrapper.MAX_CHAR_PER_LINE, true);
			listToAdd.AddRange(splitResult);
		}

		internal IEnumerable<ZString> Codes
		{
			get
			{
				var permitNumbers = goodsItem.Commodity.AdditionalDocuments.ToArray();
				var firstPermitNumber = permitNumbers.FirstOrDefault();
				var secondPermitNumber = permitNumbers.ElementAtOrDefault(1);
				var originDocument = goodsItem.Origin.AdditionalDocument;
				bool firstPermitNumberValid = firstPermitNumber != null && !firstPermitNumber.ID.IsEmpty;
				if (firstPermitNumberValid)
				{
					yield return DocumentWrapperHelper.FormatAdditionalDocument(firstPermitNumber.ID, firstPermitNumber.SequenceNumeric);
				}
				else
				{
					yield return DocumentWrapperHelper.FormatAdditionalDocument(originDocument.ID, originDocument.SequenceNumeric);
				}

				if (secondPermitNumber != null && !secondPermitNumber.ID.IsEmpty)
				{
					yield return DocumentWrapperHelper.FormatAdditionalDocument(secondPermitNumber.ID, secondPermitNumber.SequenceNumeric);
				}
				else if (firstPermitNumberValid)
				{
					yield return DocumentWrapperHelper.FormatAdditionalDocument(originDocument.ID, originDocument.SequenceNumeric);
				}

				var tariffNumber = goodsItem.Commodity.Classifications.FirstOrDefault((x) => x.IdentificationTypeCode == MessageConstants.IdentificationTypeCodes.HS)?.ID ?? ZString.Empty;
				yield return DocumentWrapperHelper.FormatTariffNumber(tariffNumber);

				var bondedNoteCode = goodsItem.Commodity.BondedNoteCode;
				var assignedNumber = goodsItem.AdditionalDocuments.FirstOrDefault()?.ID ?? ZString.Empty;
				if (!bondedNoteCode.IsEmpty || !assignedNumber.IsEmpty)
				{
					var delimiter = (!bondedNoteCode.IsEmpty && !assignedNumber.IsEmpty) ? new ZString("/") : ZString.Empty;
					yield return bondedNoteCode + delimiter + assignedNumber;
				}
				else
				{
					yield return ZString.Empty;
				}
			}
		}

		ZString CurrencyTypeCode => n5203Declaration?.CurrencyExchange?.CurrencyTypeCode ?? ZString.Empty;
		internal IEnumerable<ZString> Prices
		{
			get
			{
				var decimalPlace = DocumentWrapperHelper.GetDecimalPlaces(goodsItems, x => x.Commodity.InvoiceLine.UnitPriceAmount);
				yield return CurrencyTypeCode;
				yield return DocumentWrapperHelper.FormatNumber(goodsItem.Commodity.InvoiceLine.UnitPriceAmount, decimalPlace);
			}
		}

		internal IEnumerable<ZString> WeightAndQuantity
		{
			get
			{
				var decimalPlace = DocumentWrapperHelper.GetDecimalPlaces(goodsItems, x => x?.GoodsMeasure?.NetWeightMeasure ?? ZDecimal.Zero);
				var statisticalMeasure = goodsItem.GoodsStatisticalMeasure;
				var goodsMeasure = goodsItem.GoodsMeasure;
				yield return string.Format(CultureInfo.InvariantCulture, "{0} {1}", DocumentWrapperHelper.FormatNumber(goodsItem.GoodsMeasure.NetWeightMeasure, decimalPlace), Constants.UnitOfQuantityCodes.Kilograms);
				yield return FomatNumberWithUnitCode("{0} {1}", goodsMeasure.UnitCode, goodsMeasure.TariffQuantity, x => x?.GoodsMeasure?.TariffQuantity ?? ZDecimal.Zero);
				yield return FomatNumberWithUnitCode("({0} {1})", statisticalMeasure.StatisticalUnitCode, statisticalMeasure.TariffQuantity, x => x?.GoodsStatisticalMeasure?.TariffQuantity ?? ZDecimal.Zero);
			}
		}

		ZString FomatNumberWithUnitCode(string format, ZString unitCode, ZDecimal number, Func<IGovernmentAgencyGoodsItem, ZDecimal> getCalcNumber)
		{
			var result = ZString.Empty;
			if (!unitCode.IsEmpty)
			{
				var decimalPlace = DocumentWrapperHelper.GetDecimalPlaces(goodsItems, x => getCalcNumber(x));
				result = string.Format(CultureInfo.InvariantCulture, format, DocumentWrapperHelper.FormatNumber(number, decimalPlace), unitCode);
			}
			return result;
		}

		IEnumerable<GovernmentAgencyGoodsItem> goodsItems => n5203Declaration.GoodsShipment.GovernmentAgencyGoodsItems.Cast<GovernmentAgencyGoodsItem>();

		internal IEnumerable<ZString> FOBValue
		{
			get
			{
				yield return DocumentWrapperHelper.FormatCurrencyMacro(goodsItem.Commodity.InvoiceLine.ItemChargeAmount, Core.Constants.CurrencyCodes.Taiwan);
			}
		}

		internal IEnumerable<ZString> StatisticCode
		{
			get
			{
				yield return goodsItem.GovernmentProcedure.CurrentCode;
			}
		}
	}

	class GoodsItemListPrintWrapper
	{
		readonly JobDeclarationDocumentAddressConfig jobDeclarationDocumentAddressConfig;

		public GoodsItemListPrintWrapper(IN5203Declaration declaration, List<GovernmentAgencyGoodsItem> goodsItems1, JobDeclarationDocumentAddressConfig jobDeclarationDocumentAddressConfig)
		{
			Sections = new BusinessObjectCollectionWrapper<GoodsItemList7Col4RowSection>();
			n5203Declaration = declaration;
			goodsItems = goodsItems1;
			this.jobDeclarationDocumentAddressConfig = jobDeclarationDocumentAddressConfig;
		}

		readonly List<GovernmentAgencyGoodsItem> goodsItems;
		readonly IN5203Declaration n5203Declaration;

		GoodsItemList7Col4RowSection CurrentSection
		{
			get
			{
				if (currentSection == null)
				{
					StartNewSection();
				}
				return currentSection;
			}
		}
		GoodsItemList7Col4RowSection currentSection;

		internal BusinessObjectCollectionWrapper<GoodsItemList7Col4RowSection> Sections { get; }

		void BreakCurrentSection()
		{
			currentSection = null;
		}

		void StartNewSection()
		{
			currentSection = new GoodsItemList7Col4RowSection();
			Sections.Add(currentSection);
		}

		internal void ProcessData()
		{
			ZDecimal netWeightSum = 0;
			ZDecimal fobSum = 0;
			var quantitySums = new Dictionary<ZString, ZDecimal>();
			var statisticQuantitySums = new Dictionary<ZString, ZDecimal>();

			var goodsItems = this.goodsItems;
			foreach (var item in goodsItems)
			{
				ProcessItemGrouping(item);
				CalculateItemDetails(item, ref fobSum, ref netWeightSum);
				ProcessItemDetails(item, quantitySums, statisticQuantitySums);
			}

			ProcessTotals(fobSum, netWeightSum, quantitySums, statisticQuantitySums);
		}

		void ProcessItemGrouping(GovernmentAgencyGoodsItem item)
		{
			var grouping = item.EntryLineGroupForDocument;

			if (grouping.IsEmpty)
			{
				return;
			}

			var groupingLines = DocumentWrapperHelper.SplitTextByLineBreak(grouping, ExportCustomsDeclarationDocumentWrapper.MAX_CHAR_PER_LINE, true);
			for (var i = 0; i < groupingLines.Count; ++i)
			{
				if (i == 0)
				{
					CurrentSection.AddLineToBox33List(ZString.Empty, true);
				}
				BreakCurrentSection();
				CurrentSection.AddLineToBox33List(groupingLines[i]);
			}

			BreakCurrentSection();
			CurrentSection.AddLineToBox33List(ZString.Empty, true);
			BreakCurrentSection();
		}

		void CalculateItemDetails(GovernmentAgencyGoodsItem goodsItem, ref ZDecimal fobSum, ref ZDecimal netWeightSum)
		{
			netWeightSum += goodsItem.GoodsMeasure.NetWeightMeasure;
			fobSum += goodsItem.Commodity.InvoiceLine.ItemChargeAmount;
		}

		void ProcessItemDetails(GovernmentAgencyGoodsItem goodsItem,
			Dictionary<ZString, ZDecimal> quantitySums,
			Dictionary<ZString, ZDecimal> statisticQuantitySums)
		{
			if (!goodsItem.GoodsMeasure.UnitCode.IsEmpty)
			{
				if (quantitySums.ContainsKey(goodsItem.GoodsMeasure.UnitCode))
				{
					quantitySums[goodsItem.GoodsMeasure.UnitCode] = quantitySums[goodsItem.GoodsMeasure.UnitCode] + goodsItem.GoodsMeasure.TariffQuantity;
				}
				else
				{
					quantitySums.Add(goodsItem.GoodsMeasure.UnitCode, goodsItem.GoodsMeasure.TariffQuantity);
				}
			}
			if (!goodsItem.GoodsStatisticalMeasure.StatisticalUnitCode.IsEmpty)
			{
				if (statisticQuantitySums.ContainsKey(goodsItem.GoodsStatisticalMeasure.StatisticalUnitCode))
				{
					statisticQuantitySums[goodsItem.GoodsStatisticalMeasure.StatisticalUnitCode] = statisticQuantitySums[goodsItem.GoodsStatisticalMeasure.StatisticalUnitCode] + goodsItem.GoodsStatisticalMeasure.TariffQuantity;
				}
				else
				{
					statisticQuantitySums.Add(goodsItem.GoodsStatisticalMeasure.StatisticalUnitCode, goodsItem.GoodsStatisticalMeasure.TariffQuantity);
				}
			}

			var goodsItemWrapper = new GoodsItemPrintWrapper(n5203Declaration, goodsItem, jobDeclarationDocumentAddressConfig);
			CurrentSection.IsNotPrintEmpty = true;
			CurrentSection.Box32ItemNumber = goodsItemWrapper.SequenceNum;
			var brandLines = DocumentWrapperHelper.SplitTextByWidthOnce(goodsItemWrapper.Brand, 220, ExportCustomsDeclarationDocumentWrapper.DocumentDefaultFont);
			CurrentSection.Box33BrandLine1 = brandLines.FirstRowText;
			CurrentSection.Box33BrandLine2 = brandLines.RemainingText;
			CurrentSection.AddLineToBox33List(ZString.Empty);
			goodsItemWrapper.Codes.ToList().ForEach(line => CurrentSection.AddLineToBox34Box35List(line));
			goodsItemWrapper.Prices.ToList().ForEach(line => CurrentSection.AddLineToBox36List(line));
			goodsItemWrapper.WeightAndQuantity.ToList().ForEach(line => CurrentSection.AddLineToBox37Box38Box39List(line));
			goodsItemWrapper.FOBValue.ToList().ForEach(line => CurrentSection.AddLineToBox40List(line));
			goodsItemWrapper.StatisticCode.ToList().ForEach(line => CurrentSection.AddLineToBox41List(line));
			SetTrademarkImage(goodsItem);
			var index = 0;
			foreach (var line in goodsItemWrapper.GetDetails(CurrentSection))
			{
				index++;
				if (index > 3)
				{
					BreakCurrentSection();
				}
				CurrentSection.AddLineToBox33List(line);
			}

			BreakCurrentSection();
		}

		void SetTrademarkImage(GovernmentAgencyGoodsItem goodsItem)
		{
			var trademarkImage = goodsItem.InvoiceLine.TrademarkImage;
			if (trademarkImage != null)
			{
				if (CurrentSection.DetailsFull)
				{
					BreakCurrentSection();
				}

				CurrentSection.TrademarkImage = trademarkImage;
				if (CurrentSection.Box33BrandLine2.IsEmpty)
				{
					CurrentSection.TrademarkImage1 = trademarkImage;
				}
				else
				{
					CurrentSection.TrademarkImage2 = trademarkImage;
				}
			}
		}

		public int TotalRowCount { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no translation needed")]
		void ProcessTotals(ZDecimal fobSum, ZDecimal netWeightSum, Dictionary<ZString, ZDecimal> quantitySums, Dictionary<ZString, ZDecimal> statisticQuantitySums)
		{
			CurrentSection.IsTotal = true;
			CurrentSection.AddLineToBox34Box35List(ZString.Empty);
			CurrentSection.AddLineToBox34Box35List("Total:");
			CurrentSection.AddLineToBox36List("        ----------");
			CurrentSection.AddLineToBox40List("----------------------");
			CurrentSection.AddLineToBox40List(DocumentWrapperHelper.FormatCurrencyMacro(fobSum, Core.Constants.CurrencyCodes.Taiwan));
			CurrentSection.AddLineToBox40List("vvvvvvvvvv");
			CurrentSection.AddLineToBox33List(ZString.Empty);
			CurrentSection.AddLineToBox33List(ZString.Empty);
			CurrentSection.AddLineToBox37Box38Box39List("----------------------");
			CurrentSection.AddLineToBox37Box38Box39List(string.Format(CultureInfo.InvariantCulture, "{0} {1}", DocumentWrapperHelper.FormatWeightNumber(netWeightSum), Constants.UnitOfQuantityCodes.Kilograms));
			int rowCount = 3;
			foreach (var line in quantitySums)
			{
				if (CurrentSection.WeightAndQuantityFull)
				{
					BreakCurrentSection();
				}
				CurrentSection.AddLineToBox37Box38Box39List(string.Format(CultureInfo.InvariantCulture, "{0} {1}", DocumentWrapperHelper.FormatQuantityNumber(line.Value), line.Key));
				CurrentSection.AddLineToBox33List(ZString.Empty);
				rowCount++;
			}
			foreach (var line in statisticQuantitySums)
			{
				if (CurrentSection.WeightAndQuantityFull)
				{
					BreakCurrentSection();
				}
				CurrentSection.AddLineToBox37Box38Box39List(string.Format(CultureInfo.InvariantCulture, "({0} {1})", DocumentWrapperHelper.FormatQuantityNumber(line.Value), line.Key));
				CurrentSection.AddLineToBox33List(ZString.Empty);
				rowCount++;
			}
			if (CurrentSection.WeightAndQuantityFull)
			{
				BreakCurrentSection();
			}
			CurrentSection.AddLineToBox37Box38Box39List("vvvvvvvvvvvv");
			CurrentSection.AddLineToBox33List(ZString.Empty);
			TotalRowCount = rowCount++;
			BreakCurrentSection();
		}
	}

	internal class GoodsItemList7Col4RowSection : NonPersistentBusinessObject
	{
		public GoodsItemList7Col4RowSection()
		{
			box33List = new List<ZString>();
			box34Box35List = new List<ZString>();
			box36List = new List<ZString>();
			box37Box38Box39List = new List<ZString>();
			box40List = new List<ZString>();
			box41List = new List<ZString>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no translation needed")]
		const string PrintEmptyValue = "(本欄空白)";

		internal void AddLineToBox33List(ZString line, bool showEvenRowIsEmpty = false)
		{
			box33List.Add(line);
			ShowEvenRowIsEmpty = showEvenRowIsEmpty;
		}

		internal void AddLineToBox34Box35List(ZString line)
		{
			box34Box35List.Add(line);
		}

		internal void AddLineToBox36List(ZString line)
		{
			box36List.Add(line);
		}

		internal void AddLineToBox37Box38Box39List(ZString line)
		{
			box37Box38Box39List.Add(line);
		}

		internal void AddLineToBox40List(ZString line)
		{
			box40List.Add(line);
		}

		internal void AddLineToBox41List(ZString line)
		{
			box41List.Add(line);
		}

		internal List<ZString> box33List { get; }
		readonly List<ZString> box34Box35List;
		readonly List<ZString> box36List;
		readonly List<ZString> box37Box38Box39List;
		readonly List<ZString> box40List;
		readonly List<ZString> box41List;

		public Image TrademarkImage { get; set; }
		public Image TrademarkImage1 { get; set; }
		public Image TrademarkImage2 { get; set; }
		ZString GetBox33(ZInt detailIndex) => box33List.Count > detailIndex ? box33List[detailIndex] : ZString.Empty;
		ZString GetBox34Box35(ZInt codeIndex) => box34Box35List.Count > codeIndex ? box34Box35List[codeIndex] : ZString.Empty;
		ZString GetBox36(ZInt priceIndex) => box36List.Count > priceIndex ? box36List[priceIndex] : ZString.Empty;
		ZString GetBox37Box38Box39(ZInt weightIndex) => box37Box38Box39List.Count > weightIndex ? box37Box38Box39List[weightIndex] : ZString.Empty;
		ZString GetBox40(ZInt fobIndex) => box40List.Count > fobIndex ? box40List[fobIndex] : ZString.Empty;
		ZString GetBox41(ZInt statisticCodeIndex) => box41List.Count > statisticCodeIndex ? box41List[statisticCodeIndex] : ZString.Empty;
		internal bool DetailsFull => box33List.Count > 3;
		internal bool WeightAndQuantityFull => box37Box38Box39List.Count > 3;
		internal bool IsTotal { get; set; }
		internal bool IsNotPrintEmpty { get; set; }
		internal bool ShowEvenRowIsEmpty { get; set; }
		public ZBool ShouldHideRow1 => Box33DescriptionOfGoods_Line1.IsEmpty && Box34ImportExportPermitNumberAndItemNumber_Line1.IsEmpty && Box36UnitPrice_Line1.IsEmpty && Box37NetWeight.IsEmpty && Box40FOBValue_Line1.IsEmpty && Box41ModeOfStatistics_Line1.IsEmpty && Box33BrandLine1.IsEmpty && Box32ItemNumber.IsEmpty && !ShowEvenRowIsEmpty;
		public ZBool ShouldHideRow2 => Box33DescriptionOfGoods_Line2.IsEmpty && Box34ImportExportPermitNumberAndItemNumber_Line2.IsEmpty && Box36UnitPrice_Line2.IsEmpty && Box38QuantityAndUnit.IsEmpty && Box40FOBValue_Line2.IsEmpty && Box41ModeOfStatistics_Line2.IsEmpty && TrademarkImage1 == null && Box33BrandLine2.IsEmpty;
		public ZBool ShouldHideRow3 => Box33DescriptionOfGoods_Line3.IsEmpty && Box35CCCCode.IsEmpty && Box36UnitPrice_Line3.IsEmpty && Box39StatisticsQuantityAndUnit_Line1.IsEmpty && Box40FOBValue_Line3.IsEmpty && Box41ModeOfStatistics_Line3.IsEmpty && (TrademarkImage1 == null || TrademarkImage1.Height <= 24) && TrademarkImage2 == null;
		public ZBool ShouldHideRow4 => Box33DescriptionOfGoods_Line4.IsEmpty && Box35BondedGoodsCodeAndAssignedNumber.IsEmpty && Box36UnitPrice_Line4.IsEmpty && Box39StatisticsQuantityAndUnit_Line2.IsEmpty && Box40FOBValue_Line4.IsEmpty && Box41ModeOfStatistics_Line4.IsEmpty && (TrademarkImage1 == null || TrademarkImage1.Height <= 36) && (TrademarkImage2 == null || TrademarkImage2?.Height <= 24);
		public int RowCountToShow => (!ShouldHideRow1 ? 1 : 0) + (!ShouldHideRow2 ? 1 : 0) + (!ShouldHideRow3 ? 1 : 0) + (!ShouldHideRow4 ? 1 : 0);
		public ZString Box33DescriptionOfGoods_Line1 => GetBox33(0).ReplaceLineBreakWithSpace();
		public ZString Box33DescriptionOfGoods_Line2 => GetBox33(1).ReplaceLineBreakWithSpace();
		public ZString Box33DescriptionOfGoods_Line3 => GetBox33(2).ReplaceLineBreakWithSpace();
		public ZString Box33DescriptionOfGoods_Line4 => GetBox33(3).ReplaceLineBreakWithSpace();
		public ZString Box32ItemNumber { get; set; }
		public ZString Box33Brand { get; set; }
		public ZString Box33BrandLine1 { get; set; }
		public ZString Box33BrandLine2 { get; set; }
		public ZString Box34ImportExportPermitNumberAndItemNumber_Line1 => GetBox34Box35(0);
		public ZString Box34ImportExportPermitNumberAndItemNumber_Line2 => GetBox34Box35(1);
		public ZString Box35CCCCode => GetBox34Box35(2);
		public ZString Box35BondedGoodsCodeAndAssignedNumber => GetBox34Box35(3);
		public ZString Box36UnitPrice_Line1 => GetBox36(0);
		public ZString Box36UnitPrice_Line2 => GetBox36(1);
		public ZString Box36UnitPrice_Line3 => GetBox36(2);
		public ZString Box36UnitPrice_Line4 => GetBox36(3);
		public ZString Box36Line11 => IsNotPrintEmpty || IsTotal ? Box36UnitPrice_Line1 : ZString.Empty;
		public ZString Box36Line31 => IsNotPrintEmpty ? (ZString)PrintEmptyValue : ZString.Empty;
		public ZString Box37NetWeight => GetBox37Box38Box39(0);
		public ZString Box38QuantityAndUnit => GetBox37Box38Box39(1);
		public ZString Box39StatisticsQuantityAndUnit_Line1 => GetBox37Box38Box39(2);
		public ZString Box39StatisticsQuantityAndUnit_Line2 => GetBox37Box38Box39(3);
		public ZString Box40FOBValue_Line1 => GetBox40(0);
		public ZString Box40FOBValue_Line2 => GetBox40(1);
		public ZString Box40FOBValue_Line3 => GetBox40(2);
		public ZString Box40FOBValue_Line4 => GetBox40(3);
		public ZString Box40Line11 => IsNotPrintEmpty || IsTotal ? (ZString)PrintEmptyValue : ZString.Empty;
		public ZString Box40Line21 => ZString.Empty;
		public ZString Box41ModeOfStatistics_Line1 => GetBox41(0);
		public ZString Box41ModeOfStatistics_Line2 => GetBox41(1);
		public ZString Box41ModeOfStatistics_Line3 => GetBox41(2);
		public ZString Box41ModeOfStatistics_Line4 => GetBox41(3);
	}
	#endregion
}
