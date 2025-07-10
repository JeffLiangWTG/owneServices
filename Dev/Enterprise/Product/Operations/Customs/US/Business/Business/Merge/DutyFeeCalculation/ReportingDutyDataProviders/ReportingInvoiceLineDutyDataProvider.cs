using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class ReportingInvoiceLineDutyDataProvider : IInvoiceLineDutyDataProvider, IDutyData
	{
		public ReportingInvoiceLineDutyDataProvider(JobComInvoiceLine invoiceLine, ReportingDeclarationDutyDataProvider reportingDeclaration)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
			this.reportingDeclaration = Argument.NotNull(reportingDeclaration, nameof(reportingDeclaration));
			dutyData = invoiceLine;
			field = new Dictionary<string, object>();
			field.Add(JobComInvoiceLine.Schema.US_ADDuty, ZDecimal.Zero);
			field.Add(JobComInvoiceLine.Schema.US_CVDuty, ZDecimal.Zero);
			field.Add(JobComInvoiceLine.Schema.US_Duty, ZDecimal.Zero);
			field.Add(JobComInvoiceLine.Schema.US_SupDuty, ZDecimal.Zero);
			field.Add(JobComInvoiceLine.Schema.US_SupAdditionalTariff1Duty, ZDecimal.Zero);
			field.Add(JobComInvoiceLine.Schema.US_SupAdditionalTariff2Duty, ZDecimal.Zero);
			field.Add(JobComInvoiceLine.Schema.US_SupAdditionalTariff3Duty, ZDecimal.Zero);
			field.Add(JobComInvoiceLine.Schema.US_SupAdditionalTariff4Duty, ZDecimal.Zero);
			field.Add(JobComInvoiceLine.Schema.US_SupAdditionalTariff5Duty, ZDecimal.Zero);
			field.Add(JobComInvoiceLine.Schema.US_PayableMPF, ZDecimal.Zero);
			field.Add(JobComInvoiceLine.Schema.US_OverrideDuty, ZBool.False);
			field.Add(JobComInvoiceLine.Schema.US_OverrideSupDuty, ZBool.False);
			field.Add(JobComInvoiceLine.Schema.US_OverrideSupAdditionalTariff1Duty, ZBool.False);
			field.Add(JobComInvoiceLine.Schema.US_OverrideSupAdditionalTariff2Duty, ZBool.False);
			field.Add(JobComInvoiceLine.Schema.US_OverrideSupAdditionalTariff3Duty, ZBool.False);
			field.Add(JobComInvoiceLine.Schema.US_OverrideSupAdditionalTariff4Duty, ZBool.False);
			field.Add(JobComInvoiceLine.Schema.US_OverrideSupAdditionalTariff5Duty, ZBool.False);
		}

		internal void StoreResult(string dutyColumn, string payableMPFColumn, string spiColumn)
		{
			invoiceLine[dutyColumn] = US_Duty;
			invoiceLine[payableMPFColumn] = US_PayableMPF;
			if (spiColumn != null)
			{
				invoiceLine[spiColumn] = SpecialProgramsIndicatorCountry;
			}
		}

		readonly JobComInvoiceLine invoiceLine;
		readonly ReportingDeclarationDutyDataProvider reportingDeclaration;
		readonly IDutyData dutyData;

		public bool IsDutyFreeSPIClaimed
		{
			get
			{
				return (invoiceLine.ImportTariff?.BecomesDutyFreeDueTo(SpecialProgramsIndicatorCountry) ?? false) ||
					(invoiceLine.ImportSupTariff?.BecomesDutyFreeDueTo(SpecialProgramsIndicatorCountry) ?? false);
			}
		}

		object IInvoiceLineDutyDataProvider.this[string propertyName]
		{
			get => field[propertyName];
			set => field[propertyName] = value;
		}
		readonly Dictionary<string, object> field;

		T GetValue<T>(string propertyName)
			where T : IZType
		{
			return field.TryGetValue(propertyName, out var result) ? (T)result : default(T);
		}

		void SetValue(string propertyName, object value)
		{
			field[propertyName] = value;
		}

		ZBool IInvoiceLineDutyDataProvider.IsADDManual => invoiceLine.IsADDManual;

		ZBool IInvoiceLineDutyDataProvider.IsCVDManual => invoiceLine.IsCVDManual;

		bool IInvoiceLineDutyDataProvider.IsInformal => invoiceLine.IsInformal;

		ZDecimal IInvoiceLineDutyDataProvider.JI_CustomsValue => invoiceLine.JI_CustomsValue;

		ZDecimal IInvoiceLineDutyDataProvider.TotalOriginalGoodsValueInUSD => invoiceLine.TotalOriginalGoodsValueInUSD;

		ZGuid IInvoiceLineDutyDataProvider.PK => invoiceLine.PK;

		ZShort IInvoiceLineDutyDataProvider.JI_LineNo => invoiceLine.JI_LineNo;

		ZGuid IInvoiceLineDutyDataProvider.JI_ParentID => invoiceLine.JI_ParentID;

		ZString IInvoiceLineDutyDataProvider.JI_AddInfo => invoiceLine.JI_AddInfo;

		ZBool IInvoiceLineDutyDataProvider.US_OverrideDuty => invoiceLine.US_OverrideDuty;

		ZBool IInvoiceLineDutyDataProvider.US_OverrideSupDuty => invoiceLine.US_OverrideSupDuty;

		ZBool IInvoiceLineDutyDataProvider.US_OverrideSupAdditionalTariff1Duty => invoiceLine.US_OverrideSupAdditionalTariff1Duty;
		ZBool IInvoiceLineDutyDataProvider.US_OverrideSupAdditionalTariff2Duty => invoiceLine.US_OverrideSupAdditionalTariff2Duty;
		ZBool IInvoiceLineDutyDataProvider.US_OverrideSupAdditionalTariff3Duty => invoiceLine.US_OverrideSupAdditionalTariff3Duty;
		ZBool IInvoiceLineDutyDataProvider.US_OverrideSupAdditionalTariff4Duty => invoiceLine.US_OverrideSupAdditionalTariff4Duty;
		ZBool IInvoiceLineDutyDataProvider.US_OverrideSupAdditionalTariff5Duty => invoiceLine.US_OverrideSupAdditionalTariff5Duty;

		ZBool IInvoiceLineDutyDataProvider.IsQuotaProductExclusion => ((IInvoiceLineDutyDataProvider)invoiceLine).IsQuotaProductExclusion;

		ZBool IInvoiceLineDutyDataProvider.HasSupTariffOnly => ((IInvoiceLineDutyDataProvider)invoiceLine).HasSupTariffOnly;

		public ZDecimal US_ADDuty { get => GetValue<ZDecimal>(JobComInvoiceLine.Schema.US_ADDuty); set => SetValue(JobComInvoiceLine.Schema.US_ADDuty, value); }
		public ZDecimal US_CVDuty { get => GetValue<ZDecimal>(JobComInvoiceLine.Schema.US_CVDuty); set => SetValue(JobComInvoiceLine.Schema.US_CVDuty, value); }
		public ZDecimal US_Duty { get => GetValue<ZDecimal>(JobComInvoiceLine.Schema.US_Duty); set => SetValue(JobComInvoiceLine.Schema.US_Duty, value); }
		public ZDecimal US_SupDuty { get => GetValue<ZDecimal>(JobComInvoiceLine.Schema.US_SupDuty); set => SetValue(JobComInvoiceLine.Schema.US_SupDuty, value); }
		public ZDecimal US_PayableMPF { get => GetValue<ZDecimal>(JobComInvoiceLine.Schema.US_PayableMPF); set => SetValue(JobComInvoiceLine.Schema.US_PayableMPF, value); }
		public ZDecimal US_SupAdditionalTariff1Duty { get => GetValue<ZDecimal>(JobComInvoiceLine.Schema.US_SupAdditionalTariff1Duty); set => SetValue(JobComInvoiceLine.Schema.US_SupAdditionalTariff1Duty, value); }
		public ZDecimal US_SupAdditionalTariff2Duty { get => GetValue<ZDecimal>(JobComInvoiceLine.Schema.US_SupAdditionalTariff2Duty); set => SetValue(JobComInvoiceLine.Schema.US_SupAdditionalTariff2Duty, value); }
		public ZDecimal US_SupAdditionalTariff3Duty { get => GetValue<ZDecimal>(JobComInvoiceLine.Schema.US_SupAdditionalTariff3Duty); set => SetValue(JobComInvoiceLine.Schema.US_SupAdditionalTariff3Duty, value); }
		public ZDecimal US_SupAdditionalTariff4Duty { get => GetValue<ZDecimal>(JobComInvoiceLine.Schema.US_SupAdditionalTariff4Duty); set => SetValue(JobComInvoiceLine.Schema.US_SupAdditionalTariff4Duty, value); }
		public ZDecimal US_SupAdditionalTariff5Duty { get => GetValue<ZDecimal>(JobComInvoiceLine.Schema.US_SupAdditionalTariff5Duty); set => SetValue(JobComInvoiceLine.Schema.US_SupAdditionalTariff5Duty, value); }

		ZShort? IInvoiceLineDutyDataProvider.InvoiceDisplaySequence => ((IInvoiceLineDutyDataProvider)invoiceLine).InvoiceDisplaySequence;

		public IFees FeeCusCodes => feeCusCodes ?? (feeCusCodes = CreateFeeCusCodes());
		ReportingFeesDutyDataProvider feeCusCodes;

		ReportingFeesDutyDataProvider CreateFeeCusCodes()
		{
			var result = new ReportingFeesDutyDataProvider();
			foreach (var fee in invoiceLine.FeeCusCodes.OfType<FeeCusCodeData>().Where(x => x.CY_IsOverridden))
			{
				var tax = result.AddNew();
				tax.Code = fee.CY_Code;
				tax.Amount = fee.CY_FeeAmount;
				tax.SelectedRateType = fee.CY_SelectedRateType;
				tax.IsOverridden = true;
			}
			return result;
		}

		IEnumerable<IEntryLineDutyDataProvider> IInvoiceLineDutyDataProvider.AllEntryLines
		{
			get
			{
				if (entryLines == null)
				{
					entryLines = ((IInvoiceLineDutyDataProvider)invoiceLine).AllEntryLines.Cast<CusEntryLine>().Select(x => reportingDeclaration.GetOrCreate(x)).ToArray();
				}
				return entryLines;
			}
		}
		ReportingEntryLineDutyDataProvider[] entryLines;

		ZString IDutyData.Tariff => dutyData.Tariff;

		USCTariff IDutyData.ImportTariff => dutyData.ImportTariff;

		ZDate IDutyData.DateForDutyCalculation => dutyData.DateForDutyCalculation;

		ZDecimal IDutyData.Quantity1 => dutyData.Quantity1;

		ZString IDutyData.UQ1 => dutyData.UQ1;

		ZDecimal IDutyData.Quantity2 => dutyData.Quantity2;

		ZString IDutyData.UQ2 => dutyData.UQ2;

		ZDecimal IDutyData.Quantity3 => dutyData.Quantity3;

		ZString IDutyData.UQ3 => dutyData.UQ3;

		ZDecimal IDutyData.CustomsValue => dutyData.CustomsValue;

		ZDecimal IDutyData.SupCustomsValue => dutyData.SupCustomsValue;

		ZString IDutyData.SpecialProgramsIndicatorPrimary => ZString.Empty;

		public ZString SpecialProgramsIndicatorCountry
		{
			get
			{
				if (!spi.HasValue)
				{
					spi = reportingDeclaration.CalculateNewSPI(invoiceLine.US_UC_NKCountryOfOrigin, invoiceLine.AddInfoLookups.SPIList);
				}
				return spi.Value;
			}
		}
		ZString? spi;

		ZString IDutyData.CountryOfOrigin => dutyData.CountryOfOrigin;

		ZString IDutyData.SpecialProgramsIndicatorSecondary => dutyData.SpecialProgramsIndicatorSecondary;

		ZString IDutyData.SelectedRateType => dutyData.SelectedRateType;

		BusinessObjectFactory IDutyData.Factory => dutyData.Factory;

		ZDecimal IDutyData.ValueForADD => dutyData.ValueForADD;

		ZDecimal IDutyData.ADDDepositRate => dutyData.ADDDepositRate;

		ZString IDutyData.ADDCaseRateTypeQualifier => dutyData.ADDCaseRateTypeQualifier;

		ZDecimal IDutyData.ADDQuantity => dutyData.ADDQuantity;

		ZDecimal? IDutyData.ADDutyManual => dutyData.ADDutyManual;

		ZDecimal IDutyData.ValueForCVD => dutyData.ValueForCVD;

		ZDecimal IDutyData.CVDDepositRate => dutyData.CVDDepositRate;

		ZString IDutyData.CVDCaseRateTypeQualifier => dutyData.CVDCaseRateTypeQualifier;

		ZDecimal IDutyData.CVDQuantity => dutyData.CVDQuantity;

		ZDecimal? IDutyData.CVDutyManual => dutyData.CVDutyManual;

		IDutyData IDutyData.ParentTariffLine
		{
			get
			{
				if (parentLine == null)
				{
					var line = invoiceLine.ParentTariffLine;
					parentLine = line == null ? null : reportingDeclaration.GetOrCreate(line);
				}
				return parentLine;
			}
		}
		ReportingInvoiceLineDutyDataProvider parentLine;

		bool IDutyData.IsCottonFeeExemptIndicated => dutyData.IsCottonFeeExemptIndicated;

		bool IDutyData.HasCottonCertificate => dutyData.HasCottonCertificate;

		ZBool IDutyData.IsSetXLine => dutyData.IsSetXLine;

		ZBool IDutyData.IsSetVLine => dutyData.IsSetVLine;

		bool IDutyData.IsAMSFeeExempt => dutyData.IsAMSFeeExempt;

		bool IDutyData.IsRaspberryFeeExempt => dutyData.IsRaspberryFeeExempt;

		ZString IDutyData.EntryType => dutyData.EntryType;

		bool IDutyData.IsClearedInPR => dutyData.IsClearedInPR;

		bool IDutyData.IsSecondaryTariffLine => dutyData.IsSecondaryTariffLine;

		bool IDutyData.IsDomesticMerchandise => dutyData.IsDomesticMerchandise;

		bool IDutyData.HasTextileCategoryNo => dutyData.HasTextileCategoryNo;

		bool IDutyData.IsCombineSecondaryTariffLine => dutyData.IsCombineSecondaryTariffLine;

		IEnumerable<IDutyData> IDutyData.CombineChildLines => combineChildLines ?? (combineChildLines = dutyData.CombineChildLines.Cast<JobComInvoiceLine>().Select(x => reportingDeclaration.GetOrCreate(x)).ToArray());
		ReportingInvoiceLineDutyDataProvider[] combineChildLines;
		IReadOnlyList<ZString> IDutyData.SupTariffs => dutyData.SupTariffs;

		IDutyData IDutyData.CombineParentLine
		{
			get
			{
				if (combineParentLine == null)
				{
					var line = (JobComInvoiceLine)dutyData.CombineParentLine;
					combineParentLine = line == null ? null : reportingDeclaration.GetOrCreate(line);
				}
				return combineParentLine;
			}
		}
		ReportingInvoiceLineDutyDataProvider combineParentLine;
		IEnumerable<IDutyData> IDutyData.CombineAllLines => combineAllLines ?? (combineAllLines = dutyData.CombineAllLines.Cast<JobComInvoiceLine>().Select(x => reportingDeclaration.GetOrCreate(x)).ToArray());
		ReportingInvoiceLineDutyDataProvider[] combineAllLines;
	}
}
