using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public interface IDutyResult
	{
		Money TotalAmount { get; }
		ZDecimal PercentOfValue { get; }
		Money PerUnitAmount { get; }
		ZString PerUnitUQ { get; }
		ZString RateString { get; }
		ZDecimal NoneCustomsValueAmount { get; }

		void ClearIfDutyIsNotToBeCalculated();
	}

	public class DutyResult : IDutyResult
	{
		public DutyResult()
		{
			TotalAmount = Money.Empty;
		}

		public Money TotalAmount
		{
			get { return totalAmount; }
			set { totalAmount = value; }
		}

		public ZDecimal PercentOfValue
		{
			get { return percentOfValue; }
			set { percentOfValue = value; }
		}

		public ZDecimal PerUnitAmount
		{
			get { return perUnitAmount; }
			set { perUnitAmount = value; }
		}

		public ZString PerUnitUQ
		{
			get { return perUnitUQ; }
			set { perUnitUQ = value; }
		}

		public ZDecimal NoneCustomsValueAmount
		{
			get { return noneCustomsValueAmount; }
			set { noneCustomsValueAmount = value; }
		}

		public bool IsRateStringFree
		{
			get { return RateString.EndsWith(DutyFreeString, System.StringComparison.OrdinalIgnoreCase); }
		}

		internal const string DutyFreeString = "Free";

		public ZString RateString
		{
			get { return rateString; }
			set { rateString = value; }
		}

		public void ClearIfDutyIsNotToBeCalculated()
		{
			if (InvalidDutyRate.IsInvalid(PercentOfValue) || InvalidDutyRate.IsInvalid(PerUnitAmount))
			{
				PercentOfValue = 0m;
				TotalAmount = Money.Empty;
				PerUnitAmount = 0m;
				PerUnitUQ = "";
				RateString = ZString.Empty;
				NoneCustomsValueAmount = 0m;
			}
		}

		#region Implementation

		Money totalAmount;
		ZDecimal percentOfValue;
		ZDecimal perUnitAmount;
		ZString perUnitUQ;
		ZString rateString;
		ZDecimal noneCustomsValueAmount;

		Money IDutyResult.PerUnitAmount
		{
			get { return new Money(PerUnitAmount, JobDeclaration.GetLocalCurrency()); }
		}

		#endregion
	}
}
