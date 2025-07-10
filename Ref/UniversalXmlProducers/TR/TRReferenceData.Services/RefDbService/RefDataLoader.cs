using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.TRReferenceData.Services
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
			public const string RefCusTradeGroupUpdate = nameof(RefCusTradeGroupUpdate);
			public const string RefCusTradeGroupCountryUpdate = nameof(RefCusTradeGroupCountryUpdate);
		}

		public static async Task<List<TEntity>> GetRefDataAsync<TEntity>(
			string actionName,
			string[] expands,
			params (string FieldName, RefDataComparisonOperator ComparisonOperator, object ComparisonValue)[] filters
		) where TEntity : RefDataRepoModelEntityType
		{
			var action = $"{ApplicationConfig.RefDbServiceURI.TrimEnd('/')}/{actionName}";
			var expandsQuery = GetExpands(expands);
			var filtersQuery = GetFiltersString(filters);

			var urlBuilder = new StringBuilder(action);
			if (!(string.IsNullOrWhiteSpace(expandsQuery) && string.IsNullOrWhiteSpace(filtersQuery)))
			{
				urlBuilder.Append('?');
				if (!string.IsNullOrWhiteSpace(expandsQuery))
				{
					urlBuilder.Append(expandsQuery);
				}
				if (!string.IsNullOrWhiteSpace(filtersQuery))
				{
					urlBuilder.Append('&');
					urlBuilder.Append(filtersQuery);
				}
			}

			return await GetRefDataAsync<TEntity>(urlBuilder.ToString());
		}

		public static async Task<List<TEntity>> GetRefDataAsync<TEntity>(
			string actionName,
			params (string FieldName, RefDataComparisonOperator ComparisonOperator, object ComparisonValue)[] filters
		) where TEntity : RefDataRepoModelEntityType
			=> await GetRefDataAsync<TEntity>(actionName, null, filters);

		static async Task<List<TRefData>> GetRefDataAsync<TRefData>(string uri) where TRefData : RefDataRepoModelEntityType
		{
			if (Uri.TryCreate(uri, new UriCreationOptions(), out var requestUri))
			{
				string resultJson = null;

				RefdbRepoUpdateWrapper<TRefData> resultWrapper = null;
				try
				{
					using (var loader = TextLoader.New(requestUri))
					{
						resultJson = await loader.LoadAsync();
					}

					resultWrapper = JsonConvert.DeserializeObject<RefdbRepoUpdateWrapper<TRefData>>(resultJson);
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
			else
			{
				return new List<TRefData>();
			}
		}

		public static string GetExpands(string[] expands)
		{
			var result = "";

			if (expands != null)
			{
				for (var idx = expands.Length - 1; idx >= 0; idx--)
				{
					if (idx == expands.Length - 1)
					{
						result = $"$expand={expands[idx]}";
					}
					else
					{
						result = $"$expand={expands[idx]}({result})";
					}
				}
			}
			return result;
		}

		public static string GetFiltersString((string FieldName, RefDataComparisonOperator ComparisonOperator, object ComparisonValue)[] filters)
		{
			if (filters == null || filters.Length == 0)
			{
				return "";
			}

			var filterBuilder = new StringBuilder();
			for (var filterIdx = 0; filterIdx < filters.Length; filterIdx++)
			{
				filterBuilder.Append(CultureInfo.InvariantCulture, $"{(filterIdx == 0 ? "$filter=" : " and ")}{filters[filterIdx].FieldName} {GetFilterOperator(filters[filterIdx].ComparisonOperator)} {ToFilterString(filters[filterIdx].ComparisonValue)}");
			}
			return filterBuilder.ToString();
		}

		public static string GetFilterOperator(RefDataComparisonOperator input)
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

		public static string ToFilterString(object input)
		{
			switch (input)
			{
				case DateTime dateTime:
					return dateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
				case DateTimeOffset dateTimeOffset:
					return dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
				case Array array:
					return $"({string.Join(',', array.Cast<object>().Select(ToFilterString))})";
				default:
					return $"'{input}'";
			}
		}

		public class RefdbRepoUpdateWrapper<TRefData> where TRefData : RefDataRepoModelEntityType
		{
			public TRefData[] Value { get; set; }
		}
	}
}
