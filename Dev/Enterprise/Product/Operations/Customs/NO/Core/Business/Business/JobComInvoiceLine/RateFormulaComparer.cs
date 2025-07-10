using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NO.Business
{
	public class RateFormulaComparer : IComparer<ZString>
	{
		public RateFormulaComparer(IUniversalRateCalcData universalRateCalcData)
		{
			UniversalRateCalcData = universalRateCalcData;
		}

		public IUniversalRateCalcData UniversalRateCalcData { get; }

		int IComparer<ZString>.Compare(ZString x, ZString y)
		{
			if (x == y)
			{ return 0; }
			if (IsZeroOrEmpty(x))
			{ return -1; }
			if (IsZeroOrEmpty(y))
			{ return 1; }
			return GetDutyAmount(x).CompareTo(GetDutyAmount(y));
		}

		static bool IsZeroOrEmpty(ZString formula)
		{
			return formula == "0" || formula == ZString.Empty;
		}

		ZDecimal GetDutyAmount(ZString formula)
		{
			return DutyCalculatorHelper.Calculate(UniversalRateCalcData, formula, 2, ZString.Empty, ZDecimal.Zero, shouldTruncate: false, canBeNegative: false);
		}
	}
}
