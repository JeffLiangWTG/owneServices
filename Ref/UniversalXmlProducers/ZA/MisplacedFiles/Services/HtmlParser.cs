using System;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using NUnit.Framework;

namespace ZAReferenceData.Services
{
	public class HtmlParser : IHtmlParser
	{
		public string GetUpdateDate(string url, string nodeName)
		{
			using (var webClient = new WebClient())
			{
				ServicesProvider.Logger.Info($"Downloading html string from: {url}");
				var html = webClient.DownloadString(url);
				var doc = new HtmlDocument();
				doc.LoadHtml(html);
				ServicesProvider.Logger.Info($"Finding {nodeName} in html doc.");
				foreach (HtmlNode htmlNode in doc.DocumentNode.SelectNodes("//br"))
				{
					if (htmlNode.NextSibling == null || htmlNode.NextSibling.NextSibling == null)
					{
						continue;
					}
					var fileName = htmlNode.NextSibling.NextSibling.InnerText.Trim();
					if (fileName == nodeName)
					{
						var infoString = htmlNode.NextSibling.InnerText.Trim();
						var updateDate = string.Join(" ", infoString.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Take(3)).Trim();
						ServicesProvider.Logger.Info($"{nodeName} found with update date: {updateDate}");
						return updateDate;
					}
				}
				return string.Empty;
			}
		}
	}
}

namespace ZAReferenceData.Services.Test
{
	[TestFixture]
	public class HtmlParserTest
	{
		static readonly HtmlParser Parser = new HtmlParser();

		[Test]
		public void GetUpdateDateShouldReturnCurrentDateTimeIfNodeNameExist()
		{
			var url = Utilities.CurrentFolder() + "\\TestResources\\VesselAgentCodeList.html";
			var result = Parser.GetUpdateDate(url, "VesselAgents.csv");
			Assert.IsNotEmpty(result);
			Assert.True(result == "11/14/2018 11:52 AM");

			result = Parser.GetUpdateDate(url, "CargoCarrierAir.csv");
			Assert.IsNotEmpty(result);
			Assert.True(result == "9/10/2019 2:52 PM");

			result = Parser.GetUpdateDate(url, "CargoCarrierSea.csv");
			Assert.IsNotEmpty(result);
			Assert.True(result == "9/11/2019 1:53 PM");
		}

		[Test]
		public void GetUpdateDateShouldReturnEmptyIfNodeNameDoesNotExist()
		{
			var url = Utilities.CurrentFolder() + "\\TestResources\\VesselAgentCodeList.html";
			var result = Parser.GetUpdateDate(url, "VesselAgentss.csv");
			Assert.IsEmpty(result);
		}
	}
}
