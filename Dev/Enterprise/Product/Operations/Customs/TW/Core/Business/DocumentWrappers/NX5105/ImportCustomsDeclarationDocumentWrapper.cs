using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.Customs.TW.Messaging;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.TW.Business.MessageConstants;

namespace Enterprise.Customs.TW.Business
{
	public class ImportCustomsDeclarationDocumentWrapper : DocumentWrapper, IDocumentWrapper, IChapterNameGenerator
	{
		const int MAX_LINE_WIDTH_FOR_PART1 = 524;
		const int MaxLineCountForPart1 = 18;

		const int MAX_CHAR_PER_LINE = 35;
		const int MAX_CHAR_PER_LINE_WITH_TRADEMARK = 26;
		const int firstPageMaxSectionBodyRowCount = 18;
		const int otherPageMaxSectionBodyRowCount = 56;
		const int MarksLinesInPart1ForAir = 12;
		const int MarksLinesInPart1ForSea = 10;
		const int ContainerLinesInPart1ForSea = 2;

		readonly Font sectionBodyFont;
		readonly JobDeclarationDocumentAddressConfig jobDeclarationDocumentAddressConfig;
		readonly int customizeSectionBodyRow;

		#region SuppressResourceStringsCheckRegion
		const string MarksPrintedAtEnd = "標記資料列印於後";
		const string ContinueMarksPrinting = "(*****續列標記資料*****)";
		const string ContainersPrintedAtEnd = "貨櫃號碼資料列印於後";
		const string ContinueContainersPrinting = "(*****續列貨櫃號碼資料*****)";
		const string OtherDeclarationParticularsPrintedAtEnd = "其他申報事項資料列印於後";
		const string ContinueOtherDeclarationParticularsPrinting = "(*****續列其他申報事項資料*****)";
		const string OtherDeclarationParticulars = "其他申報事項：";
		const string FobAmountCaption = "離  岸  價  格";
		const string ExwAmountCaption = "出  廠  價  格";
		const string RAP = "RAP";
		const string ROR = "ROR";
		#endregion

		public ImportCustomsDeclarationDocumentWrapper(CusEntryHeader entryHeader, BusinessObjectFactory factory)
			: this(entryHeader, entryHeader != null ? new NX5105MessageSendingObject(entryHeader) : null, factory)
		{
		}

		protected ImportCustomsDeclarationDocumentWrapper(CusEntryHeader entryHeader, INX5105Declaration declaration, BusinessObjectFactory factory)
			: base(entryHeader, factory)
		{
			Argument.NotNull(entryHeader, nameof(entryHeader));
			cusEntryHeader = entryHeader;
			jobDeclarationDocumentAddressConfig = entryHeader.Declaration?.DocumentSupporter?.JobDeclarationDocumentAddressConfig;
			if (!int.TryParse(jobDeclarationDocumentAddressConfig?.CustomizeSectionBodyRow, out customizeSectionBodyRow))
			{
				customizeSectionBodyRow = 0;
			}
			sectionBodyFont = new Font("MingLiU-ExtB", 9f);
			Sections = new BusinessObjectCollectionWrapper<ImportDeclarationSectionBodyWrapper>();
			Part2Sections = new BusinessObjectCollectionWrapper<ImportPart2SectionBodyWrapper>();
			dutyList = new List<Tuple<ZString, ZString, ZDecimal, ZBool>>();
			entryInstruction = entryHeader.EntryInstruction;
			fINX5105Declaration = declaration;

			var goodsShipment = fINX5105Declaration.GoodsShipment;
			if (goodsShipment != null)
			{
				SetDutyCharges(goodsShipment);
				goodsItems = goodsShipment.GovernmentAgencyGoodsItems.ToList();
				SetSectionBody(goodsItems);
			}
		}

		public static ImportCustomsDeclarationDocumentWrapper New(BusinessObject bizO, BusinessObjectFactory factoryToWrap)
		{
			return bizO is CusEntryHeader entryHeader ? new ImportCustomsDeclarationDocumentWrapper(entryHeader, factoryToWrap) : null;
		}

		void SetDutyCharges(IGoodsShipment goodsShipment)
		{
			foreach (var typeCode in DutyTaxFeeTypeCode)
			{
				var taxFee = GetAmountFromEntryPayInfos(typeCode.Item1);
				if (taxFee.HasValue)
				{
					var des = typeCode.Item1 == DutyTaxFeeCodeList.Codes.C10 ? (ZString)$"{typeCode.Item2}({entryInstruction.CEI_DaysOfDelayedDeclaration}日)" : typeCode.Item2;
					dutyList.Add(new Tuple<ZString, ZString, ZDecimal, ZBool>(des, typeCode.Item3, taxFee.Value, typeCode.Item4));
					if (dutyList.Count == 4)
					{
						break;
					}
				}
			}

			if (!dutyList.Any())
			{
				var taxFees = goodsShipment.DutyTaxFees.Where(x => x.AdValoremTaxBaseAmount > ZDecimal.Zero);
				foreach (var typeCode in DutyTaxFeeTypeCode)
				{
					var taxFee = taxFees.FirstOrDefault(x => x.TypeCode == typeCode.Item1);
					if (taxFee != null)
					{
						var des = taxFee.TypeCode == DutyTaxFeeCodeList.Codes.C10 ? (ZString)$"{typeCode.Item2}({entryInstruction.CEI_DaysOfDelayedDeclaration}日)" : typeCode.Item2;
						dutyList.Add(new Tuple<ZString, ZString, ZDecimal, ZBool>(des, typeCode.Item3, taxFee.AdValoremTaxBaseAmount, typeCode.Item4));
						if (dutyList.Count == 4)
						{
							break;
						}
					}
				}
			}
		}

		void SetSectionBody(List<IGovernmentAgencyGoodsItem> goodsItems)
		{
			var unitPriceAmountDeciamlPlace = GetDecimalPlaceForUnitPriceAmount(goodsItems);

			foreach (var item in goodsItems)
			{
				SetSectionGrouping(item);
				var textLines = GetTextLinesForSectionBody(item);
				GenerateSectionBody(item, textLines, unitPriceAmountDeciamlPlace);
			}

			CalculateTotal(goodsItems);
			FillUpFirstPage();
			SetPart1AndPart2();
			GenerateSectionsForPart2();
		}

		void FillUpFirstPage()
		{
			var currentRowCountToShow = Sections.Cast<ImportDeclarationSectionBodyWrapper>().Sum(c => c.RowCountToShow);
			for (int i = currentRowCountToShow; i < firstPageMaxSectionBodyRowCount; i++)
			{
				AddBlankSection();
			}
		}

		void SetPart1AndPart2()
		{
			var part1 = new List<ZString>();
			var part2 = new List<ZString>();
			var marksLines = DocumentWrapperHelper.SplitTextByWidth(MarksNumbers, MAX_LINE_WIDTH_FOR_PART1, sectionBodyFont);
			var containerLines = DocumentWrapperHelper.SplitTextByWidth(ContainerNumbers, MAX_LINE_WIDTH_FOR_PART1, sectionBodyFont);
			var otherInfoLines = DocumentWrapperHelper.SplitTextByWidth(OtherDeclarationInfo, MAX_LINE_WIDTH_FOR_PART1, sectionBodyFont);

			var marksCount = marksLines.SubRowTexts.Count;
			var containersCount = containerLines.SubRowTexts.Count;
			var othersCount = otherInfoLines.SubRowTexts.Count;
			var isSea = (Declaration?.TransportMode ?? string.Empty) == TransportTypeList.Codes.Sea;
			int marksLinesInPart1 = isSea ? MarksLinesInPart1ForSea : MarksLinesInPart1ForAir;
			int containersLinesInPart1 = isSea ? ContainerLinesInPart1ForSea : 0;
			var otherInfoLinesInPart1 = MaxLineCountForPart1 - marksLinesInPart1 - containersLinesInPart1 - 1;

			if (marksCount > marksLinesInPart1)
			{
				var marksNumbersSplitResult = DocumentWrapperHelper.SplitTextByWidth(MarksNumbers, MAX_LINE_WIDTH_FOR_PART1, sectionBodyFont, marksLinesInPart1 - 1);
				part1.AddRange(marksNumbersSplitResult.SubRowTexts);
				part1.Add(MarksPrintedAtEnd);

				part2.Add(ContinueMarksPrinting);
				part2.Add(marksNumbersSplitResult.RemainingText.Trim());
			}
			else
			{
				part1.AddRange(marksLines.SubRowTexts);
				for (int i = 0; i < marksLinesInPart1 - marksCount; i++)
				{
					part1.Add(ZString.Empty);
				}
			}

			if (isSea)
			{
				if (containersCount > containersLinesInPart1)
				{
					var containersSplitResult = DocumentWrapperHelper.SplitTextByWidth(ContainerNumbers, MAX_LINE_WIDTH_FOR_PART1, sectionBodyFont, containersLinesInPart1 - 1);
					part1.AddRange(containersSplitResult.SubRowTexts);
					part1.Add(ContainersPrintedAtEnd);

					part2.Add(ContinueContainersPrinting);
					part2.Add(containersSplitResult.RemainingText.Trim());
				}
				else
				{
					part1.AddRange(containerLines.SubRowTexts);
					for (int i = 0; i < containersLinesInPart1 - containersCount; i++)
					{
						part1.Add(ZString.Empty);
					}
				}
			}

			if (othersCount > otherInfoLinesInPart1)
			{
				var containersSplitResult = DocumentWrapperHelper.SplitTextByWidth(OtherDeclarationInfo, MAX_LINE_WIDTH_FOR_PART1, sectionBodyFont, otherInfoLinesInPart1 - 1);
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
			Part2.ForEach(c =>
			{
				var section = new ImportPart2SectionBodyWrapper() { OtherDeclarationsFront = c };
				Part2Sections.Add(section);
			});
		}

		void SetSectionGrouping(IGovernmentAgencyGoodsItem item)
		{
			var goodsItem = (NX5105GoodsShipment_GovernmentAgencyGoodsItem)item;
			var grouping = goodsItem.EntryLineGroupForDocument.Trim();
			if (!grouping.IsEmpty)
			{
				var groupingLines = DocumentWrapperHelper.SplitTextByLineBreak(grouping, MAX_CHAR_PER_LINE, true);
				for (var i = 0; i < groupingLines.Count; ++i)
				{
					if (i == 0)
					{
						AddBlankSection(false);
					}
					var section = new ImportDeclarationSectionBodyWrapper();
					section.Box35[0] = groupingLines[i];
					section.ShowInSameCellIfPossible = true;
					Sections.Add(section);
				}

				if (customizeSectionBodyRow == 0)
				{
					AddBlankSection();
				}
			}
		}

		void AddBlankSection(bool showInSameCellIfPossible = true)
		{
			var section = new ImportDeclarationSectionBodyWrapper();
			section.Box35[0] = ZString.Empty;
			section.ShowEvenRowIsEmpty = true;
			section.ShowInSameCellIfPossible = showInSameCellIfPossible;
			Sections.Add(section);
		}

		List<ZString> ProcessQuantityAndStatisticalData(IEnumerable<IGovernmentAgencyGoodsItem> governmentAgencyGoodsItems)
		{
			var tariffQuantityList = new List<ZString>();
			var tariffQuantityGroups = governmentAgencyGoodsItems.GroupBy(x => x.GoodsMeasure?.UnitCode ?? ZString.Empty);
			if (tariffQuantityGroups != null && tariffQuantityGroups.Any())
			{
				foreach (var group in tariffQuantityGroups)
				{
					if (!group.Key.IsEmpty)
					{
						tariffQuantityList.Add(FormatQuantityAndUnit(group.Sum(x => x.GoodsMeasure.TariffQuantity), group.Key));
					}
				}
			}

			var statisticsQualityGroups = governmentAgencyGoodsItems.GroupBy(x => x.GoodsStatisticalMeasure?.StatisticalUnitCode ?? ZString.Empty);
			if (statisticsQualityGroups != null && statisticsQualityGroups.Any())
			{
				foreach (var group in statisticsQualityGroups)
				{
					if (!group.Key.IsEmpty)
					{
						tariffQuantityList.Add(FormatQuantityAndUnit(group.Sum(x => x.GoodsStatisticalMeasure.TariffQuantity), group.Key, true));
					}
				}
			}
			return tariffQuantityList;
		}

		int TotalRowCount => Sections.Cast<ImportDeclarationSectionBodyWrapper>().Where(c => c.IsTotal).Sum(c => c.RowCountToShow);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not to be translated")]
		void CalculateTotal(List<IGovernmentAgencyGoodsItem> governmentAgencyGoodsItems)
		{
			var totalNetWeight = governmentAgencyGoodsItems.Sum(x => x.GoodsMeasure?.NetWeightMeasure ?? ZDecimal.Zero);
			var totalDutyPayingValue = governmentAgencyGoodsItems.Sum(x => x.Commodity?.DutyTaxFee?.AdValoremTaxBaseAmount ?? ZDecimal.Zero);
			var tariffQuantityList = ProcessQuantityAndStatisticalData(governmentAgencyGoodsItems);

			var entryLines = cusEntryHeader.MergedLines.Cast<CusEntryLine>();

			var totalRAPPrice = entryLines.Where(x => x.IsRAP).Sum(x => x.CL_Calc_RAPRORPriceLocalAmount.Round(0));
			bool isRAPPriceValid = totalRAPPrice != 0m;
			var totalRORPrice = entryLines.Where(x => x.IsROR).Sum(x => x.CL_Calc_RAPRORPriceLocalAmount.Round(0));
			bool isRORPriceValid = totalRORPrice != 0m;

			var section1 = new ImportDeclarationSectionBodyWrapper();
			Sections.Add(section1);
			section1.IsTotal = true;
			section1.AddPriceItem("----------");
			section1.AddPriceItem((NoResString)"Total:   ");
			section1.AddDutyPayingValueItem("--------------");
			var totalDutyPayingValueFormat = DocumentWrapperHelper.FormatNumber(new ZDecimal(totalDutyPayingValue));
			section1.AddDutyPayingValueItem(totalDutyPayingValueFormat);
			section1.AddDutyPayingValueItem(totalDutyPayingValueFormat.IsEmpty ? ZString.Empty : new ZString((NoResString)"vvvvvvvvvvvvvv"));
			section1.AddStatisticsQualityAndUnitItem("----------------");
			section1.AddStatisticsQualityAndUnitItem(string.Format(CultureInfo.InvariantCulture, "{0}{1}", DocumentWrapperHelper.FormatNumber(new ZDecimal(totalNetWeight), netWeightMeasureDecimalPlace), Constants.UnitOfQuantityCodes.Kilograms));

			foreach (var tariffQuantity in tariffQuantityList)
			{
				section1 = CheckIfNeedNewSection(section1);
				section1.AddStatisticsQualityAndUnitItem(tariffQuantity);
				section1.AddDutyPayingValueItem(ZString.Empty);
			}
			section1 = CheckIfNeedNewSection(section1);
			section1.AddStatisticsQualityAndUnitItem("vvvvvvvvvvvvvv");
			section1.AddDutyPayingValueItem(ZString.Empty);
			var rapTotalLocalPrice = isRAPPriceValid ? ZString.Format("({0} {1:###,###,###,###})", RAP, totalRAPPrice) : ZString.Empty;
			if (!rapTotalLocalPrice.IsEmpty)
			{
				section1 = CheckIfNeedNewSection(section1);
				section1.AddDutyPayingValueItem(rapTotalLocalPrice);
			}
			var rorTotalLocalPrice = isRORPriceValid ? ZString.Format("({0} {1:###,###,###,###})", ROR, totalRORPrice) : ZString.Empty;
			if (!rorTotalLocalPrice.IsEmpty)
			{
				section1 = CheckIfNeedNewSection(section1);
				section1.AddDutyPayingValueItem(rorTotalLocalPrice);
			}

			ImportDeclarationSectionBodyWrapper CheckIfNeedNewSection(ImportDeclarationSectionBodyWrapper section)
			{
				if (section.IsNeedBreakAndNew)
				{
					section = new ImportDeclarationSectionBodyWrapper();
					section.IsTotal = true;
					Sections.Add(section);
				}
				return section;
			}
		}

		List<ZString> GetTextLinesForSectionBody(IGovernmentAgencyGoodsItem item)
		{
			var resultLines = new List<ZString>();
			var originCountryCode = item.Origin.CountryCode;
			var country = originCountryCode.IsEmpty ? "" : $"{RefCountry.LoadFromCountryCode(new BusinessObjectFactory(), originCountryCode)?.RN_Desc ?? ZString.Empty}-{originCountryCode}";
			resultLines.Add(country);
			if (jobDeclarationDocumentAddressConfig != null)
			{
				var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().OrderBy(c => c.Position);
				var commodity = item.Commodity;
				var commodityNumbers = commodity.CommodityNumbers;
				var itemAdditionalDocuments = item.AdditionalDocuments.ToArray();
				foreach (var goodsDescriptionConfig in goodsDescriptionConfigs)
				{
					var description = ZString.Empty;
					var field = goodsDescriptionConfig.Field;
					var isMultipleData = false;
					switch (field)
					{
						case ImportDeclarationDocumentFieldList.Codes.OwnerPartNumber:
							description = commodityNumbers.FirstOrDefault(x => x.IdentifierTypeCode == MessageConstants.IdentificationTypeCodes.BP)?.ID ?? ZString.Empty;
							break;
						case ImportDeclarationDocumentFieldList.Codes.SupplierPartNumber:
							description = commodityNumbers.FirstOrDefault(x => x.IdentifierTypeCode == MessageConstants.IdentificationTypeCodes.SA)?.ID ?? ZString.Empty;
							break;
						case ImportDeclarationDocumentFieldList.Codes.GoodsDescription:
							description = (item is NX5105GoodsShipment_GovernmentAgencyGoodsItem itemBO) ? itemBO.CommodityDescriptionForDocument : commodity.Description;
							break;
						case ImportDeclarationDocumentFieldList.Codes.Brand:
							description = commodity.Name;
							break;
						case ImportDeclarationDocumentFieldList.Codes.Model:
							description = commodity.CommercialCategorizationID;
							break;
						case ImportDeclarationDocumentFieldList.Codes.Specification:
							description = commodity.Constituent?.ElementDescription ?? ZString.Empty;
							break;
						case ImportDeclarationDocumentFieldList.Codes.PreviousBondedEntryNumber:
							var itemPreBondedDocument = item.PreBondedDocument;
							if (itemPreBondedDocument != null)
							{
								description = DocumentWrapperHelper.FormatAdditionalDocument(itemPreBondedDocument.ID, itemPreBondedDocument.LineNumeric);
							}
							break;
						case ImportDeclarationDocumentFieldList.Codes.PreviousEntryNumber:
							var itemPreviousDocument = item.PreviousDocument;
							if (itemPreviousDocument != null)
							{
								description = DocumentWrapperHelper.FormatAdditionalDocument(itemPreviousDocument.ID, itemPreviousDocument.LineNumeric);
							}
							break;
						case ImportDeclarationDocumentFieldList.Codes.Permits:
							var itemAdditionalDocumentsLines = GetItemAdditionalDocumentsLines(itemAdditionalDocuments);
							isMultipleData = itemAdditionalDocumentsLines.Count() > 1;
							description = ZString.Join("\r\n", itemAdditionalDocumentsLines.ToArray());
							break;
						case ImportDeclarationDocumentFieldList.Codes.CertificateOfOrigin:
							description = GetOriginDocumentLine(itemAdditionalDocuments, item.Origin.AdditionalDocument);
							break;
						case ImportDeclarationDocumentFieldList.Codes.SHTCImportPermit:
							description = commodity.SHTCImportPermitID;
							break;
						case ImportDeclarationDocumentFieldList.Codes.CITESImportPermit:
							description = commodity.CITESImportPermitID;
							break;
						case ImportDeclarationDocumentFieldList.Codes.AssignedNumbers:
							var itemCommodityAdditionalDocumentsLines = GetItemCommodityAdditionalDocumentsLines(commodity.AdditionalDocuments);
							isMultipleData = itemCommodityAdditionalDocumentsLines.Count() > 1;
							description = ZString.Join("\r\n", itemCommodityAdditionalDocumentsLines.ToArray());
							break;
						default:
							break;
					}
					if (!description.IsEmpty)
					{
						var numberOfLinesShortenedForTrademark = ((NX5105GoodsShipment_GovernmentAgencyGoodsItem)item).InvoiceLine.TrademarkImage != null ? Math.Max(0, 4 - resultLines.Count) : 0;
						var maxLineWidth = numberOfLinesShortenedForTrademark > 0 ? MAX_CHAR_PER_LINE_WITH_TRADEMARK : MAX_CHAR_PER_LINE;
						var goodsDescriptionConfigCaption = goodsDescriptionConfig.Caption;
						var captionAndDescription = string.Format("{0}{1}", goodsDescriptionConfigCaption, description);
						if (isMultipleData || DocumentWrapperHelper.IsTextExceedsMaxChar(captionAndDescription, maxLineWidth))
						{
							SplitTextByWidthAndAddToListForSectionBody(string.Format("{0}\r\n{1}", goodsDescriptionConfigCaption, description), resultLines, numberOfLinesShortenedForTrademark);
						}
						else
						{
							SplitTextByWidthAndAddToListForSectionBody(captionAndDescription, resultLines, numberOfLinesShortenedForTrademark);
						}
					}
				}
			}
			return resultLines;
		}

		void SplitTextByWidthAndAddToListForSectionBody(ZString text, List<ZString> listToAdd, int numberOfLinesShortenedForTrademark)
		{
			var textToProcess = text.Trim();
			if (numberOfLinesShortenedForTrademark > 0)
			{
				var splitResultWithTradeMark = DocumentWrapperHelper.SplitTextByLineBreak(textToProcess, MAX_CHAR_PER_LINE_WITH_TRADEMARK, true);
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
			var splitResult = DocumentWrapperHelper.SplitTextByLineBreak(textToProcess, MAX_CHAR_PER_LINE, true);
			listToAdd.AddRange(splitResult);
		}

		int GetDecimalPlaceForUnitPriceAmount(List<IGovernmentAgencyGoodsItem> goodItems)
		{
			var lines = goodItems.Select(x => x?.Commodity?.InvoiceLine).Where(x => x != null);
			return Math.Min(lines.Any() ? lines.Max(x => x.UnitPriceAmount.DecimalPlaces) : 0, 6);
		}

		IEnumerable<ZString> GetItemAdditionalDocumentsLines(IAdditionalDocument[] permitNumbers)
		{
			var thirdPermitNumberId = permitNumbers.Length > 2 ? permitNumbers[2].ID : ZString.Empty;
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

		ZString GetOriginDocumentLine(IAdditionalDocument[] permitNumbers, IAdditionalDocument originDocument)
		{
			var result = ZString.Empty;
			var originDocumentID = originDocument?.ID ?? ZString.Empty;
			if (!originDocumentID.IsEmpty)
			{
				var firstPermitNumberValid = (!permitNumbers.FirstOrDefault()?.ID.IsEmpty) ?? false;
				var secondPermitNumberValid = (!permitNumbers.ElementAtOrDefault(1)?.ID.IsEmpty) ?? false;
				if (firstPermitNumberValid && secondPermitNumberValid)
				{
					result = DocumentWrapperHelper.FormatAdditionalDocument(originDocument.ID, originDocument.SequenceNumeric);
				}
			}
			return result;
		}

		IEnumerable<ZString> GetItemCommodityAdditionalDocumentsLines(IEnumerable<IAdditionalDocument> additionalDocuments)
		{
			if (additionalDocuments != null)
			{
				int docNum = Math.Min(additionalDocuments.Count(), 10);
				for (int i = 1; i < docNum; i++)
				{
					var id = additionalDocuments.ElementAt(i).ID;
					if (!id.IsEmpty)
					{
						yield return id;
					}
				}
			}
		}

		void ProcessDocuments(IEnumerable<IAdditionalDocument> documents, IOrigin origin, ICommodity commodity, ImportDeclarationSectionBodyWrapper section)
		{
			var additionalDocuments = documents.ToList();
			additionalDocuments.Add(origin?.AdditionalDocument);

			var bondedNoteCode = commodity?.BondedNoteCode ?? ZString.Empty;
			var firstCommodityDocumentId = commodity?.AdditionalDocuments?.FirstOrDefault()?.ID ?? ZString.Empty;
			if (!firstCommodityDocumentId.IsEmpty && !bondedNoteCode.IsEmpty)
			{
				section.Box38AssignedNumber = ZString.Format("{0}/{1}", bondedNoteCode, firstCommodityDocumentId);
			}
			else if (!firstCommodityDocumentId.IsEmpty)
			{
				section.Box38AssignedNumber = firstCommodityDocumentId;
			}
			else if (!bondedNoteCode.IsEmpty)
			{
				section.Box38AssignedNumber = bondedNoteCode;
			}

			var firstLine = DocumentWrapperHelper.GetFormatAdditionalDocument(additionalDocuments.FirstOrDefault());
			if (firstLine.IsEmpty)
			{
				firstLine = Constants.NIL;
			}
			var secondLine = DocumentWrapperHelper.GetFormatAdditionalDocument(additionalDocuments.ElementAtOrDefault(1));

			section.Box37ImportPermitNumberAndItemNumber_Line1 = firstLine;
			section.Box37ImportPermitNumberAndItemNumber_Line2 = secondLine;
		}

		void ProcessCommodity(ICommodity commodity, ImportDeclarationSectionBodyWrapper section, int decimalPlaceForUnitPriceAmount)
		{
			var classificationId = commodity.Classifications?.FirstOrDefault(x => x.IdentificationTypeCode == MessageConstants.IdentificationTypeCodes.HS)?.ID ?? ZString.Empty;
			section.Box38CCCCode = Regex.Replace(classificationId, @"^(.{4})(.{2})(.{2})(.{2})(.{1})$", "$1.$2.$3.$4-$5");

			var currency = commodity.InvoiceLine?.CurrencyTypeCode ?? ZString.Empty;
			section.IsNotPrintEmpty = true;
			section.AddPriceItem(ZString.Format("{0} {1}", TradeTermsConditionCode, currency));
			section.TermOfTradeCurrency = currency;
			section.AddPriceItem(GetDecimalToString(commodity.InvoiceLine?.UnitPriceAmount ?? ZDecimal.Zero, decimalPlaceForUnitPriceAmount));

			section.Box44Ad_ValoremDutyRate = GetDecimalToPercentString(commodity.DutyTaxFeeAmount?.TaxRateNumeric * 100M ?? ZDecimal.Zero);
			SetRateOfCommodityTaxAnfDutyPayment(commodity, section);

			SetDetailDutyPayingValues(commodity, section);

			section.AddPriceItem(ZString.Empty);
			var entryLine = (commodity as NX5105GovernmentAgencyGoodsItem_Commodity)?.EntryLine;
			if (entryLine != null)
			{
				var repairOrRentalRoyaltyString = ZString.Empty;
				if (entryLine.IsRAPOrROR)
				{
					var raprorPrice = entryLine.CL_Calc_RAPRORUnitPrice;
					var isRAP = entryLine.IsRAP;
					if (isRAP || !raprorPrice.IsEmpty)
					{
						repairOrRentalRoyaltyString = ZString.Format("({0} {1} {2:###,###,###,##0.######})", isRAP ? RAP : ROR, entryLine.CL_Calc_RAPRORUnitCurr, raprorPrice);
					}
				}
				section.AddPriceItem(repairOrRentalRoyaltyString);
			}
		}

		void ProcessSecondaryTariffRate(JobComInvoiceLine invoiceLine, ICommodity commodity, ImportDeclarationSectionBodyWrapper section)
		{
			var dutyTaxRate = GetDecimalUnitCodeToString(commodity?.DutyTaxFeeQuantity?.TaxRateNumeric ?? ZDecimal.Zero, commodity.DutyTaxFeeQuantity?.DutyUnitCode ?? ZString.Empty);
			var invoiceLineTax = invoiceLine.Taxes.Cast<JobComInvoiceLineTax>();
			var ttTariffDetail = invoiceLineTax.FirstOrDefault(c => c.JLT_Type == Constants.UniversalReferenceConstants.RefCusRateTypes.TT);
			var atTariffDetail = invoiceLineTax.FirstOrDefault(c => c.JLT_Type == Constants.UniversalReferenceConstants.RefCusRateTypes.AT);
			if (!string.IsNullOrEmpty(dutyTaxRate))
			{
				section.Box44SpecificDutyRate = dutyTaxRate;
			}
			else if (ttTariffDetail != null)
			{
				var rates = ttTariffDetail.UniversalTariff?.Rates;
				var tatRateFormula = rates?.FirstOrDefault(c => c.RateCode == UniversalReferenceConstants.RefCusRateCodes.TAT)?.ZZ2_RateFormula ?? ZString.Empty;
				var hwsRateFormula = rates?.FirstOrDefault(c => c.RateCode == UniversalReferenceConstants.RefCusRateCodes.HWS)?.ZZ2_RateFormula ?? ZString.Empty;
				section.Box44SpecificDutyRate = Regex.Replace(tatRateFormula, @"(\d*) \* \[(\D*)\]\D*", "$1/$2");
				section.Box44OtherDutyRate = Regex.Replace(hwsRateFormula, @"(\d*) \* \[(\D*)\]\D*", "$1/$2");
			}
			else if (atTariffDetail != null)
			{
				var formula = ZString.Empty;
				if (invoiceLine.CusEntryLine is CusEntryLine entryLine && Declaration is JobDeclaration declaration)
				{
					var rate = atTariffDetail.UniversalTariff?.Rates?.FirstOrDefault(c => c.RateCode == UniversalReferenceConstants.RefCusRateCodes.TAT);
					if (rate != null)
					{
						var entryLineData = new EntryLineUniversalRate(entryLine);
						var strategy = new DutyCalculatorStrategy(declaration);
						var dutyCalculationIntermediateResult = strategy.CalculateDutyAmount(entryLine, entryLineData, rate.ZZ2_RateFormula, UniversalReferenceConstants.RefCusRateCodes.TAT, rate.ZZ2_RateFormulaDerivedFrom);
						formula = $"{dutyCalculationIntermediateResult.Rate}/{dutyCalculationIntermediateResult.UnitOfCalculation}";
					}
				}
				if (formula.IsEmpty)
				{
					var atRateFormula = atTariffDetail.FormattedTariffRate;
					formula = Regex.Replace(atRateFormula, @"(\d*) \* \[(\D*)\]\D*", "$1/$2");
				}

				section.Box44SpecificDutyRate = formula;
			}
		}

		void SetDetailDutyPayingValues(ICommodity commodity, ImportDeclarationSectionBodyWrapper section)
		{
			var customsValue = commodity.DutyTaxFee?.AdValoremTaxBaseAmount ?? ZDecimal.Zero;
			var customsValueFormatString = customsValue.IsEmpty ? ZString.Empty : ZString.Format("{0:###,###,###,###}", customsValue);
			section.AddDutyPayingValueItem(customsValueFormatString);
			section.AddDutyPayingValueItem(GetDecimalToString(commodity.DutyTaxFee?.SpecificTaxBaseQuantity ?? ZDecimal.Zero, "{0:###,##0.##}"));
			section.AddDutyPayingValueItem(ZString.Empty);
			var entryLine = (commodity as NX5105GovernmentAgencyGoodsItem_Commodity)?.EntryLine;
			if (entryLine != null)
			{
				var repairOrRentalRoyaltyString = ZString.Empty;
				if (entryLine.IsRAPOrROR)
				{
					var raprorPrice = entryLine.CL_Calc_RAPRORPriceLocalAmount.Round(0);
					var isRAP = entryLine.IsRAP;
					if (isRAP || !raprorPrice.IsEmpty)
					{
						repairOrRentalRoyaltyString = ZString.Format("({0} {1:###,###,###,##0})", isRAP ? RAP : ROR, raprorPrice);
					}
				}
				section.AddDutyPayingValueItem(repairOrRentalRoyaltyString);
			}
		}

		static void SetRateOfCommodityTaxAnfDutyPayment(ICommodity commodity, ImportDeclarationSectionBodyWrapper section)
		{
			section.Box45CodeOfDutyPayment = commodity.GovernmentProcedure?.CurrentCode ?? ZString.Empty;

			var commodityDutyOtherTaxFee = commodity.DutyOtherTaxFees.FirstOrDefault(x => x.TypeCode == DutyTaxFeeCodeList.Codes.B10 || x.TypeCode == DutyTaxFeeCodeList.Codes.B19);
			var commoditySpecificallyTaxFee = commodity.DutyOtherTaxFees.FirstOrDefault(x => x.TypeCode == DutyTaxFeeCodeList.Codes.B60 || x.TypeCode == DutyTaxFeeCodeList.Codes.B89);
			if (commodityDutyOtherTaxFee != null)
			{
				var methodcode = commodityDutyOtherTaxFee.MethodCode;
				if (methodcode == MethodCodes._1)
				{
					section.Box46CommodityTaxRate_Line2 = GetDecimalToPercentString(commodityDutyOtherTaxFee.TaxRateNumeric * 100m);
				}
				else if (methodcode == MethodCodes._2)
				{
					section.Box46CommodityTaxRate_Line2 = GetDecimalUnitCodeToString(commodityDutyOtherTaxFee.TaxRateNumeric, commodityDutyOtherTaxFee.MethodOfCalculation);
				}
			}

			if (commoditySpecificallyTaxFee != null)
			{
				section.Box46CommodityTaxRate_Line1 = GetDecimalToPercentString(commoditySpecificallyTaxFee.TaxRateNumeric * 100m);
			}
		}

		static string GetDecimalToPercentString(ZDecimal amount)
		{
			return ZString.Format("{0}%", amount.ToStringTrimZeros());
		}

		static string GetDecimalToString(ZDecimal amount, string format = "", string defaultValue = "")
		{
			return amount == ZDecimal.Zero ? new ZString(defaultValue) : (string.IsNullOrEmpty(format) ? new ZString(amount.ToString()) : ZString.Format(format, amount));
		}

		static string GetDecimalToString(ZDecimal amount, int decimalPlace, string defaultValue = "")
		{
			return amount == ZDecimal.Zero ? new ZString(defaultValue) : ZString.Format("{0:###,##0." + new ZString('0', decimalPlace) + "}", amount);
		}

		static string GetDecimalUnitCodeToString(ZDecimal amount, ZString unitCode)
		{
			return amount == ZDecimal.Zero ? ZString.Empty : ZString.Format("{0:######.##}/{1}", amount, unitCode);
		}

		void GenerateSectionBody(IGovernmentAgencyGoodsItem item, List<ZString> textLines, int decimalPlaceForUnitPriceAmount)
		{
			var additionalDocuments = item.AdditionalDocuments;
			var commodity = item.Commodity;
			var section = new ImportDeclarationSectionBodyWrapper();
			var invoiceLine = ((NX5105GoodsShipment_GovernmentAgencyGoodsItem)item).InvoiceLine;
			section.TrademarkImage = invoiceLine.TrademarkImage;
			for (var i = 0; i < textLines.Count; i++)
			{
				if (customizeSectionBodyRow > 0 && i > 3)
				{
					section.Box35[0] = textLines[i];
					section.ShowInSameCellIfPossible = true;
					Sections.Add(section);
					section = new ImportDeclarationSectionBodyWrapper();
					continue;
				}

				var lineIndex = i % 4;
				if (i != 0)
				{
					section.Box35[lineIndex] = textLines[i];
				}
				else
				{
					section.Box34ItemNumber = item.SequenceNumeric == ZInt.Zero ? string.Empty : item.SequenceNumeric.ToString();
					section.CountryCode = textLines[i];

					if (additionalDocuments != null)
					{
						ProcessDocuments(additionalDocuments, item.Origin, commodity, section);
					}

					if (commodity != null)
					{
						ProcessCommodity(commodity, section, decimalPlaceForUnitPriceAmount);
					}

					ProcessSecondaryTariffRate(invoiceLine, commodity, section);

					var goodsMeasure = item.GoodsMeasure;
					var unitCode = goodsMeasure?.UnitCode ?? ZString.Empty;
					var netWeightFormat = goodsMeasure != null ? (ZString)string.Format(CultureInfo.InvariantCulture, "{0}{1}", DocumentWrapperHelper.FormatNumber(goodsMeasure.NetWeightMeasure, netWeightMeasureDecimalPlace), Constants.UnitOfQuantityCodes.Kilograms) : ZString.Empty;
					var qualityAndUnitFormat = !unitCode.IsEmpty ? (ZString)string.Format(CultureInfo.InvariantCulture, "{0}{1}", DocumentWrapperHelper.FormatNumber(goodsMeasure.TariffQuantity, tariffQuantityDecimalPlace), unitCode) : ZString.Empty;
					unitCode = item.GoodsStatisticalMeasure?.StatisticalUnitCode ?? ZString.Empty;
					var statisticsQualityAndUnitFormat = !unitCode.IsEmpty ? (ZString)string.Format(CultureInfo.InvariantCulture, "({0}{1})", DocumentWrapperHelper.FormatNumber(item.GoodsStatisticalMeasure.TariffQuantity, statisticalQuantityDecimalPlace), unitCode) : ZString.Empty;
					section.AddStatisticsQualityAndUnitItem(netWeightFormat);
					section.AddStatisticsQualityAndUnitItem(qualityAndUnitFormat);
					section.AddStatisticsQualityAndUnitItem(statisticsQualityAndUnitFormat);
				}

				if (lineIndex == 3 || i == textLines.Count - 1)
				{
					Sections.Add(section);
					section = new ImportDeclarationSectionBodyWrapper();
				}
			}
		}

		ZString FormatQuantityAndUnit(ZDecimal quantity, ZString unitCode, bool isStatistical = false)
		{
			var result = string.Format(CultureInfo.InvariantCulture, "{0}{1}", DocumentWrapperHelper.FormatQuantityNumber(quantity), unitCode);
			if (isStatistical)
			{
				result = ZString.Format("({0})", result);
			}
			return result;
		}

		#region Properties
		readonly INX5105Declaration fINX5105Declaration;
		readonly CusEntryHeader cusEntryHeader;
		readonly CusEntryInstruction entryInstruction;
		readonly List<Tuple<ZString, ZString, ZDecimal, ZBool>> dutyList;

		public ZString DeclDocTypeCodeAndDescription => Declaration?.DeclDocTypeCodeAndDescription.InsertSafe(7, "\r\n") ?? ZString.Empty;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Type Codes")]
		static List<Tuple<ZString, ZString, ZString, ZBool>> DutyTaxFeeTypeCode
		{
			get
			{
				return dutyTaxFeeTypeCode ?? (dutyTaxFeeTypeCode = new List<Tuple<ZString, ZString, ZString, ZBool>>()
				{
					{ new Tuple<ZString, ZString, ZString, ZBool>(DutyTaxFeeCodeList.Codes.B10, DutyTaxFeeCodeList.Descriptions.B10, "Commodity Tax", ZBool.False ) },
					{ new Tuple<ZString, ZString, ZString, ZBool>(DutyTaxFeeCodeList.Codes.B19, DutyTaxFeeCodeList.Descriptions.B19, "Commodity Tax", ZBool.True ) },
					{ new Tuple<ZString, ZString, ZString, ZBool>(DutyTaxFeeCodeList.Codes.B40, DutyTaxFeeCodeList.Descriptions.B40, "Business tax", ZBool.False ) },
					{ new Tuple<ZString, ZString, ZString, ZBool>(DutyTaxFeeCodeList.Codes.B49, DutyTaxFeeCodeList.Descriptions.B49, "Business tax", ZBool.True ) },
					{ new Tuple<ZString, ZString, ZString, ZBool>(DutyTaxFeeCodeList.Codes.B60, DutyTaxFeeCodeList.Descriptions.B60, "SSG", ZBool.False) },
					{ new Tuple<ZString, ZString, ZString, ZBool>(DutyTaxFeeCodeList.Codes.B89, DutyTaxFeeCodeList.Descriptions.B89, "SSG", ZBool.True ) },
					{ new Tuple<ZString, ZString, ZString, ZBool>(DutyTaxFeeCodeList.Codes.B31, DutyTaxFeeCodeList.Descriptions.B31, "Tobacco and Alcohol Tax", ZBool.False) },
					{ new Tuple<ZString, ZString, ZString, ZBool>(DutyTaxFeeCodeList.Codes.B69, DutyTaxFeeCodeList.Descriptions.B69, "Tobacco and Alcohol Tax", ZBool.True ) },
					{ new Tuple<ZString, ZString, ZString, ZBool>(DutyTaxFeeCodeList.Codes.B32, DutyTaxFeeCodeList.Descriptions.B32, "Health and Welfare Surcharge", ZBool.False ) },
					{ new Tuple<ZString, ZString, ZString, ZBool>(DutyTaxFeeCodeList.Codes.B79, DutyTaxFeeCodeList.Descriptions.B79, "Health and Welfare Surcharge", ZBool.True ) },
					{ new Tuple<ZString, ZString, ZString, ZBool>(DutyTaxFeeCodeList.Codes.A20, DutyTaxFeeCodeList.Descriptions.A20, "Countervailing Duty", ZBool.False) },
					{ new Tuple<ZString, ZString, ZString, ZBool>(DutyTaxFeeCodeList.Codes.A30, DutyTaxFeeCodeList.Descriptions.A30, "Anti-Dumping Duty", ZBool.False) },
					{ new Tuple<ZString, ZString, ZString, ZBool>(DutyTaxFeeCodeList.Codes.A40, DutyTaxFeeCodeList.Descriptions.A40, "Retaliatory Duty", ZBool.False) },
					{ new Tuple<ZString, ZString, ZString, ZBool>(DutyTaxFeeCodeList.Codes.C10, DutyTaxFeeCodeList.Descriptions.C10, "Late Declaration Fee", ZBool.False) },
					{ new Tuple<ZString, ZString, ZString, ZBool>(DutyTaxFeeCodeList.Codes.A50, DutyTaxFeeCodeList.Descriptions.A50, "Additional Duty", ZBool.False) },
				});
			}
		}

		[ThreadStatic]
		static List<Tuple<ZString, ZString, ZString, ZBool>> dutyTaxFeeTypeCode;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not to be translated")]
		const string Sea = "海運";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not to be translated")]
		const string Air = "空運";

		public BusinessObjectCollectionWrapper<ImportPart2SectionBodyWrapper> Part2Sections { get; }

		public BusinessObjectCollectionWrapper<ImportDeclarationSectionBodyWrapper> Sections { get; }

		public BusinessObjectCollectionWrapper<ImportDeclarationSectionBodyWrapper> FirstPageGoodsItemListSections
		{
			get
			{
				if (firstPageGoodsItemListSections == null)
				{
					firstPageGoodsItemListSections = new BusinessObjectCollectionWrapper<ImportDeclarationSectionBodyWrapper>();
					var goodsItemListSections = Sections;
					var remainingPageRowCount = firstPageMaxSectionBodyRowCount;
					var cellRowUsed = 0;
					foreach (ImportDeclarationSectionBodyWrapper goodsItem in goodsItemListSections)
					{
						var goodsItemRowCountToShow = goodsItem.RowCountToShow;
						if (goodsItem.IsTotal && remainingPageRowCount < TotalRowCount)
						{
							FillUpWithEmptyRows(firstPageGoodsItemListSections, remainingPageRowCount);
							break;
						}
						else if (remainingPageRowCount >= goodsItemRowCountToShow)
						{
							if (customizeSectionBodyRow > 0 && !goodsItem.IsTotal)
							{
								if (remainingPageRowCount < customizeSectionBodyRow)
								{
									if (goodsItem.ShouldShowInSameCellIfPossible && (cellRowUsed + goodsItemRowCountToShow) <= customizeSectionBodyRow)
									{
										firstPageGoodsItemListSections.Add(goodsItem);
										cellRowUsed += goodsItemRowCountToShow;
										remainingPageRowCount -= goodsItemRowCountToShow;
										continue;
									}
									else
									{
										FillUpWithEmptyRows(firstPageGoodsItemListSections, remainingPageRowCount);
										break;
									}
								}

								if (firstPageGoodsItemListSections.Count > 0)
								{
									if (goodsItem.ShouldShowInSameCellIfPossible && (cellRowUsed + goodsItemRowCountToShow) <= customizeSectionBodyRow)
									{
										firstPageGoodsItemListSections.Add(goodsItem);
										cellRowUsed += goodsItemRowCountToShow;
									}
									else
									{
										var emptyRowsCount = customizeSectionBodyRow - cellRowUsed;
										FillUpWithEmptyRows(firstPageGoodsItemListSections, emptyRowsCount);
										remainingPageRowCount -= emptyRowsCount;

										if (remainingPageRowCount >= customizeSectionBodyRow)
										{
											FillUpWithDashMiniRow(firstPageGoodsItemListSections);
											firstPageGoodsItemListSections.Add(goodsItem);
											cellRowUsed = goodsItemRowCountToShow;
										}
										else
										{
											FillUpWithEmptyRows(firstPageGoodsItemListSections, remainingPageRowCount);
											break;
										}
									}
								}
								else
								{
									firstPageGoodsItemListSections.Add(goodsItem);
									cellRowUsed = goodsItemRowCountToShow;
								}
							}
							else
							{
								firstPageGoodsItemListSections.Add(goodsItem);
							}
						}
						remainingPageRowCount -= goodsItemRowCountToShow;
					}
				}
				return firstPageGoodsItemListSections;
			}
		}
		BusinessObjectCollectionWrapper<ImportDeclarationSectionBodyWrapper> firstPageGoodsItemListSections;

		public BusinessObjectCollectionWrapper<ImportDeclarationSectionBodyWrapper> OtherPageGoodsItemListSections
		{
			get
			{
				if (otherPageGoodsItemListSections == null)
				{
					otherPageGoodsItemListSections = new BusinessObjectCollectionWrapper<ImportDeclarationSectionBodyWrapper>();
					var goodsItemListSections = Sections.Cast<ImportDeclarationSectionBodyWrapper>();
					var firstPageGoodsItemListSections = FirstPageGoodsItemListSections.Cast<ImportDeclarationSectionBodyWrapper>();
					if (firstPageGoodsItemListSections.Sum(c => c.RowCountToShow) < goodsItemListSections.Sum(c => c.RowCountToShow) || customizeSectionBodyRow > 0)
					{
						var pageGoodsItemListSections = new BusinessObjectCollectionWrapper<ImportDeclarationSectionBodyWrapper>();
						pageGoodsItemListSections.AddRange(goodsItemListSections);
						pageGoodsItemListSections.RemoveRange(firstPageGoodsItemListSections);
						var remainingPageRowCount = (customizeSectionBodyRow > 0 && customizeSectionBodyRow <= 5) ? otherPageMaxSectionBodyRowCount - 1 : otherPageMaxSectionBodyRowCount;
						var remainingTotalRowCount = TotalRowCount;
						var cellRowUsed = 0;
						foreach (ImportDeclarationSectionBodyWrapper goodsItem in pageGoodsItemListSections)
						{
							var goodsItemRowCountToShow = goodsItem.RowCountToShow;
							var isTotal = goodsItem.IsTotal;
							if (isTotal)
							{
								if (remainingPageRowCount < remainingTotalRowCount)
								{
									FillUpWithEmptyRows(otherPageGoodsItemListSections, remainingPageRowCount);
									remainingPageRowCount = otherPageMaxSectionBodyRowCount;
								}
								otherPageGoodsItemListSections.Add(goodsItem);
								remainingTotalRowCount -= goodsItemRowCountToShow;
							}
							else if (remainingPageRowCount >= goodsItemRowCountToShow)
							{
								if (customizeSectionBodyRow > 0)
								{
									var otherPageMaxSectionBodyRowCountForcustomizeSectionBodyRow = customizeSectionBodyRow <= 5 ? otherPageMaxSectionBodyRowCount - 1 : otherPageMaxSectionBodyRowCount;

									if (remainingPageRowCount < customizeSectionBodyRow)
									{
										if (goodsItem.ShouldShowInSameCellIfPossible && (cellRowUsed + goodsItemRowCountToShow) <= customizeSectionBodyRow)
										{
											otherPageGoodsItemListSections.Add(goodsItem);
											cellRowUsed += goodsItemRowCountToShow;
											remainingPageRowCount -= goodsItemRowCountToShow;
											continue;
										}
										else
										{
											FillUpWithEmptyRows(otherPageGoodsItemListSections, remainingPageRowCount);
											cellRowUsed = 0;
											remainingPageRowCount = otherPageMaxSectionBodyRowCountForcustomizeSectionBodyRow;
										}
									}

									if (remainingPageRowCount < otherPageMaxSectionBodyRowCountForcustomizeSectionBodyRow)
									{
										if (goodsItem.ShouldShowInSameCellIfPossible && (cellRowUsed + goodsItemRowCountToShow) <= customizeSectionBodyRow)
										{
											otherPageGoodsItemListSections.Add(goodsItem);
											cellRowUsed += goodsItemRowCountToShow;
											remainingPageRowCount -= goodsItemRowCountToShow;
										}
										else
										{
											var emptyRowsCount = customizeSectionBodyRow - cellRowUsed;
											FillUpWithEmptyRows(otherPageGoodsItemListSections, emptyRowsCount);
											remainingPageRowCount -= emptyRowsCount;

											if (remainingPageRowCount >= customizeSectionBodyRow)
											{
												FillUpWithDashMiniRow(otherPageGoodsItemListSections);
												otherPageGoodsItemListSections.Add(goodsItem);
												cellRowUsed = goodsItemRowCountToShow;
												remainingPageRowCount -= goodsItemRowCountToShow;
											}
											else
											{
												FillUpWithEmptyRows(otherPageGoodsItemListSections, remainingPageRowCount);
												otherPageGoodsItemListSections.Add(goodsItem);
												cellRowUsed = goodsItemRowCountToShow;
												remainingPageRowCount = otherPageMaxSectionBodyRowCountForcustomizeSectionBodyRow - goodsItemRowCountToShow;
											}
										}
									}
									else
									{
										otherPageGoodsItemListSections.Add(goodsItem);
										cellRowUsed = goodsItemRowCountToShow;
										remainingPageRowCount -= goodsItemRowCountToShow;
									}
								}
								else
								{
									otherPageGoodsItemListSections.Add(goodsItem);
									remainingPageRowCount -= goodsItemRowCountToShow;
								}
							}
							else
							{
								FillUpWithEmptyRows(otherPageGoodsItemListSections, remainingPageRowCount);
								remainingPageRowCount = otherPageMaxSectionBodyRowCount;
								otherPageGoodsItemListSections.Add(goodsItem);
								cellRowUsed = goodsItemRowCountToShow;
								remainingPageRowCount -= goodsItemRowCountToShow;
							}
						}
					}
				}
				return otherPageGoodsItemListSections;
			}
		}
		BusinessObjectCollectionWrapper<ImportDeclarationSectionBodyWrapper> otherPageGoodsItemListSections;

		void FillUpWithEmptyRows(BusinessObjectCollectionWrapper<ImportDeclarationSectionBodyWrapper> pageGoodsItemListSections, int rowCount)
		{
			for (var i = 0; i < rowCount; i++)
			{
				pageGoodsItemListSections.Add(new ImportDeclarationSectionBodyWrapper() { ShowEvenRowIsEmpty = true });
			}
		}

		void FillUpWithDashMiniRow(BusinessObjectCollectionWrapper<ImportDeclarationSectionBodyWrapper> pageGoodsItemListSections)
		{
			pageGoodsItemListSections.Add(new ImportDeclarationSectionBodyWrapper() { ShowDashLine = true });
		}

		public ZString CategoriesOfTransport
		{
			get
			{
				var result = ZString.Empty;
				var typeCode = fINX5105Declaration?.BorderTransportMeans?.TypeCode ?? ZString.Empty;
				if (typeCode == MessageConstants.BorderTransportMeansTypeCodes._1)
				{
					result = Sea;
				}
				else if (typeCode == MessageConstants.BorderTransportMeansTypeCodes._4)
				{
					result = Air;
				}
				return result;
			}
		}

		#region TypeOfDeclaration
		public ZString TypeOfDeclaration => fINX5105Declaration?.TypeCode ?? ZString.Empty;

		public ZString DeclarationTypeDescription => entryInstruction?.CEI_StyleDescription.InsertSafe(9, "\r\n") ?? ZString.Empty;
		#endregion

		public ZString DeclarationNo => CommonHelper.RemoveEntryNumberPlaceHolder(fINX5105Declaration?.ID ?? ZString.Empty);

		public ZString DeclarationNoFormatted => DocumentWrapperHelper.GetDeclarationIDFormat(DeclarationNo);

		public ZString DeclarationNoBarCode => DocumentWrapperHelper.FormatBarCode(DeclarationNo);

		public ZString Registration => fINX5105Declaration?.GoodsShipment?.Consignment?.BorderTransportMeans?.Registration ?? ZString.Empty;

		public ZString ManifestSerialNumber => fINX5105Declaration?.GoodsShipment?.Consignment?.ManifestSerialNumber ?? ZString.Empty;

		JobDeclaration Declaration => cusEntryHeader.Declaration;

		public ZString NameOfVesselOrTransportId
		{
			get
			{
				var transportModeCode = Declaration?.TransportMode ?? ZString.Empty;
				var result = ZString.Empty;
				switch (transportModeCode)
				{
					case TransportTypeList.Codes.Sea:
						result = Declaration.JE_VesselName;
						break;
					case TransportTypeList.Codes.Air:
						result = Declaration.JE_VoyageFlightNo;
						break;
				}
				return result;
			}
		}

		public ZString CodeOfVessel => CategoriesOfTransport == Air ? ZString.Empty : (fINX5105Declaration?.GoodsShipment?.Consignment?.BorderTransportMeans?.ID ?? ZString.Empty);

		public ZString JourneyID => fINX5105Declaration?.GoodsShipment?.Consignment?.BorderTransportMeans?.JourneyID ?? ZString.Empty;

		public ZString MasterBillNo => fINX5105Declaration?.GoodsShipment?.Consignment?.TransportContractDocuments?.FirstOrDefault(x => x.TypeCode == MessageConstants.TransportContractDocumentTypeCodes._704 || x.TypeCode == MessageConstants.TransportContractDocumentTypeCodes._741)?.ID ?? ZString.Empty;

		public ZDecimal ExchangeRate => fINX5105Declaration?.CurrencyExchange?.RateNumeric ?? ZDecimal.Zero;

		public ZString CodeOfPortOfLoading => fINX5105Declaration?.GoodsShipment?.Consignment?.LoadingLocation?.ID ?? ZString.Empty;

		public ZString NameOfPortOfLoading => cusEntryHeader.Declaration?.PortOfOriginName ?? ZString.Empty;

		public ZString ExportationDate
		{
			get
			{
				var datetime = fINX5105Declaration?.GoodsShipment?.ExitDateTime ?? ZDateTime.Empty;
				return datetime.ToTaiWanDateString();
			}
		}

		public ZString DateOfImportation
		{
			get
			{
				var datetime = fINX5105Declaration?.BorderTransportMeans?.ArrivalDateTime ?? ZDateTime.Empty;
				return datetime.ToTaiWanDateString();
			}
		}

		public ZString FreightCurrency => fINX5105Declaration?.CurrencyExchange?.CurrencyTypeCode ?? ZString.Empty;

		ZDecimal Freight => fINX5105Declaration?.GoodsShipment?.CustomsValuation?.FreightChargeAmount ?? ZDecimal.Zero;

		public ZString FreightFormat => FormatChargeCurrencyMacro(Freight);

		public ZString CodeOfStorageLocationCode => fINX5105Declaration?.GoodsShipment?.Consignment?.GoodsLocation ?? ZString.Empty;

		public ZString CodeOfStorageLocationDescription => entryInstruction?.GoodsLocation?.ZZD_Description ?? ZString.Empty;

		public ZString CodeOfStorageLocation
		{
			get
			{
				var locationStringBuild = new ZStringBuilder();
				locationStringBuild.AppendIfNotEmpty(CodeOfStorageLocationCode);
				if (RegistryHelper.DefaultPrintingGoodsLocationDescription)
				{
					locationStringBuild.AppendIfNotEmpty(CodeOfStorageLocationDescription);
				}
				return locationStringBuild.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZString TransportModeCode => fINX5105Declaration?.GoodsShipment?.Consignment?.ArrivalTransportMeansTypeCode ?? ZString.Empty;

		public ZString DateOfDeclaration
		{
			get
			{
				var datetime = entryInstruction?.DateOfValuation ?? ZDateTime.Empty;
				return datetime.ToTaiWanDateString();
			}
		}

		public ZString InsuranceFeeCurrency => fINX5105Declaration?.CurrencyExchange?.CurrencyTypeCode ?? ZString.Empty;

		ZDecimal InsuranceFee => fINX5105Declaration?.GoodsShipment?.CustomsValuation?.ExitToEntryChargeAmount ?? ZDecimal.Zero;

		public ZString InsuranceFeeFormat => FormatChargeCurrencyMacro(InsuranceFee);

		public ZString BAN => fINX5105Declaration?.Importer?.ID ?? ZString.Empty;

		public ZString ImporterCustomsSupervisionCode => fINX5105Declaration?.Importer?.CustomsControlID ?? ZString.Empty;

		public ZString RelatedToSeller => fINX5105Declaration?.GoodsShipment?.CustomsValuation?.PartyRelationshipCode ?? ZString.Empty;

		public ZString ModeOfDutyPayment => fINX5105Declaration?.DutyTaxFee?.DutyMethodCode ?? ZString.Empty;

		public ZString ExpensesToBeAddedCurrency => fINX5105Declaration?.CurrencyExchange?.CurrencyTypeCode ?? ZString.Empty;

		ZDecimal ExpensesToBeAdded => fINX5105Declaration?.GoodsShipment?.CustomsValuation?.OtherChargeAmount ?? ZDecimal.Zero;

		public ZString ExpensesToBeAddedFormat => FormatChargeCurrencyMacro(ExpensesToBeAdded);

		public ZString ExpensesToBeDeductedCurrency => fINX5105Declaration?.CurrencyExchange?.CurrencyTypeCode ?? ZString.Empty;

		ZDecimal ExpensesToBeDeducted => fINX5105Declaration?.GoodsShipment?.CustomsValuation?.OtherDeductionAmount ?? ZDecimal.Zero;

		public ZString ExpensesToBeDeductedFormat => FormatChargeCurrencyMacro(ExpensesToBeDeducted);

		public ZString ImporterChineseName => fINX5105Declaration?.Importer?.ChineseName ?? ZString.Empty;

		public ZString ImporterChineseNamePart1 => ImporterChineseName.Left(17);

		public ZString ImporterChineseNamePart2 => ImporterChineseName.SubstringSafe(17);

		public ZString ImporterEnglishName => fINX5105Declaration?.Importer?.Name ?? ZString.Empty;

		public ZString PaymentOnAccountBusinessID => fINX5105Declaration?.Importer?.PaymentOnAccountBusinessID ?? ZString.Empty;

		public ZString ImporterAddressLine
		{
			get
			{
				var chineseAddressLine = jobDeclarationDocumentAddressConfig?.HideIMPImporterTradChineseAddr ?? false ? ZString.Empty : fINX5105Declaration?.Importer?.Address?.ChineseLine ?? ZString.Empty;
				var englishAddressLine = jobDeclarationDocumentAddressConfig?.HideIMPImporterEnglishAddr ?? false ? ZString.Empty : fINX5105Declaration?.Importer?.Address?.Line ?? ZString.Empty;
				return DocumentWrapperHelper.GetAddressLine(chineseAddressLine, englishAddressLine);
			}
		}

		public ZString ImporterAEOCode => fINX5105Declaration?.Importer?.LPCOAuthorizedParty?.ID ?? ZString.Empty;

		public ZString CustomsApprovalNo => fINX5105Declaration?.DutyTaxFee?.PaymentObligationGuaranteeReferenceID ?? ZString.Empty;

		public ZString SellerChineseName => fINX5105Declaration?.GoodsShipment?.Seller?.ChineseName ?? ZString.Empty;

		public ZString SellerEnglishName => fINX5105Declaration?.GoodsShipment?.Seller?.Name ?? ZString.Empty;

		public ZString SellerAddressLine
		{
			get
			{
				var chineseAddressLine = jobDeclarationDocumentAddressConfig?.HideIMPSellerTradChineseAddr ?? false ? ZString.Empty : fINX5105Declaration?.GoodsShipment?.Seller?.Address?.ChineseLine ?? ZString.Empty;
				var englishAddressLine = fINX5105Declaration?.GoodsShipment?.Seller?.Address?.Line ?? ZString.Empty;
				return DocumentWrapperHelper.GetAddressLine(chineseAddressLine, englishAddressLine);
			}
		}

		public ZString SellerAEONo => fINX5105Declaration?.GoodsShipment?.Seller?.LPCOAuthorizedParty?.ID ?? ZString.Empty;

		public ZString SellerCountryCode => fINX5105Declaration?.GoodsShipment?.Seller?.Address?.CountryCode ?? ZString.Empty;

		public ZString BusinessAdministrationNo => fINX5105Declaration?.GoodsShipment?.Seller?.ID ?? ZString.Empty;

		public ZString SellerCustomsSupervisionCode => fINX5105Declaration?.GoodsShipment?.Seller?.CustomsControlID ?? ZString.Empty;

		public ZString TotalPackageQuantityAndUnit
		{
			get
			{
				var totalPackageQuantity = fINX5105Declaration?.TotalPackageQuantity ?? ZInt.Zero;
				var typeCode = fINX5105Declaration?.Packaging?.TypeCode ?? ZString.Empty;
				var result = ZString.Empty;
				if (!typeCode.IsEmpty)
				{
					result = ZString.Format("{0} / {1}", totalPackageQuantity, typeCode);
				}
				return result;
			}
		}

		public ZString DescriptionOfPackage => fINX5105Declaration?.Packaging?.PackagingMaterialDescription ?? ZString.Empty;

		public ZDecimal GrossWeight => fINX5105Declaration?.TotalGrossMassMeasure ?? ZDecimal.Zero;

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

		ZString GetOtherDeclarationFrontAtLine(int idx)
		{
			if (Part1.Count > idx)
			{
				return Part1[idx];
			}
			return ZString.Empty;
		}

		ZString MarksNumbers => fINX5105Declaration?.Packaging?.MarksNumbers ?? ZString.Empty;

		ZString ContainerNumbers
		{
			get
			{
				var result = ZString.Empty;
				var containers = fINX5105Declaration?.GoodsShipment?.Consignment?.TransportEquipments?.Select(x => GetContainerCodesForPrinting(x));
				if (containers != null && containers.Any())
				{
					result = ZString.Join("\r\n", containers.ToArray());
				}
				return result;
			}
		}

		ZString GetContainerCodesForPrinting(ITransportEquipment container)
		{
			var result = container.ID;
			if (!container.CharacteristicCode.IsEmpty)
			{
				result += ZString.Format("/{0}", container.CharacteristicCode);
			}
			if (!container.UsedCapacityCode.IsEmpty)
			{
				result += ZString.Format("/{0}", container.UsedCapacityCode);
			}
			var seals = container.Seals;
			foreach (var seal in seals.Take(2))
			{
				if (!seal.IsEmpty)
				{
					result += ZString.Format("/{0}", seal);
				}
			}
			return result;
		}

		ZString OtherDeclarationInfo => entryInstruction?.TW_TradersRemarks ?? ZString.Empty;

		List<ZString> Part1 { get; set; }

		List<ZString> Part2 { get; set; }

		public ZDecimal TotalAmount
		{
			get
			{
				var confirmedTotalDutyTaxFee = cusEntryHeader.CH_ConfirmedTotalDutyTaxFee;
				return confirmedTotalDutyTaxFee.IsEmpty ? dutyTaxFeeTotalCashDutyTaxFeeAmount : confirmedTotalDutyTaxFee;
			}
		}

		ZDecimal dutyTaxFeeTotalCashDutyTaxFeeAmount => fINX5105Declaration?.DutyTaxFee?.TotalCashDutyTaxFeeAmount ?? ZDecimal.Zero;

		public ZDecimal TotalNonCashAmount
		{
			get
			{
				var confirmedTotalDutyTaxFeeDeferred = cusEntryHeader.CH_ConfirmedTotalDutyTaxFeeDeferred;
				return confirmedTotalDutyTaxFeeDeferred.IsEmpty ? dutyTaxFeeTotalNonCashDutyTaxFeeAmount : confirmedTotalDutyTaxFeeDeferred;
			}
		}

		ZDecimal dutyTaxFeeTotalNonCashDutyTaxFeeAmount => fINX5105Declaration?.DutyTaxFee?.TotalNonCashDutyTaxFeeAmount ?? ZDecimal.Zero;

		public ZDecimal BusinessTaxBase
		{
			get
			{
				var confirmedBusinessTaxBase = cusEntryHeader.CH_ConfirmedBusinessTaxBase;
				return confirmedBusinessTaxBase.IsEmpty ? goodsShipmentCustomsValuationOtherChargeDeductionAmount : confirmedBusinessTaxBase;
			}
		}

		ZDecimal goodsShipmentCustomsValuationOtherChargeDeductionAmount => fINX5105Declaration?.GoodsShipment?.CustomsValuation?.OtherChargeDeductionAmount ?? ZDecimal.Zero;

		public ZString ClearanceType => Declaration?.ClearanceStatus ?? ZString.Empty;

		public ZString ClearanceCode => fINX5105Declaration?.AssociatedGovernmentProcedureCode ?? ZString.Empty;

		public ZString DuplicateNo
		{
			get
			{
				var duplicates = entryInstruction?.DeclarationDuplicates?.Cast<DeclarationDuplicate>().Take(2)?.Select(x => x.CY_Code);
				var result = ZString.Empty;
				if (duplicates != null)
				{
					result = ZString.Join("\r\n", duplicates.ToArray());
				}
				return result;
			}
		}

		public ZString NoOfCopies
		{
			get
			{
				var duplicates = entryInstruction?.DeclarationDuplicates?.Cast<DeclarationDuplicate>().Take(2)?.Select(x => x.CY_Data);
				var result = ZString.Empty;
				if (duplicates != null)
				{
					result = ZString.Join("\r\n", duplicates.ToArray());
				}
				return result;
			}
		}

		ZString DeclarantCompanyName
		{
			get
			{
				var result = ZString.Empty;
				var companyAddress = Declaration?.DeclarantAddress;
				if (companyAddress != null)
				{
					if (companyAddress.OA_Language == Core.SharedConstants.Languages.ChineseTraditional)
					{
						result = companyAddress.CompanyName;
					}
					if (result.IsEmpty)
					{
						var zhTWTranslation = companyAddress.GetTranslatedAddressInSpecificLanguage(Core.SharedConstants.Languages.ChineseTraditional);
						result = zhTWTranslation?.CompanyName ?? ZString.Empty;
					}
				}
				return result;
			}
		}

		public ZString DeclarantAndAEOCode
		{
			get
			{
				var declarantBuilder = new ZStringBuilder();
				var agent = fINX5105Declaration?.Agent;
				if (agent != null)
				{
					declarantBuilder.AppendIfNotEmpty(DeclarantCompanyName);
					declarantBuilder.Append(" ");
					declarantBuilder.AppendIfNotEmpty(agent.ID);
					declarantBuilder.AppendLine();
					declarantBuilder.AppendIfNotEmpty(agent.LPCOAuthorizedParty?.ID ?? ZString.Empty);
				}
				return declarantBuilder.ToString().Trim();
			}
		}

		public ZString DedicatedStaff
		{
			get
			{
				var representativePersonName = fINX5105Declaration?.RepresentativePersonName;
				var fullName = cusEntryHeader.Declaration?.CusAgent?.GS_FullName ?? ZString.Empty;
				return ZString.Format("{0}\r\n{1}", fullName, representativePersonName);
			}
		}

		public ZString HouseBillNo => fINX5105Declaration?.GoodsShipment?.Consignment?.TransportContractDocuments?.FirstOrDefault(x => x.TypeCode == MessageConstants.TransportContractDocumentTypeCodes._703 || x.TypeCode == MessageConstants.TransportContractDocumentTypeCodes._714)?.ID ?? ZString.Empty;

		public ZDecimal TotalInvoiceAmount => fINX5105Declaration?.GoodsShipment?.ItemChargeAmount ?? ZDecimal.Zero;

		public ZString TotalInvoiceAmountCurrency => fINX5105Declaration?.CurrencyExchange?.CurrencyTypeCode ?? ZString.Empty;

		public ZDecimal TotalAdValoremTaxBaseAmount => cusEntryHeader.CH_TotalCustomsValueInInvoiceCurrency;

		public ZString TotalAdValoremTaxBaseAmountCurrency => cusEntryHeader.FirstInvoiceCurrencyCode;

		public ZDecimal TotalCIFAmount => fINX5105Declaration?.GoodsShipment?.TotalCIFAmount ?? ZDecimal.Zero;

		public ZString TotalCIFAmountCurrency => Core.Constants.CurrencyCodes.Taiwan;

		public ZString A10ImportDuty
		{
			get
			{
				var amount = A10AmountFromN5110 ?? fINX5105Declaration?.GoodsShipment?.DutyTaxFees?.FirstOrDefault(x => x.TypeCode == DutyTaxFeeCodeList.Codes.A10)?.AdValoremTaxBaseAmount ?? ZDecimal.Zero;
				return amount == ZDecimal.Zero ? string.Empty : amount.ToString();
			}
		}

		ZDecimal? A10AmountFromN5110 => GetAmountFromEntryPayInfos(EntryChargeTypeList.Codes.A10);

		ZDecimal? B51AmountFromN5110 => GetAmountFromEntryPayInfos(EntryChargeTypeList.Codes.B51);

		ZDecimal? GetAmountFromEntryPayInfos(ZString chargeCode)
		{
			return cusEntryHeader.EntryPayInfos.Where(x => x.C9_TransactionType == chargeCode).OrderByDescending(x => x.C9_SystemCreateTimeUtc).FirstOrDefault()?.C9_PaymentAmount;
		}

		public ZString A19ImportDuty
		{
			get
			{
				var amount = A10AmountFromN5110.HasValue ? ZDecimal.Zero : (fINX5105Declaration?.GoodsShipment?.DutyTaxFees?.FirstOrDefault(x => x.TypeCode == DutyTaxFeeCodeList.Codes.A19)?.AdValoremTaxBaseAmount ?? ZDecimal.Zero);
				return amount == ZDecimal.Zero ? string.Empty : amount.ToString();
			}
		}

		public ZString B51ImportDuty
		{
			get
			{
				var amount = B51AmountFromN5110 ?? fINX5105Declaration?.GoodsShipment?.DutyTaxFees?.FirstOrDefault(x => x.TypeCode == DutyTaxFeeCodeList.Codes.B51)?.AdValoremTaxBaseAmount ?? ZDecimal.Zero;
				return amount == ZDecimal.Zero ? string.Empty : amount.ToString();
			}
		}

		public ZString B59ImportDuty
		{
			get
			{
				var amount = A10AmountFromN5110.HasValue ? ZDecimal.Zero : (fINX5105Declaration?.GoodsShipment?.DutyTaxFees?.FirstOrDefault(x => x.TypeCode == DutyTaxFeeCodeList.Codes.B59)?.AdValoremTaxBaseAmount ?? ZDecimal.Zero);
				return amount == ZDecimal.Zero ? string.Empty : amount.ToString();
			}
		}

		public ZString DutyTaxFee1Description => dutyList.FirstOrDefault()?.Item1 ?? ZString.Empty;

		public ZString DutyTaxFee2Description => dutyList.ElementAtOrDefault(1)?.Item1 ?? ZString.Empty;

		public ZString DutyTaxFee3Description => dutyList.ElementAtOrDefault(2)?.Item1 ?? ZString.Empty;

		public ZString DutyTaxFee4Description => dutyList.ElementAtOrDefault(3)?.Item1 ?? ZString.Empty;

		public ZString DutyTaxFee1DescriptionEN => dutyList.FirstOrDefault()?.Item2 ?? ZString.Empty;

		public ZString DutyTaxFee2DescriptionEN => dutyList.ElementAtOrDefault(1)?.Item2 ?? ZString.Empty;

		public ZString DutyTaxFee3DescriptionEN => dutyList.ElementAtOrDefault(2)?.Item2 ?? ZString.Empty;

		public ZString DutyTaxFee4DescriptionEN => dutyList.ElementAtOrDefault(3)?.Item2 ?? ZString.Empty;

		public ZDecimal DutyTaxFee1 => dutyList.FirstOrDefault()?.Item3 ?? ZDecimal.Zero;

		public ZDecimal DutyTaxFee2 => dutyList.ElementAtOrDefault(1)?.Item3 ?? ZDecimal.Zero;

		public ZDecimal DutyTaxFee3 => dutyList.ElementAtOrDefault(2)?.Item3 ?? ZDecimal.Zero;

		public ZDecimal DutyTaxFee4 => dutyList.ElementAtOrDefault(3)?.Item3 ?? ZDecimal.Zero;

		public ZBool DutyTaxISDEF1 => dutyList.FirstOrDefault()?.Item4 ?? ZBool.False;

		public ZBool DutyTaxISDEF2 => dutyList.ElementAtOrDefault(1)?.Item4 ?? ZBool.False;

		public ZBool DutyTaxISDEF3 => dutyList.ElementAtOrDefault(2)?.Item4 ?? ZBool.False;

		public ZBool DutyTaxISDEF4 => dutyList.ElementAtOrDefault(3)?.Item4 ?? ZBool.False;

		public ZString JobNumber => cusEntryHeader.Declaration?.JE_DeclarationReference ?? ZString.Empty;

		public ZString OwnerReference => cusEntryHeader.Declaration?.JE_OwnerRef ?? ZString.Empty;

		public ZString EntryReleaseDate => cusEntryHeader.CH_EntryReleaseDate.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);

		public ZString CreateUser
		{
			get
			{
				var createUser = cusEntryHeader.Declaration?.JE_SystemCreateUser ?? ZString.Empty;
				return !createUser.IsEmpty ? cusEntryHeader.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, createUser)?.GS_FullName ?? ZString.Empty : ZString.Empty;
			}
		}

		public ZDecimal TotalCIFAmountInUSD
		{
			get
			{
				var result = ZDecimal.Zero;
				var rateDate = fINX5105Declaration?.AcceptanceDateTime ?? ZDateTime.Empty;
				if (!rateDate.IsEmpty && !TotalCIFAmount.IsEmpty)
				{
					result = cusEntryHeader.LocalCurrency.ConvertUsingCustomsRate(rateDate, TotalCIFAmount, RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates));
				}
				return result;
			}
		}

		internal IEnumerable<IGovernmentAgencyGoodsItem> GovernmentAgencyGoodsItems => fINX5105Declaration?.GoodsShipment?.GovernmentAgencyGoodsItems;

		public ZString TariffChapterName => ((IChapterNameGenerator)this).GetChapterNameGenerator()?.GetTariffChapterName() ?? ZString.Empty;

		public CusEntryHeader CusEntryHeader => cusEntryHeader;

		#endregion

		readonly List<IGovernmentAgencyGoodsItem> goodsItems;

		CachedProperty<int> netWeightMeasureDecimalPlaceCached;
		int netWeightMeasureDecimalPlace => Factory.GetValue(ref netWeightMeasureDecimalPlaceCached, () => DocumentWrapperHelper.GetDecimalPlaces(goodsItems, x => x?.GoodsMeasure?.NetWeightMeasure ?? ZDecimal.Zero));

		CachedProperty<int> tariffQuantityDecimalPlaceCached;
		int tariffQuantityDecimalPlace => Factory.GetValue(ref tariffQuantityDecimalPlaceCached, () => DocumentWrapperHelper.GetDecimalPlaces(goodsItems, x => x?.GoodsMeasure?.TariffQuantity ?? ZDecimal.Zero));

		CachedProperty<int> statisticalQuantityDecimalPlaceCached;
		int statisticalQuantityDecimalPlace => Factory.GetValue(ref statisticalQuantityDecimalPlaceCached, () => DocumentWrapperHelper.GetDecimalPlaces(goodsItems, x => x?.GoodsStatisticalMeasure?.TariffQuantity ?? ZDecimal.Zero));

		public ZDecimal CommonDecimalPlaces => 2;

		ZString FormatChargeCurrencyMacro(ZDecimal amount) => amount <= 0 ? new ZString(Constants.NIL) : DocumentWrapperHelper.FormatNumber(amount, CommonDecimalPlaces.ToZInt());

		#region IChapterNameGenerator
		BaseChapterNameGenerator IChapterNameGenerator.GetChapterNameGenerator()
		{
			BaseChapterNameGenerator result = null;
			if (goodsItems != null)
			{
				var declarationNo = DeclarationNo;
				if (declarationNo.StartsWith(TaiwanCustomsDistrictList.Codes.A))
				{
					result = new ChapterNameGeneratorA(this);
				}
				else if (declarationNo.StartsWith(Constants.CustomsOffice.BA) || declarationNo.StartsWith(Constants.CustomsOffice.BJ))
				{
					result = new ChapterNameGeneratorBA(this);
				}
				else if (declarationNo.StartsWith(Constants.CustomsOffice.BC))
				{
					result = new ChapterNameGeneratorBC(this);
				}
				else if (declarationNo.StartsWith(Constants.CustomsOffice.BD))
				{
					result = new ChapterNameGeneratorBD(this);
				}
				else if (declarationNo.StartsWith(Constants.CustomsOffice.BE))
				{
					result = new ChapterNameGeneratorBE(this);
				}
				else if (declarationNo.StartsWith(Constants.CustomsOffice.BF))
				{
					result = new ChapterNameGeneratorBF(this);
				}
				else if (declarationNo.StartsWith(TaiwanCustomsDistrictList.Codes.C))
				{
					result = new ChapterNameGeneratorC(this);
				}
				else if (declarationNo.StartsWith(TaiwanCustomsDistrictList.Codes.D))
				{
					result = new ChapterNameGeneratorD(this);
				}
			}

			return result;
		}
		#endregion

		public ZString TotalInvoiceAmountCaption => TradeTermsConditionCode == Core.Constants.IncoTerms.ExWorks ? ExwAmountCaption : FobAmountCaption;

		ZString TradeTermsConditionCode => fINX5105Declaration?.GoodsShipment?.TradeTermsConditionCode ?? ZString.Empty;
	}

	public class ImportPart2SectionBodyWrapper : DocumentWrapper
	{
		public ImportPart2SectionBodyWrapper()
		{
		}

		public ZString OtherDeclarationsFront { get; set; }
	}

	public class ImportDeclarationSectionBodyWrapper : DocumentWrapper
	{
		public ImportDeclarationSectionBodyWrapper()
		{
			priceList = new List<ZString>() { Capacity = 4 };
			dutyPayingValueList = new List<ZString>() { Capacity = 4 };
			statisticsQualityAndUnitList = new List<ZString>() { Capacity = 4 };
		}

		public Image TrademarkImage { get; set; }

		public ZString Box34ItemNumber { get; set; }

		public ZString CountryCode { get; set; }

		internal ZString[] Box35 = new ZString[4];

		public ZString Box35DescriptionOfGoods_Line1 => Box35[0].ReplaceLineBreakWithSpace();

		public ZString Box35DescriptionOfGoods_Line2 => Box35[1].ReplaceLineBreakWithSpace();

		public ZString Box35DescriptionOfGoods_Line3 => Box35[2].ReplaceLineBreakWithSpace();

		public ZString Box35DescriptionOfGoods_Line4 => Box35[3].ReplaceLineBreakWithSpace();

		public ZString Box37ImportPermitNumberAndItemNumber_Line1 { get; set; }

		public ZString Box37ImportPermitNumberAndItemNumber_Line2 { get; set; }

		public ZString Box38CCCCode { get; set; }

		public ZString Box38AssignedNumber { get; set; }

		internal bool IsTotal { get; set; }

		internal bool IsNotPrintEmpty { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no translation needed")]
		const string PrintEmptyValue = "(本欄空白)";

		internal bool ShowEvenRowIsEmpty { get; set; }
		internal bool ShowDashLine { get; set; }
		internal bool ShowInSameCellIfPossible { get; set; }
		public ZBool ShouldHideRow1 => Box35DescriptionOfGoods_Line1.IsEmpty && Box37ImportPermitNumberAndItemNumber_Line1.IsEmpty && Box39IncoTermAndCurrency.IsEmpty && Box40NetWeight.IsEmpty && Box43CustomsValue_Line1.IsEmpty && Box44Ad_ValoremDutyRate.IsEmpty && Box34ItemNumber.IsEmpty && CountryCode.IsEmpty && !ShowEvenRowIsEmpty;
		public ZBool ShouldHideRow2 => Box35DescriptionOfGoods_Line2.IsEmpty && Box37ImportPermitNumberAndItemNumber_Line2.IsEmpty && Box39UnitPrice_Line1.IsEmpty && Box41QuantityAndUnit.IsEmpty && Box43CustomsValue_Line2.IsEmpty && Box44Ad_ValoremDutyRate.IsEmpty && Box44OtherDutyRate.IsEmpty && TrademarkImage == null;
		public ZBool ShouldHideRow3 => Box35DescriptionOfGoods_Line3.IsEmpty && Box38CCCCode.IsEmpty && Box39UnitPrice_Line2.IsEmpty && Box42StatisticsQuantityAndUnit_Line1.IsEmpty && Box43CustomsValue_Line3.IsEmpty && Box44SpecificDutyRate.IsEmpty && (TrademarkImage == null || TrademarkImage?.Height <= 24);
		public ZBool ShouldHideRow4 => Box35DescriptionOfGoods_Line4.IsEmpty && Box38AssignedNumber.IsEmpty && Box39RorRapUnitPrice.IsEmpty && Box42StatisticsQuantityAndUnit_Line2.IsEmpty && Box43RorRapCustomsValue.IsEmpty && (TrademarkImage == null || TrademarkImage?.Height <= 36);
		public ZBool ShouldHideDashLine => !ShowDashLine;
		public int RowCountToShow => (!ShouldHideRow1 ? 1 : 0) + (!ShouldHideRow2 ? 1 : 0) + (!ShouldHideRow3 ? 1 : 0) + (!ShouldHideRow4 ? 1 : 0);
		public ZBool ShouldShowInSameCellIfPossible => ShowInSameCellIfPossible;

		#region Prices
		public ZString Box39IncoTermAndCurrency => GetPrice(0);

		public ZString Box39UnitPrice_Line1 => GetPrice(1);

		public ZString Box39UnitPrice_Line2 => GetPrice(2);

		public ZString Box39RorRapUnitPrice => GetPrice(3);

		public ZString Box39Line11 => IsNotPrintEmpty || IsTotal ? Box39IncoTermAndCurrency : ZString.Empty;

		public ZString Box39Line21 => IsNotPrintEmpty ? (ZString)PrintEmptyValue : ZString.Empty;

		ZString GetPrice(ZInt index) => priceList.Count > index ? priceList[index] : ZString.Empty;

		internal void AddPriceItem(ZString item)
		{
			priceList.Add(item);
		}

		readonly List<ZString> priceList;
		#endregion

		#region StatisticsQualityAndUnits
		public ZString Box40NetWeight => GetStatisticsQualityAndUnit(0);

		public ZString Box41QuantityAndUnit => GetStatisticsQualityAndUnit(1);

		public ZString Box42StatisticsQuantityAndUnit_Line1 => GetStatisticsQualityAndUnit(2);

		public ZString Box42StatisticsQuantityAndUnit_Line2 => GetStatisticsQualityAndUnit(3);

		internal ZBool IsNeedBreakAndNew => statisticsQualityAndUnitList.Count == 4;

		ZString GetStatisticsQualityAndUnit(ZInt index) => statisticsQualityAndUnitList.Count > index ? statisticsQualityAndUnitList[index] : ZString.Empty;

		internal void AddStatisticsQualityAndUnitItem(ZString item)
		{
			statisticsQualityAndUnitList.Add(item);
		}

		readonly List<ZString> statisticsQualityAndUnitList;
		#endregion

		#region DutyPayingValues

		public ZString Box43CustomsValue_Line1 => GetDutyPayingValue(0);

		public ZString Box43CustomsValue_Line2 => GetDutyPayingValue(1);

		public ZString Box43CustomsValue_Line3 => GetDutyPayingValue(2);

		public ZString Box43RorRapCustomsValue => GetDutyPayingValue(3);

		public ZString Box43Line11 => IsNotPrintEmpty || IsTotal ? (ZString)PrintEmptyValue : ZString.Empty;

		public ZString Box43Line21 => ZString.Empty;

		ZString GetDutyPayingValue(ZInt index) => dutyPayingValueList.Count > index ? dutyPayingValueList[index] : ZString.Empty;

		internal void AddDutyPayingValueItem(ZString item)
		{
			dutyPayingValueList.Add(item);
		}

		readonly List<ZString> dutyPayingValueList;
		#endregion

		public ZString TermOfTradeCurrency { get; set; }

		public ZString Box44Ad_ValoremDutyRate { get; set; }

		public ZString Box44SpecificDutyRate { get; set; }

		public ZString Box44OtherDutyRate { get; set; }

		public ZString Box45CodeOfDutyPayment { get; set; }

		public ZString Box46CommodityTaxRate_Line1 { get; set; }

		public ZString Box46CommodityTaxRate_Line2 { get; set; }
	}
}
