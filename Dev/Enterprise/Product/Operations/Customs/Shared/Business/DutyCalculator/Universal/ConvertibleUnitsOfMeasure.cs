using System.Collections.Immutable;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DutyCalculator;

public static class ConvertibleUnitsOfMeasure
{
	public static readonly ImmutableDictionary<string, decimal> WeightConversionDictionary = new Dictionary<string, decimal>()
	{
		{ Core.Constants.Weight.Kilograms, 1 },
		{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram, 1m },
		{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Gram, 0.001m },
		{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Hectokilogram, 100m },
		{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Tonne, 1000m },
	}.ToImmutableDictionary();

	public static readonly ImmutableDictionary<string, decimal> VolumeConversionDictionary = new Dictionary<string, decimal>()
	{
		{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Volume.Litre, 1m },
		{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Volume.Hectolitre, 100m },
		{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Volume.Kilolitre, 1000m },
	}.ToImmutableDictionary();

	public static readonly ImmutableDictionary<string, decimal> AlcoholConversionDictionary = new Dictionary<string, decimal>()
	{
		{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Alcohol.LitrePureAlcohol, 1m },
		{ UniversalReferenceConstants.RefCusCodeList.CustomsUq.Alcohol.PercentageVolumeHectolitre, 1m } // Watch out, code reviewer!  1 LPA = 1 ASVX
	}.ToImmutableDictionary();
}
