using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DutyCalculator.Testing;

public sealed class RateCalcDataForTesting : IUniversalRateCalcData
{
	public DateTime DateOfValuation => new DateTime(2020, 01, 01);

	public decimal ValueForDuty { get; set; } = 5000.49m;

	public decimal CustomsValue => 6000.51m;

	public IDictionary<string, decimal> UnitOfMeasureValueList { get; private set; } = new Dictionary<string, decimal>()
	{
		{ "KGM", 55.123m },
		{ "DTN", 13.849m },
	};

	public IDictionary<string, decimal> CountrySpecificValueList { get; } = new Dictionary<string, decimal>()
	{
		{ "PI", 3.14m },
		{ "SR2", 1.41m },
	};

	public IList<Tuple<string, string>> AdditionalInformationList { get; private set; } = new List<Tuple<string, string>>()
	{
		new Tuple<string, string>("VFD", "AA"),
	};

	public IDictionary<string, string> MeursingExpressionList { get; } = new Dictionary<string, string>()
	{
		{ "EA(1)", "VFD * 0.04" },
		{ "ADFM(1)",  "10 * [DTN]" },
	};

	public override string ToString()
	{
		var uomValues = string.Join(", ", UnitOfMeasureValueList.Select(x => $"{x.Key}={x.Value}"));
		var countrySpecificValues = string.Join(", ", CountrySpecificValueList.Select(x => $"{x.Key}={x.Value}"));
		var additionalInfo = string.Join(", ", AdditionalInformationList.Select(x => $"({x.Item1},{x.Item2})"));
		return string.Format($"VFD={ValueForDuty}|CustomsValue={CustomsValue}|UOMValues={uomValues}|CountrySpecific={countrySpecificValues}|AdditionalInfo={additionalInfo}");
	}
}
