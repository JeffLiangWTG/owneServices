using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DutyCalculator;

public class RateCalcMeursingExpressionReplacer<TEntity> where TEntity : class, IBusiness, IDeclarationProvider
{
	public RateCalcMeursingExpressionReplacer(TEntity entity, Func<TEntity, BaseJobComInvoiceLine> invoiceLineProviderFunction)
	{
		Entity = Argument.NotNull(entity, nameof(entity));
		Declaration = Argument.NotNull(entity.Declaration, nameof(entity.Declaration));
		Argument.NotNull(invoiceLineProviderFunction, nameof(invoiceLineProviderFunction));
		RandomLine = Argument.NotNull(invoiceLineProviderFunction.Invoke(entity), nameof(RandomLine));
	}

	public Dictionary<string, string> ReplaceApplicableMeursingExpressions(RateView rateView)
	{
		var rateFormula = rateView?.ZZ2_RateFormula ?? ZString.Empty;
		if (rateFormula.IsEmpty || !IsRateDuty(rateView))
		{
			return GetEmptyResult();
		}

		var matches = IEnumerableExtensions.DistinctBy(new Regex(MeursingDetectorPattern)
			.Matches(rateView.ZZ2_RateFormula)
			.Cast<Match>(), x => x.Value);

		if (!matches.Any())
		{
			return GetEmptyResult();
		}

		var meursingTariff = GetMeursingTariff();
		if (meursingTariff is null)
		{
			return matches.ToDictionary(m => GetMeursingCode(m), _ => DefaultReplacementValueWhenNoReplacementFormulaFound);
		}

		return matches.ToDictionary(m => GetMeursingCode(m), m => GetReplacedFormula(meursingTariff, m));
	}

	#region Implementation

	string GetReplacedFormula(TariffView meursingTariff, Match match)
	{
		var rateCodeToBeFound = match.Groups[1].Value;
		var additionalCodeToBeFound = match.Groups[2].Value;
		var criteria = new SpecificRateSelectionCriteria(RandomLine.EffectiveCountryOfOrigin
			, RandomLine.GetDefaultDataGroupingCode(Customs.Business.DefaultDataGroupingType.Tariff)
			, ZString.Empty
			, ZString.Empty
			, new HashSet<ZString> { additionalCodeToBeFound }
			, RandomLine.EffectiveDateForDutyRate
			, ZString.Empty
			, rateCodeToBeFound);

		return meursingTariff.GetApplicableRate(criteria)?.ZZ2_RateFormula ?? DefaultReplacementValueWhenNoReplacementFormulaFound;
	}

	protected virtual TariffView GetMeursingTariff() => null;

	string GetMeursingCode(Match match) => match.Value.Replace("#", "");

	bool IsRateDuty(RateView rateView) => rateView.ZZ2_ZZR_RateTypeCode == UniversalReferenceConstants.RefCusRateTypes.Dty;

	Dictionary<string, string> GetEmptyResult() => new Dictionary<string, string>();

	#endregion

	protected readonly TEntity Entity;
	protected readonly BaseJobComInvoiceLine RandomLine;
	protected readonly BaseJobDeclaration Declaration;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Pattern string")]
	const string MeursingDetectorPattern = @"#(\w+)\((\d+)\)#";
	const string DefaultReplacementValueWhenNoReplacementFormulaFound = "0";
}
