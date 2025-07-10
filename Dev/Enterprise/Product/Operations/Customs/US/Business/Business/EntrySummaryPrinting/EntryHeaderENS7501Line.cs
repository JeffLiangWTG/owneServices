using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	public abstract class EntryHeaderENS7501Line : EntrySummary7501Line, IObsoleteValidation
	{
		protected EntryHeaderENS7501Line(CusEntryLine line, bool printInvoiceHeading, bool printInvoiceDetails)
			: base(line.Factory, line.Header.EntryType)
		{
			this.line = line;
			this.iCusline = line;
			this.PrintInvoiceHeading = printInvoiceHeading;
			this.PrintInvoiceDetails = printInvoiceDetails;

			var secondaryTariffLines = new List<CusEntryLine>(line.ChildSecondaryEntryLines);
			if (line.IsCombinedLine())
			{
				secondaryTariffLines = secondaryTariffLines.Where(x => !x.CL_AdValoremTariff.IsEmpty).ToList();
				if (line.IsVParentLine)
				{
					var childVLines = line.ChildVLines.Where(x => x.US_SupLine);
					foreach (var child in childVLines)
					{
						secondaryTariffLines.Add(child);
						secondaryTariffLines.AddRange(child.ChildSecondaryEntryLines.Where(x => !x.CL_AdValoremTariff.IsEmpty));
					}
				}
			}

			InitialiseSecondaryTariffLines(secondaryTariffLines);

			this.iSecondaryCusline1 = secondaryTariffLine1;
			this.iSecondaryCusline2 = secondaryTariffLine2;
			this.iSecondaryCusline3 = secondaryTariffLine3;
			this.iSecondaryCusline4 = secondaryTariffLine4;
			this.iSecondaryCusline5 = secondaryTariffLine5;
			this.iSecondaryCusline6 = secondaryTariffLine6;
			this.iSecondaryCusline7 = secondaryTariffLine7;

			RefreshFeesAndTaxes();
		}

		void InitialiseSecondaryTariffLines(List<CusEntryLine> secondaryTariffLines)
		{
			if (secondaryTariffLines != null)
			{
				Func<CusEntryLine, ZBool> shouldPrintEntryLine = (entryLine) => entryLine != null && (entryLine.IsSecondaryTariffLine || entryLine.IsVChildLine);
				var totalLinesCount = secondaryTariffLines.Count;

				this.secondaryTariffLine1 = totalLinesCount > 0 ? shouldPrintEntryLine(secondaryTariffLines[0]) ? secondaryTariffLines[0] : null : null;
				this.secondaryTariffLine2 = totalLinesCount > 1 ? shouldPrintEntryLine(secondaryTariffLines[1]) ? secondaryTariffLines[1] : null : null;
				this.secondaryTariffLine3 = totalLinesCount > 2 ? shouldPrintEntryLine(secondaryTariffLines[2]) ? secondaryTariffLines[2] : null : null;
				this.secondaryTariffLine4 = totalLinesCount > 3 ? shouldPrintEntryLine(secondaryTariffLines[3]) ? secondaryTariffLines[3] : null : null;
				this.secondaryTariffLine5 = totalLinesCount > 4 ? shouldPrintEntryLine(secondaryTariffLines[4]) ? secondaryTariffLines[4] : null : null;
				this.secondaryTariffLine6 = totalLinesCount > 5 ? shouldPrintEntryLine(secondaryTariffLines[5]) ? secondaryTariffLines[5] : null : null;
				this.secondaryTariffLine7 = totalLinesCount > 6 ? shouldPrintEntryLine(secondaryTariffLines[6]) ? secondaryTariffLines[6] : null : null;
			}
		}

		readonly ICusEntryLine iCusline;
		readonly ICusEntryLine iSecondaryCusline1;
		readonly ICusEntryLine iSecondaryCusline2;
		readonly ICusEntryLine iSecondaryCusline3;
		readonly ICusEntryLine iSecondaryCusline4;
		readonly ICusEntryLine iSecondaryCusline5;
		readonly ICusEntryLine iSecondaryCusline6;
		readonly ICusEntryLine iSecondaryCusline7;

		#region EntrySummary7501Line Methods

		public override ZDate DateForAD_CVD
		{
			get { return line.RandomLine.DateForADD_CVD; }
		}

		protected override EntrySummary7501Invoice GetInvoiceDetails()
		{
			return line.RandomLine.InvoiceHeader == null ? null : new EntrySummary7501Invoice(line.RandomLine.InvoiceHeader, line.RandomLine.InvoiceHeader.JZ_InvoiceDisplaySequence, line.Header.HasMixedRelationshipIndicators);
		}

		public override ZString TransRelatedInd
		{
			get { return line.TransRelatedInd; }
		}

		public override ZString LineNumber
		{
			get { return line.CL_LineNumberFormatted; }
		}

		public override ZBool SecondaryTariffLine
		{
			get { return secondaryTariffLine1 != null; }
		}

		public override ZBool SecondaryTariffLine2
		{
			get { return secondaryTariffLine2 != null; }
		}

		public override ZBool SecondaryTariffLine3
		{
			get { return secondaryTariffLine3 != null; }
		}

		public override ZBool SecondaryTariffLine4
		{
			get { return secondaryTariffLine4 != null; }
		}

		public override ZBool SecondaryTariffLine5
		{
			get { return secondaryTariffLine5 != null; }
		}

		public override ZBool SecondaryTariffLine6
		{
			get { return secondaryTariffLine6 != null; }
		}

		public override ZBool SecondaryTariffLine7
		{
			get { return secondaryTariffLine7 != null; }
		}

		public override ZString Description
		{
			get { return TariffDescription(line.CL_AdValoremTariff); }
		}

		public override ZString SPIAndOrSecondarySPI
		{
			get { return line.SPIAndOrSecondarySPI; }
		}

		public override ZString CountryOfOriginForLine
		{
			get { return line.CountryOfOriginForLine; }
		}

		public override ZString CountryOfExportForLine
		{
			get { return line.CountryOfExportForLine; }
		}

		public override ZString ExportDateForTextile
		{
			get
			{
				ZString result = ZString.Empty;

				if (!iCusline.DateOfExportationFromCountryOfOrigin.IsEmpty)
				{
					result = "D/E " + iCusline.DateOfExportationFromCountryOfOrigin.ToString("MM/dd/yyyy");
				}

				return result;
			}
		}

		protected override ZDate ExportDateCore
		{
			get { return line.Header.HasMultiExportDates ? line.ExportDate : ZDate.Empty; }
		}

		public override ZString PortOfLadingForLine
		{
			get { return line.PortOfLadingForLine; }
		}

		public override ZString FormattedTariff
		{
			get { return line.FormattedTariff; }
		}

		public override ZString ExclusionNumber
		{
			get
			{
				var result = new ZStringBuilder();
				if (line.IsCombinedLine())
				{
					var lines = new List<JobComInvoiceLine>();
					var parentLine = line.ParentLine ?? line;
					lines.Add(parentLine.RandomLine);
					foreach (var childLine in parentLine.ChildLines)
					{
						if (!lines.Contains(childLine.RandomLine))
						{
							lines.Add(childLine.RandomLine);
						}
					}
					foreach (var eachLine in lines)
					{
						if (!eachLine.US_ExclusionNumber.IsEmpty)
						{
							result.Append(ProductExclusionTitle + eachLine.US_ExclusionNumber);
						}
					}
				}
				else if (!line.RandomLine?.US_ExclusionNumber.IsEmpty ?? false)
				{
					result.Append(ProductExclusionTitle + line.RandomLine.US_ExclusionNumber);
				}

				return result.ToStringWithNewLineBetweenAppends();
			}
		}
		const string ProductExclusionTitle = "Product Exclusion No: ";

		public override ZDecimal GrossWeightInKilograms
		{
			get { return iCusline.GrossWeightInKilograms; }
		}

		public override ZDecimal CustomsQuantity
		{
			get { return iCusline.Quantity1; }
		}

		public override ZString CustomsUnitQty
		{
			get { return line.CustomsUnitQty; }
		}

		public override ZDecimal TotalLinePriceInLocalCurrencyRounded
		{
			get { return ((Customs.Business.ICusEntryLine)line).CL_CustomsValue; }
		}

		public override ZString ChargesRounded
		{
			get { return line.ChargesRounded; }
		}

		public override ZString DutyPercentAsString
		{
			get
			{
				ZString result = ZString.Empty;
				var dutyData = new EntryLineIEntryLineOrInvoiceLineDutyData(line);
				if (IsTIBEntry)
				{
					if (!line.IsSetVLine)
					{
						var calculator = new AppendixFDutyCalculator(line, Factory);
						result = calculator.DutyResult.RateString;
					}
				}
				else if (result.IsEmpty && CalculateDutyForDocument.ShouldCalculateDutyOnlyForDocumnet(dutyData))
				{
					result = CalculateDutyForDocument.PrinterDuty(dutyData).RateString;
				}

				if (result.IsEmpty)
				{
					result = line.CL_DutyPercentAsString;
				}

				return result;
			}
		}

		public override ZString SecondQtyAndUQ
		{
			get { return GetQuantityAndUnit(iCusline.Quantity2, line.SecondCustomsUnitQty); }
		}

		public override ZString ThirdQtyAndUQ
		{
			get { return GetQuantityAndUnit(iCusline.Quantity3, line.ThirdCustomsUnitQty); }
		}

		protected abstract IACCase GetADCVDCaseRecord(ZString caseNo);

		bool ADDOnParent
		{
			get
			{
				return !line.US_SupLine || GetADCVDCaseRecord(line.RandomLine.US_ADDCaseNo).MatchesTariff(line.RandomLine.US_SupTariff);    // on supplementary lines, JI_Tariff is the child, US_SupTariff is Parent
			}
		}

		public override ZString ADDNo
		{
			// need to go back to source as line ADD details includes secondary lines
			get
			{
				ZString result = ZString.Empty;

				if (line.US_SupLine)
				{
					if (ADDOnParent)
					{
						result = line.ADDNo;
					}
				}
				else if (line.ChildLines.Count > 0)
				{
					if (!line.ADDNo.IsEmpty)
					{
						if (SecondaryLine1ADDNo.IsEmpty)
						{
							result = line.ADDNo;
						}
					}
				}
				else
				{
					result = line.ADDNo;
				}

				return result;
			}
		}

		ZString ADDCVDSuretyValue
		{
			get { return line.Declaration.US_ADDCVDSuretyCode.IsEmpty ? "" : "Surety Code #" + line.Declaration.US_ADDCVDSuretyCode; }
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
				if (!ADDNo.IsEmpty)
				{
					var depositValue = ZDecimal.Zero;
					if (IsTIBEntry)
					{
						depositValue = line.RandomLine.US_ADDDepositValue;
					}
					else
					{
						depositValue = iCusline.ADDSpecificDepositValue;
					}

					if (depositValue > 0 & depositValue != TotalLinePriceInLocalCurrencyRounded)
					{
						result = "(" + depositValue.ToString() + ")";
					}
				}

				return result;
			}
		}

		public override ZString ADDRate
		{
			get { return GetADDRateDescription(line); }
		}

		public override ZString ADDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (!ADDNo.IsEmpty)
				{
					if (iCusline.AntidumpingDuty > 0)
					{
						if (iCusline.BondedAntidumpingDuty)
						{
							result = "(" + iCusline.AntidumpingDuty.ToString(2) + ")";
						}
						else
						{
							result = iCusline.AntidumpingDuty.ToString(2);
						}
					}
					else if (IsTIBEntry)
					{
						ZDecimal antiDumpingDuty = new LineDutyFeeCalculator.ADD_CVDCalculator().CalculateADDDuty(line, new EntryLineIEntryLineOrInvoiceLineDutyData(line).GetADDAdjustValueForCombineLines(), out var percentOfValue);
						result = antiDumpingDuty.Round(2).ToString();
					}
				}

				return result;
			}
		}

		bool CVDOnParent
		{
			get
			{
				return !line.US_SupLine || GetADCVDCaseRecord(line.RandomLine.US_CVDCaseNo).MatchesTariff(line.RandomLine.US_SupTariff);    // on supplementary lines, JI_Tariff is the child, US_SupTariff is Parent
			}
		}

		public override ZString CVDNo
		{
			// need to go back to source as line CVD details includes secondary lines
			get
			{
				ZString result = ZString.Empty;

				if (IsTIBEntry && CVDOnParent)
				{
					result = line.RandomLine.US_CVDCaseNo;
					if (!result.IsEmpty)
					{
						result = result.StartsWith("C") ? result.ToString() : "C" + result.ToString();
						result = (result.SubstringSafe(0, 4) + "-" + result.SubstringSafe(4, 3) + "-" + result.SubstringSafe(7, 3)).TrimEnd('-');
					}
				}
				else
				{
					if (line.US_SupLine)
					{
						if (CVDOnParent)
						{
							result = line.CVDNo;
						}
					}
					else if (line.ChildLines.Count > 0)
					{
						if (!line.CVDNo.IsEmpty)
						{
							if (SecondaryLine1CVDNo.IsEmpty)
							{
								result = line.CVDNo;
							}
						}
					}
					else
					{
						result = line.CVDNo;
					}
				}

				return result;
			}
		}

		public override ZString CVDSurety
		{
			get { return CVDNo.IsEmpty ? ZString.Empty : ADDCVDSuretyValue; }
		}

		public override ZString CVDSpecificDepositValueFormatted
		{
			get
			{
				ZString result = ZString.Empty;
				if (!CVDNo.IsEmpty)
				{
					var depositValue = ZDecimal.Zero;
					if (IsTIBEntry && !line.RandomLine.US_CVDDepositValue.IsEmpty)
					{
						depositValue = line.RandomLine.US_CVDDepositValue;
					}
					else
					{
						depositValue = iCusline.CVDSpecificDepositValue;
					}

					if (depositValue > 0 & depositValue != TotalLinePriceInLocalCurrencyRounded)
					{
						result = "(" + depositValue.ToString() + ")";
					}
				}

				return result;
			}
		}

		public override ZString CVDRate
		{
			get { return GetCVDRateDescription(line); }
		}

		public override ZString CVDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (iCusline.CountervailingDuty > 0)
				{
					if (iCusline.BondedCountervailingDuty)
					{
						result = "(" + iCusline.CountervailingDuty.ToString(2) + ")";
					}
					else
					{
						result = iCusline.CountervailingDuty.ToString(2);
					}
				}
				else if (IsTIBEntry)
				{
					ZDecimal countervailingDuty = new LineDutyFeeCalculator.ADD_CVDCalculator().CalculateCVDDuty(line, new EntryLineIEntryLineOrInvoiceLineDutyData(line).GetCVDAdjustValueForCombineLines(), out var percentOfValue);
					result = countervailingDuty.Round(2).ToString();
				}

				return result;
			}
		}

		public override ZDecimal LumberExportPrice
		{
			get { return iCusline.SoftwoodLumberExportPrice; }
		}

		public override ZString LumberImporterDeclaration
		{
			get { return iCusline.IsSoftwoodLumberImporterDeclaration ? "Y" : ""; }
		}

		public override ZDecimal LumberExportCharges
		{
			get { return iCusline.SoftwoodLumberExportCharges; }
		}

		public override ZDecimal DutyAmount
		{
			get
			{
				var result = ZDecimal.Zero;
				var dutyData = new EntryLineIEntryLineOrInvoiceLineDutyData(line) as IEntryLineOrInvoiceLineDutyData;
				if (Chapter98Helper.HaveTIB9813TariffLine(dutyData))
				{
					var dutyResult = Chapter98Helper.CalculateTIBDutyForPrint(line);
					if (dutyResult != null)
					{
						result = dutyResult.TotalAmount.Amount;
					}
				}
				else
				{
					result = line.DutyAmount;
					if (result.IsEmpty && CalculateDutyForDocument.ShouldCalculateDutyOnlyForDocumnet(dutyData))
					{
						result = CalculateDutyForDocument.PrinterDuty(dutyData).TotalAmount.Amount;
					}
				}
				return result;
			}
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
			get { return !iCusline.VisaNumber.IsEmpty ? "V " + iCusline.VisaNumber : ""; }
		}

		protected virtual ZString CottonCertificateNumberOrganicExemptionCertificateNumber
		{
			get { return !iCusline.CottonCertificateNumberOrganicExemptionCertificateNumber.IsEmpty ? "C " + iCusline.CottonCertificateNumberOrganicExemptionCertificateNumber : ""; }
		}

		protected virtual ZString CanadianExportCertificateSugar
		{
			get { return !iCusline.CanadianExportCertificateSugar.IsEmpty ? "C " + iCusline.CanadianExportCertificateSugar : ""; }
		}

		protected virtual ZString CBTPACertificationNumber
		{
			get { return !iCusline.CBTPACertificationNumber.IsEmpty ? "C " + iCusline.CBTPACertificationNumber : ""; }
		}

		public override ZString LicenseNumber
		{
			get
			{
				if (!licenseNumberCached.HasValue)
				{
					licenseNumberCached = ZString.Empty;

					if (!iCusline.AgricultureLicenseNumber.IsEmpty)
					{
						licenseNumberCached = iCusline.AgricultureLicenseNumber;
					}
					else if (!iCusline.MiscellaneousPermitLicenseNumber.IsEmpty)
					{
						licenseNumberCached = iCusline.MiscellaneousPermitLicenseNumber;
					}
					else if (!iCusline.WoolLicense.IsEmpty)
					{
						licenseNumberCached = iCusline.WoolLicense;
					}
				}
				return licenseNumberCached.Value;
			}
		}
		ZString? licenseNumberCached;

		#region Block29Elements

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

					if (line != null && line.Declaration != null && line.Declaration.ImporterWrapper != null)
					{
						if (line.Declaration.ShouldPrintProductNumber7501)
						{
							if (!line.PartNo.IsEmpty)
							{
								block29Elements.Add(line.PartNo);
							}
						}

						if (!line.CustomAttrib1.IsEmpty && line.Declaration.ImporterWrapper.ZO_ENSPrintCustomAttrib1)
						{
							block29Elements.Add(line.CustomAttrib1);
						}

						if (!line.CustomAttrib2.IsEmpty && line.Declaration.ImporterWrapper.ZO_ENSPrintCustomAttrib2)
						{
							block29Elements.Add(line.CustomAttrib2);
						}

						if (!line.CustomAttrib3.IsEmpty && line.Declaration.ImporterWrapper.ZO_ENSPrintCustomAttrib3)
						{
							block29Elements.Add(line.CustomAttrib3);
						}
					}
				}

				return block29Elements;
			}
		}

		public override ZString TextileCategoryNumberWithLabel
		{
			get { return line.TextileCategoryWithLabel; }
		}

		public override ZString LineLevelManufacturerIDWithLabel
		{
			get { return line.LineLevelManufacturerIDWithLabel; }
		}

		public override ZString BindingRulingWithLabel
		{
			get { return line.BindingRulingWithLabel; }
		}

		#endregion

		#region Fees

		public override ZString LineFeePercentAsString
		{
			get { return GetFeeRate(LineFeeCode, TaxDeferred); }
		}

		protected override ZString LineFeeRateType
		{
			get { return GetRateType(LineFeeCode); }
		}

		protected override Dictionary<ZString, ZDecimal> PopulateLineFeesAndTaxes()
		{
			var result = new Dictionary<ZString, ZDecimal>();

			var fees = new CusEntryLineFeesGenerator(line, true).Fees;

			foreach (IFee fee in fees)
			{
				if (fee.Amount > 0)
				{
					result.Add(fee.Code, fee.Amount);
				}
			}

			return result;
		}

		public override ZBool HasMPF
		{
			get
			{
				return line != null && line.US_HasMPF
					|| secondaryTariffLine1 != null && secondaryTariffLine1.US_HasMPF
					|| secondaryTariffLine2 != null && secondaryTariffLine2.US_HasMPF
					|| secondaryTariffLine3 != null && secondaryTariffLine3.US_HasMPF
					|| secondaryTariffLine4 != null && secondaryTariffLine4.US_HasMPF
					|| secondaryTariffLine5 != null && secondaryTariffLine5.US_HasMPF
					|| secondaryTariffLine6 != null && secondaryTariffLine6.US_HasMPF
					|| secondaryTariffLine7 != null && secondaryTariffLine7.US_HasMPF;
			}
		}

		public override ZString MPFPercentAsString
		{
			get
			{
				var dateForMPFCalc = line != null ? line.DateForMPFCalculation : ZDateTime.Today;
				var currentMPFRate = new FeeCalculationHelper(Factory, dateForMPFCalc).GetCurrentRate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
				return currentMPFRate != null ? currentMPFRate.ZZF_Value.ToStringTrimZeros() + "%" : "";
			}
		}

		#endregion

		#region Secondary Tariff Lines

		#region Secondary Line 1

		public override ZString SecondaryLine1Description
		{
			get { return SecondaryTariffLine ? TariffDescription(secondaryTariffLine1.CL_AdValoremTariff) : ZString.Empty; }
		}

		public override ZString SecondaryLine1FormattedTariff
		{
			get { return SecondaryTariffLine ? secondaryTariffLine1.FormattedTariff : ZString.Empty; }
		}

		public override ZString SecondaryLine1SecondQtyAndUQ
		{
			get { return SecondaryTariffLine ? GetQuantityAndUnit(iSecondaryCusline1.Quantity2, secondaryTariffLine1.SecondCustomsUnitQty) : ZString.Empty; }
		}

		public override ZString SecondaryLine1ThirdQtyAndUQ
		{
			get { return SecondaryTariffLine ? GetQuantityAndUnit(iSecondaryCusline1.Quantity3, secondaryTariffLine1.ThirdCustomsUnitQty) : ZString.Empty; }
		}

		public override ZString SecondaryLine1ADDNo
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine && !secondaryTariffLine1.RandomLine.US_ADDCaseNo.IsEmpty)
				{
					if (line.US_SupLine && ADDOnParent)
					{
						// AD relates to Parent tariff, don't print it here
					}
					else
					{
						ZString antidumpingCaseNumber = secondaryTariffLine1.RandomLine.US_ADDCaseNo;
						result = antidumpingCaseNumber.StartsWith("A") ? antidumpingCaseNumber.ToString() : "A" + antidumpingCaseNumber.ToString();
						result = (result.SubstringSafe(0, 4) + "-" + result.SubstringSafe(4, 3) + "-" + result.SubstringSafe(7, 3)).TrimEnd('-');
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine1ADDSurety
		{
			get { return SecondaryTariffLine ? SecondaryLine1ADDNo.IsEmpty ? ZString.Empty : ADDCVDSuretyValue : ZString.Empty; }
		}

		public override ZString SecondaryLine1ADDSpecificDepositValueFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (!SecondaryLine1ADDNo.IsEmpty)
				{
					var depositValue = ZDecimal.Zero;
					if (IsTIBEntry)
					{
						depositValue = secondaryTariffLine1.RandomLine.US_ADDDepositValue;
					}
					else if (line.US_SupLine)
					{
						depositValue = iCusline.ADDSpecificDepositValue;
					}
					else if (SecondaryTariffLine)
					{
						depositValue = iSecondaryCusline1.ADDSpecificDepositValue;
					}

					if (depositValue > 0 & depositValue != SecondaryLine1TotalLinePriceInLocalCurrencyRounded)
					{
						result = "(" + depositValue.ToString() + ")";
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine1ADDRate
		{
			get
			{
				var result = ZString.Empty;
				if (line.US_SupLine && !SecondaryLine1ADDNo.IsEmpty)
				{
					result = GetADDRateDescription(line);
				}
				else
				{
					result = GetADDRateDescription(secondaryTariffLine1);
				}

				return result;
			}
		}

		public override ZString SecondaryLine1ADDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				var cusEntryLine = line.US_SupLine ? line : secondaryTariffLine1;

				if (cusEntryLine != null && !SecondaryLine1ADDNo.IsEmpty)
				{
					ZDecimal amount = cusEntryLine.AntidumpingDuty;
					if (amount > 0)
					{
						var icusEntryLine = cusEntryLine as ICusEntryLine;
						if (icusEntryLine != null && icusEntryLine.BondedAntidumpingDuty)
						{
							result = "(" + amount.ToString(2) + ")";
						}
						else
						{
							result = amount.ToString(2);
						}
					}
					else if (IsTIBEntry)
					{
						var feeCalculator = new LineDutyFeeCalculator.ADD_CVDCalculator();
						var antiDumpingDuty = feeCalculator.CalculateADDDuty(cusEntryLine, new EntryLineIEntryLineOrInvoiceLineDutyData(cusEntryLine).GetADDAdjustValueForCombineLines(), out var percentOfValue);
						result = antiDumpingDuty.Round(2).ToString();
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine1CVDNo
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine && !secondaryTariffLine1.RandomLine.US_CVDCaseNo.IsEmpty)
				{
					if (line.US_SupLine && CVDOnParent)
					{
						// CVD relates to Parent tariff, don't print it here
					}
					else
					{
						ZString countervailingCaseNumber = secondaryTariffLine1.RandomLine.US_CVDCaseNo;
						result = countervailingCaseNumber.StartsWith("C") ? countervailingCaseNumber.ToString() : "C" + countervailingCaseNumber.ToString();
						result = (result.SubstringSafe(0, 4) + "-" + result.SubstringSafe(4, 3) + "-" + result.SubstringSafe(7, 3)).TrimEnd('-');
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine1CVDSurety
		{
			get { return SecondaryTariffLine ? SecondaryLine1CVDNo.IsEmpty ? ZString.Empty : ADDCVDSuretyValue : ZString.Empty; }
		}

		public override ZString SecondaryLine1CVDSpecificDepositValueFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine && !SecondaryLine1CVDNo.IsEmpty)
				{
					var depositValue = ZDecimal.Zero;
					if (IsTIBEntry)
					{
						depositValue = secondaryTariffLine1.RandomLine.US_ADDDepositValue;
					}
					else
					{
						depositValue = iSecondaryCusline1.CVDSpecificDepositValue;
					}

					if (depositValue > 0 & depositValue != (decimal)SecondaryLine1TotalLinePriceInLocalCurrencyRounded)
					{
						result = "(" + depositValue.ToString() + ")";
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine1CVDRate
		{
			get
			{
				var result = ZString.Empty;

				var cusEntryLine = line.US_SupLine ? line : secondaryTariffLine1;
				if (cusEntryLine != null)
				{
					result = GetCVDRateDescription(cusEntryLine);
				}

				return result;
			}
		}

		public override ZString SecondaryLine1CVDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				var cusEntryLine = line.US_SupLine ? line : secondaryTariffLine1;

				if (cusEntryLine != null && !SecondaryLine1CVDNo.IsEmpty)
				{
					var amount = cusEntryLine.CountervailingDuty;
					if (amount > 0)
					{
						var icusEntryLine = cusEntryLine as ICusEntryLine;
						if (icusEntryLine != null && icusEntryLine.BondedCountervailingDuty)
						{
							result = "(" + amount.ToString(2) + ")";
						}
						else
						{
							result = amount.ToString(2);
						}
					}
					else if (IsTIBEntry)
					{
						var feeCalculator = new LineDutyFeeCalculator.ADD_CVDCalculator();
						var countervailingDuty = feeCalculator.CalculateCVDDuty(cusEntryLine, new EntryLineIEntryLineOrInvoiceLineDutyData(cusEntryLine).GetCVDAdjustValueForCombineLines(), out var percentOfValue);
						result = countervailingDuty.Round(2).ToString();
					}
				}

				return result;
			}
		}

		public override ZDecimal SecondaryLine1CustomsQuantity
		{
			get { return SecondaryTariffLine ? iSecondaryCusline1.Quantity1 : 0; }
		}

		public override ZString SecondaryLine1CustomsUnitQty
		{
			get { return SecondaryTariffLine ? secondaryTariffLine1.CustomsUnitQty : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine1TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine ? new SecondaryTariffLineWrapper(secondaryTariffLine1).ValueInUSD.Round(0) : ZDecimal.Zero; }
		}

		public override ZString SecondaryLine1DutyPercentAsString
		{
			get { return SecondaryTariffLine ? GetDutyRateString(secondaryTariffLine1, base.SecondaryLine1DutyPercentAsString) : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine1DutyAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (SecondaryTariffLine)
				{
					if (USComponentsAssembledAbroad && line.IsParentLine && HasSecondaryWatchLine(line))
					{
						ZDecimal customsValue = line.CustomsValue.Amount + secondaryTariffLine1.CustomsValue.Amount;
						result = ChildLineCalculator(secondaryTariffLine1, customsValue).DutyResult.TotalAmount.Amount.Round(2);
					}
					else
					{
						result = GetDutyAmount(secondaryTariffLine1);
					}
				}

				return result;
			}
		}

		#endregion

		#region Secondary Line 2

		public override ZString SecondaryLine2Description
		{
			get { return SecondaryTariffLine2 ? TariffDescription(secondaryTariffLine2.CL_AdValoremTariff) : ZString.Empty; }
		}

		public override ZString SecondaryLine2FormattedTariff
		{
			get { return SecondaryTariffLine2 ? secondaryTariffLine2.FormattedTariff : ZString.Empty; }
		}

		public override ZString SecondaryLine2SecondQtyAndUQ
		{
			get { return SecondaryTariffLine2 ? GetQuantityAndUnit(iSecondaryCusline2.Quantity2, secondaryTariffLine2.SecondCustomsUnitQty) : ZString.Empty; }
		}

		public override ZString SecondaryLine2ThirdQtyAndUQ
		{
			get { return SecondaryTariffLine2 ? GetQuantityAndUnit(iSecondaryCusline2.Quantity3, secondaryTariffLine2.ThirdCustomsUnitQty) : ZString.Empty; }
		}

		public override ZString SecondaryLine2ADDNo
		{
			get { return SecondaryTariffLine2 ? secondaryTariffLine2.ADDNo : ZString.Empty; }
		}

		public override ZString SecondaryLine2ADDSurety
		{
			get { return SecondaryTariffLine2 ? secondaryTariffLine2.RandomLine.US_ADDCaseNo.IsEmpty ? ZString.Empty : ADDCVDSuretyValue : ZString.Empty; }
		}

		public override ZString SecondaryLine2ADDSpecificDepositValueFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine2 && !SecondaryLine2ADDNo.IsEmpty)
				{
					var depositValue = ZDecimal.Zero;
					if (IsTIBEntry)
					{
						depositValue = secondaryTariffLine2.RandomLine.US_ADDDepositValue;
					}
					else
					{
						depositValue = iSecondaryCusline2.ADDSpecificDepositValue;
					}

					if (depositValue > 0 & depositValue != SecondaryLine2TotalLinePriceInLocalCurrencyRounded)
					{
						result = "(" + depositValue.ToString() + ")";
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine2ADDRate
		{
			get { return GetADDRateDescription(secondaryTariffLine2); }
		}

		public override ZString SecondaryLine2ADDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine2)
				{
					ZDecimal amount = iSecondaryCusline2.AntidumpingDuty;
					if (amount > 0)
					{
						if (iSecondaryCusline2.BondedAntidumpingDuty)
						{
							result = "(" + amount.ToString(2) + ")";
						}
						else
						{
							result = amount.ToString(2);
						}
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine2CVDNo
		{
			get { return SecondaryTariffLine2 ? secondaryTariffLine2.CVDNo : ZString.Empty; }
		}

		public override ZString SecondaryLine2CVDSurety
		{
			get { return SecondaryTariffLine2 ? secondaryTariffLine2.RandomLine.US_CVDCaseNo.IsEmpty ? ZString.Empty : ADDCVDSuretyValue : ZString.Empty; }
		}

		public override ZString SecondaryLine2CVDSpecificDepositValueFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine2 && !SecondaryLine2CVDNo.IsEmpty)
				{
					var depositValue = ZDecimal.Zero;
					if (IsTIBEntry)
					{
						depositValue = secondaryTariffLine2.RandomLine.US_ADDDepositValue;
					}
					else
					{
						depositValue = iSecondaryCusline2.CVDSpecificDepositValue;
					}

					if (depositValue > 0 && depositValue != SecondaryLine2TotalLinePriceInLocalCurrencyRounded)
					{
						result = "(" + depositValue.ToString() + ")";
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine2CVDRate
		{
			get { return GetCVDRateDescription(secondaryTariffLine2); }
		}

		public override ZString SecondaryLine2CVDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine2)
				{
					ZDecimal amount = iSecondaryCusline2.CountervailingDuty;
					if (amount > 0)
					{
						if (iSecondaryCusline2.BondedCountervailingDuty)
						{
							result = "(" + amount.ToString(2) + ")";
						}
						else
						{
							result = amount.ToString(2);
						}
					}
				}

				return result;
			}
		}

		public override ZDecimal SecondaryLine2CustomsQuantity
		{
			get { return SecondaryTariffLine2 ? iSecondaryCusline2.Quantity1 : 0; }
		}

		public override ZString SecondaryLine2CustomsUnitQty
		{
			get { return SecondaryTariffLine2 ? secondaryTariffLine2.CustomsUnitQty : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine2TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine2 ? new SecondaryTariffLineWrapper(secondaryTariffLine2).ValueInUSD.Round(0) : ZDecimal.Zero; }
		}

		public override ZString SecondaryLine2DutyPercentAsString
		{
			get { return SecondaryTariffLine2 ? GetDutyRateString(secondaryTariffLine2, secondaryTariffLine2.CL_DutyPercentAsString) : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine2DutyAmount
		{
			get { return SecondaryTariffLine2 ? GetDutyAmount(secondaryTariffLine2) : ZDecimal.Zero; }
		}

		#endregion

		#region Secondary Line 3

		public override ZString SecondaryLine3Description
		{
			get { return SecondaryTariffLine3 ? TariffDescription(secondaryTariffLine3.CL_AdValoremTariff) : ZString.Empty; }
		}

		public override ZString SecondaryLine3FormattedTariff
		{
			get { return SecondaryTariffLine3 ? secondaryTariffLine3.FormattedTariff : ZString.Empty; }
		}

		public override ZString SecondaryLine3SecondQtyAndUQ
		{
			get { return SecondaryTariffLine3 ? GetQuantityAndUnit(iSecondaryCusline3.Quantity2, secondaryTariffLine3.SecondCustomsUnitQty) : ZString.Empty; }
		}

		public override ZString SecondaryLine3ThirdQtyAndUQ
		{
			get { return SecondaryTariffLine3 ? GetQuantityAndUnit(iSecondaryCusline3.Quantity3, secondaryTariffLine3.ThirdCustomsUnitQty) : ZString.Empty; }
		}

		public override ZString SecondaryLine3ADDNo
		{
			get { return SecondaryTariffLine3 ? secondaryTariffLine3.ADDNo : ZString.Empty; }
		}

		public override ZString SecondaryLine3ADDSurety
		{
			get { return SecondaryTariffLine3 ? secondaryTariffLine3.ADDNo.IsEmpty ? ZString.Empty : ADDCVDSuretyValue : ZString.Empty; }
		}

		public override ZString SecondaryLine3ADDSpecificDepositValueFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine3)
				{
					ZDecimal value = iSecondaryCusline3.ADDSpecificDepositValue;
					if (value > 0 && value != SecondaryLine3TotalLinePriceInLocalCurrencyRounded)
					{
						result = "(" + value.ToString() + ")";
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine3ADDRate
		{
			get { return GetADDRateDescription(secondaryTariffLine3); }
		}

		public override ZString SecondaryLine3ADDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine3)
				{
					ZDecimal amount = iSecondaryCusline3.AntidumpingDuty;
					if (amount > 0)
					{
						if (iSecondaryCusline3.BondedAntidumpingDuty)
						{
							result = "(" + amount.ToString(2) + ")";
						}
						else
						{
							result = amount.ToString(2);
						}
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine3CVDNo
		{
			get { return SecondaryTariffLine3 ? secondaryTariffLine3.CVDNo : ZString.Empty; }
		}

		public override ZString SecondaryLine3CVDSurety
		{
			get { return SecondaryTariffLine3 ? secondaryTariffLine3.RandomLine.US_CVDCaseNo.IsEmpty ? ZString.Empty : ADDCVDSuretyValue : ZString.Empty; }
		}

		public override ZString SecondaryLine3CVDSpecificDepositValueFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine3)
				{
					ZDecimal value = iSecondaryCusline3.CVDSpecificDepositValue;
					if (value > 0 & value != SecondaryLine3TotalLinePriceInLocalCurrencyRounded)
					{
						result = "(" + value.ToString() + ")";
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine3CVDRate
		{
			get { return GetCVDRateDescription(secondaryTariffLine3); }
		}

		public override ZString SecondaryLine3CVDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine3)
				{
					ZDecimal amount = iSecondaryCusline3.CountervailingDuty;
					if (amount > 0)
					{
						if (iSecondaryCusline3.BondedCountervailingDuty)
						{
							result = "(" + amount.ToString(2) + ")";
						}
						else
						{
							result = amount.ToString(2);
						}
					}
				}

				return result;
			}
		}

		public override ZDecimal SecondaryLine3CustomsQuantity
		{
			get { return SecondaryTariffLine3 ? iSecondaryCusline3.Quantity1 : 0; }
		}

		public override ZString SecondaryLine3CustomsUnitQty
		{
			get { return SecondaryTariffLine3 ? secondaryTariffLine3.CustomsUnitQty : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine3TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine3 ? new SecondaryTariffLineWrapper(secondaryTariffLine3).ValueInUSD.Round(0) : ZDecimal.Zero; }
		}

		public override ZString SecondaryLine3DutyPercentAsString
		{
			get { return SecondaryTariffLine3 ? GetDutyRateString(secondaryTariffLine3, base.SecondaryLine3DutyPercentAsString) : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine3DutyAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (SecondaryTariffLine3)
				{
					if (USComponentsAssembledAbroad && line.IsParentLine && HasSecondaryWatchLine(line))
					{
						ZDecimal customsValue = secondaryTariffLine2.CustomsValue.Amount + secondaryTariffLine3.CustomsValue.Amount;
						result = ChildLineCalculator(secondaryTariffLine3, customsValue).DutyResult.TotalAmount.Amount.Round(2);
					}
					else
					{
						result = GetDutyAmount(secondaryTariffLine3);
					}
				}

				return result;
			}
		}

		#endregion

		#region Secondary Line 4

		public override ZString SecondaryLine4Description
		{
			get { return SecondaryTariffLine4 ? TariffDescription(secondaryTariffLine4.CL_AdValoremTariff) : ZString.Empty; }
		}

		public override ZString SecondaryLine4FormattedTariff
		{
			get { return SecondaryTariffLine4 ? secondaryTariffLine4.FormattedTariff : ZString.Empty; }
		}

		public override ZString SecondaryLine4SecondQtyAndUQ
		{
			get { return SecondaryTariffLine4 ? GetQuantityAndUnit(iSecondaryCusline4.Quantity2, secondaryTariffLine4.SecondCustomsUnitQty) : ZString.Empty; }
		}

		public override ZString SecondaryLine4ThirdQtyAndUQ
		{
			get { return SecondaryTariffLine4 ? GetQuantityAndUnit(iSecondaryCusline4.Quantity3, secondaryTariffLine4.ThirdCustomsUnitQty) : ZString.Empty; }
		}

		public override ZString SecondaryLine4ADDNo
		{
			get { return SecondaryTariffLine4 ? secondaryTariffLine4.ADDNo : ZString.Empty; }
		}

		public override ZString SecondaryLine4ADDSurety
		{
			get { return SecondaryTariffLine4 ? secondaryTariffLine4.ADDNo.IsEmpty ? ZString.Empty : ADDCVDSuretyValue : ZString.Empty; }
		}

		public override ZString SecondaryLine4ADDSpecificDepositValueFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine4)
				{
					ZDecimal value = iSecondaryCusline4.ADDSpecificDepositValue;
					if (value > 0 & value != SecondaryLine4TotalLinePriceInLocalCurrencyRounded)
					{
						result = "(" + value.ToString() + ")";
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine4ADDRate
		{
			get { return GetADDRateDescription(secondaryTariffLine4); }
		}

		public override ZString SecondaryLine4ADDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine4)
				{
					ZDecimal amount = iSecondaryCusline4.AntidumpingDuty;
					if (amount > 0)
					{
						if (iSecondaryCusline4.BondedAntidumpingDuty)
						{
							result = "(" + amount.ToString(2) + ")";
						}
						else
						{
							result = amount.ToString(2);
						}
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine4CVDNo
		{
			get { return SecondaryTariffLine4 ? secondaryTariffLine4.CVDNo : ZString.Empty; }
		}

		public override ZString SecondaryLine4CVDSurety
		{
			get { return SecondaryTariffLine4 ? secondaryTariffLine4.RandomLine.US_CVDCaseNo.IsEmpty ? ZString.Empty : ADDCVDSuretyValue : ZString.Empty; }
		}

		public override ZString SecondaryLine4CVDSpecificDepositValueFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine4)
				{
					ZDecimal value = iSecondaryCusline4.CVDSpecificDepositValue;
					if (value > 0 & value != SecondaryLine4TotalLinePriceInLocalCurrencyRounded)
					{
						result = "(" + value.ToString() + ")";
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine4CVDRate
		{
			get { return GetCVDRateDescription(secondaryTariffLine4); }
		}

		public override ZString SecondaryLine4CVDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine4)
				{
					ZDecimal amount = iSecondaryCusline4.CountervailingDuty;
					if (amount > 0)
					{
						if (iSecondaryCusline4.BondedCountervailingDuty)
						{
							result = "(" + amount.ToString(2) + ")";
						}
						else
						{
							result = amount.ToString(2);
						}
					}
				}

				return result;
			}
		}

		public override ZDecimal SecondaryLine4CustomsQuantity
		{
			get { return SecondaryTariffLine4 ? iSecondaryCusline4.Quantity1 : 0; }
		}

		public override ZString SecondaryLine4CustomsUnitQty
		{
			get { return SecondaryTariffLine4 ? secondaryTariffLine4.CustomsUnitQty : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine4TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine4 ? new SecondaryTariffLineWrapper(secondaryTariffLine4).ValueInUSD.Round(0) : ZDecimal.Zero; }
		}

		public override ZString SecondaryLine4DutyPercentAsString
		{
			get { return SecondaryTariffLine4 ? GetDutyRateString(secondaryTariffLine4, secondaryTariffLine4.CL_DutyPercentAsString) : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine4DutyAmount
		{
			get { return SecondaryTariffLine4 ? GetDutyAmount(secondaryTariffLine4) : ZDecimal.Zero; }
		}

		#endregion

		#region Secondary Line 5

		public override ZString SecondaryLine5Description
		{
			get { return SecondaryTariffLine5 ? TariffDescription(secondaryTariffLine5.CL_AdValoremTariff) : ZString.Empty; }
		}

		public override ZString SecondaryLine5FormattedTariff
		{
			get { return SecondaryTariffLine5 ? secondaryTariffLine5.FormattedTariff : ZString.Empty; }
		}

		public override ZString SecondaryLine5SecondQtyAndUQ
		{
			get { return SecondaryTariffLine5 ? GetQuantityAndUnit(iSecondaryCusline5.Quantity2, secondaryTariffLine5.SecondCustomsUnitQty) : ZString.Empty; }
		}

		public override ZString SecondaryLine5ThirdQtyAndUQ
		{
			get { return SecondaryTariffLine5 ? GetQuantityAndUnit(iSecondaryCusline5.Quantity3, secondaryTariffLine5.ThirdCustomsUnitQty) : ZString.Empty; }
		}

		public override ZString SecondaryLine5ADDNo
		{
			get { return SecondaryTariffLine5 ? secondaryTariffLine5.ADDNo : ZString.Empty; }
		}

		public override ZString SecondaryLine5ADDSurety
		{
			get { return SecondaryTariffLine5 ? secondaryTariffLine5.ADDNo.IsEmpty ? ZString.Empty : ADDCVDSuretyValue : ZString.Empty; }
		}

		public override ZString SecondaryLine5ADDSpecificDepositValueFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine5)
				{
					ZDecimal value = iSecondaryCusline5.ADDSpecificDepositValue;
					if (value > 0 & value != SecondaryLine5TotalLinePriceInLocalCurrencyRounded)
					{
						result = "(" + value.ToString() + ")";
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine5ADDRate
		{
			get { return GetADDRateDescription(secondaryTariffLine5); }
		}

		public override ZString SecondaryLine5ADDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine5)
				{
					ZDecimal amount = iSecondaryCusline5.AntidumpingDuty;
					if (amount > 0)
					{
						if (iSecondaryCusline5.BondedAntidumpingDuty)
						{
							result = "(" + amount.ToString(2) + ")";
						}
						else
						{
							result = amount.ToString(2);
						}
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine5CVDNo
		{
			get { return SecondaryTariffLine5 ? secondaryTariffLine5.CVDNo : ZString.Empty; }
		}

		public override ZString SecondaryLine5CVDSurety
		{
			get { return SecondaryTariffLine5 ? secondaryTariffLine5.RandomLine.US_CVDCaseNo.IsEmpty ? ZString.Empty : ADDCVDSuretyValue : ZString.Empty; }
		}

		public override ZString SecondaryLine5CVDSpecificDepositValueFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine5)
				{
					ZDecimal value = iSecondaryCusline5.CVDSpecificDepositValue;
					if (value > 0 & value != SecondaryLine5TotalLinePriceInLocalCurrencyRounded)
					{
						result = "(" + value.ToString() + ")";
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine5CVDRate
		{
			get { return GetCVDRateDescription(secondaryTariffLine5); }
		}

		public override ZString SecondaryLine5CVDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine5)
				{
					ZDecimal amount = iSecondaryCusline5.CountervailingDuty;
					if (amount > 0)
					{
						if (iSecondaryCusline5.BondedCountervailingDuty)
						{
							result = "(" + amount.ToString(2) + ")";
						}
						else
						{
							result = amount.ToString(2);
						}
					}
				}

				return result;
			}
		}

		public override ZDecimal SecondaryLine5CustomsQuantity
		{
			get { return SecondaryTariffLine5 ? iSecondaryCusline5.Quantity1 : 0; }
		}

		public override ZString SecondaryLine5CustomsUnitQty
		{
			get { return SecondaryTariffLine5 ? secondaryTariffLine5.CustomsUnitQty : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine5TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine5 ? new SecondaryTariffLineWrapper(secondaryTariffLine5).ValueInUSD.Round(0) : ZDecimal.Zero; }
		}

		public override ZString SecondaryLine5DutyPercentAsString
		{
			get { return SecondaryTariffLine5 ? GetDutyRateString(secondaryTariffLine5, base.SecondaryLine5DutyPercentAsString) : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine5DutyAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (SecondaryTariffLine5)
				{
					if (USComponentsAssembledAbroad && line.IsParentLine && HasSecondaryWatchLine(line))
					{
						ZDecimal customsValue = secondaryTariffLine4.CustomsValue.Amount + secondaryTariffLine5.CustomsValue.Amount;
						result = ChildLineCalculator(secondaryTariffLine5, customsValue).DutyResult.TotalAmount.Amount.Round(2);
					}
					else
					{
						result = GetDutyAmount(secondaryTariffLine5);
					}
				}

				return result;
			}
		}

		#endregion

		#region Secondary Line 6

		public override ZString SecondaryLine6Description
		{
			get { return SecondaryTariffLine6 ? TariffDescription(secondaryTariffLine6.CL_AdValoremTariff) : ZString.Empty; }
		}

		public override ZString SecondaryLine6FormattedTariff
		{
			get { return SecondaryTariffLine6 ? secondaryTariffLine6.FormattedTariff : ZString.Empty; }
		}

		public override ZString SecondaryLine6SecondQtyAndUQ
		{
			get { return SecondaryTariffLine6 ? GetQuantityAndUnit(iSecondaryCusline6.Quantity2, secondaryTariffLine6.SecondCustomsUnitQty) : ZString.Empty; }
		}

		public override ZString SecondaryLine6ThirdQtyAndUQ
		{
			get { return SecondaryTariffLine6 ? GetQuantityAndUnit(iSecondaryCusline6.Quantity3, secondaryTariffLine6.ThirdCustomsUnitQty) : ZString.Empty; }
		}

		public override ZString SecondaryLine6ADDNo
		{
			get { return SecondaryTariffLine6 ? secondaryTariffLine6.ADDNo : ZString.Empty; }
		}

		public override ZString SecondaryLine6ADDSurety
		{
			get { return SecondaryTariffLine6 ? secondaryTariffLine6.ADDNo.IsEmpty ? ZString.Empty : ADDCVDSuretyValue : ZString.Empty; }
		}

		public override ZString SecondaryLine6ADDSpecificDepositValueFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine6)
				{
					ZDecimal value = iSecondaryCusline6.ADDSpecificDepositValue;
					if (value > 0 & value != SecondaryLine6TotalLinePriceInLocalCurrencyRounded)
					{
						result = "(" + value.ToString() + ")";
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine6ADDRate
		{
			get { return GetADDRateDescription(secondaryTariffLine6); }
		}

		public override ZString SecondaryLine6ADDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine6)
				{
					ZDecimal amount = iSecondaryCusline6.AntidumpingDuty;
					if (amount > 0)
					{
						if (iSecondaryCusline6.BondedAntidumpingDuty)
						{
							result = "(" + amount.ToString(2) + ")";
						}
						else
						{
							result = amount.ToString(2);
						}
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine6CVDNo
		{
			get { return SecondaryTariffLine6 ? secondaryTariffLine6.CVDNo : ZString.Empty; }
		}

		public override ZString SecondaryLine6CVDSurety
		{
			get { return SecondaryTariffLine6 ? secondaryTariffLine6.RandomLine.US_CVDCaseNo.IsEmpty ? ZString.Empty : ADDCVDSuretyValue : ZString.Empty; }
		}

		public override ZString SecondaryLine6CVDSpecificDepositValueFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine6)
				{
					ZDecimal value = iSecondaryCusline6.CVDSpecificDepositValue;
					if (value > 0 & value != SecondaryLine6TotalLinePriceInLocalCurrencyRounded)
					{
						result = "(" + value.ToString() + ")";
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine6CVDRate
		{
			get { return GetCVDRateDescription(secondaryTariffLine6); }
		}

		public override ZString SecondaryLine6CVDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine6)
				{
					ZDecimal amount = iSecondaryCusline6.CountervailingDuty;
					if (amount > 0)
					{
						if (iSecondaryCusline6.BondedCountervailingDuty)
						{
							result = "(" + amount.ToString(2) + ")";
						}
						else
						{
							result = amount.ToString(2);
						}
					}
				}

				return result;
			}
		}

		public override ZDecimal SecondaryLine6CustomsQuantity
		{
			get { return SecondaryTariffLine6 ? iSecondaryCusline6.Quantity1 : 0; }
		}

		public override ZString SecondaryLine6CustomsUnitQty
		{
			get { return SecondaryTariffLine6 ? secondaryTariffLine6.CustomsUnitQty : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine6TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine6 ? new SecondaryTariffLineWrapper(secondaryTariffLine6).ValueInUSD.Round(0) : ZDecimal.Zero; }
		}

		public override ZString SecondaryLine6DutyPercentAsString
		{
			get { return SecondaryTariffLine6 ? GetDutyRateString(secondaryTariffLine6, secondaryTariffLine6.CL_DutyPercentAsString) : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine6DutyAmount
		{
			get { return SecondaryTariffLine6 ? GetDutyAmount(secondaryTariffLine6) : ZDecimal.Zero; }
		}

		#endregion

		#region Secondary Line 7

		public override ZString SecondaryLine7Description
		{
			get { return SecondaryTariffLine7 ? TariffDescription(secondaryTariffLine7.CL_AdValoremTariff) : ZString.Empty; }
		}

		public override ZString SecondaryLine7FormattedTariff
		{
			get { return SecondaryTariffLine7 ? secondaryTariffLine7.FormattedTariff : ZString.Empty; }
		}

		public override ZString SecondaryLine7SecondQtyAndUQ
		{
			get { return SecondaryTariffLine7 ? GetQuantityAndUnit(iSecondaryCusline7.Quantity2, secondaryTariffLine7.SecondCustomsUnitQty) : ZString.Empty; }
		}

		public override ZString SecondaryLine7ThirdQtyAndUQ
		{
			get { return SecondaryTariffLine7 ? GetQuantityAndUnit(iSecondaryCusline7.Quantity3, secondaryTariffLine7.ThirdCustomsUnitQty) : ZString.Empty; }
		}

		public override ZString SecondaryLine7ADDNo
		{
			get { return SecondaryTariffLine7 ? secondaryTariffLine7.ADDNo : ZString.Empty; }
		}

		public override ZString SecondaryLine7ADDSurety
		{
			get { return SecondaryTariffLine7 ? secondaryTariffLine7.ADDNo.IsEmpty ? ZString.Empty : ADDCVDSuretyValue : ZString.Empty; }
		}

		public override ZString SecondaryLine7ADDSpecificDepositValueFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine7)
				{
					ZDecimal value = iSecondaryCusline7.ADDSpecificDepositValue;
					if (value > 0 & value != SecondaryLine7TotalLinePriceInLocalCurrencyRounded)
					{
						result = "(" + value.ToString() + ")";
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine7ADDRate
		{
			get { return GetADDRateDescription(secondaryTariffLine7); }
		}

		public override ZString SecondaryLine7ADDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine7)
				{
					ZDecimal add = iSecondaryCusline7.AntidumpingDuty;
					if (add > 0)
					{
						if (iSecondaryCusline7.BondedAntidumpingDuty)
						{
							result = "(" + add.ToString(2) + ")";
						}
						else
						{
							result = add.ToString(2);
						}
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine7CVDNo
		{
			get { return SecondaryTariffLine7 ? secondaryTariffLine7.CVDNo : ZString.Empty; }
		}

		public override ZString SecondaryLine7CVDSurety
		{
			get { return SecondaryTariffLine7 ? secondaryTariffLine7.RandomLine.US_CVDCaseNo.IsEmpty ? ZString.Empty : ADDCVDSuretyValue : ZString.Empty; }
		}

		public override ZString SecondaryLine7CVDSpecificDepositValueFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine7)
				{
					if (iSecondaryCusline7.CVDSpecificDepositValue > 0 & iSecondaryCusline7.CVDSpecificDepositValue != SecondaryLine7TotalLinePriceInLocalCurrencyRounded)
					{
						result = "(" + iSecondaryCusline7.CVDSpecificDepositValue.ToString() + ")";
					}
				}

				return result;
			}
		}

		public override ZString SecondaryLine7CVDRate
		{
			get { return GetCVDRateDescription(secondaryTariffLine7); }
		}

		public override ZString SecondaryLine7CVDFormatted
		{
			get
			{
				ZString result = ZString.Empty;

				if (SecondaryTariffLine7)
				{
					ZDecimal countervailingDuty = iSecondaryCusline7.CountervailingDuty;
					if (countervailingDuty > 0)
					{
						if (iSecondaryCusline7.BondedCountervailingDuty)
						{
							result = "(" + countervailingDuty.ToString(2) + ")";
						}
						else
						{
							result = countervailingDuty.ToString(2);
						}
					}
				}

				return result;
			}
		}

		public override ZDecimal SecondaryLine7CustomsQuantity
		{
			get { return SecondaryTariffLine7 ? iSecondaryCusline7.Quantity1 : 0; }
		}

		public override ZString SecondaryLine7CustomsUnitQty
		{
			get { return SecondaryTariffLine7 ? secondaryTariffLine7.CustomsUnitQty : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine7TotalLinePriceInLocalCurrencyRounded
		{
			get { return SecondaryTariffLine7 ? new SecondaryTariffLineWrapper(secondaryTariffLine7).ValueInUSD.Round(0) : ZDecimal.Zero; }
		}

		public override ZString SecondaryLine7DutyPercentAsString
		{
			get { return SecondaryTariffLine7 ? GetDutyRateString(secondaryTariffLine7, base.SecondaryLine7DutyPercentAsString) : ZString.Empty; }
		}

		public override ZDecimal SecondaryLine7DutyAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (SecondaryTariffLine7)
				{
					if (USComponentsAssembledAbroad && line.IsParentLine && HasSecondaryWatchLine(line))
					{
						ZDecimal customsValue = secondaryTariffLine6.CustomsValue.Amount + secondaryTariffLine7.CustomsValue.Amount;
						result = ChildLineCalculator(secondaryTariffLine7, customsValue).DutyResult.TotalAmount.Amount.Round(2);
					}
					else
					{
						result = GetDutyAmount(secondaryTariffLine7);
					}
				}

				return result;
			}
		}

		#endregion

		ZString GetDutyRateString(CusEntryLine entryLine, ZString normalDutyString)
		{
			var result = normalDutyString;
			var dutyData = new EntryLineIEntryLineOrInvoiceLineDutyData(entryLine);

			if (IsTIBEntry)
			{
				if (entryLine.IsSetVLine)
				{
					result = ZString.Empty;
				}
				else
				{
					if (entryLine.IsCombinedLine())
					{
						var dutyResult = Chapter98Helper.CalculateTIBDutyForPrint(entryLine);
						if (dutyResult != null)
						{
							result = dutyResult.RateString;
						}
					}
					else
					{
						var calculator = AppendixFDutyCalculator.NewWithCombinedCustomsValue(entryLine, entryLine.ParentLine);
						result = calculator.DutyResult.RateString;
					}
				}
			}
			else if (result.Equals(normalDutyString) && CalculateDutyForDocument.ShouldCalculateDutyOnlyForDocumnet(dutyData))
			{
				result = CalculateDutyForDocument.PrinterDuty(dutyData).RateString;
			}

			return result;
		}

		ZDecimal GetDutyAmount(CusEntryLine entryLine)
		{
			ZDecimal result = ZDecimal.Zero;
			var dutyData = new EntryLineIEntryLineOrInvoiceLineDutyData(entryLine);

			if (IsTIBEntry)
			{
				if (!entryLine.IsSetVLine)
				{
					if (entryLine.IsCombinedLine())
					{
						var dutyResult = Chapter98Helper.CalculateTIBDutyForPrint(entryLine);
						if (dutyResult != null)
						{
							result = dutyResult.TotalAmount.Amount;
						}
					}
					else
					{
						var calculator = AppendixFDutyCalculator.NewWithCombinedCustomsValue(entryLine, entryLine.ParentLine);
						result = calculator.DutyResult.TotalAmount.Amount;
					}
				}
			}
			else
			{
				result = entryLine.DutyAmount;

				if (result.IsEmpty && CalculateDutyForDocument.ShouldCalculateDutyOnlyForDocumnet(dutyData))
				{
					result = CalculateDutyForDocument.PrinterDuty(dutyData).TotalAmount.Amount;
				}
			}

			return result;
		}

		#endregion

		protected abstract string GetADCVDRateDescriptionFromCaseRecord(ZString caseNo, string rateType, ZDecimal depositRateOverride);

		ZString GetADDRateDescription(CusEntryLine entryLine)
		{
			var result = ZString.Empty;
			if (entryLine != null)
			{
				var adCaseNumber = IsTIBEntry ? entryLine.RandomLine.US_ADDCaseNo : entryLine.AntidumpingCaseNumber;
				var adRateQualifier = IsTIBEntry ? entryLine.RandomLine.US_ADDDepositRateIndicator : entryLine.ADDCaseRateTypeQualifier;
				var adRateOverride = IsTIBEntry ? entryLine.RandomLine.US_ADDDepositRateOverride : entryLine.ADDDepositRate;
				result = GetADCVDRateDescriptionFromCaseRecord(adCaseNumber, adRateQualifier, adRateOverride);
			}

			return result;
		}

		ZString GetCVDRateDescription(CusEntryLine entryLine)
		{
			var result = ZString.Empty;
			if (entryLine != null)
			{
				var cvCaseNumber = IsTIBEntry ? entryLine.RandomLine.US_CVDCaseNo : entryLine.CountervailingCaseNumber;
				var cvRateQualifier = IsTIBEntry ? entryLine.RandomLine.US_CVDDepositRateIndicator : entryLine.CVDCaseRateTypeQualifier;
				var cvRateOverride = IsTIBEntry ? entryLine.RandomLine.US_CVDDepositRateOverride : entryLine.CVDDepositRate;
				result = GetADCVDRateDescriptionFromCaseRecord(cvCaseNumber, cvRateQualifier, cvRateOverride);
			}

			return result;
		}

		ZString TariffDescription(ZString tariff)
		{
			ZString result = ZString.Empty;

			USCTariff uscTariff = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, tariff));

			if (uscTariff != null)
			{
				result = uscTariff.UE_ShortDescription;
			}

			return result;
		}

		AppendixFDutyCalculator ChildLineCalculator(CusEntryLine childLine, ZDecimal customsValue)
		{
			var dutyData = new DutyDataProxy(childLine);
			dutyData.CustomsValue = customsValue;
			return new AppendixFDutyCalculator(dutyData, childLine.Factory);
		}

		#endregion
	}
}
