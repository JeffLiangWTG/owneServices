using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public static class SACUTradeGroupDownloader
	{
		public static string[] Download(HttpClient client)
		{
			var response = client.GetAsync(new Uri(ConfigurationProvider.Configuration.GetSection("URL_SACU_AGREEMENTS").Value))?.Result;

			if (response != null && response.IsSuccessStatusCode)
			{
				var result = GetCountries(response);
				return result;
			}

			return null;
		}

		static string[] GetCountries(HttpResponseMessage responseMessage)
		{
			var page = new HtmlDocument();
			page.LoadHtml(responseMessage.Content.ReadAsStringAsync().Result);
			var countries = page.DocumentNode?.SelectNodes("//*[contains(@href,'/member-states/')]").Select(x => TextCleaner(x.Attributes["href"]?.Value)).Distinct().ToArray();
			return countries;
		}

		static string TextCleaner(string text)
		{
			var result = Regex.Replace(text, "/member-states/", "");
			result = Regex.Replace(result, "[^A-Za-z]", " ");
			result = string.Join(" ", result.Split(" ").Select(x => FirstLetterToUpperCase(x)));
			return result;
		}

		static string FirstLetterToUpperCase(string text) => string.Concat(text[0].ToString().ToUpper(CultureInfo.CurrentCulture), text.AsSpan(1));
	}
}
