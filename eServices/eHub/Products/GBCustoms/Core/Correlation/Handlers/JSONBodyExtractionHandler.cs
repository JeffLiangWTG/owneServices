using Common.Logging;

using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CargoWise.eHub.Products.GBCustoms.Core.Correlation.Handlers
{
    public class JSONBodyExtractionHandler : IExtractionHandler
    {
        public string Extract(ILog logger, string content, string propertyName)
        {
			JObject jsonObject;
			try
			{
				jsonObject = JObject.Parse(JsonUnescape(content));
			}
			catch (Newtonsoft.Json.JsonReaderException)
			{
				try
				{
					jsonObject = JObject.Parse(content);
				}
				catch (Exception ex)
				{
					logger.Warn($"GBCustomsCorrelation - Could not parse JSON content: {content}{Environment.NewLine}Exception Message: {ex.Message}");
					return string.Empty;
				}
			}
			catch (Exception ex)
			{
				logger.Warn($"GBCustomsCorrelation - Could not parse JSON content: {content}{Environment.NewLine}{Environment.NewLine}Exception Message: {ex.Message}");
				return string.Empty;
			}

			return jsonObject.SelectToken(propertyName)?.ToString() ?? "";
		}

		public string Extract(ILog logger, string content, List<string> propertyNames)
		{
			foreach (var propertyName in propertyNames)
			{
				var value = Extract(logger, content, propertyName);
				if (!string.IsNullOrEmpty(value) )
				{
					return value;
				}
			}

			return string.Empty;
		}


			private string JsonUnescape(string jsonString)
		{
            jsonString = jsonString.Replace("\"{", "{");
            jsonString = jsonString.Replace("}\"", "}");
            return Regex.Unescape(jsonString);
        }
    }
}
