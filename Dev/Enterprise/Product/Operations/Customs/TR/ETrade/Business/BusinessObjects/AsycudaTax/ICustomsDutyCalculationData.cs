using CargoWise.Types;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public interface ICustomsDutyCalculationData
	{
		ZString DataGrouping { get; }
		ZDateTime EffectiveDate { get; }
		ZString RateType { get; }
		ZString RateCode { get; }
		ZString TariffType { get; }
		ZString TariffCode { get; }
		ZDecimal CustomsValue { get; set; }
		ZString ExportCountry { get; }
		ZString AdditionalCode { get; }

		ZString GetFixedDutyFormula();
	}
}
