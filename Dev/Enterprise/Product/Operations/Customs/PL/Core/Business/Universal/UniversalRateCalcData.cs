using System;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class UniversalRateCalcData : EU.Business.EUUniversalRateCalcData
{
	public UniversalRateCalcData(CusEntryLine entryLine, RateView rateView) : base(entryLine, rateView)
	{
	}

	protected new CusEntryLine EntryLine => (CusEntryLine)base.EntryLine;

	protected override decimal CustomsValueCore => EntryLine.CUDCurrencyConverter
		.ConvertExact(new Money(ZArchitecture.Core.Utilities.Round(base.CustomsValueCore, 0), EntryLine.LocalCurrency), EntryLine.EURCurrency, false)
		.Amount;

	public override DateTime DateOfValuation => ((JobComInvoiceLine)RandomLine).GetCurrencyConverterDateForRate.ToDateTime();
}
