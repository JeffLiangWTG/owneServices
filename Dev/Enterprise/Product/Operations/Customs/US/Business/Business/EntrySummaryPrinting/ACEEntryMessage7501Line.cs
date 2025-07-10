using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	public class ACEEntryMessage7501Line : EntrySummary7501Line, IObsoleteValidation
	{
		public ACEEntryMessage7501Line(EntryMessageLine entryMessageLine, ZBool multiOrigin, ZBool multiExport, ZBool multiLading, ZBool multiManufacturer, ZBool multiRelationship, bool hasMultipleExportDates, ZBool printInvoiceHeading, ZBool printInvoiceDetails, ZString aDDCVDSuretyCode, ZString[] dutyPercentageStrings, US7501DocPrinting docData, US7501DocPrinting[] childLinesDocData, ZString entryType)
			: base(entryMessageLine.Factory, entryType)
		{
			this.aens40 = entryMessageLine.aens40;
			this.aens43RulingsList = entryMessageLine.aens43;
			this.aens47PartyList = entryMessageLine.aens47;
			this.aens50TariffList = entryMessageLine.aens50;
			this.aens51 = entryMessageLine.aens51;
			this.aens52VisaList = entryMessageLine.aens52;
			this.aens53List = entryMessageLine.aens53;
			this.aens54 = entryMessageLine.aens54;
			this.aens60 = entryMessageLine.aens60;
			this.aens62ChargesList = entryMessageLine.aens62;
			this.mpfRate = entryMessageLine.MPFRate;

			this.multiOrigin = multiOrigin;
			this.multiExport = multiExport;
			this.multiLading = multiLading;
			this.multiManufacturer = multiManufacturer;
			this.multiRelationship = multiRelationship;
			this.multiExportDate = hasMultipleExportDates;
			this.PrintInvoiceHeading = printInvoiceHeading;
			this.PrintInvoiceDetails = printInvoiceDetails;
			this.aDDCVDSuretyCode = aDDCVDSuretyCode;
			this.dutyPercentageStrings = dutyPercentageStrings;
			this.docData = docData;
			this.childLinesDocData = childLinesDocData;

			RefreshFeesAndTaxes();
		}

		readonly AENS40 aens40;
		readonly List<AENS43> aens43RulingsList;
		readonly List<AENS47> aens47PartyList;
		readonly List<AENS50> aens50TariffList;

		readonly AENS51 aens51;
		readonly List<AENS52> aens52VisaList;

		readonly List<AENS53> aens53List;
		readonly List<AENS54> aens54;
		readonly AENS60 aens60;
		readonly List<IChargeBlock> aens62ChargesList;

		readonly ZBool multiOrigin;
		readonly ZBool multiExport;
		readonly ZBool multiLading;
		readonly ZBool multiManufacturer;
		readonly ZBool multiRelationship;
		readonly bool multiExportDate;
		readonly ZString aDDCVDSuretyCode;
		readonly ZString[] dutyPercentageStrings;
		readonly US7501DocPrinting docData;
		readonly US7501DocPrinting[] childLinesDocData;

		USCTariff ImportTariff1
		{
			get { return importTariff1 ?? (importTariff1 = new USCTariff.Loader(Factory).LoadBestMatch(aens50TariffList[0].HTSNumber, DateForDutyCalculation)); }
		}
		USCTariff importTariff1;

		USCTariff ImportTariff2
		{
			get
			{
				if (importTariff2 == null && SecondaryTariffLine)
				{
					importTariff2 = new USCTariff.Loader(Factory).LoadBestMatch(aens50TariffList[1].HTSNumber, DateForDutyCalculation);
				}
				return importTariff2;
			}
		}
		USCTariff importTariff2;

		USCTariff ImportTariff3
		{
			get
			{
				if (importTariff3 == null && SecondaryTariffLine2)
				{
					importTariff3 = new USCTariff.Loader(Factory).LoadBestMatch(aens50TariffList[2].HTSNumber, DateForDutyCalculation);
				}
				return importTariff3;
			}
		}
		USCTariff importTariff3;

		public override ZDate DateForDutyCalculation
		{
			get
			{
				var result = docData.US_DutyDate;

				if (result.IsEmpty && InvoiceLine != null)
				{
					result = InvoiceLine.EffectiveDateForDutyRate;
				}
				return !result.IsEmpty ? result.Date : ZDate.Today;
			}
		}

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Factory.Load<JobComInvoiceLine>(docData.US_InvoiceLinePK)); }
		}
		JobComInvoiceLine invoiceLine;

		#region EntrySummary7501Line Methods

		public override ZDate DateForAD_CVD
		{
			get
			{
				var result = docData.US_DutyDate;

				if (result.IsEmpty && InvoiceLine != null)
				{
					result = InvoiceLine.DateForADD_CVD;
				}
				return !result.IsEmpty ? result.Date : ZDate.Today;
			}
		}

		public override ZBool AdValoremConversionCalculation
		{
			get { return !docData.US_AVWatches.IsEmpty; }
		}

		public override ZString AVWatches { get { return docData.US_AVWatches; } }
		public override ZDecimal AVWatchesDuty { get { return docData.US_AVWatchesDuty; } }
		public override ZString AVCases { get { return docData.US_AVCases; } }
		public override ZDecimal AVCasesDuty { get { return docData.US_AVCasesDuty; } }
		public override ZString AVBracelets { get { return docData.US_AVBracelets; } }
		public override ZDecimal AVBraceletsDuty { get { return docData.US_AVBraceletsDuty; } }
		public override ZString AVBatteries { get { return docData.US_AVBatteries; } }
		public override ZDecimal AVBatteriesDuty { get { return docData.US_AVBatteriesDuty; } }
		public override ZDecimal AVTotalDuty { get { return docData.US_AVTotalDuty; } }
		public override ZString AVLine2 { get { return docData.US_AVLine2; } }

		public override ZBool ProRatedCalculation
		{
			get { return !docData.US_ProRatedLine1.IsEmpty; }
		}

		public override ZString ProRatedLine1
		{
			get { return docData.US_ProRatedLine1; }
		}

		public override ZString ProRatedLine2
		{
			get { return docData.US_ProRatedLine2; }
		}

		public override ZString ProRatedLine3
		{
			get { return docData.US_ProRatedLine3; }
		}

		protected override ZBool GetPrintSPIOnSecondLineCore()
		{
			return SecondaryTariffLine && (ImportTariff1 == null || !ImportTariff1.Applies(TariffRuleList.Codes.InLieuTariffs, DateForDutyCalculation));
		}

		protected override EntrySummary7501Invoice GetInvoiceDetails()
		{
			JobComInvoiceHeader invoiceHeader = Factory.Load<JobComInvoiceHeader>(docData.US_InvoicePK);
			if (invoiceHeader != null)
			{
				return new EntrySummary7501Invoice(invoiceHeader, docData.US_InvoiceSeq, multiRelationship);
			}

			return null;
		}

		public override ZString TransRelatedInd
		{
			get
			{
				ZString result = ZString.Empty;

				if (multiRelationship)
				{
					result = aens40.RelatedPartyIndicator;
				}

				return result;
			}
		}

		public override ZString LineNumber
		{
			get { return aens40.LineItemIdentifier; }
		}

		public override ZBool SecondaryTariffLine
		{
			get { return aens50TariffList.Count > 1; }
		}

		public override ZBool SecondaryTariffLine2
		{
			get { return aens50TariffList.Count > 2; }
		}

		public override ZBool SecondaryTariffLine3
		{
			get { return aens50TariffList.Count > 3; }
		}

		public override ZBool SecondaryTariffLine4
		{
			get { return aens50TariffList.Count > 4; }
		}

		public override ZBool SecondaryTariffLine5
		{
			get { return aens50TariffList.Count > 5; }
		}

		public override ZBool SecondaryTariffLine6
		{
			get { return aens50TariffList.Count > 6; }
		}

		public override ZBool SecondaryTariffLine7
		{
			get { return aens50TariffList.Count > 7; }
		}

		public override ZString Description
		{
			get { return ImportTariff1 != null ? ImportTariff1.UE_ShortDescription : ZString.Empty; }
		}

		public override ZString SPIAndOrSecondarySPI
		{
			get
			{
				if (!spiAndOrSecondarySPICached.HasValue)
				{
					var result = aens40.TradeAgreementSpecialProgramClaimCode;
					var secondarySPI = aens40.ProductClaimCode;
					if (!secondarySPI.IsEmpty)
					{
						result += (!result.IsEmpty ? "." : "") + secondarySPI;
					}

					var articleSetIndicator = aens40.ArticleSetIndicator;
					if (!articleSetIndicator.IsEmpty)
					{
						result += (!result.IsEmpty ? "," : "") + articleSetIndicator;
					}
					spiAndOrSecondarySPICached = result;
				}
				return spiAndOrSecondarySPICached.Value;
			}
		}
		ZString? spiAndOrSecondarySPICached;

		public override ZString CountryOfOriginForLine
		{
			get { return multiOrigin ? "O," + aens40.CountryOfOriginCode : ""; }
		}

		public override ZString CountryOfExportForLine
		{
			get { return multiExport ? "E," + aens40.CountryOfExportCode : ""; }
		}

		public override ZString ExportDateForTextile
		{
			get
			{
				ZString result = ZString.Empty;

				if (!aens40.DateOfExportationforTextiles.IsEmpty)
				{
					result = "D/E " + aens40.DateOfExportationforTextiles.ToString("MM/dd/yyyy");
				}

				return result;
			}
		}

		TariffFormatter TariffFormatter => tariffFormatter ?? (tariffFormatter = new TariffFormatter());
		TariffFormatter tariffFormatter;

		public override ZString PortOfLadingForLine
		{
			get { return multiLading ? aens40.ForeignPortOfLadingCode : ZString.Empty; }
		}

		protected override ZDate ExportDateCore
		{
			get { return multiExportDate ? aens40.DateOfExportation : ZDate.Empty; }
		}

		public override ZString FormattedTariff
		{
			get { return TariffFormatter.DisplayFormat(aens50TariffList[0].HTSNumber); }
		}

		public override ZString ExclusionNumber
		{
			get
			{
				var aens54ForExclusionNumbers = aens54.Where(x => x.ImportersAdditionalDeclarationTypeCode == AdditionalDeclarationTypeCodeList.Codes._02 || x.ImportersAdditionalDeclarationTypeCode == AdditionalDeclarationTypeCodeList.Codes._03).Select(x => x.ImportersAdditionalDeclarationInformation);
				var result = new ZStringBuilder();

				foreach (var number in aens54ForExclusionNumbers)
				{
					result.Append(ProductExclusionTitle + number);
				}
				return result.ToStringWithNewLineBetweenAppends();
			}
		}
		const string ProductExclusionTitle = "Product Exclusion No: ";

		public override ZDecimal GrossWeightInKilograms
		{
			get { return aens40.GrossShippingWeight; }
		}

		public override ZDecimal CustomsQuantity
		{
			get { return ZDecimal.ParseSafe(aens50TariffList[0].Quantity1.ParseForABI(12, 2).ToStringTrimZeros(), 0); }
		}

		public override ZString CustomsUnitQty
		{
			get { return aens50TariffList[0].UnitOfMeasureCode1; }
		}

		public override ZDecimal TotalLinePriceInLocalCurrencyRounded
		{
			get { return aens50TariffList[0].ValueOfGoodsAmount.Round(0); }
		}

		public override ZString ChargesRounded
		{
			get
			{
				ZString chargesValue = ZString.Empty;
				if (aens40.ChargesAmount > 0)
				{
					chargesValue = "C" + aens40.ChargesAmount.ToString();
				}

				return chargesValue;
			}
		}

		public override ZString DutyPercentAsString
		{
			get { return dutyPercentageStrings[0]; }
		}

		public override ZString SecondQtyAndUQ
		{
			get { return GetQuantityAndUnit(aens50TariffList[0].Quantity2.ParseForABI(12, 2), aens50TariffList[0].UnitOfMeasureCode2); }
		}

		public override ZString ThirdQtyAndUQ
		{
			get { return GetQuantityAndUnit(aens50TariffList[0].Quantity3.ParseForABI(12, 2), aens50TariffList[0].UnitOfMeasureCode3); }
		}

		#region ADD/CVD

		AENS53 AENS53ADDBlock
		{
			get { return aens53ADDBlock ?? (aens53ADDBlock = aens53List.Find(x => x.CaseNumber.StartsWith("A"))); }
		}
		AENS53 aens53ADDBlock;

		AENS53 AENS53CVDBlock
		{
			get { return aens53CVDBlock ?? (aens53CVDBlock = aens53List.Find(x => x.CaseNumber.StartsWith("C"))); }
		}
		AENS53 aens53CVDBlock;

		bool ADDOnParentLine
		{
			get { return docData.US_ADDOnParentOrChild == ADDCVDOnParentOrChild.Codes.Parent; }
		}

		bool ADDOnChildLine
		{
			get { return docData.US_ADDOnParentOrChild == ADDCVDOnParentOrChild.Codes.Child; }
		}

		bool CVDOnParentLine
		{
			get { return docData.US_CVDOnParentOrChild == ADDCVDOnParentOrChild.Codes.Parent; }
		}

		bool CVDOnChildLine
		{
			get { return docData.US_CVDOnParentOrChild == ADDCVDOnParentOrChild.Codes.Child; }
		}

		public override ZString ADDNo
		{
			get
			{
				var result = ZString.Empty;

				if (ADDOnParentLine)
				{
					result = GetAddNoFrom53Blocks();
				}
				return result;
			}
		}

		ZString GetAddNoFrom53Blocks()
		{
			var result = ZString.Empty;
			var antidumpingCaseNumber = AENS53ADDBlock != null ? AENS53ADDBlock.CaseNumber : ZString.Empty;

			if (!antidumpingCaseNumber.IsEmpty)
			{
				result = (antidumpingCaseNumber.SubstringSafe(0, 4) + "-" +
						antidumpingCaseNumber.SubstringSafe(4, 3) + "-" +
						antidumpingCaseNumber.SubstringSafe(7, 3)).TrimEnd('-');
			}
			return result;
		}

		ZString ADDCVDSuretyValue
		{
			get { return aDDCVDSuretyCode.IsEmpty ? "" : "Surety Code #" + aDDCVDSuretyCode; }
		}

		public override ZString ADDSurety
		{
			get { return ADDNo.IsEmpty ? ZString.Empty : ADDCVDSuretyValue; }
		}

		public override ZString ADDSpecificDepositValueFormatted
		{
			get
			{
				var result = ZString.Empty;

				if (AENS53ADDBlock != null && AENS53ADDBlock.ADCVDValueOfGoodsAmount > 0 &&
						 AENS53ADDBlock.ADCVDValueOfGoodsAmount != TotalLinePriceInLocalCurrencyRounded)
				{
					result = "(" + AENS53ADDBlock.ADCVDValueOfGoodsAmount.ToString() + ")";
				}
				return result;
			}
		}

		public override ZString ADDRate
		{
			get
			{
				var result = "";
				if (ADDOnParentLine && AENS53ADDBlock != null)
				{
					result = GetADCVDRateDescriptionFromAENS53(AENS53ADDBlock);
				}
				return result;
			}
		}

		public override ZString ADDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (ADDOnParentLine && AENS53ADDBlock != null && AENS53ADDBlock.ADCVDDutyAmount > 0)
				{
					if (AENS53ADDBlock.BondCashClaimCode == "B")
					{
						result = "(" + AENS53ADDBlock.ADCVDDutyAmount.ToString(2) + ")";
					}
					else
					{
						result = AENS53ADDBlock.ADCVDDutyAmount.ToString(2);
					}
				}

				return result;
			}
		}

		public override ZString CVDNo
		{
			get
			{
				ZString result = ZString.Empty;

				if (CVDOnParentLine)
				{
					result = GetCVDNumberFrom53Block();
				}
				return result;
			}
		}

		ZString GetCVDNumberFrom53Block()
		{
			var result = ZString.Empty;
			if (AENS53CVDBlock != null && !AENS53CVDBlock.CaseNumber.IsEmpty)
			{
				result = (AENS53CVDBlock.CaseNumber.SubstringSafe(0, 4) + "-" +
						AENS53CVDBlock.CaseNumber.SubstringSafe(4, 3) + "-" +
						AENS53CVDBlock.CaseNumber.SubstringSafe(7, 3)).TrimEnd('-');
			}
			return result;
		}

		public override ZString CVDSurety
		{
			get { return CVDOnParentLine && CVDNo.IsEmpty ? ZString.Empty : ADDCVDSuretyValue; }
		}

		public override ZString CVDSpecificDepositValueFormatted
		{
			get
			{
				var result = ZString.Empty;
				if (AENS53CVDBlock != null && AENS53CVDBlock.ADCVDValueOfGoodsAmount > 0 &&
						 AENS53CVDBlock.ADCVDValueOfGoodsAmount != TotalLinePriceInLocalCurrencyRounded)
				{
					result = "(" + AENS53CVDBlock.ADCVDValueOfGoodsAmount.ToString() + ")";
				}
				return result;
			}
		}

		public override ZString CVDRate
		{
			get
			{
				var result = "";
				if (CVDOnParentLine && AENS53CVDBlock != null)
				{
					result = GetADCVDRateDescriptionFromAENS53(AENS53CVDBlock);
				}
				return result;
			}
		}

		public override ZString CVDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (CVDOnParentLine && AENS53CVDBlock != null && AENS53CVDBlock.ADCVDDutyAmount > 0)
				{
					if (AENS53CVDBlock.BondCashClaimCode == "B")
					{
						result = "(" + AENS53CVDBlock.ADCVDDutyAmount.ToString(2) + ")";
					}
					else
					{
						result = AENS53CVDBlock.ADCVDDutyAmount.ToString(2);
					}
				}

				return result;
			}
		}

		#endregion

		public override ZDecimal LumberExportPrice
		{
			get
			{
				var result = ZDecimal.Zero;
				var aens54ForLumberExportPrice = aens54.FirstOrDefault(x => x.ImportersAdditionalDeclarationTypeCode == "01" && x.ImportersAdditionalDeclarationInformation.Length > 10);
				if (aens54ForLumberExportPrice != null)
				{
					result = ZDecimal.ParseSafe(aens54ForLumberExportPrice.ImportersAdditionalDeclarationInformation.SubstringSafe(1, 10), ZDecimal.Zero);
				}
				return result;
			}
		}

		public override ZString LumberImporterDeclaration
		{
			get
			{
				var result = ZString.Empty;
				var aens54ForLumberImporterDeclaration = aens54.FirstOrDefault(x => x.ImportersAdditionalDeclarationTypeCode == "01" && x.ImportersAdditionalDeclarationInformation.Length >= 1);
				if (aens54ForLumberImporterDeclaration != null)
				{
					result = aens54ForLumberImporterDeclaration.ImportersAdditionalDeclarationInformation.SubstringSafe(0, 1);
				}
				return result;
			}
		}

		public override ZDecimal LumberExportCharges
		{
			get
			{
				var result = ZDecimal.Zero;
				var aens54ForLumberExportCharges = aens54.FirstOrDefault(x => x.ImportersAdditionalDeclarationTypeCode == "01" && x.ImportersAdditionalDeclarationInformation.Length > 11);
				if (aens54ForLumberExportCharges != null)
				{
					result = ZDecimal.ParseSafe(aens54ForLumberExportCharges.ImportersAdditionalDeclarationInformation.SubstringSafe(11, 10), ZDecimal.Zero);
				}
				return result;
			}
		}

		public override ZDecimal DutyAmount
		{
			get
			{
				var result = docData.US_DutyAmount;
				if (result.IsEmpty)
				{
					result = aens50TariffList[0].DutyAmount;
				}
				return result;
			}
		}

		protected override bool USComponentsAssembledAbroad
		{
			get { return ImportTariff1 != null && ImportTariff1.Applies(TariffRuleList.Codes.AssembledAbroadOfUSProducts, DateForDutyCalculation); }
		}

		string GetADCVDRateDescriptionFromAENS53(AENS53 aens53)
		{
			if (aens53.CaseRateTypeQualifierCode == DepositRateIndicatorList.Codes.Specific)
			{
				var adCase = Factory.LoadFromNaturalKey<USCACCase>(USCACCaseSchema.U5_CaseNumber, aens53.CaseNumber);
				var caseRate = adCase != null ? adCase.CaseRates.GetDepositRate(DateForAD_CVD) : null;
				return DepositRateIndicatorList.GetACERateDescriptionForSpecificOrOverrideSpecific(aens53.CaseDepositRate, caseRate != null ? caseRate.U6_Unit : ZString.Empty, caseRate != null ? caseRate.U6_UnitDesc : ZString.Empty);
			}
			else
			{
				return DepositRateIndicatorList.GetAdValoremRateDescriptionFromPercentage(aens53.CaseNumber, aens53.CaseDepositRate);
			}
		}

		#region Visa and Certificates

		public override ZString VisaCertificateNumber
		{
			get
			{
				var result = ZString.Empty;

				if (!VisaNumber.IsEmpty)
				{
					result = VisaNumber;
				}
				else if (!CottonCertificateNumberOrganicExemptionCertificateNumber.IsEmpty)
				{
					result = CottonCertificateNumberOrganicExemptionCertificateNumber;
				}
				else if (!CanadianExportCertificateSugar.IsEmpty)
				{
					result = CanadianExportCertificateSugar;
				}
				else if (!CBTPACertificationNumber.IsEmpty)
				{
					result = CBTPACertificationNumber;
				}

				return result;
			}
		}

		ZString VisaNumber
		{
			get { return aens51 != null ? (!aens51.StandardVisaNumber.IsEmpty ? "V " + aens51.StandardVisaNumber : "") : ""; }
		}

		ZString CottonCertificateNumberOrganicExemptionCertificateNumber
		{
			get
			{
				var licences = from AENS52 lic in aens52VisaList
							   where LicencePermitTypeList.DeclareInCottonOrganicExemptionFieldInACS(lic.LicenseCertificatePermitTypeCode)
							   select lic.LicenseNumberCertificateNumberPermitNumber;

				return licences.Any() ? "C " + new ZStringBuilder(licences).ToStringWithDelimiterBetweenAppends(", ") : "";
			}
		}

		AENS52 GetBlock52ByLicenseType(ZString licenseTypeCode)
		{
			return aens52VisaList != null ?
					aens52VisaList.Find(x => x.LicenseCertificatePermitTypeCode == licenseTypeCode)
					: null;
		}

		ZString CanadianExportCertificateSugar
		{
			get
			{
				var result = ZString.Empty;
				var block52 = GetBlock52ByLicenseType(LicencePermitTypeList.Codes._16);
				if (block52 != null)
				{
					result = "C " + block52.LicenseNumberCertificateNumberPermitNumber;
				}

				return result;
			}
		}

		ZString CBTPACertificationNumber
		{
			get
			{
				var result = ZString.Empty;
				var block52 = GetBlock52ByLicenseType(LicencePermitTypeList.Codes._18);
				if (block52 != null)
				{
					result = "C " + block52.LicenseNumberCertificateNumberPermitNumber;
				}

				return result;
			}
		}

		public override ZString LicenseNumber
		{
			get
			{
				if (!licenseNumberCached.HasValue)
				{
					licenseNumberCached = ZString.Empty;
					if (aens52VisaList != null)
					{
						licenseNumberCached = string.Join("\r\n", aens52VisaList.Where(x => !x.LicenseNumberCertificateNumberPermitNumber.IsEmpty).Select(x => x.LicenseCertificatePermitTypeCode + "-" + x.LicenseNumberCertificateNumberPermitNumber.ToUpper()));
					}
				}
				return licenseNumberCached.Value;
			}
		}
		ZString? licenseNumberCached;

		#endregion

		#region Block29Elements

		public override ZString TextileCategoryNumberWithLabel
		{
			get
			{
				var result = ZString.Empty;
				if (!aens40.CategoryCodeforTextiles.IsEmpty)
				{
					result = "CAT " + aens40.CategoryCodeforTextiles;
				}

				return result;
			}
		}

		public override ZString LineLevelManufacturerIDWithLabel
		{
			get
			{
				var result = ZString.Empty;

				if (multiManufacturer && !ManufacturerID.IsEmpty) //if multiManufacturer, it needs to print at line level
				{
					result = "MID " + ManufacturerID;
				}
				return result;
			}
		}

		ZString ManufacturerID
		{
			get
			{
				if (!manufacturerIDCached.HasValue)
				{
					manufacturerIDCached = ZString.Empty;

					if (aens47PartyList != null)
					{
						var block = aens47PartyList.Find(x => x.ArticlePartyTypeCode == "M");
						if (block != null)
						{
							manufacturerIDCached = block.ArticlePartyIdentifier;
						}
					}
				}
				return manufacturerIDCached.Value;
			}
		}
		ZString? manufacturerIDCached;

		public override ZString BindingRulingWithLabel
		{
			get
			{
				var result = ZString.Empty;

				if (aens43RulingsList != null)
				{
					foreach (var rulingDetail in aens43RulingsList)
					{
						if (rulingDetail.RulingTypeCode == PIRPRulingTypeList.Codes.BindingRulings)
						{
							result = "RLNG " + rulingDetail.RulingNumber;
							break;
						}
					}
				}

				return result;
			}
		}

		public override List<ZString> Block29Elements
		{
			get
			{
				if (block29Elements == null)
				{
					block29Elements = new List<ZString>();

					if (!TextileCategoryNumberWithLabel.IsEmpty)
					{
						block29Elements.Add(TextileCategoryNumberWithLabel);
					}

					if (!LineLevelManufacturerIDWithLabel.IsEmpty)
					{
						block29Elements.Add(LineLevelManufacturerIDWithLabel);
					}

					if (!BindingRulingWithLabel.IsEmpty)
					{
						block29Elements.Add(BindingRulingWithLabel);
					}

					if (docData != null)
					{
						var invoiceLine = Factory.Load<JobComInvoiceLine>(docData.US_InvoiceLinePK);
						if (invoiceLine != null)
						{
							var declaration = invoiceLine.Declaration;
							if (declaration != null)
							{
								if (declaration.ShouldPrintProductNumber7501)
								{
									block29Elements.Add(invoiceLine.JI_PartNo);
								}

								var impWrapper = declaration.ImporterWrapper;
								if (impWrapper != null)
								{
									if (!invoiceLine.JI_CustomAttrib1.IsEmpty && impWrapper.ZO_ENSPrintCustomAttrib1)
									{
										block29Elements.Add(invoiceLine.JI_CustomAttrib1);
									}

									if (!invoiceLine.JI_CustomAttrib2.IsEmpty && impWrapper.ZO_ENSPrintCustomAttrib2)
									{
										block29Elements.Add(invoiceLine.JI_CustomAttrib2);
									}

									if (!invoiceLine.JI_CustomAttrib3.IsEmpty && impWrapper.ZO_ENSPrintCustomAttrib3)
									{
										block29Elements.Add(invoiceLine.JI_CustomAttrib3);
									}
								}
							}
						}
					}
				}

				return block29Elements;
			}
		}

		#endregion

		#region Fees

		IChargeBlock ChargeAmountForCode(ZString chargeCode)
		{
			if (aens62ChargesList != null)
			{
				foreach (IChargeBlock chargeDetail in aens62ChargesList)
				{
					string chargeDetailCode = chargeDetail.AccountingClassCode.ToString().PadLeft(3, '0');
					if (chargeDetailCode == chargeCode)
					{
						return chargeDetail;
					}
				}
			}

			return null;
		}

		public override ZString LineFeePercentAsString
		{
			get { return FeeAsString(LineFeeCode); }
		}

		protected override Dictionary<ZString, ZDecimal> PopulateLineFeesAndTaxes()
		{
			var result = new Dictionary<ZString, ZDecimal>();

			if (aens62ChargesList != null)
			{
				foreach (IChargeBlock ens62 in aens62ChargesList)
				{
					if (ens62.UserFeeAmount > 0 && CusFeeCodeConstants.GetAccountingClassFeeCodeList(Factory).ContainsCode(ens62.AccountingClassCode))
					{
						result.Add(ens62.AccountingClassCode, ens62.UserFeeAmount);
					}
				}
			}

			if (aens60 != null && aens60.IRTaxAmount > 0)
			{
				foreach (string taxCode in CusFeeCodeConstants.GetTaxCodes())
				{
					if (GetDocPrintingDataMatching(taxCode) != null)
					{
						result.Add(taxCode, aens60.IRTaxAmount);
						break;
					}
				}
			}

			return result;
		}

		protected override ZString LineFeeRateType
		{
			get
			{
				var docData = GetDocPrintingDataMatching(LineFeeCode);
				return docData != null ? docData.US_SpecificRate : ZString.Empty;
			}
		}

		ZString FeeAsString(string feeCode)
		{
			var docData = GetDocPrintingDataMatching(feeCode);
			return docData != null ? docData.US_FEEPercentAsString : ZString.Empty;
		}

		US7501DocPrinting GetDocPrintingDataMatching(ZString feeCode)
		{
			var result = docData;

			if (result.US_FEECode != feeCode)
			{
				result = null;

				if (childLinesDocData != null)
				{
					foreach (US7501DocPrinting childLineDocData in childLinesDocData)
					{
						if (childLineDocData.US_FEECode == feeCode)
						{
							result = childLineDocData;
							break;
						}
					}
				}
			}

			return result;
		}

		public override ZBool HasMPF
		{
			get { return ChargeAmountForCode(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing) != null; }
		}

		public override ZString MPFPercentAsString
		{
			get { return mpfRate; }
		}
		readonly ZString mpfRate;

		#endregion

		#region Secondary Tariff Lines

		#region Secondary Line 1

		public override ZString SecondaryLine1Description
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine)
				{
					result = ImportTariff2 != null ? ImportTariff2.UE_ShortDescription : ZString.Empty;
				}

				return result;
			}
		}

		public override ZString SecondaryLine1FormattedTariff => SecondaryTariffLine ? TariffFormatter.DisplayFormat(aens50TariffList[1].HTSNumber) : string.Empty;

		public override ZString SecondaryLine1SecondQtyAndUQ
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine)
				{
					result = GetQuantityAndUnit(aens50TariffList[1].Quantity2.ParseForABI(12, 2), aens50TariffList[1].UnitOfMeasureCode2);
				}

				return result;
			}
		}

		public override ZString SecondaryLine1ThirdQtyAndUQ
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine)
				{
					result = GetQuantityAndUnit(aens50TariffList[1].Quantity3.ParseForABI(12, 2), aens50TariffList[1].UnitOfMeasureCode3);
				}

				return result;
			}
		}

		#region Secondary line countervailing & anti dumping duty
		/// <summary>
		/// Secondary line fields are not available for ADD & CVD in the message, but values do go in Totals.
		/// These details even if entered on the secondary line are sent in the aens40 & aens60 blocks of the set
		/// </summary>

		bool ADDOnSecondaryLine
		{
			get { return !SecondaryLine1ADDNo.IsEmpty; }
		}

		public override ZString SecondaryLine1ADDNo
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine && ADDOnChildLine)
				{
					result = GetAddNoFrom53Blocks();
				}
				return result;
			}
		}

		public override ZString SecondaryLine1ADDSurety
		{
			get { return ADDOnSecondaryLine ? ADDCVDSuretyValue : ZString.Empty; }
		}

		public override ZString SecondaryLine1ADDSpecificDepositValueFormatted
		{
			get
			{
				var result = ZString.Empty;

				if (ADDOnSecondaryLine && AENS53ADDBlock != null)
				{
					if (AENS53ADDBlock.ADCVDValueOfGoodsAmount > 0 & AENS53ADDBlock.ADCVDValueOfGoodsAmount != TotalLinePriceInLocalCurrencyRounded)
					{
						result = "(" + AENS53ADDBlock.ADCVDValueOfGoodsAmount.ToString() + ")";
					}
				}
				return result;
			}
		}

		public override ZString SecondaryLine1ADDRate
		{
			get
			{
				return ADDOnSecondaryLine && AENS53ADDBlock != null ? GetADCVDRateDescriptionFromAENS53(AENS53ADDBlock) : "";
			}
		}

		public override ZString SecondaryLine1ADDFormatted
		{
			get
			{
				var result = ZString.Empty;

				if (ADDOnSecondaryLine && AENS53ADDBlock != null && AENS53ADDBlock.ADCVDDutyAmount > 0)
				{
					if (AENS53ADDBlock.BondCashClaimCode == "B")
					{
						result = "(" + AENS53ADDBlock.ADCVDDutyAmount.ToString(2) + ")";
					}
					else
					{
						result = AENS53ADDBlock.ADCVDDutyAmount.ToString(2);
					}
				}
				return result;
			}
		}

		bool CVDOnSecondaryLine
		{
			get { return !SecondaryLine1CVDNo.IsEmpty; }
		}

		public override ZString SecondaryLine1CVDNo
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine && CVDOnChildLine)
				{
					result = GetCVDNumberFrom53Block();
				}
				return result;
			}
		}

		public override ZString SecondaryLine1CVDSurety
		{
			get { return CVDOnSecondaryLine ? ADDCVDSuretyValue : ZString.Empty; }
		}

		public override ZString SecondaryLine1CVDSpecificDepositValueFormatted
		{
			get
			{
				var result = ZString.Empty;

				if (CVDOnSecondaryLine && AENS53CVDBlock != null)
				{
					if (AENS53CVDBlock.ADCVDValueOfGoodsAmount > 0 & AENS53CVDBlock.ADCVDValueOfGoodsAmount != TotalLinePriceInLocalCurrencyRounded)
					{
						result = "(" + AENS53CVDBlock.ADCVDValueOfGoodsAmount.ToString() + ")";
					}
				}
				return result;
			}
		}

		public override ZString SecondaryLine1CVDRate
		{
			get
			{
				return CVDOnSecondaryLine && AENS53CVDBlock != null ? GetADCVDRateDescriptionFromAENS53(AENS53CVDBlock) : "";
			}
		}

		public override ZString SecondaryLine1CVDFormatted
		{
			get
			{
				var result = ZString.Empty;

				if (CVDOnSecondaryLine && AENS53CVDBlock != null && AENS53CVDBlock.ADCVDDutyAmount > 0)
				{
					if (AENS53CVDBlock.BondCashClaimCode == "B")
					{
						result = "(" + AENS53CVDBlock.ADCVDDutyAmount.ToString(2) + ")";
					}
					else
					{
						result = AENS53CVDBlock.ADCVDDutyAmount.ToString(2);
					}
				}
				return result;
			}
		}

		#endregion

		public override ZDecimal SecondaryLine1CustomsQuantity
		{
			get { return SecondaryTariffLine ? aens50TariffList[1].Quantity1.ParseForABI(12, 2) : 0; }
		}

		public override ZString SecondaryLine1CustomsUnitQty
		{
			get { return SecondaryTariffLine ? aens50TariffList[1].UnitOfMeasureCode1 : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine1TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine ? aens50TariffList[1].ValueOfGoodsAmount.Round(0) : ZDecimal.Zero; }
		}

		public override ZDecimal SecondaryLine1DutyAmount
		{
			get
			{
				var result = ZDecimal.Zero;

				if (SecondaryTariffLine)
				{
					if (IsTIBEntry)
					{
						if (childLinesDocData != null && childLinesDocData.Length > 0 && childLinesDocData[0].US_SecondaryLineTIBCalculatedDutyAmount > 0)
						{
							result = childLinesDocData[0].US_SecondaryLineTIBCalculatedDutyAmount;
						}
					}
					else if (childLinesDocData.Length > 0 && childLinesDocData[0].US_IsAdditionalTotalPrintingDuty)
					{
						result = childLinesDocData[0].US_DutyAmount;
					}
					else if (docData.US_SecondaryLine1DutyAmount > 0)
					{
						result = docData.US_SecondaryLine1DutyAmount;
					}
					else
					{
						result = aens50TariffList[1].DutyAmount;
					}
				}
				return result;
			}
		}

		public override ZString SecondaryLine1DutyPercentAsString
		{
			get { return dutyPercentageStrings[1]; }
		}

		public override ZBool PrintSPIOnSecondLine1 => PrintSPIOnSecondaryLine(SecondaryTariffLine ? aens50TariffList[1].HTSNumber : ZString.Empty);

		#endregion

		#region Secondary Line 2

		public override ZString SecondaryLine2Description
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine2)
				{
					result = ImportTariff3 != null ? ImportTariff3.UE_ShortDescription : ZString.Empty;
				}

				return result;
			}
		}

		public override ZString SecondaryLine2FormattedTariff
		{
			get { return SecondaryTariffLine2 ? TariffFormatter.DisplayFormat(aens50TariffList[2].HTSNumber) : ""; }
		}

		public override ZString SecondaryLine2SecondQtyAndUQ
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine2)
				{
					result = GetQuantityAndUnit(aens50TariffList[2].Quantity2.ParseForABI(12, 2), aens50TariffList[2].UnitOfMeasureCode2);
				}

				return result;
			}
		}

		public override ZString SecondaryLine2ThirdQtyAndUQ
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine2)
				{
					result = GetQuantityAndUnit(aens50TariffList[2].Quantity3.ParseForABI(12, 2), aens50TariffList[2].UnitOfMeasureCode3);
				}

				return result;
			}
		}

		#region Secondary line2 countervailing & anti dumping duty
		/// <summary>
		/// Cannot send ADD / CVD details electronically for other than 1 secondary line.
		/// All subsequent values when printing from message will be empty.
		/// </summary>

		public override ZString SecondaryLine2ADDNo
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine2ADDSurety
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine2ADDSpecificDepositValueFormatted
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine2ADDRate
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine2ADDFormatted
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine2CVDNo
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine2CVDSurety
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine2CVDSpecificDepositValueFormatted
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine2CVDRate
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine2CVDFormatted
		{
			get { return ZString.Empty; }
		}

		#endregion

		public override ZDecimal SecondaryLine2CustomsQuantity
		{
			get { return SecondaryTariffLine2 ? aens50TariffList[2].Quantity1.ParseForABI(12, 2) : 0; }
		}

		public override ZString SecondaryLine2CustomsUnitQty
		{
			get { return SecondaryTariffLine2 ? aens50TariffList[2].UnitOfMeasureCode1 : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine2TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine2 ? aens50TariffList[2].ValueOfGoodsAmount.Round(0) : ZDecimal.Zero; }
		}

		public override ZString SecondaryLine2DutyPercentAsString
		{
			get { return dutyPercentageStrings[2]; }
		}

		public override ZDecimal SecondaryLine2DutyAmount
		{
			get
			{
				var result = ZDecimal.Zero;
				if (SecondaryTariffLine2)
				{
					if (IsTIBEntry)
					{
						if (childLinesDocData != null && childLinesDocData.Length > 1 && childLinesDocData[1].US_SecondaryLineTIBCalculatedDutyAmount > 0)
						{
							result = childLinesDocData[1].US_SecondaryLineTIBCalculatedDutyAmount;
						}
					}
					else if (childLinesDocData.Length > 1 && childLinesDocData[1].US_IsAdditionalTotalPrintingDuty)
					{
						result = childLinesDocData[1].US_DutyAmount;
					}
					else
					{
						result = aens50TariffList[2].DutyAmount;
					}
				}
				return result;
			}
		}

		public override ZBool PrintSPIOnSecondLine2 => PrintSPIOnSecondaryLine(SecondaryTariffLine2 ? aens50TariffList[2].HTSNumber : ZString.Empty);

		#endregion

		#region Secondary Line 3

		public override ZString SecondaryLine3Description
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine3)
				{
					var uscTariff = new USCTariff.Loader(Factory).LoadBestMatch(aens50TariffList[3].HTSNumber, DateForDutyCalculation);
					if (uscTariff != null)
					{
						result = uscTariff.UE_ShortDescription;
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine3FormattedTariff
		{
			get { return SecondaryTariffLine3 ? TariffFormatter.DisplayFormat(aens50TariffList[3].HTSNumber) : ""; }
		}

		public override ZString SecondaryLine3SecondQtyAndUQ
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine3)
				{
					result = GetQuantityAndUnit(aens50TariffList[3].Quantity2.ParseForABI(12, 2), aens50TariffList[3].UnitOfMeasureCode2);
				}

				return result;
			}
		}

		public override ZString SecondaryLine3ThirdQtyAndUQ
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine3)
				{
					result = GetQuantityAndUnit(aens50TariffList[3].Quantity3.ParseForABI(12, 2), aens50TariffList[3].UnitOfMeasureCode3);
				}

				return result;
			}
		}

		#region Secondary Line3 countervailing & anti dumping duty
		/// <summary>
		/// Cannot send ADD / CVD details electronically for other than 1 secondary line.
		/// All subsequent values when printing from message will be empty.
		/// </summary>

		public override ZString SecondaryLine3ADDNo
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine3ADDSurety
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine3ADDSpecificDepositValueFormatted
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine3ADDRate
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine3ADDFormatted
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine3CVDNo
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine3CVDSurety
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine3CVDSpecificDepositValueFormatted
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine3CVDRate
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine3CVDFormatted
		{
			get { return ZString.Empty; }
		}

		#endregion

		public override ZDecimal SecondaryLine3CustomsQuantity
		{
			get { return SecondaryTariffLine3 ? aens50TariffList[3].Quantity1.ParseForABI(12, 2) : 0; }
		}

		public override ZString SecondaryLine3CustomsUnitQty
		{
			get { return SecondaryTariffLine3 ? aens50TariffList[3].UnitOfMeasureCode1 : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine3TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine3 ? aens50TariffList[3].ValueOfGoodsAmount.Round(0) : ZDecimal.Zero; }
		}

		public override ZDecimal SecondaryLine3DutyAmount
		{
			get
			{
				var result = ZDecimal.Zero;

				if (SecondaryTariffLine3)
				{
					if (IsTIBEntry)
					{
						if (childLinesDocData != null && childLinesDocData.Length > 2 && childLinesDocData[2].US_SecondaryLineTIBCalculatedDutyAmount > 0)
						{
							result = childLinesDocData[2].US_SecondaryLineTIBCalculatedDutyAmount;
						}
					}
					else if (childLinesDocData.Length > 2 && childLinesDocData[2].US_IsAdditionalTotalPrintingDuty)
					{
						result = childLinesDocData[2].US_DutyAmount;
					}
					else if (docData.US_SecondaryLine3DutyAmount > 0)
					{
						result = docData.US_SecondaryLine3DutyAmount;
					}
					else
					{
						result = aens50TariffList[3].DutyAmount;
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine3DutyPercentAsString
		{
			get { return dutyPercentageStrings[3]; }
		}

		public override ZBool PrintSPIOnSecondLine3 => PrintSPIOnSecondaryLine(SecondaryTariffLine3 ? aens50TariffList[3].HTSNumber : ZString.Empty);

		#endregion

		#region Secondary Line 4

		public override ZString SecondaryLine4Description
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine4)
				{
					var uscTariff = new USCTariff.Loader(Factory).LoadBestMatch(aens50TariffList[4].HTSNumber, DateForDutyCalculation);
					if (uscTariff != null)
					{
						result = uscTariff.UE_ShortDescription;
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine4FormattedTariff
		{
			get { return SecondaryTariffLine4 ? TariffFormatter.DisplayFormat(aens50TariffList[4].HTSNumber) : ""; }
		}

		public override ZString SecondaryLine4SecondQtyAndUQ
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine4)
				{
					result = GetQuantityAndUnit(aens50TariffList[4].Quantity2.ParseForABI(12, 2), aens50TariffList[4].UnitOfMeasureCode2);
				}

				return result;
			}
		}

		public override ZString SecondaryLine4ThirdQtyAndUQ
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine4)
				{
					result = GetQuantityAndUnit(aens50TariffList[4].Quantity3.ParseForABI(12, 2), aens50TariffList[4].UnitOfMeasureCode3);
				}

				return result;
			}
		}

		#region Secondary Line4 countervailing & anti dumping duty
		/// <summary>
		/// Cannot send ADD / CVD details electronically for other than 1 secondary line.
		/// All subsequent values when printing from message will be empty.
		/// </summary>

		public override ZString SecondaryLine4ADDNo
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine4ADDSurety
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine4ADDSpecificDepositValueFormatted
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine4ADDRate
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine4ADDFormatted
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine4CVDNo
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine4CVDSurety
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine4CVDSpecificDepositValueFormatted
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine4CVDRate
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine4CVDFormatted
		{
			get { return ZString.Empty; }
		}

		#endregion

		public override ZDecimal SecondaryLine4CustomsQuantity
		{
			get { return SecondaryTariffLine4 ? aens50TariffList[4].Quantity1.ParseForABI(12, 2) : 0; }
		}

		public override ZString SecondaryLine4CustomsUnitQty
		{
			get { return SecondaryTariffLine4 ? aens50TariffList[4].UnitOfMeasureCode1 : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine4TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine4 ? aens50TariffList[4].ValueOfGoodsAmount.Round(0) : ZDecimal.Zero; }
		}

		public override ZString SecondaryLine4DutyPercentAsString
		{
			get { return dutyPercentageStrings[4]; }
		}

		public override ZDecimal SecondaryLine4DutyAmount
		{
			get
			{
				var result = ZDecimal.Zero;
				if (SecondaryTariffLine4)
				{
					if (IsTIBEntry)
					{
						if (childLinesDocData != null && childLinesDocData.Length > 3 && childLinesDocData[3].US_SecondaryLineTIBCalculatedDutyAmount > 0)
						{
							result = childLinesDocData[3].US_SecondaryLineTIBCalculatedDutyAmount;
						}
					}
					else if (childLinesDocData.Length > 3 && childLinesDocData[3].US_IsAdditionalTotalPrintingDuty)
					{
						result = childLinesDocData[3].US_DutyAmount;
					}
					else
					{
						result = aens50TariffList[4].DutyAmount;
					}
				}
				return result;
			}
		}

		public override ZBool PrintSPIOnSecondLine4 => PrintSPIOnSecondaryLine(SecondaryTariffLine4 ? aens50TariffList[4].HTSNumber : ZString.Empty);

		#endregion

		#region Secondary Line 5

		public override ZString SecondaryLine5Description
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine5)
				{
					var uscTariff = new USCTariff.Loader(Factory).LoadBestMatch(aens50TariffList[5].HTSNumber, DateForDutyCalculation);
					if (uscTariff != null)
					{
						result = uscTariff.UE_ShortDescription;
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine5FormattedTariff
		{
			get { return SecondaryTariffLine5 ? TariffFormatter.DisplayFormat(aens50TariffList[5].HTSNumber) : ""; }
		}

		public override ZString SecondaryLine5SecondQtyAndUQ
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine5)
				{
					result = GetQuantityAndUnit(aens50TariffList[5].Quantity2.ParseForABI(12, 2), aens50TariffList[5].UnitOfMeasureCode2);
				}

				return result;
			}
		}

		public override ZString SecondaryLine5ThirdQtyAndUQ
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine5)
				{
					result = GetQuantityAndUnit(aens50TariffList[5].Quantity3.ParseForABI(12, 2), aens50TariffList[5].UnitOfMeasureCode3);
				}

				return result;
			}
		}

		#region Secondary Line5 countervailing & anti dumping duty
		/// <summary>
		/// Cannot send ADD / CVD details electronically for other than 1 secondary line.
		/// All subsequent values when printing from message will be empty.
		/// </summary>

		public override ZString SecondaryLine5ADDNo
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine5ADDSurety
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine5ADDSpecificDepositValueFormatted
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine5ADDRate
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine5ADDFormatted
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine5CVDNo
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine5CVDSurety
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine5CVDSpecificDepositValueFormatted
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine5CVDRate
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine5CVDFormatted
		{
			get { return ZString.Empty; }
		}

		#endregion

		public override ZDecimal SecondaryLine5CustomsQuantity
		{
			get { return SecondaryTariffLine5 ? aens50TariffList[5].Quantity1.ParseForABI(12, 2) : 0; }
		}

		public override ZString SecondaryLine5CustomsUnitQty
		{
			get { return SecondaryTariffLine5 ? aens50TariffList[5].UnitOfMeasureCode1 : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine5TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine5 ? aens50TariffList[5].ValueOfGoodsAmount.Round(0) : ZDecimal.Zero; }
		}

		public override ZDecimal SecondaryLine5DutyAmount
		{
			get
			{
				var result = ZDecimal.Zero;

				if (SecondaryTariffLine5)
				{
					if (IsTIBEntry)
					{
						if (childLinesDocData != null && childLinesDocData.Length > 4 && childLinesDocData[4].US_SecondaryLineTIBCalculatedDutyAmount > 0)
						{
							result = childLinesDocData[4].US_SecondaryLineTIBCalculatedDutyAmount;
						}
					}
					else if (childLinesDocData.Length > 4 && childLinesDocData[4].US_IsAdditionalTotalPrintingDuty)
					{
						result = childLinesDocData[4].US_DutyAmount;
					}
					else if (docData.US_SecondaryLine5DutyAmount > 0)
					{
						result = docData.US_SecondaryLine5DutyAmount;
					}
					else
					{
						result = aens50TariffList[5].DutyAmount;
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine5DutyPercentAsString
		{
			get { return dutyPercentageStrings[5]; }
		}

		public override ZBool PrintSPIOnSecondLine5 => PrintSPIOnSecondaryLine(SecondaryTariffLine5 ? aens50TariffList[5].HTSNumber : ZString.Empty);

		#endregion

		#region Secondary Line 6

		public override ZString SecondaryLine6Description
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine6)
				{
					var uscTariff = new USCTariff.Loader(Factory).LoadBestMatch(aens50TariffList[6].HTSNumber, DateForDutyCalculation);
					if (uscTariff != null)
					{
						result = uscTariff.UE_ShortDescription;
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine6FormattedTariff
		{
			get { return SecondaryTariffLine6 ? TariffFormatter.DisplayFormat(aens50TariffList[6].HTSNumber) : ""; }
		}

		public override ZString SecondaryLine6SecondQtyAndUQ
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine6)
				{
					result = GetQuantityAndUnit(aens50TariffList[6].Quantity2.ParseForABI(12, 2), aens50TariffList[6].UnitOfMeasureCode2);
				}

				return result;
			}
		}

		public override ZString SecondaryLine6ThirdQtyAndUQ
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine6)
				{
					result = GetQuantityAndUnit(aens50TariffList[6].Quantity3.ParseForABI(12, 2), aens50TariffList[6].UnitOfMeasureCode3);
				}

				return result;
			}
		}

		#region Secondary Line6 countervailing & anti dumping duty
		/// <summary>
		/// Cannot send ADD / CVD details electronically for other than 1 secondary line.
		/// All subsequent values when printing from message will be empty.
		/// </summary>

		public override ZString SecondaryLine6ADDNo
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine6ADDSurety
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine6ADDSpecificDepositValueFormatted
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine6ADDRate
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine6ADDFormatted
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine6CVDNo
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine6CVDSurety
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine6CVDSpecificDepositValueFormatted
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine6CVDRate
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine6CVDFormatted
		{
			get { return ZString.Empty; }
		}

		#endregion

		public override ZDecimal SecondaryLine6CustomsQuantity
		{
			get { return SecondaryTariffLine6 ? aens50TariffList[6].Quantity1.ParseForABI(12, 2) : 0; }
		}

		public override ZString SecondaryLine6CustomsUnitQty
		{
			get { return SecondaryTariffLine6 ? aens50TariffList[6].UnitOfMeasureCode1 : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine6TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine6 ? aens50TariffList[6].ValueOfGoodsAmount.Round(0) : ZDecimal.Zero; }
		}

		public override ZString SecondaryLine6DutyPercentAsString
		{
			get { return dutyPercentageStrings[6]; }
		}

		public override ZDecimal SecondaryLine6DutyAmount
		{
			get
			{
				var result = ZDecimal.Zero;
				if (SecondaryTariffLine6)
				{
					if (IsTIBEntry)
					{
						if (childLinesDocData != null && childLinesDocData.Length > 5 && childLinesDocData[5].US_SecondaryLineTIBCalculatedDutyAmount > 0)
						{
							result = childLinesDocData[5].US_SecondaryLineTIBCalculatedDutyAmount;
						}
					}
					else if (childLinesDocData.Length > 5 && childLinesDocData[5].US_IsAdditionalTotalPrintingDuty)
					{
						result = childLinesDocData[5].US_DutyAmount;
					}
					else
					{
						result = aens50TariffList[6].DutyAmount;
					}
				}
				return result;
			}
		}

		public override ZBool PrintSPIOnSecondLine6 => PrintSPIOnSecondaryLine(SecondaryTariffLine6 ? aens50TariffList[6].HTSNumber : ZString.Empty);

		#endregion

		#region Secondary Line 7

		public override ZString SecondaryLine7Description
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine7)
				{
					USCTariff uscTariff = new USCTariff.Loader(Factory).LoadBestMatch(aens50TariffList[7].HTSNumber, DateForDutyCalculation);

					if (uscTariff != null)
					{
						result = uscTariff.UE_ShortDescription;
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine7FormattedTariff
		{
			get { return SecondaryTariffLine7 ? TariffFormatter.DisplayFormat(aens50TariffList[7].HTSNumber) : ""; }
		}

		public override ZString SecondaryLine7SecondQtyAndUQ
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine7)
				{
					result = GetQuantityAndUnit(aens50TariffList[7].Quantity2.ParseForABI(12, 2), aens50TariffList[7].UnitOfMeasureCode2);
				}

				return result;
			}
		}

		public override ZString SecondaryLine7ThirdQtyAndUQ
		{
			get
			{
				var result = ZString.Empty;

				if (SecondaryTariffLine7)
				{
					result = GetQuantityAndUnit(aens50TariffList[7].Quantity3.ParseForABI(12, 2), aens50TariffList[7].UnitOfMeasureCode3);
				}

				return result;
			}
		}

		#region Secondary Line7 countervailing & anti dumping duty
		/// <summary>
		/// Cannot send ADD / CVD details electronically for other than 1 secondary line.
		/// All subsequent values when printing from message will be empty.
		/// </summary>

		public override ZString SecondaryLine7ADDNo
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine7ADDSurety
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine7ADDSpecificDepositValueFormatted
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine7ADDRate
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine7ADDFormatted
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine7CVDNo
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine7CVDSurety
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine7CVDSpecificDepositValueFormatted
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine7CVDRate
		{
			get { return ZString.Empty; }
		}

		public override ZString SecondaryLine7CVDFormatted
		{
			get { return ZString.Empty; }
		}

		#endregion

		public override ZDecimal SecondaryLine7CustomsQuantity
		{
			get { return SecondaryTariffLine7 ? aens50TariffList[7].Quantity1.ParseForABI(12, 2) : 0; }
		}

		public override ZString SecondaryLine7CustomsUnitQty
		{
			get { return SecondaryTariffLine7 ? aens50TariffList[7].UnitOfMeasureCode1 : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine7TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine7 ? aens50TariffList[7].ValueOfGoodsAmount.Round(0) : ZDecimal.Zero; }
		}

		public override ZDecimal SecondaryLine7DutyAmount
		{
			get
			{
				var result = ZDecimal.Zero;

				if (SecondaryTariffLine7)
				{
					if (IsTIBEntry)
					{
						if (childLinesDocData != null && childLinesDocData.Length > 6 && childLinesDocData[6].US_SecondaryLineTIBCalculatedDutyAmount > 0)
						{
							result = childLinesDocData[6].US_SecondaryLineTIBCalculatedDutyAmount;
						}
					}
					else if (childLinesDocData.Length > 6 && childLinesDocData[6].US_IsAdditionalTotalPrintingDuty)
					{
						result = childLinesDocData[6].US_DutyAmount;
					}
					else if (docData.US_SecondaryLine7DutyAmount > 0)
					{
						result = docData.US_SecondaryLine7DutyAmount;
					}
					else
					{
						result = aens50TariffList[7].DutyAmount;
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine7DutyPercentAsString
		{
			get { return dutyPercentageStrings[7]; }
		}

		public override ZBool PrintSPIOnSecondLine7 => PrintSPIOnSecondaryLine(SecondaryTariffLine7 ? aens50TariffList[7].HTSNumber : ZString.Empty);

		#endregion

		ZBool PrintSPIOnSecondaryLine(ZString tariffNumber)
		{
			return PrintSPIOnSecondLine && !tariffNumber.IsEmpty && !Chapter98Helper.Is98Tariff(tariffNumber) && !Chapter98Helper.Is99Tariff(tariffNumber);
		}

		#endregion

		#endregion
	}
}
