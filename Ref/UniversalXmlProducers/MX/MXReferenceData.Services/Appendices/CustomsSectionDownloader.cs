using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.IdentityModel.Tokens;

namespace CargoWise.RefDbRepo.MXReferenceData.Services
{
	public static class CustomsSectionDownloader
	{
		const string InnerHtml = "apendice1";

		static string RemoveNonNumbers(string text) => Regex.Replace(text, "[^0-9.]", "");

		public static List<CustomsSectionDTO> Download(HttpClient client)
		{
			using (var getAsync = client.GetAsync(new Uri(ConfigurationProvider.CustomsFacilities.DownloadPath)))
			{
				var httpResponseMessage = getAsync?.Result;
				Contract.Assume(httpResponseMessage != null);

				var customsSectionTable = GetTableValue(httpResponseMessage.Content.ReadAsStringAsync()?.Result, InnerHtml);
				Contract.Assume(customsSectionTable != null);

				var page = new HtmlDocument();
				page.LoadHtml(customsSectionTable);
				var trNodes = page.DocumentNode?.SelectNodes("//td");
				Contract.Assume(trNodes != null);

				List<CustomsSectionDTO> list = new List<CustomsSectionDTO>();
				
				for (int i = 4; i < trNodes.Count; i++)
				{
					var node = new CustomsSectionDTO();

					node.CustomCode = RemoveNonNumbers(trNodes[i++].InnerText);
					node.SectionCode = RemoveNonNumbers(trNodes[i++].InnerText);
					node.SectionDescription = trNodes[i].InnerText.Trim();

					if (!node.SectionCode.IsNullOrEmpty())
					{
						list.Add(node);
					}

				}
				return list;
			}
		}

		static string GetTableValue(string html, string innerHtml)
		{
			var page = new HtmlDocument();
			page.LoadHtml(html);
			var hrefNode = page.DocumentNode?.SelectNodes("//a[@name='" + innerHtml + "']")?.FirstOrDefault();
			Contract.Assume(hrefNode != null);

			var hrefValue = hrefNode.ParentNode.ParentNode.ParentNode.ParentNode.ParentNode.InnerHtml;

			return hrefValue;
		}
	}
}
