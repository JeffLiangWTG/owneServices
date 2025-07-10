using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	public class EntryMessageENS7501Line : EntrySummary7501Line, IObsoleteValidation
	{
		public EntryMessageENS7501Line(EntryMessageLine entryMessageLine, ZBool multiOrigin, ZBool multiExport, ZBool multiLading, ZBool multiManufacturer)
			: this(entryMessageLine, multiOrigin, multiExport, multiLading, multiManufacturer, false, false, false, false, ZString.Empty, null, null, null, ZString.Empty)
		{
		}

		public EntryMessageENS7501Line(EntryMessageLine entryMessageLine, ZBool multiOrigin, ZBool multiExport, ZBool multiLading, ZBool multiManufacturer, ZBool multiRelationship, bool hasMultipleExportDates, ZBool printInvoiceHeading, ZBool printInvoiceDetails, ZString aDDCVDSuretyCode, ZString[] dutyPercentageStrings, US7501DocPrinting docData, US7501DocPrinting[] childLinesDocData, ZString entryType)
			: base(entryMessageLine.Factory, entryType)
		{
			this.ens40 = entryMessageLine.ens40;
			this.ens43RulingsList = entryMessageLine.ens43;
			this.ens50 = entryMessageLine.ens50;
			this.ens51 = entryMessageLine.ens51;
			if (ens51 == null)
			{
				ens51 = new ENS51();
			}

			this.ens52 = entryMessageLine.ens52;
			this.ens60 = entryMessageLine.ens60;
			this.ens62ChargesList = entryMessageLine.ens62;
			this.ens70 = entryMessageLine.ens70;
			this.ens80 = entryMessageLine.ens80;
			this.ens81AdditionalSecondaryTariffList = entryMessageLine.ens81;
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

		readonly ENS40 ens40;
		readonly List<ENS43> ens43RulingsList;
		readonly ENS50 ens50;
		readonly ENS51 ens51;
		readonly ENS52 ens52;
		readonly ENS60 ens60;
		readonly List<ENS62> ens62ChargesList;
		readonly ENS70 ens70;
		readonly ENS80 ens80;
		readonly List<ENS81> ens81AdditionalSecondaryTariffList;

		readonly ZBool multiOrigin;
		readonly ZBool multiExport;
		readonly ZBool multiLading;
		readonly ZBool multiManufacturer;
		readonly ZBool multiRelationship;
		readonly ZBool multiExportDate;
		readonly ZString aDDCVDSuretyCode;
		readonly ZString[] dutyPercentageStrings;
		readonly US7501DocPrinting docData;
		readonly US7501DocPrinting[] childLinesDocData;

		USCTariff ImportTariff1
		{
			get { return importTariff1 ?? (importTariff1 = new USCTariff.Loader(Factory).LoadBestMatch(ens50.TariffNumber1, DateForDutyCalculation)); }
		}
		USCTariff importTariff1;

		USCTariff ImportTariff2
		{
			get
			{
				if (importTariff2 == null && SecondaryTariffLine)
				{
					importTariff2 = new USCTariff.Loader(Factory).LoadBestMatch(ens70.TariffNumber2, DateForDutyCalculation);
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
					importTariff3 = new USCTariff.Loader(Factory).LoadBestMatch(ens80.TariffNumber3, DateForDutyCalculation);
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

		protected override ZDate ExportDateCore
		{
			get { return multiExportDate ? ens50.DateOfExportation : ZDate.Empty; }
		}

		#region EntrySummary7501Line Methods

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

		public override ZString ExclusionNumber
		{
			get { return ZString.Empty; }
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
					result = ens50.RelatedPartyIndicator;
				}

				return result;
			}
		}

		public override ZString LineNumber
		{
			get { return ens40.LineItemNumber.ToString(); }
		}

		public override ZBool SecondaryTariffLine
		{
			get { return ens70 != null; }
		}

		public override ZBool SecondaryTariffLine2
		{
			get { return ens80 != null; }
		}

		public override ZBool SecondaryTariffLine3
		{
			get { return ens81AdditionalSecondaryTariffList.Count > 0; }
		}

		public override ZBool SecondaryTariffLine4
		{
			get { return ens81AdditionalSecondaryTariffList.Count > 1; }
		}

		public override ZBool SecondaryTariffLine5
		{
			get { return ens81AdditionalSecondaryTariffList.Count > 2; }
		}

		public override ZBool SecondaryTariffLine6
		{
			get { return ens81AdditionalSecondaryTariffList.Count > 3; }
		}

		public override ZBool SecondaryTariffLine7
		{
			get { return ens81AdditionalSecondaryTariffList.Count > 4; }
		}

		public override ZString Description
		{
			get { return ImportTariff1 != null ? ImportTariff1.UE_ShortDescription : ZString.Empty; }
		}

		public override ZString SPIAndOrSecondarySPI
		{
			get
			{
				ZString result = ZString.Empty;

				if (!ens50.SpecialProgramsIndicatorCountry.IsEmpty)
				{
					result = ens50.SpecialProgramsIndicatorCountry;
				}
				else if (!ens50.SpecialProgramsIndicatorPrimary.IsEmpty)
				{
					result = ens50.SpecialProgramsIndicatorPrimary;
				}

				if (!ens50.SpecialProgramsIndicatorSecondary.IsEmpty)
				{
					result = result.IsEmpty ? ens50.SpecialProgramsIndicatorSecondary.ToString() : result + "." + ens50.SpecialProgramsIndicatorSecondary;
				}

				return result;
			}
		}

		public override ZString CountryOfOriginForLine
		{
			get { return multiOrigin ? "O," + ens40.CountryOfOrigin : ""; }
		}

		public override ZString CountryOfExportForLine
		{
			get { return multiExport ? "E," + ens50.CountryOfExport : ""; }
		}

		public override ZString ExportDateForTextile
		{
			get
			{
				ZString result = ZString.Empty;

				if (!ens51.DateOfExportationTextiles.IsEmpty)
				{
					result = "D/E " + ens51.DateOfExportationTextiles.ToString("MM/dd/yyyy");
				}

				return result;
			}
		}

		public override ZString PortOfLadingForLine
		{
			get { return multiLading ? ens40.PortOfLading : ZString.Empty; }
		}

		public override ZString FormattedTariff
		{
			get { return ens50.TariffNumber1.Insert(6, ".").Insert(4, "."); }
		}

		public override ZDecimal GrossWeightInKilograms
		{
			get { return ens40.GrossWeight; }
		}

		public override ZDecimal CustomsQuantity
		{
			get { return ens50.Quantity1; }
		}

		public override ZString CustomsUnitQty
		{
			get { return ens50.UnitOfMeasure1; }
		}

		public override ZDecimal TotalLinePriceInLocalCurrencyRounded
		{
			get { return ens40.Value.Round(0); }
		}

		public override ZString ChargesRounded
		{
			get
			{
				ZString chargesValue = ZString.Empty;
				if (ens40.Charges > 0)
				{
					chargesValue = "C" + ens40.Charges.ToString();
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
			get { return GetQuantityAndUnit(ens50.Quantity2, ens50.UnitOfMeasure2); }
		}

		public override ZString ThirdQtyAndUQ
		{
			get { return GetQuantityAndUnit(ens50.Quantity3, ens50.UnitOfMeasure3); }
		}

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
				ZString result = ZString.Empty;

				if (!ens60.AntidumpingCaseNumber.IsEmpty && ADDOnParentLine)
				{
					result = ens60.AntidumpingCaseNumber;
				}

				if (!result.IsEmpty)
				{
					result = result.StartsWith("A") ? result.ToString() : "A" + result.ToString();
					result = (result.SubstringSafe(0, 4) + "-" + result.SubstringSafe(4, 3) + "-" + result.SubstringSafe(7, 3)).TrimEnd('-');
				}

				return result;
			}
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
				ZString result = ZString.Empty;
				if (ADDOnParentLine && ens40.ADDSpecificDepositValue > 0 && ens40.ADDSpecificDepositValue != TotalLinePriceInLocalCurrencyRounded)
				{
					result = "(" + ens40.ADDSpecificDepositValue + ")";
				}

				return result;
			}
		}

		public override ZString ADDRate
		{
			get
			{
				return ADDOnParentLine ? DepositRateIndicatorList.GetAdValoremRateDescription(ens60.AntidumpingCaseNumber, ens60.ADDDepositRate) : "";
			}
		}

		public override ZString ADDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (ADDOnParentLine && ens60.AntidumpingDuty > 0)
				{
					if (ens60.BondedADDIndicator == "1")
					{
						result = "(" + ens60.AntidumpingDuty.ToString(2) + ")";
					}
					else
					{
						result = ens60.AntidumpingDuty.ToString(2);
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

				if (CVDOnParentLine && !ens60.CountervailingCaseNumber.IsEmpty)
				{
					result = ens60.CountervailingCaseNumber;
				}
				if (!result.IsEmpty)
				{
					result = result.StartsWith("C") ? result.ToString() : "C" + result.ToString();
					result = (result.SubstringSafe(0, 4) + "-" + result.SubstringSafe(4, 3) + "-" + result.SubstringSafe(7, 3)).TrimEnd('-');
				}

				return result;
			}
		}

		public override ZString CVDSurety
		{
			get { return CVDOnParentLine && CVDNo.IsEmpty ? ZString.Empty : ADDCVDSuretyValue; }
		}

		public override ZString CVDSpecificDepositValueFormatted
		{
			get
			{
				ZString result = ZString.Empty;
				if (CVDOnParentLine && ens40.CVDSpecificDepositValue > 0 && ens40.CVDSpecificDepositValue != TotalLinePriceInLocalCurrencyRounded)
				{
					result = "(" + ens40.CVDSpecificDepositValue + ")";
				}

				return result;
			}
		}

		public override ZString CVDRate
		{
			get
			{
				return CVDOnParentLine ? DepositRateIndicatorList.GetAdValoremRateDescription(ens60.CountervailingCaseNumber, ens60.CVDDepositRate) : "";
			}
		}

		public override ZString CVDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (CVDOnParentLine && ens60.CountervailingDuty > 0)
				{
					if (ens60.BondedCVDIndicator == "1")
					{
						result = "(" + ens60.CountervailingDuty.ToString(2) + ")";
					}
					else
					{
						result = ens60.CountervailingDuty.ToString(2);
					}
				}

				return result;
			}
		}

		public override ZDecimal LumberExportPrice
		{
			get { return ens52 != null ? ens52.OtherDataIndicator1 == "01" ? Convert.ToDecimal(ens52.OtherDataElement1) : 0 : 0; }
		}

		public override ZString LumberImporterDeclaration
		{
			get { return ens52 != null ? ens52.OtherDataIndicator2 == "01" ? ens52.OtherDataElement2.SubstringSafe(0, 1) : ZString.Empty : ZString.Empty; }
		}

		public override ZDecimal LumberExportCharges
		{
			get { return ens52 != null ? ens52.OtherDataIndicator2 == "01" ? Convert.ToDecimal(ens52.OtherDataElement2.SubstringSafe(1)) : 0 : 0; }
		}

		public override ZDecimal DutyAmount
		{
			get { return ens50.Duty; }
		}

		protected override bool USComponentsAssembledAbroad
		{
			get { return ImportTariff1 != null && ImportTariff1.Applies(TariffRuleList.Codes.AssembledAbroadOfUSProducts, DateForDutyCalculation); }
		}

		public override ZString VisaCertificateNumber
		{
			get
			{
				ZString result = ZString.Empty;

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
			get { return ens51 != null ? !ens51.VisaNumber.IsEmpty ? "V " + ens51.VisaNumber : "" : ""; }
		}

		ZString CottonCertificateNumberOrganicExemptionCertificateNumber
		{
			get { return ens51 != null ? !ens51.CottonCertificateNumberOrganicExemptionCertificateNumber.IsEmpty ? "C " + ens51.CottonCertificateNumberOrganicExemptionCertificateNumber : "" : ""; }
		}

		ZString CanadianExportCertificateSugar
		{
			get { return ens52 != null ? !ens52.CanadianExportCertificateSugar.IsEmpty ? "C " + ens52.CanadianExportCertificateSugar : "" : ""; }
		}

		ZString CBTPACertificationNumber
		{
			get { return ens52 != null ? !ens52.CBTPACertificationNumber.IsEmpty ? "C " + ens52.CBTPACertificationNumber : "" : ""; }
		}

		public override ZString LicenseNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (!AgricultureLicenseNumber.IsEmpty)
				{
					result = AgricultureLicenseNumber;
				}
				else if (!MiscellaneousPermitLicenseNumber.IsEmpty)
				{
					result = MiscellaneousPermitLicenseNumber;
				}
				else if (!WoolLicense.IsEmpty)
				{
					result = WoolLicense;
				}

				return result;
			}
		}

		ZString AgricultureLicenseNumber
		{
			get { return ens51 != null ? ens51.AgricultureLicenseNumber : ZString.Empty; }
		}

		ZString MiscellaneousPermitLicenseNumber
		{
			get { return ens52 != null ? ens52.MiscellaneousPermitLicenseNumber : ZString.Empty; }
		}

		ZString WoolLicense
		{
			get { return ens52 != null ? ens52.WoolLicense : ZString.Empty; }
		}

		#region Block29Elements

		public override ZString TextileCategoryNumberWithLabel
		{
			get { return ens51 != null ? !ens51.CategoryNumber.IsEmpty ? "CAT " + ens51.CategoryNumber : "" : ""; }
		}

		public override ZString LineLevelManufacturerIDWithLabel
		{
			get
			{
				ZString result = ZString.Empty;

				if (multiManufacturer) //If so, it needs to print at line level
				{
					ZString manufacturerCode = ens60.ManufacturerSupplierCode;
					if (!manufacturerCode.IsEmpty)
					{
						result = "MID " + manufacturerCode;
					}
				}

				return result;
			}
		}

		public override ZString BindingRulingWithLabel
		{
			get
			{
				ZString result = ZString.Empty;

				if (ens43RulingsList != null)
				{
					foreach (ENS43 rulingDetail in ens43RulingsList)
					{
						if (!rulingDetail.PreImportationReviewProgramPIRPRulingsNumber.IsEmpty)
						{
							if (rulingDetail.TypeIndicator == PIRPRulingTypeList.Codes.BindingRulings)
							{
								result = "RLNG " + rulingDetail.PreImportationReviewProgramPIRPRulingsNumber;
								break;
							}
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
							if (invoiceLine.Declaration != null && invoiceLine.Declaration.ShouldPrintProductNumber7501)
							{
								block29Elements.Add(invoiceLine.JI_PartNo);
							}
							block29Elements.Add(invoiceLine.JI_CustomAttrib1);
							block29Elements.Add(invoiceLine.JI_CustomAttrib2);
							block29Elements.Add(invoiceLine.JI_CustomAttrib3);
						}
					}
				}

				return block29Elements;
			}
		}

		#endregion

		#region Fees

		ENS62 ChargeAmountForCode(ZString chargeCode)
		{
			if (ens62ChargesList != null)
			{
				foreach (ENS62 chargeDetail in ens62ChargesList)
				{
					string chargeDetailCode = chargeDetail.ClassCode.ToString().PadLeft(3, '0');
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

			if (ens62ChargesList != null)
			{
				foreach (ENS62 ens62 in ens62ChargesList)
				{
					if (ens62.UserFeeAmount > 0 && CusFeeCodeConstants.GetAccountingClassFeeCodeList(Factory).ContainsCode(ens62.ClassCode))
					{
						result.Add(ens62.ClassCode, ens62.UserFeeAmount);
					}
				}
			}

			if (ens60 != null && ens60.InternalRevenueServiceIRSTax > 0)
			{
				foreach (string taxCode in CusFeeCodeConstants.GetTaxCodes())
				{
					if (GetDocPrintingDataMatching(taxCode) != null)
					{
						result.Add(taxCode, ens60.InternalRevenueServiceIRSTax);
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
			US7501DocPrinting docData = GetDocPrintingDataMatching(feeCode);
			return docData != null ? docData.US_FEEPercentAsString : ZString.Empty;
		}

		US7501DocPrinting GetDocPrintingDataMatching(ZString feeCode)
		{
			US7501DocPrinting result = docData;

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
				ZString result = ZString.Empty;

				if (SecondaryTariffLine)
				{
					result = ImportTariff2 != null ? ImportTariff2.UE_ShortDescription : ZString.Empty;
				}

				return result;
			}
		}

		public override ZString SecondaryLine1FormattedTariff
		{
			get { return SecondaryTariffLine ? ens70.TariffNumber2.Insert(6, ".").Insert(4, ".") : ""; }
		}

		public override ZString SecondaryLine1SecondQtyAndUQ
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine)
				{
					result = GetQuantityAndUnit(ens70.Quantity2, ens70.Unit2);
				}

				return result;
			}
		}

		public override ZString SecondaryLine1ThirdQtyAndUQ
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine)
				{
					result = GetQuantityAndUnit(ens70.Quantity3, ens70.Unit3);
				}

				return result;
			}
		}

		#region Secondary line countervailing & anti dumping duty
		/// <summary>
		/// Secondary line fields are not available for ADD & CVD in the message, but values do go in Totals.
		/// These details even if entered on the secondary line are sent in the ens40 & ens60 blocks of the set
		/// </summary>

		bool ADDOnSecondaryLine
		{
			get { return !SecondaryLine1ADDNo.IsEmpty; }
		}

		public override ZString SecondaryLine1ADDNo
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine)
				{
					if (ADDOnChildLine && !ens60.AntidumpingCaseNumber.IsEmpty)
					{
						result = ens60.AntidumpingCaseNumber;
					}

					if (!result.IsEmpty)
					{
						result = result.StartsWith("A") ? result.ToString() : "A" + result.ToString();
						result = (result.SubstringSafe(0, 4) + "-" + result.SubstringSafe(4, 3) + "-" + result.SubstringSafe(7, 3)).TrimEnd('-');
					}
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

				if (ADDOnSecondaryLine)
				{
					if (ens40.ADDSpecificDepositValue > 0 & ens40.ADDSpecificDepositValue != TotalLinePriceInLocalCurrencyRounded)
					{
						result = "(" + ens40.ADDSpecificDepositValue + ")";
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine1ADDRate
		{
			get
			{
				return ADDOnSecondaryLine ? DepositRateIndicatorList.GetAdValoremRateDescription(ens60.AntidumpingCaseNumber, ens60.ADDDepositRate) : "";
			}
		}

		public override ZString SecondaryLine1ADDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (ADDOnSecondaryLine && ens60.AntidumpingDuty > 0)
				{
					if (ens60.BondedADDIndicator == "1")
					{
						result = "(" + ens60.AntidumpingDuty.ToString(2) + ")";
					}
					else
					{
						result = ens60.AntidumpingDuty.ToString(2);
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
				ZString result = ZString.Empty;

				if (SecondaryTariffLine)
				{
					if (CVDOnChildLine && !ens60.CountervailingCaseNumber.IsEmpty)
					{
						result = ens60.CountervailingCaseNumber;
					}

					if (!result.IsEmpty)
					{
						result = result.StartsWith("C") ? result.ToString() : "C" + result.ToString();
						result = (result.SubstringSafe(0, 4) + "-" + result.SubstringSafe(4, 3) + "-" + result.SubstringSafe(7, 3)).TrimEnd('-');
					}
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
				ZString result = ZString.Empty;

				if (CVDOnSecondaryLine)
				{
					if (ens40.CVDSpecificDepositValue > 0 & ens40.CVDSpecificDepositValue != TotalLinePriceInLocalCurrencyRounded)
					{
						result = "(" + ens40.CVDSpecificDepositValue + ")";
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine1CVDRate
		{
			get
			{
				return CVDOnSecondaryLine ? DepositRateIndicatorList.GetAdValoremRateDescription(ens60.CountervailingCaseNumber, ens60.CVDDepositRate) : "";
			}
		}

		public override ZString SecondaryLine1CVDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (CVDOnSecondaryLine && ens60.CountervailingDuty > 0)
				{
					if (ens60.BondedCVDIndicator == "1")
					{
						result = "(" + ens60.CountervailingDuty.ToString(2) + ")";
					}
					else
					{
						result = ens60.CountervailingDuty.ToString(2);
					}
				}

				return result;
			}
		}

		#endregion

		public override ZDecimal SecondaryLine1CustomsQuantity
		{
			get { return SecondaryTariffLine ? ens70.Quantity1 : 0; }
		}

		public override ZString SecondaryLine1CustomsUnitQty
		{
			get { return SecondaryTariffLine ? ens70.Unit1 : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine1TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine ? ens70.Value.Round(0) : ZDecimal.Zero; }
		}

		public override ZDecimal SecondaryLine1DutyAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (SecondaryTariffLine)
				{
					if (IsTIBEntry)
					{
						if (childLinesDocData != null && childLinesDocData.Length > 0 && childLinesDocData[0].US_SecondaryLineTIBCalculatedDutyAmount > 0)
						{
							result = childLinesDocData[0].US_SecondaryLineTIBCalculatedDutyAmount;
						}
					}
					else if (docData.US_SecondaryLine1DutyAmount > 0)
					{
						result = docData.US_SecondaryLine1DutyAmount;
					}
					else
					{
						result = ens70.Duty;
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine1DutyPercentAsString
		{
			get { return dutyPercentageStrings[1]; }
		}

		#endregion

		#region Secondary Line 2

		public override ZString SecondaryLine2Description
		{
			get
			{
				ZString result = ZString.Empty;

				if (ens80 != null)
				{
					result = ImportTariff3 != null ? ImportTariff3.UE_ShortDescription : ZString.Empty;
				}

				return result;
			}
		}

		public override ZString SecondaryLine2FormattedTariff
		{
			get { return ens80 != null ? ens80.TariffNumber3.Insert(6, ".").Insert(4, ".") : ""; }
		}

		public override ZString SecondaryLine2SecondQtyAndUQ
		{
			get
			{
				ZString result = ZString.Empty;

				if (ens80 != null)
				{
					result = GetQuantityAndUnit(ens80.Quantity2, ens80.Unit2);
				}

				return result;
			}
		}

		public override ZString SecondaryLine2ThirdQtyAndUQ
		{
			get
			{
				ZString result = ZString.Empty;

				if (ens80 != null)
				{
					result = GetQuantityAndUnit(ens80.Quantity3, ens80.Unit3);
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
			get { return ens80 != null ? ens80.Quantity1 : 0; }
		}

		public override ZString SecondaryLine2CustomsUnitQty
		{
			get { return ens80 != null ? ens80.Unit1 : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine2TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine2 ? ens80.Value.Round(0) : ZDecimal.Zero; }
		}

		public override ZString SecondaryLine2DutyPercentAsString
		{
			get { return dutyPercentageStrings[2]; }
		}

		public override ZDecimal SecondaryLine2DutyAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (SecondaryTariffLine2)
				{
					if (IsTIBEntry)
					{
						if (childLinesDocData != null && childLinesDocData.Length > 1 && childLinesDocData[1].US_SecondaryLineTIBCalculatedDutyAmount > 0)
						{
							result = childLinesDocData[1].US_SecondaryLineTIBCalculatedDutyAmount;
						}
					}
					else
					{
						result = ens80 != null ? ens80.Duty : 0;
					}
				}

				return result;
			}
		}

		#endregion

		#region Secondary Line 3

		public override ZString SecondaryLine3Description
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine3)
				{
					USCTariff uscTariff = new USCTariff.Loader(Factory).LoadBestMatch(ens81AdditionalSecondaryTariffList[0].AdditionalTariffNumber, DateForDutyCalculation);
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
			get { return SecondaryTariffLine3 ? ens81AdditionalSecondaryTariffList[0].AdditionalTariffNumber.Insert(6, ".").Insert(4, ".") : ""; }
		}

		public override ZString SecondaryLine3SecondQtyAndUQ
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine3)
				{
					result = GetQuantityAndUnit(ens81AdditionalSecondaryTariffList[0].Quantity2, ens81AdditionalSecondaryTariffList[0].Unit2);
				}

				return result;
			}
		}

		public override ZString SecondaryLine3ThirdQtyAndUQ
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine3)
				{
					result = GetQuantityAndUnit(ens81AdditionalSecondaryTariffList[0].Quantity3, ens81AdditionalSecondaryTariffList[0].Unit3);
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
			get { return SecondaryTariffLine3 ? ens81AdditionalSecondaryTariffList[0].Quantity1 : 0; }
		}

		public override ZString SecondaryLine3CustomsUnitQty
		{
			get { return SecondaryTariffLine3 ? ens81AdditionalSecondaryTariffList[0].Unit1 : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine3TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine3 ? ens81AdditionalSecondaryTariffList[0].Value.Round(0) : ZDecimal.Zero; }
		}

		public override ZDecimal SecondaryLine3DutyAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (SecondaryTariffLine3)
				{
					if (IsTIBEntry)
					{
						if (childLinesDocData != null && childLinesDocData.Length > 2 && childLinesDocData[2].US_SecondaryLineTIBCalculatedDutyAmount > 0)
						{
							result = childLinesDocData[2].US_SecondaryLineTIBCalculatedDutyAmount;
						}
					}
					else if (docData.US_SecondaryLine3DutyAmount > 0)
					{
						result = docData.US_SecondaryLine3DutyAmount;
					}
					else
					{
						result = ens81AdditionalSecondaryTariffList[0].Duty;
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine3DutyPercentAsString
		{
			get { return dutyPercentageStrings[3]; }
		}

		#endregion

		#region Secondary Line 4

		public override ZString SecondaryLine4Description
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine4)
				{
					USCTariff uscTariff = new USCTariff.Loader(Factory).LoadBestMatch(ens81AdditionalSecondaryTariffList[1].AdditionalTariffNumber, DateForDutyCalculation);
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
			get { return SecondaryTariffLine4 ? ens81AdditionalSecondaryTariffList[1].AdditionalTariffNumber.Insert(6, ".").Insert(4, ".") : ""; }
		}

		public override ZString SecondaryLine4SecondQtyAndUQ
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine4)
				{
					result = GetQuantityAndUnit(ens81AdditionalSecondaryTariffList[1].Quantity2, ens81AdditionalSecondaryTariffList[1].Unit2);
				}

				return result;
			}
		}

		public override ZString SecondaryLine4ThirdQtyAndUQ
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine4)
				{
					result = GetQuantityAndUnit(ens81AdditionalSecondaryTariffList[1].Quantity3, ens81AdditionalSecondaryTariffList[1].Unit3);
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
			get { return SecondaryTariffLine4 ? ens81AdditionalSecondaryTariffList[1].Quantity1 : 0; }
		}

		public override ZString SecondaryLine4CustomsUnitQty
		{
			get { return SecondaryTariffLine4 ? ens81AdditionalSecondaryTariffList[1].Unit1 : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine4TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine4 ? ens81AdditionalSecondaryTariffList[1].Value.Round(0) : ZDecimal.Zero; }
		}

		public override ZString SecondaryLine4DutyPercentAsString
		{
			get { return dutyPercentageStrings[4]; }
		}

		public override ZDecimal SecondaryLine4DutyAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (SecondaryTariffLine4)
				{
					if (IsTIBEntry)
					{
						if (childLinesDocData != null && childLinesDocData.Length > 3 && childLinesDocData[3].US_SecondaryLineTIBCalculatedDutyAmount > 0)
						{
							result = childLinesDocData[3].US_SecondaryLineTIBCalculatedDutyAmount;
						}
					}
					else
					{
						result = ens81AdditionalSecondaryTariffList[1].Duty;
					}
				}

				return result;
			}
		}

		#endregion

		#region Secondary Line 5

		public override ZString SecondaryLine5Description
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine5)
				{
					USCTariff uscTariff = new USCTariff.Loader(Factory).LoadBestMatch(ens81AdditionalSecondaryTariffList[2].AdditionalTariffNumber, DateForDutyCalculation);
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
			get { return SecondaryTariffLine5 ? ens81AdditionalSecondaryTariffList[2].AdditionalTariffNumber.Insert(6, ".").Insert(4, ".") : ""; }
		}

		public override ZString SecondaryLine5SecondQtyAndUQ
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine5)
				{
					result = GetQuantityAndUnit(ens81AdditionalSecondaryTariffList[2].Quantity2, ens81AdditionalSecondaryTariffList[2].Unit2);
				}

				return result;
			}
		}

		public override ZString SecondaryLine5ThirdQtyAndUQ
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine5)
				{
					result = GetQuantityAndUnit(ens81AdditionalSecondaryTariffList[2].Quantity3, ens81AdditionalSecondaryTariffList[2].Unit3);
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
			get { return SecondaryTariffLine5 ? ens81AdditionalSecondaryTariffList[2].Quantity1 : 0; }
		}

		public override ZString SecondaryLine5CustomsUnitQty
		{
			get { return SecondaryTariffLine5 ? ens81AdditionalSecondaryTariffList[2].Unit1 : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine5TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine5 ? ens81AdditionalSecondaryTariffList[2].Value.Round(0) : ZDecimal.Zero; }
		}

		public override ZDecimal SecondaryLine5DutyAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (SecondaryTariffLine5)
				{
					if (IsTIBEntry)
					{
						if (childLinesDocData != null && childLinesDocData.Length > 4 && childLinesDocData[4].US_SecondaryLineTIBCalculatedDutyAmount > 0)
						{
							result = childLinesDocData[4].US_SecondaryLineTIBCalculatedDutyAmount;
						}
					}
					else if (docData.US_SecondaryLine5DutyAmount > 0)
					{
						result = docData.US_SecondaryLine5DutyAmount;
					}
					else
					{
						result = ens81AdditionalSecondaryTariffList[2].Duty;
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine5DutyPercentAsString
		{
			get { return dutyPercentageStrings[5]; }
		}

		#endregion

		#region Secondary Line 6

		public override ZString SecondaryLine6Description
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine6)
				{
					USCTariff uscTariff = new USCTariff.Loader(Factory).LoadBestMatch(ens81AdditionalSecondaryTariffList[3].AdditionalTariffNumber, DateForDutyCalculation);
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
			get { return SecondaryTariffLine6 ? ens81AdditionalSecondaryTariffList[3].AdditionalTariffNumber.Insert(6, ".").Insert(4, ".") : ""; }
		}

		public override ZString SecondaryLine6SecondQtyAndUQ
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine6)
				{
					result = GetQuantityAndUnit(ens81AdditionalSecondaryTariffList[3].Quantity2, ens81AdditionalSecondaryTariffList[3].Unit2);
				}

				return result;
			}
		}

		public override ZString SecondaryLine6ThirdQtyAndUQ
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine6)
				{
					result = GetQuantityAndUnit(ens81AdditionalSecondaryTariffList[3].Quantity3, ens81AdditionalSecondaryTariffList[3].Unit3);
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
			get { return SecondaryTariffLine6 ? ens81AdditionalSecondaryTariffList[3].Quantity1 : 0; }
		}

		public override ZString SecondaryLine6CustomsUnitQty
		{
			get { return SecondaryTariffLine6 ? ens81AdditionalSecondaryTariffList[3].Unit1 : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine6TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine6 ? ens81AdditionalSecondaryTariffList[3].Value.Round(0) : ZDecimal.Zero; }
		}

		public override ZString SecondaryLine6DutyPercentAsString
		{
			get { return dutyPercentageStrings[6]; }
		}

		public override ZDecimal SecondaryLine6DutyAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (SecondaryTariffLine6)
				{
					if (IsTIBEntry)
					{
						if (childLinesDocData != null && childLinesDocData.Length > 5 && childLinesDocData[5].US_SecondaryLineTIBCalculatedDutyAmount > 0)
						{
							result = childLinesDocData[5].US_SecondaryLineTIBCalculatedDutyAmount;
						}
					}
					else
					{
						result = ens81AdditionalSecondaryTariffList[3].Duty;
					}
				}

				return result;
			}
		}

		#endregion

		#region Secondary Line 7

		public override ZString SecondaryLine7Description
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine7)
				{
					USCTariff uscTariff = new USCTariff.Loader(Factory).LoadBestMatch(ens81AdditionalSecondaryTariffList[4].AdditionalTariffNumber, DateForDutyCalculation);

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
			get { return SecondaryTariffLine7 ? ens81AdditionalSecondaryTariffList[4].AdditionalTariffNumber.Insert(6, ".").Insert(4, ".") : ""; }
		}

		public override ZString SecondaryLine7SecondQtyAndUQ
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine7)
				{
					result = GetQuantityAndUnit(ens81AdditionalSecondaryTariffList[4].Quantity2, ens81AdditionalSecondaryTariffList[4].Unit2);
				}

				return result;
			}
		}

		public override ZString SecondaryLine7ThirdQtyAndUQ
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine7)
				{
					result = GetQuantityAndUnit(ens81AdditionalSecondaryTariffList[4].Quantity3, ens81AdditionalSecondaryTariffList[4].Unit3);
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
			get { return SecondaryTariffLine7 ? ens81AdditionalSecondaryTariffList[4].Quantity1 : 0; }
		}

		public override ZString SecondaryLine7CustomsUnitQty
		{
			get { return SecondaryTariffLine7 ? ens81AdditionalSecondaryTariffList[4].Unit1 : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine7TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine7 ? ens81AdditionalSecondaryTariffList[4].Value.Round(0) : ZDecimal.Zero; }
		}

		public override ZDecimal SecondaryLine7DutyAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (SecondaryTariffLine7)
				{
					if (IsTIBEntry)
					{
						if (childLinesDocData != null && childLinesDocData.Length > 6 && childLinesDocData[6].US_SecondaryLineTIBCalculatedDutyAmount > 0)
						{
							result = childLinesDocData[6].US_SecondaryLineTIBCalculatedDutyAmount;
						}
					}
					else if (docData.US_SecondaryLine7DutyAmount > 0)
					{
						result = docData.US_SecondaryLine7DutyAmount;
					}
					else
					{
						result = ens81AdditionalSecondaryTariffList[4].Duty;
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine7DutyPercentAsString
		{
			get { return dutyPercentageStrings[7]; }
		}

		#endregion

		#endregion

		#endregion
	}
}
