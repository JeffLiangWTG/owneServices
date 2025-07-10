using System;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface ITaxAmountCalculator
	{
		ZDecimal GetExtraTaxAmountFromTaxAmount(ZDecimal taxAmount, ZDecimal rate, ZDecimal effectiveExtraRate);

		(ZDecimal oSTaxAmount, ZDecimal oSExTaxAmount) SplitOSTotalToTaxAndExTaxAmounts(ZDecimal localTaxAmount, Func<ZDecimal> calculateOSExTaxAmountFromLocalExTaxAmount, ZDecimal overseasTotal);

		ZDecimal GetExtraTaxAmountFromExTaxAmount(ZDecimal exTaxAmount, ZDecimal effectiveExtraRate);
	}
}
