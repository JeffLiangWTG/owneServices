namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using System.Collections.Generic;
	using CargoWise.Types;

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement)]
	public partial class PMSQ1 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement)]
	public partial class PMSQ2 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement)]
	public partial class PMSQA : MessageBlock, IStatementFees
	{
		IEnumerable<KeyValuePair<ZString, ZDecimal>> IStatementFees.Fees
		{
			get
			{
				if (FirstFeeAmount > 0m)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(FirstFeeClassCode, FirstFeeAmount);
				}

				if (SecondFeeAmount > 0m)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(SecondFeeClassCode, SecondFeeAmount);
				}

				if (ThirdFeeAmount > 0m)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(ThirdFeeClassCode, ThirdFeeAmount);
				}

				if (FourthFeeAmount > 0m)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(FourthFeeClassCode, FourthFeeAmount);
				}

				if (FifthFeeAmount > 0m)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(FifthFeeClassCode, FifthFeeAmount);
				}
			}
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement)]
	public partial class PMSQ3 : MessageBlock, IStatementFees
	{
		IEnumerable<KeyValuePair<ZString, ZDecimal>> IStatementFees.Fees
		{
			get
			{
				if (!TotalDuty.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Duty, TotalDuty);
				}
				if (!TotalTax.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable, TotalTax);
				}
			}
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement)]
	public partial class PMSQ4 : MessageBlock, IStatementFees
	{
		IEnumerable<KeyValuePair<ZString, ZDecimal>> IStatementFees.Fees
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
			}
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement)]
	public partial class PMSQE : MessageBlock, IStatementFees
	{
		IEnumerable<KeyValuePair<ZString, ZDecimal>> IStatementFees.Fees
		{
			get
			{
				if (FirstFeeAmount > 0m)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(FirstFeeClassCode, FirstFeeAmount);
				}

				if (SecondFeeAmount > 0m)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(SecondFeeClassCode, SecondFeeAmount);
				}

				if (ThirdFeeAmount > 0m)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(ThirdFeeClassCode, ThirdFeeAmount);
				}

				if (FourthFeeAmount > 0m)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(FourthFeeClassCode, FourthFeeAmount);
				}

				if (FifthFeeAmount > 0m)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(FifthFeeClassCode, FifthFeeAmount);
				}
			}
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement)]
	public partial class PMSQ5 : MessageBlock, IStatementFees
	{
		IEnumerable<KeyValuePair<ZString, ZDecimal>> IStatementFees.Fees
		{
			get
			{
				if (!TotalDuty.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.Duty, TotalDuty);
				}
				if (!TotalTax.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable, TotalTax);
				}
			}
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement)]
	public partial class PMSQ6 : MessageBlock, IStatementFees
	{
		IEnumerable<KeyValuePair<ZString, ZDecimal>> IStatementFees.Fees
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
			}
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement)]
	public partial class PMSQJ : MessageBlock, IStatementFees
	{
		IEnumerable<KeyValuePair<ZString, ZDecimal>> IStatementFees.Fees
		{
			get
			{
				if (FirstFeeAmount > 0m)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(FirstFeeClassCode, FirstFeeAmount);
				}

				if (SecondFeeAmount > 0m)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(SecondFeeClassCode, SecondFeeAmount);
				}

				if (ThirdFeeAmount > 0m)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(ThirdFeeClassCode, ThirdFeeAmount);
				}

				if (FourthFeeAmount > 0m)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(FourthFeeClassCode, FourthFeeAmount);
				}

				if (FifthFeeAmount > 0m)
				{
					yield return new KeyValuePair<ZString, ZDecimal>(FifthFeeClassCode, FifthFeeAmount);
				}
			}
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement)]
	public partial class PMSQ7_01 : MessageBlock, IStatementDeletedEntries
	{
		IEnumerable<KeyValuePair<ZString, ZString>> IStatementDeletedEntries.DeletedEntries
		{
			get
			{
				if (!EntrySummaryNumber.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZString>(EntrySummaryNumber, DeleteSource);
				}

				if (!EntrySummaryNumber1.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZString>(EntrySummaryNumber1, DeleteSource1);
				}

				if (!EntrySummaryNumber2.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZString>(EntrySummaryNumber2, DeleteSource2);
				}

				if (!EntrySummaryNumber3.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZString>(EntrySummaryNumber3, DeleteSource3);
				}
			}
		}

		ZString IStatementDeletedEntries.StatementNumber
		{
			get { return PeriodicDailyStatementNumber; }
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement)]
	public partial class PMSQ7_02 : MessageBlock, IStatementDeletedEntries
	{
		IEnumerable<KeyValuePair<ZString, ZString>> IStatementDeletedEntries.DeletedEntries
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

		ZString IStatementDeletedEntries.StatementNumber
		{
			get { return PeriodicDailyStatementNumber; }
		}
	}

	[ApplicationIdentifier(MessageBlockDictionary.EmptyApplicationIdentifier, "ASC")]
	public partial class PMSQX : MessageBlock, IStatementRerouteResponse
	{
		#region IRerouteResponse Members

		ZString IStatementRerouteResponse.ErrorCode
		{
			get { return ErrorCode; }
		}

		ZString IStatementRerouteResponse.MessageText
		{
			get { return MessageText; }
		}

		ZInt IStatementRerouteResponse.TotalNumberOfReroutes
		{
			get { return TotalNumberOfReroutes; }
		}

		#endregion
	}
}
