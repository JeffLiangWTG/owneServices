using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.ESReferenceData.Services
{
	public abstract class RequestCodeProvider
	{
		readonly static IFormatProvider invariantCulture = System.Globalization.CultureInfo.InvariantCulture;

		protected string RequestExportableDocument<T>(string baseURL, string date, T type) where T : Enum
		{
			var queryURL = $"{baseURL}?VEZ=BUSCAR&FECCON={date}&IDETAB=";
			var webRequestWrapper = GetWebRequestWrapper();

			var htmlContent = webRequestWrapper.GetContent(queryURL + Enum.GetName(typeof(T), type));
			var postArgs = GetPostArgsFromHtml(htmlContent, Constants.AEAT.EmptyKeysForExportableDocs, $" Type: {type}.");

			postArgs["VEZ"] = "EXPOREXCEL";

			return webRequestWrapper.GetContentFromPost(baseURL, GetFormattedPostData(postArgs), false);
		}

		protected abstract Encoding Encoding { get; }

		protected static string GetFormattedPostData(Dictionary<string, string> postArgs)
		{
			var postData = string.Empty;
			foreach (string key in postArgs.Keys)
			{
				postData += $"{HttpUtility.UrlEncode(key)}={HttpUtility.UrlEncode(postArgs[key])}&";
			}
			return postData;
		}

		protected Dictionary<string, string> GetPostArgsFromHtml(string htmlContent, IEnumerable<string> emptyKeys, string invalidNodesCustomMessage = "")
		{
			var doc = new HtmlDocument();
			doc.LoadHtml(htmlContent);
			var postArgs = new Dictionary<string, string>();
			try
			{
				foreach (var argument in doc.DocumentNode.SelectNodes($"//form[@id='Query']//li[@class='oculto']//input"))
				{
					var key = argument.GetAttributeValue("id", string.Empty);
					var value = argument.GetAttributeValue("value", string.Empty);

					if (IsArgumentsInvalid(key, value, emptyKeys))
					{
						throw GetNewExceptionToThrow($"Empty key or value. Or key can't have a value for {GetEmptyKeysMessage(emptyKeys)}.");
					}
					postArgs.Add(key, value);
				}
			}
			catch (NullReferenceException)
			{
				throw GetNewExceptionToThrow($"Cannot process the requested page. Invalid nodes.{invalidNodesCustomMessage}");
			}

			return postArgs;
		}

		protected abstract Exception GetNewExceptionToThrow(string exceptionMessage);

		protected virtual IWebRequestWrapper GetWebRequestWrapper() => new WebRequestWrapper(Encoding);

		protected double CalulateNumberOfPages(Dictionary<string, string> postArgs)
		{
			if (!postArgs.ContainsKey(NumberOfResultsKey) || !double.TryParse(postArgs[NumberOfResultsKey], result: out var numberOfResults))
			{
				throw GetNewExceptionToThrow("Cannot continue due to empty or invalid 'NUM_RESULTADOS_RS'");
			}
			return Math.Ceiling(numberOfResults / ResultsPerPage);
		}

		protected static void PreparePostArgsForNextPage(Dictionary<string, string> postArgs, int currentResults)
		{
			postArgs["CLASGTE"] = currentResults + postArgs["CLASGTE"].Substring(postArgs["CLASGTE"].IndexOf('/'), postArgs["CLASGTE"].Length - postArgs["CLASGTE"].IndexOf('/'));
			postArgs["TOTALES"] = postArgs["TOTALES"].Replace((currentResults - ResultsPerPage).ToString(invariantCulture), currentResults.ToString(invariantCulture));
		}

		#region Implementation

		static string GetEmptyKeysMessage(IEnumerable<string> emptyKeys) => string.Join(", ", emptyKeys.Select(x => $"'{x}'"));

		static bool IsArgumentsInvalid(string key, string value, IEnumerable<string> emptyKeys) => string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value) && !emptyKeys.Contains(key);

		#endregion

		public const int ResultsPerPage = 20;
		const string NumberOfResultsKey = "NUM_RESULTADOS_RS";
	}
}
