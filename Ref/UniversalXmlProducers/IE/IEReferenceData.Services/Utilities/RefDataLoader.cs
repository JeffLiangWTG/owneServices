using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.IEReferenceData.Services
{
	public static class RefDataLoader
	{
		public enum RefDataComparisonOperator
		{
			Equal,
			In,
			LessThan,
			GreaterThan,
		}

		public static class RefDataActionNames
		{
			public const string RefCusTariffUpdate = nameof(RefCusTariffUpdate);
			public const string RefCusCodeListUpdate = nameof(RefCusCodeListUpdate);
			public const string RefCusCodeTypeUpdate = nameof(RefCusCodeTypeUpdate);
		}

		public static async Task<List<TRefData>> GetRefDataAsync<TRefData>(
			string actionName,
			params (string FieldName, RefDataComparisonOperator ComparisonOperator, object ComparisonValue)[] filters
		) where TRefData : RefDataRepoModelEntityType
			=> await GetRefDataAsync<TRefData>($"{ApplicationConfig.Instance.RefDbServiceURI}{GetFiltersString(actionName, filters)}");

		public static async Task<List<TRefData>> GetRefDataAsync<TRefData>(string uri) where TRefData : RefDataRepoModelEntityType
		{
			var requestUri = new Uri(uri);
			string resultJson = null;

			RefdbRepoUpdateWrapper<TRefData> resultWrapper = null;
			try
			{
				using (var loader = TextLoader.New(requestUri))
				{
					resultJson = await loader.LoadAsync();
				}
				if (resultJson is null)
				{
					var errorMessage = $"Unable to load ref data from URI: {requestUri}";
					Console.Error.WriteLine(errorMessage);
				}
				else
				{
					resultWrapper = JsonConvert.DeserializeObject<RefdbRepoUpdateWrapper<TRefData>>(resultJson);
				}
			}
			catch (InvalidOperationException ex)
			{
				Console.Error.WriteLine($"Unable to get valid data, URI: {uri}");
				Console.Error.WriteLine(ex.Message);
				Console.Error.WriteLine(ex.StackTrace);
			}
			catch (JsonSerializationException ex)
			{
				Console.Error.WriteLine(ex.Message);
				Console.Error.WriteLine(ex.StackTrace);
			}

			return resultWrapper?.Value.ToList() ?? new List<TRefData>();
		}

		static string GetFiltersString(string actionName, (string FieldName, RefDataComparisonOperator ComparisonOperator, object ComparisonValue)[] filters)
		{
			var filterBuilder = new StringBuilder(actionName);
			for (var filterIdx = 0; filterIdx < filters.Length; filterIdx++)
			{
				filterBuilder.Append(CultureInfo.InvariantCulture, $"{(filterIdx == 0 ? "?$filter=" : " and ")}{filters[filterIdx].FieldName} {GetFilterOperator(filters[filterIdx].ComparisonOperator)} {ToFilterString(filters[filterIdx].ComparisonValue)}");
			}
			return filterBuilder.ToString();

			string GetFilterOperator(RefDataComparisonOperator input)
			{
				switch (input)
				{
					case RefDataComparisonOperator.Equal:
					default:
						return "eq";
					case RefDataComparisonOperator.LessThan:
						return "le";
					case RefDataComparisonOperator.GreaterThan:
						return "ge";
					case RefDataComparisonOperator.In:
						return "in";
				}
			}

			string ToFilterString(object input)
			{
				if (input is DateTime dateTime)
				{
					return dateTime.ToString("yyyy-MM-ddThh:mm:ss", CultureInfo.InvariantCulture);
				}
				else if (input is Array array)
				{
					return $"({string.Join(',', array.Cast<object>().Select(item => ToFilterString(item)).ToArray())})";
				}
				else
				{
					return $"'{input}'";
				}
			}
		}

		class RefdbRepoUpdateWrapper<TRefData> where TRefData : RefDataRepoModelEntityType
		{
			public TRefData[] Value { get; set; }
		}
	}
}
