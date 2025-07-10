using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[SingleObjectAroundARow]
	public class USImportEntryLine : AutoUSImportEntryLine, IDrawbackEntryLine, IDutyData
	{
		public USImportEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			SuspendValidation();
			var entryLinePKToLoadInvoiceLine = EntryLinePKToLoadInvoiceLine;
			lock (entryLinePKToLoadInvoiceLine)
			{
				entryLinePKToLoadInvoiceLine.Add(PK);
			}
			Factory.AddFetchHint(USImportEntryInvoiceLineSchema.USL_CL, PK);
			Factory.AddFetchHint(USImportEntryLineFeeSchema.USF_CL, PK);
		}

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public USImportEntryLine[] AllRelatedEntryLines => Factory.GetCachedValue("AllUSDrawbackEntryLines" + USE_CH.ToStringKey(), () =>
		{
			return Factory.Load<USImportEntryLine>(new ZQuery(USImportEntryLineSchema.USE_CH, USE_CH));
		});
		IDrawbackEntryLine[] IDrawbackEntryLine.AllDrawbackEntryLines => AllRelatedEntryLines;
		IEnumerable<IEntryLine> IEntryLine.AllRelatedEntryLines => AllRelatedEntryLines;

		public ZDateTime EntryDate => USE_EntryDate;

		public ZString EntryPort => USE_EntryPort;

		public bool IsFTZAdmission => USE_IsFTZAdmission;

		public ZString BaseDutyRateDesc => USE_DutyRateDesc;
		public ZString DutyRateDescription
		{
			get
			{
				if (!dutyRateDescriptionCached.HasValue)
				{
					dutyRateDescriptionCached = Helper.GetDutyRateDesc();
				}
				return dutyRateDescriptionCached.Value;
			}
		}
		ZString? dutyRateDescriptionCached;

		public bool NoDutyRateExists
		{
			get
			{
				if (!noDutyRateExistsCached.HasValue)
				{
					noDutyRateExistsCached = DutyRateWrapper.GetWrapper(this)?.IsInvalidDutyRate() ?? false;
				}
				return noDutyRateExistsCached.Value;
			}
		}
		bool? noDutyRateExistsCached;

		public ZString InvoiceNumber => RandomLine.InvoiceNumber;

		public ZString PartNo => RandomLine.JI_PartNo;

		public ZDecimal CustomsQuantity
		{
			get
			{
				if (!customsQuantityCached.HasValue)
				{
					customsQuantityCached = Helper.GetCustomsQuantity();
				}
				return customsQuantityCached.Value;
			}
		}
		ZDecimal? customsQuantityCached;

		public ZString CustomsUnitQty
		{
			get
			{
				if (!customsUnitQtyCached.HasValue)
				{
					customsUnitQtyCached = Helper.GetCustomsUnitQty();
				}
				return customsUnitQtyCached.Value;
			}
		}
		ZString? customsUnitQtyCached;

		public ZDecimal SecondCustomsQuantity
		{
			get
			{
				if (!secondCustomsQuantityCached.HasValue)
				{
					secondCustomsQuantityCached = Helper.GetSecondCustomsQuantity();
				}
				return secondCustomsQuantityCached.Value;
			}
		}
		ZDecimal? secondCustomsQuantityCached;

		public ZString SecondCustomsUnitQty
		{
			get
			{
				if (!secondCustomsUnitQtyCached.HasValue)
				{
					secondCustomsUnitQtyCached = Helper.GetSecondCustomsUnitQty();
				}
				return secondCustomsUnitQtyCached.Value;
			}
		}
		ZString? secondCustomsUnitQtyCached;

		public ZDecimal ThirdCustomsQuantity
		{
			get
			{
				if (!thirdCustomsQuantityCached.HasValue)
				{
					thirdCustomsQuantityCached = Helper.GetThirdCustomsQuantity();
				}
				return thirdCustomsQuantityCached.Value;
			}
		}
		ZDecimal? thirdCustomsQuantityCached;

		public ZString ThirdCustomsUnitQty
		{
			get
			{
				if (!thirdCustomsUnitQtyCached.HasValue)
				{
					thirdCustomsUnitQtyCached = Helper.GetThirdCustomsUnitQty();
				}
				return thirdCustomsUnitQtyCached.Value;
			}
		}
		ZString? thirdCustomsUnitQtyCached;

		public ZDecimal ExciseTax
		{
			get
			{
				LoadFeesAndGatherData();
				return exciseTax;
			}
		}

		public ZDecimal HMFAmount
		{
			get
			{
				LoadFeesAndGatherData();
				return hmfAmountCached.Value;
			}
		}

		public ZDecimal HMFAmountForEntry => USE_HMFAmountForEntry;

		public ZBool US_HasMPF => USE_HasMPF;
		public ZBool US_SupLine => USE_SupLine;
		public ZBool US_SupAdditionalLine => false;
		public ZBool US_SupAdditionalLine2 => false;
		public ZBool US_SupAdditionalLine3 => false;
		public ZBool US_SupAdditionalLine4 => false;
		public ZBool US_SupAdditionalLine5 => false;

		public ZDecimal MPFAmount
		{
			get
			{
				LoadFeesAndGatherData();
				return mpfAmountCached.Value;
			}
		}

		public ZDecimal MPFAmountForEntry => USE_MPFAmountForEntry;

		public ZDecimal PayableMPFAmount
		{
			get
			{
				LoadInvoiceLinesAndGatherData();
				return payableMPFAmount;
			}
		}
		ZDecimal payableMPFAmount;

		public ZDecimal TotalFeeAmount
		{
			get
			{
				LoadFeesAndGatherData();
				return totalFeeAmount;
			}
		}

		public ZDecimal TotalEnteredValueForEntry => RandomLine.USL_TotalEnteredValue;

		public ZDecimal RoundedCustomsValue
		{
			get
			{
				if (!roundedCustomsValueCached.HasValue)
				{
					roundedCustomsValueCached = Helper.GetRoundedCustomsValue();
				}
				return roundedCustomsValueCached.Value;
			}
		}
		ZDecimal? roundedCustomsValueCached;
		public ZBool IsSecondaryTariffLine
		{
			get
			{
				if (!isSecondaryTariffLineCached.HasValue)
				{
					isSecondaryTariffLineCached = Helper.GetIsSecondaryTariffLine();
				}
				return isSecondaryTariffLineCached.Value;
			}
		}
		ZBool? isSecondaryTariffLineCached;

		public USImportEntryLine ParentLine
		{
			get
			{
				if (parentLineCached == null)
				{
					parentLineCached = new CachedValue<USImportEntryLine>(() => Factory.Load<USImportEntryLine>(USE_CL_ParentLine));
				}
				return parentLineCached.Value;
			}
		}
		CachedValue<USImportEntryLine> parentLineCached;
		IDrawbackEntryLine IDrawbackEntryLine.ParentLine => ParentLine;
		IEntryLine IEntryLine.ParentLine => ParentLine;

		IEnumerable<IFee> IDrawbackEntryLine.Fees => Fees;
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public USImportEntryLineFee[] Fees
		{
			get
			{
				LoadFeesAndGatherData();
				return fees;
			}
		}
		USImportEntryLineFee[] fees;

		void LoadFeesAndGatherData()
		{
			if (fees == null)
			{
				fees = Factory.Load<USImportEntryLineFee>(new ZQuery(USImportEntryLineFeeSchema.USF_CL, PK)).OrderBy(x => x.USF_ChargeType).ThenBy(x => x.PK).ToArray();
				dutyAmountCached = null;
				hmfAmountCached = null;
				mpfAmountCached = null;
				exciseTax = ZDecimal.Zero;
				totalFeeAmount = ZDecimal.Zero;
				foreach (var fee in fees)
				{
					totalFeeAmount += fee.USF_ChargeAmount;
					if (!fee.USF_IsLandedCostOnly)
					{
						switch (fee.USF_ChargeType)
						{
							case Core.Constants.Customs.CusEntryFeeTypes.DutyAmount:
								if (!dutyAmountCached.HasValue)
								{
									dutyAmountCached = fee.USF_ChargeAmount;
								}
								break;
							case Core.Constants.USCustoms.FeeCodes.HMF:
								if (!dutyAmountCached.HasValue)
								{
									hmfAmountCached = fee.USF_ChargeAmount;
								}
								break;
							case Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing:
								if (!dutyAmountCached.HasValue)
								{
									mpfAmountCached = fee.USF_ChargeAmount;
								}
								break;
						}
					}
					if (CusFeeCodeConstants.IsExciseTax(fee.USF_ChargeType))
					{
						exciseTax += fee.USF_ChargeAmount;
					}
				}
				if (!dutyAmountCached.HasValue)
				{
					dutyAmountCached = ZDecimal.Zero;
				}
				if (!hmfAmountCached.HasValue)
				{
					hmfAmountCached = ZDecimal.Zero;
				}
				if (!mpfAmountCached.HasValue)
				{
					mpfAmountCached = ZDecimal.Zero;
				}
			}
		}
		ZDecimal? dutyAmountCached;
		ZDecimal? hmfAmountCached;
		ZDecimal? mpfAmountCached;
		ZDecimal exciseTax;
		ZDecimal totalFeeAmount;

		IEnumerable<IEntryLine> IEntryLine.ChildSecondaryEntryLines => ChildSecondaryEntryLines;
		IEnumerable<IDrawbackEntryLine> IDrawbackEntryLine.ChildSecondaryEntryLines => ChildSecondaryEntryLines;
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public USImportEntryLine[] ChildSecondaryEntryLines => childSecondaryEntryLines ?? (childSecondaryEntryLines = ChildLines.Where(x => x.IsSecondaryTariffLine).ToArray());
		USImportEntryLine[] childSecondaryEntryLines;

		public EntryLineHelper Helper => helper ?? (helper = new EntryLineHelper(this));
		EntryLineHelper helper;

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public USImportEntryLine[] ChildLines => fChildLines ?? (fChildLines = Helper.GetChildLines().Cast<USImportEntryLine>().ToArray());
		USImportEntryLine[] fChildLines;
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public USImportEntryLine[] ChildVLines => fChildVLines ?? (fChildVLines = ChildLines.Where(x => x.IsSetVLine && !x.CL_AdValoremTariff.IsEmpty).ToArray());
		USImportEntryLine[] fChildVLines;

		public ZShort CL_LineNumber => USE_LineNumber;

		public ZString CL_AdValoremTariff => USE_AdValoremTariff;

		public ZString CL_Description => USE_Description;

		public ZString InvoiceUQ => RandomLine.USL_InvoiceUQ;

		public ZDecimal CL_CustomsValue => USE_CustomsValue;

		public ZDecimal DutyAmount
		{
			get
			{
				LoadFeesAndGatherData();
				return dutyAmountCached.Value;
			}
		}

		public ZDecimal InvoiceQuantity
		{
			get
			{
				LoadInvoiceLinesAndGatherData();
				return invoiceQuantity;
			}
		}
		ZDecimal invoiceQuantity;

		public ZDecimal GetFeeAmount(ZString feeTypeCode)
		{
			var result = ZDecimal.Zero;
			switch (feeTypeCode)
			{
				case Core.Constants.Customs.CusEntryFeeTypes.DutyAmount:
					result = DutyAmount;
					break;
				case Core.Constants.USCustoms.FeeCodes.HMF:
					result = HMFAmount;
					break;
				case Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing:
					result = MPFAmount;
					break;
				default:
					feeAmounts = feeAmounts ?? new Dictionary<ZString, ZDecimal>();
					if (!feeAmounts.TryGetValue(feeTypeCode, out result))
					{
						result = Fees.FirstOrDefault(x => !x.USF_IsLandedCostOnly && x.USF_ChargeType == feeTypeCode)?.USF_ChargeAmount ?? ZDecimal.Zero;
						feeAmounts.Add(feeTypeCode, result);
					}
					break;
			}
			return result;
		}
		Dictionary<ZString, ZDecimal> feeAmounts;

		IEnumerable<IInvoiceLine> IEntryLine.InvoiceLines => InvoiceLines;
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public USImportEntryInvoiceLine[] InvoiceLines
		{
			get
			{
				LoadInvoiceLinesAndGatherData();
				return invoiceLines;
			}
		}
		void LoadInvoiceLinesAndGatherData()
		{
			if (invoiceLines == null)
			{
				invoiceQuantity = ZDecimal.Zero;
				payableMPFAmount = ZDecimal.Zero;
				var result = new List<USImportEntryInvoiceLine>();
				var query = new ZQuery(USImportEntryInvoiceLineSchema.USL_CL, PK);
				var invoiceLinePKs = GetInvoiceLinePKsFromCusUnderbond();
				if (invoiceLinePKs.Length > 0)
				{
					query.AddToFilter(JoinCondition.Or, USImportEntryInvoiceLineSchema.PK, invoiceLinePKs);
				}
				foreach (var invoiceLine in Factory.Load<USImportEntryInvoiceLine>(query).OrderBy(x => x.USL_InvoiceNumber.IsEmpty ? x.USL_JZ.ToString() : x.USL_InvoiceNumber.ToString()).ThenBy(x => x.ParentTariffLine?.USL_LineNo ?? x.USL_LineNo).ThenBy(x => x.PK))
				{
					result.Add(invoiceLine);
					invoiceQuantity += invoiceLine.USL_InvoiceQuantity;
					payableMPFAmount += invoiceLine.USL_PayableMPF;
				}
				invoiceLines = result.ToArray();
			}
		}
		USImportEntryInvoiceLine[] invoiceLines;

		ZGuid[] GetInvoiceLinePKsFromCusUnderbond()
		{
			var dictionary = EntryLinePK_InvoiceLinePKsDictionary;
			if (!dictionary.TryGetValue(PK, out var result))
			{
				var entryLinePKToLoadInvoiceLine = EntryLinePKToLoadInvoiceLine;
				if (entryLinePKToLoadInvoiceLine.Count > 0)
				{
					lock (entryLinePKToLoadInvoiceLine)
					{
						foreach (var batch in entryLinePKToLoadInvoiceLine.Batch(MaximumParametersPerFetchHint).ToArray().Where(x => x.Any()))
						{
							var parameters = new ZSqlParameterCollection();
							var parameterStringBuilder = new ZStringBuilder();
							var i = 0;
							foreach (var entryLinePK in batch)
							{
								var parameterName = $"@USEL_PK" + i.ToString(CultureInfo.InvariantCulture);
								parameterStringBuilder.Append(parameterName);
								parameters.Add(parameterName, entryLinePK, CusUnderbondDecSchema.BU_CL);
								i++;
							}
							var collection = new DynamicBusinessObjectCollection(Factory);
							collection.Load($"SELECT {CusUnderbondDecSchema.Constants.BU_CL}, {CusUnderbondDecSchema.Constants.BU_JI} FROM {CusUnderbondDecSchema.Constants.SqlSchemaName}.{CusUnderbondDecSchema.Constants.TableName} WHERE {CusUnderbondDecSchema.Constants.BU_CL} IN ({parameterStringBuilder.ToStringWithDelimiterBetweenAppends(",")})", parameters);
							foreach (var groupByData in collection.Select(x => (new ZGuid(x[CusUnderbondDecSchema.Constants.BU_CL]), new ZGuid(x[CusUnderbondDecSchema.Constants.BU_JI]))).GroupBy(x => x.Item1))
							{
								var invoiceLinePKs = groupByData.Select(x => x.Item2).ToArray();
								dictionary.Add(groupByData.Key, invoiceLinePKs);
								if (groupByData.Key == PK)
								{
									result = invoiceLinePKs;
								}
							}
						}
						entryLinePKToLoadInvoiceLine.Clear();
					}
				}
				if (result == null && !dictionary.TryGetValue(PK, out result)) // need to recheck the dictionary as data could have been added above
				{
					result = Array.Empty<ZGuid>();
					dictionary.Add(PK, result);
				}
			}
			return result;
		}
		HashSet<ZGuid> EntryLinePKToLoadInvoiceLine => Factory.GetCachedValue("EntryLinePKToLoadInvoiceLine", () => new HashSet<ZGuid>());
		Dictionary<ZGuid, ZGuid[]> EntryLinePK_InvoiceLinePKsDictionary => Factory.GetCachedValue("EntryLinePK_InvoiceLinePKsDictionary", () => new Dictionary<ZGuid, ZGuid[]>());
		int MaximumParametersPerFetchHint => Factory.GetCachedValue("MaximumParametersPerFetchHint", () => ObjectFactory.Get<IEntityFrameworkSettings>().MaximumParametersPerFetchHint);
		IInvoiceLine IEntryLine.RandomLine => RandomLine;

		public USImportEntryInvoiceLine RandomLine => randomLine ?? (randomLine = Helper.GetRandomLine() as USImportEntryInvoiceLine ?? Factory.GetNull<USImportEntryInvoiceLine>());
		USImportEntryInvoiceLine randomLine;

		IInvoiceLine IEntryLine.FirstInvoiceLineAfterSortedOnInvoiceLineNo => FirstInvoiceLineAfterSortedOnInvoiceLineNo;
		public USImportEntryInvoiceLine FirstInvoiceLineAfterSortedOnInvoiceLineNo
		{
			get
			{
				if (firstInvoiceLineAfterSortedOnInvoiceLineNoCached == null)
				{
					firstInvoiceLineAfterSortedOnInvoiceLineNoCached = new CachedValue<USImportEntryInvoiceLine>(() =>
					{
						return (USImportEntryInvoiceLine)Helper.GetFirstInvoiceLineAfterSortedOnInvoiceLineNo();
					});
				}
				return firstInvoiceLineAfterSortedOnInvoiceLineNoCached.Value;
			}
		}
		CachedValue<USImportEntryInvoiceLine> firstInvoiceLineAfterSortedOnInvoiceLineNoCached;

		public ZBool IsSetXLine => RandomLine.IsSetXLine;
		public ZBool IsSetVLine => RandomLine.IsSetVLine;
		public ZBool IsVParentLine => RandomLine.IsVParentLine;
		public ZBool IsVChildLine => RandomLine.IsVChildLine;
		public ZGuid US_CL_ParentLine => USE_CL_ParentLine;
		public bool IsACE => USE_IsACE;

		bool IEntryLine.IsCustomsChargeToBeCalculated => true;

		public bool IsConsumptionFTZ => USE_IsConsumptionFTZ;

		public ZDecimal TotalCustomsValueIncludingSecondaryLinesFromInvoiceLines
		{
			get
			{
				if (totalCustomsValueIncludingSecondaryLinesFromInvoiceLinesCached == null)
				{
					totalCustomsValueIncludingSecondaryLinesFromInvoiceLinesCached = new CachedProperty<ZDecimal>(Factory, Helper.GetTotalCustomsValueIncludingSecondaryLinesFromInvoiceLines);
				}
				return totalCustomsValueIncludingSecondaryLinesFromInvoiceLinesCached.Value;
			}
		}
		CachedProperty<ZDecimal> totalCustomsValueIncludingSecondaryLinesFromInvoiceLinesCached;

		public ZDecimal TotalCustomsValueIncludingSecondaryLines
		{
			get
			{
				if (!totalCustomsValueIncludingSecondaryLinesCached.HasValue)
				{
					totalCustomsValueIncludingSecondaryLinesCached = Helper.GetTotalCustomsValueIncludingSecondaryLines();
				}
				return totalCustomsValueIncludingSecondaryLinesCached.Value;
			}
		}
		ZDecimal? totalCustomsValueIncludingSecondaryLinesCached;

		public ZDecimal TotalTaxIncludingSecondaryLines
		{
			get
			{
				if (!totalTaxIncludingSecondaryLinesCached.HasValue)
				{
					totalTaxIncludingSecondaryLinesCached = Helper.GetTotalTaxIncludingSecondaryLines();
				}
				return totalTaxIncludingSecondaryLinesCached.Value;
			}
		}
		ZDecimal? totalTaxIncludingSecondaryLinesCached;

		public ZDecimal TotalFeeAmountIncludingSecondaryLines
		{
			get
			{
				if (!totalFeeAmountIncludingSecondaryLinesCached.HasValue)
				{
					totalFeeAmountIncludingSecondaryLinesCached = Helper.GetTotalFeeAmountIncludingSecondaryLines();
				}
				return totalFeeAmountIncludingSecondaryLinesCached.Value;
			}
		}
		ZDecimal? totalFeeAmountIncludingSecondaryLinesCached;

		public ZDecimal TotalMPFIncludingSecondaryLines
		{
			get
			{
				if (!totalMPFIncludingSecondaryLinesCached.HasValue)
				{
					totalMPFIncludingSecondaryLinesCached = Helper.GetTotalMPFIncludingSecondaryLines();
				}
				return totalMPFIncludingSecondaryLinesCached.Value;
			}
		}
		ZDecimal? totalMPFIncludingSecondaryLinesCached;

		public ZDecimal TotalHMFIncludingSecondaryLines
		{
			get
			{
				if (!totalHMFIncludingSecondaryLinesCached.HasValue)
				{
					totalHMFIncludingSecondaryLinesCached = Helper.GetTotalHMFIncludingSecondaryLines();
				}
				return totalHMFIncludingSecondaryLinesCached.Value;
			}
		}
		ZDecimal? totalHMFIncludingSecondaryLinesCached;

		public ZDecimal TotalDutyIncludingSecondaryLines
		{
			get
			{
				if (!totalDutyIncludingSecondaryLinesCached.HasValue)
				{
					totalDutyIncludingSecondaryLinesCached = Helper.GetTotalDutyIncludingSecondaryLines();
				}
				return totalDutyIncludingSecondaryLinesCached.Value;
			}
		}
		ZDecimal? totalDutyIncludingSecondaryLinesCached;

		public ZDecimal TotalPayableMPFIncludingSecondaryLines
		{
			get
			{
				if (!totalPayableMPFIncludingSecondaryLinesCached.HasValue)
				{
					totalPayableMPFIncludingSecondaryLinesCached = Helper.GetTotalPayableMPFIncludingSecondaryLines();
				}
				return totalPayableMPFIncludingSecondaryLinesCached.Value;
			}
		}
		ZDecimal? totalPayableMPFIncludingSecondaryLinesCached;

		#region IDutyData Members
		public ZString Tariff => CL_AdValoremTariff;
		public ZDate DateForDutyCalculation => RandomLine.EffectiveDateForDutyRate;
		public ZString SpecialProgramsIndicatorPrimary
		{
			get
			{
				if (!specialProgramsIndicatorPrimaryCached.HasValue)
				{
					var spi = RandomLine.USL_SPI;
					specialProgramsIndicatorPrimaryCached = !spi.IsEmpty && Factory.GetCachedValue<PrimarySpecProgramIndicatorList>().ContainsCode(spi) ? this.GetEffectiveSPIForDutyCalculation(spi) : ZString.Empty;
				}
				return specialProgramsIndicatorPrimaryCached.Value;
			}
		}
		ZString? specialProgramsIndicatorPrimaryCached;

		public ZString SpecialProgramsIndicatorCountry
		{
			get
			{
				if (!specialProgramsIndicatorCountryCached.HasValue)
				{
					var spi = RandomLine.USL_SPI;
					specialProgramsIndicatorCountryCached = !spi.IsEmpty && Factory.GetCachedValue<SpecialProgramList>().ContainsCode(spi) ? this.GetEffectiveSPIForDutyCalculation(spi) : ZString.Empty;
				}
				return specialProgramsIndicatorCountryCached.Value;
			}
		}
		ZString? specialProgramsIndicatorCountryCached;

		public ZString CountryOfOrigin => RandomLine.USL_UC_NKCountryOfOrigin;

		public ZString SpecialProgramsIndicatorSecondary
		{
			get
			{
				if (!specialProgramsIndicatorSecondaryCached.HasValue)
				{
					var invoiceLine = this.RandomLine;
					var result = invoiceLine.USL_SecondarySPI;

					if (IsSecondaryTariffLine && (invoiceLine.IsSetXLine || invoiceLine.IsSetVLine))
					{
						result = ZString.Empty;
					}
					specialProgramsIndicatorSecondaryCached = result;
				}
				return specialProgramsIndicatorSecondaryCached.Value;
			}
		}
		ZString? specialProgramsIndicatorSecondaryCached;
		USCTariff IDutyData.ImportTariff
		{
			get
			{
				var dateForDutyCalculation = DateForDutyCalculation;
				return Factory.GetCachedValue(USE_AdValoremTariff.PadRight(USCTariff.Schema.UE_TariffMaxLength) + dateForDutyCalculation.ToString(),
					delegate
					{
						return new USCTariff.Loader(Factory).LoadBestMatch(USE_AdValoremTariff, dateForDutyCalculation);
					});
			}
		}

		ZDecimal IDutyData.Quantity1 => throw new NotSupportedException();

		ZString IDutyData.UQ1 => throw new NotSupportedException();

		ZDecimal IDutyData.Quantity2 => throw new NotSupportedException();

		ZString IDutyData.UQ2 => throw new NotSupportedException();

		ZDecimal IDutyData.Quantity3 => throw new NotSupportedException();

		ZString IDutyData.UQ3 => throw new NotSupportedException();

		ZDecimal IDutyData.CustomsValue => throw new NotSupportedException();

		ZDecimal IDutyData.SupCustomsValue => throw new NotSupportedException();

		ZString IDutyData.SelectedRateType => throw new NotSupportedException();

		ZDecimal IDutyData.ValueForADD => throw new NotSupportedException();

		ZDecimal IDutyData.ADDDepositRate => throw new NotSupportedException();

		ZString IDutyData.ADDCaseRateTypeQualifier => throw new NotSupportedException();

		ZDecimal IDutyData.ADDQuantity => throw new NotSupportedException();

		ZDecimal? IDutyData.ADDutyManual => throw new NotSupportedException();

		ZDecimal IDutyData.ValueForCVD => throw new NotSupportedException();

		ZDecimal IDutyData.CVDDepositRate => throw new NotSupportedException();

		ZString IDutyData.CVDCaseRateTypeQualifier => throw new NotSupportedException();

		ZDecimal IDutyData.CVDQuantity => throw new NotSupportedException();

		ZDecimal? IDutyData.CVDutyManual => throw new NotSupportedException();

		IDutyData IDutyData.ParentTariffLine => throw new NotSupportedException();

		bool IDutyData.IsCottonFeeExemptIndicated => throw new NotSupportedException();

		bool IDutyData.HasCottonCertificate => throw new NotSupportedException();

		ZBool IDutyData.IsSetXLine => throw new NotSupportedException();

		ZBool IDutyData.IsSetVLine => throw new NotSupportedException();

		bool IDutyData.IsAMSFeeExempt => throw new NotSupportedException();

		bool IDutyData.IsRaspberryFeeExempt => throw new NotSupportedException();

		ZString IDutyData.EntryType => throw new NotSupportedException();

		bool IDutyData.IsClearedInPR => throw new NotSupportedException();

		bool IDutyData.IsSecondaryTariffLine => throw new NotSupportedException();

		bool IDutyData.IsDomesticMerchandise => throw new NotSupportedException();

		bool IDutyData.HasTextileCategoryNo => throw new NotSupportedException();

		bool IDutyData.IsCombineSecondaryTariffLine => throw new NotSupportedException();

		IEnumerable<IDutyData> IDutyData.CombineChildLines => throw new NotSupportedException();

		IReadOnlyList<ZString> IDutyData.SupTariffs => throw new NotSupportedException();

		IDutyData IDutyData.CombineParentLine => throw new NotSupportedException();

		IEnumerable<IDutyData> IDutyData.CombineAllLines => throw new NotSupportedException();

		#endregion

		public override void Delete()
		{
			throw new NotSupportedException("Cannot delete a view data");
		}
	}
}
