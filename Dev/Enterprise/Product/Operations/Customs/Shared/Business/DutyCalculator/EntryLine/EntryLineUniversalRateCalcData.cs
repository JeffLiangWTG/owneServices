using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DutyCalculator;

public class EntryLineUniversalRateCalcData : IUniversalRateCalcData, IUniversalRateDataWithCustomsValueFormula
{
	public EntryLineUniversalRateCalcData(CusEntryLine entryLine, RateView rateView)
	{
		EntryLine = Argument.NotNull(entryLine, nameof(entryLine));
		RandomLine = Argument.NotNull(entryLine.RandomLine, nameof(entryLine.RandomLine));
		RateView = rateView;
	}

	protected CusEntryLine EntryLine { get; }

	protected BaseJobComInvoiceLine RandomLine { get; }

	protected RateView RateView { get; }

	public virtual DateTime DateOfValuation => RandomLine.EffectiveAssessmentDate.ToDateTime();

	public decimal ValueForDuty
	{
		get
		{
			if (string.IsNullOrWhiteSpace(CustomsValueFormula))
			{
				return CustomsValue;
			}

			if (!cachedValueForDuty.HasValue)
			{
				var calcResult = new UniversalRateCalculator(CustomsValueFormula, this).Calculate();
				cachedValueForDuty = GetRoundedValueForDuty(calcResult);
			}

			return cachedValueForDuty.Value;
		}
	}

	decimal? cachedValueForDuty;

	decimal GetRoundedValueForDuty(decimal valueForDuty) => Utilities.Round(valueForDuty, 3);

	public string CustomsValueFormula
	{
		get { return customsValueFormula; }
		set
		{
			cachedValueForDuty = null;
			customsValueFormula = value;
		}
	}
	string customsValueFormula;

	public decimal CustomsValue => CustomsValueCore;

	protected virtual decimal CustomsValueCore => EntryLine.CL_CustomsValue;

	public IDictionary<string, decimal> UnitOfMeasureValueList => unitOfMeasureValueList ??= GetUnitOfMeasureValueList();

	IDictionary<string, decimal> unitOfMeasureValueList;

	public IDictionary<string, decimal> CountrySpecificValueList => countrySpecificValueList ??= GetCountrySpecificValueDictionary();
	IDictionary<string, decimal> countrySpecificValueList;

	public IList<Tuple<string, string>> AdditionalInformationList => additionalInformationList ??= GetInitialAdditionalInformationList();
	IList<Tuple<string, string>> additionalInformationList;

	public IDictionary<string, string> MeursingExpressionList => meursingExpressionList ??= GetMeursingExpressionList();
	IDictionary<string, string> meursingExpressionList;

	protected virtual IList<Tuple<string, string>> GetInitialAdditionalInformationList() => new List<Tuple<string, string>>();

	protected virtual IDictionary<string, string> GetMeursingExpressionList()
		=> new RateCalcMeursingExpressionReplacer<CusEntryLine>(EntryLine, e => e.RandomLine).ReplaceApplicableMeursingExpressions(RateView);

	protected virtual IDictionary<string, decimal> GetCountrySpecificValueDictionary()
		=> new RateCalcSpecificValueAggregator().GetSpecificValueDictionary(EntryLine);

	protected virtual IDictionary<string, decimal> GetUnitOfMeasureValueList()
		=> new RateCalcUnitOfMeasureAggregator().GetUomQtyDictionary(EntryLine);
}
