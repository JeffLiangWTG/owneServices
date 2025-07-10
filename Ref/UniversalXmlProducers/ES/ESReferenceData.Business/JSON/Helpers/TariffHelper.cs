using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.ESReferenceData.Services;
using Newtonsoft.Json.Linq;

namespace CargoWise.RefDbRepo.ESReferenceData.Business;

public static class TariffHelper
{
	public static Uri GetQueryUrlForTariffCode(string refDbServiceURI, string tariffCode, DateTime currentDate, bool isESTariff = false, bool checkExactTariff = false)
		=> new($"{refDbServiceURI}{RefDbRepoTariffURL}{PropertiesSelected}$filter={GetTariffFilter(tariffCode, checkExactTariff)}{GetDataGroupingFilter(isESTariff)} and ZZ1_EndDate ge {currentDate:s}Z");

	static string GetTariffFilter(string tariffCode, bool checkExactTariff)
		=> checkExactTariff ? $"ZZ1_TariffCode eq '{tariffCode}'" : $"startswith(ZZ1_TariffCode, '{tariffCode}')";

	static string GetDataGroupingFilter(bool isESTariff)
		=> isESTariff ? ESDataGroupingFilter : EUNDataGroupingFilter;

	public static string GetDataGroupingWhenESTariff(Func<string, string, bool, bool, Uri> getQueryUrlForTariffCodeMethod, string refDbServiceURI, string tariffCode, StringBuilder errorBuilder)
	{
		var tariffCodesJsonContent = DownloadJson.Download(getQueryUrlForTariffCodeMethod(refDbServiceURI, tariffCode, true, true).AbsoluteUri);
		var tariffCodesJObject = JsonHelper.GetJsonItem<RefCusTariffCodeSchema>(new StringReader(tariffCodesJsonContent), errorBuilder);
		var tariffCodesToExport = tariffCodesJObject.value;

		return tariffCodesToExport.Any() ? Constants.CountryCode : null;
	}

	const string RefDbRepoTariffURL = "RefCusTariffUpdate?";
	const string PropertiesSelected = "$select=ZZ1_TariffCode&";
	const string ESDataGroupingFilter = " and ZZ1_ZZZ_NKDataGrouping eq 'ES'";
	const string EUNDataGroupingFilter = " and ZZ1_ZZZ_NKDataGrouping eq 'EUN'";

	public static string RemoveLastPairOfZeros(string tariff)
	{
		while (tariff.Length > 0 && tariff.Substring(tariff.Length - 2).Equals("00", StringComparison.Ordinal))
		{
			tariff = tariff[..^2];
		}
		return tariff;
	}

	public static string[] GetTariffCodesArray(JObject jObject, StringBuilder errorBuilder)
	{
		var tariffCodeSchema = jObject.ToObject<RefCusTariffCodeSchema>();
		return GetTariffCodesArray(tariffCodeSchema, errorBuilder);
	}

	public static string[] GetTariffCodesArray(RefCusTariffCodeSchema tariffCodeSchema, StringBuilder errorBuilder)
	{
		var result = new List<string>();

		try
		{
			var filteredList = tariffCodeSchema.value.ToList().Where(x => x.ZZ1_TariffCode.Length == 10).ToList();
			foreach (var measureItem in filteredList)
			{
				var code = measureItem.ZZ1_TariffCode;
				if (!result.Contains(code))
				{
					result.Add(code);
				}
			}
		}
		catch (ArgumentException e)
		{
			errorBuilder.AppendLine(e.Message.Replace("\r\n", ""));
		}
		return [.. result];
	}
}
