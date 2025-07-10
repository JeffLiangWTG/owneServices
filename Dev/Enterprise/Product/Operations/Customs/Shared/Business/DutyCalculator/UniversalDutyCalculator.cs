using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DutyCalculator;

public abstract class UniversalDutyCalculator<TEntity, TUniversalRateCalcData> : IUniversalDutyCalculator
	where TEntity : BusinessObject
	where TUniversalRateCalcData : IUniversalRateCalcData
{
	protected UniversalDutyCalculator(TEntity entity,
		RateCalculationVisitorMode rateCalculationVisitorMode,
		Func<TEntity, RateView, IUniversalRateCalcData> createCalcDataFactory)
	{
		this.entity = Argument.NotNull(entity, nameof(this.entity));
		this.createCalcDataFactory = Argument.NotNull(createCalcDataFactory, nameof(createCalcDataFactory));
		this.rateCalculationVisitorMode = rateCalculationVisitorMode;
	}

	readonly TEntity entity;
	readonly Func<TEntity, RateView, IUniversalRateCalcData> createCalcDataFactory;
	readonly RateCalculationVisitorMode rateCalculationVisitorMode;

	public IDutyCalculationResult CleanFormulaAndCalculate(RateView rateView)
	{
		Argument.NotNull(rateView, nameof(rateView));

		var rateFormula = CleanupFormula(rateView);
		return Calculate(rateFormula, rateView);
	}

	protected IDutyCalculationResult Calculate(ZString rateFormula, RateView rateView)
	{
		var universalRateData = CreateUniversalRateCalcData(rateView);
		SetCustomsValueFormula(universalRateData, rateView);
		var calculationResult = RateCalculationVisitor.CalculateDuties(universalRateData, rateFormula, GetRateCalculationVisitorCreator(rateView));
		return calculationResult;
	}

	protected void SetCustomsValueFormula(IUniversalRateCalcData universalRateData, RateView rateForCalculation)
	{
		var formula = rateForCalculation?.CusRateType?.ZZR_CustomsValueFormula ?? ZString.Empty;
		if (!formula.IsEmpty && universalRateData is IUniversalRateDataWithCustomsValueFormula dataWithCustomsValueFormula)
		{
			dataWithCustomsValueFormula.CustomsValueFormula = formula;
		}
	}

	protected virtual ZString GetConvertedFormulaForCleanUpFormula(RateView rateView)
	{
		return rateView.ZZ2_RateFormula;
	}

	protected virtual IRateCalculationVisitorCreator GetRateCalculationVisitorCreator(RateView rateView)
		=> RateCalculationVisitorFactory.CreateVisitor(rateCalculationVisitorMode);

	protected internal IUniversalRateCalcData CreateUniversalRateCalcData(RateView rateView) => createCalcDataFactory(entity, rateView);

	ZString CleanupFormula(RateView rateView)
	{
		var result = GetConvertedFormulaForCleanUpFormula(rateView);

		if (result == "0")
		{
			result = FormattableString.Invariant($"{UniversalReferenceConstants.ValueForDuty}*{result}");
		}

		return result;
	}
}
