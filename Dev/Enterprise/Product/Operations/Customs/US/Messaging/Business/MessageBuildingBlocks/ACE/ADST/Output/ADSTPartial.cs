
namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using System.Collections.Generic;
	using CargoWise.Types;
	using Enterprise.Registry.Business.Customs.US;

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class ADSTQ1 : MessageBlock, IDailyStatementDutyAndTax
	{
		ZString IDailyStatementDutyAndTax.EntryFilerCode
		{
			get { return EntryFilerCode; }
		}

		ZString IDailyStatementDutyAndTax.EntryNumber
		{
			get { return EntryNumber; }
		}

		ZString IDailyStatementDutyAndTax.EntryType
		{
			get { return EntryType; }
		}

		ZString IDailyStatementDutyAndTax.DistrictPortOfEntrySummary
		{
			get { return DistrictPortOfEntrySummary; }
		}

		ZDecimal IDailyStatementDutyAndTax.EstimatedDutyAmount
		{
			get { return EstimatedDutyAmount; }
		}

		ZDecimal IDailyStatementDutyAndTax.EstimatedTaxAmount
		{
			get { return EstimatedTaxAmount; }
		}

		ZString IDailyStatementDutyAndTax.DeferredTaxIndicator
		{
			get { return DeferredTaxIndicator; }
		}

		ZDecimal IDailyStatementDutyAndTax.CountervailingDutyAmount
		{
			get { return ZDecimal.Zero; }
		}

		ZDecimal IDailyStatementDutyAndTax.AntidumpingDutyAmount
		{
			get { return ZDecimal.Zero; }
		}

		ZString IDailyStatementDutyAndTax.BrokerReferenceNumber
		{
			get { return BrokerReferenceNumber; }
		}

		ZString IDailyStatementDutyAndTax.MessageType
		{
			get { return ACEApplicationIdentifierCodeList.Codes.DailyStatement; }
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class ADSTQ2 : MessageBlock, IDailyStatementDutyAndTaxContinued
	{
		ZString IDailyStatementDutyAndTaxContinued.TeamNumber
		{
			get { return TeamNumber; }
		}

		ZString IDailyStatementDutyAndTaxContinued.CensusWarningIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IDailyStatementDutyAndTaxContinued.ACEIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IDailyStatementDutyAndTaxContinued.PaperlessElectronicIndicator
		{
			get { return "P"; }
		}

		ZString IDailyStatementDutyAndTaxContinued.ElectronicInvoiceIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IDailyStatementDutyAndTaxContinued.PaymentTypeIndicator
		{
			get { return PaymentTypeIndicator; }
		}

		ZDecimal IDailyStatementDutyAndTaxContinued.CountervailingDutyAmount
		{
			get { return CountervailingDutyAmount; }
		}

		ZDecimal IDailyStatementDutyAndTaxContinued.AntidumpingDutyAmount
		{
			get { return AntidumpingDutyAmount; }
		}

		ZString IDailyStatementDutyAndTaxContinued.CountervailingIndicator
		{
			get { return CountervailingIndicator; }
		}

		ZString IDailyStatementDutyAndTaxContinued.AntidumpingIndicator
		{
			get { return AntidumpingIndicator; }
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class ADSTQ3 : MessageBlock, IStatementFees
	{
		public IEnumerable<KeyValuePair<ZString, ZDecimal>> Fees
		{
			get
			{
				if (!TotalEstimatedDuty.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Duty, TotalEstimatedDuty);
				}
				if (!TotalEstimatedTax.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable, TotalEstimatedTax);
				}
				if (!TotalDeferredTax.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred, TotalDeferredTax);
				}
			}
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class ADSTQ4 : MessageBlock, IStatementFees
	{
		public IEnumerable<KeyValuePair<ZString, ZDecimal>> Fees
		{
			get
			{
				if (!TotalAntidumpingDuty.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, TotalAntidumpingDuty);
				}
				if (!TotalCountervailingDuty.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, TotalCountervailingDuty);
				}
				if (!TotalAmountDue.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Constants.TotalAmountDue, TotalAmountDue);
				}
				if (!TotalInterestAmountForReconciliationSummary.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, TotalInterestAmountForReconciliationSummary);
				}
				if (!TotalNumberRevenueProducingEntries.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Constants.TotalNumberRevenueProducingEntries, (ZDecimal)TotalNumberRevenueProducingEntries);
				}
				if (!TotalNumberNonRevenueProducingEntries.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Constants.TotalNumberNonRevenueProducingEntries, (ZDecimal)TotalNumberNonRevenueProducingEntries);
				}
			}
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class ADSTQ5 : MessageBlock, IStatementFees
	{
		public IEnumerable<KeyValuePair<ZString, ZDecimal>> Fees
		{
			get
			{
				if (!TotalEstimatedDuty.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Duty, TotalEstimatedDuty);
				}
				if (!TotalEstimatedTax.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable, TotalEstimatedTax);
				}
				if (!TotalDeferredTax.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred, TotalDeferredTax);
				}
			}
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class ADSTQ6 : MessageBlock, IStatementFees
	{
		public IEnumerable<KeyValuePair<ZString, ZDecimal>> Fees
		{
			get
			{
				if (!TotalAntidumpingDuty.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, TotalAntidumpingDuty);
				}
				if (!TotalCountervailingDuty.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, TotalCountervailingDuty);
				}
				if (!TotalAmountDue.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Constants.TotalAmountDue, TotalAmountDue);
				}
				if (!TotalInterestAmountForReconciliationSummary.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, TotalInterestAmountForReconciliationSummary);
				}
				if (!TotalNumberRevenueProducingEntries.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Constants.TotalNumberRevenueProducingEntries, (ZDecimal)TotalNumberRevenueProducingEntries);
				}
				if (!TotalNumberNonRevenueProducingEntries.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Constants.TotalNumberNonRevenueProducingEntries, (ZDecimal)TotalNumberNonRevenueProducingEntries);
				}
			}
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class ADSTQ7 : MessageBlock, IStatementDeletedEntries
	{
		ZString IStatementDeletedEntries.StatementNumber
		{
			get { return DailyStatementNumber; }
		}

		public IEnumerable<KeyValuePair<ZString, ZString>> DeletedEntries
		{
			get
			{
				if (!EntryNumber.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZString>(EntryNumber, DeleteSource);
				}
				if (!EntryNumber1.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZString>(EntryNumber1, DeleteSource1);
				}
				if (!EntryNumber2.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZString>(EntryNumber2, DeleteSource2);
				}
				if (!EntryNumber3.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZString>(EntryNumber3, DeleteSource3);
				}
			}
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class ADSTQA : MessageBlock, IStatementFees
	{
		public IEnumerable<KeyValuePair<ZString, ZDecimal>> Fees
		{
			get
			{
				if (EntryChargeTypeList.IsFeeType(FirstFeeClassCode))
				{
					yield return new KeyValuePair<ZString, ZDecimal>(FirstFeeClassCode, FirstFeeAmount);
				}
				if (EntryChargeTypeList.IsFeeType(SecondFeeClassCode))
				{
					yield return new KeyValuePair<ZString, ZDecimal>(SecondFeeClassCode, SecondFeeAmount);
				}
				if (EntryChargeTypeList.IsFeeType(ThirdFeeClassCode))
				{
					yield return new KeyValuePair<ZString, ZDecimal>(ThirdFeeClassCode, ThirdFeeAmount);
				}
				if (EntryChargeTypeList.IsFeeType(FourthFeeClassCode))
				{
					yield return new KeyValuePair<ZString, ZDecimal>(FourthFeeClassCode, FourthFeeAmount);
				}
				if (EntryChargeTypeList.IsFeeType(FifthFeeClassCode))
				{
					yield return new KeyValuePair<ZString, ZDecimal>(FifthFeeClassCode, FifthFeeAmount);
				}
			}
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class ADSTQE : MessageBlock, IStatementFees
	{
		public IEnumerable<KeyValuePair<ZString, ZDecimal>> Fees
		{
			get
			{
				if (EntryChargeTypeList.IsFeeType(FirstFeeClassCode))
				{
					yield return new KeyValuePair<ZString, ZDecimal>(FirstFeeClassCode, FirstFeeAmount);
				}
				if (EntryChargeTypeList.IsFeeType(SecondFeeClassCode))
				{
					yield return new KeyValuePair<ZString, ZDecimal>(SecondFeeClassCode, SecondFeeAmount);
				}
				if (EntryChargeTypeList.IsFeeType(ThirdFeeClassCode))
				{
					yield return new KeyValuePair<ZString, ZDecimal>(ThirdFeeClassCode, ThirdFeeAmount);
				}
				if (EntryChargeTypeList.IsFeeType(FourthFeeClassCode))
				{
					yield return new KeyValuePair<ZString, ZDecimal>(FourthFeeClassCode, FourthFeeAmount);
				}
				if (EntryChargeTypeList.IsFeeType(FifthFeeClassCode))
				{
					yield return new KeyValuePair<ZString, ZDecimal>(FifthFeeClassCode, FifthFeeAmount);
				}
			}
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.DailyStatement)]
	public partial class ADSTQJ : MessageBlock, IStatementFees
	{
		public IEnumerable<KeyValuePair<ZString, ZDecimal>> Fees
		{
			get
			{
				if (EntryChargeTypeList.IsFeeType(FirstFeeClassCode))
				{
					yield return new KeyValuePair<ZString, ZDecimal>(FirstFeeClassCode, FirstFeeAmount);
				}
				if (EntryChargeTypeList.IsFeeType(SecondFeeClassCode))
				{
					yield return new KeyValuePair<ZString, ZDecimal>(SecondFeeClassCode, SecondFeeAmount);
				}
				if (EntryChargeTypeList.IsFeeType(ThirdFeeClassCode))
				{
					yield return new KeyValuePair<ZString, ZDecimal>(ThirdFeeClassCode, ThirdFeeAmount);
				}
				if (EntryChargeTypeList.IsFeeType(FourthFeeClassCode))
				{
					yield return new KeyValuePair<ZString, ZDecimal>(FourthFeeClassCode, FourthFeeAmount);
				}
				if (EntryChargeTypeList.IsFeeType(FifthFeeClassCode))
				{
					yield return new KeyValuePair<ZString, ZDecimal>(FifthFeeClassCode, FifthFeeAmount);
				}
			}
		}
	}
}
