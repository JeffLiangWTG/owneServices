using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.Core.Orchestrations.HttpRetry.Contract
{
	public static class HttpHelper
	{
		public static string ConvertToHttpAdapterHeader(params HttpHeader[] headers)
		{
			if (!headers.Any())
			{
				return string.Empty;
			}

			var headerGroupings = headers
				.Where(header => header != null)
				.Where(header => !string.IsNullOrEmpty(header.Key))
				.Where(header => !string.IsNullOrEmpty(header.Value))
				.GroupBy(header => header.Key, header => header.Value)
				.ToArray();

			if (headerGroupings.Any(grouping => grouping.Count() > 1))
			{
				throw new ArgumentException("Duplicated header keys are not allowed!", "headers");
			}

			var headerBuilder = new StringBuilder();
			
			foreach (var headerGrouping in headerGroupings)
			{
				headerBuilder
					.AppendFormat(CultureInfo.InvariantCulture, "{0}: {1}", headerGrouping.Key, headerGrouping.Single())
					.AppendLine();
			}

			return headerBuilder
				.ToString()
				.Trim();
		}

		public static HttpHeader[] ConvertFromHttpAdapterHeader(string adapterHeader)
		{
			if (string.IsNullOrEmpty(adapterHeader))
			{
				return new HttpHeader[0];
			}

			return adapterHeader
				.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
				.Select(token => token.Split(':'))
				.Where(kvp => kvp.Length == 2)
				.Select(kvp => HttpHeader.Create(kvp[0].Trim(), kvp[1].Trim()))
				.ToArray();
		}

		public static OverridingStatusCodeConfig FindOverridingConfig(OverridingStatusCodeConfig[] overridingConfigs, HttpError httpError)
		{
			return FindOverridingConfig(
				overridingConfigs,
				config => 
					httpError.StatusCode == config.StatusCode,
				config => 
					!string.IsNullOrEmpty(config.Message) && 
					!string.IsNullOrEmpty(httpError.Message) && 
					httpError.Message.Contains(config.Message),
				config => 
					string.IsNullOrEmpty(config.Message));
		}

		public static OverridingExceptionConfig FindOverridingConfig(OverridingExceptionConfig[] overridingConfigs, Exception exception)
		{
			var exceptionType = exception
				.GetType()
				.FullName;

			return FindOverridingConfig(
				overridingConfigs,
				config => 
					exceptionType == config.ExceptionType,
				config =>
					!string.IsNullOrEmpty(config.ExceptionMessage) &&
					!string.IsNullOrEmpty(exception.Message) && 
					exception.Message.Contains(config.ExceptionMessage),
				config => 
					string.IsNullOrEmpty(config.ExceptionMessage));
		}

		private static TConfig FindOverridingConfig<TConfig>(
			TConfig[] configs,
			Func<TConfig, bool> filterByPrimaryField,
			Func<TConfig, bool> filterBySecondaryField,
			Func<TConfig, bool> filterByEmptySecondaryField)
			where TConfig : class
		{
			if (configs == null || !configs.Any())
			{
				return null;
			}
			
			var matchedConfigs = configs
				.Where(filterByPrimaryField)
				.ToArray();

			if (!matchedConfigs.Any())
			{
				return null;
			}

			var matchedSpecificConfigs = matchedConfigs
				.Where(filterBySecondaryField)
				.ToArray();

			var matchedConfig = default(TConfig);

			if (!matchedSpecificConfigs.Any())
			{
				var matchedGenericConfigs = matchedConfigs
					.Where(filterByEmptySecondaryField)
					.ToArray();

				if (matchedGenericConfigs.Length > 1)
				{
					throw new ArgumentException("Matched multiple generic overriding configurations!");
				}

				matchedConfig = matchedGenericConfigs.Any()
					? matchedGenericConfigs.Single()
					: null;
			}
			else
			{
				if (matchedSpecificConfigs.Length > 1)
				{
					throw new ArgumentException("Matched multiple specific overriding configurations!");
				}

				matchedConfig = matchedSpecificConfigs.Single();
			}

			return matchedConfig;
		}
	}
}
