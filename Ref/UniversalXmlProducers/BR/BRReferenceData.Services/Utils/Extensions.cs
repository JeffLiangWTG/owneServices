using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public static class Extensions
	{

		public static Task<HttpResponseMessage> PostAsyncEx(this HttpClient httpClient, string requestUri, HttpContent content) => httpClient.PostAsync(new Uri(requestUri), content);

		public static Task<byte[]> GetByteArrayAsyncEx(this HttpClient httpClient, string requestUri) => httpClient.GetByteArrayAsync(new Uri(requestUri));

		public static Task<HttpResponseMessage> GetAsyncEx(this HttpClient httpClient, string requestUri) => httpClient.GetAsync(new Uri(requestUri));

		public static HttpResponseMessage GetSyncEx(this HttpClient httpClient, string requestUri) => httpClient.GetAsyncEx(requestUri)?.Result;

		public static HttpResponseMessage PostSyncEx(this HttpClient httpClient, HttpContent content, string requestUri) => httpClient.PostAsyncEx(requestUri, content)?.Result;

		public static string RemoveNonDecimalValues(this string text) => RegexForDecimalsOnly.Replace(text, "");

		public static string RemoveNonNumberValues(this string text) => RegexForNumbersOnly.Replace(text, "");

		static Regex RegexForDecimalsOnly => fRegexForDecimalsOnly ??= new Regex("[^0-9,.]");
		static Regex fRegexForDecimalsOnly;

		static Regex RegexForNumbersOnly => fRegexForNumbersOnly ??= new Regex("[^0-9]");
		static Regex fRegexForNumbersOnly;

		public static string GetElementValueAsString(this XElement element, string elementName, int maxLength)
		{
			var value = element?.Element(elementName)?.Value;
			if (maxLength == 0)
			{
				return value;
			}
			else if (value != null && value.Length > maxLength)
			{
				value = value.Substring(0, maxLength);
			}

			return value;
		}
	}
}
