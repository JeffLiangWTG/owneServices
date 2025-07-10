using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.CNReferenceData.Services
{
	public class CheckForUpdatesResponse : IResponse
	{
		public UpdateData HaveIsUpdate { get; set; }
		public string STATE { get; set; }
		public string STATE_INFO { get; set; }
	}

	public class UpdateData
	{
		public int IS_UPDATE_COUNT { get; set; }
		public string STATE { get; set; }
	}

	public class GetUpdatesResponse : IResponse
	{
		public ResultData RESULT_DATA_LIST { get; set; }
		public string STATE { get; set; }
		public string STATE_INFO { get; set; }

		public int PageNumber { get; set; }
		public int CountOfUpdates => RESULT_DATA_LIST?.HS_TAX?.Count ?? 0;
	}

	public class ResultData
	{
		public List<HSData> HS_TAX { get; set; }
	}

	public interface IResponse
	{
		string STATE { get; }
		string STATE_INFO { get; }
	}

	public interface IJsonData
	{
		DateTime ValidFrom { get; }
		DateTime ValidTo { get; }
		string Operator { get; }
		DateTime CreateTime { get; }
		string HS_CODE { get; }
	}

	public interface IRateData : IJsonData
	{
		string Rate { get; }
	}

	public static class Operator
	{
		public const string Add = "a";
		public const string Delete = "d";
		public const string None = "n";
	}

	public static class ResultDataHelper
	{
#pragma warning disable CA1502 // Avoid excessive complexity
		public static string GetRateByTradeGroup(this PTData data, string tradeGroup)
#pragma warning restore CA1502 // Avoid excessive complexity
		{
			switch (tradeGroup)
			{
				case "ASEAN":
					return data.ASAEN;
				case "APEC":
					return data.FTA_ASIA_PACIFIC_COUNTRIES_5;
				case "AU":
					return data.FTA_AUSTRALIA;
				case "CL":
					return data.FTA_CHILE;
				case "CR":
					return data.FTA_COSTARICA;
				case "GE":
					return data.FTA_GEORGIA;
				case "HN":
					return data.FTA_HONDURAS;
				case "HK":
					return data.FTA_HONGKONG;
				case "IS":
					return data.FTA_ICELAND;
				case "MO":
					return data.FTA_MACAU;
				case "MU":
					return data.FTA_MAURITIUS;
				case "MV":
					return data.FTA_MALDIVES;
				case "NZ":
					return data.FTA_NEWZEALAND;
				case "PK":
					return data.FTA_PAKISTAN;
				case "PE":
					return data.FTA_PERU;
				case "SG":
					return data.FTA_SINGAPORE;
				case "KR":
					return data.FTA_SOUTHKOREA;
				case "CH":
					return data.FTA_SWITZERLAND;
				case "TW":
					return data.FTA_TAIWAN;
				case "KH":
					return data.FTA_CAMBODIA;
				case "NI":
					return data.FTA_NICARAGUA;
				case "EC":
					return data.FTA_ECUADOR;
				case "RS":
					return data.FTA_SERBIA;
				case "LDCAPEC":
					return data.SP_ASIA_PACIFIC_COUNTRIES_2;
				case "LDCKH":
					return data.SP_CAMBODIA;
				case "LDCLA":
					return data.SP_LAOS;
				case "LDC":
					return data.SP_LDC;
				case "LDC1":
					return data.SP_LDC1; // Obsoleted
				case "LDC2":
					return data.SP_LDC2; // Obsoleted
				case "LDC3":
					return data.SP_LDC3; // Obsoleted
				case "LDCMM":
					return data.SP_MYANMAR;
				default:
					return string.Empty;
			}
		}

		public static DateTime GetStartDate(this IJsonData data)
		{
			return data.ValidFrom == DateTime.MinValue ? data.CreateTime : data.ValidFrom;
		}

		static DateTime MaxSmallDateTime => new DateTime(2079, 6, 6, 23, 59, 0);

		public static DateTime GetEndDate(this DateTime endDate) => endDate == DateTime.MinValue || endDate > MaxSmallDateTime ? MaxSmallDateTime : endDate.AddMinutes(-1);

		public static DateTime GetEndDate(this IJsonData data)
		{
			var endDate = data.IsDeleted() && data.ValidTo == DateTime.MinValue ? data.CreateTime : data.ValidTo;
			return endDate.GetEndDate();
		}

		public static bool IsDeleted(this IJsonData data) => data.Operator == Operator.Delete;

		public static bool IsAdded(this IJsonData data) => data.Operator == Operator.Add;

		public static IRateData GetEffectiveData<T1, T2>(T1 data1, T2 data2) where T1 : IRateData where T2 : IRateData
		{
			IRateData effectiveData = null;

			if (data1 != null)
			{
				if (!data1.IsDeleted() && !string.IsNullOrEmpty(data1.Rate))
				{
					effectiveData = data1;
				}
			}
			if (effectiveData == null && data2 != null)
			{
				effectiveData = data2;
			}

			return effectiveData;
		}

		public static string GetDebugInfo(this IJsonData data)
		{
			var result = $"{data.GetType().Name} for {data.HS_CODE}, Created at {data.CreateTime:yyyy-MM-dd}, From {data.ValidFrom:yyyy-MM-dd}";
			if (data.ValidTo > DateTime.MinValue)
			{
				result += $" to {data.ValidTo:yyyy-MM-dd}";
			}

			return result;
		}

		public static T GetEffectiveData<T>(this IEnumerable<T> dataList, ILog log, bool returnLastIfAllDeleted = true) where T : IJsonData
		{
			T effectiveData = default;

			if (dataList != null)
			{
				dataList = dataList.Where(x => x != null);
				if (dataList.Any())
				{
					var effectiveDataList = dataList.Where(x => !x.IsDeleted());

					effectiveData = effectiveDataList.OrderBy(x => x.ValidFrom == DateTime.MinValue ? x.CreateTime : x.ValidFrom).LastOrDefault();
					if (effectiveDataList.Count() > 1)
					{
						log.Warning($"Multiply effective data, picking the last one: {effectiveData.GetDebugInfo()}");

						foreach (var data in effectiveDataList.Where(x => x.GetHashCode() != effectiveData.GetHashCode()))
						{
							log.Warning($"                         Other effective data: {data.GetDebugInfo()}");
						}
					}

					if (effectiveData == null && returnLastIfAllDeleted)
					{
						return dataList.OrderBy(x => x.ValidTo == DateTime.MinValue ? x.CreateTime : x.ValidTo).LastOrDefault();
					}
				}
			}
			return effectiveData;
		}

		public static IEnumerable<T> GetModifiedDataList<T>(this IEnumerable<T> dataList) where T : IJsonData
		{
			return dataList.Where(x => x != null && x.Operator != Operator.None).OrderBy(x => x.ValidFrom);
		}

		public static bool IsSuccess(this IResponse response) => response.STATE == "1";

		public static string StateInfo(this IResponse response) => $"{response.STATE} - {response.STATE_INFO}";
	}
}
