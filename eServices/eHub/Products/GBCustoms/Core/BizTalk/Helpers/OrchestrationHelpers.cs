using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Common.Logging;
using Newtonsoft.Json.Linq;

namespace CargoWise.eHub.Products.GBCustoms.Core.BT.Helpers
{
	public static class OrchestrationHelpers
	{
		public static string ExtracAndRemoveHTMLcontentFromErrorResponse(ref string input)
		{
			if(!(input.IndexOf("HTML", StringComparison.OrdinalIgnoreCase) >= 0))
			{
				return string.Empty;
			}

			var extractedHTML = string.Empty;
			Regex regex = new Regex("<.*:*ResponseText>.*<.*:*Text>(.*<HTML>.*</HTML>).*</.*:*Text>.*</.*:*ResponseText>", RegexOptions.Singleline);
			var match = regex.Match(input);
			if (match.Success)
			{
				extractedHTML = match.Groups[1].Value;
				var htmlIndex = input.IndexOf(extractedHTML);
				input = input.Remove(htmlIndex, extractedHTML.Length);
			}

			return extractedHTML.Trim();
		}

		public static string EncodeResponseText(string responseCode)
		{
			return string.IsNullOrWhiteSpace(responseCode) ? string.Empty : Convert.ToBase64String(Encoding.UTF8.GetBytes(responseCode));
		}

		public static string GetInnerXmlFromXelement(XElement element)
		{
			var reader = element.CreateReader();
			reader.MoveToContent();
			return reader.ReadInnerXml();
		}

		public static string ExtractJsonValue(ILog logger, string content, string propertyName)
		{
			JObject jsonObject;
			try
			{
				jsonObject = JObject.Parse(Regex.Unescape(content));
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
				logger.Warn($"GBCustomsCorrelation - Could not parse JSON content: {content}{Environment.NewLine}Exception Message: {ex.Message}");
				return string.Empty;
			}

			return jsonObject.SelectToken(propertyName)?.ToString() ?? "";
		}
	}
}
