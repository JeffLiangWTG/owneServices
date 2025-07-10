using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DutyCalculator;

public class InvoiceLineUniversalRateCalcData : IUniversalRateCalcData, IUniversalRateDataWithCustomsValueFormula
{
	public InvoiceLineUniversalRateCalcData(BaseJobComInvoiceLine invoiceLine, RateView rateView)
	{
		InvoiceLine = Argument.NotNull(invoiceLine, nameof(InvoiceLine));
		RateView = Argument.NotNull(rateView, nameof(RateView));
	}

	public BaseJobComInvoiceLine InvoiceLine { get; }
	public RateView RateView { get; }

	public DateTime DateOfValuation => InvoiceLine.EffectiveAssessmentDate.ToDateTime();

	public decimal ValueForDuty
	{
		get
		{
			if (string.IsNullOrWhiteSpace(CustomsValueFormula))
			{
				return CustomsValue;
			}
			else
			{
				if (!cachedValueForDuty.HasValue)
				{
					var calcResult = new UniversalRateCalculator(CustomsValueFormula, this).Calculate();
					cachedValueForDuty = Utilities.Round(calcResult, 3);
				}

				return cachedValueForDuty.Value;
			}
		}
	}
	decimal? cachedValueForDuty;

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
	protected virtual decimal CustomsValueCore => InvoiceLine.JI_CustomsValue;

	public IDictionary<string, decimal> UnitOfMeasureValueList => unitOfMeasureValueList ??= GetUnitOfMeasureValueList();
	IDictionary<string, decimal> unitOfMeasureValueList;

	public IDictionary<string, decimal> CountrySpecificValueList => countrySpecificValueList ??= GetCountrySpecificValueDictionary();
	IDictionary<string, decimal> countrySpecificValueList;

	public IList<Tuple<string, string>> AdditionalInformationList => additionalInformationList ??= GetInitialAdditionalInformationList();
	IList<Tuple<string, string>> additionalInformationList;

	public IDictionary<string, string> MeursingExpressionList => meursingExpressionList ??= GetMeursingExpressionList();
	IDictionary<string, string> meursingExpressionList;

	protected virtual IDictionary<string, decimal> GetUnitOfMeasureValueList()
		=> new RateCalcUnitOfMeasureAggregator().GetUomQtyDictionary(InvoiceLine);

	protected virtual IList<Tuple<string, string>> GetInitialAdditionalInformationList() => new List<Tuple<string, string>>();

	protected virtual IDictionary<string, string> GetMeursingExpressionList()
		=> new RateCalcMeursingExpressionReplacer<BaseJobComInvoiceLine>(InvoiceLine, i => i).ReplaceApplicableMeursingExpressions(RateView);

	protected virtual IDictionary<string, decimal> GetCountrySpecificValueDictionary()
		=> new RateCalcSpecificValueAggregator().GetSpecificValueDictionary(InvoiceLine);
}
