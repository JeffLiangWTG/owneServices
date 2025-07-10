using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Schema;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	public abstract class EntrySummary7501Line : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected EntrySummary7501Line(BusinessObjectFactory factory, ZString entryType)
			: base(factory)
		{
			this.entryType = entryType;
		}

		public CusEntryLine line
		{
			get;
			set;
		}
		public CusEntryLine secondaryTariffLine1;
		public CusEntryLine secondaryTariffLine2;
		public CusEntryLine secondaryTariffLine3;
		public CusEntryLine secondaryTariffLine4;
		public CusEntryLine secondaryTariffLine5;
		public CusEntryLine secondaryTariffLine6;
		public CusEntryLine secondaryTariffLine7;
		public readonly ZString entryType;

		public ZBool PrintInvoiceHeading { get; internal set; }
		public ZBool PrintInvoiceDetails { get; internal set; }

		protected bool IsTIBEntry
		{
			get { return entryType == EntryTypeList.Codes.TemporaryImportationBond; }
		}

		protected IACELicenceAndPermit GetLicenseBlock(List<IACELicenceAndPermit> ens52VisaList)
		{
			return ens52VisaList.FirstOrDefault(x => x.LicenseCertificatePermitTypeCode != LicencePermitTypeList.Codes._12 &&
					x.LicenseCertificatePermitTypeCode != LicencePermitTypeList.Codes._22 && x.LicenseCertificatePermitTypeCode != LicencePermitTypeList.Codes._23 &&
					x.LicenseCertificatePermitTypeCode != LicencePermitTypeList.Codes._16 && x.LicenseCertificatePermitTypeCode != LicencePermitTypeList.Codes._18); //any first except cotton/organic/CA Sugar/CBTPA certificates because they are printed in VisaCertificateNumber field.
		}

		#region IEntrySummaryLines Members

		public EntrySummary7501Invoice InvoiceDetails
		{
			get { return fInvoiceDetails ?? (fInvoiceDetails = GetInvoiceDetails()); }
		}
		EntrySummary7501Invoice fInvoiceDetails;

		protected abstract EntrySummary7501Invoice GetInvoiceDetails();

		public abstract ZString TransRelatedInd { get; }
		public abstract ZString LineNumber { get; }
		public abstract ZBool SecondaryTariffLine { get; }
		public abstract ZBool SecondaryTariffLine2 { get; }
		public abstract ZBool SecondaryTariffLine3 { get; }
		public abstract ZBool SecondaryTariffLine4 { get; }
		public abstract ZBool SecondaryTariffLine5 { get; }
		public abstract ZBool SecondaryTariffLine6 { get; }
		public abstract ZBool SecondaryTariffLine7 { get; }
		public abstract ZString Description { get; }
		public abstract ZString SPIAndOrSecondarySPI { get; }
		public abstract ZString CountryOfOriginForLine { get; }
		public abstract ZString CountryOfExportForLine { get; }
		public abstract ZString ExportDateForTextile { get; }
		protected abstract ZDate ExportDateCore { get; }
		public abstract ZString PortOfLadingForLine { get; }
		public abstract ZString FormattedTariff { get; }
		public abstract ZString ExclusionNumber { get; }
		public abstract ZDecimal GrossWeightInKilograms { get; }
		public abstract ZDecimal CustomsQuantity { get; }
		public abstract ZString CustomsUnitQty { get; }
		public ZString QuantityAndUnitQty
		{
			get { return CustomsQuantity.ToString() + " " + CustomsUnitQty; }
		}

		public ZString ExportDate
		{
			get { return ExportDateCore.IsValid ? ExportDateCore.ToString("MMddyy") : string.Empty; }
		}

		public abstract ZDecimal TotalLinePriceInLocalCurrencyRounded { get; }
		public abstract ZString ChargesRounded { get; }
		public abstract ZString DutyPercentAsString { get; }
		public abstract ZString SecondQtyAndUQ { get; }
		public abstract ZString ThirdQtyAndUQ { get; }
		public abstract ZString ADDNo { get; }
		public abstract ZString ADDSurety { get; }
		public abstract ZString ADDSpecificDepositValueFormatted { get; }
		public abstract ZString ADDRate { get; }
		public abstract ZString ADDFormatted { get; }
		public abstract ZString CVDNo { get; }
		public abstract ZString CVDSurety { get; }
		public abstract ZString CVDSpecificDepositValueFormatted { get; }
		public abstract ZString CVDRate { get; }
		public abstract ZString CVDFormatted { get; }

		public ZString Block29Element1
		{
			get { return Block29Elements.Count > 0 ? Block29Elements[0] : ZString.Empty; }
		}

		public ZString Block29Element2
		{
			get { return Block29Elements.Count > 1 ? Block29Elements[1] : ZString.Empty; }
		}

		public ZString Block29Element3
		{
			get { return Block29Elements.Count > 2 ? Block29Elements[2] : ZString.Empty; }
		}

		public ZString Block29Element4
		{
			get { return Block29Elements.Count > 3 ? Block29Elements[3] : ZString.Empty; }
		}

		public ZString Block29Element5
		{
			get { return Block29Elements.Count > 4 ? Block29Elements[4] : ZString.Empty; }
		}

		public ZString Block29Element6
		{
			get { return Block29Elements.Count > 5 ? Block29Elements[5] : ZString.Empty; }
		}

		public ZString Block29Element7
		{
			get { return Block29Elements.Count > 6 ? Block29Elements[6] : ZString.Empty; }
		}

		public abstract List<ZString> Block29Elements { get; }
		protected List<ZString> block29Elements;

		public abstract ZString TextileCategoryNumberWithLabel { get; }
		public abstract ZString LineLevelManufacturerIDWithLabel { get; }
		public abstract ZString BindingRulingWithLabel { get; }

		public ZBool HideSecondQtyLine
		{
			get { return ADDNo.IsEmpty && SecondQtyAndUQ.IsEmpty; }
		}

		public ZBool HideThirdQtyLine
		{
			get { return CVDNo.IsEmpty && ThirdQtyAndUQ.IsEmpty; }
		}

		protected ZString GetQuantityAndUnit(ZDecimal quantity, ZString unit)
		{
			ZString result = ZString.Empty;

			if (!quantity.IsEmpty)
			{
				result = quantity.ToStringTrimZeros() + " " + unit;
			}

			return result;
		}

		bool ShouldHideForCombine(CusEntryLine line)
		{
			return line != null && line.IsCombinedLine() && !line.IsNormalTariffLine();
		}

		public ZBool HideSecondaryLine1SecondQtyLine
		{
			get { return !SecondaryTariffLine || (SecondaryLine1ADDNo.IsEmpty && SecondaryLine1SecondQtyAndUQ.IsEmpty) || ShouldHideForCombine(secondaryTariffLine1); }
		}

		public ZBool HideSecondaryLine1ThirdQtyLine
		{
			get { return !SecondaryTariffLine || (SecondaryLine1CVDNo.IsEmpty && SecondaryLine1ThirdQtyAndUQ.IsEmpty) || ShouldHideForCombine(secondaryTariffLine1); }
		}

		public ZBool HideSecondaryLine2SecondQtyLine
		{
			get { return !SecondaryTariffLine2 || (SecondaryLine2ADDNo.IsEmpty && SecondaryLine2SecondQtyAndUQ.IsEmpty) || ShouldHideForCombine(secondaryTariffLine2); }
		}

		public ZBool HideSecondaryLine2ThirdQtyLine
		{
			get { return !SecondaryTariffLine2 || (SecondaryLine2CVDNo.IsEmpty && SecondaryLine2ThirdQtyAndUQ.IsEmpty) || ShouldHideForCombine(secondaryTariffLine2); }
		}

		public ZBool HideSecondaryLine3SecondQtyLine
		{
			get { return !SecondaryTariffLine3 || (SecondaryLine3ADDNo.IsEmpty && SecondaryLine3SecondQtyAndUQ.IsEmpty) || ShouldHideForCombine(secondaryTariffLine3); }
		}

		public ZBool HideSecondaryLine3ThirdQtyLine
		{
			get { return !SecondaryTariffLine3 || (SecondaryLine3CVDNo.IsEmpty && SecondaryLine3ThirdQtyAndUQ.IsEmpty) || ShouldHideForCombine(secondaryTariffLine3); }
		}

		public ZBool HideSecondaryLine4SecondQtyLine
		{
			get { return !SecondaryTariffLine4 || (SecondaryLine4ADDNo.IsEmpty && SecondaryLine4SecondQtyAndUQ.IsEmpty) || ShouldHideForCombine(secondaryTariffLine4); }
		}

		public ZBool HideSecondaryLine4ThirdQtyLine
		{
			get { return !SecondaryTariffLine4 || (SecondaryLine4CVDNo.IsEmpty && SecondaryLine4ThirdQtyAndUQ.IsEmpty) || ShouldHideForCombine(secondaryTariffLine4); }
		}

		public ZBool HideSecondaryLine5SecondQtyLine
		{
			get { return !SecondaryTariffLine5 || (SecondaryLine5ADDNo.IsEmpty && SecondaryLine5SecondQtyAndUQ.IsEmpty) || ShouldHideForCombine(secondaryTariffLine5); }
		}

		public ZBool HideSecondaryLine5ThirdQtyLine
		{
			get { return !SecondaryTariffLine5 || (SecondaryLine5CVDNo.IsEmpty && SecondaryLine5ThirdQtyAndUQ.IsEmpty) || ShouldHideForCombine(secondaryTariffLine5); }
		}

		public ZBool HideSecondaryLine6SecondQtyLine
		{
			get { return !SecondaryTariffLine6 || (SecondaryLine6ADDNo.IsEmpty && SecondaryLine6SecondQtyAndUQ.IsEmpty) || ShouldHideForCombine(secondaryTariffLine6); }
		}

		public ZBool HideSecondaryLine6ThirdQtyLine
		{
			get { return !SecondaryTariffLine6 || (SecondaryLine6CVDNo.IsEmpty && SecondaryLine6ThirdQtyAndUQ.IsEmpty) || ShouldHideForCombine(secondaryTariffLine6); }
		}

		public ZBool HideSecondaryLine7SecondQtyLine
		{
			get { return !SecondaryTariffLine7 || (SecondaryLine7ADDNo.IsEmpty && SecondaryLine7SecondQtyAndUQ.IsEmpty) || ShouldHideForCombine(secondaryTariffLine7); }
		}

		public ZBool HideSecondaryLine7ThirdQtyLine
		{
			get { return !SecondaryTariffLine7 || (SecondaryLine7CVDNo.IsEmpty && SecondaryLine7ThirdQtyAndUQ.IsEmpty) || ShouldHideForCombine(secondaryTariffLine7); }
		}

		public ZBool HideCountryOfExportLine
		{
			get { return CountryOfExportForLine.IsEmpty && Block29Element2.IsEmpty && TransRelatedInd.IsEmpty; }
		}

		public ZBool HidePortOfLadingLine
		{
			get { return PortOfLadingForLine.IsEmpty && Block29Element3.IsEmpty && LicenseNumber.IsEmpty; }
		}

		public abstract ZDecimal LumberExportPrice { get; }
		public abstract ZString LumberImporterDeclaration { get; }
		public abstract ZDecimal LumberExportCharges { get; }
		public abstract ZDecimal DutyAmount { get; }
		public abstract ZString VisaCertificateNumber { get; }
		public abstract ZString LicenseNumber { get; }

		ZString GetLicenseText(ZString licenseText)
		{
			return licenseText.IsEmpty ? "" : "PMT/LIC#";
		}

		public ZString LicenseText => GetLicenseText(LicenseNumber);

		public ZString LicenseText1 => GetLicenseText(SecondaryLine1LicenseNumber);

		public ZString LicenseText2 => GetLicenseText(SecondaryLine2LicenseNumber);

		public ZString LicenseText3 => GetLicenseText(SecondaryLine3LicenseNumber);

		public ZString LicenseText4 => GetLicenseText(SecondaryLine4LicenseNumber);

		public ZString LicenseText5 => GetLicenseText(SecondaryLine5LicenseNumber);

		public ZString LicenseText6 => GetLicenseText(SecondaryLine6LicenseNumber);

		public ZString LicenseText7 => GetLicenseText(SecondaryLine7LicenseNumber);

		public ZString MPFCodeAndDescription
		{
			get
			{
				if (!mpfCodeAndDescription.HasValue)
				{
					var code = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode, ZDateTime.Now);
					mpfCodeAndDescription = code == null ? "" : (code.ZZD_Code + " " + code.ZZD_Description.ToUpper() + " (MPF)");
				}
				return mpfCodeAndDescription.Value;
			}
		}
		ZString? mpfCodeAndDescription;

		public ZString HMFCodeAndDescription
		{
			get
			{
				if (!hmfCodeAndDescription.HasValue)
				{
					var code = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, Core.Constants.USCustoms.FeeCodes.HMF, Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode, ZDateTime.Now);
					hmfCodeAndDescription = code == null ? "" : (code.ZZD_Code + " " + code.ZZD_Description.ToUpper() + " (HMF)");
				}
				return hmfCodeAndDescription.Value;
			}
		}
		ZString? hmfCodeAndDescription;

		public ZDecimal MPFAmount
		{
			get
			{
				if (!mpfAmount.HasValue)
				{
					ZDecimal result;
					FeesAndTaxes.TryGetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, out result);
					mpfAmount = result;
				}
				return mpfAmount.Value;
			}
		}
		ZDecimal? mpfAmount;

		public abstract ZBool HasMPF { get; }

		public abstract ZString MPFPercentAsString { get; }

		public ZDecimal HMFAmount
		{
			get
			{
				if (!hmfAmount.HasValue)
				{
					ZDecimal result;
					FeesAndTaxes.TryGetValue(Core.Constants.USCustoms.FeeCodes.HMF, out result);
					hmfAmount = result;
				}
				return hmfAmount.Value;
			}
		}
		ZDecimal? hmfAmount;

		public ZString HMFPercentAsString
		{
			get { return new FeeCalculationHelper(Factory, DateForDutyCalculation).HMFRatePercentage.ToString(CultureInfo.CurrentCulture).TrimEnd('0') + "%"; }
		}

		public virtual ZDate DateForDutyCalculation
		{
			get { return line != null ? line.DateForDutyCalculation : ZDate.Empty; }
		}

		public ZDecimal LineFeeAmount
		{
			get
			{
				if (!lineFeeAmount.HasValue)
				{
					ZDecimal result = ZDecimal.Zero;

					if (!LineFeeCode.IsEmpty)
					{
						FeesAndTaxes.TryGetValue(LineFeeCode, out result);
					}

					lineFeeAmount = result;
				}
				return lineFeeAmount.Value;
			}
		}
		ZDecimal? lineFeeAmount;

		public ZString LineFeeDescription
		{
			get
			{
				var description = CusFeeCodeConstants.GetAccountingClassFeeCodeList(Factory).GetDescriptionFromCode(LineFeeCode);

				return LineFeeCode
					+ (string.IsNullOrEmpty(description) ? "" : " " + description.ToUpper())
					+ (LineFeeRateType.IsEmpty ? "" : " (" + LineFeeRateType + ")");
			}
		}

		protected ZString LineFeeCode
		{
			get
			{
				if (!lineFeeCode.HasValue)
				{
					lineFeeCode = ZString.Empty;

					List<ZString> feeCodes = new List<ZString>(FeesAndTaxes.Keys);
					feeCodes.Remove(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
					feeCodes.Remove(Core.Constants.USCustoms.FeeCodes.HMF);

					if (feeCodes.Count > 1)
					{
						feeCodes.Sort((x, y) => x.CompareTo(y));
					}

					if (feeCodes.Count > 0)
					{
						lineFeeCode = feeCodes[0];
					}
				}
				return lineFeeCode.Value;
			}
		}
		ZString? lineFeeCode;

		public abstract ZString LineFeePercentAsString { get; }

		protected abstract ZString LineFeeRateType { get; }

		Dictionary<ZString, ZDecimal> FeesAndTaxes
		{
			get
			{
				if (feesAndTaxes == null)
				{
					feesAndTaxes = PopulateLineFeesAndTaxes();
				}
				return feesAndTaxes;
			}
		}
		Dictionary<ZString, ZDecimal> feesAndTaxes;

		protected abstract Dictionary<ZString, ZDecimal> PopulateLineFeesAndTaxes();

		protected void RefreshFeesAndTaxes()
		{
			feesAndTaxes = null;
		}

		protected ZString GetRateType(ZString feeCode)
		{
			ZString result = ZString.Empty;

			if (feeCode == line.RandomLine.US_TaxCode)
			{
				result = line.RandomLine.US_TaxRateT;
			}
			else
			{
				FeeCusCodeData feeData = line.RandomLine.FeeCusCodes.GetFirstElementHaving(feeCode);
				if (feeData != null)
				{
					result = feeData.CY_SelectedRateType;
				}
			}

			return result;
		}

		public ZBool TaxDeferred
		{
			get
			{
				var declaration = line.Declaration;
				return declaration != null && declaration.TaxDeferred;
			}
		}

		public abstract ZString SecondaryLine1Description { get; }
		public abstract ZString SecondaryLine1FormattedTariff { get; }
		public virtual ZString SecondaryLine1LicenseNumber => ZString.Empty;
		public abstract ZString SecondaryLine1SecondQtyAndUQ { get; }
		public abstract ZString SecondaryLine1ThirdQtyAndUQ { get; }
		public abstract ZString SecondaryLine1ADDNo { get; }
		public abstract ZString SecondaryLine1ADDSurety { get; }
		public abstract ZString SecondaryLine1ADDSpecificDepositValueFormatted { get; }
		public abstract ZString SecondaryLine1ADDRate { get; }
		public abstract ZString SecondaryLine1ADDFormatted { get; }
		public abstract ZString SecondaryLine1CVDNo { get; }
		public abstract ZString SecondaryLine1CVDSurety { get; }
		public abstract ZString SecondaryLine1CVDSpecificDepositValueFormatted { get; }
		public abstract ZString SecondaryLine1CVDRate { get; }
		public abstract ZString SecondaryLine1CVDFormatted { get; }
		public abstract ZDecimal SecondaryLine1CustomsQuantity { get; }
		public abstract ZString SecondaryLine1CustomsUnitQty { get; }
		public abstract ZDecimal SecondaryLine1TotalLinePriceInLocalCurrencyRounded { get; }
		public virtual ZString SecondaryLine1DutyPercentAsString => SecondaryLineDutyPercentAsString(SecondaryTariffLine, secondaryTariffLine1);
		public abstract ZDecimal SecondaryLine1DutyAmount { get; }
		public virtual ZBool PrintSPIOnSecondLine1 => PrintSPIOnSecondaryLine(secondaryTariffLine1);

		public abstract ZString SecondaryLine2Description { get; }
		public abstract ZString SecondaryLine2FormattedTariff { get; }
		public virtual ZString SecondaryLine2LicenseNumber => ZString.Empty;
		public abstract ZString SecondaryLine2SecondQtyAndUQ { get; }
		public abstract ZString SecondaryLine2ThirdQtyAndUQ { get; }
		public abstract ZString SecondaryLine2ADDNo { get; }
		public abstract ZString SecondaryLine2ADDSurety { get; }
		public abstract ZString SecondaryLine2ADDSpecificDepositValueFormatted { get; }
		public abstract ZString SecondaryLine2ADDRate { get; }
		public abstract ZString SecondaryLine2ADDFormatted { get; }
		public abstract ZString SecondaryLine2CVDNo { get; }
		public abstract ZString SecondaryLine2CVDSurety { get; }
		public abstract ZString SecondaryLine2CVDSpecificDepositValueFormatted { get; }
		public abstract ZString SecondaryLine2CVDRate { get; }
		public abstract ZString SecondaryLine2CVDFormatted { get; }
		public abstract ZDecimal SecondaryLine2CustomsQuantity { get; }
		public abstract ZString SecondaryLine2CustomsUnitQty { get; }
		public abstract ZDecimal SecondaryLine2TotalLinePriceInLocalCurrencyRounded { get; }
		public abstract ZString SecondaryLine2DutyPercentAsString { get; }
		public abstract ZDecimal SecondaryLine2DutyAmount { get; }
		public virtual ZBool PrintSPIOnSecondLine2 => PrintSPIOnSecondaryLine(secondaryTariffLine2);

		public abstract ZString SecondaryLine3Description { get; }
		public abstract ZString SecondaryLine3FormattedTariff { get; }
		public virtual ZString SecondaryLine3LicenseNumber => ZString.Empty;
		public abstract ZString SecondaryLine3SecondQtyAndUQ { get; }
		public abstract ZString SecondaryLine3ThirdQtyAndUQ { get; }
		public abstract ZString SecondaryLine3ADDNo { get; }
		public abstract ZString SecondaryLine3ADDSurety { get; }
		public abstract ZString SecondaryLine3ADDSpecificDepositValueFormatted { get; }
		public abstract ZString SecondaryLine3ADDRate { get; }
		public abstract ZString SecondaryLine3ADDFormatted { get; }
		public abstract ZString SecondaryLine3CVDNo { get; }
		public abstract ZString SecondaryLine3CVDSurety { get; }
		public abstract ZString SecondaryLine3CVDSpecificDepositValueFormatted { get; }
		public abstract ZString SecondaryLine3CVDRate { get; }
		public abstract ZString SecondaryLine3CVDFormatted { get; }
		public abstract ZDecimal SecondaryLine3CustomsQuantity { get; }
		public abstract ZString SecondaryLine3CustomsUnitQty { get; }
		public abstract ZDecimal SecondaryLine3TotalLinePriceInLocalCurrencyRounded { get; }
		public virtual ZString SecondaryLine3DutyPercentAsString => SecondaryLineDutyPercentAsString(SecondaryTariffLine3, secondaryTariffLine3);
		public abstract ZDecimal SecondaryLine3DutyAmount { get; }
		public virtual ZBool PrintSPIOnSecondLine3 => PrintSPIOnSecondaryLine(secondaryTariffLine3);

		public abstract ZString SecondaryLine4Description { get; }
		public abstract ZString SecondaryLine4FormattedTariff { get; }
		public virtual ZString SecondaryLine4LicenseNumber => ZString.Empty;
		public abstract ZString SecondaryLine4SecondQtyAndUQ { get; }
		public abstract ZString SecondaryLine4ThirdQtyAndUQ { get; }
		public abstract ZString SecondaryLine4ADDNo { get; }
		public abstract ZString SecondaryLine4ADDSurety { get; }
		public abstract ZString SecondaryLine4ADDSpecificDepositValueFormatted { get; }
		public abstract ZString SecondaryLine4ADDRate { get; }
		public abstract ZString SecondaryLine4ADDFormatted { get; }
		public abstract ZString SecondaryLine4CVDNo { get; }
		public abstract ZString SecondaryLine4CVDSurety { get; }
		public abstract ZString SecondaryLine4CVDSpecificDepositValueFormatted { get; }
		public abstract ZString SecondaryLine4CVDRate { get; }
		public abstract ZString SecondaryLine4CVDFormatted { get; }
		public abstract ZDecimal SecondaryLine4CustomsQuantity { get; }
		public abstract ZString SecondaryLine4CustomsUnitQty { get; }
		public abstract ZDecimal SecondaryLine4TotalLinePriceInLocalCurrencyRounded { get; }
		public abstract ZString SecondaryLine4DutyPercentAsString { get; }
		public abstract ZDecimal SecondaryLine4DutyAmount { get; }
		public virtual ZBool PrintSPIOnSecondLine4 => PrintSPIOnSecondaryLine(secondaryTariffLine4);

		public abstract ZString SecondaryLine5Description { get; }
		public abstract ZString SecondaryLine5FormattedTariff { get; }
		public virtual ZString SecondaryLine5LicenseNumber => ZString.Empty;
		public abstract ZString SecondaryLine5SecondQtyAndUQ { get; }
		public abstract ZString SecondaryLine5ThirdQtyAndUQ { get; }
		public abstract ZString SecondaryLine5ADDNo { get; }
		public abstract ZString SecondaryLine5ADDSurety { get; }
		public abstract ZString SecondaryLine5ADDSpecificDepositValueFormatted { get; }
		public abstract ZString SecondaryLine5ADDRate { get; }
		public abstract ZString SecondaryLine5ADDFormatted { get; }
		public abstract ZString SecondaryLine5CVDNo { get; }
		public abstract ZString SecondaryLine5CVDSurety { get; }
		public abstract ZString SecondaryLine5CVDSpecificDepositValueFormatted { get; }
		public abstract ZString SecondaryLine5CVDRate { get; }
		public abstract ZString SecondaryLine5CVDFormatted { get; }
		public abstract ZDecimal SecondaryLine5CustomsQuantity { get; }
		public abstract ZString SecondaryLine5CustomsUnitQty { get; }
		public abstract ZDecimal SecondaryLine5TotalLinePriceInLocalCurrencyRounded { get; }
		public virtual ZString SecondaryLine5DutyPercentAsString => SecondaryLineDutyPercentAsString(SecondaryTariffLine5, secondaryTariffLine5);
		public abstract ZDecimal SecondaryLine5DutyAmount { get; }
		public virtual ZBool PrintSPIOnSecondLine5 => PrintSPIOnSecondaryLine(secondaryTariffLine5);

		public abstract ZString SecondaryLine6Description { get; }
		public abstract ZString SecondaryLine6FormattedTariff { get; }
		public virtual ZString SecondaryLine6LicenseNumber => ZString.Empty;
		public abstract ZString SecondaryLine6SecondQtyAndUQ { get; }
		public abstract ZString SecondaryLine6ThirdQtyAndUQ { get; }
		public abstract ZString SecondaryLine6ADDNo { get; }
		public abstract ZString SecondaryLine6ADDSurety { get; }
		public abstract ZString SecondaryLine6ADDSpecificDepositValueFormatted { get; }
		public abstract ZString SecondaryLine6ADDRate { get; }
		public abstract ZString SecondaryLine6ADDFormatted { get; }
		public abstract ZString SecondaryLine6CVDNo { get; }
		public abstract ZString SecondaryLine6CVDSurety { get; }
		public abstract ZString SecondaryLine6CVDSpecificDepositValueFormatted { get; }
		public abstract ZString SecondaryLine6CVDRate { get; }
		public abstract ZString SecondaryLine6CVDFormatted { get; }
		public abstract ZDecimal SecondaryLine6CustomsQuantity { get; }
		public abstract ZString SecondaryLine6CustomsUnitQty { get; }
		public abstract ZDecimal SecondaryLine6TotalLinePriceInLocalCurrencyRounded { get; }
		public abstract ZString SecondaryLine6DutyPercentAsString { get; }
		public abstract ZDecimal SecondaryLine6DutyAmount { get; }
		public virtual ZBool PrintSPIOnSecondLine6 => PrintSPIOnSecondaryLine(secondaryTariffLine6);

		public abstract ZString SecondaryLine7Description { get; }
		public abstract ZString SecondaryLine7FormattedTariff { get; }
		public virtual ZString SecondaryLine7LicenseNumber => ZString.Empty;
		public abstract ZString SecondaryLine7SecondQtyAndUQ { get; }
		public abstract ZString SecondaryLine7ThirdQtyAndUQ { get; }
		public abstract ZString SecondaryLine7ADDNo { get; }
		public abstract ZString SecondaryLine7ADDSurety { get; }
		public abstract ZString SecondaryLine7ADDSpecificDepositValueFormatted { get; }
		public abstract ZString SecondaryLine7ADDRate { get; }
		public abstract ZString SecondaryLine7ADDFormatted { get; }
		public abstract ZString SecondaryLine7CVDNo { get; }
		public abstract ZString SecondaryLine7CVDSurety { get; }
		public abstract ZString SecondaryLine7CVDSpecificDepositValueFormatted { get; }
		public abstract ZString SecondaryLine7CVDRate { get; }
		public abstract ZString SecondaryLine7CVDFormatted { get; }
		public abstract ZDecimal SecondaryLine7CustomsQuantity { get; }
		public abstract ZString SecondaryLine7CustomsUnitQty { get; }
		public abstract ZDecimal SecondaryLine7TotalLinePriceInLocalCurrencyRounded { get; }
		public virtual ZString SecondaryLine7DutyPercentAsString => SecondaryLineDutyPercentAsString(SecondaryTariffLine7, secondaryTariffLine7);
		public abstract ZDecimal SecondaryLine7DutyAmount { get; }
		public virtual ZBool PrintSPIOnSecondLine7 => PrintSPIOnSecondaryLine(secondaryTariffLine7);

		public abstract ZDate DateForAD_CVD { get; }

		protected ZString GetFeeRate(ZString feeCode, bool taxDeferred)
		{
			IFeeCalculationDataProvider feeLine = GetFeeLineApplicable(feeCode);

			return feeLine.GetTaxOrFeeRate(feeCode, taxDeferred);
		}

		protected IFeeCalculationDataProvider GetFeeLineApplicable(ZString feeCode)
		{
			IFeeCalculationDataProvider feeLine = this.line;

			if (feeLine.ImportTariff == null || !feeLine.ImportTariff.IsFeeApplicable(feeCode))
			{
				foreach (IFeeCalculationDataProvider childLine in line.ChildSecondaryEntryLines)
				{
					if (childLine.ImportTariff != null && childLine.ImportTariff.IsFeeApplicable(feeCode))
					{
						feeLine = childLine;
						break;
					}
				}
			}

			return feeLine;
		}

		protected virtual bool USComponentsAssembledAbroad
		{
			get { return line.ImportTariff != null && line.ImportTariff.Applies(TariffRuleList.Codes.AssembledAbroadOfUSProducts, line.DateForDutyCalculation); }
		}

		ZString SecondaryLineDutyPercentAsString(bool tariffLineValid, CusEntryLine secondaryLineChecking)
		{
			ZString dutyPercentString = ZString.Empty;

			if (tariffLineValid && secondaryLineChecking != null)
			{
				var isDutyFreeOnSecondaryLine = secondaryLineChecking.CL_DutyPercentAsString.ToUpper() == "FREE";
				if (USComponentsAssembledAbroad && line.IsParentLine && !isDutyFreeOnSecondaryLine)
				{
					var dutyData = new DutyDataProxy(secondaryLineChecking);
					dutyData.CustomsValue = line.CustomsValue.Amount;

					dutyPercentString = line.USComponentsAssembledAbroadDutyRateForPrint(dutyData);
				}
				else if (AdValoremConversionCalculation && isDutyFreeOnSecondaryLine)
				{
					dutyPercentString = fAVRateString;
				}
				else
				{
					dutyPercentString = secondaryLineChecking.CL_DutyPercentAsString;
				}
			}

			return dutyPercentString;
		}

		ZBool PrintSPIOnSecondaryLine(CusEntryLine secondaryLineChecking)
		{
			return PrintSPIOnSecondLine && secondaryLineChecking != null && secondaryLineChecking.IsNormalTariffLine();
		}

		public bool HasSecondaryWatchLine(CusEntryLine line)
		{
			// Chapter 91 - Clocks and watches and parts thereof
			bool result = false;

			foreach (CusEntryLine childline in line.ChildLines)
			{
				if (childline.IsNormalTariffLine())
				{
					result = childline.CL_AdValoremTariff.StartsWith("91");
					break;
				}
			}

			return result;
		}

		public virtual ZBool ProRatedCalculation
		{
			get
			{
				if (proRatedCalculationChecked == null)
				{
					fProRatedCalculation = false;

					if (line.IsRandomLineSPINotCAAndS && HasSecondaryWatchLine(line) && line.ImportTariff != null &&
						line.ImportTariff.Applies(TariffRuleList.Codes.AssembledAbroadOfUSProducts, line.DateForDutyCalculation))
					{
						fProRatedCalculation = true;
					}

					proRatedCalculationChecked = fProRatedCalculation;
				}

				return fProRatedCalculation;
			}
		}
		ZBool fProRatedCalculation;
		object proRatedCalculationChecked;

		public virtual ZBool AdValoremConversionCalculation
		{
			get
			{
				if (!fAdValoremConversionCalculation.HasValue)
				{
					fAdValoremConversionCalculation = false;

					if (HasSecondaryWatchLine(line))
					{
						bool applicableSPICountry = false;

						USCCountry countryOfOrigin = Factory.LoadTop1<USCCountry>(new ZQuery(USCCountrySchema.UC_Code, ((ICusEntryLine)line).CountryOfOrigin));
						if (countryOfOrigin != null)
						{
							applicableSPICountry = countryOfOrigin.IsValidForSPI(((ICusEntryLine)line).SpecialProgramsIndicatorCountry, line.DateForDutyCalculation);
						}

						if (!applicableSPICountry && line.ImportTariff is USCTariff importTariff && importTariff.Applies(TariffRuleList.Codes.RepairTariffs, line.DateForDutyCalculation))
						{
							fAdValoremConversionCalculation = true;
							ExtractAdValoremDetails(importTariff.UE_Tariff);
						}
					}
				}

				return fAdValoremConversionCalculation.Value;
			}
		}
		ZBool? fAdValoremConversionCalculation;

		public virtual ZString AVWatches { get { return fAVWatches; } }
		public virtual ZDecimal AVWatchesDuty { get { return fAVWatchesDuty; } }
		public virtual ZString AVCases { get { return fAVCases; } }
		public virtual ZDecimal AVCasesDuty { get { return fAVCasesDuty; } }
		public virtual ZString AVBracelets { get { return fAVBracelets; } }
		public virtual ZDecimal AVBraceletsDuty { get { return fAVBraceletsDuty; } }
		public virtual ZString AVBatteries { get { return fAVBatteries; } }
		public virtual ZDecimal AVBatteriesDuty { get { return fAVBatteriesDuty; } }
		public virtual ZDecimal AVTotalDuty { get { return fAVTotalDuty; } }
		public virtual ZString AVLine2 { get { return fAVLine2; } }

		void ExtractAdValoremDetails(ZString repairTariff)
		{
			fAVWatches = ZString.Empty;
			fAVCases = ZString.Empty;
			fAVBracelets = ZString.Empty;
			fAVBatteries = ZString.Empty;
			fAVLine2 = ZString.Empty;
			fAVRateString = ZString.Empty;
			fAVWatchesDuty = 0;
			fAVCasesDuty = 0;
			fAVBraceletsDuty = 0;
			fAVBatteriesDuty = 0;
			fAVTotalDuty = 0;

			if (line.IsParentLine)
			{
				var childLineNo = ZInt.Zero;
				var customsValue = ZDecimal.Zero;
				var repairCustomsValue = line.CL_CustomsValue;
				var totalEnteredValueForLine = line.CL_CustomsValue;

				foreach (var childLine in line.ChildLines)
				{
					if (childLine.CL_AdValoremTariff == repairTariff || childLine.IsNormalTariffLine())
					{
						totalEnteredValueForLine += childLine.CL_CustomsValue;

						if (childLine.CL_AdValoremTariff != repairTariff)
						{
							customsValue = repairCustomsValue + childLine.CL_CustomsValue;
						}
						else
						{
							repairCustomsValue = childLine.CL_CustomsValue;
						}
					}

					if (childLine.IsNormalTariffLine())
					{
						childLineNo++;

						if (childLineNo == 1)
						{
							AppendixFDutyCalculator childLineCalculator = ChildLineCalculator(childLine, customsValue);
							fAVWatches = childLine.CustomsQuantity.ToString(childLine.CustomsQuantity.DecimalPlaces) + " x $" + childLineCalculator.DutyResult.PerUnitAmount.Amount.ToString(childLineCalculator.DutyResult.PerUnitAmount.Amount.DecimalPlaces) + " " + childLineCalculator.DutyResult.PerUnitUQ;
							fAVWatchesDuty = childLineCalculator.DutyResult.TotalAmount.Amount;
							fAVTotalDuty += fAVWatchesDuty;
						}
						else if (childLineNo == 2)
						{
							AppendixFDutyCalculator childLineCalculator = ChildLineCalculator(childLine, customsValue);
							fAVCases = "$" + customsValue.ToString(customsValue.DecimalPlaces) + " x " + childLineCalculator.DutyResult.PercentOfValue.ToString(childLineCalculator.DutyResult.PercentOfValue.DecimalPlaces) + "%";
							fAVCasesDuty = childLineCalculator.DutyResult.TotalAmount.Amount;
							fAVTotalDuty += fAVCasesDuty;
						}
						else if (childLineNo == 3)
						{
							AppendixFDutyCalculator childLineCalculator = ChildLineCalculator(childLine, customsValue);
							fAVBracelets = "$" + customsValue.ToString(customsValue.DecimalPlaces) + " x " + childLineCalculator.DutyResult.PercentOfValue.ToString(childLineCalculator.DutyResult.PercentOfValue.DecimalPlaces) + "%";
							fAVBraceletsDuty = childLineCalculator.DutyResult.TotalAmount.Amount;
							fAVTotalDuty += fAVBraceletsDuty;
						}
						else if (childLineNo == 4)
						{
							AppendixFDutyCalculator childLineCalculator = ChildLineCalculator(childLine, customsValue);
							fAVBatteries = "$" + customsValue.ToString(customsValue.DecimalPlaces) + " x " + childLineCalculator.DutyResult.PercentOfValue.ToString(childLineCalculator.DutyResult.PercentOfValue.DecimalPlaces) + "%";
							fAVBatteriesDuty = childLineCalculator.DutyResult.TotalAmount.Amount;
							fAVTotalDuty += fAVBatteriesDuty;
						}
					}
				}

				ZDecimal fAVRate = totalEnteredValueForLine > 0 ? (fAVTotalDuty / totalEnteredValueForLine) * 100 : 0;
				fAVRateString = fAVRate.Truncate(3).ToString() + "%";

				fAVLine2 = "$" + fAVTotalDuty + "/$" + totalEnteredValueForLine.ToString(2) + " (Total Entered Value)" + " = " + fAVRateString;
			}
		}

		AppendixFDutyCalculator ChildLineCalculator(CusEntryLine childLine, ZDecimal customsValue)
		{
			var dutyData = new DutyDataProxy(childLine);
			dutyData.CustomsValue = customsValue;

			return new AppendixFDutyCalculator(dutyData, childLine.Factory);
		}

		ZString fAVWatches;
		ZDecimal fAVWatchesDuty;
		ZString fAVCases;
		ZDecimal fAVCasesDuty;
		ZString fAVBracelets;
		ZDecimal fAVBraceletsDuty;
		ZString fAVBatteries;
		ZDecimal fAVBatteriesDuty;
		ZDecimal fAVTotalDuty;
		ZString fAVLine2;
		ZString fAVRateString;

		public virtual ZString ProRatedLine1
		{
			get
			{
				ZString result = ZString.Empty;

				if (ProRatedCalculation)
				{
					result = line.FormattedTariff + " (Free) " + ComponentValue + " / " + totalValueForLine + " (Total Value) = " + ProRatedPercent + "%";
				}

				return result;
			}
		}

		ZDecimal totalValueForLine
		{
			get
			{
				ZDecimal result = line.RoundedCustomsValue;

				foreach (CusEntryLine childLine in line.ChildLines)
				{
					result = result + childLine.RoundedCustomsValue;
				}

				return result.Round(0);
			}
		}

		ZDecimal ComponentValue
		{
			get
			{
				var result = line.CustomsValueRounded;
				int childLineNo = 0;

				foreach (CusEntryLine childLine in line.ChildLines)
				{
					childLineNo++;
					if (childLineNo == 2 || childLineNo == 4 || childLineNo == 6)
					{
						result = result + childLine.CustomsValueRounded;
					}
				}

				return result;
			}
		}

		ZDecimal ProRatedPercent
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (totalValueForLine > 0)
				{
					result = (ComponentValue / totalValueForLine) * 100;
				}

				return result.Round(3);
			}
		}

		public virtual ZString ProRatedLine2
		{
			get
			{
				ZString result = ZString.Empty;

				if (ProRatedCalculation)
				{
					result = ProRatedPercent + "% x $" + TotalComponentDuty + " (Total Duty Column 34) = $" + ProRatedDuty.Round(2);
				}

				return result;
			}
		}

		public ZBool PrintSPIOnSecondLine
		{
			get
			{
				if (!fShouldPrintSPIOnFirstLine.HasValue)
				{
					fShouldPrintSPIOnFirstLine = GetPrintSPIOnSecondLineCore();
				}

				return fShouldPrintSPIOnFirstLine.Value;
			}
		}
		ZBool? fShouldPrintSPIOnFirstLine;

		protected virtual ZBool GetPrintSPIOnSecondLineCore()
		{
			return SecondaryTariffLine && (line.ImportTariff == null || !line.ImportTariff.Applies(TariffRuleList.Codes.InLieuTariffs, line.DateForDutyCalculation));
		}

		ZDecimal TotalComponentDuty
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (line.IsParentLine)
				{
					int childLineNo = 0;
					ZDecimal customsValue = 0;

					foreach (CusEntryLine childLine in line.ChildLines)
					{
						childLineNo++;
						if (childLineNo == 1)
						{
							customsValue = line.CustomsValue.Amount + childLine.CustomsValue.Amount;
						}
						else if (childLineNo == 2 || childLineNo == 4 || childLineNo == 6)
						{
							customsValue = childLine.CustomsValue.Amount;
						}
						else if (childLineNo == 3 || childLineNo == 5 || childLineNo == 7)
						{
							customsValue += childLine.CustomsValue.Amount;
						}

						if (childLineNo == 1 || childLineNo == 3 || childLineNo == 5 || childLineNo == 7)
						{
							var dutyData = new DutyDataProxy(childLine);
							dutyData.CustomsValue = customsValue;

							AppendixFDutyCalculator childLineCalculator = new AppendixFDutyCalculator(dutyData, line.Factory);

							result += childLineCalculator.DutyResult.TotalAmount.Amount.Round(2);
						}
					}
				}

				return result;
			}
		}

		public virtual ZString ProRatedLine3
		{
			get
			{
				ZString result = ZString.Empty;

				if (ProRatedCalculation)
				{
					result = "$" + TotalComponentDuty + " - $" + ProRatedDuty.Round(2) + " = $" + (TotalComponentDuty - ProRatedDuty.Round(2)).ToString() + " (Total Duty Due)";
				}

				return result;
			}
		}

		ZDecimal ProRatedDuty
		{
			get { return ProRatedPercent * TotalComponentDuty / 100; }
		}

		#endregion
	}
}
