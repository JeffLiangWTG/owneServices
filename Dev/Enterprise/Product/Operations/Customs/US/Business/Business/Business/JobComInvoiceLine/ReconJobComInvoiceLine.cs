using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	partial class JobComInvoiceLine
	{
		#region Original Fields for recon

		internal ZDecimal OrigDairyQty
		{
			get
			{
				return
						US_R_OrigFirstUQ == ABIUnitOfMeasureList.Codes.ContentKilogram ? US_R_OrigFirstQty :
						US_R_OrigSecondUQ == ABIUnitOfMeasureList.Codes.ContentKilogram ? US_R_OrigSecondQty :
						US_R_OrigThirdUQ == ABIUnitOfMeasureList.Codes.ContentKilogram ? US_R_OrigThirdQty : ZDecimal.Zero;
			}
		}

		bool IsCKGReportingUnitForOrigTariff
		{
			get
			{
				return US_R_OrigFirstUQ == ABIUnitOfMeasureList.Codes.ContentKilogram ||
					US_R_OrigSecondUQ == ABIUnitOfMeasureList.Codes.ContentKilogram ||
					US_R_OrigThirdUQ == ABIUnitOfMeasureList.Codes.ContentKilogram;
			}
		}

		public ZDecimal US_R_OrigHMFAmount
		{
			get { return ReconOriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF); }
			set
			{
				ReconOriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.HMF, value);
				US_R_OrigHMFAmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_R_OrigHMFAmountInfo
		{
			get { return GetZPropertyInfo(Schema.US_R_OrigHMFAmount); }
		}

		internal bool US_R_OrigHMFAmount_ReadOnly
		{
			get { return ShouldChargeBeReadonly(Core.Constants.USCustoms.FeeCodes.HMF); }
		}

		public ZDecimal US_R_OrigMPFAmount
		{
			get { return ReconOriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing); }
			set { ReconOriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, value); }
		}

		public ZPropertyInfo US_R_OrigMPFAmountInfo
		{
			get { return GetZPropertyInfo(Schema.US_R_OrigMPFAmount); }
		}

		internal bool US_R_OrigMPFAmount_ReadOnly
		{
			get { return ShouldChargeBeReadonly(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.ReconFeeAndChargeList))]
		[MaxLength(3)]
		public ZString US_R_OrigOtherFeeCode
		{
			get
			{
				if (firstOriginOtherFee == null || firstOriginOtherFee.IsDeleted)
				{
					firstOriginOtherFee = ReconOriginalCharges.GetFirstFeeOtherThanTaxHMFOrMPF();
				}
				return firstOriginOtherFee != null ? firstOriginOtherFee.CY_Code : ZString.Empty;
			}
			set
			{
				CheckMaximumLength(US_R_OrigOtherFeeCodeInfo, value);

				if (firstOriginOtherFee == null || firstOriginOtherFee.IsDeleted)
				{
					firstOriginOtherFee = ReconOriginalCharges.GetFirstFeeOtherThanTaxHMFOrMPF();
				}

				if (firstOriginOtherFee == null)
				{
					if (value != ZString.Empty)
					{
						firstOriginOtherFee = ReconOriginalCharges.AddNew(value);
					}
				}
				else
				{
					firstOriginOtherFee.CY_Code = value;
				}

				if (US_R_OrigOtherFeeCode.IsEmpty)
				{
					US_R_OrigOtherFeeAmount = ZDecimal.Zero;
				}

				US_R_OrigOtherFeeCodeInfo.RefreshBinding();
			}
		}
		ReconEntryOriginalCharge firstOriginOtherFee;

		public ZPropertyInfo US_R_OrigOtherFeeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.US_R_OrigOtherFeeCode); }
		}

		internal bool US_R_OrigOtherFeeCode_ReadOnly
		{
			get { return ShouldChargeBeReadonly(US_R_OrigOtherFeeCode); }
		}

		bool ShouldChargeBeReadonly(ZString chargeType)
		{
			var charge = ReconOriginalCharges.GetCharge(chargeType);
			var chargeReadonly = charge == null || charge.CY_Amount_ReadOnly;
			var chargeParent = (IReconOriginalChargeParent)this;
			return chargeParent.ShouldCalculateOrigDuty && chargeReadonly;
		}

		public ZDecimal US_R_OrigOtherFeeAmount
		{
			get { return ReconOriginalCharges.GetAmount(US_R_OrigOtherFeeCode); }
			set
			{
				ReconOriginalCharges.SetAmount(US_R_OrigOtherFeeCode, value);
				US_R_OrigOtherFeeAmountInfo.RefreshBinding();
			}
		}

		bool US_R_OrigOtherFeeAmount_ReadOnly
		{
			get { return US_R_OrigOtherFeeCode_ReadOnly; }
		}

		public ZPropertyInfo US_R_OrigOtherFeeAmountInfo
		{
			get { return GetZPropertyInfo(Schema.US_R_OrigOtherFeeAmount); }
		}

		public override ZBool US_R_OrigOverrideSupDuty
		{
			get { return base.US_R_OrigOverrideSupDuty; }
			set
			{
				ZBool oldValue = US_R_OrigOverrideSupDuty;
				base.US_R_OrigOverrideSupDuty = value;
				if (!IsCopying && oldValue != US_R_OrigOverrideSupDuty && !US_OverrideSupDuty && US_OverrideSupDuty != US_R_OrigOverrideSupDuty)
				{
					US_OverrideSupDuty = US_R_OrigOverrideSupDuty;
				}
			}
		}

		public override ZDecimal US_R_OrigSupDuty
		{
			get { return base.US_R_OrigSupDuty; }
			set
			{
				ZDecimal oldValue = US_R_OrigSupDuty;
				base.US_R_OrigSupDuty = value;
				if (!IsCopying && oldValue != US_R_OrigSupDuty && US_OverrideSupDuty && US_SupDuty.IsEmpty && US_SupDuty != US_R_OrigSupDuty)
				{
					US_SupDuty = US_R_OrigSupDuty;
				}
			}
		}
		public override ZDecimal US_R_Orig98Value
		{
			get { return base.US_R_Orig98Value; }
			set
			{
				ZDecimal oldValue = US_R_Orig98Value;
				base.US_R_Orig98Value = value;
				if (!IsCopying && oldValue != US_R_Orig98Value && US_98GoodsValue.IsEmpty && US_98GoodsValue != US_R_Orig98Value)
				{
					US_98GoodsValue = US_R_Orig98Value;
				}
			}
		}

		public override ZBool US_R_OrigOverrideDuty
		{
			get { return base.US_R_OrigOverrideDuty; }
			set
			{
				ZBool oldValue = US_R_OrigOverrideDuty;
				base.US_R_OrigOverrideDuty = value;
				if (!IsCopying && oldValue != US_R_OrigOverrideDuty && !US_OverrideDuty && US_OverrideDuty != US_R_OrigOverrideDuty)
				{
					US_OverrideDuty = US_R_OrigOverrideDuty;
				}
			}
		}

		public override ZDecimal US_R_OrigDuty
		{
			get { return base.US_R_OrigDuty; }
			set
			{
				ZDecimal oldValue = US_R_OrigDuty;
				base.US_R_OrigDuty = value;
				if (!IsCopying && oldValue != US_R_OrigDuty)
				{
					InvoiceHeader?.ReconOriginalEntry?.RefreshEntryWithTotalOriginalCustomsFees();
					if (US_OverrideDuty && US_Duty.IsEmpty && US_Duty != US_R_OrigDuty)
					{
						US_Duty = US_R_OrigDuty;
					}
				}
			}
		}

		public override ZString US_R_OrigCottonFeeExempt
		{
			get { return base.US_R_OrigCottonFeeExempt; }
			set
			{
				ZString oldValue = US_R_OrigCottonFeeExempt;
				base.US_R_OrigCottonFeeExempt = value;
				if (!IsCopying && oldValue != US_R_OrigCottonFeeExempt && US_CottonFeeExempt.IsEmpty && US_CottonFeeExempt != US_R_OrigCottonFeeExempt)
				{
					US_CottonFeeExempt = US_R_OrigCottonFeeExempt;
				}
			}
		}

		public override ZDecimal US_R_OrigCV
		{
			get { return base.US_R_OrigCV; }
			set
			{
				ZDecimal oldValue = US_R_OrigCV;
				base.US_R_OrigCV = value;
				if (!IsCopying && oldValue != US_R_OrigCV && JI_LinePrice.IsEmpty && JI_LinePrice != US_R_OrigCV)
				{
					JI_LinePrice = US_R_OrigCV;
				}
			}
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.Tariffs))]
		public override ZString US_R_OrigTariff
		{
			get { return base.US_R_OrigTariff; }
			set
			{
				ZString oldValue = US_R_OrigTariff;
				base.US_R_OrigTariff = TariffFormatter.Format(value);

				ITariff checkTariff = OriginalImportTariff;
				if (checkTariff != null)
				{
					US_R_OrigFirstUQ = checkTariff.Unit1;
					US_R_OrigSecondUQ = checkTariff.Unit2;
					US_R_OrigThirdUQ = checkTariff.Unit3;
				}
				else
				{
					US_R_OrigFirstUQ = ZString.Empty;
					US_R_OrigSecondUQ = ZString.Empty;
					US_R_OrigThirdUQ = ZString.Empty;
				}

				if (oldValue != US_R_OrigTariff && JI_Tariff.IsEmpty && JI_Tariff != US_R_OrigTariff)
				{
					JI_Tariff = US_R_OrigTariff;
				}

				DefaultTaxRelatedFields(OriginalImportTariff, JobComInvoiceLine.Schema.US_R_OrigTaxApply, JobComInvoiceLine.Schema.US_R_OrigTaxCode, JobComInvoiceLine.Schema.US_R_OrigTaxRateT, JobComInvoiceLine.Schema.US_R_OrigTaxRateS, JobComInvoiceLine.Schema.US_R_OrigTaxRate, JobComInvoiceLine.Schema.US_R_OrigTaxQty);
				ReconOrigTariffMarkedForReferenceFileRequest = TariffValidator.CorrectTariffLength(US_R_OrigTariff);
			}
		}

		public override ZString US_R_OrigTaxApply
		{
			get { return base.US_R_OrigTaxApply; }
			set
			{
				ZString oldValue = US_R_OrigTaxApply;

				base.US_R_OrigTaxApply = value;

				if (!IsCopying && oldValue != US_R_OrigTaxApply)
				{
					US_R_OrigTaxCode = ZString.Empty;
					if (OrigTaxRateT_ReadOnly)
					{
						US_R_OrigTaxRateT = ZString.Empty;
					}
					US_R_OrigTaxRateS = ZString.Empty;
					US_R_OrigTaxRate = ZDecimal.Zero;
					US_R_OrigTaxQty = ZDecimal.Zero;
					if (US_TaxApply.IsEmpty && US_TaxApply != US_R_OrigTaxApply)
					{
						US_TaxApply = US_R_OrigTaxApply;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(OrigTaxCode_ReadOnly))]
		public override ZString US_R_OrigTaxCode
		{
			get
			{
				ZString result = base.US_R_OrigTaxCode;

				if (result.IsEmpty && TaxApplyList.IsTaxApplicable(US_R_OrigTaxApply))
				{
					result = GetDefaultTaxCode(OriginalImportTariff);
				}

				return result;
			}
			set
			{
				ZString oldTaxCode = US_R_OrigTaxCode;

				ZString valueToAssign = GetDefaultTaxCode(OriginalImportTariff) == value ? ZString.Empty : value;

				base.US_R_OrigTaxCode = valueToAssign;

				if (!IsCopying && oldTaxCode != US_R_OrigTaxCode)
				{
					US_R_OrigTaxRateS = ZString.Empty;
					US_R_OrigTaxRateT = ZString.Empty;
					US_R_OrigTaxRate = ZDecimal.Zero;
					US_R_OrigTaxQty = ZDecimal.Zero;

					if (US_TaxCode.IsEmpty && US_TaxCode != US_R_OrigTaxCode)
					{
						US_TaxCode = US_R_OrigTaxCode;
					}

					DeleteElementInFeeCusCodes(oldTaxCode, ReconOriginalCharges);

					var charge = ReconOriginalCharges.GetCharge(US_R_OrigTaxCode);
					if (charge == null)
					{
						ReconOriginalCharges.AddNew(US_R_OrigTaxCode);
					}
					if (US_R_OrigTaxCode.IsEmpty)
					{
						US_R_OrigTaxAmount = ZDecimal.Zero;
					}
				}
			}
		}

		bool OrigTaxCode_ReadOnly
		{
			get { return !IsOrigTaxRateApplicable; }
		}

		[ReadOnlyMember(nameof(US_R_OrigTaxAmount_ReadOnly))]
		public ZDecimal US_R_OrigTaxAmount
		{
			get { return ReconOriginalCharges.GetAmount(US_R_OrigTaxCode); }
			set
			{
				ReconOriginalCharges.SetAmount(US_R_OrigTaxCode, value);
				US_R_OrigTaxAmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_R_OrigTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.US_R_OrigTaxAmount); }
		}

		bool US_R_OrigTaxAmount_ReadOnly
		{
			get { return US_R_OrigTaxApply.IsEmpty || US_R_OrigTaxApply == TaxApplyList.Codes.No || US_R_OrigTaxCode.IsEmpty; }
		}

		[ReadOnlyMember(nameof(OrigTaxRateT_ReadOnly))]
		public override ZString US_R_OrigTaxRateT
		{
			get { return base.US_R_OrigTaxRateT; }
			set
			{
				ZString oldValue = US_R_OrigTaxRateT;
				base.US_R_OrigTaxRateT = value;
				if (!IsCopying && oldValue != US_R_OrigTaxRateT)
				{
					UpdateTaxRateTypeInFeeCusCodes(US_R_OrigTaxCode, US_R_OrigTaxRateT, ReconOriginalCharges);
					if (US_TaxRateT.IsEmpty && US_TaxRateT != US_R_OrigTaxRateT && !TaxRateT_ReadOnly)
					{
						US_TaxRateT = US_R_OrigTaxRateT;
					}
				}
			}
		}

		public bool OrigTaxRateT_ReadOnly
		{
			get { return IsTaxRateTypeReadOnly(OriginalImportTariff, US_R_OrigTaxCode, US_R_OrigTaxApply); }
		}

		[ReadOnlyMember(nameof(OrigTaxRateS_ReadOnly))]
		public override ZString US_R_OrigTaxRateS
		{
			get
			{
				ZString result = base.US_R_OrigTaxRateS;

				if (result.IsEmpty && TaxApplyList.IsTaxApplicable(US_R_OrigTaxApply))
				{
					result = AppendixBTaxRateList.GetNormalTaxRateString(OriginalImportTariff, US_R_OrigTaxCode, US_R_OrigTaxRateT);
				}

				return result;
			}
			set
			{
				bool hasChanges = US_R_OrigTaxRateS != value;

				ZString valueToAssign = AppendixBTaxRateList.GetNormalTaxRateString(OriginalImportTariff, US_R_OrigTaxCode, US_R_OrigTaxRateT) == value ? ZString.Empty : value;

				base.US_R_OrigTaxRateS = valueToAssign;

				if (hasChanges && !IsCopying)
				{
					if (IsOrigTaxRateSpecifiedManually)
					{
						US_R_OrigTaxRate = 0m;
					}
					else
					{
						US_R_OrigTaxRate = AppendixBTaxRateList.GetRate(US_R_OrigTaxRateS);
					}

					if (US_TaxRateS.IsEmpty && US_TaxRateS != US_R_OrigTaxRateS)
					{
						US_TaxRateS = US_R_OrigTaxRateS;
					}

					if (!IsOrigTaxRateQuantityRequired)
					{
						US_R_OrigTaxQty = ZDecimal.Zero;
					}
				}
			}
		}

		public bool OrigTaxRateS_ReadOnly => !IsOrigTaxRateApplicable;

		public ZBool IsOrigTaxRateSpecifiedManually
		{
			get { return US_R_OrigTaxRateS.EqualsIgnoringCase(AppendixBTaxRateList.Codes.Specify) || IsOriginalTaxRateReduced; }
		}

		public ZBool IsOrigTaxRateQuantityRequired => AppendixBTaxRateList.DoesUQMatchNoneOfCustomsUQs(US_R_OrigTaxRateS, US_R_OrigFirstUQ, US_R_OrigSecondUQ);
		internal bool IsOrigTaxRateApplicable => TaxApplyList.IsTaxApplicable(US_R_OrigTaxApply);
		internal bool IsOriginalTaxRateOverridden => US_R_OrigTaxApply == TaxApplyList.Codes.Override;

		public ZBool IsOriginalTaxRateReduced
		{
			get { return US_R_OrigTaxRateS.EqualsIgnoringCase(AppendixBTaxRateList.CBMAEligible); }
		}

		[DecimalPlaces(4)]
		[ReadOnlyMember(nameof(US_R_OrigTaxQty_ReadOnly))]
		public override ZDecimal US_R_OrigTaxQty
		{
			get
			{
				return base.US_R_OrigTaxQty;
			}
			set
			{
				bool hasChanges = US_R_OrigTaxQty != value;

				base.US_R_OrigTaxQty = value;

				if (hasChanges && !IsCopying)
				{
					if (US_TaxQty.IsEmpty && US_TaxQty != US_R_OrigTaxQty && IsTaxQtyRequired)
					{
						US_TaxQty = US_R_OrigTaxQty;
					}
				}
			}
		}

		internal bool US_R_OrigTaxQty_ReadOnly => !IsOrigTaxRateQuantityRequired;

		[DecimalPlaces(8)]
		[ReadOnlyMember(nameof(US_R_OrigTaxRate_ReadOnly))]
		public override ZDecimal US_R_OrigTaxRate
		{
			get
			{
				ZDecimal result = base.US_R_OrigTaxRate;

				if (result.IsEmpty && !US_R_OrigTaxRateS.IsEmpty)
				{
					result = AppendixBTaxRateList.GetRate(US_R_OrigTaxRateS);
				}

				return result;
			}
			set
			{
				ZDecimal oldValue = US_R_OrigTaxRate;
				base.US_R_OrigTaxRate = value;
				if (!IsCopying && oldValue != US_R_OrigTaxRate && US_TaxRate.IsEmpty && US_TaxRate != US_R_OrigTaxRate)
				{
					US_TaxRate = US_R_OrigTaxRate;
				}
			}
		}

		internal bool US_R_OrigTaxRate_ReadOnly => !IsOrigTaxRateSpecifiedManually;

		public bool ReconOrigTariffIsCandidateForUpdateRequest
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsRecon && !US_R_OrigTariff.IsEmpty && EffectiveDateForDutyRate.IsValid && OriginalImportTariff == null;
			}
		}

		public bool ReconOrigTariffMarkedForReferenceFileRequest
		{
			get { return reconOrigTariffMarkedForReferenceFileRequest; }
			set
			{
				if (reconOrigTariffMarkedForReferenceFileRequest != value)
				{
					reconOrigTariffMarkedForReferenceFileRequest = value;
					if (reconOrigTariffMarkedForReferenceFileRequest)
					{
						var declartion = Declaration;
						if (declartion != null)
						{
							declartion.SetNeedToRequestTariffs();
						}
					}
				}
			}
		}
		bool reconOrigTariffMarkedForReferenceFileRequest;

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.Tariffs))]
		public ZString OriginalTariffFormatted
		{
			get { return TariffFormatter.DisplayFormat(US_R_OrigTariff); }
			set
			{
				ZString formattedValue = value.ExcludeChars(" .");
				ZString newValue = formattedValue.Length > 10 ? formattedValue.SubstringSafe(0, 10) : formattedValue;
				US_R_OrigTariff = newValue;
			}
		}

		public ZPropertyInfo OriginalTariffFormattedInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.OriginalTariffFormatted, x => US_R_OrigTariffInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.ReconOrigSPIList))]
		public override ZString US_R_OrigSPI
		{
			get { return base.US_R_OrigSPI; }
			set
			{
				ZString oldValue = US_R_OrigSPI;
				base.US_R_OrigSPI = value;
				if (!IsCopying && oldValue != US_R_OrigSPI && US_SPI.IsEmpty && US_SPI != US_R_OrigSPI)
				{
					US_SPI = US_R_OrigSPI;
				}
			}
		}
		[DecimalPlaces(5)]
		public override ZDecimal US_R_OrigFirstQty
		{
			get { return base.US_R_OrigFirstQty; }
			set
			{
				bool hasChanges = base.US_R_OrigFirstQty != value;
				base.US_R_OrigFirstQty = value;

				if (hasChanges && !IsCopying)
				{
					ClearCustomsQuantityIfSameAsEffective(Schema.US_R_OrigSupQty1, US_R_OrigFirstQty, US_R_OrigSupUQ1, US_R_OrigFirstUQ, OriginalImportSupTariff);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		public override ZString US_R_OrigFirstUQ
		{
			get { return base.US_R_OrigFirstUQ; }
			set
			{
				bool hasChanges = base.US_R_OrigFirstUQ != value;

				base.US_R_OrigFirstUQ = value;

				if (hasChanges && !IsCopying)
				{
					US_R_OrigFirstQty = ZDecimal.Zero;
				}
			}
		}
		[DecimalPlaces(5)]
		public override ZDecimal US_R_OrigSecondQty
		{
			get { return base.US_R_OrigSecondQty; }
			set
			{
				bool hasChanges = base.US_R_OrigSecondQty != value;
				base.US_R_OrigSecondQty = value;

				if (hasChanges && !IsCopying)
				{
					ClearCustomsQuantityIfSameAsEffective(Schema.US_R_OrigSupQty2, US_R_OrigSecondQty, US_R_OrigSupUQ2, US_R_OrigSecondUQ, OriginalImportSupTariff);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		public override ZString US_R_OrigSecondUQ
		{
			get { return base.US_R_OrigSecondUQ; }
			set
			{
				bool hasChanges = base.US_R_OrigSecondUQ != value;

				base.US_R_OrigSecondUQ = value;

				if (hasChanges && !IsCopying)
				{
					US_R_OrigSecondQty = ZDecimal.Zero;
				}
			}
		}
		[DecimalPlaces(5)]
		public override ZDecimal US_R_OrigThirdQty
		{
			get { return base.US_R_OrigThirdQty; }
			set
			{
				bool hasChanges = base.US_R_OrigThirdQty != value;
				base.US_R_OrigThirdQty = value;

				if (hasChanges && !IsCopying)
				{
					ClearCustomsQuantityIfSameAsEffective(Schema.US_R_OrigSupQty3, US_R_OrigThirdQty, US_R_OrigSupUQ3, US_R_OrigThirdUQ, OriginalImportSupTariff);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		public override ZString US_R_OrigThirdUQ
		{
			get { return base.US_R_OrigThirdUQ; }
			set
			{
				bool hasChanges = base.US_R_OrigThirdUQ != value;

				base.US_R_OrigThirdUQ = value;

				if (hasChanges && !IsCopying)
				{
					US_R_OrigThirdQty = ZDecimal.Zero;
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.ReconOriginalEntries))]
		public ZGuid US_CH_ReconEntry
		{
			get { return InvoiceHeader != null ? InvoiceHeader.US_CH_ReconEntry : ZGuid.Empty; }
		}

		public ZPropertyInfo US_CH_ReconEntryInfo
		{
			get { return GetZPropertyInfo(Schema.US_CH_ReconEntry); }
		}

		[ReadOnlyMember(nameof(US_R_OrigRateType_ReadOnly))]
		public override ZString US_R_OrigRateType
		{
			get { return base.US_R_OrigRateType; }
			set { base.US_R_OrigRateType = value; }
		}

		public bool US_R_OrigRateType_ReadOnly
		{
			get { return !(OriginalImportTariff != null && OriginalImportTariff.IsSpecificSpecificDutyRate); }
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.Tariffs))]
		public override ZString US_R_OrigSupTariff
		{
			get { return base.US_R_OrigSupTariff; }
			set
			{
				ZString valueToSet = TariffFormatter.Format(value);
				ZString oldValue = US_R_OrigSupTariff;
				base.US_R_OrigSupTariff = valueToSet;

				if (!IsCopying && oldValue != US_R_OrigSupTariff)
				{
					ITariff checkTariff = OriginalImportSupTariff;

					US_R_OrigSupUQ1 = checkTariff != null ? checkTariff.Unit1 : ZString.Empty;
					US_R_OrigSupUQ2 = checkTariff != null ? checkTariff.Unit2 : ZString.Empty;
					US_R_OrigSupUQ3 = checkTariff != null ? checkTariff.Unit3 : ZString.Empty;

					ReconOrigSupTariffMarkedForReferenceFileRequest = TariffValidator.CorrectTariffLength(US_R_OrigSupTariff);

					if (HasEmptySupTariff && US_SupTariff != US_R_OrigSupTariff)
					{
						US_SupTariff = US_R_OrigSupTariff;
					}

					SetSupTariffInChildLine(US_R_OrigSupTariff, Schema.US_R_OrigSupTariff);
				}
			}
		}

		public bool ReconOrigSupTariffIsCandidateForUpdateRequest
		{
			get { return Declaration != null && Declaration.IsRecon && !US_R_OrigSupTariff.IsEmpty && EffectiveDateForDutyRate.IsValid && OriginalImportSupTariff == null; }
		}

		public bool ReconOrigSupTariffMarkedForReferenceFileRequest
		{
			get { return reconOrigSupTariffMarkedForReferenceFileRequest; }
			set
			{
				if (reconOrigSupTariffMarkedForReferenceFileRequest != value)
				{
					reconOrigSupTariffMarkedForReferenceFileRequest = value;
					if (reconOrigSupTariffMarkedForReferenceFileRequest)
					{
						var declartion = Declaration;
						if (declartion != null)
						{
							declartion.SetNeedToRequestTariffs();
						}
					}
				}
			}
		}
		bool reconOrigSupTariffMarkedForReferenceFileRequest;

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.Tariffs))]
		public ZString OriginalSupTariffFormatted
		{
			get { return TariffFormatter.DisplayFormat(US_R_OrigSupTariff); }
			set
			{
				ZString oldValue = OriginalSupTariffFormatted;
				ZString formattedValue = value.ExcludeChars(" .");
				ZString newValue = formattedValue.Length > 10 ? formattedValue.SubstringSafe(0, 10) : formattedValue;
				US_R_OrigSupTariff = newValue;
				OriginalSupTariffFormattedInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo OriginalSupTariffFormattedInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.OriginalSupTariffFormatted, x => US_R_OrigSupTariffInfo); }
		}

		[ReadOnlyMember(nameof(US_R_OrigSupQty1_ReadOnly))]
		public override ZDecimal US_R_OrigSupQty1
		{
			get { return GetEffectiveCustomsQuantity(base.US_R_OrigSupQty1, US_R_OrigSupUQ1, US_R_OrigFirstUQ, US_R_OrigFirstQty, OriginalImportSupTariff); }
			set
			{
				ZDecimal valueToSet = value;

				ZDecimal effectiveValue = GetEffectiveCustomsQuantity(ZDecimal.Zero, US_R_OrigSupUQ1, US_R_OrigFirstUQ, US_R_OrigFirstQty, OriginalImportSupTariff);
				if (valueToSet == effectiveValue)
				{
					valueToSet = ZDecimal.Zero;
				}

				ZDecimal oldValue = US_R_OrigSupQty1;
				base.US_R_OrigSupQty1 = valueToSet;
				if (!IsCopying && oldValue != US_R_OrigSupQty1 && US_SupQty1.IsEmpty && US_SupQty1 != US_R_OrigSupQty1)
				{
					US_SupQty1 = US_R_OrigSupQty1;
				}
			}
		}

		ZDecimal GetEffectiveCustomsQuantity(ZDecimal baseValue, ZString uq, ZString effectiveUQ, ZDecimal effectiveQty, USCTariff tariffToCheck)
		{
			ZDecimal result = baseValue;

			if (result.IsEmpty && uq == effectiveUQ && tariffToCheck != null && !tariffToCheck.Applies(TariffRuleList.Codes.AdditionalTariffs, EffectiveDateForDutyRate))
			{
				result = effectiveQty;
			}

			return result;
		}

		void ClearCustomsQuantityIfSameAsEffective(string qtyFieldName, ZDecimal effectiveQty, ZString uq, ZString effectiveUQ, USCTariff tariffToCheck)
		{
			if (tariffToCheck != null && !tariffToCheck.Applies(TariffRuleList.Codes.AdditionalTariffs, EffectiveDateForDutyRate))
			{
				var value = (ZDecimal)this[qtyFieldName];

				if (value.Equals(effectiveQty) && uq.EqualsIgnoringCase(effectiveUQ))
				{
					this[qtyFieldName] = ZDecimal.Zero;
				}
			}
		}

		bool US_R_OrigSupQty1_ReadOnly
		{
			get { return !IsQuantityRequired(US_R_OrigSupUQ1); }
		}

		[ReadOnlyMember(nameof(US_R_OrigSupUQ1_ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		public override ZString US_R_OrigSupUQ1
		{
			get { return base.US_R_OrigSupUQ1; }
			set
			{
				ZString oldValue = US_R_OrigSupUQ1;
				base.US_R_OrigSupUQ1 = value;

				if (!IsCopying && oldValue != US_R_OrigSupUQ1)
				{
					US_R_OrigSupQty1 = ZDecimal.Zero;
					if (US_SupUQ1.IsEmpty && US_SupUQ1 != US_R_OrigSupUQ1)
					{
						US_SupUQ1 = US_R_OrigSupUQ1;
					}
				}
			}
		}

		bool US_R_OrigSupUQ1_ReadOnly
		{
			get { return true; }
		}

		[ReadOnlyMember(nameof(US_R_OrigSupQty2_ReadOnly))]
		public override ZDecimal US_R_OrigSupQty2
		{
			get { return GetEffectiveCustomsQuantity(base.US_R_OrigSupQty2, US_R_OrigSupUQ2, US_R_OrigSecondUQ, US_R_OrigSecondQty, OriginalImportSupTariff); }
			set
			{
				ZDecimal oldValue = US_R_OrigSupQty2;
				ZDecimal valueToSet = value;

				ZDecimal effectiveValue = GetEffectiveCustomsQuantity(ZDecimal.Zero, US_R_OrigSupUQ2, US_R_OrigSecondUQ, US_R_OrigSecondQty, OriginalImportSupTariff);
				if (valueToSet == effectiveValue)
				{
					valueToSet = ZDecimal.Zero;
				}

				base.US_R_OrigSupQty2 = valueToSet;
				if (!IsCopying && oldValue != US_R_OrigSupQty2 && US_SupQty2.IsEmpty && US_SupQty2 != US_R_OrigSupQty2)
				{
					US_SupQty2 = US_R_OrigSupQty2;
				}
			}
		}

		bool US_R_OrigSupQty2_ReadOnly
		{
			get { return !IsQuantityRequired(US_R_OrigSupUQ2); }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.InvoiceUQList))]
		[ReadOnlyMember(nameof(US_R_OrigSupUQ2_ReadOnly))]
		public override ZString US_R_OrigSupUQ2
		{
			get { return base.US_R_OrigSupUQ2; }
			set
			{
				ZString oldValue = US_R_OrigSupUQ2;
				base.US_R_OrigSupUQ2 = value;

				if (!IsCopying && oldValue != US_R_OrigSupUQ2)
				{
					US_R_OrigSupQty2 = ZDecimal.Zero;
					if (US_SupUQ2.IsEmpty && US_SupUQ2 != US_R_OrigSupUQ2)
					{
						US_SupUQ2 = US_R_OrigSupUQ2;
					}
				}
			}
		}

		bool US_R_OrigSupUQ2_ReadOnly
		{
			get { return true; }
		}

		[ReadOnlyMember(nameof(US_R_OrigSupQty3_ReadOnly))]
		public override ZDecimal US_R_OrigSupQty3
		{
			get { return GetEffectiveCustomsQuantity(base.US_R_OrigSupQty3, US_R_OrigSupUQ3, US_R_OrigThirdUQ, US_R_OrigThirdQty, OriginalImportSupTariff); }
			set
			{
				ZDecimal valueToSet = value;

				ZDecimal effectiveValue = GetEffectiveCustomsQuantity(ZDecimal.Zero, US_R_OrigSupUQ3, US_R_OrigThirdUQ, US_R_OrigThirdQty, OriginalImportSupTariff);
				if (valueToSet == effectiveValue)
				{
					valueToSet = ZDecimal.Zero;
				}

				ZDecimal oldValue = US_R_OrigSupQty3;
				base.US_R_OrigSupQty3 = valueToSet;
				if (!IsCopying && oldValue != US_R_OrigSupQty3 && US_SupQty3.IsEmpty && US_SupQty3 != US_R_OrigSupQty3)
				{
					US_SupQty3 = US_R_OrigSupQty3;
				}
			}
		}

		bool US_R_OrigSupQty3_ReadOnly
		{
			get { return !IsQuantityRequired(US_R_OrigSupUQ3); }
		}

		[ReadOnlyMember(nameof(US_R_OrigSupUQ3_ReadOnly))]
		public override ZString US_R_OrigSupUQ3
		{
			get { return base.US_R_OrigSupUQ3; }
			set
			{
				ZString oldValue = US_R_OrigSupUQ3;
				base.US_R_OrigSupUQ3 = value;

				if (!IsCopying && oldValue != US_R_OrigSupUQ3)
				{
					US_R_OrigSupQty3 = ZDecimal.Zero;
					if (US_SupUQ3.IsEmpty && US_SupUQ3 != US_R_OrigSupUQ3)
					{
						US_SupUQ3 = US_R_OrigSupUQ3;
					}
				}
			}
		}

		bool US_R_OrigSupUQ3_ReadOnly
		{
			get { return true; }
		}

		internal bool HasDecrease
		{
			get
			{
				var parentAndChildLines = new System.Collections.Generic.List<JobComInvoiceLine>();
				parentAndChildLines.Add(this);
				parentAndChildLines.AddRange(ChildLines);
				if (IsChildLine && ParentTariffLine != null)
				{
					parentAndChildLines.Add(ParentTariffLine);
				}
				return parentAndChildLines.Any(line => line.HasDecreaseCore);
			}
		}

		public void ResetValuesForRecon()
		{
			IBusinessObjectInternals objectInternals = this;
			var oldIsCopying = objectInternals.IsCopying;
			try
			{
				objectInternals.IsCopying = true;
				GetAddInfo().LoadPropertiesFromString(JI_AddInfo);

				OriginalTariffFormatted = JI_FormattedTariff.Left(12);
				OriginalSupTariffFormatted = SupTariffFormatted.Left(12);

				US_R_OrigOverrideDuty = US_OverrideDuty;
				US_R_OrigDuty = US_Duty;
				US_R_OrigTaxCode = US_TaxCode;
				US_R_OrigTaxApply = US_TaxApply;
				US_R_OrigTaxRate = US_TaxRate;
				US_R_OrigTaxRateS = US_TaxRateS;
				US_R_OrigTaxRateT = US_TaxRateT;
				US_R_OrigTaxQty = US_TaxQty;

				US_R_OrigOverrideSupDuty = US_OverrideSupDuty;
				US_R_OrigSupDuty = US_SupDuty;
				US_R_Orig98Value = US_98GoodsValue;

				US_R_OrigHMFAmount = US_R_ReconHMFAmount;
				US_R_OrigMPFAmount = US_R_ReconMPFAmount;
				US_R_OrigCV = JI_LinePrice;
				US_R_Textile = US_R_Textile;

				US_R_OrigRateType = US_SelectedRateType;
				US_R_OrigCottonFeeExempt = US_CottonFeeExempt;
				US_R_OrigSPI = US_SPI;
				US_R_OrigFirstUQ = JI_CustomsUnitQty.Left(3);
				US_R_OrigFirstQty = JI_CustomsQuantity;
				US_R_OrigSecondUQ = JI_CustomsSecondUnitQty.Left(3);
				US_R_OrigSecondQty = JI_CustomsSecondQuantity;
				US_R_OrigThirdQty = JI_CustomsThirdQuantity;
				US_R_OrigThirdUQ = JI_CustomsThirdUnitQty.Left(3);

				US_R_OrigSupUQ1 = US_SupUQ1;
				US_R_OrigSupQty1 = US_SupQty1;
				US_R_OrigSupUQ2 = US_SupUQ2;
				US_R_OrigSupQty2 = US_SupQty2;
				US_R_OrigSupUQ3 = US_SupUQ3;
				US_R_OrigSupQty3 = US_SupQty3;
			}
			finally
			{
				objectInternals.IsCopying = oldIsCopying;
			}
		}

		bool HasDecreaseCore
		{
			get
			{
				var reconPayable = FeeCusCodes.Cast<FeeCusCodeData>().Sum(x => x.CY_FeeAmount) + US_Duty;
				var origPayable = ReconOriginalCharges.Cast<ReconEntryOriginalCharge>().Sum(x => x.CY_Amount) + US_R_OrigDuty;
				return reconPayable < origPayable;
			}
		}

		public ZBool US_R_OverrideOriginMPF
		{
			get
			{
				if (firstOriginMPFCharge == null || firstOriginMPFCharge.IsDeleted)
				{
					firstOriginMPFCharge = ReconOriginalCharges.GetCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
				}

				return firstOriginMPFCharge != null && firstOriginMPFCharge.CY_IsOverridden;
			}
			set
			{
				if (firstOriginMPFCharge == null || firstOriginMPFCharge.IsDeleted)
				{
					firstOriginMPFCharge = ReconOriginalCharges.GetCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
				}

				if (firstOriginMPFCharge == null)
				{
					firstOriginMPFCharge = ReconOriginalCharges.AddNew();
					firstOriginMPFCharge.CY_Code = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
				}

				firstOriginMPFCharge.CY_IsOverridden = value;
				US_R_OverrideOriginMPFInfo.RefreshBinding();
			}
		}
		ReconEntryOriginalCharge firstOriginMPFCharge;

		public ZPropertyInfo US_R_OverrideOriginMPFInfo
		{
			get { return GetZPropertyInfo(Schema.US_R_OverrideOriginMPF); }
		}

		public ZBool US_R_OverrideOriginHMF
		{
			get
			{
				if (firstOriginHMFCharge == null || firstOriginHMFCharge.IsDeleted)
				{
					firstOriginHMFCharge = ReconOriginalCharges.GetCharge(Core.Constants.USCustoms.FeeCodes.HMF);
				}

				return firstOriginHMFCharge != null && firstOriginHMFCharge.CY_IsOverridden;
			}
			set
			{
				if (firstOriginHMFCharge == null || firstOriginHMFCharge.IsDeleted)
				{
					firstOriginHMFCharge = ReconOriginalCharges.GetCharge(Core.Constants.USCustoms.FeeCodes.HMF);
				}

				if (firstOriginHMFCharge == null)
				{
					firstOriginHMFCharge = ReconOriginalCharges.AddNew();
					firstOriginHMFCharge.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
				}

				firstOriginHMFCharge.CY_IsOverridden = value;
				US_R_OverrideOriginHMFInfo.RefreshBinding();
			}
		}
		ReconEntryOriginalCharge firstOriginHMFCharge;

		public ZPropertyInfo US_R_OverrideOriginHMFInfo
		{
			get { return GetZPropertyInfo(Schema.US_R_OverrideOriginHMF); }
		}

		public ZBool US_R_OverrideOrigOtherFeeAmount
		{
			get
			{
				if (firstOriginOtherFee == null || firstOriginOtherFee.IsDeleted)
				{
					firstOriginOtherFee = ReconOriginalCharges.GetFirstFeeOtherThanTaxHMFOrMPF();
				}

				return firstOriginOtherFee != null && firstOriginOtherFee.CY_IsOverridden;
			}
			set
			{
				if (firstOriginOtherFee == null || firstOriginOtherFee.IsDeleted)
				{
					firstOriginOtherFee = ReconOriginalCharges.GetFirstFeeOtherThanTaxHMFOrMPF();
				}

				if (firstOriginOtherFee == null)
				{
					firstOriginOtherFee = ReconOriginalCharges.AddNew();
				}

				firstOriginOtherFee.CY_IsOverridden = value;
				US_R_OverrideOrigOtherFeeAmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_R_OverrideOrigOtherFeeAmountInfo
		{
			get { return GetZPropertyInfo(Schema.US_R_OverrideOrigOtherFeeAmount); }
		}

		#endregion

		#region Recon Fields

		public ZDecimal US_R_ReconMPFAmount
		{
			get { return FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing); }
			set
			{
				FeeCusCodes.SetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, value);
				US_R_ReconMPFAmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_R_ReconMPFAmountInfo
		{
			get { return GetZPropertyInfo(Schema.US_R_ReconMPFAmount); }
		}

		internal bool US_R_ReconMPFAmount_ReadOnly
		{
			get { return !US_R_OverrideReconMPF; }
		}

		public ZDecimal US_R_ReconHMFAmount
		{
			get { return FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF); }
			set
			{
				FeeCusCodes.SetAmount(Core.Constants.USCustoms.FeeCodes.HMF, value);
				US_R_ReconHMFAmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_R_ReconHMFAmountInfo
		{
			get { return GetZPropertyInfo(Schema.US_R_ReconHMFAmount); }
		}

		internal bool US_R_ReconHMFAmount_ReadOnly
		{
			get { return !US_R_OverrideReconHMF; }
		}

		bool IsReconFeeOtherThanTaxHMFOrMPF(ZString code)
		{
			return code != Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing
				&& code != Core.Constants.USCustoms.FeeCodes.HMF
				&& !CusFeeCodeConstants.IsExciseTax(code);
		}

		FeeCusCodeData GetFirstReconFeeOtherThanTaxHMFOrMPF()
		{
			FeeCusCodeData fee = null;
			var otherFees = FeeCusCodes.Cast<FeeCusCodeData>().Where(x => IsReconFeeOtherThanTaxHMFOrMPF(x.CY_Code));
			if (otherFees.Any())
			{
				var validFees = otherFees.Cast<FeeCusCodeData>().Where(x => x.CY_FeeAmount > 0);
				if (validFees.Any())
				{
					var cottenFee = validFees.FirstOrDefault(x => x.CY_Code == Core.Constants.USCustoms.FeeCodes.Cotton);
					fee = cottenFee ?? validFees.FirstOrDefault();
				}
				else
				{
					fee = otherFees.FirstOrDefault();
				}
			}
			return fee;
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.ReconFeeAndChargeList))]
		[MaxLength(3)]
		public ZString US_R_ReconOtherFeeCode
		{
			get => GetFirstReconFeeOtherThanTaxHMFOrMPF()?.CY_Code ?? ZString.Empty;
			set
			{
				CheckMaximumLength(US_R_OrigOtherFeeCodeInfo, value);
				var fee = GetFirstReconFeeOtherThanTaxHMFOrMPF();
				if (fee == null)
				{
					if (value != ZString.Empty)
					{
						var newFee = FeeCusCodes.AddNew(value);
					}
				}
				else
				{
					fee.CY_Code = value;
				}

				if (US_R_ReconOtherFeeCode.IsEmpty)
				{
					US_R_ReconOtherFeeAmount = ZDecimal.Zero;
				}
			}
		}

		public ZPropertyInfo US_R_ReconOtherFeeCodeInfo => GetZPropertyInfo(Schema.US_R_ReconOtherFeeCode);

		public ZDecimal US_R_ReconOtherFeeAmount
		{
			get => FeeCusCodes.GetFeeOrChargeAmount(US_R_ReconOtherFeeCode);
			set => FeeCusCodes.SetAmount(US_R_ReconOtherFeeCode, value);
		}

		bool US_R_ReconOtherFeeAmount_ReadOnly => !US_R_OverrideReconOtherFeeAmount;

		public ZPropertyInfo US_R_ReconOtherFeeAmountInfo
		{
			get { return GetZPropertyInfo(Schema.US_R_ReconOtherFeeAmount); }
		}

		public void UpdateReconChargeDetails()
		{
			US_R_OverrideReconHMFInfo.RefreshBinding();
			US_R_ReconHMFAmountInfo.RefreshBinding();
			US_R_OverrideReconMPFInfo.RefreshBinding();
			US_R_ReconMPFAmountInfo.RefreshBinding();
			US_R_OverrideReconOtherFeeAmountInfo.RefreshBinding();
			US_R_ReconOtherFeeCodeInfo.RefreshBinding();
			US_R_ReconOtherFeeAmountInfo.RefreshBinding();
		}

		public ZBool US_R_OverrideReconMPF
		{
			get
			{
				if (firstReconMPFCharge == null || firstReconMPFCharge.IsDeleted)
				{
					firstReconMPFCharge = FeeCusCodes.GetCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
				}

				return firstReconMPFCharge != null && firstReconMPFCharge.CY_IsOverridden;
			}
			set
			{
				if (firstReconMPFCharge == null || firstReconMPFCharge.IsDeleted)
				{
					firstReconMPFCharge = FeeCusCodes.GetCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
				}

				if (firstReconMPFCharge == null)
				{
					firstReconMPFCharge = FeeCusCodes.AddNew();
					firstReconMPFCharge.CY_Code = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
				}

				firstReconMPFCharge.CY_IsOverridden = value;
				US_R_OverrideReconMPFInfo.RefreshBinding();
			}
		}
		FeeCusCodeData firstReconMPFCharge;

		public ZPropertyInfo US_R_OverrideReconMPFInfo
		{
			get { return GetZPropertyInfo(Schema.US_R_OverrideReconMPF); }
		}

		public ZBool US_R_OverrideReconHMF
		{
			get
			{
				if (firstReconHMFCharge == null || firstReconHMFCharge.IsDeleted)
				{
					firstReconHMFCharge = FeeCusCodes.GetCharge(Core.Constants.USCustoms.FeeCodes.HMF);
				}

				return firstReconHMFCharge != null && firstReconHMFCharge.CY_IsOverridden;
			}
			set
			{
				if (firstReconHMFCharge == null || firstReconHMFCharge.IsDeleted)
				{
					firstReconHMFCharge = FeeCusCodes.GetCharge(Core.Constants.USCustoms.FeeCodes.HMF);
				}

				if (firstReconHMFCharge == null)
				{
					firstReconHMFCharge = FeeCusCodes.AddNew();
					firstReconHMFCharge.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
				}

				firstReconHMFCharge.CY_IsOverridden = value;
				US_R_OverrideReconHMFInfo.RefreshBinding();
			}
		}
		FeeCusCodeData firstReconHMFCharge;

		public ZPropertyInfo US_R_OverrideReconHMFInfo
		{
			get { return GetZPropertyInfo(Schema.US_R_OverrideReconHMF); }
		}

		public ZBool US_R_OverrideReconOtherFeeAmount
		{
			get => GetFirstReconFeeOtherThanTaxHMFOrMPF()?.CY_IsOverridden ?? false;
			set
			{
				var fee = GetFirstReconFeeOtherThanTaxHMFOrMPF() ?? FeeCusCodes.AddNew();

				fee.CY_IsOverridden = value;
				US_R_OverrideReconOtherFeeAmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_R_OverrideReconOtherFeeAmountInfo
		{
			get { return GetZPropertyInfo(Schema.US_R_OverrideReconOtherFeeAmount); }
		}

		#endregion

		public ZInt LinePriceDecimals
		{
			get { return 2; }
		}

		#region Related Objects

		/// <summary>
		/// For recon. This is the tariff number on original import jobs
		/// </summary>
		public USCTariff OriginalImportTariff
		{
			get
			{
				return Factory.GetCachedValue(US_R_OrigTariff + EffectiveDateForDutyRate.ToString(),
					delegate
					{
						return new USCTariff.Loader(Factory).LoadBestMatch(US_R_OrigTariff, EffectiveDateForDutyRate);
					});
			}
		}

		/// <summary>
		/// For recon. This is the tariff number on original import jobs
		/// </summary>
		public USCTariff OriginalImportSupTariff
		{
			get
			{
				return Factory.GetCachedValue(US_R_OrigSupTariff + EffectiveDateForDutyRate.ToString(),
					delegate
					{
						return new USCTariff.Loader(Factory).LoadBestMatch(US_R_OrigSupTariff, EffectiveDateForDutyRate);
					});
			}
		}

		#endregion

		#region IReconOriginalChargeParent Members

		CodeDescriptionPairList IReconOriginalChargeParent.FeeAndChargeList
		{
			get => CusFeeCodeConstants.GetAccountingClassFeeCodeList(Factory);
		}

		bool IReconOriginalChargeParent.DefaultValueForOverridenForNewChild
		{
			get { return false; }
		}

		USCTariff IReconOriginalChargeParent.Tariff
		{
			get { return ImportTariff; }
		}

		void IReconOriginalChargeParent.SynchroniseOnNonCommittedAdded(ReconEntryOriginalCharge charge)
		{
			if (charge != null && !charge.CY_Code.IsEmpty)
			{
				ZString chargeType = charge.CY_Code;
				if (FeeCusCodes.Find(new ZQuery(CusCodeDataSchema.CY_Code, chargeType)).Length == 0)
				{
					FeeCusCodes.AddNew(chargeType);
				}
			}
		}

		bool IReconOriginalChargeParent.ShouldCalculateOrigDuty
		{
			get { return InvoiceHeader != null && InvoiceHeader.ReconOriginalEntry != null ? InvoiceHeader.ReconOriginalEntry.US_R_CalcOrigDuty : ZBool.False; }
		}

		bool IReconOriginalChargeParent.MonthlyFiling => false;

		void IReconOriginalChargeParent.UpdateChargeDetails()
		{
			US_R_OverrideOriginHMFInfo.RefreshBinding();
			US_R_OrigHMFAmountInfo.RefreshBinding();
			US_R_OverrideOriginMPFInfo.RefreshBinding();
			US_R_OrigMPFAmountInfo.RefreshBinding();
			US_R_OverrideOrigOtherFeeAmountInfo.RefreshBinding();
			US_R_OrigOtherFeeCodeInfo.RefreshBinding();
			US_R_OrigOtherFeeAmountInfo.RefreshBinding();
		}

		BusinessObject IReconOriginalChargeParent.ParentAsBusinessObject
		{
			get { return this; }
		}

		#endregion

		public bool IsChangedForReconForOneLine()
		{
			bool result =
				JI_Tariff != US_R_OrigTariff ||
				ReconMergeStrategy.GetSPIToMerge(US_SPI) != ReconMergeStrategy.GetSPIToMerge(US_R_OrigSPI) ||
				JI_CustomsQuantity != US_R_OrigFirstQty ||
				JI_CustomsSecondQuantity != US_R_OrigSecondQty ||
				JI_CustomsThirdQuantity != US_R_OrigThirdQty ||
				US_SelectedRateType != US_R_OrigRateType ||
				JI_CustomsValue - US_98GoodsValue != US_R_OrigCV ||
				US_SupTariff != US_R_OrigSupTariff ||
				US_SupQty1 != US_R_OrigSupQty1 ||
				US_SupQty2 != US_R_OrigSupQty2 ||
				US_SupQty3 != US_R_OrigSupQty3 ||
				US_98GoodsValue != US_R_Orig98Value;

			if (!result)//duty and fee calculation will mostly be affected by the change above. However duty/fee rate change or manual override can happen 
			{
				if (US_Duty != US_R_OrigDuty || US_SupDuty != US_R_OrigSupDuty)
				{
					result = true;
				}
				else
				{
					foreach (CodeDescriptionPair pair in CusFeeCodeConstants.GetAccountingClassFeeCodeList(Factory))
					{
						if (ReconOriginalCharges.GetFeeOrChargeAmount(pair.Code) != FeeCusCodes.GetFeeOrChargeAmount(pair.Code))
						{
							result = true;
							break;
						}
					}
				}
			}

			return result;
		}
	}
}
